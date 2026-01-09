# Handler System Architecture

This document provides an in-depth look at the WebVerse-Runtime handler system, including the base handler architecture, individual handler implementations, lifecycle management, and extension patterns.

## Handler System Overview

The handler system is the core architectural pattern of WebVerse-Runtime, providing a modular and extensible way to process different types of content and functionality.

### Design Principles

```plantuml
@startuml Handler-Design-Principles
!theme plain

title Handler System Design Principles

package "Design Principles" {
  [Separation of Concerns] -> [Each handler manages one responsibility]
  [Pluggable Architecture] -> [Handlers can be added/removed dynamically]
  [Standardized Interface] -> [Common lifecycle and communication patterns]
  [Dependency Management] -> [Clear dependency relationships]
  [Event-Driven Communication] -> [Loose coupling through events]
}

package "Benefits" {
  [Maintainability] -> [Easier to modify and debug]
  [Testability] -> [Individual handlers can be unit tested]
  [Extensibility] -> [New handlers can be added easily]
  [Reusability] -> [Handlers can be reused across projects]
}

[Design Principles] --> [Benefits]

@enduml
```

## BaseHandler Architecture

### Base Handler Interface

**Location**: `Assets/Runtime/Utilities/Scripts/BaseHandler.cs`

```csharp
public abstract class BaseHandler : MonoBehaviour
{
    protected bool isInitialized = false;
    
    /// <summary>
    /// Initialize the handler
    /// </summary>
    public virtual void Initialize()
    {
        isInitialized = true;
        LoggingUtility.Log($"[{GetType().Name}] Initialized");
    }
    
    /// <summary>
    /// Terminate the handler and clean up resources
    /// </summary>
    public virtual void Terminate()
    {
        isInitialized = false;
        LoggingUtility.Log($"[{GetType().Name}] Terminated");
    }
    
    /// <summary>
    /// Check if handler is properly initialized
    /// </summary>
    public bool IsInitialized => isInitialized;
}
```

### Handler Lifecycle

```plantuml
@startuml Handler-Lifecycle
!theme plain

title Handler Lifecycle Management

[*] -> Created : GameObject Creation
Created -> Initializing : Initialize() Called
Initializing -> Initialized : Initialization Complete
Initialized -> Processing : Normal Operations
Processing -> Processing : Handle Requests
Processing -> Terminating : Terminate() Called
Terminating -> Terminated : Cleanup Complete
Terminated -> [*] : GameObject Destroyed

note right of Processing
  Main operational state
  - Process content
  - Handle API calls
  - Manage resources
end note

@enduml
```

### Handler Registration and Discovery

```plantuml
@startuml Handler-Registration
!theme plain

title Handler Registration and Discovery

participant "WebVerse Runtime" as Runtime
participant "Handler Registry" as Registry
participant "Handler Factory" as Factory
participant "Handler Instance" as Handler

Runtime -> Registry : Register Handler Types
Runtime -> Factory : Create Handler Instances

loop For Each Handler Type
    Factory -> Handler : Instantiate
    Factory -> Handler : Configure Dependencies
    Handler -> Registry : Register Instance
    Runtime -> Handler : Initialize()
end

Runtime -> Registry : Get Handler by Type
Registry -> Runtime : Return Handler Instance

@enduml
```

## Core Handler Implementations

### FileHandler

**Purpose**: Manages file system operations and local caching
**Location**: `Assets/Runtime/Handlers/FileHandler/Scripts/FileHandler.cs`

#### Responsibilities
- Local file storage management
- Directory structure creation
- File existence checking and validation
- Image and binary file operations
- Cache management

