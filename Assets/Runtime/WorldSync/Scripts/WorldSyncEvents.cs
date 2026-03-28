// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.WorldSync
{
    /// <summary>
    /// Event arguments for connection state changes.
    /// </summary>
    public class ConnectionStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Previous connection state.
        /// </summary>
        public ConnectionState PreviousState { get; }

        /// <summary>
        /// New connection state.
        /// </summary>
        public ConnectionState NewState { get; }

        public ConnectionStateChangedEventArgs(ConnectionState previousState, ConnectionState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    /// <summary>
    /// Event arguments for reconnection attempts.
    /// </summary>
    public class ReconnectingEventArgs : EventArgs
    {
        /// <summary>
        /// Current attempt number (1-based).
        /// </summary>
        public int Attempt { get; }

        /// <summary>
        /// Maximum number of attempts.
        /// </summary>
        public int MaxAttempts { get; }

        /// <summary>
        /// Delay before this attempt in milliseconds.
        /// </summary>
        public int DelayMs { get; }

        public ReconnectingEventArgs(int attempt, int maxAttempts, int delayMs)
        {
            Attempt = attempt;
            MaxAttempts = maxAttempts;
            DelayMs = delayMs;
        }
    }

    /// <summary>
    /// Event arguments for entity creation.
    /// </summary>
    public class EntityCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// The created entity.
        /// </summary>
        public SyncEntity Entity { get; }

        /// <summary>
        /// Correlation ID from the request.
        /// </summary>
        public string CorrelationId { get; }

        public EntityCreatedEventArgs(SyncEntity entity, string correlationId = null)
        {
            Entity = entity;
            CorrelationId = correlationId;
        }
    }

    /// <summary>
    /// Event arguments for entity deletion.
    /// </summary>
    public class EntityDeletedEventArgs : EventArgs
    {
        /// <summary>
        /// ID of the deleted entity.
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        /// IDs of orphaned children (if any).
        /// </summary>
        public string[] OrphanedChildren { get; }

        public EntityDeletedEventArgs(string entityId, string[] orphanedChildren = null)
        {
            EntityId = entityId;
            OrphanedChildren = orphanedChildren ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Event arguments for entity transform updates.
    /// </summary>
    public class EntityTransformEventArgs : EventArgs
    {
        /// <summary>
        /// ID of the entity.
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        /// Type of transform update (position, rotation, scale).
        /// </summary>
        public string TransformType { get; }

        /// <summary>
        /// New position (if updated).
        /// </summary>
        public SyncVector3? Position { get; }

        /// <summary>
        /// New rotation (if updated).
        /// </summary>
        public SyncQuaternion? Rotation { get; }

        /// <summary>
        /// New scale (if updated).
        /// </summary>
        public SyncVector3? Scale { get; }

        public EntityTransformEventArgs(string entityId, string transformType,
            SyncVector3? position = null, SyncQuaternion? rotation = null, SyncVector3? scale = null)
        {
            EntityId = entityId;
            TransformType = transformType;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }

    /// <summary>
    /// Event arguments for client joining session.
    /// </summary>
    public class ClientJoinedEventArgs : EventArgs
    {
        /// <summary>
        /// The client that joined.
        /// </summary>
        public SyncClient Client { get; }

        public ClientJoinedEventArgs(SyncClient client)
        {
            Client = client;
        }
    }

    /// <summary>
    /// Event arguments for client leaving session.
    /// </summary>
    public class ClientLeftEventArgs : EventArgs
    {
        /// <summary>
        /// ID of the client that left.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Reason for leaving (graceful, timeout, disconnected).
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Number of entities removed.
        /// </summary>
        public int EntitiesRemoved { get; }

        /// <summary>
        /// IDs of removed entities.
        /// </summary>
        public string[] RemovedEntityIds { get; }

        public ClientLeftEventArgs(string clientId, string reason, int entitiesRemoved = 0, string[] removedEntityIds = null)
        {
            ClientId = clientId;
            Reason = reason;
            EntitiesRemoved = entitiesRemoved;
            RemovedEntityIds = removedEntityIds ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Event arguments for environment updates.
    /// </summary>
    public class EnvironmentUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Property that was updated.
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// New environment state.
        /// </summary>
        public EnvironmentState Environment { get; }

        public EnvironmentUpdatedEventArgs(string property, EnvironmentState environment)
        {
            Property = property;
            Environment = environment;
        }
    }

    /// <summary>
    /// Event arguments for custom messages.
    /// </summary>
    public class CustomMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Message type.
        /// </summary>
        public string MessageType { get; }

        /// <summary>
        /// Sender client ID.
        /// </summary>
        public string SenderId { get; }

        /// <summary>
        /// Message payload.
        /// </summary>
        public object Payload { get; }

        public CustomMessageEventArgs(string messageType, string senderId, object payload)
        {
            MessageType = messageType;
            SenderId = senderId;
            Payload = payload;
        }
    }

    /// <summary>
    /// Event arguments for session destruction.
    /// </summary>
    public class SessionDestroyedEventArgs : EventArgs
    {
        /// <summary>
        /// Session ID that was destroyed.
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Reason for destruction.
        /// </summary>
        public string Reason { get; }

        public SessionDestroyedEventArgs(string sessionId, string reason)
        {
            SessionId = sessionId;
            Reason = reason;
        }
    }
}
