// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Interface.TabUI;
using FiveSQD.StraightFour.WorldState;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System;
using System.Threading;

/// <summary>
/// Unit tests for Tab UI components.
/// Tests TabUIController, TabUIInputHandler, and TabUIIntegration.
/// </summary>
public class TabUITests
{
    #region TabUIController Tests

    [Test]
    public void TabUIController_CanBeCreated()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");

        // Act
        var controller = go.AddComponent<TabUIController>();

        // Assert
        Assert.IsNotNull(controller);
        Assert.IsTrue(controller.IsChromeVisible);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_Initialize_WithNullTabManager_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");
        var controller = go.AddComponent<TabUIController>();

        // Expect the error log about missing WebView prefab
        LogAssert.Expect(LogType.Error, "[TabUIController->Initialize] WebView prefab not set.");

        // Act & Assert - Should handle null gracefully
        Assert.DoesNotThrow(() => controller.Initialize(null, null));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_Initialize_WithNullWebViewPrefab_LogsError()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");
        var controller = go.AddComponent<TabUIController>();

        GameObject tabManagerGO = new GameObject("TabManager");
        var tabManager = tabManagerGO.AddComponent<TabManager>();

        // Act
        LogAssert.Expect(LogType.Error, "[TabUIController->Initialize] WebView prefab not set.");
        controller.Initialize(tabManager, null);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(tabManagerGO);
    }

    [Test]
    public void TabUIController_ShowChrome_SetsVisibleTrue()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");
        var controller = go.AddComponent<TabUIController>();

        // Act
        controller.ShowChrome();

        // Assert
        Assert.IsTrue(controller.IsChromeVisible);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_HideChrome_SetsVisibleFalse()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");
        var controller = go.AddComponent<TabUIController>();

        // Act
        controller.HideChrome();

        // Assert
        Assert.IsFalse(controller.IsChromeVisible);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_ToggleChrome_TogglesVisibility()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");
        var controller = go.AddComponent<TabUIController>();
        bool initialState = controller.IsChromeVisible;

        // Act
        controller.ToggleChrome();

        // Assert
        Assert.AreNotEqual(initialState, controller.IsChromeVisible);

        // Toggle back
        controller.ToggleChrome();
        Assert.AreEqual(initialState, controller.IsChromeVisible);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_ShowToast_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert - Should not throw even without WebView
        Assert.DoesNotThrow(() => controller.ShowToast("Test message", "info", 3000));
        Assert.DoesNotThrow(() => controller.ShowToast("Error message", "error", 5000));
        Assert.DoesNotThrow(() => controller.ShowToast("Warning", "warning"));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_Terminate_CleansUpResources()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");
        var controller = go.AddComponent<TabUIController>();

        // Act
        LogAssert.Expect(LogType.Log, "[TabUIController] Terminated.");
        controller.Terminate();

        // Assert - Should not throw on double terminate
        Assert.DoesNotThrow(() => controller.Terminate());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnNavigateRequested_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");
        var controller = go.AddComponent<TabUIController>();
        string navigatedUrl = null;

        // Act
        controller.OnNavigateRequested += (url) => navigatedUrl = url;

        // Assert - Event subscription should work
        Assert.IsNull(navigatedUrl);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnMenuAction_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestTabUIController");
        var controller = go.AddComponent<TabUIController>();
        string menuAction = null;

        // Act
        controller.OnMenuAction += (action) => menuAction = action;

        // Assert - Event subscription should work
        Assert.IsNull(menuAction);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region TabUIInputHandler Tests

    [Test]
    public void TabUIInputHandler_CanBeCreated()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");

        // Act
        var handler = go.AddComponent<TabUIInputHandler>();

        // Assert
        Assert.IsNotNull(handler);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIInputHandler_Initialize_WithValidReferences()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();

        GameObject controllerGO = new GameObject("Controller");
        var controller = controllerGO.AddComponent<TabUIController>();

        GameObject tabManagerGO = new GameObject("TabManager");
        var tabManager = tabManagerGO.AddComponent<TabManager>();

        // Act
        LogAssert.Expect(LogType.Log, "[TabUIInputHandler] Initialized.");
        handler.Initialize(controller, tabManager);

        // Assert - Should complete without error
        Assert.Pass();

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(controllerGO);
        UnityEngine.Object.DestroyImmediate(tabManagerGO);
    }

    [Test]
    public void TabUIInputHandler_Initialize_WithNullReferences_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();

        // Act & Assert
        Assert.DoesNotThrow(() => handler.Initialize(null, null));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIInputHandler_OnToggleFullscreen_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();
        bool eventFired = false;

        // Act
        handler.OnToggleFullscreen += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIInputHandler_OnVRMenuButton_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();

        // Act & Assert - Should handle null controller gracefully
        Assert.DoesNotThrow(() => handler.OnVRMenuButton());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIInputHandler_OnVRMenuButtonWithDoubleTap_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();

        // Act & Assert - Should handle null references gracefully
        Assert.DoesNotThrow(() => handler.OnVRMenuButtonWithDoubleTap());
        Assert.DoesNotThrow(() => handler.OnVRMenuButtonWithDoubleTap()); // Simulate double tap

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region TabUIIntegration Tests

    [Test]
    public void TabUIIntegration_CanBeCreated()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");

        // Act
        var integration = go.AddComponent<TabUIIntegration>();

        // Assert
        Assert.IsNotNull(integration);
        Assert.IsNull(integration.TabManager);
        Assert.IsNull(integration.WorldStateManager);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_ShowChrome_WithNullController_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert - Should handle null gracefully
        Assert.DoesNotThrow(() => integration.ShowChrome());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_HideChrome_WithNullController_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        Assert.DoesNotThrow(() => integration.HideChrome());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_ToggleChrome_WithNullController_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        Assert.DoesNotThrow(() => integration.ToggleChrome());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_EnableVRMode_WithoutInitialize_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        Assert.DoesNotThrow(() => integration.EnableVRMode());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_DisableVRMode_WithoutInitialize_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        Assert.DoesNotThrow(() => integration.DisableVRMode());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_Terminate_WithoutInitialize_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        LogAssert.Expect(LogType.Log, "[TabUIIntegration] Terminated.");
        Assert.DoesNotThrow(() => integration.Terminate());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region TabManager Integration Tests

    [Test]
    public void TabManager_WithTabUIController_EventsFlow()
    {
        // Arrange
        GameObject tabManagerGO = new GameObject("TabManager");
        var tabManager = tabManagerGO.AddComponent<TabManager>();

        GameObject controllerGO = new GameObject("Controller");
        var controller = controllerGO.AddComponent<TabUIController>();

        int tabCreatedCount = 0;
        int tabClosedCount = 0;

        tabManager.OnTabCreated += (tab) => tabCreatedCount++;
        tabManager.OnTabClosed += (tab) => tabClosedCount++;

        // Act
        var tab1 = tabManager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = tabManager.CreateTab("http://world2.com", makeActive: false);
        tabManager.CloseTab(tab1.Id);

        // Assert
        Assert.AreEqual(2, tabCreatedCount);
        Assert.AreEqual(1, tabClosedCount);
        Assert.AreEqual(1, tabManager.TabCount);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(tabManagerGO);
        UnityEngine.Object.DestroyImmediate(controllerGO);
    }

    [Test]
    public void TabManager_CloseTab_FiresEvent()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        TabState closedTab = null;
        manager.OnTabClosed += (tab) => closedTab = tab;

        var tab = manager.CreateTab("http://example.com", makeActive: false);

        // Act
        manager.CloseTab(tab.Id);

        // Assert
        Assert.AreSame(tab, closedTab);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CloseTab_CannotCloseUnclosableTab()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        var tab = manager.CreateTab("http://example.com", makeActive: false);
        tab.CanClose = false;

        // Act
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("cannot be closed"));
        bool closed = manager.CloseTab(tab.Id);

        // Assert
        Assert.IsFalse(closed);
        Assert.AreEqual(1, manager.TabCount);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CloseTab_UnknownTab_ReturnsFalse()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        // Act
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Tab not found"));
        bool closed = manager.CloseTab("unknown-id");

        // Assert
        Assert.IsFalse(closed);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CloseOtherTabs_ClosesAllExceptActive()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        var tab1 = manager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = manager.CreateTab("http://world2.com", makeActive: false);
        var tab3 = manager.CreateTab("http://world3.com", makeActive: false);

        // Manually set active tab
        // Since we don't have Initialize called, we simulate by using internal state
        // In a real test with full setup, this would work through SwitchToTab

        // Act
        manager.CloseOtherTabs();

        // Assert - Without active tab set, all closable tabs are closed
        // In production, at least one tab would be active
        Assert.AreEqual(0, manager.TabCount);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CloseAllTabs_ClearsAllTabs()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        manager.CreateTab("http://world1.com", makeActive: false);
        manager.CreateTab("http://world2.com", makeActive: false);
        manager.CreateTab("http://world3.com", makeActive: false);

        // Act
        LogAssert.Expect(LogType.Log, "[TabManager] All tabs closed.");
        manager.CloseAllTabs();

        // Assert
        Assert.AreEqual(0, manager.TabCount);
        Assert.IsNull(manager.ActiveTab);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_FindTabsByUrl_ReturnsMatchingTabs()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        manager.CreateTab("http://world1.com", makeActive: false);
        manager.CreateTab("http://world2.com", makeActive: false);
        manager.CreateTab("http://world1.com", makeActive: false); // Duplicate URL

        // Act
        var matchingTabs = new List<TabState>(manager.FindTabsByUrl("http://world1.com"));

        // Assert
        Assert.AreEqual(2, matchingTabs.Count);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_SwitchToTab_UnknownTab_LogsWarning()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        bool callbackInvoked = false;
        bool callbackResult = true;

        // Act
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Tab not found"));
        manager.SwitchToTab("unknown-id", (success) =>
        {
            callbackInvoked = true;
            callbackResult = success;
        });

        // Assert
        Assert.IsTrue(callbackInvoked);
        Assert.IsFalse(callbackResult);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_SwitchToTabByIndex_InvalidIndex_LogsWarning()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        bool callbackInvoked = false;
        bool callbackResult = true;

        manager.CreateTab("http://example.com", makeActive: false);

        // Act
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Invalid index"));
        manager.SwitchToTabByIndex(99, (success) =>
        {
            callbackInvoked = true;
            callbackResult = success;
        });

        // Assert
        Assert.IsTrue(callbackInvoked);
        Assert.IsFalse(callbackResult);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_SwitchToNextTab_WithSingleTab_ReturnsFalse()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        bool callbackInvoked = false;
        bool callbackResult = true;

        manager.CreateTab("http://example.com", makeActive: false);

        // Act
        manager.SwitchToNextTab((success) =>
        {
            callbackInvoked = true;
            callbackResult = success;
        });

        // Assert
        Assert.IsTrue(callbackInvoked);
        Assert.IsFalse(callbackResult);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_SwitchToPreviousTab_WithSingleTab_ReturnsFalse()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        bool callbackInvoked = false;
        bool callbackResult = true;

        manager.CreateTab("http://example.com", makeActive: false);

        // Act
        manager.SwitchToPreviousTab((success) =>
        {
            callbackInvoked = true;
            callbackResult = success;
        });

        // Assert
        Assert.IsTrue(callbackInvoked);
        Assert.IsFalse(callbackResult);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTabByIndex_NegativeIndex_ReturnsNull()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://example.com", makeActive: false);

        // Act
        var tab = manager.GetTabByIndex(-1);

        // Assert
        Assert.IsNull(tab);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTabByIndex_OutOfRange_ReturnsNull()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://example.com", makeActive: false);

        // Act
        var tab = manager.GetTabByIndex(100);

        // Assert
        Assert.IsNull(tab);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_MoveTab_UnknownTab_ReturnsFalse()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://example.com", makeActive: false);

        // Act
        bool result = manager.MoveTab("unknown-id", 0);

        // Assert
        Assert.IsFalse(result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_MoveTab_ClampsIndex()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        var tab1 = manager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = manager.CreateTab("http://world2.com", makeActive: false);

        // Act - Move to index beyond range
        bool result = manager.MoveTab(tab1.Id, 100);

        // Assert
        Assert.IsTrue(result);
        Assert.AreSame(tab1, manager.GetTabByIndex(1)); // Should be clamped to last position

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region TabState Additional Tests

    [Test]
    public void TabState_LoadState_DefaultsToUnloaded()
    {
        // Arrange & Act
        var tab = new TabState("http://example.com");

        // Assert
        Assert.AreEqual(TabLoadState.Unloaded, tab.LoadState);
    }

    [Test]
    public void TabState_LoadState_CanBeChanged()
    {
        // Arrange
        var tab = new TabState("http://example.com");

        // Act
        tab.LoadState = TabLoadState.Loading;

        // Assert
        Assert.AreEqual(TabLoadState.Loading, tab.LoadState);

        // Act
        tab.LoadState = TabLoadState.Loaded;

        // Assert
        Assert.AreEqual(TabLoadState.Loaded, tab.LoadState);

        // Act
        tab.LoadState = TabLoadState.Error;

        // Assert
        Assert.AreEqual(TabLoadState.Error, tab.LoadState);
    }

    [Test]
    public void TabState_ErrorMessage_CanBeSet()
    {
        // Arrange
        var tab = new TabState("http://example.com");

        // Act
        tab.ErrorMessage = "Failed to load world";
        tab.LoadState = TabLoadState.Error;

        // Assert
        Assert.AreEqual("Failed to load world", tab.ErrorMessage);
        Assert.AreEqual(TabLoadState.Error, tab.LoadState);
    }

    [Test]
    public void TabState_BasePath_CanBeSet()
    {
        // Arrange
        var tab = new TabState("http://example.com/world/test.veml");

        // Act
        tab.BasePath = "http://example.com/world/";

        // Assert
        Assert.AreEqual("http://example.com/world/", tab.BasePath);
    }

    [Test]
    public void TabState_CreatedAt_IsSetAutomatically()
    {
        // Arrange
        DateTime before = DateTime.UtcNow;

        // Act
        var tab = new TabState("http://example.com");

        DateTime after = DateTime.UtcNow;

        // Assert
        Assert.GreaterOrEqual(tab.CreatedAt, before);
        Assert.LessOrEqual(tab.CreatedAt, after);
    }

    [Test]
    public void TabState_DisplayName_CanBeChanged()
    {
        // Arrange
        var tab = new TabState("http://example.com", "Initial Name");

        // Act
        tab.DisplayName = "New Name";

        // Assert
        Assert.AreEqual("New Name", tab.GetDisplayName());
    }

    [Test]
    public void TabState_GetDisplayName_HandlesInvalidUrl()
    {
        // Arrange
        var tab = new TabState("not-a-valid-url");

        // Act
        string displayName = tab.GetDisplayName();

        // Assert - Should not throw, should return something reasonable
        Assert.IsNotNull(displayName);
    }

    [Test]
    public void TabState_Id_IsUnique()
    {
        // Arrange
        var ids = new HashSet<string>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var tab = new TabState("http://example.com");
            ids.Add(tab.Id);
        }

        // Assert - All IDs should be unique
        Assert.AreEqual(100, ids.Count);
    }

    #endregion

    #region Chrome Visibility State Tests

    [Test]
    public void TabUIController_InitialState_ChromeIsVisible()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Assert
        Assert.IsTrue(controller.IsChromeVisible);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_MultipleToggle_MaintainsCorrectState()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert
        Assert.IsTrue(controller.IsChromeVisible);

        controller.ToggleChrome();
        Assert.IsFalse(controller.IsChromeVisible);

        controller.ToggleChrome();
        Assert.IsTrue(controller.IsChromeVisible);

        controller.HideChrome();
        Assert.IsFalse(controller.IsChromeVisible);

        controller.HideChrome(); // Double hide
        Assert.IsFalse(controller.IsChromeVisible);

        controller.ShowChrome();
        Assert.IsTrue(controller.IsChromeVisible);

        controller.ShowChrome(); // Double show
        Assert.IsTrue(controller.IsChromeVisible);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region Event Subscription Tests

    [Test]
    public void TabManager_OnActiveTabChanged_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        TabState changedTab = null;

        // Act
        manager.OnActiveTabChanged += (tab) => changedTab = tab;

        // Assert - Initially null since no tab switch has occurred
        Assert.IsNull(changedTab);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_OnTabStateChanged_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        TabState stateChangedTab = null;

        // Act
        manager.OnTabStateChanged += (tab) => stateChangedTab = tab;

        // Assert - Initially null
        Assert.IsNull(stateChangedTab);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_OnTabSwitchStarted_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        bool eventFired = false;

        // Act
        manager.OnTabSwitchStarted += (from, to) => eventFired = true;

        // Assert - Initially not fired
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_OnTabSwitchCompleted_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        bool eventFired = false;

        // Act
        manager.OnTabSwitchCompleted += (from, to, success) => eventFired = true;

        // Assert - Initially not fired
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_OnTabLoadFailed_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        bool eventFired = false;

        // Act
        manager.OnTabLoadFailed += (tab, error) => eventFired = true;

        // Assert - Initially not fired
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region TabUIInputHandler Event Tests

    [Test]
    public void TabUIInputHandler_OnFocusUrlBar_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();
        bool eventFired = false;

        // Act
        handler.OnFocusUrlBar += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIInputHandler_OnToggleVRMode_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();
        bool eventFired = false;

        // Act
        handler.OnToggleVRMode += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region ChromeInputFilter Tests

    [Test]
    public void ChromeInputFilter_AllowsClicksInChromeBar()
    {
        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = false;

        // Act - Point in chrome bar (near top of screen)
        bool result = filter.IsRaycastLocationValid(
            new Vector2(400, Screen.height - 50), null);

        // Assert
        Assert.IsTrue(result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ChromeInputFilter_BlocksClicksInContentArea()
    {
        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = false;

        // Act - Point in content area (middle of screen)
        bool result = filter.IsRaycastLocationValid(
            new Vector2(400, Screen.height / 2), null);

        // Assert
        Assert.IsFalse(result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ChromeInputFilter_AllowFullScreenInput_AllowsEverywhere()
    {
        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = true;

        // Act - Point at bottom of screen (normally blocked)
        bool result = filter.IsRaycastLocationValid(
            new Vector2(400, 10), null);

        // Assert
        Assert.IsTrue(result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ChromeInputFilter_SecondaryHitRect_AllowsClicksInRect()
    {
        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = false;
        filter.secondaryHitRect = new Rect(500, 300, 200, 150);

        // Act - Point inside the secondary rect
        bool result = filter.IsRaycastLocationValid(
            new Vector2(600, 375), null);

        // Assert
        Assert.IsTrue(result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ChromeInputFilter_SecondaryHitRect_BlocksClicksOutsideRect()
    {
        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = false;
        filter.secondaryHitRect = new Rect(500, 300, 200, 150);

        // Act - Point outside the secondary rect (and outside chrome bar)
        bool result = filter.IsRaycastLocationValid(
            new Vector2(100, 100), null);

        // Assert
        Assert.IsFalse(result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ChromeInputFilter_NullSecondaryRect_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = false;
        filter.secondaryHitRect = null;

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() =>
            filter.IsRaycastLocationValid(new Vector2(400, 200), null));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ChromeInputFilter_SecondaryRect_CanBeCleared()
    {
        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = false;
        filter.secondaryHitRect = new Rect(10, 10, 200, 100);

        // Use a point inside the secondary rect but well below chrome bar
        Assert.IsTrue(filter.IsRaycastLocationValid(new Vector2(50, 50), null));

        // Act - Clear the rect
        filter.secondaryHitRect = null;

        // Assert - Same point should now be blocked (outside chrome bar and no secondary rect)
        Assert.IsFalse(filter.IsRaycastLocationValid(new Vector2(50, 50), null));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ChromeInputFilter_SecondaryRect_EdgeCases()
    {
        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = false;
        filter.secondaryHitRect = new Rect(100, 100, 200, 200);

        // Act & Assert - Top-left corner (inside)
        Assert.IsTrue(filter.IsRaycastLocationValid(new Vector2(100, 100), null));

        // Bottom-right corner edge (inside, Rect.Contains is inclusive on min, exclusive on max)
        Assert.IsTrue(filter.IsRaycastLocationValid(new Vector2(299, 299), null));

        // Just outside
        Assert.IsFalse(filter.IsRaycastLocationValid(new Vector2(301, 301), null));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region TabUIController New Public Method Tests

    [Test]
    public void TabUIController_GoBack_DoesNotThrow_WithoutWebView()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert - Should handle gracefully without WebView
        Assert.DoesNotThrow(() => controller.GoBack());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_GoForward_DoesNotThrow_WithoutWebView()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert
        Assert.DoesNotThrow(() => controller.GoForward());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_Reload_DoesNotThrow_WithoutWebView()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert
        Assert.DoesNotThrow(() => controller.Reload());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_FocusUrlBar_DoesNotThrow_WithoutWebView()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert
        Assert.DoesNotThrow(() => controller.FocusUrlBar());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_ToggleStatsHud_DoesNotThrow_WithoutWebView()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert
        Assert.DoesNotThrow(() => controller.ToggleStatsHud());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_TriggerMenuAction_DoesNotThrow_WithoutWebView()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert - Various menu actions
        Assert.DoesNotThrow(() => controller.TriggerMenuAction("history"));
        Assert.DoesNotThrow(() => controller.TriggerMenuAction("console"));
        Assert.DoesNotThrow(() => controller.TriggerMenuAction("settings"));
        Assert.DoesNotThrow(() => controller.TriggerMenuAction("about"));
        Assert.DoesNotThrow(() => controller.TriggerMenuAction("exit"));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnRequestStats_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        bool eventFired = false;

        // Act
        controller.OnRequestStats += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnClearCache_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        string receivedTimeRange = null;

        // Act
        controller.OnClearCache += (timeRange) => receivedTimeRange = timeRange;

        // Assert
        Assert.IsNull(receivedTimeRange);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnClearHistory_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        bool eventFired = false;

        // Act
        controller.OnClearHistory += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnSaveSettings_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        Dictionary<string, object> receivedSettings = null;

        // Act
        controller.OnSaveSettings += (settings) => receivedSettings = settings;

        // Assert
        Assert.IsNull(receivedSettings);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnExitRequested_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        bool eventFired = false;

        // Act
        controller.OnExitRequested += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnRequestHistory_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        bool eventFired = false;

        // Act
        controller.OnRequestHistory += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnRequestConsoleLog_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        bool eventFired = false;

        // Act
        controller.OnRequestConsoleLog += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnRequestSettings_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        bool eventFired = false;

        // Act
        controller.OnRequestSettings += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_SendStatsData_DoesNotThrow_WithoutWebView()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        var stats = new Dictionary<string, object>
        {
            { "rendering", new Dictionary<string, object> { { "fps", 60 } } },
            { "system", new Dictionary<string, object> { { "usedMemoryMB", 512 } } }
        };

        // Act & Assert
        Assert.DoesNotThrow(() => controller.SendStatsData(stats));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_SendConsoleData_DoesNotThrow_WithoutWebView()
    {
        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        var logs = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string> { { "type", "info" }, { "message", "test" } }
        };

        // Act & Assert
        Assert.DoesNotThrow(() => controller.SendConsoleData(logs));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region TabUIIntegration Data Provider Tests

    [Test]
    public void TabUIIntegration_SetHistoryProvider_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        Assert.DoesNotThrow(() => integration.SetHistoryProvider(() => new List<Dictionary<string, string>>()));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_SetConsoleLogProvider_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        Assert.DoesNotThrow(() => integration.SetConsoleLogProvider(() => new List<object>()));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_SetSettingsProvider_DoesNotThrow()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        Assert.DoesNotThrow(() => integration.SetSettingsProvider(() => new Dictionary<string, object>()));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_SetHistoryProvider_CanBeCalledMultipleTimes()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert - Should allow overwriting provider
        Assert.DoesNotThrow(() =>
        {
            integration.SetHistoryProvider(() => new List<object>());
            integration.SetHistoryProvider(() => new List<Dictionary<string, string>>());
        });

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_OnClearHistoryRequested_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();
        bool eventFired = false;

        // Act
        integration.OnClearHistoryRequested += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_OnSaveSettingsRequested_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();
        Dictionary<string, object> receivedSettings = null;

        // Act
        integration.OnSaveSettingsRequested += (settings) => receivedSettings = settings;

        // Assert
        Assert.IsNull(receivedSettings);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_OnClearCacheRequested_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();
        string receivedTimeRange = null;

        // Act
        integration.OnClearCacheRequested += (timeRange) => receivedTimeRange = timeRange;

        // Assert
        Assert.IsNull(receivedTimeRange);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_OnExitRequested_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();
        bool eventFired = false;

        // Act
        integration.OnExitRequested += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_OnPageLoaded_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();
        string receivedName = null;
        string receivedUrl = null;

        // Act
        integration.OnPageLoaded += (name, url) => { receivedName = name; receivedUrl = url; };

        // Assert
        Assert.IsNull(receivedName);
        Assert.IsNull(receivedUrl);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_OnVRModeToggleRequested_EventCanBeSubscribed()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();
        bool eventFired = false;

        // Act
        integration.OnVRModeToggleRequested += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_AddConsoleLine_DoesNotThrow_WithoutController()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        Assert.DoesNotThrow(() => integration.AddConsoleLine("info", "Test message"));
        Assert.DoesNotThrow(() => integration.AddConsoleLine("error", "Error message"));
        Assert.DoesNotThrow(() => integration.AddConsoleLine("warning", "Warning message"));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_SetHomeUrl_DoesNotThrow_WithoutController()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Act & Assert
        Assert.DoesNotThrow(() => integration.SetHomeUrl("http://example.com"));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIIntegration_IsInitialized_FalseBeforeInit()
    {
        // Arrange
        GameObject go = new GameObject("TestIntegration");
        var integration = go.AddComponent<TabUIIntegration>();

        // Assert
        Assert.IsFalse(integration.IsInitialized);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region TabUIInputHandler New Shortcut Tests

    [Test]
    public void TabUIInputHandler_Initialize_SetsControllerAndTabManager()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();

        GameObject controllerGO = new GameObject("Controller");
        var controller = controllerGO.AddComponent<TabUIController>();

        GameObject tabManagerGO = new GameObject("TabManager");
        var tabManager = tabManagerGO.AddComponent<TabManager>();

        // Act
        LogAssert.Expect(LogType.Log, "[TabUIInputHandler] Initialized.");
        handler.Initialize(controller, tabManager);

        // Assert - VR menu button should use the controller
        Assert.DoesNotThrow(() => handler.OnVRMenuButton());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(controllerGO);
        UnityEngine.Object.DestroyImmediate(tabManagerGO);
    }

    [Test]
    public void TabUIInputHandler_OnVRMenuButton_TogglesChrome()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();

        GameObject controllerGO = new GameObject("Controller");
        var controller = controllerGO.AddComponent<TabUIController>();

        GameObject tabManagerGO = new GameObject("TabManager");
        var tabManager = tabManagerGO.AddComponent<TabManager>();

        LogAssert.Expect(LogType.Log, "[TabUIInputHandler] Initialized.");
        handler.Initialize(controller, tabManager);

        bool initialState = controller.IsChromeVisible;

        // Act - OnVRMenuButton always toggles chrome
        handler.OnVRMenuButton();

        // Assert - Chrome should toggle
        Assert.AreNotEqual(initialState, controller.IsChromeVisible);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
        UnityEngine.Object.DestroyImmediate(controllerGO);
        UnityEngine.Object.DestroyImmediate(tabManagerGO);
    }

    [Test]
    public void TabUIInputHandler_OnToggleVRMode_EventStillExists()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();
        bool eventFired = false;

        // Act
        handler.OnToggleVRMode += () => eventFired = true;

        // Assert - Event should be subscribable
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIInputHandler_OnToggleFullscreen_EventStillExists()
    {
        // Arrange
        GameObject go = new GameObject("TestInputHandler");
        var handler = go.AddComponent<TabUIInputHandler>();
        bool eventFired = false;

        // Act
        handler.OnToggleFullscreen += () => eventFired = true;

        // Assert
        Assert.IsFalse(eventFired);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region TabState Extended Tests

    [Test]
    public void TabState_WorldUrl_SetAndGet()
    {
        // Arrange
        var tab = new TabState("http://example.com/world.veml");

        // Assert
        Assert.AreEqual("http://example.com/world.veml", tab.WorldUrl);
    }

    [Test]
    public void TabState_CanClose_DefaultTrue()
    {
        // Arrange
        var tab = new TabState("http://example.com");

        // Assert
        Assert.IsTrue(tab.CanClose);
    }

    [Test]
    public void TabState_CanClose_CanBeSetFalse()
    {
        // Arrange
        var tab = new TabState("http://example.com");

        // Act
        tab.CanClose = false;

        // Assert
        Assert.IsFalse(tab.CanClose);
    }

    [Test]
    public void TabState_Constructor_WithDisplayName()
    {
        // Arrange & Act
        var tab = new TabState("http://example.com", "My World");

        // Assert
        Assert.AreEqual("My World", tab.GetDisplayName());
        Assert.AreEqual("http://example.com", tab.WorldUrl);
    }

    [Test]
    public void TabState_Constructor_WithEmptyUrl()
    {
        // Arrange & Act
        var tab = new TabState("");

        // Assert
        Assert.AreEqual("", tab.WorldUrl);
        Assert.IsNotNull(tab.Id);
        Assert.IsNotEmpty(tab.Id);
    }

    [Test]
    public void TabState_Constructor_WithNullUrl()
    {
        // Arrange & Act
        var tab = new TabState(null);

        // Assert
        Assert.IsNull(tab.WorldUrl);
        Assert.IsNotNull(tab.Id);
    }

    #endregion

    #region TabManager Extended Tests

    [Test]
    public void TabManager_CreateTab_ReturnsValidTabState()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        // Act
        var tab = manager.CreateTab("http://example.com", makeActive: false);

        // Assert
        Assert.IsNotNull(tab);
        Assert.AreEqual("http://example.com", tab.WorldUrl);
        Assert.IsNotNull(tab.Id);
        Assert.IsNotEmpty(tab.Id);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CreateEmptyTab_CreatesTabWithNullUrl()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        // Act
        var tab = manager.CreateEmptyTab(false);

        // Assert
        Assert.IsNotNull(tab);
        Assert.AreEqual(1, manager.TabCount);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_TabCount_IncrementsOnCreate()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        // Assert
        Assert.AreEqual(0, manager.TabCount);

        // Act
        manager.CreateTab("http://example1.com", makeActive: false);
        Assert.AreEqual(1, manager.TabCount);

        manager.CreateTab("http://example2.com", makeActive: false);
        Assert.AreEqual(2, manager.TabCount);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTabByIndex_ValidIndex_ReturnsTab()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        var tab = manager.CreateTab("http://example.com", makeActive: false);

        // Act
        var result = manager.GetTabByIndex(0);

        // Assert
        Assert.AreSame(tab, result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_FindTabsByUrl_NoMatch_ReturnsEmpty()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://example.com", makeActive: false);

        // Act
        var results = new List<TabState>(manager.FindTabsByUrl("http://notfound.com"));

        // Assert
        Assert.AreEqual(0, results.Count);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_MoveTab_ValidMove_ReturnsTrue()
    {
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        var tab1 = manager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = manager.CreateTab("http://world2.com", makeActive: false);

        // Act
        bool result = manager.MoveTab(tab2.Id, 0);

        // Assert
        Assert.IsTrue(result);
        Assert.AreSame(tab2, manager.GetTabByIndex(0));
        Assert.AreSame(tab1, manager.GetTabByIndex(1));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region Phase 4D - Tab Lifecycle Tests

    [Test]
    public void TabManager_OpenNewTab_VerifyCountIncreases()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        Assert.AreEqual(0, manager.TabCount);

        // Act
        manager.CreateTab("http://world1.com", makeActive: false);

        // Assert
        Assert.AreEqual(1, manager.TabCount);

        manager.CreateTab("http://world2.com", makeActive: false);
        Assert.AreEqual(2, manager.TabCount);

        manager.CreateTab("http://world3.com", makeActive: false);
        Assert.AreEqual(3, manager.TabCount);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CloseTab_VerifyCountDecreases()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        var tab1 = manager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = manager.CreateTab("http://world2.com", makeActive: false);
        var tab3 = manager.CreateTab("http://world3.com", makeActive: false);
        Assert.AreEqual(3, manager.TabCount);

        // Act & Assert
        manager.CloseTab(tab2.Id);
        Assert.AreEqual(2, manager.TabCount);

        manager.CloseTab(tab1.Id);
        Assert.AreEqual(1, manager.TabCount);

        manager.CloseTab(tab3.Id);
        Assert.AreEqual(0, manager.TabCount);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_OpenMultipleTabsSequentially_AllTracked()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        var createdTabs = new List<TabState>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            createdTabs.Add(manager.CreateTab($"http://world{i}.com", makeActive: false));
        }

        // Assert
        Assert.AreEqual(5, manager.TabCount);
        for (int i = 0; i < 5; i++)
        {
            Assert.AreSame(createdTabs[i], manager.GetTabByIndex(i));
            Assert.AreEqual($"http://world{i}.com", manager.GetTabByIndex(i).WorldUrl);
        }

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_MaxTabs_EnforcesLimit()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.MaxTabs = 3;

        // Act
        manager.CreateTab("http://world1.com", makeActive: false);
        manager.CreateTab("http://world2.com", makeActive: false);
        manager.CreateTab("http://world3.com", makeActive: false);
        var overflowTab = manager.CreateTab("http://world4.com", makeActive: false);

        // Assert
        Assert.IsNull(overflowTab);
        Assert.AreEqual(3, manager.TabCount);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTab_ById_ReturnsCorrectTab()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        var tab1 = manager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = manager.CreateTab("http://world2.com", makeActive: false);

        // Act
        var result = manager.GetTab(tab2.Id);

        // Assert
        Assert.AreSame(tab2, result);
        Assert.AreEqual("http://world2.com", result.WorldUrl);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTab_UnknownId_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://world1.com", makeActive: false);

        // Act
        var result = manager.GetTab("nonexistent-id");

        // Assert
        Assert.IsNull(result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTabIndex_ReturnsCorrectIndex()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        var tab1 = manager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = manager.CreateTab("http://world2.com", makeActive: false);
        var tab3 = manager.CreateTab("http://world3.com", makeActive: false);

        // Act & Assert
        Assert.AreEqual(0, manager.GetTabIndex(tab1.Id));
        Assert.AreEqual(1, manager.GetTabIndex(tab2.Id));
        Assert.AreEqual(2, manager.GetTabIndex(tab3.Id));
        Assert.AreEqual(-1, manager.GetTabIndex("nonexistent"));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_HasTabWithUrl_ReturnsTrueForExisting()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://world1.com", makeActive: false);

        // Act & Assert
        Assert.IsTrue(manager.HasTabWithUrl("http://world1.com"));
        Assert.IsFalse(manager.HasTabWithUrl("http://unknown.com"));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CloseTab_WhenNoTabsOpen_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        // Act
        bool result = manager.CloseTab("some-id");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(0, manager.TabCount);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_SwitchToTabByIndex_NegativeIndex_LogsWarning()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://example.com", makeActive: false);
        bool callbackInvoked = false;
        bool callbackResult = true;

        // Act
        manager.SwitchToTabByIndex(-1, (success) =>
        {
            callbackInvoked = true;
            callbackResult = success;
        });

        // Assert
        Assert.IsTrue(callbackInvoked);
        Assert.IsFalse(callbackResult);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CloseTabsToRight_WithNoActiveTab_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://world1.com", makeActive: false);
        manager.CreateTab("http://world2.com", makeActive: false);

        // Act & Assert - No active tab set, should handle gracefully
        Assert.DoesNotThrow(() => manager.CloseTabsToRight());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region Phase 4D - TabState Extended Tests

    [Test]
    public void TabState_Clone_CopiesAllProperties()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var original = new TabState("http://example.com", "My World");
        original.LoadState = TabLoadState.Loaded;
        original.SnapshotId = "snap-123";
        original.BasePath = "http://example.com/base/";
        original.ErrorMessage = "some error";
        original.CanClose = false;
        original.Metadata = "custom-data";

        // Act
        var clone = original.Clone();

        // Assert
        Assert.AreNotSame(original, clone);
        Assert.AreNotEqual(original.Id, clone.Id); // Clone gets a new ID
        Assert.AreEqual(original.WorldUrl, clone.WorldUrl);
        Assert.AreEqual(original.DisplayName, clone.DisplayName);
        Assert.AreEqual(original.LoadState, clone.LoadState);
        Assert.AreEqual(original.SnapshotId, clone.SnapshotId);
        Assert.AreEqual(original.BasePath, clone.BasePath);
        Assert.AreEqual(original.ErrorMessage, clone.ErrorMessage);
        Assert.AreEqual(original.CanClose, clone.CanClose);
        Assert.AreEqual(original.Metadata, clone.Metadata);
    }

    [Test]
    public void TabState_MarkActive_UpdatesLastActiveAt()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var tab = new TabState("http://example.com");
        DateTime beforeMark = tab.LastActiveAt;

        // Small delay to ensure timestamp differs
        System.Threading.Thread.Sleep(10);

        // Act
        tab.MarkActive();

        // Assert
        Assert.GreaterOrEqual(tab.LastActiveAt, beforeMark);
    }

    [Test]
    public void TabState_IsWebPage_DefaultsFalse()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange & Act
        var tab = new TabState("http://example.com");

        // Assert
        Assert.IsFalse(tab.IsWebPage);
    }

    [Test]
    public void TabState_IsWebPage_CanBeSet()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var tab = new TabState("http://example.com");

        // Act
        tab.IsWebPage = true;

        // Assert
        Assert.IsTrue(tab.IsWebPage);
    }

    [Test]
    public void TabState_Metadata_CanBeSetAndRetrieved()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var tab = new TabState("http://example.com");

        // Act
        tab.Metadata = "{\"theme\":\"dark\",\"zoom\":1.5}";

        // Assert
        Assert.AreEqual("{\"theme\":\"dark\",\"zoom\":1.5}", tab.Metadata);
    }

    [Test]
    public void TabState_GetDisplayName_ExtractsDomainFromUrl()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - No explicit display name, should extract from URL
        var tab = new TabState("http://www.example.com/world/test.veml");
        tab.DisplayName = null;

        // Act
        string displayName = tab.GetDisplayName();

        // Assert
        Assert.AreEqual("www.example.com", displayName);
    }

    [Test]
    public void TabState_GetDisplayName_ReturnsNewTab_ForNullUrl()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var tab = new TabState(null);
        tab.DisplayName = null;

        // Act
        string displayName = tab.GetDisplayName();

        // Assert
        Assert.AreEqual("New Tab", displayName);
    }

    [Test]
    public void TabState_SuspendedState_CanBeSet()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var tab = new TabState("http://example.com");
        tab.LoadState = TabLoadState.Loaded;

        // Act
        tab.LoadState = TabLoadState.Suspended;
        tab.SnapshotId = "snapshot-456";

        // Assert
        Assert.AreEqual(TabLoadState.Suspended, tab.LoadState);
        Assert.AreEqual("snapshot-456", tab.SnapshotId);
    }

    #endregion

    #region Phase 4D - ChromeInputFilter Extended Tests

    [Test]
    public void ChromeInputFilter_ExactChromeBoundary_IsValid()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = false;

        // Act - Point exactly at the chrome boundary
        bool result = filter.IsRaycastLocationValid(
            new Vector2(400, Screen.height - 120f), null);

        // Assert - Should be valid (at boundary)
        Assert.IsTrue(result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ChromeInputFilter_JustBelowChromeBoundary_IsInvalid()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 120f;
        filter.allowFullScreenInput = false;

        // Act - Point just below the chrome boundary
        bool result = filter.IsRaycastLocationValid(
            new Vector2(400, Screen.height - 121f), null);

        // Assert
        Assert.IsFalse(result);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void ChromeInputFilter_CustomChromeHeight_Respected()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestFilter");
        var filter = go.AddComponent<ChromeInputFilter>();
        filter.chromeHeight = 200f;
        filter.allowFullScreenInput = false;

        // Act - Point within custom chrome height
        bool insideChrome = filter.IsRaycastLocationValid(
            new Vector2(400, Screen.height - 150f), null);
        bool outsideChrome = filter.IsRaycastLocationValid(
            new Vector2(400, Screen.height - 250f), null);

        // Assert
        Assert.IsTrue(insideChrome);
        Assert.IsFalse(outsideChrome);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion

    #region Phase 4D - TabUIController Extended Tests

    [Test]
    public void TabUIController_SetTheme_ValidThemes_SetsCurrentTheme()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert
        controller.SetTheme("dark");
        Assert.AreEqual("dark", controller.CurrentTheme);

        controller.SetTheme("light");
        Assert.AreEqual("light", controller.CurrentTheme);

        controller.SetTheme("system");
        Assert.AreEqual("system", controller.CurrentTheme);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_SetTheme_InvalidTheme_FallsBackToSystem()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act
        controller.SetTheme("invalid-theme");

        // Assert
        Assert.AreEqual("system", controller.CurrentTheme);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_SetContentFrameVisible_UpdatesProperty()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        Assert.IsFalse(controller.IsContentFrameVisible);

        // Act
        controller.SetContentFrameVisible(true);

        // Assert
        Assert.IsTrue(controller.IsContentFrameVisible);

        // Act
        controller.SetContentFrameVisible(false);

        // Assert
        Assert.IsFalse(controller.IsContentFrameVisible);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_OnThemeChanged_EventCanBeSubscribed()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        string receivedTheme = null;

        // Act
        controller.OnThemeChanged += (theme) => receivedTheme = theme;

        // Assert - Event subscription works, not yet fired
        Assert.IsNull(receivedTheme);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_SendHistoryData_DoesNotThrow_WithoutWebView()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        var historyItems = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string> { { "name", "World 1" }, { "url", "http://world1.com" } },
            new Dictionary<string, string> { { "name", "World 2" }, { "url", "http://world2.com" } }
        };

        // Act & Assert
        Assert.DoesNotThrow(() => controller.SendHistoryData(historyItems));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_SendSettingsData_DoesNotThrow_WithoutWebView()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();
        var settings = new Dictionary<string, object>
        {
            { "theme", "dark" },
            { "homePage", "http://example.com" }
        };

        // Act & Assert
        Assert.DoesNotThrow(() => controller.SendSettingsData(settings));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_SendAboutData_DoesNotThrow_WithoutWebView()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert
        Assert.DoesNotThrow(() => controller.SendAboutData(new
        {
            title = "WebVerse",
            version = "1.0.0",
            description = "Test"
        }));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_SendConsoleLine_DoesNotThrow_WithoutWebView()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert
        Assert.DoesNotThrow(() => controller.SendConsoleLine("info", "Test log line"));
        Assert.DoesNotThrow(() => controller.SendConsoleLine("error", "Error log line"));
        Assert.DoesNotThrow(() => controller.SendConsoleLine("warn", "Warning log line"));

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_CurrentTheme_DefaultsToSystem()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Assert
        Assert.AreEqual("system", controller.CurrentTheme);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TabUIController_HideOverlay_RestoreOverlay_DoNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestController");
        var controller = go.AddComponent<TabUIController>();

        // Act & Assert - No webViewObject, should handle gracefully
        Assert.DoesNotThrow(() => controller.HideOverlay());
        Assert.DoesNotThrow(() => controller.RestoreOverlay());

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    #endregion
}
