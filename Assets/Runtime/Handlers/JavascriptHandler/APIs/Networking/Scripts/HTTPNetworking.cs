// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Networking
{
    /// <summary>
    /// Class for HTTP networking.
    /// </summary>
    public class HTTPNetworking
    {
        /// <summary>
        /// Fetch request options.
        /// </summary>
        public struct FetchRequestOptions
        {
            /// <summary>
            /// Body.
            /// </summary>
            public string body;
            
            /// <summary>
            /// Cache.
            /// </summary>
            public string cache;

            /// <summary>
            /// Credentials.
            /// </summary>
            public string credentials;

            /// <summary>
            /// Headers.
            /// </summary>
            public string[] headers;

            /// <summary>
            /// Keepalive.
            /// </summary>
            public bool keepalive;

            /// <summary>
            /// HTTP Method to use (GET, POST, PUT, DELETE, PATCH, MERGE, OPTIONS, CONNECT, QUERY).
            /// </summary>
            public string method;

            /// <summary>
            /// Mode.
            /// </summary>
            public string mode;

            /// <summary>
            /// Priority.
            /// </summary>
            public string priority;

            /// <summary>
            /// Redirect.
            /// </summary>
            public string redirect;

            /// <summary>
            /// Referrer.
            /// </summary>
            public string referrer;

            /// <summary>
            /// Referrer Policy.
            /// </summary>
            public string referrerPolicy;
        }

        /// <summary>
        /// Class for HTTP request.
        /// </summary>
        public class Request
        {
            /// <summary>
            /// Body.
            /// </summary>
            public string body;

            /// <summary>
            /// Cache.
            /// </summary>
            public string cache;

            /// <summary>
            /// Credentials.
            /// </summary>
            public string credentials;

            /// <summary>
            /// Headers.
            /// </summary>
            public string[] headers;

            /// <summary>
            /// Keepalive.
            /// </summary>
            public bool keepalive;

            /// <summary>
            /// Integrity.
            /// </summary>
            public string integrity;

            /// <summary>
            /// HTTP Method to use (GET, POST, PUT, DELETE, PATCH, MERGE, OPTIONS, CONNECT, QUERY).
            /// </summary>
            public string method;

            /// <summary>
            /// Mode.
            /// </summary>
            public string mode;

            /// <summary>
            /// Priority.
            /// </summary>
            public string priority;

            /// <summary>
            /// Redirect.
            /// </summary>
            public string redirect;

            /// <summary>
            /// Referrer.
            /// </summary>
            public string referrer;

            /// <summary>
            /// Referrer Policy.
            /// </summary>
            public string referrerPolicy;

            /// <summary>
            /// Resource URIs.
            /// </summary>
            public string resourceURI;

            /// <summary>
            /// Constructor for an HTTP Request.
            /// </summary>
            /// <param name="input">URI to use.</param>
            public Request(string input)
            {
                body = "";
                cache = "default";
                credentials = "same-origin";
                headers = new string[0];
                integrity = "";
                keepalive = false;
                method = "GET";
                mode = "cors";
                priority = "default";
                redirect = "follow";
                referrer = "about:client";
                referrerPolicy = "";
                resourceURI = input;
            }

            /// <summary>
            /// Constructor for an HTTP Request.
            /// </summary>
            /// <param name="input">URI to use.</param>
            /// <param name="options">Fetch Request Options.</param>
            public Request(string input, FetchRequestOptions options)
            {
                body = options.body;
                cache = options.cache;
                credentials = options.credentials;
                headers = options.headers;
                integrity = "";
                keepalive = options.keepalive;
                method = options.method;
                mode = options.mode;
                priority = options.priority;
                redirect = options.redirect;
                referrer = options.referrer;
                referrerPolicy = options.referrerPolicy;
                resourceURI = input;
            }
        }

        /// <summary>
        /// Class for an HTTP Response.
        /// </summary>
        public class Response
        {
            /// <summary>
            /// Status code.
            /// </summary>
            public int status;

            /// <summary>
            /// Status text.
            /// </summary>
            public string statusText;

            /// <summary>
            /// Data.
            /// </summary>
            public byte[] data;

            /// <summary>
            /// Constructor for an HTTP Response.
            /// </summary>
            /// <param name="status">Status code.</param>
            /// <param name="statusText">Status text.</param>
            /// <param name="data">Data.</param>
            public Response(int status, string statusText, byte[] data) 
            {
                this.status = status;
                this.statusText = statusText;
                this.data = data;
            }
        }

        /// <summary>
        /// Perform a Fetch.
        /// </summary>
        /// <param name="resource">URI of the resource to fetch.</param>
        /// <param name="onFinished">Logic to execute when the request has finished.</param>
        public static void Fetch(string resource, string onFinished)
        {
            if (string.IsNullOrEmpty(resource))
            {
                Logging.LogWarning("[HTTPNetworking:Fetch] Invalid Resource");
                return;
            }

            Fetch(new Request(resource), onFinished);
        }

        /// <summary>
        /// Perform a Fetch.
        /// </summary>
        /// <param name="resource">URI of the resource to fetch.</param>
        /// <param name="onFinished">Logic to execute when the request has finished.</param>
        public static void Post(string resource, string data, string dataType, string onFinished)
        {
            if (string.IsNullOrEmpty(resource))
            {
                Logging.LogWarning("[HTTPNetworking:Fetch] Invalid Resource");
                return;
            }

            Request req = new Request(resource);
            req.method = "POST";
            Fetch(req, onFinished, data, dataType);
        }

        /// <summary>
        /// Perform a Fetch.
        /// </summary>
        /// <param name="resource">URI of the resource to fetch.</param>
        /// <param name="options">Fetch Request Options.</param>
        /// <param name="onFinished">Logic to execute when the request has finished.</param>
        public static void Fetch(string resource, FetchRequestOptions options, string onFinished)
        {
            if (string.IsNullOrEmpty(resource))
            {
                Logging.LogWarning("[HTTPNetworking:Fetch] Invalid Resource");
                return;
            }

            Fetch(new Request(resource, options), onFinished);
        }

        /// <summary>
        /// Perform a Fetch.
        /// </summary>
        /// <param name="request">Request to fetch.</param>
        /// <param name="onFinished">Logic to execute when the request has finished.</param>
        public static void Fetch(Request request, string onFinished, string data = null, string dataType = null)
        {
#if USE_WEBINTERFACE
            WebInterface.HTTP.HTTPRequest.HTTPMethod method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Get;
            switch (request.method.ToUpper())
            {
                case "GET":
                    method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Get;
                    break;

                case "POST":
                    method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Post;
                    break;

                case "PUT":
                    method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Put;
                    break;

                case "DELETE":
                    method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Delete;
                    break;

                case "PATCH":
                    method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Patch;
                    break;

                case "MERGE":
                    method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Merge;
                    break;

                case "OPTIONS":
                    method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Options;
                    break;

                case "CONNECT":
                    method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Connect;
                    break;

                case "QUERY":
                    method = WebInterface.HTTP.HTTPRequest.HTTPMethod.Query;
                    break;

                default:
                    Logging.LogWarning("[HTTPNetworking:Fetch] Invalid Method " + request.method);
                    break;
            }

            Action<int, Dictionary<string, string>, byte[]> onFinishedAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
            {
                Response resp = new Response(code, "", data);
                if (!string.IsNullOrEmpty(onFinished))
                {
                    string dataToReturn = "";
                    if (resp != null)
                    {
                        if (resp.data != null)
                        {
                            dataToReturn = System.Text.Encoding.UTF8.GetString(resp.data);
                        }
                    }
                    WebVerseRuntime.Instance.timeHandler.CallAsynchronously(onFinished, new object[] { dataToReturn });
                    //WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onFinished, new object[] { dataToReturn });
                }
            });

            WebInterface.HTTP.HTTPRequest httpReq = new WebInterface.HTTP.HTTPRequest(request.resourceURI, method, onFinishedAction, data, dataType);
            httpReq.Send();
#endif
        }
    }
}