// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiveSQD.WebVerse.Handlers.Voice.Providers
{
    /// <summary>
    /// Mock voice provider for testing purposes.
    /// Fully synchronous to avoid deadlocks in Unity tests.
    /// </summary>
    public class MockVoiceProvider : BaseVoiceProvider
    {
        private readonly List<(byte[] data, uint seq)> _sentAudio = new List<(byte[], uint)>();
        private bool _isConnected;

        /// <summary>
        /// Whether to simulate connection failure.
        /// </summary>
        public bool SimulateConnectionFailure { get; set; }

        /// <summary>
        /// Number of connection attempts made.
        /// </summary>
        public int ConnectionAttempts { get; private set; }

        /// <summary>
        /// Audio data that has been sent.
        /// </summary>
        public IReadOnlyList<(byte[] data, uint seq)> SentAudio => _sentAudio;

        /// <summary>
        /// Clear sent audio history.
        /// </summary>
        public void ClearSentAudio()
        {
            _sentAudio.Clear();
        }

        /// <summary>
        /// Simulate receiving audio from a remote user.
        /// </summary>
        public void SimulateAudioReceived(string userId, byte[] opusData, uint sequenceNumber)
        {
            RaiseAudioReceived(userId, opusData, sequenceNumber);
        }

        /// <summary>
        /// Simulate a user joining.
        /// </summary>
        public void SimulateUserJoined(string userId)
        {
            RaiseUserJoined(userId);
        }

        /// <summary>
        /// Simulate a user leaving.
        /// </summary>
        public void SimulateUserLeft(string userId)
        {
            RaiseUserLeft(userId);
        }

        /// <summary>
        /// Simulate user speaking state change.
        /// </summary>
        public void SimulateUserSpeaking(string userId, bool isSpeaking)
        {
            RaiseUserSpeaking(userId, isSpeaking);
        }

        /// <summary>
        /// Simulate connection loss.
        /// </summary>
        public void SimulateConnectionLostNow(string reason = null)
        {
            _isConnected = false;
            OnConnectionLost(reason);
        }

        /// <summary>
        /// Force transition to error state.
        /// </summary>
        public void SimulateError(string reason)
        {
            _isConnected = false;
            TransitionToError(reason);
        }

        /// <inheritdoc />
        protected override Task ConnectInternalAsync(VoiceConfig config)
        {
            ConnectionAttempts++;

            if (SimulateConnectionFailure)
            {
                throw new Exception("Simulated connection failure");
            }

            _isConnected = true;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override Task DisconnectInternalAsync()
        {
            _isConnected = false;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task SendAudioAsync(byte[] opusData, uint sequenceNumber)
        {
            if (!_isConnected)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_CONNECTION_LOST, "Not connected");
            }

            _sentAudio.Add((opusData, sequenceNumber));
            return Task.CompletedTask;
        }
    }
}
