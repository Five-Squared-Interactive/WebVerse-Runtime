// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Entity;
using UnityEditor;

/// <summary>
/// Story 1b integration tests for fill-to-height on a live WaterBodyEntity. Uses a local
/// world setup that wires waterBodyPrefab (the shared SpecializedEntityTests.SetUpWorld does
/// not), so a WaterBodyEntity can actually instantiate.
/// </summary>
public class WaterFillIntegrationTests
{
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
            // Manager may already be torn down.
        }
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

    private IEnumerator LoadWaterBody(Action<WaterBodyEntity> onReady)
    {
        bool loaded = false;
        Guid id = StraightFour.ActiveWorld.entityManager.LoadWaterBodyEntity(
            Color.cyan, Color.blue, Color.white, Color.cyan,
            0.5f, 10f, 32f, 0.8f, 4f, 0.3f, 0.5f, 1f, 10f, 1f, 0.7f,
            null, Vector3.zero, Quaternion.identity, onLoaded: () => { loaded = true; });
        Assert.AreNotEqual(Guid.Empty, id);

        float elapsed = 0f;
        while (!loaded && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        WaterBodyEntity wbe = StraightFour.ActiveWorld.entityManager.FindEntity(id) as WaterBodyEntity;
        onReady(wbe);
    }

    [UnityTest]
    public IEnumerator FitToBounds_SetsFootprintAndLevel()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();

        WaterBodyEntity wbe = null;
        yield return LoadWaterBody(e => wbe = e);
        Assert.IsNotNull(wbe, "WaterBodyEntity should load with waterBodyPrefab wired.");

        Bounds basin = new Bounds(new Vector3(0f, 2f, 0f), new Vector3(20f, 4f, 20f));
        bool ok = wbe.FitToBounds(basin, 3f);

        Assert.IsTrue(ok);
        Assert.IsTrue(wbe.HasWaterVolume());
        Assert.AreEqual(3f, wbe.GetWaterLevel(), 0.0001f);
        Assert.AreEqual(basin.min.x, wbe.GetFootprint().min.x, 0.0001f);
        Assert.AreEqual(basin.max.z, wbe.GetFootprint().max.z, 0.0001f);
        Assert.AreEqual(3f, wbe.transform.position.y, 0.0001f);
    }

    [UnityTest]
    public IEnumerator SetWaterLevel_MovesSurfaceOnly()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();

        WaterBodyEntity wbe = null;
        yield return LoadWaterBody(e => wbe = e);
        Assert.IsNotNull(wbe);

        Bounds basin = new Bounds(new Vector3(0f, 2f, 0f), new Vector3(20f, 4f, 20f));
        wbe.FitToBounds(basin, 3f);
        Bounds footprintBefore = wbe.GetFootprint();

        bool ok = wbe.SetWaterLevel(5f);

        Assert.IsTrue(ok);
        Assert.AreEqual(5f, wbe.GetWaterLevel(), 0.0001f);
        Assert.AreEqual(5f, wbe.transform.position.y, 0.0001f);
        Assert.AreEqual(footprintBefore.min.x, wbe.GetFootprint().min.x, 0.0001f);
        Assert.AreEqual(footprintBefore.max.z, wbe.GetFootprint().max.z, 0.0001f);
    }

    [UnityTest]
    public IEnumerator SetWaterLevel_BeforeFill_DoesNotRegisterDegenerateVolume()
    {
        LogAssert.ignoreFailingMessages = true;
        BuoyancyController.ResetForTests();
        yield return SetUpWorldWithWater();

        WaterBodyEntity wbe = null;
        yield return LoadWaterBody(e => wbe = e);
        Assert.IsNotNull(wbe);

        // SetWaterLevel before any fill must not fabricate a zero-area footprint or register.
        bool ok = wbe.SetWaterLevel(4f);
        Assert.IsTrue(ok);
        Assert.IsFalse(wbe.HasWaterVolume(), "No footprint exists yet, so no volume should be active.");
        Assert.AreEqual(4f, wbe.GetWaterLevel(), 0.0001f);
        Assert.AreEqual(4f, wbe.transform.position.y, 0.0001f);
        BuoyancyController.ResetForTests();
    }

    [UnityTest]
    public IEnumerator FitToEntity_NullTarget_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        yield return SetUpWorldWithWater();

        WaterBodyEntity wbe = null;
        yield return LoadWaterBody(e => wbe = e);
        Assert.IsNotNull(wbe);

        bool ok = wbe.FitToEntity(null, 3f);
        Assert.IsFalse(ok);
        Assert.IsFalse(wbe.HasWaterVolume());
    }

    [UnityTest]
    public IEnumerator TryGetWorldRendererBounds_AxisAlignedCube_MatchesPosition()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(5f, 0f, 0f);
        yield return null;

        bool found = WaterFill.TryGetWorldRendererBounds(cube, out Bounds b);

        Assert.IsTrue(found);
        Assert.AreEqual(5f, b.center.x, 0.05f);
        Assert.AreEqual(1f, b.size.x, 0.05f);
        Assert.AreEqual(1f, b.size.z, 0.05f);
        UnityEngine.Object.DestroyImmediate(cube);
    }

    [UnityTest]
    public IEnumerator TryGetWorldRendererBounds_RotatedCube_WiderAabb()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        yield return null;

        bool found = WaterFill.TryGetWorldRendererBounds(cube, out Bounds b);

        Assert.IsTrue(found);
        // A unit cube rotated 45 deg about Y has an XZ AABB of ~sqrt(2) per side.
        Assert.Greater(b.size.x, 1.3f);
        Assert.Greater(b.size.z, 1.3f);
        UnityEngine.Object.DestroyImmediate(cube);
    }
}
