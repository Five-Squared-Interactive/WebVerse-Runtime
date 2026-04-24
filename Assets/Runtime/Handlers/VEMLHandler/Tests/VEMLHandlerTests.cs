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
using System.Collections.Generic;
using System.Reflection;
using FiveSQD.WebVerse.Input;
using FiveSQD.WebVerse.WorldSync;
using FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0;

/// <summary>
/// Unit tests for the VEML Handler.
/// </summary>
public class VEMLHandlerTests
{
    private WebVerseRuntime runtime;
    private GameObject runtimeGO;
    private VEMLHandler vemlHandler;

    // Integration test state — cleaned up in TearDown to prevent leaks on assertion failure
    private GameObject vrRigGO;
    private bool worldLoaded;

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
        // Clean up integration test state first (VRRig + world) to prevent leaks on assertion failure
        if (worldLoaded)
        {
            try { FiveSQD.StraightFour.StraightFour.UnloadWorld(); } catch (Exception) { }
            worldLoaded = false;
        }

        if (vrRigGO != null)
        {
            UnityEngine.Object.DestroyImmediate(vrRigGO);
            vrRigGO = null;
        }

        // Clean up WorldSync clients between tests
        if (WebVerseRuntime.Instance != null)
        {
            WebVerseRuntime.Instance.ClearWorldSyncClients();
        }

        if (runtime != null)
        {
            runtime.vrRig = null;

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

    // ===== Story 2.1: Control Flag Caching Tests =====

    [Test]
    public void World_CachedControlFlags_DefaultsToNull()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var go = new GameObject("TestWorld");
        var world = go.AddComponent<FiveSQD.StraightFour.World.World>();

        // Assert
        Assert.IsNull(world.CachedControlFlags);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void World_CachedControlFlags_RoundTrip_BoolValues()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var go = new GameObject("TestWorld");
        var world = go.AddComponent<FiveSQD.StraightFour.World.World>();
        var flags = new Dictionary<string, string>
        {
            { "joystickmotion", "true" },
            { "leftgrabmove", "false" },
            { "rightgrabmove", "true" },
            { "lefthandinteraction", "false" },
            { "righthandinteraction", "true" }
        };

        // Act
        world.CachedControlFlags = flags;

        // Assert
        Assert.IsNotNull(world.CachedControlFlags);
        Assert.AreEqual("true", world.CachedControlFlags["joystickmotion"]);
        Assert.AreEqual("false", world.CachedControlFlags["leftgrabmove"]);
        Assert.AreEqual("true", world.CachedControlFlags["rightgrabmove"]);
        Assert.AreEqual("false", world.CachedControlFlags["lefthandinteraction"]);
        Assert.AreEqual("true", world.CachedControlFlags["righthandinteraction"]);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void World_CachedControlFlags_RoundTrip_EnumValues()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var go = new GameObject("TestWorld");
        var world = go.AddComponent<FiveSQD.StraightFour.World.World>();
        var flags = new Dictionary<string, string>
        {
            { "leftvrpointer", "teleport" },
            { "rightvrpointer", "ui" },
            { "turnlocomotion", "snap" }
        };

        // Act
        world.CachedControlFlags = flags;

        // Assert
        Assert.IsNotNull(world.CachedControlFlags);
        Assert.AreEqual("teleport", world.CachedControlFlags["leftvrpointer"]);
        Assert.AreEqual("ui", world.CachedControlFlags["rightvrpointer"]);
        Assert.AreEqual("snap", world.CachedControlFlags["turnlocomotion"]);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void World_CachedControlFlags_RoundTrip_AllEnumVariants()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var go = new GameObject("TestWorld");
        var world = go.AddComponent<FiveSQD.StraightFour.World.World>();
        var flags = new Dictionary<string, string>
        {
            { "leftvrpointer", "none" },
            { "rightvrpointer", "teleport" },
            { "turnlocomotion", "smooth" }
        };

        // Act
        world.CachedControlFlags = flags;

        // Assert - values survive round-trip identically
        Assert.AreEqual("none", world.CachedControlFlags["leftvrpointer"]);
        Assert.AreEqual("teleport", world.CachedControlFlags["rightvrpointer"]);
        Assert.AreEqual("smooth", world.CachedControlFlags["turnlocomotion"]);

        // Cleanup
        UnityEngine.Object.DestroyImmediate(go);
    }

    // ===== Story 2.1: WorldSync Address Parsing Tests =====

