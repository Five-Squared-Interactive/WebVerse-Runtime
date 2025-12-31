# WebVerse-Runtime Documentation

Welcome to the comprehensive documentation for WebVerse-Runtime, the Unity3D runtime component of the WebVerse ecosystem.

## Navigation

### Getting Started
- [Quick Start Guide](./getting-started.md)
- [Installation and Setup](./installation.md)
- [Development Environment](./development.md)

### Architecture & Design
- [System Architecture](./architecture/README.md)
- [Component Overview](./architecture/components.md)
- [Data Flow](./architecture/data-flow.md)
- [Handler System](./architecture/handlers.md)

### API Documentation
- [JavaScript API](./api/javascript-api.md)
- [Handler APIs](./api/handlers.md)
- [Manager APIs](./api/managers.md)
- [Utility APIs](./api/utilities.md)

### Configuration
- [Runtime Configuration](./configuration/runtime.md)
- [Handler Configuration](./configuration/handlers.md)
- [Build Configuration](./configuration/build.md)

### Performance & Optimization
- [WebGL/WebGPU Memory Optimization](./developer/webgl-memory-optimization-summary.md)
  - [Detailed Analysis](./developer/webgl-memory-optimization-analysis.md)
  - [Implementation Guide](./developer/webgl-memory-optimization-implementation.md)

### Extensibility
- [Creating Custom Handlers](./extensibility/custom-handlers.md)
- [Extending APIs](./extensibility/api-extensions.md)
- [Plugin Development](./extensibility/plugins.md)

### Examples & Tutorials
- [Basic Usage Examples](./examples/basic-usage.md)
- [Advanced Scenarios](./examples/advanced.md)
- [Integration Examples](./examples/integration.md)

### Developer Reference
- [Testing Guide](./developer/testing.md)
- [Build System](./developer/build-system.md)
- [Contributing Guidelines](./developer/contributing.md)
- [WebGL/WebGPU Memory Optimization](./developer/webgl-memory-optimization-summary.md)
  - [Full Analysis](./developer/webgl-memory-optimization-analysis.md)
  - [Implementation Guide](./developer/webgl-memory-optimization-implementation.md)

## Overview

WebVerse-Runtime is the Unity3D runtime that enables immersive web-based virtual environments. It provides a comprehensive system for handling various types of content, managing user interactions, and facilitating communication with external services.

### Key Features

- **Multi-format Content Support**: Handle VEML, GLTF, images, and more
- **JavaScript Integration**: Execute JavaScript for dynamic world behavior
- **Input Management**: Support for desktop, VR, and mobile input
- **Real-time Synchronization**: VOS (Virtual Operating System) synchronization
- **Extensible Architecture**: Plugin-based handler system
- **Web Integration**: Seamless integration with web interfaces

### Architecture Highlights

The system is built around a modular handler architecture where specialized handlers manage different aspects of the runtime:

- **Core Runtime**: Main orchestration and lifecycle management
- **Content Handlers**: VEML, GLTF, File, Image processing
- **JavaScript Handler**: Script execution and API exposure
- **I/O Managers**: Input processing and output management
- **Synchronization**: VOS integration for multi-user experiences

## Documentation Standards

This documentation follows these conventions:

- **Markdown Format**: All documentation is written in Markdown for version control compatibility
- **PlantUML Diagrams**: Visual diagrams are created using PlantUML for maintainability
- **Modular Structure**: Each major component has its own documentation section
- **Example-driven**: Concepts are illustrated with practical examples
- **API Reference**: Complete API documentation with parameters and return values

## Quick Links

- [Main Repository](https://github.com/Five-Squared-Interactive/WebVerse-Runtime)
- [WebVerse Ecosystem](https://github.com/Five-Squared-Interactive/WebVerse)
- [World Engine](https://github.com/Five-Squared-Interactive/WebVerse-WorldEngine)
- [JavaScript APIs](https://five-squared-interactive.github.io/World-APIs/)
- [VEML Specification](https://github.com/Five-Squared-Interactive/VEML/wiki/Document-Structure)