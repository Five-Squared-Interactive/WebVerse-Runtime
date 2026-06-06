// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Networking
{
    /// <summary>
    /// MQTT Quality of Service Levels.
    /// </summary>
    public enum QOSLevel
    {
        AtMostOnceDelivery = 0b00, AtLeastOnceDelivery = 0b01,
        ExactlyOnceDelivery = 0b10, Reserved = 0b11
    }

    /// <summary>
    /// MQTT Payload Types.
    /// </summary>
    public enum PayloadTypes
    {
        Bytes = 0b00,
        UTF8 = 0b01
    }

    /// <summary>
    /// Class for an MQTT Buffer Segment.
    /// </summary>
    public class BufferSegment
    {
        /// <summary>
        /// Count.
        /// </summary>
        public readonly int count;

        /// <summary>
        /// Data.
        /// </summary>
        public readonly byte[] data;

        /// <summary>
        /// Offset.
        /// </summary>
        public readonly int offset;

        /// <summary>
        /// Constructor for an MQTT Buffer Segment.
        /// </summary>
        /// <param name="count">Segment count.</param>
        /// <param name="data">Segment data.</param>
        /// <param name="offset">Segment offset.</param>
        public BufferSegment(int count, byte[] data, int offset)
        {
            this.count = count;
            this.data = data;
            this.offset = offset;
        }
    }

    /// <summary>
    /// Class for an MQTT Message.
    /// </summary>
    public class MQTTMessage
    {
        /// <summary>
        /// Content Type.
        /// </summary>
        public readonly string contentType;

        /// <summary>
        /// Correlation Data.
        /// </summary>
        public readonly BufferSegment correlationData;

        /// <summary>
        /// Expiry Interval.
        /// </summary>
        public readonly TimeSpan expiryInterval;

        /// <summary>
        /// Is Duplicate Flag.
        /// </summary>
        public readonly bool isDuplicate;

        /// <summary>
        /// Payload.
        /// </summary>
        public readonly BufferSegment payload;

        /// <summary>
        /// Payload Format.
        /// </summary>
        public readonly PayloadTypes payloadFormat;

        /// <summary>
        /// QoS Level.
        /// </summary>
        public readonly QOSLevel qosLevel;

        /// <summary>
        /// Response Topic.
        /// </summary>
        public readonly string responseTopic;

        /// <summary>
        /// Retain Flag.
        /// </summary>
        public readonly bool retain;

        /// <summary>
        /// Topic.
        /// </summary>
        public readonly string topic;

        /// <summary>
        /// User Properties.
        /// </summary>
        public readonly List<KeyValuePair<string, string>> userProperties;

        /// <summary>
        /// Constructor for an MQTT Message.
        /// </summary>
        /// <param name="contentType">Content type.</param>
        /// <param name="correlationData">Correlation data.</param>
        /// <param name="expiryInterval">Expiry interval.</param>
        /// <param name="isDuplicate">Is duplicate flag.</param>
        /// <param name="payload">Payload</param>
        /// <param name="payloadFormat">Payload format.</param>
        /// <param name="qosLevel">QoS level.</param>
        /// <param name="responseTopic">Response topic.</param>
        /// <param name="retain">Retain flag.</param>
        /// <param name="userProperties">User properties.</param>
        public MQTTMessage(string contentType, BufferSegment correlationData, TimeSpan expiryInterval,
            bool isDuplicate, BufferSegment payload, PayloadTypes payloadFormat, QOSLevel qosLevel,
            string responseTopic, bool retain, string topic, List<KeyValuePair<string, string>> userProperties)
        {
            this.contentType = contentType;
            this.correlationData = correlationData;
            this.expiryInterval = expiryInterval;
            this.isDuplicate = isDuplicate;
            this.payload = payload;
            this.payloadFormat = payloadFormat;
            this.qosLevel = qosLevel;
            this.responseTopic = responseTopic;
            this.retain = retain;
            this.topic = topic;
            this.userProperties = userProperties;
        }
    }

#if USE_WEBINTERFACE
    /// <summary>
    /// Class for an MQTT Client.
    /// </summary>
    public class MQTTClient : IEventEmitter
    {
        /// <summary>
        /// Reference to internal MQTT client.
        /// </summary>
        private WebInterface.MQTT.MQTTClient internalClient;

        /// <summary>
        /// Constructor for an MQTT Client.
        /// </summary>
        /// <param name="host">Host address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="useTLS">Whether or not to use TLS.</param>
        /// <param name="transport">The transport to use (either "tcp" or "websockets").</param>
        /// <param name="onConnected">Logic to invoke upon connection.</param>
        /// <param name="onDisconnected">Logic to invoke upon disconnection.</param>
        /// <param name="onStateChanged">Logic to invoke upon connection state change.</param>
        /// <param name="onError">Logic to invoke upon connection error.</param>
        /// <param name="path">Server path.</param>
        public MQTTClient(string host, int port, bool useTLS, string transport,
            string onConnected, string onDisconnected, string onStateChanged, string onError,
            string path = "/mqtt")
        {
            WebInterface.MQTT.MQTTClient.Transports supportedTransports = WebInterface.MQTT.MQTTClient.Transports.TCP;
            switch (transport.ToLower())
            {
                case "tcp":
                    supportedTransports = WebInterface.MQTT.MQTTClient.Transports.TCP;
                    break;

                case "websockets":
                    supportedTransports = WebInterface.MQTT.MQTTClient.Transports.WebSockets;
                    break;

                default:
                    Logging.LogWarning("[MQTTClient] Invalid transport " + transport);
                    return;
            }

            // Aligned with HTTPNetworking's callback pattern (HTTPNetworking.cs:343):
            // user passes a bare function name; we invoke it via
            // timeHandler.CallAsynchronously with boxed args. Replaces the prior
            // `Run(expr.Replace("?", "varNames"))` design which only worked when
            // the substituted identifiers happened to be JS globals — they
            // weren't, so callbacks like onMessage couldn't deliver data.
            Action<WebInterface.MQTT.MQTTClient> onConnectedAction = new Action<WebInterface.MQTT.MQTTClient>((client) =>
            {
                if (!string.IsNullOrEmpty(onConnected))
                {
                    WebVerseRuntime.Instance.timeHandler.CallAsynchronously(onConnected, new object[] { });
                }
            });

            Action<WebInterface.MQTT.MQTTClient, byte, string> onDisconnectedAction
                = new Action<WebInterface.MQTT.MQTTClient, byte, string>((client, code, msg) =>
            {
                if (!string.IsNullOrEmpty(onDisconnected))
                {
                    WebVerseRuntime.Instance.timeHandler.CallAsynchronously(
                        onDisconnected, new object[] { code, msg ?? "" });
                }
            });

            Action<WebInterface.MQTT.MQTTClient, WebInterface.MQTT.MQTTClient.ClientState,
                WebInterface.MQTT.MQTTClient.ClientState> onStateChangedAction
                = new Action<WebInterface.MQTT.MQTTClient, WebInterface.MQTT.MQTTClient.ClientState,
                WebInterface.MQTT.MQTTClient.ClientState>((client, from, to) =>
                {
                    if (!string.IsNullOrEmpty(onStateChanged))
                    {
                        WebVerseRuntime.Instance.timeHandler.CallAsynchronously(
                            onStateChanged, new object[] { from.ToString(), to.ToString() });
                    }
                });

            Action<WebInterface.MQTT.MQTTClient, string> onErrorAction
                = new Action<WebInterface.MQTT.MQTTClient, string>((client, msg) =>
                {
                    if (!string.IsNullOrEmpty(onError))
                    {
                        WebVerseRuntime.Instance.timeHandler.CallAsynchronously(
                            onError, new object[] { msg ?? "" });
                    }
                });

            internalClient = new WebInterface.MQTT.MQTTClient(host, port, useTLS, supportedTransports,
                onConnectedAction, onDisconnectedAction, onStateChangedAction, onErrorAction, path);
        }

        /// <summary>
        /// Connect the MQTT Client.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Connect()
        {
            if (internalClient == null)
            {
                Logging.LogWarning("[MQTTClient:Connect] Not initialized.");
                return false;
            }

            internalClient.Connect();
            return true;
        }

        /// <summary>
        /// Disconnect the MQTT Client.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Disconnect()
        {
            if (internalClient == null)
            {
                Logging.LogWarning("[MQTTClient:Disconnect] Not initialized.");
                return false;
            }

            internalClient.Disconnect();
            return true;
        }

        /// <summary>
        /// Subscribe to a topic.
        /// </summary>
        /// <param name="topic">Topic to subscribe to.</param>
        /// <param name="onAcknowledged">Logic to execute when subscription request has been acknowledged.</param>
        /// <param name="onMessage">Logic to execute when a message has been received.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Subscribe(string topic, string onAcknowledged, string onMessage)
        {
            if (internalClient == null)
            {
                Logging.LogWarning("[MQTTClient:Subscribe] Not initialized.");
                return false;
            }

            Action<string> onAcknowledgedAction = new Action<string>((msg) =>
            {
                if (!string.IsNullOrEmpty(onAcknowledged))
                {
                    WebVerseRuntime.Instance.timeHandler.CallAsynchronously(
                        onAcknowledged, new object[] { msg ?? "" });
                }
            });

            Action<WebInterface.MQTT.MQTTClient, string, string, WebInterface.MQTT.MQTTMessage> onMessageAction
                = new Action<WebInterface.MQTT.MQTTClient, string, string, WebInterface.MQTT.MQTTMessage>((client, topic, topicName, msg) =>
            {
                // FIXED: was checking onAcknowledged here; should be onMessage.
                if (!string.IsNullOrEmpty(onMessage))
                {
                    // Decode payload from BufferSegment to UTF-8 string, mirroring
                    // HTTPNetworking's pattern. Binary-only payloads would need a
                    // separate API; chunk responses are JSON so UTF-8 is right.
                    string payload = "";
                    if (msg != null && msg.payload != null && msg.payload.data != null)
                    {
                        payload = System.Text.Encoding.UTF8.GetString(
                            msg.payload.data, msg.payload.offset, msg.payload.count);
                    }
                    WebVerseRuntime.Instance.timeHandler.CallAsynchronously(
                        onMessage, new object[] { topic ?? "", topicName ?? "", payload });
                }
            });

            internalClient.Subscribe(topic, onAcknowledgedAction, onMessageAction);
            return true;
        }

        /// <summary>
        /// UnSubscribe from a topic.
        /// </summary>
        /// <param name="topic">Topic to subscribe to.</param>
        /// <param name="onAcknowledged">Logic to execute when unsubscribe request has been acknowledged.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool UnSubscribe(string topic, string onAcknowledged)
        {
            if (internalClient == null)
            {
                Logging.LogWarning("[MQTTClient:UnSubscribe] Not initialized.");
                return false;
            }

            Action<string> onAcknowledgedAction = new Action<string>((msg) =>
            {
                if (!string.IsNullOrEmpty(onAcknowledged))
                {
                    WebVerseRuntime.Instance.timeHandler.CallAsynchronously(
                        onAcknowledged, new object[] { msg ?? "" });
                }
            });

            internalClient.UnSubscribe(topic, onAcknowledgedAction);
            return true;
        }

        /// <summary>
        /// Publish a message.
        /// </summary>
        /// <param name="topic">Topic to subscribe send on</param>
        /// <param name="message">Message to send.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Publish(string topic, string message)
        {
            if (internalClient == null)
            {
                Logging.LogWarning("[MQTTClient:Publish] Not initialized.");
                return false;
            }

            internalClient.Publish(topic, message);
            return true;
        }

        #region IEventEmitter Implementation

        private Dictionary<string, List<Jint.Native.JsValue>> _listeners;
        public Dictionary<string, List<Jint.Native.JsValue>> Listeners
            => _listeners ??= new Dictionary<string, List<Jint.Native.JsValue>>();

        private HashSet<Jint.Native.JsValue> _onceListeners;
        public HashSet<Jint.Native.JsValue> OnceListeners
            => _onceListeners ??= new HashSet<Jint.Native.JsValue>();

        private HashSet<string> _emittingEvents;
        public HashSet<string> EmittingEvents
            => _emittingEvents ??= new HashSet<string>();

        private bool _isDisposed = false;
        public bool IsDisposed => _isDisposed;

        public void DisposeEvents()
        {
            if (_isDisposed) return;
            ((IEventEmitter)this).DisposeAllListeners();
            _isDisposed = true;
        }

        #endregion
    }
#endif
}