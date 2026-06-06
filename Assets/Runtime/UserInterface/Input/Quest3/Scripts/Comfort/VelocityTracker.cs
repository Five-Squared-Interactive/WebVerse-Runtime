// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.VR.Comfort
{
    /// <summary>
    /// Tracks movement velocity from a target transform's position delta each frame.
    /// Platform-agnostic — works for thumbstick, teleport, hand tracking, or any locomotion source.
    /// Runs in LateUpdate to capture final position after all movement providers have applied.
    /// </summary>
    public class VelocityTracker : MonoBehaviour
    {
        private Transform _target;
        private Vector3 _lastPosition;
        private float _currentVelocity;
        private bool _initialized;

        /// <summary>
        /// Set the transform to track (typically the VR camera).
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            _initialized = false;
        }

        /// <summary>
        /// Get the current velocity magnitude (m/s).
        /// Returns 0 if no target is set or on the first frame after SetTarget.
        /// </summary>
        public float GetVelocity()
        {
            return _currentVelocity;
        }

        private void OnDisable()
        {
            _currentVelocity = 0f;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                _currentVelocity = 0f;
                return;
            }

            if (!_initialized)
            {
                _lastPosition = _target.position;
                _initialized = true;
                _currentVelocity = 0f;
                return;
            }

            if (Time.deltaTime <= 0f)
            {
                _currentVelocity = 0f;
                return;
            }

            Vector3 currentPos = _target.position;
            _currentVelocity = (currentPos - _lastPosition).magnitude / Time.deltaTime;
            _lastPosition = currentPos;
        }
    }
}
