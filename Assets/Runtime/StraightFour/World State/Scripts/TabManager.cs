// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiveSQD.StraightFour.Utilities;
using UnityEngine;

namespace FiveSQD.StraightFour.WorldState
{
    /// <summary>
    /// Manages tabs for the tabbed runtime system.
    /// Handles tab creation, switching, closing, and state management.
    /// </summary>
    public class TabManager : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Fired when a new tab is created.
        /// </summary>
        public event Action<TabState> OnTabCreated;

        /// <summary>
        /// Fired when a tab is closed.
        /// </summary>
        public event Action<TabState> OnTabClosed;

        /// <summary>
        /// Fired when switching tabs begins.
        /// </summary>
        public event Action<TabState, TabState> OnTabSwitchStarted;

        /// <summary>
        /// Fired when switching tabs completes.
        /// </summary>
        public event Action<TabState, TabState, bool> OnTabSwitchCompleted;

        /// <summary>
        /// Fired when the active tab changes.
        /// </summary>
        public event Action<TabState> OnActiveTabChanged;

        /// <summary>
        /// Fired when a tab's state changes.
        /// </summary>
        public event Action<TabState> OnTabStateChanged;

        /// <summary>
        /// Fired when a tab fails to load.
        /// </summary>
        public event Action<TabState, string> OnTabLoadFailed;

        /// <summary>
        /// Fired when a tab thumbnail is captured.
        /// Parameters: tabId, base64 data URL.
        /// </summary>
        public event Action<string, string> OnTabThumbnailCaptured;

        /// <summary>
        /// Fired when a tab switch needs to navigate to a URL
        /// (e.g., restoring a webpage tab). Parameter: URL to navigate to.
        /// </summary>
        public event Action<string> OnTabNavigateRequested;

        #endregion

        #region Properties

        /// <summary>
        /// Maximum number of tabs allowed.
        /// </summary>
        public int MaxTabs { get; set; } = 10;

        /// <summary>
        /// Currently active tab.
        /// </summary>
        public TabState ActiveTab { get; private set; }

        /// <summary>
        /// All open tabs.
        /// </summary>
        public IReadOnlyList<TabState> Tabs => tabs.AsReadOnly();

        /// <summary>
        /// Number of open tabs.
        /// </summary>
        public int TabCount => tabs.Count;

        /// <summary>
        /// Whether a tab switch is currently in progress.
        /// </summary>
        public bool IsSwitching { get; private set; }

        #endregion

        #region Private Fields

        private List<TabState> tabs = new List<TabState>();
        private WorldStateManager stateManager;
        private WorldStateRestorer stateRestorer;
        public TabThumbnailService ThumbnailService => thumbnailService;
        private TabThumbnailService thumbnailService;
        private World.World currentWorld;

        // Callbacks for world loading integration
        private Func<string, string, Action<World.World, bool>, Coroutine> loadWorldCallback;
        private Action<World.World> unloadWorldCallback;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the tab manager.
        /// </summary>
        /// <param name="stateManager">World state manager for snapshots.</param>
        /// <param name="loadWorld">Callback to load a world. Parameters: (url, basePath, onComplete).</param>
        /// <param name="unloadWorld">Callback to unload the current world.</param>
        public void Initialize(
            WorldStateManager stateManager,
            Func<string, string, Action<World.World, bool>, Coroutine> loadWorld,
            Action<World.World> unloadWorld)
        {
            this.stateManager = stateManager;
            this.loadWorldCallback = loadWorld;
            this.unloadWorldCallback = unloadWorld;

            // Create or get the restorer component
            stateRestorer = GetComponent<WorldStateRestorer>();
            if (stateRestorer == null)
            {
                stateRestorer = gameObject.AddComponent<WorldStateRestorer>();
            }

            // Create or get the thumbnail service component
            thumbnailService = GetComponent<TabThumbnailService>();
            if (thumbnailService == null)
            {
                thumbnailService = gameObject.AddComponent<TabThumbnailService>();
            }
            thumbnailService.OnThumbnailCaptured += HandleThumbnailCaptured;

            LogSystem.Log("[TabManager] Initialized.");
        }

        /// <summary>
        /// Set the current world reference (for state capture).
        /// </summary>
        public void SetCurrentWorld(World.World world)
        {
            currentWorld = world;
            if (ActiveTab != null)
            {
                ActiveTab.LoadState = TabLoadState.Loaded;
                OnTabStateChanged?.Invoke(ActiveTab);
            }
        }

