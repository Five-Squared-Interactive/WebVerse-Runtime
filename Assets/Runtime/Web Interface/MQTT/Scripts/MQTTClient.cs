// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

#if USE_BESTHTTP
using Best.MQTT;
using Best.MQTT.Packets.Builders;
using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.WebInterface.MQTT
{
    /// <summary>
    /// Enumeration for QoS levels.
    /// </summary>
    public enum QOSLevel
    {
        AtMostOnceDelivery = 0b00, AtLeastOnceDelivery = 0b01,
        ExactlyOnceDelivery = 0b10, Reserved = 0b11
    }

    /// <summary>
    /// Enumeration for payload types.
    /// </summary>
    public enum PayloadTypes
    {
        Bytes = 0b00,
        UTF8 = 0b01
    }

    /// <summary>
    /// Class for a buffer segment.
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
        /// Constructor for a buffer segment.
        /// </summary>
        /// <param name="count">Count.</param>
        /// <param name="data">Data.</param>
        /// <param name="offset">Offset.</param>
        public BufferSegment(int count, byte[] data, int offset)
        {
            this.count = count;
            this.data = data;
            this.offset = offset;
        }

        /// <summary>
        /// Convert from MQTT buffer segment.
        /// </summary>
        /// <param name="bufferSegment">MQTT buffer segment.</param>
        public static BufferSegment FromBestMQTT(Best.HTTP.Shared.PlatformSupport.Memory.BufferSegment bufferSegment)
        {
            return new BufferSegment(bufferSegment.Count, bufferSegment.Data, bufferSegment.Offset);
        }

        /// <summary>
        /// Convert to MQTT buffer segment.
        /// </summary>
        /// <param name="bufferSegment">Buffer segment.</param>
        public static Best.HTTP.Shared.PlatformSupport.Memory.BufferSegment ToBestMQTT(BufferSegment bufferSegment)
        {
            return new Best.HTTP.Shared.PlatformSupport.Memory.BufferSegment(
                bufferSegment.data, bufferSegment.offset, bufferSegment.count);
        }
    }

    /// <summary>
    /// Class for an MQTT message.
    /// </summary>
    public class MQTTMessage
    {
        /// <summary>
        /// Content type.
        /// </summary>
        public readonly string contentType;

        /// <summary>
        /// Correlation data.
        /// </summary>
        public readonly BufferSegment correlationData;

        /// <summary>
        /// Expiry interval.
        /// </summary>
        public readonly TimeSpan expiryInterval;

        /// <summary>
        /// Is duplicate.
        /// </summary>
        public readonly bool isDuplicate;

        /// <summary>
        /// Payload.
        /// </summary>
        public readonly BufferSegment payload;

        /// <summary>
        /// Payload format.
        /// </summary>
        public readonly PayloadTypes payloadFormat;

        /// <summary>
        /// QoS level.
        /// </summary>
        public readonly QOSLevel qosLevel;

        /// <summary>
        /// Response topic.
        /// </summary>
        public readonly string responseTopic;

        /// <summary>
        /// Retain.
        /// </summary>
        public readonly bool retain;

        /// <summary>
        /// Topic.
        /// </summary>
        public readonly string topic;

        /// <summary>
        /// User properties.
        /// </summary>
        public readonly List<KeyValuePair<string, string>> userProperties;

        /// <summary>
        /// Constructor for an MQTT message.
        /// </summary>
        /// <param name="contentType">Content type.</param>
        /// <param name="correlationData">Correlation data.</param>
        /// <param name="expiryInterval">Expiry interval.</param>
        /// <param name="isDuplicate">Is duplicate.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="payloadFormat">Payload format.</param>
        /// <param name="qosLevel">QoS level.</param>
        /// <param name="responseTopic">Response topic.</param>
        /// <param name="retain">Retain.</param>
        /// <param name="topic">Topic.</param>
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

        /// <summary>
        /// Convert from MQTT message.
        /// </summary>
        /// <param name="applicationMessage">MQTT message.</param>
        public static MQTTMessage FromBestMQTT(ApplicationMessage applicationMessage)
        {
            return new MQTTMessage(applicationMessage.ContentType,
                BufferSegment.FromBestMQTT(applicationMessage.CorrelationData),
                applicationMessage.ExpiryInterval, applicationMessage.IsDuplicate,
                BufferSegment.FromBestMQTT(applicationMessage.Payload),
                (PayloadTypes) applicationMessage.PayloadFormat,
                (QOSLevel) applicationMessage.QoS,
                applicationMessage.ResponseTopic, applicationMessage.Retain,
                applicationMessage.Topic, applicationMessage.UserProperties);
        }
    }

    /// <summary>
    /// Class for an MQTT client.
    /// </summary>
    public class MQTTClient
    {
        /// <summary>
        /// Enumeration for MQTT transports.
        /// </summary>
        public enum Transports { TCP, WebSockets }

        /// <summary>
        /// Enumeration for client states.
        /// </summary>
        public enum ClientState { Initial = 0, TransportConnecting = 1,
            TransportConnected = 2, Connected = 3, Disconnecting = 4, Disconnected = 5 }

        /// <summary>
        /// Host.
        /// </summary>
        public string host
        {
            get
            {
                if (mqttClient == null || mqttClient.Options == null)
                {
                    Logging.LogWarning("[MQTTClient->host] No client.");
                    return null;
                }

                return mqttClient.Options.Host;
            }
        }

        /// <summary>
        /// Port.
        /// </summary>
        public int port
        {
            get
            {
                if (mqttClient == null || mqttClient.Options == null)
                {
                    Logging.LogWarning("[MQTTClient->port] No client.");
                    return -1;
                }

                return mqttClient.Options.Port;
            }
        }

        /// <summary>
        /// Use TLS.
        /// </summary>
        public bool useTLS
        {
            get
            {
                if (mqttClient == null || mqttClient.Options == null)
                {
                    Logging.LogWarning("[MQTTClient->useTLS] No client.");
                    return false;
                }

                return mqttClient.Options.UseTLS;
            }
        }

        /// <summary>
        /// Transport.
        /// </summary>
        public SupportedTransports transport
        {
            get
            {
                if (mqttClient == null || mqttClient.Options == null)
                {
                    Logging.LogWarning("[MQTTClient->transport] No client.");
                    return SupportedTransports.WebSocket;
                }

                return mqttClient.Options.Transport;
            }
        }

        /// <summary>
        /// Client state.
        /// </summary>
        public ClientState clientState
        {
            get
            {
                if (mqttClient == null)
                {
                    Logging.LogWarning("[MQTTClient->clientState] No MQTT client.");
                    return ClientState.Initial;
                }

                switch (mqttClient.State)
                {
                    case ClientStates.Initial:
                        return ClientState.Initial;

                    case ClientStates.TransportConnecting:
                        return ClientState.TransportConnecting;

                    case ClientStates.TransportConnected:
                        return ClientState.TransportConnected;

                    case ClientStates.Connected:
                        return ClientState.Connected;

                    case ClientStates.Disconnecting:
                        return ClientState.Disconnecting;

                    case ClientStates.Disconnected:
                        return ClientState.Disconnected;

                    default:
                        return ClientState.Initial;
                }
            }
        }

        /// <summary>
        /// Reference to the internal MQTT client.
        /// </summary>
        private Best.MQTT.MQTTClient mqttClient;

        /// <summary>
        /// Constructor for an MQTT client.
        /// </summary>
        /// <param name="host">Host.</param>
        /// <param name="port">port.</param>
        /// <param name="useTLS">Whether or not to use TLS.</param>
        /// <param name="supportedTransports">Supported transports.</param>
        /// <param name="onConnected">Action to invoke upon connection.</param>
        /// <param name="onDisconnected">Action to invoke upon disconnection.</param>
        /// <param name="onStateChanged">Action to invoke upon connection state change.</param>
        /// <param name="onError">Action to invoke upon connection error.</param>
        /// <param name="path">Path.</param>
        public MQTTClient(string host, int port, bool useTLS, Transports supportedTransports,
            Action<MQTTClient> onConnected, Action<MQTTClient, byte, string> onDisconnected,
            Action<MQTTClient, ClientState, ClientState> onStateChanged,
            Action<MQTTClient, string> onError,
            string path = "/mqtt")
        {
            ConnectionOptions connectionOptions = new ConnectionOptions();
            connectionOptions.Host = host;
            connectionOptions.Port = port;
            connectionOptions.UseTLS = useTLS;
#if UNITY_EDITOR || !UNITY_WEBGL
            connectionOptions.Transport = supportedTransports ==
                Transports.WebSockets ? SupportedTransports.WebSocket : SupportedTransports.TCP;
#else
            connectionOptions.Transport = SupportedTransports.WebSocket;
#endif
            connectionOptions.Path = path;

            mqttClient = new Best.MQTT.MQTTClient(connectionOptions);

            mqttClient.OnConnected += new Best.MQTT.OnConnectedDelegate((client) =>
            {
                onConnected.Invoke(this);
            });

            mqttClient.OnDisconnect += new OnDisconnectDelegate((client, code, msg) =>
            {
                onDisconnected.Invoke(this, (byte) code, msg);
            });

            mqttClient.OnStateChanged += new OnStateChangedDelegate((client, oldState, newState) =>
            {
                onStateChanged.Invoke(this, (ClientState) oldState, (ClientState) newState);
            });

            mqttClient.OnError += new Best.MQTT.OnErrorDelegate((client, msg) =>
            {
                onError.Invoke(this, msg);
            });
        }

        /// <summary>
        /// Connect the MQTT client.
        /// </summary>
        public void Connect()
        {
            if (mqttClient == null)
            {
                Logging.LogWarning("[MQTTClient->Connect] Not initialized.");
                return;
            }
            
            mqttClient.BeginConnect(ConnectPacketBuilderCallback);
        }

        /// <summary>
        /// Disconnect the MQTT client.
        /// </summary>
        public void Disconnect(string reason = "disconnecting")
        {
            if (mqttClient == null)
            {
                Logging.LogWarning("[MQTTClient->Disconnect] Not initialized.");
                return;
            }
            
            mqttClient.CreateDisconnectPacketBuilder()
                .WithReasonCode(DisconnectReasonCodes.NormalDisconnection)
                .BeginDisconnect();
        }

        /// <summary>
        /// Subscribe the MQTT client to a topic.
        /// </summary>
        /// <param name="topic">Topic to subscribe to.</param>
        /// <param name="onAcknowledged">Action to invoke upon subscription acknowledgement.</param>
        /// <param name="onMessage">Action to invoke upon receiving a message.</param>
        public void Subscribe(string topic, Action<string> onAcknowledged,
            Action<MQTTClient, string, string, MQTTMessage> onMessage)
        {
            if (mqttClient == null)
            {
                Logging.LogWarning("[MQTTClient->Subscribe] Not initialized.");
                return;
            }

            mqttClient.CreateSubscriptionBuilder(topic)
                .WithAcknowledgementCallback((mqttClient, topicFilter, reasonCode) =>
                {
                    if (onAcknowledged != null)
                    {
                        onAcknowledged.Invoke(reasonCode.ToString());
                    }
                })
                .WithMessageCallback(new SubscriptionMessageDelegate((client, topic, topicName, msg) =>
                {
                    onMessage.Invoke(this, topic.ToString(), topicName, MQTTMessage.FromBestMQTT(msg));
                }))
                .WithMaximumQoS(Best.MQTT.Packets.QoSLevels.ExactlyOnceDelivery)
                .BeginSubscribe();
        }

        /// <summary>
        /// Unsubscribe the MQTT client from a topic.
        /// </summary>
        /// <param name="topic">Topic to unsubscribe from.</param>
        /// <param name="onAcknowledged">Action to invoke upon unsubscribe acknowledgement.</param>
        public void UnSubscribe(string topic, Action<string> onAcknowledged)
        {
            if (mqttClient == null)
            {
                Logging.LogWarning("[MQTTClient->UnSubscribe] Not initialized.");
                return;
            }

            if (mqttClient.State == ClientStates.Connected)
            {
                mqttClient.CreateUnsubscribePacketBuilder(topic)
                    .WithAcknowledgementCallback((mqttClient, topicFilter, reasonCode) =>
                    {
                        if (onAcknowledged != null)
                        {
                            onAcknowledged.Invoke(reasonCode.ToString());
                        }
                    })
                    .BeginUnsubscribe();
            }
        }

        /// <summary>
        /// Publish a message through the MQTT client.
        /// </summary>
        /// <param name="topic">Topic to send the message on.</param>
        /// <param name="message">Message to send.</param>
        public void Publish(string topic, string message)
        {
            if (mqttClient == null)
            {
                Logging.LogWarning("[MQTTClient->Publish] Not initialized.");
                return;
            }

            if (mqttClient.State != ClientStates.Connected)
            {
                Logging.LogWarning("[MQTTClient->Publish] Not connected");
                return;
            }

            mqttClient.CreateApplicationMessageBuilder(topic)
                .WithPayload(message)
                .WithQoS(Best.MQTT.Packets.QoSLevels.ExactlyOnceDelivery)
                .BeginPublish();
        }

        /// <summary>
        /// Conect packet builder callback.
        /// </summary>
        /// <param name="mqttClient">MQTT client.</param>
        /// <param name="builder">Builder.</param>
        private ConnectPacketBuilder ConnectPacketBuilderCallback(Best.MQTT.MQTTClient mqttClient, ConnectPacketBuilder builder)
        {
            // TODO smarter tracking of client id and other options.
            return builder.WithClientID(Guid.NewGuid().ToString());
        }
    }
}
#endif