#### FileHandler Architecture
```plantuml
@startuml FileHandler-Architecture
!theme plain

title FileHandler Internal Architecture

class FileHandler {
  -string fileDirectory
  -Dictionary<string, FileMetadata> fileCache
  +Initialize(string directory)
  +CreateFileInFileDirectory(string fileName, Texture2D image)
  +FileExistsInFileDirectory(string fileName) : bool
  +CreateDirectoryStructure(string fileName)
  +GetFileInfo(string fileName) : FileInfo
}

class FileMetadata {
  +string filePath
  +long fileSize
  +DateTime lastModified
  +string checksum
}

class DirectoryManager {
  +CreateDirectory(string path)
  +ValidateDirectoryPath(string path)
  +GetDirectoryInfo(string path) : DirectoryInfo
}

FileHandler --> FileMetadata : manages
FileHandler --> DirectoryManager : uses

@enduml
```

#### File Operation Flow
```plantuml
@startuml File-Operation-Flow
!theme plain

title FileHandler Operation Flow

start
:File Operation Request;
:Validate File Path;
:Check Security Constraints;

if (Directory Exists?) then (no)
  :Create Directory Structure;
endif

if (Read Operation?) then (yes)
  :Check File Cache;
  if (File in Cache?) then (yes)
    :Return Cached Data;
  else (no)
    :Read from File System;
    :Update Cache;
    :Return File Data;
  endif
elseif (Write Operation?) then (yes)
  :Prepare File Data;
  :Write to File System;
  :Update Cache;
  :Verify Write Success;
endif

:Operation Complete;
stop

@enduml
```

### VEMLHandler

**Purpose**: Processes Virtual Environment Markup Language documents
**Location**: `Assets/Runtime/Handlers/VEMLHandler/`

#### VEML Schema Evolution
```plantuml
@startuml VEML-Schema-Evolution
!theme plain

title VEML Schema Version Support

package "VEML Schema Versions" {
  [VEML 2.3] --> [VEML 2.4] : Automatic Conversion
  [VEML 2.4] --> [VEML 3.0] : Automatic Conversion
  [VEML 3.0] --> [Processing Engine] : Native Support
}

package "Conversion Utilities" {
  [V2.3 Converter] -> [V3.0 Schema]
  [V2.4 Converter] -> [V3.0 Schema]
}

[VEML 2.3] -> [V2.3 Converter]
[VEML 2.4] -> [V2.4 Converter]

@enduml
```

#### VEML Processing Pipeline
```plantuml
@startuml VEML-Processing-Pipeline
!theme plain

title VEML Document Processing Pipeline

start
:VEML Document Input;

partition "Document Parsing" {
  :Parse XML Structure;
  :Validate Schema Version;
  :Extract Document Metadata;
}

partition "Schema Conversion" {
  if (Version 3.0?) then (yes)
    :Process Directly;
  elseif (Version 2.4?) then (yes)
    :Convert V2.4 to V3.0;
  elseif (Version 2.3?) then (yes)
    :Convert V2.3 to V3.0;
  else (unsupported)
    :Throw Schema Error;
    stop
  endif
}

partition "Content Processing" {
  :Process Environment Settings;
  :Extract Entity Definitions;
  :Resolve Asset References;
  :Build Entity Hierarchy;
}

partition "Unity Integration" {
  :Create GameObjects;
  :Apply Components;
  :Set Properties;
  :Configure Relationships;
}

:VEML Scene Ready;
stop

@enduml
```

#### VEML Entity System
```plantuml
@startuml VEML-Entity-System
!theme plain

title VEML Entity System Architecture

abstract class VEMLEntity {
  +string id
  +Transform transform
  +Dictionary<string, object> properties
  +List<VEMLEntity> children
  +VEMLEntity parent
  +CreateGameObject() : GameObject
  +ApplyProperties()
}

class MeshEntity extends VEMLEntity {
  +string meshFile
  +Material[] materials
  +LoadMesh()
  +ApplyMaterials()
}

class LightEntity extends VEMLEntity {
  +LightType lightType
  +Color color
  +float intensity
  +ConfigureLight()
}

class TextEntity extends VEMLEntity {
  +string text
  +Font font
  +Color textColor
  +CreateTextMesh()
}

class VoxelEntity extends VEMLEntity {
  +Vector3Int voxelSize
  +VoxelData[] voxels
  +GenerateVoxelMesh()
}

VEMLEntity --> "1..*" VEMLEntity : children

@enduml
```

