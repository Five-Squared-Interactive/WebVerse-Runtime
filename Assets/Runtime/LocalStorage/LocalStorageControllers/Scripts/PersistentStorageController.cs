// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

#if !UNITY_WEBGL || UNITY_EDITOR
using FiveSQD.WebVerse.Utilities;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace FiveSQD.WebVerse.LocalStorage
{
    /// <summary>
    /// Controller for an instance of persistent storage.
    /// </summary>
    public class PersistentStorageController : BaseStorageController
    {
        /// <summary>
        /// Maximum number of entries for cache storage.
        /// </summary>
        private int maxEntries;

        /// <summary>
        /// Maximum length of a storage entry.
        /// </summary>
        private int maxEntryLength;

        /// <summary>
        /// Maximum length of a storage key.
        /// </summary>
        private int maxKeyLength;

        /// <summary>
        /// Version number for sqlite.
        /// </summary>
        private string sqliteVersion;

        /// <summary>
        /// Path to the sqlite table.
        /// </summary>
        private string tablePath;

        /// <summary>
        /// Initialize the storage controller.
        /// </summary>
        /// <param name="maxEntries">Maximum number of entries in storage.</param>
        /// <param name="maxEntryLength">Maximum length of a storage entry.</param>
        /// <param name="maxKeyLength">Maximum length of a storage key.</param>
        /// <param name="sqliteVersion">Version number for sqlite.</param>
        /// <param name="pathToTable">Path to sqlite table.</param>
        public void Initialize(int maxEntries, int maxEntryLength, int maxKeyLength,
            string sqliteVersion, string pathToTable)
        {
            base.Initialize();
            this.maxEntries = maxEntries;
            this.maxEntryLength = maxEntryLength;
            this.maxKeyLength = maxKeyLength;
            this.sqliteVersion = sqliteVersion;
            tablePath = pathToTable;
            InitializeTable();
        }

        /// <summary>
        /// Terminate the storage controller.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();
        }

        /// <summary>
        /// Set an item in persistent storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public override void SetItem(string key, string value)
        {
            if (GetEntryCount() >= maxEntries)
            {
                // If not the case, the table won't grow.
                if (GetItem(key) == null)
                {
                    Logging.LogWarning("[PersistentStorageController->SetItem] Persistent Storage full.");
                    return;
                }
            }

            key = RestrictSize(key, maxKeyLength);

            value = RestrictSize(value, maxEntryLength);

            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(tablePath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "DELETE FROM STORAGE WHERE KEY=@KEY";
            cmd.Parameters.AddWithValue("@KEY", key);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "INSERT INTO STORAGE(KEY,VALUE) VALUES(@KEY, @VALUE)";
            cmd.Parameters.AddWithValue("@KEY", key);
            cmd.Parameters.AddWithValue("@VALUE", value);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbConn.Close();
        }

        /// <summary>
        /// Get an item from persistent storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>The entry corresponding to the key, or null if none exist.</returns>
        public override string GetItem(string key)
        {
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(tablePath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand("SELECT * FROM STORAGE WHERE KEY='" + key + "'", dbConn);
            SqliteDataReader reader = cmd.ExecuteReader();

            List<string> readResults = new List<string>();
            while (reader.Read())
            {
                readResults.Add(reader.GetString(1));
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
                Logging.LogWarning("[PersistentStorageController->GetItem] More than 1 result found.");
                return readResults[0];
            }
        }

        /// <summary>
        /// Remove an item from storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        public override void RemoveItem(string key)
        {
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(tablePath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "DELETE FROM STORAGE WHERE KEY=@KEY";
            cmd.Parameters.AddWithValue("@KEY", key);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbConn.Close();
        }

        /// <summary>
        /// Clear all items from storage.
        /// </summary>
        public override void Clear()
        {
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(tablePath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "DELETE FROM STORAGE";
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbConn.Close();
        }

        /// <summary>
        /// Get the key of an index in storage.
        /// </summary>
        /// <param name="index">Index for which to get the key.</param>
        /// <returns>The key corresponding to the index, or null if the index does not exist.</returns>
        public override string Key(int index)
        {
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(tablePath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand("SELECT KEY FROM STORAGE LIMIT 1 OFFSET " + index, dbConn);
            SqliteDataReader reader = cmd.ExecuteReader();

            List<string> readResults = new List<string>();
            while (reader.Read())
            {
                readResults.Add(reader.GetString(0));
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
                Logging.LogWarning("[PersistentStorageController->Key] More than 1 result found.");
                return readResults[0];
            }
        }

        /// <summary>
        /// Get the count of entries in storage.
        /// </summary>
        /// <returns>The count of entries in storage.</returns>
        private int GetEntryCount()
        {
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(tablePath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand("SELECT COUNT(*) FROM STORAGE", dbConn);
            SqliteDataReader reader = cmd.ExecuteReader();

            List<int> readResults = new List<int>();
            while (reader.Read())
            {
                readResults.Add(reader.GetInt32(0));
            }

            dbConn.Close();

            if (readResults.Count == 1)
            {
                return readResults[0];
            }
            else if (readResults.Count < 1)
            {
                Logging.LogWarning("[PersistentStorageController->GetEntryCount] Unable to get count.");
                return 0;
            }
            else
            {
                Logging.LogWarning("[PersistentStorageController->GetEntryCount] More than 1 result for count found.");
                return readResults[0];
            }
        }

        /// <summary>
        /// Initialize the sqlite table.
        /// </summary>
        private void InitializeTable()
        {
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(tablePath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS STORAGE (KEY,VALUE)";
            cmd.ExecuteNonQuery();

            dbConn.Close();
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

        /// <summary>
        /// Restrict the size of a string.
        /// </summary>
        /// <param name="str">Raw string.</param>
        /// <param name="maxSize">Maximum size for the string.</param>
        /// <returns>The restricted string.</returns>
        private string RestrictSize(string str, int maxSize)
        {
            if (str.Length > maxSize)
            {
                return str.Substring(0, maxSize);
            }
            else
            {
                return str;
            }
        }
    }
}
#endif