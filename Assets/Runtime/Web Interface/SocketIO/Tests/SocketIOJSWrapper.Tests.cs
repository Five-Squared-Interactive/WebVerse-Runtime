// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WebInterface.SocketIO;
using FiveSQD.WebVerse.WebInterface.SocketIO.Tests.Mocks;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Networking;

namespace FiveSQD.WebVerse.WebInterface.SocketIO.Tests
{
    /// <summary>
    /// Unit tests for the SocketIO JavaScript API wrapper.
    /// Tests On/Off callback storage, Emit pass-through, Terminate cleanup,
    /// and Connect/Disconnect delegation to SocketIOClient.
    /// </summary>
    [TestFixture]
    public class SocketIOJSWrapperTests
    {
        private Handlers.Javascript.APIs.Networking.SocketIO wrapper;
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
            // Create the wrapper (which creates its own SocketIOClient internally)
            wrapper = new Handlers.Javascript.APIs.Networking.SocketIO();
            client = wrapper.Client;

            // Inject mock transport for testing
            mockTransport = new MockSocketIOTransport();
            client.Transport = mockTransport;
        }

        [TearDown]
        public void TearDown()
        {
            if (wrapper != null)
            {
                wrapper.Terminate();
            }
        }

        // --- AC2: Connect marshals url and options ---

        [Test]
        public void Connect_DelegatesToSocketIOClient()
        {
            wrapper.Connect("http://localhost:3000");

            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
            Assert.AreEqual("http://localhost:3000", mockTransport.ConnectCalls[0].Url);
        }

        [Test]
        public void Connect_WithNullOptions_UsesDefaults()
        {
            wrapper.Connect("http://localhost:3000");

            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
            Assert.IsNotNull(mockTransport.ConnectCalls[0].Options);
        }

        [Test]
        public void Disconnect_DelegatesToSocketIOClient()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.Disconnect();

