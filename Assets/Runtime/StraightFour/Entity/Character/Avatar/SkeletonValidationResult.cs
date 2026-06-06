// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Result of validating a skeleton against humanoid bone requirements.
    /// </summary>
    public struct SkeletonValidationResult
    {
        /// <summary>
        /// Whether the skeleton passed validation.
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// Descriptive message explaining the validation outcome.
        /// Null or empty for valid results.
        /// </summary>
        public string Message;

        /// <summary>
        /// List of bone names that were expected but not found.
        /// Null or empty for valid results.
        /// </summary>
        public List<string> MissingBones;

        /// <summary>
        /// Creates a valid result indicating the skeleton conforms to humanoid requirements.
        /// </summary>
        public static SkeletonValidationResult Valid()
        {
            return new SkeletonValidationResult
            {
                IsValid = true,
                Message = null,
                MissingBones = null
            };
        }

        /// <summary>
        /// Creates an invalid result with a descriptive message and list of missing bones.
        /// </summary>
        public static SkeletonValidationResult Invalid(string message, List<string> missingBones)
        {
            return new SkeletonValidationResult
            {
                IsValid = false,
                Message = message,
                MissingBones = missingBones != null ? new List<string>(missingBones) : new List<string>()
            };
        }
    }
}
