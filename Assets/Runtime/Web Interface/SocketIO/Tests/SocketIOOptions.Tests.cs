// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WebInterface.SocketIO;

namespace FiveSQD.WebVerse.WebInterface.SocketIO.Tests
{
    /// <summary>
    /// Tests for SocketIOOptions default values.
    /// </summary>
    [TestFixture]
    public class SocketIOOptionsTests
    {
        private SocketIOOptions options;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [SetUp]
        public void SetUp()
        {
            options = new SocketIOOptions();
        }

        [Test]
        public void DefaultAuth_IsNull()
        {
            Assert.IsNull(options.auth,
                "Default auth should be null.");
        }

        [Test]
        public void DefaultHeaders_IsNull()
        {
            Assert.IsNull(options.headers,
                "Default headers should be null.");
        }

        [Test]
        public void DefaultQuery_IsNull()
        {
            Assert.IsNull(options.query,
                "Default query should be null.");
        }

        [Test]
        public void DefaultTransport_IsPolling()
        {
            Assert.AreEqual("polling", options.transport,
                "Default transport should be 'polling'.");
        }

        [Test]
        public void DefaultReconnection_IsTrue()
        {
            Assert.IsTrue(options.reconnection,
                "Default reconnection should be true.");
        }

        [Test]
        public void DefaultReconnectionAttempts_Is5()
        {
            Assert.AreEqual(5, options.reconnectionAttempts,
                "Default reconnectionAttempts should be 5.");
        }

        [Test]
        public void DefaultReconnectionDelay_Is1000()
        {
            Assert.AreEqual(1000, options.reconnectionDelay,
                "Default reconnectionDelay should be 1000ms.");
        }

        [Test]
        public void DefaultReconnectionDelayMax_Is5000()
        {
            Assert.AreEqual(5000, options.reconnectionDelayMax,
                "Default reconnectionDelayMax should be 5000ms.");
        }

        [Test]
        public void DefaultTimeout_Is20000()
        {
            Assert.AreEqual(20000, options.timeout,
                "Default timeout should be 20000ms.");
        }

        [Test]
        public void DefaultAckTimeout_Is10000()
        {
            Assert.AreEqual(10000, options.ackTimeout,
                "Default ackTimeout should be 10000ms.");
        }

        [Test]
        public void DefaultQueueSize_Is100()
        {
            Assert.AreEqual(100, options.queueSize,
                "Default queueSize should be 100.");
        }

        [Test]
        public void CustomValues_AreSetCorrectly()
        {
            options.auth = "my-token";
            options.transport = "websocket";
            options.reconnection = false;
            options.reconnectionAttempts = 10;
            options.timeout = 5000;

            Assert.AreEqual("my-token", options.auth);
            Assert.AreEqual("websocket", options.transport);
            Assert.IsFalse(options.reconnection);
            Assert.AreEqual(10, options.reconnectionAttempts);
            Assert.AreEqual(5000, options.timeout);
        }
    }
}
