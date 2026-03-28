// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Handedness enum for left/right hand identification.
    /// Local enum to avoid dependency on UnityEngine.XR.Hands assembly.
    /// </summary>
    public enum HandednessQ3
    {
        Left = 0,
        Right = 1
    }

    /// <summary>
    /// Handles hand tracking input for Quest 3.
    /// Translates hand gestures to InputManager calls.
    /// </summary>
    public class Quest3HandInput : BasePlatformInput
    {
        #region References

        /// <summary>
        /// The hand tracking wrapper.
        /// </summary>
        [Tooltip("The hand tracking wrapper.")]
        public Quest3HandTracking handTracking;

        /// <summary>
        /// Maximum raycast distance for hand pointing.
        /// </summary>
        [Tooltip("Maximum raycast distance for hand pointing.")]
        public float maxRaycastDistance = 100f;

        /// <summary>
        /// Layer mask for hand raycasts.
        /// </summary>
        [Tooltip("Layer mask for hand raycasts.")]
        public LayerMask raycastLayerMask = -1; // All layers by default

        /// <summary>
        /// Reference to the input mode manager.
        /// </summary>
        [Tooltip("Reference to the input mode manager.")]
        public InputModeManager inputModeManager;

        #endregion

        #region State

        private bool leftPinching = false;
        private bool rightPinching = false;
        private bool leftPointing = false;
        private bool rightPointing = false;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (handTracking != null)
            {
                handTracking.OnPinchStateChanged += OnPinchStateChanged;
                handTracking.OnHandTrackingStateChanged += OnHandTrackingStateChanged;
            }
        }

        private void OnDisable()
        {
            if (handTracking != null)
            {
                handTracking.OnPinchStateChanged -= OnPinchStateChanged;
                handTracking.OnHandTrackingStateChanged -= OnHandTrackingStateChanged;
            }
        }

        private void Update()
        {
            if (handTracking == null || !handTracking.IsHandTrackingAvailable)
            {
                return;
            }

            // Update pointing state
            UpdatePointingState();
        }

        #endregion

        #region BasePlatformInput Implementation

        /// <summary>
        /// Gets raycast hit for hand-based pointing.
        /// </summary>
        /// <param name="direction">Direction parameter (ignored for hand tracking, uses hand ray).</param>
        /// <param name="pointerIndex">0 = left hand, 1 = right hand</param>
        /// <returns>Tuple of RaycastHit and direction, or null if no hit.</returns>
        public override Tuple<RaycastHit, Vector3> GetPointerRaycast(Vector3 direction, int pointerIndex = 0)
        {
            if (handTracking == null)
            {
                return null;
            }

            bool isTracked = pointerIndex == 0 ? handTracking.IsLeftHandTracked : handTracking.IsRightHandTracked;
            bool isPointing = pointerIndex == 0 ? leftPointing : rightPointing;

            if (!isTracked || !isPointing)
            {
                return null;
            }

            if (handTracking.TryGetPointingRay(pointerIndex, out Vector3 origin, out Vector3 handDirection))
            {
                Ray ray = new Ray(origin, handDirection);
                if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, raycastLayerMask))
                {
                    return new Tuple<RaycastHit, Vector3>(hit, handDirection);
                }
            }

            return null;
        }

        #endregion

        #region Event Handlers

        private void OnPinchStateChanged(int handIndex, bool isPinching)
        {
            if (WebVerseRuntime.Instance?.inputManager == null)
            {
                return;
            }

            // Register hand input with mode manager
            if (isPinching && inputModeManager != null)
            {
                inputModeManager.RegisterHandInput();
            }

            var inputManager = WebVerseRuntime.Instance.inputManager;

            if (handIndex == 0) // Left hand
            {
                leftPinching = isPinching;

                if (isPinching)
                {
                    inputManager.LeftPinch();
                    inputManager.leftPinchValue = true;

                    // If right hand not pinching, also trigger general pinch
                    if (!rightPinching)
                    {
                        inputManager.Pinch();
                    }

                    // Also trigger left mouse button for compatibility
                    inputManager.Left();
                }
                else
                {
                    inputManager.EndLeftPinch();
                    inputManager.leftPinchValue = false;

                    if (!rightPinching)
                    {
                        inputManager.EndPinch();
                    }

                    inputManager.EndLeft();
                }
            }
            else // Right hand
            {
                rightPinching = isPinching;

                if (isPinching)
                {
                    inputManager.RightPinch();
                    inputManager.rightPinchValue = true;

                    if (!leftPinching)
                    {
                        inputManager.Pinch();
                    }

                    inputManager.Right();
                }
                else
                {
                    inputManager.EndRightPinch();
                    inputManager.rightPinchValue = false;

                    if (!leftPinching)
                    {
                        inputManager.EndPinch();
                    }

                    inputManager.EndRight();
                }
            }

            string handName = handIndex == 0 ? "Left" : "Right";
            Logging.Log($"[Quest3HandInput] {handName} pinch: {isPinching}");
        }

        private void OnHandTrackingStateChanged(int handIndex, bool isTracked)
        {
            if (WebVerseRuntime.Instance?.inputManager == null)
            {
                return;
            }

            // When hand tracking is lost, end any active gestures
            if (!isTracked)
            {
                if (handIndex == 0) // Left hand
                {
                    if (leftPinching)
                    {
                        OnPinchStateChanged(0, false);
                    }
                    leftPointing = false;
                }
                else // Right hand
                {
                    if (rightPinching)
                    {
                        OnPinchStateChanged(1, false);
                    }
                    rightPointing = false;
                }
            }

            string handName = handIndex == 0 ? "Left" : "Right";
            Logging.Log($"[Quest3HandInput] {handName} hand tracking: {isTracked}");
        }

        #endregion

        #region Private Methods

        private void UpdatePointingState()
        {
            if (handTracking.IsLeftHandTracked)
            {
                bool wasPointing = leftPointing;
                leftPointing = handTracking.IsPointing(0);

                if (wasPointing != leftPointing)
                {
                    if (leftPointing && inputModeManager != null)
                    {
                        inputModeManager.RegisterHandInput();
                    }
                    Logging.Log($"[Quest3HandInput] Left hand pointing: {leftPointing}");
                }
            }
            else
            {
                leftPointing = false;
            }

            if (handTracking.IsRightHandTracked)
            {
                bool wasPointing = rightPointing;
                rightPointing = handTracking.IsPointing(1);

                if (wasPointing != rightPointing)
                {
                    if (rightPointing && inputModeManager != null)
                    {
                        inputModeManager.RegisterHandInput();
                    }
                    Logging.Log($"[Quest3HandInput] Right hand pointing: {rightPointing}");
                }
            }
            else
            {
                rightPointing = false;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets whether a hand is currently pointing.
        /// </summary>
        /// <param name="handedness">Which hand.</param>
        /// <returns>True if pointing gesture detected.</returns>
        public bool IsHandPointing(HandednessQ3 handedness)
        {
            return handedness == HandednessQ3.Left ? leftPointing : rightPointing;
        }

        /// <summary>
        /// Gets whether a hand is currently pinching.
        /// </summary>
        /// <param name="handedness">Which hand.</param>
        /// <returns>True if pinching.</returns>
        public bool IsHandPinching(HandednessQ3 handedness)
        {
            return handedness == HandednessQ3.Left ? leftPinching : rightPinching;
        }

        /// <summary>
        /// Gets the index fingertip position for direct touch.
        /// </summary>
        /// <param name="handedness">Which hand.</param>
        /// <param name="position">Fingertip position.</param>
        /// <returns>True if available.</returns>
        public bool TryGetTouchPosition(HandednessQ3 handedness, out Vector3 position)
        {
            position = Vector3.zero;
            if (handTracking == null)
            {
                return false;
            }
            return handTracking.TryGetIndexTipPosition((int)handedness, out position);
        }

        #endregion
    }
}
