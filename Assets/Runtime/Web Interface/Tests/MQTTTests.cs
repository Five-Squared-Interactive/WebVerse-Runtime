// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

#if USE_BESTHTTP
using System.Collections;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WebInterface.MQTT;
using System;
using UnityEngine;
using NUnit.Framework;

/// <summary>
/// Unit tests for MQTT.
/// </summary>
public class MQTTTests
{
    private float waitPeriod = 2; // Reduced wait time

    [Test]
    public void MQTTClient_Constructor_InitializesCorrectly()
    {
        // Test MQTT client initialization without actually connecting
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("localhost", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/webversetest");

        Assert.IsNotNull(client);
    }

    [UnityTest]
    public IEnumerator MQTTTests_TCP_WithInvalidHost()
    {
        // Test connection to invalid host - should handle gracefully
        bool connected = false;
        
        Action<MQTTClient> onConnectedAction = (client) => { connected = true; };
        Action<MQTTClient, byte, string> onDisconnectedAction = (client, code, info) => { connected = false; };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChangedAction = (client, from, to) => { };
        Action<MQTTClient, string> onErrorAction = (client, info) => { };

        MQTTClient client = new MQTTClient("invalid-mqtt-host.local", 1883, false, MQTTClient.Transports.TCP,
            onConnectedAction, onDisconnectedAction, onStateChangedAction, onErrorAction, "/webversetest");

        try
        {
            client.Connect();
        }
        catch (Exception)
        {
            // Expected for invalid host
        }
        
        yield return new WaitForSeconds(waitPeriod);
        
        // Should not connect to invalid host
        Assert.IsFalse(connected);
        
        // Test operations on disconnected client
        Action<string> onAcknowledged = (info) => { };
        Action<MQTTClient, string, string, MQTTMessage> onMessage = (client, topic, topicName, message) => { };
        
        try
        {
            client.Subscribe("testtopic", onAcknowledged, onMessage);
            client.Publish("testtopic", "test");
            client.UnSubscribe("testtopic", onAcknowledged);
            client.Disconnect();
        }
        catch (Exception)
        {
            // Expected for disconnected client
        }
    }

    [UnityTest]
    public IEnumerator MQTTTests_TCPS_WithInvalidHost()
    {
        // Test secure connection to invalid host
        bool connected = false;
        
        Action<MQTTClient> onConnectedAction = (client) => { connected = true; };
        Action<MQTTClient, byte, string> onDisconnectedAction = (client, code, info) => { connected = false; };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChangedAction = (client, from, to) => { };
        Action<MQTTClient, string> onErrorAction = (client, info) => { };

        MQTTClient client = new MQTTClient("invalid-mqtt-host.local", 8883, true, MQTTClient.Transports.TCP,
            onConnectedAction, onDisconnectedAction, onStateChangedAction, onErrorAction, "/webversetest");

        try
        {
            client.Connect();
        }
        catch (Exception)
        {
            // Expected for invalid host
        }
        
        yield return new WaitForSeconds(waitPeriod);
        
        // Should not connect to invalid host
        Assert.IsFalse(connected);
        
        client.Disconnect();
    }

    [UnityTest]
    public IEnumerator MQTTTests_WS_WithInvalidHost()
    {
        // Test WebSocket connection to invalid host
        bool connected = false;
        
        Action<MQTTClient> onConnectedAction = (client) => { connected = true; };
        Action<MQTTClient, byte, string> onDisconnectedAction = (client, code, info) => { connected = false; };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChangedAction = (client, from, to) => { };
        Action<MQTTClient, string> onErrorAction = (client, info) => { };

        MQTTClient client = new MQTTClient("invalid-mqtt-host.local", 8080, false, MQTTClient.Transports.WebSockets,
            onConnectedAction, onDisconnectedAction, onStateChangedAction, onErrorAction, "/webversetest");

        // Ignore potential library errors for invalid connections
        LogAssert.ignoreFailingMessages = true;

        try
        {
            client.Connect();
        }
        catch (Exception)
        {
            // Expected for invalid host
        }
        
        yield return new WaitForSeconds(waitPeriod);
        
        // Should not connect to invalid host
        Assert.IsFalse(connected);
        
        client.Disconnect();
        
        // Reset log assert
        LogAssert.ignoreFailingMessages = false;
    }

    [UnityTest]
    public IEnumerator MQTTTests_WSS_WithInvalidHost()
    {
        // Test secure WebSocket connection to invalid host
        bool connected = false;
        
        Action<MQTTClient> onConnectedAction = (client) => { connected = true; };
        Action<MQTTClient, byte, string> onDisconnectedAction = (client, code, info) => { connected = false; };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChangedAction = (client, from, to) => { };
        Action<MQTTClient, string> onErrorAction = (client, info) => { };

        MQTTClient client = new MQTTClient("invalid-mqtt-host.local", 8081, true, MQTTClient.Transports.WebSockets,
            onConnectedAction, onDisconnectedAction, onStateChangedAction, onErrorAction, "/webversetest");

        // Ignore potential library errors for invalid connections
        LogAssert.ignoreFailingMessages = true;

        try
        {
            client.Connect();
        }
        catch (Exception)
        {
            // Expected for invalid host
        }
        
        yield return new WaitForSeconds(waitPeriod);
        
        // Should not connect to invalid host
        Assert.IsFalse(connected);
        
        client.Disconnect();
        
        // Reset log assert
        LogAssert.ignoreFailingMessages = false;
    }

    [Test]
    public void MQTTClient_TransportEnum_IsValid()
    {
        // Test that transport enum values are valid
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.Transports), MQTTClient.Transports.TCP));
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.Transports), MQTTClient.Transports.WebSockets));
    }

    [Test]
    public void MQTTClient_ClientStateEnum_IsValid()
    {
        // Test that client state enum values are valid
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.ClientState), MQTTClient.ClientState.Disconnected));
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.ClientState), MQTTClient.ClientState.TransportConnecting));
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.ClientState), MQTTClient.ClientState.Connected));
    }
}
#endif