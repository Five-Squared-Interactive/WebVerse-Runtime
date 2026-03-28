// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Quest3;
using FiveSQD.WebVerse.Interface.MultibarMenu;
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
        /// VR Multibar.
        /// </summary>
        [Tooltip("VR Multibar.")]
        public Multibar vrMultibar;

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

            nativeSettings.Initialize("3", System.IO.Path.Combine(Application.persistentDataPath, settingsFilePath));
            nativeHistory.Initialize("3", System.IO.Path.Combine(Application.persistentDataPath, historyFilePath));

            LoadRuntime();

            // Initialize VR
            StartCoroutine(InitializeVR());

            vrMultibar.Initialize(Multibar.MultibarMode.VR, nativeSettings);

            string homeURL = nativeSettings.GetHomeURL();
            if (!string.IsNullOrEmpty(homeURL))
            {
                vrMultibar.SetURL(homeURL);
                vrMultibar.Enter();
            }
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

            // Set up WebView as child of VR multibar
            if (runtime.webverseWebView != null && vrMultibar != null)
            {
                runtime.webverseWebView.SetupVRMode(vrMultibar.transform);
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
            LocalStorage.LocalStorageManager.LocalStorageMode storageMode = GetStorageMode();
            if (storageMode != LocalStorage.LocalStorageManager.LocalStorageMode.Cache &&
                storageMode != LocalStorage.LocalStorageManager.LocalStorageMode.Persistent)
            {
                Logging.LogError("[Quest3Mode->LoadRuntime] Could not get storage mode.");
                return;
            }

            uint maxEntries = GetMaxEntries();
            if (maxEntries <= 0 || maxEntries >= 262144)
            {
                Logging.LogError("[Quest3Mode->LoadRuntime] Invalid max entries value.");
                return;
            }

            uint maxEntryLength = GetMaxEntryLength();
            if (maxEntryLength <= 8 || maxEntryLength >= 131072)
            {
                Logging.LogError("[Quest3Mode->LoadRuntime] Invalid max entry length value.");
                return;
            }

            uint maxKeyLength = GetMaxKeyLength();
            if (maxKeyLength <= 4 || maxKeyLength >= 8192)
            {
                Logging.LogError("[Quest3Mode->LoadRuntime] Invalid max key length value.");
                return;
            }

            string filesDirectory = System.IO.Path.Combine(Application.persistentDataPath, GetCacheDirectory());
            if (string.IsNullOrEmpty(filesDirectory))
            {
                Logging.LogError("[Quest3Mode->LoadRuntime] Invalid files directory value.");
                return;
            }

            float worldLoadTimeout = GetWorldLoadTimeout();
            if (worldLoadTimeout <= 0)
            {
                Logging.LogError("[Quest3Mode->LoadRuntime] Invalid world load timeout.");
                worldLoadTimeout = 120;
            }

            runtime.Initialize(storageMode, (int)maxEntries, (int)maxEntryLength, (int)maxKeyLength,
                filesDirectory, worldLoadTimeout);
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
                Logging.LogError("[Quest3Mode->GetStorageMode] Invalid storage mode.");
                return LocalStorage.LocalStorageManager.LocalStorageMode.Uninitialized;
            }
        }

        /// <summary>
        /// Get the Max Local Storage Entries.
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
        /// Get the Max Local Storage Entry Length.
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
        /// Get the Max Local Storage Key Length.
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
        /// Get the Cache Directory.
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
        /// Get the World Load Timeout.
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