        #endregion

        #region Tab Creation

        /// <summary>
        /// Create a new tab.
        /// </summary>
        /// <param name="worldUrl">URL of the world to load.</param>
        /// <param name="displayName">Optional display name.</param>
        /// <param name="makeActive">Whether to switch to this tab immediately.</param>
        /// <returns>The created tab state, or null if max tabs reached.</returns>
        public TabState CreateTab(string worldUrl, string displayName = null, bool makeActive = true)
        {
            if (tabs.Count >= MaxTabs)
            {
                LogSystem.LogWarning($"[TabManager->CreateTab] Maximum tabs ({MaxTabs}) reached.");
                return null;
            }

            var tab = new TabState(worldUrl, displayName);
            tabs.Add(tab);

            LogSystem.Log($"[TabManager->CreateTab] Created tab '{tab.GetDisplayName()}' ({tab.Id}).");
            OnTabCreated?.Invoke(tab);

            if (makeActive)
            {
                SwitchToTab(tab.Id);
            }

            return tab;
        }

        /// <summary>
        /// Create a new empty tab (home/start page).
        /// </summary>
        /// <param name="makeActive">Whether to switch to this tab immediately.</param>
        /// <returns>The created tab state.</returns>
        public TabState CreateEmptyTab(bool makeActive = true)
        {
            return CreateTab(null, "New Tab", makeActive);
        }

        #endregion

        #region Tab Switching

        /// <summary>
        /// Switch to a tab by ID.
        /// </summary>
        /// <param name="tabId">ID of the tab to switch to.</param>
        /// <param name="onComplete">Optional callback when switch completes.</param>
        public void SwitchToTab(string tabId, Action<bool> onComplete = null)
        {
            var targetTab = tabs.FirstOrDefault(t => t.Id == tabId);
            if (targetTab == null)
            {
                LogSystem.LogWarning($"[TabManager->SwitchToTab] Tab not found: {tabId}");
                onComplete?.Invoke(false);
                return;
            }

            if (ActiveTab?.Id == tabId)
            {
                LogSystem.Log($"[TabManager->SwitchToTab] Already on tab {tabId}.");
                onComplete?.Invoke(true);
                return;
            }

            if (IsSwitching)
            {
                LogSystem.LogWarning("[TabManager->SwitchToTab] Switch already in progress.");
                onComplete?.Invoke(false);
                return;
            }

            StartCoroutine(SwitchToTabCoroutine(targetTab, onComplete));
        }

        /// <summary>
        /// Switch to tab by index.
        /// </summary>
        public void SwitchToTabByIndex(int index, Action<bool> onComplete = null)
        {
            if (index < 0 || index >= tabs.Count)
            {
                LogSystem.LogWarning($"[TabManager->SwitchToTabByIndex] Invalid index: {index}");
                onComplete?.Invoke(false);
                return;
            }

            SwitchToTab(tabs[index].Id, onComplete);
        }

        /// <summary>
        /// Switch to the next tab.
        /// </summary>
        public void SwitchToNextTab(Action<bool> onComplete = null)
        {
            if (tabs.Count <= 1)
            {
                onComplete?.Invoke(false);
                return;
            }

            int currentIndex = ActiveTab != null ? tabs.IndexOf(ActiveTab) : -1;
            int nextIndex = (currentIndex + 1) % tabs.Count;
            SwitchToTabByIndex(nextIndex, onComplete);
        }

        /// <summary>
        /// Switch to the previous tab.
        /// </summary>
        public void SwitchToPreviousTab(Action<bool> onComplete = null)
        {
            if (tabs.Count <= 1)
            {
                onComplete?.Invoke(false);
                return;
            }

            int currentIndex = ActiveTab != null ? tabs.IndexOf(ActiveTab) : 0;
            int prevIndex = (currentIndex - 1 + tabs.Count) % tabs.Count;
            SwitchToTabByIndex(prevIndex, onComplete);
        }

