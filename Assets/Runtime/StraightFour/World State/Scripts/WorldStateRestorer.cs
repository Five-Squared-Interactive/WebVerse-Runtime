// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using FiveSQD.StraightFour.Utilities;
using FiveSQD.StraightFour.Entity;
using UnityEngine;
using Newtonsoft.Json;

namespace FiveSQD.StraightFour.WorldState
{
    /// <summary>
    /// Helper class to capture coroutine results.
    /// </summary>
    internal class EntityResult
    {
        public BaseEntity Entity { get; set; }
    }

    /// <summary>
    /// Restores world state from snapshots.
    /// Used for tab switching and world state persistence.
    /// </summary>
    public class WorldStateRestorer : MonoBehaviour
    {
        /// <summary>
        /// Current restoration progress (0-1).
        /// </summary>
        public float Progress { get; private set; }

        /// <summary>
        /// Whether a restoration is currently in progress.
        /// </summary>
        public bool IsRestoring { get; private set; }

        /// <summary>
        /// Event fired when restoration completes.
        /// </summary>
        public event Action<bool> OnRestoreComplete;

        /// <summary>
        /// Event fired when an entity is restored.
        /// </summary>
        public event Action<EntitySnapshot, BaseEntity> OnEntityRestored;

        /// <summary>
        /// Mapping of snapshot entity IDs to restored entities.
        /// </summary>
        private Dictionary<string, BaseEntity> restoredEntities;

        /// <summary>
        /// Entities waiting for parent restoration.
        /// </summary>
        private List<EntitySnapshot> pendingParentEntities;

        /// <summary>
        /// The world being restored to.
        /// </summary>
        private World.World targetWorld;

        /// <summary>
        /// Restore a world state from a snapshot.
        /// </summary>
        /// <param name="world">Target world to restore to.</param>
        /// <param name="snapshot">Snapshot to restore from.</param>
        /// <param name="onComplete">Optional callback when restoration completes.</param>
        public void RestoreWorldState(World.World world, WorldStateSnapshot snapshot, Action<bool> onComplete = null)
        {
            if (world == null)
            {
                LogSystem.LogError("[WorldStateRestorer->RestoreWorldState] World is null.");
                onComplete?.Invoke(false);
                return;
            }

            if (snapshot == null)
            {
                LogSystem.LogError("[WorldStateRestorer->RestoreWorldState] Snapshot is null.");
                onComplete?.Invoke(false);
                return;
            }

            if (IsRestoring)
            {
                LogSystem.LogWarning("[WorldStateRestorer->RestoreWorldState] Restoration already in progress.");
                onComplete?.Invoke(false);
                return;
            }

            targetWorld = world;
            if (onComplete != null)
            {
                OnRestoreComplete += onComplete;
            }

            StartCoroutine(RestoreWorldStateCoroutine(snapshot));
        }

        /// <summary>
        /// Restore world state from JSON string.
        /// </summary>
        /// <param name="world">Target world to restore to.</param>
        /// <param name="json">JSON snapshot string.</param>
        /// <param name="onComplete">Optional callback when restoration completes.</param>
        public void RestoreWorldStateFromJson(World.World world, string json, Action<bool> onComplete = null)
        {
            var snapshot = WorldStateSerializer.DeserializeFromJson(json);
            if (snapshot == null)
            {
                LogSystem.LogError("[WorldStateRestorer->RestoreWorldStateFromJson] Failed to deserialize snapshot.");
                onComplete?.Invoke(false);
                return;
            }

            RestoreWorldState(world, snapshot, onComplete);
        }

        /// <summary>
        /// Cancel an in-progress restoration.
        /// </summary>
        public void CancelRestore()
        {
            if (IsRestoring)
            {
                StopAllCoroutines();
                IsRestoring = false;
                Progress = 0f;
                LogSystem.Log("[WorldStateRestorer->CancelRestore] Restoration cancelled.");
            }
        }

