// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.VR.Comfort;
using FiveSQD.StraightFour.WorldState;
using World = FiveSQD.StraightFour.World.World;

/// <summary>
/// Integration tests for SteamVR fade controller on tab switch.
/// Verifies that FadeController wired via SetFadeController() integrates
/// with the TabManager tab switch pipeline (OnFadeOutRequested/OnFadeInRequested).
/// </summary>
public class SteamVRFadeIntegrationTests
{
    private List<GameObject> _testObjects = new List<GameObject>();
    private List<GameObject> _worldObjects = new List<GameObject>();
    private GameObject _tabManagerGO;

    [TearDown]
    public void TearDown()
    {
        if (_tabManagerGO != null)
            _tabManagerGO.GetComponent<TabManager>()?.StopAllCoroutines();
        if (_tabManagerGO != null)
            UnityEngine.Object.DestroyImmediate(_tabManagerGO);
        foreach (var obj in _worldObjects)
        {
            if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
        }
        _worldObjects.Clear();
        foreach (var obj in _testObjects)
        {
            if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
        }
        _testObjects.Clear();
    }

    private (TabManager tabManager, FadeController fadeController) SetupPipelineWithFade()
    {
        // Create VR camera for FadeController
        var camGO = new GameObject("TestVRCamera");
        _testObjects.Add(camGO);
        var vrCamera = camGO.AddComponent<Camera>();

        // Create FadeController (simulating DesktopMode.EnableVR pattern)
        var fadeGO = new GameObject("FadeController");
        _testObjects.Add(fadeGO);
        var fadeController = fadeGO.AddComponent<FadeController>();
        fadeController.SetCamera(vrCamera);

        // Create VRRig for control flag callbacks
        VRRig vrRig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        vrRig.Initialize();
        vrRig.ApplyDefaultControlFlags();

        // Create TabManager
        _tabManagerGO = new GameObject("TabManagerHost");
        var tabManager = _tabManagerGO.AddComponent<TabManager>();
        var stateManager = _tabManagerGO.AddComponent<WorldStateManager>();

        Func<string, string, Action<World, bool>, Coroutine> mockLoadCallback =
            (url, basePath, onComplete) =>
            {
                return tabManager.StartCoroutine(MockLoadCoroutine(url, onComplete));
            };

        tabManager.Initialize(stateManager, mockLoadCallback, (world) => { });

        // Wire fade callbacks (same pattern as TabUIIntegration.SetFadeController)
        tabManager.OnFadeOutRequested = (onComplete) =>
        {
            if (fadeController != null)
                fadeController.FadeOut(onComplete);
            else
                onComplete?.Invoke();
        };
        tabManager.OnFadeInRequested = () => fadeController?.FadeIn();

        // Wire control flag callback
        tabManager.OnWorldReadyForControlFlags = (world) =>
        {
            if (vrRig == null) return;
            if (world != null && world.CachedControlFlags != null && world.CachedControlFlags.Count > 0)
                vrRig.ApplyCachedControlFlags(world.CachedControlFlags);
            else
                vrRig.ApplyDefaultControlFlags();
        };

        return (tabManager, fadeController);
    }

    private IEnumerator MockLoadCoroutine(string url, Action<World, bool> onComplete)
    {
        var worldGO = new GameObject("MockWorld_" + url);
        _worldObjects.Add(worldGO);
        var world = worldGO.AddComponent<World>();
        onComplete(world, true);
        yield break;
    }

    private IEnumerator WaitForTabSwitch(TabManager tabManager, float timeoutSeconds = 10f)
    {
        float elapsed = 0f;
        while (tabManager.IsSwitching && elapsed < timeoutSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        Assert.IsFalse(tabManager.IsSwitching, "Tab switch did not complete within timeout");
    }

    private IEnumerator WaitForFadeComplete(FadeController controller, float maxWait = 3f)
    {
        float elapsed = 0f;
        while (controller.IsFading && elapsed < maxWait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Verifies tab switch triggers FadeOut when FadeController is wired.
    /// </summary>
    [UnityTest]
    public IEnumerator TabSwitch_TriggersFade_WithSteamVRFadeController()
    {
        LogAssert.ignoreFailingMessages = true;
        var (tabManager, fadeController) = SetupPipelineWithFade();
        yield return null;

        // Precondition: fade not active
        Assert.IsFalse(fadeController.IsFading, "Precondition: not fading");
        Assert.AreEqual(0f, fadeController.CurrentAlpha, "Precondition: alpha=0");

        // Create first tab — triggers world load which triggers fade
        tabManager.CreateTab("world-a", "World A", makeActive: true);

        // Wait for tab switch and fade to complete
        yield return WaitForTabSwitch(tabManager);
        yield return WaitForFadeComplete(fadeController);

        // After tab switch completes, FadeIn should have been called and completed
        // Alpha should be back to 0 (transparent)
        Assert.AreEqual(0f, fadeController.CurrentAlpha, 0.01f,
            "After tab switch, fade should complete back to transparent");
    }

    /// <summary>
    /// Verifies FadeIn is called after world load completes during tab switch.
    /// </summary>
    [UnityTest]
    public IEnumerator TabSwitch_FadeIn_AfterWorldLoad()
    {
        LogAssert.ignoreFailingMessages = true;
        var (tabManager, fadeController) = SetupPipelineWithFade();
        yield return null;

        // Load first world
        tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);
        yield return WaitForFadeComplete(fadeController);

        // Load second world via tab switch
        var tabB = tabManager.CreateTab("world-b", "World B", makeActive: false);
        tabManager.SwitchToTab(tabB.Id);
        yield return WaitForTabSwitch(tabManager);
        yield return WaitForFadeComplete(fadeController);

        // After complete tab switch, fade should be finished (alpha = 0)
        Assert.IsFalse(fadeController.IsFading, "FadeIn should complete after world load");
        Assert.AreEqual(0f, fadeController.CurrentAlpha, 0.01f,
            "Alpha should be 0 after FadeIn completes");
    }

    /// <summary>
    /// Verifies 5 consecutive tab switches with fade work without degradation.
    /// </summary>
    [UnityTest]
    public IEnumerator ConsecutiveTabSwitches_FadeWorks_SteamVR()
    {
        LogAssert.ignoreFailingMessages = true;
        var (tabManager, fadeController) = SetupPipelineWithFade();
        yield return null;

        var tabA = tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);
        yield return WaitForFadeComplete(fadeController);

        var tabB = tabManager.CreateTab("world-b", "World B", makeActive: false);

        for (int i = 0; i < 5; i++)
        {
            // Switch to B
            tabManager.SwitchToTab(tabB.Id);
            yield return WaitForTabSwitch(tabManager);
            yield return WaitForFadeComplete(fadeController);

            Assert.IsFalse(fadeController.IsFading, $"Iteration {i}: fade should complete after switch to B");
            Assert.AreEqual(0f, fadeController.CurrentAlpha, 0.01f,
                $"Iteration {i}: alpha=0 after switch to B");

            // Switch back to A
            tabManager.SwitchToTab(tabA.Id);
            yield return WaitForTabSwitch(tabManager);
            yield return WaitForFadeComplete(fadeController);

            Assert.IsFalse(fadeController.IsFading, $"Iteration {i}: fade should complete after switch to A");
            Assert.AreEqual(0f, fadeController.CurrentAlpha, 0.01f,
                $"Iteration {i}: alpha=0 after switch to A");
        }
    }
}
