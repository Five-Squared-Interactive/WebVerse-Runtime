// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if USE_BESTHTTP
using System;
using System.Collections.Generic;
using Best.SocketIO;
using Best.SocketIO.Events;
using Best.SocketIO.Transports;
using Best.HTTP.Shared.PlatformSupport.Memory;
using PlatformSupport.Collections.ObjectModel;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.WebInterface.SocketIO
{
    /// <summary>
    /// Transport adapter wrapping the Best Socket.IO library behind ISocketIOTransport.
    /// Entire file is conditionally compiled with USE_BESTHTTP.
    /// </summary>
    public class BestSocketIOTransport : ISocketIOTransport
    {
        /// <summary>
        /// The Best Socket.IO manager.
        /// </summary>
        private SocketManager manager;

        /// <summary>
        /// The root namespace socket.
        /// </summary>
        private Socket rootSocket;

        /// <summary>
        /// Reconnection attempt counter for event dispatch.
        /// </summary>
        private int reconnectAttemptCount;

        /// <summary>
        /// The namespace path this transport is bound to.
        /// </summary>
        private string namespacePath = "/";

        /// <summary>
        /// Whether Disconnect() was called by the client (for disconnect reason tracking).
        /// </summary>
        private bool clientInitiatedDisconnect;

        /// <summary>
        /// Whether this is the root transport (owns the manager and OnIncomingPacket handler).
        /// </summary>
        private bool isRootTransport;

        /// <summary>
        /// Namespace transports created via Of(). Only populated on root transport.
        /// </summary>
        private Dictionary<string, BestSocketIOTransport> namespaceTransports;

        public bool IsConnected
        {
            get { return manager != null && manager.State == States.Open; }
        }

        public string SocketId
        {
            get { return rootSocket?.Id; }
        }

        public event Action<string> OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnConnectError;
        public event Action<int> OnReconnectAttempt;
        public event Action<int> OnReconnected;
        public event Action OnReconnectFailed;
        public event Action<string, string> OnEvent;
        public event Action<string, byte[]> OnBinaryEvent;

        public void Connect(string url, SocketIOOptions options)
        {
            try
            {
                var socketOptions = MapOptions(options);
                manager = new SocketManager(new Uri(url), socketOptions);
                rootSocket = manager.Socket;
                reconnectAttemptCount = 0;
                clientInitiatedDisconnect = false;
                isRootTransport = true;
                namespacePath = "/";
                namespaceTransports = new Dictionary<string, BestSocketIOTransport>();

                WireSocketEvents();
                WireIncomingPacketHandler();

                manager.Open();
            }
            catch (Exception ex)
            {
                Logging.LogError("[BestSocketIOTransport->Connect] " + ex.Message);
                OnConnectError?.Invoke(ex.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                clientInitiatedDisconnect = true;

                if (manager != null)
                {
                    // Clear incoming packet handler before closing
                    manager.OnIncomingPacket = null;
                    manager.Close();
                }

                // Clean up references
                namespaceTransports?.Clear();
                rootSocket = null;
                manager = null;
            }
            catch (Exception ex)
            {
                Logging.LogError("[BestSocketIOTransport->Disconnect] " + ex.Message);
            }
        }

        public void Emit(string eventName, string data)
        {
            try
            {
                rootSocket?.Emit(eventName, data);
            }
            catch (Exception ex)
            {
                Logging.LogError("[BestSocketIOTransport->Emit] " + ex.Message);
            }
        }

        public void EmitBinary(string eventName, byte[] data)
        {
            try
            {
                rootSocket?.Emit(eventName, data);
            }
            catch (Exception ex)
            {
                Logging.LogError("[BestSocketIOTransport->EmitBinary] " + ex.Message);
            }
        }

        public void EmitWithAck(string eventName, string data, Action<string> ackCallback)
        {
            try
            {
                rootSocket?.ExpectAcknowledgement<string>((response) =>
                {
                    try
                    {
                        ackCallback?.Invoke(response);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[BestSocketIOTransport] Ack callback error: " + ex.Message);
                    }
                }).Emit(eventName, data);
            }
            catch (Exception ex)
            {
                Logging.LogError("[BestSocketIOTransport->EmitWithAck] " + ex.Message);
            }
        }

        public void JoinRoom(string room)
        {
            try
            {
                rootSocket?.Emit("join", room);
            }
            catch (Exception ex)
            {
                Logging.LogError("[BestSocketIOTransport->JoinRoom] " + ex.Message);
            }
        }

        public void LeaveRoom(string room)
        {
            try
            {
                rootSocket?.Emit("leave", room);
            }
            catch (Exception ex)
            {
                Logging.LogError("[BestSocketIOTransport->LeaveRoom] " + ex.Message);
            }
        }

        public ISocketIOTransport Of(string nsp)
        {
            try
            {
                if (manager == null)
                {
                    Logging.LogWarning("[BestSocketIOTransport->Of] Manager not initialized.");
                    return null;
                }

                var nsSocket = manager.GetSocket(nsp);
                var nsTransport = new BestSocketIOTransport();
                nsTransport.manager = manager;
                nsTransport.rootSocket = nsSocket;
                nsTransport.namespacePath = nsp;
                nsTransport.isRootTransport = false;
                nsTransport.WireSocketEvents();

                // Register with root so OnIncomingPacket routes to it
                if (namespaceTransports != null)
                {
                    namespaceTransports[nsp] = nsTransport;
                }

                return nsTransport;
            }
            catch (Exception ex)
            {
                Logging.LogError("[BestSocketIOTransport->Of] " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Map our SocketIOOptions to Best Socket.IO SocketOptions.
        /// </summary>
        private SocketOptions MapOptions(SocketIOOptions options)
        {
            var opts = new SocketOptions();
            opts.AutoConnect = false;
            opts.Reconnection = options.reconnection;
            opts.ReconnectionAttempts = options.reconnectionAttempts;
            opts.ReconnectionDelay = TimeSpan.FromMilliseconds(options.reconnectionDelay);
            opts.ReconnectionDelayMax = TimeSpan.FromMilliseconds(options.reconnectionDelayMax);
            opts.Timeout = TimeSpan.FromMilliseconds(options.timeout);

            if (string.Equals(options.transport, "websocket", StringComparison.OrdinalIgnoreCase))
            {
                opts.ConnectWith = TransportTypes.WebSocket;
            }

            if (!string.IsNullOrEmpty(options.auth))
            {
                string authToken = options.auth;
                opts.Auth = (manager, socket) => new { token = authToken };
            }

            if (options.query != null)
            {
                opts.AdditionalQueryParams = new ObservableDictionary<string, string>();
                foreach (var kvp in options.query)
                {
                    opts.AdditionalQueryParams.Add(kvp.Key, kvp.Value);
                }
            }

            if (options.headers != null)
            {
                opts.HTTPRequestCustomizationCallback = (manager, request) =>
                {
                    foreach (var kvp in options.headers)
                    {
                        request.SetHeader(kvp.Key, kvp.Value);
                    }
                };
            }

            return opts;
        }

        /// <summary>
        /// Wire socket-level events (connect, disconnect, error, reconnect).
        /// Called for both root and namespace transports.
        /// </summary>
        private void WireSocketEvents()
        {
            if (rootSocket == null) return;

            // Connect event
            rootSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, (response) =>
            {
                try
                {
                    OnConnected?.Invoke(response.sid);
                }
                catch (Exception ex)
                {
                    Logging.LogError("[BestSocketIOTransport] OnConnected handler error: " + ex.Message);
                }
            });

            // Disconnect event
            rootSocket.On(SocketIOEventTypes.Disconnect, () =>
            {
                try
                {
                    string reason = clientInitiatedDisconnect
                        ? "io client disconnect"
                        : "transport close";
                    OnDisconnected?.Invoke(reason);
                }
                catch (Exception ex)
                {
                    Logging.LogError("[BestSocketIOTransport] OnDisconnected handler error: " + ex.Message);
                }
            });

            // Error event
            rootSocket.On<Error>(SocketIOEventTypes.Error, (error) =>
            {
                try
                {
                    OnConnectError?.Invoke(error?.message ?? "Unknown error");
                }
                catch (Exception ex)
                {
                    Logging.LogError("[BestSocketIOTransport] OnConnectError handler error: " + ex.Message);
                }
            });

            // Reconnect events — only on root transport (manager emits these on root socket only)
            if (isRootTransport)
            {
                rootSocket.On("reconnect_attempt", () =>
                {
                    try
                    {
                        reconnectAttemptCount++;
                        OnReconnectAttempt?.Invoke(reconnectAttemptCount);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[BestSocketIOTransport] OnReconnectAttempt handler error: " + ex.Message);
                    }
                });

                rootSocket.On("reconnect", () =>
                {
                    try
                    {
                        OnReconnected?.Invoke(reconnectAttemptCount);
                        reconnectAttemptCount = 0;
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[BestSocketIOTransport] OnReconnected handler error: " + ex.Message);
                    }
                });

                rootSocket.On("reconnect_failed", () =>
                {
                    try
                    {
                        OnReconnectFailed?.Invoke();
                        reconnectAttemptCount = 0;
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[BestSocketIOTransport] OnReconnectFailed handler error: " + ex.Message);
                    }
                });
            }
        }

        /// <summary>
        /// Set up the central OnIncomingPacket handler on the manager.
        /// Only called by the root transport. Routes packets by namespace.
        /// </summary>
        private void WireIncomingPacketHandler()
        {
            if (manager == null) return;

            manager.OnIncomingPacket = (mgr, packet) =>
            {
                try
                {
                    if (packet.SocketIOEvent != SocketIOEventTypes.Event &&
                        packet.SocketIOEvent != SocketIOEventTypes.BinaryEvent)
                    {
                        return;
                    }

                    string packetNsp = packet.Namespace ?? "/";

                    // Route to the correct transport by namespace
                    if (packetNsp == namespacePath)
                    {
                        DispatchPacket(packet);
                    }
                    else if (namespaceTransports != null &&
                             namespaceTransports.TryGetValue(packetNsp, out var nsTransport))
                    {
                        nsTransport.DispatchPacket(packet);
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError("[BestSocketIOTransport] Incoming packet routing error: " + ex.Message);
                }
            };
        }

        /// <summary>
        /// Dispatch an incoming packet as a user event or binary event.
        /// </summary>
        private void DispatchPacket(IncomingPacket packet)
        {
            try
            {
                if (packet.SocketIOEvent == SocketIOEventTypes.Event)
                {
                    string eventName = packet.EventName;
                    string data = packet.DecodedArg?.ToString() ?? packet.Payload;
                    OnEvent?.Invoke(eventName, data);
                }
                else if (packet.SocketIOEvent == SocketIOEventTypes.BinaryEvent)
                {
                    string eventName = packet.EventName;
                    byte[] binary = null;

                    if (packet.Attachements != null && packet.Attachements.Count > 0)
                    {
                        var segment = packet.Attachements[0];
                        binary = new byte[segment.Count];
                        segment.CopyTo(binary);
                    }
                    else
                    {
                        binary = Array.Empty<byte>();
                    }

                    OnBinaryEvent?.Invoke(eventName, binary);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError("[BestSocketIOTransport] Packet dispatch error: " + ex.Message);
            }
        }
    }
}
#endif
