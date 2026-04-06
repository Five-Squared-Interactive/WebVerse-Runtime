// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.WebInterface.SocketIO
{
    /// <summary>
    /// Domain-abstracted transport interface for Socket.IO communication.
    /// Implementations wrap a concrete transport library (e.g., Best Socket.IO)
    /// behind this interface to enable TDD and conditional compilation.
    /// </summary>
    public interface ISocketIOTransport
    {
        /// <summary>
        /// Connect to a Socket.IO server.
        /// </summary>
        /// <param name="url">The server URL.</param>
        /// <param name="options">Connection options.</param>
        void Connect(string url, SocketIOOptions options);

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Emit a named event with JSON string data.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">JSON data string.</param>
        void Emit(string eventName, string data);

        /// <summary>
        /// Emit a named event with binary data.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">Binary data.</param>
        void EmitBinary(string eventName, byte[] data);

        /// <summary>
        /// Emit a named event and wait for server acknowledgement.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">JSON data string.</param>
        /// <param name="ackCallback">Callback invoked with the server's acknowledgement response.</param>
        void EmitWithAck(string eventName, string data, Action<string> ackCallback);

        /// <summary>
        /// Request the server to join a named room.
        /// </summary>
        /// <param name="room">The room name.</param>
        void JoinRoom(string room);

        /// <summary>
        /// Request the server to leave a named room.
        /// </summary>
        /// <param name="room">The room name.</param>
        void LeaveRoom(string room);

        /// <summary>
        /// Get a transport scoped to the specified namespace.
        /// </summary>
        /// <param name="nsp">The namespace path (e.g., "/chat").</param>
        /// <returns>A transport instance scoped to the namespace.</returns>
        ISocketIOTransport Of(string nsp);

        /// <summary>
        /// Whether the transport is currently connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// The server-assigned socket ID.
        /// </summary>
        string SocketId { get; }

        /// <summary>
        /// Fired when the transport successfully connects. Parameter is the socket ID.
        /// </summary>
        event Action<string> OnConnected;

        /// <summary>
        /// Fired when the transport disconnects. Parameter is the reason string.
        /// </summary>
        event Action<string> OnDisconnected;

        /// <summary>
        /// Fired when a connection error occurs. Parameter is the error message.
        /// </summary>
        event Action<string> OnConnectError;

        /// <summary>
        /// Fired when a reconnection attempt starts. Parameter is the attempt number.
        /// </summary>
        event Action<int> OnReconnectAttempt;

        /// <summary>
        /// Fired when reconnection succeeds. Parameter is the attempt number.
        /// </summary>
        event Action<int> OnReconnected;

        /// <summary>
        /// Fired when all reconnection attempts have failed.
        /// </summary>
        event Action OnReconnectFailed;

        /// <summary>
        /// Fired when a named event is received. Parameters are event name and JSON data.
        /// </summary>
        event Action<string, string> OnEvent;

        /// <summary>
        /// Fired when a binary event is received. Parameters are event name and binary data.
        /// </summary>
        event Action<string, byte[]> OnBinaryEvent;
    }
}
