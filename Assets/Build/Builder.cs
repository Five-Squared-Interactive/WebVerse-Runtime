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
        private const string WebRuntimeScene = "Assets/Runtime/TopLevel/Scenes/WebRuntime.unity";
        private const string DesktopRuntimeScene = "Assets/Runtime/TopLevel/Scenes/DesktopRuntime.unity";

        /// <summary>
        /// Build WebGL with Gzip compression.
        /// </summary>
        public static void BuildWebGLCompressed()
        {
            Debug.Log("Starting WebGL Compressed build...");
            
            // Configure WebGL build settings (production build)
            ConfigureWebGLBuildSettings(isDebugBuild: false);
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = WebGLCompressedPath,
                options = BuildOptions.None,
                scenes = new string[] { WebRuntimeScene },
                target = BuildTarget.WebGL
            };

            ExecuteBuild(options, "WebGL Compressed");
        }

        /// <summary>
        /// Build WebGL with Gzip compression (Debug mode).
        /// </summary>
        public static void BuildWebGLCompressedDebug()
        {
            Debug.Log("Starting WebGL Compressed Debug build...");
            
            // Configure WebGL build settings (debug build)
            ConfigureWebGLBuildSettings(isDebugBuild: true);
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = WebGLCompressedPath + "-Debug",
                options = BuildOptions.Development,
                scenes = new string[] { WebRuntimeScene },
                target = BuildTarget.WebGL
            };

            ExecuteBuild(options, "WebGL Compressed Debug");
        }

        /// <summary>
        /// Build WebGL without compression.
        /// </summary>
        public static void BuildWebGLUncompressed()
        {
            Debug.Log("Starting WebGL Uncompressed build...");
            
            // Configure WebGL build settings (production build)
            ConfigureWebGLBuildSettings(isDebugBuild: false);
            
            // Set WebGL compression to disabled
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = WebGLUncompressedPath,
                options = BuildOptions.None,
                scenes = new string[] { WebRuntimeScene },
                target = BuildTarget.WebGL
            };

            ExecuteBuild(options, "WebGL Uncompressed");
        }

        /// <summary>
        /// Build WebGL without compression (Debug mode).
        /// </summary>
        public static void BuildWebGLUncompressedDebug()
        {
            Debug.Log("Starting WebGL Uncompressed Debug build...");
            
            // Configure WebGL build settings (debug build)
            ConfigureWebGLBuildSettings(isDebugBuild: true);
            
            // Set WebGL compression to disabled
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = WebGLUncompressedPath + "-Debug",
                options = BuildOptions.Development,
                scenes = new string[] { WebRuntimeScene },
                target = BuildTarget.WebGL
            };

            ExecuteBuild(options, "WebGL Uncompressed Debug");
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
                scenes = new string[] { DesktopRuntimeScene },
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
                scenes = new string[] { DesktopRuntimeScene },
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
        /// Configure WebGL-specific build settings.
        /// </summary>
        /// <param name="isDebugBuild">Whether this is a debug build.</param>
        private static void ConfigureWebGLBuildSettings(bool isDebugBuild)
        {
            // Set compression to Gzip
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            
            // Set linker target to Wasm
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            
            // Enable data caching
            PlayerSettings.WebGL.dataCaching = true;
            
            // Configure exception support based on build type
            if (isDebugBuild)
            {
                // Debug builds: full exception support
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
                Debug.Log("[WebGL Config] Debug build: Using full exception support with stacktrace");
            }
            else
            {
                // Production builds: explicitly thrown exceptions only
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
                Debug.Log("[WebGL Config] Production build: Using explicitly thrown exceptions only");
            }
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
