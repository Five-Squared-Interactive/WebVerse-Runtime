// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for contexts.
    /// </summary>
    public class Context
    {
        /// <summary>
        /// Set a context.
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        /// <param name="context">Context.</param>
        public static void DefineContext(string contextName, object context)
        {
            WebVerseRuntime.Instance.javascriptHandler.DefineContext(contextName, context);
        }

        /// <summary>
        /// Get a context.
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        /// <returns>Context.</returns>
        public static object GetContext(string contextName)
        {
            return WebVerseRuntime.Instance.javascriptHandler.GetContext(contextName);
        }
    }
}