# OMI-StraightFour Integration Guide

**Version:** 1.0
**Date:** 2026-01-24
**WebVerse-Runtime Version:** Unity 2021.3.26 with URP

---

## Overview

This guide documents how Open Metaverse Interoperability (OMI) glTF extensions integrate with the StraightFour World Engine in WebVerse-Runtime. The integration uses a **true integration architecture** where OMI handlers directly create and configure StraightFour entities without intermediate configuration objects.

**Key Principles:**
- OMI extensions define entity behavior and properties
- Handlers create StraightFour entities directly from OMI data
- No configuration layer - direct property mapping from OMI JSON to Unity components
- Factory pattern determines entity type from extension combinations
- Priority-based execution ensures correct setup sequence

---

## Architecture Overview

### Integration Flow

```
glTF File (with OMI extensions)
    ↓
glTFast Importer (Unity plugin)
    ↓
Unity GameObjects created for each node
    ↓
OMI Handlers execute in priority order
    ↓
First Handler → StraightFourEntityFactory.CreateEntityFromNode()
    ↓
Factory analyzes extensions → determines entity type
    ↓
Factory creates StraightFour entity component
    ↓
Factory registers entity with EntityManager (GUID tracking)
    ↓
Factory creates JavaScript wrapper (API access)
    ↓
Subsequent Handlers → GetOrCreateEntity() returns existing entity
    ↓
Handlers configure entity properties from OMI JSON
    ↓
glTF import complete → entities ready for use
```

### Key Components

#### 1. StraightFourEntityFactory

**Location:** `Assets/Runtime/Handlers/OMIHandler/Scripts/StraightFour/StraightFourEntityFactory.cs`

**Purpose:** Central factory for creating appropriate StraightFour entities based on OMI extension analysis.

**Supported Entity Types:**
- `Container` - Empty nodes, grouping nodes
- `Mesh` - Nodes with mesh data
- `Automobile` - Nodes with `OMI_vehicle_body` + wheels
- `Airplane` - Nodes with `OMI_vehicle_body` + vertical thrusters or no wheels
- `Character` - Nodes with `OMI_personality` or CharacterController
- `Audio` - Nodes with `KHR_audio_emitter` or `OMI_audio_emitter`

**Key Methods:**
```csharp
public static BaseEntity CreateEntityFromNode(
    GameObject gameObject,
    int nodeIndex,
    OMIImportContext context,
    BaseEntity parent = null)
```

Creates entity, registers with EntityManager, creates JavaScript wrapper, returns entity instance.

#### 2. StraightFourHandlerBase

**Location:** `Assets/Runtime/Handlers/OMIHandler/Scripts/StraightFour/StraightFourHandlerBase.cs`

**Purpose:** Base class for all OMI-StraightFour handlers providing common utilities.

**Key Methods:**
```csharp
protected BaseEntity GetOrCreateEntity(
    OMIImportContext context,
    int nodeIndex,
    GameObject gameObject,
    BaseEntity parent = null)
```

Safety mechanism - returns existing entity if already created, otherwise creates new entity via factory.

**Additional Methods:**
- `GetEntityForNode()` - Check if entity exists for node
- `RegisterEntity()` - Add entity to node-to-entity map
- `GetRuntime()` - Access WebVerseRuntime
- `LogVerbose()` - Conditional logging

#### 3. OMIExtensionDetector

**Location:** `Assets/Runtime/Handlers/OMIHandler/Scripts/StraightFour/OMIExtensionDetector.cs`

**Purpose:** Utility for checking OMI extensions on nodes.

**Key Methods:**
```csharp
public static bool HasExtension(
    OMIImportContext context,
    int nodeIndex,
    string extensionName)

public static List<int> FindChildNodesWithExtension(
    OMIImportContext context,
    int parentNodeIndex,
    string extensionName)
```

Performance: O(1) extension checks, O(n) recursive child searches.

---

## Entity Type Determination

The factory determines entity type using **priority-based extension checking**:

### Priority Order

1. **Vehicle** - `OMI_vehicle_body` present
   - Aircraft: Has vertical thrusters OR (has vehicle_body AND no wheels)
   - Automobile: Has vehicle_body AND has wheels

2. **Character** - `OMI_personality` present OR GameObject has CharacterController

