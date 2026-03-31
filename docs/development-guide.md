# WebVerse-Runtime Development Guide

## Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| **Unity** | **6000.0.58f2** (Unity 6) | Must match exactly for project compatibility |
| **Render Pipeline** | Universal Render Pipeline (URP) | Included via package manager |
| **Git** | Latest | With LFS and submodule support |
| **.NET** | Standard 2.1 | Included with Unity |
| **IDE** | Visual Studio, Rider, or VS Code | Unity IDE integrations included |

## Required Paid Assets (Unity Asset Store)

These are **not included** in the repository and must be purchased separately:

| Asset | Version | Purpose |
|-------|---------|---------|
| Best HTTP | v3.0.17+ | HTTP/HTTPS communication |
| Best MQTT | v3.0.4+ | MQTT protocol support |
| Best WebSockets | v3.0.8+ | WebSocket communication |

## Setup

### 1. Clone Repository

```bash
git clone --recurse-submodules https://github.com/Five-Squared-Interactive/WebVerse-Runtime.git
cd WebVerse-Runtime
```

If already cloned without submodules:
```bash
git submodule update --init --recursive
```

### 2. Open in Unity

1. Open Unity Hub and add the project folder
2. Ensure Unity **6000.0.58f2** is installed
3. Open the project — you will see errors until step 3

### 3. Import Paid Assets

From the Unity Package Manager, import:
- Best HTTP
- Best MQTT
- Best WebSockets

### 4. Verify

Open the desktop scene at `Assets/Runtime/TopLevel/Scenes/DesktopRuntime/` and enter Play mode.

---

## Running WebVerse

### Desktop Mode
1. Open scene: `Assets/Runtime/TopLevel/Scenes/DesktopRuntime/`
2. Select the "WebVerse" GameObject in the Scene Hierarchy
3. Configure test parameters on the `DesktopMode` component:
   - `testStorageMode` — Cache or Persistent
   - `testFilesDirectory` — Local file storage path
   - `testWorldLoadTimeout` — Timeout in seconds
4. Press Play in the Unity Editor

### WebGL Mode
1. Use the `WebMode.cs` entry point
2. Build for WebGL target via File > Build Settings

### Meta Quest 3
1. Use the `Quest3Mode.cs` entry point
2. Ensure Meta XR SDK is configured
3. Build for Android (Quest) target

---

## Building

### Build Methods (via CI/CD or Editor)

| Build Target | Build Method | Output Path |
|-------------|-------------|-------------|
| WebGL | `FiveSQD.WebVerse.Building.Builder.BuildWebGL` | `build/lightweight/WebGL` |
| Windows Desktop | `FiveSQD.WebVerse.Building.Builder.BuildWindowsFocusedMode` | `build/focused-desktop/StandaloneWindows64` |
| Windows SteamVR | `FiveSQD.WebVerse.Building.Builder.BuildWindowsFocusedModeSteamVR` | `build/focused-steamvr/StandaloneWindows64` |
| Mac Desktop | Via build-runtime.yml | `build/focused-desktop/StandaloneOSX` |
| iOS | Via build-runtime.yml | `build/ios/iOS` |
| Android | Via build-runtime.yml | `build/android/Android` |

### Manual Build
1. File > Build Settings
2. Select target platform
3. Configure player settings
4. Build

---

## Testing

### Unit Test Framework
WebVerse uses the **Unity Test Framework** (v1.5.1) with PlayMode tests.

### Running Tests

**Via Unity Editor:**
1. Window > General > Test Runner
2. Select PlayMode tab
3. Run All or select specific tests

**Via Command Line (CI):**
```bash
# Uses game-ci/unity-test-runner action
# Test mode: playmode
```

### Test Organization

Tests are **co-located** with their source code:

| Test Suite | Location | Tests |
|-----------|----------|-------|
| FileHandler | `Handlers/FileHandler/Tests/` | File I/O operations |
| GLTFHandler | `Handlers/GLTFHandler/Tests/` | 3D model loading |
| ImageHandler | `Handlers/ImageHandler/Tests/` | Image/texture loading |
| JavascriptHandler | `Handlers/JavascriptHandler/Tests/` | JS API execution |
| VEMLHandler | `Handlers/VEMLHandler/Tests/` | VEML parsing |
| X3DHandler | `Handlers/X3DHandler/Tests/` | X3D parsing |
| VoiceHandler | `Handlers/VoiceHandler/Tests/` | Voice chat |
| TimeHandler | `Handlers/TimeHandler/Tests/` | Interval scheduling |
| OMIHandler | `Handlers/OMIHandler/Tests/` | OMI extensions |
| LocalStorage | `LocalStorage/Tests/` | Storage CRUD |
| Utilities | `Utilities/Tests/` | Memory cleanup, utilities |
| VOSSynchronizer | `VOSSynchronizer/Tests/` | VOS sync |
| WorldSync | `WorldSync/Tests/` | WorldSync multiplayer |
| Web Interface | `Web Interface/Tests/` | HTTP/WS/MQTT |
| TabUI | `TopLevel/UserInterface/TabUI/Tests/` | Tab UI |
| Camera | `StraightFour/Testing/CameraTests/` | Camera system |
| Entity | `StraightFour/Testing/EntityTests/` | Entity lifecycle |
| ErrorHandling | `StraightFour/Testing/ErrorHandlingTests/` | Error recovery |
| Synchronization | `StraightFour/Testing/SynchronizationTests/` | Sync |
| WorldState | `StraightFour/Testing/WorldStateTests/` | State snapshots |
| WorldStorage | `StraightFour/Testing/WorldStorageTests/` | Persistence |
| World | `StraightFour/Testing/WorldTests/` | World lifecycle |

