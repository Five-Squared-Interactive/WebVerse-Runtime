// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using UnityEditor;

public class WorldStorageTests
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
    public IEnumerator WorldStorageTests_General()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
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

    [UnityTest]
    public IEnumerator WorldStorageTests_GetNonexistentKey()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        StraightFour.ActiveWorld.storageManager.Initialize(16, 16, 16);

        // Get item for a key that was never set should return null.
        Assert.IsNull(StraightFour.ActiveWorld.storageManager.GetItem("doesnotexist"));
        Assert.IsNull(StraightFour.ActiveWorld.storageManager.GetItem("another"));
        Assert.IsNull(StraightFour.ActiveWorld.storageManager.GetItem(""));
    }

    [UnityTest]
    public IEnumerator WorldStorageTests_OverwriteExistingKey()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        StraightFour.ActiveWorld.storageManager.Initialize(16, 16, 16);

        // Set a key, then overwrite it multiple times.
        StraightFour.ActiveWorld.storageManager.SetItem("mykey", "first");
        Assert.AreEqual("first", StraightFour.ActiveWorld.storageManager.GetItem("mykey"));

        StraightFour.ActiveWorld.storageManager.SetItem("mykey", "second");
        Assert.AreEqual("second", StraightFour.ActiveWorld.storageManager.GetItem("mykey"));

        StraightFour.ActiveWorld.storageManager.SetItem("mykey", "third");
        Assert.AreEqual("third", StraightFour.ActiveWorld.storageManager.GetItem("mykey"));
    }

    [UnityTest]
    public IEnumerator WorldStorageTests_GetAllItems()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        StraightFour.ActiveWorld.storageManager.Initialize(16, 16, 16);

        // Set multiple items and retrieve all.
        StraightFour.ActiveWorld.storageManager.SetItem("a", "1");
        StraightFour.ActiveWorld.storageManager.SetItem("b", "2");
        StraightFour.ActiveWorld.storageManager.SetItem("c", "3");

        var allItems = StraightFour.ActiveWorld.storageManager.GetAllItems();
        Assert.IsNotNull(allItems);
        Assert.AreEqual(3, allItems.Count);
        Assert.AreEqual("1", allItems["a"]);
        Assert.AreEqual("2", allItems["b"]);
        Assert.AreEqual("3", allItems["c"]);
    }

    [UnityTest]
    public IEnumerator WorldStorageTests_SetAllItems()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        StraightFour.ActiveWorld.storageManager.Initialize(16, 16, 16);

        // Set some initial items.
        StraightFour.ActiveWorld.storageManager.SetItem("old", "data");
        Assert.AreEqual("data", StraightFour.ActiveWorld.storageManager.GetItem("old"));

        // Replace all items with a new dictionary.
        var newItems = new System.Collections.Generic.Dictionary<string, string>
        {
            { "x", "10" },
            { "y", "20" }
        };
        StraightFour.ActiveWorld.storageManager.SetAllItems(newItems);

        // Old key should no longer exist.
        Assert.IsNull(StraightFour.ActiveWorld.storageManager.GetItem("old"));

        // New keys should exist.
        Assert.AreEqual("10", StraightFour.ActiveWorld.storageManager.GetItem("x"));
        Assert.AreEqual("20", StraightFour.ActiveWorld.storageManager.GetItem("y"));
    }

    [UnityTest]
    public IEnumerator WorldStorageTests_MultipleKeys()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        StraightFour.ActiveWorld.storageManager.Initialize(16, 16, 16);

        // Set multiple distinct keys and verify they don't interfere.
        StraightFour.ActiveWorld.storageManager.SetItem("alpha", "one");
        StraightFour.ActiveWorld.storageManager.SetItem("beta", "two");
        StraightFour.ActiveWorld.storageManager.SetItem("gamma", "three");

        Assert.AreEqual("one", StraightFour.ActiveWorld.storageManager.GetItem("alpha"));
        Assert.AreEqual("two", StraightFour.ActiveWorld.storageManager.GetItem("beta"));
        Assert.AreEqual("three", StraightFour.ActiveWorld.storageManager.GetItem("gamma"));

        // Overwrite one key and verify others remain unchanged.
        StraightFour.ActiveWorld.storageManager.SetItem("beta", "updated");
        Assert.AreEqual("one", StraightFour.ActiveWorld.storageManager.GetItem("alpha"));
        Assert.AreEqual("updated", StraightFour.ActiveWorld.storageManager.GetItem("beta"));
        Assert.AreEqual("three", StraightFour.ActiveWorld.storageManager.GetItem("gamma"));
    }
}