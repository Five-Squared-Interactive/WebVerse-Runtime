// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using UnityEngine;
using FiveSQD.WebVerse.Handlers.JSONEntity;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.StraightFour.Entity;

namespace FiveSQD.WebVerse.Examples
{
    /// <summary>
    /// Example script demonstrating Image, HTML, and Light entity creation from JSON using JSONEntityHandler.
    /// This script shows how to load and create various multimedia and lighting entities.
    /// </summary>
    public class MultiEntityJSONExample : MonoBehaviour
    {
        [Header("JSON Entity Handler")]
        [SerializeField]
        private JSONEntityHandler jsonEntityHandler;

        [Header("Example JSON Files")]
        [SerializeField]
        private string imageExamplePath = "Files/Examples/Entities/image_example.json";
        
        [SerializeField]
        private string htmlUIExamplePath = "Files/Examples/Entities/html_ui_example.json";
        
        [SerializeField]
        private string html3DExamplePath = "Files/Examples/Entities/html_3d_example.json";
        
        [SerializeField]
        private string lightPointExamplePath = "Files/Examples/Entities/light_point_example.json";
        
        [SerializeField]
        private string lightSpotExamplePath = "Files/Examples/Entities/light_spot_example.json";
        
        [SerializeField]
        private string lightDirectionalExamplePath = "Files/Examples/Entities/light_directional_example.json";
        
        [SerializeField]
        private string mixedSceneExamplePath = "Files/Examples/Entities/mixed_entities_scene.json";

        [Header("Demo Controls")]
        [SerializeField]
        private bool loadImageEntities = true;
        
        [SerializeField]
        private bool loadHTMLEntities = true;
        
        [SerializeField]
        private bool loadLightEntities = true;
        
        [SerializeField]
        private bool loadMixedScene = false;

        [Header("Parent Entities")]
        [SerializeField]
        private BaseEntity canvasEntity; // For UI elements
        
        [SerializeField]
        private BaseEntity worldParentEntity; // For 3D world elements

        private BaseEntity createdCanvas;

        void Start()
        {
            if (jsonEntityHandler == null)
            {
                jsonEntityHandler = FindFirstObjectByType<JSONEntityHandler>();
                if (jsonEntityHandler == null)
                {
                    Logging.LogError("[MultiEntityJSONExample] JSONEntityHandler not found in scene");
                    return;
                }
            }

            StartCoroutine(LoadEntityExamples());
        }

        /// <summary>
        /// Load all entity examples based on configuration.
        /// </summary>
        private IEnumerator LoadEntityExamples()
        {
            yield return new WaitForSeconds(1.0f); // Wait for system initialization

            // Create a canvas if we don't have one for UI elements
            if (canvasEntity == null && (loadImageEntities || loadHTMLEntities))
            {
                yield return CreateDemoCanvas();
            }

            if (loadMixedScene)
            {
                yield return LoadMixedSceneExample();
            }
            else
            {
                if (loadImageEntities)
                {
                    yield return LoadImageEntityExamples();
                }

                if (loadHTMLEntities)
                {
                    yield return LoadHTMLEntityExamples();
                }

                if (loadLightEntities)
                {
                    yield return LoadLightEntityExamples();
                }
            }
        }

