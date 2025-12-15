// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Handlers.File;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;
using OMI;
using OMI.Integration;
#if NEWTONSOFT_JSON
using Newtonsoft.Json.Linq;
#endif
#if USE_WEBINTERFACE
using FiveSQD.WebVerse.WebInterface.HTTP;
#endif

namespace FiveSQD.WebVerse.Handlers.OMI
{
    /// <summary>
    /// Handler for loading and processing OMI-enabled glTF/GLB documents as worlds.
    /// </summary>
    public class OMIHandler : BaseHandler
    {
        /// <summary>
        /// Reference to the WebVerse runtime.
        /// </summary>
        public WebVerseRuntime runtime;

        /// <summary>
        /// Timeout for HTTP requests in seconds.
        /// </summary>
        public float timeout = 30f;

        /// <summary>
        /// Runtime settings for the handler.
        /// </summary>
        private OMIHandlerSettings settings = new OMIHandlerSettings();

        /// <summary>
        /// Gets the current settings.
        /// </summary>
        public OMIHandlerSettings Settings => settings;

        /// <summary>
        /// The world adapter used by this handler.
        /// </summary>
        private StraightFourOMIAdapter worldAdapter;

        /// <summary>
        /// Registry for spawn points in the current world.
        /// </summary>
        private SpawnPointRegistry spawnPointRegistry;

        /// <summary>
        /// Gets the spawn point registry for the current world.
        /// </summary>
        public SpawnPointRegistry SpawnPoints => spawnPointRegistry;

        /// <summary>
        /// Initialize the OMI Handler with default settings.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            spawnPointRegistry = new SpawnPointRegistry();
            
            // Create and initialize the world adapter
            worldAdapter = gameObject.AddComponent<StraightFourOMIAdapter>();
            worldAdapter.Initialize(WebVerseRuntime.Instance, spawnPointRegistry);

            Logging.Log("[OMIHandler] Initialized.");
        }

        /// <summary>
        /// Initialize the OMI Handler with specific settings.
        /// </summary>
        /// <param name="runtimeSettings">Settings to use for this handler instance.</param>
        public void Initialize(OMIHandlerSettings runtimeSettings)
        {
            settings = runtimeSettings ?? new OMIHandlerSettings();
            Initialize();
        }

        /// <summary>
        /// Terminate the OMI Handler.
        /// </summary>
        public override void Terminate()
        {
            if (worldAdapter != null)
            {
                worldAdapter.Cleanup();
                Destroy(worldAdapter);
                worldAdapter = null;
            }

            spawnPointRegistry?.Clear();
            spawnPointRegistry = null;

            base.Terminate();
            Logging.Log("[OMIHandler] Terminated.");
        }

        /// <summary>
        /// Reset the handler state for loading a new world.
        /// </summary>
        public void Reset()
        {
            spawnPointRegistry?.Clear();
            worldAdapter?.Cleanup();
            worldAdapter?.Initialize(WebVerseRuntime.Instance, spawnPointRegistry);
        }

        /// <summary>
        /// Get the title/name of a glTF world.
        /// </summary>
        /// <param name="resourceURI">URI of the glTF file.</param>
        /// <param name="onComplete">Callback with the world title.</param>
        public void GetWorldTitle(string resourceURI, Action<string> onComplete)
        {
            StartCoroutine(GetWorldTitleCoroutine(resourceURI, onComplete));
        }

        /// <summary>
        /// Load an OMI/glTF document into the current world.
        /// </summary>
        /// <param name="resourceURI">URI of the glTF file (http/https or local file).</param>
        /// <param name="onComplete">Callback when loading is complete. Provides success/failure.</param>
        public void LoadOMIDocumentIntoWorld(string resourceURI, Action<bool> onComplete)
        {
            if (worldAdapter == null)
            {
                Logging.LogError("[OMIHandler] World adapter not initialized.");
                onComplete?.Invoke(false);
                return;
            }

            Logging.Log($"[OMIHandler] Loading OMI/glTF from: {resourceURI}");

            StartCoroutine(LoadOMIDocumentCoroutine(resourceURI, onComplete));
        }

