// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Static utility that maps technical avatar loading errors to user-friendly messages.
    /// Follows the pattern of BaseWorldLoadingErrorHandler.GetUserFriendlyErrorMessage().
    /// </summary>
    public static class AvatarNotification
    {
        /// <summary>
        /// Maps a technical error message to a user-friendly notification string.
        /// Uses keyword matching to classify the error type.
        /// </summary>
        /// <param name="technicalError">The raw technical error string from the loading pipeline.</param>
        /// <returns>A user-friendly message suitable for display.</returns>
        public static string MapErrorToUserMessage(string technicalError)
        {
            if (string.IsNullOrEmpty(technicalError))
            {
                return "Something went wrong loading the avatar. The default avatar will be used.";
            }

            var lower = technicalError.ToLowerInvariant();

            // Skeleton validation failures
            if (lower.Contains("missing") && (lower.Contains("bone") || lower.Contains("skeleton")))
            {
                return "This avatar model isn't compatible. It's missing required bones for animation.";
            }

            // Null/empty URI
            if (lower.Contains("null") && lower.Contains("empty"))
            {
                return "No avatar file was specified.";
            }

            // Network/download failures
            if (lower.Contains("network") || lower.Contains("download") ||
                lower.Contains("http") || lower.Contains("timeout"))
            {
                return "Couldn't download the avatar. Check your connection and try again.";
            }

            // Load failures (URI-based)
            if (lower.Contains("failed to load") && lower.Contains("uri"))
            {
                return "Couldn't load the avatar file. The file may be unavailable or the address may be incorrect.";
            }

            // Instantiation failures (file parsed but couldn't be used)
            if (lower.Contains("instantiate"))
            {
                return "The avatar file couldn't be set up. It may not be a supported avatar model.";
            }

            // Parse/format errors
            if (lower.Contains("parse") || lower.Contains("corrupt") ||
                lower.Contains("format") || lower.Contains("gltf"))
            {
                return "The avatar file appears to be damaged or in an unsupported format.";
            }

            // Generic fallback
            return "Something went wrong loading the avatar. The default avatar will be used.";
        }
    }
}
