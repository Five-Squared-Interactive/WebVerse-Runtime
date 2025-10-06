# JSONEntityHandler Usage Guide

The JSONEntityHandler provides an API to process JSON-encoded entities and create them using direct API method calls for optimal performance and compatibility. Supports container entities, mesh entities, and terrain entities.

## Implementation Details

### Entity Creation Methods
The handler directly calls the WebVerse API methods for entity creation:

**Mesh Entities:**
- `MeshEntity.CreateCube()` for cube primitives
- `MeshEntity.CreateSphere()` for sphere primitives  
- `MeshEntity.CreateCylinder()` for cylinder primitives
- And so on for all supported primitive types

**Terrain Entities:**
- `TerrainEntity.CreateHeightmap()` for heightmap terrain
- `TerrainEntity.CreateHybrid()` for hybrid terrain with voxel modifications

This approach ensures maximum compatibility with existing WebVerse systems and leverages the battle-tested entity creation code.

## Quick Start

### 1. Initialize the Handler

```csharp
JSONEntityHandler jsonHandler = GetComponent<JSONEntityHandler>();
jsonHandler.Initialize();
```

### 2. Process JSON Container Entity (Recommended Method)

```csharp
string jsonEntity = @"{
    ""id"": ""550e8400-e29b-41d4-a716-446655440000"",
    ""tag"": ""MyContainer"",
    ""position"": { ""x"": 0, ""y"": 1, ""z"": 0 },
    ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
    ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
    ""isSize"": false,
    ""parentId"": null,
    ""children"": []
}";

jsonHandler.ProcessContainerEntityJSON(
    jsonString: jsonEntity,
    parentEntity: null, // or specify a parent entity
    onSuccess: (entityId, createdEntity) =>
    {
        Debug.Log($"Successfully created container entity: {entityId}");
        // Use the created entity
    },
    onError: (errorMessage) =>
    {
        Debug.LogError($"Failed to create entity: {errorMessage}");
    }
);
```

### 3. Process JSON Mesh Entity

```csharp
string jsonMeshEntity = @"{
    ""id"": ""550e8400-e29b-41d4-a716-446655440001"",
    ""tag"": ""MyCube"",
    ""position"": { ""x"": 2, ""y"": 1, ""z"": 0 },
    ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
    ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
    ""isSize"": false,
    ""meshType"": ""primitive"",
    ""meshSource"": ""cube"",
    ""color"": { ""r"": 1, ""g"": 0, ""b"": 0, ""a"": 1 },
    ""children"": []
}";

jsonHandler.ProcessMeshEntityJSON(
    jsonString: jsonMeshEntity,
    parentEntity: null,
    onSuccess: (entityId, createdEntity) =>
    {
        Debug.Log($"Successfully created mesh entity: {entityId}");
        // Use the created mesh entity
    },
    onError: (errorMessage) =>
    {
        Debug.LogError($"Failed to create mesh entity: {errorMessage}");
    }
);
```

### 3. Load from File

```csharp
jsonHandler.LoadContainerEntityFromFile(
    filePath: "path/to/entity.json",
    parentEntity: null,
    onComplete: (success, entityId, createdEntity) =>
    {
        if (success)
        {
            Debug.Log($"Loaded entity from file: {entityId}");
        }
        else
        {
            Debug.LogError("Failed to load entity from file");
        }
    }
);
```

## JSON Schema

### Basic Container Entity
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "tag": "MyContainer",
    "position": { "x": 0, "y": 1, "z": 0 },
    "rotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
    "scale": { "x": 1, "y": 1, "z": 1 },
    "isSize": false,
    "parentId": null,
    "children": []
}
```

### Basic Mesh Entity (Primitive)
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "tag": "MyCube",
    "position": { "x": 0, "y": 1, "z": 0 },
    "rotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
    "scale": { "x": 1, "y": 1, "z": 1 },
    "isSize": false,
    "meshType": "primitive",
    "meshSource": "cube",
    "color": { "r": 1, "g": 0, "b": 0, "a": 1 },
    "children": []
}
```

