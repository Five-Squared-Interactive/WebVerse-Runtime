// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Quest3;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    [TestFixture]
    public class Quest3AnchorPlacerTests
    {
        private MockSurfaceDetector _mockDetector;
        private Quest3AnchorPlacer _placer;
        private Dictionary<string, Vector3> _entityPositions;

        [SetUp]
        public void SetUp()
        {
            _mockDetector = new MockSurfaceDetector();
            _entityPositions = new Dictionary<string, Vector3>();
            _placer = new Quest3AnchorPlacer(_mockDetector);
            _placer.GetEntityPosition = (id) =>
                _entityPositions.TryGetValue(id, out var pos) ? pos : (Vector3?)null;
            _placer.SetEntityPosition = (id, pos) => _entityPositions[id] = pos;
        }

        // --- Story 3.1: RegisterAnchor ---

        [Test]
        public void RegisterAnchor_WithFloorPlane_SetsEntityYToPlaneHeight()
        {
            _entityPositions["e1"] = new Vector3(1, 5, 1);
            AddFloorPlane("floor1", 0f, 10f);

            _placer.RegisterAnchor("e1", AnchorType.Floor, new Vector3(1, 5, 1));

            Assert.AreEqual(0f, _entityPositions["e1"].y, 0.01f);
        }

        [Test]
        public void RegisterAnchor_NoMatchingPlane_EntityRemainsAtOriginalPosition()
        {
            _entityPositions["e1"] = new Vector3(1, 5, 1);
            // No planes

            _placer.RegisterAnchor("e1", AnchorType.Floor, new Vector3(1, 5, 1));

            Assert.AreEqual(5f, _entityPositions["e1"].y, 0.01f);
        }

        [Test]
        public void IsEntityAnchored_RegisteredAndPlaced_ReturnsTrue()
        {
            _entityPositions["e1"] = Vector3.zero;
            AddFloorPlane("floor1", 0f, 10f);

            _placer.RegisterAnchor("e1", AnchorType.Floor);

            Assert.IsTrue(_placer.IsEntityAnchored("e1"));
        }

        [Test]
        public void IsEntityAnchored_RegisteredNotPlaced_ReturnsFalse()
        {
            _entityPositions["e1"] = Vector3.zero;
            // No planes

            _placer.RegisterAnchor("e1", AnchorType.Floor);

            Assert.IsFalse(_placer.IsEntityAnchored("e1"));
        }

        [Test]
        public void IsEntityAnchored_NotRegistered_ReturnsFalse()
        {
            Assert.IsFalse(_placer.IsEntityAnchored("nonexistent"));
        }

        [Test]
        public void GetEntityAnchorType_Registered_ReturnsCorrectType()
        {
            _placer.RegisterAnchor("e1", AnchorType.Floor);

            Assert.AreEqual(AnchorType.Floor, _placer.GetEntityAnchorType("e1"));
        }

        [Test]
        public void GetEntityAnchorType_NotRegistered_ReturnsNone()
        {
            Assert.AreEqual(AnchorType.None, _placer.GetEntityAnchorType("nonexistent"));
        }

        [Test]
        public void UnregisterAnchor_RestoresOriginalPosition()
        {
            _entityPositions["e1"] = new Vector3(1, 5, 1);
            AddFloorPlane("floor1", 0f, 10f);
            _placer.RegisterAnchor("e1", AnchorType.Floor, new Vector3(1, 5, 1));

            _placer.UnregisterAnchor("e1");

            Assert.AreEqual(5f, _entityPositions["e1"].y, 0.01f);
        }

        [Test]
        public void OnPlanesUpdated_RepositionsAnchoredEntities()
        {
            _entityPositions["e1"] = new Vector3(0, 5, 0);
            AddFloorPlane("floor1", 0f, 10f);
            _placer.RegisterAnchor("e1", AnchorType.Floor);
            Assert.AreEqual(0f, _entityPositions["e1"].y, 0.01f);

            // Update plane to new height
            _mockDetector.Planes[0].Position = new Vector3(0, 0.5f, 0);
            _placer.OnPlanesUpdated(_mockDetector.Planes);

            Assert.AreEqual(0.5f, _entityPositions["e1"].y, 0.01f);
        }

        // --- Story 3.2: SelectBestPlane ---

        [Test]
        public void SelectBestPlane_MultipleFloors_NoHint_ReturnsLargestByArea()
        {
            var planes = new List<DetectedPlane>
            {
                CreatePlane("small", PlaneType.Floor, Vector3.zero, 2f),
                CreatePlane("large", PlaneType.Floor, Vector3.zero, 20f),
                CreatePlane("medium", PlaneType.Floor, Vector3.zero, 6f)
            };

            var result = _placer.SelectBestPlane(AnchorType.Floor, null, planes);

            Assert.AreEqual("large", result.Id);
        }

        [Test]
        public void SelectBestPlane_MultipleFloors_WithHint_ReturnsNearestToHint()
        {
            var planes = new List<DetectedPlane>
            {
                CreatePlane("far", PlaneType.Floor, new Vector3(100, 0, 100), 20f),
                CreatePlane("near", PlaneType.Floor, new Vector3(1, 0, 1), 2f)
            };

            var result = _placer.SelectBestPlane(AnchorType.Floor, new Vector3(0, 5, 0), planes);

            Assert.AreEqual("near", result.Id);
        }

        [Test]
        public void SelectBestPlane_WithHint_PreservesXZSetsYToPlaneHeight()
        {
            _entityPositions["e1"] = new Vector3(3, 10, 7);
            AddFloorPlane("floor1", 1.5f, 10f);
            _placer.RegisterAnchor("e1", AnchorType.Floor, new Vector3(3, 10, 7));

            Assert.AreEqual(3f, _entityPositions["e1"].x, 0.01f);
            Assert.AreEqual(1.5f, _entityPositions["e1"].y, 0.01f);
            Assert.AreEqual(7f, _entityPositions["e1"].z, 0.01f);
        }

        [Test]
        public void SelectBestPlane_TableType_MatchesTablePlanes()
        {
            var planes = new List<DetectedPlane>
            {
                CreatePlane("floor1", PlaneType.Floor, Vector3.zero, 20f),
                CreatePlane("table1", PlaneType.Table, Vector3.zero, 2f)
            };

            var result = _placer.SelectBestPlane(AnchorType.Table, null, planes);

            Assert.AreEqual("table1", result.Id);
        }

        [Test]
        public void SelectBestPlane_FloorType_OnlyTablePlanes_ReturnsNull()
        {
            var planes = new List<DetectedPlane>
            {
                CreatePlane("table1", PlaneType.Table, Vector3.zero, 2f)
            };

            var result = _placer.SelectBestPlane(AnchorType.Floor, null, planes);

            Assert.IsNull(result);
        }

        [Test]
        public void SelectBestPlane_NoPlanes_ReturnsNull()
        {
            var result = _placer.SelectBestPlane(AnchorType.Floor, null, new List<DetectedPlane>());

            Assert.IsNull(result);
        }

        [Test]
        public void SelectBestPlane_EqualDistance_LargestAreaWins()
        {
            var planes = new List<DetectedPlane>
            {
                CreatePlane("small", PlaneType.Floor, new Vector3(1, 0, 0), 2f),
                CreatePlane("big", PlaneType.Floor, new Vector3(-1, 0, 0), 20f)
            };

            // Both are distance 1 from origin
            var result = _placer.SelectBestPlane(AnchorType.Floor, Vector3.zero, planes);

            Assert.AreEqual("big", result.Id);
        }

        // --- Story 3.3: Floating fallback ---

        [Test]
        public void RegisterAnchor_NoPlanes_EntityFloatsAtOriginalPosition()
        {
            _entityPositions["e1"] = new Vector3(1, 5, 1);
            _placer.RegisterAnchor("e1", AnchorType.Floor, new Vector3(1, 5, 1));

            Assert.AreEqual(5f, _entityPositions["e1"].y, 0.01f);
            Assert.IsFalse(_placer.IsEntityAnchored("e1"));
        }

        [Test]
        public void OnPlanesUpdated_FloatingEntity_PlaneAppears_AutoAnchors()
        {
            _entityPositions["e1"] = new Vector3(0, 5, 0);
            _placer.RegisterAnchor("e1", AnchorType.Floor);

            Assert.IsFalse(_placer.IsEntityAnchored("e1"));

            // Plane appears
            _mockDetector.Planes.Add(CreatePlane("floor1", PlaneType.Floor, Vector3.zero, 10f));
            _placer.OnPlanesUpdated(_mockDetector.Planes);

            Assert.IsTrue(_placer.IsEntityAnchored("e1"));
            Assert.AreEqual(0f, _entityPositions["e1"].y, 0.01f);
        }

        [Test]
        public void OnModeChanged_VR_RestoresOriginalPositions()
        {
            _entityPositions["e1"] = new Vector3(1, 5, 1);
            AddFloorPlane("floor1", 0f, 10f);
            _placer.RegisterAnchor("e1", AnchorType.Floor, new Vector3(1, 5, 1));
            Assert.AreEqual(0f, _entityPositions["e1"].y, 0.01f);

            _placer.OnModeChanged(XRDisplayMode.VR);

            Assert.AreEqual(5f, _entityPositions["e1"].y, 0.01f);
        }

        [Test]
        public void OnModeChanged_VR_PreservesRegistrations()
        {
            _entityPositions["e1"] = Vector3.zero;
            _placer.RegisterAnchor("e1", AnchorType.Floor);

            _placer.OnModeChanged(XRDisplayMode.VR);

            Assert.AreEqual(AnchorType.Floor, _placer.GetEntityAnchorType("e1"));
        }

        [Test]
        public void OnModeChanged_AR_ReAnchorsWithAvailablePlanes()
        {
            _entityPositions["e1"] = new Vector3(0, 5, 0);
            AddFloorPlane("floor1", 0f, 10f);
            _placer.RegisterAnchor("e1", AnchorType.Floor);
            _placer.OnModeChanged(XRDisplayMode.VR);
            Assert.AreEqual(5f, _entityPositions["e1"].y, 0.01f);

            _placer.OnModeChanged(XRDisplayMode.AR);

            Assert.AreEqual(0f, _entityPositions["e1"].y, 0.01f);
            Assert.IsTrue(_placer.IsEntityAnchored("e1"));
        }

        [Test]
        public void OnModeChanged_AR_NoPlanes_RemainsFloating()
        {
            _entityPositions["e1"] = new Vector3(0, 5, 0);
            _placer.RegisterAnchor("e1", AnchorType.Floor);
            _placer.OnModeChanged(XRDisplayMode.VR);

            _mockDetector.Planes.Clear();
            _placer.OnModeChanged(XRDisplayMode.AR);

            Assert.IsFalse(_placer.IsEntityAnchored("e1"));
        }

        [Test]
        public void OnModeChanged_VR_ThenAR_FullRoundTrip()
        {
            _entityPositions["e1"] = new Vector3(0, 5, 0);
            AddFloorPlane("floor1", 1f, 10f);
            _placer.RegisterAnchor("e1", AnchorType.Floor);
            Assert.AreEqual(1f, _entityPositions["e1"].y, 0.01f);

            _placer.OnModeChanged(XRDisplayMode.VR);
            Assert.AreEqual(5f, _entityPositions["e1"].y, 0.01f);

            _placer.OnModeChanged(XRDisplayMode.AR);
            Assert.AreEqual(1f, _entityPositions["e1"].y, 0.01f);
        }

        // --- Integration scenario ---

        [Test]
        public void Scenario_InitialScan_NoPlanes_ThenPlanesDetected_EntitiesAnchor()
        {
            _entityPositions["e1"] = new Vector3(0, 3, 0);
            _entityPositions["e2"] = new Vector3(5, 7, 5);
            _placer.RegisterAnchor("e1", AnchorType.Floor);
            _placer.RegisterAnchor("e2", AnchorType.Floor);

            Assert.IsFalse(_placer.IsEntityAnchored("e1"));
            Assert.IsFalse(_placer.IsEntityAnchored("e2"));

            // Planes detected
            _mockDetector.Planes.Add(CreatePlane("floor1", PlaneType.Floor, Vector3.zero, 10f));
            _placer.OnPlanesUpdated(_mockDetector.Planes);

            Assert.IsTrue(_placer.IsEntityAnchored("e1"));
            Assert.IsTrue(_placer.IsEntityAnchored("e2"));
        }

        // --- Helpers ---

        private void AddFloorPlane(string id, float height, float area)
        {
            float side = Mathf.Sqrt(area);
            _mockDetector.Planes.Add(new DetectedPlane
            {
                Id = id,
                Classification = PlaneType.Floor,
                Position = new Vector3(0, height, 0),
                Rotation = Quaternion.identity,
                Bounds = new Vector2(side, side),
                Area = area
            });
        }

        private DetectedPlane CreatePlane(string id, PlaneType type, Vector3 pos, float area)
        {
            float side = Mathf.Sqrt(area);
            return new DetectedPlane
            {
                Id = id,
                Classification = type,
                Position = pos,
                Rotation = Quaternion.identity,
                Bounds = new Vector2(side, side),
                Area = area
            };
        }
    }
}