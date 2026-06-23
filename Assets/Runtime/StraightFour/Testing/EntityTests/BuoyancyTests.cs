// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;

/// <summary>
/// Story 2 pure unit tests: buoyancy force, overlapping-volume resolution, and the
/// underwater hysteresis state machine. No physics or world required.
/// </summary>
public class BuoyancyTests
{
    private static BuoyancyProfile Profile(float strength = 10f, float cap = 1f)
    {
        return new BuoyancyProfile
        {
            strength = strength,
            submersionDepthCap = cap,
            submergedLinearDrag = 2f,
            submergedAngularDrag = 1f
        };
    }

    // ---- UpwardForce (AC2) ----

    [Test]
    public void UpwardForce_AtOrAboveSurface_Zero()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(0f, Buoyancy.UpwardForce(0f, Profile()));
        Assert.AreEqual(0f, Buoyancy.UpwardForce(-1f, Profile()));
    }

    [Test]
    public void UpwardForce_IncreasesWithDepthUntilCap()
    {
        LogAssert.ignoreFailingMessages = true;
        BuoyancyProfile p = Profile(strength: 10f, cap: 1f);
        float shallow = Buoyancy.UpwardForce(0.25f, p);
        float deeper = Buoyancy.UpwardForce(0.75f, p);
        Assert.Greater(deeper, shallow);
        Assert.AreEqual(2.5f, shallow, 0.0001f);
        Assert.AreEqual(7.5f, deeper, 0.0001f);
    }

    [Test]
    public void UpwardForce_ClampedAtSubmersionCap()
    {
        LogAssert.ignoreFailingMessages = true;
        BuoyancyProfile p = Profile(strength: 10f, cap: 1f);
        Assert.AreEqual(10f, Buoyancy.UpwardForce(1f, p), 0.0001f);
        Assert.AreEqual(10f, Buoyancy.UpwardForce(5f, p), 0.0001f);
    }

    [Test]
    public void UpwardForce_MonotonicInStrength()
    {
        LogAssert.ignoreFailingMessages = true;
        float weak = Buoyancy.UpwardForce(0.5f, Profile(strength: 5f));
        float strong = Buoyancy.UpwardForce(0.5f, Profile(strength: 50f));
        Assert.Greater(strong, weak);
    }

    // ---- SelectGoverningVolume (AC7 / M3) ----

    private static WaterVolume Vol(float level)
    {
        return new WaterVolume(level, new Bounds(Vector3.zero, new Vector3(20f, 1f, 20f)));
    }

    [Test]
    public void SelectGoverningVolume_NoVolumeContainsPoint_False()
    {
        LogAssert.ignoreFailingMessages = true;
        List<WaterVolume> vols = new List<WaterVolume> { Vol(0f) };
        bool found = Buoyancy.SelectGoverningVolume(new Vector3(100f, 0f, 0f), vols, out int idx);
        Assert.IsFalse(found);
        Assert.AreEqual(-1, idx);
    }

    [Test]
    public void SelectGoverningVolume_OverlapPicksHighestLevel()
    {
        LogAssert.ignoreFailingMessages = true;
        List<WaterVolume> vols = new List<WaterVolume> { Vol(2f), Vol(5f), Vol(3f) };
        bool found = Buoyancy.SelectGoverningVolume(new Vector3(0f, -1f, 0f), vols, out int idx);
        Assert.IsTrue(found);
        Assert.AreEqual(1, idx); // the level=5 volume
    }

    [Test]
    public void SelectGoverningVolume_TieResolvesByRegistrationOrder()
    {
        LogAssert.ignoreFailingMessages = true;
        List<WaterVolume> vols = new List<WaterVolume> { Vol(4f), Vol(4f) };
        Buoyancy.SelectGoverningVolume(new Vector3(0f, 0f, 0f), vols, out int idx);
        Assert.AreEqual(0, idx); // earliest registered wins the tie
    }

    // ---- UnderwaterStateTracker (AC5) ----

    [Test]
    public void Tracker_EntersWhenCrossingDown_OncePerCrossing()
    {
        LogAssert.ignoreFailingMessages = true;
        UnderwaterStateTracker t = new UnderwaterStateTracker(0.05f, -0.05f);
        Assert.AreEqual(UnderwaterStateTracker.Transition.None, t.Update(-1f));
        Assert.AreEqual(UnderwaterStateTracker.Transition.Entered, t.Update(0.5f));
        Assert.AreEqual(UnderwaterStateTracker.Transition.None, t.Update(0.6f)); // already under
        Assert.IsTrue(t.IsUnderwater);
    }

    [Test]
    public void Tracker_ExitsWhenCrossingUp_OncePerCrossing()
    {
        LogAssert.ignoreFailingMessages = true;
        UnderwaterStateTracker t = new UnderwaterStateTracker(0.05f, -0.05f);
        t.Update(0.5f); // enter
        Assert.AreEqual(UnderwaterStateTracker.Transition.Exited, t.Update(-0.5f));
        Assert.AreEqual(UnderwaterStateTracker.Transition.None, t.Update(-0.6f));
        Assert.IsFalse(t.IsUnderwater);
    }

    [Test]
    public void Tracker_OscillationWithinBand_NoExtraTransitions()
    {
        LogAssert.ignoreFailingMessages = true;
        UnderwaterStateTracker t = new UnderwaterStateTracker(0.05f, -0.05f);
        t.Update(0.5f); // enter
        // Jitter around the surface within the hysteresis band: stays underwater, no events.
        Assert.AreEqual(UnderwaterStateTracker.Transition.None, t.Update(0.02f));
        Assert.AreEqual(UnderwaterStateTracker.Transition.None, t.Update(-0.02f));
        Assert.AreEqual(UnderwaterStateTracker.Transition.None, t.Update(0.0f));
        Assert.IsTrue(t.IsUnderwater);
    }
}