### GLTF Mesh Entity
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "tag": "MyGLTFModel",
    "position": { "x": 3, "y": 0, "z": 0 },
    "rotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
    "scale": { "x": 1, "y": 1, "z": 1 },
    "isSize": false,
    "meshType": "gltf",
    "meshSource": "Models/MyModel.gltf",
    "meshResources": [
        "Textures/ModelTexture.png",
        "Materials/ModelMaterial.mat"
    ],
    "children": []
}
```

### Container with Children
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "tag": "ParentContainer",
    "position": { "x": 0, "y": 0, "z": 0 },
    "rotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
    "scale": { "x": 1, "y": 1, "z": 1 },
    "isSize": false,
    "parentId": null,
    "children": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440001",
            "tag": "ChildContainer1",
            "position": { "x": 1, "y": 0, "z": 0 },
            "rotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
            "scale": { "x": 0.5, "y": 0.5, "z": 0.5 },
            "isSize": false,
            "parentId": "550e8400-e29b-41d4-a716-446655440000",
            "children": []
        },
        {
            "id": "550e8400-e29b-41d4-a716-446655440002",
            "tag": "ChildContainer2",
            "position": { "x": -1, "y": 0, "z": 0 },
            "rotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
            "scale": { "x": 0.75, "y": 0.75, "z": 0.75 },
            "isSize": false,
            "parentId": "550e8400-e29b-41d4-a716-446655440000",
            "children": []
        }
    ]
}
```

## Terrain Entity Examples

### Simple Heightmap Terrain

```csharp
string terrainJson = @"{
    ""id"": ""terrain-001"",
    ""tag"": ""Sample Terrain"",
    ""position"": { ""x"": 0, ""y"": 0, ""z"": 0 },
    ""terrainType"": ""heightmap"",
    ""length"": 100.0,
    ""width"": 100.0,
    ""height"": 20.0,
    ""heights"": [
        [0, 1, 2, 1, 0],
        [1, 3, 5, 3, 1],
        [2, 5, 8, 5, 2],
        [1, 3, 5, 3, 1],
        [0, 1, 2, 1, 0]
    ],
    ""layers"": [
        {
            ""diffuseTexture"": ""Textures/grass.jpg"",
            ""normalTexture"": ""Textures/grass_normal.jpg"",
            ""specular"": { ""r"": 0.1, ""g"": 0.1, ""b"": 0.1, ""a"": 1.0 },
            ""metallic"": 0.0,
            ""smoothness"": 0.3,
            ""sizeFactor"": 1
        }
    ],
    ""layerMasks"": [],
    ""stitchTerrains"": false
}";

jsonHandler.ProcessTerrainEntityJSON(terrainJson, null,
    onSuccess: (entityId, entity) => {
        Debug.Log($"Created terrain: {entityId}");
    },
    onError: (error) => {
        Debug.LogError($"Failed to create terrain: {error}");
    });
```

### Hybrid Terrain with Modifications

```json
{
  "id": "hybrid-terrain-001",
  "tag": "Modifiable Terrain",
  "position": { "x": 0, "y": 0, "z": 0 },
  "terrainType": "hybrid",
  "length": 50.0,
  "width": 50.0,
  "height": 15.0,
  "heights": [
    [0, 0, 0],
    [0, 2, 0],
    [0, 0, 0]
  ],
  "layers": [
    {
      "diffuseTexture": "Textures/dirt.jpg",
      "normalTexture": "Textures/dirt_normal.jpg",
      "specular": { "r": 0.2, "g": 0.15, "b": 0.1, "a": 1.0 },
      "metallic": 0.1,
      "smoothness": 0.2,
      "sizeFactor": 2
    }
  ],
  "modifications": [
    {
      "operation": "dig",
      "position": { "x": 10, "y": 0, "z": 10 },
      "brushType": "sphere",
      "layer": 0,
      "size": 5.0
    },
    {
      "operation": "build",
      "position": { "x": -10, "y": 0, "z": -10 },
      "brushType": "roundedCube",
      "layer": 0,
      "size": 3.0
    }
  ],
  "stitchTerrains": true
}
```

## API Methods

### Container Entity Methods

- **`ProcessContainerEntityJSON`** - High-level method with full validation (recommended)
- **`LoadContainerEntityFromJSON`** - Load container entity from JSON string
- **`LoadContainerEntityFromFile`** - Load container entity from JSON file
- **`ParseEntityFromJSON`** - Parse JSON without creating container entity
- **`CreateContainerEntity`** - Create entity from parsed JSONContainerEntity
- **`ConvertEntityToJSON`** - Convert existing entity back to JSON

### Mesh Entity Methods