        /// <summary>
        /// Main restoration coroutine.
        /// </summary>
        private IEnumerator RestoreWorldStateCoroutine(WorldStateSnapshot snapshot)
        {
            IsRestoring = true;
            Progress = 0f;
            restoredEntities = new Dictionary<string, BaseEntity>();
            pendingParentEntities = new List<EntitySnapshot>();

            LogSystem.Log($"[WorldStateRestorer] Starting restoration of '{snapshot.worldName}' with {snapshot.entities.Count} entities.");

            // Phase 1: Restore environment (5%)
            RestoreEnvironment(snapshot.environment);
            Progress = 0.05f;
            yield return null;

            // Phase 2: Restore world storage (10%)
            RestoreWorldStorage(snapshot.storage);
            Progress = 0.10f;
            yield return null;

            // Phase 3: Restore entities (10% - 90%)
            int totalEntities = snapshot.entities.Count;
            int restoredCount = 0;

            // First pass: Restore root entities (no parent)
            foreach (var entitySnapshot in snapshot.entities)
            {
                if (string.IsNullOrEmpty(entitySnapshot.parentId))
                {
                    yield return StartCoroutine(RestoreEntityCoroutine(entitySnapshot, null));
                    restoredCount++;
                    Progress = 0.10f + (0.80f * restoredCount / totalEntities);
                }
                else
                {
                    pendingParentEntities.Add(entitySnapshot);
                }
            }

            // Subsequent passes: Restore child entities
            int maxPasses = 10; // Prevent infinite loops
            int pass = 0;
            while (pendingParentEntities.Count > 0 && pass < maxPasses)
            {
                pass++;
                var stillPending = new List<EntitySnapshot>();

                foreach (var entitySnapshot in pendingParentEntities)
                {
                    if (restoredEntities.ContainsKey(entitySnapshot.parentId))
                    {
                        var parent = restoredEntities[entitySnapshot.parentId];
                        yield return StartCoroutine(RestoreEntityCoroutine(entitySnapshot, parent));
                        restoredCount++;
                        Progress = 0.10f + (0.80f * restoredCount / totalEntities);
                    }
                    else
                    {
                        stillPending.Add(entitySnapshot);
                    }
                }

                pendingParentEntities = stillPending;
                yield return null;
            }

            if (pendingParentEntities.Count > 0)
            {
                LogSystem.LogWarning($"[WorldStateRestorer] {pendingParentEntities.Count} entities could not find their parents.");
            }

            // Phase 4: Restore camera (95%)
            RestoreCamera(snapshot.camera);
            Progress = 0.95f;
            yield return null;

            // Phase 5: Apply physics states (100%)
            ApplyPhysicsStates(snapshot.entities);
            Progress = 1.0f;

            IsRestoring = false;
            LogSystem.Log($"[WorldStateRestorer] Restoration complete. Restored {restoredEntities.Count} entities.");

            OnRestoreComplete?.Invoke(true);
            OnRestoreComplete = null;
        }

