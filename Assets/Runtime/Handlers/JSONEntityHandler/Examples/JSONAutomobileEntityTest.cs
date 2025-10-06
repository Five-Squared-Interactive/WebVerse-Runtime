// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.WebVerse.Handlers.JSONEntity;

namespace FiveSQD.WebVerse.Examples
{
    /// <summary>
    /// Example demonstrating automobile entity creation from JSON data.
    /// </summary>
    public class JSONAutomobileEntityTest : MonoBehaviour
    {
        [Header("JSON Configuration")]
        [TextArea(15, 35)]
        public string automobileEntityJSON = @"{
  ""id"": ""b2c3d4e5-f6a7-8901-bcde-f23456789012"",
  ""tag"": ""PlayerCar"",
  ""position"": { ""x"": 5, ""y"": 0, ""z"": 0 },
  ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
  ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
  ""isSize"": false,
  ""meshObject"": ""https://example.com/models/sports_car.gltf"",
  ""meshResources"": [
    ""https://example.com/models/sports_car_diffuse.png"",
    ""https://example.com/models/sports_car_normal.png""
  ],
  ""wheels"": [
    { ""wheelSubMesh"": ""FrontLeft"", ""wheelRadius"": 0.35 },
    { ""wheelSubMesh"": ""FrontRight"", ""wheelRadius"": 0.35 },
    { ""wheelSubMesh"": ""RearLeft"", ""wheelRadius"": 0.35 },
    { ""wheelSubMesh"": ""RearRight"", ""wheelRadius"": 0.35 }
  ],
  ""mass"": 1200.0,
  ""automobileType"": ""Default"",
  ""throttle"": 0.0,
  ""steer"": 0.0,
  ""brake"": 0.0,
  ""handBrake"": 0.0,
  ""horn"": false,
  ""gear"": 1,
  ""engineStartStop"": false,
  ""checkForUpdateIfCached"": true,
  ""children"": []
}";

        [Header("Runtime Controls")]
        public bool createAutomobileOnStart = false;
        
        private JSONEntityHandler jsonHandler;
        private FiveSQD.StraightFour.Entity.BaseEntity createdEntity;

        void Start()
        {
            // Get the JSONEntityHandler
            jsonHandler = FindFirstObjectByType<JSONEntityHandler>();
            
            if (jsonHandler == null)
            {
                Debug.LogError("[JSONAutomobileEntityTest] JSONEntityHandler not found!");
                return;
            }

            if (createAutomobileOnStart)
            {
                CreateAutomobileFromJSON();
            }
        }

        /// <summary>
        /// Create an automobile entity from the JSON configuration.
        /// </summary>
        [ContextMenu("Create Automobile from JSON")]
        public void CreateAutomobileFromJSON()
        {
            if (jsonHandler == null)
            {
                Debug.LogError("[JSONAutomobileEntityTest] JSONEntityHandler not available");
                return;
            }

            Debug.Log("[JSONAutomobileEntityTest] Creating automobile entity from JSON...");

            jsonHandler.LoadAutomobileEntityFromJSON(automobileEntityJSON, null, (success, entityId, entity) =>
            {
                if (success && entity != null)
                {
                    createdEntity = entity;
                    Debug.Log($"[JSONAutomobileEntityTest] Successfully created automobile entity with ID: {entityId}");
                    
                    // Demonstrate runtime property access
                    if (entity is FiveSQD.StraightFour.Entity.AutomobileEntity automobileEntity)
                    {
                        Debug.Log($"[JSONAutomobileEntityTest] Automobile properties - Throttle: {automobileEntity.throttle}");
                        Debug.Log($"[JSONAutomobileEntityTest] Gear: {automobileEntity.gear}, Brake: {automobileEntity.brake}");
                    }
                }
                else
                {
                    Debug.LogError("[JSONAutomobileEntityTest] Failed to create automobile entity");
                }
            });
        }

        /// <summary>
        /// Test automobile controls - accelerate.
        /// </summary>
        [ContextMenu("Accelerate")]
        public void Accelerate()
        {
            if (createdEntity is FiveSQD.StraightFour.Entity.AutomobileEntity automobile)
            {
                automobile.throttle = Mathf.Clamp01(automobile.throttle + 0.1f);
                Debug.Log($"[JSONAutomobileEntityTest] Throttle increased to: {automobile.throttle}");
            }
            else
            {
                Debug.LogWarning("[JSONAutomobileEntityTest] No automobile entity available");
            }
        }

