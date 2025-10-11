# Audio Entity JSON Support Documentation

The JSONEntityHandler now supports creating audio entities from JSON data. This allows you to define and create audio sources with various properties including 3D positioning, volume control, looping, and playback settings.

## Audio Entities

Audio entities provide sound capabilities with full control over audio properties and 3D spatial positioning.

### JSON Structure

```json
{
  "type": "audio",
  "id": "unique-entity-id",
  "tag": "AudioEntityTag",
  "position": {"x": 0.0, "y": 0.0, "z": 0.0},
  "rotation": {"x": 0.0, "y": 0.0, "z": 0.0, "w": 1.0},
  "audioFile": "Assets/Audio/sound.wav",
  "loop": false,
  "priority": 128,
  "volume": 1.0,
  "pitch": 1.0,
  "stereoPan": 0.0,
  "playOnLoad": false
}
```

### Properties

#### Core Properties
- **audioFile**: Path to the audio file (required, .wav format supported)
- **position**: 3D position in world coordinates for spatial audio
- **rotation**: Quaternion rotation (affects 3D audio orientation)

#### Audio Playback Properties
- **loop**: Whether the audio should loop continuously (true/false)
- **priority**: Audio priority (0-256, lower values = higher priority)
- **volume**: Playback volume (0.0 to 1.0, where 1.0 is maximum)
- **pitch**: Audio pitch (-3.0 to 3.0, where 1.0 is normal speed)
- **stereoPan**: Stereo panning (-1.0 to 1.0, -1.0=left, 0.0=center, 1.0=right)
- **playOnLoad**: Whether to start playing immediately after creation (true/false)

### Property Ranges and Validation

- **Priority**: 0-256 (0 = highest priority, 256 = lowest priority)
- **Volume**: 0.0-1.0 (automatically clamped if out of range)
- **Pitch**: -3.0 to 3.0 (automatically clamped if out of range)
- **Stereo Pan**: -1.0 to 1.0 (automatically clamped if out of range)

### Audio File Requirements

- **Format**: Currently supports .wav files
- **Path**: Should be relative to the project Assets folder
- **Loading**: Audio files are loaded asynchronously when the entity is created

### Usage Examples

#### Background Music
```json
{
  "type": "audio",
  "tag": "BackgroundMusic",
  "position": {"x": 0.0, "y": 0.0, "z": 0.0},
  "audioFile": "Assets/Audio/background_music.wav",
  "loop": true,
  "priority": 128,
  "volume": 0.5,
  "playOnLoad": true
}
```

#### Sound Effect
```json
{
  "type": "audio",
  "tag": "ExplosionSound",
  "position": {"x": 5.0, "y": 1.0, "z": -3.0},
  "audioFile": "Assets/Audio/explosion.wav",
  "loop": false,
  "priority": 64,
  "volume": 0.8,
  "pitch": 1.2,
  "playOnLoad": false
}
```

#### Ambient Sound
```json
{
  "type": "audio",
  "tag": "ForestAmbience",
  "position": {"x": -10.0, "y": 2.0, "z": 15.0},
  "audioFile": "Assets/Audio/forest_ambience.wav",
  "loop": true,
  "priority": 200,
  "volume": 0.3,
  "stereoPan": -0.2,
  "playOnLoad": true
}
```

#### Voice Over
```json
{
  "type": "audio",
  "tag": "NarratorVoice",
  "position": {"x": 0.0, "y": 0.0, "z": 0.0},
  "audioFile": "Assets/Audio/narrator_voice.wav",
  "loop": false,
  "priority": 32,
  "volume": 0.9,
  "playOnLoad": false
}
```

## API Methods

### Audio Entity Methods
- `CreateAudioEntity(JSONAudioEntity, BaseEntity, Action<Guid?, BaseEntity>)`
- `LoadAudioEntityFromJSON(string, BaseEntity, Action<bool, Guid?, BaseEntity>)`

### Usage Example in Code

```csharp
// Load from JSON file
handler.LoadAudioEntityFromJSON(jsonString, parentEntity, (success, entityId, entity) => {
    if (success) {
        Debug.Log($"Audio entity created: {entityId}");
        
        // Control playback after creation
        var audioEntity = entity as AudioEntity;
        audioEntity.Play();  // Start playing
        audioEntity.Stop();  // Stop playing
        audioEntity.TogglePause(true);  // Pause
    }
});

// Create directly from data
JSONAudioEntity audioData = new JSONAudioEntity {
    audioFile = "Assets/Audio/my_sound.wav",
    volume = 0.7f,
    loop = true,
    playOnLoad = true
};

handler.CreateAudioEntity(audioData, parentEntity, (entityId, entity) => {
    Debug.Log($"Audio entity created: {entityId}");
});
```

### Audio Control Methods

After creating an audio entity, you can control playback:

```csharp
var audioEntity = createdEntity as AudioEntity;

// Playback control
audioEntity.Play();              // Start playing
audioEntity.Stop();              // Stop playing
audioEntity.TogglePause(true);   // Pause/unpause

// Property adjustment
audioEntity.volume = 0.5f;       // Adjust volume
audioEntity.pitch = 1.2f;        // Adjust pitch
audioEntity.loop = true;         // Enable looping
audioEntity.priority = 64;       // Change priority
audioEntity.stereoPan = -0.5f;   // Adjust stereo panning
```

## Audio Types and Use Cases

### Background Music
- **Priority**: Medium (128-200)
- **Volume**: Low to medium (0.3-0.6)
- **Loop**: Usually true
- **Position**: Typically at origin (0,0,0)
- **Play on Load**: Usually true

### Sound Effects
- **Priority**: High (32-128)
- **Volume**: Medium to high (0.6-1.0)
- **Loop**: Usually false
- **Position**: At specific 3D locations
- **Play on Load**: Usually false (triggered by events)

### Ambient Sounds
- **Priority**: Low (150-256)
- **Volume**: Low (0.2-0.4)
- **Loop**: Usually true
- **Position**: Distributed in 3D space
- **Play on Load**: Usually true

### Voice Over / Dialog
- **Priority**: Very high (16-64)
- **Volume**: High (0.8-1.0)
- **Loop**: Usually false
- **Position**: Typically at origin or player position
- **Play on Load**: Usually false (triggered by events)

## Example Files

- `AudioEntity_BackgroundMusic.json` - Looping background music
- `AudioEntity_SoundEffect.json` - One-shot sound effect
- `AudioEntity_Ambient.json` - 3D positioned ambient sound
- `AudioEntity_VoiceOver.json` - High-priority voice audio

## Integration Features

Audio entities integrate seamlessly with the JSONEntityHandler system and support:

- Parent-child relationships with other entities
- Async creation with callback support
- Automatic validation and property clamping
- 3D spatial audio positioning
- File loading with error handling
- Comprehensive audio property control
- Consistent JSON serialization format

All audio entities follow the same creation patterns as other entity types in the WebVerse system.