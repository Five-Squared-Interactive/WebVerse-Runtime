// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Motion state for an entity.
    /// </summary>
    public struct EntityMotion
    {
        /// <summary>
        /// Angular velocity of the entity.
        /// </summary>
        public Vector3 angularVelocity;

        /// <summary>
        /// Whether or not the entity is stationary.
        /// </summary>
        public bool stationary;

        /// <summary>
        /// Velocity of the entity.
        /// </summary>
        public Vector3 velocity;
    }
}