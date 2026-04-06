// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Abstract base class for VR world loading error handlers.
    /// Provides VR-friendly error dialogs when world loading fails.
    /// Extend this class for platform-specific implementations.
    /// </summary>
    public abstract class BaseWorldLoadingErrorHandler : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Fired when user selects Retry.
        /// </summary>
        public event Action OnRetryRequested;

        /// <summary>
        /// Fired when user selects Back to Browser.
        /// </summary>
        public event Action OnBackToBrowserRequested;

        #endregion

        #region Configuration

        /// <summary>
        /// Canvas group for error dialog.
        /// </summary>
        [Tooltip("Canvas group for error dialog.")]
        public CanvasGroup canvasGroup;

        /// <summary>
        /// Text for error message.
        /// </summary>
        [Tooltip("Text for error message.")]
        public Text errorMessageText;

        /// <summary>
        /// Text for error details.
        /// </summary>
        [Tooltip("Text for error details.")]
        public Text errorDetailsText;

        /// <summary>
        /// Retry button.
        /// </summary>
        [Tooltip("Retry button.")]
        public Button retryButton;

        /// <summary>
        /// Back to browser button.
        /// </summary>
        [Tooltip("Back to browser button.")]
        public Button backButton;

        /// <summary>
        /// Keep waiting button (for timeout).
        /// </summary>
        [Tooltip("Keep waiting button.")]
        public Button keepWaitingButton;

        /// <summary>
        /// Loading timeout in seconds.
        /// </summary>
        [Tooltip("Loading timeout (seconds).")]
        public float loadingTimeout = 30.0f;

        /// <summary>
        /// Warning timeout before showing extended loading message.
        /// </summary>
        [Tooltip("Warning timeout (seconds).")]
        public float warningTimeout = 10.0f;

        /// <summary>
        /// Number of retry attempts before showing additional guidance.
        /// </summary>
        [Tooltip("Retry attempts before extra guidance.")]
        public int maxRetryBeforeGuidance = 3;

        /// <summary>
        /// Fade duration for dialog.
        /// </summary>
        [Tooltip("Fade duration (seconds).")]
        public float fadeDuration = 0.3f;

        /// <summary>
        /// Distance from camera for dialog positioning.
        /// </summary>
        [Tooltip("Distance from camera (meters).")]
        public float distanceFromCamera = 2.0f;

        #endregion

        #region Protected Fields

        protected float loadingStartTime;
        protected bool isLoading;
        protected bool isShowingWarning;
        protected bool isShowingError;
        protected string currentLoadingUrl;
        protected int retryAttempts;
        protected float targetAlpha;

        #endregion

        #region Unity Lifecycle

        protected virtual void Start()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (keepWaitingButton != null)
            {
                keepWaitingButton.onClick.AddListener(OnKeepWaitingClicked);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            targetAlpha = 0f;
        }

        protected virtual void Update()
        {
            UpdateFade();
            UpdatePosition();
            CheckLoadingTimeout();
            CheckRuntimeState();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when world loading starts.
        /// </summary>
        /// <param name="url">URL being loaded.</param>
        public void OnWorldLoadingStarted(string url)
        {
            currentLoadingUrl = url;
            loadingStartTime = Time.time;
            isLoading = true;
            isShowingWarning = false;
            isShowingError = false;

            Hide();

            Logging.Log($"[{GetType().Name}] Loading started: {url}");
        }

        /// <summary>
        /// Called when world loading succeeds.
        /// </summary>
        public void OnWorldLoadingSucceeded()
        {
            isLoading = false;
            isShowingWarning = false;
            Hide();
            retryAttempts = 0;

            Logging.Log($"[{GetType().Name}] Loading succeeded");
        }

        /// <summary>
        /// Called when world loading fails.
        /// </summary>
        /// <param name="errorMessage">Error description.</param>
        public void OnWorldLoadingFailed(string errorMessage)
        {
            isLoading = false;
            isShowingWarning = false;
            ShowError(errorMessage);

            Logging.Log($"[{GetType().Name}] Loading failed: {errorMessage}");
        }

        /// <summary>
        /// Shows a partial load warning.
        /// </summary>
        /// <param name="warningMessage">Warning description.</param>
        public void ShowPartialLoadWarning(string warningMessage)
        {
            ShowWarning("Some content may not display correctly", warningMessage);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Checks the runtime state for errors.
        /// Override to integrate with platform-specific runtime.
        /// </summary>
        protected abstract void CheckRuntimeState();

        #endregion

        #region Protected Methods

        protected virtual void CheckLoadingTimeout()
        {
            if (!isLoading || isShowingError)
            {
                return;
            }

            float loadingDuration = Time.time - loadingStartTime;

            if (loadingDuration >= warningTimeout && !isShowingWarning)
            {
                ShowTimeoutWarning();
            }

            if (loadingDuration >= loadingTimeout)
            {
                isLoading = false;
                ShowTimeoutError();
            }
        }

        protected virtual void ShowTimeoutWarning()
        {
            isShowingWarning = true;

            if (errorMessageText != null)
            {
                errorMessageText.text = "Loading is taking longer than expected";
            }

            if (errorDetailsText != null)
            {
                errorDetailsText.text = "The world is still loading. You can keep waiting or cancel.";
            }

            if (retryButton != null) retryButton.gameObject.SetActive(false);
            if (keepWaitingButton != null) keepWaitingButton.gameObject.SetActive(true);
            if (backButton != null) backButton.gameObject.SetActive(true);

            targetAlpha = 1f;

            Logging.Log($"[{GetType().Name}] Showing timeout warning");
        }

        protected virtual void ShowTimeoutError()
        {
            isShowingError = true;

            if (errorMessageText != null)
            {
                errorMessageText.text = "World loading timed out";
            }

            if (errorDetailsText != null)
            {
                errorDetailsText.text = "The world took too long to load. Check your connection and try again.";
            }

            if (retryButton != null) retryButton.gameObject.SetActive(true);
            if (keepWaitingButton != null) keepWaitingButton.gameObject.SetActive(false);
            if (backButton != null) backButton.gameObject.SetActive(true);

            targetAlpha = 1f;

            Logging.Log($"[{GetType().Name}] Showing timeout error");
        }

        protected virtual void ShowError(string errorMessage)
        {
            isShowingError = true;
            retryAttempts++;

            if (errorMessageText != null)
            {
                errorMessageText.text = "Failed to load world";
            }

            if (errorDetailsText != null)
            {
                string details = GetUserFriendlyErrorMessage(errorMessage);
                if (retryAttempts >= maxRetryBeforeGuidance)
                {
                    details += "\n\nThis world may be temporarily unavailable. Try a different world or check back later.";
                }
                errorDetailsText.text = details;
            }

            if (retryButton != null) retryButton.gameObject.SetActive(true);
            if (keepWaitingButton != null) keepWaitingButton.gameObject.SetActive(false);
            if (backButton != null) backButton.gameObject.SetActive(true);

            targetAlpha = 1f;

            Logging.Log($"[{GetType().Name}] Showing error (attempt {retryAttempts})");
        }

        protected virtual void ShowWarning(string title, string message)
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = title;
            }

            if (errorDetailsText != null)
            {
                errorDetailsText.text = message;
            }

            if (retryButton != null) retryButton.gameObject.SetActive(false);
            if (keepWaitingButton != null) keepWaitingButton.gameObject.SetActive(false);
            if (backButton != null) backButton.gameObject.SetActive(true);

            targetAlpha = 1f;
        }

        protected virtual string GetUserFriendlyErrorMessage(string technicalMessage)
        {
            if (string.IsNullOrEmpty(technicalMessage))
            {
                return "An unknown error occurred while loading the world.";
            }

            if (technicalMessage.Contains("network") || technicalMessage.Contains("connection"))
            {
                return "Unable to connect to the world. Check your internet connection.";
            }

            if (technicalMessage.Contains("timeout"))
            {
                return "The world took too long to respond. The server may be busy.";
            }

            if (technicalMessage.Contains("404") || technicalMessage.Contains("not found"))
            {
                return "The world could not be found. It may have been moved or deleted.";
            }

            if (technicalMessage.Contains("parse") || technicalMessage.Contains("format"))
            {
                return "The world data appears to be corrupted or in an unsupported format.";
            }

            return "An error occurred while loading the world. Please try again.";
        }

        protected virtual void Hide()
        {
            targetAlpha = 0f;
            isShowingWarning = false;
            isShowingError = false;
        }

        protected virtual void OnRetryClicked()
        {
            Hide();
            OnRetryRequested?.Invoke();

            if (!string.IsNullOrEmpty(currentLoadingUrl))
            {
                OnWorldLoadingStarted(currentLoadingUrl);
            }

            Logging.Log($"[{GetType().Name}] Retry requested");
        }

        protected virtual void OnBackClicked()
        {
            Hide();
            isLoading = false;
            OnBackToBrowserRequested?.Invoke();

            Logging.Log($"[{GetType().Name}] Back to browser requested");
        }

        protected virtual void OnKeepWaitingClicked()
        {
            loadingStartTime = Time.time - warningTimeout + 10f;
            isShowingWarning = false;
            Hide();

            Logging.Log($"[{GetType().Name}] Keep waiting requested");
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
            if (targetAlpha <= 0.01f)
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