3. **Audio** - `KHR_audio_emitter` OR `OMI_audio_emitter` present

4. **Mesh** - GameObject has MeshFilter/MeshRenderer

5. **Container** - Default for empty nodes

### Aircraft vs. Automobile Detection

**Aircraft Criteria:**
- Has `OMI_vehicle_thruster` children with vertical orientation (thrust direction aligned with Y-axis > 0.7), OR
- Has `OMI_vehicle_body` but no `OMI_vehicle_wheel` children

**Automobile Criteria:**
- Has `OMI_vehicle_body` AND has `OMI_vehicle_wheel` children

**Example glTF:**
```json
{
  "nodes": [
    {
      "name": "car",
      "extensions": {
        "OMI_vehicle_body": { ... }
      },
      "children": [1, 2, 3, 4]
    },
    {
      "name": "wheel_front_left",
      "extensions": {
        "OMI_vehicle_wheel": { ... }
      }
    }
  ]
}
```
→ Creates `AutomobileEntity` (vehicle body + wheels)

```json
{
  "nodes": [
    {
      "name": "helicopter",
      "extensions": {
        "OMI_vehicle_body": { ... }
      },
      "children": [1, 2]
    },
    {
      "name": "rotor_main",
      "extensions": {
        "OMI_vehicle_thruster": {
          "thrustDirection": [0, 1, 0]
        }
      }
    }
  ]
}
```
→ Creates `AirplaneEntity` (vehicle body + vertical thruster)

---

## Handler Priority System

Handlers execute in priority order (higher number = executes first).

### Document-Level Handlers (95-100)

**Purpose:** Setup, validation, data preparation

| Handler | Priority | Purpose |
|---------|----------|---------|
| `StraightFourGltfDocumentHandler` | 100 | Store glTF JSON in context for handler access |

### Node-Level Handlers (50-90)

**Purpose:** Entity creation and configuration

| Handler | Priority | Entity Creation | Purpose |
|---------|----------|----------------|---------|
| `StraightFourPhysicsBodyHandler` | 90 | No | Configure Rigidbody physics |
| `StraightFourPhysicsShapeNodeHandler` | 85 | No | Add collision shapes |
| `StraightFourPhysicsJointHandler` | 80 | No | Create physics joints |
| `StraightFourPhysicsGravityNodeHandler` | 70 | No | Gravity modifiers |
| `StraightFourVehicleBodyHandler` | 60 | **Yes** | Vehicle physics, creates vehicle entities |
| `StraightFourAudioSourceHandler` | 60 | **Yes** | Audio configuration, creates audio entities |
| `StraightFourVehicleWheelHandler` | 55 | No | Configure vehicle wheels |
| `StraightFourVehicleThrusterHandler` | 55 | No | Configure vehicle thrusters |
| `StraightFourSeatHandler` | 50 | No | Add seat components |
| `StraightFourLinkHandler` | 50 | No | Add link behavior |
| `StraightFourSpawnPointHandler` | 50 | No | Register spawn points (metadata only) |

### Priority Rationale

**Physics First (90):**
- Physics handlers run first to ensure Rigidbody exists
- Vehicle handlers (60) expect Rigidbody to already be configured
- Prevents conflicts and ensures correct initialization order

**Vehicle After Physics (60):**
- VehicleBodyHandler creates vehicle entities
- Configures vehicle-specific physics (damping, centerOfMass if not set by physics handler)
- Adds vehicle behavior components

**Wheels/Thrusters Last (55):**
- Require vehicle entity to exist
- Configure vehicle subsystems after main entity created

### Conflict Resolution: OMI_physics_body + OMI_vehicle_body

When a node has both extensions, **physics body takes precedence** for Rigidbody configuration:

**Implementation:** Marker component pattern (`OMIPhysicsBodyConfigured`)

```csharp
// PhysicsBodyHandler adds marker after configuring Rigidbody
if (targetObject.GetComponent<OMIPhysicsBodyConfigured>() == null)
{
    targetObject.AddComponent<OMIPhysicsBodyConfigured>();
}

// VehicleBodyHandler checks for marker before setting conflicting properties
var physicsBodyConfigured = automobile.gameObject.GetComponent<OMIPhysicsBodyConfigured>();
if (physicsBodyConfigured != null)
{
    // Skip centerOfMass configuration (physics body already set it)
    // Log warning to inform user
}
else
{
    // Configure centerOfMass from vehicle body extension
}
```

