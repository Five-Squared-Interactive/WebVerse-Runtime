// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Quest 3 implementation of no-input guidance display.
    /// Extends BaseNoInputGuidance with Quest 3-specific integration.
    /// Displays guidance messages when no input is detected.
    /// </summary>
    public class NoInputGuidance : BaseNoInputGuidance
    {
        #region Configuration

        /// <summary>
        /// Reference to the Quest 3 input mode manager.
        /// </summary>
        [Tooltip("Reference to the input mode manager.")]
        public InputModeManager inputModeManager;

        /// <summary>
        /// Reference to Quest 3 hand tracking for tracking loss detection.
        /// </summary>
        [Tooltip("Reference to hand tracking.")]
        public Quest3HandTracking handTracking;

        #endregion

        #region BaseNoInputGuidance Implementation

        protected override BaseVRInputModeManager GetInputModeManager()
        {
            return inputModeManager;
        }

        protected override IHandTrackingProvider GetHandTrackingProvider()
        {
            return handTracking;
        }

        #endregion
    }
}
