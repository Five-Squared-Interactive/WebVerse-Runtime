// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Core
{
    /// <summary>
    /// Static constants for all event names in the World API event system.
    /// Provides autocomplete, validation, and single source of truth for event names.
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// World lifecycle events.
        /// </summary>
        public static class World
        {
            /// <summary>
            /// Fires when a world begins loading.
            /// </summary>
            public const string Load = "load";

            /// <summary>
            /// Fires when a world has fully loaded and is interactive.
            /// </summary>
            public const string Ready = "ready";

            /// <summary>
            /// Fires when a world loading error occurs.
            /// </summary>
            public const string Error = "error";
        }

        /// <summary>
        /// Entity lifecycle events.
        /// </summary>
        public static class Entity
        {
            /// <summary>
            /// Fires when an entity is created and registered.
            /// </summary>
            public const string Spawn = "spawn";

            /// <summary>
            /// Fires before an entity is removed.
            /// </summary>
            public const string Destroy = "destroy";

            /// <summary>
            /// Fires when an entity's position changes via SetPosition.
            /// </summary>
            public const string Position = "position";

            /// <summary>
            /// Fires when an entity's rotation changes via SetRotation.
            /// </summary>
            public const string Rotation = "rotation";

            /// <summary>
            /// Fires when an entity's scale changes via SetScale.
            /// </summary>
            public const string Scale = "scale";

            /// <summary>
            /// Fires when an entity's visibility changes via SetVisibility.
            /// </summary>
            public const string Visibility = "visibility";
        }

        /// <summary>
        /// Collision events for entities with physics colliders.
        /// </summary>
        public static class Collision
        {
            /// <summary>
            /// Fires when another entity enters the collision zone.
            /// </summary>
            public const string Enter = "collision:enter";

            /// <summary>
            /// Fires when another entity exits the collision zone.
            /// </summary>
            public const string Exit = "collision:exit";
        }

        /// <summary>
        /// Pre-built validation set for O(1) event name lookup.
        /// Built at static initialization with zero per-frame cost.
        /// </summary>
        private static readonly HashSet<string> _validEvents = new HashSet<string>
        {
            World.Load, World.Ready, World.Error,
            Entity.Spawn, Entity.Destroy,
            Entity.Position, Entity.Rotation, Entity.Scale, Entity.Visibility,
            Collision.Enter, Collision.Exit
        };

        /// <summary>
        /// Check if an event name is a recognized event constant.
        /// Accepts object type for safe Jint marshalling — non-string values return false.
        /// </summary>
        /// <param name="eventName">The event name to validate. Non-string values return false.</param>
        /// <returns>True if the event name is a recognized string constant, false otherwise.</returns>
        public static bool IsValid(object eventName)
        {
            return eventName is string s && !string.IsNullOrEmpty(s) && _validEvents.Contains(s);
        }
    }
}
