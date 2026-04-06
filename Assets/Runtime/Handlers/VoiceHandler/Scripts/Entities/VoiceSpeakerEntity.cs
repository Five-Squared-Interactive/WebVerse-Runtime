// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Voice.Audio;

namespace FiveSQD.WebVerse.Handlers.Voice.Entities
{
    /// <summary>
    /// Entity for playing back voice audio spatially from a remote user.
    /// </summary>
    public class VoiceSpeakerEntity : MonoBehaviour
    {
        /// <summary>
        /// Event fired when speaking state changes.
        /// </summary>
        public event Action<bool> OnSpeakingChanged;

        /// <summary>
        /// User ID associated with this speaker.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Whether this speaker is currently receiving audio.
        /// </summary>
        public bool IsSpeaking { get; private set; }

        /// <summary>
        /// Volume level (0-1).
        /// </summary>
        public float Volume
        {
            get => _audioSource?.volume ?? 1f;
            set
            {
                if (_audioSource != null)
                {
                    _audioSource.volume = Mathf.Clamp01(value);
                }
            }
        }

        /// <summary>
        /// Whether this speaker is muted.
        /// </summary>
        public bool IsMuted
        {
            get => _audioSource?.mute ?? false;
            set
            {
                if (_audioSource != null)
                {
                    _audioSource.mute = value;
                }
            }
        }

        /// <summary>
        /// Spatial blend (0 = 2D, 1 = 3D).
        /// </summary>
        public float SpatialBlend
        {
            get => _audioSource?.spatialBlend ?? 1f;
            set
            {
                if (_audioSource != null)
                {
                    _audioSource.spatialBlend = Mathf.Clamp01(value);
                }
            }
        }

        /// <summary>
        /// Minimum distance for 3D audio attenuation.
        /// </summary>
        public float MinDistance
        {
            get => _audioSource?.minDistance ?? 1f;
            set
            {
                if (_audioSource != null)
                {
                    _audioSource.minDistance = value;
                }
            }
        }

        /// <summary>
        /// Maximum distance for 3D audio attenuation.
        /// </summary>
        public float MaxDistance
        {
            get => _audioSource?.maxDistance ?? 25f;
            set
            {
                if (_audioSource != null)
                {
                    _audioSource.maxDistance = value;
                }
            }
        }

        /// <summary>
        /// The transform this speaker is attached to (for following a character).
        /// </summary>
        public Transform AttachedTo { get; private set; }

        /// <summary>
        /// Local offset from the attached transform.
        /// </summary>
        public Vector3 AttachmentOffset { get; set; } = Vector3.zero;

        private IOpusCodec _codec;
        private AudioSource _audioSource;
        private AudioClip _playbackClip;

        // Ring buffer for decoded audio
        private float[] _ringBuffer;
        private int _writePosition;
        private int _readPosition;
        private readonly object _bufferLock = new object();

        // Playback state
        private bool _isPlaying;
        private int _sampleRate;
        private int _channels;
        private const int BufferLengthSeconds = 2;
        private const int MinBufferedSamples = 960; // One frame at 48kHz/20ms

        // Jitter buffer
        private readonly Queue<(byte[] data, uint seq)> _jitterBuffer = new Queue<(byte[], uint)>();
        private uint _lastPlayedSequence;
        private const int JitterBufferTarget = 3; // Target frames in buffer
        private const int MaxJitterBufferSize = 10;

        // Silence detection
        private float _lastAudioTime;
        private const float SilenceTimeout = 0.5f; // 500ms

        private bool _disposed;

