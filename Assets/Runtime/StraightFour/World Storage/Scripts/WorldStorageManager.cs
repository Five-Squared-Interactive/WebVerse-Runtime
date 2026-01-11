// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using FiveSQD.StraightFour.Utilities;

namespace FiveSQD.StraightFour.WorldStorage
{
    /// <summary>
    /// Class for the World Storage Manager.
    /// </summary>
    public class WorldStorageManager : BaseManager
    {
        /// <summary>
        /// Maximum number of entries for world storage.
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
        /// Dictionary containing world storage.
        /// </summary>
        private Dictionary<string, string> storageDictionary;

        /// <summary>
        /// Initialize the world storage.
        /// </summary>
        /// <param name="maxEntries">Maximum number of entries.</param>
        /// <param name="maxEntryLength">Maximum length of an entry.</param>
        /// <param name="maxKeyLength">Maximum length of a key.</param>
        public void Initialize(int maxEntries, int maxEntryLength, int maxKeyLength)
        {
            base.Initialize();
            this.maxEntries = maxEntries;
            this.maxEntryLength = maxEntryLength;
            this.maxKeyLength = maxKeyLength;
            storageDictionary = new Dictionary<string, string>();
        }

        /// <summary>
        /// Terminate the world storage manager.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();
            storageDictionary.Clear();
        }

        /// <summary>
        /// Set an item in world storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public void SetItem(string key, string value)
        {
            if (storageDictionary == null)
            {
                LogSystem.LogError("[WorldStorageManager->SetItem] World Storage not initialized.");
                return;
            }

            if (storageDictionary.Count >= maxEntries)
            {
                LogSystem.LogWarning("[WorldStorageManager->SetItem] World Storage full.");
                return;
            }

            key = RestrictSize(key, maxKeyLength);

            value = RestrictSize(value, maxEntryLength);

            storageDictionary[key] = value;
        }

        /// <summary>
        /// Get an item from world storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>The entry corresponding to the key, or null if none exist.</returns>
        public string GetItem(string key)
        {
            if (storageDictionary == null)
            {
                LogSystem.LogError("[WorldStorageManager->GetItem] World Storage not initialized.");
                return null;
            }

            if (key.Length > maxKeyLength)
            {
                LogSystem.LogWarning("[WorldStorageManager->GetItem] Invalid key: too long.");
                return null;
            }

            if (!storageDictionary.ContainsKey(key))
            {
                return null;
            }

            return storageDictionary[key];
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