// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using UnityEditor;

public class CameraTests
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
    public IEnumerator CameraTests_General()
    {
        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        Assert.IsNotNull(StraightFour.ActiveWorld.cameraManager);

        GameObject parentGO = new GameObject("parent");

        // Set Parent.
        StraightFour.ActiveWorld.cameraManager.SetParent(parentGO);
        Assert.AreEqual(parentGO.transform, StraightFour.ActiveWorld.cameraManager.cam.transform.parent);

        // Set Position.
        StraightFour.ActiveWorld.cameraManager.SetPosition(new Vector3(1, 2, 3), false);
        Assert.AreEqual(new Vector3(1, 2, 3), StraightFour.ActiveWorld.cameraManager.cam.transform.position);
        StraightFour.ActiveWorld.cameraManager.SetPosition(new Vector3(3, 4, 5), true);
        Assert.AreEqual(new Vector3(3, 4, 5), StraightFour.ActiveWorld.cameraManager.cam.transform.localPosition);

        // Set Rotation.
        StraightFour.ActiveWorld.cameraManager.SetRotation(new Quaternion(0.1f, 0.2f, 0.3f, 0.4f), false);
        StraightFour.ActiveWorld.cameraManager.SetRotation(new Quaternion(0.3f, 0.4f, 0.5f, 0.6f), true);

        // Set Euler Rotation.
        StraightFour.ActiveWorld.cameraManager.SetEulerRotation(new Vector3(1, 2, 3), false);
        StraightFour.ActiveWorld.cameraManager.SetEulerRotation(new Vector3(4, 5, 6), true);

        // Set Scale.
        StraightFour.ActiveWorld.cameraManager.SetScale(new Vector3(1, 2, 3));
        Assert.AreEqual(new Vector3(1, 2, 3), StraightFour.ActiveWorld.cameraManager.cam.transform.localScale);
    }
}