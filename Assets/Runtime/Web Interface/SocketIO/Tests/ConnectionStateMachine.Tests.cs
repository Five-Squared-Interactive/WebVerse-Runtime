// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WebInterface.SocketIO;
using FiveSQD.WebVerse.WebInterface.SocketIO.Tests.Mocks;

namespace FiveSQD.WebVerse.WebInterface.SocketIO.Tests
{
    /// <summary>
    /// Tests for the ConnectionState enum.
    /// </summary>
    [TestFixture]
    public class ConnectionStateEnumTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [Test]
        public void ConnectionState_HasExactlyFiveValues()
        {
            var values = Enum.GetValues(typeof(ConnectionState));
            Assert.AreEqual(5, values.Length,
                "ConnectionState should have exactly 5 values.");
        }

        [Test]
        public void ConnectionState_ContainsDisconnected()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Disconnected),
                "ConnectionState should contain Disconnected.");
        }

        [Test]
        public void ConnectionState_ContainsConnecting()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Connecting),
                "ConnectionState should contain Connecting.");
        }

        [Test]
        public void ConnectionState_ContainsConnected()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Connected),
                "ConnectionState should contain Connected.");
        }

        [Test]
        public void ConnectionState_ContainsReconnecting()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Reconnecting),
                "ConnectionState should contain Reconnecting.");
        }

        [Test]
        public void ConnectionState_ContainsDisconnecting()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(ConnectionState), ConnectionState.Disconnecting),
                "ConnectionState should contain Disconnecting.");
        }

        [Test]
        public void ConnectionState_ValuesAreExactly_Disconnected_Connecting_Connected_Reconnecting_Disconnecting()
        {
            var expectedNames = new[] { "Disconnected", "Connecting", "Connected", "Reconnecting", "Disconnecting" };
            var actualNames = Enum.GetNames(typeof(ConnectionState));

            CollectionAssert.AreEquivalent(expectedNames, actualNames,
                "ConnectionState should contain exactly: Disconnected, Connecting, Connected, Reconnecting, Disconnecting.");
        }
    }

    /// <summary>
    /// Tests for state machine transitions exercised through SocketIOClient.
    /// Covers ALL valid transitions and verifies invalid transitions are rejected.
    /// </summary>
    [TestFixture]
    public class ConnectionStateMachineTransitionTests
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
            gameObject = new GameObject("StateMachineTest");
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

        // --- Valid Transitions ---

        [Test]
        public void ValidTransition_Disconnected_To_Connecting()
        {
            Assert.AreEqual(ConnectionState.Disconnected, client.State);

            client.Connect("http://localhost:3000");

            Assert.AreEqual(ConnectionState.Connecting, client.State);
        }

        [Test]
        public void ValidTransition_Connecting_To_Connected()
        {
            client.Connect("http://localhost:3000");
            Assert.AreEqual(ConnectionState.Connecting, client.State);

            mockTransport.SimulateConnected("id-1");

            Assert.AreEqual(ConnectionState.Connected, client.State);
        }

        [Test]
        public void ValidTransition_Connecting_To_Disconnected_OnError()
        {
            client.Connect("http://localhost:3000");
            Assert.AreEqual(ConnectionState.Connecting, client.State);

            LogAssert.Expect(LogType.Error, "[SocketIOClient] Connection failed: refused");
            mockTransport.SimulateConnectError("refused");

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
        }

        [Test]
        public void ValidTransition_Connected_To_Disconnecting()
        {
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            Assert.AreEqual(ConnectionState.Connected, client.State);

            client.Disconnect();

            Assert.AreEqual(ConnectionState.Disconnecting, client.State);
        }

        [Test]
        public void ValidTransition_Disconnecting_To_Disconnected()
        {
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Disconnect();
            Assert.AreEqual(ConnectionState.Disconnecting, client.State);

            mockTransport.SimulateDisconnected("io client disconnect");

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
        }

        // --- Invalid Transitions (rejected silently) ---

        [Test]
        public void InvalidTransition_Connected_To_Connecting_Rejected()
        {
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");

            client.Connect("http://localhost:4000");

            Assert.AreEqual(ConnectionState.Connected, client.State);
            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
        }

        [Test]
        public void InvalidTransition_Connecting_To_Connecting_Rejected()
        {
            client.Connect("http://localhost:3000");

            client.Connect("http://localhost:4000");

            Assert.AreEqual(ConnectionState.Connecting, client.State);
            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
        }

        [Test]
        public void InvalidTransition_Disconnected_To_Disconnecting_Rejected()
        {
            client.Disconnect();

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
            Assert.AreEqual(0, mockTransport.DisconnectCallCount);
        }

        [Test]
        public void InvalidTransition_Disconnecting_To_Connected_NotPossible()
        {
            client.Connect("http://localhost:3000");
            mockTransport.SimulateConnected("id-1");
            client.Disconnect();

            // Attempting Connect while Disconnecting should be rejected
            client.Connect("http://localhost:4000");

            Assert.AreEqual(ConnectionState.Disconnecting, client.State);
            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
        }

        [Test]
        public void InvalidTransition_Reconnecting_To_Connecting_Rejected()
        {
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            client.Connect("http://localhost:4000");

            Assert.AreEqual(ConnectionState.Reconnecting, client.State);
            Assert.AreEqual(1, mockTransport.ConnectCalls.Count);
        }

        [Test]
        public void InvalidTransition_Connecting_To_Disconnecting_Rejected()
        {
            client.Connect("http://localhost:3000");

            // Disconnect from Connecting state should be rejected (not Connected)
            client.Disconnect();

            Assert.AreEqual(ConnectionState.Connecting, client.State);
            Assert.AreEqual(0, mockTransport.DisconnectCallCount);
        }

        // --- Story 2.2: Reconnecting State Transitions ---

        [Test]
        public void ValidTransition_Connected_TransportDisconnect_ReconnectionTrue_ToReconnecting()
        {
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            Assert.AreEqual(ConnectionState.Connected, client.State);

            // Transport-initiated disconnect (NOT client.Disconnect())
            mockTransport.SimulateDisconnected("transport close");

            Assert.AreEqual(ConnectionState.Reconnecting, client.State);
        }

        [Test]
        public void ValidTransition_Connected_TransportDisconnect_ReconnectionFalse_ToDisconnected()
        {
            var opts = new SocketIOOptions { reconnection = false };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            Assert.AreEqual(ConnectionState.Connected, client.State);

            mockTransport.SimulateDisconnected("transport close");

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
        }

        [Test]
        public void ValidTransition_Connected_ClientDisconnect_NeverReconnects()
        {
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");

            client.Disconnect(); // Client-initiated

            Assert.AreEqual(ConnectionState.Disconnecting, client.State); // NOT Reconnecting
        }

        [Test]
        public void ValidTransition_Reconnecting_OnReconnected_ToConnected()
        {
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            mockTransport.SimulateReconnected(2);

            Assert.AreEqual(ConnectionState.Connected, client.State);
        }

        [Test]
        public void ValidTransition_Reconnecting_OnReconnectFailed_ToDisconnected()
        {
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            mockTransport.SimulateReconnectFailed();

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
        }

        [Test]
        public void ValidTransition_Reconnecting_ClientDisconnect_ToDisconnected()
        {
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            mockTransport.SimulateConnected("id-1");
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            client.Disconnect();

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
        }

        // --- Full Lifecycle ---

        [Test]
        public void FullLifecycle_Connect_TransportDisconnect_Reconnect_Connected()
        {
            var opts = new SocketIOOptions { reconnection = true };
            client.Connect("http://localhost:3000", opts);
            Assert.AreEqual(ConnectionState.Connecting, client.State);

            mockTransport.SimulateConnected("id-1");
            Assert.AreEqual(ConnectionState.Connected, client.State);

            // Transport-initiated disconnect
            mockTransport.SimulateDisconnected("transport close");
            Assert.AreEqual(ConnectionState.Reconnecting, client.State);

            // Reconnection attempts
            mockTransport.SimulateReconnectAttempt(1);
            mockTransport.SimulateReconnectAttempt(2);

            // Successful reconnection
            mockTransport.SimulateReconnected(2);
            Assert.AreEqual(ConnectionState.Connected, client.State);
        }

        [Test]
        public void FullLifecycle_Connect_Disconnect_ReconnectSuccessfully()
        {
            // Connect
            client.Connect("http://localhost:3000");
            Assert.AreEqual(ConnectionState.Connecting, client.State);

            mockTransport.SimulateConnected("id-1");
            Assert.AreEqual(ConnectionState.Connected, client.State);

            // Disconnect
            client.Disconnect();
            Assert.AreEqual(ConnectionState.Disconnecting, client.State);

            mockTransport.SimulateDisconnected("io client disconnect");
            Assert.AreEqual(ConnectionState.Disconnected, client.State);

            // Reconnect
            client.Connect("http://localhost:3000");
            Assert.AreEqual(ConnectionState.Connecting, client.State);

            mockTransport.SimulateConnected("id-2");
            Assert.AreEqual(ConnectionState.Connected, client.State);
            Assert.AreEqual("id-2", client.Id);
        }
    }
}
