# Basic Usage Examples

This document provides practical examples of how to use WebVerse-Runtime for common scenarios, from simple content loading to interactive experiences.

## Example 1: Loading and Displaying VEML Content

### VEML Document Example

First, let's create a simple VEML document that defines a basic scene:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<veml xmlns="http://www.fivesqd.com/schemas/veml/3.0" version="3.0">
    <metadata>
        <title>Basic Scene Example</title>
        <description>A simple scene with ground, cubes, and lighting</description>
        <author>WebVerse Tutorial</author>
    </metadata>
    
    <environment>
        <!-- Sky and lighting -->
        <background>
            <color>lightblue</color>
        </background>
        
        <lighting>
            <ambient-color>white</ambient-color>
            <ambient-intensity>0.3</ambient-intensity>
        </lighting>
        
        <!-- Ground plane -->
        <entity id="ground" type="plane">
            <transform>
                <position x="0" y="0" z="0"/>
                <rotation x="0" y="0" z="0"/>
                <scale x="20" y="1" z="20"/>
            </transform>
            <color>darkgreen</color>
            <physics>
                <collider type="box"/>
                <static>true</static>
            </physics>
        </entity>
        
        <!-- Colored cubes -->
        <entity id="redCube" type="cube">
            <transform>
                <position x="-3" y="1" z="0"/>
            </transform>
            <color>red</color>
            <physics>
                <collider type="box"/>
                <rigidbody>
                    <mass>1.0</mass>
                </rigidbody>
            </physics>
        </entity>
        
        <entity id="blueCube" type="cube">
            <transform>
                <position x="0" y="1" z="0"/>
            </transform>
            <color>blue</color>
            <physics>
                <collider type="box"/>
                <rigidbody>
                    <mass>1.0</mass>
                </rigidbody>
            </physics>
        </entity>
        
        <entity id="greenCube" type="cube">
            <transform>
                <position x="3" y="1" z="0"/>
            </transform>
            <color>green</color>
            <physics>
                <collider type="box"/>
                <rigidbody>
                    <mass>1.0</mass>
                </rigidbody>
            </physics>
        </entity>
        
        <!-- Directional light (sun) -->
        <entity id="sunLight" type="light">
            <transform>
                <position x="0" y="10" z="0"/>
                <rotation x="50" y="-30" z="0"/>
            </transform>
            <light>
                <type>directional</type>
                <color>white</color>
                <intensity>1.5</intensity>
                <cast-shadows>true</cast-shadows>
            </light>
        </entity>
    </environment>
</veml>
```

### JavaScript Loading Code

```javascript
// Main initialization function
function initializeScene() {
    Logging.Log("Starting scene initialization...");
    
    // Load the VEML document
    VEML.loadDocument("scenes/basic-scene.veml", function(result) {
        if (result.success) {
            Logging.Log("VEML scene loaded successfully!");
            onSceneLoaded();
        } else {
            Logging.LogError("Failed to load VEML scene: " + result.error);
        }
    });
}

// Called when scene loading is complete
function onSceneLoaded() {
    Logging.Log("Scene is ready for interaction");
    
    // Set up camera position
    setupCamera();
    
    // Enable user interaction
    setupInteraction();
    
    // Start any animations or updates
    startSceneUpdates();
}

function setupCamera() {
    // Position camera to view the scene
    var cameraPosition = new Vector3(0, 5, 10);
    var lookAtPosition = new Vector3(0, 1, 0);
    
    Camera.setPosition(cameraPosition);
    Camera.lookAt(lookAtPosition);
    
    Logging.Log("Camera positioned at: " + cameraPosition);
}
```

## Example 2: Interactive Object Manipulation

### Click-to-Select System

```javascript
// Global variables
var selectedObject = null;
var originalColor = null;

// Set up mouse interaction
function setupInteraction() {
    Input.onMouseDown = function(button, screenPosition) {
        if (button === 0) { // Left mouse button
            handleMouseClick(screenPosition);
        }
    };
    
    Input.onKeyDown = function(key) {
        handleKeyPress(key);
    };
}

function handleMouseClick(screenPosition) {
    // Convert screen position to world ray
    var ray = Camera.screenPointToRay(screenPosition);
    
    // Perform raycast
    var hit = Physics.raycast(ray, 100); // Max distance 100 units
    
    if (hit.collider) {
        var clickedEntity = hit.collider.entity;
        selectObject(clickedEntity);
        
        Logging.Log("Clicked on: " + clickedEntity.name + 
                   " at position: " + hit.point);
    } else {
        // Clicked on empty space - deselect
        deselectObject();
    }
}

