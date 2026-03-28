// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Voice;

/// <summary>
/// Unit tests for Voice JavaScript API.
/// Note: These tests verify API surface without WebVerseRuntime context.
/// Full integration tests require a running WebVerseRuntime.
/// </summary>
public class VoiceAPITests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        // Clear any previously registered callbacks
        Voice.ClearCallbacks();
    }

    [TearDown]
    public void TearDown()
    {
        Voice.ClearCallbacks();
    }

    #region Story 6.1: Connect/Disconnect API Tests

    [Test]
    public void Connect_WithoutRuntime_ReturnsFalse()
    {
        // Without WebVerseRuntime, connect should fail gracefully
        bool result = Voice.Connect();
        Assert.IsFalse(result);
    }

    [Test]
    public void Disconnect_WithoutRuntime_ReturnsFalse()
    {
        bool result = Voice.Disconnect();
        Assert.IsFalse(result);
    }

    [Test]
    public void IsEnabled_WithoutRuntime_ReturnsFalse()
    {
        bool result = Voice.IsEnabled();
        Assert.IsFalse(result);
    }

    [Test]
    public void GetState_WithoutRuntime_ReturnsDisconnected()
    {
        string state = Voice.GetState();
        Assert.AreEqual("disconnected", state);
    }

    #endregion

    #region Story 6.2: Mute API Tests

    [Test]
    public void SetMuted_WithoutRuntime_NoException()
    {
        // Should not throw even without runtime
        Assert.DoesNotThrow(() => Voice.SetMuted(true));
        Assert.DoesNotThrow(() => Voice.SetMuted(false));
    }

    [Test]
    public void IsMuted_WithoutRuntime_ReturnsTrue()
    {
        // Default to muted when no runtime
        bool result = Voice.IsMuted();
        Assert.IsTrue(result);
    }

    [Test]
    public void IsCapturing_WithoutRuntime_ReturnsFalse()
    {
        bool result = Voice.IsCapturing();
        Assert.IsFalse(result);
    }

    [Test]
    public void IsSpeaking_WithoutRuntime_ReturnsFalse()
    {
        bool result = Voice.IsSpeaking();
        Assert.IsFalse(result);
    }

    [Test]
    public void StartCapture_WithoutRuntime_ReturnsFalse()
    {
        bool result = Voice.StartCapture();
        Assert.IsFalse(result);
    }

    [Test]
    public void StopCapture_WithoutRuntime_NoException()
    {
        Assert.DoesNotThrow(() => Voice.StopCapture());
    }

    #endregion

    #region Story 6.3: Event Callback Registration Tests

    [Test]
    public void OnConnected_CanRegisterCallback()
    {
        Assert.DoesNotThrow(() => Voice.OnConnected("myConnectCallback"));
    }

    [Test]
    public void OnDisconnected_CanRegisterCallback()
    {
        Assert.DoesNotThrow(() => Voice.OnDisconnected("myDisconnectCallback"));
    }

    [Test]
    public void OnUserJoined_CanRegisterCallback()
    {
        Assert.DoesNotThrow(() => Voice.OnUserJoined("myUserJoinedCallback"));
    }

    [Test]
    public void OnUserLeft_CanRegisterCallback()
    {
        Assert.DoesNotThrow(() => Voice.OnUserLeft("myUserLeftCallback"));
    }

    [Test]
    public void OnUserSpeaking_CanRegisterCallback()
    {
        Assert.DoesNotThrow(() => Voice.OnUserSpeaking("mySpeakingCallback"));
    }

    [Test]
    public void OnMuteChanged_CanRegisterCallback()
    {
        Assert.DoesNotThrow(() => Voice.OnMuteChanged("myMuteCallback"));
    }

    [Test]
    public void OnError_CanRegisterCallback()
    {
        Assert.DoesNotThrow(() => Voice.OnError("myErrorCallback"));
    }

    [Test]
    public void ClearCallbacks_NoException()
    {
        Voice.OnConnected("cb1");
        Voice.OnDisconnected("cb2");
        Voice.OnUserJoined("cb3");

        Assert.DoesNotThrow(() => Voice.ClearCallbacks());
    }

    #endregion

    #region Remote User Control Tests

    [Test]
    public void SetSpeakerPosition_WithoutRuntime_NoException()
    {
        var position = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(1, 2, 3);
        Assert.DoesNotThrow(() => Voice.SetSpeakerPosition("user123", position));
    }

    [Test]
    public void MuteUser_WithoutRuntime_NoException()
    {
        Assert.DoesNotThrow(() => Voice.MuteUser("user123", true));
    }

    [Test]
    public void SetUserVolume_WithoutRuntime_NoException()
    {
        Assert.DoesNotThrow(() => Voice.SetUserVolume("user123", 0.5f));
    }

    #endregion
}
