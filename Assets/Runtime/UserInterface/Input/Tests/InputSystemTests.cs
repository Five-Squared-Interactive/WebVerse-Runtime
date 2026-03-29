// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Desktop;

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
}
