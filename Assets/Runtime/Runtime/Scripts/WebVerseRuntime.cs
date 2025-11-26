// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.LocalStorage;
using FiveSQD.WebVerse.Handlers.File;
using FiveSQD.WebVerse.Handlers.GLTF;
using FiveSQD.WebVerse.Handlers.Image;
using FiveSQD.WebVerse.Handlers.Javascript;
#if USE_WEBINTERFACE
using FiveSQD.WebVerse.VOSSynchronization;
#endif
using System.IO;
using FiveSQD.WebVerse.Handlers.VEML;
using System;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.WebInterface.HTTP;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Data;
using System.Collections.Generic;
using FiveSQD.WebVerse.WebView;
using FiveSQD.WebVerse.Output;
using FiveSQD.WebVerse.Input.SteamVR;
using FiveSQD.WebVerse.Input.Desktop;
using Vuplex.WebView;
using FiveSQD.WebVerse.Handlers.JSONEntity;
using FiveSQD.WebVerse.Handlers.X3D;

namespace FiveSQD.WebVerse.Runtime
{
    /// <summary>
    /// The WebVerse Runtime
    /// </summary>
    public class WebVerseRuntime : MonoBehaviour
    {
        /// <summary>
        /// Serializable automobile entity type mapping.
        /// </summary>
        [Tooltip("Serializable automobile entity type mapping.")]
        [Serializable]
        public struct SerializableAutomobileEntityType
        {
            /// <summary>
            /// Type enum.
            /// </summary>
            public StraightFour.Entity.EntityManager.AutomobileEntityType type;

            /// <summary>
            /// State settings.
            /// </summary>
            public NWH.VehiclePhysics2.StateSettings stateSettings;
        }

        /// <summary>
        /// Enumeration for runtime states.
        /// </summary>
        public enum RuntimeState
        {
            /// <summary>
            /// Nothing is loaded.
            /// </summary>
            Unloaded = 0,

            /// <summary>
            /// World is being loaded.
            /// </summary>
            LoadingWorld = 1,

            /// <summary>
            /// World has been loaded.
            /// </summary>
            LoadedWorld = 2,

            /// <summary>
            /// WebPage has been loaded.
            /// </summary>
            WebPage = 3,

            /// <summary>
            /// A fatal error has been encountered.
            /// </summary>
            Error = 7
        }

        /// <summary>
        /// WebVerse Runtime settings.
        /// </summary>
        public struct RuntimeSettings
        {
            /// <summary>
            /// The storage mode. Valid values: cache, persistent.
            /// </summary>
            public string storageMode;

            /// <summary>
            /// Maximum storage entries.
            /// </summary>
            public int maxEntries;

            /// <summary>
            /// Maximum storage entry length.
            /// </summary>
            public int maxEntryLength;

            /// <summary>
            /// Maximum storage entry key length.
            /// </summary>
            public int maxKeyLength;

            /// <summary>
            /// Directory to use for files.
            /// </summary>
            public string filesDirectory;

            /// <summary>
            /// World Load Timeout.
            /// </summary>
            public float timeout;

            /// <summary>
            /// Logging configuration.
            /// </summary>
            public LoggingConfiguration loggingConfiguration;
        }

        /// <summary>
        /// WebVerse version.
        /// </summary>
        public static readonly string versionString = "v2.2.0";

        /// <summary>
        /// WebVerse codename.
        /// </summary>
        public static readonly string codenameString = "Terra Firma";

        /// <summary>
        /// Static reference to the WebVerse runtime.
        /// </summary>
        public static WebVerseRuntime Instance;

        /// <summary>
        /// Current state of the WebVerse Runtime.
        /// </summary>
        public RuntimeState state { get; private set; }

        /// <summary>
        /// The World Engine.
        /// </summary>
        [Tooltip("The World Engine.")]
        public StraightFour.StraightFour straightFour { get; private set; }

        /// <summary>
        /// The File Handler
        /// </summary>
        [Tooltip("The File Handler.")]
        public FileHandler fileHandler { get; private set; }

        /// <summary>
        /// The Image Handler.
        /// </summary>
        [Tooltip("The Image Handler.")]
        public ImageHandler imageHandler { get; private set; }

