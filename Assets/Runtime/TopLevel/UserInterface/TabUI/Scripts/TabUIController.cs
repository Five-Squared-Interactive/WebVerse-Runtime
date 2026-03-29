// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using FiveSQD.StraightFour.WorldState;
using FiveSQD.WebVerse.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if VUPLEX_XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif
#if VUPLEX_INCLUDED
using Vuplex.WebView;
#endif

namespace FiveSQD.WebVerse.Interface.TabUI
{
    /// <summary>
    /// Controller for the Vuplex WebView-based Tab UI chrome.
    /// Bridges TabManager state to the HTML/CSS/JS UI.
    /// </summary>
    public class TabUIController : MonoBehaviour
    {
        #region Serialized Fields

        /// <summary>
        /// The WebView prefab to use for the Tab UI.
        /// </summary>
        [SerializeField]
        private GameObject webViewPrefab;

        /// <summary>
        /// Whether this is a VR instance.
        /// </summary>
        [SerializeField]
        private bool isVR;

        /// <summary>
        /// Set VR mode before calling Initialize.
        /// </summary>
        public bool IsVR { get => isVR; set => isVR = value; }

        /// <summary>
        /// Parent transform for VR mode positioning.
        /// </summary>
        [SerializeField]
        private Transform vrParent;

        /// <summary>
        /// Set VR parent transform before calling Initialize.
        /// </summary>
        public Transform VRParent { get => vrParent; set => vrParent = value; }

        /// <summary>
        /// VR camera for canvas event camera. Set before Initialize.
        /// </summary>
        public Camera VRCamera { get; set; }

        #endregion

        #region Private Fields

        private GameObject webViewObject;
#if VUPLEX_INCLUDED
        private CanvasWebViewPrefab webViewPrefabComponent;
        private IWebView webView;
#endif
        private TabManager tabManager;
        private ChromeInputFilter inputFilter;
        private bool isInitialized;
        private bool webViewReady;
        private bool chromeVisible = true;
        private bool contentFrameVisible;

        // Navigation history for current tab
        private Stack<string> backHistory = new Stack<string>();
        private Stack<string> forwardHistory = new Stack<string>();

        // Queued messages to send once WebView is ready
        private Queue<string> pendingMessages = new Queue<string>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether the chrome is currently visible.
        /// </summary>
        public bool IsChromeVisible => chromeVisible;

        /// <summary>
        /// Whether the content frame is currently visible (webpage loaded).
        /// </summary>
        public bool IsContentFrameVisible => contentFrameVisible;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the Tab UI controller.
        /// </summary>
        /// <param name="tabManager">The tab manager instance.</param>
        /// <param name="webViewPrefab">Optional WebView prefab override.</param>
        public void Initialize(TabManager tabManager, GameObject webViewPrefab = null)
        {
            if (isInitialized)
            {
                Logging.LogWarning("[TabUIController->Initialize] Already initialized.");
                return;
            }

            this.tabManager = tabManager;

            if (webViewPrefab != null)
            {
                this.webViewPrefab = webViewPrefab;
            }

            if (this.webViewPrefab == null)
            {
                Logging.LogError("[TabUIController->Initialize] WebView prefab not set.");
                return;
            }

            // Subscribe to TabManager events
            SubscribeToTabManager();

            // Create and set up WebView
            SetupWebView();

            isInitialized = true;
            Logging.Log("[TabUIController] Initialized.");
        }

        /// <summary>
        /// Subscribe to TabManager events.
        /// </summary>
        private void SubscribeToTabManager()
        {
            if (tabManager == null) return;

            tabManager.OnTabCreated += HandleTabCreated;
            tabManager.OnTabClosed += HandleTabClosed;
            tabManager.OnActiveTabChanged += HandleActiveTabChanged;
            tabManager.OnTabStateChanged += HandleTabStateChanged;
            tabManager.OnTabSwitchStarted += HandleTabSwitchStarted;
            tabManager.OnTabSwitchCompleted += HandleTabSwitchCompleted;
            tabManager.OnTabThumbnailCaptured += HandleTabThumbnailCaptured;
        }

