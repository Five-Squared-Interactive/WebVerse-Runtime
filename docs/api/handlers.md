# Handler APIs Reference

This document provides detailed API reference for all handlers in the WebVerse-Runtime system, including method signatures, parameters, return values, and usage examples.

## BaseHandler API

All handlers inherit from the `BaseHandler` base class, providing common functionality and lifecycle management.

### BaseHandler Class

```csharp
public abstract class BaseHandler : MonoBehaviour
{
    protected bool isInitialized { get; private set; }
    
    public virtual void Initialize();
    public virtual void Terminate();
    public bool IsInitialized { get; }
}
```

#### Methods

##### Initialize()
Initializes the handler and sets up required resources.

**Signature**: `public virtual void Initialize()`
**Returns**: `void`
**Throws**: May throw initialization exceptions if setup fails

**Example**:
```csharp
var handler = GetComponent<MyCustomHandler>();
handler.Initialize();
if (handler.IsInitialized)
{
    // Handler ready for use
}
```

##### Terminate()
Terminates the handler and cleans up resources.

**Signature**: `public virtual void Terminate()`
**Returns**: `void`

## FileHandler API

Manages file system operations and local file caching.

### FileHandler Class

```csharp
public class FileHandler : BaseHandler
{
    public string fileDirectory { get; private set; }
    
    public void Initialize(string fileDirectory);
    public bool FileExistsInFileDirectory(string fileName);
    public void CreateFileInFileDirectory(string fileName, Texture2D image);
    public void CreateDirectoryStructure(string fileName);
    public string GetFullPath(string fileName);
    public FileInfo GetFileInfo(string fileName);
    public void DeleteFileInFileDirectory(string fileName);
}
```

#### Properties

##### fileDirectory
Gets the current file directory path.

**Type**: `string`
**Access**: Read-only

#### Methods

##### Initialize(string fileDirectory)
Initializes the file handler with a specific directory.

**Parameters**:
- `fileDirectory` (string): Directory path for file operations

**Example**:
```csharp
var fileHandler = GetComponent<FileHandler>();
fileHandler.Initialize("WebVerseFiles");
```

##### FileExistsInFileDirectory(string fileName)
Checks if a file exists in the file directory.

**Parameters**:
- `fileName` (string): Relative path to the file

**Returns**: `bool` - True if file exists, false otherwise

**Example**:
```csharp
if (fileHandler.FileExistsInFileDirectory("textures/wall.png"))
{
    // File exists, proceed with loading
}
```

##### CreateFileInFileDirectory(string fileName, Texture2D image)
Creates an image file in the file directory.

**Parameters**:
- `fileName` (string): Relative path for the new file
- `image` (Texture2D): Image data to write

**Example**:
```csharp
Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
fileHandler.CreateFileInFileDirectory("screenshots/capture.png", screenshot);
```

##### GetFullPath(string fileName)
Gets the full system path for a relative file name.

**Parameters**:
- `fileName` (string): Relative file path

**Returns**: `string` - Full system path

**Example**:
```csharp
string fullPath = fileHandler.GetFullPath("models/character.gltf");
// Returns: "C:/WebVerse/WebVerseFiles/models/character.gltf"
```

## VEMLHandler API

Processes Virtual Environment Markup Language documents.

### VEMLHandler Class

```csharp
public class VEMLHandler : BaseHandler
{
    public WebVerseRuntime runtime;
    public float timeout { get; set; }
    
    public void LoadVEMLDocumentIntoWorld(string resourceURI, Action<bool> onComplete);
    public void GetWorldName(string resourceURI, Action<string> onComplete);
}
```

#### Properties

##### runtime
Reference to the WebVerse runtime instance.

**Type**: `WebVerseRuntime`
**Access**: Public

##### timeout
Timeout for VEML loading requests in seconds.

**Type**: `float`
**Default**: `10`

#### Methods

