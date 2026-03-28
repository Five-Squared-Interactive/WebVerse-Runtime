// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using FiveSQD.StraightFour.Utilities;
using UnityEngine;

namespace FiveSQD.StraightFour.WorldState
{
    /// <summary>
    /// Manager for world state snapshots.
    /// Handles capturing, storing, and restoring world states for tab switching.
    /// </summary>
    public class WorldStateManager : BaseManager
    {
        /// <summary>
        /// Maximum number of snapshots to keep in memory per world.
        /// </summary>
        private int maxSnapshotsPerWorld = 1;

        /// <summary>
        /// Maximum total memory for snapshots in bytes.
        /// </summary>
        private long maxSnapshotMemory = 100 * 1024 * 1024; // 100MB default

        /// <summary>
        /// Current estimated memory usage for snapshots.
        /// </summary>
        private long currentMemoryUsage = 0;

        /// <summary>
        /// Dictionary of world snapshots indexed by world name.
        /// </summary>
        private Dictionary<string, List<WorldStateSnapshot>> worldSnapshots;

        /// <summary>
        /// Cached JSON strings for quick restore (avoids re-serialization).
        /// </summary>
        private Dictionary<string, string> snapshotCache;

        /// <summary>
        /// Event fired when a snapshot is captured.
        /// </summary>
        public event Action<string, WorldStateSnapshot> OnSnapshotCaptured;

        /// <summary>
        /// Event fired when a snapshot is restored.
        /// </summary>
        public event Action<string, WorldStateSnapshot> OnSnapshotRestored;

        /// <summary>
        /// Initialize the world state manager.
        /// </summary>
        /// <param name="maxSnapshotsPerWorld">Maximum snapshots per world.</param>
        /// <param name="maxMemoryMB">Maximum memory in MB for all snapshots.</param>
        public void Initialize(int maxSnapshotsPerWorld = 1, long maxMemoryMB = 100)
        {
            base.Initialize();
            this.maxSnapshotsPerWorld = maxSnapshotsPerWorld;
            this.maxSnapshotMemory = maxMemoryMB * 1024 * 1024;
            worldSnapshots = new Dictionary<string, List<WorldStateSnapshot>>();
            snapshotCache = new Dictionary<string, string>();
            currentMemoryUsage = 0;
            LogSystem.Log("[WorldStateManager] Initialized.");
        }

        /// <summary>
        /// Terminate the world state manager.
        /// </summary>
        public override void Terminate()
        {
            ClearAllSnapshots();
            worldSnapshots = null;
            snapshotCache = null;
            base.Terminate();
            LogSystem.Log("[WorldStateManager] Terminated.");
        }

        /// <summary>
        /// Capture and store a snapshot of the given world.
        /// </summary>
        /// <param name="world">World to capture.</param>
        /// <param name="worldName">Name/identifier for the world.</param>
        /// <param name="basePath">Base path for world resources.</param>
        /// <returns>The captured snapshot, or null on failure.</returns>
        public WorldStateSnapshot CaptureSnapshot(World.World world, string worldName, string basePath)
        {
            if (world == null)
            {
                LogSystem.LogError("[WorldStateManager->CaptureSnapshot] World is null.");
                return null;
            }

            // Capture the snapshot
            var snapshot = WorldStateSerializer.CaptureWorldState(world, worldName, basePath);
            if (snapshot == null)
            {
                LogSystem.LogError("[WorldStateManager->CaptureSnapshot] Failed to capture world state.");
                return null;
            }

            // Serialize and cache
            string json = WorldStateSerializer.SerializeToJson(snapshot);
            if (string.IsNullOrEmpty(json))
            {
                LogSystem.LogError("[WorldStateManager->CaptureSnapshot] Failed to serialize snapshot.");
                return null;
            }

            // Estimate memory and check limits
            long snapshotSize = json.Length * 2; // UTF-16
            if (currentMemoryUsage + snapshotSize > maxSnapshotMemory)
            {
                // Try to free memory by removing oldest snapshots
                if (!FreeMemory(snapshotSize))
                {
                    LogSystem.LogWarning("[WorldStateManager->CaptureSnapshot] Not enough memory for snapshot.");
                    return null;
                }
            }

            // Store the snapshot
            StoreSnapshot(worldName, snapshot, json, snapshotSize);

            OnSnapshotCaptured?.Invoke(worldName, snapshot);
            LogSystem.Log($"[WorldStateManager->CaptureSnapshot] Captured snapshot for '{worldName}' ({snapshotSize / 1024}KB).");
            return snapshot;
        }

        /// <summary>
        /// Get the latest snapshot for a world.
        /// </summary>
        /// <param name="worldName">Name of the world.</param>
        /// <returns>Latest snapshot or null if none exists.</returns>
        public WorldStateSnapshot GetLatestSnapshot(string worldName)
        {
            if (worldSnapshots == null || !worldSnapshots.ContainsKey(worldName))
            {
                return null;
            }

            var snapshots = worldSnapshots[worldName];
            if (snapshots == null || snapshots.Count == 0)
            {
                return null;
            }

            return snapshots[snapshots.Count - 1];
        }

        /// <summary>
        /// Get the cached JSON for a world's latest snapshot.
        /// </summary>
        /// <param name="worldName">Name of the world.</param>
        /// <returns>JSON string or null.</returns>
        public string GetLatestSnapshotJson(string worldName)
        {
            if (snapshotCache == null || !snapshotCache.ContainsKey(worldName))
            {
                return null;
            }
            return snapshotCache[worldName];
        }

