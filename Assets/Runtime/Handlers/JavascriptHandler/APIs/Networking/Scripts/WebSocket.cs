// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Networking
{
#if USE_WEBINTERFACE
    /// <summary>
    /// Class for WebSocket Events.
    /// </summary>
    public class WebSocketEvent
    {
        /// <summary>
        /// Struct for WebSocket Data.
        /// </summary>
        public struct Data
        {
            /// <summary>
            /// Binary Data.
            /// </summary>
            public byte[] binary;

            /// <summary>
            /// String Data.
            /// </summary>
            public string message;
        }

        /// <summary>
        /// WebSocket for the Event.
        /// </summary>
        public WebSocket webSocket;

        /// <summary>
        /// Event Code.
        /// </summary>
        public int code;

        /// <summary>
        /// Details of the Event.
        /// </summary>
        public string details;

        /// <summary>
        /// Data for the Event.
        /// </summary>
        public Data data;

        /// <summary>
        /// Constructor for a WebSocket Event.
        /// </summary>
        /// <param name="webSocket">WebSocket for the event.</param>
        /// <param name="message">Message for the event.</param>
        /// <param name="code">Event code.</param>
        /// <param name="details">Details of the event.</param>
        public WebSocketEvent(WebSocket webSocket, string message, int code, string details)
        {
            this.webSocket = webSocket;
            data = new Data()
            {
                message = message
            };
            this.code = code;
            this.details = details;
        }

        /// <summary>
        /// Constructor for a WebSocket Event.
        /// </summary>
        /// <param name="webSocket">WebSocket for the event.</param>
        /// <param name="binary">Binary data for the event.</param>
        /// <param name="code">Event code.</param>
        /// <param name="details">Details of the event.</param>
        public WebSocketEvent(WebSocket webSocket, byte[] binary, int code, string details)
        {
            this.webSocket = webSocket;
            data = new Data()
            {
                binary = binary
            };
            this.code = code;
            this.details = details;
        }
    }

    /// <summary>
    /// Class for a WebSocket.
    /// </summary>
    public class WebSocket
    {
        /// <summary>
        /// Reference to the internal WebSocket.
        /// </summary>
        private WebInterface.WebSocket.WebSocket internalWebSocket;

        /// <summary>
        /// Constructor for a WebSocket.
        /// </summary>
        /// <param name="uri">URI for the WebSocket.</param>
        public WebSocket(string uri)
        {
            internalWebSocket = new WebInterface.WebSocket.WebSocket(uri, null, null, null, null, null);
        }

        /// <summary>
        /// Add an event listener to the WebSocket.
        /// </summary>
        /// <param name="type">Type of event ("open", "close", "binary", "message", or "error").</param>
        /// <param name="functionToCall">Logic to execute when event occurs.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool AddEventListener(string type, string functionToCall)
        {
            if (internalWebSocket == null)
            {
                Logging.LogWarning("[WebSocket:AddEventListener] WebSocket not set up.");
                return false;
            }

            WebSocketEvent evt = null;
            switch (type)
            {
                case "open":
                    System.Action<WebInterface.WebSocket.WebSocket> onOpenAction = string.IsNullOrEmpty(functionToCall) ?
                        null : (ws) =>
                    {
                        evt = new WebSocketEvent(this, (string) null, 0, null);
                        WebVerseRuntime.Instance.javascriptHandler.Run(functionToCall.Replace("?", "evt"));
                    };
                    return internalWebSocket.AddOnOpenAction(onOpenAction);

                case "close":
                    System.Action<WebInterface.WebSocket.WebSocket, ushort, string> onClosedAction = string.IsNullOrEmpty(functionToCall) ?
                        null : (ws, code, msg) =>
                        {
                            evt = new WebSocketEvent(this, (string) null, code, msg);
                            WebVerseRuntime.Instance.javascriptHandler.Run(functionToCall.Replace("?", "evt"));
                        };
                    return internalWebSocket.AddOnClosedAction(onClosedAction);

                case "binary":
                    System.Action<WebInterface.WebSocket.WebSocket, byte[]> onBinaryAction = string.IsNullOrEmpty(functionToCall) ?
                        null : (ws, data) =>
                        {
                            evt = new WebSocketEvent(this, data, 0, null);
                            WebVerseRuntime.Instance.javascriptHandler.Run(functionToCall.Replace("?", "evt"));
                        };
                    return internalWebSocket.AddOnBinaryAction(onBinaryAction);

                case "message":
                    System.Action<WebInterface.WebSocket.WebSocket, string> onMessageAction = string.IsNullOrEmpty(functionToCall) ?
                        null : (ws, msg) =>
                        {
                            evt = new WebSocketEvent(this, msg, 0, null);
                            WebVerseRuntime.Instance.javascriptHandler.Run(functionToCall.Replace("?", "evt"));
                        };
                    return internalWebSocket.AddOnMessageAction(onMessageAction);

                case "error":
                    System.Action<WebInterface.WebSocket.WebSocket, string> onErrorAction = string.IsNullOrEmpty(functionToCall) ?
                        null : (ws, msg) =>
                        {
                            evt = new WebSocketEvent(this, (string) null, -1, msg);
                            WebVerseRuntime.Instance.javascriptHandler.Run(functionToCall.Replace("?", "evt"));
                        };
                    return internalWebSocket.AddOnErrorAction(onErrorAction);

                default:
                    Logging.LogWarning("[WebSocket:AddEventListener] Unknown event type " + type);
                    return false;
            }
        }

        /// <summary>
        /// Send a message on the WebSocket.
        /// </summary>
        /// <param name="dataToSend">Message to send.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Send(string dataToSend)
        {
            if (internalWebSocket == null)
            {
                Logging.LogWarning("[WebSocket:Send] WebSocket not set up.");
                return false;
            }

            if (string.IsNullOrEmpty(dataToSend))
            {
                Logging.LogWarning("[WebSocket:Send] No data to send.");
                return false;
            }

            internalWebSocket.Send(dataToSend);
            return true;
        }

        /// <summary>
        /// Send a message on the WebSocket.
        /// </summary>
        /// <param name="dataToSend">Data to send.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Send(byte[] dataToSend)
        {
            if (internalWebSocket == null)
            {
                Logging.LogWarning("[WebSocket:Send] WebSocket not set up.");
                return false;
            }

            if (dataToSend == null || dataToSend.Length == 0)
            {
                Logging.LogWarning("[WebSocket:Send] No data to send.");
            }

            internalWebSocket.Send(dataToSend);
            return true;
        }
    }
#endif
}