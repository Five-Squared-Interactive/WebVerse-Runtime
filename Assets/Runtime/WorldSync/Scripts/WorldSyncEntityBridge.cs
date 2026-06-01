// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FiveSQD.StraightFour.Entity;
using FiveSQD.StraightFour.Utilities;
using UnityEngine;

namespace FiveSQD.WebVerse.WorldSync
{
    /// <summary>
    /// Bridges a local StraightFour <see cref="BaseEntity"/> to a WorldSync server-side entity,
    /// forwarding transform changes to the session.
    /// <remarks>
    /// Transform change detection uses polling (no event surface on <see cref="BaseEntity"/>).
    /// Call <see cref="PollTransformChanges"/> from an external Update loop at the desired rate.
    /// The bridge does NOT create its own MonoBehaviour; the caller (JS API fire-and-forget helper)
    /// is responsible for scheduling polls.
    /// </remarks>
    /// </summary>
    public class WorldSyncEntityBridge
    {
        /// <summary>
        /// Maps StraightFour entity concrete types to WorldSync entity type strings.
        /// </summary>
        private static readonly Dictionary<Type, string> EntityTypeMap = new Dictionary<Type, string>
        {
            { typeof(MeshEntity), WorldSyncEntityTypes.Mesh },
            { typeof(CharacterEntity), WorldSyncEntityTypes.Character },
            { typeof(LightEntity), WorldSyncEntityTypes.Light },
            { typeof(ContainerEntity), WorldSyncEntityTypes.Container },
            { typeof(AutomobileEntity), WorldSyncEntityTypes.Automobile },
            { typeof(AirplaneEntity), WorldSyncEntityTypes.Airplane },
            { typeof(AudioEntity), WorldSyncEntityTypes.Audio },
            { typeof(VoxelEntity), WorldSyncEntityTypes.Voxel },
            { typeof(CanvasEntity), WorldSyncEntityTypes.Canvas },
            { typeof(TextEntity), WorldSyncEntityTypes.Text },
            { typeof(ButtonEntity), WorldSyncEntityTypes.Button },
            { typeof(InputEntity), WorldSyncEntityTypes.Input },
            { typeof(ImageEntity), WorldSyncEntityTypes.Image },
            { typeof(WaterBodyEntity), WorldSyncEntityTypes.WaterBody },
            { typeof(TerrainEntity), WorldSyncEntityTypes.Terrain },
            { typeof(HybridTerrainEntity), WorldSyncEntityTypes.HybridTerrain },
            { typeof(WaterBlockerEntity), WorldSyncEntityTypes.WaterBlocker },
            { typeof(HTMLEntity), WorldSyncEntityTypes.Html },
            { typeof(DropdownEntity), WorldSyncEntityTypes.Dropdown },
        };

        /// <summary>
        /// Server-assigned entity ID (set after <see cref="StartAsync"/> completes).
        /// </summary>
        public string ServerEntityId { get; private set; }

        /// <summary>
        /// Local entity's GUID.
        /// </summary>
        public Guid LocalEntityId { get; private set; }

        /// <summary>
        /// Whether to delete the server entity when the bridge stops.
        /// </summary>
        public bool DeleteWithClient { get; private set; }

        /// <summary>
        /// Whether this bridge is actively forwarding.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Test seam: incremented every time a transform change is detected and forwarded.
        /// </summary>
        internal int TestHook_TransformUpdateCount;

        private readonly WorldSyncClient _client;
        private readonly BaseEntity _localEntity;
        private readonly SyncSession _session;
        private readonly string _filePath;
        private readonly string[] _resources;

        // Last-known transform for polling-based change detection.
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Vector3 _lastScale;

        /// <summary>
        /// Create a new entity bridge.
        /// </summary>
        /// <param name="client">WorldSync client owning the session.</param>
        /// <param name="localEntity">Local StraightFour entity to mirror.</param>
        /// <param name="deleteWithClient">Whether to delete the server entity on Stop.</param>
        /// <param name="filePath">Optional file path associated with the entity.</param>
        /// <param name="resources">Optional resources associated with the entity.</param>
        /// <param name="session">Session to create the entity in; defaults to client.CurrentSession.</param>
        public WorldSyncEntityBridge(WorldSyncClient client, BaseEntity localEntity,
            bool deleteWithClient, string filePath = null, string[] resources = null,
            SyncSession session = null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _localEntity = localEntity ?? throw new ArgumentNullException(nameof(localEntity));
            _session = session ?? client.CurrentSession;
            DeleteWithClient = deleteWithClient;
            _filePath = filePath;
            _resources = resources;
            LocalEntityId = localEntity.id;
        }

        /// <summary>
        /// Start the bridge: creates a server-side entity mirroring the local entity.
        /// </summary>
        /// <returns>True if the server entity was created successfully.</returns>
        public async Task<bool> StartAsync()
        {
            return await CreateServerEntityAsync("StartAsync");
        }

        /// <summary>
        /// Suspend the bridge: stops polling but does NOT delete the server entity
        /// or remove the bridge from the client's dictionary.
        /// Call <see cref="ResumeAsync"/> after reconnection to resurrect.
        /// </summary>
        public void Suspend()
        {
            IsActive = false;
        }

