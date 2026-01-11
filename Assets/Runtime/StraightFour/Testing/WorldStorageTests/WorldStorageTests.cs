// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using UnityEditor;

public class WorldStorageTests
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
    public IEnumerator WorldStorageTests_General()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        Assert.IsNotNull(StraightFour.ActiveWorld.storageManager);

        StraightFour.ActiveWorld.storageManager.Initialize(16, 16, 16);
        StraightFour.ActiveWorld.storageManager.SetItem("key", "value");
        Assert.AreEqual("value", StraightFour.ActiveWorld.storageManager.GetItem("key"));
        StraightFour.ActiveWorld.storageManager.SetItem("key", "newvalue");
        Assert.AreEqual("newvalue", StraightFour.ActiveWorld.storageManager.GetItem("key"));
        Assert.AreEqual(null, StraightFour.ActiveWorld.storageManager.GetItem("nonexistent"));
        StraightFour.ActiveWorld.storageManager.SetItem("largestkey......", "largestvalue....");
        Assert.AreEqual("largestvalue....", StraightFour.ActiveWorld.storageManager.GetItem("largestkey......"));
        StraightFour.ActiveWorld.storageManager.SetItem("toolargekey......", "value");
        Assert.AreEqual("value", StraightFour.ActiveWorld.storageManager.GetItem("toolargekey....."));
        StraightFour.ActiveWorld.storageManager.SetItem("somekey", "toolargevalue....");
        Assert.AreEqual("toolargevalue...", StraightFour.ActiveWorld.storageManager.GetItem("somekey"));
        for (int i = 4; i < 16; i++)
        {
            StraightFour.ActiveWorld.storageManager.SetItem("key" + i, "value" + i);
            Assert.AreEqual("value" + i, StraightFour.ActiveWorld.storageManager.GetItem("key" + i));
        }
        LogAssert.Expect(LogType.Warning, "[WorldStorageManager->SetItem] World Storage full.");
        StraightFour.ActiveWorld.storageManager.SetItem("key16", "value16");
    }
}