// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FiveSQD.WebVerse.Utilities;

[assembly: InternalsVisibleTo("FiveSQD.WebVerse.WebInterface.SocketIO.Tests")]

namespace FiveSQD.WebVerse.WebInterface.SocketIO
{
    /// <summary>
    /// Socket.IO client manager. Provides connect/disconnect lifecycle,
    /// state machine, and event dispatch for Socket.IO communication.
    /// </summary>
    public class SocketIOClient : BaseManager
    {
        /// <summary>
        /// Current connection state.
        /// </summary>
        private ConnectionState connectionState = ConnectionState.Disconnected;

        /// <summary>
        /// The transport implementation.
        /// </summary>
        private ISocketIOTransport transport;

        /// <summary>
        /// Internal setter for transport injection (used by tests).
        /// </summary>
        internal ISocketIOTransport Transport
        {
            get { return transport; }
            set
            {
                UnwireTransportEvents();
                transport = value;
                WireTransportEvents();
            }
        }

        /// <summary>
        /// Connection options for the current/next connection.
        /// </summary>
        private SocketIOOptions options;

        /// <summary>
        /// Whether the last disconnect was initiated by the client (Disconnect() call).
        /// When true, HandleDisconnected will NOT attempt reconnection.
        /// </summary>
        private bool clientInitiatedDisconnect;

        /// <summary>
        /// The URL for the current/next connection.
        /// </summary>
        private string url;

        /// <summary>
        /// Outbound message queue for buffering during Reconnecting state.
        /// </summary>
        private MessageQueue messageQueue;

        /// <summary>
        /// Cached namespace socket instances, keyed by namespace path.
        /// </summary>
        private Dictionary<string, NamespacedSocket> namespaceCache
            = new Dictionary<string, NamespacedSocket>();

        /// <summary>
        /// Callbacks for system and user events.
        /// </summary>
        private Dictionary<string, List<Action<string>>> eventCallbacks
            = new Dictionary<string, List<Action<string>>>();

        /// <summary>
        /// Callbacks for binary events. Separate from string callbacks
        /// because binary events have a different signature: (eventName, byte[]).
        /// </summary>
        private Dictionary<string, List<Action<string, byte[]>>> binaryEventCallbacks
            = new Dictionary<string, List<Action<string, byte[]>>>();

        /// <summary>
        /// One-time callbacks for named events. Stored separately from eventCallbacks
        /// to allow safe remove-before-invoke during EmitEvent dispatch.
        /// </summary>
        private Dictionary<string, List<Action<string>>> onceCallbacks
            = new Dictionary<string, List<Action<string>>>();

        /// <summary>
        /// Catch-all callbacks invoked for every event with (eventName, data).
        /// </summary>
        private List<Action<string, string>> onAnyCallbacks
            = new List<Action<string, string>>();

        /// <summary>
        /// Whether the client is currently connected.
        /// </summary>
        public bool Connected
        {
            get { return connectionState == ConnectionState.Connected; }
        }

        /// <summary>
        /// The server-assigned socket ID.
        /// </summary>
        public string Id
        {
            get { return transport?.SocketId; }
        }

        /// <summary>
        /// The current connection state (exposed for testing).
        /// </summary>
        internal ConnectionState State
        {
            get { return connectionState; }
        }

        /// <summary>
        /// Initialize the Socket.IO client.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            connectionState = ConnectionState.Disconnected;
            namespaceCache = new Dictionary<string, NamespacedSocket>();
            eventCallbacks = new Dictionary<string, List<Action<string>>>();
            binaryEventCallbacks = new Dictionary<string, List<Action<string, byte[]>>>();
            onceCallbacks = new Dictionary<string, List<Action<string>>>();
            onAnyCallbacks = new List<Action<string, string>>();

#if USE_BESTHTTP
            transport = new BestSocketIOTransport();
#else
            Logging.LogWarning("[SocketIOClient] USE_BESTHTTP not defined. Socket.IO unavailable.");
#endif

            if (transport != null)
            {
                WireTransportEvents();
            }
        }

