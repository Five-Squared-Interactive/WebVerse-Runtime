using UnityEngine;
using FiveSQD.WebVerse.Handlers.JSONEntity;
using System;

namespace FiveSQD.WebVerse.Examples
{
    /// <summary>
    /// Example demonstrating the updated JSONEntityHandler using MeshEntity.cs methods directly.
    /// </summary>
    public class JSONEntityPrimitiveTest : MonoBehaviour
    {
        [Header("JSONEntityHandler Test")]
        public string testJsonPrimitive = @"{
  ""id"": ""test-cube-001"",
  ""tag"": ""Test Cube"",
  ""position"": { ""x"": 0.0, ""y"": 1.0, ""z"": 0.0 },
  ""rotation"": { ""x"": 0.0, ""y"": 45.0, ""z"": 0.0, ""w"": 1.0 },
  ""scale"": { ""x"": 1.5, ""y"": 1.5, ""z"": 1.5 },
  ""meshType"": ""primitive"",
  ""meshSource"": ""cube"",
  ""color"": { ""r"": 0.0, ""g"": 0.8, ""b"": 1.0, ""a"": 1.0 }
}";

        [Header("Test New Primitives")]
        public bool testAllPrimitives = false;

        private JSONEntityHandler jsonHandler;

        void Start()
        {
            jsonHandler = FindFirstObjectByType<JSONEntityHandler>();
            if (jsonHandler == null)
            {
                Debug.LogError("JSONEntityHandler not found in scene!");
                return;
            }

            // Test basic cube creation
            TestBasicPrimitive();

            // Optionally test all primitive types
            if (testAllPrimitives)
            {
                TestAllPrimitiveTypes();
            }
        }

        void TestBasicPrimitive()
        {
            Debug.Log("Testing primitive creation using MeshEntity.cs methods directly...");

            jsonHandler.ProcessMeshEntityJSON(testJsonPrimitive, null,
                onSuccess: (entityId, entity) =>
                {
                    Debug.Log($"SUCCESS: Created primitive entity with ID: {entityId}");
                    Debug.Log($"Entity GameObject: {entity?.name}");
                    Debug.Log("This primitive was created using MeshEntity.CreateCube() directly!");
                },
                onError: (error) =>
                {
                    Debug.LogError($"FAILED to create primitive: {error}");
                }
            );
        }

        void TestAllPrimitiveTypes()
        {
            string[] primitiveTypes = { "cube", "sphere", "cylinder", "capsule", "plane", 
                                     "torus", "cone", "rectangularpyramid", "tetrahedron", 
                                     "prism", "arch" };

            Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta,
                             Color.cyan, Color.white, Color.gray, Color.black, new Color(1, 0.5f, 0),
                             new Color(0.5f, 0, 1) };

            for (int i = 0; i < primitiveTypes.Length; i++)
            {
                Vector3 position = new Vector3((i % 4) * 3, 0, (i / 4) * 3);
                Color color = colors[i % colors.Length];

                string json = $@"{{
  ""id"": ""test-{primitiveTypes[i]}-{i:000}"",
  ""tag"": ""Test {primitiveTypes[i]}"",
  ""position"": {{ ""x"": {position.x}, ""y"": {position.y}, ""z"": {position.z} }},
  ""meshType"": ""primitive"",
  ""meshSource"": ""{primitiveTypes[i]}"",
  ""color"": {{ ""r"": {color.r}, ""g"": {color.g}, ""b"": {color.b}, ""a"": {color.a} }}
}}";

                Debug.Log($"Creating {primitiveTypes[i]} at position {position}");

                jsonHandler.ProcessMeshEntityJSON(json, null,
                    onSuccess: (entityId, entity) =>
                    {
                        Debug.Log($"SUCCESS: Created {primitiveTypes[i]} entity with ID: {entityId}");
                    },
                    onError: (error) =>
                    {
                        Debug.LogError($"FAILED to create {primitiveTypes[i]}: {error}");
                    }
                );
            }
        }
    }
}