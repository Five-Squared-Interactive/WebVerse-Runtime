// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Interface for AR content anchoring.
    /// Manages anchoring entities to detected real-world surfaces.
    /// InputManager holds a nullable reference; null on non-AR platforms.
    /// </summary>
    public interface IAnchorPlacer
    {
        /// <summary>
        /// Register an entity for anchor placement on a detected surface.
        /// </summary>
        /// <param name="entityId">Unique ID of the entity to anchor.</param>
        /// <param name="positionHint">Optional preferred world position hint.</param>
        /// <returns>True if anchor registration succeeded.</returns>
        bool RegisterAnchor(string entityId, Vector3? positionHint = null);

        /// <summary>
        /// Unregister an entity from anchor placement.
        /// </summary>
        /// <param name="entityId">Unique ID of the entity to unanchor.</param>
        /// <returns>True if the entity was anchored and has been removed.</returns>
        bool UnregisterAnchor(string entityId);

        /// <summary>
        /// Check whether an entity is currently anchored to a surface.
        /// </summary>
        /// <param name="entityId">Unique ID of the entity.</param>
        /// <returns>True if the entity is anchored.</returns>
        bool IsEntityAnchored(string entityId);

        /// <summary>
        /// Get the anchor type for an entity.
        /// </summary>
        /// <param name="entityId">Unique ID of the entity.</param>
        /// <returns>The anchor type, or AnchorType.None if not anchored.</returns>
        AnchorType GetEntityAnchorType(string entityId);

        /// <summary>
        /// Notify the anchor placer that detected planes have been updated.
        /// Called by the surface detector when planes change.
        /// </summary>
        /// <param name="planes">Updated list of detected planes.</param>
        void OnPlanesUpdated(List<DetectedPlane> planes);

        /// <summary>
        /// Notify the anchor placer that the display mode has changed.
        /// </summary>
        /// <param name="mode">The new display mode.</param>
        void OnModeChanged(XRDisplayMode mode);
    }
}