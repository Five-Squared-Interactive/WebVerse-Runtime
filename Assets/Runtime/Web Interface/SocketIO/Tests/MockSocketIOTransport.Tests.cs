// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using NUnit.Framework;
using FiveSQD.WebVerse.WebInterface.SocketIO;
using FiveSQD.WebVerse.WebInterface.SocketIO.Tests.Mocks;

namespace FiveSQD.WebVerse.WebInterface.SocketIO.Tests
{
    /// <summary>
    /// Tests for MockSocketIOTransport verifying call recording and event simulation.
    /// </summary>
    [TestFixture]
    public class MockSocketIOTransportTests
    {
        private MockSocketIOTransport mock;

        [SetUp]
        public void SetUp()
        {
            mock = new MockSocketIOTransport();
        }

        // --- Call Recording Tests ---

        [Test]
        public void Connect_RecordsCallWithArguments()
        {
            var options = new SocketIOOptions();
            mock.Connect("http://localhost:3000", options);

            Assert.AreEqual(1, mock.ConnectCalls.Count);
            Assert.AreEqual("http://localhost:3000", mock.ConnectCalls[0].Url);
            Assert.AreSame(options, mock.ConnectCalls[0].Options);
        }

        [Test]
        public void Disconnect_RecordsCallCount()
        {
            mock.Disconnect();
            mock.Disconnect();

            Assert.AreEqual(2, mock.DisconnectCallCount);
        }

        [Test]
        public void Emit_RecordsCallWithArguments()
        {
            mock.Emit("chat", "{\"msg\":\"hello\"}");

            Assert.AreEqual(1, mock.EmitCalls.Count);
            Assert.AreEqual("chat", mock.EmitCalls[0].EventName);
            Assert.AreEqual("{\"msg\":\"hello\"}", mock.EmitCalls[0].Data);
        }

        [Test]
        public void EmitBinary_RecordsCallWithArguments()
        {
            var data = new byte[] { 1, 2, 3 };
            mock.EmitBinary("image", data);

            Assert.AreEqual(1, mock.EmitBinaryCalls.Count);
            Assert.AreEqual("image", mock.EmitBinaryCalls[0].EventName);
            Assert.AreSame(data, mock.EmitBinaryCalls[0].Data);
        }

        [Test]
        public void EmitWithAck_RecordsCallWithArguments()
        {
            Action<string> callback = (response) => { };
            mock.EmitWithAck("score", "{\"points\":100}", callback);

            Assert.AreEqual(1, mock.EmitWithAckCalls.Count);
            Assert.AreEqual("score", mock.EmitWithAckCalls[0].EventName);
            Assert.AreEqual("{\"points\":100}", mock.EmitWithAckCalls[0].Data);
            Assert.AreSame(callback, mock.EmitWithAckCalls[0].Callback);
        }

        [Test]
        public void JoinRoom_RecordsCallWithRoom()
        {
            mock.JoinRoom("lobby");

            Assert.AreEqual(1, mock.JoinRoomCalls.Count);
            Assert.AreEqual("lobby", mock.JoinRoomCalls[0]);
        }

        [Test]
        public void LeaveRoom_RecordsCallWithRoom()
        {
            mock.LeaveRoom("lobby");

            Assert.AreEqual(1, mock.LeaveRoomCalls.Count);
            Assert.AreEqual("lobby", mock.LeaveRoomCalls[0]);
        }

        [Test]
        public void Of_RecordsCallAndReturnsNewTransport()
        {
            var result = mock.Of("/chat");

            Assert.AreEqual(1, mock.OfCalls.Count);
            Assert.AreEqual("/chat", mock.OfCalls[0]);
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<MockSocketIOTransport>(result);
        }

        // --- Controllable Properties Tests ---

        [Test]
        public void IsConnected_DefaultsFalse()
        {
            Assert.IsFalse(mock.IsConnected);
        }

        [Test]
        public void IsConnected_CanBeSetDirectly()
        {
            mock.IsConnected = true;
            Assert.IsTrue(mock.IsConnected);
        }

        [Test]
        public void SocketId_DefaultsNull()
        {
            Assert.IsNull(mock.SocketId);
        }

        [Test]
        public void SocketId_CanBeSetDirectly()
        {
            mock.SocketId = "test-id-123";
            Assert.AreEqual("test-id-123", mock.SocketId);
        }

        // --- Event Simulation Tests ---

        [Test]
        public void SimulateConnected_FiresOnConnectedWithSocketId()
        {
            string receivedId = null;
            mock.OnConnected += (id) => receivedId = id;

            mock.SimulateConnected("abc-123");

            Assert.AreEqual("abc-123", receivedId);
            Assert.IsTrue(mock.IsConnected);
            Assert.AreEqual("abc-123", mock.SocketId);
        }

        [Test]
        public void SimulateDisconnected_FiresOnDisconnectedWithReason()
        {
            string receivedReason = null;
            mock.OnDisconnected += (reason) => receivedReason = reason;

            mock.SimulateDisconnected("transport close");

            Assert.AreEqual("transport close", receivedReason);
            Assert.IsFalse(mock.IsConnected);
            Assert.IsNull(mock.SocketId);
        }

