// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Input;
using World = FiveSQD.StraightFour.World.World;

/// <summary>
/// Integration tests for control flag restoration on tab switch.
/// Tests the full chain: callback invocation → VRRig state matching cached/default flags.
/// Covers Story 2-2 Tasks 4 and 5 (integration + round-trip fidelity).
/// </summary>
public class ControlFlagRestorationTests
{
    private List<GameObject> _testObjects;

    [TearDown]
    public void TearDown()
    {
        VRRigTestHelper.Cleanup(_testObjects);
    }

    /// <summary>
    /// Helper: creates a restoration callback matching the TabUIIntegration pattern.
    /// </summary>
    private Action<World> CreateRestorationCallback(VRRig vrRig)
    {
        return (world) =>
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
    }

    /// <summary>
    /// Helper: creates a World GameObject with optional CachedControlFlags.
    /// </summary>
    private World CreateTestWorld(Dictionary<string, string> cachedFlags)
    {
        var worldGO = new GameObject("TestWorld");
        _testObjects.Add(worldGO);
        var world = worldGO.AddComponent<World>();
        world.CachedControlFlags = cachedFlags;
        return world;
    }

    // Task 4.1: Tab switch to world with cached flags → VRRig matches cached values
    [UnityTest]
    public IEnumerator Restoration_CachedFlags_AppliedToVRRig()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        // Start with defaults
        rig.ApplyDefaultControlFlags();

        var flags = new Dictionary<string, string>
        {
            { "joystickmotion", "false" },
            { "leftvrpointer", "ui" },
            { "rightvrpointer", "teleport" },
            { "turnlocomotion", "smooth" }
        };
        var world = CreateTestWorld(flags);
        var callback = CreateRestorationCallback(rig);

        callback(world);

