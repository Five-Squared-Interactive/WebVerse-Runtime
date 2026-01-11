// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Utilities
{
    /// <summary>
    /// Log system for the World Engine.
    /// </summary>
    public class LogSystem
    {
        /// <summary>
        /// Type for a log message.
        /// </summary>
        public enum Type { Default, Debug, Warning, Error };

        /// <summary>
        /// Current logging configuration. If null, all logging is enabled.
        /// </summary>
        private static LoggingConfig loggingConfig;

        /// <summary>
        /// Set the logging configuration.
        /// </summary>
        /// <param name="config">The logging configuration to use. If null, all logging is enabled.</param>
        public static void SetLoggingConfig(LoggingConfig config)
        {
            loggingConfig = config;
        }

        /// <summary>
        /// Get the current logging configuration.
        /// </summary>
        /// <returns>The current logging configuration, or null if not set.</returns>
        public static LoggingConfig GetLoggingConfig()
        {
            return loggingConfig;
        }

        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="type">Type of the message.</param>
        public static void Log(string message, Type type = Type.Default)
        {
            // Check if this log type should be filtered out
            if (loggingConfig != null && !loggingConfig.ShouldLog(type))
            {
                return;
            }

            // Forward to Unity's Logging System.
            switch (type)
            {
                case Type.Debug:
                    // TODO: Only in development build.
                    Debug.Log(message);
                    break;

                case Type.Warning:
                    Debug.LogWarning(message);
                    break;

                case Type.Error:
                    Debug.LogError(message);
                    break;

                case Type.Default:
                default:
                    Debug.Log(message);
                    break;
            }

            // Log to consoles.
            //Parallels.Infrastructure.ConsoleCommandParser.LogMessage(message, type);
        }

        /// <summary>
        /// Log a debug message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void LogDebug(string message)
        {
            Log(message, Type.Debug);
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void LogWarning(string message)
        {
            Log(message, Type.Warning);
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void LogError(string message)
        {
            Log(message, Type.Error);
        }
    }
}