// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if USE_WEBINTERFACE
using Newtonsoft.Json.Linq;
using FiveSQD.WebVerse.WebInterface.MQTT;
#endif

namespace FiveSQD.WebVerse.WorldSync
{
    /// <summary>
    /// Connection state of the WorldSync client.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Not connected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Connection in progress.
        /// </summary>
        Connecting,

        /// <summary>
        /// Connected and ready.
        /// </summary>
        Connected,

        /// <summary>
        /// Reconnecting after connection loss.
        /// </summary>
        Reconnecting,

        /// <summary>
        /// Disconnecting.
        /// </summary>
        Disconnecting
    }

    /// <summary>
    /// Pending operation for queue during disconnection.
    /// </summary>
    internal class PendingOperation
    {
        public string Type { get; set; }
        public string SessionId { get; set; }
        public object Payload { get; set; }
        public TaskCompletionSource<object> Completion { get; set; }
        public DateTime QueuedAt { get; set; }
    }

    /// <summary>
    /// Named payload for entity property updates (position, rotation, scale, visibility, highlight).
    /// </summary>
    internal class QueuedEntityUpdatePayload
    {
        public string EntityId { get; set; }
        public object Value { get; set; }
    }

    /// <summary>
    /// Named payload for parent updates.
    /// </summary>
    internal class QueuedParentUpdatePayload
    {
        public string ChildId { get; set; }
        public string ParentId { get; set; }
    }

    /// <summary>
    /// Named payload for custom messages.
    /// </summary>
    internal class QueuedMessagePayload
    {
        public string Topic { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// WorldSync client for Unity.
    /// Provides real-time entity synchronization with auto-reconnect.
    /// </summary>
    public class WorldSyncClient
    {
        /// <summary>
        /// Client configuration.
        /// </summary>
        public WorldSyncConfig Config { get; private set; }

        /// <summary>
        /// Current connection state.
        /// </summary>
        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

        /// <summary>
        /// Whether the client is currently connected.
        /// </summary>
        public bool IsConnected => State == ConnectionState.Connected;

        /// <summary>
        /// Current active session (if any).
        /// </summary>
        public SyncSession CurrentSession { get; private set; }

        /// <summary>
        /// Number of pending operations in queue.
        /// </summary>
        public int PendingOperationCount => _operationQueue.Count;

        /// <summary>
        /// Event raised when connected to server.
        /// </summary>
        public event Action OnConnected;

        /// <summary>
        /// Event raised when disconnected from server.
        /// </summary>
        public event Action<string> OnDisconnected;

        /// <summary>
        /// Event raised when reconnection starts.
        /// </summary>
        public event Action<int> OnReconnecting;

        /// <summary>
        /// Event raised when reconnection succeeds.
        /// </summary>
        public event Action OnReconnected;

        /// <summary>
        /// Event raised when reconnection fails.
        /// </summary>
        public event Action<int> OnReconnectionFailed;

        /// <summary>
        /// Event raised when state recovery completes.
        /// </summary>
        public event Action OnStateRecovered;

        /// <summary>
        /// Event raised when session expires during reconnection.
        /// </summary>
        public event Action<string> OnSessionExpired;

        /// <summary>
        /// Event raised on any error.
        /// </summary>
        public event Action<WorldSyncException> OnError;

        /// <summary>
        /// Maximum pending operations before dropping oldest.
        /// </summary>
        public int MaxPendingOperations { get; set; } = 100;

        // Internal state
        private readonly Queue<PendingOperation> _operationQueue = new Queue<PendingOperation>();
        private readonly object _queueLock = new object();
        private int _reconnectAttempt = 0;
        private string _lastSessionId;
        private bool _wasInSession;
        private bool _intentionalDisconnect;
        private TaskCompletionSource<bool> _connectTcs;

        /// <summary>
        /// When true, ConnectInternalAsync simulates a connection failure.
        /// </summary>
        internal bool SimulateConnectionFailure;

        /// <summary>
        /// When true, bypasses real MQTT and uses test hook paths for all operations.
        /// </summary>
        internal bool UseTestHooks;

#if USE_WEBINTERFACE
        private MQTTClient _mqttClient;
#endif

        // Request-response infrastructure
        private readonly Dictionary<string, TaskCompletionSource<string>> _pendingRequests
            = new Dictionary<string, TaskCompletionSource<string>>();
        private readonly object _requestLock = new object();

        // Test hooks for session and entity operations
        internal string SimulateCreateSessionId;
        internal SessionState SimulateJoinSessionState;
        internal bool SimulateRequestTimeout;
        internal WorldSyncErrorCode? SimulateServerError;
        internal string SimulateCreateEntityId;

        // Test seam for JS API tests where the client is created internally.
        // Newly-constructed WorldSyncClient instances inherit these defaults.
        internal static bool DefaultUseTestHooks;
        internal static string DefaultSimulateCreateSessionId;
        internal static SessionState DefaultSimulateJoinSessionState;
        internal static string DefaultSimulateCreateEntityId;
        internal static int DefaultSimulateSendCustomMessageInvocations;
        internal static int DefaultSimulateDeleteEntityInvocations;
        internal static bool DefaultSimulateResumeEntityFailure;

        // Instance-level test counters (copied from static defaults in ctor).
        internal int SimulateSendCustomMessageInvocations;
        internal int SimulateDeleteEntityInvocations;
        internal bool SimulateResumeEntityFailure;

        // Entity bridge tracking.
        private readonly Dictionary<Guid, WorldSyncEntityBridge> _entityBridges
            = new Dictionary<Guid, WorldSyncEntityBridge>();
        private readonly object _bridgeLock = new object();

        /// <summary>
        /// Create a new WorldSync client.
        /// </summary>
        /// <param name="config">Client configuration.</param>
        public WorldSyncClient(WorldSyncConfig config)
        {
            config.Validate();
            Config = config;
            UseTestHooks = DefaultUseTestHooks;
            SimulateCreateSessionId = DefaultSimulateCreateSessionId;
            SimulateJoinSessionState = DefaultSimulateJoinSessionState;
            SimulateCreateEntityId = DefaultSimulateCreateEntityId;
            SimulateSendCustomMessageInvocations = DefaultSimulateSendCustomMessageInvocations;
            SimulateDeleteEntityInvocations = DefaultSimulateDeleteEntityInvocations;
            SimulateResumeEntityFailure = DefaultSimulateResumeEntityFailure;
        }

        /// <summary>
        /// Try to register an entity bridge. Returns false if the local entity is already bridged.
        /// </summary>
        public bool TryAddEntityBridge(Guid localEntityId, WorldSyncEntityBridge bridge)
        {
            lock (_bridgeLock)
            {
                if (_entityBridges.ContainsKey(localEntityId))
                    return false;
                _entityBridges[localEntityId] = bridge;
                return true;
            }
        }

        /// <summary>
        /// Remove and return an entity bridge. Returns null if not found.
        /// </summary>
        public WorldSyncEntityBridge TryRemoveEntityBridge(Guid localEntityId)
        {
            lock (_bridgeLock)
            {
                if (_entityBridges.TryGetValue(localEntityId, out var bridge))
                {
                    _entityBridges.Remove(localEntityId);
                    return bridge;
                }
                return null;
            }
        }

        /// <summary>
        /// Check if a local entity is currently bridged.
        /// </summary>
        public bool HasBridgeFor(Guid localEntityId)
        {
            lock (_bridgeLock)
            {
                return _entityBridges.ContainsKey(localEntityId);
            }
        }

        /// <summary>
        /// Stop and remove all entity bridges.
        /// </summary>
        public void ClearEntityBridges()
        {
            lock (_bridgeLock)
            {
                foreach (var bridge in _entityBridges.Values)
                {
                    bridge.Stop();
                }
                _entityBridges.Clear();
            }
        }

        /// <summary>
        /// Event raised for each bridge that is successfully resumed after reconnection.
        /// The subscriber (JS API layer) is responsible for restarting the polling coroutine.
        /// </summary>
        public event Action<WorldSyncEntityBridge> OnBridgeResumed;

        /// <summary>
        /// Suspend all entity bridges — stops polling but preserves the dictionary.
        /// Called on unexpected disconnect before auto-reconnect.
        /// </summary>
        internal void SuspendBridges()
        {
            lock (_bridgeLock)
            {
                foreach (var bridge in _entityBridges.Values)
                {
                    bridge.Suspend();
                }
            }
        }

        /// <summary>
        /// Resume all suspended entity bridges — re-creates server entities and
        /// fires <see cref="OnBridgeResumed"/> for each successful bridge.
        /// Removes bridges that fail to resume.
        /// </summary>
        internal async Task ResumeBridgesAsync()
        {
            List<KeyValuePair<Guid, WorldSyncEntityBridge>> bridgesCopy;
            lock (_bridgeLock)
            {
                bridgesCopy = new List<KeyValuePair<Guid, WorldSyncEntityBridge>>(_entityBridges);
            }

            var failedIds = new List<Guid>();

            foreach (var kvp in bridgesCopy)
            {
                bool resumed = await kvp.Value.ResumeAsync();
                if (resumed)
                {
                    OnBridgeResumed?.Invoke(kvp.Value);
                }
                else
                {
                    Debug.LogWarning("[WorldSync] Bridge failed to resume for entity: " + kvp.Key);
                    failedIds.Add(kvp.Key);
                }
            }

            if (failedIds.Count > 0)
            {
                lock (_bridgeLock)
                {
                    foreach (var id in failedIds)
                    {
                        _entityBridges.Remove(id);
                    }
                }
            }
        }

        /// <summary>
        /// Connect to the WorldSync server.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (State != ConnectionState.Disconnected)
            {
                throw new WorldSyncException(
                    WorldSyncErrorCode.InvalidConfig,
                    $"Cannot connect while in state: {State}"
                );
            }

            State = ConnectionState.Connecting;

            try
            {
                await ConnectInternalAsync();
                State = ConnectionState.Connected;
                _reconnectAttempt = 0;
                OnConnected?.Invoke();
            }
            catch (Exception ex)
            {
                State = ConnectionState.Disconnected;
                var error = ex as WorldSyncException ??
                    WorldSyncException.ConnectionFailed(ex.Message);
                OnError?.Invoke(error);
                throw error;
            }
        }

        /// <summary>
        /// Disconnect from the WorldSync server.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (State == ConnectionState.Disconnected)
            {
                return;
            }

            State = ConnectionState.Disconnecting;

            try
            {
                // Clean up entity bridges before disconnecting.
                ClearEntityBridges();

                // Leave current session if any
                if (CurrentSession != null && CurrentSession.IsValid)
                {
                    try
                    {
                        await LeaveSessionAsync(CurrentSession);
                    }
                    catch
                    {
                        // Ignore errors during disconnect
                    }
                }

                await DisconnectInternalAsync();
            }
            finally
            {
                State = ConnectionState.Disconnected;
                CurrentSession = null;
                OnDisconnected?.Invoke("user_disconnect");
            }
        }

        /// <summary>
        /// Create a new session.
        /// </summary>
        /// <param name="tag">Session tag/name.</param>
        /// <returns>Created session.</returns>
        public async Task<SyncSession> CreateSessionAsync(string tag)
        {
            EnsureConnected();

            if (UseTestHooks)
            {
                if (SimulateRequestTimeout)
                    throw new WorldSyncException(WorldSyncErrorCode.RequestTimeout, "Request timed out");
                if (SimulateServerError.HasValue)
                    throw new WorldSyncException(SimulateServerError.Value, "Server error");

                var sid = SimulateCreateSessionId ?? Guid.NewGuid().ToString();
                var cat = DateTime.UtcNow.ToString("o");
                var sess = new SyncSession(this, sid, tag, cat, Config.ClientId);
                CurrentSession = sess;
                _lastSessionId = sid;
                _wasInSession = true;
                await Task.CompletedTask;
                return sess;
            }

#if USE_WEBINTERFACE
            var messageId = Guid.NewGuid().ToString();
            var json = $"{{\"message-id\":\"{messageId}\",\"client-id\":\"{Config.ClientId}\",\"client-tag\":\"{Config.ClientTag}\",\"tag\":\"{tag}\"}}";
            var response = await PublishAndWaitAsync("wsync/session/create", json, messageId, Config.ConnectTimeoutMs);

            var responseObj = JObject.Parse(response);
            var sessionId = responseObj.Value<string>("session-id");
            var createdAt = responseObj.Value<string>("created-at");
            if (string.IsNullOrEmpty(sessionId))
                throw new WorldSyncException(WorldSyncErrorCode.InvalidPayload, "Server response missing session-id");

            var session = new SyncSession(this, sessionId, tag, createdAt ?? DateTime.UtcNow.ToString("o"), Config.ClientId);
            SubscribeToSessionStatusTopics(sessionId);
            CurrentSession = session;
            _lastSessionId = sessionId;
            _wasInSession = true;
            return session;
#else
            if (SimulateRequestTimeout)
                throw new WorldSyncException(WorldSyncErrorCode.RequestTimeout, "Request timed out");
            if (SimulateServerError.HasValue)
                throw new WorldSyncException(SimulateServerError.Value, "Server error");

            var sessionId = SimulateCreateSessionId ?? Guid.NewGuid().ToString();
            var createdAt = DateTime.UtcNow.ToString("o");
            var session = new SyncSession(this, sessionId, tag, createdAt, Config.ClientId);
            CurrentSession = session;
            _lastSessionId = sessionId;
            _wasInSession = true;
            await Task.CompletedTask;
            return session;
#endif
        }

        /// <summary>
        /// Join an existing session.
        /// </summary>
        /// <param name="sessionId">Session ID to join.</param>
        /// <returns>Joined session with full state.</returns>
        public async Task<SyncSession> JoinSessionAsync(string sessionId)
        {
            EnsureConnected();

            if (UseTestHooks)
            {
                if (SimulateRequestTimeout)
                    throw new WorldSyncException(WorldSyncErrorCode.RequestTimeout, "Request timed out");
                if (SimulateServerError.HasValue)
                    throw new WorldSyncException(SimulateServerError.Value, "Server error");

                var st = SimulateJoinSessionState;
                var tg = st?.SessionTag ?? "unknown";
                var ca = st?.CreatedAt ?? DateTime.UtcNow.ToString("o");
                var sess = new SyncSession(this, sessionId, tg, ca, Config.ClientId);
                if (st != null)
                {
                    sess.InitializeState(st);
                }
                CurrentSession = sess;
                _lastSessionId = sessionId;
                _wasInSession = true;
                await Task.CompletedTask;
                return sess;
            }

#if USE_WEBINTERFACE
            var messageId = Guid.NewGuid().ToString();
            var json = $"{{\"message-id\":\"{messageId}\",\"client-id\":\"{Config.ClientId}\",\"client-tag\":\"{Config.ClientTag}\"}}";
            var response = await PublishAndWaitAsync($"wsync/session/{sessionId}/join", json, messageId, Config.ConnectTimeoutMs);

            var responseObj = JObject.Parse(response);
            var tag = responseObj.Value<string>("tag") ?? "unknown";
            var createdAt = responseObj.Value<string>("created-at") ?? DateTime.UtcNow.ToString("o");

            var session = new SyncSession(this, sessionId, tag, createdAt, Config.ClientId);
            var state = ParseSessionState(responseObj, sessionId);
            if (state != null)
            {
                session.InitializeState(state);
            }

            SubscribeToSessionStatusTopics(sessionId);
            CurrentSession = session;
            _lastSessionId = sessionId;
            _wasInSession = true;
            return session;
#else
            if (SimulateRequestTimeout)
                throw new WorldSyncException(WorldSyncErrorCode.RequestTimeout, "Request timed out");
            if (SimulateServerError.HasValue)
                throw new WorldSyncException(SimulateServerError.Value, "Server error");

            var state = SimulateJoinSessionState;
            var tag = state?.SessionTag ?? "unknown";
            var createdAt = state?.CreatedAt ?? DateTime.UtcNow.ToString("o");
            var session = new SyncSession(this, sessionId, tag, createdAt, Config.ClientId);
            if (state != null)
            {
                session.InitializeState(state);
            }
            CurrentSession = session;
            _lastSessionId = sessionId;
            _wasInSession = true;
            await Task.CompletedTask;
            return session;
#endif
        }

        /// <summary>
        /// Leave the current session.
        /// </summary>
        internal async Task LeaveSessionAsync(SyncSession session)
        {
            if (session == null || !session.IsValid)
            {
                return;
            }

#if USE_WEBINTERFACE
            if (!UseTestHooks)
            {
                var messageId = Guid.NewGuid().ToString();
                var json = $"{{\"message-id\":\"{messageId}\",\"client-id\":\"{Config.ClientId}\",\"session-id\":\"{session.SessionId}\"}}";
                _mqttClient?.Publish($"wsync/session/{session.SessionId}/exit", json);
                UnsubscribeFromSessionStatusTopics(session.SessionId);
            }
#endif

            session.Invalidate("left");

            if (CurrentSession == session)
            {
                CurrentSession = null;
                _wasInSession = false;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Destroy a session (owner only).
        /// </summary>
        internal async Task DestroySessionAsync(SyncSession session)
        {
            if (session == null || !session.IsValid)
            {
                return;
            }

            EnsureConnected();

            if (UseTestHooks)
            {
                if (SimulateRequestTimeout)
                    throw new WorldSyncException(WorldSyncErrorCode.RequestTimeout, "Request timed out");
                if (SimulateServerError.HasValue)
                    throw new WorldSyncException(SimulateServerError.Value, "Server error");
            }
            else
            {
#if USE_WEBINTERFACE
                var messageId = Guid.NewGuid().ToString();
                var json = $"{{\"message-id\":\"{messageId}\",\"client-id\":\"{Config.ClientId}\",\"session-id\":\"{session.SessionId}\"}}";
                await PublishAndWaitAsync($"wsync/session/{session.SessionId}/destroy", json, messageId, Config.ConnectTimeoutMs);
                UnsubscribeFromSessionStatusTopics(session.SessionId);
#endif
            }

            session.Invalidate("destroyed");

            if (CurrentSession == session)
            {
                CurrentSession = null;
                _wasInSession = false;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Create an entity in a session.
        /// </summary>
        internal async Task<SyncEntity> CreateEntityAsync(SyncSession session, SyncEntity entity)
        {
            if (!IsConnected)
            {
                return QueueOrThrow<SyncEntity>("entity.create", session.SessionId, entity);
            }

            // Validate and normalize entity type (AC6: unknown falls back to container, null/empty throws)
            entity.EntityType = WorldSyncEntityTypes.GetFallbackType(entity.EntityType);

            if (UseTestHooks)
            {
                if (SimulateRequestTimeout)
                    throw new WorldSyncException(WorldSyncErrorCode.RequestTimeout, "Request timed out");
                if (SimulateServerError.HasValue)
                    throw new WorldSyncException(SimulateServerError.Value, "Server error");

                entity.EntityId = SimulateCreateEntityId ?? entity.EntityId ?? Guid.NewGuid().ToString();
                session.HandleEntityCreated(entity);
                await Task.CompletedTask;
                return entity;
            }

#if USE_WEBINTERFACE
            var messageId = Guid.NewGuid().ToString();
            var obj = new JObject
            {
                ["message-id"] = messageId,
                ["client-id"] = Config.ClientId,
                ["entity-id"] = entity.EntityId ?? Guid.NewGuid().ToString(),
                ["entity-type"] = entity.EntityType,
                ["entity-tag"] = entity.EntityTag ?? "",
                ["position"] = new JObject { ["x"] = entity.Position.x, ["y"] = entity.Position.y, ["z"] = entity.Position.z },
                ["rotation"] = new JObject { ["x"] = entity.Rotation.x, ["y"] = entity.Rotation.y, ["z"] = entity.Rotation.z, ["w"] = entity.Rotation.w },
                ["scale"] = new JObject { ["x"] = entity.Scale.x, ["y"] = entity.Scale.y, ["z"] = entity.Scale.z },
                ["visible"] = entity.Visible
            };
            if (!string.IsNullOrEmpty(entity.ParentId))
                obj["parent-id"] = entity.ParentId;
            if (entity.Properties != null && entity.Properties.Count > 0)
            {
                var props = new JObject();
                foreach (var kvp in entity.Properties)
                    props[kvp.Key] = JToken.FromObject(kvp.Value);
                obj["properties"] = props;
            }
            var json = obj.ToString(Newtonsoft.Json.Formatting.None);
            var response = await PublishAndWaitAsync(
                $"wsync/request/{session.SessionId}/entity/create", json, messageId, Config.ConnectTimeoutMs);

            var responseObj = JObject.Parse(response);
            var confirmedId = responseObj.Value<string>("entity-id");
            if (!string.IsNullOrEmpty(confirmedId))
                entity.EntityId = confirmedId;

            session.HandleEntityCreated(entity);
            return entity;
#endif
        }

        /// <summary>
        /// Delete an entity from a session.
        /// </summary>
        internal async Task DeleteEntityAsync(SyncSession session, string entityId)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.delete", session.SessionId, entityId);
                return;
            }

            if (UseTestHooks)
            {
                if (SimulateRequestTimeout)
                    throw new WorldSyncException(WorldSyncErrorCode.RequestTimeout, "Request timed out");
                if (SimulateServerError.HasValue)
                    throw new WorldSyncException(SimulateServerError.Value, "Server error");

                SimulateDeleteEntityInvocations++;
                session.HandleEntityDeleted(entityId);
                await Task.CompletedTask;
                return;
            }

#if USE_WEBINTERFACE
            var messageId = Guid.NewGuid().ToString();
            var json = $"{{\"message-id\":\"{messageId}\",\"client-id\":\"{Config.ClientId}\"}}";
            await PublishAndWaitAsync(
                $"wsync/request/{session.SessionId}/entity/{entityId}/delete", json, messageId, Config.ConnectTimeoutMs);
            session.HandleEntityDeleted(entityId);
#endif
        }

        /// <summary>
        /// Update entity position.
        /// </summary>
        internal async Task UpdateEntityPositionAsync(SyncSession session, string entityId, SyncVector3 position)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.position", session.SessionId,
                    new QueuedEntityUpdatePayload { EntityId = entityId, Value = position });
                return;
            }

            if (!UseTestHooks)
            {
#if USE_WEBINTERFACE
                var messageId = Guid.NewGuid().ToString();
                var json = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{\"message-id\":\"{0}\",\"client-id\":\"{1}\",\"position\":{{\"x\":{2},\"y\":{3},\"z\":{4}}}}}",
                    messageId, Config.ClientId, position.x, position.y, position.z);
                _mqttClient.Publish($"wsync/request/{session.SessionId}/entity/{entityId}/position", json);
#endif
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Update entity rotation.
        /// </summary>
        internal async Task UpdateEntityRotationAsync(SyncSession session, string entityId, SyncQuaternion rotation)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.rotation", session.SessionId,
                    new QueuedEntityUpdatePayload { EntityId = entityId, Value = rotation });
                return;
            }

            if (!UseTestHooks)
            {
#if USE_WEBINTERFACE
                var messageId = Guid.NewGuid().ToString();
                var json = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{\"message-id\":\"{0}\",\"client-id\":\"{1}\",\"rotation\":{{\"x\":{2},\"y\":{3},\"z\":{4},\"w\":{5}}}}}",
                    messageId, Config.ClientId, rotation.x, rotation.y, rotation.z, rotation.w);
                _mqttClient.Publish($"wsync/request/{session.SessionId}/entity/{entityId}/rotation", json);
#endif
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Update entity scale.
        /// </summary>
        internal async Task UpdateEntityScaleAsync(SyncSession session, string entityId, SyncVector3 scale)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.scale", session.SessionId,
                    new QueuedEntityUpdatePayload { EntityId = entityId, Value = scale });
                return;
            }

            if (!UseTestHooks)
            {
#if USE_WEBINTERFACE
                var messageId = Guid.NewGuid().ToString();
                var json = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{{\"message-id\":\"{0}\",\"client-id\":\"{1}\",\"scale\":{{\"x\":{2},\"y\":{3},\"z\":{4}}}}}",
                    messageId, Config.ClientId, scale.x, scale.y, scale.z);
                _mqttClient.Publish($"wsync/request/{session.SessionId}/entity/{entityId}/scale", json);
#endif
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Set entity parent.
        /// </summary>
        internal async Task SetEntityParentAsync(SyncSession session, string childId, string parentId)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.parent", session.SessionId,
                    new QueuedParentUpdatePayload { ChildId = childId, ParentId = parentId });
                return;
            }

            if (!UseTestHooks)
            {
#if USE_WEBINTERFACE
                var messageId = Guid.NewGuid().ToString();
                var json = $"{{\"message-id\":\"{messageId}\",\"client-id\":\"{Config.ClientId}\",\"parent-id\":\"{parentId ?? ""}\"}}";
                _mqttClient.Publish($"wsync/request/{session.SessionId}/entity/{childId}/parent", json);
#endif
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Set entity visibility.
        /// </summary>
        internal async Task SetEntityVisibilityAsync(SyncSession session, string entityId, bool visible)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.visibility", session.SessionId,
                    new QueuedEntityUpdatePayload { EntityId = entityId, Value = visible });
                return;
            }

            if (!UseTestHooks)
            {
#if USE_WEBINTERFACE
                var messageId = Guid.NewGuid().ToString();
                var json = $"{{\"message-id\":\"{messageId}\",\"client-id\":\"{Config.ClientId}\",\"visible\":{(visible ? "true" : "false")}}}";
                _mqttClient.Publish($"wsync/request/{session.SessionId}/entity/{entityId}/visibility", json);
#endif
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Set entity highlight state.
        /// </summary>
        internal async Task SetEntityHighlightAsync(SyncSession session, string entityId, bool highlight)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.highlight", session.SessionId,
                    new QueuedEntityUpdatePayload { EntityId = entityId, Value = highlight });
                return;
            }

            if (!UseTestHooks)
            {
#if USE_WEBINTERFACE
                var messageId = Guid.NewGuid().ToString();
                var json = $"{{\"message-id\":\"{messageId}\",\"client-id\":\"{Config.ClientId}\",\"highlight\":{(highlight ? "true" : "false")}}}";
                _mqttClient.Publish($"wsync/request/{session.SessionId}/entity/{entityId}/highlight", json);
#endif
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Send a custom message through the sync channel.
        /// </summary>
        internal async Task SendCustomMessageAsync(SyncSession session, string topic, string payload)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new WorldSyncException(WorldSyncErrorCode.InvalidMessage,
                    "Custom message topic cannot be null or empty");
            }

            if (!IsConnected)
            {
                QueueOrThrow<bool>("message.custom", session.SessionId,
                    new QueuedMessagePayload { Topic = topic, Message = payload });
                return;
            }

            if (UseTestHooks)
            {
                SimulateSendCustomMessageInvocations++;
            }
            else
            {
#if USE_WEBINTERFACE
                var messageId = Guid.NewGuid().ToString();
                var json = $"{{\"message-id\":\"{messageId}\",\"client-id\":\"{Config.ClientId}\",\"topic\":\"{EscapeJsonString(topic)}\",\"payload\":\"{EscapeJsonString(payload ?? "")}\"}}";
                _mqttClient.Publish($"wsync/request/{session.SessionId}/message/custom", json);
#endif
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Escape a string for embedding in a JSON value.
        /// </summary>
        private static string EscapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value)) return value ?? "";
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// Handle connection lost - triggers auto-reconnect if enabled.
        /// </summary>
        private async void HandleConnectionLost(string reason)
        {
            if (State == ConnectionState.Disconnecting || State == ConnectionState.Disconnected)
            {
                return;
            }

            // Suspend bridges before firing events or reconnecting —
            // stops polling but preserves the dictionary for resurrection.
            SuspendBridges();

            OnDisconnected?.Invoke(reason);

            if (!Config.AutoReconnect.Enabled)
            {
                State = ConnectionState.Disconnected;
                ClearEntityBridges();
                return;
            }

            // Start auto-reconnect
            await AttemptReconnectAsync();
        }

        /// <summary>
        /// Attempt to reconnect with exponential backoff.
        /// </summary>
        private async Task AttemptReconnectAsync()
        {
            State = ConnectionState.Reconnecting;
            _reconnectAttempt = 0;

            while (_reconnectAttempt < Config.AutoReconnect.MaxAttempts)
            {
                _reconnectAttempt++;
                OnReconnecting?.Invoke(_reconnectAttempt);

                // Calculate delay with exponential backoff
                var delay = (int)(Config.AutoReconnect.InitialDelayMs *
                    Math.Pow(Config.AutoReconnect.BackoffMultiplier, _reconnectAttempt - 1));
                delay = Math.Min(delay, Config.AutoReconnect.MaxDelayMs);

                await Task.Delay(delay);

                try
                {
                    await ConnectInternalAsync();
                    State = ConnectionState.Connected;
                    _reconnectAttempt = 0;
                    OnReconnected?.Invoke();

                    // Attempt session recovery
                    if (_wasInSession && !string.IsNullOrEmpty(_lastSessionId))
                    {
                        await RecoverSessionAsync();
                    }

                    // Resume suspended bridges after session recovery — only if
                    // the session is still valid (SessionNotFound sets CurrentSession = null).
                    if (CurrentSession != null && CurrentSession.IsValid)
                    {
                        await ResumeBridgesAsync();
                    }
                    else
                    {
                        ClearEntityBridges();
                    }

                    // Process queued operations
                    await ProcessOperationQueueAsync();

                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[WorldSync] Reconnect attempt {_reconnectAttempt} failed: {ex.Message}");
                }
            }

            // All attempts failed — destroy suspended bridges and discard queued operations
            ClearEntityBridges();
            DiscardOperationQueue(_reconnectAttempt);

            State = ConnectionState.Disconnected;
            OnReconnectionFailed?.Invoke(_reconnectAttempt);

            var error = WorldSyncException.ReconnectionFailed(_reconnectAttempt);
            OnError?.Invoke(error);
        }

        /// <summary>
        /// Recover session state after reconnection.
        /// </summary>
        private async Task RecoverSessionAsync()
        {
            try
            {
                // Try to rejoin the previous session
                var session = await JoinSessionAsync(_lastSessionId);
                CurrentSession = session;
                OnStateRecovered?.Invoke();
            }
            catch (WorldSyncException ex) when (ex.Code == WorldSyncErrorCode.SessionNotFound)
            {
                // Session no longer exists — capture old session for cleanup handlers
                // before nulling CurrentSession.
                LastExpiredSession = CurrentSession;
                CurrentSession = null;
                _wasInSession = false;
                OnSessionExpired?.Invoke(_lastSessionId);
            }
        }

        /// <summary>
        /// Process queued operations after reconnection.
        /// </summary>
        internal async Task ProcessOperationQueueAsync()
        {
            List<PendingOperation> operations;

            lock (_queueLock)
            {
                operations = new List<PendingOperation>(_operationQueue);
                _operationQueue.Clear();
            }

            if (CurrentSession == null || !CurrentSession.IsValid)
            {
                // Session is gone — fault everything
                foreach (var op in operations)
                {
                    op.Completion?.TrySetException(
                        new WorldSyncException(WorldSyncErrorCode.SessionNotFound,
                            "Session expired during reconnection"));
                }
                if (operations.Count > 0)
                {
                    Debug.LogWarning($"[WorldSync] Discarded {operations.Count} queued operation(s) — session expired.");
                }
                return;
            }

            foreach (var op in operations)
            {
                try
                {
                    await ReplayOperationAsync(op);
                }
                catch (Exception ex)
                {
                    op.Completion?.TrySetException(ex);
                }
            }
        }

        private async Task ReplayOperationAsync(PendingOperation op)
        {
            switch (op.Type)
            {
                case "entity.create":
                {
                    var entity = (SyncEntity)op.Payload;
                    var result = await CreateEntityAsync(CurrentSession, entity);
                    op.Completion?.TrySetResult(result);
                    break;
                }

                case "entity.delete":
                {
                    var entityId = (string)op.Payload;
                    if (!CurrentSession.HasEntity(entityId))
                    {
                        Debug.LogWarning($"[WorldSync] Queue replay skipped {op.Type}: entity {entityId} not found in recovered session.");
                        op.Completion?.TrySetException(
                            new WorldSyncException(WorldSyncErrorCode.InvalidPayload,
                                "Entity not found in recovered session"));
                        break;
                    }
                    await DeleteEntityAsync(CurrentSession, entityId);
                    op.Completion?.TrySetResult(null);
                    break;
                }

                case "entity.update.position":
                {
                    var payload = (QueuedEntityUpdatePayload)op.Payload;
                    if (!CurrentSession.HasEntity(payload.EntityId))
                    {
                        Debug.LogWarning($"[WorldSync] Queue replay skipped {op.Type}: entity {payload.EntityId} not found in recovered session.");
                        op.Completion?.TrySetException(
                            new WorldSyncException(WorldSyncErrorCode.InvalidPayload,
                                "Entity not found in recovered session"));
                        break;
                    }
                    await UpdateEntityPositionAsync(CurrentSession, payload.EntityId, (SyncVector3)payload.Value);
                    op.Completion?.TrySetResult(null);
                    break;
                }

                case "entity.update.rotation":
                {
                    var payload = (QueuedEntityUpdatePayload)op.Payload;
                    if (!CurrentSession.HasEntity(payload.EntityId))
                    {
                        Debug.LogWarning($"[WorldSync] Queue replay skipped {op.Type}: entity {payload.EntityId} not found in recovered session.");
                        op.Completion?.TrySetException(
                            new WorldSyncException(WorldSyncErrorCode.InvalidPayload,
                                "Entity not found in recovered session"));
                        break;
                    }
                    await UpdateEntityRotationAsync(CurrentSession, payload.EntityId, (SyncQuaternion)payload.Value);
                    op.Completion?.TrySetResult(null);
                    break;
                }

                case "entity.update.scale":
                {
                    var payload = (QueuedEntityUpdatePayload)op.Payload;
                    if (!CurrentSession.HasEntity(payload.EntityId))
                    {
                        Debug.LogWarning($"[WorldSync] Queue replay skipped {op.Type}: entity {payload.EntityId} not found in recovered session.");
                        op.Completion?.TrySetException(
                            new WorldSyncException(WorldSyncErrorCode.InvalidPayload,
                                "Entity not found in recovered session"));
                        break;
                    }
                    await UpdateEntityScaleAsync(CurrentSession, payload.EntityId, (SyncVector3)payload.Value);
                    op.Completion?.TrySetResult(null);
                    break;
                }

                case "entity.update.parent":
                {
                    var payload = (QueuedParentUpdatePayload)op.Payload;
                    if (!CurrentSession.HasEntity(payload.ChildId))
                    {
                        Debug.LogWarning($"[WorldSync] Queue replay skipped {op.Type}: entity {payload.ChildId} not found in recovered session.");
                        op.Completion?.TrySetException(
                            new WorldSyncException(WorldSyncErrorCode.InvalidPayload,
                                "Entity not found in recovered session"));
                        break;
                    }
                    await SetEntityParentAsync(CurrentSession, payload.ChildId, payload.ParentId);
                    op.Completion?.TrySetResult(null);
                    break;
                }

                case "entity.update.visibility":
                {
                    var payload = (QueuedEntityUpdatePayload)op.Payload;
                    if (!CurrentSession.HasEntity(payload.EntityId))
                    {
                        Debug.LogWarning($"[WorldSync] Queue replay skipped {op.Type}: entity {payload.EntityId} not found in recovered session.");
                        op.Completion?.TrySetException(
                            new WorldSyncException(WorldSyncErrorCode.InvalidPayload,
                                "Entity not found in recovered session"));
                        break;
                    }
                    await SetEntityVisibilityAsync(CurrentSession, payload.EntityId, (bool)payload.Value);
                    op.Completion?.TrySetResult(null);
                    break;
                }

                case "entity.update.highlight":
                {
                    var payload = (QueuedEntityUpdatePayload)op.Payload;
                    if (!CurrentSession.HasEntity(payload.EntityId))
                    {
                        Debug.LogWarning($"[WorldSync] Queue replay skipped {op.Type}: entity {payload.EntityId} not found in recovered session.");
                        op.Completion?.TrySetException(
                            new WorldSyncException(WorldSyncErrorCode.InvalidPayload,
                                "Entity not found in recovered session"));
                        break;
                    }
                    await SetEntityHighlightAsync(CurrentSession, payload.EntityId, (bool)payload.Value);
                    op.Completion?.TrySetResult(null);
                    break;
                }

                case "message.custom":
                {
                    var payload = (QueuedMessagePayload)op.Payload;
                    await SendCustomMessageAsync(CurrentSession, payload.Topic, payload.Message);
                    op.Completion?.TrySetResult(null);
                    break;
                }

                default:
                    Debug.LogWarning($"[WorldSync] Queue replay skipped unknown operation type: {op.Type}");
                    op.Completion?.TrySetException(
                        new WorldSyncException(WorldSyncErrorCode.InternalError,
                            $"Unknown queued operation type: {op.Type}"));
                    break;
            }
        }

        /// <summary>
        /// The session that was active before it expired during reconnection.
        /// Set in RecoverSessionAsync before CurrentSession is nulled, so cleanup
        /// handlers can unsubscribe from the old session's events.
        /// </summary>
        public SyncSession LastExpiredSession { get; private set; }

        /// <summary>
        /// Test seam: fire OnStateRecovered event.
        /// </summary>
        internal void FireOnStateRecovered()
        {
            OnStateRecovered?.Invoke();
        }

        /// <summary>
        /// Test seam: fire OnSessionExpired event.
        /// </summary>
        internal void FireOnSessionExpired(string sessionId)
        {
            OnSessionExpired?.Invoke(sessionId);
        }

        /// <summary>
        /// Test seam: directly enqueue an operation for testing ProcessOperationQueueAsync.
        /// </summary>
        internal void EnqueueOperation(PendingOperation operation)
        {
            lock (_queueLock)
            {
                _operationQueue.Enqueue(operation);
            }
        }

        /// <summary>
        /// Discard all queued operations, faulting each TCS with ReconnectionFailed.
        /// </summary>
        internal void DiscardOperationQueue(int attempts)
        {
            List<PendingOperation> operations;
            lock (_queueLock)
            {
                operations = new List<PendingOperation>(_operationQueue);
                _operationQueue.Clear();
            }

            if (operations.Count == 0) return;

            var ex = WorldSyncException.ReconnectionFailed(attempts);
            foreach (var op in operations)
            {
                op.Completion?.TrySetException(ex);
            }
            Debug.LogWarning($"[WorldSync] Discarded {operations.Count} queued operation(s) after {attempts} failed reconnection attempt(s).");
        }

        /// <summary>
        /// Queue an operation or throw if not connected and queue is disabled.
        /// </summary>
        private T QueueOrThrow<T>(string type, string sessionId, object payload)
        {
            if (State != ConnectionState.Reconnecting)
            {
                throw WorldSyncException.NotConnected();
            }

            lock (_queueLock)
            {
                // Drop oldest if queue is full
                while (_operationQueue.Count >= MaxPendingOperations)
                {
                    var dropped = _operationQueue.Dequeue();
                    dropped.Completion?.TrySetException(
                        new WorldSyncException(WorldSyncErrorCode.InternalError, "Operation dropped from queue")
                    );
                    Debug.LogWarning($"[WorldSync] Operation queue full, dropped: {dropped.Type}");
                }

                var operation = new PendingOperation
                {
                    Type = type,
                    SessionId = sessionId,
                    Payload = payload,
                    Completion = new TaskCompletionSource<object>(),
                    QueuedAt = DateTime.UtcNow
                };

                _operationQueue.Enqueue(operation);
            }

            // Return default value - actual result will come from queue processing
            return default;
        }

        /// <summary>
        /// Connect to MQTT broker internally.
        /// </summary>
        private async Task ConnectInternalAsync()
        {
            // Test hook path: bypass real MQTT for unit tests
            if (UseTestHooks)
            {
                if (SimulateConnectionFailure)
                {
                    throw WorldSyncException.ConnectionFailed("Simulated connection failure");
                }
                await Task.CompletedTask;
                return;
            }

#if USE_WEBINTERFACE
            _intentionalDisconnect = false;
            _connectTcs = new TaskCompletionSource<bool>();

            var transport = Config.Transport == WorldSyncTransport.TCP
                ? MQTTClient.Transports.TCP
                : MQTTClient.Transports.WebSockets;

            _mqttClient = new MQTTClient(
                Config.Host, Config.Port, Config.Tls.Enabled, transport,
                OnMqttConnected, OnMqttDisconnected, OnMqttStateChanged, OnMqttError,
                Config.WebSocketPath, Config.ClientId);

            _mqttClient.Connect();

            // Wait for connection with timeout
            var timeoutTask = Task.Delay(Config.ConnectTimeoutMs);
            var completedTask = await Task.WhenAny(_connectTcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                CleanupMqttClient();
                throw new WorldSyncException(
                    WorldSyncErrorCode.ConnectionTimeout,
                    $"Connection timed out after {Config.ConnectTimeoutMs}ms");
            }

            // Check if connection failed
            if (!_connectTcs.Task.Result)
            {
                CleanupMqttClient();
                throw WorldSyncException.ConnectionFailed("MQTT connection failed");
            }

            // Subscribe to client-specific response topic for request-response pattern
            _mqttClient.Subscribe("wsync/response/" + Config.ClientId,
                (topic) => Debug.Log("[WorldSync] Subscribed to response topic: " + topic),
                OnResponseReceived);
#else
            if (SimulateConnectionFailure)
            {
                throw WorldSyncException.ConnectionFailed("Simulated connection failure");
            }
            await Task.CompletedTask;
#endif
        }

        /// <summary>
        /// Disconnect from MQTT broker internally.
        /// </summary>
        private async Task DisconnectInternalAsync()
        {
            // Test hook path: no-op disconnect
            if (UseTestHooks)
            {
                await Task.CompletedTask;
                return;
            }

#if USE_WEBINTERFACE
            _intentionalDisconnect = true;

            // Cancel all pending requests
            lock (_requestLock)
            {
                foreach (var tcs in _pendingRequests.Values)
                {
                    tcs.TrySetException(WorldSyncException.NotConnected());
                }
                _pendingRequests.Clear();
            }

            if (_mqttClient != null)
            {
                _mqttClient.Disconnect("user_disconnect");
                _mqttClient = null;
            }

            await Task.CompletedTask;
#else
            await Task.CompletedTask;
#endif
        }

