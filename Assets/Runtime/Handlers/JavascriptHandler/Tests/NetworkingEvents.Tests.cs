// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using Jint;
using Jint.Native;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Networking;

namespace FiveSQD.WebVerse.Handlers.Javascript.Tests
{
    /// <summary>
    /// Tests for networking event system integration.
    /// WebSocket and MQTTClient implement IEventEmitter (instance-based).
    /// HTTPNetworking has static event methods.
    /// Note: WebSocket and MQTTClient are guarded by #if USE_WEBINTERFACE —
    /// these tests compile regardless since IEventEmitter itself has no guards.
    /// </summary>
    [TestFixture]
    public class NetworkingEventsTests
    {
        private Engine _engine;
        private List<string> _results;

        [SetUp]
        public void SetUp()
        {
            _engine = new Engine();
            _results = new List<string>();
            _engine.SetValue("results", _results);
            HTTPNetworking.DisposeAllHTTPListeners();
            ObserverLimits.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            HTTPNetworking.DisposeAllHTTPListeners();
            ObserverLimits.Reset();
        }

        // --- HTTPNetworking Static Events ---

        [Test]
        public void HTTPOnRegistersAndEmitFires()
        {
            LogAssert.ignoreFailingMessages = true;
            var cb = _engine.Evaluate("(function() { results.Add('http-response'); })");
            HTTPNetworking.on("response", cb);
            HTTPNetworking.Emit("response");
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("http-response", _results[0]);
        }

        [Test]
        public void HTTPOnceAutoRemoves()
        {
            LogAssert.ignoreFailingMessages = true;
            var cb = _engine.Evaluate("(function() { results.Add('once'); })");
            HTTPNetworking.once("response", cb);
            HTTPNetworking.Emit("response");
            Assert.AreEqual(1, _results.Count);
            _results.Clear();
            HTTPNetworking.Emit("response");
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void HTTPOffRemovesListener()
        {
            LogAssert.ignoreFailingMessages = true;
            var cb = _engine.Evaluate("(function() { results.Add('fired'); })");
            HTTPNetworking.on("response", cb);
            HTTPNetworking.off("response", cb);
            HTTPNetworking.Emit("response");
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void HTTPDisposeClearsAll()
        {
            LogAssert.ignoreFailingMessages = true;
            var cb = _engine.Evaluate("(function() { results.Add('x'); })");
            HTTPNetworking.on("response", cb);
            HTTPNetworking.on("error", cb);
            HTTPNetworking.DisposeAllHTTPListeners();
            HTTPNetworking.Emit("response");
            HTTPNetworking.Emit("error");
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void HTTPMultipleListenersFireInOrder()
        {
            LogAssert.ignoreFailingMessages = true;
            var cbA = _engine.Evaluate("(function() { results.Add('A'); })");
            var cbB = _engine.Evaluate("(function() { results.Add('B'); })");
            HTTPNetworking.on("response", cbA);
            HTTPNetworking.on("response", cbB);
            HTTPNetworking.Emit("response");
            Assert.AreEqual(2, _results.Count);
            Assert.AreEqual("A", _results[0]);
            Assert.AreEqual("B", _results[1]);
        }

        // --- WebSocket and MQTTClient IEventEmitter ---
        // These classes are guarded by #if USE_WEBINTERFACE.
        // We test the IEventEmitter pattern using a BaseEntity proxy
        // since the pattern is identical (interface default methods).

        [Test]
        public void IEventEmitterPatternWorksForNetworkingClasses()
        {
            LogAssert.ignoreFailingMessages = true;
            // Verify the IEventEmitter pattern works on any implementing class
            // using BaseEntity as a proxy (same default method implementation)
            var entity = new APIs.Entity.BaseEntity();
            IEventEmitter emitter = entity;

            var cb = _engine.Evaluate("(function() { results.Add('ws-message'); })");
            emitter.On("message", cb);
            emitter.Emit("message");

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("ws-message", _results[0]);
        }

        [Test]
        public void IEventEmitterOnceWorksForNetworkingPattern()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new APIs.Entity.BaseEntity();
            IEventEmitter emitter = entity;

            var cb = _engine.Evaluate("(function() { results.Add('open'); })");
            emitter.Once("open", cb);

            emitter.Emit("open");
            Assert.AreEqual(1, _results.Count);

            _results.Clear();
            emitter.Emit("open");
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void IEventEmitterDisposeWorksForNetworkingPattern()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new APIs.Entity.BaseEntity();
            IEventEmitter emitter = entity;

            var cb1 = _engine.Evaluate("(function() { results.Add('msg'); })");
            var cb2 = _engine.Evaluate("(function() { results.Add('err'); })");
            emitter.On("message", cb1);
            emitter.On("error", cb2);

            entity.DisposeEvents();

            emitter.Emit("message");
            emitter.Emit("error");
            Assert.AreEqual(0, _results.Count);
        }
    }
}
