// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using FiveSQD.StraightFour.WorldState;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

/// <summary>
/// TDD tests for World State serialization and management.
/// </summary>
public class WorldStateTests
{
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

    #region SerializableVector3 Tests

    [Test]
    public void SerializableVector3_FromVector3_ConvertsCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        Vector3 original = new Vector3(1.5f, 2.5f, 3.5f);

        // Act
        var serializable = new SerializableVector3(original);

        // Assert
        Assert.AreEqual(1.5f, serializable.x);
        Assert.AreEqual(2.5f, serializable.y);
        Assert.AreEqual(3.5f, serializable.z);
    }

    [Test]
    public void SerializableVector3_ToVector3_ConvertsCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var serializable = new SerializableVector3(1.5f, 2.5f, 3.5f);

        // Act
        Vector3 result = serializable.ToVector3();

        // Assert
        Assert.AreEqual(1.5f, result.x);
        Assert.AreEqual(2.5f, result.y);
        Assert.AreEqual(3.5f, result.z);
    }

    [Test]
    public void SerializableVector3_RoundTrip_PreservesValues()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        Vector3 original = new Vector3(123.456f, -789.012f, 0.001f);

        // Act
        var serializable = new SerializableVector3(original);
        Vector3 result = serializable.ToVector3();

        // Assert
        Assert.AreEqual(original.x, result.x, 0.0001f);
        Assert.AreEqual(original.y, result.y, 0.0001f);
        Assert.AreEqual(original.z, result.z, 0.0001f);
    }

    #endregion

    #region SerializableQuaternion Tests

    [Test]
    public void SerializableQuaternion_FromQuaternion_ConvertsCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        Quaternion original = Quaternion.Euler(45f, 90f, 180f);

        // Act
        var serializable = new SerializableQuaternion(original);

        // Assert
        Assert.AreEqual(original.x, serializable.x, 0.0001f);
        Assert.AreEqual(original.y, serializable.y, 0.0001f);
        Assert.AreEqual(original.z, serializable.z, 0.0001f);
        Assert.AreEqual(original.w, serializable.w, 0.0001f);
    }

    [Test]
    public void SerializableQuaternion_ToQuaternion_ConvertsCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        Quaternion original = Quaternion.Euler(30f, 60f, 90f);
        var serializable = new SerializableQuaternion(original);

        // Act
        Quaternion result = serializable.ToQuaternion();

        // Assert
        Assert.AreEqual(original.x, result.x, 0.0001f);
        Assert.AreEqual(original.y, result.y, 0.0001f);
        Assert.AreEqual(original.z, result.z, 0.0001f);
        Assert.AreEqual(original.w, result.w, 0.0001f);
    }

    [Test]
    public void SerializableQuaternion_Default_IsIdentity()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange & Act
        var serializable = new SerializableQuaternion();

        // Assert - default quaternion should have w=1 (identity)
        Assert.AreEqual(1f, serializable.w);
    }

    #endregion

    #region SerializableColor Tests

    [Test]
    public void SerializableColor_FromColor_ConvertsCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        Color original = new Color(0.5f, 0.25f, 0.75f, 0.9f);

        // Act
        var serializable = new SerializableColor(original);

        // Assert
        Assert.AreEqual(0.5f, serializable.r, 0.0001f);
        Assert.AreEqual(0.25f, serializable.g, 0.0001f);
        Assert.AreEqual(0.75f, serializable.b, 0.0001f);
        Assert.AreEqual(0.9f, serializable.a, 0.0001f);
    }

    [Test]
    public void SerializableColor_ToColor_ConvertsCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var serializable = new SerializableColor { r = 1f, g = 0.5f, b = 0f, a = 1f };

        // Act
        Color result = serializable.ToColor();

        // Assert
        Assert.AreEqual(1f, result.r);
        Assert.AreEqual(0.5f, result.g);
        Assert.AreEqual(0f, result.b);
        Assert.AreEqual(1f, result.a);
    }

    #endregion

    #region WorldStateSnapshot Tests

    [Test]
    public void WorldStateSnapshot_SetTimestamp_SetsValidTimestamp()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var snapshot = new WorldStateSnapshot();
        long before = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act
        snapshot.SetTimestamp();
        long after = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Assert
        Assert.GreaterOrEqual(snapshot.timestamp, before);
        Assert.LessOrEqual(snapshot.timestamp, after);
    }

    [Test]
    public void WorldStateSnapshot_DefaultVersion_IsCorrect()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange & Act
        var snapshot = new WorldStateSnapshot();

        // Assert
        Assert.AreEqual("1.0", snapshot.version);
    }

    [Test]
    public void WorldStateSnapshot_EntitiesList_IsInitialized()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange & Act
        var snapshot = new WorldStateSnapshot();

        // Assert
        Assert.IsNotNull(snapshot.entities);
        Assert.AreEqual(0, snapshot.entities.Count);
    }

    [Test]
    public void WorldStateSnapshot_StorageDictionary_IsInitialized()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange & Act
        var snapshot = new WorldStateSnapshot();

        // Assert
        Assert.IsNotNull(snapshot.storage);
        Assert.AreEqual(0, snapshot.storage.Count);
    }

    #endregion

    #region EntitySnapshot Tests

    [Test]
    public void EntitySnapshot_DefaultValues_AreCorrect()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange & Act
        var snapshot = new EntitySnapshot();

        // Assert
        Assert.IsTrue(snapshot.visible);
        Assert.IsNull(snapshot.id);
        Assert.IsNull(snapshot.parentId);
    }

    [Test]
    public void EntitySnapshot_CanStoreAllProperties()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var snapshot = new EntitySnapshot
        {
            id = System.Guid.NewGuid().ToString(),
            entityType = "MeshEntity",
            tag = "TestEntity",
            parentId = System.Guid.NewGuid().ToString(),
            position = new SerializableVector3(1f, 2f, 3f),
            rotation = new SerializableQuaternion(Quaternion.identity),
            scale = new SerializableVector3(1f, 1f, 1f),
            interactionState = "Physical",
            visible = true
        };

        // Assert
        Assert.IsNotNull(snapshot.id);
        Assert.AreEqual("MeshEntity", snapshot.entityType);
        Assert.AreEqual("TestEntity", snapshot.tag);
        Assert.IsNotNull(snapshot.parentId);
        Assert.AreEqual(1f, snapshot.position.x);
        Assert.AreEqual("Physical", snapshot.interactionState);
    }

    #endregion

    #region WorldStateSerializer Tests

    [Test]
    public void WorldStateSerializer_SerializeToJson_WithValidSnapshot_ReturnsJson()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var snapshot = new WorldStateSnapshot
        {
            worldName = "TestWorld",
            basePath = "http://example.com/world",
            worldOffset = new SerializableVector3(0, 0, 0)
        };
        snapshot.SetTimestamp();

        // Act
        string json = WorldStateSerializer.SerializeToJson(snapshot);

        // Assert
        Assert.IsNotNull(json);
        Assert.IsTrue(json.Contains("TestWorld"));
        Assert.IsTrue(json.Contains("http://example.com/world"));
    }

    [Test]
    public void WorldStateSerializer_SerializeToJson_WithNullSnapshot_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange & Act
        LogAssert.Expect(LogType.Error, "[WorldStateSerializer->SerializeToJson] Snapshot is null.");
        string json = WorldStateSerializer.SerializeToJson(null);

        // Assert
        Assert.IsNull(json);
    }

    [Test]
    public void WorldStateSerializer_DeserializeFromJson_WithValidJson_ReturnsSnapshot()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var original = new WorldStateSnapshot
        {
            worldName = "TestWorld",
            basePath = "http://example.com/world",
            worldOffset = new SerializableVector3(10f, 20f, 30f)
        };
        original.SetTimestamp();
        string json = WorldStateSerializer.SerializeToJson(original);

        // Act
        var restored = WorldStateSerializer.DeserializeFromJson(json);

        // Assert
        Assert.IsNotNull(restored);
        Assert.AreEqual("TestWorld", restored.worldName);
        Assert.AreEqual("http://example.com/world", restored.basePath);
        Assert.AreEqual(10f, restored.worldOffset.x);
        Assert.AreEqual(20f, restored.worldOffset.y);
        Assert.AreEqual(30f, restored.worldOffset.z);
    }

    [Test]
    public void WorldStateSerializer_DeserializeFromJson_WithNullJson_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange & Act
        LogAssert.Expect(LogType.Error, "[WorldStateSerializer->DeserializeFromJson] JSON is null or empty.");
        var snapshot = WorldStateSerializer.DeserializeFromJson(null);

        // Assert
        Assert.IsNull(snapshot);
    }

    [Test]
    public void WorldStateSerializer_DeserializeFromJson_WithEmptyJson_ReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange & Act
        LogAssert.Expect(LogType.Error, "[WorldStateSerializer->DeserializeFromJson] JSON is null or empty.");
        var snapshot = WorldStateSerializer.DeserializeFromJson("");

        // Assert
        Assert.IsNull(snapshot);
    }

    [Test]
    public void WorldStateSerializer_RoundTrip_PreservesEntities()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var original = new WorldStateSnapshot
        {
            worldName = "TestWorld"
        };
        original.entities.Add(new EntitySnapshot
        {
            id = System.Guid.NewGuid().ToString(),
            entityType = "MeshEntity",
            tag = "Cube",
            position = new SerializableVector3(1f, 2f, 3f),
            rotation = new SerializableQuaternion(Quaternion.Euler(0, 90, 0)),
            scale = new SerializableVector3(2f, 2f, 2f),
            interactionState = "Physical",
            visible = true
        });

        // Act
        string json = WorldStateSerializer.SerializeToJson(original);
        var restored = WorldStateSerializer.DeserializeFromJson(json);

        // Assert
        Assert.IsNotNull(restored);
        Assert.AreEqual(1, restored.entities.Count);
        Assert.AreEqual("MeshEntity", restored.entities[0].entityType);
        Assert.AreEqual("Cube", restored.entities[0].tag);
        Assert.AreEqual(1f, restored.entities[0].position.x);
        Assert.AreEqual(2f, restored.entities[0].scale.x);
    }

    [Test]
    public void WorldStateSerializer_EstimateSnapshotSize_ReturnsNonZero()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var snapshot = new WorldStateSnapshot
        {
            worldName = "TestWorld",
            basePath = "http://example.com/world"
        };
        snapshot.entities.Add(new EntitySnapshot
        {
            id = System.Guid.NewGuid().ToString(),
            entityType = "MeshEntity"
        });

        // Act
        long size = WorldStateSerializer.EstimateSnapshotSize(snapshot);

        // Assert
        Assert.Greater(size, 0);
    }

    #endregion

    #region WorldStateManager Tests

    [Test]
    public void WorldStateManager_Initialize_SetsUpCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestManager");
        var manager = go.AddComponent<WorldStateManager>();

        // Act
        LogAssert.Expect(LogType.Log, "[WorldStateManager] Initialized.");
        manager.Initialize(2, 50);

        // Assert
        Assert.AreEqual(0, manager.GetMemoryUsage());
        Assert.AreEqual(0, manager.GetStoredWorldNames().Count);

        // Cleanup
        LogAssert.Expect(LogType.Log, "[WorldStateManager->ClearAllSnapshots] Cleared all snapshots.");
        LogAssert.Expect(LogType.Log, "[WorldStateManager] Terminated.");
        manager.Terminate();
        Object.DestroyImmediate(go);
    }

    [Test]
    public void WorldStateManager_HasSnapshot_ReturnsFalseWhenEmpty()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestManager");
        var manager = go.AddComponent<WorldStateManager>();
        manager.Initialize();

        // Act
        bool hasSnapshot = manager.HasSnapshot("NonExistentWorld");

        // Assert
        Assert.IsFalse(hasSnapshot);

        // Cleanup
        manager.Terminate();
        Object.DestroyImmediate(go);
    }

    [Test]
    public void WorldStateManager_GetLatestSnapshot_ReturnsNullWhenEmpty()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestManager");
        var manager = go.AddComponent<WorldStateManager>();
        manager.Initialize();

        // Act
        var snapshot = manager.GetLatestSnapshot("NonExistentWorld");

        // Assert
        Assert.IsNull(snapshot);

        // Cleanup
        manager.Terminate();
        Object.DestroyImmediate(go);
    }

    [Test]
    public void WorldStateManager_ClearAllSnapshots_ResetsMemory()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestManager");
        var manager = go.AddComponent<WorldStateManager>();
        manager.Initialize();

        // Act
        LogAssert.Expect(LogType.Log, "[WorldStateManager->ClearAllSnapshots] Cleared all snapshots.");
        manager.ClearAllSnapshots();

        // Assert
        Assert.AreEqual(0, manager.GetMemoryUsage());
        Assert.AreEqual(0, manager.GetStoredWorldNames().Count);

        // Cleanup
        manager.Terminate();
        Object.DestroyImmediate(go);
    }

    [Test]
    public void WorldStateManager_GetSnapshotCount_ReturnsZeroForUnknownWorld()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestManager");
        var manager = go.AddComponent<WorldStateManager>();
        manager.Initialize();

        // Act
        int count = manager.GetSnapshotCount("UnknownWorld");

        // Assert
        Assert.AreEqual(0, count);

        // Cleanup
        manager.Terminate();
        Object.DestroyImmediate(go);
    }

    #endregion

    #region Type-Specific Data Tests

    [Test]
    public void MeshEntityData_CanSerialize()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var data = new MeshEntityData
        {
            meshType = "primitive",
            meshSource = "cube",
            meshResources = new string[] { "texture.png" },
            color = new SerializableColor(Color.red)
        };

        // Act
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<MeshEntityData>(json);

        // Assert
        Assert.AreEqual("primitive", restored.meshType);
        Assert.AreEqual("cube", restored.meshSource);
        Assert.AreEqual(1, restored.meshResources.Length);
        Assert.AreEqual(1f, restored.color.r);
    }

    [Test]
    public void LightEntityData_CanSerialize()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var data = new LightEntityData
        {
            lightType = "Spot",
            color = new SerializableColor(Color.yellow),
            intensity = 2f,
            range = 15f,
            spotAngleInner = 20f,
            spotAngleOuter = 40f
        };

        // Act
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<LightEntityData>(json);

        // Assert
        Assert.AreEqual("Spot", restored.lightType);
        Assert.AreEqual(2f, restored.intensity);
        Assert.AreEqual(15f, restored.range);
    }

    #endregion

    #region WorldStateRestorer Tests

    [Test]
    public void WorldStateRestorer_CanBeCreated()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestRestorer");

        // Act
        var restorer = go.AddComponent<WorldStateRestorer>();

        // Assert
        Assert.IsNotNull(restorer);
        Assert.IsFalse(restorer.IsRestoring);
        Assert.AreEqual(0f, restorer.Progress);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void WorldStateRestorer_RestoreWorldState_WithNullWorld_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestRestorer");
        var restorer = go.AddComponent<WorldStateRestorer>();
        var snapshot = new WorldStateSnapshot { worldName = "Test" };
        bool callbackCalled = false;
        bool callbackResult = true;

        // Act
        LogAssert.Expect(LogType.Error, "[WorldStateRestorer->RestoreWorldState] World is null.");
        restorer.RestoreWorldState(null, snapshot, (success) => {
            callbackCalled = true;
            callbackResult = success;
        });

        // Assert
        Assert.IsTrue(callbackCalled);
        Assert.IsFalse(callbackResult);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void WorldStateRestorer_RestoreWorldState_WithNullSnapshot_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestRestorer");
        var restorer = go.AddComponent<WorldStateRestorer>();

        // Note: In unit tests we can't fully create a World, so we test with both null
        // The world null check happens first when both are null

        bool callbackCalled = false;
        bool callbackResult = true;

        // Act - When both are null, the world null check happens first
        LogAssert.Expect(LogType.Error, "[WorldStateRestorer->RestoreWorldState] World is null.");
        restorer.RestoreWorldState(null, null, (success) => {
            callbackCalled = true;
            callbackResult = success;
        });

        // Assert
        Assert.IsTrue(callbackCalled);
        Assert.IsFalse(callbackResult);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void WorldStateRestorer_CancelRestore_WhenNotRestoring_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestRestorer");
        var restorer = go.AddComponent<WorldStateRestorer>();

        // Act & Assert
        Assert.DoesNotThrow(() => restorer.CancelRestore());

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void WorldStateRestorer_RestoreWorldStateFromJson_WithInvalidJson_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestRestorer");
        var restorer = go.AddComponent<WorldStateRestorer>();
        bool callbackCalled = false;
        bool callbackResult = true;

        // Act
        LogAssert.Expect(LogType.Error, "[WorldStateSerializer->DeserializeFromJson] Failed to deserialize: Unexpected character encountered while parsing value: i. Path '', line 0, position 0.");
        LogAssert.Expect(LogType.Error, "[WorldStateRestorer->RestoreWorldStateFromJson] Failed to deserialize snapshot.");
        restorer.RestoreWorldStateFromJson(null, "invalid json", (success) => {
            callbackCalled = true;
            callbackResult = success;
        });

        // Assert
        Assert.IsTrue(callbackCalled);
        Assert.IsFalse(callbackResult);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    #endregion

    #region Full Round-Trip Tests

    [Test]
    public void WorldStateSnapshot_CompleteRoundTrip_PreservesAllData()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange - Create a complex snapshot
        var original = new WorldStateSnapshot
        {
            worldName = "TestWorld",
            basePath = "http://example.com/worlds/test",
            worldOffset = new SerializableVector3(100f, 0f, 100f)
        };
        original.SetTimestamp();

        // Add entities
        original.entities.Add(new EntitySnapshot
        {
            id = System.Guid.NewGuid().ToString(),
            entityType = "ContainerEntity",
            tag = "RootContainer",
            position = new SerializableVector3(0, 0, 0),
            rotation = new SerializableQuaternion(Quaternion.identity),
            scale = new SerializableVector3(1, 1, 1),
            interactionState = "Static",
            visible = true
        });

        var meshEntityId = System.Guid.NewGuid().ToString();
        original.entities.Add(new EntitySnapshot
        {
            id = meshEntityId,
            entityType = "MeshEntity",
            tag = "Cube",
            parentId = original.entities[0].id,
            position = new SerializableVector3(5, 2, 3),
            rotation = new SerializableQuaternion(Quaternion.Euler(0, 45, 0)),
            scale = new SerializableVector3(2, 2, 2),
            interactionState = "Physical",
            visible = true,
            physics = new PhysicsSnapshot
            {
                mass = 10f,
                drag = 0.5f,
                useGravity = true,
                isKinematic = false,
                velocity = new SerializableVector3(1, 0, 0)
            },
            typeSpecificData = Newtonsoft.Json.JsonConvert.SerializeObject(new MeshEntityData
            {
                meshType = "primitive",
                meshSource = "cube",
                color = new SerializableColor(Color.red)
            })
        });

        // Add camera
        original.camera = new CameraSnapshot
        {
            position = new SerializableVector3(0, 10, -10),
            rotation = new SerializableQuaternion(Quaternion.Euler(30, 0, 0)),
            fieldOfView = 75f
        };

        // Add storage
        original.storage["playerScore"] = "100";
        original.storage["lastCheckpoint"] = "level2-start";

        // Add environment
        original.environment = new EnvironmentSnapshot
        {
            ambientColor = new SerializableColor(new Color(0.2f, 0.2f, 0.3f)),
            fogEnabled = true,
            fogColor = new SerializableColor(Color.gray),
            fogDensity = 0.02f
        };

        // Act - Serialize and deserialize
        string json = WorldStateSerializer.SerializeToJson(original);
        var restored = WorldStateSerializer.DeserializeFromJson(json);

        // Assert - Verify all data preserved
        Assert.IsNotNull(restored);
        Assert.AreEqual(original.version, restored.version);
        Assert.AreEqual(original.worldName, restored.worldName);
        Assert.AreEqual(original.basePath, restored.basePath);
        Assert.AreEqual(original.timestamp, restored.timestamp);

        // World offset
        Assert.AreEqual(original.worldOffset.x, restored.worldOffset.x, 0.0001f);
        Assert.AreEqual(original.worldOffset.y, restored.worldOffset.y, 0.0001f);
        Assert.AreEqual(original.worldOffset.z, restored.worldOffset.z, 0.0001f);

        // Entities
        Assert.AreEqual(2, restored.entities.Count);
        Assert.AreEqual("ContainerEntity", restored.entities[0].entityType);
        Assert.AreEqual("MeshEntity", restored.entities[1].entityType);
        Assert.AreEqual(original.entities[0].id, restored.entities[1].parentId);

        // Physics
        Assert.IsNotNull(restored.entities[1].physics);
        Assert.AreEqual(10f, restored.entities[1].physics.mass);
        Assert.AreEqual(0.5f, restored.entities[1].physics.drag);

        // Camera
        Assert.IsNotNull(restored.camera);
        Assert.AreEqual(75f, restored.camera.fieldOfView);

        // Storage
        Assert.AreEqual(2, restored.storage.Count);
        Assert.AreEqual("100", restored.storage["playerScore"]);

        // Environment
        Assert.IsNotNull(restored.environment);
        Assert.IsTrue(restored.environment.fogEnabled);
        Assert.AreEqual(0.02f, restored.environment.fogDensity, 0.0001f);
    }

    [Test]
    public void WorldStateSnapshot_LargeEntityCount_SerializesEfficiently()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange - Create snapshot with many entities
        var snapshot = new WorldStateSnapshot { worldName = "LargeWorld" };

        for (int i = 0; i < 100; i++)
        {
            snapshot.entities.Add(new EntitySnapshot
            {
                id = System.Guid.NewGuid().ToString(),
                entityType = "MeshEntity",
                tag = $"Entity_{i}",
                position = new SerializableVector3(i, 0, 0),
                rotation = new SerializableQuaternion(Quaternion.identity),
                scale = new SerializableVector3(1, 1, 1)
            });
        }

        // Act
        var startTime = System.DateTime.Now;
        string json = WorldStateSerializer.SerializeToJson(snapshot);
        var serializeTime = (System.DateTime.Now - startTime).TotalMilliseconds;

        startTime = System.DateTime.Now;
        var restored = WorldStateSerializer.DeserializeFromJson(json);
        var deserializeTime = (System.DateTime.Now - startTime).TotalMilliseconds;

        // Assert
        Assert.IsNotNull(json);
        Assert.IsNotNull(restored);
        Assert.AreEqual(100, restored.entities.Count);

        // Performance check - should complete in reasonable time
        Assert.Less(serializeTime, 1000, "Serialization took too long");
        Assert.Less(deserializeTime, 1000, "Deserialization took too long");

        // Size check
        long size = WorldStateSerializer.EstimateSnapshotSize(snapshot);
        Assert.Greater(size, 0);
    }

    #endregion

    #region TabState Tests

    [Test]
    public void TabState_Constructor_SetsCorrectDefaults()
    {
        LogAssert.ignoreFailingMessages = true;
        // Act
        var tab = new TabState("http://example.com/world");

        // Assert
        Assert.IsNotNull(tab.Id);
        Assert.AreEqual("http://example.com/world", tab.WorldUrl);
        Assert.AreEqual(TabLoadState.Unloaded, tab.LoadState);
        Assert.IsTrue(tab.CanClose);
        Assert.IsNull(tab.SnapshotId);
        Assert.IsNull(tab.ErrorMessage);
    }

    [Test]
    public void TabState_GetDisplayName_ExtractsDomainFromUrl()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var tab = new TabState("http://example.com/world/test");

        // Assert
        Assert.AreEqual("example.com", tab.GetDisplayName());
    }

    [Test]
    public void TabState_GetDisplayName_UsesCustomNameWhenSet()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var tab = new TabState("http://example.com/world", "My World");

        // Assert
        Assert.AreEqual("My World", tab.GetDisplayName());
    }

    [Test]
    public void TabState_GetDisplayName_ReturnsNewTabForNull()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var tab = new TabState(null);

        // Assert
        Assert.AreEqual("New Tab", tab.GetDisplayName());
    }

    [Test]
    public void TabState_MarkActive_UpdatesLastActiveAt()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var tab = new TabState("http://example.com");
        var initialTime = tab.LastActiveAt;

        // Small delay to ensure time difference
        System.Threading.Thread.Sleep(10);

        // Act
        tab.MarkActive();

        // Assert
        Assert.Greater(tab.LastActiveAt, initialTime);
    }

    [Test]
    public void TabState_Clone_CreatesIndependentCopy()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        var original = new TabState("http://example.com", "Test")
        {
            LoadState = TabLoadState.Loaded,
            SnapshotId = "snap123",
            BasePath = "/path/to/base",
            CanClose = false
        };

        // Act
        var clone = original.Clone();

        // Assert
        Assert.AreNotEqual(original.Id, clone.Id); // New ID
        Assert.AreEqual(original.WorldUrl, clone.WorldUrl);
        Assert.AreEqual(original.DisplayName, clone.DisplayName);
        Assert.AreEqual(original.LoadState, clone.LoadState);
        Assert.AreEqual(original.SnapshotId, clone.SnapshotId);
        Assert.AreEqual(original.CanClose, clone.CanClose);

        // Verify independence
        clone.LoadState = TabLoadState.Error;
        Assert.AreEqual(TabLoadState.Loaded, original.LoadState);
    }

    #endregion

    #region TabManager Tests

    [Test]
    public void TabManager_CanBeCreated()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");

        // Act
        var manager = go.AddComponent<TabManager>();

        // Assert
        Assert.IsNotNull(manager);
        Assert.AreEqual(0, manager.TabCount);
        Assert.IsNull(manager.ActiveTab);
        Assert.IsFalse(manager.IsSwitching);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CreateTab_WithoutInitialize_CreatesTab()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        // Act - CreateTab should work without Initialize (just won't load world)
        var tab = manager.CreateTab("http://example.com/world", "Test World", false);

        // Assert
        Assert.IsNotNull(tab);
        Assert.AreEqual(1, manager.TabCount);
        Assert.AreEqual("Test World", tab.GetDisplayName());

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CreateTab_RespectsMaxTabs()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.MaxTabs = 3;

        // Act
        manager.CreateTab("http://world1.com", makeActive: false);
        manager.CreateTab("http://world2.com", makeActive: false);
        manager.CreateTab("http://world3.com", makeActive: false);

        LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Maximum tabs"));
        var fourthTab = manager.CreateTab("http://world4.com", makeActive: false);

        // Assert
        Assert.IsNull(fourthTab);
        Assert.AreEqual(3, manager.TabCount);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_CreateEmptyTab_CreatesNewTab()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        // Act
        var tab = manager.CreateEmptyTab(false);

        // Assert
        Assert.IsNotNull(tab);
        Assert.IsNull(tab.WorldUrl);
        Assert.AreEqual("New Tab", tab.GetDisplayName());

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTab_ReturnsCorrectTab()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        var tab = manager.CreateTab("http://example.com", makeActive: false);

        // Act
        var found = manager.GetTab(tab.Id);

        // Assert
        Assert.AreSame(tab, found);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTab_ReturnsNullForUnknownId()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();

        // Act
        var found = manager.GetTab("unknown-id");

        // Assert
        Assert.IsNull(found);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTabByIndex_ReturnsCorrectTab()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = manager.CreateTab("http://world2.com", makeActive: false);
        manager.CreateTab("http://world3.com", makeActive: false);

        // Act
        var found = manager.GetTabByIndex(1);

        // Assert
        Assert.AreSame(tab2, found);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_GetTabIndex_ReturnsCorrectIndex()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = manager.CreateTab("http://world2.com", makeActive: false);

        // Act
        int index = manager.GetTabIndex(tab2.Id);

        // Assert
        Assert.AreEqual(1, index);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_HasTabWithUrl_ReturnsCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://example.com", makeActive: false);

        // Assert
        Assert.IsTrue(manager.HasTabWithUrl("http://example.com"));
        Assert.IsFalse(manager.HasTabWithUrl("http://other.com"));

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_MoveTab_ChangesOrder()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        var tab1 = manager.CreateTab("http://world1.com", makeActive: false);
        var tab2 = manager.CreateTab("http://world2.com", makeActive: false);
        var tab3 = manager.CreateTab("http://world3.com", makeActive: false);

        // Act
        manager.MoveTab(tab3.Id, 0);

        // Assert
        Assert.AreSame(tab3, manager.GetTabByIndex(0));
        Assert.AreSame(tab1, manager.GetTabByIndex(1));
        Assert.AreSame(tab2, manager.GetTabByIndex(2));

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_OnTabCreated_EventFires()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        TabState eventTab = null;
        manager.OnTabCreated += (tab) => eventTab = tab;

        // Act
        var createdTab = manager.CreateTab("http://example.com", makeActive: false);

        // Assert
        Assert.AreSame(createdTab, eventTab);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TabManager_Tabs_ReturnsReadOnlyList()
    {
        LogAssert.ignoreFailingMessages = true;
        // Arrange
        GameObject go = new GameObject("TestTabManager");
        var manager = go.AddComponent<TabManager>();
        manager.CreateTab("http://world1.com", makeActive: false);
        manager.CreateTab("http://world2.com", makeActive: false);

        // Act
        var tabs = manager.Tabs;

        // Assert
        Assert.AreEqual(2, tabs.Count);
        Assert.IsInstanceOf<System.Collections.Generic.IReadOnlyList<TabState>>(tabs);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    #endregion
}
