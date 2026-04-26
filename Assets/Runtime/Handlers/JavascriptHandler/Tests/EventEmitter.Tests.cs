// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Jint;
using Jint.Native;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;

namespace FiveSQD.WebVerse.Handlers.Javascript.Tests
{
    /// <summary>
    /// Test implementation of IEventEmitter for unit testing.
    /// </summary>
    public class TestEventEmitter : IEventEmitter
    {
        public Dictionary<string, List<JsValue>> Listeners { get; }
            = new Dictionary<string, List<JsValue>>();

        public HashSet<JsValue> OnceListeners { get; }
            = new HashSet<JsValue>();

        public HashSet<string> EmittingEvents { get; }
            = new HashSet<string>();

        public bool IsDisposed { get; set; } = false;
    }

    /// <summary>
    /// Tests for the IEventEmitter interface default method implementations.
    /// </summary>
    [TestFixture]
    public class EventEmitterTests
    {
        private Engine _engine;
        private IEventEmitter _emitter;

        [SetUp]
        public void SetUp()
        {
            _engine = new Engine();
            _emitter = new TestEventEmitter();
        }

        // --- On() Registration Tests (AC #1, #2) ---

        [Test]
        public void OnStoresCallbackInListenersDictionary()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            _emitter.On(Events.Entity.Spawn, callback);

            Assert.IsTrue(_emitter.Listeners.ContainsKey(Events.Entity.Spawn));
            Assert.AreEqual(1, _emitter.Listeners[Events.Entity.Spawn].Count);
        }

        [Test]
        public void OnReturnsUnsubscribeFunction()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            var unsub = _emitter.On(Events.Entity.Spawn, callback);

