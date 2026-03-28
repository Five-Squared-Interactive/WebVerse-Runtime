// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.Voice;
using FiveSQD.WebVerse.Handlers.Voice.Providers;

/// <summary>
/// Unit tests for IVoiceProvider interface and implementations.
/// </summary>
public class VoiceProviderTests
{
    private MockVoiceProvider _provider;
    private VoiceConfig _validConfig;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        _provider = new MockVoiceProvider();
        _provider.AutoReconnectEnabled = false; // Always disable to avoid async loops
        _validConfig = new VoiceConfig
        {
            Provider = "websocket",
            Endpoint = "wss://voice.example.com/v1"
        };
    }

    [TearDown]
    public void TearDown()
    {
        if (_provider != null)
        {
            _provider.AutoReconnectEnabled = false;
            _provider.Dispose();
            _provider = null;
        }
    }

    #region Story 2.1: IVoiceProvider Interface Tests

    [Test]
    public void IVoiceProvider_InitialState_IsDisconnected()
    {
        Assert.AreEqual(VoiceConnectionState.Disconnected, _provider.State);
    }

    [Test]
    public void IVoiceProvider_HasRequiredEvents()
    {
        bool stateChangedFired = false;
        bool audioReceivedFired = false;
        bool userJoinedFired = false;
        bool userLeftFired = false;
        bool userSpeakingFired = false;

        _provider.OnStateChanged += (s, e) => stateChangedFired = true;
        _provider.OnAudioReceived += (s, e) => audioReceivedFired = true;
        _provider.OnUserJoined += (s, e) => userJoinedFired = true;
        _provider.OnUserLeft += (s, e) => userLeftFired = true;
        _provider.OnUserSpeaking += (s, e) => userSpeakingFired = true;

        // Connect synchronously
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();

        // Trigger events
        _provider.SimulateAudioReceived("user1", new byte[] { 1, 2, 3 }, 1);
        _provider.SimulateUserJoined("user2");
        _provider.SimulateUserLeft("user2");
        _provider.SimulateUserSpeaking("user1", true);

        Assert.IsTrue(stateChangedFired, "OnStateChanged should fire");
        Assert.IsTrue(audioReceivedFired, "OnAudioReceived should fire");
        Assert.IsTrue(userJoinedFired, "OnUserJoined should fire");
        Assert.IsTrue(userLeftFired, "OnUserLeft should fire");
        Assert.IsTrue(userSpeakingFired, "OnUserSpeaking should fire");
    }

    [Test]
    public void VoiceConnectionState_HasAllRequiredValues()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Disconnected));
        Assert.IsTrue(Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Connecting));
        Assert.IsTrue(Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Connected));
        Assert.IsTrue(Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Reconnecting));
        Assert.IsTrue(Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Error));
    }

    #endregion

    #region Story 2.2: Connection State Machine Tests

    [Test]
    public void Connect_FromDisconnected_TransitionsToConnecting()
    {
        bool sawConnecting = false;
        _provider.OnStateChanged += (s, e) =>
        {
            if (e.NewState == VoiceConnectionState.Connecting)
            {
                sawConnecting = true;
            }
        };

        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();

        Assert.IsTrue(sawConnecting, "Should have transitioned through Connecting");
        Assert.AreEqual(VoiceConnectionState.Connected, _provider.State);
    }

    [Test]
    public void Connect_Success_TransitionsToConnected()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();
        Assert.AreEqual(VoiceConnectionState.Connected, _provider.State);
    }

    [Test]
    public void Connect_Failure_TransitionsToError()
    {
        _provider.SimulateConnectionFailure = true;

        LogAssert.Expect(LogType.Error, "[BaseVoiceProvider->ConnectAsync] Connection failed: Simulated connection failure");

        Assert.Throws<VoiceException>(() =>
        {
            _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();
        });

        Assert.AreEqual(VoiceConnectionState.Error, _provider.State);
    }

    [Test]
    public void ConnectionLost_WithAutoReconnectDisabled_TransitionsToError()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();
        Assert.AreEqual(VoiceConnectionState.Connected, _provider.State);

        LogAssert.Expect(LogType.Warning, "[BaseVoiceProvider] Connection lost: Test connection loss");
        LogAssert.Expect(LogType.Error, "[BaseVoiceProvider] Error: Reconnection disabled");

        _provider.SimulateConnectionLostNow("Test connection loss");

        Assert.AreEqual(VoiceConnectionState.Error, _provider.State);
    }

    [Test]
    public void Disconnect_FromConnected_TransitionsToDisconnected()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();
        Assert.AreEqual(VoiceConnectionState.Connected, _provider.State);

        _provider.DisconnectAsync().GetAwaiter().GetResult();
        Assert.AreEqual(VoiceConnectionState.Disconnected, _provider.State);
    }

    [Test]
    public void StateChange_FiresOnStateChangedEvent()
    {
        var stateChanges = new System.Collections.Generic.List<VoiceConnectionState>();

        _provider.OnStateChanged += (s, e) =>
        {
            stateChanges.Add(e.NewState);
        };

        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();
        _provider.DisconnectAsync().GetAwaiter().GetResult();

        Assert.IsTrue(stateChanges.Contains(VoiceConnectionState.Connecting));
        Assert.IsTrue(stateChanges.Contains(VoiceConnectionState.Connected));
        Assert.IsTrue(stateChanges.Contains(VoiceConnectionState.Disconnected));
    }

    [Test]
    public void Connect_WhenAlreadyConnected_IsIdempotent()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();
        int connectionAttemptsBefore = _provider.ConnectionAttempts;

        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();

        Assert.AreEqual(connectionAttemptsBefore, _provider.ConnectionAttempts);
    }

    #endregion

    #region Story 2.3: Send/Receive Audio Tests

    [Test]
    public void SendAudio_WhenConnected_Succeeds()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();

        byte[] testData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        _provider.SendAudioAsync(testData, 1).GetAwaiter().GetResult();

        Assert.AreEqual(1, _provider.SentAudio.Count);
        Assert.AreEqual(testData, _provider.SentAudio[0].data);
        Assert.AreEqual(1u, _provider.SentAudio[0].seq);
    }

    [Test]
    public void SendAudio_WhenNotConnected_Throws()
    {
        byte[] testData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        var ex = Assert.Throws<VoiceException>(() =>
        {
            _provider.SendAudioAsync(testData, 1).GetAwaiter().GetResult();
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_CONNECTION_LOST, ex.ErrorCode);
    }

    [Test]
    public void OnAudioReceived_FiresWithCorrectData()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();

        string receivedUserId = null;
        byte[] receivedData = null;
        uint receivedSeq = 0;

        _provider.OnAudioReceived += (s, e) =>
        {
            receivedUserId = e.UserId;
            receivedData = e.OpusData;
            receivedSeq = e.SequenceNumber;
        };

        byte[] testData = new byte[] { 0x10, 0x20, 0x30 };
        _provider.SimulateAudioReceived("user123", testData, 42);

        Assert.AreEqual("user123", receivedUserId);
        Assert.AreEqual(testData, receivedData);
        Assert.AreEqual(42u, receivedSeq);
    }

    [Test]
    public void OnUserJoined_FiresWithUserId()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();

        string joinedUserId = null;
        _provider.OnUserJoined += (s, e) => joinedUserId = e.UserId;

        _provider.SimulateUserJoined("newUser");

        Assert.AreEqual("newUser", joinedUserId);
    }

    [Test]
    public void OnUserLeft_FiresWithUserId()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();

        string leftUserId = null;
        _provider.OnUserLeft += (s, e) => leftUserId = e.UserId;

        _provider.SimulateUserLeft("leavingUser");

        Assert.AreEqual("leavingUser", leftUserId);
    }

    [Test]
    public void OnUserSpeaking_FiresWithUserIdAndState()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();

        string speakingUserId = null;
        bool? isSpeaking = null;

        _provider.OnUserSpeaking += (s, e) =>
        {
            speakingUserId = e.UserId;
            isSpeaking = e.IsSpeaking;
        };

        _provider.SimulateUserSpeaking("speaker1", true);

        Assert.AreEqual("speaker1", speakingUserId);
        Assert.AreEqual(true, isSpeaking);
    }

    #endregion

    #region Story 2.4: Reconnection Logic Tests (State-based, no async)

    [Test]
    public void AutoReconnectEnabled_DefaultsToTrue()
    {
        var freshProvider = new MockVoiceProvider();
        Assert.IsTrue(freshProvider.AutoReconnectEnabled);
        freshProvider.Dispose();
    }

    [Test]
    public void TransitionToError_SetsErrorState()
    {
        _provider.ConnectAsync(_validConfig).GetAwaiter().GetResult();

        LogAssert.Expect(LogType.Error, "[BaseVoiceProvider] Error: Fatal error");

        _provider.SimulateError("Fatal error");

        Assert.AreEqual(VoiceConnectionState.Error, _provider.State);
    }

    #endregion

    #region Config Validation Tests

    [Test]
    public void Connect_WithNullConfig_Throws()
    {
        var ex = Assert.Throws<VoiceException>(() =>
        {
            _provider.ConnectAsync(null).GetAwaiter().GetResult();
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
    }

    [Test]
    public void Connect_WithInvalidConfig_Throws()
    {
        var invalidConfig = new VoiceConfig(); // Missing endpoint

        var ex = Assert.Throws<VoiceException>(() =>
        {
            _provider.ConnectAsync(invalidConfig).GetAwaiter().GetResult();
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
    }

    #endregion
}
