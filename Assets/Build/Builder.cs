// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FiveSQD.WebVerse.Building
{
    /// <summary>
    /// Class for automated building.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Build for WebGL.
        /// </summary>
        public static void BuildWebGL()
        {
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = "../../WebGL",
                options = BuildOptions.None,
                scenes = new string[]
                {
                    "Assets/Runtime/TopLevel/Scenes/LightweightMode.unity"
                },
                target = BuildTarget.WebGL
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build Successful. Build written to " + options.locationPathName);
            }
            else if (report.summary.result == BuildResult.Failed)
            {
                Debug.Log("Build Failed.");
            }
            else if (report.summary.result == BuildResult.Cancelled)
            {
                Debug.Log("Build Cancelled.");
            }
            else if (report.summary.result == BuildResult.Unknown)
            {
                Debug.Log("Unknown Build Result.");
            }
            else
            {
                Debug.Log("Unidentified Build Result.");
            }
        }

        /// <summary>
        /// Build for WindowsFocusedMode.
        /// </summary>
        public static void BuildWindowsFocusedMode()
        {
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = "../../WindowsFocusedMode",
                options = BuildOptions.None,
                scenes = new string[]
                {
                    "Assets/Runtime/TopLevel/Scenes/FocusedMode.unity"
                },
                target = BuildTarget.StandaloneWindows64
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build Successful. Build written to " + options.locationPathName);
            }
            else if (report.summary.result == BuildResult.Failed)
            {
                Debug.Log("Build Failed.");
            }
            else if (report.summary.result == BuildResult.Cancelled)
            {
                Debug.Log("Build Cancelled.");
            }
            else if (report.summary.result == BuildResult.Unknown)
            {
                Debug.Log("Unknown Build Result.");
            }
            else
            {
                Debug.Log("Unidentified Build Result.");
            }
        }

        /// <summary>
        /// Build for WindowsFocusedMode SteamVR.
        /// </summary>
        public static void BuildWindowsFocusedModeSteamVR()
        {
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = "../../WindowsFocusedMode-SteamVR",
                options = BuildOptions.None,
                scenes = new string[]
                {
                    "Assets/Runtime/TopLevel/Scenes/FocusedMode-SteamVR.unity"
                },
                target = BuildTarget.StandaloneWindows64
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build Successful. Build written to " + options.locationPathName);
            }
            else if (report.summary.result == BuildResult.Failed)
            {
                Debug.Log("Build Failed.");
            }
            else if (report.summary.result == BuildResult.Cancelled)
            {
                Debug.Log("Build Cancelled.");
            }
            else if (report.summary.result == BuildResult.Unknown)
            {
                Debug.Log("Unknown Build Result.");
            }
            else
            {
                Debug.Log("Unidentified Build Result.");
            }
        }
    }
}
#endif