- **`ProcessMeshEntityJSON`** - High-level method with full validation (recommended)
- **`LoadMeshEntityFromJSON`** - Load mesh entity from JSON string
- **`ParseMeshEntityFromJSON`** - Parse JSON without creating mesh entity
- **`CreateMeshEntity`** - Create entity from parsed JSONMeshEntity

### Terrain Entity Methods

- **`ProcessTerrainEntityJSON`** - High-level method with full validation (recommended)
- **`LoadTerrainEntityFromJSON`** - Load terrain entity from JSON string
- **`ParseTerrainEntityFromJSON`** - Parse JSON without creating terrain entity
- **`CreateTerrainEntity`** - Create entity from parsed JSONTerrainEntity

### Utility Methods

- **`IsReady()`** - Check if handler can process entities
- **`ValidateJSONFormat()`** - Validate JSON string format
- **`GetHandlerStats()`** - Get handler status information

## Field Descriptions

### Container Entity Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | No | GUID string. If not provided, a new one will be generated |
| `tag` | string | No | Entity tag for identification |
| `position` | JSONVector3 | No | World position (defaults to Vector3.zero) |
| `rotation` | JSONQuaternion | No | World rotation (defaults to Quaternion.identity) |
| `scale` | JSONVector3 | No | Entity scale (defaults to Vector3.one) |
| `isSize` | boolean | No | Whether scale represents size instead of scale |
| `parentId` | string | No | ID of parent entity (for reference, not used in creation) |
| `children` | JSONContainerEntity[] | No | Array of child entities |

### Mesh Entity Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | No | GUID string. If not provided, a new one will be generated |
| `tag` | string | No | Entity tag for identification |
| `position` | JSONVector3 | No | World position (defaults to Vector3.zero) |
| `rotation` | JSONQuaternion | No | World rotation (defaults to Quaternion.identity) |
| `scale` | JSONVector3 | No | Entity scale (defaults to Vector3.one) |
| `isSize` | boolean | No | Whether scale represents size instead of scale |
| `parentId` | string | No | ID of parent entity (for reference, not used in creation) |
| `meshType` | string | **Yes** | Type of mesh: "primitive" or "gltf" |
| `meshSource` | string | **Yes** | Source path or primitive type (e.g., "cube", "Models/model.gltf") |
| `meshResources` | string[] | No | Additional resources like textures (mainly for GLTF) |
| `color` | JSONColor | No | Color to apply to primitive meshes (ignored for GLTF) |
| `children` | JSONMeshEntity[] | No | Array of child mesh entities |

### Color Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `r` | float | No | Red component (0-1, defaults to 0) |
| `g` | float | No | Green component (0-1, defaults to 0) |
| `b` | float | No | Blue component (0-1, defaults to 0) |
| `a` | float | No | Alpha component (0-1, defaults to 1) |

### Mesh Type Values

- **`primitive`** - Unity primitive shapes using WebVerse primitive prefabs
  - Valid `meshSource` values: "cube", "sphere", "cylinder", "capsule", "plane", "torus", "cone", "rectangularpyramid", "tetrahedron", "prism", "arch"
  - Supports `color` field for basic material coloring
