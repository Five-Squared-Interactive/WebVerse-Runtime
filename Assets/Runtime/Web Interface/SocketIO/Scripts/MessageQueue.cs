// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;

namespace FiveSQD.WebVerse.WebInterface.SocketIO
{
    /// <summary>
    /// A queued outbound message with event name and JSON data.
    /// Struct to avoid GC pressure in Unity.
    /// </summary>
    public struct QueuedMessage
    {
        public string EventName;
        public string Data;

        public QueuedMessage(string eventName, string data)
        {
            EventName = eventName;
            Data = data;
        }
    }

    /// <summary>
    /// Array-backed circular buffer for outbound message queuing.
    /// O(1) enqueue/dequeue. Drops oldest on overflow.
    /// Supports unbounded mode when capacity is 0.
    /// </summary>
    public class MessageQueue
    {
        private QueuedMessage[] buffer;
        private List<QueuedMessage> unboundedBuffer;
        private int head;
        private int tail;
        private int count;
        private readonly int capacity;
        private readonly bool unbounded;

        /// <summary>
        /// Number of messages currently in the queue.
        /// </summary>
        public int Count
        {
            get { return unbounded ? unboundedBuffer.Count : count; }
        }

        /// <summary>
        /// Maximum capacity of the queue. 0 means unbounded.
        /// </summary>
        public int Capacity
        {
            get { return capacity; }
        }

        /// <summary>
        /// Whether the bounded queue is at capacity. Always false for unbounded.
        /// </summary>
        public bool IsFull
        {
            get { return !unbounded && count == capacity; }
        }

        /// <summary>
        /// Create a new MessageQueue.
        /// </summary>
        /// <param name="capacity">Maximum capacity. 0 for unbounded.</param>
        public MessageQueue(int capacity)
        {
            this.capacity = capacity;
            this.unbounded = capacity == 0;

            if (unbounded)
            {
                unboundedBuffer = new List<QueuedMessage>();
            }
            else
            {
                buffer = new QueuedMessage[capacity];
                head = 0;
                tail = 0;
                count = 0;
            }
        }

        /// <summary>
        /// Enqueue a message. If bounded and full, drops the oldest message.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">The JSON data.</param>
        public void Enqueue(string eventName, string data)
        {
            if (unbounded)
            {
                unboundedBuffer.Add(new QueuedMessage(eventName, data));
                return;
            }

            buffer[tail] = new QueuedMessage(eventName, data);
            tail = (tail + 1) % capacity;

            if (count == capacity)
            {
                // Drop oldest by advancing head
                head = (head + 1) % capacity;
            }
            else
            {
                count++;
            }
        }

        /// <summary>
        /// Dequeue all messages in FIFO order and reset the queue.
        /// </summary>
        /// <returns>Array of queued messages in FIFO order.</returns>
        public QueuedMessage[] DequeueAll()
        {
            if (unbounded)
            {
                var result = unboundedBuffer.ToArray();
                unboundedBuffer.Clear();
                return result;
            }

            var messages = new QueuedMessage[count];
            for (int i = 0; i < messages.Length; i++)
            {
                messages[i] = buffer[(head + i) % capacity];
            }

            head = 0;
            tail = 0;
            count = 0;
            return messages;
        }

        /// <summary>
        /// Clear all queued messages.
        /// </summary>
        public void Clear()
        {
            if (unbounded)
            {
                unboundedBuffer.Clear();
                return;
            }

            head = 0;
            tail = 0;
            count = 0;
        }
    }
}
