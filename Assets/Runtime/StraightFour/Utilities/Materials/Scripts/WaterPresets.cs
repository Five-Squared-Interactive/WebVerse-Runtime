// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Materials
{
    /// <summary>
    /// Named water-surface presets so authors can start from a feeling ("calm-lake",
    /// "stormy-ocean") instead of tuning fifteen parameters. Each preset is a complete
    /// <see cref="WaterSurfaceProperties"/>; callers may override individual fields afterward.
    /// </summary>
    public static class WaterPresets
    {
        public const string CalmLake = "calm-lake";
        public const string TropicalShallows = "tropical-shallows";
        public const string MurkyPond = "murky-pond";
        public const string StormyOcean = "stormy-ocean";

        /// <summary>
        /// Resolve a preset by name (case-insensitive). Unknown or empty names resolve to the
        /// calm-lake default and return false so callers can warn.
        /// </summary>
        /// <param name="name">Preset name.</param>
        /// <param name="props">The resolved surface properties.</param>
        /// <returns>True if a named preset matched; false if the default was used.</returns>
        public static bool TryResolve(string name, out WaterSurfaceProperties props)
        {
            switch ((name ?? "").Trim().ToLowerInvariant())
            {
                case CalmLake:
                    props = CalmLakeProps();
                    return true;
                case TropicalShallows:
                    props = TropicalShallowsProps();
                    return true;
                case MurkyPond:
                    props = MurkyPondProps();
                    return true;
                case StormyOcean:
                    props = StormyOceanProps();
                    return true;
                default:
                    props = CalmLakeProps();
                    return false;
            }
        }

        private static WaterSurfaceProperties CalmLakeProps()
        {
            return new WaterSurfaceProperties
            {
                shallowColor = new Color(0.30f, 0.55f, 0.65f, 0.85f),
                deepColor = new Color(0.06f, 0.18f, 0.30f, 1.0f),
                specularColor = Color.white,
                scatteringColor = new Color(0.15f, 0.35f, 0.40f, 1.0f),
                deepStart = 0.5f,
                deepEnd = 12.0f,
                distortion = 18.0f,
                smoothness = 0.9f,
                numWaves = 3.0f,
                waveAmplitude = 0.06f,
                waveSteepness = 0.2f,
                waveSpeed = 0.4f,
                waveLength = 14.0f,
                scale = 1.0f,
                intensity = 0.5f
            };
        }

        private static WaterSurfaceProperties TropicalShallowsProps()
        {
            return new WaterSurfaceProperties
            {
                shallowColor = new Color(0.45f, 0.85f, 0.85f, 0.75f),
                deepColor = new Color(0.10f, 0.55f, 0.65f, 1.0f),
                specularColor = Color.white,
                scatteringColor = new Color(0.35f, 0.80f, 0.80f, 1.0f),
                deepStart = 0.3f,
                deepEnd = 6.0f,
                distortion = 24.0f,
                smoothness = 0.92f,
                numWaves = 4.0f,
                waveAmplitude = 0.10f,
                waveSteepness = 0.3f,
                waveSpeed = 0.7f,
                waveLength = 9.0f,
                scale = 1.0f,
                intensity = 0.7f
            };
        }

        private static WaterSurfaceProperties MurkyPondProps()
        {
            return new WaterSurfaceProperties
            {
                shallowColor = new Color(0.25f, 0.30f, 0.20f, 0.95f),
                deepColor = new Color(0.05f, 0.10f, 0.06f, 1.0f),
                specularColor = new Color(0.6f, 0.6f, 0.5f, 1.0f),
                scatteringColor = new Color(0.18f, 0.22f, 0.12f, 1.0f),
                deepStart = 0.2f,
                deepEnd = 3.0f,
                distortion = 10.0f,
                smoothness = 0.5f,
                numWaves = 2.0f,
                waveAmplitude = 0.04f,
                waveSteepness = 0.15f,
                waveSpeed = 0.25f,
                waveLength = 8.0f,
                scale = 0.8f,
                intensity = 0.35f
            };
        }

        private static WaterSurfaceProperties StormyOceanProps()
        {
            return new WaterSurfaceProperties
            {
                shallowColor = new Color(0.20f, 0.35f, 0.40f, 0.9f),
                deepColor = new Color(0.03f, 0.10f, 0.18f, 1.0f),
                specularColor = Color.white,
                scatteringColor = new Color(0.12f, 0.28f, 0.32f, 1.0f),
                deepStart = 0.6f,
                deepEnd = 20.0f,
                distortion = 48.0f,
                smoothness = 0.8f,
                numWaves = 8.0f,
                waveAmplitude = 0.6f,
                waveSteepness = 0.7f,
                waveSpeed = 1.6f,
                waveLength = 18.0f,
                scale = 1.4f,
                intensity = 0.9f
            };
        }
    }
}
