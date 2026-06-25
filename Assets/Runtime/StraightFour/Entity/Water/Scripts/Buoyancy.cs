// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Per-body buoyancy configuration.
    /// </summary>
    public struct BuoyancyProfile
    {
        /// <summary>Upward force applied per metre of submerged depth.</summary>
        public float strength;

        /// <summary>Depth (m) at which the upward force stops growing (≈ body height).</summary>
        public float submersionDepthCap;

        /// <summary>Linear damping applied while submerged.</summary>
        public float submergedLinearDrag;

        /// <summary>Angular damping applied while submerged.</summary>
        public float submergedAngularDrag;

        /// <summary>
        /// Strength of the horizontal force from wave slope (water shoving the body along the
        /// surface). 0 = no lateral push (objects only bob in place).
        /// </summary>
        public float lateralStrength;

        /// <summary>A reasonable default profile for a ~1 m, near-neutral object.</summary>
        public static BuoyancyProfile Default
        {
            get
            {
                return new BuoyancyProfile
                {
                    strength = 30f,
                    submersionDepthCap = 1f,
                    submergedLinearDrag = 2f,
                    submergedAngularDrag = 1f,
                    lateralStrength = 12f
                };
            }
        }
    }

    /// <summary>
    /// Pure buoyancy math: force computation and overlapping-volume resolution. No Unity
    /// lifecycle dependency.
    /// </summary>
    public static class Buoyancy
    {
        /// <summary>
        /// Upward buoyancy force magnitude for a submerged depth. Zero at/above the surface,
        /// increases linearly with depth, and is clamped once the body is fully submerged.
        /// </summary>
        /// <param name="submergedDepth">Depth below the surface (m).</param>
        /// <param name="profile">Buoyancy profile.</param>
        /// <returns>Upward force magnitude (>= 0).</returns>
        public static float UpwardForce(float submergedDepth, BuoyancyProfile profile)
        {
            if (submergedDepth <= 0f)
            {
                return 0f;
            }
            return profile.strength * Mathf.Min(submergedDepth, profile.submersionDepthCap);
        }

        /// <summary>
        /// Select the volume that governs a world-space point when volumes overlap: among
        /// volumes whose footprint contains the point, the one with the highest water level.
        /// Ties keep the earliest-registered volume (strict greater-than).
        /// </summary>
        /// <param name="point">World-space point.</param>
        /// <param name="volumes">Candidate volumes, in registration order.</param>
        /// <param name="index">Index of the governing volume, or -1 if none contain the point.</param>
        /// <returns>True if a governing volume was found.</returns>
        public static bool SelectGoverningVolume(Vector3 point, IReadOnlyList<WaterVolume> volumes, out int index)
        {
            index = -1;
            float best = float.NegativeInfinity;
            if (volumes == null)
            {
                return false;
            }
            for (int i = 0; i < volumes.Count; i++)
            {
                if (volumes[i].InsideFootprint(point) && volumes[i].waterLevel > best)
                {
                    best = volumes[i].waterLevel;
                    index = i;
                }
            }
            return index >= 0;
        }
    }

    /// <summary>
    /// Hysteresis state machine for "is this point underwater". Tracks signed depth
    /// (waterLevel - pointY, positive below the surface) and only transitions when the point
    /// crosses past an enter/exit band, so jitter at the surface does not chatter events.
    /// </summary>
    public class UnderwaterStateTracker
    {
        /// <summary>A state transition emitted by <see cref="Update"/>.</summary>
        public enum Transition
        {
            None,
            Entered,
            Exited
        }

        private readonly float enterDepth;
        private readonly float exitDepth;

        /// <summary>Whether the tracked point is currently considered underwater.</summary>
        public bool IsUnderwater { get; private set; }

        /// <summary>
        /// Create a tracker.
        /// </summary>
        /// <param name="enterDepth">Signed depth at/above which the point becomes underwater (e.g. +0.05).</param>
        /// <param name="exitDepth">Signed depth at/below which the point surfaces (e.g. -0.05).</param>
        public UnderwaterStateTracker(float enterDepth = 0.05f, float exitDepth = -0.05f)
        {
            this.enterDepth = enterDepth;
            this.exitDepth = exitDepth;
        }

        /// <summary>
        /// Update with the current signed depth and return any state transition.
        /// </summary>
        /// <param name="signedDepth">waterLevel - pointY (positive when below the surface).</param>
        /// <returns>The transition that occurred, if any.</returns>
        public Transition Update(float signedDepth)
        {
            if (!IsUnderwater && signedDepth >= enterDepth)
            {
                IsUnderwater = true;
                return Transition.Entered;
            }
            if (IsUnderwater && signedDepth <= exitDepth)
            {
                IsUnderwater = false;
                return Transition.Exited;
            }
            return Transition.None;
        }
    }
}
