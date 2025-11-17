// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Light Properties struct.
    /// </summary>
    public struct LightProperties
    {
        /// <summary>
        /// Color of the light.
        /// </summary>
        public Color color;

        /// <summary>
        /// Temperature of the light.
        /// </summary>
        public int temperature;

        /// <summary>
        /// Intensity of the light.
        /// </summary>
        public float intensity;

        /// <summary>
        /// Range of the light.
        /// </summary>
        public float range;

        /// <summary>
        /// Inner spot angle for the light.
        /// </summary>
        public float innerSpotAngle;

        /// <summary>
        /// Outer spot angle for the light.
        /// </summary>
        public float outerSpotAngle;
    }
}