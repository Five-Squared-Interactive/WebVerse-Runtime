// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Legacy input mode enum for Quest 3 backwards compatibility.
    /// Use VRInputMode from the base namespace for new code.
    /// </summary>
    public enum InputMode
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
    /// Quest 3 implementation of VR input mode management.
    /// Extends BaseVRInputModeManager with Quest 3-specific hand tracking and controller integration.
    /// </summary>
    public class InputModeManager : BaseVRInputModeManager
    {
        #region Configuration

        /// <summary>
        /// Reference to the Quest 3 hand tracking component.
        /// </summary>
        [Tooltip("Reference to the hand tracking component.")]
        public Quest3HandTracking handTracking;

        /// <summary>
        /// Reference to the controller input component.
        /// </summary>
        [Tooltip("Reference to the controller input component.")]
        public Quest3Input controllerInput;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current input mode using the legacy enum for backwards compatibility.
        /// </summary>
        public new InputMode CurrentInputMode
        {
            get => (InputMode)base.CurrentInputMode;
        }

        #endregion

        #region Events

        /// <summary>
        /// Legacy event for backwards compatibility.
        /// Fired when the input mode changes.
        /// </summary>
        public new event System.Action<InputMode, InputMode> OnInputModeChanged;

        #endregion

        #region Unity Lifecycle

        protected override void Start()
        {
            base.Start();

            // Check for hand input if hands are tracked on start
            CheckHandInput();
        }

        protected override void Update()
        {
            base.Update();

            // Additional hand tracking check
            CheckHandInput();
        }

        #endregion

        #region BaseVRInputModeManager Implementation

        protected override IHandTrackingProvider GetHandTrackingProvider()
        {
            return handTracking;
        }

        protected override bool AreControllersAvailable()
        {
            return controllerInput != null;
        }

        protected override void OnModeChanged(VRInputMode previousMode, VRInputMode newMode)
        {
            base.OnModeChanged(previousMode, newMode);

            // Fire legacy event for backwards compatibility
            OnInputModeChanged?.Invoke((InputMode)previousMode, (InputMode)newMode);

            // Notify input manager
            NotifyInputManager();
        }

        #endregion

        #region Private Methods

        private void CheckHandInput()
        {
            if (handTracking == null)
            {
                return;
            }

            // Check if hands are tracked and actively being used
            bool handsTracked = handTracking.IsLeftHandTracked || handTracking.IsRightHandTracked;

            if (handsTracked && base.CurrentInputMode == VRInputMode.None)
            {
                // Switch to hands if no input was active
                ForceInputMode(VRInputMode.Hands);
            }
        }

        private void NotifyInputManager()
        {
            if (WebVerseRuntime.Instance?.inputManager == null)
            {
                return;
            }

            // Update InputManager's input mode tracking
            var inputManager = WebVerseRuntime.Instance.inputManager;

            // Set the current input mode value via reflection if field exists
            SetInputManagerMode(inputManager, base.CurrentInputMode);
        }

        private void SetInputManagerMode(InputManager inputManager, VRInputMode mode)
        {
            // Try to set via reflection if the field exists
            var field = typeof(InputManager).GetField("currentInputMode",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(inputManager, (int)mode);
            }

            // Also try quest3InputMode field
            var quest3Field = typeof(InputManager).GetField("quest3InputMode",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (quest3Field != null)
            {
                quest3Field.SetValue(inputManager, (int)mode);
            }
        }

        #endregion
    }
}