function selectObject(entity) {
    // Deselect previous object
    deselectObject();
    
    // Select new object
    selectedObject = entity;
    originalColor = entity.getProperty("color") || Color.white;
    
    // Highlight selected object
    entity.setProperty("color", Color.yellow);
    
    Logging.Log("Selected object: " + entity.name);
}

function deselectObject() {
    if (selectedObject) {
        // Restore original color
        selectedObject.setProperty("color", originalColor);
        
        Logging.Log("Deselected object: " + selectedObject.name);
        
        selectedObject = null;
        originalColor = null;
    }
}
```

### Object Manipulation

```javascript
function handleKeyPress(key) {
    if (!selectedObject) return;
    
    var moveDistance = 0.5;
    var currentPos = selectedObject.position;
    
    switch(key) {
        case "W":
            selectedObject.position = Vector3.Add(currentPos, 
                Vector3.Scale(Vector3.forward, moveDistance));
            break;
            
        case "S":
            selectedObject.position = Vector3.Add(currentPos, 
                Vector3.Scale(Vector3.back, moveDistance));
            break;
            
        case "A":
            selectedObject.position = Vector3.Add(currentPos, 
                Vector3.Scale(Vector3.left, moveDistance));
            break;
            
        case "D":
            selectedObject.position = Vector3.Add(currentPos, 
                Vector3.Scale(Vector3.right, moveDistance));
            break;
            
        case "Q":
            selectedObject.position = Vector3.Add(currentPos, 
                Vector3.Scale(Vector3.up, moveDistance));
            break;
            
        case "E":
            selectedObject.position = Vector3.Add(currentPos, 
                Vector3.Scale(Vector3.down, moveDistance));
            break;
            
        case "R":
            // Rotate object
            var currentRotation = selectedObject.rotation;
            selectedObject.rotation = Quaternion.Multiply(currentRotation, 
                Quaternion.Euler(0, 15, 0));
            break;
            
        case "Delete":
            // Delete selected object
            selectedObject.destroy();
            selectedObject = null;
            Logging.Log("Object deleted");
            break;
    }
}
```

## Example 3: Dynamic Content Creation

### Procedural Scene Generation

```javascript
function createProceduralScene() {
    Logging.Log("Creating procedural scene...");
    
    // Create ground
    createGround();
    
    // Create building structures
    createBuildings(5, 10); // 5 buildings, max height 10
    
    // Create trees
    createTrees(20); // 20 trees
    
    // Create lighting
    createLighting();
    
    Logging.Log("Procedural scene creation complete");
}

function createGround() {
    var ground = Entity.create("ProceduralGround");
    ground.addComponent("MeshRenderer");
    ground.addComponent("BoxCollider");
    
    // Set properties
    ground.position = Vector3.zero;
    ground.localScale = new Vector3(50, 1, 50);
    
    // Create ground material
    var groundMaterial = {
        color: new Color(0.2, 0.6, 0.2, 1.0), // Dark green
        metallic: 0.0,
        roughness: 0.8
    };
    ground.setMaterial(groundMaterial);
    
    return ground;
}

function createBuildings(count, maxHeight) {
    for (var i = 0; i < count; i++) {
        var building = createRandomBuilding(maxHeight);
        
        // Random position on ground
        var x = Math.randomRange(-20, 20);
        var z = Math.randomRange(-20, 20);
        var height = Math.randomRange(2, maxHeight);
        
        building.position = new Vector3(x, height * 0.5, z);
        building.localScale = new Vector3(
            Math.randomRange(1, 3),  // Width
            height,                  // Height
            Math.randomRange(1, 3)   // Depth
        );
    }
}

function createRandomBuilding(maxHeight) {
    var building = Entity.create("Building_" + Math.randomInt(1000, 9999));
    building.addComponent("MeshRenderer");
    building.addComponent("BoxCollider");
    
    // Random building color
    var buildingColor = new Color(
        Math.randomRange(0.3, 0.8),
        Math.randomRange(0.3, 0.8),
        Math.randomRange(0.3, 0.8),
        1.0
    );
    
    var buildingMaterial = {
        color: buildingColor,
        metallic: 0.1,
        roughness: 0.7
    };
    building.setMaterial(buildingMaterial);
    
    return building;
}

function createTrees(count) {
    for (var i = 0; i < count; i++) {
        createRandomTree();
    }
}

