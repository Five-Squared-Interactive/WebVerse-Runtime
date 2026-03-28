// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
#if XR_HANDS_1_1_OR_NEWER
using UnityEngine.XR.Hands;
#endif
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Wrapper for XR Hands subsystem providing hand tracking state and joint data.
    /// Abstracts hand tracking for testability and cleaner integration with WebVerse input.
    /// Requires com.unity.xr.hands package to be installed for full functionality.
    /// Implements IHandTrackingProvider for platform-agnostic hand tracking integration.
    /// </summary>
    public class Quest3HandTracking : MonoBehaviour, IHandTrackingProvider
    {
        #region Events

        /// <summary>
        /// Fired when hand tracking state changes.
        /// Parameters: handIndex (0=left, 1=right), isTracked
        /// </summary>
        public event Action<int, bool> OnHandTrackingStateChanged;

        /// <summary>
        /// Fired when a pinch gesture is detected.
        /// Parameters: handIndex (0=left, 1=right), isPinching
        /// </summary>
        public event Action<int, bool> OnPinchStateChanged;

        /// <summary>
        /// Fired when hand tracking is lost for an extended period.
        /// Parameters: handIndex (0=left, 1=right), lostDuration (seconds)
        /// </summary>
        public event Action<int, float> OnTrackingLost;

        /// <summary>
        /// Fired when hand tracking recovers after being lost.
        /// Parameters: handIndex (0=left, 1=right), recoveryTime (seconds since lost)
        /// </summary>
        public event Action<int, float> OnTrackingRecovered;

        #endregion

        #region Properties

        /// <summary>
        /// Whether the left hand is currently tracked.
        /// </summary>
        public bool IsLeftHandTracked => leftHandTracked;

        /// <summary>
        /// Whether the right hand is currently tracked.
        /// </summary>
        public bool IsRightHandTracked => rightHandTracked;

        /// <summary>
        /// Whether the left hand is currently pinching.
        /// </summary>
        public bool IsLeftPinching => leftPinching;

        /// <summary>
        /// Whether the right hand is currently pinching.
        /// </summary>
        public bool IsRightPinching => rightPinching;

        /// <summary>
        /// Whether any hand tracking is available.
        /// </summary>
        public bool IsHandTrackingAvailable
        {
            get
            {
#if XR_HANDS_1_1_OR_NEWER
                return xrHandSubsystem != null && xrHandSubsystem.running;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Duration in seconds since left hand tracking was lost. 0 if tracked.
        /// </summary>
        public float LeftHandLostDuration => leftHandTracked ? 0f : (Time.time - leftHandLostTime);

        /// <summary>
        /// Duration in seconds since right hand tracking was lost. 0 if tracked.
        /// </summary>
        public float RightHandLostDuration => rightHandTracked ? 0f : (Time.time - rightHandLostTime);

        /// <summary>
        /// Last known position of left hand palm. Valid even when not tracked.
        /// </summary>
        public Vector3 LastLeftHandPosition => lastLeftHandPosition;

        /// <summary>
        /// Last known position of right hand palm. Valid even when not tracked.
        /// </summary>
        public Vector3 LastRightHandPosition => lastRightHandPosition;

        /// <summary>
        /// Whether left hand was recently tracked (within recovery window).
        /// </summary>
        public bool IsLeftHandRecentlyTracked => !leftHandTracked && LeftHandLostDuration < recentTrackingThreshold;

        /// <summary>
        /// Whether right hand was recently tracked (within recovery window).
        /// </summary>
        public bool IsRightHandRecentlyTracked => !rightHandTracked && RightHandLostDuration < recentTrackingThreshold;

        #endregion

        #region Configuration

        /// <summary>
        /// Pinch threshold for triggering pinch events (0-1).
        /// Higher values require fingers to be closer together.
        /// </summary>
        [Tooltip("Pinch threshold (0-1). Higher = fingers must be closer.")]
        [Range(0.5f, 0.95f)]
        public float pinchThreshold = 0.8f;

        /// <summary>
        /// Distance threshold for pinch detection (in meters).
        /// </summary>
        [Tooltip("Maximum distance between index tip and thumb tip for pinch (meters).")]
        public float pinchDistanceThreshold = 0.02f;

        /// <summary>
        /// Whether to show debug gizmos for hand joints.
        /// </summary>
        [Tooltip("Show debug gizmos for hand joints in editor.")]
        public bool showDebugGizmos = false;

        /// <summary>
        /// Time threshold for "recently tracked" status (seconds).
        /// </summary>
        [Tooltip("Time window for 'recently tracked' status (seconds).")]
        public float recentTrackingThreshold = 2.0f;

        /// <summary>
        /// Time before firing OnTrackingLost event (seconds).
        /// Brief occlusions don't trigger the event.
        /// </summary>
        [Tooltip("Delay before firing tracking lost event (seconds).")]
        public float trackingLostDelay = 0.5f;

        #endregion

        #region Private Fields

#if XR_HANDS_1_1_OR_NEWER
        private XRHandSubsystem xrHandSubsystem;
        private Dictionary<XRHandJointID, Pose> leftHandJoints = new Dictionary<XRHandJointID, Pose>();
        private Dictionary<XRHandJointID, Pose> rightHandJoints = new Dictionary<XRHandJointID, Pose>();
#endif
        private bool leftHandTracked = false;
        private bool rightHandTracked = false;
        private bool leftPinching = false;
        private bool rightPinching = false;

        // Fallback joint storage when XR Hands not available
        private Dictionary<int, Pose> leftHandJointsFallback = new Dictionary<int, Pose>();
        private Dictionary<int, Pose> rightHandJointsFallback = new Dictionary<int, Pose>();

        // Tracking loss state
        private float leftHandLostTime;
        private float rightHandLostTime;
        private Vector3 lastLeftHandPosition;
        private Vector3 lastRightHandPosition;
        private bool leftTrackingLostEventFired;
        private bool rightTrackingLostEventFired;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            FindHandSubsystem();
        }

        private void OnDisable()
        {
#if XR_HANDS_1_1_OR_NEWER
            if (xrHandSubsystem != null)
            {
                xrHandSubsystem.updatedHands -= OnHandsUpdated;
            }
#endif
        }

        private void Update()
        {
#if XR_HANDS_1_1_OR_NEWER
            // Try to find subsystem if not available
            if (xrHandSubsystem == null || !xrHandSubsystem.running)
            {
                FindHandSubsystem();
            }
#endif
            // Check for tracking loss events
            CheckTrackingLossEvents();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the ray origin for hand-based pointing.
        /// Uses the index finger tip and wrist to determine pointing direction.
        /// </summary>
        /// <param name="handIndex">Which hand (0=left, 1=right).</param>
        /// <param name="origin">Ray origin position.</param>
        /// <param name="direction">Ray direction.</param>
        /// <returns>True if ray data is available.</returns>
        public bool TryGetPointingRay(int handIndex, out Vector3 origin, out Vector3 direction)
        {
            origin = Vector3.zero;
            direction = Vector3.forward;

#if XR_HANDS_1_1_OR_NEWER
            var joints = handIndex == 0 ? leftHandJoints : rightHandJoints;
            bool isTracked = handIndex == 0 ? leftHandTracked : rightHandTracked;

            if (!isTracked || joints.Count == 0)
            {
                return false;
            }

            // Use index finger tip as origin, direction from wrist through index tip
            if (joints.TryGetValue(XRHandJointID.IndexTip, out Pose indexTip) &&
                joints.TryGetValue(XRHandJointID.Wrist, out Pose wrist))
            {
                origin = indexTip.position;
                direction = (indexTip.position - wrist.position).normalized;
                return true;
            }

            // Fallback: use palm
            if (joints.TryGetValue(XRHandJointID.Palm, out Pose palm))
            {
                origin = palm.position;
                direction = palm.forward;
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Gets the aim ray using Meta's hand tracking aim extension.
        /// More accurate than simple joint-based calculation.
        /// </summary>
        /// <param name="handIndex">Which hand (0=left, 1=right).</param>
        /// <param name="origin">Ray origin.</param>
        /// <param name="direction">Ray direction.</param>
        /// <returns>True if aim ray available.</returns>
        public bool TryGetAimRay(int handIndex, out Vector3 origin, out Vector3 direction)
        {
            // For now, delegate to pointing ray
            // TODO: Integrate Meta Hand Tracking Aim extension for more accurate aiming
            return TryGetPointingRay(handIndex, out origin, out direction);
        }

        /// <summary>
        /// Gets the index fingertip position for direct touch interaction.
        /// </summary>
        /// <param name="handIndex">Which hand (0=left, 1=right).</param>
        /// <param name="position">Fingertip position.</param>
        /// <returns>True if position available.</returns>
        public bool TryGetIndexTipPosition(int handIndex, out Vector3 position)
        {
            position = Vector3.zero;

#if XR_HANDS_1_1_OR_NEWER
            var joints = handIndex == 0 ? leftHandJoints : rightHandJoints;
            bool isTracked = handIndex == 0 ? leftHandTracked : rightHandTracked;

            if (!isTracked)
            {
                return false;
            }

            if (joints.TryGetValue(XRHandJointID.IndexTip, out Pose pose))
            {
                position = pose.position;
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Gets the current pinch strength (0-1) for a hand.
        /// </summary>
        /// <param name="handIndex">Which hand (0=left, 1=right).</param>
        /// <returns>Pinch strength 0-1, or 0 if not tracked.</returns>
        public float GetPinchStrength(int handIndex)
        {
#if XR_HANDS_1_1_OR_NEWER
            var joints = handIndex == 0 ? leftHandJoints : rightHandJoints;
            bool isTracked = handIndex == 0 ? leftHandTracked : rightHandTracked;

            if (!isTracked)
            {
                return 0f;
            }

            if (joints.TryGetValue(XRHandJointID.IndexTip, out Pose indexTip) &&
                joints.TryGetValue(XRHandJointID.ThumbTip, out Pose thumbTip))
            {
                float distance = Vector3.Distance(indexTip.position, thumbTip.position);
                // Convert distance to strength (closer = higher strength)
                float strength = 1f - Mathf.Clamp01(distance / pinchDistanceThreshold);
                return strength;
            }
#endif
            return 0f;
        }

        /// <summary>
        /// Checks if the hand is in a pointing gesture.
        /// Index finger extended, other fingers curled.
        /// </summary>
        /// <param name="handIndex">Which hand (0=left, 1=right).</param>
        /// <returns>True if pointing gesture detected.</returns>
        public bool IsPointing(int handIndex)
        {
#if XR_HANDS_1_1_OR_NEWER
            var joints = handIndex == 0 ? leftHandJoints : rightHandJoints;
            bool isTracked = handIndex == 0 ? leftHandTracked : rightHandTracked;

            if (!isTracked || joints.Count == 0)
            {
                return false;
            }

            // Simple pointing detection: index tip is extended away from palm
            if (joints.TryGetValue(XRHandJointID.IndexTip, out Pose indexTip) &&
                joints.TryGetValue(XRHandJointID.Palm, out Pose palm) &&
                joints.TryGetValue(XRHandJointID.MiddleTip, out Pose middleTip))
            {
                float indexDistance = Vector3.Distance(indexTip.position, palm.position);
                float middleDistance = Vector3.Distance(middleTip.position, palm.position);

                // Index should be more extended than middle finger
                return indexDistance > middleDistance * 1.2f;
            }
#endif
            return false;
        }

        #endregion

        #region Private Methods

        private void FindHandSubsystem()
        {
#if XR_HANDS_1_1_OR_NEWER
            var subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);

            if (subsystems.Count > 0)
            {
                xrHandSubsystem = subsystems[0];
                xrHandSubsystem.updatedHands -= OnHandsUpdated; // Remove if already subscribed
                xrHandSubsystem.updatedHands += OnHandsUpdated;
                Logging.Log("[Quest3HandTracking] XR Hand Subsystem found and connected.");
            }
#else
            Logging.Log("[Quest3HandTracking] XR Hands package not available. Hand tracking disabled.");
#endif
        }

#if XR_HANDS_1_1_OR_NEWER
        private void OnHandsUpdated(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            // Process left hand
            bool wasLeftTracked = leftHandTracked;
            leftHandTracked = (updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints) != 0;

            if (leftHandTracked)
            {
                UpdateJointData(subsystem.leftHand, leftHandJoints);
                UpdatePinchState(0); // 0 = left
                UpdateLastKnownPosition(0); // Save position while tracked

                // Check for recovery
                if (!wasLeftTracked && leftTrackingLostEventFired)
                {
                    float recoveryTime = Time.time - leftHandLostTime;
                    OnTrackingRecovered?.Invoke(0, recoveryTime);
                    leftTrackingLostEventFired = false;
                    Logging.Log($"[Quest3HandTracking] Left hand tracking recovered after {recoveryTime:F2}s");
                }
            }

            if (wasLeftTracked != leftHandTracked)
            {
                OnHandTrackingStateChanged?.Invoke(0, leftHandTracked);

                if (!leftHandTracked)
                {
                    leftHandLostTime = Time.time;
                    leftTrackingLostEventFired = false; // Will fire after delay
                }

                Logging.Log($"[Quest3HandTracking] Left hand tracking: {leftHandTracked}");
            }

            // Process right hand
            bool wasRightTracked = rightHandTracked;
            rightHandTracked = (updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandJoints) != 0;

            if (rightHandTracked)
            {
                UpdateJointData(subsystem.rightHand, rightHandJoints);
                UpdatePinchState(1); // 1 = right
                UpdateLastKnownPosition(1); // Save position while tracked

                // Check for recovery
                if (!wasRightTracked && rightTrackingLostEventFired)
                {
                    float recoveryTime = Time.time - rightHandLostTime;
                    OnTrackingRecovered?.Invoke(1, recoveryTime);
                    rightTrackingLostEventFired = false;
                    Logging.Log($"[Quest3HandTracking] Right hand tracking recovered after {recoveryTime:F2}s");
                }
            }

            if (wasRightTracked != rightHandTracked)
            {
                OnHandTrackingStateChanged?.Invoke(1, rightHandTracked);

                if (!rightHandTracked)
                {
                    rightHandLostTime = Time.time;
                    rightTrackingLostEventFired = false; // Will fire after delay
                }

                Logging.Log($"[Quest3HandTracking] Right hand tracking: {rightHandTracked}");
            }
        }

        private void UpdateJointData(XRHand hand, Dictionary<XRHandJointID, Pose> jointDict)
        {
            jointDict.Clear();

            for (int i = 0; i < XRHandJointID.EndMarker.ToIndex(); i++)
            {
                XRHandJointID jointId = XRHandJointIDUtility.FromIndex(i);
                XRHandJoint joint = hand.GetJoint(jointId);

                if (joint.TryGetPose(out Pose pose))
                {
                    jointDict[jointId] = pose;
                }
            }
        }

        private void UpdatePinchState(int handIndex)
        {
            bool wasPinching = handIndex == 0 ? leftPinching : rightPinching;
            float pinchStrength = GetPinchStrength(handIndex);
            bool isPinching = pinchStrength >= pinchThreshold;

            if (handIndex == 0)
            {
                leftPinching = isPinching;
            }
            else
            {
                rightPinching = isPinching;
            }

            if (wasPinching != isPinching)
            {
                OnPinchStateChanged?.Invoke(handIndex, isPinching);
            }
        }

        private void UpdateLastKnownPosition(int handIndex)
        {
            // Try to get palm position
            if (handIndex == 0)
            {
                if (leftHandJoints.TryGetValue(XRHandJointID.Palm, out Pose palm))
                {
                    lastLeftHandPosition = palm.position;
                }
            }
            else
            {
                if (rightHandJoints.TryGetValue(XRHandJointID.Palm, out Pose palm))
                {
                    lastRightHandPosition = palm.position;
                }
            }
        }
#endif

        private void CheckTrackingLossEvents()
        {
            // Check left hand
            if (!leftHandTracked && !leftTrackingLostEventFired)
            {
                float lostDuration = Time.time - leftHandLostTime;
                if (lostDuration >= trackingLostDelay)
                {
                    leftTrackingLostEventFired = true;
                    OnTrackingLost?.Invoke(0, lostDuration);
                    Logging.Log($"[Quest3HandTracking] Left hand tracking lost for {lostDuration:F2}s");
                }
            }

            // Check right hand
            if (!rightHandTracked && !rightTrackingLostEventFired)
            {
                float lostDuration = Time.time - rightHandLostTime;
                if (lostDuration >= trackingLostDelay)
                {
                    rightTrackingLostEventFired = true;
                    OnTrackingLost?.Invoke(1, lostDuration);
                    Logging.Log($"[Quest3HandTracking] Right hand tracking lost for {lostDuration:F2}s");
                }
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || !Application.isPlaying)
            {
                return;
            }

#if XR_HANDS_1_1_OR_NEWER
            // Draw left hand joints
            if (leftHandTracked)
            {
                Gizmos.color = Color.cyan;
                foreach (var joint in leftHandJoints.Values)
                {
                    Gizmos.DrawWireSphere(joint.position, 0.005f);
                }
            }

            // Draw right hand joints
            if (rightHandTracked)
            {
                Gizmos.color = Color.magenta;
                foreach (var joint in rightHandJoints.Values)
                {
                    Gizmos.DrawWireSphere(joint.position, 0.005f);
                }
            }
#endif
        }

        #endregion
    }
}
