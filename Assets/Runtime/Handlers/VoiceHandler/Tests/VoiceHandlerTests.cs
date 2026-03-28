// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.Voice;

/// <summary>
/// Unit tests for VoiceHandler.
/// </summary>
public class VoiceHandlerTests
{
    private GameObject _handlerGameObject;
    private VoiceHandler _voiceHandler;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        _handlerGameObject = new GameObject("VoiceHandler");
        _voiceHandler = _handlerGameObject.AddComponent<VoiceHandler>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_handlerGameObject != null)
        {
            Object.DestroyImmediate(_handlerGameObject);
        }
    }

    #region Story 1.3: VoiceHandler Initialization Tests

    [Test]
    public void Initialize_WithValidConfig_SetsStateToDisconnected()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Endpoint = "wss://voice.example.com/v1"
        };

        // Act
        _voiceHandler.Initialize(config);

        // Assert
        Assert.AreEqual(VoiceConnectionState.Disconnected, _voiceHandler.State);
    }

    [Test]
    public void Initialize_ConfigAccessible_ReturnsCorrectValues()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Provider = "websocket",
            Endpoint = "wss://voice.example.com/v1",
            MinDistance = 2f,
            MaxDistance = 30f,
            Rolloff = "linear"
        };

        // Act
        _voiceHandler.Initialize(config);

        // Assert
        Assert.IsNotNull(_voiceHandler.Config);
        Assert.AreEqual("websocket", _voiceHandler.Config.Provider);
        Assert.AreEqual("wss://voice.example.com/v1", _voiceHandler.Config.Endpoint);
        Assert.AreEqual(2f, _voiceHandler.Config.MinDistance);
        Assert.AreEqual(30f, _voiceHandler.Config.MaxDistance);
        Assert.AreEqual("linear", _voiceHandler.Config.Rolloff);
    }

    [Test]
    public void Initialize_InvalidConfig_ThrowsVoiceException()
    {
        // Arrange - config with no endpoint
        var config = new VoiceConfig
        {
            Provider = "websocket"
        };

        // Act & Assert
        var ex = Assert.Throws<VoiceException>(() => _voiceHandler.Initialize(config));
        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
    }

    [Test]
    public void Initialize_NullConfig_ThrowsVoiceException()
    {
        // Act & Assert
        var ex = Assert.Throws<VoiceException>(() => _voiceHandler.Initialize(null));
        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
        Assert.That(ex.Message, Does.Contain("null").IgnoreCase);
    }

    [Test]
    public void Initialize_WithoutConfig_LogsError()
    {
        // Arrange & Act - Logging.LogError outputs via Debug.LogError
        LogAssert.Expect(LogType.Error, "[VoiceHandler->Initialize] Initialize must be called with VoiceConfig.");
        _voiceHandler.Initialize();

        // The Initialize() override logs an error
    }

    [Test]
    public void Initialize_LogsWithVoiceHandlerPrefix()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Endpoint = "wss://voice.example.com/v1"
        };

        // Expect the initialization log message
        LogAssert.Expect(LogType.Log, "[VoiceHandler] Initialized with endpoint: wss://voice.example.com/v1");
        LogAssert.Expect(LogType.Log, "[VoiceHandler] Initialized.");

        // Act
        _voiceHandler.Initialize(config);
    }

    [Test]
    public void Terminate_CleansUpProperly()
    {
        // Arrange
        var config = new VoiceConfig
        {
            Endpoint = "wss://voice.example.com/v1"
        };
        _voiceHandler.Initialize(config);

        // Expect termination log
        LogAssert.Expect(LogType.Log, "[VoiceHandler] Terminated.");

        // Act
        _voiceHandler.Terminate();

        // Assert
        Assert.AreEqual(VoiceConnectionState.Disconnected, _voiceHandler.State);
        Assert.IsNull(_voiceHandler.Config);
    }

    [Test]
    public void State_InitialValue_IsDisconnected()
    {
        // Assert - state should be Disconnected before Initialize
        Assert.AreEqual(VoiceConnectionState.Disconnected, _voiceHandler.State);
    }

    [Test]
    public void VoiceConnectionState_HasExpectedValues()
    {
        // Assert all expected states exist
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Disconnected));
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Connecting));
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Connected));
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Reconnecting));
        Assert.IsTrue(System.Enum.IsDefined(typeof(VoiceConnectionState), VoiceConnectionState.Error));
    }

    #endregion
}