        /// <summary>
        /// Unsubscribe from TabManager events.
        /// </summary>
        private void UnsubscribeFromTabManager()
        {
            if (tabManager == null) return;

            tabManager.OnTabCreated -= HandleTabCreated;
            tabManager.OnTabClosed -= HandleTabClosed;
            tabManager.OnActiveTabChanged -= HandleActiveTabChanged;
            tabManager.OnTabStateChanged -= HandleTabStateChanged;
            tabManager.OnTabSwitchStarted -= HandleTabSwitchStarted;
            tabManager.OnTabSwitchCompleted -= HandleTabSwitchCompleted;
            tabManager.OnTabThumbnailCaptured -= HandleTabThumbnailCaptured;
        }

        /// <summary>
        /// Set up the Vuplex WebView.
        /// </summary>
        private void SetupWebView()
        {
#if VUPLEX_INCLUDED
            webViewObject = Instantiate(webViewPrefab);
            webViewPrefabComponent = webViewObject.GetComponentInChildren<CanvasWebViewPrefab>();

            if (webViewPrefabComponent == null)
            {
                Logging.LogError("[TabUIController->SetupWebView] CanvasWebViewPrefab not found in prefab.");
                return;
            }

            // Set sort order high so chrome overlays the 3D scene
            var canvas = webViewObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 100;
            }

            // Set up VR mode if needed
            if (isVR)
            {
                SetupVRMode();
            }

            // Wait for WebView to be ready
            webViewPrefabComponent.Initialized += OnWebViewInitialized;
#else
            Logging.LogWarning("[TabUIController->SetupWebView] Vuplex not included.");
#endif
        }

#if VUPLEX_INCLUDED
        /// <summary>
        /// Called when WebView is initialized.
        /// </summary>
        private void OnWebViewInitialized(object sender, EventArgs e)
        {
            webView = webViewPrefabComponent.WebView;

            // Disable the default white background so CSS transparent works
            webView.SetDefaultBackgroundEnabled(false);

            // Attach ChromeInputFilter to the Vuplex view's RawImage so clicks
            // in the content area pass through to the Content WebView underneath.
            var rawImage = webViewPrefabComponent.GetComponentInChildren<UnityEngine.UI.RawImage>();
            if (rawImage != null)
            {
                inputFilter = rawImage.gameObject.AddComponent<ChromeInputFilter>();
            }

            // Subscribe to messages from JS
            webView.MessageEmitted += OnWebViewMessage;

            // Load the Tab UI HTML from StreamingAssets
            string tabUIPath = System.IO.Path.Combine(Application.streamingAssetsPath, "TabUI", "index.html");

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android StreamingAssets needs special handling
            tabUIPath = "file:///android_asset/TabUI/index.html";
#elif UNITY_IOS && !UNITY_EDITOR
            tabUIPath = "file://" + tabUIPath;
#else
            tabUIPath = "file:///" + tabUIPath.Replace("\\", "/");
#endif

            webView.LoadUrl(tabUIPath);

            // The JS will send a 'ready' message when loaded
            Logging.Log("[TabUIController] WebView initialized, loading Tab UI.");
        }
#endif

        /// <summary>
        /// Set up VR mode positioning and input.
        /// </summary>
        private void SetupVRMode()
        {
            if (webViewObject == null) return;

            Canvas canvas = webViewObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;

                // Use the explicit VR camera for event processing
                if (VRCamera != null)
                {
                    canvas.worldCamera = VRCamera;
                }
                else
                {
                    canvas.worldCamera = Camera.main;
                    Logging.LogWarning("[TabUIController->SetupVRMode] VRCamera is null, falling back to Camera.main");
                }

                // Set up RectTransform for VR — large enough to show full UI
                RectTransform rt = webViewObject.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(1600, 900);
                    rt.localScale = Vector3.one * 0.001f;
                }

                // XRUIInputModule requires TrackedDeviceGraphicRaycaster for VR pointer interaction
                var graphicRaycaster = webViewObject.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster != null) graphicRaycaster.enabled = false;
