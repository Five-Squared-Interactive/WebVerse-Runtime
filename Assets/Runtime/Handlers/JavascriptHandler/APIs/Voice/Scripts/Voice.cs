// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Handlers.Voice;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Voice
{
    /// <summary>
    /// JavaScript API for voice chat functionality.
    /// </summary>
    public class Voice
    {
        private static string _onConnectedCallback;
        private static string _onDisconnectedCallback;
        private static string _onUserJoinedCallback;
        private static string _onUserLeftCallback;
        private static string _onUserSpeakingCallback;
        private static string _onMuteChangedCallback;
        private static string _onErrorCallback;
        private static bool _eventsWired;

        /// <summary>
        /// Connect to the voice channel.
        /// </summary>
        /// <returns>True if connection initiated, false if not configured.</returns>
        public static bool Connect()
        {
            VoiceHandler handler = GetVoiceHandler();
            if (handler == null || handler.Config == null)
            {
                Logging.LogWarning("[Voice.Connect] Voice not configured for this world.");
                return false;
            }

            WireEvents(handler);

            _ = handler.ConnectAsync();
            return true;
        }

        /// <summary>
        /// Disconnect from the voice channel.
        /// </summary>
        /// <returns>True if disconnect initiated.</returns>
        public static bool Disconnect()
        {
            VoiceHandler handler = GetVoiceHandler();
            if (handler == null)
            {
                return false;
            }

            _ = handler.DisconnectAsync();
            return true;
        }

        /// <summary>
        /// Start microphone capture.
        /// </summary>
        /// <returns>True if capture started.</returns>
        public static bool StartCapture()
        {
            VoiceHandler handler = GetVoiceHandler();
            if (handler == null)
            {
                return false;
            }

            try
            {
                return handler.StartCapture();
            }
            catch (VoiceException ex)
            {
                Logging.LogWarning($"[Voice.StartCapture] {ex.Message}");
                InvokeCallback(_onErrorCallback, ex.ErrorCode.ToString(), ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stop microphone capture.
        /// </summary>
        public static void StopCapture()
        {
            VoiceHandler handler = GetVoiceHandler();
            handler?.StopCapture();
        }

        /// <summary>
        /// Set the local mute state.
        /// </summary>
        /// <param name="muted">True to mute, false to unmute.</param>
        public static void SetMuted(bool muted)
        {
            VoiceHandler handler = GetVoiceHandler();
            handler?.SetMuted(muted);
        }

        /// <summary>
        /// Get the current mute state.
        /// </summary>
        /// <returns>True if muted, false if unmuted.</returns>
        public static bool IsMuted()
        {
            VoiceHandler handler = GetVoiceHandler();
            if (handler?.InputEntity == null)
            {
                return true;
            }
            return handler.InputEntity.IsMuted;
        }

        /// <summary>
        /// Check if capture is currently enabled.
        /// </summary>
        /// <returns>True if capturing.</returns>
        public static bool IsCapturing()
        {
            VoiceHandler handler = GetVoiceHandler();
            return handler?.IsCaptureEnabled ?? false;
        }

        /// <summary>
        /// Check if the local user is currently speaking.
        /// </summary>
        /// <returns>True if speaking.</returns>
        public static bool IsSpeaking()
        {
            VoiceHandler handler = GetVoiceHandler();
            return handler?.IsLocalSpeaking ?? false;
        }

        /// <summary>
        /// Get the current connection state.
        /// </summary>
        /// <returns>Connection state string: disconnected, connecting, connected, reconnecting, error.</returns>
        public static string GetState()
        {
            VoiceHandler handler = GetVoiceHandler();
            if (handler == null)
            {
                return "disconnected";
            }

            switch (handler.State)
            {
                case VoiceConnectionState.Disconnected:
                    return "disconnected";
                case VoiceConnectionState.Connecting:
                    return "connecting";
                case VoiceConnectionState.Connected:
                    return "connected";
                case VoiceConnectionState.Reconnecting:
                    return "reconnecting";
                case VoiceConnectionState.Error:
                    return "error";
                default:
                    return "disconnected";
            }
        }

        /// <summary>
        /// Check if voice is enabled for this world.
        /// </summary>
        /// <returns>True if voice is configured.</returns>
        public static bool IsEnabled()
        {
            VoiceHandler handler = GetVoiceHandler();
            return handler?.Config != null;
        }

        /// <summary>
        /// Set the position of a remote user's speaker.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="position">World position.</param>
        public static void SetSpeakerPosition(string userId, Vector3 position)
        {
            VoiceHandler handler = GetVoiceHandler();
            handler?.SetSpeakerPosition(userId, new UnityEngine.Vector3(position.x, position.y, position.z));
        }

        /// <summary>
        /// Mute a specific remote user.
        /// </summary>
        /// <param name="userId">User ID to mute.</param>
        /// <param name="muted">True to mute.</param>
        public static void MuteUser(string userId, bool muted)
        {
            VoiceHandler handler = GetVoiceHandler();
            handler?.MuteUser(userId, muted);
        }

        /// <summary>
        /// Set volume for a specific remote user.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="volume">Volume level (0-1).</param>
        public static void SetUserVolume(string userId, float volume)
        {
            VoiceHandler handler = GetVoiceHandler();
            handler?.SetUserVolume(userId, volume);
        }

        /// <summary>
        /// Attach a user's voice speaker to an entity (e.g., a character).
        /// The speaker will follow the entity's position for spatial audio.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="entity">Entity to attach to.</param>
        /// <param name="offset">Local offset from the entity (e.g., head position).</param>
        /// <returns>True if attachment succeeded.</returns>
        public static bool AttachSpeakerToEntity(string userId, Entity.BaseEntity entity, Vector3 offset = null)
        {
            VoiceHandler handler = GetVoiceHandler();
            if (handler == null || entity == null)
            {
                return false;
            }

            StraightFour.Entity.BaseEntity internalEntity = Entity.EntityAPIHelper.GetPrivateEntity(entity);
            if (internalEntity == null)
            {
                return false;
            }

            UnityEngine.Vector3 unityOffset = offset != null
                ? new UnityEngine.Vector3(offset.x, offset.y, offset.z)
                : UnityEngine.Vector3.zero;

            return handler.AttachSpeakerToTransform(userId, internalEntity.transform, unityOffset);
        }

        /// <summary>
        /// Detach a user's voice speaker from its attached entity.
        /// </summary>
        /// <param name="userId">User ID.</param>
        public static void DetachSpeaker(string userId)
        {
            VoiceHandler handler = GetVoiceHandler();
            handler?.DetachSpeaker(userId);
        }

        #region Event Callbacks

        /// <summary>
        /// Register a callback for when voice connects.
        /// </summary>
        /// <param name="callback">JavaScript function name to call.</param>
        public static void OnConnected(string callback)
        {
            _onConnectedCallback = callback;
        }

        /// <summary>
        /// Register a callback for when voice disconnects.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives reason string.</param>
        public static void OnDisconnected(string callback)
        {
            _onDisconnectedCallback = callback;
        }

        /// <summary>
        /// Register a callback for when a user joins voice.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives userId.</param>
        public static void OnUserJoined(string callback)
        {
            _onUserJoinedCallback = callback;
        }

        /// <summary>
        /// Register a callback for when a user leaves voice.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives userId.</param>
        public static void OnUserLeft(string callback)
        {
            _onUserLeftCallback = callback;
        }

        /// <summary>
        /// Register a callback for when a user's speaking state changes.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives userId, isSpeaking.</param>
        public static void OnUserSpeaking(string callback)
        {
            _onUserSpeakingCallback = callback;
        }

        /// <summary>
        /// Register a callback for when local mute state changes.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives isMuted.</param>
        public static void OnMuteChanged(string callback)
        {
            _onMuteChangedCallback = callback;
        }

        /// <summary>
        /// Register a callback for voice errors.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives errorCode, message.</param>
        public static void OnError(string callback)
        {
            _onErrorCallback = callback;
        }

        /// <summary>
        /// Clear all registered callbacks.
        /// </summary>
        public static void ClearCallbacks()
        {
            _onConnectedCallback = null;
            _onDisconnectedCallback = null;
            _onUserJoinedCallback = null;
            _onUserLeftCallback = null;
            _onUserSpeakingCallback = null;
            _onMuteChangedCallback = null;
            _onErrorCallback = null;
        }

        #endregion

        #region Private Helpers

        private static VoiceHandler GetVoiceHandler()
        {
            if (WebVerseRuntime.Instance == null)
            {
                return null;
            }

            // Voice handler would be accessed through runtime
            // This depends on how VoiceHandler is registered with WebVerseRuntime
            return WebVerseRuntime.Instance.voiceHandler;
        }

        private static void WireEvents(VoiceHandler handler)
        {
            if (_eventsWired || handler == null)
            {
                return;
            }

            handler.OnStateChanged += (s, e) =>
            {
                if (e.NewState == VoiceConnectionState.Connected)
                {
                    InvokeCallback(_onConnectedCallback);
                }
                else if (e.NewState == VoiceConnectionState.Disconnected)
                {
                    InvokeCallback(_onDisconnectedCallback, "disconnected");
                }
                else if (e.NewState == VoiceConnectionState.Error)
                {
                    InvokeCallback(_onDisconnectedCallback, "error");
                    InvokeCallback(_onErrorCallback, "VOICE_CONNECTION_FAILED", "Connection failed");
                }
            };

            handler.OnUserJoined += (s, e) =>
            {
                InvokeCallback(_onUserJoinedCallback, e.UserId);
            };

            handler.OnUserLeft += (s, e) =>
            {
                InvokeCallback(_onUserLeftCallback, e.UserId);
            };

            handler.OnUserSpeaking += (s, e) =>
            {
                InvokeCallback(_onUserSpeakingCallback, e.UserId, e.IsSpeaking.ToString().ToLowerInvariant());
            };

            _eventsWired = true;
        }

        private static void InvokeCallback(string callback, params string[] args)
        {
            if (string.IsNullOrEmpty(callback))
            {
                return;
            }

            if (WebVerseRuntime.Instance?.javascriptHandler == null)
            {
                return;
            }

            string script;
            if (args == null || args.Length == 0)
            {
                script = $"{callback}()";
            }
            else
            {
                string argsStr = string.Join(", ", System.Array.ConvertAll(args, arg => $"'{EscapeString(arg)}'"));
                script = $"{callback}({argsStr})";
            }

            WebVerseRuntime.Instance.javascriptHandler.RunScript(script);
        }

        private static string EscapeString(string input)
        {
            if (input == null) return "";
            return input.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        #endregion
    }
}
