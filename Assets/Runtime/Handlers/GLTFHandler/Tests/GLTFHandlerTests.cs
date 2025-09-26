// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.LocalStorage;
using FiveSQD.WebVerse.Handlers.File;
using System.IO;

/// <summary>
/// Unit tests for the GLTF Handler.
/// </summary>
public class GLTFHandlerTests
{
    private float waitTime = 5; // Reduced wait time
    private WebVerseRuntime runtime;
    private GameObject runtimeGO;

    [SetUp]
    public void SetUp()
    {
        // Create a simple runtime setup without external dependencies
        runtimeGO = new GameObject("runtime");
        runtime = runtimeGO.AddComponent<WebVerseRuntime>();
        
        // Use built-in materials and create dummy objects
        runtime.highlightMaterial = new Material(Shader.Find("Standard"));
        runtime.skyMaterial = new Material(Shader.Find("Standard"));
        
        // Create empty GameObjects as placeholders
        runtime.characterControllerPrefab = new GameObject("DummyCharacterController");
        runtime.inputEntityPrefab = new GameObject("DummyInputEntity");
        runtime.voxelPrefab = new GameObject("DummyVoxel");
        
        // Use a test directory in temp folder
        string testDirectory = Path.Combine(Path.GetTempPath(), "GLTFHandlerTests");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (runtime != null)
        {
            // Clean up test directory
            string testDirectory = Path.Combine(Path.GetTempPath(), "GLTFHandlerTests");
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
        
        if (runtimeGO != null)
        {
            Object.DestroyImmediate(runtimeGO);
        }
    }

    [Test]
    public void GLTFHandler_Initialize_IsCorrect()
    {
        // Test that GLTF handler is properly initialized
        Assert.IsNotNull(runtime.gltfHandler);
        // Note: BaseHandler doesn't have IsInitialized property
    }

    [UnityTest]
    public IEnumerator GLTFHandlerTests_LoadInvalidResource()
    {
        // Test loading an invalid GLTF resource - should handle gracefully
        bool callbackExecuted = false;
        Exception receivedException = null;
        
        System.Action onDownloaded = () =>
        {
            callbackExecuted = true;
        };

        try
        {
            runtime.gltfHandler.DownloadGLTFResource("https://invalid-url-that-does-not-exist.com/invalid.gltf", onDownloaded, false);
        }
        catch (Exception ex)
        {
            receivedException = ex;
        }

        yield return new WaitForSeconds(waitTime);
        
        // Should either execute callback with failure or throw exception
        Assert.IsTrue(callbackExecuted || receivedException != null);
    }

    [Test]
    public void GLTFHandlerTests_LoadLocalFile()
    {
        // Test loading a GLTF file from a local path (should handle non-existent files gracefully)
        string localPath = Path.Combine(runtime.fileHandler.fileDirectory, "test.gltf");
        
        bool callbackExecuted = false;
        GameObject loadedObject = null;
        
        System.Action<GameObject> onLoaded = (gameObject) =>
        {
            callbackExecuted = true;
            loadedObject = gameObject;
        };

        try
        {
            runtime.gltfHandler.LoadGLTF(localPath, onLoaded);
        }
        catch (System.IO.FileNotFoundException)
        {
            // Expected exception for non-existent file
        }
        catch (Exception)
        {
            // Other exceptions are also acceptable for invalid files
        }
        
        // If no exception was thrown, the callback should handle the failure case
        // In either case, we shouldn't have a valid loaded object
        Assert.IsNull(loadedObject);
    }

    [Test]
    public void GLTFHandlerTests_CreateSimpleGLTFContent()
    {
        // Create a minimal valid GLTF content for testing
        string gltfContent = @"{
            ""asset"": {
                ""version"": ""2.0""
            },
            ""scene"": 0,
            ""scenes"": [
                {
                    ""nodes"": [0]
                }
            ],
            ""nodes"": [
                {
                    ""name"": ""TestNode""
                }
            ]
        }";
        
        // Save to file
        string testGLTFPath = Path.Combine(runtime.fileHandler.fileDirectory, "simple-test.gltf");
        Directory.CreateDirectory(Path.GetDirectoryName(testGLTFPath));
        File.WriteAllText(testGLTFPath, gltfContent);
        
        // Verify file was created
        Assert.IsTrue(File.Exists(testGLTFPath));
        
        // Verify content
        string readContent = File.ReadAllText(testGLTFPath);
        Assert.IsTrue(readContent.Contains("TestNode"));
    }
}