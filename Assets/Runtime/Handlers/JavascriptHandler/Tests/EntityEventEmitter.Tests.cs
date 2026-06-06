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
    /// Tests for BaseEntity's IEventEmitter implementation.
    /// Verifies that entities gain event capability through the interface
    /// and that per-instance isolation and disposal work correctly.
    /// </summary>
    [TestFixture]
    public class EntityEventEmitterTests
    {
        private Engine _engine;

        [SetUp]
        public void SetUp()
        {
            _engine = new Engine();
        }

        // --- IEventEmitter Properties Exist (AC #1) ---

        [Test]
        public void BaseEntityImplementsIEventEmitter()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            Assert.IsInstanceOf<IEventEmitter>(entity);
        }

        [Test]
        public void ListenersPropertyIsNotNullAfterConstruction()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            Assert.IsNotNull(entity.Listeners);
        }

        [Test]
        public void OnceListenersPropertyIsNotNullAfterConstruction()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            Assert.IsNotNull(entity.OnceListeners);
        }

        [Test]
        public void EmittingEventsPropertyIsNotNullAfterConstruction()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            Assert.IsNotNull(entity.EmittingEvents);
        }

        [Test]
        public void IsDisposedIsFalseAfterConstruction()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            Assert.IsFalse(entity.IsDisposed);
        }

        // --- Event Registration Works on BaseEntity (AC #1) ---

        [Test]
        public void OnRegistersListenerOnEntity()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var callback = CreateJsFunction("function() { return 1; }");

            emitter.On(Events.Entity.Spawn, callback);

            Assert.IsTrue(entity.Listeners.ContainsKey(Events.Entity.Spawn));
            Assert.AreEqual(1, entity.Listeners[Events.Entity.Spawn].Count);
        }

        [Test]
        public void OnRegistersCallbackReference()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var callback = CreateJsFunction("function() { return 'fired'; }");

            emitter.On(Events.Entity.Spawn, callback);

            Assert.AreSame(callback, entity.Listeners[Events.Entity.Spawn][0]);
        }

        // --- Per-Instance Isolation (AC #2) ---

        [Test]
        public void TwoEntitiesHaveSeparateListenerStorage()
        {
            LogAssert.ignoreFailingMessages = true;
            var entityA = new BaseEntity();
            var entityB = new BaseEntity();
            IEventEmitter emitterA = entityA;
            IEventEmitter emitterB = entityB;
            var callback = CreateJsFunction("function() { return 1; }");

            emitterA.On(Events.Entity.Spawn, callback);

            Assert.IsTrue(entityA.Listeners.ContainsKey(Events.Entity.Spawn));
            Assert.IsFalse(entityB.Listeners.ContainsKey(Events.Entity.Spawn));
        }

        [Test]
        public void TwoEntitiesHaveSeparateOnceListenerStorage()
        {
            LogAssert.ignoreFailingMessages = true;
            var entityA = new BaseEntity();
            var entityB = new BaseEntity();
            IEventEmitter emitterA = entityA;
            var callback = CreateJsFunction("function() { return 1; }");

            emitterA.Once(Events.Entity.Spawn, callback);

            Assert.IsTrue(entityA.OnceListeners.Contains(callback));
            Assert.IsFalse(entityB.OnceListeners.Contains(callback));
        }

        // --- DisposeEvents (AC #3) ---

        [Test]
        public void DisposeEventsClearsAllListeners()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var callbackA = CreateJsFunction("function() { return 'A'; }");
            var callbackB = CreateJsFunction("function() { return 'B'; }");

            emitter.On(Events.Entity.Spawn, callbackA);
            emitter.Once(Events.Entity.Destroy, callbackB);

            entity.DisposeEvents();

            Assert.AreEqual(0, entity.Listeners.Count);
            Assert.AreEqual(0, entity.OnceListeners.Count);
            Assert.AreEqual(0, entity.EmittingEvents.Count);
        }

        [Test]
        public void DisposeEventsSetsIsDisposedTrue()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            Assert.IsFalse(entity.IsDisposed);

            entity.DisposeEvents();

            Assert.IsTrue(entity.IsDisposed);
        }

        [Test]
        public void OnRejectedAfterDisposeEvents()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            entity.DisposeEvents();

            var callback = CreateJsFunction("function() { return 1; }");
            var unsub = emitter.On(Events.Entity.Spawn, callback);

            // Registration should be rejected — IsDisposed is true
            Assert.AreEqual(0, entity.Listeners.Count);
            Assert.IsFalse(unsub());
        }

        [Test]
        public void DisposeEventsDoesNotAffectOtherEntities()
        {
            LogAssert.ignoreFailingMessages = true;
            var entityA = new BaseEntity();
            var entityB = new BaseEntity();
            IEventEmitter emitterA = entityA;
            IEventEmitter emitterB = entityB;
            var callback = CreateJsFunction("function() { return 1; }");

            emitterA.On(Events.Entity.Spawn, callback);
            emitterB.On(Events.Entity.Spawn, callback);

            entityA.DisposeEvents();

            Assert.AreEqual(0, entityA.Listeners.Count);
            Assert.IsTrue(entityA.IsDisposed);
            Assert.AreEqual(1, entityB.Listeners[Events.Entity.Spawn].Count);
            Assert.IsFalse(entityB.IsDisposed);
        }

        // --- Off() Tests (Review Patch) ---

        [Test]
        public void OffRemovesSpecificListenerFromEntity()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var callbackA = CreateJsFunction("function() { return 'A'; }");
            var callbackB = CreateJsFunction("function() { return 'B'; }");

            emitter.On(Events.Entity.Spawn, callbackA);
            emitter.On(Events.Entity.Spawn, callbackB);
            emitter.Off(Events.Entity.Spawn, callbackA);

            Assert.AreEqual(1, entity.Listeners[Events.Entity.Spawn].Count);
        }

        [Test]
        public void OffAllRemovesAllListenersForEvent()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var callbackA = CreateJsFunction("function() { return 'A'; }");
            var callbackB = CreateJsFunction("function() { return 'B'; }");

            emitter.On(Events.Entity.Spawn, callbackA);
            emitter.On(Events.Entity.Spawn, callbackB);
            emitter.Off(Events.Entity.Spawn);

            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Entity.Spawn));
        }

        // --- Once End-to-End Test (Review Patch) ---

        [Test]
        public void OnceRegistersInBothListenersAndOnceListeners()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var callback = CreateJsFunction("function() { return 'once'; }");

            emitter.Once(Events.Entity.Spawn, callback);

            Assert.AreEqual(1, entity.Listeners[Events.Entity.Spawn].Count);
            Assert.IsTrue(entity.OnceListeners.Contains(callback));
        }

        // --- Emit with Zero Listeners Test (Review Patch) ---

        [Test]
        public void EmitWithNoListenersDoesNotThrow()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            Assert.DoesNotThrow(() => emitter.Emit(Events.Entity.Spawn));
            Assert.DoesNotThrow(() => emitter.Emit("nonexistent"));
        }

        // --- Lazy Initialization Tests (Review Patch) ---

        [Test]
        public void LazyInitializationCreatesCollectionsOnFirstAccess()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            // Access through the property triggers lazy init
            Assert.IsNotNull(entity.Listeners);
            Assert.IsNotNull(entity.OnceListeners);
            Assert.IsNotNull(entity.EmittingEvents);
        }

        // --- Idempotent DisposeEvents Test (Review Patch) ---

        [Test]
        public void DisposeEventsCalledTwiceDoesNotThrow()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var callback = CreateJsFunction("function() { return 1; }");
            emitter.On(Events.Entity.Spawn, callback);

            entity.DisposeEvents();
            Assert.DoesNotThrow(() => entity.DisposeEvents());
            Assert.IsTrue(entity.IsDisposed);
        }

        // --- Helper Methods ---

        private JsValue CreateJsFunction(string functionExpression)
        {
            return _engine.Evaluate($"({functionExpression})");
        }
    }
}
