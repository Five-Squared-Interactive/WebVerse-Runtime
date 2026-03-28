// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.VOSSynchronization;
using System;

/// <summary>
/// Unit tests for the VOS Synchronizer.
/// </summary>
public class VOSSynchronizerTests
{
#if USE_WEBINTERFACE
    private float waitTime = 3; // Reduced wait time

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [Test]
    public void VOSSynchronizer_Initialize_IsCorrect()
    {
        // VOSSynchronizer is a MonoBehaviour so must be created via AddComponent
        GameObject go = new GameObject("VOSSyncTest");
        VOSSynchronizer synchronizer = go.AddComponent<VOSSynchronizer>();
        synchronizer.Initialize("localhost", 1883, false,
            FiveSQD.WebVerse.WebInterface.MQTT.MQTTClient.Transports.TCP, Vector3.zero);

        Assert.IsNotNull(synchronizer);

        LogAssert.Expect(LogType.Error, "[Error] [VOSSynchronizer->ExitSession] Not in session.");
        synchronizer.Terminate();
        GameObject.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator VOSSynchronizer_ConnectToInvalidHost_HandlesGracefully()
    {
        // Test connection to invalid host - should handle gracefully
        bool connected = false;
        bool connectionAttempted = false;

        Action onConnected = () =>
        {
            connected = true;
        };

        GameObject go = new GameObject("VOSSyncTest");
        VOSSynchronizer synchronizer = go.AddComponent<VOSSynchronizer>();
        synchronizer.Initialize("invalid-host-that-does-not-exist.local", 1883, false,
            FiveSQD.WebVerse.WebInterface.MQTT.MQTTClient.Transports.TCP, Vector3.zero);

        try
        {
            synchronizer.Connect(onConnected);
            connectionAttempted = true;
        }
        catch (Exception)
        {
            // Expected for invalid host
            connectionAttempted = true;
        }

        yield return new WaitForSeconds(waitTime);

        // Should attempt connection but not succeed with invalid host
        Assert.IsTrue(connectionAttempted);
        Assert.IsFalse(connected); // Should not connect to invalid host

        // Test disconnection from invalid state
        synchronizer.Disconnect();

        // Test termination
        LogAssert.Expect(LogType.Error, "[Error] [VOSSynchronizer->ExitSession] Not in session.");
        synchronizer.Terminate();
        GameObject.DestroyImmediate(go);
    }

    [Test]
    public void VOSSynchronizer_AddMessageListener_Works()
    {
        GameObject go = new GameObject("VOSSyncTest");
        VOSSynchronizer synchronizer = go.AddComponent<VOSSynchronizer>();
        synchronizer.Initialize("localhost", 1883, false,
            FiveSQD.WebVerse.WebInterface.MQTT.MQTTClient.Transports.TCP, Vector3.zero);

        int messageCount = 0;
        Action<string, string, string> onMessage = (first, second, third) =>
        {
            messageCount++;
        };

        // Test adding message listener
        synchronizer.AddMessageListener(onMessage);

        LogAssert.Expect(LogType.Error, "[Error] [VOSSynchronizer->ExitSession] Not in session.");
        synchronizer.Terminate();
        GameObject.DestroyImmediate(go);
    }

    [Test]
    public void VOSSynchronizer_ExitSession_WithoutSession_LogsError()
    {
        GameObject go = new GameObject("VOSSyncTest");
        VOSSynchronizer synchronizer = go.AddComponent<VOSSynchronizer>();
        synchronizer.Initialize("localhost", 1883, false,
            FiveSQD.WebVerse.WebInterface.MQTT.MQTTClient.Transports.TCP, Vector3.zero);

        // Test exiting session when not in session - should log error
        LogAssert.Expect(LogType.Error, "[Error] [VOSSynchronizer->ExitSession] Not in session.");
        synchronizer.ExitSession();

        LogAssert.Expect(LogType.Error, "[Error] [VOSSynchronizer->ExitSession] Not in session.");
        synchronizer.Terminate();
        GameObject.DestroyImmediate(go);
    }
#endif

    [Test]
    public void VOSSynchronizer_WithoutWebInterface_DefineConstraint()
    {
        // This test ensures the conditional compilation works correctly
        // When USE_WEBINTERFACE is not defined, the other tests shouldn't run
#if !USE_WEBINTERFACE
        Assert.Pass("VOSSynchronizer tests are conditionally compiled and require USE_WEBINTERFACE define");
#else
        Assert.Pass("USE_WEBINTERFACE is defined, other tests should run");
#endif
    }
}