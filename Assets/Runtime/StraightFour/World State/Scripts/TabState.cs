// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.StraightFour.WorldState
{
    /// <summary>
    /// Represents the loading state of a tab.
    /// </summary>
    public enum TabLoadState
    {
        /// <summary>Tab created but world not yet loaded.</summary>
        Unloaded,
        /// <summary>World is currently loading.</summary>
        Loading,
        /// <summary>World is fully loaded and active.</summary>
        Loaded,
        /// <summary>World was unloaded but state is preserved.</summary>
        Suspended,
        /// <summary>An error occurred during loading.</summary>
        Error
    }

    /// <summary>
    /// Represents the state of a single tab in the tabbed runtime.
    /// </summary>
    public class TabState
    {
        /// <summary>
        /// Unique identifier for this tab.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The world URL or identifier for this tab.
        /// </summary>
        public string WorldUrl { get; set; }

        /// <summary>
        /// Display name for the tab (defaults to URL if not set).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Current loading state of the tab.
        /// </summary>
        public TabLoadState LoadState { get; set; }

        /// <summary>
        /// The snapshot ID if this tab's state has been captured.
        /// Null if no snapshot exists.
        /// </summary>
        public string SnapshotId { get; set; }

        /// <summary>
        /// When the tab was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// When the tab was last active.
        /// </summary>
        public DateTime LastActiveAt { get; set; }

        /// <summary>
        /// Base path for world resources.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Error message if LoadState is Error.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Whether this tab can be closed.
        /// </summary>
        public bool CanClose { get; set; } = true;

        /// <summary>
        /// Whether this tab is displaying a webpage (not a VOS world).
        /// Webpage tabs skip the world load/unload pipeline during tab switches.
        /// </summary>
        public bool IsWebPage { get; set; }

        /// <summary>
        /// Custom metadata for the tab.
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Create a new tab state.
        /// </summary>
        /// <param name="worldUrl">The world URL for this tab.</param>
        /// <param name="displayName">Optional display name.</param>
        public TabState(string worldUrl, string displayName = null)
        {
            Id = Guid.NewGuid().ToString();
            WorldUrl = worldUrl;
            DisplayName = displayName ?? ExtractDisplayName(worldUrl);
            LoadState = TabLoadState.Unloaded;
            CreatedAt = DateTime.UtcNow;
            LastActiveAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Mark this tab as active now.
        /// </summary>
        public void MarkActive()
        {
            LastActiveAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Get the effective display name for the tab.
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(DisplayName))
                return DisplayName;

            return ExtractDisplayName(WorldUrl);
        }

        /// <summary>
        /// Extract a display name from a URL.
        /// </summary>
        private static string ExtractDisplayName(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "New Tab";

            try
            {
                // Try to extract domain or last path segment
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                {
                    return uri.Host;
                }

                // Fallback: use last segment of path
                int lastSlash = url.LastIndexOf('/');
                if (lastSlash >= 0 && lastSlash < url.Length - 1)
                {
                    return url.Substring(lastSlash + 1);
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return url.Length > 30 ? url.Substring(0, 27) + "..." : url;
        }

        /// <summary>
        /// Create a copy of this tab state.
        /// </summary>
        public TabState Clone()
        {
            return new TabState(WorldUrl, DisplayName)
            {
                LoadState = this.LoadState,
                SnapshotId = this.SnapshotId,
                BasePath = this.BasePath,
                ErrorMessage = this.ErrorMessage,
                CanClose = this.CanClose,
                Metadata = this.Metadata
            };
        }
    }
}
