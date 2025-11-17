// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace FiveSQD.WebVerse.LocalStorage
{
    /// <summary>
    /// Controller for an instance of cache storage.
    /// </summary>
    public class CacheStorageController : BaseStorageController
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
        /// Dictionary containing cache storage.
        /// </summary>
        private Dictionary<string, string> cacheDictionary;

        /// <summary>
        /// Initialize the storage controller.
        /// </summary>
        /// <param name="maxEntries">Maximum number of entries in storage.</param>
        /// <param name="maxEntryLength">Maximum length of a storage entry.</param>
        /// <param name="maxKeyLength">Maximum length of a storage key.</param>
        public void Initialize(int maxEntries, int maxEntryLength, int maxKeyLength)
        {
            base.Initialize();
            this.maxEntries = maxEntries;
            this.maxEntryLength = maxEntryLength;
            this.maxKeyLength = maxKeyLength;
            cacheDictionary = new Dictionary<string, string>();
        }

        /// <summary>
        /// Terminate the storage controller.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();
        }

        /// <summary>
        /// Set an item in cache storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public override void SetItem(string key, string value)
        {
            if (cacheDictionary == null)
            {
                Logging.LogError("[CacheStorageController->SetItem] Cache Storage not initialized.");
                return;
            }

            if (cacheDictionary.Count >= maxEntries)
            {
                // If this is not the case, the dictionary won't grow.
                if (!cacheDictionary.ContainsKey(key))
                {
                    Logging.LogWarning("[CacheStorageController->SetItem] Cache Storage full.");
                    return;
                }
            }

            key = RestrictSize(key, maxKeyLength);

            value = RestrictSize(value, maxEntryLength);

            cacheDictionary[key] = value;
        }

        /// <summary>
        /// Get an item from cache storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>The entry corresponding to the key, or null if none exist.</returns>
        public override string GetItem(string key)
        {
            if (cacheDictionary == null)
            {
                Logging.LogError("[CacheStorageController->GetItem] Cache Storage not initialized.");
                return null;
            }

            if (key.Length > maxKeyLength)
            {
                Logging.LogWarning("[CacheStorageController->GetItem] Invalid key: too long.");
                return null;
            }

            if (!cacheDictionary.ContainsKey(key))
            {
                return null;
            }

            return cacheDictionary[key];
        }

        /// <summary>
        /// Remove an item from storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        public override void RemoveItem(string key)
        {
            if (cacheDictionary == null)
            {
                Logging.LogError("[CacheStorageController->RemoveItem] Cache Storage not initialized.");
                return;
            }

            if (key.Length > maxKeyLength)
            {
                Logging.LogWarning("[CacheStorageController->RemoveItem] Invalid key: too long.");
                return;
            }

            if (!cacheDictionary.ContainsKey(key))
            {
                return;
            }

            cacheDictionary.Remove(key);
        }

        /// <summary>
        /// Clear all items from storage.
        /// </summary>
        public override void Clear()
        {
            if (cacheDictionary == null)
            {
                Logging.LogError("[CacheStorageController->Clear] Cache Storage not initialized.");
                return;
            }

            cacheDictionary.Clear();
        }

        /// <summary>
        /// Get the key of an index in storage.
        /// </summary>
        /// <param name="index">Index for which to get the key.</param>
        /// <returns>The key corresponding to the index, or null if the index does not exist.</returns>
        public override string Key(int index)
        {
            if (cacheDictionary == null)
            {
                Logging.LogError("[CacheStorageController->Key] Cache Storage not initialized.");
                return null;
            }

            if (cacheDictionary.Count <= index)
            {
                return null;
            }

            KeyValuePair<string, string> entry = cacheDictionary.ElementAt(index);
            return entry.Key;
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