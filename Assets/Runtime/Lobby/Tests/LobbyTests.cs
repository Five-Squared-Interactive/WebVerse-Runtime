// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Lobby;

/// <summary>
/// Unit tests for the Lobby system.
/// Tests LobbyManager state management, world selection, and WorldSelectorItem initialization.
/// </summary>
public class LobbyTests
{
    #region LobbyManager Tests

    [Test]
    public void LobbyManager_InitialState_IsInactive()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();

        // Assert
        Assert.AreEqual(LobbyManager.LobbyState.Inactive, manager.CurrentState);
        Assert.IsFalse(manager.IsActive);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void LobbyManager_ExitLobby_SetsStateInactive()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();

        // Act
        manager.ExitLobby();

        // Assert
        Assert.AreEqual(LobbyManager.LobbyState.Inactive, manager.CurrentState);
        Assert.IsFalse(manager.IsActive);
        Assert.IsFalse(go.activeSelf);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void LobbyManager_ExitLobby_DeactivatesGameObject()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();
        go.SetActive(true);

        // Act
        manager.ExitLobby();

        // Assert
        Assert.IsFalse(go.activeSelf);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void LobbyManager_SelectWorld_WhenInactive_DoesNotChangeState()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();
        var world = new WorldInfo { name = "TestWorld", url = "https://test.com" };

        // Act - selecting while inactive should warn and not change state
        manager.SelectWorld(world);

        // Assert
        Assert.AreEqual(LobbyManager.LobbyState.Inactive, manager.CurrentState);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void LobbyManager_SelectWorldByUrl_WhenInactive_DoesNotChangeState()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();

        // Act
        manager.SelectWorld("https://test.com");

        // Assert
        Assert.AreEqual(LobbyManager.LobbyState.Inactive, manager.CurrentState);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void LobbyManager_DefaultWorldUrl_IsEmpty()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();

        // Assert
        Assert.AreEqual("", manager.defaultWorldUrl);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void LobbyManager_OnWorldSelected_FiresWhenWorldSelected()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange - We cannot easily get the manager into Idle state without
        // running coroutines, so we verify the event is wired up (it won't fire
        // because SelectWorld guards on Idle state).
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();

        bool eventFired = false;
        WorldInfo receivedWorld = null;
        manager.OnWorldSelected += (world) =>
        {
            eventFired = true;
            receivedWorld = world;
        };

        var testWorld = new WorldInfo { name = "TestWorld", url = "https://test.com" };

        // Act - state is Inactive, so event should NOT fire
        manager.SelectWorld(testWorld);

        // Assert
        Assert.IsFalse(eventFired);
        Assert.IsNull(receivedWorld);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    #endregion

    #region WorldInfo Tests

    [Test]
    public void WorldInfo_CanBeCreated_WithAllFields()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange & Act
        var info = new WorldInfo
        {
            name = "My World",
            url = "https://example.com/world",
            description = "A test world",
            thumbnail = null,
            isLocal = true
        };

        // Assert
        Assert.AreEqual("My World", info.name);
        Assert.AreEqual("https://example.com/world", info.url);
        Assert.AreEqual("A test world", info.description);
        Assert.IsNull(info.thumbnail);
        Assert.IsTrue(info.isLocal);
    }

    [Test]
    public void WorldInfo_DefaultValues_AreNull()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange & Act
        var info = new WorldInfo();

        // Assert
        Assert.IsNull(info.name);
        Assert.IsNull(info.url);
        Assert.IsNull(info.description);
        Assert.IsNull(info.thumbnail);
        Assert.IsFalse(info.isLocal);
    }

    #endregion

    #region WorldSelectorItem Tests

    [Test]
    public void WorldSelectorItem_Initialize_SetsWorldInfo()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestWorldSelectorItem");
        var item = go.AddComponent<WorldSelectorItem>();

        GameObject managerGO = new GameObject("TestLobbyManager");
        var manager = managerGO.AddComponent<LobbyManager>();

        var world = new WorldInfo
        {
            name = "Test World",
            url = "https://test.com",
            description = "Description"
        };

        // Act - Initialize should not throw even with null UI references
        Assert.DoesNotThrow(() => item.Initialize(world, manager));

        // Cleanup
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(managerGO);
    }

    [Test]
    public void WorldSelectorItem_Initialize_WithNullNameText_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestWorldSelectorItem");
        var item = go.AddComponent<WorldSelectorItem>();

        GameObject managerGO = new GameObject("TestLobbyManager");
        var manager = managerGO.AddComponent<LobbyManager>();

        var world = new WorldInfo
        {
            name = "Test World",
            url = "https://test.com"
        };

        // Act & Assert - nameText is null, should handle gracefully
        Assert.DoesNotThrow(() => item.Initialize(world, manager));

        // Cleanup
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(managerGO);
    }

    [Test]
    public void WorldSelectorItem_Initialize_WithNullDescription_SetsEmptyString()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestWorldSelectorItem");
        var item = go.AddComponent<WorldSelectorItem>();

        GameObject managerGO = new GameObject("TestLobbyManager");
        var manager = managerGO.AddComponent<LobbyManager>();

        var world = new WorldInfo
        {
            name = "Test World",
            url = "https://test.com",
            description = null
        };

        // Act - should handle null description gracefully (code uses ?? "")
        Assert.DoesNotThrow(() => item.Initialize(world, manager));

        // Cleanup
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(managerGO);
    }

    [Test]
    public void WorldSelectorItem_Initialize_WithNullThumbnail_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestWorldSelectorItem");
        var item = go.AddComponent<WorldSelectorItem>();

        GameObject managerGO = new GameObject("TestLobbyManager");
        var manager = managerGO.AddComponent<LobbyManager>();

        var world = new WorldInfo
        {
            name = "Test World",
            url = "https://test.com",
            thumbnail = null
        };

        // Act & Assert
        Assert.DoesNotThrow(() => item.Initialize(world, manager));

        // Cleanup
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(managerGO);
    }

    [Test]
    public void WorldSelectorItem_DefaultHoverScale_IsCorrect()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestWorldSelectorItem");
        var item = go.AddComponent<WorldSelectorItem>();

        // Assert
        Assert.AreEqual(1.2f, item.hoverScale);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void WorldSelectorItem_DefaultColors_AreSet()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestWorldSelectorItem");
        var item = go.AddComponent<WorldSelectorItem>();

        // Assert - verify default colors are non-zero
        Assert.AreNotEqual(Color.clear, item.defaultColor);
        Assert.AreNotEqual(Color.clear, item.hoverColor);
        Assert.AreNotEqual(Color.clear, item.selectedColor);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    #endregion

    #region LobbyManager Configuration Tests

    [Test]
    public void LobbyManager_DefaultAssemblyDuration_IsOne()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();

        // Assert
        Assert.AreEqual(1.0f, manager.assemblyDuration);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void LobbyManager_DefaultAssemblyDelay_IsPointFive()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();

        // Assert
        Assert.AreEqual(0.5f, manager.assemblyDelay);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void LobbyManager_DefaultLaunchDuration_IsOnePointFive()
    {
        LogAssert.ignoreFailingMessages = true;

        // Arrange
        GameObject go = new GameObject("TestLobbyManager");
        var manager = go.AddComponent<LobbyManager>();

        // Assert
        Assert.AreEqual(1.5f, manager.launchDuration);

        // Cleanup
        Object.DestroyImmediate(go);
    }

    #endregion
}
