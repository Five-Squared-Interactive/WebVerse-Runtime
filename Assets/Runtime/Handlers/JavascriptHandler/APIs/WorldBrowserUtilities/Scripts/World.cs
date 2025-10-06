// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

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
            WebVerseRuntime.Instance.LoadWorld(url, new System.Action<string>((name) =>
            {
                foreach (Multibar multibar in Multibar.GetMultibars())
                {
                    multibar.AddToHistory(System.DateTime.Now, name, url);
                    multibar.ToggleMultibar();
                    multibar.ToggleMultibar();
                }
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