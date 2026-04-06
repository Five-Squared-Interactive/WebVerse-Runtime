// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.WorldSync.Tests
{
    /// <summary>
    /// Tests for WorldSyncConfig (Story 6.1).
    /// </summary>
    [TestFixture]
    public class WorldSyncConfigTests
    {
        [Test]
        public void Constructor_SetsDefaultValues()
        {
            var config = new WorldSyncConfig();

            Assert.AreEqual(1883, config.Port);
            Assert.AreEqual(WorldSyncTransport.WebSocket, config.Transport);
            Assert.IsFalse(config.Tls.Enabled);
            Assert.IsNotNull(config.ClientId);
            Assert.AreEqual(30000, config.HeartbeatIntervalMs);
            Assert.IsTrue(config.AutoReconnect.Enabled);
            Assert.AreEqual(3, config.AutoReconnect.MaxAttempts);
        }

        [Test]
        public void Validate_ThrowsOnMissingHost()
        {
            var config = new WorldSyncConfig
            {
                ClientTag = "Test"
            };

            var ex = Assert.Throws<WorldSyncConfigException>(() => config.Validate());
            StringAssert.Contains("Host", ex.Message);
        }

        [Test]
        public void Validate_ThrowsOnInvalidPort()
        {
            var config = new WorldSyncConfig
            {
                Host = "localhost",
                Port = 99999,
                ClientTag = "Test"
            };

            var ex = Assert.Throws<WorldSyncConfigException>(() => config.Validate());
            StringAssert.Contains("Port", ex.Message);
        }

        [Test]
        public void Validate_ThrowsOnMissingClientTag()
        {
            var config = new WorldSyncConfig
            {
                Host = "localhost"
            };

            var ex = Assert.Throws<WorldSyncConfigException>(() => config.Validate());
            StringAssert.Contains("ClientTag", ex.Message);
        }

        [Test]
        public void Validate_ThrowsOnLowHeartbeat()
        {
            var config = new WorldSyncConfig
            {
                Host = "localhost",
                ClientTag = "Test",
                HeartbeatIntervalMs = 500
            };

            var ex = Assert.Throws<WorldSyncConfigException>(() => config.Validate());
            StringAssert.Contains("HeartbeatIntervalMs", ex.Message);
        }

        [Test]
        public void Validate_PassesWithValidConfig()
        {
            var config = new WorldSyncConfig
            {
                Host = "localhost",
                Port = 1883,
                ClientTag = "TestClient"
            };

            Assert.IsTrue(config.Validate());
        }

        [Test]
        public void Builder_CreatesValidConfig()
        {
            var config = WorldSyncConfig.Builder()
                .WithHost("broker.example.com")
                .WithPort(8883)
                .WithTransport(WorldSyncTransport.TCP)
                .WithTls(true)
                .WithClientId("my-client")
                .WithClientTag("Player1")
                .WithClientToken("secret-token")
                .WithHeartbeatInterval(15000)
                .WithAutoReconnect(true, 5, 2000)
                .Build();

            Assert.AreEqual("broker.example.com", config.Host);
            Assert.AreEqual(8883, config.Port);
            Assert.AreEqual(WorldSyncTransport.TCP, config.Transport);
            Assert.IsTrue(config.Tls.Enabled);
            Assert.AreEqual("my-client", config.ClientId);
            Assert.AreEqual("Player1", config.ClientTag);
            Assert.AreEqual("secret-token", config.ClientToken);
            Assert.AreEqual(15000, config.HeartbeatIntervalMs);
            Assert.IsTrue(config.AutoReconnect.Enabled);
            Assert.AreEqual(5, config.AutoReconnect.MaxAttempts);
            Assert.AreEqual(2000, config.AutoReconnect.InitialDelayMs);
        }

        [Test]
        public void Builder_WithoutAutoReconnect_DisablesIt()
        {
            var config = WorldSyncConfig.Builder()
                .WithHost("localhost")
                .WithClientTag("Test")
                .WithoutAutoReconnect()
                .Build();

            Assert.IsFalse(config.AutoReconnect.Enabled);
        }
    }

    /// <summary>
    /// Tests for SyncSession (Story 6.2).
    /// </summary>
    [TestFixture]
    public class SyncSessionTests
    {
        private WorldSyncClient _client;
        private WorldSyncConfig _config;

        [SetUp]
        public void Setup()
        {
            _config = WorldSyncConfig.Builder()
                .WithHost("localhost")
                .WithClientTag("TestClient")
                .Build();
            _client = new WorldSyncClient(_config);
        }

        [Test]
        public void Session_HasCorrectProperties()
        {
            var session = new SyncSession(_client, "session-123", "TestWorld",
                "2024-01-01T00:00:00Z", "client-1");

            Assert.AreEqual("session-123", session.SessionId);
            Assert.AreEqual("TestWorld", session.SessionTag);
            Assert.AreEqual("2024-01-01T00:00:00Z", session.CreatedAt);
            Assert.AreEqual("client-1", session.LocalClientId);
            Assert.IsTrue(session.IsValid);
        }

        [Test]
        public void Session_InitializeState_PopulatesCollections()
        {
            var session = new SyncSession(_client, "session-123", "TestWorld",
                "2024-01-01T00:00:00Z", "client-1");

            var state = new SessionState
            {
                SessionId = "session-123",
                Clients = new List<SyncClient>
                {
                    new SyncClient { ClientId = "client-1", ClientTag = "Player1" },
                    new SyncClient { ClientId = "client-2", ClientTag = "Player2" }
                },
                Entities = new List<SyncEntity>
                {
                    new SyncEntity { EntityId = "entity-1", OwnerId = "client-1", EntityType = "mesh" }
                }
            };

            session.InitializeState(state);

            Assert.AreEqual(2, session.ClientCount);
            Assert.AreEqual(1, session.EntityCount);
            Assert.IsTrue(session.HasEntity("entity-1"));
        }

        [Test]
        public void Session_GetEntity_ReturnsCorrectEntity()
        {
            var session = new SyncSession(_client, "session-123", "TestWorld",
                "2024-01-01T00:00:00Z", "client-1");

            var entity = new SyncEntity
            {
                EntityId = "entity-1",
                OwnerId = "client-1",
                EntityType = "mesh",
                EntityTag = "TestMesh"
            };

            session.HandleEntityCreated(entity);

            var retrieved = session.GetEntity("entity-1");
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("TestMesh", retrieved.EntityTag);
        }

        [Test]
        public void Session_GetEntity_ReturnsNullForMissing()
        {
            var session = new SyncSession(_client, "session-123", "TestWorld",
                "2024-01-01T00:00:00Z", "client-1");

            var retrieved = session.GetEntity("non-existent");
            Assert.IsNull(retrieved);
        }

        [Test]
        public void Session_HandleEntityDeleted_RemovesEntity()
        {
            var session = new SyncSession(_client, "session-123", "TestWorld",
                "2024-01-01T00:00:00Z", "client-1");

            session.HandleEntityCreated(new SyncEntity { EntityId = "entity-1" });
            Assert.IsTrue(session.HasEntity("entity-1"));

            session.HandleEntityDeleted("entity-1");
            Assert.IsFalse(session.HasEntity("entity-1"));
        }

        [Test]
        public void Session_HandleEntityTransform_UpdatesPosition()
        {
            var session = new SyncSession(_client, "session-123", "TestWorld",
                "2024-01-01T00:00:00Z", "client-1");

            session.HandleEntityCreated(new SyncEntity { EntityId = "entity-1" });
            session.HandleEntityTransform("entity-1", new SyncVector3(10, 20, 30), null, null);

            var entity = session.GetEntity("entity-1");
            Assert.AreEqual(10, entity.Position.x);
            Assert.AreEqual(20, entity.Position.y);
            Assert.AreEqual(30, entity.Position.z);
        }

        [Test]
        public void Session_Invalidate_MakesSessionInvalid()
        {
            var session = new SyncSession(_client, "session-123", "TestWorld",
                "2024-01-01T00:00:00Z", "client-1");

            Assert.IsTrue(session.IsValid);

            session.Invalidate("test");

            Assert.IsFalse(session.IsValid);
        }

        [Test]
        public void Session_GetOwnedEntities_ReturnsOnlyOwned()
        {
            var session = new SyncSession(_client, "session-123", "TestWorld",
                "2024-01-01T00:00:00Z", "client-1");

            session.HandleEntityCreated(new SyncEntity { EntityId = "e1", OwnerId = "client-1" });
            session.HandleEntityCreated(new SyncEntity { EntityId = "e2", OwnerId = "client-2" });
            session.HandleEntityCreated(new SyncEntity { EntityId = "e3", OwnerId = "client-1" });

            var owned = session.GetOwnedEntities();

            Assert.AreEqual(2, owned.Count);
            Assert.IsTrue(owned.Exists(e => e.EntityId == "e1"));
            Assert.IsTrue(owned.Exists(e => e.EntityId == "e3"));
        }
    }

    /// <summary>
    /// Tests for WorldSyncClient connection (Stories 6.3, 6.4, 6.5).
    /// </summary>
    [TestFixture]
    public class WorldSyncClientConnectionTests
    {
        [Test]
        public void Client_CreatesWithValidConfig()
        {
            var config = WorldSyncConfig.Builder()
                .WithHost("localhost")
                .WithClientTag("Test")
                .Build();

            var client = new WorldSyncClient(config);

            Assert.AreEqual(ConnectionState.Disconnected, client.State);
            Assert.IsFalse(client.IsConnected);
        }

        [Test]
        public void Client_ThrowsOnInvalidConfig()
        {
            var config = new WorldSyncConfig(); // Missing required fields

            Assert.Throws<WorldSyncConfigException>(() => new WorldSyncClient(config));
        }

        [Test]
        public void Client_AutoReconnectConfig_IsPreserved()
        {
            var config = WorldSyncConfig.Builder()
                .WithHost("localhost")
                .WithClientTag("Test")
                .WithAutoReconnect(true, 5, 500)
                .Build();

            var client = new WorldSyncClient(config);

            Assert.IsTrue(client.Config.AutoReconnect.Enabled);
            Assert.AreEqual(5, client.Config.AutoReconnect.MaxAttempts);
            Assert.AreEqual(500, client.Config.AutoReconnect.InitialDelayMs);
        }
    }

    /// <summary>
    /// Tests for operation queue (Story 6.6).
    /// </summary>
    [TestFixture]
    public class OperationQueueTests
    {
        [Test]
        public void Client_HasZeroPendingOperations_Initially()
        {
            var config = WorldSyncConfig.Builder()
                .WithHost("localhost")
                .WithClientTag("Test")
                .Build();

            var client = new WorldSyncClient(config);

            Assert.AreEqual(0, client.PendingOperationCount);
        }

        [Test]
        public void Client_MaxPendingOperations_HasDefaultValue()
        {
            var config = WorldSyncConfig.Builder()
                .WithHost("localhost")
                .WithClientTag("Test")
                .Build();

            var client = new WorldSyncClient(config);

            Assert.AreEqual(100, client.MaxPendingOperations);
        }

        [Test]
        public void Client_MaxPendingOperations_CanBeSet()
        {
            var config = WorldSyncConfig.Builder()
                .WithHost("localhost")
                .WithClientTag("Test")
                .Build();

            var client = new WorldSyncClient(config);
            client.MaxPendingOperations = 50;

            Assert.AreEqual(50, client.MaxPendingOperations);
        }
    }

    /// <summary>
    /// Tests for WorldSyncException.
    /// </summary>
    [TestFixture]
    public class WorldSyncExceptionTests
    {
        [Test]
        public void Exception_HasCorrectCode()
        {
            var ex = new WorldSyncException(WorldSyncErrorCode.SessionNotFound, "Test message");

            Assert.AreEqual(WorldSyncErrorCode.SessionNotFound, ex.Code);
            Assert.AreEqual("Test message", ex.Message);
        }

        [Test]
        public void SessionNotFound_CreatesCorrectException()
        {
            var ex = WorldSyncException.SessionNotFound("session-123");

            Assert.AreEqual(WorldSyncErrorCode.SessionNotFound, ex.Code);
            StringAssert.Contains("session-123", ex.Message);
        }

        [Test]
        public void NotConnected_CreatesCorrectException()
        {
            var ex = WorldSyncException.NotConnected();

            Assert.AreEqual(WorldSyncErrorCode.NotConnected, ex.Code);
        }

        [Test]
        public void ReconnectionFailed_IncludesAttemptCount()
        {
            var ex = WorldSyncException.ReconnectionFailed(3);

            Assert.AreEqual(WorldSyncErrorCode.ReconnectionFailed, ex.Code);
            StringAssert.Contains("3", ex.Message);
        }

        [Test]
        public void ToString_IncludesCodeAndMessage()
        {
            var ex = new WorldSyncException(WorldSyncErrorCode.EntityNotFound, "Entity missing");

            var str = ex.ToString();

            StringAssert.Contains("EntityNotFound", str);
            StringAssert.Contains("Entity missing", str);
        }
    }

    /// <summary>
    /// Tests for SyncEntity and types.
    /// </summary>
    [TestFixture]
    public class WorldSyncTypesTests
    {
        [Test]
        public void SyncVector3_ConvertsToUnityVector3()
        {
            var syncVec = new SyncVector3(1, 2, 3);
            UnityEngine.Vector3 unityVec = syncVec;

            Assert.AreEqual(1, unityVec.x);
            Assert.AreEqual(2, unityVec.y);
            Assert.AreEqual(3, unityVec.z);
        }

        [Test]
        public void SyncVector3_ConvertsFromUnityVector3()
        {
            var unityVec = new UnityEngine.Vector3(4, 5, 6);
            SyncVector3 syncVec = unityVec;

            Assert.AreEqual(4, syncVec.x);
            Assert.AreEqual(5, syncVec.y);
            Assert.AreEqual(6, syncVec.z);
        }

        [Test]
        public void SyncQuaternion_ConvertsToUnityQuaternion()
        {
            var syncQuat = new SyncQuaternion(0, 0, 0, 1);
            UnityEngine.Quaternion unityQuat = syncQuat;

            Assert.AreEqual(0, unityQuat.x);
            Assert.AreEqual(0, unityQuat.y);
            Assert.AreEqual(0, unityQuat.z);
            Assert.AreEqual(1, unityQuat.w);
        }

        [Test]
        public void SyncEntity_IsOwnedBy_ReturnsTrueForOwner()
        {
            var entity = new SyncEntity
            {
                EntityId = "entity-1",
                OwnerId = "client-123"
            };

            Assert.IsTrue(entity.IsOwnedBy("client-123"));
            Assert.IsFalse(entity.IsOwnedBy("client-456"));
        }

        [Test]
        public void SyncEntity_DefaultValues_AreSet()
        {
            var entity = new SyncEntity();

            Assert.AreEqual(SyncVector3.Zero.x, entity.Position.x);
            Assert.AreEqual(SyncQuaternion.Identity.w, entity.Rotation.w);
            Assert.AreEqual(SyncVector3.One.x, entity.Scale.x);
            Assert.IsTrue(entity.Visible);
            Assert.IsFalse(entity.Highlight);
            Assert.AreEqual(InteractionState.Static, entity.InteractionState);
        }
    }
}
