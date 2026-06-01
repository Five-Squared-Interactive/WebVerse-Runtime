// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Data class representing a raw scene anchor from the platform SDK.
    /// </summary>
    public class SceneAnchorData
    {
        public string Uuid { get; set; }
        public string SemanticLabel { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector2 PlaneBounds { get; set; }
    }

    /// <summary>
    /// Thin wrapper interface for the platform scene API (OVRSceneManager).
    /// Abstracts the Meta SDK so Quest3SurfaceDetector is testable.
    /// </summary>
    public interface ISceneProvider
    {
        /// <summary>
        /// Whether scene capture/scanning is currently active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Start scene capture.
        /// </summary>
        void StartCapture();

        /// <summary>
        /// Stop scene capture.
        /// </summary>
        void StopCapture();

        /// <summary>
        /// Get all currently detected scene anchors.
        /// </summary>
        List<SceneAnchorData> GetAnchors();

        /// <summary>
        /// Fired when the scene model is loaded or updated.
        /// </summary>
        event Action OnSceneModelUpdated;

        /// <summary>
        /// Fired when tracking is lost.
        /// </summary>
        event Action OnTrackingLost;

        /// <summary>
        /// Fired when tracking is re-acquired.
        /// </summary>
        event Action OnTrackingAcquired;
    }
}