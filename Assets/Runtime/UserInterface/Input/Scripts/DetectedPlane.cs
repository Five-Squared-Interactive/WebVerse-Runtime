// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Represents a real-world plane detected by the AR surface detection system.
    /// Plain C# class (not MonoBehaviour) for testability.
    /// </summary>
    public class DetectedPlane
    {
        /// <summary>
        /// Unique identifier for this detected plane.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Classification of the plane (Floor, Table, Wall, etc.).
        /// </summary>
        public PlaneType Classification { get; set; }

        /// <summary>
        /// World-space position of the plane center.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// World-space rotation of the plane.
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// Width (x) and height (z) extents of the plane in meters.
        /// </summary>
        public Vector2 Bounds { get; set; }

        /// <summary>
        /// Total area of the plane in square meters.
        /// </summary>
        public float Area { get; set; }

        /// <summary>
        /// Check whether a world-space point lies within this plane's bounds.
        /// Projects the point onto the plane's local coordinate system and checks
        /// whether the local x/z coordinates fall within +/- Bounds/2.
        /// </summary>
        /// <param name="point">World-space point to test.</param>
        /// <returns>True if the point is within the plane bounds.</returns>
        public bool ContainsPoint(Vector3 point)
        {
            Vector3 localPoint = Quaternion.Inverse(Rotation) * (point - Position);
            float halfWidth = Bounds.x / 2f;
            float halfHeight = Bounds.y / 2f;
            return Mathf.Abs(localPoint.x) <= halfWidth
                && Mathf.Abs(localPoint.z) <= halfHeight;
        }
    }
}