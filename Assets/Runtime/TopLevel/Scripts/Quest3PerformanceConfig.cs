// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// Configures Quest 3 performance settings for optimal VR experience.
    /// Uses Unity XR subsystems and OVRPlugin for Quest-specific optimizations.
    /// </summary>
    public static class Quest3PerformanceConfig
    {
        /// <summary>
        /// Target display frequency for Quest 3.
        /// </summary>
        public const float TargetDisplayFrequency = 72.0f;

        /// <summary>
        /// Whether performance settings have been applied.
        /// </summary>
        private static bool isConfigured = false;

        /// <summary>
        /// Applies optimal performance settings for Quest 3.
        /// Should be called early in initialization, before world loading.
        /// </summary>
        public static void Apply()
        {
            if (isConfigured)
            {
                Logging.LogWarning("[Quest3PerformanceConfig] Performance settings already applied.");
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            ApplyAndroidPerformanceSettings();
#else
            Logging.Log("[Quest3PerformanceConfig] Not on Android - skipping Quest performance settings.");
#endif

            isConfigured = true;
        }

#if UNITY_ANDROID
        /// <summary>
        /// Applies Android/Quest-specific performance settings.
        /// </summary>
        private static void ApplyAndroidPerformanceSettings()
        {
            try
            {
                // Configure display frequency using Unity XR
                ConfigureDisplayFrequency();

                // Configure foveated rendering using OVRPlugin
                ConfigureFoveatedRendering();

                // Configure CPU/GPU performance levels using OVRPlugin
                ConfigurePerformanceLevels();

                // Configure additional optimizations
                ConfigureAdditionalOptimizations();

                Logging.Log("[Quest3PerformanceConfig] Quest 3 performance settings applied successfully.");
            }
            catch (System.Exception ex)
            {
                Logging.LogError($"[Quest3PerformanceConfig] Failed to apply performance settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures the display refresh rate via OVRPlugin.
        /// </summary>
        private static void ConfigureDisplayFrequency()
        {
            try
            {
                // OVRPlugin is a static class in the Meta XR SDK
                var ovrPluginType = System.Type.GetType("OVRPlugin, Assembly-CSharp") ??
                                    System.Type.GetType("OVRPlugin, Oculus.VR") ??
                                    System.Type.GetType("OVRPlugin, Meta.XR.Core");

                if (ovrPluginType != null)
                {
                    var systemDisplayFrequencyProp = ovrPluginType.GetProperty("systemDisplayFrequency",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (systemDisplayFrequencyProp != null && systemDisplayFrequencyProp.CanWrite)
                    {
                        systemDisplayFrequencyProp.SetValue(null, TargetDisplayFrequency);
                        Logging.Log($"[Quest3PerformanceConfig] Display frequency set to {TargetDisplayFrequency} Hz via OVRPlugin.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logging.LogWarning($"[Quest3PerformanceConfig] OVRPlugin display frequency fallback failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures foveated rendering using OVRPlugin.
        /// </summary>
        private static void ConfigureFoveatedRendering()
        {
            try
            {
                var ovrPluginType = System.Type.GetType("OVRPlugin, Assembly-CSharp") ??
                                    System.Type.GetType("OVRPlugin, Oculus.VR") ??
                                    System.Type.GetType("OVRPlugin, Meta.XR.Core");

                if (ovrPluginType != null)
                {
                    // Set foveated rendering level to High (value 2)
                    // OVRPlugin.FoveatedRenderingLevel: Off=0, Low=1, Medium=2, High=3, HighTop=4
                    var foveatedLevelProp = ovrPluginType.GetProperty("foveatedRenderingLevel",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (foveatedLevelProp != null && foveatedLevelProp.CanWrite)
                    {
                        // Get the enum type
                        var foveatedLevelType = foveatedLevelProp.PropertyType;
                        var highValue = System.Enum.Parse(foveatedLevelType, "High");
                        foveatedLevelProp.SetValue(null, highValue);
                        Logging.Log("[Quest3PerformanceConfig] Foveated rendering set to High via OVRPlugin.");
                    }

                    // Enable dynamic foveated rendering
                    var useDynamicProp = ovrPluginType.GetProperty("useDynamicFoveatedRendering",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (useDynamicProp != null && useDynamicProp.CanWrite)
                    {
                        useDynamicProp.SetValue(null, true);
                        Logging.Log("[Quest3PerformanceConfig] Dynamic foveated rendering enabled.");
                    }
                }
                else
                {
                    Logging.Log("[Quest3PerformanceConfig] OVRPlugin not found - foveation not configured.");
                }
            }
            catch (System.Exception ex)
            {
                Logging.LogWarning($"[Quest3PerformanceConfig] Foveated rendering config failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures CPU and GPU performance levels using OVRPlugin.
        /// </summary>
        private static void ConfigurePerformanceLevels()
        {
            try
            {
                var ovrPluginType = System.Type.GetType("OVRPlugin, Assembly-CSharp") ??
                                    System.Type.GetType("OVRPlugin, Oculus.VR") ??
                                    System.Type.GetType("OVRPlugin, Meta.XR.Core");

                if (ovrPluginType != null)
                {
                    // suggestedCpuPerfLevel - ProcessorPerformanceLevel: PowerSavings=0, SustainedLow=1, SustainedHigh=2, Boost=3
                    var cpuPerfProp = ovrPluginType.GetProperty("suggestedCpuPerfLevel",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (cpuPerfProp != null && cpuPerfProp.CanWrite)
                    {
                        var perfLevelType = cpuPerfProp.PropertyType;
                        var sustainedHighValue = System.Enum.Parse(perfLevelType, "SustainedHigh");
                        cpuPerfProp.SetValue(null, sustainedHighValue);
                        Logging.Log("[Quest3PerformanceConfig] CPU performance level set to SustainedHigh.");
                    }

                    // suggestedGpuPerfLevel
                    var gpuPerfProp = ovrPluginType.GetProperty("suggestedGpuPerfLevel",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (gpuPerfProp != null && gpuPerfProp.CanWrite)
                    {
                        var perfLevelType = gpuPerfProp.PropertyType;
                        var sustainedHighValue = System.Enum.Parse(perfLevelType, "SustainedHigh");
                        gpuPerfProp.SetValue(null, sustainedHighValue);
                        Logging.Log("[Quest3PerformanceConfig] GPU performance level set to SustainedHigh.");
                    }
                }
                else
                {
                    Logging.Log("[Quest3PerformanceConfig] OVRPlugin not found - performance levels not configured.");
                }
            }
            catch (System.Exception ex)
            {
                Logging.LogWarning($"[Quest3PerformanceConfig] Performance levels config failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures additional Quest 3 optimizations.
        /// </summary>
        private static void ConfigureAdditionalOptimizations()
        {
            // Disable VSync at application level - let XR runtime handle it
            QualitySettings.vSyncCount = 0;

            // Don't set Application.targetFrameRate for VR - let XR runtime control timing

            Logging.Log("[Quest3PerformanceConfig] Additional optimizations applied.");
        }
#endif

        /// <summary>
        /// Gets whether performance settings have been applied.
        /// </summary>
        /// <returns>True if Apply() has been called.</returns>
        public static bool IsConfigured()
        {
            return isConfigured;
        }

        /// <summary>
        /// Resets the configuration state. Used for testing.
        /// </summary>
        public static void Reset()
        {
            isConfigured = false;
        }

        /// <summary>
        /// Logs current performance settings for debugging.
        /// </summary>
        public static void LogCurrentSettings()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                Logging.Log("[Quest3PerformanceConfig] Current Settings:");
                Logging.Log($"  VSync Count: {QualitySettings.vSyncCount}");
                Logging.Log($"  Target Frame Rate: {Application.targetFrameRate}");

                // Try to log OVRPlugin values via reflection
                var ovrPluginType = System.Type.GetType("OVRPlugin, Assembly-CSharp") ??
                                    System.Type.GetType("OVRPlugin, Oculus.VR") ??
                                    System.Type.GetType("OVRPlugin, Meta.XR.Core");

                if (ovrPluginType != null)
                {
                    var freqProp = ovrPluginType.GetProperty("systemDisplayFrequency",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (freqProp != null)
                    {
                        var freq = freqProp.GetValue(null);
                        Logging.Log($"  Display Frequency: {freq} Hz");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logging.LogError($"[Quest3PerformanceConfig] Failed to log settings: {ex.Message}");
            }
#else
            Logging.Log("[Quest3PerformanceConfig] Not on Android - no Quest settings to log.");
#endif
        }
    }
}