        private IEnumerator SwitchToTabCoroutine(TabState targetTab, Action<bool> onComplete)
        {
            IsSwitching = true;
            var previousTab = ActiveTab;

            LogSystem.Log($"[TabManager] Switching from '{previousTab?.GetDisplayName() ?? "none"}' to '{targetTab.GetDisplayName()}'.");
            OnTabSwitchStarted?.Invoke(previousTab, targetTab);

            // Phase 1: Capture thumbnail and world state
            if (previousTab != null && previousTab.LoadState == TabLoadState.Loaded)
            {
                // Capture thumbnail before switching (works for both worlds and webpages)
                LogSystem.Log("[TabManager] Capturing thumbnail for outgoing tab...");
                thumbnailService?.CaptureThumbnail(previousTab.Id);

                // Capture world state (only for VOS worlds)
                if (currentWorld != null)
                {
                    LogSystem.Log("[TabManager] Capturing current world state...");
                    var snapshot = WorldStateSerializer.CaptureWorldState(
                        currentWorld,
                        previousTab.WorldUrl,
                        previousTab.BasePath
                    );

                    if (snapshot != null)
                    {
                        stateManager.AddSnapshot(previousTab.WorldUrl, snapshot);
                        previousTab.SnapshotId = snapshot.timestamp.ToString();
                        previousTab.LoadState = TabLoadState.Suspended;
                        OnTabStateChanged?.Invoke(previousTab);
                    }
                }
            }

            // Phase 2: Unload current world
            if (currentWorld != null)
            {
                LogSystem.Log("[TabManager] Unloading current world...");
                unloadWorldCallback?.Invoke(currentWorld);
                currentWorld = null;
                yield return null; // Allow cleanup
            }

            // Phase 3: Set new active tab
            ActiveTab = targetTab;
            targetTab.MarkActive();
            OnActiveTabChanged?.Invoke(targetTab);

            // Phase 4: Load target content
            if (!string.IsNullOrEmpty(targetTab.WorldUrl))
            {
                if (targetTab.IsWebPage)
                {
                    // Webpage tabs: navigate directly, skip world pipeline
                    LogSystem.Log($"[TabManager] Navigating to webpage: {targetTab.WorldUrl}");
                    targetTab.LoadState = TabLoadState.Loaded;
                    OnTabStateChanged?.Invoke(targetTab);
                    OnTabNavigateRequested?.Invoke(targetTab.WorldUrl);

                    // Schedule thumbnail captures after page loads
                    StartCoroutine(CaptureDelayedThumbnail(targetTab.Id, 2.0f));
                    StartCoroutine(CaptureDelayedThumbnail(targetTab.Id, 15.0f));
                }
                else
                {
                    // World tabs: load through world pipeline
                    targetTab.LoadState = TabLoadState.Loading;
                    OnTabStateChanged?.Invoke(targetTab);

                    bool loadComplete = false;
                    bool loadSuccess = false;
                    World.World loadedWorld = null;

                    LogSystem.Log($"[TabManager] Loading world: {targetTab.WorldUrl}");

                    loadWorldCallback?.Invoke(targetTab.WorldUrl, targetTab.BasePath, (world, success) =>
                    {
                        loadComplete = true;
                        loadSuccess = success;
                        loadedWorld = world;
                    });

                    // Wait for load to complete (with timeout)
                    float switchTimeout = 120f;
                    float switchElapsed = 0f;
                    while (!loadComplete && switchElapsed < switchTimeout)
                    {
                        switchElapsed += Time.deltaTime;
                        yield return null;
                    }

                    if (!loadComplete)
                    {
                        LogSystem.LogWarning("[TabManager] Tab switch load timed out.");
                        loadComplete = true;
                        loadSuccess = false;
                    }

                    if (loadSuccess && loadedWorld != null)
                    {
                        currentWorld = loadedWorld;

                        // Phase 5: Restore state if we have a snapshot
                        if (!string.IsNullOrEmpty(targetTab.SnapshotId))
                        {
                            var snapshot = stateManager.GetLatestSnapshot(targetTab.WorldUrl);
                            if (snapshot != null)
                            {
                                LogSystem.Log("[TabManager] Restoring world state...");
                                bool restoreComplete = false;

                                stateRestorer.RestoreWorldState(loadedWorld, snapshot, (restoreSuccess) =>
                                {
                                    restoreComplete = true;
                                    if (!restoreSuccess)
                                    {
                                        LogSystem.LogWarning("[TabManager] State restoration failed.");
                                    }
                                });

                                while (!restoreComplete)
                                {
                                    yield return null;
                                }
                            }
                        }

                        targetTab.LoadState = TabLoadState.Loaded;
                        OnTabStateChanged?.Invoke(targetTab);

                        // Schedule multi-capture thumbnails for the new tab
                        StartCoroutine(CaptureDelayedThumbnail(targetTab.Id, 0.5f));
                        StartCoroutine(CaptureDelayedThumbnail(targetTab.Id, 15.0f));
                    }
                    else if (loadSuccess && targetTab.IsWebPage)
                    {
                        // Webpage loaded successfully through world pipeline
                        LogSystem.Log("[TabManager] Webpage loaded successfully.");

                        // Schedule thumbnail captures after page loads
                        StartCoroutine(CaptureDelayedThumbnail(targetTab.Id, 2.0f));
                        StartCoroutine(CaptureDelayedThumbnail(targetTab.Id, 15.0f));
                    }
                    else
                    {
                        targetTab.LoadState = TabLoadState.Error;
                        targetTab.ErrorMessage = "Failed to load world.";
                        OnTabStateChanged?.Invoke(targetTab);
                        OnTabLoadFailed?.Invoke(targetTab, targetTab.ErrorMessage);
                    }
                }
            }

            IsSwitching = false;
            LogSystem.Log($"[TabManager] Switch complete. Active tab: '{targetTab.GetDisplayName()}'");
            OnTabSwitchCompleted?.Invoke(previousTab, targetTab, true);
            onComplete?.Invoke(true);
        }

