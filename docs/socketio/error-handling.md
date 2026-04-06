# Socket.IO Error Handling Guide

This guide covers every error and warning you may encounter when using the WebVerse Socket.IO API, with causes and resolution steps.

## How Errors Surface

Errors in WebVerse Socket.IO appear in three ways:

1. **System events** — Connection-level errors delivered to your registered callbacks via `On()`. These are the primary way to handle connection problems in your world scripts.
2. **Warning logs** — State guard violations logged to the Unity console. These indicate your code is calling methods at the wrong time (e.g., emitting when not connected).
3. **Error logs** — Internal errors logged to the Unity console. These indicate exceptions in callbacks, transport failures, or configuration problems.

---

## Connection Errors (System Events)

These errors are delivered through the event system. Register listeners to handle them in your world script.

### `connect_error`

The connection attempt failed.

**Common causes:**
- Wrong server URL (typo, wrong port)
- Server is down or unreachable
- Authentication rejected (invalid `auth` token in options)
- Connection timeout exceeded (`timeout` option, default 20 seconds)
- Network firewall blocking WebSocket/polling connections

**Resolution:**
```javascript
socket.On("connect_error", "onConnectError");

function onConnectError(errorMessage) {
    log("Connection failed: " + errorMessage);
    // Check the error message for clues:
    // - "timeout" → server may be down, increase timeout option
    // - "xhr poll error" → network issue or CORS misconfiguration
    // - "Invalid credentials" → check auth token
}
```

### `disconnect`

The connection was lost after being established.

**Reason strings and what they mean:**

| Reason | Meaning | Auto-Reconnects? |
|--------|---------|-------------------|
| `"transport close"` | Network connection dropped (WiFi lost, server crashed) | Yes (if `reconnection: true`) |
| `"ping timeout"` | Server stopped responding to heartbeats | Yes |
| `"client disconnect"` | You called `Disconnect()` | No |

**Resolution:**
```javascript
socket.On("disconnect", "onDisconnect");

function onDisconnect(reason) {
    if (reason === "client disconnect") {
        log("You disconnected intentionally");
    } else {
        log("Connection lost: " + reason);
        // Auto-reconnection will start if enabled
    }
}
```

### `reconnect_attempt`

A reconnection attempt is starting.

**Resolution:**
```javascript
socket.On("reconnect_attempt", "onReconnectAttempt");

function onReconnectAttempt(attemptNumber) {
    log("Reconnecting... attempt " + attemptNumber);
    // Show "reconnecting" UI to the user
}
```

### `reconnect`

Reconnection succeeded.

**Resolution:**
```javascript
socket.On("reconnect", "onReconnect");

function onReconnect(attemptCount) {
    log("Reconnected after " + attemptCount + " attempts");
    // IMPORTANT: Re-join rooms — room membership is lost on disconnect
    socket.JoinRoom("lobby");
}
```

### `reconnect_failed`

All reconnection attempts have been exhausted.

**Resolution:**
```javascript
socket.On("reconnect_failed", "onReconnectFailed");

function onReconnectFailed() {
    log("Could not reconnect to server");
    // Options:
    // 1. Show "connection lost" UI
    // 2. Offer a manual retry button that calls Connect() again
    // 3. Increase reconnectionAttempts in options for longer retry
}
```

---

## State Guard Warnings

These warnings appear in the Unity console when you call a method in an invalid state. They are not fatal — the call is simply ignored.

### "Transport not initialized"

**Message:** `[SocketIOClient->METHOD] Transport not initialized.`

**Cause:** The Socket.IO transport layer is not available. This happens when:
- The `USE_BESTHTTP` preprocessor symbol is not defined in your build settings
- The Best Socket.IO package is not installed

**Resolution:** Ensure `USE_BESTHTTP` is defined in your Unity project's scripting define symbols (Project Settings → Player → Scripting Define Symbols).

### "Not connected"

