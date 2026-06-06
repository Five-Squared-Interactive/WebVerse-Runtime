// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Interface for AR surface detection.
    /// Platform implementations detect real-world planes (floors, tables, walls).
    /// InputManager holds a nullable reference; null on non-AR platforms.
    /// </summary>
    public interface ISurfaceDetector
    {
        /// <summary>
        /// Whether the detector is currently scanning for surfaces.
        /// </summary>
        bool IsScanning { get; }

        /// <summary>
        /// Start scanning for real-world surfaces.
        /// </summary>
        void StartScanning();

        /// <summary>
        /// Stop scanning for real-world surfaces.
        /// </summary>
        void StopScanning();

        /// <summary>
        /// Get all detected planes matching the specified type.
        /// </summary>
        /// <param name="planeType">Type of plane to filter by. Use Any for all planes.</param>
        /// <returns>List of detected planes matching the filter.</returns>
        List<DetectedPlane> GetPlanes(PlaneType planeType);
    }
}