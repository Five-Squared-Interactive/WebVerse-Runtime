using System;
using UnityEngine;
using FiveSQD.WebVerse.Handlers.JSONEntity;

namespace FiveSQD.WebVerse.Examples
{
    /// <summary>
    /// Example demonstrating character entity creation using JSONEntityHandler.
    /// Shows various character configurations and usage patterns.
    /// </summary>
    public class CharacterEntityJSONExample : MonoBehaviour
    {
        [Header("JSON Entity Handler")]
        public JSONEntityHandler jsonEntityHandler;
        
        [Header("Example Characters")]
        [TextArea(10, 15)]
        public string basicCharacterJSON = @"{
  ""type"": ""character"",
  ""id"": ""550e8400-e29b-41d4-a716-446655440030"",
  ""tag"": ""ExampleBasicCharacter"",
  ""position"": { ""x"": 0, ""y"": 0, ""z"": 0 },
  ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
  ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
  ""meshObject"": """",
  ""fixHeight"": true
}";

        [TextArea(10, 15)]
        public string animatedCharacterJSON = @"{
  ""type"": ""character"",
  ""id"": ""550e8400-e29b-41d4-a716-446655440031"",
  ""tag"": ""ExampleAnimatedCharacter"",
  ""position"": { ""x"": 5, ""y"": 0, ""z"": 0 },
  ""rotation"": { ""x"": 0, ""y"": 45, ""z"": 0, ""w"": 0.7071 },
  ""meshObject"": ""Characters/Hero/hero.gltf"",
  ""meshResources"": [
    ""Characters/Hero/hero.gltf"",
    ""Characters/Hero/textures/hero_diffuse.png"",
    ""Characters/Hero/textures/hero_normal.png""
  ],
  ""meshOffset"": { ""x"": 0, ""y"": -0.3, ""z"": 0 },
  ""avatarLabelOffset"": { ""x"": 0, ""y"": 2.2, ""z"": 0 },
  ""fixHeight"": true
}";

        [TextArea(10, 15)]
        public string npcCharacterJSON = @"{
  ""type"": ""character"",
  ""id"": ""550e8400-e29b-41d4-a716-446655440032"",
  ""tag"": ""ExampleNPCCharacter"",
  ""position"": { ""x"": -3, ""y"": 0, ""z"": 5 },
  ""rotation"": { ""x"": 0, ""y"": 180, ""z"": 0, ""w"": 0 },
  ""scale"": { ""x"": 1.1, ""y"": 1.1, ""z"": 1.1 },
  ""meshObject"": ""Characters/NPC/shopkeeper.gltf"",
  ""meshResources"": [
    ""Characters/NPC/shopkeeper.gltf"",
    ""Characters/NPC/textures/clothes.png""
  ],
  ""avatarLabelOffset"": { ""x"": 0, ""y"": 2.4, ""z"": 0 },
  ""fixHeight"": true,
  ""checkForUpdateIfCached"": false
}";

        private void Start()
        {
            // Find JSONEntityHandler if not assigned
            if (jsonEntityHandler == null)
            {
                jsonEntityHandler = FindFirstObjectByType<JSONEntityHandler>();
            }

            if (jsonEntityHandler == null)
            {
                Debug.LogError("[CharacterEntityJSONExample] JSONEntityHandler not found!");
                return;
            }

            // Wait a moment for initialization then create examples
            Invoke(nameof(CreateExampleCharacters), 2f);
        }

        /// <summary>
        /// Create example character entities demonstrating different configurations.
        /// </summary>
        private void CreateExampleCharacters()
        {
            if (!jsonEntityHandler.IsReady())
            {
                Debug.LogError("[CharacterEntityJSONExample] JSONEntityHandler not ready!");
                return;
            }

            Debug.Log("[CharacterEntityJSONExample] Creating example character entities...");

            // Create basic character (no custom mesh)
            CreateBasicCharacter();

            // Create animated character (with custom mesh)
            CreateAnimatedCharacter();

            // Create NPC character (scaled and positioned)
            CreateNPCCharacter();

            // Create character from file (if available)
            CreateCharacterFromFile();
        }

        /// <summary>
        /// Create a basic character entity without custom mesh.
        /// </summary>
        private void CreateBasicCharacter()
        {
            Debug.Log("[CharacterEntityJSONExample] Creating basic character...");

            jsonEntityHandler.LoadCharacterEntityFromJSON(
                basicCharacterJSON,
                null,
                (success, entityId, createdEntity) =>
                {
                    if (success && entityId.HasValue)
                    {
                        Debug.Log($"[CharacterEntityJSONExample] Basic character created successfully: {entityId}");
                        
                        // Demonstrate character movement
                        StartCoroutine(DemonstrateCharacterMovement(createdEntity));
                    }
                    else
                    {
                        Debug.LogError("[CharacterEntityJSONExample] Failed to create basic character");
                    }
                }
            );
        }

        /// <summary>
        /// Create an animated character entity with custom mesh.
        /// </summary>
        private void CreateAnimatedCharacter()
        {
            Debug.Log("[CharacterEntityJSONExample] Creating animated character...");

            jsonEntityHandler.LoadCharacterEntityFromJSON(
                animatedCharacterJSON,
                null,
                (success, entityId, createdEntity) =>
                {
                    if (success && entityId.HasValue)
                    {
                        Debug.Log($"[CharacterEntityJSONExample] Animated character created successfully: {entityId}");
                        
                        // Log character properties
                        LogCharacterProperties(createdEntity, "Animated Character");
                    }
                    else
                    {
                        Debug.LogError("[CharacterEntityJSONExample] Failed to create animated character");
                    }
                }
            );
        }

        /// <summary>
        /// Create an NPC character entity with custom properties.
        /// </summary>
        private void CreateNPCCharacter()
        {
            Debug.Log("[CharacterEntityJSONExample] Creating NPC character...");

            jsonEntityHandler.LoadCharacterEntityFromJSON(
                npcCharacterJSON,
                null,
                (success, entityId, createdEntity) =>
                {
                    if (success && entityId.HasValue)
                    {
                        Debug.Log($"[CharacterEntityJSONExample] NPC character created successfully: {entityId}");
                        
                        // Demonstrate character properties access
                        LogCharacterProperties(createdEntity, "NPC Character");
                    }
                    else
                    {
                        Debug.LogError("[CharacterEntityJSONExample] Failed to create NPC character");
                    }
                }
            );
        }

        /// <summary>
        /// Create character from an external JSON file.
        /// </summary>
        private void CreateCharacterFromFile()
        {
            string filePath = "Characters/Examples/hero_character.json";
            
            Debug.Log($"[CharacterEntityJSONExample] Attempting to load character from file: {filePath}");

            jsonEntityHandler.LoadContainerEntityFromFile(
                filePath,
                null,
                (success, entityId, createdEntity) =>
                {
                    if (success && entityId.HasValue)
                    {
                        Debug.Log($"[CharacterEntityJSONExample] Character loaded from file successfully: {entityId}");
                    }
                    else
                    {
                        Debug.LogWarning($"[CharacterEntityJSONExample] Could not load character from file: {filePath}");
                    }
                }
            );
        }

        /// <summary>
        /// Demonstrate character movement capabilities.
        /// </summary>
        private System.Collections.IEnumerator DemonstrateCharacterMovement(FiveSQD.StraightFour.Entity.BaseEntity characterEntity)
        {
            if (characterEntity == null)
            {
                yield break;
            }

            Debug.Log("[CharacterEntityJSONExample] Demonstrating character movement...");

            // Get character entity wrapper
            var charEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(characterEntity) as FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CharacterEntity;
            
            if (charEntity != null)
            {
                // Wait a moment
                yield return new WaitForSeconds(2f);

                // Move forward
                Debug.Log("[CharacterEntityJSONExample] Moving character forward...");
                charEntity.Move(new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(0, 0, 2));
                
                yield return new WaitForSeconds(2f);

                // Move right
                Debug.Log("[CharacterEntityJSONExample] Moving character right...");
                charEntity.Move(new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(2, 0, 0));
                
                yield return new WaitForSeconds(2f);

                // Jump
                Debug.Log("[CharacterEntityJSONExample] Making character jump...");
                charEntity.Jump(5f);
                
                yield return new WaitForSeconds(1f);

                // Check if on surface
                bool onSurface = charEntity.IsOnSurface();
                Debug.Log($"[CharacterEntityJSONExample] Character on surface: {onSurface}");
            }
        }

        /// <summary>
        /// Log character entity properties for debugging.
        /// </summary>
        private void LogCharacterProperties(FiveSQD.StraightFour.Entity.BaseEntity characterEntity, string characterName)
        {
            if (characterEntity == null)
                return;

            Debug.Log($"[CharacterEntityJSONExample] {characterName} Properties:");
            Debug.Log($"  - ID: {characterEntity.id}");
            Debug.Log($"  - Tag: {characterEntity.entityTag}");
            Debug.Log($"  - Position: {characterEntity.transform.position}");
            Debug.Log($"  - Rotation: {characterEntity.transform.rotation}");
            Debug.Log($"  - Scale: {characterEntity.transform.localScale}");

            // Get character entity wrapper for additional properties
            var charEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(characterEntity) as FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CharacterEntity;
            
            if (charEntity != null)
            {
                Debug.Log($"  - Fix Height: {charEntity.fixHeight}");
                Debug.Log($"  - On Surface: {charEntity.IsOnSurface()}");
            }
        }

        /// <summary>
        /// Create multiple characters in a formation.
        /// </summary>
        [ContextMenu("Create Character Formation")]
        public void CreateCharacterFormation()
        {
            if (jsonEntityHandler == null || !jsonEntityHandler.IsReady())
            {
                Debug.LogError("[CharacterEntityJSONExample] JSONEntityHandler not ready for formation creation!");
                return;
            }

            int characterCount = 5;
            float spacing = 3f;

            for (int i = 0; i < characterCount; i++)
            {
                // Create a modified character JSON with unique position and ID
                var formationCharacterData = new
                {
                    type = "character",
                    id = Guid.NewGuid().ToString(),
                    tag = $"FormationCharacter_{i}",
                    position = new { x = i * spacing, y = 0, z = 10 },
                    rotation = new { x = 0, y = 0, z = 0, w = 1 },
                    scale = new { x = 1, y = 1, z = 1 },
                    meshObject = "",
                    fixHeight = true
                };

                string characterJSON = Newtonsoft.Json.JsonConvert.SerializeObject(formationCharacterData, Newtonsoft.Json.Formatting.Indented);

                jsonEntityHandler.LoadCharacterEntityFromJSON(
                    characterJSON,
                    null,
                    (success, entityId, createdEntity) =>
                    {
                        if (success)
                        {
                            Debug.Log($"[CharacterEntityJSONExample] Formation character {i} created: {entityId}");
                        }
                        else
                        {
                            Debug.LogError($"[CharacterEntityJSONExample] Failed to create formation character {i}");
                        }
                    }
                );
            }

            Debug.Log($"[CharacterEntityJSONExample] Creating formation of {characterCount} characters...");
        }

        /// <summary>
        /// Cleanup method for testing.
        /// </summary>
        [ContextMenu("Clear All Characters")]
        public void ClearAllCharacters()
        {
            // Note: In a real implementation, you would track created entities
            // and destroy them properly. This is just for demonstration.
            Debug.Log("[CharacterEntityJSONExample] Character cleanup would happen here");
        }
    }
}