### JavaScriptHandler

**Purpose**: Executes JavaScript code and provides API integration
**Location**: `Assets/Runtime/Handlers/JavascriptHandler/Scripts/JavascriptHandler.cs`

#### JavaScript Execution Architecture
```plantuml
@startuml JavaScript-Execution-Architecture
!theme plain

title JavaScript Handler Architecture

class JavaScriptHandler {
  -ExecutionEngine jsEngine
  -Dictionary<string, Type> apiRegistry
  -Queue<ExecutionTask> taskQueue
  +Initialize()
  +RegisterAPI(string name, Type type)
  +ExecuteScript(string script) : object
  +QueueExecution(ExecutionTask task)
}

class ExecutionEngine {
  +Execute(string code) : object
  +SetGlobalVariable(string name, object value)
  +GetGlobalVariable(string name) : object
  +ValidateScript(string code) : bool
}

class APIRegistry {
  +RegisterType(string name, Type type)
  +GetType(string name) : Type
  +GetAPIHelpers() : List<APIHelper>
}

class ExecutionTask {
  +string script
  +Dictionary<string, object> parameters
  +Action<object> callback
  +DateTime createdAt
}

JavaScriptHandler --> ExecutionEngine : uses
JavaScriptHandler --> APIRegistry : manages
JavaScriptHandler --> ExecutionTask : queues

@enduml
```

#### JavaScript API System
```plantuml
@startuml JavaScript-API-System
!theme plain

title JavaScript API System Architecture

package "API Categories" {
  package "World Types" {
    [Vector2/3/4]
    [Color]
    [Transform]
    [Quaternion]
  }
  
  package "Entity APIs" {
    [Entity Management]
    [Component System]  
    [Hierarchy Operations]
  }
  
  package "Data APIs" {
    [JSON Processing]
    [Async Operations]
    [Data Persistence]
  }
  
  package "Utility APIs" {
    [Camera Control]
    [Input Handling]
    [Time Management]
    [Logging]
  }
  
  package "Environment APIs" {
    [Scene Management]
    [Lighting Control]
    [Physics Settings]
  }
}

[JavaScript Handler] --> [World Types]
[JavaScript Handler] --> [Entity APIs]
[JavaScript Handler] --> [Data APIs]
[JavaScript Handler] --> [Utility APIs]
[JavaScript Handler] --> [Environment APIs]

@enduml
```

#### JavaScript API Call Flow
```plantuml
@startuml JS-API-Call-Flow
!theme plain

title JavaScript API Call Processing Flow

participant "JavaScript Code" as JS
participant "JavaScript Handler" as JSHandler
participant "API Router" as Router
participant "API Helper" as Helper
participant "Unity System" as Unity

JS -> JSHandler : API Function Call (e.g., Entity.create())
JSHandler -> JSHandler : Validate Call
JSHandler -> Router : Route API Call
Router -> Helper : Get Appropriate Helper
Helper -> Helper : Process Parameters
Helper -> Unity : Execute Unity Operations
Unity -> Helper : Return Results
Helper -> Router : Format Response
Router -> JSHandler : API Response
JSHandler -> JS : Return Value

note right of Helper
  API Helpers bridge between
  JavaScript and Unity systems
end note

@enduml
```

### GLTFHandler

**Purpose**: Loads and processes GLTF 3D models
**Location**: `Assets/Runtime/Handlers/GLTFHandler/`

