// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.Voice.Audio
{
    /// <summary>
    /// Jitter buffer for smoothing audio packet timing.
    /// Buffers incoming packets and releases them at consistent intervals.
    /// </summary>
    public class JitterBuffer : IDisposable
    {
        /// <summary>
        /// Default target buffer size in frames.
        /// </summary>
        public const int DefaultTargetFrames = 3;

        /// <summary>
        /// Maximum buffer size in frames.
        /// </summary>
        public const int DefaultMaxFrames = 10;

        /// <summary>
        /// Target number of frames to buffer before playback.
        /// </summary>
        public int TargetFrames { get; }

        /// <summary>
        /// Maximum number of frames to buffer.
        /// </summary>
        public int MaxFrames { get; }

        /// <summary>
        /// Current number of frames in buffer.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _buffer.Count;
                }
            }
        }

        /// <summary>
        /// Whether the buffer is ready for playback (has enough frames).
        /// </summary>
        public bool IsReady
        {
            get
            {
                lock (_lock)
                {
                    return _buffer.Count >= TargetFrames;
                }
            }
        }

        /// <summary>
        /// Last sequence number that was dequeued.
        /// </summary>
        public uint LastPlayedSequence { get; private set; }

        /// <summary>
        /// Statistics: total packets received.
        /// </summary>
        public int TotalPacketsReceived { get; private set; }

        /// <summary>
        /// Statistics: packets dropped due to being too late.
        /// </summary>
        public int PacketsDropped { get; private set; }

        /// <summary>
        /// Statistics: packets reordered.
        /// </summary>
        public int PacketsReordered { get; private set; }

        private readonly SortedList<uint, byte[]> _buffer;
        private readonly object _lock = new object();
        private bool _disposed;

        /// <summary>
        /// Create a new JitterBuffer with default settings.
        /// </summary>
        public JitterBuffer() : this(DefaultTargetFrames, DefaultMaxFrames)
        {
        }

        /// <summary>
        /// Create a new JitterBuffer with custom settings.
        /// </summary>
        /// <param name="targetFrames">Target frames to buffer before playback.</param>
        /// <param name="maxFrames">Maximum frames to buffer.</param>
        public JitterBuffer(int targetFrames, int maxFrames)
        {
            if (targetFrames <= 0)
            {
                throw new ArgumentException("targetFrames must be positive", nameof(targetFrames));
            }
            if (maxFrames < targetFrames)
            {
                throw new ArgumentException("maxFrames must be >= targetFrames", nameof(maxFrames));
            }

            TargetFrames = targetFrames;
            MaxFrames = maxFrames;
            _buffer = new SortedList<uint, byte[]>();
        }

        /// <summary>
        /// Add a packet to the buffer.
        /// </summary>
        /// <param name="data">Opus-encoded audio data.</param>
        /// <param name="sequenceNumber">Sequence number for ordering.</param>
        /// <returns>True if packet was added, false if dropped.</returns>
        public bool Enqueue(byte[] data, uint sequenceNumber)
        {
            if (_disposed)
            {
                return false;
            }

            if (data == null || data.Length == 0)
            {
                return false;
            }

            lock (_lock)
            {
                TotalPacketsReceived++;

                // Drop packets that are too old
                if (LastPlayedSequence > 0 && sequenceNumber <= LastPlayedSequence)
                {
                    PacketsDropped++;
                    return false;
                }

                // Drop if buffer is full and this packet is not the next expected
                if (_buffer.Count >= MaxFrames)
                {
                    // Make room by dropping oldest
                    if (_buffer.Count > 0)
                    {
                        _buffer.RemoveAt(0);
                    }
                }

                // Check for reordering
                if (_buffer.Count > 0)
                {
                    var lastKey = _buffer.Keys[_buffer.Count - 1];
                    if (sequenceNumber < lastKey)
                    {
                        PacketsReordered++;
                    }
                }

                // Add or replace
                if (!_buffer.ContainsKey(sequenceNumber))
                {
                    _buffer.Add(sequenceNumber, data);
                }

                return true;
            }
        }

        /// <summary>
        /// Get the next packet from the buffer.
        /// </summary>
        /// <param name="data">Output audio data.</param>
        /// <param name="sequenceNumber">Output sequence number.</param>
        /// <returns>True if a packet was available, false if buffer is empty.</returns>
        public bool Dequeue(out byte[] data, out uint sequenceNumber)
        {
            data = null;
            sequenceNumber = 0;

            if (_disposed)
            {
                return false;
            }

            lock (_lock)
            {
                if (_buffer.Count == 0)
                {
                    return false;
                }

                // Get and remove the oldest packet (lowest sequence number)
                sequenceNumber = _buffer.Keys[0];
                data = _buffer.Values[0];
                _buffer.RemoveAt(0);

                LastPlayedSequence = sequenceNumber;
                return true;
            }
        }

        /// <summary>
        /// Peek at the next sequence number without removing.
        /// </summary>
        /// <param name="sequenceNumber">Output sequence number.</param>
        /// <returns>True if a packet is available.</returns>
        public bool PeekSequence(out uint sequenceNumber)
        {
            sequenceNumber = 0;

            lock (_lock)
            {
                if (_buffer.Count == 0)
                {
                    return false;
                }

                sequenceNumber = _buffer.Keys[0];
                return true;
            }
        }

        /// <summary>
        /// Check for gaps in the sequence and return the number of missing packets.
        /// </summary>
        /// <returns>Number of missing packets between last played and next in buffer.</returns>
        public int GetGapCount()
        {
            lock (_lock)
            {
                if (_buffer.Count == 0 || LastPlayedSequence == 0)
                {
                    return 0;
                }

                uint nextSeq = _buffer.Keys[0];
                if (nextSeq <= LastPlayedSequence + 1)
                {
                    return 0;
                }

                return (int)(nextSeq - LastPlayedSequence - 1);
            }
        }

        /// <summary>
        /// Clear all buffered packets.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _buffer.Clear();
                LastPlayedSequence = 0;
            }
        }

        /// <summary>
        /// Reset statistics.
        /// </summary>
        public void ResetStatistics()
        {
            TotalPacketsReceived = 0;
            PacketsDropped = 0;
            PacketsReordered = 0;
        }

        /// <summary>
        /// Dispose the jitter buffer.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            Clear();
            _disposed = true;
        }
    }
}
