// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FiveSQD.WebVerse.Input.Mobile
{
    /// <summary>
    /// Class for interpreting Mobile touch input.
    /// </summary>
    public class MobileInput : BasePlatformInput
    {
        /// <summary>
        /// Whether touch input is enabled.
        /// </summary>
        public bool touchInputEnabled { get; set; } = true;

        /// <summary>
        /// Whether touch movement is enabled for locomotion.
        /// </summary>
        public bool touchMovementEnabled { get; set; } = true;

        /// <summary>
        /// Whether touch look is enabled for camera movement.
        /// </summary>
        public bool touchLookEnabled { get; set; } = true;

        /// <summary>
        /// Touch sensitivity for look movement.
        /// </summary>
        public float touchSensitivity { get; set; } = 1.0f;

        /// <summary>
        /// Minimum distance for touch drag to register as movement.
        /// </summary>
        public float touchDragThreshold { get; set; } = 10.0f;

        /// <summary>
        /// Whether pinch to zoom is enabled.
        /// </summary>
        public bool pinchZoomEnabled { get; set; } = true;

        /// <summary>
        /// Last touch position for delta calculations.
        /// </summary>
        private Vector2 lastTouchPosition;

        /// <summary>
        /// Whether we're currently in a drag operation.
        /// </summary>
        private bool isDragging = false;

        /// <summary>
        /// Initial distance between two touches for pinch detection.
        /// </summary>
        private float initialPinchDistance = 0f;

        /// <summary>
        /// Whether we're currently in a pinch gesture.
        /// </summary>
        private bool isPinching = false;

        /// <summary>
        /// Start of touch for gesture detection.
        /// </summary>
        private Vector2 touchStartPosition;

        /// <summary>
        /// Time when touch started.
        /// </summary>
        private float touchStartTime;

        /// <summary>
        /// Maximum time for tap detection (in seconds).
        /// </summary>
        public float tapTimeThreshold { get; set; } = 0.3f;

        /// <summary>
        /// Maximum distance for tap detection (in pixels).
        /// </summary>
        public float tapDistanceThreshold { get; set; } = 50.0f;

        /// <summary>
        /// Current primary touch position.
        /// </summary>
        public Vector2 primaryTouchPosition { get; private set; }

        /// <summary>
        /// Current secondary touch position.
        /// </summary>
        public Vector2 secondaryTouchPosition { get; private set; }

        /// <summary>
        /// Current touch count.
        /// </summary>
        public int touchCount { get; private set; }

        /// <summary>
        /// Invoked on primary touch press/release.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnPrimaryTouch(InputAction.CallbackContext context)
        {
            if (!touchInputEnabled)
                return;

            if (context.phase == InputActionPhase.Started)
            {
                touchStartPosition = primaryTouchPosition;
                touchStartTime = Time.time;
                lastTouchPosition = primaryTouchPosition;
                isDragging = false;

                // Trigger left click equivalent
                WebVerseRuntime.Instance.inputManager.Left();
                WebVerseRuntime.Instance.inputManager.leftValue = true;
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                float touchDuration = Time.time - touchStartTime;
                float touchDistance = Vector2.Distance(touchStartPosition, primaryTouchPosition);

                // Check if this was a tap gesture
                if (touchDuration < tapTimeThreshold && touchDistance < tapDistanceThreshold)
                {
                    // This was a tap - already handled by the Left() call above
                }

                isDragging = false;
                isPinching = false;

                // End left click equivalent
                WebVerseRuntime.Instance.inputManager.EndLeft();
                WebVerseRuntime.Instance.inputManager.leftValue = false;

                // End any ongoing movement
                if (touchMovementEnabled || touchLookEnabled)
                {
                    WebVerseRuntime.Instance.inputManager.EndMove();
                    WebVerseRuntime.Instance.inputManager.EndLook();
                }
            }
        }

        /// <summary>
        /// Invoked on primary touch position change.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnPrimaryTouchPosition(InputAction.CallbackContext context)
        {
            if (!touchInputEnabled)
                return;

            primaryTouchPosition = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Invoked on primary touch delta change.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnPrimaryTouchDelta(InputAction.CallbackContext context)
        {
            if (!touchInputEnabled)
                return;

            Vector2 touchDelta = context.ReadValue<Vector2>();

            // Only process movement if touch is active and we have significant movement
            if (WebVerseRuntime.Instance.inputManager.leftValue && touchDelta.magnitude > touchDragThreshold)
            {
                if (!isDragging)
                {
                    isDragging = true;
                }

                // Handle single touch drag as look movement
                if (touchCount == 1 && touchLookEnabled)
                {
                    Vector2 lookValue = touchDelta * touchSensitivity * Time.deltaTime;
                    WebVerseRuntime.Instance.inputManager.lookValue = lookValue;
                    WebVerseRuntime.Instance.inputManager.Look(lookValue);

                    // Apply look to DesktopRig if available (for camera movement)
                    if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                    {
                        WebVerseRuntime.Instance.inputManager.desktopRig.ApplyLook(lookValue);
                    }
                }
            }
        }

        /// <summary>
        /// Invoked on secondary touch press/release.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnSecondaryTouch(InputAction.CallbackContext context)
        {
            if (!touchInputEnabled)
                return;

            if (context.phase == InputActionPhase.Started)
            {
                // Secondary touch started - check for pinch gesture
                if (touchCount >= 2 && pinchZoomEnabled)
                {
                    initialPinchDistance = Vector2.Distance(primaryTouchPosition, secondaryTouchPosition);
                    isPinching = true;
                }
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                isPinching = false;
                initialPinchDistance = 0f;
            }
        }

        /// <summary>
        /// Invoked on secondary touch position change.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnSecondaryTouchPosition(InputAction.CallbackContext context)
        {
            if (!touchInputEnabled)
                return;

            secondaryTouchPosition = context.ReadValue<Vector2>();

            // Handle pinch gesture
            if (isPinching && touchCount >= 2 && pinchZoomEnabled)
            {
                float currentDistance = Vector2.Distance(primaryTouchPosition, secondaryTouchPosition);
                float pinchDelta = currentDistance - initialPinchDistance;

                if (Mathf.Abs(pinchDelta) > touchDragThreshold)
                {
                    // Convert pinch to scroll wheel equivalent (middle mouse scroll)
                    if (pinchDelta > 0)
                    {
                        // Pinch out - zoom in (scroll up equivalent)
                        WebVerseRuntime.Instance.inputManager.Middle();
                    }
                    else
                    {
                        // Pinch in - zoom out (scroll down equivalent)
                        WebVerseRuntime.Instance.inputManager.Middle();
                    }

                    initialPinchDistance = currentDistance;
                }
            }
        }

        /// <summary>
        /// Invoked on touch count change.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnTouchCount(InputAction.CallbackContext context)
        {
            if (!touchInputEnabled)
                return;

            touchCount = context.ReadValue<int>();

            // Reset gesture states when touch count changes
            if (touchCount < 2)
            {
                isPinching = false;
                initialPinchDistance = 0f;
            }
        }

        /// <summary>
        /// Get a raycast from the pointer (primary touch position).
        /// </summary>
        /// <param name="direction">Direction to cast the ray in.</param>
        /// <param name="pointerIndex">Index of the pointer to get raycast from.</param>
        /// <returns>A raycast from the pointer, or null.</returns>
        public override Tuple<RaycastHit, Vector3> GetPointerRaycast(Vector3 direction, int pointerIndex = 0)
        {
            if (pointerIndex == 0)
            {
                RaycastHit hit;
                Ray ray = StraightFour.StraightFour.ActiveWorld.cameraManager.cam.ScreenPointToRay(primaryTouchPosition);

                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;

                    if (objectHit)
                    {
                        return new Tuple<RaycastHit, Vector3>(hit, StraightFour.StraightFour.ActiveWorld.cameraManager.GetPosition(false));
                    }
                }
            }
            else if (pointerIndex == 1 && touchCount >= 2)
            {
                RaycastHit hit;
                Ray ray = StraightFour.StraightFour.ActiveWorld.cameraManager.cam.ScreenPointToRay(secondaryTouchPosition);

                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;

                    if (objectHit)
                    {
                        return new Tuple<RaycastHit, Vector3>(hit, StraightFour.StraightFour.ActiveWorld.cameraManager.GetPosition(false));
                    }
                }
            }
            else
            {
                Logging.LogWarning("[MobileInput->GetPointerRaycast] Only indices 0 and 1 are supported for Mobile touch input.");
            }

            return null;
        }
    }
}