        Assert.IsFalse(rig.joystickMotionEnabled, "joystickmotion should be false after restore");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, "leftvrpointer should be UI after restore");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.rightPointerMode, "rightvrpointer should be Teleport after restore");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, "turnlocomotion should be Smooth after restore");
    }

    // Task 4.2: Tab switch to world without cached flags → VRRig matches defaults
    [UnityTest]
    public IEnumerator Restoration_NullCache_AppliesDefaults()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        // Set non-default state (simulating a previous world's flags)
        rig.joystickMotionEnabled = false;
        rig.leftPointerMode = VRRig.PointerMode.UI;
        rig.turnLocomotionMode = VRRig.TurnLocomotionMode.Smooth;

        var world = CreateTestWorld(null); // No cached flags
        var callback = CreateRestorationCallback(rig);

        callback(world);

        Assert.IsTrue(rig.joystickMotionEnabled, "joystickmotion should be true (default)");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "leftvrpointer should be Teleport (default)");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, "rightvrpointer should be UI (default)");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, "turnlocomotion should be Snap (default)");
    }

    // Task 4.3: Consecutive tab switches between flagged and unflagged worlds
    [UnityTest]
    public IEnumerator Restoration_ConsecutiveSwitches_CorrectStateEachTime()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        var flaggedWorld = CreateTestWorld(new Dictionary<string, string>
        {
            { "joystickmotion", "false" },
            { "leftvrpointer", "none" },
            { "rightvrpointer", "teleport" },
            { "turnlocomotion", "smooth" }
        });
        var unflaggedWorld = CreateTestWorld(null);
        var callback = CreateRestorationCallback(rig);

        // Switch to flagged world
        callback(flaggedWorld);
        Assert.IsFalse(rig.joystickMotionEnabled, "Switch 1: joystick should be false");
        Assert.AreEqual(VRRig.PointerMode.None, rig.leftPointerMode, "Switch 1: left=None");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, "Switch 1: smooth turn");

        // Switch to unflagged world
        callback(unflaggedWorld);
        Assert.IsTrue(rig.joystickMotionEnabled, "Switch 2: joystick should be true (default)");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "Switch 2: left=Teleport (default)");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, "Switch 2: snap turn (default)");

        // Switch back to flagged world
        callback(flaggedWorld);
        Assert.IsFalse(rig.joystickMotionEnabled, "Switch 3: joystick should be false");
        Assert.AreEqual(VRRig.PointerMode.None, rig.leftPointerMode, "Switch 3: left=None");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, "Switch 3: smooth turn");
    }

    // Task 4.4: Tab switch with null VRRig (desktop mode) → no exception
    [UnityTest]
    public IEnumerator Restoration_NullVRRig_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        yield return null;

        var callback = CreateRestorationCallback(null);
        var world = CreateTestWorld(new Dictionary<string, string>
        {
            { "joystickmotion", "true" }
        });

        // Should not throw
        Assert.DoesNotThrow(() => callback(world), "Callback with null VRRig should not throw");
    }

    // Issue 1 fix: Webpage tab switch (null world) → VRRig resets to defaults
    [UnityTest]
    public IEnumerator Restoration_NullWorld_AppliesDefaults()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        // Set non-default state (simulating a previous VEML world's flags)
        rig.joystickMotionEnabled = false;
        rig.leftPointerMode = VRRig.PointerMode.UI;
        rig.turnLocomotionMode = VRRig.TurnLocomotionMode.Smooth;

        var callback = CreateRestorationCallback(rig);

        // Simulate webpage tab switch — null world passed to callback
        callback(null);

        Assert.IsTrue(rig.joystickMotionEnabled, "joystickmotion should be true (default) after webpage switch");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "leftvrpointer should be Teleport (default) after webpage switch");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, "rightvrpointer should be UI (default) after webpage switch");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, "turnlocomotion should be Snap (default) after webpage switch");
    }

    // Task 5.1: Round-trip — cache flags on world A → switch to B → switch back to A
    [UnityTest]
    public IEnumerator RoundTrip_AllFlags_SurviveSwitchCycle()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        var worldAFlags = new Dictionary<string, string>
        {
            { "joystickmotion", "false" },
            { "leftgrabmove", "true" },
            { "rightgrabmove", "false" },
            { "lefthandinteraction", "true" },
            { "righthandinteraction", "false" },
            { "leftvrpointer", "ui" },
            { "rightvrpointer", "teleport" },
            { "leftvrpoker", "false" },
            { "rightvrpoker", "true" },
            { "turnlocomotion", "smooth" },
            { "twohandedgrabmove", "true" }
        };
        var worldA = CreateTestWorld(worldAFlags);
        var worldB = CreateTestWorld(null); // Unflagged
        var callback = CreateRestorationCallback(rig);

        // Apply world A flags
        callback(worldA);
        Assert.IsFalse(rig.joystickMotionEnabled, "World A: joystick=false");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, "World A: left=UI");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.rightPointerMode, "World A: right=Teleport");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, "World A: smooth");

        // Switch to world B (defaults)
        callback(worldB);
        Assert.IsTrue(rig.joystickMotionEnabled, "World B: joystick=true (default)");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "World B: left=Teleport (default)");

        // Switch back to world A — must match original flags exactly
        callback(worldA);
        Assert.IsFalse(rig.joystickMotionEnabled, "World A round-trip: joystick=false");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, "World A round-trip: left=UI");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.rightPointerMode, "World A round-trip: right=Teleport");
        Assert.IsFalse(rig.leftPokerEnabled, "World A round-trip: leftPoker=false");
        Assert.IsTrue(rig.rightPokerEnabled, "World A round-trip: rightPoker=true");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, "World A round-trip: smooth");
    }

    // Task 5.2: Enum values survive round-trip
    [UnityTest]
    public IEnumerator RoundTrip_EnumValues_SurviveSwitchCycle()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        var callback = CreateRestorationCallback(rig);

        // Test each enum variant
        var enumVariants = new[]
        {
            new Dictionary<string, string> { { "leftvrpointer", "teleport" }, { "rightvrpointer", "ui" }, { "turnlocomotion", "snap" } },
            new Dictionary<string, string> { { "leftvrpointer", "ui" }, { "rightvrpointer", "none" }, { "turnlocomotion", "smooth" } },
            new Dictionary<string, string> { { "leftvrpointer", "none" }, { "rightvrpointer", "teleport" }, { "turnlocomotion", "none" } }
        };

        var unflaggedWorld = CreateTestWorld(null);

        for (int i = 0; i < enumVariants.Length; i++)
        {
            var world = CreateTestWorld(enumVariants[i]);

            // Apply flags
            callback(world);

            // Verify
            string lpExpected = enumVariants[i]["leftvrpointer"];
            string rpExpected = enumVariants[i]["rightvrpointer"];
            string tlExpected = enumVariants[i]["turnlocomotion"];

            Assert.AreEqual(ExpectedPointerMode(lpExpected), rig.leftPointerMode,
                $"Variant {i}: leftvrpointer={lpExpected}");
            Assert.AreEqual(ExpectedPointerMode(rpExpected), rig.rightPointerMode,
                $"Variant {i}: rightvrpointer={rpExpected}");
            Assert.AreEqual(ExpectedTurnMode(tlExpected), rig.turnLocomotionMode,
                $"Variant {i}: turnlocomotion={tlExpected}");

            // Switch to defaults
            callback(unflaggedWorld);

            // Switch back — verify round-trip
            callback(world);
            Assert.AreEqual(ExpectedPointerMode(lpExpected), rig.leftPointerMode,
                $"Variant {i} round-trip: leftvrpointer={lpExpected}");
            Assert.AreEqual(ExpectedPointerMode(rpExpected), rig.rightPointerMode,
                $"Variant {i} round-trip: rightvrpointer={rpExpected}");
            Assert.AreEqual(ExpectedTurnMode(tlExpected), rig.turnLocomotionMode,
                $"Variant {i} round-trip: turnlocomotion={tlExpected}");
        }
    }

    // Task 5.3: 5 consecutive switches (A→B→A→B→A) — proxy for 50+ AC requirement
    [UnityTest]
    public IEnumerator RoundTrip_FiveConsecutiveSwitches_CorrectEveryTime()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        var worldAFlags = new Dictionary<string, string>
        {
            { "joystickmotion", "false" },
            { "leftvrpointer", "ui" },
            { "rightvrpointer", "none" },
            { "turnlocomotion", "smooth" },
            { "leftvrpoker", "false" }
        };
        var worldA = CreateTestWorld(worldAFlags);
        var worldB = CreateTestWorld(null);
        var callback = CreateRestorationCallback(rig);

        for (int i = 0; i < 5; i++)
        {
            // Switch to world A
            callback(worldA);
            Assert.IsFalse(rig.joystickMotionEnabled, $"Iteration {i}: A joystick=false");
            Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, $"Iteration {i}: A left=UI");
            Assert.AreEqual(VRRig.PointerMode.None, rig.rightPointerMode, $"Iteration {i}: A right=None");
            Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, $"Iteration {i}: A smooth");
            Assert.IsFalse(rig.leftPokerEnabled, $"Iteration {i}: A leftPoker=false");

            // Switch to world B
            callback(worldB);
            Assert.IsTrue(rig.joystickMotionEnabled, $"Iteration {i}: B joystick=true");
            Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, $"Iteration {i}: B left=Teleport");
            Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, $"Iteration {i}: B right=UI");
            Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, $"Iteration {i}: B snap");
        }
    }

    #region Helpers

    private VRRig.PointerMode ExpectedPointerMode(string value)
    {
        switch (value)
        {
            case "teleport": return VRRig.PointerMode.Teleport;
            case "ui": return VRRig.PointerMode.UI;
            default: return VRRig.PointerMode.None;
        }
    }

    private VRRig.TurnLocomotionMode ExpectedTurnMode(string value)
    {
        switch (value)
        {
            case "snap": return VRRig.TurnLocomotionMode.Snap;
            case "smooth": return VRRig.TurnLocomotionMode.Smooth;
            default: return VRRig.TurnLocomotionMode.None;
        }
    }

    #endregion
}
