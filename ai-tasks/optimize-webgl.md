## **Title**  
Implement High‑Priority WebGL/WebGPU Memory Optimizations

## **Summary**  
Apply all high‑priority memory and performance optimizations identified in the WebGL/WebGPU Memory Optimization Analysis. This includes updating memory configuration, enabling streaming mipmaps, reducing exception support in production builds, and adding WebGL‑optimized quality settings.

## **Detailed Requirements**

### **1. Update WebGL Memory Configuration**
Modify `ProjectSettings/ProjectSettings.asset` to apply:
- Initial memory size: **64 MB**  
- Maximum memory size: **2048 MB**  
- Linear growth step: **32 MB**  
- Geometric growth step: **0.15**  
- Geometric growth cap: **128 MB**

### **2. Enable Streaming Mipmaps**
Update `QualitySettings.asset`:
- `streamingMipmapsActive = 1`  
- `streamingMipmapsMemoryBudget = 256`  
- `streamingMipmapsAddAllCameras = 1`  
- `streamingMipmapsMaxLevelReduction = 3`

### **3. Add WebGL‑Optimized Quality Preset**
Create a new quality level named **WebGL‑Optimized** with:
- `pixelLightCount = 1`  
- `shadows = 1`  
- `shadowResolution = 0`  
- `antiAliasing = 0`  
- `asyncUploadBufferSize = 8`  
- `asyncUploadTimeSlice = 2`  
- `streamingMipmapsActive = 1`  
- `streamingMipmapsMemoryBudget = 256`  
- `particleRaycastBudget = 64`

Set this preset as the default for WebGL.

### **4. Reduce Exception Support for Production Builds**
In `Builder.cs`:
- Production builds use `ExplicitlyThrownExceptionsOnly`  
- Debug builds retain full exception support

### **5. Update Build Pipeline Settings**
In `Builder.cs`:
- Ensure compression = Gzip  
- Enable decompression fallback  
- Ensure linker target = Wasm  
- Ensure data caching is enabled  

### **6. Add Resource Cleanup Coroutine**
Add periodic cleanup to WebVerseRuntime or a new ResourceManager:
- Runs every 60 seconds  
- Calls `Resources.UnloadUnusedAssets()`  
- Calls `System.GC.Collect()`  
- WebGL‑only

## **Acceptance Criteria**
- WebGL builds use updated memory configuration  
- Streaming mipmaps enabled and functioning  
- WebGL‑Optimized quality preset appears in ProjectSettings and is active for WebGL  
- Production builds use reduced exception support  
- Build pipeline applies compression, caching, and correct linker settings  
- Resource cleanup runs automatically in WebGL builds  
- All changes compile and produce a working WebGL build  
- Documentation updated in `docs/developer/webgl-memory-optimization-summary.md`

## **Safety Constraints**
- Do not modify non‑WebGL platform settings  
- Do not change unrelated quality presets  
- Do not modify asset files outside ProjectSettings or Build scripts  
- Do not introduce breaking API changes  
- All changes must remain within allowed repo paths:
  - `ProjectSettings/`  
  - `Assets/Build/`  
  - `Assets/Runtime/`  
  - `docs/developer/`

## **Expected Outputs**
- Updated ProjectSettings with optimized memory configuration  
- New WebGL‑Optimized quality preset  
- Updated Builder.cs with production/debug exception logic  
- New or updated ResourceManager integration  
- Updated documentation summarizing changes  
- A successful WebGL build demonstrating reduced memory usage
