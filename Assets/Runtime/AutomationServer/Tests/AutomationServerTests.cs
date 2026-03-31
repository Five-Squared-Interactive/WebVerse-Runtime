// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Automation;

namespace FiveSQD.WebVerse.Automation.Tests
{
    /// <summary>
    /// Unit tests for AutomationServer and CommandDispatcher (Phase 4A).
    /// </summary>
    [TestFixture]
    public class AutomationServerTests
    {
        // ───────────────────────────────────────────────
        // AutomationServer lifecycle tests
        // ───────────────────────────────────────────────

        [Test]
        public void AutomationServer_Initialize_CreatesServerAndSetsRunning()
        {
            LogAssert.ignoreFailingMessages = true;

            GameObject go = new GameObject("TestAutomationServer");
            AutomationServer server = go.AddComponent<AutomationServer>();

            Assert.IsNotNull(server);

            // Initialize on a high port to avoid conflicts.
            Assert.DoesNotThrow(() => server.Initialize(18900));

            // Cleanup.
            server.Terminate();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void AutomationServer_Terminate_AfterInitialize_DoesNotThrow()
        {
            LogAssert.ignoreFailingMessages = true;

            GameObject go = new GameObject("TestAutomationServer");
            AutomationServer server = go.AddComponent<AutomationServer>();

            server.Initialize(18901);
            Assert.DoesNotThrow(() => server.Terminate());

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AutomationServer_Terminate_WithoutInitialize_DoesNotThrow()
        {
            LogAssert.ignoreFailingMessages = true;

            GameObject go = new GameObject("TestAutomationServer");
            AutomationServer server = go.AddComponent<AutomationServer>();

            // Terminate without calling Initialize should not throw.
            Assert.DoesNotThrow(() => server.Terminate());

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AutomationServer_DoubleTerminate_DoesNotThrow()
        {
            LogAssert.ignoreFailingMessages = true;

            GameObject go = new GameObject("TestAutomationServer");
            AutomationServer server = go.AddComponent<AutomationServer>();

            server.Initialize(18902);
            server.Terminate();

            // Second terminate should be safe.
            Assert.DoesNotThrow(() => server.Terminate());

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AutomationServer_DestroyGameObject_CallsTerminate()
        {
            LogAssert.ignoreFailingMessages = true;

            GameObject go = new GameObject("TestAutomationServer");
            AutomationServer server = go.AddComponent<AutomationServer>();

            server.Initialize(18903);

            // Destroying the GameObject triggers OnDestroy which calls Terminate.
            Assert.DoesNotThrow(() => Object.DestroyImmediate(go));
        }

        // ───────────────────────────────────────────────
        // EscapeJson tests
        // ───────────────────────────────────────────────

        [Test]
        public void EscapeJson_NullOrEmpty_ReturnsEmpty()
        {
            LogAssert.ignoreFailingMessages = true;

            Assert.AreEqual("", AutomationServer.EscapeJson(null));
            Assert.AreEqual("", AutomationServer.EscapeJson(""));
        }

        [Test]
        public void EscapeJson_PlainString_ReturnsUnchanged()
        {
            LogAssert.ignoreFailingMessages = true;

            Assert.AreEqual("hello world", AutomationServer.EscapeJson("hello world"));
        }

        [Test]
        public void EscapeJson_SpecialCharacters_AreEscaped()
        {
            LogAssert.ignoreFailingMessages = true;

            Assert.AreEqual("line1\\nline2", AutomationServer.EscapeJson("line1\nline2"));
            Assert.AreEqual("col1\\tcol2", AutomationServer.EscapeJson("col1\tcol2"));
            Assert.AreEqual("say \\\"hi\\\"", AutomationServer.EscapeJson("say \"hi\""));
            Assert.AreEqual("back\\\\slash", AutomationServer.EscapeJson("back\\slash"));
            Assert.AreEqual("cr\\r", AutomationServer.EscapeJson("cr\r"));
        }

        // ───────────────────────────────────────────────
        // CommandDispatcher tests
        // ───────────────────────────────────────────────

        [Test]
        public void CommandDispatcher_Dispatch_OptionsMethod_Returns204()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("OPTIONS", "/api/v1/status", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(204, responseStatusCode);
            Assert.AreEqual("", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_UnknownRoute_Returns404()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("GET", "/api/v1/nonexistent", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(404, responseStatusCode);
            StringAssert.Contains("Unknown route", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_UnknownRouteWithoutPrefix_Returns404()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("GET", "/totally/unknown", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(404, responseStatusCode);
            StringAssert.Contains("Unknown route", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_StatusRoute_NoRuntime_ReturnsNotInitialized()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("GET", "/api/v1/status", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(200, responseStatusCode);
            Assert.AreEqual("application/json", responseContentType);
            StringAssert.Contains("not_initialized", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_WorldLoadRoute_MissingUrl_Returns400()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("POST", "/api/v1/world/load", "{}",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(400, responseStatusCode);
            StringAssert.Contains("Missing 'url'", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_WorldLoadRoute_NoRuntime_Returns503()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("POST", "/api/v1/world/load", "{\"url\": \"https://example.com\"}",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(503, responseStatusCode);
            StringAssert.Contains("Runtime not initialized", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_WorldUnloadRoute_NoRuntime_Returns503()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("POST", "/api/v1/world/unload", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(503, responseStatusCode);
            StringAssert.Contains("Runtime not initialized", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_WorldStateRoute_NoRuntime_ReturnsNotInitialized()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("GET", "/api/v1/world/state", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(200, responseStatusCode);
            StringAssert.Contains("not_initialized", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_EntitiesRoute_NoRuntime_ReturnsEmptyList()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("GET", "/api/v1/entities", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(200, responseStatusCode);
            StringAssert.Contains("\"entities\": []", responseBody);
            StringAssert.Contains("\"count\": 0", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_ScriptRunRoute_MissingScript_Returns400()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("POST", "/api/v1/script/run", "{}",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(400, responseStatusCode);
            StringAssert.Contains("Missing 'script'", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_ScriptRunRoute_NoRuntime_Returns503()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("POST", "/api/v1/script/run", "{\"script\": \"console.log('hi')\"}",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(503, responseStatusCode);
            StringAssert.Contains("not initialized", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_EntityRoute_NoRuntime_Returns404()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("GET", "/api/v1/entity/some-id", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(404, responseStatusCode);
            StringAssert.Contains("No world loaded", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_TrailingSlash_IsStripped()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            // /status with trailing slash should behave the same as /status.
            dispatcher.Dispatch("GET", "/api/v1/status/", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual(200, responseStatusCode);
            StringAssert.Contains("not_initialized", responseBody);
        }

        [Test]
        public void CommandDispatcher_Dispatch_ResponseContentType_IsJson()
        {
            LogAssert.ignoreFailingMessages = true;

            CommandDispatcher dispatcher = new CommandDispatcher();

            dispatcher.Dispatch("GET", "/api/v1/status", "",
                out string responseBody, out string responseContentType,
                out byte[] responseBytes, out int responseStatusCode);

            Assert.AreEqual("application/json", responseContentType);
            Assert.IsNull(responseBytes);
        }
    }
}
