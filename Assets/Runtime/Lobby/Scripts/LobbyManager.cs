// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Lobby
{
    /// <summary>
    /// Manages the Blastoff lobby environment.
    /// Handles entry animations, world selection, and launch transitions.
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        #region Enums

        /// <summary>
        /// Current state of the lobby.
        /// </summary>
        public enum LobbyState
        {
            Inactive,
            Entering,
            Idle,
            Selecting,
            Launching
        }

        #endregion

        #region Inspector Fields

        [Header("References")]
        [Tooltip("The skybox material using BlastoffSkybox shader.")]
        public Material skyboxMaterial;

        [Tooltip("The floor grid renderer.")]
        public Renderer floorGridRenderer;

        [Tooltip("Parent transform for world selector UI elements.")]
        public Transform worldSelectorParent;

        [Tooltip("Prefab for world selection items.")]
        public GameObject worldItemPrefab;

        [Header("Entry Animation")]
        [Tooltip("Duration of UI assembly animation.")]
        public float assemblyDuration = 1.0f;

        [Tooltip("Delay before UI starts assembling.")]
        public float assemblyDelay = 0.5f;

        [Header("Launch Animation")]
        [Tooltip("Duration of launch transition.")]
        public float launchDuration = 1.5f;

        [Tooltip("Star drift speed multiplier during launch.")]
        public float launchDriftMultiplier = 20f;

        [Header("World Sources")]
        [Tooltip("Native history for loading visited worlds.")]
        public NativeHistory nativeHistory;

        [Tooltip("Default world to load if none selected.")]
        public string defaultWorldUrl = "";

        #endregion

        #region Private Fields

        private LobbyState currentState = LobbyState.Inactive;
        private List<WorldInfo> availableWorlds = new List<WorldInfo>();
        private WorldInfo selectedWorld;
        private Material[] wireframeMaterials;
        private float originalDriftSpeed;

        #endregion

        #region Properties

        /// <summary>
        /// Current lobby state.
        /// </summary>
        public LobbyState CurrentState => currentState;

        /// <summary>
        /// Whether the lobby is active and visible.
        /// </summary>
        public bool IsActive => currentState != LobbyState.Inactive;

        #endregion

        #region Events

        /// <summary>
        /// Fired when a world is selected.
        /// </summary>
        public event Action<WorldInfo> OnWorldSelected;

        /// <summary>
        /// Fired when launch begins.
        /// </summary>
        public event Action<WorldInfo> OnLaunchStarted;

        /// <summary>
        /// Fired when lobby entry completes.
        /// </summary>
        public event Action OnEntryComplete;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache original drift speed
            if (skyboxMaterial != null && skyboxMaterial.HasProperty("_DriftSpeed"))
            {
                originalDriftSpeed = skyboxMaterial.GetFloat("_DriftSpeed");
            }
        }

        private void OnDestroy()
        {
            // Reset skybox drift speed
            if (skyboxMaterial != null && skyboxMaterial.HasProperty("_DriftSpeed"))
            {
                skyboxMaterial.SetFloat("_DriftSpeed", originalDriftSpeed);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enter the lobby with animation.
        /// </summary>
        public void EnterLobby()
        {
            if (currentState != LobbyState.Inactive)
            {
                Logging.LogWarning("[LobbyManager] Already in lobby.");
                return;
            }

            StartCoroutine(EnterLobbySequence());
        }

        /// <summary>
        /// Exit the lobby immediately (no animation).
        /// </summary>
        public void ExitLobby()
        {
            currentState = LobbyState.Inactive;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Select a world and begin launch sequence.
        /// </summary>
        public void SelectWorld(WorldInfo world)
        {
            if (currentState != LobbyState.Idle)
            {
                Logging.LogWarning("[LobbyManager] Cannot select world - lobby not in idle state.");
                return;
            }

            selectedWorld = world;
            currentState = LobbyState.Selecting;
            OnWorldSelected?.Invoke(world);

            StartCoroutine(LaunchSequence());
        }

        /// <summary>
        /// Select a world by URL.
        /// </summary>
        public void SelectWorld(string worldUrl)
        {
            SelectWorld(new WorldInfo { url = worldUrl, name = "World" });
        }

        /// <summary>
        /// Refresh the available worlds list.
        /// </summary>
        public void RefreshWorldList()
        {
            StartCoroutine(LoadWorldList());
        }

        #endregion

        #region Private Methods - Entry Sequence

        private IEnumerator EnterLobbySequence()
        {
            currentState = LobbyState.Entering;
            gameObject.SetActive(true);

            // Set wireframe materials to assembly start
            SetAssemblyProgress(0f);

            // Wait before assembly
            yield return new WaitForSeconds(assemblyDelay);

            // Animate UI assembly
            yield return StartCoroutine(AnimateAssembly());

            // Load world list
            yield return StartCoroutine(LoadWorldList());

            currentState = LobbyState.Idle;
            OnEntryComplete?.Invoke();

            Logging.Log("[LobbyManager] Lobby entry complete.");
        }

        private IEnumerator AnimateAssembly()
        {
            float elapsed = 0f;
            while (elapsed < assemblyDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / assemblyDuration;

                // Ease out cubic
                progress = 1f - Mathf.Pow(1f - progress, 3f);

                SetAssemblyProgress(progress);
                yield return null;
            }

            SetAssemblyProgress(1f);
        }

        private void SetAssemblyProgress(float progress)
        {
            if (wireframeMaterials == null)
            {
                wireframeMaterials = GetWireframeMaterials();
            }

            foreach (var mat in wireframeMaterials)
            {
                if (mat != null && mat.HasProperty("_AssemblyProgress"))
                {
                    mat.SetFloat("_AssemblyProgress", progress);
                }
            }
        }

        private Material[] GetWireframeMaterials()
        {
            var materials = new List<Material>();

            if (worldSelectorParent != null)
            {
                var renderers = worldSelectorParent.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    foreach (var mat in renderer.materials)
                    {
                        if (mat.shader.name.Contains("WireframeUI"))
                        {
                            materials.Add(mat);
                        }
                    }
                }
            }

            return materials.ToArray();
        }

        #endregion

        #region Private Methods - Launch Sequence

        private IEnumerator LaunchSequence()
        {
            currentState = LobbyState.Launching;
            OnLaunchStarted?.Invoke(selectedWorld);

            Logging.Log($"[LobbyManager] Launching to: {selectedWorld.url}");

            // Accelerate star drift for launch effect
            if (skyboxMaterial != null && skyboxMaterial.HasProperty("_DriftSpeed"))
            {
                yield return StartCoroutine(AccelerateStarDrift());
            }

            // Load the world
            if (WebVerseRuntime.Instance != null)
            {
                WebVerseRuntime.Instance.LoadWorld(selectedWorld.url, null);
            }
            else
            {
                Logging.LogError("[LobbyManager] WebVerseRuntime not available.");
            }

            // Exit lobby
            ExitLobby();
        }

        private IEnumerator AccelerateStarDrift()
        {
            float elapsed = 0f;
            float halfDuration = launchDuration * 0.5f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                float speed = Mathf.Lerp(originalDriftSpeed, originalDriftSpeed * launchDriftMultiplier, t * t);
                skyboxMaterial.SetFloat("_DriftSpeed", speed);
                yield return null;
            }
        }

        #endregion

        #region Private Methods - World List

        private IEnumerator LoadWorldList()
        {
            availableWorlds.Clear();

            // Load worlds from history
            if (nativeHistory != null)
            {
                var historyItems = nativeHistory.GetAllItemsFromHistory();
                if (historyItems != null)
                {
                    // Track unique URLs to avoid duplicates (keep most recent)
                    var seenUrls = new HashSet<string>();

                    // Sort by timestamp descending (most recent first)
                    System.Array.Sort(historyItems, (a, b) => b.Item1.CompareTo(a.Item1));

                    foreach (var item in historyItems)
                    {
                        string url = item.Item3;
                        if (!seenUrls.Contains(url))
                        {
                            seenUrls.Add(url);
                            var worldInfo = new WorldInfo
                            {
                                name = item.Item2, // Site name from history
                                url = url,
                                isLocal = false
                            };
                            availableWorlds.Add(worldInfo);
                        }
                    }
                }
            }
            else
            {
                Logging.LogWarning("[LobbyManager] NativeHistory not configured.");
            }

            // Update UI
            PopulateWorldSelector();

            yield return null;
        }

        private void PopulateWorldSelector()
        {
            if (worldSelectorParent == null || worldItemPrefab == null)
            {
                Logging.LogWarning("[LobbyManager] World selector not configured.");
                return;
            }

            // Clear existing items
            foreach (Transform child in worldSelectorParent)
            {
                Destroy(child.gameObject);
            }

            // Create new items
            float angle = 0f;
            float angleStep = availableWorlds.Count > 0 ? 360f / availableWorlds.Count : 0f;
            float radius = 1.5f;

            foreach (var world in availableWorlds)
            {
                var item = Instantiate(worldItemPrefab, worldSelectorParent);

                // Position in a circle around player
                float rad = angle * Mathf.Deg2Rad;
                item.transform.localPosition = new Vector3(
                    Mathf.Sin(rad) * radius,
                    0f,
                    Mathf.Cos(rad) * radius
                );

                // Face center
                item.transform.LookAt(worldSelectorParent.position);
                item.transform.Rotate(0, 180, 0);

                // Setup item
                var selector = item.GetComponent<WorldSelectorItem>();
                if (selector != null)
                {
                    selector.Initialize(world, this);
                }

                angle += angleStep;
            }
        }

        #endregion
    }

    /// <summary>
    /// Information about a loadable world.
    /// </summary>
    [Serializable]
    public class WorldInfo
    {
        public string name;
        public string url;
        public string description;
        public Texture2D thumbnail;
        public bool isLocal;
    }
}
