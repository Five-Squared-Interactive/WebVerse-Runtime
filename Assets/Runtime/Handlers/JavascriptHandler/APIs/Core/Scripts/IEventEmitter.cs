// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Core
{
    /// <summary>
    /// Global observer limit tracking for the event system.
    /// Enforces a hard cap on total active observers to prevent performance degradation.
    /// </summary>
    public static class ObserverLimits
    {
        /// <summary>
        /// Maximum total active observers across all emitters. Hard cap with developer warning.
        /// </summary>
        public const int MaxObservers = 1000;

        /// <summary>
        /// Current total active observer count across all IEventEmitter instances.
        /// </summary>
        public static int CurrentCount { get; internal set; } = 0;

        /// <summary>
        /// Check if a new observer can be registered.
        /// </summary>
        public static bool CanRegister => CurrentCount < MaxObservers;

        /// <summary>
        /// Reset the counter (for testing or world unload).
        /// </summary>
        internal static void Reset() { CurrentCount = 0; }
    }

    /// <summary>
    /// Interface for event-emitting objects in the World API.
    /// Uses C# default interface methods so any class can gain event capability
    /// by implementing this interface and providing a Listeners dictionary
    /// and OnceListeners set.
    /// </summary>
    public interface IEventEmitter
    {
        /// <summary>
        /// Storage for event listeners. Each implementor must provide this property.
        /// Keys are event names, values are lists of JsValue callback references.
        /// </summary>
        Dictionary<string, List<Jint.Native.JsValue>> Listeners { get; }

        /// <summary>
        /// Set of callbacks registered via Once() that should auto-remove after firing.
        /// Each implementor must provide this property.
        /// </summary>
        HashSet<Jint.Native.JsValue> OnceListeners { get; }

        /// <summary>
        /// Set of event names currently being emitted, for re-entrancy protection.
        /// Prevents infinite recursion when a listener emits the same event.
        /// Each implementor must provide this property.
        /// </summary>
        HashSet<string> EmittingEvents { get; }

        /// <summary>
        /// Whether this emitter has been disposed (e.g., entity destroyed).
        /// Implementors should return true after DisposeAllListeners is called
        /// during destruction to prevent new listener registration.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Register an event listener. Returns an unsubscribe function.
        /// Logs a warning for unrecognized event names but still registers the listener.
        /// </summary>
        /// <param name="eventName">The event name to listen for.</param>
        /// <param name="callback">The JsValue function reference to invoke when the event fires.</param>
        /// <returns>An unsubscribe function that removes this specific listener when called.</returns>
        Func<bool> On(string eventName, Jint.Native.JsValue callback)
        {
            if (IsDisposed)
            {
                Logging.LogError("[EventSystem] Cannot register listener on disposed emitter.");
                return () => false;
            }

            if (string.IsNullOrEmpty(eventName))
            {
                Logging.LogError("[EventSystem] Event name cannot be null or empty.");
                return () => false;
            }

            if (callback == null || callback == Jint.Native.JsValue.Undefined || callback == Jint.Native.JsValue.Null)
            {
                Logging.LogError($"[EventSystem] Callback for '{eventName}' is null or undefined.");
                return () => false;
            }

            if (!ObserverLimits.CanRegister)
            {
                Logging.LogWarning(
                    $"[EventSystem] Observer limit reached ({ObserverLimits.MaxObservers}). " +
                    $"New listener for '{eventName}' rejected.");
                return () => false;
            }

            if (!Events.IsValid(eventName))
            {
                Logging.LogWarning(
                    $"[EventSystem] Unrecognized event name: '{eventName}'. " +
                    "Check Events constants for valid names.");
            }

            if (!Listeners.ContainsKey(eventName))
                Listeners[eventName] = new List<Jint.Native.JsValue>();

            Listeners[eventName].Add(callback);
            ObserverLimits.CurrentCount++;

            // Return unsubscribe function
            bool unsubscribed = false;
            return () =>
            {
                if (unsubscribed) return false;
                unsubscribed = true;
                Off(eventName, callback);
                return true;
            };
        }

        /// <summary>
        /// Register a one-time event listener that auto-removes after first invocation.
        /// </summary>
        /// <param name="eventName">The event name to listen for.</param>
        /// <param name="callback">The JsValue function reference to invoke once.</param>
        /// <returns>An unsubscribe function that removes this listener before it fires.</returns>
        Func<bool> Once(string eventName, Jint.Native.JsValue callback)
        {
            // Register normally via On()
            var unsub = On(eventName, callback);

            // Only track in OnceListeners if On() actually registered the callback
            if (!string.IsNullOrEmpty(eventName)
                && Listeners.ContainsKey(eventName)
                && Listeners[eventName].Contains(callback))
            {
                OnceListeners.Add(callback);
            }

            return unsub;
        }

        /// <summary>
        /// Remove a specific listener for an event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="callback">The specific callback to remove.</param>
        void Off(string eventName, Jint.Native.JsValue callback)
        {
            if (string.IsNullOrEmpty(eventName) || !Listeners.ContainsKey(eventName))
                return;

            bool removed = Listeners[eventName].Remove(callback);
            if (removed) ObserverLimits.CurrentCount--;

            // Only remove from OnceListeners if we actually removed from this event's list
            if (removed)
                OnceListeners.Remove(callback);

            if (Listeners[eventName].Count == 0)
                Listeners.Remove(eventName);
        }

        /// <summary>
        /// Remove all listeners for an event.
        /// </summary>
        /// <param name="eventName">The event name to clear all listeners for.</param>
        void Off(string eventName)
        {
            if (string.IsNullOrEmpty(eventName) || !Listeners.ContainsKey(eventName))
                return;

            // Remove any once-tracked callbacks for this event
            int count = Listeners[eventName].Count;
            foreach (var cb in Listeners[eventName])
                OnceListeners.Remove(cb);

            Listeners.Remove(eventName);
            ObserverLimits.CurrentCount -= count;
        }

        /// <summary>
        /// Emit an event, invoking all registered listeners in registration order.
        /// Uses catch-log-continue: one listener's exception never blocks others.
        /// Once() listeners are auto-removed after firing.
        /// </summary>
        /// <param name="eventName">The event name to emit.</param>
        /// <param name="args">Arguments to pass to each listener callback.</param>
        void Emit(string eventName, params Jint.Native.JsValue[] args)
        {
            if (string.IsNullOrEmpty(eventName) || !Listeners.ContainsKey(eventName))
                return;

            // Re-entrancy guard — prevent infinite recursion if a listener emits the same event
            if (!EmittingEvents.Add(eventName))
            {
                Logging.LogWarning(
                    $"[EventSystem] Re-entrant Emit detected for '{eventName}'. Skipping to prevent stack overflow.");
                return;
            }

            try
            {
                // ToList() copy for safe iteration — listeners may be removed during callbacks
                var toRemove = new List<Jint.Native.JsValue>();

                foreach (var callback in Listeners[eventName].ToList())
                {
                    try
                    {
                        Runtime.WebVerseRuntime.Instance.javascriptHandler.Engine.Call(callback, Jint.Native.JsValue.Undefined, args);
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError(
                            $"[EventSystem] Listener error for '{eventName}': {ex.Message}");
                        // Continue to next listener — never break the chain
                    }

                    // If this was a Once() listener, mark for removal
                    if (OnceListeners.Contains(callback))
                    {
                        toRemove.Add(callback);
                    }
                }

                // Remove Once() listeners after all callbacks have fired
                foreach (var cb in toRemove)
                {
                    // Remove from OnceListeners directly in case Listeners key was removed mid-emit
                    OnceListeners.Remove(cb);
                    if (Listeners.ContainsKey(eventName))
                    {
                        if (Listeners[eventName].Remove(cb))
                            ObserverLimits.CurrentCount--;
                        if (Listeners[eventName].Count == 0)
                            Listeners.Remove(eventName);
                    }
                }
            }
            finally
            {
                EmittingEvents.Remove(eventName);
            }
        }

        /// <summary>
        /// Remove all listeners across all events. Called during entity destruction.
        /// </summary>
        void DisposeAllListeners()
        {
            // Decrement global observer count for all listeners being removed
            foreach (var kvp in Listeners)
                ObserverLimits.CurrentCount -= kvp.Value.Count;

            Listeners.Clear();
            OnceListeners.Clear();
            EmittingEvents.Clear();
        }
    }
}
