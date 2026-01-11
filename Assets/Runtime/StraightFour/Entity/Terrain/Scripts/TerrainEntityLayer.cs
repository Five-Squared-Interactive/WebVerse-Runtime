// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Entity.Terrain
{
    /// <summary>
    /// Struct for a terrain entity layer.
    /// </summary>
    public struct TerrainEntityLayer
    {
        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public Texture2D diffuse;

        /// <summary>
        /// Diffuse texture path.
        /// </summary>
        public string diffusePath;

        /// <summary>
        /// Normal texture.
        /// </summary>
        public Texture2D normal;

        /// <summary>
        /// Normal texture path.
        /// </summary>
        public string normalPath;

        /// <summary>
        /// Mask texture.
        /// </summary>
        public Texture2D mask;

        /// <summary>
        /// Mask texture path.
        /// </summary>
        public string maskPath;

        /// <summary>
        /// Specularity.
        /// </summary>
        public Color specular;

        /// <summary>
        /// Metallic factor. Must be between 0 and 1.
        /// </summary>
        [Range(0, 1)]
        public float metallic;

        /// <summary>
        /// Smoothness factor. Must be between 0 and 1.
        /// </summary>
        [Range(0, 1)]
        public float smoothness;

        /// <summary>
        /// Size factor to apply to terrain textures.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int sizeFactor;
    }
}