        /// <summary>
        /// Coroutine to get the world title from a glTF file.
        /// </summary>
        private IEnumerator GetWorldTitleCoroutine(string resourceURI, Action<string> onComplete)
        {
            string defaultTitle = "glTF World";

            // Determine if this is a remote or local file
            bool isRemote = resourceURI.StartsWith("http://") || resourceURI.StartsWith("https://");

            byte[] data = null;
            string localPath = null;

            if (isRemote)
            {
#if USE_WEBINTERFACE
                bool downloadComplete = false;

                Action<int, System.Collections.Generic.Dictionary<string, string>, byte[]> onDownloaded =
                    (code, headers, responseData) =>
                    {
                        downloadComplete = true;
                        if (code >= 200 && code < 300 && responseData != null)
                        {
                            data = responseData;
                        }
                    };

                HTTPRequest request = new HTTPRequest(resourceURI, HTTPRequest.HTTPMethod.Get, onDownloaded);
                request.Send();

                float elapsed = 0f;
                while (!downloadComplete && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (!downloadComplete || data == null)
                {
                    onComplete?.Invoke(defaultTitle);
                    yield break;
                }
#else
                onComplete?.Invoke(defaultTitle);
                yield break;
#endif
            }
            else
            {
                // Local file
                localPath = ResolveLocalPath(resourceURI);
                if (string.IsNullOrEmpty(localPath) || !System.IO.File.Exists(localPath))
                {
                    onComplete?.Invoke(defaultTitle);
                    yield break;
                }

                try
                {
                    data = System.IO.File.ReadAllBytes(localPath);
                }
                catch (Exception ex)
                {
                    Logging.LogWarning($"[OMIHandler] Error reading file for title: {ex.Message}");
                    onComplete?.Invoke(defaultTitle);
                    yield break;
                }
            }

            // Try to extract title from glTF
            // For now, use the filename as the title
            // TODO: Parse glTF JSON to extract scene name or extras.title
            string title = ExtractTitleFromURI(resourceURI);
            onComplete?.Invoke(title);
        }

        /// <summary>
        /// Coroutine to load an OMI/glTF document.
        /// </summary>
        private IEnumerator LoadOMIDocumentCoroutine(string resourceURI, Action<bool> onComplete)
        {
            bool isRemote = resourceURI.StartsWith("http://") || resourceURI.StartsWith("https://");
            string baseUrl = GetBaseUrl(resourceURI);

            if (isRemote)
            {
                yield return StartCoroutine(LoadFromRemote(resourceURI, baseUrl, onComplete));
            }
            else
            {
                yield return StartCoroutine(LoadFromLocal(resourceURI, baseUrl, onComplete));
            }
        }

        /// <summary>
        /// Load glTF from a remote URL.
        /// </summary>
        private IEnumerator LoadFromRemote(string url, string baseUrl, Action<bool> onComplete)
        {
#if USE_WEBINTERFACE
            // First, download the file
            bool downloadComplete = false;
            byte[] data = null;

            Action<int, System.Collections.Generic.Dictionary<string, string>, byte[]> onDownloaded =
                (code, headers, responseData) =>
                {
                    downloadComplete = true;
                    if (code >= 200 && code < 300 && responseData != null)
                    {
                        data = responseData;
                    }
                    else
                    {
                        Logging.LogError($"[OMIHandler] Failed to download glTF. HTTP {code}");
                    }
                };

            HTTPRequest request = new HTTPRequest(url, HTTPRequest.HTTPMethod.Get, onDownloaded);
            request.Send();

            // Wait for download
            float elapsed = 0f;
            while (!downloadComplete && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!downloadComplete)
            {
                Logging.LogError("[OMIHandler] glTF download timed out.");
                onComplete?.Invoke(false);
                yield break;
            }

            if (data == null)
            {
                onComplete?.Invoke(false);
                yield break;
            }

            // Load from the downloaded data
            yield return StartCoroutine(LoadFromData(data, new Uri(url), onComplete));
#else
            Logging.LogError("[OMIHandler] Web interface not available for remote loading.");
            onComplete?.Invoke(false);
            yield break;
#endif
        }

        /// <summary>
        /// Load glTF from a local file.
        /// </summary>
        private IEnumerator LoadFromLocal(string filePath, string baseUrl, Action<bool> onComplete)
        {
            string resolvedPath = ResolveLocalPath(filePath);

            if (string.IsNullOrEmpty(resolvedPath) || !System.IO.File.Exists(resolvedPath))
            {
                Logging.LogError($"[OMIHandler] File not found: {filePath}");
                onComplete?.Invoke(false);
                yield break;
            }

            byte[] data = null;
            try
            {
                data = System.IO.File.ReadAllBytes(resolvedPath);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[OMIHandler] Error reading file: {ex.Message}");
                onComplete?.Invoke(false);
                yield break;
            }

            if (data == null || data.Length == 0)
            {
                Logging.LogError("[OMIHandler] File is empty.");
                onComplete?.Invoke(false);
                yield break;
            }

            Uri baseUri = new Uri("file:///" + resolvedPath.Replace("\\", "/"));
            yield return StartCoroutine(LoadFromData(data, baseUri, onComplete));
        }

        /// <summary>
        /// Load glTF from byte data using OMI loader with StraightFour handlers.
        /// </summary>
        private IEnumerator LoadFromData(byte[] data, Uri baseUri, Action<bool> onComplete)
        {
            Logging.Log($"[OMIHandler] Starting LoadFromData with {data.Length} bytes, baseUri: {baseUri}");
            
#if NEWTONSOFT_JSON
            yield return StartCoroutine(LoadFromDataNewtonsoft(data, baseUri, onComplete));
#else
            Logging.LogError("[OMIHandler] NEWTONSOFT_JSON is required for OMI extension support. Please add the define symbol.");
            onComplete?.Invoke(false);
            yield break;
#endif
        }

#if NEWTONSOFT_JSON
        /// <summary>
        /// Load glTF using OMIDirectJsonLoader with full extension support.
        /// </summary>
        private IEnumerator LoadFromDataNewtonsoft(byte[] data, Uri baseUri, Action<bool> onComplete)
        {
            // Create a parent container for the loaded content
            GameObject parentContainer = new GameObject("OMI_LoadedWorld");
            
            // Get the extension manager from the adapter with custom StraightFour handlers
            OMIExtensionManager extensionManager = worldAdapter.CreateExtensionManager();
            
            // Create import settings
            var importSettings = new OMIImportSettings
            {
                VerboseLogging = Settings.verboseLogging,
                ImportPhysicsShapes = true,
                ImportPhysicsBodies = true,
                ImportPhysicsJoints = true,
                ImportPhysicsGravity = true,
                ImportSpawnPoints = true,
                ImportSeats = true,
                ImportLinks = true,
                ImportAudioEmitters = true,
                ImportPersonality = true,
                ImportVehicleBodies = true,
                ImportVehicleWheels = true
            };
            
            // Create the OMI loader
            OMIDirectJsonLoader loader = null;
            try
            {
                loader = new OMIDirectJsonLoader(extensionManager, importSettings);
            }
            catch (Exception ex)
            {
                Destroy(parentContainer);
                Logging.LogError($"[OMIHandler] Failed to create OMIDirectJsonLoader: {ex.Message}\n{ex.StackTrace}");
                onComplete?.Invoke(false);
                yield break;
            }

            using (loader)
            {
                // Store runtime and spawn point registry in context (will be available after load)
                // Note: Context is created during LoadAsync
                
                // Load the glTF data
                Logging.Log("[OMIHandler] Starting glTF load with Newtonsoft...");
                Task<bool> loadTask = null;
                try
                {
                    loadTask = loader.LoadAsync(data, baseUri);
                }
                catch (Exception ex)
                {
                    Destroy(parentContainer);
                    Logging.LogError($"[OMIHandler] Exception starting LoadAsync: {ex.Message}\n{ex.StackTrace}");
                    onComplete?.Invoke(false);
                    yield break;
                }

                while (!loadTask.IsCompleted)
                {
                    yield return null;
                }

                if (loadTask.IsFaulted)
                {
                    Destroy(parentContainer);
                    string errorMsg = loadTask.Exception?.InnerException?.Message ?? loadTask.Exception?.Message ?? "Unknown error";
                    Logging.LogError($"[OMIHandler] Failed to load glTF (faulted): {errorMsg}");
                    if (loadTask.Exception != null)
                    {
                        Logging.LogError($"[OMIHandler] Stack trace: {loadTask.Exception.StackTrace}");
                    }
                    onComplete?.Invoke(false);
                    yield break;
                }

                if (!loadTask.Result)
                {
                    Destroy(parentContainer);
                    Logging.LogError("[OMIHandler] LoadAsync returned false - glTF parsing failed");
                    onComplete?.Invoke(false);
                    yield break;
                }

                Logging.Log("[OMIHandler] glTF loaded successfully, instantiating...");
                
                // Store runtime and registry in context for handlers to access
                if (loader.Context != null)
                {
                    loader.Context.CustomData[StraightFourCustomDataKeys.Runtime] = WebVerseRuntime.Instance;
                    loader.Context.CustomData[StraightFourCustomDataKeys.SpawnPointRegistry] = spawnPointRegistry;
                }

                // Instantiate the scene with parent container
                // This also processes all OMI extensions via the registered handlers
                Task<GameObject> instantiateTask = null;
                try
                {
                    instantiateTask = loader.InstantiateAsync(parentContainer.transform);
                }
                catch (Exception ex)
                {
                    Destroy(parentContainer);
                    Logging.LogError($"[OMIHandler] Exception starting InstantiateAsync: {ex.Message}\n{ex.StackTrace}");
                    onComplete?.Invoke(false);
                    yield break;
                }

                while (!instantiateTask.IsCompleted)
                {
                    yield return null;
                }

                if (instantiateTask.IsFaulted)
                {
                    Destroy(parentContainer);
                    string errorMsg = instantiateTask.Exception?.InnerException?.Message ?? instantiateTask.Exception?.Message ?? "Unknown error";
                    Logging.LogError($"[OMIHandler] Failed to instantiate glTF (faulted): {errorMsg}");
                    if (instantiateTask.Exception != null)
                    {
                        Logging.LogError($"[OMIHandler] Stack trace: {instantiateTask.Exception.StackTrace}");
                    }
                    onComplete?.Invoke(false);
                    yield break;
                }

                if (instantiateTask.Result == null)
                {
                    Destroy(parentContainer);
                    Logging.LogError("[OMIHandler] InstantiateAsync returned null");
                    onComplete?.Invoke(false);
                    yield break;
                }

                GameObject rootObject = instantiateTask.Result;
                Logging.Log($"[OMIHandler] Instantiated root object: {rootObject.name}");

                // Post-processing (entity creation from context, if needed)
                yield return StartCoroutine(worldAdapter.PostProcessScene(rootObject, loader.Context));

                // Apply spawn point if available
                ApplySpawnPoint();

                Logging.Log("[OMIHandler] glTF world loaded successfully with Newtonsoft loader.");
                onComplete?.Invoke(true);
            }
        }
#endif

        /// <summary>
        /// Apply spawn point to player based on settings.
        /// </summary>
        private void ApplySpawnPoint()
        {
            if (!Settings.enableSpawnPoints || spawnPointRegistry.Count == 0)
            {
                return;
            }

            var spawnPoint = spawnPointRegistry.GetSpawnPoint(
                Settings.spawnMode,
                Settings.spawnTeam,
                Settings.spawnPointName);

            if (spawnPoint != null)
            {
                // TODO: Apply spawn point to player/camera
                // This will depend on how WebVerse handles player positioning
                Logging.Log($"[OMIHandler] Spawn point available: {spawnPoint.Title ?? "(unnamed)"} at {spawnPoint.Position}");
                
                // Example: If there's a way to set initial camera/player position:
                // runtime.SetPlayerPosition(spawnPoint.Position, spawnPoint.Rotation);
            }
        }

        /// <summary>
        /// Resolve a file path to an absolute path.
        /// </summary>
        private string ResolveLocalPath(string filePath)
        {
            // Strip file:// prefix if present
            if (filePath.StartsWith("file://"))
            {
                filePath = filePath.Substring(7);
            }

            // Try the file handler directory first (where downloaded files are cached)
            if (runtime != null && runtime.fileHandler != null)
            {
                string fileHandlerPath = Path.Combine(
                    runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(filePath));
                
                if (System.IO.File.Exists(fileHandlerPath))
                {
                    return fileHandlerPath;
                }
            }

            // Try as absolute path
            if (System.IO.File.Exists(filePath))
            {
                return filePath;
            }

            return null;
        }

        /// <summary>
        /// Get the base URL from a resource URI.
        /// </summary>
        private string GetBaseUrl(string resourceURI)
        {
            if (string.IsNullOrEmpty(resourceURI))
                return "";

            // For URLs
            if (resourceURI.StartsWith("http://") || resourceURI.StartsWith("https://"))
            {
                int lastSlash = resourceURI.LastIndexOf('/');
                if (lastSlash > 8)
                {
                    return resourceURI.Substring(0, lastSlash + 1);
                }
            }

            // For file paths
            return Path.GetDirectoryName(resourceURI) ?? "";
        }

        /// <summary>
        /// Extract a title from a URI (uses filename without extension).
        /// </summary>
        private string ExtractTitleFromURI(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return "glTF World";

            try
            {
                string filename = Path.GetFileNameWithoutExtension(uri);
                if (!string.IsNullOrEmpty(filename))
                {
                    // Convert underscores/hyphens to spaces and title case
                    filename = filename.Replace("_", " ").Replace("-", " ");
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(filename.ToLower());
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return "glTF World";
        }

        /// <summary>
        /// Parse JSON from glTF/GLB byte data.
        /// </summary>
        /// <param name="data">Raw glTF or GLB data.</param>
        /// <returns>Parsed JObject or null if parsing failed.</returns>
#if NEWTONSOFT_JSON
        private JObject ParseGltfJson(byte[] data)
        {
            try
            {
                if (data == null || data.Length < 4)
                    return null;

                // Check for GLB magic number (0x46546C67 = "glTF" in little-endian)
                if (data.Length >= 12 && 
                    data[0] == 0x67 && data[1] == 0x6C && data[2] == 0x54 && data[3] == 0x46)
                {
                    // GLB format - parse binary container
                    return ParseGlbJson(data);
                }
                else
                {
                    // Assume plain JSON glTF
                    var jsonString = System.Text.Encoding.UTF8.GetString(data);
                    return JObject.Parse(jsonString);
                }
            }
            catch (Exception ex)
            {
                Logging.LogWarning($"[OMIHandler] Failed to parse glTF JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse JSON from a GLB binary file.
        /// </summary>
        /// <param name="data">Raw GLB data.</param>
        /// <returns>Parsed JObject or null if parsing failed.</returns>
        private JObject ParseGlbJson(byte[] data)
        {
            // GLB header: magic (4) + version (4) + length (4) = 12 bytes
            // Chunk header: length (4) + type (4) = 8 bytes
            // First chunk is JSON (type 0x4E4F534A = "JSON")
            
            if (data.Length < 20)
                return null;

            int jsonChunkLength = BitConverter.ToInt32(data, 12);
            uint jsonChunkType = BitConverter.ToUInt32(data, 16);

            // Verify it's a JSON chunk (0x4E4F534A)
            if (jsonChunkType != 0x4E4F534A)
            {
                Logging.LogWarning("[OMIHandler] GLB first chunk is not JSON");
                return null;
            }

            if (data.Length < 20 + jsonChunkLength)
            {
                Logging.LogWarning("[OMIHandler] GLB data truncated");
                return null;
            }

            var jsonString = System.Text.Encoding.UTF8.GetString(data, 20, jsonChunkLength);
            return JObject.Parse(jsonString);
        }
#endif
    }
}
