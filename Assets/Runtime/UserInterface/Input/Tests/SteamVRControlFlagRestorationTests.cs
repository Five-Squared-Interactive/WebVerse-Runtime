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
/// Integration tests for SteamVR control flag restoration on tab switch.
/// Verifies the platform-agnostic TabManager → OnWorldReadyForControlFlags → VRRig pipeline
/// works correctly for SteamVR users (same behavior as Quest 3).
/// </summary>
public class SteamVRControlFlagRestorationTests
{
    private List<GameObject> _testObjects = new List<GameObject>();
    private List<GameObject> _worldObjects = new List<GameObject>();
    private GameObject _tabManagerGO;

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = true;

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

    private (TabManager tabManager, VRRig vrRig) SetupPipeline(
        Dictionary<string, Dictionary<string, string>> worldFlags)
    {
        // Simulate SteamVR path: Initialize() + ApplyDefaultControlFlags()
        VRRig vrRig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        vrRig.Initialize();
        vrRig.ApplyDefaultControlFlags();

        _tabManagerGO = new GameObject("TabManagerHost");
        var tabManager = _tabManagerGO.AddComponent<TabManager>();
        var stateManager = _tabManagerGO.AddComponent<WorldStateManager>();

        Func<string, string, Action<World, bool>, Coroutine> mockLoadCallback =
            (url, basePath, onComplete) =>
            {
                return tabManager.StartCoroutine(MockLoadCoroutine(url, worldFlags, onComplete));
            };

        tabManager.Initialize(stateManager, mockLoadCallback, (world) => { });

        // Wire control flag callback (same as TabUIIntegration pattern)
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

    /// <summary>
    /// Verifies cached control flags are restored after tab switch on SteamVR.
    /// The VRRig is initialized via the SteamVR path (Initialize + ApplyDefaultControlFlags).
    /// </summary>
    [UnityTest]
    public IEnumerator TabSwitch_RestoresCachedFlags_SteamVRRig()
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

        // Verify defaults are active before world load
        Assert.IsTrue(rig.joystickMotionEnabled, "Precondition: defaults should be active");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "Precondition: left=Teleport");

        // Load flagged world
        tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);

        Assert.IsFalse(rig.joystickMotionEnabled, "joystickmotion should be false (cached)");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, "leftvrpointer should be UI (cached)");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.rightPointerMode, "rightvrpointer should be Teleport (cached)");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, "turnlocomotion should be Smooth (cached)");
    }

    /// <summary>
    /// Verifies defaults are applied when switching to a world with no cached flags.
    /// </summary>
    [UnityTest]
    public IEnumerator TabSwitch_AppliesDefaults_WhenNoCachedFlags_SteamVR()
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
            // world-b has no flags → defaults
        };

        var (tabManager, rig) = SetupPipeline(worldFlags);
        yield return null;

        // Load flagged world first
        tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);
        Assert.IsFalse(rig.joystickMotionEnabled, "World A: joystick should be false");

        // Switch to unflagged world
        var tabB = tabManager.CreateTab("world-b", "World B", makeActive: false);
        tabManager.SwitchToTab(tabB.Id);
        yield return WaitForTabSwitch(tabManager);

        Assert.IsTrue(rig.joystickMotionEnabled, "Defaults: joystickmotion should be true");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "Defaults: left=Teleport");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, "Defaults: right=UI");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, "Defaults: snap turn");
    }

    /// <summary>
    /// Verifies 5 consecutive tab switches produce correct flags every time.
    /// </summary>
    [UnityTest]
    public IEnumerator ConsecutiveTabSwitches_CorrectFlags_SteamVR()
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

        var tabA = tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);

        var tabB = tabManager.CreateTab("world-b", "World B", makeActive: false);

        for (int i = 0; i < 5; i++)
        {
            // Switch to A (if not already there)
            if (i > 0)
            {
                tabManager.SwitchToTab(tabA.Id);
                yield return WaitForTabSwitch(tabManager);
            }

            Assert.IsFalse(rig.joystickMotionEnabled, $"Iteration {i}: A joystick=false");
            Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, $"Iteration {i}: A left=UI");
            Assert.AreEqual(VRRig.PointerMode.Teleport, rig.rightPointerMode, $"Iteration {i}: A right=Teleport");
            Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, $"Iteration {i}: A smooth");

            // Switch to B → defaults
            tabManager.SwitchToTab(tabB.Id);
            yield return WaitForTabSwitch(tabManager);

            Assert.IsTrue(rig.joystickMotionEnabled, $"Iteration {i}: B joystick=true (default)");
            Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, $"Iteration {i}: B left=Teleport (default)");
            Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, $"Iteration {i}: B right=UI (default)");
            Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, $"Iteration {i}: B snap (default)");
        }
    }

    /// <summary>
    /// Verifies tab switch works correctly without FadeController (SteamVR desktop mode
    /// before Story 5.2 wires comfort components). OnFadeOutRequested/OnFadeInRequested
    /// are null — tab switch should proceed normally.
    /// </summary>
    [UnityTest]
    public IEnumerator NullFadeController_TabSwitchStillWorks()
    {
        LogAssert.ignoreFailingMessages = true;

        var worldFlags = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "world-a", new Dictionary<string, string>
                {
                    { "joystickmotion", "false" }
                }
            }
        };

        var (tabManager, rig) = SetupPipeline(worldFlags);
        yield return null;

        // Explicitly ensure no fade callbacks (simulating SteamVR without comfort components)
        Assert.IsNull(tabManager.OnFadeOutRequested, "Precondition: no fade-out callback");
        Assert.IsNull(tabManager.OnFadeInRequested, "Precondition: no fade-in callback");

        tabManager.CreateTab("world-a", "World A", makeActive: true);
        yield return WaitForTabSwitch(tabManager);

        Assert.IsFalse(rig.joystickMotionEnabled, "Flags should be restored even without FadeController");

        // Switch to unflagged world
        var tabB = tabManager.CreateTab("world-b", "World B", makeActive: false);
        tabManager.SwitchToTab(tabB.Id);
        yield return WaitForTabSwitch(tabManager);

        Assert.IsTrue(rig.joystickMotionEnabled, "Defaults should apply even without FadeController");
    }
}