        /// <summary>
        /// Terminate the Socket.IO client. Disconnects and releases all resources.
        /// </summary>
        public override void Terminate()
        {
            if (connectionState == ConnectionState.Connected ||
                connectionState == ConnectionState.Connecting ||
                connectionState == ConnectionState.Reconnecting)
            {
                if (transport != null)
                {
                    try
                    {
                        transport.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[SocketIOClient] Terminate disconnect error: " + ex.Message);
                    }
                }
            }

            UnwireTransportEvents();

            // Dispose all namespace instances
            if (namespaceCache != null)
            {
                foreach (var kvp in namespaceCache)
                {
                    kvp.Value?.Dispose();
                }
                namespaceCache.Clear();
            }

            eventCallbacks?.Clear();
            binaryEventCallbacks?.Clear();
            onceCallbacks?.Clear();
            onAnyCallbacks?.Clear();
            messageQueue?.Clear();
            connectionState = ConnectionState.Disconnected;

            base.Terminate();
        }

        /// <summary>
        /// Connect to a Socket.IO server.
        /// </summary>
        /// <param name="url">The server URL.</param>
        /// <param name="options">Connection options (uses defaults if null).</param>
        public void Connect(string url, SocketIOOptions options = null)
        {
            if (transport == null)
            {
                Logging.LogWarning("[SocketIOClient->Connect] Transport not initialized.");
                return;
            }

            if (connectionState != ConnectionState.Disconnected)
            {
                Logging.LogWarning("[SocketIOClient->Connect] Not in Disconnected state.");
                return;
            }

            this.url = url;
            this.options = options ?? new SocketIOOptions();
            clientInitiatedDisconnect = false;
            messageQueue = new MessageQueue(this.options.queueSize);

            SetState(ConnectionState.Connecting);
            transport.Connect(url, this.options);
        }

        /// <summary>
        /// Disconnect from the Socket.IO server.
        /// </summary>
        public void Disconnect()
        {
            if (connectionState == ConnectionState.Disconnected)
            {
                Logging.LogWarning("[SocketIOClient->Disconnect] Already disconnected.");
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[SocketIOClient->Disconnect] Transport not initialized.");
                return;
            }

            if (connectionState == ConnectionState.Reconnecting)
            {
                // Cancel reconnection
                clientInitiatedDisconnect = true;
                Logging.Log("[SocketIOClient] Reconnection cancelled by client disconnect");
                SetState(ConnectionState.Disconnected);
                transport.Disconnect();
                return;
            }

            if (connectionState != ConnectionState.Connected)
            {
                Logging.LogWarning("[SocketIOClient->Disconnect] Not in Connected state.");
                return;
            }

            clientInitiatedDisconnect = true;
            SetState(ConnectionState.Disconnecting);
            transport.Disconnect();
        }

        /// <summary>
        /// Emit a named event with JSON data to the server.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">JSON data string.</param>
        public void Emit(string eventName, string data)
        {
            if (transport == null)
            {
                Logging.LogWarning("[SocketIOClient->Emit] Transport not initialized.");
                return;
            }

            if (connectionState == ConnectionState.Connected)
            {
                transport.Emit(eventName, data);
                return;
            }

            if (connectionState == ConnectionState.Reconnecting && messageQueue != null)
            {
                if (messageQueue.IsFull)
                {
                    Logging.LogWarning("[SocketIOClient] Message queue full, dropping oldest");
                }
                messageQueue.Enqueue(eventName, data);
                return;
            }

            Logging.LogWarning("[SocketIOClient->Emit] Not connected, message dropped.");
        }

        /// <summary>
        /// Register a callback for a named event.
        /// Works for both system events (connect, disconnect, connect_error)
        /// and user events.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="callback">The callback to invoke.</param>
        public void On(string eventName, Action<string> callback)
        {
            if (string.IsNullOrEmpty(eventName) || callback == null)
            {
                return;
            }

            if (!eventCallbacks.ContainsKey(eventName))
            {
                eventCallbacks[eventName] = new List<Action<string>>();
            }

            eventCallbacks[eventName].Add(callback);
        }

