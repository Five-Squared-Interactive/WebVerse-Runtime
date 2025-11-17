// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for local storage.
    /// </summary>
    public class LocalStorage
    {
        /// <summary>
        /// Set an item in world storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public static void SetItem(string key, string value)
        {
            WebVerseRuntime.Instance.localStorageManager.SetItem(
                StraightFour.StraightFour.ActiveWorld.siteName, key, value);
        }

        /// <summary>
        /// Get an item from world storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>The entry corresponding to the key, or null if none exist.</returns>
        public static string GetItem(string key)
        {
            return WebVerseRuntime.Instance.localStorageManager.GetItem(
                StraightFour.StraightFour.ActiveWorld.siteName, key);
        }

        /// <summary>
        /// Remove an item from local storage.
        /// </summary>
        /// <param name="key">Key of the item to remove.</param>
        public static void RemoveItem(string key)
        {
            WebVerseRuntime.Instance.localStorageManager.RemoveItem(
                StraightFour.StraightFour.ActiveWorld.siteName, key);
        }
    }
}