        /// <summary>
        /// Test automobile controls - brake.
        /// </summary>
        [ContextMenu("Brake")]
        public void ApplyBrake()
        {
            if (createdEntity is FiveSQD.StraightFour.Entity.AutomobileEntity automobile)
            {
                automobile.brake = Mathf.Clamp01(automobile.brake + 0.2f);
                automobile.throttle = 0f; // Release throttle when braking
                Debug.Log($"[JSONAutomobileEntityTest] Brake applied: {automobile.brake}");
            }
            else
            {
                Debug.LogWarning("[JSONAutomobileEntityTest] No automobile entity available");
            }
        }

        /// <summary>
        /// Test automobile controls - steer left.
        /// </summary>
        [ContextMenu("Steer Left")]
        public void SteerLeft()
        {
            if (createdEntity is FiveSQD.StraightFour.Entity.AutomobileEntity automobile)
            {
                automobile.steer = Mathf.Clamp(automobile.steer - 0.1f, -1f, 1f);
                Debug.Log($"[JSONAutomobileEntityTest] Steering: {automobile.steer}");
            }
            else
            {
                Debug.LogWarning("[JSONAutomobileEntityTest] No automobile entity available");
            }
        }

        /// <summary>
        /// Test automobile controls - steer right.
        /// </summary>
        [ContextMenu("Steer Right")]
        public void SteerRight()
        {
            if (createdEntity is FiveSQD.StraightFour.Entity.AutomobileEntity automobile)
            {
                automobile.steer = Mathf.Clamp(automobile.steer + 0.1f, -1f, 1f);
                Debug.Log($"[JSONAutomobileEntityTest] Steering: {automobile.steer}");
            }
            else
            {
                Debug.LogWarning("[JSONAutomobileEntityTest] No automobile entity available");
            }
        }

        /// <summary>
        /// Test automobile controls - toggle horn.
        /// </summary>
        [ContextMenu("Toggle Horn")]
        public void ToggleHorn()
        {
            if (createdEntity is FiveSQD.StraightFour.Entity.AutomobileEntity automobile)
            {
                automobile.horn = !automobile.horn;
                Debug.Log($"[JSONAutomobileEntityTest] Horn: {(automobile.horn ? "ON" : "OFF")}");
            }
            else
            {
                Debug.LogWarning("[JSONAutomobileEntityTest] No automobile entity available");
            }
        }

        /// <summary>
        /// Delete the created automobile entity.
        /// </summary>
        [ContextMenu("Delete Automobile")]
        public void DeleteAutomobile()
        {
            if (createdEntity != null)
            {
                GameObject.Destroy(createdEntity.gameObject);
                createdEntity = null;
                Debug.Log("[JSONAutomobileEntityTest] Automobile entity deleted");
            }
            else
            {
                Debug.LogWarning("[JSONAutomobileEntityTest] No automobile entity to delete");
            }
        }

        void OnGUI()
        {
            if (jsonHandler == null) return;

            GUILayout.BeginArea(new Rect(320, 10, 300, 250));
            GUILayout.Label("JSON Automobile Entity Test", GUI.skin.box);
            
            if (GUILayout.Button("Create Automobile"))
            {
                CreateAutomobileFromJSON();
            }
            
            if (createdEntity != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Accelerate"))
                {
                    Accelerate();
                }
                if (GUILayout.Button("Brake"))
                {
                    ApplyBrake();
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Steer Left"))
                {
                    SteerLeft();
                }
                if (GUILayout.Button("Steer Right"))
                {
                    SteerRight();
                }
                GUILayout.EndHorizontal();
                
                if (GUILayout.Button("Toggle Horn"))
                {
                    ToggleHorn();
                }
                
                if (GUILayout.Button("Delete Automobile"))
                {
                    DeleteAutomobile();
                }
                
                if (createdEntity is FiveSQD.StraightFour.Entity.AutomobileEntity automobile)
                {
                    GUILayout.Label($"Throttle: {automobile.throttle:F2}");
                    GUILayout.Label($"Brake: {automobile.brake:F2}");
                    GUILayout.Label($"Steering: {automobile.steer:F2}");
                    GUILayout.Label($"Horn: {(automobile.horn ? "ON" : "OFF")}");
                    GUILayout.Label($"Position: {automobile.GetPosition(false)}");
                }
            }
            
            GUILayout.EndArea();
        }
    }
}