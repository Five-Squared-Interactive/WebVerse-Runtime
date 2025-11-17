// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
#if !UNITY_WEBGL
using Mono.Data.Sqlite;
#endif
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// Class for Native Settings.
    /// </summary>
    public class NativeSettings : MonoBehaviour
    {
        /// <summary>
        /// Tutorial State.
        /// </summary>
        public enum TutorialState { DO_NOT_SHOW = 0, UNINITIALIZED = -1 }

        /// <summary>
        /// Key for Home URL.
        /// </summary>
        private readonly string homeURLKey = "HOME_URL";

        /// <summary>
        /// Key for Storage Mode.
        /// </summary>
        private readonly string storageModeKey = "STORAGE_MODE";

        /// <summary>
        /// Key for Max Storage Entries.
        /// </summary>
        private readonly string maxStorageEntriesKey = "MAX_STORAGE_ENTRIES";

        /// <summary>
        /// Key for Max Storage Key Length.
        /// </summary>
        private readonly string maxStorageKeyLengthKey = "MAX_STORAGE_KEY_LEN";

        /// <summary>
        /// Key for Max Storage Entry Length.
        /// </summary>
        private readonly string maxStorageEntryLengthKey = "MAX_STORAGE_ENTRY_LEN";

        /// <summary>
        /// Key for Cache Directory.
        /// </summary>
        private readonly string cacheDirectoryKey = "CACHE_DIRECTORY";

        /// <summary>
        /// Key for World Load Timeout.
        /// </summary>
        private readonly string worldLoadTimeoutKey = "WORLD_LOAD_TIMEOUT";

        /// <summary>
        /// Key for Tutorial State.
        /// </summary>
        private readonly string tutorialStateKey = "TUTORIAL_STATE";

        /// <summary>
        /// Default Storage Mode.
        /// </summary>
        private readonly string defaultStorageMode = "persistent";

        /// <summary>
        /// Default Max Storage Entries.
        /// </summary>
        private readonly uint defaultMaxStorageEntries = 65536;

        /// <summary>
        /// Defaylt Max Storage Key Length.
        /// </summary>
        private readonly uint defaultMaxStorageKeyLength = 512;

        /// <summary>
        /// Default Max Storage Entry Length.
        /// </summary>
        private readonly uint defaultMaxStorageEntryLength = 16384;

        /// <summary>
        /// Default Cache Directory.
        /// </summary>
        private readonly string defaultCacheDirectory = "wv_cache/";

        /// <summary>
        /// Default World Load Timeout.
        /// </summary>
        private readonly uint defaultWorldLoadTimeout = 60;

        /// <summary>
        /// Default Tutorial State.
        /// </summary>
        private readonly TutorialState defaultTutorialState = TutorialState.UNINITIALIZED;

        /// <summary>
        /// Version number for sqlite.
        /// </summary>
        private string sqliteVersion;

        /// <summary>
        /// Path to the sqlite database.
        /// </summary>
        private string dbPath;

        /// <summary>
        /// Initialize Native Settings.
        /// </summary>
        /// <param name="sqliteVersion">SQLite Version.</param>
        /// <param name="pathToDB">Path to Database.</param>
        public void Initialize(string sqliteVersion, string pathToDB)
        {
            this.sqliteVersion = sqliteVersion;
            dbPath = pathToDB;
            InitializeSettingsTable();
        }

        /// <summary>
        /// Terminate Native Settings.
        /// </summary>
        public void Terminate()
        {

        }

        /// <summary>
        /// Get the Home URL.
        /// </summary>
        /// <returns>The Home URL.</returns>
        public string GetHomeURL()
        {
            object rawResult = GetItem(homeURLKey);
            if (rawResult == null)
            {
                return null;
            }
            else if (rawResult is string)
            {
                return (string) rawResult;
            }
            else
            {
                Logging.LogWarning("[NativeSettings->GetHomeURL] Home URL not a string.");
                return null;
            }
        }

        /// <summary>
        /// Set the Home URL.
        /// </summary>
        /// <param name="homeURL">Home URL.</param>
        public void SetHomeURL(string homeURL)
        {
            SetItem(homeURLKey, homeURL);
        }

        /// <summary>
        /// Get the Storage Mode.
        /// </summary>
        /// <returns>The Storage Mode.</returns>
        public string GetStorageMode()
        {
            object rawResult = GetItem(storageModeKey);
            if (rawResult == null)
            {
                Logging.LogWarning("[NativeSettings->GetStorageMode] Storage Mode not set. Defaulting.");
                SetStorageMode(defaultStorageMode);
                return defaultStorageMode;
            }
            else if (rawResult is string)
            {
                if ((string) rawResult != "persistent" && (string) rawResult != "cache")
                {
                    Logging.LogWarning("[NativeSettings->GetStorageMode] Storage Mode invalid. Defaulting.");
                    SetStorageMode(defaultStorageMode);
                    return defaultStorageMode;
                }
                return (string) rawResult;
            }
            else
            {
                Logging.LogWarning("[NativeSettings->GetStorageMode] Storage Mode not a string. Defaulting");
                SetStorageMode(defaultStorageMode);
                return defaultStorageMode;
            }
        }

        /// <summary>
        /// Set the Storage Mode.
        /// </summary>
        /// <param name="storageMode">Storage Mode.</param>
        public void SetStorageMode(string storageMode)
        {
            SetItem(storageModeKey, storageMode);
        }

        /// <summary>
        /// Get the Max Storage Entries.
        /// </summary>
        /// <returns>The Max Storage Entries.</returns>
        public uint GetMaxStorageEntries()
        {
            object rawResult = GetItem(maxStorageEntriesKey);
            if (rawResult == null)
            {
                Logging.LogWarning("[NativeSettings->GetMaxStorageEntries] Max Storage Entries not set. Defaulting.");
                SetMaxStorageEntries(defaultMaxStorageEntries);
                return defaultMaxStorageEntries;
            }
            else if (rawResult is long)
            {
                return (uint) ((long) rawResult);
            }
            else
            {
                Logging.Log(rawResult.GetType().ToString());
                Logging.LogWarning("[NativeSettings->GetMaxStorageEntries] Max Storage Entries not a long. Defaulting.");
                SetMaxStorageEntries(defaultMaxStorageEntries);
                return defaultMaxStorageEntries;
            }
        }

        /// <summary>
        /// Set the Max Storage Entries.
        /// </summary>
        /// <param name="maxStorageEntries">Max Storage Entries.</param>
        public void SetMaxStorageEntries(uint maxStorageEntries)
        {
            SetItem(maxStorageEntriesKey, maxStorageEntries);
        }

        /// <summary>
        /// Get the Max Storage Key Length.
        /// </summary>
        /// <returns>The Max Storage Key Length.</returns>
        public uint GetMaxStorageKeyLength()
        {
            object rawResult = GetItem(maxStorageKeyLengthKey);
            if (rawResult == null)
            {
                Logging.LogWarning("[NativeSettings->GetMaxStorageKeyLength] Max Storage Key Length not set. Defaulting.");
                SetMaxStorageKeyLength(defaultMaxStorageKeyLength);
                return defaultMaxStorageKeyLength;
            }
            else if (rawResult is long)
            {
                return (uint) ((long) rawResult);
            }
            else
            {
                Logging.LogWarning("[NativeSettings->GetMaxStorageKeyLength] Max Storage Key Length not a long. Defaulting.");
                SetMaxStorageKeyLength(defaultMaxStorageKeyLength);
                return defaultMaxStorageKeyLength;
            }
        }

        /// <summary>
        /// Set the Max Storage Key Length.
        /// </summary>
        /// <param name="maxStorageKeyLength">Max Storage Key Length.</param>
        public void SetMaxStorageKeyLength(uint maxStorageKeyLength)
        {
            SetItem(maxStorageKeyLengthKey, maxStorageKeyLength);
        }

        /// <summary>
        /// Get the Max Storage Entry Length.
        /// </summary>
        /// <returns>The Max Storage Entry Length.</returns>
        public uint GetMaxStorageEntryLength()
        {
            object rawResult = GetItem(maxStorageEntryLengthKey);
            if (rawResult == null)
            {
                Logging.LogWarning("[NativeSettings->GetMaxStorageEntryLength] Max Storage Entry Length not set. Defaulting.");
                SetMaxStorageEntryLength(defaultMaxStorageEntryLength);
                return defaultMaxStorageEntryLength;
            }
            else if (rawResult is long)
            {
                return (uint) ((long) rawResult);
            }
            else
            {
                Logging.LogWarning("[NativeSettings->GetMaxStorageEntryLength] Max Storage Entry Length not a long. Defaulting.");
                SetMaxStorageEntryLength(defaultMaxStorageEntryLength);
                return defaultMaxStorageEntryLength;
            }
        }

        /// <summary>
        /// Set the Max Storage Entry Length.
        /// </summary>
        /// <param name="maxStorageEntryLength">Max Storage Entry Length.</param>
        public void SetMaxStorageEntryLength(uint maxStorageEntryLength)
        {
            SetItem(maxStorageEntryLengthKey, maxStorageEntryLength);
        }

        /// <summary>
        /// Get the Cache Directory.
        /// </summary>
        /// <returns>The Cache Directory.</returns>
        public string GetCacheDirectory()
        {
            object rawResult = GetItem(cacheDirectoryKey);
            if (rawResult == null)
            {
                Logging.LogWarning("[NativeSettings->GetCacheDirectory] Cache Directory not set. Defaulting.");
                SetCacheDirectory(defaultCacheDirectory);
                return defaultCacheDirectory;
            }
            else if (rawResult is string)
            {
                return (string) rawResult;
            }
            else
            {
                Logging.LogWarning("[NativeSettings->GetCacheDirectory] Cache Directory not a string. Defaulting.");
                SetCacheDirectory(defaultCacheDirectory);
                return defaultCacheDirectory;
            }
        }

        /// <summary>
        /// Set the Cache Directory.
        /// </summary>
        /// <param name="cacheDirectory">Cache Directory.</param>
        public void SetCacheDirectory(string cacheDirectory)
        {
            SetItem(cacheDirectoryKey, cacheDirectory);
        }

        /// <summary>
        /// Get the World Load Timeout.
        /// </summary>
        /// <returns>The World Load Timeout.</returns>
        public uint GetWorldLoadTimeout()
        {
            object rawResult = GetItem(worldLoadTimeoutKey);
            if (rawResult == null)
            {
                Logging.LogWarning("[NativeSettings->GetWorldLoadTimeout] World Load Timeout not set. Defaulting.");
                SetWorldLoadTimeout(defaultWorldLoadTimeout);
                return defaultWorldLoadTimeout;
            }
            else if (rawResult is long)
            {
                return (uint) ((long) rawResult);
            }
            else
            {
                Logging.LogWarning("[NativeSettings->GetWorldLoadTimeout] World Load Timeout not a long. Defaulting.");
                SetWorldLoadTimeout(defaultWorldLoadTimeout);
                return defaultWorldLoadTimeout;
            }
        }

        /// <summary>
        /// Set the World Load Timeout.
        /// </summary>
        /// <param name="worldLoadTimeout">World Load Timeout.</param>
        public void SetWorldLoadTimeout(uint worldLoadTimeout)
        {
            SetItem(worldLoadTimeoutKey, worldLoadTimeout);
        }

        /// <summary>
        /// Get the Tutorial State.
        /// </summary>
        /// <returns>The Tutorial State.</returns>
        public TutorialState GetTutorialState()
        {
            object rawResult = GetItem(tutorialStateKey);
            if (rawResult == null)
            {
                Logging.LogWarning("[NativeSettings->GetTutorialState] Tutorial State not set. Defaulting.");
                SetTutorialState((TutorialState) defaultTutorialState);
                return defaultTutorialState;
            }
            else if (rawResult is long)
            {
                return (TutorialState) ((long) rawResult);
            }
            else
            {
                Logging.LogWarning("[NativeSettings->GetTutorialState] Tutorial State not a long. Defaulting.");
                SetTutorialState(defaultTutorialState);
                return defaultTutorialState;
            }
        }

        /// <summary>
        /// Set the Tutorial State.
        /// </summary>
        /// <param name="tutorialState">Tutorial State.</param>
        public void SetTutorialState(TutorialState tutorialState)
        {
            SetItem(tutorialStateKey, tutorialState);
        }

        /// <summary>
        /// Set an item in settings.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        private void SetItem(string key, object value)
        {
#if !UNITY_WEBGL
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "DELETE FROM SETTINGS WHERE KEY=@KEY";
            cmd.Parameters.AddWithValue("@KEY", key);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "INSERT INTO SETTINGS(KEY,VALUE) VALUES(@KEY, @VALUE)";
            cmd.Parameters.AddWithValue("@KEY", key);
            cmd.Parameters.AddWithValue("@VALUE", value);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbConn.Close();
#endif
        }

        /// <summary>
        /// Get an item from settings.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>The entry corresponding to the key, or null if none exist.</returns>
        private object GetItem(string key)
        {
#if UNITY_WEBGL
            return null;
#else
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand("SELECT * FROM SETTINGS WHERE KEY='" + key + "'", dbConn);
            SqliteDataReader reader = cmd.ExecuteReader();

            List<object> readResults = new List<object>();
            while (reader.Read())
            {
                readResults.Add(reader.GetValue(1));
            }

            dbConn.Close();

            if (readResults.Count == 1)
            {
                return readResults[0];
            }
            else if (readResults.Count < 1)
            {
                return null;
            }
            else
            {
                Logging.LogWarning("[NativeSettings->GetItem] More than 1 result found.");
                return readResults[0];
            }
#endif
        }

        /// <summary>
        /// Remove an item from settings.
        /// </summary>
        /// <param name="key">Entry key.</param>
        private void RemoveItem(string key)
        {
#if !UNITY_WEBGL
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "DELETE FROM SETTINGS WHERE KEY=@KEY";
            cmd.Parameters.AddWithValue("@KEY", key);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbConn.Close();
#endif
        }

        /// <summary>
        /// Initialize the Settings Table.
        /// </summary>
        private void InitializeSettingsTable()
        {
#if !UNITY_WEBGL
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS SETTINGS (KEY,VALUE)";
            cmd.ExecuteNonQuery();

            dbConn.Close();

            GetHomeURL();
            GetMaxStorageEntries();
            GetMaxStorageKeyLength();
            GetMaxStorageEntryLength();
            GetCacheDirectory();
            GetWorldLoadTimeout();
            GetTutorialState();
#endif
        }

        /// <summary>
        /// Get the connection string for sqlite.
        /// </summary>
        /// <param name="path">Path to the sqlite table.</param>
        /// <returns>A connection string for the sqlite table.</returns>
        private string GetConnectionString(string path)
        {
            return "Data Source=" + path + "; Version=" + sqliteVersion + ";";
        }
    }
}