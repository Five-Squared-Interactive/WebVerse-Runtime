# WebVerse-Runtime Architecture

## Overview

WebVerse-Runtime is a Unity3D-based runtime system for creating immersive web-based virtual environments. It provides a comprehensive framework for loading, processing, and rendering various content types while managing user interactions and enabling real-time synchronization.

**Core Technology**: Unity 2021.3.26 with Universal Render Pipeline (URP)  
**Primary Language**: C# (Unity scripts), JavaScript (for dynamic behavior)  
**Target Platforms**: Windows, WebGL

## Quick Architecture Summary

```
External Systems → WebVerse-Runtime → StraightFour World Engine → Unity Rendering
     ↓                    ↓                      ↓                        ↓
Content Servers → Handlers (VEML/GLTF/JS) → Entities (Mesh/Character/etc.) → GameObjects
User Input → Input Manager → Event System → StraightFour APIs → Entity Updates
VOS Server → Synchronizers → World State → StraightFour Sync → Scene Updates
```

## Key Architectural Patterns

### 1. Modular Handler Architecture

The system uses a **handler-based architecture** where specialized handlers manage different content types and functionality:

- **BaseHandler**: Abstract base class providing lifecycle management (`Initialize()`, `Terminate()`)
- **Content Handlers**: VEML, GLTF, File, Image handlers process specific content types
- **Execution Handlers**: JavaScript and Time handlers manage runtime behavior
- **Pluggable Design**: Handlers can be added, removed, or replaced independently

**Benefits**: Separation of concerns, easy testing, extensible design

### 2. Event-Driven Communication

Components communicate through events to maintain loose coupling:

- **Input Events**: User interactions trigger events processed by Input Manager
- **JavaScript Events**: Script execution triggers events for appropriate systems
- **Synchronization Events**: Network events update local state via VOS synchronizers

### 3. Unity Integration

Deep integration with Unity3D provides the rendering and physics foundation:

- **GameObject Architecture**: Each major component is a Unity GameObject
- **Unity Lifecycle**: Components follow Unity's initialization and update patterns
- **Scene Management**: Integration with Unity's scene system for content organization

## System Components

### Core Runtime

**WebVerseRuntime** (`Assets/Runtime/Runtime/Scripts/WebVerseRuntime.cs`)
- Central orchestrator for the entire system
- Manages component initialization and lifecycle
- Handles runtime configuration and settings
- Provides centralized error handling and logging

### Handler System

All handlers inherit from **BaseHandler** (`Assets/Runtime/Utilities/Scripts/BaseHandler.cs`)

#### Content Handlers

1. **FileHandler** (`Assets/Runtime/Handlers/FileHandler/`)
   - Local file storage management
   - Directory structure creation
   - File caching and validation
   - Image and binary file operations

2. **VEMLHandler** (`Assets/Runtime/Handlers/VEMLHandler/`)
   - Processes Virtual Environment Markup Language documents
   - Schema version conversion (V2.3, V2.4, V3.0 support)
   - Entity creation and hierarchy setup
   - Asset reference resolution

3. **GLTFHandler** (`Assets/Runtime/Handlers/GLTFHandler/`)
   - Loads GLTF/GLB 3D models
   - Material and texture application
   - Animation setup
   - Scene hierarchy integration

4. **ImageHandler** (`Assets/Runtime/Handlers/ImageHandler/`)
   - Image file loading (PNG, JPG, etc.)
   - Texture2D creation and management
   - Image format conversion
   - Texture memory optimization

5. **JSONEntityHandler** (`Assets/Runtime/Handlers/JSONEntityHandler/`)
   - JSON-based entity definitions
   - Alternative to VEML for simpler entity structures

6. **X3DHandler** (`Assets/Runtime/Handlers/X3DHandler/`)
   - X3D format support for legacy content
   - StraightFour adapter for X3D to World Engine integration

7. **OMIHandler** (`Assets/Runtime/Handlers/OMIHandler/`)
   - Open Metaverse Interoperability (OMI) extensions
   - Physics body, joints, shapes, and gravity handling
   - Vehicle wheel and thruster systems
   - Audio emitter support
   - Spawn point and seat management
   - StraightFour adapters for OMI document and node handlers

#### Execution Handlers

1. **JavaScriptHandler** (`Assets/Runtime/Handlers/JavascriptHandler/`)
   - JavaScript code execution via JINT engine
   - API exposure to JavaScript (Entity, Camera, Input, etc.)
   - Event handling and callbacks
   - Bridge between scripts and Unity systems

