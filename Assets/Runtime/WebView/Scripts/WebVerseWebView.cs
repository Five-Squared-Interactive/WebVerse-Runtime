// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if WV_VR_ENABLED
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif
#if VUPLEX_INCLUDED
using Vuplex.WebView;
#endif

namespace FiveSQD.WebVerse.WebView
{
    /// <summary>
    /// Class for a WebVerse WebView.
    /// </summary>
    public class WebVerseWebView : MonoBehaviour
    {
        /// <summary>
        /// The WebView object.
        /// </summary>
        private GameObject webViewObject;
#if VUPLEX_INCLUDED
        /// <summary>
        /// Canvas WebView Prefab.
        /// </summary>
        private CanvasWebViewPrefab cwvPrefab;
#endif
        /// <summary>
        /// URL load queue.
        /// </summary>
        private Queue<string> urlsToLoad;

        /// <summary>
        /// Whether or not the WebView is set up.
        /// </summary>
        private bool webViewSetUp;

        /// <summary>
        /// Whether VR mode has been set up.
        /// </summary>
        private bool vrModeSetUp;

        /// <summary>
        /// Local position offset in VR mode.
        /// </summary>
        private Vector3 vrLocalPosition = new Vector3(75.25f, 150, 790);

        /// <summary>
        /// Size of the WebView panel in VR mode (width, height in pixels).
        /// </summary>
        private Vector2 vrPanelSize = new Vector2(800, 500);

        /// <summary>
        /// Fired when the WebView visibility changes. Parameter is true when shown, false when hidden.
        /// </summary>
        public event Action<bool> OnVisibilityChanged;

        /// <summary>
        /// Fired when the page title changes. Parameter is the new title.
        /// </summary>
        public event Action<string> OnTitleChanged;

        /// <summary>
        /// Initialize the WebVerse WebView.
        /// </summary>
        public void Initialize()
        {
            webViewSetUp = false;
            vrModeSetUp = false;
            if (WebVerseRuntime.Instance == null || WebVerseRuntime.Instance.webVerseWebViewPrefab == null)
            {
                Logging.LogWarning("[WebVerseWebView->Initialize] WebView prefab not set, skipping WebView initialization.");
                urlsToLoad = new Queue<string>();
                return;
            }
            webViewObject = Instantiate(WebVerseRuntime.Instance.webVerseWebViewPrefab);
#if VUPLEX_INCLUDED
            cwvPrefab = webViewObject.GetComponentInChildren<CanvasWebViewPrefab>();
            if (cwvPrefab != null)
            {
                //cwvPrefab.KeyboardEnabled = false;
            }
#endif

            Hide();
            urlsToLoad = new Queue<string>();
            Logging.Log("[WebVerseWebView] Initialized.");
        }

        /// <summary>
        /// Set up VR mode by parenting to the multibar.
        /// </summary>
        /// <param name="multibarTransform">The multibar transform to parent to.</param>
        public void SetupVRMode(Transform multibarTransform)
        {
            if (webViewObject == null)
            {
                Logging.LogError("[WebVerseWebView->SetupVRMode] WebView object not initialized.");
                return;
            }

            if (multibarTransform == null)
            {
                Logging.LogError("[WebVerseWebView->SetupVRMode] Multibar transform is null.");
                return;
            }

            // Parent to the multibar
            webViewObject.transform.SetParent(multibarTransform);
            webViewObject.transform.localRotation = Quaternion.identity;
            webViewObject.transform.localScale = Vector3.one;

            Canvas canvas = webViewObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                // Convert to world space canvas for VR
                canvas.renderMode = RenderMode.WorldSpace;

                // Set up the RectTransform for VR panel size and position
                RectTransform rt = webViewObject.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Lower-left anchor
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.zero;
                    rt.pivot = Vector2.zero;
                    rt.anchoredPosition = new Vector2(vrLocalPosition.x, vrLocalPosition.y);
                    rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, vrLocalPosition.z);
                    rt.sizeDelta = vrPanelSize;
                }

                // Disable the standard GraphicRaycaster for VR (use TrackedDeviceGraphicRaycaster instead)
                GraphicRaycaster graphicRaycaster = webViewObject.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster != null)
                {
                    graphicRaycaster.enabled = false;
                }

