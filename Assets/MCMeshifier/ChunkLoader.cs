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

        // Use a task to run the async export
        Task<bool> exportTask = ExportGameObjectToGltfAsync(chunkObject, filePath);
        
        // Wait for the task to complete
        while (!exportTask.IsCompleted)
        {
            yield return null;
        }

        if (exportTask.Exception != null)
        {
            Debug.LogError($"GLTF export failed with exception: {exportTask.Exception.InnerException?.Message ?? exportTask.Exception.Message}");
        }
        else if (exportTask.Result)
        {
            Debug.Log($"Successfully exported chunk to: {filePath}");
        }
        else
        {
            Debug.LogError($"GLTF export failed for chunk ({chunk.chunk_x}, {chunk.chunk_z})");
        }
    }

    private async Task<bool> ExportGameObjectToGltfAsync(GameObject gameObject, string filePath)
    {
        try
        {
            var exportSettings = new ExportSettings
            {
                Format = Path.GetExtension(filePath).ToLowerInvariant() == ".glb"
                    ? GltfFormat.Binary
                    : GltfFormat.Json,
                FileConflictResolution = FileConflictResolution.Overwrite
            };

            var gameObjectExportSettings = new GameObjectExportSettings
            {
                OnlyActiveInHierarchy = true,
                DisabledComponents = false
            };

            var export = new GameObjectExport(exportSettings, gameObjectExportSettings);
            export.AddScene(new[] { gameObject }, gameObject.name);

            return await export.SaveToFileAndDispose(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"GLTF export error: {ex.Message}");
            return false;
        }
    }

    private void GenerateChunkMesh(Chunk chunk)
    {
        currentChunkObject = new GameObject($"Chunk_{chunk.chunk_x}_{chunk.chunk_z}");
        currentChunkObject.transform.SetParent(transform);

        if (chunk.blocks == null) return;

        // Build a HashSet of all block positions for quick neighbor lookup
        HashSet<Vector3Int> blockPositions = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, Block> blockLookup = new Dictionary<Vector3Int, Block>();
        foreach (var block in chunk.blocks)
        {
            // Skip air blocks
            if (IsAirBlock(block.block))
                continue;
                
            Vector3Int pos = new Vector3Int(block.x, block.y, block.z);
            blockPositions.Add(pos);
            blockLookup[pos] = block;
        }

        // Collect all faces with their texture UVs
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        foreach (var block in chunk.blocks)
        {
            // Skip air blocks
            if (IsAirBlock(block.block))
                continue;
                
            Vector3Int blockPos = new Vector3Int(block.x, block.y, block.z);
            Vector3 worldPos = new Vector3(block.x * blockSize, block.y * blockSize, block.z * blockSize);
            
            // Get texture mapping for this block
            BlockTextureMapping mapping = GetBlockTextureMapping(block.block);

            // Check and add exposed faces
            // Top (Y+)
            if (!blockPositions.Contains(blockPos + Vector3Int.up))
            {
                AddFace(worldPos, Vector3.up, Vector3.forward, Vector3.right, mapping.top,
                    vertices, normals, uvs, triangles);
            }
            // Bottom (Y-)
            if (!blockPositions.Contains(blockPos + Vector3Int.down))
            {
                AddFace(worldPos, Vector3.down, Vector3.back, Vector3.right, mapping.bottom,
                    vertices, normals, uvs, triangles);
            }
            // North (Z+)
            if (!blockPositions.Contains(blockPos + new Vector3Int(0, 0, 1)))
            {
                AddFace(worldPos, Vector3.forward, Vector3.up, Vector3.left, mapping.north,
                    vertices, normals, uvs, triangles);
            }
            // South (Z-)
            if (!blockPositions.Contains(blockPos + new Vector3Int(0, 0, -1)))
            {
                AddFace(worldPos, Vector3.back, Vector3.up, Vector3.right, mapping.south,
                    vertices, normals, uvs, triangles);
            }
            // East (X+)
            if (!blockPositions.Contains(blockPos + Vector3Int.right))
            {
                AddFace(worldPos, Vector3.right, Vector3.up, Vector3.forward, mapping.east,
                    vertices, normals, uvs, triangles);
            }
            // West (X-)
            if (!blockPositions.Contains(blockPos + Vector3Int.left))
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

    private BlockTextureMapping GetBlockTextureMapping(string blockName)
    {
        // Strip prefix like "universal_minecraft:" or "minecraft:"
        string cleanName = blockName;
        if (cleanName.Contains(":"))
        {
            cleanName = cleanName.Substring(cleanName.LastIndexOf(':') + 1);
        }

        // Try to find exact match in blockMappings
        if (blockMappings.TryGetValue(cleanName, out BlockTextureMapping mapping))
        {
            return mapping;
        }

        // Try variations in blockMappings
        string[] variations = new string[]
        {
            cleanName,
            cleanName.Replace("_block", ""),
            cleanName + "_block",
            "oak_" + cleanName,  // log -> oak_log, planks -> oak_planks, etc.
            "stone_" + cleanName, // bricks -> stone_bricks, etc.
        };

        // Special case mappings
        if (cleanName == "plant")
        {
            variations = new string[] { "double_plant_grass_bottom", "grass" };
        }

        foreach (string variant in variations)
        {
            if (blockMappings.TryGetValue(variant, out mapping))
            {
                // Cache it for future lookups
                blockMappings[cleanName] = mapping;
                return mapping;
            }
        }

        // Try to generate mapping from textureUVs
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
        bool foundAny = false;

        // Special case mappings
        if (blockName == "plant")
        {
            blockName = "double_plant_grass_bottom";
        }

        // Try to find the base texture
        string[] baseVariants = new string[]
        {
            blockName,
            blockName.Replace("_block", ""),
            blockName + "_block",
            "oak_" + blockName,  // log -> oak_log
            "stone_" + blockName, // bricks -> stone_bricks
        };

        foreach (string variant in baseVariants)
        {
            if (textureUVs.TryGetValue(variant, out baseUV))
            {
                foundAny = true;
                // Update blockName to matched variant for subsequent lookups
                blockName = variant;
                break;
            }
        }

        // Try to find top texture
        string[] topVariants = new string[]
        {
            blockName + "_top",
            blockName.Replace("_block", "") + "_top",
        };
        foreach (string variant in topVariants)
        {
            if (textureUVs.TryGetValue(variant, out topUV))
            {
                foundAny = true;
                break;
            }
        }

        // Try to find bottom texture
        string[] bottomVariants = new string[]
        {
            blockName + "_bottom",
            blockName.Replace("_block", "") + "_bottom",
        };
        foreach (string variant in bottomVariants)
        {
            if (textureUVs.TryGetValue(variant, out bottomUV))
            {
                foundAny = true;
                break;
            }
        }

        // Try to find side texture
        string[] sideVariants = new string[]
        {
            blockName + "_side",
            blockName.Replace("_block", "") + "_side",
        };
        foreach (string variant in sideVariants)
        {
            if (textureUVs.TryGetValue(variant, out sideUV))
            {
                foundAny = true;
                break;
            }
        }

        if (!foundAny)
        {
            return null;
        }

        // Build the mapping with fallbacks
        BlockTextureMapping mapping = new BlockTextureMapping();
        
        // Use specific textures if found, otherwise fall back to base
        mapping.top = topUV.width > 0 ? topUV : baseUV;
        mapping.bottom = bottomUV.width > 0 ? bottomUV : baseUV;
        Rect sides = sideUV.width > 0 ? sideUV : baseUV;
        mapping.north = sides;
        mapping.south = sides;
        mapping.east = sides;
        mapping.west = sides;

        return mapping;
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
