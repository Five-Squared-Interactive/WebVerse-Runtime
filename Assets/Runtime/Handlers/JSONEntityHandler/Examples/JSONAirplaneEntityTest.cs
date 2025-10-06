// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.WebVerse.Handlers.JSONEntity;

namespace FiveSQD.WebVerse.Examples
{
    /// <summary>
    /// Example demonstrating airplane entity creation from JSON data.
    /// </summary>
    public class JSONAirplaneEntityTest : MonoBehaviour
    {
        [Header("JSON Configuration")]
        [TextArea(10, 30)]
        public string airplaneEntityJSON = @"{
  ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
  ""tag"": ""PlayerAirplane"",
  ""position"": { ""x"": 0, ""y"": 10, ""z"": 0 },
  ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
  ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
  ""isSize"": false,
  ""meshObject"": ""https://example.com/models/cessna172.gltf"",
  ""meshResources"": [
    ""https://example.com/models/cessna172_texture.png"",
    ""https://example.com/models/cessna172_normal.png""
  ],
  ""mass"": 750.0,
  ""throttle"": 0.0,
  ""pitch"": 0.0,
  ""roll"": 0.0,
  ""yaw"": 0.0,
  ""checkForUpdateIfCached"": true,
  ""children"": []
}";

        [Header("Runtime Controls")]
        public bool createAirplaneOnStart = false;
        
        private JSONEntityHandler jsonHandler;
        private FiveSQD.StraightFour.Entity.BaseEntity createdEntity;

        void Start()
        {
            // Get the JSONEntityHandler
            jsonHandler = FindFirstObjectByType<JSONEntityHandler>();
            
            if (jsonHandler == null)
            {
                Debug.LogError("[JSONAirplaneEntityTest] JSONEntityHandler not found!");
                return;
            }

            if (createAirplaneOnStart)
            {
                CreateAirplaneFromJSON();
            }
        }

        /// <summary>
        /// Create an airplane entity from the JSON configuration.
        /// </summary>
        [ContextMenu("Create Airplane from JSON")]
        public void CreateAirplaneFromJSON()
        {
            if (jsonHandler == null)
            {
                Debug.LogError("[JSONAirplaneEntityTest] JSONEntityHandler not available");
                return;
            }

            Debug.Log("[JSONAirplaneEntityTest] Creating airplane entity from JSON...");

            jsonHandler.LoadAirplaneEntityFromJSON(airplaneEntityJSON, null, (success, entityId, entity) =>
            {
                if (success && entity != null)
                {
                    createdEntity = entity;
                    Debug.Log($"[JSONAirplaneEntityTest] Successfully created airplane entity with ID: {entityId}");
                    
                    // Demonstrate runtime property access
                    if (entity is FiveSQD.StraightFour.Entity.AirplaneEntity airplaneEntity)
                    {
                        Debug.Log($"[JSONAirplaneEntityTest] Airplane properties - Throttle: {airplaneEntity.throttle}");
                    }
                }
                else
                {
                    Debug.LogError("[JSONAirplaneEntityTest] Failed to create airplane entity");
                }
            });
        }

        /// <summary>
        /// Test airplane controls - increase throttle.
        /// </summary>
        [ContextMenu("Increase Throttle")]
        public void IncreaseThrottle()
        {
            if (createdEntity is FiveSQD.StraightFour.Entity.AirplaneEntity airplane)
            {
                airplane.throttle = Mathf.Clamp01(airplane.throttle + 0.1f);
                Debug.Log($"[JSONAirplaneEntityTest] Throttle increased to: {airplane.throttle}");
            }
            else
            {
                Debug.LogWarning("[JSONAirplaneEntityTest] No airplane entity available");
            }
        }

        /// <summary>
        /// Test airplane controls - decrease throttle.
        /// </summary>
        [ContextMenu("Decrease Throttle")]
        public void DecreaseThrottle()
        {
            if (createdEntity is FiveSQD.StraightFour.Entity.AirplaneEntity airplane)
            {
                airplane.throttle = Mathf.Clamp01(airplane.throttle - 0.1f);
                Debug.Log($"[JSONAirplaneEntityTest] Throttle decreased to: {airplane.throttle}");
            }
            else
            {
                Debug.LogWarning("[JSONAirplaneEntityTest] No airplane entity available");
            }
        }

        /// <summary>
        /// Delete the created airplane entity.
        /// </summary>
        [ContextMenu("Delete Airplane")]
        public void DeleteAirplane()
        {
            if (createdEntity != null)
            {
                GameObject.Destroy(createdEntity.gameObject);
                createdEntity = null;
                Debug.Log("[JSONAirplaneEntityTest] Airplane entity deleted");
            }
            else
            {
                Debug.LogWarning("[JSONAirplaneEntityTest] No airplane entity to delete");
            }
        }

        void OnGUI()
        {
            if (jsonHandler == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("JSON Airplane Entity Test", GUI.skin.box);
            
            if (GUILayout.Button("Create Airplane"))
            {
                CreateAirplaneFromJSON();
            }
            
            if (createdEntity != null)
            {
                if (GUILayout.Button("Increase Throttle"))
                {
                    IncreaseThrottle();
                }
                
                if (GUILayout.Button("Decrease Throttle"))
                {
                    DecreaseThrottle();
                }
                
                if (GUILayout.Button("Delete Airplane"))
                {
                    DeleteAirplane();
                }
                
                if (createdEntity is FiveSQD.StraightFour.Entity.AirplaneEntity airplane)
                {
                    GUILayout.Label($"Throttle: {airplane.throttle:F2}");
                    GUILayout.Label($"Position: {airplane.GetPosition(false)}");
                }
            }
            
            GUILayout.EndArea();
        }
    }
}