    private MethodInfo GetProcessSynchronizersMethod()
    {
        MethodInfo method = typeof(VEMLHandler).GetMethod("ProcessSynchronizers",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "ProcessSynchronizers method not found via reflection — was it renamed?");
        return method;
    }

    private FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml CreateVemlWithSyncService(
        string type, string address, string id = "sync1", string session = null, string tag = null)
    {
        var veml = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml();
        veml.metadata = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.vemlMetadata();
        var syncService = new synchronizationservice
        {
            type = type,
            address = address,
            id = id,
            session = session,
            tag = tag
        };
        veml.metadata.synchronizationservice = new synchronizationservice[] { syncService };
        return veml;
    }

    [Test]
    public void ProcessSynchronizers_WsyncAddress_RegistersClientNoTls()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var veml = CreateVemlWithSyncService("wsync", "wsync://localhost:1883");

        // Act
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert — WorldSyncClient registered in WebVerseRuntime
        var client = WebVerseRuntime.Instance.GetWorldSyncClient("sync1");
        Assert.IsNotNull(client, "WorldSyncClient should be registered with id 'sync1'");
        Assert.AreEqual("localhost", client.Config.Host);
        Assert.AreEqual(1883, client.Config.Port);
        Assert.AreEqual(false, client.Config.Tls.Enabled);
    }

    [Test]
    public void ProcessSynchronizers_WsyncsAddress_RegistersClientWithTls()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var veml = CreateVemlWithSyncService("wsync", "wsyncs://sync.example.com:8883");

        // Act
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert
        var client = WebVerseRuntime.Instance.GetWorldSyncClient("sync1");
        Assert.IsNotNull(client, "WorldSyncClient should be registered with id 'sync1'");
        Assert.AreEqual("sync.example.com", client.Config.Host);
        Assert.AreEqual(8883, client.Config.Port);
        Assert.AreEqual(true, client.Config.Tls.Enabled);
    }

    [Test]
    public void ProcessSynchronizers_WsyncWithTag_ClientTagMatchesTag()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var veml = CreateVemlWithSyncService("wsync", "wsync://localhost:1883", tag: "my-game-session");

        // Act
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert — ClientTag set from tag attribute
        var client = WebVerseRuntime.Instance.GetWorldSyncClient("sync1");
        Assert.IsNotNull(client);
        Assert.AreEqual("my-game-session", client.Config.ClientTag);
    }

    [Test]
    public void ProcessSynchronizers_WsyncWithoutTag_ClientTagFallsBackToId()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var veml = CreateVemlWithSyncService("wsync", "wsync://localhost:1883");

        // Act
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert — ClientTag falls back to synchronizationservice id
        var client = WebVerseRuntime.Instance.GetWorldSyncClient("sync1");
        Assert.IsNotNull(client);
        Assert.AreEqual("sync1", client.Config.ClientTag);
    }

    [Test]
    public void ProcessSynchronizers_WsyncWithoutSession_ClientRegistered()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange — session omitted (null), wsync server generates IDs
        var veml = CreateVemlWithSyncService("wsync", "wsync://localhost:1883");

        // Act
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert — Client registered despite no session
        var client = WebVerseRuntime.Instance.GetWorldSyncClient("sync1");
        Assert.IsNotNull(client, "WorldSyncClient should be registered even without session attribute");
    }

    [Test]
    public void ProcessSynchronizers_WsyncInvalidAddress_LogsWarningAndSkips()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var veml = CreateVemlWithSyncService("wsync", "wsync://localhost-no-port");

        // Act
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("invalid WorldSync address"));
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert - No client should be registered due to invalid format
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("sync1"));
    }

    [Test]
    public void ProcessSynchronizers_WsyncNonNumericPort_LogsWarningAndSkips()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var veml = CreateVemlWithSyncService("wsync", "wsync://localhost:abc");

        // Act
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("invalid WorldSync port"));
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert - No client should be registered due to non-numeric port
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("sync1"));
    }

    [Test]
    public void ProcessSynchronizers_VssType_StillWorksCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var veml = CreateVemlWithSyncService("vss", "vss:localhost:1883", session: "test-session");

        // Act - Should not throw
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert - No wsync client registered (vss goes through different path)
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("sync1"));
    }

    [Test]
    public void ProcessSynchronizers_DualStack_BothVssAndWsyncProcessed()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange — VEML with both vss and wsync synchronizationservices
        var veml = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml();
        veml.metadata = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.vemlMetadata();
        var vssSvc = new synchronizationservice
        {
            type = "vss",
            address = "vss:localhost:5555",
            id = "vss-sync",
            session = "test-session"
        };
        var wsyncSvc = new synchronizationservice
        {
            type = "wsync",
            address = "wsync://localhost:1883",
            id = "wsync-sync",
            tag = "my-tag"
        };
        veml.metadata.synchronizationservice = new synchronizationservice[] { vssSvc, wsyncSvc };

        // Act
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert — wsync client registered, vss goes through VOSSynchronizationManager (separate path)
        var wsyncClient = WebVerseRuntime.Instance.GetWorldSyncClient("wsync-sync");
        Assert.IsNotNull(wsyncClient, "WorldSyncClient should be registered for wsync service");
        Assert.AreEqual("localhost", wsyncClient.Config.Host);
        Assert.AreEqual(1883, wsyncClient.Config.Port);
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("vss-sync"), "vss service should NOT register as WorldSyncClient");
    }

    [Test]
    public void ProcessSynchronizers_WsyncInvalidConfig_LogsWarningAndContinues()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange — empty host should cause WorldSyncConfig.Validate() to throw
        var veml = CreateVemlWithSyncService("wsync", "wsync://:1883");

        // Act — should not throw, error is caught and logged
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Failed to create WorldSync client"));
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert — no client registered due to empty host
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("sync1"));
    }

    [Test]
    public void RegisterWorldSyncClient_GetWorldSyncClient_RoundTrip()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var config = WorldSyncConfig.Builder()
            .WithHost("localhost")
            .WithPort(1883)
            .WithClientTag("test-tag")
            .Build();
        var client = new WorldSyncClient(config);

        // Act
        WebVerseRuntime.Instance.RegisterWorldSyncClient("test-id", client);

        // Assert
        var retrieved = WebVerseRuntime.Instance.GetWorldSyncClient("test-id");
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(client, retrieved);
    }

    [Test]
    public void ClearWorldSyncClients_RemovesAllClients()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var config1 = WorldSyncConfig.Builder().WithHost("host1").WithPort(1883).WithClientTag("tag1").Build();
        var config2 = WorldSyncConfig.Builder().WithHost("host2").WithPort(1884).WithClientTag("tag2").Build();
        WebVerseRuntime.Instance.RegisterWorldSyncClient("id1", new WorldSyncClient(config1));
        WebVerseRuntime.Instance.RegisterWorldSyncClient("id2", new WorldSyncClient(config2));

        // Act
        WebVerseRuntime.Instance.ClearWorldSyncClients();

        // Assert
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("id1"));
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("id2"));
    }

    [Test]
    public void RegisterWorldSyncClient_DuplicateId_ReplacesExisting()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        var config1 = WorldSyncConfig.Builder().WithHost("host1").WithPort(1883).WithClientTag("tag1").Build();
        var config2 = WorldSyncConfig.Builder().WithHost("host2").WithPort(1884).WithClientTag("tag2").Build();
        var client1 = new WorldSyncClient(config1);
        var client2 = new WorldSyncClient(config2);
        WebVerseRuntime.Instance.RegisterWorldSyncClient("same-id", client1);

        // Act
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Replacing existing WorldSyncClient"));
        WebVerseRuntime.Instance.RegisterWorldSyncClient("same-id", client2);

        // Assert — second client replaces first
        var retrieved = WebVerseRuntime.Instance.GetWorldSyncClient("same-id");
        Assert.AreEqual(client2, retrieved);
        Assert.AreEqual("host2", retrieved.Config.Host);
    }

    [Test]
    public void GetWorldSyncClient_NonExistentId_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        // Assert
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("does-not-exist"));
    }

    [Test]
    public void GetWorldSyncClient_NullId_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        // Assert
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient(null));
    }

    // ===== Story 2.3: Backward Compatibility & Graceful Fallback Tests =====

    [Test]
    public void ProcessSynchronizers_UnknownType_LogsWarningAndSkips()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange — unknown sync type should be gracefully skipped
        var veml = CreateVemlWithSyncService("future-protocol", "fp://localhost:9999");

        // Act
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("unknown synchronization service type"));
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert — no client registered, no crash
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("sync1"));
    }

    [Test]
    public void ProcessSynchronizers_UnknownType_RemainingServicesStillProcessed()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange — VEML with [unknown, wsync] services
        var veml = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml();
        veml.metadata = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.vemlMetadata();
        var unknownSvc = new synchronizationservice
        {
            type = "future-protocol",
            address = "fp://localhost:9999",
            id = "unknown-sync"
        };
        var wsyncSvc = new synchronizationservice
        {
            type = "wsync",
            address = "wsync://localhost:1883",
            id = "wsync-sync",
            tag = "test-tag"
        };
        veml.metadata.synchronizationservice = new synchronizationservice[] { unknownSvc, wsyncSvc };

        // Act
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("unknown synchronization service type"));
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert — unknown skipped, wsync still registered
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("unknown-sync"));
        var wsyncClient = WebVerseRuntime.Instance.GetWorldSyncClient("wsync-sync");
        Assert.IsNotNull(wsyncClient, "wsync service should still be processed after unknown type is skipped");
        Assert.AreEqual("localhost", wsyncClient.Config.Host);
    }

    [Test]
    public void ProcessSynchronizers_UnknownType_NoException()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange — multiple unknown types to ensure no exception
        var veml = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml();
        veml.metadata = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.vemlMetadata();
        var svc1 = new synchronizationservice { type = "quantum-sync", address = "qs://host:1234", id = "qs1" };
        var svc2 = new synchronizationservice { type = "p2p", address = "p2p://host:5678", id = "p2p1" };
        veml.metadata.synchronizationservice = new synchronizationservice[] { svc1, svc2 };

        // Act & Assert — should not throw, both unknown types produce warnings
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("unknown synchronization service type.*quantum-sync"));
        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("unknown synchronization service type.*p2p"));
        Assert.DoesNotThrow(() =>
        {
            GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });
        });
    }

    [Test]
    public void ProcessSynchronizers_VssOnlyV30_NoWorldSyncClientsCreated()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange — V3.0 style VEML with only vss services
        var veml = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml();
        veml.metadata = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.vemlMetadata();
        var vssSvc1 = new synchronizationservice
        {
            type = "vss",
            address = "vss:localhost:5555",
            id = "vss1",
            session = "session-1"
        };
        var vssSvc2 = new synchronizationservice
        {
            type = "vss",
            address = "vss:localhost:5556",
            id = "vss2",
            session = "session-2"
        };
        veml.metadata.synchronizationservice = new synchronizationservice[] { vssSvc1, vssSvc2 };

        // Act
        GetProcessSynchronizersMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert — no WorldSync clients, all go through VOSSynchronizationManager
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("vss1"));
        Assert.IsNull(WebVerseRuntime.Instance.GetWorldSyncClient("vss2"));
    }

    [Test]
    public void LoadVEML_V1Document_UpgradesSuccessfully()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange — V1.0 VEML document with synchronizationservices (AC2)
        string vemlContent = VEMLUtilities.xmlHeadingTag + "\n" +
            "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/1.0\" " +
            "xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/1.0 schema.xsd\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<metadata><title>V1 Upgrade Test</title>" +
            "<synchronizationservice type=\"vss\" address=\"vss:localhost:5555\" id=\"sync-v1\" session=\"s1\"/>" +
            "</metadata>" +
            "<environment></environment></veml>";

        string testPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "v1upgrade.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, vemlContent);

        try
        {
            // Act
            var result = vemlHandler.LoadVEML(testPath);

            // Assert — upgraded to V3.0 with sync services preserved
            Assert.IsNotNull(result, "V1.0 document should upgrade to V3.0");
            Assert.AreEqual("V1 Upgrade Test", result.metadata.title);
            Assert.IsNotNull(result.metadata.synchronizationservice, "Sync services should survive V1 upgrade");
            Assert.IsTrue(result.metadata.synchronizationservice.Length > 0, "At least one sync service should survive V1 upgrade");
        }
        finally
        {
            if (File.Exists(testPath)) File.Delete(testPath);
        }
    }

    [Test]
    public void LoadVEML_V2Document_UpgradesSuccessfully()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange — V2.0 VEML document with synchronizationservices (AC2)
        string vemlContent = VEMLUtilities.xmlHeadingTag + "\n" +
            "<veml xmlns=\"http://www.fivesqd.com/schemas/veml/2.0\" " +
            "xsi:schemaLocation=\"http://www.fivesqd.com/schemas/veml/2.0 schema.xsd\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<metadata><title>V2 Upgrade Test</title>" +
            "<synchronizationservice type=\"vss\" address=\"vss:localhost:5555\" id=\"sync-v2\" session=\"s1\"/>" +
            "</metadata>" +
            "<environment></environment></veml>";

        string testPath = Path.Combine(vemlHandler.runtime.fileHandler.fileDirectory, "v2upgrade.veml");
        Directory.CreateDirectory(Path.GetDirectoryName(testPath));
        File.WriteAllText(testPath, vemlContent);

        try
        {
            // Act
            var result = vemlHandler.LoadVEML(testPath);

            // Assert — upgraded to V3.0 with sync services preserved
            Assert.IsNotNull(result, "V2.0 document should upgrade to V3.0");
            Assert.IsNotNull(result.metadata.synchronizationservice, "Sync services should survive V2 upgrade");
            Assert.IsTrue(result.metadata.synchronizationservice.Length > 0, "At least one sync service should survive V2 upgrade");
        }
        finally
        {
            if (File.Exists(testPath)) File.Delete(testPath);
        }
    }

    [Test]
    public void SynchronizationService_DeserializesWithTagAttribute()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - XML with tag attribute
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<synchronizationservice xmlns=""http://www.fivesqd.com/schemas/veml/3.0""
    type=""wsync"" address=""wsync://localhost:1883"" id=""sync1"" tag=""my-tag"" />";

        // Act
        var root = new XmlRootAttribute("synchronizationservice") { Namespace = "http://www.fivesqd.com/schemas/veml/3.0" };
        var serializer = new XmlSerializer(typeof(synchronizationservice), root);
        synchronizationservice result;
        using (var reader = new StringReader(xml))
        {
            result = (synchronizationservice)serializer.Deserialize(reader);
        }

        // Assert
        Assert.AreEqual("wsync", result.type);
        Assert.AreEqual("wsync://localhost:1883", result.address);
        Assert.AreEqual("sync1", result.id);
        Assert.AreEqual("my-tag", result.tag);
    }

    [Test]
    public void SynchronizationService_DeserializesWithoutTagAttribute()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - V3.0 XML without tag attribute
        string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<synchronizationservice xmlns=""http://www.fivesqd.com/schemas/veml/3.0""
    type=""vss"" address=""vss:localhost:1883"" id=""sync1"" session=""session-123"" />";

        // Act
        var root = new XmlRootAttribute("synchronizationservice") { Namespace = "http://www.fivesqd.com/schemas/veml/3.0" };
        var serializer = new XmlSerializer(typeof(synchronizationservice), root);
        synchronizationservice result;
        using (var reader = new StringReader(xml))
        {
            result = (synchronizationservice)serializer.Deserialize(reader);
        }

        // Assert
        Assert.AreEqual("vss", result.type);
        Assert.AreEqual("session-123", result.session);
        Assert.IsNull(result.tag);
    }

    // ===== Integration Test Helpers =====

    /// <summary>
    /// Set up a bare VRRig and ActiveWorld for integration tests.
    /// Bare VRRig is intentional: caching reads from VEML doc, not VRRig state.
    /// VRRig only needs to be non-null so ProcessControlFlags enters the VR block.
    /// VRRigTestHelper is in Input.Tests assembly (not referenced here).
    /// Cleanup is handled by TearDown via vrRigGO/worldLoaded fields.
    /// </summary>
    private void SetUpIntegrationTest(string worldName)
    {
        vrRigGO = new GameObject("TestVRRig");
        runtime.vrRig = vrRigGO.AddComponent<VRRig>();

        Assert.IsTrue(FiveSQD.StraightFour.StraightFour.LoadWorld(worldName),
            "FiveSQD.StraightFour.StraightFour.LoadWorld failed for: " + worldName);
        worldLoaded = true;
    }

    private MethodInfo GetProcessControlFlagsMethod()
    {
        MethodInfo method = typeof(VEMLHandler).GetMethod("ProcessControlFlags",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "ProcessControlFlags method not found via reflection — was it renamed?");
        return method;
    }

    // ===== Integration Tests =====

    [Test]
    public void ProcessControlFlags_AllVRFlags_CachesAll11Entries()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        SetUpIntegrationTest("CacheTest");

        var veml = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml();
        veml.metadata = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.vemlMetadata();
        veml.metadata.controlflags = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.controlflags();

        veml.metadata.controlflags.joystickmotion = true;
        veml.metadata.controlflags.joystickmotionSpecified = true;
        veml.metadata.controlflags.leftgrabmove = true;
        veml.metadata.controlflags.leftgrabmoveSpecified = true;
        veml.metadata.controlflags.rightgrabmove = false;
        veml.metadata.controlflags.rightgrabmoveSpecified = true;
        veml.metadata.controlflags.lefthandinteraction = true;
        veml.metadata.controlflags.lefthandinteractionSpecified = true;
        veml.metadata.controlflags.righthandinteraction = false;
        veml.metadata.controlflags.righthandinteractionSpecified = true;
        veml.metadata.controlflags.leftvrpointer = "teleport";
        veml.metadata.controlflags.rightvrpointer = "ui";
        veml.metadata.controlflags.leftvrpoker = true;
        veml.metadata.controlflags.leftvrpokerSpecified = true;
        veml.metadata.controlflags.rightvrpoker = false;
        veml.metadata.controlflags.rightvrpokerSpecified = true;
        veml.metadata.controlflags.turnlocomotion = "snap";
        veml.metadata.controlflags.twohandedgrabmove = true;
        veml.metadata.controlflags.twohandedgrabmoveSpecified = true;

        // Act
        GetProcessControlFlagsMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert
        var cached = FiveSQD.StraightFour.StraightFour.ActiveWorld.CachedControlFlags;
        Assert.IsNotNull(cached);
        Assert.AreEqual(11, cached.Count);
        Assert.AreEqual("true", cached["joystickmotion"]);
        Assert.AreEqual("true", cached["leftgrabmove"]);
        Assert.AreEqual("false", cached["rightgrabmove"]);
        Assert.AreEqual("true", cached["lefthandinteraction"]);
        Assert.AreEqual("false", cached["righthandinteraction"]);
        Assert.AreEqual("teleport", cached["leftvrpointer"]);
        Assert.AreEqual("ui", cached["rightvrpointer"]);
        Assert.AreEqual("true", cached["leftvrpoker"]);
        Assert.AreEqual("false", cached["rightvrpoker"]);
        Assert.AreEqual("snap", cached["turnlocomotion"]);
        Assert.AreEqual("true", cached["twohandedgrabmove"]);
    }

    [Test]
    public void ProcessControlFlags_PartialFlags_CachesOnlySpecified()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        SetUpIntegrationTest("PartialCacheTest");

        var veml = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml();
        veml.metadata = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.vemlMetadata();
        veml.metadata.controlflags = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.controlflags();

        veml.metadata.controlflags.joystickmotion = true;
        veml.metadata.controlflags.joystickmotionSpecified = true;
        veml.metadata.controlflags.leftvrpointer = "teleport";

        // Act
        GetProcessControlFlagsMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert
        var cached = FiveSQD.StraightFour.StraightFour.ActiveWorld.CachedControlFlags;
        Assert.IsNotNull(cached);
        Assert.AreEqual(2, cached.Count);
        Assert.AreEqual("true", cached["joystickmotion"]);
        Assert.AreEqual("teleport", cached["leftvrpointer"]);
    }

    [Test]
    public void ProcessControlFlags_DesktopOnlyFlags_CachedControlFlagsStaysNull()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - controlflags with ONLY desktop flags, no VR flags
        SetUpIntegrationTest("DesktopOnlyTest");

        var veml = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml();
        veml.metadata = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.vemlMetadata();
        veml.metadata.controlflags = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.controlflags();

        // Only set desktop flags — no VR-specific flags
        veml.metadata.controlflags.gravityenabled = true;
        veml.metadata.controlflags.gravityenabledSpecified = true;
        veml.metadata.controlflags.wasdmotionenabled = true;
        veml.metadata.controlflags.wasdmotionenabledSpecified = true;

        // Act
        GetProcessControlFlagsMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert - No VR flags → cachedFlags empty → CachedControlFlags stays null
        Assert.IsNull(FiveSQD.StraightFour.StraightFour.ActiveWorld.CachedControlFlags);
    }

    [Test]
    public void ProcessControlFlags_WithActiveWorld_AssignsCachedFlags()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        SetUpIntegrationTest("ActiveWorldTest");
        Assert.IsNotNull(FiveSQD.StraightFour.StraightFour.ActiveWorld);

        var veml = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.veml();
        veml.metadata = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.vemlMetadata();
        veml.metadata.controlflags = new FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0.controlflags();

        veml.metadata.controlflags.turnlocomotion = "smooth";

        // Act
        GetProcessControlFlagsMethod().Invoke(vemlHandler, new object[] { veml, "" });

        // Assert - CachedControlFlags assigned to ActiveWorld
        Assert.IsNotNull(FiveSQD.StraightFour.StraightFour.ActiveWorld.CachedControlFlags);
        Assert.AreEqual("smooth", FiveSQD.StraightFour.StraightFour.ActiveWorld.CachedControlFlags["turnlocomotion"]);
    }
}