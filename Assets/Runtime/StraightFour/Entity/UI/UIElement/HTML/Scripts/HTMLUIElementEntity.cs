// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.StraightFour.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
#if WV_VR_ENABLED
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif
#if VUPLEX_INCLUDED
using Vuplex.WebView;
#endif

namespace FiveSQD.StraightFour.Entity
{
    public class HTMLUIElementEntity : UIElementEntity
    {
        /// <summary>
        /// Messaging API setup for JavaScript.
        /// </summary>
        private readonly string messagingAPI =
            "function postWorldMessage(message) {" +
            "  window.vuplex.postMessage(message);" +
            "}";

        /// <summary>
        /// Message passing API setup for JavaScript.
        /// </summary>
        private readonly string messagePassingAPI =
            "class VuplexPolyfill{constructor(){this._listeners={};" + 
            "window.addEventListener('message',this._handleWindowMessage.bind(this));" +
            "}addEventListener(eventName,listener){if(!this._listeners[eventName]){" +
            "this._listeners[eventName]=[];}if(this._listeners[eventName].indexOf(listener)===-1){" +
            "this._listeners[eventName].push(listener);}}removeEventListener(eventName, listener){" +
            "if(!this._listeners[eventName]){return;}const index=this._listeners[eventName].indexOf(listener);" +
            "if(index!==-1){this._listeners[eventName].splice(index,1);}}postMessage(message){" +
            "const messageString=typeof message==='string'?message:JSON.stringify(message);" +
            "parent.postMessage({type:'vuplex.postMessage',message:messageString},'*')}_emit(eventName,...args)" +
            "{if(!this._listeners[eventName]){return;}for(const listener of this._listeners[eventName]){" +
            "try{listener(...args);}catch(error){console.error(`An error occurred while invoking the '${eventName}'" +
            " event handler.`,error);}}}_handleWindowMessage(event){if (event.data&&event.data.type==='vuplex.postMessage')" +
            "{this._emit('message',{data:event.data.message});}};}if(!window.vuplex){window.vuplex=new VuplexPolyfill();}";

        /// <summary>
        /// Action to invoke when a world message is received from the HTML entity.
        /// </summary>
        public Action<string> onWorldMessage;

#if VUPLEX_INCLUDED
        /// <summary>
        /// WebView Prefab instance.
        /// </summary>
        private CanvasWebViewPrefab canvasWebViewPrefab;
#endif

        /// <summary>
        /// Queue of URLs to load.
        /// </summary>
        private Queue<string> urlLoadQueue;

        /// <summary>
        /// Queue of HTML documents to load.
        /// </summary>
        private Queue<string> htmlLoadQueue;

        /// <summary>
        /// Queue of JavaScript execution scripts.
        /// </summary>
        private Queue<Tuple<string, Action<string>>> javascriptExecuteQueue;

        /// <summary>
        /// Whether or not messaging API has been set up.
        /// </summary>
        private bool messagingAPISetUp;

        /// <summary>
        /// Whether or not message passing API has been set up.
        /// </summary>
        private bool messagePassingAPISetUp;

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /// <param name="parentCanvas">Canvas to place the button entity on.</param>
        public override void Initialize(Guid idToSet, UIEntity parentCanvas)
        {
            base.Initialize(idToSet, parentCanvas);

            messagingAPISetUp = false;
            messagePassingAPISetUp = false;

            GameObject canvasWebView = Instantiate(StraightFour.ActiveWorld.entityManager.canvasWebViewPrefab);
            canvasWebView.transform.SetParent(transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
#if VUPLEX_INCLUDED
            canvasWebViewPrefab = canvasWebView.GetComponent<CanvasWebViewPrefab>();
            canvasWebViewPrefab.LogConsoleMessages = true;
            RectTransform rt = canvasWebView.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = canvasWebView.gameObject.AddComponent<RectTransform>();
            }
            uiElementRectTransform = rt;
#endif
            urlLoadQueue = new Queue<string>();
            htmlLoadQueue = new Queue<string>();
            javascriptExecuteQueue = new Queue<Tuple<string, Action<string>>>();

            if (parentCanvas != null)
            {
                SetParent(parentCanvas);
                ((CanvasEntity) parentCanvas).canvasObject.sortingOrder = -1;
            }

            MakeHidden();

#if WV_VR_ENABLED
            canvasWebView.AddComponent<TrackedDeviceGraphicRaycaster>();
#endif
        }

        public override bool SetSizePercent(Vector2 percent, bool synchronize = true)
        {
            targetSize = percent;

            return CorrectSizeAndPosition(Screen.width, Screen.height);
        }

        public override bool SetPositionPercent(Vector2 percent, bool synchronize = true)
        {
            targetPosition = percent;

            return CorrectSizeAndPosition(Screen.width, Screen.height);
        }

