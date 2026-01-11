using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;

public class Builder : MonoBehaviour
{
    public static void Build()
    {
        // Get build target from command line arguments
        string[] args = Environment.GetCommandLineArgs();
        BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildTarget" && i + 1 < args.Length)
            {
                Enum.TryParse(args[i + 1], out buildTarget);
                break;
            }
        }
        
        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = GetEnabledScenes();
        buildOptions.locationPathName = GetBuildPath(buildTarget);
        buildOptions.target = buildTarget;
        buildOptions.options = BuildOptions.None;
        
        // Configure WebGL-specific settings
        if (buildTarget == BuildTarget.WebGL)
        {
            ConfigureWebGLBuildSettings(ref buildOptions);
        }
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded!");
        }
        else
        {
            Debug.LogError("Build failed!");
            EditorApplication.Exit(1);
        }
    }
    
    public static void BuildWebGL()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = GetEnabledScenes();
        buildOptions.locationPathName = GetBuildPath(BuildTarget.WebGL);
        buildOptions.target = BuildTarget.WebGL;
        buildOptions.options = BuildOptions.None;
        
        ConfigureWebGLBuildSettings(ref buildOptions);
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("WebGL Build succeeded!");
        }
        else
        {
            Debug.LogError("WebGL Build failed!");
            EditorApplication.Exit(1);
        }
    }
    
    private static void ConfigureWebGLBuildSettings(ref BuildPlayerOptions buildOptions)
    {
        Debug.Log("Configuring WebGL-specific build settings...");
        
        // Set WebGL memory configuration
        PlayerSettings.WebGL.memorySize = 64;
        PlayerSettings.WebGL.maxMemorySize = 2048;
        PlayerSettings.WebGL.memoryLinearGrowthStep = 32;
        PlayerSettings.WebGL.memoryGeometricGrowthStep = 0.15f;
        PlayerSettings.WebGL.memoryGeometricGrowthCap = 128;
        
        // Set WebGL-specific compression and build settings
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.dataCaching = true;
        
        // Configure exception handling based on build type
        if (IsProductionBuild())
        {
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            Debug.Log("Production build: Using minimal exception support");
        }
        else
        {
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
            Debug.Log("Debug build: Using full exception support");
        }
        
        // Set WebGL-Optimized quality level for WebGL builds
        SetWebGLQualitySettings();
        
        Debug.Log($"WebGL Memory Configuration - Initial: {PlayerSettings.WebGL.memorySize}MB, Max: {PlayerSettings.WebGL.maxMemorySize}MB");
    }
    
    private static void SetWebGLQualitySettings()
    {
        // Find and set WebGL-Optimized quality level as default for WebGL
        string[] qualityNames = QualitySettings.names;
        for (int i = 0; i < qualityNames.Length; i++)
        {
            if (qualityNames[i] == "WebGL-Optimized")
            {
                QualitySettings.SetQualityLevel(i, false);
                Debug.Log($"Set WebGL-Optimized quality level (index {i}) as active");
                return;
            }
        }
        Debug.LogWarning("WebGL-Optimized quality level not found. Using default quality settings.");
    }
    
    private static bool IsProductionBuild()
    {
        // Check for production build indicators
        bool isDevelopmentBuild = EditorUserBuildSettings.development;
        bool hasDebugger = System.Diagnostics.Debugger.IsAttached;
        string buildType = System.Environment.GetEnvironmentVariable("BUILD_TYPE");
        
        // If any indicator suggests debug, use debug settings
        if (isDevelopmentBuild || hasDebugger || buildType == "debug")
            return false;
            
        // Default to production for safety
        return true;
    }
    
    private static string[] GetEnabledScenes()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        return scenes.ToArray();
    }
    
    private static string GetBuildPath(BuildTarget buildTarget)
    {
        string basePath = "Build/";
        switch (buildTarget)
        {
            case BuildTarget.WebGL:
                return basePath + "WebGL";
            case BuildTarget.StandaloneWindows64:
                return basePath + "Windows/WebVerseRuntime.exe";
            case BuildTarget.StandaloneOSX:
                return basePath + "MacOS/WebVerseRuntime.app";
            case BuildTarget.StandaloneLinux64:
                return basePath + "Linux/WebVerseRuntime";
            default:
                return basePath + "Default";
        }
    }
    