        /// <summary>
        /// The Javascript Handler.
        /// </summary>
        [Tooltip("The Javascript Handler.")]
        public JavascriptHandler javascriptHandler { get; private set; }

        /// <summary>
        /// The GLTF Handler.
        /// </summary>
        [Tooltip("The GLTF Handler.")]
        public GLTFHandler gltfHandler { get; private set; }

        /// <summary>
        /// The VEML Handler.
        /// </summary>
        [Tooltip("The VEML Handler.")]
        public VEMLHandler vemlHandler { get; private set; }

        /// <summary>
        /// The Time Handler.
        /// </summary>
        [Tooltip("The Time Handler.")]
        public TimeHandler timeHandler { get; private set; }

        /// <summary>
        /// The JSON Entity Handler.
        /// </summary>
        [Tooltip("The JSON Entity Handler.")]
        public JSONEntityHandler jsonEntityHandler { get; private set; }

        /// <summary>
        /// The X3D Handler.
        /// </summary>
        [Tooltip("The X3D Handler.")]
        public X3DHandler x3dHandler { get; private set; }

#if USE_WEBINTERFACE
        /// <summary>
        /// The VOS Synchronization Manager.
        /// </summary>
        [Tooltip("The VOS Synchronization Manager.")]
        public VOSSynchronizationManager vosSynchronizationManager { get; private set; }
#endif

        /// <summary>
        /// The Local Storage Manager.
        /// </summary>
        [Tooltip("The Local Storage Manager.")]
        public LocalStorageManager localStorageManager { get; private set; }

        /// <summary>
        /// The Input Manager.
        /// </summary>
        [Tooltip("The Input Manager.")]
        public InputManager inputManager { get; private set; }

        /// <summary>
        /// The Output Manager.
        /// </summary>
        public OutputManager outputManager { get; private set; }

        /// <summary>
        /// The HTTP Request Manager. This is a part of a stopgap solution while BestHTTP is broken.
        /// </summary>
        [Tooltip("The HTTP Request Manager. This is a part of a stopgap solution while BestHTTP is broken.")]
        public HTTPRequestManager httpRequestManager { get; private set; }

        /// <summary>
        /// The WebVerse WebView.
        /// </summary>
        [Tooltip("The WebVerse WebView.")]
        public WebVerseWebView webverseWebView { get; private set; }

        /// <summary>
        /// The Console.
        /// </summary>
        [Tooltip("The Console.")]
        public List<Interface.Console.Console> consoles;

        /// <summary>
        /// Crosshair.
        /// </summary>
        [Tooltip("Crosshair.")]
        public GameObject crosshair;

        /// <summary>
        /// Callback for a log message to be logged to a console.
        /// </summary>
        private List<Action<string, Logging.Type>> consoleCallbacks;

        /// <summary>
        /// Map for automobile entity types to their NWH State Settings object.
        /// </summary>
        public List<SerializableAutomobileEntityType> automobileEntityTypeMap;

        /// <summary>
        /// Prefab for an airplane entity.
        /// </summary>
        public GameObject airplaneEntityPrefab;

        /// <summary>
        /// Material to use for highlighting.
        /// </summary>
        [Tooltip("Material to use for highlighting.")]
        public Material highlightMaterial;

        /// <summary>
        /// Material to use for the sky.
        /// </summary>
        [Tooltip("Material to use for the sky.")]
        public Material skyMaterial;

        /// <summary>
        /// Material to use for the lite procedural sky.
        /// </summary>
        [Tooltip("Material to use for the lite procedural sky.")]
        public Material liteProceduralSkyMaterial;

        /// <summary>
        /// GameObject for the lite procedural sky.
        /// </summary>
        [Tooltip("GameObject for the lite procedural sky.")]
        public GameObject liteProceduralSkyObject;

        /// <summary>
        /// Environment default sky texture.
        /// </summary>
        public Texture2D defaultCloudTexture;

        /// <summary>
        /// Environment default sky texture.
        /// </summary>
        public Texture2D defaultStarTexture;

        /// <summary>
        /// Prefab for an Input Entity.
        /// </summary>
        [Tooltip("Prefab for an Input Entity.")]
        public GameObject inputEntityPrefab;

