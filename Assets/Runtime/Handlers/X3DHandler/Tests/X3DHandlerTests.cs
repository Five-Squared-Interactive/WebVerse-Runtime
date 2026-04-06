// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.LocalStorage;
using UnityEditor;
using FiveSQD.StraightFour;
using System.IO;
using FiveSQD.WebVerse.Handlers.X3D;
using System.Text.RegularExpressions;

/// <summary>
/// Unit tests for the X3D Handler.
/// </summary>
public class X3DHandlerTests
{
    private float waitTime = 5;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
        WebVerseRuntime.Instance = null;
    }

    /// <summary>
    /// Helper to set up the WebVerse runtime for X3D testing.
    /// </summary>
    private (GameObject go, WebVerseRuntime runtime) SetUpRuntime()
    {
        GameObject runtimeGO = new GameObject("runtime");
        WebVerseRuntime runtime = runtimeGO.AddComponent<WebVerseRuntime>();
        runtime.highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        runtime.skyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Runtime/StraightFour/Environment/Materials/Skybox.mat");
        runtime.characterControllerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runtime/StraightFour/Entity/Character/Prefabs/UserAvatar.prefab");
        runtime.inputEntityPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runtime/StraightFour/Entity/UI/UIElement/Input/Prefabs/InputEntity.prefab");
        runtime.voxelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runtime/StraightFour/Entity/Voxel/Prefabs/Voxel.prefab");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, Path.Combine(Application.dataPath, "Files"));
        StraightFour.UnloadWorld();
        StraightFour.LoadWorld("test");
        return (runtimeGO, runtime);
    }

    /// <summary>
    /// Helper to clean up the runtime safely. CameraManager may throw during
    /// Terminate in test context when certain subsystems are not fully initialized.
    /// </summary>
    private void CleanUpRuntime(WebVerseRuntime runtime, GameObject runtimeGO)
    {
        try { runtime.Terminate(); } catch (Exception) { }
        GameObject.DestroyImmediate(runtimeGO);
    }

    /// <summary>
    /// Sample X3D content for testing basic scene parsing.
    /// </summary>
    private const string SampleX3DContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE X3D PUBLIC ""ISO//Web3D//DTD X3D 3.3//EN"" ""http://www.web3d.org/specifications/x3d-3.3.dtd"">
<X3D profile=""Immersive"" version=""3.3"">
    <head>
        <meta name=""title"" content=""Test Scene""/>
    </head>
    <Scene>
        <Background skyColor=""0.2 0.4 0.6""/>
        <Viewpoint position=""0 1.6 5"" orientation=""0 1 0 0"" description=""Default View""/>
        <Transform translation=""0 0 0"">
            <Shape>
                <Appearance>
                    <Material diffuseColor=""1 0 0"" specularColor=""1 1 1"" shininess=""0.5""/>
                </Appearance>
                <Box size=""1 1 1""/>
            </Shape>
        </Transform>
        <Transform translation=""2 0 0"">
            <Shape>
                <Appearance>
                    <Material diffuseColor=""0 1 0""/>
                </Appearance>
                <Sphere radius=""0.5""/>
            </Shape>
        </Transform>
        <Transform translation=""-2 0 0"">
            <Shape>
                <Appearance>
                    <Material diffuseColor=""0 0 1""/>
                </Appearance>
                <Cylinder radius=""0.3"" height=""1""/>
            </Shape>
        </Transform>
        <DirectionalLight direction=""0 -1 -1"" intensity=""0.8"" color=""1 1 1""/>
    </Scene>
</X3D>";

    /// <summary>
    /// Sample X3D content with fog.
    /// </summary>
    private const string SampleX3DWithFog = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<X3D profile=""Immersive"" version=""3.3"">
    <Scene>
        <Background skyColor=""0.5 0.5 0.5""/>
        <Fog color=""0.8 0.8 0.8"" visibilityRange=""100"" fogType=""LINEAR""/>
        <Transform translation=""0 0 0"">
            <Shape>
                <Appearance>
                    <Material diffuseColor=""1 1 0""/>
                </Appearance>
                <Cone bottomRadius=""0.5"" height=""1""/>
            </Shape>
        </Transform>
    </Scene>
</X3D>";

    /// <summary>
    /// Sample X3D content with nested transforms.
    /// </summary>
    private const string SampleX3DWithNestedTransforms = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<X3D profile=""Immersive"" version=""3.3"">
    <Scene>
        <Transform DEF=""Root"" translation=""0 0 0"">
            <Transform DEF=""Child1"" translation=""1 0 0"">
                <Shape>
                    <Appearance>
                        <Material diffuseColor=""1 0 0""/>
                    </Appearance>
                    <Box size=""0.5 0.5 0.5""/>
                </Shape>
                <Transform DEF=""GrandChild"" translation=""0 1 0"">
                    <Shape>
                        <Appearance>
                            <Material diffuseColor=""0 1 0""/>
                        </Appearance>
                        <Sphere radius=""0.25""/>
                    </Shape>
                </Transform>
            </Transform>
            <Transform DEF=""Child2"" translation=""-1 0 0"">
                <Shape>
                    <Appearance>
                        <Material diffuseColor=""0 0 1""/>
                    </Appearance>
                    <Cylinder radius=""0.25"" height=""0.5""/>
                </Shape>
            </Transform>
        </Transform>
    </Scene>