#if UNITY_EDITOR
    [MenuItem("WebVerse/Setup WebGL Optimizations")]
    public static void SetupWebGLOptimizations()
    {
        Debug.Log("Setting up WebGL optimizations...");
        
        // Configure WebGL memory settings
        PlayerSettings.WebGL.memorySize = 64;
        PlayerSettings.WebGL.maxMemorySize = 2048;
        PlayerSettings.WebGL.memoryLinearGrowthStep = 32;
        PlayerSettings.WebGL.memoryGeometricGrowthStep = 0.15f;
        PlayerSettings.WebGL.memoryGeometricGrowthCap = 128;
        
        // Configure WebGL build settings
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.dataCaching = true;
        
        // Enable streaming mipmaps globally
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = 268435456; // 256 MB in bytes
        QualitySettings.streamingMipmapsAddAllCameras = true;
        QualitySettings.streamingMipmapsMaxLevelReduction = 3;
        
        // Create WebGL-Optimized quality level if it doesn't exist
        CreateWebGLQualityLevel();
        
        // Save settings
        AssetDatabase.SaveAssets();
        Debug.Log("WebGL optimizations applied successfully!");
    }
    
    [MenuItem("WebVerse/Test WebGL Settings")]
    public static void TestWebGLSettings()
    {
        Debug.Log("=== WebGL Settings Test ===");
        Debug.Log($"WebGL Memory Size: {PlayerSettings.WebGL.memorySize}MB");
        Debug.Log($"WebGL Max Memory: {PlayerSettings.WebGL.maxMemorySize}MB");
        Debug.Log($"WebGL Linear Growth Step: {PlayerSettings.WebGL.memoryLinearGrowthStep}MB");
        Debug.Log($"WebGL Geometric Growth Step: {PlayerSettings.WebGL.memoryGeometricGrowthStep}");
        Debug.Log($"WebGL Geometric Growth Cap: {PlayerSettings.WebGL.memoryGeometricGrowthCap}MB");
        Debug.Log($"Compression Format: {PlayerSettings.WebGL.compressionFormat}");
        Debug.Log($"Linker Target: {PlayerSettings.WebGL.linkerTarget}");
        Debug.Log($"Data Caching: {PlayerSettings.WebGL.dataCaching}");
        Debug.Log($"Streaming Mipmaps Active: {QualitySettings.streamingMipmapsActive}");
        Debug.Log($"Streaming Mipmaps Budget: {QualitySettings.streamingMipmapsMemoryBudget / 1048576}MB");
        Debug.Log($"Current Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
    }
    
    private static void CreateWebGLQualityLevel()
    {
        string[] qualityNames = QualitySettings.names;
        
        // Check if WebGL-Optimized already exists
        for (int i = 0; i < qualityNames.Length; i++)
        {
            if (qualityNames[i] == "WebGL-Optimized")
            {
                Debug.Log("WebGL-Optimized quality level already exists");
                return;
            }
        }
        
        // Get the current quality settings count
        int currentCount = QualitySettings.count;
        
        // Set quality count to add a new level
        QualitySettings.count = currentCount + 1;
        
        // Set the new quality level
        QualitySettings.SetQualityLevel(currentCount);
        
        // Configure the new quality level
        QualitySettings.pixelLightCount = 1;
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.antiAliasing = 0;
        QualitySettings.asyncUploadBufferSize = 8;
        QualitySettings.asyncUploadTimeSlice = 2;
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = 268435456; // 256 MB
        QualitySettings.particleRaycastBudget = 64;
        
        // Note: Unity doesn't provide a direct API to set quality level names
        // The name "WebGL-Optimized" would need to be set manually in the Quality Settings
        Debug.Log($"Created new quality level at index {currentCount}. Please manually rename it to 'WebGL-Optimized' in Quality Settings.");
    }
#endif
}