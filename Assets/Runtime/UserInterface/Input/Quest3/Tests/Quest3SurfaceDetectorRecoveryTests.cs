// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Quest3;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    [TestFixture]
    public class Quest3SurfaceDetectorRecoveryTests
    {
        private MockSceneProvider _mockScene;
        private MockAnchorPlacer _mockAnchors;
        private Quest3SurfaceDetector _detector;

        [SetUp]
        public void SetUp()
        {
            _mockScene = new MockSceneProvider();
            _mockAnchors = new MockAnchorPlacer();
            _detector = new Quest3SurfaceDetector(_mockScene);
            _detector.AnchorPlacer = _mockAnchors;
        }

        // --- Tracking loss ---

        [Test]
        public void TrackingLost_TransitionsToLostState()
        {
            _detector.StartScanning();
            _mockScene.FireTrackingLost();

            Assert.AreEqual(TrackingState.Lost, _detector.CurrentTrackingState);
        }

        [Test]
        public void TrackingLost_CallsHandleARError()
        {
            ARErrorType? receivedError = null;
            _detector.OnARError = (e) => receivedError = e;
            _detector.StartScanning();

            _mockScene.FireTrackingLost();

            Assert.AreEqual(ARErrorType.SurfaceDetectionFailed, receivedError);
        }

        [Test]
        public void TrackingLost_ReturnsCachedPlanes()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(new SceneAnchorData
            {
                Uuid = "a1", SemanticLabel = "FLOOR",
                Position = Vector3.zero, Rotation = Quaternion.identity,
                PlaneBounds = new Vector2(3, 4)
            });
            _mockScene.FireSceneModelUpdated();
            int preCount = _detector.GetPlanes(PlaneType.Any).Count;

            _mockScene.FireTrackingLost();

            Assert.AreEqual(preCount, _detector.GetPlanes(PlaneType.Any).Count);
            Assert.AreEqual("a1", _detector.GetPlanes(PlaneType.Any)[0].Id);
        }

        [Test]
        public void TrackingLost_IgnoresSceneModelUpdates()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(new SceneAnchorData
            {
                Uuid = "a1", SemanticLabel = "FLOOR",
                Position = Vector3.zero, Rotation = Quaternion.identity,
                PlaneBounds = new Vector2(3, 4)
            });
            _mockScene.FireSceneModelUpdated();
            int preUpdateCount = _mockAnchors.OnPlanesUpdatedCallCount;

            _mockScene.FireTrackingLost();

            // New scene updates should be ignored
            _mockScene.Anchors.Add(new SceneAnchorData
            {
                Uuid = "a2", SemanticLabel = "TABLE",
                Position = Vector3.zero, Rotation = Quaternion.identity,
                PlaneBounds = new Vector2(1, 1)
            });
            _mockScene.FireSceneModelUpdated();

            Assert.AreEqual(1, _detector.GetPlanes(PlaneType.Any).Count);
            Assert.AreEqual(preUpdateCount, _mockAnchors.OnPlanesUpdatedCallCount);
        }

        [Test]
        public void TrackingLost_IsScanningRemainsTrue()
        {
            _detector.StartScanning();
            _mockScene.FireTrackingLost();

            Assert.IsTrue(_detector.IsScanning);
        }

        // --- Tracking recovery ---

        [Test]
        public void TrackingRecovered_TransitionsToActiveState()
        {
            _detector.StartScanning();
            _mockScene.FireTrackingLost();
            _mockScene.FireTrackingAcquired();

            Assert.AreEqual(TrackingState.Active, _detector.CurrentTrackingState);
        }

        [Test]
        public void TrackingRecovered_UpdatesPlanesFromProvider()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(new SceneAnchorData
            {
                Uuid = "a1", SemanticLabel = "FLOOR",
                Position = Vector3.zero, Rotation = Quaternion.identity,
                PlaneBounds = new Vector2(3, 4)
            });
            _mockScene.FireSceneModelUpdated();

            _mockScene.FireTrackingLost();

            // Update anchor position during tracking loss
            _mockScene.Anchors[0] = new SceneAnchorData
            {
                Uuid = "a1", SemanticLabel = "FLOOR",
                Position = new Vector3(1, 0, 1), Rotation = Quaternion.identity,
                PlaneBounds = new Vector2(3, 4)
            };

            _mockScene.FireTrackingAcquired();

            var planes = _detector.GetPlanes(PlaneType.Any);
            Assert.AreEqual(new Vector3(1, 0, 1), planes[0].Position);
        }

        [Test]
        public void TrackingRecovered_CallsOnPlanesUpdated()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(new SceneAnchorData
            {
                Uuid = "a1", SemanticLabel = "FLOOR",
                Position = Vector3.zero, Rotation = Quaternion.identity,
                PlaneBounds = new Vector2(3, 4)
            });
            _mockScene.FireSceneModelUpdated();
            int preCount = _mockAnchors.OnPlanesUpdatedCallCount;

            _mockScene.FireTrackingLost();
            _mockScene.FireTrackingAcquired();

            Assert.AreEqual(preCount + 1, _mockAnchors.OnPlanesUpdatedCallCount);
        }

        [Test]
        public void TrackingRecovered_NotifiesTrackingStateChange()
        {
            TrackingState? lastState = null;
            _detector.OnTrackingStateChanged = (s) => lastState = s;

            _detector.StartScanning();
            _mockScene.FireTrackingLost();
            Assert.AreEqual(TrackingState.Lost, lastState);

            _mockScene.FireTrackingAcquired();
            Assert.AreEqual(TrackingState.Active, lastState);
        }

        // --- Double-loss protection ---

        [Test]
        public void TrackingLost_CalledTwice_OnlyFirstFires()
        {
            int errorCount = 0;
            _detector.OnARError = (_) => errorCount++;

            _detector.StartScanning();
            _mockScene.FireTrackingLost();
            _mockScene.FireTrackingLost();

            Assert.AreEqual(1, errorCount);
        }

        // --- Full lifecycle integration ---

        [Test]
        public void FullLifecycle_DetectLoseRecoverDetect()
        {
            _detector.StartScanning();

            // Detect surfaces
            _mockScene.Anchors.Add(new SceneAnchorData
            {
                Uuid = "a1", SemanticLabel = "FLOOR",
                Position = Vector3.zero, Rotation = Quaternion.identity,
                PlaneBounds = new Vector2(3, 4)
            });
            _mockScene.FireSceneModelUpdated();
            Assert.AreEqual(1, _detector.GetPlanes(PlaneType.Any).Count);

            // Lose tracking
            _mockScene.FireTrackingLost();
            Assert.AreEqual(TrackingState.Lost, _detector.CurrentTrackingState);
            Assert.AreEqual(1, _detector.GetPlanes(PlaneType.Any).Count); // Cached

            // Recover
            _mockScene.FireTrackingAcquired();
            Assert.AreEqual(TrackingState.Active, _detector.CurrentTrackingState);

            // Detect new surface
            _mockScene.Anchors.Add(new SceneAnchorData
            {
                Uuid = "a2", SemanticLabel = "TABLE",
                Position = new Vector3(2, 0, 2), Rotation = Quaternion.identity,
                PlaneBounds = new Vector2(1, 1)
            });
            _mockScene.FireSceneModelUpdated();
            Assert.AreEqual(2, _detector.GetPlanes(PlaneType.Any).Count);
        }
    }
}