        [Test]
        public void SimulateConnectError_FiresOnConnectErrorWithMessage()
        {
            string receivedMessage = null;
            mock.OnConnectError += (msg) => receivedMessage = msg;

            mock.SimulateConnectError("connection refused");

            Assert.AreEqual("connection refused", receivedMessage);
            Assert.IsFalse(mock.IsConnected);
        }

        [Test]
        public void SimulateEvent_FiresOnEventWithNameAndData()
        {
            string receivedEvent = null;
            string receivedData = null;
            mock.OnEvent += (name, data) => { receivedEvent = name; receivedData = data; };

            mock.SimulateEvent("message", "{\"text\":\"hi\"}");

            Assert.AreEqual("message", receivedEvent);
            Assert.AreEqual("{\"text\":\"hi\"}", receivedData);
        }

        [Test]
        public void SimulateBinaryEvent_FiresOnBinaryEventWithNameAndData()
        {
            string receivedEvent = null;
            byte[] receivedData = null;
            mock.OnBinaryEvent += (name, data) => { receivedEvent = name; receivedData = data; };

            var binaryData = new byte[] { 0xFF, 0x00, 0xAB };
            mock.SimulateBinaryEvent("file", binaryData);

            Assert.AreEqual("file", receivedEvent);
            Assert.AreSame(binaryData, receivedData);
        }

        [Test]
        public void SimulateReconnectAttempt_FiresOnReconnectAttemptWithNumber()
        {
            int receivedAttempt = 0;
            mock.OnReconnectAttempt += (num) => receivedAttempt = num;

            mock.SimulateReconnectAttempt(3);

            Assert.AreEqual(3, receivedAttempt);
        }

        [Test]
        public void SimulateReconnected_FiresOnReconnectedWithNumber()
        {
            int receivedAttempt = 0;
            mock.OnReconnected += (num) => receivedAttempt = num;

            mock.SimulateReconnected(2);

            Assert.AreEqual(2, receivedAttempt);
            Assert.IsTrue(mock.IsConnected);
        }

        [Test]
        public void SimulateReconnectFailed_FiresOnReconnectFailed()
        {
            bool fired = false;
            mock.OnReconnectFailed += () => fired = true;

            mock.SimulateReconnectFailed();

            Assert.IsTrue(fired);
            Assert.IsFalse(mock.IsConnected);
        }

        // --- Event Simulation Without Listeners (no crash) ---

        [Test]
        public void SimulateConnected_WithNoListeners_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => mock.SimulateConnected("id"));
        }

        [Test]
        public void SimulateDisconnected_WithNoListeners_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => mock.SimulateDisconnected("reason"));
        }

        [Test]
        public void SimulateConnectError_WithNoListeners_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => mock.SimulateConnectError("error"));
        }

        [Test]
        public void SimulateEvent_WithNoListeners_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => mock.SimulateEvent("test", "data"));
        }

        [Test]
        public void SimulateBinaryEvent_WithNoListeners_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => mock.SimulateBinaryEvent("test", new byte[] { 1 }));
        }

        [Test]
        public void SimulateReconnectFailed_WithNoListeners_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => mock.SimulateReconnectFailed());
        }

        // --- Reset Tests ---

        [Test]
        public void Reset_ClearsAllRecordedCalls()
        {
            mock.Connect("url", new SocketIOOptions());
            mock.Disconnect();
            mock.Emit("event", "data");
            mock.EmitBinary("event", new byte[] { 1 });
            mock.EmitWithAck("event", "data", (r) => { });
            mock.JoinRoom("room");
            mock.LeaveRoom("room");
            mock.Of("/ns");
            mock.IsConnected = true;
            mock.SocketId = "id";

            mock.Reset();

            Assert.AreEqual(0, mock.ConnectCalls.Count);
            Assert.AreEqual(0, mock.DisconnectCallCount);
            Assert.AreEqual(0, mock.EmitCalls.Count);
            Assert.AreEqual(0, mock.EmitBinaryCalls.Count);
            Assert.AreEqual(0, mock.EmitWithAckCalls.Count);
            Assert.AreEqual(0, mock.JoinRoomCalls.Count);
            Assert.AreEqual(0, mock.LeaveRoomCalls.Count);
            Assert.AreEqual(0, mock.OfCalls.Count);
            Assert.IsFalse(mock.IsConnected);
            Assert.IsNull(mock.SocketId);
        }

        // --- Multiple Calls Recording ---

        [Test]
        public void MultipleCalls_AreRecordedInOrder()
        {
            mock.Emit("first", "data1");
            mock.Emit("second", "data2");
            mock.Emit("third", "data3");

            Assert.AreEqual(3, mock.EmitCalls.Count);
            Assert.AreEqual("first", mock.EmitCalls[0].EventName);
            Assert.AreEqual("second", mock.EmitCalls[1].EventName);
            Assert.AreEqual("third", mock.EmitCalls[2].EventName);
        }
    }
}
