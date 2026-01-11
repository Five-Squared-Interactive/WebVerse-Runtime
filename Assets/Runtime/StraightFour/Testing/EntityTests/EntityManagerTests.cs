// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

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
    [TearDown]
    public void TearDown()
    {
        // Clean up any loaded world after each test
        if (StraightFour.ActiveWorld != null)
        {
            StraightFour.UnloadWorld();
        }
    }

    [UnityTest]
    public IEnumerator EntityManagerTests_General()
    {
        List<Guid> eIDs = new List<Guid>();

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        we.voxelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/Voxel/Prefabs/Voxel.prefab");
        we.inputEntityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/StraightFour/Entity/UI/UIElement/Input/Prefabs/InputEntity.prefab");
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
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
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Testing/TestResources/1.png")
            },
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Testing/TestResources/2.png")
            },
            new TerrainEntityLayer()
            {
                diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StraightFour/Testing/TestResources/3.png")
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
}