// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using Jint;
using Jint.Native;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;

namespace FiveSQD.WebVerse.Handlers.Javascript.Tests
{
    /// <summary>
    /// Tests for collision event emission mechanics.
    /// Full integration tests require Unity physics runtime;
    /// these tests verify the emit/listener pattern works for collision events.
    /// </summary>
    [TestFixture]
    public class CollisionEventsTests
    {
        private Engine _engine;
        private List<string> _results;

        [SetUp]
        public void SetUp()
        {
            _engine = new Engine();
            _results = new List<string>();
            _engine.SetValue("results", _results);
        }

        // --- Collision Enter Event ---

        [Test]
        public void CollisionEnterListenerFiresOnEmit()
        {
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = _engine.Evaluate(
                "(function(other) { results.Add('collision-enter'); })");

            emitter.On(Events.Collision.Enter, callback);
            emitter.Emit(Events.Collision.Enter, JsValue.Null);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("collision-enter", _results[0]);
        }

        [Test]
        public void CollisionEnterCallbackReceivesArgument()
        {
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var argsReceived = new List<JsValue>();
            _engine.SetValue("argsReceived", argsReceived);

            var callback = _engine.Evaluate(
                "(function(other) { argsReceived.Add(other); })");

            emitter.On(Events.Collision.Enter, callback);

            // Simulate passing the other entity as a JsValue
            var otherValue = JsValue.FromObject(_engine, "other-entity-ref");
            emitter.Emit(Events.Collision.Enter, otherValue);

            Assert.AreEqual(1, argsReceived.Count);
            Assert.AreNotEqual(JsValue.Null, argsReceived[0]);
        }

        [Test]
        public void CollisionEnterWithNullOtherEntityPassesNull()
        {
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var argsReceived = new List<JsValue>();
            _engine.SetValue("argsReceived", argsReceived);

            var callback = _engine.Evaluate(
                "(function(other) { argsReceived.Add(other); })");

            emitter.On(Events.Collision.Enter, callback);
            emitter.Emit(Events.Collision.Enter, JsValue.Null);

            Assert.AreEqual(1, argsReceived.Count);
            Assert.AreEqual(JsValue.Null, argsReceived[0]);
        }

        // --- Collision Exit Event ---

        [Test]
        public void CollisionExitListenerFiresOnEmit()
        {
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = _engine.Evaluate(
                "(function(other) { results.Add('collision-exit'); })");

            emitter.On(Events.Collision.Exit, callback);
            emitter.Emit(Events.Collision.Exit, JsValue.Null);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("collision-exit", _results[0]);
        }

        // --- Performance Guard ---

        [Test]
        public void EntityWithNoCollisionListenersHasNoOverhead()
        {
            var entity = new BaseEntity();

            // No listeners registered for collision events
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Collision.Enter));
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Collision.Exit));
        }

        [Test]
        public void CollisionListenerCheckIsO1()
        {
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            // Register a non-collision listener
            var cb = _engine.Evaluate("(function() {})");
            emitter.On(Events.Entity.Spawn, cb);

            // Collision check should be false (only spawn registered)
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Collision.Enter));
            Assert.IsTrue(entity.Listeners.ContainsKey(Events.Entity.Spawn));
        }

        // --- Multiple Collision Listeners ---

        [Test]
        public void MultipleCollisionListenersFireInOrder()
        {
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cbA = _engine.Evaluate("(function() { results.Add('A'); })");
            var cbB = _engine.Evaluate("(function() { results.Add('B'); })");

            emitter.On(Events.Collision.Enter, cbA);
            emitter.On(Events.Collision.Enter, cbB);

            emitter.Emit(Events.Collision.Enter, JsValue.Null);

            Assert.AreEqual(2, _results.Count);
            Assert.AreEqual("A", _results[0]);
            Assert.AreEqual("B", _results[1]);
        }

        // --- Once for Collision ---

        [Test]
        public void OnceCollisionListenerAutoRemovesAfterFirstCollision()
        {
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = _engine.Evaluate(
                "(function() { results.Add('once-collision'); })");

            emitter.Once(Events.Collision.Enter, callback);

            emitter.Emit(Events.Collision.Enter, JsValue.Null);
            Assert.AreEqual(1, _results.Count);

            _results.Clear();
            emitter.Emit(Events.Collision.Enter, JsValue.Null);
            Assert.AreEqual(0, _results.Count);
        }
    }
}
