// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WebInterface.SocketIO;
using FiveSQD.WebVerse.WebInterface.SocketIO.Tests.Mocks;

namespace FiveSQD.WebVerse.WebInterface.SocketIO.Tests
{
    /// <summary>
    /// Unit tests for NamespacedSocket -- event dispatch, callback management,
    /// state guards, and namespace isolation.
    /// </summary>
    [TestFixture]
    public class NamespacedSocketTests
    {
        private GameObject gameObject;
        private SocketIOClient client;
        private MockSocketIOTransport mockTransport;
        private NamespacedSocket nsSocket;
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
            gameObject = new GameObject("NamespacedSocketTest");
            client = gameObject.AddComponent<SocketIOClient>();
            mockTransport = new MockSocketIOTransport();
            client.Initialize();
            client.Transport = mockTransport;
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            nsSocket = client.Of("/test");
            nsMock = mockTransport.NamespaceTransports["/test"];
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

        // --- 4.1: Emit_WhenConnected_DelegatesToTransport ---

        [Test]
        public void Emit_WhenConnected_DelegatesToTransport()
        {
            nsSocket.Emit("chat", "{\"msg\":\"hello\"}");

            Assert.AreEqual(1, nsMock.EmitCalls.Count);
            Assert.AreEqual("chat", nsMock.EmitCalls[0].EventName);
            Assert.AreEqual("{\"msg\":\"hello\"}", nsMock.EmitCalls[0].Data);
        }

        // --- 4.2: Emit_WhenDisconnected_LogsWarning ---

        [Test]
        public void Emit_WhenDisconnected_LogsWarning()
        {
            mockTransport.SimulateDisconnected("transport close");

            LogAssert.Expect(LogType.Warning, "[NamespacedSocket->/test->Emit] Not connected.");

            nsSocket.Emit("chat", "data");

            Assert.AreEqual(0, nsMock.EmitCalls.Count);
        }

        // --- 4.3: On_RegistersCallback_FiresOnEvent ---

        [Test]
        public void On_RegistersCallback_FiresOnEvent()
        {
            string received = null;
            nsSocket.On("message", (data) => received = data);

            nsMock.SimulateEvent("message", "{\"text\":\"hi\"}");

            Assert.AreEqual("{\"text\":\"hi\"}", received);
        }

        // --- 4.4: Off_RemovesCallback ---

        [Test]
        public void Off_RemovesCallback()
        {
            string received = null;
            nsSocket.On("message", (data) => received = data);
            nsSocket.Off("message");

            nsMock.SimulateEvent("message", "data");

            Assert.IsNull(received);
        }

        // --- 4.5: Once_FiresOnceAndRemoves ---

        [Test]
        public void Once_FiresOnceAndRemoves()
        {
            int callCount = 0;
            nsSocket.Once("init", (data) => callCount++);

            nsMock.SimulateEvent("init", "first");
            nsMock.SimulateEvent("init", "second");

            Assert.AreEqual(1, callCount);
        }

        // --- 4.6: OnAny_FiresForEveryEvent ---

        [Test]
        public void OnAny_FiresForEveryEvent()
        {
            int callCount = 0;
            string lastEvent = null;
            string lastData = null;
            nsSocket.OnAny((eventName, data) =>
            {
                callCount++;
                lastEvent = eventName;
                lastData = data;
            });

            nsMock.SimulateEvent("alpha", "a-data");
            nsMock.SimulateEvent("beta", "b-data");

            Assert.AreEqual(2, callCount);
            Assert.AreEqual("beta", lastEvent);
            Assert.AreEqual("b-data", lastData);
        }

        // --- 4.7: OffAny_RemovesCallbacks ---

        [Test]
        public void OffAny_RemovesCallbacks()
        {
            int callCount = 0;
            nsSocket.OnAny((eventName, data) => callCount++);
            nsSocket.OffAny();

            nsMock.SimulateEvent("event", "data");

            Assert.AreEqual(0, callCount);
        }

        // --- 4.8: EmitBinary_WhenConnected_DelegatesToTransport ---

        [Test]
        public void EmitBinary_WhenConnected_DelegatesToTransport()
        {
            byte[] payload = new byte[] { 0x01, 0x02, 0x03 };
            nsSocket.EmitBinary("upload", payload);

            Assert.AreEqual(1, nsMock.EmitBinaryCalls.Count);
            Assert.AreEqual("upload", nsMock.EmitBinaryCalls[0].EventName);
            Assert.AreEqual(payload, nsMock.EmitBinaryCalls[0].Data);
        }

        // --- 4.9: EmitBinary_WhenDisconnected_LogsWarning ---

