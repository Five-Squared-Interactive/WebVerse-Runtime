// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

#if USE_BESTHTTP
using Best.WebSockets;
using FiveSQD.WebVerse.Utilities;
using System;

namespace FiveSQD.WebVerse.WebInterface.WebSocket
{
    /// <summary>
    /// Class for a WebSocket.
    /// </summary>
    public class WebSocket
    {
        /// <summary>
        /// Whether or not the WebSocket is open.
        /// </summary>
        public bool isOpen
        {
            get
            {
                if (webSocket == null)
                {
                    return false;
                }
                return webSocket.IsOpen;
            }
        }

        /// <summary>
        /// Action invoked when WebSocket is opened.
        /// </summary>
        public Action<WebSocket> onOpen
        {
            set
            {
                if (webSocket == null)
                {
                    Logging.LogWarning("[WebSocket->onOpen] WebSocket not set up.");
                    return;
                }

                webSocket.OnOpen = new Best.WebSockets.Implementations.OnWebSocketOpenDelegate((ws) =>
                {
                    value.Invoke(this);
                });
            }
        }

        /// <summary>
        /// Action invoked when WebSocket is closed.
        /// </summary>
        public Action<WebSocket, ushort, string> onClosed
        {
            set
            {
                if (webSocket == null)
                {
                    Logging.LogWarning("[WebSocket->onClosed] WebSocket not set up.");
                    return;
                }

                webSocket.OnClosed += new Best.WebSockets.Implementations.OnWebSocketClosedDelegate((ws, code, msg) =>
                {
                    value.Invoke(this, (ushort) code, msg);
                });
            }
        }

        /// <summary>
        /// Action invoked when binary is received.
        /// </summary>
        public Action<WebSocket, byte[]> onBinary
        {
            set
            {
                if (webSocket == null)
                {
                    Logging.LogWarning("[WebSocket->onBinary] WebSocket not set up.");
                    return;
                }

                webSocket.OnBinary = new Best.WebSockets.Implementations.OnWebSocketBinaryNoAllocDelegate((ws, data) =>
                {
                    byte[] buf = new byte[data.Count];
                    data.CopyTo(buf);
                    value.Invoke(this, buf);
                });
            }
        }

        /// <summary>
        /// Action invoked when message is received.
        /// </summary>
        public Action<WebSocket, string> onMessage
        {
            set
            {
                if (webSocket == null)
                {
                    Logging.LogWarning("[WebSocket->onMessage] WebSocket not set up.");
                    return;
                }

                webSocket.OnMessage = new Best.WebSockets.Implementations.OnWebSocketMessageDelegate((ws, msg) =>
                {
                    value.Invoke(this, msg);
                });
            }
        }

        /// <summary>
        /// Action invoked when error occurs.
        /// </summary>
        public Action<WebSocket, string> onError
        {
            set
            {
                if (webSocket == null)
                {
                    Logging.LogWarning("[WebSocket->onError] WebSocket not set up.");
                    return;
                }

                webSocket.OnClosed += new Best.WebSockets.Implementations.OnWebSocketClosedDelegate((ws, code, msg) =>
                {
                    if (code != WebSocketStatusCodes.NormalClosure)
                    {
                        value.Invoke(this, msg);
                    }
                });
            }
        }

        /// <summary>
        /// Internal WebSocket reference.
        /// </summary>
        private Best.WebSockets.WebSocket webSocket;

        /// <summary>
        /// Constructor for a WebSocket.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <param name="onOpen">Action to invoke upon open.</param>
        /// <param name="onClose">Action to invoke upon close.</param>
        /// <param name="onBinary">Action to invoke upon receiving binary.</param>
        /// <param name="onMessage">Action to invoke upon receiving message.</param>
        /// <param name="onError">Action to invoke upon error.</param>
        public WebSocket(string uri, Action<WebSocket> onOpen, Action<WebSocket, ushort, string> onClosed,
            Action<WebSocket, byte[]> onBinary, Action<WebSocket, string> onMessage, Action<WebSocket, string> onError)
        {
            webSocket = new Best.WebSockets.WebSocket(new Uri(uri));

            webSocket.OnOpen = new Best.WebSockets.Implementations.OnWebSocketOpenDelegate((ws) =>
            {
                onOpen.Invoke(this);
            });

            webSocket.OnClosed = new Best.WebSockets.Implementations.OnWebSocketClosedDelegate((ws, code, msg) =>
            {
                if (code == WebSocketStatusCodes.NormalClosure)
                {
                    onClosed.Invoke(this, (ushort) code, msg);
                }
                else
                {
                    onError.Invoke(this, msg);
                }
            });

            webSocket.OnBinary = new Best.WebSockets.Implementations.OnWebSocketBinaryNoAllocDelegate((ws, data) =>
            {
                byte[] buf = new byte[data.Count];
                data.CopyTo(buf);
                onBinary.Invoke(this, buf);
            });

            webSocket.OnMessage = new Best.WebSockets.Implementations.OnWebSocketMessageDelegate((ws, msg) =>
            {
                onMessage.Invoke(this, msg);
            });
        }

