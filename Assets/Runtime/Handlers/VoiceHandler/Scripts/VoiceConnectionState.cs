// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Voice
{
    /// <summary>
    /// Connection state for the voice handler.
    /// </summary>
    public enum VoiceConnectionState
    {
        /// <summary>
        /// Not connected to a voice server.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Attempting to connect to a voice server.
        /// </summary>
        Connecting,

        /// <summary>
        /// Connected to a voice server.
        /// </summary>
        Connected,

        /// <summary>
        /// Attempting to reconnect after a connection loss.
        /// </summary>
        Reconnecting,

        /// <summary>
        /// An error occurred with the voice connection.
        /// </summary>
        Error
    }
}
