# System Architecture

This document provides a comprehensive overview of the WebVerse-Runtime architecture, including system components, design principles, and interaction patterns.

## High-Level Architecture

```plantuml
@startuml WebVerse-Runtime-Architecture
!theme plain

title WebVerse-Runtime High-Level Architecture

package "External Systems" {
  [End User] as User
  [USD/VEML Server] as USDServer
  [Content Server] as ContentServer
  [VOS Synchronization Server] as VOSServer
}

package "WebVerse-Runtime" {
  package "User Interface" {
    [Input Manager] as InputMgr
    [Output Manager] as OutputMgr
    [Desktop Input] as DesktopInput
    [VR Input] as VRInput
  }
  
  package "Core Runtime" {
    [WebVerse Runtime] as CoreRuntime
    [Local Storage] as LocalStorage
  }
  
  package "Handlers" {
    [VEML Handler] as VEMLHandler
    [GLTF Handler] as GLTFHandler
    [File Handler] as FileHandler
    [JavaScript Handler] as JSHandler
    [Image Handler] as ImageHandler
    [Time Handler] as TimeHandler
  }
  
  package "World Engine" {
    [Unity World Engine] as WorldEngine
  }
  
  package "Synchronization" {
    [VOS Synchronizers] as VOSSync
  }
  
  package "Web Interface" {
    [WebVerse WebView] as WebView
    [HTTP Request Manager] as HTTPMgr
    [WebSocket Manager] as WSMgr
  }
}

' External connections
User --> InputMgr : Input Events
OutputMgr --> User : Visual/Audio Output
WebView <--> ContentServer : HTTP/WebSocket
HTTPMgr <--> USDServer : Content Requests
VOSSync <--> VOSServer : Synchronization

' Internal connections
CoreRuntime --> InputMgr : Initialize/Control
CoreRuntime --> OutputMgr : Initialize/Control
CoreRuntime --> VEMLHandler : Initialize/Control
CoreRuntime --> GLTFHandler : Initialize/Control
CoreRuntime --> FileHandler : Initialize/Control
CoreRuntime --> JSHandler : Initialize/Control
CoreRuntime --> ImageHandler : Initialize/Control
CoreRuntime --> TimeHandler : Initialize/Control

InputMgr --> DesktopInput : Route Input
InputMgr --> VRInput : Route Input
InputMgr --> JSHandler : Trigger Events

VEMLHandler --> WorldEngine : Create Entities
GLTFHandler --> WorldEngine : Load Models
JSHandler --> WorldEngine : API Calls
JSHandler <-> VEMLHandler : Script Integration
JSHandler <-> FileHandler : File Operations

FileHandler --> LocalStorage : Store Files
VOSSync --> LocalStorage : Cache Data
VOSSync --> WorldEngine : Synchronize State

WebView --> HTTPMgr : Web Requests
WebView --> WSMgr : WebSocket Communication

@enduml
```

## Design Principles

### 1. Modular Handler Architecture

The system employs a modular architecture where specialized handlers manage different types of content and functionality:

- **Separation of Concerns**: Each handler focuses on a specific responsibility
- **Pluggable Design**: Handlers can be added, removed, or replaced without affecting others
- **Standardized Interface**: All handlers inherit from `BaseHandler` providing consistent lifecycle management

### 2. Event-Driven Communication

Components communicate through an event-driven system that promotes loose coupling:

- **Input Events**: User interactions trigger events processed by the Input Manager
- **JavaScript Events**: Script execution triggers events handled by appropriate systems
- **Synchronization Events**: Network events update local state through VOS synchronizers

### 3. Unity Integration

Deep integration with Unity3D provides:

- **GameObjects Architecture**: Each major component is represented as Unity GameObjects
- **Unity Lifecycle**: Components follow Unity's initialization and update patterns
- **Scene Management**: Integration with Unity's scene system for content organization

## Core Components

### WebVerse Runtime (Core)

