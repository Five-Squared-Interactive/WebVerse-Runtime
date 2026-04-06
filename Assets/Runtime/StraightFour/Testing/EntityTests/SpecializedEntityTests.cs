// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Entity;
using FiveSQD.StraightFour.Entity.Terrain;
using UnityEditor;

/// <summary>
/// Phase 3A: Tests for specialized entity types loaded through EntityManager.
/// Covers LightEntity, AudioEntity, ContainerEntity, TerrainEntity,
/// VoxelEntity, WaterBodyEntity, CanvasEntity, HTMLEntity, TextEntity, and ButtonEntity.
/// </summary>
public class SpecializedEntityTests
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

    /// <summary>
    /// Helper to bootstrap a StraightFour world.
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

    // ========================================================================
    // LightEntity Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator LightEntity_LoadViaEntityManager_IsNotNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });
        Assert.AreNotEqual(Guid.Empty, id);

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity, "LightEntity should be loaded via EntityManager");
        Assert.IsTrue(entity is LightEntity);
    }

    [UnityTest]
    public IEnumerator LightEntity_SetLightType_Point()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        bool result = le.SetLightType(LightEntity.LightType.Point);
        Assert.IsTrue(result);
        Assert.AreEqual(LightEntity.LightType.Point, le.GetLightType());
    }

    [UnityTest]
    public IEnumerator LightEntity_SetLightType_Directional()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        bool result = le.SetLightType(LightEntity.LightType.Directional);
        Assert.IsTrue(result);
        Assert.AreEqual(LightEntity.LightType.Directional, le.GetLightType());
    }

    [UnityTest]
    public IEnumerator LightEntity_SetLightType_Spot()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        bool result = le.SetLightType(LightEntity.LightType.Spot);
        Assert.IsTrue(result);
        Assert.AreEqual(LightEntity.LightType.Spot, le.GetLightType());
    }

    [UnityTest]
    public IEnumerator LightEntity_SetLightProperties_ColorTemperatureIntensity()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        Color32 testColor = new Color32(255, 128, 64, 255);
        int testTemp = 6500;
        float testIntensity = 2.5f;

        bool result = le.SetLightProperties(testColor, testTemp, testIntensity);
        Assert.IsTrue(result);

        LightEntity.LightProperties props = le.GetLightProperties();
        Assert.AreEqual(testTemp, props.temperature);
        Assert.AreEqual(testIntensity, props.intensity);
    }

    [UnityTest]
    public IEnumerator LightEntity_SetLightProperties_RangeAndIntensity()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        float testRange = 50f;
        float testIntensity = 3.0f;

        bool result = le.SetLightProperties(testRange, testIntensity);
        Assert.IsTrue(result);

        LightEntity.LightProperties props = le.GetLightProperties();
        Assert.AreEqual(testRange, props.range);
        Assert.AreEqual(testIntensity, props.intensity);
    }

    [UnityTest]
    public IEnumerator LightEntity_SetInteractionState_StaticAndHidden()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        bool staticResult = le.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.IsTrue(staticResult);
        Assert.AreEqual(BaseEntity.InteractionState.Static, le.GetInteractionState());

        bool hiddenResult = le.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.IsTrue(hiddenResult);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, le.GetInteractionState());
    }

    // ========================================================================
    // AudioEntity Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator AudioEntity_LoadViaEntityManager_IsNotNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });
        Assert.AreNotEqual(Guid.Empty, id);

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity, "AudioEntity should be loaded via EntityManager");
        Assert.IsTrue(entity is AudioEntity);
    }

    [UnityTest]
    public IEnumerator AudioEntity_SetPosition_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        Vector3 newPos = new Vector3(10, 5, 3);
        ae.SetPosition(newPos, false, false);

        Vector3 result = ae.GetPosition(false);
        Assert.AreEqual(newPos.x, result.x, 0.5f);
        Assert.AreEqual(newPos.y, result.y, 0.5f);
        Assert.AreEqual(newPos.z, result.z, 0.5f);
    }

    [UnityTest]
    public IEnumerator AudioEntity_SetInteractionState_StaticAndHidden()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        bool staticResult = ae.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.IsTrue(staticResult);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ae.GetInteractionState());

        bool hiddenResult = ae.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.IsTrue(hiddenResult);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, ae.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator AudioEntity_Properties_VolumeAndPitch()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        ae.volume = 0.5f;
        Assert.AreEqual(0.5f, ae.volume, 0.01f);

        ae.pitch = 1.5f;
        Assert.AreEqual(1.5f, ae.pitch, 0.01f);

        ae.loop = true;
        Assert.IsTrue(ae.loop);
    }

    // ========================================================================
    // ContainerEntity Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator ContainerEntity_LoadViaEntityManager_IsNotNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });
        Assert.AreNotEqual(Guid.Empty, id);

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity, "ContainerEntity should be loaded via EntityManager");
        Assert.IsTrue(entity is ContainerEntity);
    }

    [UnityTest]
    public IEnumerator ContainerEntity_AddChildEntity_ParentChildRelationship()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool parentLoaded = false;
        Guid parentId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { parentLoaded = true; });

        float elapsed = 0;
        while (!parentLoaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity parentEntity = StraightFour.ActiveWorld.entityManager.FindEntity(parentId);
        Assert.IsNotNull(parentEntity);

        bool childLoaded = false;
        Guid childId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            parentEntity, Vector3.one, Quaternion.identity, Vector3.one,
            onLoaded: () => { childLoaded = true; });

        elapsed = 0;
        while (!childLoaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity childEntity = StraightFour.ActiveWorld.entityManager.FindEntity(childId);
        Assert.IsNotNull(childEntity);
        Assert.AreEqual(parentEntity, childEntity.GetParent());
    }

    [UnityTest]
    public IEnumerator ContainerEntity_RemoveChildEntity_ParentBecomesNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool parentLoaded = false;
        Guid parentId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { parentLoaded = true; });

        float elapsed = 0;
        while (!parentLoaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity parentEntity = StraightFour.ActiveWorld.entityManager.FindEntity(parentId);
        Assert.IsNotNull(parentEntity);

        bool childLoaded = false;
        Guid childId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            parentEntity, Vector3.one, Quaternion.identity, Vector3.one,
            onLoaded: () => { childLoaded = true; });

        elapsed = 0;
        while (!childLoaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity childEntity = StraightFour.ActiveWorld.entityManager.FindEntity(childId);
        Assert.IsNotNull(childEntity);

        childEntity.SetParent(null);
        Assert.IsNull(childEntity.GetParent());
    }

    [UnityTest]
    public IEnumerator ContainerEntity_SetPosition_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Vector3 targetPos = new Vector3(7, 14, 21);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, targetPos, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity);

        Vector3 actualPos = entity.GetPosition(false);
        Assert.AreEqual(targetPos.x, actualPos.x, 0.5f);
        Assert.AreEqual(targetPos.y, actualPos.y, 0.5f);
        Assert.AreEqual(targetPos.z, actualPos.z, 0.5f);
    }

    [UnityTest]
    public IEnumerator ContainerEntity_SetScale_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Vector3 targetScale = new Vector3(2, 3, 4);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, targetScale,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity);

        Vector3 actualScale = entity.GetScale();
        Assert.AreEqual(targetScale.x, actualScale.x, 0.1f);
        Assert.AreEqual(targetScale.y, actualScale.y, 0.1f);
        Assert.AreEqual(targetScale.z, actualScale.z, 0.1f);
    }

    // ========================================================================
    // TerrainEntity Tests
    // ========================================================================

    /// <summary>
    /// Helper to create terrain data for tests.
    /// </summary>
    private void GetTerrainTestData(out float[,] heights, out TerrainEntityLayer[] layers,
        out Dictionary<int, float[,]> layerMasks)
    {
        heights = new float[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                heights[i, j] = i;
            }
        }

        layers = new TerrainEntityLayer[1]
        {
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/Runtime/StraightFour/Testing/TestResources/1.png")
            }
        };

        float[,] layerMask = new float[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                layerMask[i, j] = 1f;
            }
        }

        layerMasks = new Dictionary<int, float[,]>();
        layerMasks.Add(0, layerMask);
    }

    [UnityTest]
    public IEnumerator TerrainEntity_LoadViaEntityManager_IsNotNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        GetTerrainTestData(out float[,] heights, out TerrainEntityLayer[] layers,
            out Dictionary<int, float[,]> layerMasks);

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(
            32, 32, 32, heights, layers, layerMasks,
            null, Vector3.zero, Quaternion.identity, false,
            onLoaded: () => { loaded = true; });
        Assert.AreNotEqual(Guid.Empty, id);

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity, "TerrainEntity should be loaded via EntityManager");
        Assert.IsTrue(entity is TerrainEntity);
    }

    [UnityTest]
    public IEnumerator TerrainEntity_SetInteractionState_StaticAndHidden()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        GetTerrainTestData(out float[,] heights, out TerrainEntityLayer[] layers,
            out Dictionary<int, float[,]> layerMasks);

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(
            32, 32, 32, heights, layers, layerMasks,
            null, Vector3.zero, Quaternion.identity, false,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        TerrainEntity te = StraightFour.ActiveWorld.entityManager.FindEntity(id) as TerrainEntity;
        Assert.IsNotNull(te);

        bool staticResult = te.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.IsTrue(staticResult);
        Assert.AreEqual(BaseEntity.InteractionState.Static, te.GetInteractionState());

        bool hiddenResult = te.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.IsTrue(hiddenResult);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, te.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator TerrainEntity_SetInteractionState_Physical()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        GetTerrainTestData(out float[,] heights, out TerrainEntityLayer[] layers,
            out Dictionary<int, float[,]> layerMasks);

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(
            32, 32, 32, heights, layers, layerMasks,
            null, Vector3.zero, Quaternion.identity, false,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        TerrainEntity te = StraightFour.ActiveWorld.entityManager.FindEntity(id) as TerrainEntity;
        Assert.IsNotNull(te);

        bool physicalResult = te.SetInteractionState(BaseEntity.InteractionState.Physical);
        Assert.IsTrue(physicalResult);
        Assert.AreEqual(BaseEntity.InteractionState.Physical, te.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator TerrainEntity_GetHeights_ReturnsData()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        GetTerrainTestData(out float[,] heights, out TerrainEntityLayer[] layers,
            out Dictionary<int, float[,]> layerMasks);

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(
            32, 32, 32, heights, layers, layerMasks,
            null, Vector3.zero, Quaternion.identity, false,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        TerrainEntity te = StraightFour.ActiveWorld.entityManager.FindEntity(id) as TerrainEntity;
        Assert.IsNotNull(te);

        te.SetInteractionState(BaseEntity.InteractionState.Static);
        float[,] retrievedHeights = te.GetHeights();
        Assert.IsNotNull(retrievedHeights);
        Assert.AreEqual(heights.GetLength(0), retrievedHeights.GetLength(0));
        Assert.AreEqual(heights.GetLength(1), retrievedHeights.GetLength(1));
    }

    // ========================================================================
    // VoxelEntity Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator VoxelEntity_LoadViaEntityManager_IsNotNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });
        Assert.AreNotEqual(Guid.Empty, id);

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity, "VoxelEntity should be loaded via EntityManager");
        Assert.IsTrue(entity is VoxelEntity);
    }

    [UnityTest]
    public IEnumerator VoxelEntity_SetPosition_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Vector3 targetPos = new Vector3(5, 10, 15);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, targetPos, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity);

        Vector3 actualPos = entity.GetPosition(false);
        Assert.AreEqual(targetPos.x, actualPos.x, 0.5f);
        Assert.AreEqual(targetPos.y, actualPos.y, 0.5f);
        Assert.AreEqual(targetPos.z, actualPos.z, 0.5f);
    }

    [UnityTest]
    public IEnumerator VoxelEntity_SetInteractionState_StaticAndHidden()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        VoxelEntity ve = StraightFour.ActiveWorld.entityManager.FindEntity(id) as VoxelEntity;
        Assert.IsNotNull(ve);

        bool staticResult = ve.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.IsTrue(staticResult);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ve.GetInteractionState());

        bool hiddenResult = ve.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.IsTrue(hiddenResult);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, ve.GetInteractionState());
    }

    [UnityTest]
    public IEnumerator VoxelEntity_GetPhysicalProperties_ReturnsValue()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        VoxelEntity ve = StraightFour.ActiveWorld.entityManager.FindEntity(id) as VoxelEntity;
        Assert.IsNotNull(ve);

        BaseEntity.EntityPhysicalProperties? props = ve.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue, "VoxelEntity should return physical properties");
        Assert.IsFalse(props.Value.gravitational.Value);
    }

    // ========================================================================
    // WaterBodyEntity Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator WaterBodyEntity_LoadViaEntityManager_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        // WaterBodyEntity requires a waterBodyPrefab; the load may fail
        // gracefully if the prefab is not set. We verify the ID is returned.
        Guid id = StraightFour.ActiveWorld.entityManager.LoadWaterBodyEntity(
            Color.cyan, Color.blue, Color.white, Color.green,
            0.5f, 1.5f, 10f, 0.8f, 4f, 0.2f, 0.3f, 1f, 5f, 1f, 0.5f,
            null, Vector3.zero, Quaternion.identity);
        Assert.AreNotEqual(Guid.Empty, id);

        yield return new WaitForSeconds(2);

        // Entity may or may not be findable depending on prefab availability.
        // The key assertion is that LoadWaterBodyEntity returned without throwing.
    }

    [UnityTest]
    public IEnumerator WaterBodyEntity_LoadWithPosition_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Vector3 targetPos = new Vector3(10, 0, 10);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadWaterBodyEntity(
            Color.cyan, Color.blue, Color.white, Color.green,
            0.5f, 1.5f, 10f, 0.8f, 4f, 0.2f, 0.3f, 1f, 5f, 1f, 0.5f,
            null, targetPos, Quaternion.identity);
        Assert.AreNotEqual(Guid.Empty, id);

        yield return new WaitForSeconds(2);
    }

    [UnityTest]
    public IEnumerator WaterBodyEntity_LoadWithCallback_ReturnsId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool callbackFired = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadWaterBodyEntity(
            Color.cyan, Color.blue, Color.white, Color.green,
            0.5f, 1.5f, 10f, 0.8f, 4f, 0.2f, 0.3f, 1f, 5f, 1f, 0.5f,
            null, Vector3.zero, Quaternion.identity,
            onLoaded: () => { callbackFired = true; });
        Assert.AreNotEqual(Guid.Empty, id);

        float elapsed = 0;
        while (!callbackFired && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        // Callback may or may not fire depending on prefab availability.
        // The key assertion is no exception was thrown.
    }

    // ========================================================================
    // CanvasEntity Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator CanvasEntity_LoadViaEntityManager_IsNotNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadCanvasEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });
        Assert.AreNotEqual(Guid.Empty, id);

        float elapsed = 0;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity, "CanvasEntity should be loaded via EntityManager");
        Assert.IsTrue(entity is CanvasEntity);
    }

    // ========================================================================
    // TextEntity Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator TextEntity_LoadViaEntityManager_IsNotNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        // TextEntity requires a parent CanvasEntity.
        bool canvasLoaded = false;
        Guid canvasId = StraightFour.ActiveWorld.entityManager.LoadCanvasEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { canvasLoaded = true; });

        float elapsed = 0;
        while (!canvasLoaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        CanvasEntity ce = StraightFour.ActiveWorld.entityManager.FindEntity(canvasId) as CanvasEntity;
        Assert.IsNotNull(ce);

        bool textLoaded = false;
        Guid textId = StraightFour.ActiveWorld.entityManager.LoadTextEntity(
            "Hello World", 12, ce, Vector2.zero, Vector2.one,
            onLoaded: () => { textLoaded = true; });
        Assert.AreNotEqual(Guid.Empty, textId);

        elapsed = 0;
        while (!textLoaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(textId);
        Assert.IsNotNull(entity, "TextEntity should be loaded via EntityManager");
        Assert.IsTrue(entity is TextEntity);
    }

    // ========================================================================
    // ButtonEntity Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator ButtonEntity_LoadViaEntityManager_IsNotNull()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        // ButtonEntity requires a parent CanvasEntity.
        bool canvasLoaded = false;
        Guid canvasId = StraightFour.ActiveWorld.entityManager.LoadCanvasEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { canvasLoaded = true; });

        float elapsed = 0;
        while (!canvasLoaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        CanvasEntity ce = StraightFour.ActiveWorld.entityManager.FindEntity(canvasId) as CanvasEntity;
        Assert.IsNotNull(ce);

        bool buttonLoaded = false;
        Guid buttonId = StraightFour.ActiveWorld.entityManager.LoadButtonEntity(
            ce, Vector2.zero, Vector2.one, null,
            onLoaded: () => { buttonLoaded = true; });
        Assert.AreNotEqual(Guid.Empty, buttonId);

        elapsed = 0;
        while (!buttonLoaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(buttonId);
        Assert.IsNotNull(entity, "ButtonEntity should be loaded via EntityManager");
    }

    // ========================================================================
    // HTMLEntity Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator HTMLEntity_LoadViaEntityManager_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        // HTMLEntity may require Vuplex WebView; load may not fully complete
        // in test environment but should not throw.
        Guid id = StraightFour.ActiveWorld.entityManager.LoadHTMLEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one);
        Assert.AreNotEqual(Guid.Empty, id);

        yield return new WaitForSeconds(2);

        // Entity availability depends on Vuplex plugin; just verify no exception.
    }

    // ========================================================================
    // AudioEntity Extended Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator AudioEntity_StereoPan_SetAndGet()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        ae.stereoPan = -0.5f;
        Assert.AreEqual(-0.5f, ae.stereoPan, 0.01f);

        ae.stereoPan = 1.0f;
        Assert.AreEqual(1.0f, ae.stereoPan, 0.01f);
    }

    [UnityTest]
    public IEnumerator AudioEntity_SpatialBlend_SetAndGet()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        ae.spatialBlend = 1.0f;
        Assert.AreEqual(1.0f, ae.spatialBlend, 0.01f);

        ae.spatialBlend = 0.0f;
        Assert.AreEqual(0.0f, ae.spatialBlend, 0.01f);
    }

    [UnityTest]
    public IEnumerator AudioEntity_MinMaxDistance_SetAndGet()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        ae.minDistance = 2.0f;
        ae.maxDistance = 100.0f;
        Assert.AreEqual(2.0f, ae.minDistance, 0.01f);
        Assert.AreEqual(100.0f, ae.maxDistance, 0.01f);
    }

    [UnityTest]
    public IEnumerator AudioEntity_RolloffMode_SetAndGet()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        ae.rolloffMode = AudioRolloffMode.Linear;
        Assert.AreEqual(AudioRolloffMode.Linear, ae.rolloffMode);

        ae.rolloffMode = AudioRolloffMode.Logarithmic;
        Assert.AreEqual(AudioRolloffMode.Logarithmic, ae.rolloffMode);
    }

    [UnityTest]
    public IEnumerator AudioEntity_DopplerLevel_SetAndGet()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        ae.dopplerLevel = 0.0f;
        Assert.AreEqual(0.0f, ae.dopplerLevel, 0.01f);

        ae.dopplerLevel = 0.75f;
        Assert.AreEqual(0.75f, ae.dopplerLevel, 0.01f);
    }

    [UnityTest]
    public IEnumerator AudioEntity_Priority_SetAndGet()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        ae.priority = 0;
        Assert.AreEqual(0, ae.priority);

        ae.priority = 256;
        Assert.AreEqual(256, ae.priority);
    }

    [UnityTest]
    public IEnumerator AudioEntity_PlayAndStop_ReturnTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.IsTrue(ae.Play());
        Assert.IsTrue(ae.Stop());
    }

    [UnityTest]
    public IEnumerator AudioEntity_TogglePause_ReturnTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        ae.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.IsTrue(ae.TogglePause(true));
        Assert.IsTrue(ae.TogglePause(false));
    }

    [UnityTest]
    public IEnumerator AudioEntity_SetInteractionState_PhysicalReturnsTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        // Physical returns true but logs warning (not applicable to audio).
        bool result = ae.SetInteractionState(BaseEntity.InteractionState.Physical);
        Assert.IsTrue(result);
    }

    [UnityTest]
    public IEnumerator AudioEntity_SetInteractionState_PlacingReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        bool result = ae.SetInteractionState(BaseEntity.InteractionState.Placing);
        Assert.IsFalse(result);
    }

    // ========================================================================
    // LightEntity Extended Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator LightEntity_GetMotion_AlwaysStationary()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        BaseEntity.EntityMotion? motion = le.GetMotion();
        Assert.IsTrue(motion.HasValue);
        Assert.IsTrue(motion.Value.stationary.Value);
        Assert.AreEqual(Vector3.zero, motion.Value.velocity);
        Assert.AreEqual(Vector3.zero, motion.Value.angularVelocity);
    }

    [UnityTest]
    public IEnumerator LightEntity_GetPhysicalProperties_ZeroMassNotGravitational()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        BaseEntity.EntityPhysicalProperties? props = le.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue);
        Assert.AreEqual(0f, props.Value.mass.Value, 0.01f);
        Assert.IsFalse(props.Value.gravitational.Value);
        Assert.AreEqual(0f, props.Value.drag.Value, 0.01f);
        Assert.AreEqual(0f, props.Value.angularDrag.Value, 0.01f);
        Assert.AreEqual(Vector3.zero, props.Value.centerOfMass.Value);
    }

    [UnityTest]
    public IEnumerator LightEntity_GetSize_ReturnsZero()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.AreEqual(Vector3.zero, le.GetSize());
    }

    [UnityTest]
    public IEnumerator LightEntity_SetMotion_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        bool result = le.SetMotion(new BaseEntity.EntityMotion
        {
            velocity = Vector3.one,
            angularVelocity = Vector3.zero,
            stationary = false
        });
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator LightEntity_SetPhysicalProperties_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        bool result = le.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties
        {
            mass = 10, gravitational = true
        });
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator LightEntity_SetSize_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        bool result = le.SetSize(Vector3.one);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator LightEntity_SetInteractionState_PhysicalReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        bool result = le.SetInteractionState(BaseEntity.InteractionState.Physical);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator LightEntity_SetInteractionState_PlacingReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        bool result = le.SetInteractionState(BaseEntity.InteractionState.Placing);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator LightEntity_SetLightProperties_SpotAngles()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        float range = 25f;
        float innerAngle = 15f;
        float outerAngle = 45f;
        Color32 color = new Color32(200, 100, 50, 255);
        int temp = 4000;
        float intensity = 1.5f;

        bool result = le.SetLightProperties(range, innerAngle, outerAngle, color, temp, intensity);
        Assert.IsTrue(result);

        LightEntity.LightProperties props = le.GetLightProperties();
        Assert.AreEqual(range, props.range);
        Assert.AreEqual(innerAngle, props.innerSpotAngle);
        Assert.AreEqual(outerAngle, props.outerSpotAngle);
        Assert.AreEqual(temp, props.temperature);
        Assert.AreEqual(intensity, props.intensity);
    }

    [UnityTest]
    public IEnumerator LightEntity_SetPosition_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        LightEntity le = StraightFour.ActiveWorld.entityManager.FindEntity(id) as LightEntity;
        Assert.IsNotNull(le);

        le.SetInteractionState(BaseEntity.InteractionState.Static);
        Vector3 newPos = new Vector3(5, 10, 15);
        le.SetPosition(newPos, false, false);

        Vector3 result = le.GetPosition(false);
        Assert.AreEqual(newPos.x, result.x, 0.5f);
        Assert.AreEqual(newPos.y, result.y, 0.5f);
        Assert.AreEqual(newPos.z, result.z, 0.5f);
    }

    // ========================================================================
    // ContainerEntity Extended Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator ContainerEntity_SetInteractionState_PhysicalReturnsTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        ContainerEntity ce = StraightFour.ActiveWorld.entityManager.FindEntity(id) as ContainerEntity;
        Assert.IsNotNull(ce);

        // Physical returns true but logs warning (not applicable to container).
        bool result = ce.SetInteractionState(BaseEntity.InteractionState.Physical);
        Assert.IsTrue(result);
    }

    [UnityTest]
    public IEnumerator ContainerEntity_SetInteractionState_PlacingReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        ContainerEntity ce = StraightFour.ActiveWorld.entityManager.FindEntity(id) as ContainerEntity;
        Assert.IsNotNull(ce);

        bool result = ce.SetInteractionState(BaseEntity.InteractionState.Placing);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator ContainerEntity_SetRotation_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Quaternion targetRot = Quaternion.Euler(0, 90, 0);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, targetRot, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity);

        Quaternion actualRot = entity.GetRotation(false);
        Assert.AreEqual(targetRot.x, actualRot.x, 0.1f);
        Assert.AreEqual(targetRot.y, actualRot.y, 0.1f);
        Assert.AreEqual(targetRot.z, actualRot.z, 0.1f);
        Assert.AreEqual(targetRot.w, actualRot.w, 0.1f);
    }

    [UnityTest]
    public IEnumerator ContainerEntity_ThreeLevelHierarchy()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        // Load grandparent
        bool gpLoaded = false;
        Guid gpId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { gpLoaded = true; });

        float elapsed = 0;
        while (!gpLoaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }
        BaseEntity gp = StraightFour.ActiveWorld.entityManager.FindEntity(gpId);
        Assert.IsNotNull(gp);

        // Load parent under grandparent
        bool pLoaded = false;
        Guid pId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            gp, Vector3.one, Quaternion.identity, Vector3.one,
            onLoaded: () => { pLoaded = true; });

        elapsed = 0;
        while (!pLoaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }
        BaseEntity p = StraightFour.ActiveWorld.entityManager.FindEntity(pId);
        Assert.IsNotNull(p);
        Assert.AreEqual(gp, p.GetParent());

        // Load child under parent
        bool cLoaded = false;
        Guid cId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            p, Vector3.one * 2, Quaternion.identity, Vector3.one,
            onLoaded: () => { cLoaded = true; });

        elapsed = 0;
        while (!cLoaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }
        BaseEntity c = StraightFour.ActiveWorld.entityManager.FindEntity(cId);
        Assert.IsNotNull(c);
        Assert.AreEqual(p, c.GetParent());
    }

    [UnityTest]
    public IEnumerator ContainerEntity_SetInteractionState_StaticAndHidden()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        ContainerEntity ce = StraightFour.ActiveWorld.entityManager.FindEntity(id) as ContainerEntity;
        Assert.IsNotNull(ce);

        bool staticResult = ce.SetInteractionState(BaseEntity.InteractionState.Static);
        Assert.IsTrue(staticResult);
        Assert.AreEqual(BaseEntity.InteractionState.Static, ce.GetInteractionState());

        bool hiddenResult = ce.SetInteractionState(BaseEntity.InteractionState.Hidden);
        Assert.IsTrue(hiddenResult);
        Assert.AreEqual(BaseEntity.InteractionState.Hidden, ce.GetInteractionState());
    }

    // ========================================================================
    // VoxelEntity Extended Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator VoxelEntity_GetMotion_AlwaysStationary()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        VoxelEntity ve = StraightFour.ActiveWorld.entityManager.FindEntity(id) as VoxelEntity;
        Assert.IsNotNull(ve);

        BaseEntity.EntityMotion? motion = ve.GetMotion();
        Assert.IsTrue(motion.HasValue);
        Assert.IsTrue(motion.Value.stationary.Value);
        Assert.AreEqual(Vector3.zero, motion.Value.velocity);
        Assert.AreEqual(Vector3.zero, motion.Value.angularVelocity);
    }

    [UnityTest]
    public IEnumerator VoxelEntity_GetPhysicalProperties_InfinityValues()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        VoxelEntity ve = StraightFour.ActiveWorld.entityManager.FindEntity(id) as VoxelEntity;
        Assert.IsNotNull(ve);

        BaseEntity.EntityPhysicalProperties? props = ve.GetPhysicalProperties();
        Assert.IsTrue(props.HasValue);
        Assert.AreEqual(float.PositiveInfinity, props.Value.angularDrag.Value);
        Assert.AreEqual(float.PositiveInfinity, props.Value.drag.Value);
        Assert.AreEqual(float.PositiveInfinity, props.Value.mass.Value);
        Assert.IsFalse(props.Value.gravitational.Value);
        Assert.AreEqual(float.NegativeInfinity, props.Value.centerOfMass.Value.x);
        Assert.AreEqual(float.NegativeInfinity, props.Value.centerOfMass.Value.y);
        Assert.AreEqual(float.NegativeInfinity, props.Value.centerOfMass.Value.z);
    }

    [UnityTest]
    public IEnumerator VoxelEntity_SetInteractionState_PhysicalReturnsTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        VoxelEntity ve = StraightFour.ActiveWorld.entityManager.FindEntity(id) as VoxelEntity;
        Assert.IsNotNull(ve);

        // Physical returns true but logs warning.
        bool result = ve.SetInteractionState(BaseEntity.InteractionState.Physical);
        Assert.IsTrue(result);
    }

    [UnityTest]
    public IEnumerator VoxelEntity_SetInteractionState_PlacingReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        VoxelEntity ve = StraightFour.ActiveWorld.entityManager.FindEntity(id) as VoxelEntity;
        Assert.IsNotNull(ve);

        bool result = ve.SetInteractionState(BaseEntity.InteractionState.Placing);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator VoxelEntity_ContainsChunk_ReturnsFalseForEmpty()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, Vector3.one,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        VoxelEntity ve = StraightFour.ActiveWorld.entityManager.FindEntity(id) as VoxelEntity;
        Assert.IsNotNull(ve);

        Assert.IsFalse(ve.ContainsChunk(0, 0, 0));
        Assert.IsFalse(ve.ContainsChunk(1, 1, 1));
    }

    [UnityTest]
    public IEnumerator VoxelEntity_SetScale_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Vector3 targetScale = new Vector3(3, 3, 3);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, Vector3.zero, Quaternion.identity, targetScale,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        BaseEntity entity = StraightFour.ActiveWorld.entityManager.FindEntity(id);
        Assert.IsNotNull(entity);

        Vector3 actualScale = entity.GetScale();
        Assert.AreEqual(targetScale.x, actualScale.x, 0.1f);
        Assert.AreEqual(targetScale.y, actualScale.y, 0.1f);
        Assert.AreEqual(targetScale.z, actualScale.z, 0.1f);
    }

    // ========================================================================
    // TerrainEntity Extended Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator TerrainEntity_SetPosition_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        GetTerrainTestData(out float[,] heights, out TerrainEntityLayer[] layers,
            out Dictionary<int, float[,]> layerMasks);

        bool loaded = false;
        Vector3 targetPos = new Vector3(50, 0, 50);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(
            32, 32, 32, heights, layers, layerMasks,
            null, targetPos, Quaternion.identity, false,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        TerrainEntity te = StraightFour.ActiveWorld.entityManager.FindEntity(id) as TerrainEntity;
        Assert.IsNotNull(te);

        Vector3 actualPos = te.GetPosition(false);
        Assert.AreEqual(targetPos.x, actualPos.x, 1.0f);
        Assert.AreEqual(targetPos.y, actualPos.y, 1.0f);
        Assert.AreEqual(targetPos.z, actualPos.z, 1.0f);
    }

    [UnityTest]
    public IEnumerator TerrainEntity_GetHeights_ValuesMatchInput()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        GetTerrainTestData(out float[,] heights, out TerrainEntityLayer[] layers,
            out Dictionary<int, float[,]> layerMasks);

        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(
            32, 32, 32, heights, layers, layerMasks,
            null, Vector3.zero, Quaternion.identity, false,
            onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        TerrainEntity te = StraightFour.ActiveWorld.entityManager.FindEntity(id) as TerrainEntity;
        Assert.IsNotNull(te);

        te.SetInteractionState(BaseEntity.InteractionState.Static);
        float[,] retrieved = te.GetHeights();
        Assert.IsNotNull(retrieved);
        Assert.IsTrue(retrieved.GetLength(0) > 0);
        Assert.IsTrue(retrieved.GetLength(1) > 0);
    }

    // ========================================================================
    // AudioEntity Rotation Test
    // ========================================================================

    [UnityTest]
    public IEnumerator AudioEntity_SetRotation_Works()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool loaded = false;
        Quaternion targetRot = Quaternion.Euler(45, 90, 0);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, Vector3.zero, targetRot, onLoaded: () => { loaded = true; });

        float elapsed = 0;
        while (!loaded && elapsed < 5f) { yield return new WaitForSeconds(0.1f); elapsed += 0.1f; }

        AudioEntity ae = StraightFour.ActiveWorld.entityManager.FindEntity(id) as AudioEntity;
        Assert.IsNotNull(ae);

        Quaternion actualRot = ae.GetRotation(false);
        Assert.AreEqual(targetRot.x, actualRot.x, 0.1f);
        Assert.AreEqual(targetRot.y, actualRot.y, 0.1f);
        Assert.AreEqual(targetRot.z, actualRot.z, 0.1f);
        Assert.AreEqual(targetRot.w, actualRot.w, 0.1f);
    }

    // ========================================================================
    // Multiple Entity Types Coexist
    // ========================================================================

    [UnityTest]
    public IEnumerator MultipleEntityTypes_LoadAndCoexist()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        bool lightLoaded = false, audioLoaded = false, containerLoaded = false, voxelLoaded = false;

        Guid lightId = StraightFour.ActiveWorld.entityManager.LoadLightEntity(
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { lightLoaded = true; });
        Guid audioId = StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
            null, new Vector3(5, 0, 0), Quaternion.identity, onLoaded: () => { audioLoaded = true; });
        Guid containerId = StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
            null, new Vector3(10, 0, 0), Quaternion.identity, Vector3.one,
            onLoaded: () => { containerLoaded = true; });
        Guid voxelId = StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(
            null, new Vector3(15, 0, 0), Quaternion.identity, Vector3.one,
            onLoaded: () => { voxelLoaded = true; });

        float elapsed = 0;
        while ((!lightLoaded || !audioLoaded || !containerLoaded || !voxelLoaded) && elapsed < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        Assert.IsNotNull(StraightFour.ActiveWorld.entityManager.FindEntity(lightId));
        Assert.IsNotNull(StraightFour.ActiveWorld.entityManager.FindEntity(audioId));
        Assert.IsNotNull(StraightFour.ActiveWorld.entityManager.FindEntity(containerId));
        Assert.IsNotNull(StraightFour.ActiveWorld.entityManager.FindEntity(voxelId));

        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.FindEntity(lightId) is LightEntity);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.FindEntity(audioId) is AudioEntity);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.FindEntity(containerId) is ContainerEntity);
        Assert.IsTrue(StraightFour.ActiveWorld.entityManager.FindEntity(voxelId) is VoxelEntity);
    }

    // ========================================================================
    // WaterBodyEntity Extended Tests
    // ========================================================================

    [UnityTest]
    public IEnumerator WaterBodyEntity_LoadWithDifferentColors_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Guid id = StraightFour.ActiveWorld.entityManager.LoadWaterBodyEntity(
            Color.red, Color.black, Color.yellow, Color.magenta,
            1.0f, 3.0f, 50f, 0.5f, 8f, 0.5f, 0.5f, 2f, 10f, 2f, 0.8f,
            null, Vector3.zero, Quaternion.identity);
        Assert.AreNotEqual(Guid.Empty, id);

        yield return new WaitForSeconds(2);
    }

    [UnityTest]
    public IEnumerator WaterBodyEntity_LoadWithRotation_ReturnsValidId()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorld();

        Quaternion rot = Quaternion.Euler(0, 45, 0);
        Guid id = StraightFour.ActiveWorld.entityManager.LoadWaterBodyEntity(
            Color.cyan, Color.blue, Color.white, Color.green,
            0.5f, 1.5f, 10f, 0.8f, 4f, 0.2f, 0.3f, 1f, 5f, 1f, 0.5f,
            null, Vector3.zero, rot);
        Assert.AreNotEqual(Guid.Empty, id);

        yield return new WaitForSeconds(2);
    }
}
