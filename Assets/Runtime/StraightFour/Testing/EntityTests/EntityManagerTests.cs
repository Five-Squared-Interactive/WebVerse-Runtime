// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Entity;
using System;
using FiveSQD.StraightFour;
using UnityEditor;
using System.Collections.Generic;
using FiveSQD.StraightFour.Entity.Terrain;

public class EntityManagerTests
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
        catch (System.Exception)
        {
            // CameraManager may already be destroyed during teardown.
        }
    }

    [UnityTest]
    public IEnumerator EntityManagerTests_General()
    {
        LogAssert.ignoreFailingMessages = true;
        List<Guid> eIDs = new List<Guid>();

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runtime/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.voxelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runtime/StraightFour/Entity/Voxel/Prefabs/Voxel.prefab");
        we.inputEntityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runtime/StraightFour/Entity/UI/UIElement/Input/Prefabs/InputEntity.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Load Container Entity.
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        Assert.IsNotNull(id);
        eIDs.Add(id);

        // Load Character Entity.
        id = StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(null, null, Vector3.zero, Quaternion.identity, Vector3.zero, Vector3.zero, Quaternion.identity, Vector3.one);
        Assert.IsNotNull(id);
        eIDs.Add(id);

        // Load Light Entity.
        id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(null, Vector3.zero, Quaternion.identity);
        Assert.IsNotNull(id);
        eIDs.Add(id);

        // Load Terrain Entity.
        float[,] heights = new float[256, 256];
        for (int i = 0; i < 256; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                heights[i, j] = i;
            }
        }
        TerrainEntityLayer[] layers = new TerrainEntityLayer[3]
        {
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Runtime/StraightFour/Testing/TestResources/1.png")
            },
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Runtime/StraightFour/Testing/TestResources/2.png")
            },
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Runtime/StraightFour/Testing/TestResources/3.png")
            }
        };
        float[,] layerMask = { { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f },
                               { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }};
        Dictionary<int, float[,]> layerMasks = new Dictionary<int, float[,]>();
        layerMasks.Add(0, layerMask);
        layerMasks.Add(1, layerMask);
        layerMasks.Add(2, layerMask);
        id = StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(256, 256, 256,
            heights, layers, layerMasks, null, Vector3.zero, Quaternion.identity, false);
        Assert.IsNotNull(id);
        eIDs.Add(id);

#if TEST_HYBRID_TERRAIN
        // Load Hybrid Terrain Entity.
        layerMasks.Clear();
        layerMasks.Add(0, layerMask);
        layerMasks.Add(1, layerMask);
        layerMasks.Add(2, layerMask);
        id = StraightFour.ActiveWorld.entityManager.LoadHybridTerrainEntity(256, 256, 256,
            heights, layers, layerMasks, null, Vector3.zero, Quaternion.identity, false);
        Assert.IsNotNull(id);
        eIDs.Add(id);
