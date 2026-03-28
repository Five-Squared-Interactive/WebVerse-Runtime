// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0;

namespace FiveSQD.WebVerse.Handlers.Voice
{
    /// <summary>
    /// Binding configuration for auto-attaching voice speakers to entities.
    /// </summary>
    public class VoiceBinding
    {
        /// <summary>
        /// User pattern to match (e.g., "*" for all users, "user123" for specific user).
        /// </summary>
        public string UserPattern { get; set; } = "*";

        /// <summary>
        /// Entity tag to attach the speaker to.
        /// </summary>
        public string ToEntityTag { get; set; }

        /// <summary>
        /// Local offset from the entity (e.g., head/mouth position).
        /// </summary>
        public Vector3 Offset { get; set; } = Vector3.zero;

        /// <summary>
        /// Check if this binding matches a user ID.
        /// </summary>
        /// <param name="userId">The user ID to match.</param>
        /// <returns>True if the binding matches.</returns>
        public bool Matches(string userId)
        {
            if (string.IsNullOrEmpty(UserPattern) || UserPattern == "*")
            {
                return true;
            }
            return UserPattern == userId;
        }
    }

    /// <summary>
    /// Configuration for voice chat functionality.
    /// </summary>
    public class VoiceConfig
    {
        /// <summary>
        /// Voice provider type (e.g., "websocket").
        /// </summary>
        public string Provider { get; set; } = "websocket";

        /// <summary>
        /// Voice server endpoint URL. Required.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Minimum distance for spatial audio falloff. Default: 1.
        /// </summary>
        public float MinDistance { get; set; } = 1f;

        /// <summary>
        /// Maximum distance for spatial audio. Default: 25.
        /// </summary>
        public float MaxDistance { get; set; } = 25f;

        /// <summary>
        /// Audio rolloff mode (e.g., "logarithmic", "linear"). Default: "logarithmic".
        /// </summary>
        public string Rolloff { get; set; } = "logarithmic";

        /// <summary>
        /// Voice speaker bindings for auto-attaching to entities.
        /// </summary>
        public List<VoiceBinding> Bindings { get; set; } = new List<VoiceBinding>();

        /// <summary>
        /// Validate the configuration and throw VoiceException if invalid.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Endpoint))
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG,
                    "Voice endpoint is required.");
            }

            if (MinDistance < 0)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG,
                    "MinDistance cannot be negative.");
            }

            if (MaxDistance < 0)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG,
                    "MaxDistance cannot be negative.");
            }

            if (MinDistance > MaxDistance)
            {
                throw new VoiceException(VoiceErrorCode.VOICE_INVALID_CONFIG,
                    "MinDistance cannot be greater than MaxDistance.");
            }
        }

        /// <summary>
        /// Create a VoiceConfig from a VEML voice element.
        /// </summary>
        /// <param name="vemlVoice">The VEML voice element.</param>
        /// <returns>A VoiceConfig, or null if the element is null.</returns>
        public static VoiceConfig FromVEML(voice vemlVoice)
        {
            if (vemlVoice == null)
            {
                return null;
            }

            try
            {
                var config = new VoiceConfig
                {
                    Provider = vemlVoice.provider ?? "websocket",
                    Endpoint = vemlVoice.endpoint,
                    MinDistance = vemlVoice.mindistanceSpecified ? vemlVoice.mindistance : 1f,
                    MaxDistance = vemlVoice.maxdistanceSpecified ? vemlVoice.maxdistance : 25f,
                    Rolloff = vemlVoice.rolloff ?? "logarithmic"
                };

                // Parse bindings
                if (vemlVoice.bindings != null)
                {
                    foreach (var binding in vemlVoice.bindings)
                    {
                        config.Bindings.Add(new VoiceBinding
                        {
                            UserPattern = binding.userpattern ?? "*",
                            ToEntityTag = binding.toentitytag,
                            Offset = ParseOffset(binding.offset)
                        });
                    }
                }

                return config;
            }
            catch (System.Exception ex)
            {
                Logging.LogError($"[VoiceConfig->FromVEML] Failed to parse voice configuration: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse an offset string (e.g., "0,1.7,0") to a Vector3.
        /// </summary>
        private static Vector3 ParseOffset(string offsetStr)
        {
            if (string.IsNullOrEmpty(offsetStr))
            {
                return Vector3.zero;
            }

            try
            {
                string[] parts = offsetStr.Split(',');
                if (parts.Length >= 3)
                {
                    return new Vector3(
                        float.Parse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture)
                    );
                }
            }
            catch (System.Exception ex)
            {
                Logging.LogWarning($"[VoiceConfig] Failed to parse offset '{offsetStr}': {ex.Message}");
            }

            return Vector3.zero;
        }
    }
}
