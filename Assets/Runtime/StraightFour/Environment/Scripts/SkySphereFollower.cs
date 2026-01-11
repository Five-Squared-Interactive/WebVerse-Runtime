// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Environment
{
    /// <summary>
    /// Class for a sky sphere that follows a transform.
    /// </summary>
    public class SkySphereFollower : MonoBehaviour
    {
        /// <summary>
        /// Transform for the sky sphere to follow.
        /// </summary>
        [Tooltip("Transform for the sky sphere to follow.")]
        public Transform transformToFollow;

        void Update()
        {
            transform.position = transformToFollow.position;
        }
    }
}