        /// <summary>
        /// Add an on open action.
        /// </summary>
        /// <param name="action">Action to add.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool AddOnOpenAction(Action<WebSocket> action)
        {
            if (webSocket == null)
            {
                Logging.LogWarning("[WebSocket->AddOnOpenAction] WebSocket not set up.");
                return false;
            }

            webSocket.OnOpen += new Best.WebSockets.Implementations.OnWebSocketOpenDelegate((ws) =>
            {
                action.Invoke(this);
            });

            return true;
        }

        /// <summary>
        /// Add an on close action.
        /// </summary>
        /// <param name="action">Action to add.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool AddOnClosedAction(Action<WebSocket, ushort, string> action)
        {
            if (webSocket == null)
            {
                Logging.LogWarning("[WebSocket->AddOnClosedAction] WebSocket not set up.");
                return false;
            }

            webSocket.OnClosed += new Best.WebSockets.Implementations.OnWebSocketClosedDelegate((ws, code, msg) =>
            {
                if (code == WebSocketStatusCodes.NormalClosure)
                {
                    action.Invoke(this, (ushort) code, msg);
                }
            });

            return true;
        }

        /// <summary>
        /// Add an on binary action.
        /// </summary>
        /// <param name="action">Action to add.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool AddOnBinaryAction(Action<WebSocket, byte[]> action)
        {
            if (webSocket == null)
            {
                Logging.LogWarning("[WebSocket->AddOnBinaryAction] WebSocket not set up.");
                return false;
            }

            webSocket.OnBinary += new Best.WebSockets.Implementations.OnWebSocketBinaryNoAllocDelegate((ws, data) =>
            {
                byte[] buf = new byte[data.Count];
                data.CopyTo(buf);
                action.Invoke(this, buf);
            });

            return true;
        }

        /// <summary>
        /// Add an on message action.
        /// </summary>
        /// <param name="action">Action to add.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool AddOnMessageAction(Action<WebSocket, string> action)
        {
            if (webSocket == null)
            {
                Logging.LogWarning("[WebSocket->AddOnMessageAction] WebSocket not set up.");
                return false;
            }

            webSocket.OnMessage += new Best.WebSockets.Implementations.OnWebSocketMessageDelegate((ws, msg) =>
            {
                action.Invoke(this, msg);
            });

            return true;
        }

        /// <summary>
        /// Add an on error action.
        /// </summary>
        /// <param name="action">Action to add.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool AddOnErrorAction(Action<WebSocket, string> action)
        {
            if (webSocket == null)
            {
                Logging.LogWarning("[WebSocket->AddOnErrorAction] WebSocket not set up.");
                return false;
            }

            webSocket.OnClosed += new Best.WebSockets.Implementations.OnWebSocketClosedDelegate((ws, code, msg) =>
            {
                if (code != WebSocketStatusCodes.NormalClosure)
                {
                    action.Invoke(this, msg);
                }
            });

            return true;
        }

        /// <summary>
        /// Open the WebSocket.
        /// </summary>
        public void Open()
        {
            if (webSocket == null)
            {
                Logging.LogWarning("[WebSocket->Open] WebSocket not initialized.");
                return;
            }

            if (webSocket.IsOpen)
            {
                Logging.LogWarning("[WebSocket->Open] WebSocket already open.");
                return;
            }

            webSocket.Open();
        }

        /// <summary>
        /// Close the WebSocket.
        /// </summary>
        public void Close()
        {
            if (webSocket == null)
            {
                Logging.LogWarning("[WebSocket->Close] WebSocket not initialized.");
                return;
            }

            if (!webSocket.IsOpen)
            {
                Logging.LogWarning("[WebSocket->Close] WebSocket not open.");
            }

            webSocket.Close();
        }

        /// <summary>
        /// Send a message on the WebSocket.
        /// </summary>
        /// <param name="dataToSend">Data to send.</param>
        public void Send(string dataToSend)
        {
            if (webSocket == null)
            {
                Logging.LogWarning("[WebSocket->Send] WebSocket not initialized.");
                return;
            }

            webSocket.Send(dataToSend);
        }

        /// <summary>
        /// Send a message on the WebSocket.
        /// </summary>
        /// <param name="dataToSend">Data to send.</param>
        public void Send(byte[] dataToSend)
        {
            if (webSocket == null)
            {
                Logging.LogWarning("[WebSocket->Send] WebSocket not initialized.");
                return;
            }

            webSocket.Send(dataToSend);
        }
    }
}
#endif