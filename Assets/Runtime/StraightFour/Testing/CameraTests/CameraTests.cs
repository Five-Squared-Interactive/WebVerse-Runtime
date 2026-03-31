// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using UnityEditor;

public class CameraTests
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
        // Clean up any loaded world after each test
        try
        {
            if (StraightFour.ActiveWorld != null)
            {
                StraightFour.UnloadWorld();
            }
        }
        catch (System.Exception) { }
    }

    [UnityTest]
    public IEnumerator CameraTests_General()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;

        // Verify Camera.main is available before loading the world
        Assert.IsNotNull(Camera.main, "Camera.main must be available before LoadWorld");

        StraightFour.LoadWorld("test");

        Assert.IsNotNull(StraightFour.ActiveWorld.cameraManager);
        Assert.IsNotNull(StraightFour.ActiveWorld.cameraManager.cam, "CameraManager.cam must not be null");

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

    [UnityTest]
    public IEnumerator CameraTests_GetPosition()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set and get world position.
        StraightFour.ActiveWorld.cameraManager.SetPosition(new Vector3(10, 20, 30), false);
        Vector3 worldPos = StraightFour.ActiveWorld.cameraManager.GetPosition(false);
        Assert.AreEqual(new Vector3(10, 20, 30), worldPos);

        // Set and get local position.
        StraightFour.ActiveWorld.cameraManager.SetPosition(new Vector3(5, 6, 7), true);
        Vector3 localPos = StraightFour.ActiveWorld.cameraManager.GetPosition(true);
        Assert.AreEqual(new Vector3(5, 6, 7), localPos);
    }

    [UnityTest]
    public IEnumerator CameraTests_GetRotation()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set and get world rotation.
        Quaternion rot = Quaternion.Euler(45, 90, 0);
        StraightFour.ActiveWorld.cameraManager.SetRotation(rot, false);
        Quaternion gotRot = StraightFour.ActiveWorld.cameraManager.GetRotation(false);
        Assert.AreEqual(rot.x, gotRot.x, 0.001f);
        Assert.AreEqual(rot.y, gotRot.y, 0.001f);
        Assert.AreEqual(rot.z, gotRot.z, 0.001f);
        Assert.AreEqual(rot.w, gotRot.w, 0.001f);

        // Set and get local rotation.
        Quaternion localRot = Quaternion.Euler(10, 20, 30);
        StraightFour.ActiveWorld.cameraManager.SetRotation(localRot, true);
        Quaternion gotLocalRot = StraightFour.ActiveWorld.cameraManager.GetRotation(true);
        Assert.AreEqual(localRot.x, gotLocalRot.x, 0.001f);
        Assert.AreEqual(localRot.y, gotLocalRot.y, 0.001f);
        Assert.AreEqual(localRot.z, gotLocalRot.z, 0.001f);
        Assert.AreEqual(localRot.w, gotLocalRot.w, 0.001f);
    }

    [UnityTest]
    public IEnumerator CameraTests_GetEulerRotation()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set and get world euler rotation.
        Vector3 euler = new Vector3(45, 90, 0);
        StraightFour.ActiveWorld.cameraManager.SetEulerRotation(euler, false);
        Vector3 gotEuler = StraightFour.ActiveWorld.cameraManager.GetEulerRotation(false);
        Assert.AreEqual(euler.x, gotEuler.x, 0.01f);
        Assert.AreEqual(euler.y, gotEuler.y, 0.01f);
        Assert.AreEqual(euler.z, gotEuler.z, 0.01f);

        // Set and get local euler rotation.
        Vector3 localEuler = new Vector3(10, 20, 30);
        StraightFour.ActiveWorld.cameraManager.SetEulerRotation(localEuler, true);
        Vector3 gotLocalEuler = StraightFour.ActiveWorld.cameraManager.GetEulerRotation(true);
        Assert.AreEqual(localEuler.x, gotLocalEuler.x, 0.01f);
        Assert.AreEqual(localEuler.y, gotLocalEuler.y, 0.01f);
        Assert.AreEqual(localEuler.z, gotLocalEuler.z, 0.01f);
    }

    [UnityTest]
    public IEnumerator CameraTests_GetScale()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set and get scale.
        StraightFour.ActiveWorld.cameraManager.SetScale(new Vector3(2, 3, 4));
        Vector3 scale = StraightFour.ActiveWorld.cameraManager.GetScale();
        Assert.AreEqual(new Vector3(2, 3, 4), scale);
    }

    [UnityTest]
    public IEnumerator CameraTests_SetParentNull()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set parent to a GameObject.
        GameObject parentGO = new GameObject("parent");
        StraightFour.ActiveWorld.cameraManager.SetParent(parentGO);
        Assert.AreEqual(parentGO.transform, StraightFour.ActiveWorld.cameraManager.cam.transform.parent);

        // Set parent back to null (should use default camera parent).
        StraightFour.ActiveWorld.cameraManager.SetParent(null);
        // Camera should no longer be parented to our custom parent.
        Assert.AreNotEqual(parentGO.transform, StraightFour.ActiveWorld.cameraManager.cam.transform.parent);
    }

    [UnityTest]
    public IEnumerator CameraTests_MultiplePositionUpdates()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set position multiple times and verify only the last one sticks.
        StraightFour.ActiveWorld.cameraManager.SetPosition(new Vector3(1, 1, 1), false);
        StraightFour.ActiveWorld.cameraManager.SetPosition(new Vector3(2, 2, 2), false);
        StraightFour.ActiveWorld.cameraManager.SetPosition(new Vector3(3, 3, 3), false);
        Vector3 finalPos = StraightFour.ActiveWorld.cameraManager.GetPosition(false);
        Assert.AreEqual(new Vector3(3, 3, 3), finalPos);
    }

    [UnityTest]
    public IEnumerator CameraTests_FieldOfView()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Modify field of view directly on the camera and verify.
        StraightFour.ActiveWorld.cameraManager.cam.fieldOfView = 90f;
        Assert.AreEqual(90f, StraightFour.ActiveWorld.cameraManager.cam.fieldOfView, 0.01f);

        StraightFour.ActiveWorld.cameraManager.cam.fieldOfView = 30f;
        Assert.AreEqual(30f, StraightFour.ActiveWorld.cameraManager.cam.fieldOfView, 0.01f);
    }

    [UnityTest]
    public IEnumerator CameraTests_ParentedPositionLocal()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize Camera.
        GameObject camGO = new GameObject();
        Camera camera = camGO.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -100);
        camGO.tag = "MainCamera";

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Parent camera, then set local position relative to parent.
        GameObject parentGO = new GameObject("parent");
        parentGO.transform.position = new Vector3(10, 10, 10);
        StraightFour.ActiveWorld.cameraManager.SetParent(parentGO);

        StraightFour.ActiveWorld.cameraManager.SetPosition(new Vector3(1, 2, 3), true);
        Vector3 localPos = StraightFour.ActiveWorld.cameraManager.GetPosition(true);
        Assert.AreEqual(new Vector3(1, 2, 3), localPos);

        // World position should be offset by parent position.
        Vector3 worldPos = StraightFour.ActiveWorld.cameraManager.cam.transform.position;
        Assert.AreEqual(11f, worldPos.x, 0.01f);
        Assert.AreEqual(12f, worldPos.y, 0.01f);
        Assert.AreEqual(13f, worldPos.z, 0.01f);
    }
}
