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
        /// Height in local pixels where the chrome bar lives.
        /// Desktop: measured from the top of the screen.
        /// Mobile: measured from the bottom of the screen.
        /// VR: measured from the bottom of the panel.
        /// Matches the CSS: spacing-md(16) + bar-height(96) + spacing-sm(8) = 120.
        /// </summary>
        public float chromeHeight = 120f;

        /// <summary>
        /// When true, chrome bar is at the bottom of the screen (mobile mode).
        /// </summary>
        public bool chromeAtBottom = false;

        /// <summary>
        /// When true, allows raycasts everywhere (for modals/dropdowns that
        /// extend into the content area).
        /// </summary>
        public bool allowFullScreenInput = false;

        /// <summary>
        /// When true, VR mode is active and local-coordinate filtering is
        /// used instead of screen-space filtering.
        /// </summary>
        public bool vrMode = false;

        /// <summary>
        /// Optional secondary hit rect in screen coordinates (bottom-left origin).
        /// Used for the stats HUD overlay.
        /// </summary>
        public Rect? secondaryHitRect = null;

        private RectTransform cachedRT;
        private int logFrameCounter;
        private bool hasLoggedConfig = false;

        /// <summary>
        /// Determines whether the given screen point should be considered
        /// a valid raycast hit on this graphic.
        /// </summary>
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            // One-time config log to confirm filter is active and correctly configured
            if (!hasLoggedConfig)
            {
                hasLoggedConfig = true;
                Debug.Log($"[ChromeInputFilter] Config: chromeAtBottom={chromeAtBottom}, " +
                    $"chromeHeight={chromeHeight:F0}, vrMode={vrMode}, " +
                    $"Screen=({Screen.width}x{Screen.height}), allowFullScreen={allowFullScreenInput}");
            }

            if (allowFullScreenInput) return true;

            if (vrMode)
            {
                // In VR, convert screen-space hit point back to local coordinates
                // on this RawImage's RectTransform so we can check if it's in
                // the chrome bar region (bottom of the panel).
                if (cachedRT == null) cachedRT = transform as RectTransform;
                if (cachedRT != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    cachedRT, screenPoint, eventCamera, out Vector2 localPoint))
                {
                    // rect.yMin is the bottom edge in local space.
                    // Chrome bar occupies [yMin, yMin + chromeHeight].
                    float chromeBarTop = cachedRT.rect.yMin + chromeHeight;
                    bool inChromeBar = localPoint.y <= chromeBarTop;

                    // Diagnostic logging (every ~60 calls to avoid spam)
                    if (++logFrameCounter >= 60)
                    {
                        logFrameCounter = 0;
                        Debug.Log($"[ChromeInputFilter VR] local=({localPoint.x:F0},{localPoint.y:F0}), " +
                            $"rect=({cachedRT.rect.yMin:F0} to {cachedRT.rect.yMax:F0}), " +
                            $"chromeBarTop={chromeBarTop:F0}, hit={inChromeBar}");
                    }

                    return inChromeBar;
                }
                // Conversion failed — allow raycast as fallback
                return true;
            }

            bool hit;

            // Screen coordinates: (0,0) = bottom-left, (width,height) = top-right.
            if (chromeAtBottom)
            {
                // Mobile: chrome bar at bottom of screen.
                hit = screenPoint.y <= chromeHeight;
            }
            else
            {
                // Desktop: chrome bar at top of screen.
                hit = screenPoint.y >= (Screen.height - chromeHeight);
            }

            // Allow raycasts in secondary hit rect (e.g. stats HUD).
            if (!hit && secondaryHitRect.HasValue && secondaryHitRect.Value.Contains(screenPoint))
                hit = true;

            // Periodic diagnostic logging for non-VR modes
            if (++logFrameCounter >= 120)
            {
                logFrameCounter = 0;
                Debug.Log($"[ChromeInputFilter] screenPoint=({screenPoint.x:F0},{screenPoint.y:F0}), " +
                    $"chromeHeight={chromeHeight:F0}, chromeAtBottom={chromeAtBottom}, hit={hit}");
            }

            return hit;
        }
    }
}
