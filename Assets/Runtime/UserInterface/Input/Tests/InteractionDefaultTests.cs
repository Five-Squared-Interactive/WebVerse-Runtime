// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Input;

/// <summary>
/// Tests for VR interaction defaults (Story 1.2) — verifies that pointer ray, grab, poke,
/// and hand tracking interactions work correctly after Initialize() + ApplyDefaultControlFlags().
/// </summary>
public class InteractionDefaultTests
{
    private List<GameObject> _testObjects;

    // ── Task 1: Pointer ray defaults (AC#1, AC#5) ──

    [UnityTest]
    public IEnumerator ApplyDefaults_EnablesRightRayInteractor()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        rig.ApplyDefaultControlFlags();

        Assert.AreEqual(VRRig.PointerMode.UI, rig.rightPointerMode,
            "Right hand should be UI pointer ray after defaults");
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaults_DisablesLeftRayInteractor()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        rig.ApplyDefaultControlFlags();

        Assert.AreEqual(VRRig.PointerMode.Teleport, rig.leftPointerMode,
            "Left hand should be Teleport after defaults");
        Assert.IsFalse(rig.leftRayInteractor.enabled,
            "Left ray interactor should be disabled when left hand is Teleport mode");
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaults_DisablesRightTeleportInteractor()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        rig.ApplyDefaultControlFlags();

        Assert.IsFalse(rig.rightTeleportInteractor.enabled,
            "Right teleport interactor should be disabled when right hand is UI mode");
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaults_EnablesLeftTeleportInteractor()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        rig.ApplyDefaultControlFlags();

        Assert.IsTrue(rig.leftTeleportInteractor.enabled,
            "Left teleport interactor should be enabled when left hand is Teleport mode");
        VRRigTestHelper.Cleanup(_testObjects);
    }

    // ── Task 2: Grab interaction defaults (AC#2, AC#5) ──

    [UnityTest]
    public IEnumerator ApplyDefaults_PreservesLeftDirectGrab()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        Assert.IsTrue(rig.leftDirectGrabEnabled,
            "Precondition: left grab should be enabled after Initialize()");

        rig.ApplyDefaultControlFlags();

        Assert.IsTrue(rig.leftDirectGrabEnabled,
            "Left direct grab should remain enabled after ApplyDefaultControlFlags");
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaults_PreservesRightDirectGrab()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        Assert.IsTrue(rig.rightDirectGrabEnabled,
            "Precondition: right grab should be enabled after Initialize()");

        rig.ApplyDefaultControlFlags();

        Assert.IsTrue(rig.rightDirectGrabEnabled,
            "Right direct grab should remain enabled after ApplyDefaultControlFlags");
        VRRigTestHelper.Cleanup(_testObjects);
    }

    // ── Task 3: Poke interaction defaults (AC#3, AC#5) ──

    [UnityTest]
    public IEnumerator ApplyDefaults_PreservesLeftPoke()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        Assert.IsTrue(rig.leftPokerEnabled,
            "Precondition: left poke should be enabled after Initialize()");

        rig.ApplyDefaultControlFlags();

        Assert.IsTrue(rig.leftPokerEnabled,
            "Left poke should remain enabled after ApplyDefaultControlFlags");
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaults_PreservesRightPoke()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.Initialize();
        Assert.IsTrue(rig.rightPokerEnabled,
            "Precondition: right poke should be enabled after Initialize()");

        rig.ApplyDefaultControlFlags();

        Assert.IsTrue(rig.rightPokerEnabled,
            "Right poke should remain enabled after ApplyDefaultControlFlags");
        VRRigTestHelper.Cleanup(_testObjects);
    }

    // ── Task 4: Hand tracking not disrupted (AC#4) ──

    [UnityTest]
    public IEnumerator ApplyDefaults_HandTrackingReferencesUnchanged()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        var handTrackingBefore = rig.handTracking;
        var inputModeManagerBefore = rig.inputModeManager;
        var enableHandTrackingBefore = rig.enableHandTracking;

        rig.ApplyDefaultControlFlags();

        Assert.AreEqual(handTrackingBefore, rig.handTracking,
            "handTracking reference should not be modified by ApplyDefaultControlFlags");
        Assert.AreEqual(inputModeManagerBefore, rig.inputModeManager,
            "inputModeManager reference should not be modified by ApplyDefaultControlFlags");
        Assert.AreEqual(enableHandTrackingBefore, rig.enableHandTracking,
            "enableHandTracking flag should not be modified by ApplyDefaultControlFlags");
        VRRigTestHelper.Cleanup(_testObjects);
    }

    [UnityTest]
    public IEnumerator ApplyDefaults_HandTrackingPreservedAfterInitialize()
    {
        LogAssert.ignoreFailingMessages = true;
        _testObjects = new List<GameObject>();
        VRRig rig = VRRigTestHelper.CreateWiredVRRig(_testObjects);
        yield return null;

        rig.enableHandTracking = true;
        rig.Initialize();

        var handTrackingAfterInit = rig.handTracking;
        var inputModeManagerAfterInit = rig.inputModeManager;

        rig.ApplyDefaultControlFlags();

        Assert.AreEqual(handTrackingAfterInit, rig.handTracking,
            "handTracking reference should survive ApplyDefaultControlFlags after Initialize");
        Assert.AreEqual(inputModeManagerAfterInit, rig.inputModeManager,
            "inputModeManager reference should survive ApplyDefaultControlFlags after Initialize");
        Assert.IsTrue(rig.enableHandTracking,
            "enableHandTracking should remain true after ApplyDefaultControlFlags");
        VRRigTestHelper.Cleanup(_testObjects);
    }
}