#### GLTF Import Pipeline
```plantuml
@startuml GLTF-Import-Pipeline
!theme plain

title GLTF Import Processing Pipeline

start
:GLTF File Request;

partition "File Loading" {
  :Load GLTF/GLB File;
  :Parse JSON Descriptor;
  :Extract Binary Data;
}

partition "Asset Extraction" {
  fork
    :Extract Meshes;
  fork again
    :Extract Materials;
  fork again  
    :Extract Textures;
  fork again
    :Extract Animations;
  end fork
}

partition "Unity Conversion" {
  :Convert Meshes to Unity Format;
  :Create Unity Materials;
  :Import Textures;
  :Setup Animation Controllers;
}

partition "Scene Integration" {
  :Create GameObject Hierarchy;
  :Apply Components;
  :Configure Renderers;
  :Setup Colliders;
}

:GLTF Model Ready;
stop

@enduml
```

#### GLTF Data Structure
```plantuml
@startuml GLTF-Data-Structure
!theme plain

title GLTF Data Structure Mapping

class GLTFDocument {
  +GLTFScene[] scenes
  +GLTFNode[] nodes
  +GLTFMesh[] meshes
  +GLTFMaterial[] materials
  +GLTFTexture[] textures
  +GLTFAnimation[] animations
}

class GLTFScene {
  +int[] nodeIndices
  +string name
}

class GLTFNode {
  +Transform transform
  +int meshIndex
  +int[] childIndices
  +string name
}

class GLTFMesh {
  +GLTFPrimitive[] primitives
  +string name
}

class GLTFMaterial {
  +PBRMetallicRoughness pbrMetallicRoughness
  +NormalTexture normalTexture
  +EmissiveTexture emissiveTexture
  +string name
}

GLTFDocument --> GLTFScene
GLTFDocument --> GLTFNode
GLTFDocument --> GLTFMesh
GLTFDocument --> GLTFMaterial
GLTFScene --> GLTFNode : references
GLTFNode --> GLTFMesh : references

@enduml
```

### ImageHandler

**Purpose**: Manages image loading and texture creation
**Location**: `Assets/Runtime/Handlers/ImageHandler/`

#### Image Processing Flow
```plantuml
@startuml Image-Processing-Flow
!theme plain

title Image Processing Flow

start
:Image Load Request;

partition "Format Detection" {
  :Analyze File Header;
  :Identify Image Format;
  if (Supported Format?) then (no)
    :Return Format Error;
    stop
  endif
}

partition "Image Loading" {
  :Load Image Data;
  :Decode Image Format;
  :Validate Image Properties;
}

partition "Texture Creation" {
  :Create Texture2D;
  :Apply Texture Settings;
  :Generate Mipmaps;
  :Compress if Needed;
}

partition "Memory Management" {
  :Cache Texture;
  :Track Memory Usage;
  if (Memory Limit Exceeded?) then (yes)
    :Cleanup Old Textures;
  endif
}

:Texture Ready;
stop

@enduml
```

### TimeHandler

**Purpose**: Provides time-related functionality and scheduling
**Location**: `Assets/Runtime/Handlers/TimeHandler/`

#### Time Management System
```plantuml
@startuml Time-Management-System
!theme plain

title Time Handler System Architecture

class TimeHandler {
  -List<ScheduledTask> scheduledTasks
  -Dictionary<string, Timer> namedTimers
  -float timeScale
  +ScheduleTask(float delay, Action callback)
  +CreateTimer(string name, float interval)
  +SetTimeScale(float scale)
  +GetCurrentTime() : DateTime
}

class ScheduledTask {
  +string id
  +float executeTime
  +Action callback
  +bool recurring
  +float interval
}

class Timer {
  +string name
  +float interval
  +float elapsed
  +bool isRunning
  +Action onTick
  +Start()
  +Stop()
  +Reset()
}

TimeHandler --> ScheduledTask : manages
TimeHandler --> Timer : manages

@enduml
```

## Handler Communication Patterns

### Inter-Handler Communication

