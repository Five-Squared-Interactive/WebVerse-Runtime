// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a voxel block definition.
    /// </summary>
    public class VoxelBlockInfo
    {
        /// <summary>
        /// ID of the voxel block.
        /// </summary>
        public int id;

        /// <summary>
        /// Dictionary of IDs and voxel block subtypes.
        /// </summary>
        public Dictionary<int, VoxelBlockSubType> subTypes = new Dictionary<int, VoxelBlockSubType>();

        /// <summary>
        /// Constructor for voxel block info.
        /// </summary>
        /// <param name="id">ID of the voxel block.</param>
        public VoxelBlockInfo(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// Add a subtype.
        /// </summary>
        /// <param name="id">ID of the subtype.</param>
        /// <param name="invisible">Whether or not the subtype is invisible.</param>
        /// <param name="topTexture">Texture to use on top of the block.</param>
        /// <param name="bottomTexture">Texture to use at the bottom of the block.</param>
        /// <param name="leftTexture">Texture to use on the left of the block.</param>
        /// <param name="rightTexture">Texture to use on the right of the block.</param>
        /// <param name="frontTexture">Texture to use on the front of the block.</param>
        /// <param name="bottomTexture">Texture to use on the bottom of the block.</param>
        public void AddSubType(int id, bool invisible, string topTexture, string bottomTexture,
            string leftTexture, string rightTexture, string frontTexture, string backTexture)
        {
            subTypes[id] = new VoxelBlockSubType()
            {
                id = id,
                invisible = invisible,
                topTex = topTexture,
                bottomTex = bottomTexture,
                leftTex = leftTexture,
                rightTex = rightTexture,
                frontTex = frontTexture,
                backTex = backTexture
            };
        }
    }
}