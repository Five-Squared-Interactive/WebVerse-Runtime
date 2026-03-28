// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Voice
{
    /// <summary>
    /// Exception for voice-related errors.
    /// </summary>
    public class VoiceException : Exception
    {
        /// <summary>
        /// The error code associated with this exception.
        /// </summary>
        public VoiceErrorCode ErrorCode { get; }

        /// <summary>
        /// Create a new VoiceException.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        public VoiceException(VoiceErrorCode code, string message) : base(message)
        {
            ErrorCode = code;
        }

        /// <summary>
        /// Create a new VoiceException with an inner exception.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public VoiceException(VoiceErrorCode code, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = code;
        }
    }
}
