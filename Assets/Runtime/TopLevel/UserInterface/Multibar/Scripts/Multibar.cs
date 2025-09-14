// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FiveSQD.WebVerse.Interface.MultibarMenu
{
    /// <summary>
    /// Class for Multibar.
    /// </summary>
    public class Multibar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// Mode for Multibar.
        /// </summary>
        public enum MultibarMode { Desktop = 0, VR = 1, Mobile = 2 }

        /// <summary>
        /// Whether or not the Multibar is VR.
        /// </summary>
        public bool isVR;

        /// <summary>
        /// Other multibar (to synchronize state with).
        /// </summary>
        public Multibar otherMultibar;

        /// <summary>
        /// The camera object (for VR).
        /// </summary>
        public GameObject cameraObject;

        /// <summary>
        /// The desktop mode script.
        /// </summary>
        public DesktopMode desktopMode;

        /// <summary>
        /// The top level multibar object.
        /// </summary>
        public GameObject topLevelObject;

        /// <summary>
        /// The right click menu.
        /// </summary>
        public GameObject rightClickMenu;

        /// <summary>
        /// The multibar dropdown.
        /// </summary>
        public GameObject multibarDropdown;

        /// <summary>
        /// The desktop toggle tooltip.
        /// </summary>
        public GameObject desktopToggleTooltip;

        /// <summary>
        /// The VR toggle tooltip.
        /// </summary>
        public GameObject vrToggleTooltip;

        /// <summary>
        /// The fullscreen button object.
        /// </summary>
        public GameObject fullScreenButtonObject;

        /// <summary>
        /// The unfullscreen button object.
        /// </summary>
        public GameObject unFullScreenButtonObject;

        /// <summary>
        /// The VR button.
        /// </summary>
        public Button vrButton;

        /// <summary>
        /// The no VR button.
        /// </summary>
        public Button noVRButton;

        /// <summary>
        /// The VR button object.
        /// </summary>
        public GameObject vrButtonObject;

        /// <summary>
        /// The no VR button object.
        /// </summary>
        public GameObject noVRButtonObject;

        /// <summary>
        /// The URL back button.
        /// </summary>
        public Button urlBackButton;

        /// <summary>
        /// The URL forward button.
        /// </summary>
        public Button urlForwardButton;

        /// <summary>
        /// The URL reload button.
        /// </summary>
        public Button urlReloadButton;

        /// <summary>
        /// The history menu.
        /// </summary>
        public GameObject historyMenu;

        /// <summary>
        /// The settings menu.
        /// </summary>h
        public GameObject settingsMenu;

        /// <summary>
        /// The console menu.
        /// </summary>
        public GameObject consoleMenu;

        /// <summary>
        /// The about menu.
        /// </summary>
        public GameObject aboutMenu;

        /// <summary>
        /// The exit menu.
        /// </summary>
        public GameObject exitMenu;

        /// <summary>
        /// The tutorial menu.
        /// </summary>
        public GameObject tutorialMenu;

        /// <summary>
        /// The multibar input field.
        /// </summary>
        public TMP_InputField multibarInputField;

        /// <summary>
        /// The multibar input.
        /// </summary>
        public MultibarInput multibarInput;

        /// <summary>
        /// The multibar start position.
        /// </summary>
        public Vector2 multibarStartPosition;

        /// <summary>
        /// The multibar standard position.
        /// </summary>
        public Vector2 multibarStandardPosition;

        /// <summary>
        /// Whether or not the multibar is selected.
        /// </summary>
        private bool isSelected = false;

        /// <summary>
        /// Previous URLs.
        /// </summary>
        private Stack<string> prevURLs;

        /// <summary>
        /// Next URLs.
        /// </summary>
        private Stack<string> nextURLs;

        /// <summary>
        /// Settings.
        /// </summary>
        private NativeSettings settings;

        /// <summary>
        /// Whether or not control is pressed.
        /// </summary>
        private bool controlPressed;

        /// <summary>
        /// Initialize the multibar.
        /// </summary>
        /// <param name="mode">Mode to initialize in.</param>
        /// <param name="settings">Settings.</param>
        public void Initialize(MultibarMode mode, NativeSettings settings = null)
        {
            rightClickMenu.SetActive(false);
            multibarDropdown.SetActive(false);
            prevURLs = new Stack<string>();
            nextURLs = new Stack<string>();
            UpdateNavButtons();
            MoveToStartPosition();
            this.settings = settings;
            controlPressed = false;
            if (multibarInput.keyboard != null)
            {
                Input.Keyboard.Keyboard keyboard = multibarInput.keyboard.GetComponent<Input.Keyboard.Keyboard>();
                if (keyboard == null)
                {
                    Logging.LogError("[Multibar->Initialize] Error getting keyboard script.");
                }
                else
                {
                    keyboard.onEnter += new Action<string>((url) => {
                        Enter();
                    });
                }
            }

            switch (mode)
            {
                case MultibarMode.VR:
                    InitializeVRMode();
                    break;

                case MultibarMode.Desktop:
                    InitializeDesktopMode();
                    break;

                case MultibarMode.Mobile:
                    InitializeMobileMode();
                    break;

                default:
                    Logging.LogError("[Multibar->Initialize] Invalid mode. Defaulting to desktop.");
                    InitializeDesktopMode();
                    break;
            }
        }

        /// <summary>
        /// Toggle the multibar.
        /// </summary>
        public void ToggleMultibar()
        {
            gameObject.SetActive(!gameObject.activeSelf);
            if (isVR)
            {
                transform.position = cameraObject.transform.position;
                transform.rotation = Quaternion.Euler(cameraObject.transform.rotation.eulerAngles.x,
                    cameraObject.transform.rotation.eulerAngles.y, 0);
                if (multibarInput.keyboard != null)
                {
                    multibarInput.keyboard.SetActive(false);
                }
            }
            if (desktopToggleTooltip != null)
            {
                desktopToggleTooltip.SetActive(false);
            }
            if (vrToggleTooltip != null)
            {
                vrToggleTooltip.SetActive(false);
            }
            if (multibarDropdown != null)
            {
                multibarDropdown.SetActive(false);
            }
        }

        /// <summary>
        /// Toggle the multibar dropdown.
        /// </summary>
        public void ToggleMultibarDropdown()
        {
            multibarDropdown.SetActive(!multibarDropdown.activeSelf);
            MoveToStandardPosition();
        }

        /// <summary>
        /// Called when pointer enters.
        /// </summary>
        /// <param name="data">Pointer event data.</param>
        public void OnPointerEnter(PointerEventData data)
        {

        }

        /// <summary>
        /// Called when pointer exits.
        /// </summary>
        /// <param name="data">Pointer event data.</param>
        public void OnPointerExit(PointerEventData data)
        {

        }

        /// <summary>
        /// Called when URL text changes.
        /// </summary>
        /// <param name="urlText">New URL text.</param>
        public void OnURLTextChange(string urlText)
        {
            if (otherMultibar != null)
            {
                otherMultibar.multibarInputField.text = urlText;
            }
        }

        /// <summary>
        /// Called when selected.
        /// </summary>
        public void Selected()
        {
            isSelected = true;

            if (multibarInput.keyboard != null)
            {
                multibarInput.keyboard.SetActive(true);
            }
        }

        /// <summary>
        /// Called when deselected.
        /// </summary>
        public void Deselected()
        {
            isSelected = false;

            if (multibarInput.keyboard != null)
            {
                //multibarInput.keyboard.SetActive(false);
            }
        }

        /// <summary>
        /// Set the URL.
        /// </summary>
        /// <param name="url">URL to set.</param>
        public void SetURL(string url)
        {
            multibarInputField.text = url;
        }

        /// <summary>
        /// Perform a submit.
        /// </summary>
        public void Enter()
        {
            if (WebVerseRuntime.Instance.currentURL != null)
            {
                prevURLs.Push(WebVerseRuntime.Instance.currentURL);
            }
            LoadURL(multibarInputField.text);
            UpdateNavButtons();
            MoveToStandardPosition();
            if (otherMultibar != null)
            {
                otherMultibar.UpdateNavButtonsFromOtherMultibar(prevURLs, nextURLs, multibarInputField.text);
            }
            EventSystem.current.SetSelectedGameObject(null);
        }

        /// <summary>
        /// Update the nav buttons with information from the other multibar.
        /// </summary>
        /// <param name="prevURLs">Prev URLs stack.</param>
        /// <param name="nextURLs">Next URLs stack.</param>
        /// <param name="currentURL">Current URL.</param>
        public void UpdateNavButtonsFromOtherMultibar(Stack<string> prevURLs, Stack<string> nextURLs, string currentURL)
        {
            this.prevURLs = prevURLs;
            this.nextURLs = nextURLs;
            multibarInputField.text = currentURL;
            UpdateNavButtons();
        }

        /// <summary>
        /// Perform a submit.
        /// </summary>
        /// <param name="context">Input calling context.</param>
        public void Enter(InputAction.CallbackContext context)
        {
            if (isSelected && context.phase == InputActionPhase.Started && !string.IsNullOrEmpty(multibarInputField.text))
            {
                Enter();
            }
        }

        /// <summary>
        /// Handle control key.
        /// </summary>
        /// <param name="context">Input calling context.</param>
        public void Control(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                controlPressed = true;
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                controlPressed = false;
            }
        }

        /// <summary>
        /// Handle H key.
        /// </summary>
        /// <param name="context">Callback calling context.</param>
        public void H(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (controlPressed)
                {
                    // Ctrl+H.
                    History();
                }
            }
        }

        /// <summary>
        /// Handle S key.
        /// </summary>
        /// <param name="context">Callback calling context.</param>
        public void S(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (controlPressed)
                {
                    // Ctrl+S.
                    Settings();
                }
            }
        }

        /// <summary>
        /// Handle F12 key.
        /// </summary>
        /// <param name="context">Callback calling context.</param>
        public void F12(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                Console();
            }
        }

        /// <summary>
        /// Handle I key.
        /// </summary>
        /// <param name="context">Callback calling context.</param>
        public void I(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (controlPressed)
                {
                    // Ctrl+I.
                    About();
                }
            }
        }

        /// <summary>
        /// Handle Q key.
        /// </summary>
        /// <param name="context">Callback calling context.</param>
        public void Q(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (controlPressed)
                {
                    // Ctrl+Q.
                    Exit();
                }
            }
        }

        /// <summary>
        /// Handle URL back press.
        /// </summary>
        public void URLBack()
        {
            if (prevURLs.Count > 0)
            {
                nextURLs.Push(WebVerseRuntime.Instance.currentURL);
                LoadURL(prevURLs.Pop());
                UpdateNavButtons();
                if (otherMultibar != null)
                {
                    otherMultibar.UpdateNavButtonsFromOtherMultibar(prevURLs, nextURLs, multibarInputField.text);
                }
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// Handle URL forward press.
        /// </summary>
        public void URLForward()
        {
            if (nextURLs.Count > 0)
            {
                prevURLs.Push(WebVerseRuntime.Instance.currentURL);
                LoadURL(nextURLs.Pop());
                UpdateNavButtons();
                if (otherMultibar != null)
                {
                    otherMultibar.UpdateNavButtonsFromOtherMultibar(prevURLs, nextURLs, multibarInputField.text);
                }
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// Handle URL reload press.
        /// </summary>
        public void URLReload()
        {
            LoadURL(WebVerseRuntime.Instance.currentURL);
            EventSystem.current.SetSelectedGameObject(null);
        }

        /// <summary>
        /// Handle fullscreen press.
        /// </summary>
        public void FullScreen()
        {
            if (isVR)
            {
                fullScreenButtonObject.SetActive(false);
                unFullScreenButtonObject.SetActive(false);
            }
            else
            {
                fullScreenButtonObject.SetActive(false);
                unFullScreenButtonObject.SetActive(true);
                Screen.fullScreen = true;
            }
        }

        /// <summary>
        /// Handle unfullscreen press.
        /// </summary>
        public void UnFullScreen()
        {
            if (isVR)
            {
                fullScreenButtonObject.SetActive(false);
                unFullScreenButtonObject.SetActive(false);
            }
            else
            {
                fullScreenButtonObject.SetActive(true);
                unFullScreenButtonObject.SetActive(false);
                Screen.fullScreen = false;
            }
        }

        /// <summary>
        /// Handle VR press.
        /// </summary>
        public void VR()
        {
            vrButtonObject.SetActive(false);
            noVRButtonObject.SetActive(true);
            desktopMode?.EnableVR();
        }

        /// <summary>
        /// Handle No VR press.
        /// </summary>
        public void NoVR()
        {
            vrButtonObject.SetActive(true);
            noVRButtonObject.SetActive(false);
            desktopMode?.DisableVR();
        }

        /// <summary>
        /// Set up VR/No VR buttons for desktop multibar.
        /// <param name="vrEnabled">Whether or not VR is enabled.</param>
        /// </summary>
        public void SetUpDesktopMultibarVRButton(bool vrEnabled)
        {
            vrButtonObject.SetActive(!vrEnabled);
            noVRButtonObject.SetActive(vrEnabled);
        }

        /// <summary>
        /// Set up VR/No VR buttons for VR multibar. Will set to VR state
        /// (as VR multibar will be disabled when in Non VR state).
        /// </summary>
        public void SetUpVRMultibarVRButton()
        {
            vrButtonObject.SetActive(false);
            noVRButtonObject.SetActive(true);
        }

        /// <summary>
        /// Handle settings press.
        /// </summary>
        public void Settings()
        {
            CloseAllMenus();
            settingsMenu.SetActive(true);
            Settings.Settings settingsScript = settingsMenu.GetComponent<Settings.Settings>();
            if (settingsScript == null)
            {
                Logging.LogError("[Multibar->Settings] Unable to find Settings Menu script.");
                return;
            }
            settingsScript.Initialize();
        }

        /// <summary>
        /// Handle history press.
        /// </summary>
        public void History()
        {
            CloseAllMenus();
            historyMenu.SetActive(true);
            History.History historyMenuScript = historyMenu.GetComponent<History.History>();
            if (historyMenuScript == null)
            {
                Logging.LogError("[Multibar->History] Unable to find History Menu script.");
                return;
            }
            historyMenuScript.Initialize();
        }

        /// <summary>
        /// Handle console press.
        /// </summary>
        public void Console()
        {
            CloseAllMenus();
            consoleMenu.SetActive(true);
        }

        /// <summary>
        /// Handle about press.
        /// </summary>
        public void About()
        {
            CloseAllMenus();
            aboutMenu.SetActive(true);
            About.About aboutMenuScript = aboutMenu.GetComponent<About.About>();
            if (aboutMenuScript == null)
            {
                Logging.LogError("[Multibar->About] Unable to find About Menu script.");
                return;
            }
            aboutMenuScript.Initialize();
        }

        /// <summary>
        /// Handle exit press.
        /// </summary>
        public void Exit()
        {
            CloseAllMenus();
            exitMenu.SetActive(true);
            ExitMenu.ExitMenu exitMenuScript = exitMenu.GetComponent<ExitMenu.ExitMenu>();
            if (exitMenuScript == null)
            {
                Logging.LogError("[Multibar->Exit] Unable to find Exit Menu script.");
                return;
            }
            exitMenuScript.Initialize(null);
        }

        /// <summary>
        /// Show tutorial.
        /// </summary>
        public void Tutorial()
        {
            CloseAllMenus();
            MoveToStandardPosition();
            tutorialMenu.SetActive(true);
            Tutorial.Tutorial tutorialMenuScript = tutorialMenu.GetComponent<Tutorial.Tutorial>();
            if (tutorialMenuScript == null)
            {
                Logging.LogError("[Multibar->Tutorial] Unable to find Tutorial Menu script.");
                return;
            }
            tutorialMenuScript.Initialize(new Action(() =>
            {
                if (settings != null)
                {
                    settings.SetTutorialState(NativeSettings.TutorialState.DO_NOT_SHOW);
                }
            }));
        }

        /// <summary>
        /// Initialize desktop mode.
        /// </summary>
        private void InitializeDesktopMode()
        {
            desktopToggleTooltip?.SetActive(true);
            vrToggleTooltip?.SetActive(false);
        }

        /// <summary>
        /// Initialize VR mode.
        /// </summary>
        private void InitializeVRMode()
        {
            desktopToggleTooltip.SetActive(false);
            vrToggleTooltip.SetActive(true);
        }

        /// <summary>
        /// Initialize mobile mode.
        /// </summary>
        private void InitializeMobileMode()
        {

        }

        /// <summary>
        /// Close all menus.
        /// </summary>
        private void CloseAllMenus()
        {
            desktopToggleTooltip.SetActive(false);
            vrToggleTooltip.SetActive(false);
            multibarDropdown.SetActive(false);
            if (historyMenu != null) historyMenu.SetActive(false);
            if (settingsMenu != null) settingsMenu.SetActive(false);
            if (consoleMenu != null) consoleMenu.SetActive(false);
            if (aboutMenu != null) aboutMenu.SetActive(false);
            if (exitMenu != null) exitMenu.SetActive(false);
        }

        /// <summary>
        /// Load a URL.
        /// </summary>
        /// <param name="url">URL to load.</param>
        private void LoadURL(string url)
        {
            WebVerseRuntime.Instance.LoadURL(url, new Action<string>((name) =>
            {
                AddToHistory(DateTime.Now, name, url);
                ToggleMultibar();
                ToggleMultibar();
                if (tutorialMenu != null)
                {
                    if (tutorialMenu.activeSelf)
                    {
                        tutorialMenu.SetActive(false);
                        tutorialMenu.SetActive(true);
                    }
                }
            }));
        }

        /// <summary>
        /// Add entry to history.
        /// </summary>
        /// <param name="timestamp">Timestamp.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="siteURL">Site URL.</param>
        private void AddToHistory(DateTime timestamp, string siteName, string siteURL)
        {
            desktopMode?.desktopHistory.AddItemToHistory(timestamp, siteName, siteURL);
        }

        /// <summary>
        /// Update navigation buttons.
        /// </summary>
        private void UpdateNavButtons()
        {
            if (prevURLs.Count > 0)
            {
                urlBackButton.interactable = true;
            }
            else
            {
                urlBackButton.interactable = false;
            }

            if (nextURLs.Count > 0)
            {
                urlForwardButton.interactable = true;
            }
            else
            {
                urlForwardButton.interactable = false;
            }

            if (WebVerseRuntime.Instance.currentURL == null)
            {
                urlReloadButton.interactable = false;
            }
            else
            {
                urlReloadButton.interactable = true;
            }
        }

        /// <summary>
        /// Move multibar to start position.
        /// </summary>
        private void MoveToStartPosition()
        {
            if (isVR)
            {
                return;
            }

            RectTransform rt = topLevelObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                Logging.LogError("[Multibar->MoveToStartPosition] Object does not contain RectTransform.");
                return;
            }

            rt.anchoredPosition = multibarStartPosition;
        }

        /// <summary>
        /// Move multibar to standard position.
        /// </summary>
        private void MoveToStandardPosition()
        {
            if (isVR)
            {
                return;
            }

            RectTransform rt = topLevelObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                Logging.LogError("[Multibar->MoveToStandardPosition] Object does not contain RectTransform.");
                return;
            }

            rt.anchoredPosition = multibarStandardPosition;
        }
    }
}