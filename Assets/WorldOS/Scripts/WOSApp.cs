// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.IO;
using System.Text;
using Best.MQTT;
using Best.MQTT.Packets;
using Best.MQTT.Packets.Builders;
using UnityEngine;

/// <summary>
/// WorldOS (WOS) App module for connecting to and communicating with the MQTT bus.
/// </summary>
public class WOSApp : MonoBehaviour
{
    private MQTTClient client;
    private StreamWriter logWriter;
    private string logFilePath;

    void Awake()
    {
        // Initialize log file
        logFilePath = Path.Combine(Application.persistentDataPath, "wos.log");
        InitializeLogger();
    }

    private void InitializeLogger()
    {
        try
        {
            logWriter = new StreamWriter(logFilePath, true, Encoding.UTF8);
            logWriter.AutoFlush = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize log file: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a message to both Unity console and log file.
    /// </summary>
    /// <param name="text">The text to log</param>
    public void Log(string text)
    {
        Debug.Log(text);
        
        if (logWriter != null)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                logWriter.WriteLine($"{timestamp} {text}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write to log file: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Connects to the WorldOS MQTT bus.
    /// </summary>
    /// <param name="appName">Name of the application</param>
    /// <param name="wosPort">MQTT broker port</param>
    /// <param name="onConnect">Callback invoked when connection is established</param>
    public void ConnectToWOS(string appName, int wosPort, Action onConnect)
    {
        Log($"[{appName}] Connecting to MQTT bus...");

        var options = new ConnectionOptions
        {
            Host = "localhost",
            Port = wosPort,
            Transport = SupportedTransports.TCP
        };

        client = new MQTTClient(options);

        client.OnConnected += (MQTTClient c) =>
        {
            Log($"[{appName}] Connected to MQTT bus.");
            onConnect?.Invoke();
        };

        client.OnError += (MQTTClient c, string error) =>
        {
            Log($"[{appName}] MQTT Error: {error}");
        };

        client.OnDisconnect += (MQTTClient c, DisconnectReasonCodes code, string reason) =>
        {
            Log($"[{appName}] Disconnected from MQTT bus. Code: {code}, Reason: {reason}");
        };

        Debug.Log($"Connecting to MQTT on port {wosPort}...");
        client.BeginConnect(ConnectPacketBuilderCallback);
    }

    private ConnectPacketBuilder ConnectPacketBuilderCallback(MQTTClient client, ConnectPacketBuilder builder)
    {
        // Configure the connection packet with default settings
        builder.WithKeepAlive(60);
        return builder;
    }

    /// <summary>
    /// Subscribes to a topic on the WorldOS MQTT bus.
    /// </summary>
    /// <param name="appName">Name of the application</param>
    /// <param name="subscriptionTopic">The topic to subscribe to</param>
    /// <param name="onMessage">Callback invoked when a message is received (topic, payload)</param>
    public void SubscribeToWOS(string appName, string subscriptionTopic, Action<string, byte[]> onMessage)
    {
        if (client == null)
        {
            Log($"[{appName}] WOS not connected.");
            return;
        }

        client.CreateBulkSubscriptionBuilder()
            .WithTopic(new SubscribeTopicBuilder(subscriptionTopic)
                .WithMaximumQoS(QoSLevels.ExactlyOnceDelivery)
                .WithAcknowledgementCallback((MQTTClient c, SubscriptionTopic topic, SubscribeAckReasonCodes reasonCode) =>
                {
                    Log($"[{appName}] Subscribed to {subscriptionTopic}.");
                })
                .WithMessageCallback((MQTTClient c, SubscriptionTopic topic, string topicName, ApplicationMessage message) =>
                {
                    byte[] payload = null;
                    if (message.Payload != null && message.Payload.Count > 0)
                    {
                        payload = new byte[message.Payload.Count];
                        Array.Copy(message.Payload.Data, message.Payload.Offset, payload, 0, message.Payload.Count);
                    }
                    onMessage?.Invoke(topicName, payload);
                }))
            .BeginSubscribe();
    }

    /// <summary>
    /// Publishes a message to a topic on the WorldOS MQTT bus.
    /// </summary>
    /// <param name="appName">Name of the application</param>
    /// <param name="topic">The topic to publish to</param>
    /// <param name="message">The message to publish</param>
    public void PublishOnWOS(string appName, string topic, string message)
    {
        if (client == null)
        {
            Log($"[{appName}] WOS not connected.");
            return;
        }

        client.CreateApplicationMessageBuilder(topic)
            .WithPayload(message)
            .WithQoS(QoSLevels.ExactlyOnceDelivery)
            .BeginPublish();
    }

    /// <summary>
    /// Publishes a message to a topic on the WorldOS MQTT bus.
    /// </summary>
    /// <param name="appName">Name of the application</param>
    /// <param name="topic">The topic to publish to</param>
    /// <param name="message">The message bytes to publish</param>
    public void PublishOnWOS(string appName, string topic, byte[] message)
    {
        if (client == null)
        {
            Log($"[{appName}] WOS not connected.");
            return;
        }

        client.CreateApplicationMessageBuilder(topic)
            .WithPayload(message)
            .WithQoS(QoSLevels.ExactlyOnceDelivery)
            .BeginPublish();
    }

    void OnDestroy()
    {
        // Disconnect from MQTT
        if (client != null)
        {
            client.CreateDisconnectPacketBuilder().BeginDisconnect();
            client = null;
        }

        // Close log file
        if (logWriter != null)
        {
            logWriter.Close();
            logWriter.Dispose();
            logWriter = null;
        }
    }
}
