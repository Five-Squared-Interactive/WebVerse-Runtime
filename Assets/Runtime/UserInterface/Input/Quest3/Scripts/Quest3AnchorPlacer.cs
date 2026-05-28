// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Internal registration data for an anchored entity.
    /// </summary>
    internal class AnchorRegistration
    {
        public string EntityId;
        public AnchorType AnchorType;
        public Vector3 OriginalPosition;
        public Vector3? PositionHint;
        public DetectedPlane TargetPlane;
        public bool IsAnchored;
        public bool IsFloating;
        public Vector3 TargetPosition;
        public Vector3 CurrentPosition;
        public bool IsTransitioning;
        public float TransitionStartTime;
        public Vector3 TransitionStartPosition;
    }

    /// <summary>
    /// Quest 3 implementation of IAnchorPlacer.
    /// Manages anchoring entities to detected real-world surfaces.
    /// </summary>
    public class Quest3AnchorPlacer : IAnchorPlacer
    {
        private const float ANCHOR_TRANSITION_DURATION = 0.3f;

        private readonly ISurfaceDetector _surfaceDetector;
        private readonly Dictionary<string, AnchorRegistration> _registrations
            = new Dictionary<string, AnchorRegistration>();

        /// <summary>
        /// Delegate to resolve entity position by ID. Set by the platform mode.
        /// Returns the entity's current world position, or null if not found.
        /// </summary>
        public System.Func<string, Vector3?> GetEntityPosition { get; set; }

        /// <summary>
        /// Delegate to set entity position by ID. Set by the platform mode.
        /// </summary>
        public System.Action<string, Vector3> SetEntityPosition { get; set; }

        public Quest3AnchorPlacer(ISurfaceDetector surfaceDetector)
        {
            _surfaceDetector = surfaceDetector;
        }

        public bool RegisterAnchor(string entityId, Vector3? positionHint = null)
        {
            return RegisterAnchor(entityId, AnchorType.Floor, positionHint);
        }

        /// <summary>
        /// Register an entity for anchor placement with a specific anchor type.
        /// </summary>
        public bool RegisterAnchor(string entityId, AnchorType anchorType, Vector3? positionHint = null)
        {
            if (string.IsNullOrEmpty(entityId) || anchorType == AnchorType.None) return false;

            var originalPos = GetEntityPosition?.Invoke(entityId) ?? positionHint ?? Vector3.zero;

            var reg = new AnchorRegistration
            {
                EntityId = entityId,
                AnchorType = anchorType,
                OriginalPosition = originalPos,
                PositionHint = positionHint,
                IsAnchored = false,
                IsFloating = true,
                CurrentPosition = originalPos,
                TargetPosition = originalPos
            };

            _registrations[entityId] = reg;

            // Try to anchor immediately
            var planes = _surfaceDetector?.GetPlanes(PlaneType.Any) ?? new List<DetectedPlane>();
            TryAnchor(reg, planes);

            return true;
        }

        public bool UnregisterAnchor(string entityId)
        {
            if (!_registrations.TryGetValue(entityId, out var reg)) return false;

            // Restore original position
            SetEntityPosition?.Invoke(entityId, reg.OriginalPosition);
            _registrations.Remove(entityId);
            return true;
        }

        public bool IsEntityAnchored(string entityId)
        {
            return _registrations.TryGetValue(entityId, out var reg) && reg.IsAnchored;
        }

        public AnchorType GetEntityAnchorType(string entityId)
        {
            return _registrations.TryGetValue(entityId, out var reg) ? reg.AnchorType : AnchorType.None;
        }

        public void OnPlanesUpdated(List<DetectedPlane> planes)
        {
            foreach (var reg in _registrations.Values)
            {
                TryAnchor(reg, planes);
            }
        }

        public void OnModeChanged(XRDisplayMode mode)
        {
            if (mode == XRDisplayMode.VR)
            {
                // Restore all to original positions, keep registrations
                foreach (var reg in _registrations.Values)
                {
                    reg.IsAnchored = false;
                    reg.IsFloating = true;
                    reg.IsTransitioning = false;
                    reg.TargetPlane = null;
                    reg.CurrentPosition = reg.OriginalPosition;
                    SetEntityPosition?.Invoke(reg.EntityId, reg.OriginalPosition);
                }
            }
            else if (mode == XRDisplayMode.AR)
            {
                // Re-anchor with current planes
                var planes = _surfaceDetector?.GetPlanes(PlaneType.Any) ?? new List<DetectedPlane>();
                foreach (var reg in _registrations.Values)
                {
                    TryAnchor(reg, planes);
                }
            }
        }

        /// <summary>
        /// Select the best matching plane for an anchor type and optional position hint.
        /// Pure function over its inputs.
        /// </summary>
        internal DetectedPlane SelectBestPlane(AnchorType type, Vector3? positionHint, List<DetectedPlane> planes)
        {
            var planeType = MapAnchorToPlaneType(type);
            if (planeType == null) return null;

            var candidates = planes.Where(p => p.Classification == planeType.Value).ToList();
            if (candidates.Count == 0) return null;

            if (positionHint.HasValue)
            {
                // Nearest to hint (horizontal distance), tie-break by largest area
                var hint2D = new Vector2(positionHint.Value.x, positionHint.Value.z);
                return candidates
                    .OrderBy(p => Vector2.Distance(hint2D, new Vector2(p.Position.x, p.Position.z)))
                    .ThenByDescending(p => p.Area)
                    .First();
            }
            else
            {
                // Largest area
                return candidates.OrderByDescending(p => p.Area).First();
            }
        }

        private void TryAnchor(AnchorRegistration reg, List<DetectedPlane> planes)
        {
            var bestPlane = SelectBestPlane(reg.AnchorType, reg.PositionHint, planes);

            if (bestPlane != null)
            {
                var targetPos = reg.PositionHint.HasValue
                    ? new Vector3(reg.PositionHint.Value.x, bestPlane.Position.y, reg.PositionHint.Value.z)
                    : new Vector3(reg.OriginalPosition.x, bestPlane.Position.y, reg.OriginalPosition.z);

                reg.TargetPlane = bestPlane;
                reg.TargetPosition = targetPos;
                reg.IsAnchored = true;
                reg.IsFloating = false;

                // For now, instant placement. Smooth transition would be frame-based.
                reg.CurrentPosition = targetPos;
                SetEntityPosition?.Invoke(reg.EntityId, targetPos);
            }
            else
            {
                reg.TargetPlane = null;
                reg.IsAnchored = false;
                reg.IsFloating = true;
            }
        }

        private PlaneType? MapAnchorToPlaneType(AnchorType type)
        {
            switch (type)
            {
                case AnchorType.Floor: return PlaneType.Floor;
                case AnchorType.Wall: return PlaneType.Wall;
                case AnchorType.Table: return PlaneType.Table;
                default: return null;
            }
        }

        /// <summary>
        /// Get all registered entity IDs (for testing/debug).
        /// </summary>
        internal IReadOnlyCollection<string> GetRegisteredEntityIds()
        {
            return _registrations.Keys.ToList().AsReadOnly();
        }

        /// <summary>
        /// Get registration data for testing.
        /// </summary>
        internal AnchorRegistration GetRegistration(string entityId)
        {
            return _registrations.TryGetValue(entityId, out var reg) ? reg : null;
        }
    }
}