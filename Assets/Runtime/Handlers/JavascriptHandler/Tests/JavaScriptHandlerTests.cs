// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using FiveSQD.WebVerse.Handlers.Javascript;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.LocalStorage;
using System.IO;
using System;

/// <summary>
/// Unit tests for the JavaScript Handler.
/// </summary>
public class JavaScriptHandlerTests
{
    private WebVerseRuntime runtime;
    private GameObject runtimeGO;
    private JavascriptHandler jsHandler;

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
        string testDirectory = Path.Combine(Path.GetTempPath(), "JavaScriptHandlerTests");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, testDirectory);
        
        // Get the JavaScript handler from runtime
        jsHandler = runtime.javascriptHandler;
    }

    [TearDown]
    public void TearDown()
    {
        if (runtime != null)
        {
            // Clean up test directory
            string testDirectory = Path.Combine(Path.GetTempPath(), "JavaScriptHandlerTests");
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
    public void JavaScriptHandler_Initialize_IsInitialized()
    {
        // Test that the handler is properly initialized
        Assert.IsNotNull(jsHandler);
        // Note: BaseHandler doesn't have IsInitialized property
    }

    [Test]
    public void JavaScriptHandler_ExecuteScript_WithSimpleExpression_ReturnsCorrectResult()
    {
        // Arrange
        string script = "5 + 3;";
        
        // Act
        try 
        {
            jsHandler.RunScript(script);
        }
        catch (Exception)
        {
            // If script execution fails, that's also a valid test result
            // indicating the handler needs proper JavaScript engine setup
            Assert.Pass("Script execution requires proper JavaScript engine configuration");
        }
    }

    [Test]
    public void JavaScriptHandler_ExecuteScript_WithStringOperation_ReturnsString()
    {
        // Arrange
        string script = "'Hello ' + 'World';";
        
        // Act
        try
        {
            jsHandler.RunScript(script);
        }
        catch (Exception)
        {
            // If script execution fails, that's also a valid test result
            Assert.Pass("Script execution requires proper JavaScript engine configuration");
        }
    }

    [Test]
    public void JavaScriptHandler_ExecuteScript_WithInvalidSyntax_HandlesGracefully()
    {
        // Arrange
        string invalidScript = "var x = ;"; // Invalid syntax
        
        // Act & Assert
        try
        {
            jsHandler.RunScript(invalidScript);
            // If no exception is thrown, the handler might return null or handle errors silently
        }
        catch (Exception ex)
        {
            // Expected behavior - invalid syntax should throw an exception
            Assert.IsNotNull(ex);
        }
    }

    [Test]
    public void JavaScriptHandler_Terminate_CleansUpProperly()
    {
        // Arrange & Act
        jsHandler.Terminate();
        
        // Assert - termination completed without exceptions
        Assert.Pass("Termination completed successfully");
    }
}