#if VUPLEX_XR_INTERACTION_TOOLKIT
                if (webViewObject.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                    webViewObject.AddComponent<TrackedDeviceGraphicRaycaster>();
#endif
            }

            // Do NOT parent to VR rig — we position it manually on toggle
            // so it spawns in front of the user like the legacy Multibar
            webViewObject.transform.SetParent(null);

            // Start hidden — user toggles it on with the menu button
            webViewObject.SetActive(false);
            chromeVisible = false;

            // Send mode to WebView
            SendModeToWebView("vr");

            // Diagnostic logging
            _LogVRSetupDiagnostics();
        }

        /// <summary>
        /// Log diagnostics about the VR setup for debugging pointer interaction.
        /// </summary>
        private void _LogVRSetupDiagnostics()
        {
            Canvas canvas = webViewObject?.GetComponent<Canvas>();
            Logging.Log($"[TabUIController VR Diagnostics] Canvas: {(canvas != null ? "found" : "MISSING")}");
            if (canvas != null)
            {
                Logging.Log($"  renderMode={canvas.renderMode}, worldCamera={(canvas.worldCamera != null ? canvas.worldCamera.name : "NULL")}");
            }

#if VUPLEX_XR_INTERACTION_TOOLKIT
            var tdgr = webViewObject?.GetComponent<TrackedDeviceGraphicRaycaster>();
            Logging.Log($"  TrackedDeviceGraphicRaycaster: {(tdgr != null ? $"present, enabled={tdgr.enabled}" : "MISSING")}");
#endif

            var gr = webViewObject?.GetComponent<GraphicRaycaster>();
            Logging.Log($"  GraphicRaycaster: {(gr != null ? $"present, enabled={gr.enabled}" : "not present")}");

            // Check EventSystem input module
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es != null)
            {
                Logging.Log($"  EventSystem inputModule: {es.currentInputModule?.GetType().Name ?? "NULL"}");
            }

            // Check for Graphic raycast targets under the canvas
            var graphics = webViewObject?.GetComponentsInChildren<Graphic>(true);
            Logging.Log($"  Graphic children (including inactive): {graphics?.Length ?? 0}");
            if (graphics != null)
            {
                foreach (var g in graphics)
                {
                    Logging.Log($"    {g.gameObject.name}: type={g.GetType().Name}, raycastTarget={g.raycastTarget}, enabled={g.enabled}");
                }
            }
        }

        /// <summary>
        /// Position the VR chrome in front of the user's current view.
        /// Called each time the chrome is shown in VR mode.
        /// </summary>
        private void PositionVRChromeInFrontOfUser()
        {
            Camera vrCam = VRCamera != null ? VRCamera : Camera.main;
            if (vrCam == null || webViewObject == null) return;

            // Place 2m in front of the camera, matching the user's gaze direction
            // but keeping it level (no pitch)
            Vector3 forward = vrCam.transform.forward;
            forward.y = 0;
            forward.Normalize();
            webViewObject.transform.position = vrCam.transform.position + forward * 1.5f;
            webViewObject.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        #endregion

        #region WebView Message Handling

        /// <summary>
        /// Handle messages from the WebView.
        /// </summary>
#if VUPLEX_INCLUDED
        private void OnWebViewMessage(object sender, EventArgs<string> e)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<TabUIMessage>(e.Value);
                HandleUIMessage(message);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[TabUIController->OnWebViewMessage] Error parsing message: {ex.Message}");
            }
        }