The central orchestrator responsible for:

- **Component Initialization**: Setting up all handlers and managers in proper order
- **Lifecycle Management**: Coordinating startup, runtime, and shutdown phases
- **Configuration Management**: Handling runtime configuration and settings
- **Error Handling**: Centralized error handling and logging

**Key Methods:**
- `InitializeComponents()`: Sets up all system components
- `StartRuntime()`: Begins runtime execution
- `Terminate()`: Cleanly shuts down all systems

### Handler System

Specialized handlers manage different content types and functionality:

#### Content Handlers
- **VEML Handler**: Processes Virtual Environment Markup Language files
- **GLTF Handler**: Loads and manages 3D models and scenes
- **Image Handler**: Processes and manages image assets
- **File Handler**: Manages file system operations and caching

#### Execution Handlers
- **JavaScript Handler**: Executes JavaScript code and exposes APIs
- **Time Handler**: Manages timing and scheduling operations

### Manager System

Managers handle cross-cutting concerns:

#### Input/Output Managers
- **Input Manager**: Processes and routes input events from various sources
- **Output Manager**: Manages visual and audio output to users

#### Storage and Networking
- **Local Storage Manager**: Handles local data persistence
- **HTTP Request Manager**: Manages HTTP communications
- **WebSocket Manager**: Handles real-time communication

### Synchronization System

Enables multi-user and distributed scenarios:

- **VOS Synchronizers**: Maintain state consistency across clients
- **Synchronization Manager**: Coordinates synchronization operations
- **Message Handling**: Processes synchronization messages and updates

## Component Interaction Patterns

### Initialization Flow

```plantuml
@startuml Initialization-Flow
!theme plain

title WebVerse-Runtime Initialization Flow

participant "Application" as App
participant "WebVerse Runtime" as Runtime
participant "Input Manager" as InputMgr
participant "Output Manager" as OutputMgr
participant "Handlers" as Handlers
participant "Local Storage" as Storage
participant "World Engine" as WorldEngine

App -> Runtime : Initialize()
activate Runtime

Runtime -> Storage : Initialize()
activate Storage
Storage --> Runtime : Ready
deactivate Storage

Runtime -> InputMgr : Initialize()
activate InputMgr
InputMgr --> Runtime : Ready
deactivate InputMgr

Runtime -> OutputMgr : Initialize()
activate OutputMgr
OutputMgr --> Runtime : Ready
deactivate OutputMgr

Runtime -> Handlers : Initialize() [for each handler]
activate Handlers
Handlers -> WorldEngine : Setup Integration
activate WorldEngine
WorldEngine --> Handlers : Ready
deactivate WorldEngine
Handlers --> Runtime : Ready
deactivate Handlers

Runtime -> Runtime : StartRuntime()
Runtime --> App : Initialized

deactivate Runtime
@enduml
```

### Content Loading Flow

```plantuml
@startuml Content-Loading-Flow
!theme plain

title Content Loading Flow

participant "External Source" as External
participant "WebVerse Runtime" as Runtime
participant "VEML Handler" as VEML
participant "GLTF Handler" as GLTF
participant "File Handler" as Files
participant "World Engine" as WorldEngine

External -> Runtime : Load Content Request
Runtime -> VEML : Process VEML Document
activate VEML

VEML -> Files : Load Referenced Assets
activate Files
Files -> Files : Cache Locally
Files --> VEML : Asset Data
deactivate Files

VEML -> GLTF : Load 3D Models
activate GLTF
GLTF -> Files : Get Model Files
Files --> GLTF : Model Data
GLTF -> WorldEngine : Create GameObjects
WorldEngine --> GLTF : Objects Created
GLTF --> VEML : Models Ready
deactivate GLTF

VEML -> WorldEngine : Setup Scene Hierarchy
WorldEngine --> VEML : Scene Ready
VEML --> Runtime : Content Loaded

deactivate VEML
@enduml
```

