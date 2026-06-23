// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Pure value object describing a filled water volume: a horizontal surface at a
    /// world-space height (<see cref="waterLevel"/>) bounded by an XZ <see cref="footprint"/>.
    ///
    /// This type has no Unity lifecycle dependency. It is the single seam that buoyancy,
    /// submersion, and fill-to-height all consume — keep it free of MonoBehaviour so it
    /// stays trivially unit-testable.
    /// </summary>
    public struct WaterVolume
    {
        /// <summary>
        /// World-space Y of the water surface.
        /// </summary>
        public float waterLevel;

        /// <summary>
        /// World-space bounds of the volume. Only the X and Z extents are used for the
        /// inside-test; the Y extent is ignored (the surface height is <see cref="waterLevel"/>).
        /// </summary>
        public Bounds footprint;

        /// <summary>
        /// Create a water volume.
        /// </summary>
        /// <param name="waterLevel">World-space Y of the water surface.</param>
        /// <param name="footprint">World-space bounds; X/Z used for the inside-test.</param>
        public WaterVolume(float waterLevel, Bounds footprint)
        {
            this.waterLevel = waterLevel;
            this.footprint = footprint;
        }

        /// <summary>
        /// Whether a world-space point is inside the footprint in XZ (inclusive of the edge).
        /// </summary>
        /// <param name="worldPoint">Point to test.</param>
        /// <returns>True if the point's X and Z lie within the footprint extents.</returns>
        public bool InsideFootprint(Vector3 worldPoint)
        {
            return worldPoint.x >= footprint.min.x && worldPoint.x <= footprint.max.x
                && worldPoint.z >= footprint.min.z && worldPoint.z <= footprint.max.z;
        }

        /// <summary>
        /// Whether a world-space point is underwater: strictly below the surface and inside
        /// the footprint. A point exactly at the surface is not considered underwater.
        /// </summary>
        /// <param name="worldPoint">Point to test.</param>
        /// <returns>True if submerged.</returns>
        public bool IsUnderwater(Vector3 worldPoint)
        {
            return worldPoint.y < waterLevel && InsideFootprint(worldPoint);
        }

        /// <summary>
        /// Depth of a world-space point below the surface, or 0 if at/above the surface or
        /// outside the footprint.
        /// </summary>
        /// <param name="worldPoint">Point to test.</param>
        /// <returns>Submerged depth in world units (>= 0).</returns>
        public float SubmergedDepth(Vector3 worldPoint)
        {
            if (!InsideFootprint(worldPoint))
            {
                return 0f;
            }
            return Mathf.Max(0f, waterLevel - worldPoint.y);
        }
    }
}
