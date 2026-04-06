// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Voice
{
    /// <summary>
    /// Error codes for voice-related operations.
    /// </summary>
    public enum VoiceErrorCode
    {
        /// <summary>
        /// Invalid voice configuration provided.
        /// </summary>
        VOICE_INVALID_CONFIG,

        /// <summary>
        /// Failed to establish connection to voice server.
        /// </summary>
        VOICE_CONNECTION_FAILED,

        /// <summary>
        /// Lost connection to voice server.
        /// </summary>
        VOICE_CONNECTION_LOST,

        /// <summary>
        /// Permission denied for microphone or voice features.
        /// </summary>
        VOICE_PERMISSION_DENIED,

        /// <summary>
        /// Error encoding audio data.
        /// </summary>
        VOICE_ENCODING_ERROR,

        /// <summary>
        /// Error decoding audio data.
        /// </summary>
        VOICE_DECODING_ERROR
    }
}
