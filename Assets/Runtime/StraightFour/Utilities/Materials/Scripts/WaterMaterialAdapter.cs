// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.StraightFour.Materials
{
    /// <summary>
    /// Semantic water-surface parameters, decoupled from any specific shader's property
    /// names. Entities speak in these terms; <see cref="WaterMaterialAdapter"/> maps them to
    /// the active water shader (currently the 3rd-party URPWater asset).
    /// </summary>
    public struct WaterSurfaceProperties
    {
        public Color shallowColor;
        public Color deepColor;
        public Color specularColor;
        public Color scatteringColor;
        public float deepStart;
        public float deepEnd;
        public float distortion;
        public float smoothness;
        public float numWaves;
        public float waveAmplitude;
        public float waveSteepness;
        public float waveSpeed;
        public float waveLength;
        public float scale;
        public float intensity;
    }

    /// <summary>
    /// Resolved set of shader property name -> value pairs. Produced by
    /// <see cref="WaterMaterialAdapter.Resolve"/> so the semantic-to-shader mapping can be
    /// asserted in unit tests without a live shader, then applied to a Material by
    /// <see cref="WaterMaterialAdapter.Apply"/>.
    /// </summary>
    public sealed class WaterMaterialPropertySet
    {
        public readonly Dictionary<string, Color> colors = new Dictionary<string, Color>();
        public readonly Dictionary<string, float> floats = new Dictionary<string, float>();
        public readonly Dictionary<string, Vector4> vectors = new Dictionary<string, Vector4>();
    }

    /// <summary>
    /// Single point of truth for water shader property names. Centralizing these here means a
    /// future shader swap or a low-end (Quest) fallback shader touches ONE file instead of
    /// every entity. Replaces the magic strings formerly duplicated across WaterBodyEntity
    /// and WaterBlockerEntity.
    /// </summary>
    public static class WaterMaterialAdapter
    {
        // URPWater shader property names.
        public const string ShallowColor = "_Color";
        public const string DeepColor = "_DepthColor";
        public const string SpecularColor = "_SpecColor";
        public const string ScatteringColor = "_ScatteringColor";
        public const string DepthStart = "_DepthStart";
        public const string DepthEnd = "_DepthEnd";
        public const string Distortion = "_Distortion";
        public const string Smoothness = "_Smoothness";
        public const string WaveCount = "_WaveCount";
        public const string WaveAmplitude = "_WaveAmplitude";
        public const string WaveSteepness = "_WaveSteepness";
        public const string WaveSpeed = "_WaveSpeed";
        public const string WaveLength = "_WaveLength";
        public const string NormalMapTilings = "_NormalMapATilings";
        public const string NormalMapSpeeds = "_NormalMapASpeeds";
        public const string NormalMapIntensity = "_NormalMapAIntensity";

        /// <summary>
        /// Resolve semantic water properties into concrete shader name/value pairs. Pure —
        /// no Material required.
        /// </summary>
        /// <param name="p">Semantic water surface properties.</param>
        /// <returns>The resolved property set.</returns>
        public static WaterMaterialPropertySet Resolve(WaterSurfaceProperties p)
        {
            WaterMaterialPropertySet set = new WaterMaterialPropertySet();

            set.colors[ShallowColor] = p.shallowColor;
            set.colors[DeepColor] = p.deepColor;
            set.colors[SpecularColor] = p.specularColor;
            set.colors[ScatteringColor] = p.scatteringColor;

            set.floats[DepthStart] = p.deepStart;
            set.floats[DepthEnd] = p.deepEnd;
            set.floats[Distortion] = p.distortion;
            set.floats[Smoothness] = p.smoothness;
            set.floats[WaveCount] = p.numWaves;
            set.floats[WaveAmplitude] = p.waveAmplitude;
            set.floats[WaveSteepness] = p.waveSteepness;
            set.floats[WaveSpeed] = p.waveSpeed;
            set.floats[WaveLength] = p.waveLength;
            set.floats[NormalMapIntensity] = p.intensity;

            set.vectors[NormalMapTilings] =
                new Vector4(p.scale, p.scale, p.scale * 0.75f, p.scale * 0.75f);
            set.vectors[NormalMapSpeeds] =
                new Vector4(p.waveSpeed, p.waveSpeed, p.waveSpeed * 0.75f, p.waveSpeed * 0.75f);

            return set;
        }

        /// <summary>
        /// Apply the full semantic water surface properties to a material.
        /// </summary>
        /// <param name="mat">Material to write to.</param>
        /// <param name="p">Semantic water surface properties.</param>
        public static void Apply(Material mat, WaterSurfaceProperties p)
        {
            if (mat == null)
            {
                return;
            }
            Apply(mat, Resolve(p));
        }

        /// <summary>
        /// Apply a resolved property set to a material.
        /// </summary>
        /// <param name="mat">Material to write to.</param>
        /// <param name="set">Resolved property set.</param>
        public static void Apply(Material mat, WaterMaterialPropertySet set)
        {
            if (mat == null || set == null)
            {
                return;
            }
            foreach (KeyValuePair<string, Color> kv in set.colors)
            {
                mat.SetColor(kv.Key, kv.Value);
            }
            foreach (KeyValuePair<string, float> kv in set.floats)
            {
                mat.SetFloat(kv.Key, kv.Value);
            }
            foreach (KeyValuePair<string, Vector4> kv in set.vectors)
            {
                mat.SetVector(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// Apply the depth/specular colour subset only (shallow + deep colours, depth zone,
        /// and distortion). Used by the partial SetSpecular paths on the water entities.
        /// </summary>
        /// <param name="mat">Material to write to.</param>
        /// <param name="shallowColor">Shallow zone colour.</param>
        /// <param name="deepColor">Deep zone colour.</param>
        /// <param name="depthStart">Start of the deep zone.</param>
        /// <param name="depthEnd">End of the deep zone.</param>
        /// <param name="distortion">Distortion factor.</param>
        public static void ApplyDepth(Material mat, Color shallowColor, Color deepColor,
            float depthStart, float depthEnd, float distortion)
        {
            if (mat == null)
            {
                return;
            }
            mat.SetColor(ShallowColor, shallowColor);
            mat.SetColor(DeepColor, deepColor);
            mat.SetFloat(DepthStart, depthStart);
            mat.SetFloat(DepthEnd, depthEnd);
            mat.SetFloat(Distortion, distortion);
        }
    }
}
