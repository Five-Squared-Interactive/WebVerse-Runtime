// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Text;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.StraightFour.Entity;

namespace FiveSQD.WebVerse.Automation
{
    /// <summary>
    /// Dispatches automation commands to the appropriate WebVerse Runtime methods.
    /// All methods are called on the Unity main thread.
    /// </summary>
    public class CommandDispatcher
    {
        /// <summary>
        /// API version prefix for all routes.
        /// </summary>
        private const string API_PREFIX = "/api/v1";

        /// <summary>
        /// Dispatch a command based on HTTP method and path.
        /// </summary>
        /// <param name="method">HTTP method (GET, POST, OPTIONS).</param>
        /// <param name="path">Request path.</param>
        /// <param name="body">Request body (JSON).</param>
        /// <param name="responseBody">Output response body.</param>
        /// <param name="responseContentType">Output response content type.</param>
        /// <param name="responseBytes">Output response bytes (for binary responses).</param>
        /// <param name="responseStatusCode">Output HTTP status code.</param>
        public void Dispatch(string method, string path, string body,
            out string responseBody, out string responseContentType,
            out byte[] responseBytes, out int responseStatusCode)
        {
            responseContentType = "application/json";
            responseBytes = null;
            responseStatusCode = 200;
            responseBody = "";

            // Handle CORS preflight.
            if (method == "OPTIONS")
            {
                responseStatusCode = 204;
                responseBody = "";
                return;
            }

            // Strip API prefix.
            string route = path;
            if (route.StartsWith(API_PREFIX))
            {
                route = route.Substring(API_PREFIX.Length);
            }

            // Remove trailing slash.
            if (route.EndsWith("/") && route.Length > 1)
            {
                route = route.Substring(0, route.Length - 1);
            }

            try
            {
                switch (route)
                {
                    case "/status":
                        HandleStatus(out responseBody, out responseStatusCode);
                        break;

                    case "/world/load":
                        HandleWorldLoad(body, out responseBody, out responseStatusCode);
                        break;

                    case "/world/unload":
                        HandleWorldUnload(out responseBody, out responseStatusCode);
                        break;

                    case "/world/state":
                        HandleWorldState(out responseBody, out responseStatusCode);
                        break;

                    case "/entities":
                        HandleListEntities(out responseBody, out responseStatusCode);
                        break;

                    case "/script/run":
                        HandleScriptRun(body, out responseBody, out responseStatusCode);
                        break;

                    case "/screenshot":
                        HandleScreenshot(out responseBody, out responseContentType,
                            out responseBytes, out responseStatusCode);
                        break;

                    case "/quit":
                        HandleQuit(out responseBody, out responseStatusCode);
                        break;

                    default:
                        // Check for entity-specific route: /entity/{id}
                        if (route.StartsWith("/entity/"))
                        {
                            string entityId = route.Substring("/entity/".Length);
                            HandleGetEntity(entityId, out responseBody, out responseStatusCode);
                        }
                        else
                        {
                            responseStatusCode = 404;
                            responseBody = "{\"error\": \"Unknown route: " +
                                AutomationServer.EscapeJson(route) + "\"}";
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                responseStatusCode = 500;
                responseBody = "{\"error\": \"" + AutomationServer.EscapeJson(e.Message) + "\"}";
                Logging.LogError("[CommandDispatcher->Dispatch] Error: " + e);
            }
        }

        /// <summary>
        /// Handle GET /status — returns runtime state and version.
        /// </summary>
        private void HandleStatus(out string responseBody, out int statusCode)
        {
            statusCode = 200;

            if (WebVerseRuntime.Instance == null)
            {
                responseBody = "{\"state\": \"not_initialized\", \"version\": \""
                    + WebVerseRuntime.versionString + "\"}";
                return;
            }

            string stateStr = GetRuntimeStateString(WebVerseRuntime.Instance.state);
            responseBody = "{\"state\": \"" + stateStr + "\", \"version\": \""
                + WebVerseRuntime.versionString + "\"}";
        }

        /// <summary>
        /// Handle POST /world/load — loads a world from a URL.
        /// Expected body: {"url": "https://..."}
        /// </summary>
        private void HandleWorldLoad(string body, out string responseBody, out int statusCode)
        {
            string url = ExtractJsonStringValue(body, "url");
            if (string.IsNullOrEmpty(url))
            {
                statusCode = 400;
                responseBody = "{\"error\": \"Missing 'url' in request body.\"}";
                return;
            }

            if (WebVerseRuntime.Instance == null)
            {
                statusCode = 503;
                responseBody = "{\"error\": \"Runtime not initialized.\"}";
                return;
            }

            WebVerseRuntime.Instance.LoadURL(url);

            statusCode = 202;
            responseBody = "{\"status\": \"loading\", \"url\": \""
                + AutomationServer.EscapeJson(url) + "\"}";
        }

        /// <summary>
        /// Handle POST /world/unload — unloads the current world.
        /// </summary>
        private void HandleWorldUnload(out string responseBody, out int statusCode)
        {
            if (WebVerseRuntime.Instance == null)
            {
                statusCode = 503;
                responseBody = "{\"error\": \"Runtime not initialized.\"}";
                return;
            }

            WebVerseRuntime.Instance.UnloadWorld();
            statusCode = 200;
            responseBody = "{\"status\": \"unloaded\"}";
        }

        /// <summary>
        /// Handle GET /world/state — returns the current world load state.
        /// </summary>
        private void HandleWorldState(out string responseBody, out int statusCode)
        {
            statusCode = 200;

            if (WebVerseRuntime.Instance == null)
            {
                responseBody = "{\"state\": \"not_initialized\", \"worldName\": null, \"url\": null}";
                return;
            }

            string stateStr = GetRuntimeStateString(WebVerseRuntime.Instance.state);
            string worldName = "null";
            string currentUrl = "null";

            if (StraightFour.StraightFour.ActiveWorld != null)
            {
                worldName = "\"" + AutomationServer.EscapeJson(
                    StraightFour.StraightFour.ActiveWorld.siteName ?? "") + "\"";
            }

            if (!string.IsNullOrEmpty(WebVerseRuntime.Instance.currentURL))
            {
                currentUrl = "\"" + AutomationServer.EscapeJson(
                    WebVerseRuntime.Instance.currentURL) + "\"";
            }

            responseBody = "{\"state\": \"" + stateStr + "\", \"worldName\": "
                + worldName + ", \"url\": " + currentUrl + "}";
        }

        /// <summary>
        /// Handle GET /entities — lists all entities in the current world.
        /// </summary>
        private void HandleListEntities(out string responseBody, out int statusCode)
        {
            if (WebVerseRuntime.Instance == null ||
                StraightFour.StraightFour.ActiveWorld == null)
            {
                statusCode = 200;
                responseBody = "{\"entities\": [], \"count\": 0}";
                return;
            }

            BaseEntity[] entities = StraightFour.StraightFour.ActiveWorld
                .entityManager.GetAllEntities();

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"entities\": [");

            for (int i = 0; i < entities.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(SerializeEntitySummary(entities[i]));
            }

            sb.Append("], \"count\": ");
            sb.Append(entities.Length);
            sb.Append("}");

            statusCode = 200;
            responseBody = sb.ToString();
        }

        /// <summary>
        /// Handle GET /entity/{id} — returns detailed info for a specific entity.
        /// </summary>
        private void HandleGetEntity(string entityId, out string responseBody, out int statusCode)
        {
            if (WebVerseRuntime.Instance == null ||
                StraightFour.StraightFour.ActiveWorld == null)
            {
                statusCode = 404;
                responseBody = "{\"error\": \"No world loaded.\"}";
                return;
            }

            Guid guid;
            if (!Guid.TryParse(entityId, out guid))
            {
                // Try finding by tag.
                BaseEntity[] allEntities = StraightFour.StraightFour.ActiveWorld
                    .entityManager.GetAllEntities();
                BaseEntity found = null;
                foreach (BaseEntity entity in allEntities)
                {
                    if (entity.entityTag == entityId)
                    {
                        found = entity;
                        break;
                    }
                }

                if (found == null)
                {
                    statusCode = 404;
                    responseBody = "{\"error\": \"Entity not found: "
                        + AutomationServer.EscapeJson(entityId) + "\"}";
                    return;
                }

                statusCode = 200;
                responseBody = SerializeEntityDetail(found);
                return;
            }

            BaseEntity entityByGuid = StraightFour.StraightFour.ActiveWorld
                .entityManager.FindEntity(guid);

            if (entityByGuid == null)
            {
                statusCode = 404;
                responseBody = "{\"error\": \"Entity not found: " + entityId + "\"}";
                return;
            }

            statusCode = 200;
            responseBody = SerializeEntityDetail(entityByGuid);
        }

        /// <summary>
        /// Handle POST /script/run — executes JavaScript and returns the result.
        /// Expected body: {"script": "..."}
        /// </summary>
        private void HandleScriptRun(string body, out string responseBody, out int statusCode)
        {
            string script = ExtractJsonStringValue(body, "script");
            if (string.IsNullOrEmpty(script))
            {
                statusCode = 400;
                responseBody = "{\"error\": \"Missing 'script' in request body.\"}";
                return;
            }

            if (WebVerseRuntime.Instance == null ||
                WebVerseRuntime.Instance.javascriptHandler == null)
            {
                statusCode = 503;
                responseBody = "{\"error\": \"JavaScript handler not initialized.\"}";
                return;
            }

            object result = WebVerseRuntime.Instance.javascriptHandler.Run(script);
            string resultStr = result != null ? result.ToString() : "null";

            statusCode = 200;
            responseBody = "{\"result\": \"" + AutomationServer.EscapeJson(resultStr) + "\"}";
        }

        /// <summary>
        /// Handle GET /screenshot — captures the current frame as a PNG image.
        /// </summary>
        private void HandleScreenshot(out string responseBody, out string responseContentType,
            out byte[] responseBytes, out int statusCode)
        {
            responseBody = "";

            try
            {
                // Capture the screen.
                Texture2D screenTexture = ScreenCapture.CaptureScreenshotAsTexture();
                if (screenTexture == null)
                {
                    statusCode = 500;
                    responseContentType = "application/json";
                    responseBody = "{\"error\": \"Failed to capture screenshot.\"}";
                    responseBytes = null;
                    return;
                }

                responseBytes = screenTexture.EncodeToPNG();
                UnityEngine.Object.Destroy(screenTexture);

                statusCode = 200;
                responseContentType = "image/png";
            }
            catch (Exception e)
            {
                statusCode = 500;
                responseContentType = "application/json";
                responseBody = "{\"error\": \"Screenshot failed: "
                    + AutomationServer.EscapeJson(e.Message) + "\"}";
                responseBytes = null;
            }
        }

        /// <summary>
        /// Handle POST /quit — shuts down the application.
        /// </summary>
        private void HandleQuit(out string responseBody, out int statusCode)
        {
            statusCode = 200;
            responseBody = "{\"status\": \"shutting_down\"}";

            // Schedule quit for next frame to allow the response to be sent.
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Serialize a brief summary of an entity to JSON.
        /// </summary>
        private string SerializeEntitySummary(BaseEntity entity)
        {
            if (entity == null) return "null";

            Vector3 pos = Vector3.zero;
            try { pos = entity.GetPosition(false); } catch { }

            Vector3 rot = Vector3.zero;
            try { rot = entity.GetEulerRotation(false); } catch { }

            return "{\"id\": \"" + entity.id.ToString()
                + "\", \"type\": \"" + AutomationServer.EscapeJson(entity.GetType().Name)
                + "\", \"tag\": " + (entity.entityTag != null
                    ? "\"" + AutomationServer.EscapeJson(entity.entityTag) + "\""
                    : "null")
                + ", \"position\": {\"x\": " + pos.x + ", \"y\": " + pos.y
                    + ", \"z\": " + pos.z + "}"
                + ", \"rotation\": {\"x\": " + rot.x + ", \"y\": " + rot.y
                    + ", \"z\": " + rot.z + "}"
                + ", \"visible\": " + (entity.GetVisibility() ? "true" : "false")
                + "}";
        }

        /// <summary>
        /// Serialize detailed information about an entity to JSON.
        /// </summary>
        private string SerializeEntityDetail(BaseEntity entity)
        {
            if (entity == null) return "{\"error\": \"Entity is null.\"}";

            Vector3 pos = Vector3.zero;
            try { pos = entity.GetPosition(false); } catch { }

            Vector3 rot = Vector3.zero;
            try { rot = entity.GetEulerRotation(false); } catch { }

            Vector3 scale = Vector3.one;
            try { scale = entity.GetScale(); } catch { }

            string interactionState = entity.GetInteractionState().ToString();

            // Get children.
            StringBuilder childrenSb = new StringBuilder("[");
            try
            {
                BaseEntity[] children = entity.GetChildren();
                for (int i = 0; i < children.Length; i++)
                {
                    if (i > 0) childrenSb.Append(", ");
                    childrenSb.Append("\"" + children[i].id.ToString() + "\"");
                }
            }
            catch { }
            childrenSb.Append("]");

            // Get parent.
            string parentId = "null";
            try
            {
                BaseEntity parent = entity.GetParent();
                if (parent != null)
                {
                    parentId = "\"" + parent.id.ToString() + "\"";
                }
            }
            catch { }

            return "{\"id\": \"" + entity.id.ToString()
                + "\", \"type\": \"" + AutomationServer.EscapeJson(entity.GetType().Name)
                + "\", \"tag\": " + (entity.entityTag != null
                    ? "\"" + AutomationServer.EscapeJson(entity.entityTag) + "\""
                    : "null")
                + ", \"position\": {\"x\": " + pos.x + ", \"y\": " + pos.y
                    + ", \"z\": " + pos.z + "}"
                + ", \"rotation\": {\"x\": " + rot.x + ", \"y\": " + rot.y
                    + ", \"z\": " + rot.z + "}"
                + ", \"scale\": {\"x\": " + scale.x + ", \"y\": " + scale.y
                    + ", \"z\": " + scale.z + "}"
                + ", \"visible\": " + (entity.GetVisibility() ? "true" : "false")
                + ", \"interactionState\": \"" + interactionState + "\""
                + ", \"parent\": " + parentId
                + ", \"children\": " + childrenSb.ToString()
                + "}";
        }

        /// <summary>
        /// Convert a RuntimeState enum to a string representation.
        /// </summary>
        private string GetRuntimeStateString(WebVerseRuntime.RuntimeState state)
        {
            switch (state)
            {
                case WebVerseRuntime.RuntimeState.Unloaded:
                    return "unloaded";
                case WebVerseRuntime.RuntimeState.LoadingWorld:
                    return "loading";
                case WebVerseRuntime.RuntimeState.LoadedWorld:
                    return "loaded";
                case WebVerseRuntime.RuntimeState.WebPage:
                    return "webpage";
                case WebVerseRuntime.RuntimeState.Error:
                default:
                    return "error";
            }
        }

        /// <summary>
        /// Simple JSON string value extractor. Extracts the value for a given key
        /// from a simple JSON object. Does not handle nested objects.
        /// </summary>
        /// <param name="json">JSON string.</param>
        /// <param name="key">Key to extract.</param>
        /// <returns>The value, or null if not found.</returns>
        private string ExtractJsonStringValue(string json, string key)
        {
            if (string.IsNullOrEmpty(json)) return null;

            string searchKey = "\"" + key + "\"";
            int keyIndex = json.IndexOf(searchKey);
            if (keyIndex < 0) return null;

            int colonIndex = json.IndexOf(':', keyIndex + searchKey.Length);
            if (colonIndex < 0) return null;

            // Find the start of the value (skip whitespace).
            int valueStart = colonIndex + 1;
            while (valueStart < json.Length && json[valueStart] == ' ')
            {
                valueStart++;
            }

            if (valueStart >= json.Length) return null;

            // Check if the value is a string (starts with a quote).
            if (json[valueStart] == '"')
            {
                int valueEnd = json.IndexOf('"', valueStart + 1);
                if (valueEnd < 0) return null;
                return json.Substring(valueStart + 1, valueEnd - valueStart - 1);
            }

            // Otherwise, extract until comma, closing brace, or end of string.
            int end = valueStart;
            while (end < json.Length && json[end] != ',' && json[end] != '}')
            {
                end++;
            }
            return json.Substring(valueStart, end - valueStart).Trim();
        }
    }
}
