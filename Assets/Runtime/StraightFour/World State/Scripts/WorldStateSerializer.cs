// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using FiveSQD.StraightFour.Utilities;
using FiveSQD.StraightFour.Entity;
using UnityEngine;
using Newtonsoft.Json;

namespace FiveSQD.StraightFour.WorldState
{
    /// <summary>
    /// Serializer for capturing and restoring world state.
    /// Used for tab switching and world persistence.
    /// </summary>
    public class WorldStateSerializer
    {
        /// <summary>
        /// JSON serializer settings for consistent formatting.
        /// </summary>
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// Capture the current world state as a snapshot.
        /// </summary>
        /// <param name="world">The world to capture.</param>
        /// <param name="worldName">Name/URL of the world.</param>
        /// <param name="basePath">Base path for world resources.</param>
        /// <returns>WorldStateSnapshot containing all serializable state.</returns>
        public static WorldStateSnapshot CaptureWorldState(World.World world, string worldName, string basePath)
        {
            if (world == null)
            {
                LogSystem.LogError("[WorldStateSerializer->CaptureWorldState] World is null.");
                return null;
            }

            var snapshot = new WorldStateSnapshot
            {
                worldName = worldName,
                basePath = basePath,
                worldOffset = new SerializableVector3(world.worldOffset)
            };
            snapshot.SetTimestamp();

            // Capture all entities
            CaptureEntities(world, snapshot);

            // Capture camera state
            snapshot.camera = CaptureCameraState(world);

            // Capture world storage
            CaptureWorldStorage(world, snapshot);

            // Capture environment
            snapshot.environment = CaptureEnvironmentState();

            LogSystem.Log($"[WorldStateSerializer->CaptureWorldState] Captured {snapshot.entities.Count} entities.");
            return snapshot;
        }