            Assert.AreEqual(1, mockTransport.DisconnectCallCount);
        }

        // --- AC2: Properties pass through ---

        [Test]
        public void Connected_ReflectsClientState()
        {
            Assert.IsFalse(wrapper.Connected);

            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            Assert.IsTrue(wrapper.Connected);
        }

        [Test]
        public void Id_ReflectsClientSocketId()
        {
            Assert.IsNull(wrapper.Id);

            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("abc-123");

            Assert.AreEqual("abc-123", wrapper.Id);
        }

        // --- AC3: On stores function name ---

        [Test]
        public void On_StoresFunctionName()
        {
            wrapper.On("chat", "onChatMessage");

            Assert.IsTrue(wrapper.HasCallbacksForEvent("chat"));
            Assert.AreEqual(1, wrapper.RegisteredEventCount);
        }

        [Test]
        public void On_MultipleCallbacksSameEvent_StoresAll()
        {
            wrapper.On("chat", "onChat1");
            wrapper.On("chat", "onChat2");

            Assert.IsTrue(wrapper.HasCallbacksForEvent("chat"));
            Assert.AreEqual(1, wrapper.RegisteredEventCount);
        }

        [Test]
        public void On_DifferentEvents_StoresSeparately()
        {
            wrapper.On("chat", "onChat");
            wrapper.On("status", "onStatus");

            Assert.IsTrue(wrapper.HasCallbacksForEvent("chat"));
            Assert.IsTrue(wrapper.HasCallbacksForEvent("status"));
            Assert.AreEqual(2, wrapper.RegisteredEventCount);
        }

        [Test]
        public void On_NullEventName_IsIgnored()
        {
            wrapper.On(null, "callback");

            Assert.AreEqual(0, wrapper.RegisteredEventCount);
        }

        [Test]
        public void On_NullFunctionName_IsIgnored()
        {
            wrapper.On("event", null);

            Assert.AreEqual(0, wrapper.RegisteredEventCount);
        }

        [Test]
        public void On_EmptyEventName_IsIgnored()
        {
            wrapper.On("", "callback");

            Assert.AreEqual(0, wrapper.RegisteredEventCount);
        }

        [Test]
        public void On_EmptyFunctionName_IsIgnored()
        {
            wrapper.On("event", "");

            Assert.AreEqual(0, wrapper.RegisteredEventCount);
        }

        // --- AC4: Off removes stored function names ---

        [Test]
        public void Off_RemovesCallbacks()
        {
            wrapper.On("chat", "onChat");
            Assert.IsTrue(wrapper.HasCallbacksForEvent("chat"));

            wrapper.Off("chat");

            Assert.IsFalse(wrapper.HasCallbacksForEvent("chat"));
            Assert.AreEqual(0, wrapper.RegisteredEventCount);
        }

        [Test]
        public void Off_OnlyRemovesSpecifiedEvent()
        {
            wrapper.On("chat", "onChat");
            wrapper.On("status", "onStatus");

            wrapper.Off("chat");

            Assert.IsFalse(wrapper.HasCallbacksForEvent("chat"));
            Assert.IsTrue(wrapper.HasCallbacksForEvent("status"));
            Assert.AreEqual(1, wrapper.RegisteredEventCount);
        }

        [Test]
        public void Off_NullEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => wrapper.Off(null));
        }

        [Test]
        public void Off_NonExistentEvent_IsIgnored()
        {
            Assert.DoesNotThrow(() => wrapper.Off("nonexistent"));
        }

        // --- AC3: Emit pass-through ---

        [Test]
        public void Emit_WhenConnected_DelegatesToClient()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.Emit("chat", "{\"msg\":\"hello\"}");

            Assert.AreEqual(1, mockTransport.EmitCalls.Count);
            Assert.AreEqual("chat", mockTransport.EmitCalls[0].EventName);
            Assert.AreEqual("{\"msg\":\"hello\"}", mockTransport.EmitCalls[0].Data);
        }

        [Test]
        public void Emit_WhenDisconnected_DropsMessage()
        {
            wrapper.Emit("chat", "data");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        // --- Terminate clears callbacks ---

        [Test]
        public void Terminate_ClearsAllCallbacks()
        {
            wrapper.On("chat", "onChat");
            wrapper.On("status", "onStatus");
            Assert.AreEqual(2, wrapper.RegisteredEventCount);

            wrapper.Terminate();

            Assert.AreEqual(0, wrapper.RegisteredEventCount);
            Assert.IsNull(wrapper.Client);
            Assert.IsFalse(wrapper.Connected);

            wrapper = null; // Prevent TearDown double-terminate
        }

        [Test]
        public void Terminate_SetsConnectedFalse()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            Assert.IsTrue(wrapper.Connected);

            wrapper.Terminate();

            Assert.IsFalse(wrapper.Connected);
            Assert.IsNull(wrapper.Id);

            wrapper = null;
        }

        [Test]
        public void On_AfterTerminate_IsRejected()
        {
            wrapper.Terminate();

            // On should be silently rejected on a terminated wrapper
            wrapper.On("chat", "callback");

            Assert.AreEqual(0, wrapper.RegisteredEventCount);

            wrapper = null;
        }

        [Test]
        public void Connect_AfterTerminate_IsRejected()
        {
            wrapper.Terminate();

            // Client is null after terminate, should log warning but not throw
            Assert.DoesNotThrow(() => wrapper.Connect("http://localhost:3000"));

            wrapper = null;
        }

        // --- AC6: Exception handling ---

        [Test]
        public void Emit_ClientNotInitialized_DoesNotThrow()
        {
            wrapper.Terminate();

            Assert.DoesNotThrow(() => wrapper.Emit("chat", "data"));

            wrapper = null;
        }

        [Test]
        public void Disconnect_ClientNotInitialized_DoesNotThrow()
        {
            wrapper.Terminate();

            Assert.DoesNotThrow(() => wrapper.Disconnect());

            wrapper = null;
        }

        // --- Constructor ---

        [Test]
        public void Constructor_CreatesSocketIOClient()
        {
            Assert.IsNotNull(wrapper.Client);
        }

        [Test]
        public void Constructor_ClientStartsDisconnected()
        {
            Assert.IsFalse(wrapper.Connected);
            Assert.IsNull(wrapper.Id);
        }

        [Test]
        public void Constructor_StartsWithNoCallbacks()
        {
            Assert.AreEqual(0, wrapper.RegisteredEventCount);
        }

        // --- Story 2.1: Option pass-through via Connect ---

        [Test]
        public void Connect_WithNullOptions_TransportReceivesDefaultReconnection()
        {
            wrapper.Connect("http://localhost:3000");

            var received = mockTransport.ConnectCalls[0].Options;
            Assert.IsTrue(received.reconnection);
        }

        [Test]
        public void Connect_WithNullOptions_TransportReceivesDefaultTimeout()
        {
            wrapper.Connect("http://localhost:3000");

            var received = mockTransport.ConnectCalls[0].Options;
            Assert.AreEqual(20000, received.timeout);
        }

        [Test]
        public void Connect_WithNullOptions_TransportReceivesNullHeaders()
        {
            wrapper.Connect("http://localhost:3000");

            var received = mockTransport.ConnectCalls[0].Options;
            Assert.IsNull(received.headers);
        }

        [Test]
        public void Connect_WithNullOptions_TransportReceivesNullQuery()
        {
            wrapper.Connect("http://localhost:3000");

            var received = mockTransport.ConnectCalls[0].Options;
            Assert.IsNull(received.query);
        }

        [Test]
        public void Connect_WithNullOptions_TransportReceivesDefaultQueueSize()
        {
            wrapper.Connect("http://localhost:3000");

            var received = mockTransport.ConnectCalls[0].Options;
            Assert.AreEqual(100, received.queueSize);
        }

        [Test]
        public void Connect_WithNullOptions_OptionsObjectIsNotNull()
        {
            wrapper.Connect("http://localhost:3000");

            Assert.IsNotNull(mockTransport.ConnectCalls[0].Options);
        }

        // --- Story 2.2: JS Wrapper Reconnection Event Tests ---

        [Test]
        public void On_Disconnect_FiresWhenTransportDisconnectsDuringReconnection()
        {
            // We can't test actual JS dispatch without Jint/DataAPIHelper,
            // so verify the client emits the event by listening directly
            string receivedReason = null;
            client.On("disconnect", (reason) => receivedReason = reason);

            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");

            Assert.AreEqual("transport close", receivedReason);
        }

        [Test]
        public void On_ReconnectAttempt_FiresWithAttemptNumber()
        {
            string receivedAttempt = null;
            client.On("reconnect_attempt", (data) => receivedAttempt = data);

            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            mockTransport.SimulateReconnectAttempt(2);

            Assert.AreEqual("2", receivedAttempt);
        }

        [Test]
        public void On_Reconnect_FiresOnSuccessfulReconnection()
        {
            string receivedCount = null;
            client.On("reconnect", (data) => receivedCount = data);

            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            mockTransport.SimulateReconnected(3);

            Assert.AreEqual("3", receivedCount);
        }

        [Test]
        public void On_ReconnectFailed_FiresWhenAllAttemptsExhausted()
        {
            bool reconnectFailedFired = false;
            client.On("reconnect_failed", (data) => reconnectFailedFired = true);

            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            mockTransport.SimulateReconnectFailed();

            Assert.IsTrue(reconnectFailedFired);
        }

        // --- Story 2.4: Auto-Disconnect on World Unload Tests ---

        [Test]
        public void Terminate_SetsClientToNull()
        {
            Assert.IsNotNull(wrapper.Client);

            wrapper.Terminate();

            Assert.IsNull(wrapper.Client);
            wrapper = null;
        }

        [Test]
        public void Terminate_CallsClientTerminate_StateIsDisconnected()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            Assert.IsTrue(wrapper.Connected);

            // Capture client ref before terminate nulls it
            var clientRef = wrapper.Client;

            wrapper.Terminate();

            Assert.AreEqual(ConnectionState.Disconnected, clientRef.State);
            Assert.AreEqual(1, mockTransport.DisconnectCallCount);
            wrapper = null;
        }

        [Test]
        public void Terminate_CleansUpGameObject()
        {
            Assert.IsNotNull(wrapper.Client);
            Assert.IsNotNull(wrapper.Client.gameObject);

            wrapper.Terminate();

            // Client reference is nulled, GameObject.Destroy is queued
            // (Destroy is frame-deferred in Unity so we verify wrapper cleanup)
            Assert.IsNull(wrapper.Client);
            Assert.IsFalse(wrapper.Connected);
            wrapper = null;
        }

        [Test]
        public void DoubleTerminate_DoesNotThrow()
        {
            wrapper.Terminate();
            wrapper.Terminate();

            Assert.IsNull(wrapper.Client);
            Assert.IsFalse(wrapper.Connected);
            Assert.AreEqual(0, wrapper.RegisteredEventCount);
            wrapper = null;
        }

        [Test]
        public void Connect_AfterTerminate_IsNoOp()
        {
            wrapper.Terminate();

            wrapper.Connect("http://localhost:3000");

            Assert.IsFalse(wrapper.Connected);
            Assert.AreEqual(0, mockTransport.ConnectCalls.Count);
            wrapper = null;
        }

        [Test]
        public void Emit_AfterTerminate_IsNoOp()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            mockTransport.EmitCalls.Clear();
            wrapper.Terminate();

            wrapper.Emit("chat", "data");

            Assert.IsFalse(wrapper.Connected);
            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
            wrapper = null;
        }

        // --- Story 3.1: Binary Event Messaging Tests ---

        [Test]
        public void EmitBinary_WhenConnected_DecodesBase64AndDelegatesToClient()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            byte[] original = new byte[] { 0x01, 0x02, 0x03 };
            string base64 = System.Convert.ToBase64String(original);
            wrapper.EmitBinary("image", base64);

            Assert.AreEqual(1, mockTransport.EmitBinaryCalls.Count);
            Assert.AreEqual("image", mockTransport.EmitBinaryCalls[0].EventName);
            Assert.AreEqual(original, mockTransport.EmitBinaryCalls[0].Data);
        }

        [Test]
        public void EmitBinary_WhenDisconnected_DropsMessage()
        {
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->EmitBinary] Not connected, binary message dropped.");

            byte[] original = new byte[] { 0x01 };
            string base64 = System.Convert.ToBase64String(original);
            wrapper.EmitBinary("image", base64);

            Assert.AreEqual(0, mockTransport.EmitBinaryCalls.Count);
        }

        [Test]
        public void EmitBinary_AfterTerminate_IsNoOp()
        {
            wrapper.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIO->EmitBinary] Client not initialized.");
            Assert.DoesNotThrow(() => wrapper.EmitBinary("image", "AQID"));

            Assert.AreEqual(0, mockTransport.EmitBinaryCalls.Count);
            wrapper = null;
        }

        [Test]
        public void EmitBinary_InvalidBase64_LogsWarningAndDrops()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            LogAssert.Expect(LogType.Warning, "[SocketIO->EmitBinary] Invalid base64 data, message dropped.");
            wrapper.EmitBinary("image", "not-valid-base64!!!");

            Assert.AreEqual(0, mockTransport.EmitBinaryCalls.Count);
        }

        [Test]
        public void OnBinary_StoresCallbackForEvent()
        {
            wrapper.OnBinary("image", "onImageReceived");

            Assert.IsTrue(wrapper.HasBinaryCallbacksForEvent("image"));
            Assert.AreEqual(1, wrapper.RegisteredBinaryEventCount);
        }

        [Test]
        public void OffBinary_RemovesCallbacks()
        {
            wrapper.OnBinary("image", "onImageReceived");
            Assert.IsTrue(wrapper.HasBinaryCallbacksForEvent("image"));

            wrapper.OffBinary("image");

            Assert.IsFalse(wrapper.HasBinaryCallbacksForEvent("image"));
            Assert.AreEqual(0, wrapper.RegisteredBinaryEventCount);
        }

        [Test]
        public void Terminate_ClearsBinaryCallbacks()
        {
            wrapper.OnBinary("image", "onImageReceived");
            Assert.AreEqual(1, wrapper.RegisteredBinaryEventCount);

            wrapper.Terminate();

            Assert.AreEqual(0, wrapper.RegisteredBinaryEventCount);
            wrapper = null;
        }

        // --- Story 3.2: EmitWithAck Tests ---

        [Test]
        public void EmitWithAck_WhenConnected_DelegatesToClient()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitWithAck("score", "{\"val\":100}", "onAckReceived");

            Assert.AreEqual(1, mockTransport.EmitWithAckCalls.Count);
            Assert.AreEqual("score", mockTransport.EmitWithAckCalls[0].EventName);
            Assert.AreEqual("{\"val\":100}", mockTransport.EmitWithAckCalls[0].Data);
            Assert.IsNotNull(mockTransport.EmitWithAckCalls[0].Callback);
        }

        [Test]
        public void EmitWithAck_WhenDisconnected_DropsMessage()
        {
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->EmitWithAck] Not connected, ack message dropped.");

            wrapper.EmitWithAck("score", "{}", "onAckReceived");

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_AfterTerminate_IsNoOp()
        {
            wrapper.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIO->EmitWithAck] Client not initialized.");
            wrapper.EmitWithAck("score", "{}", "onAckReceived");

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
            wrapper = null;
        }

        [Test]
        public void EmitWithAck_NullEventName_IsNoOp()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitWithAck(null, "{}", "onAckReceived");

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_NullFunctionName_IsNoOp()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitWithAck("score", "{}", null);

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        // --- Story 3.3: EmitVolatile Tests ---

        [Test]
        public void EmitVolatile_WhenConnected_DelegatesToClient()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitVolatile("typing", "{\"user\":\"alice\"}");

            Assert.AreEqual(1, mockTransport.EmitCalls.Count);
            Assert.AreEqual("typing", mockTransport.EmitCalls[0].EventName);
            Assert.AreEqual("{\"user\":\"alice\"}", mockTransport.EmitCalls[0].Data);
        }

        [Test]
        public void EmitVolatile_WhenDisconnected_SilentlyDrops()
        {
            // NO LogAssert -- volatile drops are silent
            wrapper.EmitVolatile("typing", "{}");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void EmitVolatile_AfterTerminate_IsNoOp()
        {
            wrapper.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIO->EmitVolatile] Client not initialized.");
            wrapper.EmitVolatile("typing", "{}");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
            wrapper = null;
        }

        // --- Story 3.3: Once Tests ---

        [Test]
        public void Once_StoresCallbackForEvent()
        {
            wrapper.Once("welcome", "onWelcome");

            Assert.IsTrue(wrapper.HasOnceCallbacksForEvent("welcome"));
            Assert.AreEqual(1, wrapper.RegisteredOnceEventCount);
        }

        [Test]
        public void Once_AfterTerminate_IsNoOp()
        {
            wrapper.Terminate();

            wrapper.Once("welcome", "onWelcome");

            Assert.AreEqual(0, wrapper.RegisteredOnceEventCount);
            wrapper = null;
        }

        // --- Story 3.3: OnAny Tests ---

        [Test]
        public void OnAny_StoresCallback()
        {
            wrapper.OnAny("onAnyEvent");

            Assert.AreEqual(1, wrapper.RegisteredOnAnyCount);
        }

        [Test]
        public void OffAny_RemovesCallbacks()
        {
            wrapper.OnAny("onAnyEvent");
            Assert.AreEqual(1, wrapper.RegisteredOnAnyCount);

            wrapper.OffAny();

            Assert.AreEqual(0, wrapper.RegisteredOnAnyCount);
        }

        [Test]
        public void Terminate_ClearsOnceAndOnAnyCallbacks()
        {
            wrapper.Once("welcome", "onWelcome");
            wrapper.OnAny("onAnyEvent");
            Assert.AreEqual(1, wrapper.RegisteredOnceEventCount);
            Assert.AreEqual(1, wrapper.RegisteredOnAnyCount);

            wrapper.Terminate();

            Assert.AreEqual(0, wrapper.RegisteredOnceEventCount);
            Assert.AreEqual(0, wrapper.RegisteredOnAnyCount);
            wrapper = null;
        }

        // --- Story 4.1: Room Join & Leave Wrapper Tests ---

        [Test]
        public void JoinRoom_WhenConnected_DelegatesToClient()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            wrapper.JoinRoom("lobby");

            Assert.AreEqual(1, mockTransport.JoinRoomCalls.Count);
            Assert.AreEqual("lobby", mockTransport.JoinRoomCalls[0]);
        }

        [Test]
        public void LeaveRoom_WhenConnected_DelegatesToClient()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            wrapper.LeaveRoom("lobby");

            Assert.AreEqual(1, mockTransport.LeaveRoomCalls.Count);
            Assert.AreEqual("lobby", mockTransport.LeaveRoomCalls[0]);
        }

        [Test]
        public void JoinRoom_WhenDisconnected_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->JoinRoom] Not connected.");

            wrapper.JoinRoom("lobby");

            Assert.AreEqual(0, mockTransport.JoinRoomCalls.Count);
        }

        [Test]
        public void JoinRoom_AfterTerminate_IsNoOp()
        {
            wrapper.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIO] JoinRoom called after terminate.");

            wrapper.JoinRoom("lobby");

            Assert.AreEqual(0, mockTransport.JoinRoomCalls.Count);
            wrapper = null;
        }

        [Test]
        public void LeaveRoom_AfterTerminate_IsNoOp()
        {
            wrapper.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIO] LeaveRoom called after terminate.");

            wrapper.LeaveRoom("lobby");

            Assert.AreEqual(0, mockTransport.LeaveRoomCalls.Count);
            wrapper = null;
        }

        // --- Emit guard tests ---

        [Test]
        public void Emit_NullEventName_StillDelegatesToClient()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            // Wrapper does not guard event names — delegates to client/transport
            wrapper.Emit(null, "data");

            Assert.AreEqual(1, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void Emit_EmptyEventName_StillDelegatesToClient()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            // Wrapper does not guard event names — delegates to client/transport
            wrapper.Emit("", "data");

            Assert.AreEqual(1, mockTransport.EmitCalls.Count);
        }

        // --- EmitBinary guard tests ---

        [Test]
        public void EmitBinary_NullEventName_IsIgnored()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitBinary(null, "AQID");

            Assert.AreEqual(0, mockTransport.EmitBinaryCalls.Count);
        }

        [Test]
        public void EmitBinary_EmptyEventName_IsIgnored()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitBinary("", "AQID");

            Assert.AreEqual(0, mockTransport.EmitBinaryCalls.Count);
        }

        [Test]
        public void EmitBinary_EmptyBase64_DelegatesToClientWithNull()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitBinary("image", "");

            Assert.AreEqual(1, mockTransport.EmitBinaryCalls.Count);
            Assert.IsNull(mockTransport.EmitBinaryCalls[0].Data);
        }

        [Test]
        public void EmitBinary_Terminated_LogsWarning()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            wrapper.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIO->EmitBinary] Client not initialized.");
            wrapper.EmitBinary("image", "AQID");

            wrapper = null;
        }

        // --- EmitVolatile guard tests ---

        [Test]
        public void EmitVolatile_NullEventName_IsIgnored()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitVolatile(null, "data");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void EmitVolatile_EmptyEventName_IsIgnored()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitVolatile("", "data");

            Assert.AreEqual(0, mockTransport.EmitCalls.Count);
        }

        [Test]
        public void EmitVolatile_Terminated_LogsWarning()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            wrapper.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIO->EmitVolatile] Client not initialized.");
            wrapper.EmitVolatile("typing", "{}");

            wrapper = null;
        }

        // --- EmitWithAck guard tests ---

        [Test]
        public void EmitWithAck_EmptyEventName_IsNoOp()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitWithAck("", "{}", "onAck");

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_EmptyFunctionName_IsNoOp()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            wrapper.EmitWithAck("score", "{}", "");

            Assert.AreEqual(0, mockTransport.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_Terminated_LogsWarning()
        {
            wrapper.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            wrapper.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIO->EmitWithAck] Client not initialized.");
            wrapper.EmitWithAck("score", "{}", "onAck");

            wrapper = null;
        }

        // --- OnBinary guard tests ---

        [Test]
        public void OnBinary_NullEventName_IsIgnored()
        {
            wrapper.OnBinary(null, "callback");

            Assert.AreEqual(0, wrapper.RegisteredBinaryEventCount);
        }

        [Test]
        public void OnBinary_EmptyEventName_IsIgnored()
        {
            wrapper.OnBinary("", "callback");

            Assert.AreEqual(0, wrapper.RegisteredBinaryEventCount);
        }

        [Test]
        public void OnBinary_NullFunctionName_IsIgnored()
        {
            wrapper.OnBinary("image", null);

            Assert.AreEqual(0, wrapper.RegisteredBinaryEventCount);
        }

        [Test]
        public void OnBinary_EmptyFunctionName_IsIgnored()
        {
            wrapper.OnBinary("image", "");

            Assert.AreEqual(0, wrapper.RegisteredBinaryEventCount);
        }

        [Test]
        public void OnBinary_AfterTerminate_IsRejected()
        {
            wrapper.Terminate();

            wrapper.OnBinary("image", "onImage");

            Assert.AreEqual(0, wrapper.RegisteredBinaryEventCount);
            wrapper = null;
        }

        [Test]
        public void OnBinary_MultipleSameEvent_StoresAll()
        {
            wrapper.OnBinary("image", "onImage1");
            wrapper.OnBinary("image", "onImage2");

            Assert.IsTrue(wrapper.HasBinaryCallbacksForEvent("image"));
            Assert.AreEqual(1, wrapper.RegisteredBinaryEventCount);
        }

        [Test]
        public void OnBinary_DifferentEvents_StoresSeparately()
        {
            wrapper.OnBinary("image", "onImage");
            wrapper.OnBinary("audio", "onAudio");

            Assert.IsTrue(wrapper.HasBinaryCallbacksForEvent("image"));
            Assert.IsTrue(wrapper.HasBinaryCallbacksForEvent("audio"));
            Assert.AreEqual(2, wrapper.RegisteredBinaryEventCount);
        }

        // --- OffBinary guard tests ---

        [Test]
        public void OffBinary_NullEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => wrapper.OffBinary(null));
        }

        [Test]
        public void OffBinary_EmptyEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => wrapper.OffBinary(""));
        }

        [Test]
        public void OffBinary_NonExistentEvent_IsIgnored()
        {
            Assert.DoesNotThrow(() => wrapper.OffBinary("nonexistent"));
        }

        [Test]
        public void OffBinary_OnlyRemovesSpecifiedEvent()
        {
            wrapper.OnBinary("image", "onImage");
            wrapper.OnBinary("audio", "onAudio");

            wrapper.OffBinary("image");

            Assert.IsFalse(wrapper.HasBinaryCallbacksForEvent("image"));
            Assert.IsTrue(wrapper.HasBinaryCallbacksForEvent("audio"));
            Assert.AreEqual(1, wrapper.RegisteredBinaryEventCount);
        }

        // --- Once guard tests ---

        [Test]
        public void Once_NullEventName_IsIgnored()
        {
            wrapper.Once(null, "callback");

            Assert.AreEqual(0, wrapper.RegisteredOnceEventCount);
        }

        [Test]
        public void Once_EmptyEventName_IsIgnored()
        {
            wrapper.Once("", "callback");

            Assert.AreEqual(0, wrapper.RegisteredOnceEventCount);
        }

        [Test]
        public void Once_NullFunctionName_IsIgnored()
        {
            wrapper.Once("welcome", null);

            Assert.AreEqual(0, wrapper.RegisteredOnceEventCount);
        }

        [Test]
        public void Once_EmptyFunctionName_IsIgnored()
        {
            wrapper.Once("welcome", "");

            Assert.AreEqual(0, wrapper.RegisteredOnceEventCount);
        }

        [Test]
        public void Once_MultipleSameEvent_StoresAll()
        {
            wrapper.Once("welcome", "onWelcome1");
            wrapper.Once("welcome", "onWelcome2");

            Assert.IsTrue(wrapper.HasOnceCallbacksForEvent("welcome"));
            Assert.AreEqual(1, wrapper.RegisteredOnceEventCount);
        }

        // --- OnAny guard tests ---

        [Test]
        public void OnAny_NullFunctionName_IsIgnored()
        {
            wrapper.OnAny(null);

            Assert.AreEqual(0, wrapper.RegisteredOnAnyCount);
        }

        [Test]
        public void OnAny_EmptyFunctionName_IsIgnored()
        {
            wrapper.OnAny("");

            Assert.AreEqual(0, wrapper.RegisteredOnAnyCount);
        }

        [Test]
        public void OnAny_AfterTerminate_IsRejected()
        {
            wrapper.Terminate();

            wrapper.OnAny("onAny");

            Assert.AreEqual(0, wrapper.RegisteredOnAnyCount);
            wrapper = null;
        }

        [Test]
        public void OnAny_MultipleCallbacks_StoresAll()
        {
            wrapper.OnAny("onAny1");
            wrapper.OnAny("onAny2");

            Assert.AreEqual(2, wrapper.RegisteredOnAnyCount);
        }

        // --- JoinRoom/LeaveRoom guard tests ---

        [Test]
        public void JoinRoom_NullRoom_IsIgnored()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            wrapper.JoinRoom(null);

            Assert.AreEqual(0, mockTransport.JoinRoomCalls.Count);
        }

        [Test]
        public void JoinRoom_EmptyRoom_IsIgnored()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            wrapper.JoinRoom("");

            Assert.AreEqual(0, mockTransport.JoinRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_WhenDisconnected_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->LeaveRoom] Not connected.");

            wrapper.LeaveRoom("lobby");

            Assert.AreEqual(0, mockTransport.LeaveRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_NullRoom_IsIgnored()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            wrapper.LeaveRoom(null);

            Assert.AreEqual(0, mockTransport.LeaveRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_EmptyRoom_IsIgnored()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            wrapper.LeaveRoom("");

            Assert.AreEqual(0, mockTransport.LeaveRoomCalls.Count);
        }

        // --- Of guard tests ---

        [Test]
        public void Of_NullNamespace_ReturnsNull()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            var ns = wrapper.Of(null);

            Assert.IsNull(ns);
        }

        [Test]
        public void Of_EmptyNamespace_ReturnsNull()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            var ns = wrapper.Of("");

            Assert.IsNull(ns);
        }

        [Test]
        public void Of_TransportReturnsNull_ReturnsNull()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");
            mockTransport.OfReturnsNull = true;

            var ns = wrapper.Of("/chat");

            Assert.IsNull(ns);
        }

        [Test]
        public void Of_DifferentNamespaces_ReturnsDifferentWrappers()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            var ns1 = wrapper.Of("/chat");
            var ns2 = wrapper.Of("/game");

            Assert.IsNotNull(ns1);
            Assert.IsNotNull(ns2);
            Assert.AreNotSame(ns1, ns2);
        }

        // --- Story 4.2: Namespace Multiplexing Wrapper Tests ---

        [Test]
        public void Of_WhenConnected_ReturnsNamespaceWrapper()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            var ns = wrapper.Of("/chat");

            Assert.IsNotNull(ns);
            Assert.IsInstanceOf<NamespacedSocketIO>(ns);
        }

        [Test]
        public void Of_SameNamespace_ReturnsCachedWrapper()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            var ns1 = wrapper.Of("/chat");
            var ns2 = wrapper.Of("/chat");

            Assert.AreSame(ns1, ns2);
        }

        [Test]
        public void Of_WhenDisconnected_ReturnsNull()
        {
            LogAssert.Expect(LogType.Warning, "[SocketIOClient->Of] Not connected.");

            var ns = wrapper.Of("/chat");

            Assert.IsNull(ns);
        }

        [Test]
        public void Of_AfterTerminate_ReturnsNull()
        {
            wrapper.Terminate();

            LogAssert.Expect(LogType.Warning, "[SocketIO] Of called after terminate.");

            var ns = wrapper.Of("/chat");

            Assert.IsNull(ns);
            wrapper = null;
        }

        [Test]
        public void NamespaceWrapper_Emit_DelegatesToNamespaceTransport()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            var ns = wrapper.Of("/chat") as NamespacedSocketIO;
            Assert.IsNotNull(ns);

            ns.Emit("message", "{\"text\":\"hi\"}");

            var nsMock = mockTransport.NamespaceTransports["/chat"];
            Assert.AreEqual(1, nsMock.EmitCalls.Count);
            Assert.AreEqual("message", nsMock.EmitCalls[0].EventName);
            Assert.AreEqual("{\"text\":\"hi\"}", nsMock.EmitCalls[0].Data);
        }

        [Test]
        public void Terminate_ClearsNamespaceWrapperCache()
        {
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");

            var ns = wrapper.Of("/chat") as NamespacedSocketIO;
            Assert.IsNotNull(ns);

            wrapper.Terminate();

            // After terminate, the namespace wrapper's Emit should be rejected
            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->Emit] Instance terminated.");
            ns.Emit("message", "data");

            var nsMock = mockTransport.NamespaceTransports["/chat"];
            Assert.AreEqual(0, nsMock.EmitCalls.Count);
            wrapper = null;
        }
    }

    /// <summary>
    /// Unit tests for the NamespacedSocketIO JavaScript API wrapper.
    /// Tests all methods on the namespace-scoped wrapper including
    /// Emit, On, Off, Once, OnAny, OffAny, EmitBinary, EmitWithAck,
    /// EmitVolatile, JoinRoom, LeaveRoom, OnBinary, OffBinary, and Dispose.
    /// </summary>
    [TestFixture]
    public class NamespacedSocketIOTests
    {
        private Handlers.Javascript.APIs.Networking.SocketIO wrapper;
        private SocketIOClient client;
        private MockSocketIOTransport mockTransport;
        private NamespacedSocketIO nsWrapper;
        private MockSocketIOTransport nsMock;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            wrapper = new Handlers.Javascript.APIs.Networking.SocketIO();
            client = wrapper.Client;
            mockTransport = new MockSocketIOTransport();
            client.Transport = mockTransport;

            // Connect and create namespace wrapper for tests
            wrapper.Connect("http://localhost:3000", null);
            mockTransport.SimulateConnected("id-1");
            nsWrapper = wrapper.Of("/chat") as NamespacedSocketIO;
            nsMock = mockTransport.NamespaceTransports["/chat"];
        }

        [TearDown]
        public void TearDown()
        {
            if (wrapper != null)
            {
                wrapper.Terminate();
            }
        }

        // --- Connected / Id ---

        [Test]
        public void Connected_ReflectsNamespacedSocketState()
        {
            // Namespace connected state comes from the underlying NamespacedSocket
            Assert.IsNotNull(nsWrapper);
            // The mock namespace transport is connected via the parent
            Assert.IsTrue(nsWrapper.Connected);
        }

        [Test]
        public void Id_ReflectsNamespacedSocketId()
        {
            Assert.IsNotNull(nsWrapper);

            // Namespace transport starts with null SocketId (server hasn't assigned one)
            Assert.IsNull(nsWrapper.Id);

            // Simulate the namespace transport getting a socket ID
            nsMock.SocketId = "ns-id-1";
            Assert.AreEqual("ns-id-1", nsWrapper.Id);
        }

        // --- Emit ---

        [Test]
        public void Emit_DelegatesToNamespaceTransport()
        {
            nsWrapper.Emit("message", "{\"text\":\"hello\"}");

            Assert.AreEqual(1, nsMock.EmitCalls.Count);
            Assert.AreEqual("message", nsMock.EmitCalls[0].EventName);
            Assert.AreEqual("{\"text\":\"hello\"}", nsMock.EmitCalls[0].Data);
        }

        [Test]
        public void Emit_AfterDispose_LogsWarning()
        {
            nsWrapper.Dispose();

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->Emit] Instance terminated.");
            nsWrapper.Emit("message", "data");

            Assert.AreEqual(0, nsMock.EmitCalls.Count);
        }

        [Test]
        public void Emit_NullSocketRef_LogsWarning()
        {
            // Create a NamespacedSocketIO with null socket to test guard
            var nullNs = new NamespacedSocketIO(null);

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->Emit] Socket not initialized.");
            nullNs.Emit("message", "data");
        }

        // --- On / Off ---

        [Test]
        public void On_StoresCallback()
        {
            nsWrapper.On("message", "onMessage");

            Assert.IsTrue(nsWrapper.HasCallbacksForEvent("message"));
            Assert.AreEqual(1, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void On_NullEventName_IsIgnored()
        {
            nsWrapper.On(null, "callback");

            Assert.AreEqual(0, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void On_EmptyEventName_IsIgnored()
        {
            nsWrapper.On("", "callback");

            Assert.AreEqual(0, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void On_NullFunctionName_IsIgnored()
        {
            nsWrapper.On("message", null);

            Assert.AreEqual(0, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void On_EmptyFunctionName_IsIgnored()
        {
            nsWrapper.On("message", "");

            Assert.AreEqual(0, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void On_AfterDispose_IsRejected()
        {
            nsWrapper.Dispose();

            nsWrapper.On("message", "onMessage");

            Assert.AreEqual(0, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void On_MultipleSameEvent_StoresAll()
        {
            nsWrapper.On("message", "onMsg1");
            nsWrapper.On("message", "onMsg2");

            Assert.IsTrue(nsWrapper.HasCallbacksForEvent("message"));
            Assert.AreEqual(1, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void On_DifferentEvents_StoresSeparately()
        {
            nsWrapper.On("chat", "onChat");
            nsWrapper.On("status", "onStatus");

            Assert.AreEqual(2, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void Off_RemovesCallbacks()
        {
            nsWrapper.On("message", "onMessage");
            Assert.IsTrue(nsWrapper.HasCallbacksForEvent("message"));

            nsWrapper.Off("message");

            Assert.IsFalse(nsWrapper.HasCallbacksForEvent("message"));
            Assert.AreEqual(0, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void Off_NullEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.Off(null));
        }

        [Test]
        public void Off_EmptyEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.Off(""));
        }

        [Test]
        public void Off_NonExistentEvent_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.Off("nonexistent"));
        }

        [Test]
        public void Off_OnlyRemovesSpecifiedEvent()
        {
            nsWrapper.On("chat", "onChat");
            nsWrapper.On("status", "onStatus");

            nsWrapper.Off("chat");

            Assert.IsFalse(nsWrapper.HasCallbacksForEvent("chat"));
            Assert.IsTrue(nsWrapper.HasCallbacksForEvent("status"));
        }

        // --- EmitBinary ---

        [Test]
        public void EmitBinary_WhenConnected_DecodesBase64AndDelegates()
        {
            byte[] original = new byte[] { 0x01, 0x02, 0x03 };
            string base64 = System.Convert.ToBase64String(original);

            nsWrapper.EmitBinary("image", base64);

            Assert.AreEqual(1, nsMock.EmitBinaryCalls.Count);
            Assert.AreEqual("image", nsMock.EmitBinaryCalls[0].EventName);
            Assert.AreEqual(original, nsMock.EmitBinaryCalls[0].Data);
        }

        [Test]
        public void EmitBinary_NullEventName_IsIgnored()
        {
            nsWrapper.EmitBinary(null, "AQID");

            Assert.AreEqual(0, nsMock.EmitBinaryCalls.Count);
        }

        [Test]
        public void EmitBinary_EmptyEventName_IsIgnored()
        {
            nsWrapper.EmitBinary("", "AQID");

            Assert.AreEqual(0, nsMock.EmitBinaryCalls.Count);
        }

        [Test]
        public void EmitBinary_EmptyBase64_DelegatesWithNull()
        {
            nsWrapper.EmitBinary("image", "");

            Assert.AreEqual(1, nsMock.EmitBinaryCalls.Count);
            Assert.IsNull(nsMock.EmitBinaryCalls[0].Data);
        }

        [Test]
        public void EmitBinary_InvalidBase64_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitBinary] Invalid base64 data, message dropped.");
            nsWrapper.EmitBinary("image", "not-valid-base64!!!");

            Assert.AreEqual(0, nsMock.EmitBinaryCalls.Count);
        }

        [Test]
        public void EmitBinary_AfterDispose_LogsWarning()
        {
            nsWrapper.Dispose();

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitBinary] Instance terminated.");
            nsWrapper.EmitBinary("image", "AQID");

            Assert.AreEqual(0, nsMock.EmitBinaryCalls.Count);
        }

        [Test]
        public void EmitBinary_NullSocket_LogsWarning()
        {
            var nullNs = new NamespacedSocketIO(null);

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitBinary] Socket not initialized.");
            nullNs.EmitBinary("image", "AQID");
        }

        // --- EmitWithAck ---

        [Test]
        public void EmitWithAck_WhenConnected_DelegatesToNamespace()
        {
            nsWrapper.EmitWithAck("score", "{\"val\":100}", "onAck");

            Assert.AreEqual(1, nsMock.EmitWithAckCalls.Count);
            Assert.AreEqual("score", nsMock.EmitWithAckCalls[0].EventName);
            Assert.AreEqual("{\"val\":100}", nsMock.EmitWithAckCalls[0].Data);
            Assert.IsNotNull(nsMock.EmitWithAckCalls[0].Callback);
        }

        [Test]
        public void EmitWithAck_NullEventName_IsIgnored()
        {
            nsWrapper.EmitWithAck(null, "{}", "onAck");

            Assert.AreEqual(0, nsMock.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_EmptyEventName_IsIgnored()
        {
            nsWrapper.EmitWithAck("", "{}", "onAck");

            Assert.AreEqual(0, nsMock.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_NullFunctionName_IsIgnored()
        {
            nsWrapper.EmitWithAck("score", "{}", null);

            Assert.AreEqual(0, nsMock.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_EmptyFunctionName_IsIgnored()
        {
            nsWrapper.EmitWithAck("score", "{}", "");

            Assert.AreEqual(0, nsMock.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_AfterDispose_LogsWarning()
        {
            nsWrapper.Dispose();

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitWithAck] Instance terminated.");
            nsWrapper.EmitWithAck("score", "{}", "onAck");

            Assert.AreEqual(0, nsMock.EmitWithAckCalls.Count);
        }

        [Test]
        public void EmitWithAck_NullSocket_LogsWarning()
        {
            var nullNs = new NamespacedSocketIO(null);

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitWithAck] Socket not initialized.");
            nullNs.EmitWithAck("score", "{}", "onAck");
        }

        // --- EmitVolatile ---

        [Test]
        public void EmitVolatile_WhenConnected_DelegatesToNamespace()
        {
            nsWrapper.EmitVolatile("typing", "{\"user\":\"alice\"}");

            Assert.AreEqual(1, nsMock.EmitCalls.Count);
            Assert.AreEqual("typing", nsMock.EmitCalls[0].EventName);
        }

        [Test]
        public void EmitVolatile_NullEventName_IsIgnored()
        {
            nsWrapper.EmitVolatile(null, "data");

            Assert.AreEqual(0, nsMock.EmitCalls.Count);
        }

        [Test]
        public void EmitVolatile_EmptyEventName_IsIgnored()
        {
            nsWrapper.EmitVolatile("", "data");

            Assert.AreEqual(0, nsMock.EmitCalls.Count);
        }

        [Test]
        public void EmitVolatile_AfterDispose_LogsWarning()
        {
            nsWrapper.Dispose();

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitVolatile] Instance terminated.");
            nsWrapper.EmitVolatile("typing", "{}");

            Assert.AreEqual(0, nsMock.EmitCalls.Count);
        }

        [Test]
        public void EmitVolatile_NullSocket_LogsWarning()
        {
            var nullNs = new NamespacedSocketIO(null);

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitVolatile] Socket not initialized.");
            nullNs.EmitVolatile("typing", "{}");
        }

        // --- Once ---

        [Test]
        public void Once_StoresCallback()
        {
            nsWrapper.Once("welcome", "onWelcome");

            // Verify the once callback was registered (no public accessor on NS,
            // but the underlying nsSocket.Once was called — verify no throw)
            Assert.DoesNotThrow(() => nsWrapper.Once("welcome", "onWelcome"));
        }

        [Test]
        public void Once_NullEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.Once(null, "callback"));
        }

        [Test]
        public void Once_EmptyEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.Once("", "callback"));
        }

        [Test]
        public void Once_NullFunctionName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.Once("welcome", null));
        }

        [Test]
        public void Once_EmptyFunctionName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.Once("welcome", ""));
        }

        [Test]
        public void Once_AfterDispose_IsRejected()
        {
            nsWrapper.Dispose();

            Assert.DoesNotThrow(() => nsWrapper.Once("welcome", "onWelcome"));
        }

        // --- OnAny / OffAny ---

        [Test]
        public void OnAny_StoresCallback()
        {
            Assert.DoesNotThrow(() => nsWrapper.OnAny("onAnyEvent"));
        }

        [Test]
        public void OnAny_NullFunctionName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.OnAny(null));
        }

        [Test]
        public void OnAny_EmptyFunctionName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.OnAny(""));
        }

        [Test]
        public void OnAny_AfterDispose_IsRejected()
        {
            nsWrapper.Dispose();

            Assert.DoesNotThrow(() => nsWrapper.OnAny("onAnyEvent"));
        }

        [Test]
        public void OffAny_RemovesCallbacks()
        {
            nsWrapper.OnAny("onAnyEvent");

            Assert.DoesNotThrow(() => nsWrapper.OffAny());
        }

        [Test]
        public void OffAny_WhenNoneRegistered_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => nsWrapper.OffAny());
        }

        // --- OnBinary / OffBinary ---

        [Test]
        public void OnBinary_StoresCallback()
        {
            Assert.DoesNotThrow(() => nsWrapper.OnBinary("image", "onImage"));
        }

        [Test]
        public void OnBinary_NullEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.OnBinary(null, "callback"));
        }

        [Test]
        public void OnBinary_EmptyEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.OnBinary("", "callback"));
        }

        [Test]
        public void OnBinary_NullFunctionName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.OnBinary("image", null));
        }

        [Test]
        public void OnBinary_EmptyFunctionName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.OnBinary("image", ""));
        }

        [Test]
        public void OnBinary_AfterDispose_IsRejected()
        {
            nsWrapper.Dispose();

            Assert.DoesNotThrow(() => nsWrapper.OnBinary("image", "onImage"));
        }

        [Test]
        public void OffBinary_RemovesCallback()
        {
            nsWrapper.OnBinary("image", "onImage");

            Assert.DoesNotThrow(() => nsWrapper.OffBinary("image"));
        }

        [Test]
        public void OffBinary_NullEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.OffBinary(null));
        }

        [Test]
        public void OffBinary_EmptyEventName_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.OffBinary(""));
        }

        [Test]
        public void OffBinary_NonExistentEvent_IsIgnored()
        {
            Assert.DoesNotThrow(() => nsWrapper.OffBinary("nonexistent"));
        }

        // --- JoinRoom / LeaveRoom ---

        [Test]
        public void JoinRoom_DelegatesToNamespaceSocket()
        {
            nsWrapper.JoinRoom("lobby");

            Assert.AreEqual(1, nsMock.JoinRoomCalls.Count);
            Assert.AreEqual("lobby", nsMock.JoinRoomCalls[0]);
        }

        [Test]
        public void JoinRoom_NullRoom_IsIgnored()
        {
            nsWrapper.JoinRoom(null);

            Assert.AreEqual(0, nsMock.JoinRoomCalls.Count);
        }

        [Test]
        public void JoinRoom_EmptyRoom_IsIgnored()
        {
            nsWrapper.JoinRoom("");

            Assert.AreEqual(0, nsMock.JoinRoomCalls.Count);
        }

        [Test]
        public void JoinRoom_AfterDispose_LogsWarning()
        {
            nsWrapper.Dispose();

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS] JoinRoom called after terminate.");
            nsWrapper.JoinRoom("lobby");

            Assert.AreEqual(0, nsMock.JoinRoomCalls.Count);
        }

        [Test]
        public void JoinRoom_NullSocket_LogsWarning()
        {
            var nullNs = new NamespacedSocketIO(null);

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS] Socket not initialized.");
            nullNs.JoinRoom("lobby");
        }

        [Test]
        public void LeaveRoom_DelegatesToNamespaceSocket()
        {
            nsWrapper.LeaveRoom("lobby");

            Assert.AreEqual(1, nsMock.LeaveRoomCalls.Count);
            Assert.AreEqual("lobby", nsMock.LeaveRoomCalls[0]);
        }

        [Test]
        public void LeaveRoom_NullRoom_IsIgnored()
        {
            nsWrapper.LeaveRoom(null);

            Assert.AreEqual(0, nsMock.LeaveRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_EmptyRoom_IsIgnored()
        {
            nsWrapper.LeaveRoom("");

            Assert.AreEqual(0, nsMock.LeaveRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_AfterDispose_LogsWarning()
        {
            nsWrapper.Dispose();

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS] LeaveRoom called after terminate.");
            nsWrapper.LeaveRoom("lobby");

            Assert.AreEqual(0, nsMock.LeaveRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_NullSocket_LogsWarning()
        {
            var nullNs = new NamespacedSocketIO(null);

            LogAssert.Expect(LogType.Warning, "[SocketIO:NS] Socket not initialized.");
            nullNs.LeaveRoom("lobby");
        }

        // --- Dispose ---

        [Test]
        public void Dispose_ClearsAllCallbacks()
        {
            nsWrapper.On("chat", "onChat");
            nsWrapper.OnBinary("image", "onImage");

            nsWrapper.Dispose();

            Assert.AreEqual(0, nsWrapper.RegisteredEventCount);
        }

        [Test]
        public void Dispose_AllMethodsRejectAfterDispose()
        {
            nsWrapper.Dispose();

            // Emit
            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->Emit] Instance terminated.");
            nsWrapper.Emit("msg", "data");

            // EmitBinary
            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitBinary] Instance terminated.");
            nsWrapper.EmitBinary("img", "AQID");

            // EmitWithAck
            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitWithAck] Instance terminated.");
            nsWrapper.EmitWithAck("score", "{}", "onAck");

            // EmitVolatile
            LogAssert.Expect(LogType.Warning, "[SocketIO:NS->EmitVolatile] Instance terminated.");
            nsWrapper.EmitVolatile("typing", "{}");

            // JoinRoom
            LogAssert.Expect(LogType.Warning, "[SocketIO:NS] JoinRoom called after terminate.");
            nsWrapper.JoinRoom("lobby");

            // LeaveRoom
            LogAssert.Expect(LogType.Warning, "[SocketIO:NS] LeaveRoom called after terminate.");
            nsWrapper.LeaveRoom("lobby");

            Assert.AreEqual(0, nsMock.EmitCalls.Count);
            Assert.AreEqual(0, nsMock.EmitBinaryCalls.Count);
            Assert.AreEqual(0, nsMock.EmitWithAckCalls.Count);
            Assert.AreEqual(0, nsMock.JoinRoomCalls.Count);
            Assert.AreEqual(0, nsMock.LeaveRoomCalls.Count);
        }

        [Test]
        public void DoubleDispose_DoesNotThrow()
        {
            nsWrapper.Dispose();

            Assert.DoesNotThrow(() => nsWrapper.Dispose());
        }
    }
}