        /// <summary>
        /// Remove all callbacks for a named event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        public void Off(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            eventCallbacks.Remove(eventName);
            onceCallbacks.Remove(eventName);
        }

        /// <summary>
        /// Emit a binary event with byte data to the server.
        /// Binary messages are NOT queued during Reconnecting — they are dropped.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">Binary data.</param>
        public void EmitBinary(string eventName, byte[] data)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[SocketIOClient->EmitBinary] Transport not initialized.");
                return;
            }

            if (connectionState != ConnectionState.Connected)
            {
                Logging.LogWarning("[SocketIOClient->EmitBinary] Not connected, binary message dropped.");
                return;
            }

            transport.EmitBinary(eventName, data);
        }

        /// <summary>
        /// Emit a named event and wait for server acknowledgement.
        /// Ack messages are NOT queued during Reconnecting — ack callbacks
        /// are tied to a specific connection session.
        /// If ackCallback is null, falls back to a regular Emit (no ack overhead).
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">JSON data string.</param>
        /// <param name="ackCallback">Callback invoked with the server's ack response.</param>
        public void EmitWithAck(string eventName, string data, Action<string> ackCallback)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[SocketIOClient->EmitWithAck] Transport not initialized.");
                return;
            }

            if (connectionState != ConnectionState.Connected)
            {
                Logging.LogWarning("[SocketIOClient->EmitWithAck] Not connected, ack message dropped.");
                return;
            }

            if (ackCallback == null)
            {
                transport.Emit(eventName, data);
                return;
            }

            transport.EmitWithAck(eventName, data, ackCallback);
        }

        /// <summary>
        /// Emit a volatile event. When connected, delegates to transport.Emit.
        /// When not connected, silently drops the message (no queuing, no warning).
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">JSON data string.</param>
        public void EmitVolatile(string eventName, string data)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[SocketIOClient->EmitVolatile] Transport not initialized.");
                return;
            }

            if (connectionState != ConnectionState.Connected)
            {
                return; // SILENT drop -- volatile semantics
            }

            transport.Emit(eventName, data);
        }

        /// <summary>
        /// Request the server to join a named room.
        /// Only works when connected. Not queued during reconnection.
        /// </summary>
        /// <param name="room">The room name.</param>
        public void JoinRoom(string room)
        {
            if (string.IsNullOrEmpty(room))
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[SocketIOClient->JoinRoom] Transport not initialized.");
                return;
            }

            if (connectionState != ConnectionState.Connected)
            {
                Logging.LogWarning("[SocketIOClient->JoinRoom] Not connected.");
                return;
            }

            transport.JoinRoom(room);
        }

        /// <summary>
        /// Request the server to leave a named room.
        /// Only works when connected.
        /// </summary>
        /// <param name="room">The room name.</param>
        public void LeaveRoom(string room)
        {
            if (string.IsNullOrEmpty(room))
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[SocketIOClient->LeaveRoom] Transport not initialized.");
                return;
            }

            if (connectionState != ConnectionState.Connected)
            {
                Logging.LogWarning("[SocketIOClient->LeaveRoom] Not connected.");
                return;
            }

            transport.LeaveRoom(room);
        }

        /// <summary>
        /// Get or create a namespace-scoped socket instance.
        /// Returns a cached instance if the namespace was previously requested.
        /// Only works when connected.
        /// </summary>
        /// <param name="nsp">The namespace path (e.g., "/chat").</param>
        /// <returns>A NamespacedSocket for the namespace, or null if unavailable.</returns>
        public NamespacedSocket Of(string nsp)
        {
            if (string.IsNullOrEmpty(nsp))
            {
                return null;
            }

            if (transport == null)
            {
                Logging.LogWarning("[SocketIOClient->Of] Transport not initialized.");
                return null;
            }

            if (connectionState != ConnectionState.Connected)
            {
                Logging.LogWarning("[SocketIOClient->Of] Not connected.");
                return null;
            }

            if (namespaceCache.ContainsKey(nsp))
            {
                return namespaceCache[nsp];
            }

            var nsTransport = transport.Of(nsp);
            if (nsTransport == null)
            {
                Logging.LogWarning("[SocketIOClient->Of] Transport returned null for namespace: " + nsp);
                return null;
            }

            var ns = new NamespacedSocket(nsTransport, this, nsp);
            namespaceCache[nsp] = ns;
            return ns;
        }

        /// <summary>
        /// Register a one-time callback for a named event.
        /// The callback is automatically removed after the first invocation.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="callback">The callback to invoke once.</param>
        public void Once(string eventName, Action<string> callback)
        {
            if (string.IsNullOrEmpty(eventName) || callback == null)
            {
                return;
            }

            if (!onceCallbacks.ContainsKey(eventName))
            {
                onceCallbacks[eventName] = new List<Action<string>>();
            }

            onceCallbacks[eventName].Add(callback);
        }

        /// <summary>
        /// Register a catch-all callback invoked for every event.
        /// </summary>
        /// <param name="callback">The callback to invoke with (eventName, data).</param>
        public void OnAny(Action<string, string> callback)
        {
            if (callback == null)
            {
                return;
            }

            onAnyCallbacks.Add(callback);
        }

        /// <summary>
        /// Remove all catch-all callbacks registered via OnAny.
        /// </summary>
        public void OffAny()
        {
            onAnyCallbacks.Clear();
        }

        /// <summary>
        /// Register a callback for a named binary event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="callback">The callback to invoke with (eventName, data).</param>
        public void OnBinary(string eventName, Action<string, byte[]> callback)
        {
            if (string.IsNullOrEmpty(eventName) || callback == null)
            {
                return;
            }

            if (!binaryEventCallbacks.ContainsKey(eventName))
            {
                binaryEventCallbacks[eventName] = new List<Action<string, byte[]>>();
            }

            binaryEventCallbacks[eventName].Add(callback);
        }

        /// <summary>
        /// Remove all binary callbacks for a named event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        public void OffBinary(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            binaryEventCallbacks.Remove(eventName);
        }

        /// <summary>
        /// Set the connection state.
        /// </summary>
        /// <param name="newState">The new state.</param>
        private void SetState(ConnectionState newState)
        {
            connectionState = newState;
        }

        /// <summary>
        /// Emit a system/user event to all registered callbacks.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">The event data.</param>
        private void EmitEvent(string eventName, string data)
        {
            // 1. Regular callbacks
            if (eventCallbacks.ContainsKey(eventName))
            {
                var callbacks = eventCallbacks[eventName];
                for (int i = 0; i < callbacks.Count; i++)
                {
                    try
                    {
                        callbacks[i]?.Invoke(data);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[SocketIOClient] Callback error for event '"
                            + eventName + "': " + ex.Message);
                    }
                }
            }

            // 2. Once callbacks -- remove BEFORE invoking to prevent re-entry issues
            if (onceCallbacks.ContainsKey(eventName))
            {
                var once = onceCallbacks[eventName];
                onceCallbacks.Remove(eventName);
                for (int i = 0; i < once.Count; i++)
                {
                    try
                    {
                        once[i]?.Invoke(data);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[SocketIOClient] Once callback error for '"
                            + eventName + "': " + ex.Message);
                    }
                }
            }

            // 3. OnAny callbacks -- invoked for every event with (eventName, data)
            for (int i = 0; i < onAnyCallbacks.Count; i++)
            {
                try
                {
                    onAnyCallbacks[i]?.Invoke(eventName, data);
                }
                catch (Exception ex)
                {
                    Logging.LogError("[SocketIOClient] OnAny callback error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Subscribe to transport events.
        /// </summary>
        private void WireTransportEvents()
        {
            if (transport == null) return;

            transport.OnConnected += HandleConnected;
            transport.OnDisconnected += HandleDisconnected;
            transport.OnConnectError += HandleConnectError;
            transport.OnEvent += HandleEvent;
            transport.OnReconnectAttempt += HandleReconnectAttempt;
            transport.OnReconnected += HandleReconnected;
            transport.OnReconnectFailed += HandleReconnectFailed;
            transport.OnBinaryEvent += HandleBinaryEvent;
        }

        /// <summary>
        /// Unsubscribe from transport events.
        /// </summary>
        private void UnwireTransportEvents()
        {
            if (transport == null) return;

            transport.OnConnected -= HandleConnected;
            transport.OnDisconnected -= HandleDisconnected;
            transport.OnConnectError -= HandleConnectError;
            transport.OnEvent -= HandleEvent;
            transport.OnReconnectAttempt -= HandleReconnectAttempt;
            transport.OnReconnected -= HandleReconnected;
            transport.OnReconnectFailed -= HandleReconnectFailed;
            transport.OnBinaryEvent -= HandleBinaryEvent;
        }

        /// <summary>
        /// Handle transport connected event.
        /// </summary>
        /// <param name="socketId">The server-assigned socket ID.</param>
        private void HandleConnected(string socketId)
        {
            SetState(ConnectionState.Connected);
            Logging.Log("[SocketIOClient] Connected, id: " + socketId);
            EmitEvent("connect", socketId);
        }

        /// <summary>
        /// Handle transport disconnected event.
        /// </summary>
        /// <param name="reason">The disconnection reason.</param>
        private void HandleDisconnected(string reason)
        {
            if (!clientInitiatedDisconnect && options != null && options.reconnection)
            {
                SetState(ConnectionState.Reconnecting);
                Logging.Log("[SocketIOClient] Disconnected: " + reason + " -- reconnecting...");
                EmitEvent("disconnect", reason);
            }
            else
            {
                SetState(ConnectionState.Disconnected);
                Logging.Log("[SocketIOClient] Disconnected: " + reason);
                EmitEvent("disconnect", reason);
            }
        }

        /// <summary>
        /// Handle transport reconnection attempt event.
        /// </summary>
        /// <param name="attemptNumber">The attempt number.</param>
        private void HandleReconnectAttempt(int attemptNumber)
        {
            EmitEvent("reconnect_attempt", attemptNumber.ToString());
        }

        /// <summary>
        /// Handle transport successful reconnection event.
        /// </summary>
        /// <param name="attemptNumber">The attempt count that succeeded.</param>
        private void HandleReconnected(int attemptNumber)
        {
            SetState(ConnectionState.Connected);
            ReplayQueue();
            Logging.Log("[SocketIOClient] Reconnected after " + attemptNumber + " attempts");
            EmitEvent("reconnect", attemptNumber.ToString());
        }

        /// <summary>
        /// Replay all queued messages via transport in FIFO order.
        /// Called after reconnection succeeds, before notifying listeners.
        /// </summary>
        private void ReplayQueue()
        {
            if (messageQueue == null || messageQueue.Count == 0) return;

            var messages = messageQueue.DequeueAll();
            Logging.Log("[SocketIOClient] Replaying " + messages.Length + " queued messages");

            for (int i = 0; i < messages.Length; i++)
            {
                transport.Emit(messages[i].EventName, messages[i].Data);
            }
        }

        /// <summary>
        /// Handle transport reconnection failure (all attempts exhausted).
        /// </summary>
        private void HandleReconnectFailed()
        {
            SetState(ConnectionState.Disconnected);
            Logging.Log("[SocketIOClient] Reconnection failed after all attempts");
            EmitEvent("reconnect_failed", "all reconnection attempts exhausted");
        }

        /// <summary>
        /// Handle transport connection error event.
        /// </summary>
        /// <param name="message">The error message.</param>
        private void HandleConnectError(string message)
        {
            SetState(ConnectionState.Disconnected);
            Logging.LogError("[SocketIOClient] Connection failed: " + message);
            EmitEvent("connect_error", message);
        }

        /// <summary>
        /// Handle transport incoming user event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">The event data.</param>
        private void HandleEvent(string eventName, string data)
        {
            EmitEvent(eventName, data);
        }

        /// <summary>
        /// Handle transport incoming binary event.
        /// Dispatches to all registered binary callbacks for the event.
        /// Silently drops if no callbacks are registered.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">The binary data.</param>
        private void HandleBinaryEvent(string eventName, byte[] data)
        {
            if (!binaryEventCallbacks.ContainsKey(eventName))
            {
                return;
            }

            var callbacks = binaryEventCallbacks[eventName];
            for (int i = 0; i < callbacks.Count; i++)
            {
                try
                {
                    callbacks[i]?.Invoke(eventName, data);
                }
                catch (Exception ex)
                {
                    Logging.LogError("[SocketIOClient] Binary callback error for '"
                        + eventName + "': " + ex.Message);
                }
            }
        }
    }
}