#if USE_WEBINTERFACE
        /// <summary>
        /// Callback when MQTT client connects successfully.
        /// </summary>
        private void OnMqttConnected(MQTTClient client)
        {
            Debug.Log("[WorldSync] MQTT connected");
            _connectTcs?.TrySetResult(true);
        }

        /// <summary>
        /// Callback when MQTT client disconnects.
        /// </summary>
        private void OnMqttDisconnected(MQTTClient client, byte code, string message)
        {
            if (_intentionalDisconnect)
            {
                return;
            }

            Debug.LogWarning($"[WorldSync] Unexpected MQTT disconnect: code={code}, message={message}");
            HandleConnectionLost(message ?? "unexpected_disconnect");
        }

        /// <summary>
        /// Callback when MQTT client state changes.
        /// </summary>
        private void OnMqttStateChanged(MQTTClient client, MQTTClient.ClientState oldState,
            MQTTClient.ClientState newState)
        {
            Debug.Log($"[WorldSync] MQTT state: {oldState} => {newState}");

            // Note: WorldSyncClient.State is managed by ConnectAsync/DisconnectAsync/HandleConnectionLost,
            // not by raw MQTT state transitions, to avoid conflicting state updates.
        }

        /// <summary>
        /// Clean up MQTT client after failure or timeout.
        /// Prevents ghost callbacks from firing into stale state.
        /// </summary>
        private void CleanupMqttClient()
        {
            _intentionalDisconnect = true;
            _connectTcs = null;
            if (_mqttClient != null)
            {
                try { _mqttClient.Disconnect("cleanup"); } catch { }
                _mqttClient = null;
            }
        }

        /// <summary>
        /// Callback when MQTT client encounters an error.
        /// </summary>
        private void OnMqttError(MQTTClient client, string message)
        {
            Debug.LogError($"[WorldSync] MQTT error: {message}");
            var error = WorldSyncException.ConnectionFailed(message);
            OnError?.Invoke(error);
            _connectTcs?.TrySetResult(false);
        }

        /// <summary>
        /// Publish a request and wait for a correlated response.
        /// </summary>
        private async Task<string> PublishAndWaitAsync(string topic, string jsonPayload,
            string messageId, int timeoutMs)
        {
            var tcs = new TaskCompletionSource<string>();

            lock (_requestLock)
            {
                _pendingRequests[messageId] = tcs;
            }

            try
            {
                _mqttClient.Publish(topic, jsonPayload);
            }
            catch
            {
                lock (_requestLock)
                {
                    _pendingRequests.Remove(messageId);
                }
                throw;
            }

            var timeoutTask = Task.Delay(timeoutMs);
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

            lock (_requestLock)
            {
                _pendingRequests.Remove(messageId);
            }

            if (completedTask == timeoutTask)
            {
                throw new WorldSyncException(WorldSyncErrorCode.RequestTimeout,
                    $"Request to {topic} timed out after {timeoutMs}ms");
            }

            var response = tcs.Task.Result;

            // Check for error response
            var responseObj = JObject.Parse(response);
            if (responseObj.Value<bool>("success") != true)
            {
                var errorCode = responseObj.Value<string>("code");
                var errorMessage = responseObj.Value<string>("message");
                throw new WorldSyncException(
                    MapServerErrorCode(errorCode),
                    errorMessage ?? $"Server error on {topic}");
            }

            return response;
        }

        /// <summary>
        /// Callback for messages on the client response topic.
        /// Routes responses to pending request handlers by correlation-id.
        /// </summary>
        private void OnResponseReceived(MQTTClient client, string topicFilter,
            string topicName, MQTTMessage message)
        {
            var payload = System.Text.Encoding.UTF8.GetString(
                message.payload.data, message.payload.offset, message.payload.count);

            JObject obj;
            try { obj = JObject.Parse(payload); }
            catch { return; }

            var correlationId = obj.Value<string>("correlation-id");
            if (string.IsNullOrEmpty(correlationId)) return;

            TaskCompletionSource<string> tcs;
            lock (_requestLock)
            {
                if (!_pendingRequests.TryGetValue(correlationId, out tcs)) return;
            }

            tcs.TrySetResult(payload);
        }

        /// <summary>
        /// Subscribe to session status broadcast topics.
        /// </summary>
        private void SubscribeToSessionStatusTopics(string sessionId)
        {
            var topic = $"wsync/status/{sessionId}/#";
            _mqttClient.Subscribe(topic,
                (t) => Debug.Log($"[WorldSync] Subscribed to status topics: {t}"),
                OnSessionStatusMessage);
        }

        /// <summary>
        /// Unsubscribe from session status broadcast topics.
        /// </summary>
        private void UnsubscribeFromSessionStatusTopics(string sessionId)
        {
            var topic = $"wsync/status/{sessionId}/#";
            _mqttClient?.UnSubscribe(topic,
                (t) => Debug.Log($"[WorldSync] Unsubscribed from status topics: {t}"));
        }

        /// <summary>
        /// Callback for incoming session status messages.
        /// Routes to the appropriate SyncSession handler based on topic.
        /// </summary>
        private void OnSessionStatusMessage(MQTTClient client, string topicFilter,
            string topicName, MQTTMessage message)
        {
            if (CurrentSession == null || !CurrentSession.IsValid) return;

            var payload = System.Text.Encoding.UTF8.GetString(
                message.payload.data, message.payload.offset, message.payload.count);

            RouteStatusMessage(topicName, payload);
        }
