// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.WebVerse.Input.Quest3;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// MonoBehaviour wrapper for Quest3PerformanceConfig.
    /// Attach this to a GameObject to automatically apply Quest 3 performance settings on start.
    /// </summary>
    public class Quest3PerformanceInitializer : MonoBehaviour
    {
        /// <summary>
        /// Whether to apply settings automatically on Awake.
        /// </summary>
        [Tooltip("Apply performance settings automatically on Awake.")]
        public bool applyOnAwake = true;

        /// <summary>
        /// Whether to only apply on Quest 3 platform.
        /// If false, will attempt to apply on any platform (useful for testing).
        /// </summary>
        [Tooltip("Only apply settings when running on Quest 3.")]
        public bool quest3Only = true;

        /// <summary>
        /// Whether to log current settings after applying.
        /// </summary>
        [Tooltip("Log settings after applying.")]
        public bool logSettings = false;

        private void Awake()
        {
            if (applyOnAwake)
            {
                ApplySettings();
            }
        }

        /// <summary>
        /// Manually apply Quest 3 performance settings.
        /// </summary>
        public void ApplySettings()
        {
            if (quest3Only && !Quest3PlatformDetector.IsQuest3Platform())
            {
                return;
            }

            Quest3PerformanceConfig.Apply();

            if (logSettings)
            {
                Quest3PerformanceConfig.LogCurrentSettings();
            }
        }
    }
}