##### LoadVEMLDocumentIntoWorld(string resourceURI, Action<bool> onComplete)
Loads a VEML document from a resource URI into the current world.

**Parameters**:
- `resourceURI` (string): URI of the VEML resource to load
- `onComplete` (Action<bool>): Callback executed when loading completes

**Example**:
```csharp
vemlHandler.LoadVEMLDocumentIntoWorld("https://example.com/scenes/main.veml", (success) =>
{
    if (success)
    {
        Debug.Log("VEML document loaded successfully into world");
    }
    else
    {
        Debug.LogError("Failed to load VEML document");
    }
});
```

##### GetWorldName(string resourceURI, Action<string> onComplete)
Gets the name of a world from a VEML resource URI.

**Parameters**:
- `resourceURI` (string): URI of the VEML resource
- `onComplete` (Action<string>): Callback executed with the world name

**Example**:
```csharp
vemlHandler.GetWorldName("https://example.com/scenes/main.veml", (worldName) =>
{
    Debug.Log("World name: " + worldName);
});
```

**Note**: The VEMLHandler automatically handles schema validation and legacy version conversion (from versions 1.0 through 2.4 to 3.0) without requiring configuration. This behavior is built into the loading process and cannot be disabled.

## JavaScriptHandler API

Executes JavaScript code and provides API integration.

### JavaScriptHandler Class

```csharp
public class JavaScriptHandler : BaseHandler
{
    public int maxExecutionTimeMs { get; set; }
    public int maxMemoryUsageMB { get; set; }
    public bool enableConsoleLogging { get; set; }
    
    public object ExecuteScript(string script);
    public void ExecuteScriptAsync(string script, Action<object> onComplete);
    public void RegisterAPI(string name, Type type);
    public void SetGlobalVariable(string name, object value);
    public object GetGlobalVariable(string name);
    public void ClearGlobals();
}
```

#### Properties

##### maxExecutionTimeMs
Maximum execution time for JavaScript scripts in milliseconds.

**Type**: `int`
**Default**: `100`

##### maxMemoryUsageMB
Maximum memory usage for JavaScript execution in megabytes.

**Type**: `int`
**Default**: `64`

##### enableConsoleLogging
Controls whether JavaScript console output is logged.

**Type**: `bool`
**Default**: `true`

#### Methods

##### ExecuteScript(string script)
Executes JavaScript code synchronously.

**Parameters**:
- `script` (string): JavaScript code to execute

**Returns**: `object` - Execution result

**Throws**: `JavaScriptExecutionException` if execution fails

**Example**:
```csharp
string jsCode = "var result = 5 + 3; result;";
object result = jsHandler.ExecuteScript(jsCode);
Debug.Log("Result: " + result); // Output: Result: 8
```

##### ExecuteScriptAsync(string script, Action<object> onComplete)
Executes JavaScript code asynchronously.

**Parameters**:
- `script` (string): JavaScript code to execute
- `onComplete` (Action<object>): Callback with execution result

**Example**:
```csharp
string jsCode = "Entity.create('TestEntity');";
jsHandler.ExecuteScriptAsync(jsCode, (result) =>
{
    Debug.Log("Script executed, result: " + result);
});
```

##### RegisterAPI(string name, Type type)
Registers a C# type to be accessible from JavaScript.

**Parameters**:
- `name` (string): Name to use in JavaScript
- `type` (Type): C# type to register

**Example**:
```csharp
jsHandler.RegisterAPI("MyAPI", typeof(MyCustomAPI));
// Now accessible in JavaScript as: MyAPI.someMethod()
```

## GLTFHandler API

Handles loading and processing of GLTF 3D models.

### GLTFHandler Class

```csharp
public class GLTFHandler : BaseHandler
{
    public bool importAnimations { get; set; }
    public bool importMaterials { get; set; }
    public int maxTextureSize { get; set; }
    
    public void LoadGLTF(string filePath, Action<GameObject> onComplete);
    public void LoadGLTFFromBytes(byte[] data, Action<GameObject> onComplete);
    public GLTFImportSettings GetImportSettings();
    public void SetImportSettings(GLTFImportSettings settings);
}
```

