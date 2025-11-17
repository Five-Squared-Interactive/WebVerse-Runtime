// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.LocalStorage
{
    /// <summary>
    /// Manager for local storage.
    /// </summary>
    public class LocalStorageManager : BaseManager
    {
        /// <summary>
        /// Enumeration for the local storage mode.
        /// </summary>
        public enum LocalStorageMode { Uninitialized, Cache, Persistent }

        /// <summary>
        /// Maximum number of entries.
        /// </summary>
        public int maxEntries { get; private set; }

        /// <summary>
        /// Maximum length of an entry.
        /// </summary>
        public int maxEntryLength { get; private set; }

        /// <summary>
        /// Maximum length of a key.
        /// </summary>
        public int maxKeyLength { get; private set; }

        /// <summary>
        /// The current mode that storage is in.
        /// </summary>
        private LocalStorageMode storageMode = LocalStorageMode.Uninitialized;

        /// <summary>
        /// Dictionary of storage controllers and their corresponding sites.
        /// </summary>
        private Dictionary<string, BaseStorageController> storageControllers;

        /// <summary>
        /// Root gameobject for the storage controllers.
        /// </summary>
        private GameObject storageControllersGO;

        /// <summary>
        /// Initialize the local storage manager.
        /// </summary>
        /// <param name="mode">Mode to set local storage to. Must be cache or persistent.</param>
        /// <param name="maxEntries">Maximum number of entries.</param>
        /// <param name="maxEntryLength">Maximum length of an entry.</param>
        /// <param name="maxKeyLength">Maximum length of a key.</param>
        public void Initialize(LocalStorageMode mode, int maxEntries, int maxEntryLength, int maxKeyLength)
        {
            base.Initialize();

            if (mode == LocalStorageMode.Uninitialized)
            {
                Logging.LogError("[LocalStorageManager->Initialize] Invalid storage mode: Uninitialized.");
                return;
            }
            storageMode = mode;
            storageControllersGO = new GameObject("StorageControllers");
            storageControllersGO.transform.SetParent(transform);
            storageControllers = new Dictionary<string, BaseStorageController>();
            this.maxEntries = maxEntries;
            this.maxEntryLength = maxEntryLength;
            this.maxKeyLength = maxKeyLength;
        }

        /// <summary>
        /// Terminate the local storage manager.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();
            foreach (BaseStorageController sc in storageControllers.Values)
            {
                sc.Terminate();
            }
            storageControllers = new Dictionary<string, BaseStorageController>();
            Destroy(storageControllersGO);
            storageMode = LocalStorageMode.Uninitialized;
        }

        /// <summary>
        /// Add a site to local storage.
        /// </summary>
        /// <param name="site">Name of the site.</param>
        public void AddSite(string site)
        {
            if (storageControllers == null)
            {
                Logging.LogError("[LocalStorageManager->AddSite] Local storage manager not initialized.");
                return;
            }

            if (storageControllers.ContainsKey(site))
            {
                Logging.LogError("[LocalStorageManager->AddSite] Local storage manager already contains site: " + site + ".");
                return;
            }

            GameObject scGO = new GameObject(site);
            scGO.transform.SetParent(storageControllersGO.transform);
            switch (storageMode)
            {
                case LocalStorageMode.Uninitialized:
                    Logging.LogError("[LocalStorageManager->AddSite] Cannot add site in uninitialized state.");
                    return;

                case LocalStorageMode.Cache:
                    CacheStorageController cSC = scGO.AddComponent<CacheStorageController>();
                    storageControllers.Add(site, cSC);
                    cSC.Initialize(maxEntries, maxEntryLength, maxKeyLength);
                    return;
#if !UNITY_WEBGL || UNITY_EDITOR
                case LocalStorageMode.Persistent:
                    PersistentStorageController pSC = scGO.AddComponent<PersistentStorageController>();
                    storageControllers.Add(site, pSC);
                    pSC.Initialize(maxEntries, maxEntryLength, maxKeyLength, "3", site + ".db");
                    return;
#endif
                default:
                    return;
            }
        }

        /// <summary>
        /// Set an item in world storage.
        /// </summary>
        /// <param name="site">Site to set item in's storage.</param>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public void SetItem(string site, string key, string value)
        {
            if (storageControllers == null)
            {
                Logging.LogError("[LocalStorageManager->SetItem] Local storage manager not initialized.");
                return;
            }

            if (!storageControllers.ContainsKey(site))
            {
                Logging.LogError("[LocalStorageManager->SetItem] Local storage manager does not contain site: " + site + ".");
                return;
            }

            if (storageMode != LocalStorageMode.Cache && storageMode != LocalStorageMode.Persistent)
            {
                Logging.LogError("[LocalStorageManager->SetItem] Invalid state: " + storageMode + ". Cannot set item.");
                return;
            }

            storageControllers[site].SetItem(key, value);
        }

        /// <summary>
        /// Get an item from world storage.
        /// </summary>
        /// <param name="site">Site to get item from's storage</param>
        /// <param name="key">Entry key.</param>
        /// <returns>The entry corresponding to the key, or null if none exist.</returns>
        public string GetItem(string site, string key)
        {
            if (storageControllers == null)
            {
                Logging.LogError("[LocalStorageManager->GetItem] Local storage manager not initialized.");
                return null;
            }

            if (!storageControllers.ContainsKey(site))
            {
                Logging.LogError("[LocalStorageManager->GetItem] Local storage manager does not contain site: " + site + ".");
                return null;
            }

            if (storageMode != LocalStorageMode.Cache && storageMode != LocalStorageMode.Persistent)
            {
                Logging.LogError("[LocalStorageManager->GetItem] Invalid state: " + storageMode + ". Cannot get item.");
                return null;
            }

            return storageControllers[site].GetItem(key);
        }

        /// <summary>
        /// Remove an item from local storage.
        /// </summary>
        /// <param name="site">Site of the item to remove.</param>
        /// <param name="key">Key of the item to remove.</param>
        public void RemoveItem(string site, string key)
        {
            if (storageControllers == null)
            {
                Logging.LogError("[LocalStorageManager->RemoveItem] Local storage manager not initialized.");
                return;
            }

            if (!storageControllers.ContainsKey(site))
            {
                Logging.LogError("[LocalStorageManager->RemoveItem] Local storage manager does not contain site: " + site + ".");
                return;
            }

            if (storageMode != LocalStorageMode.Cache && storageMode != LocalStorageMode.Persistent)
            {
                Logging.LogError("[LocalStorageManager->RemoveItem] Invalid state: " + storageMode + ". Cannot remove item.");
                return;
            }

            storageControllers[site].RemoveItem(key);
        }

        /// <summary>
        /// Clear local storage at a site.
        /// </summary>
        /// <param name="site">Site to clear.</param>
        public void Clear(string site)
        {
            if (storageControllers == null)
            {
                Logging.LogError("[LocalStorageManager->Clear] Local storage manager not initialized.");
                return;
            }

            if (!storageControllers.ContainsKey(site))
            {
                Logging.LogError("[LocalStorageManager->Clear] Local storage manager does not contain site: " + site + ".");
                return;
            }

            if (storageMode != LocalStorageMode.Cache && storageMode != LocalStorageMode.Persistent)
            {
                Logging.LogError("[LocalStorageManager->Clear] Invalid state: " + storageMode + ". Cannot clear.");
                return;
            }

            storageControllers[site].Clear();
        }

        /// <summary>
        /// Get the key of an index in storage.
        /// </summary>
        /// <param name="site">Site to get key from.</param>
        /// <param name="index">Index to get key for.</param>
        /// <returns></returns>
        public string Key(string site, int index)
        {
            if (storageControllers == null)
            {
                Logging.LogError("[LocalStorageManager->Key] Local storage manager not initialized.");
                return null;
            }

            if (!storageControllers.ContainsKey(site))
            {
                Logging.LogError("[LocalStorageManager->Key] Local storage manager does not contain site: " + site + ".");
                return null;
            }

            if (storageMode != LocalStorageMode.Cache && storageMode != LocalStorageMode.Persistent)
            {
                Logging.LogError("[LocalStorageManager->Key] Invalid state: " + storageMode + ". Cannot get key.");
                return null;
            }

            return storageControllers[site].Key(index);
        }
    }
}