// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;
using FiveSQD.StraightFour.Materials;

/// <summary>
/// Story 1a unit tests for the pure water core: WaterVolume predicate and
/// WaterMaterialAdapter property mapping. These are shader- and world-independent,
/// so they use plain [Test] (no SetUpWorld coroutine).
/// </summary>
public class WaterCoreTests
{
    // A 20x20 footprint centered at origin, surface at y = 0.
    private static WaterVolume MakeVolume(float waterLevel = 0f)
    {
        Bounds footprint = new Bounds(Vector3.zero, new Vector3(20f, 1f, 20f));
        return new WaterVolume(waterLevel, footprint);
    }

    // ---- WaterVolume.IsUnderwater (AC1) ----

    [Test]
    public void IsUnderwater_PointBelowLevelInsideFootprint_True()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterVolume v = MakeVolume(0f);
        Assert.IsTrue(v.IsUnderwater(new Vector3(0f, -1f, 0f)));
    }

    [Test]
    public void IsUnderwater_PointAboveLevelInsideFootprint_False()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterVolume v = MakeVolume(0f);
        Assert.IsFalse(v.IsUnderwater(new Vector3(0f, 1f, 0f)));
    }

    [Test]
    public void IsUnderwater_PointBelowLevelOutsideFootprint_False()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterVolume v = MakeVolume(0f);
        Assert.IsFalse(v.IsUnderwater(new Vector3(100f, -1f, 0f)));
    }

    [Test]
    public void IsUnderwater_PointExactlyAtLevel_False()
    {
        LogAssert.ignoreFailingMessages = true;
        // Strictly below the surface is underwater; exactly at the surface is not.
        WaterVolume v = MakeVolume(0f);
        Assert.IsFalse(v.IsUnderwater(new Vector3(0f, 0f, 0f)));
    }

    [Test]
    public void IsUnderwater_PointAtFootprintEdge_True()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterVolume v = MakeVolume(0f);
        // x = 10 is the inclusive edge of the 20-wide footprint centered at origin.
        Assert.IsTrue(v.IsUnderwater(new Vector3(10f, -0.5f, 0f)));
    }

    // ---- WaterVolume.SubmergedDepth (AC2) ----

    [Test]
    public void SubmergedDepth_AboveSurface_Zero()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterVolume v = MakeVolume(0f);
        Assert.AreEqual(0f, v.SubmergedDepth(new Vector3(0f, 5f, 0f)));
    }

    [Test]
    public void SubmergedDepth_BelowSurface_PositiveDepth()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterVolume v = MakeVolume(3f);
        Assert.AreEqual(5f, v.SubmergedDepth(new Vector3(0f, -2f, 0f)), 0.0001f);
    }

    [Test]
    public void SubmergedDepth_OutsideFootprint_Zero()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterVolume v = MakeVolume(0f);
        Assert.AreEqual(0f, v.SubmergedDepth(new Vector3(100f, -5f, 0f)));
    }

    // ---- WaterMaterialAdapter.Resolve mapping (AC4) ----

    private static WaterSurfaceProperties DefaultProps()
    {
        return new WaterSurfaceProperties
        {
            shallowColor = new Color(0.4f, 0.8f, 1.0f, 0.8f),
            deepColor = new Color(0.1f, 0.3f, 0.7f, 1.0f),
            specularColor = Color.white,
            scatteringColor = new Color(0.2f, 0.6f, 0.8f, 1.0f),
            deepStart = 0.5f,
            deepEnd = 10.0f,
            distortion = 32.0f,
            smoothness = 0.8f,
            numWaves = 4.0f,
            waveAmplitude = 0.3f,
            waveSteepness = 0.5f,
            waveSpeed = 1.0f,
            waveLength = 10.0f,
            scale = 1.0f,
            intensity = 0.7f
        };
    }

    [Test]
    public void Resolve_MapsColorsToCorrectProperties()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterMaterialPropertySet set = WaterMaterialAdapter.Resolve(DefaultProps());
        Assert.AreEqual(new Color(0.4f, 0.8f, 1.0f, 0.8f), set.colors[WaterMaterialAdapter.ShallowColor]);
        Assert.AreEqual(new Color(0.1f, 0.3f, 0.7f, 1.0f), set.colors[WaterMaterialAdapter.DeepColor]);
        Assert.AreEqual(Color.white, set.colors[WaterMaterialAdapter.SpecularColor]);
        Assert.AreEqual(new Color(0.2f, 0.6f, 0.8f, 1.0f), set.colors[WaterMaterialAdapter.ScatteringColor]);
    }

    [Test]
    public void Resolve_MapsFloatsToCorrectProperties()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterMaterialPropertySet set = WaterMaterialAdapter.Resolve(DefaultProps());
        Assert.AreEqual(0.5f, set.floats[WaterMaterialAdapter.DepthStart]);
        Assert.AreEqual(10.0f, set.floats[WaterMaterialAdapter.DepthEnd]);
        Assert.AreEqual(32.0f, set.floats[WaterMaterialAdapter.Distortion]);
        Assert.AreEqual(0.8f, set.floats[WaterMaterialAdapter.Smoothness]);
        Assert.AreEqual(4.0f, set.floats[WaterMaterialAdapter.WaveCount]);
        Assert.AreEqual(0.3f, set.floats[WaterMaterialAdapter.WaveAmplitude]);
        Assert.AreEqual(0.5f, set.floats[WaterMaterialAdapter.WaveSteepness]);
        Assert.AreEqual(1.0f, set.floats[WaterMaterialAdapter.WaveSpeed]);
        Assert.AreEqual(10.0f, set.floats[WaterMaterialAdapter.WaveLength]);
        Assert.AreEqual(0.7f, set.floats[WaterMaterialAdapter.NormalMapIntensity]);
    }

    [Test]
    public void Resolve_MapsNormalMapVectorsFromScaleAndSpeed()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterSurfaceProperties p = DefaultProps();
        p.scale = 2.0f;
        p.waveSpeed = 4.0f;
        WaterMaterialPropertySet set = WaterMaterialAdapter.Resolve(p);
        Assert.AreEqual(new Vector4(2.0f, 2.0f, 1.5f, 1.5f), set.vectors[WaterMaterialAdapter.NormalMapTilings]);
        Assert.AreEqual(new Vector4(4.0f, 4.0f, 3.0f, 3.0f), set.vectors[WaterMaterialAdapter.NormalMapSpeeds]);
    }
}