        /// <summary>
        /// Check if a snapshot exists for a world.
        /// </summary>
        /// <param name="worldName">Name of the world.</param>
        /// <returns>True if snapshot exists.</returns>
        public bool HasSnapshot(string worldName)
        {
            return worldSnapshots != null &&
                   worldSnapshots.ContainsKey(worldName) &&
                   worldSnapshots[worldName].Count > 0;
        }

        /// <summary>
        /// Remove all snapshots for a world.
        /// </summary>
        /// <param name="worldName">Name of the world.</param>
        public void RemoveSnapshots(string worldName)
        {
            if (worldSnapshots == null) return;

            if (worldSnapshots.ContainsKey(worldName))
            {
                // Estimate freed memory
                if (snapshotCache.ContainsKey(worldName))
                {
                    currentMemoryUsage -= snapshotCache[worldName].Length * 2;
                    snapshotCache.Remove(worldName);
                }
                worldSnapshots.Remove(worldName);
                LogSystem.Log($"[WorldStateManager->RemoveSnapshots] Removed snapshots for '{worldName}'.");
            }
        }

        /// <summary>
        /// Clear all stored snapshots.
        /// </summary>
        public void ClearAllSnapshots()
        {
            if (worldSnapshots != null)
            {
                worldSnapshots.Clear();
            }
            if (snapshotCache != null)
            {
                snapshotCache.Clear();
            }
            currentMemoryUsage = 0;
            LogSystem.Log("[WorldStateManager->ClearAllSnapshots] Cleared all snapshots.");
        }

        /// <summary>
        /// Get list of worlds with stored snapshots.
        /// </summary>
        /// <returns>List of world names.</returns>
        public List<string> GetStoredWorldNames()
        {
            if (worldSnapshots == null)
            {
                return new List<string>();
            }
            return new List<string>(worldSnapshots.Keys);
        }

        /// <summary>
        /// Get current memory usage for snapshots.
        /// </summary>
        /// <returns>Memory usage in bytes.</returns>
        public long GetMemoryUsage()
        {
            return currentMemoryUsage;
        }

        /// <summary>
        /// Get snapshot count for a world.
        /// </summary>
        /// <param name="worldName">Name of the world.</param>
        /// <returns>Number of snapshots.</returns>
        public int GetSnapshotCount(string worldName)
        {
            if (worldSnapshots == null || !worldSnapshots.ContainsKey(worldName))
            {
                return 0;
            }
            return worldSnapshots[worldName].Count;
        }

        /// <summary>
        /// Store an existing snapshot in the manager.
        /// Use this when you have already captured a snapshot via WorldStateSerializer.
        /// </summary>
        /// <param name="worldName">Name of the world.</param>
        /// <param name="snapshot">The snapshot to store.</param>
        /// <returns>True if stored successfully.</returns>
        public bool AddSnapshot(string worldName, WorldStateSnapshot snapshot)
        {
            if (worldSnapshots == null || snapshot == null)
            {
                return false;
            }

            string json = WorldStateSerializer.SerializeToJson(snapshot);
            if (string.IsNullOrEmpty(json))
            {
                LogSystem.LogError("[WorldStateManager->AddSnapshot] Failed to serialize snapshot.");
                return false;
            }

            long snapshotSize = json.Length * 2; // UTF-16
            if (currentMemoryUsage + snapshotSize > maxSnapshotMemory)
            {
                if (!FreeMemory(snapshotSize))
                {
                    LogSystem.LogWarning("[WorldStateManager->AddSnapshot] Not enough memory for snapshot.");
                    return false;
                }
            }

            StoreSnapshot(worldName, snapshot, json, snapshotSize);
            OnSnapshotCaptured?.Invoke(worldName, snapshot);
            LogSystem.Log($"[WorldStateManager->AddSnapshot] Added snapshot for '{worldName}'.");
            return true;
        }

        /// <summary>
        /// Store a snapshot in the manager.
        /// </summary>
        private void StoreSnapshot(string worldName, WorldStateSnapshot snapshot, string json, long size)
        {
            if (!worldSnapshots.ContainsKey(worldName))
            {
                worldSnapshots[worldName] = new List<WorldStateSnapshot>();
            }

            var snapshots = worldSnapshots[worldName];

            // Remove oldest if at limit
            while (snapshots.Count >= maxSnapshotsPerWorld)
            {
                snapshots.RemoveAt(0);
            }

            snapshots.Add(snapshot);
            snapshotCache[worldName] = json;
            currentMemoryUsage += size;
        }

        /// <summary>
        /// Try to free memory by removing old snapshots.
        /// </summary>
        /// <param name="requiredBytes">Bytes needed.</param>
        /// <returns>True if enough memory was freed.</returns>
        private bool FreeMemory(long requiredBytes)
        {
            // Find oldest snapshots across all worlds and remove them
            while (currentMemoryUsage + requiredBytes > maxSnapshotMemory && worldSnapshots.Count > 0)
            {
                // Find the world with the oldest snapshot
                string oldestWorld = null;
                long oldestTimestamp = long.MaxValue;

                foreach (var kvp in worldSnapshots)
                {
                    if (kvp.Value.Count > 0 && kvp.Value[0].timestamp < oldestTimestamp)
                    {
                        oldestTimestamp = kvp.Value[0].timestamp;
                        oldestWorld = kvp.Key;
                    }
                }

                if (oldestWorld != null)
                {
                    RemoveSnapshots(oldestWorld);
                }
                else
                {
                    break;
                }
            }

            return currentMemoryUsage + requiredBytes <= maxSnapshotMemory;
        }
    }
}