function createRandomTree() {
    // Create tree trunk
    var trunk = Entity.create("TreeTrunk_" + Math.randomInt(1000, 9999));
    trunk.addComponent("MeshRenderer");
    trunk.addComponent("CapsuleCollider");
    
    // Position randomly
    var x = Math.randomRange(-25, 25);
    var z = Math.randomRange(-25, 25);
    trunk.position = new Vector3(x, 2, z);
    trunk.localScale = new Vector3(0.3, 2, 0.3);
    
    // Brown trunk material
    var trunkMaterial = {
        color: new Color(0.4, 0.2, 0.1, 1.0),
        metallic: 0.0,
        roughness: 1.0
    };
    trunk.setMaterial(trunkMaterial);
    
    // Create tree foliage
    var foliage = Entity.create("TreeFoliage", trunk);
    foliage.addComponent("MeshRenderer");
    foliage.localPosition = new Vector3(0, 1.5, 0);
    foliage.localScale = new Vector3(2, 2, 2);
    
    // Green foliage material
    var foliageMaterial = {
        color: new Color(0.1, 0.6, 0.1, 1.0),
        metallic: 0.0,
        roughness: 0.9
    };
    foliage.setMaterial(foliageMaterial);
}

function createLighting() {
    // Create sun light
    var sun = LightEntity.create("ProceduralSun");
    sun.setType("Directional");
    sun.setColor(new Color(1, 0.95, 0.8, 1));
    sun.setIntensity(1.2);
    sun.rotation = Quaternion.Euler(50, -30, 0);
    
    // Create ambient lighting
    Environment.setAmbientColor(new Color(0.4, 0.4, 0.6, 1));
    Environment.setAmbientIntensity(0.3);
    
    // Add some point lights for atmosphere
    for (var i = 0; i < 3; i++) {
        var streetLight = LightEntity.create("StreetLight_" + i);
        streetLight.setType("Point");
        streetLight.setColor(new Color(1, 0.8, 0.6, 1));
        streetLight.setIntensity(2.0);
        streetLight.setRange(8.0);
        
        streetLight.position = new Vector3(
            Math.randomRange(-15, 15),
            4,
            Math.randomRange(-15, 15)
        );
    }
}
```

## Example 4: Animation and Movement

### Smooth Object Animation

```javascript
// Animation system
var animatedObjects = [];

function startSceneUpdates() {
    // Start the main update loop
    Time.setInterval(updateScene, 16); // ~60 FPS
}

function updateScene() {
    updateAnimations();
    updateRotatingObjects();
    updateFloatingObjects();
}

function updateAnimations() {
    var currentTime = Time.time;
    
    for (var i = 0; i < animatedObjects.length; i++) {
        var obj = animatedObjects[i];
        updateObjectAnimation(obj, currentTime);
    }
}

function addFloatingAnimation(entity, amplitude, speed) {
    var animData = {
        entity: entity,
        type: "floating",
        originalY: entity.position.y,
        amplitude: amplitude || 1.0,
        speed: speed || 1.0,
        startTime: Time.time
    };
    
    animatedObjects.push(animData);
}

function addRotatingAnimation(entity, axis, speed) {
    var animData = {
        entity: entity,
        type: "rotating",
        axis: axis || Vector3.up,
        speed: speed || 45.0, // degrees per second
        startTime: Time.time
    };
    
    animatedObjects.push(animData);
}

function updateObjectAnimation(animData, currentTime) {
    var elapsed = currentTime - animData.startTime;
    
    switch(animData.type) {
        case "floating":
            updateFloatingAnimation(animData, elapsed);
            break;
            
        case "rotating":
            updateRotatingAnimation(animData, elapsed);
            break;
    }
}

function updateFloatingAnimation(animData, elapsed) {
    var offset = Math.sin(elapsed * animData.speed) * animData.amplitude;
    var newY = animData.originalY + offset;
    
    var currentPos = animData.entity.position;
    animData.entity.position = new Vector3(currentPos.x, newY, currentPos.z);
}

function updateRotatingAnimation(animData, elapsed) {
    var rotationAmount = animData.speed * elapsed;
    var rotation = Quaternion.Euler(
        animData.axis.x * rotationAmount,
        animData.axis.y * rotationAmount,
        animData.axis.z * rotationAmount
    );
    
    animData.entity.rotation = rotation;
}

// Example usage
function addAnimationsToScene() {
    // Find cubes and add animations
    var redCube = Entity.findByName("redCube");
    if (redCube) {
        addFloatingAnimation(redCube, 0.5, 2.0); // Gentle floating
    }
    
    var blueCube = Entity.findByName("blueCube");
    if (blueCube) {
        addRotatingAnimation(blueCube, Vector3.up, 30); // Slow rotation
    }
    
    var greenCube = Entity.findByName("greenCube");
    if (greenCube) {
        addFloatingAnimation(greenCube, 0.3, 3.0);
        addRotatingAnimation(greenCube, Vector3.up, 60); // Both animations
    }
}
```

## Example 5: Data Persistence and Loading

### Save/Load System

```javascript
// Save system for persistent data
var saveData = {
    playerPosition: Vector3.zero,
    objectStates: {},
    sceneSettings: {}
};

