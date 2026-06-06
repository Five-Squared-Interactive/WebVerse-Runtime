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
    /// Tests for entity property change event emission.
    /// Verifies that SetPosition/SetRotation/SetScale/SetVisibility emit
    /// the correct events when listeners are registered.
    /// Note: These test the emit mechanics via direct Emit() calls since
    /// the actual Set* methods require a valid internalEntity (Unity runtime).
    /// </summary>
    [TestFixture]
    public class PropertyChangeEventsTests
    {
        private Engine _engine;
        private List<string> _results;

        [SetUp]
        public void SetUp()
        {
            _engine = new Engine();
            _results = new List<string>();
            _engine.SetValue("results", _results);
            ObserverLimits.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            ObserverLimits.Reset();
        }

        // --- Event Constants Exist ---

        [Test]
        public void PositionEventConstantExists()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual("position", Events.Entity.Position);
            Assert.IsTrue(Events.IsValid("position"));
        }

        [Test]
        public void RotationEventConstantExists()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual("rotation", Events.Entity.Rotation);
            Assert.IsTrue(Events.IsValid("rotation"));
        }

        [Test]
        public void ScaleEventConstantExists()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual("scale", Events.Entity.Scale);
            Assert.IsTrue(Events.IsValid("scale"));
        }

        [Test]
        public void VisibilityEventConstantExists()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.AreEqual("visibility", Events.Entity.Visibility);
            Assert.IsTrue(Events.IsValid("visibility"));
        }

        // --- Property Events Emit Correctly ---

        [Test]
        public void PositionListenerFiresOnEmit()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cb = _engine.Evaluate("(function() { results.Add('position-changed'); })");
            emitter.On(Events.Entity.Position, cb);
            emitter.Emit(Events.Entity.Position);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("position-changed", _results[0]);
        }

        [Test]
        public void RotationListenerFiresOnEmit()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cb = _engine.Evaluate("(function() { results.Add('rotation-changed'); })");
            emitter.On(Events.Entity.Rotation, cb);
            emitter.Emit(Events.Entity.Rotation);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("rotation-changed", _results[0]);
        }

        [Test]
        public void ScaleListenerFiresOnEmit()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cb = _engine.Evaluate("(function() { results.Add('scale-changed'); })");
            emitter.On(Events.Entity.Scale, cb);
            emitter.Emit(Events.Entity.Scale);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("scale-changed", _results[0]);
        }

        [Test]
        public void VisibilityListenerFiresOnEmit()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cb = _engine.Evaluate("(function() { results.Add('visibility-changed'); })");
            emitter.On(Events.Entity.Visibility, cb);
            emitter.Emit(Events.Entity.Visibility);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("visibility-changed", _results[0]);
        }

        // --- Performance Guard: No Overhead Without Listeners ---

        [Test]
        public void EntityWithNoPropertyListenersHasNoOverhead()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();

            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Entity.Position));
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Entity.Rotation));
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Entity.Scale));
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Entity.Visibility));
        }

        [Test]
        public void ListenerCheckIsO1DictionaryLookup()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            // Register only position listener
            var cb = _engine.Evaluate("(function() {})");
            emitter.On(Events.Entity.Position, cb);

            // Only position should have listeners
            Assert.IsTrue(entity.Listeners.ContainsKey(Events.Entity.Position));
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Entity.Rotation));
            Assert.IsFalse(entity.Listeners.ContainsKey(Events.Entity.Scale));
        }

        // --- Multiple Listeners on Same Property ---

        [Test]
        public void MultiplePositionListenersFireInOrder()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cbA = _engine.Evaluate("(function() { results.Add('A'); })");
            var cbB = _engine.Evaluate("(function() { results.Add('B'); })");

            emitter.On(Events.Entity.Position, cbA);
            emitter.On(Events.Entity.Position, cbB);
            emitter.Emit(Events.Entity.Position);

            Assert.AreEqual(2, _results.Count);
            Assert.AreEqual("A", _results[0]);
            Assert.AreEqual("B", _results[1]);
        }

        // --- Once for Property Events ---

        [Test]
        public void OncePropertyListenerAutoRemoves()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new BaseEntity();
            IEventEmitter emitter = entity;

            var cb = _engine.Evaluate("(function() { results.Add('once'); })");
            emitter.Once(Events.Entity.Position, cb);

            emitter.Emit(Events.Entity.Position);
            Assert.AreEqual(1, _results.Count);

            _results.Clear();
            emitter.Emit(Events.Entity.Position);
            Assert.AreEqual(0, _results.Count);
        }
    }
}
