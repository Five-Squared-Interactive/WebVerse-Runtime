// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using NUnit.Framework;
using FiveSQD.WebVerse.Handlers.Voice.Audio;

/// <summary>
/// Unit tests for JitterBuffer.
/// </summary>
public class JitterBufferTests
{
    private JitterBuffer _buffer;

    [SetUp]
    public void SetUp()
    {
        _buffer = new JitterBuffer();
    }

    [TearDown]
    public void TearDown()
    {
        if (_buffer != null)
        {
            _buffer.Dispose();
            _buffer = null;
        }
    }

    #region Story 4.1: Jitter Buffer Tests

    [Test]
    public void JitterBuffer_DefaultSettings_HasCorrectValues()
    {
        Assert.AreEqual(3, _buffer.TargetFrames);
        Assert.AreEqual(10, _buffer.MaxFrames);
        Assert.AreEqual(0, _buffer.Count);
        Assert.IsFalse(_buffer.IsReady);
    }

    [Test]
    public void JitterBuffer_CustomSettings_HasCorrectValues()
    {
        using (var customBuffer = new JitterBuffer(5, 15))
        {
            Assert.AreEqual(5, customBuffer.TargetFrames);
            Assert.AreEqual(15, customBuffer.MaxFrames);
        }
    }

    [Test]
    public void JitterBuffer_InvalidTargetFrames_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new JitterBuffer(0, 10);
        });
    }

    [Test]
    public void JitterBuffer_MaxLessThanTarget_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new JitterBuffer(5, 3);
        });
    }

    [Test]
    public void Enqueue_ValidPacket_Succeeds()
    {
        byte[] data = new byte[] { 1, 2, 3 };

        bool result = _buffer.Enqueue(data, 1);

        Assert.IsTrue(result);
        Assert.AreEqual(1, _buffer.Count);
    }

    [Test]
    public void Enqueue_NullData_ReturnsFalse()
    {
        bool result = _buffer.Enqueue(null, 1);

        Assert.IsFalse(result);
        Assert.AreEqual(0, _buffer.Count);
    }

    [Test]
    public void Enqueue_EmptyData_ReturnsFalse()
    {
        bool result = _buffer.Enqueue(new byte[0], 1);

        Assert.IsFalse(result);
        Assert.AreEqual(0, _buffer.Count);
    }

    [Test]
    public void Enqueue_MultiplePackets_OrderedBySequence()
    {
        _buffer.Enqueue(new byte[] { 3 }, 3);
        _buffer.Enqueue(new byte[] { 1 }, 1);
        _buffer.Enqueue(new byte[] { 2 }, 2);

        Assert.AreEqual(3, _buffer.Count);

        _buffer.Dequeue(out byte[] data1, out uint seq1);
        _buffer.Dequeue(out byte[] data2, out uint seq2);
        _buffer.Dequeue(out byte[] data3, out uint seq3);

        Assert.AreEqual(1u, seq1);
        Assert.AreEqual(2u, seq2);
        Assert.AreEqual(3u, seq3);
    }

    [Test]
    public void Enqueue_OutOfOrder_TracksReordering()
    {
        _buffer.Enqueue(new byte[] { 3 }, 3);
        _buffer.Enqueue(new byte[] { 1 }, 1); // Out of order

        Assert.AreEqual(1, _buffer.PacketsReordered);
    }

    [Test]
    public void Enqueue_OldPacket_DropsIt()
    {
        _buffer.Enqueue(new byte[] { 1 }, 1);
        _buffer.Dequeue(out _, out _); // Play packet 1

        bool result = _buffer.Enqueue(new byte[] { 1 }, 1); // Try to add packet 1 again

        Assert.IsFalse(result);
        Assert.AreEqual(1, _buffer.PacketsDropped);
    }

    [Test]
    public void Enqueue_ExceedsMax_DropsOldest()
    {
        using (var smallBuffer = new JitterBuffer(2, 3))
        {
            smallBuffer.Enqueue(new byte[] { 1 }, 1);
            smallBuffer.Enqueue(new byte[] { 2 }, 2);
            smallBuffer.Enqueue(new byte[] { 3 }, 3);
            smallBuffer.Enqueue(new byte[] { 4 }, 4); // Should drop packet 1

            Assert.AreEqual(3, smallBuffer.Count);

            smallBuffer.Dequeue(out _, out uint firstSeq);
            Assert.AreEqual(2u, firstSeq); // Packet 1 was dropped
        }
    }

    [Test]
    public void Dequeue_EmptyBuffer_ReturnsFalse()
    {
        bool result = _buffer.Dequeue(out byte[] data, out uint seq);

        Assert.IsFalse(result);
        Assert.IsNull(data);
        Assert.AreEqual(0u, seq);
    }

    [Test]
    public void Dequeue_HasPacket_ReturnsCorrectData()
    {
        byte[] originalData = new byte[] { 10, 20, 30 };
        _buffer.Enqueue(originalData, 5);

        bool result = _buffer.Dequeue(out byte[] data, out uint seq);

        Assert.IsTrue(result);
        Assert.AreEqual(originalData, data);
        Assert.AreEqual(5u, seq);
        Assert.AreEqual(5u, _buffer.LastPlayedSequence);
    }

    [Test]
    public void IsReady_BelowTarget_ReturnsFalse()
    {
        _buffer.Enqueue(new byte[] { 1 }, 1);
        _buffer.Enqueue(new byte[] { 2 }, 2);

        Assert.IsFalse(_buffer.IsReady); // Need 3 frames
    }

    [Test]
    public void IsReady_AtTarget_ReturnsTrue()
    {
        _buffer.Enqueue(new byte[] { 1 }, 1);
        _buffer.Enqueue(new byte[] { 2 }, 2);
        _buffer.Enqueue(new byte[] { 3 }, 3);

        Assert.IsTrue(_buffer.IsReady);
    }

    [Test]
    public void PeekSequence_HasPacket_ReturnsSequence()
    {
        _buffer.Enqueue(new byte[] { 1 }, 5);

        bool result = _buffer.PeekSequence(out uint seq);

        Assert.IsTrue(result);
        Assert.AreEqual(5u, seq);
        Assert.AreEqual(1, _buffer.Count); // Not removed
    }

    [Test]
    public void PeekSequence_Empty_ReturnsFalse()
    {
        bool result = _buffer.PeekSequence(out uint seq);

        Assert.IsFalse(result);
    }

    [Test]
    public void GetGapCount_NoGap_ReturnsZero()
    {
        _buffer.Enqueue(new byte[] { 1 }, 1);
        _buffer.Dequeue(out _, out _);
        _buffer.Enqueue(new byte[] { 2 }, 2);

        int gap = _buffer.GetGapCount();

        Assert.AreEqual(0, gap);
    }

    [Test]
    public void GetGapCount_HasGap_ReturnsCount()
    {
        _buffer.Enqueue(new byte[] { 1 }, 1);
        _buffer.Dequeue(out _, out _); // Played seq 1
        _buffer.Enqueue(new byte[] { 5 }, 5); // Missing 2, 3, 4

        int gap = _buffer.GetGapCount();

        Assert.AreEqual(3, gap);
    }

    [Test]
    public void Clear_RemovesAllPackets()
    {
        _buffer.Enqueue(new byte[] { 1 }, 1);
        _buffer.Enqueue(new byte[] { 2 }, 2);
        _buffer.Enqueue(new byte[] { 3 }, 3);

        _buffer.Clear();

        Assert.AreEqual(0, _buffer.Count);
        Assert.AreEqual(0u, _buffer.LastPlayedSequence);
    }

    [Test]
    public void ResetStatistics_ClearsCounters()
    {
        _buffer.Enqueue(new byte[] { 1 }, 1);
        _buffer.Enqueue(new byte[] { 3 }, 3);
        _buffer.Dequeue(out _, out _);
        _buffer.Enqueue(new byte[] { 1 }, 1); // Dropped

        Assert.Greater(_buffer.TotalPacketsReceived, 0);
        Assert.Greater(_buffer.PacketsDropped, 0);

        _buffer.ResetStatistics();

        Assert.AreEqual(0, _buffer.TotalPacketsReceived);
        Assert.AreEqual(0, _buffer.PacketsDropped);
        Assert.AreEqual(0, _buffer.PacketsReordered);
    }

    [Test]
    public void Dispose_PreventsEnqueue()
    {
        _buffer.Dispose();

        bool result = _buffer.Enqueue(new byte[] { 1 }, 1);

        Assert.IsFalse(result);
    }

    [Test]
    public void Dispose_PreventsDequeue()
    {
        _buffer.Enqueue(new byte[] { 1 }, 1);
        _buffer.Dispose();

        bool result = _buffer.Dequeue(out _, out _);

        Assert.IsFalse(result);
    }

    #endregion
}