#if WV_VR_ENABLED
                // Add TrackedDeviceGraphicRaycaster for VR input if not already present
                if (webViewObject.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                {
                    webViewObject.AddComponent<TrackedDeviceGraphicRaycaster>();
                }
#endif
            }

            vrModeSetUp = true;
            Logging.Log("[WebVerseWebView] VR mode setup complete, parented to multibar.");
        }

        /// <summary>
        /// Set up VR mode for the TabUI flow by parenting the content WebView
        /// to the chrome panel so they move together.
        /// </summary>
        /// <param name="chromeTransform">The chrome panel transform to parent to.</param>
        /// <param name="vrCamera">The VR camera for canvas event processing.</param>
        public void SetupVRModeForTabUI(Transform chromeTransform, Camera vrCamera)
        {
            if (webViewObject == null)
            {
                Logging.LogError("[WebVerseWebView->SetupVRModeForTabUI] WebView object not initialized.");
                return;
            }

            if (chromeTransform == null)
            {
                Logging.LogError("[WebVerseWebView->SetupVRModeForTabUI] Chrome transform is null.");
                return;
            }

            // Parent to the chrome panel so content moves with it
            webViewObject.transform.SetParent(chromeTransform);
            webViewObject.transform.localRotation = Quaternion.identity;
            webViewObject.transform.localScale = Vector3.one;

            Canvas canvas = webViewObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = vrCamera != null ? vrCamera : Camera.main;
                // Render behind the chrome overlay (lower sorting order)
                canvas.overrideSorting = true;
                canvas.sortingOrder = -1;

                // Match the chrome panel size and fill the content area below the chrome bar.
                // Chrome panel is 1600x900 with localScale 0.001.
                // Content fills the same space — the chrome bar overlays on top.
                RectTransform rt = webViewObject.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(1600, 900);
                    rt.localPosition = Vector3.zero;
                    rt.localScale = Vector3.one;
                }

                // Remove content insets — in VR the content fills the full panel
                SetContentInsets(0, 0, 0, 0);

                // Disable standard GraphicRaycaster for VR
                GraphicRaycaster graphicRaycaster = webViewObject.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster != null)
                {
                    graphicRaycaster.enabled = false;
                }

#if WV_VR_ENABLED
                if (webViewObject.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                {
                    webViewObject.AddComponent<TrackedDeviceGraphicRaycaster>();
                }
#endif
            }

            vrModeSetUp = true;
            Logging.Log("[WebVerseWebView] VR mode setup for TabUI complete, parented to chrome panel.");
        }

        /// <summary>
        /// Restore the content WebView from VR mode back to screen-space overlay.
        /// </summary>
        public void DisableVRMode()
        {
            if (webViewObject == null) return;

            webViewObject.transform.SetParent(null);

            Canvas canvas = webViewObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                canvas.overrideSorting = false;
                canvas.sortingOrder = 0;

                // Re-enable standard GraphicRaycaster
                GraphicRaycaster graphicRaycaster = webViewObject.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster != null)
                {
                    graphicRaycaster.enabled = true;
                }

#if WV_VR_ENABLED
                var trackedRaycaster = webViewObject.GetComponent<TrackedDeviceGraphicRaycaster>();
                if (trackedRaycaster != null)
                {
                    Destroy(trackedRaycaster);
                }
