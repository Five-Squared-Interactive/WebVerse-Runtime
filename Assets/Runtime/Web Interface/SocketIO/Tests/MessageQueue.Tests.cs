// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WebInterface.SocketIO;

namespace FiveSQD.WebVerse.WebInterface.SocketIO.Tests
{
    /// <summary>
    /// Unit tests for MessageQueue circular buffer.
    /// Pure unit tests -- no Unity dependencies.
    /// </summary>
    [TestFixture]
    public class MessageQueueTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        // --- Constructor ---

        [Test]
        public void Constructor_WithCapacity100_CreatesEmptyQueue()
        {
            var queue = new MessageQueue(100);

            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(100, queue.Capacity);
        }

        [Test]
        public void Constructor_WithCapacity50_SetsCapacity()
        {
            var queue = new MessageQueue(50);

            Assert.AreEqual(50, queue.Capacity);
        }

        [Test]
        public void Constructor_WithCapacity0_CreatesUnboundedQueue()
        {
            var queue = new MessageQueue(0);

            Assert.AreEqual(0, queue.Capacity);
            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(queue.IsFull);
        }

        // --- Enqueue ---

        [Test]
        public void Enqueue_SingleMessage_CountIsOne()
        {
            var queue = new MessageQueue(10);

            queue.Enqueue("chat", "hello");

            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void Enqueue_And_DequeueAll_ReturnsCorrectMessage()
        {
            var queue = new MessageQueue(10);

            queue.Enqueue("chat", "{\"msg\":\"hello\"}");

            var messages = queue.DequeueAll();
            Assert.AreEqual(1, messages.Length);
            Assert.AreEqual("chat", messages[0].EventName);
            Assert.AreEqual("{\"msg\":\"hello\"}", messages[0].Data);
        }

        [Test]
        public void Enqueue_ThreeMessages_DequeueAll_ReturnsFIFO()
        {
            var queue = new MessageQueue(10);

            queue.Enqueue("e1", "d1");
            queue.Enqueue("e2", "d2");
            queue.Enqueue("e3", "d3");

            var messages = queue.DequeueAll();
            Assert.AreEqual(3, messages.Length);
            Assert.AreEqual("e1", messages[0].EventName);
            Assert.AreEqual("d1", messages[0].Data);
            Assert.AreEqual("e2", messages[1].EventName);
            Assert.AreEqual("d2", messages[1].Data);
            Assert.AreEqual("e3", messages[2].EventName);
            Assert.AreEqual("d3", messages[2].Data);
        }

        // --- DequeueAll ---

        [Test]
        public void DequeueAll_OnEmptyQueue_ReturnsEmptyArray()
        {
            var queue = new MessageQueue(10);

            var messages = queue.DequeueAll();

            Assert.AreEqual(0, messages.Length);
        }

        [Test]
        public void DequeueAll_ResetsCountToZero()
        {
            var queue = new MessageQueue(10);
            queue.Enqueue("e1", "d1");
            queue.Enqueue("e2", "d2");
            Assert.AreEqual(2, queue.Count);

            queue.DequeueAll();

            Assert.AreEqual(0, queue.Count);
        }

        // --- Overflow (Drop Oldest) ---

        [Test]
        public void Enqueue_AtCapacity_DropsOldest()
        {
            var queue = new MessageQueue(3);

            queue.Enqueue("e1", "d1");
            queue.Enqueue("e2", "d2");
            queue.Enqueue("e3", "d3");
            queue.Enqueue("e4", "d4"); // drops e1

            var messages = queue.DequeueAll();
            Assert.AreEqual(3, messages.Length);
            Assert.AreEqual("e2", messages[0].EventName);
            Assert.AreEqual("e3", messages[1].EventName);
            Assert.AreEqual("e4", messages[2].EventName);
        }

        [Test]
        public void CircularBuffer_Wraparound_MaintainsFIFO()
        {
            var queue = new MessageQueue(3);

            // Fill to capacity
            queue.Enqueue("e1", "d1");
            queue.Enqueue("e2", "d2");
            queue.Enqueue("e3", "d3");

            // Overflow twice -- drops e1, e2
            queue.Enqueue("e4", "d4");
            queue.Enqueue("e5", "d5");

            var messages = queue.DequeueAll();
            Assert.AreEqual(3, messages.Length);
            Assert.AreEqual("e3", messages[0].EventName);
            Assert.AreEqual("e4", messages[1].EventName);
            Assert.AreEqual("e5", messages[2].EventName);
        }

        // --- Clear ---

        [Test]
        public void Clear_ResetsCountToZero()
        {
            var queue = new MessageQueue(10);
            queue.Enqueue("e1", "d1");
            queue.Enqueue("e2", "d2");
            Assert.AreEqual(2, queue.Count);

            queue.Clear();

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void Clear_ThenDequeueAll_ReturnsEmpty()
        {
            var queue = new MessageQueue(10);
            queue.Enqueue("e1", "d1");
            queue.Clear();

            var messages = queue.DequeueAll();

            Assert.AreEqual(0, messages.Length);
        }

        // --- IsFull ---

        [Test]
        public void IsFull_WhenAtCapacity_ReturnsTrue()
        {
            var queue = new MessageQueue(2);
            queue.Enqueue("e1", "d1");
            queue.Enqueue("e2", "d2");

            Assert.IsTrue(queue.IsFull);
        }

        [Test]
        public void IsFull_WhenNotAtCapacity_ReturnsFalse()
        {
            var queue = new MessageQueue(10);
            queue.Enqueue("e1", "d1");

            Assert.IsFalse(queue.IsFull);
        }

        [Test]
        public void IsFull_Unbounded_AlwaysFalse()
        {
            var queue = new MessageQueue(0);
            for (int i = 0; i < 200; i++)
            {
                queue.Enqueue("e" + i, "d" + i);
            }

            Assert.IsFalse(queue.IsFull);
        }

        // --- Unbounded Mode ---

        [Test]
        public void Unbounded_EnqueueBeyond100_NoDrop()
        {
            var queue = new MessageQueue(0);

            for (int i = 0; i < 150; i++)
            {
                queue.Enqueue("e" + i, "d" + i);
            }

            Assert.AreEqual(150, queue.Count);

            var messages = queue.DequeueAll();
            Assert.AreEqual(150, messages.Length);
            Assert.AreEqual("e0", messages[0].EventName);
            Assert.AreEqual("e149", messages[149].EventName);
        }

        [Test]
        public void Unbounded_DequeueAll_ClearsQueue()
        {
            var queue = new MessageQueue(0);
            queue.Enqueue("e1", "d1");
            queue.Enqueue("e2", "d2");

            queue.DequeueAll();

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void Unbounded_Clear_ResetsCount()
        {
            var queue = new MessageQueue(0);
            queue.Enqueue("e1", "d1");
            queue.Clear();

            Assert.AreEqual(0, queue.Count);
        }

        // --- Multiple Cycles ---

        [Test]
        public void MultipleCycles_EnqueueDequeueEnqueue_MaintainsFIFO()
        {
            var queue = new MessageQueue(5);

            // First cycle
            queue.Enqueue("a1", "d1");
            queue.Enqueue("a2", "d2");
            queue.DequeueAll();

            // Second cycle
            queue.Enqueue("b1", "d1");
            queue.Enqueue("b2", "d2");
            queue.Enqueue("b3", "d3");

            var messages = queue.DequeueAll();
            Assert.AreEqual(3, messages.Length);
            Assert.AreEqual("b1", messages[0].EventName);
            Assert.AreEqual("b2", messages[1].EventName);
            Assert.AreEqual("b3", messages[2].EventName);
        }
    }
}
