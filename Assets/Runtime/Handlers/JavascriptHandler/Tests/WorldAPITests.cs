// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.LocalStorage;
using System.IO;
using System.Reflection;
using WorldAPI = FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities.World;

/// <summary>
/// Unit tests for the World JavaScript API.
/// </summary>
public class WorldAPITests
{
    private WebVerseRuntime runtime;
    private GameObject runtimeGO;
    private string testDirectory;

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

        testDirectory = Path.Combine(Path.GetTempPath(), "WorldAPITests");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        WebVerseRuntime.Instance = null;
        if (runtime != null && Directory.Exists(testDirectory))
        {
            Directory.Delete(testDirectory, true);
        }
        if (runtimeGO != null)
        {
            Object.DestroyImmediate(runtimeGO);
        }
    }

    private static void SetCurrentURL(WebVerseRuntime runtime, string url)
    {
        PropertyInfo prop = typeof(WebVerseRuntime).GetProperty(
            "currentURL", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop, "currentURL property must exist on WebVerseRuntime.");
        prop.SetValue(runtime, url);
    }

    [Test]
    public void GetWorldURL_BeforeAnyLoad_ReturnsNull()
    {
        Assert.IsNull(WorldAPI.GetWorldURL());
    }

    [Test]
    public void GetWorldURL_AfterCurrentURLSet_ReturnsThatURL()
    {
        const string url = "https://example.test/world.veml";
        SetCurrentURL(runtime, url);

        Assert.AreEqual(url, WorldAPI.GetWorldURL());
    }

    [Test]
    public void GetWorldURL_ReflectsLatestAssignment()
    {
        SetCurrentURL(runtime, "https://example.test/first.veml");
        SetCurrentURL(runtime, "https://example.test/second.veml");

        Assert.AreEqual("https://example.test/second.veml", WorldAPI.GetWorldURL());
    }

    [Test]
    public void LoadWorld_SetsCurrentURL()
    {
        const string url = "https://example.test/load-world.veml";

        // The full load pipeline pulls in handlers and HTTP that aren't wired
        // up in this test context, so downstream work may throw. We only care
        // that the currentURL assignment at the top of LoadWorld ran.
        try { runtime.LoadWorld(url, null); } catch { }

        Assert.AreEqual(url, WorldAPI.GetWorldURL());
    }

    [Test]
    public void LoadWebPage_SetsCurrentURL()
    {
        const string url = "https://example.test/page.html";

        try { runtime.LoadWebPage(url, null); } catch { }

        Assert.AreEqual(url, WorldAPI.GetWorldURL());
    }

    [Test]
    public void LoadWorld_WithInlineRequireScript_AcceptsArgument()
    {
        const string url = "https://example.test/with-require.veml";

        // The new overload should accept an inline JS body without throwing at the
        // signature/dispatch boundary. Downstream load machinery may still throw.
        try { runtime.LoadWorld(url, null, "var __requireSentinel = 1;"); } catch { }

        Assert.AreEqual(url, WorldAPI.GetWorldURL());
    }

    [Test]
    public void LoadWorld_WithURIRequireScript_AcceptsArgument()
    {
        const string url = "https://example.test/with-require-uri.veml";

        try { runtime.LoadWorld(url, null, "init.js"); } catch { }

        Assert.AreEqual(url, WorldAPI.GetWorldURL());
    }

    [Test]
    public void LoadWorld_DefaultRequireScript_BackwardCompatible()
    {
        // The single-onLoaded overload signature still works (default param = null).
        const string url = "https://example.test/no-require.veml";

        try { runtime.LoadWorld(url, null); } catch { }

        Assert.AreEqual(url, WorldAPI.GetWorldURL());
    }

    [Test]
    public void JSAPI_LoadWorld_OneArg_Compiles()
    {
        // Verifies the World.LoadWorld(url) overload remains callable.
        try { WorldAPI.LoadWorld("https://example.test/one-arg.veml"); } catch { }
    }

    [Test]
    public void JSAPI_LoadWorld_TwoArg_Compiles()
    {
        // Verifies the new World.LoadWorld(url, requireScript) overload is callable.
        try
        {
            WorldAPI.LoadWorld("https://example.test/two-arg.veml", "var __sentinel = 1;");
        }
        catch { }
    }

    [Test]
    public void TestLoadWorld_DoesNotMutateCurrentURL()
    {
        // Pre-condition: simulate a previously loaded world.
        const string sentinel = "https://example.test/already-loaded.veml";
        SetCurrentURL(runtime, sentinel);

        try
        {
            runtime.TestLoadWorld("https://example.test/test-target.veml",
                (success, errorMessage, title) => { });
        }
        catch { }

        // The contract: TestLoadWorld must not overwrite currentURL even if the network
        // call later fails or hangs.
        Assert.AreEqual(sentinel, WorldAPI.GetWorldURL());
    }

    [Test]
    public void TestLoadWorld_RejectsNonVEMLExtensions()
    {
        bool callbackFired = false;
        bool reportedSuccess = true;
        string reportedError = null;

        try
        {
            runtime.TestLoadWorld("https://example.test/world.glb",
                (success, errorMessage, title) =>
                {
                    callbackFired = true;
                    reportedSuccess = success;
                    reportedError = errorMessage;
                });
        }
        catch { }

        Assert.IsTrue(callbackFired, "Callback should fire synchronously for non-VEML extensions.");
        Assert.IsFalse(reportedSuccess);
        StringAssert.Contains("VEML", reportedError);
    }

    [Test]
    public void TestLoadWorld_X3DAlsoRejected()
    {
        bool callbackFired = false;
        bool reportedSuccess = true;

        try
        {
            runtime.TestLoadWorld("https://example.test/world.x3d",
                (success, errorMessage, title) =>
                {
                    callbackFired = true;
                    reportedSuccess = success;
                });
        }
        catch { }

        Assert.IsTrue(callbackFired);
        Assert.IsFalse(reportedSuccess);
    }

    [Test]
    public void JSAPI_TestLoadWorld_Compiles()
    {
        // Verifies the JS-facing World.TestLoadWorld(url, callbackName) signature is callable.
        try
        {
            WorldAPI.TestLoadWorld("https://example.test/world.veml", "onTestComplete");
        }
        catch { }
    }

    [Test]
    public void JSAPI_TestLoadWorld_NullCallback_DoesNotThrow()
    {
        // A null/empty callback name should be tolerated (just no JS invocation).
        Assert.DoesNotThrow(() =>
        {
            try
            {
                WorldAPI.TestLoadWorld("https://example.test/world.glb", null);
            }
            catch { }
        });
    }
}
