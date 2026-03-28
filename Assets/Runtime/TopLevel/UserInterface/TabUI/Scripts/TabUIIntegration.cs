// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using FiveSQD.StraightFour.WorldState;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;
using UnityEngine.Profiling;

namespace FiveSQD.WebVerse.Interface.TabUI
{
    /// <summary>
    /// Integration component that wires TabUI with WebVerseRuntime.
    /// Add this to your scene alongside DesktopMode to enable tabbed browsing.
    /// </summary>
    public class TabUIIntegration : MonoBehaviour
    {
        /// <summary>
        /// FPS history for the stats HUD graph.
        /// </summary>
        private const int FpsHistorySize = 60;
        private readonly List<float> fpsHistory = new List<float>();
        private float fpsAccumulator;
        private int fpsFrameCount;
        private float fpsUpdateInterval = 0.5f;
        private float fpsNextUpdate;
        #region Serialized Fields

        [Header("Required References")]

        /// <summary>
        /// Reference to the WebVerse Runtime.
        /// </summary>
        [SerializeField]
        [Tooltip("Reference to the WebVerse Runtime.")]
        private Runtime.WebVerseRuntime runtime;

        /// <summary>
        /// Prefab for the Tab UI WebView (should contain CanvasWebViewPrefab).
        /// </summary>
        [SerializeField]
        [Tooltip("Prefab for the Tab UI WebView.")]
        private GameObject tabUIWebViewPrefab;

        [Header("VR Configuration")]

        /// <summary>
        /// Parent transform for VR mode Tab UI.
        /// </summary>
        [SerializeField]
        [Tooltip("Parent transform for VR mode Tab UI.")]
        private Transform vrParent;

        [Header("Configuration")]

        /// <summary>
        /// Maximum number of tabs allowed.
        /// </summary>
        [SerializeField]
        [Tooltip("Maximum number of tabs allowed.")]
        private int maxTabs = 10;

        /// <summary>
        /// Maximum snapshot memory in MB.
        /// </summary>
        [SerializeField]
        [Tooltip("Maximum memory for world state snapshots in MB.")]
        private int maxSnapshotMemoryMB = 100;

        /// <summary>
        /// Home URL to load on startup.
        /// </summary>
        [SerializeField]
        [Tooltip("Home URL to load on startup.")]
        private string homeUrl;

        #endregion

        #region Private Fields

        private WorldStateManager worldStateManager;
        private TabManager tabManager;
        private TabUIController desktopTabUIController;
        private TabUIController vrTabUIController;
        private TabUIInputHandler inputHandler;
        private bool isVRMode;

        // Data providers — set by DesktopMode to supply data from NativeHistory/NativeSettings
        private Func<object> historyProvider;
        private Func<object> consoleLogProvider;
        private Func<object> settingsProvider;

        // Logging callback reference for cleanup
        private Action<string, Logging.Type> logCallback;

        // Re-entrancy guard for log forwarding
        private bool isForwardingLog;

        // Buffered console log entries
        private static readonly List<Dictionary<string, string>> consoleLogBuffer = new List<Dictionary<string, string>>();
        private const int MaxConsoleLogEntries = 500;


        #endregion

        #region Events

        /// <summary>
        /// Fired when the user requests VR mode toggle from the Chrome WebView.
        /// DesktopMode should subscribe to handle the actual VR toggle.
        /// </summary>
        public event Action OnVRModeToggleRequested;

        /// <summary>
        /// Fired when the user requests to clear browsing history.
        /// </summary>
        public event Action OnClearHistoryRequested;

        /// <summary>
        /// Fired when the user saves settings from the Chrome WebView.
        /// </summary>
        public event Action<Dictionary<string, object>> OnSaveSettingsRequested;

        /// <summary>
        /// Fired when the user requests to clear cache from the Chrome WebView.
        /// </summary>
        public event Action<string> OnClearCacheRequested;

        /// <summary>
        /// Fired when the user confirms exit from the Chrome WebView.
        /// </summary>
        public event Action OnExitRequested;

        /// <summary>
        /// Fired when a page finishes loading. Parameters: siteName, url.
        /// DesktopMode subscribes to record browsing history.
        /// </summary>
        public event Action<string, string> OnPageLoaded;

