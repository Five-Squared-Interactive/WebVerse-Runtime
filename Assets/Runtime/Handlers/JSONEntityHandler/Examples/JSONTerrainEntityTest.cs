using UnityEngine;
using FiveSQD.WebVerse.Handlers.JSONEntity;
using System;

namespace FiveSQD.WebVerse.Examples
{
    /// <summary>
    /// Example demonstrating terrain entity creation with JSONEntityHandler.
    /// </summary>
    public class JSONTerrainEntityTest : MonoBehaviour
    {
        [Header("Terrain Entity Test")]
        public string testHeightmapTerrain = @"{
  ""id"": ""test-heightmap-terrain"",
  ""tag"": ""Test Heightmap"",
  ""position"": { ""x"": 0, ""y"": 0, ""z"": 0 },
  ""terrainType"": ""heightmap"",
  ""length"": 50.0,
  ""width"": 50.0,
  ""height"": 10.0,
  ""heights"": [
    [0, 1, 2, 3, 2, 1, 0],
    [1, 2, 4, 6, 4, 2, 1],
    [2, 4, 6, 8, 6, 4, 2],
    [3, 6, 8, 10, 8, 6, 3],
    [2, 4, 6, 8, 6, 4, 2],
    [1, 2, 4, 6, 4, 2, 1],
    [0, 1, 2, 3, 2, 1, 0]
  ],
  ""layers"": [
    {
      ""diffuseTexture"": ""Textures/grass.jpg"",
      ""normalTexture"": ""Textures/grass_normal.jpg"",
      ""specular"": { ""r"": 0.1, ""g"": 0.2, ""b"": 0.1, ""a"": 1.0 },
      ""metallic"": 0.0,
      ""smoothness"": 0.4,
      ""sizeFactor"": 1
    }
  ],
  ""layerMasks"": [],
  ""stitchTerrains"": false
}";

        [Header("Hybrid Terrain Test")]
        public string testHybridTerrain = @"{
  ""id"": ""test-hybrid-terrain"",
  ""tag"": ""Test Hybrid"",
  ""position"": { ""x"": 60, ""y"": 0, ""z"": 0 },
  ""terrainType"": ""hybrid"",
  ""length"": 30.0,
  ""width"": 30.0,
  ""height"": 8.0,
  ""heights"": [
    [0, 1, 1, 0],
    [1, 2, 2, 1],
    [1, 2, 2, 1],
    [0, 1, 1, 0]
  ],
  ""layers"": [
    {
      ""diffuseTexture"": ""Textures/dirt.jpg"",
      ""specular"": { ""r"": 0.2, ""g"": 0.15, ""b"": 0.1, ""a"": 1.0 },
      ""metallic"": 0.1,
      ""smoothness"": 0.2,
      ""sizeFactor"": 1
    }
  ],
  ""modifications"": [
    {
      ""operation"": ""dig"",
      ""position"": { ""x"": 5, ""y"": 0, ""z"": 5 },
      ""brushType"": ""sphere"",
      ""layer"": 0,
      ""size"": 3.0
    }
  ],
  ""stitchTerrains"": false
}";

        [Header("Test Settings")]
        public bool testHeightmap = true;
        public bool testHybrid = true;

        private JSONEntityHandler jsonHandler;

        void Start()
        {
            jsonHandler = FindFirstObjectByType<JSONEntityHandler>();
            if (jsonHandler == null)
            {
                Debug.LogError("JSONEntityHandler not found in scene!");
                return;
            }

            Debug.Log("Testing terrain entity creation with JSONEntityHandler...");

            if (testHeightmap)
            {
                TestHeightmapTerrain();
            }

            if (testHybrid)
            {
                TestHybridTerrain();
            }
        }

        void TestHeightmapTerrain()
        {
            Debug.Log("Creating heightmap terrain...");

            jsonHandler.ProcessTerrainEntityJSON(testHeightmapTerrain, null,
                onSuccess: (entityId, entity) =>
                {
                    Debug.Log($"SUCCESS: Created heightmap terrain with ID: {entityId}");
                    Debug.Log($"Entity GameObject: {entity?.name}");
                    Debug.Log("This terrain was created using TerrainEntity.CreateHeightmap() directly!");
                },
                onError: (error) =>
                {
                    Debug.LogError($"FAILED to create heightmap terrain: {error}");
                }
            );
        }

        void TestHybridTerrain()
        {
            Debug.Log("Creating hybrid terrain...");

            jsonHandler.ProcessTerrainEntityJSON(testHybridTerrain, null,
                onSuccess: (entityId, entity) =>
                {
                    Debug.Log($"SUCCESS: Created hybrid terrain with ID: {entityId}");
                    Debug.Log($"Entity GameObject: {entity?.name}");
                    Debug.Log("This terrain was created using TerrainEntity.CreateHybrid() directly!");
                    
                    // Test runtime modification (hybrid terrain only)
                    TestRuntimeModification(entity);
                },
                onError: (error) =>
                {
                    Debug.LogError($"FAILED to create hybrid terrain: {error}");
                }
            );
        }

        void TestRuntimeModification(FiveSQD.StraightFour.Entity.BaseEntity terrainEntity)
        {
            // Test if we can perform runtime modifications on hybrid terrain
            if (terrainEntity is FiveSQD.StraightFour.Entity.HybridTerrainEntity hybridTerrain)
            {
                Debug.Log("Testing runtime terrain modification...");
                
                // Dig a hole at a different position
                bool digResult = hybridTerrain.Dig(
                    new UnityEngine.Vector3(-5, 0, -5),
                    FiveSQD.StraightFour.Entity.Terrain.TerrainEntityBrushType.sphere,
                    0, // layer
                    2.0f, // size
                    true // synchronizeChange
                );
                
                Debug.Log($"Runtime dig operation result: {digResult}");
                
                // Build a mound at another position
                bool buildResult = hybridTerrain.Build(
                    new UnityEngine.Vector3(8, 0, 8),
                    FiveSQD.StraightFour.Entity.Terrain.TerrainEntityBrushType.roundedCube,
                    0, // layer
                    2.5f, // size
                    true // synchronizeChange
                );
                
                Debug.Log($"Runtime build operation result: {buildResult}");
            }
            else
            {
                Debug.Log("Entity is not a hybrid terrain - runtime modifications not supported");
            }
        }

        [ContextMenu("Create Sample Terrains")]
        void CreateSampleTerrains()
        {
            if (jsonHandler == null)
            {
                jsonHandler = FindFirstObjectByType<JSONEntityHandler>();
            }

            TestHeightmapTerrain();
            TestHybridTerrain();
        }
    }
}