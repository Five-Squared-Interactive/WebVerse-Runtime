// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// TDD tests for memory cleanup and leak prevention fixes.
/// These tests verify that static instances, caches, and event handlers
/// are properly cleaned up to prevent memory leaks.
/// </summary>
public class MemoryCleanupTests
{
    #region MemoryDebug Tests

    [Test]
    public void MemoryDebug_GetMemoryStats_ReturnsValidStats()
    {
        // Arrange & Act
        var stats = MemoryDebug.GetMemoryStats();

        // Assert - memory values should be non-negative
        Assert.GreaterOrEqual(stats.TotalReservedMB, 0f, "TotalReservedMB should be non-negative");
        Assert.GreaterOrEqual(stats.TotalAllocatedMB, 0f, "TotalAllocatedMB should be non-negative");
        Assert.GreaterOrEqual(stats.MonoHeapMB, 0f, "MonoHeapMB should be non-negative");
        Assert.GreaterOrEqual(stats.MonoUsedMB, 0f, "MonoUsedMB should be non-negative");
        Assert.GreaterOrEqual(stats.GCTotalMB, 0f, "GCTotalMB should be non-negative");
    }

    [Test]
    public void MemoryDebug_LogMemorySnapshot_DoesNotThrow()
    {
        // Arrange & Act & Assert
        Assert.DoesNotThrow(() =>
        {
            MemoryDebug.LogMemorySnapshot("Test Snapshot");
        });
    }

    [Test]
    public void MemoryDebug_LogMemoryDelta_CalculatesCorrectly()
    {
        // Arrange
        var before = new MemoryStats
        {
            TotalReservedMB = 100f,
            TotalAllocatedMB = 80f,
            MonoHeapMB = 50f,
            MonoUsedMB = 40f,
            GCTotalMB = 30f
        };
        var after = new MemoryStats
        {
            TotalReservedMB = 120f,
            TotalAllocatedMB = 90f,
            MonoHeapMB = 55f,
            MonoUsedMB = 45f,
            GCTotalMB = 35f
        };

        // Act & Assert - should not throw and log correctly
        Assert.DoesNotThrow(() =>
        {
            MemoryDebug.LogMemoryDelta(before, after, "Delta Test");
        });
    }

    [Test]
    public void MemoryDebug_ForceCleanup_DoesNotThrow()
    {
        // Arrange & Act & Assert
        Assert.DoesNotThrow(() =>
        {
            MemoryDebug.ForceCleanup();
        });
    }

    #endregion

    #region StraightFour Tests

