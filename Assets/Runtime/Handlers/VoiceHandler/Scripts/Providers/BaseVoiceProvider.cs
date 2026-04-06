// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Voice.Providers
{
    /// <summary>
    /// Base class for voice providers implementing connection state machine.
    /// </summary>
    public abstract class BaseVoiceProvider : IVoiceProvider
    {
        private VoiceConnectionState _state = VoiceConnectionState.Disconnected;
        private readonly object _stateLock = new object();
        private CancellationTokenSource _reconnectCts;
        private bool _disposed;

        // Reconnection constants
        private const int AggressiveRetryCount = 3;
        private const int AggressiveRetryDelayMs = 500;
        private const int InitialBackoffMs = 1000;
        private const int MaxBackoffMs = 30000;
        private const int MaxTotalRetries = 10;

        /// <summary>
        /// Whether automatic reconnection is enabled.
        /// </summary>
        public bool AutoReconnectEnabled { get; set; } = true;

        /// <summary>
        /// Current voice configuration.
        /// </summary>
        protected VoiceConfig Config { get; private set; }

        /// <summary>
        /// Current connection state.
        /// </summary>
        public VoiceConnectionState State
        {
            get { lock (_stateLock) { return _state; } }
            protected set
            {
                VoiceConnectionState previousState;
                lock (_stateLock)
                {
                    if (_state == value) return;
                    previousState = _state;
                    _state = value;
                }
                OnStateChanged?.Invoke(this, new VoiceStateChangedEventArgs(previousState, value));
            }
        }

        /// <inheritdoc />
        public event EventHandler<VoiceStateChangedEventArgs> OnStateChanged;

        /// <inheritdoc />
        public event EventHandler<VoiceAudioReceivedEventArgs> OnAudioReceived;

        /// <inheritdoc />
        public event EventHandler<VoiceUserEventArgs> OnUserJoined;

        /// <inheritdoc />
        public event EventHandler<VoiceUserEventArgs> OnUserLeft;

        /// <inheritdoc />
        public event EventHandler<VoiceUserSpeakingEventArgs> OnUserSpeaking;

        /// <inheritdoc />
        public async Task ConnectAsync(VoiceConfig config)
        {
            if (config == null)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG, "VoiceConfig cannot be null.");
            }

            config.Validate();
            Config = config;

            if (State == VoiceConnectionState.Connected || State == VoiceConnectionState.Connecting)
            {
                Logging.LogWarning("[BaseVoiceProvider->ConnectAsync] Already connected or connecting.");
                return;
            }

            CancelReconnection();
            State = VoiceConnectionState.Connecting;

            try
            {
                await ConnectInternalAsync(config);
                State = VoiceConnectionState.Connected;
                Logging.Log($"[BaseVoiceProvider] Connected to {config.Endpoint}");
            }
            catch (Exception ex)
            {
                Logging.LogError($"[BaseVoiceProvider->ConnectAsync] Connection failed: {ex.Message}");
                State = VoiceConnectionState.Error;
                throw new VoiceException(VoiceErrorCode.VOICE_CONNECTION_FAILED,
                    $"Failed to connect to voice server: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task DisconnectAsync()
        {
            CancelReconnection();

            if (State == VoiceConnectionState.Disconnected)
            {
                return;
            }

            try
            {
                await DisconnectInternalAsync();
            }
            catch (Exception ex)
            {
                Logging.LogWarning($"[BaseVoiceProvider->DisconnectAsync] Error during disconnect: {ex.Message}");
            }
            finally
            {
                State = VoiceConnectionState.Disconnected;
            }
        }

        /// <inheritdoc />
        public abstract Task SendAudioAsync(byte[] opusData, uint sequenceNumber);

        /// <summary>
        /// Internal connection implementation.
        /// </summary>
        protected abstract Task ConnectInternalAsync(VoiceConfig config);

        /// <summary>
        /// Internal disconnection implementation.
        /// </summary>
        protected abstract Task DisconnectInternalAsync();

        /// <summary>
        /// Called when connection is lost unexpectedly.
        /// Triggers reconnection logic.
        /// </summary>
        protected void OnConnectionLost(string reason = null)
        {
            if (State == VoiceConnectionState.Disconnected || State == VoiceConnectionState.Error)
            {
                return;
            }

            Logging.LogWarning($"[BaseVoiceProvider] Connection lost: {reason ?? "Unknown reason"}");
            State = VoiceConnectionState.Reconnecting;
            StartReconnection();
        }

        /// <summary>
        /// Raise the OnAudioReceived event.
        /// </summary>
        protected void RaiseAudioReceived(string userId, byte[] opusData, uint sequenceNumber)
        {
            OnAudioReceived?.Invoke(this, new VoiceAudioReceivedEventArgs(userId, opusData, sequenceNumber));
        }

        /// <summary>
        /// Raise the OnUserJoined event.
        /// </summary>
        protected void RaiseUserJoined(string userId)
        {
            OnUserJoined?.Invoke(this, new VoiceUserEventArgs(userId));
        }

        /// <summary>
        /// Raise the OnUserLeft event.
        /// </summary>
        protected void RaiseUserLeft(string userId)
        {
            OnUserLeft?.Invoke(this, new VoiceUserEventArgs(userId));
        }

        /// <summary>
        /// Raise the OnUserSpeaking event.
        /// </summary>
        protected void RaiseUserSpeaking(string userId, bool isSpeaking)
        {
            OnUserSpeaking?.Invoke(this, new VoiceUserSpeakingEventArgs(userId, isSpeaking));
        }

        private void StartReconnection()
        {
            if (!AutoReconnectEnabled)
            {
                TransitionToError("Reconnection disabled");
                return;
            }

            CancelReconnection();
            _reconnectCts = new CancellationTokenSource();
            _ = ReconnectLoopAsync(_reconnectCts.Token);
        }

        private void CancelReconnection()
        {
            if (_reconnectCts != null)
            {
                _reconnectCts.Cancel();
                _reconnectCts.Dispose();
                _reconnectCts = null;
            }
        }

        private async Task ReconnectLoopAsync(CancellationToken ct)
        {
            int totalAttempts = 0;

            // Phase 1: Aggressive retries
            for (int i = 0; i < AggressiveRetryCount && !ct.IsCancellationRequested; i++)
            {
                totalAttempts++;
                Logging.Log($"[BaseVoiceProvider] Reconnection attempt {i + 1}/{AggressiveRetryCount} (aggressive phase)");

                if (await TryReconnectAsync(ct))
                {
                    return;
                }

                try
                {
                    await Task.Delay(AggressiveRetryDelayMs, ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }

            // Phase 2: Exponential backoff (with max retries)
            int backoffMs = InitialBackoffMs;
            while (!ct.IsCancellationRequested && totalAttempts < MaxTotalRetries)
            {
                totalAttempts++;
                Logging.Log($"[BaseVoiceProvider] Reconnection attempt {totalAttempts}/{MaxTotalRetries} (backoff: {backoffMs}ms)");

                if (await TryReconnectAsync(ct))
                {
                    return;
                }

                try
                {
                    await Task.Delay(backoffMs, ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                backoffMs = Math.Min(backoffMs * 2, MaxBackoffMs);
            }

            // Max retries exceeded
            if (!ct.IsCancellationRequested)
            {
                TransitionToError("Max reconnection attempts exceeded");
            }
        }

        private async Task<bool> TryReconnectAsync(CancellationToken ct)
        {
            if (ct.IsCancellationRequested || Config == null)
            {
                return false;
            }

            try
            {
                await ConnectInternalAsync(Config);
                State = VoiceConnectionState.Connected;
                Logging.Log("[BaseVoiceProvider] Reconnection successful");
                return true;
            }
            catch (Exception ex)
            {
                Logging.LogWarning($"[BaseVoiceProvider] Reconnection attempt failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Transition to error state and stop reconnection.
        /// </summary>
        protected void TransitionToError(string reason)
        {
            CancelReconnection();
            State = VoiceConnectionState.Error;
            Logging.LogError($"[BaseVoiceProvider] Error: {reason}");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                CancelReconnection();
                _ = DisconnectAsync();
            }

            _disposed = true;
        }
    }
}