2. **TimeHandler** (`Assets/Runtime/Handlers/TimeHandler/`)
   - Time synchronization
   - Scheduled task execution
   - Timer management

### Manager System

1. **InputManager** (`Assets/Runtime/UserInterface/Input/Scripts/InputManager.cs`)
   - Processes input from Desktop (mouse/keyboard) and VR controllers
   - Routes input events to appropriate handlers
   - Triggers JavaScript event callbacks

2. **OutputManager** (`Assets/Runtime/UserInterface/Output/Scripts/OutputManager.cs`)
   - Screen resolution management
   - Display configuration
   - Performance monitoring

3. **LocalStorageManager** (`Assets/Runtime/LocalStorage/LocalStorageManager/`)
   - Persistent data storage (PersistentStorageController)
   - Temporary caching (CacheStorageController)
   - Key-value storage interface

4. **HTTPRequestManager**
   - Asynchronous HTTP requests
   - Request queuing and caching
   - Error handling and retry logic

### Synchronization System

1. **VOSSynchronizer** (`Assets/Runtime/VOSSynchronizer/`)
   - State synchronization with VOS (Virtual Operating System) servers
   - Message processing and conflict resolution
   - Multi-user state consistency

2. **VOSSynchronizationManager**
   - Coordinates multiple synchronizers
   - Policy enforcement
   - Performance monitoring

### Web Interface Components

1. **WebVerseWebView**
   - HTML rendering within Unity
   - JavaScript execution
   - Web API access

2. **WebSocket Manager**
   - Real-time bidirectional communication
   - Connection management
   - Message handling

### StraightFour World Engine

**StraightFour** (`Assets/Runtime/StraightFour/`) is the core World Engine that manages all world entities, environments, and gameplay systems. It provides the foundation for creating and managing virtual worlds within WebVerse-Runtime.

**Main Class**: `StraightFour.cs` - Central World Engine orchestrator  
**World Class**: `World.cs` - Represents individual world instances

#### StraightFour Subsystems

1. **Entity System** (`Assets/Runtime/StraightFour/Entity/`)
   - **EntityManager**: Core entity creation, management, and lifecycle
   - **Base Entities**: Foundation classes for all entity types
   - **Mesh Entities**: 3D model-based entities with rendering and physics
   - **Character Entities**: Avatar and NPC character controllers
   - **Automobile Entities**: Vehicle physics and control systems
   - **Airplane Entities**: Aircraft physics and flight controls
   - **Light Entities**: Dynamic lighting (point, directional, spot lights)
   - **Audio Entities**: Spatial audio sources and emitters
   - **UI Entities**: Canvas, buttons, text, images, input fields, dropdowns, HTML elements
   - **Container Entities**: Entity grouping and hierarchy management
   - **Voxel Entities**: Voxel-based terrain and structures
   - **Water Entities**: Water simulation and rendering
   - **Terrain Entities**: Heightmap-based terrain systems

2. **Camera System** (`Assets/Runtime/StraightFour/Camera/`)
   - Camera management and control
   - View mode switching (first-person, third-person, free-cam)
   - Camera following and targeting
   - Viewport and rendering configuration

3. **Environment System** (`Assets/Runtime/StraightFour/Environment/`)
   - Sky rendering and skybox management
   - Environmental lighting configuration
   - Atmosphere and weather effects
   - Day/night cycle support

4. **World Storage** (`Assets/Runtime/StraightFour/World Storage/`)
   - World state persistence
   - Entity data serialization/deserialization
   - Save/load functionality
   - World metadata management

5. **Synchronization** (`Assets/Runtime/StraightFour/Synchronization/`)
   - Real-time world state synchronization
   - Entity state replication
   - Network message handling
   - Conflict resolution for multi-user scenarios

6. **Utilities** (`Assets/Runtime/StraightFour/Utilities/`)
   - **Logging**: World Engine logging system
   - **Managers**: Resource and lifecycle managers
   - **Materials**: Material management and caching
   - **Tags**: Entity tagging and categorization

#### StraightFour Integration with Handlers

StraightFour works closely with the handler system:

- **VEML Handler → StraightFour**: VEML documents define entities that StraightFour instantiates
- **GLTF Handler → StraightFour**: Loaded 3D models become StraightFour mesh entities
- **JavaScript Handler ↔ StraightFour**: JavaScript APIs directly manipulate StraightFour entities
- **Input Manager → StraightFour**: User input is processed and routed to StraightFour entities
- **VOS Synchronizer ↔ StraightFour**: Synchronizes StraightFour world state across clients

