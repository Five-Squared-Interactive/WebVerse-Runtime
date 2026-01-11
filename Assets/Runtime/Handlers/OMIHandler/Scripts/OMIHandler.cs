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
using OMIPhysicsShapeRoot = global::OMI.Extensions.PhysicsShape.OMIPhysicsShapeRoot;
using OMIPhysicsBodyRoot = global::OMI.Extensions.PhysicsBody.OMIPhysicsBodyRoot;
using OMIPhysicsBodyNode = global::OMI.Extensions.PhysicsBody.OMIPhysicsBodyNode;
using OMIPhysicsJointRoot = global::OMI.Extensions.PhysicsJoint.OMIPhysicsJointRoot;
using OMIPhysicsJointNode = global::OMI.Extensions.PhysicsJoint.OMIPhysicsJointNode;
using OMIPhysicsGravityRoot = global::OMI.Extensions.PhysicsGravity.OMIPhysicsGravityRoot;
using OMIPhysicsGravityNode = global::OMI.Extensions.PhysicsGravity.OMIPhysicsGravityNode;
using OMISpawnPointNode = global::OMI.Extensions.SpawnPoint.OMISpawnPointNode;
using OMISeatNode = global::OMI.Extensions.Seat.OMISeatNode;
using OMILinkNode = global::OMI.Extensions.Link.OMILinkNode;
using OMIPersonalityNode = global::OMI.Extensions.Personality.OMIPersonalityNode;
using OMIVehicleBodyNode = global::OMI.Extensions.Vehicle.OMIVehicleBodyNode;
using OMIVehicleThrusterNode = global::OMI.Extensions.Vehicle.OMIVehicleThrusterNode;
using OMIVehicleThrusterRoot = global::OMI.Extensions.Vehicle.OMIVehicleThrusterRoot;
using OMIVehicleWheelNode = global::OMI.Extensions.Vehicle.OMIVehicleWheelNode;
using OMIVehicleWheelRoot = global::OMI.Extensions.Vehicle.OMIVehicleWheelRoot;
using OMIVehicleHoverThrusterNode = global::OMI.Extensions.Vehicle.OMIVehicleHoverThrusterNode;
using OMIEnvironmentSkyDocumentData = global::OMI.Extensions.EnvironmentSky.OMIEnvironmentSkyDocumentData;
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
        /// Load glTF using custom loader with proper glTFast settings and OMI extension support.
        /// </summary>
        private IEnumerator LoadFromDataNewtonsoft(byte[] data, Uri baseUri, Action<bool> onComplete)
        {
            // Create a parent container for the loaded content
            GameObject parentContainer = new GameObject("OMI_LoadedWorld");
            
            // Get the extension manager from the adapter with custom StraightFour handlers
            OMIExtensionManager extensionManager = worldAdapter.CreateExtensionManager();
            
            // Create OMI import settings
            var omiSettings = new OMIImportSettings
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
                ImportVehicleWheels = true,
                ImportVehicleThrusters = true,
                ImportVehicleHoverThrusters = true
            };
            
            // Create glTFast import with proper settings (matching GLTFLoader)
            GLTFast.GltfImport gltfImport = new GLTFast.GltfImport();
            
            // Load with proper import settings
            Logging.Log("[OMIHandler] Starting glTF load with proper ImportSettings...");
            Task<bool> loadTask = null;
            try
            {
                loadTask = gltfImport.Load(data, baseUri, new GLTFast.ImportSettings()
                {
                    GenerateMipMaps = true,
                    NodeNameMethod = GLTFast.NameImportMethod.OriginalUnique
                });
            }
            catch (Exception ex)
            {
                Destroy(parentContainer);
                Logging.LogError($"[OMIHandler] Exception starting glTF LoadAsync: {ex.Message}\n{ex.StackTrace}");
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
                onComplete?.Invoke(false);
                yield break;
            }

            if (!loadTask.Result)
            {
                Destroy(parentContainer);
                Logging.LogError("[OMIHandler] glTF LoadAsync returned false - parsing failed");
                onComplete?.Invoke(false);
                yield break;
            }

            Logging.Log("[OMIHandler] glTF loaded successfully, instantiating...");
            
            // Instantiate the scene
            Task<bool> instantiateTask = null;
            try
            {
                instantiateTask = gltfImport.InstantiateMainSceneAsync(parentContainer.transform);
            }
            catch (Exception ex)
            {
                Destroy(parentContainer);
                Logging.LogError($"[OMIHandler] Exception starting InstantiateMainSceneAsync: {ex.Message}\n{ex.StackTrace}");
                onComplete?.Invoke(false);
                yield break;
            }

            while (!instantiateTask.IsCompleted)
            {
                yield return null;
            }

            if (instantiateTask.IsFaulted || !instantiateTask.Result)
            {
                Destroy(parentContainer);
                string errorMsg = instantiateTask.Exception?.InnerException?.Message ?? instantiateTask.Exception?.Message ?? "Instantiation failed";
                Logging.LogError($"[OMIHandler] Failed to instantiate glTF: {errorMsg}");
                onComplete?.Invoke(false);
                yield break;
            }

            // Find the instantiated root object
            GameObject rootObject = null;
            if (parentContainer.transform.childCount > 0)
            {
                rootObject = parentContainer.transform.GetChild(parentContainer.transform.childCount - 1).gameObject;
            }

            if (rootObject == null)
            {
                Destroy(parentContainer);
                Logging.LogError("[OMIHandler] Could not find instantiated root object");
                onComplete?.Invoke(false);
                yield break;
            }

            Logging.Log($"[OMIHandler] Instantiated root object: {rootObject.name}");

            // --- DEBUG: Print Unity hierarchy after import ---
            PrintHierarchy(rootObject.transform, 0);

            // Parse OMI extensions from the glTF JSON
            JObject jsonRoot = OMIJsonExtensions.ParseGltfData(data);
            if (jsonRoot == null)
            {
                Logging.LogWarning("[OMIHandler] Could not parse glTF JSON for OMI extensions, continuing without extensions");
            }
            
            // Create OMI import context with our properly-configured glTFast import
            OMIImportContext context = new OMIImportContext(gltfImport, extensionManager, omiSettings);
            context.RootObject = rootObject;
            context.BuildNodeMapping(rootObject);

            // Build and store node parent mapping for handler use
            if (jsonRoot != null)
            {
                var parentMap = FiveSQD.WebVerse.Handlers.OMI.StraightFour.StraightFourOMIAdapter.BuildNodeParentMapping(jsonRoot);
                context.CustomData["SF_NodeParentIndices"] = parentMap;
            }

            // Store runtime and registry in context for handlers to access
            context.CustomData[StraightFourCustomDataKeys.Runtime] = WebVerseRuntime.Instance;
            context.CustomData[StraightFourCustomDataKeys.SpawnPointRegistry] = spawnPointRegistry;

            // Process OMI extensions if JSON was parsed successfully
            if (jsonRoot != null)
            {
                yield return StartCoroutine(ProcessOMIExtensionsCoroutine(jsonRoot, context, extensionManager, omiSettings));
            }

            // Post-processing (entity creation from context, if needed)
            yield return StartCoroutine(worldAdapter.PostProcessScene(rootObject, context));

            // --- DEBUG: Print Unity hierarchy after OMI/entity processing ---
            PrintHierarchy(rootObject.transform, 0);

            // Apply spawn point if available
            ApplySpawnPoint();

            Logging.Log("[OMIHandler] glTF world loaded successfully.");
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Process OMI extensions from parsed JSON.
        /// </summary>
        private IEnumerator ProcessOMIExtensionsCoroutine(JObject root, OMIImportContext context, OMIExtensionManager extensionManager, OMIImportSettings settings)
        {
            Logging.Log("[OMIHandler] Processing OMI extensions...");

            // Process document-level extensions
            yield return StartCoroutine(ProcessDocumentExtensionsCoroutine(root, context, extensionManager, settings));

            // Process node-level extensions
            yield return StartCoroutine(ProcessNodeExtensionsCoroutine(root, context, extensionManager, settings));

            // Execute any deferred actions (e.g., resolving cross-references like pilotSeat)
            context.ExecuteDeferredActions();
        }

        private IEnumerator ProcessDocumentExtensionsCoroutine(JObject root, OMIImportContext context, OMIExtensionManager extensionManager, OMIImportSettings settings)
        {
            Logging.Log($"[OMIHandler] ProcessDocumentExtensionsCoroutine: ImportPhysicsShapes={settings.ImportPhysicsShapes}");
            
            // Process OMI_physics_shape document extension
            if (settings.ImportPhysicsShapes)
            {
                Logging.Log("[OMIHandler] Trying to get OMI_physics_shape document extension...");
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIPhysicsShapeRoot>(root, OMIJsonExtensions.ExtensionNames.PhysicsShape, out var shapesRoot))
                {
                    Logging.Log($"[OMIHandler] Found OMI_physics_shape with {shapesRoot?.Shapes?.Length ?? 0} shapes");
                    
                    // Find document handler by iterating (since document and node handlers share extension name)
                    IOMIDocumentExtensionHandler<OMIPhysicsShapeRoot> handler = null;
                    var docHandlers = extensionManager.GetDocumentHandlers();
                    for (int i = 0; i < docHandlers.Count; i++)
                    {
                        if (docHandlers[i] is IOMIDocumentExtensionHandler<OMIPhysicsShapeRoot> typedHandler)
                        {
                            handler = typedHandler;
                            break;
                        }
                    }
                    
                    Logging.Log($"[OMIHandler] Handler for OMI_physics_shape: {handler?.GetType().Name ?? "NULL"}");
                    if (handler != null)
                    {
                        var task = handler.OnDocumentImportAsync(shapesRoot, context, default);
                        while (!task.IsCompleted) yield return null;
                        Logging.Log("[OMIHandler] OMI_physics_shape document handler completed");
                    }
                }
                else
                {
                    Logging.Log("[OMIHandler] No OMI_physics_shape document extension found");
                }
            }

            // Process OMI_physics_body document extension
            if (settings.ImportPhysicsBodies)
            {
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIPhysicsBodyRoot>(root, OMIJsonExtensions.ExtensionNames.PhysicsBody, out var bodyRoot))
                {
                    var handler = extensionManager.GetHandler<OMIPhysicsBodyRoot>(OMIJsonExtensions.ExtensionNames.PhysicsBody);
                    if (handler != null)
                    {
                        var task = handler.OnImportAsync(bodyRoot, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_physics_joint document extension
            if (settings.ImportPhysicsJoints)
            {
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIPhysicsJointRoot>(root, OMIJsonExtensions.ExtensionNames.PhysicsJoint, out var jointRoot))
                {
                    var handler = extensionManager.GetHandler<OMIPhysicsJointRoot>(OMIJsonExtensions.ExtensionNames.PhysicsJoint);
                    if (handler != null)
                    {
                        var task = handler.OnImportAsync(jointRoot, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_physics_gravity document extension
            if (settings.ImportPhysicsGravity)
            {
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIPhysicsGravityRoot>(root, OMIJsonExtensions.ExtensionNames.PhysicsGravity, out var gravityRoot))
                {
                    var handler = extensionManager.GetHandler<OMIPhysicsGravityRoot>(OMIJsonExtensions.ExtensionNames.PhysicsGravity);
                    if (handler != null)
                    {
                        var task = handler.OnImportAsync(gravityRoot, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_vehicle_thruster document extension
            if (settings.ImportVehicleThrusters)
            {
                Logging.Log($"[OMIHandler] Looking for OMI_vehicle_thruster document extension...");
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIVehicleThrusterRoot>(root, OMIJsonExtensions.ExtensionNames.VehicleThruster, out var thrusterRoot))
                {
                    Logging.Log($"[OMIHandler] Found OMI_vehicle_thruster document extension, thrusters count={thrusterRoot?.thrusters?.Length ?? 0}");
                    
                    // Find document handler by scanning all document handlers (since node handler uses same extension name)
                    IOMIDocumentExtensionHandler<OMIVehicleThrusterRoot> handler = null;
                    foreach (var h in extensionManager.GetDocumentHandlers())
                    {
                        if (h is IOMIDocumentExtensionHandler<OMIVehicleThrusterRoot> docHandler)
                        {
                            handler = docHandler;
                            break;
                        }
                    }
                    
                    Logging.Log($"[OMIHandler] Document handler for OMI_vehicle_thruster: {(handler != null ? handler.GetType().Name : "NULL")}");
                    if (handler != null)
                    {
                        var task = handler.OnImportAsync(thrusterRoot, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
                else
                {
                    Logging.Log($"[OMIHandler] OMI_vehicle_thruster document extension NOT found in glTF");
                }
            }
            else
            {
                Logging.Log($"[OMIHandler] ImportVehicleThrusters is disabled");
            }

            // Process OMI_vehicle_wheel document extension
            if (settings.ImportVehicleWheels)
            {
                Logging.Log($"[OMIHandler] Looking for OMI_vehicle_wheel document extension...");
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIVehicleWheelRoot>(root, OMIJsonExtensions.ExtensionNames.VehicleWheel, out var wheelRoot))
                {
                    Logging.Log($"[OMIHandler] Found OMI_vehicle_wheel document extension, wheels count={wheelRoot?.wheels?.Length ?? 0}");
                    
                    // Find document handler by scanning all document handlers (since node handler uses same extension name)
                    IOMIDocumentExtensionHandler<OMIVehicleWheelRoot> handler = null;
                    foreach (var h in extensionManager.GetDocumentHandlers())
                    {
                        if (h is IOMIDocumentExtensionHandler<OMIVehicleWheelRoot> docHandler)
                        {
                            handler = docHandler;
                            break;
                        }
                    }
                    
                    Logging.Log($"[OMIHandler] Document handler for OMI_vehicle_wheel: {(handler != null ? handler.GetType().Name : "NULL")}");
                    if (handler != null)
                    {
                        var task = handler.OnDocumentImportAsync(wheelRoot, context, default);
                        while (!task.IsCompleted) yield return null;
                        Logging.Log("[OMIHandler] OMI_vehicle_wheel document handler completed");
                    }
                }
                else
                {
                    Logging.Log($"[OMIHandler] OMI_vehicle_wheel document extension NOT found in glTF");
                }
            }
            else
            {
                Logging.Log($"[OMIHandler] ImportVehicleWheels is disabled");
            }

            // Process OMI_environment_sky document extension (always enabled)
            {
                Logging.Log($"[OMIHandler] Looking for OMI_environment_sky document extension...");
                if (OMIJsonExtensions.TryGetDocumentExtension<OMIEnvironmentSkyDocumentData>(root, OMIJsonExtensions.ExtensionNames.EnvironmentSky, out var skyRoot))
                {
                    Logging.Log($"[OMIHandler] Found OMI_environment_sky document extension, skies count={skyRoot?.Skies?.Count ?? 0}");
                    
                    // Find document handler by scanning all document handlers
                    IOMIDocumentExtensionHandler<OMIEnvironmentSkyDocumentData> handler = null;
                    foreach (var h in extensionManager.GetDocumentHandlers())
                    {
                        if (h is IOMIDocumentExtensionHandler<OMIEnvironmentSkyDocumentData> docHandler)
                        {
                            handler = docHandler;
                            break;
                        }
                    }
                    
                    Logging.Log($"[OMIHandler] Document handler for OMI_environment_sky: {(handler != null ? handler.GetType().Name : "NULL")}");
                    if (handler != null)
                    {
                        var task = handler.OnDocumentImportAsync(skyRoot, context, default);
                        while (!task.IsCompleted) yield return null;
                        Logging.Log("[OMIHandler] OMI_environment_sky document handler completed");
                    }
                }
                else
                {
                    Logging.Log($"[OMIHandler] OMI_environment_sky document extension NOT found in glTF");
                }
            }
        }

        private IEnumerator ProcessNodeExtensionsCoroutine(JObject root, OMIImportContext context, OMIExtensionManager extensionManager, OMIImportSettings settings)
        {
            var nodes = OMIJsonExtensions.GetNodes(root);
            if (nodes == null) yield break;

            Logging.Log($"[OMIHandler] ProcessNodeExtensionsCoroutine: Processing {nodes.Count} nodes");

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i] as JObject;
                if (node == null) continue;

                var gameObject = context.GetGameObject(i);
                if (gameObject == null)
                {
                    if (settings.VerboseLogging)
                    {
                        Logging.LogWarning($"[OMIHandler] No GameObject for node {i}");
                    }
                    continue;
                }

                // Log all extensions on this node
                var nodeExt = node["extensions"] as JObject;
                if (nodeExt != null)
                {
                    var extNames = new System.Collections.Generic.List<string>();
                    foreach (var prop in nodeExt.Properties())
                    {
                        extNames.Add(prop.Name);
                    }
                    Logging.Log($"[OMIHandler] Node {i} ({gameObject.name}) extensions: [{string.Join(", ", extNames)}]");
                    
                    // Log the raw physics body JSON if present
                    if (nodeExt.ContainsKey("OMI_physics_body"))
                    {
                        Logging.Log($"[OMIHandler] Node {i} OMI_physics_body raw: {nodeExt["OMI_physics_body"]}");
                    }
                }
                else
                {
                    Logging.Log($"[OMIHandler] Node {i} ({gameObject.name}) has no extensions");
                }

                yield return StartCoroutine(ProcessNodeExtensionCoroutine(node, i, gameObject, context, extensionManager, settings));
            }
        }

        private IEnumerator ProcessNodeExtensionCoroutine(JObject node, int nodeIndex, GameObject gameObject, OMIImportContext context, OMIExtensionManager extensionManager, OMIImportSettings settings)
        {
            // Check what extensions this node has
            var nodeExtensions = node["extensions"] as JObject;
            if (nodeExtensions != null && nodeExtensions.ContainsKey("OMI_physics_shape"))
            {
                Logging.Log($"[OMIHandler] Node {nodeIndex} ({gameObject?.name}) has OMI_physics_shape, ImportPhysicsShapes={settings.ImportPhysicsShapes}");
            }
            
            // Process OMI_physics_body node extension
            if (settings.ImportPhysicsBodies)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIPhysicsBodyNode>(node, OMIJsonExtensions.ExtensionNames.PhysicsBody, out var bodyNode))
                {
                    var handler = extensionManager.GetHandler<OMIPhysicsBodyNode>(OMIJsonExtensions.ExtensionNames.PhysicsBody) as IOMINodeExtensionHandler<OMIPhysicsBodyNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(bodyNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_physics_shape node extension (shape references on nodes for compound colliders)
            if (settings.ImportPhysicsShapes)
            {
                // First check if node has the extension at all
                var extensions = node["extensions"] as JObject;
                var hasShapeExt = extensions != null && extensions.ContainsKey(OMIJsonExtensions.ExtensionNames.PhysicsShape);
                if (hasShapeExt)
                {
                    Logging.Log($"[OMIHandler] Node {nodeIndex} ({gameObject.name}) has OMI_physics_shape extension: {extensions[OMIJsonExtensions.ExtensionNames.PhysicsShape]}");
                }
                
                if (OMIJsonExtensions.TryGetNodeExtension<FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers.OMIPhysicsShapeNodeRef>(node, OMIJsonExtensions.ExtensionNames.PhysicsShape, out var shapeNode))
                {
                    Logging.Log($"[OMIHandler] Found OMI_physics_shape on node {nodeIndex} (GameObject: {gameObject.name}), shape={shapeNode.Shape}");
                    
                    // Find node handler by iterating (since document and node handlers share extension name)
                    IOMINodeExtensionHandler<FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers.OMIPhysicsShapeNodeRef> handler = null;
                    var nodeHandlers = extensionManager.GetNodeHandlers();
                    Logging.Log($"[OMIHandler] Found {nodeHandlers.Count} node handlers");
                    for (int i = 0; i < nodeHandlers.Count; i++)
                    {
                        Logging.Log($"[OMIHandler] Node handler {i}: {nodeHandlers[i].GetType().Name}");
                        if (nodeHandlers[i] is IOMINodeExtensionHandler<FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers.OMIPhysicsShapeNodeRef> typedHandler)
                        {
                            handler = typedHandler;
                            Logging.Log($"[OMIHandler] Found matching handler: {typedHandler.GetType().Name}");
                            break;
                        }
                    }
                    
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(shapeNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                        Logging.Log($"[OMIHandler] Processed OMI_physics_shape on {gameObject.name}");
                    }
                    else
                    {
                        Logging.LogWarning($"[OMIHandler] No handler registered for OMI_physics_shape node extension");
                    }
                }
                else if (hasShapeExt)
                {
                    Logging.LogWarning($"[OMIHandler] Failed to parse OMI_physics_shape on node {nodeIndex}");
                }
            }

            // Process OMI_physics_joint node extension
            if (settings.ImportPhysicsJoints)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIPhysicsJointNode>(node, OMIJsonExtensions.ExtensionNames.PhysicsJoint, out var jointNode))
                {
                    var handler = extensionManager.GetHandler<OMIPhysicsJointNode>(OMIJsonExtensions.ExtensionNames.PhysicsJoint) as IOMINodeExtensionHandler<OMIPhysicsJointNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(jointNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_physics_gravity node extension
            if (settings.ImportPhysicsGravity)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIPhysicsGravityNode>(node, OMIJsonExtensions.ExtensionNames.PhysicsGravity, out var gravityNode))
                {
                    var handler = extensionManager.GetHandler<OMIPhysicsGravityNode>(OMIJsonExtensions.ExtensionNames.PhysicsGravity) as IOMINodeExtensionHandler<OMIPhysicsGravityNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(gravityNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_spawn_point node extension
            if (settings.ImportSpawnPoints)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMISpawnPointNode>(node, OMIJsonExtensions.ExtensionNames.SpawnPoint, out var spawnNode))
                {
                    var handler = extensionManager.GetHandler<OMISpawnPointNode>(OMIJsonExtensions.ExtensionNames.SpawnPoint) as IOMINodeExtensionHandler<OMISpawnPointNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(spawnNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_seat node extension
            if (settings.ImportSeats)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMISeatNode>(node, OMIJsonExtensions.ExtensionNames.Seat, out var seatNode))
                {
                    Logging.Log($"[OMIHandler] Found OMI_seat on node {nodeIndex} (GameObject: {gameObject.name})");
                    var handler = extensionManager.GetHandler<OMISeatNode>(OMIJsonExtensions.ExtensionNames.Seat) as IOMINodeExtensionHandler<OMISeatNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(seatNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                        Logging.Log($"[OMIHandler] Processed OMI_seat on {gameObject.name}");
                    }
                    else
                    {
                        Logging.LogWarning($"[OMIHandler] No handler registered for OMI_seat");
                    }
                }
            }

            // Process OMI_link node extension
            if (settings.ImportLinks)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMILinkNode>(node, OMIJsonExtensions.ExtensionNames.Link, out var linkNode))
                {
                    var handler = extensionManager.GetHandler<OMILinkNode>(OMIJsonExtensions.ExtensionNames.Link) as IOMINodeExtensionHandler<OMILinkNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(linkNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_personality node extension
            if (settings.ImportPersonality)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIPersonalityNode>(node, OMIJsonExtensions.ExtensionNames.Personality, out var personalityNode))
                {
                    var handler = extensionManager.GetHandler<OMIPersonalityNode>(OMIJsonExtensions.ExtensionNames.Personality) as IOMINodeExtensionHandler<OMIPersonalityNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(personalityNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_vehicle_body node extension
            if (settings.ImportVehicleBodies)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIVehicleBodyNode>(node, OMIJsonExtensions.ExtensionNames.VehicleBody, out var vehicleBodyNode))
                {
                    Logging.Log($"[OMIHandler] Found OMI_vehicle_body on node {nodeIndex} (GameObject: {gameObject.name}), pilotSeat: {vehicleBodyNode.pilotSeat}");
                    var handler = extensionManager.GetHandler<OMIVehicleBodyNode>(OMIJsonExtensions.ExtensionNames.VehicleBody) as IOMINodeExtensionHandler<OMIVehicleBodyNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(vehicleBodyNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                        Logging.Log($"[OMIHandler] Processed OMI_vehicle_body on {gameObject.name}");
                    }
                    else
                    {
                        Logging.LogWarning($"[OMIHandler] No handler registered for OMI_vehicle_body");
                    }
                }
            }

            // Process OMI_vehicle_thruster node extension
            if (settings.ImportVehicleThrusters)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIVehicleThrusterNode>(node, OMIJsonExtensions.ExtensionNames.VehicleThruster, out var vehicleThrusterNode))
                {
                    Logging.Log($"[OMIHandler] Found OMI_vehicle_thruster on node {nodeIndex} (GameObject: {gameObject.name})");
                    var handler = extensionManager.GetHandler<OMIVehicleThrusterNode>(OMIJsonExtensions.ExtensionNames.VehicleThruster) as IOMINodeExtensionHandler<OMIVehicleThrusterNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(vehicleThrusterNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                    else
                    {
                        Logging.LogWarning($"[OMIHandler] No handler registered for OMI_vehicle_thruster");
                    }
                }
            }

            // Process OMI_vehicle_wheel node extension
            if (settings.ImportVehicleWheels)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIVehicleWheelNode>(node, OMIJsonExtensions.ExtensionNames.VehicleWheel, out var vehicleWheelNode))
                {
                    var handler = extensionManager.GetHandler<OMIVehicleWheelNode>(OMIJsonExtensions.ExtensionNames.VehicleWheel) as IOMINodeExtensionHandler<OMIVehicleWheelNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(vehicleWheelNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
            }

            // Process OMI_vehicle_hover_thruster node extension
            if (settings.ImportVehicleHoverThrusters)
            {
                if (OMIJsonExtensions.TryGetNodeExtension<OMIVehicleHoverThrusterNode>(node, OMIJsonExtensions.ExtensionNames.VehicleHoverThruster, out var vehicleHoverThrusterNode))
                {
                    var handler = extensionManager.GetHandler<OMIVehicleHoverThrusterNode>(OMIJsonExtensions.ExtensionNames.VehicleHoverThruster) as IOMINodeExtensionHandler<OMIVehicleHoverThrusterNode>;
                    if (handler != null)
                    {
                        var task = handler.OnNodeImportAsync(vehicleHoverThrusterNode, nodeIndex, gameObject, context, default);
                        while (!task.IsCompleted) yield return null;
                    }
                }
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

        /// <summary>
        /// Recursively print the Unity hierarchy for debugging.
        /// </summary>
        private void PrintHierarchy(Transform t, int indent)
        {
            string prefix = new string(' ', indent * 2);
            Debug.Log($"{prefix}{t.name}");
            for (int i = 0; i < t.childCount; i++)
            {
                PrintHierarchy(t.GetChild(i), indent + 1);
            }
        }
    }
}
