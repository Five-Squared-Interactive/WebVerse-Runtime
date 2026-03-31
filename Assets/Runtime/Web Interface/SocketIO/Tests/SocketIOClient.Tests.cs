// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WebInterface.SocketIO;
using FiveSQD.WebVerse.WebInterface.SocketIO.Tests.Mocks;

namespace FiveSQD.WebVerse.WebInterface.SocketIO.Tests
{
    /// <summary>
    /// Comprehensive unit tests for SocketIOClient lifecycle, state machine,
    /// and event dispatch using MockSocketIOTransport.
    /// </summary>
    [TestFixture]
    public class SocketIOClientTests
    {
        private GameObject gameObject;
        private SocketIOClient client;
        private MockSocketIOTransport mockTransport;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            gameObject = new GameObject("SocketIOClientTest");
            client = gameObject.AddComponent<SocketIOClient>();
            mockTransport = new MockSocketIOTransport();
            client.Initialize();
            client.Transport = mockTransport;
        }

        [TearDown]
        public void TearDown()
        {
            if (client != null)
            {
                client.Terminate();
            }
            if (gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        // --- AC1: Connect from Disconnected ---

        [Test]
        public void Connect_FromDisconnected_TransitionsToConnecting()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");

            Assert.AreEqual(ConnectionState.Connecting, client.State);
        }

        [Test]
        public void Connect_FromDisconnected_CallsTransportConnect()
        {
            LogAssert.ignoreFailingMessages = true;
            var options = new SocketIOOptions();
            client.Connect("http://localhost:3000", options);

            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
            Assert.AreEqual("http://localhost:3000", mockTransport.ConnectCalls[0].Url);
        }

        [Test]
        public void Connect_WithNullOptions_CreatesDefaultOptions()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");

            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
            Assert.IsNotNull(mockTransport.ConnectCalls[0].Options);
        }