#### World Lifecycle

```
1. WebVerseRuntime creates StraightFour GameObject
2. StraightFour.LoadWorld(title, queryParams) called
3. World instance created with WorldInfo configuration
4. Handlers load content and create entities via StraightFour APIs
5. StraightFour.ActiveWorld provides access to current world
6. Entities updated via StraightFour entity management
7. StraightFour.UnloadWorld() for cleanup
```

#### Key StraightFour Capabilities

- **Entity Lifecycle**: Create, modify, destroy entities with full component support
- **Physics Integration**: Built-in physics for vehicles, characters, and objects
- **Spatial Audio**: 3D positional audio with attenuation
- **UI Systems**: Both world-space and screen-space UI elements
- **Material Management**: Shared materials with highlight and preview support
- **Character Controllers**: Ready-to-use first and third-person controllers
- **Vehicle Physics**: Automobile and aircraft physics via NWH Vehicle Physics integration

## Data Flow

### Content Loading Flow

```
1. External Source (HTTP/Local) → HTTP Request Manager
2. HTTP Request Manager → File Handler (caching)
3. VEML Handler → Parse VEML document
4. VEML Handler → Request assets (3D models, textures) from File Handler
5. GLTF Handler / Image Handler → Process specific assets
6. Handlers → Create Unity GameObjects
7. World Engine → Render scene
```

### Input Processing Flow

```
1. Hardware Input → Platform Input (Desktop/VR)
2. Platform Input → Input Manager
3. Input Manager → JavaScript Handler (trigger event functions)
4. Input Manager → Unity Input System (direct events)
5. JavaScript Handler → World APIs → World Engine (API calls)
```

### JavaScript API Integration Flow

```
1. JavaScript Code → API Call (e.g., Entity.create())
2. JavaScript Handler → API Helper
3. API Helper → Unity World Engine (create GameObject)
4. World Engine → Return entity reference
5. API Helper → JavaScript Handler → Return to JavaScript
```

## Component Initialization Sequence

```
1. WebVerseRuntime.Initialize()
2. Create StraightFour GameObject (World Engine)
3. Initialize StraightFour component
4. Create Handlers GameObject
5. Initialize LocalStorageManager
6. Initialize FileHandler
7. Initialize ContentHandlers (VEML, GLTF, Image, JSON, X3D, OMI)
8. Initialize JavaScriptHandler
9. Setup JavaScript API Helpers
10. Initialize InputManager
11. Initialize OutputManager
12. Initialize HTTPRequestManager
13. Initialize WebVerseWebView
14. Runtime Ready
```

## Directory Structure

```
WebVerse-Runtime/
├── Assets/
│   ├── Runtime/
│   │   ├── Runtime/Scripts/          # Core runtime (WebVerseRuntime.cs)
│   │   ├── StraightFour/             # World Engine (core entity & world system)
│   │   │   ├── StraightFour.cs       # Main World Engine class
│   │   │   ├── World.cs              # World instance management
│   │   │   ├── Entity/               # All entity types
│   │   │   │   ├── Manager/          # Entity management system
│   │   │   │   ├── Base/             # Base entity classes
│   │   │   │   ├── Mesh/             # 3D mesh entities
│   │   │   │   ├── Character/        # Character controllers
│   │   │   │   ├── Automobile/       # Vehicle physics
│   │   │   │   ├── Airplane/         # Aircraft physics
│   │   │   │   ├── Light/            # Light entities
│   │   │   │   ├── Audio/            # Audio entities
│   │   │   │   ├── UI/               # UI entity system
│   │   │   │   ├── Container/        # Entity containers
│   │   │   │   ├── Voxel/            # Voxel entities
│   │   │   │   ├── Water/            # Water entities
│   │   │   │   └── Terrain/          # Terrain entities
│   │   │   ├── Camera/               # Camera management
│   │   │   ├── Environment/          # Sky, lighting, atmosphere
│   │   │   ├── World Storage/        # World persistence
│   │   │   ├── Synchronization/      # World state sync
│   │   │   ├── Utilities/            # World Engine utilities
│   │   │   └── Testing/              # World Engine tests
│   │   ├── Handlers/                 # All handler implementations
│   │   │   ├── FileHandler/
│   │   │   ├── VEMLHandler/
│   │   │   ├── GLTFHandler/
│   │   │   ├── ImageHandler/
│   │   │   ├── JavascriptHandler/
│   │   │   ├── TimeHandler/
│   │   │   ├── JSONEntityHandler/
│   │   │   ├── X3DHandler/
│   │   │   └── OMIHandler/           # OMI extensions with StraightFour adapters
│   │   ├── LocalStorage/             # Storage management
│   │   │   ├── LocalStorageManager/
│   │   │   └── LocalStorageControllers/
│   │   ├── VOSSynchronizer/          # Multi-user synchronization
│   │   ├── UserInterface/            # Input/Output management
│   │   │   ├── Input/
│   │   │   └── Output/
│   │   ├── Utilities/Scripts/        # Base classes (BaseHandler, etc.)
│   │   ├── TopLevel/                 # Main scenes and entry points
│   │   └── 3rd-party/                # SQLite, JINT
│   ├── WebGLTemplates/               # WebGL build templates
│   ├── Packages/                     # Unity packages
│   └── ProjectSettings/              # Unity project configuration
├── docs/                             # Detailed documentation
│   ├── architecture/                 # In-depth architecture docs
│   ├── api/                          # API reference
│   ├── configuration/                # Configuration guides
│   └── examples/                     # Usage examples
└── README.md                         # Quick start guide
```

