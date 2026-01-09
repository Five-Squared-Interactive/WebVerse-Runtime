# Component Overview

This document provides detailed information about each major component in the WebVerse-Runtime system, including their responsibilities, interfaces, and implementation details.

## Core Runtime Components

### WebVerseRuntime

**Location**: `Assets/Runtime/Runtime/Scripts/WebVerseRuntime.cs`
**Inherits**: `MonoBehaviour`

The main orchestrator for the entire WebVerse-Runtime system.

#### Responsibilities
- Component lifecycle management
- Runtime configuration
- Error handling and logging coordination
- Scene management integration

#### Key Properties
```csharp
public LocalStorageManager localStorageManager;
public InputManager inputManager;
public OutputManager outputManager;
public HTTPRequestManager httpRequestManager;
public WebVerseWebView webverseWebView;
```

#### Component Initialization Flow
```plantuml
@startuml WebVerseRuntime-Initialization
!theme plain

title WebVerseRuntime Component Initialization

start
:Create Handlers GameObject;
:Initialize Local Storage Manager;
:Initialize File Handler;
:Initialize VEML Handler;
:Initialize GLTF Handler;
:Initialize Image Handler;
:Initialize JavaScript Handler;
:Setup JavaScript API Helpers;
:Initialize Input Manager;
:Initialize Output Manager;
:Initialize HTTP Request Manager;
:Initialize WebVerse WebView;
:Setup Console Logging;
:Runtime Ready;
stop

@enduml
```

#### Configuration
- Storage mode (Persistent/Cache)
- File directories
- Timeout settings
- Handler-specific configurations

## Handler Components

### BaseHandler

**Location**: `Assets/Runtime/Utilities/Scripts/BaseHandler.cs`
**Type**: Abstract Base Class

Provides common functionality for all handlers in the system.

#### Standard Interface
```csharp
public abstract class BaseHandler : MonoBehaviour
{
    public virtual void Initialize();
    public virtual void Terminate();
    protected bool isInitialized;
}
```

### FileHandler

**Location**: `Assets/Runtime/Handlers/FileHandler/Scripts/FileHandler.cs`
**Inherits**: `BaseHandler`

Manages file system operations and local file caching.

#### Responsibilities
- Local file storage management
- Directory structure creation
- File existence checking
- Image file operations

#### Key Methods
```csharp
public void Initialize(string fileDirectory);
public bool FileExistsInFileDirectory(string fileName);
public void CreateFileInFileDirectory(string fileName, Texture2D image);
public void CreateDirectoryStructure(string fileName);
```

#### File Operations Flow
```plantuml
@startuml FileHandler-Operations
!theme plain

title File Handler Operations Flow

start
:File Operation Request;
if (File exists?) then (yes)
  :Return existing file;
else (no)
  :Create directory structure;
  :Perform file operation;
  :Cache file locally;
endif
:Return result;
stop

@enduml
```

### VEMLHandler

**Location**: `Assets/Runtime/Handlers/VEMLHandler/`

Processes Virtual Environment Markup Language documents and converts them to Unity scene hierarchies.

#### Responsibilities
- VEML document parsing
- Schema version conversion (V2.3, V2.4, V3.0)
- Entity creation and hierarchy setup
- Asset reference resolution

#### Schema Support
- **VEML 3.0**: Current primary schema
- **VEML 2.4**: Legacy support with automatic conversion
- **VEML 2.3**: Legacy support with automatic conversion

#### VEML Processing Flow
```plantuml
@startuml VEML-Processing
!theme plain

title VEML Document Processing Flow

start
:Receive VEML Document;
:Validate Schema Version;
if (Version 3.0?) then (yes)
  :Process directly;
elseif (Version 2.4?) then (yes)
  :Convert from V2.4 to V3.0;
elseif (Version 2.3?) then (yes)
  :Convert from V2.3 to V3.0;
else (unknown)
  :Error - Unsupported Version;
  stop
endif
:Parse Environment Settings;
:Process Entity Hierarchy;
:Resolve Asset References;
:Create Unity GameObjects;
:Apply Components and Settings;
:Scene Ready;
stop

@enduml
```

### GLTFHandler

**Location**: `Assets/Runtime/Handlers/GLTFHandler/`

Handles loading and processing of GLTF (GL Transmission Format) 3D models.

#### Responsibilities
- GLTF model loading
- Texture and material application
- Animation setup
- Scene hierarchy integration

#### GLTF Import Pipeline
```plantuml
@startuml GLTF-Import-Pipeline
!theme plain

title GLTF Import Pipeline

start
:GLTF File Request;
:Load GLTF Data;
:Parse JSON Descriptor;
:Load Binary Data (GLB);
:Extract Meshes;
:Extract Materials;
:Extract Textures;
:Extract Animations;
:Create Unity GameObject Hierarchy;
:Apply Components;
:Integrate with World Engine;
:Model Ready;
stop

@enduml
```

### JavaScriptHandler

