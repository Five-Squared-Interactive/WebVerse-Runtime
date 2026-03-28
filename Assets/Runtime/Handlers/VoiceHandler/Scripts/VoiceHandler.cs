// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Voice.Providers;
using FiveSQD.WebVerse.Handlers.Voice.Audio;
using FiveSQD.WebVerse.Handlers.Voice.Entities;

namespace FiveSQD.WebVerse.Handlers.Voice
{
    /// <summary>
    /// Handler for spatial voice chat functionality.
    /// </summary>
    public class VoiceHandler : BaseHandler
    {
        /// <summary>
        /// The current voice configuration.
        /// </summary>
        public VoiceConfig Config { get; private set; }

        /// <summary>
        /// The current connection state.
        /// </summary>
        public VoiceConnectionState State { get; private set; } = VoiceConnectionState.Disconnected;

        /// <summary>
        /// The voice provider for transport.
        /// </summary>
        public IVoiceProvider Provider { get; private set; }

        /// <summary>
        /// The Opus codec for encoding/decoding.
        /// </summary>
        public IOpusCodec Codec { get; private set; }

        /// <summary>
        /// The local voice input entity (microphone).
        /// </summary>
        public VoiceInputEntity InputEntity { get; private set; }

        /// <summary>
        /// Whether microphone capture is enabled.
        /// </summary>
        public bool IsCaptureEnabled => InputEntity?.IsCapturing ?? false;

        /// <summary>
        /// Whether the local user is speaking.
        /// </summary>
        public bool IsLocalSpeaking => InputEntity?.IsSpeaking ?? false;

        /// <summary>
        /// Remote voice speakers keyed by user ID.
        /// </summary>
        private readonly Dictionary<string, VoiceSpeakerEntity> _remoteSpeakers =
            new Dictionary<string, VoiceSpeakerEntity>();

        /// <summary>
        /// GameObject for managing voice entities.
        /// </summary>
        private GameObject _voiceEntitiesRoot;

        /// <summary>
        /// Fired when the connection state changes.
        /// </summary>
        public event EventHandler<VoiceStateChangedEventArgs> OnStateChanged;

        /// <summary>
        /// Fired when audio is received from a remote user.
        /// </summary>
        public event EventHandler<VoiceAudioReceivedEventArgs> OnAudioReceived;

        /// <summary>
        /// Fired when a user joins the voice channel.
        /// </summary>
        public event EventHandler<VoiceUserEventArgs> OnUserJoined;

        /// <summary>
        /// Fired when a user leaves the voice channel.
        /// </summary>
        public event EventHandler<VoiceUserEventArgs> OnUserLeft;

        /// <summary>
        /// Fired when a user's speaking state changes.
        /// </summary>
        public event EventHandler<VoiceUserSpeakingEventArgs> OnUserSpeaking;

        /// <summary>
        /// Fired when local speaking state changes.
        /// </summary>
        public event Action<bool> OnLocalSpeakingChanged;

        /// <summary>
        /// Initialize the voice handler without configuration.
        /// Logs an error - use Initialize(VoiceConfig) instead.
        /// </summary>
        public override void Initialize()
        {
            Logging.LogError("[VoiceHandler->Initialize] Initialize must be called with VoiceConfig.");
        }

        /// <summary>
        /// Initialize the voice handler with configuration.
        /// </summary>
        /// <param name="config">The voice configuration.</param>
        public void Initialize(VoiceConfig config)
        {
            if (config == null)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG,
                    "VoiceConfig cannot be null.");
            }

            config.Validate();
            Config = config;
            State = VoiceConnectionState.Disconnected;

            // Create entities root
            _voiceEntitiesRoot = new GameObject("VoiceEntities");

            // Create codec
            Codec = new OpusCodec();

            // Create provider based on config
            Provider = CreateProvider(config);
            if (Provider != null)
            {
                WireProviderEvents(Provider);
            }

            // Create input entity
            GameObject inputGO = new GameObject("VoiceInput");
            inputGO.transform.SetParent(_voiceEntitiesRoot.transform);
            InputEntity = inputGO.AddComponent<VoiceInputEntity>();
            InputEntity.Initialize(Codec);
            InputEntity.OnAudioEncoded += OnLocalAudioEncoded;
            InputEntity.OnSpeakingChanged += OnLocalSpeakingStateChanged;

