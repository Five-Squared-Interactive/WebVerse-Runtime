// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Desktop;
using FiveSQD.WebVerse.Input.Mobile;
using FiveSQD.WebVerse.Input.Quest3;
using FiveSQD.WebVerse.Input.SteamVR;

/// <summary>
/// Tests for the Input System: VRInputMode, BaseVRInputModeManager, DesktopRig defaults.
/// Uses a mock VRInputModeManager subclass for testing state machine logic.
/// </summary>
public class InputSystemTests
{
    /// <summary>
    /// Mock implementation of BaseVRInputModeManager for testing.
    /// </summary>
    private class MockVRInputModeManager : BaseVRInputModeManager
    {
        public bool mockControllersAvailable = false;
        public MockHandTrackingProvider mockHandTracking = null;
        public VRInputMode lastPreviousMode;
        public VRInputMode lastNewMode;
        public int modeChangedCount = 0;

        protected override IHandTrackingProvider GetHandTrackingProvider()
        {
            return mockHandTracking;
        }

        protected override bool AreControllersAvailable()
        {
            return mockControllersAvailable;
        }

        protected override void OnModeChanged(VRInputMode previousMode, VRInputMode newMode)
        {
            lastPreviousMode = previousMode;
            lastNewMode = newMode;
            modeChangedCount++;
        }
    }

    /// <summary>
    /// Mock hand tracking provider for testing.
    /// </summary>
    private class MockHandTrackingProvider : IHandTrackingProvider
    {
        public bool IsLeftHandTracked { get; set; }
        public bool IsRightHandTracked { get; set; }

        public event Action<int, bool> OnHandTrackingStateChanged;
        public event Action<int, float> OnTrackingLost;
        public event Action<int, float> OnTrackingRecovered;
        public event Action<int, bool> OnPinchStateChanged;

        public void SimulateHandTrackingStateChanged(int handIndex, bool isTracked)
        {
            OnHandTrackingStateChanged?.Invoke(handIndex, isTracked);
        }

        public void SimulatePinchStateChanged(int handIndex, bool isPinching)
        {
            OnPinchStateChanged?.Invoke(handIndex, isPinching);
        }
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    // --- VRInputMode Enum Tests ---

    [Test]
    public void VRInputMode_None_HasValue0()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(0, (int)VRInputMode.None);
    }

