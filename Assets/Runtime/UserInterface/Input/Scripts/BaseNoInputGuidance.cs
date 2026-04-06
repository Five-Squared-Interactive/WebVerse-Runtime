// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.UI;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Abstract base class for VR no-input guidance displays.
    /// Shows helpful messages when input is lost or unavailable.
    /// Extend this class for platform-specific implementations.
    /// </summary>
    public abstract class BaseNoInputGuidance : MonoBehaviour
    {
        #region Enums

        /// <summary>
        /// The current guidance display state.
        /// </summary>
        protected enum GuidanceState
        {
            Hidden,
            NoInput,
            HandsLost,
            HandsLostControllersAvailable
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Canvas group for fading the guidance.
        /// </summary>
        [Tooltip("Canvas group for fading.")]
        public CanvasGroup canvasGroup;

        /// <summary>
        /// Text component for guidance message.
        /// </summary>
        [Tooltip("Text for guidance message.")]
        public Text guidanceText;

        /// <summary>
        /// Time in seconds before showing no-input guidance.
        /// </summary>
        [Tooltip("Delay before showing guidance (seconds).")]
        public float noInputDelay = 2.0f;

        /// <summary>
        /// Fade duration for showing/hiding guidance.
        /// </summary>
        [Tooltip("Fade duration (seconds).")]
        public float fadeDuration = 0.5f;

        /// <summary>
        /// Whether to position guidance in world space.
        /// </summary>
        [Tooltip("Position guidance in world space.")]
        public bool worldSpacePositioning = true;

        /// <summary>
        /// Distance from camera for world-space positioning.
        /// </summary>
        [Tooltip("Distance from camera (meters).")]
        public float distanceFromCamera = 2.5f;

        /// <summary>
        /// Message when both hands and controllers unavailable.
        /// </summary>
        [Tooltip("Message when no input available.")]
        [TextArea]
        public string noInputMessage = "Move hands into view or pick up controllers";

        /// <summary>
        /// Message when hand tracking lost but controllers available.
        /// </summary>
        [Tooltip("Message when hands lost but controllers available.")]
        [TextArea]
        public string handsLostControllersAvailableMessage = "Use controllers or move hands into view";

        /// <summary>
        /// Message when hand tracking lost (both hands).
        /// </summary>
        [Tooltip("Message when hand tracking lost.")]
        [TextArea]
        public string handTrackingLostMessage = "Move your hands into the camera's view";

        #endregion

        #region Protected Fields

        protected float targetAlpha;
        protected bool isShowing;
        protected GuidanceState currentState = GuidanceState.Hidden;

        #endregion

        #region Unity Lifecycle

        protected virtual void Start()
        {
            var modeManager = GetInputModeManager();
            if (modeManager != null)
            {
                modeManager.OnInputModeChanged += OnInputModeChanged;
            }

            var handTracking = GetHandTrackingProvider();
            if (handTracking != null)
            {
                handTracking.OnTrackingLost += OnTrackingLost;
                handTracking.OnTrackingRecovered += OnTrackingRecovered;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            targetAlpha = 0f;
        }

        protected virtual void OnDestroy()
        {
            var modeManager = GetInputModeManager();
            if (modeManager != null)
            {
                modeManager.OnInputModeChanged -= OnInputModeChanged;
            }

            var handTracking = GetHandTrackingProvider();
            if (handTracking != null)
            {
                handTracking.OnTrackingLost -= OnTrackingLost;
                handTracking.OnTrackingRecovered -= OnTrackingRecovered;
            }
        }

        protected virtual void Update()
        {
            UpdateGuidanceState();
            UpdateFade();
            UpdatePosition();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Forces guidance to show with custom message.
        /// </summary>
        /// <param name="message">Custom message to display.</param>
        public void ShowCustomMessage(string message)
        {
            if (guidanceText != null)
            {
                guidanceText.text = message;
            }
            targetAlpha = 1f;
            isShowing = true;
        }

        /// <summary>
        /// Forces guidance to hide.
        /// </summary>
        public void Hide()
        {
            targetAlpha = 0f;
            isShowing = false;
            currentState = GuidanceState.Hidden;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Gets the input mode manager for this platform.
        /// </summary>
        /// <returns>The input mode manager.</returns>
        protected abstract BaseVRInputModeManager GetInputModeManager();

        /// <summary>
        /// Gets the hand tracking provider for this platform.
        /// </summary>
        /// <returns>The hand tracking provider, or null if not available.</returns>
        protected abstract IHandTrackingProvider GetHandTrackingProvider();

        #endregion

        #region Protected Methods

        protected virtual void OnInputModeChanged(VRInputMode previousMode, VRInputMode newMode)
        {
            if (newMode != VRInputMode.None)
            {
                Hide();
            }
        }

        protected virtual void OnTrackingLost(int handIndex, float duration)
        {
            var handTracking = GetHandTrackingProvider();
            if (handTracking == null)
            {
                return;
            }

            bool bothHandsLost = !handTracking.IsLeftHandTracked && !handTracking.IsRightHandTracked;

            if (bothHandsLost)
            {
                var modeManager = GetInputModeManager();
                bool controllersAvailable = modeManager != null &&
                    modeManager.CurrentInputMode == VRInputMode.Controllers;

                if (controllersAvailable)
                {
                    ShowState(GuidanceState.HandsLostControllersAvailable);
                }
                else
                {
                    ShowState(GuidanceState.HandsLost);
                }
            }
        }

        protected virtual void OnTrackingRecovered(int handIndex, float recoveryTime)
        {
            var handTracking = GetHandTrackingProvider();
            if (handTracking == null)
            {
                return;
            }

            if (handTracking.IsLeftHandTracked || handTracking.IsRightHandTracked)
            {
                if (currentState == GuidanceState.HandsLost ||
                    currentState == GuidanceState.HandsLostControllersAvailable)
                {
                    Hide();
                }
            }
        }

        protected virtual void UpdateGuidanceState()
        {
            var modeManager = GetInputModeManager();
            if (modeManager == null)
            {
                return;
            }

            if (modeManager.CurrentInputMode == VRInputMode.None)
            {
                if (modeManager.TimeSinceLastInput >= noInputDelay)
                {
                    ShowState(GuidanceState.NoInput);
                }
            }
            else if (currentState == GuidanceState.NoInput)
            {
                Hide();
            }
        }

        protected virtual void ShowState(GuidanceState state)
        {
            if (currentState == state && isShowing)
            {
                return;
            }

            currentState = state;

            string message = GetMessageForState(state);
            if (guidanceText != null)
            {
                guidanceText.text = message;
            }

            targetAlpha = 1f;
            isShowing = true;

            Logging.Log($"[{GetType().Name}] Showing guidance: {state}");
        }

        protected virtual string GetMessageForState(GuidanceState state)
        {
            switch (state)
            {
                case GuidanceState.NoInput:
                    return noInputMessage;
                case GuidanceState.HandsLost:
                    return handTrackingLostMessage;
                case GuidanceState.HandsLostControllersAvailable:
                    return handsLostControllersAvailableMessage;
                default:
                    return "";
            }
        }

        protected virtual void UpdateFade()
        {
            if (canvasGroup == null)
            {
                return;
            }

            float currentAlpha = canvasGroup.alpha;
            if (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
            {
                float fadeSpeed = 1f / fadeDuration;
                canvasGroup.alpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            }
            else
            {
                canvasGroup.alpha = targetAlpha;
            }
        }

        protected virtual void UpdatePosition()
        {
            if (!worldSpacePositioning || !isShowing)
            {
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Vector3 forward = mainCamera.transform.forward;
            Vector3 targetPosition = mainCamera.transform.position + forward * distanceFromCamera;

            transform.position = targetPosition;

            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0);
        }

        #endregion
    }
}
