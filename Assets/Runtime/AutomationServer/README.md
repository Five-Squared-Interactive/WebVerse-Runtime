# WebVerse Automation Server — API Reference

The WebVerse Automation Server is an embedded HTTP REST server that enables external processes to control the WebVerse Runtime. It is designed for automated testing, scripted workflows, and CI/CD integration.

> [!IMPORTANT]
> The server only runs when explicitly enabled via command-line flag.  
> It binds to `localhost` only and is **not** accessible from other machines.

---

## Getting Started

### 1. Launch with automation enabled

```bash
WebVerse.exe --automation-port 9876
```

### 2. Send commands via HTTP

```bash
curl http://localhost:9876/api/v1/status
```

All endpoints are prefixed with `/api/v1`. Responses are JSON unless otherwise noted.

---

## Endpoints

### `GET /api/v1/status`

Returns the current runtime state and version.

**Response `200`**
```json
{
  "state": "loaded",
  "version": "v2.2.0"
}
```

| `state` value       | Meaning                            |
|----------------------|------------------------------------|
| `"not_initialized"`  | Runtime has not started yet        |
| `"unloaded"`         | Runtime ready, no world loaded     |
| `"loading"`          | World is currently loading         |
| `"loaded"`           | World is loaded and running        |
| `"webpage"`          | A webpage is loaded                |
| `"error"`            | A fatal error has occurred         |

---

### `POST /api/v1/world/load`

Begins loading a world from a URL. Returns immediately — loading is **asynchronous**. Poll `/api/v1/world/state` to know when loading is complete.

**Request body**
```json
{
  "url": "https://example.com/world.veml"
}
```

**Response `202` (Accepted)**
```json
{
  "status": "loading",
  "url": "https://example.com/world.veml"
}
```

**Error `400`** — missing URL:
```json
{ "error": "Missing 'url' in request body." }
```

**Error `503`** — runtime not ready:
```json
{ "error": "Runtime not initialized." }
```

Supported URL formats:
- VEML worlds: `https://example.com/world.veml`
- X3D worlds: `https://example.com/scene.x3d`
- glTF/GLB worlds: `https://example.com/scene.glb`
- Local files: `file:///C:/path/to/world.veml`

---

### `POST /api/v1/world/unload`

Unloads the currently loaded world.

**Response `200`**
```json
{
  "status": "unloaded"
}
```

---

### `GET /api/v1/world/state`

Returns the current world loading state, world name, and URL.

**Response `200` (world loaded)**
```json
{
  "state": "loaded",
  "worldName": "My World",
  "url": "https://example.com/world.veml"
}
```

**Response `200` (no world)**
```json
{
  "state": "unloaded",
  "worldName": null,
  "url": null
}
```

---

### `GET /api/v1/entities`

Returns a summary of all entities in the current world.

**Response `200`**
```json
{
  "entities": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "type": "MeshEntity",
      "tag": "player-cube",
      "position": { "x": 1.5, "y": 0.0, "z": -3.2 },
      "rotation": { "x": 0.0, "y": 45.0, "z": 0.0 },
      "visible": true
    },
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "type": "LightEntity",
      "tag": null,
      "position": { "x": 0.0, "y": 10.0, "z": 0.0 },
      "rotation": { "x": 50.0, "y": -30.0, "z": 0.0 },
      "visible": true
    }
  ],
  "count": 2
}
```

**Response `200` (no world loaded)** — returns an empty list:
```json
{ "entities": [], "count": 0 }
```

Entity types include: `ContainerEntity`, `MeshEntity`, `LightEntity`, `CharacterEntity`, `TerrainEntity`, `CanvasEntity`, `VoxelEntity`, and others.

---

### `GET /api/v1/entity/{identifier}`

Returns detailed information about a single entity. The `{identifier}` can be either:
- A **GUID** (e.g., `a1b2c3d4-e5f6-7890-abcd-ef1234567890`)
- An **entity tag** (e.g., `player-cube`)

If a tag is provided, the first entity with a matching tag is returned.

