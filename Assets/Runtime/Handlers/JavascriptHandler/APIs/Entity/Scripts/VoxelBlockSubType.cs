// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Struct for voxel block subtype.
    /// </summary>
    public struct VoxelBlockSubType
    {
        /// <summary>
        /// ID of the block.
        /// </summary>
        public int id;

        /// <summary>
        /// Whether or not the block subtype is invisible.
        /// </summary>
        public bool invisible;

        /// <summary>
        /// Front texture for the block subtype.
        /// </summary>
        public string frontTex;

        /// <summary>
        /// Back texture for the block subtype.
        /// </summary>
        public string backTex;

        /// <summary>
        /// Top texture for the block subtype.
        /// </summary>
        public string topTex;

        /// <summary>
        /// Bottom texture for the block subtype.
        /// </summary>
        public string bottomTex;

        /// <summary>
        /// Left texture for the block subtype.
        /// </summary>
        public string leftTex;

        /// <summary>
        /// Right texture for the block subtype.
        /// </summary>
        public string rightTex;
    }
}