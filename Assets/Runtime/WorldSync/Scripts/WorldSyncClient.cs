// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

        /// <summary>
        /// Create a new WorldSync client.
        /// </summary>
        /// <param name="config">Client configuration.</param>
        public WorldSyncClient(WorldSyncConfig config)
        {
            config.Validate();
            Config = config;
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

            // TODO: Implement actual MQTT message sending
            var sessionId = Guid.NewGuid().ToString();
            var createdAt = DateTime.UtcNow.ToString("o");

            var session = new SyncSession(this, sessionId, tag, createdAt, Config.ClientId);
            CurrentSession = session;
            _lastSessionId = sessionId;
            _wasInSession = true;

            return session;
        }

        /// <summary>
        /// Join an existing session.
        /// </summary>
        /// <param name="sessionId">Session ID to join.</param>
        /// <returns>Joined session with full state.</returns>
        public async Task<SyncSession> JoinSessionAsync(string sessionId)
        {
            EnsureConnected();

            // TODO: Implement actual MQTT message sending and wait for response
            var session = new SyncSession(this, sessionId, "unknown", DateTime.UtcNow.ToString("o"), Config.ClientId);
            CurrentSession = session;
            _lastSessionId = sessionId;
            _wasInSession = true;

            return session;
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

            // TODO: Implement actual MQTT message sending
            session.Invalidate("left");

            if (CurrentSession == session)
            {
                CurrentSession = null;
                _wasInSession = false;
            }
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

            // TODO: Implement actual MQTT message sending
            return entity;
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

            // TODO: Implement actual MQTT message sending
        }

        /// <summary>
        /// Update entity position.
        /// </summary>
        internal async Task UpdateEntityPositionAsync(SyncSession session, string entityId, SyncVector3 position)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.position", session.SessionId,
                    new { entityId, position });
                return;
            }

            // TODO: Implement actual MQTT message sending
        }

        /// <summary>
        /// Update entity rotation.
        /// </summary>
        internal async Task UpdateEntityRotationAsync(SyncSession session, string entityId, SyncQuaternion rotation)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.rotation", session.SessionId,
                    new { entityId, rotation });
                return;
            }

            // TODO: Implement actual MQTT message sending
        }

        /// <summary>
        /// Update entity scale.
        /// </summary>
        internal async Task UpdateEntityScaleAsync(SyncSession session, string entityId, SyncVector3 scale)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.scale", session.SessionId,
                    new { entityId, scale });
                return;
            }

            // TODO: Implement actual MQTT message sending
        }

        /// <summary>
        /// Set entity parent.
        /// </summary>
        internal async Task SetEntityParentAsync(SyncSession session, string childId, string parentId)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.parent", session.SessionId,
                    new { childId, parentId });
                return;
            }

            // TODO: Implement actual MQTT message sending
        }

        /// <summary>
        /// Set entity visibility.
        /// </summary>
        internal async Task SetEntityVisibilityAsync(SyncSession session, string entityId, bool visible)
        {
            if (!IsConnected)
            {
                QueueOrThrow<bool>("entity.update.visibility", session.SessionId,
                    new { entityId, visible });
                return;
            }

            // TODO: Implement actual MQTT message sending
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

            OnDisconnected?.Invoke(reason);

            if (!Config.AutoReconnect.Enabled)
            {
                State = ConnectionState.Disconnected;
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

                    // Process queued operations
                    await ProcessOperationQueueAsync();

                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[WorldSync] Reconnect attempt {_reconnectAttempt} failed: {ex.Message}");
                }
            }

            // All attempts failed
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
                // Session no longer exists
                CurrentSession = null;
                _wasInSession = false;
                OnSessionExpired?.Invoke(_lastSessionId);
            }
        }

        /// <summary>
        /// Process queued operations after reconnection.
        /// </summary>
        private async Task ProcessOperationQueueAsync()
        {
            List<PendingOperation> operations;

            lock (_queueLock)
            {
                operations = new List<PendingOperation>(_operationQueue);
                _operationQueue.Clear();
            }

            foreach (var op in operations)
            {
                try
                {
                    // TODO: Replay operation based on type
                    op.Completion?.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    op.Completion?.TrySetException(ex);
                }
            }
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
            // TODO: Use the existing MQTTClient from WebInterface.MQTT
            // This is a placeholder implementation
            await Task.Delay(100);
        }

        /// <summary>
        /// Disconnect from MQTT broker internally.
        /// </summary>
        private async Task DisconnectInternalAsync()
        {
            // TODO: Use the existing MQTTClient from WebInterface.MQTT
            await Task.Delay(50);
        }

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
    }
}