**Messages:**
- `[SocketIOClient->Emit] Not connected, message dropped.`
- `[SocketIOClient->EmitBinary] Not connected, binary message dropped.`
- `[SocketIOClient->EmitWithAck] Not connected, ack message dropped.`
- `[SocketIOClient->JoinRoom] Not connected.`
- `[SocketIOClient->LeaveRoom] Not connected.`
- `[SocketIOClient->Of] Not connected.`

**Cause:** You called a method that requires an active connection while the client is disconnected or connecting.

**Resolution:**
```javascript
// Option 1: Check Connected before calling
if (socket.Connected) {
    socket.Emit("chat", JSON.stringify({ text: "hello" }));
}

// Option 2: Wait for the connect event
socket.On("connect", "onReady");
function onReady(socketId) {
    socket.Emit("chat", JSON.stringify({ text: "hello" }));
}
```

**Note:** `Emit()` is the exception — during the Reconnecting state, `Emit` calls are **queued** and replayed on reconnect. All other methods (`EmitBinary`, `EmitWithAck`, `JoinRoom`, `LeaveRoom`, `Of`) are dropped with a warning.

### "Not in Connected state"

**Message:** `[SocketIOClient->Disconnect] Not in Connected state.`

**Cause:** You called `Disconnect()` while the client was in the Connecting or Reconnecting state (not fully connected yet).

**Resolution:** Wait for the `connect` event before disconnecting, or check `socket.Connected` first.

### "Already disconnected"

**Message:** `[SocketIOClient->Disconnect] Already disconnected.`

**Cause:** You called `Disconnect()` when the client was already in the Disconnected state.

**Resolution:** Check `socket.Connected` before calling `Disconnect()`, or simply ignore this warning — it's harmless.

### "Not in Disconnected state"

**Message:** `[SocketIOClient->Connect] Not in Disconnected state.`

**Cause:** You called `Connect()` while already connected, connecting, or reconnecting.

**Resolution:** Only call `Connect()` when not already connected. To reconnect, call `Disconnect()` first, then `Connect()`.

### "Instance terminated"

**Messages:**
- `[SocketIO->Emit] Instance terminated.`
- `[SocketIO:NS->Emit] Instance terminated.`

**Cause:** You are using a socket instance after `Terminate()` was called. This commonly happens when:
- The world or tab unloaded (automatic terminate)
- You manually called `Terminate()` and then tried to use the socket

**Resolution:** Create a new `SocketIO()` instance. A terminated instance cannot be reused.

### "Client not initialized" / "Socket not initialized"

**Messages:**
- `[SocketIO->Emit] Client not initialized.`
- `[SocketIO:NS->Emit] Socket not initialized.`

**Cause:** The internal SocketIOClient failed to create during construction. Check the console for the creation error.

**Resolution:** Check earlier console output for `[SocketIO] Failed to create SocketIOClient` errors. This typically indicates a Unity component setup issue.

---

## Callback & Data Errors

### "Callback error for event"

**Messages:**
- `[SocketIO] Callback error for event 'EVENT': MESSAGE`
- `[SocketIOClient] Callback error for event 'EVENT': MESSAGE`

**Cause:** Your callback function threw an exception. The exception is caught — it does not crash the Jint engine or affect other callbacks.

**Resolution:**
```javascript
// BAD — throws if data is not valid JSON
socket.On("message", "onMessage");
function onMessage(data) {
    var msg = JSON.parse(data);  // throws if data is not JSON
    log(msg.text);
}

// GOOD — handle potential parse errors
socket.On("message", "onMessageSafe");
function onMessageSafe(data) {
    try {
        var msg = JSON.parse(data);
        log(msg.text);
    } catch (e) {
        log("Failed to parse message: " + data);
    }
}
```

**Note:** The same pattern applies to `Once callback error`, `OnAny callback error`, `Binary callback error`, and `Ack callback error`. In all cases, the exception is caught and logged — other callbacks and the engine continue running.