#### Properties

##### importAnimations
Controls whether animations are imported from GLTF files.

**Type**: `bool`
**Default**: `true`

##### importMaterials
Controls whether materials are imported from GLTF files.

**Type**: `bool`
**Default**: `true`

##### maxTextureSize
Maximum texture size for imported textures.

**Type**: `int`
**Default**: `2048`

#### Methods

##### LoadGLTF(string filePath, Action<GameObject> onComplete)
Loads a GLTF model from a file path.

**Parameters**:
- `filePath` (string): Path to the GLTF file
- `onComplete` (Action<GameObject>): Callback with loaded GameObject

**Example**:
```csharp
gltfHandler.LoadGLTF("models/character.gltf", (loadedModel) =>
{
    if (loadedModel != null)
    {
        loadedModel.transform.position = Vector3.zero;
        Debug.Log("Model loaded: " + loadedModel.name);
    }
});
```

##### LoadGLTFFromBytes(byte[] data, Action<GameObject> onComplete)
Loads a GLTF model from byte array data.

**Parameters**:
- `data` (byte[]): GLTF file data as bytes
- `onComplete` (Action<GameObject>): Callback with loaded GameObject

## ImageHandler API

Manages image loading and texture creation.

### ImageHandler Class

```csharp
public class ImageHandler : BaseHandler
{
    public bool enableTextureCompression { get; set; }
    public int maxTextureSize { get; set; }
    public bool generateMipmaps { get; set; }
    
    public void LoadImage(string filePath, Action<Texture2D> onComplete);
    public void LoadImageFromBytes(byte[] data, Action<Texture2D> onComplete);
    public Texture2D CreateTextureFromColor(Color color, int width = 1, int height = 1);
    public void OptimizeTexture(Texture2D texture);
}
```

#### Properties

##### enableTextureCompression
Controls whether loaded textures are compressed.

**Type**: `bool`
**Default**: `true`

##### maxTextureSize
Maximum size for loaded textures.

**Type**: `int`
**Default**: `2048`

##### generateMipmaps
Controls whether mipmaps are generated for textures.

**Type**: `bool`
**Default**: `true`

#### Methods

##### LoadImage(string filePath, Action<Texture2D> onComplete)
Loads an image from a file path.

**Parameters**:
- `filePath` (string): Path to the image file
- `onComplete` (Action<Texture2D>): Callback with loaded texture

**Example**:
```csharp
imageHandler.LoadImage("textures/wall.png", (texture) =>
{
    if (texture != null)
    {
        renderer.material.mainTexture = texture;
    }
});
```

##### CreateTextureFromColor(Color color, int width, int height)
Creates a solid color texture.

**Parameters**:
- `color` (Color): Color for the texture
- `width` (int): Texture width (default: 1)
- `height` (int): Texture height (default: 1)

**Returns**: `Texture2D` - Created texture

**Example**:
```csharp
Texture2D redTexture = imageHandler.CreateTextureFromColor(Color.red, 256, 256);
```

## TimeHandler API

Provides time-related functionality and scheduling services.

### TimeHandler Class

```csharp
public class TimeHandler : BaseHandler
{
    public float timeScale { get; set; }
    
    public void ScheduleTask(float delay, Action callback);
    public void ScheduleRepeatingTask(float interval, Action callback);
    public string CreateTimer(string name, float interval, Action onTick);
    public void StartTimer(string name);
    public void StopTimer(string name);
    public void RemoveTimer(string name);
    public DateTime GetCurrentTime();
    public float GetDeltaTime();
}
```

#### Properties

##### timeScale
Controls the time scale for scheduled tasks.

**Type**: `float`
**Default**: `1.0f`

#### Methods

