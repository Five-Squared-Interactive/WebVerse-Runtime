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
    /// <summary>
    /// Class for an HTML entity.
    /// </summary>
    public class HTMLEntity : UIEntity
    {
        /// <summary>
        /// Messaging API setup for JavaScript.
        /// </summary>
        private readonly string messagingAPI =
            "postWorldMessage = function(message) {" +
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
        private WebViewPrefab webViewPrefab;
#endif
        /// <summary>
        /// Sizer gameobject.
        /// </summary>
        private GameObject sizer;

        /// <summary>
        /// Queue of URLs to load.
        /// </summary>
        private Queue<string> urlLoadQueue;

        /// <summary>
        /// Queue of HTML documents to load.
        /// </summary>
        private Queue<string> htmlLoadQueue;

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(Guid idToSet)
        {
            base.Initialize(idToSet);
#if VUPLEX_INCLUDED
            GameObject webView = Instantiate(StraightFour.ActiveWorld.entityManager.webViewPrefab);
            webView.transform.SetParent(transform);
            webViewPrefab = webView.GetComponent<WebViewPrefab>();
            webViewPrefab.LogConsoleMessages = true;
            sizer = webView.transform.GetChild(0).gameObject;
            urlLoadQueue = new Queue<string>();
            htmlLoadQueue = new Queue<string>();
#endif

#if WV_VR_ENABLED
            gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
#endif

            MakeHidden();
        }

        /// <summary>
        /// Set the position of the entity.
        /// </summary>
        /// <param name="position">Position to set.</param>
        /// <param name="local">Whether or not the position is local.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetPosition(Vector3 position, bool local, bool synchronize = true)
        {
            if (local)
            {
                transform.localPosition = position;
            }
            else
            {
                Vector3 worldOffset = StraightFour.ActiveWorld.worldOffset;
                transform.position = new Vector3(position.x + worldOffset.x,
                    position.y + worldOffset.y, position.z + worldOffset.z);;
            }

            if (synchronize && synchronizer != null && positionUpdateTime > minUpdateTime)
            {
                synchronizer.SetPosition(this, position);
                positionUpdateTime = 0;
            }

            return true;
        }

        /// <summary>
        /// Set the size of the entity. This method cannot be used to set the size of an HTML entity; a Vector2 must be used.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetSize(Vector3 size, bool synchronize = true)
        {
            LogSystem.LogWarning("[HTMLEntity->SetSize] Cannot set size of an HTML entity using a Vector3.");

            return false;
        }

        /// <summary>
        /// Set the size of the entity.
        /// Must be implemented by inheriting classes, as the size of an entity is dependent
        /// on its type.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetSize(Vector2 size, bool synchronize = true)
        {
#if VUPLEX_INCLUDED
            if (webViewPrefab == null)
            {
                LogSystem.LogError("[HTMLEntity->SetSize] Invalid HTML entity.");
                return false;
            }

            webViewPrefab.Resize(size.x, size.y);

            if (synchronize && synchronizer != null && sizeUpdateTime > minUpdateTime)
            {
                synchronizer.SetSize(this, size);
                sizeUpdateTime = 0;
            }
#endif
            return true;
        }

        /// <summary>
        /// Get the position of the entity.
        /// </summary>
        /// <param name="local">Whether or not to provide the local position.</param>
        /// <returns>The position of the entity.</returns>
        public override Vector3 GetPosition(bool local)
        {
            Vector3 worldOffset = StraightFour.ActiveWorld.worldOffset;
            return local ? transform.localPosition : new Vector3(transform.position.x - worldOffset.x,
                    transform.position.y - worldOffset.y, transform.position.z - worldOffset.z);;
        }

        /// <summary>
        /// Get the size of the entity.
        /// Must be implemented by inheriting classes, as the size of an entity is dependent
        /// on its type.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public override Vector3 GetSize()
        {
            if (sizer == null)
            {
                LogSystem.LogError("[HTMLEntity->GetSize] Invalid HTML entity.");
                return Vector3.zero;
            }

            return new Vector3(sizer.transform.position.x, sizer.transform.position.y, 0);
        }

        /// <summary>
        /// Load content from a URL.
        /// </summary>
        /// <param name="url">URL to load content from.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool LoadFromURL(string url)
        {
#if VUPLEX_INCLUDED
            if (webViewPrefab == null)
            {
                LogSystem.LogError("[HTMLEntity->LoadFromURL] Invalid HTML entity.");
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
            if (webViewPrefab == null)
            {
                LogSystem.LogError("[HTMLEntity->LoadHTML] Invalid HTML entity.");
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
            if (webViewPrefab == null || webViewPrefab.WebView == null)
            {
                LogSystem.LogError("[HTMLEntity->GetURL] Invalid HTML entity.");
                return null;
            }

            return webViewPrefab.WebView.Url;
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
            if (webViewPrefab == null || webViewPrefab.WebView == null)
            {
                LogSystem.LogError("[HTMLEntity->ExecuteJavaScript] Invalid HTML entity.");
                return false;
            }

            webViewPrefab.WebView.ExecuteJavaScript(logic, onComplete);
#endif
            return true;
        }

        /// <summary>
        /// Set up the messaging API.
        /// </summary>
        private void SetUpMessagingAPI()
        {
#if VUPLEX_INCLUDED
            webViewPrefab.WebView.PageLoadScripts.Add(messagingAPI);
            webViewPrefab.WebView.MessageEmitted += (sender, e) =>
            {
                if (onWorldMessage != null)
                {
                    onWorldMessage.Invoke(e.Value);
                }
            };
#endif
        }

        /// <summary>
        /// Set up the message passing API.
        /// </summary>
        private void SetUpMessagePassingAPI()
        {
#if VUPLEX_INCLUDED
            webViewPrefab.WebView.PageLoadScripts.Add(messagePassingAPI);
            webViewPrefab.WebView.MessageEmitted += (sender, e) =>
            {
                if (onWorldMessage != null)
                {
                    onWorldMessage.Invoke(e.Value);
                }
            };
#endif
        }

        private void Update()
        {
#if VUPLEX_INCLUDED
            if (webViewPrefab == null || webViewPrefab.WebView == null)
            {
                return;
            }

            if (urlLoadQueue.Count > 0)
            {
                string urlToLoad = urlLoadQueue.Dequeue();
                SetUpMessagingAPI();
                SetUpMessagePassingAPI();
                webViewPrefab.WebView.LoadUrl(urlToLoad);
            }

            if (htmlLoadQueue.Count > 0)
            {
                string htmlToLoad = htmlLoadQueue.Dequeue();
                SetUpMessagingAPI();
                SetUpMessagePassingAPI();
                webViewPrefab.WebView.LoadHtml(htmlToLoad);
            }
#endif
        }
    }
}