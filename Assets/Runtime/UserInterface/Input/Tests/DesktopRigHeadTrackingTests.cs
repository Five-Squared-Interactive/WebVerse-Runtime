// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Avatar;
using FiveSQD.WebVerse.Input.Desktop;
using FiveSQD.StraightFour.Entity;

namespace FiveSQD.WebVerse.Input.Tests
{
    /// <summary>
    /// Tests for DesktopRig → AvatarHeadTrackingDriver integration.
    /// Verifies that ApplyLook correctly feeds pitch to the head tracking driver
    /// and that the sign convention (negative xRotation = looking up) is preserved.
    /// </summary>
    [TestFixture]
    public class DesktopRigHeadTrackingTests
    {
        private GameObject _rigGO;
        private GameObject _avatarGO;
        private GameObject _cameraGO;

        [TearDown]
        public void TearDown()
        {
            if (_cameraGO != null) Object.DestroyImmediate(_cameraGO);
            if (_avatarGO != null) Object.DestroyImmediate(_avatarGO);
            if (_rigGO != null) Object.DestroyImmediate(_rigGO);
        }

        [Test]
        public void ApplyLook_UpwardMouse_SetsPositivePitchOnDriver()
        {
            LogAssert.ignoreFailingMessages = true;

            // Create camera
            _cameraGO = new GameObject("Camera");
            var cam = _cameraGO.AddComponent<Camera>();

            // Create avatar entity with animation manager
            _avatarGO = new GameObject("Avatar");
            var entity = _avatarGO.AddComponent<CharacterEntity>();
            var manager = _avatarGO.AddComponent<AvatarAnimationManager>();
            manager.Initialize();

            // Create DesktopRig and wire it
            _rigGO = new GameObject("DesktopRig");
            var rig = _rigGO.AddComponent<DesktopRig>();
            rig.avatarEntity = entity;
            rig.cameraTransform = _cameraGO.transform;
            rig.mouseLookEnabled = true;
            rig.mouseSensitivity = 1f;

            // Simulate upward mouse movement (positive Y = looking up)
            rig.ApplyLook(new Vector2(0f, 10f));

            var driver = _avatarGO.GetComponent<AvatarHeadTrackingDriver>();
            Assert.IsNotNull(driver, "HeadTrackingDriver should exist after Initialize");

            // Tick the driver so smoothed values advance toward target
            driver.ManualUpdate(10f);

            // Positive mouse Y → xRotation goes negative → -xRotation is positive → pitch positive = up
            Assert.Greater(driver.CurrentHeadPitch, 0f,
                "Upward mouse look should produce positive head pitch (looking up)");
        }

        [Test]
        public void ApplyLook_DownwardMouse_SetsNegativePitchOnDriver()
        {
            LogAssert.ignoreFailingMessages = true;

            _cameraGO = new GameObject("Camera");
            _cameraGO.AddComponent<Camera>();

            _avatarGO = new GameObject("Avatar");
            var entity = _avatarGO.AddComponent<CharacterEntity>();
            var manager = _avatarGO.AddComponent<AvatarAnimationManager>();
            manager.Initialize();

            _rigGO = new GameObject("DesktopRig");
            var rig = _rigGO.AddComponent<DesktopRig>();
            rig.avatarEntity = entity;
            rig.cameraTransform = _cameraGO.transform;
            rig.mouseLookEnabled = true;
            rig.mouseSensitivity = 1f;

            // Simulate downward mouse movement (negative Y = looking down)
            rig.ApplyLook(new Vector2(0f, -10f));

            var driver = _avatarGO.GetComponent<AvatarHeadTrackingDriver>();
            Assert.IsNotNull(driver, "HeadTrackingDriver should exist after Initialize");

            driver.ManualUpdate(10f);

            Assert.Less(driver.CurrentHeadPitch, 0f,
                "Downward mouse look should produce negative head pitch (looking down)");
        }

        [Test]
        public void ApplyLook_HeadYawAlwaysZero_BodyHandlesHorizontalRotation()
        {
            LogAssert.ignoreFailingMessages = true;

            _cameraGO = new GameObject("Camera");
            _cameraGO.AddComponent<Camera>();

            _avatarGO = new GameObject("Avatar");
            var entity = _avatarGO.AddComponent<CharacterEntity>();
            var manager = _avatarGO.AddComponent<AvatarAnimationManager>();
            manager.Initialize();

            _rigGO = new GameObject("DesktopRig");
            var rig = _rigGO.AddComponent<DesktopRig>();
            rig.avatarEntity = entity;
            rig.cameraTransform = _cameraGO.transform;
            rig.mouseLookEnabled = true;
            rig.mouseSensitivity = 1f;

            // Simulate horizontal mouse movement
            rig.ApplyLook(new Vector2(30f, 0f));

            var driver = _avatarGO.GetComponent<AvatarHeadTrackingDriver>();
            Assert.IsNotNull(driver, "HeadTrackingDriver should exist after Initialize");

            driver.ManualUpdate(10f);

            // Head yaw should be zero — body rotation handles horizontal look
            Assert.AreEqual(0f, driver.CurrentHeadYaw, 0.001f,
                "Head yaw should be 0 because avatar body handles horizontal rotation");
        }
    }
}
