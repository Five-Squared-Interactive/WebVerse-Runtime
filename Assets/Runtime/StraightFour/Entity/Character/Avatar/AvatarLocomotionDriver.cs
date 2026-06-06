// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Drives Animator locomotion blend tree parameters from movement input.
    /// Reads raw input magnitude (0-1) and smoothly updates LocomotionSpeed
    /// and LocomotionDirection on the Animator.
    /// </summary>
    public class AvatarLocomotionDriver : MonoBehaviour
    {
        [SerializeField] private float acceleration = 6f;
        [SerializeField] private float deceleration = 4f;
        [SerializeField] private float directionSpeed = 360f;

        private AvatarAnimationManager _animationManager;
        private Animator _animator;
        private float _currentSpeed;
        private float _currentDirection;
        private Vector2 _movementInput;

        private static readonly int SpeedParam = Animator.StringToHash("LocomotionSpeed");
        private static readonly int DirectionParam = Animator.StringToHash("LocomotionDirection");

        /// <summary>
        /// Current smoothed locomotion speed (0-1 range).
        /// </summary>
        public float CurrentSpeed => _currentSpeed;

        /// <summary>
        /// Current smoothed locomotion direction in degrees (-180 to 180).
        /// </summary>
        public float CurrentDirection => _currentDirection;

        /// <summary>
        /// Initializes the locomotion driver with a reference to the animation manager.
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
        /// Sets the raw movement input vector. Magnitude is clamped to [0, 1].
        /// </summary>
        /// <param name="input">Movement input (x = strafe, y = forward/back).</param>
        public void SetMovementInput(Vector2 input)
        {
            _movementInput = Vector2.ClampMagnitude(input, 1f);
        }

        /// <summary>
        /// Called by Unity each frame. Smoothly updates Animator parameters.
        /// </summary>
        private void Update()
        {
            ManualUpdate(Time.deltaTime);
        }

        /// <summary>
        /// Updates locomotion parameters with the given delta time.
        /// Public for testability — tests call this directly instead of relying on Unity Update.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update.</param>
        public void ManualUpdate(float deltaTime)
        {
            float targetSpeed = _movementInput.magnitude;

            // Smooth speed using acceleration/deceleration rates
            float rate = targetSpeed > _currentSpeed ? acceleration : deceleration;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, rate * deltaTime);

            // Calculate direction only when there is movement input
            if (_movementInput.sqrMagnitude > 0.001f)
            {
                float targetDirection = Mathf.Atan2(_movementInput.x, _movementInput.y) * Mathf.Rad2Deg;
                _currentDirection = Mathf.MoveTowardsAngle(_currentDirection, targetDirection, directionSpeed * deltaTime);
            }

            // Update Animator parameters if available
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.SetFloat(SpeedParam, _currentSpeed);
                _animator.SetFloat(DirectionParam, _currentDirection);
            }
        }

        /// <summary>
        /// Populates the LocomotionSpeed and LocomotionDirection fields of an AvatarState struct
        /// with current smoothed values.
        /// </summary>
        /// <param name="state">The AvatarState to populate (passed by reference).</param>
        public void PopulateState(ref AvatarState state)
        {
            state.LocomotionSpeed = _currentSpeed;
            state.LocomotionDirection = _currentDirection;
        }
    }
}