        /// <summary>
        /// Initialize the voice speaker entity.
        /// </summary>
        /// <param name="userId">User ID this speaker represents.</param>
        /// <param name="codec">Opus codec to use for decoding.</param>
        /// <param name="config">Voice configuration for spatial settings.</param>
        public void Initialize(string userId, IOpusCodec codec, VoiceConfig config)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG, "UserId cannot be null or empty.");
            }

            if (codec == null)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG, "Codec cannot be null.");
            }

            UserId = userId;
            _codec = codec;
            _sampleRate = codec.SampleRate;
            _channels = codec.Channels;

            // Create ring buffer
            int bufferSize = _sampleRate * _channels * BufferLengthSeconds;
            _ringBuffer = new float[bufferSize];
            _writePosition = 0;
            _readPosition = 0;

            // Create audio source
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            _audioSource.spatialBlend = 1f; // Full 3D

            // Apply spatial settings from config
            if (config != null)
            {
                _audioSource.minDistance = config.MinDistance;
                _audioSource.maxDistance = config.MaxDistance;

                switch (config.Rolloff?.ToLowerInvariant())
                {
                    case "linear":
                        _audioSource.rolloffMode = AudioRolloffMode.Linear;
                        break;
                    case "custom":
                        _audioSource.rolloffMode = AudioRolloffMode.Custom;
                        break;
                    default:
                        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                        break;
                }
            }

            // Create streaming audio clip
            _playbackClip = AudioClip.Create(
                $"VoiceSpeaker_{userId}",
                _sampleRate * BufferLengthSeconds,
                _channels,
                _sampleRate,
                true,
                OnAudioRead);

            _audioSource.clip = _playbackClip;

            Logging.Log($"[VoiceSpeakerEntity] Initialized for user: {userId}");
        }

        /// <summary>
        /// Attach this speaker to a transform (e.g., a character's head).
        /// The speaker will follow the transform's position.
        /// </summary>
        /// <param name="target">Transform to attach to.</param>
        /// <param name="offset">Local offset from the target (e.g., mouth position).</param>
        public void AttachTo(Transform target, Vector3 offset = default)
        {
            AttachedTo = target;
            AttachmentOffset = offset;

            if (target != null)
            {
                // Parent to target for automatic position updates
                transform.SetParent(target);
                transform.localPosition = offset;
                transform.localRotation = Quaternion.identity;
                Logging.Log($"[VoiceSpeakerEntity] Attached to {target.name}");
            }
        }

        /// <summary>
        /// Detach this speaker from any attached transform.
        /// </summary>
        public void Detach()
        {
            if (AttachedTo != null)
            {
                // Preserve world position when detaching
                Vector3 worldPos = transform.position;
                transform.SetParent(null);
                transform.position = worldPos;
                AttachedTo = null;
                Logging.Log($"[VoiceSpeakerEntity] Detached from parent");
            }
        }

        /// <summary>
        /// Receive encoded audio data from a remote user.
        /// </summary>
        /// <param name="opusData">Opus-encoded audio data.</param>
        /// <param name="sequenceNumber">Sequence number for ordering.</param>
        public void ReceiveAudio(byte[] opusData, uint sequenceNumber)
        {
            if (_disposed)
            {
                return;
            }

            if (opusData == null || opusData.Length == 0)
            {
                return;
            }

            lock (_jitterBuffer)
            {
                // Discard old packets
                if (sequenceNumber <= _lastPlayedSequence && _lastPlayedSequence > 0)
                {
                    return;
                }

                // Add to jitter buffer
                if (_jitterBuffer.Count < MaxJitterBufferSize)
                {
                    _jitterBuffer.Enqueue((opusData, sequenceNumber));
                }

                // Start playback when buffer reaches target
                if (!_isPlaying && _jitterBuffer.Count >= JitterBufferTarget)
                {
                    StartPlayback();
                }
            }

            _lastAudioTime = Time.time;
        }

        private void StartPlayback()
        {
            if (_isPlaying || _audioSource == null)
            {
                return;
            }

            _isPlaying = true;
            _audioSource.Play();

            if (!IsSpeaking)
            {
                IsSpeaking = true;
                OnSpeakingChanged?.Invoke(true);
            }

            Logging.Log($"[VoiceSpeakerEntity] Started playback for user: {UserId}");
        }

        private void StopPlayback()
        {
            if (!_isPlaying)
            {
                return;
            }

            _isPlaying = false;
            _audioSource?.Stop();

            if (IsSpeaking)
            {
                IsSpeaking = false;
                OnSpeakingChanged?.Invoke(false);
            }

            // Clear buffers
            lock (_bufferLock)
            {
                Array.Clear(_ringBuffer, 0, _ringBuffer.Length);
                _writePosition = 0;
                _readPosition = 0;
            }

            lock (_jitterBuffer)
            {
                _jitterBuffer.Clear();
            }

            Logging.Log($"[VoiceSpeakerEntity] Stopped playback for user: {UserId}");
        }

        private void Update()
        {
            if (_disposed)
            {
                return;
            }

            // Process jitter buffer
            ProcessJitterBuffer();

            // Check for silence timeout
            if (_isPlaying && Time.time - _lastAudioTime > SilenceTimeout)
            {
                StopPlayback();
            }
        }

        private void ProcessJitterBuffer()
        {
            lock (_jitterBuffer)
            {
                // Decode packets from jitter buffer
                while (_jitterBuffer.Count > 0)
                {
                    var (opusData, seq) = _jitterBuffer.Dequeue();

                    // Check for gaps and apply PLC
                    if (_lastPlayedSequence > 0 && seq > _lastPlayedSequence + 1)
                    {
                        int missedPackets = (int)(seq - _lastPlayedSequence - 1);
                        for (int i = 0; i < missedPackets && i < 3; i++)
                        {
                            try
                            {
                                short[] plcSamples = _codec.DecodePLC();
                                WriteToRingBuffer(plcSamples);
                            }
                            catch (Exception ex)
                            {
                                Logging.LogWarning($"[VoiceSpeakerEntity] PLC failed: {ex.Message}");
                            }
                        }
                    }

                    try
                    {
                        short[] pcmSamples = _codec.Decode(opusData);
                        WriteToRingBuffer(pcmSamples);
                        _lastPlayedSequence = seq;
                    }
                    catch (Exception ex)
                    {
                        Logging.LogWarning($"[VoiceSpeakerEntity] Decoding failed: {ex.Message}");
                    }
                }
            }
        }

        private void WriteToRingBuffer(short[] pcmSamples)
        {
            if (pcmSamples == null || pcmSamples.Length == 0)
            {
                return;
            }

            lock (_bufferLock)
            {
                for (int i = 0; i < pcmSamples.Length; i++)
                {
                    // Convert short to float
                    _ringBuffer[_writePosition] = pcmSamples[i] / (float)short.MaxValue;
                    _writePosition = (_writePosition + 1) % _ringBuffer.Length;

                    // Handle overrun
                    if (_writePosition == _readPosition)
                    {
                        _readPosition = (_readPosition + 1) % _ringBuffer.Length;
                    }
                }
            }
        }

        private void OnAudioRead(float[] data)
        {
            lock (_bufferLock)
            {
                int available = AvailableSamples();

                if (available < data.Length)
                {
                    // Underrun - fill with silence
                    Array.Clear(data, 0, data.Length);
                    return;
                }

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = _ringBuffer[_readPosition];
                    _readPosition = (_readPosition + 1) % _ringBuffer.Length;
                }
            }
        }

        private int AvailableSamples()
        {
            if (_writePosition >= _readPosition)
            {
                return _writePosition - _readPosition;
            }
            return _ringBuffer.Length - _readPosition + _writePosition;
        }

        private void OnDestroy()
        {
            Dispose();
        }

        /// <summary>
        /// Clean up resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _isPlaying = false;
            IsSpeaking = false;

            // Detach from parent
            AttachedTo = null;

            // Clear buffers safely
            lock (_bufferLock)
            {
                if (_ringBuffer != null)
                {
                    Array.Clear(_ringBuffer, 0, _ringBuffer.Length);
                }
                _writePosition = 0;
                _readPosition = 0;
            }

            lock (_jitterBuffer)
            {
                _jitterBuffer.Clear();
            }

            // Destroy Unity objects - use DestroyImmediate in Editor/tests
            if (_playbackClip != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_playbackClip);
                }
                else
                {
                    DestroyImmediate(_playbackClip);
                }
                _playbackClip = null;
            }

            if (_audioSource != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_audioSource);
                }
                else
                {
                    DestroyImmediate(_audioSource);
                }
                _audioSource = null;
            }
        }
    }
}
