// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Input;

/// <summary>
/// Tests for VRRig.ApplyDefaultControlFlags() — verifies that sensible VR locomotion
/// and interaction defaults are applied for worlds without VEML control flags.
/// </summary>
public class DefaultControlFlagTests
{
    private List<GameObject> _testObjects;

    [UnityTest]
    public IEnumerator ApplyDefaultControlFlags_SetsLeftPointerToTeleport()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.ApplyDefaultControlFlags();

        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode);
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaultControlFlags_SetsRightPointerToUI()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.ApplyDefaultControlFlags();

        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode);
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaultControlFlags_EnablesJoystickMotion()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.ApplyDefaultControlFlags();

        Assert.IsTrue(rig.joystickMotionEnabled);
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaultControlFlags_SetsSnapTurn()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.ApplyDefaultControlFlags();

        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode);
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaultControlFlags_PreservesGrabDefaults()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        rig.ApplyDefaultControlFlags();

        Assert.IsTrue(rig.leftDirectGrabEnabled);
        Assert.IsTrue(rig.rightDirectGrabEnabled);
        Assert.IsTrue(rig.leftPokerEnabled);
        Assert.IsTrue(rig.rightPokerEnabled);
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaultControlFlags_OverridesNonePointerMode()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        Assert.AreEqual(VRRig.PointerMode.None, rig.leftPointerMode,
            "Precondition: Initialize() should set leftPointerMode to None (the bug)");

        rig.ApplyDefaultControlFlags();

        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode,
            "ApplyDefaultControlFlags should fix leftPointerMode from None to Teleport");
        VRRigTestHelper.Cleanup(_testObjects);
    }
}
