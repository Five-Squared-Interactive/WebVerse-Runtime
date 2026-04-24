// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

#if USE_WEBINTERFACE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FiveSQD.StraightFour.Utilities;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.WorldSync;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldSync
{
    /// <summary>
    /// WorldSync (wsync) Session Management Methods exposed to JavaScript.
    /// Provides Create/Join/Exit/Destroy session lifecycle for WorldSync 2.0
    /// in parallel to the legacy <see cref="VOSSynchronization.VOSSynchronization"/> API.
    /// </summary>
    public class WorldSync
    {
        /// <summary>
        /// WorldSync Transports.
        /// </summary>
        public enum Transport { TCP, WebSocket }

        /// <summary>
        /// Test seam: incremented every time the JoinSession callback is invoked.
        /// Internal so Jint cannot expose it to world scripts.
        /// </summary>
        internal static int TestHook_JoinCallbackInvocations;

        /// <summary>
        /// Test seam: the last callback string passed to the JS engine.
        /// Internal so Jint cannot expose it to world scripts.
        /// </summary>
        internal static string TestHook_LastInvokedCallback;

        /// <summary>
        /// Test seam: incremented every time a RegisterMessageCallback handler is invoked.
        /// </summary>
        internal static int TestHook_MessageCallbackInvocations;

        /// <summary>
        /// Test seam: the last message callback string passed to CallWithParams.
        /// </summary>
        internal static string TestHook_LastMessageCallback;

        /// <summary>
        /// Test seam: incremented every time a message callback is re-attached after reconnection.
        /// </summary>
        internal static int TestHook_MessageCallbackReattachmentCount;

        /// <summary>
        /// Test seam: incremented every time a state-change callback is invoked.
        /// </summary>
        internal static int TestHook_StateChangeCallbackInvocations;

        /// <summary>
        /// Test seam: the last state-change callback string passed to CallWithParams.
        /// </summary>
        internal static string TestHook_LastStateChangeCallback;

        /// <summary>
        /// Tracks ids currently executing a Leave/Destroy helper so reentrant
        /// ExitSession/DestroySession calls no-op instead of racing on the same session.
        /// </summary>
        private static readonly HashSet<string> _exitingIds = new HashSet<string>();

        /// <summary>
        /// Tracks registered message callback handlers keyed by (sessionID, callback) for
        /// duplicate-handler guard and detach on ExitSession/DestroySession.
        /// </summary>
        private static readonly Dictionary<(string sessionID, string callback), Action<string, string, string>>
            _messageCallbackHandlers = new Dictionary<(string, string), Action<string, string, string>>();
        private static readonly object _callbackLock = new object();

        /// <summary>
        /// Test seam: clears the internal message callback handler dictionary.
        /// Call from TearDown to ensure test isolation.
        /// </summary>
        internal static void ClearMessageCallbackHandlers()
        {
            lock (_callbackLock)
            {
                _messageCallbackHandlers.Clear();
            }
        }

        /// <summary>
        /// Tracks registered state-change callback handlers keyed by (sessionID, callback).
        /// Each entry stores the list of Action delegates subscribed to the client's events
        /// so they can be detached on exit/destroy.
        /// </summary>
        private static readonly Dictionary<(string sessionID, string callback), List<Action>>
            _stateChangeCallbackHandlers = new Dictionary<(string, string), List<Action>>();

        /// <summary>
        /// Test seam: detaches all state-change event handlers and clears the dictionary.
        /// Call from TearDown BEFORE clients are destroyed to prevent stale callbacks
        /// firing during cleanup.
        /// </summary>
        internal static void ClearStateChangeCallbackHandlers()
        {
            lock (_callbackLock)
            {
                foreach (var kvp in _stateChangeCallbackHandlers)
                {
                    foreach (var detach in kvp.Value)
                    {
                        try { detach(); } catch { }
                    }
                }
                _stateChangeCallbackHandlers.Clear();
            }
        }

        private static bool EnsureRuntime(string caller)
        {
            if (WebVerseRuntime.Instance == null)
            {
                LogSystem.LogError("[WorldSync:" + caller + "] WebVerseRuntime.Instance is null.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create a WorldSync Session.
        /// </summary>
        /// <param name="host">Host of the WorldSync broker.</param>
        /// <param name="port">Port of the WorldSync broker.</param>
        /// <param name="tls">Whether or not to use TLS.</param>
        /// <param name="id">Caller-supplied identifier used as the WebVerseRuntime registry key.</param>
        /// <param name="tag">Human-readable tag (used for ClientTag and session tag).</param>
        /// <param name="transport">Transport to use.</param>
        /// <returns>Whether the operation was initiated successfully.</returns>
        public static bool CreateSession(string host, int port, bool tls, string id, string tag,
            Transport transport = Transport.TCP)
        {
            return CreateSession(host, port, tls, id, tag, Vector3.zero, transport);
        }

        /// <summary>
        /// Create a WorldSync Session.
        /// </summary>
        /// <param name="host">Host of the WorldSync broker.</param>
        /// <param name="port">Port of the WorldSync broker.</param>
        /// <param name="tls">Whether or not to use TLS.</param>
        /// <param name="id">Caller-supplied identifier used as the WebVerseRuntime registry key.</param>
        /// <param name="tag">Human-readable tag (used for ClientTag and session tag).</param>
        /// <param name="worldOffset">Offset for this client in the world. Currently a no-op for VOS API parity (see Epic 4 backlog).</param>
        /// <param name="transport">Transport to use.</param>
        /// <param name="clientID">Optional client ID. A new GUID is generated when null.</param>
        /// <param name="clientToken">Optional client authentication token.</param>
        /// <returns>Whether the operation was initiated successfully.</returns>
        public static bool CreateSession(string host, int port, bool tls, string id, string tag,
            Vector3 worldOffset, // TODO(Epic 4): route worldOffset into WorldSyncConfig when supported.
            Transport transport = Transport.TCP,
            string clientID = null, string clientToken = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                LogSystem.LogError("[WorldSync:CreateSession] id is required.");
                return false;
            }
            if (!EnsureRuntime("CreateSession")) return false;

            if (WebVerseRuntime.Instance.GetWorldSyncClient(id) != null)
            {
                LogSystem.LogError("[WorldSync:CreateSession] id '" + id +
                    "' already has a registered client; call ExitSession or DestroySession first.");
                return false;
            }

            WorldSyncClient client;
            try
            {
                var config = WorldSyncConfig.Builder()
                    .WithHost(host)
                    .WithPort(port)
                    .WithTls(tls)
                    .WithTransport(transport == Transport.TCP
                        ? WorldSyncTransport.TCP : WorldSyncTransport.WebSocket)
                    .WithClientId(string.IsNullOrEmpty(clientID) ? Guid.NewGuid().ToString() : clientID)
                    .WithClientToken(clientToken)
                    .WithClientTag(string.IsNullOrEmpty(tag) ? id : tag)
                    .Build();
                client = new WorldSyncClient(config);
            }
            catch (Exception ex)
            {
                LogSystem.LogError("[WorldSync:CreateSession] Invalid config: " + ex.Message);
                return false;
            }

            WebVerseRuntime.Instance.RegisterWorldSyncClient(id, client);
            _ = ConnectAndCreateAsync(client, string.IsNullOrEmpty(tag) ? id : tag, id);
            return true;
        }

        /// <summary>
        /// Join a WorldSync Session.
        /// </summary>
        /// <param name="host">Host of the WorldSync broker.</param>
        /// <param name="port">Port of the WorldSync broker.</param>
        /// <param name="tls">Whether or not to use TLS.</param>
        /// <param name="id">Caller-supplied identifier used as the WebVerseRuntime registry key.</param>
        /// <param name="tag">Human-readable tag (used for ClientTag).</param>
        /// <param name="sessionId">Identifier of the existing session to join.</param>
        /// <param name="callback">Optional JS function name invoked after the join succeeds.</param>
        /// <param name="transport">Transport to use.</param>
        /// <param name="clientID">Optional client ID.</param>
        /// <param name="clientToken">Optional client authentication token.</param>
        /// <returns>The local client ID (Config.ClientId), or null on failure.</returns>
        public static string JoinSession(string host, int port, bool tls, string id, string tag,
            string sessionId, string callback = null, Transport transport = Transport.TCP,
            string clientID = null, string clientToken = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                LogSystem.LogError("[WorldSync:JoinSession] id is required.");
                return null;
            }
            if (string.IsNullOrEmpty(sessionId))
            {
                LogSystem.LogError("[WorldSync:JoinSession] sessionId is required.");
                return null;
            }
            if (!EnsureRuntime("JoinSession")) return null;

            if (WebVerseRuntime.Instance.GetWorldSyncClient(id) != null)
            {
                LogSystem.LogError("[WorldSync:JoinSession] id '" + id +
                    "' already has a registered client; call ExitSession or DestroySession first.");
                return null;
            }

            WorldSyncClient client;
            try
            {
                var config = WorldSyncConfig.Builder()
                    .WithHost(host)
                    .WithPort(port)
                    .WithTls(tls)
                    .WithTransport(transport == Transport.TCP
                        ? WorldSyncTransport.TCP : WorldSyncTransport.WebSocket)
                    .WithClientId(string.IsNullOrEmpty(clientID) ? Guid.NewGuid().ToString() : clientID)
                    .WithClientToken(clientToken)
                    .WithClientTag(string.IsNullOrEmpty(tag) ? id : tag)
                    .Build();
                client = new WorldSyncClient(config);
            }
            catch (Exception ex)
            {
                LogSystem.LogError("[WorldSync:JoinSession] Invalid config: " + ex.Message);
                return null;
            }

            WebVerseRuntime.Instance.RegisterWorldSyncClient(id, client);
            _ = ConnectAndJoinAsync(client, sessionId, callback, id);
            return client.Config.ClientId;
        }

        /// <summary>
        /// Exit a WorldSync Session — leaves the current session and disconnects the client.
        /// </summary>
        /// <param name="id">Identifier the client was registered with.</param>
        /// <returns>True if a registered client was found and exit was initiated; false otherwise.</returns>
        public static bool ExitSession(string id)
        {
            if (!EnsureRuntime("ExitSession")) return false;

            var client = WebVerseRuntime.Instance.GetWorldSyncClient(id);
            if (client == null)
            {
                LogSystem.LogError("[WorldSync:ExitSession] No WorldSyncClient registered for id: " + id);
                return false;
            }

            lock (_exitingIds)
            {
                if (_exitingIds.Contains(id))
                {
                    LogSystem.LogError("[WorldSync:ExitSession] Exit already in progress for id: " + id);
                    return false;
                }
                _exitingIds.Add(id);
            }

            // Detach callbacks before async leave.
            DetachMessageCallbacks(id, client.CurrentSession);
            DetachStateChangeCallbacks(id, client);
            _ = LeaveAndDisconnectAsync(client, id);
            return true;
        }

        /// <summary>
        /// Destroy a WorldSync Session — destroys the server session (owner-only) and disconnects.
        /// </summary>
        /// <param name="id">Identifier the client was registered with.</param>
        /// <returns>True if a registered client was found and destroy was initiated; false otherwise.</returns>
        public static bool DestroySession(string id)
        {
            if (!EnsureRuntime("DestroySession")) return false;

            var client = WebVerseRuntime.Instance.GetWorldSyncClient(id);
            if (client == null)
            {
                LogSystem.LogError("[WorldSync:DestroySession] No WorldSyncClient registered for id: " + id);
                return false;
            }

            lock (_exitingIds)
            {
                if (_exitingIds.Contains(id))
                {
                    LogSystem.LogError("[WorldSync:DestroySession] Exit already in progress for id: " + id);
                    return false;
                }
                _exitingIds.Add(id);
            }

            // Detach callbacks before async destroy.
            DetachMessageCallbacks(id, client.CurrentSession);
            DetachStateChangeCallbacks(id, client);
            _ = DestroyAndDisconnectAsync(client, id);
            return true;
        }

        /// <summary>
        /// Indicates whether a session is established for the given id.
        /// </summary>
        /// <param name="id">Identifier the client was registered with.</param>
        /// <returns>True only if a registered client exists with a non-null, valid CurrentSession.</returns>
        public static bool IsSessionEstablished(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            if (WebVerseRuntime.Instance == null)
            {
                return false;
            }
            var client = WebVerseRuntime.Instance.GetWorldSyncClient(id);
            return client != null && client.CurrentSession != null && client.CurrentSession.IsValid;
        }

        /// <summary>
        /// Start synchronizing a local entity with the WorldSync session.
        /// Creates a server-side mirror entity and registers a bridge that forwards
        /// local transform changes to the session.
        /// </summary>
        /// <param name="sessionID">Identifier the client was registered with.</param>
        /// <param name="entityID">GUID string of the local StraightFour entity.</param>
        /// <param name="deleteWithClient">Whether to delete the server entity when the bridge is stopped.</param>
        /// <param name="filePath">Optional file path associated with the entity.</param>
        /// <param name="resources">Optional resources associated with the entity.</param>
        /// <returns>True if the bridge was registered and entity creation was initiated.</returns>
        public static bool StartSynchronizingEntity(string sessionID, string entityID,
            bool deleteWithClient = false, string filePath = null, string[] resources = null)
        {
            if (!EnsureRuntime("StartSynchronizingEntity")) return false;

            if (string.IsNullOrEmpty(sessionID))
            {
                LogSystem.LogError("[WorldSync:StartSynchronizingEntity] sessionID is required.");
                return false;
            }
            if (string.IsNullOrEmpty(entityID))
            {
                LogSystem.LogError("[WorldSync:StartSynchronizingEntity] entityID is required.");
                return false;
            }

            var client = WebVerseRuntime.Instance.GetWorldSyncClient(sessionID);
            if (client == null)
            {
                LogSystem.LogError("[WorldSync:StartSynchronizingEntity] No WorldSyncClient registered for sessionID: " + sessionID);
                return false;
            }

            if (client.CurrentSession == null || !client.CurrentSession.IsValid)
            {
                LogSystem.LogError("[WorldSync:StartSynchronizingEntity] Session is not valid for sessionID: " + sessionID);
                return false;
            }

            if (!Guid.TryParse(entityID, out Guid uuid))
            {
                LogSystem.LogError("[WorldSync:StartSynchronizingEntity] Invalid entity UUID: " + entityID);
                return false;
            }

            if (StraightFour.StraightFour.ActiveWorld == null
                || StraightFour.StraightFour.ActiveWorld.entityManager == null)
            {
                LogSystem.LogError("[WorldSync:StartSynchronizingEntity] Unable to find entity: " + entityID);
                return false;
            }

            StraightFour.Entity.BaseEntity localEntity =
                StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(uuid);
            if (localEntity == null)
            {
                LogSystem.LogError("[WorldSync:StartSynchronizingEntity] Unable to find entity: " + entityID);
                return false;
            }

            var bridge = new WorldSyncEntityBridge(client, localEntity, deleteWithClient, filePath, resources);
            if (!client.TryAddEntityBridge(uuid, bridge))
            {
                LogSystem.LogError("[WorldSync:StartSynchronizingEntity] Entity already bridged: " + entityID);
                return false;
            }

            _ = StartBridgeAsync(bridge, sessionID, entityID);
            return true;
        }

        /// <summary>
        /// Stop synchronizing a local entity with the WorldSync session.
        /// Removes the bridge; optionally deletes the server-side entity if registered with deleteWithClient=true.
        /// </summary>
        /// <param name="sessionID">Identifier the client was registered with.</param>
        /// <param name="entityID">GUID string of the local StraightFour entity.</param>
        /// <returns>True if a bridge was found and removed.</returns>
        public static bool StopSynchronizingEntity(string sessionID, string entityID)
        {
            if (!EnsureRuntime("StopSynchronizingEntity")) return false;

            if (string.IsNullOrEmpty(sessionID))
            {
                LogSystem.LogError("[WorldSync:StopSynchronizingEntity] sessionID is required.");
                return false;
            }
            if (string.IsNullOrEmpty(entityID))
            {
                LogSystem.LogError("[WorldSync:StopSynchronizingEntity] entityID is required.");
                return false;
            }

            var client = WebVerseRuntime.Instance.GetWorldSyncClient(sessionID);
            if (client == null)
            {
                LogSystem.LogError("[WorldSync:StopSynchronizingEntity] No WorldSyncClient registered for sessionID: " + sessionID);
                return false;
            }

            if (!Guid.TryParse(entityID, out Guid uuid))
            {
                LogSystem.LogError("[WorldSync:StopSynchronizingEntity] Invalid entity UUID: " + entityID);
                return false;
            }

            var bridge = client.TryRemoveEntityBridge(uuid);
            if (bridge == null)
            {
                LogSystem.LogError("[WorldSync:StopSynchronizingEntity] No bridge registered for entity: " + entityID);
                return false;
            }

            bridge.Stop();
            return true;
        }

        /// <summary>
        /// Send a custom message through the WorldSync session.
        /// </summary>
        /// <param name="sessionID">Identifier the client was registered with.</param>
        /// <param name="topic">Application-specific message topic (required, non-empty).</param>
        /// <param name="message">Message payload (may be empty).</param>
        /// <returns>True if the message was sent successfully.</returns>
        public static bool SendMessage(string sessionID, string topic, string message)
        {
            if (!EnsureRuntime("SendMessage")) return false;

            if (string.IsNullOrEmpty(sessionID))
            {
                LogSystem.LogError("[WorldSync:SendMessage] sessionID is required.");
                return false;
            }
            if (string.IsNullOrEmpty(topic))
            {
                LogSystem.LogError("[WorldSync:SendMessage] topic is required.");
                return false;
            }

            var client = WebVerseRuntime.Instance.GetWorldSyncClient(sessionID);
            if (client == null)
            {
                LogSystem.LogError("[WorldSync:SendMessage] No WorldSyncClient registered for sessionID: " + sessionID);
                return false;
            }

            if (client.CurrentSession == null || !client.CurrentSession.IsValid)
            {
                LogSystem.LogError("[WorldSync:SendMessage] Session is not valid for sessionID: " + sessionID);
                return false;
            }

            try
            {
                client.CurrentSession.SendMessage(topic, message);
                return true;
            }
            catch (Exception ex)
            {
                LogSystem.LogError("[WorldSync:SendMessage] " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Register a callback for custom messages received on the WorldSync session.
        /// The callback is invoked with (topic, senderId, payload) parameters.
        /// Duplicate registration for the same (sessionID, callback) pair is a no-op.
        /// </summary>
        /// <param name="sessionID">Identifier the client was registered with.</param>
        /// <param name="callback">JS function name to invoke on message receipt.</param>
        /// <returns>True if the handler was registered (or was already registered).</returns>
        public static bool RegisterMessageCallback(string sessionID, string callback)
        {
            if (!EnsureRuntime("RegisterMessageCallback")) return false;

            if (string.IsNullOrEmpty(sessionID))
            {
                LogSystem.LogError("[WorldSync:RegisterMessageCallback] sessionID is required.");
                return false;
            }
            if (string.IsNullOrEmpty(callback))
            {
                LogSystem.LogError("[WorldSync:RegisterMessageCallback] callback is required.");
                return false;
            }

            var client = WebVerseRuntime.Instance.GetWorldSyncClient(sessionID);
            if (client == null)
            {
                LogSystem.LogError("[WorldSync:RegisterMessageCallback] No WorldSyncClient registered for sessionID: " + sessionID);
                return false;
            }

            if (client.CurrentSession == null || !client.CurrentSession.IsValid)
            {
                LogSystem.LogError("[WorldSync:RegisterMessageCallback] Session is not valid for sessionID: " + sessionID);
                return false;
            }

            var key = (sessionID, callback);
            lock (_callbackLock)
            {
                if (_messageCallbackHandlers.ContainsKey(key))
                {
                    // Duplicate-handler guard: already attached, no-op.
                    return true;
                }

                Action<string, string, string> handler = (string topic, string senderId, string payload) =>
                {
                    TestHook_LastMessageCallback = callback;
                    System.Threading.Interlocked.Increment(ref TestHook_MessageCallbackInvocations);
                    WebVerseRuntime.Instance?.javascriptHandler?.CallWithParams(callback,
                        new object[] { topic, senderId, payload });
                };

                client.CurrentSession.OnCustomMessage += handler;
                _messageCallbackHandlers[key] = handler;
            }

            return true;
        }

        /// <summary>
        /// Get the local client ID for the given session.
        /// </summary>
        /// <param name="sessionID">Identifier the client was registered with.</param>
        /// <returns>The session's LocalClientId, or null if not found.</returns>
        public static string GetLocalClientId(string sessionID)
        {
            if (string.IsNullOrEmpty(sessionID)) return null;
            if (WebVerseRuntime.Instance == null) return null;

            var client = WebVerseRuntime.Instance.GetWorldSyncClient(sessionID);
            if (client?.CurrentSession == null) return null;
            return client.CurrentSession.LocalClientId;
        }

        /// <summary>
        /// Get the current connection state of a WorldSync client.
        /// </summary>
        /// <param name="sessionID">Identifier the client was registered with.</param>
        /// <returns>Connection state string ("connected", "reconnecting", "disconnected", etc.), or null if not found.</returns>
        public static string GetConnectionState(string sessionID)
        {
            if (string.IsNullOrEmpty(sessionID)) return null;
            if (WebVerseRuntime.Instance == null) return null;

            var client = WebVerseRuntime.Instance.GetWorldSyncClient(sessionID);
            if (client == null) return null;

            return client.State.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Register a callback for connection state changes on a WorldSync client.
        /// The callback is invoked with (sessionID, newStateString) parameters.
        /// Duplicate registration for the same (sessionID, callback) pair is a no-op.
        /// </summary>
        /// <param name="sessionID">Identifier the client was registered with.</param>
        /// <param name="callback">JS function name to invoke on state change.</param>
        /// <returns>True if the handler was registered (or was already registered); false on error.</returns>
        public static bool OnConnectionStateChanged(string sessionID, string callback)
        {
            if (!EnsureRuntime("OnConnectionStateChanged")) return false;

            if (string.IsNullOrEmpty(sessionID))
            {
                LogSystem.LogError("[WorldSync:OnConnectionStateChanged] sessionID is required.");
                return false;
            }
            if (string.IsNullOrEmpty(callback))
            {
                LogSystem.LogError("[WorldSync:OnConnectionStateChanged] callback is required.");
                return false;
            }

            var client = WebVerseRuntime.Instance.GetWorldSyncClient(sessionID);
            if (client == null)
            {
                LogSystem.LogError("[WorldSync:OnConnectionStateChanged] No WorldSyncClient registered for sessionID: " + sessionID);
                return false;
            }

            var key = (sessionID, callback);
            lock (_callbackLock)
            {
                if (_stateChangeCallbackHandlers.ContainsKey(key))
                {
                    // Duplicate-handler guard: already attached, no-op.
                    return true;
                }

                var delegates = new List<Action>();

                Action onConnected = () =>
                {
                    TestHook_LastStateChangeCallback = callback;
                    System.Threading.Interlocked.Increment(ref TestHook_StateChangeCallbackInvocations);
                    WebVerseRuntime.Instance?.javascriptHandler?.CallWithParams(callback,
                        new object[] { sessionID, "connected" });
                };

                Action<int> onReconnecting = (attempt) =>
                {
                    TestHook_LastStateChangeCallback = callback;
                    System.Threading.Interlocked.Increment(ref TestHook_StateChangeCallbackInvocations);
                    WebVerseRuntime.Instance?.javascriptHandler?.CallWithParams(callback,
                        new object[] { sessionID, "reconnecting" });
                };

                Action onReconnected = () =>
                {
                    TestHook_LastStateChangeCallback = callback;
                    System.Threading.Interlocked.Increment(ref TestHook_StateChangeCallbackInvocations);
                    WebVerseRuntime.Instance?.javascriptHandler?.CallWithParams(callback,
                        new object[] { sessionID, "connected" });
                };

                Action<string> onDisconnected = (reason) =>
                {
                    TestHook_LastStateChangeCallback = callback;
                    System.Threading.Interlocked.Increment(ref TestHook_StateChangeCallbackInvocations);
                    WebVerseRuntime.Instance?.javascriptHandler?.CallWithParams(callback,
                        new object[] { sessionID, "disconnected" });
                };

                Action<int> onReconnectionFailed = (attempts) =>
                {
                    TestHook_LastStateChangeCallback = callback;
                    System.Threading.Interlocked.Increment(ref TestHook_StateChangeCallbackInvocations);
                    WebVerseRuntime.Instance?.javascriptHandler?.CallWithParams(callback,
                        new object[] { sessionID, "disconnected" });
                };

                client.OnConnected += onConnected;
                client.OnReconnecting += onReconnecting;
                client.OnReconnected += onReconnected;
                client.OnDisconnected += onDisconnected;
                client.OnReconnectionFailed += onReconnectionFailed;

                // Store detach actions — each one unsubscribes from the corresponding event.
                delegates.Add(() => client.OnConnected -= onConnected);
                delegates.Add(() => client.OnReconnecting -= onReconnecting);
                delegates.Add(() => client.OnReconnected -= onReconnected);
                delegates.Add(() => client.OnDisconnected -= onDisconnected);
                delegates.Add(() => client.OnReconnectionFailed -= onReconnectionFailed);

                _stateChangeCallbackHandlers[key] = delegates;
            }

            return true;
        }

        /// <summary>
        /// Detach all registered state-change callbacks for the given session id.
        /// Called before ExitSession/DestroySession async helpers.
        /// </summary>
        private static void DetachStateChangeCallbacks(string sessionID, WorldSyncClient client)
        {
            if (client == null) return;

            lock (_callbackLock)
            {
                var keysToRemove = new List<(string, string)>();
                foreach (var kvp in _stateChangeCallbackHandlers)
                {
                    if (kvp.Key.sessionID == sessionID)
                    {
                        // Run all detach actions — swallow exceptions from disposed clients.
                        foreach (var detach in kvp.Value)
                        {
                            try { detach(); } catch { }
                        }
                        keysToRemove.Add(kvp.Key);
                    }
                }
                foreach (var key in keysToRemove)
                {
                    _stateChangeCallbackHandlers.Remove(key);
                }
            }
        }

        /// <summary>
        /// Detach all registered message callbacks for the given session id.
        /// Called before ExitSession/DestroySession async helpers.
        /// </summary>
        private static void DetachMessageCallbacks(string sessionID, SyncSession session)
        {
            if (session == null) return;

            lock (_callbackLock)
            {
                var keysToRemove = new List<(string, string)>();
                foreach (var kvp in _messageCallbackHandlers)
                {
                    if (kvp.Key.sessionID == sessionID)
                    {
                        session.OnCustomMessage -= kvp.Value;
                        keysToRemove.Add(kvp.Key);
                    }
                }
                foreach (var key in keysToRemove)
                {
                    _messageCallbackHandlers.Remove(key);
                }
            }
        }

        /// <summary>
        /// Re-attach all registered message callbacks for the given session to the client's new CurrentSession.
        /// Called after successful session recovery (OnStateRecovered).
        /// </summary>
        private static void ReattachMessageCallbacks(string sessionID, WorldSyncClient client)
        {
            if (client?.CurrentSession == null) return;

            lock (_callbackLock)
            {
                foreach (var kvp in _messageCallbackHandlers)
                {
                    if (kvp.Key.sessionID == sessionID)
                    {
                        client.CurrentSession.OnCustomMessage += kvp.Value;
                        System.Threading.Interlocked.Increment(ref TestHook_MessageCallbackReattachmentCount);
                    }
                }
            }
        }

        /// <summary>
        /// Handle session expiry: detach message and state-change callbacks for the expired session.
        /// </summary>
        private static void HandleSessionExpired(string sessionID, WorldSyncClient client)
        {
            var oldSession = client.LastExpiredSession;
            lock (_callbackLock)
            {
                var keysToRemove = new List<(string, string)>();
                foreach (var kvp in _messageCallbackHandlers)
                {
                    if (kvp.Key.sessionID == sessionID)
                    {
                        // Unsubscribe from old session's event if available.
                        if (oldSession != null)
                        {
                            try { oldSession.OnCustomMessage -= kvp.Value; } catch { }
                        }
                        keysToRemove.Add(kvp.Key);
                    }
                }
                foreach (var key in keysToRemove)
                {
                    _messageCallbackHandlers.Remove(key);
                }
            }
            DetachStateChangeCallbacks(sessionID, client);
        }

        private static async Task StartBridgeAsync(WorldSyncEntityBridge bridge, string sessionID, string entityID)
        {
            try
            {
                bool started = await bridge.StartAsync();
                if (started)
                {
                    // Start polling loop on the runtime MonoBehaviour to forward transform changes at ~20 Hz.
                    WebVerseRuntime.Instance?.StartCoroutine(PollBridgeCoroutine(bridge));
                }
            }
            catch (Exception ex)
            {
                // Rollback bridge registration so a retry is possible (AC6: no partial state).
                var client = WebVerseRuntime.Instance?.GetWorldSyncClient(sessionID);
                client?.TryRemoveEntityBridge(bridge.LocalEntityId);
                LogSystem.LogError("[WorldSync:StartSynchronizingEntity] Bridge start failed for entity="
                    + entityID + " session=" + sessionID + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Coroutine that polls a bridge's transform at ~20 Hz until the bridge is no longer active.
        /// </summary>
        private static IEnumerator PollBridgeCoroutine(WorldSyncEntityBridge bridge)
        {
            var wait = new UnityEngine.WaitForSeconds(0.05f);
            while (bridge.IsActive)
            {
                bridge.PollTransformChanges();
                yield return wait;
            }
        }

        private static void HandleBridgeResumed(WorldSyncEntityBridge bridge)
        {
            WebVerseRuntime.Instance?.StartCoroutine(PollBridgeCoroutine(bridge));
        }

        private static async Task ConnectAndCreateAsync(WorldSyncClient client, string tag, string id)
        {
            try
            {
                // Subscribe to bridge resume events so polling coroutines restart after reconnect.
                client.OnBridgeResumed += HandleBridgeResumed;
                // Re-attach message callbacks after session recovery.
                client.OnStateRecovered += () => ReattachMessageCallbacks(id, client);
                // Clean up callbacks on session expiry.
                client.OnSessionExpired += (_) => HandleSessionExpired(id, client);

                await client.ConnectAsync();
                await client.CreateSessionAsync(tag);
            }
            catch (Exception ex)
            {
                LogSystem.LogError("[WorldSync:CreateSession] Connect/create failed for id=" + id + ": " + ex.Message);
            }
        }

        private static async Task ConnectAndJoinAsync(WorldSyncClient client, string sessionId,
            string callback, string id)
        {
            try
            {
                // Subscribe to bridge resume events so polling coroutines restart after reconnect.
                client.OnBridgeResumed += HandleBridgeResumed;
                // Re-attach message callbacks after session recovery.
                client.OnStateRecovered += () => ReattachMessageCallbacks(id, client);
                // Clean up callbacks on session expiry.
                client.OnSessionExpired += (_) => HandleSessionExpired(id, client);

                await client.ConnectAsync();
                await client.JoinSessionAsync(sessionId);
                if (!string.IsNullOrEmpty(callback))
                {
                    TestHook_LastInvokedCallback = callback;
                    WebVerseRuntime.Instance.javascriptHandler.Run(callback);
                    System.Threading.Interlocked.Increment(ref TestHook_JoinCallbackInvocations);
                }
            }
            catch (Exception ex)
            {
                LogSystem.LogError("[WorldSync:JoinSession] Connect/join failed for id=" + id + ": " + ex.Message);
            }
        }

        private static async Task LeaveAndDisconnectAsync(WorldSyncClient client, string id)
        {
            try
            {
                if (client.CurrentSession != null && client.CurrentSession.IsValid)
                {
                    client.CurrentSession.Leave();
                }
                await client.DisconnectAsync();
            }
            catch (Exception ex)
            {
                LogSystem.LogError("[WorldSync:ExitSession] Leave/disconnect failed for id=" + id + ": " + ex.Message);
            }
            finally
            {
                // Note: DetachMessageCallbacks + DetachStateChangeCallbacks already called
                // synchronously in ExitSession before this async helper was launched.
                WebVerseRuntime.Instance?.UnregisterWorldSyncClient(id);
                lock (_exitingIds) { _exitingIds.Remove(id); }
            }
        }

        private static async Task DestroyAndDisconnectAsync(WorldSyncClient client, string id)
        {
            try
            {
                if (client.CurrentSession != null && client.CurrentSession.IsValid)
                {
                    client.CurrentSession.Destroy();
                }
                await client.DisconnectAsync();
            }
            catch (Exception ex)
            {
                LogSystem.LogError("[WorldSync:DestroySession] Destroy/disconnect failed for id=" + id + ": " + ex.Message);
            }
            finally
            {
                // Note: DetachMessageCallbacks + DetachStateChangeCallbacks already called
                // synchronously in DestroySession before this async helper was launched.
                WebVerseRuntime.Instance?.UnregisterWorldSyncClient(id);
                lock (_exitingIds) { _exitingIds.Remove(id); }
            }
        }
    }
}
#endif
