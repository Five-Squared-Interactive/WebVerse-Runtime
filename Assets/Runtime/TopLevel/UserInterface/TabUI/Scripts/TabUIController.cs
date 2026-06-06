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
using UnityEngine.XR.Interaction.Toolkit.Interactors;
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

        private bool isMobile;

        /// <summary>
        /// Set mobile mode before calling Initialize.
        /// </summary>
        public bool IsMobile { get => isMobile; set => isMobile = value; }

        private bool isTablet;

        /// <summary>
        /// Set tablet mode before calling Initialize. Implies mobile.
        /// </summary>
        public bool IsTablet { get => isTablet; set => isTablet = value; }

        /// <summary>
        /// Returns the mode string based on current platform flags.
        /// Priority: mobile/tablet > vr > desktop.
        /// </summary>
        public string GetModeString()
        {
            return isMobile
                ? (isTablet ? "tablet" : "mobile")
                : (isVR ? "vr" : "desktop");
        }

        /// <summary>
        /// Chrome bar position: "top" or "bottom". Set before Initialize.
        /// </summary>
        public string ChromePosition { get; set; } = "bottom";

        /// <summary>
        /// Safe area insets in pixels from each screen edge.
        /// </summary>
        public struct SafeAreaInsets
        {
            public float top, bottom, left, right;
        }

        /// <summary>
        /// Compute safe area insets from a safe area rect and screen dimensions.
        /// </summary>
        public static SafeAreaInsets GetSafeAreaInsets(Rect safeArea, int screenWidth, int screenHeight)
        {
            return new SafeAreaInsets
            {
                top = screenHeight - (safeArea.y + safeArea.height),
                bottom = safeArea.y,
                left = safeArea.x,
                right = screenWidth - (safeArea.x + safeArea.width)
            };
        }

        /// <summary>
        /// Returns true if orientation has changed from cached value.
        /// </summary>
        public static bool HasOrientationChanged(ScreenOrientation cached, ScreenOrientation current)
        {
            return cached != current;
        }

        private ScreenOrientation cachedOrientation;
        private Rect cachedSafeArea;
        private bool cachedKeyboardVisible;
        private int cachedKeyboardHeight;

        /// <summary>
        /// Formats a JavaScript call to setKeyboardState with the given visibility and height.
        /// </summary>
        public static string FormatKeyboardStateMessage(bool visible, int height)
        {
            string visibleStr = visible ? "true" : "false";
            return $"window.tabUI?.setKeyboardState({{ visible: {visibleStr}, height: {height} }});";
        }

        /// <summary>
        /// Extracts keyboard height in pixels from the keyboard area rect.
        /// Returns 0 if keyboard is not visible.
        /// </summary>
        public static int GetKeyboardHeight(Rect keyboardArea)
        {
            if (keyboardArea.height <= 0) return 0;
            return (int)keyboardArea.height;
        }

        /// <summary>
        /// Determines if a tap at the given Y coordinate is within the edge zone
        /// for chrome reactivation.
        /// </summary>
        public static bool IsEdgeTap(float tapY, int screenHeight, string chromePosition)
        {
            const float EDGE_ZONE = 20f;
            if (chromePosition == "top" && tapY < EDGE_ZONE) return true;
            if (chromePosition == "bottom" && tapY > (screenHeight - EDGE_ZONE)) return true;
            return false;
        }

        /// <summary>
        /// Formats a JavaScript call to handleEdgeTap with the given coordinates.
        /// </summary>
        public static string FormatEdgeTapMessage(int tapY, int screenHeight)
        {
            return $"window.tabUI?.handleEdgeTap({tapY}, {screenHeight});";
        }

        /// <summary>
        /// Format JavaScript call to set mobile tab limit.
        /// </summary>
        public static string FormatSetMobileTabLimitMessage(int limit)
        {
            return $"window.tabUI?.setMobileTabLimit({limit});";
        }

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

        /// <summary>
        /// The root transform of the chrome WebView panel.
        /// Used to parent content WebViews so they move with the chrome in VR.
        /// </summary>
        public Transform ChromeTransform => webViewObject != null ? webViewObject.transform : null;

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

            // Subscribe to memory pressure events
            Application.lowMemory += HandleMemoryPressure;

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
                if (isVR) inputFilter.vrMode = true;
                if (isMobile)
                {
                    inputFilter.chromeAtBottom = true;
                    // 10% of screen height covers the bar (7vh) plus margins
                    inputFilter.chromeHeight = Screen.height * 0.10f;
                }
            }

            // Wire any keyboard in the prefab to send input to this WebView
            var keyboard = webViewObject.GetComponentInChildren<FiveSQD.WebVerse.Input.Keyboard.Keyboard>(true);
            if (keyboard != null)
            {
                keyboard.webViewTarget = webView;
                Logging.Log("[TabUIController] Keyboard wired to chrome WebView.");
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

            // Cache-bust: append timestamp so Vuplex reloads fresh content
            tabUIPath += "?t=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

                // XRUIInputModule requires TrackedDeviceGraphicRaycaster for VR pointer interaction.
                // Destroy the standard GraphicRaycaster — just disabling it can cause conflicts.
                var graphicRaycaster = webViewObject.GetComponent<GraphicRaycaster>();
#if VUPLEX_XR_INTERACTION_TOOLKIT
                // Must check type exactly so we don't destroy the TrackedDeviceGraphicRaycaster
                if (graphicRaycaster != null && graphicRaycaster.GetType() == typeof(GraphicRaycaster))
                    Destroy(graphicRaycaster);
                if (webViewObject.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                    webViewObject.AddComponent<TrackedDeviceGraphicRaycaster>();
#endif
                // Disable CanvasScaler — it interferes with WorldSpace canvas raycasting
                var canvasScaler = webViewObject.GetComponent<CanvasScaler>();
                if (canvasScaler != null) canvasScaler.enabled = false;
            }

            // Do NOT parent to VR rig — we position it manually on toggle
            // so it spawns in front of the user like the legacy Multibar
            webViewObject.transform.SetParent(null);

            // Start hidden — user toggles it on with the menu button
            webViewObject.SetActive(false);
            chromeVisible = false;

            // In VR, allow all raycasts (screen-space chrome bar filter doesn't apply)
            if (inputFilter != null) inputFilter.vrMode = true;

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
            try
            {
                Canvas canvas = webViewObject?.GetComponent<Canvas>();
                string worldCam = "NULL";
                try { worldCam = canvas?.worldCamera != null ? canvas.worldCamera.name : "NULL"; } catch { worldCam = "ERROR"; }
                Logging.Log($"[TabUIController VR Diag] Canvas={canvas != null} renderMode={canvas?.renderMode} worldCamera={worldCam}");

                var gr = webViewObject?.GetComponent<GraphicRaycaster>();
                Logging.Log($"[TabUIController VR Diag] GraphicRaycaster={(gr != null ? $"enabled={gr.enabled}" : "none")}");

#if VUPLEX_XR_INTERACTION_TOOLKIT
                var tdgr = webViewObject?.GetComponent<TrackedDeviceGraphicRaycaster>();
                Logging.Log($"[TabUIController VR Diag] TrackedDeviceGraphicRaycaster={(tdgr != null ? $"enabled={tdgr.enabled}" : "MISSING")}");
#else
                Logging.Log("[TabUIController VR Diag] VUPLEX_XR_INTERACTION_TOOLKIT is NOT defined");
#endif

                var es = EventSystem.current;
                Logging.Log($"[TabUIController VR Diag] EventSystem inputModule={es?.currentInputModule?.GetType().FullName ?? "NULL"}");

                // Layer check
                Logging.Log($"[TabUIController VR Diag] webViewObject layer={webViewObject.layer} ({LayerMask.LayerToName(webViewObject.layer)})");

                // All registered raycasters in scene
                var allRaycasters = FindObjectsOfType<UnityEngine.EventSystems.BaseRaycaster>();
                Logging.Log($"[TabUIController VR Diag] Active BaseRaycasters in scene: {allRaycasters.Length}");
                foreach (var rc in allRaycasters)
                {
                    Logging.Log($"[TabUIController VR Diag]   Raycaster: {rc.gameObject.name} type={rc.GetType().Name} enabled={rc.enabled} active={rc.gameObject.activeInHierarchy}");
                }

                // Check XRInteractionManager and registered interactors
                var xrManager = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
                if (xrManager != null)
                {
                    var interactors = new List<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor>();
                    xrManager.GetRegisteredInteractors(interactors);
                    Logging.Log($"[TabUIController VR Diag] XRInteractionManager registered interactors: {interactors.Count}");
                    foreach (var interactor in interactors)
                    {
                        var mb = interactor as MonoBehaviour;
                        Logging.Log($"[TabUIController VR Diag]   Interactor: {mb?.gameObject.name ?? "?"} type={interactor.GetType().Name} enabled={mb?.enabled} active={mb?.gameObject.activeInHierarchy}");
                    }
                }
                else
                {
                    Logging.LogWarning("[TabUIController VR Diag] No XRInteractionManager found in scene");
                }

#if VUPLEX_XR_INTERACTION_TOOLKIT
                // NearFarInteractor-specific diagnostics
                var nearFar = FindObjectOfType<NearFarInteractor>();
                if (nearFar != null)
                {
                    Logging.Log($"[TabUIController VR Diag] NearFarInteractor: enableFarCasting={nearFar.enableFarCasting}, enableUIInteraction={nearFar.enableUIInteraction}, isActiveAndEnabled={nearFar.isActiveAndEnabled}, hasSelection={nearFar.hasSelection}");

                    // Check if registered with XRUIInputModule
                    var uiModule = es?.currentInputModule as XRUIInputModule;
                    if (uiModule != null)
                    {
                        TrackedDeviceModel uiModel;
                        bool hasModel = uiModule.GetTrackedDeviceModel(nearFar, out uiModel);
                        Logging.Log($"[TabUIController VR Diag] NearFarInteractor registered with XRUIInputModule: {hasModel}");
                    }

                    // Check TryGetUIModel and its data
                    TrackedDeviceModel nfModel;
                    bool gotModel = nearFar.TryGetUIModel(out nfModel);
                    Logging.Log($"[TabUIController VR Diag] NearFarInteractor.TryGetUIModel={gotModel}, raycastPoints={nfModel.raycastPoints?.Count ?? 0}");
                }
                else
                {
                    Logging.LogWarning("[TabUIController VR Diag] No NearFarInteractor found in scene");
                }
#endif

                // Check GraphicRegistry for this canvas
                try
                {
                    var canvasForGraphics = webViewObject?.GetComponent<Canvas>();
                    Logging.Log($"[TabUIController VR Diag] canvasForGraphics={(canvasForGraphics != null ? "found" : "null")}");
                    if (canvasForGraphics != null)
                    {
                        var registeredGraphics = GraphicRegistry.GetGraphicsForCanvas(canvasForGraphics);
                        Logging.Log($"[TabUIController VR Diag] GraphicRegistry graphics for canvas: {registeredGraphics.Count}");
                        for (int gi = 0; gi < registeredGraphics.Count; gi++)
                        {
                            var g = registeredGraphics[gi];
                            Logging.Log($"[TabUIController VR Diag]   RegGraphic: {g.gameObject.name} depth={g.depth} raycastTarget={g.raycastTarget} cull={g.canvasRenderer.cull} layer={g.gameObject.layer}");

                            // Check CanvasGroup hierarchy
                            var current = g.transform;
                            while (current != null)
                            {
                                var cg = current.GetComponent<CanvasGroup>();
                                if (cg != null)
                                {
                                    Logging.Log($"[TabUIController VR Diag]   CanvasGroup on '{current.name}': blocksRaycasts={cg.blocksRaycasts}, interactable={cg.interactable}, alpha={cg.alpha}, ignoreParent={cg.ignoreParentGroups}");
                                }
                                current = current.parent;
                            }

                            // Manual raycast test using eventCamera
                            var evtCam = canvasForGraphics.worldCamera ?? Camera.main;
                            var rt = g.rectTransform;
                            var corners = new Vector3[4];
                            rt.GetWorldCorners(corners);
                            var center = (corners[0] + corners[2]) * 0.5f;
                            var screenPt = evtCam.WorldToScreenPoint(center);
                            bool containsPoint = RectTransformUtility.RectangleContainsScreenPoint(rt, screenPt, evtCam);
                            Logging.Log($"[TabUIController VR Diag]   RectContainsScreenPoint(center)={containsPoint}, screenPt={screenPt}, worldCenter={center}");

                            // Check ICanvasRaycastFilter implementations
                            var filterCheck = g.transform;
                            while (filterCheck != null)
                            {
                                var filters = filterCheck.GetComponents<ICanvasRaycastFilter>();
                                foreach (var f in filters)
                                {
                                    bool valid = f.IsRaycastLocationValid(screenPt, evtCam);
                                    Logging.Log($"[TabUIController VR Diag]   RaycastFilter on '{filterCheck.name}': type={f.GetType().Name} valid={valid}");
                                }
                                filterCheck = filterCheck.parent;
                            }

                            // Call graphic.Raycast directly
                            bool graphicRaycastResult = g.Raycast(screenPt, evtCam);
                            Logging.Log($"[TabUIController VR Diag]   graphic.Raycast={graphicRaycastResult}");
                        }
                    }
                }
                catch (Exception grEx)
                {
                    Logging.LogError($"[TabUIController VR Diag] GraphicRegistry check exception: {grEx}");
                }

                var graphics = webViewObject?.GetComponentsInChildren<Graphic>(true);
                Logging.Log($"[TabUIController VR Diag] Graphic children count={graphics?.Length ?? 0}");
                if (graphics != null)
                {
                    foreach (var g in graphics)
                    {
                        Logging.Log($"[TabUIController VR Diag]   Graphic: {g.gameObject.name} type={g.GetType().Name} raycastTarget={g.raycastTarget} active={g.gameObject.activeInHierarchy} layer={g.gameObject.layer}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[TabUIController VR Diag] Exception: {ex}");
            }
        }

        private void _LogHierarchy(Transform t, int depth)
        {
            string indent = new string(' ', depth * 2);
            var components = t.GetComponents<Component>();
            string compNames = string.Join(", ", System.Array.ConvertAll(components, c => c?.GetType().Name ?? "null"));
            Logging.Log($"[TabUIController VR Hierarchy] {indent}{t.name} [{compNames}] active={t.gameObject.activeSelf}");
            for (int i = 0; i < t.childCount; i++)
            {
                _LogHierarchy(t.GetChild(i), depth + 1);
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

                case "urlBarFocused":
                    HandleUrlBarFocused();
                    break;

                case "overlayOpened":
                    if (inputFilter != null) inputFilter.allowFullScreenInput = true;
                    break;

                case "overlayClosed":
                    if (inputFilter != null) inputFilter.allowFullScreenInput = false;
                    break;

                case "acceptSessionRestore":
                    HandleRestoreSessionAccepted();
                    break;

                case "declineSessionRestore":
                    TabSessionSerializer.ClearSession();
                    Logging.Log("[TabUIController] Saved session cleared by user.");
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

            // Send initial state — priority: mobile/tablet > vr > desktop
            SendModeToWebView(GetModeString());
            if (isMobile) InjectMobileStyles();
            SendSafeAreaToWebView();
            SendChromePositionToWebView();
            SyncAllTabsToWebView();
            UpdateNavStateInWebView();

            // Send any pending messages
            while (pendingMessages.Count > 0)
            {
                ExecuteJavaScript(pendingMessages.Dequeue());
            }

            // Cache initial orientation, safe area, and keyboard state for change detection
            cachedOrientation = Screen.orientation;
            cachedSafeArea = Screen.safeArea;
            cachedKeyboardVisible = false;
            cachedKeyboardHeight = 0;
            SendOrientationToWebView(cachedOrientation);

            Logging.Log("[TabUIController] UI ready, initial state synced.");

            // Check for saved session from force-kill (AC4)
            if (TabSessionSerializer.HasSavedSession())
            {
                Logging.Log("[TabUIController] Saved session detected, showing restore prompt.");
                string js = "window.tabUI?.showRestorePrompt();";
                ExecuteJavaScript(js);
            }
        }

        /// <summary>
        /// Handle user accepting session restore from prompt.
        /// </summary>
        private void HandleRestoreSessionAccepted()
        {
            if (!TabSessionSerializer.HasSavedSession()) return;

            var session = TabSessionSerializer.LoadSession();
            if (session.tabs == null || session.tabs.Count == 0)
            {
                TabSessionSerializer.ClearSession();
                return;
            }

            // Recreate tabs via TabManager
            foreach (var entry in session.tabs)
            {
                var tab = tabManager?.CreateTab(entry.url, makeActive: false);
                if (tab != null && !string.IsNullOrEmpty(entry.displayName))
                {
                    tab.DisplayName = entry.displayName;
                }
            }

            // Switch to the previously active tab
            if (!string.IsNullOrEmpty(session.activeTabId))
            {
                // Find the restored tab by URL match (IDs are regenerated)
                var activeEntry = session.tabs.Find(t => t.id == session.activeTabId);
                if (activeEntry != null)
                {
                    var matchingTabs = tabManager?.FindTabsByUrl(activeEntry.url);
                    var firstMatch = matchingTabs?.FirstOrDefault();
                    if (firstMatch != null)
                    {
                        tabManager?.SwitchToTab(firstMatch.Id);
                    }
                }
            }

            TabSessionSerializer.ClearSession();
            Logging.Log("[TabUIController] Session restored from saved state.");
        }

        /// <summary>
        /// Handle URL bar focused — ensure the Chrome WebView has keyboard focus
        /// so hardware keyboard input reaches the Chromium input element.
        /// </summary>
        private void HandleUrlBarFocused()
        {
#if VUPLEX_INCLUDED
            webView?.SetFocused(true);
            bool simActive = false;
#if UNITY_EDITOR
            simActive = UnityEngine.Device.SystemInfo.deviceType != SystemInfo.deviceType;
#endif
            Logging.Log($"[TabUIController] URL bar focused, SetFocused(true) called. " +
                $"Screen=({Screen.width}x{Screen.height}), " +
                $"simActive={simActive}, " +
                $"touchSupported={UnityEngine.Input.touchSupported}, " +
                $"mousePresent={UnityEngine.Input.mousePresent}");
            StartInputDiag();
#endif
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

            // Update URL bar immediately so user sees the navigated URL
            SendUrlToWebView(url);
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
            SendUrlToWebView(prevUrl);
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
            SendUrlToWebView(nextUrl);
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

            // If switched-to tab was evicted (Suspended), show toast — reload is
            // handled by TabManager.SwitchToTab which initiates the load sequence.
            if (tab != null && tab.LoadState == TabLoadState.Suspended)
            {
                ExecuteJavaScript("window.tabUI?.showReloadingToast();");
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
        /// Inject mobile-optimized CSS directly via JS to bypass Vuplex cache.
        /// Uses viewport-relative units so the bar scales correctly regardless of DPR.
        /// </summary>
        private void InjectMobileStyles()
        {
            // Build CSS using max() so vh units scale up on real phones but never
            // shrink below default px sizes in the Editor mobile simulator.
            string css = ".mobile-mode{" +
                "--bar-height:max(7vh,80px)!important;" +
                "--bar-padding-h:max(1.5vw,12px)!important;" +
                "--bar-padding-v:max(1vh,8px)!important;" +
                "--bar-radius:max(3.5vh,40px)!important;" +
                "--tabs-button-size:max(6.5vh,64px)!important;" +
                "--nav-btn-size:max(4.5vh,48px)!important;" +
                "--tab-icon-size:max(3vh,32px)!important;" +
                "--font-size-md:max(1.6vh,16px)!important;" +
                "--font-size-lg:max(1.8vh,18px)!important;" +
                "--spacing-md:max(0.8vh,8px)!important;" +
                "--touch-target-min:max(4.5vh,48px)!important;" +
                "--gap-button-bar:max(0.5vh,4px)!important;" +
                "}" +
                ".mobile-mode .chrome{left:2vw!important;right:2vw!important;}" +
                ".mobile-mode #btn-fullscreen,.mobile-mode #btn-vr,.mobile-mode .nav-btn-wrapper{display:none!important;}" +
                ".mobile-mode .url-bar-container{min-width:0!important;}" +
                ".mobile-mode .nav-btn svg{width:max(3vh,36px)!important;height:max(3vh,36px)!important;}" +
                ".mobile-mode .tabs-button__icon svg{width:max(3.5vh,40px)!important;height:max(3.5vh,40px)!important;}" +
                ".mobile-mode .url-bar{font-size:max(1.6vh,16px)!important;height:max(5vh,44px)!important;border-radius:max(2.5vh,22px)!important;padding:0 max(1.5vw,12px)!important;}";

            string js = "var s=document.createElement('style');s.textContent='" + css.Replace("'", "\\'") + "';document.head.appendChild(s);";
            ExecuteJavaScript(js);
            Logging.Log("[TabUIController] Injected mobile styles via JS.");
        }

        /// <summary>
        /// Send safe area insets to WebView as CSS custom properties.
        /// </summary>
        private void SendSafeAreaToWebView()
        {
            if (!isMobile) return;
            var insets = GetSafeAreaInsets(Screen.safeArea, Screen.width, Screen.height);
            string js = $"window.tabUI?.setSafeArea({{ top: {(int)insets.top}, bottom: {(int)insets.bottom}, left: {(int)insets.left}, right: {(int)insets.right} }});";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Send chrome position preference to WebView.
        /// </summary>
        private void SendChromePositionToWebView()
        {
            if (!isMobile) return;
            string js = $"window.tabUI?.setChromePosition('{EscapeJs(ChromePosition)}');";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Send orientation string to WebView.
        /// </summary>
        private void SendOrientationToWebView(ScreenOrientation orientation)
        {
            if (!isMobile) return;
            string orient = (orientation == ScreenOrientation.LandscapeLeft ||
                             orientation == ScreenOrientation.LandscapeRight)
                ? "landscape" : "portrait";
            string js = $"window.tabUI?.setOrientation('{orient}');";
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Check for orientation and safe area changes each frame (mobile only).
        /// </summary>
        private void CheckOrientationAndSafeArea()
        {
            if (!isMobile || !webViewReady) return;

            var currentOrientation = Screen.orientation;
            var currentSafeArea = Screen.safeArea;

            if (HasOrientationChanged(cachedOrientation, currentOrientation))
            {
                cachedOrientation = currentOrientation;
                SendOrientationToWebView(currentOrientation);
            }

            if (cachedSafeArea != currentSafeArea)
            {
                cachedSafeArea = currentSafeArea;
                SendSafeAreaToWebView();
            }
        }

        /// <summary>
        /// Send keyboard state to WebView.
        /// </summary>
        private void SendKeyboardStateToWebView(bool visible, int height)
        {
            if (!isMobile) return;
            string js = FormatKeyboardStateMessage(visible, height);
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Sends startAutoHide message to the chrome WebView.
        /// Call when world interaction begins (user touches 3D content).
        /// </summary>
        public void SendStartAutoHide()
        {
            if (!isMobile || !webViewReady) return;
            ExecuteJavaScript("window.tabUI?.startAutoHideTimer();");
        }

        /// <summary>
        /// Sends stopAutoHide message to the chrome WebView.
        /// Call when chrome interaction resumes.
        /// </summary>
        public void SendStopAutoHide()
        {
            if (!isMobile || !webViewReady) return;
            ExecuteJavaScript("window.tabUI?.stopAutoHideTimer();");
        }

        /// <summary>
        /// Sends edge tap message to the chrome WebView for chrome reactivation.
        /// </summary>
        public void SendEdgeTap(int tapY, int screenHeight)
        {
            if (!isMobile || !webViewReady) return;
            string js = FormatEdgeTapMessage(tapY, screenHeight);
            ExecuteJavaScript(js);
        }

        /// <summary>
        /// Send platform back event to the Chrome WebView for mobile back button handling.
        /// </summary>
        public void SendPlatformBack()
        {
            if (!isMobile || !webViewReady) return;
            ExecuteJavaScript("window.tabUI?.handlePlatformBack();");
        }

        /// <summary>
        /// Check for keyboard visibility and height changes each frame (mobile only).
        /// </summary>
        private void CheckKeyboardState()
        {
            if (!isMobile || !webViewReady) return;

            bool currentVisible = TouchScreenKeyboard.visible;
            int currentHeight = 0;
            if (currentVisible)
            {
                currentHeight = GetKeyboardHeight(TouchScreenKeyboard.area);
            }

            if (currentVisible != cachedKeyboardVisible || currentHeight != cachedKeyboardHeight)
            {
                cachedKeyboardVisible = currentVisible;
                cachedKeyboardHeight = currentHeight;
                SendKeyboardStateToWebView(currentVisible, currentHeight);
            }
        }

        private int inputDiagCounter;
        private bool inputDiagEnabled;

        /// <summary>
        /// Call from HandleUrlBarFocused to start capturing input diagnostics for a few seconds.
        /// </summary>
        private void StartInputDiag()
        {
            inputDiagEnabled = true;
            inputDiagCounter = 300; // ~5 seconds at 60fps
        }

        private void Update()
        {
            if (isMobile && webViewReady)
            {
                CheckOrientationAndSafeArea();
                CheckKeyboardState();
            }

            // Temporary input diagnostic — log every frame that has keyboard input
            if (inputDiagEnabled && inputDiagCounter > 0)
            {
                inputDiagCounter--;
                if (UnityEngine.Input.anyKeyDown || UnityEngine.Input.inputString.Length > 0)
                {
                    Logging.Log($"[TabUI InputDiag] inputString=\"{UnityEngine.Input.inputString}\" anyKeyDown={UnityEngine.Input.anyKeyDown} mouseBtn0={UnityEngine.Input.GetMouseButtonDown(0)}");
                }
                if (inputDiagCounter == 0)
                {
                    inputDiagEnabled = false;
                    Logging.Log("[TabUI InputDiag] Diagnostic window ended.");
                }
            }
        }

        /// <summary>
        /// Handle app pause/resume lifecycle for mobile session persistence.
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!isInitialized || tabManager == null) return;

            if (pauseStatus)
            {
                // App backgrounded — serialize and persist tab state
                var entries = tabManager.Tabs.Select(t => new TabSessionSerializer.TabEntry
                {
                    id = t.Id,
                    url = t.WorldUrl,
                    displayName = t.GetDisplayName(),
                    lastActiveAt = t.LastActiveAt.ToString("o")
                }).ToList();

                string chromePos = PlayerPrefs.GetString("TabUI_ChromePosition", "bottom");
                var sessionData = new TabSessionSerializer.SessionData
                {
                    tabs = entries,
                    activeTabId = tabManager.ActiveTab?.Id,
                    chromePosition = chromePos,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                TabSessionSerializer.SaveSession(sessionData);

                Logging.Log("[TabUIController] Session saved on pause.");
            }
            else
            {
                // App foregrounded — check if world is still in memory
                if (tabManager.ActiveTab != null && tabManager.ActiveTab.LoadState == TabLoadState.Suspended)
                {
                    // World was reclaimed — reload from stored URL
                    Logging.Log("[TabUIController] Active tab suspended, reloading from URL.");
                    ExecuteJavaScript("window.tabUI?.showReloadingToast();");

                    tabManager.ActiveTab.LoadState = TabLoadState.Loading;
                    tabManager.NotifyTabStateChanged(tabManager.ActiveTab);

                    // Trigger reload of the active tab
                    if (!string.IsNullOrEmpty(tabManager.ActiveTab.WorldUrl))
                    {
                        OnNavigateRequested?.Invoke(tabManager.ActiveTab.WorldUrl);
                    }
                }
                // If world is still in memory, no action needed (AC2)
            }
        }

        /// <summary>
        /// Handle OS memory pressure by evicting the least-recently-used background tab.
        /// </summary>
        private void HandleMemoryPressure()
        {
            if (!isInitialized || tabManager == null) return;

            var evictIds = MemoryPressureHandler.EvaluateEviction(
                tabManager.Tabs, tabManager.ActiveTab?.Id);

            foreach (var tabId in evictIds)
            {
                var tab = tabManager.Tabs.FirstOrDefault(t => t.Id == tabId);
                if (tab != null)
                {
                    MemoryPressureHandler.ExecuteEviction(tab);
                    tabManager.NotifyTabStateChanged(tab);
                }
            }

            if (evictIds.Count > 0)
            {
                Logging.Log($"[TabUIController] Memory pressure: evicted {evictIds.Count} background tab(s)");
            }
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

            // Force canvas rebuild so graphics get valid depth for raycast detection
            Canvas.ForceUpdateCanvases();

            // In VR, reposition in front of the user each time
            if (isVR)
            {
                PositionVRChromeInFrontOfUser();
                _LogVRSetupDiagnostics();
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
            Application.lowMemory -= HandleMemoryPressure;
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

    /// <summary>
    /// Evaluates the correct back navigation action based on current state and platform.
    /// </summary>
    public static class MobileBackHandler
    {
        public enum BackAction
        {
            None,
            NavigateBack,
            HideChrome,
            ShowExitDialog,
            CloseOverlay
        }

        /// <summary>
        /// Pure function: determines the back action based on current state.
        /// Priority: close overlay → navigate back → hide chrome → exit dialog (Android) / none (iOS).
        /// </summary>
        public static BackAction EvaluateBackAction(bool hasHistory, bool chromeVisible, bool hasOverlay, string platform)
        {
            if (platform != "android" && platform != "ios")
                return BackAction.None;

            if (hasOverlay)
                return BackAction.CloseOverlay;

            if (hasHistory)
                return BackAction.NavigateBack;

            if (chromeVisible)
                return BackAction.HideChrome;

            if (platform == "android")
                return BackAction.ShowExitDialog;

            return BackAction.None;
        }
    }

    /// <summary>
    /// Pure helper for mobile tab limit logic.
    /// </summary>
    public static class MobileTabLimitHandler
    {
        /// <summary>
        /// Returns true if a new tab should be blocked (at or over limit).
        /// </summary>
        public static bool ShouldBlockNewTab(int currentCount, int limit)
        {
            return currentCount >= limit;
        }
    }

    /// <summary>
    /// Serializes and persists tab session state for background/foreground and force-kill recovery.
    /// </summary>
    public static class TabSessionSerializer
    {
        private const string PlayerPrefsKey = "TabUI_Session";

        public class TabEntry
        {
            public string id;
            public string url;
            public string displayName;
            public string lastActiveAt;
        }

        public class SessionData
        {
            public List<TabEntry> tabs = new List<TabEntry>();
            public string activeTabId;
            public string chromePosition;
            public string timestamp;
        }

        /// <summary>
        /// Serializes tab data into a JSON string with metadata.
        /// </summary>
        public static string Serialize(List<TabEntry> tabs, string activeTabId, string chromePosition)
        {
            var data = new SessionData
            {
                tabs = tabs ?? new List<TabEntry>(),
                activeTabId = activeTabId,
                chromePosition = chromePosition,
                timestamp = DateTime.UtcNow.ToString("o")
            };
            return JsonConvert.SerializeObject(data);
        }

        /// <summary>
        /// Deserializes a JSON string into SessionData. Returns empty session on null, empty, or malformed input.
        /// </summary>
        public static SessionData Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new SessionData();

            try
            {
                var data = JsonConvert.DeserializeObject<SessionData>(json);
                if (data == null)
                    return new SessionData();
                if (data.tabs == null)
                    data.tabs = new List<TabEntry>();
                return data;
            }
            catch
            {
                return new SessionData();
            }
        }

        /// <summary>
        /// Persists session data to PlayerPrefs.
        /// </summary>
        public static void SaveSession(SessionData data)
        {
            string json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads session data from PlayerPrefs. Returns empty session if no data exists.
        /// </summary>
        public static SessionData LoadSession()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
                return new SessionData();

            string json = PlayerPrefs.GetString(PlayerPrefsKey);
            return Deserialize(json);
        }

        /// <summary>
        /// Returns true if a saved session exists in PlayerPrefs.
        /// </summary>
        public static bool HasSavedSession()
        {
            return PlayerPrefs.HasKey(PlayerPrefsKey);
        }

        /// <summary>
        /// Deletes the saved session from PlayerPrefs.
        /// </summary>
        public static void ClearSession()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Evaluates and executes memory pressure eviction for background tabs using LRU ordering.
    /// </summary>
    public static class MemoryPressureHandler
    {
        /// <summary>
        /// Evaluates which background tabs should be evicted under memory pressure.
        /// Returns tab IDs in LRU order (oldest LastActiveAt first).
        /// Excludes the active tab, already-Suspended tabs, and non-Loaded tabs.
        /// </summary>
        public static List<string> EvaluateEviction(IReadOnlyList<TabState> tabs, string activeTabId, int count = 1)
        {
            if (tabs == null || tabs.Count == 0)
                return new List<string>();

            return tabs
                .Where(t => t.Id != activeTabId && t.LoadState == TabLoadState.Loaded)
                .OrderBy(t => t.LastActiveAt)
                .Take(count)
                .Select(t => t.Id)
                .ToList();
        }

        /// <summary>
        /// Executes eviction on a single tab by setting its LoadState to Suspended.
        /// Preserves Id, WorldUrl, and DisplayName.
        /// </summary>
        public static void ExecuteEviction(TabState tab)
        {
            if (tab == null) return;
            tab.LoadState = TabLoadState.Suspended;
        }
    }

    /// <summary>
    /// Pure helper for gesture conflict detection.
    /// </summary>
    public static class GestureConflictHandler
    {
        private const int EdgeZone = 20;

        /// <summary>
        /// Returns true if the swipe starts within the edge zone (reserved for iOS system gestures).
        /// </summary>
        public static bool ShouldSuppressSwipe(int startX, int screenWidth)
        {
            return startX < EdgeZone || startX > (screenWidth - EdgeZone);
        }
    }
}
