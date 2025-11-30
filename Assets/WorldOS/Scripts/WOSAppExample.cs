// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Text;
using UnityEngine;

/// <summary>
/// Example script demonstrating how to use the WOSApp module.
/// </summary>
public class WOSAppExample : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField]
    private string appName = "ExampleApp";
    
    [SerializeField]
    private int mqttPort = 1883;
    
    [SerializeField]
    private string publishTopic = "wos/example/output";
    
    [SerializeField]
    private string subscribeTopic = "wos/example/input";

    private WOSApp wosApp;
    private float messageTimer = 0f;
    private int messageCount = 0;

    void Start()
    {
        // Get or add the WOSApp component
        wosApp = gameObject.GetComponent<WOSApp>();
        if (wosApp == null)
        {
            wosApp = gameObject.AddComponent<WOSApp>();
        }

        // Connect to WorldOS
        wosApp.ConnectToWOS(appName, mqttPort, OnConnected);
    }

    /// <summary>
    /// Called when successfully connected to the MQTT bus.
    /// </summary>
    private void OnConnected()
    {
        wosApp.Log($"[{appName}] Connection established! Setting up subscriptions...");

        // Subscribe to a topic
        wosApp.SubscribeToWOS(appName, subscribeTopic, OnMessageReceived);

        // Publish a welcome message
        wosApp.PublishOnWOS(appName, publishTopic, $"{appName} has connected!");
    }

    /// <summary>
    /// Called when a message is received on a subscribed topic.
    /// </summary>
    /// <param name="topic">The topic the message was received on</param>
    /// <param name="payload">The message payload as bytes</param>
    private void OnMessageReceived(string topic, byte[] payload)
    {
        // Convert payload to string (assuming UTF-8 encoding)
        string message = payload != null ? Encoding.UTF8.GetString(payload) : "null";
        
        wosApp.Log($"[{appName}] Received message on topic '{topic}': {message}");

        // Example: Echo the message back on a different topic
        wosApp.PublishOnWOS(appName, publishTopic, $"Echo: {message}");
    }

    void Update()
    {
        // Example: Send a periodic message every 5 seconds
        messageTimer += Time.deltaTime;
        if (messageTimer >= 5f)
        {
            messageTimer = 0f;
            messageCount++;
            
            string periodicMessage = $"Periodic message #{messageCount} from {appName}";
            wosApp.PublishOnWOS(appName, publishTopic, periodicMessage);
        }
    }

    // Example: Public method that can be called from UI or other scripts
    public void SendCustomMessage(string message)
    {
        if (wosApp != null)
        {
            wosApp.PublishOnWOS(appName, publishTopic, message);
        }
    }

    // Example: Subscribe to an additional topic at runtime
    public void SubscribeToAdditionalTopic(string topic)
    {
        if (wosApp != null)
        {
            wosApp.SubscribeToWOS(appName, topic, (receivedTopic, payload) =>
            {
                string message = payload != null ? Encoding.UTF8.GetString(payload) : "null";
                wosApp.Log($"[{appName}] Message on additional topic '{receivedTopic}': {message}");
            });
        }
    }
}