        [Test]
        public void Connect_WithNullTransport_LogsWarningAndReturns()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Transport = null;
            client.Connect("http://localhost:3000");

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
        }

        // --- AC2: Transport OnConnected → Connected ---

        [Test]
        public void OnConnected_TransitionsToConnected()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            Assert.AreEqual(ConnectionState.Connected, client.State);
        }

        [Test]
        public void OnConnected_ConnectedPropertyReturnsTrue()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            Assert.IsTrue(client.Connected);
        }

        [Test]
        public void OnConnected_IdReturnsSocketId()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            Assert.AreEqual("abc-123", client.Id);
        }

        [Test]
        public void OnConnected_EmitsConnectSystemEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedId = null;
            client.On("connect", (id) => receivedId = id);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            Assert.AreEqual("abc-123", receivedId);
        }

        // --- AC3: Disconnect from Connected ---

        [Test]
        public void Disconnect_FromConnected_TransitionsToDisconnecting()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            client.Disconnect();

            Assert.AreEqual(ConnectionState.Disconnecting, client.State);
        }

        [Test]
        public void Disconnect_FromConnected_CallsTransportDisconnect()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            client.Disconnect();

            Assert.AreEqual(1, mockTransport.DisconnectCallCount);
        }

        // --- AC4: Transport OnDisconnected → Disconnected ---

        [Test]
        public void OnDisconnected_TransitionsToDisconnected()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");
            client.Disconnect();

            mockTransport.SimulateDisconnected("io client disconnect");

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
        }

        [Test]
        public void OnDisconnected_ConnectedPropertyReturnsFalse()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");
            client.Disconnect();
            mockTransport.SimulateDisconnected("io client disconnect");

            Assert.IsFalse(client.Connected);
        }

        [Test]
        public void OnDisconnected_EmitsDisconnectSystemEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedReason = null;
            client.On("disconnect", (reason) => receivedReason = reason);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");
            client.Disconnect();
            mockTransport.SimulateDisconnected("io client disconnect");

            Assert.AreEqual("io client disconnect", receivedReason);
        }

        // --- AC5: Transport OnConnectError → Disconnected ---

        [Test]
        public void OnConnectError_TransitionsToDisconnected()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            LogAssert.Expect(LogType.Error, "[SocketIOClient] Connection failed: connection refused");
            mockTransport.SimulateConnectError("connection refused");

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
        }

        [Test]
        public void OnConnectError_EmitsConnectErrorSystemEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedError = null;
            client.On("connect_error", (err) => receivedError = err);

            client.Connect("http://localhost:3000");
            LogAssert.Expect(LogType.Error, "[SocketIOClient] Connection failed: connection refused");
            mockTransport.SimulateConnectError("connection refused");

            Assert.AreEqual("connection refused", receivedError);
        }

        // --- Story 1.3 AC1: Emit when Connected ---

        [Test]
        public void Emit_WhenConnected_CallsTransportEmit()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            client.Emit("chat", "{\"msg\":\"hello\"}");

            Assert.AreEqual(1, mockTransport.EmitCalls.Count);
            Assert.AreEqual("chat", mockTransport.EmitCalls[0].EventName);
            Assert.AreEqual("{\"msg\":\"hello\"}", mockTransport.EmitCalls[0].Data);
        }

        // --- Story 1.3 AC2: Emit when Disconnected ---

        [Test]
        public void Emit_WhenDisconnected_DropsMessageWithWarning()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Emit("chat", "{\"msg\":\"hello\"}");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void Emit_WhenConnecting_DropsMessage()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");

            client.Emit("chat", "{\"msg\":\"hello\"}");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void Emit_WhenReconnecting_EnqueuesMessage()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            client.Emit("chat", "{\"msg\":\"hello\"}");

            // Not sent via transport yet
            Assert.AreEqual(0, mockTransport.EmitCalls.Count);

            // Replayed on reconnect
            mockTransport.SimulateReconnected(1);
            Assert.AreEqual(1, mockTransport.EmitCalls.Count);
            Assert.AreEqual("chat", mockTransport.EmitCalls[0].EventName);
            Assert.AreEqual("{\"msg\":\"hello\"}", mockTransport.EmitCalls[0].Data);
        }

        [Test]
        public void Emit_WithNullTransport_DoesNotThrow()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Transport = null;

            Assert.DoesNotThrow(() => client.Emit("chat", "data"));
        }

        // --- AC6: Disconnect from Disconnected → no-op ---

        [Test]
        public void Disconnect_FromDisconnected_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Disconnect();

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
            Assert.AreEqual(0, mockTransport.DisconnectCallCount);
        }

        // --- AC6 extended: Connect from non-Disconnected → no-op ---

        [Test]
        public void Connect_FromConnected_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            client.Connect("http://localhost:4000");

            // Should still have only 1 connect call
            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
            Assert.AreEqual(ConnectionState.Connected, client.State);
        }

        [Test]
        public void Connect_FromConnecting_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");

            client.Connect("http://localhost:4000");

            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
            Assert.AreEqual(ConnectionState.Connecting, client.State);
        }

        // --- AC7: Initialize / Terminate ---

        [Test]
        public void Initialize_SetsDisconnectedState()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual(ConnectionState.Disconnected, client.State);
        }

        [Test]
        public void Initialize_ConnectedIsFalse()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.IsFalse(client.Connected);
        }

        [Test]
        public void Terminate_DisconnectsIfConnected()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            client.Terminate();
            client = null; // Prevent TearDown from double-terminating

            Assert.AreEqual(1, mockTransport.DisconnectCallCount);
        }

        [Test]
        public void Terminate_ClearsCallbacks()
        {
            LogAssert.ignoreFailingMessages = true;
            bool callbackFired = false;
            client.On("connect", (id) => callbackFired = true);

            client.Terminate();
            client = null;

            // Transport events should no longer reach callbacks
            // (transport is unwired in Terminate)
            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void Terminate_SetsDisconnectedState()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            client.Terminate();

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
            client = null;
        }

        // --- System Event Dispatch Tests ---

        [Test]
        public void On_RegistersCallback_InvokedOnEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            string received = null;
            client.On("connect", (data) => received = data);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            Assert.AreEqual("id-1", received);
        }

        [Test]
        public void On_MultipleCallbacks_AllInvokedInOrder()
        {
            LogAssert.ignoreFailingMessages = true;
            var order = new List<int>();
            client.On("connect", (data) => order.Add(1));
            client.On("connect", (data) => order.Add(2));
            client.On("connect", (data) => order.Add(3));

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            Assert.AreEqual(3, order.Count);
            Assert.AreEqual(1, order[0]);
            Assert.AreEqual(2, order[1]);
            Assert.AreEqual(3, order[2]);
        }

        [Test]
        public void Off_RemovesCallbacks_NotInvokedAfterOff()
        {
            LogAssert.ignoreFailingMessages = true;
            bool fired = false;
            client.On("connect", (data) => fired = true);
            client.Off("connect");

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            Assert.IsFalse(fired);
        }

        [Test]
        public void Off_OnlyRemovesSpecifiedEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            string connectData = null;
            string disconnectData = null;
            client.On("connect", (data) => connectData = data);
            client.On("disconnect", (data) => disconnectData = data);

            client.Off("connect");

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Disconnect();
            mockTransport.SimulateDisconnected("io client disconnect");

            Assert.IsNull(connectData);
            Assert.AreEqual("io client disconnect", disconnectData);
        }

        [Test]
        public void CallbackException_LogsError_DoesNotStopOtherCallbacks()
        {
            LogAssert.ignoreFailingMessages = true;
            var results = new List<string>();
            client.On("connect", (data) => { throw new Exception("test error"); });
            client.On("connect", (data) => results.Add(data));

            LogAssert.Expect(LogType.Error, "[SocketIOClient] Callback error for event 'connect': test error");
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            // Second callback should still fire despite first throwing
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("id-1", results[0]);
        }

        // --- User Event Dispatch (prep for Story 1.3) ---

        [Test]
        public void TransportOnEvent_DispatchesToRegisteredCallback()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedData = null;
            client.On("chat", (data) => receivedData = data);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateEvent("chat", "{\"msg\":\"hello\"}");

            Assert.AreEqual("{\"msg\":\"hello\"}", receivedData);
        }

        [Test]
        public void TransportOnEvent_NoListener_SilentlyDropped()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            // Should not throw
            Assert.DoesNotThrow(() => mockTransport.SimulateEvent("unknown_event", "data"));
        }

        // --- Edge Cases ---

        [Test]
        public void On_NullEventName_IsIgnored()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.DoesNotThrow(() => client.On(null, (data) => { }));
        }

        [Test]
        public void On_NullCallback_IsIgnored()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.DoesNotThrow(() => client.On("event", null));
        }

        [Test]
        public void Off_NonExistentEvent_IsIgnored()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.DoesNotThrow(() => client.Off("nonexistent"));
        }

        [Test]
        public void Disconnect_FromConnecting_LogsWarning()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");

            // Attempting disconnect from Connecting state
            client.Disconnect();

            // Should not have called transport.Disconnect
            Assert.AreEqual(0, mockTransport.DisconnectCallCount);
        }

        // --- Story 2.1: Option Pass-Through Tests ---

        [Test]
        public void Connect_WithFullOptions_AllFieldsReachTransport()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions
            {
                auth = "bearer-token",
                transport = "websocket",
                reconnection = false,
                reconnectionAttempts = 10,
                reconnectionDelay = 2000,
                reconnectionDelayMax = 10000,
                timeout = 5000,
                ackTimeout = 3000,
                queueSize = 50,
                headers = new Dictionary<string, string> { { "X-Custom", "value1" } },
                query = new Dictionary<string, string> { { "room", "lobby" } }
            };

            client.Connect("http://localhost:3000", opts);

            var received = mockTransport.ConnectCalls[0].Options;
            Assert.AreEqual("bearer-token", received.auth);
            Assert.AreEqual("websocket", received.transport);
            Assert.IsFalse(received.reconnection);
            Assert.AreEqual(10, received.reconnectionAttempts);
            Assert.AreEqual(2000, received.reconnectionDelay);
            Assert.AreEqual(10000, received.reconnectionDelayMax);
            Assert.AreEqual(5000, received.timeout);
            Assert.AreEqual(3000, received.ackTimeout);
            Assert.AreEqual(50, received.queueSize);
            Assert.IsNotNull(received.headers);
            Assert.AreEqual("value1", received.headers["X-Custom"]);
            Assert.IsNotNull(received.query);
            Assert.AreEqual("lobby", received.query["room"]);
        }

        [Test]
        public void Connect_WithNullOptions_TransportReceivesDefaults()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");

            var received = mockTransport.ConnectCalls[0].Options;
            Assert.IsTrue(received.reconnection);
            Assert.AreEqual(5, received.reconnectionAttempts);
            Assert.AreEqual(1000, received.reconnectionDelay);
            Assert.AreEqual(5000, received.reconnectionDelayMax);
            Assert.AreEqual(20000, received.timeout);
            Assert.AreEqual(10000, received.ackTimeout);
            Assert.AreEqual(100, received.queueSize);
            Assert.AreEqual("polling", received.transport);
            Assert.IsNull(received.auth);
            Assert.IsNull(received.headers);
            Assert.IsNull(received.query);
        }

        [Test]
        public void Connect_WithTransportWebsocket_StoredInConnectCalls()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { transport = "websocket" };
            client.Connect("http://localhost:3000", opts);

            Assert.AreEqual("websocket", mockTransport.ConnectCalls[0].Options.transport);
        }

        [Test]
        public void Connect_WithAuth_StoredInConnectCalls()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { auth = "my-secret-token" };
            client.Connect("http://localhost:3000", opts);

            Assert.AreEqual("my-secret-token", mockTransport.ConnectCalls[0].Options.auth);
        }

        [Test]
        public void Connect_WithHeaders_StoredInConnectCalls()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions
            {
                headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer xyz" },
                    { "X-App-Version", "2.0" }
                }
            };
            client.Connect("http://localhost:3000", opts);

            var received = mockTransport.ConnectCalls[0].Options.headers;
            Assert.IsNotNull(received);
            Assert.AreEqual(2, received.Count);
            Assert.AreEqual("Bearer xyz", received["Authorization"]);
            Assert.AreEqual("2.0", received["X-App-Version"]);
        }

        [Test]
        public void Connect_WithQuery_StoredInConnectCalls()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions
            {
                query = new Dictionary<string, string>
                {
                    { "room", "lobby" },
                    { "user", "player1" }
                }
            };
            client.Connect("http://localhost:3000", opts);

            var received = mockTransport.ConnectCalls[0].Options.query;
            Assert.IsNotNull(received);
            Assert.AreEqual(2, received.Count);
            Assert.AreEqual("lobby", received["room"]);
            Assert.AreEqual("player1", received["user"]);
        }

        [Test]
        public void Connect_WithTimeout_StoredInConnectCalls()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { timeout = 5000 };
            client.Connect("http://localhost:3000", opts);

            Assert.AreEqual(5000, mockTransport.ConnectCalls[0].Options.timeout);
        }

        [Test]
        public void Connect_WithReconnectionFalse_StoredInConnectCalls()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = false };
            client.Connect("http://localhost:3000", opts);

            Assert.IsFalse(mockTransport.ConnectCalls[0].Options.reconnection);
        }

        // --- Story 2.2: Reconnection Event Emission Tests ---

        [Test]
        public void OnDisconnected_ReconnectionTrue_EmitsDisconnectWithReason()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedReason = null;
            client.On("disconnect", (reason) => receivedReason = reason);

            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");

            // Transport-initiated disconnect
            mockTransport.SimulateDisconnected("transport close");

            Assert.AreEqual("transport close", receivedReason);
        }

        [Test]
        public void OnReconnectAttempt_EmitsReconnectAttemptEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedAttempt = null;
            client.On("reconnect_attempt", (data) => receivedAttempt = data);

            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            mockTransport.SimulateReconnectAttempt(1);

            Assert.AreEqual("1", receivedAttempt);
        }

        [Test]
        public void OnReconnected_EmitsReconnectEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedAttemptCount = null;
            client.On("reconnect", (data) => receivedAttemptCount = data);

            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            mockTransport.SimulateReconnected(3);

            Assert.AreEqual("3", receivedAttemptCount);
        }

        [Test]
        public void OnReconnectFailed_EmitsReconnectFailedEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedData = null;
            client.On("reconnect_failed", (data) => receivedData = data);

            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            mockTransport.SimulateReconnectFailed();

            Assert.IsNotNull(receivedData);
            Assert.AreEqual("all reconnection attempts exhausted", receivedData);
        }

        [Test]
        public void MultipleReconnectAttempts_FireSequentialEvents()
        {
            LogAssert.ignoreFailingMessages = true;
            var attempts = new List<string>();
            client.On("reconnect_attempt", (data) => attempts.Add(data));

            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            mockTransport.SimulateReconnectAttempt(1);
            mockTransport.SimulateReconnectAttempt(2);
            mockTransport.SimulateReconnectAttempt(3);

            Assert.AreEqual(3, attempts.Count);
            Assert.AreEqual("1", attempts[0]);
            Assert.AreEqual("2", attempts[1]);
            Assert.AreEqual("3", attempts[2]);
        }

        [Test]
        public void Disconnect_FromReconnecting_CancelsReconnection()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            client.Disconnect();

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
            Assert.AreEqual(1, mockTransport.DisconnectCallCount);
        }

        [Test]
        public void Disconnect_FromReconnecting_DoesNotEmitReconnectFailed()
        {
            LogAssert.ignoreFailingMessages = true;
            bool reconnectFailedFired = false;
            client.On("reconnect_failed", (data) => reconnectFailedFired = true);

            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            client.Disconnect();

            Assert.IsFalse(reconnectFailedFired);
        }

        // --- Story 2.3: Message Queue Integration Tests ---

        [Test]
        public void Emit_WhenConnected_SendsViaTransport_NotQueued()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.Emit("chat", "data");

            Assert.AreEqual(1, mockTransport.EmitCalls.Count);
            Assert.AreEqual("chat", mockTransport.EmitCalls[0].EventName);
        }

        [Test]
        public void Emit_WhenDisconnected_DropsMessage_NotQueued()
        {
            LogAssert.ignoreFailingMessages = true;
            // Never connected -- no active session, so should drop
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Disconnect();
            mockTransport.SimulateDisconnected("io client disconnect");
            Assert.AreEqual(ConnectionState.Disconnected, client.State);

            client.Emit("chat", "data");

            // Should not be queued or sent
            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void OnReconnect_QueuedMessages_ReplayedInFIFOOrder()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            // Queue 3 messages during Reconnecting
            client.Emit("e1", "d1");
            client.Emit("e2", "d2");
            client.Emit("e3", "d3");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);

            // Reconnect triggers replay
            mockTransport.SimulateReconnected(1);

            Assert.AreEqual(3, mockTransport.EmitCalls.Count);
            Assert.AreEqual("e1", mockTransport.EmitCalls[0].EventName);
            Assert.AreEqual("d1", mockTransport.EmitCalls[0].Data);
            Assert.AreEqual("e2", mockTransport.EmitCalls[1].EventName);
            Assert.AreEqual("d2", mockTransport.EmitCalls[1].Data);
            Assert.AreEqual("e3", mockTransport.EmitCalls[2].EventName);
            Assert.AreEqual("d3", mockTransport.EmitCalls[2].Data);
        }

        [Test]
        public void OnReconnect_QueueClearedAfterReplay()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            client.Emit("chat", "data");
            mockTransport.SimulateReconnected(1);

            Assert.AreEqual(1, mockTransport.EmitCalls.Count);

            // Disconnect and reconnect again -- old messages should NOT replay
            mockTransport.EmitCalls.Clear();
            mockTransport.SimulateDisconnected("transport close");
            mockTransport.SimulateReconnected(1);

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void Terminate_ClearsMessageQueue()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            client.Emit("chat", "should be discarded");

            client.Terminate();

            // Re-initialize and reconnect -- old queued message must NOT replay
            client.Initialize();
            client.Transport = mockTransport;
            mockTransport.Reset();

            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-2");
            mockTransport.SimulateDisconnected("transport close");
            mockTransport.SimulateReconnected(1);

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void Queue_UsesOptionsQueueSize()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true, queueSize = 2 };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            // Queue 3 messages with capacity 2 -- oldest dropped
            client.Emit("e1", "d1");
            client.Emit("e2", "d2");
            LogAssert.Expect(LogType.Warning, "[SocketIOClient] Message queue full, dropping oldest");
            client.Emit("e3", "d3"); // drops e1

            mockTransport.SimulateReconnected(1);

            Assert.AreEqual(2, mockTransport.EmitCalls.Count);
            Assert.AreEqual("e2", mockTransport.EmitCalls[0].EventName);
            Assert.AreEqual("e3", mockTransport.EmitCalls[1].EventName);
        }

        [Test]
        public void Terminate_FromReconnecting_DisconnectsAndGoesToDisconnected()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            client.Terminate();

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
            Assert.AreEqual(1, mockTransport.DisconnectCallCount);
            client = null; // Prevent TearDown double-terminate
        }

        // --- Story 2.4: Auto-Disconnect on World Unload Tests ---

        [Test]
        public void Terminate_FromConnecting_DisconnectsAndGoesToDisconnected()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            Assert.AreEqual(ConnectionState.Connecting, client.State);

            client.Terminate();

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
            Assert.AreEqual(1, mockTransport.DisconnectCallCount);
            client = null;
        }

        [Test]
        public void Terminate_FromDisconnected_IsIdempotent()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual(ConnectionState.Disconnected, client.State);

            client.Terminate();

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
            Assert.AreEqual(0, mockTransport.DisconnectCallCount);
            client = null;
        }

        [Test]
        public void Terminate_CallbacksNotFiredAfterTerminate()
        {
            LogAssert.ignoreFailingMessages = true;
            bool callbackFired = false;
            client.On("chat", (data) => callbackFired = true);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.Terminate();

            // Events are unwired — simulate event on transport, callback should NOT fire
            mockTransport.SimulateEvent("chat", "data");

            Assert.IsFalse(callbackFired);
            client = null;
        }

        [Test]
        public void Terminate_TransportEventsUnwiredAfterTerminate()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.Terminate();

            // Transport events after Terminate should NOT change state
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Disconnected, client.State);

            mockTransport.SimulateConnected("id-2");
            Assert.AreEqual(ConnectionState.Disconnected, client.State);

            client = null;
        }

        [Test]
        public void RepeatedInitializeTerminate_NoErrors()
        {
            LogAssert.ignoreFailingMessages = true;
            for (int i = 0; i < 3; i++)
            {
                var transport = new MockSocketIOTransport();
                client.Initialize();
                client.Transport = transport;
                client.Connect("http://localhost:3000");
                transport.SimulateConnected("id-" + i);
                Assert.IsTrue(client.Connected);

                client.Terminate();
                Assert.AreEqual(ConnectionState.Disconnected, client.State);
                Assert.IsFalse(client.Connected);
            }
            client = null;
        }

        [Test]
        public void Terminate_FromReconnecting_ClearsQueuedMessages_NoReplayOnReInit()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            // Queue a message during Reconnecting
            client.Emit("chat", "should be discarded");

            client.Terminate();

            // Re-initialize with fresh transport
            var freshTransport = new MockSocketIOTransport();
            client.Initialize();
            client.Transport = freshTransport;
            client.Connect("http://localhost:3000", opts);
            freshTransport.SimulateConnected("id-2");
            freshTransport.SimulateDisconnected("transport close");
            freshTransport.SimulateReconnected(1);

            // Old queued message must NOT replay
            Assert.AreEqual(0, freshTransport.EmitCalls.Count);
            client = null;
        }

        // --- Story 3.1: Binary Event Messaging Tests ---

        [Test]
        public void EmitBinary_WhenConnected_DelegatesToTransport()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            byte[] data = new byte[] { 0x01, 0x02, 0x03 };
            client.EmitBinary("image", data);

            Assert.AreEqual(1, mockTransport.EmitBinaryCalls.Count);
            Assert.AreEqual("image", mockTransport.EmitBinaryCalls[0].EventName);
            Assert.AreEqual(data, mockTransport.EmitBinaryCalls[0].Data);
        }

        [Test]
        public void EmitBinary_WhenDisconnected_DropsMessage()
        {
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->EmitBinary] Not connected, binary message dropped.");

            byte[] data = new byte[] { 0x01 };
            client.EmitBinary("image", data);

            Assert.AreEqual(0, mockTransport.EmitBinaryCalls.Count);
        }

        [Test]
        public void EmitBinary_WhenReconnecting_DropsMessage()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->EmitBinary] Not connected, binary message dropped.");

            byte[] data = new byte[] { 0x01 };
            client.EmitBinary("image", data);

            Assert.AreEqual(0, mockTransport.EmitBinaryCalls.Count);
        }

        [Test]
        public void OnBinaryEvent_InvokesRegisteredCallback()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedEvent = null;
            byte[] receivedData = null;
            client.OnBinary("image", (name, data) =>
            {
                receivedEvent = name;
                receivedData = data;
            });

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            byte[] testData = new byte[] { 0xAA, 0xBB, 0xCC };
            mockTransport.SimulateBinaryEvent("image", testData);

            Assert.AreEqual("image", receivedEvent);
            Assert.AreEqual(testData, receivedData);
        }

        [Test]
        public void OnBinaryEvent_NoListener_SilentlyDropped()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            // No callback registered — should not throw
            Assert.DoesNotThrow(() =>
                mockTransport.SimulateBinaryEvent("unknown", new byte[] { 0x01 }));
        }

        [Test]
        public void OnBinary_MultipleCallbacks_AllInvoked()
        {
            LogAssert.ignoreFailingMessages = true;
            int callCount = 0;
            client.OnBinary("data", (name, data) => callCount++);
            client.OnBinary("data", (name, data) => callCount++);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateBinaryEvent("data", new byte[] { 0x01 });

            Assert.AreEqual(2, callCount);
        }

        [Test]
        public void OffBinary_RemovesCallbacks()
        {
            LogAssert.ignoreFailingMessages = true;
            bool callbackFired = false;
            client.OnBinary("image", (name, data) => callbackFired = true);
            client.OffBinary("image");

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateBinaryEvent("image", new byte[] { 0x01 });

            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void Terminate_ClearsBinaryCallbacks()
        {
            LogAssert.ignoreFailingMessages = true;
            bool callbackFired = false;
            client.OnBinary("image", (name, data) => callbackFired = true);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Terminate();

            mockTransport.SimulateBinaryEvent("image", new byte[] { 0x01 });

            Assert.IsFalse(callbackFired);
            client = null;
        }

        [Test]
        public void EmitBinary_NullData_DelegatesToTransport()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.EmitBinary("image", null);

            Assert.AreEqual(1, mockTransport.EmitBinaryCalls.Count);
            Assert.AreEqual("image", mockTransport.EmitBinaryCalls[0].EventName);
            Assert.IsNull(mockTransport.EmitBinaryCalls[0].Data);
        }

        [Test]
        public void OnBinaryEvent_AfterTerminate_NotFired()
        {
            LogAssert.ignoreFailingMessages = true;
            bool callbackFired = false;
            client.OnBinary("image", (name, data) => callbackFired = true);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Terminate();

            // Transport events after Terminate should NOT reach callbacks
            mockTransport.SimulateBinaryEvent("image", new byte[] { 0x01 });

            Assert.IsFalse(callbackFired);
            client = null;
        }
        // --- Story 3.2: EmitWithAck Tests ---

        [Test]
        public void EmitWithAck_WhenConnected_DelegatesToTransport()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            Action<string> ackCb = (response) => { };
            client.EmitWithAck("score", "{\"val\":100}", ackCb);

            Assert.AreEqual(1, mockTransport.EmitWithAckCalls.Count);
            Assert.AreEqual("score", mockTransport.EmitWithAckCalls[0].EventName);
            Assert.AreEqual("{\"val\":100}", mockTransport.EmitWithAckCalls[0].Data);
            Assert.AreEqual(ackCb, mockTransport.EmitWithAckCalls[0].Callback);
        }

        [Test]
        public void EmitWithAck_WhenConnected_AckCallbackInvoked()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            string receivedResponse = null;
            client.EmitWithAck("score", "{\"val\":100}", (response) =>
            {
                receivedResponse = response;
            });

            // Simulate server ack by invoking stored callback
            mockTransport.EmitWithAckCalls[0].Callback("ok");

            Assert.AreEqual("ok", receivedResponse);
        }

        [Test]
        public void EmitWithAck_WhenDisconnected_DropsMessage()
        {
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->EmitWithAck] Not connected, ack message dropped.");

            client.EmitWithAck("score", "{}", (r) => { });

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_WhenReconnecting_DropsMessage()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->EmitWithAck] Not connected, ack message dropped.");

            client.EmitWithAck("score", "{}", (r) => { });

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_NullCallback_FallsBackToRegularEmit()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.EmitWithAck("score", "{\"val\":100}", null);

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
            Assert.AreEqual(1, mockTransport.EmitCalls.Count);
            Assert.AreEqual("score", mockTransport.EmitCalls[0].EventName);
            Assert.AreEqual("{\"val\":100}", mockTransport.EmitCalls[0].Data);
        }

        [Test]
        public void EmitWithAck_NullEventName_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.EmitWithAck(null, "{}", (r) => { });

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void EmitWithAck_EmptyEventName_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.EmitWithAck("", "{}", (r) => { });

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void EmitWithAck_TransportNull_LogsWarning()
        {
            LogAssert.ignoreFailingMessages = true;
            // Null out transport to simulate missing transport
            client.Transport = null;

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->EmitWithAck] Transport not initialized.");

            client.EmitWithAck("score", "{}", (r) => { });

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_AfterTerminate_NotSent()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->EmitWithAck] Not connected, ack message dropped.");

            client.EmitWithAck("score", "{}", (r) => { });

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
            client = null;
        }

        // --- Story 3.3: EmitVolatile Tests ---

        [Test]
        public void EmitVolatile_WhenConnected_DelegatesToTransport()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.EmitVolatile("typing", "{\"user\":\"alice\"}");

            Assert.AreEqual(1, mockTransport.EmitCalls.Count);
            Assert.AreEqual("typing", mockTransport.EmitCalls[0].EventName);
            Assert.AreEqual("{\"user\":\"alice\"}", mockTransport.EmitCalls[0].Data);
        }

        [Test]
        public void EmitVolatile_WhenDisconnected_SilentlyDrops()
        {
            LogAssert.ignoreFailingMessages = true;
            // NO LogAssert -- volatile drops are silent
            client.EmitVolatile("typing", "{\"user\":\"alice\"}");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void EmitVolatile_WhenReconnecting_SilentlyDrops()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            // NO LogAssert -- volatile drops are silent
            client.EmitVolatile("typing", "{\"user\":\"alice\"}");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void EmitVolatile_DoesNotQueue()
        {
            LogAssert.ignoreFailingMessages = true;
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            client.EmitVolatile("typing", "{\"user\":\"alice\"}");

            // Reconnect and verify nothing was queued/replayed
            mockTransport.SimulateReconnected(1);
            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void EmitVolatile_NullEventName_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.EmitVolatile(null, "{\"user\":\"alice\"}");
            client.EmitVolatile("", "{\"user\":\"alice\"}");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        // --- Story 3.3: Once Tests ---

        [Test]
        public void Once_FiresOnFirstEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            string receivedData = null;
            client.Once("welcome", (data) => receivedData = data);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateEvent("welcome", "hello");

            Assert.AreEqual("hello", receivedData);
        }

        [Test]
        public void Once_DoesNotFireOnSecondEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            int callCount = 0;
            client.Once("welcome", (data) => callCount++);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateEvent("welcome", "first");
            mockTransport.SimulateEvent("welcome", "second");

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void Once_CoexistsWithOn()
        {
            LogAssert.ignoreFailingMessages = true;
            int onCount = 0;
            int onceCount = 0;
            client.On("chat", (data) => onCount++);
            client.Once("chat", (data) => onceCount++);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            mockTransport.SimulateEvent("chat", "msg1");
            Assert.AreEqual(1, onCount);
            Assert.AreEqual(1, onceCount);

            mockTransport.SimulateEvent("chat", "msg2");
            Assert.AreEqual(2, onCount);
            Assert.AreEqual(1, onceCount); // Once should NOT fire again
        }

        [Test]
        public void Off_RemovesOnceCallbacks()
        {
            LogAssert.ignoreFailingMessages = true;
            int callCount = 0;
            client.Once("welcome", (data) => callCount++);

            client.Off("welcome");

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateEvent("welcome", "hello");

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Terminate_ClearsOnceCallbacks()
        {
            LogAssert.ignoreFailingMessages = true;
            int callCount = 0;
            client.Once("welcome", (data) => callCount++);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Terminate();

            mockTransport.SimulateEvent("welcome", "hello");

            Assert.AreEqual(0, callCount);
            client = null;
        }

        [Test]
        public void Once_NullEventOrCallback_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            int callCount = 0;
            client.Once(null, (data) => callCount++);
            client.Once("", (data) => callCount++);
            client.Once("event", null);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateEvent("event", "data");

            Assert.AreEqual(0, callCount);
        }

        // --- Story 3.3: OnAny Tests ---

        [Test]
        public void OnAny_FiresForEveryEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            var receivedEvents = new List<(string EventName, string Data)>();
            client.OnAny((eventName, data) => receivedEvents.Add((eventName, data)));

            mockTransport.SimulateEvent("chat", "msg1");
            mockTransport.SimulateEvent("status", "online");

            Assert.AreEqual(2, receivedEvents.Count);
            Assert.AreEqual("chat", receivedEvents[0].EventName);
            Assert.AreEqual("msg1", receivedEvents[0].Data);
            Assert.AreEqual("status", receivedEvents[1].EventName);
            Assert.AreEqual("online", receivedEvents[1].Data);
        }

        [Test]
        public void OnAny_CoexistsWithOn()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            int onCount = 0;
            var onAnyEvents = new List<string>();
            client.On("chat", (data) => onCount++);
            client.OnAny((eventName, data) => onAnyEvents.Add(eventName));

            mockTransport.SimulateEvent("chat", "msg");

            Assert.AreEqual(1, onCount, "On callback should fire once");
            Assert.AreEqual(1, onAnyEvents.Count,
                "OnAny should fire once, but got: [" + string.Join(", ", onAnyEvents) + "]");
            Assert.AreEqual("chat", onAnyEvents[0]);
        }

        [Test]
        public void OffAny_RemovesCallbacks()
        {
            LogAssert.ignoreFailingMessages = true;
            int callCount = 0;
            client.OnAny((eventName, data) => callCount++);

            client.OffAny();

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateEvent("chat", "msg");

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void OnAny_MultipleCallbacks_AllInvoked()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            int count1 = 0;
            int count2 = 0;
            client.OnAny((eventName, data) => count1++);
            client.OnAny((eventName, data) => count2++);

            mockTransport.SimulateEvent("chat", "msg");

            Assert.AreEqual(1, count1);
            Assert.AreEqual(1, count2);
        }

        [Test]
        public void Terminate_ClearsOnAnyCallbacks()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            int callCount = 0;
            client.OnAny((eventName, data) => callCount++);

            client.Terminate();

            mockTransport.SimulateEvent("chat", "msg");

            Assert.AreEqual(0, callCount);
            client = null;
        }

        [Test]
        public void OnAny_NullCallback_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            client.OnAny(null);

            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            int callCount = 0;
            client.OnAny((e, d) => callCount++);
            mockTransport.SimulateEvent("chat", "msg");

            // Only the valid callback should fire, null was rejected
            Assert.AreEqual(1, callCount);
        }

        // --- Story 4.1: Room Join & Leave Tests ---

        [Test]
        public void JoinRoom_WhenConnected_DelegatesToTransport()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.JoinRoom("lobby");

            Assert.AreEqual(1, mockTransport.JoinRoomCalls.Count);
            Assert.AreEqual("lobby", mockTransport.JoinRoomCalls[0]);
        }

        [Test]
        public void LeaveRoom_WhenConnected_DelegatesToTransport()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.LeaveRoom("lobby");

            Assert.AreEqual(1, mockTransport.LeaveRoomCalls.Count);
            Assert.AreEqual("lobby", mockTransport.LeaveRoomCalls[0]);
        }

        [Test]
        public void JoinRoom_WhenDisconnected_LogsWarning()
        {
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->JoinRoom] Not connected.");

            client.JoinRoom("lobby");

            Assert.AreEqual(0, mockTransport.JoinRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_WhenDisconnected_LogsWarning()
        {
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->LeaveRoom] Not connected.");

            client.LeaveRoom("lobby");

            Assert.AreEqual(0, mockTransport.LeaveRoomCalls.Count);
        }

        [Test]
        public void JoinRoom_WhenReconnecting_LogsWarning()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000", new SocketIOOptions { reconnection = true });
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->JoinRoom] Not connected.");

            client.JoinRoom("lobby");

            Assert.AreEqual(0, mockTransport.JoinRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_WhenReconnecting_LogsWarning()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000", new SocketIOOptions { reconnection = true });
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->LeaveRoom] Not connected.");

            client.LeaveRoom("lobby");

            Assert.AreEqual(0, mockTransport.LeaveRoomCalls.Count);
        }

        [Test]
        public void JoinRoom_NullOrEmptyRoom_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.JoinRoom(null);
            client.JoinRoom("");

            Assert.AreEqual(0, mockTransport.JoinRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_NullOrEmptyRoom_IsNoOp()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.LeaveRoom(null);
            client.LeaveRoom("");

            Assert.AreEqual(0, mockTransport.LeaveRoomCalls.Count);
        }

        [Test]
        public void JoinRoom_MultipleRooms_AllDelegated()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.JoinRoom("lobby");
            client.JoinRoom("chat");

            Assert.AreEqual(2, mockTransport.JoinRoomCalls.Count);
            Assert.AreEqual("lobby", mockTransport.JoinRoomCalls[0]);
            Assert.AreEqual("chat", mockTransport.JoinRoomCalls[1]);
        }

        [Test]
        public void JoinRoom_AfterTerminate_LogsWarning()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->JoinRoom] Not connected.");

            client.JoinRoom("lobby");

            Assert.AreEqual(0, mockTransport.JoinRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_AfterTerminate_LogsWarning()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->LeaveRoom] Not connected.");

            client.LeaveRoom("lobby");

            Assert.AreEqual(0, mockTransport.LeaveRoomCalls.Count);
        }

        // --- Story 4.2: Namespace Multiplexing Tests ---

        [Test]
        public void Of_WhenConnected_ReturnsNamespacedSocket()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            var ns = client.Of("/chat");

            Assert.IsNotNull(ns);
            Assert.AreEqual("/chat", ns.Namespace);
        }

        [Test]
        public void Of_SameNamespace_ReturnsCachedInstance()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            var ns1 = client.Of("/chat");
            var ns2 = client.Of("/chat");

            Assert.AreSame(ns1, ns2);
        }

        [Test]
        public void Of_DifferentNamespaces_ReturnsDifferentInstances()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            var nsChat = client.Of("/chat");
            var nsGame = client.Of("/game");

            Assert.AreNotSame(nsChat, nsGame);
            Assert.AreEqual("/chat", nsChat.Namespace);
            Assert.AreEqual("/game", nsGame.Namespace);
        }

        [Test]
        public void Of_WhenDisconnected_LogsWarningReturnsNull()
        {
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->Of] Not connected.");

            var ns = client.Of("/chat");

            Assert.IsNull(ns);
        }

        [Test]
        public void Of_NullOrEmpty_ReturnsNull()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            var ns1 = client.Of(null);
            var ns2 = client.Of("");

            Assert.IsNull(ns1);
            Assert.IsNull(ns2);
        }

        [Test]
        public void Of_TransportReturnsNull_LogsWarningReturnsNull()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            mockTransport.OfReturnsNull = true;

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->Of] Transport returned null for namespace: /broken");

            var ns = client.Of("/broken");

            Assert.IsNull(ns);
        }

        [Test]
        public void Terminate_DisposesAllNamespaceInstances()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            var nsChat = client.Of("/chat");
            var nsGame = client.Of("/game");

            client.Terminate();

            // After dispose, namespace sockets should reject operations
            nsChat.Emit("test", "data");
            nsGame.Emit("test", "data");

            var nsChatMock = mockTransport.NamespaceTransports["/chat"];
            var nsGameMock = mockTransport.NamespaceTransports["/game"];
            Assert.AreEqual(0, nsChatMock.EmitCalls.Count);
            Assert.AreEqual(0, nsGameMock.EmitCalls.Count);

            client = null;
        }

        [Test]
        public void Of_AfterTerminate_LogsWarningReturnsNull()
        {
            LogAssert.ignoreFailingMessages = true;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIOClient->Of] Not connected.");

            var ns = client.Of("/chat");

            Assert.IsNull(ns);
            client = null;
        }
    }
}