**Location**: `Assets/Runtime/Handlers/JavascriptHandler/Scripts/JavascriptHandler.cs`
**Inherits**: `BaseHandler`

Executes JavaScript code and provides API integration with Unity systems.

#### Responsibilities
- JavaScript code execution
- API exposure to JavaScript
- Event handling and callbacks
- Bridge between scripts and Unity

#### API Categories
```plantuml
@startuml JavaScript-API-Structure
!theme plain

title JavaScript API Structure

package "World Types" {
  [Vector2, Vector3, Vector4]
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
  [Data Persistence]
  [Async Operations]
}

package "Environment APIs" {
  [Scene Management]
  [Lighting Control]
  [Physics Settings]
}

package "Utility APIs" {
  [Camera Control]
  [Input Handling]
  [Logging]
  [Time Management]
}

package "Networking APIs" {
  [HTTP Requests]
  [WebSocket Communication]
  [VOS Synchronization]
}

[JavaScriptHandler] --> "World Types"
[JavaScriptHandler] --> "Entity APIs"
[JavaScriptHandler] --> "Data APIs"
[JavaScriptHandler] --> "Environment APIs"
[JavaScriptHandler] --> "Utility APIs"
[JavaScriptHandler] --> "Networking APIs"

@enduml
```

#### JavaScript Execution Model
```plantuml
@startuml JavaScript-Execution
!theme plain

title JavaScript Execution Model

start
:JavaScript Code Request;
:Queue Execution Task;
:Validate Script;
if (Script Valid?) then (yes)
  :Execute on Main Thread;
  :Process API Calls;
  :Update Unity Systems;
  :Return Results;
else (no)
  :Log Error;
  :Return Error;
endif
:Execution Complete;
stop

@enduml
```

### ImageHandler

**Location**: `Assets/Runtime/Handlers/ImageHandler/`

Manages image loading, processing, and texture creation.

#### Responsibilities
- Image file loading (PNG, JPG, etc.)
- Texture2D creation
- Image format conversion
- Texture memory management

### TimeHandler

**Location**: `Assets/Runtime/Handlers/TimeHandler/`

Provides time-related functionality and scheduling services.

#### Responsibilities
- Time synchronization
- Scheduled task execution
- Timer management
- Time zone handling

## Manager Components

### InputManager

**Location**: `Assets/Runtime/UserInterface/Input/Scripts/InputManager.cs`
**Inherits**: `BaseManager`

Processes and routes input events from various input sources.

#### Input Sources
- Desktop (Mouse, Keyboard)
- VR Controllers
- Touch Input
- Custom Input Devices

#### Input Event Flow
```plantuml
@startuml Input-Event-Flow
!theme plain

title Input Event Processing Flow

start
:Input Event Occurs;
:Platform Input Captures Event;
:Forward to Input Manager;
if (Input Enabled?) then (yes)
  :Process Event Type;
  :Execute Registered Functions;
  :Trigger JavaScript Events;
  :Update Unity Input System;
else (no)
  :Ignore Input;
endif
:Event Processing Complete;
stop

@enduml
```

#### Input Function Registration
- Left/Right/Middle mouse functions
- Key and KeyCode functions
- VR controller functions
- Touch gesture functions

### OutputManager

**Location**: `Assets/Runtime/UserInterface/Output/Scripts/OutputManager.cs`
**Inherits**: `BaseManager`

Manages visual and audio output to users.

#### Responsibilities
- Screen resolution management
- Display configuration
- Output device management
- Performance monitoring

#### Screen Management
```plantuml
@startuml Screen-Management
!theme plain

title Screen Management Flow

start
:Monitor Screen Changes;
:Check Screen Dimensions;
if (Screen Changed?) then (yes)
  :Update Screen Properties;
  :Notify Registered Callbacks;
  :Adjust UI Elements;
  :Update Rendering Settings;
else (no)
  :Continue Monitoring;
endif
stop

@enduml
```

### LocalStorageManager

**Location**: `Assets/Runtime/LocalStorage/LocalStorageManager/Scripts/LocalStorageManager.cs`

Provides local data persistence with multiple storage strategies.

#### Storage Controllers
- **PersistentStorageController**: Long-term data persistence
- **CacheStorageController**: Temporary data caching
- **BaseStorageController**: Common storage interface

#### Storage Operations
```plantuml
@startuml Storage-Operations
!theme plain

title Local Storage Operations

start
:Storage Request;
:Determine Storage Mode;
if (Persistent Mode?) then (yes)
  :Use Persistent Controller;
else (cache)
  :Use Cache Controller;
endif
:Validate Key/Value;
:Perform Operation (Set/Get/Delete);
:Update Storage;
:Return Result;
stop

@enduml
```

## Synchronization Components

### VOSSynchronizer

**Location**: `Assets/Runtime/VOSSynchronizer/Scripts/VOSSynchronizer.cs`

Handles synchronization with Virtual Operating System (VOS) services.

