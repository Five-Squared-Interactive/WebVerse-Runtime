// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Input mode enum for VR platforms.
    /// Defines the possible input sources available across VR headsets.
    /// </summary>
    public enum VRInputMode
    {
        /// <summary>
        /// No input source detected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Hand tracking is the active input.
        /// </summary>
        Hands = 1,

        /// <summary>
        /// Controllers are the active input.
        /// </summary>
        Controllers = 2
    }

    /// <summary>
    /// Interface for hand tracking providers.
    /// Implement this interface for platform-specific hand tracking (Quest, Pico, etc.).
    /// </summary>
    public interface IHandTrackingProvider
    {
        /// <summary>
        /// Whether left hand is currently tracked.
        /// </summary>
        bool IsLeftHandTracked { get; }

        /// <summary>
        /// Whether right hand is currently tracked.
        /// </summary>
        bool IsRightHandTracked { get; }

        /// <summary>
        /// Fired when hand tracking state changes.
        /// Parameters: handIndex (0=left, 1=right), isTracked
        /// </summary>
        event Action<int, bool> OnHandTrackingStateChanged;

        /// <summary>
        /// Fired when hand tracking is lost for a hand.
        /// Parameters: handIndex (0=left, 1=right), duration of loss
        /// </summary>
        event Action<int, float> OnTrackingLost;

        /// <summary>
        /// Fired when hand tracking recovers for a hand.
        /// Parameters: handIndex (0=left, 1=right), time taken to recover
        /// </summary>
        event Action<int, float> OnTrackingRecovered;

        /// <summary>
        /// Fired when pinch state changes.
        /// Parameters: handIndex (0=left, 1=right), isPinching
        /// </summary>
        event Action<int, bool> OnPinchStateChanged;
    }

    /// <summary>
    /// Abstract base class for managing VR input modes.
    /// Handles switching between hand tracking and controller input.
    /// Extend this class for platform-specific implementations.
    /// </summary>
    public abstract class BaseVRInputModeManager : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Fired when the input mode changes.
        /// </summary>
        public event Action<VRInputMode, VRInputMode> OnInputModeChanged;

        #endregion

        #region Properties

        /// <summary>
        /// The current active input mode.
        /// </summary>
        public VRInputMode CurrentInputMode
        {
            get => currentInputMode;
            protected set
            {
                if (currentInputMode != value)
                {
                    VRInputMode previousMode = currentInputMode;
                    currentInputMode = value;
                    OnInputModeChanged?.Invoke(previousMode, currentInputMode);
                    OnModeChanged(previousMode, currentInputMode);
                    Logging.Log($"[{GetType().Name}] Input mode changed: {previousMode} -> {currentInputMode}");
                }
            }
        }

        /// <summary>
        /// Whether hand tracking is currently active.
        /// </summary>
        public bool IsHandTrackingActive => currentInputMode == VRInputMode.Hands;

        /// <summary>
        /// Whether controllers are currently active.
        /// </summary>
        public bool IsControllersActive => currentInputMode == VRInputMode.Controllers;

        /// <summary>
        /// Time since last input was detected from any source.
        /// </summary>
        public float TimeSinceLastInput => Time.time - lastInputTime;

        #endregion

        #region Configuration

        /// <summary>
        /// Time in seconds before switching from controllers to hands when both are available.
        /// Prevents rapid switching when user briefly moves hands while holding controllers.
        /// </summary>
        [Tooltip("Delay before switching from controllers to hands (seconds).")]
        public float controllerToHandDelay = 0.5f;

        /// <summary>
        /// Time in seconds of no input before triggering no-input state.
        /// </summary>
        [Tooltip("Time before no-input state is triggered (seconds).")]
        public float noInputTimeout = 2.0f;

        #endregion

        #region Protected Fields

        protected VRInputMode currentInputMode = VRInputMode.None;
        protected float lastInputTime;
        protected float lastControllerInputTime;
        protected float lastHandInputTime;
        protected bool pendingHandSwitch;
        protected float handSwitchRequestTime;

        #endregion

        #region Unity Lifecycle

        protected virtual void Start()
        {
            lastInputTime = Time.time;
            lastControllerInputTime = Time.time;
            lastHandInputTime = Time.time;

            // Subscribe to hand tracking events if available
            var handTracking = GetHandTrackingProvider();
            if (handTracking != null)
            {
                handTracking.OnHandTrackingStateChanged += OnHandTrackingStateChanged;
                handTracking.OnPinchStateChanged += OnHandPinchStateChanged;
            }
        }

        protected virtual void OnDestroy()
        {
            var handTracking = GetHandTrackingProvider();
            if (handTracking != null)
            {
                handTracking.OnHandTrackingStateChanged -= OnHandTrackingStateChanged;
                handTracking.OnPinchStateChanged -= OnHandPinchStateChanged;
            }
        }

        protected virtual void Update()
        {
            ProcessPendingSwitch();
            CheckNoInputTimeout();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Forces a switch to the specified input mode.
        /// Use sparingly - automatic detection is preferred.
        /// </summary>
        /// <param name="mode">The mode to switch to.</param>
        public void ForceInputMode(VRInputMode mode)
        {
            CurrentInputMode = mode;
            pendingHandSwitch = false;
        }

        /// <summary>
        /// Registers that controller input was received.
        /// Called by platform input when any controller button/stick is used.
        /// </summary>
        public void RegisterControllerInput()
        {
            lastControllerInputTime = Time.time;
            lastInputTime = Time.time;
            pendingHandSwitch = false;

            if (CurrentInputMode != VRInputMode.Controllers)
            {
                CurrentInputMode = VRInputMode.Controllers;
            }
        }

        /// <summary>
        /// Registers that hand input was received.
        /// Called by platform hand input when hand gestures are detected.
        /// </summary>
        public void RegisterHandInput()
        {
            lastHandInputTime = Time.time;
            lastInputTime = Time.time;

            if (CurrentInputMode == VRInputMode.Controllers)
            {
                if (!pendingHandSwitch)
                {
                    pendingHandSwitch = true;
                    handSwitchRequestTime = Time.time;
                }
            }
            else if (CurrentInputMode != VRInputMode.Hands)
            {
                CurrentInputMode = VRInputMode.Hands;
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Gets the hand tracking provider for this platform.
        /// </summary>
        /// <returns>The hand tracking provider, or null if not available.</returns>
        protected abstract IHandTrackingProvider GetHandTrackingProvider();

        /// <summary>
        /// Checks if controllers are available on this platform.
        /// </summary>
        /// <returns>True if controllers are available.</returns>
        protected abstract bool AreControllersAvailable();

        #endregion

        #region Protected Methods

        /// <summary>
        /// Called when mode changes. Override to perform platform-specific actions.
        /// </summary>
        protected virtual void OnModeChanged(VRInputMode previousMode, VRInputMode newMode)
        {
            // Override in derived classes for platform-specific behavior
        }

        protected virtual void ProcessPendingSwitch()
        {
            if (pendingHandSwitch)
            {
                float timeSinceRequest = Time.time - handSwitchRequestTime;
                float timeSinceControllerInput = Time.time - lastControllerInputTime;

                if (timeSinceRequest >= controllerToHandDelay && timeSinceControllerInput >= controllerToHandDelay)
                {
                    CurrentInputMode = VRInputMode.Hands;
                    pendingHandSwitch = false;
                }
            }
        }

        protected virtual void CheckNoInputTimeout()
        {
            if (CurrentInputMode == VRInputMode.None)
            {
                return;
            }

            float timeSinceInput = Time.time - lastInputTime;

            if (timeSinceInput >= noInputTimeout)
            {
                var handTracking = GetHandTrackingProvider();
                bool handsTracked = handTracking != null &&
                    (handTracking.IsLeftHandTracked || handTracking.IsRightHandTracked);

                if (!handsTracked && CurrentInputMode == VRInputMode.Hands)
                {
                    CurrentInputMode = VRInputMode.None;
                }
            }
        }

        protected virtual void OnHandTrackingStateChanged(int handIndex, bool isTracked)
        {
            if (isTracked)
            {
                RegisterHandInput();
            }
            else
            {
                var handTracking = GetHandTrackingProvider();
                if (handTracking != null &&
                    !handTracking.IsLeftHandTracked &&
                    !handTracking.IsRightHandTracked &&
                    CurrentInputMode == VRInputMode.Hands)
                {
                    if (AreControllersAvailable())
                    {
                        CurrentInputMode = VRInputMode.Controllers;
                    }
                    else
                    {
                        CurrentInputMode = VRInputMode.None;
                    }
                }
            }
        }

        protected virtual void OnHandPinchStateChanged(int handIndex, bool isPinching)
        {
            if (isPinching)
            {
                RegisterHandInput();
            }
        }

        #endregion
    }
}
