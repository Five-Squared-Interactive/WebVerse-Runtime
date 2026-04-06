// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.WebInterface.SocketIO
{
    /// <summary>
    /// Represents the connection state of a Socket.IO client.
    /// </summary>
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Disconnecting
    }
}
