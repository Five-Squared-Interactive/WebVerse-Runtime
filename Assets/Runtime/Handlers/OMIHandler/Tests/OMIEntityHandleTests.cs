// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.OMI;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;

/// <summary>
/// Unit tests for OMIEntityHandle and OMIHandlerSettings.
/// </summary>
public class OMIEntityHandleTests
{
    // ── OMIEntityHandle default constructor ──

    [Test]
    public void DefaultConstructor_Id_IsNotEmpty()
    {
        LogAssert.ignoreFailingMessages = true;

        var handle = new OMIEntityHandle();

        Assert.AreNotEqual(Guid.Empty, handle.Id);
    }

    [Test]
    public void DefaultConstructor_Success_IsFalse()
    {
        LogAssert.ignoreFailingMessages = true;

        var handle = new OMIEntityHandle();

        Assert.IsFalse(handle.Success);
    }

    [Test]
    public void DefaultConstructor_NodeIndex_IsNegativeOne()
    {
        LogAssert.ignoreFailingMessages = true;

        var handle = new OMIEntityHandle();

        Assert.AreEqual(-1, handle.NodeIndex);
    }

    [Test]
    public void DefaultConstructor_Tag_IsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        var handle = new OMIEntityHandle();

        Assert.IsNull(handle.Tag);
    }

    // ── OMIEntityHandle constructor with Id ──

    [Test]
    public void GuidConstructor_SetsId()
    {
        LogAssert.ignoreFailingMessages = true;

        var expected = Guid.NewGuid();
        var handle = new OMIEntityHandle(expected);

        Assert.AreEqual(expected, handle.Id);
    }

    [Test]
    public void GuidConstructor_EmptyGuid_SetsIdToEmpty()
    {
        LogAssert.ignoreFailingMessages = true;

        var handle = new OMIEntityHandle(Guid.Empty);

        Assert.AreEqual(Guid.Empty, handle.Id);
    }

    [Test]
    public void GuidConstructor_Success_IsFalse()
    {
        LogAssert.ignoreFailingMessages = true;

        var handle = new OMIEntityHandle(Guid.NewGuid());

        Assert.IsFalse(handle.Success);
    }

    // ── Property setters ──

    [Test]
    public void Tag_CanBeSet()
    {
        LogAssert.ignoreFailingMessages = true;

        var handle = new OMIEntityHandle();
        handle.Tag = "TestNode";

        Assert.AreEqual("TestNode", handle.Tag);
    }

    [Test]
    public void Success_CanBeSetToTrue()
    {
        LogAssert.ignoreFailingMessages = true;

        var handle = new OMIEntityHandle();
        handle.Success = true;

        Assert.IsTrue(handle.Success);
    }

    [Test]
    public void NodeIndex_CanBeSet()
    {
        LogAssert.ignoreFailingMessages = true;

        var handle = new OMIEntityHandle();
        handle.NodeIndex = 42;

        Assert.AreEqual(42, handle.NodeIndex);
    }

    // ── Multiple instances are independent ──

    [Test]
    public void MultipleInstances_HaveUniqueIds()
    {
        LogAssert.ignoreFailingMessages = true;

        var a = new OMIEntityHandle();
        var b = new OMIEntityHandle();

        Assert.AreNotEqual(a.Id, b.Id);
    }

    [Test]
    public void MultipleInstances_HaveIndependentState()
    {
        LogAssert.ignoreFailingMessages = true;

        var a = new OMIEntityHandle();
        var b = new OMIEntityHandle();

        a.Tag = "First";
        a.Success = true;
        a.NodeIndex = 1;

        b.Tag = "Second";
        b.NodeIndex = 2;

        Assert.AreEqual("First", a.Tag);
        Assert.IsTrue(a.Success);
        Assert.AreEqual(1, a.NodeIndex);

        Assert.AreEqual("Second", b.Tag);
        Assert.IsFalse(b.Success);
        Assert.AreEqual(2, b.NodeIndex);
    }

    // ── OMIHandlerSettings defaults ──

    [Test]
    public void Settings_DefaultConstructor_EnableFlagsAreTrue()
    {
        LogAssert.ignoreFailingMessages = true;

        var settings = new OMIHandlerSettings();

        Assert.IsTrue(settings.enablePhysics);
        Assert.IsTrue(settings.enableSpawnPoints);
        Assert.IsTrue(settings.enableSeats);
        Assert.IsTrue(settings.enableLinks);
    }

    [Test]
    public void Settings_DefaultConstructor_SpawnModeIsFirst()
    {
        LogAssert.ignoreFailingMessages = true;

        var settings = new OMIHandlerSettings();

        Assert.AreEqual(SpawnPointSelectionMode.First, settings.spawnMode);
        Assert.IsTrue(settings.autoApplySpawnPoint);
        Assert.AreEqual("", settings.spawnTeam);
        Assert.AreEqual("", settings.spawnPointName);
    }

    [Test]
    public void Settings_DefaultConstructor_LinkSecurityDefaults()
    {
        LogAssert.ignoreFailingMessages = true;

        var settings = new OMIHandlerSettings();

        Assert.IsTrue(settings.allowExternalLinks);
        Assert.IsTrue(settings.allowWorldLinks);
    }

    [Test]
    public void Settings_DefaultConstructor_VerboseLoggingIsFalse()
    {
        LogAssert.ignoreFailingMessages = true;

        var settings = new OMIHandlerSettings();

        Assert.IsFalse(settings.verboseLogging);
        Assert.AreEqual(-1, settings.physicsLayer);
        Assert.IsNull(settings.defaultPhysicsMaterial);
    }

    [Test]
    public void Settings_CreateDefault_ReturnsSameAsConstructor()
    {
        LogAssert.ignoreFailingMessages = true;

        var settings = OMIHandlerSettings.CreateDefault();

        Assert.IsNotNull(settings);
        Assert.IsTrue(settings.enablePhysics);
        Assert.AreEqual(SpawnPointSelectionMode.First, settings.spawnMode);
        Assert.IsFalse(settings.verboseLogging);
    }

    // ── OMILinkBehavior (MonoBehaviour) ──

    [UnityTest]
    public IEnumerator LinkBehavior_Initialize_SetsUrlAndTitle()
    {
        LogAssert.ignoreFailingMessages = true;

        var go = new GameObject("LinkTest");
        var link = go.AddComponent<OMILinkBehavior>();

        link.Initialize("https://example.com", "Example");

        Assert.AreEqual("https://example.com", link.Url);
        Assert.AreEqual("Example", link.Title);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LinkBehavior_ShowTooltip_DefaultsToTrue()
    {
        LogAssert.ignoreFailingMessages = true;

        var go = new GameObject("LinkTooltipTest");
        var link = go.AddComponent<OMILinkBehavior>();

        Assert.IsTrue(link.ShowTooltip);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    // ── OMISeatBehavior (MonoBehaviour) ──

    [UnityTest]
    public IEnumerator SeatBehavior_IsOccupied_DefaultsToFalse()
    {
        LogAssert.ignoreFailingMessages = true;

        var go = new GameObject("SeatTest");
        var seat = go.AddComponent<OMISeatBehavior>();

        Assert.IsFalse(seat.IsOccupied);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_SeatRotation_DefaultsToIdentity()
    {
        LogAssert.ignoreFailingMessages = true;

        var go = new GameObject("SeatRotTest");
        var seat = go.AddComponent<OMISeatBehavior>();

        Assert.AreEqual(Quaternion.identity, seat.SeatRotation);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    // ── OMISeatBehavior expanded tests ──

    [UnityTest]
    public IEnumerator SeatBehavior_Initialize_WithOffsets_SetsValues()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatInitTest");
        var seat = go.AddComponent<OMISeatBehavior>();

        Vector3 backOff = new Vector3(0f, 0.5f, -0.2f);
        Vector3 footOff = new Vector3(0f, -0.5f, 0.3f);
        seat.Initialize(backOff, footOff, 0f);

        Assert.AreEqual(backOff, seat.BackOffset);
        Assert.AreEqual(footOff, seat.FootOffset);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_Initialize_WithAngle_SetsSeatRotation()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatAngleTest");
        var seat = go.AddComponent<OMISeatBehavior>();

        float angleRad = Mathf.PI / 2f; // 90 degrees
        seat.Initialize(Vector3.zero, Vector3.zero, angleRad);

        float expectedY = angleRad * Mathf.Rad2Deg;
        Assert.AreEqual(expectedY, seat.SeatRotation.eulerAngles.y, 0.01f);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_BackOffset_DefaultsToZero()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatBackTest");
        var seat = go.AddComponent<OMISeatBehavior>();

        Assert.AreEqual(Vector3.zero, seat.BackOffset);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_FootOffset_DefaultsToZero()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatFootTest");
        var seat = go.AddComponent<OMISeatBehavior>();

        Assert.AreEqual(Vector3.zero, seat.FootOffset);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_GetSeatPosition_ReturnsTransformPosition()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatPosTest");
        go.transform.position = new Vector3(5f, 1f, 3f);
        var seat = go.AddComponent<OMISeatBehavior>();

        Assert.AreEqual(new Vector3(5f, 1f, 3f), seat.GetSeatPosition());

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_GetSeatRotation_CombinesTransformAndSeatRotation()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatRotCombineTest");
        var seat = go.AddComponent<OMISeatBehavior>();

        // Default SeatRotation is identity, so result should equal transform.rotation
        Quaternion result = seat.GetSeatRotation();
        Assert.AreEqual(go.transform.rotation.x, result.x, 0.001f);
        Assert.AreEqual(go.transform.rotation.y, result.y, 0.001f);
        Assert.AreEqual(go.transform.rotation.z, result.z, 0.001f);
        Assert.AreEqual(go.transform.rotation.w, result.w, 0.001f);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_GetBackIKPosition_TransformsOffset()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatBackIKTest");
        go.transform.position = new Vector3(10f, 0f, 0f);
        var seat = go.AddComponent<OMISeatBehavior>();
        seat.BackOffset = new Vector3(0f, 1f, 0f);

        Vector3 backIK = seat.GetBackIKPosition();
        Assert.AreEqual(10f, backIK.x, 0.01f);
        Assert.AreEqual(1f, backIK.y, 0.01f);
        Assert.AreEqual(0f, backIK.z, 0.01f);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_GetFootIKPosition_TransformsOffset()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatFootIKTest");
        go.transform.position = new Vector3(10f, 0f, 0f);
        var seat = go.AddComponent<OMISeatBehavior>();
        seat.FootOffset = new Vector3(0f, -0.5f, 0.3f);

        Vector3 footIK = seat.GetFootIKPosition();
        Assert.AreEqual(10f, footIK.x, 0.01f);
        Assert.AreEqual(-0.5f, footIK.y, 0.01f);
        Assert.AreEqual(0.3f, footIK.z, 0.01f);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_Stand_WhenNotOccupied_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatStandTest");
        var seat = go.AddComponent<OMISeatBehavior>();

        Assert.IsFalse(seat.Stand());

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SeatBehavior_Sit_NullCharacter_ReturnsFalse()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("SeatSitNullTest");
        var seat = go.AddComponent<OMISeatBehavior>();

        Assert.IsFalse(seat.Sit(null));
        Assert.IsFalse(seat.IsOccupied);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    // ── OMILinkBehavior expanded tests ──

    [UnityTest]
    public IEnumerator LinkBehavior_GetDisplayName_ReturnsTitle_WhenSet()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("LinkDisplayNameTest");
        var link = go.AddComponent<OMILinkBehavior>();
        link.Initialize("https://example.com", "My Link");

        Assert.AreEqual("My Link", link.GetDisplayName());

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LinkBehavior_GetDisplayName_ReturnsUrl_WhenTitleNull()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("LinkDisplayNameUrlTest");
        var link = go.AddComponent<OMILinkBehavior>();
        link.Initialize("https://example.com");

        Assert.AreEqual("https://example.com", link.GetDisplayName());

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LinkBehavior_GetDisplayName_ReturnsUrl_WhenTitleEmpty()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("LinkDisplayNameEmptyTest");
        var link = go.AddComponent<OMILinkBehavior>();
        link.Initialize("https://example.com", "");

        Assert.AreEqual("https://example.com", link.GetDisplayName());

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LinkBehavior_Activate_EmptyUrl_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("LinkActivateEmptyTest");
        var link = go.AddComponent<OMILinkBehavior>();
        link.Initialize("");

        link.Activate();

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LinkBehavior_Activate_NullUrl_NoException()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("LinkActivateNullTest");
        var link = go.AddComponent<OMILinkBehavior>();
        link.Url = null;

        link.Activate();

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LinkBehavior_ShowTooltip_CanBeDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("LinkTooltipDisableTest");
        var link = go.AddComponent<OMILinkBehavior>();
        link.ShowTooltip = false;

        Assert.IsFalse(link.ShowTooltip);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LinkBehavior_Initialize_OverwritesPreviousValues()
    {
        LogAssert.ignoreFailingMessages = true;
        var go = new GameObject("LinkReinitTest");
        var link = go.AddComponent<OMILinkBehavior>();
        link.Initialize("https://first.com", "First");
        link.Initialize("https://second.com", "Second");

        Assert.AreEqual("https://second.com", link.Url);
        Assert.AreEqual("Second", link.Title);

        UnityEngine.Object.DestroyImmediate(go);
        yield return null;
    }

    // ── OMIHandlerSettings expanded tests ──

    [Test]
    public void Settings_EnablePhysics_CanBeDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.enablePhysics = false;
        Assert.IsFalse(settings.enablePhysics);
    }

    [Test]
    public void Settings_EnableSpawnPoints_CanBeDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.enableSpawnPoints = false;
        Assert.IsFalse(settings.enableSpawnPoints);
    }

    [Test]
    public void Settings_EnableSeats_CanBeDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.enableSeats = false;
        Assert.IsFalse(settings.enableSeats);
    }

    [Test]
    public void Settings_EnableLinks_CanBeDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.enableLinks = false;
        Assert.IsFalse(settings.enableLinks);
    }

    [Test]
    public void Settings_SpawnMode_CanBeSetToRandom()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.spawnMode = SpawnPointSelectionMode.Random;
        Assert.AreEqual(SpawnPointSelectionMode.Random, settings.spawnMode);
    }

    [Test]
    public void Settings_SpawnMode_CanBeSetToTeamBased()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.spawnMode = SpawnPointSelectionMode.TeamBased;
        Assert.AreEqual(SpawnPointSelectionMode.TeamBased, settings.spawnMode);
    }

    [Test]
    public void Settings_SpawnMode_CanBeSetToNamed()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.spawnMode = SpawnPointSelectionMode.Named;
        Assert.AreEqual(SpawnPointSelectionMode.Named, settings.spawnMode);
    }

    [Test]
    public void Settings_AllowExternalLinks_CanBeDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.allowExternalLinks = false;
        Assert.IsFalse(settings.allowExternalLinks);
    }

    [Test]
    public void Settings_AllowWorldLinks_CanBeDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.allowWorldLinks = false;
        Assert.IsFalse(settings.allowWorldLinks);
    }

    [Test]
    public void Settings_VerboseLogging_CanBeEnabled()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.verboseLogging = true;
        Assert.IsTrue(settings.verboseLogging);
    }

    [Test]
    public void Settings_PhysicsLayer_CanBeSet()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.physicsLayer = 8;
        Assert.AreEqual(8, settings.physicsLayer);
    }

    [Test]
    public void Settings_SpawnTeam_CanBeSet()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.spawnTeam = "RedTeam";
        Assert.AreEqual("RedTeam", settings.spawnTeam);
    }

    [Test]
    public void Settings_SpawnPointName_CanBeSet()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.spawnPointName = "LobbySpawn";
        Assert.AreEqual("LobbySpawn", settings.spawnPointName);
    }

    [Test]
    public void Settings_AutoApplySpawnPoint_CanBeDisabled()
    {
        LogAssert.ignoreFailingMessages = true;
        var settings = new OMIHandlerSettings();
        settings.autoApplySpawnPoint = false;
        Assert.IsFalse(settings.autoApplySpawnPoint);
    }

    // ── SpawnPointSelectionMode enum ──

    [Test]
    public void SpawnPointSelectionMode_First_HasValue0()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(0, (int)SpawnPointSelectionMode.First);
    }

    [Test]
    public void SpawnPointSelectionMode_Random_HasValue1()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(1, (int)SpawnPointSelectionMode.Random);
    }

    [Test]
    public void SpawnPointSelectionMode_TeamBased_HasValue2()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(2, (int)SpawnPointSelectionMode.TeamBased);
    }

    [Test]
    public void SpawnPointSelectionMode_Named_HasValue3()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(3, (int)SpawnPointSelectionMode.Named);
    }

    [Test]
    public void SpawnPointSelectionMode_AllValues_CountIs4()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(SpawnPointSelectionMode));
        Assert.AreEqual(4, values.Length);
    }

    // ── SpawnPointData ──

    [Test]
    public void SpawnPointData_Properties_CanBeSet()
    {
        LogAssert.ignoreFailingMessages = true;
        var data = new SpawnPointData();
        data.Position = new Vector3(10f, 0f, 20f);
        data.Rotation = Quaternion.Euler(0f, 90f, 0f);
        data.Title = "MainSpawn";
        data.Team = "Blue";
        data.Group = "Lobby";

        Assert.AreEqual(10f, data.Position.x);
        Assert.AreEqual(20f, data.Position.z);
        Assert.AreEqual("MainSpawn", data.Title);
        Assert.AreEqual("Blue", data.Team);
        Assert.AreEqual("Lobby", data.Group);
    }

    [Test]
    public void SpawnPointData_DefaultConstructor_StringsAreNull()
    {
        LogAssert.ignoreFailingMessages = true;
        var data = new SpawnPointData();

        Assert.IsNull(data.Title);
        Assert.IsNull(data.Team);
        Assert.IsNull(data.Group);
    }

    [Test]
    public void SpawnPointData_DefaultConstructor_PositionIsZero()
    {
        LogAssert.ignoreFailingMessages = true;
        var data = new SpawnPointData();

        Assert.AreEqual(Vector3.zero, data.Position);
    }

    // ── StraightFourCustomDataKeys ──

    [Test]
    public void CustomDataKeys_Prefix_IsSF()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual("SF_", StraightFourCustomDataKeys.Prefix);
    }

    [Test]
    public void CustomDataKeys_NodeToEntity_StartsWithPrefix()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsTrue(StraightFourCustomDataKeys.NodeToEntity.StartsWith("SF_"));
        Assert.AreEqual("SF_NodeToEntity", StraightFourCustomDataKeys.NodeToEntity);
    }

    [Test]
    public void CustomDataKeys_PhysicsShapes_StartsWithPrefix()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual("SF_PhysicsShapes", StraightFourCustomDataKeys.PhysicsShapes);
    }

    [Test]
    public void CustomDataKeys_PhysicsMaterials_StartsWithPrefix()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual("SF_PhysicsMaterials", StraightFourCustomDataKeys.PhysicsMaterials);
    }

    [Test]
    public void CustomDataKeys_CollisionFilters_StartsWithPrefix()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual("SF_CollisionFilters", StraightFourCustomDataKeys.CollisionFilters);
    }

    [Test]
    public void CustomDataKeys_JointSettings_StartsWithPrefix()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual("SF_JointSettings", StraightFourCustomDataKeys.JointSettings);
    }

    [Test]
    public void CustomDataKeys_WorldGravity_StartsWithPrefix()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual("SF_WorldGravity", StraightFourCustomDataKeys.WorldGravity);
    }

    [Test]
    public void CustomDataKeys_AudioData_StartsWithPrefix()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual("SF_AudioData", StraightFourCustomDataKeys.AudioData);
    }

    [Test]
    public void CustomDataKeys_Runtime_StartsWithPrefix()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual("SF_Runtime", StraightFourCustomDataKeys.Runtime);
    }

    [Test]
    public void CustomDataKeys_SpawnPointRegistry_StartsWithPrefix()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual("SF_SpawnPointRegistry", StraightFourCustomDataKeys.SpawnPointRegistry);
    }

    [Test]
    public void CustomDataKeys_AllKeysUnique()
    {
        LogAssert.ignoreFailingMessages = true;
        var keys = new HashSet<string>
        {
            StraightFourCustomDataKeys.NodeToEntity,
            StraightFourCustomDataKeys.PhysicsShapes,
            StraightFourCustomDataKeys.PhysicsMaterials,
            StraightFourCustomDataKeys.CollisionFilters,
            StraightFourCustomDataKeys.JointSettings,
            StraightFourCustomDataKeys.WorldGravity,
            StraightFourCustomDataKeys.Skies,
            StraightFourCustomDataKeys.AudioData,
            StraightFourCustomDataKeys.AudioSources,
            StraightFourCustomDataKeys.LoadedAudioClips,
            StraightFourCustomDataKeys.WheelSettings,
            StraightFourCustomDataKeys.Runtime,
            StraightFourCustomDataKeys.SpawnPointRegistry
        };
        Assert.AreEqual(13, keys.Count);
    }
}
