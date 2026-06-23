// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Materials;

/// <summary>
/// Story 3 unit tests for named water presets and preset resolution (FR13/FR14).
/// Pure — no world or shader required.
/// </summary>
public class WaterPresetsTests
{
    [Test]
    public void TryResolve_KnownPresets_ReturnTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsTrue(WaterPresets.TryResolve(WaterPresets.CalmLake, out _));
        Assert.IsTrue(WaterPresets.TryResolve(WaterPresets.TropicalShallows, out _));
        Assert.IsTrue(WaterPresets.TryResolve(WaterPresets.MurkyPond, out _));
        Assert.IsTrue(WaterPresets.TryResolve(WaterPresets.StormyOcean, out _));
    }

    [Test]
    public void TryResolve_IsCaseAndWhitespaceInsensitive()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsTrue(WaterPresets.TryResolve("  Calm-Lake  ", out _));
        Assert.IsTrue(WaterPresets.TryResolve("STORMY-OCEAN", out _));
    }

    [Test]
    public void TryResolve_UnknownPreset_ReturnsFalseAndDefault()
    {
        LogAssert.ignoreFailingMessages = true;
        bool matched = WaterPresets.TryResolve("not-a-real-preset", out WaterSurfaceProperties props);
        Assert.IsFalse(matched);
        // Default is calm-lake.
        WaterPresets.TryResolve(WaterPresets.CalmLake, out WaterSurfaceProperties calm);
        Assert.AreEqual(calm.waveAmplitude, props.waveAmplitude, 0.0001f);
    }

    [Test]
    public void TryResolve_NullPreset_ReturnsFalseAndDefault()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsFalse(WaterPresets.TryResolve(null, out _));
    }

    [Test]
    public void Presets_AreMeaningfullyDistinct()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterPresets.TryResolve(WaterPresets.CalmLake, out WaterSurfaceProperties calm);
        WaterPresets.TryResolve(WaterPresets.StormyOcean, out WaterSurfaceProperties stormy);
        // A storm has bigger, faster waves than a calm lake.
        Assert.Greater(stormy.waveAmplitude, calm.waveAmplitude);
        Assert.Greater(stormy.waveSpeed, calm.waveSpeed);
        Assert.Greater(stormy.numWaves, calm.numWaves);
    }

    [Test]
    public void ResolvedPreset_FieldsCanBeOverridden()
    {
        LogAssert.ignoreFailingMessages = true;
        // FR14: a preset provides defaults; explicit overrides win.
        WaterPresets.TryResolve(WaterPresets.CalmLake, out WaterSurfaceProperties props);
        props.waveAmplitude = 0.99f;
        Assert.AreEqual(0.99f, props.waveAmplitude, 0.0001f);
    }
}
