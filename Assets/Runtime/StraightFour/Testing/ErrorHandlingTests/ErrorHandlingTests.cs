// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Entity;
using FiveSQD.StraightFour.Synchronization;
using UnityEditor;

public class ErrorHandlingTests
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
    public IEnumerator ErrorHandlingTests_LoadWorldTwice()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        
        // First load should succeed
        bool firstLoad = StraightFour.LoadWorld("test");
        Assert.IsTrue(firstLoad);
        Assert.IsNotNull(StraightFour.ActiveWorld);

        // Second load should fail and log error
        LogAssert.Expect(LogType.Error, "[StraightFour->LoadWorld] Cannot load world. A world is loaded.");
        bool secondLoad = StraightFour.LoadWorld("test2");
        Assert.IsFalse(secondLoad);
    }

    [UnityTest]
    public IEnumerator ErrorHandlingTests_NullEntityOperations()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Test synchronizer operations with null entities
        GameObject synchGO = new GameObject();
        BaseSynchronizer bs = synchGO.AddComponent<BaseSynchronizer>();

        // All these should return UNSUPPORTED for null entities
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetVisibility(null, true));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.DeleteSynchronizedEntity(null));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetHighlight(null, true));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetParent(null, null));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetPosition(null, Vector3.zero));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetRotation(null, Quaternion.identity));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetScale(null, Vector3.zero));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetSize(null, Vector3.zero));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetPhysicalProperties(null, null));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SetMotion(null, null));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.MakeWorldCanvas(null));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.MakeScreenCanvas(null));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.AddSynchronizedEntity(null, false));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.RemoveSynchronizedEntity(null));
        Assert.AreEqual(BaseSynchronizer.StatusCode.UNSUPPORTED, bs.SendMessage(null, null));
    }

    [Test]
    public void ErrorHandlingTests_ActiveWorldWithoutInstance()
    {
        // Test that ActiveWorld returns null when no instance exists
        // This test doesn't create a StraightFour instance
        Assert.IsNull(StraightFour.ActiveWorld);
    }

    [UnityTest]
    public IEnumerator ErrorHandlingTests_EntityIDProtection()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set up Entity.
        GameObject go = new GameObject();
        BaseEntity be = go.AddComponent<BaseEntity>();

        // Initialize Entity with ID
        System.Guid entityID = System.Guid.NewGuid();
        be.Initialize(entityID);
        Assert.AreEqual(entityID, be.id);

        // Ensure ID cannot be changed after initialization
        System.Guid newID = System.Guid.NewGuid();
        Assert.Throws<System.InvalidOperationException>(() => be.id = newID);
        
        // ID should still be the original
        Assert.AreEqual(entityID, be.id);
    }

    [UnityTest]
    public IEnumerator ErrorHandlingTests_QueryParams()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        
        // Load world with query parameters
        string queryParams = "param1=value1&param2=value2&param3=value%20with%20spaces";
        StraightFour.LoadWorld("test", queryParams);
        Assert.IsNotNull(StraightFour.ActiveWorld);

        // Test parameter retrieval
        Assert.AreEqual("value1", we.GetParam("param1"));
        Assert.AreEqual("value2", we.GetParam("param2"));
        Assert.AreEqual("value with spaces", we.GetParam("param3"));
        Assert.IsNull(we.GetParam("nonexistent"));
        Assert.IsNull(we.GetParam(null));
    }

    [UnityTest] 
    public IEnumerator ErrorHandlingTests_UnloadWorldStates()
    {
        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");
        
        Assert.IsNotNull(StraightFour.ActiveWorld);
        
        // Unload world
        LogAssert.Expect(LogType.Log, "[StraightFour->UnloadWorld] Unloading World...");
        LogAssert.Expect(LogType.Log, "[StraightFour->UnloadWorld] World Unloaded. Destroying World Object...");
        LogAssert.Expect(LogType.Log, "[StraightFour->UnloadWorld] World Object Destroyed.");
        StraightFour.UnloadWorld();
        
        yield return null; // Wait for destroy
        Assert.IsNull(StraightFour.ActiveWorld);
        
        // Should be able to load another world now
        bool secondLoad = StraightFour.LoadWorld("test2");
        Assert.IsTrue(secondLoad);
        Assert.IsNotNull(StraightFour.ActiveWorld);
    }
}