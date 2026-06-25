// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;

/// <summary>
/// Pure unit tests for the Gerstner wave surface (WaterWaves) and wave-driven submersion on
/// WaterVolume — the mechanism that makes floating bodies ride the visible waves.
/// </summary>
public class WaterWavesTests
{
    private static WaterWaves WavyWater()
    {
        // Matches a lively surface: count 6, length 8, steepness 0.6, speed 1.5, amplitude 0.5.
        return WaterWaves.FromProperties(6f, 8f, 0.6f, 1.5f, 0.5f);
    }

    [Test]
    public void HeightOffset_ZeroAmplitude_IsFlat()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterWaves calm = WaterWaves.FromProperties(6f, 8f, 0.6f, 1.5f, 0f);
        Assert.AreEqual(0f, calm.HeightOffset(3f, 5f, 1.2f), 0.0001f);
    }

    [Test]
    public void HeightOffset_ZeroWaveCount_IsFlat()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterWaves none = WaterWaves.FromProperties(0f, 8f, 0.6f, 1.5f, 0.5f);
        Assert.AreEqual(0f, none.HeightOffset(3f, 5f, 1.2f), 0.0001f);
    }

    [Test]
    public void HeightOffset_VariesOverTime()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterWaves w = WavyWater();
        float h0 = w.HeightOffset(2f, 2f, 0.0f);
        float h1 = w.HeightOffset(2f, 2f, 1.0f);
        Assert.AreNotEqual(h0, h1, "A moving surface should change height at a fixed point over time.");
    }

    [Test]
    public void HeightOffset_VariesAcrossSpace()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterWaves w = WavyWater();
        float ha = w.HeightOffset(0f, 0f, 0.5f);
        float hb = w.HeightOffset(7f, -4f, 0.5f);
        Assert.AreNotEqual(ha, hb, "Surface height should differ across the footprint.");
    }

    [Test]
    public void WaterVolume_SurfaceHeight_AddsWaveOffsetToLevel()
    {
        LogAssert.ignoreFailingMessages = true;
        Bounds footprint = new Bounds(Vector3.zero, new Vector3(40f, 1f, 40f));
        WaterVolume v = new WaterVolume(3f, footprint) { waves = WavyWater() };
        float expected = 3f + v.waves.HeightOffset(1f, 1f, 0.7f);
        Assert.AreEqual(expected, v.SurfaceHeight(new Vector3(1f, 99f, 1f), 0.7f), 0.0001f);
    }

    [Test]
    public void WaterVolume_SubmergedDepth_BobsWithTime()
    {
        LogAssert.ignoreFailingMessages = true;
        Bounds footprint = new Bounds(Vector3.zero, new Vector3(40f, 1f, 40f));
        WaterVolume v = new WaterVolume(0f, footprint) { waves = WavyWater() };
        Vector3 p = new Vector3(2f, -0.2f, 2f); // just below the mean surface, inside footprint
        float d0 = v.SubmergedDepth(p, 0.0f);
        float d1 = v.SubmergedDepth(p, 1.0f);
        Assert.AreNotEqual(d0, d1, "Submerged depth should change with the waves over time (bobbing).");
    }

    [Test]
    public void WaterVolume_SubmergedDepth_OutsideFootprint_IsZeroEvenWithWaves()
    {
        LogAssert.ignoreFailingMessages = true;
        Bounds footprint = new Bounds(Vector3.zero, new Vector3(40f, 1f, 40f));
        WaterVolume v = new WaterVolume(0f, footprint) { waves = WavyWater() };
        Assert.AreEqual(0f, v.SubmergedDepth(new Vector3(100f, -1f, 0f), 0.5f));
    }
}
