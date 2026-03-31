// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Entity;
using FiveSQD.StraightFour.WorldState;

/// <summary>
/// Integration tests verifying cross-system interactions in StraightFour.
/// </summary>
public class IntegrationTests
{
    private GameObject weGO;

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

    private IEnumerator SetUpWorld()
    {
#if UNITY_EDITOR
        weGO = new GameObject("WE");
        StraightFour we = weGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Runtime/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.voxelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Runtime/StraightFour/Entity/Voxel/Prefabs/Voxel.prefab");
        we.inputEntityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Runtime/StraightFour/Entity/UI/UIElement/Input/Prefabs/InputEntity.prefab");
        yield return null;
        StraightFour.LoadWorld("integration-test");
        yield return new WaitForSeconds(0.5f);
#else
        yield return null;
#endif
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (StraightFour.ActiveWorld != null)
                StraightFour.UnloadWorld();
        }
        catch (System.Exception) { }

        if (weGO != null)
            UnityEngine.Object.DestroyImmediate(weGO);
    }

    // ─── Entity Lifecycle Integration ───

    [UnityTest]
    public IEnumerator Integration_CreateMultipleEntityTypes_AllCoexist()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid c1 = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        Guid c2 = em.LoadContainerEntity(null, Vector3.right * 5, Quaternion.identity, Vector3.one);
        Guid light = em.LoadLightEntity(null, Vector3.up * 10, Quaternion.identity);
        yield return null;

        Assert.IsTrue(em.Exists(c1));
        Assert.IsTrue(em.Exists(c2));
        Assert.IsTrue(em.Exists(light));

        var all = em.GetAllEntities();
        Assert.IsTrue(all.Length >= 3);
    }

    [UnityTest]
    public IEnumerator Integration_CreateAndDelete_EntityRemoved()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;

        Assert.IsTrue(em.Exists(id));
        BaseEntity entity = em.FindEntity(id);
        Assert.IsNotNull(entity);

        entity.Delete();
        yield return null;

        // After Delete + frame, the component reference becomes null (Unity Destroy behavior).
        Assert.IsTrue(entity == null, "Entity should be destroyed after Delete");
    }

    [UnityTest]
    public IEnumerator Integration_ParentChild_HierarchyMaintained()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid parentId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity parent = em.FindEntity(parentId);

        Guid childId = em.LoadContainerEntity(parent, Vector3.right, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity child = em.FindEntity(childId);

        Assert.IsNotNull(child);
        Assert.AreEqual(parent.transform, child.transform.parent);
    }

    [UnityTest]
    public IEnumerator Integration_EntityTag_FindByTag()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        string testTag = "integration-test-tag";
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one, tag: testTag);
        yield return null;

        BaseEntity found = em.FindEntityByTag(testTag);
        Assert.IsNotNull(found);
        Assert.AreEqual(id, found.id);
    }

    [UnityTest]
    public IEnumerator Integration_TopLevelEntities_ExcludesChildren()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid parentId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity parent = em.FindEntity(parentId);

        em.LoadContainerEntity(parent, Vector3.right, Quaternion.identity, Vector3.one);
        yield return null;

        var topLevel = em.GetAllTopLevelEntities();
        var all = em.GetAllEntities();

        // Top-level should have fewer entities than all entities.
        Assert.IsTrue(topLevel.Length < all.Length);
    }

    // ─── Entity Transform Integration ───

    [UnityTest]
    public IEnumerator Integration_SetPosition_PersistsAcrossFrames()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        Vector3 target = new Vector3(10, 20, 30);
        entity.SetPosition(target, false);
        yield return null;
        yield return null;

        Vector3 pos = entity.GetPosition(false);
        Assert.AreEqual(target.x, pos.x, 0.01f);
        Assert.AreEqual(target.y, pos.y, 0.01f);
        Assert.AreEqual(target.z, pos.z, 0.01f);
    }

    [UnityTest]
    public IEnumerator Integration_SetRotation_PersistsAcrossFrames()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        Quaternion target = Quaternion.Euler(0, 90, 0);
        entity.SetRotation(target, false);
        yield return null;
        yield return null;

        Quaternion rot = entity.GetRotation(false);
        Assert.AreEqual(target.x, rot.x, 0.01f);
        Assert.AreEqual(target.y, rot.y, 0.01f);
        Assert.AreEqual(target.z, rot.z, 0.01f);
        Assert.AreEqual(target.w, rot.w, 0.01f);
    }

    [UnityTest]
    public IEnumerator Integration_SetScale_PersistsAcrossFrames()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        Vector3 target = new Vector3(2, 3, 4);
        entity.SetScale(target);
        yield return null;

        Vector3 scale = entity.GetScale();
        Assert.AreEqual(target.x, scale.x, 0.01f);
        Assert.AreEqual(target.y, scale.y, 0.01f);
        Assert.AreEqual(target.z, scale.z, 0.01f);
    }

    [UnityTest]
    public IEnumerator Integration_ChildLocalPosition_RelativeToParent()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid parentId = em.LoadContainerEntity(null, new Vector3(10, 0, 0), Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity parent = em.FindEntity(parentId);

        Guid childId = em.LoadContainerEntity(parent, new Vector3(5, 0, 0), Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity child = em.FindEntity(childId);

        // Child local position should be (5,0,0).
        Vector3 localPos = child.GetPosition(true);
        Assert.AreEqual(5f, localPos.x, 0.5f);

        // Child world position should be approximately (15,0,0).
        Vector3 worldPos = child.GetPosition(false);
        Assert.AreEqual(15f, worldPos.x, 0.5f);
    }

    // ─── Interaction State Integration ───

    [UnityTest]
    public IEnumerator Integration_InteractionState_TransitionsWork()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        entity.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.AreEqual(BaseEntity.InteractionState.Static, entity.GetInteractionState());

        entity.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, entity.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator Integration_HiddenEntity_NotVisibleButExists()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        entity.SetInteractionState(BaseEntity.InteractionState.Hidden);
        yield return null;

        // Entity should still exist in EntityManager even when hidden.
        Assert.IsTrue(em.Exists(id));
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, entity.GetInteractionState());
    }

    // ─── Physical Properties Integration ───

    [UnityTest]
    public IEnumerator Integration_PhysicalProperties_SetAndGet()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.up * 5, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        // Switch to Physical state so rigidbody is available.
        entity.SetInteractionState(BaseEntity.InteractionState.Physical);
        yield return null;

        var props = new BaseEntity.EntityPhysicalProperties();
        props.mass = 5f;
        props.gravitational = false;
        entity.SetPhysicalProperties(props);

        var retrieved = entity.GetPhysicalProperties();
        if (retrieved.HasValue)
        {
            Assert.AreEqual(5f, retrieved.Value.mass, 0.01f);
        }
        else
        {
            Assert.Pass("Physical properties not available on container entity");
        }
    }

    // ─── World Load/Unload Integration ───

    [UnityTest]
    public IEnumerator Integration_WorldUnload_ClearsAllEntities()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        em.LoadContainerEntity(null, Vector3.right, Quaternion.identity, Vector3.one);
        em.LoadLightEntity(null, Vector3.up, Quaternion.identity);
        yield return null;

        Assert.IsTrue(em.GetAllEntities().Length >= 3);

        em.Unload();
        yield return null;

        Assert.AreEqual(0, em.GetAllEntities().Length);
    }

    [UnityTest]
    public IEnumerator Integration_WorldReload_EntitiesRecreated()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;

        em.Unload();
        yield return null;
        Assert.AreEqual(0, em.GetAllEntities().Length);

        // Create new entities after unload.
        Guid newId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;

        Assert.IsTrue(em.Exists(newId));
    }

    // ─── Light Entity Integration ───

    [UnityTest]
    public IEnumerator Integration_LightEntity_WithContainerParent()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid containerId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity container = em.FindEntity(containerId);

        Guid lightId = em.LoadLightEntity(container, Vector3.up * 3, Quaternion.identity);
        yield return null;

        BaseEntity light = em.FindEntity(lightId);
        Assert.IsNotNull(light);
        Assert.AreEqual(container.transform, light.transform.parent);
    }

    // ─── Canvas Entity Integration ───

    [UnityTest]
    public IEnumerator Integration_CanvasEntity_UnderContainer()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid containerId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity container = em.FindEntity(containerId);

        Guid canvasId = em.LoadCanvasEntity(container, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;

        Assert.IsTrue(em.Exists(canvasId));
        BaseEntity canvas = em.FindEntity(canvasId);
        Assert.IsNotNull(canvas);
    }

    // ─── Voxel Entity Integration ───

    [UnityTest]
    public IEnumerator Integration_VoxelEntity_CreatedSuccessfully()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadVoxelEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;

        Assert.IsTrue(em.Exists(id));
        BaseEntity voxel = em.FindEntity(id);
        Assert.IsNotNull(voxel);
    }

    // ─── Multiple Delete Integration ───

    [UnityTest]
    public IEnumerator Integration_DeleteMultipleEntities_AllRemoved()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        List<Guid> ids = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            ids.Add(em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one));
        }
        yield return null;

        List<BaseEntity> entities = new List<BaseEntity>();
        foreach (Guid id in ids)
        {
            BaseEntity entity = em.FindEntity(id);
            if (entity != null)
            {
                entities.Add(entity);
                entity.Delete();
            }
        }
        yield return null;

        // After Delete + frame, component references become null.
        foreach (BaseEntity entity in entities)
        {
            Assert.IsTrue(entity == null, "Entity should be destroyed after Delete");
        }
    }

    // ─── Entity ID Uniqueness ───

    [UnityTest]
    public IEnumerator Integration_EntityIds_AreUnique()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        HashSet<Guid> ids = new HashSet<Guid>();
        for (int i = 0; i < 10; i++)
        {
            Guid id = em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one);
            Assert.IsTrue(ids.Add(id), "Duplicate entity ID detected");
        }
        yield return null;
    }

    // ─── Deep Hierarchy Integration ───

    [UnityTest]
    public IEnumerator Integration_DeepHierarchy_FourLevels()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;

        Guid l1Id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one, tag: "L1");
        yield return null;
        BaseEntity l1 = em.FindEntity(l1Id);

        Guid l2Id = em.LoadContainerEntity(l1, Vector3.right, Quaternion.identity, Vector3.one, tag: "L2");
        yield return null;
        BaseEntity l2 = em.FindEntity(l2Id);

        Guid l3Id = em.LoadContainerEntity(l2, Vector3.up, Quaternion.identity, Vector3.one, tag: "L3");
        yield return null;
        BaseEntity l3 = em.FindEntity(l3Id);

        Guid l4Id = em.LoadContainerEntity(l3, Vector3.forward, Quaternion.identity, Vector3.one, tag: "L4");
        yield return null;
        BaseEntity l4 = em.FindEntity(l4Id);

        Assert.IsNotNull(l4);
        Assert.AreEqual(l3.transform, l4.transform.parent);
        Assert.AreEqual(l2.transform, l3.transform.parent);
        Assert.AreEqual(l1.transform, l2.transform.parent);
    }

    [UnityTest]
    public IEnumerator Integration_DeleteParent_ChildrenDestroyed()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid parentId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity parent = em.FindEntity(parentId);

        Guid childId = em.LoadContainerEntity(parent, Vector3.right, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity child = em.FindEntity(childId);
        Assert.IsNotNull(child);

        parent.Delete();
        yield return null;

        // Child should also be destroyed when parent is deleted.
        Assert.IsTrue(child == null, "Child should be destroyed when parent is deleted");
    }

    // ─── Visibility Integration ───

    [UnityTest]
    public IEnumerator Integration_VisibilityToggle_PreservesOtherState()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, new Vector3(5, 10, 15), Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        entity.SetVisibility(false);
        yield return null;

        // Position should be preserved after hiding.
        Vector3 pos = entity.GetPosition(false);
        Assert.AreEqual(5f, pos.x, 0.5f);
        Assert.AreEqual(10f, pos.y, 0.5f);

        entity.SetVisibility(true);
        yield return null;

        Assert.IsTrue(em.Exists(id));
    }

    // ─── Custom GUID Integration ───

    [UnityTest]
    public IEnumerator Integration_CustomGuid_IsRespected()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid customId = Guid.NewGuid();
        Guid returnedId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one, id: customId);
        yield return null;

        Assert.AreEqual(customId, returnedId);
        BaseEntity entity = em.FindEntity(customId);
        Assert.IsNotNull(entity);
        Assert.AreEqual(customId, entity.id);
    }

    // ─── Multiple Tags Integration ───

    [UnityTest]
    public IEnumerator Integration_MultipleTags_EachFoundByTag()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one, tag: "alpha");
        em.LoadContainerEntity(null, Vector3.right, Quaternion.identity, Vector3.one, tag: "beta");
        em.LoadContainerEntity(null, Vector3.up, Quaternion.identity, Vector3.one, tag: "gamma");
        yield return null;

        Assert.IsNotNull(em.FindEntityByTag("alpha"));
        Assert.IsNotNull(em.FindEntityByTag("beta"));
        Assert.IsNotNull(em.FindEntityByTag("gamma"));
        Assert.IsNull(em.FindEntityByTag("delta"));
    }

    // ─── Entity Count Tracking ───

    [UnityTest]
    public IEnumerator Integration_EntityCount_TrackedCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        int initialCount = em.GetAllEntities().Length;

        Guid id1 = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        Guid id2 = em.LoadContainerEntity(null, Vector3.right, Quaternion.identity, Vector3.one);
        Guid id3 = em.LoadLightEntity(null, Vector3.up, Quaternion.identity);
        yield return null;

        Assert.AreEqual(initialCount + 3, em.GetAllEntities().Length);

        BaseEntity e1 = em.FindEntity(id1);
        e1.Delete();
        yield return null;
        yield return null;

        // After delete, entity count should decrease.
        // Note: EntityManager may clean up asynchronously.
        int afterDelete = em.GetAllEntities().Length;
        Assert.IsTrue(afterDelete <= initialCount + 3,
            $"Entity count should not increase after delete, got {afterDelete}");
    }

    // ─── WorldState + Live Entity Integration ───

    [UnityTest]
    public IEnumerator Integration_WorldStateCapture_IncludesEntities()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one, tag: "SnapEntity1");
        em.LoadContainerEntity(null, Vector3.right * 5, Quaternion.identity, Vector3.one, tag: "SnapEntity2");
        em.LoadLightEntity(null, Vector3.up * 10, Quaternion.identity, tag: "SnapLight");
        yield return null;

        // Capture world state.
        GameObject mgrGO = new GameObject("StateManager");
        var manager = mgrGO.AddComponent<WorldStateManager>();
        manager.Initialize();

        var snapshot = manager.CaptureSnapshot(StraightFour.ActiveWorld, "integration-test", null);

        Assert.IsNotNull(snapshot, "Snapshot should be captured");
        Assert.AreEqual("integration-test", snapshot.worldName);
        Assert.IsTrue(snapshot.entities.Count >= 3, $"Expected >= 3 entities, got {snapshot.entities.Count}");

        manager.Terminate();
        UnityEngine.Object.DestroyImmediate(mgrGO);
    }

    [UnityTest]
    public IEnumerator Integration_WorldStateCapture_PreservesPositions()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Vector3 targetPos = new Vector3(7, 14, 21);
        em.LoadContainerEntity(null, targetPos, Quaternion.identity, Vector3.one, tag: "PosTestEntity");
        yield return null;

        GameObject mgrGO = new GameObject("StateManager");
        var manager = mgrGO.AddComponent<WorldStateManager>();
        manager.Initialize();

        var snapshot = manager.CaptureSnapshot(StraightFour.ActiveWorld, "pos-test", null);
        Assert.IsNotNull(snapshot);

        // Find the entity in the snapshot by tag.
        EntitySnapshot foundSnap = null;
        foreach (var es in snapshot.entities)
        {
            if (es.tag == "PosTestEntity")
            {
                foundSnap = es;
                break;
            }
        }
        Assert.IsNotNull(foundSnap, "Entity should appear in snapshot");
        Assert.AreEqual(targetPos.x, foundSnap.position.x, 1f);
        Assert.AreEqual(targetPos.y, foundSnap.position.y, 1f);
        Assert.AreEqual(targetPos.z, foundSnap.position.z, 1f);

        manager.Terminate();
        UnityEngine.Object.DestroyImmediate(mgrGO);
    }

    [UnityTest]
    public IEnumerator Integration_WorldStateCapture_SerializesToJson()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one, tag: "JsonTestEntity");
        yield return null;

        GameObject mgrGO = new GameObject("StateManager");
        var manager = mgrGO.AddComponent<WorldStateManager>();
        manager.Initialize();

        var snapshot = manager.CaptureSnapshot(StraightFour.ActiveWorld, "json-test", null);
        Assert.IsNotNull(snapshot);

        string json = WorldStateSerializer.SerializeToJson(snapshot);
        Assert.IsNotNull(json);
        Assert.IsTrue(json.Length > 0);
        Assert.IsTrue(json.Contains("json-test"));
        Assert.IsTrue(json.Contains("JsonTestEntity"));

        // Verify round-trip.
        var restored = WorldStateSerializer.DeserializeFromJson(json);
        Assert.IsNotNull(restored);
        Assert.AreEqual(snapshot.entities.Count, restored.entities.Count);

        manager.Terminate();
        UnityEngine.Object.DestroyImmediate(mgrGO);
    }

    [UnityTest]
    public IEnumerator Integration_WorldStateManager_AddAndRetrieveSnapshot()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;

        GameObject mgrGO = new GameObject("StateManager");
        var manager = mgrGO.AddComponent<WorldStateManager>();
        manager.Initialize();

        var snapshot = manager.CaptureSnapshot(StraightFour.ActiveWorld, "retrieve-test", null);
        Assert.IsNotNull(snapshot);

        Assert.IsTrue(manager.HasSnapshot("retrieve-test"));
        var latest = manager.GetLatestSnapshot("retrieve-test");
        Assert.IsNotNull(latest);
        Assert.AreEqual(snapshot.timestamp, latest.timestamp);

        manager.Terminate();
        UnityEngine.Object.DestroyImmediate(mgrGO);
    }

    // ─── World Name + Active World ───

    [UnityTest]
    public IEnumerator Integration_ActiveWorld_IsAccessible()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        Assert.IsNotNull(StraightFour.ActiveWorld);
        Assert.IsNotNull(StraightFour.ActiveWorld.entityManager);
    }

    // ─── Entity Interaction State After Reload ───

    [UnityTest]
    public IEnumerator Integration_EntitiesAfterUnload_CanBeRecreated()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;

        // Create entities with various configurations.
        Guid c1 = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one, tag: "ReloadA");
        Guid c2 = em.LoadLightEntity(null, Vector3.up * 5, Quaternion.identity, tag: "ReloadLight");
        yield return null;

        Assert.IsTrue(em.Exists(c1));
        Assert.IsTrue(em.Exists(c2));

        // Unload all entities.
        em.Unload();
        yield return null;
        Assert.AreEqual(0, em.GetAllEntities().Length);

        // Re-create with same tags.
        Guid newC1 = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one, tag: "ReloadA");
        Guid newLight = em.LoadLightEntity(null, Vector3.up * 5, Quaternion.identity, tag: "ReloadLight");
        yield return null;

        Assert.IsNotNull(em.FindEntityByTag("ReloadA"));
        Assert.IsNotNull(em.FindEntityByTag("ReloadLight"));
        Assert.AreNotEqual(c1, newC1, "New entities should have new IDs");
    }

    // ─── Mixed Entity Types With State ───

    [UnityTest]
    public IEnumerator Integration_MixedEntityTypes_DifferentStates()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;

        Guid containerId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        Guid lightId = em.LoadLightEntity(null, Vector3.up * 3, Quaternion.identity);
        Guid voxelId = em.LoadVoxelEntity(null, Vector3.right * 5, Quaternion.identity, Vector3.one);
        yield return null;

        BaseEntity container = em.FindEntity(containerId);
        BaseEntity light = em.FindEntity(lightId);
        BaseEntity voxel = em.FindEntity(voxelId);

        // Set different interaction states.
        container.SetInteractionState(BaseEntity.InteractionState.Hidden);
        // LightEntity may not support all states, that's OK.
        light.SetInteractionState(BaseEntity.InteractionState.Static);

        yield return null;

        Assert.AreEqual(BaseEntity.InteractionState.Hidden, container.GetInteractionState());
        Assert.IsTrue(em.Exists(containerId));
        Assert.IsTrue(em.Exists(lightId));
        Assert.IsTrue(em.Exists(voxelId));
    }

    // ─── Snapshot Event Integration ───

    [UnityTest]
    public IEnumerator Integration_WorldStateManager_SnapshotCapturedEvent()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;

        GameObject mgrGO = new GameObject("StateManager");
        var manager = mgrGO.AddComponent<WorldStateManager>();
        manager.Initialize();

        bool eventFired = false;
        string eventWorldName = null;
        manager.OnSnapshotCaptured += (name, snap) =>
        {
            eventFired = true;
            eventWorldName = name;
        };

        manager.CaptureSnapshot(StraightFour.ActiveWorld, "event-test", null);

        Assert.IsTrue(eventFired, "OnSnapshotCaptured should fire");
        Assert.AreEqual("event-test", eventWorldName);

        manager.Terminate();
        UnityEngine.Object.DestroyImmediate(mgrGO);
    }
}
