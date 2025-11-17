// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

#if USE_BESTHTTP
using Best.HTTP;
using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace FiveSQD.WebVerse.WebInterface.HTTP
{
    /// <summary>
    /// Class for an HTTP request.
    /// </summary>
    public class HTTPRequest
    {
        /// <summary>
        /// Enumeration for HTTP request methods.
        /// </summary>
        public enum HTTPMethod { Get = 0, Head = 1, Post = 2, Put = 3, Delete = 4,
            Patch = 5, Merge = 6, Options = 7, Connect = 8, Query = 9 }

        // Swapping out BestHTTP here as it seems to have broken in the latest release...
#if true
        /// <summary>
        /// Internal HTTP request object.
        /// </summary>
        private UnityWebRequest request;

        /// <summary>
        /// Action to perform upon completing the request and receiving byte array data.
        /// </summary>
        private Action<int, Dictionary<string, string>, byte[]> onFinishedBytes;

        /// <summary>
        /// Action to perform upon completing the request and receiving Texture2D data.
        /// </summary>
        private Action<int, Dictionary<string, string>, Texture2D> onFinishedTexture;

        /// <summary>
        /// Constructor for an HTTP request.
        /// </summary>
        /// <param name="uri">URI to send the request to.</param>
        /// <param name="method">Method to use in the request.</param>
        /// <param name="onFinished">Action to perform upon completing the request.</param>
        public HTTPRequest(string uri, HTTPMethod method, Action<int, Dictionary<string, string>, byte[]> onFinished, string data = null, string dataType = null)
        {
            switch (method)
            {
                case HTTPMethod.Get:
                    request = UnityWebRequest.Get(uri);
                    break;

                case HTTPMethod.Post:
                    request = UnityWebRequest.Post(uri, data, dataType);
                    break;

                case HTTPMethod.Head:
                    request = UnityWebRequest.Head(uri);
                    break;

                case HTTPMethod.Delete:
                    request = UnityWebRequest.Delete(uri);
                    break;

                default:
                    Logging.LogWarning("[HTTPRequest] Unsupported method.");
                    break;
            }

            onFinishedBytes = onFinished;
        }

        /// <summary>
        /// Constructor for an HTTP request.
        /// </summary>
        /// <param name="uri">URI to send the request to.</param>
        /// <param name="method">Method to use in the request.</param>
        /// <param name="onFinished">Action to perform upon completing the request.</param>
        public HTTPRequest(string uri, HTTPMethod method, Action<int, Dictionary<string, string>, Texture2D> onFinished)
        {
            switch (method)
            {
                case HTTPMethod.Get:
                    request = UnityWebRequest.Get(uri);
                    break;

                case HTTPMethod.Head:
                    request = UnityWebRequest.Head(uri);
                    break;

                case HTTPMethod.Delete:
                    request = UnityWebRequest.Delete(uri);
                    break;

                default:
                    Logging.LogWarning("[HTTPRequest] Unsupported method.");
                    break;
            }

            onFinishedTexture = onFinished;
        }

        /// <summary>
        /// Send the HTTP request.
        /// </summary>
        public void Send()
        {
            if (request != null)
            {
                if (HTTPRequestManager.instance == null)
                {
                    Logging.LogError("[HTTPRequest->Send] No request manager.");
                    return;
                }

                if (onFinishedBytes != null)
                {
                    HTTPRequestManager.instance.StartCoroutine(HTTPRequestManager.instance.HandleRequest(request, onFinishedBytes));
                }
                else if (onFinishedTexture != null)
                {
                    HTTPRequestManager.instance.StartCoroutine(HTTPRequestManager.instance.HandleRequest(request, onFinishedTexture));
                }
            }
        }

        
#else
        /// <summary>
        /// Internal HTTP request object.
        /// </summary>
        private Best.HTTP.HTTPRequest request;

        /// <summary>
        /// Constructor for an HTTP request.
        /// </summary>
        /// <param name="uri">URI to send the request to.</param>
        /// <param name="method">Method to use in the request.</param>
        /// <param name="onFinished">Action to perform upon completing the request.</param>
        public HTTPRequest(string uri, HTTPMethod method, Action<int, Dictionary<string, string>, byte[]> onFinished)
        {
            request = new Best.HTTP.HTTPRequest(new Uri(uri), (HTTPMethods) method, new OnRequestFinishedDelegate((req, resp) =>
            {
                Dictionary<string, string> respHeaders = new Dictionary<string, string>();
                foreach (KeyValuePair<string, List<string>> header in resp.Headers)
                {
                    string combinedValue = "";
                    foreach (string val in header.Value)
                    {
                        combinedValue = combinedValue + val;
                    }
                    if (combinedValue.EndsWith(','))
                    {
                        combinedValue = combinedValue.Substring(0, combinedValue.Length - 1);
                    }

                    respHeaders.Add(header.Key, combinedValue);
                }

                if (onFinished != null)
                {
                    if (resp == null)
                    {
                        onFinished.Invoke(-1, respHeaders, null);
                    }
                    else
                    {
                        onFinished.Invoke(resp.StatusCode, respHeaders, resp.Data);
                    }
                }
            }));
            request.DownloadSettings.ContentStreamMaxBuffered = 1073741824;
        }

        /// <summary>
        /// Constructor for an HTTP request.
        /// </summary>
        /// <param name="uri">URI to send the request to.</param>
        /// <param name="method">Method to use in the request.</param>
        /// <param name="onFinished">Action to perform upon completing the request.</param>
        public HTTPRequest(string uri, HTTPMethod method, Action<int, Dictionary<string, string>, Texture2D> onFinished)
        {
            request = new Best.HTTP.HTTPRequest(new Uri(uri), (HTTPMethods) method, new OnRequestFinishedDelegate((req, resp) =>
            {
                Dictionary<string, string> respHeaders = new Dictionary<string, string>();
                foreach (KeyValuePair<string, List<string>> header in resp.Headers)
                {
                    string combinedValue = "";
                    foreach (string val in header.Value)
                    {
                        combinedValue = combinedValue + val + ";";
                    }
                    if (combinedValue.EndsWith(';'))
                    {
                        combinedValue = combinedValue.Substring(0, combinedValue.Length - 1);
                    }

                    respHeaders.Add(header.Key, combinedValue);
                }

                if (onFinished != null)
                {
                    if (resp == null)
                    {
                        onFinished.Invoke(-1, respHeaders, null);
                    }
                    else
                    {
                        onFinished.Invoke(resp.StatusCode, respHeaders, resp.DataAsTexture2D);
                    }
                }
            }));
        }

        /// <summary>
        /// Send the HTTP request.
        /// </summary>
        public void Send()
        {
            if (request != null)
            {
                request.Send();
            }
        }
#endif
    }
}
#endif