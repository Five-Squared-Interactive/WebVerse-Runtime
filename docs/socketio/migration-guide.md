# Migrating from Standard Socket.IO to WebVerse

This guide maps standard Socket.IO client JavaScript patterns to their WebVerse equivalents. If you've used Socket.IO before, you can port your existing code using this reference.

## What's Different

WebVerse Socket.IO provides the same real-time messaging capabilities as the standard Socket.IO client, with these key differences:

1. **Callbacks use function names, not function references** — You pass a string function name to `On()`, not an inline function or arrow function
2. **Data is always strings** — Use `JSON.stringify()` for objects and `btoa()` for binary data
3. **PascalCase API** — All method and property names use PascalCase (`Emit`, `Connected`) not camelCase (`emit`, `connected`)
4. **Two-step connection** — Create the client with `new SocketIO()`, then call `Connect(url)` separately
5. **No method chaining** — `socket.volatile.emit(...)` becomes `socket.EmitVolatile(...)`
6. **Binary uses base64** — Binary data is sent as base64-encoded strings via `EmitBinary()`, not as Buffer/ArrayBuffer
7. **Auth is a string** — `{ auth: "token" }` not `{ auth: { token: "..." } }`

---

## API Mapping

### Connection

| Standard Socket.IO | WebVerse | Notes |
|-------------------|----------|-------|
| `const socket = io("https://server.com")` | `var socket = new SocketIO(); socket.Connect("https://server.com");` | Two-step: create then connect |
| `const socket = io("https://server.com", { auth: { token: "x" } })` | `socket.Connect("https://server.com", { auth: "x" })` | Auth is string, not nested object |
| `socket.disconnect()` | `socket.Disconnect()` | PascalCase |
| `socket.connected` | `socket.Connected` | PascalCase property |
| `socket.id` | `socket.Id` | PascalCase property |

### Emitting Events

| Standard Socket.IO | WebVerse | Notes |
|-------------------|----------|-------|
| `socket.emit("chat", { text: "hi" })` | `socket.Emit("chat", JSON.stringify({ text: "hi" }))` | Data must be JSON string |
| `socket.emit("chat", data, (ack) => { ... })` | `socket.EmitWithAck("chat", JSON.stringify(data), "onAck")` | Separate method, function name for callback |
| `socket.volatile.emit("cursor", data)` | `socket.EmitVolatile("cursor", JSON.stringify(data))` | Method, not property chain |
| `socket.emit("upload", buffer)` | `socket.EmitBinary("upload", btoa(data))` | Separate method, base64 encoded |

### Listening for Events

| Standard Socket.IO | WebVerse | Notes |
|-------------------|----------|-------|
| `socket.on("chat", (data) => { ... })` | `socket.On("chat", "onChat")` | Function name string |
| `socket.off("chat")` | `socket.Off("chat")` | Same |
| `socket.once("welcome", (data) => { ... })` | `socket.Once("welcome", "onWelcome")` | Function name string |
| `socket.onAny((event, ...args) => { ... })` | `socket.OnAny("onAnyEvent")` | Callback receives `(eventName, data)` |
| `socket.offAny()` | `socket.OffAny()` | Same |

### Rooms

| Standard Socket.IO | WebVerse | Notes |
|-------------------|----------|-------|
| *Server-side: `socket.join("lobby")`* | `socket.JoinRoom("lobby")` | Client sends join request to server |
| *Server-side: `socket.leave("lobby")`* | `socket.LeaveRoom("lobby")` | Client sends leave request to server |

**Note:** In standard Socket.IO, room management is server-side only. WebVerse provides client-side `JoinRoom`/`LeaveRoom` methods that send join/leave requests to the server. Your server must handle these events (typically by listening for `"join"` and `"leave"` events).

### Namespaces

| Standard Socket.IO | WebVerse | Notes |
|-------------------|----------|-------|
| `const chat = io("https://server.com/chat")` | `var chat = socket.Of("/chat")` | From existing connected socket |
| `chat.emit("msg", data)` | `chat.Emit("msg", data)` | Same API surface as main socket |
| `chat.on("msg", cb)` | `chat.On("msg", "onMsg")` | Function name string |

---

## Side-by-Side Examples

### Standard Socket.IO Client

```javascript
const socket = io("https://my-server.com", {
    auth: { token: "my-token" },
    reconnection: true,
    reconnectionAttempts: 5
});

socket.on("connect", () => {
    console.log("Connected:", socket.id);
    socket.emit("join", { room: "lobby" });
});

socket.on("message", (data) => {
    console.log("Message:", data.text);
});

socket.on("disconnect", (reason) => {
    console.log("Disconnected:", reason);
});

socket.emit("chat", { text: "Hello!" });
```

### WebVerse Equivalent