        /// <summary>
        /// Correct the size and position of the UI element entity.
        /// </summary>
        /// <param name="screenWidth">Width of the screen.</param>
        /// <param name="screenHeight">Height of the screen.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool CorrectSizeAndPosition(float screenWidth, float screenHeight)
        {
            RectTransform rt = uiElementRectTransform;
            if (rt == null)
            {
                LogSystem.LogWarning("[HTMLUIElementEntity->CorrectSizeAndPosition] No rect transform.");
                return false;
            }

            CanvasEntity parentCanvasEntity = GetParentCanvasEntity();
            if (parentCanvasEntity == null)
            {
                LogSystem.LogError("[HTMLUIElementEntity->CorrectSizeAndPosition] No parent canvas entity.");
                return false;
            }

            RectTransform parentRT = parentCanvasEntity.GetComponent<RectTransform>();
            if (parentRT == null)
            {
                LogSystem.LogError("[HTMLUIElementEntity->CorrectSizeAndPosition] No parent canvas entity rect transform.");
                return false;
            }

            Vector2 worldSize = new Vector2(parentRT.sizeDelta.x * targetSize.x, parentRT.sizeDelta.y * targetSize.y);
            Vector3 worldPos = new Vector3(parentRT.sizeDelta.x * targetPosition.x, -1 * parentRT.sizeDelta.y * targetPosition.y);
            
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            rt.sizeDelta = worldSize;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localPosition = Vector3.zero;
            rt.anchoredPosition = worldPos;

            return true;
        }

        /// <summary>
        /// Load content from a URL.
        /// </summary>
        /// <param name="url">URL to load content from.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool LoadFromURL(string url)
        {
#if VUPLEX_INCLUDED
            if (canvasWebViewPrefab == null)
            {
                LogSystem.LogError("[HTMLUIElementEntity->LoadFromURL] Invalid HTML UI Element entity.");
                return false;
            }

            urlLoadQueue.Enqueue(url);
#endif
            return true;
        }

        /// <summary>
        /// Load content from a URL.
        /// </summary>
        /// <param name="url">URL to load content from.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool LoadHTML(string html)
        {
#if VUPLEX_INCLUDED
            if (canvasWebViewPrefab == null)
            {
                LogSystem.LogError("[HTMLEntity->LoadHTML] Invalid HTML UI Element entity.");
                return false;
            }

            htmlLoadQueue.Enqueue(html);
#endif
            return true;
        }

        /// <summary>
        /// Get the current URL.
        /// </summary>
        /// <returns>The current URL, or null.</returns>
        public string GetURL()
        {
#if VUPLEX_INCLUDED
            if (canvasWebViewPrefab == null || canvasWebViewPrefab.WebView == null)
            {
                LogSystem.LogError("[HTMLEntity->GetURL] Invalid HTML UI Element entity.");
                return null;
            }

            return canvasWebViewPrefab.WebView.Url;
#else
            return null;
#endif
        }

        /// <summary>
        /// Execute JavaScript logic.
        /// </summary>
        /// <param name="logic">Logic to execute.</param>
        /// <param name="onComplete">Action to invoke upon completion. Provides return
        /// from JavaScript as string.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool ExecuteJavaScript(string logic, Action<string> onComplete)
        {
#if VUPLEX_INCLUDED
            if (canvasWebViewPrefab == null)
            {
                LogSystem.LogError("[HTMLEntity->ExecuteJavaScript] Invalid HTML UI Element entity.");
                return false;
            }

            javascriptExecuteQueue.Enqueue(new Tuple<string, Action<string>>(logic, onComplete));
#endif
            return false;
        }

        /// <summary>
        /// Set up the messaging API.
        /// </summary>
        private void SetUpMessagingAPI()
        {
#if VUPLEX_INCLUDED
            if (!messagingAPISetUp)
            {
                if (!canvasWebViewPrefab.WebView.PageLoadScripts.Contains(messagingAPI))
                {
                    canvasWebViewPrefab.WebView.PageLoadScripts.Add(messagingAPI);
                }

                canvasWebViewPrefab.WebView.MessageEmitted += (sender, e) =>
                {
                    if (onWorldMessage != null)
                    {
                        onWorldMessage.Invoke(e.Value);
                    }
                };

                messagingAPISetUp = true;
            }
#endif
        }

        /// <summary>
        /// Set up the message passing API.
        /// </summary>
        private void SetUpMessagePassingAPI()
        {
#if VUPLEX_INCLUDED
            if (!messagePassingAPISetUp)
            {
                if (!canvasWebViewPrefab.WebView.PageLoadScripts.Contains(messagePassingAPI))
                {
                    canvasWebViewPrefab.WebView.PageLoadScripts.Add(messagePassingAPI);
                }

                messagePassingAPISetUp = true;
            }
#endif
        }

        private void Update()
        {
#if VUPLEX_INCLUDED
            if (canvasWebViewPrefab == null || canvasWebViewPrefab.WebView == null)
            {
                return;
            }

            if (urlLoadQueue.Count > 0)
            {
                string urlToLoad = urlLoadQueue.Dequeue();
                SetUpMessagingAPI();
                SetUpMessagePassingAPI();
                canvasWebViewPrefab.WebView.LoadUrl(urlToLoad);
            }

            if (htmlLoadQueue.Count > 0)
            {
                string htmlToLoad = htmlLoadQueue.Dequeue();
                SetUpMessagingAPI();
                SetUpMessagePassingAPI();
                canvasWebViewPrefab.WebView.LoadHtml(htmlToLoad);
            }

            if (javascriptExecuteQueue.Count > 0)
            {
                Tuple<string, Action<string>> javascriptToExecute = javascriptExecuteQueue.Dequeue();
                SetUpMessagingAPI();
                SetUpMessagePassingAPI();
                
                if (javascriptToExecute != null)
                {
                    canvasWebViewPrefab.WebView.ExecuteJavaScript(javascriptToExecute.Item1, javascriptToExecute.Item2);
                }
            }
#endif
        }
    }
}