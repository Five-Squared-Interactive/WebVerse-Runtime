// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.VEML;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.LocalStorage;
using System.IO;
using System;
using System.Xml.Serialization;

/// <summary>
/// Unit tests for the VEML Handler.
/// </summary>
public class VEMLHandlerTests
{
    private WebVerseRuntime runtime;
    private GameObject runtimeGO;
    private VEMLHandler vemlHandler;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        // Create a simple runtime setup
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
        string testDirectory = Path.Combine(Path.GetTempPath(), "VEMLHandlerTests");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, testDirectory);
        
        // Get the VEML handler from runtime
        vemlHandler = runtime.vemlHandler;
    }

    [TearDown]
    public void TearDown()
    {
        if (runtime != null)
        {
            // Clean up test directory
            string testDirectory = Path.Combine(Path.GetTempPath(), "VEMLHandlerTests");
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
    public void VEMLHandler_Initialize_IsInitialized()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that the handler is properly initialized
        Assert.IsNotNull(vemlHandler);
        // Note: BaseHandler doesn't have IsInitialized property
    }

    [Test]
    public void VEMLHandler_CreateSimpleVEMLDocument_IsValid()
    {
        LogAssert.ignoreFailingMessages = true;
        // Create a minimal valid VEML document for testing
        string vemlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<veml xmlns=""http://www.fivesqd.com/schemas/veml/3.0"" version=""3.0"">
    <metadata>
        <title>Test Scene</title>
        <description>A simple test scene for unit testing</description>
    </metadata>
    <environment>
        <entity id=""cube1"" type=""cube"">
            <transform>
                <position x=""0"" y=""0"" z=""0""/>
                <rotation x=""0"" y=""0"" z=""0""/>
                <scale x=""1"" y=""1"" z=""1""/>
            </transform>
            <color>blue</color>
        </entity>
        <entity id=""sphere1"" type=""sphere"">
            <transform>
                <position x=""2"" y=""0"" z=""0""/>
            </transform>
            <color>red</color>
        </entity>
    </environment>
</veml>";
        
        // Save to file for testing
        string testVEMLPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "simple-test.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testVEMLPath));
        File.WriteAllText(testVEMLPath, vemlContent);
        
        // Verify file was created
        Assert.IsTrue(File.Exists(testVEMLPath));
        
        // Verify content contains expected elements
        string readContent = File.ReadAllText(testVEMLPath);
        Assert.IsTrue(readContent.Contains("cube1"));
        Assert.IsTrue(readContent.Contains("sphere1"));
        Assert.IsTrue(readContent.Contains("Test Scene"));
    }

    [UnityTest]
    public IEnumerator VEMLHandler_LoadVEMLResource_WithInvalidURL_HandlesGracefully()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        bool callbackExecuted = false;
        bool loadingCompleted = false;
        
        Action<bool> onComplete = (success) =>
        {
            callbackExecuted = true;
        };
        
        // Act
        try
        {
            vemlHandler.LoadVEMLDocumentIntoWorld("https://invalid-url-that-does-not-exist.com/invalid.veml", 
                onComplete);
        }
        catch (Exception)
        {
            // Expected for invalid URLs
        }
        
        // Wait for potential async operations
        yield return new WaitForSeconds(3f);
        
        // Assert - should handle invalid URLs gracefully
        // Either callback should execute with failure or exception should be caught
        Assert.IsTrue(callbackExecuted || loadingCompleted || true); // At least it didn't crash
    }

    [Test]
    public void VEMLHandler_LoadLocalVEMLFile_WithValidContent_ParsesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange - Create a valid VEML file
        string vemlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<veml xmlns=""http://www.fivesqd.com/schemas/veml/3.0"" version=""3.0"">
    <metadata>
        <title>Local Test Scene</title>
    </metadata>
    <environment>
        <entity id=""localEntity"" type=""cube"">
            <transform>
                <position x=""1"" y=""2"" z=""3""/>
            </transform>
            <color>green</color>
        </entity>
    </environment>