#endif
            }

            vrModeSetUp = false;
            Logging.Log("[WebVerseWebView] VR mode disabled, restored to screen-space.");
        }

        /// <summary>
        /// Terminate the WebVerse WebView.
        /// </summary>
        public void Terminate()
        {
#if VUPLEX_INCLUDED
            // Dispose Vuplex WebView before destroying to release native resources
            if (cwvPrefab != null)
            {
                cwvPrefab.Destroy();
                cwvPrefab = null;
            }
#endif
            if (webViewObject != null)
            {
                Destroy(webViewObject);
            }
            urlsToLoad = null;
            webViewSetUp = false;
            Logging.Log("[WebVerseWebView] Terminated.");
        }

        /// <summary>
        /// Load a URL.
        /// </summary>
        /// <param name="url">URL to load.</param>
        public void LoadURL(string url)
        {
#if VUPLEX_INCLUDED
            if (cwvPrefab == null)
            {
                Logging.LogError("[WebVerseWebView->LoadURL] WebVerse WebView not set up.");
                return;
            }

            urlsToLoad.Enqueue(url);
#endif
        }

        /// <summary>
        /// Unload.
        /// </summary>
        public void Unload()
        {
#if VUPLEX_INCLUDED
            if (cwvPrefab == null)
            {
                Logging.LogError("[WebVerseWebView->Unload] WebVerse WebView not set up.");
                return;
            }

            if (cwvPrefab.WebView != null)
            {
                cwvPrefab.WebView.StopLoad();
            }
#endif
        }

        /// <summary>
        /// Show the WebVerse WebView.
        /// </summary>
        public void Show()
        {
            if (webViewObject == null)
            {
                Logging.LogError("[WebVerseWebView->Show] WebVerse WebView not set up.");
                return;
            }

            Logging.Log("[WebVerseWebView->Show] Showing WebView. VR mode set up: " + vrModeSetUp);
            webViewObject.SetActive(true);
            OnVisibilityChanged?.Invoke(true);
        }

        /// <summary>
        /// Hide the WebVerse WebView.
        /// </summary>
        public void Hide()
        {
            if (webViewObject == null)
            {
                Logging.LogError("[WebVerseWebView->Hide] WebVerse WebView not set up.");
                return;
            }

            webViewObject.SetActive(false);
            OnVisibilityChanged?.Invoke(false);
        }

        /// <summary>
        /// Check if the WebView is currently visible.
        /// </summary>
        /// <returns>True if visible.</returns>
        public bool IsVisible()
        {
            return webViewObject != null && webViewObject.activeSelf;
        }

        /// <summary>
        /// Set content insets to offset the WebView from the screen edges.
        /// Used to leave room for the Tab UI chrome bar.
        /// </summary>
        /// <param name="top">Top inset in pixels.</param>
        /// <param name="left">Left inset in pixels.</param>
        /// <param name="bottom">Bottom inset in pixels.</param>
        /// <param name="right">Right inset in pixels.</param>
        public void SetContentInsets(float top, float left, float bottom, float right)
        {
#if VUPLEX_INCLUDED
            if (cwvPrefab == null) return;

            RectTransform rt = cwvPrefab.GetComponent<RectTransform>();
            if (rt == null) return;

            // Stretch anchors to fill parent
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Apply offsets (insets from edges)
            rt.offsetMin = new Vector2(left, bottom);    // left, bottom
            rt.offsetMax = new Vector2(-right, -top);    // right, top (negative = inward)

            Logging.Log($"[WebVerseWebView] Content insets set: top={top}, left={left}, bottom={bottom}, right={right}");
#endif
        }

        private void Update()
        {
#if VUPLEX_INCLUDED
            if (cwvPrefab != null && cwvPrefab.WebView != null)
            {
                if (webViewSetUp == false)
                {
                    SetUpWebView();
                }

                if (urlsToLoad.Count > 0)
                {
                    cwvPrefab.WebView.LoadUrl(urlsToLoad.Dequeue());
                }
            }
#endif
        }

        /// <summary>
        /// Set up the WebView.
        /// </summary>
        private void SetUpWebView()
        {
#if VUPLEX_INCLUDED
            if (cwvPrefab == null || cwvPrefab.WebView == null)
            {
                Logging.LogError("[WebVerseWebView->SetUpWebView] WebVerse WebView not set up.");
                return;
            }

            cwvPrefab.WebView.PageLoadScripts.Add(@"
                setInterval(() => {
                    const newLinks = document.querySelectorAll('a[href]:not([overridden])');
                    for (const link of newLinks) {
                        link.setAttribute('overridden', true);
                        link.addEventListener('click', event => {
                            window.vuplex.postMessage('link:' + link.href);
                            event.preventDefault();
                        });
                    }
                }, 250);
            ");

            cwvPrefab.WebView.MessageEmitted += (sender, eventArgs) => {
                var message = eventArgs.Value;
                var prefix = "link:";
                if (message.StartsWith(prefix))
                {
                    var linkUrl = message.Substring(prefix.Length);
                    WebVerseRuntime.Instance.LoadURL(linkUrl);
                }
            };

            cwvPrefab.WebView.TitleChanged += (sender, eventArgs) => {
                OnTitleChanged?.Invoke(eventArgs.Value);
            };

            webViewSetUp = true;
#endif
        }
    }
}
