// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FiveSQD.WebVerse.Handlers.OMI;

/// <summary>
/// Unit tests for SpawnPointRegistry.
/// </summary>
public class SpawnPointRegistryTests
{
    private SpawnPointRegistry _registry;

    [SetUp]
    public void SetUp()
    {
        _registry = new SpawnPointRegistry();
    }

    [Test]
    public void Count_Initially_IsZero()
    {
        Assert.AreEqual(0, _registry.Count);
    }

    [Test]
    public void SpawnPoints_Initially_IsEmpty()
    {
        Assert.IsNotNull(_registry.SpawnPoints);
        Assert.AreEqual(0, _registry.SpawnPoints.Count);
    }

    [Test]
    public void Register_SingleSpawnPoint_CountIsOne()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Spawn1");

        Assert.AreEqual(1, _registry.Count);
    }

    [Test]
    public void Register_MultipleSpawnPoints_CountIsCorrect()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Spawn1");
        _registry.Register(Vector3.one, Quaternion.identity, "Spawn2");
        _registry.Register(Vector3.up, Quaternion.identity, "Spawn3");

        Assert.AreEqual(3, _registry.Count);
    }

    [Test]
    public void SpawnPoints_AfterRegister_ContainsRegisteredPoints()
    {
        _registry.Register(new Vector3(1, 2, 3), Quaternion.identity, "A");
        _registry.Register(new Vector3(4, 5, 6), Quaternion.identity, "B");

        Assert.AreEqual(2, _registry.SpawnPoints.Count);
        Assert.AreEqual("A", _registry.SpawnPoints[0].Title);
        Assert.AreEqual("B", _registry.SpawnPoints[1].Title);
        Assert.AreEqual(new Vector3(1, 2, 3), _registry.SpawnPoints[0].Position);
    }

    [Test]
    public void Clear_RemovesAllSpawnPoints()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Spawn1");
        _registry.Register(Vector3.one, Quaternion.identity, "Spawn2");

        _registry.Clear();

        Assert.AreEqual(0, _registry.Count);
        Assert.AreEqual(0, _registry.SpawnPoints.Count);
    }

    [Test]
    public void GetSpawnPoint_Random_ReturnsARegisteredPoint()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "A");
        _registry.Register(Vector3.one, Quaternion.identity, "B");

        var result = _registry.GetSpawnPoint(SpawnPointSelectionMode.Random);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Title == "A" || result.Title == "B",
            "Random spawn point should be one of the registered points");
    }

    [Test]
    public void GetSpawnPoint_Random_WithNoPoints_ReturnsNull()
    {
        var result = _registry.GetSpawnPoint(SpawnPointSelectionMode.Random);

        Assert.IsNull(result);
    }

    [Test]
    public void GetSpawnPointByTeam_ReturnsCorrectSubset()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "RedSpawn1", team: "Red");
        _registry.Register(Vector3.one, Quaternion.identity, "BlueSpawn1", team: "Blue");
        _registry.Register(Vector3.up, Quaternion.identity, "RedSpawn2", team: "Red");

        var result = _registry.GetSpawnPointByTeam("Red");

        Assert.IsNotNull(result);
        Assert.AreEqual("Red", result.Team);
    }

    [Test]
    public void GetSpawnPointByTeam_NoMatches_ReturnsFirstAvailable()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Spawn1", team: "Red");

        var result = _registry.GetSpawnPointByTeam("Green");

        Assert.IsNotNull(result);
        Assert.AreEqual("Spawn1", result.Title,
            "When no team match, should return first available spawn point");
    }

    [Test]
    public void GetSpawnPointsByGroup_ReturnsCorrectSubset()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Spawn1", group: "North");
        _registry.Register(Vector3.one, Quaternion.identity, "Spawn2", group: "South");
        _registry.Register(Vector3.up, Quaternion.identity, "Spawn3", group: "North");

        var results = _registry.GetSpawnPointsByGroup("North");

        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.TrueForAll(sp => sp.Group == "North"));
    }

    [Test]
    public void GetSpawnPointsByGroup_NoMatches_ReturnsEmptyList()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Spawn1", group: "North");

        var results = _registry.GetSpawnPointsByGroup("West");

        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count);
    }

    [Test]
    public void Register_DuplicatePoint_BothAreStored()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Same");
        _registry.Register(Vector3.zero, Quaternion.identity, "Same");

        Assert.AreEqual(2, _registry.Count);
    }

    [Test]
    public void ClearThenRegister_Works()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Before");
        _registry.Clear();
        _registry.Register(Vector3.one, Quaternion.identity, "After");

        Assert.AreEqual(1, _registry.Count);
        Assert.AreEqual("After", _registry.SpawnPoints[0].Title);
    }

    [Test]
    public void GetSpawnPoint_Random_AfterClear_ReturnsNull()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Spawn1");
        _registry.Clear();

        var result = _registry.GetSpawnPoint(SpawnPointSelectionMode.Random);

        Assert.IsNull(result);
    }

    [Test]
    public void GetSpawnPoint_First_ReturnsFirstRegistered()
    {
        _registry.Register(new Vector3(1, 0, 0), Quaternion.identity, "First");
        _registry.Register(new Vector3(2, 0, 0), Quaternion.identity, "Second");

        var result = _registry.GetSpawnPoint(SpawnPointSelectionMode.First);

        Assert.IsNotNull(result);
        Assert.AreEqual("First", result.Title);
    }

    [Test]
    public void GetSpawnPointByName_ReturnsMatchingPoint()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Lobby");
        _registry.Register(Vector3.one, Quaternion.identity, "Arena");

        var result = _registry.GetSpawnPointByName("Arena");

        Assert.IsNotNull(result);
        Assert.AreEqual("Arena", result.Title);
        Assert.AreEqual(Vector3.one, result.Position);
    }

    [Test]
    public void GetSpawnPointByTeam_CaseInsensitive_ReturnsMatch()
    {
        _registry.Register(Vector3.zero, Quaternion.identity, "Spawn1", team: "Red");

        var result = _registry.GetSpawnPointByTeam("red");

        Assert.IsNotNull(result);
        Assert.AreEqual("Red", result.Team);
    }
}
