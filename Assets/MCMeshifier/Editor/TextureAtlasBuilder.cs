using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TextureAtlasBuilder : EditorWindow
{
    private string texturePackPath = "";
    private string outputPath = "Assets/MCMeshifier/GeneratedAtlas";
    private int atlasSize = 4096;
    private int textureSize = 16; // Minecraft textures are 16x16

    [MenuItem("Tools/MCMeshifier/Build Texture Atlas")]
    public static void ShowWindow()
    {
        GetWindow<TextureAtlasBuilder>("Texture Atlas Builder");
    }

    void OnGUI()
    {
        GUILayout.Label("Minecraft Texture Atlas Builder", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        texturePackPath = EditorGUILayout.TextField("Texture Pack Path", texturePackPath);
        if (GUILayout.Button("Browse..."))
        {
            string path = EditorUtility.OpenFolderPanel("Select Texture Pack Root", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                texturePackPath = path;
            }
        }

        EditorGUILayout.Space();

        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        atlasSize = EditorGUILayout.IntPopup("Atlas Size", atlasSize, 
            new string[] { "1024", "2048", "4096", "8192" }, 
            new int[] { 1024, 2048, 4096, 8192 });

        EditorGUILayout.Space();

        if (GUILayout.Button("Build Atlas"))
        {
            BuildAtlas();
        }
    }

    private void BuildAtlas()
    {
        if (string.IsNullOrEmpty(texturePackPath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a texture pack path.", "OK");
            return;
        }

        string blockTexturesPath = Path.Combine(texturePackPath, "assets", "minecraft", "textures", "block");
        string modelsPath = Path.Combine(texturePackPath, "assets", "minecraft", "models", "block");

        if (!Directory.Exists(blockTexturesPath))
        {
            EditorUtility.DisplayDialog("Error", $"Block textures folder not found at:\n{blockTexturesPath}", "OK");
            return;
        }

        try
        {
            // Ensure output directory exists
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Get all PNG files
            string[] pngFiles = Directory.GetFiles(blockTexturesPath, "*.png", SearchOption.TopDirectoryOnly);
            
            // Filter out animated textures and emissive maps for now
            pngFiles = pngFiles.Where(f => 
                !File.Exists(f + ".mcmeta") && 
                !f.Contains("_emissive") &&
                !f.Contains("_s.png")).ToArray();

            Debug.Log($"Found {pngFiles.Length} textures to pack");

            // Calculate atlas dimensions
            int texturesPerRow = atlasSize / textureSize;
            int maxTextures = texturesPerRow * texturesPerRow;

            if (pngFiles.Length > maxTextures)
            {
                Debug.LogWarning($"Too many textures ({pngFiles.Length}) for atlas size. Only first {maxTextures} will be included.");
                pngFiles = pngFiles.Take(maxTextures).ToArray();
            }

            // Create atlas texture
            Texture2D atlas = new Texture2D(atlasSize, atlasSize, TextureFormat.RGBA32, false);
            atlas.filterMode = FilterMode.Point; // Pixel-perfect for Minecraft style
            
            // Fill with magenta (missing texture indicator)
            Color[] fillColors = new Color[atlasSize * atlasSize];
            for (int i = 0; i < fillColors.Length; i++)
            {
                fillColors[i] = (i / textureSize + i % textureSize) % 2 == 0 ? Color.magenta : Color.black;
            }
            atlas.SetPixels(fillColors);

            // Dictionary to store texture name -> UV rect
            Dictionary<string, Rect> textureUVs = new Dictionary<string, Rect>();

            float uvSize = (float)textureSize / atlasSize;

            for (int i = 0; i < pngFiles.Length; i++)
            {
                string filePath = pngFiles[i];
                string textureName = Path.GetFileNameWithoutExtension(filePath);

                EditorUtility.DisplayProgressBar("Building Atlas", $"Processing {textureName}", (float)i / pngFiles.Length);

                // Load texture
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);

                // Calculate position in atlas
                int row = i / texturesPerRow;
                int col = i % texturesPerRow;
                int x = col * textureSize;
                int y = atlasSize - (row + 1) * textureSize; // Flip Y for Unity

                // Resize if necessary and copy to atlas
                if (tex.width != textureSize || tex.height != textureSize)
                {
                    // For textures that aren't 16x16, just use the first 16x16 or scale
                    tex = ResizeTexture(tex, textureSize, textureSize);
                }

                Color[] pixels = tex.GetPixels();
                atlas.SetPixels(x, y, textureSize, textureSize, pixels);

                // Store UV coordinates
                float uvX = (float)x / atlasSize;
                float uvY = (float)y / atlasSize;
                textureUVs[textureName] = new Rect(uvX, uvY, uvSize, uvSize);

                DestroyImmediate(tex);
            }

            atlas.Apply();
            EditorUtility.ClearProgressBar();

            // Save atlas texture
            byte[] atlasBytes = atlas.EncodeToPNG();
            string atlasPath = Path.Combine(outputPath, "BlockAtlas.png");
            File.WriteAllBytes(atlasPath, atlasBytes);
            
            DestroyImmediate(atlas);

            // Parse models and build block mapping
            Dictionary<string, BlockTextureMapping> blockMappings = ParseBlockModels(modelsPath, textureUVs);

            // Save UV mapping data as JSON
            BlockAtlasData atlasData = new BlockAtlasData
            {
                textureUVs = textureUVs.Select(kvp => new TextureUVEntry { name = kvp.Key, rect = kvp.Value }).ToList(),
                blockMappings = blockMappings.Select(kvp => new BlockMappingEntry { blockName = kvp.Key, mapping = kvp.Value }).ToList()
            };

            string jsonPath = Path.Combine(outputPath, "BlockAtlasData.json");
            string json = JsonUtility.ToJson(atlasData, true);
            File.WriteAllText(jsonPath, json);

            // Refresh asset database
            AssetDatabase.Refresh();

            // Create material
            CreateAtlasMaterial(atlasPath);

            EditorUtility.DisplayDialog("Success", 
                $"Atlas built successfully!\n\nTextures packed: {pngFiles.Length}\nBlock mappings: {blockMappings.Count}\n\nOutput: {outputPath}", 
                "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"Error building atlas: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog("Error", $"Failed to build atlas:\n{ex.Message}", "OK");
        }
    }

    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        rt.filterMode = FilterMode.Point;
        
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        
        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }

    private Dictionary<string, BlockTextureMapping> ParseBlockModels(string modelsPath, Dictionary<string, Rect> textureUVs)
    {
        Dictionary<string, BlockTextureMapping> mappings = new Dictionary<string, BlockTextureMapping>();

        if (!Directory.Exists(modelsPath))
        {
            Debug.LogWarning($"Models path not found: {modelsPath}. Using texture names directly.");
            // Fallback: create simple mappings from texture names
            foreach (var kvp in textureUVs)
            {
                string blockName = kvp.Key;
                // Skip variants and special textures
                if (blockName.Contains("_side") || blockName.Contains("_top") || blockName.Contains("_bottom") ||
                    blockName.Contains("_front") || blockName.Contains("_back") || blockName.Contains("_end"))
                    continue;

                mappings[blockName] = CreateSimpleMapping(blockName, textureUVs);
            }
            return mappings;
        }

        string[] modelFiles = Directory.GetFiles(modelsPath, "*.json");

        foreach (string modelFile in modelFiles)
        {
            try
            {
                string json = File.ReadAllText(modelFile);
                string blockName = Path.GetFileNameWithoutExtension(modelFile);

                // Skip variants (numbered versions, etc.)
                if (blockName.Contains("_stage") || blockName.EndsWith("_1") || blockName.EndsWith("_2") || 
                    blockName.EndsWith("_3") || blockName.EndsWith("_4") || blockName.EndsWith("_5"))
                    continue;

                BlockTextureMapping mapping = ParseModelJson(json, textureUVs, modelsPath);
                if (mapping != null && mapping.HasAnyTexture())
                {
                    mappings[blockName] = mapping;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse model {modelFile}: {ex.Message}");
            }
        }

        // Add common blocks that might be missing
        AddCommonBlockMappings(mappings, textureUVs);

        return mappings;
    }

    private BlockTextureMapping ParseModelJson(string json, Dictionary<string, Rect> textureUVs, string modelsPath)
    {
        // Simple JSON parsing for Minecraft model format
        BlockTextureMapping mapping = new BlockTextureMapping();

        // Check for parent model
        if (json.Contains("\"parent\""))
        {
            string parent = ExtractJsonValue(json, "parent");
            if (!string.IsNullOrEmpty(parent))
            {
                parent = parent.Replace("minecraft:block/", "").Replace("block/", "");
                
                // Handle common parent types
                if (parent == "cube_all")
                {
                    string allTex = ExtractTextureRef(json, "all");
                    if (!string.IsNullOrEmpty(allTex) && textureUVs.TryGetValue(allTex, out Rect uv))
                    {
                        mapping.top = mapping.bottom = mapping.north = mapping.south = mapping.east = mapping.west = uv;
                    }
                }
                else if (parent == "cube_column" || parent == "cube_column_horizontal")
                {
                    string endTex = ExtractTextureRef(json, "end");
                    string sideTex = ExtractTextureRef(json, "side");
                    
                    if (!string.IsNullOrEmpty(endTex) && textureUVs.TryGetValue(endTex, out Rect endUV))
                    {
                        mapping.top = mapping.bottom = endUV;
                    }
                    if (!string.IsNullOrEmpty(sideTex) && textureUVs.TryGetValue(sideTex, out Rect sideUV))
                    {
                        mapping.north = mapping.south = mapping.east = mapping.west = sideUV;
                    }
                }
                else if (parent == "cube_bottom_top")
                {
                    string topTex = ExtractTextureRef(json, "top");
                    string bottomTex = ExtractTextureRef(json, "bottom");
                    string sideTex = ExtractTextureRef(json, "side");
                    
                    if (!string.IsNullOrEmpty(topTex) && textureUVs.TryGetValue(topTex, out Rect topUV))
                        mapping.top = topUV;
                    if (!string.IsNullOrEmpty(bottomTex) && textureUVs.TryGetValue(bottomTex, out Rect bottomUV))
                        mapping.bottom = bottomUV;
                    if (!string.IsNullOrEmpty(sideTex) && textureUVs.TryGetValue(sideTex, out Rect sideUV))
                        mapping.north = mapping.south = mapping.east = mapping.west = sideUV;
                }
                else if (parent == "cube")
                {
                    Rect? upUV = GetTextureUV(json, "up", textureUVs);
                    Rect? downUV = GetTextureUV(json, "down", textureUVs);
                    Rect? northUV = GetTextureUV(json, "north", textureUVs);
                    Rect? southUV = GetTextureUV(json, "south", textureUVs);
                    Rect? eastUV = GetTextureUV(json, "east", textureUVs);
                    Rect? westUV = GetTextureUV(json, "west", textureUVs);
                    
                    if (upUV.HasValue) mapping.top = upUV.Value;
                    if (downUV.HasValue) mapping.bottom = downUV.Value;
                    if (northUV.HasValue) mapping.north = northUV.Value;
                    if (southUV.HasValue) mapping.south = southUV.Value;
                    if (eastUV.HasValue) mapping.east = eastUV.Value;
                    if (westUV.HasValue) mapping.west = westUV.Value;
                }
            }
        }

        // Direct texture definitions override parent
        if (json.Contains("\"textures\""))
        {
            Rect? allTex = GetTextureUV(json, "all", textureUVs);
            if (allTex.HasValue)
            {
                mapping.top = mapping.bottom = mapping.north = mapping.south = mapping.east = mapping.west = allTex.Value;
            }

            Rect? topTex = GetTextureUV(json, "top", textureUVs);
            if (topTex.HasValue) mapping.top = topTex.Value;

            Rect? bottomTex = GetTextureUV(json, "bottom", textureUVs);
            if (bottomTex.HasValue) mapping.bottom = bottomTex.Value;

            Rect? endTex = GetTextureUV(json, "end", textureUVs);
            if (endTex.HasValue) mapping.top = mapping.bottom = endTex.Value;

            Rect? sideTex = GetTextureUV(json, "side", textureUVs);
            if (sideTex.HasValue) mapping.north = mapping.south = mapping.east = mapping.west = sideTex.Value;
        }

        return mapping;
    }

    private Rect? GetTextureUV(string json, string key, Dictionary<string, Rect> textureUVs)
    {
        string texRef = ExtractTextureRef(json, key);
        if (!string.IsNullOrEmpty(texRef) && textureUVs.TryGetValue(texRef, out Rect uv))
        {
            return uv;
        }
        return null;
    }

    private string ExtractTextureRef(string json, string key)
    {
        string value = ExtractJsonValue(json, key);
        if (string.IsNullOrEmpty(value)) return null;

        // Remove minecraft:block/ prefix
        value = value.Replace("minecraft:block/", "").Replace("block/", "");
        return value;
    }

    private string ExtractJsonValue(string json, string key)
    {
        // Simple regex-free JSON value extraction
        string searchKey = $"\"{key}\"";
        int keyIndex = json.IndexOf(searchKey);
        if (keyIndex < 0) return null;

        int colonIndex = json.IndexOf(':', keyIndex);
        if (colonIndex < 0) return null;

        int valueStart = -1;
        int valueEnd = -1;

        for (int i = colonIndex + 1; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '"')
            {
                if (valueStart < 0)
                    valueStart = i + 1;
                else
                {
                    valueEnd = i;
                    break;
                }
            }
        }

        if (valueStart >= 0 && valueEnd > valueStart)
        {
            return json.Substring(valueStart, valueEnd - valueStart);
        }

        return null;
    }

    private BlockTextureMapping CreateSimpleMapping(string textureName, Dictionary<string, Rect> textureUVs)
    {
        BlockTextureMapping mapping = new BlockTextureMapping();

        // Try to find texture variations
        Rect baseUV = textureUVs.ContainsKey(textureName) ? textureUVs[textureName] : default;
        Rect topUV = textureUVs.ContainsKey(textureName + "_top") ? textureUVs[textureName + "_top"] : baseUV;
        Rect bottomUV = textureUVs.ContainsKey(textureName + "_bottom") ? textureUVs[textureName + "_bottom"] : baseUV;
        Rect sideUV = textureUVs.ContainsKey(textureName + "_side") ? textureUVs[textureName + "_side"] : baseUV;

        mapping.top = topUV;
        mapping.bottom = bottomUV;
        mapping.north = mapping.south = mapping.east = mapping.west = sideUV.width > 0 ? sideUV : baseUV;

        return mapping;
    }

    private void AddCommonBlockMappings(Dictionary<string, BlockTextureMapping> mappings, Dictionary<string, Rect> textureUVs)
    {
        // Add grass_block if not present
        if (!mappings.ContainsKey("grass_block"))
        {
            BlockTextureMapping grass = new BlockTextureMapping();
            if (textureUVs.TryGetValue("grass_block_top", out Rect grassTop))
                grass.top = grassTop;
            if (textureUVs.TryGetValue("dirt", out Rect dirt))
                grass.bottom = dirt;
            if (textureUVs.TryGetValue("grass_block_side", out Rect grassSide))
                grass.north = grass.south = grass.east = grass.west = grassSide;
            
            if (grass.HasAnyTexture())
                mappings["grass_block"] = grass;
        }

        // Add stone if not present
        if (!mappings.ContainsKey("stone") && textureUVs.TryGetValue("stone", out Rect stoneUV))
        {
            mappings["stone"] = new BlockTextureMapping
            {
                top = stoneUV, bottom = stoneUV,
                north = stoneUV, south = stoneUV, east = stoneUV, west = stoneUV
            };
        }

        // Add bedrock if not present
        if (!mappings.ContainsKey("bedrock") && textureUVs.TryGetValue("bedrock", out Rect bedrockUV))
        {
            mappings["bedrock"] = new BlockTextureMapping
            {
                top = bedrockUV, bottom = bedrockUV,
                north = bedrockUV, south = bedrockUV, east = bedrockUV, west = bedrockUV
            };
        }

        // Add dirt if not present
        if (!mappings.ContainsKey("dirt") && textureUVs.TryGetValue("dirt", out Rect dirtUV))
        {
            mappings["dirt"] = new BlockTextureMapping
            {
                top = dirtUV, bottom = dirtUV,
                north = dirtUV, south = dirtUV, east = dirtUV, west = dirtUV
            };
        }
    }

    private void CreateAtlasMaterial(string atlasPath)
    {
        // Load the atlas texture
        AssetDatabase.Refresh();
        
        string relativePath = atlasPath.Replace(Application.dataPath, "Assets");
        // Fix path separators for Unity
        relativePath = relativePath.Replace("\\", "/");
        
        Texture2D atlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);

        if (atlasTexture == null)
        {
            Debug.LogWarning($"Could not load atlas texture for material creation at path: {relativePath}");
            return;
        }

        // Set texture import settings
        string texturePath = AssetDatabase.GetAssetPath(atlasTexture);
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            
            // Reload texture after reimport
            atlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
        }

        // Create material using a shader - try URP first, then fallback
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Texture");
        }
        
        if (shader == null)
        {
            Debug.LogError("Could not find any valid shader! Please manually assign a shader to the material.");
            return;
        }

        Material material = new Material(shader);
        material.mainTexture = atlasTexture;
        
        // Set properties based on shader type
        if (shader.name.Contains("Universal Render Pipeline/Lit"))
        {
            material.SetTexture("_BaseMap", atlasTexture);
            material.SetFloat("_Smoothness", 0f);
            material.SetFloat("_Metallic", 0f);
        }
        else if (shader.name.Contains("Universal Render Pipeline/Unlit") || 
                 shader.name.Contains("Universal Render Pipeline/Simple Lit"))
        {
            material.SetTexture("_BaseMap", atlasTexture);
        }
        else if (shader.name == "Standard")
        {
            material.SetFloat("_Glossiness", 0f);
            material.SetFloat("_Metallic", 0f);
        }

        string materialPath = Path.Combine(outputPath, "BlockAtlasMaterial.mat");
        materialPath = materialPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
        
        // Delete existing material if it exists
        if (File.Exists(materialPath.Replace("Assets", Application.dataPath)))
        {
            AssetDatabase.DeleteAsset(materialPath);
        }
        
        AssetDatabase.CreateAsset(material, materialPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Material created with shader: {shader.name}, texture: {atlasTexture.name}");
    }
}

// Note: BlockTextureMapping, TextureUVEntry, BlockMappingEntry, and BlockAtlasData
// are defined in ChunkLoader.cs to be available at runtime
