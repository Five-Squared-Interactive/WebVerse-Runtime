// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
#nullable enable
    /// <summary>
    /// Physical properties for an entity.
    /// </summary>
    public class EntityPhysicalProperties
    {
        /// <summary>
        /// Angular drag of the entity.
        /// </summary>
        public float? angularDrag;

        /// <summary>
        /// Center of mass of the entity.
        /// </summary>
        public Vector3? centerOfMass;

        /// <summary>
        /// Drag of the entity.
        /// </summary>
        public float? drag;

        /// <summary>
        /// Whether or not the entity is gravitational.
        /// </summary>
        public bool? gravitational;

        /// <summary>
        /// Mass of the entity.
        /// </summary>
        public float? mass;

        /// <summary>
        /// Constructor for entity physical properties.
        /// </summary>
        /// <param name="angularDrag">Angular drag of the entity.</param>
        /// <param name="centerOfMass">Center of mass of the entity.</param>
        /// <param name="drag">Drag of the entity.</param>
        /// <param name="gravitational">Whether or not the entity is gravitational.</param>
        /// <param name="mass">Mass of the entity.</param>
        public EntityPhysicalProperties(float? angularDrag, Vector3? centerOfMass, float? drag, bool? gravitational, float? mass)
        {
            this.angularDrag = angularDrag;
            this.centerOfMass = centerOfMass is null ? null : centerOfMass;
            this.drag = drag;
            this.gravitational = gravitational;
            this.mass = mass;
        }
    }
}