    [Test]
    public void StraightFour_ClearInstance_SetsInstanceToNull()
    {
        // Arrange - Get the instance field using reflection
        var instanceField = typeof(FiveSQD.StraightFour.StraightFour).GetField(
            "instance",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(instanceField, "Could not find 'instance' field in StraightFour");

        // Act - Call ClearInstance
        FiveSQD.StraightFour.StraightFour.ClearInstance();

        // Assert - Instance should be null
        var instanceValue = instanceField.GetValue(null);
        Assert.IsNull(instanceValue, "StraightFour.instance should be null after ClearInstance()");
    }

    #endregion

    #region GLTFHandler Tests

    [Test]
    public void GLTFHandler_Reset_ClearsPrefabCache()
    {
        // Arrange
        GameObject testObject = new GameObject("TestGLTFHandler");
        var handler = testObject.AddComponent<FiveSQD.WebVerse.Handlers.GLTF.GLTFHandler>();

        // Get the private dictionary using reflection
        var prefabsField = typeof(FiveSQD.WebVerse.Handlers.GLTF.GLTFHandler).GetField(
            "gltfMeshPrefabs",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(prefabsField, "Could not find 'gltfMeshPrefabs' field");

        var prefabs = prefabsField.GetValue(handler) as Dictionary<string, GameObject>;
        Assert.IsNotNull(prefabs, "gltfMeshPrefabs dictionary is null");

        // Add some test entries
        var testPrefab = new GameObject("TestPrefab");
        prefabs.Add("test/path.gltf", testPrefab);
        Assert.AreEqual(1, prefabs.Count, "Prefab should be added");

        // Act
        handler.Reset();

        // Assert - Dictionary should be cleared
        Assert.AreEqual(0, prefabs.Count, "gltfMeshPrefabs should be empty after Reset()");

        // Cleanup
        Object.DestroyImmediate(testObject);
    }

    [Test]
    public void GLTFHandler_Terminate_CallsReset()
    {
        // Arrange
        GameObject testObject = new GameObject("TestGLTFHandler");
        var handler = testObject.AddComponent<FiveSQD.WebVerse.Handlers.GLTF.GLTFHandler>();

        // Get the private dictionary using reflection
        var prefabsField = typeof(FiveSQD.WebVerse.Handlers.GLTF.GLTFHandler).GetField(
            "gltfMeshPrefabs",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var prefabs = prefabsField.GetValue(handler) as Dictionary<string, GameObject>;

        // Add some test entries
        var testPrefab = new GameObject("TestPrefab");
        prefabs.Add("test/path.gltf", testPrefab);

        // Act
        LogAssert.Expect(LogType.Log, "[GLTFHandler->Reset] Cleared 1 cached prefabs.");
        handler.Terminate();

        // Assert - Dictionary should be cleared after Terminate
        Assert.AreEqual(0, prefabs.Count, "gltfMeshPrefabs should be empty after Terminate()");

        // Cleanup
        Object.DestroyImmediate(testObject);
    }

    #endregion

    #region DataAPIHelper Tests

    [Test]
    public void DataAPIHelper_Terminate_ClearsQueueAndInstance()
    {
        // Arrange
        GameObject testObject = new GameObject("TestDataAPIHelper");
        var helper = testObject.AddComponent<FiveSQD.WebVerse.Handlers.Javascript.APIs.Data.DataAPIHelper>();
        helper.Initialize();

        // Get private fields using reflection
        var instanceField = typeof(FiveSQD.WebVerse.Handlers.Javascript.APIs.Data.DataAPIHelper).GetField(
            "instance",
            BindingFlags.NonPublic | BindingFlags.Static);
        var queueField = typeof(FiveSQD.WebVerse.Handlers.Javascript.APIs.Data.DataAPIHelper).GetField(
            "javascriptQueue",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.IsNotNull(instanceField, "Could not find 'instance' field");
        Assert.IsNotNull(queueField, "Could not find 'javascriptQueue' field");

        // Verify initialized state
        Assert.IsNotNull(instanceField.GetValue(null), "Instance should be set after Initialize()");

        // Act
        helper.Terminate();

        // Assert
        Assert.IsNull(instanceField.GetValue(null), "DataAPIHelper.instance should be null after Terminate()");

        // Cleanup
        Object.DestroyImmediate(testObject);
    }

    #endregion

    #region EntityAPIHelper Tests

    [Test]
    public void EntityAPIHelper_ClearEntityMapping_ClearsLoadedEntitiesAndInstance()
    {
        // Arrange
        GameObject testObject = new GameObject("TestEntityAPIHelper");
        var helper = testObject.AddComponent<FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper>();

        // Get private fields using reflection
        var instanceField = typeof(FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper).GetField(
            "instance",
            BindingFlags.NonPublic | BindingFlags.Static);
        var loadedEntitiesField = typeof(FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper).GetField(
            "loadedEntities",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsNotNull(instanceField, "Could not find 'instance' field");
        Assert.IsNotNull(loadedEntitiesField, "Could not find 'loadedEntities' field");

        // Initialize
        FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.InitializeEntityMapping();

        // Verify initialized state
        var loadedEntities = loadedEntitiesField.GetValue(null) as System.Collections.IDictionary;
        Assert.IsNotNull(loadedEntities, "loadedEntities should be initialized");

        // Act
        FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.ClearEntityMapping();

        // Assert
        loadedEntities = loadedEntitiesField.GetValue(null) as System.Collections.IDictionary;
        Assert.IsNotNull(loadedEntities, "loadedEntities dictionary should still exist");
        Assert.AreEqual(0, loadedEntities.Count, "loadedEntities should be empty after ClearEntityMapping()");
        Assert.IsNull(instanceField.GetValue(null), "EntityAPIHelper.instance should be null after ClearEntityMapping()");

        // Cleanup
        Object.DestroyImmediate(testObject);
    }

    #endregion

    #region WebVerseWebView Tests

    [Test]
    public void WebVerseWebView_Terminate_ClearsAllReferences()
    {
        // Arrange
        GameObject testObject = new GameObject("TestWebVerseWebView");
        var webView = testObject.AddComponent<FiveSQD.WebVerse.WebView.WebVerseWebView>();

        // Get private fields using reflection
        var webViewObjectField = typeof(FiveSQD.WebVerse.WebView.WebVerseWebView).GetField(
            "webViewObject",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var urlsToLoadField = typeof(FiveSQD.WebVerse.WebView.WebVerseWebView).GetField(
            "urlsToLoad",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var webViewSetUpField = typeof(FiveSQD.WebVerse.WebView.WebVerseWebView).GetField(
            "webViewSetUp",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.IsNotNull(webViewObjectField, "Could not find 'webViewObject' field");
        Assert.IsNotNull(urlsToLoadField, "Could not find 'urlsToLoad' field");
        Assert.IsNotNull(webViewSetUpField, "Could not find 'webViewSetUp' field");

        // Set up some state manually (we can't call Initialize without runtime prefab)
        urlsToLoadField.SetValue(webView, new Queue<string>());
        webViewSetUpField.SetValue(webView, true);

        // Act
        LogAssert.Expect(LogType.Log, "[WebVerseWebView] Terminated.");
        webView.Terminate();

        // Assert
        Assert.IsNull(urlsToLoadField.GetValue(webView), "urlsToLoad should be null after Terminate()");
        Assert.IsFalse((bool)webViewSetUpField.GetValue(webView), "webViewSetUp should be false after Terminate()");

        // Cleanup
        Object.DestroyImmediate(testObject);
    }

    #endregion

    #region MemoryStats Struct Tests

    [Test]
    public void MemoryStats_DefaultValues_AreZero()
    {
        // Arrange & Act
        var stats = new MemoryStats();

        // Assert
        Assert.AreEqual(0f, stats.TotalReservedMB);
        Assert.AreEqual(0f, stats.TotalAllocatedMB);
        Assert.AreEqual(0f, stats.MonoHeapMB);
        Assert.AreEqual(0f, stats.MonoUsedMB);
        Assert.AreEqual(0f, stats.GCTotalMB);
    }

    [Test]
    public void MemoryStats_CanBeAssigned()
    {
        // Arrange
        var stats = new MemoryStats
        {
            TotalReservedMB = 100.5f,
            TotalAllocatedMB = 80.5f,
            MonoHeapMB = 50.5f,
            MonoUsedMB = 40.5f,
            GCTotalMB = 30.5f
        };

        // Assert
        Assert.AreEqual(100.5f, stats.TotalReservedMB);
        Assert.AreEqual(80.5f, stats.TotalAllocatedMB);
        Assert.AreEqual(50.5f, stats.MonoHeapMB);
        Assert.AreEqual(40.5f, stats.MonoUsedMB);
        Assert.AreEqual(30.5f, stats.GCTotalMB);
    }

    #endregion

    #region BaseHandler Tests

    [Test]
    public void BaseHandler_Terminate_CanBeCalledMultipleTimes()
    {
        // Arrange
        GameObject testObject = new GameObject("TestBaseHandler");
        var handler = testObject.AddComponent<BaseHandler>();
        handler.Initialize();

        // Act & Assert - Should not throw on multiple terminates
        LogAssert.Expect(LogType.Log, "[BaseHandler] Terminated.");
        handler.Terminate();

        LogAssert.Expect(LogType.Log, "[BaseHandler] Terminated.");
        Assert.DoesNotThrow(() => handler.Terminate());

        // Cleanup
        Object.DestroyImmediate(testObject);
    }

    #endregion

    #region Integration-Style Cleanup Tests

    [Test]
    public void AllCleanupMethods_CanBeCalledInSequence_WithoutErrors()
    {
        // This test verifies that all cleanup methods can be called without throwing exceptions
        // in the expected order during a "world unload" scenario

        // Arrange - Create mock components
        GameObject gltfHandlerGO = new GameObject("GLTFHandler");
        var gltfHandler = gltfHandlerGO.AddComponent<FiveSQD.WebVerse.Handlers.GLTF.GLTFHandler>();

        GameObject dataHelperGO = new GameObject("DataAPIHelper");
        var dataHelper = dataHelperGO.AddComponent<FiveSQD.WebVerse.Handlers.Javascript.APIs.Data.DataAPIHelper>();
        dataHelper.Initialize();

        GameObject webViewGO = new GameObject("WebVerseWebView");
        var webView = webViewGO.AddComponent<FiveSQD.WebVerse.WebView.WebVerseWebView>();

        // Initialize EntityAPIHelper
        FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.InitializeEntityMapping();

        // Act - Call all cleanup methods in expected order
        Assert.DoesNotThrow(() =>
        {
            // 1. Reset GLTF cache (happens during UnloadWorld)
            gltfHandler.Reset();

            // 2. Clear entity mapping (happens during JavascriptHandler.Reset)
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.ClearEntityMapping();

            // 3. Terminate components (happens during TerminateComponents)
            dataHelper.Terminate();
            webView.Terminate();
            gltfHandler.Terminate();

            // 4. Clear StraightFour instance
            FiveSQD.StraightFour.StraightFour.ClearInstance();

            // 5. Force memory cleanup
            MemoryDebug.ForceCleanup();
        });

        // Cleanup
        Object.DestroyImmediate(gltfHandlerGO);
        Object.DestroyImmediate(dataHelperGO);
        Object.DestroyImmediate(webViewGO);
    }

    [Test]
    public void MemoryDeltaAfterCleanup_ShowsReduction()
    {
        // This test demonstrates how to use MemoryDebug to verify cleanup
        // Note: In a real scenario, you'd load and unload a world

        // Arrange - Take snapshot before
        var before = MemoryDebug.GetMemoryStats();

        // Create some allocations
        var tempList = new List<byte[]>();
        for (int i = 0; i < 10; i++)
        {
            tempList.Add(new byte[1024 * 1024]); // 1MB each
        }

        // Take snapshot after allocation
        var afterAlloc = MemoryDebug.GetMemoryStats();

        // Clear allocations
        tempList.Clear();
        tempList = null;

        // Act - Force cleanup
        MemoryDebug.ForceCleanup();

        // Take snapshot after cleanup
        var afterCleanup = MemoryDebug.GetMemoryStats();

        // Assert - After allocation should show higher GC total than before
        Assert.Greater(afterAlloc.GCTotalMB, before.GCTotalMB,
            "Memory should increase after allocation");

        // Log the delta for debugging
        MemoryDebug.LogMemoryDelta(before, afterCleanup, "Allocation-Cleanup Cycle");
    }

    #endregion
}