function saveSceneState() {
    Logging.Log("Saving scene state...");
    
    // Save camera position
    saveData.playerPosition = Camera.getPosition();
    
    // Save object states
    saveData.objectStates = {};
    var allEntities = Entity.findAll();
    
    for (var i = 0; i < allEntities.length; i++) {
        var entity = allEntities[i];
        if (entity.hasProperty("persistent")) {
            saveData.objectStates[entity.id] = {
                position: entity.position,
                rotation: entity.rotation,
                scale: entity.localScale,
                color: entity.getProperty("color"),
                active: entity.active
            };
        }
    }
    
    // Save scene settings
    saveData.sceneSettings = {
        ambientColor: Environment.getAmbientColor(),
        ambientIntensity: Environment.getAmbientIntensity(),
        fogEnabled: Environment.getFogEnabled(),
        timeOfDay: getTimeOfDay()
    };
    
    // Store in local storage
    var saveString = JSON.stringify(saveData);
    LocalStorage.setItem("sceneState", saveString);
    
    Logging.Log("Scene state saved successfully");
}

function loadSceneState() {
    Logging.Log("Loading scene state...");
    
    var saveString = LocalStorage.getItem("sceneState");
    if (!saveString) {
        Logging.LogWarning("No saved scene state found");
        return false;
    }
    
    try {
        saveData = JSON.parse(saveString);
        
        // Restore camera position
        Camera.setPosition(saveData.playerPosition);
        
        // Restore object states
        for (var entityId in saveData.objectStates) {
            var entity = Entity.findById(entityId);
            if (entity) {
                var state = saveData.objectStates[entityId];
                entity.position = state.position;
                entity.rotation = state.rotation;
                entity.localScale = state.scale;
                entity.setProperty("color", state.color);
                entity.setActive(state.active);
            }
        }
        
        // Restore scene settings
        if (saveData.sceneSettings) {
            Environment.setAmbientColor(saveData.sceneSettings.ambientColor);
            Environment.setAmbientIntensity(saveData.sceneSettings.ambientIntensity);
            Environment.setFogEnabled(saveData.sceneSettings.fogEnabled);
            setTimeOfDay(saveData.sceneSettings.timeOfDay);
        }
        
        Logging.Log("Scene state loaded successfully");
        return true;
        
    } catch (error) {
        Logging.LogError("Failed to load scene state: " + error);
        return false;
    }
}

// Auto-save functionality
function enableAutoSave(intervalSeconds) {
    Time.setInterval(function() {
        saveSceneState();
    }, intervalSeconds * 1000);
    
    Logging.Log("Auto-save enabled (interval: " + intervalSeconds + "s)");
}

// Key bindings for save/load
function setupSaveLoadKeys() {
    Input.onKeyDown = function(key) {
        if (Input.isKeyPressed("LeftControl") || Input.isKeyPressed("RightControl")) {
            switch(key) {
                case "S":
                    saveSceneState();
                    break;
                case "L":
                    loadSceneState();
                    break;
            }
        }
    };
}
```

## Running the Examples

### Initialize Everything

```javascript
// Main initialization function that ties everything together
function main() {
    Logging.Log("=== WebVerse Basic Usage Examples ===");
    
    // Wait for WebVerse to be ready
    if (WebVerseRuntime.isInitialized) {
        startExamples();
    } else {
        WebVerseRuntime.onInitialized = function() {
            startExamples();
        };
    }
}

function startExamples() {
    // Initialize the scene
    initializeScene();
    
    // Set up save/load system
    setupSaveLoadKeys();
    enableAutoSave(30); // Auto-save every 30 seconds
    
    // Try to load previous state
    var loaded = loadSceneState();
    if (!loaded) {
        // If no saved state, create procedural content
        Time.setTimeout(function() {
            createProceduralScene();
            addAnimationsToScene();
        }, 2000); // Wait 2 seconds for VEML to load
    }
    
    Logging.Log("All examples initialized successfully!");
}

// Start the examples
main();
```

These examples demonstrate the core capabilities of WebVerse-Runtime and provide a foundation for building more complex interactive experiences. Each example can be used independently or combined to create rich virtual environments.