## JavaScript API Categories

The JavaScript Handler exposes comprehensive APIs to scripts:

1. **World Types**: Vector2/3/4, Color, Transform, Quaternion
2. **Entity APIs**: Entity management, component system, hierarchy operations
3. **Data APIs**: JSON processing, data persistence, async operations
4. **Environment APIs**: Scene management, lighting control, physics settings
5. **Utility APIs**: Camera control, input handling, logging, time management
6. **Networking APIs**: HTTP requests, WebSocket communication, VOS synchronization

## Extensibility

### Creating Custom Handlers

To extend functionality:

1. Inherit from `BaseHandler` base class
2. Override `Initialize()` and `Terminate()` methods
3. Implement custom processing logic
4. Register with WebVerseRuntime

```csharp
public class CustomHandler : BaseHandler
{
    public override void Initialize()
    {
        base.Initialize();
        // Custom initialization
    }
    
    public override void Terminate()
    {
        // Custom cleanup
        base.Terminate();
    }
    
    public void ProcessCustomData(CustomData data)
    {
        if (!IsInitialized) return;
        // Custom processing logic
    }
}
```

### Plugin Architecture

- Use Unity's Assembly Definition Files (asmdef) for modular plugins
- Handlers can declare dependencies on other handlers
- Configuration system supports plugin-specific settings

## Performance Considerations

### Asynchronous Operations
- Content loading is performed asynchronously
- File I/O uses Unity's async APIs
- Network requests are non-blocking

### Memory Management
- Intelligent asset caching reduces memory usage
- Minimal allocations in update loops
- Proper resource disposal when no longer needed

### Threading Model
- **Main Thread**: Unity GameObject operations, rendering
- **Background Threads**: File I/O, network operations, heavy computation
- **JavaScript Execution**: Runs on main thread with controlled execution time

## Security Architecture

### JavaScript Sandboxing
- API limitations to safe operations only
- File system access limited to designated directories
- Network access controlled through request managers

### Content Validation
- VEML schema validation
- Asset file type and size validation
- JavaScript code validation before execution

## Integration with WebVerse Ecosystem

WebVerse-Runtime is part of a larger ecosystem:

- **WebVerse**: Top-level NodeJS/Electron application (container)
- **WebVerse-Runtime**: This Unity runtime (orchestration, handlers, and I/O)
- **StraightFour World Engine**: Core world logic and entity management (integrated within Runtime)
- **VEML**: Virtual Environment Markup Language specification
- **World APIs**: JavaScript APIs for world interaction (exposed via JavaScriptHandler)
- **VSS**: VOS Synchronization Service for multi-user experiences

**Note**: StraightFour is the internal name for the World Engine component that is integrated directly into WebVerse-Runtime at `Assets/Runtime/StraightFour/`. It provides the entity system, camera management, environment control, and world storage capabilities.

## Common Use Cases

1. **Static Content Loading**: Load VEML documents with 3D models and environments
2. **Dynamic Content Generation**: Create entities and scenes programmatically via JavaScript
3. **Interactive Experiences**: Handle user input and trigger behaviors
4. **Multi-User Environments**: Synchronize state across multiple clients via VOS
5. **Custom Content Types**: Extend with custom handlers for specific formats

