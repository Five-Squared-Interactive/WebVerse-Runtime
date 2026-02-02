// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System;
using System.Collections.Generic;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Utilities;
using OMI;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour
{
    /// <summary>
    /// Factory for creating appropriate StraightFour entities from glTF nodes with OMI extensions.
    /// Analyzes OMI extension data to determine correct entity type.
    /// </summary>
    public static class StraightFourEntityFactory
    {
        /// <summary>
        /// Analyzes a glTF node and creates the appropriate StraightFour entity.
        /// </summary>
        /// <param name="gameObject">The GameObject for this node.</param>
        /// <param name="nodeIndex">The glTF node index.</param>
        /// <param name="context">Import context with extension data.</param>
        /// <param name="parent">Optional parent entity.</param>
        /// <returns>Created entity, or null on failure.</returns>
        public static BaseEntity CreateEntityFromNode(
            GameObject gameObject,
            int nodeIndex,
            OMIImportContext context,
            BaseEntity parent = null)
        {
            if (gameObject == null)
            {
                Logging.LogError("[EntityFactory] GameObject is null");
                return null;
            }

            Guid entityId = Guid.NewGuid();

            // Analyze extensions to determine entity type
            var entityType = DetermineEntityType(nodeIndex, context, gameObject);

            BaseEntity entity = null;

            switch (entityType)
            {
                case EntityType.Automobile:
                    entity = CreateAutomobileEntity(gameObject, entityId, parent, context);
                    break;

                case EntityType.Airplane:
                    entity = CreateAirplaneEntity(gameObject, entityId, parent, context);
                    break;

                case EntityType.Character:
                    entity = CreateCharacterEntity(gameObject, entityId, parent, context);
                    break;

                case EntityType.Audio:
                    entity = CreateAudioEntity(gameObject, entityId, parent, context);
                    break;

                case EntityType.Mesh:
                    entity = CreateMeshEntity(gameObject, entityId, parent, context);
                    break;

                case EntityType.Container:
                    entity = CreateContainerEntity(gameObject, entityId, parent, context);
                    break;

                default:
                    Logging.LogWarning($"[EntityFactory] Unknown entity type for node {nodeIndex}");
                    entity = CreateContainerEntity(gameObject, entityId, parent, context);
                    break;
            }

            if (entity != null)
            {
                entity.entityTag = entityId.ToString();

                // Register entity with EntityManager so it's tracked for JavaScript API access
                if (FiveSQD.StraightFour.StraightFour.ActiveWorld?.entityManager != null)
                {
                    FiveSQD.StraightFour.StraightFour.ActiveWorld.entityManager.RegisterEntity(entity, entityId);
                }

                // Create JavaScript wrapper and register mapping for JavaScript API access
                CreateAndMapJavaScriptWrapper(entity, entityType);

                Logging.Log($"[EntityFactory] Created {entityType} entity for node {nodeIndex}: {gameObject.name}");
            }

            return entity;
        }

        /// <summary>
        /// Entity types that can be created from OMI data.
        /// </summary>
        private enum EntityType
        {
            Container,
            Mesh,
            Automobile,
            Airplane,
            Character,
            Audio
        }

        /// <summary>
        /// Analyzes OMI extensions to determine the correct entity type.
        /// </summary>
        private static EntityType DetermineEntityType(
            int nodeIndex,
            OMIImportContext context,
            GameObject gameObject)
        {
            // Check for vehicle body extension
            if (HasExtension(context, nodeIndex, "OMI_vehicle_body"))
            {
                // Distinguish between ground vehicle and aircraft
                if (IsAircraft(nodeIndex, context))
                {
                    return EntityType.Airplane;
                }
                else
                {
                    return EntityType.Automobile;
                }
            }

            // Check for character/personality
            if (HasExtension(context, nodeIndex, "OMI_personality") ||
                HasCharacterController(gameObject))
            {
                return EntityType.Character;
            }

            // Check for audio emitter
            if (HasExtension(context, nodeIndex, "KHR_audio_emitter") ||
                HasExtension(context, nodeIndex, "OMI_audio_emitter"))
            {
                return EntityType.Audio;
            }

            // Check if it has a mesh
            if (HasMesh(gameObject))
            {
                return EntityType.Mesh;
            }

            // Default to container for empty nodes
            return EntityType.Container;
        }

        /// <summary>
        /// Determines if a vehicle node represents an aircraft.
        /// Aircraft are identified by: vertical thrusters OR (vehicle body with no wheels).
        /// </summary>
        private static bool IsAircraft(int nodeIndex, OMIImportContext context)
        {
            bool hasVerticalThrusters = HasVerticalThrusters(nodeIndex, context);
            bool hasWheels = HasWheels(nodeIndex, context);

            // Aircraft if:
            // 1. Has vertical thrusters (VTOL, helicopter, rocket), OR
            // 2. Has vehicle_body but no wheels (glider, fixed-wing without landing gear)
            return hasVerticalThrusters || (HasExtension(context, nodeIndex, "OMI_vehicle_body") && !hasWheels);
        }

        /// <summary>
        /// Checks if node has vertical-oriented thrusters.
        /// Vertical thrusters have their thrust direction (local +Z) pointing mostly up/down (Y-axis).
        /// </summary>
        private static bool HasVerticalThrusters(int nodeIndex, OMIImportContext context)
        {
            // Find all child nodes with OMI_vehicle_thruster extension
            var thrusterNodeIndices = OMIExtensionDetector.FindChildNodesWithExtension(
                context, nodeIndex, "OMI_vehicle_thruster");

            if (thrusterNodeIndices.Count == 0)
            {
                return false;
            }

            // Get the glTF JSON to check node orientations
            if (!context.CustomData.TryGetValue("SF_GltfJson", out var jsonObj))
            {
                Logging.LogWarning("[EntityFactory] No glTF JSON found for thruster orientation check");
                return false;
            }

            var root = jsonObj as JObject;
            if (root == null)
            {
                return false;
            }

            var nodes = root["nodes"] as JArray;
            if (nodes == null)
            {
                return false;
            }

            // Check if any thruster has vertical orientation
            // Thrust direction is local +Z axis (forward in Unity)
            // Vertical means the Z axis is aligned with world Y axis (up/down)
            foreach (int thrusterIndex in thrusterNodeIndices)
            {
                if (thrusterIndex < 0 || thrusterIndex >= nodes.Count)
                {
                    continue;
                }

                var thrusterNode = nodes[thrusterIndex] as JObject;
                if (thrusterNode == null)
                {
                    continue;
                }

                // Get node rotation (quaternion: [x, y, z, w])
                var rotation = thrusterNode["rotation"] as JArray;
                Quaternion nodeRotation = Quaternion.identity;

                if (rotation != null && rotation.Count == 4)
                {
                    // glTF quaternions are [x, y, z, w]
                    nodeRotation = new Quaternion(
                        rotation[0].Value<float>(),
                        rotation[1].Value<float>(),
                        rotation[2].Value<float>(),
                        rotation[3].Value<float>()
                    );
                }

                // Transform local forward (0, 0, 1) by rotation to get thrust direction
                Vector3 thrustDirection = nodeRotation * Vector3.forward;

                // Check if thrust direction is mostly vertical (aligned with Y-axis)
                // Use a threshold of 0.7 (~45 degrees) for vertical classification
                float verticalAlignment = Mathf.Abs(thrustDirection.y);
                if (verticalAlignment > 0.7f)
                {
                    return true; // Found at least one vertical thruster
                }
            }

            return false; // No vertical thrusters found
        }

        /// <summary>
        /// Checks if node or children have wheel extensions.
        /// </summary>
        private static bool HasWheels(int nodeIndex, OMIImportContext context)
        {
            // Find all child nodes with OMI_vehicle_wheel extension
            var wheelNodeIndices = OMIExtensionDetector.FindChildNodesWithExtension(
                context, nodeIndex, "OMI_vehicle_wheel");

            return wheelNodeIndices.Count > 0;
        }

        /// <summary>
        /// Checks if a node has a specific OMI extension.
        /// </summary>
        private static bool HasExtension(OMIImportContext context, int nodeIndex, string extensionName)
        {
            return OMIExtensionDetector.HasExtension(context, nodeIndex, extensionName);
        }

        /// <summary>
        /// Checks if GameObject has a CharacterController component.
        /// </summary>
        private static bool HasCharacterController(GameObject gameObject)
        {
            return gameObject.GetComponent<CharacterController>() != null;
        }

        /// <summary>
        /// Checks if GameObject has mesh components.
        /// </summary>
        private static bool HasMesh(GameObject gameObject)
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            return meshFilter != null && meshRenderer != null && meshFilter.sharedMesh != null;
        }

        /// <summary>
        /// Creates an AutomobileEntity.
        /// </summary>
        private static AutomobileEntity CreateAutomobileEntity(
            GameObject gameObject,
            Guid entityId,
            BaseEntity parent,
            OMIImportContext context)
        {
            var automobile = gameObject.AddComponent<AutomobileEntity>();
            automobile.Initialize(entityId);
            automobile.SetParent(parent);

            // Get vehicle type configuration from runtime
            var runtime = GetRuntime(context);
            if (runtime?.straightFour?.automobileEntityTypeMap != null)
            {
                // Default to sedan for now
                var vehicleType = EntityManager.AutomobileEntityType.Default;
                if (runtime.straightFour.automobileEntityTypeMap.TryGetValue(vehicleType, out var stateSettings))
                {
                    automobile.stateSettings = stateSettings;
                }
            }

            Logging.Log($"[EntityFactory] Created AutomobileEntity: {gameObject.name}");
            return automobile;
        }

        /// <summary>
        /// Creates an AirplaneEntity.
        /// </summary>
        private static AirplaneEntity CreateAirplaneEntity(
            GameObject gameObject,
            Guid entityId,
            BaseEntity parent,
            OMIImportContext context)
        {
            var airplane = gameObject.AddComponent<AirplaneEntity>();
            airplane.Initialize(entityId);
            airplane.SetParent(parent);

            Logging.Log($"[EntityFactory] Created AirplaneEntity: {gameObject.name}");
            return airplane;
        }

        /// <summary>
        /// Creates a CharacterEntity.
        /// </summary>
        private static CharacterEntity CreateCharacterEntity(
            GameObject gameObject,
            Guid entityId,
            BaseEntity parent,
            OMIImportContext context)
        {
            var character = gameObject.AddComponent<CharacterEntity>();
            character.Initialize(entityId);
            character.SetParent(parent);

            Logging.Log($"[EntityFactory] Created CharacterEntity: {gameObject.name}");
            return character;
        }

        /// <summary>
        /// Creates an AudioEntity.
        /// </summary>
        private static AudioEntity CreateAudioEntity(
            GameObject gameObject,
            Guid entityId,
            BaseEntity parent,
            OMIImportContext context)
        {
            var audio = gameObject.AddComponent<AudioEntity>();
            audio.Initialize(entityId);
            audio.SetParent(parent);

            Logging.Log($"[EntityFactory] Created AudioEntity: {gameObject.name}");
            return audio;
        }

        /// <summary>
        /// Creates a MeshEntity.
        /// </summary>
        private static MeshEntity CreateMeshEntity(
            GameObject gameObject,
            Guid entityId,
            BaseEntity parent,
            OMIImportContext context)
        {
            // Check if MeshEntity already exists (from glTFast import)
            var existingMeshEntity = gameObject.GetComponent<MeshEntity>();
            if (existingMeshEntity != null)
            {
                return existingMeshEntity;
            }

            var meshEntity = gameObject.AddComponent<MeshEntity>();
            meshEntity.Initialize(entityId);
            meshEntity.SetParent(parent);

            Logging.Log($"[EntityFactory] Created MeshEntity: {gameObject.name}");
            return meshEntity;
        }

        /// <summary>
        /// Creates a ContainerEntity.
        /// </summary>
        private static ContainerEntity CreateContainerEntity(
            GameObject gameObject,
            Guid entityId,
            BaseEntity parent,
            OMIImportContext context)
        {
            // Check if ContainerEntity already exists
            var existingContainer = gameObject.GetComponent<ContainerEntity>();
            if (existingContainer != null)
            {
                return existingContainer;
            }

            var container = gameObject.AddComponent<ContainerEntity>();
            container.Initialize(entityId);
            container.SetParent(parent);

            Logging.Log($"[EntityFactory] Created ContainerEntity: {gameObject.name}");
            return container;
        }

        /// <summary>
        /// Gets WebVerseRuntime from context.
        /// </summary>
        private static Runtime.WebVerseRuntime GetRuntime(OMIImportContext context)
        {
            if (context.CustomData.TryGetValue(StraightFourCustomDataKeys.Runtime, out var runtime))
            {
                return runtime as Runtime.WebVerseRuntime;
            }
            return Runtime.WebVerseRuntime.Instance;
        }

        /// <summary>
        /// Creates JavaScript API wrapper for entity and registers mapping.
        /// Required for JavaScript code to access OMI entities via world.getEntity(id).
        /// </summary>
        private static void CreateAndMapJavaScriptWrapper(BaseEntity internalEntity, EntityType entityType)
        {
            if (internalEntity == null)
            {
                return;
            }

            // Create appropriate JavaScript wrapper based on entity type
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity jsWrapper = null;

            switch (entityType)
            {
                case EntityType.Automobile:
                    var autoWrapper = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AutomobileEntity();
                    autoWrapper.internalEntity = internalEntity;
                    autoWrapper.internalEntityType = typeof(FiveSQD.StraightFour.Entity.AutomobileEntity);
                    jsWrapper = autoWrapper;
                    break;

                case EntityType.Airplane:
                    var airplaneWrapper = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AirplaneEntity();
                    airplaneWrapper.internalEntity = internalEntity;
                    airplaneWrapper.internalEntityType = typeof(FiveSQD.StraightFour.Entity.AirplaneEntity);
                    jsWrapper = airplaneWrapper;
                    break;

                case EntityType.Character:
                    var characterWrapper = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CharacterEntity();
                    characterWrapper.internalEntity = internalEntity;
                    characterWrapper.internalEntityType = typeof(FiveSQD.StraightFour.Entity.CharacterEntity);
                    jsWrapper = characterWrapper;
                    break;

                case EntityType.Audio:
                    var audioWrapper = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AudioEntity();
                    audioWrapper.internalEntity = internalEntity;
                    audioWrapper.internalEntityType = typeof(FiveSQD.StraightFour.Entity.AudioEntity);
                    jsWrapper = audioWrapper;
                    break;

                case EntityType.Mesh:
                    var meshWrapper = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity();
                    meshWrapper.internalEntity = internalEntity;
                    meshWrapper.internalEntityType = typeof(FiveSQD.StraightFour.Entity.MeshEntity);
                    jsWrapper = meshWrapper;
                    break;

                case EntityType.Container:
                    var containerWrapper = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.ContainerEntity();
                    containerWrapper.internalEntity = internalEntity;
                    containerWrapper.internalEntityType = typeof(FiveSQD.StraightFour.Entity.ContainerEntity);
                    jsWrapper = containerWrapper;
                    break;

                default:
                    Logging.LogWarning($"[EntityFactory] Unknown entity type for JavaScript wrapper: {entityType}");
                    return;
            }

            // Register mapping so JavaScript API can find this entity
            if (jsWrapper != null)
            {
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.AddEntityMapping(internalEntity, jsWrapper);
                Logging.Log($"[EntityFactory] Registered JavaScript API mapping for {entityType} entity");
            }
        }
    }
}
#endif
