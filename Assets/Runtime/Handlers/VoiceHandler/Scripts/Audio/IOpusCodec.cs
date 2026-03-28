// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Voice.Audio
{
    /// <summary>
    /// Interface for Opus audio encoding and decoding.
    /// </summary>
    public interface IOpusCodec : IDisposable
    {
        /// <summary>
        /// Sample rate in Hz (typically 48000).
        /// </summary>
        int SampleRate { get; }

        /// <summary>
        /// Number of channels (1 for mono, 2 for stereo).
        /// </summary>
        int Channels { get; }

        /// <summary>
        /// Frame size in samples per channel (typically 960 for 20ms at 48kHz).
        /// </summary>
        int FrameSize { get; }

        /// <summary>
        /// Encode PCM audio data to Opus format.
        /// </summary>
        /// <param name="pcmData">Raw PCM audio samples (16-bit signed integers).</param>
        /// <returns>Opus-encoded data.</returns>
        byte[] Encode(short[] pcmData);

        /// <summary>
        /// Decode Opus data to PCM audio.
        /// </summary>
        /// <param name="opusData">Opus-encoded data.</param>
        /// <returns>Raw PCM audio samples (16-bit signed integers).</returns>
        short[] Decode(byte[] opusData);

        /// <summary>
        /// Decode with packet loss concealment (when a packet is missing).
        /// </summary>
        /// <returns>Concealed PCM audio samples.</returns>
        short[] DecodePLC();
    }
}
