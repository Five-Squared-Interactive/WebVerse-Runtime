// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.WebInterface.SocketIO.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of ISocketIOTransport for unit testing.
    /// Records all method calls and allows simulation of transport events.
    /// </summary>
    public class MockSocketIOTransport : ISocketIOTransport
    {
        // --- Call Recording ---

        /// <summary>
        /// Records of Connect calls: (url, options).
        /// </summary>
        public List<(string Url, SocketIOOptions Options)> ConnectCalls { get; } = new List<(string, SocketIOOptions)>();

        /// <summary>
        /// Number of Disconnect calls.
        /// </summary>
        public int DisconnectCallCount { get; private set; }

        /// <summary>
        /// Records of Emit calls: (eventName, data).
        /// </summary>
        public List<(string EventName, string Data)> EmitCalls { get; } = new List<(string, string)>();

        /// <summary>
        /// Records of EmitBinary calls: (eventName, data).
        /// </summary>
        public List<(string EventName, byte[] Data)> EmitBinaryCalls { get; } = new List<(string, byte[])>();

        /// <summary>
        /// Records of EmitWithAck calls: (eventName, data, callback).
        /// </summary>
        public List<(string EventName, string Data, Action<string> Callback)> EmitWithAckCalls { get; }
            = new List<(string, string, Action<string>)>();

        /// <summary>
        /// Records of JoinRoom calls.
        /// </summary>
        public List<string> JoinRoomCalls { get; } = new List<string>();

        /// <summary>
        /// Records of LeaveRoom calls.
        /// </summary>
        public List<string> LeaveRoomCalls { get; } = new List<string>();

        /// <summary>
        /// Records of Of calls.
        /// </summary>
        public List<string> OfCalls { get; } = new List<string>();

        /// <summary>
        /// Cached namespace transport mocks, keyed by namespace path.
        /// Allows tests to retrieve the namespace mock for event simulation and call verification.
        /// </summary>
        public Dictionary<string, MockSocketIOTransport> NamespaceTransports { get; }
            = new Dictionary<string, MockSocketIOTransport>();

        // --- Controllable Properties ---

        /// <summary>
        /// Settable for test control.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Settable for test control.
        /// </summary>
        public string SocketId { get; set; }

        /// <summary>
        /// When true, Of() returns null instead of a mock transport.
        /// </summary>
        public bool OfReturnsNull { get; set; }

        // --- Events ---

        public event Action<string> OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnConnectError;
        public event Action<int> OnReconnectAttempt;
        public event Action<int> OnReconnected;
        public event Action OnReconnectFailed;
        public event Action<string, string> OnEvent;
        public event Action<string, byte[]> OnBinaryEvent;

        // --- Interface Methods ---

        public void Connect(string url, SocketIOOptions options)
        {
            ConnectCalls.Add((url, options));
        }

        public void Disconnect()
        {
            DisconnectCallCount++;
        }

        public void Emit(string eventName, string data)
        {
            EmitCalls.Add((eventName, data));
        }

        public void EmitBinary(string eventName, byte[] data)
        {
            EmitBinaryCalls.Add((eventName, data));
        }

        public void EmitWithAck(string eventName, string data, Action<string> ackCallback)
        {
            EmitWithAckCalls.Add((eventName, data, ackCallback));
        }

        public void JoinRoom(string room)
        {
            JoinRoomCalls.Add(room);
        }

        public void LeaveRoom(string room)
        {
            LeaveRoomCalls.Add(room);
        }

        public ISocketIOTransport Of(string nsp)
        {
            OfCalls.Add(nsp);
            if (OfReturnsNull) return null;
            if (!NamespaceTransports.ContainsKey(nsp))
            {
                NamespaceTransports[nsp] = new MockSocketIOTransport();
            }
            return NamespaceTransports[nsp];
        }

        // --- Simulation Methods ---

        /// <summary>
        /// Simulate a successful connection event.
        /// </summary>
        /// <param name="socketId">The server-assigned socket ID.</param>
        public void SimulateConnected(string socketId)
        {
            SocketId = socketId;
            IsConnected = true;
            OnConnected?.Invoke(socketId);
        }

        /// <summary>
        /// Simulate a disconnection event.
        /// </summary>
        /// <param name="reason">The disconnection reason.</param>
        public void SimulateDisconnected(string reason)
        {
            IsConnected = false;
            SocketId = null;
            OnDisconnected?.Invoke(reason);
        }

        /// <summary>
        /// Simulate a connection error event.
        /// </summary>
        /// <param name="message">The error message.</param>
        public void SimulateConnectError(string message)
        {
            IsConnected = false;
            OnConnectError?.Invoke(message);
        }

        /// <summary>
        /// Simulate an incoming named event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">The JSON data.</param>
        public void SimulateEvent(string eventName, string data)
        {
            OnEvent?.Invoke(eventName, data);
        }

        /// <summary>
        /// Simulate an incoming binary event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">The binary data.</param>
        public void SimulateBinaryEvent(string eventName, byte[] data)
        {
            OnBinaryEvent?.Invoke(eventName, data);
        }

        /// <summary>
        /// Simulate a reconnection attempt event.
        /// </summary>
        /// <param name="attemptNumber">The attempt number.</param>
        public void SimulateReconnectAttempt(int attemptNumber)
        {
            OnReconnectAttempt?.Invoke(attemptNumber);
        }

        /// <summary>
        /// Simulate a successful reconnection event.
        /// </summary>
        /// <param name="attemptNumber">The attempt number that succeeded.</param>
        public void SimulateReconnected(int attemptNumber)
        {
            IsConnected = true;
            OnReconnected?.Invoke(attemptNumber);
        }

        /// <summary>
        /// Simulate reconnection failure (all attempts exhausted).
        /// </summary>
        public void SimulateReconnectFailed()
        {
            IsConnected = false;
            OnReconnectFailed?.Invoke();
        }

        /// <summary>
        /// Reset all recorded calls and state for test reuse.
        /// </summary>
        public void Reset()
        {
            ConnectCalls.Clear();
            DisconnectCallCount = 0;
            EmitCalls.Clear();
            EmitBinaryCalls.Clear();
            EmitWithAckCalls.Clear();
            JoinRoomCalls.Clear();
            LeaveRoomCalls.Clear();
            OfCalls.Clear();
            NamespaceTransports.Clear();
            IsConnected = false;
            SocketId = null;
            OfReturnsNull = false;
        }
    }
}
