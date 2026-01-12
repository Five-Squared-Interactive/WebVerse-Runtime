using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public class Builder : MonoBehaviour
{
    public static void PerformBuild()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/WebGL";
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        // Configure WebGL-specific settings before build
        if (buildPlayerOptions.target == BuildTarget.WebGL)
        {
            ConfigureWebGLSettings();
        }

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

    private static void ConfigureWebGLSettings()
    {
        // Set WebGL memory configuration for better performance
        PlayerSettings.WebGL.memorySize = 64; // 64 MB initial
        
        // Configure compression and optimization
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        
        // Set exception handling based on development build
        if (EditorUserBuildSettings.development)
        {
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
        }
        else
        {
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        }

        Debug.Log("WebGL settings configured for optimized memory usage");
    }

    public static void BuildWebGL()
    {
        // Set build target to WebGL
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        
        // Configure WebGL-specific settings
        ConfigureWebGLSettings();
        
        // Perform the build
        PerformBuild();
    }
}