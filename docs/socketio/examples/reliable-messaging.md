# Example: Reliable Messaging

Use `EmitWithAck` to send events and wait for server acknowledgement — essential for operations where you need confirmation that the server processed your message.

## The Script

```javascript
var socket = new SocketIO();

socket.On("connect", "onConnect");
socket.On("connect_error", "onConnectError");
socket.On("disconnect", "onDisconnect");

socket.Connect("https://my-server.com", {
    ackTimeout: 5000
});

// --- Handler Functions ---

function onConnect(socketId) {
    log("Connected: " + socketId);

    // Save player score with server acknowledgement
    var scoreData = JSON.stringify({ playerId: "player1", score: 9500, level: 3 });
    socket.EmitWithAck("save_score", scoreData, "onScoreSaved");
    log("Saving score...");
}

function onScoreSaved(response) {
    try {
        var result = JSON.parse(response);
        if (result.success) {
            log("Score saved successfully! Server ID: " + result.id);
        } else {
            log("Score save failed: " + result.error);
        }
    } catch (e) {
        // Timeout or non-JSON response
        log("Score save error: " + response);
    }
}

function onConnectError(errorMessage) {
    log("Connection failed: " + errorMessage);
}

function onDisconnect(reason) {
    log("Disconnected: " + reason);
}

// --- Game Functions ---

function purchaseItem(itemId) {
    if (socket.Connected) {
        var data = JSON.stringify({ itemId: itemId, currency: "coins" });
        socket.EmitWithAck("purchase", data, "onPurchaseResult");
        log("Processing purchase...");
    } else {
        log("Cannot purchase: not connected");
    }
}

function onPurchaseResult(response) {
    try {
        var result = JSON.parse(response);
        if (result.success) {
            log("Purchased " + result.itemName + " for " + result.cost + " coins");
        } else {
            log("Purchase failed: " + result.error);
        }
    } catch (e) {
        log("Purchase error: " + response);
    }
}
```

## How It Works

1. **`EmitWithAck("save_score", data, "onScoreSaved")`** — Sends an event and registers a callback that fires when the server acknowledges receipt. The third argument is a function name string.
2. **`ackTimeout: 5000`** — If the server doesn't acknowledge within 5 seconds, the callback is invoked with a timeout error. Set this in connection options.
3. **Response parsing** — The server's acknowledgement data arrives as a string. Use `JSON.parse()` inside a try/catch — timeout responses are plain error strings, not JSON.
4. **Connection check** — `EmitWithAck` is **not queued** during disconnection. Always check `socket.Connected` before calling it for critical operations.

## When to Use Acks

**Use `EmitWithAck` for:**
- Saving game state or scores
- In-app purchases or transactions
- Room join confirmations
- Any operation where you need to know the server processed it

**Use regular `Emit` for:**
- Chat messages (queued during disconnect, replayed on reconnect)
- Position updates
- Non-critical notifications

**Use `EmitVolatile` for:**
- Cursor position (stale data is useless)
- Typing indicators
- Any message where dropping is acceptable

## See Also

- [API Reference](../api-reference.md) — See `EmitWithAck` method signature
- [API Reference — Connection Options](../api-reference.md#connection-options) for `ackTimeout` configuration
- [Error Handling Guide](../error-handling.md) — See "Callback error for event" for troubleshooting