#endif

        /// <summary>
        /// Handle a parsed UI message.
        /// </summary>
        private void HandleUIMessage(TabUIMessage message)
        {
            if (message == null) return;

            if (message.type != "requestStats" && message.type != "hudBounds")
                Logging.Log($"[TabUIController] Received message: {message.type}");

            switch (message.type)
            {
                case "ready":
                    HandleUIReady();
                    break;

                case "navigate":
                    HandleNavigate(message.url);
                    break;

                case "switchTab":
                    tabManager?.SwitchToTab(message.tabId);
                    break;

                case "closeTab":
                    tabManager?.CloseTab(message.tabId);
                    break;

                case "newTab":
                    tabManager?.CreateEmptyTab(true);
                    break;

                case "goBack":
                    HandleGoBack();
                    break;

                case "goForward":
                    HandleGoForward();
                    break;

                case "reload":
                    HandleReload();
                    break;

                case "menuAction":
                    HandleMenuAction(message.action);
                    break;

                case "requestHistory":
                    OnRequestHistory?.Invoke();
                    break;

                case "requestConsoleLog":
                    OnRequestConsoleLog?.Invoke();
                    break;

                case "requestSettings":
                    OnRequestSettings?.Invoke();
                    break;

                case "requestAboutInfo":
                    OnRequestAboutInfo?.Invoke();
                    break;

                case "clearHistory":
                    OnClearHistory?.Invoke();
                    break;

                case "saveSettings":
                    OnSaveSettings?.Invoke(message.settings);
                    break;

                case "exit":
                    OnExitRequested?.Invoke();
                    break;

                case "requestStats":
                    OnRequestStats?.Invoke();
                    break;

                case "requestHideChrome":
                    HideChrome();
                    break;

                case "themeChanged":
                    HandleThemeChanged(message.theme ?? "system");
                    break;

                case "requestThumbnail":
                    tabManager?.RequestThumbnail(message.tabId);
                    break;

                case "requestAllThumbnails":
                    tabManager?.RequestAllThumbnails();
                    break;

                case "clearCache":
                    OnClearCache?.Invoke(message.timeRange ?? "all");
                    break;

                case "hudBounds":
                    HandleHudBounds(message);
                    break;

                case "overlayOpened":
                    if (inputFilter != null) inputFilter.allowFullScreenInput = true;
                    break;

                case "overlayClosed":
                    if (inputFilter != null) inputFilter.allowFullScreenInput = false;
                    break;

                default:
                    Logging.LogWarning($"[TabUIController] Unknown message type: {message.type}");
                    break;
            }
        }

        /// <summary>
        /// Handle UI ready message.
        /// </summary>
        private void HandleUIReady()
        {
            webViewReady = true;

            // Send initial state
            SendModeToWebView(isVR ? "vr" : "desktop");
            SyncAllTabsToWebView();
            UpdateNavStateInWebView();

            // Send any pending messages
            while (pendingMessages.Count > 0)
            {
                ExecuteJavaScript(pendingMessages.Dequeue());
            }

            Logging.Log("[TabUIController] UI ready, initial state synced.");
        }

        /// <summary>
        /// Handle navigate request.
        /// </summary>
        private void HandleNavigate(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            // Add current URL to back history
            if (tabManager?.ActiveTab != null && !string.IsNullOrEmpty(tabManager.ActiveTab.WorldUrl))
            {
                backHistory.Push(tabManager.ActiveTab.WorldUrl);
                forwardHistory.Clear();
            }

            // Create tab with the URL or load in active tab
            if (tabManager?.ActiveTab?.WorldUrl == null)
            {
                // Active tab is empty, load here
                if (tabManager?.ActiveTab != null) tabManager.ActiveTab.WorldUrl = url;
                // Will need to trigger the actual world load via runtime
            }

            // Notify runtime to load the URL
            OnNavigateRequested?.Invoke(url);

            UpdateNavStateInWebView();
        }

        /// <summary>
        /// Handle go back.
        /// </summary>
        private void HandleGoBack()
        {
            if (backHistory.Count == 0) return;

            string currentUrl = tabManager?.ActiveTab?.WorldUrl;
            if (!string.IsNullOrEmpty(currentUrl))
            {
                forwardHistory.Push(currentUrl);
            }

            string prevUrl = backHistory.Pop();
            OnNavigateRequested?.Invoke(prevUrl);
            UpdateNavStateInWebView();
        }

        /// <summary>
        /// Handle go forward.
        /// </summary>
        private void HandleGoForward()
        {
            if (forwardHistory.Count == 0) return;

            string currentUrl = tabManager?.ActiveTab?.WorldUrl;
            if (!string.IsNullOrEmpty(currentUrl))
            {
                backHistory.Push(currentUrl);
            }

            string nextUrl = forwardHistory.Pop();
            OnNavigateRequested?.Invoke(nextUrl);
            UpdateNavStateInWebView();
        }

        /// <summary>
        /// Handle reload.
        /// </summary>
        private void HandleReload()
        {
            string currentUrl = tabManager?.ActiveTab?.WorldUrl;
            if (!string.IsNullOrEmpty(currentUrl))
            {
                OnNavigateRequested?.Invoke(currentUrl);
            }
        }

        /// <summary>
        /// Handle menu action.
        /// </summary>
        private void HandleMenuAction(string action)
        {
            OnMenuAction?.Invoke(action);
        }

        /// <summary>
        /// Handle HUD bounds message. Converts CSS coordinates (top-left origin)
        /// to screen coordinates (bottom-left origin) for the raycast filter.
        /// </summary>
        private void HandleHudBounds(TabUIMessage message)
        {
            if (inputFilter == null) return;

            if (message.visible)
            {
                // Convert CSS coords (origin top-left) to screen coords (origin bottom-left)
                float screenY = Screen.height - (message.y + message.height);
                inputFilter.secondaryHitRect = new Rect(message.x, screenY, message.width, message.height);
            }
            else
            {
                inputFilter.secondaryHitRect = null;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fired when navigation is requested.
        /// </summary>
        public event Action<string> OnNavigateRequested;

        /// <summary>
        /// Fired when a menu action is requested.
        /// </summary>
        public event Action<string> OnMenuAction;

        /// <summary>
        /// Fired when the HTML UI requests browsing history data.
        /// </summary>
        public event Action OnRequestHistory;

        /// <summary>
        /// Fired when the HTML UI requests console log data.
        /// </summary>
        public event Action OnRequestConsoleLog;

        /// <summary>
        /// Fired when the HTML UI requests current settings.
        /// </summary>
        public event Action OnRequestSettings;

        /// <summary>
        /// Fired when the HTML UI requests about info.
        /// </summary>
        public event Action OnRequestAboutInfo;

        /// <summary>
        /// Fired when the HTML UI requests history be cleared.
        /// </summary>
        public event Action OnClearHistory;

        /// <summary>
        /// Fired when the HTML UI saves settings.
        /// </summary>
        public event Action<Dictionary<string, object>> OnSaveSettings;

        /// <summary>
        /// Fired when the user confirms exit.
        /// </summary>
        public event Action OnExitRequested;

        /// <summary>
        /// Fired when the HTML UI requests performance stats.
        /// </summary>
        public event Action OnRequestStats;

        /// <summary>
        /// Fired when the HTML UI requests cache be cleared.
        /// </summary>
        public event Action<string> OnClearCache;

        #endregion

        #region TabManager Event Handlers

        private void HandleTabCreated(TabState tab)
        {
            SyncAllTabsToWebView();
        }

        private void HandleTabClosed(TabState tab)
        {
            SyncAllTabsToWebView();
        }

        private void HandleActiveTabChanged(TabState tab)
        {
            SyncAllTabsToWebView();
            SendActiveTabToWebView(tab?.Id);

            // Update URL bar
            if (tab != null)
            {
                SendUrlToWebView(tab.WorldUrl);
            }

            // Clear navigation history for new tab context
            backHistory.Clear();
            forwardHistory.Clear();
            UpdateNavStateInWebView();
        }

        private void HandleTabStateChanged(TabState tab)
        {
            // Sync full tab list (includes display name changes)
            SyncAllTabsToWebView();
            SendTabLoadStateToWebView(tab.Id, tab.LoadState.ToString().ToLower());

            // Show/hide content frame based on load state
            if (tab.Id == tabManager?.ActiveTab?.Id)
            {
                bool hasContent = tab.LoadState == TabLoadState.Loaded ||
                                  tab.LoadState == TabLoadState.Loading;
                SetContentFrameVisible(hasContent);
            }
        }

        private void HandleTabSwitchStarted(TabState fromTab, TabState toTab)
        {
            SetLoadingInWebView(true);
        }

        private void HandleTabSwitchCompleted(TabState fromTab, TabState toTab, bool success)
        {
            SetLoadingInWebView(false);
            if (success && toTab != null)
            {
                SendUrlToWebView(toTab.WorldUrl);
            }
        }

        private void HandleTabThumbnailCaptured(string tabId, string dataUrl)
        {
            SendThumbnailToWebView(tabId, dataUrl);
        }

        #endregion

        #region WebView State Updates

        /// <summary>
        /// Sync all tabs to the WebView.
        /// </summary>
        private void SyncAllTabsToWebView()
        {
            if (tabManager == null) return;

            var tabsData = tabManager.Tabs.Select(t => new TabUITabData
            {
                id = t.Id,
                url = t.WorldUrl,
                displayName = t.GetDisplayName(),
                loadState = t.LoadState.ToString().ToLower(),
                isActive = t.Id == tabManager.ActiveTab?.Id
            }).ToList();

            string json = JsonConvert.SerializeObject(tabsData);
            string js = $"window.tabUI?.updateTabs({json});";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Send active tab ID to WebView.
        /// </summary>
        private void SendActiveTabToWebView(string tabId)
        {
            string js = $"window.tabUI?.setActiveTab('{EscapeJs(tabId)}');";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Send tab load state to WebView.
        /// </summary>
        private void SendTabLoadStateToWebView(string tabId, string loadState)
        {
            string js = $"window.tabUI?.updateTabLoadState('{EscapeJs(tabId)}', '{EscapeJs(loadState)}');";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Send tab thumbnail to WebView.
        /// </summary>
        private void SendThumbnailToWebView(string tabId, string dataUrl)
        {
            // Data URLs can be very long, so we send them via a structured message approach
            string js = $"window.tabUI?.updateTabThumbnail('{EscapeJs(tabId)}', '{EscapeJs(dataUrl)}');";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Send URL to WebView.
        /// </summary>
        private void SendUrlToWebView(string url)
        {
            string js = $"window.tabUI?.setUrl('{EscapeJs(url ?? "")}');";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Set loading state in WebView.
        /// </summary>
        private void SetLoadingInWebView(bool isLoading)
        {
            string js = $"window.tabUI?.setLoading({(isLoading ? "true" : "false")});";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Update navigation state in WebView.
        /// </summary>
        private void UpdateNavStateInWebView()
        {
            bool canGoBack = backHistory.Count > 0;
            bool canGoForward = forwardHistory.Count > 0;
            bool canReload = tabManager?.ActiveTab?.WorldUrl != null;

            string js = $"window.tabUI?.updateNavState({(canGoBack ? "true" : "false")}, {(canGoForward ? "true" : "false")}, {(canReload ? "true" : "false")});";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Send mode to WebView.
        /// </summary>
        private void SendModeToWebView(string mode)
        {
            string js = $"window.tabUI?.setMode('{EscapeJs(mode)}');";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Execute JavaScript in the WebView.
        /// </summary>
        private void ExecuteJavaScript(string js)
        {
#if VUPLEX_INCLUDED
            if (!webViewReady)
            {
                pendingMessages.Enqueue(js);
                return;
            }

            webView?.ExecuteJavaScript(js);
#endif
        }

        /// <summary>
        /// Escape a string for JavaScript.
        /// </summary>
        private string EscapeJs(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        #endregion

        #region Menu Trigger

        /// <summary>
        /// Trigger a menu action in the Chrome WebView (e.g., open a modal).
        /// Called by TabUIInputHandler for keyboard shortcuts.
        /// </summary>
        /// <param name="action">The menu action (history, console, settings, about, exit).</param>
        public void TriggerMenuAction(string action)
        {
            ExecuteJavaScript($"window.tabUI?.openModal('{EscapeJs(action)}');");
        }

        /// <summary>
        /// Toggle the stats HUD overlay from C# (keyboard shortcut).
        /// </summary>
        public void ToggleStatsHud()
        {
            ExecuteJavaScript("window.tabUI?.toggleStatsHud();");
        }

        /// <summary>
        /// Focus the URL bar in the Chrome WebView.
        /// </summary>
        public void FocusUrlBar()
        {
            ExecuteJavaScript("document.getElementById('url-bar')?.focus(); document.getElementById('url-bar')?.select();");
        }

        /// <summary>
        /// Trigger navigation back.
        /// </summary>
        public void GoBack()
        {
            HandleGoBack();
        }

        /// <summary>
        /// Trigger navigation forward.
        /// </summary>
        public void GoForward()
        {
            HandleGoForward();
        }

        /// <summary>
        /// Trigger page reload.
        /// </summary>
        public void Reload()
        {
            HandleReload();
        }

        #endregion

        #region Chrome Visibility

        /// <summary>
        /// Show the chrome.
        /// </summary>
        public void ShowChrome()
        {
            chromeVisible = true;
            webViewObject?.SetActive(true);

            // In VR, reposition in front of the user each time
            if (isVR)
            {
                PositionVRChromeInFrontOfUser();
            }

            ExecuteJavaScript("window.tabUI?.showChrome();");
        }

        /// <summary>
        /// Hide the chrome.
        /// </summary>
        public void HideChrome()
        {
            chromeVisible = false;
            if (isVR)
            {
                // In VR, fully hide the WebView object
                webViewObject?.SetActive(false);
            }
            else
            {
                ExecuteJavaScript("window.tabUI?.hideChrome();");
                // Keep WebView active but chrome hidden via CSS
            }
        }

        /// <summary>
        /// Toggle chrome visibility.
        /// </summary>
        public void ToggleChrome()
        {
            Logging.Log($"[TabUIController] ToggleChrome called. isVR={isVR}, chromeVisible={chromeVisible}, webViewObject={(webViewObject != null ? "exists" : "null")}");
            if (chromeVisible)
            {
                HideChrome();
            }
            else
            {
                ShowChrome();
            }
        }

        /// <summary>
        /// Hide the chrome overlay completely (disables the Canvas).
        /// Used during thumbnail capture to keep UI out of screenshots.
        /// </summary>
        public void HideOverlay()
        {
            if (webViewObject != null) webViewObject.SetActive(false);
        }

        /// <summary>
        /// Restore the chrome overlay after capture.
        /// </summary>
        public void RestoreOverlay()
        {
            if (webViewObject != null) webViewObject.SetActive(true);
        }

        #endregion

        #region Toast Notifications

        /// <summary>
        /// Show a toast notification.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="type">Toast type (info, success, warning, error).</param>
        /// <param name="durationMs">Duration in milliseconds (0 for manual dismiss).</param>
        public void ShowToast(string message, string type = "info", int durationMs = 3000)
        {
            string js = $"window.tabUI?.showToast('{EscapeJs(message)}', '{EscapeJs(type)}', {durationMs});";
            ExecuteJavaScript(js);
        }

        #endregion

        #region Data Responses

        /// <summary>
        /// Send browsing history data to the Chrome WebView.
        /// </summary>
        /// <param name="historyItems">Serializable list of history items.</param>
        public void SendHistoryData(object historyItems)
        {
            string json = JsonConvert.SerializeObject(historyItems);
            ExecuteJavaScript($"window.tabUI?.updateHistory({json});");
        }

        /// <summary>
        /// Send console log data to the Chrome WebView.
        /// </summary>
        /// <param name="logLines">Serializable list of log lines.</param>
        public void SendConsoleData(object logLines)
        {
            string json = JsonConvert.SerializeObject(logLines);
            ExecuteJavaScript($"window.tabUI?.updateConsole({json});");
        }

        /// <summary>
        /// Send a single console line to the Chrome WebView.
        /// </summary>
        /// <param name="type">Log type (msg, warn, error, debug, internal).</param>
        /// <param name="message">Log message.</param>
        public void SendConsoleLine(string type, string message)
        {
            ExecuteJavaScript($"window.tabUI?.addConsoleLine('{EscapeJs(type)}', '{EscapeJs(message)}');");
        }

        /// <summary>
        /// Send current settings to the Chrome WebView.
        /// </summary>
        /// <param name="settings">Serializable settings object.</param>
        public void SendSettingsData(object settings)
        {
            string json = JsonConvert.SerializeObject(settings);
            ExecuteJavaScript($"window.tabUI?.updateSettings({json});");
        }

        /// <summary>
        /// Send about info to the Chrome WebView.
        /// </summary>
        /// <param name="aboutInfo">Serializable about info object.</param>
        public void SendAboutData(object aboutInfo)
        {
            string json = JsonConvert.SerializeObject(aboutInfo);
            ExecuteJavaScript($"window.tabUI?.updateAboutInfo({json});");
        }

        /// <summary>
        /// Send performance stats to the Chrome WebView.
        /// </summary>
        /// <param name="stats">Serializable stats object.</param>
        public void SendStatsData(object stats)
        {
            string json = JsonConvert.SerializeObject(stats);
            ExecuteJavaScript($"window.tabUI?.updateStats({json});");
        }

        #endregion

        #region Content Frame

        /// <summary>
        /// Show the content frame (rounded border for web content area).
        /// </summary>
        public void ShowContentFrame()
        {
            ExecuteJavaScript("window.tabUI?.showContentFrame();");
        }

        /// <summary>
        /// Hide the content frame.
        /// </summary>
        public void HideContentFrame()
        {
            ExecuteJavaScript("window.tabUI?.hideContentFrame();");
        }

        /// <summary>
        /// Set content frame visibility.
        /// </summary>
        /// <param name="visible">Whether the content frame should be visible.</param>
        public void SetContentFrameVisible(bool visible)
        {
            contentFrameVisible = visible;
            string js = $"window.tabUI?.setContentFrameVisible({(visible ? "true" : "false")});";
            ExecuteJavaScript(js);
        }

        #endregion

        #region Theme

        /// <summary>
        /// Current theme setting.
        /// </summary>
        public string CurrentTheme { get; private set; } = "system";

        /// <summary>
        /// Event fired when theme changes.
        /// </summary>
        public event Action<string> OnThemeChanged;

        /// <summary>
        /// Set the UI theme.
        /// </summary>
        /// <param name="theme">Theme to set: "system", "dark", or "light".</param>
        public void SetTheme(string theme)
        {
            if (theme != "system" && theme != "dark" && theme != "light")
            {
                Logging.LogWarning($"[TabUIController] Invalid theme: {theme}. Using 'system'.");
                theme = "system";
            }

            CurrentTheme = theme;
            string js = $"window.tabUI?.setTheme('{EscapeJs(theme)}');";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Handle theme change notification from WebView.
        /// </summary>
        private void HandleThemeChanged(string theme)
        {
            CurrentTheme = theme;
            OnThemeChanged?.Invoke(theme);
            Logging.Log($"[TabUIController] Theme changed to: {theme}");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Terminate the Tab UI controller.
        /// </summary>
        public void Terminate()
        {
            UnsubscribeFromTabManager();

#if VUPLEX_INCLUDED
            if (webViewPrefabComponent != null)
            {
                webViewPrefabComponent.Destroy();
                webViewPrefabComponent = null;
            }
#endif

            if (webViewObject != null)
            {
                Destroy(webViewObject);
                webViewObject = null;
            }

            isInitialized = false;
            webViewReady = false;

            Logging.Log("[TabUIController] Terminated.");
        }

        private void OnDestroy()
        {
            Terminate();
        }

        #endregion

        #region Message Types

        /// <summary>
        /// Message from the Tab UI WebView.
        /// </summary>
        [Serializable]
        private class TabUIMessage
        {
            public string type;
            public string url;
            public string tabId;
            public string action;
            public string theme;
            public string timeRange;
            public bool visible;
            public float x;
            public float y;
            public float width;
            public float height;
            public Dictionary<string, object> settings;
        }

        /// <summary>
        /// Tab data for sending to WebView.
        /// </summary>
        [Serializable]
        private class TabUITabData
        {
            public string id;
            public string url;
            public string displayName;
            public string loadState;
            public bool isActive;
            public string icon;
        }

        #endregion
    }
}
