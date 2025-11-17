// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.LocalStorage;

/// <summary>
/// Unit tests for Local Storage.
/// </summary>
public class LocalStorageTests
{
    [Test]
    public void LocalStorageTests_Cache()
    {
        GameObject storageGO = new GameObject();
        LocalStorageManager storageManager = storageGO.AddComponent<LocalStorageManager>();

        // Add Site.
        LogAssert.Expect(LogType.Error, "[LocalStorageManager->AddSite] Local storage manager not initialized.");
        storageManager.AddSite("test");

        // Initialize.
        storageManager.Initialize(LocalStorageManager.LocalStorageMode.Cache, 16, 16, 16);

        // Add Site.
        storageManager.AddSite("test");
        LogAssert.Expect(LogType.Error, "[LocalStorageManager->AddSite] Local storage manager already contains site: test.");
        storageManager.AddSite("test");
        storageManager.AddSite("test2");

        // Set Item/Get Item.
        storageManager.SetItem("test", "key", "value");
        Assert.AreEqual("value", storageManager.GetItem("test", "key"));
        LogAssert.Expect(LogType.Error, "[LocalStorageManager->SetItem] Local storage manager does not contain site: invalidsite.");
        storageManager.SetItem("invalidsite", "key", "value");
        storageManager.SetItem("test2", "newkey", "value");
        Assert.AreEqual("value", storageManager.GetItem("test2", "newkey"));
        Assert.AreEqual(null, storageManager.GetItem("test", "newkey"));
        storageManager.SetItem("test", "key", "newvalue");
        Assert.AreEqual("newvalue", storageManager.GetItem("test", "key"));
        Assert.AreEqual(null, storageManager.GetItem("test", "nonexistent"));
        LogAssert.Expect(LogType.Error, "[LocalStorageManager->GetItem] Local storage manager does not contain site: invalidsite.");
        Assert.AreEqual(null, storageManager.GetItem("invalidsite", "nonexistant"));
        storageManager.SetItem("test", "largestkey......", "largestvalue....");
        Assert.AreEqual("largestvalue....", storageManager.GetItem("test", "largestkey......"));
        storageManager.SetItem("test", "toolargekey......", "value");
        Assert.AreEqual("value", storageManager.GetItem("test", "toolargekey....."));
        storageManager.SetItem("test", "somekey", "toolargevalue....");
        Assert.AreEqual("toolargevalue...", storageManager.GetItem("test", "somekey"));
        for (int i = 4; i < 16; i++)
        {
            storageManager.SetItem("test", "key" + i, "value" + i);
            Assert.AreEqual("value" + i, storageManager.GetItem("test", "key" + i));
        }
        LogAssert.Expect(LogType.Warning, "[CacheStorageController->SetItem] Cache Storage full.");
        storageManager.SetItem("test", "key16", "value16");

        // Delete Item.
        storageManager.RemoveItem("test", "key15");
        Assert.AreEqual(null, storageManager.GetItem("test", "key15"));

        // Key.
        string key = storageManager.Key("test", 4);
        Assert.AreEqual("key4", key);

        // Clear.
        storageManager.Clear("test");
        Assert.AreEqual(null, storageManager.Key("test", 0));
    }

    [Test]
    public void LocalStorageTests_Persistent()
    {
        GameObject storageGO = new GameObject();
        LocalStorageManager storageManager = storageGO.AddComponent<LocalStorageManager>();

        // Add Site.
        LogAssert.Expect(LogType.Error, "[LocalStorageManager->AddSite] Local storage manager not initialized.");
        storageManager.AddSite("test");

        // Initialize.
        storageManager.Initialize(LocalStorageManager.LocalStorageMode.Persistent, 16, 16, 16);

        // Add Site.
        storageManager.AddSite("test");
        LogAssert.Expect(LogType.Error, "[LocalStorageManager->AddSite] Local storage manager already contains site: test.");
        storageManager.AddSite("test");
        storageManager.AddSite("test2");

        // Clear.
        storageManager.Clear("test");
        storageManager.Clear("test2");

        // Set Item/Get Item.
        storageManager.SetItem("test", "key", "value");
        Assert.AreEqual("value", storageManager.GetItem("test", "key"));
        LogAssert.Expect(LogType.Error, "[LocalStorageManager->SetItem] Local storage manager does not contain site: invalidsite.");
        storageManager.SetItem("invalidsite", "key", "value");
        storageManager.SetItem("test2", "newkey", "value");
        Assert.AreEqual("value", storageManager.GetItem("test2", "newkey"));
        Assert.AreEqual(null, storageManager.GetItem("test", "newkey"));
        storageManager.SetItem("test", "key", "newvalue");
        Assert.AreEqual("newvalue", storageManager.GetItem("test", "key"));
        Assert.AreEqual(null, storageManager.GetItem("test", "nonexistent"));
        LogAssert.Expect(LogType.Error, "[LocalStorageManager->GetItem] Local storage manager does not contain site: invalidsite.");
        Assert.AreEqual(null, storageManager.GetItem("invalidsite", "nonexistant"));
        storageManager.SetItem("test", "largestkey......", "largestvalue....");
        Assert.AreEqual("largestvalue....", storageManager.GetItem("test", "largestkey......"));
        storageManager.SetItem("test", "toolargekey......", "value");
        Assert.AreEqual("value", storageManager.GetItem("test", "toolargekey....."));
        storageManager.SetItem("test", "somekey", "toolargevalue....");
        Assert.AreEqual("toolargevalue...", storageManager.GetItem("test", "somekey"));
        for (int i = 4; i < 16; i++)
        {
            storageManager.SetItem("test", "key" + i, "value" + i);
            Assert.AreEqual("value" + i, storageManager.GetItem("test", "key" + i));
        }
        LogAssert.Expect(LogType.Warning, "[PersistentStorageController->SetItem] Persistent Storage full.");
        storageManager.SetItem("test", "key16", "value16");

        // Delete Item.
        storageManager.RemoveItem("test", "key15");
        Assert.AreEqual(null, storageManager.GetItem("test", "key15"));

        // Key.
        string key = storageManager.Key("test", 4);
        Assert.AreEqual("key4", key);

        // Clear.
        storageManager.Clear("test");
        Assert.AreEqual(null, storageManager.Key("test", 0));
    }
}