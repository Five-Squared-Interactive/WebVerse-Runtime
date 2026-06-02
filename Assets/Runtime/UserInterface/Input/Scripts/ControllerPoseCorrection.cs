// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Corrects the tracked pose applied by TrackedPoseDriver on VR controller GameObjects.
    /// Attach to each controller (Left Controller, Right Controller) alongside TrackedPoseDriver.
    /// Applies a configurable rotation offset after the tracked pose is set each frame.
    /// This fixes upside-down or backwards controller orientations caused by OpenXR runtime
    /// pose convention differences (e.g., SteamVR grip pose convention vs expected convention).
    /// </summary>
    [DefaultExecutionOrder(32000)] // Run well after TrackedPoseDriver
    public class ControllerPoseCorrection : MonoBehaviour
    {
        /// <summary>
        /// Euler-angle rotation offset applied to the tracked pose each frame.
        /// Default (0, 180, 0) corrects for SteamVR's 180-degree Y-axis grip pose offset.
        /// </summary>
        [Tooltip("Euler rotation offset applied after TrackedPoseDriver updates. (0,180,0) fixes SteamVR backwards controllers.")]
        public Vector3 rotationOffsetEuler = new Vector3(0f, 180f, 0f);

        private Quaternion _correctionQuat;
        private TrackedPoseDriver _trackedPoseDriver;

        private void OnEnable()
        {
            _correctionQuat = Quaternion.Euler(rotationOffsetEuler);
            _trackedPoseDriver = GetComponent<TrackedPoseDriver>();
            Application.onBeforeRender += OnBeforeRender;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
        }

        private void OnValidate()
        {
            _correctionQuat = Quaternion.Euler(rotationOffsetEuler);
        }

        private void LateUpdate()
        {
            ApplyCorrection();
        }

        private void OnBeforeRender()
        {
            ApplyCorrection();
        }

        private void ApplyCorrection()
        {
            if (_trackedPoseDriver == null || !_trackedPoseDriver.enabled)
                return;

            transform.localRotation = transform.localRotation * _correctionQuat;
        }
    }
}
