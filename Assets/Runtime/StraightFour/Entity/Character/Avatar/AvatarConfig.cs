// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Configuration for avatar loading and behavior.
    /// </summary>
    public class AvatarConfig
    {
        /// <summary>
        /// URI of the avatar model to load (glTF or VRM).
        /// Null when using the default avatar.
        /// </summary>
        public string AvatarUri;

        /// <summary>
        /// Whether to fall back to the default avatar on load failure.
        /// </summary>
        public bool FallbackEnabled = true;

        /// <summary>
        /// URI of the fallback avatar model. Null uses the built-in default.
        /// </summary>
        public string FallbackAvatarUri;
    }
}