            Assert.IsNotNull(unsub);
            Assert.IsTrue(unsub is Func<bool>);
        }

        [Test]
        public void UnsubscribeFunctionRemovesSpecificListener()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            var unsub = _emitter.On(Events.Entity.Spawn, callback);

            Assert.AreEqual(1, _emitter.Listeners[Events.Entity.Spawn].Count);

            var result = unsub();
            Assert.IsTrue(result);
            Assert.IsFalse(_emitter.Listeners.ContainsKey(Events.Entity.Spawn));
        }

        [Test]
        public void UnsubscribeCalledTwiceReturnsFalseSecondTime()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            var unsub = _emitter.On(Events.Entity.Spawn, callback);

            Assert.IsTrue(unsub());
            Assert.IsFalse(unsub());
        }

        [Test]
        public void OnRejectsNullEventName()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            var unsub = _emitter.On(null, callback);

            Assert.IsFalse(unsub());
            Assert.AreEqual(0, _emitter.Listeners.Count);
        }

        [Test]
        public void OnRejectsEmptyEventName()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            var unsub = _emitter.On("", callback);

            Assert.IsFalse(unsub());
            Assert.AreEqual(0, _emitter.Listeners.Count);
        }

        [Test]
        public void OnRejectsNullCallback()
        {
            var unsub = _emitter.On(Events.Entity.Spawn, null);
            Assert.IsFalse(unsub());
            Assert.AreEqual(0, _emitter.Listeners.Count);
        }

        [Test]
        public void OnRejectsRegistrationOnDisposedEmitter()
        {
            ((TestEventEmitter)_emitter).IsDisposed = true;
            var callback = CreateJsFunction("function() { return 1; }");
            var unsub = _emitter.On(Events.Entity.Spawn, callback);

            Assert.IsFalse(unsub());
            Assert.AreEqual(0, _emitter.Listeners.Count);
        }

        // --- Off() Removal Tests (AC #4, #5) ---

        [Test]
        public void OffRemovesSpecificCallback()
        {
            var callbackA = CreateJsFunction("function() { return 'A'; }");
            var callbackB = CreateJsFunction("function() { return 'B'; }");
            _emitter.On(Events.Entity.Spawn, callbackA);
            _emitter.On(Events.Entity.Spawn, callbackB);

            _emitter.Off(Events.Entity.Spawn, callbackA);

            Assert.AreEqual(1, _emitter.Listeners[Events.Entity.Spawn].Count);
            Assert.AreSame(callbackB, _emitter.Listeners[Events.Entity.Spawn][0]);
        }

        [Test]
        public void OffRemovesAllListenersForEvent()
        {
            var callbackA = CreateJsFunction("function() { return 'A'; }");
            var callbackB = CreateJsFunction("function() { return 'B'; }");
            _emitter.On(Events.Entity.Spawn, callbackA);
            _emitter.On(Events.Entity.Spawn, callbackB);

            _emitter.Off(Events.Entity.Spawn);

            Assert.IsFalse(_emitter.Listeners.ContainsKey(Events.Entity.Spawn));
        }

        [Test]
        public void OffCleansUpEmptyEventKey()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            _emitter.On(Events.Entity.Spawn, callback);
            _emitter.Off(Events.Entity.Spawn, callback);

            Assert.IsFalse(_emitter.Listeners.ContainsKey(Events.Entity.Spawn));
        }

        [Test]
        public void OffWithNullEventNameDoesNotThrow()
        {
            Assert.DoesNotThrow(() => _emitter.Off(null, JsValue.Undefined));
            Assert.DoesNotThrow(() => _emitter.Off(null));
        }

        // --- Once() Auto-Remove Tests (AC #3) ---

        [Test]
        public void OnceRegistersCallbackInListeners()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            _emitter.Once(Events.Entity.Spawn, callback);

            Assert.IsTrue(_emitter.Listeners.ContainsKey(Events.Entity.Spawn));
            Assert.AreEqual(1, _emitter.Listeners[Events.Entity.Spawn].Count);
        }

        [Test]
        public void OnceTracksCallbackInOnceListeners()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            _emitter.Once(Events.Entity.Spawn, callback);

            Assert.IsTrue(_emitter.OnceListeners.Contains(callback));
        }

        [Test]
        public void OnceCallbackRemovedAfterEmit()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            _emitter.Once(Events.Entity.Spawn, callback);

            _emitter.Emit(Events.Entity.Spawn);

            Assert.IsFalse(_emitter.Listeners.ContainsKey(Events.Entity.Spawn));
            Assert.IsFalse(_emitter.OnceListeners.Contains(callback));
        }

        [Test]
        public void OnceUnsubscribeRemovesBeforeFiring()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            var unsub = _emitter.Once(Events.Entity.Spawn, callback);

            unsub();

            Assert.IsFalse(_emitter.Listeners.ContainsKey(Events.Entity.Spawn));
            Assert.IsFalse(_emitter.OnceListeners.Contains(callback));
        }

        // --- Emit() Registration Order Tests (AC #6) ---

        [Test]
        public void EmitFiresListenersInRegistrationOrder()
        {
            var results = new List<string>();

            _engine.SetValue("results", results);
            var cbA = CreateJsFunction("function() { results.Add('A'); }");
            var cbB = CreateJsFunction("function() { results.Add('B'); }");
            var cbC = CreateJsFunction("function() { results.Add('C'); }");

            _emitter.On(Events.Entity.Spawn, cbA);
            _emitter.On(Events.Entity.Spawn, cbB);
            _emitter.On(Events.Entity.Spawn, cbC);

            _emitter.Emit(Events.Entity.Spawn);

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("A", results[0]);
            Assert.AreEqual("B", results[1]);
            Assert.AreEqual("C", results[2]);
        }

        [Test]
        public void EmitAfterRemovalSkipsRemovedListener()
        {
            var results = new List<string>();

            _engine.SetValue("results", results);
            var cbA = CreateJsFunction("function() { results.Add('A'); }");
            var cbB = CreateJsFunction("function() { results.Add('B'); }");
            var cbC = CreateJsFunction("function() { results.Add('C'); }");

            _emitter.On(Events.Entity.Spawn, cbA);
            _emitter.On(Events.Entity.Spawn, cbB);
            _emitter.On(Events.Entity.Spawn, cbC);

            _emitter.Off(Events.Entity.Spawn, cbB);
            _emitter.Emit(Events.Entity.Spawn);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("A", results[0]);
            Assert.AreEqual("C", results[1]);
        }

        [Test]
        public void EmitWithNoListenersDoesNotThrow()
        {
            Assert.DoesNotThrow(() => _emitter.Emit(Events.Entity.Spawn));
            Assert.DoesNotThrow(() => _emitter.Emit(null));
            Assert.DoesNotThrow(() => _emitter.Emit(""));
        }

        // --- Emit() Catch-Log-Continue Tests (AC #7) ---

        [Test]
        public void EmitContinuesAfterListenerException()
        {
            var results = new List<string>();
            _engine.SetValue("results", results);

            var cbA = CreateJsFunction("function() { results.Add('A'); }");
            var cbBad = CreateJsFunction("function() { throw new Error('boom'); }");
            var cbC = CreateJsFunction("function() { results.Add('C'); }");

            _emitter.On(Events.Entity.Spawn, cbA);
            _emitter.On(Events.Entity.Spawn, cbBad);
            _emitter.On(Events.Entity.Spawn, cbC);

            // Should not throw — catch-log-continue
            Assert.DoesNotThrow(() => _emitter.Emit(Events.Entity.Spawn));

            // A and C should have fired, B's exception was caught
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("A", results[0]);
            Assert.AreEqual("C", results[1]);
        }

        // --- DisposeAllListeners Tests (AC #8) ---

        [Test]
        public void DisposeAllListenersClearsEverything()
        {
            var cbA = CreateJsFunction("function() { return 'A'; }");
            var cbB = CreateJsFunction("function() { return 'B'; }");

            _emitter.On(Events.Entity.Spawn, cbA);
            _emitter.On(Events.World.Ready, cbB);
            _emitter.Once(Events.Entity.Destroy, cbA);

            _emitter.DisposeAllListeners();

            Assert.AreEqual(0, _emitter.Listeners.Count);
            Assert.AreEqual(0, _emitter.OnceListeners.Count);
        }

        // --- Event Name Validation Tests (AC #9) ---

        [Test]
        public void OnWithUnrecognizedEventNameStillRegisters()
        {
            var callback = CreateJsFunction("function() { return 1; }");

            // "custom:event" is not in Events constants — should warn but register
            _emitter.On("custom:event", callback);

            Assert.IsTrue(_emitter.Listeners.ContainsKey("custom:event"));
            Assert.AreEqual(1, _emitter.Listeners["custom:event"].Count);
        }

        // --- Once + Emit Integration Tests ---

        [Test]
        public void OnceMixedWithOnFiresInRegistrationOrder()
        {
            var results = new List<string>();
            _engine.SetValue("results", results);

            var cbOn = CreateJsFunction("function() { results.Add('on'); }");
            var cbOnce = CreateJsFunction("function() { results.Add('once'); }");
            var cbOn2 = CreateJsFunction("function() { results.Add('on2'); }");

            _emitter.On(Events.Entity.Spawn, cbOn);
            _emitter.Once(Events.Entity.Spawn, cbOnce);
            _emitter.On(Events.Entity.Spawn, cbOn2);

            // First emit — all three fire
            _emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("on", results[0]);
            Assert.AreEqual("once", results[1]);
            Assert.AreEqual("on2", results[2]);

            // Second emit — only the two On() listeners fire
            results.Clear();
            _emitter.Emit(Events.Entity.Spawn);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("on", results[0]);
            Assert.AreEqual("on2", results[1]);
        }

        // --- Patch: Once() does not leak on failed registration ---

        [Test]
        public void OnceDoesNotLeakOnceListenersWhenOnFails()
        {
            ((TestEventEmitter)_emitter).IsDisposed = true;
            var callback = CreateJsFunction("function() { return 1; }");
            _emitter.Once(Events.Entity.Spawn, callback);

            // OnceListeners should NOT contain the callback since On() rejected it
            Assert.AreEqual(0, _emitter.OnceListeners.Count);
            Assert.AreEqual(0, _emitter.Listeners.Count);
        }

        [Test]
        public void OnceDoesNotLeakOnNullEventName()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            _emitter.Once(null, callback);

            Assert.AreEqual(0, _emitter.OnceListeners.Count);
        }

        // --- Patch: Off() only removes from OnceListeners for correct event ---

        [Test]
        public void OffDoesNotRemoveOnceListenerFromDifferentEvent()
        {
            var callback = CreateJsFunction("function() { return 1; }");

            // Register as Once for spawn
            _emitter.Once(Events.Entity.Spawn, callback);
            Assert.IsTrue(_emitter.OnceListeners.Contains(callback));

            // Try to Off() for a different event — should NOT remove from OnceListeners
            _emitter.Off(Events.World.Ready, callback);
            Assert.IsTrue(_emitter.OnceListeners.Contains(callback));
        }

        [Test]
        public void OffRemovesFromOnceListenersWhenActuallyRemoved()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            _emitter.Once(Events.Entity.Spawn, callback);

            _emitter.Off(Events.Entity.Spawn, callback);
            Assert.IsFalse(_emitter.OnceListeners.Contains(callback));
        }

        // --- Patch: Re-entrant Emit() protection ---

        [Test]
        public void EmitPreventsReentrantRecursion()
        {
            // Create a callback that tries to emit the same event recursively
            _engine.SetValue("emitter", _emitter);
            _engine.SetValue("Events", typeof(FiveSQD.WebVerse.Handlers.Javascript.APIs.Core.Events));

            var recursiveCallback = _engine.Evaluate(
                "(function() { emitter.Emit('spawn'); })");

            _emitter.On(Events.Entity.Spawn, recursiveCallback);

            // Should not throw StackOverflowException — re-entrancy guard prevents it
            Assert.DoesNotThrow(() => _emitter.Emit(Events.Entity.Spawn));
        }

        [Test]
        public void EmitClearsEmittingEventsAfterCompletion()
        {
            var callback = CreateJsFunction("function() { return 1; }");
            _emitter.On(Events.Entity.Spawn, callback);

            _emitter.Emit(Events.Entity.Spawn);

            // EmittingEvents should be cleared after emit completes
            Assert.AreEqual(0, _emitter.EmittingEvents.Count);
        }

        [Test]
        public void EmitClearsEmittingEventsEvenAfterException()
        {
            var badCallback = CreateJsFunction("function() { throw new Error('boom'); }");
            _emitter.On(Events.Entity.Spawn, badCallback);

            _emitter.Emit(Events.Entity.Spawn);

            // EmittingEvents should be cleared even when listener throws
            Assert.AreEqual(0, _emitter.EmittingEvents.Count);
        }

        // --- Helper Methods ---

        /// <summary>
        /// Create a JsValue function reference from a JS function expression string.
        /// </summary>
        private JsValue CreateJsFunction(string functionExpression)
        {
            return _engine.Evaluate($"({functionExpression})");
        }
    }
}
