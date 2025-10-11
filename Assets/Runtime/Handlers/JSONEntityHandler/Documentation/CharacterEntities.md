# Character Entity Documentation

## Overview

Character entities in the WebVerse JSONEntityHandler allow you to create interactive characters with 3D models, physics-based movement, and customizable properties. These entities support both default characters (without custom meshes) and fully animated character models loaded from GLTF files.

## JSONCharacterEntity Properties

### Basic Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `type` | string | "character" | Entity type identifier |
| `id` | string | auto-generated | Unique GUID for the character entity |
| `tag` | string | null | Descriptive tag for identification |
| `position` | JSONVector3 | {0,0,0} | World position of the character |
| `rotation` | JSONQuaternion | {0,0,0,1} | World rotation of the character |
| `scale` | JSONVector3 | {1,1,1} | Scale applied to the character |
| `isSize` | boolean | false | Whether scale represents size vs scaling |

### Character-Specific Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `meshObject` | string | "" | Path to character GLTF file |
| `meshResources` | string[] | [] | Additional mesh resources (textures, etc.) |
| `meshOffset` | JSONVector3 | {0,0,0} | Offset applied to the character mesh |
| `meshRotation` | JSONQuaternion | {0,0,0,1} | Rotation applied to the character mesh |
| `avatarLabelOffset` | JSONVector3 | {0,2,0} | Offset for character name label |
| `fixHeight` | boolean | true | Auto-adjust height if below ground |
| `checkForUpdateIfCached` | boolean | true | Check for mesh updates if cached |

## Character Entity Types

### 1. Default Character
Basic character entity without custom mesh - uses system default character model.

```json
{
  "type": "character",
  "id": "550e8400-e29b-41d4-a716-446655440020",
  "tag": "BasicCharacter",
  "position": { "x": 0, "y": 0, "z": 0 },
  "rotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
  "scale": { "x": 1, "y": 1, "z": 1 },
  "meshObject": "",
  "fixHeight": true
}
```

### 2. Custom Mesh Character
Character with custom GLTF model and textures.

```json
{
  "type": "character",
  "id": "550e8400-e29b-41d4-a716-446655440021",
  "tag": "CustomCharacter",
  "position": { "x": 5, "y": 0, "z": 0 },
  "meshObject": "Characters/Soldier/soldier.gltf",
  "meshResources": [
    "Characters/Soldier/soldier.gltf",
    "Characters/Soldier/textures/soldier_diffuse.png"
  ],
  "meshOffset": { "x": 0, "y": -0.5, "z": 0 },
  "avatarLabelOffset": { "x": 0, "y": 2.2, "z": 0 }
}
```

## API Methods

### Creating Character Entities

#### ParseCharacterEntityFromJSON(jsonString)
Parses JSON string into a JSONCharacterEntity object with validation.

```javascript
// JavaScript API usage
let characterJson = JSON.stringify({
  type: "character",
  tag: "Player1",
  position: { x: 0, y: 0, z: 0 },
  meshObject: "Characters/hero.gltf"
});

WebVerse.JSONEntityHandler.LoadCharacterEntityFromJSON(characterJson, null, 
  function(success, entityId, entity) {
    if (success) {
      console.log("Character created:", entityId);
    }
  });
```

#### CreateCharacterEntity(entity, parentEntity, onComplete)
Creates character entity from JSONCharacterEntity data.

#### LoadCharacterEntityFromJSON(jsonString, parentEntity, onComplete)
High-level method to parse JSON and create character entity.

### Character Control Methods

After creation, character entities support movement and interaction:

```javascript
// Move character
characterEntity.Move({ x: 1, y: 0, z: 0 });

// Make character jump
characterEntity.Jump(5.0);

// Check if character is on surface
let onGround = characterEntity.IsOnSurface();

// Update character model
characterEntity.SetCharacterModel("newModel.gltf", 
  { x: 0, y: 0, z: 0 }, // mesh offset
  { x: 0, y: 0, z: 0, w: 1 }, // mesh rotation
  { x: 0, y: 2, z: 0 } // label offset
);
```

