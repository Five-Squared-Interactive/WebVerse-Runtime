// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.Input.Quest3;

namespace FiveSQD.WebVerse.Input.Quest3.Tests
{
    [TestFixture]
    public class ScanningFeedbackControllerTests
    {
        private MockSurfaceDetector _mockDetector;
        private ScanningFeedbackController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockDetector = new MockSurfaceDetector();
            _mockDetector.IsScanning = true;
            _controller = new ScanningFeedbackController(_mockDetector, 5f);
        }

        [Test]
        public void InitialState_IsHidden()
        {
            Assert.AreEqual(ScanningFeedbackState.Hidden, _controller.State);
        }

        [Test]
        public void OnScanningStarted_NoSurfaces_StateIsScanning()
        {
            _controller.OnScanningStarted();

            Assert.AreEqual(ScanningFeedbackState.Scanning, _controller.State);
            Assert.AreEqual("Scanning for surfaces...", _controller.Message);
        }

        [Test]
        public void FirstSurfaceDetected_TransitionsToHidden()
        {
            _controller.OnScanningStarted();

            _mockDetector.Planes.Add(new DetectedPlane
            {
                Id = "p1", Classification = PlaneType.Floor,
                Position = Vector3.zero, Rotation = Quaternion.identity,
                Bounds = new Vector2(2, 2), Area = 4
            });
            _controller.Update(1f);

            Assert.AreEqual(ScanningFeedbackState.Hidden, _controller.State);
            Assert.IsNull(_controller.Message);
        }

        [Test]
        public void NoSurfacesAfter5s_TransitionsToNoSurfacesWarning()
        {
            _controller.OnScanningStarted();

            _controller.Update(5f); // 5 seconds elapsed

            Assert.AreEqual(ScanningFeedbackState.NoSurfacesWarning, _controller.State);
            Assert.AreEqual("No surfaces detected. Move to a clear area.", _controller.Message);
        }

        [Test]
        public void NoSurfacesBefore5s_StaysInScanning()
        {
            _controller.OnScanningStarted();

            _controller.Update(3f); // 3 seconds elapsed

            Assert.AreEqual(ScanningFeedbackState.Scanning, _controller.State);
        }

        [Test]
        public void SurfaceDetectedDuringNoSurfacesWarning_TransitionsToHidden()
        {
            _controller.OnScanningStarted();
            _controller.Update(5f); // Enter warning state

            _mockDetector.Planes.Add(new DetectedPlane
            {
                Id = "p1", Classification = PlaneType.Floor,
                Position = Vector3.zero, Rotation = Quaternion.identity,
                Bounds = new Vector2(2, 2), Area = 4
            });
            _controller.Update(6f);

            Assert.AreEqual(ScanningFeedbackState.Hidden, _controller.State);
        }

        [Test]
        public void StopScanning_TransitionsToHidden()
        {
            _controller.OnScanningStarted();
            _mockDetector.IsScanning = false;

            _controller.Update(1f);

            Assert.AreEqual(ScanningFeedbackState.Hidden, _controller.State);
        }

        [Test]
        public void OnScanningStopped_TransitionsToHidden()
        {
            _controller.OnScanningStarted();
            _controller.OnScanningStopped();

            Assert.AreEqual(ScanningFeedbackState.Hidden, _controller.State);
            Assert.IsNull(_controller.Message);
        }

        // --- Relocating state (Story 2.3) ---

        [Test]
        public void ShowRelocating_TransitionsToRelocating()
        {
            _controller.ShowRelocating();

            Assert.AreEqual(ScanningFeedbackState.Relocating, _controller.State);
            Assert.AreEqual("Relocating surfaces...", _controller.Message);
        }

        [Test]
        public void HideRelocating_TransitionsToHidden()
        {
            _controller.ShowRelocating();
            _controller.HideRelocating();

            Assert.AreEqual(ScanningFeedbackState.Hidden, _controller.State);
            Assert.IsNull(_controller.Message);
        }

        [Test]
        public void HideRelocating_WhenNotRelocating_DoesNothing()
        {
            _controller.OnScanningStarted();
            _controller.HideRelocating();

            Assert.AreEqual(ScanningFeedbackState.Scanning, _controller.State);
        }

        [Test]
        public void Relocating_IgnoresUpdateTransitions()
        {
            _controller.ShowRelocating();

            // Even if scanning stops, relocating state is preserved
            _mockDetector.IsScanning = false;
            _controller.Update(10f);

            Assert.AreEqual(ScanningFeedbackState.Relocating, _controller.State);
        }
    }
}