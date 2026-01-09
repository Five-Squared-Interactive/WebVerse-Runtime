// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI
{
    /// <summary>
    /// Mode for selecting spawn points when loading a world.
    /// </summary>
    public enum SpawnPointSelectionMode
    {
        /// <summary>
        /// Use the first spawn point found.
        /// </summary>
        First,

        /// <summary>
        /// Randomly select a spawn point.
        /// </summary>
        Random,

        /// <summary>
        /// Select spawn point based on team tag.
        /// </summary>
        TeamBased,

        /// <summary>
        /// Select spawn point by name/title.
        /// </summary>
        Named
    }

    /// <summary>
    /// Runtime settings for the OMI Handler.
    /// These are configured programmatically by the WebVerse application, not by end users.
    /// </summary>
    [Serializable]
    public class OMIHandlerSettings
    {
        // Extension processing flags (application-level defaults)
        public bool enablePhysics = true;
        public bool enableSpawnPoints = true;
        public bool enableSeats = true;
        public bool enableLinks = true;

        // Physics configuration
        public int physicsLayer = -1;
        public PhysicsMaterial defaultPhysicsMaterial;

        // Spawn point configuration
        public SpawnPointSelectionMode spawnMode = SpawnPointSelectionMode.First;
        public string spawnTeam = "";
        public string spawnPointName = "";
        public bool autoApplySpawnPoint = true;

        // Link security settings (application policy)
        public bool allowExternalLinks = true;
        public bool allowWorldLinks = true;

        // Debug (for development builds)
        public bool verboseLogging = false;

        /// <summary>
        /// Creates settings with sensible defaults for runtime use.
        /// </summary>
        public static OMIHandlerSettings CreateDefault()
        {
            return new OMIHandlerSettings();
        }
    }
}