        #endregion

        #region Public Properties

        /// <summary>
        /// The Tab Manager instance.
        /// </summary>
        public TabManager TabManager => tabManager;

        /// <summary>
        /// The World State Manager instance.
        /// </summary>
        public WorldStateManager WorldStateManager => worldStateManager;

        /// <summary>
        /// The active Tab UI Controller.
        /// </summary>
        public TabUIController ActiveTabUIController => isVRMode ? vrTabUIController : desktopTabUIController;

        /// <summary>
        /// Whether the Tab UI has been initialized successfully.
        /// </summary>
        public bool IsInitialized => tabManager != null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the home URL before initialization. Called by DesktopMode
        /// to pass the home URL from settings.
        /// </summary>
        /// <param name="url">The home URL to load on startup.</param>
        public void SetHomeUrl(string url)
        {
            homeUrl = url;
        }

        /// <summary>
        /// Set the data provider for browsing history.
        /// The provider should return a list of objects with { name, url, timestamp } fields.
        /// Called by DesktopMode to wire NativeHistory.
        /// </summary>
        public void SetHistoryProvider(Func<object> provider)
        {
            historyProvider = provider;
        }

        /// <summary>
        /// Set the data provider for console log entries.
        /// The provider should return a list of objects with { type, message, timestamp } fields.
        /// </summary>
        public void SetConsoleLogProvider(Func<object> provider)
        {
            consoleLogProvider = provider;
        }

        /// <summary>
        /// Set the data provider for settings.
        /// The provider should return an object with settings key/value pairs.
        /// Called by DesktopMode to wire NativeSettings.
        /// </summary>
        public void SetSettingsProvider(Func<object> provider)
        {
            settingsProvider = provider;
        }

        /// <summary>
        /// Forward a console log line to the Chrome WebView.
        /// Call this from runtime log handlers.
        /// </summary>
        public void AddConsoleLine(string type, string message)
        {
            ActiveTabUIController?.SendConsoleLine(type, message);
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            StartCoroutine(InitializeAfterRuntime());
        }

        private IEnumerator InitializeAfterRuntime()
        {
            // Wait for runtime to initialize
            while (Runtime.WebVerseRuntime.Instance == null)
            {
                yield return null;
            }

            Initialize();
        }

        private void Update()
        {
            // Track FPS for history graph
            fpsAccumulator += Time.unscaledDeltaTime;
            fpsFrameCount++;
            if (Time.unscaledTime >= fpsNextUpdate)
            {
                float avgFps = fpsFrameCount / fpsAccumulator;
                fpsHistory.Add(Mathf.Round(avgFps));
                if (fpsHistory.Count > FpsHistorySize)
                    fpsHistory.RemoveAt(0);
                fpsAccumulator = 0;
                fpsFrameCount = 0;
                fpsNextUpdate = Time.unscaledTime + fpsUpdateInterval;
            }
        }

