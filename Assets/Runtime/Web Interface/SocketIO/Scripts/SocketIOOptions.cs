// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;

namespace FiveSQD.WebVerse.WebInterface.SocketIO
{
    /// <summary>
    /// Configuration options for a Socket.IO connection.
    /// </summary>
    public class SocketIOOptions
    {
        /// <summary>
        /// Authentication token string.
        /// </summary>
        public string auth;

        /// <summary>
        /// Custom HTTP headers to send with the connection.
        /// </summary>
        public Dictionary<string, string> headers;

        /// <summary>
        /// Query parameters appended to the connection URL.
        /// </summary>
        public Dictionary<string, string> query;

        /// <summary>
        /// Transport type: "websocket" or "polling".
        /// </summary>
        public string transport = "polling";

        /// <summary>
        /// Whether to automatically reconnect on disconnect.
        /// </summary>
        public bool reconnection = true;

        /// <summary>
        /// Maximum number of reconnection attempts.
        /// </summary>
        public int reconnectionAttempts = 5;

        /// <summary>
        /// Initial reconnection delay in milliseconds.
        /// </summary>
        public int reconnectionDelay = 1000;

        /// <summary>
        /// Maximum reconnection delay in milliseconds.
        /// </summary>
        public int reconnectionDelayMax = 5000;

        /// <summary>
        /// Connection timeout in milliseconds.
        /// </summary>
        public int timeout = 20000;

        /// <summary>
        /// Acknowledgement timeout in milliseconds.
        /// </summary>
        public int ackTimeout = 10000;

        /// <summary>
        /// Maximum number of messages to queue during disconnect.
        /// </summary>
        public int queueSize = 100;
    }
}
