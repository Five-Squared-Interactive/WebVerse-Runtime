// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Terrain entity layer.
    /// </summary>
    public struct TerrainEntityLayer
    {
        /// <summary>
        /// Diffuse texture.
        /// </summary>
        public string diffuseTexture;

        /// <summary>
        /// Normal texture.
        /// </summary>
        public string normalTexture;

        /// <summary>
        /// Mask texture.
        /// </summary>
        public string maskTexture;

        /// <summary>
        /// Specularity.
        /// </summary>
        public WorldTypes.Color specular;

        /// <summary>
        /// Metallic factor. Must be between 0 and 1.
        /// </summary>
        public float metallic;

        /// <summary>
        /// Smoothness factor. Must be between 0 and 1.
        /// </summary>
        public float smoothness;

        /// <summary>
        /// Size factor to apply to terrain textures.
        /// </summary>
        public int sizeFactor;
    }
}