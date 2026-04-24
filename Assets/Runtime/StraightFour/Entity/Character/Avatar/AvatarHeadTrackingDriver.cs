// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Drives avatar head bone rotation from mouse-look input.
    /// Applies procedural rotation in LateUpdate after Mecanim processes
    /// locomotion animation, creating a smooth overlay.
    /// </summary>
    public class AvatarHeadTrackingDriver : MonoBehaviour
    {
        [SerializeField] private float smoothSpeed = 120f;
        [SerializeField] private float maxYaw = 70f;
        [SerializeField] private float maxPitch = 40f;

        private AvatarAnimationManager _animationManager;
        private Animator _animator;
        private Transform _headBone;
        private float _currentHeadYaw;
        private float _currentHeadPitch;
        private float _targetHeadYaw;
        private float _targetHeadPitch;
        private bool _isEnabled = true;

        /// <summary>
        /// Current smoothed head yaw in degrees (-maxYaw to maxYaw).
        /// </summary>
        public float CurrentHeadYaw => _currentHeadYaw;

        /// <summary>
        /// Current smoothed head pitch in degrees (-maxPitch to maxPitch).
        /// </summary>
        public float CurrentHeadPitch => _currentHeadPitch;

        /// <summary>
        /// Initializes the head tracking driver with a reference to the animation manager.
        /// </summary>
        /// <param name="animationManager">The avatar animation manager that owns the Animator.</param>
        public void Initialize(AvatarAnimationManager animationManager)
        {
            _animationManager = animationManager;
            _animator = animationManager.Animator;
            ResolveHeadBone();
        }

        /// <summary>
        /// Updates the cached Animator reference and re-resolves the head bone.
        /// Called when AvatarLoader switches to a custom avatar's Animator.
        /// </summary>
        /// <param name="animator">The new Animator reference.</param>
        public void UpdateAnimator(Animator animator)
        {
            _animator = animator;
            ResolveHeadBone();
        }

        /// <summary>
        /// Sets the target head look rotation. Values are clamped to natural human range.
        /// </summary>
        /// <param name="yaw">Target yaw in degrees (positive = right).</param>
        /// <param name="pitch">Target pitch in degrees (positive = up).</param>
        public void SetHeadLookInput(float yaw, float pitch)
        {
            _targetHeadYaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
            _targetHeadPitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        }

        /// <summary>
        /// Enables or disables head tracking updates. When disabled, LateUpdate/ManualUpdate
        /// skip processing. Used to prevent conflict with IK head tracking in VR mode.
        /// </summary>
        /// <param name="enabled">True to enable, false to disable.</param>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        /// <summary>
        /// Updates head tracking values with the given delta time.
        /// Public for testability — tests call this directly instead of relying on Unity LateUpdate.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update.</param>
        public void ManualUpdate(float deltaTime)
        {
            if (!_isEnabled) return;
            _currentHeadYaw = Mathf.MoveTowards(_currentHeadYaw, _targetHeadYaw, smoothSpeed * deltaTime);
            _currentHeadPitch = Mathf.MoveTowards(_currentHeadPitch, _targetHeadPitch, smoothSpeed * deltaTime);
        }

        /// <summary>
        /// Called by Unity after all animation processing. Applies procedural
        /// head rotation on top of Mecanim locomotion animation.
        /// </summary>
        private void LateUpdate()
        {
            if (!_isEnabled) return;
            ManualUpdate(Time.deltaTime);
            ApplyHeadRotation();
        }

        /// <summary>
        /// Applies the current yaw/pitch rotation to the head bone transform.
        /// </summary>
        private void ApplyHeadRotation()
        {
            if (_headBone == null)
            {
                return;
            }

            // Overlay procedural rotation on top of animation-driven rotation
            _headBone.localRotation *= Quaternion.Euler(_currentHeadPitch, _currentHeadYaw, 0f);
        }

        /// <summary>
        /// Populates the HeadYaw and HeadPitch fields of an AvatarState struct
        /// with current smoothed rotation values.
        /// </summary>
        /// <param name="state">The AvatarState to populate (passed by reference).</param>
        public void PopulateState(ref AvatarState state)
        {
            state.HeadYaw = _currentHeadYaw;
            state.HeadPitch = _currentHeadPitch;
        }

        /// <summary>
        /// Resolves the head bone Transform from the current Animator's humanoid avatar.
        /// </summary>
        private void ResolveHeadBone()
        {
            _headBone = null;
            if (_animator != null && _animator.isHuman)
            {
                _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
            }
        }
    }
}
