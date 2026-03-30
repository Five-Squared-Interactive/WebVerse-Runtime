// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.SteamVR;
using FiveSQD.WebVerse.Interface.TabUI;
using UnityEngine;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// WebVerse Desktop Mode.
    /// </summary>
    public class DesktopMode : MonoBehaviour
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
        /// Tutorial State to use in Unity Editor tests.
        /// </summary>
        [Tooltip("Tutorial State to use in Unity Editor tests.")]
        public NativeSettings.TutorialState testTutorialState = NativeSettings.TutorialState.UNINITIALIZED;

        /// <summary>
        /// Logging Configuration to use in Unity Editor tests.
        /// </summary>
        [Tooltip("Logging Configuration to use in Unity Editor tests.")]
        public bool testLoggingEnableConsoleOutput = true;

        /// <summary>
        /// Enable default logging in Unity Editor tests.
        /// </summary>
        [Tooltip("Enable default logging in Unity Editor tests.")]
        public bool testLoggingEnableDefault = true;

        /// <summary>
        /// Enable debug logging in Unity Editor tests.
        /// </summary>
        [Tooltip("Enable debug logging in Unity Editor tests.")]
        public bool testLoggingEnableDebug = true;

        /// <summary>
        /// Enable warning logging in Unity Editor tests.
        /// </summary>
        [Tooltip("Enable warning logging in Unity Editor tests.")]
        public bool testLoggingEnableWarning = true;

        /// <summary>
        /// Enable error logging in Unity Editor tests.
        /// </summary>
        [Tooltip("Enable error logging in Unity Editor tests.")]
        public bool testLoggingEnableError = true;

        /// <summary>
        /// WebVerse Runtime.
        /// </summary>
        [Tooltip("WebVerse Runtime.")]
        public WebVerseRuntime runtime;

        /// <summary>
        /// Desktop Settings.
        /// </summary>
        [Tooltip("Desktop Settings.")]
        public NativeSettings desktopSettings;

        /// <summary>
        /// Desktop History.
        /// </summary>
        [Tooltip("Desktop History.")]
        public NativeHistory desktopHistory;

        /// <summary>
        /// The Desktop Rig.
        /// </summary>
        [Tooltip("The Desktop Rig.")]
        public GameObject desktopRig;

        /// <summary>
        /// The VR Rig.
        /// </summary>
        [Tooltip("The VR Rig.")]
        public GameObject vrRig;

        /// <summary>
        /// The VR Camera.
        /// </summary>
        [Tooltip("The VR Camera.")]
        public Camera desktopCamera;

        /// <summary>
        /// The Desktop Camera.
        /// </summary>
        [Tooltip("The Desktop Camera.")]
        public Camera vrCamera;

        /// <summary>
        /// The top-level VR rig.
        /// </summary>
        [Tooltip("The top-level VR rig.")]
        public GameObject topLevelVRRig;

        /// <summary>
        /// The Desktop Input.
        /// </summary>
        [Tooltip("The Desktop Input.")]
        public GameObject desktopInput;

        /// <summary>
        /// The SteamVR Input.
        /// </summary>
        [Tooltip("The SteamVR Input.")]
        public GameObject steamVRInput;

        /// <summary>
        /// The Desktop Platform Input.
        /// </summary>
        [Tooltip("The Desktop Platform Input.")]
        public BasePlatformInput desktopPlatformInput;

        /// <summary>
        /// The VR Platform Input.
        /// </summary>
        [Tooltip("The VR Platform Input.")]
        public BasePlatformInput vrPlatformInput;

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
        /// Whether or not VR is enabled.
        /// </summary>
        private bool vrEnabled;

        /// <summary>
        /// Cached SteamVR input component for VR menu button routing.
        /// </summary>
        private SteamVRInput steamVRInputComponent;

        /// <summary>
        /// Enable VR.
        /// </summary>
        public void EnableVR()
        {
            vrEnabled = true;
            desktopRig.SetActive(false);
            vrRig.transform.position = desktopRig.transform.position;
            vrRig.SetActive(true);
            topLevelVRRig.SetActive(true);
            desktopInput.SetActive(false);
            steamVRInput.SetActive(true);

            runtime.platformInput = vrPlatformInput;
            runtime.inputManager.platformInput = vrPlatformInput;
            runtime.vr = true;
            SetCanvasEventCamera(vrCamera);
            skySphereFollower.transformToFollow = vrCamera.transform;

            // Start XR init — VRRig and TabUI setup happen after XR is ready
            StartCoroutine(EnableVRCoroutine());
        }

        /// <summary>
        /// Disable VR.
        /// </summary>
        public void DisableVR()
        {
            if (vrEnabled)
            {
                Logging.Log("[FocusedMode->DisableVR] Stopping XR...");
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StopSubsystems();
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.DeinitializeLoader();
                Logging.Log("[FocusedMode->DisableVR] XR stopped completely.");
            }
            vrEnabled = false;
            vrRig.SetActive(false);
            desktopRig.transform.position = vrRig.transform.position;
            desktopRig.SetActive(true);
            topLevelVRRig.SetActive(false);
            desktopInput.SetActive(true);
            steamVRInput.SetActive(false);
            runtime.platformInput = desktopPlatformInput;
            if (runtime.inputManager != null)
            {
                runtime.inputManager.platformInput = desktopPlatformInput;
            }
            runtime.vr = false;
            SetCanvasEventCamera(desktopCamera);
            skySphereFollower.transformToFollow = desktopCamera.transform;

            // Unsubscribe VR menu button from TabUI
            if (steamVRInputComponent != null && tabUIIntegration != null)
            {
                steamVRInputComponent.OnMenuPressed -= tabUIIntegration.ToggleChrome;
                steamVRInputComponent = null;
            }

            // Switch Tab UI back to Desktop mode
            if (tabUIIntegration != null)
            {
                tabUIIntegration.DisableVRMode();
            }
        }

        private void Awake()
        {
            vrEnabled = false;
            DisableVR();
            desktopSettings.Initialize("3", settingsFilePath);
            desktopHistory.Initialize("3", historyFilePath);

            LoadRuntime();

            string homeURL = desktopSettings.GetHomeURL();

            // Initialize Tab UI
            if (tabUIIntegration != null)
            {
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
                tabUIIntegration.OnVRModeToggleRequested += () => { if (vrEnabled) DisableVR(); else EnableVR(); };

                Logging.Log("[DesktopMode->Awake] Tab UI initialized with data providers.");
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

            string filesDirectory = GetCacheDirectory();
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

            LoggingConfiguration loggingConfig = GetLoggingConfiguration();

            int automationPort = GetAutomationPort();

            runtime.Initialize(storageMode, (int) maxEntries, (int) maxEntryLength, (int) maxKeyLength,
                filesDirectory, worldLoadTimeout, loggingConfig, automationPort);
        }

        /// <summary>
        /// Get the automation server port from command-line arguments.
        /// Returns 0 if not specified (server disabled).
        /// Usage: WebVerse.exe --automation-port 9876
        /// </summary>
        /// <returns>The automation port, or 0 if disabled.</returns>
        private int GetAutomationPort()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--automation-port")
                {
                    int port;
                    if (int.TryParse(args[i + 1], out port) && port > 0 && port <= 65535)
                    {
                        Logging.Log("[DesktopMode->GetAutomationPort] Automation server port: " + port);
                        return port;
                    }
                    else
                    {
                        Logging.LogWarning("[DesktopMode->GetAutomationPort] Invalid port value: " + args[i + 1]);
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Enable VR in a coroutine.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator EnableVRCoroutine()
        {
            if (UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader != null )
            {
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StopSubsystems();
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }

            Logging.Log("[DesktopMode->EnableVRCoroutine] Initializing XR...");
            yield return UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Logging.LogError("[DesktopMode->EnableVRCoroutine] Initializing XR Failed. Check Editor or Player log for details.");
                vrEnabled = false;
                yield break;
            }

            Logging.Log("[DesktopMode->EnableVRCoroutine] Starting XR...");
            UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StartSubsystems();

            // Wait one frame for XR subsystems to fully activate
            yield return null;

            // Initialize VR rig now that XR is ready (sets pointer modes, platform config)
            FiveSQD.WebVerse.Input.VRRig vrRigComponent = null;
            if (vrRig != null)
                vrRigComponent = vrRig.GetComponentInChildren<FiveSQD.WebVerse.Input.VRRig>();
            if (vrRigComponent == null && topLevelVRRig != null)
                vrRigComponent = topLevelVRRig.GetComponentInChildren<FiveSQD.WebVerse.Input.VRRig>();
            if (vrRigComponent == null)
                vrRigComponent = FindObjectOfType<FiveSQD.WebVerse.Input.VRRig>();
            if (vrRigComponent != null)
            {
                vrRigComponent.Initialize();
                Logging.Log($"[DesktopMode->EnableVR] VRRig initialized. rightPointerMode={vrRigComponent.rightPointerMode}, rayType={vrRigComponent.rayInteractorType}, rightRay={(vrRigComponent.rightRayInteractor != null ? $"enabled={vrRigComponent.rightRayInteractor.enabled}" : "NULL")}, rightNearFar={(vrRigComponent.rightNearFarInteractor != null ? $"enabled={vrRigComponent.rightNearFarInteractor.enabled}" : "NULL")}");
            }
            else
            {
                Logging.LogWarning("[DesktopMode->EnableVR] VRRig component NOT FOUND anywhere in scene");
            }

            // Switch Tab UI to VR mode
            if (tabUIIntegration != null)
            {
                tabUIIntegration.EnableVRMode();
            }

            // Route VR menu button to TabUI chrome toggle
            steamVRInputComponent = steamVRInput.GetComponentInChildren<SteamVRInput>();
            if (steamVRInputComponent != null && tabUIIntegration != null)
            {
                steamVRInputComponent.OnMenuPressed += tabUIIntegration.ToggleChrome;
                Logging.Log("[DesktopMode->EnableVR] Subscribed VR menu button to TabUI ToggleChrome.");
            }
            else
            {
                Logging.LogWarning($"[DesktopMode->EnableVR] Failed to subscribe VR menu button. steamVRInputComponent={(steamVRInputComponent != null ? "found" : "null")}, tabUIIntegration={(tabUIIntegration != null ? "found" : "null")}");
            }
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
            storageMode = desktopSettings.GetStorageMode();
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
            return desktopSettings.GetMaxStorageEntries();
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
            return desktopSettings.GetMaxStorageEntryLength();
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
            return desktopSettings.GetMaxStorageKeyLength();
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
            return desktopSettings.GetCacheDirectory();
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
            return desktopSettings.GetWorldLoadTimeout();
#endif
        }

        /// <summary>
        /// Get the Tutorial State, provided by settings file in built app, and by 'testTutorialState'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Tutorial State.</returns>
        private NativeSettings.TutorialState GetTutorialState()
        {
#if UNITY_EDITOR
            return testTutorialState;
#else
            return desktopSettings.GetTutorialState();
#endif
        }

        /// <summary>
        /// Get the Logging Configuration, provided by settings file in built app, and by test variables
        /// in Editor mode.
        /// </summary>
        /// <returns>Logging Configuration.</returns>
        private LoggingConfiguration GetLoggingConfiguration()
        {
#if UNITY_EDITOR
            return new LoggingConfiguration
            {
                enableConsoleOutput = testLoggingEnableConsoleOutput,
                enableDefault = testLoggingEnableDefault,
                enableDebug = testLoggingEnableDebug,
                enableWarning = testLoggingEnableWarning,
                enableError = testLoggingEnableError,
                enableScriptDefault = testLoggingEnableDefault,
                enableScriptDebug = testLoggingEnableDebug,
                enableScriptWarning = testLoggingEnableWarning,
                enableScriptError = testLoggingEnableError
            };
#else
            // In production, use a more conservative configuration
            return LoggingConfiguration.CreateProduction();
#endif
        }

        #region Tab UI Data Providers

        /// <summary>
        /// Get browsing history formatted for the Tab UI.
        /// </summary>
        public object GetHistoryData()
        {
            try
            {
                if (desktopHistory == null) return new List<Dictionary<string, string>>();

                var items = desktopHistory.GetAllItemsFromHistory();
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
                Logging.LogError($"[DesktopMode->GetHistoryData] Error: {ex.Message}");
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
                    { "homeURL", desktopSettings.GetHomeURL() ?? "" },
                    { "worldLoadTimeout", (int) desktopSettings.GetWorldLoadTimeout() },
                    { "storageMode", desktopSettings.GetStorageMode() },
                    { "maxStorageEntries", (int) desktopSettings.GetMaxStorageEntries() },
                    { "maxStorageKeyLength", (int) desktopSettings.GetMaxStorageKeyLength() },
                    { "maxStorageEntryLength", (int) desktopSettings.GetMaxStorageEntryLength() },
                    { "cacheDirectory", desktopSettings.GetCacheDirectory() }
                };
            }
            catch (Exception ex)
            {
                Logging.LogError($"[DesktopMode->GetSettingsData] Error: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Handle clear history request from Tab UI.
        /// </summary>
        public void HandleClearHistory()
        {
            if (desktopHistory != null)
            {
                desktopHistory.ClearHistory();
                Logging.Log("[DesktopMode] History cleared.");
            }
        }

        /// <summary>
        /// Handle save settings request from Tab UI.
        /// </summary>
        public void HandleSaveSettings(Dictionary<string, object> settings)
        {
            if (desktopSettings == null || settings == null) return;

            try
            {
                if (settings.TryGetValue("homeURL", out object homeUrl))
                    desktopSettings.SetHomeURL(homeUrl?.ToString() ?? "");

                if (settings.TryGetValue("storageMode", out object storageMode))
                    desktopSettings.SetStorageMode(storageMode?.ToString() ?? "persistent");

                if (settings.TryGetValue("worldLoadTimeout", out object wlt))
                    desktopSettings.SetWorldLoadTimeout(Convert.ToUInt32(wlt));

                if (settings.TryGetValue("maxStorageEntries", out object mse))
                    desktopSettings.SetMaxStorageEntries(Convert.ToUInt32(mse));

                if (settings.TryGetValue("maxStorageKeyLength", out object mskl))
                    desktopSettings.SetMaxStorageKeyLength(Convert.ToUInt32(mskl));

                if (settings.TryGetValue("maxStorageEntryLength", out object msel))
                    desktopSettings.SetMaxStorageEntryLength(Convert.ToUInt32(msel));

                if (settings.TryGetValue("cacheDirectory", out object cacheDir))
                    desktopSettings.SetCacheDirectory(cacheDir?.ToString() ?? "");

                Logging.Log("[DesktopMode] Settings saved.");
            }
            catch (Exception ex)
            {
                Logging.LogError($"[DesktopMode->HandleSaveSettings] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle page loaded — record in browsing history.
        /// </summary>
        public void HandlePageLoaded(string siteName, string url)
        {
            if (desktopHistory != null && !string.IsNullOrEmpty(url))
            {
                desktopHistory.AddItemToHistory(DateTime.Now, siteName ?? "Web Page", url);
            }
        }

        /// <summary>
        /// Handle clear cache request from Tab UI.
        /// </summary>
        public void HandleClearCache(string timeRange)
        {
            try
            {
                string fullPath = GetCacheDirectory();
                if (string.IsNullOrEmpty(fullPath))
                {
                    Logging.LogWarning("[DesktopMode->HandleClearCache] Cache directory not configured.");
                    return;
                }

                if (System.IO.Directory.Exists(fullPath))
                {
                    foreach (var file in System.IO.Directory.GetFiles(fullPath))
                        System.IO.File.Delete(file);
                    foreach (var dir in System.IO.Directory.GetDirectories(fullPath))
                        System.IO.Directory.Delete(dir, true);
                    Logging.Log($"[DesktopMode] Cache cleared (timeRange: {timeRange}, path: {fullPath}).");
                }
                else
                {
                    Logging.Log($"[DesktopMode] Cache directory does not exist: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[DesktopMode->HandleClearCache] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle exit request from Tab UI.
        /// </summary>
        public void HandleExit()
        {
            Logging.Log("[DesktopMode] Exit requested.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}