// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using TMPro;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Interface.Settings
{
    /// <summary>
    /// Class for settings menu.
    /// </summary>
    public class Settings : MonoBehaviour
    {
        /// <summary>
        /// Desktop mode script.
        /// </summary>
        public NativeSettings nativeSettings;

        /// <summary>
        /// Home URL input.
        /// </summary>
        public TMP_InputField homeURLInput;

        /// <summary>
        /// Storage mode dropdown.
        /// </summary>
        public TMP_Dropdown storageModeDropdown;

        /// <summary>
        /// Max storage entries input.
        /// </summary>
        public TMP_InputField maxStorageEntriesInput;

        /// <summary>
        /// Max storage key length input.
        /// </summary>
        public TMP_InputField maxStorageKeyLengthInput;

        /// <summary>
        /// Max storage entry length input.
        /// </summary>
        public TMP_InputField maxStorageEntryLengthInput;

        /// <summary>
        /// Cache directory input.
        /// </summary>
        public TMP_InputField cacheDirectoryInput;

        /// <summary>
        /// World load timeout input.
        /// </summary>
        public TMP_InputField worldLoadTimeoutInput;

        /// <summary>
        /// Initialize settings menu.
        /// </summary>
        public void Initialize()
        {
            homeURLInput.text = nativeSettings.GetHomeURL();
            string storageMode = nativeSettings.GetStorageMode();
            if (storageMode == "persistent")
            {
                storageModeDropdown.value = 0;
            }
            else if (storageMode == "cache")
            {
                storageModeDropdown.value = 1;
            }
            else
            {
                Logging.LogError("[Settings->Initialize] Invalid Storage Mode.");
            }
            maxStorageEntriesInput.text = nativeSettings.GetMaxStorageEntries().ToString();
            maxStorageKeyLengthInput.text = nativeSettings.GetMaxStorageKeyLength().ToString();
            maxStorageEntryLengthInput.text = nativeSettings.GetMaxStorageEntryLength().ToString();
            cacheDirectoryInput.text = nativeSettings.GetCacheDirectory();
            worldLoadTimeoutInput.text = nativeSettings.GetWorldLoadTimeout().ToString();
        }

        /// <summary>
        /// Terminate settings menu.
        /// </summary>
        public void Terminate()
        {

        }

        /// <summary>
        /// Return from settings menu.
        /// </summary>
        public void Return()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Update home.
        /// </summary>
        /// <param name="home">New home to use.</param>
        public void UpdateHome(string home)
        {
            if (!string.IsNullOrEmpty(home))
            {
                nativeSettings.SetHomeURL(home);
            }
        }

        /// <summary>
        /// Update storage mode.
        /// </summary>
        /// <param name="storageMode">New storage mode to use.</param>
        public void UpdateStorageMode(int storageMode)
        {
            string modeString;
            if (storageMode == 0)
            {
                modeString = "persistent";
            }
            else if (storageMode == 1)
            {
                modeString = "cache";
            }
            else
            {
                Logging.LogError("[Settings->UpdateStorageMode] Invalid storage mode.");
                return;
            }
            nativeSettings.SetStorageMode(modeString);
        }

        /// <summary>
        /// Update max entries.
        /// </summary>
        /// <param name="maxEntries">New max entries to use.</param>
        public void UpdateMaxEntries(string maxEntries)
        {
            if (!string.IsNullOrEmpty(maxEntries))
            {
                uint maxEntriesVal = uint.Parse(maxEntries);
                nativeSettings.SetMaxStorageEntries(maxEntriesVal);
            }
        }

        /// <summary>
        /// Update max key length.
        /// </summary>
        /// <param name="maxKeyLen">New max key length to use.</param>
        public void UpdateMaxKeyLen(string maxKeyLen)
        {
            if (!string.IsNullOrEmpty(maxKeyLen))
            {
                uint maxKeyLenVal = uint.Parse(maxKeyLen);
                nativeSettings.SetMaxStorageKeyLength(maxKeyLenVal);
            }
        }

        /// <summary>
        /// Update max entry length.
        /// </summary>
        /// <param name="maxEntryLen">New max entry length to use.</param>
        public void UpdateMaxEntryLen(string maxEntryLen)
        {
            if (!string.IsNullOrEmpty(maxEntryLen))
            {
                uint maxEntryLenVal = uint.Parse(maxEntryLen);
                nativeSettings.SetMaxStorageEntryLength(maxEntryLenVal);
            }
        }

        /// <summary>
        /// Update cache directory.
        /// </summary>
        /// <param name="cacheDirectory">New cache directory to use.</param>
        public void UpdateCacheDirectory(string cacheDirectory)
        {
            if (!string.IsNullOrEmpty(cacheDirectory))
            {
                nativeSettings.SetCacheDirectory(cacheDirectory);
            }
        }

        /// <summary>
        /// Update world load timeout.
        /// </summary>
        /// <param name="worldLoadTimeout">New world load timeout to use.</param>
        public void UpdateWorldLoadTimeout(string worldLoadTimeout)
        {
            if (!string.IsNullOrEmpty(worldLoadTimeout))
            {
                uint worldLoadTimeoutVal = uint.Parse(worldLoadTimeout);
                nativeSettings.SetWorldLoadTimeout(worldLoadTimeoutVal);
            }
        }
    }
}