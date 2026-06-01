// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using FiveSQD.WebVerse.Input;

/// <summary>
/// Shared test helper for creating a fully wired VRRig instance.
/// VRRig properties are computed from real XR interactor components —
/// a bare VRRig with null references always returns None/false from getters.
/// </summary>
public static class VRRigTestHelper
{
    /// <summary>
    /// Minimal MonoBehaviour stand-in for the dynamic move provider.
    /// VRRig.dynamicMoveProvider is typed as MonoBehaviour; the getter
    /// only checks null and .enabled, so any MonoBehaviour works.
    /// </summary>
    public class MockDynamicMoveProvider : MonoBehaviour { }

    /// <summary>
    /// Create a VRRig with all XR interactor components wired up so that
    /// computed properties reflect actual state changes from the setters.
    /// All created GameObjects are added to the provided list for cleanup.
    /// </summary>
    public static VRRig CreateWiredVRRig(List<GameObject> testObjects)
    {
        var go = new GameObject("TestVRRig");
        testObjects.Add(go);
        var rig = go.AddComponent<VRRig>();

        var leftTeleportGO = new GameObject("LeftTeleport");
        testObjects.Add(leftTeleportGO);
        rig.leftTeleportInteractor = leftTeleportGO.AddComponent<XRRayInteractor>();

        var leftRayGO = new GameObject("LeftRay");
        testObjects.Add(leftRayGO);
        rig.leftRayInteractor = leftRayGO.AddComponent<XRRayInteractor>();

        var rightTeleportGO = new GameObject("RightTeleport");
        testObjects.Add(rightTeleportGO);
        rig.rightTeleportInteractor = rightTeleportGO.AddComponent<XRRayInteractor>();

        var rightRayGO = new GameObject("RightRay");
        testObjects.Add(rightRayGO);
        rig.rightRayInteractor = rightRayGO.AddComponent<XRRayInteractor>();

        var snapGO = new GameObject("SnapTurn");
        testObjects.Add(snapGO);
        rig.snapTurnProvider = snapGO.AddComponent<SnapTurnProvider>();

        var contGO = new GameObject("ContinuousTurn");
        testObjects.Add(contGO);
        rig.continuousTurnProvider = contGO.AddComponent<ContinuousTurnProvider>();

        var moveGO = new GameObject("DynamicMove");
        testObjects.Add(moveGO);
        rig.dynamicMoveProvider = moveGO.AddComponent<MockDynamicMoveProvider>();

        var leftDirectGO = new GameObject("LeftDirect");
        testObjects.Add(leftDirectGO);
        rig.leftDirectInteractor = leftDirectGO.AddComponent<XRDirectInteractor>();

        var rightDirectGO = new GameObject("RightDirect");
        testObjects.Add(rightDirectGO);
        rig.rightDirectInteractor = rightDirectGO.AddComponent<XRDirectInteractor>();

        var leftPokeGO = new GameObject("LeftPoke");
        testObjects.Add(leftPokeGO);
        rig.leftPokeInteractor = leftPokeGO.AddComponent<XRPokeInteractor>();

        var rightPokeGO = new GameObject("RightPoke");
        testObjects.Add(rightPokeGO);
        rig.rightPokeInteractor = rightPokeGO.AddComponent<XRPokeInteractor>();

        var teleportProviderGO = new GameObject("TeleportProvider");
        testObjects.Add(teleportProviderGO);
        rig.teleportationProvider = teleportProviderGO.AddComponent<TeleportationProvider>();

        return rig;
    }

    /// <summary>
    /// Destroy all test GameObjects immediately.
    /// </summary>
    public static void Cleanup(List<GameObject> testObjects)
    {
        if (testObjects == null) return;
        foreach (var obj in testObjects)
        {
            if (obj != null) Object.DestroyImmediate(obj);
        }
        testObjects.Clear();
    }
}
