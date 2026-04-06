// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.StraightFour.WorldState;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FiveSQD.WebVerse.Interface.TabUI
{
    /// <summary>
    /// Handles keyboard shortcuts for the Tab UI.
    /// Replaces MultibarInput with expanded tab management shortcuts.
    /// </summary>
    public class TabUIInputHandler : MonoBehaviour
    {
        #region Serialized Fields

        /// <summary>
        /// Reference to the Tab UI controller.
        /// </summary>
        [SerializeField]
        private TabUIController tabUIController;

        /// <summary>
        /// Reference to the Tab Manager.
        /// </summary>
        [SerializeField]
        private TabManager tabManager;

        /// <summary>
        /// Key to toggle chrome visibility.
        /// </summary>
        [SerializeField]
        private Key chromeToggleKey = Key.Tab;

        /// <summary>
        /// Alternative key to toggle chrome (also closes dropdowns first).
        /// </summary>
        [SerializeField]
        private Key escapeKey = Key.Escape;

        #endregion

        #region Private Fields

        private bool controlHeld;
        private bool shiftHeld;
        private bool altHeld;

        /// <summary>
        /// Cooldown to prevent double-toggle from key bounce.
        /// </summary>
        private float lastEscapeTime;
        private const float EscapeCooldown = 0.3f;

        #endregion

        #region Events

        /// <summary>
        /// Fired when fullscreen should be toggled.
        /// </summary>
        public event Action OnToggleFullscreen;

        /// <summary>
        /// Fired when VR mode should be toggled.
        /// </summary>
        public event Action OnToggleVRMode;

        /// <summary>
        /// Fired when URL bar should be focused.
        /// </summary>
        public event Action OnFocusUrlBar;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the input handler.
        /// </summary>
        /// <param name="controller">The Tab UI controller.</param>
        /// <param name="tabManager">The Tab Manager.</param>
        public void Initialize(TabUIController controller, TabManager tabManager)
        {
            this.tabUIController = controller;
            this.tabManager = tabManager;
            Logging.Log("[TabUIInputHandler] Initialized.");
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            // Track modifier keys
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            controlHeld = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
            shiftHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            altHeld = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;

            // Process key presses
            ProcessKeyboardInput(keyboard);
        }

        /// <summary>
        /// Process keyboard input.
        /// </summary>
        private void ProcessKeyboardInput(Keyboard keyboard)
        {
            // Chrome toggle (Tab key, no modifiers)
            if (keyboard[chromeToggleKey].wasPressedThisFrame && !controlHeld && !shiftHeld && !altHeld)
            {
                HandleChromeToggle();
                return;
            }

            // Escape key
            if (keyboard[escapeKey].wasPressedThisFrame)
            {
                HandleEscape();
                return;
            }

            // Alt+Key shortcuts
            if (altHeld)
            {
                ProcessAltShortcuts(keyboard);
            }

            // Control+Key shortcuts
            if (controlHeld)
            {
                ProcessControlShortcuts(keyboard);
            }

            // Function keys
            ProcessFunctionKeys(keyboard);
        }

        /// <summary>
        /// Process Alt+Key shortcuts.
        /// </summary>
        private void ProcessAltShortcuts(Keyboard keyboard)
        {
            // Alt+Left: Go Back
            if (keyboard.leftArrowKey.wasPressedThisFrame)
            {
                HandleGoBack();
                return;
            }

            // Alt+Right: Go Forward
            if (keyboard.rightArrowKey.wasPressedThisFrame)
            {
                HandleGoForward();
                return;
            }
        }

        /// <summary>
        /// Process Control+Key shortcuts.
        /// </summary>
        private void ProcessControlShortcuts(Keyboard keyboard)
        {
            // Ctrl+T: New Tab
            if (keyboard.tKey.wasPressedThisFrame)
            {
                HandleNewTab();
                return;
            }

            // Ctrl+W: Close Current Tab
            if (keyboard.wKey.wasPressedThisFrame)
            {
                HandleCloseCurrentTab();
                return;
            }

            // Ctrl+Tab / Ctrl+Shift+Tab: Cycle Tabs
            if (keyboard.tabKey.wasPressedThisFrame)
            {
                if (shiftHeld)
                {
                    HandlePreviousTab();
                }
                else
                {
                    HandleNextTab();
                }
                return;
            }

            // Ctrl+L: Focus URL Bar
            if (keyboard.lKey.wasPressedThisFrame)
            {
                HandleFocusUrlBar();
                return;
            }

            // Ctrl+R: Reload
            if (keyboard.rKey.wasPressedThisFrame)
            {
                HandleReload();
                return;
            }

            // Ctrl+H: History
            if (keyboard.hKey.wasPressedThisFrame)
            {
                HandleOpenHistory();
                return;
            }

            // Ctrl+S: Settings
            if (keyboard.sKey.wasPressedThisFrame)
            {
                HandleOpenSettings();
                return;
            }

            // Ctrl+I: About
            if (keyboard.iKey.wasPressedThisFrame)
            {
                HandleOpenAbout();
                return;
            }

            // Ctrl+Q: Exit
            if (keyboard.qKey.wasPressedThisFrame)
            {
                HandleExit();
                return;
            }

            // Ctrl+1-9: Switch to tab by index
            for (int i = 1; i <= 9; i++)
            {
                Key numKey = (Key)((int)Key.Digit1 + (i - 1));
                if (keyboard[numKey].wasPressedThisFrame)
                {
                    HandleSwitchToTabByIndex(i - 1);
                    return;
                }
            }
        }

        /// <summary>
        /// Process function key shortcuts.
        /// </summary>
        private void ProcessFunctionKeys(Keyboard keyboard)
        {
            // F3: Toggle Stats HUD
            if (keyboard.f3Key.wasPressedThisFrame)
            {
                HandleToggleStatsHud();
                return;
            }

            // F5: Reload
            if (keyboard.f5Key.wasPressedThisFrame)
            {
                HandleReload();
                return;
            }

            // F11: Fullscreen
            if (keyboard.f11Key.wasPressedThisFrame)
            {
                HandleToggleFullscreen();
                return;
            }

            // F12: Console
            if (keyboard.f12Key.wasPressedThisFrame)
            {
                HandleOpenConsole();
                return;
            }
        }

        #endregion

        #region Input Actions (for use with Input System PlayerInput)

        /// <summary>
        /// Called on chrome toggle input action.
        /// </summary>
        public void OnChromeToggle(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                HandleChromeToggle();
            }
        }

        /// <summary>
        /// Called on new tab input action.
        /// </summary>
        public void OnNewTab(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                HandleNewTab();
            }
        }

        /// <summary>
        /// Called on close tab input action.
        /// </summary>
        public void OnCloseTab(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                HandleCloseCurrentTab();
            }
        }

        /// <summary>
        /// Called on next tab input action.
        /// </summary>
        public void OnNextTab(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                HandleNextTab();
            }
        }

        /// <summary>
        /// Called on previous tab input action.
        /// </summary>
        public void OnPreviousTab(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                HandlePreviousTab();
            }
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Handle chrome toggle.
        /// </summary>
        private void HandleChromeToggle()
        {
            if (tabUIController != null)
            {
                tabUIController.ToggleChrome();
            }
        }

        /// <summary>
        /// Handle escape key. Shows chrome if hidden; does not hide chrome
        /// while the content frame is active (webpage loaded). Includes a
        /// cooldown to prevent key-bounce double-toggles.
        /// </summary>
        private void HandleEscape()
        {
            if (tabUIController == null) return;

            // Debounce to prevent key bounce
            if (Time.time - lastEscapeTime < EscapeCooldown) return;
            lastEscapeTime = Time.time;

            if (!tabUIController.IsChromeVisible)
            {
                tabUIController.ShowChrome();
            }
            else if (!tabUIController.IsContentFrameVisible)
            {
                tabUIController.HideChrome();
            }
        }

        /// <summary>
        /// Handle new tab.
        /// </summary>
        private void HandleNewTab()
        {
            if (tabManager != null)
            {
                tabManager.CreateEmptyTab(true);

                // Show chrome and focus URL bar
                if (tabUIController != null && !tabUIController.IsChromeVisible)
                {
                    tabUIController.ShowChrome();
                }
                tabUIController?.FocusUrlBar();
            }
        }

        /// <summary>
        /// Handle close current tab.
        /// </summary>
        private void HandleCloseCurrentTab()
        {
            if (tabManager?.ActiveTab != null)
            {
                tabManager.CloseTab(tabManager.ActiveTab.Id);
            }
        }

        /// <summary>
        /// Handle next tab.
        /// </summary>
        private void HandleNextTab()
        {
            tabManager?.SwitchToNextTab();
        }

        /// <summary>
        /// Handle previous tab.
        /// </summary>
        private void HandlePreviousTab()
        {
            tabManager?.SwitchToPreviousTab();
        }

        /// <summary>
        /// Handle switch to tab by index.
        /// </summary>
        private void HandleSwitchToTabByIndex(int index)
        {
            tabManager?.SwitchToTabByIndex(index);
        }

        /// <summary>
        /// Handle go back.
        /// </summary>
        private void HandleGoBack()
        {
            tabUIController?.GoBack();
        }

        /// <summary>
        /// Handle go forward.
        /// </summary>
        private void HandleGoForward()
        {
            tabUIController?.GoForward();
        }

        /// <summary>
        /// Handle reload.
        /// </summary>
        private void HandleReload()
        {
            tabUIController?.Reload();
        }

        /// <summary>
        /// Handle focus URL bar.
        /// </summary>
        private void HandleFocusUrlBar()
        {
            // Show chrome first if hidden
            if (tabUIController != null && !tabUIController.IsChromeVisible)
            {
                tabUIController.ShowChrome();
            }
            tabUIController?.FocusUrlBar();
        }

        /// <summary>
        /// Handle toggle stats HUD.
        /// </summary>
        private void HandleToggleStatsHud()
        {
            tabUIController?.ToggleStatsHud();
        }

        /// <summary>
        /// Handle open history — sends menuAction to Chrome WebView.
        /// </summary>
        private void HandleOpenHistory()
        {
            if (tabUIController != null && !tabUIController.IsChromeVisible)
            {
                tabUIController.ShowChrome();
            }
            tabUIController?.TriggerMenuAction("history");
        }

        /// <summary>
        /// Handle open console — sends menuAction to Chrome WebView.
        /// </summary>
        private void HandleOpenConsole()
        {
            tabUIController?.TriggerMenuAction("console");
        }

        /// <summary>
        /// Handle open settings — sends menuAction to Chrome WebView.
        /// </summary>
        private void HandleOpenSettings()
        {
            if (tabUIController != null && !tabUIController.IsChromeVisible)
            {
                tabUIController.ShowChrome();
            }
            tabUIController?.TriggerMenuAction("settings");
        }

        /// <summary>
        /// Handle open about — sends menuAction to Chrome WebView.
        /// </summary>
        private void HandleOpenAbout()
        {
            tabUIController?.TriggerMenuAction("about");
        }

        /// <summary>
        /// Handle exit — sends menuAction to Chrome WebView.
        /// </summary>
        private void HandleExit()
        {
            tabUIController?.TriggerMenuAction("exit");
        }

        /// <summary>
        /// Handle toggle fullscreen.
        /// </summary>
        private void HandleToggleFullscreen()
        {
            OnToggleFullscreen?.Invoke();
        }

        #endregion

        #region VR Controller Input

        /// <summary>
        /// Handle VR menu button press.
        /// </summary>
        public void OnVRMenuButton()
        {
            HandleChromeToggle();
        }

        /// <summary>
        /// Handle VR menu button double tap (for tab cycling).
        /// </summary>
        private float lastMenuButtonTime;
        private const float DoubleTapThreshold = 0.3f;

        public void OnVRMenuButtonWithDoubleTap()
        {
            float currentTime = Time.time;
            if (currentTime - lastMenuButtonTime < DoubleTapThreshold)
            {
                // Double tap - cycle tabs
                HandleNextTab();
            }
            else
            {
                // Single tap - toggle chrome
                HandleChromeToggle();
            }
            lastMenuButtonTime = currentTime;
        }

        #endregion
    }
}
