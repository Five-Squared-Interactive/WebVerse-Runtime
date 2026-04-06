// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

namespace FiveSQD.WebVerse.Lobby
{
    /// <summary>
    /// Interactive world selector item for the lobby.
    /// Handles hover, select, and launch interactions.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class WorldSelectorItem : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI Elements")]
        [Tooltip("Text displaying the world name.")]
        public TextMeshPro nameText;

        [Tooltip("Text displaying the world description.")]
        public TextMeshPro descriptionText;

        [Tooltip("Renderer for the thumbnail/icon.")]
        public Renderer thumbnailRenderer;

        [Tooltip("The main wireframe visual.")]
        public Renderer wireframeRenderer;

        [Header("Hover Effects")]
        [Tooltip("Scale multiplier when hovered.")]
        public float hoverScale = 1.2f;

        [Tooltip("Glow intensity when hovered.")]
        public float hoverGlowIntensity = 1.5f;

        [Tooltip("Animation speed for hover transitions.")]
        public float hoverTransitionSpeed = 8f;

        [Header("Colors")]
        [Tooltip("Default wireframe color.")]
        public Color defaultColor = new Color(0.7f, 0.9f, 1f, 1f);

        [Tooltip("Hovered wireframe color.")]
        public Color hoverColor = new Color(1f, 0.8f, 0.4f, 1f);

        [Tooltip("Selected wireframe color.")]
        public Color selectedColor = new Color(1f, 0.5f, 0.2f, 1f);

        #endregion

        #region Private Fields

        private WorldInfo worldInfo;
        private LobbyManager lobbyManager;
        private XRSimpleInteractable interactable;
        private Material wireframeMaterial;

        private Vector3 originalScale;
        private float targetScale = 1f;
        private Color targetColor;
        private float targetGlow = 0.8f;
        private bool isHovered;
        private bool isSelected;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            originalScale = transform.localScale;
            targetColor = defaultColor;

            if (wireframeRenderer != null)
            {
                wireframeMaterial = wireframeRenderer.material;
            }
        }

        private void OnEnable()
        {
            if (interactable != null)
            {
                interactable.hoverEntered.AddListener(OnHoverEnter);
                interactable.hoverExited.AddListener(OnHoverExit);
                interactable.selectEntered.AddListener(OnSelect);
            }
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.hoverEntered.RemoveListener(OnHoverEnter);
                interactable.hoverExited.RemoveListener(OnHoverExit);
                interactable.selectEntered.RemoveListener(OnSelect);
            }
        }

        private void Update()
        {
            // Animate scale
            float currentScaleFactor = transform.localScale.x / originalScale.x;
            float newScaleFactor = Mathf.Lerp(currentScaleFactor, targetScale, Time.deltaTime * hoverTransitionSpeed);
            transform.localScale = originalScale * newScaleFactor;

            // Animate color and glow
            if (wireframeMaterial != null)
            {
                if (wireframeMaterial.HasProperty("_Color"))
                {
                    Color currentColor = wireframeMaterial.GetColor("_Color");
                    wireframeMaterial.SetColor("_Color", Color.Lerp(currentColor, targetColor, Time.deltaTime * hoverTransitionSpeed));
                }

                if (wireframeMaterial.HasProperty("_GlowIntensity"))
                {
                    float currentGlow = wireframeMaterial.GetFloat("_GlowIntensity");
                    wireframeMaterial.SetFloat("_GlowIntensity", Mathf.Lerp(currentGlow, targetGlow, Time.deltaTime * hoverTransitionSpeed));
                }
            }
        }

        private void OnDestroy()
        {
            // Clean up instanced material
            if (wireframeMaterial != null)
            {
                Destroy(wireframeMaterial);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize this world selector item.
        /// </summary>
        public void Initialize(WorldInfo world, LobbyManager manager)
        {
            worldInfo = world;
            lobbyManager = manager;

            // Update UI
            if (nameText != null)
            {
                nameText.text = world.name;
            }

            if (descriptionText != null)
            {
                descriptionText.text = world.description ?? "";
            }

            if (thumbnailRenderer != null && world.thumbnail != null)
            {
                thumbnailRenderer.material.mainTexture = world.thumbnail;
            }
        }

        #endregion

        #region Interaction Handlers

        private void OnHoverEnter(HoverEnterEventArgs args)
        {
            if (isSelected) return;

            isHovered = true;
            targetScale = hoverScale;
            targetColor = hoverColor;
            targetGlow = hoverGlowIntensity;
        }

        private void OnHoverExit(HoverExitEventArgs args)
        {
            if (isSelected) return;

            isHovered = false;
            targetScale = 1f;
            targetColor = defaultColor;
            targetGlow = 0.8f;
        }

        private void OnSelect(SelectEnterEventArgs args)
        {
            if (lobbyManager == null || worldInfo == null) return;

            isSelected = true;
            targetColor = selectedColor;
            targetGlow = 2f;

            // Trigger world launch
            lobbyManager.SelectWorld(worldInfo);
        }

        #endregion
    }
}
