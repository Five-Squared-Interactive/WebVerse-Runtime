// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Pure C# replica of the URPWater Gerstner wave vertical displacement
    /// (URPWaterWave.hlsl). Lets physics sample the same surface height the shader renders,
    /// so floating bodies ride the visible waves. No Unity lifecycle dependency; time is
    /// passed in (use Unity's `_Time.x`, i.e. Time.time / 20, to match the shader).
    /// </summary>
    public struct WaterWaves
    {
        public float waveCount;     // _WaveCount
        public float waveLength;    // _WaveLength
        public float waveSteepness; // _WaveSteepness
        public float waveSpeed;     // _WaveSpeed
        public float waveAmplitude; // _WaveAmplitude

        private const float Gravity = 9.8f;
        private const float TwoPi = 6.28318f;

        // These constant tables mirror URPWaterWave.hlsl exactly so the physics surface
        // matches the rendered surface.
        private static readonly Vector2[] Directions =
        {
            new Vector2(0.53f, 0.45f),
            new Vector2(-0.209f, 0.4f),
            new Vector2(-0.125f, 0.592f),
            new Vector2(0.482f, -0.876f),
            new Vector2(-0.729f, -0.694f),
            new Vector2(-0.248f, 0.968f),
            new Vector2(0.844f, -0.538f)
        };

        private static readonly float[] Lengths = { 3.56f, 2.85f, 2.10f, 1.30f, 1.10f, 1.2f };
        private static readonly float[] SteepnessRange = { 1.0f, 1.8f, 1.6f, 1.25f, 0.5f };
        private static readonly float[] Speeds = { 0.62f, -0.8f, 0.45f, -0.75f, 0.88f, 0.70f, -0.56f, 0.35f, -0.71f };

        /// <summary>
        /// Build wave parameters from a water surface's authored properties.
        /// </summary>
        public static WaterWaves FromProperties(float numWaves, float waveLength, float waveSteepness,
            float waveSpeed, float waveAmplitude)
        {
            return new WaterWaves
            {
                waveCount = numWaves,
                waveLength = waveLength,
                waveSteepness = waveSteepness,
                waveSpeed = waveSpeed,
                waveAmplitude = waveAmplitude
            };
        }

        /// <summary>
        /// Vertical displacement of the water surface above its base level at a world-space XZ
        /// position and shader time (Time.time / 20). Returns 0 when amplitude/count is zero.
        /// </summary>
        /// <param name="worldX">World-space X.</param>
        /// <param name="worldZ">World-space Z.</param>
        /// <param name="shaderTime">Unity `_Time.x` (Time.time / 20).</param>
        /// <returns>Surface height offset (world units).</returns>
        public float HeightOffset(float worldX, float worldZ, float shaderTime)
        {
            int count = Mathf.Clamp(Mathf.RoundToInt(waveCount), 0, 32);
            if (count <= 0 || waveAmplitude == 0f || waveLength <= 0f)
            {
                return 0f;
            }

            float sumY = 0f;
            for (int i = 0; i < count; i++)
            {
                float steepnessMul = Mathf.Lerp(1.0f, 0.1f, (1.0f / 32.0f) * i);
                float length = Lengths[i % Lengths.Length] * waveLength;
                if (length <= 0f)
                {
                    continue;
                }
                float steepness = SteepnessRange[i % SteepnessRange.Length] * waveSteepness * steepnessMul;
                float speed = Speeds[i % Speeds.Length] * waveSpeed;
                Vector2 d = Directions[i % Directions.Length].normalized;

                float dispersion = TwoPi / length;
                float c = Mathf.Sqrt(Gravity / dispersion) * speed;
                float f = dispersion * (d.x * worldX + d.y * worldZ - c * shaderTime);
                float a = steepness / (dispersion * 1.5f);
                sumY += a * Mathf.Sin(f);
            }

            return waveAmplitude * sumY;
        }
    }
}
