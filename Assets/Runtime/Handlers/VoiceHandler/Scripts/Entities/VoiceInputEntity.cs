// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Voice.Audio;

namespace FiveSQD.WebVerse.Handlers.Voice.Entities
{
    /// <summary>
    /// Entity for capturing microphone input and encoding it for voice transmission.
    /// </summary>
    public class VoiceInputEntity : MonoBehaviour
    {
        /// <summary>
        /// Event fired when encoded audio data is ready to transmit.
        /// </summary>
        public event Action<byte[], uint> OnAudioEncoded;

        /// <summary>
        /// Event fired when speaking state changes.
        /// </summary>
        public event Action<bool> OnSpeakingChanged;

        /// <summary>
        /// Whether the microphone is currently capturing.
        /// </summary>
        public bool IsCapturing { get; private set; }

        /// <summary>
        /// Whether the user is currently speaking (based on VAD).
        /// </summary>
        public bool IsSpeaking { get; private set; }

        /// <summary>
        /// Whether the microphone is muted.
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        /// Voice Activity Detection threshold (0-1).
        /// </summary>
        public float VADThreshold { get; set; } = 0.01f;

        /// <summary>
        /// Sample rate in Hz.
        /// </summary>
        public int SampleRate => _codec?.SampleRate ?? OpusCodec.DefaultSampleRate;

        /// <summary>
        /// Frame size in samples.
        /// </summary>
        public int FrameSize => _codec?.FrameSize ?? 960;

        /// <summary>
        /// Name of the microphone device being used.
        /// </summary>
        public string DeviceName { get; private set; }

        private IOpusCodec _codec;
        private AudioClip _microphoneClip;
        private int _lastSamplePosition;
        private uint _sequenceNumber;
        private bool _disposed;

        // Circular buffer for accumulating samples
        private readonly Queue<float> _sampleBuffer = new Queue<float>();

        // VAD state
        private int _silenceFrames;
        private const int SilenceFramesBeforeStop = 15; // ~300ms at 20ms frames

        /// <summary>
        /// Initialize the voice input entity.
        /// </summary>
        /// <param name="codec">Opus codec to use for encoding.</param>
        /// <param name="deviceName">Microphone device name (null for default).</param>
        public void Initialize(IOpusCodec codec, string deviceName = null)
        {
            if (codec == null)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG, "Codec cannot be null.");
            }

            _codec = codec;
            DeviceName = deviceName;
            _sequenceNumber = 0;

            Logging.Log($"[VoiceInputEntity] Initialized with device: {deviceName ?? "default"}");
        }

        /// <summary>
        /// Start capturing from the microphone.
        /// </summary>
        /// <returns>True if capture started successfully.</returns>
        public bool StartCapture()
        {
#if UNITY_WEBGL
            return false;
#else
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(VoiceInputEntity));
            }

            if (IsCapturing)
            {
                Logging.LogWarning("[VoiceInputEntity->StartCapture] Already capturing.");
                return true;
            }

            if (!HasMicrophonePermission())
            {
                throw new VoiceException(VoiceErrorCode.VOICE_PERMISSION_DENIED,
                    "Microphone permission not granted.");
            }

            string[] devices = Microphone.devices;
            if (devices.Length == 0)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG,
                    "No microphone devices available.");
            }

            // Use specified device or default
            string device = DeviceName;
            if (string.IsNullOrEmpty(device) || !Array.Exists(devices, d => d == device))
            {
                device = null; // Use default device
            }

            try
            {
                // Start recording: 1 second buffer, looping
                _microphoneClip = Microphone.Start(device, true, 1, SampleRate);
                if (_microphoneClip == null)
                {
                    throw new VoiceException(VoiceErrorCode.VOICE_ENCODING_ERROR,
                        "Failed to start microphone.");
                }

                _lastSamplePosition = 0;
                _sampleBuffer.Clear();
                IsCapturing = true;

                Logging.Log($"[VoiceInputEntity] Started capture on device: {device ?? "default"}");
                return true;
            }
            catch (VoiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_ENCODING_ERROR,
                    $"Failed to start microphone: {ex.Message}", ex);
            }
#endif
        }

        /// <summary>
        /// Stop capturing from the microphone.
        /// </summary>
        public void StopCapture()
        {
#if UNITY_WEBGL
            
#else
            if (!IsCapturing)
            {
                return;
            }

            try
            {
                Microphone.End(DeviceName);
            }
            catch (Exception ex)
            {
                Logging.LogWarning($"[VoiceInputEntity->StopCapture] Error stopping microphone: {ex.Message}");
            }

            if (_microphoneClip != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_microphoneClip);
                }
                else
                {
                    DestroyImmediate(_microphoneClip);
                }
                _microphoneClip = null;
            }

            _sampleBuffer.Clear();
            IsCapturing = false;

            if (IsSpeaking)
            {
                IsSpeaking = false;
                OnSpeakingChanged?.Invoke(false);
            }

            Logging.Log("[VoiceInputEntity] Stopped capture.");
#endif
        }

        /// <summary>
        /// Check if microphone permission is available.
        /// </summary>
        public bool HasMicrophonePermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                UnityEngine.Android.Permission.Microphone);
