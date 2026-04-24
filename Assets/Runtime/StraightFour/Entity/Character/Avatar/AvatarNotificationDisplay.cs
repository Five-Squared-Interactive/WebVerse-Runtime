// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using UnityEngine;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Displays avatar-related notifications to the user.
    /// Follows the InputModeIndicator pattern: auto-dismiss after configurable duration.
    /// If no UI elements are present (e.g., in tests), the notification is logged and
    /// events still fire, but no visual display occurs.
    /// </summary>
    public class AvatarNotificationDisplay : MonoBehaviour
    {
        /// <summary>
        /// Fired when a notification is shown. Parameter: the notification message.
        /// </summary>
        public event Action<string> OnNotificationShown;

        [SerializeField]
        private float displayDuration = 5f;

        [SerializeField]
        private float fadeDuration = 0.5f;

        private CanvasGroup _canvasGroup;
        private UnityEngine.UI.Text _messageText;
        private Coroutine _autoDismissCoroutine;

        /// <summary>
        /// The last message that was shown via Show().
        /// Useful for testing without requiring UI elements.
        /// </summary>
        public string LastMessage { get; private set; }

        private void Awake()
        {
            // Try to find UI elements — they may not exist in test environments
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _messageText = GetComponentInChildren<UnityEngine.UI.Text>();

            // Start hidden if canvas group exists
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Shows a notification message. Auto-dismisses after displayDuration.
        /// If no UI elements are present, the message is still stored in LastMessage
        /// and OnNotificationShown fires.
        /// </summary>
        /// <param name="message">The user-friendly message to display.</param>
        public void Show(string message)
        {
            LastMessage = message;

            // Cancel any existing auto-dismiss
            if (_autoDismissCoroutine != null)
            {
                StopCoroutine(_autoDismissCoroutine);
                _autoDismissCoroutine = null;
            }

            // Update UI if available
            if (_messageText != null)
            {
                _messageText.text = message;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = false;
            }

            OnNotificationShown?.Invoke(message);
            Debug.Log($"[AvatarNotificationDisplay] {message}");

            // Start auto-dismiss
            if (gameObject.activeInHierarchy)
            {
                _autoDismissCoroutine = StartCoroutine(AutoDismissCoroutine());
            }
        }

        /// <summary>
        /// Hides the notification immediately.
        /// </summary>
        public void Hide()
        {
            if (_autoDismissCoroutine != null)
            {
                StopCoroutine(_autoDismissCoroutine);
                _autoDismissCoroutine = null;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private IEnumerator AutoDismissCoroutine()
        {
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            if (_canvasGroup != null)
            {
                float startAlpha = _canvasGroup.alpha;
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                    yield return null;
                }
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }

            _autoDismissCoroutine = null;
        }
    }
}
