# Socket.IO Quick-Start Guide

Get started with real-time messaging in your WebVerse world scripts.

## Connect, Emit, and Listen

```javascript
var socket = new SocketIO();
socket.Connect("https://my-server.com");
socket.On("connect", "onConnected");
socket.On("message", "onMessage");

function onConnected(socketId) { socket.Emit("chat", '{"text":"Hello!"}'); }
function onMessage(data) { log("Received: " + data); }
```

That's 7 lines — you're connected and exchanging messages.

## Connection with Options

Pass an options object to configure authentication, transport, and reconnection behavior.

```javascript
var socket = new SocketIO();

socket.Connect("https://my-server.com", {
    auth: "my-bearer-token",
    headers: { "X-Custom-Header": "value" },
    query: { "room": "lobby", "user": "player1" },
    transport: "websocket",
    reconnection: true,
    reconnectionAttempts: 10,
    timeout: 15000
});
```

See [Connection Options](api-reference.md#connection-options) in the API Reference for all available fields and defaults.

## Listening for Events

### Persistent Listener

```javascript
// Fires every time "chat" event is received
socket.On("chat", "onChat");

function onChat(data) {
    log("Chat: " + data);
}
```

### One-Time Listener

```javascript
// Fires once, then auto-removes
socket.Once("welcome", "onWelcome");

function onWelcome(data) {
    log("Welcome message: " + data);
}
```

### Catch-All Listener

```javascript
// Fires for every event — useful for debugging
socket.OnAny("onAnyEvent");

function onAnyEvent(eventName, data) {
    log("[" + eventName + "] " + data);
}
```

### Removing Listeners

```javascript
// Remove all listeners for a specific event
socket.Off("chat");

// Remove all catch-all listeners
socket.OffAny();
```

## Sending Messages

### Standard Emit

```javascript
socket.Emit("chat", JSON.stringify({ text: "Hello world" }));
```

### Emit with Acknowledgement

```javascript
// Server confirms receipt by calling your callback
socket.EmitWithAck("save", JSON.stringify({ id: 1 }), "onSaveAck");

function onSaveAck(response) {
    log("Server acknowledged: " + response);
}
```

### Volatile Emit (Fire-and-Forget)

```javascript
// Silently dropped if not connected — no warning, no queuing
socket.EmitVolatile("cursor", JSON.stringify({ x: 100, y: 200 }));
```

### Binary Emit

```javascript
// Send binary data as base64-encoded string
var payload = "raw binary content";
socket.EmitBinary("upload", btoa(payload));
```

## Rooms

Request the server to join or leave named rooms for scoped messaging.

```javascript
socket.JoinRoom("lobby");
socket.LeaveRoom("lobby");
```

**Note:** Room membership is server-side state. After a reconnection, you must re-join rooms manually (see [Connection Resilience](#connection-resilience) below).

## Namespaces

Multiplex independent communication channels on a single connection.

```javascript
var chat = socket.Of("/chat");
var game = socket.Of("/game");

// Each namespace has its own listeners — fully isolated
chat.On("message", "onChatMessage");
game.On("message", "onGameMessage");

chat.Emit("message", JSON.stringify({ text: "Hello chat!" }));
game.Emit("move", JSON.stringify({ x: 10, y: 20 }));
```

Namespace wrappers support the full API: `Emit`, `On`, `Off`, `Once`, `OnAny`, `OffAny`, `EmitBinary`, `EmitWithAck`, `EmitVolatile`, `JoinRoom`, `LeaveRoom`, `OnBinary`, `OffBinary`.

## Connection Resilience

Socket.IO auto-reconnects by default. Listen for system events to track connection state.

```javascript
socket.On("disconnect", "onDisconnect");
socket.On("reconnect_attempt", "onReconnectAttempt");
socket.On("reconnect", "onReconnect");
socket.On("reconnect_failed", "onReconnectFailed");

function onDisconnect(reason) {
    log("Disconnected: " + reason);
}

function onReconnectAttempt(attemptNumber) {
    log("Reconnecting... attempt " + attemptNumber);
}

function onReconnect(attemptCount) {
    log("Reconnected after " + attemptCount + " attempts");
    // Re-join rooms after reconnect
    socket.JoinRoom("lobby");
}

function onReconnectFailed() {
    log("All reconnection attempts failed");
}
```

**Message queuing:** Standard `Emit` calls are automatically queued during disconnection and replayed in order when reconnected. `EmitBinary`, `EmitWithAck`, `EmitVolatile`, `JoinRoom`, and `LeaveRoom` are NOT queued.

## Disconnecting

```javascript
// Graceful disconnect
socket.Disconnect();
```

When a world or tab unloads, all Socket.IO connections are automatically disconnected — no cleanup code needed.

## What's Next

- **[API Reference](api-reference.md)** — Complete documentation for every method, property, option, and event
- **[Error Handling Guide](error-handling.md)** — Troubleshooting connection errors, callback failures, and disconnect reasons
- **[Migration Guide](migration-guide.md)** — Mapping standard Socket.IO patterns to WebVerse equivalents
- **[Example World Scripts](examples/index.md)** — Working examples for chat, rooms, binary transfer, and resilient connections
