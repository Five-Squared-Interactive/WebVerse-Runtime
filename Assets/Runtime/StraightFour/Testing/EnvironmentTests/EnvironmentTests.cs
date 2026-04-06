// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour;
using UnityEditor;

public class EnvironmentTests
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
    public IEnumerator EnvironmentTests_EnvironmentManagerExists()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Verify environment manager is available.
        Assert.IsNotNull(StraightFour.ActiveWorld.environmentManager);
    }

    [UnityTest]
    public IEnumerator EnvironmentTests_ApplySkyMaterial()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Apply sky material and verify it returns true.
        bool result = StraightFour.ActiveWorld.environmentManager.ApplySkyMaterial();
        Assert.IsTrue(result);

        // Verify RenderSettings.skybox is set.
        Assert.IsNotNull(RenderSettings.skybox);
    }

    [UnityTest]
    public IEnumerator EnvironmentTests_SetSolidColorSky()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Set solid color sky.
        Color skyColor = new Color(0.2f, 0.4f, 0.6f, 1f);
        bool result = StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(skyColor);
        Assert.IsTrue(result);

        // Verify RenderSettings.skybox is still set.
        Assert.IsNotNull(RenderSettings.skybox);
    }

    [UnityTest]
    public IEnumerator EnvironmentTests_SetSkyTexture()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Create a test texture and set it as sky texture.
        Texture2D testTexture = new Texture2D(64, 64);
        bool result = StraightFour.ActiveWorld.environmentManager.SetSkyTexture(testTexture);
        Assert.IsTrue(result);

        // Verify RenderSettings.skybox is still set.
        Assert.IsNotNull(RenderSettings.skybox);
    }

    [UnityTest]
    public IEnumerator EnvironmentTests_ActivateLiteFog()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Activate fog with valid parameters.
        Color fogColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        float fogDensity = 0.5f;
        bool result = StraightFour.ActiveWorld.environmentManager.ActivateLiteFog(fogColor, fogDensity);
        Assert.IsTrue(result);

        // Verify fog is enabled.
        Assert.IsTrue(RenderSettings.fog);
        Assert.AreEqual(fogColor, RenderSettings.fogColor);
        Assert.AreEqual(fogDensity, RenderSettings.fogDensity, 0.001f);
        Assert.AreEqual(FogMode.Exponential, RenderSettings.fogMode);
    }

    [UnityTest]
    public IEnumerator EnvironmentTests_DisableFog()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // First enable fog.
        StraightFour.ActiveWorld.environmentManager.ActivateLiteFog(Color.gray, 0.3f);
        Assert.IsTrue(RenderSettings.fog);

        // Disable fog.
        bool result = StraightFour.ActiveWorld.environmentManager.DisableFog();
        Assert.IsTrue(result);
        Assert.IsFalse(RenderSettings.fog);
    }

    [UnityTest]
    public IEnumerator EnvironmentTests_ActivateLiteFogInvalidDensity()
    {
        LogAssert.ignoreFailingMessages = true;

        // Initialize World Engine and Load World.
        GameObject WEGO = new GameObject();
        StraightFour we = WEGO.AddComponent<StraightFour>();
        we.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        yield return null;
        StraightFour.LoadWorld("test");

        // Activate fog with invalid density (> 1).
        bool result = StraightFour.ActiveWorld.environmentManager.ActivateLiteFog(Color.white, 1.5f);
        Assert.IsFalse(result);

        // Activate fog with invalid density (< 0).
        result = StraightFour.ActiveWorld.environmentManager.ActivateLiteFog(Color.white, -0.1f);
        Assert.IsFalse(result);
    }
}
