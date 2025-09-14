// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Interface.MultibarMenu;
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
        public DesktopSettings.TutorialState testTutorialState = DesktopSettings.TutorialState.UNINITIALIZED;

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
        /// Desktop Multibar.
        /// </summary>
        [Tooltip("Desktop Multibar.")]
        public Multibar desktopMultibar;

        /// <summary>
        /// VR Multibar.
        /// </summary>
        [Tooltip("VR Multibar;")]
        public Multibar vrMultibar;

        /// <summary>
        /// Desktop Settings.
        /// </summary>
        [Tooltip("Desktop Settings.")]
        public DesktopSettings desktopSettings;

        /// <summary>
        /// Desktop History.
        /// </summary>
        [Tooltip("Desktop History.")]
        public DesktopHistory desktopHistory;

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
        /// Whether or not VR is enabled.
        /// </summary>
        private bool vrEnabled;

        /// <summary>
        /// Enable VR.
        /// </summary>
        public void EnableVR()
        {
            vrEnabled = true;
            vrMultibar.gameObject.SetActive(true);
            StartCoroutine(EnableVRCoroutine());
            desktopRig.SetActive(false);
            vrRig.transform.position = desktopRig.transform.position;
            vrRig.SetActive(true);
            topLevelVRRig.SetActive(true);
            desktopInput.SetActive(false);
            steamVRInput.SetActive(true);
            runtime.platformInput = vrPlatformInput;
            runtime.inputManager.platformInput = vrPlatformInput;
            runtime.vr = true;
            vrMultibar.SetUpVRMultibarVRButton();
            SetCanvasEventCamera(vrCamera);
            skySphereFollower.transformToFollow = vrCamera.transform;
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
            vrMultibar.gameObject.SetActive(false);
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
            desktopMultibar.SetUpDesktopMultibarVRButton(false);
            SetCanvasEventCamera(desktopCamera);
            skySphereFollower.transformToFollow = desktopCamera.transform;
        }

        private void Awake()
        {
            vrEnabled = false;
            DisableVR();
            desktopSettings.Initialize("3", settingsFilePath);
            desktopHistory.Initialize("3", historyFilePath);

            LoadRuntime();

            desktopMultibar.Initialize(Multibar.MultibarMode.Desktop, desktopSettings);
            vrMultibar.Initialize(Multibar.MultibarMode.VR, desktopSettings);

            DesktopSettings.TutorialState tutorialState = GetTutorialState();
            if (tutorialState != DesktopSettings.TutorialState.DO_NOT_SHOW)
            {
                desktopMultibar.Tutorial();
            }

            string homeURL = desktopSettings.GetHomeURL();
            if (!string.IsNullOrEmpty(homeURL))
            {
                desktopMultibar.SetURL(homeURL);
                desktopMultibar.Enter();
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

            runtime.Initialize(storageMode, (int) maxEntries, (int) maxEntryLength, (int) maxKeyLength,
                filesDirectory, worldLoadTimeout, loggingConfig);
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
            
            Logging.Log("[FocusedMode->EnableVRCoroutine] Initializing XR...");
            yield return UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Logging.LogError("[FocusedMode->EnableVRCoroutine] Initializing XR Failed. Check Editor or Player log for details.");
                vrEnabled = false;
                desktopMultibar.NoVR();
            }
            else
            {
                Logging.Log("[FocusedMode->EnableVRCoroutine] Starting XR...");
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StartSubsystems();
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
        private DesktopSettings.TutorialState GetTutorialState()
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
    }
}