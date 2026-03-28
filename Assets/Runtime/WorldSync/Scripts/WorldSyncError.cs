// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.WorldSync
{
    /// <summary>
    /// Error codes for WorldSync operations.
    /// </summary>
    public enum WorldSyncErrorCode
    {
        /// <summary>
        /// Session not found.
        /// </summary>
        SessionNotFound,

        /// <summary>
        /// Session already exists.
        /// </summary>
        SessionExists,

        /// <summary>
        /// Entity not found.
        /// </summary>
        EntityNotFound,

        /// <summary>
        /// Entity already exists.
        /// </summary>
        EntityExists,

        /// <summary>
        /// Client not found.
        /// </summary>
        ClientNotFound,

        /// <summary>
        /// Client not in session.
        /// </summary>
        ClientNotInSession,

        /// <summary>
        /// Unauthorized operation.
        /// </summary>
        Unauthorized,

        /// <summary>
        /// Forbidden operation.
        /// </summary>
        Forbidden,

        /// <summary>
        /// Invalid payload format.
        /// </summary>
        InvalidPayload,

        /// <summary>
        /// Invalid entity type.
        /// </summary>
        InvalidEntityType,

        /// <summary>
        /// Invalid configuration.
        /// </summary>
        InvalidConfig,

        /// <summary>
        /// Invalid hierarchy operation.
        /// </summary>
        InvalidHierarchy,

        /// <summary>
        /// Invalid message format.
        /// </summary>
        InvalidMessage,

        /// <summary>
        /// Unsupported protocol version.
        /// </summary>
        UnsupportedProtocol,

        /// <summary>
        /// Invalid message chunk.
        /// </summary>
        ChunkInvalid,

        /// <summary>
        /// Payload too large.
        /// </summary>
        PayloadTooLarge,

        /// <summary>
        /// Connection failed.
        /// </summary>
        ConnectionFailed,

        /// <summary>
        /// Not connected to server.
        /// </summary>
        NotConnected,

        /// <summary>
        /// Connection timeout.
        /// </summary>
        ConnectionTimeout,

        /// <summary>
        /// Reconnection failed.
        /// </summary>
        ReconnectionFailed,

        /// <summary>
        /// Session expired.
        /// </summary>
        SessionExpired,

        /// <summary>
        /// Internal error.
        /// </summary>
        InternalError
    }

    /// <summary>
    /// Exception for WorldSync operations.
    /// </summary>
    public class WorldSyncException : Exception
    {
        /// <summary>
        /// Error code for this exception.
        /// </summary>
        public WorldSyncErrorCode Code { get; }

        /// <summary>
        /// Additional details about the error.
        /// </summary>
        public object Details { get; }

        /// <summary>
        /// Create a new WorldSyncException.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <param name="message">Error message.</param>
        /// <param name="details">Optional additional details.</param>
        public WorldSyncException(WorldSyncErrorCode code, string message, object details = null)
            : base(message)
        {
            Code = code;
            Details = details;
        }

        /// <summary>
        /// Create a new WorldSyncException with inner exception.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Inner exception.</param>
        /// <param name="details">Optional additional details.</param>
        public WorldSyncException(WorldSyncErrorCode code, string message, Exception innerException, object details = null)
            : base(message, innerException)
        {
            Code = code;
            Details = details;
        }

        /// <summary>
        /// Returns the string representation of this exception.
        /// </summary>
        public override string ToString()
        {
            return $"WorldSyncException [{Code}]: {Message}";
        }

        /// <summary>
        /// Create a SessionNotFound exception.
        /// </summary>
        public static WorldSyncException SessionNotFound(string sessionId)
        {
            return new WorldSyncException(
                WorldSyncErrorCode.SessionNotFound,
                $"Session not found: {sessionId}",
                new { sessionId }
            );
        }

        /// <summary>
        /// Create an EntityNotFound exception.
        /// </summary>
        public static WorldSyncException EntityNotFound(string entityId)
        {
            return new WorldSyncException(
                WorldSyncErrorCode.EntityNotFound,
                $"Entity not found: {entityId}",
                new { entityId }
            );
        }

        /// <summary>
        /// Create a NotConnected exception.
        /// </summary>
        public static WorldSyncException NotConnected()
        {
            return new WorldSyncException(
                WorldSyncErrorCode.NotConnected,
                "Not connected to WorldSync server"
            );
        }

        /// <summary>
        /// Create a ConnectionFailed exception.
        /// </summary>
        public static WorldSyncException ConnectionFailed(string reason)
        {
            return new WorldSyncException(
                WorldSyncErrorCode.ConnectionFailed,
                $"Connection failed: {reason}",
                new { reason }
            );
        }

        /// <summary>
        /// Create a ConnectionTimeout exception.
        /// </summary>
        public static WorldSyncException ConnectionTimeout()
        {
            return new WorldSyncException(
                WorldSyncErrorCode.ConnectionTimeout,
                "Connection timed out"
            );
        }

        /// <summary>
        /// Create a ReconnectionFailed exception.
        /// </summary>
        public static WorldSyncException ReconnectionFailed(int attempts)
        {
            return new WorldSyncException(
                WorldSyncErrorCode.ReconnectionFailed,
                $"Reconnection failed after {attempts} attempts",
                new { attempts }
            );
        }

        /// <summary>
        /// Create a SessionExpired exception.
        /// </summary>
        public static WorldSyncException SessionExpired(string sessionId)
        {
            return new WorldSyncException(
                WorldSyncErrorCode.SessionExpired,
                $"Session expired: {sessionId}",
                new { sessionId }
            );
        }
    }
}