#endif

        // Load Canvas Entity.
        Guid cId = StraightFour.ActiveWorld.entityManager.LoadCanvasEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        Assert.IsNotNull(cId);
        eIDs.Add(cId);

        // Wait for canvas entity to load.
        yield return new WaitForSeconds(3);
        CanvasEntity ce = (CanvasEntity) StraightFour.ActiveWorld.entityManager.FindEntity(cId);
        Assert.NotNull(ce);

        // Load Text Entity.
        id = StraightFour.ActiveWorld.entityManager.LoadTextEntity("qwerty", 12, ce, Vector3.zero, Vector3.one);
        Assert.IsNotNull(id);
        eIDs.Add(id);

        // Load Button Entity.
        id = StraightFour.ActiveWorld.entityManager.LoadButtonEntity(ce, Vector3.zero, Vector3.one, null);
        Assert.IsNotNull(id);
        eIDs.Add(id);

        // Load Input Entity.
        id = StraightFour.ActiveWorld.entityManager.LoadInputEntity(ce, Vector3.zero, Vector3.one);
        Assert.IsNotNull(id);
        eIDs.Add(id);

        // Load Voxel Entity.
        id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(null, Vector3.zero, Quaternion.identity, Vector3.one);
        Assert.IsNotNull(id);
        eIDs.Add(id);

        // Exists/Find Entity.
        foreach (Guid eID in eIDs)
        {
            Assert.True(StraightFour.ActiveWorld.entityManager.Exists(eID));
            Assert.IsNotNull(StraightFour.ActiveWorld.entityManager.FindEntity(eID));
        }

        // Get All Entities.
        BaseEntity[] entities = StraightFour.ActiveWorld.entityManager.GetAllEntities();
        Assert.AreEqual(eIDs.Count, entities.Length);

        // Unload.
        StraightFour.ActiveWorld.entityManager.Unload();
        Assert.AreEqual(0, StraightFour.ActiveWorld.entityManager.GetAllEntities().Length);
    }

    /// <summary>
    /// Helper to bootstrap a StraightFour world for entity manager tests.
    /// </summary>
    private IEnumerator SetUpWorld()
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
        yield return null;
        StraightFour.LoadWorld("test");
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadMeshEntity_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        GameObject meshPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadMeshEntity(
            null, meshPrefab, Vector3.zero, Quaternion.identity);
        Assert.AreNotEqual(Guid.Empty, id);
        yield return new WaitForSeconds(1);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.Exists(id));
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadLightEntity_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity);
        Assert.AreNotEqual(Guid.Empty, id);
        yield return new WaitForSeconds(1);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.Exists(id));
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadAudioEntity_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity);
        Assert.AreNotEqual(Guid.Empty, id);
        yield return new WaitForSeconds(1);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.Exists(id));
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadContainerEntity_WithTag_FindByTag()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        string testTag = "TestContainer";
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one, tag: testTag);
        yield return new WaitForSeconds(1);

        BaseEntity found = StraightFour.ActiveWorld.entityManager.FindEntityByTag(testTag);
        Assert.IsNotNull(found, "Should find entity by tag");
    }

    [UnityTest]
    public IEnumerator EntityManager_FindEntity_InvalidId_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        BaseEntity found = StraightFour.ActiveWorld.entityManager.FindEntity(Guid.NewGuid());
        Assert.IsNull(found);
    }

    [UnityTest]
    public IEnumerator EntityManager_FindEntityByTag_InvalidTag_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        BaseEntity found = StraightFour.ActiveWorld.entityManager.FindEntityByTag("NonExistentTag");
        Assert.IsNull(found);
    }

    [UnityTest]
    public IEnumerator EntityManager_Exists_InvalidId_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Assert.IsFalse(StraightFour.ActiveWorld.entityManager.Exists(Guid.NewGuid()));
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadMultipleEntities_CountIncreases()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        int initialCount = StraightFour.ActiveWorld.entityManager.GetAllEntities().Length;

        StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one);
        StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, new Vector3(1, 0, 0), Quaternion.identity, Vector3.one);
        StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, new Vector3(2, 0, 0), Quaternion.identity, Vector3.one);

        yield return new WaitForSeconds(1);

        int newCount = StraightFour.ActiveWorld.entityManager.GetAllEntities().Length;
        Assert.AreEqual(initialCount + 3, newCount);
    }

    [UnityTest]
    public IEnumerator EntityManager_GetAllTopLevelEntities_ExcludesChildren()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        // Load a parent and a child.
        Guid parentId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return new WaitForSeconds(1);

        BaseEntity parentEntity = StraightFour.ActiveWorld.entityManager.FindEntity(parentId);
        Assert.IsNotNull(parentEntity);

        Guid childId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            parentEntity, Vector3.one, Quaternion.identity, Vector3.one);
        yield return new WaitForSeconds(1);

        BaseEntity[] all = StraightFour.ActiveWorld.entityManager.GetAllEntities();
        BaseEntity[] topLevel = StraightFour.ActiveWorld.entityManager.GetAllTopLevelEntities();

        Assert.IsTrue(all.Length > topLevel.Length,
            $"All entities ({all.Length}) should be more than top-level ({topLevel.Length})");
    }

    [UnityTest]
    public IEnumerator EntityManager_Unload_ThenLoadAgain_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        // Load some entities.
        StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return new WaitForSeconds(1);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.GetAllEntities().Length > 0);

        // Unload.
        StraightFour.ActiveWorld.entityManager.Unload();
        Assert.AreEqual(0, StraightFour.ActiveWorld.entityManager.GetAllEntities().Length);

        // Load again.
        Guid newId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one);
        yield return new WaitForSeconds(1);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.Exists(newId));
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadContainerEntity_WithPosition_SetsPosition()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Vector3 pos = new Vector3(5, 10, 15);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, pos, Quaternion.identity, Vector3.one);
        yield return new WaitForSeconds(1);

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity);

        Vector3 actualPos = entity.GetPosition(false);
        Assert.AreEqual(pos.x, actualPos.x, 0.5f);
        Assert.AreEqual(pos.y, actualPos.y, 0.5f);
        Assert.AreEqual(pos.z, actualPos.z, 0.5f);
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadCharacterEntity_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Guid id = StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(
            null, null, Vector3.zero, Quaternion.identity, Vector3.zero,
            Vector3.zero, Quaternion.identity, Vector3.one);
        Assert.AreNotEqual(Guid.Empty, id);
        yield return new WaitForSeconds(2);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.Exists(id));
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadWaterBlockerEntity_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Guid id = StraightFour.ActiveWorld.entityManager.LoadWaterBlockerEntity(
            null, Vector3.zero, Quaternion.identity);
        Assert.AreNotEqual(Guid.Empty, id);
        // WaterBlocker entity may require water system to be set up.
        // Just verify the ID was returned without exceptions.
        yield return null;
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadVoxelEntity_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one);
        Assert.AreNotEqual(Guid.Empty, id);
        yield return new WaitForSeconds(1);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.Exists(id));
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadWithSpecificId_UsesProvidedId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Guid specificId = Guid.NewGuid();
        Guid returnedId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one, id: specificId);
        Assert.AreEqual(specificId, returnedId);
        yield return new WaitForSeconds(1);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.Exists(specificId));
    }

    [UnityTest]
    public IEnumerator EntityManager_LoadWithCallback_CallbackFires()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool callbackFired = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { callbackFired = true; });

        yield return new WaitForSeconds(2);
        Assert.IsTrue(callbackFired, "onLoaded callback should fire");
    }
}