            Logging.Log($"[VoiceHandler] Initialized with endpoint: {config.Endpoint}");
            base.Initialize();
        }

        /// <summary>
        /// Connect to the voice server.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (Provider == null)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_CONNECTION_FAILED,
                    "Voice provider not initialized.");
            }

            if (Config == null)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG,
                    "Voice not configured.");
            }

            await Provider.ConnectAsync(Config);
        }

        /// <summary>
        /// Disconnect from the voice server.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (Provider != null)
            {
                await Provider.DisconnectAsync();
            }
        }

        /// <summary>
        /// Send audio data to the voice server.
        /// </summary>
        /// <param name="opusData">Opus-encoded audio frame.</param>
        /// <param name="sequenceNumber">Sequence number for ordering.</param>
        public async Task SendAudioAsync(byte[] opusData, uint sequenceNumber)
        {
            if (Provider == null || State != VoiceConnectionState.Connected)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_CONNECTION_LOST,
                    "Not connected to voice server.");
            }

            await Provider.SendAudioAsync(opusData, sequenceNumber);
        }

        /// <summary>
        /// Start microphone capture.
        /// </summary>
        /// <returns>True if capture started successfully.</returns>
        public bool StartCapture()
        {
            if (InputEntity == null)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG,
                    "Voice input not initialized.");
            }

            return InputEntity.StartCapture();
        }

        /// <summary>
        /// Stop microphone capture.
        /// </summary>
        public void StopCapture()
        {
            InputEntity?.StopCapture();
        }

        /// <summary>
        /// Set microphone mute state.
        /// </summary>
        /// <param name="muted">Whether to mute the microphone.</param>
        public void SetMuted(bool muted)
        {
            if (InputEntity != null)
            {
                InputEntity.IsMuted = muted;
            }
        }

        /// <summary>
        /// Get or create a speaker entity for a remote user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The VoiceSpeakerEntity for the user.</returns>
        public VoiceSpeakerEntity GetOrCreateSpeaker(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            if (_remoteSpeakers.TryGetValue(userId, out VoiceSpeakerEntity existing))
            {
                return existing;
            }

            // Create new speaker
            GameObject speakerGO = new GameObject($"VoiceSpeaker_{userId}");
            speakerGO.transform.SetParent(_voiceEntitiesRoot.transform);
            VoiceSpeakerEntity speaker = speakerGO.AddComponent<VoiceSpeakerEntity>();
            speaker.Initialize(userId, Codec, Config);
            _remoteSpeakers[userId] = speaker;

            Logging.Log($"[VoiceHandler] Created speaker for user: {userId}");
            return speaker;
        }

        /// <summary>
        /// Remove a speaker entity for a remote user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        public void RemoveSpeaker(string userId)
        {
            if (_remoteSpeakers.TryGetValue(userId, out VoiceSpeakerEntity speaker))
            {
                speaker.Dispose();
                UnityEngine.Object.Destroy(speaker.gameObject);
                _remoteSpeakers.Remove(userId);
                Logging.Log($"[VoiceHandler] Removed speaker for user: {userId}");
            }
        }

        /// <summary>
        /// Attach a user's voice speaker to a transform (e.g., a character's head).
        /// The speaker will follow the transform's position for spatial audio.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="target">Transform to attach to (e.g., character head bone).</param>
        /// <param name="offset">Local offset from the target (e.g., mouth position).</param>
        /// <returns>True if attachment succeeded.</returns>
        public bool AttachSpeakerToTransform(string userId, Transform target, Vector3 offset = default)
        {
            VoiceSpeakerEntity speaker = GetOrCreateSpeaker(userId);
            if (speaker == null)
            {
                return false;
            }

            speaker.AttachTo(target, offset);
            return true;
        }

        /// <summary>
        /// Detach a user's voice speaker from its attached transform.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        public void DetachSpeaker(string userId)
        {
            if (_remoteSpeakers.TryGetValue(userId, out VoiceSpeakerEntity speaker))
            {
                speaker.Detach();
            }
        }

        /// <summary>
        /// Set the position of a remote user's speaker.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="position">The world position.</param>
        public void SetSpeakerPosition(string userId, Vector3 position)
        {
            if (_remoteSpeakers.TryGetValue(userId, out VoiceSpeakerEntity speaker))
            {
                speaker.transform.position = position;
            }
        }

        /// <summary>
        /// Mute a specific remote user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="muted">Whether to mute the user.</param>
        public void MuteUser(string userId, bool muted)
        {
            if (_remoteSpeakers.TryGetValue(userId, out VoiceSpeakerEntity speaker))
            {
                speaker.IsMuted = muted;
            }
        }

        /// <summary>
        /// Set volume for a specific remote user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="volume">Volume level (0-1).</param>
        public void SetUserVolume(string userId, float volume)
        {
            if (_remoteSpeakers.TryGetValue(userId, out VoiceSpeakerEntity speaker))
            {
                speaker.Volume = volume;
            }
        }

        private void OnLocalAudioEncoded(byte[] opusData, uint sequenceNumber)
        {
            if (State != VoiceConnectionState.Connected || Provider == null)
            {
                return;
            }

            // Fire and forget - we don't await in event handler
            _ = Provider.SendAudioAsync(opusData, sequenceNumber);
        }

        private void OnLocalSpeakingStateChanged(bool isSpeaking)
        {
            OnLocalSpeakingChanged?.Invoke(isSpeaking);
        }

        private void OnRemoteAudioReceived(object sender, VoiceAudioReceivedEventArgs e)
        {
            VoiceSpeakerEntity speaker = GetOrCreateSpeaker(e.UserId);
            speaker?.ReceiveAudio(e.OpusData, e.SequenceNumber);
        }

        /// <summary>
        /// Terminate the voice handler and clean up resources.
        /// </summary>
        public override void Terminate()
        {
            // Clean up input entity
            if (InputEntity != null)
            {
                InputEntity.OnAudioEncoded -= OnLocalAudioEncoded;
                InputEntity.OnSpeakingChanged -= OnLocalSpeakingStateChanged;
                InputEntity.Dispose();
                InputEntity = null;
            }

            // Clean up remote speakers
            foreach (var speaker in _remoteSpeakers.Values)
            {
                speaker.Dispose();
                if (UnityEngine.Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(speaker.gameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(speaker.gameObject);
                }
            }
            _remoteSpeakers.Clear();

            // Clean up entities root
            if (_voiceEntitiesRoot != null)
            {
                if (UnityEngine.Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(_voiceEntitiesRoot);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(_voiceEntitiesRoot);
                }
                _voiceEntitiesRoot = null;
            }

            // Clean up codec
            if (Codec != null)
            {
                Codec.Dispose();
                Codec = null;
            }

            // Clean up provider
            if (Provider != null)
            {
                Provider.Dispose();
                Provider = null;
            }

            State = VoiceConnectionState.Disconnected;
            Config = null;

            base.Terminate();
        }

        /// <summary>
        /// Create a voice provider based on configuration.
        /// </summary>
        private IVoiceProvider CreateProvider(VoiceConfig config)
        {
            switch (config.Provider?.ToLowerInvariant())
            {
                case "websocket":
                default:
                    return new WebSocketVoiceProvider();
            }
        }

        /// <summary>
        /// Apply voice bindings for a user (auto-attach speaker to entity).
        /// </summary>
        /// <param name="userId">The user ID.</param>
        private void ApplyVoiceBindings(string userId)
        {
            if (Config?.Bindings == null || Config.Bindings.Count == 0)
            {
                return;
            }

            // Find matching binding for this user
            VoiceBinding matchingBinding = null;
            foreach (var binding in Config.Bindings)
            {
                if (binding.Matches(userId))
                {
                    matchingBinding = binding;
                    break;
                }
            }

            if (matchingBinding == null || string.IsNullOrEmpty(matchingBinding.ToEntityTag))
            {
                return;
            }

            // Find entity with matching tag
            if (StraightFour.StraightFour.ActiveWorld?.entityManager == null)
            {
                return;
            }

            foreach (var entity in StraightFour.StraightFour.ActiveWorld.entityManager.GetAllEntities())
            {
                if (entity.entityTag == matchingBinding.ToEntityTag)
                {
                    // Attach speaker to this entity
                    AttachSpeakerToTransform(userId, entity.transform, matchingBinding.Offset);
                    Logging.Log($"[VoiceHandler] Auto-attached speaker for user '{userId}' to entity with tag '{matchingBinding.ToEntityTag}'");
                    break;
                }
            }
        }

        /// <summary>
        /// Wire up provider events to handler events.
        /// </summary>
        private void WireProviderEvents(IVoiceProvider provider)
        {
            provider.OnStateChanged += (s, e) =>
            {
                State = e.NewState;
                OnStateChanged?.Invoke(this, e);
            };

            provider.OnAudioReceived += (s, e) =>
            {
                // Route audio to speaker entity
                OnRemoteAudioReceived(s, e);
                // Also fire public event
                OnAudioReceived?.Invoke(this, e);
            };

            provider.OnUserJoined += (s, e) =>
            {
                // Pre-create speaker for user
                GetOrCreateSpeaker(e.UserId);

                // Apply auto-bindings if configured
                ApplyVoiceBindings(e.UserId);

                OnUserJoined?.Invoke(this, e);
            };

            provider.OnUserLeft += (s, e) =>
            {
                // Remove speaker for user
                RemoveSpeaker(e.UserId);
                OnUserLeft?.Invoke(this, e);
            };

            provider.OnUserSpeaking += (s, e) =>
            {
                OnUserSpeaking?.Invoke(this, e);
            };
        }
    }
}
