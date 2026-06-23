// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;

/// <summary>
/// Story 1b unit tests for the pure WaterFill.Plan geometry: footprint derivation,
/// surface placement, degenerate-input rejection, and level-vs-basin info flags.
/// Pure — no world or prefab required.
/// </summary>
public class WaterFillTests
{
    // A basin AABB spanning x/z [-10, 10], floor y = 0, top y = 4.
    private static Bounds BasinBounds()
    {
        return new Bounds(new Vector3(0f, 2f, 0f), new Vector3(20f, 4f, 20f));
    }

    [Test]
    public void Plan_FootprintEqualsWorldAabbXZ()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterFill.FillPlan plan = WaterFill.Plan(BasinBounds(), 3f);
        Assert.AreEqual(WaterFill.FillResult.Ok, plan.result);
        Assert.AreEqual(-10f, plan.volume.footprint.min.x, 0.0001f);
        Assert.AreEqual(10f, plan.volume.footprint.max.x, 0.0001f);
        Assert.AreEqual(-10f, plan.volume.footprint.min.z, 0.0001f);
        Assert.AreEqual(10f, plan.volume.footprint.max.z, 0.0001f);
    }

    [Test]
    public void Plan_SurfaceSitsAtWaterLevel()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterFill.FillPlan plan = WaterFill.Plan(BasinBounds(), 3f);
        Assert.AreEqual(3f, plan.volume.waterLevel, 0.0001f);
        Assert.AreEqual(0f, plan.surfacePosition.x, 0.0001f);
        Assert.AreEqual(3f, plan.surfacePosition.y, 0.0001f);
        Assert.AreEqual(0f, plan.surfacePosition.z, 0.0001f);
    }

    [Test]
    public void Plan_SurfaceScaleMatchesFootprintXZ()
    {
        LogAssert.ignoreFailingMessages = true;
        WaterFill.FillPlan plan = WaterFill.Plan(BasinBounds(), 3f, 0.5f);
        Assert.AreEqual(20f, plan.surfaceScale.x, 0.0001f);
        Assert.AreEqual(0.5f, plan.surfaceScale.y, 0.0001f);
        Assert.AreEqual(20f, plan.surfaceScale.z, 0.0001f);
    }

    [Test]
    public void Plan_ExplicitBoxEquivalentToAutoFitForSameBounds()
    {
        LogAssert.ignoreFailingMessages = true;
        Bounds b = BasinBounds();
        WaterFill.FillPlan a = WaterFill.Plan(b, 3f);
        WaterFill.FillPlan c = WaterFill.Plan(new Bounds(b.center, b.size), 3f);
        Assert.AreEqual(a.volume.footprint, c.volume.footprint);
        Assert.AreEqual(a.volume.waterLevel, c.volume.waterLevel);
    }

    [Test]
    public void Plan_ZeroAreaFootprint_RejectedAsDegenerate()
    {
        LogAssert.ignoreFailingMessages = true;
        Bounds flat = new Bounds(Vector3.zero, new Vector3(0f, 4f, 20f));
        WaterFill.FillPlan plan = WaterFill.Plan(flat, 1f);
        Assert.AreEqual(WaterFill.FillResult.DegenerateBounds, plan.result);
    }

    [Test]
    public void Plan_LevelBelowFloor_ValidButFlaggedAndEmptySubmersion()
    {
        LogAssert.ignoreFailingMessages = true;
        // Basin floor is y = 0; level at -1 is below it.
        WaterFill.FillPlan plan = WaterFill.Plan(BasinBounds(), -1f);
        Assert.AreEqual(WaterFill.FillResult.Ok, plan.result);
        Assert.IsTrue(plan.levelBelowFloor);
        // A point inside the footprint at the floor is not below the surface => no submersion.
        Assert.AreEqual(0f, plan.volume.SubmergedDepth(new Vector3(0f, 0f, 0f)), 0.0001f);
    }

    [Test]
    public void Plan_NaNOrInfiniteLevel_RejectedAsDegenerate()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(WaterFill.FillResult.DegenerateBounds,
            WaterFill.Plan(BasinBounds(), float.NaN).result);
        Assert.AreEqual(WaterFill.FillResult.DegenerateBounds,
            WaterFill.Plan(BasinBounds(), float.PositiveInfinity).result);
    }

    [Test]
    public void Plan_LevelAboveTop_ValidOverfillFlagged()
    {
        LogAssert.ignoreFailingMessages = true;
        // Basin top is y = 4; level at 6 is above it.
        WaterFill.FillPlan plan = WaterFill.Plan(BasinBounds(), 6f);
        Assert.AreEqual(WaterFill.FillResult.Ok, plan.result);
        Assert.IsTrue(plan.levelAboveTop);
    }
}
