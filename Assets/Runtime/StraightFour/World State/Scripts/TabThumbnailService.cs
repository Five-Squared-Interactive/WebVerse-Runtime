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
    /// Service for capturing, encoding, and caching tab thumbnail previews.
    /// Thumbnails are captured from the Unity camera and encoded as JPEG data URLs.
    /// </summary>
    public class TabThumbnailService : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Thumbnail width in pixels.
        /// </summary>
        public const int ThumbnailWidth = 256;

        /// <summary>
        /// Thumbnail height in pixels (16:9 aspect ratio).
        /// </summary>
        public const int ThumbnailHeight = 144;

        /// <summary>
        /// JPEG quality (0-100). 75% gives ~15-30KB per thumbnail.
        /// </summary>
        public const int JpegQuality = 75;

        /// <summary>
        /// Memory budget for thumbnails in bytes (10MB).
        /// </summary>
        public const long MemoryBudgetBytes = 10 * 1024 * 1024;

        /// <summary>
        /// Estimated average size per thumbnail in bytes (~20KB).
        /// </summary>
        private const int EstimatedThumbnailSize = 20 * 1024;

        #endregion

        #region Events

        /// <summary>
        /// Called before a screenshot is taken to hide UI overlays.
        /// </summary>
        public event Action OnBeforeCapture;

        /// <summary>
        /// Called after a screenshot is taken to restore UI overlays.
        /// </summary>
        public event Action OnAfterCapture;

        /// <summary>
        /// Fired when a thumbnail is captured or updated.
        /// Parameters: tabId, base64 data URL.
        /// </summary>
        public event Action<string, string> OnThumbnailCaptured;

        #endregion

        #region Private Fields

        // Cache: tabId -> (dataUrl, sizeBytes, lastAccessTime)
        private Dictionary<string, ThumbnailCacheEntry> cache = new Dictionary<string, ThumbnailCacheEntry>();

        // Reusable RenderTexture for captures
        private RenderTexture captureTexture;

        // Reusable Texture2D for reading pixels
        private Texture2D readTexture;

        // Current total size of cached thumbnails
        private long currentCacheSize = 0;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            CreateCaptureResources();
        }

        private void OnDestroy()
        {
            ReleaseCaptureResources();
            ClearAllThumbnails();
        }

        /// <summary>
        /// Create reusable capture resources.
        /// </summary>
        private void CreateCaptureResources()
        {
            captureTexture = new RenderTexture(ThumbnailWidth, ThumbnailHeight, 24, RenderTextureFormat.ARGB32);
            captureTexture.antiAliasing = 2;
            captureTexture.Create();

            readTexture = new Texture2D(ThumbnailWidth, ThumbnailHeight, TextureFormat.RGB24, false);
        }

        /// <summary>
        /// Release capture resources.
        /// </summary>
        private void ReleaseCaptureResources()
        {
            if (captureTexture != null)
            {
                captureTexture.Release();
                Destroy(captureTexture);
                captureTexture = null;
            }

            if (readTexture != null)
            {
                Destroy(readTexture);
                readTexture = null;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Capture a thumbnail for the specified tab using the given camera.
        /// </summary>
        /// <param name="tabId">The tab ID to associate with this thumbnail.</param>
        /// <param name="camera">The camera to capture from. If null, uses Camera.main.</param>
        /// <returns>Base64 data URL of the captured thumbnail, or null if capture failed.</returns>
        public string CaptureThumbnail(string tabId, UnityEngine.Camera camera = null)
        {
            if (string.IsNullOrEmpty(tabId))
            {
                LogSystem.LogWarning("[TabThumbnailService] Cannot capture thumbnail: tabId is null or empty.");
                return null;
            }

            // Use screen capture (end-of-frame) to include UI overlays like WebViews
            StartCoroutine(CaptureScreenCoroutine(tabId));
            return null; // Async — result delivered via OnThumbnailCaptured event
        }

        /// <summary>
        /// Get a cached thumbnail for the specified tab.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        /// <returns>Base64 data URL of the thumbnail, or null if not cached.</returns>
        public string GetThumbnail(string tabId)
        {
            if (string.IsNullOrEmpty(tabId))
                return null;

            if (cache.TryGetValue(tabId, out ThumbnailCacheEntry entry))
            {
                // Update access time for LRU
                entry.LastAccessTime = DateTime.UtcNow;
                return entry.DataUrl;
            }

            return null;
        }

        /// <summary>
        /// Check if a thumbnail exists for the specified tab.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        /// <returns>True if a thumbnail is cached.</returns>
        public bool HasThumbnail(string tabId)
        {
            return !string.IsNullOrEmpty(tabId) && cache.ContainsKey(tabId);
        }

        /// <summary>
        /// Remove a thumbnail from the cache.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        public void RemoveThumbnail(string tabId)
        {
            if (string.IsNullOrEmpty(tabId))
                return;

            if (cache.TryGetValue(tabId, out ThumbnailCacheEntry entry))
            {
                currentCacheSize -= entry.SizeBytes;
                cache.Remove(tabId);
                LogSystem.Log($"[TabThumbnailService] Removed thumbnail for tab {tabId}.");
            }
        }

        /// <summary>
        /// Clear all cached thumbnails.
        /// </summary>
        public void ClearAllThumbnails()
        {
            cache.Clear();
            currentCacheSize = 0;
            LogSystem.Log("[TabThumbnailService] Cleared all thumbnails.");
        }

        /// <summary>
        /// Get the current cache size in bytes.
        /// </summary>
        public long GetCacheSize()
        {
            return currentCacheSize;
        }

        /// <summary>
        /// Get the number of cached thumbnails.
        /// </summary>
        public int GetCacheCount()
        {
            return cache.Count;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Capture the full screen (including UI overlays) at end of frame,
        /// scale down to thumbnail size, and fire the captured event.
        /// </summary>
        private IEnumerator CaptureScreenCoroutine(string tabId)
        {
            // Hide chrome UI before capture
            OnBeforeCapture?.Invoke();

            // Wait a frame so the UI is actually hidden in the render
            yield return new WaitForEndOfFrame();
            yield return null;
            yield return new WaitForEndOfFrame();

            try
            {
                // Capture full screen (without chrome overlay)
                Texture2D screenTex = ScreenCapture.CaptureScreenshotAsTexture();

                // Scale down to thumbnail size
                RenderTexture scaledRT = RenderTexture.GetTemporary(ThumbnailWidth, ThumbnailHeight, 0);
                Graphics.Blit(screenTex, scaledRT);

                RenderTexture previousActive = RenderTexture.active;
                RenderTexture.active = scaledRT;
                readTexture.ReadPixels(new Rect(0, 0, ThumbnailWidth, ThumbnailHeight), 0, 0);
                readTexture.Apply();
                RenderTexture.active = previousActive;

                RenderTexture.ReleaseTemporary(scaledRT);
                Destroy(screenTex);

                // Encode to JPEG
                byte[] jpegData = readTexture.EncodeToJPG(JpegQuality);
                string base64 = Convert.ToBase64String(jpegData);
                string dataUrl = $"data:image/jpeg;base64,{base64}";

                // Store and fire event
                StoreThumbnail(tabId, dataUrl);
                OnThumbnailCaptured?.Invoke(tabId, dataUrl);

                LogSystem.Log($"[TabThumbnailService] Captured screen thumbnail for tab {tabId}.");
            }
            catch (Exception ex)
            {
                LogSystem.LogError($"[TabThumbnailService] Error capturing screen thumbnail: {ex.Message}");
            }
            finally
            {
                // Restore chrome UI
                OnAfterCapture?.Invoke();
            }
        }

        /// <summary>
        /// Store a thumbnail in the cache, evicting old entries if needed.
        /// </summary>
        private void StoreThumbnail(string tabId, string dataUrl)
        {
            // Remove existing entry for this tab
            RemoveThumbnail(tabId);

            // Calculate size (approximate from base64 length)
            int sizeBytes = dataUrl.Length; // Close enough for cache management

            // Evict old entries if we're over budget
            while (currentCacheSize + sizeBytes > MemoryBudgetBytes && cache.Count > 0)
            {
                EvictLeastRecentlyUsed();
            }

            // Add new entry
            cache[tabId] = new ThumbnailCacheEntry
            {
                DataUrl = dataUrl,
                SizeBytes = sizeBytes,
                LastAccessTime = DateTime.UtcNow
            };
            currentCacheSize += sizeBytes;
        }

        /// <summary>
        /// Evict the least recently used thumbnail from the cache.
        /// </summary>
        private void EvictLeastRecentlyUsed()
        {
            if (cache.Count == 0)
                return;

            // Find the entry with the oldest access time
            string oldestKey = null;
            DateTime oldestTime = DateTime.MaxValue;

            foreach (var kvp in cache)
            {
                if (kvp.Value.LastAccessTime < oldestTime)
                {
                    oldestTime = kvp.Value.LastAccessTime;
                    oldestKey = kvp.Key;
                }
            }

            if (oldestKey != null)
            {
                currentCacheSize -= cache[oldestKey].SizeBytes;
                cache.Remove(oldestKey);
                LogSystem.Log($"[TabThumbnailService] Evicted LRU thumbnail for tab {oldestKey}.");
            }
        }

        #endregion

        #region Cache Entry

        /// <summary>
        /// Cache entry for a thumbnail.
        /// </summary>
        private class ThumbnailCacheEntry
        {
            public string DataUrl;
            public int SizeBytes;
            public DateTime LastAccessTime;
        }

        #endregion
    }
}
