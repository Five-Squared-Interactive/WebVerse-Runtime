// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FiveSQD.WebVerse.Utilities;

[assembly: InternalsVisibleTo("FiveSQD.WebVerse.WebInterface.SocketIO.Tests")]

namespace FiveSQD.WebVerse.WebInterface.SocketIO
{
    /// <summary>
    /// A namespace-scoped socket that shares the parent SocketIOClient's connection.
    /// Created via SocketIOClient.Of(namespace). Has independent event callbacks
    /// but delegates transport operations to a namespace-scoped ISocketIOTransport.
    /// </summary>
    public class NamespacedSocket
    {
        /// <summary>
        /// The namespace-scoped transport.
        /// </summary>
        private ISocketIOTransport transport;

        /// <summary>
        /// The parent client that owns this namespace socket.
        /// </summary>
        private SocketIOClient parentClient;

        /// <summary>
        /// The namespace path (e.g., "/chat").
        /// </summary>
        private string namespacePath;

        /// <summary>
        /// Whether this namespace socket has been disposed.
        /// </summary>
        private bool terminated;

        /// <summary>
        /// Callbacks for named events.
        /// </summary>
        private Dictionary<string, List<Action<string>>> eventCallbacks
            = new Dictionary<string, List<Action<string>>>();

        /// <summary>
        /// Callbacks for binary events.
        /// </summary>
        private Dictionary<string, List<Action<string, byte[]>>> binaryEventCallbacks
            = new Dictionary<string, List<Action<string, byte[]>>>();

        /// <summary>
        /// One-time callbacks for named events.
        /// </summary>
        private Dictionary<string, List<Action<string>>> onceCallbacks
            = new Dictionary<string, List<Action<string>>>();

        /// <summary>
        /// Catch-all callbacks invoked for every event.
        /// </summary>
        private List<Action<string, string>> onAnyCallbacks
            = new List<Action<string, string>>();

        /// <summary>
        /// Whether the parent connection is active.
        /// </summary>
        public bool Connected
        {
            get { return parentClient != null && parentClient.Connected; }
        }

        /// <summary>
        /// The server-assigned socket ID for this namespace.
        /// </summary>
        public string Id
        {
            get { return transport?.SocketId; }
        }

        /// <summary>
        /// The namespace path.
        /// </summary>
        public string Namespace
        {
            get { return namespacePath; }
        }

        /// <summary>
        /// Create a namespace-scoped socket.
        /// </summary>
        /// <param name="nsTransport">The namespace-scoped transport.</param>
        /// <param name="parent">The parent SocketIOClient.</param>
        /// <param name="nsp">The namespace path.</param>
        internal NamespacedSocket(ISocketIOTransport nsTransport, SocketIOClient parent, string nsp)
        {
            transport = nsTransport;
            parentClient = parent;
            namespacePath = nsp;
            terminated = false;

            WireTransportEvents();
        }

        /// <summary>
        /// Emit a named event with JSON data, scoped to this namespace.
        /// </summary>
        public void Emit(string eventName, string data)
        {
            if (terminated)
            {
                return;
            }

            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->Emit] Transport not initialized.");
                return;
            }

