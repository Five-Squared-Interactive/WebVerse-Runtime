// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using FiveSQD.WebVerse.Input;

namespace FiveSQD.WebVerse.Input.Tests
{
    [TestFixture]
    public class DetectedPlaneTests
    {
        private DetectedPlane CreateFloorPlane(Vector3 position, Quaternion rotation, Vector2 bounds)
        {
            return new DetectedPlane
            {
                Id = "test-plane",
                Classification = PlaneType.Floor,
                Position = position,
                Rotation = rotation,
                Bounds = bounds,
                Area = bounds.x * bounds.y
            };
        }

        [Test]
        public void ContainsPoint_PointInsideBounds_ReturnsTrue()
        {
            var plane = CreateFloorPlane(Vector3.zero, Quaternion.identity, new Vector2(2f, 2f));

            Assert.IsTrue(plane.ContainsPoint(new Vector3(0.5f, 0f, 0.5f)));
        }

        [Test]
        public void ContainsPoint_PointOutsideBounds_ReturnsFalse()
        {
            var plane = CreateFloorPlane(Vector3.zero, Quaternion.identity, new Vector2(2f, 2f));

            Assert.IsFalse(plane.ContainsPoint(new Vector3(5f, 0f, 5f)));
        }

        [Test]
        public void ContainsPoint_PointOnEdge_ReturnsTrue()
        {
            var plane = CreateFloorPlane(Vector3.zero, Quaternion.identity, new Vector2(2f, 2f));

            Assert.IsTrue(plane.ContainsPoint(new Vector3(1f, 0f, 1f)));
        }

        [Test]
        public void ContainsPoint_PointJustOutsideEdge_ReturnsFalse()
        {
            var plane = CreateFloorPlane(Vector3.zero, Quaternion.identity, new Vector2(2f, 2f));

            Assert.IsFalse(plane.ContainsPoint(new Vector3(1.01f, 0f, 0f)));
        }

        [Test]
        public void ContainsPoint_WithRotatedPlane_ReturnsTrue()
        {
            // Plane rotated 90 degrees around Y axis
            var plane = CreateFloorPlane(
                Vector3.zero,
                Quaternion.Euler(0f, 90f, 0f),
                new Vector2(2f, 4f));

            // After 90-degree Y rotation, local X maps to world -Z, local Z maps to world X
            // Bounds: x=2 (half=1), y=4 (half=2)
            // A point at world (1.5, 0, 0) should be within bounds (local z=1.5 < half=2)
            Assert.IsTrue(plane.ContainsPoint(new Vector3(1.5f, 0f, 0f)));
        }

        [Test]
        public void ContainsPoint_WithRotatedPlane_OutsideBounds_ReturnsFalse()
        {
            var plane = CreateFloorPlane(
                Vector3.zero,
                Quaternion.Euler(0f, 90f, 0f),
                new Vector2(2f, 4f));

            // After 90-degree Y rotation, local X maps to world -Z
            // A point at world (0, 0, 1.5) maps to local x ~= -1.5, which exceeds half=1
            Assert.IsFalse(plane.ContainsPoint(new Vector3(0f, 0f, 1.5f)));
        }

        [Test]
        public void ContainsPoint_WithOffset_PointInsideBounds_ReturnsTrue()
        {
            var plane = CreateFloorPlane(
                new Vector3(10f, 0f, 10f),
                Quaternion.identity,
                new Vector2(2f, 2f));

            Assert.IsTrue(plane.ContainsPoint(new Vector3(10.5f, 0f, 10.5f)));
        }

        [Test]
        public void ContainsPoint_WithOffset_PointOutsideBounds_ReturnsFalse()
        {
            var plane = CreateFloorPlane(
                new Vector3(10f, 0f, 10f),
                Quaternion.identity,
                new Vector2(2f, 2f));

            Assert.IsFalse(plane.ContainsPoint(new Vector3(0f, 0f, 0f)));
        }

        [Test]
        public void ContainsPoint_PointAbovePlane_ReturnsTrue()
        {
            // ContainsPoint only checks x/z bounds, not y distance
            var plane = CreateFloorPlane(Vector3.zero, Quaternion.identity, new Vector2(2f, 2f));

            Assert.IsTrue(plane.ContainsPoint(new Vector3(0f, 5f, 0f)));
        }

        [Test]
        public void ContainsPoint_CenterPoint_ReturnsTrue()
        {
            var plane = CreateFloorPlane(
                new Vector3(5f, 1f, 5f),
                Quaternion.identity,
                new Vector2(3f, 3f));

            Assert.IsTrue(plane.ContainsPoint(new Vector3(5f, 1f, 5f)));
        }

        [Test]
        public void Properties_CanBeSetAndRetrieved()
        {
            var plane = new DetectedPlane
            {
                Id = "plane-42",
                Classification = PlaneType.Table,
                Position = new Vector3(1f, 2f, 3f),
                Rotation = Quaternion.Euler(10f, 20f, 30f),
                Bounds = new Vector2(1.5f, 2.5f),
                Area = 3.75f
            };

            Assert.AreEqual("plane-42", plane.Id);
            Assert.AreEqual(PlaneType.Table, plane.Classification);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), plane.Position);
            Assert.AreEqual(new Vector2(1.5f, 2.5f), plane.Bounds);
            Assert.AreEqual(3.75f, plane.Area, 0.001f);
        }
    }
}