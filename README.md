# WebVerse-Runtime

WebVerse Unity Runtime.

## Description

This is the Unity3D runtime for WebVerse.

* For the top-level WebVerse application (NodeJS/Electron project), see [WebVerse](https://github.com/Five-Squared-Interactive/WebVerse)
* For the WebVerse World Engine (Unity3D project), see [WebVerse World Engine](https://github.com/Five-Squared-Interactive/WebVerse-WorldEngine)
* For WebVerse Samples, see [WebVerse Samples](https://github.com/Five-Squared-Interactive/WebVerse-Samples)
* For the Virtual Environment Markup Language, see [VEML](https://github.com/Five-Squared-Interactive/VEML/wiki/Document-Structure)
* For the WebVerse JavaScript World APIs, see [World APIs](https://five-squared-interactive.github.io/World-APIs/)
* For Virtual reality Operating System (VOS) Synchronization Service (VSS), see [VSS](https://github.com/Five-Squared-Interactive/VOS-Synchronization)

## Architecture

![WebVerse-Runtime-Architecture](https://github.com/Five-Squared-Interactive/WebVerse-Runtime/assets/16926525/3877dfec-e541-4475-9bcf-7d07a7421ce1)

## Developing

### Development Prerequisites

* Unity 2021.3.26 with Universal Render Pipeline

#### Unity Asset Store Packages

WebVerse is free and open source, however, it does leverage some paid Unity Asset Store packages. Therefore, we are not able to distribute these. However, you can obtain these packages at the links provided below and import them into your cloned Unity project.

* Best HTTP v3.0.4: https://assetstore.unity.com/packages/tools/network/best-http-267636
* Best MQTT v3.0.2: https://assetstore.unity.com/packages/tools/network/best-mqtt-268762
* Best WebSockets v3.0.1: https://assetstore.unity.com/packages/tools/network/best-websockets-268757

### Setup

1. Clone the repository and navigate to that directory:
   ```
   git clone https://github.com/Five-Squared-Interactive/WebVerse-Runtime.git
   cd WebVerse-Runtime
   ```

2. Open the project in Unity. The project does not need to be opened in Safe Mode. You will see multiple errors in the Unity UI until step 3 is performed.

3. From the Unity Package Manager, import all required Asset Store Packages listed above.

### Running and Building

#### Running WebVerse
1. Navigate to a WebVerse scene in Unity. These are located at `Runtime/TopLevel/Scenes/`.
2. Once the scene is open, select the "WebVerse" gameobject from the Scene Hierarchy pane. Populate the test parameters in the "Focused Mode" or "Lightweight Mode" script from the Inspector pane.
3. Run the scene from the Unity Editor using the Play button.

#### Running Unit Tests
The [Unit Test Runner](https://docs.unity3d.com/2021.3/Documentation/Manual/testing-editortestsrunner.html) can be used to run WebVerse's unit tests.

#### Building WebVerse
The WebVerse Runtime can be built for Windows or WebGL.

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Contact

Email - fivesquaredtechnologies@gmail.com
