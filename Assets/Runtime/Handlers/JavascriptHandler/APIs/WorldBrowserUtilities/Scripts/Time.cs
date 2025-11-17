// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.StraightFour.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for time utilities.
    /// </summary>
    public class Time
    {
        /// <summary>
        /// Set interval at which to run a function.
        /// </summary>
        /// <param name="function">Function to run.</param>
        /// <param name="interval">Interval at which to run the function.</param>
        /// <returns>ID of the registered function, or null.</returns>
        public static UUID SetInterval(string function, float interval)
        {
            Guid id = WebVerseRuntime.Instance.timeHandler.StartInvoking(function, interval);
            if (id == null)
            {
                LogSystem.LogError("[Time->SetInterval] Unable to assign ID.");
                return null;
            }
            return new UUID(id.ToString());
        }

        public static bool CallAsynchronously(string function)
        {
            WebVerseRuntime.Instance.timeHandler.CallAsynchronously(function);

            return true;
        }

        /// <summary>
        /// Stop running a registered function.
        /// </summary>
        /// <param name="id">ID of the registered function to stop running.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool StopInterval(string id)
        {
            Guid uuid = Guid.Parse(id);
            if (uuid == null)
            {
                LogSystem.LogWarning("[Time->StopInterval] Invalid ID.");
                return false;
            }
            WebVerseRuntime.Instance.timeHandler.StopInvoking(uuid);
            return true;
        }

        /// <summary>
        /// Set timeout after which to run logic.
        /// </summary>
        /// <param name="logic">Logic to run.</param>
        /// <param name="timeout">Timeout after which to run the specified logic.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetTimeout(string logic, int timeout)
        {
            WebVerseRuntime.Instance.javascriptHandler.RunScriptAfterTimeout(logic, timeout);
            return true;
        }
    }
}