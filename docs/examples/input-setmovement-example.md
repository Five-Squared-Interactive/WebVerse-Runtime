# SetMovement API Example

This document provides practical examples of how to use the `Input.SetMovement()` API to programmatically control character or camera movement in WebVerse-Runtime.

## Overview

The `SetMovement(Vector3 amount)` method allows JavaScript code to directly set movement input values, enabling automated movement, AI-controlled characters, and scripted sequences.

**Key Points:**
- Accepts a `Vector3` parameter (x, y, z)
- Uses **x** and **z** components for horizontal plane movement
- The **y** component is ignored
- Maps to the underlying Vector2 movement system (x, z) → (x, y)

## Example 1: Simple Forward Movement

```javascript
// Move forward at normal speed
var forwardMovement = new Vector3(0, 0, 1);
Input.SetMovement(forwardMovement);

// After some time, stop movement
setTimeout(function() {
    Input.SetMovement(new Vector3(0, 0, 0));
}, 5000); // Stop after 5 seconds
```

## Example 2: Diagonal Movement

```javascript
// Move forward and to the right
var diagonalMovement = new Vector3(0.7, 0, 0.7); // Normalized diagonal
Input.SetMovement(diagonalMovement);
```

## Example 3: Circular Movement Pattern

```javascript
// Create a circular movement pattern
var angle = 0;
var radius = 1.0;

function updateCircularMovement() {
    angle += 0.05; // Rotation speed
    
    var x = Math.cos(angle) * radius;
    var z = Math.sin(angle) * radius;
    
    Input.SetMovement(new Vector3(x, 0, z));
    
    // Continue the pattern
    setTimeout(updateCircularMovement, 16); // ~60 FPS
}

// Start the circular movement
updateCircularMovement();
```

## Example 4: Patrol Route

```javascript
// Define waypoints
var waypoints = [
    new Vector3(0, 0, 1),   // Forward
    new Vector3(1, 0, 0),   // Right
    new Vector3(0, 0, -1),  // Backward
    new Vector3(-1, 0, 0)   // Left
];

var currentWaypoint = 0;
var waypointDuration = 2000; // 2 seconds per waypoint

function patrolMovement() {
    // Set movement toward current waypoint
    Input.SetMovement(waypoints[currentWaypoint]);
    
    // Move to next waypoint after duration
    setTimeout(function() {
        currentWaypoint = (currentWaypoint + 1) % waypoints.length;
        patrolMovement();
    }, waypointDuration);
}

// Start patrol
patrolMovement();
```

## Example 5: Smooth Movement Transition

```javascript
// Smoothly interpolate between movement states
var currentMovement = new Vector3(0, 0, 0);
var targetMovement = new Vector3(0, 0, 1);
var lerpSpeed = 0.1;

function smoothMovement() {
    // Lerp toward target
    currentMovement.x += (targetMovement.x - currentMovement.x) * lerpSpeed;
    currentMovement.z += (targetMovement.z - currentMovement.z) * lerpSpeed;
    
    Input.SetMovement(currentMovement);
    
    // Continue smoothing
    setTimeout(smoothMovement, 16); // ~60 FPS
}

// Change target movement direction
function setTarget(x, z) {
    targetMovement = new Vector3(x, 0, z);
}

// Start smooth movement
smoothMovement();

// Example: Change direction after 3 seconds
setTimeout(function() {
    setTarget(1, 0); // Move right
}, 3000);
```

## Example 6: Reading and Modifying Current Movement

```javascript
// Get current movement value
var current = Input.GetMoveValue();
Logging.Log("Current X: " + current.x + ", Y: " + current.y);

// Modify current movement (reverse direction)
var reversed = new Vector3(-current.x, 0, -current.y);
Input.SetMovement(reversed);
```

## Example 7: Stop All Movement

```javascript
// Immediately stop all movement
Input.SetMovement(new Vector3(0, 0, 0));
```

## Example 8: Integration with Input Events

```javascript
// Combine manual and automated movement
var autoMoveEnabled = false;

// Manual input
Input.onKeyDown = function(key) {
    if (key === "F") {
        autoMoveEnabled = !autoMoveEnabled;
        Logging.Log("Auto-move: " + autoMoveEnabled);
    }
};

// Automated movement loop
function autoMoveLoop() {
    if (autoMoveEnabled) {
        Input.SetMovement(new Vector3(0, 0, 1)); // Move forward
    }
    setTimeout(autoMoveLoop, 16);
}

autoMoveLoop();
```

## Best Practices

1. **Normalization**: For consistent speed, normalize your movement vectors:
   ```javascript
   var movement = new Vector3(1, 0, 1);
   var length = Math.sqrt(movement.x * movement.x + movement.z * movement.z);
   if (length > 0) {
       movement.x /= length;
       movement.z /= length;
   }
   Input.SetMovement(movement);
   ```

2. **Cleanup**: Always reset movement when disabling automated control:
   ```javascript
   function stopAutomation() {
       Input.SetMovement(new Vector3(0, 0, 0));
   }
   ```

3. **Frame Rate Independence**: Use delta time for smooth movement regardless of frame rate:
   ```javascript
   var lastTime = Date.now();
   
   function update() {
       var currentTime = Date.now();
       var deltaTime = (currentTime - lastTime) / 1000.0;
       lastTime = currentTime;
       
       // Calculate movement based on deltaTime
       var speed = 2.0; // units per second
       var movement = new Vector3(0, 0, speed * deltaTime);
       Input.SetMovement(movement);
       
       setTimeout(update, 16);
   }
   ```

## Common Use Cases

- **Cutscenes**: Automate character movement during story sequences
- **AI NPCs**: Control non-player character movement
- **Tutorials**: Guide players through interactive demonstrations
- **Testing**: Automate movement for testing game mechanics
- **Accessibility**: Provide alternative input methods for players
- **Replay Systems**: Recreate recorded movement patterns

## Troubleshooting

**Movement not working?**
- Ensure `wasdMotionEnabled` is set appropriately for your input mode
- Check that the InputManager is properly initialized
- Verify Vector3 values are within expected range (-1 to 1 for normal speed)

**Jittery movement?**
- Use consistent timing (e.g., `setTimeout` with fixed intervals)
- Consider smoothing/interpolation techniques
- Normalize movement vectors for consistent speed
