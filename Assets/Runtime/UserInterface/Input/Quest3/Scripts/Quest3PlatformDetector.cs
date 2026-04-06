// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.XR;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Utility class for detecting Quest 3 platform at runtime.
    /// </summary>
    public static class Quest3PlatformDetector
    {
        /// <summary>
        /// Cached detection result to avoid repeated checks.
        /// </summary>
        private static bool? cachedResult = null;

        /// <summary>
        /// Checks if the current platform is Meta Quest 3.
        /// </summary>
        /// <returns>True if running on Quest 3, false otherwise.</returns>
        public static bool IsQuest3Platform()
        {
            // Return cached result if available
            if (cachedResult.HasValue)
            {
                return cachedResult.Value;
            }

            cachedResult = DetectQuest3();
            return cachedResult.Value;
        }

        /// <summary>
        /// Forces a re-detection of the platform.
        /// Useful if XR subsystem initialization is delayed.
        /// </summary>
        /// <returns>True if running on Quest 3, false otherwise.</returns>
        public static bool RedetectPlatform()
        {
            cachedResult = null;
            return IsQuest3Platform();
        }

        /// <summary>
        /// Performs the actual Quest 3 detection.
        /// </summary>
        /// <returns>True if Quest 3 is detected, false otherwise.</returns>
        private static bool DetectQuest3()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Check if XR is enabled and a device is active
            if (!XRSettings.enabled)
            {
                Logging.Log("[Quest3PlatformDetector] XR not enabled.");
                return false;
            }

            if (!XRSettings.isDeviceActive)
            {
                Logging.Log("[Quest3PlatformDetector] No XR device active.");
                return false;
            }

            // Check the loaded device name for Quest/Oculus/OpenXR
            string loadedDeviceName = XRSettings.loadedDeviceName;
            if (string.IsNullOrEmpty(loadedDeviceName))
            {
                Logging.Log("[Quest3PlatformDetector] No XR device loaded.");
                return false;
            }

            string deviceNameLower = loadedDeviceName.ToLower();
            bool isMetaDevice = deviceNameLower.Contains("oculus") ||
                                deviceNameLower.Contains("quest") ||
                                deviceNameLower.Contains("meta") ||
                                deviceNameLower.Contains("openxr");

            if (isMetaDevice)
            {
                // Additional check: verify it's Quest 3 specifically via system info
                string deviceModel = SystemInfo.deviceModel.ToLower();
                string deviceName = SystemInfo.deviceName.ToLower();

                // Quest 3 device model contains "Quest 3" or similar
                bool isQuest3 = deviceModel.Contains("quest 3") ||
                                deviceModel.Contains("quest3") ||
                                deviceName.Contains("quest 3") ||
                                deviceName.Contains("quest3");

                // If we can't determine it's specifically Quest 3,
                // but it's a Meta device on Android with OpenXR, assume it's compatible
                // Quest 2 and Quest Pro will also work with this code
                if (!isQuest3)
                {
                    Logging.Log($"[Quest3PlatformDetector] Meta device detected ({deviceModel}), treating as Quest-compatible.");
                    return true;
                }

                Logging.Log("[Quest3PlatformDetector] Quest 3 detected.");
                return true;
            }

            Logging.Log($"[Quest3PlatformDetector] Non-Quest device: {loadedDeviceName}");
            return false;
#else
            // Not Android or running in Editor - not Quest 3
            return false;
#endif
        }

        /// <summary>
        /// Checks if any Meta Quest device is detected (Quest 2, Quest 3, Quest Pro).
        /// </summary>
        /// <returns>True if any Quest device is detected.</returns>
        public static bool IsAnyQuestPlatform()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!XRSettings.enabled || !XRSettings.isDeviceActive)
            {
                return false;
            }

            string loadedDeviceName = XRSettings.loadedDeviceName;
            if (string.IsNullOrEmpty(loadedDeviceName))
            {
                return false;
            }

            string deviceNameLower = loadedDeviceName.ToLower();
            return deviceNameLower.Contains("oculus") ||
                   deviceNameLower.Contains("quest") ||
                   deviceNameLower.Contains("meta") ||
                   deviceNameLower.Contains("openxr");
#else
            return false;
#endif
        }

        /// <summary>
        /// Gets the detected Quest device model name.
        /// </summary>
        /// <returns>Device model name, or empty string if not on Quest.</returns>
        public static string GetQuestDeviceModel()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (IsAnyQuestPlatform())
            {
                return SystemInfo.deviceModel;
            }
#endif
            return string.Empty;
        }
    }
}