        /// <summary>
        /// Prefab for a Character Controller.
        /// </summary>
        [Tooltip("Prefab for a Character Controller.")]
        public GameObject characterControllerPrefab;

        /// <summary>
        /// Character controller label prefab.
        /// </summary>
        [Tooltip("Character controller label prefab.")]
        public GameObject characterControllerLabelPrefab;

        /// <summary>
        /// Prefab for a Voxel.
        /// </summary>
        [Tooltip("Prefab for a Voxel.")]
        public GameObject voxelPrefab;

        /// <summary>
        /// Prefab for a water body.
        /// </summary>
        [Tooltip("Prefab for a water body.")]
        public GameObject waterBodyPrefab;

        /// <summary>
        /// Prefab for a water blocker.
        /// </summary>
        [Tooltip("Prefab for a water blocker.")]
        public GameObject waterBlockerPrefab;

        /// <summary>
        /// Prefab for a WebView.
        /// </summary>
        [Tooltip("Prefab for a WebView.")]
        public GameObject webViewPrefab;

        /// <summary>
        /// Prefab for a Canvas WebView.
        /// </summary>
        [Tooltip("Prefab for a Canvas WebView.")]
        public GameObject canvasWebViewPrefab;

        /// <summary>
        /// Prefab for the WebVerse WebView.
        /// </summary>
        [Tooltip("Prefab for the WebVerse WebView.")]
        public GameObject webVerseWebViewPrefab;

        /// <summary>
        /// Camera offset.
        /// </summary>
        [Tooltip("Camera offset.")]
        public GameObject cameraOffset;

        /// <summary>
        /// Whether or not world is in VR mode.
        /// </summary>
        [Tooltip("Whether or not world is in VR mode.")]
        public bool vr;

        /// <summary>
        /// Platform Input.
        /// </summary>
        [Tooltip("Platform Input.")]
        public BasePlatformInput platformInput;

        /// <summary>
        /// The VR Rig.
        /// </summary>
        [Tooltip("The VR Rig.")]
        public VRRig vrRig;

        /// <summary>
        /// The Desktop Rig.
        /// </summary>
        [Tooltip("The Desktop Rig.")]
        public DesktopRig desktopRig;

        /// <summary>
        /// The base path of the current world.
        /// </summary>
        [Tooltip("The base path of the current world.")]
        public string currentBasePath { get; private set; }

        /// <summary>
        /// The URL of the current world/site.
        /// </summary>
        [Tooltip("The URL of the current world/site.")]
        public string currentURL { get; private set; }

        /// <summary>
        /// The prefab to use for cube mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for cube mesh entities.")]
        public GameObject cubeMeshPrefab;

        /// <summary>
        /// The prefab to use for capsule mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for capsule mesh entities.")]
        public GameObject capsuleMeshPrefab;

        /// <summary>
        /// The prefab to use for cone mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for cone mesh entities.")]
        public GameObject coneMeshPrefab;

        /// <summary>
        /// The prefab to use for cylinder mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for cylinder mesh entities.")]
        public GameObject cylinderMeshPrefab;

        /// <summary>
        /// The prefab to use for plane mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for plane mesh entities.")]
        public GameObject planeMeshPrefab;

        /// <summary>
        /// The prefab to use for rectangular pyramid mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for rectangular pyramid mesh entities.")]
        public GameObject rectangularPyramidMeshPrefab;

        /// <summary>
        /// The prefab to use for sphere mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for sphere mesh entities.")]
        public GameObject sphereMeshPrefab;

        /// <summary>
        /// The prefab to use for tetrahedron mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for tetrahedron mesh entities.")]
        public GameObject tetrahedronMeshPrefab;

        /// <summary>
        /// The prefab to use for torus mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for torus mesh entities.")]
        public GameObject torusMeshPrefab;

        /// <summary>
        /// The prefab to use for prism mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for prism mesh entities.")]
        public GameObject prismMeshPrefab;

        /// <summary>
        /// The prefab to use for arch mesh entities.
        /// </summary>
        [Tooltip("The prefab to use for arch mesh entities.")]
        public GameObject archMeshPrefab;

        /// <summary>
        /// The reflection probe.
        /// </summary>
        [Tooltip("The reflection probe.")]
        public ReflectionProbe reflectionProbe;

