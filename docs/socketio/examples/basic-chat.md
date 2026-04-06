# Example: Basic Chat

Connect to a Socket.IO server, send messages, and listen for incoming messages — the simplest real-time pattern.

## The Script

```javascript
var socket = new SocketIO();

// Register listeners before connecting
socket.On("connect", "onConnect");
socket.On("connect_error", "onConnectError");
socket.On("message", "onMessage");
socket.On("disconnect", "onDisconnect");

socket.Connect("https://my-server.com");

// --- Handler Functions ---

function onConnect(socketId) {
    log("Connected with ID: " + socketId);
    socket.Emit("message", JSON.stringify({ text: "Hello from WebVerse!", sender: "player1" }));
}

function onConnectError(errorMessage) {
    log("Connection failed: " + errorMessage);
}

function onMessage(data) {
    var msg = JSON.parse(data);
    log("[Chat] " + msg.sender + ": " + msg.text);
}

function onDisconnect(reason) {
    log("Disconnected: " + reason);
}
```

## How It Works

1. **`new SocketIO()`** — Creates a new Socket.IO client instance (no connection yet).
2. **`On("connect", "onConnect")`** — Registers `onConnect` as a persistent listener for the `connect` system event. The function name is passed as a string, not a function reference.
3. **`On("connect_error", "onConnectError")`** — Listens for connection failures so you can log or display the error.
4. **`On("message", "onMessage")`** — Listens for incoming `message` events from the server.
5. **`Connect("https://my-server.com")`** — Initiates the connection. Listeners are registered first so no events are missed.
6. **`Emit("message", JSON.stringify(...))`** — Sends a `message` event with a JSON-encoded payload. All data must be passed as a JSON string.
7. **`JSON.parse(data)`** — Incoming data arrives as a string. Parse it to access object properties.

## Try It

Point the `Connect()` URL at any Socket.IO server that echoes or broadcasts `message` events. Expected behavior:

1. Console shows `Connected with ID: <server-assigned-id>`
2. Server receives `{ text: "Hello from WebVerse!", sender: "player1" }`
3. If the server broadcasts the message back, console shows `[Chat] player1: Hello from WebVerse!`
4. On disconnect, console shows `Disconnected: <reason>`

## See Also

- [API Reference](../api-reference.md) — See `Emit` and `On` method signatures
- [Error Handling Guide](../error-handling.md) for troubleshooting connection errors
