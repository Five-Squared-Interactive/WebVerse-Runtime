// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using Jint;
using Jint.Native;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;

namespace FiveSQD.WebVerse.Handlers.Javascript.Tests
{
    /// <summary>
    /// Tests for collision event listener registration mechanics.
    /// Emit requires a full WebVerseRuntime (Jint engine singleton), so these
    /// unit tests verify On/Once/Off/Listeners without calling Emit.
    /// </summary>
    [TestFixture]
    public class CollisionEventsTests
    {
        private Engine _engine;

        [SetUp]
        public void SetUp()
        {
            _engine = new Engine();
        }

        // --- Collision Enter Registration ---

        [Test]
        public void CollisionEnterListenerRegisters()
        {
            LogAssert.ignoreFailingMessages = true;

            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = _engine.Evaluate("(function(other) {})");
            emitter.On(Events.Collision.Enter, callback);

            Assert.IsTrue(entity.Listeners.ContainsKey(Events.Collision.Enter));
            Assert.AreEqual(1, entity.Listeners[Events.Collision.Enter].Count);
        }

        [Test]
        public void CollisionEnterListenerUnregisters()
        {
            LogAssert.ignoreFailingMessages = true;

            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = _engine.Evaluate("(function(other) {})");
            var unsub = emitter.On(Events.Collision.Enter, callback);

            Assert.IsTrue(entity.Listeners.ContainsKey(Events.Collision.Enter));

            unsub();

            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Collision.Enter));
        }

        [Test]
        public void CollisionExitListenerRegisters()
        {
            LogAssert.ignoreFailingMessages = true;

            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = _engine.Evaluate("(function(other) {})");
            emitter.On(Events.Collision.Exit, callback);

            Assert.IsTrue(entity.Listeners.ContainsKey(Events.Collision.Exit));
            Assert.AreEqual(1, entity.Listeners[Events.Collision.Exit].Count);
        }

        // --- Performance Guard ---

        [Test]
        public void EntityWithNoCollisionListenersHasNoOverhead()
        {
            LogAssert.ignoreFailingMessages = true;

            var entity = new BaseEntity();

            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Collision.Enter));
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Collision.Exit));
        }

        [Test]
        public void CollisionListenerCheckIsO1()
        {
            LogAssert.ignoreFailingMessages = true;

            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cb = _engine.Evaluate("(function() {})");
            emitter.On(Events.Entity.Spawn, cb);

            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Collision.Enter));
            Assert.IsTrue(entity.Listeners.ContainsKey(Events.Entity.Spawn));
        }

        // --- Multiple Collision Listeners ---

        [Test]
        public void MultipleCollisionListenersRegisterInOrder()
        {
            LogAssert.ignoreFailingMessages = true;

            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cbA = _engine.Evaluate("(function() {})");
            var cbB = _engine.Evaluate("(function() {})");

            emitter.On(Events.Collision.Enter, cbA);
            emitter.On(Events.Collision.Enter, cbB);

            Assert.AreEqual(2, entity.Listeners[Events.Collision.Enter].Count);
            Assert.AreSame(cbA, entity.Listeners[Events.Collision.Enter][0]);
            Assert.AreSame(cbB, entity.Listeners[Events.Collision.Enter][1]);
        }

        // --- Once for Collision ---

        [Test]
        public void OnceCollisionListenerRegistersInOnceSet()
        {
            LogAssert.ignoreFailingMessages = true;

            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = _engine.Evaluate("(function() {})");
            emitter.Once(Events.Collision.Enter, callback);

            Assert.IsTrue(entity.Listeners.ContainsKey(Events.Collision.Enter));
            Assert.AreEqual(1, entity.Listeners[Events.Collision.Enter].Count);
            Assert.IsTrue(entity.OnceListeners.Contains(callback));
        }

        // --- Off removes collision listener ---

        [Test]
        public void OffRemovesSpecificCollisionListener()
        {
            LogAssert.ignoreFailingMessages = true;

            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cbA = _engine.Evaluate("(function() {})");
            var cbB = _engine.Evaluate("(function() {})");

            emitter.On(Events.Collision.Enter, cbA);
            emitter.On(Events.Collision.Enter, cbB);

            emitter.Off(Events.Collision.Enter, cbA);

            Assert.AreEqual(1, entity.Listeners[Events.Collision.Enter].Count);
            Assert.AreSame(cbB, entity.Listeners[Events.Collision.Enter][0]);
        }
    }
}