**Response `200`**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "type": "MeshEntity",
  "tag": "player-cube",
  "position": { "x": 1.5, "y": 0.0, "z": -3.2 },
  "rotation": { "x": 0.0, "y": 45.0, "z": 0.0 },
  "scale": { "x": 1.0, "y": 1.0, "z": 1.0 },
  "visible": true,
  "interactionState": "Physical",
  "parent": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "children": [
    "d4e5f6a7-b8c9-0123-defa-234567890123"
  ]
}
```

| Field              | Type       | Description                                            |
|--------------------|------------|--------------------------------------------------------|
| `id`               | string     | Entity GUID                                            |
| `type`             | string     | Entity class name                                      |
| `tag`              | string?    | Entity tag, or `null` if none                          |
| `position`         | {x, y, z}  | World-space position                                   |
| `rotation`         | {x, y, z}  | Euler rotation in degrees                              |
| `scale`            | {x, y, z}  | Local scale                                            |
| `visible`          | boolean    | Whether the entity's GameObject is active              |
| `interactionState` | string     | `"Hidden"`, `"Static"`, `"Physical"`, or `"Placing"`   |
| `parent`           | string?    | Parent entity GUID, or `null`                          |
| `children`         | string[]   | Array of child entity GUIDs                            |

**Error `404`** — entity not found:
```json
{ "error": "Entity not found: my-missing-tag" }
```

---

### `POST /api/v1/script/run`

Executes a JavaScript snippet in the WebVerse JavaScript runtime and returns the result.

**Request body**
```json
{
  "script": "entity.get('player-cube').getPosition()"
}
```

**Response `200`**
```json
{
  "result": "(1.5, 0.0, -3.2)"
}
```

The `result` field is always a string (via `.ToString()`). If the script returns nothing, the value is `"null"`.

**Error `400`** — missing script:
```json
{ "error": "Missing 'script' in request body." }
```

**Error `503`** — handler not ready:
```json
{ "error": "JavaScript handler not initialized." }
```

---

### `GET /api/v1/screenshot`

Captures the current rendered frame as a PNG image.

**Response `200`** — binary PNG data  
`Content-Type: image/png`

This is the only endpoint that does **not** return JSON. Save the response body directly to a file:
```bash
curl http://localhost:9876/api/v1/screenshot --output screenshot.png
```

**Error `500`** (JSON):
```json
{ "error": "Failed to capture screenshot." }
```

---

### `POST /api/v1/quit`

Shuts down the WebVerse application. The response is sent before the process exits.

**Response `200`**
```json
{
  "status": "shutting_down"
}
```

---

## Error Handling

All error responses use a consistent format:

```json
{ "error": "Description of the error." }
```

| HTTP Code | Meaning                                        |
|-----------|------------------------------------------------|
| `200`     | Success                                        |
| `202`     | Accepted (async operation started)             |
| `400`     | Bad request (missing or invalid parameters)    |
| `404`     | Not found (unknown route or entity)            |
| `500`     | Internal server error                          |
| `503`     | Service unavailable (runtime not initialized)  |
| `504`     | Gateway timeout (command took > 30 seconds)    |

---

## CORS

The server includes CORS headers on all responses, enabling use from browser-based tooling:

```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, OPTIONS
Access-Control-Allow-Headers: Content-Type
```

`OPTIONS` preflight requests return `204 No Content`.

---

## Usage Examples

### Node.js

```javascript
const http = require('http');

function automationRequest(method, path, body = null) {
  return new Promise((resolve, reject) => {
    const options = {
      hostname: 'localhost',
      port: 9876,
      path: `/api/v1${path}`,
      method,
      headers: { 'Content-Type': 'application/json' }
    };

    const req = http.request(options, (res) => {
      let data = '';
      res.on('data', chunk => data += chunk);
      res.on('end', () => {
        try { resolve(JSON.parse(data)); }
        catch { resolve(data); }
      });
    });

    req.on('error', reject);
    if (body) req.write(JSON.stringify(body));
    req.end();
  });
}

// Example test flow
async function runTest() {
  // Check runtime is ready
  const status = await automationRequest('GET', '/status');
  console.log('Runtime state:', status.state);

  // Load a world
  await automationRequest('POST', '/world/load', {
    url: 'https://example.com/world.veml'
  });

  // Poll until loaded
  let state;
  do {
    await new Promise(r => setTimeout(r, 1000));
    state = await automationRequest('GET', '/world/state');
  } while (state.state === 'loading');

  console.log('World loaded:', state.worldName);

  // Inspect entities
  const entities = await automationRequest('GET', '/entities');
  console.log(`Found ${entities.count} entities`);

  // Get specific entity by tag
  const player = await automationRequest('GET', '/entity/player-cube');
  console.log('Player position:', player.position);

  // Run a script
  const result = await automationRequest('POST', '/script/run', {
    script: '2 + 2'
  });
  console.log('Script result:', result.result);

  // Shut down
  await automationRequest('POST', '/quit');
}

runTest().catch(console.error);
```

### Python

```python
import requests
import time

BASE = "http://localhost:9876/api/v1"

# Check status
status = requests.get(f"{BASE}/status").json()
print(f"Runtime state: {status['state']}")

# Load a world
requests.post(f"{BASE}/world/load", json={
    "url": "https://example.com/world.veml"
})

# Poll until loaded
while True:
    state = requests.get(f"{BASE}/world/state").json()
    if state["state"] != "loading":
        break
    time.sleep(1)

print(f"World loaded: {state['worldName']}")

# List entities
entities = requests.get(f"{BASE}/entities").json()
print(f"Found {entities['count']} entities")

# Get entity detail
player = requests.get(f"{BASE}/entity/player-cube").json()
print(f"Player position: {player['position']}")

# Capture screenshot
screenshot = requests.get(f"{BASE}/screenshot")
with open("screenshot.png", "wb") as f:
    f.write(screenshot.content)

# Run JavaScript
result = requests.post(f"{BASE}/script/run", json={
    "script": "entity.get('player-cube').getPosition()"
}).json()
print(f"Script result: {result['result']}")

# Quit
requests.post(f"{BASE}/quit")
```

### curl (shell scripts)

```bash
#!/bin/bash
BASE="http://localhost:9876/api/v1"

# Wait for runtime to be ready
until curl -s "$BASE/status" | grep -q '"state"'; do
  sleep 1
done

# Load world
curl -s -X POST "$BASE/world/load" \
  -H "Content-Type: application/json" \
  -d '{"url": "https://example.com/world.veml"}'

# Wait for world to load
while true; do
  STATE=$(curl -s "$BASE/world/state" | grep -o '"state": "[^"]*"')
  echo "State: $STATE"
  [[ "$STATE" == *"loading"* ]] || break
  sleep 1
done

# List entities
curl -s "$BASE/entities" | python -m json.tool

# Screenshot
curl -s "$BASE/screenshot" --output screenshot.png

# Quit
curl -s -X POST "$BASE/quit"
```
