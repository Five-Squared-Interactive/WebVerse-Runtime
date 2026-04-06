// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Quest3;
using FiveSQD.WebVerse.Interface.TabUI;
using UnityEngine;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// WebVerse Quest 3 Mode.
    /// </summary>
    public class Quest3Mode : MonoBehaviour
    {
        /// <summary>
        /// Path to WebVerse settings file.
        /// </summary>
        [Tooltip("Path to WebVerse settings file.")]
        public string settingsFilePath = ".wv-settings";

        /// <summary>
        /// Path to WebVerse history file.
        /// </summary>
        [Tooltip("Path to WebVerse history file.")]
        public string historyFilePath = ".wv-history";

        /// <summary>
        /// Storage mode to use in Unity Editor tests.
        /// </summary>
        [Tooltip("Storage mode to use in Unity Editor tests.")]
        public string testStorageMode = "persistent";

        /// <summary>
        /// Maximum local storage entries to use in Unity Editor tests.
        /// </summary>
        [Tooltip("Maximum local storage entries to use in Unity Editor tests.")]
        public string testMaxEntries = "65536";

        /// <summary>
        /// Maximum local storage entry length to use in Unity Editor tests.
        /// </summary>
        [Tooltip("Maximum local storage entry length to use in Unity Editor tests.")]
        public string testMaxEntryLength = "16384";

        /// <summary>
        /// Maximum local storage key length to use in Unity Editor tests.
        /// </summary>
        [Tooltip("Maximum local storage key length to use in Unity Editor tests.")]
        public string testMaxKeyLength = "512";

        /// <summary>
        /// Directory to use for files in Unity Editor tests.
        /// </summary>
        [Tooltip("Directory to use for files in Unity Editor Tests")]
        public string testFilesDirectory = "Files";

        /// <summary>
        /// World Load Timeout to use in Unity Editor tests.
        /// </summary>
        [Tooltip("World Load Timeout to use in Unity Editor tests.")]
        public string testWorldLoadTimeout = "120";

        /// <summary>
        /// WebVerse Runtime.
        /// </summary>
        [Tooltip("WebVerse Runtime.")]
        public WebVerseRuntime runtime;

        /// <summary>
        /// Native Settings.
        /// </summary>
        [Tooltip("Native Settings.")]
        public NativeSettings nativeSettings;

        /// <summary>
        /// Native History.
        /// </summary>
        [Tooltip("Native History.")]
        public NativeHistory nativeHistory;

        /// <summary>
        /// The VR Rig.
        /// </summary>
        [Tooltip("The VR Rig.")]
        public GameObject vrRig;

        /// <summary>
        /// The VR Camera.
        /// </summary>
        [Tooltip("The VR Camera.")]
        public Camera vrCamera;

        /// <summary>
        /// The Quest 3 Input.
        /// </summary>
        [Tooltip("The Quest 3 Input.")]
        public GameObject quest3Input;

        /// <summary>
        /// The Quest 3 Platform Input.
        /// </summary>
        [Tooltip("The Quest 3 Platform Input.")]
        public BasePlatformInput quest3PlatformInput;

        /// <summary>
        /// Sky sphere follower.
        /// </summary>
        [Tooltip("Sky sphere follower.")]
        public StraightFour.Environment.SkySphereFollower skySphereFollower;

        /// <summary>
        /// Tab UI Integration.
        /// </summary>
        [Tooltip("Tab UI Integration.")]
        public TabUIIntegration tabUIIntegration;

        /// <summary>
        /// Cached Quest3Input component for button event subscriptions.
        /// </summary>
        private Quest3Input quest3InputComponent;


        private void Awake()
        {
            // Platform detection - verify we're on Quest 3 or compatible Quest device
#if !UNITY_EDITOR
            if (!Quest3PlatformDetector.IsQuest3Platform())
            {
                string deviceModel = Quest3PlatformDetector.GetQuestDeviceModel();
                if (string.IsNullOrEmpty(deviceModel))
                {
                    Logging.LogError("[Quest3Mode->Awake] Not running on Quest platform. Disabling Quest3Mode.");
                }
                else
                {
                    Logging.LogWarning($"[Quest3Mode->Awake] Running on {deviceModel}, not Quest 3. Proceeding anyway.");
                }
                // Don't return - allow the mode to proceed on other Quest devices (Quest 2, Quest Pro)
                // Only truly disable if not on any Quest device
                if (!Quest3PlatformDetector.IsAnyQuestPlatform())
                {
                    enabled = false;
                    return;
                }
            }
            else
            {
                Logging.Log("[Quest3Mode->Awake] Quest 3 platform detected successfully.");
            }
#else
            Logging.Log("[Quest3Mode->Awake] Running in Editor - skipping platform detection.");
#endif

            // Apply Quest 3 performance settings early, before runtime initialization
            Quest3PerformanceConfig.Apply();

            Logging.Log("[Q3TabUI] Awake: calling LoadRuntime");
            LoadRuntime();
            Logging.Log($"[Q3TabUI] Awake: LoadRuntime OK, Instance={(Runtime.WebVerseRuntime.Instance != null ? "SET" : "NULL")}");

            // Initialize VR
            StartCoroutine(InitializeVR());

            string homeURL = "";

            // Initialize Tab UI
            Logging.Log($"[Q3TabUI] Awake: tabUIIntegration={(tabUIIntegration != null ? "SET" : "NULL")}");
            if (tabUIIntegration != null)
            {
                Logging.Log($"[Q3TabUI] Awake: vrCamera={(vrCamera != null ? vrCamera.name : "NULL")}");
                // Pass VR camera (not set on TabUIIntegration in scene)
                if (vrCamera != null)
                {
                    tabUIIntegration.SetVRCamera(vrCamera);
                }

                tabUIIntegration.SetHomeUrl(homeURL);

                // Wire data providers
                tabUIIntegration.SetHistoryProvider(() => GetHistoryData());
                tabUIIntegration.SetConsoleLogProvider(() => GetConsoleLogData());
                tabUIIntegration.SetSettingsProvider(() => GetSettingsData());

                // Wire action handlers
                tabUIIntegration.OnClearHistoryRequested += HandleClearHistory;
                tabUIIntegration.OnSaveSettingsRequested += HandleSaveSettings;
                tabUIIntegration.OnClearCacheRequested += HandleClearCache;
                tabUIIntegration.OnExitRequested += HandleExit;
                tabUIIntegration.OnPageLoaded += HandlePageLoaded;

                // TabUIIntegration initializes in Start() (after Awake), so defer VR mode enable
                StartCoroutine(EnableVRModeAfterTabUIInit());

                Logging.Log("[Q3TabUI] Awake: Tab UI providers wired, coroutines started");
            }
        }

        /// <summary>
        /// Wait for TabUIIntegration to finish initializing, then enable VR mode.
        /// </summary>
        private IEnumerator EnableVRModeAfterTabUIInit()
        {
            Logging.Log($"[Q3TabUI] EnableVRModeAfterTabUIInit: waiting for IsInitialized (currently {tabUIIntegration.IsInitialized})");
            int frames = 0;
            while (!tabUIIntegration.IsInitialized)
            {
                frames++;
                if (frames % 300 == 0)
                {
                    Logging.Log($"[Q3TabUI] EnableVRModeAfterTabUIInit: still waiting after {frames} frames, Instance={(Runtime.WebVerseRuntime.Instance != null ? "SET" : "NULL")}");
                }
                yield return null;
            }

            Logging.Log($"[Q3TabUI] EnableVRModeAfterTabUIInit: IsInitialized=true after {frames} frames, calling EnableVRMode");
            tabUIIntegration.EnableVRMode();
            Logging.Log("[Q3TabUI] EnableVRModeAfterTabUIInit: EnableVRMode completed");
        }

        /// <summary>
        /// Initialize VR in a coroutine.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator InitializeVR()
        {
            // Wait for XR to be initialized by XR Management (auto-start)
            yield return new WaitForSeconds(0.1f);

            // Set up the runtime for VR
            runtime.platformInput = quest3PlatformInput;
            if (runtime.inputManager != null)
            {
                runtime.inputManager.platformInput = quest3PlatformInput;
            }
            runtime.vr = true;

            // Set canvas event camera
            SetCanvasEventCamera(vrCamera);

            // Set sky sphere follower
            if (skySphereFollower != null)
            {
                skySphereFollower.transformToFollow = vrCamera.transform;
            }

            // Route Quest 3 menu button and Y button to Tab UI chrome toggle
            if (tabUIIntegration != null && quest3Input != null)
            {
                quest3InputComponent = quest3Input.GetComponent<Quest3Input>();
                if (quest3InputComponent != null)
                {
                    quest3InputComponent.OnMenuPressed += tabUIIntegration.ToggleChrome;
                    quest3InputComponent.OnLeftSecondaryPressed += tabUIIntegration.ToggleChrome;
                    Logging.Log("[Quest3Mode->InitializeVR] Menu and Y buttons wired to Tab UI ToggleChrome.");
                }
            }

            // Initialize VR rig (sets up platform-specific controller models)
            if (vrRig != null)
            {
                var vrRigComponent = vrRig.GetComponent<VRRig>();
                if (vrRigComponent != null)
                {
                    vrRigComponent.Initialize();
                }
            }

            Logging.Log("[Quest3Mode->InitializeVR] Quest 3 VR initialized.");
        }

        /// <summary>
        /// Load the runtime.
        /// </summary>
        private void LoadRuntime()
        {
            int maxEntries = int.Parse(testMaxEntries);
            int maxEntryLength = int.Parse(testMaxEntryLength);
            int maxKeyLength = int.Parse(testMaxKeyLength);
            string filesDirectory = System.IO.Path.Combine(Application.persistentDataPath, testFilesDirectory);
            float worldLoadTimeout = float.Parse(testWorldLoadTimeout);
            if (worldLoadTimeout <= 0) worldLoadTimeout = 120;

            runtime.Initialize(LocalStorage.LocalStorageManager.LocalStorageMode.Cache,
                maxEntries, maxEntryLength, maxKeyLength, filesDirectory, worldLoadTimeout);
        }

        /// <summary>
        /// Sets the event camera for all canvas entities.
        /// </summary>
        /// <param name="eventCamera">Event camera to set all canvas entities' event camera to.</param>
        private void SetCanvasEventCamera(Camera eventCamera)
        {
            if (StraightFour.StraightFour.ActiveWorld != null)
            {
                foreach (StraightFour.Entity.BaseEntity entity in StraightFour.StraightFour.ActiveWorld.entityManager.GetAllEntities())
                {
                    if (entity is StraightFour.Entity.CanvasEntity)
                    {
                        ((StraightFour.Entity.CanvasEntity)entity).canvasObject.worldCamera = eventCamera;
                    }
                }
            }
        }

        /// <summary>
        /// Get the Local Storage Mode.
        /// </summary>
        /// <returns>Local Storage Mode.</returns>

        #region Tab UI Data Providers

        /// <summary>
        /// Get browsing history formatted for the Tab UI.
        /// </summary>
        public object GetHistoryData()
        {
            try
            {
                if (nativeHistory == null) return new List<Dictionary<string, string>>();

                var items = nativeHistory.GetAllItemsFromHistory();
                if (items == null || items.Length == 0) return new List<Dictionary<string, string>>();

                var result = new List<Dictionary<string, string>>();
                var sorted = items.OrderByDescending(item => item.Item1);
                foreach (var item in sorted)
                {
                    result.Add(new Dictionary<string, string>
                    {
                        { "timestamp", item.Item1.ToString("o") },
                        { "name", item.Item2 ?? "" },
                        { "url", item.Item3 ?? "" }
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[Quest3Mode->GetHistoryData] Error: {ex.Message}");
                return new List<Dictionary<string, string>>();
            }
        }

        /// <summary>
        /// Get console log data formatted for the Tab UI.
        /// Returns empty list — live console lines are forwarded via Logging callback.
        /// </summary>
        public object GetConsoleLogData()
        {
            return new List<object>();
        }

        /// <summary>
        /// Get current settings formatted for the Tab UI.
        /// </summary>
        public object GetSettingsData()
        {
            try
            {
                return new Dictionary<string, object>
                {
                    { "homeURL", nativeSettings.GetHomeURL() ?? "" },
                    { "worldLoadTimeout", (int) nativeSettings.GetWorldLoadTimeout() },
                    { "storageMode", nativeSettings.GetStorageMode() },
                    { "maxStorageEntries", (int) nativeSettings.GetMaxStorageEntries() },
                    { "maxStorageKeyLength", (int) nativeSettings.GetMaxStorageKeyLength() },
                    { "maxStorageEntryLength", (int) nativeSettings.GetMaxStorageEntryLength() },
                    { "cacheDirectory", nativeSettings.GetCacheDirectory() }
                };
            }
            catch (Exception ex)
            {
                Logging.LogError($"[Quest3Mode->GetSettingsData] Error: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Handle clear history request from Tab UI.
        /// </summary>
        public void HandleClearHistory()
        {
            if (nativeHistory != null)
            {
                nativeHistory.ClearHistory();
                Logging.Log("[Quest3Mode] History cleared.");
            }
        }

        /// <summary>
        /// Handle save settings request from Tab UI.
        /// </summary>
        public void HandleSaveSettings(Dictionary<string, object> settings)
        {
            if (nativeSettings == null || settings == null) return;

            try
            {
                if (settings.TryGetValue("homeURL", out object homeUrl))
                    nativeSettings.SetHomeURL(homeUrl?.ToString() ?? "");

                if (settings.TryGetValue("storageMode", out object storageMode))
                    nativeSettings.SetStorageMode(storageMode?.ToString() ?? "persistent");

                if (settings.TryGetValue("worldLoadTimeout", out object wlt))
                    nativeSettings.SetWorldLoadTimeout(Convert.ToUInt32(wlt));

                if (settings.TryGetValue("maxStorageEntries", out object mse))
                    nativeSettings.SetMaxStorageEntries(Convert.ToUInt32(mse));

                if (settings.TryGetValue("maxStorageKeyLength", out object mskl))
                    nativeSettings.SetMaxStorageKeyLength(Convert.ToUInt32(mskl));

                if (settings.TryGetValue("maxStorageEntryLength", out object msel))
                    nativeSettings.SetMaxStorageEntryLength(Convert.ToUInt32(msel));

                if (settings.TryGetValue("cacheDirectory", out object cacheDir))
                    nativeSettings.SetCacheDirectory(cacheDir?.ToString() ?? "");

                Logging.Log("[Quest3Mode] Settings saved.");
            }
            catch (Exception ex)
            {
                Logging.LogError($"[Quest3Mode->HandleSaveSettings] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle page loaded — record in browsing history.
        /// </summary>
        public void HandlePageLoaded(string siteName, string url)
        {
            if (nativeHistory != null && !string.IsNullOrEmpty(url))
            {
                nativeHistory.AddItemToHistory(DateTime.Now, siteName ?? "Web Page", url);
            }
        }

        /// <summary>
        /// Handle clear cache request from Tab UI.
        /// </summary>
        public void HandleClearCache(string timeRange)
        {
            try
            {
                string cacheDir = testFilesDirectory;
                string fullPath = System.IO.Path.Combine(Application.persistentDataPath, cacheDir);
                if (string.IsNullOrEmpty(fullPath))
                {
                    Logging.LogWarning("[Quest3Mode->HandleClearCache] Cache directory not configured.");
                    return;
                }

                if (System.IO.Directory.Exists(fullPath))
                {
                    foreach (var file in System.IO.Directory.GetFiles(fullPath))
                        System.IO.File.Delete(file);
                    foreach (var dir in System.IO.Directory.GetDirectories(fullPath))
                        System.IO.Directory.Delete(dir, true);
                    Logging.Log($"[Quest3Mode] Cache cleared (timeRange: {timeRange}, path: {fullPath}).");
                }
                else
                {
                    Logging.Log($"[Quest3Mode] Cache directory does not exist: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[Quest3Mode->HandleClearCache] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle exit request from Tab UI.
        /// </summary>
        public void HandleExit()
        {
            Logging.Log("[Quest3Mode] Exit requested.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        private void OnDestroy()
        {
            // Unsubscribe button events from Tab UI
            if (quest3InputComponent != null && tabUIIntegration != null)
            {
                quest3InputComponent.OnMenuPressed -= tabUIIntegration.ToggleChrome;
                quest3InputComponent.OnLeftSecondaryPressed -= tabUIIntegration.ToggleChrome;
            }

            // Unsubscribe Tab UI event handlers
            if (tabUIIntegration != null)
            {
                tabUIIntegration.OnClearHistoryRequested -= HandleClearHistory;
                tabUIIntegration.OnSaveSettingsRequested -= HandleSaveSettings;
                tabUIIntegration.OnClearCacheRequested -= HandleClearCache;
                tabUIIntegration.OnExitRequested -= HandleExit;
                tabUIIntegration.OnPageLoaded -= HandlePageLoaded;
            }
        }
    }
}
