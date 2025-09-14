// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Utilities
{
    /// <summary>
    /// Configuration settings for the logging system.
    /// </summary>
    [Serializable]
    public struct LoggingConfiguration
    {
        /// <summary>
        /// Whether to enable console output for log messages.
        /// </summary>
        public bool enableConsoleOutput;

        /// <summary>
        /// Whether to enable logging for Default type messages.
        /// </summary>
        public bool enableDefault;

        /// <summary>
        /// Whether to enable logging for Debug type messages.
        /// </summary>
        public bool enableDebug;

        /// <summary>
        /// Whether to enable logging for Warning type messages.
        /// </summary>
        public bool enableWarning;

        /// <summary>
        /// Whether to enable logging for Error type messages.
        /// </summary>
        public bool enableError;

        /// <summary>
        /// Whether to enable logging for ScriptDefault type messages.
        /// </summary>
        public bool enableScriptDefault;

        /// <summary>
        /// Whether to enable logging for ScriptDebug type messages.
        /// </summary>
        public bool enableScriptDebug;

        /// <summary>
        /// Whether to enable logging for ScriptWarning type messages.
        /// </summary>
        public bool enableScriptWarning;

        /// <summary>
        /// Whether to enable logging for ScriptError type messages.
        /// </summary>
        public bool enableScriptError;

        /// <summary>
        /// Creates a default logging configuration with all features enabled.
        /// </summary>
        /// <returns>Default logging configuration.</returns>
        public static LoggingConfiguration CreateDefault()
        {
            return new LoggingConfiguration
            {
                enableConsoleOutput = true,
                enableDefault = true,
                enableDebug = true,
                enableWarning = true,
                enableError = true,
                enableScriptDefault = true,
                enableScriptDebug = true,
                enableScriptWarning = true,
                enableScriptError = true
            };
        }

        /// <summary>
        /// Creates a production logging configuration with limited output.
        /// </summary>
        /// <returns>Production logging configuration.</returns>
        public static LoggingConfiguration CreateProduction()
        {
            return new LoggingConfiguration
            {
                enableConsoleOutput = false,
                enableDefault = false,
                enableDebug = false,
                enableWarning = true,
                enableError = true,
                enableScriptDefault = false,
                enableScriptDebug = false,
                enableScriptWarning = true,
                enableScriptError = true
            };
        }
    }
}