        /// <summary>
        /// Initialize the WebVerse Runtime.
        /// </summary>
        /// <param name="settings">The runtime settings to use.</param>
        public void Initialize(RuntimeSettings settings)
        {
            // Apply logging configuration first
            Logging.SetConfiguration(settings.loggingConfiguration);

            LocalStorageManager.LocalStorageMode mode = LocalStorageManager.LocalStorageMode.Uninitialized;
            if (settings.storageMode.ToLower() == "cache")
            {
                mode = LocalStorageManager.LocalStorageMode.Cache;
            }
            else if (settings.storageMode.ToLower() == "persistent")
            {
                mode = LocalStorageManager.LocalStorageMode.Persistent;
            }
            else
            {
                Logging.LogError("[WebVerseRuntime->Initialize] Invalid storage mode.");
                return;
            }

            if (settings.maxEntries <= 0 || settings.maxEntries >= 262144)
            {
                Logging.LogError("[WebVerseRuntime->Initialize] Invalid max entries value.");
                return;
            }

            if (settings.maxEntryLength <= 8 || settings.maxEntryLength >= 131072)
            {
                Logging.LogError("[WebVerseRuntime->Initialize] Invalid max entry length value.");
                return;
            }

            if (settings.maxKeyLength <= 4 || settings.maxKeyLength >= 8192)
            {
                Logging.LogError("[WebVerseRuntime->Initialize] Invalid max key length value.");
            }

            Initialize(mode, settings.maxEntries, settings.maxEntryLength,
                settings.maxKeyLength, settings.filesDirectory, settings.timeout);
        }

        /// <summary>
        /// Initialize the WebVerse Runtime.
        /// </summary>
        /// <param name="storageMode">Mode to set local storage to. Must be cache or persistent.</param>
        /// <param name="maxEntries">Maximum number of storage entries.</param>
        /// <param name="maxEntryLength">Maximum length of a storage entry.</param>
        /// <param name="maxKeyLength">Maximum length of a storage entry key.</param>
        /// <param name="filesDirectory">Directory to use for files.</param>
        /// <param name="timeout">World Load Timeout.</param>
        /// <param name="loggingConfiguration">Logging configuration to use. If not provided, uses default configuration.</param>
        public void Initialize(LocalStorageManager.LocalStorageMode storageMode,
            int maxEntries, int maxEntryLength, int maxKeyLength, string filesDirectory,
            float timeout = 120, LoggingConfiguration? loggingConfiguration = null)
        {
            if (Instance != null)
            {
                Logging.LogError("[WebVerseRuntime->Initialize] Already initialized.");
                return;
            }

            Instance = this;

            // Apply logging configuration
            if (loggingConfiguration.HasValue)
            {
                Logging.SetConfiguration(loggingConfiguration.Value);
            }
            else
            {
                Logging.SetConfiguration(LoggingConfiguration.CreateDefault());
            }

            InitializeComponents(storageMode, maxEntries, maxEntryLength,
                maxKeyLength, filesDirectory, timeout);

            state = RuntimeState.Unloaded;
        }

        /// <summary>
        /// Terminate the WebVerse Runtime.
        /// </summary>
        public void Terminate()
        {
            if (Instance == null)
            {
                Logging.LogError("[WebVerseRuntime->Terminate] Not initialized.");
                return;
            }

            TerminateComponents();
        }

