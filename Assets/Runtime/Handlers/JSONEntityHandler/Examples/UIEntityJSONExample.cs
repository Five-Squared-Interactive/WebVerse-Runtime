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
    /// Example script demonstrating UI entity creation from JSON using JSONEntityHandler.
    /// This script shows how to load and create canvas, button, text, input, and dropdown entities.
    /// </summary>
    public class UIEntityJSONExample : MonoBehaviour
    {
        [Header("JSON Entity Handler")]
        [SerializeField]
        private JSONEntityHandler jsonEntityHandler;

        [Header("Example JSON Files")]
        [SerializeField]
        private string canvasExamplePath = "Files/Examples/UI/canvas_example.json";
        
        [SerializeField]
        private string worldCanvasExamplePath = "Files/Examples/UI/world_canvas_example.json";
        
        [SerializeField]
        private string buttonExamplePath = "Files/Examples/UI/button_example.json";
        
        [SerializeField]
        private string textExamplePath = "Files/Examples/UI/text_example.json";
        
        [SerializeField]
        private string inputExamplePath = "Files/Examples/UI/input_example.json";
        
        [SerializeField]
        private string dropdownExamplePath = "Files/Examples/UI/dropdown_example.json";

        [Header("Demo Controls")]
        [SerializeField]
        private bool loadScreenSpaceCanvas = true;
        
        [SerializeField]
        private bool loadWorldSpaceCanvas = false;
        
        [SerializeField]
        private bool loadIndividualElements = false;

        private BaseEntity currentScreenCanvas;
        private BaseEntity currentWorldCanvas;

        void Start()
        {
            if (jsonEntityHandler == null)
            {
                jsonEntityHandler = FindFirstObjectByType<JSONEntityHandler>();
                if (jsonEntityHandler == null)
                {
                    Logging.LogError("[UIEntityJSONExample] JSONEntityHandler not found in scene");
                    return;
                }
            }

            StartCoroutine(LoadUIExamples());
        }

        /// <summary>
        /// Load all UI examples based on configuration.
        /// </summary>
        private IEnumerator LoadUIExamples()
        {
            yield return new WaitForSeconds(1.0f); // Wait for system initialization

            if (loadScreenSpaceCanvas)
            {
                yield return LoadScreenSpaceCanvasExample();
            }

            if (loadWorldSpaceCanvas)
            {
                yield return LoadWorldSpaceCanvasExample();
            }

            if (loadIndividualElements && currentScreenCanvas != null)
            {
                yield return LoadIndividualElementExamples();
            }
        }

        /// <summary>
        /// Load screen space canvas with nested UI elements.
        /// </summary>
        private IEnumerator LoadScreenSpaceCanvasExample()
        {
            Logging.Log("[UIEntityJSONExample] Loading screen space canvas example");
            
            string jsonContent = LoadJSONFile(canvasExamplePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                Logging.LogError($"[UIEntityJSONExample] Failed to load JSON file: {canvasExamplePath}");
                yield break;
            }

            bool loadComplete = false;
            jsonEntityHandler.LoadCanvasEntityFromJSON(jsonContent, null, (success, entityId, entity) =>
            {
                loadComplete = true;
                if (success)
                {
                    Logging.Log($"[UIEntityJSONExample] Screen space canvas loaded successfully: {entityId}");
                    currentScreenCanvas = entity;
                }
                else
                {
                    Logging.LogError("[UIEntityJSONExample] Failed to load screen space canvas");
                }
            });

            // Wait for completion
            while (!loadComplete)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Load world space canvas example.
        /// </summary>
        private IEnumerator LoadWorldSpaceCanvasExample()
        {
            Logging.Log("[UIEntityJSONExample] Loading world space canvas example");
            
            string jsonContent = LoadJSONFile(worldCanvasExamplePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                Logging.LogError($"[UIEntityJSONExample] Failed to load JSON file: {worldCanvasExamplePath}");
                yield break;
            }

            bool loadComplete = false;
            jsonEntityHandler.LoadCanvasEntityFromJSON(jsonContent, null, (success, entityId, entity) =>
            {
                loadComplete = true;
                if (success)
                {
                    Logging.Log($"[UIEntityJSONExample] World space canvas loaded successfully: {entityId}");
                    currentWorldCanvas = entity;
                }
                else
                {
                    Logging.LogError("[UIEntityJSONExample] Failed to load world space canvas");
                }
            });

            // Wait for completion
            while (!loadComplete)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Load individual UI element examples into existing canvas.
        /// </summary>
        private IEnumerator LoadIndividualElementExamples()
        {
            Logging.Log("[UIEntityJSONExample] Loading individual UI element examples");

            // Load button example
            yield return LoadUIElement("button", buttonExamplePath, currentScreenCanvas, 
                jsonEntityHandler.LoadButtonEntityFromJSON);

            yield return new WaitForSeconds(0.5f);

            // Load text example  
            yield return LoadUIElement("text", textExamplePath, currentScreenCanvas, 
                jsonEntityHandler.LoadTextEntityFromJSON);

            yield return new WaitForSeconds(0.5f);

            // Load input example
            yield return LoadUIElement("input", inputExamplePath, currentScreenCanvas, 
                jsonEntityHandler.LoadInputEntityFromJSON);

            yield return new WaitForSeconds(0.5f);

            // Load dropdown example
            yield return LoadUIElement("dropdown", dropdownExamplePath, currentScreenCanvas, 
                jsonEntityHandler.LoadDropdownEntityFromJSON);
        }

        /// <summary>
        /// Generic method to load a UI element.
        /// </summary>
        private IEnumerator LoadUIElement(string elementType, string filePath, BaseEntity parentEntity, 
            Action<string, BaseEntity, Action<bool, Guid?, BaseEntity>> loadMethod)
        {
            string jsonContent = LoadJSONFile(filePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                Logging.LogError($"[UIEntityJSONExample] Failed to load {elementType} JSON file: {filePath}");
                yield break;
            }

            bool loadComplete = false;
            loadMethod(jsonContent, parentEntity, (success, entityId, entity) =>
            {
                loadComplete = true;
                if (success)
                {
                    Logging.Log($"[UIEntityJSONExample] {elementType} loaded successfully: {entityId}");
                }
                else
                {
                    Logging.LogError($"[UIEntityJSONExample] Failed to load {elementType}");
                }
            });

            // Wait for completion
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
                    Logging.LogError($"[UIEntityJSONExample] File not found: {fullPath}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[UIEntityJSONExample] Error loading file {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Public methods that can be called from JavaScript callbacks.
        /// </summary>
        public void OnMainButtonClick()
        {
            Logging.Log("[UIEntityJSONExample] Main button clicked!");
        }

        public void OnDropdownChange(int selectedIndex)
        {
            Logging.Log($"[UIEntityJSONExample] Dropdown selection changed to index: {selectedIndex}");
        }

        public void OnWorldInteract()
        {
            Logging.Log("[UIEntityJSONExample] World interact button clicked!");
        }

        public void OnQualityChange(int selectedIndex)
        {
            string[] qualities = { "Low Quality", "Medium Quality", "High Quality", "Ultra Quality" };
            if (selectedIndex >= 0 && selectedIndex < qualities.Length)
            {
                Logging.Log($"[UIEntityJSONExample] Quality changed to: {qualities[selectedIndex]}");
            }
        }

        /// <summary>
        /// Cleanup method to destroy created entities.
        /// </summary>
        public void ClearAllUIElements()
        {
            if (currentScreenCanvas != null)
            {
                // Entity cleanup would be handled by the entity manager
                currentScreenCanvas = null;
                Logging.Log("[UIEntityJSONExample] Screen canvas cleared");
            }

            if (currentWorldCanvas != null)
            {
                // Entity cleanup would be handled by the entity manager  
                currentWorldCanvas = null;
                Logging.Log("[UIEntityJSONExample] World canvas cleared");
            }
        }

        void OnDestroy()
        {
            ClearAllUIElements();
        }

        /// <summary>
        /// Test method to demonstrate programmatic entity creation.
        /// </summary>
        [ContextMenu("Test Programmatic UI Creation")]
        public void TestProgrammaticCreation()
        {
            if (jsonEntityHandler == null) return;

            // Create a simple canvas programmatically
            var canvasData = new JSONCanvasEntity
            {
                id = "test-programmatic-canvas",
                tag = "test-canvas",
                canvasType = "screen",
                position = new JSONVector3 { x = 0, y = 0, z = 0 },
                rotation = new JSONQuaternion { x = 0, y = 0, z = 0, w = 1 },
                scale = new JSONVector3 { x = 1, y = 1, z = 1 }
            };

            jsonEntityHandler.CreateCanvasEntity(canvasData, null, (entityId, entity) =>
            {
                if (entityId.HasValue && entity != null)
                {
                    Logging.Log($"[UIEntityJSONExample] Programmatic canvas created: {entityId}");
                    
                    // Create a button in the new canvas
                    var buttonData = new JSONButtonEntity
                    {
                        id = "test-programmatic-button",
                        tag = "test-button",
                        text = "Programmatic Button",
                        positionPercent = new JSONVector2 { x = 0.4f, y = 0.4f },
                        sizePercent = new JSONVector2 { x = 0.2f, y = 0.1f },
                        onClick = "OnMainButtonClick",
                        backgroundColor = new JSONColor { r = 1.0f, g = 0.5f, b = 0.0f, a = 1.0f }
                    };

                    jsonEntityHandler.CreateButtonEntity(buttonData, entity, (buttonId, buttonEntity) =>
                    {
                        if (buttonId.HasValue)
                        {
                            Logging.Log($"[UIEntityJSONExample] Programmatic button created: {buttonId}");
                        }
                    });
                }
            });
        }
    }
}