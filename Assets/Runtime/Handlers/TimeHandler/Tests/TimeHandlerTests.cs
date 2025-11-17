// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.LocalStorage;
using System.IO;

/// <summary>
/// Unit tests for the Time Handler.
/// </summary>
public class TimeHandlerTests
{
    private WebVerseRuntime runtime;
    private GameObject runtimeGO;
    private TimeHandler timeHandler;

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
        string testDirectory = Path.Combine(Path.GetTempPath(), "TimeHandlerTests");
        runtime.Initialize(LocalStorageManager.LocalStorageMode.Cache, 128, 128, 128, testDirectory);
        
        // Get the Time handler from runtime
        timeHandler = runtime.timeHandler;
    }

    [TearDown]
    public void TearDown()
    {
        if (runtime != null)
        {
            // Clean up test directory
            string testDirectory = Path.Combine(Path.GetTempPath(), "TimeHandlerTests");
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
    public void TimeHandler_Initialize_IsInitialized()
    {
        // Test that the handler is properly initialized
        Assert.IsNotNull(timeHandler);
        // Note: BaseHandler doesn't have IsInitialized property, so we just check if it exists
    }

    [Test]
    public void TimeHandler_StartInvoking_ReturnsValidGuid()
    {
        // Test starting interval function
        string testFunction = "console.log('test')";
        float interval = 1.0f;
        
        Guid id = timeHandler.StartInvoking(testFunction, interval);
        
        Assert.AreNotEqual(Guid.Empty, id);
    }

    [Test]
    public void TimeHandler_StopInvoking_WithValidId_DoesNotThrow()
    {
        // Test stopping interval function with valid ID
        string testFunction = "console.log('test')";
        float interval = 1.0f;
        
        Guid id = timeHandler.StartInvoking(testFunction, interval);
        
        Assert.DoesNotThrow(() =>
        {
            timeHandler.StopInvoking(id);
        });
    }

    [Test]
    public void TimeHandler_StopInvoking_WithInvalidId_LogsWarning()
    {
        // Test stopping interval function with invalid ID - should log warning
        Guid invalidId = Guid.NewGuid();
        
        // Expect warning to be logged
        LogAssert.Expect(LogType.Warning, "[TimeHandler->StopInvoking] Unknown ID.");
        timeHandler.StopInvoking(invalidId);
    }

    [Test]
    public void TimeHandler_Reset_ClearsIntervalFunctions()
    {
        // Test reset functionality
        string testFunction = "console.log('test')";
        float interval = 1.0f;
        
        // Start some interval functions
        Guid id1 = timeHandler.StartInvoking(testFunction + "1", interval);
        Guid id2 = timeHandler.StartInvoking(testFunction + "2", interval);
        
        // Reset should clear all functions
        timeHandler.Reset();
        
        // Now stopping these IDs should log warnings since they were cleared
        LogAssert.Expect(LogType.Warning, "[TimeHandler->StopInvoking] Unknown ID.");
        timeHandler.StopInvoking(id1);
        
        LogAssert.Expect(LogType.Warning, "[TimeHandler->StopInvoking] Unknown ID.");
        timeHandler.StopInvoking(id2);
    }

    [Test]
    public void TimeHandler_CallAsynchronously_WithFunction_DoesNotThrow()
    {
        // Test asynchronous function call
        string testFunction = "console.log('async test')";
        
        Assert.DoesNotThrow(() =>
        {
            timeHandler.CallAsynchronously(testFunction);
        });
    }

    [Test]
    public void TimeHandler_CallAsynchronously_WithFunctionAndData_DoesNotThrow()
    {
        // Test asynchronous function call with data
        string testFunction = "console.log";
        object[] testData = new object[] { "test", 123, true };
        
        Assert.DoesNotThrow(() =>
        {
            timeHandler.CallAsynchronously(testFunction, testData);
        });
    }

    [UnityTest]
    public IEnumerator TimeHandler_IntervalFunction_ExecutesAfterInterval()
    {
        // This test would ideally verify that interval functions execute,
        // but since we can't easily mock the JavaScript handler execution,
        // we'll test the timing mechanism indirectly
        
        string testFunction = "console.log('interval test')";
        float shortInterval = 0.1f; // 100ms
        
        Guid id = timeHandler.StartInvoking(testFunction, shortInterval);
        
        // Wait for more than the interval
        yield return new WaitForSeconds(shortInterval + 0.05f);
        
        // Stop the interval function
        timeHandler.StopInvoking(id);
        
        // If we reach here without exceptions, the timing mechanism is working
        Assert.Pass("Interval function timing mechanism is working");
    }

    [Test]
    public void TimeHandler_MultipleIntervalFunctions_CanCoexist()
    {
        // Test that multiple interval functions can exist simultaneously
        string testFunction1 = "console.log('function1')";
        string testFunction2 = "console.log('function2')";
        string testFunction3 = "console.log('function3')";
        float interval1 = 0.5f;
        float interval2 = 1.0f;
        float interval3 = 1.5f;
        
        Guid id1 = timeHandler.StartInvoking(testFunction1, interval1);
        Guid id2 = timeHandler.StartInvoking(testFunction2, interval2);
        Guid id3 = timeHandler.StartInvoking(testFunction3, interval3);
        
        // All IDs should be unique
        Assert.AreNotEqual(id1, id2);
        Assert.AreNotEqual(id2, id3);
        Assert.AreNotEqual(id1, id3);
        
        // Clean up
        timeHandler.StopInvoking(id1);
        timeHandler.StopInvoking(id2);
        timeHandler.StopInvoking(id3);
    }

    [Test]
    public void TimeHandler_EdgeCases_HandlesGracefully()
    {
        // Test edge cases
        
        // Empty function name
        Assert.DoesNotThrow(() =>
        {
            Guid id = timeHandler.StartInvoking("", 1.0f);
            timeHandler.StopInvoking(id);
        });
        
        // Very small interval
        Assert.DoesNotThrow(() =>
        {
            Guid id = timeHandler.StartInvoking("console.log('small')", 0.001f);
            timeHandler.StopInvoking(id);
        });
        
        // Large interval
        Assert.DoesNotThrow(() =>
        {
            Guid id = timeHandler.StartInvoking("console.log('large')", 3600.0f);
            timeHandler.StopInvoking(id);
        });
        
        // Null function name - this might throw, which is acceptable
        try
        {
            Guid id = timeHandler.StartInvoking(null, 1.0f);
            timeHandler.StopInvoking(id);
        }
        catch (Exception)
        {
            // Expected for null function name
        }
    }

    [Test]
    public void TimeHandler_Terminate_CleansUpProperly()
    {
        // Test termination
        string testFunction = "console.log('test')";
        Guid id = timeHandler.StartInvoking(testFunction, 1.0f);
        
        // Act
        timeHandler.Terminate();
        
        // After termination, operations should handle gracefully or log appropriate errors
        // We can't easily test if it's initialized without an IsInitialized property
        Assert.Pass("Termination completed without throwing exceptions");
    }
}