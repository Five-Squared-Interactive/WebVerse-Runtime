# X3D Handler for WebVerse-Runtime

This module provides X3D (Extensible 3D) file format support for WebVerse-Runtime, allowing X3D scenes to be loaded and rendered using the StraightFour world engine.

## Architecture

The X3D integration follows a decoupled architecture using the existing X3DUnity library:

```
┌─────────────────────────────────────────────────────────────────────┐
│                      X3D Library (Assets/Runtime/X3D/)               │
│                                                                      │
│  ┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐ │
│  │   X3DParser     │───▶│ X3DUnityConverter│───▶│IX3DWorldAdapter │ │
│  │   (XML→Nodes)   │    │  (Node→Object)   │    │   (Interface)   │ │
│  └─────────────────┘    └──────────────────┘    └────────┬────────┘ │
│                                 ▲                        │          │
│                                 │                        │          │
│                         ┌───────┴───────┐                │          │
│                         │X3DWorldBuilder│────────────────┘          │
│                         │(Nodes→Adapter)│                           │
│                         └───────────────┘                           │
└─────────────────────────────────────────────────────────────────────┘
                                                           │
                                           ┌───────────────┘
                                           │ Implements
                                           ▼
┌─────────────────────────────────────────────────────────────────────┐
│                          X3DHandler                                  │
│  ┌─────────────────────────┐    ┌────────────────────────────────┐ │
│  │ StraightFourX3DAdapter  │───▶│       StraightFour             │ │
│  │ (implements interface)  │    │    (World Engine)              │ │
│  └─────────────────────────┘    └────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Components

### X3D Library (`Assets/Runtime/X3D/`)

The X3D library is imported from `packages/X3DUnity` and extended with abstraction interfaces:

- **`X3DParser`**: Parses X3D XML documents into an `X3DNode` tree
- **`X3DNode`**: In-memory representation of X3D elements with attributes and children
- **`X3DUnityConverter`**: Converts X3DNode to Unity GameObjects (from X3DUnity library)
- **`X3DWorldBuilder`**: Bridges X3DParser output to IX3DWorldAdapter (new)
- **`IX3DWorldAdapter`**: Interface that decouples X3D from any specific world engine (new)
- **X3D Component Scripts**: `X3DTransform`, `X3DShape`, `X3DMaterialComponent`, etc.

### X3D Handler (`Assets/Runtime/Handlers/X3DHandler/`)

- **`X3DHandler`**: Main handler for loading X3D documents from URLs or strings
- **`StraightFourX3DAdapter`**: Implements `IX3DWorldAdapter` for StraightFour world engine

## Supported X3D Nodes

### Grouping Nodes
- Transform
- Group
- StaticGroup
- Switch
- Anchor
- Billboard
- LOD

### Geometry Nodes
- Box
- Sphere
- Cylinder
- Cone
- IndexedFaceSet
- IndexedLineSet
- TriangleSet
- QuadSet
- PointSet
- LineSet

### Appearance Nodes
- Appearance
- Material
- ImageTexture
- MovieTexture

### Lighting Nodes
- DirectionalLight
- PointLight
- SpotLight

### Environment Nodes
- Background
- Fog
- Viewpoint
- GeoViewpoint
- NavigationInfo

### Animation Nodes
- TimeSensor
- PositionInterpolator
- OrientationInterpolator
- ColorInterpolator
- ScalarInterpolator
- ROUTE

### Other Nodes
- WorldInfo
- Sound/AudioClip
- Inline
- GeoElevationGrid
- GeoCoordinate
- GeoOrigin
- Script
- TouchSensor

## Usage

### Basic Usage

```csharp
// Get the X3D handler from runtime
X3DHandler x3dHandler = webVerseRuntime.x3dHandler;

// Create and set the adapter (connects X3D to StraightFour)
StraightFourX3DAdapter adapter = gameObject.AddComponent<StraightFourX3DAdapter>();
adapter.Initialize();
x3dHandler.SetWorldAdapter(adapter);

// Load an X3D document
x3dHandler.LoadX3DDocumentIntoWorld("path/to/scene.x3d", (success) => {
    if (success) {
        Debug.Log("X3D scene loaded successfully!");
    }
});
```

### Loading from String

```csharp
string x3dContent = @"
<X3D version='3.3'>
  <Scene>
    <Shape>
      <Box size='2 2 2'/>
    </Shape>
  </Scene>
</X3D>";

x3dHandler.LoadX3DFromString(x3dContent, "", (success) => {
    Debug.Log($"Loaded: {success}");
});
```

## Creating Custom Adapters

To use X3D with a different world engine, implement `IX3DWorldAdapter`:

```csharp
public class MyCustomAdapter : IX3DWorldAdapter
{
    public X3DEntityHandle CreateContainerEntity(...)
    {
        // Create container in your engine
    }
    
    public X3DEntityHandle CreateMeshEntity(...)
    {
        // Create mesh in your engine
    }
    
    // ... implement other interface methods
}
```

## Coordinate System

X3D uses a right-handed coordinate system, while Unity uses left-handed. The library automatically converts:

- Positions: Z-axis is flipped
- Rotations: Converted appropriately
- Directions: Z-axis is flipped

## Testing

Test assets are located in `Tests/TestAssets/`:

- `test_scene.x3d` - Comprehensive test scene with various node types

## Future Enhancements

- Animation support (TimeSensor, Interpolators, ROUTE)
- Scripting support (Script node)
- Full geographic coordinate conversion
- Inline model loading integration with GLTF handler
- Texture coordinate transforms
