// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Threading.Tasks;

namespace FiveSQD.WebVerse.Handlers.Voice.Providers
{
    /// <summary>
    /// Event arguments for audio received from a remote user.
    /// </summary>
    public class VoiceAudioReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The user ID of the sender.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// The Opus-encoded audio data.
        /// </summary>
        public byte[] OpusData { get; }

        /// <summary>
        /// Sequence number for ordering.
        /// </summary>
        public uint SequenceNumber { get; }

        public VoiceAudioReceivedEventArgs(string userId, byte[] opusData, uint sequenceNumber)
        {
            UserId = userId;
            OpusData = opusData;
            SequenceNumber = sequenceNumber;
        }
    }

    /// <summary>
    /// Event arguments for user join/leave events.
    /// </summary>
    public class VoiceUserEventArgs : EventArgs
    {
        /// <summary>
        /// The user ID.
        /// </summary>
        public string UserId { get; }

        public VoiceUserEventArgs(string userId)
        {
            UserId = userId;
        }
    }

    /// <summary>
    /// Event arguments for user speaking state changes.
    /// </summary>
    public class VoiceUserSpeakingEventArgs : EventArgs
    {
        /// <summary>
        /// The user ID.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Whether the user is currently speaking.
        /// </summary>
        public bool IsSpeaking { get; }

        public VoiceUserSpeakingEventArgs(string userId, bool isSpeaking)
        {
            UserId = userId;
            IsSpeaking = isSpeaking;
        }
    }

    /// <summary>
    /// Event arguments for connection state changes.
    /// </summary>
    public class VoiceStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The previous connection state.
        /// </summary>
        public VoiceConnectionState PreviousState { get; }

        /// <summary>
        /// The new connection state.
        /// </summary>
        public VoiceConnectionState NewState { get; }

        /// <summary>
        /// Optional reason for the state change.
        /// </summary>
        public string Reason { get; }

        public VoiceStateChangedEventArgs(VoiceConnectionState previousState, VoiceConnectionState newState, string reason = null)
        {
            PreviousState = previousState;
            NewState = newState;
            Reason = reason;
        }
    }

    /// <summary>
    /// Interface for voice transport providers.
    /// Abstracts the underlying transport mechanism (WebSocket, WebRTC, etc.).
    /// </summary>
    public interface IVoiceProvider : IDisposable
    {
        /// <summary>
        /// Current connection state.
        /// </summary>
        VoiceConnectionState State { get; }

        /// <summary>
        /// Fired when the connection state changes.
        /// </summary>
        event EventHandler<VoiceStateChangedEventArgs> OnStateChanged;

        /// <summary>
        /// Fired when audio data is received from a remote user.
        /// </summary>
        event EventHandler<VoiceAudioReceivedEventArgs> OnAudioReceived;

        /// <summary>
        /// Fired when a user joins the voice channel.
        /// </summary>
        event EventHandler<VoiceUserEventArgs> OnUserJoined;

        /// <summary>
        /// Fired when a user leaves the voice channel.
        /// </summary>
        event EventHandler<VoiceUserEventArgs> OnUserLeft;

        /// <summary>
        /// Fired when a user's speaking state changes.
        /// </summary>
        event EventHandler<VoiceUserSpeakingEventArgs> OnUserSpeaking;

        /// <summary>
        /// Connect to the voice server.
        /// </summary>
        /// <param name="config">Voice configuration with endpoint and settings.</param>
        /// <returns>Task that completes when connected.</returns>
        Task ConnectAsync(VoiceConfig config);

        /// <summary>
        /// Disconnect from the voice server.
        /// </summary>
        /// <returns>Task that completes when disconnected.</returns>
        Task DisconnectAsync();

        /// <summary>
        /// Send encoded audio data to the voice server.
        /// </summary>
        /// <param name="opusData">Opus-encoded audio frame.</param>
        /// <param name="sequenceNumber">Sequence number for ordering.</param>
        /// <returns>Task that completes when data is sent.</returns>
        Task SendAudioAsync(byte[] opusData, uint sequenceNumber);
    }
}