            if (!Connected)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->Emit] Not connected.");
                return;
            }

            transport.Emit(eventName, data);
        }

        /// <summary>
        /// Register a callback for a named event on this namespace.
        /// </summary>
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
        /// Remove all callbacks for a named event on this namespace.
        /// </summary>
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
        /// Register a one-time callback for a named event on this namespace.
        /// </summary>
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
        /// Register a catch-all callback for this namespace.
        /// </summary>
        public void OnAny(Action<string, string> callback)
        {
            if (callback == null)
            {
                return;
            }

            onAnyCallbacks.Add(callback);
        }

        /// <summary>
        /// Remove all catch-all callbacks for this namespace.
        /// </summary>
        public void OffAny()
        {
            onAnyCallbacks.Clear();
        }

        /// <summary>
        /// Emit a binary event scoped to this namespace.
        /// </summary>
        public void EmitBinary(string eventName, byte[] data)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (terminated)
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->EmitBinary] Transport not initialized.");
                return;
            }

            if (!Connected)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->EmitBinary] Not connected.");
                return;
            }

            transport.EmitBinary(eventName, data);
        }

        /// <summary>
        /// Emit with acknowledgement scoped to this namespace.
        /// </summary>
        public void EmitWithAck(string eventName, string data, Action<string> ackCallback)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (terminated)
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->EmitWithAck] Transport not initialized.");
                return;
            }

            if (!Connected)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->EmitWithAck] Not connected.");
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
        /// Emit a volatile event scoped to this namespace.
        /// Silently drops if not connected.
        /// </summary>
        public void EmitVolatile(string eventName, string data)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (terminated)
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->EmitVolatile] Transport not initialized.");
                return;
            }

            if (!Connected)
            {
                return; // SILENT drop -- volatile semantics
            }

            transport.Emit(eventName, data);
        }

        /// <summary>
        /// Request the server to join a named room on this namespace.
        /// </summary>
        public void JoinRoom(string room)
        {
            if (string.IsNullOrEmpty(room))
            {
                return;
            }

            if (terminated)
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->JoinRoom] Transport not initialized.");
                return;
            }

            if (!Connected)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->JoinRoom] Not connected.");
                return;
            }

            transport.JoinRoom(room);
        }

        /// <summary>
        /// Request the server to leave a named room on this namespace.
        /// </summary>
        public void LeaveRoom(string room)
        {
            if (string.IsNullOrEmpty(room))
            {
                return;
            }

            if (terminated)
            {
                return;
            }

            if (transport == null)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->LeaveRoom] Transport not initialized.");
                return;
            }

            if (!Connected)
            {
                Logging.LogWarning("[NamespacedSocket->" + namespacePath + "->LeaveRoom] Not connected.");
                return;
            }

            transport.LeaveRoom(room);
        }

        /// <summary>
        /// Register a callback for a named binary event on this namespace.
        /// </summary>
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
        /// Remove all binary callbacks for a named event on this namespace.
        /// </summary>
        public void OffBinary(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            binaryEventCallbacks.Remove(eventName);
        }

        /// <summary>
        /// Dispose this namespace socket. Called by parent SocketIOClient.Terminate().
        /// </summary>
        internal void Dispose()
        {
            terminated = true;
            UnwireTransportEvents();
            eventCallbacks?.Clear();
            binaryEventCallbacks?.Clear();
            onceCallbacks?.Clear();
            onAnyCallbacks?.Clear();
        }

        /// <summary>
        /// Dispatch an event to all registered callbacks on this namespace.
        /// </summary>
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
                        Logging.LogError("[NamespacedSocket->" + namespacePath
                            + "] Callback error for event '" + eventName + "': " + ex.Message);
                    }
                }
            }

            // 2. Once callbacks -- remove BEFORE invoking
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
                        Logging.LogError("[NamespacedSocket->" + namespacePath
                            + "] Once callback error for '" + eventName + "': " + ex.Message);
                    }
                }
            }

            // 3. OnAny callbacks
            for (int i = 0; i < onAnyCallbacks.Count; i++)
            {
                try
                {
                    onAnyCallbacks[i]?.Invoke(eventName, data);
                }
                catch (Exception ex)
                {
                    Logging.LogError("[NamespacedSocket->" + namespacePath
                        + "] OnAny callback error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Handle incoming event from namespace transport.
        /// </summary>
        private void HandleEvent(string eventName, string data)
        {
            EmitEvent(eventName, data);
        }

        /// <summary>
        /// Handle incoming binary event from namespace transport.
        /// </summary>
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
                    Logging.LogError("[NamespacedSocket->" + namespacePath
                        + "] Binary callback error for '" + eventName + "': " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Subscribe to namespace transport events (OnEvent and OnBinaryEvent only).
        /// </summary>
        private void WireTransportEvents()
        {
            if (transport == null) return;

            transport.OnEvent += HandleEvent;
            transport.OnBinaryEvent += HandleBinaryEvent;
        }

        /// <summary>
        /// Unsubscribe from namespace transport events.
        /// </summary>
        private void UnwireTransportEvents()
        {
            if (transport == null) return;

            transport.OnEvent -= HandleEvent;
            transport.OnBinaryEvent -= HandleBinaryEvent;
        }
    }
}
