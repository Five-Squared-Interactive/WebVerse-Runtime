// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Visual feedback state for surface scanning.
    /// </summary>
    public enum ScanningFeedbackState
    {
        Hidden,
        Scanning,
        NoSurfacesWarning,
        Relocating
    }

    /// <summary>
    /// Controls visual feedback during AR surface scanning.
    /// Manages state transitions based on ISurfaceDetector and tracking state.
    /// Plain C# class for testability; UI rendering handled separately.
    /// </summary>
    public class ScanningFeedbackController
    {
        private readonly ISurfaceDetector _surfaceDetector;
        private ScanningFeedbackState _state = ScanningFeedbackState.Hidden;
        private float _scanStartTime;
        private readonly float _noSurfacesTimeoutSeconds;

        /// <summary>
        /// Current feedback state.
        /// </summary>
        public ScanningFeedbackState State => _state;

        /// <summary>
        /// Current feedback message to display (or null if hidden).
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Create a new ScanningFeedbackController.
        /// </summary>
        /// <param name="surfaceDetector">Surface detector to monitor.</param>
        /// <param name="noSurfacesTimeoutSeconds">Seconds before showing "no surfaces" warning. Default 5s.</param>
        public ScanningFeedbackController(ISurfaceDetector surfaceDetector, float noSurfacesTimeoutSeconds = 5f)
        {
            _surfaceDetector = surfaceDetector;
            _noSurfacesTimeoutSeconds = noSurfacesTimeoutSeconds;
        }

        /// <summary>
        /// Notify that scanning has started.
        /// </summary>
        public void OnScanningStarted()
        {
            _state = ScanningFeedbackState.Scanning;
            Message = "Scanning for surfaces...";
            _scanStartTime = GetTime();
            Logging.Log("[ScanningFeedbackController] Scanning state entered.");
        }

        /// <summary>
        /// Notify that scanning has stopped externally.
        /// </summary>
        public void OnScanningStopped()
        {
            _state = ScanningFeedbackState.Hidden;
            Message = null;
        }

        /// <summary>
        /// Notify that tracking has been lost.
        /// </summary>
        public void ShowRelocating()
        {
            _state = ScanningFeedbackState.Relocating;
            Message = "Relocating surfaces...";
            Logging.Log("[ScanningFeedbackController] Relocating state entered.");
        }

        /// <summary>
        /// Notify that tracking has been recovered.
        /// </summary>
        public void HideRelocating()
        {
            if (_state == ScanningFeedbackState.Relocating)
            {
                _state = ScanningFeedbackState.Hidden;
                Message = null;
            }
        }

        /// <summary>
        /// Call each frame to update feedback state based on detector status.
        /// </summary>
        /// <param name="currentTime">Current time in seconds (Time.time or mockable).</param>
        public void Update(float currentTime)
        {
            if (_state == ScanningFeedbackState.Relocating) return;

            if (!_surfaceDetector.IsScanning)
            {
                if (_state != ScanningFeedbackState.Hidden)
                {
                    _state = ScanningFeedbackState.Hidden;
                    Message = null;
                }
                return;
            }

            int planeCount = _surfaceDetector.GetPlanes(PlaneType.Any).Count;

            if (planeCount > 0 && (_state == ScanningFeedbackState.Scanning || _state == ScanningFeedbackState.NoSurfacesWarning))
            {
                _state = ScanningFeedbackState.Hidden;
                Message = null;
                return;
            }

            if (_state == ScanningFeedbackState.Scanning && planeCount == 0)
            {
                float elapsed = currentTime - _scanStartTime;
                if (elapsed >= _noSurfacesTimeoutSeconds)
                {
                    _state = ScanningFeedbackState.NoSurfacesWarning;
                    Message = "No surfaces detected. Move to a clear area.";
                }
            }
        }

        /// <summary>
        /// Override for time source. Defaults to 0; tests provide mock time via Update parameter.
        /// </summary>
        protected virtual float GetTime()
        {
            return 0f;
        }
    }
}