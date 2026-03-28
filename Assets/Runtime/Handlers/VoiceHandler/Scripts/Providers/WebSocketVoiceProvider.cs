// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Text;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
#if USE_BESTHTTP
using FiveSQD.WebVerse.WebInterface.WebSocket;
#endif

namespace FiveSQD.WebVerse.Handlers.Voice.Providers
{
    /// <summary>
    /// Voice provider using WebSocket transport.
    /// </summary>
    public class WebSocketVoiceProvider : BaseVoiceProvider
    {
#if USE_BESTHTTP
        private WebSocket _webSocket;
#endif
        private TaskCompletionSource<bool> _connectTcs;
        private bool _intentionalDisconnect;

        // Protocol message types
        private const byte MSG_TYPE_AUDIO = 0x01;
        private const byte MSG_TYPE_USER_JOINED = 0x02;
        private const byte MSG_TYPE_USER_LEFT = 0x03;
        private const byte MSG_TYPE_USER_SPEAKING = 0x04;

        /// <inheritdoc />
        protected override Task ConnectInternalAsync(VoiceConfig config)
        {
#if USE_BESTHTTP
            _intentionalDisconnect = false;
            _connectTcs = new TaskCompletionSource<bool>();

            try
            {
                _webSocket = new WebSocket(
                    config.Endpoint,
                    OnWebSocketOpen,
                    OnWebSocketClosed,
                    OnWebSocketBinary,
                    OnWebSocketMessage,
                    OnWebSocketError
                );

                _webSocket.Open();
            }
            catch (Exception ex)
            {
                _connectTcs.TrySetException(ex);
            }

            return _connectTcs.Task;
#else
            return Task.FromException(new VoiceException(
                VoiceErrorCode.VOICE_CONNECTION_FAILED,
                "WebSocket support requires USE_BESTHTTP define."));
#endif
        }

        /// <inheritdoc />
        protected override Task DisconnectInternalAsync()
        {
#if USE_BESTHTTP
            _intentionalDisconnect = true;

            if (_webSocket != null && _webSocket.isOpen)
            {
                _webSocket.Close();
            }

            _webSocket = null;
#endif
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task SendAudioAsync(byte[] opusData, uint sequenceNumber)
        {
#if USE_BESTHTTP
            if (_webSocket == null || !_webSocket.isOpen)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_CONNECTION_LOST, "WebSocket not connected");
            }

            // Build audio packet: [type(1)][seq(4)][data(n)]
            byte[] packet = new byte[1 + 4 + opusData.Length];
            packet[0] = MSG_TYPE_AUDIO;
            packet[1] = (byte)(sequenceNumber >> 24);
            packet[2] = (byte)(sequenceNumber >> 16);
            packet[3] = (byte)(sequenceNumber >> 8);
            packet[4] = (byte)sequenceNumber;
            Array.Copy(opusData, 0, packet, 5, opusData.Length);

            _webSocket.Send(packet);
            return Task.CompletedTask;
#else
            return Task.FromException(new VoiceException(
                VoiceErrorCode.VOICE_CONNECTION_FAILED,
                "WebSocket support requires USE_BESTHTTP define."));
#endif
        }

#if USE_BESTHTTP
        private void OnWebSocketOpen(WebSocket ws)
        {
            Logging.Log("[WebSocketVoiceProvider] WebSocket connected");
            _connectTcs?.TrySetResult(true);
        }

        private void OnWebSocketClosed(WebSocket ws, ushort code, string message)
        {
            Logging.Log($"[WebSocketVoiceProvider] WebSocket closed: {code} - {message}");

            if (!_intentionalDisconnect)
            {
                OnConnectionLost($"WebSocket closed: {code} - {message}");
            }
        }

        private void OnWebSocketBinary(WebSocket ws, byte[] data)
        {
            if (data == null || data.Length < 1)
            {
                return;
            }

            byte msgType = data[0];

            switch (msgType)
            {
                case MSG_TYPE_AUDIO:
                    HandleAudioMessage(data);
                    break;

                case MSG_TYPE_USER_JOINED:
                    HandleUserJoinedMessage(data);
                    break;

                case MSG_TYPE_USER_LEFT:
                    HandleUserLeftMessage(data);
                    break;

                case MSG_TYPE_USER_SPEAKING:
                    HandleUserSpeakingMessage(data);
                    break;

                default:
                    Logging.LogWarning($"[WebSocketVoiceProvider] Unknown message type: {msgType}");
                    break;
            }
        }

        private void OnWebSocketMessage(WebSocket ws, string message)
        {
            // Text messages are used for control messages (JSON)
            Logging.LogDebug($"[WebSocketVoiceProvider] Received text message: {message}");
        }

        private void OnWebSocketError(WebSocket ws, string error)
        {
            Logging.LogError($"[WebSocketVoiceProvider] WebSocket error: {error}");
            _connectTcs?.TrySetException(new Exception(error));

            if (!_intentionalDisconnect)
            {
                OnConnectionLost($"WebSocket error: {error}");
            }
        }

        private void HandleAudioMessage(byte[] data)
        {
            // Format: [type(1)][userId length(1)][userId(n)][seq(4)][opusData(n)]
            if (data.Length < 7)
            {
                Logging.LogWarning("[WebSocketVoiceProvider] Audio message too short");
                return;
            }

            int offset = 1;
            int userIdLength = data[offset++];

            if (data.Length < 2 + userIdLength + 4)
            {
                Logging.LogWarning("[WebSocketVoiceProvider] Audio message malformed");
                return;
            }

            string userId = Encoding.UTF8.GetString(data, offset, userIdLength);
            offset += userIdLength;

            uint sequenceNumber = (uint)(
                (data[offset] << 24) |
                (data[offset + 1] << 16) |
                (data[offset + 2] << 8) |
                data[offset + 3]
            );
            offset += 4;

            int opusLength = data.Length - offset;
            byte[] opusData = new byte[opusLength];
            Array.Copy(data, offset, opusData, 0, opusLength);

            RaiseAudioReceived(userId, opusData, sequenceNumber);
        }

        private void HandleUserJoinedMessage(byte[] data)
        {
            // Format: [type(1)][userId length(1)][userId(n)]
            if (data.Length < 2)
            {
                return;
            }

            int userIdLength = data[1];
            if (data.Length < 2 + userIdLength)
            {
                return;
            }

            string userId = Encoding.UTF8.GetString(data, 2, userIdLength);
            RaiseUserJoined(userId);
        }

        private void HandleUserLeftMessage(byte[] data)
        {
            // Format: [type(1)][userId length(1)][userId(n)]
            if (data.Length < 2)
            {
                return;
            }

            int userIdLength = data[1];
            if (data.Length < 2 + userIdLength)
            {
                return;
            }

            string userId = Encoding.UTF8.GetString(data, 2, userIdLength);
            RaiseUserLeft(userId);
        }

        private void HandleUserSpeakingMessage(byte[] data)
        {
            // Format: [type(1)][userId length(1)][userId(n)][isSpeaking(1)]
            if (data.Length < 3)
            {
                return;
            }

            int userIdLength = data[1];
            if (data.Length < 3 + userIdLength)
            {
                return;
            }

            string userId = Encoding.UTF8.GetString(data, 2, userIdLength);
            bool isSpeaking = data[2 + userIdLength] != 0;
            RaiseUserSpeaking(userId, isSpeaking);
        }
#endif

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if USE_BESTHTTP
                if (_webSocket != null)
                {
                    _intentionalDisconnect = true;
                    _webSocket.Close();
                    _webSocket = null;
                }
#endif
            }

            base.Dispose(disposing);
        }
    }
}