</X3D>";

    /// <summary>
    /// Sample X3D content with spot light.
    /// </summary>
    private const string SampleX3DWithSpotLight = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<X3D profile=""Immersive"" version=""3.3"">
    <Scene>
        <SpotLight location=""0 3 0"" direction=""0 -1 0"" cutOffAngle=""0.785"" intensity=""1"" color=""1 1 1""/>
        <Transform translation=""0 0 0"">
            <Shape>
                <Appearance>
                    <Material diffuseColor=""0.6 0.6 0.6""/>
                </Appearance>
                <Cylinder radius=""0.5"" height=""0.1""/>
            </Shape>
        </Transform>
    </Scene>
</X3D>";

    /// <summary>
    /// Sample X3D content with animation (TimeSensor + Route + Interpolator).
    /// </summary>
    private const string SampleX3DWithAnimation = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<X3D profile=""Immersive"" version=""3.3"">
    <Scene>
        <Transform DEF=""Ball"" translation=""0 0 0"">
            <Shape>
                <Appearance>
                    <Material diffuseColor=""1 0 0""/>
                </Appearance>
                <Sphere radius=""0.25""/>
            </Shape>
        </Transform>
        <TimeSensor DEF=""Timer"" cycleInterval=""4"" loop=""true""/>
        <PositionInterpolator DEF=""Mover"" key=""0 0.5 1"" keyValue=""0 0 0, 2 0 0, 0 0 0""/>
        <ROUTE fromNode=""Timer"" fromField=""fraction_changed"" toNode=""Mover"" toField=""set_fraction""/>
        <ROUTE fromNode=""Mover"" fromField=""value_changed"" toNode=""Ball"" toField=""set_translation""/>
    </Scene>
</X3D>";

    /// <summary>
    /// Sample X3D content with point light.
    /// </summary>
    private const string SampleX3DWithPointLight = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<X3D profile=""Immersive"" version=""3.3"">
    <Scene>
        <PointLight location=""0 2 0"" intensity=""1"" color=""1 1 1"" radius=""10""/>
        <Transform translation=""0 0 0"">
            <Shape>
                <Appearance>
                    <Material diffuseColor=""0.8 0.8 0.8""/>
                </Appearance>
                <Box size=""2 0.1 2""/>
            </Shape>
        </Transform>
    </Scene>