- **`gltf`** - GLTF 3D models
  - `meshSource`: Path to .gltf or .glb file
  - `meshResources`: Additional texture/material files
  - `color` field is ignored (uses model's materials)

### Terrain Entity Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | No | GUID string. If not provided, a new one will be generated |
| `tag` | string | No | Entity tag for identification |
| `position` | JSONVector3 | No | World position (defaults to Vector3.zero) |
| `rotation` | JSONQuaternion | No | World rotation (defaults to Quaternion.identity) |
| `scale` | JSONVector3 | No | Entity scale (defaults to Vector3.one) |
| `isSize` | boolean | No | Whether scale represents size instead of scale |
| `parentId` | string | No | ID of parent entity (for reference, not used in creation) |
| `terrainType` | string | **Yes** | Type of terrain: "heightmap" or "hybrid" |
| `length` | float | **Yes** | Length of the terrain in terrain units |
| `width` | float | **Yes** | Width of the terrain in terrain units |
| `height` | float | **Yes** | Height of the terrain in terrain units |
| `heights` | float[][] | **Yes** | 2D array of heights for the terrain (jagged array) |
| `layers` | JSONTerrainEntityLayer[] | **Yes** | Array of terrain layers (textures and materials) |
| `layerMasks` | JSONTerrainEntityLayerMask[] | No | Array of layer masks for texture blending |
| `modifications` | JSONTerrainEntityModification[] | No | Array of modifications (for hybrid terrain only) |
| `stitchTerrains` | boolean | No | Whether to stitch this terrain with adjacent terrains |
| `children` | JSONTerrainEntity[] | No | Array of child terrain entities |

### Terrain Layer Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `diffuseTexture` | string | No | Path to diffuse texture |
| `normalTexture` | string | No | Path to normal map texture |
| `maskTexture` | string | No | Path to mask texture |
| `specular` | JSONColor | No | Specular color (defaults to gray) |
| `metallic` | float | No | Metallic factor (0-1, defaults to 0) |
| `smoothness` | float | No | Smoothness factor (0-1, defaults to 0.5) |
| `sizeFactor` | int | No | Texture size factor (defaults to 1) |

### Terrain Modification Fields (Hybrid Terrain Only)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `operation` | string | **Yes** | Type of modification: "build" or "dig" |
| `position` | JSONVector3 | **Yes** | Position of the modification |
| `brushType` | string | **Yes** | Brush type: "sphere" or "roundedCube" |
| `layer` | int | **Yes** | Layer index for the modification |
| `size` | float | **Yes** | Size of the modification in meters |

### Terrain Type Values

- **`heightmap`** - Traditional heightmap terrain
  - Uses height array to define terrain shape
  - Supports multiple texture layers with blending
  - Better performance for large terrains
- **`hybrid`** - Hybrid terrain with voxel capabilities  
  - Combines heightmap base with voxel modifications
  - Supports runtime digging and building operations
  - More flexible but higher memory usage

## Airplane Entities

### Creating Airplane Entities

```csharp
string airplaneJSON = @"{
    ""id"": ""550e8400-e29b-41d4-a716-446655440004"",
    ""tag"": ""PlayerAirplane"",
    ""position"": { ""x"": 0, ""y"": 10, ""z"": 0 },
    ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
    ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
    ""isSize"": false,
    ""meshObject"": ""https://example.com/models/cessna172.gltf"",
    ""meshResources"": [
        ""https://example.com/models/cessna172_texture.png""
    ],
    ""mass"": 750.0,
    ""throttle"": 0.0,
    ""pitch"": 0.0,
    ""roll"": 0.0,
    ""yaw"": 0.0,
    ""checkForUpdateIfCached"": true,
    ""children"": []
}";

jsonHandler.LoadAirplaneEntityFromJSON(airplaneJSON, null, (success, entityId, entity) =>
{
    if (success && entity != null)
    {
        Debug.Log($"Airplane created with ID: {entityId}");
        
        // Control the airplane
        if (entity is FiveSQD.StraightFour.Entity.AirplaneEntity airplane)
        {
            airplane.throttle = 0.7f;  // Set throttle to 70%
            airplane.pitch = 0.1f;     // Nose up
        }
    }
});
```

### Airplane JSON Fields Reference

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | No | Unique identifier (GUID format) |
| `tag` | string | No | User-defined tag for the entity |
| `position` | JSONVector3 | No | Position in world space (defaults to origin) |
| `rotation` | JSONQuaternion | No | Rotation quaternion (defaults to identity) |
| `scale` | JSONVector3 | No | Scale factors (defaults to 1,1,1) |
| `isSize` | bool | No | Whether scale represents size vs scale factor |
| `meshObject` | string | **Yes** | URL/path to GLTF model file |
| `meshResources` | string[] | No | Additional resource files (textures, etc.) |
| `mass` | float | No | Physics mass in kg (defaults to 1000) |
| `throttle` | float | No | Engine throttle 0-1 (defaults to 0) |
| `pitch` | float | No | Pitch control -1 to 1 (defaults to 0) |
| `roll` | float | No | Roll control -1 to 1 (defaults to 0) |
| `yaw` | float | No | Yaw control -1 to 1 (defaults to 0) |
| `checkForUpdateIfCached` | bool | No | Check for model updates (defaults to true) |
| `children` | JSONAirplaneEntity[] | No | Child airplane entities |

## Automobile Entities

### Creating Automobile Entities

```csharp
string automobileJSON = @"{
    ""id"": ""550e8400-e29b-41d4-a716-446655440005"",
    ""tag"": ""PlayerCar"",
    ""position"": { ""x"": 5, ""y"": 0, ""z"": 0 },
    ""rotation"": { ""x"": 0, ""y"": 0, ""z"": 0, ""w"": 1 },
    ""scale"": { ""x"": 1, ""y"": 1, ""z"": 1 },
    ""isSize"": false,
    ""meshObject"": ""https://example.com/models/sports_car.gltf"",
    ""meshResources"": [
        ""https://example.com/models/sports_car_diffuse.png""
    ],
    ""wheels"": [
        { ""wheelSubMesh"": ""FrontLeft"", ""wheelRadius"": 0.35 },
        { ""wheelSubMesh"": ""FrontRight"", ""wheelRadius"": 0.35 },
        { ""wheelSubMesh"": ""RearLeft"", ""wheelRadius"": 0.35 },
        { ""wheelSubMesh"": ""RearRight"", ""wheelRadius"": 0.35 }
    ],
    ""mass"": 1200.0,
    ""automobileType"": ""Default"",
    ""throttle"": 0.0,
    ""steer"": 0.0,
    ""brake"": 0.0,
    ""handBrake"": 0.0,
    ""horn"": false,
    ""gear"": 1,
    ""engineStartStop"": false,
    ""checkForUpdateIfCached"": true,
    ""children"": []
}";

jsonHandler.LoadAutomobileEntityFromJSON(automobileJSON, null, (success, entityId, entity) =>
{
    if (success && entity != null)
    {
        Debug.Log($"Automobile created with ID: {entityId}");
        
        // Control the automobile
        if (entity is FiveSQD.StraightFour.Entity.AutomobileEntity car)
        {
            car.throttle = 0.5f;  // 50% throttle
            car.steer = 0.2f;     // Slight right turn
            car.gear = 1;         // First gear
        }
    }
});
```

### Automobile JSON Fields Reference

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | No | Unique identifier (GUID format) |
| `tag` | string | No | User-defined tag for the entity |
| `position` | JSONVector3 | No | Position in world space (defaults to origin) |
| `rotation` | JSONQuaternion | No | Rotation quaternion (defaults to identity) |
| `scale` | JSONVector3 | No | Scale factors (defaults to 1,1,1) |
| `isSize` | bool | No | Whether scale represents size vs scale factor |
| `meshObject` | string | **Yes** | URL/path to GLTF model file |
| `meshResources` | string[] | No | Additional resource files (textures, etc.) |
| `wheels` | JSONAutomobileEntityWheel[] | **Yes** | Wheel configuration array |
| `mass` | float | No | Physics mass in kg (defaults to 1500) |
| `automobileType` | string | No | Vehicle type: "Default" (defaults to "Default") |
| `throttle` | float | No | Accelerator pedal 0-1 (defaults to 0) |
| `steer` | float | No | Steering wheel -1 to 1 (defaults to 0) |
| `brake` | float | No | Brake pedal 0-1 (defaults to 0) |
| `handBrake` | float | No | Hand brake 0-1 (defaults to 0) |
| `horn` | bool | No | Horn activation (defaults to false) |
| `gear` | int | No | Current gear (defaults to 0) |
| `engineStartStop` | bool | No | Engine running state (defaults to false) |
| `checkForUpdateIfCached` | bool | No | Check for model updates (defaults to true) |
| `children` | JSONAutomobileEntity[] | No | Child automobile entities |

### Automobile Wheel Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `wheelSubMesh` | string | **Yes** | Name of the wheel submesh in the model |
| `wheelRadius` | float | **Yes** | Radius of the wheel in meters |

## Complete Examples

Check the Examples folder for complete working demonstrations:
- `JSONEntityExample.cs` - Basic container entity usage
- `JSONEntityPrimitiveTest.cs` - All primitive mesh types
- `JSONTerrainEntityTest.cs` - Terrain entity with heightmaps and layers
- `JSONAirplaneEntityTest.cs` - Airplane entity with flight controls
- `JSONAutomobileEntityTest.cs` - Automobile entity with driving controls

## Error Handling

The handler provides comprehensive error handling with detailed logging:

- JSON parsing errors
- GUID format validation
- EntityManager availability checks
- File I/O errors
- Entity creation failures
- GLTF model loading failures
- Wheel configuration validation

All methods include appropriate callbacks for success and error cases.