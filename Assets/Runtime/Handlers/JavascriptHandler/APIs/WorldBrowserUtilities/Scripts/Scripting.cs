// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for scripting utilities.
    /// </summary>
    public class Scripting
    {
        /// <summary>
        /// Run a script.
        /// </summary>
        /// <param name="scriptToRun">Script to run.</param>
        public static void RunScript(string scriptToRun)
        {
            WebVerseRuntime.Instance.javascriptHandler.RunScript(scriptToRun);
        }
    }
}