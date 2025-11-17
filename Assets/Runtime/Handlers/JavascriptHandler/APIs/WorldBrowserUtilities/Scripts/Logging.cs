// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for logging.
    /// </summary>
    public class Logging
    {
        /// <summary>
        /// Type for a log message.
        /// </summary>
        public enum Type { Default, Debug, Warning, Error };

        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="type">Type of the message.</param>
        public static void Log(string message, Type type = Type.Default)
        {
            WebVerse.Utilities.Logging.Type tp = WebVerse.Utilities.Logging.Type.Default;
            switch (type)
            {
                case Type.Default:
                    tp = WebVerse.Utilities.Logging.Type.ScriptDefault;
                    break;

                case Type.Debug:
                    tp = WebVerse.Utilities.Logging.Type.ScriptDebug;
                    break;

                case Type.Warning:
                    tp = WebVerse.Utilities.Logging.Type.ScriptWarning;
                    break;

                case Type.Error:
                    tp = WebVerse.Utilities.Logging.Type.ScriptError;
                    break;
            }

            WebVerse.Utilities.Logging.Log(message, tp);
        }

        /// <summary>
        /// Log a debug message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void LogDebug(string message)
        {
            WebVerse.Utilities.Logging.Log(message, WebVerse.Utilities.Logging.Type.ScriptDefault);
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void LogWarning(string message)
        {
            WebVerse.Utilities.Logging.Log(message, WebVerse.Utilities.Logging.Type.ScriptWarning);
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public static void LogError(string message)
        {
            WebVerse.Utilities.Logging.Log(message, WebVerse.Utilities.Logging.Type.ScriptError);
        }
    }
}