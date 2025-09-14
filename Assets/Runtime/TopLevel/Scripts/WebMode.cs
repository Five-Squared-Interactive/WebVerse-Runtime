// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using System.IO;
using FiveSQD.WebVerse.Interface.MultibarMenu;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// WebVerse Web Mode.
    /// </summary>
    public class WebMode : MonoBehaviour
    {
        /// <summary>
        /// Maximum local storage entries to use in Unity Editor tests.
        /// </summary>
        [Tooltip("Maximum local storage entries to use in Unity Editor tests.")]
        public string testMaxEntries = "2048";

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
        /// URL to load in Unity Editor tests.
        /// </summary>
        [Tooltip("URL to load in Unity Editor tests.")]
        public string testWorldURL = "https://raw.githubusercontent.com/Five-Squared-Interactive/WebVerse-Samples/main/Simple-Meshes-Scene/index.veml";

        /// <summary>
        /// World Load Timeout to use in Unity Editor tests.
        /// </summary>
        [Tooltip("World Load Timeout to use in Unity Editor tests.")]
        public string testWorldLoadTimeout;

        /// <summary>
        /// Multibar Enabled used in Unity Editor tests.
        /// </summary>
        [Tooltip("Multibar Enabled used in Unity Editor tests.")]
        public string testMultibarEnabled = "true";

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
        /// Holder for the Multibar.
        /// </summary>
        [Tooltip("Holder for the Multibar.")]
        public GameObject multibarHolder;

        /// <summary>
        /// Load a world.
        /// </summary>
        /// <param name="uri">URI of the world to load.</param>
        public void LoadWorld(string uri)
        {
            if (runtime == null)
            {
                Logging.LogError("[LightweightMode->LoadWorld] No runtime.");
                return;
            }

            runtime.LoadURL(uri);
        }

        /// <summary>
        /// Unload the current world.
        /// </summary>
        public void UnloadWorld()
        {
            if (runtime == null)
            {
                Logging.LogError("[LightweightMode->LoadWorld] No runtime.");
                return;
            }

            runtime.UnloadWorld();
        }

        private void Awake()
        {
            LoadRuntime();

            bool multibarEnabled = GetMultibarEnabled();
            if (multibarEnabled)
            {
                multibarHolder.SetActive(true);
                desktopMultibar.Initialize(Multibar.MultibarMode.Mobile);
            }
            else
            {
                multibarHolder.SetActive(false);
            }
        }

        /// <summary>
        /// Load the runtime.
        /// </summary>
        private void LoadRuntime()
        {
            int maxEntries = GetMaxEntries();
            if (maxEntries <= 0 || maxEntries >= 8192)
            {
                Logging.LogError("[LightweightMode->LoadRuntime] Invalid max entries value.");
                return;
            }

            int maxEntryLength = GetMaxEntryLength();
            if (maxEntryLength <= 8 || maxEntryLength >= 131072)
            {
                Logging.LogError("[LightweightMode->LoadRuntime] Invalid max entry length value.");
                return;
            }

            int maxKeyLength = GetMaxKeyLength();
            if (maxKeyLength <= 4 || maxKeyLength >= 8192)
            {
                Logging.LogError("[LightweightMode->LoadRuntime] Invalid max key length value.");
                return;
            }

            string filesDirectory = GetFilesDirectory();
            if (string.IsNullOrEmpty(filesDirectory))
            {
                Logging.LogError("[LightweightMode->LoadRuntime] Invalid files directory value.");
                return;
            }

            string worldURL = GetWorldURL();
            if (string.IsNullOrEmpty(worldURL))
            {
                Logging.LogError("[LightweightMode->LoadRuntime] Invalid world URL value.");
            }

            float worldLoadTimeout = GetWorldLoadTimeout();
            if (worldLoadTimeout <= 0)
            {
                Logging.LogError("[LightweightMode->LoadRuntime] Invalid world load timeout.");
                worldLoadTimeout = 120;
            }

            runtime.Initialize(LocalStorage.LocalStorageManager.LocalStorageMode.Cache,
                maxEntries, maxEntryLength, maxKeyLength, filesDirectory, worldLoadTimeout);

            if (!string.IsNullOrEmpty(worldURL))
            {
                LoadWorld(worldURL);
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
#elif UNITY_WEBGL
            int queryStart = Application.absoluteURL.IndexOf("?") + 1;
            if (queryStart > 1 && queryStart < Application.absoluteURL.Length - 1)
            {
                string query = Application.absoluteURL.Substring(queryStart);

                string[] sections = query.Split(new char[] { '&' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string section in sections)
                {
                    int valueStart = section.IndexOf("=") + 1;
                    string[] keyValue = section.Split('=');
                    if (keyValue.Length >= 2)
                    {
                        string key = keyValue[0];
                        string value = section.Substring(valueStart);
                        if (key.ToLower() == "max_entries")
                        {
                            maxEntries = value;
                            break;
                        }
                    }
                }
            }
#endif
            return int.Parse(maxEntries);
        }

        /// <summary>
        /// Get the Max Local Storage Entry Length, provided by URL query parameters in built app,
        /// and by 'testMaxEntryLength' variable in Editor mode.
        /// </summary>
        /// <returns>Max Local Storage Entry Length.</returns>
        private int GetMaxEntryLength()
        {
            string maxEntryLength = "";

#if UNITY_EDITOR
            maxEntryLength = testMaxEntryLength;
#elif UNITY_WEBGL
            int queryStart = Application.absoluteURL.IndexOf("?") + 1;
            if (queryStart > 1 && queryStart < Application.absoluteURL.Length - 1)
            {
                string query = Application.absoluteURL.Substring(queryStart);

                string[] sections = query.Split(new char[] { '&' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string section in sections)
                {
                    int valueStart = section.IndexOf("=") + 1;
                    string[] keyValue = section.Split('=');
                    if (keyValue.Length >= 2)
                    {
                        string key = keyValue[0];
                        string value = section.Substring(valueStart);
                        if (key.ToLower() == "max_entry_length")
                        {
                            maxEntryLength = value;
                            break;
                        }
                    }
                }
            }
#endif
            return int.Parse(maxEntryLength);
        }

        /// <summary>
        /// Get the Max Local Storage Key Length, provided by URL query parameters in built app,
        /// and by 'testMaxKeyLength' variable in Editor mode.
        /// </summary>
        /// <returns>Max Local Storage Key Length.</returns>
        private int GetMaxKeyLength()
        {
            string maxKeyLength = "";

#if UNITY_EDITOR
            maxKeyLength = testMaxKeyLength;
#elif UNITY_WEBGL
            int queryStart = Application.absoluteURL.IndexOf("?") + 1;
            if (queryStart > 1 && queryStart < Application.absoluteURL.Length - 1)
            {
                string query = Application.absoluteURL.Substring(queryStart);

                string[] sections = query.Split(new char[] { '&' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string section in sections)
                {
                    int valueStart = section.IndexOf("=") + 1;
                    string[] keyValue = section.Split('=');
                    if (keyValue.Length >= 2)
                    {
                        string key = keyValue[0];
                        string value = section.Substring(valueStart);
                        if (key.ToLower() == "max_key_length")
                        {
                            maxKeyLength = value;
                            break;
                        }
                    }
                }
            }
#endif
            return int.Parse(maxKeyLength);
        }

        /// <summary>
        /// Get the Files Directory, provided by URL query parameters in built app,
        /// and by 'testFilesDirectory' variable in Editor mode.
        /// </summary>
        /// <returns>Files Directory.</returns>
        private string GetFilesDirectory()
        {
            string filesDirectory = "";

#if UNITY_EDITOR
            filesDirectory = Path.Combine(Application.dataPath, testFilesDirectory);
#elif UNITY_WEBGL
            int queryStart = Application.absoluteURL.IndexOf("?") + 1;
            if (queryStart > 1 && queryStart < Application.absoluteURL.Length - 1)
            {
                string query = Application.absoluteURL.Substring(queryStart);

                string[] sections = query.Split(new char[] { '&' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string section in sections)
                {
                    int valueStart = section.IndexOf("=") + 1;
                    string[] keyValue = section.Split('=');
                    if (keyValue.Length >= 2)
                    {
                        string key = keyValue[0];
                        string value = section.Substring(valueStart);
                        if (key.ToLower() == "files_directory")
                        {
                            filesDirectory = value;
                            break;
                        }
                    }
                }
            }
#endif
            return filesDirectory;
        }

        /// <summary>
        /// Get the World URL, provided by URL query parameters in built app,
        /// and by 'testWorldURL' variable in Editor mode.
        /// </summary>
        /// <returns>World URL.</returns>
        private string GetWorldURL()
        {
            string worldURL = "";
#if UNITY_EDITOR
            worldURL = testWorldURL;
#elif UNITY_WEBGL
            int queryStart = Application.absoluteURL.IndexOf("?") + 1;
            if (queryStart > 1 && queryStart < Application.absoluteURL.Length - 1)
            {
                string query = Application.absoluteURL.Substring(queryStart);
                
                string[] sections = query.Split(new char[] { '&' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string section in sections)
                {
                    int valueStart = section.IndexOf("=") + 1;
                    string[] keyValue = section.Split('=');
                    if (keyValue.Length >= 2)
                    {
                        string key = keyValue[0];
                        string value = section.Substring(valueStart);
                        if (key.ToLower() == "world_url")
                        {
                            worldURL = value;
                            break;
                        }
                    }
                }
            }
#endif
            
            return worldURL;
        }

        /// <summary>
        /// Get the World Load Timeout, provided by URL query parameters in built app,
        /// and by 'testWorldLoadTimeout' variable in Editor mode.
        /// </summary>
        /// <returns>World Load Timeout.</returns>
        private float GetWorldLoadTimeout()
        {
            string timeout = "";
#if UNITY_EDITOR
            timeout = testWorldLoadTimeout;
#elif UNITY_WEBGL
            int queryStart = Application.absoluteURL.IndexOf("?") + 1;
            if (queryStart > 1 && queryStart < Application.absoluteURL.Length - 1)
            {
                string query = Application.absoluteURL.Substring(queryStart);
                
                string[] sections = query.Split(new char[] { '&' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string section in sections)
                {
                    int valueStart = section.IndexOf("=") + 1;
                    string[] keyValue = section.Split('=');
                    if (keyValue.Length >= 2)
                    {
                        string key = keyValue[0];
                        string value = section.Substring(valueStart);
                        if (key.ToLower() == "world_load_timeout")
                        {
                            timeout = value;
                            break;
                        }
                    }
                }
            }
#endif

            return float.Parse(timeout);
        }

        /// <summary>
        /// Get Multibar Enabled, provided by URL query parameters in built app,
        /// and by 'testMultibarEnabled' variable in Editor mode.
        /// </summary>
        /// <returns>Whether Multibar is enabled.</returns>
        private bool GetMultibarEnabled()
        {
            string multibarEnabled = "";
#if UNITY_EDITOR
            multibarEnabled = testMultibarEnabled;
#elif UNITY_WEBGL
            int queryStart = Application.absoluteURL.IndexOf("?") + 1;
            if (queryStart > 1 && queryStart < Application.absoluteURL.Length - 1)
            {
                string query = Application.absoluteURL.Substring(queryStart);
                
                string[] sections = query.Split(new char[] { '&' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string section in sections)
                {
                    int valueStart = section.IndexOf("=") + 1;
                    string[] keyValue = section.Split('=');
                    if (keyValue.Length >= 2)
                    {
                        string key = keyValue[0];
                        string value = section.Substring(valueStart);
                        if (key.ToLower() == "multibar_enabled")
                        {
                            multibarEnabled = value;
                            break;
                        }
                    }
                }
            }
#endif

            return multibarEnabled.ToLower() == "true" ? true : false;
        }
    }
}