**Properties Affected:**
- `centerOfMass` - Physics body wins, vehicle body skipped
- `linearDamping` / `angularDamping` - Vehicle body always configures (no conflict)
- `mass`, `useGravity`, `isKinematic` - Only set by physics body

**User Experience:**
- Warning logged when dual extensions detected
- Clear precedence documented
- No silent overwrites

---

## Supported OMI Extensions

### Physics Extensions

#### 1. OMI_physics_body

**Specification:** [OMI_physics_body](https://github.com/omigroup/gltf-extensions/tree/main/extensions/2.0/OMI_physics_body)

**Purpose:** Configure Unity Rigidbody component for physics simulation.

**Handler:** `StraightFourPhysicsBodyHandler` (Priority 90)

**Mapping:**

| OMI Property | Unity Property | Notes |
|--------------|---------------|-------|
| `motion.type` | `Rigidbody.isKinematic` | "kinematic" → true, "dynamic"/"static" → false |
| `motion.mass` | `Rigidbody.mass` | Direct mapping |
| `motion.centerOfMass` | `Rigidbody.centerOfMass` | Converted from glTF to Unity coordinates |
| `motion.inertiaTensor` | `Rigidbody.inertiaTensor` | Direct mapping |
| `motion.gravityFactor` | `Rigidbody.useGravity` | > 0 → true, else false |
| `motion.linearDamping` | `Rigidbody.linearDamping` | Direct mapping (if not set by vehicle body) |
| `motion.angularDamping` | `Rigidbody.angularDamping` | Direct mapping (if not set by vehicle body) |

**Example:**
```json
{
  "extensions": {
    "OMI_physics_body": {
      "motion": {
        "type": "dynamic",
        "mass": 1500,
        "centerOfMass": [0, -0.5, 0],
        "gravityFactor": 1.0,
        "linearDamping": 0.05,
        "angularDamping": 0.05
      }
    }
  }
}
```

#### 2. OMI_physics_shape

**Purpose:** Add collision shapes to GameObjects.

**Handler:** `StraightFourPhysicsShapeNodeHandler` (Priority 85)

**Supported Shapes:**
- `box` → `BoxCollider`
- `sphere` → `SphereCollider`
- `capsule` → `CapsuleCollider`
- `cylinder` → `CapsuleCollider` (Unity doesn't have CylinderCollider)
- `convex` → `MeshCollider` (convex = true)
- `trimesh` → `MeshCollider` (convex = false)

**Properties:**
- `isTrigger` → `Collider.isTrigger`
- Shape-specific size parameters mapped to collider dimensions

#### 3. OMI_physics_joint

**Purpose:** Create physics constraints between objects.

**Handler:** `StraightFourPhysicsJointHandler` (Priority 80)

**Supported Joints:**
- `fixed` → `FixedJoint`
- `hinge` → `HingeJoint`
- `slider` → `ConfigurableJoint` (linear constraint)
- `spring` → `SpringJoint`

**Configuration:**
- `connectedNode` → Find target entity and set `connectedBody`
- Joint-specific parameters (limits, spring, damper) mapped to Unity joint properties

#### 4. OMI_physics_gravity

**Purpose:** Modify gravity for objects or create gravity volumes.

**Handler:** `StraightFourPhysicsGravityNodeHandler` (Priority 70)

**Features:**
- Gravity modifiers: Apply custom gravity vector to Rigidbody
- Gravity volumes: Trigger colliders that apply gravity to entering objects

### Vehicle Extensions

#### 5. OMI_vehicle_body

**Purpose:** Configure vehicle physics and behavior.

**Handler:** `StraightFourVehicleBodyHandler` (Priority 60)

**Creates:** `AutomobileEntity` or `AirplaneEntity` (based on aircraft detection)

**Mapping:**

| OMI Property | Unity Property/Behavior | Notes |
|--------------|------------------------|-------|
| `linearDampeners` | `Rigidbody.linearDamping` | true → 0.5f (automobile) or 1.0f (airplane) |
| `angularDampeners` | `Rigidbody.angularDamping` | true → 0.5f (automobile) or 2.0f (airplane) |
| `centerOfMass` | `Rigidbody.centerOfMass` | Only if physics body didn't already set it |
| `maxSpeed` | `OMIVehicleBodyBehavior.MaxSpeed` | Speed limit enforcement |
| `useThrottle` | `AirplaneEntity.throttle` | Airplane: true → 0.0f, false → 1.0f |

**Behavior Component:** `OMIVehicleBodyBehavior` added to GameObject
- Applies angular forces (pitch, yaw, roll) from activation vectors
- Enforces max speed limit
- Handles throttle input (if useThrottle enabled)

#### 6. OMI_vehicle_wheel

**Purpose:** Configure automobile wheels.

**Handler:** `StraightFourVehicleWheelHandler` (Priority 55)

**Integration:** Configures NWH Vehicle Physics wheel components on AutomobileEntity.

#### 7. OMI_vehicle_thruster

**Purpose:** Configure aircraft thrusters.

**Handler:** `StraightFourVehicleThrusterHandler` (Priority 55)

**Integration:** Configures thrust components on AirplaneEntity, used for aircraft detection.

### Audio Extensions

#### 8. KHR_audio_emitter / OMI_audio_emitter

**Purpose:** Create spatial audio sources.

**Handler:** `StraightFourAudioSourceHandler` (Priority 60)

**Creates:** `AudioEntity`

**Mapping:**

| OMI Property | Unity Property | Notes |
|--------------|---------------|-------|
| `gain` | `AudioSource.volume` | Direct mapping |
| `loop` | `AudioSource.loop` | Direct mapping |
| `autoPlay` | Triggers `AudioSource.Play()` | If true, audio plays on load |
| `coneInnerAngle` | `AudioSource.spread` | Spatial audio cone |
| `coneOuterAngle` | `AudioSource.spread` | Outer cone calculation |
| `coneOuterGain` | `AudioSource.volume` | Outer cone volume attenuation |

**Note:** Audio clip must be loaded separately (referenced via glTF buffer or external URL).

### Interaction Extensions

#### 9. OMI_seat

**Purpose:** Define character seating positions with IK.

**Handler:** `StraightFourSeatHandler` (Priority 50)

**Behavior Component:** `OMISeatBehavior` added to GameObject

**Mapping:**

| OMI Property | SeatData Property | Notes |
|--------------|------------------|-------|
| `back` | `backDirection` | Hip/back position limit |
| `foot` | `footOffset` | Foot position in local space |
| `knee` | `kneeOffset` | Knee position in local space |
| `angle` | `seatAngle` | Converted from radians to degrees |

**Features:**
- `TrySit(character)` / `StandUp()` methods
- Occupancy tracking (local only, see multiplayer limitations)
- IK target positions for seated characters

#### 10. OMI_link

**Purpose:** Add hyperlink behavior to objects.

**Handler:** `StraightFourLinkHandler` (Priority 50)

**Behavior Component:** `OMILinkBehavior` added to GameObject

**Features:**
- Click to navigate to URL
- Supports HTTP/HTTPS links
- Integrates with InputManager for raycasting

#### 11. OMI_spawn_point

**Purpose:** Define player spawn locations.

**Handler:** `StraightFourSpawnPointHandler` (Priority 50)

**Special:** Does NOT create entity (metadata only)

**Mapping:**

| OMI Property | SpawnPointRegistry Entry |
|--------------|-------------------------|
| `title` | Name/label for spawn point |
| Node position | World position for spawn |
| Node rotation | Spawn orientation |

**Storage:** Registered with `SpawnPointRegistry` for level/scene management.

#### 12. OMI_personality

**Purpose:** Define character personality traits.

**Handler:** Currently not implemented (extension detection only)

**Effect:** Triggers Character entity creation when present.

---

## EntityManager Integration

### Registration Flow

All OMI-created entities are automatically registered with the EntityManager:

```csharp
// Inside StraightFourEntityFactory.CreateEntityFromNode()

// 1. Create entity
BaseEntity entity = CreateAutomobileEntity(gameObject, entityId, parent, context);

// 2. Register with EntityManager
var runtime = GetRuntime(context);
if (runtime?.straightFour != null)
{
    runtime.straightFour.RegisterEntity(entity, entityId);
}

// 3. Create JavaScript wrapper
CreateAndMapJavaScriptWrapper(entity, entityType);
```

### EntityManager.RegisterEntity()

**Purpose:** Track entities by GUID for JavaScript API access, world save/load, and multiplayer sync.

**Implementation:**
```csharp
public void RegisterEntity(BaseEntity entity, Guid id)
{
    if (entities.ContainsKey(id))
    {
        // Overwrite existing (warning logged)
        entities[id] = entity;
    }
    else
    {
        entities.Add(id, entity);
    }
}
```

**Dictionary:** `Dictionary<Guid, BaseEntity>`
- O(1) add/lookup/remove
- Persists for lifetime of world instance
- Cleared on world unload

### JavaScript API Integration

Entities receive JavaScript wrappers for API access:

```csharp
private static void CreateAndMapJavaScriptWrapper(BaseEntity entity, EntityType entityType)
{
    // Determine wrapper type
    Type wrapperType = entityType switch
    {
        EntityType.Automobile => typeof(AutomobileEntityAPI),
        EntityType.Airplane => typeof(AirplaneEntityAPI),
        EntityType.Audio => typeof(AudioEntityAPI),
        EntityType.Character => typeof(CharacterEntityAPI),
        EntityType.Mesh => typeof(MeshEntityAPI),
        EntityType.Container => typeof(ContainerEntityAPI),
        _ => typeof(BaseEntityAPI)
    };

    // Create wrapper via reflection
    var wrapper = Activator.CreateInstance(wrapperType);
    var entityProperty = wrapperType.GetProperty("entity");
    entityProperty.SetValue(wrapper, entity);

    // Add mapping for JavaScript access
    EntityAPIHelper.AddEntityMapping(entity.id, wrapper);
}
```

**JavaScript Access:**
```javascript
// Get entity by GUID
var vehicle = Entity.Get(vehicleGuid);

// Type-specific API methods available
vehicle.setThrottle(0.8);
vehicle.setSteering(0.5);
```

**Entity.Get() Type Checking:**

Updated to include vehicle entities:
```csharp
public object Get(string id)
{
    BaseEntity entity = entityManager.FindEntity(Guid.Parse(id));

    // Type checking with vehicle support
    if (entity is MeshEntity) return new MeshEntityAPI(entity as MeshEntity);
    if (entity is AutomobileEntity) return new AutomobileEntityAPI(entity as AutomobileEntity);
    if (entity is AirplaneEntity) return new AirplaneEntityAPI(entity as AirplaneEntity);
    if (entity is AudioEntity) return new AudioEntityAPI(entity as AudioEntity);
    if (entity is CharacterEntity) return new CharacterEntityAPI(entity as CharacterEntity);
    // ... other entity types
    return new BaseEntityAPI(entity);
}
```

---

## Multiplayer Synchronization

### What Syncs ✅

OMI entities integrate with VOS multiplayer via entity-level synchronization:

**Entity Creation:**
- `AutomobileEntity`, `AirplaneEntity`, `AudioEntity` creation syncs across clients
- Initial configuration (position, rotation, scale) syncs
- Parent-child hierarchy syncs

**Transform Sync:**
- `SetPosition()`, `SetRotation()`, `SetScale()` sync real-time

**Physics Sync:**
- `SetPhysicalProperties()` syncs Rigidbody properties:
  - `mass`, `centerOfMass`, `drag` (linearDamping), `angularDrag` (angularDamping), `useGravity`
- `SetMotion()` syncs motion state:
  - `velocity` (linearVelocity), `angularVelocity`, `stationary` (isKinematic)

**Result:** Physics-driven vehicle movement visible to all clients.

### What Does NOT Sync ❌

**OMI Behavior Components:**

OMI behavior state does NOT synchronize automatically:

| Component | Property | Sync Status |
|-----------|----------|-------------|
| `OMIVehicleBodyBehavior` | `AngularActivation` | ❌ NOT synced |
| | `LinearActivation` | ❌ NOT synced |
| | `currentThrottle` | ❌ NOT synced |
| `OMISeatBehavior` | `IsOccupied` | ❌ NOT synced |
| | `OccupyingCharacter` | ❌ NOT synced |
| `AudioEntity` | Playback state (play/pause/stop) | ❌ NOT synced |
| | Playback position | ❌ NOT synced |

**Root Cause:** VOSSynchronizer designed for entity-level sync only (transform, physics). No component-level sync architecture.

**Impact:**
- **Vehicle inputs:** Other clients don't see throttle/steering values (only see motion result)
- **Seat occupancy:** Multiple clients can sit in same seat
- **Audio playback:** Play/pause/stop doesn't propagate to other clients

### Multiplayer Enhancement Backlog

Enhancement tasks documented in: `_bmad-output/Multiplayer-Sync-Enhancement-Backlog.md`

**Priority 2 (Recommended Before Release):**
- MP.1: Audio Playback Sync (2-4h) - Most noticeable issue
- MP.2: Seat Occupancy Sync (1-2h) - Use parent sync pattern
- MP.3: Sync Limitations Documentation (1-2h) - Immediate value

**Priority 3 (Optional):**
- MP.4: Vehicle Input Visualization Sync (4-6h) - For UI gauges, steering wheel animations
- MP.5: Audio Spatial Properties Sync (2-3h) - Runtime volume/pitch changes

**Total P2 Effort:** 4-8 hours

For detailed analysis, see: `_bmad-output/V9-VOS-Multiplayer-Sync-Analysis.md`

---

## Performance Characteristics

### Entity Creation Cost

**Analyzed via code review (V.10 Performance Analysis):**

| Entity Type | Creation Cost | Notes |
|-------------|--------------|-------|
| Container | ~0.5ms | Simplest entity type |
| Mesh | ~0.5-1ms | + mesh data processing |
| Audio | ~0.5-1ms | + AudioSource configuration |
| Character | ~0.5-1ms | + CharacterController setup |
| Automobile | ~0.7-1.3ms | + Rigidbody + vehicle physics |
| Airplane | ~1.5-3ms | + aircraft detection (recursive child search) |

**Additional Overhead:**
- EntityManager.RegisterEntity: ~0.05ms per entity
- JavaScript wrapper creation: ~0.08-0.14ms per entity (reflection-based)
- GetOrCreateEntity safety check: ~0.02ms per cache hit

**Total per Entity:** ~0.5-3ms depending on complexity

### Scaling Characteristics

**100-Entity Scene:** ~80-160ms total load time (acceptable)

| Entity Count | Load Time (Mixed Scene) | Assessment |
|--------------|------------------------|------------|
| 10 | ~8-12ms | ✅ Imperceptible |
| 50 | ~40-60ms | ✅ Acceptable |
| 100 | ~80-160ms | ✅ Acceptable |
| 200 | ~160-320ms | ⚠️ Noticeable but tolerable |
| 500+ | ~400-800ms+ | ❌ Consider optimization |

**Scaling:** Linear O(n) - no exponential growth

**Bottlenecks (Low Priority):**
1. Aircraft detection: ~1-2ms extra (recursive child search for thrusters)
2. Multiple HasExtension calls: ~0.2ms per entity (5 calls worst case)
3. JavaScript wrapper creation: ~0.08-0.14ms per entity (reflection overhead)

**Assessment:** No optimization needed for typical scenes (10-200 entities).

For detailed performance analysis, see: `_bmad-output/V10-Performance-Analysis.md`

---

## Best Practices

### 1. Entity Type Selection

**Use OMI extensions to define entity type:**
- Want physics? Add `OMI_physics_body`
- Want vehicle? Add `OMI_vehicle_body`
- Want audio? Add `KHR_audio_emitter` or `OMI_audio_emitter`
- Want seating? Add `OMI_seat` to seat nodes

**Factory handles the rest** - no manual entity type specification needed.

### 2. Extension Combinations

**Physics + Vehicle (Recommended):**
```json
{
  "extensions": {
    "OMI_physics_body": {
      "motion": {
        "type": "dynamic",
        "mass": 1500,
        "centerOfMass": [0, -0.5, 0]
      }
    },
    "OMI_vehicle_body": {
      "linearDampeners": true,
      "angularDampeners": true
    }
  }
}
```
- Physics body sets mass, centerOfMass, inertiaTensor
- Vehicle body sets damping
- Physics takes precedence for centerOfMass

**Vehicle + Audio + Seats:**
```json
{
  "name": "car",
  "extensions": { "OMI_vehicle_body": {...} },
  "children": [1, 2, 3, 4, 5]
},
{
  "name": "driver_seat",
  "extensions": { "OMI_seat": {...} }
},
{
  "name": "engine_sound",
  "extensions": { "KHR_audio_emitter": {...} }
}
```
- Vehicle creates AutomobileEntity
- Seat handler adds seat behavior
- Audio handler creates AudioEntity child

### 3. Avoiding Common Pitfalls

**Don't:**
- ❌ Mix `OMI_vehicle_wheel` with aircraft (creates Automobile instead of Airplane)
- ❌ Put `OMI_seat` on vehicle body node (seat should be child node)
- ❌ Expect OMI behavior state to sync in multiplayer (use P2 enhancements)

**Do:**
- ✅ Use consistent extension combinations
- ✅ Follow OMI specifications for property values
- ✅ Test multiplayer scenarios early (identify sync limitations)
- ✅ Use physics body + vehicle body together for vehicles

### 4. Debugging

**Logging:**
- Enable verbose logging in OMI import context: `context.Settings.VerboseLogging = true`
- Check Unity console for handler execution logs
- Factory logs entity type determination

**Common Issues:**
- Entity not created → Check extension presence via OMIExtensionDetector
- Wrong entity type → Check aircraft detection logic (thrusters vs. wheels)
- Multiple entities for same node → GetOrCreateEntity should prevent this (report bug)

---

## Migration from Previous Architecture

**Old Approach (Configuration Pattern):**
- Entities configured via `ConfigureFromOMI()` methods
- Configuration objects passed between handlers
- OMI knowledge in both handlers and entity classes

**New Approach (True Integration):**
- Entities created via factory pattern
- Handlers configure properties directly from OMI JSON
- OMI knowledge only in handlers

**Breaking Changes:** None for end users (glTF files work the same)

**Internal Changes:** Entity classes no longer have `ConfigureFromOMI()` methods

For migration details, see: `_bmad-output/Phase-2-Refactor-Summary.md`

---

## Validation Status

**Testing Phase:** Post-refactor validation (V.1-V.13)

**Completed Validation:**
- ✅ V.1: Compilation (zero errors)
- ⚠️ V.2: Vehicle Test (logic validated, E2E pending)
- ⚠️ V.3: Audio Test (logic validated, E2E pending)
- ✅ V.4: JavaScript API (gap fixed, 83 lines added)
- ⚠️ V.5: World Save/Load (infrastructure validated, feature not implemented yet)
- ⚠️ V.6: Physics Handlers (temporary acceptance, refactoring required long-term)
- ✅ V.7: Spawn Point Handler (clean implementation, approved)
- ⚠️ V.8: Integration Test (logic validated, Rigidbody conflict fixed)
- ⚠️ V.9: Multiplayer Sync (analysis complete, gaps documented)
- ✅ V.10: Performance (acceptable, 5-20% faster than old approach)

**Pending Validation:**
- V.11: Update Documentation (in progress)
- V.12: Final Architectural Validation
- V.13: Cleanup Deprecated Code

**Known Limitations:**
- E2E runtime tests deferred to comprehensive testing phase
- Physics handler architectural violations (component additions) accepted temporarily
- Multiplayer sync gaps identified (audio playback, seat occupancy, vehicle inputs)

For validation status, see: `_bmad-output/Validation-Roadmap-Status.md`

---

## Related Documentation

- **Architecture Overview:** `WebVerse-Runtime/architecture.md`
- **Validation Reports:** `_bmad-output/V*-*.md` (13 reports)
- **Multiplayer Sync Analysis:** `_bmad-output/V9-VOS-Multiplayer-Sync-Analysis.md`
- **Performance Analysis:** `_bmad-output/V10-Performance-Analysis.md`
- **Enhancement Backlog:** `_bmad-output/Multiplayer-Sync-Enhancement-Backlog.md`
- **Refactor Summary:** `_bmad-output/Phase-2-Refactor-Summary.md`

---

## Support and Contributions

**Questions:** Check validation reports in `_bmad-output/` directory

**Bug Reports:** Include glTF file, console logs, and expected vs. actual behavior

**Feature Requests:** Submit via appropriate channels with use case description

---

**Document Version:** 1.0
**Last Updated:** 2026-01-24
**Status:** Active - reflects current OMI-StraightFour integration architecture
