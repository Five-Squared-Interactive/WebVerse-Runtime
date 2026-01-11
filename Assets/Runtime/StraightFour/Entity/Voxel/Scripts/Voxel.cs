// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Entity.Voxels
{
    /// <summary>
    /// Class for a voxel.
    /// </summary>
    public class Voxel : MonoBehaviour
    {
        /// <summary>
        /// Top segment of the voxel.
        /// </summary>
        [Tooltip("Top segment of the voxel.")]
        public GameObject top;

        /// <summary>
        /// Bottom segment of the voxel.
        /// </summary>
        [Tooltip("Bottom segment of the voxel.")]
        public GameObject bottom;

        /// <summary>
        /// Left segment of the voxel.
        /// </summary>
        [Tooltip("Left segment of the voxel.")]
        public GameObject left;

        /// <summary>
        /// Right segment of the voxel.
        /// </summary>
        [Tooltip("Right segment of the voxel.")]
        public GameObject right;

        /// <summary>
        /// Front segment of the voxel.
        /// </summary>
        [Tooltip("Front segment of the voxel.")]
        public GameObject front;

        /// <summary>
        /// Back segment of the voxel.
        /// </summary>
        [Tooltip("Back segment of the voxel.")]
        public GameObject back;

        /// <summary>
        /// Type for the voxel.
        /// </summary>
        [Tooltip("Type for the voxel.")]
        public int type;

        /// <summary>
        /// Subtype for the voxel.
        /// </summary>
        [Tooltip("Subtype for the voxel.")]
        public int subType;
    }
}