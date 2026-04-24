// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.VR.Comfort;

/// <summary>
/// PlayMode tests for FadeController.
/// Validates fade-out/fade-in animations, callback invocation, component structure,
/// shader property writes, duration timing, and render queue ordering.
/// </summary>
public class FadeControllerTests
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

    private FadeController CreateFadeController()
    {
        var go = new GameObject("FadeControllerHost");
        _testObjects.Add(go);
        var controller = go.AddComponent<FadeController>();
        return controller;
    }

    /// <summary>
    /// Wait until fade completes or timeout.
    /// </summary>
    private IEnumerator WaitForFadeComplete(FadeController controller, float maxWait = 3f)
    {
        float elapsed = 0f;
        while (controller.IsFading && elapsed < maxWait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // ==================== 3.2: FadeOut completes to full black ====================

    [UnityTest]
    public IEnumerator FadeOut_CompletesToFullBlack()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null; // Awake frame

        controller.FadeOut(null);
        yield return WaitForFadeComplete(controller);

        Assert.AreEqual(1f, controller.CurrentAlpha, 0.001f,
            "CurrentAlpha should be 1.0 after FadeOut completes");
        Assert.IsTrue(controller.IsRendering,
            "MeshRenderer should be enabled at full black");
        Assert.IsFalse(controller.IsFading,
            "IsFading should be false after completion");
    }

    // ==================== 3.3: FadeOut invokes callback ====================

    [UnityTest]
    public IEnumerator FadeOut_InvokesCallback()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        int callbackCount = 0;
        controller.FadeOut(() => callbackCount++);
        yield return WaitForFadeComplete(controller);

        Assert.AreEqual(1, callbackCount,
            "FadeOut callback should be invoked exactly once");
    }

    // ==================== 3.4: FadeIn completes to fully transparent ====================

    [UnityTest]
    public IEnumerator FadeIn_CompletesToFullTransparent()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        // First fade out fully
        controller.FadeOut(null);
        yield return WaitForFadeComplete(controller);
        Assert.AreEqual(1f, controller.CurrentAlpha, 0.001f, "Should be fully opaque first");

        // Now fade in
        controller.FadeIn();
        yield return WaitForFadeComplete(controller);

        Assert.AreEqual(0f, controller.CurrentAlpha, 0.001f,
            "CurrentAlpha should be 0 after FadeIn completes");
        Assert.IsFalse(controller.IsRendering,
            "MeshRenderer should be disabled after FadeIn completes");
        Assert.IsFalse(controller.IsFading,
            "IsFading should be false after completion");
    }

    // ==================== 3.5: FadeIn with callback invokes onComplete ====================

    [UnityTest]
    public IEnumerator FadeIn_WithCallback_InvokesOnComplete()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        // Fade out first
        controller.FadeOut(null);
        yield return WaitForFadeComplete(controller);

        // Fade in with callback
        int callbackCount = 0;
        controller.FadeIn(() => callbackCount++);
        yield return WaitForFadeComplete(controller);

        Assert.AreEqual(1, callbackCount,
            "FadeIn callback should be invoked exactly once");
    }

    // ==================== 3.6: Initial state — renderer disabled, alpha 0 ====================

    [UnityTest]
    public IEnumerator Initial_RendererDisabled()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null; // Awake frame

        Assert.IsFalse(controller.IsRendering,
            "MeshRenderer should start disabled");
        Assert.AreEqual(0f, controller.CurrentAlpha, 0.001f,
            "CurrentAlpha should start at 0");
        Assert.IsFalse(controller.IsFading,
            "IsFading should start false");
    }

    // ==================== 3.7: Component structure — 1 MeshRenderer, 1 MeshFilter, 1 Material, 4-vertex quad ====================

    [UnityTest]
    public IEnumerator ComponentStructure_SingleRendererFilterMaterial()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        var go = controller.gameObject;
        var renderers = go.GetComponents<MeshRenderer>();
        var filters = go.GetComponents<MeshFilter>();

        Assert.AreEqual(1, renderers.Length, "Should have exactly 1 MeshRenderer");
        Assert.AreEqual(1, filters.Length, "Should have exactly 1 MeshFilter");
        Assert.IsNotNull(renderers[0].material, "MeshRenderer should have a material assigned");
        Assert.IsNotNull(filters[0].mesh, "MeshFilter should have a mesh assigned");

        var mesh = filters[0].mesh;
        Assert.AreEqual(4, mesh.vertexCount, "Quad should have 4 vertices");
        Assert.AreEqual(6, mesh.triangles.Length, "Quad should have 6 triangle indices (2 triangles)");
    }

    // ==================== 3.8: Shader property _FadeAlpha is set during fade ====================

    [UnityTest]
    public IEnumerator ShaderProperty_FadeAlphaIsSet()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        var renderer = controller.GetComponent<MeshRenderer>();
        Assert.IsNotNull(renderer, "MeshRenderer should exist");

        // Use sharedMaterial to read the same instance FadeController writes to
        var mat = renderer.sharedMaterial;
        Assert.IsNotNull(mat, "Material should exist");
        Assert.IsTrue(mat.HasFloat("_FadeAlpha"),
            "Material should have _FadeAlpha property");

        // Initially 0
        float initial = mat.GetFloat("_FadeAlpha");
        Assert.AreEqual(0f, initial, 0.001f, "Initial _FadeAlpha should be 0");

        // Start fade out and check after a frame
        controller.FadeOut(null);
        yield return null;

        float active = mat.GetFloat("_FadeAlpha");
        Assert.Greater(active, 0f, "_FadeAlpha should be > 0 during fade out");
    }

    // ==================== 3.9: FadeOut duration respects _fadeOutDuration ====================

    [UnityTest]
    public IEnumerator FadeOut_Duration_Respects_Setting()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        // Default _fadeOutDuration is 0.3f
        // Fade should take at least ~0.25s (some tolerance for frame timing)
        float startTime = Time.time;
        controller.FadeOut(null);

        // Check that fade hasn't completed within the first ~0.15s
        while (Time.time - startTime < 0.15f)
        {
            yield return null;
            if (!controller.IsFading && controller.CurrentAlpha >= 0.99f)
            {
                // Completed too quickly
                float elapsed = Time.time - startTime;
                Assert.Fail($"Fade completed too quickly at {elapsed * 1000f:F0}ms (expected >= ~200ms)");
            }
        }

        // Now wait for completion
        yield return WaitForFadeComplete(controller);

        float totalTime = Time.time - startTime;
        Assert.GreaterOrEqual(totalTime, 0.2f,
            "FadeOut should take at least ~200ms with default 0.3s duration");
    }

    // ==================== 3.10: Null callback — no crash ====================

    [UnityTest]
    public IEnumerator NullCallback_NoCrash()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        // FadeOut with null callback — should not throw
        controller.FadeOut(null);
        yield return WaitForFadeComplete(controller);

        Assert.AreEqual(1f, controller.CurrentAlpha, 0.001f,
            "Should complete FadeOut with null callback");

        // FadeIn with null callback — should not throw
        controller.FadeIn(null);
        yield return WaitForFadeComplete(controller);

        Assert.AreEqual(0f, controller.CurrentAlpha, 0.001f,
            "Should complete FadeIn with null callback");
    }

    // ==================== 3.11: No GC allocations during steady-state fade ====================

    [UnityTest]
    public IEnumerator SteadyState_NoGCAllocations()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        // Warm up — do one complete fade cycle
        controller.FadeOut(null);
        yield return WaitForFadeComplete(controller);
        controller.FadeIn();
        yield return WaitForFadeComplete(controller);

        // Now measure during a fade out
        controller.FadeOut(null);
        yield return null; // Let first frame of fade run

        int gcBefore = GC.CollectionCount(0);
        for (int i = 0; i < 60; i++)
        {
            yield return null;
        }
        int gcAfter = GC.CollectionCount(0);

        // Tolerate <=1 collection — Unity subsystems may trigger Gen0 GC independently
        Assert.LessOrEqual(gcAfter - gcBefore, 1,
            "FadeController should not trigger GC during steady-state operation");
    }

    // ==================== Edge case: FadeOut while already fading replaces callback ====================

    [UnityTest]
    public IEnumerator FadeOut_WhileAlreadyFading_ReplacesCallback()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        int callback1Count = 0;
        int callback2Count = 0;

        // Start first fade out
        controller.FadeOut(() => callback1Count++);
        yield return null; // Let it run one frame

        // Replace with second fade out while still fading
        Assert.IsTrue(controller.IsFading, "Should still be fading");
        controller.FadeOut(() => callback2Count++);
        yield return WaitForFadeComplete(controller);

        Assert.AreEqual(0, callback1Count,
            "First callback should NOT be invoked when replaced");
        Assert.AreEqual(1, callback2Count,
            "Second callback should be invoked exactly once");
    }

    // ==================== 3.12: Render queue above vignette ====================

    [UnityTest]
    public IEnumerator RenderQueue_AboveVignette()
    {
        LogAssert.ignoreFailingMessages = true;

        var controller = CreateFadeController();
        yield return null;

        var renderer = controller.GetComponent<MeshRenderer>();
        Assert.IsNotNull(renderer, "MeshRenderer should exist");

        var mat = renderer.sharedMaterial;
        Assert.IsNotNull(mat, "Material should exist");

        // Vignette uses Overlay+100 = 4100, Fade uses Overlay+200 = 4200
        int vignetteQueue = 4100; // Overlay (4000) + 100
        Assert.Greater(mat.renderQueue, vignetteQueue,
            $"FadeController render queue ({mat.renderQueue}) should be > vignette ({vignetteQueue})");
    }
}
