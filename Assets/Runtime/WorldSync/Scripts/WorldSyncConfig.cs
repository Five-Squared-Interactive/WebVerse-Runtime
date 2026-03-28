// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.WorldSync
{
    /// <summary>
    /// Transport type for WorldSync connection.
    /// </summary>
    public enum WorldSyncTransport
    {
        /// <summary>
        /// TCP transport (native platforms only).
        /// </summary>
        TCP,

        /// <summary>
        /// WebSocket transport (all platforms).
        /// </summary>
        WebSocket
    }

    /// <summary>
    /// Configuration for auto-reconnection behavior.
    /// </summary>
    [Serializable]
    public class AutoReconnectConfig
    {
        /// <summary>
        /// Whether auto-reconnect is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum number of reconnection attempts before giving up.
        /// </summary>
        public int MaxAttempts { get; set; } = 3;

        /// <summary>
        /// Initial delay before first reconnection attempt in milliseconds.
        /// </summary>
        public int InitialDelayMs { get; set; } = 1000;

        /// <summary>
        /// Maximum delay between reconnection attempts in milliseconds.
        /// </summary>
        public int MaxDelayMs { get; set; } = 30000;

        /// <summary>
        /// Multiplier for exponential backoff (e.g., 2.0 for doubling).
        /// </summary>
        public float BackoffMultiplier { get; set; } = 2.0f;
    }

    /// <summary>
    /// TLS configuration options.
    /// </summary>
    [Serializable]
    public class TlsConfig
    {
        /// <summary>
        /// Whether TLS is enabled.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Whether to validate server certificate (set to false for self-signed certs in development).
        /// </summary>
        public bool ValidateCertificate { get; set; } = true;
    }

    /// <summary>
    /// Configuration for WorldSync client connection.
    /// </summary>
    [Serializable]
    public class WorldSyncConfig
    {
        /// <summary>
        /// MQTT broker host address.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// MQTT broker port.
        /// </summary>
        public int Port { get; set; } = 1883;

        /// <summary>
        /// Transport type (TCP or WebSocket).
        /// </summary>
        public WorldSyncTransport Transport { get; set; } = WorldSyncTransport.WebSocket;

        /// <summary>
        /// TLS configuration.
        /// </summary>
        public TlsConfig Tls { get; set; } = new TlsConfig();

        /// <summary>
        /// Unique client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Client authentication token.
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// Human-readable client tag/name.
        /// </summary>
        public string ClientTag { get; set; }

        /// <summary>
        /// Heartbeat interval in milliseconds.
        /// </summary>
        public int HeartbeatIntervalMs { get; set; } = 30000;

        /// <summary>
        /// Auto-reconnect configuration.
        /// </summary>
        public AutoReconnectConfig AutoReconnect { get; set; } = new AutoReconnectConfig();

        /// <summary>
        /// Connection timeout in milliseconds.
        /// </summary>
        public int ConnectTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// WebSocket path (used when Transport is WebSocket).
        /// </summary>
        public string WebSocketPath { get; set; } = "/mqtt";

        /// <summary>
        /// Create a default configuration.
        /// </summary>
        public WorldSyncConfig()
        {
            ClientId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Validate the configuration.
        /// </summary>
        /// <returns>True if configuration is valid.</returns>
        /// <exception cref="WorldSyncConfigException">Thrown when configuration is invalid.</exception>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Host))
            {
                throw new WorldSyncConfigException("Host is required and cannot be empty.");
            }

            if (Port < 1 || Port > 65535)
            {
                throw new WorldSyncConfigException($"Port must be between 1 and 65535, got {Port}.");
            }

            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new WorldSyncConfigException("ClientId is required and cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(ClientTag))
            {
                throw new WorldSyncConfigException("ClientTag is required and cannot be empty.");
            }

            if (HeartbeatIntervalMs < 1000)
            {
                throw new WorldSyncConfigException($"HeartbeatIntervalMs must be at least 1000ms, got {HeartbeatIntervalMs}.");
            }

            if (ConnectTimeoutMs < 1000)
            {
                throw new WorldSyncConfigException($"ConnectTimeoutMs must be at least 1000ms, got {ConnectTimeoutMs}.");
            }

            if (AutoReconnect != null)
            {
                if (AutoReconnect.MaxAttempts < 1)
                {
                    throw new WorldSyncConfigException($"AutoReconnect.MaxAttempts must be at least 1, got {AutoReconnect.MaxAttempts}.");
                }

                if (AutoReconnect.InitialDelayMs < 100)
                {
                    throw new WorldSyncConfigException($"AutoReconnect.InitialDelayMs must be at least 100ms, got {AutoReconnect.InitialDelayMs}.");
                }

                if (AutoReconnect.BackoffMultiplier < 1.0f)
                {
                    throw new WorldSyncConfigException($"AutoReconnect.BackoffMultiplier must be at least 1.0, got {AutoReconnect.BackoffMultiplier}.");
                }
            }

            return true;
        }

        /// <summary>
        /// Create a builder for WorldSyncConfig.
        /// </summary>
        public static WorldSyncConfigBuilder Builder()
        {
            return new WorldSyncConfigBuilder();
        }
    }

    /// <summary>
    /// Builder for WorldSyncConfig.
    /// </summary>
    public class WorldSyncConfigBuilder
    {
        private readonly WorldSyncConfig _config = new WorldSyncConfig();

        /// <summary>
        /// Set the host address.
        /// </summary>
        public WorldSyncConfigBuilder WithHost(string host)
        {
            _config.Host = host;
            return this;
        }

        /// <summary>
        /// Set the port.
        /// </summary>
        public WorldSyncConfigBuilder WithPort(int port)
        {
            _config.Port = port;
            return this;
        }

        /// <summary>
        /// Set the transport type.
        /// </summary>
        public WorldSyncConfigBuilder WithTransport(WorldSyncTransport transport)
        {
            _config.Transport = transport;
            return this;
        }

        /// <summary>
        /// Enable TLS.
        /// </summary>
        public WorldSyncConfigBuilder WithTls(bool enabled = true, bool validateCertificate = true)
        {
            _config.Tls = new TlsConfig
            {
                Enabled = enabled,
                ValidateCertificate = validateCertificate
            };
            return this;
        }

        /// <summary>
        /// Set the client ID.
        /// </summary>
        public WorldSyncConfigBuilder WithClientId(string clientId)
        {
            _config.ClientId = clientId;
            return this;
        }

        /// <summary>
        /// Set the client token.
        /// </summary>
        public WorldSyncConfigBuilder WithClientToken(string clientToken)
        {
            _config.ClientToken = clientToken;
            return this;
        }

        /// <summary>
        /// Set the client tag.
        /// </summary>
        public WorldSyncConfigBuilder WithClientTag(string clientTag)
        {
            _config.ClientTag = clientTag;
            return this;
        }

        /// <summary>
        /// Set the heartbeat interval.
        /// </summary>
        public WorldSyncConfigBuilder WithHeartbeatInterval(int intervalMs)
        {
            _config.HeartbeatIntervalMs = intervalMs;
            return this;
        }

        /// <summary>
        /// Configure auto-reconnect.
        /// </summary>
        public WorldSyncConfigBuilder WithAutoReconnect(bool enabled = true, int maxAttempts = 3, int initialDelayMs = 1000)
        {
            _config.AutoReconnect = new AutoReconnectConfig
            {
                Enabled = enabled,
                MaxAttempts = maxAttempts,
                InitialDelayMs = initialDelayMs
            };
            return this;
        }

        /// <summary>
        /// Disable auto-reconnect.
        /// </summary>
        public WorldSyncConfigBuilder WithoutAutoReconnect()
        {
            _config.AutoReconnect = new AutoReconnectConfig { Enabled = false };
            return this;
        }

        /// <summary>
        /// Set the connection timeout.
        /// </summary>
        public WorldSyncConfigBuilder WithConnectTimeout(int timeoutMs)
        {
            _config.ConnectTimeoutMs = timeoutMs;
            return this;
        }

        /// <summary>
        /// Set the WebSocket path.
        /// </summary>
        public WorldSyncConfigBuilder WithWebSocketPath(string path)
        {
            _config.WebSocketPath = path;
            return this;
        }

        /// <summary>
        /// Build and validate the configuration.
        /// </summary>
        /// <returns>Validated WorldSyncConfig.</returns>
        /// <exception cref="WorldSyncConfigException">Thrown when configuration is invalid.</exception>
        public WorldSyncConfig Build()
        {
            _config.Validate();
            return _config;
        }
    }

    /// <summary>
    /// Exception thrown when WorldSync configuration is invalid.
    /// </summary>
    public class WorldSyncConfigException : Exception
    {
        /// <summary>
        /// Create a new WorldSyncConfigException.
        /// </summary>
        public WorldSyncConfigException(string message) : base(message)
        {
        }

        /// <summary>
        /// Create a new WorldSyncConfigException with inner exception.
        /// </summary>
        public WorldSyncConfigException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
