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
/// Story 2 integration tests: buoyancy forces, drag, the unregistered-body guard, player
/// submersion events, and overlapping-volume resolution against live physics.
/// </summary>
public class BuoyancyIntegrationTests
{
    private readonly List<GameObject> spawned = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
        BuoyancyController.ResetForTests();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in spawned)
        {
            if (go != null)
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
        spawned.Clear();
        BuoyancyController.ResetForTests();
        try
        {
            if (StraightFour.ActiveWorld != null)
            {
                StraightFour.UnloadWorld();
            }
        }
        catch (Exception) { }
    }

    private IEnumerator SetUpWorldWithWater()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject WEGO = new GameObject("WE");
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Runtime/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.voxelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Runtime/StraightFour/Entity/Voxel/Prefabs/Voxel.prefab");
        we.inputEntityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Runtime/StraightFour/Entity/UI/UIElement/Input/Prefabs/InputEntity.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        we.waterBodyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Runtime/StraightFour/Entity/Water/Prefabs/WaterBody.prefab");
        yield return null;
        StraightFour.LoadWorld("test");
    }

    // Fill a large basin centred at origin to the given water level. Returns the entity.
    private IEnumerator MakeWater(float waterLevel, Action<WaterBodyEntity> onReady)
    {
        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadWaterBodyEntity(
            Color.cyan, Color.blue, Color.white, Color.cyan,
            // waveAmplitude = 0 -> calm/flat surface so buoyancy assertions are deterministic.
            0.5f, 10f, 32f, 0.8f, 4f, 0f, 0.5f, 1f, 10f, 1f, 0.7f,
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });
        float elapsed = 0f;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        WaterBodyEntity wbe = StraightFour.ActiveWorld.entityManager.FindEntity(id) as WaterBodyEntity;
        wbe.FitToBounds(new Bounds(new Vector3(0f, 0f, 0f), new Vector3(40f, 10f, 40f)), waterLevel);
        onReady(wbe);
    }

    private Rigidbody NewBody(Vector3 position, float mass, bool gravity)
    {
        GameObject go = new GameObject("Body");
        go.transform.position = position;
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.useGravity = gravity;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        spawned.Add(go);
        return rb;
    }

    private static BuoyancyProfile FloatyProfile()
    {
        return new BuoyancyProfile
        {
            strength = 40f,
            submersionDepthCap = 2f,
            submergedLinearDrag = 5f,
            submergedAngularDrag = 1f
        };
    }

    private IEnumerator Step(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    [UnityTest]
    public IEnumerator RegisteredBody_BelowSurface_RisesAndDoesNotLaunch()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();
        WaterBodyEntity water = null;
        yield return MakeWater(0f, w => water = w);
        Assert.IsNotNull(water);

        Rigidbody rb = NewBody(new Vector3(0f, -3f, 0f), 1f, true);
        BuoyancyController.Instance.RegisterBody(rb, FloatyProfile());

        yield return Step(250);

        float y = rb.position.y;
        Assert.Greater(y, -3f, "Body should rise from its release depth.");
        Assert.Greater(y, -1.5f, "Body should float near the surface, not sink.");
        Assert.Less(y, 0.1f, "Body should not launch above the surface.");
    }

    [UnityTest]
    public IEnumerator DenserSinks_LighterFloats()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();
        WaterBodyEntity water = null;
        yield return MakeWater(0f, w => water = w);

        Rigidbody light = NewBody(new Vector3(-2f, -2f, 0f), 0.5f, true);
        Rigidbody heavy = NewBody(new Vector3(2f, -2f, 0f), 30f, true);
        BuoyancyController.Instance.RegisterBody(light, FloatyProfile());
        BuoyancyController.Instance.RegisterBody(heavy, FloatyProfile());

        yield return Step(250);

        Assert.Greater(light.position.y, heavy.position.y + 1f,
            "Lighter body floats above the denser one.");
        Assert.Less(heavy.position.y, -1.5f, "Denser-than-water body sinks.");
    }

    [UnityTest]
    public IEnumerator SubmergedBody_HasHigherDrag_RestoredOutOfWater()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();
        WaterBodyEntity water = null;
        yield return MakeWater(0f, w => water = w);

        Rigidbody rb = NewBody(new Vector3(0f, -1f, 0f), 1f, false);
        BuoyancyController.Instance.RegisterBody(rb, FloatyProfile());

        yield return Step(2);
        Assert.AreEqual(5f, rb.linearDamping, 0.001f, "Submerged drag should be applied.");

        rb.position = new Vector3(0f, 50f, 0f);
        yield return Step(2);
        Assert.AreEqual(0f, rb.linearDamping, 0.001f, "Base drag should be restored out of water.");
    }

    [UnityTest]
    public IEnumerator UnregisteredBody_ReceivesNoBuoyancy()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();
        WaterBodyEntity water = null;
        yield return MakeWater(0f, w => water = w);

        Rigidbody rb = NewBody(new Vector3(0f, -2f, 0f), 1f, true);
        // Intentionally NOT registered.

        yield return Step(100);

        Assert.Less(rb.position.y, -2f, "Unregistered body should fall under gravity, no buoyancy.");
        Assert.IsFalse(BuoyancyController.Instance.IsRegistered(rb));
    }

    [UnityTest]
    public IEnumerator PlayerCrossingSurface_FiresOneEventPerCrossing()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();
        WaterBodyEntity water = null;
        yield return MakeWater(0f, w => water = w);

        GameObject playerGO = new GameObject("PlayerPoint");
        playerGO.transform.position = new Vector3(0f, 5f, 0f);
        spawned.Add(playerGO);

        int enters = 0;
        int exits = 0;
        BuoyancyController.Instance.playerPoint = playerGO.transform;
        BuoyancyController.Instance.OnPlayerUnderwaterChanged += under =>
        {
            if (under) { enters++; } else { exits++; }
        };

        yield return Step(2); // above surface, no event
        playerGO.transform.position = new Vector3(0f, -2f, 0f); // dive
        yield return Step(2);
        // jitter within the hysteresis band, no extra events
        playerGO.transform.position = new Vector3(0f, -0.02f, 0f);
        yield return Step(1);
        playerGO.transform.position = new Vector3(0f, 0.02f, 0f);
        yield return Step(1);
        playerGO.transform.position = new Vector3(0f, 5f, 0f); // surface
        yield return Step(2);

        Assert.AreEqual(1, enters, "Exactly one enter event per downward crossing.");
        Assert.AreEqual(1, exits, "Exactly one exit event per upward crossing.");
    }

    [UnityTest]
    public IEnumerator OverlappingVolumes_HighestLevelGoverns_ForceNotDoubled()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();
        WaterBodyEntity lower = null;
        WaterBodyEntity higher = null;
        yield return MakeWater(2f, w => lower = w);
        yield return MakeWater(5f, w => higher = w);
        Assert.IsNotNull(lower);
        Assert.IsNotNull(higher);

        // Gravity off, no base drag: isolate the buoyancy impulse over one step.
        Rigidbody rb = NewBody(new Vector3(0f, 0f, 0f), 1f, false);
        BuoyancyProfile p = new BuoyancyProfile
        {
            strength = 10f,
            submersionDepthCap = 100f,
            submergedLinearDrag = 2f,
            submergedAngularDrag = 1f
        };
        BuoyancyController.Instance.RegisterBody(rb, p);

        yield return Step(1);

        // Governing volume is level 5 -> depth 5 -> force 50 -> dv ~= 50 * fixedDt / mass,
        // lightly reduced by submerged drag. Summing both volumes (force 70) would overshoot.
        float dt = Time.fixedDeltaTime;
        float expected = (p.strength * 5f) * dt; // ~1.0 at dt=0.02
        Assert.That(rb.linearVelocity.y, Is.EqualTo(expected).Within(0.2f),
            "Only the highest-level volume governs; forces are not summed.");
    }

    // End-to-end: fill a basin MESH to a height, confirm the underwater predicate, and watch a
    // dropped object float. Ties Story 1a (WaterVolume predicate), 1b (fit-to-bounds), and 2
    // (buoyancy) together — the epic's core authoring-to-behavior loop.
    [UnityTest]
    public IEnumerator Epic_FloodBasinMeshAndFloatObject()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();

        // A basin mesh (cube) whose world bounds the water fills to.
        GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        basin.transform.position = Vector3.zero;
        basin.transform.localScale = new Vector3(20f, 4f, 20f);
        spawned.Add(basin);
        yield return null;

        WaterBodyEntity water = null;
        yield return MakeWater(1f, w => water = w);
        Assert.IsNotNull(water);

        // Fill to the basin mesh's world AABB at y = 1 (exercises TryGetWorldRendererBounds + FitToBounds).
        Assert.IsTrue(WaterFill.TryGetWorldRendererBounds(basin, out Bounds basinBounds));
        Assert.IsTrue(water.FitToBounds(basinBounds, 1f));
        water.SetBuoyancyProfile(BuoyancyProfile.Default);

        // The JS-exposed predicate agrees with the fill.
        Assert.IsTrue(water.GetWaterVolume().IsUnderwater(new Vector3(0f, -1f, 0f)));
        Assert.IsFalse(water.GetWaterVolume().IsUnderwater(new Vector3(0f, 3f, 0f)));

        // Drop a buoyant object below the surface — it should float, not sink or launch.
        Rigidbody ball = NewBody(new Vector3(0f, -2f, 0f), 1f, true);
        BuoyancyController.Instance.RegisterBody(ball, FloatyProfile());

        yield return Step(250);

        Assert.Greater(ball.position.y, -2f, "Object rose due to buoyancy.");
        Assert.Greater(ball.position.y, -1.5f, "Object floats near the surface, did not sink.");
        Assert.Less(ball.position.y, 1.2f, "Object did not launch above the waterline.");
    }

    [UnityTest]
    public IEnumerator Compaction_RemovesDestroyedBody()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();

        Rigidbody rb = NewBody(Vector3.zero, 1f, false);
        BuoyancyController.Instance.RegisterBody(rb, FloatyProfile());
        Assert.AreEqual(1, BuoyancyController.Instance.BodyCount);

        UnityEngine.Object.DestroyImmediate(rb.gameObject);
        yield return Step(1);

        Assert.AreEqual(0, BuoyancyController.Instance.BodyCount,
            "A destroyed body should be compacted out of the controller.");
    }

    [UnityTest]
    public IEnumerator Entity_SetBuoyant_RegistersAndUnregistersRigidbody()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();

        // A WaterBodyEntity has a rigidbody on its root, so it stands in for a physical entity
        // to exercise BaseEntity.SetBuoyant (the path MakeBuoyant uses from JS).
        WaterBodyEntity entityWithBody = null;
        yield return MakeWater(0f, w => entityWithBody = w);
        Assert.IsNotNull(entityWithBody);

        int before = BuoyancyController.Instance.BodyCount;
        Assert.IsTrue(entityWithBody.SetBuoyant(true));
        Assert.AreEqual(before + 1, BuoyancyController.Instance.BodyCount, "SetBuoyant should register the rigidbody.");

        Assert.IsTrue(entityWithBody.SetBuoyant(false));
        Assert.AreEqual(before, BuoyancyController.Instance.BodyCount, "SetBuoyant(false) should unregister it.");
    }
}