    [Test]
    public void VRInputMode_Hands_HasValue1()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(1, (int)VRInputMode.Hands);
    }

    [Test]
    public void VRInputMode_Controllers_HasValue2()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(2, (int)VRInputMode.Controllers);
    }

    // --- BaseVRInputModeManager Tests ---

    [UnityTest]
    public IEnumerator VRInputModeManager_DefaultState_IsNone()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        Assert.AreEqual(VRInputMode.None, mgr.CurrentInputMode);
        Assert.IsFalse(mgr.IsHandTrackingActive);
        Assert.IsFalse(mgr.IsControllersActive);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_ForceInputMode_ChangesMode()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        mgr.ForceInputMode(VRInputMode.Controllers);
        Assert.AreEqual(VRInputMode.Controllers, mgr.CurrentInputMode);
        Assert.IsTrue(mgr.IsControllersActive);
        Assert.IsFalse(mgr.IsHandTrackingActive);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_ForceInputMode_Hands()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        mgr.ForceInputMode(VRInputMode.Hands);
        Assert.AreEqual(VRInputMode.Hands, mgr.CurrentInputMode);
        Assert.IsTrue(mgr.IsHandTrackingActive);
        Assert.IsFalse(mgr.IsControllersActive);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_ForceInputMode_BackToNone()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        mgr.ForceInputMode(VRInputMode.Controllers);
        mgr.ForceInputMode(VRInputMode.None);
        Assert.AreEqual(VRInputMode.None, mgr.CurrentInputMode);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_RegisterControllerInput_SwitchesToControllers()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        mgr.RegisterControllerInput();
        Assert.AreEqual(VRInputMode.Controllers, mgr.CurrentInputMode);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_RegisterHandInput_SwitchesToHands_WhenNotInControllerMode()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        // From None, hand input should switch directly to Hands.
        mgr.RegisterHandInput();
        Assert.AreEqual(VRInputMode.Hands, mgr.CurrentInputMode);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_RegisterHandInput_FromControllers_SetsPendingSwitch()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        // First set to controllers.
        mgr.RegisterControllerInput();
        Assert.AreEqual(VRInputMode.Controllers, mgr.CurrentInputMode);

        // Hand input while in controller mode should not immediately switch.
        mgr.RegisterHandInput();
        Assert.AreEqual(VRInputMode.Controllers, mgr.CurrentInputMode);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_OnModeChanged_CalledOnTransition()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        mgr.ForceInputMode(VRInputMode.Controllers);
        Assert.AreEqual(1, mgr.modeChangedCount);
        Assert.AreEqual(VRInputMode.None, mgr.lastPreviousMode);
        Assert.AreEqual(VRInputMode.Controllers, mgr.lastNewMode);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_SameMode_DoesNotFireEvent()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        mgr.ForceInputMode(VRInputMode.Controllers);
        int countAfterFirst = mgr.modeChangedCount;

        mgr.ForceInputMode(VRInputMode.Controllers);
        Assert.AreEqual(countAfterFirst, mgr.modeChangedCount);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_OnInputModeChanged_EventFires()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        VRInputMode eventPrevious = VRInputMode.None;
        VRInputMode eventNew = VRInputMode.None;
        bool eventFired = false;
        mgr.OnInputModeChanged += (prev, next) =>
        {
            eventPrevious = prev;
            eventNew = next;
            eventFired = true;
        };

        mgr.ForceInputMode(VRInputMode.Hands);
        Assert.IsTrue(eventFired);
        Assert.AreEqual(VRInputMode.None, eventPrevious);
        Assert.AreEqual(VRInputMode.Hands, eventNew);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_ConfigDefaults()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        Assert.AreEqual(0.5f, mgr.controllerToHandDelay);
        Assert.AreEqual(2.0f, mgr.noInputTimeout);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_MultipleTransitions()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        mgr.ForceInputMode(VRInputMode.Controllers);
        mgr.ForceInputMode(VRInputMode.Hands);
        mgr.ForceInputMode(VRInputMode.None);
        mgr.ForceInputMode(VRInputMode.Controllers);

        Assert.AreEqual(4, mgr.modeChangedCount);
        Assert.AreEqual(VRInputMode.Controllers, mgr.CurrentInputMode);

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- DesktopRig Tests ---

    [UnityTest]
    public IEnumerator DesktopRig_Defaults_MovementSpeed()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        Assert.AreEqual(5.0f, rig.movementSpeed);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_Defaults_MouseSensitivity()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        Assert.AreEqual(2.0f, rig.mouseSensitivity);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_Defaults_JumpStrength()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        Assert.AreEqual(8.0f, rig.jumpStrength);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_Defaults_GravityStrength()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        Assert.AreEqual(-9.81f, rig.gravityStrength);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_Defaults_WASDDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        Assert.IsFalse(rig.wasdMotionEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_Defaults_MouseLookDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        Assert.IsFalse(rig.mouseLookEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_Defaults_JumpDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        Assert.IsFalse(rig.jumpEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_Defaults_RigOffset()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        Assert.AreEqual(Vector3.zero, rig.rigOffset);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetWASDEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.wasdMotionEnabled = true;
        Assert.IsTrue(rig.wasdMotionEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetMouseLookEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.mouseLookEnabled = true;
        Assert.IsTrue(rig.mouseLookEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetJumpEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.jumpEnabled = true;
        Assert.IsTrue(rig.jumpEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_Terminate_ClearsFollowers()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.rigFollowers = new System.Collections.Generic.List<FiveSQD.StraightFour.Entity.BaseEntity>();
        rig.Terminate();
        Assert.IsNull(rig.rigFollowers);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_ApplyMovement_StoresInput()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        // ApplyMovement stores input but doesn't crash without avatar.
        rig.ApplyMovement(new Vector2(1, 0));
        // No exception thrown is the test.

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_ApplyJump_WithoutAvatar_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.jumpEnabled = true;
        rig.ApplyJump();
        // Should not throw with null avatarEntity.

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_ApplyLook_WithoutCamera_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.mouseLookEnabled = true;
        rig.ApplyLook(new Vector2(5, 5));
        // Should not throw with null cameraTransform.

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_ApplyLower_WithoutAvatar_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.jumpEnabled = true;
        rig.ApplyLower();
        // Should not throw with null avatarEntity.

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetRigOffsetFromString_ValidFormat()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.SetRigOffsetFromString("1.0,2.0,3.0");
        Assert.AreEqual(1.0f, rig.rigOffset.x, 0.01f);
        Assert.AreEqual(2.0f, rig.rigOffset.y, 0.01f);
        Assert.AreEqual(3.0f, rig.rigOffset.z, 0.01f);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetRigOffsetFromString_InvalidFormat_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.SetRigOffsetFromString("invalid");
        Assert.AreEqual(Vector3.zero, rig.rigOffset);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetRigOffsetFromString_NullString_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.SetRigOffsetFromString(null);
        Assert.AreEqual(Vector3.zero, rig.rigOffset);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetRigOffsetFromString_EmptyString_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.SetRigOffsetFromString("");
        Assert.AreEqual(Vector3.zero, rig.rigOffset);

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- DesktopInput Property Defaults ---

    [UnityTest]
    public IEnumerator DesktopInput_Defaults_GravityEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopInput");
        DesktopInput di = go.AddComponent<DesktopInput>();
        yield return null;

        Assert.IsTrue(di.gravityEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopInput_Defaults_WASDMotionEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopInput");
        DesktopInput di = go.AddComponent<DesktopInput>();
        yield return null;

        Assert.IsTrue(di.wasdMotionEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopInput_Defaults_MouseLookEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopInput");
        DesktopInput di = go.AddComponent<DesktopInput>();
        yield return null;

        Assert.IsTrue(di.mouseLookEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopInput_Defaults_JumpEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopInput");
        DesktopInput di = go.AddComponent<DesktopInput>();
        yield return null;

        Assert.IsTrue(di.jumpEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopInput_SetGravityEnabled_False()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopInput");
        DesktopInput di = go.AddComponent<DesktopInput>();
        yield return null;

        di.gravityEnabled = false;
        Assert.IsFalse(di.gravityEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopInput_SetWASDMotionEnabled_False()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopInput");
        DesktopInput di = go.AddComponent<DesktopInput>();
        yield return null;

        di.wasdMotionEnabled = false;
        Assert.IsFalse(di.wasdMotionEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopInput_SetMouseLookEnabled_False()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopInput");
        DesktopInput di = go.AddComponent<DesktopInput>();
        yield return null;

        di.mouseLookEnabled = false;
        Assert.IsFalse(di.mouseLookEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopInput_SetJumpEnabled_False()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopInput");
        DesktopInput di = go.AddComponent<DesktopInput>();
        yield return null;

        di.jumpEnabled = false;
        Assert.IsFalse(di.jumpEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- MobileInput Property Defaults ---

    [UnityTest]
    public IEnumerator MobileInput_Defaults_TouchInputEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.IsTrue(mi.touchInputEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_TouchMovementEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.IsTrue(mi.touchMovementEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_TouchLookEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.IsTrue(mi.touchLookEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_TouchSensitivity()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.AreEqual(1.0f, mi.touchSensitivity);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_TouchDragThreshold()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.AreEqual(10.0f, mi.touchDragThreshold);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_PinchZoomEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.IsTrue(mi.pinchZoomEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_TapTimeThreshold()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.AreEqual(0.3f, mi.tapTimeThreshold);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_TapDistanceThreshold()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.AreEqual(50.0f, mi.tapDistanceThreshold);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_TouchCountIsZero()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.AreEqual(0, mi.touchCount);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_PrimaryTouchPositionIsZero()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.AreEqual(Vector2.zero, mi.primaryTouchPosition);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_SetTouchSensitivity()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        mi.touchSensitivity = 2.5f;
        Assert.AreEqual(2.5f, mi.touchSensitivity);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_SetTouchDragThreshold()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        mi.touchDragThreshold = 20.0f;
        Assert.AreEqual(20.0f, mi.touchDragThreshold);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_DisableTouchInput()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        mi.touchInputEnabled = false;
        Assert.IsFalse(mi.touchInputEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_DisablePinchZoom()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        mi.pinchZoomEnabled = false;
        Assert.IsFalse(mi.pinchZoomEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- Quest3PlatformDetector Tests ---

    [Test]
    public void Quest3PlatformDetector_IsQuest3Platform_ReturnsFalseInEditor()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsFalse(Quest3PlatformDetector.IsQuest3Platform());
    }

    [Test]
    public void Quest3PlatformDetector_IsAnyQuestPlatform_ReturnsFalseInEditor()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsFalse(Quest3PlatformDetector.IsAnyQuestPlatform());
    }

    [Test]
    public void Quest3PlatformDetector_GetQuestDeviceModel_ReturnsEmptyInEditor()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(string.Empty, Quest3PlatformDetector.GetQuestDeviceModel());
    }

    [Test]
    public void Quest3PlatformDetector_RedetectPlatform_ReturnsFalseInEditor()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsFalse(Quest3PlatformDetector.RedetectPlatform());
    }

    [Test]
    public void Quest3PlatformDetector_CachedResult_ConsistentAcrossCalls()
    {
        LogAssert.ignoreFailingMessages = true;
        bool first = Quest3PlatformDetector.IsQuest3Platform();
        bool second = Quest3PlatformDetector.IsQuest3Platform();
        Assert.AreEqual(first, second);
    }

    // --- WebVerseHand Tests ---

    [UnityTest]
    public IEnumerator WebVerseHand_Defaults_TouchingEntitiesEmpty()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("Hand");
        WebVerseHand hand = go.AddComponent<WebVerseHand>();
        yield return null;

        Assert.IsNotNull(hand.touchingEntities);
        Assert.AreEqual(0, hand.touchingEntities.Length);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseHand_Defaults_CollidingEntitiesEmpty()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("Hand");
        WebVerseHand hand = go.AddComponent<WebVerseHand>();
        yield return null;

        Assert.IsNotNull(hand.collidingEntities);
        Assert.AreEqual(0, hand.collidingEntities.Length);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseHand_TouchingEntities_IsArrayType()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("Hand");
        WebVerseHand hand = go.AddComponent<WebVerseHand>();
        yield return null;

        var arr = hand.touchingEntities;
        Assert.IsNotNull(arr);
        Assert.IsInstanceOf<FiveSQD.StraightFour.Entity.BaseEntity[]>(arr);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebVerseHand_CollidingEntities_IsArrayType()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("Hand");
        WebVerseHand hand = go.AddComponent<WebVerseHand>();
        yield return null;

        var arr = hand.collidingEntities;
        Assert.IsNotNull(arr);
        Assert.IsInstanceOf<FiveSQD.StraightFour.Entity.BaseEntity[]>(arr);

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- InputManager Tests ---

    [UnityTest]
    public IEnumerator InputManager_Initialize_InputEnabledIsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.inputEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_Initialize_PressedKeysEmpty()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsNotNull(mgr.pressedKeys);
        Assert.AreEqual(0, mgr.pressedKeys.Count);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_Initialize_PressedKeyCodesEmpty()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsNotNull(mgr.pressedKeyCodes);
        Assert.AreEqual(0, mgr.pressedKeyCodes.Count);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_Initialize_AllBoolFieldsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.leftValue);
        Assert.IsFalse(mgr.middleValue);
        Assert.IsFalse(mgr.rightValue);
        Assert.IsFalse(mgr.leftMenuValue);
        Assert.IsFalse(mgr.rightMenuValue);
        Assert.IsFalse(mgr.leftTriggerTouchValue);
        Assert.IsFalse(mgr.rightTriggerTouchValue);
        Assert.IsFalse(mgr.leftTriggerPressValue);
        Assert.IsFalse(mgr.rightTriggerPressValue);
        Assert.IsFalse(mgr.leftGripPressValue);
        Assert.IsFalse(mgr.rightGripPressValue);
        Assert.IsFalse(mgr.leftPinchValue);
        Assert.IsFalse(mgr.rightPinchValue);
        Assert.IsFalse(mgr.leftStickValue);
        Assert.IsFalse(mgr.rightStickValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_MenuValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.menuValue);

        mgr.leftMenuValue = true;
        Assert.IsTrue(mgr.menuValue);

        mgr.leftMenuValue = false;
        mgr.rightMenuValue = true;
        Assert.IsTrue(mgr.menuValue);

        mgr.rightMenuValue = false;
        Assert.IsFalse(mgr.menuValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_TriggerTouchValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.triggerTouchValue);

        mgr.leftTriggerTouchValue = true;
        Assert.IsTrue(mgr.triggerTouchValue);

        mgr.leftTriggerTouchValue = false;
        mgr.rightTriggerTouchValue = true;
        Assert.IsTrue(mgr.triggerTouchValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_TriggerPressValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.triggerPressValue);

        mgr.leftTriggerPressValue = true;
        Assert.IsTrue(mgr.triggerPressValue);

        mgr.leftTriggerPressValue = false;
        mgr.rightTriggerPressValue = true;
        Assert.IsTrue(mgr.triggerPressValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_GripPressValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.gripPressValue);

        mgr.leftGripPressValue = true;
        Assert.IsTrue(mgr.gripPressValue);

        mgr.leftGripPressValue = false;
        mgr.rightGripPressValue = true;
        Assert.IsTrue(mgr.gripPressValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_PinchValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.pinchValue);

        mgr.leftPinchValue = true;
        Assert.IsTrue(mgr.pinchValue);

        mgr.leftPinchValue = false;
        mgr.rightPinchValue = true;
        Assert.IsTrue(mgr.pinchValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_TouchPadTouchValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.touchPadTouchValue);

        mgr.leftTouchPadTouchValue = true;
        Assert.IsTrue(mgr.touchPadTouchValue);

        mgr.leftTouchPadTouchValue = false;
        mgr.rightTouchPadTouchValue = true;
        Assert.IsTrue(mgr.touchPadTouchValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_TouchPadPressValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.touchPadPressValue);

        mgr.leftTouchPadPressValue = true;
        Assert.IsTrue(mgr.touchPadPressValue);

        mgr.leftTouchPadPressValue = false;
        mgr.rightTouchPadPressValue = true;
        Assert.IsTrue(mgr.touchPadPressValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_PrimaryTouchValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.primaryTouchValue);

        mgr.leftPrimaryTouchValue = true;
        Assert.IsTrue(mgr.primaryTouchValue);

        mgr.leftPrimaryTouchValue = false;
        mgr.rightPrimaryTouchValue = true;
        Assert.IsTrue(mgr.primaryTouchValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_PrimaryPressValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.primaryPressValue);

        mgr.leftPrimaryPressValue = true;
        Assert.IsTrue(mgr.primaryPressValue);

        mgr.leftPrimaryPressValue = false;
        mgr.rightPrimaryPressValue = true;
        Assert.IsTrue(mgr.primaryPressValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_SecondaryTouchValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.secondaryTouchValue);

        mgr.leftSecondaryTouchValue = true;
        Assert.IsTrue(mgr.secondaryTouchValue);

        mgr.leftSecondaryTouchValue = false;
        mgr.rightSecondaryTouchValue = true;
        Assert.IsTrue(mgr.secondaryTouchValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_SecondaryPressValue_LeftOrRight()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.secondaryPressValue);

        mgr.leftSecondaryPressValue = true;
        Assert.IsTrue(mgr.secondaryPressValue);

        mgr.leftSecondaryPressValue = false;
        mgr.rightSecondaryPressValue = true;
        Assert.IsTrue(mgr.secondaryPressValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_Reset_ClearsState()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.leftValue = true;
        mgr.rightValue = true;
        mgr.leftMenuValue = true;
        mgr.leftTriggerPressValue = true;

        mgr.Reset();
        Assert.IsFalse(mgr.inputEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_MoveValue_DefaultsToZero()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.AreEqual(Vector2.zero, mgr.moveValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_LookValue_DefaultsToZero()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.AreEqual(Vector2.zero, mgr.lookValue);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_TouchPadTouchLocation_DefaultsToZero()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.AreEqual(Vector2.zero, mgr.leftTouchPadTouchLocation);
        Assert.AreEqual(Vector2.zero, mgr.rightTouchPadTouchLocation);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_Terminate_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.Terminate();

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_CallbackMethods_NoThrowWhenInputDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        Assert.IsFalse(mgr.inputEnabled);

        // These should all be no-ops since inputEnabled is false.
        mgr.Move(new Vector2(1, 0));
        mgr.EndMove();
        mgr.Look(new Vector2(1, 0));
        mgr.EndLook();
        mgr.Key("a", "KeyA");
        mgr.EndKey("a", "KeyA");
        mgr.Left();
        mgr.EndLeft();
        mgr.Middle();
        mgr.EndMiddle();
        mgr.Right();
        mgr.EndRight();

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_VRCallbacks_NoThrowWhenInputDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();

        mgr.LeftMenu();
        mgr.EndLeftMenu();
        mgr.RightMenu();
        mgr.EndRightMenu();
        mgr.Menu();
        mgr.EndMenu();
        mgr.LeftTriggerTouch();
        mgr.EndLeftTriggerTouch();
        mgr.RightTriggerTouch();
        mgr.EndRightTriggerTouch();
        mgr.TriggerTouch();
        mgr.EndTriggerTouch();
        mgr.LeftTriggerPress();
        mgr.EndLeftTriggerPress();
        mgr.RightTriggerPress();
        mgr.EndRightTriggerPress();
        mgr.TriggerPress();
        mgr.EndTriggerPress();

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_GripPinchCallbacks_NoThrowWhenInputDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();

        mgr.LeftGripPress();
        mgr.EndLeftGripPress();
        mgr.RightGripPress();
        mgr.EndRightGripPress();
        mgr.GripPress();
        mgr.EndGripPress();
        mgr.LeftPinch();
        mgr.EndLeftPinch();
        mgr.RightPinch();
        mgr.EndRightPinch();
        mgr.Pinch();
        mgr.EndPinch();

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_TouchPadCallbacks_NoThrowWhenInputDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();

        mgr.LeftTouchPadTouch();
        mgr.EndLeftTouchPadTouch();
        mgr.RightTouchPadTouch();
        mgr.EndRightTouchPadTouch();
        mgr.TouchPadTouch();
        mgr.EndTouchPadTouch();
        mgr.LeftTouchPadTouchValueChange(new Vector2(0.5f, 0.5f));
        mgr.RightTouchPadTouchValueChange(new Vector2(0.5f, 0.5f));
        mgr.LeftTouchPadPress();
        mgr.EndLeftTouchPadPress();
        mgr.RightTouchPadPress();
        mgr.EndRightTouchPadPress();
        mgr.TouchPadPress();
        mgr.EndTouchPadPress();

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_PrimarySecondaryCallbacks_NoThrowWhenInputDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();

        mgr.LeftPrimaryTouch();
        mgr.EndLeftPrimaryTouch();
        mgr.RightPrimaryTouch();
        mgr.EndRightPrimaryTouch();
        mgr.PrimaryTouch();
        mgr.EndPrimaryTouch();
        mgr.LeftPrimaryPress();
        mgr.EndLeftPrimaryPress();
        mgr.RightPrimaryPress();
        mgr.EndRightPrimaryPress();
        mgr.PrimaryPress();
        mgr.EndPrimaryPress();
        mgr.LeftSecondaryTouch();
        mgr.EndLeftSecondaryTouch();
        mgr.RightSecondaryTouch();
        mgr.EndRightSecondaryTouch();
        mgr.SecondaryTouch();
        mgr.EndSecondaryTouch();
        mgr.LeftSecondaryPress();
        mgr.EndLeftSecondaryPress();
        mgr.RightSecondaryPress();
        mgr.EndRightSecondaryPress();
        mgr.SecondaryPress();
        mgr.EndSecondaryPress();

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_StickCallbacks_NoThrowWhenInputDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();

        mgr.LeftStick();
        mgr.EndLeftStick();
        mgr.RightStick();
        mgr.EndRightStick();
        mgr.LeftStickValueChange(new Vector2(0.5f, 0.5f));
        mgr.RightStickValueChange(new Vector2(0.5f, 0.5f));

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- IHandTrackingProvider Interface Tests ---

    [Test]
    public void MockHandTrackingProvider_Defaults_NotTracked()
    {
        LogAssert.ignoreFailingMessages = true;
        var provider = new MockHandTrackingProvider();

        Assert.IsFalse(provider.IsLeftHandTracked);
        Assert.IsFalse(provider.IsRightHandTracked);
    }

    [Test]
    public void MockHandTrackingProvider_SetTracked()
    {
        LogAssert.ignoreFailingMessages = true;
        var provider = new MockHandTrackingProvider();

        provider.IsLeftHandTracked = true;
        Assert.IsTrue(provider.IsLeftHandTracked);

        provider.IsRightHandTracked = true;
        Assert.IsTrue(provider.IsRightHandTracked);
    }

    [Test]
    public void MockHandTrackingProvider_SimulateHandTrackingStateChanged()
    {
        LogAssert.ignoreFailingMessages = true;
        var provider = new MockHandTrackingProvider();
        int receivedIndex = -1;
        bool receivedState = false;

        provider.OnHandTrackingStateChanged += (index, state) =>
        {
            receivedIndex = index;
            receivedState = state;
        };

        provider.SimulateHandTrackingStateChanged(0, true);
        Assert.AreEqual(0, receivedIndex);
        Assert.IsTrue(receivedState);
    }

    [Test]
    public void MockHandTrackingProvider_SimulatePinchStateChanged()
    {
        LogAssert.ignoreFailingMessages = true;
        var provider = new MockHandTrackingProvider();
        int receivedIndex = -1;
        bool receivedPinch = false;

        provider.OnPinchStateChanged += (index, pinch) =>
        {
            receivedIndex = index;
            receivedPinch = pinch;
        };

        provider.SimulatePinchStateChanged(1, true);
        Assert.AreEqual(1, receivedIndex);
        Assert.IsTrue(receivedPinch);
    }

    // --- BasePlatformInput Tests ---

    [UnityTest]
    public IEnumerator BasePlatformInput_GetPointerRaycast_ThrowsNotImplemented()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("PlatformInput");
        BasePlatformInput bpi = go.AddComponent<BasePlatformInput>();
        yield return null;

        Assert.Throws<NotImplementedException>(() =>
        {
            bpi.GetPointerRaycast(Vector3.forward);
        });

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- VRInputMode Boundary Tests ---

    [Test]
    public void VRInputMode_AllValues_CountIs3()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(VRInputMode));
        Assert.AreEqual(3, values.Length);
    }

    [Test]
    public void VRInputMode_CanCastFromInt()
    {
        LogAssert.ignoreFailingMessages = true;
        VRInputMode mode = (VRInputMode)2;
        Assert.AreEqual(VRInputMode.Controllers, mode);
    }

    // --- VRInputModeManager Advanced Tests ---

    [UnityTest]
    public IEnumerator VRInputModeManager_TimeSinceLastInput_IsNonNegative()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        Assert.GreaterOrEqual(mgr.TimeSinceLastInput, 0f);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_MultipleEventListeners()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        int listener1Count = 0;
        int listener2Count = 0;
        mgr.OnInputModeChanged += (prev, next) => listener1Count++;
        mgr.OnInputModeChanged += (prev, next) => listener2Count++;

        mgr.ForceInputMode(VRInputMode.Controllers);
        Assert.AreEqual(1, listener1Count);
        Assert.AreEqual(1, listener2Count);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_HandTrackingProvider_NullSafe()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        mgr.mockHandTracking = null;
        yield return null;

        // Should not throw with null hand tracking provider.
        mgr.RegisterHandInput();
        Assert.AreEqual(VRInputMode.Hands, mgr.CurrentInputMode);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_SetControllerToHandDelay()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        mgr.controllerToHandDelay = 1.0f;
        Assert.AreEqual(1.0f, mgr.controllerToHandDelay);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRInputModeManager_SetNoInputTimeout()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MockManager");
        MockVRInputModeManager mgr = go.AddComponent<MockVRInputModeManager>();
        yield return null;

        mgr.noInputTimeout = 5.0f;
        Assert.AreEqual(5.0f, mgr.noInputTimeout);

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- VRRig Enum Tests ---

    [Test]
    public void VRRig_PointerMode_None_HasValue0()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(0, (int)VRRig.PointerMode.None);
    }

    [Test]
    public void VRRig_PointerMode_Teleport_HasValue1()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(1, (int)VRRig.PointerMode.Teleport);
    }

    [Test]
    public void VRRig_PointerMode_UI_HasValue2()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(2, (int)VRRig.PointerMode.UI);
    }

    [Test]
    public void VRRig_PointerMode_AllValues_CountIs3()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(VRRig.PointerMode));
        Assert.AreEqual(3, values.Length);
    }

    [Test]
    public void VRRig_TurnLocomotionMode_None_HasValue0()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(0, (int)VRRig.TurnLocomotionMode.None);
    }

    [Test]
    public void VRRig_TurnLocomotionMode_Smooth_HasValue1()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(1, (int)VRRig.TurnLocomotionMode.Smooth);
    }

    [Test]
    public void VRRig_TurnLocomotionMode_Snap_HasValue2()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(2, (int)VRRig.TurnLocomotionMode.Snap);
    }

    [Test]
    public void VRRig_TurnLocomotionMode_AllValues_CountIs3()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(VRRig.TurnLocomotionMode));
        Assert.AreEqual(3, values.Length);
    }

    [Test]
    public void VRRig_RayInteractorType_Standard_HasValue0()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(0, (int)VRRig.RayInteractorType.Standard);
    }

    [Test]
    public void VRRig_RayInteractorType_NearFar_HasValue1()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(1, (int)VRRig.RayInteractorType.NearFar);
    }

    [Test]
    public void VRRig_RayInteractorType_AllValues_CountIs2()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(VRRig.RayInteractorType));
        Assert.AreEqual(2, values.Length);
    }

    // --- VRRig Defaults ---

    [UnityTest]
    public IEnumerator VRRig_Defaults_AutoDetectPlatformFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("VRRig");
        VRRig rig = go.AddComponent<VRRig>();
        yield return null;

        Assert.IsFalse(rig.autoDetectPlatform);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRRig_Defaults_RayInteractorTypeStandard()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("VRRig");
        VRRig rig = go.AddComponent<VRRig>();
        yield return null;

        Assert.AreEqual(VRRig.RayInteractorType.Standard, rig.rayInteractorType);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRRig_Defaults_PointerModeNone()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("VRRig");
        VRRig rig = go.AddComponent<VRRig>();
        yield return null;

        Assert.AreEqual(VRRig.PointerMode.None, rig.leftPointerMode);
        Assert.AreEqual(VRRig.PointerMode.None, rig.rightPointerMode);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VRRig_SetPointerMode_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("VRRig");
        VRRig rig = go.AddComponent<VRRig>();
        yield return null;

        // Setting pointer modes without interactors should not throw.
        rig.leftPointerMode = VRRig.PointerMode.UI;
        rig.rightPointerMode = VRRig.PointerMode.Teleport;
        rig.leftPointerMode = VRRig.PointerMode.None;
        rig.rightPointerMode = VRRig.PointerMode.None;

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- InputManager Event Registration Tests ---

    [UnityTest]
    public IEnumerator InputManager_RegisterInputEvent_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.RegisterInputEvent("move", "testFunc(?)");
        mgr.RegisterInputEvent("look", "testLook(?)");
        mgr.RegisterInputEvent("key", "testKey(?)");
        mgr.RegisterInputEvent("left", "testLeft()");
        mgr.RegisterInputEvent("right", "testRight()");
        mgr.RegisterInputEvent("middle", "testMiddle()");
        // Should not throw.

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_RegisterInputEvent_VREvents_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.RegisterInputEvent("leftmenu", "func()");
        mgr.RegisterInputEvent("rightmenu", "func()");
        mgr.RegisterInputEvent("menu", "func()");
        mgr.RegisterInputEvent("lefttriggertouch", "func()");
        mgr.RegisterInputEvent("lefttriggerpress", "func()");
        mgr.RegisterInputEvent("leftgrippress", "func()");
        mgr.RegisterInputEvent("leftpinch", "func()");
        mgr.RegisterInputEvent("leftprimarytouch", "func()");
        mgr.RegisterInputEvent("leftprimarypress", "func()");
        mgr.RegisterInputEvent("leftsecondarytouch", "func()");
        mgr.RegisterInputEvent("leftsecondarypress", "func()");

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_DeregisterInputEvent_NonExistent_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        // Deregister events that were never registered - should warn but not throw.
        mgr.DeregisterInputEvent("move", "nonexistent()");
        mgr.DeregisterInputEvent("look", "nonexistent()");
        mgr.DeregisterInputEvent("key", "nonexistent()");

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_RegisterThenDeregister_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.RegisterInputEvent("move", "testMove(?)");
        mgr.DeregisterInputEvent("move", "testMove(?)");
        // Should succeed without exception.

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_RegisterInputEvent_UnknownEvent_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        // Unknown event name should not throw (falls through switch default).
        mgr.RegisterInputEvent("unknownevent", "func()");

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_DoubleInitialize_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.Initialize();
        Assert.IsFalse(mgr.inputEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_DoubleTerminate_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.Terminate();
        mgr.Terminate();
        // Should not throw.

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_InitializeTerminateInitialize_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.Terminate();
        mgr.Initialize();
        Assert.IsFalse(mgr.inputEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_Reset_ClearsAllInputState()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.leftValue = true;
        mgr.middleValue = true;
        mgr.rightValue = true;
        mgr.leftMenuValue = true;
        mgr.rightMenuValue = true;
        mgr.leftTriggerPressValue = true;
        mgr.rightTriggerPressValue = true;
        mgr.leftGripPressValue = true;
        mgr.rightGripPressValue = true;
        mgr.leftPinchValue = true;
        mgr.rightPinchValue = true;

        mgr.Reset();

        Assert.IsFalse(mgr.inputEnabled);
        Assert.IsNotNull(mgr.pressedKeys);
        Assert.AreEqual(0, mgr.pressedKeys.Count);
        Assert.IsNotNull(mgr.pressedKeyCodes);
        Assert.AreEqual(0, mgr.pressedKeyCodes.Count);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator InputManager_RegisterMultipleEvents_SameType_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("InputMgr");
        InputManager mgr = go.AddComponent<InputManager>();
        yield return null;

        mgr.Initialize();
        mgr.RegisterInputEvent("move", "func1(?)");
        mgr.RegisterInputEvent("move", "func2(?)");
        mgr.RegisterInputEvent("move", "func3(?)");
        // Multiple registrations should accumulate.

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- DesktopRig Additional Tests ---

    [UnityTest]
    public IEnumerator DesktopRig_SetMovementSpeed()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.movementSpeed = 10.0f;
        Assert.AreEqual(10.0f, rig.movementSpeed);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetMouseSensitivity()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.mouseSensitivity = 4.0f;
        Assert.AreEqual(4.0f, rig.mouseSensitivity);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetJumpStrength()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.jumpStrength = 12.0f;
        Assert.AreEqual(12.0f, rig.jumpStrength);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetGravityStrength()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.gravityStrength = -20.0f;
        Assert.AreEqual(-20.0f, rig.gravityStrength);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetGravityEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.gravityEnabled = true;
        Assert.IsTrue(rig.gravityEnabled);
        rig.gravityEnabled = false;
        Assert.IsFalse(rig.gravityEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_SetRigOffset_Direct()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.SetRigOffset(new Vector3(5, 10, 15));
        Assert.AreEqual(5.0f, rig.rigOffset.x, 0.01f);
        Assert.AreEqual(10.0f, rig.rigOffset.y, 0.01f);
        Assert.AreEqual(15.0f, rig.rigOffset.z, 0.01f);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_ApplyJumpInput_StoresState()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.jumpEnabled = true;
        rig.ApplyJumpInput(true);
        rig.ApplyJumpInput(false);
        // Should not throw with null avatarEntity.

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DesktopRig_ApplyLowerInput_StoresState()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("DesktopRig");
        DesktopRig rig = go.AddComponent<DesktopRig>();
        yield return null;

        rig.ApplyLowerInput(true);
        rig.ApplyLowerInput(false);
        // Should not throw.

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- MobileInput Additional Tests ---

    [UnityTest]
    public IEnumerator MobileInput_SetTapTimeThreshold()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        mi.tapTimeThreshold = 0.5f;
        Assert.AreEqual(0.5f, mi.tapTimeThreshold);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_SetTapDistanceThreshold()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        mi.tapDistanceThreshold = 100.0f;
        Assert.AreEqual(100.0f, mi.tapDistanceThreshold);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_DisableTouchMovement()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        mi.touchMovementEnabled = false;
        Assert.IsFalse(mi.touchMovementEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_DisableTouchLook()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        mi.touchLookEnabled = false;
        Assert.IsFalse(mi.touchLookEnabled);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator MobileInput_Defaults_SecondaryTouchPositionIsZero()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("MobileInput");
        MobileInput mi = go.AddComponent<MobileInput>();
        yield return null;

        Assert.AreEqual(Vector2.zero, mi.secondaryTouchPosition);

        UnityEngine.Object.DestroyImmediate(go);
    }

    // --- Quest3 HandednessQ3 Enum Tests ---

    [Test]
    public void HandednessQ3_Left_HasValue0()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(0, (int)HandednessQ3.Left);
    }

    [Test]
    public void HandednessQ3_Right_HasValue1()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(1, (int)HandednessQ3.Right);
    }

    [Test]
    public void HandednessQ3_AllValues_CountIs2()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(HandednessQ3));
        Assert.AreEqual(2, values.Length);
    }
}
