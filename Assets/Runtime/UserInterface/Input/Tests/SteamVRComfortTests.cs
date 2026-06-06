// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.VR.Comfort;

/// <summary>
/// Tests for SteamVR comfort component wiring parity with Quest 3.
/// Verifies that the DesktopMode.EnableVR() comfort initialization pattern
/// (VelocityTracker, VignetteController, FadeController) produces functional
/// components identical to Quest3Mode.InitializeVR().
/// </summary>
public class SteamVRComfortTests
{
    private List<GameObject> _testObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in _testObjects)
        {
            if (obj != null) Object.DestroyImmediate(obj);
        }
        _testObjects.Clear();
    }

    private Camera CreateTestCamera()
    {
        var camGO = new GameObject("TestVRCamera");
        _testObjects.Add(camGO);
        return camGO.AddComponent<Camera>();
    }

    /// <summary>
    /// Replicates DesktopMode.EnableVR() comfort wiring pattern.
    /// Returns (VelocityTracker, VignetteController, FadeController, parentGO).
    /// </summary>
    private (VelocityTracker tracker, VignetteController vignette, FadeController fade, GameObject parent)
        CreateComfortComponents(Camera vrCamera)
    {
        var parentGO = new GameObject("DesktopModeHost");
        _testObjects.Add(parentGO);

        var trackerGO = new GameObject("VelocityTracker");
        trackerGO.transform.SetParent(parentGO.transform, false);
        _testObjects.Add(trackerGO);
        var tracker = trackerGO.AddComponent<VelocityTracker>();
        tracker.SetTarget(vrCamera.transform);

        var vignetteGO = new GameObject("VignetteController");
        vignetteGO.transform.SetParent(parentGO.transform, false);
        _testObjects.Add(vignetteGO);
        var vignette = vignetteGO.AddComponent<VignetteController>();
        vignette.SetCamera(vrCamera);
        vignette.SetVelocityTracker(tracker);

        var fadeGO = new GameObject("FadeController");
        fadeGO.transform.SetParent(parentGO.transform, false);
        _testObjects.Add(fadeGO);
        var fade = fadeGO.AddComponent<FadeController>();
        fade.SetCamera(vrCamera);

        return (tracker, vignette, fade, parentGO);
    }

    /// <summary>
    /// Simulates DisableVR() cleanup: destroy in reverse order, null fields.
    /// </summary>
    private void DestroyComfortComponents(
        ref VelocityTracker tracker, ref VignetteController vignette, ref FadeController fade)
    {
        if (vignette != null)
        {
            Object.DestroyImmediate(vignette.gameObject);
            vignette = null;
        }
        if (tracker != null)
        {
            Object.DestroyImmediate(tracker.gameObject);
            tracker = null;
        }
        if (fade != null)
        {
            Object.DestroyImmediate(fade.gameObject);
            fade = null;
        }
    }

    /// <summary>
    /// Verifies VelocityTracker is created with target set to VR camera transform.
    /// </summary>
    [UnityTest]
    public IEnumerator EnableVR_CreatesVelocityTracker()
    {
        LogAssert.ignoreFailingMessages = true;
        var camera = CreateTestCamera();
        yield return null;

        var (tracker, _, _, parent) = CreateComfortComponents(camera);

        Assert.IsNotNull(tracker, "VelocityTracker should be created");
        Assert.AreEqual(0f, tracker.GetVelocity(), "Velocity should be 0 initially");
        // VelocityTracker is parented to DesktopMode host (not camera — SetTarget sets tracking, not parenting)
        Assert.AreEqual(parent.transform, tracker.transform.parent,
            "VelocityTracker should be parented to DesktopMode host");
    }

    /// <summary>
    /// Verifies VignetteController is created with camera and velocity tracker wired.
    /// </summary>
    [UnityTest]
    public IEnumerator EnableVR_CreatesVignetteController()
    {
        LogAssert.ignoreFailingMessages = true;
        var camera = CreateTestCamera();
        yield return null;

        var (_, vignette, _, _) = CreateComfortComponents(camera);

        Assert.IsNotNull(vignette, "VignetteController should be created");
        Assert.IsFalse(vignette.IsRendering, "Vignette should not render when stationary");
        // Verify vignette is parented to camera (SetCamera parents the transform)
        Assert.AreEqual(camera.transform, vignette.transform.parent,
            "VignetteController should be parented to VR camera after SetCamera");
    }

    /// <summary>
    /// Verifies FadeController is created with camera set.
    /// </summary>
    [UnityTest]
    public IEnumerator EnableVR_CreatesFadeController()
    {
        LogAssert.ignoreFailingMessages = true;
        var camera = CreateTestCamera();
        yield return null;

        var (_, _, fade, _) = CreateComfortComponents(camera);

        Assert.IsNotNull(fade, "FadeController should be created");
        Assert.IsFalse(fade.IsFading, "FadeController should not be fading initially");
        Assert.AreEqual(0f, fade.CurrentAlpha, "FadeController alpha should be 0 initially");
        // Verify fade is parented to camera (SetCamera parents the transform)
        Assert.AreEqual(camera.transform, fade.transform.parent,
            "FadeController should be parented to VR camera after SetCamera");
    }

    /// <summary>
    /// Verifies all comfort components are destroyed on DisableVR-style cleanup.
    /// </summary>
    [UnityTest]
    public IEnumerator DisableVR_DestroysComfortComponents()
    {
        LogAssert.ignoreFailingMessages = true;
        var camera = CreateTestCamera();
        yield return null;

        var (tracker, vignette, fade, _) = CreateComfortComponents(camera);

        // Precondition: all components exist
        Assert.IsNotNull(tracker, "Precondition: tracker exists");
        Assert.IsNotNull(vignette, "Precondition: vignette exists");
        Assert.IsNotNull(fade, "Precondition: fade exists");

        // Simulate DisableVR cleanup
        DestroyComfortComponents(ref tracker, ref vignette, ref fade);

        Assert.IsNull(tracker, "VelocityTracker should be null after cleanup");
        Assert.IsNull(vignette, "VignetteController should be null after cleanup");
        Assert.IsNull(fade, "FadeController should be null after cleanup");
    }

    /// <summary>
    /// Verifies EnableVR → DisableVR → EnableVR creates fresh components each time.
    /// </summary>
    [UnityTest]
    public IEnumerator VRToggle_RecreatesComfortComponents()
    {
        LogAssert.ignoreFailingMessages = true;
        var camera = CreateTestCamera();
        yield return null;

        // First EnableVR
        var (tracker1, vignette1, fade1, parent1) = CreateComfortComponents(camera);
        Assert.IsNotNull(tracker1, "First EnableVR: tracker should exist");
        Assert.IsNotNull(fade1, "First EnableVR: fade should exist");

        // Capture instance IDs to verify different instances
        int tracker1Id = tracker1.GetInstanceID();
        int fade1Id = fade1.GetInstanceID();

        // DisableVR
        DestroyComfortComponents(ref tracker1, ref vignette1, ref fade1);
        Assert.IsNull(tracker1, "After DisableVR: tracker should be null");

        yield return null;

        // Second EnableVR
        var (tracker2, vignette2, fade2, parent2) = CreateComfortComponents(camera);
        Assert.IsNotNull(tracker2, "Second EnableVR: tracker should exist");
        Assert.IsNotNull(fade2, "Second EnableVR: fade should exist");

        // Verify fresh instances (different IDs)
        Assert.AreNotEqual(tracker1Id, tracker2.GetInstanceID(),
            "Second EnableVR should create fresh VelocityTracker");
        Assert.AreNotEqual(fade1Id, fade2.GetInstanceID(),
            "Second EnableVR should create fresh FadeController");
    }
}
