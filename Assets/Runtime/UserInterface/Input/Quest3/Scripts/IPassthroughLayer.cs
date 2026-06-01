// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Thin wrapper interface for passthrough layer control.
    /// Abstracts OVRPassthroughLayer so Quest3ARProvider is testable without the Meta SDK.
    /// </summary>
    public interface IPassthroughLayer
    {
        /// <summary>
        /// Whether the passthrough layer is currently enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Enable the passthrough layer.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable the passthrough layer.
        /// </summary>
        void Disable();
    }
}