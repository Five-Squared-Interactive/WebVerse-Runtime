// Copyright (c) 2019-2024 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for World Storage.
    /// </summary>
    public class WorldStorage
    {
        /// <summary>
        /// Set an item in world storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public void SetItem(string key, string value)
        {
            WorldEngine.WorldEngine.ActiveWorld.storageManager.SetItem(key, value);
        }

        /// <summary>
        /// Get an item from world storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>The entry corresponding to the key, or null if none exist.</returns>
        public string GetItem(string key)
        {
            return WorldEngine.WorldEngine.ActiveWorld.storageManager.GetItem(key);
        }
    }
}