// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Quest3;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    [TestFixture]
    public class Quest3SurfaceDetectorTests
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

        // --- StartScanning / StopScanning ---

        [Test]
        public void StartScanning_SetsIsScanningTrue()
        {
            _detector.StartScanning();
            Assert.IsTrue(_detector.IsScanning);
        }

        [Test]
        public void StopScanning_SetsIsScanningFalse()
        {
            _detector.StartScanning();
            _detector.StopScanning();
            Assert.IsFalse(_detector.IsScanning);
        }

        [Test]
        public void StopScanning_ClearsPlanes()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("a1", "FLOOR", 2f, 3f));
            _mockScene.FireSceneModelUpdated();

            _detector.StopScanning();

            Assert.AreEqual(0, _detector.GetPlanes(PlaneType.Any).Count);
        }

        // --- Plane population ---

        [Test]
        public void SceneModelUpdated_PopulatesPlanesFromAnchors()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("a1", "FLOOR", 2f, 3f));
            _mockScene.Anchors.Add(CreateAnchor("a2", "TABLE", 1f, 1f));
            _mockScene.FireSceneModelUpdated();

            var all = _detector.GetPlanes(PlaneType.Any);
            Assert.AreEqual(2, all.Count);
        }

        [Test]
        public void GetPlanes_Floor_FiltersCorrectly()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("a1", "FLOOR", 2f, 3f));
            _mockScene.Anchors.Add(CreateAnchor("a2", "TABLE", 1f, 1f));
            _mockScene.Anchors.Add(CreateAnchor("a3", "FLOOR", 4f, 2f));
            _mockScene.FireSceneModelUpdated();

            var floors = _detector.GetPlanes(PlaneType.Floor);
            Assert.AreEqual(2, floors.Count);
            Assert.IsTrue(floors.TrueForAll(p => p.Classification == PlaneType.Floor));
        }

        [Test]
        public void GetPlanes_Any_ReturnsAll()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("a1", "FLOOR", 2f, 3f));
            _mockScene.Anchors.Add(CreateAnchor("a2", "TABLE", 1f, 1f));
            _mockScene.Anchors.Add(CreateAnchor("a3", "WALL_FACE", 5f, 3f));
            _mockScene.FireSceneModelUpdated();

            var all = _detector.GetPlanes(PlaneType.Any);
            Assert.AreEqual(3, all.Count);
        }

        [Test]
        public void GetPlanes_SortedByAreaDescending()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("small", "FLOOR", 1f, 1f));  // area=1
            _mockScene.Anchors.Add(CreateAnchor("large", "FLOOR", 5f, 4f));  // area=20
            _mockScene.Anchors.Add(CreateAnchor("medium", "FLOOR", 3f, 2f)); // area=6
            _mockScene.FireSceneModelUpdated();

            var floors = _detector.GetPlanes(PlaneType.Floor);
            Assert.AreEqual(20f, floors[0].Area, 0.01f);
            Assert.AreEqual(6f, floors[1].Area, 0.01f);
            Assert.AreEqual(1f, floors[2].Area, 0.01f);
        }

        // --- Plane update and removal ---

        [Test]
        public void SceneModelUpdated_UpdatesExistingPlaneById()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("a1", "FLOOR", 2f, 3f, new Vector3(1, 0, 1)));
            _mockScene.FireSceneModelUpdated();

            // Update position
            _mockScene.Anchors[0] = CreateAnchor("a1", "FLOOR", 2f, 3f, new Vector3(5, 0, 5));
            _mockScene.FireSceneModelUpdated();

            var planes = _detector.GetPlanes(PlaneType.Any);
            Assert.AreEqual(1, planes.Count);
            Assert.AreEqual(new Vector3(5, 0, 5), planes[0].Position);
        }

        [Test]
        public void SceneModelUpdated_RemovesDisappearedAnchors()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("a1", "FLOOR", 2f, 3f));
            _mockScene.Anchors.Add(CreateAnchor("a2", "TABLE", 1f, 1f));
            _mockScene.FireSceneModelUpdated();

            _mockScene.Anchors.RemoveAt(1); // Remove a2
            _mockScene.FireSceneModelUpdated();

            Assert.AreEqual(1, _detector.GetPlanes(PlaneType.Any).Count);
            Assert.AreEqual("a1", _detector.GetPlanes(PlaneType.Any)[0].Id);
        }

        // --- OnPlanesUpdated notification ---

        [Test]
        public void SceneModelUpdated_CallsOnPlanesUpdated()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("a1", "FLOOR", 2f, 3f));
            _mockScene.FireSceneModelUpdated();

            Assert.AreEqual(1, _mockAnchors.OnPlanesUpdatedCallCount);
        }

        // --- Label mapping ---

        [Test]
        public void LabelMapping_WallFace_MapsToWall()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("w1", "WALL_FACE", 3f, 2f));
            _mockScene.FireSceneModelUpdated();

            var walls = _detector.GetPlanes(PlaneType.Wall);
            Assert.AreEqual(1, walls.Count);
        }

        [Test]
        public void LabelMapping_Unknown_MapsToAny()
        {
            _detector.StartScanning();
            _mockScene.Anchors.Add(CreateAnchor("u1", "CEILING", 3f, 2f));
            _mockScene.FireSceneModelUpdated();

            var any = _detector.GetPlanes(PlaneType.Any);
            Assert.AreEqual(1, any.Count);
            Assert.AreEqual(PlaneType.Any, any[0].Classification);
        }

        // --- Null safety ---

        [Test]
        public void NullSceneProvider_DoesNotThrow()
        {
            var detector = new Quest3SurfaceDetector(null);
            Assert.DoesNotThrow(() => detector.StartScanning());
            Assert.DoesNotThrow(() => detector.StopScanning());
            Assert.AreEqual(0, detector.GetPlanes(PlaneType.Any).Count);
        }

        // --- Helper ---

        private SceneAnchorData CreateAnchor(string uuid, string label, float w, float h, Vector3? pos = null)
        {
            return new SceneAnchorData
            {
                Uuid = uuid,
                SemanticLabel = label,
                Position = pos ?? Vector3.zero,
                Rotation = Quaternion.identity,
                PlaneBounds = new Vector2(w, h)
            };
        }
    }
}