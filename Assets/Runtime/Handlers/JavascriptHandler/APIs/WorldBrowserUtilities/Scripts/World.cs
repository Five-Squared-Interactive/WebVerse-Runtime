// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Core;
using FiveSQD.WebVerse.Interface.MultibarMenu;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for World utilities and lifecycle events.
    /// Provides static event methods (on/off/once) for world lifecycle events.
    /// </summary>
    public class World
    {
        #region Existing API

        /// <summary>
        /// Get a URL Query Parameter.
        /// </summary>
        /// <param name="key">Key of the Query Parameter.</param>
        /// <returns>The value of the Query Parameter, or null.</returns>
        public static string GetQueryParam(string key)
        {
            return WebVerseRuntime.Instance.straightFour.GetParam(key);
        }

        /// <summary>
        /// Get the URL of the currently loaded World or Web Page.
        /// </summary>
        /// <returns>The URL of the current World or Web Page, or null if none has been loaded.</returns>
        public static string GetWorldURL()
        {
            return WebVerseRuntime.Instance.currentURL;
        }

        /// <summary>
        /// Get the current World Load State.
        /// </summary>
        /// <returns>One of: unloaded, loadingworld, loadedworld, webpage, error.</returns>
        public static string GetWorldLoadState()
        {
            switch (WebVerseRuntime.Instance.state)
            {
                case WebVerseRuntime.RuntimeState.Unloaded:
                    return "unloaded";

                case WebVerseRuntime.RuntimeState.LoadingWorld:
                    return "loadingworld";

                case WebVerseRuntime.RuntimeState.LoadedWorld:
                    return "loadedworld";

                case WebVerseRuntime.RuntimeState.WebPage:
                    return "webpage";

                case WebVerseRuntime.RuntimeState.Error:
                default:
                    return "error";
            }
        }

        /// <summary>
        /// Load a World from a URL.
        /// </summary>
        /// <param name="url">The URL of the World to load.</param>
        public static void LoadWorld(string url)
        {
            LoadWorld(url, null);
        }

        /// <summary>
        /// Load a World from a URL, along with a script to run in the same JINT engine as the world's
        /// own scripts.
        /// </summary>
        /// <param name="url">The URL of the World to load.</param>
        /// <param name="requireScript">Either inline JavaScript logic, or a URI ending in ".js" pointing
        /// to a script resource. The script is prepended to the world's script list and runs first.
        /// Only supported for VEML worlds; ignored for x3d and glTF worlds.</param>
        public static void LoadWorld(string url, string requireScript)
        {
            WebVerseRuntime.Instance.LoadWorld(url, new System.Action<string>((name) =>
            {
                foreach (Multibar multibar in Multibar.GetMultibars())
                {
                    multibar.AddToHistory(System.DateTime.Now, name, url);
                    multibar.ToggleMultibar();
                    multibar.ToggleMultibar();
                }
            }), requireScript);
        }

        /// <summary>
        /// Dry-run validation of a World's VEML without switching to it. Downloads and parses the
        /// VEML, downloads (but does not execute) referenced scripts, and HEAD-requests referenced
        /// asset URIs. Reports the result via the JS callback. Does not unload the active world,
        /// mutate runtime state, or touch the JINT engine.
        /// </summary>
        /// <param name="url">The URL of the World to test.</param>
        /// <param name="onTestComplete">Name of a JS function to invoke when the test completes. The
        /// function is called with three arguments: (success: bool, errorMessage: string|null, title:
        /// string|null). On success, errorMessage is null. On failure, errorMessage is a
        /// newline-separated list of issues. title is the parsed metadata.title when the document
        /// parsed, otherwise null.</param>
        public static void TestLoadWorld(string url, string onTestComplete)
        {
            WebVerseRuntime.Instance.TestLoadWorld(url,
                new System.Action<bool, string, string>((success, errorMessage, title) =>
                {
                    if (string.IsNullOrEmpty(onTestComplete))
                    {
                        return;
                    }
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                        onTestComplete, new object[] { success, errorMessage, title });
                }));
        }

        /// <summary>
        /// Load a Web Page from a URL.
        /// </summary>
        /// <param name="url">The URL of the Web Page to load.</param>
        public static void LoadWebPage(string url)
        {
            WebVerseRuntime.Instance.LoadWebPage(url, new System.Action<string>((name) =>
            {
                foreach (Multibar multibar in Multibar.GetMultibars())
                {
                    multibar.AddToHistory(System.DateTime.Now, name, url);
                    multibar.ToggleMultibar();
                    multibar.ToggleMultibar();
                }
            }));
        }

        #endregion

        #region Event System

        /// <summary>
        /// Current API version for the World API event system.
        /// </summary>
        public static string apiVersion => "1.0.0";

        /// <summary>
        /// Event listener storage. Keys are event names, values are callback lists.
        /// </summary>
        private static Dictionary<string, List<Jint.Native.JsValue>> _listeners
            = new Dictionary<string, List<Jint.Native.JsValue>>();

        /// <summary>
        /// Tracks callbacks registered via once() for auto-removal after first fire.
        /// </summary>
        private static HashSet<Jint.Native.JsValue> _onceListeners
            = new HashSet<Jint.Native.JsValue>();

        /// <summary>
        /// Tracks event names currently being emitted for re-entrancy protection.
        /// </summary>
        private static HashSet<string> _emittingEvents
            = new HashSet<string>();

        /// <summary>
        /// Register an event listener on World. Returns an unsubscribe function.
        /// </summary>
        /// <param name="eventName">The event name to listen for (e.g., "ready", "load", "error").</param>
        /// <param name="callback">The function to invoke when the event fires.</param>
        /// <returns>An unsubscribe function that removes this listener when called.</returns>
        public static Func<bool> on(string eventName, Jint.Native.JsValue callback)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Logging.LogError("[EventSystem] Event name cannot be null or empty.");
                return () => false;
            }

            if (callback == null || callback == Jint.Native.JsValue.Undefined || callback == Jint.Native.JsValue.Null)
            {
                Logging.LogError($"[EventSystem] Callback for '{eventName}' is null or undefined.");
                return () => false;
            }

            if (!Events.IsValid(eventName))
            {
                Logging.LogWarning(
                    $"[EventSystem] Unrecognized event name: '{eventName}'. " +
                    "Check Events constants for valid names.");
            }

            if (!_listeners.ContainsKey(eventName))
                _listeners[eventName] = new List<Jint.Native.JsValue>();

            _listeners[eventName].Add(callback);

            bool unsubscribed = false;
            return () =>
            {
                if (unsubscribed) return false;
                unsubscribed = true;
                off(eventName, callback);
                return true;
            };
        }

        /// <summary>
        /// Register a one-time event listener that auto-removes after first invocation.
        /// </summary>
        /// <param name="eventName">The event name to listen for.</param>
        /// <param name="callback">The function to invoke once.</param>
        /// <returns>An unsubscribe function.</returns>
        public static Func<bool> once(string eventName, Jint.Native.JsValue callback)
        {
            var unsub = on(eventName, callback);

            // Only track if on() actually registered it
            if (!string.IsNullOrEmpty(eventName)
                && _listeners.ContainsKey(eventName)
                && _listeners[eventName].Contains(callback))
            {
                _onceListeners.Add(callback);
            }

            return unsub;
        }

        /// <summary>
        /// Remove a specific listener for an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="callback">The specific callback to remove.</param>
        public static void off(string eventName, Jint.Native.JsValue callback)
        {
            if (string.IsNullOrEmpty(eventName) || !_listeners.ContainsKey(eventName))
                return;

            bool removed = _listeners[eventName].Remove(callback);

            if (removed)
                _onceListeners.Remove(callback);

            if (_listeners[eventName].Count == 0)
                _listeners.Remove(eventName);
        }

        /// <summary>
        /// Remove all listeners for an event.
        /// </summary>
        /// <param name="eventName">The event name to clear.</param>
        public static void off(string eventName)
        {
            if (string.IsNullOrEmpty(eventName) || !_listeners.ContainsKey(eventName))
                return;

            foreach (var cb in _listeners[eventName])
                _onceListeners.Remove(cb);

            _listeners.Remove(eventName);
        }

        /// <summary>
        /// Emit a World event, invoking all registered listeners in registration order.
        /// Uses catch-log-continue: one listener's exception never blocks others.
        /// </summary>
        /// <param name="eventName">The event name to emit.</param>
        /// <param name="args">Arguments to pass to listeners.</param>
        internal static void Emit(string eventName, params Jint.Native.JsValue[] args)
        {
            if (string.IsNullOrEmpty(eventName) || !_listeners.ContainsKey(eventName))
                return;

            // Re-entrancy guard
            if (!_emittingEvents.Add(eventName))
            {
                Logging.LogWarning(
                    $"[EventSystem] Re-entrant Emit detected for World '{eventName}'. Skipping.");
                return;
            }

            try
            {
                var toRemove = new List<Jint.Native.JsValue>();

                foreach (var callback in _listeners[eventName].ToList())
                {
                    try
                    {
                        Runtime.WebVerseRuntime.Instance.javascriptHandler.Engine.Call(callback, Jint.Native.JsValue.Undefined, args);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError(
                            $"[EventSystem] Listener error for World '{eventName}': {ex.Message}");
                    }

                    if (_onceListeners.Contains(callback))
                    {
                        toRemove.Add(callback);
                    }
                }

                foreach (var cb in toRemove)
                {
                    _onceListeners.Remove(cb);
                    if (_listeners.ContainsKey(eventName))
                    {
                        _listeners[eventName].Remove(cb);
                        if (_listeners[eventName].Count == 0)
                            _listeners.Remove(eventName);
                    }
                }
            }
            finally
            {
                _emittingEvents.Remove(eventName);
            }
        }

        /// <summary>
        /// Clear all World event listeners. Called before loading a new world
        /// to prevent listener leaks between world navigations.
        /// </summary>
        internal static void DisposeAllWorldListeners()
        {
            _listeners.Clear();
            _onceListeners.Clear();
            _emittingEvents.Clear();
        }

        #endregion

        #region Debug

        /// <summary>
        /// Debug utilities for World event introspection.
        /// </summary>
        public static class debug
        {
            /// <summary>
            /// List all active World event listeners with event names and counts.
            /// </summary>
            /// <returns>Array of objects with event name and listener count.</returns>
            public static object[] listListeners()
            {
                var result = new List<object>();
                foreach (var kvp in _listeners)
                {
                    result.Add(new { @event = kvp.Key, count = kvp.Value.Count });
                }
                return result.ToArray();
            }
        }

        #endregion
    }
}
