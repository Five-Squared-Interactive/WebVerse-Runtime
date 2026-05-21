// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Entity;
using UnityEditor;

/// <summary>
/// PlayMode tests that exercise CharacterEntity grounding and gravity behavior to validate four
/// hypotheses about a VR floating/oscillating bug:
///   (1) IsOnSurface raycast (0.25 units) is too short to reliably detect contact.
///   (2) CharacterController.isGrounded is never consulted.
///   (3) Pure-vertical Move() calls behave differently from mixed-axis ones.
///   (4) currentVelocity is treated inconsistently as both velocity (m/s) and per-tick displacement.
///
/// Each test is self-contained: it builds a floor (or no floor for free-fall) and a CharacterEntity,
/// then asserts on grounded/position state after a known physics interval. The tests are tuned so
/// they fail on the current code path and pass with the proposed fixes (longer raycast, isGrounded
/// fallback, units fix, etc.) — so they double as a regression gate.
/// </summary>
public class CharacterEntityGroundingTests
{
    private GameObject weGO;
    private StraightFour straightFour;
    private GameObject cameraGO;
    private GameObject floorGO;

    private const float CHARACTER_HEIGHT = 2.0f;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (StraightFour.ActiveWorld != null)
            {
                StraightFour.UnloadWorld();
            }
        }
        catch (Exception)
        {
            // CameraManager may already be destroyed.
        }
        if (floorGO != null) { UnityEngine.Object.DestroyImmediate(floorGO); floorGO = null; }
        if (cameraGO != null) { UnityEngine.Object.DestroyImmediate(cameraGO); cameraGO = null; }
    }

    /// <summary>
    /// Builds a StraightFour world, a flat floor at y=0 (top surface), and spawns a CharacterEntity
    /// at the provided spawnPosition. fixHeight is disabled so the rescue-warp doesn't mask grounding
    /// bugs. The returned CharacterEntity is fully initialized.
    /// </summary>
    private IEnumerator BuildSceneAndCharacter(Vector3 spawnPosition, bool buildFloor,
        Action<CharacterEntity> callback)
    {
        LogAssert.ignoreFailingMessages = true;

        cameraGO = new GameObject("TestCamera");
        Camera camera = cameraGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -10);
        cameraGO.tag = "MainCamera";

        if (buildFloor)
        {
            // A wide flat box collider with its top surface at y=0.
            floorGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorGO.name = "TestFloor";
            floorGO.transform.position = new Vector3(0, -0.5f, 0);
            floorGO.transform.localScale = new Vector3(20f, 1f, 20f);
        }

        weGO = new GameObject("WE");
        straightFour = weGO.AddComponent<StraightFour>();
        straightFour.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Runtime/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        straightFour.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("grounding-test");

        bool loaded = false;
        Guid charId = StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(
            null, null, Vector3.zero, Quaternion.identity, new Vector3(0, 2, 0),
            spawnPosition, Quaternion.identity, Vector3.one,
            tag: "test-char",
            onLoaded: () => { loaded = true; });

        float elapsed = 0f;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        CharacterEntity ce = StraightFour.ActiveWorld.entityManager.FindEntity(charId) as CharacterEntity;
        Assert.IsNotNull(ce, "CharacterEntity failed to load within 5s.");
        ce.fixHeight = false; // Disable rescue warp so we observe raw grounding behavior.
        callback(ce);
    }

    /// <summary>
    /// Helper: project the character's foot Y from transform.position and CharacterController height.
    /// </summary>
    private static float FootY(CharacterEntity ce)
    {
        CharacterController cc = ce.GetComponent<CharacterController>();
        return ce.transform.position.y - cc.height / 2f + cc.center.y;
    }

    // ---------------------------------------------------------------------------------------------
    // Suspect 1 + 4: After dropping from 3m, the character should settle with foot on the floor.
    // ---------------------------------------------------------------------------------------------

    [UnityTest]
    public IEnumerator CharacterEntity_Drops_FromThreeMeters_SettlesOnFloor()
    {
        CharacterEntity ce = null;
        yield return BuildSceneAndCharacter(new Vector3(0, 3f, 0), buildFloor: true, c => ce = c);

        // Let physics settle. Free-fall from 3m takes ~0.78s; allow generous margin.
        yield return new WaitForSeconds(3f);

        float foot = FootY(ce);
        Assert.That(foot, Is.EqualTo(0f).Within(0.10f),
            $"Character did not settle on floor. Foot y = {foot:F3}, expected ~0. " +
            $"Transform y = {ce.transform.position.y:F3}. Suspect 1 (short raycast), " +
            $"Suspect 4 (units), or a Rigidbody fighting CharacterController.");
    }

    // ---------------------------------------------------------------------------------------------
    // Suspect 1: After landing, the character should hold position. Bobbing > a few mm is the bug.
    // ---------------------------------------------------------------------------------------------

    [UnityTest]
    public IEnumerator CharacterEntity_AfterLanding_DoesNotOscillate()
    {
        CharacterEntity ce = null;
        yield return BuildSceneAndCharacter(new Vector3(0, 3f, 0), buildFloor: true, c => ce = c);

        // Wait for initial settle.
        yield return new WaitForSeconds(3f);

        // Sample foot y over ~1s of physics. With CharacterEntity throttled to ~25 Hz updates,
        // we sample 40 frames to cover one second comfortably.
        List<float> samples = new List<float>();
        for (int i = 0; i < 50; i++)
        {
            samples.Add(FootY(ce));
            yield return new WaitForFixedUpdate();
        }

        float min = float.MaxValue, max = float.MinValue;
        foreach (float y in samples) { if (y < min) min = y; if (y > max) max = y; }
        float swing = max - min;

        Assert.Less(swing, 0.02f,
            $"Character oscillated by {swing:F4} m after settling (min={min:F4} max={max:F4}). " +
            "Suspect 1 (short raycast) or Rigidbody-vs-CharacterController fight.");
    }

    // ---------------------------------------------------------------------------------------------
    // Suspect 1 direct: IsOnSurface() must return true when the foot is touching the floor.
    // ---------------------------------------------------------------------------------------------

    [UnityTest]
    public IEnumerator CharacterEntity_IsOnSurface_TrueAtSmallGapAboveFloor()
    {
        CharacterEntity ce = null;
        // Place character so foot is 0.05 m above floor — well within ANY reasonable grounding margin.
        // Transform y must be height/2 + 0.05 = 1.05.
        yield return BuildSceneAndCharacter(new Vector3(0, CHARACTER_HEIGHT / 2f + 0.05f, 0),
            buildFloor: true, c => ce = c);

        // One physics tick so the controller registers position.
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        Assert.IsTrue(ce.IsOnSurface(),
            "IsOnSurface returned false when foot was only 0.05 m above floor. " +
            "Suspect 1 (raycast too short) — but 0.05 < 0.25 so this should always pass; " +
            "if it fails, the foot computation in IsOnSurface is wrong (transform.position " +
            "may not be where we think it is).");
    }

    /// <summary>
    /// IsOnSurface SHOULD return false when the gap is well beyond the raycast distance — this
    /// documents current behavior and pairs with the previous test as a boundary regression.
    /// </summary>
    [UnityTest]
    public IEnumerator CharacterEntity_IsOnSurface_FalseAtLargeGapAboveFloor()
    {
        CharacterEntity ce = null;
        // Foot 1.5 m above floor.
        yield return BuildSceneAndCharacter(new Vector3(0, CHARACTER_HEIGHT / 2f + 1.5f, 0),
            buildFloor: true, c => ce = c);

        // Disable gravity so the character can't fall during the check.
        Rigidbody rb = ce.GetComponent<Rigidbody>();
        if (rb != null) rb.useGravity = false;

        yield return new WaitForFixedUpdate();

        Assert.IsFalse(ce.IsOnSurface(),
            "IsOnSurface returned true at a 1.5 m gap. Either the raycast was extended (good!) " +
            "or it's hitting something other than the floor (the character's own collider?).");
    }

    // ---------------------------------------------------------------------------------------------
    // Suspect 4: Free-fall integration should approximate s = 0.5 * g * t^2. The unit-confusion
    // bug (gravity adds to velocity per second, but velocity is passed as per-tick displacement)
    // produces an order-of-magnitude faster fall.
    //
    // Analytic free-fall over t=0.3s: dy = 0.5 * 9.81 * 0.09 ≈ 0.44 m.
    // Buggy code: dy ≈ -0.196 * (1+2+...+N) where N ≈ t/0.04 ≈ 7 ticks → dy ≈ 5.5 m.
    //
    // We assert |displacement| < 1.0 m. Real physics: well under. Buggy: way over.
    // ---------------------------------------------------------------------------------------------

    [UnityTest]
    public IEnumerator CharacterEntity_FreeFall_RoughlyMatchesGravity()
    {
        CharacterEntity ce = null;
        // No floor. Spawn high so the character has room to fall.
        yield return BuildSceneAndCharacter(new Vector3(0, 100f, 0), buildFloor: false, c => ce = c);

        // Let one tick pass to ensure the character is initialized in a normal state.
        yield return new WaitForFixedUpdate();
        float startY = ce.transform.position.y;

        yield return new WaitForSeconds(0.3f);
        float endY = ce.transform.position.y;
        float dropped = startY - endY;

        Assert.Less(dropped, 1.0f,
            $"Character fell {dropped:F3} m in 0.3 s. Analytic free-fall is ~0.44 m; >1 m strongly " +
            "suggests Suspect 4: currentVelocity is being treated as per-tick displacement when " +
            "gravity is added as v += a·dt (units mismatch).");
        Assert.Greater(dropped, 0.05f,
            $"Character barely moved ({dropped:F3} m in 0.3 s). Gravity may be disabled or " +
            "the rigidbody is kinematic and the custom gravity path didn't run.");
    }

    // ---------------------------------------------------------------------------------------------
    // Suspect 2 + 3: CharacterController.isGrounded should be consulted in grounding decisions, and
    // pure-vertical Move() may behave worse than mixed-axis. This test compares behavior of an
    // idle character vs one that's been "tickled" with a tiny horizontal perturbation each frame.
    // If the perturbation produces visibly different (less bouncy) settling, Suspect 3 is real.
    // ---------------------------------------------------------------------------------------------

    [UnityTest]
    public IEnumerator CharacterEntity_PureVerticalSettle_NoWorseThan_MixedAxisSettle()
    {
        CharacterEntity ce = null;
        yield return BuildSceneAndCharacter(new Vector3(0, 3f, 0), buildFloor: true, c => ce = c);

        // Phase A: idle settle for 3s, measure final y.
        yield return new WaitForSeconds(3f);
        float idleFootY = FootY(ce);

        // Reset position to drop again, this time with tiny horizontal Move()s during fall.
        ce.SetPosition(new Vector3(0, 3f, 0), local: true, synchronize: false);
        yield return new WaitForFixedUpdate();

        for (int i = 0; i < 120; i++)
        {
            // Tiny non-zero horizontal nudge — invokes the CharacterController's mixed-axis solver.
            ce.Move(new Vector3(0.0001f, 0, 0), synchronize: false);
            yield return new WaitForFixedUpdate();
        }
        float nudgedFootY = FootY(ce);

        // Both should settle near the floor (y=0). If the nudged version is closer to the floor than
        // the idle version by more than 5 cm, Suspect 3 is confirmed.
        float diff = Mathf.Abs(idleFootY - nudgedFootY);
        Assert.Less(diff, 0.05f,
            $"Idle-settle foot y = {idleFootY:F4}, nudged-settle foot y = {nudgedFootY:F4}, " +
            $"diff = {diff:F4} m. A large diff means the CharacterController.Move() solver " +
            "behaves differently for pure-vertical vs mixed-axis input (Suspect 3) — fix C from " +
            "the diagnosis would address this.");
    }

    // ---------------------------------------------------------------------------------------------
    // Diagnostic dump: not a pass/fail test, but exercises the path and logs the key signals every
    // tick so a human can read the console after a run. Useful first-look during investigation.
    // ---------------------------------------------------------------------------------------------

    [UnityTest]
    [Explicit("Diagnostic only — prints per-tick grounding state. Run manually when investigating.")]
    public IEnumerator CharacterEntity_LogsGroundingStateEachTick()
    {
        CharacterEntity ce = null;
        yield return BuildSceneAndCharacter(new Vector3(0, 3f, 0), buildFloor: true, c => ce = c);

        CharacterController cc = ce.GetComponent<CharacterController>();
        Rigidbody rb = ce.GetComponent<Rigidbody>();

        for (int i = 0; i < 80; i++)
        {
            Vector3 foot = ce.transform.position - new Vector3(0, cc.height / 2f, 0);
            bool onSurface = ce.IsOnSurface();
            bool ccGrounded = cc.isGrounded;

            float gap = -1f;
            if (Physics.Raycast(foot, Vector3.down, out RaycastHit hit, 10f,
                ~0, QueryTriggerInteraction.Ignore))
            {
                gap = hit.distance;
            }

            Debug.Log($"[t={Time.time:F3}] footY={foot.y:F4} gap={gap:F4} " +
                $"IsOnSurface={onSurface} cc.isGrounded={ccGrounded} " +
                $"rb.kinematic={rb?.isKinematic} rb.useGravity={rb?.useGravity}");

            yield return new WaitForFixedUpdate();
        }

        Assert.Pass("Diagnostic log complete — inspect Console output.");
    }
}