#elif UNITY_IOS && !UNITY_EDITOR
            return Application.HasUserAuthorization(UserAuthorization.Microphone);
#elif UNITY_WEBGL && !UNITY_EDITOR
            return false;
#else
            // On desktop/WebGL, assume permission is available if devices exist
            return Microphone.devices.Length > 0;
#endif
        }

        /// <summary>
        /// Request microphone permission (async on mobile platforms).
        /// </summary>
        public void RequestMicrophonePermission(Action<bool> callback)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (HasMicrophonePermission())
            {
                callback?.Invoke(true);
                return;
            }

            var callbacks = new UnityEngine.Android.PermissionCallbacks();
            callbacks.PermissionGranted += (permission) => callback?.Invoke(true);
            callbacks.PermissionDenied += (permission) => callback?.Invoke(false);
            callbacks.PermissionDeniedAndDontAskAgain += (permission) => callback?.Invoke(false);

            UnityEngine.Android.Permission.RequestUserPermission(
                UnityEngine.Android.Permission.Microphone, callbacks);
#elif UNITY_IOS && !UNITY_EDITOR
            StartCoroutine(RequestIOSMicrophonePermission(callback));
#elif UNITY_WEBGL

#else
            callback?.Invoke(HasMicrophonePermission());
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        private System.Collections.IEnumerator RequestIOSMicrophonePermission(Action<bool> callback)
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            callback?.Invoke(Application.HasUserAuthorization(UserAuthorization.Microphone));
        }
#endif

        /// <summary>
        /// Get available microphone devices.
        /// </summary>
        public string[] GetAvailableDevices()
        {
#if UNITY_WEBGL
            return new string[]{};
#else
            return Microphone.devices;
#endif
        }

        private void Update()
        {
            if (!IsCapturing || _microphoneClip == null || IsMuted)
            {
                return;
            }

            ProcessMicrophoneData();
        }

        private void ProcessMicrophoneData()
        {
#if !UNITY_WEBGL
            int currentPosition = Microphone.GetPosition(DeviceName);

            if (currentPosition < 0)
            {
                // Microphone not ready
                return;
            }

            // Calculate how many new samples are available
            int samplesAvailable;
            if (currentPosition >= _lastSamplePosition)
            {
                samplesAvailable = currentPosition - _lastSamplePosition;
            }
            else
            {
                // Wrapped around
                samplesAvailable = (_microphoneClip.samples - _lastSamplePosition) + currentPosition;
            }

            if (samplesAvailable <= 0)
            {
                return;
            }

            // Read new samples
            float[] newSamples = new float[samplesAvailable];
            _microphoneClip.GetData(newSamples, _lastSamplePosition);
            _lastSamplePosition = currentPosition;

            // Add to buffer
            foreach (float sample in newSamples)
            {
                _sampleBuffer.Enqueue(sample);
            }

            // Process full frames
            int frameSize = FrameSize;
            while (_sampleBuffer.Count >= frameSize)
            {
                float[] frameSamples = new float[frameSize];
                for (int i = 0; i < frameSize; i++)
                {
                    frameSamples[i] = _sampleBuffer.Dequeue();
                }

                ProcessFrame(frameSamples);
            }
#endif
        }

        private void ProcessFrame(float[] frameSamples)
        {
            // Voice Activity Detection
            float energy = CalculateEnergy(frameSamples);
            bool voiceDetected = energy > VADThreshold;

            // Update speaking state with hysteresis
            if (voiceDetected)
            {
                _silenceFrames = 0;
                if (!IsSpeaking)
                {
                    IsSpeaking = true;
                    OnSpeakingChanged?.Invoke(true);
                }
            }
            else
            {
                _silenceFrames++;
                if (IsSpeaking && _silenceFrames >= SilenceFramesBeforeStop)
                {
                    IsSpeaking = false;
                    OnSpeakingChanged?.Invoke(false);
                }
            }

            // Only encode and transmit if speaking
            if (IsSpeaking)
            {
                // Convert float samples to short (16-bit PCM)
                short[] pcmData = ConvertToShort(frameSamples);

                try
                {
                    byte[] encoded = _codec.Encode(pcmData);
                    _sequenceNumber++;
                    OnAudioEncoded?.Invoke(encoded, _sequenceNumber);
                }
                catch (Exception ex)
                {
                    Logging.LogWarning($"[VoiceInputEntity->ProcessFrame] Encoding failed: {ex.Message}");
                }
            }
        }

        private float CalculateEnergy(float[] samples)
        {
            float sum = 0f;
            foreach (float sample in samples)
            {
                sum += sample * sample;
            }
            return sum / samples.Length;
        }

        private short[] ConvertToShort(float[] floatSamples)
        {
            short[] shortSamples = new short[floatSamples.Length];
            for (int i = 0; i < floatSamples.Length; i++)
            {
                float clamped = Mathf.Clamp(floatSamples[i], -1f, 1f);
                shortSamples[i] = (short)(clamped * short.MaxValue);
            }
            return shortSamples;
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

            StopCapture();
            _disposed = true;
        }
    }
}