### JavaScript API Integration

```plantuml
@startuml JavaScript-API-Integration
!theme plain

title JavaScript API Integration

participant "JavaScript Code" as JS
participant "JavaScript Handler" as JSHandler
participant "API Helper" as APIHelper
participant "World Engine" as WorldEngine
participant "Handlers" as Handlers

JS -> JSHandler : API Call (e.g., Entity.create())
activate JSHandler

JSHandler -> APIHelper : Route API Call
activate APIHelper

APIHelper -> WorldEngine : Create Unity GameObject
activate WorldEngine
WorldEngine -> WorldEngine : Instantiate Entity
WorldEngine --> APIHelper : Entity Created
deactivate WorldEngine

APIHelper -> Handlers : Notify Entity Creation
activate Handlers
Handlers -> Handlers : Update Internal State
Handlers --> APIHelper : State Updated
deactivate Handlers

APIHelper --> JSHandler : API Result
deactivate APIHelper

JSHandler --> JS : Return Value

deactivate JSHandler
@enduml
```

## Data Flow Architecture

### Input Processing

```plantuml
@startuml Input-Data-Flow
!theme plain

title Input Data Flow

[User Input Device] -> [Platform Input (Desktop/VR)]
[Platform Input (Desktop/VR)] -> [Input Manager]
[Input Manager] -> [JavaScript Handler] : Trigger Event Functions
[Input Manager] -> [World Engine] : Direct Unity Events
[JavaScript Handler] -> [World APIs] : Execute Event Scripts
[World APIs] -> [World Engine] : API Calls
@enduml
```

### Content Data Flow

```plantuml
@startuml Content-Data-Flow
!theme plain

title Content Data Flow

[External Server] -> [HTTP Request Manager] : Content Request
[HTTP Request Manager] -> [File Handler] : Cache Content
[File Handler] -> [Local Storage] : Persist Files
[VEML Handler] -> [File Handler] : Load VEML
[VEML Handler] -> [GLTF Handler] : Load Models
[VEML Handler] -> [Image Handler] : Load Textures
[GLTF Handler] -> [World Engine] : Create 3D Objects
[Image Handler] -> [World Engine] : Apply Textures
[VEML Handler] -> [World Engine] : Setup Scene Hierarchy
@enduml
```

## Performance Considerations

### Asynchronous Operations

- **Content Loading**: All content loading operations are performed asynchronously
- **File I/O**: File operations use Unity's async APIs
- **Network Requests**: HTTP and WebSocket operations are non-blocking

### Memory Management

- **Asset Caching**: Intelligent caching reduces memory usage
- **Garbage Collection**: Minimal allocations in update loops
- **Resource Cleanup**: Proper disposal of resources when no longer needed

### Threading Model

- **Main Thread**: Unity's main thread handles GameObject operations
- **Background Threads**: File I/O and network operations use background threads
- **JavaScript Execution**: JavaScript runs on the main thread with controlled execution time

## Security Architecture

### JavaScript Sandboxing

- **API Limitations**: JavaScript APIs are restricted to safe operations
- **File System Access**: Limited to designated directories
- **Network Access**: Controlled through request managers

### Content Validation

- **VEML Validation**: Schema validation for VEML documents
- **Asset Validation**: File type and size validation
- **Script Validation**: JavaScript code validation before execution

## Extensibility Points

### Custom Handlers

The architecture supports custom handlers through:

- **BaseHandler Interface**: Inherit from BaseHandler for lifecycle management
- **Registration System**: Register custom handlers with the runtime
- **API Integration**: Expose custom functionality through JavaScript APIs

### Plugin Architecture

- **Assembly Definition Files**: Use Unity's asmdef system for modular plugins
- **Dependency Injection**: Handlers can depend on other handlers
- **Configuration System**: Plugin-specific configuration support

This architecture provides a robust, scalable foundation for virtual world experiences while maintaining flexibility for customization and extension.