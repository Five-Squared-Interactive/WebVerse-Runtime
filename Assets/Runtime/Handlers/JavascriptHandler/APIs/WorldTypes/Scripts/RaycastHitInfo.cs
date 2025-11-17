// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Struct for raycast hit information.
    /// </summary>
    public struct RaycastHitInfo
    {
        /// <summary>
        /// Entity that was hit.
        /// </summary>
        public BaseEntity entity;

        /// <summary>
        /// Origin from which the ray was cast.
        /// </summary>
        public Vector3 origin;

        /// <summary>
        /// Point (in world coordinates) at which entity was hit.
        /// </summary>
        public Vector3 hitPoint;

        /// <summary>
        /// Normal of the hit point.
        /// </summary>
        public Vector3 hitPointNormal;
    }
}