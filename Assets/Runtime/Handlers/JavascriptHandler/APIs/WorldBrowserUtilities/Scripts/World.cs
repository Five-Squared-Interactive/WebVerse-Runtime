// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Interface.MultibarMenu;
using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for World utilities.
    /// </summary>
    public class World
    {
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
    }
}