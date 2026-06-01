// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Input;

/// <summary>
/// Tests for SteamVR locomotion default parity with Quest 3.
/// Verifies that VRRig.ApplyDefaultControlFlags() produces identical results
/// regardless of platform initialization path (Quest3Mode vs DesktopMode.EnableVR).
/// </summary>
public class SteamVRDefaultTests
{
    private List<GameObject> _testObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        VRRigTestHelper.Cleanup(_testObjects);
    }

    /// <summary>
    /// Simulates DesktopMode.EnableVR() flow: Initialize() then ApplyDefaultControlFlags().
    /// Verifies all 4 locomotion defaults match the expected values.
    /// </summary>
    [UnityTest]
    public IEnumerator EnableVR_AppliesDefaultControlFlags()
    {
        LogAssert.ignoreFailingMessages = true;
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        // Simulate DesktopMode.EnableVR() flow
        rig.Initialize();
        rig.ApplyDefaultControlFlags();

        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode,
            "SteamVR: left pointer should be Teleport after EnableVR flow");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode,
            "SteamVR: right pointer should be UI after EnableVR flow");
        Assert.IsTrue(rig.joystickMotionEnabled,
            "SteamVR: joystick motion should be enabled after EnableVR flow");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode,
            "SteamVR: turn locomotion should be Snap after EnableVR flow");
    }

    /// <summary>
    /// Cross-platform parity assertion: SteamVR defaults match Quest 3 defaults exactly.
    /// Both platforms use the same VRRig.ApplyDefaultControlFlags() method.
    /// </summary>
    [UnityTest]
    public IEnumerator SteamVR_DefaultsMatchQuest3()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();

        // Create two rigs to simulate both platform paths
        VRRig quest3Rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        VRRig steamVRRig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        // Quest 3 path: Initialize() + ApplyDefaultControlFlags() (from Quest3Mode.InitializeVR)
        quest3Rig.Initialize();
        quest3Rig.ApplyDefaultControlFlags();

        // SteamVR path: Initialize() + ApplyDefaultControlFlags() (from DesktopMode.EnableVR)
        steamVRRig.Initialize();
        steamVRRig.ApplyDefaultControlFlags();

        Assert.AreEqual(quest3Rig.leftPointerMode, steamVRRig.leftPointerMode,
            "leftPointerMode parity: Quest 3 and SteamVR must match");
        Assert.AreEqual(quest3Rig.rightPointerMode, steamVRRig.rightPointerMode,
            "rightPointerMode parity: Quest 3 and SteamVR must match");
        Assert.AreEqual(quest3Rig.joystickMotionEnabled, steamVRRig.joystickMotionEnabled,
            "joystickMotionEnabled parity: Quest 3 and SteamVR must match");
        Assert.AreEqual(quest3Rig.turnLocomotionMode, steamVRRig.turnLocomotionMode,
            "turnLocomotionMode parity: Quest 3 and SteamVR must match");
    }

    /// <summary>
    /// Verifies that ApplyDefaultControlFlags() correctly overrides Initialize() defaults.
    /// Initialize() sets leftPointerMode to None and joystickMotion conditionally —
    /// ApplyDefaultControlFlags() must fix both regardless of platform.
    /// </summary>
    [UnityTest]
    public IEnumerator SteamVR_DefaultsOverrideInitialize()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();

        // Precondition: Initialize() sets leftPointerMode to None (the original bug)
        Assert.AreEqual(VRRig.PointerMode.None, rig.leftPointerMode,
            "Precondition: Initialize() should set leftPointerMode to None");

        rig.ApplyDefaultControlFlags();

        // Postcondition: ApplyDefaultControlFlags() fixes to Teleport
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode,
            "ApplyDefaultControlFlags should override None to Teleport");
        Assert.IsTrue(rig.joystickMotionEnabled,
            "ApplyDefaultControlFlags should enable joystick motion");
    }
}
