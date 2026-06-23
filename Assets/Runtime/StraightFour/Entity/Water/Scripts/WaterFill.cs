// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Pure geometry for filling a volume to a height. Given a world-space basin AABB and a
    /// water level, it derives the <see cref="WaterVolume"/>, the surface transform, and
    /// validation/info flags. No Unity lifecycle dependency — fully unit-testable.
    /// </summary>
    public static class WaterFill
    {
        /// <summary>
        /// Outcome of a fill plan.
        /// </summary>
        public enum FillResult
        {
            /// <summary>Plan is usable.</summary>
            Ok,

            /// <summary>Footprint has zero area in XZ; nothing to fill.</summary>
            DegenerateBounds
        }

        /// <summary>
        /// A computed fill plan: the resulting volume, where to place the surface, and flags.
        /// </summary>
        public struct FillPlan
        {
            public FillResult result;
            public WaterVolume volume;
            public Vector3 surfacePosition;
            public Vector3 surfaceScale;
            public bool levelBelowFloor;
            public bool levelAboveTop;
        }

        /// <summary>
        /// Plan a water fill for the given world-space basin bounds and water level.
        /// </summary>
        /// <param name="worldBounds">World-space AABB of the basin/region to fill.</param>
        /// <param name="waterLevel">World-space Y of the water surface.</param>
        /// <param name="surfaceThicknessY">Local Y scale for the surface plane (cosmetic).</param>
        /// <returns>The fill plan. Check <see cref="FillPlan.result"/> before using it.</returns>
        public static FillPlan Plan(Bounds worldBounds, float waterLevel, float surfaceThicknessY = 1f)
        {
            FillPlan plan = new FillPlan();

            if (!float.IsFinite(waterLevel)
                || !float.IsFinite(worldBounds.center.x) || !float.IsFinite(worldBounds.center.z)
                || !float.IsFinite(worldBounds.size.x) || !float.IsFinite(worldBounds.size.z)
                || worldBounds.size.x <= 0f || worldBounds.size.z <= 0f)
            {
                plan.result = FillResult.DegenerateBounds;
                return plan;
            }

            plan.result = FillResult.Ok;
            plan.volume = new WaterVolume(waterLevel, worldBounds);
            plan.surfacePosition = new Vector3(worldBounds.center.x, waterLevel, worldBounds.center.z);
            plan.surfaceScale = new Vector3(worldBounds.size.x, surfaceThicknessY, worldBounds.size.z);
            plan.levelBelowFloor = waterLevel < worldBounds.min.y;
            plan.levelAboveTop = waterLevel > worldBounds.max.y;
            return plan;
        }

        /// <summary>
        /// Compute the world-space AABB encapsulating all renderers under a root. For a rotated
        /// root this is the world AABB (larger than the silhouette) — by design.
        /// </summary>
        /// <param name="root">Root GameObject whose child renderers define the bounds.</param>
        /// <param name="bounds">The resulting world-space bounds, if any.</param>
        /// <returns>True if at least one renderer was found.</returns>
        public static bool TryGetWorldRendererBounds(GameObject root, out Bounds bounds)
        {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
            if (root == null)
            {
                return false;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            bool initialized = false;
            foreach (Renderer r in renderers)
            {
                if (r == null)
                {
                    continue;
                }
                if (!initialized)
                {
                    bounds = r.bounds;
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            return initialized;
        }
    }
}
