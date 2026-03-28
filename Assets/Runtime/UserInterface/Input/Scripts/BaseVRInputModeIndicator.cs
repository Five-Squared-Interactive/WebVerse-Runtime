// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.UI;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Abstract base class for VR input mode indicators.
    /// Displays the current input mode (Hands or Controllers) in VR.
    /// Extend this class for platform-specific implementations.
    /// </summary>
    public abstract class BaseVRInputModeIndicator : MonoBehaviour
    {
        #region Configuration

        /// <summary>
        /// The canvas group for fading the indicator.
        /// </summary>
        [Tooltip("Canvas group for fading.")]
        public CanvasGroup canvasGroup;

        /// <summary>
        /// Text component showing the mode name.
        /// </summary>
        [Tooltip("Text showing current mode.")]
        public Text modeText;

        /// <summary>
        /// Image showing hand icon.
        /// </summary>
        [Tooltip("Hand icon image.")]
        public Image handIcon;

        /// <summary>
        /// Image showing controller icon.
        /// </summary>
        [Tooltip("Controller icon image.")]
        public Image controllerIcon;

        /// <summary>
        /// Time to show indicator after mode change.
        /// </summary>
        [Tooltip("Duration to show indicator after mode change (seconds).")]
        public float showDuration = 3.0f;

        /// <summary>
        /// Fade out duration.
        /// </summary>
        [Tooltip("Fade out duration (seconds).")]
        public float fadeDuration = 0.5f;

        /// <summary>
        /// Whether indicator follows the camera.
        /// </summary>
        [Tooltip("Whether indicator follows camera position.")]
        public bool followCamera = true;

        /// <summary>
        /// Distance from camera when following.
        /// </summary>
        [Tooltip("Distance from camera (meters).")]
        public float distanceFromCamera = 2.0f;

        /// <summary>
        /// Vertical offset from camera center.
        /// </summary>
        [Tooltip("Vertical offset from camera center (meters).")]
        public float verticalOffset = -0.5f;

        /// <summary>
        /// Text to display for hands mode.
        /// </summary>
        [Tooltip("Text for hands mode.")]
        public string handsText = "Hands";

        /// <summary>
        /// Text to display for controllers mode.
        /// </summary>
        [Tooltip("Text for controllers mode.")]
        public string controllersText = "Controllers";

        /// <summary>
        /// Text to display for no input mode.
        /// </summary>
        [Tooltip("Text for no input mode.")]
        public string noInputText = "No Input";

        #endregion

        #region Protected Fields

        protected VRInputMode lastDisplayedMode = VRInputMode.None;
        protected float showTimer;
        protected float targetAlpha;
        protected bool isShowing;

        #endregion

        #region Unity Lifecycle

        protected virtual void Start()
        {
            var modeManager = GetInputModeManager();
            if (modeManager != null)
            {
                modeManager.OnInputModeChanged += OnInputModeChanged;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            targetAlpha = 0f;
            isShowing = false;
        }

        protected virtual void OnDestroy()
        {
            var modeManager = GetInputModeManager();
            if (modeManager != null)
            {
                modeManager.OnInputModeChanged -= OnInputModeChanged;
            }
        }

        protected virtual void Update()
        {
            UpdateFade();

            if (followCamera && isShowing)
            {
                UpdatePosition();
            }

            if (isShowing && showTimer > 0)
            {
                showTimer -= Time.deltaTime;
                if (showTimer <= 0)
                {
                    targetAlpha = 0f;
                    isShowing = false;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Forces the indicator to show with the current mode.
        /// </summary>
        public void Show()
        {
            var modeManager = GetInputModeManager();
            if (modeManager != null)
            {
                ShowMode(modeManager.CurrentInputMode);
            }
        }

        /// <summary>
        /// Immediately hides the indicator.
        /// </summary>
        public void Hide()
        {
            targetAlpha = 0f;
            isShowing = false;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Gets the input mode manager for this platform.
        /// </summary>
        /// <returns>The input mode manager.</returns>
        protected abstract BaseVRInputModeManager GetInputModeManager();

        #endregion

        #region Protected Methods

        protected virtual void OnInputModeChanged(VRInputMode previousMode, VRInputMode newMode)
        {
            ShowMode(newMode);
        }

        protected virtual void ShowMode(VRInputMode mode)
        {
            lastDisplayedMode = mode;
            UpdateVisuals(mode);

            targetAlpha = 1f;
            showTimer = showDuration;
            isShowing = true;

            Logging.Log($"[{GetType().Name}] Showing mode: {mode}");
        }

        protected virtual void UpdateVisuals(VRInputMode mode)
        {
            if (modeText != null)
            {
                modeText.text = GetModeText(mode);
            }

            if (handIcon != null)
            {
                handIcon.gameObject.SetActive(mode == VRInputMode.Hands);
            }

            if (controllerIcon != null)
            {
                controllerIcon.gameObject.SetActive(mode == VRInputMode.Controllers);
            }
        }

        protected virtual string GetModeText(VRInputMode mode)
        {
            switch (mode)
            {
                case VRInputMode.Hands:
                    return handsText;
                case VRInputMode.Controllers:
                    return controllersText;
                case VRInputMode.None:
                    return noInputText;
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
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 targetPosition = mainCamera.transform.position +
                forward * distanceFromCamera +
                Vector3.up * verticalOffset;

            transform.position = targetPosition;

            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0);
        }

        #endregion
    }
}
