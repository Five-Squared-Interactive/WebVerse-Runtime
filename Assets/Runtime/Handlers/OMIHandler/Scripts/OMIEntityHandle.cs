// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.OMI
{
    /// <summary>
    /// Handle for tracking entities created from OMI/glTF loading.
    /// </summary>
    public class OMIEntityHandle
    {
        /// <summary>
        /// Unique identifier for the entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tag/name for the entity (from glTF node name).
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Whether the entity was successfully created.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The glTF node index this entity corresponds to.
        /// </summary>
        public int NodeIndex { get; set; }

        /// <summary>
        /// Creates a new entity handle.
        /// </summary>
        public OMIEntityHandle()
        {
            Id = Guid.NewGuid();
            Success = false;
            NodeIndex = -1;
        }

        /// <summary>
        /// Creates a new entity handle with the specified ID.
        /// </summary>
        /// <param name="id">The ID to use.</param>
        public OMIEntityHandle(Guid id)
        {
            Id = id;
            Success = false;
            NodeIndex = -1;
        }
    }
}