#endif

        /// <summary>
        /// Ensure client is connected.
        /// </summary>
        private void EnsureConnected()
        {
            if (State != ConnectionState.Connected)
            {
                throw WorldSyncException.NotConnected();
            }
        }

        /// <summary>
        /// Route a session status message based on topic to the appropriate SyncSession handler.
        /// </summary>
        private void RouteStatusMessage(string topic, string payload)
        {
            // Topic format: wsync/status/{sessionId}/{...}
            var parts = topic.Split('/');
            if (parts.Length < 5) return;

            var sessionId = parts[2];
            if (CurrentSession == null || CurrentSession.SessionId != sessionId) return;

            if (parts[3] == "client")
            {
                if (parts[4] == "joined")
                {
                    var syncClient = new SyncClient
                    {
                        ClientId = ExtractJsonString(payload, "client-id"),
                        ClientTag = ExtractJsonString(payload, "client-tag"),
                        JoinedAt = ExtractJsonString(payload, "joined-at")
                    };
                    CurrentSession.HandleClientJoined(syncClient);
                }
                else if (parts[4] == "left")
                {
                    var clientId = ExtractJsonString(payload, "client-id");
                    var reason = ExtractJsonString(payload, "reason") ?? "unknown";
                    CurrentSession.HandleClientLeft(clientId, reason);
                }
            }
            else if (parts[3] == "entity")
            {
                if (parts[4] == "created")
                {
                    var entity = new SyncEntity
                    {
                        EntityId = ExtractJsonString(payload, "entity-id"),
                        OwnerId = ExtractJsonString(payload, "owner-id"),
                        EntityType = ExtractJsonString(payload, "entity-type"),
                        EntityTag = ExtractJsonString(payload, "entity-tag")
                    };

                    // Parse transform if present
                    var pos = ExtractJsonVector3(payload, "position");
                    if (pos.HasValue) entity.Position = pos.Value;
                    var rot = ExtractJsonQuaternion(payload, "rotation");
                    if (rot.HasValue) entity.Rotation = rot.Value;
                    var scl = ExtractJsonVector3(payload, "scale");
                    if (scl.HasValue) entity.Scale = scl.Value;

                    // Parse state fields
                    var visible = ExtractJsonBool(payload, "visible");
                    if (visible.HasValue) entity.Visible = visible.Value;
                    var highlight = ExtractJsonBool(payload, "highlight");
                    if (highlight.HasValue) entity.Highlight = highlight.Value;
                    var parentId = ExtractJsonString(payload, "parent-id");
                    if (parentId != null) entity.ParentId = parentId;

                    // Parse properties (filePath, resources, etc.)
                    var properties = ExtractJsonProperties(payload);
                    if (properties != null)
                        entity.Properties = properties;

                    CurrentSession.HandleEntityCreated(entity);
                }
                else if (parts.Length >= 6)
                {
                    var entityId = parts[4];
                    if (parts[5] == "updated")
                    {
                        // Transform fields
                        var position = ExtractJsonVector3(payload, "position");
                        var rotation = ExtractJsonQuaternion(payload, "rotation");
                        var scale = ExtractJsonVector3(payload, "scale");

                        bool hasTransform = position.HasValue || rotation.HasValue || scale.HasValue;

                        if (hasTransform)
                        {
                            CurrentSession.HandleEntityTransform(entityId, position, rotation, scale);
                        }

                        // Non-transform state fields
                        var visible = ExtractJsonBool(payload, "visible");
                        var highlight = ExtractJsonBool(payload, "highlight");
                        var parentId = ExtractJsonString(payload, "parent-id");
                        var interactionState = ExtractJsonString(payload, "interaction-state");

                        bool hasState = visible.HasValue || highlight.HasValue
                            || parentId != null || interactionState != null;

                        if (hasState)
                        {
                            CurrentSession.HandleEntityStateUpdate(entityId,
                                visible, highlight, parentId, interactionState);
                        }
                    }
                    else if (parts[5] == "deleted")
                    {
                        CurrentSession.HandleEntityDeleted(entityId);
                    }
                }
            }
            else if (parts[3] == "message")
            {
                if (parts[4] == "custom")
                {
                    var msgTopic = ExtractJsonString(payload, "topic");
                    var senderId = ExtractJsonString(payload, "sender-id");
                    var msgPayload = ExtractJsonString(payload, "payload");
                    CurrentSession.HandleCustomMessage(msgTopic, senderId, msgPayload);
                }
            }
        }

        /// <summary>
        /// Extract a string value from a JSON payload by key name.
        /// Uses simple string parsing to avoid Newtonsoft.Json dependency in test builds.
        /// </summary>
        private static string ExtractJsonString(string json, string key)
        {
            var search = $"\"{key}\":\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;
            var start = idx + search.Length;
            var end = json.IndexOf('"', start);
            if (end < 0) return null;
            return json.Substring(start, end - start);
        }

        /// <summary>
        /// Extract a nested vector3 object {x,y,z} from a JSON payload.
        /// </summary>
        private static SyncVector3? ExtractJsonVector3(string json, string key)
        {
            var search = $"\"{key}\":{{";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0)
            {
                // Try with space after colon
                search = $"\"{key}\": {{";
                idx = json.IndexOf(search, StringComparison.Ordinal);
                if (idx < 0) return null;
            }
            var start = idx + search.Length - 1; // include the '{'
            var end = json.IndexOf('}', start);
            if (end < 0) return null;
            var inner = json.Substring(start, end - start + 1);
            var x = ExtractJsonFloat(inner, "x");
            var y = ExtractJsonFloat(inner, "y");
            var z = ExtractJsonFloat(inner, "z");
            if (!x.HasValue || !y.HasValue || !z.HasValue) return null;
            return new SyncVector3(x.Value, y.Value, z.Value);
        }

        /// <summary>
        /// Extract a nested quaternion object {x,y,z,w} from a JSON payload.
        /// </summary>
        private static SyncQuaternion? ExtractJsonQuaternion(string json, string key)
        {
            var search = $"\"{key}\":{{";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0)
            {
                search = $"\"{key}\": {{";
                idx = json.IndexOf(search, StringComparison.Ordinal);
                if (idx < 0) return null;
            }
            var start = idx + search.Length - 1;
            var end = json.IndexOf('}', start);
            if (end < 0) return null;
            var inner = json.Substring(start, end - start + 1);
            var x = ExtractJsonFloat(inner, "x");
            var y = ExtractJsonFloat(inner, "y");
            var z = ExtractJsonFloat(inner, "z");
            var w = ExtractJsonFloat(inner, "w");
            if (!x.HasValue || !y.HasValue || !z.HasValue || !w.HasValue) return null;
            return new SyncQuaternion(x.Value, y.Value, z.Value, w.Value);
        }

        /// <summary>
        /// Extract a float value from a JSON string by key name.
        /// </summary>
        private static float? ExtractJsonFloat(string json, string key)
        {
            var search = $"\"{key}\":";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0)
            {
                search = $"\"{key}\": ";
                idx = json.IndexOf(search, StringComparison.Ordinal);
                if (idx < 0) return null;
            }
            var start = idx + search.Length;
            // Skip whitespace
            while (start < json.Length && json[start] == ' ') start++;
            var end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '.' || json[end] == '-' || json[end] == 'E' || json[end] == 'e' || json[end] == '+'))
                end++;
            if (end == start) return null;
            if (float.TryParse(json.Substring(start, end - start),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var val))
                return val;
            return null;
        }

        /// <summary>
        /// Extract a boolean value from a JSON payload by key name.
        /// </summary>
        private static bool? ExtractJsonBool(string json, string key)
        {
            var search = $"\"{key}\":";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0)
            {
                search = $"\"{key}\": ";
                idx = json.IndexOf(search, StringComparison.Ordinal);
                if (idx < 0) return null;
            }
            var start = idx + search.Length;
            while (start < json.Length && json[start] == ' ') start++;
            if (start >= json.Length) return null;
            if (json.Length >= start + 4 && json.Substring(start, 4) == "true") return true;
            if (json.Length >= start + 5 && json.Substring(start, 5) == "false") return false;
            return null;
        }

        /// <summary>
        /// Extract a nested "properties" object from a JSON payload.
        /// Returns a Dictionary with string keys. Handles filePath (string),
        /// resources (string array), and other simple values.
        /// </summary>
        private static Dictionary<string, object> ExtractJsonProperties(string json)
        {
            try
            {
                var obj = JObject.Parse(json);
                var propsToken = obj["properties"];
                if (propsToken == null || propsToken.Type != JTokenType.Object)
                    return null;

                var result = new Dictionary<string, object>();
                foreach (var kvp in (JObject)propsToken)
                {
                    if (kvp.Value.Type == JTokenType.String)
                        result[kvp.Key] = kvp.Value.Value<string>();
                    else if (kvp.Value.Type == JTokenType.Array)
                        result[kvp.Key] = kvp.Value.ToObject<string[]>();
                    else if (kvp.Value.Type == JTokenType.Integer)
                        result[kvp.Key] = kvp.Value.Value<long>();
                    else if (kvp.Value.Type == JTokenType.Float)
                        result[kvp.Key] = kvp.Value.Value<double>();
                    else if (kvp.Value.Type == JTokenType.Boolean)
                        result[kvp.Key] = kvp.Value.Value<bool>();
                    else
                        result[kvp.Key] = kvp.Value.ToString();
                }
                return result.Count > 0 ? result : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Simulate receiving a status message on a topic (test hook).
        /// </summary>
        internal void SimulateStatusMessage(string topic, string jsonPayload)
        {
            RouteStatusMessage(topic, jsonPayload);
        }

        /// <summary>
        /// Simulate a client joined status event (test hook).
        /// </summary>
        internal void SimulateClientJoinedStatus(SyncClient client)
        {
            CurrentSession?.HandleClientJoined(client);
        }

        /// <summary>
        /// Simulate a client left status event (test hook).
        /// </summary>
        internal void SimulateClientLeftStatus(string clientId, string reason)
        {
            CurrentSession?.HandleClientLeft(clientId, reason);
        }

        /// <summary>
        /// Simulate an entity created status event (test hook).
        /// </summary>
        internal void SimulateEntityCreatedStatus(SyncEntity entity)
        {
            CurrentSession?.HandleEntityCreated(entity);
        }

        /// <summary>
        /// Simulate an entity deleted status event (test hook).
        /// </summary>
        internal void SimulateEntityDeletedStatus(string entityId)
        {
            CurrentSession?.HandleEntityDeleted(entityId);
        }

        /// <summary>
        /// Simulate an entity transform updated status event (test hook).
        /// </summary>
        internal void SimulateEntityUpdatedStatus(string entityId, SyncVector3? position,
            SyncQuaternion? rotation, SyncVector3? scale)
        {
            CurrentSession?.HandleEntityTransform(entityId, position, rotation, scale);
        }

#if USE_WEBINTERFACE
        /// <summary>
        /// Parse a session state from a join response JSON object.
        /// Extracts entities and clients arrays for full state initialization.
        /// </summary>
        private static SessionState ParseSessionState(JObject obj, string sessionId)
        {
            var state = new SessionState
            {
                SessionId = sessionId,
                SessionTag = obj.Value<string>("tag"),
                CreatedAt = obj.Value<string>("created-at")
            };

            var entitiesArray = obj["entities"] as JArray;
            if (entitiesArray != null)
            {
                state.Entities = new List<SyncEntity>();
                foreach (var item in entitiesArray)
                {
                    var entity = new SyncEntity
                    {
                        EntityId = item.Value<string>("entity-id"),
                        OwnerId = item.Value<string>("owner-id"),
                        EntityType = item.Value<string>("entity-type"),
                        EntityTag = item.Value<string>("entity-tag")
                    };

                    var pos = item["position"];
                    if (pos != null && pos.Type == JTokenType.Object)
                    {
                        entity.Position = new SyncVector3(
                            pos.Value<float>("x"), pos.Value<float>("y"), pos.Value<float>("z"));
                    }

                    var rot = item["rotation"];
                    if (rot != null && rot.Type == JTokenType.Object)
                    {
                        entity.Rotation = new SyncQuaternion(
                            rot.Value<float>("x"), rot.Value<float>("y"),
                            rot.Value<float>("z"), rot.Value<float>("w"));
                    }

                    var scl = item["scale"];
                    if (scl != null && scl.Type == JTokenType.Object)
                    {
                        entity.Scale = new SyncVector3(
                            scl.Value<float>("x"), scl.Value<float>("y"), scl.Value<float>("z"));
                    }

                    state.Entities.Add(entity);
                }
            }

            var clientsArray = obj["clients"] as JArray;
            if (clientsArray != null)
            {
                state.Clients = new List<SyncClient>();
                foreach (var item in clientsArray)
                {
                    state.Clients.Add(new SyncClient
                    {
                        ClientId = item.Value<string>("client-id"),
                        ClientTag = item.Value<string>("client-tag"),
                        JoinedAt = item.Value<string>("joined-at")
                    });
                }
            }

            return state;
        }
#endif

        /// <summary>
        /// Map a server error code string to a WorldSyncErrorCode enum value.
        /// </summary>
        internal static WorldSyncErrorCode MapServerErrorCode(string serverCode)
        {
            if (string.IsNullOrEmpty(serverCode)) return WorldSyncErrorCode.InternalError;

            switch (serverCode)
            {
                case "SESSION_NOT_FOUND": return WorldSyncErrorCode.SessionNotFound;
                case "SESSION_EXISTS": return WorldSyncErrorCode.SessionExists;
                case "UNAUTHORIZED": return WorldSyncErrorCode.Unauthorized;
                case "FORBIDDEN": return WorldSyncErrorCode.Forbidden;
                case "INVALID_PAYLOAD": return WorldSyncErrorCode.InvalidPayload;
                case "CLIENT_NOT_IN_SESSION": return WorldSyncErrorCode.ClientNotInSession;
                case "ENTITY_NOT_FOUND": return WorldSyncErrorCode.EntityNotFound;
                case "ENTITY_EXISTS": return WorldSyncErrorCode.EntityExists;
                case "CLIENT_NOT_FOUND": return WorldSyncErrorCode.ClientNotFound;
                case "INVALID_ENTITY_TYPE": return WorldSyncErrorCode.InvalidEntityType;
                case "INVALID_HIERARCHY": return WorldSyncErrorCode.InvalidHierarchy;
                case "INVALID_MESSAGE": return WorldSyncErrorCode.InvalidMessage;
                case "UNSUPPORTED_PROTOCOL": return WorldSyncErrorCode.UnsupportedProtocol;
                case "CHUNK_INVALID": return WorldSyncErrorCode.ChunkInvalid;
                case "PAYLOAD_TOO_LARGE": return WorldSyncErrorCode.PayloadTooLarge;
                case "SESSION_EXPIRED": return WorldSyncErrorCode.SessionExpired;
                case "CONNECTION_TIMEOUT": return WorldSyncErrorCode.ConnectionTimeout;
                case "REQUEST_TIMEOUT": return WorldSyncErrorCode.RequestTimeout;
                case "INTERNAL_ERROR": return WorldSyncErrorCode.InternalError;
                default: return WorldSyncErrorCode.InternalError;
            }
        }
    }
}