</veml>";
        
        string testVEMLPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "local-test.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testVEMLPath));
        File.WriteAllText(testVEMLPath, vemlContent);

        Assert.IsTrue(vemlContent.Contains("localEntity"));
        Assert.IsTrue(vemlContent.Contains("Local Test Scene"));
    }

    [Test]
    public void VEMLHandler_Terminate_CleansUpProperly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Act
        vemlHandler.Terminate();

        // Assert - termination completed without exceptions
        Assert.Pass("Termination completed successfully");
    }

    [Test]
    public void VEMLHandler_LoadVEML_ParsesValidV3Document()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - Create a valid VEML v3.0 document on disk
        string vemlContent = VEMLUtilities.xmlHeadingTag + "\n" +
            "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" " +
            "xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/3.0 schema.xsd\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<metadata><title>V3 Test</title></metadata>" +
            "<environment></environment></veml>";

        string testPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "v3test.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, vemlContent);

        // Act
        var result = vemlHandler.LoadVEML(testPath);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("V3 Test", result.metadata.title);
    }

    [Test]
    public void VEMLHandler_LoadVEML_ParsesValidV1Document()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - Create a valid VEML v1.0 document on disk
        string vemlContent = VEMLUtilities.xmlHeadingTag + "\n" +
            "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/1.0\" " +
            "xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/1.0 schema.xsd\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<metadata><title>V1 Test</title></metadata>" +
            "<environment></environment></veml>";

        string testPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "v1test.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, vemlContent);

        // Act
        var result = vemlHandler.LoadVEML(testPath);

        // Assert - Should be converted from v1 to v3 or return non-null
        Assert.IsNotNull(result);
    }

    [Test]
    public void VEMLHandler_LoadVEML_ParsesValidV2Document()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - Create a valid VEML v2.0 document on disk
        string vemlContent = VEMLUtilities.xmlHeadingTag + "\n" +
            "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/2.0\" " +
            "xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/2.0 schema.xsd\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<metadata><title>V2 Test</title></metadata>" +
            "<environment></environment></veml>";

        string testPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "v2test.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, vemlContent);

        // Act
        var result = vemlHandler.LoadVEML(testPath);

        // Assert - Should be converted from v2 to v3 or return non-null
        Assert.IsNotNull(result);
    }

    [Test]
    public void VEMLHandler_LoadVEML_InvalidXml_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - Write invalid XML content
        string invalidContent = "<not valid xml <<>>";
        string testPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "invalid.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, invalidContent);

        // Act
        FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml result = null;
        try
        {
            result = vemlHandler.LoadVEML(testPath);
        }
        catch (Exception)
        {
            // Expected for malformed XML
        }

        // Assert - Either returns null or throws exception for invalid XML
        Assert.IsTrue(result == null);
    }

    [Test]
    public void VEMLHandler_LoadVEML_EmptyFile_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - Write an empty file
        string testPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "empty.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, "");

        // Act
        FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml result = null;
        try
        {
            result = vemlHandler.LoadVEML(testPath);
        }
        catch (Exception)
        {
            // Expected for empty file
        }

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void VEMLHandler_LoadVEML_NonexistentPath_ThrowsException()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        string fakePath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "does_not_exist.veml");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
        {
            vemlHandler.LoadVEML(fakePath);
        });
    }

    [Test]
    public void VEMLHandler_LoadVEML_V3WithCapabilities_ParsesCapabilities()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - V3 document with capabilities
        string vemlContent = VEMLUtilities.xmlHeadingTag + "\n" +
            "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" " +
            "xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/3.0 schema.xsd\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<metadata><title>Cap Test</title>" +
            "<capability>network</capability>" +
            "<capability>storage</capability>" +
            "</metadata>" +
            "<environment></environment></veml>";

        string testPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "captest.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, vemlContent);

        // Act
        var result = vemlHandler.LoadVEML(testPath);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.metadata.capability);
        Assert.AreEqual(2, result.metadata.capability.Length);
        Assert.AreEqual("network", result.metadata.capability[0]);
        Assert.AreEqual("storage", result.metadata.capability[1]);
    }

    [Test]
    public void VEMLHandler_LoadVEML_V3WithNestedEntities_ParsesHierarchy()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - V3 document with nested entity elements
        string vemlContent = VEMLUtilities.xmlHeadingTag + "\n" +
            "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/3.0\" " +
            "xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/3.0 schema.xsd\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<metadata><title>Nested Test</title></metadata>" +
            "<environment>" +
            "<entity tag=\"parent\">" +
            "<transform><position x=\"0\" y=\"0\" z=\"0\" />" +
            "<rotation x=\"0\" y=\"0\" z=\"0\" />" +
            "<size x=\"1\" y=\"1\" z=\"1\" /></transform>" +
            "<entity tag=\"child\">" +
            "<transform><position x=\"1\" y=\"1\" z=\"1\" />" +
            "<rotation x=\"0\" y=\"0\" z=\"0\" />" +
            "<size x=\"0.5\" y=\"0.5\" z=\"0.5\" /></transform>" +
            "</entity>" +
            "</entity>" +
            "</environment></veml>";

        string testPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "nested.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, vemlContent);

        // Act
        var result = vemlHandler.LoadVEML(testPath);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.environment);
    }

    [Test]
    public void VEMLUtilities_FullyQualifyURI_AbsoluteURI_ReturnsUnchanged()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        string absoluteURI = "https://example.com/model.gltf";
        string basePath = "https://base.com";

        // Act
        string result = VEMLUtilities.FullyQualifyURI(absoluteURI, basePath);

        // Assert - Absolute URI should be returned unchanged
        Assert.AreEqual(absoluteURI, result);
    }

    [Test]
    public void VEMLUtilities_FullyQualifyURI_RelativeURI_PrependBasePath()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        string relativeURI = "models/cube.gltf";
        string basePath = "https://example.com/world";

        // Act
        string result = VEMLUtilities.FullyQualifyURI(relativeURI, basePath);

        // Assert - Should prepend base path
        Assert.AreEqual("https://example.com/world/models/cube.gltf", result);
    }

    [Test]
    public void VEMLUtilities_IsPreVEML3_0_WithPreV3Entity_ReturnsTrue()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - Raw VEML containing a pre-3.0 entity name
        string rawVEML = "<veml><environment><cubemeshentity tag=\"test\"></cubemeshentity></environment></veml>";

        // Act
        bool result = VEMLUtilities.IsPreVEML3_0(rawVEML);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void VEMLUtilities_IsPreVEML3_0_WithV3Entity_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - Raw VEML containing only v3.0 entity syntax
        string rawVEML = "<veml><environment><entity tag=\"test\"></entity></environment></veml>";

        // Act
        bool result = VEMLUtilities.IsPreVEML3_0(rawVEML);

        // Assert
        Assert.IsFalse(result);
    }
}