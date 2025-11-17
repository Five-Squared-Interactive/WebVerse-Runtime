using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace FiveSQD.WebVerse.WebInterface.HTTP
{
    public class HTTPRequestManager : BaseManager
    {
        public static HTTPRequestManager instance;

        public override void Initialize()
        {
            instance = this;

            base.Initialize();
        }

        public IEnumerator HandleRequest(UnityWebRequest request, Action<int, Dictionary<string, string>, byte[]> onFinished)
        {
            if (request != null)
            {
                yield return request.SendWebRequest();

                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Logging.LogError("[HTTPRequestManager->HandleRequest]" + request.uri + ":" + request.error);
                        onFinished.Invoke((int) request.responseCode, request.GetResponseHeaders(), null);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Logging.LogError("[HTTPRequestManager->HandleRequest]" + request.uri + ":" + request.error);
                        onFinished.Invoke((int) request.responseCode, request.GetResponseHeaders(), null);
                        break;
                    case UnityWebRequest.Result.Success:
                        onFinished.Invoke((int) request.responseCode, request.GetResponseHeaders(),
                            request.downloadHandler == null ? null : request.downloadHandler.data);
                        break;
                }
            }
        }

        public IEnumerator HandleRequest(UnityWebRequest request, Action<int, Dictionary<string, string>, Texture2D> onFinished)
        {
            if (request != null)
            {
                yield return request.SendWebRequest();

                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Logging.LogError("[HTTPRequestManager->HandleRequest]" + request.error);
                        onFinished.Invoke((int) request.responseCode, request.GetResponseHeaders(), null);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Logging.LogError("[HTTPRequestManager->HandleRequest]" + request.error);
                        onFinished.Invoke((int) request.responseCode, request.GetResponseHeaders(), null);
                        break;
                    case UnityWebRequest.Result.Success:
                        if (request.downloadHandler != null)
                        {
                            Texture2D tex = new Texture2D(2, 2);
                            tex.LoadImage(request.downloadHandler.data);
                            onFinished.Invoke((int) request.responseCode, request.GetResponseHeaders(), tex);
                        }
                        else
                        {
                            onFinished.Invoke((int) request.responseCode, request.GetResponseHeaders(), null);
                        }
                        break;
                }
            }
        }
    }
}