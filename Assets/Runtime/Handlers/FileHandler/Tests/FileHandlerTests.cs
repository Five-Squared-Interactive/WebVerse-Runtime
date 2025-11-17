// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.File;
using FiveSQD.WebVerse.Runtime;
using NUnit.Framework;
using UnityEngine;
using FiveSQD.WebVerse.LocalStorage;
using System.IO;

/// <summary>
/// Unit tests for the File Handler.
/// </summary>
public class FileHandlerTests
{
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
        runtime.webVerseWebViewPrefab = new GameObject("DummyWebView");
        
        // Use a test directory in temp folder instead of Application.dataPath
        string testDirectory = Path.Combine(Path.GetTempPath(), "FileHandlerTests");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (runtime != null)
        {
            // Clean up test directory
            string testDirectory = Path.Combine(Path.GetTempPath(), "FileHandlerTests");
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
    public void FileHandlerTests_General()
    {
        // To File URI.
        string rawURI = "https://test.com/some/path/to/a/resource.html#thisthing";
        string fileURI = FileHandler.ToFileURI(rawURI);
        Assert.AreEqual("https~//test.com/some/path/to/a/resource.html#thisthing", fileURI);

        // From File URI.
        string restoredURI = FileHandler.FromFileURI(fileURI);
        Assert.AreEqual(rawURI, restoredURI);

        // Create File in File Directory (byte array).
        byte[] fileData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        runtime.fileHandler.CreateFileInFileDirectory("path/to/testfile", fileData);
        Assert.IsTrue(runtime.fileHandler.FileExistsInFileDirectory("path/to/testfile"));

        // Get File in File Directory.
        byte[] readData = runtime.fileHandler.GetFileInFileDirectory("path/to/testfile");
        Assert.AreEqual(fileData, readData);

        // Delete File in File Directory.
        runtime.fileHandler.DeleteFileInFileDirectory("path/to/testfile");
        Assert.IsFalse(runtime.fileHandler.FileExistsInFileDirectory("path/to/testfile"));

        // Create File in File Directory (Texture2D).
        Texture2D image = new Texture2D(16, 16);
        image.filterMode = FilterMode.Bilinear;
        Color[] colors = new Color[256];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.red;
        }
        image.SetPixels(colors);
        image.Apply(); // Apply the changes to the texture
        runtime.fileHandler.CreateFileInFileDirectory("path/to/testfile", image);
        Assert.IsTrue(runtime.fileHandler.FileExistsInFileDirectory("path/to/testfile"));

        // Get File in File Directory.
        readData = runtime.fileHandler.GetFileInFileDirectory("path/to/testfile");
        Assert.AreEqual(image.EncodeToPNG(), readData);

        // Delete File in File Directory.
        runtime.fileHandler.DeleteFileInFileDirectory("path/to/testfile");
        Assert.IsFalse(runtime.fileHandler.FileExistsInFileDirectory("path/to/testfile"));
        
        // Clean up texture
        Object.DestroyImmediate(image);
    }
}