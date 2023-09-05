using FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities;
using UnityEngine;

namespace FiveSQD.WebVerse.Runtime
{
    public class FocusedMode : MonoBehaviour
    {
        /// <summary>
        /// URI to use in Unity Editor tests.
        /// </summary>
        [Tooltip("URI to use in Unity Editor tests.")]
        public string testURI;

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
        /// WebVerse Runtime.
        /// </summary>
        [Tooltip("WebVerse Runtime.")]
        public WebVerseRuntime runtime;

        private void Awake()
        {
            LoadRuntime();
        }

        /// <summary>
        /// Load the runtime.
        /// </summary>
        private void LoadRuntime()
        {
            string uri = GetWorldURI();
            if (string.IsNullOrEmpty(uri))
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Could not get world URI.");
                return;
            }

            LocalStorage.LocalStorageManager.LocalStorageMode storageMode = GetStorageMode();
            if (storageMode != LocalStorage.LocalStorageManager.LocalStorageMode.Cache &&
                storageMode != LocalStorage.LocalStorageManager.LocalStorageMode.Persistent)
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Could not get storage mode.");
                return;
            }

            int maxEntries = GetMaxEntries();
            if (maxEntries <= 0 || maxEntries >= 262144)
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Invalid max entries value.");
                return;
            }

            int maxEntryLength = GetMaxEntryLength();
            if (maxEntryLength <= 8 || maxEntryLength >= 131072)
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Invalid max entry length value.");
                return;
            }

            int maxKeyLength = GetMaxKeyLength();
            if (maxKeyLength <= 4 || maxKeyLength >= 8192)
            {
                Logging.LogError("[FocusedMode->LoadRuntime] Invalid max key length value.");
                return;
            }

            runtime.Initialize(storageMode, maxEntries, maxEntryLength, maxKeyLength);
            runtime.LoadWorld(uri);
        }

        /// <summary>
        /// Get the World URI, provided by command line in built app, and by 'testURI'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>World URI.</returns>
        private string GetWorldURI()
        {
#if UNITY_EDITOR
            return testURI;
#else
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("uri="))
                {
                    return arg.Substring(4);
                }
            }
            return null;
#endif
        }

        /// <summary>
        /// Get the Local Storage Mode, provided by command line in built app, and by 'testStorageMode'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Local Storage Mode.</returns>
        private LocalStorage.LocalStorageManager.LocalStorageMode GetStorageMode()
        {
            string storageMode = "";

#if UNITY_EDITOR
            storageMode = testStorageMode;
#else
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("storagemode="))
                {
                    storageMode = arg.Substring(12);
                }
            }
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
        /// Get the Max Local Storage Entries, provided by command line in built app, and by 'testMaxEntries'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Max Local Storage Entries.</returns>
        private int GetMaxEntries()
        {
            string maxEntries = "";

#if UNITY_EDITOR
            maxEntries = testMaxEntries;
#else
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("maxentries="))
                {
                    maxEntries = arg.Substring(11);
                }
            }
#endif
            return int.Parse(maxEntries);
        }

        /// <summary>
        /// Get the Max Local Storage Entry Length, provided by command line in built app, and by 'testMaxEntryLength'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Max Local Storage Entry Length.</returns>
        private int GetMaxEntryLength()
        {
            string maxEntryLength = "";

#if UNITY_EDITOR
            maxEntryLength = testMaxEntryLength;
#else
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("maxentrylength="))
                {
                    maxEntryLength = arg.Substring(15);
                }
            }
#endif
            return int.Parse(maxEntryLength);
        }

        /// <summary>
        /// Get the Max Local Storage Key Length, provided by command line in built app, and by 'testMaxKeyLength'
        /// variable in Editor mode.
        /// </summary>
        /// <returns>Max Local Storage Key Length.</returns>
        private int GetMaxKeyLength()
        {
            string maxKeyLength = "";

#if UNITY_EDITOR
            maxKeyLength = testMaxKeyLength;
#else
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("maxkeylength="))
                {
                    maxKeyLength = arg.Substring(13);
                }
            }
#endif
            return int.Parse(maxKeyLength);
        }
    }
}