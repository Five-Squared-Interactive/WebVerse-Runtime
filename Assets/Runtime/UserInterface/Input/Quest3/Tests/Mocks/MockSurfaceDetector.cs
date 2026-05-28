// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using FiveSQD.WebVerse.Input;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    /// <summary>
    /// Mock ISurfaceDetector for testing.
    /// </summary>
    public class MockSurfaceDetector : ISurfaceDetector
    {
        public bool IsScanning { get; set; }
        public List<DetectedPlane> Planes { get; set; } = new List<DetectedPlane>();
        public int StartScanningCallCount { get; private set; }
        public int StopScanningCallCount { get; private set; }

        public void StartScanning()
        {
            IsScanning = true;
            StartScanningCallCount++;
        }

        public void StopScanning()
        {
            IsScanning = false;
            StopScanningCallCount++;
        }

        public List<DetectedPlane> GetPlanes(PlaneType planeType)
        {
            if (planeType == PlaneType.Any) return Planes.ToList();
            return Planes.Where(p => p.Classification == planeType).ToList();
        }
    }
}