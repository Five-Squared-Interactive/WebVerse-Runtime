# Example: Connection Resilience

Handle the full connection lifecycle — connect, disconnect, reconnection attempts, and failure — with user-facing feedback at every stage.

## The Script

```javascript
var socket = new SocketIO();
var myRooms = ["lobby", "game-42"];
var maxAttempts = 5;

// Register all system event listeners before connecting
socket.On("connect", "onConnect");
socket.On("disconnect", "onDisconnect");
socket.On("connect_error", "onConnectError");
socket.On("reconnect_attempt", "onReconnectAttempt");
socket.On("reconnect", "onReconnect");
socket.On("reconnect_failed", "onReconnectFailed");

socket.Connect("https://my-server.com", {
    reconnection: true,
    reconnectionAttempts: maxAttempts,
    reconnectionDelay: 1000,
    reconnectionDelayMax: 5000,
    timeout: 15000
});

// --- System Event Handlers ---

function onConnect(socketId) {
    log("[Status] Connected (ID: " + socketId + ")");
    // Join rooms on initial connection
    joinAllRooms();
}

function onDisconnect(reason) {
    if (reason === "client disconnect") {
        log("[Status] Disconnected intentionally");
    } else {
        log("[Status] Connection lost: " + reason);
        log("[Status] Auto-reconnection will start...");
    }
}

function onConnectError(errorMessage) {
    log("[Status] Connection error: " + errorMessage);
}

function onReconnectAttempt(attemptNumber) {
    log("[Status] Reconnecting... attempt " + attemptNumber + " of " + maxAttempts);
}

function onReconnect(attemptCount) {
    log("[Status] Reconnected after " + attemptCount + " attempts");
    // Re-join rooms — membership is lost on disconnect
    joinAllRooms();
}

function onReconnectFailed() {
    log("[Status] All reconnection attempts failed");
    log("[Status] Call socket.Connect() to try again manually");
}

// --- Helper Functions ---

function joinAllRooms() {
    for (var i = 0; i < myRooms.length; i++) {
        socket.JoinRoom(myRooms[i]);
        log("[Rooms] Joined: " + myRooms[i]);
    }
}
```

## How It Works

### Connection Flow

```
Connect() → [Connecting] → onConnect → [Connected]
                         → onConnectError → [Disconnected]
```

### Disconnection Flow

```
[Connected] → network lost → onDisconnect("transport close")
           → [Reconnecting] → onReconnectAttempt(1)
                             → onReconnectAttempt(2)
                             → ...
                             → onReconnect(N) → [Connected]
                             OR
                             → onReconnectFailed → [Disconnected]
```

### System Events Explained

| Event | When It Fires | What to Do |
|-------|--------------|------------|
| `connect` | Connection established | Join rooms, start sending data |
| `disconnect` | Connection lost | Show "offline" UI, wait for reconnection |
| `connect_error` | Connection attempt failed | Log error, check server URL |
| `reconnect_attempt` | Each reconnection try | Show "reconnecting" UI with attempt count |
| `reconnect` | Reconnection succeeded | Re-join rooms, resume operations |
| `reconnect_failed` | All attempts exhausted | Show "connection lost" UI, offer manual retry |

### Room Re-join Pattern

Room membership is server-side state. When the connection drops, the server forgets your room memberships. Always re-join rooms in both `onConnect` (initial connection) and `onReconnect` (after reconnection).

## Configuration

Reconnection behavior is controlled by connection options:

```javascript
socket.Connect("https://my-server.com", {
    reconnection: true,         // Enable auto-reconnect (default: true)
    reconnectionAttempts: 5,    // Max attempts before giving up (default: 5)
    reconnectionDelay: 1000,    // Initial delay in ms (default: 1000)
    reconnectionDelayMax: 5000, // Maximum delay cap in ms (default: 5000)
    timeout: 15000              // Connection timeout in ms (default: 20000)
});
```

**Exponential backoff:** Delays increase with each attempt (1000, 2000, 4000, 5000, 5000) up to the max.

**Disable reconnection:** Set `reconnection: false` to transition directly to Disconnected on connection loss.

## See Also

- [API Reference — System Events](../api-reference.md#system-events) for all event details
- [API Reference — Connection Options](../api-reference.md#connection-options) for all configuration fields
- [Error Handling Guide — Connection Errors](../error-handling.md#connection-errors-system-events) for troubleshooting
- [Room Messaging Example](room-messaging.md) for room patterns
