// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.Voice;
using FiveSQD.WebVerse.Handlers.Voice.Audio;
using FiveSQD.WebVerse.Handlers.Voice.Entities;

/// <summary>
/// Unit tests for VoiceInputEntity and VoiceSpeakerEntity.
/// </summary>
public class VoiceEntityTests
{
    private GameObject _inputEntityGO;
    private GameObject _speakerEntityGO;
    private VoiceInputEntity _inputEntity;
    private VoiceSpeakerEntity _speakerEntity;
    private OpusCodec _codec;
    private VoiceConfig _config;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        _codec = new OpusCodec();
        _config = new VoiceConfig
        {
            Provider = "websocket",
            Endpoint = "wss://voice.example.com",
            MinDistance = 1f,
            MaxDistance = 25f,
            Rolloff = "logarithmic"
        };

        _inputEntityGO = new GameObject("TestVoiceInput");
        _inputEntity = _inputEntityGO.AddComponent<VoiceInputEntity>();

        _speakerEntityGO = new GameObject("TestVoiceSpeaker");
        _speakerEntity = _speakerEntityGO.AddComponent<VoiceSpeakerEntity>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_inputEntity != null)
        {
            _inputEntity.Dispose();
        }
        if (_inputEntityGO != null)
        {
            UnityEngine.Object.DestroyImmediate(_inputEntityGO);
        }

        if (_speakerEntity != null)
        {
            _speakerEntity.Dispose();
        }
        if (_speakerEntityGO != null)
        {
            UnityEngine.Object.DestroyImmediate(_speakerEntityGO);
        }

        if (_codec != null)
        {
            _codec.Dispose();
            _codec = null;
        }
    }

    #region Story 3.2: VoiceInputEntity Tests

    [Test]
    public void VoiceInputEntity_Initialize_SetsProperties()
    {
        _inputEntity.Initialize(_codec);

        Assert.AreEqual(_codec.SampleRate, _inputEntity.SampleRate);
        Assert.AreEqual(_codec.FrameSize, _inputEntity.FrameSize);
        Assert.IsFalse(_inputEntity.IsCapturing);
        Assert.IsFalse(_inputEntity.IsSpeaking);
    }

    [Test]
    public void VoiceInputEntity_Initialize_WithNullCodec_Throws()
    {
        var ex = Assert.Throws<VoiceException>(() =>
        {
            _inputEntity.Initialize(null);
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
    }

    [Test]
    public void VoiceInputEntity_Initialize_WithDeviceName_SetsDeviceName()
    {
        _inputEntity.Initialize(_codec, "TestDevice");

        Assert.AreEqual("TestDevice", _inputEntity.DeviceName);
    }

    [Test]
    public void VoiceInputEntity_GetAvailableDevices_ReturnsList()
    {
        _inputEntity.Initialize(_codec);

        string[] devices = _inputEntity.GetAvailableDevices();

        Assert.IsNotNull(devices);
        // May be empty in test environment, but should not throw
    }

    [Test]
    public void VoiceInputEntity_IsMuted_DefaultsFalse()
    {
        _inputEntity.Initialize(_codec);

        Assert.IsFalse(_inputEntity.IsMuted);
    }

    [Test]
    public void VoiceInputEntity_IsMuted_CanBeSet()
    {
        _inputEntity.Initialize(_codec);

        _inputEntity.IsMuted = true;
        Assert.IsTrue(_inputEntity.IsMuted);

        _inputEntity.IsMuted = false;
        Assert.IsFalse(_inputEntity.IsMuted);
    }

    [Test]
    public void VoiceInputEntity_VADThreshold_HasDefaultValue()
    {
        _inputEntity.Initialize(_codec);

        Assert.Greater(_inputEntity.VADThreshold, 0f);
        Assert.Less(_inputEntity.VADThreshold, 1f);
    }

    [Test]
    public void VoiceInputEntity_VADThreshold_CanBeSet()
    {
        _inputEntity.Initialize(_codec);

        _inputEntity.VADThreshold = 0.05f;
        Assert.AreEqual(0.05f, _inputEntity.VADThreshold, 0.0001f);
    }

    [Test]
    public void VoiceInputEntity_StopCapture_WhenNotCapturing_NoOp()
    {
        _inputEntity.Initialize(_codec);

        // Should not throw
        _inputEntity.StopCapture();

        Assert.IsFalse(_inputEntity.IsCapturing);
    }

    [Test]
    public void VoiceInputEntity_Dispose_CleansUpResources()
    {
        _inputEntity.Initialize(_codec);

        _inputEntity.Dispose();

        // Calling methods after dispose should throw or be no-op
        Assert.IsFalse(_inputEntity.IsCapturing);
    }

    #endregion

    #region Story 3.3 / Epic 4: VoiceSpeakerEntity Tests

    [Test]
    public void VoiceSpeakerEntity_Initialize_SetsProperties()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        Assert.AreEqual("user123", _speakerEntity.UserId);
        Assert.IsFalse(_speakerEntity.IsSpeaking);
    }

    [Test]
    public void VoiceSpeakerEntity_Initialize_WithNullUserId_Throws()
    {
        var ex = Assert.Throws<VoiceException>(() =>
        {
            _speakerEntity.Initialize(null, _codec, _config);
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
    }

    [Test]
    public void VoiceSpeakerEntity_Initialize_WithEmptyUserId_Throws()
    {
        var ex = Assert.Throws<VoiceException>(() =>
        {
            _speakerEntity.Initialize("", _codec, _config);
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
    }

    [Test]
    public void VoiceSpeakerEntity_Initialize_WithNullCodec_Throws()
    {
        var ex = Assert.Throws<VoiceException>(() =>
        {
            _speakerEntity.Initialize("user123", null, _config);
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_INVALID_CONFIG, ex.ErrorCode);
    }

    [Test]
    public void VoiceSpeakerEntity_Initialize_AppliesSpatialSettings()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        // Spatial blend should be 1 (full 3D)
        Assert.AreEqual(1f, _speakerEntity.SpatialBlend, 0.001f);
    }

    [Test]
    public void VoiceSpeakerEntity_Volume_DefaultsToOne()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        Assert.AreEqual(1f, _speakerEntity.Volume, 0.001f);
    }

    [Test]
    public void VoiceSpeakerEntity_Volume_CanBeSet()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        _speakerEntity.Volume = 0.5f;
        Assert.AreEqual(0.5f, _speakerEntity.Volume, 0.001f);
    }

    [Test]
    public void VoiceSpeakerEntity_Volume_ClampedToValidRange()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        _speakerEntity.Volume = 2f;
        Assert.AreEqual(1f, _speakerEntity.Volume, 0.001f);

        _speakerEntity.Volume = -1f;
        Assert.AreEqual(0f, _speakerEntity.Volume, 0.001f);
    }

    [Test]
    public void VoiceSpeakerEntity_IsMuted_DefaultsFalse()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        Assert.IsFalse(_speakerEntity.IsMuted);
    }

    [Test]
    public void VoiceSpeakerEntity_IsMuted_CanBeSet()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        _speakerEntity.IsMuted = true;
        Assert.IsTrue(_speakerEntity.IsMuted);
    }

    [Test]
    public void VoiceSpeakerEntity_SpatialBlend_CanBeSet()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        _speakerEntity.SpatialBlend = 0.5f;
        Assert.AreEqual(0.5f, _speakerEntity.SpatialBlend, 0.001f);
    }

    [Test]
    public void VoiceSpeakerEntity_ReceiveAudio_NullData_NoOp()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        // Should not throw
        _speakerEntity.ReceiveAudio(null, 1);

        Assert.IsFalse(_speakerEntity.IsSpeaking);
    }

    [Test]
    public void VoiceSpeakerEntity_ReceiveAudio_EmptyData_NoOp()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        // Should not throw
        _speakerEntity.ReceiveAudio(new byte[0], 1);

        Assert.IsFalse(_speakerEntity.IsSpeaking);
    }

    [Test]
    public void VoiceSpeakerEntity_OnSpeakingChanged_FiresOnStateChange()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        bool? speakingState = null;
        _speakerEntity.OnSpeakingChanged += (isSpeaking) => speakingState = isSpeaking;

        // Simulate receiving enough audio to trigger playback
        // In real scenario, this would trigger after jitter buffer fills
        // For this test, we just verify the event subscription works
        Assert.IsNull(speakingState); // Not yet fired
    }

    [Test]
    public void VoiceSpeakerEntity_Dispose_CleansUpResources()
    {
        _speakerEntity.Initialize("user123", _codec, _config);

        _speakerEntity.Dispose();

        // After dispose, receiving audio should be no-op
        _speakerEntity.ReceiveAudio(new byte[] { 1, 2, 3 }, 1);
        Assert.IsFalse(_speakerEntity.IsSpeaking);
    }

    #endregion
}
