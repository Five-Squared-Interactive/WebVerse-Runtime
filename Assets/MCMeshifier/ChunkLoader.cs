using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using GLTFast.Export;

public class ChunkLoader : MonoBehaviour
{
    [Header("Input Source (use one or the other)")]
    [SerializeField] private string jsonFilePath;
    [SerializeField] private string jsonDirectoryPath;
    [SerializeField] private bool deleteSourceFilesAfterProcessing = false;
    
    [Header("Settings")]
    [SerializeField] private Material atlasMaterial;
    [SerializeField] private TextAsset atlasDataFile;
    [SerializeField] private float blockSize = 1f;
    [SerializeField] private float delayBetweenChunks = 1f;
    [SerializeField] private KeyCode continueKey = KeyCode.Space;
    [SerializeField] private bool deleteChunksAfterGeneration = true;
    
    [Header("GLTF Export")]
    [SerializeField] private bool exportToGltf = false;
    [SerializeField] private string gltfExportDirectory;
    [SerializeField] private bool exportAsGlb = true;

    private GameObject currentChunkObject;
    private bool isPaused = false;
    private bool isFirstChunk = true;

    // Atlas data
    private Dictionary<string, BlockTextureMapping> blockMappings;
    private Dictionary<string, Rect> textureUVs;
    private BlockTextureMapping missingTextureMapping;
    private HashSet<string> warnedBlocks = new HashSet<string>(); // Avoid spam

    void Start()
    {
        LoadAtlasData();
        
        // Directory mode takes priority if specified
        if (!string.IsNullOrEmpty(jsonDirectoryPath))
        {
            StartCoroutine(LoadAndProcessDirectoryCoroutine(jsonDirectoryPath));
        }
        else if (!string.IsNullOrEmpty(jsonFilePath))
        {
            StartCoroutine(LoadAndProcessChunksCoroutine(jsonFilePath));
        }
    }

    private IEnumerator LoadAndProcessDirectoryCoroutine(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError($"Directory not found: {directoryPath}");
            yield break;
        }

        string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");
        
        if (jsonFiles.Length == 0)
        {
            Debug.LogWarning($"No JSON files found in directory: {directoryPath}");
            yield break;
        }

        Debug.Log($"Found {jsonFiles.Length} JSON files in directory.");
        Debug.Log("===================================");

        isFirstChunk = true;

        for (int i = 0; i < jsonFiles.Length; i++)
        {
            string filePath = jsonFiles[i];
            
            Chunk chunk = null;
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                chunk = JsonUtility.FromJson<Chunk>(jsonContent);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing JSON file {filePath}: {ex.Message}");
                continue;
            }

            if (chunk == null || chunk.blocks == null)
            {
                Debug.LogWarning($"Invalid chunk data in file: {filePath}");
                continue;
            }

            int blockCount = chunk.blocks?.Count ?? 0;
            int entityCount = chunk.entities?.Count ?? 0;
            int blockEntityCount = chunk.block_entities?.Count ?? 0;

            Debug.Log($"File {i + 1}/{jsonFiles.Length}: {Path.GetFileName(filePath)}");
            Debug.Log($"Chunk ({chunk.chunk_x}, {chunk.chunk_z}) - Dimension: {chunk.dimension}");
            Debug.Log($"  Blocks: {blockCount}, Entities: {entityCount}, Block Entities: {blockEntityCount}");

            // Destroy previous chunk if it exists and deletion is enabled
            if (deleteChunksAfterGeneration && currentChunkObject != null)
            {
                Destroy(currentChunkObject);
            }

            // Generate the chunk mesh
            GenerateChunkMesh(chunk);

            // Export to GLTF if enabled
            if (exportToGltf && currentChunkObject != null)
            {
                yield return StartCoroutine(ExportChunkToGltfCoroutine(chunk, currentChunkObject));
            }

            // Pause after first chunk and wait for user input
            if (isFirstChunk)
            {
                isFirstChunk = false;
                isPaused = true;
                Debug.Log($"Paused after first chunk. Press '{continueKey}' to continue...");
                
                while (isPaused)
                {
                    yield return null;
                }
            }
            else
            {
                // Wait before processing the next chunk
                yield return new WaitForSeconds(delayBetweenChunks);
            }