        /// <summary>
        /// Fire OnTabStateChanged for the given tab (used by external callers
        /// that modify tab state directly, e.g. display name updates).
        /// </summary>
        public void NotifyTabStateChanged(TabState tab)
        {
            if (tab != null) OnTabStateChanged?.Invoke(tab);
        }

        /// <summary>
        /// Schedule a thumbnail capture for the given tab after a delay.
        /// </summary>
        public void ScheduleThumbnailCapture(string tabId, float delaySeconds)
        {
            StartCoroutine(CaptureDelayedThumbnail(tabId, delaySeconds));
        }

        private IEnumerator CaptureDelayedThumbnail(string tabId, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);

            // Only capture if this tab is still active
            if (ActiveTab?.Id == tabId)
            {
                thumbnailService?.CaptureThumbnail(tabId);
            }
        }

        #endregion

        #region Tab Closing

        /// <summary>
        /// Close a tab by ID.
        /// </summary>
        /// <param name="tabId">ID of the tab to close.</param>
        /// <returns>True if tab was closed.</returns>
        public bool CloseTab(string tabId)
        {
            var tab = tabs.FirstOrDefault(t => t.Id == tabId);
            if (tab == null)
            {
                LogSystem.LogWarning($"[TabManager->CloseTab] Tab not found: {tabId}");
                return false;
            }

            if (!tab.CanClose)
            {
                LogSystem.LogWarning($"[TabManager->CloseTab] Tab cannot be closed: {tabId}");
                return false;
            }

            // If closing the active tab, switch to another first
            if (ActiveTab?.Id == tabId)
            {
                int currentIndex = tabs.IndexOf(tab);
                int newIndex = currentIndex > 0 ? currentIndex - 1 : (tabs.Count > 1 ? 1 : -1);

                if (newIndex >= 0)
                {
                    // Switch to adjacent tab, then close
                    SwitchToTab(tabs[newIndex].Id, (success) =>
                    {
                        PerformCloseTab(tab);
                    });
                    return true;
                }
                else
                {
                    // Last tab - just close and unload
                    if (currentWorld != null)
                    {
                        unloadWorldCallback?.Invoke(currentWorld);
                        currentWorld = null;
                    }
                    ActiveTab = null;
                }
            }

            PerformCloseTab(tab);
            return true;
        }

        private void PerformCloseTab(TabState tab)
        {
            // Remove any stored snapshots for this tab
            if (!string.IsNullOrEmpty(tab.WorldUrl) && stateManager != null)
            {
                stateManager.RemoveSnapshots(tab.WorldUrl);
            }

            // Remove thumbnail for this tab
            thumbnailService?.RemoveThumbnail(tab.Id);

            tabs.Remove(tab);
            LogSystem.Log($"[TabManager->CloseTab] Closed tab '{tab.GetDisplayName()}'.");
            OnTabClosed?.Invoke(tab);
        }

        /// <summary>
        /// Close all tabs except the active one.
        /// </summary>
        public void CloseOtherTabs()
        {
            var tabsToClose = tabs.Where(t => t.Id != ActiveTab?.Id && t.CanClose).ToList();
            foreach (var tab in tabsToClose)
            {
                PerformCloseTab(tab);
            }
        }

        /// <summary>
        /// Close all tabs to the right of the active tab.
        /// </summary>
        public void CloseTabsToRight()
        {
            if (ActiveTab == null) return;

            int activeIndex = tabs.IndexOf(ActiveTab);
            var tabsToClose = tabs.Skip(activeIndex + 1).Where(t => t.CanClose).ToList();
            foreach (var tab in tabsToClose)
            {
                PerformCloseTab(tab);
            }
        }

        #endregion

        #region Tab Queries

        /// <summary>
        /// Get a tab by ID.
        /// </summary>
        public TabState GetTab(string tabId)
        {
            return tabs.FirstOrDefault(t => t.Id == tabId);
        }

        /// <summary>
        /// Get a tab by index.
        /// </summary>
        public TabState GetTabByIndex(int index)
        {
            if (index < 0 || index >= tabs.Count) return null;
            return tabs[index];
        }

        /// <summary>
        /// Get the index of a tab.
        /// </summary>
        public int GetTabIndex(string tabId)
        {
            return tabs.FindIndex(t => t.Id == tabId);
        }

        /// <summary>
        /// Find tabs by world URL.
        /// </summary>
        public IEnumerable<TabState> FindTabsByUrl(string worldUrl)
        {
            return tabs.Where(t => t.WorldUrl == worldUrl);
        }

        /// <summary>
        /// Check if a tab with the given URL exists.
        /// </summary>
        public bool HasTabWithUrl(string worldUrl)
        {
            return tabs.Any(t => t.WorldUrl == worldUrl);
        }

        #endregion

        #region Tab Reordering

        /// <summary>
        /// Move a tab to a new position.
        /// </summary>
        public bool MoveTab(string tabId, int newIndex)
        {
            var tab = tabs.FirstOrDefault(t => t.Id == tabId);
            if (tab == null) return false;

            newIndex = Mathf.Clamp(newIndex, 0, tabs.Count - 1);
            tabs.Remove(tab);
            tabs.Insert(newIndex, tab);

            return true;
        }

        #endregion

        #region Thumbnails

        /// <summary>
        /// Request a thumbnail for a specific tab.
        /// If the thumbnail is cached, fires OnTabThumbnailCaptured immediately.
        /// If not cached and the tab is the active tab, captures a new thumbnail.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        public void RequestThumbnail(string tabId)
        {
            if (string.IsNullOrEmpty(tabId) || thumbnailService == null)
                return;

            // Check if we have a cached thumbnail
            string cached = thumbnailService.GetThumbnail(tabId);
            if (!string.IsNullOrEmpty(cached))
            {
                OnTabThumbnailCaptured?.Invoke(tabId, cached);
                return;
            }

            // If this is the active tab, capture now
            if (ActiveTab?.Id == tabId)
            {
                thumbnailService.CaptureThumbnail(tabId);
            }
        }

        /// <summary>
        /// Request thumbnails for all tabs.
        /// </summary>
        public void RequestAllThumbnails()
        {
            foreach (var tab in tabs)
            {
                RequestThumbnail(tab.Id);
            }
        }

        /// <summary>
        /// Get the cached thumbnail for a tab.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        /// <returns>Base64 data URL or null if not cached.</returns>
        public string GetThumbnail(string tabId)
        {
            return thumbnailService?.GetThumbnail(tabId);
        }

        /// <summary>
        /// Handle thumbnail captured event from the service.
        /// </summary>
        private void HandleThumbnailCaptured(string tabId, string dataUrl)
        {
            OnTabThumbnailCaptured?.Invoke(tabId, dataUrl);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Close all tabs and cleanup.
        /// </summary>
        public void CloseAllTabs()
        {
            if (currentWorld != null)
            {
                unloadWorldCallback?.Invoke(currentWorld);
                currentWorld = null;
            }

            var allTabs = tabs.ToList();
            tabs.Clear();
            ActiveTab = null;

            foreach (var tab in allTabs)
            {
                OnTabClosed?.Invoke(tab);
            }

            // Clear all thumbnails
            thumbnailService?.ClearAllThumbnails();

            LogSystem.Log("[TabManager] All tabs closed.");
        }

        private void OnDestroy()
        {
            // Unsubscribe from thumbnail events
            if (thumbnailService != null)
            {
                thumbnailService.OnThumbnailCaptured -= HandleThumbnailCaptured;
            }

            CloseAllTabs();
        }

        #endregion
    }
}
