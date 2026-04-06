// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Utilities;

#if USE_CONCENTUS
using Concentus.Structs;
using Concentus.Enums;
#endif

namespace FiveSQD.WebVerse.Handlers.Voice.Audio
{
    /// <summary>
    /// Opus codec implementation using Concentus library.
    /// Requires USE_CONCENTUS define and Concentus.dll to function.
    /// Falls back to passthrough mode if Concentus is not available.
    /// </summary>
    public class OpusCodec : IOpusCodec
    {
        /// <summary>
        /// Default sample rate for voice (48kHz).
        /// </summary>
        public const int DefaultSampleRate = 48000;

        /// <summary>
        /// Default channels (mono).
        /// </summary>
        public const int DefaultChannels = 1;

        /// <summary>
        /// Default frame duration in milliseconds.
        /// </summary>
        public const int DefaultFrameDurationMs = 20;

        /// <summary>
        /// Default bitrate for voice.
        /// </summary>
        public const int DefaultBitrate = 24000;

        /// <inheritdoc />
        public int SampleRate { get; }

        /// <inheritdoc />
        public int Channels { get; }

        /// <inheritdoc />
        public int FrameSize { get; }

#if USE_CONCENTUS
        private readonly OpusEncoder _encoder;
        private readonly OpusDecoder _decoder;
        private readonly byte[] _encodeBuffer;
#endif

        private bool _disposed;

        /// <summary>
        /// Create a new OpusCodec with default settings (48kHz, mono, 20ms frames).
        /// </summary>
        public OpusCodec() : this(DefaultSampleRate, DefaultChannels, DefaultFrameDurationMs)
        {
        }

        /// <summary>
        /// Create a new OpusCodec with custom settings.
        /// </summary>
        /// <param name="sampleRate">Sample rate in Hz.</param>
        /// <param name="channels">Number of channels (1 or 2).</param>
        /// <param name="frameDurationMs">Frame duration in milliseconds (2.5, 5, 10, 20, 40, 60).</param>
        public OpusCodec(int sampleRate, int channels, int frameDurationMs)
        {
            SampleRate = sampleRate;
            Channels = channels;
            FrameSize = sampleRate * frameDurationMs / 1000;

#if USE_CONCENTUS
            try
            {
                _encoder = new OpusEncoder(sampleRate, channels, OpusApplication.OPUS_APPLICATION_VOIP);
                _encoder.Bitrate = DefaultBitrate;
                _encoder.SignalType = OpusSignal.OPUS_SIGNAL_VOICE;
                _encoder.Complexity = 5;

                _decoder = new OpusDecoder(sampleRate, channels);

                // Max Opus frame size
                _encodeBuffer = new byte[4000];

                Logging.Log($"[OpusCodec] Initialized with Concentus: {sampleRate}Hz, {channels}ch, {frameDurationMs}ms frames");
            }
            catch (Exception ex)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_ENCODING_ERROR,
                    $"Failed to initialize Opus codec: {ex.Message}", ex);
            }
#else
            Logging.LogWarning("[OpusCodec] Concentus not available, using passthrough mode");
#endif
        }

        /// <inheritdoc />
        public byte[] Encode(short[] pcmData)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(OpusCodec));
            }

            if (pcmData == null || pcmData.Length == 0)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_ENCODING_ERROR, "PCM data is null or empty");
            }

            if (pcmData.Length != FrameSize * Channels)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_ENCODING_ERROR,
                    $"PCM data length {pcmData.Length} does not match expected frame size {FrameSize * Channels}");
            }

#if USE_CONCENTUS
            try
            {
                int encodedLength = _encoder.Encode(pcmData, 0, FrameSize, _encodeBuffer, 0, _encodeBuffer.Length);

                byte[] result = new byte[encodedLength];
                Array.Copy(_encodeBuffer, result, encodedLength);
                return result;
            }
            catch (Exception ex)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_ENCODING_ERROR,
                    $"Opus encoding failed: {ex.Message}", ex);
            }
#else
            // Passthrough: convert shorts to bytes
            byte[] result = new byte[pcmData.Length * 2];
            Buffer.BlockCopy(pcmData, 0, result, 0, result.Length);
            return result;
#endif
        }

        /// <inheritdoc />
        public short[] Decode(byte[] opusData)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(OpusCodec));
            }

            if (opusData == null || opusData.Length == 0)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_DECODING_ERROR, "Opus data is null or empty");
            }

#if USE_CONCENTUS
            try
            {
                short[] pcmOutput = new short[FrameSize * Channels];
                int decodedSamples = _decoder.Decode(opusData, 0, opusData.Length, pcmOutput, 0, FrameSize, false);

                if (decodedSamples != FrameSize)
                {
                    // Resize if needed
                    Array.Resize(ref pcmOutput, decodedSamples * Channels);
                }

                return pcmOutput;
            }
            catch (Exception ex)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_DECODING_ERROR,
                    $"Opus decoding failed: {ex.Message}", ex);
            }
#else
            // Passthrough: convert bytes back to shorts
            short[] result = new short[opusData.Length / 2];
            Buffer.BlockCopy(opusData, 0, result, 0, opusData.Length);
            return result;
#endif
        }

        /// <inheritdoc />
        public short[] DecodePLC()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(OpusCodec));
            }

#if USE_CONCENTUS
            try
            {
                short[] pcmOutput = new short[FrameSize * Channels];
                _decoder.Decode(null, 0, 0, pcmOutput, 0, FrameSize, true);
                return pcmOutput;
            }
            catch (Exception ex)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_DECODING_ERROR,
                    $"Opus PLC failed: {ex.Message}", ex);
            }
#else
            // Return silence
            return new short[FrameSize * Channels];
#endif
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;

#if USE_CONCENTUS
            // Concentus encoder/decoder don't implement IDisposable
            // but we mark as disposed to prevent further use
#endif

            _disposed = true;
        }
    }
}
