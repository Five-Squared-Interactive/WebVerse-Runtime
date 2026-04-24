// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Avatar;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.StraightFour.Entity;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Avatar
{
    /// <summary>
    /// JavaScript API for avatar emote and tracking mode control.
    /// Follows the Voice.cs static handler pattern.
    /// </summary>
    public class Avatar
    {
        private static string _onEmoteStartedCallback;
        private static string _onEmoteEndedCallback;
        private static string _onAvatarLoadedCallback;
        private static string _onAvatarLoadFailedCallback;
        private static string _onTrackingModeChangedCallback;
        private static bool _eventsWired;
        private static CharacterEntity _cachedCharacterEntity;
        private static AvatarAnimationManager _wiredManager;
        private static System.Action<string> _emoteStartedHandler;
        private static System.Action<string> _emoteEndedHandler;
        private static System.Action<string> _avatarLoadedHandler;
        private static System.Action<string> _avatarLoadFailedHandler;
        private static System.Action<AvatarTrackingMode> _trackingModeChangedHandler;

        #region Emote Methods

        /// <summary>
        /// Play an emote animation on the local user's avatar.
        /// </summary>
        /// <param name="emoteName">Name of the emote to play.</param>
        public static void PlayEmote(string emoteName)
        {
            if (string.IsNullOrEmpty(emoteName)) return;

            AvatarAnimationManager manager = GetAnimationManager();
            if (manager == null || manager.EmoteDriver == null) return;

            WireEvents(manager);
            manager.EmoteDriver.PlayEmote(emoteName);
        }

        /// <summary>
        /// Stop the currently playing emote animation.
        /// </summary>
        public static void StopEmote()
        {
            AvatarAnimationManager manager = GetAnimationManager();
            if (manager == null || manager.EmoteDriver == null) return;

            manager.EmoteDriver.StopEmote();
        }

        /// <summary>
        /// Get the name of the currently playing emote.
        /// </summary>
        /// <returns>Emote name, or null if no emote is playing.</returns>
        public static string GetCurrentEmote()
        {
            AvatarAnimationManager manager = GetAnimationManager();
            if (manager == null || manager.EmoteDriver == null) return null;

            return manager.EmoteDriver.CurrentEmote;
        }

        #endregion

        #region Tracking Mode Methods

        /// <summary>
        /// Set the avatar tracking mode.
        /// </summary>
        /// <param name="mode">"animation" for desktop mode, "ik" for VR IK mode.</param>
        public static void SetTrackingMode(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return;

            CharacterEntity character = GetCharacterEntity();
            if (character == null) return;

            string normalized = mode.ToLowerInvariant().Trim();

            if (normalized == "animation")
            {
                if (character.IsVRMode)
                {
                    character.SetVRMode(false);
                }
            }
            else if (normalized == "ik")
            {
                if (!character.IsVRMode)
                {
                    character.SetVRMode(true);
                }
            }
            else
            {
                Logging.LogWarning($"[Avatar API] Unknown tracking mode: '{mode}'. Use 'animation' or 'ik'.");
            }
        }

        /// <summary>
        /// Get the current avatar tracking mode.
        /// </summary>
        /// <returns>"animation" or "ik".</returns>
        public static string GetTrackingMode()
        {
            CharacterEntity character = GetCharacterEntity();
            if (character == null) return "animation";

            return character.IsVRMode ? "ik" : "animation";
        }

        #endregion

        #region Event Callbacks

        /// <summary>
        /// Register a callback for when an emote starts playing.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives emote name.</param>
        public static void OnEmoteStarted(string callback)
        {
            _onEmoteStartedCallback = callback;
            WireEvents(GetAnimationManager());
        }

        /// <summary>
        /// Register a callback for when an emote finishes playing.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives emote name.</param>
        public static void OnEmoteEnded(string callback)
        {
            _onEmoteEndedCallback = callback;
            WireEvents(GetAnimationManager());
        }

        /// <summary>
        /// Register a callback for when an avatar is successfully loaded.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives avatar URI.</param>
        public static void OnAvatarLoaded(string callback)
        {
            _onAvatarLoadedCallback = callback;
            WireEvents(GetAnimationManager());
        }

        /// <summary>
        /// Register a callback for when an avatar load fails.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives error message.</param>
        public static void OnAvatarLoadFailed(string callback)
        {
            _onAvatarLoadFailedCallback = callback;
            WireEvents(GetAnimationManager());
        }

        /// <summary>
        /// Register a callback for when the tracking mode changes.
        /// </summary>
        /// <param name="callback">JavaScript function name to call. Receives mode string ("animation" or "ik").</param>
        public static void OnTrackingModeChanged(string callback)
        {
            _onTrackingModeChangedCallback = callback;
            WireEvents(GetAnimationManager());
        }

        /// <summary>
        /// Clear all registered callbacks and reset event wiring state.
        /// Resetting _eventsWired allows re-wiring to a new AvatarAnimationManager
        /// after world transitions.
        /// </summary>
        public static void ClearCallbacks()
        {
            UnwireEvents();
            _onEmoteStartedCallback = null;
            _onEmoteEndedCallback = null;
            _onAvatarLoadedCallback = null;
            _onAvatarLoadFailedCallback = null;
            _onTrackingModeChangedCallback = null;
            _eventsWired = false;
            _cachedCharacterEntity = null;
        }

        #endregion

        #region State Queries

        /// <summary>
        /// Get a JSON string of current avatar state.
        /// </summary>
        /// <returns>JSON string with emote, tracking, and locomotion state, or null if no runtime.</returns>
        public static string GetState()
        {
            AvatarAnimationManager manager = GetAnimationManager();
            if (manager == null) return null;

            string currentEmote = manager.EmoteDriver != null ? manager.EmoteDriver.CurrentEmote : null;
            bool isPlayingEmote = manager.EmoteDriver != null && manager.EmoteDriver.IsPlayingEmote;
            string trackingMode = GetTrackingMode();
            float speed = manager.LocomotionDriver != null ? manager.LocomotionDriver.CurrentSpeed : 0f;
            float direction = manager.LocomotionDriver != null ? manager.LocomotionDriver.CurrentDirection : 0f;

            string emoteJson = currentEmote != null
                ? $"\"{EscapeJsonString(currentEmote)}\""
                : "null";

            return $"{{\"currentEmote\":{emoteJson},\"isPlayingEmote\":{(isPlayingEmote ? "true" : "false")},\"trackingMode\":\"{trackingMode}\",\"locomotionSpeed\":{speed.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"locomotionDirection\":{direction.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}";
        }

        /// <summary>
        /// Get the current locomotion speed.
        /// </summary>
        /// <returns>Smoothed speed value 0-1, or 0 if no runtime.</returns>
        public static float GetLocomotionSpeed()
        {
            AvatarAnimationManager manager = GetAnimationManager();
            if (manager == null || manager.LocomotionDriver == null) return 0f;

            return manager.LocomotionDriver.CurrentSpeed;
        }

        /// <summary>
        /// Get the current locomotion direction.
        /// </summary>
        /// <returns>Direction in degrees -180 to 180, or 0 if no runtime.</returns>
        public static float GetLocomotionDirection()
        {
            AvatarAnimationManager manager = GetAnimationManager();
            if (manager == null || manager.LocomotionDriver == null) return 0f;

            return manager.LocomotionDriver.CurrentDirection;
        }

        /// <summary>
        /// Check if an emote is currently playing.
        /// </summary>
        /// <returns>True if an emote is active, false otherwise.</returns>
        public static bool IsPlayingEmote()
        {
            AvatarAnimationManager manager = GetAnimationManager();
            if (manager == null || manager.EmoteDriver == null) return false;

            return manager.EmoteDriver.IsPlayingEmote;
        }

        #endregion

        #region Avatar Loading

        /// <summary>
        /// Load an avatar from a URI (glTF/VRM).
        /// </summary>
        /// <param name="uri">URI of the avatar model to load.</param>
        public static void LoadAvatar(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return;

            AvatarAnimationManager manager = GetAnimationManager();
            if (manager == null || manager.AvatarLoader == null) return;

            WireEvents(manager);
            manager.AvatarLoader.LoadAvatarAsync(uri);
        }

        #endregion

        #region Private Helpers

        private static CharacterEntity GetCharacterEntity()
        {
            if (WebVerseRuntime.Instance == null) return null;

            // Return cached reference if still valid (not destroyed)
            if (_cachedCharacterEntity != null) return _cachedCharacterEntity;

            if (StraightFour.StraightFour.ActiveWorld == null
                || StraightFour.StraightFour.ActiveWorld.entityManager == null)
            {
                return null;
            }

            foreach (BaseEntity entity in StraightFour.StraightFour.ActiveWorld.entityManager.GetAllEntities())
            {
                if (entity is CharacterEntity ce)
                {
                    _cachedCharacterEntity = ce;
                    return ce;
                }
            }

            return null;
        }

        private static AvatarAnimationManager GetAnimationManager()
        {
            CharacterEntity character = GetCharacterEntity();
            if (character == null) return null;

            AvatarAnimationManager manager = character.AvatarAnimationManager;
            if (manager == null || !manager.IsInitialized) return null;

            return manager;
        }

        private static void WireEvents(AvatarAnimationManager manager)
        {
            if (_eventsWired || manager == null) return;

            _emoteStartedHandler = (emoteName) => InvokeCallback(_onEmoteStartedCallback, emoteName);
            _emoteEndedHandler = (emoteName) => InvokeCallback(_onEmoteEndedCallback, emoteName);
            _avatarLoadedHandler = (uri) => InvokeCallback(_onAvatarLoadedCallback, uri);
            _avatarLoadFailedHandler = (errorMsg) => InvokeCallback(_onAvatarLoadFailedCallback, errorMsg);
            _trackingModeChangedHandler = (mode) =>
            {
                string modeStr = mode == AvatarTrackingMode.IK ? "ik" : "animation";
                InvokeCallback(_onTrackingModeChangedCallback, modeStr);
            };

            manager.OnEmoteStarted += _emoteStartedHandler;
            manager.OnEmoteEnded += _emoteEndedHandler;
            manager.OnAvatarLoaded += _avatarLoadedHandler;
            manager.OnAvatarLoadFailed += _avatarLoadFailedHandler;
            manager.OnTrackingModeChanged += _trackingModeChangedHandler;

            _wiredManager = manager;
            _eventsWired = true;
        }

        private static void UnwireEvents()
        {
            if (!_eventsWired || _wiredManager == null) return;

            _wiredManager.OnEmoteStarted -= _emoteStartedHandler;
            _wiredManager.OnEmoteEnded -= _emoteEndedHandler;
            _wiredManager.OnAvatarLoaded -= _avatarLoadedHandler;
            _wiredManager.OnAvatarLoadFailed -= _avatarLoadFailedHandler;
            _wiredManager.OnTrackingModeChanged -= _trackingModeChangedHandler;

            _wiredManager = null;
            _emoteStartedHandler = null;
            _emoteEndedHandler = null;
            _avatarLoadedHandler = null;
            _avatarLoadFailedHandler = null;
            _trackingModeChangedHandler = null;
        }

        private static void InvokeCallback(string callback, params string[] args)
        {
            if (string.IsNullOrEmpty(callback)) return;

            if (WebVerseRuntime.Instance?.javascriptHandler == null) return;

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

        private static string EscapeJsonString(string input)
        {
            if (input == null) return "";
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        #endregion
    }
}
