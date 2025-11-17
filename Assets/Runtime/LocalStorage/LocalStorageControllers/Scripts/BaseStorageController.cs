// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.LocalStorage
{
    /// <summary>
    /// Base class for a storage controller.
    /// All methods must be overriden, and the base methods should not be called.
    /// </summary>
    public class BaseStorageController : BaseController
    {
        /// <summary>
        /// Set an item in storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public virtual void SetItem(string key, string value)
        {
            Logging.LogError("[BaseStorageController->SetItem] Storage controller does not implement a SetItem() method.");
        }

        /// <summary>
        /// Get an item from storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <returns>The entry corresponding to the key, or null if none exist.</returns>
        public virtual string GetItem(string key)
        {
            Logging.LogError("[BaseStorageController->GetItem] Storage controller does not implement a GetItem() method.");

            return null;
        }

        /// <summary>
        /// Remove an item from storage.
        /// </summary>
        /// <param name="key">Entry key.</param>
        public virtual void RemoveItem(string key)
        {
            Logging.LogError("[BaseStorageController->RemoveItem] Storage controller does not implement a RemoveItem() method.");

            return;
        }

        /// <summary>
        /// Clear all items from storage.
        /// </summary>
        public virtual void Clear()
        {
            Logging.LogError("[BaseStorageController->Clear] Storage controller does not implement a Clear() method.");

            return;
        }

        /// <summary>
        /// Get the key of an index in storage.
        /// </summary>
        /// <param name="index">Index for which to get the key.</param>
        /// <returns>The key corresponding to the index, or null if the index does not exist.</returns>
        public virtual string Key(int index)
        {
            Logging.LogError("[BaseStorageController->Key] Storage controller does not implement a Key() method.");

            return null;
        }
    }
}