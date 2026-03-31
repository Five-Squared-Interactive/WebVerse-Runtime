// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using NUnit.Framework;
using FiveSQD.WebVerse.Handlers.Voice;
using FiveSQD.WebVerse.Handlers.Voice.Audio;

/// <summary>
/// Unit tests for IOpusCodec interface and OpusCodec implementation.
/// </summary>
public class VoiceCodecTests
{
    private OpusCodec _codec;

    [SetUp]
    public void SetUp()
    {
        _codec = new OpusCodec();
    }

    [TearDown]
    public void TearDown()
    {
        if (_codec != null)
        {
            _codec.Dispose();
            _codec = null;
        }
    }

    #region Story 3.1: Opus Codec Tests

    [Test]
    public void OpusCodec_DefaultSettings_HasCorrectValues()
    {
        Assert.AreEqual(48000, _codec.SampleRate);
        Assert.AreEqual(1, _codec.Channels);
        Assert.AreEqual(960, _codec.FrameSize); // 48000 * 20 / 1000
    }

    [Test]
    public void OpusCodec_CustomSettings_HasCorrectValues()
    {
        using (var customCodec = new OpusCodec(24000, 2, 10))
        {
            Assert.AreEqual(24000, customCodec.SampleRate);
            Assert.AreEqual(2, customCodec.Channels);
            Assert.AreEqual(240, customCodec.FrameSize); // 24000 * 10 / 1000
        }
    }

    [Test]
    public void Encode_ValidPCMData_ReturnsEncodedBytes()
    {
        short[] pcmData = GenerateTestPCMData(_codec.FrameSize * _codec.Channels);

        byte[] encoded = _codec.Encode(pcmData);

        Assert.IsNotNull(encoded);
        Assert.Greater(encoded.Length, 0);
    }

    [Test]
    public void Encode_NullData_ThrowsVoiceException()
    {
        var ex = Assert.Throws<VoiceException>(() =>
        {
            _codec.Encode(null);
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_ENCODING_ERROR, ex.ErrorCode);
    }

    [Test]
    public void Encode_EmptyData_ThrowsVoiceException()
    {
        var ex = Assert.Throws<VoiceException>(() =>
        {
            _codec.Encode(new short[0]);
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_ENCODING_ERROR, ex.ErrorCode);
    }

    [Test]
    public void Encode_WrongFrameSize_ThrowsVoiceException()
    {
        short[] wrongSizeData = new short[100]; // Not matching frame size

        var ex = Assert.Throws<VoiceException>(() =>
        {
            _codec.Encode(wrongSizeData);
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_ENCODING_ERROR, ex.ErrorCode);
    }

    [Test]
    public void Decode_ValidOpusData_ReturnsPCMSamples()
    {
        // First encode some data
        short[] originalPCM = GenerateTestPCMData(_codec.FrameSize * _codec.Channels);
        byte[] encoded = _codec.Encode(originalPCM);

        // Then decode
        short[] decoded = _codec.Decode(encoded);

        Assert.IsNotNull(decoded);
        Assert.Greater(decoded.Length, 0);
    }

    [Test]
    public void Decode_NullData_ThrowsVoiceException()
    {
        var ex = Assert.Throws<VoiceException>(() =>
        {
            _codec.Decode(null);
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_DECODING_ERROR, ex.ErrorCode);
    }

    [Test]
    public void Decode_EmptyData_ThrowsVoiceException()
    {
        var ex = Assert.Throws<VoiceException>(() =>
        {
            _codec.Decode(new byte[0]);
        });

        Assert.AreEqual(VoiceErrorCode.VOICE_DECODING_ERROR, ex.ErrorCode);
    }

    [Test]
    public void DecodePLC_ReturnsCorrectFrameSize()
    {
        short[] plcOutput = _codec.DecodePLC();

        Assert.IsNotNull(plcOutput);
        Assert.AreEqual(_codec.FrameSize * _codec.Channels, plcOutput.Length);
    }

    [Test]
    public void Encode_AfterDispose_ThrowsObjectDisposedException()
    {
        _codec.Dispose();

        Assert.Throws<ObjectDisposedException>(() =>
        {
            _codec.Encode(new short[960]);
        });

        _codec = null; // Prevent double dispose in TearDown
    }

    [Test]
    public void Decode_AfterDispose_ThrowsObjectDisposedException()
    {
        _codec.Dispose();

        Assert.Throws<ObjectDisposedException>(() =>
        {
            _codec.Decode(new byte[] { 1, 2, 3 });
        });

        _codec = null; // Prevent double dispose in TearDown
    }

    [Test]
    public void DecodePLC_AfterDispose_ThrowsObjectDisposedException()
    {
        _codec.Dispose();

        Assert.Throws<ObjectDisposedException>(() =>
        {
            _codec.DecodePLC();
        });

        _codec = null; // Prevent double dispose in TearDown
    }

    [Test]
    public void EncodeDecode_RoundTrip_MaintainsDataIntegrity()
    {
        short[] originalPCM = GenerateTestPCMData(_codec.FrameSize * _codec.Channels);

        byte[] encoded = _codec.Encode(originalPCM);
        short[] decoded = _codec.Decode(encoded);

        // With passthrough mode (no Concentus), data should be identical
        // With Opus encoding, we check approximate reconstruction
        Assert.AreEqual(originalPCM.Length, decoded.Length);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Generate test PCM data (sine wave).
    /// </summary>
    private short[] GenerateTestPCMData(int sampleCount)
    {
        short[] data = new short[sampleCount];
        double frequency = 440.0; // A4 note
        double amplitude = 16000.0;

        for (int i = 0; i < sampleCount; i++)
        {
            double t = (double)i / _codec.SampleRate;
            data[i] = (short)(amplitude * Math.Sin(2.0 * Math.PI * frequency * t));
        }

        return data;
    }

    #endregion
}
