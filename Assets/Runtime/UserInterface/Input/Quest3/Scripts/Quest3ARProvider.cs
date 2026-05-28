// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Quest 3 implementation of IARProvider.
    /// Manages passthrough rendering via a thin IPassthroughLayer abstraction.
    /// Plain C# class (not MonoBehaviour) for testability.
    /// </summary>
    public class Quest3ARProvider : IARProvider
    {
        private readonly IPassthroughLayer _passthroughLayer;
        private readonly Camera _camera;
        private XRDisplayMode _currentDisplayMode = XRDisplayMode.VR;
        private CameraClearFlags _originalClearFlags;
        private Color _originalBackgroundColor;
        private bool _originalFlagsSaved;

        /// <summary>
        /// Delegate for AR error handling. Set by InputModeManager to receive error notifications.
        /// </summary>
        public Action<ARErrorType> OnARError { get; set; }

        /// <summary>
        /// Create a new Quest3ARProvider.
        /// </summary>
        /// <param name="passthroughLayer">Passthrough layer wrapper to control.</param>
        /// <param name="camera">Main camera for clear flag management. May be null in tests.</param>
        public Quest3ARProvider(IPassthroughLayer passthroughLayer, Camera camera = null)
        {
            _passthroughLayer = passthroughLayer;
            _camera = camera;
        }

        /// <summary>
        /// Current XR display mode.
        /// </summary>
        public XRDisplayMode CurrentDisplayMode => _currentDisplayMode;

        /// <summary>
        /// Whether passthrough is supported on this device.
        /// </summary>
        public bool IsPassthroughSupported => _passthroughLayer != null;

        /// <summary>
        /// Enable passthrough rendering, switching to AR mode.
        /// Catches exceptions from the passthrough layer and falls back to VR mode.
        /// </summary>
        public void EnablePassthrough()
        {
            if (_passthroughLayer == null)
            {
                Logging.LogWarning("[Quest3ARProvider] No passthrough layer available.");
                return;
            }

            try
            {
                _passthroughLayer.Enable();
                _currentDisplayMode = XRDisplayMode.AR;

                if (_camera != null)
                {
                    if (!_originalFlagsSaved)
                    {
                        _originalClearFlags = _camera.clearFlags;
                        _originalBackgroundColor = _camera.backgroundColor;
                        _originalFlagsSaved = true;
                    }
                    _camera.clearFlags = CameraClearFlags.SolidColor;
                    _camera.backgroundColor = Color.clear;
                }

                Logging.Log("[Quest3ARProvider] Passthrough enabled, mode set to AR.");
            }
            catch (Exception ex)
            {
                Logging.LogWarning($"[Quest3ARProvider] Passthrough initialization failed: {ex.Message}");
                _currentDisplayMode = XRDisplayMode.VR;

                // Defensive cleanup
                try { _passthroughLayer.Disable(); } catch { }

                OnARError?.Invoke(ARErrorType.PassthroughFailed);
            }
        }

        /// <summary>
        /// Disable passthrough rendering, switching to VR mode.
        /// </summary>
        public void DisablePassthrough()
        {
            if (_passthroughLayer == null) return;

            _passthroughLayer.Disable();
            _currentDisplayMode = XRDisplayMode.VR;

            if (_camera != null && _originalFlagsSaved)
            {
                _camera.clearFlags = _originalClearFlags;
                _camera.backgroundColor = _originalBackgroundColor;
            }

            Logging.Log("[Quest3ARProvider] Passthrough disabled, mode set to VR.");
        }

        /// <summary>
        /// Called when a mid-session passthrough failure is detected.
        /// Falls back to VR mode via the same error handling path.
        /// </summary>
        public void HandleMidSessionFailure()
        {
            if (_currentDisplayMode != XRDisplayMode.AR) return;

            Logging.LogWarning("[Quest3ARProvider] Mid-session passthrough failure detected, falling back to VR.");
            DisablePassthrough();
            OnARError?.Invoke(ARErrorType.PassthroughFailed);
        }
    }
}