            // Delete source file if enabled
            if (deleteSourceFilesAfterProcessing)
            {
                try
                {
                    File.Delete(filePath);
                    Debug.Log($"Deleted source file: {Path.GetFileName(filePath)}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to delete source file {filePath}: {ex.Message}");
                }
            }
        }

        // Clean up the last chunk if deletion is enabled
        if (deleteChunksAfterGeneration && currentChunkObject != null)
        {
            Destroy(currentChunkObject);
            currentChunkObject = null;
        }

        Debug.Log("Finished processing all files in directory.");
    }

    private void LoadAtlasData()
    {
        blockMappings = new Dictionary<string, BlockTextureMapping>();

        // Create missing texture UV (pink/black checkerboard - first slot in atlas or fallback)
        missingTextureMapping = new BlockTextureMapping
        {
            top = new Rect(0, 0, 0.00390625f, 0.00390625f), // 16/4096
            bottom = new Rect(0, 0, 0.00390625f, 0.00390625f),
            north = new Rect(0, 0, 0.00390625f, 0.00390625f),
            south = new Rect(0, 0, 0.00390625f, 0.00390625f),
            east = new Rect(0, 0, 0.00390625f, 0.00390625f),
            west = new Rect(0, 0, 0.00390625f, 0.00390625f)
        };

        if (atlasDataFile == null)
        {
            Debug.LogWarning("No atlas data file assigned. Using default UVs.");
            return;
        }

        try
        {
            BlockAtlasData atlasData = JsonUtility.FromJson<BlockAtlasData>(atlasDataFile.text);
            
            // First load texture UVs for fallback lookups
            Dictionary<string, Rect> textureUVs = new Dictionary<string, Rect>();
            if (atlasData?.textureUVs != null)
            {
                foreach (var entry in atlasData.textureUVs)
                {
                    textureUVs[entry.name] = entry.rect;
                }
                Debug.Log($"Loaded {textureUVs.Count} texture UVs");
            }
            
            // Load explicit block mappings
            if (atlasData?.blockMappings != null)
            {
                foreach (var entry in atlasData.blockMappings)
                {
                    blockMappings[entry.blockName] = entry.mapping;
                }
                Debug.Log($"Loaded {blockMappings.Count} block texture mappings");
            }
            
            // Store textureUVs for fallback generation
            this.textureUVs = textureUVs;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load atlas data: {ex.Message}");
        }
    }

    public void LoadAndProcessChunks(string filePath)
    {
        StartCoroutine(LoadAndProcessChunksCoroutine(filePath));
    }

    private IEnumerator LoadAndProcessChunksCoroutine(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON file not found: {filePath}");
            yield break;
        }

        WorldData worldData = null;
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            worldData = JsonUtility.FromJson<WorldData>(jsonContent);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing JSON: {ex.Message}");
            yield break;
        }

        if (worldData?.region?.chunks == null)
        {
            Debug.LogError("Failed to parse world data or no chunks found.");
            yield break;
        }

        Debug.Log($"Level Name: {worldData.metadata?.level_name}");
        Debug.Log($"Game Version: {worldData.metadata?.game_version}");
        Debug.Log($"Dimension: {worldData.region.dimension}");
        Debug.Log($"Total Chunks: {worldData.region.chunks.Count}");
        Debug.Log("-----------------------------------");

        isFirstChunk = true;

        foreach (var chunk in worldData.region.chunks)
        {
            int blockCount = chunk.blocks?.Count ?? 0;
            int entityCount = chunk.entities?.Count ?? 0;
            int blockEntityCount = chunk.block_entities?.Count ?? 0;

            Debug.Log($"Chunk ({chunk.chunk_x}, {chunk.chunk_z}):");
            Debug.Log($"  Blocks: {blockCount}");
            Debug.Log($"  Entities: {entityCount}");
            Debug.Log($"  Block Entities: {blockEntityCount}");

            // Destroy previous chunk if it exists and deletion is enabled
            if (deleteChunksAfterGeneration && currentChunkObject != null)
            {
                Destroy(currentChunkObject);
            }

            // Generate the new chunk
            GenerateChunkMesh(chunk);

            // Export to GLTF if enabled
            if (exportToGltf && currentChunkObject != null)
            {
                yield return StartCoroutine(ExportChunkToGltfCoroutine(chunk, currentChunkObject));
            }

            // Pause after first chunk and wait for user input
            if (isFirstChunk)
            {
                isFirstChunk = false;
                isPaused = true;
                Debug.Log($"Paused after first chunk. Press '{continueKey}' to continue...");
                
                while (isPaused)
                {
                    yield return null;
                }
            }
            else
            {
                // Wait before processing the next chunk
                yield return new WaitForSeconds(delayBetweenChunks);
            }
        }

        // Clean up the last chunk if deletion is enabled
        if (deleteChunksAfterGeneration && currentChunkObject != null)
        {
            Destroy(currentChunkObject);
            currentChunkObject = null;
        }

        Debug.Log("Finished processing all chunks.");
    }

    private IEnumerator ExportChunkToGltfCoroutine(Chunk chunk, GameObject chunkObject)
    {
        if (string.IsNullOrEmpty(gltfExportDirectory))
        {
            Debug.LogWarning("GLTF export directory not specified. Skipping export.");
            yield break;
        }

        // Validate the chunk object has renderable content
        var meshFilter = chunkObject.GetComponentInChildren<MeshFilter>();
        var meshRenderer = chunkObject.GetComponentInChildren<MeshRenderer>();
        if (meshFilter == null || meshFilter.sharedMesh == null || meshFilter.sharedMesh.vertexCount == 0)
        {
            Debug.LogWarning($"Chunk ({chunk.chunk_x}, {chunk.chunk_z}) has no mesh data — skipping GLTF export.");
            yield break;
        }
        if (meshRenderer == null || meshRenderer.sharedMaterial == null)
        {
            Debug.LogWarning($"Chunk ({chunk.chunk_x}, {chunk.chunk_z}) has no material — skipping GLTF export.");
            yield break;
        }

        Debug.Log($"Chunk mesh has {meshFilter.sharedMesh.vertexCount} vertices, " +
                  $"{meshFilter.sharedMesh.triangles.Length / 3} triangles, " +
                  $"material: {meshRenderer.sharedMaterial.name}");

        // Ensure export directory exists
        if (!Directory.Exists(gltfExportDirectory))
        {
            try
            {
                Directory.CreateDirectory(gltfExportDirectory);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create export directory: {ex.Message}");
                yield break;
            }
        }

        string extension = exportAsGlb ? "glb" : "gltf";
        string fileName = $"chunk_{chunk.chunk_x}_{chunk.chunk_z}.{extension}";
        string filePath = Path.Combine(gltfExportDirectory, fileName);

        Debug.Log($"Exporting chunk to: {filePath}");

        bool exportDone = false;
        bool exportSuccess = false;
        string exportError = null;

        // Run the async export and bridge back to the coroutine
        async void DoExport()
        {
            try
            {
                var exportSettings = new ExportSettings
                {
                    Format = exportAsGlb ? GltfFormat.Binary : GltfFormat.Json,
                    FileConflictResolution = FileConflictResolution.Overwrite
                };

                var gameObjectExportSettings = new GameObjectExportSettings
                {
                    OnlyActiveInHierarchy = true,
                    DisabledComponents = false
                };

                var export = new GameObjectExport(exportSettings, gameObjectExportSettings);
                export.AddScene(new[] { chunkObject }, chunkObject.name);

                exportSuccess = await export.SaveToFileAndDispose(filePath);
            }
            catch (Exception ex)
            {
                exportError = ex.Message;
                exportSuccess = false;
            }
            finally
            {
                exportDone = true;
            }
        }

        DoExport();

        // Wait for the async export to complete, with a timeout
        float timeout = 30f;
        float elapsed = 0f;
        while (!exportDone && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!exportDone)
        {
            Debug.LogError($"GLTF export timed out after {timeout}s for chunk ({chunk.chunk_x}, {chunk.chunk_z})");
        }
        else if (exportError != null)
        {
            Debug.LogError($"GLTF export failed with exception: {exportError}");
        }
        else if (exportSuccess)
        {
            // Verify the file was actually written
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                Debug.Log($"Successfully exported chunk to: {filePath} ({fileInfo.Length} bytes)");
            }
            else
            {
                Debug.LogError($"GLTF export reported success but file not found: {filePath}");
            }
        }
        else
        {
            Debug.LogError($"GLTF export returned false for chunk ({chunk.chunk_x}, {chunk.chunk_z})");
        }
    }

    private void GenerateChunkMesh(Chunk chunk)
    {
        currentChunkObject = new GameObject($"Chunk_{chunk.chunk_x}_{chunk.chunk_z}");
        currentChunkObject.transform.SetParent(transform);

        if (chunk.blocks == null) return;

        // Build a HashSet of occluding block positions for neighbor face culling.
        // Non-occluding blocks (glass, leaves, cross shapes, etc.) don't hide neighbor faces.
        HashSet<Vector3Int> occludingPositions = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, Block> blockLookup = new Dictionary<Vector3Int, Block>();
        foreach (var block in chunk.blocks)
        {
            if (IsAirBlock(block.block))
                continue;

            Vector3Int pos = new Vector3Int(block.x, block.y, block.z);
            blockLookup[pos] = block;

            if (IsOccludingBlock(block.block))
                occludingPositions.Add(pos);
        }

        // Collect all faces with their texture UVs
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        foreach (var block in chunk.blocks)
        {
            if (IsAirBlock(block.block))
                continue;

            BlockShape shape = GetBlockShape(block.block);
            if (shape == BlockShape.Skip)
                continue;

            Vector3Int blockPos = new Vector3Int(block.x, block.y, block.z);
            Vector3 worldPos = new Vector3(block.x * blockSize, block.y * blockSize, block.z * blockSize);

            BlockTextureMapping mapping = GetBlockTextureMapping(block.block);

            if (shape == BlockShape.Cross)
            {
                // Use the north face texture for the cross (arbitrary, but consistent)
                AddCross(worldPos, mapping.north, vertices, normals, uvs, triangles);
                continue;
            }

            if (shape == BlockShape.FlatHorizontal)
            {
                AddFlatHorizontal(worldPos, mapping.top, vertices, normals, uvs, triangles);
                continue;
            }

            // FullCube or NonOccluding — standard 6-face cube with neighbor culling
            // Top (Y+)
            if (!occludingPositions.Contains(blockPos + Vector3Int.up))
            {
                AddFace(worldPos, Vector3.up, Vector3.forward, Vector3.right, mapping.top,
                    vertices, normals, uvs, triangles);
            }
            // Bottom (Y-)
            if (!occludingPositions.Contains(blockPos + Vector3Int.down))
            {
                AddFace(worldPos, Vector3.down, Vector3.back, Vector3.right, mapping.bottom,
                    vertices, normals, uvs, triangles);
            }
            // North (Z+)
            if (!occludingPositions.Contains(blockPos + new Vector3Int(0, 0, 1)))
            {
                AddFace(worldPos, Vector3.forward, Vector3.up, Vector3.left, mapping.north,
                    vertices, normals, uvs, triangles);
            }
            // South (Z-)
            if (!occludingPositions.Contains(blockPos + new Vector3Int(0, 0, -1)))
            {
                AddFace(worldPos, Vector3.back, Vector3.up, Vector3.right, mapping.south,
                    vertices, normals, uvs, triangles);
            }
            // East (X+)
            if (!occludingPositions.Contains(blockPos + Vector3Int.right))
            {
                AddFace(worldPos, Vector3.right, Vector3.up, Vector3.forward, mapping.east,
                    vertices, normals, uvs, triangles);
            }
            // West (X-)
            if (!occludingPositions.Contains(blockPos + Vector3Int.left))
            {
                AddFace(worldPos, Vector3.left, Vector3.up, Vector3.back, mapping.west,
                    vertices, normals, uvs, triangles);
            }
        }

        if (vertices.Count == 0) return;

        // Create the mesh
        GameObject meshObject = new GameObject("ChunkMesh");
        meshObject.transform.SetParent(currentChunkObject.transform);
        meshObject.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = $"Chunk_{chunk.chunk_x}_{chunk.chunk_z}_Mesh";

        // Use 32-bit indices if we have more than 65535 vertices
        if (vertices.Count > 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();

        meshFilter.mesh = mesh;
        meshRenderer.material = atlasMaterial != null ? atlasMaterial : new Material(Shader.Find("Standard"));
    }

    /// <summary>
    /// Try to find a texture UV by trying multiple name variants.
    /// Returns true if found and sets uvOut.
    /// </summary>
    private bool TryFindTextureUV(string name, out Rect uvOut)
    {
        uvOut = default;
        if (textureUVs == null) return false;
        if (textureUVs.TryGetValue(name, out uvOut)) return true;
        return false;
    }

    /// <summary>
    /// Try many naming variants for a given base name against the textureUVs dictionary.
    /// Returns the UV rect if any variant matches.
    /// </summary>
    private bool TryFindTextureVariants(string baseName, out Rect uvOut)
    {
        uvOut = default;
        if (textureUVs == null) return false;

        // Direct match
        if (textureUVs.TryGetValue(baseName, out uvOut)) return true;

        // Common pack suffixes: _0, _1, _side, _s, _bush, _vertical, _ends, etc.
        string[] suffixes = { "_0", "_1", "_side", "_s", "_bush", "_vertical",
                              "_ends", "_insides_top", "_insides_bottom",
                              "_sides", "_front", "_emissive", "_item",
                              "_stage_1", "_slab", "_single" };
        foreach (string suffix in suffixes)
        {
            if (textureUVs.TryGetValue(baseName + suffix, out uvOut)) return true;
        }

        // Strip common suffixes and retry
        string[] strippable = { "_block", "_bricks", "_ore", "_planks" };
        foreach (string strip in strippable)
        {
            if (baseName.EndsWith(strip))
            {
                string stripped = baseName.Substring(0, baseName.Length - strip.Length);
                if (textureUVs.TryGetValue(stripped, out uvOut)) return true;
                foreach (string suffix in suffixes)
                {
                    if (textureUVs.TryGetValue(stripped + suffix, out uvOut)) return true;
                }
            }
        }

        // Try adding common prefixes
        string[] prefixes = { "oak_", "stone_", "mossy_", "polished_" };
        foreach (string prefix in prefixes)
        {
            if (textureUVs.TryGetValue(prefix + baseName, out uvOut)) return true;
        }

        return false;
    }

    private BlockTextureMapping GetBlockTextureMapping(string blockName)
    {
        // Strip prefix like "universal_minecraft:" or "minecraft:"
        string cleanName = blockName;
        if (cleanName.Contains(":"))
        {
            cleanName = cleanName.Substring(cleanName.LastIndexOf(':') + 1);
        }

        // Try to find exact match in blockMappings (cached results end up here too)
        if (blockMappings.TryGetValue(cleanName, out BlockTextureMapping mapping))
        {
            return mapping;
        }

        // Try variations in blockMappings
        List<string> variations = new List<string>
        {
            cleanName,
            cleanName.Replace("_block", ""),
            cleanName + "_block",
            "oak_" + cleanName,
            "stone_" + cleanName,
        };

        // Special case mappings for Amulet's universal_minecraft generic names
        if (cleanName == "plant") variations.AddRange(new[] { "short_grass", "double_plant_grass_bottom", "grass" });
        if (cleanName == "double_plant") variations.AddRange(new[] { "tall_grass_bottom", "large_fern_bottom", "short_grass" });
        if (cleanName == "water") variations.AddRange(new[] { "water_still", "water_flow" });
        if (cleanName == "lava") variations.AddRange(new[] { "lava_still", "lava_flow" });
        if (cleanName == "fire") variations.AddRange(new[] { "fire_0", "fire_1" });
        if (cleanName == "soul_fire") variations.AddRange(new[] { "soul_fire_0", "soul_fire_1" });
        if (cleanName == "log") variations.AddRange(new[] { "oak_log_horizontal", "oak_log_vertical", "oak_log" });
        if (cleanName == "planks") variations.AddRange(new[] { "oak_planks", "planks" });
        if (cleanName == "stained_glass") variations.Add("glass");
        if (cleanName == "stained_terracotta") variations.AddRange(new[] { "terracotta", "terracotta_side" });
        if (cleanName == "infested_block") variations.AddRange(new[] { "stone", "stone_1" });
        if (cleanName == "rail") variations.AddRange(new[] { "track_rails", "rail_corner" });
        if (cleanName == "torch") variations.AddRange(new[] { "torch_emissive" });
        if (cleanName == "snow") variations.Add("snow_block");
        if (cleanName == "chest") variations.Add("chest");
        if (cleanName == "cocoa") variations.Add("cocoa_stage0");
        if (cleanName == "beacon") variations.AddRange(new[] { "beacon_base_side", "beacon_core" });
        if (cleanName == "cave_vines") variations.Add("cave_vines_lit_emissive");
        if (cleanName == "cave_vines_plant") variations.Add("cave_vines_plant_lit_emissive");
        if (cleanName == "pointed_dripstone") variations.Add("dripstone_single");
        if (cleanName == "dripstone_block") variations.Add("dripstone_single");
        if (cleanName == "andesite") variations.Add("andesite_single");
        if (cleanName == "diorite") variations.Add("diorite_single");
        if (cleanName == "granite") variations.Add("granite_single");
        if (cleanName == "deepslate") variations.AddRange(new[] { "deepslate_diamond_ore_side" });
        if (cleanName == "lily_pad") variations.AddRange(new[] { "lily_pad_0", "lily_pad_1" });
        if (cleanName == "redstone_ore") variations.AddRange(new[] { "redstone_ore_0", "redstone_ore_off_0" });
        if (cleanName == "deepslate_redstone_ore") variations.AddRange(new[] { "deepslate_redstone_ore_0", "deepslate_redstone_ore_off_0" });
        if (cleanName == "vine") variations.AddRange(new[] { "vine_0", "vine_1" });
        if (cleanName == "moss_block") variations.Add("moss_block");

        foreach (string variant in variations)
        {
            if (blockMappings.TryGetValue(variant, out mapping))
            {
                blockMappings[cleanName] = mapping;
                return mapping;
            }
        }

        // Try to generate mapping from textureUVs with comprehensive fallbacks
        if (textureUVs != null)
        {
            BlockTextureMapping generated = TryGenerateMappingFromTextures(cleanName);
            if (generated != null)
            {
                blockMappings[cleanName] = generated;
                return generated;
            }
        }

        // Return missing texture (only warn once per block type)
        if (!warnedBlocks.Contains(cleanName))
        {
            warnedBlocks.Add(cleanName);
            Debug.LogWarning($"No texture found for block: {blockName} (cleaned: {cleanName})");
        }
        return missingTextureMapping;
    }

    private BlockTextureMapping TryGenerateMappingFromTextures(string blockName)
    {
        Rect baseUV = default;
        Rect topUV = default;
        Rect bottomUV = default;
        Rect sideUV = default;
        bool foundBase = false;
        bool foundTop = false;
        bool foundBottom = false;
        bool foundSide = false;

        // ── Special-case rewrites ──
        if (blockName == "plant") blockName = "short_grass";
        if (blockName == "grass") blockName = "short_grass";

        // ── 1. Find the base/all-faces texture ──
        if (TryFindTextureVariants(blockName, out baseUV))
        {
            foundBase = true;
        }

        // ── 2. Find top texture ──
        string[] topCandidates = {
            blockName + "_top", blockName + "_top_1", blockName + "_top_0",
            blockName.Replace("_block", "") + "_top",
            blockName + "_vertical",  // logs in BarelyDefault
            blockName + "_end", blockName + "_ends",
        };
        foreach (string c in topCandidates)
        {
            if (TryFindTextureUV(c, out topUV)) { foundTop = true; break; }
        }

        // ── 3. Find bottom texture ──
        string[] bottomCandidates = {
            blockName + "_bottom", blockName + "_bottom_0", blockName + "_bottom_1",
            blockName.Replace("_block", "") + "_bottom",
            blockName + "_end", blockName + "_ends",
        };
        foreach (string c in bottomCandidates)
        {
            if (TryFindTextureUV(c, out bottomUV)) { foundBottom = true; break; }
        }

        // ── 4. Find side texture ──
        string[] sideCandidates = {
            blockName + "_side", blockName + "_side_0", blockName + "_side_1",
            blockName + "_side_v",
            blockName.Replace("_block", "") + "_side",
            blockName + "_horizontal",  // logs in BarelyDefault
            blockName + "_front",
            blockName + "_sides",
        };
        foreach (string c in sideCandidates)
        {
            if (TryFindTextureUV(c, out sideUV)) { foundSide = true; break; }
        }

        // ── 5. Nothing found at all — try harder with fuzzy match ──
        if (!foundBase && !foundTop && !foundBottom && !foundSide)
        {
            // Try partial/prefix match: find any texture that starts with blockName
            foreach (var kvp in textureUVs)
            {
                if (kvp.Key.StartsWith(blockName + "_") || kvp.Key == blockName)
                {
                    baseUV = kvp.Value;
                    foundBase = true;
                    break;
                }
            }

            // Still nothing — try without common suffixes
            string reduced = blockName;
            string[] trims = { "_block", "_bricks", "_ore", "_stone", "_planks",
                               "_log", "_wood", "_stem", "_hyphae" };
            foreach (string trim in trims)
            {
                if (reduced.EndsWith(trim))
                {
                    reduced = reduced.Substring(0, reduced.Length - trim.Length);
                    break;
                }
            }
            if (reduced != blockName && !foundBase)
            {
                foreach (var kvp in textureUVs)
                {
                    if (kvp.Key.StartsWith(reduced + "_") || kvp.Key == reduced)
                    {
                        baseUV = kvp.Value;
                        foundBase = true;
                        break;
                    }
                }
            }
        }

        if (!foundBase && !foundTop && !foundBottom && !foundSide)
            return null;

        // ── Build the mapping with cascading fallbacks ──
        BlockTextureMapping mapping = new BlockTextureMapping();

        // Determine the best fallback order
        Rect allFallback = foundBase ? baseUV :
                           foundSide ? sideUV :
                           foundTop  ? topUV  : bottomUV;

        mapping.top    = foundTop    ? topUV    : allFallback;
        mapping.bottom = foundBottom ? bottomUV : allFallback;
        Rect sides     = foundSide   ? sideUV   : allFallback;
        mapping.north = sides;
        mapping.south = sides;
        mapping.east  = sides;
        mapping.west  = sides;

        return mapping;
    }

    private enum BlockShape
    {
        FullCube,
        Cross,          // Two diagonal intersecting planes (flowers, grass, saplings)
        FlatHorizontal, // Single quad on ground (rails, pressure plates, carpets)
        NonOccluding,   // Full cube but transparent — doesn't hide neighbor faces (glass, leaves, water)
        Skip            // Not rendered (redstone wire, signs, banners, heads, etc.)
    }

    // Block names that should NOT be treated as full cubes.
    // Checked against the cleaned name (after stripping namespace prefix).
    private static readonly HashSet<string> crossBlocks = new HashSet<string>
    {
        // Flowers
        "dandelion", "poppy", "blue_orchid", "allium", "azure_bluet",
        "red_tulip", "orange_tulip", "white_tulip", "pink_tulip",
        "oxeye_daisy", "cornflower", "lily_of_the_valley", "wither_rose",
        "torchflower", "eyeblossom",
        // Saplings
        "oak_sapling", "spruce_sapling", "birch_sapling", "jungle_sapling",
        "acacia_sapling", "dark_oak_sapling", "cherry_sapling", "pale_oak_sapling",
        "mangrove_propagule", "azalea", "flowering_azalea",
        // Grass & ferns
        "short_grass", "tall_grass", "fern", "large_fern",
        "dead_bush", "nether_sprouts", "warped_roots", "crimson_roots",
        // Crops
        "wheat", "carrots", "potatoes", "beetroots", "sweet_berry_bush",
        "pitcher_crop", "torchflower_crop",
        // Mushrooms & fungi
        "brown_mushroom", "red_mushroom", "crimson_fungus", "warped_fungus",
        // Vines & hanging
        "vine", "cave_vines", "cave_vines_plant",
        "weeping_vines", "weeping_vines_plant",
        "twisting_vines", "twisting_vines_plant",
        // Underwater plants
        "seagrass", "tall_seagrass", "kelp", "kelp_plant",
        // Other cross-shaped
        "sugar_cane", "bamboo", "cobweb", "fire", "soul_fire",
        "hanging_roots", "spore_blossom",
    };

    private static readonly HashSet<string> flatBlocks = new HashSet<string>
    {
        // Rails
        "rail", "powered_rail", "detector_rail", "activator_rail",
        // Pressure plates
        "stone_pressure_plate", "oak_pressure_plate", "spruce_pressure_plate",
        "birch_pressure_plate", "jungle_pressure_plate", "acacia_pressure_plate",
        "dark_oak_pressure_plate", "mangrove_pressure_plate", "cherry_pressure_plate",
        "bamboo_pressure_plate", "crimson_pressure_plate", "warped_pressure_plate",
        "polished_blackstone_pressure_plate", "light_weighted_pressure_plate",
        "heavy_weighted_pressure_plate", "pale_oak_pressure_plate",
        // Carpets
        "white_carpet", "orange_carpet", "magenta_carpet", "light_blue_carpet",
        "yellow_carpet", "lime_carpet", "pink_carpet", "gray_carpet",
        "light_gray_carpet", "cyan_carpet", "purple_carpet", "blue_carpet",
        "brown_carpet", "green_carpet", "red_carpet", "black_carpet",
        "moss_carpet",
        // Other flat
        "lily_pad", "snow", "redstone_wire",
        "repeater", "comparator",
        "sculk_vein", "glow_lichen",
    };

    private static readonly HashSet<string> skipBlocks = new HashSet<string>
    {
        // Signs
        "oak_sign", "spruce_sign", "birch_sign", "jungle_sign",
        "acacia_sign", "dark_oak_sign", "mangrove_sign", "cherry_sign",
        "bamboo_sign", "crimson_sign", "warped_sign", "pale_oak_sign",
        "oak_wall_sign", "spruce_wall_sign", "birch_wall_sign", "jungle_wall_sign",
        "acacia_wall_sign", "dark_oak_wall_sign", "mangrove_wall_sign", "cherry_wall_sign",
        "bamboo_wall_sign", "crimson_wall_sign", "warped_wall_sign", "pale_oak_wall_sign",
        "oak_hanging_sign", "spruce_hanging_sign", "birch_hanging_sign", "jungle_hanging_sign",
        "acacia_hanging_sign", "dark_oak_hanging_sign", "mangrove_hanging_sign",
        "cherry_hanging_sign", "bamboo_hanging_sign", "crimson_hanging_sign",
        "warped_hanging_sign", "pale_oak_hanging_sign",
        // Banners
        "white_banner", "orange_banner", "magenta_banner", "light_blue_banner",
        "yellow_banner", "lime_banner", "pink_banner", "gray_banner",
        "light_gray_banner", "cyan_banner", "purple_banner", "blue_banner",
        "brown_banner", "green_banner", "red_banner", "black_banner",
        // Heads & skulls
        "skeleton_skull", "wither_skeleton_skull", "zombie_head", "player_head",
        "creeper_head", "dragon_head", "piglin_head",
        "skeleton_wall_skull", "wither_skeleton_wall_skull", "zombie_wall_head",
        "player_wall_head", "creeper_wall_head", "dragon_wall_head", "piglin_wall_head",
        // Item frames
        "item_frame", "glow_item_frame",
        // Too complex / thin geometry
        "string", "tripwire", "tripwire_hook",
        "flower_pot", "potted_oak_sapling", "potted_spruce_sapling",
        "potted_birch_sapling", "potted_jungle_sapling", "potted_acacia_sapling",
        "potted_dark_oak_sapling", "potted_cherry_sapling", "potted_mangrove_propagule",
        "potted_azalea_bush", "potted_flowering_azalea_bush",
        "potted_fern", "potted_dead_bush", "potted_dandelion", "potted_poppy",
        "potted_blue_orchid", "potted_allium", "potted_azure_bluet",
        "potted_red_tulip", "potted_orange_tulip", "potted_white_tulip",
        "potted_pink_tulip", "potted_oxeye_daisy", "potted_cornflower",
        "potted_lily_of_the_valley", "potted_wither_rose", "potted_bamboo",
        "potted_crimson_fungus", "potted_warped_fungus", "potted_crimson_roots",
        "potted_warped_roots", "potted_cactus", "potted_red_mushroom",
        "potted_brown_mushroom", "potted_torchflower",
        // Misc
        "end_rod", "lightning_rod", "chain",
        "candle", "white_candle", "orange_candle", "magenta_candle",
        "light_blue_candle", "yellow_candle", "lime_candle", "pink_candle",
        "gray_candle", "light_gray_candle", "cyan_candle", "purple_candle",
        "blue_candle", "brown_candle", "green_candle", "red_candle", "black_candle",
        "structure_void", "barrier", "light",
        "moving_piston",
    };

    private static readonly HashSet<string> nonOccludingBlocks = new HashSet<string>
    {
        // Glass
        "glass", "glass_pane",
        "white_stained_glass", "orange_stained_glass", "magenta_stained_glass",
        "light_blue_stained_glass", "yellow_stained_glass", "lime_stained_glass",
        "pink_stained_glass", "gray_stained_glass", "light_gray_stained_glass",
        "cyan_stained_glass", "purple_stained_glass", "blue_stained_glass",
        "brown_stained_glass", "green_stained_glass", "red_stained_glass",
        "black_stained_glass", "tinted_glass",
        "white_stained_glass_pane", "orange_stained_glass_pane", "magenta_stained_glass_pane",
        "light_blue_stained_glass_pane", "yellow_stained_glass_pane", "lime_stained_glass_pane",
        "pink_stained_glass_pane", "gray_stained_glass_pane", "light_gray_stained_glass_pane",
        "cyan_stained_glass_pane", "purple_stained_glass_pane", "blue_stained_glass_pane",
        "brown_stained_glass_pane", "green_stained_glass_pane", "red_stained_glass_pane",
        "black_stained_glass_pane",
        // Leaves
        "oak_leaves", "spruce_leaves", "birch_leaves", "jungle_leaves",
        "acacia_leaves", "dark_oak_leaves", "mangrove_leaves", "cherry_leaves",
        "azalea_leaves", "flowering_azalea_leaves", "pale_oak_leaves",
        // Ice
        "ice", "frosted_ice",
        // Water & lava (rendered as cube but transparent)
        "water", "lava",
        // Slime & honey
        "slime_block", "honey_block",
    };

    private BlockShape GetBlockShape(string blockName)
    {
        if (string.IsNullOrEmpty(blockName))
            return BlockShape.Skip;

        string cleanName = blockName.ToLowerInvariant();
        if (cleanName.Contains(":"))
            cleanName = cleanName.Substring(cleanName.LastIndexOf(':') + 1);

        if (skipBlocks.Contains(cleanName))
            return BlockShape.Skip;
        if (crossBlocks.Contains(cleanName))
            return BlockShape.Cross;
        if (flatBlocks.Contains(cleanName))
            return BlockShape.FlatHorizontal;
        if (nonOccludingBlocks.Contains(cleanName))
            return BlockShape.NonOccluding;

        // Catch-all patterns for blocks we might have missed
        if (cleanName.EndsWith("_sign") || cleanName.EndsWith("_wall_sign") ||
            cleanName.EndsWith("_hanging_sign") || cleanName.EndsWith("_banner") ||
            cleanName.EndsWith("_wall_banner") || cleanName.EndsWith("_candle"))
            return BlockShape.Skip;
        if (cleanName.EndsWith("_carpet"))
            return BlockShape.FlatHorizontal;
        if (cleanName.EndsWith("_pressure_plate"))
            return BlockShape.FlatHorizontal;

        return BlockShape.FullCube;
    }

    private bool IsAirBlock(string blockName)
    {
        if (string.IsNullOrEmpty(blockName))
            return true;

        string lowerName = blockName.ToLowerInvariant();
        return lowerName.Contains("air") ||
               lowerName.EndsWith(":air") ||
               lowerName == "universal_minecraft:air" ||
               lowerName == "minecraft:air" ||
               lowerName == "universal_minecraft:cave_air" ||
               lowerName == "minecraft:cave_air" ||
               lowerName == "universal_minecraft:void_air" ||
               lowerName == "minecraft:void_air";
    }

    private bool IsOccludingBlock(string blockName)
    {
        if (IsAirBlock(blockName)) return false;
        BlockShape shape = GetBlockShape(blockName);
        // Only full cubes occlude their neighbors
        return shape == BlockShape.FullCube;
    }

    private void AddFace(Vector3 blockPos, Vector3 normal, Vector3 up, Vector3 right, Rect uvRect,
        List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
    {
        float halfSize = blockSize / 2f;
        Vector3 center = blockPos + normal * halfSize;

        int baseIndex = vertices.Count;

        // Calculate the four corners of the face
        Vector3 v0 = center - up * halfSize - right * halfSize;
        Vector3 v1 = center + up * halfSize - right * halfSize;
        Vector3 v2 = center + up * halfSize + right * halfSize;
        Vector3 v3 = center - up * halfSize + right * halfSize;

        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);

        // Apply UV coordinates from atlas rect
        uvs.Add(new Vector2(uvRect.x, uvRect.y));
        uvs.Add(new Vector2(uvRect.x, uvRect.y + uvRect.height));
        uvs.Add(new Vector2(uvRect.x + uvRect.width, uvRect.y + uvRect.height));
        uvs.Add(new Vector2(uvRect.x + uvRect.width, uvRect.y));

        triangles.Add(baseIndex + 0);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 0);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 3);
    }

    /// <summary>
    /// Add an X-shaped cross (two diagonal planes) for plants, flowers, grass, etc.
    /// Both sides of each plane are rendered (double-sided).
    /// </summary>
    private void AddCross(Vector3 blockPos, Rect uvRect,
        List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
    {
        float halfSize = blockSize / 2f;

        // Diagonal 1: from (-x,-z) to (+x,+z)
        Vector3 v0 = blockPos + new Vector3(-halfSize, -halfSize, -halfSize);
        Vector3 v1 = blockPos + new Vector3(-halfSize,  halfSize, -halfSize);
        Vector3 v2 = blockPos + new Vector3( halfSize,  halfSize,  halfSize);
        Vector3 v3 = blockPos + new Vector3( halfSize, -halfSize,  halfSize);

        // Diagonal 2: from (-x,+z) to (+x,-z)
        Vector3 v4 = blockPos + new Vector3(-halfSize, -halfSize,  halfSize);
        Vector3 v5 = blockPos + new Vector3(-halfSize,  halfSize,  halfSize);
        Vector3 v6 = blockPos + new Vector3( halfSize,  halfSize, -halfSize);
        Vector3 v7 = blockPos + new Vector3( halfSize, -halfSize, -halfSize);

        Vector3 n1 = Vector3.Cross(v2 - v0, v1 - v0).normalized;
        Vector3 n2 = Vector3.Cross(v6 - v4, v5 - v4).normalized;

        Vector2 uv0 = new Vector2(uvRect.x, uvRect.y);
        Vector2 uv1 = new Vector2(uvRect.x, uvRect.y + uvRect.height);
        Vector2 uv2 = new Vector2(uvRect.x + uvRect.width, uvRect.y + uvRect.height);
        Vector2 uv3 = new Vector2(uvRect.x + uvRect.width, uvRect.y);

        // Diagonal 1 — front face
        int bi = vertices.Count;
        vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
        normals.Add(n1); normals.Add(n1); normals.Add(n1); normals.Add(n1);
        uvs.Add(uv0); uvs.Add(uv1); uvs.Add(uv2); uvs.Add(uv3);
        triangles.Add(bi); triangles.Add(bi+1); triangles.Add(bi+2);
        triangles.Add(bi); triangles.Add(bi+2); triangles.Add(bi+3);

        // Diagonal 1 — back face
        bi = vertices.Count;
        vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
        normals.Add(-n1); normals.Add(-n1); normals.Add(-n1); normals.Add(-n1);
        uvs.Add(uv3); uvs.Add(uv2); uvs.Add(uv1); uvs.Add(uv0);
        triangles.Add(bi); triangles.Add(bi+2); triangles.Add(bi+1);
        triangles.Add(bi); triangles.Add(bi+3); triangles.Add(bi+2);

        // Diagonal 2 — front face
        bi = vertices.Count;
        vertices.Add(v4); vertices.Add(v5); vertices.Add(v6); vertices.Add(v7);
        normals.Add(n2); normals.Add(n2); normals.Add(n2); normals.Add(n2);
        uvs.Add(uv0); uvs.Add(uv1); uvs.Add(uv2); uvs.Add(uv3);
        triangles.Add(bi); triangles.Add(bi+1); triangles.Add(bi+2);
        triangles.Add(bi); triangles.Add(bi+2); triangles.Add(bi+3);

        // Diagonal 2 — back face
        bi = vertices.Count;
        vertices.Add(v4); vertices.Add(v5); vertices.Add(v6); vertices.Add(v7);
        normals.Add(-n2); normals.Add(-n2); normals.Add(-n2); normals.Add(-n2);
        uvs.Add(uv3); uvs.Add(uv2); uvs.Add(uv1); uvs.Add(uv0);
        triangles.Add(bi); triangles.Add(bi+2); triangles.Add(bi+1);
        triangles.Add(bi); triangles.Add(bi+3); triangles.Add(bi+2);
    }

    /// <summary>
    /// Add a flat horizontal quad sitting on the bottom of the block space.
    /// Used for rails, pressure plates, carpets, etc. Double-sided.
    /// </summary>
    private void AddFlatHorizontal(Vector3 blockPos, Rect uvRect,
        List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
    {
        float halfSize = blockSize / 2f;
        // Slight offset above the bottom face to avoid z-fighting with the block below
        float yOffset = -halfSize + 0.01f;

        Vector3 v0 = blockPos + new Vector3(-halfSize, yOffset, -halfSize);
        Vector3 v1 = blockPos + new Vector3(-halfSize, yOffset,  halfSize);
        Vector3 v2 = blockPos + new Vector3( halfSize, yOffset,  halfSize);
        Vector3 v3 = blockPos + new Vector3( halfSize, yOffset, -halfSize);

        Vector2 uv0 = new Vector2(uvRect.x, uvRect.y);
        Vector2 uv1 = new Vector2(uvRect.x, uvRect.y + uvRect.height);
        Vector2 uv2 = new Vector2(uvRect.x + uvRect.width, uvRect.y + uvRect.height);
        Vector2 uv3 = new Vector2(uvRect.x + uvRect.width, uvRect.y);

        // Top face
        int bi = vertices.Count;
        vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
        normals.Add(Vector3.up); normals.Add(Vector3.up); normals.Add(Vector3.up); normals.Add(Vector3.up);
        uvs.Add(uv0); uvs.Add(uv1); uvs.Add(uv2); uvs.Add(uv3);
        triangles.Add(bi); triangles.Add(bi+1); triangles.Add(bi+2);
        triangles.Add(bi); triangles.Add(bi+2); triangles.Add(bi+3);

        // Bottom face
        bi = vertices.Count;
        vertices.Add(v0); vertices.Add(v1); vertices.Add(v2); vertices.Add(v3);
        normals.Add(Vector3.down); normals.Add(Vector3.down); normals.Add(Vector3.down); normals.Add(Vector3.down);
        uvs.Add(uv0); uvs.Add(uv1); uvs.Add(uv2); uvs.Add(uv3);
        triangles.Add(bi); triangles.Add(bi+2); triangles.Add(bi+1);
        triangles.Add(bi); triangles.Add(bi+3); triangles.Add(bi+2);
    }

    void Update()
    {
        if (isPaused && Input.GetKeyDown(continueKey))
        {
            isPaused = false;
            Debug.Log("Continuing...");
        }
    }
}

#region Data Classes

[Serializable]
public class WorldData
{
    public Metadata metadata;
    public RegionContainer region;
}

[Serializable]
public class Metadata
{
    public string level_name;
    public string world_path;
    public string game_version;
    public string platform;
}

[Serializable]
public class RegionContainer
{
    public RegionBounds region;
    public string dimension;
    public List<Chunk> chunks;
}

[Serializable]
public class RegionBounds
{
    public int min_chunk_x;
    public int min_chunk_z;
    public int max_chunk_x;
    public int max_chunk_z;
}

[Serializable]
public class Chunk
{
    public int chunk_x;
    public int chunk_z;
    public string dimension;
    public List<Block> blocks;
    public List<Entity> entities;
    public List<BlockEntity> block_entities;
}

[Serializable]
public class Block
{
    public int x;
    public int y;
    public int z;
    public string block;
    public BlockProperties properties;
}

[Serializable]
public class BlockProperties
{
    public string infiniburn;
}

[Serializable]
public class Entity
{
    // Add entity properties as needed
}

[Serializable]
public class BlockEntity
{
    // Add block entity properties as needed
}

#endregion

#region Atlas Data Classes

[Serializable]
public class BlockTextureMapping
{
    public Rect top;
    public Rect bottom;
    public Rect north;
    public Rect south;
    public Rect east;
    public Rect west;

    public bool HasAnyTexture()
    {
        return top.width > 0 || bottom.width > 0 || north.width > 0 || 
               south.width > 0 || east.width > 0 || west.width > 0;
    }
}

[Serializable]
public class TextureUVEntry
{
    public string name;
    public Rect rect;
}

[Serializable]
public class BlockMappingEntry
{
    public string blockName;
    public BlockTextureMapping mapping;
}

[Serializable]
public class BlockAtlasData
{
    public List<TextureUVEntry> textureUVs;
    public List<BlockMappingEntry> blockMappings;
}

#endregion
