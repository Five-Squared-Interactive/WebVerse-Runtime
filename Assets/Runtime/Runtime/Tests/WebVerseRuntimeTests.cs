// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.LocalStorage;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for WebVerseRuntime MonoBehaviour.
/// Tests component lifecycle, initialization, termination, and default state.
/// </summary>
public class WebVerseRuntimeTests
{
    /// <summary>
    /// Helper to clean up the static Instance between tests.
    /// </summary>
    private void CleanupInstance()
    {
        WebVerseRuntime.Instance = null;
    }

    #region Component Creation Tests

    [UnityTest]
    public IEnumerator WebVerseRuntime_CanBeAddedToGameObject()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestRuntime");

        // Act
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        // Assert
        Assert.IsNotNull(runtime);

        // Cleanup
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_Instance_IsNullBeforeInitialize()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        CleanupInstance();
        GameObject go = new GameObject("TestRuntime");

        // Act
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        // Assert - Instance is set in Initialize, not Awake
        Assert.IsNull(WebVerseRuntime.Instance);

        // Cleanup
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    #endregion

    #region State Tests

    [UnityTest]
    public IEnumerator WebVerseRuntime_State_IsUnloaded_BeforeInitialization()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        // Assert - default enum value is Unloaded (0)
        Assert.AreEqual(WebVerseRuntime.RuntimeState.Unloaded, runtime.state);

        // Cleanup
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_CurrentURL_IsNull_BeforeLoading()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        // Assert
        Assert.IsTrue(string.IsNullOrEmpty(runtime.currentURL));

        // Cleanup
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_CurrentBasePath_IsNull_BeforeLoading()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        // Assert
        Assert.IsTrue(string.IsNullOrEmpty(runtime.currentBasePath));

        // Cleanup
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    #endregion

    #region Initialize Tests

    [UnityTest]
    public IEnumerator WebVerseRuntime_Initialize_WithCacheMode_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        CleanupInstance();
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            runtime.Initialize(
                LocalStorageManager.LocalStorageMode.Cache,
                1024, 4096, 256,
                Application.temporaryCachePath, 30f);
        });
        yield return null;

        // Cleanup
        runtime.Terminate();
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_Initialize_SetsInstance()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        CleanupInstance();
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        // Act
        runtime.Initialize(
            LocalStorageManager.LocalStorageMode.Cache,
            1024, 4096, 256,
            Application.temporaryCachePath, 30f);
        yield return null;

        // Assert
        Assert.IsNotNull(WebVerseRuntime.Instance);
        Assert.AreEqual(runtime, WebVerseRuntime.Instance);

        // Cleanup
        runtime.Terminate();
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_Initialize_SetsStateToUnloaded()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        CleanupInstance();
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        // Act
        runtime.Initialize(
            LocalStorageManager.LocalStorageMode.Cache,
            1024, 4096, 256,
            Application.temporaryCachePath, 30f);
        yield return null;

        // Assert - After Initialize, state should be Unloaded (no world loaded yet)
        Assert.AreEqual(WebVerseRuntime.RuntimeState.Unloaded, runtime.state);

        // Cleanup
        runtime.Terminate();
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_Initialize_WithSettingsStruct_CacheMode_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        CleanupInstance();
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        var settings = new WebVerseRuntime.RuntimeSettings
        {
            storageMode = "cache",
            maxEntries = 1024,
            maxEntryLength = 4096,
            maxKeyLength = 256,
            filesDirectory = Application.temporaryCachePath,
            timeout = 30f
        };

        // Act & Assert
        Assert.DoesNotThrow(() => runtime.Initialize(settings));
        yield return null;

        // Cleanup
        runtime.Terminate();
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    #endregion

    #region Terminate Tests

    [UnityTest]
    public IEnumerator WebVerseRuntime_Terminate_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        CleanupInstance();
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        runtime.Initialize(
            LocalStorageManager.LocalStorageMode.Cache,
            1024, 4096, 256,
            Application.temporaryCachePath, 30f);
        yield return null;

        // Act & Assert
        Assert.DoesNotThrow(() => runtime.Terminate());
        yield return null;

        // Cleanup
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_Terminate_ClearsInstance()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        CleanupInstance();
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        runtime.Initialize(
            LocalStorageManager.LocalStorageMode.Cache,
            1024, 4096, 256,
            Application.temporaryCachePath, 30f);
        yield return null;

        // Act
        runtime.Terminate();
        yield return null;

        // Assert
        Assert.IsNull(WebVerseRuntime.Instance);

        // Cleanup
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_Terminate_WithoutInitialize_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        CleanupInstance();
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        // Act & Assert - Terminate without Initialize should not throw
        Assert.DoesNotThrow(() => runtime.Terminate());
        yield return null;

        // Cleanup
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_DoubleTerninate_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        CleanupInstance();
        GameObject go = new GameObject("TestRuntime");
        var runtime = go.AddComponent<WebVerseRuntime>();
        yield return null;

        runtime.Initialize(
            LocalStorageManager.LocalStorageMode.Cache,
            1024, 4096, 256,
            Application.temporaryCachePath, 30f);
        yield return null;

        // Act & Assert - Double terminate should not throw
        Assert.DoesNotThrow(() => runtime.Terminate());
        Assert.DoesNotThrow(() => runtime.Terminate());
        yield return null;

        // Cleanup
        CleanupInstance();
        Object.DestroyImmediate(go);
    }

    #endregion

    #region Version and Metadata Tests

    [UnityTest]
    public IEnumerator WebVerseRuntime_VersionString_IsNotNullOrEmpty()
    {
        LogAssert.ignoreFailingMessages = true;

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(WebVerseRuntime.versionString));
        yield return null;
    }

    [UnityTest]
    public IEnumerator WebVerseRuntime_CodenameString_IsNotNullOrEmpty()
    {
        LogAssert.ignoreFailingMessages = true;

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(WebVerseRuntime.codenameString));
        yield return null;
    }

    #endregion

    #region RuntimeState Enum Tests

    [Test]
    public void RuntimeState_Unloaded_HasValueZero()
    {
        LogAssert.ignoreFailingMessages = true;

        Assert.AreEqual(0, (int)WebVerseRuntime.RuntimeState.Unloaded);
    }

    [Test]
    public void RuntimeState_LoadingWorld_HasValueOne()
    {
        LogAssert.ignoreFailingMessages = true;

        Assert.AreEqual(1, (int)WebVerseRuntime.RuntimeState.LoadingWorld);
    }

    [Test]
    public void RuntimeState_LoadedWorld_HasValueTwo()
    {
        LogAssert.ignoreFailingMessages = true;

        Assert.AreEqual(2, (int)WebVerseRuntime.RuntimeState.LoadedWorld);
    }

    #endregion
}