## Key Design Decisions

1. **Unity-Based**: Leverages Unity's mature rendering, physics, and platform support
2. **Handler Pattern**: Enables modular, testable, and extensible architecture
3. **JavaScript Integration**: Allows dynamic behavior without recompilation
4. **Event-Driven**: Promotes loose coupling between components
5. **Async-First**: Non-blocking operations for better performance
6. **Schema Versioning**: Backward compatibility for VEML documents

## Testing Strategy

- **Unit Tests**: Individual handler testing (Unity Test Framework)
- **Integration Tests**: Handler communication and dependency testing
- **Performance Tests**: Memory usage, processing speed, resource cleanup
- **Scene Tests**: End-to-end content loading and rendering validation

Location: Tests are co-located with components (e.g., `Assets/Runtime/LocalStorage/Tests/`)

## Dependencies

### Unity Asset Store Packages (Required)
- Best HTTP v3.0.4: HTTP/HTTPS communication
- Best MQTT v3.0.2: MQTT protocol support
- Best WebSockets v3.0.1: WebSocket communication

### Integrated Libraries
- **JINT**: JavaScript interpreter for C# (`Assets/Runtime/3rd-party/JINT/`)
- **SQLite**: Database for local storage (`Assets/Runtime/3rd-party/SQLite/`)
- **StraightFour**: World Engine (integrated at `Assets/Runtime/StraightFour/`)
- **NWH Vehicle Physics**: Vehicle and aircraft physics (commercial asset)

### External Dependencies
- WebVerse World Engine repository provides additional world logic components

## Configuration

Configuration is primarily done through Unity Inspector on the WebVerseRuntime GameObject:

- **Storage Mode**: Persistent or Cache
- **File Directories**: Base paths for file storage
- **Timeout Settings**: Network and script execution timeouts
- **Handler-Specific Settings**: Each handler has custom configuration options
- **Platform Settings**: Desktop/VR/WebGL specific configurations

## Build Targets

- **Windows**: Native desktop application
- **WebGL**: Browser-based deployment (with memory and API limitations)

## Version Support

- **VEML**: V3.0 (native), V2.4 and V2.3 (automatic conversion)
- **GLTF**: 2.0 specification
- **Unity**: 2021.3.26 with URP (tested and recommended version)

## Additional Resources

For detailed information, see:

- [System Architecture](docs/architecture/README.md) - Comprehensive architecture with diagrams
- [Handler System](docs/architecture/handlers.md) - Detailed handler documentation
- [Component Overview](docs/architecture/components.md) - Individual component details
- [Data Flow](docs/architecture/data-flow.md) - Detailed data flow patterns
- [JavaScript API](docs/api/javascript-api.md) - Complete API reference
- [Getting Started](docs/getting-started.md) - Setup and usage guide

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        External Systems                          │
│  (Content Servers, VOS Server, USD/VEML Server)                │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                     WebVerse-Runtime                             │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │            WebVerseRuntime (Core Orchestrator)            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌─────────────────┐  ┌─────────────────┐  ┌───────────────┐  │
│  │  Input Manager  │  │ Output Manager  │  │HTTP Request   │  │
│  │  (Desktop/VR)   │  │  (Display)      │  │Manager        │  │
│  └─────────────────┘  └─────────────────┘  └───────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    Handler System                         │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │  │
│  │  │ FileHandler  │  │ VEMLHandler  │  │ GLTFHandler  │  │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │  │
│  │  │ImageHandler  │  │JSHandler     │  │ TimeHandler  │  │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              Local Storage Manager                        │  │
│  │    (Persistent Storage + Cache Storage)                  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │           VOS Synchronizer System                         │  │
│  │     (Multi-user state synchronization)                   │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              StraightFour World Engine                           │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                  Entity System                            │  │
│  │  (Mesh, Character, Vehicle, Light, Audio, UI, etc.)     │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌─────────────────┐  ┌─────────────────┐  ┌───────────────┐  │
│  │ Camera System   │  │  Environment    │  │ World Storage │  │
│  └─────────────────┘  └─────────────────┘  └───────────────┘  │
│                                                                   │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Unity Rendering Engine                        │
│         (GameObjects, Components, Rendering, Physics)           │
└─────────────────────────────────────────────────────────────────┘
```

---

**Note**: This document provides an AI-friendly overview of the WebVerse-Runtime architecture. For detailed technical specifications, UML diagrams, and implementation details, refer to the comprehensive documentation in the `docs/architecture/` directory.