#### Responsibilities
- State synchronization
- Message processing
- Conflict resolution
- Network communication

#### Synchronization Flow
```plantuml
@startuml VOS-Synchronization
!theme plain

title VOS Synchronization Flow

participant "Local Client" as Local
participant "VOS Synchronizer" as Sync
participant "VOS Server" as Server
participant "Remote Clients" as Remote

Local -> Sync : State Change
Sync -> Server : Sync Message
Server -> Remote : Broadcast Update
Remote -> Server : Acknowledge
Server -> Sync : Confirmation
Sync -> Local : Sync Complete

@enduml
```

### VOSSynchronizationManager

**Location**: `Assets/Runtime/VOSSynchronizer/Scripts/VOSSynchronizationManager.cs`

Coordinates multiple synchronizers and manages synchronization policies.

#### Management Functions
- Synchronizer registration
- Policy enforcement
- Conflict resolution
- Performance monitoring

## Web Interface Components

### WebVerseWebView

Provides web browser functionality within the Unity environment.

#### Capabilities
- HTML rendering
- JavaScript execution
- DOM manipulation
- Web API access

### HTTPRequestManager

Manages HTTP communication with external services.

#### Features
- Asynchronous requests
- Request queuing
- Error handling
- Response caching

#### HTTP Request Flow
```plantuml
@startuml HTTP-Request-Flow
!theme plain

title HTTP Request Processing

start
:HTTP Request;
:Validate URL and Parameters;
:Add to Request Queue;
:Process Asynchronously;
:Send HTTP Request;
if (Response OK?) then (yes)
  :Process Response;
  :Cache if Applicable;
  :Return Success;
else (error)
  :Log Error;
  :Return Error Response;
endif
:Request Complete;
stop

@enduml
```

## Platform-Specific Components

### DesktopInput

**Location**: `Assets/Runtime/UserInterface/Input/Desktop/Scripts/DesktopInput.cs`
**Inherits**: `BasePlatformInput`

Handles desktop-specific input (mouse, keyboard).

#### Input Mapping
- Mouse clicks (Left, Right, Middle)
- Mouse movement and scroll
- Keyboard input
- Keyboard shortcuts

#### Desktop Input Processing
```plantuml
@startuml Desktop-Input-Processing
!theme plain

title Desktop Input Processing

start
:Desktop Input Event;
if (Mouse Event?) then (yes)
  :Process Mouse Input;
  :Update Cursor State;
  :Generate Raycast;
elseif (Keyboard Event?) then (yes)
  :Process Key Input;
  :Handle Key Combinations;
  :Update Key States;
endif
:Forward to Input Manager;
stop

@enduml
```

### VR Input Components

Handle VR-specific input from controllers and headsets.

#### VR Input Features
- 6DOF tracking
- Controller button mapping
- Gesture recognition
- Haptic feedback

## Utility Components

### BaseManager

**Location**: `Assets/Runtime/Utilities/Scripts/BaseManager.cs`

Base class for all manager components providing common functionality.

### BaseController

**Location**: `Assets/Runtime/Utilities/Scripts/BaseController.cs`

Base class for controller components.

### Logging

**Location**: `Assets/Runtime/Utilities/Scripts/Logging.cs`

Centralized logging system for the entire runtime.

#### Logging Features
- Multiple log levels (Info, Warning, Error)
- Category-based filtering
- Console integration
- File output support

## Component Integration Patterns

### Dependency Injection

Components can declare dependencies on other components:

```csharp
[RequireComponent(typeof(FileHandler))]
public class VEMLHandler : BaseHandler
{
    private FileHandler fileHandler;
    
    public override void Initialize()
    {
        fileHandler = GetComponent<FileHandler>();
        // ... initialization logic
    }
}
```

### Event-Based Communication

Components communicate through events to maintain loose coupling:

```csharp
public class InputManager : BaseManager
{
    public event Action<InputEvent> OnInputEvent;
    
    private void ProcessInput()
    {
        OnInputEvent?.Invoke(inputEvent);
    }
}
```

### Service Locator Pattern

Global services are accessible through static references:

```csharp
public static class WebVerseRuntime
{
    public static Instance { get; private set; }
    public InputManager inputManager;
    public OutputManager outputManager;
    // ... other managers
}
```

## Performance Considerations

### Component Lifecycle

- **Lazy Initialization**: Components initialize only when needed
- **Proper Disposal**: Components clean up resources in Terminate()
- **Memory Management**: Minimize allocations in Update() methods

### Threading Model

- **Main Thread**: Unity operations and GameObject manipulation
- **Background Threads**: File I/O, network operations, and heavy computations
- **Thread Safety**: Proper synchronization for shared resources

### Scalability Features

- **Object Pooling**: Reuse of GameObjects and components
- **LOD System**: Level-of-detail for complex scenes
- **Culling**: Frustum and occlusion culling for performance

This component overview provides the foundation for understanding how WebVerse-Runtime components work together to create immersive virtual environments.