```plantuml
@startuml Inter-Handler-Communication
!theme plain

title Handler Communication Patterns

participant "VEML Handler" as VEML
participant "Event System" as Events
participant "GLTF Handler" as GLTF
participant "File Handler" as Files
participant "JavaScript Handler" as JS

VEML -> Events : Publish "ModelNeeded" Event
Events -> GLTF : Route Event to GLTF Handler
GLTF -> Files : Request Model File
Files -> GLTF : Return File Data
GLTF -> Events : Publish "ModelLoaded" Event
Events -> VEML : Deliver Event
VEML -> JS : Trigger JavaScript Callbacks
JS -> Events : Publish "EntityCreated" Event

note over Events
  Central event system enables
  loose coupling between handlers
end note

@enduml
```

### Handler Dependencies

```plantuml
@startuml Handler-Dependencies
!theme plain

title Handler Dependency Graph

[WebVerse Runtime] --> [File Handler] : creates first
[WebVerse Runtime] --> [Time Handler] : creates early
[File Handler] --> [Image Handler] : required for images
[File Handler] --> [GLTF Handler] : required for models
[VEML Handler] --> [File Handler] : required for assets
[VEML Handler] --> [GLTF Handler] : required for 3D content
[VEML Handler] --> [Image Handler] : required for textures
[JavaScript Handler] --> [File Handler] : required for file ops
[JavaScript Handler] --> [VEML Handler] : required for entity ops
[JavaScript Handler] --> [Time Handler] : required for timing

@enduml
```

## Handler Extension Patterns

### Creating Custom Handlers

#### Custom Handler Template

```csharp
using FiveSQD.WebVerse.Utilities;
using UnityEngine;

namespace MyProject.Handlers
{
    /// <summary>
    /// Custom handler for specific functionality
    /// </summary>
    public class CustomHandler : BaseHandler
    {
        [Header("Custom Handler Settings")]
        public string customProperty = "default";
        
        private CustomProcessor processor;
        
        public override void Initialize()
        {
            // Validate dependencies
            if (string.IsNullOrEmpty(customProperty))
            {
                Logging.LogError("[CustomHandler] Custom property not set");
                return;
            }
            
            // Initialize components
            processor = new CustomProcessor(customProperty);
            
            // Call base initialization
            base.Initialize();
            
            Logging.Log("[CustomHandler] Initialized successfully");
        }
        
        public override void Terminate()
        {
            // Cleanup resources
            processor?.Dispose();
            processor = null;
            
            // Call base termination
            base.Terminate();
        }
        
        // Custom functionality
        public void ProcessCustomData(CustomData data)
        {
            if (!IsInitialized)
            {
                Logging.LogError("[CustomHandler] Handler not initialized");
                return;
            }
            
            processor.Process(data);
        }
    }
}
```

#### Handler Registration Pattern

```plantuml
@startuml Custom-Handler-Registration
!theme plain

title Custom Handler Registration Process

start
:Create Custom Handler Class;
:Inherit from BaseHandler;
:Implement Required Methods;

partition "Registration" {
  :Add Handler to Runtime GameObject;
  :Configure Handler Properties;
  :Set Up Dependencies;
}

partition "Integration" {
  :Register with Handler Registry;
  :Expose APIs if Needed;
  :Configure Event Handlers;
}

partition "Testing" {
  :Create Unit Tests;
  :Test Handler Lifecycle;
  :Validate Functionality;
}

:Custom Handler Ready;
stop

@enduml
```

### Handler Plugin Architecture

