// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using FiveSQD.WebVerse.Input.Quest3;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    /// <summary>
    /// Mock ISceneProvider for testing Quest3SurfaceDetector.
    /// </summary>
    public class MockSceneProvider : ISceneProvider
    {
        public bool IsActive { get; set; }
        public List<SceneAnchorData> Anchors { get; set; } = new List<SceneAnchorData>();
        public int StartCaptureCallCount { get; private set; }
        public int StopCaptureCallCount { get; private set; }

        public event Action OnSceneModelUpdated;
        public event Action OnTrackingLost;
        public event Action OnTrackingAcquired;

        public void StartCapture()
        {
            IsActive = true;
            StartCaptureCallCount++;
        }

        public void StopCapture()
        {
            IsActive = false;
            StopCaptureCallCount++;
        }

        public List<SceneAnchorData> GetAnchors() => Anchors;

        public void FireSceneModelUpdated() => OnSceneModelUpdated?.Invoke();
        public void FireTrackingLost() => OnTrackingLost?.Invoke();
        public void FireTrackingAcquired() => OnTrackingAcquired?.Invoke();
    }
}