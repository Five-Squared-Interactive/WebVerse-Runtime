// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Bridges VR thumbstick input to the LocomotionDriver.
    /// Receives thumbstick values each frame and forwards them
    /// to AvatarLocomotionDriver.SetMovementInput().
    /// </summary>
    public class VRLocomotionBridge : MonoBehaviour
    {
        private AvatarLocomotionDriver _locomotionDriver;
        private bool _isInitialized;

        /// <summary>
        /// Initializes the bridge with a reference to the locomotion driver.
        /// </summary>
        /// <param name="locomotionDriver">The locomotion driver to forward input to.</param>
        public void Initialize(AvatarLocomotionDriver locomotionDriver)
        {
            if (locomotionDriver == null) return;
            if (_isInitialized) return;

            _locomotionDriver = locomotionDriver;
            _isInitialized = true;
        }

        /// <summary>
        /// Forwards thumbstick input to the locomotion driver.
        /// </summary>
        /// <param name="input">Thumbstick input (x = strafe, y = forward/back).</param>
        public void SetThumbstickInput(Vector2 input)
        {
            if (!_isInitialized) return;
            _locomotionDriver.SetMovementInput(input);
        }

        /// <summary>
        /// Cleans up references. After this call, SetThumbstickInput is a no-op.
        /// </summary>
        public void Cleanup()
        {
            _locomotionDriver = null;
            _isInitialized = false;
        }
    }
}
