// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if !UNITY_WEBGL
using Mono.Data.Sqlite;
#endif
using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using System;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// Class for Native History.
    /// </summary>
    public class NativeHistory : MonoBehaviour
    {
        /// <summary>
        /// Version number for sqlite.
        /// </summary>
        private string sqliteVersion;

        /// <summary>
        /// Path to the sqlite database.
        /// </summary>
        private string dbPath;

        /// <summary>
        /// Initialize Native History.
        /// </summary>
        /// <param name="sqliteVersion">SQLite version.</param>
        /// <param name="pathToDB">Path to SQLite database.</param>
        public void Initialize(string sqliteVersion, string pathToDB)
        {
            this.sqliteVersion = sqliteVersion;
            dbPath = pathToDB;
            InitializeHistoryTable();
        }

        /// <summary>
        /// Terminate Native History.
        /// </summary>
        public void Terminate()
        {

        }

        /// <summary>
        /// Clear Native History.
        /// </summary>
        public void ClearHistory()
        {
            RemoveAllItems();
        }

        /// <summary>
        /// Add Item to Native History.
        /// </summary>
        /// <param name="timestamp">Timestamp.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="siteURL">Site URL.</param>
        public void AddItemToHistory(DateTime timestamp, string siteName, string siteURL)
        {
            SetItem(((DateTimeOffset) timestamp).ToUnixTimeSeconds(), siteName, siteURL);
        }

        /// <summary>
        /// Get All Items from Native History.
        /// </summary>
        /// <returns>Array of Native History Entries, Each Containing Timestamp, Site Name, and Site URL.</returns>
        public Tuple<DateTime, string, string>[] GetAllItemsFromHistory()
        {
            Tuple<long, string, string>[] items = GetAllItems();

            if (items == null)
            {
                return new Tuple<DateTime, string, string>[0];
            }

            List<Tuple<DateTime, string, string>> returnList = new List<Tuple<DateTime, string, string>>();
            foreach (Tuple<long, string, string> item in items)
            {
                if (item != null)
                {
                    returnList.Add(new Tuple<DateTime, string, string>(
                        DateTimeOffset.FromUnixTimeSeconds(item.Item1).UtcDateTime, item.Item2, item.Item3));
                }
            }

            return returnList.ToArray();
        }

        /// <summary>
        /// Set an Item in Native History.
        /// </summary>
        /// <param name="timestamp">Entry timestamp.</param>
        /// <param name="siteName">Entry site name.</param>
        /// <param name="siteURL">Entry site URL.</param>
        private void SetItem(long timestamp, string siteName, string siteURL)
        {
#if !UNITY_WEBGL
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "DELETE FROM HISTORY WHERE TIMESTAMP=@TIMESTAMP";
            cmd.Parameters.AddWithValue("@TIMESTAMP", timestamp);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "INSERT INTO HISTORY(TIMESTAMP,SITENAME,SITEURL) VALUES(@TIMESTAMP, @SITENAME, @SITEURL)";
            cmd.Parameters.AddWithValue("@TIMESTAMP", timestamp);
            cmd.Parameters.AddWithValue("@SITENAME", siteName);
            cmd.Parameters.AddWithValue("@SITEURL", siteURL);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbConn.Close();
#endif
        }

        /// <summary>
        /// Get an Item from Native History.
        /// </summary>
        /// <param name="timestamp">Entry timestamp.</param>
        /// <returns>The entry corresponding to the timestamp, or null if none exist.</returns>
        private object GetItem(long timestamp)
        {
#if UNITY_WEBGL
            return null;
#else
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand("SELECT * FROM HISTORY WHERE TIMESTAMP='" + timestamp + "'", dbConn);
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
                Logging.LogWarning("[NativeHistory->GetItem] More than 1 result found.");
                return readResults[0];
            }
#endif
        }

        /// <summary>
        /// Get all Items from Native History.
        /// </summary>
        /// <returns>All entries, or null if none exist.</returns>
        private Tuple<long, string, string>[] GetAllItems()
        {
#if UNITY_WEBGL
            return null;
#else
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand("SELECT * FROM HISTORY", dbConn);
            SqliteDataReader reader = cmd.ExecuteReader();

            List<Tuple<long, string, string>> readResults = new List<Tuple<long, string, string>>();
            while (reader.Read())
            {
                readResults.Add(new Tuple<long, string, string>(reader.GetInt64(0), reader.GetString(1), reader.GetString(2)));
            }

            dbConn.Close();

            if (readResults.Count < 1)
            {
                return null;
            }
            else
            {
                return readResults.ToArray();
            }
#endif
        }

        /// <summary>
        /// Remove an Item from Native History.
        /// </summary>
        /// <param name="timestamp">Entry timestamp.</param>
        private void RemoveItem(long timestamp)
        {
#if !UNITY_WEBGL
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "DELETE FROM HISTORY WHERE TIMESTAMP=@TIMESTAMP";
            cmd.Parameters.AddWithValue("@TIMESTAMP", timestamp);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbConn.Close();
#endif
        }

        /// <summary>
        /// Remove all Items from Native History.
        /// </summary>
        private void RemoveAllItems()
        {
#if !UNITY_WEBGL
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "DELETE FROM HISTORY";
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbConn.Close();
#endif
        }
        
        /// <summary>
        /// Initialize the History Table.
        /// </summary>
        private void InitializeHistoryTable()
        {
#if !UNITY_WEBGL
            SqliteConnection dbConn = new SqliteConnection(GetConnectionString(dbPath));
            dbConn.Open();

            SqliteCommand cmd = new SqliteCommand(dbConn);
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS HISTORY (TIMESTAMP,SITENAME,SITEURL)";
            cmd.ExecuteNonQuery();

            dbConn.Close();
#endif
        }

        /// <summary>
        /// Get the Connection String for SQLite.
        /// </summary>
        /// <param name="path">Path to the sqlite table.</param>
        /// <returns>A connection string for the sqlite table.</returns>
        private string GetConnectionString(string path)
        {
            return "Data Source=" + path + "; Version=" + sqliteVersion + ";";
        }
    }
}