        /// <summary>
        /// Load a URL.
        /// </summary>
        /// <param name="url">URL containing the world or HTML page to load.</param>
        /// <param name="onLoaded">Action to perform on load. Provides string containing loaded world name.</param>
        public void LoadURL(string url, Action<string> onLoaded = null)
        {
            currentURL = url;

            if (url.StartsWith("file://") || url.StartsWith("/") || url.StartsWith(".") || url[1] == ':')
            {
                UnloadWebPage();
                LoadWorld(url, onLoaded);
            }
            else
            {
                Action<int, Dictionary<string, string>, byte[]> onRespAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
                {
                    if (code > 399)
                    {
                        Logging.LogWarning("[WebVerseRuntime->LoadURL] URL " + url + " returned code " + code);
                        return;
                    }

                    // Check if possibly web page.
                    if (headers.ContainsKey("Content-Type"))
                    {
                        string contentType = headers["Content-Type"];
                        if (contentType.Contains("text/html") || contentType.Contains("application/binary") || contentType.Contains("text/plain"))
                        {
                            Logging.Log("[WebVerseRuntime->LoadURL] Identified webpage. Loading...");
                            Logging.Log("[WebVerseRuntime->LoadURL] Identified webpage. Unloading any world...");
                            UnloadWorld();
                            Logging.Log("[WebVerseRuntime->LoadURL] Identified webpage. Loading web page...");
                            LoadWebPage(url, onLoaded);
                            Logging.Log("[WebVerseRuntime->LoadURL] Webpage loaded.");
                            return;
                        }
                    }

                    // Otherwise, treat as world.
                    Logging.Log("[WebVerseRuntime->LoadURL] Identified world. Loading.");
                    UnloadWebPage();
                    LoadWorld(url, onLoaded);
                });
#if USE_WEBINTERFACE
                HTTPRequest headerReq = new HTTPRequest(url, HTTPRequest.HTTPMethod.Head, onRespAction);
                headerReq.Send();
#endif
            }
        }

        /// <summary>
        /// Load a world.
        /// </summary>
        /// <param name="url">URL containing the world to load.</param>
        /// <param name="onLoaded">Action to perform on load. Provides string containing loaded world name.</param>
        public void LoadWorld(string url, Action<string> onLoaded)
        {
            if (straightFour == null)
            {
                Logging.LogError("[WebVerseRuntime->LoadWorld] World Engine not initialized.");
                return;
            }

            if (StraightFour.StraightFour.ActiveWorld != null)
            {
                UnloadWorld();
            }

            string baseURL = url;
            string queryParams = "";
            if (url.Contains("?"))
            {
                baseURL = url.Substring(0, url.IndexOf('?'));
                queryParams = url.Substring(url.IndexOf('?') + 1);
            }

            Action<bool> onLoadComplete = ((result) =>
            {
                if (result == true)
                {
                    reflectionProbe.enabled = false;
                    reflectionProbe.enabled = true;
                    reflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
                    state = RuntimeState.LoadedWorld;
                }
                else
                {
                    state = RuntimeState.Error;
                }

                if (onLoaded != null)
                {
                    onLoaded.Invoke(StraightFour.StraightFour.ActiveWorld.siteName);
                }
            });

            Action<string> onFound = (title) =>
            {
                state = RuntimeState.LoadingWorld;
                currentBasePath = VEMLUtilities.FormatURI(Path.GetDirectoryName(baseURL));
                StraightFour.Utilities.LoggingConfig loggingConfig = new StraightFour.Utilities.LoggingConfig()
                {
                    enableDebug = Logging.GetConfiguration().enableDebug,
                    enableError = Logging.GetConfiguration().enableError,
                    enableWarning = Logging.GetConfiguration().enableWarning,
                    enableDefault = Logging.GetConfiguration().enableDefault
                };
                StraightFour.StraightFour.LoadWorld(title, queryParams);
                vemlHandler.LoadVEMLDocumentIntoWorld(baseURL, onLoadComplete);
            };
            
            vemlHandler.GetWorldName(baseURL, onFound);
        }

        /// <summary>
        /// Unload a world.
        /// </summary>
        public void UnloadWorld()
        {
            if (StraightFour.StraightFour.ActiveWorld == null)
            {
                Logging.LogWarning("[WebVerseRuntime->UnloadWorld] No world to unload.");
                return;
            }

            Logging.Log("[WebVerseRuntime->UnloadWorld] Unloading world: "
                + StraightFour.StraightFour.ActiveWorld.siteName + ".");
            
            Logging.Log("[WebVerseRuntime->UnloadWorld] Resetting Javascript Handler...");

            if (javascriptHandler != null)
            {
                javascriptHandler.Reset();
            }

            Logging.Log("[WebVerseRuntime->UnloadWorld] Javascript Handler reset. Resetting Time Handler...");

            if (timeHandler != null)
            {
                timeHandler.Reset();
            }

            Logging.Log("[WebVerseRuntime->UnloadWorld] Time Handler reset. Resetting Input Manager...");

            if (inputManager != null)
            {
                inputManager.Reset();
            }

            Logging.Log("[WebVerseRuntime->UnloadWorld] Input Manager reset. Resetting VOS Synchronization Manager...");
#if USE_WEBINTERFACE
            if (vosSynchronizationManager != null)
            {
                vosSynchronizationManager.Reset();
            }
#endif
            Logging.Log("[WebVerseRuntime->UnloadWorld] VOS Synchronization Manager reset. Unloading World...");

            StraightFour.StraightFour.UnloadWorld();
            state = RuntimeState.Unloaded;

            Logging.Log("[WebVerseRuntime->UnloadWorld] World Unloaded.");
        }

