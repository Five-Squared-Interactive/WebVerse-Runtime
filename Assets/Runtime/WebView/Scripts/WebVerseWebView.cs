// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System.Collections.Generic;
using UnityEngine;
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
        /// Initialize the WebVerse WebView.
        /// </summary>
        public void Initialize()
        {
            webViewSetUp = false;
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
        /// Terminate the WebVerse WebView.
        /// </summary>
        public void Terminate()
        {
            if (webViewObject != null)
            {
                Destroy(webViewObject);
            }
#if VUPLEX_INCLUDED
            cwvPrefab = null;
#endif
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

            webViewObject.SetActive(true);
        }

        /// <summary>
        /// Hid the WebVerse WebView.
        /// </summary>
        public void Hide()
        {
            if (webViewObject == null)
            {
                Logging.LogError("[WebVerseWebView->Hide] WebVerse WebView not set up.");
                return;
            }

            webViewObject.SetActive(false);
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

            webViewSetUp = true;
#endif
        }
    }
}