// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.WorldSync
{
    /// <summary>
    /// Represents a synchronized session.
    /// Provides object-oriented access to session operations.
    /// </summary>
    public class SyncSession
    {
        /// <summary>
        /// Unique session identifier.
        /// </summary>
        public string SessionId { get; private set; }

        /// <summary>
        /// Human-readable session tag.
        /// </summary>
        public string SessionTag { get; private set; }

        /// <summary>
        /// Timestamp when session was created.
        /// </summary>
        public string CreatedAt { get; private set; }

        /// <summary>
        /// Client ID of the local client.
        /// </summary>
        public string LocalClientId { get; private set; }

        /// <summary>
        /// Whether this session is still valid.
        /// </summary>
        public bool IsValid { get; private set; } = true;

        /// <summary>
        /// Current environment state.
        /// </summary>
        public EnvironmentState Environment { get; private set; }

        /// <summary>
        /// Event raised when an entity is created.
        /// </summary>
        public event Action<SyncEntity> OnEntityCreated;

        /// <summary>
        /// Event raised when an entity is deleted.
        /// </summary>
        public event Action<string> OnEntityDeleted;

        /// <summary>
        /// Event raised when an entity's transform is updated.
        /// </summary>
        public event Action<string, SyncVector3?, SyncQuaternion?, SyncVector3?> OnEntityTransformUpdated;

        /// <summary>
        /// Event raised when an entity's state changes.
        /// </summary>
        public event Action<SyncEntity> OnEntityStateChanged;

        /// <summary>
        /// Event raised when a client joins.
        /// </summary>
        public event Action<SyncClient> OnClientJoined;

        /// <summary>
        /// Event raised when a client leaves.
        /// </summary>
        public event Action<string, string> OnClientLeft;

        /// <summary>
        /// Event raised when environment changes.
        /// </summary>
        public event Action<EnvironmentState> OnEnvironmentChanged;

        /// <summary>
        /// Event raised when session is destroyed.
        /// </summary>
        public event Action<string> OnSessionDestroyed;

        /// <summary>
        /// Reference to the parent WorldSync client.
        /// </summary>
        private readonly WorldSyncClient _client;

        /// <summary>
        /// Dictionary of entities by ID.
        /// </summary>
        private readonly Dictionary<string, SyncEntity> _entities = new Dictionary<string, SyncEntity>();

        /// <summary>
        /// Dictionary of clients by ID.
        /// </summary>
        private readonly Dictionary<string, SyncClient> _clients = new Dictionary<string, SyncClient>();

        /// <summary>
        /// Lock for thread-safe operations.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Collection of entities in this session.
        /// </summary>
        public IReadOnlyDictionary<string, SyncEntity> Entities
        {
            get
            {
                lock (_lock)
                {
                    return new Dictionary<string, SyncEntity>(_entities);
                }
            }
        }

        /// <summary>
        /// Collection of clients in this session.
        /// </summary>
        public IReadOnlyDictionary<string, SyncClient> Clients
        {
            get
            {
                lock (_lock)
                {
                    return new Dictionary<string, SyncClient>(_clients);
                }
            }
        }

        /// <summary>
        /// Number of entities in this session.
        /// </summary>
        public int EntityCount
        {
            get
            {
                lock (_lock)
                {
                    return _entities.Count;
                }
            }
        }

        /// <summary>
        /// Number of clients in this session.
        /// </summary>
        public int ClientCount
        {
            get
            {
                lock (_lock)
                {
                    return _clients.Count;
                }
            }
        }

        /// <summary>
        /// Create a new SyncSession.
        /// </summary>
        internal SyncSession(WorldSyncClient client, string sessionId, string sessionTag,
            string createdAt, string localClientId)
        {
            _client = client;
            SessionId = sessionId;
            SessionTag = sessionTag;
            CreatedAt = createdAt;
            LocalClientId = localClientId;
            Environment = new EnvironmentState();
        }

        /// <summary>
        /// Initialize session with full state.
        /// </summary>
        internal void InitializeState(SessionState state)
        {
            lock (_lock)
            {
                _entities.Clear();
                _clients.Clear();

                if (state.Entities != null)
                {
                    foreach (var entity in state.Entities)
                    {
                        _entities[entity.EntityId] = entity;
                    }
                }

                if (state.Clients != null)
                {
                    foreach (var client in state.Clients)
                    {
                        _clients[client.ClientId] = client;
                    }
                }

                Environment = state.Environment ?? new EnvironmentState();
            }
        }

        /// <summary>
        /// Create a new entity in this session.
        /// </summary>
        /// <param name="entityType">Entity type (mesh, avatar, light, etc.).</param>
        /// <param name="tag">Human-readable entity tag.</param>
        /// <param name="properties">Type-specific properties.</param>
        /// <returns>Created entity.</returns>
        public SyncEntity CreateEntity(string entityType, string tag,
            Dictionary<string, object> properties = null)
        {
            EnsureValid();

            var entityId = Guid.NewGuid().ToString();
            var entity = new SyncEntity
            {
                EntityId = entityId,
                OwnerId = LocalClientId,
                EntityType = entityType,
                EntityTag = tag,
                Properties = properties ?? new Dictionary<string, object>()
            };

            return _client.CreateEntityAsync(this, entity).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get an entity by ID.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <returns>Entity or null if not found.</returns>
        public SyncEntity GetEntity(string entityId)
        {
            lock (_lock)
            {
                return _entities.TryGetValue(entityId, out var entity) ? entity : null;
            }
        }

        /// <summary>
        /// Check if an entity exists.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public bool HasEntity(string entityId)
        {
            lock (_lock)
            {
                return _entities.ContainsKey(entityId);
            }
        }

        /// <summary>
        /// Delete an entity from this session.
        /// </summary>
        /// <param name="entityId">Entity ID to delete.</param>
        public void DeleteEntity(string entityId)
        {
            EnsureValid();
            _client.DeleteEntityAsync(this, entityId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Update entity position.
        /// </summary>
        public void UpdateEntityPosition(string entityId, SyncVector3 position)
        {
            EnsureValid();
            _client.UpdateEntityPositionAsync(this, entityId, position).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Update entity rotation.
        /// </summary>
        public void UpdateEntityRotation(string entityId, SyncQuaternion rotation)
        {
            EnsureValid();
            _client.UpdateEntityRotationAsync(this, entityId, rotation).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Update entity scale.
        /// </summary>
        public void UpdateEntityScale(string entityId, SyncVector3 scale)
        {
            EnsureValid();
            _client.UpdateEntityScaleAsync(this, entityId, scale).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Set entity parent.
        /// </summary>
        public void SetEntityParent(string childId, string parentId)
        {
            EnsureValid();
            _client.SetEntityParentAsync(this, childId, parentId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Set entity visibility.
        /// </summary>
        public void SetEntityVisibility(string entityId, bool visible)
        {
            EnsureValid();
            _client.SetEntityVisibilityAsync(this, entityId, visible).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get entities owned by a specific client.
        /// </summary>
        public List<SyncEntity> GetEntitiesByOwner(string ownerId)
        {
            lock (_lock)
            {
                var result = new List<SyncEntity>();
                foreach (var entity in _entities.Values)
                {
                    if (entity.OwnerId == ownerId)
                    {
                        result.Add(entity);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Get entities owned by the local client.
        /// </summary>
        public List<SyncEntity> GetOwnedEntities()
        {
            return GetEntitiesByOwner(LocalClientId);
        }

        /// <summary>
        /// Leave this session gracefully.
        /// </summary>
        public void Leave()
        {
            EnsureValid();
            _client.LeaveSessionAsync(this).GetAwaiter().GetResult();
            Invalidate("left");
        }

        /// <summary>
        /// Invalidate this session.
        /// </summary>
        internal void Invalidate(string reason)
        {
            if (!IsValid) return;

            IsValid = false;
            OnSessionDestroyed?.Invoke(reason);
        }

        /// <summary>
        /// Handle entity created event from server.
        /// </summary>
        internal void HandleEntityCreated(SyncEntity entity)
        {
            lock (_lock)
            {
                _entities[entity.EntityId] = entity;
            }
            OnEntityCreated?.Invoke(entity);
        }

        /// <summary>
        /// Handle entity deleted event from server.
        /// </summary>
        internal void HandleEntityDeleted(string entityId)
        {
            lock (_lock)
            {
                _entities.Remove(entityId);
            }
            OnEntityDeleted?.Invoke(entityId);
        }

        /// <summary>
        /// Handle entity transform update from server.
        /// </summary>
        internal void HandleEntityTransform(string entityId, SyncVector3? position,
            SyncQuaternion? rotation, SyncVector3? scale)
        {
            lock (_lock)
            {
                if (_entities.TryGetValue(entityId, out var entity))
                {
                    if (position.HasValue) entity.Position = position.Value;
                    if (rotation.HasValue) entity.Rotation = rotation.Value;
                    if (scale.HasValue) entity.Scale = scale.Value;
                }
            }
            OnEntityTransformUpdated?.Invoke(entityId, position, rotation, scale);
        }

        /// <summary>
        /// Handle entity state change from server.
        /// </summary>
        internal void HandleEntityStateChange(SyncEntity updatedEntity)
        {
            lock (_lock)
            {
                if (_entities.ContainsKey(updatedEntity.EntityId))
                {
                    _entities[updatedEntity.EntityId] = updatedEntity;
                }
            }
            OnEntityStateChanged?.Invoke(updatedEntity);
        }

        /// <summary>
        /// Handle client joined event from server.
        /// </summary>
        internal void HandleClientJoined(SyncClient client)
        {
            lock (_lock)
            {
                _clients[client.ClientId] = client;
            }
            OnClientJoined?.Invoke(client);
        }

        /// <summary>
        /// Handle client left event from server.
        /// </summary>
        internal void HandleClientLeft(string clientId, string reason)
        {
            lock (_lock)
            {
                _clients.Remove(clientId);
            }
            OnClientLeft?.Invoke(clientId, reason);
        }

        /// <summary>
        /// Handle environment change from server.
        /// </summary>
        internal void HandleEnvironmentChange(EnvironmentState environment)
        {
            Environment = environment;
            OnEnvironmentChanged?.Invoke(environment);
        }

        /// <summary>
        /// Ensure session is still valid.
        /// </summary>
        private void EnsureValid()
        {
            if (!IsValid)
            {
                throw new WorldSyncException(
                    WorldSyncErrorCode.SessionNotFound,
                    "Session is no longer valid"
                );
            }
        }
    }
}