```plantuml
@startuml Handler-Plugin-Architecture
!theme plain

title Handler Plugin System Architecture

interface IHandlerPlugin {
  +string Name
  +Version Version
  +string[] Dependencies
  +Initialize()
  +Register(IHandlerRegistry registry)
  +Terminate()
}

class HandlerPlugin implements IHandlerPlugin {
  -string name
  -Version version  
  -BaseHandler[] handlers
  +LoadHandlers()
  +ValidateDependencies()
}

class HandlerRegistry {
  -Dictionary<string, Type> registeredHandlers
  -List<IHandlerPlugin> plugins
  +RegisterHandler(string name, Type handlerType)
  +LoadPlugin(IHandlerPlugin plugin)
  +GetHandler(string name) : Type
}

class PluginLoader {
  +LoadPlugin(string assemblyPath) : IHandlerPlugin
  +ValidatePlugin(IHandlerPlugin plugin) : bool
  +ResolveDependencies(IHandlerPlugin plugin)
}

HandlerRegistry --> IHandlerPlugin : manages
PluginLoader --> IHandlerPlugin : creates
HandlerPlugin --> BaseHandler : contains

@enduml
```

## Performance Optimization

### Handler Performance Patterns

#### Asynchronous Processing
```csharp
public class AsyncHandler : BaseHandler
{
    private readonly Queue<ProcessingTask> taskQueue = new Queue<ProcessingTask>();
    
    public async Task<T> ProcessAsync<T>(Func<T> operation)
    {
        return await Task.Run(() =>
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                Logging.LogError($"[AsyncHandler] Operation failed: {ex.Message}");
                throw;
            }
        });
    }
}
```

#### Object Pooling for Handlers
```plantuml
@startuml Handler-Object-Pooling
!theme plain

title Handler Object Pooling Pattern

class HandlerPool<T> {
  -Queue<T> availableHandlers
  -List<T> allHandlers
  -int maxPoolSize
  +Rent() : T
  +Return(T handler)
  +CreateHandler() : T
}

class PooledHandler {
  +bool isInUse
  +Reset()
  +Initialize()
  +Terminate()
}

HandlerPool --> PooledHandler : manages
PooledHandler --> BaseHandler : extends

@enduml
```

### Memory Management

#### Handler Resource Management
```plantuml
@startuml Handler-Resource-Management
!theme plain

title Handler Resource Management

start
:Handler Created;
:Allocate Resources;
:Register Resource Cleanup;

partition "Resource Tracking" {
  :Track Memory Usage;
  :Monitor File Handles;
  :Track Network Connections;
}

partition "Cleanup Triggers" {
  if (Memory Pressure?) then (yes)
    :Release Cached Resources;
  endif
  
  if (Handler Terminating?) then (yes)
    :Release All Resources;
  endif
  
  if (Idle Timeout?) then (yes)
    :Release Non-Essential Resources;
  endif
}

:Resources Managed;
stop

@enduml
```

## Testing Handlers

### Handler Testing Strategy

```plantuml
@startuml Handler-Testing-Strategy
!theme plain

title Handler Testing Strategy

package "Unit Tests" {
  [Handler Initialization Tests]
  [Handler Method Tests]  
  [Handler Termination Tests]
  [Error Handling Tests]
}

package "Integration Tests" {
  [Handler Communication Tests]
  [Dependency Tests]
  [Event System Tests]
  [API Integration Tests]
}

package "Performance Tests" {
  [Memory Usage Tests]
  [Processing Speed Tests]
  [Resource Cleanup Tests]
  [Concurrent Access Tests]
}

[Handler Implementation] --> [Unit Tests]
[Handler System] --> [Integration Tests]  
[Production Use] --> [Performance Tests]

@enduml
```

### Test Example

```csharp
[Test]
public void FileHandler_Initialize_CreatesDirectory()
{
    // Arrange
    var testDirectory = Path.Combine(Application.temporaryCache, "test");
    var fileHandler = new FileHandler();
    
    // Act
    fileHandler.Initialize(testDirectory);
    
    // Assert
    Assert.IsTrue(Directory.Exists(testDirectory));
    Assert.IsTrue(fileHandler.IsInitialized);
    
    // Cleanup
    fileHandler.Terminate();
    Directory.Delete(testDirectory, true);
}
```

This handler system provides a robust, extensible foundation for processing diverse content types while maintaining clean separation of concerns and enabling easy testing and maintenance.