        [Test]
        public void EmitBinary_WhenDisconnected_LogsWarning()
        {
            mockTransport.SimulateDisconnected("transport close");

            LogAssert.Expect(LogType.Warning, "[NamespacedSocket->/test->EmitBinary] Not connected.");

            nsSocket.EmitBinary("upload", new byte[] { 0x01 });

            Assert.AreEqual(0, nsMock.EmitBinaryCalls.Count);
        }

        // --- 4.10: EmitWithAck_WhenConnected_DelegatesToTransport ---

        [Test]
        public void EmitWithAck_WhenConnected_DelegatesToTransport()
        {
            string ackResult = null;
            nsSocket.EmitWithAck("save", "{\"id\":1}", (data) => ackResult = data);

            Assert.AreEqual(1, nsMock.EmitWithAckCalls.Count);
            Assert.AreEqual("save", nsMock.EmitWithAckCalls[0].EventName);
            Assert.AreEqual("{\"id\":1}", nsMock.EmitWithAckCalls[0].Data);
        }

        [Test]
        public void EmitWithAck_WhenDisconnected_LogsWarning()
        {
            mockTransport.SimulateDisconnected("transport close");

            LogAssert.Expect(LogType.Warning, "[NamespacedSocket->/test->EmitWithAck] Not connected.");

            nsSocket.EmitWithAck("save", "{}", (data) => { });

            Assert.AreEqual(0, nsMock.EmitWithAckCalls.Count);
        }

        // --- 4.11: EmitVolatile_WhenConnected_Delegates ---

        [Test]
        public void EmitVolatile_WhenConnected_Delegates()
        {
            nsSocket.EmitVolatile("cursor", "{\"x\":10}");

            Assert.AreEqual(1, nsMock.EmitCalls.Count);
            Assert.AreEqual("cursor", nsMock.EmitCalls[0].EventName);
        }

        // --- 4.12: EmitVolatile_WhenDisconnected_SilentlyDrops ---

        [Test]
        public void EmitVolatile_WhenDisconnected_SilentlyDrops()
        {
            mockTransport.SimulateDisconnected("transport close");

            // No LogAssert -- volatile silently drops
            nsSocket.EmitVolatile("cursor", "{\"x\":10}");

            Assert.AreEqual(0, nsMock.EmitCalls.Count);
        }

        // --- 4.13: JoinRoom_WhenConnected_DelegatesToTransport ---

        [Test]
        public void JoinRoom_WhenConnected_DelegatesToTransport()
        {
            nsSocket.JoinRoom("lobby");

            Assert.AreEqual(1, nsMock.JoinRoomCalls.Count);
            Assert.AreEqual("lobby", nsMock.JoinRoomCalls[0]);
        }

        // --- 4.14: LeaveRoom_WhenConnected_DelegatesToTransport ---

        [Test]
        public void LeaveRoom_WhenConnected_DelegatesToTransport()
        {
            nsSocket.LeaveRoom("lobby");

            Assert.AreEqual(1, nsMock.LeaveRoomCalls.Count);
            Assert.AreEqual("lobby", nsMock.LeaveRoomCalls[0]);
        }

        [Test]
        public void JoinRoom_WhenDisconnected_LogsWarning()
        {
            mockTransport.SimulateDisconnected("transport close");

            LogAssert.Expect(LogType.Warning, "[NamespacedSocket->/test->JoinRoom] Not connected.");

            nsSocket.JoinRoom("lobby");

            Assert.AreEqual(0, nsMock.JoinRoomCalls.Count);
        }

        [Test]
        public void LeaveRoom_WhenDisconnected_LogsWarning()
        {
            mockTransport.SimulateDisconnected("transport close");

            LogAssert.Expect(LogType.Warning, "[NamespacedSocket->/test->LeaveRoom] Not connected.");

            nsSocket.LeaveRoom("lobby");

            Assert.AreEqual(0, nsMock.LeaveRoomCalls.Count);
        }

        // --- 4.15: OnBinary_RegistersCallback_FiresOnBinaryEvent ---

        [Test]
        public void OnBinary_RegistersCallback_FiresOnBinaryEvent()
        {
            string receivedEvent = null;
            byte[] receivedData = null;
            nsSocket.OnBinary("download", (eventName, data) =>
            {
                receivedEvent = eventName;
                receivedData = data;
            });

            byte[] payload = new byte[] { 0xAA, 0xBB };
            nsMock.SimulateBinaryEvent("download", payload);

            Assert.AreEqual("download", receivedEvent);
            Assert.AreEqual(payload, receivedData);
        }

        // --- 4.16: OffBinary_RemovesCallbacks ---

        [Test]
        public void OffBinary_RemovesCallbacks()
        {
            byte[] receivedData = null;
            nsSocket.OnBinary("download", (eventName, data) => receivedData = data);
            nsSocket.OffBinary("download");

            nsMock.SimulateBinaryEvent("download", new byte[] { 0x01 });

            Assert.IsNull(receivedData);
        }