        /// <summary>
        /// Serialize a world state snapshot to JSON string.
        /// </summary>
        /// <param name="snapshot">Snapshot to serialize.</param>
        /// <returns>JSON string representation.</returns>
        public static string SerializeToJson(WorldStateSnapshot snapshot)
        {
            if (snapshot == null)
            {
                LogSystem.LogError("[WorldStateSerializer->SerializeToJson] Snapshot is null.");
                return null;
            }

            try
            {
                return JsonConvert.SerializeObject(snapshot, jsonSettings);
            }
            catch (Exception e)
            {
                LogSystem.LogError($"[WorldStateSerializer->SerializeToJson] Failed to serialize: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserialize a JSON string to a world state snapshot.
        /// </summary>
        /// <param name="json">JSON string to deserialize.</param>
        /// <returns>WorldStateSnapshot or null if deserialization fails.</returns>
        public static WorldStateSnapshot DeserializeFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                LogSystem.LogError("[WorldStateSerializer->DeserializeFromJson] JSON is null or empty.");
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<WorldStateSnapshot>(json, jsonSettings);
            }
            catch (Exception e)
            {
                LogSystem.LogError($"[WorldStateSerializer->DeserializeFromJson] Failed to deserialize: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Capture all entities from the world.
        /// </summary>
        private static void CaptureEntities(World.World world, WorldStateSnapshot snapshot)
        {
            if (world.entityManager == null)
            {
                LogSystem.LogWarning("[WorldStateSerializer->CaptureEntities] EntityManager is null.");
                return;
            }

            var allEntities = world.entityManager.GetAllEntities();
            if (allEntities == null) return;

            foreach (var entity in allEntities)
            {
                var entitySnapshot = CaptureEntity(entity);
                if (entitySnapshot != null)
                {
                    snapshot.entities.Add(entitySnapshot);
                }
            }
        }

        /// <summary>
        /// Capture a single entity's state.
        /// </summary>
        private static EntitySnapshot CaptureEntity(BaseEntity entity)
        {
            if (entity == null) return null;

            var snapshot = new EntitySnapshot
            {
                id = entity.id.ToString(),
                entityType = entity.GetType().Name,
                tag = entity.entityTag,
                visible = entity.gameObject.activeSelf
            };

            // Get parent ID if exists
            if (entity.GetParent() != null)
            {
                snapshot.parentId = entity.GetParent().id.ToString();
            }

            // Capture transform (false = world space)
            snapshot.position = new SerializableVector3(entity.GetPosition(false));
            snapshot.rotation = new SerializableQuaternion(entity.GetRotation(false));
            snapshot.scale = new SerializableVector3(entity.GetScale());

            // Capture interaction state
            snapshot.interactionState = entity.GetInteractionState().ToString();

            // Capture physics if applicable
            snapshot.physics = CapturePhysicsState(entity);

            // Capture type-specific data
            snapshot.typeSpecificData = CaptureTypeSpecificData(entity);

            return snapshot;
        }

        /// <summary>
        /// Capture physics state from an entity.
        /// </summary>
        private static PhysicsSnapshot CapturePhysicsState(BaseEntity entity)
        {
            var rb = entity.GetComponent<Rigidbody>();
            if (rb == null) return null;

            return new PhysicsSnapshot
            {
                mass = rb.mass,
                drag = rb.linearDamping,
                angularDrag = rb.angularDamping,
                useGravity = rb.useGravity,
                isKinematic = rb.isKinematic,
                velocity = new SerializableVector3(rb.linearVelocity),
                angularVelocity = new SerializableVector3(rb.angularVelocity),
                centerOfMass = new SerializableVector3(rb.centerOfMass)
            };
        }

        /// <summary>
        /// Capture type-specific data for different entity types.
        /// </summary>
        private static string CaptureTypeSpecificData(BaseEntity entity)
        {
            try
            {
                object data = null;

                if (entity is MeshEntity meshEntity)
                {
                    data = CaptureMeshEntityData(meshEntity);
                }
                else if (entity is LightEntity lightEntity)
                {
                    data = CaptureLightEntityData(lightEntity);
                }
                else if (entity is CharacterEntity characterEntity)
                {
                    data = CaptureCharacterEntityData(characterEntity);
                }
                else if (entity is TerrainEntity terrainEntity)
                {
                    data = CaptureTerrainEntityData(terrainEntity);
                }
                else if (entity is WaterBodyEntity waterEntity)
                {
                    data = CaptureWaterEntityData(waterEntity);
                }
                // Add more entity types as needed

                if (data != null)
                {
                    return JsonConvert.SerializeObject(data, jsonSettings);
                }
            }
            catch (Exception e)
            {
                LogSystem.LogWarning($"[WorldStateSerializer->CaptureTypeSpecificData] Failed for {entity.GetType().Name}: {e.Message}");
            }

            return null;
        }

        private static MeshEntityData CaptureMeshEntityData(MeshEntity entity)
        {
            var data = new MeshEntityData();

            // Get color from the mesh renderer if available
            var meshRenderer = entity.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.material != null)
            {
                data.color = new SerializableColor(meshRenderer.material.color);
            }

            return data;
        }

        private static LightEntityData CaptureLightEntityData(LightEntity entity)
        {
            var light = entity.GetComponentInChildren<Light>();
            if (light == null) return null;

            return new LightEntityData
            {
                lightType = light.type.ToString(),
                color = new SerializableColor(light.color),
                intensity = light.intensity,
                range = light.range,
                spotAngleInner = light.innerSpotAngle,
                spotAngleOuter = light.spotAngle,
                temperature = light.colorTemperature
            };
        }

        private static CharacterEntityData CaptureCharacterEntityData(CharacterEntity entity)
        {
            var cc = entity.GetComponent<CharacterController>();
            if (cc == null) return null;

            return new CharacterEntityData
            {
                radius = cc.radius,
                height = cc.height,
                center = new SerializableVector3(cc.center)
            };
        }

        private static TerrainEntityData CaptureTerrainEntityData(TerrainEntity entity)
        {
            var terrain = entity.GetComponentInChildren<Terrain>();
            if (terrain == null || terrain.terrainData == null) return null;

            var terrainData = terrain.terrainData;
            var heights2D = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

            // Flatten 2D array for serialization
            int width = heights2D.GetLength(0);
            int height = heights2D.GetLength(1);
            float[] flatHeights = new float[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flatHeights[y * width + x] = heights2D[x, y];
                }
            }

            return new TerrainEntityData
            {
                terrainType = entity is HybridTerrainEntity ? "hybrid" : "standard",
                length = terrainData.size.x,
                width = terrainData.size.z,
                height = terrainData.size.y,
                heights = flatHeights,
                heightsWidth = width,
                heightsHeight = height
            };
        }

        private static WaterEntityData CaptureWaterEntityData(WaterBodyEntity entity)
        {
            return new WaterEntityData
            {
                size = new SerializableVector3(entity.GetScale())
            };
        }

        /// <summary>
        /// Capture current camera state.
        /// </summary>
        private static CameraSnapshot CaptureCameraState(World.World world)
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) return null;

            return new CameraSnapshot
            {
                position = new SerializableVector3(mainCamera.transform.position),
                rotation = new SerializableQuaternion(mainCamera.transform.rotation),
                fieldOfView = mainCamera.fieldOfView,
                nearClipPlane = mainCamera.nearClipPlane,
                farClipPlane = mainCamera.farClipPlane
            };
        }

        /// <summary>
        /// Capture world storage state.
        /// </summary>
        private static void CaptureWorldStorage(World.World world, WorldStateSnapshot snapshot)
        {
            if (world.storageManager == null) return;

            var allItems = world.storageManager.GetAllItems();
            if (allItems != null)
            {
                snapshot.storage = new Dictionary<string, string>(allItems);
            }
        }

        /// <summary>
        /// Capture environment state.
        /// </summary>
        private static EnvironmentSnapshot CaptureEnvironmentState()
        {
            return new EnvironmentSnapshot
            {
                ambientColor = new SerializableColor(RenderSettings.ambientLight),
                fogColor = new SerializableColor(RenderSettings.fogColor),
                fogDensity = RenderSettings.fogDensity,
                fogEnabled = RenderSettings.fog
            };
        }

        /// <summary>
        /// Get the estimated size of a snapshot in bytes.
        /// </summary>
        public static long EstimateSnapshotSize(WorldStateSnapshot snapshot)
        {
            if (snapshot == null) return 0;

            string json = SerializeToJson(snapshot);
            return json?.Length * 2 ?? 0; // UTF-16 encoding
        }
    }
}
