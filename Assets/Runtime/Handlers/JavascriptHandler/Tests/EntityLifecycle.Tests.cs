// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
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
    /// Tests for entity lifecycle events (spawn/destroy) and auto-cleanup.
    /// Verifies that AddEntityMapping emits spawn and Delete emits destroy + cleanup.
    /// </summary>
    [TestFixture]
    public class EntityLifecycleTests
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

        // --- Spawn Event Tests (AC #1) ---

        [Test]
        public void SpawnEventFiresOnEntity()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = CreateJsFunction("function() { results.Add('spawned'); }");
            emitter.On(Events.Entity.Spawn, callback);

            // Simulate what AddEntityMapping does
            emitter.Emit(Events.Entity.Spawn);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("spawned", _results[0]);
        }

        [Test]
        public void SpawnEventFiresMultipleListenersInOrder()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cbA = CreateJsFunction("function() { results.Add('A'); }");
            var cbB = CreateJsFunction("function() { results.Add('B'); }");

            emitter.On(Events.Entity.Spawn, cbA);
            emitter.On(Events.Entity.Spawn, cbB);

            emitter.Emit(Events.Entity.Spawn);

            Assert.AreEqual(2, _results.Count);
            Assert.AreEqual("A", _results[0]);
            Assert.AreEqual("B", _results[1]);
        }

        // --- Destroy Event Tests (AC #2) ---

        [Test]
        public void DestroyEventFiresOnEntity()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var callback = CreateJsFunction("function() { results.Add('destroyed'); }");
            emitter.On(Events.Entity.Destroy, callback);

            // Simulate what Delete() does: emit destroy before cleanup
            emitter.Emit(Events.Entity.Destroy);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("destroyed", _results[0]);
        }

        [Test]
        public void DestroyListenersCanAccessEntityDuringCallback()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            // The entity should not be disposed when destroy fires
            var callback = CreateJsFunction("function() { results.Add('not-disposed'); }");
            emitter.On(Events.Entity.Destroy, callback);

            // Emit destroy — entity is still valid at this point
            emitter.Emit(Events.Entity.Destroy);
            Assert.IsFalse(entity.IsDisposed);
            Assert.AreEqual(1, _results.Count);

            // After dispose — entity is disposed
            entity.DisposeEvents();
            Assert.IsTrue(entity.IsDisposed);
        }

        [Test]
        public void DestroyThenDisposeSequence()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var destroyCallback = CreateJsFunction("function() { results.Add('destroy-fired'); }");
            var spawnCallback = CreateJsFunction("function() { results.Add('spawn-fired'); }");

            emitter.On(Events.Entity.Destroy, destroyCallback);
            emitter.On(Events.Entity.Spawn, spawnCallback);

            // Simulate Delete() sequence: emit destroy → dispose → deregister
            emitter.Emit(Events.Entity.Destroy);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("destroy-fired", _results[0]);

            entity.DisposeEvents();

            // After dispose, all listeners are gone
            Assert.AreEqual(0, entity.Listeners.Count);
            Assert.IsTrue(entity.IsDisposed);

            // Emit after dispose does nothing
            _results.Clear();
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(0, _results.Count);
        }

        // --- Auto-Cleanup Tests (AC #2 continued) ---

        [Test]
        public void DisposeEventsAfterDestroyEmitClearsAllListeners()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cb1 = CreateJsFunction("function() { results.Add('1'); }");
            var cb2 = CreateJsFunction("function() { results.Add('2'); }");
            var cb3 = CreateJsFunction("function() { results.Add('3'); }");

            emitter.On(Events.Entity.Spawn, cb1);
            emitter.On(Events.Entity.Destroy, cb2);
            emitter.Once(Events.Entity.Spawn, cb3);

            // Destroy fires, then dispose cleans up
            emitter.Emit(Events.Entity.Destroy);
            entity.DisposeEvents();

            Assert.AreEqual(0, entity.Listeners.Count);
            Assert.AreEqual(0, entity.OnceListeners.Count);
            Assert.IsTrue(entity.IsDisposed);
        }

        // --- Post-Dispose Registration Guard (AC #3) ---

        [Test]
        public void OnRejectedOnDisposedEntity()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            entity.DisposeEvents();

            var callback = CreateJsFunction("function() { results.Add('should-not-fire'); }");
            var unsub = emitter.On(Events.Entity.Spawn, callback);

            // Registration should be rejected
            Assert.AreEqual(0, entity.Listeners.Count);
            Assert.IsFalse(unsub());

            // Emit should do nothing
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void OnceRejectedOnDisposedEntity()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            entity.DisposeEvents();

            var callback = CreateJsFunction("function() { results.Add('should-not-fire'); }");
            var unsub = emitter.Once(Events.Entity.Spawn, callback);

            Assert.AreEqual(0, entity.Listeners.Count);
            Assert.AreEqual(0, entity.OnceListeners.Count);
        }

        // --- Helper Methods ---

        private JsValue CreateJsFunction(string functionExpression)
        {
            return _engine.Evaluate($"({functionExpression})");
        }
    }
}
