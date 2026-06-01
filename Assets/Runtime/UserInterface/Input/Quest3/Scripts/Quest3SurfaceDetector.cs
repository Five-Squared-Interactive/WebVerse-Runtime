// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Tracking state for the surface detector.
    /// </summary>
    public enum TrackingState
    {
        Active,
        Lost,
        Recovering
    }

    /// <summary>
    /// Quest 3 implementation of ISurfaceDetector.
    /// Detects real-world planes via a thin ISceneProvider abstraction.
    /// </summary>
    public class Quest3SurfaceDetector : ISurfaceDetector
    {
        private readonly ISceneProvider _sceneProvider;
        private readonly List<DetectedPlane> _planes = new List<DetectedPlane>();
        private bool _isScanning;
        private TrackingState _trackingState = TrackingState.Active;

        /// <summary>
        /// Anchor placer to notify when planes are updated.
        /// </summary>
        public IAnchorPlacer AnchorPlacer { get; set; }

        /// <summary>
        /// Delegate for AR error handling.
        /// </summary>
        public System.Action<ARErrorType> OnARError { get; set; }

        /// <summary>
        /// Delegate notified when tracking state changes.
        /// </summary>
        public System.Action<TrackingState> OnTrackingStateChanged { get; set; }

        /// <summary>
        /// Current tracking state.
        /// </summary>
        public TrackingState CurrentTrackingState => _trackingState;

        /// <summary>
        /// Whether the detector is currently scanning for surfaces.
        /// </summary>
        public bool IsScanning => _isScanning;

        public Quest3SurfaceDetector(ISceneProvider sceneProvider)
        {
            _sceneProvider = sceneProvider;

            if (_sceneProvider != null)
            {
                _sceneProvider.OnSceneModelUpdated += HandleSceneModelUpdated;
                _sceneProvider.OnTrackingLost += HandleTrackingLost;
                _sceneProvider.OnTrackingAcquired += HandleTrackingAcquired;
            }
        }

        /// <summary>
        /// Start scanning for real-world surfaces.
        /// </summary>
        public void StartScanning()
        {
            if (_sceneProvider == null) return;
            _sceneProvider.StartCapture();
            _isScanning = true;
            Logging.Log("[Quest3SurfaceDetector] Scanning started.");
        }

        /// <summary>
        /// Stop scanning for real-world surfaces.
        /// </summary>
        public void StopScanning()
        {
            if (_sceneProvider == null) return;
            _sceneProvider.StopCapture();
            _isScanning = false;
            _planes.Clear();
            _trackingState = TrackingState.Active;
            Logging.Log("[Quest3SurfaceDetector] Scanning stopped, planes cleared.");
        }

        /// <summary>
        /// Get detected planes filtered by type, sorted by area descending.
        /// Returns a defensive copy.
        /// </summary>
        public List<DetectedPlane> GetPlanes(PlaneType planeType)
        {
            IEnumerable<DetectedPlane> filtered = _planes;
            if (planeType != PlaneType.Any)
            {
                filtered = _planes.Where(p => p.Classification == planeType);
            }
            return filtered.OrderByDescending(p => p.Area).ToList();
        }

        /// <summary>
        /// Simulate tracking loss for testing.
        /// </summary>
        public void SimulateTrackingLost()
        {
            HandleTrackingLost();
        }

        /// <summary>
        /// Simulate tracking recovery for testing.
        /// </summary>
        public void SimulateTrackingAcquired()
        {
            HandleTrackingAcquired();
        }

        private void HandleSceneModelUpdated()
        {
            if (_trackingState == TrackingState.Lost) return;

            UpdatePlanesFromProvider();
        }

        private void HandleTrackingLost()
        {
            if (_trackingState == TrackingState.Lost) return;
            _trackingState = TrackingState.Lost;
            Logging.LogWarning("[Quest3SurfaceDetector] Tracking lost, freezing plane cache.");
            OnARError?.Invoke(ARErrorType.SurfaceDetectionFailed);
            OnTrackingStateChanged?.Invoke(_trackingState);
        }

        private void HandleTrackingAcquired()
        {
            if (_trackingState != TrackingState.Lost) return;
            _trackingState = TrackingState.Recovering;
            Logging.Log("[Quest3SurfaceDetector] Tracking re-acquired, updating planes.");

            UpdatePlanesFromProvider();

            _trackingState = TrackingState.Active;
            OnTrackingStateChanged?.Invoke(_trackingState);
        }

        private void UpdatePlanesFromProvider()
        {
            if (_sceneProvider == null) return;

            var anchors = _sceneProvider.GetAnchors();
            var anchorIds = new HashSet<string>();

            foreach (var anchor in anchors)
            {
                anchorIds.Add(anchor.Uuid);
                var classification = MapLabel(anchor.SemanticLabel);
                var existing = _planes.FirstOrDefault(p => p.Id == anchor.Uuid);

                if (existing != null)
                {
                    existing.Position = anchor.Position;
                    existing.Rotation = anchor.Rotation;
                    existing.Bounds = anchor.PlaneBounds;
                    existing.Area = anchor.PlaneBounds.x * anchor.PlaneBounds.y;
                    existing.Classification = classification;
                }
                else
                {
                    _planes.Add(new DetectedPlane
                    {
                        Id = anchor.Uuid,
                        Classification = classification,
                        Position = anchor.Position,
                        Rotation = anchor.Rotation,
                        Bounds = anchor.PlaneBounds,
                        Area = anchor.PlaneBounds.x * anchor.PlaneBounds.y
                    });
                }
            }

            // Remove planes whose anchors no longer exist
            _planes.RemoveAll(p => !anchorIds.Contains(p.Id));

            // Notify anchor placer
            AnchorPlacer?.OnPlanesUpdated(_planes);
        }

        private PlaneType MapLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return PlaneType.Any;
            switch (label.ToUpperInvariant())
            {
                case "FLOOR": return PlaneType.Floor;
                case "TABLE": return PlaneType.Table;
                case "WALL_FACE":
                case "WALL": return PlaneType.Wall;
                default: return PlaneType.Any;
            }
        }

        /// <summary>
        /// Clean up event subscriptions.
        /// </summary>
        public void Dispose()
        {
            if (_sceneProvider != null)
            {
                _sceneProvider.OnSceneModelUpdated -= HandleSceneModelUpdated;
                _sceneProvider.OnTrackingLost -= HandleTrackingLost;
                _sceneProvider.OnTrackingAcquired -= HandleTrackingAcquired;
            }
        }
    }
}