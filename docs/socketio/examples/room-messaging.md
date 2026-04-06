# Example: Room-Based Messaging

Join and leave named rooms for scoped messaging — only receive events relevant to your current room.

## The Script

```javascript
var socket = new SocketIO();
var currentRoom = "lobby";

socket.On("connect", "onConnect");
socket.On("connect_error", "onConnectError");
socket.On("room_message", "onRoomMessage");
socket.On("user_joined", "onUserJoined");
socket.On("user_left", "onUserLeft");
socket.On("disconnect", "onDisconnect");
socket.On("reconnect", "onReconnect");

socket.Connect("https://my-server.com");

// --- Handler Functions ---

function onConnect(socketId) {
    log("Connected: " + socketId);
    socket.JoinRoom(currentRoom);
    log("Joined room: " + currentRoom);
}

function onConnectError(errorMessage) {
    log("Connection failed: " + errorMessage);
}

function onRoomMessage(data) {
    var msg = JSON.parse(data);
    log("[" + msg.room + "] " + msg.sender + ": " + msg.text);
}

function onUserJoined(data) {
    var info = JSON.parse(data);
    log(info.user + " joined " + info.room);
}

function onUserLeft(data) {
    var info = JSON.parse(data);
    log(info.user + " left " + info.room);
}

function onDisconnect(reason) {
    log("Disconnected: " + reason);
}

function onReconnect(attemptCount) {
    log("Reconnected after " + attemptCount + " attempts");
    // Re-join room — room membership is lost on disconnect
    socket.JoinRoom(currentRoom);
    log("Re-joined room: " + currentRoom);
}

// --- Room Management Functions ---

function sendToRoom(text) {
    var payload = JSON.stringify({ text: text, room: currentRoom, sender: "player1" });
    socket.Emit("room_message", payload);
}

function switchRoom(newRoom) {
    socket.LeaveRoom(currentRoom);
    log("Left room: " + currentRoom);

    currentRoom = newRoom;

    socket.JoinRoom(currentRoom);
    log("Joined room: " + currentRoom);
}
```

## How It Works

1. **`JoinRoom("lobby")`** — Sends a join request to the server. The server adds this socket to the `lobby` room, enabling it to receive room-scoped events.
2. **`LeaveRoom("lobby")`** — Sends a leave request. The server removes this socket from the room.
3. **`switchRoom(newRoom)`** — Leaves the current room and joins a new one. Room membership is server-side state managed by the server.
4. **`onReconnect`** — After a reconnection, room membership is lost because it's server-side state. You must call `JoinRoom()` again to rejoin.

## Re-joining After Reconnect

Room membership is **server-side state** and is lost when the connection drops. Always re-join rooms in your `reconnect` handler:

```javascript
socket.On("reconnect", "onReconnect");

function onReconnect(attemptCount) {
    // Re-join all rooms your script needs
    socket.JoinRoom("lobby");
    socket.JoinRoom("game-42");
}
```

This pattern is essential for any application using rooms. Without it, your client silently stops receiving room-scoped events after a reconnection.

## Try It

Your server must handle room join/leave requests. A typical server implementation:

```javascript
// Server-side (Node.js Socket.IO server)
io.on("connection", (socket) => {
    socket.on("join", (room) => socket.join(room));
    socket.on("leave", (room) => socket.leave(room));
    socket.on("room_message", (data) => {
        io.to(data.room).emit("room_message", data);
    });
});
```

## See Also

- [API Reference](../api-reference.md) — See `JoinRoom` and `LeaveRoom` method signatures
- [Connection Resilience Example](connection-resilience.md) for full reconnection handling
