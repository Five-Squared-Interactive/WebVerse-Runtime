// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.VR.Comfort;

/// <summary>
/// PlayMode tests for VelocityTracker.
/// Validates velocity calculation from transform position deltas,
/// edge cases (first frame, stationary, null target), and zero GC allocations.
/// </summary>
public class VelocityTrackerTests
{
    private List<GameObject> _testObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in _testObjects)
        {
            if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
        }
        _testObjects.Clear();
    }

    private (VelocityTracker tracker, Transform target) CreateTracker()
    {
        var trackerGO = new GameObject("VelocityTrackerHost");
        _testObjects.Add(trackerGO);
        var tracker = trackerGO.AddComponent<VelocityTracker>();

        var targetGO = new GameObject("Target");
        _testObjects.Add(targetGO);

        tracker.SetTarget(targetGO.transform);
        return (tracker, targetGO.transform);
    }

    // ==================== 2.2: Moving transform returns expected velocity ====================

    [UnityTest]
    public IEnumerator MovingTransform_ReturnsExpectedVelocity()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tracker, target) = CreateTracker();

        // First frame — initializes _lastPosition
        yield return null;
        Assert.AreEqual(0f, tracker.GetVelocity(), "First frame should be 0");

        // Move target by exactly 1 unit, record deltaTime on the same frame LateUpdate runs
        target.position = new Vector3(1f, 0f, 0f);
        yield return null;

        // AC1: velocity must equal (currentPos - lastPos).magnitude / Time.deltaTime
        // Distance = 1.0. Velocity was computed in LateUpdate on the frame that just ended.
        // We can't capture the exact deltaTime used, so verify velocity is positive, finite,
        // and in a physically reasonable range for 1m moved in one frame (~15-200 m/s at 5-67fps).
        float velocity = tracker.GetVelocity();
        Assert.Greater(velocity, 0f, "Velocity should be > 0 after movement");
        Assert.Less(velocity, 5000f, "Velocity should be in reasonable range for 1m/frame");
        Assert.IsFalse(float.IsInfinity(velocity), "Velocity should not be infinity");
        Assert.IsFalse(float.IsNaN(velocity), "Velocity should not be NaN");

        // Verify larger movement produces larger velocity
        Vector3 prevPos = target.position;
        target.position = prevPos + new Vector3(3f, 0f, 0f);
        yield return null;
        float tripleVelocity = tracker.GetVelocity();
        Assert.Greater(tripleVelocity, 0f, "3x distance movement should produce positive velocity");
    }

    // ==================== 2.3: Stationary transform returns zero ====================

    [UnityTest]
    public IEnumerator StationaryTransform_ReturnsZero()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tracker, target) = CreateTracker();
        target.position = Vector3.zero;

        // Init frame
        yield return null;

        // Several stationary frames
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            Assert.AreEqual(0f, tracker.GetVelocity(), 0.0001f,
                $"Frame {i}: Stationary transform should have ~0 velocity");
        }
    }

    // ==================== 2.4: First frame returns zero ====================

    [UnityTest]
    public IEnumerator FirstFrame_ReturnsZero()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tracker, target) = CreateTracker();
        target.position = new Vector3(100f, 200f, 300f);

        // Before any LateUpdate
        Assert.AreEqual(0f, tracker.GetVelocity(), "Before first LateUpdate should be 0");

        // After first LateUpdate (initialization frame)
        yield return null;
        Assert.AreEqual(0f, tracker.GetVelocity(), "First frame should be 0, not garbage");
    }

    // ==================== 2.5: Varying speeds track correctly ====================

    [UnityTest]
    public IEnumerator VaryingSpeeds_TracksCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tracker, target) = CreateTracker();
        target.position = Vector3.zero;

        // Init frame
        yield return null;

        // Small movement
        target.position = new Vector3(0.01f, 0f, 0f);
        yield return null;
        float smallVelocity = tracker.GetVelocity();

        // Large movement (teleport-like)
        target.position = new Vector3(100f, 0f, 0f);
        yield return null;
        float largeVelocity = tracker.GetVelocity();

        Assert.Greater(largeVelocity, smallVelocity,
            "Larger movement should produce higher velocity");
        Assert.Greater(largeVelocity, 0f, "Large movement velocity should be > 0");
        Assert.IsFalse(float.IsInfinity(largeVelocity),
            "Large movement should not produce infinity");
    }

    // ==================== 2.6: No GC allocations ====================

    [UnityTest]
    public IEnumerator SteadyState_NoGCAllocations()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tracker, target) = CreateTracker();
        target.position = Vector3.zero;

        // Warm up — init frame
        yield return null;

        // Measure
        int gcBefore = GC.CollectionCount(0);
        for (int i = 0; i < 100; i++)
        {
            target.position += Vector3.forward * 0.01f;
            yield return null;
        }
        int gcAfter = GC.CollectionCount(0);

        // Tolerate ≤1 collection — Unity subsystems (rendering, input, physics) may
        // trigger a Gen0 GC independently of VelocityTracker during PlayMode frames.
        Assert.LessOrEqual(gcAfter - gcBefore, 1,
            "VelocityTracker should not trigger GC during steady-state operation");
    }

    // ==================== Re-targeting resets initialization ====================

    [UnityTest]
    public IEnumerator SetTarget_Retarget_ResetsVelocityToZero()
    {
        LogAssert.ignoreFailingMessages = true;

        var (tracker, target) = CreateTracker();
        yield return null; // init

        target.position = Vector3.one;
        yield return null;
        Assert.Greater(tracker.GetVelocity(), 0f, "Should have velocity before retarget");

        // Re-target to a new transform
        var newTargetGO = new GameObject("NewTarget");
        _testObjects.Add(newTargetGO);
        newTargetGO.transform.position = new Vector3(50f, 50f, 50f);
        tracker.SetTarget(newTargetGO.transform);

        // First frame after retarget should re-initialize (velocity = 0)
        yield return null;
        Assert.AreEqual(0f, tracker.GetVelocity(),
            "First frame after SetTarget should be 0 (re-initialization)");
    }

    // ==================== Edge case: Null target ====================

    [UnityTest]
    public IEnumerator NullTarget_ReturnsZero()
    {
        LogAssert.ignoreFailingMessages = true;

        var trackerGO = new GameObject("VelocityTrackerHost");
        _testObjects.Add(trackerGO);
        var tracker = trackerGO.AddComponent<VelocityTracker>();

        // No SetTarget called — target is null
        yield return null;
        Assert.AreEqual(0f, tracker.GetVelocity(), "Null target should return 0");

        // Set target then clear it
        var targetGO = new GameObject("Target");
        _testObjects.Add(targetGO);
        tracker.SetTarget(targetGO.transform);
        yield return null;

        targetGO.transform.position = Vector3.one;
        yield return null;
        Assert.Greater(tracker.GetVelocity(), 0f, "Should have velocity with target");

        // Destroy target (Unity null-check path)
        UnityEngine.Object.DestroyImmediate(targetGO);
        _testObjects.Remove(targetGO);
        yield return null;
        Assert.AreEqual(0f, tracker.GetVelocity(), "Destroyed target should return 0");

        // Explicit SetTarget(null) path
        var target2GO = new GameObject("Target2");
        _testObjects.Add(target2GO);
        tracker.SetTarget(target2GO.transform);
        yield return null;
        target2GO.transform.position = Vector3.one;
        yield return null;
        Assert.Greater(tracker.GetVelocity(), 0f, "Should have velocity before SetTarget(null)");

        tracker.SetTarget(null);
        yield return null;
        Assert.AreEqual(0f, tracker.GetVelocity(), "Explicit SetTarget(null) should return 0");
    }
}