```javascript
var socket = new SocketIO();

socket.On("connect", "onConnect");
socket.On("message", "onMessage");
socket.On("disconnect", "onDisconnect");

socket.Connect("https://my-server.com", {
    auth: "my-token",
    reconnection: true,
    reconnectionAttempts: 5
});

function onConnect(socketId) {
    log("Connected: " + socketId);
    socket.Emit("join", JSON.stringify({ room: "lobby" }));
}

function onMessage(data) {
    var msg = JSON.parse(data);
    log("Message: " + msg.text);
}

function onDisconnect(reason) {
    log("Disconnected: " + reason);
}

socket.Emit("chat", JSON.stringify({ text: "Hello!" }));
```

### Key Changes in the Example

1. `io(url, options)` → `new SocketIO()` + `Connect(url, options)`
2. `auth: { token: "..." }` → `auth: "..."`
3. Arrow functions `() => {}` → named functions + string references `"onConnect"`
4. `console.log` → `log`
5. `socket.emit("chat", { text: "Hello!" })` → `socket.Emit("chat", JSON.stringify({ text: "Hello!" }))`
6. Register listeners before connecting (recommended pattern)

---

## Connection Options Mapping

| Standard Option | WebVerse Option | Difference |
|----------------|-----------------|------------|
| `auth: { token: "x" }` | `auth: "x"` | String, not object |
| `reconnection: true` | `reconnection: true` | Same |
| `reconnectionAttempts: 5` | `reconnectionAttempts: 5` | Same |
| `reconnectionDelay: 1000` | `reconnectionDelay: 1000` | Same |
| `reconnectionDelayMax: 5000` | `reconnectionDelayMax: 5000` | Same |
| `timeout: 20000` | `timeout: 20000` | Same |
| `ackTimeout: 10000` | `ackTimeout: 10000` | Same |
| `transports: ["websocket"]` | `transport: "websocket"` | String, not array |
| `extraHeaders: { ... }` | `headers: { ... }` | Different field name |
| `query: { ... }` | `query: { ... }` | Same |
| *Not available* | `queueSize: 100` | WebVerse-specific message queue size |

---

## Features Not Available in WebVerse v1

The following standard Socket.IO features are not supported in the current WebVerse implementation:

| Feature | Standard Socket.IO | Status |
|---------|-------------------|--------|
| Client middleware | `socket.use((packet, next) => { ... })` | Not supported |
| Per-emit timeout | `socket.timeout(5000).emit(...)` | Use `ackTimeout` in connection options |
| Compression control | `socket.compress(false).emit(...)` | Not supported |
| Room queries | `socket.rooms` | Not supported (rooms are server-side only) |
| Custom parsers | `io({ parser: customParser })` | Not supported |
| Admin API | `fetchSockets()`, `socketsJoin()`, `socketsLeave()` | Not supported (server-side admin) |
| Manager events | `socket.io.on("reconnect_attempt", ...)` | Use `socket.On("reconnect_attempt", ...)` instead |
| Binary auto-detection | `socket.emit("event", buffer)` | Use `EmitBinary()` with base64-encoded data |
| Multiple arguments | `socket.emit("event", arg1, arg2, arg3)` | Pack into single JSON string |
| Prepend listeners | `socket.prependAny()`, `socket.prependAnyOutgoing()` | Not supported |
| Listener introspection | `socket.listeners()`, `socket.listenersAny()` | Not supported |
| Outgoing catch-all | `socket.onAnyOutgoing()`, `socket.offAnyOutgoing()` | Not supported |
| Socket state properties | `socket.active`, `socket.disconnected`, `socket.recovered` | Use `socket.Connected` only |
| Send shorthand | `socket.send(...args)` | Use `Emit("message", JSON.stringify(data))` instead |

---

## Common Migration Mistakes

### 1. Passing function references instead of names

```javascript
// WRONG — standard Socket.IO pattern
socket.On("message", function(data) { log(data); });

// CORRECT — WebVerse pattern
socket.On("message", "onMessage");
function onMessage(data) { log(data); }
```

### 2. Forgetting to JSON.stringify data

```javascript
// WRONG — object will be converted to "[object Object]"
socket.Emit("chat", { text: "hello" });

// CORRECT
socket.Emit("chat", JSON.stringify({ text: "hello" }));
```

### 3. Using io() instead of new SocketIO()

```javascript
// WRONG — io() function does not exist
var socket = io("https://server.com");

// CORRECT — two-step creation
var socket = new SocketIO();
socket.Connect("https://server.com");
```

### 4. Using camelCase method names

```javascript
// WRONG — camelCase
socket.emit("chat", data);
socket.on("message", "handler");

// CORRECT — PascalCase
socket.Emit("chat", data);
socket.On("message", "handler");
```

### 5. Creating namespace connections separately

```javascript
// WRONG — standard pattern creates new connection
var chat = io("https://server.com/chat");

// CORRECT — namespaces share existing connection
var socket = new SocketIO();
socket.Connect("https://server.com");
// After connect event:
var chat = socket.Of("/chat");
```
