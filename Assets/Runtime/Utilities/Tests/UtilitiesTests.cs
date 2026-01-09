// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for utility classes.
/// </summary>
public class UtilitiesTests
{
    [Test]
    public void Logging_Log_WithDefaultType_DoesNotThrow()
    {
        // Test basic logging functionality
        Assert.DoesNotThrow(() =>
        {
            Logging.Log("Test message");
        });
    }

    [Test]
    public void Logging_Log_WithDifferentTypes_DoesNotThrow()
    {
        // Test logging with different message types
        Assert.DoesNotThrow(() =>
        {
            Logging.Log("Debug message", Logging.Type.Debug);
            Logging.Log("Warning message", Logging.Type.Warning);
            Logging.Log("Error message", Logging.Type.Error);
        });
    }

    [Test]
    public void Logging_LogInfo_DoesNotThrow()
    {
        // Test LogDebug convenience method (there's no LogInfo in the class)
        Assert.DoesNotThrow(() =>
        {
            Logging.LogDebug("Debug message");
        });
    }

    [Test]
    public void Logging_LogWarning_DoesNotThrow()
    {
        // Test LogWarning convenience method  
        LogAssert.Expect(LogType.Warning, "Warning message");
        Logging.LogWarning("Warning message");
    }

    [Test]
    public void Logging_LogError_DoesNotThrow()
    {
        // Test LogError convenience method
        LogAssert.Expect(LogType.Error, "Error message");
        Logging.LogError("Error message");
    }

    [Test]
    public void BaseHandler_CanBeInstantiated()
    {
        // Test that BaseHandler can be instantiated via GameObject
        GameObject testObject = new GameObject("TestBaseHandler");
        BaseHandler handler = testObject.AddComponent<BaseHandler>();
        
        Assert.IsNotNull(handler);
        
        // Test initialization and termination
        Assert.DoesNotThrow(() =>
        {
            handler.Initialize();
            handler.Terminate();
        });
        
        Object.DestroyImmediate(testObject);
    }

    [Test] 
    public void BaseHandler_InitializeAndTerminate_LogsMessages()
    {
        GameObject testObject = new GameObject("TestBaseHandler");
        BaseHandler handler = testObject.AddComponent<BaseHandler>();
        
        // Expect initialization log
        LogAssert.Expect(LogType.Log, "[BaseHandler] Initialized.");
        handler.Initialize();
        
        // Expect termination log
        LogAssert.Expect(LogType.Log, "[BaseHandler] Terminated.");
        handler.Terminate();
        
        Object.DestroyImmediate(testObject);
    }

    [Test]
    public void Logging_RegisterAndRemoveCallback_WorksCorrectly()
    {
        // Test callback registration and removal
        bool callbackCalled = false;
        System.Action<string, Logging.Type> testCallback = (message, type) =>
        {
            callbackCalled = true;
        };
        
        // Register callback
        Logging.RegisterCallback(testCallback);
        
        // Log a message - callback should be called
        Logging.Log("Test callback message");
        Assert.IsTrue(callbackCalled);
        
        // Remove callback
        callbackCalled = false;
        Logging.RemoveCallback(testCallback);
        
        // Log another message - callback should not be called
        Logging.Log("Test callback removal");
        Assert.IsFalse(callbackCalled);
    }

    [Test]
    public void Logging_EdgeCases_HandleGracefully()
    {
        // Test edge cases
        Assert.DoesNotThrow(() =>
        {
            Logging.Log(null); // Null message
            Logging.Log(""); // Empty message
            Logging.Log("Test", (Logging.Type)999); // Invalid type
        });
    }
}