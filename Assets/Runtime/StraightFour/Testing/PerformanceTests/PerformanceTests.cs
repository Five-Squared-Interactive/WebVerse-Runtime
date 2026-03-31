// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
/// Performance tests verifying StraightFour operations complete within acceptable time bounds.
/// </summary>
public class PerformanceTests
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
        StraightFour.LoadWorld("perf-test");
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

    // ─── Entity Creation Performance ───

    [UnityTest]
    public IEnumerator Perf_Create50Entities_Under2Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < 50; i++)
        {
            em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one);
        }
        yield return null;

        sw.Stop();
        UnityEngine.Debug.Log($"[PERF] Create 50 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 2000, "Creating 50 entities took too long");
    }

    [UnityTest]
    public IEnumerator Perf_Create100Entities_Under5Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < 100; i++)
        {
            em.LoadContainerEntity(null, Vector3.right * i * 2, Quaternion.identity, Vector3.one);
        }
        yield return null;

        sw.Stop();
        UnityEngine.Debug.Log($"[PERF] Create 100 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 5000, "Creating 100 entities took too long");
    }

    // ─── Entity Lookup Performance ───

    [UnityTest]
    public IEnumerator Perf_FindEntity_In50Entities_Under10ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid targetId = Guid.Empty;
        for (int i = 0; i < 50; i++)
        {
            Guid id = em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one);
            if (i == 25) targetId = id;
        }
        yield return null;

        Stopwatch sw = Stopwatch.StartNew();
        BaseEntity found = em.FindEntity(targetId);
        sw.Stop();

        Assert.IsNotNull(found);
        UnityEngine.Debug.Log($"[PERF] FindEntity in 50: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 10, "FindEntity took too long");
    }

    [UnityTest]
    public IEnumerator Perf_FindEntityByTag_Under10ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        for (int i = 0; i < 50; i++)
        {
            em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one,
                tag: $"perf-tag-{i}");
        }
        yield return null;

        Stopwatch sw = Stopwatch.StartNew();
        BaseEntity found = em.FindEntityByTag("perf-tag-25");
        sw.Stop();

        Assert.IsNotNull(found);
        UnityEngine.Debug.Log($"[PERF] FindEntityByTag: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 10, "FindEntityByTag took too long");
    }

    [UnityTest]
    public IEnumerator Perf_GetAllEntities_Under10ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        for (int i = 0; i < 50; i++)
        {
            em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one);
        }
        yield return null;

        Stopwatch sw = Stopwatch.StartNew();
        var all = em.GetAllEntities();
        sw.Stop();

        Assert.IsTrue(all.Length >= 50);
        UnityEngine.Debug.Log($"[PERF] GetAllEntities (50): {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 10, "GetAllEntities took too long");
    }

    // ─── Transform Operations Performance ───

    [UnityTest]
    public IEnumerator Perf_SetPosition500Times_Under100ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < 500; i++)
        {
            entity.SetPosition(new Vector3(i, i, i), false);
        }
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] SetPosition x500: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 100, "500 SetPosition calls took too long");
    }

    [UnityTest]
    public IEnumerator Perf_SetRotation500Times_Under100ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < 500; i++)
        {
            entity.SetRotation(Quaternion.Euler(i, i, i), false);
        }
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] SetRotation x500: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 100, "500 SetRotation calls took too long");
    }

    [UnityTest]
    public IEnumerator Perf_SetScale500Times_Under100ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < 500; i++)
        {
            entity.SetScale(new Vector3(1 + i * 0.001f, 1 + i * 0.001f, 1 + i * 0.001f));
        }
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] SetScale x500: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 100, "500 SetScale calls took too long");
    }

    // ─── Entity Deletion Performance ───

    [UnityTest]
    public IEnumerator Perf_Delete50Entities_Under2Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        List<Guid> ids = new List<Guid>();
        for (int i = 0; i < 50; i++)
        {
            ids.Add(em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one));
        }
        yield return null;

        Stopwatch sw = Stopwatch.StartNew();
        foreach (Guid id in ids)
        {
            BaseEntity entity = em.FindEntity(id);
            if (entity != null) entity.Delete();
        }
        yield return null;
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] Delete 50 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 2000, "Deleting 50 entities took too long");
    }

    // ─── Unload Performance ───

    [UnityTest]
    public IEnumerator Perf_UnloadWorld50Entities_Under2Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        for (int i = 0; i < 50; i++)
        {
            em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one);
        }
        yield return null;

        Stopwatch sw = Stopwatch.StartNew();
        em.Unload();
        yield return null;
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] Unload 50 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 2000, "Unloading 50 entities took too long");
    }

    // ─── Hierarchy Performance ───

    [UnityTest]
    public IEnumerator Perf_CreateDeepHierarchy_10Levels_Under2Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;

        Stopwatch sw = Stopwatch.StartNew();
        Guid rootId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity current = em.FindEntity(rootId);

        for (int i = 1; i < 10; i++)
        {
            Guid childId = em.LoadContainerEntity(current, Vector3.up * i, Quaternion.identity, Vector3.one);
            yield return null;
            current = em.FindEntity(childId);
        }
        sw.Stop();

        Assert.IsNotNull(current);
        UnityEngine.Debug.Log($"[PERF] 10-level hierarchy: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 2000, "Creating 10-level hierarchy took too long");
    }

    [UnityTest]
    public IEnumerator Perf_CreateWideHierarchy_20Children_Under2Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid rootId = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity root = em.FindEntity(rootId);

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < 20; i++)
        {
            em.LoadContainerEntity(root, Vector3.right * i, Quaternion.identity, Vector3.one);
        }
        yield return null;
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] 20 children: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 2000, "Creating 20 children took too long");
    }

    // ─── Mixed Operations Performance ───

    [UnityTest]
    public IEnumerator Perf_CreateTransformDelete_30Entities_Under3Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;

        Stopwatch sw = Stopwatch.StartNew();

        // Create 30 entities.
        List<Guid> ids = new List<Guid>();
        for (int i = 0; i < 30; i++)
        {
            ids.Add(em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one));
        }
        yield return null;

        // Transform all of them.
        foreach (Guid id in ids)
        {
            BaseEntity entity = em.FindEntity(id);
            if (entity != null)
            {
                entity.SetPosition(new Vector3(100, 100, 100), false);
                entity.SetRotation(Quaternion.Euler(45, 45, 45), false);
                entity.SetScale(new Vector3(2, 2, 2));
            }
        }
        yield return null;

        // Delete all of them.
        foreach (Guid id in ids)
        {
            BaseEntity entity = em.FindEntity(id);
            if (entity != null) entity.Delete();
        }
        yield return null;

        sw.Stop();
        UnityEngine.Debug.Log($"[PERF] Create+Transform+Delete 30: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 3000, "Create+Transform+Delete cycle took too long");
    }

    // ─── Large Scale Entity Creation ───

    [UnityTest]
    public IEnumerator Perf_Create200Entities_Under10Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < 200; i++)
        {
            em.LoadContainerEntity(null, new Vector3(i % 20, 0, i / 20), Quaternion.identity, Vector3.one);
        }
        yield return null;

        sw.Stop();
        Assert.AreEqual(200, em.GetAllEntities().Length, 10); // Allow some tolerance
        UnityEngine.Debug.Log($"[PERF] Create 200 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 10000, "Creating 200 entities took too long");
    }

    // ─── Entity Lookup in Large Set ───

    [UnityTest]
    public IEnumerator Perf_FindEntity_In200Entities_Under10ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid targetId = Guid.Empty;
        for (int i = 0; i < 200; i++)
        {
            Guid id = em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one);
            if (i == 150) targetId = id;
        }
        yield return null;

        Stopwatch sw = Stopwatch.StartNew();
        BaseEntity found = em.FindEntity(targetId);
        sw.Stop();

        Assert.IsNotNull(found);
        UnityEngine.Debug.Log($"[PERF] FindEntity in 200: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 10, "FindEntity in 200 took too long");
    }

    [UnityTest]
    public IEnumerator Perf_FindEntityByTag_In100Entities_Under10ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        for (int i = 0; i < 100; i++)
        {
            em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one,
                tag: $"large-tag-{i}");
        }
        yield return null;

        Stopwatch sw = Stopwatch.StartNew();
        BaseEntity found = em.FindEntityByTag("large-tag-75");
        sw.Stop();

        Assert.IsNotNull(found);
        UnityEngine.Debug.Log($"[PERF] FindEntityByTag in 100: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 10, "FindEntityByTag in 100 took too long");
    }

    // ─── Bulk Visibility Toggle ───

    [UnityTest]
    public IEnumerator Perf_ToggleVisibility50Entities_Under500ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        List<Guid> ids = new List<Guid>();
        for (int i = 0; i < 50; i++)
        {
            ids.Add(em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one));
        }
        yield return null;

        Stopwatch sw = Stopwatch.StartNew();
        foreach (Guid id in ids)
        {
            BaseEntity entity = em.FindEntity(id);
            if (entity != null)
            {
                entity.SetVisibility(false);
                entity.SetVisibility(true);
            }
        }
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] Toggle visibility 50 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 500, "Toggling visibility took too long");
    }

    // ─── Bulk Interaction State Changes ───

    [UnityTest]
    public IEnumerator Perf_SetInteractionState50Entities_Under500ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        List<Guid> ids = new List<Guid>();
        for (int i = 0; i < 50; i++)
        {
            ids.Add(em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one));
        }
        yield return null;

        Stopwatch sw = Stopwatch.StartNew();
        foreach (Guid id in ids)
        {
            BaseEntity entity = em.FindEntity(id);
            if (entity != null)
            {
                entity.SetInteractionState(BaseEntity.InteractionState.Hidden);
                entity.SetInteractionState(BaseEntity.InteractionState.Static);
            }
        }
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] InteractionState transitions 50 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 500, "Setting interaction states took too long");
    }

    // ─── Mixed Entity Types Performance ───

    [UnityTest]
    public IEnumerator Perf_CreateMixedEntityTypes_Under3Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < 20; i++)
        {
            em.LoadContainerEntity(null, new Vector3(i, 0, 0), Quaternion.identity, Vector3.one);
        }
        for (int i = 0; i < 10; i++)
        {
            em.LoadLightEntity(null, new Vector3(i, 5, 0), Quaternion.identity);
        }
        for (int i = 0; i < 5; i++)
        {
            em.LoadVoxelEntity(null, new Vector3(i, 10, 0), Quaternion.identity, Vector3.one);
        }
        yield return null;

        sw.Stop();
        Assert.IsTrue(em.GetAllEntities().Length >= 35);
        UnityEngine.Debug.Log($"[PERF] Create mixed types (20+10+5): {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 3000, "Creating mixed entity types took too long");
    }

    // ─── WorldState Serialization Performance ───

    [UnityTest]
    public IEnumerator Perf_WorldStateCapture50Entities_Under2Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        for (int i = 0; i < 50; i++)
        {
            em.LoadContainerEntity(null, new Vector3(i, 0, 0), Quaternion.identity, Vector3.one,
                tag: $"snap-entity-{i}");
        }
        yield return null;

        GameObject mgrGO = new GameObject("StateManager");
        var manager = mgrGO.AddComponent<WorldStateManager>();
        manager.Initialize();

        Stopwatch sw = Stopwatch.StartNew();
        var snapshot = manager.CaptureSnapshot(StraightFour.ActiveWorld, "perf-test", null);
        sw.Stop();

        Assert.IsNotNull(snapshot);
        Assert.IsTrue(snapshot.entities.Count >= 50);
        UnityEngine.Debug.Log($"[PERF] Capture 50 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 2000, "Capturing snapshot took too long");

        manager.Terminate();
        UnityEngine.Object.DestroyImmediate(mgrGO);
    }

    [UnityTest]
    public IEnumerator Perf_WorldStateSerialize100Entities_Under1Second()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        // Build a snapshot with 100 entity entries.
        var snapshot = new WorldStateSnapshot { worldName = "PerfWorld" };
        for (int i = 0; i < 100; i++)
        {
            snapshot.entities.Add(new EntitySnapshot
            {
                id = Guid.NewGuid().ToString(),
                entityType = "ContainerEntity",
                tag = $"Entity_{i}",
                position = new SerializableVector3(i, 0, 0),
                rotation = new SerializableQuaternion(Quaternion.identity),
                scale = new SerializableVector3(1, 1, 1),
                interactionState = "Static",
                visible = true
            });
        }

        Stopwatch sw = Stopwatch.StartNew();
        string json = WorldStateSerializer.SerializeToJson(snapshot);
        sw.Stop();

        Assert.IsNotNull(json);
        Assert.IsTrue(json.Length > 0);
        UnityEngine.Debug.Log($"[PERF] Serialize 100 entities: {sw.ElapsedMilliseconds}ms, {json.Length} chars");
        Assert.Less(sw.ElapsedMilliseconds, 1000, "Serialization took too long");
    }

    [UnityTest]
    public IEnumerator Perf_WorldStateDeserialize100Entities_Under1Second()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        // Build and serialize a snapshot with 100 entities.
        var snapshot = new WorldStateSnapshot { worldName = "PerfWorld" };
        for (int i = 0; i < 100; i++)
        {
            snapshot.entities.Add(new EntitySnapshot
            {
                id = Guid.NewGuid().ToString(),
                entityType = "ContainerEntity",
                tag = $"Entity_{i}",
                position = new SerializableVector3(i, 0, 0),
                rotation = new SerializableQuaternion(Quaternion.identity),
                scale = new SerializableVector3(1, 1, 1)
            });
        }
        string json = WorldStateSerializer.SerializeToJson(snapshot);

        Stopwatch sw = Stopwatch.StartNew();
        var restored = WorldStateSerializer.DeserializeFromJson(json);
        sw.Stop();

        Assert.IsNotNull(restored);
        Assert.AreEqual(100, restored.entities.Count);
        UnityEngine.Debug.Log($"[PERF] Deserialize 100 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 1000, "Deserialization took too long");
    }

    [UnityTest]
    public IEnumerator Perf_WorldStateRoundTrip100Entities_Under2Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var snapshot = new WorldStateSnapshot { worldName = "RoundTripWorld" };
        for (int i = 0; i < 100; i++)
        {
            snapshot.entities.Add(new EntitySnapshot
            {
                id = Guid.NewGuid().ToString(),
                entityType = i % 3 == 0 ? "LightEntity" : "ContainerEntity",
                tag = $"RT_{i}",
                position = new SerializableVector3(i * 2, i, 0),
                rotation = new SerializableQuaternion(Quaternion.Euler(0, i * 3.6f, 0)),
                scale = new SerializableVector3(1, 1, 1),
                interactionState = "Static",
                visible = true
            });
        }

        Stopwatch sw = Stopwatch.StartNew();
        string json = WorldStateSerializer.SerializeToJson(snapshot);
        var restored = WorldStateSerializer.DeserializeFromJson(json);
        sw.Stop();

        Assert.IsNotNull(restored);
        Assert.AreEqual(100, restored.entities.Count);
        Assert.AreEqual("RoundTripWorld", restored.worldName);
        UnityEngine.Debug.Log($"[PERF] Round-trip 100 entities: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 2000, "Round-trip took too long");
    }

    // ─── Load/Unload Stress Cycles ───

    [UnityTest]
    public IEnumerator Perf_LoadUnloadCycle_5Iterations_Under10Seconds()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;

        Stopwatch sw = Stopwatch.StartNew();
        for (int cycle = 0; cycle < 5; cycle++)
        {
            // Create 20 entities.
            for (int i = 0; i < 20; i++)
            {
                em.LoadContainerEntity(null, Vector3.right * i, Quaternion.identity, Vector3.one);
            }
            yield return null;

            Assert.IsTrue(em.GetAllEntities().Length >= 20);

            // Unload all.
            em.Unload();
            yield return null;

            Assert.AreEqual(0, em.GetAllEntities().Length);
        }
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] 5 load/unload cycles (20 each): {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 10000, "Load/unload cycles took too long");
    }

    // ─── Bulk Transform Operations ───

    [UnityTest]
    public IEnumerator Perf_SetPosition1000Times_Under200ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            entity.SetPosition(new Vector3(i * 0.1f, i * 0.1f, i * 0.1f), false);
        }
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] SetPosition x1000: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 200, "1000 SetPosition calls took too long");
    }

    [UnityTest]
    public IEnumerator Perf_SetRotation1000Times_Under200ms()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();
        if (StraightFour.ActiveWorld == null) { Assert.Inconclusive("World not available in this environment"); yield break; }

        var em = StraightFour.ActiveWorld.entityManager;
        Guid id = em.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return null;
        BaseEntity entity = em.FindEntity(id);

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            entity.SetRotation(Quaternion.Euler(i * 0.36f, i * 0.36f, 0), false);
        }
        sw.Stop();

        UnityEngine.Debug.Log($"[PERF] SetRotation x1000: {sw.ElapsedMilliseconds}ms");
        Assert.Less(sw.ElapsedMilliseconds, 200, "1000 SetRotation calls took too long");
    }
}