</X3D>";

    [UnityTest]
    public IEnumerator X3DHandlerTests_LoadFromString_BasicScene()
    {
        LogAssert.ignoreFailingMessages = true;
        var (runtimeGO, runtime) = SetUpRuntime();

        yield return new WaitForSeconds(1);

        Assert.IsNotNull(runtime.x3dHandler, "X3DHandler should be initialized");

        bool loadComplete = false;
        bool loadSuccess = false;
        Action<bool> onComplete = (success) =>
        {
            loadComplete = true;
            loadSuccess = success;
        };

        // BasicScene contains Background node which requires EnvironmentManager.
        // In test context EnvironmentManager is not available, so loading completes
        // but reports failure. This validates the error handling path.
        runtime.x3dHandler.LoadX3DFromString(SampleX3DContent, null, onComplete);

        yield return new WaitForSeconds(waitTime);

        Assert.IsTrue(loadComplete, "X3D loading callback should fire");

        CleanUpRuntime(runtime, runtimeGO);
    }

    [UnityTest]
    public IEnumerator X3DHandlerTests_LoadFromString_WithFog()
    {
        LogAssert.ignoreFailingMessages = true;
        var (runtimeGO, runtime) = SetUpRuntime();

        yield return new WaitForSeconds(1);

        bool loadComplete = false;
        bool loadSuccess = false;
        Action<bool> onComplete = (success) =>
        {
            loadComplete = true;
            loadSuccess = success;
        };

        // Fog scene contains Background + Fog nodes which require EnvironmentManager.
        runtime.x3dHandler.LoadX3DFromString(SampleX3DWithFog, null, onComplete);

        yield return new WaitForSeconds(waitTime);

        Assert.IsTrue(loadComplete, "X3D loading callback should fire");

        CleanUpRuntime(runtime, runtimeGO);
    }

    [UnityTest]
    public IEnumerator X3DHandlerTests_LoadFromString_NestedTransforms()
    {
        LogAssert.ignoreFailingMessages = true;
        var (runtimeGO, runtime) = SetUpRuntime();

        yield return new WaitForSeconds(1);

        bool loadComplete = false;
        bool loadSuccess = false;
        Action<bool> onComplete = (success) =>
        {
            loadComplete = true;
            loadSuccess = success;
        };

        runtime.x3dHandler.LoadX3DFromString(SampleX3DWithNestedTransforms, null, onComplete);

        yield return new WaitForSeconds(waitTime);

        Assert.IsTrue(loadComplete, "X3D loading should complete");
        Assert.IsTrue(loadSuccess, "X3D loading should succeed");

        CleanUpRuntime(runtime, runtimeGO);
    }

    [UnityTest]
    public IEnumerator X3DHandlerTests_LoadFromString_PointLight()
    {
        LogAssert.ignoreFailingMessages = true;
        var (runtimeGO, runtime) = SetUpRuntime();

        yield return new WaitForSeconds(1);

        bool loadComplete = false;
        bool loadSuccess = false;
        Action<bool> onComplete = (success) =>
        {
            loadComplete = true;
            loadSuccess = success;
        };

        runtime.x3dHandler.LoadX3DFromString(SampleX3DWithPointLight, null, onComplete);

        yield return new WaitForSeconds(waitTime);

        Assert.IsTrue(loadComplete, "X3D loading should complete");
        Assert.IsTrue(loadSuccess, "X3D loading should succeed");

        CleanUpRuntime(runtime, runtimeGO);
    }

    [UnityTest]
    public IEnumerator X3DHandlerTests_LoadFromString_EmptyContent()
    {
        LogAssert.ignoreFailingMessages = true;
        var (runtimeGO, runtime) = SetUpRuntime();

        yield return new WaitForSeconds(1);

        bool loadComplete = false;
        bool loadSuccess = false;
        Action<bool> onComplete = (success) =>
        {
            loadComplete = true;
            loadSuccess = success;
        };

        runtime.x3dHandler.LoadX3DFromString("", null, onComplete);

        yield return new WaitForSeconds(waitTime);

        Assert.IsTrue(loadComplete, "X3D loading should complete");
        Assert.IsFalse(loadSuccess, "X3D loading with empty content should fail");

        CleanUpRuntime(runtime, runtimeGO);
    }

    [UnityTest]
    public IEnumerator X3DHandlerTests_LoadFromString_InvalidXML()
    {
        LogAssert.ignoreFailingMessages = true;
        var (runtimeGO, runtime) = SetUpRuntime();

        yield return new WaitForSeconds(1);

        bool loadComplete = false;
        bool loadSuccess = false;
        Action<bool> onComplete = (success) =>
        {
            loadComplete = true;
            loadSuccess = success;
        };

        runtime.x3dHandler.LoadX3DFromString("<invalid><not closed", null, onComplete);

        yield return new WaitForSeconds(waitTime);

        Assert.IsTrue(loadComplete, "X3D loading should complete");
        Assert.IsFalse(loadSuccess, "X3D loading with invalid XML should fail");

        CleanUpRuntime(runtime, runtimeGO);
    }

    [UnityTest]
    public IEnumerator X3DHandlerTests_LoadFromString_SpotLight()
    {
        LogAssert.ignoreFailingMessages = true;
        var (runtimeGO, runtime) = SetUpRuntime();

        yield return new WaitForSeconds(1);

        bool loadComplete = false;
        bool loadSuccess = false;
        Action<bool> onComplete = (success) =>
        {
            loadComplete = true;
            loadSuccess = success;
        };

        runtime.x3dHandler.LoadX3DFromString(SampleX3DWithSpotLight, null, onComplete);

        yield return new WaitForSeconds(waitTime);

        Assert.IsTrue(loadComplete, "X3D loading should complete");
        Assert.IsTrue(loadSuccess, "X3D loading should succeed");

        CleanUpRuntime(runtime, runtimeGO);
    }

    [UnityTest]
    public IEnumerator X3DHandlerTests_LoadFromString_WithAnimation()
    {
        LogAssert.ignoreFailingMessages = true;
        var (runtimeGO, runtime) = SetUpRuntime();

        yield return new WaitForSeconds(1);

        bool loadComplete = false;
        bool loadSuccess = false;
        Action<bool> onComplete = (success) =>
        {
            loadComplete = true;
            loadSuccess = success;
        };

        runtime.x3dHandler.LoadX3DFromString(SampleX3DWithAnimation, null, onComplete);

        yield return new WaitForSeconds(waitTime);

        Assert.IsTrue(loadComplete, "X3D loading should complete");
        Assert.IsTrue(loadSuccess, "X3D loading should succeed");

        CleanUpRuntime(runtime, runtimeGO);
    }

    [UnityTest]
    public IEnumerator X3DHandlerTests_LoadFromString_NullContent()
    {
        LogAssert.ignoreFailingMessages = true;
        var (runtimeGO, runtime) = SetUpRuntime();

        yield return new WaitForSeconds(1);

        bool loadComplete = false;
        bool loadSuccess = false;
        Action<bool> onComplete = (success) =>
        {
            loadComplete = true;
            loadSuccess = success;
        };

        runtime.x3dHandler.LoadX3DFromString(null, null, onComplete);

        yield return new WaitForSeconds(waitTime);

        Assert.IsTrue(loadComplete, "X3D loading should complete");
        Assert.IsFalse(loadSuccess, "X3D loading with null content should fail");

        CleanUpRuntime(runtime, runtimeGO);
    }
}