        private void OnDestroy()
        {
            Terminate();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the Tab UI integration.
        /// </summary>
        public void Initialize()
        {
            if (runtime == null)
            {
                runtime = Runtime.WebVerseRuntime.Instance;
            }

            if (runtime == null)
            {
                Logging.LogError("[TabUIIntegration->Initialize] WebVerseRuntime not found.");
                return;
            }

            InitializeWorldStateManager();
            InitializeTabManager();
            InitializeTabUIControllers();
            InitializeInputHandler();

            // Subscribe to WebVerseWebView events
            if (runtime.webverseWebView != null)
            {
                runtime.webverseWebView.OnVisibilityChanged += HandleWebViewVisibilityChanged;
                runtime.webverseWebView.OnTitleChanged += HandlePageTitleChanged;
            }

            // Register log callback to buffer console lines for Tab UI
            // Remove any previous callback first (static list survives domain reloads)
            if (logCallback != null)
            {
                Logging.RemoveCallback(logCallback);
            }
            logCallback = BufferLogEntry;
            Logging.RegisterCallback(logCallback);

            // Auto-wire data providers from DesktopMode if not already set
            if (historyProvider == null || settingsProvider == null)
            {
                var desktopMode = FindObjectOfType<Runtime.DesktopMode>();
                if (desktopMode != null)
                {
                    WireDesktopModeProviders(desktopMode);
                }
            }

            // Create initial tab
            if (!string.IsNullOrEmpty(homeUrl))
            {
                tabManager.CreateTab(homeUrl, null, true);
            }
            else
            {
                tabManager.CreateEmptyTab(true);
            }

            Logging.Log("[TabUIIntegration] Initialized.");
        }

        /// <summary>
        /// Initialize the World State Manager.
        /// </summary>
        private void InitializeWorldStateManager()
        {
            GameObject wsManagerGO = new GameObject("WorldStateManager");
            wsManagerGO.transform.SetParent(transform);
            worldStateManager = wsManagerGO.AddComponent<WorldStateManager>();
            worldStateManager.Initialize(1, maxSnapshotMemoryMB);
        }

        /// <summary>
        /// Initialize the Tab Manager.
        /// </summary>
        private void InitializeTabManager()
        {
            GameObject tabManagerGO = new GameObject("TabManager");
            tabManagerGO.transform.SetParent(transform);
            tabManager = tabManagerGO.AddComponent<TabManager>();
            tabManager.MaxTabs = maxTabs;

            // Set up world load callbacks
            tabManager.Initialize(
                worldStateManager,
                LoadWorldForTab,
                UnloadWorldForTab
            );

            // Handle tab switch navigation for webpage tabs
            tabManager.OnTabNavigateRequested += HandleTabNavigateRequested;

            // Handle switching to empty tabs (hide content)
            tabManager.OnActiveTabChanged += HandleActiveTabChangedForContent;

            // Hide/show chrome during thumbnail capture so it's not in the screenshot
            if (tabManager.ThumbnailService != null)
            {
                tabManager.ThumbnailService.OnBeforeCapture += HideChromeForCapture;
                tabManager.ThumbnailService.OnAfterCapture += RestoreChromeAfterCapture;
            }
        }

        /// <summary>
        /// Initialize Tab UI Controllers for desktop and VR.
        /// </summary>
        private void InitializeTabUIControllers()
        {
            // Desktop Tab UI
            GameObject desktopTabUIGO = new GameObject("DesktopTabUI");
            desktopTabUIGO.transform.SetParent(transform);
            desktopTabUIController = desktopTabUIGO.AddComponent<TabUIController>();
            desktopTabUIController.Initialize(tabManager, tabUIWebViewPrefab);

            // Wire up events
            desktopTabUIController.OnNavigateRequested += HandleNavigateRequest;
            desktopTabUIController.OnMenuAction += HandleMenuAction;
            SubscribeToControllerDataEvents(desktopTabUIController);

            // VR Tab UI (optional)
            if (vrParent != null)
            {
                GameObject vrTabUIGO = new GameObject("VRTabUI");
                vrTabUIGO.transform.SetParent(vrParent);
                vrTabUIController = vrTabUIGO.AddComponent<TabUIController>();
                vrTabUIController.IsVR = true;
                // VR controller setup is deferred until VR mode is enabled
            }
        }

        /// <summary>
        /// Initialize the input handler.
        /// </summary>
        private void InitializeInputHandler()
        {
            GameObject inputHandlerGO = new GameObject("TabUIInputHandler");
            inputHandlerGO.transform.SetParent(transform);
            inputHandler = inputHandlerGO.AddComponent<TabUIInputHandler>();
            inputHandler.Initialize(ActiveTabUIController, tabManager);

            // Wire up keyboard shortcut events
            inputHandler.OnToggleFullscreen += ToggleFullscreen;
        }

        /// <summary>
        /// Subscribe to data request events on a TabUIController.
        /// </summary>
        private void SubscribeToControllerDataEvents(TabUIController controller)
        {
            if (controller == null) return;
            controller.OnRequestHistory += HandleRequestHistory;
            controller.OnRequestConsoleLog += HandleRequestConsoleLog;
            controller.OnRequestSettings += HandleRequestSettings;
            controller.OnRequestAboutInfo += HandleRequestAboutInfo;
            controller.OnRequestStats += HandleRequestStats;
            controller.OnClearHistory += HandleClearHistory;
            controller.OnSaveSettings += HandleSaveSettings;
            controller.OnClearCache += HandleClearCache;
            controller.OnExitRequested += HandleExitRequested;
        }

        /// <summary>
        /// Unsubscribe from data request events on a TabUIController.
        /// </summary>
        private void UnsubscribeFromControllerDataEvents(TabUIController controller)
        {
            if (controller == null) return;
            controller.OnRequestHistory -= HandleRequestHistory;
            controller.OnRequestConsoleLog -= HandleRequestConsoleLog;
            controller.OnRequestSettings -= HandleRequestSettings;
            controller.OnRequestAboutInfo -= HandleRequestAboutInfo;
            controller.OnRequestStats -= HandleRequestStats;
            controller.OnClearHistory -= HandleClearHistory;
            controller.OnSaveSettings -= HandleSaveSettings;
            controller.OnClearCache -= HandleClearCache;
            controller.OnExitRequested -= HandleExitRequested;
        }

        /// <summary>
        /// Buffer a log message for the Tab UI console.
        /// Messages are sent to the UI when the console modal is opened.
        /// </summary>
        private void BufferLogEntry(string message, Logging.Type type)
        {
            if (consoleLogBuffer.Count >= MaxConsoleLogEntries)
                consoleLogBuffer.RemoveAt(0);

            consoleLogBuffer.Add(new Dictionary<string, string>
            {
                { "type", type.ToString() },
                { "message", message },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            });
        }

        /// <summary>
        /// Wire data providers and event handlers from DesktopMode.
        /// Called automatically during Initialize if providers aren't already set.
        /// </summary>
        private void WireDesktopModeProviders(Runtime.DesktopMode desktopMode)
        {
            if (historyProvider == null)
                SetHistoryProvider(() => desktopMode.GetHistoryData());
            if (consoleLogProvider == null)
                SetConsoleLogProvider(() => desktopMode.GetConsoleLogData());
            if (settingsProvider == null)
                SetSettingsProvider(() => desktopMode.GetSettingsData());

            OnClearHistoryRequested += desktopMode.HandleClearHistory;
            OnSaveSettingsRequested += desktopMode.HandleSaveSettings;
            OnClearCacheRequested += desktopMode.HandleClearCache;
            OnExitRequested += desktopMode.HandleExit;
            OnPageLoaded += desktopMode.HandlePageLoaded;

            Logging.Log("[TabUIIntegration] Auto-wired data providers and events from DesktopMode.");
        }

        #endregion

        #region Tab Manager Callbacks

        /// <summary>
        /// Load a world for a tab.
        /// </summary>
        private Coroutine LoadWorldForTab(string url, string basePath, Action<StraightFour.World.World, bool> onComplete)
        {
            return StartCoroutine(LoadWorldCoroutine(url, basePath, onComplete));
        }

        private IEnumerator LoadWorldCoroutine(string url, string basePath, Action<StraightFour.World.World, bool> onComplete)
        {
            bool loadComplete = false;
            bool loadSuccess = false;
            string loadedSiteName = null;

            runtime.LoadURL(url, (siteName) =>
            {
                loadComplete = true;
                loadSuccess = true; // Both worlds and webpages are valid loads
                loadedSiteName = siteName;

                // Detect webpage vs world
                if (StraightFour.StraightFour.ActiveWorld == null)
                {
                    // Webpage loaded — mark tab so future switches skip world pipeline
                    if (tabManager.ActiveTab != null)
                    {
                        tabManager.ActiveTab.IsWebPage = true;
                        tabManager.ActiveTab.LoadState = StraightFour.WorldState.TabLoadState.Loaded;
                    }
                }
            });

            // Wait for load to complete
            float timeout = 120f;
            float elapsed = 0f;
            while (!loadComplete && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!loadComplete)
            {
                Logging.LogWarning("[TabUIIntegration->LoadWorldCoroutine] World load timed out.");
                onComplete?.Invoke(null, false);
                yield break;
            }

            if (StraightFour.StraightFour.ActiveWorld != null)
            {
                onComplete?.Invoke(StraightFour.StraightFour.ActiveWorld, true);
                tabManager.SetCurrentWorld(StraightFour.StraightFour.ActiveWorld);
            }
            else
            {
                // Webpage — report success to avoid Error state, but no world to return
                onComplete?.Invoke(null, true);
            }

            // Record in browsing history
            if (loadSuccess)
            {
                string displayName = tabManager.ActiveTab?.DisplayName ?? loadedSiteName ?? "Web Page";
                OnPageLoaded?.Invoke(displayName, url);
            }
        }

        /// <summary>
        /// Unload a world when tab is closed or switched.
        /// </summary>
        private void UnloadWorldForTab(StraightFour.World.World world)
        {
            if (world != null && StraightFour.StraightFour.ActiveWorld == world)
            {
                runtime.UnloadWorld();
            }

        }

        #endregion

        #region Navigation

        // Content insets matching the Tab UI content frame CSS layout.
        // top = spacing-md(16) + bar-height(96) + spacing-sm(8) = 120
        // left/right/bottom = spacing-md(16)
        private const float ContentInsetTop = 120f;
        private const float ContentInsetLeft = 16f;
        private const float ContentInsetRight = 16f;
        private const float ContentInsetBottom = 16f;

        /// <summary>
        /// Handle navigation request from Tab UI.
        /// </summary>
        private void HandleNavigateRequest(string url)
        {
            if (tabManager.ActiveTab != null)
            {
                // Update the active tab's URL
                tabManager.ActiveTab.WorldUrl = url;

                // Load the URL
                runtime.LoadURL(url, (siteName) =>
                {
                    if (tabManager.ActiveTab == null) return;

                    // Update tab display name
                    if (!string.IsNullOrEmpty(siteName) && siteName != "Web Page")
                    {
                        tabManager.ActiveTab.DisplayName = siteName;
                        tabManager.ActiveTab.IsWebPage = false;
                    }
                    else
                    {
                        tabManager.ActiveTab.IsWebPage = true;
                        // Use domain name as display name for webpages
                        tabManager.ActiveTab.DisplayName = ExtractDomain(url);
                    }

                    tabManager.ActiveTab.LoadState = StraightFour.WorldState.TabLoadState.Loaded;

                    if (StraightFour.StraightFour.ActiveWorld != null)
                    {
                        tabManager.SetCurrentWorld(StraightFour.StraightFour.ActiveWorld);
                    }

                    // Notify UI of the name/state change
                    tabManager.NotifyTabStateChanged(tabManager.ActiveTab);

                    // Record in browsing history
                    OnPageLoaded?.Invoke(tabManager.ActiveTab.DisplayName, url);

                    // Schedule thumbnail captures
                    tabManager.ScheduleThumbnailCapture(tabManager.ActiveTab.Id, 2.0f);
                    tabManager.ScheduleThumbnailCapture(tabManager.ActiveTab.Id, 15.0f);
                });
            }
        }

        /// <summary>
        /// Handle tab switch navigation request (for webpage tabs that bypass the world pipeline).
        /// </summary>
        private void HandleTabNavigateRequested(string url)
        {
            runtime.LoadURL(url, (siteName) =>
            {
                if (tabManager.ActiveTab != null)
                {
                    if (!string.IsNullOrEmpty(siteName) && siteName != "Web Page")
                    {
                        tabManager.ActiveTab.DisplayName = siteName;
                        tabManager.ActiveTab.IsWebPage = false;
                    }
                    else
                    {
                        tabManager.ActiveTab.IsWebPage = true;
                        tabManager.ActiveTab.DisplayName = ExtractDomain(url);
                    }
                    tabManager.NotifyTabStateChanged(tabManager.ActiveTab);
                }
            });
        }

        /// <summary>
        /// Hide the chrome overlay for a clean thumbnail capture.
        /// </summary>
        private void HideChromeForCapture()
        {
            ActiveTabUIController?.HideOverlay();
        }

        /// <summary>
        /// Restore the chrome overlay after thumbnail capture.
        /// </summary>
        private void RestoreChromeAfterCapture()
        {
            ActiveTabUIController?.RestoreOverlay();
        }

        /// <summary>
        /// Handle active tab change — hide content WebView for empty tabs.
        /// </summary>
        private void HandleActiveTabChangedForContent(StraightFour.WorldState.TabState tab)
        {
            if (tab == null || string.IsNullOrEmpty(tab.WorldUrl))
            {
                // Empty tab: unload old content
                runtime.UnloadWorld();
                runtime.UnloadWebPage();
                HideContentFrame();

                // Load homepage if configured
                if (tab != null && !string.IsNullOrEmpty(homeUrl))
                {
                    tab.WorldUrl = homeUrl;
                    HandleNavigateRequest(homeUrl);
                }
            }
        }

        /// <summary>
        /// Handle page title changes from the content WebView.
        /// </summary>
        private void HandlePageTitleChanged(string title)
        {
            if (tabManager?.ActiveTab != null && !string.IsNullOrEmpty(title))
            {
                tabManager.ActiveTab.DisplayName = title;
                tabManager.NotifyTabStateChanged(tabManager.ActiveTab);
            }
        }

        /// <summary>
        /// Extract domain name from a URL for display purposes.
        /// </summary>
        private static string ExtractDomain(string url)
        {
            if (string.IsNullOrEmpty(url)) return "Web Page";
            try
            {
                if (System.Uri.TryCreate(url, System.UriKind.Absolute, out System.Uri uri))
                {
                    return uri.Host;
                }
            }
            catch { }
            return url.Length > 30 ? url.Substring(0, 27) + "..." : url;
        }

        /// <summary>
        /// Show the content frame and apply insets for webpage display.
        /// </summary>
        private void ShowContentFrame()
        {
            ActiveTabUIController?.SetContentFrameVisible(true);
            runtime.webverseWebView?.SetContentInsets(
                ContentInsetTop, ContentInsetLeft, ContentInsetBottom, ContentInsetRight);
        }

        /// <summary>
        /// Hide the content frame and reset insets.
        /// </summary>
        private void HideContentFrame()
        {
            ActiveTabUIController?.SetContentFrameVisible(false);
            runtime.webverseWebView?.SetContentInsets(0, 0, 0, 0);
        }

        /// <summary>
        /// Handle WebVerseWebView visibility changes.
        /// Shows content frame when a webpage is displayed, hides it otherwise.
        /// </summary>
        private void HandleWebViewVisibilityChanged(bool visible)
        {
            if (visible)
                ShowContentFrame();
            else
                HideContentFrame();
        }

        #endregion

        #region Menu Actions

        /// <summary>
        /// Handle menu action from Tab UI (fullscreen, vr-mode).
        /// Data-request actions (history, console, settings, about, exit) are
        /// handled directly by TabUIController events, not through menuAction.
        /// </summary>
        private void HandleMenuAction(string action)
        {
            switch (action)
            {
                case "fullscreen":
                    ToggleFullscreen();
                    break;
                case "vr-mode":
                    OnVRModeToggleRequested?.Invoke();
                    break;
                default:
                    Logging.LogWarning($"[TabUIIntegration] Unknown menu action: {action}");
                    break;
            }
        }

        private void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        #endregion

        #region Chrome WebView Data Requests

        /// <summary>
        /// Handle request for browsing history from Chrome WebView.
        /// Sends history data back via the active TabUIController.
        /// </summary>
        private void HandleRequestHistory()
        {
            try
            {
                if (historyProvider != null)
                {
                    var historyData = historyProvider.Invoke();
                    ActiveTabUIController?.SendHistoryData(historyData);
                }
                else
                {
                    Logging.LogWarning("[TabUIIntegration] History provider not set.");
                    ActiveTabUIController?.SendHistoryData(new List<object>());
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[TabUIIntegration->HandleRequestHistory] Error: {ex.Message}");
                ActiveTabUIController?.SendHistoryData(new List<object>());
            }
        }

        /// <summary>
        /// Handle request for console log from Chrome WebView.
        /// Sends console log data back via the active TabUIController.
        /// </summary>
        private void HandleRequestConsoleLog()
        {
            try
            {
                ActiveTabUIController?.SendConsoleData(consoleLogBuffer);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[TabUIIntegration->HandleRequestConsoleLog] Error: {ex.Message}");
                ActiveTabUIController?.SendConsoleData(new List<object>());
            }
        }

        /// <summary>
        /// Handle request for settings from Chrome WebView.
        /// Sends settings data back via the active TabUIController.
        /// </summary>
        private void HandleRequestSettings()
        {
            try
            {
                if (settingsProvider != null)
                {
                    var settingsData = settingsProvider.Invoke();
                    ActiveTabUIController?.SendSettingsData(settingsData);
                }
                else
                {
                    ActiveTabUIController?.SendSettingsData(new Dictionary<string, object>());
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[TabUIIntegration->HandleRequestSettings] Error: {ex.Message}");
                ActiveTabUIController?.SendSettingsData(new Dictionary<string, object>());
            }
        }

        /// <summary>
        /// Handle request for performance stats from Chrome WebView.
        /// Collects Unity profiling data and sends to the active TabUIController.
        /// </summary>
        private void HandleRequestStats()
        {
            float fps = 1.0f / Time.unscaledDeltaTime;
            float frameTimeMs = Time.unscaledDeltaTime * 1000f;
            long monoUsed = Profiler.GetMonoUsedSizeLong();
            long monoHeap = Profiler.GetMonoHeapSizeLong();
            long totalAllocated = Profiler.GetTotalAllocatedMemoryLong();
            long gpuMemory = Profiler.GetAllocatedMemoryForGraphicsDriver();

            var activeWorld = StraightFour.StraightFour.ActiveWorld;
            var entities = activeWorld?.entityManager?.GetAllEntities();
            int entityCount = entities?.Length ?? 0;

            // Count entity types
            int audioSources = 0;
            int physicsBodies = 0;
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (entity == null || entity.gameObject == null) continue;
                    if (entity.gameObject.GetComponent<AudioSource>() != null) audioSources++;
                    if (entity.gameObject.GetComponent<Rigidbody>() != null) physicsBodies++;
                }
            }

            string connectionState = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork ? "LAN"
                : Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ? "Cellular"
                : "Disconnected";

            var stats = new Dictionary<string, object>
            {
                { "rendering", new Dictionary<string, object>
                    {
                        { "fps", Mathf.Round(fps) },
                        { "frameTimeMs", Mathf.Round(frameTimeMs * 10f) / 10f },
                        { "gpuMemoryMB", Mathf.Round(gpuMemory / (1024f * 1024f)) },
                        { "fpsHistory", fpsHistory.ToArray() }
                    }
                },
                { "system", new Dictionary<string, object>
                    {
                        { "usedMemoryMB", Mathf.Round(totalAllocated / (1024f * 1024f)) },
                        { "totalMemoryMB", (float) SystemInfo.systemMemorySize },
                        { "monoUsedMB", Mathf.Round(monoUsed / (1024f * 1024f)) },
                        { "monoHeapMB", Mathf.Round(monoHeap / (1024f * 1024f)) },
                        { "gcTotalMB", Mathf.Round(System.GC.GetTotalMemory(false) / (1024f * 1024f)) }
                    }
                },
                { "network", new Dictionary<string, object>
                    {
                        { "isConnected", Application.internetReachability != NetworkReachability.NotReachable },
                        { "connectionState", connectionState }
                    }
                },
                { "world", new Dictionary<string, object>
                    {
                        { "entityCount", entityCount },
                        { "physicsBodies", physicsBodies },
                        { "audioSources", audioSources },
                        { "worldName", activeWorld?.siteName ?? "None" }
                    }
                }
            };

            ActiveTabUIController?.SendStatsData(stats);
        }

        /// <summary>
        /// Handle request for about info from Chrome WebView.
        /// Sends about info data back via the active TabUIController.
        /// </summary>
        private void HandleRequestAboutInfo()
        {
            ActiveTabUIController?.SendAboutData(new
            {
                title = "WebVerse",
                version = Application.version,
                description = "An open-source metaverse browser by Five Squared Interactive."
            });
        }

        /// <summary>
        /// Handle clear history request from Chrome WebView.
        /// </summary>
        private void HandleClearHistory()
        {
            OnClearHistoryRequested?.Invoke();
        }

        /// <summary>
        /// Handle save settings request from Chrome WebView.
        /// </summary>
        private void HandleSaveSettings(Dictionary<string, object> settings)
        {
            OnSaveSettingsRequested?.Invoke(settings);
        }

        /// <summary>
        /// Handle clear cache request from Chrome WebView.
        /// </summary>
        private void HandleClearCache(string timeRange)
        {
            OnClearCacheRequested?.Invoke(timeRange);
        }

        /// <summary>
        /// Handle exit request from Chrome WebView.
        /// </summary>
        private void HandleExitRequested()
        {
            OnExitRequested?.Invoke();
        }

        #endregion

        #region VR Mode

        /// <summary>
        /// Enable VR mode for Tab UI.
        /// </summary>
        public void EnableVRMode()
        {
            isVRMode = true;

            // Hide desktop Tab UI
            if (desktopTabUIController != null)
            {
                desktopTabUIController.HideChrome();
            }

            // Initialize and show VR Tab UI
            if (vrTabUIController != null && tabUIWebViewPrefab != null)
            {
                vrTabUIController.IsVR = true;
                vrTabUIController.VRParent = vrParent;
                vrTabUIController.Initialize(tabManager, tabUIWebViewPrefab);
                vrTabUIController.OnNavigateRequested += HandleNavigateRequest;
                vrTabUIController.OnMenuAction += HandleMenuAction;
                SubscribeToControllerDataEvents(vrTabUIController);
                vrTabUIController.ShowChrome();
            }

            // Update input handler reference
            if (inputHandler != null)
            {
                inputHandler.Initialize(ActiveTabUIController, tabManager);
            }
        }

        /// <summary>
        /// Disable VR mode for Tab UI.
        /// </summary>
        public void DisableVRMode()
        {
            isVRMode = false;

            // Hide VR Tab UI
            if (vrTabUIController != null)
            {
                vrTabUIController.HideChrome();
            }

            // Show desktop Tab UI
            if (desktopTabUIController != null)
            {
                desktopTabUIController.ShowChrome();
            }

            // Update input handler reference
            if (inputHandler != null)
            {
                inputHandler.Initialize(ActiveTabUIController, tabManager);
            }
        }

        #endregion

        #region Chrome Visibility

        /// <summary>
        /// Show the Tab UI chrome.
        /// </summary>
        public void ShowChrome()
        {
            ActiveTabUIController?.ShowChrome();
        }

        /// <summary>
        /// Hide the Tab UI chrome.
        /// </summary>
        public void HideChrome()
        {
            ActiveTabUIController?.HideChrome();
        }

        /// <summary>
        /// Toggle the Tab UI chrome visibility.
        /// </summary>
        public void ToggleChrome()
        {
            ActiveTabUIController?.ToggleChrome();
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Terminate the Tab UI integration.
        /// </summary>
        public void Terminate()
        {
            // Unsubscribe events
            if (desktopTabUIController != null)
            {
                desktopTabUIController.OnNavigateRequested -= HandleNavigateRequest;
                desktopTabUIController.OnMenuAction -= HandleMenuAction;
                UnsubscribeFromControllerDataEvents(desktopTabUIController);
                desktopTabUIController.Terminate();
            }

            if (vrTabUIController != null)
            {
                vrTabUIController.OnNavigateRequested -= HandleNavigateRequest;
                vrTabUIController.OnMenuAction -= HandleMenuAction;
                UnsubscribeFromControllerDataEvents(vrTabUIController);
                vrTabUIController.Terminate();
            }

            if (inputHandler != null)
            {
                inputHandler.OnToggleFullscreen -= ToggleFullscreen;
            }

            if (runtime?.webverseWebView != null)
            {
                runtime.webverseWebView.OnVisibilityChanged -= HandleWebViewVisibilityChanged;
                runtime.webverseWebView.OnTitleChanged -= HandlePageTitleChanged;
            }

            if (tabManager != null)
            {
                tabManager.OnTabNavigateRequested -= HandleTabNavigateRequested;
                tabManager.OnActiveTabChanged -= HandleActiveTabChangedForContent;

                if (tabManager.ThumbnailService != null)
                {
                    tabManager.ThumbnailService.OnBeforeCapture -= HideChromeForCapture;
                    tabManager.ThumbnailService.OnAfterCapture -= RestoreChromeAfterCapture;
                }
            }

            // Unregister log callback
            if (logCallback != null)
            {
                Logging.RemoveCallback(logCallback);
                logCallback = null;
            }

            // Cleanup managers
            tabManager?.CloseAllTabs();
            worldStateManager?.Terminate();

            Logging.Log("[TabUIIntegration] Terminated.");
        }

        #endregion
    }
}
