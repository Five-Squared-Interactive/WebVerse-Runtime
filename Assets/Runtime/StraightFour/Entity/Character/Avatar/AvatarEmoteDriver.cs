// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Drives avatar emote animations via Animator triggers.
    /// Tracks current emote state and fires start/end events through AvatarAnimationManager.
    /// Auto-detects emote completion by monitoring Animator state tags.
    /// </summary>
    public class AvatarEmoteDriver : MonoBehaviour
    {
        private AvatarAnimationManager _animationManager;
        private Animator _animator;
        private string _currentEmote;
        private bool _isPlayingEmote;
        private bool _emoteStartedThisFrame;

        /// <summary>
        /// The name of the currently playing emote, or null/empty if none.
        /// </summary>
        public string CurrentEmote => _currentEmote;

        /// <summary>
        /// Whether an emote is currently playing.
        /// </summary>
        public bool IsPlayingEmote => _isPlayingEmote;

        /// <summary>
        /// Initializes the emote driver with a reference to the animation manager.
        /// </summary>
        /// <param name="animationManager">The avatar animation manager that owns the Animator.</param>
        public void Initialize(AvatarAnimationManager animationManager)
        {
            _animationManager = animationManager;
            _animator = animationManager.Animator;
        }

        /// <summary>
        /// Updates the cached Animator reference. Called when AvatarLoader
        /// switches to a custom avatar's Animator.
        /// </summary>
        /// <param name="animator">The new Animator reference.</param>
        public void UpdateAnimator(Animator animator)
        {
            _animator = animator;
        }

        /// <summary>
        /// Plays an emote animation by setting an Animator trigger.
        /// If an emote is already playing, it is stopped first.
        /// </summary>
        /// <param name="emoteName">The name of the emote trigger in the Animator Controller.</param>
        public void PlayEmote(string emoteName)
        {
            if (string.IsNullOrEmpty(emoteName))
            {
                return;
            }

            // Stop current emote if one is playing
            if (_isPlayingEmote)
            {
                StopEmote();
            }

            _currentEmote = emoteName;
            _isPlayingEmote = true;
            _emoteStartedThisFrame = true;

            if (_animator != null)
            {
                _animator.SetTrigger(emoteName);
            }

            if (_animationManager != null)
            {
                _animationManager.FireEmoteStarted(emoteName);
            }

        }

        /// <summary>
        /// Stops the currently playing emote. Fires OnEmoteEnded event.
        /// </summary>
        public void StopEmote()
        {
            if (string.IsNullOrEmpty(_currentEmote))
            {
                return;
            }

            string previousEmote = _currentEmote;
            _currentEmote = null;
            _isPlayingEmote = false;

            if (_animator != null)
            {
                _animator.ResetTrigger(previousEmote);
            }

            if (_animationManager != null)
            {
                _animationManager.FireEmoteEnded(previousEmote);
            }
        }

        /// <summary>
        /// Populates the ActiveEmote field of an AvatarState struct.
        /// </summary>
        /// <param name="state">The AvatarState to populate (passed by reference).</param>
        public void PopulateState(ref AvatarState state)
        {
            state.ActiveEmote = _currentEmote ?? "";
        }

        /// <summary>
        /// Auto-detects emote completion by checking if the Animator has
        /// transitioned out of an "Emote"-tagged state.
        /// </summary>
        private void Update()
        {
            if (!_isPlayingEmote || _animator == null)
            {
                return;
            }

            // Skip the frame the emote was triggered — the Animator hasn't consumed
            // the trigger yet, so GetCurrentAnimatorStateInfo still returns the
            // previous (non-Emote) state and would falsely end the emote immediately.
            if (_emoteStartedThisFrame)
            {
                _emoteStartedThisFrame = false;
                return;
            }

            // Check if the Animator has exited the emote state
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsTag("Emote"))
            {
                StopEmote();
            }
        }
    }
}
