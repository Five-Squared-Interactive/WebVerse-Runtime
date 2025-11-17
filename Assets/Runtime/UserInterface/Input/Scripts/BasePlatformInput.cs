// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Base class for platform inputs.
    /// </summary>
    public class BasePlatformInput : MonoBehaviour
    {
        /// <summary>
        /// Get a raycast from the pointer.
        /// </summary>
        /// <param name="direction">Direction to cast the ray in.</param>
        /// <param name="pointerIndex">Index of the pointer to get raycast from.</param>
        /// <returns>A raycast from the pointer, or null.</returns>
        public virtual Tuple<RaycastHit, Vector3> GetPointerRaycast(Vector3 direction, int pointerIndex = 0)
        {
            throw new NotImplementedException();
        }
    }
}