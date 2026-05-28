// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Interface for AR passthrough providers.
    /// Platform implementations (Quest 3, ARKit, ARCore) implement this interface.
    /// InputManager holds a nullable reference; null on non-AR platforms.
    /// </summary>
    public interface IARProvider
    {
        /// <summary>
        /// Current XR display mode (VR or AR).
        /// </summary>
        XRDisplayMode CurrentDisplayMode { get; }

        /// <summary>
        /// Whether passthrough rendering is supported on this device.
        /// </summary>
        bool IsPassthroughSupported { get; }

        /// <summary>
        /// Enable passthrough rendering, switching to AR mode.
        /// </summary>
        void EnablePassthrough();

        /// <summary>
        /// Disable passthrough rendering, switching to VR mode.
        /// </summary>
        void DisablePassthrough();
    }
}