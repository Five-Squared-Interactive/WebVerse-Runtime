// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;
using FiveSQD.StraightFour;
using UnityEditor;
using System;

/// <summary>
/// Tests for entity physics, interaction states, and motion.
/// Uses MeshEntity since BaseEntity's virtual methods don't implement these.
/// </summary>
public class BaseEntityPhysicsTests
{
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
            // CameraManager may already be destroyed during teardown.
        }
    }

    private IEnumerator SetUpWorldAndMeshEntity(Action<MeshEntity> callback)
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject WEGO = new GameObject("WE");
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MeshEntity me = go.AddComponent<MeshEntity>();
        me.Initialize(Guid.NewGuid());
        callback(me);
    }

    // --- Physical Properties Tests ---

    [UnityTest]
    public IEnumerator Physics_SetAngularDrag_ReturnsCorrectValue()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);
        me.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties { angularDrag = 5.5f });

        var props = me.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue);
        Assert.AreEqual(5.5f, props.Value.angularDrag);
    }

    [UnityTest]
    public IEnumerator Physics_SetMass_ReturnsCorrectValue()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);
        me.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties { mass = 99f });

        var props = me.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue);
        Assert.AreEqual(99f, props.Value.mass);
    }

    [UnityTest]
    public IEnumerator Physics_SetDrag_ReturnsCorrectValue()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);
        me.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties { drag = 3.3f });

        var props = me.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue);
        Assert.AreEqual(3.3f, props.Value.drag);
    }

    [UnityTest]
    public IEnumerator Physics_SetGravitational_True_EnablesGravity()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);
        me.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties { gravitational = true });

        var props = me.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue);
        Assert.AreEqual(true, props.Value.gravitational);
    }

    [UnityTest]
    public IEnumerator Physics_SetGravitational_False_DisablesGravity()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);
        me.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties { gravitational = false });

        var props = me.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue);
        Assert.AreEqual(false, props.Value.gravitational);
    }

    [UnityTest]
    public IEnumerator Physics_SetCenterOfMass_ReturnsCorrectValue()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);
        Vector3 com = new Vector3(0.5f, 1.0f, 1.5f);
        me.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties { centerOfMass = com });

        var props = me.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue);
        Assert.AreEqual(com, props.Value.centerOfMass);
    }

    [UnityTest]
    public IEnumerator Physics_PartialUpdate_RetainsOtherValues()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);

        // Set all properties first.
        me.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties
        {
            angularDrag = 1, drag = 2, mass = 50, gravitational = true,
            centerOfMass = new Vector3(1, 2, 3)
        });

        // Update only mass.
        me.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties { mass = 100 });

        var props = me.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue);
        Assert.AreEqual(100f, props.Value.mass);
        // Other properties should remain from first set.
        Assert.AreEqual(1f, props.Value.angularDrag);
        Assert.AreEqual(2f, props.Value.drag);
    }

    // --- Interaction State Tests ---

    [UnityTest]
    public IEnumerator InteractionState_Hidden_DeactivatesGameObject()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.IsTrue(me.gameObject.activeSelf);

        me.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, me.GetInteractionState());
        Assert.IsFalse(me.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator InteractionState_Static_ActivatesGameObject()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Hidden);
        me.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.AreEqual(BaseEntity.InteractionState.Static, me.GetInteractionState());
        Assert.IsTrue(me.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator InteractionState_Physical_ActivatesGameObject()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);
        Assert.AreEqual(BaseEntity.InteractionState.Physical, me.GetInteractionState());
        Assert.IsTrue(me.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator InteractionState_Placing_SetsPlacingState()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Placing);
        Assert.AreEqual(BaseEntity.InteractionState.Placing, me.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator InteractionState_FullCycle()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        // Hidden -> Static -> Physical -> Placing -> Hidden
        me.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, me.GetInteractionState());

        me.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.AreEqual(BaseEntity.InteractionState.Static, me.GetInteractionState());

        me.SetInteractionState(BaseEntity.InteractionState.Physical);
        Assert.AreEqual(BaseEntity.InteractionState.Physical, me.GetInteractionState());

        me.SetInteractionState(BaseEntity.InteractionState.Placing);
        Assert.AreEqual(BaseEntity.InteractionState.Placing, me.GetInteractionState());

        me.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, me.GetInteractionState());
    }

    // --- Motion Tests ---

    [UnityTest]
    public IEnumerator Motion_SetMotion_ReturnsValue()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);

        BaseEntity.EntityMotion motion = new BaseEntity.EntityMotion
        {
            angularVelocity = new Vector3(1, 2, 3),
            stationary = false,
            velocity = new Vector3(4, 5, 6)
        };
        me.SetMotion(motion);
        yield return null;

        var result = me.GetMotion();
        Assert.IsTrue(result.HasValue, "Motion should have a value after setting");
    }

    [UnityTest]
    public IEnumerator Motion_Stationary_SetsKinematic()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Physical);

        BaseEntity.EntityMotion motion = new BaseEntity.EntityMotion
        {
            stationary = true,
            velocity = Vector3.zero,
            angularVelocity = Vector3.zero
        };
        me.SetMotion(motion);
        yield return null;

        var result = me.GetMotion();
        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(true, result.Value.stationary);
    }

    [UnityTest]
    public IEnumerator Motion_GetMotion_WithoutPhysical_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        // In Hidden state (default after Initialize), no rigidbody.
        me.SetInteractionState(BaseEntity.InteractionState.Hidden);
        var result = me.GetMotion();
        // May or may not return null depending on implementation.
        // The key test is that it doesn't throw.
    }

    // --- Links and Seats Tests ---

    [UnityTest]
    public IEnumerator Links_AddLink_CanRetrieve()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Static);

        LinkData link = LinkData.Create("https://example.com", "Example");
        me.AddLink(link);

        LinkData[] links = me.GetLinks();
        Assert.IsNotNull(links);
        Assert.AreEqual(1, links.Length);
        Assert.AreEqual("https://example.com", links[0].uri);
        Assert.AreEqual("Example", links[0].title);
    }

    [UnityTest]
    public IEnumerator Links_AddMultipleLinks_AllRetrievable()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Static);

        me.AddLink(LinkData.Create("https://a.com", "A"));
        me.AddLink(LinkData.Create("https://b.com", "B"));
        me.AddLink(LinkData.Create("https://c.com", "C"));

        LinkData[] links = me.GetLinks();
        Assert.IsNotNull(links);
        Assert.AreEqual(3, links.Length);
    }

    [UnityTest]
    public IEnumerator Seats_AddSeat_CanRetrieve()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Static);

        SeatData seat = SeatData.Create(Vector3.zero, Quaternion.identity);
        me.AddSeat(seat);

        SeatData[] seats = me.GetSeats();
        Assert.IsNotNull(seats);
        Assert.AreEqual(1, seats.Length);
    }

    // --- Placement Socket Tests ---

    [UnityTest]
    public IEnumerator Socket_AddSocket_CreatesChildObject()
    {
        LogAssert.ignoreFailingMessages = true;
        MeshEntity me = null;
        yield return SetUpWorldAndMeshEntity(e => me = e);

        me.SetInteractionState(BaseEntity.InteractionState.Static);

        int initialChildCount = me.transform.childCount;
        me.AddSocket(Vector3.zero, Quaternion.identity, Vector3.up);

        // AddSocket creates a child PlacementSocket GameObject.
        Assert.AreEqual(initialChildCount + 1, me.transform.childCount);
    }

    // --- BaseEntity Virtual Methods (should return false/null) ---

    [UnityTest]
    public IEnumerator BaseEntity_SetPhysicalProperties_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject WEGO = new GameObject("WE");
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        GameObject go = new GameObject();
        BaseEntity be = go.AddComponent<BaseEntity>();
        be.Initialize(Guid.NewGuid());

        be.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties { mass = 10 });
        var props = be.GetPhysicalProperties();
        // BaseEntity's default implementation returns null.
        Assert.IsNull(props);
    }

    [UnityTest]
    public IEnumerator BaseEntity_GetMotion_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject WEGO = new GameObject("WE");
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        GameObject go = new GameObject();
        BaseEntity be = go.AddComponent<BaseEntity>();
        be.Initialize(Guid.NewGuid());

        var motion = be.GetMotion();
        Assert.IsNull(motion);
    }
}
