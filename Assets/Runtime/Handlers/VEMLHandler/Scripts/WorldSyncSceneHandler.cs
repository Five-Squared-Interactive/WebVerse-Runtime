// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

#if USE_WEBINTERFACE
using System;
using System.Collections.Generic;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.WorldSync;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.VEML
{
    /// <summary>
    /// Subscribes to WorldSync session entity events and materializes/updates/deletes
    /// remote entities in the local StraightFour scene. Created automatically by
    /// VEMLHandler when a wsync synchronizationservice is established.
    /// </summary>
    public class WorldSyncSceneHandler : IDisposable
    {
        /// <summary>
        /// Mapping from server entity ID to local entity GUID.
        /// </summary>
        private readonly Dictionary<string, Guid> _serverToLocalMap = new Dictionary<string, Guid>();

        /// <summary>
        /// Entities currently loading (awaiting async load completion).
        /// </summary>
        private readonly HashSet<string> _pendingEntities = new HashSet<string>();

        /// <summary>
        /// Queued transform updates for entities that haven't finished loading yet.
        /// </summary>
        private readonly Dictionary<string, PendingTransform> _pendingTransforms
            = new Dictionary<string, PendingTransform>();

        private readonly SyncSession _session;
        private readonly string _localClientId;
        private bool _disposed;

        private struct PendingTransform
        {
            public SyncVector3? Position;
            public SyncQuaternion? Rotation;
            public SyncVector3? Scale;
        }

        /// <summary>
        /// Create a scene handler and subscribe to session entity events.
        /// </summary>
        /// <param name="session">The WorldSync session to listen to.</param>
        /// <param name="localClientId">This client's ID, used for echo suppression.</param>
        public WorldSyncSceneHandler(SyncSession session, string localClientId)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _localClientId = localClientId;

            _session.OnEntityCreated += OnRemoteEntityCreated;
            _session.OnEntityTransformUpdated += OnRemoteTransformUpdated;
            _session.OnEntityDeleted += OnRemoteEntityDeleted;
        }

        /// <summary>
        /// Unsubscribe from events and destroy all remote entities.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_session != null)
            {
                _session.OnEntityCreated -= OnRemoteEntityCreated;
                _session.OnEntityTransformUpdated -= OnRemoteTransformUpdated;
                _session.OnEntityDeleted -= OnRemoteEntityDeleted;
            }

            // Destroy all materialized remote entities
            var entityManager = StraightFour.StraightFour.ActiveWorld?.entityManager;
            if (entityManager != null)
            {
                foreach (var kvp in _serverToLocalMap)
                {
                    var entity = entityManager.FindEntity(kvp.Value);
                    if (entity != null)
                    {
                        try { entity.Delete(true); } catch { }
                    }
                }
            }

            _serverToLocalMap.Clear();
            _pendingEntities.Clear();
            _pendingTransforms.Clear();
        }

        /// <summary>
        /// Handle a remote entity creation event. Materializes the entity in the local scene.
        /// </summary>
        private void OnRemoteEntityCreated(SyncEntity syncEntity)
        {
            if (_disposed) return;

            // Echo suppression: skip entities we own
            if (syncEntity.OwnerId == _localClientId) return;

            // Skip if we already have this entity
            if (_serverToLocalMap.ContainsKey(syncEntity.EntityId)) return;

            var entityManager = StraightFour.StraightFour.ActiveWorld?.entityManager;
            if (entityManager == null)
            {
                Logging.LogWarning("[WorldSyncSceneHandler] No active world entity manager.");
                return;
            }

            var position = new Vector3(syncEntity.Position.x, syncEntity.Position.y, syncEntity.Position.z);
            var rotation = new Quaternion(
                syncEntity.Rotation.x, syncEntity.Rotation.y,
                syncEntity.Rotation.z, syncEntity.Rotation.w);
            var scale = new Vector3(syncEntity.Scale.x, syncEntity.Scale.y, syncEntity.Scale.z);

            string filePath = null;
            string[] resources = null;
            if (syncEntity.Properties != null)
            {
                if (syncEntity.Properties.TryGetValue("filePath", out var fp))
                    filePath = fp as string;
                if (syncEntity.Properties.TryGetValue("resources", out var res))
                    resources = res as string[];
            }

            string serverEntityId = syncEntity.EntityId;
            _pendingEntities.Add(serverEntityId);

            Guid localId;

            switch (syncEntity.EntityType)
            {
                case WorldSyncEntityTypes.Mesh:
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        localId = WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsMeshEntity(
                            filePath, resources, null,
                            (meshEntity) => OnEntityLoaded(serverEntityId, meshEntity, position, rotation, scale));
                    }
                    else
                    {
                        // No filePath — create a primitive cube as placeholder
                        localId = entityManager.LoadContainerEntity(null, position, rotation, scale,
                            null, syncEntity.EntityTag, false,
                            () => OnEntityLoadedById(serverEntityId, position, rotation, scale));
                    }
                    _serverToLocalMap[serverEntityId] = localId;
                    break;

                case WorldSyncEntityTypes.Character:
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        localId = WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsCharacterEntity(
                            filePath, resources,
                            Vector3.zero, Quaternion.identity, new Vector3(0, 2.5f, 0), null,
                            (charEntity) => OnEntityLoaded(serverEntityId, charEntity, position, rotation, scale));
                    }
                    else
                    {
                        localId = entityManager.LoadCharacterEntity(null, null,
                            Vector3.zero, Quaternion.identity, new Vector3(0, 2.5f, 0),
                            position, rotation, scale, null, syncEntity.EntityTag, false,
                            () => OnEntityLoadedById(serverEntityId, position, rotation, scale));
                    }
                    _serverToLocalMap[serverEntityId] = localId;
                    break;

                case WorldSyncEntityTypes.Light:
                    localId = entityManager.LoadLightEntity(null, position, rotation,
                        null, syncEntity.EntityTag,
                        () => OnEntityLoadedById(serverEntityId, position, rotation, scale));
                    _serverToLocalMap[serverEntityId] = localId;
                    break;

                case WorldSyncEntityTypes.Canvas:
                    localId = entityManager.LoadCanvasEntity(null, position, rotation, scale,
                        null, false, syncEntity.EntityTag,
                        () => OnEntityLoadedById(serverEntityId, position, rotation, scale));
                    _serverToLocalMap[serverEntityId] = localId;
                    break;

                case WorldSyncEntityTypes.Audio:
                    localId = entityManager.LoadAudioEntity(null, position, rotation,
                        null, syncEntity.EntityTag,
                        () => OnEntityLoadedById(serverEntityId, position, rotation, scale));
                    _serverToLocalMap[serverEntityId] = localId;
                    break;

                case WorldSyncEntityTypes.Voxel:
                    localId = entityManager.LoadVoxelEntity(null, position, rotation, scale,
                        null, syncEntity.EntityTag,
                        () => OnEntityLoadedById(serverEntityId, position, rotation, scale));
                    _serverToLocalMap[serverEntityId] = localId;
                    break;

                case WorldSyncEntityTypes.WaterBlocker:
                    localId = entityManager.LoadWaterBlockerEntity(null, position, rotation,
                        null, syncEntity.EntityTag,
                        () => OnEntityLoadedById(serverEntityId, position, rotation, scale));
                    _serverToLocalMap[serverEntityId] = localId;
                    break;

                case WorldSyncEntityTypes.Html:
                    localId = entityManager.LoadHTMLEntity(null, position, rotation, scale,
                        null, false, syncEntity.EntityTag, null,
                        () => OnEntityLoadedById(serverEntityId, position, rotation, scale));
                    _serverToLocalMap[serverEntityId] = localId;
                    break;

                case WorldSyncEntityTypes.Container:
                default:
                    // Container or unknown type — use container as fallback
                    if (syncEntity.EntityType != WorldSyncEntityTypes.Container)
                    {
                        Logging.LogWarning($"[WorldSyncSceneHandler] Unknown entity type '{syncEntity.EntityType}'"
                            + " — falling back to container.");
                    }
                    localId = entityManager.LoadContainerEntity(null, position, rotation, scale,
                        null, syncEntity.EntityTag, false,
                        () => OnEntityLoadedById(serverEntityId, position, rotation, scale));
                    _serverToLocalMap[serverEntityId] = localId;
                    break;
            }

            Logging.Log($"[WorldSyncSceneHandler] Materializing remote entity:"
                + $" serverId={serverEntityId}, type={syncEntity.EntityType},"
                + $" localId={localId}, filePath={filePath ?? "(none)"}");
        }

        /// <summary>
        /// Called when a GLTF entity finishes loading (provides the entity directly).
        /// </summary>
        private void OnEntityLoaded(string serverEntityId, BaseEntity entity,
            Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (_disposed) return;
            _pendingEntities.Remove(serverEntityId);

            if (entity != null)
            {
                entity.SetPosition(position, false, false);
                entity.SetRotation(rotation, false, false);
                entity.SetScale(scale, false);
            }

            ApplyPendingTransform(serverEntityId);
        }

        /// <summary>
        /// Called when a non-GLTF entity finishes loading (look up by stored GUID).
        /// </summary>
        private void OnEntityLoadedById(string serverEntityId,
            Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (_disposed) return;
            _pendingEntities.Remove(serverEntityId);

            if (_serverToLocalMap.TryGetValue(serverEntityId, out var localId))
            {
                var entity = StraightFour.StraightFour.ActiveWorld?.entityManager?.FindEntity(localId);
                if (entity != null)
                {
                    entity.SetPosition(position, false, false);
                    entity.SetRotation(rotation, false, false);
                    entity.SetScale(scale, false);
                }
            }

            ApplyPendingTransform(serverEntityId);
        }

        /// <summary>
        /// Apply any queued transform that arrived while the entity was loading.
        /// </summary>
        private void ApplyPendingTransform(string serverEntityId)
        {
            if (!_pendingTransforms.TryGetValue(serverEntityId, out var pending)) return;
            _pendingTransforms.Remove(serverEntityId);

            if (!_serverToLocalMap.TryGetValue(serverEntityId, out var localId)) return;
            var entity = StraightFour.StraightFour.ActiveWorld?.entityManager?.FindEntity(localId);
            if (entity == null) return;

            if (pending.Position.HasValue)
                entity.SetPosition(new Vector3(pending.Position.Value.x, pending.Position.Value.y,
                    pending.Position.Value.z), false, false);
            if (pending.Rotation.HasValue)
                entity.SetRotation(new Quaternion(pending.Rotation.Value.x, pending.Rotation.Value.y,
                    pending.Rotation.Value.z, pending.Rotation.Value.w), false, false);
            if (pending.Scale.HasValue)
                entity.SetScale(new Vector3(pending.Scale.Value.x, pending.Scale.Value.y,
                    pending.Scale.Value.z), false);
        }

        /// <summary>
        /// Handle remote entity transform update.
        /// </summary>
        private void OnRemoteTransformUpdated(string entityId,
            SyncVector3? position, SyncQuaternion? rotation, SyncVector3? scale)
        {
            if (_disposed) return;

            // If entity is still loading, queue the update
            if (_pendingEntities.Contains(entityId))
            {
                _pendingTransforms[entityId] = new PendingTransform
                {
                    Position = position, Rotation = rotation, Scale = scale
                };
                return;
            }

            if (!_serverToLocalMap.TryGetValue(entityId, out var localId)) return;

            var entity = StraightFour.StraightFour.ActiveWorld?.entityManager?.FindEntity(localId);
            if (entity == null) return;

            if (position.HasValue)
                entity.SetPosition(new Vector3(position.Value.x, position.Value.y, position.Value.z),
                    false, false);
            if (rotation.HasValue)
                entity.SetRotation(new Quaternion(rotation.Value.x, rotation.Value.y,
                    rotation.Value.z, rotation.Value.w), false, false);
            if (scale.HasValue)
                entity.SetScale(new Vector3(scale.Value.x, scale.Value.y, scale.Value.z), false);
        }

        /// <summary>
        /// Handle remote entity deletion.
        /// </summary>
        private void OnRemoteEntityDeleted(string entityId)
        {
            if (_disposed) return;

            _pendingEntities.Remove(entityId);
            _pendingTransforms.Remove(entityId);

            if (!_serverToLocalMap.TryGetValue(entityId, out var localId)) return;
            _serverToLocalMap.Remove(entityId);

            var entity = StraightFour.StraightFour.ActiveWorld?.entityManager?.FindEntity(localId);
            if (entity != null)
            {
                Logging.Log($"[WorldSyncSceneHandler] Deleting remote entity: serverId={entityId}, localId={localId}");
                try { entity.Delete(true); } catch { }
            }
        }
    }
}
#endif
