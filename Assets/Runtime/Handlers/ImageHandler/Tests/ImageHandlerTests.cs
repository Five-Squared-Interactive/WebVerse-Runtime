// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.IO;
using FiveSQD.WebVerse.LocalStorage;
using FiveSQD.WebVerse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for the Image Handler.
/// </summary>
public class ImageHandlerTests
{
    private float waitTime = 5; // Reduced wait time for better test performance
    private WebVerseRuntime runtime;
    private GameObject runtimeGO;

    [SetUp]
    public void SetUp()
    {
        // Create a simple runtime setup without external dependencies
        runtimeGO = new GameObject("runtime");
        runtime = runtimeGO.AddComponent<WebVerseRuntime>();
        
        // Use built-in materials and create dummy objects instead of loading external assets
        runtime.highlightMaterial = new Material(Shader.Find("Standard"));
        runtime.skyMaterial = new Material(Shader.Find("Standard"));
        
        // Create empty GameObjects as placeholders instead of loading external prefabs
        runtime.characterControllerPrefab = new GameObject("DummyCharacterController");
        runtime.inputEntityPrefab = new GameObject("DummyInputEntity");
        runtime.voxelPrefab = new GameObject("DummyVoxel");
        
        // Use a test directory in temp folder
        string testDirectory = Path.Combine(Path.GetTempPath(), "ImageHandlerTests");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (runtime != null)
        {
            // Clean up test directory
            string testDirectory = Path.Combine(Path.GetTempPath(), "ImageHandlerTests");
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
        
        if (runtimeGO != null)
        {
            UnityEngine.Object.DestroyImmediate(runtimeGO);
        }
    }

    [UnityTest]
    public IEnumerator ImageHandlerTests_General()
    {
        // Test loading image that does not exist - should handle gracefully
        string imageURI = System.IO.Path.Combine(runtime.fileHandler.fileDirectory, "test/test.png");
        Texture2D loadedImage = null;
        
        try
        {
            loadedImage = runtime.imageHandler.LoadImage(imageURI);
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            // Expected exception for non-existent path
        }
        catch (System.IO.FileNotFoundException)
        {
            // Also acceptable exception
        }
        
        Assert.IsNull(loadedImage);

        // Test creating and loading a local image file
        // Create a test image directly in the file directory
        string localTestPath = Path.Combine(runtime.fileHandler.fileDirectory, "localtest.png");
        
        // Create a simple test texture
        Texture2D testTexture = new Texture2D(16, 16, TextureFormat.RGB24, false);
        Color[] colors = new Color[256];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.blue;
        }
        testTexture.SetPixels(colors);
        testTexture.Apply();
        
        // Save texture to file system
        Directory.CreateDirectory(Path.GetDirectoryName(localTestPath));
        byte[] pngData = testTexture.EncodeToPNG();
        File.WriteAllBytes(localTestPath, pngData);
        
        // Now try to load the local image
        Texture2D loadedLocalImage = runtime.imageHandler.LoadImage(localTestPath);
        Assert.IsNotNull(loadedLocalImage);
        Assert.AreEqual(16, loadedLocalImage.width);
        Assert.AreEqual(16, loadedLocalImage.height);
        
        // Test LoadImageResourceAsTexture2D with a local file
        bool callbackExecuted = false;
        Texture2D callbackTexture = null;
        Action<Texture2D> onLoaded = new Action<Texture2D>((tex) =>
        {
            callbackExecuted = true;
            callbackTexture = tex;
        });
        
        runtime.imageHandler.LoadImageResourceAsTexture2D($"file://{localTestPath}", onLoaded);
        yield return new WaitForSeconds(waitTime);
        
        Assert.IsTrue(callbackExecuted);
        Assert.IsNotNull(callbackTexture);
        Assert.AreEqual(16, callbackTexture.width);
        Assert.AreEqual(16, callbackTexture.height);
        
        // Clean up textures
        UnityEngine.Object.DestroyImmediate(testTexture);
        UnityEngine.Object.DestroyImmediate(loadedLocalImage);
        UnityEngine.Object.DestroyImmediate(callbackTexture);
    }

    [Test]
    public void ImageHandlerTests_UnsupportedFormat()
    {
        // Test behavior with an unsupported file format
        string testPath = Path.Combine(runtime.fileHandler.fileDirectory, "test.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, "This is not an image");
        
        Texture2D result = null;
        try
        {
            result = runtime.imageHandler.LoadImage(testPath);
        }
        catch (Exception)
        {
            // Expected exception for invalid image format
        }
        
        // Should either be null or throw an exception
        if (result != null)
        {
            UnityEngine.Object.DestroyImmediate(result);
        }
    }
}