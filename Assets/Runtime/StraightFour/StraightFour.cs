// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FiveSQD.StraightFour
{
    /// <summary>
    /// Class for the World Engine.
    /// </summary>
    public class StraightFour : MonoBehaviour
    {
        /// <summary>
        /// Material to use for object highlighting.
        /// </summary>
        [Tooltip("Material to use for object highlighting.")]
        public Material highlightMaterial;

        /// <summary>
        /// Material to use for object previewing.
        /// </summary>
        [Tooltip("Material to use for object previewing.")]
        public Material previewMaterial;

        /// <summary>
        /// Material to use for the environment sky.
        /// </summary>
        [Tooltip("Material to use for the environment sky.")]
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
        /// Prefab for an input entity.
        /// </summary>
        [Tooltip("Prefab for an input entity.")]
        public GameObject inputEntityPrefab;

        /// <summary>
        /// Prefab for a webview.
        /// </summary>
        [Tooltip("Prefab for a webview.")]
        public GameObject webViewPrefab;

        /// <summary>
        /// Prefab for a canvas webview.
        /// </summary>
        [Tooltip("Prefab for a canvas webview.")]
        public GameObject canvasWebViewPrefab;

        /// <summary>
        /// Prefab for a character controller.
        /// </summary>
        [Tooltip("Prefab for a character controller.")]
        public GameObject characterControllerPrefab;

        /// <summary>
        /// Character controller label prefab.
        /// </summary>
        [Tooltip("Character controller label prefab.")]
        public GameObject characterControllerLabelPrefab;

        /// <summary>
        /// Prefab for a voxel block.
        /// </summary>
        [Tooltip("Prefab for a voxel block.")]
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
        /// Map for automobile entity types to their NWH State Settings object.
        /// </summary>
        public Dictionary<Entity.EntityManager.AutomobileEntityType,
            NWH.VehiclePhysics2.StateSettings> automobileEntityTypeMap;

        /// <summary>
        /// Prefab for an airplane entity.
        /// </summary>
        public GameObject airplaneEntityPrefab;

        /// <summary>
        /// Crosshair.
        /// </summary>
        public GameObject crosshair;
        
        /// <summary>
        /// Logging configuration for the StraightFour instance.
        /// </summary>
        [Tooltip("Logging configuration for the StraightFour instance.")]
        public Utilities.LoggingConfig loggingConfig;

        /// <summary>
        /// The active world loaded by the world engine.
        /// </summary>
        public static World.World ActiveWorld
        {
            get
            {
                if (instance == null)
                {
                    return null;
                }

                return instance.currentWorld;
            }
        }

        /// <summary>
        /// The instance of the world engine.
        /// </summary>
        private static StraightFour instance;

        /// <summary>
        /// The current world in the world engine.
        /// </summary>
        private World.World currentWorld = null;

        /// <summary>
        /// The GameObject for the current world.
        /// </summary>
        private GameObject currentWorldGO;

        /// <summary>
        /// The URL query parameters for the current world.
        /// </summary>
        private Dictionary<string, string> queryParams;

        /// <summary>
        /// Load a world.
        /// </summary>
        /// <param name="worldName">Name for the world.</param>
        /// <param name="queryParams">Query parameters for the world.</param>
        /// <param name="loggingConfig">Logging configuration to use. If null, uses the instance's default configuration.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool LoadWorld(string worldName, string queryParams = null, Utilities.LoggingConfig loggingConfig = null)
        {
            // Set the logging configuration if provided, otherwise use the instance's default
            Utilities.LoggingConfig configToUse = loggingConfig ?? instance.loggingConfig;
            Utilities.LogSystem.SetLoggingConfig(configToUse);

            if (instance.currentWorld != null)
            {
                Utilities.LogSystem.LogError("[StraightFour->LoadWorld] Cannot load world. A world is loaded.");
                return false;
            }

            World.World.WorldInfo wInfo = new World.World.WorldInfo()
            {
                automobileEntityTypeMap = instance.automobileEntityTypeMap,
                airplaneEntityPrefab = instance.airplaneEntityPrefab,
                highlightMaterial = instance.highlightMaterial,
                previewMaterial = instance.previewMaterial,
                skyMaterial = instance.skyMaterial,
                liteProceduralSkyMaterial = instance.liteProceduralSkyMaterial,
                defaultCloudTexture = instance.defaultCloudTexture,
                defaultStarTexture = instance.defaultStarTexture,
                inputEntityPrefab = instance.inputEntityPrefab,
                webViewPrefab = instance.webViewPrefab,
                canvasWebViewPrefab = instance.canvasWebViewPrefab,
                characterControllerPrefab = instance.characterControllerPrefab,
                characterControllerLabelPrefab = instance.characterControllerLabelPrefab,
                voxelPrefab = instance.voxelPrefab,
                waterBodyPrefab = instance.waterBodyPrefab,
                waterBlockerPrefab = instance.waterBlockerPrefab,
                cameraOffset = instance.cameraOffset,
                vr = instance.vr,
                maxStorageEntries = 2048,
                maxEntryLength = 2048,
                maxKeyLength = 128,
                siteName = worldName
            };

            instance.currentWorldGO = new GameObject(worldName);
            instance.currentWorldGO.transform.parent = instance.transform;
            instance.currentWorld = instance.currentWorldGO.AddComponent<World.World>();
            instance.currentWorld.liteProceduralSkyObject = instance.liteProceduralSkyObject;
            instance.currentWorld.crosshair = instance.crosshair;
            instance.currentWorld.Initialize(wInfo);

            instance.queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(queryParams))
            {
                instance.LoadQueryParams(queryParams);
            }

            return true;
        }

        /// <summary>
        /// Unload the current world.
        /// </summary>
        public static void UnloadWorld()
        {
            Utilities.LogSystem.Log("[StraightFour->UnloadWorld] Unloading World...");

            if (instance.currentWorld != null)
            {
                instance.currentWorld.Unload();
            }
            instance.currentWorld = null;

            Utilities.LogSystem.Log("[StraightFour->UnloadWorld] World Unloaded. Destroying World Object...");

            if (instance.currentWorldGO != null)
            {
                Destroy(instance.currentWorldGO);
            }

            Utilities.LogSystem.Log("[StraightFour->UnloadWorld] World Object Destroyed.");

            System.GC.Collect();
        }

        /// <summary>
        /// Get a URL Query Parameter.
        /// </summary>
        /// <param name="key">Key of the Query Parameter.</param>
        /// <returns>The value of the Query Parameter, or null.</returns>
        public string GetParam(string key)
        {
            if (queryParams.ContainsKey(key))
            {
                return queryParams[key];
            }

            return null;
        }

        /// <summary>
        /// Load the URL Query Parameters.
        /// </summary>
        /// <param name="rawParams">Raw parameter string.</param>
        private void LoadQueryParams(string rawParams)
        {
            if (queryParams == null)
            {
                Utilities.LogSystem.LogError("[StraightFour->LoadQueryParams] StraightFour not initialized.");
                return;
            }

            string pattern = @"&(?!quot;)";

            Regex reg = new Regex(pattern);
            string[] kvps = reg.Split(rawParams.Replace("%26", "&"));
            foreach (string kvp in kvps)
            {
                string[] param = kvp.Split("=");
                if (param.Length != 2)
                {
                    Utilities.LogSystem.LogWarning("[StraightFour->LoadQueryParams] Invalid parameter " + param);
                    continue;
                }

                queryParams.Add(param[0], param[1]);
            }
        }

        /// <summary>
        /// Unity Awake method.
        /// </summary>
        void Awake()
        {
            instance = this;
        }
    }
}