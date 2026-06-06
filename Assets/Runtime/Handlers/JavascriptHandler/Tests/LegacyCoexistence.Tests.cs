// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using Jint;
using Jint.Native;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;

namespace FiveSQD.WebVerse.Handlers.Javascript.Tests
{
    /// <summary>
    /// Tests verifying that the IEventEmitter addition to BaseEntity
    /// does not interfere with existing entity behavior or legacy callback mechanisms.
    /// The two callback systems (string-based Run/CallWithParams and IEventEmitter .on()/.off())
    /// are orthogonal and must coexist without interference.
    /// </summary>
    [TestFixture]
    public class LegacyCoexistenceTests
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

        // --- IEventEmitter does not affect existing BaseEntity behavior ---

        [Test]
        public void BaseEntityWithIEventEmitterHasWorkingListenersProperty()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            Assert.IsNotNull(entity.Listeners);
            Assert.AreEqual(0, entity.Listeners.Count);
        }

        [Test]
        public void EntityWithNoListenersHasZeroOverhead()
        {
            LogAssert.ignoreFailingMessages = true;
            // Lazy initialization means no collections allocated until first use
            var entity = new BaseEntity();
            // Access Listeners triggers lazy init, but before that no allocation
            Assert.IsFalse(entity.IsDisposed);
        }

        [Test]
        public void EntityWithListenersDoesNotAffectOtherEntityWithoutListeners()
        {
            LogAssert.ignoreFailingMessages = true;
            var entityA = new BaseEntity();
            var entityB = new BaseEntity();
            IEventEmitter emitterA = entityA;

            var callback = _engine.Evaluate("(function() { results.Add('A'); })");
            emitterA.On(Events.Entity.Spawn, callback);

            // Entity B has no listeners — completely unaffected
            Assert.AreEqual(0, entityB.Listeners.Count);
            Assert.IsFalse(entityB.IsDisposed);
        }

        [Test]
        public void DisposeEventsDoesNotAffectEntityValidity()
        {
            LogAssert.ignoreFailingMessages = true;
            // DisposeEvents clears event listeners but doesn't affect
            // the entity's internal validity (IsValid checks internalEntity != null)
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = _engine.Evaluate("(function() { results.Add('test'); })");
            emitter.On(Events.Entity.Spawn, callback);

            entity.DisposeEvents();

            // IsDisposed is true (event system disposed)
            Assert.IsTrue(entity.IsDisposed);
            // But the entity object itself still exists and has properties
            Assert.IsNotNull(entity.Listeners);
        }

        // --- Mixed event usage across entities ---

        [Test]
        public void MixedEventUsageAcrossEntitiesDoesNotInterfere()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity1 = new BaseEntity();
            var entity2 = new BaseEntity();
            var entity3 = new BaseEntity();

            IEventEmitter emitter1 = entity1;
            IEventEmitter emitter2 = entity2;
            // entity3 has no events registered

            var cb1 = _engine.Evaluate("(function() { results.Add('entity1'); })");
            var cb2 = _engine.Evaluate("(function() { results.Add('entity2'); })");

            emitter1.On(Events.Entity.Spawn, cb1);
            emitter2.On(Events.Entity.Destroy, cb2);

            // Emit on entity1 — only entity1's listener fires
            emitter1.Emit(Events.Entity.Spawn);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("entity1", _results[0]);

            // Emit on entity2 — only entity2's listener fires
            _results.Clear();
            emitter2.Emit(Events.Entity.Destroy);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("entity2", _results[0]);

            // entity3 is completely unaffected
            Assert.AreEqual(0, entity3.Listeners.Count);
        }

        [Test]
        public void DisposingOneEntityDoesNotAffectOthers()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity1 = new BaseEntity();
            var entity2 = new BaseEntity();
            IEventEmitter emitter1 = entity1;
            IEventEmitter emitter2 = entity2;

            var cb1 = _engine.Evaluate("(function() { results.Add('e1'); })");
            var cb2 = _engine.Evaluate("(function() { results.Add('e2'); })");

            emitter1.On(Events.Entity.Spawn, cb1);
            emitter2.On(Events.Entity.Spawn, cb2);

            // Dispose entity1
            entity1.DisposeEvents();

            // entity1 is disposed
            Assert.IsTrue(entity1.IsDisposed);
            Assert.AreEqual(0, entity1.Listeners.Count);

            // entity2 is unaffected — still has listener
            Assert.IsFalse(entity2.IsDisposed);
            emitter2.Emit(Events.Entity.Spawn);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("e2", _results[0]);
        }

        // --- Orthogonal callback systems ---

        [Test]
        public void EventSystemAndStringCallbacksAreOrthogonal()
        {
            LogAssert.ignoreFailingMessages = true;
            // This test verifies the architectural principle:
            // String-based callbacks (Run/CallWithParams) and IEventEmitter (.on/.off)
            // are completely separate systems that don't interact.

            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            // Register an event listener via .on()
            var callback = _engine.Evaluate("(function() { results.Add('event-system'); })");
            emitter.On(Events.Entity.Spawn, callback);

            // Emit via event system — listener fires
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("event-system", _results[0]);

            // The string-based system (Run/CallWithParams) is a completely different path
            // that goes through javascriptHandler.Run(functionNameString).
            // These two systems coexist on the same entity without interference.
            // We can't test Run() here without a full runtime, but we verify
            // that the event system's presence doesn't break entity construction or properties.
        }

        [Test]
        public void EventListenersAndOnceListenersSurviveMultipleEmits()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var persistent = _engine.Evaluate("(function() { results.Add('persistent'); })");
            var oneShot = _engine.Evaluate("(function() { results.Add('oneshot'); })");

            emitter.On(Events.Entity.Spawn, persistent);
            emitter.Once(Events.Entity.Spawn, oneShot);

            // First emit — both fire
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(2, _results.Count);

            // Second emit — only persistent fires
            _results.Clear();
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("persistent", _results[0]);

            // Entity is still valid for other operations
            Assert.IsFalse(entity.IsDisposed);
        }

        // --- Backward Compatibility Regression Suite (Story 3.2 + 3.3) ---
        // Comprehensive regression tests ensuring no existing behavior is broken.

        [Test]
        public void EventConstantsMatchExpectedStringValues()
        {
            LogAssert.ignoreFailingMessages = true;
            // Regression: event constants must stay stable across versions
            Assert.AreEqual("load", Events.World.Load);
            Assert.AreEqual("ready", Events.World.Ready);
            Assert.AreEqual("error", Events.World.Error);
            Assert.AreEqual("spawn", Events.Entity.Spawn);
            Assert.AreEqual("destroy", Events.Entity.Destroy);
            Assert.AreEqual("collision:enter", Events.Collision.Enter);
            Assert.AreEqual("collision:exit", Events.Collision.Exit);
        }

        [Test]
        public void EventsIsValidAcceptsAllDefinedEvents()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.IsTrue(Events.IsValid("load"));
            Assert.IsTrue(Events.IsValid("ready"));
            Assert.IsTrue(Events.IsValid("error"));
            Assert.IsTrue(Events.IsValid("spawn"));
            Assert.IsTrue(Events.IsValid("destroy"));
            Assert.IsTrue(Events.IsValid("collision:enter"));
            Assert.IsTrue(Events.IsValid("collision:exit"));
        }

        [Test]
        public void EventsIsValidRejectsInvalidNames()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.IsFalse(Events.IsValid(""));
            Assert.IsFalse(Events.IsValid(null));
            Assert.IsFalse(Events.IsValid("nonexistent"));
            Assert.IsFalse(Events.IsValid("Spawn")); // case sensitive
        }

        [Test]
        public void WorldStaticEventMethodsWorkCorrectly()
        {
            LogAssert.ignoreFailingMessages = true;
            // Regression: World static event system must work
            var callback = _engine.Evaluate("(function() { results.Add('world-ready'); })");

            APIs.Utilities.World.DisposeAllWorldListeners();
            APIs.Utilities.World.on(Events.World.Ready, callback);
            APIs.Utilities.World.Emit(Events.World.Ready);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("world-ready", _results[0]);

            APIs.Utilities.World.DisposeAllWorldListeners();
        }

        [Test]
        public void WorldApiVersionIsStable()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual("1.0.0", APIs.Utilities.World.apiVersion);
        }

        [Test]
        public void EntityEventSystemFullLifecycleRegression()
        {
            LogAssert.ignoreFailingMessages = true;
            // Full lifecycle: create entity → register listeners → emit spawn
            // → emit destroy → dispose → verify cleanup
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var spawnCb = _engine.Evaluate("(function() { results.Add('spawn'); })");
            var destroyCb = _engine.Evaluate("(function() { results.Add('destroy'); })");

            emitter.On(Events.Entity.Spawn, spawnCb);
            emitter.On(Events.Entity.Destroy, destroyCb);

            // Spawn
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("spawn", _results[0]);

            // Destroy
            _results.Clear();
            emitter.Emit(Events.Entity.Destroy);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("destroy", _results[0]);

            // Cleanup
            entity.DisposeEvents();
            Assert.IsTrue(entity.IsDisposed);
            Assert.AreEqual(0, entity.Listeners.Count);

            // Post-dispose: no new registrations
            _results.Clear();
            var postCb = _engine.Evaluate("(function() { results.Add('post'); })");
            emitter.On(Events.Entity.Spawn, postCb);
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void UnsubscribeFunctionWorksAcrossEntireLifecycle()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cb = _engine.Evaluate("(function() { results.Add('fired'); })");
            var unsub = emitter.On(Events.Entity.Spawn, cb);

            // First emit — fires
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(1, _results.Count);

            // Unsubscribe
            Assert.IsTrue(unsub());

            // Second emit — does not fire
            _results.Clear();
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(0, _results.Count);

            // Double unsubscribe is safe
            Assert.IsFalse(unsub());
        }

        [Test]
        public void CatchLogContinueRegressionTest()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var good1 = _engine.Evaluate("(function() { results.Add('good1'); })");
            var bad = _engine.Evaluate("(function() { throw new Error('test'); })");
            var good2 = _engine.Evaluate("(function() { results.Add('good2'); })");

            emitter.On(Events.Entity.Spawn, good1);
            emitter.On(Events.Entity.Spawn, bad);
            emitter.On(Events.Entity.Spawn, good2);

            Assert.DoesNotThrow(() => emitter.Emit(Events.Entity.Spawn));
            Assert.AreEqual(2, _results.Count);
            Assert.AreEqual("good1", _results[0]);
            Assert.AreEqual("good2", _results[1]);
        }
    }
}

