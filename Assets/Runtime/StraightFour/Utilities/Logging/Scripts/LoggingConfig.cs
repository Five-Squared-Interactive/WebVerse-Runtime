// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.StraightFour.Utilities
{
    /// <summary>
    /// Configuration for the logging system.
    /// </summary>
    [Serializable]
    public class LoggingConfig
    {
        /// <summary>
        /// Whether debug logging is enabled.
        /// </summary>
        public bool enableDebug = true;

        /// <summary>
        /// Whether warning logging is enabled.
        /// </summary>
        public bool enableWarning = true;

        /// <summary>
        /// Whether error logging is enabled.
        /// </summary>
        public bool enableError = true;

        /// <summary>
        /// Whether default/info logging is enabled.
        /// </summary>
        public bool enableDefault = true;

        /// <summary>
        /// Default constructor with all logging enabled.
        /// </summary>
        public LoggingConfig()
        {
        }

        /// <summary>
        /// Constructor with specific log level configuration.
        /// </summary>
        /// <param name="enableDebug">Enable debug logging.</param>
        /// <param name="enableWarning">Enable warning logging.</param>
        /// <param name="enableError">Enable error logging.</param>
        /// <param name="enableDefault">Enable default/info logging.</param>
        public LoggingConfig(bool enableDebug, bool enableWarning, bool enableError, bool enableDefault)
        {
            this.enableDebug = enableDebug;
            this.enableWarning = enableWarning;
            this.enableError = enableError;
            this.enableDefault = enableDefault;
        }

        /// <summary>
        /// Check if a specific log type should be logged based on the configuration.
        /// </summary>
        /// <param name="type">The log type to check.</param>
        /// <returns>True if the log type should be logged, false otherwise.</returns>
        public bool ShouldLog(LogSystem.Type type)
        {
            switch (type)
            {
                case LogSystem.Type.Debug:
                    return enableDebug;
                case LogSystem.Type.Warning:
                    return enableWarning;
                case LogSystem.Type.Error:
                    return enableError;
                case LogSystem.Type.Default:
                default:
                    return enableDefault;
            }
        }
    }
}