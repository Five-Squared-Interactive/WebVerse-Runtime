// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Interface.TabUI
{
    /// <summary>
    /// Raycast filter for the Chrome WebView overlay.
    /// Blocks raycasts in the content area so clicks pass through to
    /// the Content WebView underneath, while allowing clicks on the
    /// chrome bar and overlays (modals/dropdowns).
    /// </summary>
    public class ChromeInputFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        /// <summary>
        /// Height in pixels from the top of the screen where the chrome bar lives.
        /// Matches the CSS: spacing-md(16) + bar-height(96) + spacing-sm(8) = 120.
        /// </summary>
        public float chromeHeight = 120f;

        /// <summary>
        /// When true, allows raycasts everywhere (for modals/dropdowns that
        /// extend into the content area).
        /// </summary>
        public bool allowFullScreenInput = false;

        /// <summary>
        /// Optional secondary hit rect in screen coordinates (bottom-left origin).
        /// Used for the stats HUD overlay.
        /// </summary>
        public Rect? secondaryHitRect = null;

        /// <summary>
        /// Determines whether the given screen point should be considered
        /// a valid raycast hit on this graphic.
        /// </summary>
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (allowFullScreenInput) return true;

            // Allow raycasts only in the chrome bar area (top of screen).
            // Screen coordinates: (0,0) = bottom-left, (width,height) = top-right.
            if (screenPoint.y >= (Screen.height - chromeHeight)) return true;

            // Allow raycasts in secondary hit rect (e.g. stats HUD).
            if (secondaryHitRect.HasValue && secondaryHitRect.Value.Contains(screenPoint))
                return true;

            return false;
        }
    }
}
