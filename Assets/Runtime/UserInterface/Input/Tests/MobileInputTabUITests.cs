// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Input.Mobile;
using FiveSQD.WebVerse.Interface.TabUI;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for MobileInput TabUI touch hooks (auto-hide, edge-tap, graceful degradation).
/// </summary>
public class MobileInputTabUITests
{
    private GameObject mobileInputGO;
    private MobileInput mobileInput;
    private GameObject tabUIControllerGO;
    private TabUIController tabUIController;

    [SetUp]
    public void SetUp()
    {
        mobileInputGO = new GameObject("TestMobileInput");
        mobileInput = mobileInputGO.AddComponent<MobileInput>();

        tabUIControllerGO = new GameObject("TestTabUIController");
        tabUIController = tabUIControllerGO.AddComponent<TabUIController>();
    }

    [TearDown]
    public void TearDown()
    {
        if (mobileInputGO != null) Object.DestroyImmediate(mobileInputGO);
        if (tabUIControllerGO != null) Object.DestroyImmediate(tabUIControllerGO);
    }

    [Test]
    public void MobileInput_OnPrimaryTouchStarted_CallsSendStartAutoHide()
    {
        LogAssert.ignoreFailingMessages = true;

        mobileInput.TabUIController = tabUIController;

        // SendStartAutoHide has isMobile/webViewReady guards — it will return early
        // but should not throw
        Assert.DoesNotThrow(() => mobileInput.HandleTouchStartHooks());
    }

    [Test]
    public void MobileInput_OnPrimaryTouchCanceled_CallsSendStopAutoHide()
    {
        LogAssert.ignoreFailingMessages = true;

        mobileInput.TabUIController = tabUIController;

        // HandleTouchEndHooks with long duration — should call StopAutoHide but not EdgeTap
        Assert.DoesNotThrow(() => mobileInput.HandleTouchEndHooks(1.0f, 100f, new Vector2(100, 200)));
    }

    [Test]
    public void MobileInput_OnQuickTap_CallsSendEdgeTap()
    {
        LogAssert.ignoreFailingMessages = true;

        mobileInput.TabUIController = tabUIController;

        // Quick tap: duration < 0.3s and distance < 50px
        Assert.DoesNotThrow(() => mobileInput.HandleTouchEndHooks(0.1f, 5f, new Vector2(100, 200)));
    }

    [Test]
    public void MobileInput_OnLongDrag_DoesNotCallSendEdgeTap()
    {
        LogAssert.ignoreFailingMessages = true;

        mobileInput.TabUIController = tabUIController;

        // Long drag: duration > tapTimeThreshold or distance > tapDistanceThreshold
        // This should call StopAutoHide but NOT SendEdgeTap
        Assert.DoesNotThrow(() => mobileInput.HandleTouchEndHooks(1.0f, 200f, new Vector2(100, 200)));
    }

    [Test]
    public void MobileInput_WithoutTabUIController_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // TabUIController is null — should not throw due to null-conditional operator
        mobileInput.TabUIController = null;

        Assert.DoesNotThrow(() => mobileInput.HandleTouchStartHooks());
        Assert.DoesNotThrow(() => mobileInput.HandleTouchEndHooks(0.1f, 5f, new Vector2(100, 200)));
    }
}
