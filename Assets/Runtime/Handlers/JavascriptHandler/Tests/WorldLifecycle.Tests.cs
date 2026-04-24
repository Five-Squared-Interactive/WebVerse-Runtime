// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Jint;
using Jint.Native;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;

// Alias to avoid ambiguity with Jint types
using WorldAPI = FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities.World;

namespace FiveSQD.WebVerse.Handlers.Javascript.Tests
{
    /// <summary>
    /// Tests for World static event methods and lifecycle event emission.
    /// </summary>
    [TestFixture]
    public class WorldLifecycleTests
    {
        private Engine _engine;
        private List<string> _results;

        [SetUp]
        public void SetUp()
        {
            _engine = new Engine();
            _results = new List<string>();
            _engine.SetValue("results", _results);
            WorldAPI.DisposeAllWorldListeners();
        }

        [TearDown]
        public void TearDown()
        {
            WorldAPI.DisposeAllWorldListeners();
        }

        // --- on() Registration Tests ---

        [Test]
        public void OnRegistersListenerAndReturnsUnsubscribe()
        {
            var callback = CreateJsFunction("function() { results.Add('ready'); }");
            var unsub = WorldAPI.on(Events.World.Ready, callback);

            Assert.IsNotNull(unsub);
            Assert.IsTrue(unsub is Func<bool>);
        }

        [Test]
        public void OnRegistersListenerThatFiresOnEmit()
        {
            var callback = CreateJsFunction("function() { results.Add('fired'); }");
            WorldAPI.on(Events.World.Ready, callback);

            WorldAPI.Emit(Events.World.Ready);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("fired", _results[0]);
        }

        [Test]
        public void UnsubscribeRemovesListener()
        {
            var callback = CreateJsFunction("function() { results.Add('fired'); }");
            var unsub = WorldAPI.on(Events.World.Ready, callback);

            unsub();
            WorldAPI.Emit(Events.World.Ready);

            Assert.AreEqual(0, _results.Count);
        }

        // --- once() Tests ---

        [Test]
        public void OnceFiresOnceAndAutoRemoves()
        {
            var callback = CreateJsFunction("function() { results.Add('once'); }");
            WorldAPI.once(Events.World.Ready, callback);

            WorldAPI.Emit(Events.World.Ready);
            Assert.AreEqual(1, _results.Count);

            _results.Clear();
            WorldAPI.Emit(Events.World.Ready);
            Assert.AreEqual(0, _results.Count);
        }

        // --- off() Tests ---

        [Test]
        public void OffRemovesSpecificListener()
        {
            var cbA = CreateJsFunction("function() { results.Add('A'); }");
            var cbB = CreateJsFunction("function() { results.Add('B'); }");

            WorldAPI.on(Events.World.Ready, cbA);
            WorldAPI.on(Events.World.Ready, cbB);
            WorldAPI.off(Events.World.Ready, cbA);

            WorldAPI.Emit(Events.World.Ready);

            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("B", _results[0]);
        }

        [Test]
        public void OffAllRemovesAllListenersForEvent()
        {
            var cbA = CreateJsFunction("function() { results.Add('A'); }");
            var cbB = CreateJsFunction("function() { results.Add('B'); }");

            WorldAPI.on(Events.World.Ready, cbA);
            WorldAPI.on(Events.World.Ready, cbB);
            WorldAPI.off(Events.World.Ready);

            WorldAPI.Emit(Events.World.Ready);

            Assert.AreEqual(0, _results.Count);
        }

        // --- Emit() Tests ---

        [Test]
        public void EmitFiresListenersInRegistrationOrder()
        {
            var cbA = CreateJsFunction("function() { results.Add('A'); }");
            var cbB = CreateJsFunction("function() { results.Add('B'); }");
            var cbC = CreateJsFunction("function() { results.Add('C'); }");

            WorldAPI.on(Events.World.Ready, cbA);
            WorldAPI.on(Events.World.Ready, cbB);
            WorldAPI.on(Events.World.Ready, cbC);

            WorldAPI.Emit(Events.World.Ready);

            Assert.AreEqual(3, _results.Count);
            Assert.AreEqual("A", _results[0]);
            Assert.AreEqual("B", _results[1]);
            Assert.AreEqual("C", _results[2]);
        }

        [Test]
        public void EmitCatchLogContinueOnListenerException()
        {
            var cbA = CreateJsFunction("function() { results.Add('A'); }");
            var cbBad = CreateJsFunction("function() { throw new Error('boom'); }");
            var cbC = CreateJsFunction("function() { results.Add('C'); }");

            WorldAPI.on(Events.World.Ready, cbA);
            WorldAPI.on(Events.World.Ready, cbBad);
            WorldAPI.on(Events.World.Ready, cbC);

            Assert.DoesNotThrow(() => WorldAPI.Emit(Events.World.Ready));

            Assert.AreEqual(2, _results.Count);
            Assert.AreEqual("A", _results[0]);
            Assert.AreEqual("C", _results[1]);
        }