        /// <summary>
        /// Restore a single entity from snapshot.
        /// </summary>
        private IEnumerator RestoreEntityCoroutine(EntitySnapshot snapshot, BaseEntity parent)
        {
            if (snapshot == null) yield break;

            Guid entityId;
            if (!Guid.TryParse(snapshot.id, out entityId))
            {
                LogSystem.LogWarning($"[WorldStateRestorer] Invalid entity ID: {snapshot.id}");
                yield break;
            }

            var result = new EntityResult();
            Vector3 position = snapshot.position?.ToVector3() ?? Vector3.zero;
            Quaternion rotation = snapshot.rotation?.ToQuaternion() ?? Quaternion.identity;
            Vector3 scale = snapshot.scale?.ToVector3() ?? Vector3.one;

            // Create entity based on type
            switch (snapshot.entityType)
            {
                case "ContainerEntity":
                    yield return StartCoroutine(RestoreContainerEntity(entityId, parent, position, rotation, scale, snapshot.tag, result));
                    break;

                case "MeshEntity":
                    yield return StartCoroutine(RestoreMeshEntity(entityId, parent, position, rotation, scale, snapshot, result));
                    break;

                case "LightEntity":
                    yield return StartCoroutine(RestoreLightEntity(entityId, parent, position, rotation, snapshot, result));
                    break;

                case "CharacterEntity":
                    yield return StartCoroutine(RestoreCharacterEntity(entityId, parent, position, rotation, scale, snapshot, result));
                    break;

                case "TerrainEntity":
                case "HybridTerrainEntity":
                    yield return StartCoroutine(RestoreTerrainEntity(entityId, position, snapshot, result));
                    break;

                case "WaterBodyEntity":
                    yield return StartCoroutine(RestoreWaterEntity(entityId, parent, position, rotation, scale, snapshot, result));
                    break;

                default:
                    // For unsupported types, create a container as placeholder
                    LogSystem.LogWarning($"[WorldStateRestorer] Unsupported entity type: {snapshot.entityType}, creating container.");
                    yield return StartCoroutine(RestoreContainerEntity(entityId, parent, position, rotation, scale, snapshot.tag, result));
                    break;
            }

            if (result.Entity != null)
            {
                // Apply common properties
                result.Entity.SetVisibility(snapshot.visible);

                // Parse and apply interaction state
                if (!string.IsNullOrEmpty(snapshot.interactionState))
                {
                    if (Enum.TryParse<BaseEntity.InteractionState>(snapshot.interactionState, out var state))
                    {
                        result.Entity.SetInteractionState(state);
                    }
                }

                restoredEntities[snapshot.id] = result.Entity;
                OnEntityRestored?.Invoke(snapshot, result.Entity);
            }
        }

        #region Entity Type Restoration Methods

        private IEnumerator RestoreContainerEntity(Guid id, BaseEntity parent, Vector3 position, Quaternion rotation, Vector3 scale, string tag, EntityResult result)
        {
            bool loaded = false;

            targetWorld.entityManager.LoadContainerEntity(
                parent, position, rotation, scale, id, tag, false,
                () => { loaded = true; result.Entity = targetWorld.entityManager.FindEntity(id); }
            );

            while (!loaded) yield return null;
        }

        private IEnumerator RestoreMeshEntity(Guid id, BaseEntity parent, Vector3 position, Quaternion rotation, Vector3 scale, EntitySnapshot snapshot, EntityResult result)
        {
            bool loaded = false;

            // Parse type-specific data
            MeshEntityData meshData = null;
            if (!string.IsNullOrEmpty(snapshot.typeSpecificData))
            {
                try
                {
                    meshData = JsonConvert.DeserializeObject<MeshEntityData>(snapshot.typeSpecificData);
                }
                catch { }
            }

            // For now, create a basic mesh entity without the original mesh
            // Full restoration would require re-downloading/loading the original mesh
            targetWorld.entityManager.LoadContainerEntity(
                parent, position, rotation, scale, id, snapshot.tag, false,
                () => { loaded = true; result.Entity = targetWorld.entityManager.FindEntity(id); }
            );

            while (!loaded) yield return null;

            // Apply color if available
            if (result.Entity != null && meshData?.color != null)
            {
                var renderer = result.Entity.GetComponentInChildren<MeshRenderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = meshData.color.ToColor();
                }
            }
        }