##### ScheduleTask(float delay, Action callback)
Schedules a task to execute after a delay.

**Parameters**:
- `delay` (float): Delay in seconds
- `callback` (Action): Callback to execute

**Example**:
```csharp
timeHandler.ScheduleTask(2.0f, () =>
{
    Debug.Log("Task executed after 2 seconds");
});
```

##### CreateTimer(string name, float interval, Action onTick)
Creates a named timer with a regular interval.

**Parameters**:
- `name` (string): Timer name for reference
- `interval` (float): Tick interval in seconds
- `onTick` (Action): Callback executed on each tick

**Returns**: `string` - Timer ID

**Example**:
```csharp
timeHandler.CreateTimer("GameTimer", 1.0f, () =>
{
    Debug.Log("Timer tick every second");
});
timeHandler.StartTimer("GameTimer");
```

## Manager APIs

### InputManager API

```csharp
public class InputManager : BaseManager
{
    public bool inputEnabled { get; set; }
    public BasePlatformInput platformInput { get; set; }
    
    public void RegisterLeftFunction(string functionName);
    public void RegisterRightFunction(string functionName);
    public void RegisterKeyFunction(string key, string functionName);
    public void EnableInput();
    public void DisableInput();
    public Vector2 GetMousePosition();
    public bool IsKeyPressed(string key);
}
```

### OutputManager API

```csharp
public class OutputManager : BaseManager
{
    public float screenCheckPeriod { get; set; }
    
    public void RegisterScreenSizeChangeAction(Action<int, int> action);
    public Vector2 GetScreenSize();
    public void SetResolution(int width, int height);
    public bool IsFullscreen();
    public void SetFullscreen(bool fullscreen);
}
```

## Error Handling

All handler APIs follow consistent error handling patterns:

### Exception Types

```csharp
public class HandlerException : Exception
{
    public string HandlerName { get; }
    public HandlerException(string handlerName, string message) : base(message)
    {
        HandlerName = handlerName;
    }
}

public class InitializationException : HandlerException
{
    public InitializationException(string handlerName, string message) 
        : base(handlerName, message) { }
}

public class ProcessingException : HandlerException
{
    public ProcessingException(string handlerName, string message) 
        : base(handlerName, message) { }
}
```

### Error Handling Patterns

```csharp
// Synchronous error handling
try
{
    handler.ProcessSomething();
}
catch (HandlerException ex)
{
    Debug.LogError($"Handler {ex.HandlerName} error: {ex.Message}");
}

// Asynchronous error handling
handler.ProcessSomethingAsync((result, error) =>
{
    if (error != null)
    {
        Debug.LogError($"Async operation failed: {error.Message}");
    }
    else
    {
        // Process result
    }
});
```

## Best Practices

### Handler Lifecycle Management

```csharp
// Proper handler initialization
public class MyComponent : MonoBehaviour
{
    private FileHandler fileHandler;
    
    void Start()
    {
        fileHandler = FindObjectOfType<FileHandler>();
        if (fileHandler == null || !fileHandler.IsInitialized)
        {
            Debug.LogError("FileHandler not available or not initialized");
            return;
        }
        
        // Use handler safely
        UseHandler();
    }
    
    void OnDestroy()
    {
        // Cleanup if needed
        if (fileHandler != null)
        {
            // Unregister callbacks, clean up references
        }
    }
}
```

### Async Pattern Usage

```csharp
// Proper async handler usage
public async Task LoadContentAsync()
{
    var tcs = new TaskCompletionSource<bool>();
    
    vemlHandler.LoadVEMLDocument("scene.veml", (success) =>
    {
        tcs.SetResult(success);
    });
    
    bool result = await tcs.Task;
    if (result)
    {
        Debug.Log("Content loaded successfully");
    }
}
```

This API reference provides comprehensive documentation for all handler interfaces, enabling developers to effectively integrate and extend the WebVerse-Runtime system.