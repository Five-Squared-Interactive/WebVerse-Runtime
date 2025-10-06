// Example usage of JSONEntityHandler for Container, Mesh, and Terrain entities
using UnityEngine;
using FiveSQD.WebVerse.Handlers.JSONEntity;
using FiveSQD.StraightFour.Entity;
using System;

namespace Examples
{
    public class JSONEntityExample : MonoBehaviour
    {
        [SerializeField] private JSONEntityHandler jsonHandler;

        void Start()
        {
            // Initialize the handler
            if (jsonHandler == null)
            {
                jsonHandler = FindFirstObjectByType<JSONEntityHandler>();
            }

            if (jsonHandler != null)
            {
                jsonHandler.Initialize();
                
                // Wait a frame for initialization, then create entities
                Invoke(nameof(CreateExampleEntities), 0.1f);
            }
            else
            {
                Debug.LogError("JSONEntityHandler not found!");
            }
        }

        void CreateExampleEntities()
        {
            CreateContainerEntity();
            CreateMeshEntities();
        }

        void CreateContainerEntity()
        {
            string containerJSON = @"{
                ""id"": ""550e8400-e29b-41d4-a716-446655440000"",
                ""tag"": ""ExampleContainer"",
                ""position"": { ""x"": 0, ""y"": 0, ""z"": 0 },
                ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
                ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
                ""isSize"": false,
                ""parentId"": null,
                ""children"": []
            }";

            jsonHandler.ProcessContainerEntityJSON(
                jsonString: containerJSON,
                parentEntity: null,
                onSuccess: (entityId, createdEntity) =>
                {
                    Debug.Log($"Successfully created container entity: {entityId}");
                    
                    // Now create mesh entities as children of this container
                    CreateChildMeshEntities(createdEntity);
                },
                onError: (errorMessage) =>
                {
                    Debug.LogError($"Failed to create container entity: {errorMessage}");
                }
            );
        }

        void CreateMeshEntities()
        {
            // Create a primitive cube
            string cubeJSON = @"{
                ""id"": ""550e8400-e29b-41d4-a716-446655440001"",
                ""tag"": ""RedCube"",
                ""position"": { ""x"": -2, ""y"": 1, ""z"": 0 },
                ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
                ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
                ""isSize"": false,
                ""meshType"": ""primitive"",
                ""meshSource"": ""Cube"",
                ""materials"": [
                    {
                        ""name"": ""RedMaterial"",
                        ""color"": { ""r"": 1, ""g"": 0, ""b"": 0, ""a"": 1 },
                        ""shader"": ""Standard""
                    }
                ],
                ""children"": []
            }";

            jsonHandler.ProcessMeshEntityJSON(
                jsonString: cubeJSON,
                parentEntity: null,
                onSuccess: (entityId, createdEntity) =>
                {
                    Debug.Log($"Successfully created red cube: {entityId}");
                },
                onError: (errorMessage) =>
                {
                    Debug.LogError($"Failed to create red cube: {errorMessage}");
                }
            );

            // Create a primitive sphere
            string sphereJSON = @"{
                ""id"": ""550e8400-e29b-41d4-a716-446655440002"",
                ""tag"": ""BlueSphere"",
                ""position"": { ""x"": 2, ""y"": 1, ""z"": 0 },
                ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
                ""scale"": { ""x"": 1.5, ""y"": 1.5, ""z"": 1.5 },
                ""isSize"": false,
                ""meshType"": ""primitive"",
                ""meshSource"": ""Sphere"",
                ""materials"": [
                    {
                        ""name"": ""BlueMaterial"",
                        ""color"": { ""r"": 0, ""g"": 0, ""b"": 1, ""a"": 1 },
                        ""shader"": ""Standard""
                    }
                ],
                ""children"": []
            }";

            jsonHandler.ProcessMeshEntityJSON(
                jsonString: sphereJSON,
                parentEntity: null,
                onSuccess: (entityId, createdEntity) =>
                {
                    Debug.Log($"Successfully created blue sphere: {entityId}");
                },
                onError: (errorMessage) =>
                {
                    Debug.LogError($"Failed to create blue sphere: {errorMessage}");
                }
            );
        }

        void CreateChildMeshEntities(BaseEntity parentContainer)
        {
            // Create a child mesh entity under the container
            string childMeshJSON = @"{
                ""id"": ""550e8400-e29b-41d4-a716-446655440003"",
                ""tag"": ""ChildCylinder"",
                ""position"": { ""x"": 0, ""y"": 2, ""z"": 0 },
                ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
                ""scale"": { ""x"": 0.5, ""y"": 2, ""z"": 0.5 },
                ""isSize"": false,
                ""meshType"": ""primitive"",
                ""meshSource"": ""Cylinder"",
                ""materials"": [
                    {
                        ""name"": ""GreenMaterial"",
                        ""color"": { ""r"": 0, ""g"": 1, ""b"": 0, ""a"": 1 },
                        ""shader"": ""Standard""
                    }
                ],
                ""children"": []
            }";

            jsonHandler.ProcessMeshEntityJSON(
                jsonString: childMeshJSON,
                parentEntity: parentContainer,
                onSuccess: (entityId, createdEntity) =>
                {
                    Debug.Log($"Successfully created child cylinder: {entityId}");
                },
                onError: (errorMessage) =>
                {
                    Debug.LogError($"Failed to create child cylinder: {errorMessage}");
                }
            );
        }

        // Example of loading from file
        [ContextMenu("Load Entity From File")]
        void LoadFromFileExample()
        {
            string filePath = Application.dataPath + "/example-entity.json";
            
            jsonHandler.LoadContainerEntityFromFile(
                filePath: filePath,
                parentEntity: null,
                onComplete: (success, entityId, createdEntity) =>
                {
                    if (success)
                    {
                        Debug.Log($"Successfully loaded entity from file: {entityId}");
                    }
                    else
                    {
                        Debug.LogError("Failed to load entity from file");
                    }
                }
            );
        }
    }
}