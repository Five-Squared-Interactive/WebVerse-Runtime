# WebVerse-Runtime — Project Overview

## What is WebVerse-Runtime?

WebVerse-Runtime is a **Unity 6-based runtime system** for creating immersive, web-based virtual environments. It serves as the core engine behind the WebVerse platform — a metaverse browser that loads, renders, and interacts with 3D virtual worlds described by VEML (Virtual Environment Markup Language), glTF with OMI extensions, and X3D documents.

Think of it as a **3D web browser engine**: where a traditional browser renders HTML pages, WebVerse-Runtime renders interactive 3D worlds with physics, vehicles, audio, multi-user synchronization, and a JavaScript scripting layer.

## Executive Summary

| Attribute | Value |
|-----------|-------|
| **Project Type** | Unity 6 XR Runtime / Metaverse Browser Engine |
| **Engine** | Unity 6 (6000.0.58f2) |
| **Language** | C# (.NET Standard 2.1) |
| **Rendering** | Universal Render Pipeline (URP) 17.0.4 |
| **Architecture** | Modular handler-based + Entity system + JavaScript scripting |
| **Platforms** | Windows, Mac, WebGL, iOS, Android, Meta Quest 3, SteamVR |
| **Version** | v2.2.0 "Terra Firma" |
| **License** | MIT |
| **Source Files** | 357 custom C# + 385 3rd-party C# |
| **Test Suites** | 22 test assemblies, 46+ test files |

## Architecture at a Glance

```
External Systems (Content Servers, VOS Server)
        ↓
┌─────────────────────────────────────────────┐
│             WebVerseRuntime                  │
│  (Core Orchestrator — Global Singleton)      │
├─────────────────────────────────────────────┤
│  Handlers: VEML | GLTF | Image | File       │
│            JS | OMI | Voice | Time | X3D     │
│            JSONEntity                        │
├─────────────────────────────────────────────┤
│  Managers: Input | Output | HTTP | Storage   │
├─────────────────────────────────────────────┤
│  StraightFour World Engine                   │
│  ├── Entity System (20+ types)               │
│  ├── Camera System                           │
│  ├── Environment (Sky, Fog, Lighting)        │
│  ├── World State (Tabs, Snapshots)           │
│  └── World Storage (Persistence)             │
├─────────────────────────────────────────────┤
│  Synchronization: WorldSync | VOS            │
├─────────────────────────────────────────────┤
│  Web Interface: HTTP | WebSocket | MQTT      │
└─────────────────────────────────────────────┘
        ↓
   Unity Rendering Engine (GameObjects, Physics)
```

## Key Capabilities

### Content Loading
- **VEML** — Virtual Environment Markup Language (10 schema versions, V1.0–V3.0)
- **glTF/GLB** — Standard 3D model format with OMI extensions
- **X3D** — Legacy 3D web standard support
- **Images** — PNG, JPG, texture loading and caching

### Entity System (20+ Types)
- **3D Objects:** Mesh, Container, Terrain, Voxel, Water
- **Vehicles:** Automobile (NWH physics), Airplane (Silantro flight sim)
- **Characters:** Avatar/NPC with animation support
- **Media:** Audio (spatial), Light (point/directional/spot)
- **UI:** Canvas, Button, Text, Input, Image, Dropdown, HTML

### JavaScript Scripting (100+ APIs)
- **Entity management** — Create, modify, destroy 3D objects
- **Input handling** — Desktop + VR controller input
- **Camera control** — Position, rotation, follow, raycast
- **Networking** — HTTP, WebSocket, MQTT
- **Voice chat** — Spatial audio with Opus codec
- **Environment** — Sky, fog, gravity, time-of-day
- **Storage** — Per-site and per-world key-value storage

### Multi-Platform XR
- **Desktop** — Mouse + keyboard with optional SteamVR
- **Meta Quest 3** — Standalone VR with hand tracking
- **WebGL** — Browser-based (lightweight mode)
- **Mobile** — iOS and Android touch input

### Multi-User Synchronization
- **WorldSync** (new) — MQTT-based entity/environment sync with auto-reconnect
- **VOS** (legacy) — Virtual Operating System synchronization

### Open Metaverse Interoperability (OMI)
20 glTF extension handlers for standardized metaverse content:
- Physics (body, shape, joint, gravity)
- Vehicles (body, wheel, thruster)
- Audio emitters
- Spawn points, seats, links, personality

## Technology Stack

| Category | Technology | Version |
|----------|-----------|---------|
| Engine | Unity 6 | 6000.0.58f2 |
| Rendering | URP | 17.0.4 |
| XR | Meta XR SDK, OpenXR, XR Interaction Toolkit | 83.0.4 / 1.15.1 / 3.0.8 |
| JavaScript | JINT | embedded |
| Database | SQLite | embedded |
| Content | glTFast + Draco | 6.15.1 / 5.4.2 |
| Networking | Best HTTP/MQTT/WebSockets | 3.0.17 / 3.0.4 / 3.0.8 |
| WebView | Vuplex | embedded |
| Vehicle Physics | NWH Vehicle Physics 2 | embedded |
| Flight Sim | Silantro | embedded |
| Text | TextMesh Pro | 3.0.6 |
| Input | Input System | 1.15.0 |
| Performance | Burst Compiler | 1.8.25 |

## WebVerse Ecosystem

WebVerse-Runtime is part of a larger ecosystem:

| Component | Description |
|-----------|-------------|
| **WebVerse** | Top-level NodeJS/Electron application (the browser shell) |
| **WebVerse-Runtime** | This project — Unity runtime engine |
| **StraightFour** | World Engine (integrated within Runtime) |
| **VEML** | Virtual Environment Markup Language specification |
| **World APIs** | JavaScript APIs for world interaction |
| **VSS** | VOS Synchronization Service for multi-user |
| **WorldKit** | World editor (separate project in monorepo) |

## Repository Structure

- `Assets/Runtime/` — All source code (357 C# files)
- `Assets/Runtime/StraightFour/` — World Engine (entity system, camera, environment)
- `Assets/Runtime/Handlers/` — 10 content handlers + 20 OMI extension handlers
- `Assets/Runtime/TopLevel/` — Platform entry points + browser UI
- `docs/` — Generated documentation
- `.github/workflows/` — CI/CD pipelines (test, build, deploy to S3)

---

*Generated by BMAD Document Project Workflow — Exhaustive Scan*
*Project: WebVerse-Runtime | Engine: Unity 6 (6000.0.58f2) | Date: 2026-03-17*
