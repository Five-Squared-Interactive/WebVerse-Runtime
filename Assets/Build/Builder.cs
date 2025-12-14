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
        // Build output paths (relative to project root)
        private const string BuildOutputRoot = "Builds";
        private const string WebGLCompressedPath = BuildOutputRoot + "/WebGL-Compressed";
        private const string WebGLUncompressedPath = BuildOutputRoot + "/WebGL-Uncompressed";
        private const string WindowsDesktopPath = BuildOutputRoot + "/Windows-Desktop";
        private const string MacDesktopPath = BuildOutputRoot + "/Mac-Desktop";

        // Scene paths
        private const string LightweightModeScene = "Assets/Runtime/TopLevel/Scenes/LightweightMode.unity";
        private const string FocusedModeScene = "Assets/Runtime/TopLevel/Scenes/FocusedMode.unity";

        /// <summary>
        /// Build WebGL with Gzip compression.
        /// </summary>
        public static void BuildWebGLCompressed()
        {
            Debug.Log("Starting WebGL Compressed build...");
            
            // Set WebGL compression to Gzip
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = WebGLCompressedPath,
                options = BuildOptions.None,
                scenes = new string[] { LightweightModeScene },
                target = BuildTarget.WebGL
            };

            ExecuteBuild(options, "WebGL Compressed");
        }

        /// <summary>
        /// Build WebGL without compression.
        /// </summary>
        public static void BuildWebGLUncompressed()
        {
            Debug.Log("Starting WebGL Uncompressed build...");
            
            // Set WebGL compression to disabled
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = WebGLUncompressedPath,
                options = BuildOptions.None,
                scenes = new string[] { LightweightModeScene },
                target = BuildTarget.WebGL
            };

            ExecuteBuild(options, "WebGL Uncompressed");
        }

        /// <summary>
        /// Build for Windows Desktop (64-bit).
        /// </summary>
        public static void BuildWindowsDesktop()
        {
            Debug.Log("Starting Windows Desktop build...");
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = WindowsDesktopPath + "/WebVerse.exe",
                options = BuildOptions.None,
                scenes = new string[] { FocusedModeScene },
                target = BuildTarget.StandaloneWindows64
            };

            ExecuteBuild(options, "Windows Desktop");
        }

        /// <summary>
        /// Build for Mac Desktop.
        /// </summary>
        public static void BuildMacDesktop()
        {
            Debug.Log("Starting Mac Desktop build...");
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = MacDesktopPath + "/WebVerse.app",
                options = BuildOptions.None,
                scenes = new string[] { FocusedModeScene },
                target = BuildTarget.StandaloneOSX
            };

            ExecuteBuild(options, "Mac Desktop");
        }

        /// <summary>
        /// Build all targets for CI pipeline.
        /// Called with: -executeMethod FiveSQD.WebVerse.Building.Builder.BuildAll
        /// </summary>
        public static void BuildAll()
        {
            Debug.Log("Starting all builds...");
            
            BuildWebGLCompressed();
            BuildWebGLUncompressed();
            BuildWindowsDesktop();
            BuildMacDesktop();
            
            Debug.Log("All builds completed.");
        }

        /// <summary>
        /// Execute a build and log the result.
        /// </summary>
        /// <param name="options">Build options.</param>
        /// <param name="buildName">Name of the build for logging.</param>
        private static void ExecuteBuild(BuildPlayerOptions options, string buildName)
        {
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log($"[{buildName}] Build succeeded: {summary.totalSize} bytes written to {options.locationPathName}");
                    break;
                case BuildResult.Failed:
                    Debug.LogError($"[{buildName}] Build failed with {summary.totalErrors} errors.");
                    // Exit with error code for CI
                    EditorApplication.Exit(1);
                    break;
                case BuildResult.Cancelled:
                    Debug.LogWarning($"[{buildName}] Build was cancelled.");
                    EditorApplication.Exit(1);
                    break;
                case BuildResult.Unknown:
                    Debug.LogError($"[{buildName}] Build result unknown.");
                    EditorApplication.Exit(1);
                    break;
                default:
                    Debug.LogError($"[{buildName}] Unidentified build result.");
                    EditorApplication.Exit(1);
                    break;
            }
        }
    }
}
#endif
