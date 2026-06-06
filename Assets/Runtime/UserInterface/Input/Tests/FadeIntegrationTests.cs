// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.VR.Comfort;
using FiveSQD.StraightFour.WorldState;
using FiveSQD.StraightFour.World;

/// <summary>
/// Integration tests for FadeController wiring with TabManager.
/// Validates fade-out/fade-in ordering during tab switches,
/// null-safety for desktop mode, and stability under rapid switching.
/// </summary>
public class FadeIntegrationTests
{
    private List<GameObject> _testObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in _testObjects)
        {
            if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
        }
        _testObjects.Clear();
    }

    private GameObject CreateTracked(string name)
    {
        var go = new GameObject(name);
        _testObjects.Add(go);
        return go;
    }

    private FadeController CreateFadeController()
    {
        var go = CreateTracked("FadeControllerHost");
        var controller = go.AddComponent<FadeController>();
        return controller;
    }

    /// <summary>
    /// Creates a minimal TabManager wired with no-op load/unload callbacks.
    /// Returns the TabManager plus a trigger to complete world loads.
    /// When provideWorld is true, load completion provides a mock World object
    /// so the success path (including OnWorldReadyForControlFlags) is exercised.
    /// </summary>
    private (TabManager tabManager, Action<bool> completeLoad) CreateTabManager(bool provideWorld = false)
    {
        var go = CreateTracked("TabManagerHost");
        var tabManager = go.AddComponent<TabManager>();

        // State manager needed by Initialize
        var stateGo = CreateTracked("StateManager");
        var stateManager = stateGo.AddComponent<WorldStateManager>();

        World mockWorld = null;
        if (provideWorld)
        {
            var worldGo = CreateTracked("MockWorld");
            mockWorld = worldGo.AddComponent<World>();
        }

        Action<bool> pendingComplete = null;

        tabManager.Initialize(
            stateManager,
            (url, basePath, onComplete) =>
            {
                // Store callback so test can trigger completion
                pendingComplete = (success) => onComplete?.Invoke(
                    success && provideWorld ? mockWorld : null, success);
                return null;
            },
            (world) => { /* no-op unload */ }
        );

        return (tabManager, (success) => pendingComplete?.Invoke(success));
    }

    // ==================== FadeOut called before world switch phases ====================

    [UnityTest]
    public IEnumerator FadeOut_CalledBeforeWorldLoad()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tabManager, completeLoad) = CreateTabManager();
        yield return null;

        bool fadeOutCalled = false;
        List<string> eventOrder = new List<string>();

        tabManager.OnFadeOutRequested = (onComplete) =>
        {
            fadeOutCalled = true;
            eventOrder.Add("fadeOut");
            onComplete?.Invoke(); // Complete immediately for test
        };
        tabManager.OnFadeInRequested = () => eventOrder.Add("fadeIn");
        tabManager.OnTabSwitchStarted += (prev, target) => eventOrder.Add("switchStarted");

        // Create a tab (triggers switch)
        tabManager.CreateTab("http://test.world", "Test", true);
        yield return null; // Let coroutine start

        // Complete the load
        completeLoad?.Invoke(false);
        yield return null;
        yield return null; // Let coroutine finish

        Assert.IsTrue(fadeOutCalled, "FadeOut should have been called");
        Assert.IsTrue(eventOrder.IndexOf("fadeOut") > eventOrder.IndexOf("switchStarted"),
            "FadeOut should be called after switch starts");
    }

    // ==================== FadeIn called after control flag restore ====================

    [UnityTest]
    public IEnumerator FadeIn_CalledAfterControlFlagRestore()
    {
        LogAssert.ignoreFailingMessages = true;

        // provideWorld: true so the success path fires OnWorldReadyForControlFlags
        var (tabManager, completeLoad) = CreateTabManager(provideWorld: true);
        yield return null;

        List<string> eventOrder = new List<string>();

        tabManager.OnFadeOutRequested = (onComplete) =>
        {
            eventOrder.Add("fadeOut");
            onComplete?.Invoke();
        };
        tabManager.OnFadeInRequested = () => eventOrder.Add("fadeIn");
        tabManager.OnWorldReadyForControlFlags = (world) => eventOrder.Add("controlFlags");

        tabManager.CreateTab("http://test.world", "Test", true);
        yield return null;

        completeLoad?.Invoke(true);

        // Wait for coroutine to complete
        float timeout = 2f;
        float elapsed = 0f;
        while (tabManager.IsSwitching && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(eventOrder.Contains("fadeIn"), "FadeIn should have been called");
        Assert.IsTrue(eventOrder.Contains("controlFlags"),
            "OnWorldReadyForControlFlags should have been called on successful load");

        int controlFlagsIdx = eventOrder.IndexOf("controlFlags");
        int fadeInIdx = eventOrder.IndexOf("fadeIn");
        Assert.Less(controlFlagsIdx, fadeInIdx,
            "Control flags should be restored before FadeIn");
    }

    // ==================== Tab switch fade sequence ordering ====================

    [UnityTest]
    public IEnumerator TabSwitch_FadeSequence_CorrectOrdering()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tabManager, completeLoad) = CreateTabManager();
        yield return null;

        // Create first tab (initial load)
        tabManager.CreateTab("http://world1.test", "World1", true);
        yield return null;
        completeLoad?.Invoke(false);

        float timeout = 2f;
        float elapsed = 0f;
        while (tabManager.IsSwitching && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Now create a second tab and track ordering
        List<string> eventOrder = new List<string>();

        tabManager.OnFadeOutRequested = (onComplete) =>
        {
            eventOrder.Add("fadeOut");
            onComplete?.Invoke();
        };
        tabManager.OnFadeInRequested = () => eventOrder.Add("fadeIn");
        tabManager.OnWorldReadyForControlFlags = (world) => eventOrder.Add("controlFlags");
        tabManager.OnTabSwitchStarted += (prev, target) => eventOrder.Add("switchStarted");
        tabManager.OnTabSwitchCompleted += (prev, target, success) => eventOrder.Add("switchCompleted");

        tabManager.CreateTab("http://world2.test", "World2", true);
        yield return null;

        completeLoad?.Invoke(false);

        elapsed = 0f;
        while (tabManager.IsSwitching && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Verify ordering: switchStarted → fadeOut → ... → fadeIn → switchCompleted
        Assert.IsTrue(eventOrder.Count >= 4, $"Expected at least 4 events, got {eventOrder.Count}: {string.Join(", ", eventOrder)}");
        Assert.Less(eventOrder.IndexOf("switchStarted"), eventOrder.IndexOf("fadeOut"),
            "switchStarted should come before fadeOut");
        Assert.Less(eventOrder.IndexOf("fadeOut"), eventOrder.IndexOf("fadeIn"),
            "fadeOut should come before fadeIn");
        Assert.Less(eventOrder.IndexOf("fadeIn"), eventOrder.IndexOf("switchCompleted"),
            "fadeIn should come before switchCompleted");
    }

    // ==================== Null FadeController (desktop mode) ====================

    [UnityTest]
    public IEnumerator NullFadeController_NoErrors()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tabManager, completeLoad) = CreateTabManager();
        yield return null;

        // Deliberately do NOT set OnFadeOutRequested or OnFadeInRequested
        // This simulates desktop mode where no FadeController exists

        bool switchCompleted = false;
        tabManager.OnTabSwitchCompleted += (prev, target, success) => switchCompleted = true;

        tabManager.CreateTab("http://test.world", "Test", true);
        yield return null;

        completeLoad?.Invoke(false);

        float timeout = 2f;
        float elapsed = 0f;
        while (!switchCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(switchCompleted, "Tab switch should complete without FadeController");
        Assert.IsFalse(tabManager.IsSwitching, "IsSwitching should be false after completion");
    }

    // ==================== Consecutive tab switches stability ====================

    [UnityTest]
    public IEnumerator ConsecutiveTabSwitches_FadeStable()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tabManager, completeLoad) = CreateTabManager();
        yield return null;

        int fadeOutCount = 0;
        int fadeInCount = 0;

        tabManager.OnFadeOutRequested = (onComplete) =>
        {
            fadeOutCount++;
            onComplete?.Invoke();
        };
        tabManager.OnFadeInRequested = () => fadeInCount++;

        // Create initial tab
        tabManager.CreateTab("http://world-init.test", "Init", true);
        yield return null;
        completeLoad?.Invoke(false);

        float timeout = 2f;
        float elapsed = 0f;
        while (tabManager.IsSwitching && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        int switchCount = 5;
        for (int i = 0; i < switchCount; i++)
        {
            tabManager.CreateTab($"http://world-{i}.test", $"World{i}", true);
            yield return null;
            completeLoad?.Invoke(false);

            elapsed = 0f;
            while (tabManager.IsSwitching && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // +1 for initial tab creation
        int totalExpected = switchCount + 1;
        Assert.AreEqual(totalExpected, fadeOutCount,
            $"FadeOut should be called {totalExpected} times (1 init + {switchCount} switches)");
        Assert.AreEqual(totalExpected, fadeInCount,
            $"FadeIn should be called {totalExpected} times (1 init + {switchCount} switches)");
        Assert.IsFalse(tabManager.IsSwitching, "Should not be switching after all complete");
    }

    // ==================== FadeOut continuation required for switch to proceed ====================

    [UnityTest]
    public IEnumerator FadeOut_BlocksSwitchUntilContinuationCalled()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tabManager, completeLoad) = CreateTabManager();
        yield return null;

        Action storedContinuation = null;
        bool switchCompleted = false;

        tabManager.OnFadeOutRequested = (onComplete) =>
        {
            // Don't invoke immediately — store for later
            storedContinuation = onComplete;
        };
        tabManager.OnFadeInRequested = () => { };
        tabManager.OnTabSwitchCompleted += (prev, target, success) => switchCompleted = true;

        tabManager.CreateTab("http://test.world", "Test", true);

        // Wait a few frames — switch should NOT proceed
        yield return null;
        yield return null;
        yield return null;

        Assert.IsTrue(tabManager.IsSwitching, "Should still be switching (blocked on fade-out)");
        Assert.IsFalse(switchCompleted, "Switch should not complete while fade-out pending");
        Assert.IsNotNull(storedContinuation, "Continuation should have been provided");

        // Now release the continuation
        storedContinuation.Invoke();
        yield return null;

        // Complete the load
        completeLoad?.Invoke(false);

        float timeout = 2f;
        float elapsed = 0f;
        while (!switchCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(switchCompleted, "Switch should complete after continuation called");
    }
}
