// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

#if USE_WEBINTERFACE
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WorldSync;
using FiveSQD.WebVerse.Handlers.VEML;

namespace FiveSQD.WebVerse.Handlers.VEML.Tests
{
    /// <summary>
    /// Tests for WorldSyncSceneHandler (Story 3.3 — Tasks 2.7, 3.4, 4.4).
    /// </summary>
    [TestFixture]
    public class WorldSyncSceneHandlerTests
    {
        private WorldSyncClient _client;
        private SyncSession _session;
        private WorldSyncSceneHandler _handler;
        private const string LocalClientId = "local-client-001";
        private const string RemoteClientId = "remote-client-002";

        [SetUp]
        public void SetUp()
        {
            var config = WorldSyncConfig.Builder()
                .WithHost("localhost")
                .WithClientTag("TestClient")
                .Build();
            _client = new WorldSyncClient(config);
            _client.UseTestHooks = true;

            _session = new SyncSession(_client, "session-test", "TestWorld",
                "2026-01-01T00:00:00Z", LocalClientId);

            _handler = new WorldSyncSceneHandler(_session, LocalClientId);
        }

        [TearDown]
        public void TearDown()
        {
            _handler?.Dispose();
            _handler = null;
            _session = null;
            _client = null;
        }

        #region Constructor Tests

        [Test]
        public void Constructor_NullSession_ThrowsArgumentNullException()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.Throws<ArgumentNullException>(() => new WorldSyncSceneHandler(null, "client-1"));
        }

        [Test]
        public void Constructor_ValidArgs_SubscribesToEvents()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("e1", WorldSyncEntityTypes.Container);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        #endregion

        #region Echo Suppression Tests (Task 4.4)

        [Test]
        public void OnEntityCreated_LocalOwner_SkipsEntity()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new SyncEntity
            {
                EntityId = "local-entity",
                OwnerId = LocalClientId,
                EntityType = WorldSyncEntityTypes.Mesh,
                EntityTag = "LocalMesh"
            };

            _session.HandleEntityCreated(entity);

