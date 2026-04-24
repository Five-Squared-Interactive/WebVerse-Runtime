// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Defines how avatar animation is driven.
    /// </summary>
    public enum AvatarTrackingMode
    {
        /// <summary>
        /// Avatar is driven by Mecanim animation (desktop mode).
        /// </summary>
        Animation,

        /// <summary>
        /// Avatar is driven by IK tracking (VR mode).
        /// </summary>
        IK
    }
}
