# Environment Entity JSON Support Documentation

The JSONEntityHandler now supports creating voxel, water, and water blocker entities from JSON data. This extends the WebVerse entity creation system to include comprehensive environment entities.

## Voxel Entities

Voxel entities allow you to create block-based structures with customizable textures and block types.

### JSON Structure

```json
{
  "type": "voxel",
  "id": "unique-entity-id",
  "tag": "VoxelStructureTag",
  "position": {"x": 0.0, "y": 0.0, "z": 0.0},
  "rotation": {"x": 0.0, "y": 0.0, "z": 0.0, "w": 1.0},
  "scale": {"x": 1.0, "y": 1.0, "z": 1.0},
  "blockInfos": [
    {
      "id": 1,
      "subTypes": [
        {
          "id": 0,
          "invisible": false,
          "topTexture": "grass_top",
          "bottomTexture": "dirt",
          "leftTexture": "grass_side",
          "rightTexture": "grass_side",
          "frontTexture": "grass_side",
          "backTexture": "grass_side"
        }
      ]
    }
  ],
  "blocks": [
    {"x": 0, "y": 0, "z": 0, "type": 1, "subType": 0}
  ]
}
```

### Properties

- **blockInfos**: Array of block type definitions with texture mappings
- **blocks**: Array of individual block placements in the voxel grid
- **id**: Block type identifier for referencing in block placements
- **subType**: Variant of the block type with different textures
- **invisible**: Whether the block is visible or acts as a collision-only block

### Usage Example

```csharp
// Load from JSON file
handler.LoadVoxelEntityFromJSON(jsonString, parentEntity, (success, entityId, entity) => {
    if (success) {
        Debug.Log($"Voxel entity created: {entityId}");
    }
});

// Create directly from data
JSONVoxelEntity voxelData = new JSONVoxelEntity();
// Configure voxelData properties...
handler.CreateVoxelEntity(voxelData, parentEntity, (entityId, entity) => {
    Debug.Log($"Voxel entity created: {entityId}");
});
```

## Water Entities

Water entities create realistic water bodies with configurable physics and visual properties.

### JSON Structure

```json
{
  "type": "water",
  "id": "unique-entity-id",
  "tag": "WaterBodyTag",
  "position": {"x": 0.0, "y": -0.5, "z": 0.0},
  "rotation": {"x": 0.0, "y": 0.0, "z": 0.0, "w": 1.0},
  "scale": {"x": 20.0, "y": 2.0, "z": 20.0},
  "shallowColor": {"r": 0.4, "g": 0.8, "b": 1.0, "a": 0.8},
  "deepColor": {"r": 0.1, "g": 0.3, "b": 0.7, "a": 1.0},
  "specularColor": {"r": 1.0, "g": 1.0, "b": 1.0, "a": 1.0},
  "scatteringColor": {"r": 0.2, "g": 0.6, "b": 0.8, "a": 1.0},
  "deepStart": 0.5,
  "deepEnd": 10.0,
  "distortion": 32.0,
  "smoothness": 0.8,
  "intensity": 0.7,
  "numWaves": 4.0,
  "waveAmplitude": 0.3,
  "waveSteepness": 0.5,
  "waveSpeed": 1.0,
  "waveLength": 10.0,
  "waveScale": 1.0
}
```

### Properties

#### Color Properties
- **shallowColor**: Color of water in shallow areas
- **deepColor**: Color of water in deep areas
- **specularColor**: Reflection color
- **scatteringColor**: Light scattering color

#### Depth Properties
- **deepStart**: Distance where deep color begins (0.5-50.0)
- **deepEnd**: Distance where deep color is fully applied (1.0-100.0)

#### Visual Properties
- **distortion**: Water surface distortion (0-128)
- **smoothness**: Surface smoothness (0-1)
- **intensity**: Overall visual intensity (0-1)

#### Wave Properties
- **numWaves**: Number of wave patterns (1-32)
- **waveAmplitude**: Wave height (0-1)
- **waveSteepness**: Wave steepness (0-1)
- **waveSpeed**: Wave animation speed
- **waveLength**: Distance between wave peaks
- **waveScale**: Overall wave scale multiplier

### Usage Example

```csharp
// Load from JSON file
handler.LoadWaterEntityFromJSON(jsonString, parentEntity, (success, entityId, entity) => {
    if (success) {
        Debug.Log($"Water entity created: {entityId}");
    }
});

// Create directly from data
JSONWaterEntity waterData = new JSONWaterEntity();
// Configure waterData properties...
handler.CreateWaterEntity(waterData, parentEntity, (entityId, entity) => {
    Debug.Log($"Water entity created: {entityId}");
});
```

## Water Blocker Entities

Water blocker entities create invisible barriers that block water flow and interaction.

### JSON Structure

```json
{
  "type": "waterBlocker",
  "id": "unique-entity-id",
  "tag": "WaterBlockerTag",
  "position": {"x": 15.0, "y": 0.0, "z": 0.0},
  "rotation": {"x": 0.0, "y": 0.0, "z": 0.0, "w": 1.0},
  "scale": {"x": 2.0, "y": 5.0, "z": 20.0}
}
```

### Properties

- **position**: 3D position in world coordinates
- **rotation**: Quaternion rotation
- **scale**: Size of the blocking volume

### Usage Example

```csharp
// Load from JSON file
handler.LoadWaterBlockerEntityFromJSON(jsonString, parentEntity, (success, entityId, entity) => {
    if (success) {
        Debug.Log($"Water blocker entity created: {entityId}");
    }
});

// Create directly from data
JSONWaterBlockerEntity blockerData = new JSONWaterBlockerEntity();
// Configure blockerData properties...
handler.CreateWaterBlockerEntity(blockerData, parentEntity, (entityId, entity) => {
    Debug.Log($"Water blocker entity created: {entityId}");
});
```

## API Methods

### Voxel Entity Methods
- `CreateVoxelEntity(JSONVoxelEntity, BaseEntity, Action<Guid?, BaseEntity>)`
- `LoadVoxelEntityFromJSON(string, BaseEntity, Action<bool, Guid?, BaseEntity>)`

### Water Entity Methods
- `CreateWaterEntity(JSONWaterEntity, BaseEntity, Action<Guid?, BaseEntity>)`
- `LoadWaterEntityFromJSON(string, BaseEntity, Action<bool, Guid?, BaseEntity>)`

### Water Blocker Entity Methods
- `CreateWaterBlockerEntity(JSONWaterBlockerEntity, BaseEntity, Action<Guid?, BaseEntity>)`
- `LoadWaterBlockerEntityFromJSON(string, BaseEntity, Action<bool, Guid?, BaseEntity>)`

## Example Files

- `VoxelEntity_Simple.json` - Basic voxel structure
- `VoxelEntity_House.json` - Complex building with multiple block types
- `WaterEntity_Pond.json` - Calm water body
- `WaterEntity_Ocean.json` - Rough ocean water with large waves
- `WaterBlockerEntity_Dam.json` - Large water barrier
- `WaterBlockerEntity_Barrier.json` - Angled water blocker

## Integration

Environment entities integrate seamlessly with the existing JSONEntityHandler system and support:

- Parent-child relationships with other entities
- Async creation with callback support
- Automatic validation and default value handling
- Error logging and debugging support
- Consistent JSON serialization format

All environment entities follow the same creation patterns as other entity types in the WebVerse system.