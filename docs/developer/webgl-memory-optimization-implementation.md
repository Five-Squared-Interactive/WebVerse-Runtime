# WebGL/WebGPU Memory Optimization Implementation Guide

## Overview

This guide provides step-by-step instructions and code examples for implementing the memory optimizations identified in the WebGL/WebGPU Memory Optimization Analysis.

## Table of Contents

1. [Memory Configuration Updates](#memory-configuration-updates)
2. [Quality Settings Optimization](#quality-settings-optimization)
3. [Build Pipeline Enhancements](#build-pipeline-enhancements)
4. [Runtime Resource Management](#runtime-resource-management)
5. [Asset Optimization](#asset-optimization)
6. [Testing and Validation](#testing-and-validation)

## Memory Configuration Updates

### Step 1: Update ProjectSettings Memory Configuration

**File**: `ProjectSettings/ProjectSettings.asset`

**Changes**:
```yaml
# Before
webGLInitialMemorySize: 32
webGLMaximumMemorySize: 3032
webGLMemoryLinearGrowthStep: 16

# After (Recommended)
webGLInitialMemorySize: 64
webGLMaximumMemorySize: 2048
webGLMemoryLinearGrowthStep: 32
webGLMemoryGeometricGrowthStep: 0.15
webGLMemoryGeometricGrowthCap: 128
```

**Implementation**:
1. Open Unity Project Settings (Edit → Project Settings)
2. Navigate to Player → WebGL Settings → Publishing Settings
3. Update Memory Size settings:
   - Initial Memory Size: 64 MB
   - Maximum Memory Size: 2048 MB

**Rationale**: Reduces unnecessary memory overhead while providing sufficient space for typical scenes.

### Step 2: Optimize Exception Handling

**File**: `ProjectSettings/ProjectSettings.asset`

**Changes**:
```yaml
# Before
webGLExceptionSupport: 2  # Full

# After (Production)
webGLExceptionSupport: 1  # Explicitly Thrown Exceptions Only
```

**Build Script Update**:
```csharp
// In Assets/Build/Builder.cs

public static void BuildWebGLProduction()
{
    Debug.Log("Starting WebGL Production build with optimizations...");
    
    // Set exception support to explicitly thrown only
    PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
    
    // Set memory configuration
    PlayerSettings.WebGL.initialMemorySize = 64;
    PlayerSettings.WebGL.maximumMemorySize = 2048;
    
    // Enable WebGPU
    PlayerSettings.WebGL.enableWebGPU = true;
    
    // Set compression
    PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
    PlayerSettings.WebGL.decompressionFallback = true;
    
    BuildPlayerOptions options = new BuildPlayerOptions()
    {
        locationPathName = BuildOutputRoot + "/WebGL-Production",
        options = BuildOptions.None,
        scenes = new string[] { WebRuntimeScene },
        target = BuildTarget.WebGL
    };

    ExecuteBuild(options, "WebGL Production");
}

public static void BuildWebGLDebug()
{
    Debug.Log("Starting WebGL Debug build...");
    
    // Set exception support to full for debugging
    PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
    
    BuildPlayerOptions options = new BuildPlayerOptions()
    {
        locationPathName = BuildOutputRoot + "/WebGL-Debug",
        options = BuildOptions.Development,
        scenes = new string[] { WebRuntimeScene },
        target = BuildTarget.WebGL
    };

    ExecuteBuild(options, "WebGL Debug");
}
```

## Quality Settings Optimization

### Step 3: Create WebGL-Specific Quality Preset

**File**: `ProjectSettings/QualitySettings.asset`

**Add New Quality Level**:
```yaml
- serializedVersion: 4
  name: WebGL-Optimized
  pixelLightCount: 1
  shadows: 1
  shadowResolution: 0
  shadowProjection: 1
  shadowCascades: 1
  shadowDistance: 40
  shadowNearPlaneOffset: 3
  shadowmaskMode: 0
  skinWeights: 2
  globalTextureMipmapLimit: 0
  textureMipmapLimitSettings: []
  anisotropicTextures: 1
  antiAliasing: 0
  softParticles: 0
  softVegetation: 0
  realtimeReflectionProbes: 0
  billboardsFaceCameraPosition: 1
  useLegacyDetailDistribution: 1
  adaptiveVsync: 0
  vSyncCount: 0
  realtimeGICPUUsage: 25
  lodBias: 0.7
  maximumLODLevel: 0
  enableLODCrossFade: 1
  streamingMipmapsActive: 1
  streamingMipmapsAddAllCameras: 1
  streamingMipmapsMemoryBudget: 256
  streamingMipmapsRenderersPerFrame: 512
  streamingMipmapsMaxLevelReduction: 3
  streamingMipmapsMaxFileIORequests: 512
  particleRaycastBudget: 64
  asyncUploadTimeSlice: 2
  asyncUploadBufferSize: 8
  asyncUploadPersistentBuffer: 1
  resolutionScalingFixedDPIFactor: 1
  terrainQualityOverrides: 0
  terrainPixelError: 5
  terrainDetailDensityScale: 0.5
  terrainBasemapDistance: 500
  terrainDetailDistance: 50
  terrainTreeDistance: 2000
  terrainBillboardStart: 50
  terrainFadeLength: 5
  terrainMaxTrees: 50
  excludedTargetPlatforms: []
```

**Update Per-Platform Quality Settings**:
```yaml
m_PerPlatformDefaultQuality:
  WebGL: 6  # Index of WebGL-Optimized quality level
```

## Build Pipeline Enhancements

### Step 4: Enhanced Build Script with Optimizations

**File**: `Assets/Build/Builder.cs`

**Add New Methods**:
```csharp
/// <summary>
/// Optimize textures for WebGL before building.
/// </summary>
public static void OptimizeTexturesForWebGL()
{
    Debug.Log("Optimizing textures for WebGL...");
    
    string[] textureGUIDs = AssetDatabase.FindAssets("t:Texture2D");
    int optimizedCount = 0;
    
    // Exclusion patterns for paths that should not be optimized
    string[] exclusionPatterns = new string[]
    {
        Path.DirectorySeparatorChar + "3rd-party" + Path.DirectorySeparatorChar,
        Path.DirectorySeparatorChar + "TextMesh Pro" + Path.DirectorySeparatorChar,
        Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar
    };
    
    AssetDatabase.StartAssetEditing(); // Batch asset operations
    
    try
    {
        foreach (string guid in textureGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Skip certain paths using configurable exclusion patterns
            bool shouldExclude = false;
            foreach (string pattern in exclusionPatterns)
            {
                if (path.Contains(pattern))
                {
                    shouldExclude = true;
                    break;
                }
            }
            
            if (shouldExclude)
            {
                continue;
            }
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                TextureImporterPlatformSettings webglSettings = importer.GetPlatformTextureSettings("WebGL");
                
                // Only update if not already configured
                if (!webglSettings.overridden)
                {
                    webglSettings.name = "WebGL";
                    webglSettings.overridden = true;
                    webglSettings.maxTextureSize = 2048;
                    webglSettings.format = TextureImporterFormat.DXT5Crunched;
                    webglSettings.compressionQuality = 50;
                    webglSettings.crunchedCompression = true;
                    
                    importer.SetPlatformTextureSettings(webglSettings);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    optimizedCount++;
                }
            }
        }
    }
    finally
    {
        AssetDatabase.StopAssetEditing(); // Batch import all changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    Debug.Log($"Optimized {optimizedCount} textures for WebGL.");
}

/// <summary>
/// Configure build settings for optimal WebGL memory usage.
/// </summary>
private static void ConfigureOptimalWebGLSettings()
{
    // Managed code stripping
    PlayerSettings.SetManagedStrippingLevel(
        BuildTargetGroup.WebGL, 
        ManagedStrippingLevel.High
    );
    PlayerSettings.stripEngineCode = true;
    
    // IL2CPP optimization
    PlayerSettings.SetIl2CppCompilerConfiguration(
        BuildTargetGroup.WebGL,
        Il2CppCompilerConfiguration.Master
    );
    
    // Memory settings
    PlayerSettings.WebGL.initialMemorySize = 64;
    PlayerSettings.WebGL.maximumMemorySize = 2048;
    
    // WebGPU support
    PlayerSettings.WebGL.enableWebGPU = true;
    
    // Data caching
    PlayerSettings.WebGL.dataCaching = true;
    
    // Exception support (production)
    PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
    
    // Compression
    PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
    PlayerSettings.WebGL.decompressionFallback = true;
    
    // Linker target
    PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
    
    Debug.Log("WebGL settings configured for optimal memory usage.");
}

/// <summary>
/// Build WebGL with full optimization pipeline.
/// </summary>
public static void BuildWebGLOptimized()
{
    Debug.Log("Starting optimized WebGL build...");
    
    // Pre-build optimizations
    OptimizeTexturesForWebGL();
    ConfigureOptimalWebGLSettings();
    
    BuildPlayerOptions options = new BuildPlayerOptions()
    {
        locationPathName = BuildOutputRoot + "/WebGL-Optimized",
        options = BuildOptions.None,
        scenes = new string[] { WebRuntimeScene },
        target = BuildTarget.WebGL
    };

    ExecuteBuild(options, "WebGL Optimized");
    
    // Post-build report
    ReportBuildMetrics(options.locationPathName);
}

/// <summary>
/// Report build size and metrics.
/// </summary>
private static void ReportBuildMetrics(string buildPath)
{
    if (Directory.Exists(buildPath))
    {
        DirectoryInfo dir = new DirectoryInfo(buildPath);
        long totalSize = 0;
        
        foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
        {
            totalSize += file.Length;
        }
        
        Debug.Log($"Total build size: {totalSize / (1024.0 * 1024.0):F2} MB");
        
        // Find and report WASM file size
        FileInfo[] wasmFiles = dir.GetFiles("*.wasm", SearchOption.AllDirectories);
        foreach (FileInfo wasm in wasmFiles)
        {
            Debug.Log($"WASM file: {wasm.Name} - {wasm.Length / (1024.0 * 1024.0):F2} MB");
        }
    }
}
```

## Runtime Resource Management

### Step 5: Implement Resource Cleanup System

**File**: `Assets/Runtime/Runtime/Scripts/ResourceManager.cs` (New File)

```csharp
// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using System.Collections;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// Manages runtime resource cleanup and memory optimization for WebGL builds.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        [Header("Cleanup Settings")]
        [Tooltip("Interval between automatic resource cleanup (seconds)")]
        [SerializeField]
        private float cleanupInterval = 60f;
        
        [Tooltip("Enable automatic resource cleanup")]
        [SerializeField]
        private bool enableAutomaticCleanup = true;
        
        [Tooltip("Enable memory profiling logs")]
        [SerializeField]
        private bool enableMemoryProfiling = false;
        
        private Coroutine cleanupCoroutine;
        
        private void Start()
        {
            if (enableAutomaticCleanup)
            {
                StartResourceCleanup();
            }
        }
        
        private void OnDestroy()
        {
            StopResourceCleanup();
        }
        
        /// <summary>
        /// Start automatic resource cleanup.
        /// </summary>
        public void StartResourceCleanup()
        {
            if (cleanupCoroutine == null)
            {
                cleanupCoroutine = StartCoroutine(PeriodicResourceCleanup());
            }
        }
        
        /// <summary>
        /// Stop automatic resource cleanup.
        /// </summary>
        public void StopResourceCleanup()
        {
            if (cleanupCoroutine != null)
            {
                StopCoroutine(cleanupCoroutine);
                cleanupCoroutine = null;
            }
        }
        
        /// <summary>
        /// Manually trigger resource cleanup.
        /// </summary>
        public void CleanupNow()
        {
            PerformCleanup();
        }
        
        /// <summary>
        /// Periodic resource cleanup coroutine.
        /// </summary>
        private IEnumerator PeriodicResourceCleanup()
        {
            while (true)
            {
                yield return new WaitForSeconds(cleanupInterval);
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                PerformCleanup();
                #endif
            }
        }
        
        /// <summary>
        /// Perform resource cleanup operations.
        /// </summary>
        private void PerformCleanup()
        {
            if (enableMemoryProfiling)
            {
                LogMemoryUsage("Before cleanup");
            }
            
            // Unload unused assets
            Resources.UnloadUnusedAssets();
            
            // Trigger garbage collection
            System.GC.Collect();
            
            if (enableMemoryProfiling)
            {
                LogMemoryUsage("After cleanup");
            }
        }
        
        /// <summary>
        /// Log current memory usage.
        /// </summary>
        private void LogMemoryUsage(string label)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            long totalMemory = System.GC.GetTotalMemory(false);
            Debug.Log($"[ResourceManager] {label} - Managed Memory: {totalMemory / (1024.0 * 1024.0):F2} MB");
            #endif
        }
        
        /// <summary>
        /// Get memory usage statistics.
        /// </summary>
        public string GetMemoryStats()
        {
            long totalMemory = System.GC.GetTotalMemory(false);
            return $"Managed Memory: {totalMemory / (1024.0 * 1024.0):F2} MB";
        }
    }
}
```

### Step 6: Integrate Resource Manager with WebVerseRuntime

**File**: `Assets/Runtime/Runtime/Scripts/WebVerseRuntime.cs`

**Add Integration**:
```csharp
// Add to class members
private ResourceManager resourceManager;

// Add to Initialize method
public void InitializeResourceManager()
{
    GameObject rmObject = new GameObject("ResourceManager");
    rmObject.transform.SetParent(transform);
    resourceManager = rmObject.AddComponent<ResourceManager>();
    
    #if UNITY_WEBGL && !UNITY_EDITOR
    // Enable automatic cleanup for WebGL
    resourceManager.StartResourceCleanup();
    #endif
}

// Call from existing Initialize method
public void Initialize(...)
{
    // ... existing initialization code ...
    
    // Initialize resource management for WebGL
    InitializeResourceManager();
    
    // ... rest of initialization ...
}

// Add cleanup on world unload
public void UnloadWorld()
{
    // Existing world unload code...
    
    // Trigger immediate cleanup after unloading
    #if UNITY_WEBGL && !UNITY_EDITOR
    if (resourceManager != null)
    {
        resourceManager.CleanupNow();
    }
    #endif
}
```

## Asset Optimization

### Step 7: Create Editor Tool for Asset Analysis

**File**: `Assets/Editor/AssetMemoryAnalyzer.cs` (New File)

```csharp
// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FiveSQD.WebVerse.Editor
{
    /// <summary>
    /// Tool for analyzing asset memory usage.
    /// </summary>
    public class AssetMemoryAnalyzer : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<AssetInfo> assetInfos = new List<AssetInfo>();
        private bool showTexturesOnly = true;
        private int maxTextureSize = 2048;
        
        private struct AssetInfo
        {
            public string path;
            public string name;
            public long size;
            public string type;
            public int width;
            public int height;
        }
        
        [MenuItem("WebVerse/Tools/Asset Memory Analyzer")]
        public static void ShowWindow()
        {
            GetWindow<AssetMemoryAnalyzer>("Asset Memory Analyzer");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Asset Memory Analysis", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            showTexturesOnly = EditorGUILayout.Toggle("Show Textures Only", showTexturesOnly);
            maxTextureSize = EditorGUILayout.IntField("Max Texture Size", maxTextureSize);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Analyze Assets"))
            {
                AnalyzeAssets();
            }
            
            if (GUILayout.Button("Optimize Large Textures"))
            {
                OptimizeLargeTextures();
            }
            
            EditorGUILayout.Space();
            
            if (assetInfos.Count > 0)
            {
                GUILayout.Label($"Found {assetInfos.Count} assets", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var info in assetInfos.OrderByDescending(a => a.size))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(info.name, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"{info.size / 1024} KB", GUILayout.Width(100));
                    
                    if (info.width > 0)
                    {
                        EditorGUILayout.LabelField($"{info.width}x{info.height}", GUILayout.Width(100));
                    }
                    
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(info.path);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
        }
        
        private void AnalyzeAssets()
        {
            assetInfos.Clear();
            
            string searchFilter = showTexturesOnly ? "t:Texture2D" : "t:Object";
            string[] guids = AssetDatabase.FindAssets(searchFilter);
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                
                if (asset != null)
                {
                    AssetInfo info = new AssetInfo
                    {
                        path = path,
                        name = asset.name,
                        size = GetAssetSize(path),
                        type = asset.GetType().Name
                    };
                    
                    if (asset is Texture2D texture)
                    {
                        info.width = texture.width;
                        info.height = texture.height;
                    }
                    
                    assetInfos.Add(info);
                }
            }
            
            Debug.Log($"Analyzed {assetInfos.Count} assets.");
        }
        
        private long GetAssetSize(string path)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            return fileInfo.Exists ? fileInfo.Length : 0;
        }
        
        private void OptimizeLargeTextures()
        {
            int optimizedCount = 0;
            
            foreach (var info in assetInfos)
            {
                if (info.width > maxTextureSize || info.height > maxTextureSize)
                {
                    TextureImporter importer = AssetImporter.GetAtPath(info.path) as TextureImporter;
                    
                    if (importer != null)
                    {
                        importer.maxTextureSize = maxTextureSize;
                        AssetDatabase.ImportAsset(info.path);
                        optimizedCount++;
                    }
                }
            }
            
            Debug.Log($"Optimized {optimizedCount} textures.");
            AnalyzeAssets(); // Refresh
        }
    }
}
#endif
```

## Testing and Validation

### Step 8: Memory Profiling Test Scene

**File**: `Assets/Testing/MemoryProfilingTest.cs` (New File)

```csharp
// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace FiveSQD.WebVerse.Testing
{
    /// <summary>
    /// Memory profiling test utility for WebGL builds.
    /// </summary>
    public class MemoryProfilingTest : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private Text memoryText;
        
        [SerializeField]
        private Text fpsText;
        
        [Header("Settings")]
        [SerializeField]
        private float updateInterval = 1f;
        
        private float timer = 0f;
        private int frameCount = 0;
        private float fps = 0f;
        
        private void Update()
        {
            timer += Time.deltaTime;
            frameCount++;
            
            if (timer >= updateInterval)
            {
                fps = frameCount / timer;
                UpdateMemoryDisplay();
                
                timer = 0f;
                frameCount = 0;
            }
        }
        
        private void UpdateMemoryDisplay()
        {
            if (memoryText != null)
            {
                long managedMemory = System.GC.GetTotalMemory(false);
                memoryText.text = $"Memory: {managedMemory / (1024.0 * 1024.0):F2} MB";
            }
            
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {fps:F1}";
            }
        }
        
        public void TriggerGarbageCollection()
        {
            Debug.Log("Manual GC triggered");
            System.GC.Collect();
        }
        
        public void UnloadUnusedAssets()
        {
            Debug.Log("Unloading unused assets");
            Resources.UnloadUnusedAssets();
        }
    }
}
```

### Step 9: Automated Build Testing Script

**File**: `Assets/Build/BuildTester.cs` (New File)

```csharp
// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace FiveSQD.WebVerse.Building
{
    /// <summary>
    /// Automated testing for build optimizations.
    /// </summary>
    public class BuildTester
    {
        [MenuItem("WebVerse/Testing/Compare Build Sizes")]
        public static void CompareBuildSizes()
        {
            Debug.Log("=== Build Size Comparison ===");
            
            string buildRoot = "Builds";
            string[] buildDirs = new string[]
            {
                "WebGL-Compressed",
                "WebGL-Uncompressed",
                "WebGL-Production",
                "WebGL-Optimized"
            };
            
            foreach (string buildDir in buildDirs)
            {
                string path = Path.Combine(buildRoot, buildDir);
                if (Directory.Exists(path))
                {
                    long size = GetDirectorySize(path);
                    Debug.Log($"{buildDir}: {size / (1024.0 * 1024.0):F2} MB");
                }
                else
                {
                    Debug.Log($"{buildDir}: Not found");
                }
            }
        }
        
        private static long GetDirectorySize(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            long size = 0;
            
            foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                size += file.Length;
            }
            
            return size;
        }
    }
}
#endif
```

## Validation Checklist

After implementing the optimizations, validate using this checklist:

### Build Metrics
- [ ] Total build size reduced by 15-25%
- [ ] WASM file size reduced by 10-15%
- [ ] Initial load time improved by 20-30%

### Runtime Metrics
- [ ] Initial memory allocation: 64 MB
- [ ] Peak memory usage reduced by 15-30%
- [ ] Fewer memory growth operations
- [ ] Consistent frame times (no hitching during growth)

### Functional Testing
- [ ] All scenes load correctly
- [ ] No visual regressions
- [ ] Asset streaming works properly
- [ ] No memory leaks over extended sessions
- [ ] Exception handling works as expected

### Browser Compatibility
- [ ] Chrome/Chromium
- [ ] Firefox
- [ ] Safari (WebKit)
- [ ] Edge

## Rollback Plan

If issues arise:

1. **Revert Memory Settings**: Reset to original values in ProjectSettings.asset
2. **Disable Optimizations**: Set build flags to disable aggressive optimizations
3. **Re-enable Full Exceptions**: For debugging purposes
4. **Document Issues**: Log specific problems for future analysis

## Next Steps

1. Implement high-priority optimizations first
2. Test thoroughly in target browsers
3. Collect metrics before and after
4. Iterate based on real-world performance data
5. Update documentation with findings

## Support

For issues or questions:
- Email: fivesquaredtechnologies@gmail.com
- GitHub Issues: https://github.com/Five-Squared-Interactive/WebVerse-Runtime/issues

---
**Document Version**: 1.0  
**Date**: 2025-12-31  
**Author**: Implementation Guide for WebVerse-Runtime Memory Optimization Initiative
