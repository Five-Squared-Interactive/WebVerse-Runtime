// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Interface.MultibarMenu;
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
        /// Multibar.
        /// </summary>
        [Tooltip("Multibar.")]
        public Multibar multibar;

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
            nativeSettings.Initialize("3", settingsFilePath);
            nativeHistory.Initialize("3", historyFilePath);

            LoadRuntime();

            multibar.Initialize(Multibar.MultibarMode.Mobile, nativeSettings);

            string homeURL = nativeSettings.GetHomeURL();
            if (!string.IsNullOrEmpty(homeURL))
            {
                multibar.SetURL(homeURL);
                multibar.Enter();
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

            runtime.Initialize(storageMode, (int) maxEntries, (int) maxEntryLength, (int) maxKeyLength,
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