### Assembly Definitions

Each module has its own `.asmdef` for test isolation. Test assemblies reference their source assembly and `UnityEngine.TestRunner`.

---

## CI/CD Pipelines

### `testandbuild.yml` — Test & Build (PR workflow)

**Trigger:** Pull requests + manual dispatch
**Jobs:**
1. **unit-tests** — Run PlayMode tests on Ubuntu (Unity 6000.0.35f1)
2. **build-webgl** — Build WebGL (after tests pass)
3. **build-windows-focused-desktop** — Build Windows Desktop
4. **build-windows-focused-steamvr** — Build Windows SteamVR

**Artifacts:** Test results, coverage reports, build outputs

### `build-runtime.yml` — Full Build (Push to main)

**Trigger:** Push to main + manual dispatch (per-platform toggles)
**Unity Version:** 6000.0.58f2
**Platforms:**
- Windows Desktop (self-hosted runner)
- Mac Desktop (self-hosted runner)
- WebGL Compressed + Uncompressed (self-hosted)
- iOS (self-hosted macOS runner)
- Android (self-hosted)

**Post-build:** Artifacts uploaded to AWS S3

### `activation.yml` — Unity License

Activates Unity license for CI runners.

---

## Architecture Patterns for Development

### Creating a New Handler

1. Create a new folder under `Assets/Runtime/Handlers/YourHandler/`
2. Add `Scripts/` and `Tests/` subdirectories
3. Create your handler class inheriting `BaseHandler`:

```csharp
public class YourHandler : BaseHandler
{
    public override void Initialize()
    {
        base.Initialize();
        // Setup
    }

    public override void Terminate()
    {
        // Cleanup
        base.Terminate();
    }
}
```

4. Create an `.asmdef` file referencing `FiveSQD.WebVerse`
5. Register with `WebVerseRuntime.cs`

### Creating a New Entity Type

1. Add folder under `Assets/Runtime/StraightFour/Entity/YourEntity/`
2. Inherit from `BaseEntity` (or `UIEntity` for UI types)
3. Register with `EntityManager`
4. Add JavaScript API wrapper in `Handlers/JavascriptHandler/APIs/Entity/`

### Creating a New OMI Extension Handler

1. Add handler class in `Handlers/OMIHandler/Scripts/StraightFour/`
2. Inherit from `StraightFourHandlerBase`
3. Set priority (50-100, higher = executes first)
4. Use `GetOrCreateEntity()` for safe entity access
5. Register with the OMI handler system

### Exposing a New JavaScript API

1. Create API class in `Handlers/JavascriptHandler/APIs/YourCategory/Scripts/`
2. Add to the `apis` tuple array in `JavascriptHandler.cs`
3. Public methods and properties are automatically exposed to JINT

---

## Common Development Tasks

### Loading a Test World
```
1. Enter Play mode in DesktopRuntime scene
2. Type a VEML URL in the multibar
3. Press Enter to load
```

### Using the Automation API
```bash
# Check runtime status
curl http://localhost:{port}/api/v1/status

# Load a world
curl -X POST http://localhost:{port}/api/v1/world/load \
  -d '{"url": "https://example.com/world.veml"}'

# Execute JavaScript
curl -X POST http://localhost:{port}/api/v1/script/run \
  -d '{"script": "Logging.Log(\"Hello from automation\");"}'

# Capture screenshot
curl http://localhost:{port}/api/v1/screenshot --output screenshot.png
```

### Debugging
- **Console Component:** Access via Ctrl+F12 or Console panel
- **Unity Console:** Standard Unity logging (Debug.Log)
- **Logging Configuration:** Set log levels in `LoggingConfiguration`
- **Memory Profiler:** Unity Memory Profiler package (v1.1.9)

---

## Environment Configuration

### Runtime Settings (Inspector)
| Setting | Description | Default |
|---------|-------------|---------|
| Storage Mode | Cache (memory) or Persistent (SQLite) | Cache |
| Max Entries | Maximum storage entries | 2048 |
| Max Entry Length | Maximum value length | — |
| Max Key Length | Maximum key length | — |
| Files Directory | Local file storage path | — |
| World Load Timeout | Timeout for world loading (seconds) | 10 |

### Conditional Compilation Defines
| Define | Purpose |
|--------|---------|
| `USE_BESTHTTP` | Enables Best HTTP library features (WebSocket, MQTT) |
| `USE_WEBINTERFACE` | Enables Web Interface components |

---

*Generated by BMAD Document Project Workflow — Exhaustive Scan*
*Project: WebVerse-Runtime | Engine: Unity 6 (6000.0.58f2) | Date: 2026-03-17*