        /// <summary>
        /// Load a webpage.
        /// </summary>
        /// <param name="url">URL containing the webpage to load.</param>
        /// <param name="onLoaded">Action to perform on load. Provides string indicating web page.</param>
        public void LoadWebPage(string url, Action<string> onLoaded)
        {
            state = RuntimeState.WebPage;
            webverseWebView.Show();
            webverseWebView.LoadURL(url);
            if (onLoaded != null)
            {
                onLoaded.Invoke("Web Page");
            }
        }

        /// <summary>
        /// Unload a webpage.
        /// </summary>
        public void UnloadWebPage()
        {
            state = RuntimeState.Unloaded;
            webverseWebView.Unload();
            webverseWebView.Hide();
        }

        /// <summary>
        /// Initialize the WebVerse Runtime components.
        /// </summary>
        /// <param name="storageMode">Mode to set local storage to. Must be cache or persistent.</param>
        /// <param name="maxEntries">Maximum number of storage entries.</param>
        /// <param name="maxEntryLength">Maximum length of a storage entry.</param>
        /// <param name="maxKeyLength">Maximum length of a storage entry key.</param>
        /// <param name="filesDirectory">Directory to use as the files directory.</param>
        /// <param name="timeout">World Load Timeout.</param>
        private void InitializeComponents(LocalStorageManager.LocalStorageMode storageMode,
            int maxEntries, int maxEntryLength, int maxKeyLength, string filesDirectory,
            float timeout = 120)
        {
            #if UNITY_STANDALONE || UNITY_EDITOR
                // On Windows and macOS, change the User-Agent to mobile:
                Web.SetUserAgent(true);
            #elif UNITY_IOS
                // On iOS, change the User-Agent to desktop:
                Web.SetUserAgent(false);
            #elif UNITY_ANDROID
                // On Android, change the User-Agent to "random":
                Web.SetUserAgent("random");
            #endif

            // Set up World Engine.
            GameObject StraightFourGO = new GameObject("StraightFour");
            StraightFourGO.transform.SetParent(transform);
            straightFour = StraightFourGO.AddComponent<StraightFour.StraightFour>();
            straightFour.automobileEntityTypeMap
                = new Dictionary<StraightFour.Entity.EntityManager.AutomobileEntityType,
                        NWH.VehiclePhysics2.StateSettings>();
            foreach (SerializableAutomobileEntityType automobileEntityType in automobileEntityTypeMap)
            {
                StraightFour.Entity.EntityManager.AutomobileEntityType type;
                switch(automobileEntityType.type)
                {
                    case StraightFour.Entity.EntityManager.AutomobileEntityType.Default:
                    default:
                        type = StraightFour.Entity.EntityManager.AutomobileEntityType.Default;
                        break;
                }
                straightFour.automobileEntityTypeMap.Add(type, automobileEntityType.stateSettings);
            }
            straightFour.airplaneEntityPrefab = airplaneEntityPrefab;
            straightFour.highlightMaterial = highlightMaterial;
            straightFour.skyMaterial = skyMaterial;
            straightFour.liteProceduralSkyMaterial = liteProceduralSkyMaterial;
            straightFour.liteProceduralSkyObject = liteProceduralSkyObject;
            straightFour.defaultCloudTexture = defaultCloudTexture;
            straightFour.defaultStarTexture = defaultStarTexture;
            straightFour.inputEntityPrefab = inputEntityPrefab;
            straightFour.characterControllerPrefab = characterControllerPrefab;
            straightFour.characterControllerLabelPrefab = characterControllerLabelPrefab;
            straightFour.voxelPrefab = voxelPrefab;
            straightFour.waterBodyPrefab = waterBodyPrefab;
            straightFour.waterBlockerPrefab = waterBlockerPrefab;
            straightFour.webViewPrefab = webViewPrefab;
            straightFour.canvasWebViewPrefab = canvasWebViewPrefab;
            straightFour.cameraOffset = cameraOffset;
            straightFour.vr = vr;
            straightFour.crosshair = crosshair;

            // Set up Handlers.
            GameObject handlersGO = new GameObject("Handlers");
            handlersGO.transform.SetParent(transform);
            GameObject fileHandlerGO = new GameObject("File");
            fileHandlerGO.transform.SetParent(handlersGO.transform);
            fileHandler = fileHandlerGO.AddComponent<FileHandler>();
            fileHandler.Initialize(filesDirectory);
            GameObject imageHandlerGO = new GameObject("Image");
            imageHandlerGO.transform.SetParent(handlersGO.transform);
            imageHandler = imageHandlerGO.AddComponent<ImageHandler>();
            imageHandler.runtime = this;
            imageHandler.Initialize();
            GameObject javascriptHandlerGO = new GameObject("Javascript");
            javascriptHandlerGO.transform.SetParent(handlersGO.transform);
            javascriptHandler = javascriptHandlerGO.AddComponent<JavascriptHandler>();
            javascriptHandler.Initialize();
            GameObject entityAPIHelperGO = new GameObject("EntityAPIHelper");
            entityAPIHelperGO.transform.SetParent(javascriptHandlerGO.transform);
            EntityAPIHelper entityAPIHelper = entityAPIHelperGO.AddComponent<EntityAPIHelper>();
            entityAPIHelper.Initialize();
            GameObject dataAPIHelperGO = new GameObject("DataAPIHelper");
            dataAPIHelperGO.transform.SetParent(javascriptHandlerGO.transform);
            DataAPIHelper dataAPIHelper = dataAPIHelperGO.AddComponent<DataAPIHelper>();
            dataAPIHelper.Initialize();

            GameObject gltfHandlerGO = new GameObject("GLTF");
            gltfHandlerGO.transform.SetParent(handlersGO.transform);
            gltfHandler = gltfHandlerGO.AddComponent<GLTFHandler>();
            gltfHandler.Initialize();
            gltfHandler.runtime = this;
            GameObject gltfPrefabContainer = new GameObject("PrefabContainer");
            gltfPrefabContainer.transform.SetParent(gltfHandler.transform);
            gltfHandler.meshPrefabContainer = gltfPrefabContainer;
            GameObject vemlHandlerGO = new GameObject("VEML");
            vemlHandlerGO.transform.SetParent(handlersGO.transform);
            vemlHandler = vemlHandlerGO.AddComponent<VEMLHandler>();
            vemlHandler.timeout = timeout;
            vemlHandler.runtime = this;
            vemlHandler.Initialize();
            GameObject timeHandlerGO = new GameObject("Time");
            timeHandlerGO.transform.SetParent(handlersGO.transform);
            timeHandler = timeHandlerGO.AddComponent<TimeHandler>();
            timeHandler.Initialize();
            GameObject jsonEntityHandlerGO = new GameObject("JSONEntity");
            jsonEntityHandlerGO.transform.SetParent(handlersGO.transform);
            jsonEntityHandler = jsonEntityHandlerGO.AddComponent<JSONEntityHandler>();
            jsonEntityHandler.Initialize();

            // Set up X3D Handler.
            GameObject x3dHandlerGO = new GameObject("X3D");
            x3dHandlerGO.transform.SetParent(handlersGO.transform);
            x3dHandler = x3dHandlerGO.AddComponent<X3DHandler>();
            x3dHandler.runtime = this;
            x3dHandler.Initialize();
            // Create and set the StraightFour adapter
            StraightFourX3DAdapter x3dAdapter = x3dHandlerGO.AddComponent<StraightFourX3DAdapter>();
            x3dAdapter.Initialize();
            x3dHandler.SetWorldAdapter(x3dAdapter);

            // Set up VOS Synchronization Manager.
            GameObject vosSynchronizationManagerGO = new GameObject("VOSSynchronizationManager");
            vosSynchronizationManagerGO.transform.SetParent(transform);
#if USE_WEBINTERFACE
            vosSynchronizationManager = vosSynchronizationManagerGO.AddComponent<VOSSynchronizationManager>();
            vosSynchronizationManager.Initialize();
#endif

            // Set up Local Storage Manager.
            GameObject localStorageManagerGO = new GameObject("LocalStorageManager");
            localStorageManagerGO.transform.SetParent(transform);
            localStorageManager = localStorageManagerGO.AddComponent<LocalStorageManager>();
            localStorageManager.Initialize(storageMode, maxEntries, maxEntryLength, maxKeyLength);

            // Set up Input Manager.
            GameObject inputManagerGO = new GameObject("InputManager");
            inputManagerGO.transform.SetParent(transform);
            inputManager = inputManagerGO.AddComponent<InputManager>();
            inputManager.platformInput = platformInput;
            inputManager.vRRig = vrRig;
            inputManager.desktopRig = desktopRig;
            inputManager.Initialize();

            // Set up Output Manager.
            GameObject outputManagerGO = new GameObject("OutputManager");
            outputManagerGO.transform.SetParent(transform);
            outputManager = outputManagerGO.AddComponent<OutputManager>();
            outputManager.Initialize();

            // Set up HTTP Request Manager.
            GameObject httpRequestManagerGO = new GameObject("HTTPRequestManager");
            httpRequestManagerGO.transform.SetParent(transform);
            httpRequestManager = httpRequestManagerGO.AddComponent<HTTPRequestManager>();
            httpRequestManager.Initialize();

            // Set up WebVerse WebView.
            GameObject webVerseWebViewGO = new GameObject("WebVerseWebView");
            webVerseWebViewGO.transform.SetParent(transform);
            webverseWebView = webVerseWebViewGO.AddComponent<WebVerseWebView>();
            webverseWebView.Initialize();
            
            // Set up Console.
            if (consoles != null)
            {
                consoleCallbacks = new List<Action<string, Logging.Type>>();
                foreach (Interface.Console.Console console in consoles)
                {
                    console.Initialize();
                    Action<string, Logging.Type> consoleCallback = new Action<string, Logging.Type>((msg, type) =>
                    {
                        console.LogConsoleMessage(msg, type);
                    });
                    Logging.RegisterCallback(consoleCallback);
                    consoleCallbacks.Add(consoleCallback);
                }
            }
        }