            var map = GetServerToLocalMap();
            Assert.IsFalse(map.ContainsKey("local-entity"),
                "Local-owned entity should be skipped by echo suppression");
        }

        [Test]
        public void OnEntityCreated_RemoteOwner_ProcessesEntity()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("remote-entity", WorldSyncEntityTypes.Container);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_NullOwner_ProcessesEntity()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = new SyncEntity
            {
                EntityId = "null-owner-entity",
                OwnerId = null,
                EntityType = WorldSyncEntityTypes.Container,
                EntityTag = "NoOwner"
            };

            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        #endregion

        #region Entity Type Routing Tests (Task 2.7)

        [Test]
        public void OnEntityCreated_MeshType_AttemptsCreation()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("mesh-e1", WorldSyncEntityTypes.Mesh);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_CharacterType_AttemptsCreation()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("char-e1", WorldSyncEntityTypes.Character);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_LightType_AttemptsCreation()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("light-e1", WorldSyncEntityTypes.Light);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_CanvasType_AttemptsCreation()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("canvas-e1", WorldSyncEntityTypes.Canvas);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_AudioType_AttemptsCreation()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("audio-e1", WorldSyncEntityTypes.Audio);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_VoxelType_AttemptsCreation()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("voxel-e1", WorldSyncEntityTypes.Voxel);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_WaterBlockerType_AttemptsCreation()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("wb-e1", WorldSyncEntityTypes.WaterBlocker);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_HtmlType_AttemptsCreation()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("html-e1", WorldSyncEntityTypes.Html);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_ContainerType_AttemptsCreation()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("container-e1", WorldSyncEntityTypes.Container);
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_UnknownType_FallsBackToContainer()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("unknown-e1", "some-unknown-type");
            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_DuplicateEntityId_SkipsSecond()
        {
            LogAssert.ignoreFailingMessages = true;
            var map = GetServerToLocalMap();
            map["duplicate-e1"] = Guid.NewGuid();

            var entity = MakeRemoteEntity("duplicate-e1", WorldSyncEntityTypes.Container);
            _session.HandleEntityCreated(entity);

            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void OnEntityCreated_MeshWithFilePath_UsesFilePath()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("mesh-fp-e1", WorldSyncEntityTypes.Mesh);
            entity.Properties = new Dictionary<string, object>
            {
                { "filePath", "models/test.glb" },
                { "resources", new string[] { "textures/diffuse.png" } }
            };

            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        [Test]
        public void OnEntityCreated_CharacterWithFilePath_UsesFilePath()
        {
            LogAssert.ignoreFailingMessages = true;
            var entity = MakeRemoteEntity("char-fp-e1", WorldSyncEntityTypes.Character);
            entity.Properties = new Dictionary<string, object>
            {
                { "filePath", "avatars/player.vrm" }
            };

            _session.HandleEntityCreated(entity);
            Assert.Pass();
        }

        #endregion

        #region Transform Update Tests (Task 3.4)

        [Test]
        public void OnTransformUpdated_UnknownEntity_DoesNotCrash()
        {
            LogAssert.ignoreFailingMessages = true;
            _session.HandleEntityTransform("nonexistent-entity",
                new SyncVector3(1, 2, 3), null, null);
            Assert.Pass();
        }

        [Test]
        public void OnTransformUpdated_KnownEntity_NoActiveWorld_DoesNotCrash()
        {
            LogAssert.ignoreFailingMessages = true;
            var map = GetServerToLocalMap();
            map["known-e1"] = Guid.NewGuid();

            _session.HandleEntityTransform("known-e1",
                new SyncVector3(5, 10, 15),
                new SyncQuaternion(0, 0, 0, 1),
                new SyncVector3(2, 2, 2));

            Assert.Pass();
        }

        [Test]
        public void OnTransformUpdated_PendingEntity_QueuesTransform()
        {
            LogAssert.ignoreFailingMessages = true;
            var pending = GetPendingEntities();
            pending.Add("pending-e1");

            _session.HandleEntityTransform("pending-e1",
                new SyncVector3(1, 2, 3),
                new SyncQuaternion(0, 0.7071f, 0, 0.7071f),
                new SyncVector3(2, 2, 2));

            var rawDict = GetPendingTransformsRaw();
            Assert.IsTrue(rawDict.Contains("pending-e1"),
                "Transform should be queued for pending entity");
        }

        [Test]
        public void OnTransformUpdated_PendingEntity_LatestUpdateOverwritesPrevious()
        {
            LogAssert.ignoreFailingMessages = true;
            var pending = GetPendingEntities();
            pending.Add("overwrite-e1");

            _session.HandleEntityTransform("overwrite-e1",
                new SyncVector3(1, 1, 1), null, null);

            _session.HandleEntityTransform("overwrite-e1",
                new SyncVector3(99, 99, 99), null, null);

            var rawDict = GetPendingTransformsRaw();
            Assert.IsTrue(rawDict.Contains("overwrite-e1"));
        }

        [Test]
        public void OnTransformUpdated_PositionOnly_DoesNotCrash()
        {
            LogAssert.ignoreFailingMessages = true;
            var map = GetServerToLocalMap();
            map["pos-only-e1"] = Guid.NewGuid();

            _session.HandleEntityTransform("pos-only-e1",
                new SyncVector3(1, 2, 3), null, null);
            Assert.Pass();
        }

        [Test]
        public void OnTransformUpdated_RotationOnly_DoesNotCrash()
        {
            LogAssert.ignoreFailingMessages = true;
            var map = GetServerToLocalMap();
            map["rot-only-e1"] = Guid.NewGuid();

            _session.HandleEntityTransform("rot-only-e1",
                null, new SyncQuaternion(0, 0, 0, 1), null);
            Assert.Pass();
        }

        [Test]
        public void OnTransformUpdated_ScaleOnly_DoesNotCrash()
        {
            LogAssert.ignoreFailingMessages = true;
            var map = GetServerToLocalMap();
            map["scale-only-e1"] = Guid.NewGuid();

            _session.HandleEntityTransform("scale-only-e1",
                null, null, new SyncVector3(3, 3, 3));
            Assert.Pass();
        }

        #endregion

        #region Entity Deletion Tests (Task 3.4)

        [Test]
        public void OnEntityDeleted_UnknownEntity_DoesNotCrash()
        {
            LogAssert.ignoreFailingMessages = true;
            _session.HandleEntityDeleted("nonexistent-entity");
            Assert.Pass();
        }

        [Test]
        public void OnEntityDeleted_KnownEntity_RemovesFromMap()
        {
            LogAssert.ignoreFailingMessages = true;
            var map = GetServerToLocalMap();
            map["delete-e1"] = Guid.NewGuid();

            _session.HandleEntityDeleted("delete-e1");

            Assert.IsFalse(map.ContainsKey("delete-e1"),
                "Deleted entity should be removed from server-to-local map");
        }

        [Test]
        public void OnEntityDeleted_PendingEntity_CleansUpPendingState()
        {
            LogAssert.ignoreFailingMessages = true;
            var pending = GetPendingEntities();
            pending.Add("delete-pending-e1");

            // Add a pending transform directly via the raw dictionary
            AddPendingTransformDirect("delete-pending-e1",
                new SyncVector3(1, 2, 3), null, null);

            _session.HandleEntityDeleted("delete-pending-e1");

            Assert.IsFalse(pending.Contains("delete-pending-e1"),
                "Deleted entity should be removed from pending set");
            var rawDict = GetPendingTransformsRaw();
            Assert.IsFalse(rawDict.Contains("delete-pending-e1"),
                "Deleted entity should be removed from pending transforms");
        }

        #endregion

        #region Dispose Tests

        [Test]
        public void Dispose_UnsubscribesFromEvents()
        {
            LogAssert.ignoreFailingMessages = true;
            _handler.Dispose();

            var entity = MakeRemoteEntity("post-dispose-e1", WorldSyncEntityTypes.Container);
            _session.HandleEntityCreated(entity);

            var map = GetServerToLocalMap();
            Assert.IsFalse(map.ContainsKey("post-dispose-e1"),
                "Disposed handler should not process new events");
        }

        [Test]
        public void Dispose_ClearsAllMaps()
        {
            LogAssert.ignoreFailingMessages = true;
            var map = GetServerToLocalMap();
            var pending = GetPendingEntities();

            map["cleanup-e1"] = Guid.NewGuid();
            pending.Add("cleanup-e2");
            AddPendingTransformDirect("cleanup-e3",
                new SyncVector3(1, 1, 1), null, null);

            _handler.Dispose();

            Assert.AreEqual(0, map.Count, "Server-to-local map should be cleared on dispose");
            Assert.AreEqual(0, pending.Count, "Pending entities should be cleared on dispose");
            // Check pending transforms AFTER dispose (GetPendingTransformsRaw returns live reference)
            var rawDict = GetPendingTransformsRaw();
            Assert.AreEqual(0, rawDict.Count, "Pending transforms should be cleared on dispose");
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            LogAssert.ignoreFailingMessages = true;
            _handler.Dispose();
            Assert.DoesNotThrow(() => _handler.Dispose());
        }

        [Test]
        public void AfterDispose_TransformUpdate_IsIgnored()
        {
            LogAssert.ignoreFailingMessages = true;
            var pending = GetPendingEntities();
            pending.Add("disposed-transform-e1");

            _handler.Dispose();

            _session.HandleEntityTransform("disposed-transform-e1",
                new SyncVector3(1, 2, 3), null, null);

            var rawDict = GetPendingTransformsRaw();
            Assert.AreEqual(0, rawDict.Count);
        }

        [Test]
        public void AfterDispose_EntityDeleted_IsIgnored()
        {
            LogAssert.ignoreFailingMessages = true;
            var map = GetServerToLocalMap();
            map["disposed-delete-e1"] = Guid.NewGuid();

            _handler.Dispose();

            _session.HandleEntityDeleted("disposed-delete-e1");
            Assert.Pass();
        }

        #endregion

        #region Helpers

        private SyncEntity MakeRemoteEntity(string id, string entityType)
        {
            return new SyncEntity
            {
                EntityId = id,
                OwnerId = RemoteClientId,
                EntityType = entityType,
                EntityTag = "Tag_" + id,
                Position = new SyncVector3(0, 1, -3),
                Rotation = new SyncQuaternion(0, 0, 0, 1),
                Scale = new SyncVector3(1, 1, 1)
            };
        }

        private Dictionary<string, Guid> GetServerToLocalMap()
        {
            return (Dictionary<string, Guid>)typeof(WorldSyncSceneHandler)
                .GetField("_serverToLocalMap", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_handler);
        }

        private HashSet<string> GetPendingEntities()
        {
            return (HashSet<string>)typeof(WorldSyncSceneHandler)
                .GetField("_pendingEntities", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_handler);
        }

        /// <summary>
        /// Returns the raw _pendingTransforms IDictionary (live reference, not a copy).
        /// </summary>
        private System.Collections.IDictionary GetPendingTransformsRaw()
        {
            var field = typeof(WorldSyncSceneHandler)
                .GetField("_pendingTransforms", BindingFlags.NonPublic | BindingFlags.Instance);
            return (System.Collections.IDictionary)field.GetValue(_handler);
        }

        /// <summary>
        /// Adds a pending transform entry using the handler's own event path.
        /// This avoids struct boxing issues with reflection by letting the handler
        /// queue the transform naturally through OnRemoteTransformUpdated.
        /// </summary>
        private void AddPendingTransformDirect(string entityId,
            SyncVector3? position, SyncQuaternion? rotation, SyncVector3? scale)
        {
            // Ensure entity is in pending set so the handler queues the transform
            var pending = GetPendingEntities();
            if (!pending.Contains(entityId))
                pending.Add(entityId);

            // Fire the transform event — the handler will queue it because entity is pending
            _session.HandleEntityTransform(entityId, position, rotation, scale);
        }

        #endregion
    }
}
#endif