        /// <summary>
        /// Create a demo canvas for UI elements.
        /// </summary>
        private IEnumerator CreateDemoCanvas()
        {
            Logging.Log("[MultiEntityJSONExample] Creating demo canvas for UI elements");
            
            var canvasData = new JSONCanvasEntity
            {
                id = "demo-canvas-multi",
                tag = "demo-canvas",
                canvasType = "screen",
                position = new JSONVector3 { x = 0, y = 0, z = 0 },
                rotation = new JSONQuaternion { x = 0, y = 0, z = 0, w = 1 },
                scale = new JSONVector3 { x = 1, y = 1, z = 1 }
            };

            bool canvasCreated = false;
            jsonEntityHandler.CreateCanvasEntity(canvasData, null, (entityId, entity) =>
            {
                canvasCreated = true;
                if (entityId.HasValue && entity != null)
                {
                    Logging.Log($"[MultiEntityJSONExample] Demo canvas created: {entityId}");
                    createdCanvas = entity;
                    canvasEntity = entity;
                }
            });

            while (!canvasCreated)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Load image entity examples.
        /// </summary>
        private IEnumerator LoadImageEntityExamples()
        {
            Logging.Log("[MultiEntityJSONExample] Loading image entity examples");
            
            string jsonContent = LoadJSONFile(imageExamplePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                Logging.LogError($"[MultiEntityJSONExample] Failed to load image JSON file: {imageExamplePath}");
                yield break;
            }

            bool loadComplete = false;
            jsonEntityHandler.LoadImageEntityFromJSON(jsonContent, canvasEntity, (success, entityId, entity) =>
            {
                loadComplete = true;
                if (success)
                {
                    Logging.Log($"[MultiEntityJSONExample] Image entity loaded successfully: {entityId}");
                }
                else
                {
                    Logging.LogError("[MultiEntityJSONExample] Failed to load image entity");
                }
            });

            while (!loadComplete)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Load HTML entity examples.
        /// </summary>
        private IEnumerator LoadHTMLEntityExamples()
        {
            Logging.Log("[MultiEntityJSONExample] Loading HTML entity examples");

            // Load HTML UI element
            string htmlUIContent = LoadJSONFile(htmlUIExamplePath);
            if (!string.IsNullOrEmpty(htmlUIContent))
            {
                bool uiLoadComplete = false;
                jsonEntityHandler.LoadHTMLEntityFromJSON(htmlUIContent, canvasEntity, (success, entityId, entity) =>
                {
                    uiLoadComplete = true;
                    if (success)
                    {
                        Logging.Log($"[MultiEntityJSONExample] HTML UI entity loaded successfully: {entityId}");
                    }
                    else
                    {
                        Logging.LogError("[MultiEntityJSONExample] Failed to load HTML UI entity");
                    }
                });

                while (!uiLoadComplete)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.5f);

            // Load 3D HTML entity
            string html3DContent = LoadJSONFile(html3DExamplePath);
            if (!string.IsNullOrEmpty(html3DContent))
            {
                bool world3DLoadComplete = false;
                jsonEntityHandler.LoadHTMLEntityFromJSON(html3DContent, worldParentEntity, (success, entityId, entity) =>
                {
                    world3DLoadComplete = true;
                    if (success)
                    {
                        Logging.Log($"[MultiEntityJSONExample] 3D HTML entity loaded successfully: {entityId}");
                    }
                    else
                    {
                        Logging.LogError("[MultiEntityJSONExample] Failed to load 3D HTML entity");
                    }
                });

                while (!world3DLoadComplete)
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Load light entity examples.
        /// </summary>
        private IEnumerator LoadLightEntityExamples()
        {
            Logging.Log("[MultiEntityJSONExample] Loading light entity examples");

            // Load point light
            yield return LoadLightEntity("point light", lightPointExamplePath);
            yield return new WaitForSeconds(0.3f);

            // Load spot light  
            yield return LoadLightEntity("spot light", lightSpotExamplePath);
            yield return new WaitForSeconds(0.3f);

            // Load directional light
            yield return LoadLightEntity("directional light", lightDirectionalExamplePath);
        }

        /// <summary>
        /// Load a specific light entity.
        /// </summary>
        private IEnumerator LoadLightEntity(string lightType, string filePath)
        {
            string jsonContent = LoadJSONFile(filePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                Logging.LogError($"[MultiEntityJSONExample] Failed to load {lightType} JSON file: {filePath}");
                yield break;
            }

            bool loadComplete = false;
            jsonEntityHandler.LoadLightEntityFromJSON(jsonContent, worldParentEntity, (success, entityId, entity) =>
            {
                loadComplete = true;
                if (success)
                {
                    Logging.Log($"[MultiEntityJSONExample] {lightType} loaded successfully: {entityId}");
                }
                else
                {
                    Logging.LogError($"[MultiEntityJSONExample] Failed to load {lightType}");
                }
            });

            while (!loadComplete)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Load the mixed scene example with multiple entity types.
        /// </summary>
        private IEnumerator LoadMixedSceneExample()
        {
            Logging.Log("[MultiEntityJSONExample] Loading mixed scene example");

            string jsonContent = LoadJSONFile(mixedSceneExamplePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                Logging.LogError($"[MultiEntityJSONExample] Failed to load mixed scene JSON: {mixedSceneExamplePath}");
                yield break;
            }

            // Parse the mixed scene JSON manually to handle different entity types
            dynamic sceneData = null;
            try
            {
                sceneData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonContent);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[MultiEntityJSONExample] Error parsing mixed scene: {ex.Message}");
                yield break;
            }
            
            // Load entities (lights, 3D HTML)
            if (sceneData?.entities != null)
            {
                foreach (var entity in sceneData.entities)
                {
                    string entityJson = Newtonsoft.Json.JsonConvert.SerializeObject(entity);
                    string entityType = entity.type;

                    switch (entityType)
                    {
                        case "light":
                            yield return LoadSingleEntityFromJson(entityJson, "light", worldParentEntity, jsonEntityHandler.LoadLightEntityFromJSON);
                            break;
                        case "html":
                            yield return LoadSingleEntityFromJson(entityJson, "3D HTML", worldParentEntity, jsonEntityHandler.LoadHTMLEntityFromJSON);
                            break;
                    }
                    
                    yield return new WaitForSeconds(0.2f);
                }
            }

            // Load UI elements (canvas with children)
            if (sceneData?.ui_elements != null)
            {
                foreach (var uiElement in sceneData.ui_elements)
                {
                    string uiJson = Newtonsoft.Json.JsonConvert.SerializeObject(uiElement);
                    yield return LoadSingleEntityFromJson(uiJson, "UI Canvas", null, jsonEntityHandler.LoadCanvasEntityFromJSON);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        /// <summary>
        /// Load a single entity from JSON string.
        /// </summary>
        private IEnumerator LoadSingleEntityFromJson(string entityJson, string entityType, BaseEntity parentEntity, 
            Action<string, BaseEntity, Action<bool, Guid?, BaseEntity>> loadMethod)
        {
            bool loadComplete = false;
            loadMethod(entityJson, parentEntity, (success, entityId, entity) =>
            {
                loadComplete = true;
                if (success)
                {
                    Logging.Log($"[MultiEntityJSONExample] {entityType} loaded from mixed scene: {entityId}");
                }
                else
                {
                    Logging.LogError($"[MultiEntityJSONExample] Failed to load {entityType} from mixed scene");
                }
            });

            while (!loadComplete)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Load JSON content from file.
        /// </summary>
        private string LoadJSONFile(string filePath)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(Application.dataPath, "..", filePath);
                if (System.IO.File.Exists(fullPath))
                {
                    return System.IO.File.ReadAllText(fullPath);
                }
                else
                {
                    Logging.LogError($"[MultiEntityJSONExample] File not found: {fullPath}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[MultiEntityJSONExample] Error loading file {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// JavaScript callback methods for HTML entities.
        /// </summary>
        public void HandleHTMLMessage(string message)
        {
            Logging.Log($"[MultiEntityJSONExample] HTML UI Message: {message}");
        }

        public void HandleWorldHTMLMessage(string message)
        {
            Logging.Log($"[MultiEntityJSONExample] 3D HTML Message: {message}");
        }

        public void HandlePanelMessage(string message)
        {
            Logging.Log($"[MultiEntityJSONExample] Panel Message: {message}");
            
            // Handle specific panel messages
            switch (message)
            {
                case "info_requested":
                    Logging.Log("[MultiEntityJSONExample] Info panel requested");
                    break;
                case "settings_opened":
                    Logging.Log("[MultiEntityJSONExample] Settings panel opened");
                    break;
            }
        }

        public void HandleStatusMessage(string message)
        {
            Logging.Log($"[MultiEntityJSONExample] Status Widget Message: {message}");
        }

        /// <summary>
        /// Test method to demonstrate programmatic entity creation.
        /// </summary>
        [ContextMenu("Test Programmatic Multi-Entity Creation")]
        public void TestProgrammaticCreation()
        {
            if (jsonEntityHandler == null) return;

            StartCoroutine(CreateProgrammaticEntities());
        }

        /// <summary>
        /// Create entities programmatically to demonstrate API usage.
        /// </summary>
        private IEnumerator CreateProgrammaticEntities()
        {
            // Create a light
            var lightData = new JSONLightEntity
            {
                id = "test-programmatic-light",
                tag = "demo-light",
                position = new JSONVector3 { x = -2.0f, y = 3.0f, z = 2.0f },
                rotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f },
                lightType = "point",
                color = new JSONColor { r = 1.0f, g = 0.7f, b = 0.3f, a = 1.0f },
                intensity = 2.0f,
                range = 8.0f
            };

            jsonEntityHandler.CreateLightEntity(lightData, worldParentEntity, (entityId, entity) =>
            {
                if (entityId.HasValue)
                {
                    Logging.Log($"[MultiEntityJSONExample] Programmatic light created: {entityId}");
                }
            });

            yield return new WaitForSeconds(0.5f);

            // Create an image (requires canvas)
            if (canvasEntity != null)
            {
                var imageData = new JSONImageEntity
                {
                    id = "test-programmatic-image",
                    tag = "demo-image",
                    positionPercent = new JSONVector2 { x = 0.7f, y = 0.2f },
                    sizePercent = new JSONVector2 { x = 0.25f, y = 0.15f },
                    imageFile = "Images/test_image.png",
                    alignment = "center"
                };

                jsonEntityHandler.CreateImageEntity(imageData, canvasEntity, (entityId, entity) =>
                {
                    if (entityId.HasValue)
                    {
                        Logging.Log($"[MultiEntityJSONExample] Programmatic image created: {entityId}");
                    }
                });
            }

            yield return new WaitForSeconds(0.5f);

            // Create 3D HTML entity
            var htmlData = new JSONHTMLEntity
            {
                id = "test-programmatic-html",
                tag = "demo-html",
                position = new JSONVector3 { x = 1.0f, y = 1.5f, z = 1.0f },
                rotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f },
                scale = new JSONVector3 { x = 1.0f, y = 0.7f, z = 0.1f },
                isSize = true,
                isCanvasElement = false,
                html = "<div style='background:#333;color:#fff;padding:15px;text-align:center;font-family:Arial;border-radius:8px;'><h2>Programmatic HTML</h2><p>Created via code!</p></div>",
                onMessage = "HandleWorldHTMLMessage"
            };

            jsonEntityHandler.CreateHTMLEntity(htmlData, worldParentEntity, (entityId, entity) =>
            {
                if (entityId.HasValue)
                {
                    Logging.Log($"[MultiEntityJSONExample] Programmatic HTML created: {entityId}");
                }
            });
        }

        /// <summary>
        /// Cleanup method to destroy created entities.
        /// </summary>
        public void ClearAllEntities()
        {
            if (createdCanvas != null)
            {
                createdCanvas = null;
                Logging.Log("[MultiEntityJSONExample] Demo entities cleared");
            }
        }

        void OnDestroy()
        {
            ClearAllEntities();
        }
    }
}