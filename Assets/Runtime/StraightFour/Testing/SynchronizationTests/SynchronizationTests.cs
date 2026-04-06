// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Synchronization;
using UnityEditor;

public class SynchronizationTests
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
    public IEnumerator SynchronizationTests_General()
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

        GameObject synchGO = new GameObject();
        BaseSynchronizer bs = synchGO.AddComponent<BaseSynchronizer>();

        // Set Visibility.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetVisibility(null, false));

        // Delete Synchronized Entity.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.DeleteSynchronizedEntity(null));

        // Set Highlight.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetHighlight(null, false));

        // Set Parent.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetParent(null, null));

        // Set Position.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetPosition(null, Vector3.zero));

        // Set Rotation.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetRotation(null, Quaternion.identity));

        // Set Scale.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetScale(null, Vector3.zero));

        // Set Size.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetSize(null, Vector3.zero));

        // Set Physical Properties.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetPhysicalProperties(null, null));

        // Set Motion.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetMotion(null, null));

        // Make World Canvas.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.MakeWorldCanvas(null));

        // Make Screen Canvas.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.MakeScreenCanvas(null));

        // Set Position Percent.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetPositionPercent(null, Vector2.zero));

        // Set Size Percent.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetSizePercent(null, Vector2.zero));

        // Modify Terrain Entity.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.ModifyTerrainEntity(null, 0, Vector3.zero, 0, 0));

        // Set Interaction State.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetInteractionState(null, FiveSQD.StraightFour.Entity.BaseEntity.InteractionState.Static));

        // Add Synchronized Entity.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.AddSynchronizedEntity(null, false));

        // Remove Synchronized Entity.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.RemoveSynchronizedEntity(null));

        // Send Message.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SendMessage(null, null));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SendMessage("topic", "message"));
    }

    [UnityTest]
    public IEnumerator SynchronizationTests_MultipleSynchronizers()
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

        // Create multiple synchronizers and verify they are independent.
        GameObject synchGO1 = new GameObject();
        BaseSynchronizer bs1 = synchGO1.AddComponent<BaseSynchronizer>();

        GameObject synchGO2 = new GameObject();
        BaseSynchronizer bs2 = synchGO2.AddComponent<BaseSynchronizer>();

        Assert.AreNotEqual(bs1, bs2);
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs1.SetPosition(null, Vector3.zero));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs2.SetPosition(null, Vector3.one));
    }

    [UnityTest]
    public IEnumerator SynchronizationTests_SetVisibilityValues()
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

        GameObject synchGO = new GameObject();
        BaseSynchronizer bs = synchGO.AddComponent<BaseSynchronizer>();

        // Verify both true and false visibility return UNSUPPORTED on base.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetVisibility(null, true));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetVisibility(null, false));
    }

    [UnityTest]
    public IEnumerator SynchronizationTests_SetHighlightValues()
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

        GameObject synchGO = new GameObject();
        BaseSynchronizer bs = synchGO.AddComponent<BaseSynchronizer>();

        // Verify both true and false highlight return UNSUPPORTED on base.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetHighlight(null, true));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetHighlight(null, false));
    }

    [UnityTest]
    public IEnumerator SynchronizationTests_AddRemoveSynchronizedEntity()
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

        GameObject synchGO = new GameObject();
        BaseSynchronizer bs = synchGO.AddComponent<BaseSynchronizer>();

        // Add with deleteWithClient true and false.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.AddSynchronizedEntity(null, true));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.AddSynchronizedEntity(null, false));

        // Remove.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.RemoveSynchronizedEntity(null));
    }

    [UnityTest]
    public IEnumerator SynchronizationTests_SendMessageVariations()
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

        GameObject synchGO = new GameObject();
        BaseSynchronizer bs = synchGO.AddComponent<BaseSynchronizer>();

        // Null topic and message.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SendMessage(null, null));

        // Non-null topic, null message.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SendMessage("topic", null));

        // Null topic, non-null message.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SendMessage(null, "message"));

        // Non-null topic and message.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SendMessage("topic", "message"));

        // Empty strings.
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SendMessage("", ""));
    }
}