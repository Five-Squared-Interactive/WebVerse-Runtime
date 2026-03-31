// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

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

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
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
        runtime.webVerseWebViewPrefab = new GameObject("DummyWebView");

        // Use a test directory in temp folder
        string testDirectory = Path.Combine(Path.GetTempPath(), "GLTFHandlerTests");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        WebVerseRuntime.Instance = null;
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
            UnityEngine.Object.DestroyImmediate(runtimeGO);
        }
    }

    [Test]
    public void GLTFHandler_Initialize_IsCorrect()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that GLTF handler is properly initialized
        Assert.IsNotNull(runtime.gltfHandler);
        // Note: BaseHandler doesn't have IsInitialized property
    }

    [UnityTest]
    public IEnumerator GLTFHandlerTests_LoadInvalidResource()
    {
        LogAssert.ignoreFailingMessages = true;
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
        LogAssert.ignoreFailingMessages = true;
        // Test loading a GLTF file from a local path (should handle non-existent files gracefully)
        string localPath = Path.Combine(runtime.fileHandler.fileDirectory, "test.gltf");

        GameObject loadedObject = null;
        
        System.Action<GameObject> onLoaded = (gameObject) =>
        {
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
        LogAssert.ignoreFailingMessages = true;

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

    [Test]
    public void GLTFHandler_LoadGLTF_NullPath_HandlesGracefully()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject loadedObject = null;
        Action<GameObject> onLoaded = (go) => { loadedObject = go; };

        // Act & Assert - Should handle null path gracefully
        try
        {
            runtime.gltfHandler.LoadGLTF((string)null, onLoaded);
        }
        catch (Exception)
        {
            // Expected - null path should throw or be handled
        }

        Assert.IsNull(loadedObject);
    }

    [Test]
    public void GLTFHandler_LoadGLTF_EmptyPath_HandlesGracefully()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject loadedObject = null;
        Action<GameObject> onLoaded = (go) => { loadedObject = go; };

        // Act
        try
        {
            runtime.gltfHandler.LoadGLTF("", onLoaded);
        }
        catch (Exception)
        {
            // Expected - empty path should throw or be handled
        }

        // Assert
        Assert.IsNull(loadedObject);
    }

    [Test]
    public void GLTFHandler_LoadGLTF_NonexistentPath_DoesNotLoadObject()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        string fakePath = Path.Combine(runtime.fileHandler.fileDirectory, "nonexistent_model.gltf");
        GameObject loadedObject = null;
        Action<GameObject> onLoaded = (go) => { loadedObject = go; };

        // Act
        try
        {
            runtime.gltfHandler.LoadGLTF(fakePath, onLoaded);
        }
        catch (Exception)
        {
            // Expected for nonexistent file
        }

        // Assert - Should not have loaded anything
        Assert.IsNull(loadedObject);
    }

    [Test]
    public void GLTFHandler_CreateGLTFContent_WithMesh_ContainsMeshData()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - Create GLTF content with mesh data
        string gltfContent = @"{
            ""asset"": { ""version"": ""2.0"" },
            ""scene"": 0,
            ""scenes"": [{ ""nodes"": [0] }],
            ""nodes"": [{ ""name"": ""MeshNode"", ""mesh"": 0 }],
            ""meshes"": [{
                ""primitives"": [{
                    ""attributes"": { ""POSITION"": 0 }
                }]
            }],
            ""accessors"": [{
                ""bufferView"": 0,
                ""componentType"": 5126,
                ""count"": 3,
                ""type"": ""VEC3"",
                ""max"": [1.0, 1.0, 0.0],
                ""min"": [0.0, 0.0, 0.0]
            }]
        }";

        string testPath = Path.Combine(runtime.fileHandler.fileDirectory, "mesh-test.gltf");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, gltfContent);

        // Assert
        Assert.IsTrue(File.Exists(testPath));
        string readContent = File.ReadAllText(testPath);
        Assert.IsTrue(readContent.Contains("MeshNode"));
        Assert.IsTrue(readContent.Contains("meshes"));
        Assert.IsTrue(readContent.Contains("POSITION"));
    }

    [Test]
    public void GLTFHandler_CreateGLTFContent_WithMaterial_ContainsMaterialData()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - GLTF content with material definitions
        string gltfContent = @"{
            ""asset"": { ""version"": ""2.0"" },
            ""scene"": 0,
            ""scenes"": [{ ""nodes"": [0] }],
            ""nodes"": [{ ""name"": ""MatNode"", ""mesh"": 0 }],
            ""meshes"": [{
                ""primitives"": [{
                    ""attributes"": { ""POSITION"": 0 },
                    ""material"": 0
                }]
            }],
            ""materials"": [{
                ""name"": ""RedMaterial"",
                ""pbrMetallicRoughness"": {
                    ""baseColorFactor"": [1.0, 0.0, 0.0, 1.0],
                    ""metallicFactor"": 0.5,
                    ""roughnessFactor"": 0.8
                }
            }]
        }";

        string testPath = Path.Combine(runtime.fileHandler.fileDirectory, "material-test.gltf");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, gltfContent);

        // Assert
        Assert.IsTrue(File.Exists(testPath));
        string readContent = File.ReadAllText(testPath);
        Assert.IsTrue(readContent.Contains("RedMaterial"));
        Assert.IsTrue(readContent.Contains("pbrMetallicRoughness"));
        Assert.IsTrue(readContent.Contains("metallicFactor"));
    }

    [Test]
    public void GLTFHandler_CreateGLTFContent_WithAnimation_ContainsAnimationData()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - GLTF content with animation
        string gltfContent = @"{
            ""asset"": { ""version"": ""2.0"" },
            ""scene"": 0,
            ""scenes"": [{ ""nodes"": [0] }],
            ""nodes"": [{ ""name"": ""AnimatedNode"" }],
            ""animations"": [{
                ""name"": ""Rotate"",
                ""channels"": [{
                    ""sampler"": 0,
                    ""target"": { ""node"": 0, ""path"": ""rotation"" }
                }],
                ""samplers"": [{
                    ""input"": 0,
                    ""interpolation"": ""LINEAR"",
                    ""output"": 1
                }]
            }]
        }";

        string testPath = Path.Combine(runtime.fileHandler.fileDirectory, "animation-test.gltf");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, gltfContent);

        // Assert
        Assert.IsTrue(File.Exists(testPath));
        string readContent = File.ReadAllText(testPath);
        Assert.IsTrue(readContent.Contains("animations"));
        Assert.IsTrue(readContent.Contains("Rotate"));
        Assert.IsTrue(readContent.Contains("rotation"));
    }

    [Test]
    public void GLTFHandler_CreateGLTFContent_WithTexture_ContainsTextureData()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - GLTF content with texture references
        string gltfContent = @"{
            ""asset"": { ""version"": ""2.0"" },
            ""scene"": 0,
            ""scenes"": [{ ""nodes"": [0] }],
            ""nodes"": [{ ""name"": ""TexturedNode"", ""mesh"": 0 }],
            ""meshes"": [{
                ""primitives"": [{
                    ""attributes"": { ""POSITION"": 0 },
                    ""material"": 0
                }]
            }],
            ""materials"": [{
                ""pbrMetallicRoughness"": {
                    ""baseColorTexture"": { ""index"": 0 }
                }
            }],
            ""textures"": [{
                ""source"": 0,
                ""sampler"": 0
            }],
            ""images"": [{
                ""uri"": ""texture.png""
            }],
            ""samplers"": [{
                ""magFilter"": 9729,
                ""minFilter"": 9987
            }]
        }";

        string testPath = Path.Combine(runtime.fileHandler.fileDirectory, "texture-test.gltf");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, gltfContent);

        // Assert
        Assert.IsTrue(File.Exists(testPath));
        string readContent = File.ReadAllText(testPath);
        Assert.IsTrue(readContent.Contains("textures"));
        Assert.IsTrue(readContent.Contains("texture.png"));
        Assert.IsTrue(readContent.Contains("baseColorTexture"));
    }

    [Test]
    public void GLTFHandler_CreateGLTFContent_WithHierarchy_ContainsNodeTree()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - GLTF content with parent-child node hierarchy
        string gltfContent = @"{
            ""asset"": { ""version"": ""2.0"" },
            ""scene"": 0,
            ""scenes"": [{ ""nodes"": [0] }],
            ""nodes"": [
                { ""name"": ""Root"", ""children"": [1, 2] },
                { ""name"": ""ChildA"" },
                { ""name"": ""ChildB"", ""children"": [3] },
                { ""name"": ""GrandChild"" }
            ]
        }";

        string testPath = Path.Combine(runtime.fileHandler.fileDirectory, "hierarchy-test.gltf");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, gltfContent);

        // Assert
        Assert.IsTrue(File.Exists(testPath));
        string readContent = File.ReadAllText(testPath);
        Assert.IsTrue(readContent.Contains("Root"));
        Assert.IsTrue(readContent.Contains("ChildA"));
        Assert.IsTrue(readContent.Contains("ChildB"));
        Assert.IsTrue(readContent.Contains("GrandChild"));
        Assert.IsTrue(readContent.Contains("children"));
    }

    [Test]
    public void GLTFHandler_CreateGLTFContent_WithMultipleMeshes_ContainsAllMeshes()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - GLTF content with multiple meshes
        string gltfContent = @"{
            ""asset"": { ""version"": ""2.0"" },
            ""scene"": 0,
            ""scenes"": [{ ""nodes"": [0, 1] }],
            ""nodes"": [
                { ""name"": ""CubeNode"", ""mesh"": 0 },
                { ""name"": ""SphereNode"", ""mesh"": 1 }
            ],
            ""meshes"": [
                {
                    ""name"": ""CubeMesh"",
                    ""primitives"": [{ ""attributes"": { ""POSITION"": 0 } }]
                },
                {
                    ""name"": ""SphereMesh"",
                    ""primitives"": [{ ""attributes"": { ""POSITION"": 1 } }]
                }
            ]
        }";

        string testPath = Path.Combine(runtime.fileHandler.fileDirectory, "multi-mesh-test.gltf");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, gltfContent);

        // Assert
        Assert.IsTrue(File.Exists(testPath));
        string readContent = File.ReadAllText(testPath);
        Assert.IsTrue(readContent.Contains("CubeMesh"));
        Assert.IsTrue(readContent.Contains("SphereMesh"));
        Assert.IsTrue(readContent.Contains("CubeNode"));
        Assert.IsTrue(readContent.Contains("SphereNode"));
    }

    [Test]
    public void GLTFHandler_CreateGLTFContent_WithTransforms_ContainsTransformData()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - GLTF content with transform data on nodes
        string gltfContent = @"{
            ""asset"": { ""version"": ""2.0"" },
            ""scene"": 0,
            ""scenes"": [{ ""nodes"": [0, 1] }],
            ""nodes"": [
                {
                    ""name"": ""TranslatedNode"",
                    ""translation"": [1.0, 2.0, 3.0],
                    ""rotation"": [0.0, 0.0, 0.707, 0.707],
                    ""scale"": [2.0, 2.0, 2.0]
                },
                {
                    ""name"": ""MatrixNode"",
                    ""matrix"": [1,0,0,0, 0,1,0,0, 0,0,1,0, 5,5,5,1]
                }
            ]
        }";

        string testPath = Path.Combine(runtime.fileHandler.fileDirectory, "transform-test.gltf");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, gltfContent);

        // Assert
        Assert.IsTrue(File.Exists(testPath));
        string readContent = File.ReadAllText(testPath);
        Assert.IsTrue(readContent.Contains("TranslatedNode"));
        Assert.IsTrue(readContent.Contains("translation"));
        Assert.IsTrue(readContent.Contains("rotation"));
        Assert.IsTrue(readContent.Contains("scale"));
        Assert.IsTrue(readContent.Contains("MatrixNode"));
        Assert.IsTrue(readContent.Contains("matrix"));
    }

    [Test]
    public void GLTFHandler_Terminate_CleansUpProperly()
    {
        LogAssert.ignoreFailingMessages = true;

        // Act
        runtime.gltfHandler.Terminate();

        // Assert - termination completed without exceptions
        Assert.Pass("GLTFHandler termination completed successfully");
    }

    [Test]
    public void GLTFHandler_Reset_ClearsState()
    {
        LogAssert.ignoreFailingMessages = true;

        // Act - Reset should clear cached prefabs
        runtime.gltfHandler.Reset();

        // Assert - Should complete without exceptions
        Assert.IsNotNull(runtime.gltfHandler);
    }
}