        // --- 4.17: Dispose_ClearsAllCallbacks ---

        [Test]
        public void Dispose_ClearsAllCallbacks()
        {
            int callCount = 0;
            nsSocket.On("event", (data) => callCount++);
            nsSocket.Once("event", (data) => callCount++);
            nsSocket.OnAny((eventName, data) => callCount++);
            nsSocket.OnBinary("bin", (eventName, data) => callCount++);

            nsSocket.Dispose();

            nsMock.SimulateEvent("event", "data");
            nsMock.SimulateBinaryEvent("bin", new byte[] { 0x01 });

            Assert.AreEqual(0, callCount);
        }

        // --- 4.18: Emit_AfterDispose_IsRejected ---

        [Test]
        public void Emit_AfterDispose_IsRejected()
        {
            nsSocket.Dispose();

            nsSocket.Emit("chat", "data");

            Assert.AreEqual(0, nsMock.EmitCalls.Count);
        }

        // --- 4.19: EventIsolation_TwoNamespaces_OnlyOwnEventsReceived ---

        [Test]
        public void EventIsolation_TwoNamespaces_OnlyOwnEventsReceived()
        {
            var nsChatSocket = client.Of("/chat");
            var nsGameSocket = client.Of("/game");

            var nsChatMock = mockTransport.NamespaceTransports["/chat"];
            var nsGameMock = mockTransport.NamespaceTransports["/game"];

            string chatReceived = null;
            string gameReceived = null;
            nsChatSocket.On("message", (data) => chatReceived = data);
            nsGameSocket.On("message", (data) => gameReceived = data);

            nsChatMock.SimulateEvent("message", "hello-chat");

            Assert.AreEqual("hello-chat", chatReceived);
            Assert.IsNull(gameReceived);
        }

        // --- 4.20: NullOrEmptyEventName_IsNoOp ---

        [Test]
        public void NullOrEmptyEventName_IsNoOp()
        {
            // Emit with null/empty -- silent no-op
            nsSocket.Emit(null, "data");
            nsSocket.Emit("", "data");
            Assert.AreEqual(0, nsMock.EmitCalls.Count);

            // On with null/empty -- silent no-op
            int callCount = 0;
            nsSocket.On(null, (data) => callCount++);
            nsSocket.On("", (data) => callCount++);
            nsMock.SimulateEvent("any", "data");
            Assert.AreEqual(0, callCount);

            // Off with null/empty -- silent no-op (does not throw)
            nsSocket.Off(null);
            nsSocket.Off("");

            // Once with null/empty -- silent no-op
            nsSocket.Once(null, (data) => callCount++);
            nsSocket.Once("", (data) => callCount++);
            nsMock.SimulateEvent("any", "data");
            Assert.AreEqual(0, callCount);

            // EmitBinary with null/empty -- silent no-op
            nsSocket.EmitBinary(null, new byte[] { 0x01 });
            nsSocket.EmitBinary("", new byte[] { 0x01 });
            Assert.AreEqual(0, nsMock.EmitBinaryCalls.Count);

            // EmitWithAck with null/empty -- silent no-op
            nsSocket.EmitWithAck(null, "data", (ack) => { });
            nsSocket.EmitWithAck("", "data", (ack) => { });
            Assert.AreEqual(0, nsMock.EmitWithAckCalls.Count);

            // EmitVolatile with null/empty -- silent no-op
            nsSocket.EmitVolatile(null, "data");
            nsSocket.EmitVolatile("", "data");
            Assert.AreEqual(0, nsMock.EmitCalls.Count);

            // JoinRoom with null/empty -- silent no-op
            nsSocket.JoinRoom(null);
            nsSocket.JoinRoom("");
            Assert.AreEqual(0, nsMock.JoinRoomCalls.Count);

            // LeaveRoom with null/empty -- silent no-op
            nsSocket.LeaveRoom(null);
            nsSocket.LeaveRoom("");
            Assert.AreEqual(0, nsMock.LeaveRoomCalls.Count);
        }

        // --- Properties ---

        [Test]
        public void Connected_ReflectsParentState()
        {
            Assert.IsTrue(nsSocket.Connected);

            mockTransport.SimulateDisconnected("transport close");

            Assert.IsFalse(nsSocket.Connected);
        }

        [Test]
        public void Id_ReturnsTransportSocketId()
        {
            nsMock.SocketId = "ns-socket-id-123";
            Assert.AreEqual("ns-socket-id-123", nsSocket.Id);
        }

        [Test]
        public void Namespace_ReturnsPath()
        {
            Assert.AreEqual("/test", nsSocket.Namespace);
        }
    }
}
