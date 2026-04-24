// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Jint;
using Jint.Native;

using InputAPI = FiveSQD.WebVerse.Handlers.Javascript.APIs.Input.Input;

namespace FiveSQD.WebVerse.Handlers.Javascript.Tests
{
    [TestFixture]
    public class InputEventsTests
    {
        private Engine _engine;
        private List<string> _results;

        [SetUp]
        public void SetUp()
        {
            _engine = new Engine();
            _results = new List<string>();
            _engine.SetValue("results", _results);
            InputAPI.DisposeAllInputListeners();
        }

        [TearDown]
        public void TearDown()
        {
            InputAPI.DisposeAllInputListeners();
        }

        [Test]
        public void OnRegistersAndEmitFires()
        {
            var cb = _engine.Evaluate("(function() { results.Add('keydown'); })");
            InputAPI.on("keydown", cb);
            InputAPI.Emit("keydown");
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("keydown", _results[0]);
        }

        [Test]
        public void MultipleListenersFireInOrder()
        {
            var cbA = _engine.Evaluate("(function() { results.Add('A'); })");
            var cbB = _engine.Evaluate("(function() { results.Add('B'); })");
            InputAPI.on("keydown", cbA);
            InputAPI.on("keydown", cbB);
            InputAPI.Emit("keydown");
            Assert.AreEqual(2, _results.Count);
            Assert.AreEqual("A", _results[0]);
            Assert.AreEqual("B", _results[1]);
        }

        [Test]
        public void UnsubscribeRemovesListener()
        {
            var cb = _engine.Evaluate("(function() { results.Add('fired'); })");
            var unsub = InputAPI.on("keydown", cb);
            unsub();
            InputAPI.Emit("keydown");
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void OnceAutoRemoves()
        {
            var cb = _engine.Evaluate("(function() { results.Add('once'); })");
            InputAPI.once("keydown", cb);
            InputAPI.Emit("keydown");
            Assert.AreEqual(1, _results.Count);
            _results.Clear();
            InputAPI.Emit("keydown");
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void OffRemovesSpecific()
        {
            var cbA = _engine.Evaluate("(function() { results.Add('A'); })");
            var cbB = _engine.Evaluate("(function() { results.Add('B'); })");
            InputAPI.on("keydown", cbA);
            InputAPI.on("keydown", cbB);
            InputAPI.off("keydown", cbA);
            InputAPI.Emit("keydown");
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("B", _results[0]);
        }

        [Test]
        public void OffAllRemovesAllForEvent()
        {
            var cbA = _engine.Evaluate("(function() { results.Add('A'); })");
            var cbB = _engine.Evaluate("(function() { results.Add('B'); })");
            InputAPI.on("keydown", cbA);
            InputAPI.on("keydown", cbB);
            InputAPI.off("keydown");
            InputAPI.Emit("keydown");
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void CatchLogContinue()
        {
            var good = _engine.Evaluate("(function() { results.Add('good'); })");
            var bad = _engine.Evaluate("(function() { throw new Error('boom'); })");
            InputAPI.on("keydown", good);
            InputAPI.on("keydown", bad);
            InputAPI.on("keydown", good);
            Assert.DoesNotThrow(() => InputAPI.Emit("keydown"));
            Assert.AreEqual(2, _results.Count);
        }

        [Test]
        public void DisposeAllClearsEverything()
        {
            var cb = _engine.Evaluate("(function() { results.Add('x'); })");
            InputAPI.on("keydown", cb);
            InputAPI.on("mousedown", cb);
            InputAPI.DisposeAllInputListeners();
            InputAPI.Emit("keydown");
            InputAPI.Emit("mousedown");
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void DifferentInputEventsAreIndependent()
        {
            var cbKey = _engine.Evaluate("(function() { results.Add('key'); })");
            var cbMouse = _engine.Evaluate("(function() { results.Add('mouse'); })");
            InputAPI.on("keydown", cbKey);
            InputAPI.on("mousedown", cbMouse);
            InputAPI.Emit("keydown");
            Assert.AreEqual(1, _results.Count);
            Assert.AreEqual("key", _results[0]);
        }
    }
}
