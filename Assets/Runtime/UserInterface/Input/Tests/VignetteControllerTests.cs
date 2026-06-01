// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.VR.Comfort;

/// <summary>
/// PlayMode tests for VignetteController.
/// Validates activation threshold, proportional intensity, release timing,
/// component structure, and shader property writes.
/// </summary>
public class VignetteControllerTests
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

    /// <summary>
    /// Creates a VignetteController + VelocityTracker pair wired together.
    /// Returns the controller, tracker, and the tracker's target transform for movement.
    /// </summary>
    private (VignetteController controller, VelocityTracker tracker, Transform target) CreateVignetteSetup()
    {
        // Velocity tracker with target
        var trackerGO = new GameObject("VelocityTrackerHost");
        _testObjects.Add(trackerGO);
        var tracker = trackerGO.AddComponent<VelocityTracker>();

        var targetGO = new GameObject("Target");
        _testObjects.Add(targetGO);
        tracker.SetTarget(targetGO.transform);

        // Vignette controller
        var vignetteGO = new GameObject("VignetteHost");
        _testObjects.Add(vignetteGO);
        var controller = vignetteGO.AddComponent<VignetteController>();
        controller.SetVelocityTracker(tracker);

        return (controller, tracker, targetGO.transform);
    }

    // ==================== 3.2: Velocity above threshold activates within 1 frame ====================

    [UnityTest]
    public IEnumerator VelocityAboveThreshold_ActivatesWithinOneFrame()
    {
        LogAssert.ignoreFailingMessages = true;

        var (controller, tracker, target) = CreateVignetteSetup();

        // Init frame for both components
        yield return null;

        // Move target significantly (well above default 0.1 threshold)
        target.position = new Vector3(5f, 0f, 0f);
        yield return null;

        Assert.IsTrue(controller.IsRendering, "MeshRenderer should be enabled when velocity exceeds threshold");
        Assert.Greater(controller.CurrentIntensity, 0f, "Intensity should be > 0 when active");
    }

    // ==================== 3.3: Velocity below threshold fades out, disables renderer ====================

    [UnityTest]
    public IEnumerator VelocityBelowThreshold_FadesOutAndDisablesRenderer()
    {
        LogAssert.ignoreFailingMessages = true;

        var (controller, tracker, target) = CreateVignetteSetup();

        // Init frame
        yield return null;

        // Activate vignette
        target.position = new Vector3(5f, 0f, 0f);
        yield return null;
        Assert.IsTrue(controller.IsRendering, "Should be active after movement");

        // Stop moving — velocity goes to 0, release begins
        // Wait enough frames for release to complete (>=200ms at typical frame rates)
        float elapsed = 0f;
        float maxWait = 2f;
        while (controller.IsRendering && elapsed < maxWait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.IsFalse(controller.IsRendering, "MeshRenderer should be disabled after release completes");
        Assert.AreEqual(0f, controller.CurrentIntensity, 0.001f, "Intensity should be 0 after release");
    }

    // ==================== 3.4: Intensity proportional to velocity ====================

    [UnityTest]
    public IEnumerator IntensityProportionalToVelocity()
    {
        LogAssert.ignoreFailingMessages = true;

        var (controller, tracker, target) = CreateVignetteSetup();

        // Init frame
        yield return null;

        // Very small movement — just barely above threshold to get low intensity
        // Default threshold is 0.1, intensity maps via InverseLerp(0.1, 1.0, velocity)
        // We need velocity just above threshold so intensity is well below max
        target.position = new Vector3(0.005f, 0f, 0f);
        yield return null;
        float lowIntensity = controller.CurrentIntensity;

        // Reset: stop and wait for release
        float elapsed = 0f;
        while (controller.IsRendering && elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Large movement from current position — should max out intensity
        Vector3 prev = target.position;
        target.position = prev + new Vector3(10f, 0f, 0f);
        yield return null;
        float highIntensity = controller.CurrentIntensity;

        Assert.GreaterOrEqual(highIntensity, lowIntensity,
            "Higher velocity should produce equal or higher intensity");
        // At least one of them should be non-zero
        Assert.Greater(highIntensity, 0f, "High velocity should produce positive intensity");
    }

    // ==================== 3.5: Stationary keeps renderer disabled ====================

    [UnityTest]
    public IEnumerator Stationary_RendererStaysDisabled()
    {
        LogAssert.ignoreFailingMessages = true;

        var (controller, tracker, target) = CreateVignetteSetup();
        target.position = Vector3.zero;

        // Init + several stationary frames
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            Assert.IsFalse(controller.IsRendering, $"Frame {i}: Renderer should stay disabled when stationary");
            Assert.AreEqual(0f, controller.CurrentIntensity, 0.001f,
                $"Frame {i}: Intensity should be 0 when stationary");
        }
    }

    // ==================== 3.6: Component structure — 1 MeshRenderer, 1 MeshFilter, 1 Material ====================

    [UnityTest]
    public IEnumerator ComponentStructure_SingleRendererFilterMaterial()
    {
        LogAssert.ignoreFailingMessages = true;

        var (controller, tracker, target) = CreateVignetteSetup();
        yield return null;

        var go = controller.gameObject;
        var renderers = go.GetComponents<MeshRenderer>();
        var filters = go.GetComponents<MeshFilter>();

        Assert.AreEqual(1, renderers.Length, "Should have exactly 1 MeshRenderer");
        Assert.AreEqual(1, filters.Length, "Should have exactly 1 MeshFilter");
        Assert.IsNotNull(renderers[0].material, "MeshRenderer should have a material assigned");
        Assert.IsNotNull(filters[0].mesh, "MeshFilter should have a mesh assigned");

        // Verify mesh is a quad (4 vertices, 6 indices / 2 triangles)
        var mesh = filters[0].mesh;
        Assert.AreEqual(4, mesh.vertexCount, "Quad should have 4 vertices");
        Assert.AreEqual(6, mesh.triangles.Length, "Quad should have 6 triangle indices (2 triangles)");
    }

    // ==================== 3.7: Shader property _VignetteIntensity is set on material ====================

    [UnityTest]
    public IEnumerator ShaderProperty_VignetteIntensityIsSet()
    {
        LogAssert.ignoreFailingMessages = true;

        var (controller, tracker, target) = CreateVignetteSetup();
        yield return null;

        var renderer = controller.GetComponent<MeshRenderer>();
        Assert.IsNotNull(renderer, "MeshRenderer should exist");

        // Use sharedMaterial to read the same instance VignetteController writes to
        var mat = renderer.sharedMaterial;
        Assert.IsNotNull(mat, "Material should exist");
        Assert.IsTrue(mat.HasFloat("_VignetteIntensity"),
            "Material should have _VignetteIntensity property");

        // Initially 0
        float initial = mat.GetFloat("_VignetteIntensity");
        Assert.AreEqual(0f, initial, 0.001f, "Initial _VignetteIntensity should be 0");

        // Move to activate
        target.position = new Vector3(5f, 0f, 0f);
        yield return null;

        float active = mat.GetFloat("_VignetteIntensity");
        Assert.Greater(active, 0f, "_VignetteIntensity should be > 0 when active");
    }

    // ==================== 3.8: Release duration >= 200ms ====================

    [UnityTest]
    public IEnumerator ReleaseDuration_AtLeast200ms()
    {
        LogAssert.ignoreFailingMessages = true;

        var (controller, tracker, target) = CreateVignetteSetup();

        // Init frame
        yield return null;

        // Activate with large movement to ensure full intensity
        target.position = new Vector3(10f, 0f, 0f);
        yield return null;
        Assert.IsTrue(controller.IsRendering, "Should be active");
        // Verify intensity is at or near max (0.6) so release takes full _releaseTime (250ms)
        Assert.GreaterOrEqual(controller.CurrentIntensity, 0.5f,
            "Should be at or near max intensity for reliable release timing test");

        // Stop moving — start measuring release time
        // Don't move the target anymore (velocity will be 0)
        float releaseStart = Time.time;

        // Wait a few frames but check that intensity hasn't hit 0 before 200ms
        while (Time.time - releaseStart < 0.19f)
        {
            yield return null;
            // Intensity should still be > 0 within the first 190ms
            if (controller.CurrentIntensity <= 0f)
            {
                Assert.Fail($"Vignette released too quickly at {(Time.time - releaseStart) * 1000f:F0}ms (should be >= 200ms)");
            }
        }

        // Now wait for it to fully release
        float maxWait = 2f;
        float elapsed = Time.time - releaseStart;
        while (controller.IsRendering && elapsed < maxWait)
        {
            elapsed = Time.time - releaseStart;
            yield return null;
        }

        Assert.IsFalse(controller.IsRendering, "Should eventually disable after release");
    }

    // ==================== Edge case: Null VelocityTracker ====================

    [UnityTest]
    public IEnumerator NullVelocityTracker_NoCrashStaysDisabled()
    {
        LogAssert.ignoreFailingMessages = true;

        // Create VignetteController without setting a VelocityTracker
        var vignetteGO = new GameObject("VignetteHost");
        _testObjects.Add(vignetteGO);
        var controller = vignetteGO.AddComponent<VignetteController>();

        // Run several frames — should not crash
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            Assert.IsFalse(controller.IsRendering,
                $"Frame {i}: Renderer should stay disabled with null tracker");
            Assert.AreEqual(0f, controller.CurrentIntensity, 0.001f,
                $"Frame {i}: Intensity should be 0 with null tracker");
        }
    }

    // ==================== 3.9: No GC allocations during steady-state ====================

    [UnityTest]
    public IEnumerator SteadyState_NoGCAllocations()
    {
        LogAssert.ignoreFailingMessages = true;

        var (controller, tracker, target) = CreateVignetteSetup();
        target.position = Vector3.zero;

        // Warm up
        yield return null;
        target.position = new Vector3(1f, 0f, 0f);
        yield return null;

        // Measure during steady-state movement
        int gcBefore = GC.CollectionCount(0);
        for (int i = 0; i < 100; i++)
        {
            target.position += Vector3.forward * 0.5f;
            yield return null;
        }
        int gcAfter = GC.CollectionCount(0);

        // Tolerate <=1 collection — Unity subsystems may trigger Gen0 GC independently
        Assert.LessOrEqual(gcAfter - gcBefore, 1,
            "VignetteController should not trigger GC during steady-state operation");
    }
}
