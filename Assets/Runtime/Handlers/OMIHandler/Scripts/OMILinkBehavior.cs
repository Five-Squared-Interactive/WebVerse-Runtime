// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI
{
    /// <summary>
    /// Behavior component for OMI link interaction.
    /// When activated (clicked/triggered), navigates to the specified URL.
    /// </summary>
    public class OMILinkBehavior : MonoBehaviour
    {
        /// <summary>
        /// The URL to navigate to when activated.
        /// </summary>
        [Tooltip("The URL to navigate to when activated.")]
        public string Url;

        /// <summary>
        /// Optional display title for the link.
        /// </summary>
        [Tooltip("Optional display title for the link.")]
        public string Title;

        /// <summary>
        /// Whether to show a tooltip/hover indicator.
        /// </summary>
        [Tooltip("Whether to show a tooltip/hover indicator.")]
        public bool ShowTooltip = true;

        /// <summary>
        /// Whether the link is currently hovered.
        /// </summary>
        private bool isHovered = false;

        /// <summary>
        /// Initialize the link behavior.
        /// </summary>
        /// <param name="url">The URL to navigate to.</param>
        /// <param name="title">Optional display title.</param>
        public void Initialize(string url, string title = null)
        {
            Url = url;
            Title = title;
        }

        /// <summary>
        /// Check if the link URL is allowed based on current settings.
        /// </summary>
        private bool IsLinkAllowed()
        {
            if (string.IsNullOrEmpty(Url)) return false;

            // Get settings from the OMI handler if available
            var omiHandler = WebVerseRuntime.Instance?.omiHandler;
            if (omiHandler == null) return true; // Allow if no handler to check

            var settings = omiHandler.Settings;
            
            // Check for external links (http/https)
            bool isExternal = Url.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) ||
                              Url.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase);
            
            if (isExternal && !settings.allowExternalLinks)
            {
                Logging.LogWarning($"[OMILinkBehavior] External links are disabled. Blocked: {Url}");
                return false;
            }

            // Check for world links (.glb, .gltf)
            bool isWorldLink = Url.EndsWith(".glb", System.StringComparison.OrdinalIgnoreCase) ||
                               Url.EndsWith(".gltf", System.StringComparison.OrdinalIgnoreCase);
            
            if (isWorldLink && !settings.allowWorldLinks)
            {
                Logging.LogWarning($"[OMILinkBehavior] World links are disabled. Blocked: {Url}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Activate the link (navigate to URL).
        /// </summary>
        public void Activate()
        {
            if (string.IsNullOrEmpty(Url))
            {
                Logging.LogWarning("[OMILinkBehavior] Cannot activate link with empty URL.");
                return;
            }

            if (!IsLinkAllowed())
            {
                return;
            }

            Logging.Log($"[OMILinkBehavior] Activating link: {Title ?? Url}");

            // Navigate using WebVerseRuntime
            if (WebVerseRuntime.Instance != null)
            {
                WebVerseRuntime.Instance.LoadURL(Url);
            }
            else
            {
                Logging.LogError("[OMILinkBehavior] WebVerseRuntime not available.");
            }
        }

        /// <summary>
        /// Called when the mouse enters the collider.
        /// </summary>
        private void OnMouseEnter()
        {
            isHovered = true;
            OnHoverEnter();
        }

        /// <summary>
        /// Called when the mouse exits the collider.
        /// </summary>
        private void OnMouseExit()
        {
            isHovered = false;
            OnHoverExit();
        }

        /// <summary>
        /// Called when the collider is clicked.
        /// </summary>
        private void OnMouseDown()
        {
            Activate();
        }

        /// <summary>
        /// Called when something enters the trigger.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            // Check if it's the player
            if (IsPlayer(other))
            {
                OnHoverEnter();
            }
        }

        /// <summary>
        /// Called when something exits the trigger.
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other))
            {
                OnHoverExit();
            }
        }

        /// <summary>
        /// Handle hover enter.
        /// </summary>
        private void OnHoverEnter()
        {
            if (ShowTooltip)
            {
                // TODO: Show tooltip UI with link title/URL
                Logging.Log($"[OMILinkBehavior] Hover: {Title ?? Url}");
            }
        }

        /// <summary>
        /// Handle hover exit.
        /// </summary>
        private void OnHoverExit()
        {
            if (ShowTooltip)
            {
                // TODO: Hide tooltip UI
            }
        }

        /// <summary>
        /// Check if a collider belongs to the player.
        /// </summary>
        private bool IsPlayer(Collider other)
        {
            // Check for player tag or component
            // This may need adjustment based on how WebVerse identifies the player
            return other.CompareTag("Player") || 
                   other.GetComponent<FiveSQD.StraightFour.Entity.CharacterEntity>() != null;
        }

        /// <summary>
        /// Get the display name for this link.
        /// </summary>
        public string GetDisplayName()
        {
            return !string.IsNullOrEmpty(Title) ? Title : Url;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw gizmo in editor to visualize the link.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.5f);
            
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                if (col is BoxCollider box)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
                }
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, 0.25f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.8f);
            
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                if (col is BoxCollider box)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
                }
            }
        }
#endif
    }
}
