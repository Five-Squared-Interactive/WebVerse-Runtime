# Implementation Plan: WebGL/WebGPU Memory Optimizations

## 1. Files to Create/Modify

### Files to Modify:
1. **`ProjectSettings/ProjectSettings.asset`** - WebGL memory configuration
2. **`ProjectSettings/QualitySettings.asset`** - Streaming mipmaps and WebGL quality preset
3. **`Assets/Build/Builder.cs`** - Build pipeline and exception handling updates
4. **`Assets/Runtime/WebVerseRuntime.cs`** - Resource cleanup integration (assuming this exists)

### Files to Create:
1. **`Assets/Runtime/ResourceManager.cs`** - New resource management system (if WebVerseRuntime doesn't exist)
2. **`docs/developer/webgl-memory-optimization-summary.md`** - Documentation

## 2. Key Changes for Each File

### A. `ProjectSettings/ProjectSettings.asset`
```yaml
# WebGL Memory Configuration
webGLMemorySize: 67108864        # 64 MB initial
webGLMaxMemorySize: 2147483648   # 2048 MB maximum  
webGLMemoryLinearGrowthStep: 33554432  # 32 MB linear step
webGLMemoryGeometricGrowthStep: 0.15   # 15% geometric step
webGLMemoryGeometricGrowthCap: 134217728  # 128 MB geometric cap
```

### B. `ProjectSettings/QualitySettings.asset`
```yaml
# Add to existing quality settings
streamingMipmapsActive: 1
streamingMipmapsMemoryBudget: 268435456  # 256 MB
streamingMipmapsAddAllCameras: 1
streamingMipmapsMaxLevelReduction: 3

# Add new WebGL-Optimized quality level
- name: WebGL-Optimized
  pixelLightCount: 1
  shadows: 1  # Hard shadows only
  shadowResolution: 0  # Low resolution
  antiAliasing: 0
  asyncUploadBufferSize: 8388608  # 8 MB
  asyncUploadTimeSlice: 2
  streamingMipmapsActive: 1
  streamingMipmapsMemoryBudget: 268435456
  particleRaycastBudget: 64
```

### C. `Assets/Build/Builder.cs`
```csharp
public class Builder : MonoBehaviour
{
    // Add method to configure WebGL-specific settings
    private static void ConfigureWebGLBuildSettings(ref BuildPlayerOptions buildOptions)
    {
        // Set WebGL-specific player settings
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.dataCaching = true;
        
        // Configure exception handling based on build type
        if (IsProductionBuild())
        {
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        }
        else
        {
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
        }
        
        // Set WebGL-Optimized quality level for WebGL builds
        SetWebGLQualitySettings();
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
                break;
            }
        }
    }
    
    private static bool IsProductionBuild()
    {
        // Check for production build indicators
        return !EditorUserBuildSettings.development && 
               (System.Environment.GetEnvironmentVariable("BUILD_TYPE") == "production" ||
                !System.Diagnostics.Debugger.IsAttached);
    }
    
    // Modify existing build method to include WebGL configuration
    public static void BuildWebGL()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        // ... existing build setup ...
        
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
        {
            ConfigureWebGLBuildSettings(ref buildOptions);
        }
        
        // ... rest of build process ...
    }
}
```

### D. `Assets/Runtime/ResourceManager.cs` (New File)
```csharp
using System.Collections;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager instance;
    public static ResourceManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ResourceManager");
                instance = go.AddComponent<ResourceManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StartCoroutine(PeriodicResourceCleanup());
#endif
    }
    
    private IEnumerator PeriodicResourceCleanup()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f); // Run every 60 seconds
            
            // Unload unused assets
            yield return Resources.UnloadUnusedAssets();
            
            // Force garbage collection
            System.GC.Collect();
            
            Debug.Log("[ResourceManager] Periodic cleanup completed");
        }
    }
    
    public void ForceCleanup()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        StartCoroutine(ForceCleanupCoroutine());
#endif
    }
    
    private IEnumerator ForceCleanupCoroutine()
    {
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Debug.Log("[ResourceManager] Force cleanup completed");
    }
}
```

### E. Integration with WebVerseRuntime (if it exists)
```csharp
// Add to existing WebVerseRuntime.cs
public class WebVerseRuntime : MonoBehaviour
{
    private void Start()
    {
        // ... existing initialization ...
        
#if UNITY_WEBGL && !UNITY_EDITOR
        // Initialize resource management for WebGL
        ResourceManager.Instance.gameObject.transform.SetParent(this.transform);
#endif
    }
}
```

### F. `docs/developer/webgl-memory-optimization-summary.md` (New File)
```markdown
# WebGL Memory Optimization Summary

## Overview
This document summarizes the memory optimizations implemented for WebGL builds.

## Memory Configuration
- Initial memory: 64 MB
- Maximum memory: 2048 MB  
- Linear growth: 32 MB steps
- Geometric growth: 15% with 128 MB cap

## Quality Settings
- New "WebGL-Optimized" preset created
- Streaming mipmaps enabled (256 MB budget)
- Reduced lighting and shadow quality
- Optimized async upload settings

## Build Configuration
- Gzip compression enabled
- Wasm linker target
- Data caching enabled
- Production builds use minimal exception support

## Resource Management
- Automatic cleanup every 60 seconds (WebGL only)
- Unloads unused assets and forces garbage collection
- Manual cleanup available via ResourceManager

## Performance Impact
- Reduced initial load time
- Lower memory footprint
- Improved streaming texture performance
- Better garbage collection behavior
```

## 3. Dependencies Required

### No New External Dependencies
- All changes use existing Unity APIs
- No additional Asset Store packages required
- Leverages built-in Unity WebGL optimization features

### Internal Dependencies
- Requires Unity 2021.3.26+ (already specified in README)
- Universal Render Pipeline (already in use)
- WebGL build target enabled

## 4. Testing Approach

### A. Development Testing
```csharp
// Add to Builder.cs for testing
#if UNITY_EDITOR
public static void TestWebGLSettings()
{
    Debug.Log($"WebGL Memory Size: {PlayerSettings.WebGL.memorySize}");
    Debug.Log($"WebGL Max Memory: {PlayerSettings.WebGL.maxMemorySize}");
    Debug.Log($"Streaming Mipmaps: {QualitySettings.streamingMipmapsActive}");
    Debug.Log($"Current Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
}

[MenuItem("WebVerse/Test WebGL Settings")]
public static void TestSettings()
{
    TestWebGLSettings();
}
#endif
```

### B. Build Testing
1. **Memory Configuration Test**
   - Build WebGL version
   - Monitor browser console for memory allocation
   - Verify initial 64MB allocation
   - Test memory growth under load

2. **Quality Settings Test**
   - Verify WebGL-Optimized preset is active
   - Check streaming mipmaps are functioning
   - Validate reduced shadow/lighting quality

3. **Resource Cleanup Test**
   - Monitor memory usage over time
   - Verify 60-second cleanup cycles
   - Test manual cleanup functionality

### C. Performance Testing
```javascript
// Browser console test script
function monitorWebGLMemory() {
    if (performance.memory) {
        console.log('Used:', Math.round(performance.memory.usedJSHeapSize / 1048576) + 'MB');
        console.log('Total:', Math.round(performance.memory.totalJSHeapSize / 1048576) + 'MB');
        console.log('Limit:', Math.round(performance.memory.jsHeapSizeLimit / 1048576) + 'MB');
    }
}
setInterval(monitorWebGLMemory, 5000); // Monitor every 5 seconds
```

## 5. Potential Challenges and Solutions

### A. Challenge: ProjectSettings Asset Conflicts
**Problem**: Manual editing of Unity's binary asset files can cause corruption
**Solution**: 
- Use Unity Editor scripting to modify settings programmatically
- Create EditorScript for one-time setup:

```csharp
#if UNITY_EDITOR
[MenuItem("WebVerse/Setup WebGL Optimizations")]
public static void SetupWebGLOptimizations()
{
    // Configure WebGL memory settings
    PlayerSettings.WebGL.memorySize = 64;
    PlayerSettings.WebGL.maxMemorySize = 2048;
    // ... other settings
    
    // Save settings
    AssetDatabase.SaveAssets();
    Debug.Log("WebGL optimizations applied");
}
#endif
```

### B. Challenge: Quality Settings Integration
**Problem**: Adding new quality level without breaking existing presets
**Solution**:
- Append new quality level instead of inserting
- Preserve existing quality level indices
- Use platform-specific quality overrides

```csharp
public static void CreateWebGLQualityLevel()
{
    // Get current quality settings
    SerializedObject qualitySettings = new SerializedObject(QualitySettings.GetQualitySettings());
    SerializedProperty qualityLevels = qualitySettings.FindProperty("m_QualitySettings");
    
    // Add new quality level at the end
    qualityLevels.InsertArrayElementAtIndex(qualityLevels.arraySize);
    SerializedProperty newLevel = qualityLevels.GetArrayElementAtIndex(qualityLevels.arraySize - 1);
    
    // Configure properties
    newLevel.FindPropertyRelative("name").stringValue = "WebGL-Optimized";
    // ... set other properties
    
    qualitySettings.ApplyModifiedProperties();
}
```

### C. Challenge: Resource Manager Lifecycle
**Problem**: Ensuring ResourceManager survives scene changes and doesn't create duplicates
**Solution**:
- Implement singleton pattern with DontDestroyOnLoad
- Check for existing instances before creation
- Integrate with existing WebVerseRuntime lifecycle

### D. Challenge: Production vs Debug Build Detection
**Problem**: Reliably determining build type for exception handling
**Solution**:
- Use multiple indicators (development flag, environment variables, debugger attachment)
- Provide manual override option
- Default to safe production settings when in doubt

```csharp
private static bool IsProductionBuild()
{
    // Multiple checks for reliability
    bool isDevelopmentBuild = EditorUserBuildSettings.development;
    bool hasDebugger = System.Diagnostics.Debugger.IsAttached;
    string buildType = System.Environment.GetEnvironmentVariable("BUILD_TYPE");
    
    // If any indicator suggests debug, use debug settings
    if (isDevelopmentBuild || hasDebugger || buildType == "debug")
        return false;
        
    // Default to production for safety
    return true;
}
```

### E. Challenge: Memory Budget Validation
**Problem**: Ensuring memory settings don't exceed browser/device limits
**Solution**:
- Add validation in build process
- Provide warnings for potentially problematic configurations
- Include fallback values

```csharp
private static void ValidateWebGLMemorySettings()
{
    int maxMemory = PlayerSettings.WebGL.maxMemorySize;
    if (maxMemory > 2048)
    {
        Debug.LogWarning($"WebGL max memory ({maxMemory}MB) exceeds recommended 2048MB");
    }
    
    int streamingBudget = QualitySettings.streamingMipmapsMemoryBudget / (1024 * 1024);
    if (streamingBudget > maxMemory / 8)
    {
        Debug.LogWarning($"Streaming mipmap budget ({streamingBudget}MB) is too high relative to max memory");
    }
}
```

## Implementation Order

1. **Create ResourceManager** - Independent component, easiest to test
2. **Update Builder.cs** - Build pipeline changes, can be tested immediately
3. **Configure ProjectSettings** - Use Editor script approach for safety
4. **Update QualitySettings** - Add WebGL-Optimized preset
5. **Integration Testing** - Full WebGL build and performance testing
6. **Documentation** - Complete implementation summary

This plan provides a comprehensive approach to implementing all high-priority WebGL memory optimizations while maintaining system stability and following the specified constraints.