## Character Physics

Character entities include built-in physics for realistic movement:

- **Character Controller**: Handles collision detection and movement
- **Gravity**: Characters are affected by gravity
- **Ground Detection**: `IsOnSurface()` method for ground checking
- **Height Fixing**: `fixHeight` property auto-adjusts if below ground
- **Movement**: Smooth physics-based movement with `Move()` method

## Mesh Configuration

### Mesh Resources
Character meshes support various resource types:
- **GLTF Files**: Primary 3D model format
- **Textures**: Diffuse, normal, metallic maps
- **Animations**: Embedded animations in GLTF
- **Materials**: PBR material properties

### Mesh Positioning
Fine-tune character appearance with offset and rotation:

```json
{
  "meshOffset": { "x": 0, "y": -0.5, "z": 0 },
  "meshRotation": { "x": 0, "y": 90, "z": 0, "w": 0.7071 },
  "avatarLabelOffset": { "x": 0, "y": 2.2, "z": 0 }
}
```

## Performance Considerations

### Caching
- Set `checkForUpdateIfCached: false` for static characters
- Use `checkForUpdateIfCached: true` for frequently updated characters

### LOD (Level of Detail)
- Use appropriate mesh complexity for viewing distance
- Consider multiple mesh versions for different detail levels

### Resource Management
- Optimize texture sizes and formats
- Use texture atlases when possible
- Limit polygon count for mobile platforms

## Common Use Cases

### Player Characters
```json
{
  "type": "character",
  "tag": "Player",
  "meshObject": "Characters/Player/avatar.gltf",
  "fixHeight": true,
  "avatarLabelOffset": { "x": 0, "y": 2.0, "z": 0 }
}
```

### NPCs (Non-Player Characters)
```json
{
  "type": "character",
  "tag": "ShopKeeper",
  "position": { "x": 10, "y": 0, "z": 5 },
  "meshObject": "Characters/NPC/shopkeeper.gltf",
  "fixHeight": true,
  "checkForUpdateIfCached": false
}
```

### Animated Characters
```json
{
  "type": "character",
  "tag": "Guard",
  "meshObject": "Characters/Guard/guard_animated.gltf",
  "meshResources": [
    "Characters/Guard/guard_animated.gltf",
    "Characters/Guard/animations/idle.gltf",
    "Characters/Guard/animations/walk.gltf"
  ]
}
```

## Error Handling

The JSONEntityHandler provides comprehensive error handling:

- **Validation Errors**: Invalid GUID, missing required fields
- **Resource Errors**: Missing GLTF files, texture loading failures  
- **Creation Errors**: EntityManager unavailable, parent entity issues
- **Runtime Errors**: JSON parsing failures, unexpected exceptions

All errors are logged with detailed messages for debugging.

## Best Practices

1. **Always validate JSON** before processing
2. **Use meaningful tags** for entity identification
3. **Optimize mesh resources** for target platform
4. **Test character movement** in your specific environment
5. **Handle loading callbacks** properly for async operations
6. **Use appropriate label offsets** for different character sizes
7. **Consider performance impact** of multiple animated characters

## Integration Examples

### Loading from File
```javascript
WebVerse.JSONEntityHandler.LoadContainerEntityFromFile(
  "Characters/hero_character.json", 
  null, 
  function(success, entityId, entity) {
    if (success) {
      console.log("Character loaded from file");
    }
  }
);
```

### Batch Character Creation
```javascript
let characters = [
  { /* character 1 data */ },
  { /* character 2 data */ },
  { /* character 3 data */ }
];

characters.forEach((charData, index) => {
  WebVerse.JSONEntityHandler.LoadCharacterEntityFromJSON(
    JSON.stringify(charData),
    null,
    function(success, entityId, entity) {
      console.log(`Character ${index + 1} created:`, success);
    }
  );
});
```

This documentation provides a complete guide to using character entities within the WebVerse JSONEntityHandler system, enabling rich character-based experiences in your WebVerse applications.