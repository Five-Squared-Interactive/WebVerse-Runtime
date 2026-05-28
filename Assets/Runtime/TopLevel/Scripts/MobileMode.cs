// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Mobile;
using FiveSQD.WebVerse.Interface.TabUI;
using UnityEngine;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// WebVerse Mobile Mode.
    /// </summary>
    public class MobileMode : MonoBehaviour
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
        [Tooltip("Directory to use for files in Unity Editory Tests")]
        public string testFilesDirectory = System.IO.Path.Combine(Application.dataPath, "Files");

        /// <summary>
        /// World Load Timeout to use in Unity Editor tests.
        /// </summary>
        [Tooltip("World Load Timeout to use in Unity Editor tests.")]
        public string testWorldLoadTimeout;

        /// <summary>
        /// WebVerse Runtime.
        /// </summary>
        [Tooltip("WebVerse Runtime.")]
        public WebVerseRuntime runtime;

        /// <summary>
        /// Tab UI Integration.
        /// </summary>
        [Tooltip("Tab UI Integration.")]
        public TabUIIntegration tabUIIntegration;

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
        /// The Mobile Rig.
        /// </summary>
        [Tooltip("The Mobile Rig.")]
        public GameObject mobileRig;

        /// <summary>
        /// The Mobile Input.
        /// </summary>
        [Tooltip("The Mobile Input.")]
        public GameObject mobileInput;

        /// <summary>
        /// The Mobile Platform Input.
        /// </summary>
        [Tooltip("The Mobile Platform Input.")]
        public BasePlatformInput mobilePlatformInput;

        /// <summary>
        /// Sky sphere follower.
        /// </summary>
        [Tooltip("Sky sphere follower.")]
        public StraightFour.Environment.SkySphereFollower skySphereFollower;

        private void Awake()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.Portrait;

            nativeSettings.Initialize("3", System.IO.Path.Combine(Application.persistentDataPath, settingsFilePath));
            nativeHistory.Initialize("3", System.IO.Path.Combine(Application.persistentDataPath, historyFilePath));

            LoadRuntime();

            // Initialize Tab UI
            if (tabUIIntegration != null)
            {
                tabUIIntegration.forceMobile = true;
                string homeURL = nativeSettings.GetHomeURL();
                if (!string.IsNullOrEmpty(homeURL))
                    tabUIIntegration.SetHomeUrl(homeURL);

                tabUIIntegration.SetHistoryProvider(() => GetHistoryData());
                tabUIIntegration.SetConsoleLogProvider(() => GetConsoleLogData());
                tabUIIntegration.SetSettingsProvider(() => GetSettingsData());

                tabUIIntegration.OnClearHistoryRequested += HandleClearHistory;
                tabUIIntegration.OnSaveSettingsRequested += HandleSaveSettings;
                tabUIIntegration.OnClearCacheRequested += HandleClearCache;
                tabUIIntegration.OnExitRequested += HandleExit;
                tabUIIntegration.OnPageLoaded += HandlePageLoaded;

                Logging.Log("[MobileMode->Awake] Tab UI initialized with data providers.");
            }

            // Wire touch hooks to TabUI
            if (tabUIIntegration != null && mobileInput != null)
            {
                StartCoroutine(WireTabUIControllerWhenReady());
            }
        }

        private void Update()
        {
            // Android back button maps to KeyCode.Escape
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if (tabUIIntegration != null && tabUIIntegration.ActiveTabUIController != null)
                {
                    tabUIIntegration.ActiveTabUIController.SendPlatformBack();
                }
            }
        }

        private void OnDestroy()
        {
            if (tabUIIntegration != null)
            {
                tabUIIntegration.OnClearHistoryRequested -= HandleClearHistory;
                tabUIIntegration.OnSaveSettingsRequested -= HandleSaveSettings;
                tabUIIntegration.OnClearCacheRequested -= HandleClearCache;
                tabUIIntegration.OnExitRequested -= HandleExit;
                tabUIIntegration.OnPageLoaded -= HandlePageLoaded;
            }
        }

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
                Logging.LogError($"[MobileMode->GetHistoryData] Error: {ex.Message}");
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
                    { "cacheDirectory", nativeSettings.GetCacheDirectory() },
                    { "defaultAvatar", nativeSettings.GetDefaultAvatar() }
                };
            }
            catch (Exception ex)
            {
                Logging.LogError($"[MobileMode->GetSettingsData] Error: {ex.Message}");
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
                Logging.Log("[MobileMode] History cleared.");
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

                if (settings.TryGetValue("defaultAvatar", out object defaultAvatar))
                    nativeSettings.SetDefaultAvatar(defaultAvatar?.ToString() ?? "rigged");

                Logging.Log("[MobileMode] Settings saved.");
            }
            catch (Exception ex)
            {
                Logging.LogError($"[MobileMode->HandleSaveSettings] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle clear cache request from Tab UI.
        /// </summary>
        public void HandleClearCache(string timeRange)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(Application.persistentDataPath, GetCacheDirectory());
                if (string.IsNullOrEmpty(fullPath))
                {
                    Logging.LogWarning("[MobileMode->HandleClearCache] Cache directory not configured.");
                    return;
                }

                if (System.IO.Directory.Exists(fullPath))
                {
                    foreach (var file in System.IO.Directory.GetFiles(fullPath))
                        System.IO.File.Delete(file);
                    foreach (var dir in System.IO.Directory.GetDirectories(fullPath))
                        System.IO.Directory.Delete(dir, true);
                    Logging.Log($"[MobileMode] Cache cleared (timeRange: {timeRange}, path: {fullPath}).");
                }
                else
                {
                    Logging.Log($"[MobileMode] Cache directory does not exist: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[MobileMode->HandleClearCache] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle exit request from Tab UI.
        /// </summary>
        public void HandleExit()
        {
            Logging.Log("[MobileMode] Exit requested.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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

        private IEnumerator WireTabUIControllerWhenReady()
        {
            // Wait for TabUIIntegration to finish initializing (timeout after 30s)
            float timeout = 30f;
            float elapsed = 0f;
            while (tabUIIntegration != null && !tabUIIntegration.IsInitialized)
            {
                elapsed += Time.deltaTime;
                if (elapsed > timeout)
                {
                    Logging.LogWarning("[MobileMode] TabUIIntegration did not initialize within 30s. Touch hooks not wired.");
                    yield break;
                }
                yield return null;
            }

            var mobileInputComponent = mobileInput.GetComponent<MobileInput>();
            if (mobileInputComponent == null)
            {
                Logging.LogWarning("[MobileMode] mobileInput GameObject lacks MobileInput component. Touch hooks not wired.");
                yield break;
            }
            if (tabUIIntegration != null && tabUIIntegration.ActiveTabUIController != null)
            {
                mobileInputComponent.TabUIController = tabUIIntegration.ActiveTabUIController;
                Logging.Log("[MobileMode] MobileInput wired to TabUIController.");
            }
        }

        /// <summary>
        /// Load the runtime.
        /// </summary>
        private void LoadRuntime()
        {
            LocalStorage.LocalStorageManager.LocalStorageMode storageMode = GetStorageMode();
            if (storageMode != LocalStorage.LocalStorageManager.LocalStorageMode.Cache &&
                storageMode != LocalStorage.LocalStorageManager.LocalStorageMode.Persistent)
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Could not get storage mode.");
                return;
            }

            uint maxEntries = GetMaxEntries();
            if (maxEntries <= 0 || maxEntries >= 262144)
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Invalid max entries value.");
                return;
            }

            uint maxEntryLength = GetMaxEntryLength();
            if (maxEntryLength <= 8 || maxEntryLength >= 131072)
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Invalid max entry length value.");
                return;
            }

            uint maxKeyLength = GetMaxKeyLength();
            if (maxKeyLength <= 4 || maxKeyLength >= 8192)
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Invalid max key length value.");
                return;
            }

            string filesDirectory = System.IO.Path.Combine(Application.persistentDataPath, GetCacheDirectory());
            if (string.IsNullOrEmpty(filesDirectory))
            {
                Logging.LogError("[LightweightMode->LoadRuntime] Invalid files directory value.");
                return;
            }

            float worldLoadTimeout = GetWorldLoadTimeout();
            if (worldLoadTimeout <= 0)
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Invalid world load timeout.");
                worldLoadTimeout = 120;
            }

            runtime.Initialize(storageMode, (int) maxEntries, (int) maxEntryLength, (int) maxKeyLength,
                filesDirectory, worldLoadTimeout);

            runtime.defaultAvatarMode = nativeSettings.GetDefaultAvatar();
            FiveSQD.WebVerse.Avatar.AvatarAnimationManager.DefaultAvatarMode = runtime.defaultAvatarMode;
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
                        ((StraightFour.Entity.CanvasEntity) entity).canvasObject.worldCamera = eventCamera;
                    }
                }
            }
        }

        /// <summary>
        /// Get the Local Storage Mode, provided by settings file in built app, and by 'testStorageMode'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Local Storage Mode.</returns>
        private LocalStorage.LocalStorageManager.LocalStorageMode GetStorageMode()
        {
            string storageMode = "";

#if UNITY_EDITOR
            storageMode = testStorageMode;
#else
            storageMode = nativeSettings.GetStorageMode();
#endif
            if (storageMode.ToLower() == "persistent")
            {
                return LocalStorage.LocalStorageManager.LocalStorageMode.Persistent;
            }
            else if (storageMode.ToLower() == "cache")
            {
                return LocalStorage.LocalStorageManager.LocalStorageMode.Cache;
            }
            else
            {
                Logging.LogError("[FocusedMode->GetStorageMode] Invalid storage mode.");
                return LocalStorage.LocalStorageManager.LocalStorageMode.Uninitialized;
            }
        }

        /// <summary>
        /// Get the Max Local Storage Entries, provided by settings file in built app, and by 'testMaxEntries'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Max Local Storage Entries.</returns>
        private uint GetMaxEntries()
        {
#if UNITY_EDITOR
            return uint.Parse(testMaxEntries);
#else
            return nativeSettings.GetMaxStorageEntries();
#endif
        }

        /// <summary>
        /// Get the Max Local Storage Entry Length, provided by settings file in built app, and by 'testMaxEntryLength'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Max Local Storage Entry Length.</returns>
        private uint GetMaxEntryLength()
        {
#if UNITY_EDITOR
            return uint.Parse(testMaxEntryLength);
#else
            return nativeSettings.GetMaxStorageEntryLength();
#endif
        }

        /// <summary>
        /// Get the Max Local Storage Key Length, provided by settings file in built app, and by 'testMaxKeyLength'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Max Local Storage Key Length.</returns>
        private uint GetMaxKeyLength()
        {
#if UNITY_EDITOR
            return uint.Parse(testMaxKeyLength);
#else
            return nativeSettings.GetMaxStorageKeyLength();
#endif
        }

        /// <summary>
        /// Get the Cache Directory, provided by settings file in built app, and by 'testFilesDirectory'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Cache Directory.</returns>
        private string GetCacheDirectory()
        {
#if UNITY_EDITOR
            return testFilesDirectory;
#else
            return nativeSettings.GetCacheDirectory();
#endif
        }

        /// <summary>
        /// Get the World Load Timeout, provided by settings file in built app, and by 'testWorldLoadTimeout'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>World Load Timeout.</returns>
        private float GetWorldLoadTimeout()
        {
#if UNITY_EDITOR
            return float.Parse(testWorldLoadTimeout);
#else
            return nativeSettings.GetWorldLoadTimeout();
#endif
        }
    }
}
