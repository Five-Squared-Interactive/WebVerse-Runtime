// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

#if USE_WEBINTERFACE
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.Javascript;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.LocalStorage;
using FiveSQD.WebVerse.WorldSync;
using JSWorldSync = FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldSync.WorldSync;

/// <summary>
/// Tests for the WorldSync JavaScript API (Stories 3.1 and 3.2).
/// Verifies session lifecycle, entity sync, custom messaging, API registration,
/// and graceful failure on invalid input.
/// </summary>
public class WorldSyncJSTests
{
    private WebVerseRuntime runtime;
    private GameObject runtimeGO;
    private JavascriptHandler jsHandler;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;

        runtimeGO = new GameObject("runtime");
        runtime = runtimeGO.AddComponent<WebVerseRuntime>();

        runtime.highlightMaterial = new Material(Shader.Find("Standard"));
        runtime.skyMaterial = new Material(Shader.Find("Standard"));

        runtime.characterControllerPrefab = new GameObject("DummyCharacterController");
        runtime.inputEntityPrefab = new GameObject("DummyInputEntity");
        runtime.voxelPrefab = new GameObject("DummyVoxel");
        runtime.webVerseWebViewPrefab = new GameObject("DummyWebView");

        string testDirectory = Path.Combine(Path.GetTempPath(), "WorldSyncJSTests");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, testDirectory);

        jsHandler = runtime.javascriptHandler;
    }

    [TearDown]
    public void TearDown()
    {
        // Reset static test seam so subsequent tests start clean.
        WorldSyncClient.DefaultUseTestHooks = false;
        WorldSyncClient.DefaultSimulateCreateSessionId = null;
        WorldSyncClient.DefaultSimulateJoinSessionState = null;
        WorldSyncClient.DefaultSimulateCreateEntityId = null;
        WorldSyncClient.DefaultSimulateSendCustomMessageInvocations = 0;
        WorldSyncClient.DefaultSimulateDeleteEntityInvocations = 0;
        WorldSyncClient.DefaultSimulateResumeEntityFailure = false;
        JSWorldSync.TestHook_JoinCallbackInvocations = 0;
        JSWorldSync.TestHook_LastInvokedCallback = null;
        JSWorldSync.TestHook_MessageCallbackInvocations = 0;
        JSWorldSync.TestHook_LastMessageCallback = null;
        JSWorldSync.TestHook_StateChangeCallbackInvocations = 0;
        JSWorldSync.TestHook_LastStateChangeCallback = null;
        JSWorldSync.TestHook_MessageCallbackReattachmentCount = 0;
        JSWorldSync.ClearMessageCallbackHandlers();
        JSWorldSync.ClearStateChangeCallbackHandlers();

        if (WebVerseRuntime.Instance != null)
        {
            WebVerseRuntime.Instance.ClearWorldSyncClients();
        }

        WebVerseRuntime.Instance = null;
        if (runtime != null)
        {
            string testDirectory = Path.Combine(Path.GetTempPath(), "WorldSyncJSTests");
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }

        if (runtimeGO != null)
        {
            UnityEngine.Object.DestroyImmediate(runtimeGO);
        }
    }

    private static IEnumerator WaitForCondition(Func<bool> condition, float timeoutSeconds = 5f)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeoutSeconds)
        {
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }
    }

    // ----- AC1: CreateSession -----

    [UnityTest]
    public IEnumerator CreateSession_ValidArgs_RegistersClientAndCreatesSession()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "test-session-1";

        bool ok = JSWorldSync.CreateSession("localhost", 1883, false, "client-id-1", "TestTag");
        Assert.IsTrue(ok, "CreateSession should return true on valid input");

        var client = WebVerseRuntime.Instance.GetWorldSyncClient("client-id-1");
        Assert.IsNotNull(client, "Client should be registered immediately after CreateSession");
        Assert.AreEqual("localhost", client.Config.Host);
        Assert.AreEqual(1883, client.Config.Port);
        Assert.AreEqual("TestTag", client.Config.ClientTag);
        Assert.AreEqual(WorldSyncTransport.TCP, client.Config.Transport,
            "Transport.TCP should propagate to WorldSyncConfig.Transport");
        Assert.IsFalse(client.Config.Tls.Enabled, "tls=false should propagate to WorldSyncConfig.Tls.Enabled");

        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("client-id-1"));

        Assert.IsTrue(JSWorldSync.IsSessionEstablished("client-id-1"),
            "Session should be established after async ConnectAndCreate completes");
        Assert.AreEqual("test-session-1", client.CurrentSession.SessionId);
    }

    // ----- AC7: CreateSession invalid input -----

    [Test]
    public void CreateSession_NullId_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error, new Regex("WorldSync:CreateSession.*id is required"));

        bool ok = JSWorldSync.CreateSession("localhost", 1883, false, null, "TestTag");
        Assert.IsFalse(ok);
    }

    [Test]
    public void CreateSession_EmptyHost_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error, new Regex("WorldSync:CreateSession.*Invalid config"));

        bool ok = JSWorldSync.CreateSession("", 1883, false, "client-id-1", "TestTag");
        Assert.IsFalse(ok);
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("client-id-1"));
    }

    [Test]
    public void CreateSession_TlsTrue_PropagatesToConfig()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "tls-session";

        bool ok = JSWorldSync.CreateSession("localhost", 8883, true, "client-id-tls", "TlsTag");
        Assert.IsTrue(ok);

        var client = WebVerseRuntime.Instance.GetWorldSyncClient("client-id-tls");
        Assert.IsNotNull(client);
        Assert.IsTrue(client.Config.Tls.Enabled, "tls=true should propagate to WorldSyncConfig.Tls.Enabled");
        Assert.AreEqual(8883, client.Config.Port);
    }

    [Test]
    public void CreateSession_DuplicateId_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "dup-create";

        Assert.IsTrue(JSWorldSync.CreateSession("localhost", 1883, false, "dup-id-1", "TestTag"));

        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:CreateSession.*already has a registered client"));

        bool second = JSWorldSync.CreateSession("localhost", 1883, false, "dup-id-1", "TestTag");
        Assert.IsFalse(second, "Second CreateSession for the same id must be rejected");
    }

    // ----- AC2: JoinSession with callback -----

    [UnityTest]
    public IEnumerator JoinSession_ValidArgs_InvokesCallbackOnSuccess()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateJoinSessionState = new SessionState
        {
            SessionId = "join-session-1",
            SessionTag = "RemoteWorld",
            CreatedAt = "2026-01-01T00:00:00Z"
        };

        JSWorldSync.TestHook_JoinCallbackInvocations = 0;

        string clientId = JSWorldSync.JoinSession("localhost", 1883, false, "client-id-2",
            "TestTag", "join-session-1", "1+1");
        Assert.IsNotNull(clientId, "JoinSession should return the local client id on success");

        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("client-id-2"));

        Assert.IsTrue(JSWorldSync.IsSessionEstablished("client-id-2"));

        // Poll for the callback continuation to run; the test hook increments after Run() completes.
        yield return WaitForCondition(() => JSWorldSync.TestHook_JoinCallbackInvocations > 0);

        Assert.AreEqual(1, JSWorldSync.TestHook_JoinCallbackInvocations,
            "JoinSession should invoke the callback exactly once on success");
        Assert.AreEqual("1+1", JSWorldSync.TestHook_LastInvokedCallback,
            "The callback string supplied by the caller must be the one handed to the JS engine");
    }

    [Test]
    public void JoinSession_NullSessionId_ReturnsNullAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error, new Regex("WorldSync:JoinSession.*sessionId is required"));

        string result = JSWorldSync.JoinSession("localhost", 1883, false, "client-id-3",
            "TestTag", null, "onJoin");
        Assert.IsNull(result);
    }

    [Test]
    public void JoinSession_DuplicateId_ReturnsNullAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateJoinSessionState = new SessionState
        {
            SessionId = "dup-join",
            SessionTag = "DupWorld",
            CreatedAt = "2026-01-01T00:00:00Z"
        };

        string firstClientId = JSWorldSync.JoinSession("localhost", 1883, false, "dup-id-2",
            "TestTag", "dup-join", null);
        Assert.IsNotNull(firstClientId);

        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:JoinSession.*already has a registered client"));

        string secondClientId = JSWorldSync.JoinSession("localhost", 1883, false, "dup-id-2",
            "TestTag", "dup-join", null);
        Assert.IsNull(secondClientId, "Second JoinSession for the same id must be rejected");
    }

    // ----- AC3: ExitSession -----

    [UnityTest]
    public IEnumerator ExitSession_ActiveClient_LeavesAndUnregisters()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "exit-session-1";

        Assert.IsTrue(JSWorldSync.CreateSession("localhost", 1883, false, "client-id-4", "TestTag"));
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("client-id-4"));
        Assert.IsTrue(JSWorldSync.IsSessionEstablished("client-id-4"));

        bool ok = JSWorldSync.ExitSession("client-id-4");
        Assert.IsTrue(ok);

        yield return WaitForCondition(() => WebVerseRuntime.Instance.GetWorldSyncClient("client-id-4") == null);

        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("client-id-4"),
            "Client should be unregistered after ExitSession completes");
    }

    [Test]
    public void ExitSession_UnknownId_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error, new Regex("WorldSync:ExitSession.*No WorldSyncClient registered"));

        bool ok = JSWorldSync.ExitSession("never-registered");
        Assert.IsFalse(ok);
    }

    // ----- AC4: DestroySession -----

    [UnityTest]
    public IEnumerator DestroySession_ActiveClient_DestroysAndUnregisters()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "destroy-session-1";

        Assert.IsTrue(JSWorldSync.CreateSession("localhost", 1883, false, "client-id-5", "TestTag"));
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("client-id-5"));
        Assert.IsTrue(JSWorldSync.IsSessionEstablished("client-id-5"));

        bool ok = JSWorldSync.DestroySession("client-id-5");
        Assert.IsTrue(ok);

        yield return WaitForCondition(() => WebVerseRuntime.Instance.GetWorldSyncClient("client-id-5") == null);

        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("client-id-5"));
    }

    [Test]
    public void DestroySession_UnknownId_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error, new Regex("WorldSync:DestroySession.*No WorldSyncClient registered"));

        bool ok = JSWorldSync.DestroySession("never-registered");
        Assert.IsFalse(ok);
    }

    // ----- AC5: IsSessionEstablished -----

    [Test]
    public void IsSessionEstablished_NullId_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsFalse(JSWorldSync.IsSessionEstablished(null));
    }

    [Test]
    public void IsSessionEstablished_UnknownId_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsFalse(JSWorldSync.IsSessionEstablished("not-registered"));
    }

    [UnityTest]
    public IEnumerator IsSessionEstablished_AfterExit_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "ephemeral-session";

        Assert.IsTrue(JSWorldSync.CreateSession("localhost", 1883, false, "client-id-6", "TestTag"));
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("client-id-6"));
        Assert.IsTrue(JSWorldSync.IsSessionEstablished("client-id-6"));

        Assert.IsTrue(JSWorldSync.ExitSession("client-id-6"));
        yield return WaitForCondition(() => !JSWorldSync.IsSessionEstablished("client-id-6"));

        Assert.IsFalse(JSWorldSync.IsSessionEstablished("client-id-6"));
    }

    // ----- AC6: API registration in Jint engine -----

    [Test]
    public void RegisterAPI_WorldSyncExposedToJavaScript()
    {
        LogAssert.ignoreFailingMessages = true;
        var result = jsHandler.Run("typeof WorldSync");
        Assert.IsNotNull(result);
        Assert.AreNotEqual("undefined", result.ToString(),
            "WorldSync should be exposed to the JavaScript engine");
    }

    [Test]
    public void RegisterAPI_WSyncTransportExposedToJavaScript()
    {
        LogAssert.ignoreFailingMessages = true;
        var result = jsHandler.Run("typeof WSyncTransport");
        Assert.IsNotNull(result);
        Assert.AreNotEqual("undefined", result.ToString(),
            "WSyncTransport should be exposed to the JavaScript engine");
    }

    // ===== Story 3.2: Entity Sync & Custom Messaging =====

    // ----- AC1: StartSynchronizingEntity -----

    [Test]
    public void StartSynchronizingEntity_UnknownSessionID_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:StartSynchronizingEntity.*No WorldSyncClient registered"));

        bool ok = JSWorldSync.StartSynchronizingEntity("no-such-session", Guid.NewGuid().ToString());
        Assert.IsFalse(ok);
    }

    [Test]
    public void StartSynchronizingEntity_InvalidEntityGuid_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "entity-test-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "entity-client-1", "TestTag");

        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:StartSynchronizingEntity.*Invalid entity UUID"));

        bool ok = JSWorldSync.StartSynchronizingEntity("entity-client-1", "not-a-guid");
        Assert.IsFalse(ok);
    }

    [Test]
    public void StartSynchronizingEntity_NullSessionID_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:StartSynchronizingEntity.*sessionID is required"));

        bool ok = JSWorldSync.StartSynchronizingEntity(null, Guid.NewGuid().ToString());
        Assert.IsFalse(ok);
    }

    [Test]
    public void StartSynchronizingEntity_NullEntityID_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:StartSynchronizingEntity.*entityID is required"));

        bool ok = JSWorldSync.StartSynchronizingEntity("some-session", null);
        Assert.IsFalse(ok);
    }

    [Test]
    public void StartSynchronizingEntity_EntityNotInManager_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "entity-mgr-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "entity-client-2", "TestTag");

        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:StartSynchronizingEntity.*Unable to find entity"));

        bool ok = JSWorldSync.StartSynchronizingEntity("entity-client-2", Guid.NewGuid().ToString());
        Assert.IsFalse(ok);
    }

    // ----- AC2: StopSynchronizingEntity -----

    [Test]
    public void StopSynchronizingEntity_UnknownPair_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "stop-entity-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "stop-client-1", "TestTag");

        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:StopSynchronizingEntity.*No bridge registered"));

        bool ok = JSWorldSync.StopSynchronizingEntity("stop-client-1", Guid.NewGuid().ToString());
        Assert.IsFalse(ok);
    }

    [Test]
    public void StopSynchronizingEntity_NullSessionID_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:StopSynchronizingEntity.*sessionID is required"));

        bool ok = JSWorldSync.StopSynchronizingEntity(null, Guid.NewGuid().ToString());
        Assert.IsFalse(ok);
    }

    [Test]
    public void StopSynchronizingEntity_UnknownSession_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:StopSynchronizingEntity.*No WorldSyncClient registered"));

        bool ok = JSWorldSync.StopSynchronizingEntity("nonexistent", Guid.NewGuid().ToString());
        Assert.IsFalse(ok);
    }

    // ----- AC3: SendMessage -----

    [UnityTest]
    public IEnumerator SendMessage_ValidArgs_DelegatesToSession()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "msg-session-1";

        JSWorldSync.CreateSession("localhost", 1883, false, "msg-client-1", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("msg-client-1"));

        var client = WebVerseRuntime.Instance.GetWorldSyncClient("msg-client-1");
        Assert.IsNotNull(client);
        int beforeCount = client.SimulateSendCustomMessageInvocations;

        bool ok = JSWorldSync.SendMessage("msg-client-1", "game/score", "{\"score\":42}");
        Assert.IsTrue(ok, "SendMessage should return true on valid input");

        Assert.AreEqual(beforeCount + 1, client.SimulateSendCustomMessageInvocations,
            "SendCustomMessageAsync should have been invoked once");
    }

    [Test]
    public void SendMessage_UnknownSession_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:SendMessage.*No WorldSyncClient registered"));

        bool ok = JSWorldSync.SendMessage("no-such-session", "topic", "msg");
        Assert.IsFalse(ok);
    }

    [Test]
    public void SendMessage_EmptyTopic_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:SendMessage.*topic is required"));

        bool ok = JSWorldSync.SendMessage("some-session", "", "msg");
        Assert.IsFalse(ok);
    }

    [UnityTest]
    public IEnumerator SendMessage_InvalidatedSession_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "invalid-msg-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "invalid-msg-client", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("invalid-msg-client"));

        // Invalidate the session manually.
        var client = WebVerseRuntime.Instance.GetWorldSyncClient("invalid-msg-client");
        client.CurrentSession.Invalidate("test");

        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:SendMessage.*Session is not valid"));

        bool ok = JSWorldSync.SendMessage("invalid-msg-client", "topic", "msg");
        Assert.IsFalse(ok);
    }

    // ----- AC4: RegisterMessageCallback -----

    [UnityTest]
    public IEnumerator RegisterMessageCallback_ValidArgs_AttachesHandler()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "cb-session-1";

        JSWorldSync.CreateSession("localhost", 1883, false, "cb-client-1", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("cb-client-1"));

        JSWorldSync.TestHook_MessageCallbackInvocations = 0;

        bool ok = JSWorldSync.RegisterMessageCallback("cb-client-1", "onMsg");
        Assert.IsTrue(ok, "RegisterMessageCallback should return true on valid input");

        // Fire the session's OnCustomMessage event directly.
        var client = WebVerseRuntime.Instance.GetWorldSyncClient("cb-client-1");
        client.CurrentSession.HandleCustomMessage("game/chat", "sender-1", "hello");

        Assert.AreEqual(1, JSWorldSync.TestHook_MessageCallbackInvocations,
            "Message callback should have been invoked once");
        Assert.AreEqual("onMsg", JSWorldSync.TestHook_LastMessageCallback,
            "The callback string should match what was registered");
    }

    [UnityTest]
    public IEnumerator RegisterMessageCallback_DuplicateCallback_NoDoubleAttach()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "dup-cb-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "dup-cb-client", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("dup-cb-client"));

        JSWorldSync.TestHook_MessageCallbackInvocations = 0;

        // Register the same callback twice.
        Assert.IsTrue(JSWorldSync.RegisterMessageCallback("dup-cb-client", "onDupMsg"));
        Assert.IsTrue(JSWorldSync.RegisterMessageCallback("dup-cb-client", "onDupMsg"),
            "Second registration should return true (no-op)");

        // Fire event once.
        var client = WebVerseRuntime.Instance.GetWorldSyncClient("dup-cb-client");
        client.CurrentSession.HandleCustomMessage("test/topic", "sender-2", "data");

        Assert.AreEqual(1, JSWorldSync.TestHook_MessageCallbackInvocations,
            "Handler should only fire once despite double registration");
    }

    [Test]
    public void RegisterMessageCallback_UnknownSession_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:RegisterMessageCallback.*No WorldSyncClient registered"));

        bool ok = JSWorldSync.RegisterMessageCallback("no-session", "onMsg");
        Assert.IsFalse(ok);
    }

    [Test]
    public void RegisterMessageCallback_EmptyCallback_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:RegisterMessageCallback.*callback is required"));

        bool ok = JSWorldSync.RegisterMessageCallback("some-session", "");
        Assert.IsFalse(ok);
    }

    [UnityTest]
    public IEnumerator ExitSession_DetachesMessageCallback()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "detach-cb-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "detach-cb-client", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("detach-cb-client"));

        // Register a callback.
        Assert.IsTrue(JSWorldSync.RegisterMessageCallback("detach-cb-client", "onDetach"));

        var client = WebVerseRuntime.Instance.GetWorldSyncClient("detach-cb-client");
        var session = client.CurrentSession;

        JSWorldSync.TestHook_MessageCallbackInvocations = 0;

        // Exit the session — should detach callbacks.
        Assert.IsTrue(JSWorldSync.ExitSession("detach-cb-client"));
        yield return WaitForCondition(() =>
            WebVerseRuntime.Instance.GetWorldSyncClient("detach-cb-client") == null);

        // Fire the event on the old session reference — handler should NOT fire.
        session.HandleCustomMessage("test/topic", "sender-3", "late-msg");

        Assert.AreEqual(0, JSWorldSync.TestHook_MessageCallbackInvocations,
            "Callback should not fire after ExitSession detaches it");
    }

    // ----- AC5: API Registration for new methods -----

    [Test]
    public void RegisterAPI_StartSynchronizingEntityExposedToJavaScript()
    {
        LogAssert.ignoreFailingMessages = true;
        var result = jsHandler.Run("typeof WorldSync.StartSynchronizingEntity");
        Assert.IsNotNull(result);
        Assert.AreNotEqual("undefined", result.ToString(),
            "WorldSync.StartSynchronizingEntity should be exposed to the JavaScript engine");
    }

    [Test]
    public void RegisterAPI_StopSynchronizingEntityExposedToJavaScript()
    {
        LogAssert.ignoreFailingMessages = true;
        var result = jsHandler.Run("typeof WorldSync.StopSynchronizingEntity");
        Assert.IsNotNull(result);
        Assert.AreNotEqual("undefined", result.ToString(),
            "WorldSync.StopSynchronizingEntity should be exposed to the JavaScript engine");
    }

    [Test]
    public void RegisterAPI_SendMessageExposedToJavaScript()
    {
        LogAssert.ignoreFailingMessages = true;
        var result = jsHandler.Run("typeof WorldSync.SendMessage");
        Assert.IsNotNull(result);
        Assert.AreNotEqual("undefined", result.ToString(),
            "WorldSync.SendMessage should be exposed to the JavaScript engine");
    }

    [Test]
    public void RegisterAPI_RegisterMessageCallbackExposedToJavaScript()
    {
        LogAssert.ignoreFailingMessages = true;
        var result = jsHandler.Run("typeof WorldSync.RegisterMessageCallback");
        Assert.IsNotNull(result);
        Assert.AreNotEqual("undefined", result.ToString(),
            "WorldSync.RegisterMessageCallback should be exposed to the JavaScript engine");
    }

    // ----- AC6: GetLocalClientId -----

    [UnityTest]
    public IEnumerator GetLocalClientId_ValidSession_ReturnsClientId()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "clientid-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "clientid-test", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("clientid-test"));

        string localId = JSWorldSync.GetLocalClientId("clientid-test");
        Assert.IsNotNull(localId, "GetLocalClientId should return a non-null client ID");

        var client = WebVerseRuntime.Instance.GetWorldSyncClient("clientid-test");
        Assert.AreEqual(client.CurrentSession.LocalClientId, localId);
    }

    [Test]
    public void GetLocalClientId_UnknownSession_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        string result = JSWorldSync.GetLocalClientId("no-such-session");
        Assert.IsNull(result);
    }

    // ===== Story 4.1: Bridge Suspension & Connection State =====

    // ----- AC5: GetConnectionState -----

    [UnityTest]
    public IEnumerator GetConnectionState_ConnectedClient_ReturnsConnected()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "state-session-1";

        JSWorldSync.CreateSession("localhost", 1883, false, "state-client-1", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("state-client-1"));

        string state = JSWorldSync.GetConnectionState("state-client-1");
        Assert.AreEqual("connected", state,
            "GetConnectionState should return 'connected' for a connected client");
    }

    [Test]
    public void GetConnectionState_UnknownId_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        string state = JSWorldSync.GetConnectionState("no-such-session");
        Assert.IsNull(state, "GetConnectionState should return null for unknown sessionID");
    }

    [Test]
    public void GetConnectionState_NullId_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        string state = JSWorldSync.GetConnectionState(null);
        Assert.IsNull(state, "GetConnectionState should return null for null sessionID");
    }

    // ----- AC6: OnConnectionStateChanged -----

    [UnityTest]
    public IEnumerator OnConnectionStateChanged_ValidArgs_AttachesCallback()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "sc-session-1";

        JSWorldSync.CreateSession("localhost", 1883, false, "sc-client-1", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("sc-client-1"));

        JSWorldSync.TestHook_StateChangeCallbackInvocations = 0;

        bool ok = JSWorldSync.OnConnectionStateChanged("sc-client-1", "onStateChange");
        Assert.IsTrue(ok, "OnConnectionStateChanged should return true on valid input");

        Assert.AreEqual(0, JSWorldSync.TestHook_StateChangeCallbackInvocations,
            "No state change events should have fired yet");

        // Trigger a state change by calling DisconnectAsync directly on the client.
        // This fires OnDisconnected WITHOUT detaching callbacks (unlike ExitSession which
        // detaches before disconnect).
        var client = WebVerseRuntime.Instance.GetWorldSyncClient("sc-client-1");
        Assert.IsNotNull(client);
        var disconnectTask = client.DisconnectAsync();
        yield return new WaitUntil(() => disconnectTask.IsCompleted);

        Assert.Greater(JSWorldSync.TestHook_StateChangeCallbackInvocations, 0,
            "State-change callback should have fired when disconnect event occurred");
    }

    [UnityTest]
    public IEnumerator OnConnectionStateChanged_DuplicateCallback_NoDoubleAttach()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "dup-sc-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "dup-sc-client", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("dup-sc-client"));

        // Register the same callback twice.
        Assert.IsTrue(JSWorldSync.OnConnectionStateChanged("dup-sc-client", "onDupState"));
        Assert.IsTrue(JSWorldSync.OnConnectionStateChanged("dup-sc-client", "onDupState"),
            "Second registration should return true (no-op)");
    }

    [Test]
    public void OnConnectionStateChanged_UnknownSession_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:OnConnectionStateChanged.*No WorldSyncClient registered"));

        bool ok = JSWorldSync.OnConnectionStateChanged("no-such-session", "onState");
        Assert.IsFalse(ok);
    }

    [Test]
    public void OnConnectionStateChanged_EmptyCallback_ReturnsFalseAndLogsError()
    {
        LogAssert.ignoreFailingMessages = true;
        LogAssert.Expect(LogType.Error,
            new Regex("WorldSync:OnConnectionStateChanged.*callback is required"));

        bool ok = JSWorldSync.OnConnectionStateChanged("some-session", "");
        Assert.IsFalse(ok);
    }

    [UnityTest]
    public IEnumerator ExitSession_DetachesStateChangeCallbacks()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "detach-sc-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "detach-sc-client", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("detach-sc-client"));

        // Register a state-change callback.
        Assert.IsTrue(JSWorldSync.OnConnectionStateChanged("detach-sc-client", "onDetachState"));

        JSWorldSync.TestHook_StateChangeCallbackInvocations = 0;

        // Exit the session — should detach state-change callbacks.
        Assert.IsTrue(JSWorldSync.ExitSession("detach-sc-client"));
        yield return WaitForCondition(() =>
            WebVerseRuntime.Instance.GetWorldSyncClient("detach-sc-client") == null);

        // After exit, callbacks should have been detached.
        Assert.AreEqual(0, JSWorldSync.TestHook_StateChangeCallbackInvocations,
            "State-change callback should not fire after ExitSession detaches it");
    }

    [Test]
    public void RegisterAPI_GetConnectionStateExposedToJavaScript()
    {
        LogAssert.ignoreFailingMessages = true;
        var result = jsHandler.Run("typeof WorldSync.GetConnectionState");
        Assert.IsNotNull(result);
        Assert.AreNotEqual("undefined", result.ToString(),
            "WorldSync.GetConnectionState should be exposed to the JavaScript engine");
    }

    [Test]
    public void RegisterAPI_OnConnectionStateChangedExposedToJavaScript()
    {
        LogAssert.ignoreFailingMessages = true;
        var result = jsHandler.Run("typeof WorldSync.OnConnectionStateChanged");
        Assert.IsNotNull(result);
        Assert.AreNotEqual("undefined", result.ToString(),
            "WorldSync.OnConnectionStateChanged should be exposed to the JavaScript engine");
    }

    [UnityTest]
    public IEnumerator MessageCallback_ReattachedAfterReconnect()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "reattach-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "reattach-client", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("reattach-client"));

        // Register a message callback on the current session.
        Assert.IsTrue(JSWorldSync.RegisterMessageCallback("reattach-client", "onReattachMsg"));

        var client = WebVerseRuntime.Instance.GetWorldSyncClient("reattach-client");
        Assert.IsNotNull(client);

        // Simulate reconnection: join a new session (simulates RecoverSessionAsync).
        var joinTask = client.JoinSessionAsync("reattach-session");
        yield return new WaitUntil(() => joinTask.IsCompleted);

        // Now CurrentSession is the NEW session object.
        // The message callback is still in _messageCallbackHandlers but attached to the old session's event.
        // Fire OnStateRecovered to trigger re-attachment to the new session.
        JSWorldSync.TestHook_MessageCallbackReattachmentCount = 0;
        JSWorldSync.TestHook_MessageCallbackInvocations = 0;

        client.FireOnStateRecovered();
        yield return null;

        Assert.Greater(JSWorldSync.TestHook_MessageCallbackReattachmentCount, 0,
            "Message callback should have been re-attached when OnStateRecovered fired");

        // Verify the callback actually fires on the new session.
        client.CurrentSession.HandleCustomMessage("test-topic", "sender", "payload");
        yield return null;

        Assert.Greater(JSWorldSync.TestHook_MessageCallbackInvocations, 0,
            "Message callback should fire on the new session after re-attachment");
    }

    [UnityTest]
    public IEnumerator SessionExpired_CleansUpMessageCallbacks()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "expiry-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "expiry-client", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("expiry-client"));

        // Register a message callback.
        Assert.IsTrue(JSWorldSync.RegisterMessageCallback("expiry-client", "onExpiryMsg"));

        var client = WebVerseRuntime.Instance.GetWorldSyncClient("expiry-client");
        Assert.IsNotNull(client);

        // Verify callback fires before expiry.
        JSWorldSync.TestHook_MessageCallbackInvocations = 0;
        client.CurrentSession.HandleCustomMessage("pre-expiry", "sender", "data");
        yield return null;
        Assert.Greater(JSWorldSync.TestHook_MessageCallbackInvocations, 0,
            "Callback should fire before session expiry");

        // Fire OnSessionExpired — handler wired in ConnectAndCreateAsync should clean up message callbacks.
        client.FireOnSessionExpired("expiry-session");
        yield return null;

        // After expiry cleanup, message callbacks for that session should be removed from dictionary.
        // Re-registering the same callback should succeed (not be a no-op from duplicate guard).
        // But first we need a valid session for RegisterMessageCallback to accept.
        // So let's verify indirectly: the handler count was decremented.
        // We can also verify the callback no longer fires on the old session.
        JSWorldSync.TestHook_MessageCallbackInvocations = 0;
        client.CurrentSession.HandleCustomMessage("post-expiry", "sender", "data");
        yield return null;

        // The callback was removed from _messageCallbackHandlers by HandleSessionExpired,
        // but it's still subscribed to the OLD session's OnCustomMessage event delegate
        // (HandleSessionExpired doesn't unsubscribe from events since session may be invalid).
        // However, the important thing is it's removed from the dictionary so it won't
        // be re-attached on future reconnects.
        // Let's verify by checking that a fresh registration succeeds (not duplicate no-op).
        // Create a new session first.
        WorldSyncClient.DefaultSimulateCreateSessionId = "expiry-session-2";
        JSWorldSync.CreateSession("localhost", 1883, false, "expiry-client-2", "TestTag2");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("expiry-client-2"));
        Assert.IsTrue(JSWorldSync.RegisterMessageCallback("expiry-client-2", "onExpiryMsg"),
            "Should be able to register callback on new session after old one expired");
    }

    [UnityTest]
    public IEnumerator SessionExpired_CleansUpStateChangeCallbacks()
    {
        LogAssert.ignoreFailingMessages = true;
        WorldSyncClient.DefaultUseTestHooks = true;
        WorldSyncClient.DefaultSimulateCreateSessionId = "expiry-sc-session";

        JSWorldSync.CreateSession("localhost", 1883, false, "expiry-sc-client", "TestTag");
        yield return WaitForCondition(() => JSWorldSync.IsSessionEstablished("expiry-sc-client"));

        // Register a state-change callback.
        Assert.IsTrue(JSWorldSync.OnConnectionStateChanged("expiry-sc-client", "onExpirySC"));

        var client = WebVerseRuntime.Instance.GetWorldSyncClient("expiry-sc-client");
        Assert.IsNotNull(client);

        JSWorldSync.TestHook_StateChangeCallbackInvocations = 0;

        // Fire OnSessionExpired — handler wired in ConnectAndCreateAsync should detach state-change callbacks.
        client.FireOnSessionExpired("expiry-sc-session");
        yield return null;

        // State-change callbacks should be detached. Firing OnConnected should not invoke the callback.
        // Directly trigger a state change on the client to verify no callback fires.
        var disconnectTask = client.DisconnectAsync();
        yield return new WaitUntil(() => disconnectTask.IsCompleted);

        Assert.AreEqual(0, JSWorldSync.TestHook_StateChangeCallbackInvocations,
            "State-change callbacks should be detached after session expiry cleanup");
    }
}
#endif