### "Invalid base64 data, message dropped"

**Message:** `[SocketIO->EmitBinary] Invalid base64 data, message dropped.`

**Cause:** The string passed to `EmitBinary()` is not valid base64-encoded data.

**Resolution:**
```javascript
// WRONG — raw string is not base64
socket.EmitBinary("upload", "raw data");

// CORRECT — use btoa() to encode
socket.EmitBinary("upload", btoa("raw data"));
```

### "Message queue full, dropping oldest"

**Message:** `[SocketIOClient] Message queue full, dropping oldest`

**Cause:** The message queue reached its capacity (default: 100) during a reconnection period. The oldest queued message was dropped to make room for the new one.

**Resolution:**
- Increase `queueSize` in connection options: `{ queueSize: 500 }`
- Set `queueSize: 0` for unbounded queue (no limit, but uses more memory)
- Reduce emit frequency during disconnection
- Use `EmitVolatile()` for non-critical messages that don't need queuing

---

## Namespace Errors

Namespace sockets produce the same error categories as the main socket, with different prefixes.

### Error Prefixes

| Layer | Prefix Pattern | Example |
|-------|---------------|---------|
| C# Client (NamespacedSocket) | `[NamespacedSocket->/PATH->METHOD]` | `[NamespacedSocket->/chat->Emit] Not connected.` |
| JS Wrapper (NamespacedSocketIO) | `[SocketIO:NS->METHOD]` | `[SocketIO:NS->Emit] Instance terminated.` |

### "Transport returned null for namespace"

**Message:** `[SocketIOClient->Of] Transport returned null for namespace: /PATH`

**Cause:** The transport layer failed to create a socket for the requested namespace. This may indicate the server doesn't support the namespace.

**Resolution:** Verify the namespace exists on your server configuration. Ensure the server's Socket.IO setup includes the namespace path.

### Same Patterns Apply

Namespace sockets produce the same warnings as the main socket:
- "Not connected" — namespace parent is disconnected
- "Transport not initialized" — namespace transport unavailable
- "Instance terminated" — namespace wrapper used after parent terminated
- Callback errors — same catch-and-log behavior

---

## Best Practices

### Always Listen for System Events

At minimum, register handlers for connection lifecycle events:

```javascript
var socket = new SocketIO();

socket.On("connect", "onConnect");
socket.On("disconnect", "onDisconnect");
socket.On("connect_error", "onConnectError");
socket.On("reconnect_failed", "onReconnectFailed");

function onConnect(id) { log("Connected: " + id); }
function onDisconnect(reason) { log("Disconnected: " + reason); }
function onConnectError(err) { log("Error: " + err); }
function onReconnectFailed() { log("Reconnection failed"); }

socket.Connect("https://my-server.com");
```

### Re-Join Rooms After Reconnect

Room membership is server-side state and is lost on disconnect. Always re-join rooms after reconnection:

```javascript
socket.On("reconnect", "onReconnect");

function onReconnect(attemptCount) {
    socket.JoinRoom("lobby");
    socket.JoinRoom("game-42");
}
```

### Callbacks Are Safe

If your callback throws an exception, the Socket.IO engine catches it and logs the error. Other callbacks continue to fire, and the Jint engine is not affected. However, you should still handle errors in your callbacks to ensure your world script logic stays consistent.

### Check Connected Before Expensive Operations

To avoid unnecessary warning logs, check the connection state before calling methods:

```javascript
function sendUpdate(data) {
    if (socket.Connected) {
        socket.EmitWithAck("update", JSON.stringify(data), "onUpdateAck");
    }
}
```

This is optional for `Emit()` (which queues during reconnection) and `EmitVolatile()` (which silently drops), but recommended for `EmitBinary()`, `EmitWithAck()`, `JoinRoom()`, `LeaveRoom()`, and `Of()`.
