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
    /// Tests for ObserverLimits global count tracking and enforcement.
    /// </summary>
    [TestFixture]
    public class ObserverLimitsTests
    {
        private Engine _engine;

        [SetUp]
        public void SetUp()
        {
            _engine = new Engine();
            ObserverLimits.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            ObserverLimits.Reset();
        }

        // --- Count Tracking ---

        [Test]
        public void InitialCountIsZero()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual(0, ObserverLimits.CurrentCount);
        }

        [Test]
        public void OnIncrementsCount()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var cb = _engine.Evaluate("(function() {})");

            emitter.On(Events.Entity.Spawn, cb);

            Assert.AreEqual(1, ObserverLimits.CurrentCount);
        }

        [Test]
        public void MultipleOnsIncrementCorrectly()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var cb1 = _engine.Evaluate("(function() {})");
            var cb2 = _engine.Evaluate("(function() {})");
            var cb3 = _engine.Evaluate("(function() {})");

            emitter.On(Events.Entity.Spawn, cb1);
            emitter.On(Events.Entity.Spawn, cb2);
            emitter.On(Events.Entity.Destroy, cb3);

            Assert.AreEqual(3, ObserverLimits.CurrentCount);
        }

        [Test]
        public void OffDecrementsCount()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var cb = _engine.Evaluate("(function() {})");

            emitter.On(Events.Entity.Spawn, cb);
            Assert.AreEqual(1, ObserverLimits.CurrentCount);

            emitter.Off(Events.Entity.Spawn, cb);
            Assert.AreEqual(0, ObserverLimits.CurrentCount);
        }

        [Test]
        public void OffAllDecrementsCorrectly()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var cb1 = _engine.Evaluate("(function() {})");
            var cb2 = _engine.Evaluate("(function() {})");

            emitter.On(Events.Entity.Spawn, cb1);
            emitter.On(Events.Entity.Spawn, cb2);
            Assert.AreEqual(2, ObserverLimits.CurrentCount);

            emitter.Off(Events.Entity.Spawn);
            Assert.AreEqual(0, ObserverLimits.CurrentCount);
        }

        [Test]
        public void DisposeEventsDecrementsCorrectly()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var cb1 = _engine.Evaluate("(function() {})");
            var cb2 = _engine.Evaluate("(function() {})");
            var cb3 = _engine.Evaluate("(function() {})");

            emitter.On(Events.Entity.Spawn, cb1);
            emitter.On(Events.Entity.Destroy, cb2);
            emitter.On(Events.Entity.Position, cb3);
            Assert.AreEqual(3, ObserverLimits.CurrentCount);

            entity.DisposeEvents();
            Assert.AreEqual(0, ObserverLimits.CurrentCount);
        }

        [Test]
        public void UnsubscribeFunctionDecrementsCount()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var cb = _engine.Evaluate("(function() {})");

            var unsub = emitter.On(Events.Entity.Spawn, cb);
            Assert.AreEqual(1, ObserverLimits.CurrentCount);

            unsub();
            Assert.AreEqual(0, ObserverLimits.CurrentCount);
        }

        [Test]
        public void OnceDecrementedAfterEmit()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var cb = _engine.Evaluate("(function() {})");

            emitter.Once(Events.Entity.Spawn, cb);
            Assert.AreEqual(1, ObserverLimits.CurrentCount);

            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(0, ObserverLimits.CurrentCount);
        }

        // --- Limit Enforcement ---

        [Test]
        public void CanRegisterReturnsTrueUnderLimit()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.IsTrue(ObserverLimits.CanRegister);
        }

        [Test]
        public void OnRejectedAtMaxObservers()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            // Manually set count to limit
            ObserverLimits.CurrentCount = ObserverLimits.MaxObservers;

            var cb = _engine.Evaluate("(function() {})");
            var unsub = emitter.On(Events.Entity.Spawn, cb);

            // Registration should be rejected
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Entity.Spawn));
            Assert.IsFalse(unsub());
            Assert.AreEqual(ObserverLimits.MaxObservers, ObserverLimits.CurrentCount);
        }

        [Test]
        public void ExistingListenersContinueAtLimit()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;
            var results = new List<string>();
            _engine.SetValue("results", results);

            var cb = _engine.Evaluate("(function() { results.Add('fired'); })");
            emitter.On(Events.Entity.Spawn, cb);

            // Set count to limit (the one we just added counts)
            ObserverLimits.CurrentCount = ObserverLimits.MaxObservers;

            // Existing listener should still fire
            emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(1, results.Count);
        }

        // --- Cross-Entity Tracking ---

        [Test]
        public void CountTracksAcrossMultipleEntities()
        {
            LogAssert.ignoreFailingMessages = true;
            var entityA = new BaseEntity();
            var entityB = new BaseEntity();
            IEventEmitter emitterA = entityA;
            IEventEmitter emitterB = entityB;
            var cb = _engine.Evaluate("(function() {})");

            emitterA.On(Events.Entity.Spawn, cb);
            emitterB.On(Events.Entity.Spawn, cb);

            Assert.AreEqual(2, ObserverLimits.CurrentCount);

            entityA.DisposeEvents();
            Assert.AreEqual(1, ObserverLimits.CurrentCount);

            entityB.DisposeEvents();
            Assert.AreEqual(0, ObserverLimits.CurrentCount);
        }

        [Test]
        public void ResetClearsCount()
        {
            LogAssert.ignoreFailingMessages = true;
            ObserverLimits.CurrentCount = 500;
            ObserverLimits.Reset();
            Assert.AreEqual(0, ObserverLimits.CurrentCount);
        }

        [Test]
        public void MaxObserversIs1000()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual(1000, ObserverLimits.MaxObservers);
        }
    }
}
