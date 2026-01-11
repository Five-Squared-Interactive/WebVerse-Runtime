// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.StraightFour.Entity.Voxels
{
    /// <summary>
    /// Class for block subtype.
    /// </summary>
    public class BlockSubtype
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
        public Texture2D frontTex;

        /// <summary>
        /// Back texture for the block subtype.
        /// </summary>
        public Texture2D backTex;

        /// <summary>
        /// Top texture for the block subtype.
        /// </summary>
        public Texture2D topTex;

        /// <summary>
        /// Bottom texture for the block subtype.
        /// </summary>
        public Texture2D bottomTex;

        /// <summary>
        /// Left texture for the block subtype.
        /// </summary>
        public Texture2D leftTex;

        /// <summary>
        /// Right texture for the block subtype.
        /// </summary>
        public Texture2D rightTex;
    }

    /// <summary>
    /// Class for block information.
    /// </summary>
    public class BlockInfo
    {
        /// <summary>
        /// ID of the block.
        /// </summary>
        public int id;

        /// <summary>
        /// Subtypes for the block.
        /// </summary>
        public Dictionary<int, BlockSubtype> subTypes = new Dictionary<int, BlockSubtype>();

        /// <summary>
        /// Constructor for block info.
        /// </summary>
        /// <param name="id">ID of the block.</param>
        public BlockInfo(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// Add a subtype to the block info.
        /// </summary>
        /// <param name="id">ID of the subtype.</param>
        /// <param name="invisible">Whether or not the subtype is invisible.</param>
        /// <param name="top">Top texture of the subtype.</param>
        /// <param name="bottom">Bottom texture of the subtype.</param>
        /// <param name="left">Left texture of the subtype.</param>
        /// <param name="right">Right texture of the subtype.</param>
        /// <param name="front">Front texture of the subtype.</param>
        /// <param name="back">Back texture of the subtype.</param>
        public void AddSubType(int id, bool invisible, Texture2D top, Texture2D bottom,
            Texture2D left, Texture2D right, Texture2D front, Texture2D back)
        {
            subTypes[id] = new BlockSubtype()
            {
                id = id,
                invisible = invisible,
                topTex = top,
                bottomTex = bottom,
                leftTex = left,
                rightTex = right,
                frontTex = front,
                backTex = back
            };
        }
    }
}