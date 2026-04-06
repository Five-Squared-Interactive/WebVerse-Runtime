// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.Profiling;

namespace FiveSQD.WebVerse.Utilities
{
    /// <summary>
    /// Utility class for memory debugging and leak detection.
    /// Use this to track memory usage during world load/unload cycles.
    /// </summary>
    public static class MemoryDebug
    {
        /// <summary>
        /// Log a snapshot of current memory usage.
        /// </summary>
        /// <param name="label">Label to identify this snapshot.</param>
        public static void LogMemorySnapshot(string label)
        {
            var totalReserved = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
            var totalAllocated = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            var monoHeap = Profiler.GetMonoHeapSizeLong() / (1024f * 1024f);
            var monoUsed = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
            var gcTotal = System.GC.GetTotalMemory(false) / (1024f * 1024f);

            Logging.Log($"[MEMORY {label}] Reserved: {totalReserved:F1}MB | Allocated: {totalAllocated:F1}MB | Mono Heap: {monoHeap:F1}MB | Mono Used: {monoUsed:F1}MB | GC Total: {gcTotal:F1}MB");
        }

        /// <summary>
        /// Get current memory statistics as a struct.
        /// </summary>
        /// <returns>MemoryStats struct with current values.</returns>
        public static MemoryStats GetMemoryStats()
        {
            return new MemoryStats
            {
                TotalReservedMB = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f),
                TotalAllocatedMB = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f),
                MonoHeapMB = Profiler.GetMonoHeapSizeLong() / (1024f * 1024f),
                MonoUsedMB = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f),
                GCTotalMB = System.GC.GetTotalMemory(false) / (1024f * 1024f)
            };
        }

        /// <summary>
        /// Force a full garbage collection and resource cleanup.
        /// </summary>
        public static void ForceCleanup()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            Logging.Log("[MemoryDebug] Forced cleanup completed.");
        }

        /// <summary>
        /// Compare two memory snapshots and log the difference.
        /// </summary>
        /// <param name="before">Memory stats before operation.</param>
        /// <param name="after">Memory stats after operation.</param>
        /// <param name="label">Label for this comparison.</param>
        public static void LogMemoryDelta(MemoryStats before, MemoryStats after, string label)
        {
            var reservedDelta = after.TotalReservedMB - before.TotalReservedMB;
            var allocatedDelta = after.TotalAllocatedMB - before.TotalAllocatedMB;
            var monoHeapDelta = after.MonoHeapMB - before.MonoHeapMB;
            var monoUsedDelta = after.MonoUsedMB - before.MonoUsedMB;
            var gcDelta = after.GCTotalMB - before.GCTotalMB;

            Logging.Log($"[MEMORY DELTA {label}] Reserved: {reservedDelta:+0.0;-0.0}MB | Allocated: {allocatedDelta:+0.0;-0.0}MB | Mono Heap: {monoHeapDelta:+0.0;-0.0}MB | Mono Used: {monoUsedDelta:+0.0;-0.0}MB | GC Total: {gcDelta:+0.0;-0.0}MB");
        }
    }

    /// <summary>
    /// Struct to hold memory statistics for comparison.
    /// </summary>
    public struct MemoryStats
    {
        public float TotalReservedMB;
        public float TotalAllocatedMB;
        public float MonoHeapMB;
        public float MonoUsedMB;
        public float GCTotalMB;
    }
}