        private IEnumerator RestoreLightEntity(Guid id, BaseEntity parent, Vector3 position, Quaternion rotation, EntitySnapshot snapshot, EntityResult result)
        {
            bool loaded = false;

            targetWorld.entityManager.LoadLightEntity(
                parent, position, rotation, id, snapshot.tag,
                () => { loaded = true; result.Entity = targetWorld.entityManager.FindEntity(id); }
            );

            while (!loaded) yield return null;

            // Apply light-specific properties
            if (result.Entity != null && !string.IsNullOrEmpty(snapshot.typeSpecificData))
            {
                try
                {
                    var lightData = JsonConvert.DeserializeObject<LightEntityData>(snapshot.typeSpecificData);
                    if (lightData != null)
                    {
                        var light = result.Entity.GetComponentInChildren<Light>();
                        if (light != null)
                        {
                            light.color = lightData.color?.ToColor() ?? Color.white;
                            light.intensity = lightData.intensity;
                            light.range = lightData.range;
                            if (light.type == LightType.Spot)
                            {
                                light.innerSpotAngle = lightData.spotAngleInner;
                                light.spotAngle = lightData.spotAngleOuter;
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private IEnumerator RestoreCharacterEntity(Guid id, BaseEntity parent, Vector3 position, Quaternion rotation, Vector3 scale, EntitySnapshot snapshot, EntityResult result)
        {
            bool loaded = false;

            // Restore as default character (without mesh)
            targetWorld.entityManager.LoadCharacterEntity(
                parent, null, Vector3.zero, Quaternion.identity, Vector3.up * 2f,
                position, rotation, scale, id, snapshot.tag, false,
                () => { loaded = true; result.Entity = targetWorld.entityManager.FindEntity(id); }
            );

            while (!loaded) yield return null;
        }

        private IEnumerator RestoreTerrainEntity(Guid id, Vector3 position, EntitySnapshot snapshot, EntityResult result)
        {
            if (string.IsNullOrEmpty(snapshot.typeSpecificData))
            {
                LogSystem.LogWarning("[WorldStateRestorer] Terrain entity has no type-specific data.");
                yield break;
            }

            TerrainEntityData terrainData;
            try
            {
                terrainData = JsonConvert.DeserializeObject<TerrainEntityData>(snapshot.typeSpecificData);
            }
            catch (Exception e)
            {
                LogSystem.LogWarning($"[WorldStateRestorer] Failed to parse terrain data: {e.Message}");
                yield break;
            }

            if (terrainData == null)
            {
                yield break;
            }

            // Reconstruct 2D heights array
            float[,] heights = null;
            if (terrainData.heights != null && terrainData.heightsWidth > 0 && terrainData.heightsHeight > 0)
            {
                heights = new float[terrainData.heightsWidth, terrainData.heightsHeight];
                for (int y = 0; y < terrainData.heightsHeight; y++)
                {
                    for (int x = 0; x < terrainData.heightsWidth; x++)
                    {
                        int index = y * terrainData.heightsWidth + x;
                        if (index < terrainData.heights.Length)
                        {
                            heights[x, y] = terrainData.heights[index];
                        }
                    }
                }
            }

            bool loaded = false;

            if (terrainData.terrainType == "hybrid")
            {
                targetWorld.entityManager.LoadHybridTerrainEntity(
                    terrainData.length, terrainData.width, terrainData.height,
                    heights, null, null,
                    null, position, Quaternion.identity, false,
                    id, snapshot.tag,
                    () => { loaded = true; result.Entity = targetWorld.entityManager.FindEntity(id); }
                );
            }
            else
            {
                targetWorld.entityManager.LoadTerrainEntity(
                    terrainData.length, terrainData.width, terrainData.height,
                    heights, null, null,
                    null, position, Quaternion.identity, false,
                    id, snapshot.tag,
                    () => { loaded = true; result.Entity = targetWorld.entityManager.FindEntity(id); }
                );
            }

            while (!loaded) yield return null;
        }

        private IEnumerator RestoreWaterEntity(Guid id, BaseEntity parent, Vector3 position, Quaternion rotation, Vector3 scale, EntitySnapshot snapshot, EntityResult result)
        {
            bool loaded = false;

            // Default water colors and parameters
            Color shallowColor = new Color(0.2f, 0.5f, 0.8f, 0.5f);
            Color deepColor = new Color(0.1f, 0.2f, 0.4f, 0.8f);
            Color specularColor = Color.white;
            Color scatteringColor = new Color(0.0f, 0.3f, 0.2f, 1.0f);
            float deepStart = 0.2f;
            float deepEnd = 1.0f;
            float distortion = 0.5f;
            float smoothness = 0.9f;
            float numWaves = 5f;
            float waveAmplitude = 0.5f;
            float waveSteepness = 0.5f;
            float waveSpeed = 1.0f;
            float waveLength = 1.0f;
            float waterScale = 1.0f;
            float intensity = 1.0f;

            if (!string.IsNullOrEmpty(snapshot.typeSpecificData))
            {
                try
                {
                    var waterData = JsonConvert.DeserializeObject<WaterEntityData>(snapshot.typeSpecificData);
                    if (waterData != null)
                    {
                        if (waterData.shallowColor != null) shallowColor = waterData.shallowColor.ToColor();
                        if (waterData.deepColor != null) deepColor = waterData.deepColor.ToColor();
                    }
                }
                catch { }
            }

            targetWorld.entityManager.LoadWaterBodyEntity(
                shallowColor, deepColor, specularColor, scatteringColor,
                deepStart, deepEnd, distortion, smoothness,
                numWaves, waveAmplitude, waveSteepness, waveSpeed,
                waveLength, waterScale, intensity,
                parent, position, rotation,
                id, snapshot.tag,
                () => { loaded = true; result.Entity = targetWorld.entityManager.FindEntity(id); }
            );

            while (!loaded) yield return null;
        }

        #endregion

        #region State Restoration Methods

        /// <summary>
        /// Restore environment settings.
        /// </summary>
        private void RestoreEnvironment(EnvironmentSnapshot environment)
        {
            if (environment == null) return;

            if (environment.ambientColor != null)
            {
                RenderSettings.ambientLight = environment.ambientColor.ToColor();
            }

            RenderSettings.fog = environment.fogEnabled;
            if (environment.fogEnabled && environment.fogColor != null)
            {
                RenderSettings.fogColor = environment.fogColor.ToColor();
                RenderSettings.fogDensity = environment.fogDensity;
            }

            LogSystem.Log("[WorldStateRestorer] Environment restored.");
        }

        /// <summary>
        /// Restore world storage.
        /// </summary>
        private void RestoreWorldStorage(Dictionary<string, string> storage)
        {
            if (storage == null || targetWorld.storageManager == null) return;

            targetWorld.storageManager.SetAllItems(storage);
            LogSystem.Log($"[WorldStateRestorer] Restored {storage.Count} storage items.");
        }

        /// <summary>
        /// Restore camera state.
        /// </summary>
        private void RestoreCamera(CameraSnapshot camera)
        {
            if (camera == null) return;

            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) return;

            if (camera.position != null)
            {
                mainCamera.transform.position = camera.position.ToVector3();
            }
            if (camera.rotation != null)
            {
                mainCamera.transform.rotation = camera.rotation.ToQuaternion();
            }

            mainCamera.fieldOfView = camera.fieldOfView;
            mainCamera.nearClipPlane = camera.nearClipPlane;
            mainCamera.farClipPlane = camera.farClipPlane;

            LogSystem.Log("[WorldStateRestorer] Camera restored.");
        }

        /// <summary>
        /// Apply physics states to restored entities.
        /// </summary>
        private void ApplyPhysicsStates(List<EntitySnapshot> entitySnapshots)
        {
            foreach (var snapshot in entitySnapshots)
            {
                if (snapshot.physics == null) continue;
                if (!restoredEntities.ContainsKey(snapshot.id)) continue;

                var entity = restoredEntities[snapshot.id];
                var rb = entity.GetComponent<Rigidbody>();
                if (rb == null) continue;

                rb.mass = snapshot.physics.mass;
                rb.linearDamping = snapshot.physics.drag;
                rb.angularDamping = snapshot.physics.angularDrag;
                rb.useGravity = snapshot.physics.useGravity;
                rb.isKinematic = snapshot.physics.isKinematic;

                if (!rb.isKinematic)
                {
                    if (snapshot.physics.velocity != null)
                    {
                        rb.linearVelocity = snapshot.physics.velocity.ToVector3();
                    }
                    if (snapshot.physics.angularVelocity != null)
                    {
                        rb.angularVelocity = snapshot.physics.angularVelocity.ToVector3();
                    }
                }
            }

            LogSystem.Log("[WorldStateRestorer] Physics states applied.");
        }

        #endregion
    }
}
