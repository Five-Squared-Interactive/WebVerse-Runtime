// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Input;

/// <summary>
/// Tests for VRRig.ApplyCachedControlFlags() — verifies that cached control flags
/// from World.CachedControlFlags are correctly applied to the VR rig during tab switch.
/// </summary>
public class CachedControlFlagTests
{
    private List<GameObject> _testObjects;

    [TearDown]
    public void TearDown()
    {
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyCachedControlFlags_AllFlags_SetsAllProperties()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        var flags = new Dictionary<string, string>
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

        rig.ApplyCachedControlFlags(flags);

        // Verifiable properties (wired interactors in VRRigTestHelper):
        Assert.IsFalse(rig.joystickMotionEnabled, "joystickmotion should be false");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.leftPointerMode, "leftvrpointer should be UI");
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.rightPointerMode, "rightvrpointer should be Teleport");
        Assert.IsFalse(rig.leftPokerEnabled, "leftvrpoker should be false");
        Assert.IsTrue(rig.rightPokerEnabled, "rightvrpoker should be true");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, "turnlocomotion should be Smooth");

        // Note: leftGrabMoveEnabled, rightGrabMoveEnabled, twoHandedGrabMoveEnabled,
        // leftInteractionEnabled, rightInteractionEnabled are not verifiable because
        // VRRigTestHelper doesn't wire grab move providers or near-far interactors.
        // Setters silently no-op. Method correctness for these flags verified by code inspection.
    }

    [UnityTest]
    public IEnumerator ApplyCachedControlFlags_PartialFlags_SetsOnlySpecified()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        // Apply defaults first so we have known state
        rig.ApplyDefaultControlFlags();
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "Precondition: left=Teleport");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, "Precondition: snap turn");

        // Apply partial flags — only joystickmotion and turnlocomotion
        var flags = new Dictionary<string, string>
        {
            { "joystickmotion", "false" },
            { "turnlocomotion", "smooth" },
            { "leftvrpointer", "none" }
        };

        rig.ApplyCachedControlFlags(flags);

        Assert.IsFalse(rig.joystickMotionEnabled, "joystickmotion should be false");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode, "turnlocomotion should be Smooth");
        Assert.AreEqual(VRRig.PointerMode.None, rig.leftPointerMode, "leftvrpointer should be None");
        // rightPointerMode was not in flags — defaults applied first, so it should be at default (UI)
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, "rightvrpointer should be UI (default, unaffected by partial flags)");
    }

    [UnityTest]
    public IEnumerator ApplyCachedControlFlags_NullDictionary_FallsBackToDefaults()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        // Set non-default state first
        rig.joystickMotionEnabled = false;
        rig.leftPointerMode = VRRig.PointerMode.None;

        rig.ApplyCachedControlFlags(null);

        // Should have applied defaults
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "Null dict should fallback to Teleport");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, "Null dict should fallback to UI");
        Assert.IsTrue(rig.joystickMotionEnabled, "Null dict should fallback to joystick enabled");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, "Null dict should fallback to Snap");
    }

    [UnityTest]
    public IEnumerator ApplyCachedControlFlags_EmptyDictionary_FallsBackToDefaults()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        // Set non-default state first
        rig.joystickMotionEnabled = false;
        rig.leftPointerMode = VRRig.PointerMode.None;

        rig.ApplyCachedControlFlags(new Dictionary<string, string>());

        // Should have applied defaults
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode, "Empty dict should fallback to Teleport");
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode, "Empty dict should fallback to UI");
        Assert.IsTrue(rig.joystickMotionEnabled, "Empty dict should fallback to joystick enabled");
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode, "Empty dict should fallback to Snap");
    }

    [UnityTest]
    public IEnumerator ApplyCachedControlFlags_EnumValues_ParsedCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        // Test all pointer mode enum values
        rig.ApplyCachedControlFlags(new Dictionary<string, string>
        {
            { "leftvrpointer", "teleport" },
            { "rightvrpointer", "ui" },
            { "turnlocomotion", "snap" }
        });
        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode);
        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode);
        Assert.AreEqual(VRRig.TurnLocomotionMode.Snap, rig.turnLocomotionMode);

        // Test "none" values
        rig.ApplyCachedControlFlags(new Dictionary<string, string>
        {
            { "leftvrpointer", "none" },
            { "rightvrpointer", "none" },
            { "turnlocomotion", "none" }
        });
        Assert.AreEqual(VRRig.PointerMode.None, rig.leftPointerMode);
        Assert.AreEqual(VRRig.PointerMode.None, rig.rightPointerMode);
        Assert.AreEqual(VRRig.TurnLocomotionMode.None, rig.turnLocomotionMode);

        // Test smooth turn
        rig.ApplyCachedControlFlags(new Dictionary<string, string>
        {
            { "turnlocomotion", "smooth" }
        });
        Assert.AreEqual(VRRig.TurnLocomotionMode.Smooth, rig.turnLocomotionMode);
    }
}
