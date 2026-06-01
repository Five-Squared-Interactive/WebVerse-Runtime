// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Quest 3 implementation of IFadeTransition.
    /// Uses a full-screen Canvas overlay to fade to/from black during mode switches.
    /// </summary>
    public class Quest3FadeTransition : MonoBehaviour, IFadeTransition
    {
        private const float FADE_DURATION = 0.3f;

        private Image _fadeImage;
        private Coroutine _activeCoroutine;

        /// <summary>
        /// Initialize with a camera to parent the fade canvas to.
        /// </summary>
        public void Initialize(Camera camera)
        {
            // Create fade canvas
            var canvasGO = new GameObject("FadeCanvas");
            canvasGO.transform.SetParent(camera.transform, false);
            canvasGO.transform.localPosition = new Vector3(0, 0, 0.5f);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 32767;

            var rectTransform = canvasGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(2f, 2f);

            // Create black image panel
            var imageGO = new GameObject("FadePanel");
            imageGO.transform.SetParent(canvasGO.transform, false);

            _fadeImage = imageGO.AddComponent<Image>();
            _fadeImage.color = new Color(0, 0, 0, 0);
            _fadeImage.raycastTarget = false;

            var imageRect = imageGO.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;

            Logging.Log("[Quest3FadeTransition] Initialized.");
        }

        /// <summary>
        /// Fade the screen to black, then invoke the callback.
        /// </summary>
        public void FadeOut(Action callback)
        {
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, () =>
            {
                callback?.Invoke();
            }));
        }

        /// <summary>
        /// Fade the screen back from black to clear.
        /// </summary>
        public void FadeIn()
        {
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, null));
        }

        private IEnumerator FadeCoroutine(float fromAlpha, float toAlpha, Action onComplete)
        {
            if (_fadeImage == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < FADE_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FADE_DURATION);
                float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                _fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            _fadeImage.color = new Color(0, 0, 0, toAlpha);
            _activeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}