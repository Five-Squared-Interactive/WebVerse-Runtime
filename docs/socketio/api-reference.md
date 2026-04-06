# Socket.IO API Reference

Complete reference for the WebVerse Socket.IO JavaScript API. All methods are available on instances created with `new SocketIO()`.

---

## Constructor

### `new SocketIO()`

Creates a new Socket.IO client instance. The client is not connected until `Connect()` is called.

```javascript
var socket = new SocketIO();
```

---

## Connection

### `Connect(url, options?)`

Connect to a Socket.IO server.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `url` | string | Yes | Server URL (e.g., `"https://my-server.com"`) |
| `options` | object | No | Connection options (see [Connection Options](#connection-options)) |

```javascript
socket.Connect("https://my-server.com");

// With options
socket.Connect("https://my-server.com", {
    auth: "bearer-token",
    transport: "websocket",
    reconnection: true
});
```

**Behavior:**
- Transitions state from Disconnected to Connecting
- Fires `connect` event on success with the socket ID
- Fires `connect_error` event on failure with the error message
- Calling Connect while already connected or connecting is ignored with a warning

### `Disconnect()`

Gracefully disconnect from the server.

```javascript
socket.Disconnect();
```

**Behavior:**
- Cancels any in-progress reconnection
- Fires `disconnect` event with reason `"client disconnect"`
- Calling Disconnect while already disconnected is ignored with a warning

### `Terminate()`

Release all resources. Called automatically when the world or tab unloads.

```javascript
socket.Terminate();
```

**Behavior:**
- Disconnects the active connection
- Clears all event callbacks
- Disposes all namespace instances
- Discards the message queue
- After Terminate, all method calls are silently rejected

---

## Properties

### `Connected`

Whether the client is currently connected to the server.

| Type | Access |
|------|--------|
| boolean | Read-only |

```javascript
if (socket.Connected) {
    socket.Emit("ping", "{}");
}
```

### `Id`

The server-assigned socket ID. Available only after the `connect` event fires.

| Type | Access |
|------|--------|
| string | Read-only |

```javascript
socket.On("connect", "onConnect");

function onConnect(socketId) {
    log("My socket ID: " + socket.Id);  // same as socketId parameter
}
```

---

## Event Emission

### `Emit(eventName, data)`

Send a named event with JSON data to the server.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `eventName` | string | Yes | The event name |
| `data` | string | Yes | JSON-encoded data |

```javascript
socket.Emit("chat", JSON.stringify({ text: "Hello!", user: "Alice" }));
```

**Behavior:**
- When connected: sends immediately via transport
- When disconnected/reconnecting: **queued** and replayed in order on reconnect
- Null or empty `eventName`: silently ignored

### `EmitBinary(eventName, base64Data)`

Send a named event with binary data (base64-encoded) to the server.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `eventName` | string | Yes | The event name |
| `base64Data` | string | Yes | Base64-encoded binary data |

```javascript
socket.EmitBinary("upload", btoa(rawBinaryString));
```

**Behavior:**
- When connected: sends immediately
- When disconnected: **NOT queued** — warning logged, data dropped
- Null or empty `eventName`: silently ignored

### `EmitWithAck(eventName, data, callbackFunctionName)`

Send a named event and receive a server acknowledgement via callback.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `eventName` | string | Yes | The event name |
| `data` | string | Yes | JSON-encoded data |
| `callbackFunctionName` | string | Yes | Name of the JS function to call when server acknowledges |

```javascript
socket.EmitWithAck("save", JSON.stringify({ id: 42 }), "onSaved");

function onSaved(response) {
    log("Server response: " + response);
}
```

**Behavior:**
- When connected: sends immediately, callback invoked when server acknowledges
- When disconnected: **NOT queued** — warning logged, dropped
- If ack timeout expires (configurable via `ackTimeout` option), callback receives a timeout error
- Null or empty `eventName`: silently ignored

### `EmitVolatile(eventName, data)`

Send a fire-and-forget event. Silently dropped if not connected — no warning, no queuing.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `eventName` | string | Yes | The event name |
| `data` | string | Yes | JSON-encoded data |

```javascript
// Ideal for high-frequency, non-critical data like cursor positions
socket.EmitVolatile("cursor", JSON.stringify({ x: 100, y: 200 }));
```

**Behavior:**
- When connected: sends immediately (same as `Emit`)
- When disconnected: **silently dropped** — no warning logged, no queuing
- Null or empty `eventName`: silently ignored

---

## Event Listeners

### `On(eventName, functionName)`

Register a persistent callback for a named event. The callback fires every time the event is received.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `eventName` | string | Yes | The event name to listen for |
| `functionName` | string | Yes | Name of the JS function to invoke |

```javascript
socket.On("message", "onMessage");

function onMessage(data) {
    var msg = JSON.parse(data);
    log("Received: " + msg.text);
}
```

**Behavior:**
- Multiple callbacks can be registered for the same event — all fire in registration order
- Also used for system events: `connect`, `disconnect`, `connect_error`, `reconnect`, `reconnect_attempt`, `reconnect_failed`
- Null or empty `eventName` or `functionName`: silently ignored

### `Off(eventName)`

Remove all callbacks for a named event.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `eventName` | string | Yes | The event name to unregister |

```javascript
socket.Off("message");
```

**Behavior:**
- Removes persistent (`On`) and one-time (`Once`) callbacks for the event
- Does NOT remove catch-all (`OnAny`) callbacks
- Null or empty `eventName`: silently ignored

### `Once(eventName, functionName)`

Register a one-time callback. Automatically removed after the first invocation.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `eventName` | string | Yes | The event name to listen for |
| `functionName` | string | Yes | Name of the JS function to invoke |

```javascript
socket.Once("welcome", "onWelcome");

function onWelcome(data) {
    log("Welcome (fires only once): " + data);
}
```

### `OnAny(functionName)`

Register a catch-all callback that fires for every event, including system events.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `functionName` | string | Yes | Name of the JS function to invoke |

The callback receives two parameters: `(eventName, data)`.

```javascript
socket.OnAny("onAnyEvent");

function onAnyEvent(eventName, data) {
    log("[" + eventName + "] " + data);
}
```

**Note:** `OnAny` fires for ALL events, including system events like `connect` and `disconnect`. If you only want to debug user events, filter by event name in your callback.

### `OffAny()`

Remove all catch-all callbacks registered with `OnAny`.

```javascript
socket.OffAny();
```

### `OnBinary(eventName, functionName)`

Register a callback for a named binary event. The callback receives the event name and base64-encoded data.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `eventName` | string | Yes | The event name to listen for |
| `functionName` | string | Yes | Name of the JS function to invoke |

The callback receives two parameters: `(eventName, base64Data)`.

```javascript
socket.OnBinary("download", "onDownload");

function onDownload(eventName, base64Data) {
    var binaryString = atob(base64Data);
    log("Received " + binaryString.length + " bytes for event: " + eventName);
}
```

### `OffBinary(eventName)`

Remove all binary callbacks for a named event.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `eventName` | string | Yes | The event name to unregister |

```javascript
socket.OffBinary("download");
```

---

## Rooms

Room membership is server-side state. The client sends join/leave requests to the server.

### `JoinRoom(room)`

Request the server to add this socket to a named room.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `room` | string | Yes | The room name |

```javascript
socket.JoinRoom("lobby");
```

**Behavior:**
- When connected: sends join request immediately
- When disconnected: **NOT queued** — warning logged, dropped
- After reconnection, you must re-join rooms manually (room membership is lost on disconnect)
- Null or empty `room`: silently ignored

### `LeaveRoom(room)`

Request the server to remove this socket from a named room.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `room` | string | Yes | The room name |

```javascript
socket.LeaveRoom("lobby");
```

**Behavior:**
- When connected: sends leave request immediately
- When disconnected: **NOT queued** — warning logged, dropped
- Null or empty `room`: silently ignored

---

## Namespaces

Namespaces allow multiplexing independent communication channels on a single connection.

### `Of(namespace)`

Get or create a namespace-scoped socket. Returns a cached instance if the namespace was previously requested.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `namespace` | string | Yes | The namespace path (e.g., `"/chat"`) |

**Returns:** A namespace wrapper object with the full API surface, or `null` if not connected.

```javascript
var chat = socket.Of("/chat");
var game = socket.Of("/game");

// Same namespace returns the same cached instance
var chat2 = socket.Of("/chat");  // chat2 === chat
```

**Behavior:**
- When connected: returns namespace wrapper (creates if first call, returns cached if previously created)
- When disconnected: returns `null` with a warning
- Null or empty `namespace`: returns `null` silently
- No additional network connection is created — namespaces share the parent transport

### Namespace Wrapper API

The object returned by `Of()` supports the following methods and properties:

| Method/Property | Description |
|----------------|-------------|
| `Emit(eventName, data)` | Emit scoped to this namespace |
| `EmitBinary(eventName, base64Data)` | Binary emit scoped to this namespace |
| `EmitWithAck(eventName, data, callbackFn)` | Emit with ack scoped to this namespace |
| `EmitVolatile(eventName, data)` | Volatile emit scoped to this namespace |
| `On(eventName, functionName)` | Listen for events on this namespace |
| `Off(eventName)` | Remove listeners on this namespace |
| `Once(eventName, functionName)` | One-time listener on this namespace |
| `OnAny(functionName)` | Catch-all listener on this namespace |
| `OffAny()` | Remove catch-all listeners on this namespace |
| `OnBinary(eventName, functionName)` | Binary event listener on this namespace |
| `OffBinary(eventName)` | Remove binary listeners on this namespace |
| `JoinRoom(room)` | Join room on this namespace |
| `LeaveRoom(room)` | Leave room on this namespace |
| `Connected` | Whether the parent connection is active (boolean) |
| `Id` | Socket ID for this namespace (string) |

Events are fully isolated between namespaces. An `On("message", ...)` on `/chat` does NOT receive messages from `/game`.

---

## Connection Options

Pass an options object as the second parameter to `Connect()`.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `auth` | string | `null` | Authentication token sent with the connection handshake |
| `headers` | object | `null` | Custom HTTP headers (e.g., `{ "X-Api-Key": "abc" }`) |
| `query` | object | `null` | URL query parameters (e.g., `{ "room": "lobby" }`) |
| `transport` | string | `"polling"` | Transport type: `"websocket"` or `"polling"` |
| `reconnection` | boolean | `true` | Enable automatic reconnection on disconnect |
| `reconnectionAttempts` | integer | `5` | Maximum number of reconnection attempts |
| `reconnectionDelay` | integer | `1000` | Initial reconnection delay in milliseconds |
| `reconnectionDelayMax` | integer | `5000` | Maximum reconnection delay in milliseconds (backoff caps here) |
| `timeout` | integer | `20000` | Connection timeout in milliseconds |
| `ackTimeout` | integer | `10000` | Acknowledgement timeout in milliseconds |
| `queueSize` | integer | `100` | Maximum messages to queue during disconnect (0 = unbounded) |

```javascript
socket.Connect("https://my-server.com", {
    auth: "bearer-token-123",
    headers: { "X-Custom": "value" },
    query: { "room": "lobby" },
    transport: "websocket",
    reconnection: true,
    reconnectionAttempts: 10,
    reconnectionDelay: 2000,
    reconnectionDelayMax: 10000,
    timeout: 15000,
    ackTimeout: 5000,
    queueSize: 200
});
```

### Reconnection Backoff

When reconnection is enabled, delays follow exponential backoff:

```
Attempt 1: reconnectionDelay (e.g., 1000ms)
Attempt 2: reconnectionDelay * 2 (e.g., 2000ms)
Attempt 3: reconnectionDelay * 4 (e.g., 4000ms)
Attempt N: capped at reconnectionDelayMax (e.g., 5000ms)
```

---

## System Events

System events are emitted automatically by the client. Listen for them using `On()`.

### `connect`

Fired when the connection is successfully established.

**Callback receives:** `socketId` (string) — the server-assigned socket ID.

```javascript
socket.On("connect", "onConnect");

function onConnect(socketId) {
    log("Connected with ID: " + socketId);
}
```

### `disconnect`

Fired when the connection is lost.

**Callback receives:** `reason` (string) — the disconnect reason.

Common reasons:
- `"transport close"` — network connection lost
- `"ping timeout"` — server not responding
- `"client disconnect"` — `Disconnect()` was called

```javascript
socket.On("disconnect", "onDisconnect");

function onDisconnect(reason) {
    log("Disconnected: " + reason);
}
```

### `connect_error`

Fired when the connection attempt fails.

**Callback receives:** `errorMessage` (string) — description of the error.

```javascript
socket.On("connect_error", "onConnectError");

function onConnectError(errorMessage) {
    log("Connection failed: " + errorMessage);
}
```

### `reconnect_attempt`

Fired when a reconnection attempt begins.

**Callback receives:** `attemptNumber` (string) — the current attempt number.

```javascript
socket.On("reconnect_attempt", "onReconnectAttempt");

function onReconnectAttempt(attemptNumber) {
    log("Reconnection attempt #" + attemptNumber);
}
```

### `reconnect`

Fired when reconnection succeeds.

**Callback receives:** `attemptCount` (string) — total number of attempts it took.

```javascript
socket.On("reconnect", "onReconnect");

function onReconnect(attemptCount) {
    log("Reconnected after " + attemptCount + " attempts");
    // Re-join rooms here — room membership is lost on disconnect
    socket.JoinRoom("lobby");
}
```

### `reconnect_failed`

Fired when all reconnection attempts are exhausted.

**Callback receives:** no data.

```javascript
socket.On("reconnect_failed", "onReconnectFailed");

function onReconnectFailed() {
    log("Could not reconnect to server");
}
```

---

## Message Queue

The client automatically queues outbound messages when not connected and replays them in order upon reconnection.

### Queuing Rules

| Method | Queued During Disconnect? | Behavior |
|--------|--------------------------|----------|
| `Emit` | **Yes** | Queued and replayed in FIFO order on reconnect |
| `EmitBinary` | No | Warning logged, data dropped |
| `EmitWithAck` | No | Warning logged, dropped |
| `EmitVolatile` | No | Silently dropped (no warning) |
| `JoinRoom` | No | Warning logged, dropped |
| `LeaveRoom` | No | Warning logged, dropped |

### Queue Configuration

- Default queue capacity: **100 messages**
- Configurable via `queueSize` in connection options
- When the queue is full, the **oldest message is dropped** to make room for the new one
- Set `queueSize: 0` for an unbounded queue (no limit)
- The entire queue is cleared when `Terminate()` is called

### Replay Behavior

On successful reconnection:
1. All queued messages are dequeued in FIFO order
2. Each message is sent via `Emit` to the server
3. The queue is cleared after replay

---

## Quick Reference

### All Methods

| Method | Description |
|--------|-------------|
| `new SocketIO()` | Create client instance |
| `Connect(url, options?)` | Connect to server |
| `Disconnect()` | Disconnect from server |
| `Emit(event, data)` | Send event (queued if disconnected) |
| `EmitBinary(event, base64)` | Send binary event |
| `EmitWithAck(event, data, fn)` | Send with server acknowledgement |
| `EmitVolatile(event, data)` | Fire-and-forget send |
| `On(event, fn)` | Register persistent listener |
| `Off(event)` | Remove all listeners for event |
| `Once(event, fn)` | Register one-time listener |
| `OnAny(fn)` | Register catch-all listener |
| `OffAny()` | Remove catch-all listeners |
| `OnBinary(event, fn)` | Register binary event listener |
| `OffBinary(event)` | Remove binary listeners |
| `JoinRoom(room)` | Join a server room |
| `LeaveRoom(room)` | Leave a server room |
| `Of(namespace)` | Get namespace-scoped socket |
| `Terminate()` | Release all resources |

### All Properties

| Property | Type | Description |
|----------|------|-------------|
| `Connected` | boolean | Whether currently connected |
| `Id` | string | Server-assigned socket ID |

### All System Events

| Event | Callback Receives | Description |
|-------|-------------------|-------------|
| `connect` | socketId | Connection established |
| `disconnect` | reason | Connection lost |
| `connect_error` | errorMessage | Connection failed |
| `reconnect_attempt` | attemptNumber | Reconnection attempt started |
| `reconnect` | attemptCount | Reconnection succeeded |
| `reconnect_failed` | *(none)* | All reconnection attempts failed |
