// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.WebInterface.SocketIO;

[assembly: InternalsVisibleTo("FiveSQD.WebVerse.WebInterface.SocketIO.Tests")]

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Networking
{
#if USE_WEBINTERFACE
    /// <summary>
    /// JavaScript API wrapper for Socket.IO. Exposes Socket.IO functionality
    /// to world scripts running in the Jint engine.
    /// </summary>
    public class SocketIO
    {
        /// <summary>
        /// Reference to the internal SocketIOClient.
        /// </summary>
        private SocketIOClient client;

        /// <summary>
        /// GameObject hosting the SocketIOClient component.
        /// </summary>
        private GameObject clientObject;

        /// <summary>
        /// Stored JS function names keyed by event name.
        /// </summary>
        private Dictionary<string, List<string>> eventCallbacks
            = new Dictionary<string, List<string>>();

        /// <summary>
        /// Stored JS function names for binary events, keyed by event name.
        /// </summary>
        private Dictionary<string, List<string>> binaryEventCallbacks
            = new Dictionary<string, List<string>>();

        /// <summary>
        /// Stored JS function names for one-time event callbacks, keyed by event name.
        /// </summary>
        private Dictionary<string, List<string>> onceCallbacks
            = new Dictionary<string, List<string>>();

        /// <summary>
        /// Stored JS function names for catch-all event callbacks.
        /// </summary>
        private List<string> onAnyCallbacks = new List<string>();

        /// <summary>
        /// Cached namespace wrapper instances, keyed by namespace path.
        /// </summary>
        private Dictionary<string, NamespacedSocketIO> namespaceWrappers
            = new Dictionary<string, NamespacedSocketIO>();

        /// <summary>
        /// Whether the OnAny handler has been registered with the client.
        /// </summary>
        private bool onAnyRegistered;

        /// <summary>
        /// Whether this instance has been terminated.
        /// </summary>
        private bool terminated;

        /// <summary>
        /// Internal accessor for the SocketIOClient (used by tests).
        /// </summary>
        internal SocketIOClient Client
        {
            get { return client; }
        }

        /// <summary>
        /// Internal accessor for the number of registered event names (used by tests).
        /// </summary>
        internal int RegisteredEventCount
        {
            get { return eventCallbacks.Count; }
        }

        /// <summary>
        /// Internal check if a specific event has registered callbacks (used by tests).
        /// </summary>
        internal bool HasCallbacksForEvent(string eventName)
        {
            return eventCallbacks.ContainsKey(eventName) && eventCallbacks[eventName].Count > 0;
        }

        /// <summary>
        /// Internal accessor for the number of registered binary event names (used by tests).
        /// </summary>
        internal int RegisteredBinaryEventCount
        {
            get { return binaryEventCallbacks.Count; }
        }

        /// <summary>
        /// Internal check if a specific event has registered binary callbacks (used by tests).
        /// </summary>
        internal bool HasBinaryCallbacksForEvent(string eventName)
        {
            return binaryEventCallbacks.ContainsKey(eventName) && binaryEventCallbacks[eventName].Count > 0;
        }

        /// <summary>
        /// Internal accessor for the number of registered once event names (used by tests).
        /// </summary>
        internal int RegisteredOnceEventCount
        {
            get { return onceCallbacks.Count; }
        }

        /// <summary>
        /// Internal check if a specific event has registered once callbacks (used by tests).
        /// </summary>
        internal bool HasOnceCallbacksForEvent(string eventName)
        {
            return onceCallbacks.ContainsKey(eventName) && onceCallbacks[eventName].Count > 0;
        }

        /// <summary>
        /// Internal accessor for the number of registered OnAny callbacks (used by tests).
        /// </summary>
        internal int RegisteredOnAnyCount
        {
            get { return onAnyCallbacks.Count; }
        }

        /// <summary>
        /// Whether the client is connected.
        /// </summary>
        public bool Connected
        {
            get { return client != null && client.Connected; }
        }

        /// <summary>
        /// The server-assigned socket ID.
        /// </summary>
        public string Id
        {
            get { return client?.Id; }
        }

        /// <summary>
        /// Constructor for a SocketIO wrapper.
        /// Creates a new SocketIOClient on a managed GameObject.
        /// </summary>
        public SocketIO()
        {
            terminated = false;

            try
            {
                clientObject = new GameObject("SocketIOClient");
                client = clientObject.AddComponent<SocketIOClient>();
                client.Initialize();
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO] Failed to create SocketIOClient: " + ex.Message);
                client = null;
            }
        }

        /// <summary>
        /// Connect to a Socket.IO server.
        /// </summary>
        /// <param name="url">The server URL.</param>
        /// <param name="options">Optional connection options object.</param>
        public void Connect(string url, object options = null)
        {
            if (client == null)
            {
                Logging.LogWarning("[SocketIO->Connect] Client not initialized.");
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO->Connect] Instance terminated.");
                return;
            }

            try
            {
                var socketOptions = ParseOptions(options);
                client.Connect(url, socketOptions);
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO->Connect] " + ex.Message);
            }
        }

        /// <summary>
        /// Disconnect from the Socket.IO server.
        /// </summary>
        public void Disconnect()
        {
            if (client == null)
            {
                Logging.LogWarning("[SocketIO->Disconnect] Client not initialized.");
                return;
            }

            try
            {
                client.Disconnect();
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO->Disconnect] " + ex.Message);
            }
        }

        /// <summary>
        /// Emit a named event with JSON data to the server.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">JSON data string.</param>
        public void Emit(string eventName, string data)
        {
            if (client == null)
            {
                Logging.LogWarning("[SocketIO->Emit] Client not initialized.");
                return;
            }

            try
            {
                client.Emit(eventName, data);
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO->Emit] " + ex.Message);
            }
        }

        /// <summary>
        /// Register a JS callback function for a named event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="functionName">The JS function name to invoke when the event fires.</param>
        public void On(string eventName, string functionName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO->On] Instance terminated.");
                return;
            }

            bool isFirstListenerForEvent = !eventCallbacks.ContainsKey(eventName);

            if (isFirstListenerForEvent)
            {
                eventCallbacks[eventName] = new List<string>();
            }

            eventCallbacks[eventName].Add(functionName);

            // Register a C# handler on the client only once per event name.
            // The handler iterates all stored JS function names when fired.
            if (isFirstListenerForEvent && client != null)
            {
                client.On(eventName, (data) =>
                {
                    DispatchEventToJS(eventName, data);
                });
            }
        }

        /// <summary>
        /// Remove all JS callbacks for a named event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        public void Off(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            eventCallbacks.Remove(eventName);

            if (client != null)
            {
                client.Off(eventName);
            }
        }

        /// <summary>
        /// Emit a binary event. The data parameter is a base64-encoded string
        /// which is decoded to byte[] before sending via the client.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="base64Data">Base64-encoded binary data.</param>
        public void EmitBinary(string eventName, string base64Data)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (client == null)
            {
                Logging.LogWarning("[SocketIO->EmitBinary] Client not initialized.");
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO->EmitBinary] Instance terminated.");
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(base64Data))
                {
                    client.EmitBinary(eventName, null);
                    return;
                }

                byte[] data = Convert.FromBase64String(base64Data);
                client.EmitBinary(eventName, data);
            }
            catch (FormatException)
            {
                Logging.LogWarning("[SocketIO->EmitBinary] Invalid base64 data, message dropped.");
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO->EmitBinary] " + ex.Message);
            }
        }

        /// <summary>
        /// Emit a named event and wait for server acknowledgement.
        /// The ack response is dispatched to the named JS function via DataAPIHelper.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">JSON data string.</param>
        /// <param name="functionName">The JS function name to invoke with the ack response.</param>
        public void EmitWithAck(string eventName, string data, string functionName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (client == null)
            {
                Logging.LogWarning("[SocketIO->EmitWithAck] Client not initialized.");
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO->EmitWithAck] Instance terminated.");
                return;
            }

            try
            {
                Action<string> ackCallback = (response) =>
                {
                    try
                    {
                        Data.DataAPIHelper.QueueJavascript(functionName, new object[] { response });
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[SocketIO] Ack callback error: " + ex.Message);
                    }
                };

                client.EmitWithAck(eventName, data, ackCallback);
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO->EmitWithAck] " + ex.Message);
            }
        }

        /// <summary>
        /// Emit a volatile event. When connected, delegates to client.EmitVolatile.
        /// When not connected, silently drops the message.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">JSON data string.</param>
        public void EmitVolatile(string eventName, string data)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (client == null)
            {
                Logging.LogWarning("[SocketIO->EmitVolatile] Client not initialized.");
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO->EmitVolatile] Instance terminated.");
                return;
            }

            try
            {
                client.EmitVolatile(eventName, data);
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO->EmitVolatile] " + ex.Message);
            }
        }

        /// <summary>
        /// Register a one-time JS callback function for a named event.
        /// The callback is automatically removed after the first invocation.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="functionName">The JS function name to invoke once.</param>
        public void Once(string eventName, string functionName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO->Once] Instance terminated.");
                return;
            }

            if (!onceCallbacks.ContainsKey(eventName))
            {
                onceCallbacks[eventName] = new List<string>();
            }

            onceCallbacks[eventName].Add(functionName);

            if (client != null)
            {
                string fn = functionName;
                client.Once(eventName, (eventData) =>
                {
                    try
                    {
                        Data.DataAPIHelper.QueueJavascript(fn, new object[] { eventData });
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[SocketIO] Once callback error: " + ex.Message);
                    }
                });
            }
        }

        /// <summary>
        /// Register a catch-all JS callback function invoked for every event.
        /// </summary>
        /// <param name="functionName">The JS function name to invoke with (eventName, data).</param>
        public void OnAny(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO->OnAny] Instance terminated.");
                return;
            }

            onAnyCallbacks.Add(functionName);

            // Register the client-level OnAny handler only once.
            // The handler iterates all stored JS function names when fired.
            if (!onAnyRegistered && client != null)
            {
                client.OnAny((eventName, eventData) =>
                {
                    if (terminated) return;
                    for (int i = 0; i < onAnyCallbacks.Count; i++)
                    {
                        string fn = onAnyCallbacks[i];
                        try
                        {
                            Data.DataAPIHelper.QueueJavascript(fn, new object[] { eventName, eventData });
                        }
                        catch (Exception ex)
                        {
                            Logging.LogError("[SocketIO] OnAny callback error: " + ex.Message);
                        }
                    }
                });
                onAnyRegistered = true;
            }
        }

        /// <summary>
        /// Remove all catch-all JS callbacks registered via OnAny.
        /// </summary>
        public void OffAny()
        {
            onAnyCallbacks.Clear();

            if (client != null)
            {
                client.OffAny();
            }

            onAnyRegistered = false;
        }

        /// <summary>
        /// Request the server to join a named room.
        /// </summary>
        /// <param name="room">The room name.</param>
        public void JoinRoom(string room)
        {
            if (terminated)
            {
                Logging.LogWarning("[SocketIO] JoinRoom called after terminate.");
                return;
            }

            if (string.IsNullOrEmpty(room))
            {
                return;
            }

            if (client == null)
            {
                Logging.LogWarning("[SocketIO] Client not initialized.");
                return;
            }

            client.JoinRoom(room);
        }

        /// <summary>
        /// Request the server to leave a named room.
        /// </summary>
        /// <param name="room">The room name.</param>
        public void LeaveRoom(string room)
        {
            if (terminated)
            {
                Logging.LogWarning("[SocketIO] LeaveRoom called after terminate.");
                return;
            }

            if (string.IsNullOrEmpty(room))
            {
                return;
            }

            if (client == null)
            {
                Logging.LogWarning("[SocketIO] Client not initialized.");
                return;
            }

            client.LeaveRoom(room);
        }

        /// <summary>
        /// Create or retrieve a namespace-scoped socket wrapper.
        /// Returns an object with the full Socket.IO API surface (Emit, On, Off, etc.).
        /// </summary>
        /// <param name="nsp">The namespace path (e.g., "/chat").</param>
        /// <returns>A NamespacedSocketIO wrapper, or null if not connected.</returns>
        public object Of(string nsp)
        {
            if (terminated)
            {
                Logging.LogWarning("[SocketIO] Of called after terminate.");
                return null;
            }

            if (string.IsNullOrEmpty(nsp))
            {
                return null;
            }

            if (client == null)
            {
                Logging.LogWarning("[SocketIO] Client not initialized.");
                return null;
            }

            if (namespaceWrappers.ContainsKey(nsp))
            {
                return namespaceWrappers[nsp];
            }

            var ns = client.Of(nsp);
            if (ns == null)
            {
                return null;
            }

            var wrapper = new NamespacedSocketIO(ns);
            namespaceWrappers[nsp] = wrapper;
            return wrapper;
        }

        /// <summary>
        /// Register a JS callback function for a named binary event.
        /// Binary data is base64-encoded before being passed to the JS function.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="functionName">The JS function name to invoke.</param>
        public void OnBinary(string eventName, string functionName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO->OnBinary] Instance terminated.");
                return;
            }

            bool isFirstListenerForEvent = !binaryEventCallbacks.ContainsKey(eventName);

            if (isFirstListenerForEvent)
            {
                binaryEventCallbacks[eventName] = new List<string>();
            }

            binaryEventCallbacks[eventName].Add(functionName);

            if (isFirstListenerForEvent && client != null)
            {
                client.OnBinary(eventName, (name, data) =>
                {
                    DispatchBinaryEventToJS(name, data);
                });
            }
        }

        /// <summary>
        /// Remove all JS binary callbacks for a named event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        public void OffBinary(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            binaryEventCallbacks.Remove(eventName);

            if (client != null)
            {
                client.OffBinary(eventName);
            }
        }

        /// <summary>
        /// Clean up all stored callbacks and event listeners.
        /// Called when the world/tab is unloaded.
        /// </summary>
        public void Terminate()
        {
            terminated = true;

            if (client != null)
            {
                try
                {
                    client.Terminate();
                }
                catch (Exception ex)
                {
                    Logging.LogError("[SocketIO->Terminate] " + ex.Message);
                }
            }

            if (namespaceWrappers != null)
            {
                foreach (var kvp in namespaceWrappers)
                {
                    kvp.Value?.Dispose();
                }
                namespaceWrappers.Clear();
            }

            eventCallbacks.Clear();
            binaryEventCallbacks.Clear();
            onceCallbacks.Clear();
            onAnyCallbacks.Clear();
            onAnyRegistered = false;
            client = null;

            if (clientObject != null)
            {
                UnityEngine.Object.Destroy(clientObject);
                clientObject = null;
            }
        }

        /// <summary>
        /// Dispatch a transport event to all registered JS function names for that event.
        /// Uses DataAPIHelper.QueueJavascript for main-thread safety.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">The event data.</param>
        private void DispatchEventToJS(string eventName, string data)
        {
            if (terminated || !eventCallbacks.ContainsKey(eventName))
            {
                return;
            }

            var functions = eventCallbacks[eventName];
            for (int i = 0; i < functions.Count; i++)
            {
                string fn = functions[i];
                try
                {
                    Data.DataAPIHelper.QueueJavascript(fn, new object[] { data });
                }
                catch (Exception ex)
                {
                    Logging.LogError("[SocketIO] Callback error for event '"
                        + eventName + "': " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Dispatch a binary transport event to all registered JS function names.
        /// Converts byte[] to base64 string before passing to JavaScript.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="data">The binary data.</param>
        private void DispatchBinaryEventToJS(string eventName, byte[] data)
        {
            if (terminated || !binaryEventCallbacks.ContainsKey(eventName))
            {
                return;
            }

            string base64 = data != null ? Convert.ToBase64String(data) : "";

            var functions = binaryEventCallbacks[eventName];
            for (int i = 0; i < functions.Count; i++)
            {
                string fn = functions[i];
                try
                {
                    Data.DataAPIHelper.QueueJavascript(fn, new object[] { base64 });
                }
                catch (Exception ex)
                {
                    Logging.LogError("[SocketIO] Binary callback error for event '"
                        + eventName + "': " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Parse a JS options object into SocketIOOptions.
        /// </summary>
        /// <param name="options">The options object from JS.</param>
        /// <returns>Parsed SocketIOOptions.</returns>
        private SocketIOOptions ParseOptions(object options)
        {
            var socketOptions = new SocketIOOptions();

            if (options == null)
            {
                return socketOptions;
            }

            // If the options object is a Jint ObjectInstance, extract properties.
            // Each property is parsed independently so one bad value doesn't
            // prevent the rest from being applied.
            if (options is Jint.Native.Object.ObjectInstance obj)
            {
                ParseBoolProperty(obj, "reconnection", ref socketOptions.reconnection);
                ParseIntProperty(obj, "reconnectionAttempts", ref socketOptions.reconnectionAttempts);
                ParseIntProperty(obj, "reconnectionDelay", ref socketOptions.reconnectionDelay);
                ParseIntProperty(obj, "reconnectionDelayMax", ref socketOptions.reconnectionDelayMax);
                ParseIntProperty(obj, "timeout", ref socketOptions.timeout);
                ParseIntProperty(obj, "ackTimeout", ref socketOptions.ackTimeout);
                ParseIntProperty(obj, "queueSize", ref socketOptions.queueSize);
                ParseStringProperty(obj, "transport", ref socketOptions.transport);
                ParseStringProperty(obj, "auth", ref socketOptions.auth);

                // Parse nested dictionary properties
                var parsedHeaders = ParseDictionaryProperty(obj, "headers");
                if (parsedHeaders.Count > 0)
                {
                    socketOptions.headers = parsedHeaders;
                }

                var parsedQuery = ParseDictionaryProperty(obj, "query");
                if (parsedQuery.Count > 0)
                {
                    socketOptions.query = parsedQuery;
                }
            }

            return socketOptions;
        }

        /// <summary>
        /// Safely parse a boolean property from a Jint ObjectInstance.
        /// </summary>
        private void ParseBoolProperty(
            Jint.Native.Object.ObjectInstance obj, string propertyName, ref bool target)
        {
            try
            {
                var val = obj.Get(propertyName);
                if (val != null && val != Jint.Native.JsValue.Undefined)
                {
                    target = (bool)val.ToObject();
                }
            }
            catch (Exception ex)
            {
                Logging.LogWarning("[SocketIO] Error parsing " + propertyName + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Safely parse an integer property from a Jint ObjectInstance.
        /// </summary>
        private void ParseIntProperty(
            Jint.Native.Object.ObjectInstance obj, string propertyName, ref int target)
        {
            try
            {
                var val = obj.Get(propertyName);
                if (val != null && val != Jint.Native.JsValue.Undefined)
                {
                    target = (int)(double)val.ToObject();
                }
            }
            catch (Exception ex)
            {
                Logging.LogWarning("[SocketIO] Error parsing " + propertyName + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Safely parse a string property from a Jint ObjectInstance.
        /// </summary>
        private void ParseStringProperty(
            Jint.Native.Object.ObjectInstance obj, string propertyName, ref string target)
        {
            try
            {
                var val = obj.Get(propertyName);
                if (val != null && val != Jint.Native.JsValue.Undefined)
                {
                    target = val.ToObject()?.ToString();
                }
            }
            catch (Exception ex)
            {
                Logging.LogWarning("[SocketIO] Error parsing " + propertyName + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Parse a nested Jint object property into a Dictionary.
        /// Uses ToObject() conversion since ObjectInstance does not have GetOwnPropertyKeys().
        /// </summary>
        /// <param name="obj">The parent ObjectInstance.</param>
        /// <param name="propertyName">The property name to extract.</param>
        /// <returns>Parsed dictionary, empty if property not found or parse error.</returns>
        private Dictionary<string, string> ParseDictionaryProperty(
            Jint.Native.Object.ObjectInstance obj, string propertyName)
        {
            var result = new Dictionary<string, string>();
            try
            {
                var prop = obj.Get(propertyName);
                if (prop != null && prop != Jint.Native.JsValue.Undefined
                    && prop is Jint.Native.Object.ObjectInstance nestedObj)
                {
                    var dict = nestedObj.ToObject() as Dictionary<string, object>;
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            result[kvp.Key] = kvp.Value?.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogWarning("[SocketIO] Error parsing " + propertyName + ": " + ex.Message);
            }
            return result;
        }
    }

    /// <summary>
    /// JavaScript API wrapper for a namespace-scoped Socket.IO socket.
    /// Wraps a NamespacedSocket with function-name-based JS dispatch.
    /// Created by SocketIO.Of() and returned to world scripts.
    /// </summary>
    internal class NamespacedSocketIO
    {
        private NamespacedSocket nsSocket;
        private bool terminated;

        private Dictionary<string, List<string>> eventCallbacks
            = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> binaryEventCallbacks
            = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> onceCallbacks
            = new Dictionary<string, List<string>>();
        private List<string> onAnyCallbacks = new List<string>();
        private bool onAnyRegistered;

        /// <summary>
        /// Whether the namespace socket is connected.
        /// </summary>
        public bool Connected
        {
            get { return nsSocket != null && nsSocket.Connected; }
        }

        /// <summary>
        /// The server-assigned socket ID for this namespace.
        /// </summary>
        public string Id
        {
            get { return nsSocket?.Id; }
        }

        /// <summary>
        /// Internal accessor for test verification.
        /// </summary>
        internal NamespacedSocket NsSocket
        {
            get { return nsSocket; }
        }

        /// <summary>
        /// Internal accessor for the number of registered event names (used by tests).
        /// </summary>
        internal int RegisteredEventCount
        {
            get { return eventCallbacks.Count; }
        }

        /// <summary>
        /// Internal check if a specific event has registered callbacks (used by tests).
        /// </summary>
        internal bool HasCallbacksForEvent(string eventName)
        {
            return eventCallbacks.ContainsKey(eventName) && eventCallbacks[eventName].Count > 0;
        }

        internal NamespacedSocketIO(NamespacedSocket ns)
        {
            nsSocket = ns;
            terminated = false;
        }

        public void Emit(string eventName, string data)
        {
            if (nsSocket == null)
            {
                Logging.LogWarning("[SocketIO:NS->Emit] Socket not initialized.");
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS->Emit] Instance terminated.");
                return;
            }

            try
            {
                nsSocket.Emit(eventName, data);
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO:NS->Emit] " + ex.Message);
            }
        }

        public void On(string eventName, string functionName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS->On] Instance terminated.");
                return;
            }

            bool isFirstListenerForEvent = !eventCallbacks.ContainsKey(eventName);

            if (isFirstListenerForEvent)
            {
                eventCallbacks[eventName] = new List<string>();
            }

            eventCallbacks[eventName].Add(functionName);

            if (isFirstListenerForEvent && nsSocket != null)
            {
                nsSocket.On(eventName, (data) =>
                {
                    DispatchEventToJS(eventName, data);
                });
            }
        }

        public void Off(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            eventCallbacks.Remove(eventName);

            if (nsSocket != null)
            {
                nsSocket.Off(eventName);
            }
        }

        public void Once(string eventName, string functionName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS->Once] Instance terminated.");
                return;
            }

            if (!onceCallbacks.ContainsKey(eventName))
            {
                onceCallbacks[eventName] = new List<string>();
            }

            onceCallbacks[eventName].Add(functionName);

            if (nsSocket != null)
            {
                string fn = functionName;
                nsSocket.Once(eventName, (eventData) =>
                {
                    try
                    {
                        Data.DataAPIHelper.QueueJavascript(fn, new object[] { eventData });
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[SocketIO:NS] Once callback error: " + ex.Message);
                    }
                });
            }
        }

        public void OnAny(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS->OnAny] Instance terminated.");
                return;
            }

            onAnyCallbacks.Add(functionName);

            if (!onAnyRegistered && nsSocket != null)
            {
                nsSocket.OnAny((eventName, eventData) =>
                {
                    if (terminated) return;
                    for (int i = 0; i < onAnyCallbacks.Count; i++)
                    {
                        string fn = onAnyCallbacks[i];
                        try
                        {
                            Data.DataAPIHelper.QueueJavascript(fn, new object[] { eventName, eventData });
                        }
                        catch (Exception ex)
                        {
                            Logging.LogError("[SocketIO:NS] OnAny callback error: " + ex.Message);
                        }
                    }
                });
                onAnyRegistered = true;
            }
        }

        public void OffAny()
        {
            onAnyCallbacks.Clear();

            if (nsSocket != null)
            {
                nsSocket.OffAny();
            }

            onAnyRegistered = false;
        }

        public void EmitBinary(string eventName, string base64Data)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (nsSocket == null)
            {
                Logging.LogWarning("[SocketIO:NS->EmitBinary] Socket not initialized.");
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS->EmitBinary] Instance terminated.");
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(base64Data))
                {
                    nsSocket.EmitBinary(eventName, null);
                    return;
                }

                byte[] data = Convert.FromBase64String(base64Data);
                nsSocket.EmitBinary(eventName, data);
            }
            catch (FormatException)
            {
                Logging.LogWarning("[SocketIO:NS->EmitBinary] Invalid base64 data, message dropped.");
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO:NS->EmitBinary] " + ex.Message);
            }
        }

        public void EmitWithAck(string eventName, string data, string functionName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (nsSocket == null)
            {
                Logging.LogWarning("[SocketIO:NS->EmitWithAck] Socket not initialized.");
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS->EmitWithAck] Instance terminated.");
                return;
            }

            try
            {
                Action<string> ackCallback = (response) =>
                {
                    try
                    {
                        Data.DataAPIHelper.QueueJavascript(functionName, new object[] { response });
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("[SocketIO:NS] Ack callback error: " + ex.Message);
                    }
                };

                nsSocket.EmitWithAck(eventName, data, ackCallback);
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO:NS->EmitWithAck] " + ex.Message);
            }
        }

        public void EmitVolatile(string eventName, string data)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (nsSocket == null)
            {
                Logging.LogWarning("[SocketIO:NS->EmitVolatile] Socket not initialized.");
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS->EmitVolatile] Instance terminated.");
                return;
            }

            try
            {
                nsSocket.EmitVolatile(eventName, data);
            }
            catch (Exception ex)
            {
                Logging.LogError("[SocketIO:NS->EmitVolatile] " + ex.Message);
            }
        }

        public void JoinRoom(string room)
        {
            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS] JoinRoom called after terminate.");
                return;
            }

            if (string.IsNullOrEmpty(room))
            {
                return;
            }

            if (nsSocket == null)
            {
                Logging.LogWarning("[SocketIO:NS] Socket not initialized.");
                return;
            }

            nsSocket.JoinRoom(room);
        }

        public void LeaveRoom(string room)
        {
            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS] LeaveRoom called after terminate.");
                return;
            }

            if (string.IsNullOrEmpty(room))
            {
                return;
            }

            if (nsSocket == null)
            {
                Logging.LogWarning("[SocketIO:NS] Socket not initialized.");
                return;
            }

            nsSocket.LeaveRoom(room);
        }

        public void OnBinary(string eventName, string functionName)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (terminated)
            {
                Logging.LogWarning("[SocketIO:NS->OnBinary] Instance terminated.");
                return;
            }

            bool isFirstListenerForEvent = !binaryEventCallbacks.ContainsKey(eventName);

            if (isFirstListenerForEvent)
            {
                binaryEventCallbacks[eventName] = new List<string>();
            }

            binaryEventCallbacks[eventName].Add(functionName);

            if (isFirstListenerForEvent && nsSocket != null)
            {
                nsSocket.OnBinary(eventName, (name, data) =>
                {
                    DispatchBinaryEventToJS(name, data);
                });
            }
        }

        public void OffBinary(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            binaryEventCallbacks.Remove(eventName);

            if (nsSocket != null)
            {
                nsSocket.OffBinary(eventName);
            }
        }

        internal void Dispose()
        {
            terminated = true;
            eventCallbacks.Clear();
            binaryEventCallbacks.Clear();
            onceCallbacks.Clear();
            onAnyCallbacks.Clear();
            onAnyRegistered = false;
        }

        private void DispatchEventToJS(string eventName, string data)
        {
            if (terminated || !eventCallbacks.ContainsKey(eventName))
            {
                return;
            }

            var functions = eventCallbacks[eventName];
            for (int i = 0; i < functions.Count; i++)
            {
                string fn = functions[i];
                try
                {
                    Data.DataAPIHelper.QueueJavascript(fn, new object[] { data });
                }
                catch (Exception ex)
                {
                    Logging.LogError("[SocketIO:NS] Callback error for event '"
                        + eventName + "': " + ex.Message);
                }
            }
        }

        private void DispatchBinaryEventToJS(string eventName, byte[] data)
        {
            if (terminated || !binaryEventCallbacks.ContainsKey(eventName))
            {
                return;
            }

            string base64 = data != null ? Convert.ToBase64String(data) : "";

            var functions = binaryEventCallbacks[eventName];
            for (int i = 0; i < functions.Count; i++)
            {
                string fn = functions[i];
                try
                {
                    Data.DataAPIHelper.QueueJavascript(fn, new object[] { base64 });
                }
                catch (Exception ex)
                {
                    Logging.LogError("[SocketIO:NS] Binary callback error for event '"
                        + eventName + "': " + ex.Message);
                }
            }
        }
    }
#endif
}