        /// <summary>
        /// Terminate the WebVerse Runtime components.
        /// </summary>
        private void TerminateComponents()
        {
            // Terminate WebVerse WebView.
            webverseWebView.Terminate();
            Destroy(webverseWebView.gameObject);

            // Terminate HTTP Request Manager.
            httpRequestManager.Terminate();
            Destroy(httpRequestManager.gameObject);

            // Terminate Output Manager.
            outputManager.Terminate();
            Destroy(outputManager.gameObject);

            // Terminate Input Manager.
            inputManager.Terminate();
            Destroy(inputManager.gameObject);

            // Terminate Local Storage Manager.
            localStorageManager.Terminate();
            Destroy(localStorageManager.gameObject);

#if USE_WEBINTERFACE
            // Terminate VOS Synchronization Manager.
            vosSynchronizationManager.Terminate();
            Destroy(vosSynchronizationManager.gameObject);
#endif

            // Terminate Handlers.
            jsonEntityHandler.Terminate();
            timeHandler.Terminate();
            vemlHandler.Terminate();
            gltfHandler.Terminate();
            javascriptHandler.Terminate();
            imageHandler.Terminate();
            fileHandler.Terminate();
            x3dHandler.Terminate();
            Destroy(fileHandler.transform.parent.gameObject);

            // Terminate World Engine.
            if (StraightFour.StraightFour.ActiveWorld != null)
            {
                StraightFour.StraightFour.UnloadWorld();
            }
            Destroy(straightFour.gameObject);

            if (consoles != null)
            {
                foreach (Interface.Console.Console console in consoles)
                {
                    if (consoleCallbacks != null)
                    {
                        foreach (Action<string, Logging.Type> consoleCallback in consoleCallbacks.ToArray())
                        {
                            Logging.RemoveCallback(consoleCallback);
                            consoleCallbacks.Remove(consoleCallback);
                        }
                    }
                    console.Terminate();
                }
            }
        }
    }
}