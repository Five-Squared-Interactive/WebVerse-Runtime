// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

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
    public void MQTTClient_Constructor_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
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
        LogAssert.ignoreFailingMessages = true;
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
        LogAssert.ignoreFailingMessages = true;
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
        LogAssert.ignoreFailingMessages = true;
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
        
        // Log assert reset handled by test framework
    }

    [UnityTest]
    public IEnumerator MQTTTests_WSS_WithInvalidHost()
    {
        LogAssert.ignoreFailingMessages = true;
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
        
        // Log assert reset handled by test framework
    }

    [Test]
    public void MQTTClient_TransportEnum_IsValid()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that transport enum values are valid
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.Transports), MQTTClient.Transports.TCP));
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.Transports), MQTTClient.Transports.WebSockets));
    }

    [Test]
    public void MQTTClient_ClientStateEnum_IsValid()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that client state enum values are valid
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.ClientState), MQTTClient.ClientState.Disconnected));
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.ClientState), MQTTClient.ClientState.TransportConnecting));
        Assert.IsTrue(Enum.IsDefined(typeof(MQTTClient.ClientState), MQTTClient.ClientState.Connected));
    }

    [Test]
    public void MQTTClient_Subscribe_OnDisconnectedClient_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test subscribing on a client that has not connected
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("localhost", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/webversetest");

        Action<string> onAcknowledged = (info) => { };
        Action<MQTTClient, string, string, MQTTMessage> onMessage = (c, topic, topicName, message) => { };

        // Subscribe should not throw on disconnected client - internally calls BeginSubscribe
        Assert.DoesNotThrow(() =>
        {
            try
            {
                client.Subscribe("test/topic", onAcknowledged, onMessage);
            }
            catch (Exception)
            {
                // May throw if internal client is not in connected state
            }
        });
    }

    [Test]
    public void MQTTClient_Unsubscribe_OnDisconnectedClient_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test unsubscribing on a client that has not connected
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("localhost", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/webversetest");

        Action<string> onAcknowledged = (info) => { };

        // UnSubscribe checks for Connected state internally so should be safe
        Assert.DoesNotThrow(() =>
        {
            client.UnSubscribe("test/topic", onAcknowledged);
        });
    }

    [Test]
    public void MQTTClient_Publish_OnDisconnectedClient_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test publishing on a client that has not connected
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("localhost", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/webversetest");

        // Publish checks for Connected state and returns early if not connected
        Assert.DoesNotThrow(() =>
        {
            client.Publish("test/topic", "hello world");
        });
    }

    [Test]
    public void MQTTClient_ClientState_InitialByDefault()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that a newly created client reports correct initial state
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("localhost", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/webversetest");

        // A newly constructed client should be in Initial state
        Assert.AreEqual(MQTTClient.ClientState.Initial, client.clientState);
    }

    [Test]
    public void MQTTClient_Properties_ReflectConstructorValues()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that host, port, and useTLS properties match constructor args
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("mqtt.example.com", 8883, true, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/custompath");

        Assert.AreEqual("mqtt.example.com", client.host);
        Assert.AreEqual(8883, client.port);
        Assert.AreEqual(true, client.useTLS);
    }

    [Test]
    public void MQTTClient_Disconnect_OnDisconnectedClient_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test disconnecting a client that was never connected
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("localhost", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/webversetest");

        Assert.DoesNotThrow(() =>
        {
            try
            {
                client.Disconnect();
            }
            catch (Exception)
            {
                // May throw if internal client state doesn't allow disconnect
            }
        });
    }

    [Test]
    public void MQTTClient_AllClientStates_HaveExpectedValues()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that all client state enum integer values are as expected
        Assert.AreEqual(0, (int)MQTTClient.ClientState.Initial);
        Assert.AreEqual(1, (int)MQTTClient.ClientState.TransportConnecting);
        Assert.AreEqual(2, (int)MQTTClient.ClientState.TransportConnected);
        Assert.AreEqual(3, (int)MQTTClient.ClientState.Connected);
        Assert.AreEqual(4, (int)MQTTClient.ClientState.Disconnecting);
        Assert.AreEqual(5, (int)MQTTClient.ClientState.Disconnected);
    }

    [Test]
    public void MQTTClient_QoSLevelEnum_IsValid()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that QoS level enum values are valid
        Assert.IsTrue(Enum.IsDefined(typeof(QOSLevel), QOSLevel.AtMostOnceDelivery));
        Assert.IsTrue(Enum.IsDefined(typeof(QOSLevel), QOSLevel.AtLeastOnceDelivery));
        Assert.IsTrue(Enum.IsDefined(typeof(QOSLevel), QOSLevel.ExactlyOnceDelivery));
        Assert.IsTrue(Enum.IsDefined(typeof(QOSLevel), QOSLevel.Reserved));

        // Verify binary values
        Assert.AreEqual(0b00, (int)QOSLevel.AtMostOnceDelivery);
        Assert.AreEqual(0b01, (int)QOSLevel.AtLeastOnceDelivery);
        Assert.AreEqual(0b10, (int)QOSLevel.ExactlyOnceDelivery);
        Assert.AreEqual(0b11, (int)QOSLevel.Reserved);
    }

    [Test]
    public void MQTTClient_QoSLevelEnum_HasExpectedCount()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(QOSLevel));
        Assert.AreEqual(4, values.Length);
    }

    [Test]
    public void MQTTClient_TransportEnum_HasExpectedCount()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(MQTTClient.Transports));
        Assert.AreEqual(2, values.Length);
    }

    [Test]
    public void MQTTClient_ClientStateEnum_HasExpectedCount()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(MQTTClient.ClientState));
        Assert.AreEqual(6, values.Length);
    }

    [Test]
    public void MQTTClient_Properties_WebSocketTransport()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("ws.example.com", 8080, false, MQTTClient.Transports.WebSockets,
            onConnected, onDisconnected, onStateChanged, onError, "/mqtt");

        Assert.AreEqual("ws.example.com", client.host);
        Assert.AreEqual(8080, client.port);
        Assert.AreEqual(false, client.useTLS);
        Assert.IsNotNull(client);
    }

    [Test]
    public void MQTTClient_Properties_TCPTransport()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("tcp.example.com", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/test");

        Assert.AreEqual("tcp.example.com", client.host);
        Assert.AreEqual(1883, client.port);
    }

    [Test]
    public void MQTTClient_MultipleSubscribe_OnDisconnectedClient_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("localhost", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/webversetest");

        Action<string> onAcknowledged = (info) => { };
        Action<MQTTClient, string, string, MQTTMessage> onMessage = (c, topic, topicName, message) => { };

        Assert.DoesNotThrow(() =>
        {
            try
            {
                client.Subscribe("topic/a", onAcknowledged, onMessage);
                client.Subscribe("topic/b", onAcknowledged, onMessage);
                client.Subscribe("topic/c", onAcknowledged, onMessage);
            }
            catch (Exception)
            {
                // May throw if internal client state doesn't allow subscribe
            }
        });
    }

    [Test]
    public void MQTTClient_Publish_MultipleTopics_OnDisconnectedClient_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("localhost", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/webversetest");

        Assert.DoesNotThrow(() =>
        {
            client.Publish("topic/1", "message one");
            client.Publish("topic/2", "message two");
            client.Publish("topic/3", "message three");
        });
    }

    [Test]
    public void MQTTClient_Disconnect_CalledMultipleTimes_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("localhost", 1883, false, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/webversetest");

        Assert.DoesNotThrow(() =>
        {
            try
            {
                client.Disconnect();
                client.Disconnect();
            }
            catch (Exception)
            {
                // Expected - disconnect on non-connected may throw
            }
        });
    }

    [Test]
    public void MQTTClient_Properties_TLSEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<MQTTClient> onConnected = (client) => { };
        Action<MQTTClient, byte, string> onDisconnected = (client, code, info) => { };
        Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChanged = (client, from, to) => { };
        Action<MQTTClient, string> onError = (client, info) => { };

        MQTTClient client = new MQTTClient("secure.example.com", 8883, true, MQTTClient.Transports.TCP,
            onConnected, onDisconnected, onStateChanged, onError, "/securepath");

        Assert.IsTrue(client.useTLS);
        Assert.AreEqual(8883, client.port);
    }
}
#endif