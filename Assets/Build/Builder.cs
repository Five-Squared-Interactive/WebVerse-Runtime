// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FiveSQD.WebVerse.Building
{
    /// <summary>
    /// Class for automated building.
    /// Supports building for WebGL (compressed/uncompressed), Windows Desktop, Mac Desktop, iOS, and Android (APK/AAB).
    /// </summary>
    public class Builder
    {
        // Build output paths (relative to project root)
        private const string BuildOutputRoot = "Builds";
        private const string WebGLCompressedPath = BuildOutputRoot + "/WebGL-Compressed";
        private const string WebGLUncompressedPath = BuildOutputRoot + "/WebGL-Uncompressed";
        private const string WindowsDesktopPath = BuildOutputRoot + "/Windows-Desktop";
        private const string MacDesktopPath = BuildOutputRoot + "/Mac-Desktop";
        private const string AndroidPath = BuildOutputRoot + "/Android";
        private const string iOSPath = BuildOutputRoot + "/iOS";

        // Scene paths
        private const string WebRuntimeScene = "Assets/Runtime/TopLevel/Scenes/WebRuntime.unity";
        private const string DesktopRuntimeScene = "Assets/Runtime/TopLevel/Scenes/DesktopRuntime.unity";
        private const string MobileRuntimeScene = "Assets/Runtime/TopLevel/Scenes/MobileRuntime.unity";

        /// <summary>
        /// Build WebGL with Gzip compression.
        /// </summary>
        public static void BuildWebGLCompressed()
        {
            Debug.Log("Starting WebGL Compressed build...");
            
            // Configure WebGL build settings (production build with compression)
            ConfigureWebGLBuildSettings(isDebugBuild: false, compressionEnabled: true);
            
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
            
            // Configure WebGL build settings (debug build with compression)
            ConfigureWebGLBuildSettings(isDebugBuild: true, compressionEnabled: true);
            
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
            
            // Configure WebGL build settings (production build without compression)
            ConfigureWebGLBuildSettings(isDebugBuild: false, compressionEnabled: false);
            
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
            
            // Configure WebGL build settings (debug build without compression)
            ConfigureWebGLBuildSettings(isDebugBuild: true, compressionEnabled: false);
            
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
        /// Build Android APK.
        /// Supports optional keystore configuration via command line arguments:
        /// -keystorePath, -keystorePass, -keyaliasName, -keyaliasPass
        /// Called with: -executeMethod FiveSQD.WebVerse.Building.Builder.BuildAndroidAPK
        /// </summary>
        public static void BuildAndroidAPK()
        {
            Debug.Log("Starting Android APK build...");
            
            // Configure Android-specific settings
            ConfigureAndroidBuildSettings();
            
            // Parse command line arguments for keystore
            ParseAndroidKeystoreArguments("Android APK");
            
            // Set build to APK format
            EditorUserBuildSettings.buildAppBundle = false;
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = AndroidPath + "/WebVerse.apk",
                options = BuildOptions.None,
                scenes = new string[] { MobileRuntimeScene },
                target = BuildTarget.Android
            };

            ExecuteBuild(options, "Android APK");

            EditorApplication.Exit(0);
        }

        /// <summary>
        /// Build Android App Bundle (AAB) for Google Play Store.
        /// Supports optional keystore configuration via command line arguments:
        /// -keystorePath, -keystorePass, -keyaliasName, -keyaliasPass
        /// Called with: -executeMethod FiveSQD.WebVerse.Building.Builder.BuildAndroidAAB
        /// </summary>
        public static void BuildAndroidAAB()
        {
            Debug.Log("Starting Android AAB build...");
            
            // Configure Android-specific settings
            ConfigureAndroidBuildSettings();
            
            // Parse command line arguments for keystore
            ParseAndroidKeystoreArguments("Android AAB");
            
            // Set build to AAB format
            EditorUserBuildSettings.buildAppBundle = true;
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = AndroidPath + "/WebVerse.aab",
                options = BuildOptions.None,
                scenes = new string[] { MobileRuntimeScene },
                target = BuildTarget.Android
            };

            ExecuteBuild(options, "Android AAB");
        }

        /// <summary>
        /// Build iOS Xcode project.
        /// Called with: -executeMethod FiveSQD.WebVerse.Building.Builder.BuildiOS
        /// </summary>
        public static void BuildiOS()
        {
            Debug.Log("Starting iOS build...");
            
            // Configure iOS-specific settings
            ConfigureiOSBuildSettings();
            
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                locationPathName = iOSPath,
                options = BuildOptions.None,
                scenes = new string[] { MobileRuntimeScene },
                target = BuildTarget.iOS
            };

            ExecuteBuild(options, "iOS");
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
            BuildAndroidAPK();
            BuildAndroidAAB();
            BuildiOS();
            
            Debug.Log("All builds completed.");
        }

        /// <summary>
        /// Configure Android-specific build settings.
        /// </summary>
        private static void ConfigureAndroidBuildSettings()
        {
            // Set target API level (Android 13 / API level 33)
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            
            // Set minimum API level (Android 7.0 / API level 24)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            
            // Target ARM64 architecture (required by Google Play Store)
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            Debug.Log("[Android Config] Configured build settings: IL2CPP backend, ARM64 architecture, API 24-33");
        }

        /// <summary>
        /// Configure iOS-specific build settings.
        /// </summary>
        private static void ConfigureiOSBuildSettings()
        {
            // Set minimum iOS version (iOS 13.0)
            PlayerSettings.iOS.targetOSVersionString = "13.0";
            
            // Set scripting backend to IL2CPP (required for iOS)
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            
            // Set target SDK to Device SDK (for physical devices)
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            
            // Disable automatic signing (to be configured in Xcode)
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            
            Debug.Log("[iOS Config] Configured build settings: IL2CPP backend, iOS 13.0+, Device SDK");
        }

        /// <summary>
        /// Parse Android keystore arguments from command line.
        /// </summary>
        /// <param name="buildType">The type of build (for logging purposes).</param>
        private static void ParseAndroidKeystoreArguments(string buildType)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-keystorePath" && i + 1 < args.Length)
                {
                    PlayerSettings.Android.keystoreName = args[i + 1];
                    Debug.Log($"[{buildType}] Using keystore: {args[i + 1]}");
                }
                else if (args[i] == "-keystorePass" && i + 1 < args.Length)
                {
                    PlayerSettings.Android.keystorePass = args[i + 1];
                }
                else if (args[i] == "-keyaliasName" && i + 1 < args.Length)
                {
                    PlayerSettings.Android.keyaliasName = args[i + 1];
                    Debug.Log($"[{buildType}] Using key alias: {args[i + 1]}");
                }
                else if (args[i] == "-keyaliasPass" && i + 1 < args.Length)
                {
                    PlayerSettings.Android.keyaliasPass = args[i + 1];
                }
            }
        }

        /// <summary>
        /// Configure WebGL-specific build settings.
        /// </summary>
        /// <param name="isDebugBuild">Whether this is a debug build.</param>
        /// <param name="compressionEnabled">Whether to enable Gzip compression.</param>
        private static void ConfigureWebGLBuildSettings(bool isDebugBuild, bool compressionEnabled)
        {
            // Set compression settings
            if (compressionEnabled)
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;
            }
            else
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.decompressionFallback = false;
            }
            
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
