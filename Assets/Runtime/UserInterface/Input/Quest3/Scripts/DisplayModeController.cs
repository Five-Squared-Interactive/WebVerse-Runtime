// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Snapshot of an entity's transform at the time of a mode switch.
    /// </summary>
    public struct TransformSnapshot
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }

    /// <summary>
    /// Orchestrates VR/AR mode switching on Quest 3.
    /// Manages the transition sequence: fade -> passthrough toggle -> entity repositioning -> fade.
    /// Used by Quest3Mode; not directly tied to UI.
    /// </summary>
    public class DisplayModeController
    {
        private readonly InputManager _inputManager;
        private XRDisplayMode _currentDisplayMode = XRDisplayMode.VR;
        private bool _isTransitioning;
        private Dictionary<string, TransformSnapshot> _transformSnapshots
            = new Dictionary<string, TransformSnapshot>();

        /// <summary>
        /// Current display mode (VR or AR).
        /// </summary>
        public XRDisplayMode CurrentDisplayMode => _currentDisplayMode;

        /// <summary>
        /// Whether a mode transition is currently in progress.
        /// </summary>
        public bool IsTransitioning => _isTransitioning;

        public DisplayModeController(InputManager inputManager)
        {
            _inputManager = inputManager;
        }

        /// <summary>
        /// Toggle between VR and AR modes.
        /// Ignored if a transition is already in progress or if IARProvider is null.
        /// </summary>
        public void ToggleDisplayMode()
        {
            if (_isTransitioning) return;
            if (_inputManager?.arProvider == null) return;

            if (_currentDisplayMode == XRDisplayMode.VR)
                SwitchToAR();
            else
                SwitchToVR();
        }

        /// <summary>
        /// Apply a world-specified mode preference.
        /// </summary>
        /// <param name="mode">Mode string from VEML metadata ("ar", "vr", "hybrid").</param>
        public void ApplyWorldMode(string mode)
        {
            if (_inputManager?.arProvider == null) return;

            var normalized = (mode ?? "").ToLowerInvariant();
            switch (normalized)
            {
                case "ar":
                    if (_currentDisplayMode != XRDisplayMode.AR) SwitchToAR();
                    break;
                case "vr":
                    if (_currentDisplayMode != XRDisplayMode.VR) SwitchToVR();
                    break;
                case "hybrid":
                default:
                    // Stay in current mode (VR default on fresh load)
                    break;
            }
        }

        /// <summary>
        /// Execute the VR-to-AR transition sequence.
        /// </summary>
        public void SwitchToAR()
        {
            if (_isTransitioning || _inputManager?.arProvider == null) return;
            _isTransitioning = true;

            var fade = _inputManager.fadeTransition;
            if (fade != null)
            {
                fade.FadeOut(() =>
                {
                    PerformSwitchToAR();
                    fade.FadeIn();
                    _isTransitioning = false;
                });
            }
            else
            {
                PerformSwitchToAR();
                _isTransitioning = false;
            }
        }

        /// <summary>
        /// Execute the AR-to-VR transition sequence.
        /// </summary>
        public void SwitchToVR()
        {
            if (_isTransitioning || _inputManager?.arProvider == null) return;
            _isTransitioning = true;

            var fade = _inputManager.fadeTransition;
            if (fade != null)
            {
                fade.FadeOut(() =>
                {
                    PerformSwitchToVR();
                    fade.FadeIn();
                    _isTransitioning = false;
                });
            }
            else
            {
                PerformSwitchToVR();
                _isTransitioning = false;
            }
        }

        private void PerformSwitchToAR()
        {
            _inputManager.arProvider.EnablePassthrough();
            _inputManager.surfaceDetector?.StartScanning();
            _inputManager.anchorPlacer?.OnModeChanged(XRDisplayMode.AR);
            _currentDisplayMode = XRDisplayMode.AR;
            Logging.Log("[DisplayModeController] Switched to AR mode.");
        }

        private void PerformSwitchToVR()
        {
            _inputManager.arProvider.DisablePassthrough();
            _inputManager.anchorPlacer?.OnModeChanged(XRDisplayMode.VR);
            _currentDisplayMode = XRDisplayMode.VR;
            Logging.Log("[DisplayModeController] Switched to VR mode.");
        }
    }
}