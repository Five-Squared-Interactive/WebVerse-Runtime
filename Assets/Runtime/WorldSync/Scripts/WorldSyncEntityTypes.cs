// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;

namespace FiveSQD.WebVerse.WorldSync
{
    /// <summary>
    /// Entity type constants and validation for the WorldSync protocol.
    /// Defines all 21 supported entity types and provides validation utilities.
    /// </summary>
    public static class WorldSyncEntityTypes
    {
        public const string Container = "container";
        public const string Mesh = "mesh";
        public const string Character = "character";
        public const string Light = "light";
        public const string Audio = "audio";
        public const string Terrain = "terrain";
        public const string HybridTerrain = "hybrid-terrain";
        public const string Voxel = "voxel";
        public const string WaterBody = "water-body";
        public const string WaterBlocker = "water-blocker";
        public const string Airplane = "airplane";
        public const string Automobile = "automobile";
        public const string Canvas = "canvas";
        public const string Text = "text";
        public const string Button = "button";
        public const string Image = "image";
        public const string Input = "input";
        public const string Dropdown = "dropdown";
        public const string Html = "html";
        public const string VoiceSpeaker = "voice-speaker";
        public const string VoiceInput = "voice-input";

        private static readonly HashSet<string> ValidTypes = new HashSet<string>
        {
            Container, Mesh, Character, Light, Audio, Terrain, HybridTerrain,
            Voxel, WaterBody, WaterBlocker, Airplane, Automobile, Canvas,
            Text, Button, Image, Input, Dropdown, Html, VoiceSpeaker, VoiceInput
        };

        /// <summary>
        /// Check if the given entity type string is a valid WorldSync entity type.
        /// </summary>
        public static bool IsValidEntityType(string type)
        {
            return !string.IsNullOrEmpty(type) && ValidTypes.Contains(type);
        }

        /// <summary>
        /// Returns the entity type if valid, or "container" as fallback for unknown types.
        /// Throws WorldSyncException for null/empty types.
        /// </summary>
        public static string GetFallbackType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new WorldSyncException(
                    WorldSyncErrorCode.InvalidEntityType,
                    "Entity type cannot be null or empty");
            }

            return ValidTypes.Contains(type) ? type : Container;
        }
    }
}
