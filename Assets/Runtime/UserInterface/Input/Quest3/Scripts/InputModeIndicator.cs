// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Quest 3 implementation of VR input mode indicator.
    /// Extends BaseVRInputModeIndicator with Quest 3-specific integration.
    /// Visual indicator showing the current input mode (Hands or Controllers).
    /// </summary>
    public class InputModeIndicator : BaseVRInputModeIndicator
    {
        #region Configuration

        /// <summary>
        /// Reference to the Quest 3 input mode manager.
        /// </summary>
        [Tooltip("Reference to the input mode manager.")]
        public InputModeManager inputModeManager;

        #endregion

        #region BaseVRInputModeIndicator Implementation

        protected override BaseVRInputModeManager GetInputModeManager()
        {
            return inputModeManager;
        }

        #endregion
    }
}
