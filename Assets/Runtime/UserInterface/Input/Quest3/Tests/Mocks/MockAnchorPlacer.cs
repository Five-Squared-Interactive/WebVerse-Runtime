// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Input;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    /// <summary>
    /// Mock IAnchorPlacer for testing.
    /// </summary>
    public class MockAnchorPlacer : IAnchorPlacer
    {
        public List<List<DetectedPlane>> PlanesUpdatedHistory { get; } = new List<List<DetectedPlane>>();
        public Dictionary<string, AnchorType> Anchors { get; } = new Dictionary<string, AnchorType>();
        public int OnPlanesUpdatedCallCount { get; private set; }
        public XRDisplayMode LastModeChange { get; private set; }

        public bool RegisterAnchor(string entityId, Vector3? positionHint = null)
        {
            Anchors[entityId] = AnchorType.Floor;
            return true;
        }

        public bool UnregisterAnchor(string entityId)
        {
            return Anchors.Remove(entityId);
        }

        public bool IsEntityAnchored(string entityId) => Anchors.ContainsKey(entityId);

        public AnchorType GetEntityAnchorType(string entityId)
        {
            return Anchors.TryGetValue(entityId, out var type) ? type : AnchorType.None;
        }

        public void OnPlanesUpdated(List<DetectedPlane> planes)
        {
            OnPlanesUpdatedCallCount++;
            PlanesUpdatedHistory.Add(new List<DetectedPlane>(planes));
        }

        public void OnModeChanged(XRDisplayMode mode)
        {
            LastModeChange = mode;
        }
    }
}