        /// <summary>
        /// Resume a suspended bridge: re-creates the server-side entity and
        /// restores polling readiness. The caller is responsible for restarting
        /// the polling coroutine after this returns true.
        /// </summary>
        /// <returns>True if the server entity was re-created successfully; false on failure.</returns>
        public async Task<bool> ResumeAsync()
        {
            return await CreateServerEntityAsync("ResumeAsync");
        }

        /// <summary>
        /// Shared helper that creates a server-side entity. Used by both
        /// <see cref="StartAsync"/> and <see cref="ResumeAsync"/>.
        /// </summary>
        private async Task<bool> CreateServerEntityAsync(string caller)
        {
            if (_session == null || !_session.IsValid)
            {
                LogSystem.LogError("[WorldSyncEntityBridge:" + caller + "] Session is null or invalid.");
                return false;
            }

            // Test seam: simulate server-side entity re-creation failure during resume.
            if (caller == "ResumeAsync" && _client.SimulateResumeEntityFailure)
            {
                LogSystem.LogError("[WorldSyncEntityBridge:" + caller + "] Simulated resume failure.");
                return false;
            }

            try
            {
                string mappedType = MapEntityType(_localEntity);

                var properties = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(_filePath))
                    properties["filePath"] = _filePath;
                if (_resources != null && _resources.Length > 0)
                    properties["resources"] = _resources;

                var entity = new SyncEntity
                {
                    EntityId = Guid.NewGuid().ToString(),
                    OwnerId = _session.LocalClientId,
                    EntityType = mappedType,
                    EntityTag = _localEntity.gameObject != null
                        ? _localEntity.gameObject.name ?? LocalEntityId.ToString()
                        : LocalEntityId.ToString(),
                    Properties = properties
                };

                var created = await _client.CreateEntityAsync(_session, entity);
                ServerEntityId = created.EntityId;
                IsActive = true;

                // Capture current transform for polling (best-effort — test entities
                // may not have fully initialised transforms).
                try
                {
                    _lastPosition = _localEntity.GetPosition(false);
                    _lastRotation = _localEntity.GetRotation(false);
                    _lastScale = _localEntity.GetScale();
                }
                catch (Exception tex)
                {
                    LogSystem.LogWarning("[WorldSyncEntityBridge:" + caller
                        + "] Transform capture failed (non-fatal): " + tex.Message);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogSystem.LogError("[WorldSyncEntityBridge:" + caller + "] " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stop the bridge. Optionally deletes the server-side entity if
        /// <see cref="DeleteWithClient"/> is true.
        /// </summary>
        public void Stop()
        {
            if (!IsActive)
                return;

            IsActive = false;

            if (DeleteWithClient && _session != null && _session.IsValid
                && !string.IsNullOrEmpty(ServerEntityId))
            {
                try
                {
                    _session.DeleteEntity(ServerEntityId);
                }
                catch (Exception ex)
                {
                    LogSystem.LogError("[WorldSyncEntityBridge:Stop] Delete failed: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Poll the local entity's transform and forward changes to the session.
        /// Call this from an Update loop at the desired rate (e.g. 20 Hz).
        /// </summary>
        public void PollTransformChanges()
        {
            if (!IsActive || _localEntity == null || _session == null || !_session.IsValid
                || string.IsNullOrEmpty(ServerEntityId))
                return;

            try
            {
                Vector3 currentPos = _localEntity.GetPosition(false);
                Quaternion currentRot = _localEntity.GetRotation(false);
                Vector3 currentScale = _localEntity.GetScale();

                bool changed = false;

                if (currentPos != _lastPosition)
                {
                    _session.UpdateEntityPosition(ServerEntityId,
                        new SyncVector3 { x = currentPos.x, y = currentPos.y, z = currentPos.z });
                    _lastPosition = currentPos;
                    changed = true;
                }

                if (currentRot != _lastRotation)
                {
                    _session.UpdateEntityRotation(ServerEntityId,
                        new SyncQuaternion { x = currentRot.x, y = currentRot.y, z = currentRot.z, w = currentRot.w });
                    _lastRotation = currentRot;
                    changed = true;
                }

                if (currentScale != _lastScale)
                {
                    _session.UpdateEntityScale(ServerEntityId,
                        new SyncVector3 { x = currentScale.x, y = currentScale.y, z = currentScale.z });
                    _lastScale = currentScale;
                    changed = true;
                }

                if (changed)
                {
                    TestHook_TransformUpdateCount++;
                }
            }
            catch (Exception ex)
            {
                LogSystem.LogError("[WorldSyncEntityBridge:PollTransformChanges] " + ex.Message);
            }
        }

        /// <summary>
        /// Map a StraightFour entity's concrete type to a WorldSync entity type string.
        /// Returns "container" with a log warning for unknown types.
        /// </summary>
        internal static string MapEntityType(BaseEntity entity)
        {
            if (entity == null)
                return WorldSyncEntityTypes.Container;

            Type entityType = entity.GetType();
            if (EntityTypeMap.TryGetValue(entityType, out string wsyncType))
                return wsyncType;

            LogSystem.LogWarning("[WorldSyncEntityBridge:MapEntityType] Unknown entity type '"
                + entityType.Name + "'; falling back to container.");
            return WorldSyncEntityTypes.Container;
        }
    }
}
