# Example: Binary Transfer

Send and receive binary data using base64 encoding — useful for images, audio, or other binary payloads.

## The Script

```javascript
var socket = new SocketIO();

socket.On("connect", "onConnect");
socket.On("connect_error", "onConnectError");
socket.OnBinary("download", "onDownload");
socket.On("disconnect", "onDisconnect");

socket.Connect("https://my-server.com");

// --- Handler Functions ---

function onConnect(socketId) {
    log("Connected: " + socketId);

    // Send binary data as base64-encoded string
    var payload = "Hello binary world!";
    var encoded = btoa(payload);
    socket.EmitBinary("upload", encoded);
    log("Sent binary data (" + encoded.length + " base64 chars)");
}

function onConnectError(errorMessage) {
    log("Connection failed: " + errorMessage);
}

function onDownload(eventName, base64Data) {
    // Decode the received base64 data
    var decoded = atob(base64Data);
    log("Received binary on '" + eventName + "': " + decoded);
}

function onDisconnect(reason) {
    log("Disconnected: " + reason);
}
```

## How It Works

1. **`btoa(payload)`** — Encodes a string into base64 format. All binary data in WebVerse Socket.IO must be base64-encoded before sending.
2. **`EmitBinary("upload", encoded)`** — Sends binary data to the server. The data parameter must be a valid base64 string. If it's not, the message is dropped with an `Invalid base64 data` warning.
3. **`OnBinary("download", "onDownload")`** — Registers a binary event listener. The callback receives two arguments: `(eventName, base64Data)`.
4. **`atob(base64Data)`** — Decodes the received base64 string back to its original form.

## Common Pitfalls

### Forgetting to encode with btoa()

```javascript
// WRONG — raw string is not base64, message will be dropped
socket.EmitBinary("upload", "raw data here");

// CORRECT — encode first
var encoded = btoa("raw data here");
socket.EmitBinary("upload", encoded);
```

### Using On() instead of OnBinary() for binary events

```javascript
// WRONG — On() is for JSON string events
socket.On("download", "onDownload");

// CORRECT — OnBinary() for binary events
socket.OnBinary("download", "onDownload");
```

### Not checking connection before sending

```javascript
// RECOMMENDED — EmitBinary is NOT queued during disconnection
if (socket.Connected) {
    socket.EmitBinary("upload", btoa(payload));
}
```

Unlike `Emit()`, `EmitBinary()` is **not queued** during disconnection. If you call it while disconnected, the message is dropped with a warning.

## See Also

- [API Reference](../api-reference.md) — See `EmitBinary` and `OnBinary` method signatures
- [Error Handling Guide](../error-handling.md) — See "Invalid base64 data" for troubleshooting
