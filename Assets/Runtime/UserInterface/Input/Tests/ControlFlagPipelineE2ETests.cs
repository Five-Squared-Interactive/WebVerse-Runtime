// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Input;
using FiveSQD.StraightFour.WorldState;
using World = FiveSQD.StraightFour.World.World;

/// <summary>
/// E2E PlayMode tests for the control flag pipeline.
/// Exercises the full chain: TabManager wired with OnWorldReadyForControlFlags callback
/// → world load via mock callback → flag restoration → VRRig state verified.
/// </summary>
public class ControlFlagPipelineE2ETests
{
    private List<GameObject> _testObjects = new List<GameObject>();
    private List<GameObject> _worldObjects = new List<GameObject>();
    private GameObject _tabManagerGO;

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = true;

        // Stop delayed thumbnail coroutines (0.5s, 2s, 15s) before they outlive the test
        if (_tabManagerGO != null)
            _tabManagerGO.GetComponent<TabManager>()?.StopAllCoroutines();

        if (_tabManagerGO != null)
            UnityEngine.Object.DestroyImmediate(_tabManagerGO);

        foreach (var obj in _worldObjects)
        {
            if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
        }
        _worldObjects.Clear();

        VRRigTestHelper.Cleanup(_testObjects);
    }

    /// <summary>
    /// Set up a full pipeline: TabManager + WorldStateManager + VRRig + mock callbacks.
    /// WorldStateManager.Initialize() is intentionally NOT called — AddSnapshot is a no-op,
    /// keeping tests scoped to the control flag pipeline only.
    /// </summary>
    private (TabManager tabManager, VRRig vrRig) SetupPipeline(
        Dictionary<string, Dictionary<string, string>> worldFlags)
    {
        VRRig vrRig = VRRigTestHelper.CreateWiredVRRig(_testObjects);

        _tabManagerGO = new GameObject("TabManagerHost");
        var tabManager = _tabManagerGO.AddComponent<TabManager>();
        var stateManager = _tabManagerGO.AddComponent<WorldStateManager>();

        Func<string, string, Action<World, bool>, Coroutine> mockLoadCallback =
            (url, basePath, onComplete) =>
            {
                return tabManager.StartCoroutine(MockLoadCoroutine(url, worldFlags, onComplete));
            };

        Action<World> mockUnloadCallback = (world) => { };

        tabManager.Initialize(stateManager, mockLoadCallback, mockUnloadCallback);

        // Wire the control flag callback matching TabUIIntegration pattern
        tabManager.OnWorldReadyForControlFlags = (world) =>
        {
            if (vrRig == null) return;

            if (world != null && world.CachedControlFlags != null && world.CachedControlFlags.Count > 0)
            {
                vrRig.ApplyCachedControlFlags(world.CachedControlFlags);
            }
            else
            {
                vrRig.ApplyDefaultControlFlags();
            }
        };

        return (tabManager, vrRig);
    }

    private IEnumerator MockLoadCoroutine(
        string url,
        Dictionary<string, Dictionary<string, string>> worldFlags,
        Action<World, bool> onComplete)
    {
        var worldGO = new GameObject("MockWorld_" + url);
        _worldObjects.Add(worldGO);
        var world = worldGO.AddComponent<World>();

        if (worldFlags != null && worldFlags.TryGetValue(url, out var flags))
        {
            world.CachedControlFlags = flags;
        }

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

    // ==================== Task 2: Flagged world restores cached flags ====================

    [UnityTest]
    public IEnumerator Pipeline_SwitchToFlaggedWorld_RestoresCachedFlags()
    {
        LogAssert.ignoreFailingMessages = true;

        var worldFlags = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "world-a", new Dictionary<string, string>
                {
                    { "joystickmotion", "false" },
                    { "leftvrpointer", "ui" },
                    { "rightvrpointer", "teleport" },
                    { "turnlocomotion", "smooth" }
                }
            }
        };

        var (tabManager, rig) = SetupPipeline(worldFlags);
        yield return null;

        tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);

        Assert.IsFalse(rig.joystickMotionEnabled, "joystickmotion should be false");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, "leftvrpointer should be UI");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.rightPointerMode, "rightvrpointer should be Teleport");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, "turnlocomotion should be Smooth");
    }

    // ==================== Task 3: Unflagged world applies defaults ====================

    [UnityTest]
    public IEnumerator Pipeline_SwitchToUnflaggedWorld_AppliesDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var worldFlags = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "world-a", new Dictionary<string, string>
                {
                    { "joystickmotion", "false" },
                    { "leftvrpointer", "ui" }
                }
            }
            // world-b not in dict → null CachedControlFlags
        };

        var (tabManager, rig) = SetupPipeline(worldFlags);
        yield return null;

        // Switch to flagged world first
        tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);
        Assert.IsFalse(rig.joystickMotionEnabled, "World A: joystick should be false");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, "World A: left should be UI");

        // Switch to unflagged world
        var tabB = tabManager.CreateTab("world-b", "World B", makeActive: false);
        tabManager.SwitchToTab(tabB.Id);
        yield return WaitForTabSwitch(tabManager);

        Assert.IsTrue(rig.joystickMotionEnabled, "joystickmotion should be true (default)");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "leftvrpointer should be Teleport (default)");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, "rightvrpointer should be UI (default)");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, "turnlocomotion should be Snap (default)");
    }

    // ==================== Task 4: Consecutive switches ====================

    [UnityTest]
    public IEnumerator Pipeline_ConsecutiveSwitches_CorrectFlagsEveryTime()
    {
        LogAssert.ignoreFailingMessages = true;

        var worldFlags = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "world-a", new Dictionary<string, string>
                {
                    { "joystickmotion", "false" },
                    { "leftvrpointer", "ui" },
                    { "rightvrpointer", "teleport" },
                    { "turnlocomotion", "smooth" }
                }
            }
            // world-b not in dict → defaults
        };

        var (tabManager, rig) = SetupPipeline(worldFlags);
        yield return null;

        // Create both tabs
        var tabA = tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);

        var tabB = tabManager.CreateTab("world-b", "World B", makeActive: false);

        for (int i = 0; i < 3; i++)
        {
            // Verify world A flags (first iteration already switched, subsequent need explicit switch)
            if (i > 0)
            {
                tabManager.SwitchToTab(tabA.Id);
                yield return WaitForTabSwitch(tabManager);
            }

            Assert.IsFalse(rig.joystickMotionEnabled, $"Iteration {i}: A joystick=false");
            Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, $"Iteration {i}: A left=UI");
            Assert.AreEqual(VRRig.PointerMode.Teleport, rig.rightPointerMode, $"Iteration {i}: A right=Teleport");
            Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, $"Iteration {i}: A smooth");

            // Switch to world B → defaults
            tabManager.SwitchToTab(tabB.Id);
            yield return WaitForTabSwitch(tabManager);

            Assert.IsTrue(rig.joystickMotionEnabled, $"Iteration {i}: B joystick=true (default)");
            Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, $"Iteration {i}: B left=Teleport (default)");
            Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, $"Iteration {i}: B right=UI (default)");
            Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, $"Iteration {i}: B snap (default)");
        }
    }

    // ==================== Task 5: Webpage tab resets to defaults ====================

    [UnityTest]
    public IEnumerator Pipeline_WebpageTabSwitch_ResetsToDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var worldFlags = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "world-a", new Dictionary<string, string>
                {
                    { "joystickmotion", "false" },
                    { "leftvrpointer", "ui" },
                    { "rightvrpointer", "teleport" },
                    { "turnlocomotion", "smooth" }
                }
            }
        };

        var (tabManager, rig) = SetupPipeline(worldFlags);
        yield return null;

        // Switch to flagged world
        tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);
        Assert.IsFalse(rig.joystickMotionEnabled, "World A: joystick should be false");

        // Create webpage tab — ordering critical: makeActive:false, then set IsWebPage, then switch
        var webTab = tabManager.CreateTab("https://example.com", "Web", makeActive: false);
        webTab.IsWebPage = true;
        Assert.IsTrue(webTab.IsWebPage, "IsWebPage must be set before SwitchToTab");
        tabManager.SwitchToTab(webTab.Id);
        yield return WaitForTabSwitch(tabManager);

        Assert.IsTrue(rig.joystickMotionEnabled, "joystickmotion should be true (default) after webpage");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "leftvrpointer should be Teleport (default) after webpage");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, "rightvrpointer should be UI (default) after webpage");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, "turnlocomotion should be Snap (default) after webpage");
    }

    // ==================== Task 6: Callback invoked with correct world ====================

    [UnityTest]
    public IEnumerator Pipeline_CallbackInvoked_WithCorrectWorld()
    {
        LogAssert.ignoreFailingMessages = true;

        var expectedFlags = new Dictionary<string, string>
        {
            { "joystickmotion", "false" },
            { "leftvrpointer", "ui" }
        };

        var worldFlags = new Dictionary<string, Dictionary<string, string>>
        {
            { "world-a", expectedFlags }
        };

        _tabManagerGO = new GameObject("TabManagerHost");
        var tabManager = _tabManagerGO.AddComponent<TabManager>();
        var stateManager = _tabManagerGO.AddComponent<WorldStateManager>();

        Func<string, string, Action<World, bool>, Coroutine> mockLoadCallback =
            (url, basePath, onComplete) =>
            {
                return tabManager.StartCoroutine(MockLoadCoroutine(url, worldFlags, onComplete));
            };

        tabManager.Initialize(stateManager, mockLoadCallback, (world) => { });

        // Track callback invocations
        int callbackCount = 0;
        World receivedWorld = null;

        tabManager.OnWorldReadyForControlFlags = (world) =>
        {
            callbackCount++;
            receivedWorld = world;
        };

        VRRig vrRig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);

        Assert.AreEqual(1, callbackCount, "Callback should be invoked exactly once");
        Assert.IsNotNull(receivedWorld, "Callback should receive a non-null World");
        Assert.IsNotNull(receivedWorld.CachedControlFlags, "World should have CachedControlFlags");
        Assert.AreEqual(expectedFlags.Count, receivedWorld.CachedControlFlags.Count, "CachedControlFlags count should match");
        Assert.AreEqual("false", receivedWorld.CachedControlFlags["joystickmotion"], "joystickmotion flag should match");
        Assert.AreEqual("ui", receivedWorld.CachedControlFlags["leftvrpointer"], "leftvrpointer flag should match");
    }
}