        [Test]
        public void EmitWithNoListenersDoesNotThrow()
        {
            Assert.DoesNotThrow(() => WorldAPI.Emit(Events.World.Ready));
            Assert.DoesNotThrow(() => WorldAPI.Emit("nonexistent"));
        }

        // --- DisposeAllWorldListeners Tests ---

        [Test]
        public void DisposeAllWorldListenersClearsEverything()
        {
            var cbA = CreateJsFunction("function() { results.Add('A'); }");
            var cbB = CreateJsFunction("function() { results.Add('B'); }");

            WorldAPI.on(Events.World.Ready, cbA);
            WorldAPI.once(Events.World.Load, cbB);

            WorldAPI.DisposeAllWorldListeners();

            WorldAPI.Emit(Events.World.Ready);
            WorldAPI.Emit(Events.World.Load);

            Assert.AreEqual(0, _results.Count);
        }

        // --- String vs Constants Tests ---

        [Test]
        public void StringLiteralAndEventsConstantRegisterIdentically()
        {
            var cbConst = CreateJsFunction("function() { results.Add('const'); }");
            var cbString = CreateJsFunction("function() { results.Add('string'); }");

            WorldAPI.on(Events.World.Ready, cbConst);
            WorldAPI.on("ready", cbString);

            WorldAPI.Emit(Events.World.Ready);

            Assert.AreEqual(2, _results.Count);
            Assert.AreEqual("const", _results[0]);
            Assert.AreEqual("string", _results[1]);
        }

        // --- apiVersion Tests ---

        [Test]
        public void ApiVersionReturnsExpectedString()
        {
            Assert.AreEqual("1.0.0", WorldAPI.apiVersion);
        }

        // --- All Three Event Types ---

        [Test]
        public void LoadReadyErrorEventsAreIndependent()
        {
            var cbLoad = CreateJsFunction("function() { results.Add('load'); }");
            var cbReady = CreateJsFunction("function() { results.Add('ready'); }");
            var cbError = CreateJsFunction("function() { results.Add('error'); }");

            WorldAPI.on(Events.World.Load, cbLoad);
            WorldAPI.on(Events.World.Ready, cbReady);
            WorldAPI.on(Events.World.Error, cbError);

            WorldAPI.Emit(Events.World.Load);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("load", _results[0]);

            _results.Clear();
            WorldAPI.Emit(Events.World.Ready);
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("ready", _results[0]);

            _results.Clear();
            WorldAPI.Emit(Events.World.Error, JsValue.FromObject(_engine, "test error"));
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("error", _results[0]);
        }

        // --- Error Payload Tests (Review Patch) ---

        [Test]
        public void ErrorEmitDeliversArgsToListener()
        {
            var argsReceived = new List<JsValue>();
            _engine.SetValue("argsReceived", argsReceived);

            var callback = _engine.Evaluate(
                "(function(errorInfo) { argsReceived.Add(errorInfo); })");

            WorldAPI.on(Events.World.Error, callback);

            // Emit with an error object — mirrors what WebVerseRuntime does
            var errorObj = JsValue.FromObject(_engine, new { message = "Test error" });
            WorldAPI.Emit(Events.World.Error, errorObj);

            Assert.AreEqual(1, argsReceived.Count);
            // Verify the arg was delivered (non-null, non-undefined)
            Assert.IsFalse(argsReceived[0].IsNull());
            Assert.IsFalse(argsReceived[0].IsUndefined());
        }

        [Test]
        public void ReadyEmitWithNoArgsDeliversNoArgs()
        {
            var callCount = 0;
            _engine.SetValue("incrementCount", new Action(() => callCount++));

            var callback = _engine.Evaluate("(function() { incrementCount(); })");
            WorldAPI.on(Events.World.Ready, callback);

            WorldAPI.Emit(Events.World.Ready);

            Assert.AreEqual(1, callCount);
        }

        // --- Load Event Timing Test (Review Patch) ---

        [Test]
        public void LoadEventFiresBeforeListenersAreDisposed()
        {
            // Register a load listener
            var callback = CreateJsFunction("function() { results.Add('load-heard'); }");
            WorldAPI.on(Events.World.Load, callback);

            // Emit load (simulating what WebVerseRuntime does before dispose)
            WorldAPI.Emit(Events.World.Load);

            // Listener should have fired BEFORE any dispose
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("load-heard", _results[0]);

            // Now dispose (simulating what happens after load emit)
            WorldAPI.DisposeAllWorldListeners();

            // Verify listeners are cleared
            _results.Clear();
            WorldAPI.Emit(Events.World.Load);
            Assert.AreEqual(0, _results.Count);
        }

        // --- Helper Methods ---

        private JsValue CreateJsFunction(string functionExpression)
        {
            return _engine.Evaluate($"({functionExpression})");
        }
    }
}
