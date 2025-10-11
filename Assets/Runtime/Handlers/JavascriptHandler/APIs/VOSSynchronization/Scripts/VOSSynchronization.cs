// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if USE_WEBINTERFACE
using FiveSQD.StraightFour.Utilities;
using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.VOSSynchronization
{
    /// <summary>
    /// VOS Synchronization Methods.
    /// </summary>
    public class VOSSynchronization
    {
        /// <summary>
        /// VOS Synchronization Transports.
        /// </summary>
        public enum Transport { TCP, WebSocket }
#if FALSE
        /// <summary>
        /// Connect to a VOS Synchronization Server.
        /// </summary>
        /// <param name="host">Host to connect to.</param>
        /// <param name="port">Port at which to connect to host.</param>
        /// <param name="tls">Whether or not to use TLS.</param>
        /// <param name="onConnected">Logic to execute upon connection.</param>
        /// <param name="transport">Transport to use.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool ConnectToService(string host, int port, bool tls,
            string onConnected = null, Transport transport = Transport.TCP)
        {
            WebInterface.MQTT.MQTTClient.Transports tp = WebInterface.MQTT.MQTTClient.Transports.TCP;
            switch (transport)
            {
                case Transport.TCP:
                    tp = WebInterface.MQTT.MQTTClient.Transports.TCP;
                    break;

                case Transport.WebSocket:
                    tp = WebInterface.MQTT.MQTTClient.Transports.WebSockets;
                    break;
            }

            WebVerse.VOSSynchronization.VOSSynchronizer newSynchronizer
                = WebVerseRuntime.Instance.vosSynchronizationManager.AddSynchronizer(host, port, tls, tp);
            if (newSynchronizer == null)
            {
                LogSystem.LogError("[VOSSynchronization:ConnectToService] Unable to set up synchronizer.");
                return false;
            }
            Action onConnectedAction = () => {
                InvokeCallback(onConnected);
            };
            newSynchronizer.Connect(onConnected == null ? null : onConnectedAction);
            return true;
        }

        /// <summary>
        /// Disconnect from a VOS Synchronization Server.
        /// </summary>
        /// <param name="host">Host of the connection to disconnect.</param>
        /// <param name="port">Port of the connection to disconnect.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool DisconnectService(string host, int port)
        {
            WebVerse.VOSSynchronization.VOSSynchronizer[] synchronizersToRemove
                = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizers(host, port);
            if (synchronizersToRemove == null)
            {
                //LogSystem.LogError("[VOSSynchronization:DisconnectService] Can't find service to disconnect.");
                return false;
            }
            foreach (WebVerse.VOSSynchronization.VOSSynchronizer synchronizerToRemove in synchronizersToRemove)
            {
                synchronizerToRemove.Disconnect();
            }
            return true;
        }
#endif
        /// <summary>
        /// Create a VOS Synchronization Session.
        /// </summary>
        /// <param name="host">Host of the connection to create the session on.</param>
        /// <param name="port">Port of the connection to create the session on.</param>
        /// <param name="id">RFC 4122-encoded UUID identifier for the session.</param>
        /// <param name="tag">Tag for the session.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool CreateSession(string host, int port, bool tls, string id, string tag,
            Transport transport = Transport.TCP)
        {
            return CreateSession(host, port, tls, id, tag, Vector3.zero, transport);
        }

        /// <summary>
        /// Create a VOS Synchronization Session.
        /// </summary>
        /// <param name="host">Host of the connection to create the session on.</param>
        /// <param name="port">Port of the connection to create the session on.</param>
        /// <param name="id">RFC 4122-encoded UUID identifier for the session.</param>
        /// <param name="tag">Tag for the session.</param>
        /// <param name="worldOffset">Offset for this synchronized client in the world.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool CreateSession(string host, int port, bool tls, string id, string tag,
            Vector3 worldOffset, Transport transport = Transport.TCP, string clientID = null,
            string clientToken = null)
        {
            Tuple<WebVerse.VOSSynchronization.VOSSynchronizer, Guid> newSynchronizerAndSession
                = WebVerseRuntime.Instance.vosSynchronizationManager.AddSynchronizerAndSession(
                    Guid.NewGuid().ToString(), host, port, tls,
                    transport == Transport.TCP ? WebInterface.MQTT.MQTTClient.Transports.TCP :
                    WebInterface.MQTT.MQTTClient.Transports.WebSockets,
                    new UnityEngine.Vector3(worldOffset.x, worldOffset.y, worldOffset.z), Guid.Parse(id), tag,
                    clientID, clientToken);
            if (newSynchronizerAndSession == null)
            {
                LogSystem.LogError("[VOSSynchronization:CreateSession] Unable to set up synchronizer.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Destroy a VOS Synchronization Session.
        /// </summary>
        /// <param name="host">Host of the connection of the session to destroy.</param>
        /// <param name="port">Port of the connection of the session to destroy.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool DestroySession(string id)
        {
            WebVerse.VOSSynchronization.VOSSynchronizer synchronizerToDestroySessionOn
                = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerForSession(id);
            //WebVerse.VOSSynchronization.VOSSynchronizer synchronizerToDestroySessionOn
            //    = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizers(host, port);
            if (synchronizerToDestroySessionOn == null)
            {
                LogSystem.LogError("[VOSSynchronization:DestroySession] Can't find synchronizer to destroy session on.");
                return false;
            }
            synchronizerToDestroySessionOn.DestroySession();
            return true;
        }

        /// <summary>
        /// Join a VOS Synchronization Session.
        /// </summary>
        /// <param name="host">Host of the connection of the session to join.</param>
        /// <param name="port">Port of the connection of the session to join.</param>
        /// <param name="id">RFC 4122-encoded UUID identifier of the session.</param>
        /// <param name="tag">Tag of the client.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static string JoinSession(string host, int port, bool tls, string id, string tag,
            string callback = null, Transport transport = Transport.TCP, string clientID = null,
            string clientToken = null)
        {
            return JoinSession(host, port, tls, id, tag, Vector3.zero, callback, transport, clientID,
                clientToken);
        }

        /// <summary>
        /// Join a VOS Synchronization Session.
        /// </summary>
        /// <param name="host">Host of the connection of the session to join.</param>
        /// <param name="port">Port of the connection of the session to join.</param>
        /// <param name="id">RFC 4122-encoded UUID identifier of the session.</param>
        /// <param name="tag">Tag of the client.</param>
        /// <param name="worldOffset">Offset for this synchronized client in the world.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static string JoinSession(string host, int port, bool tls, string id, string tag,
            Vector3 worldOffset, string callback = null, Transport transport = Transport.TCP,
            string clientID = null, string clientToken = null)
        {
            // Handle callback.
            Action onJoinAction = null;
            if (!string.IsNullOrEmpty(callback))
            {
                onJoinAction = () => {
                    WebVerseRuntime.Instance.javascriptHandler.Run(callback);
                };
            }

            Tuple<WebVerse.VOSSynchronization.VOSSynchronizer, Guid, string> newSynchronizerAndSession = null;
            Action onSynchronizerJoinedAction = () => {
                if (onJoinAction != null)
                {
                    newSynchronizerAndSession.Item1.GetSessionState(onJoinAction);
                }
            };

            newSynchronizerAndSession
                = WebVerseRuntime.Instance.vosSynchronizationManager.AddSynchronizerAndJoinSession(
                    tag, host, port, tls,
                    transport == Transport.TCP ? WebInterface.MQTT.MQTTClient.Transports.TCP :
                    WebInterface.MQTT.MQTTClient.Transports.WebSockets,
                    new UnityEngine.Vector3(worldOffset.x, worldOffset.y, worldOffset.z),
                    Guid.Parse(id), onSynchronizerJoinedAction,
                        clientID, clientToken);
            if (newSynchronizerAndSession == null)
            {
                LogSystem.LogError("[VOSSynchronization:JoinSession] Unable to set up synchronizer.");
                return null;
            }

            Guid uuid = Guid.NewGuid();
            if (Guid.TryParse(id, out uuid) == false)
            {
                LogSystem.LogError("[VOSSynchronization:JoinSession] Invalid UUID.");
                return null;
            }

            string clID = newSynchronizerAndSession.Item3;

            return clID;
        }

        /// <summary>
        /// Exit a VOS Synchronization Session.
        /// </summary>
        /// <param name="host">Host of the connection of the session to exit.</param>
        /// <param name="port">Port of the connection of the session to exit.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool ExitSession(string id)
        {
            WebVerse.VOSSynchronization.VOSSynchronizer synchronizerToExitSessionOn
                = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerForSession(id);
            if (synchronizerToExitSessionOn == null)
            {
                LogSystem.LogError("[VOSSynchronization:ExitSession] Can't find synchronizer to exit session on.");
                return false;
            }

            synchronizerToExitSessionOn.ExitSession();
            return true;
        }

        /// <summary>
        /// Indicates whether a session is established.
        /// </summary>
        /// <param name="sessionID">ID of session to check.</param>
        /// <returns>Whether or not the session is established.</returns>
        public static bool IsSessionEstablished(string sessionID)
        {
            if (sessionID == null)
            {
                return false;
            }

            WebVerse.VOSSynchronization.VOSSynchronizer synchronizer
                = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerForSession(sessionID);
            if (synchronizer == null)
            {
                return false;
            }

            return synchronizer.currentSessionID != null;
        }

        /// <summary>
        /// Start Synchronizing an Entity.
        /// </summary>
        /// <param name="host">Host of the connection of the session to synchronize an entity on.</param>
        /// <param name="port">Port of the connection of the session to synchronize an entity on.</param>
        /// <param name="entityID">ID of the entity to synchronize.</param>
        /// <param name="deleteWithClient">Whether or not to delete the entity upon disconnection of the client.</param>
        /// <param name="filePath">Path to a file to load with the entity.</param>
        /// <param name="resources">Resources to include with the entity.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool StartSynchronizingEntity(string sessionID,
            string entityID, bool deleteWithClient = false, string filePath = null, string[] resources = null)
        {
            WebVerse.VOSSynchronization.VOSSynchronizer synchronizerToSynchronizeEntityOn
                = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerForSession(sessionID);
            if (synchronizerToSynchronizeEntityOn == null)
            {
                LogSystem.LogError("[VOSSynchronization:StartSynchronizingEntity] Can't find synchronizer to synchronize entity on.");
                return false;
            }

            Guid uuid = Guid.NewGuid();
            if (Guid.TryParse(entityID, out uuid) == false)
            {
                LogSystem.LogError("[VOSSynchronizationAPI->StartSynchronizingEntity] Invalid entity UUID.");
                return false;
            }

            StraightFour.Entity.BaseEntity entityToSynchronize = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(uuid);
            if (entityToSynchronize == null)
            {
                LogSystem.LogError("[VOSSynchronizationAPI->StartSynchronizingEntity] Unable to find entity to synchronize.");
                return false;
            }
            synchronizerToSynchronizeEntityOn.AddSynchronizedEntity(entityToSynchronize, deleteWithClient, filePath, resources);
            return true;
        }

        /// <summary>
        /// Stop Synchronizing an Entity.
        /// </summary>
        /// <param name="host">Host of the connection of the session to stop synchronizing an entity on.</param>
        /// <param name="port">Port of the connection of the session to stop synchronizing an entity on.</param>
        /// <param name="entityID">ID of the entity to stop synchronizing.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool StopSynchronizingEntity(string sessionID, string entityID)
        {
            WebVerse.VOSSynchronization.VOSSynchronizer synchronizerToStopSynchronizingEntityOn
                = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerForSession(sessionID);
            if (synchronizerToStopSynchronizingEntityOn == null)
            {
                LogSystem.LogError("[VOSSynchronization:StopSynchronizingEntity] Can't find synchronizer to stop synchronizing entity on.");
                return false;
            }

            Guid uuid = Guid.NewGuid();
            if (Guid.TryParse(entityID, out uuid) == false)
            {
                LogSystem.LogError("[VOSSynchronization:StopSynchronizingEntity] Invalid entity UUID.");
                return false;
            }

            StraightFour.Entity.BaseEntity entityToStopSynchronizing = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(uuid);
            if (entityToStopSynchronizing == null)
            {
                LogSystem.LogError("[VOSSynchronization:StopSynchronizingEntity] Unable to find entity to stop synchronizing.");
                return false;
            }

            synchronizerToStopSynchronizingEntityOn.RemoveSynchronizedEntity(entityToStopSynchronizing);
            return true;
        }

        /// <summary>
        /// Send a Message.
        /// </summary>
        /// <param name="host">Host of the connection of the session to send a message on.</param>
        /// <param name="port">Port of the connection of the session to send a message on.</param>
        /// <param name="topic">Topic to send a message on.</param>
        /// <param name="message">Message to send.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SendMessage(string sessionID, string topic, string message)
        {
            WebVerse.VOSSynchronization.VOSSynchronizer synchronizerToSendMessageOn
                = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerForSession(sessionID);
            if (synchronizerToSendMessageOn == null)
            {
                LogSystem.LogError("[VOSSynchronization:SendMessage] Can't find synchronizer to send message on.");
                return false;
            }
            
            if (!synchronizerToSendMessageOn.isConnected)
            {
                Logging.LogWarning("[VOSSynchronization:SendMessage] Not connected.");
                return false;
            }

            synchronizerToSendMessageOn.SendMessage(topic, message);
            return true;
        }

        /// <summary>
        /// Register a Message Callback.
        /// </summary>
        /// <param name="host">Host of the connection of the session to register a message callback on.</param>
        /// <param name="port">Port of the connection of the session to register a message callback on.</param>
        /// <param name="callback">Logic to invoke when message is received.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool RegisterMessageCallback(string sessionID, string callback)
        {
            WebVerse.VOSSynchronization.VOSSynchronizer synchronizerToRegisterMessageCallbackOn
                = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerForSession(sessionID);
            if (synchronizerToRegisterMessageCallbackOn == null)
            {
                LogSystem.LogError("[VOSSynchronization:RegisterMessageCallback] Can't find synchronizer to register message callback on.");
                return false;
            }

            // Handle callback.
            Action<string, string, string> onMessageAction = null;
            if (!string.IsNullOrEmpty(callback))
            {
                onMessageAction = (string topic, string senderID, string message) => {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(callback, new object[] { topic, senderID, message });
                };
            }
            synchronizerToRegisterMessageCallbackOn.AddMessageListener(onMessageAction);

            return true;
        }

        /// <summary>
        /// Get the Tag for a User.
        /// </summary>
        /// <param name="host">Host of the connection of the session of the user to get the tag of.</param>
        /// <param name="port">Port of the connection of the session of the user to get the tag of.</param>
        /// <param name="userID">ID of the user to get the tag of.</param>
        /// <returns>Tag of the user.</returns>
        public static string GetUserTag(string sessionID, string userID)
        {
            WebVerse.VOSSynchronization.VOSSynchronizer synchronizerToGetUserTagOn
                = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerForSession(sessionID);
            if (synchronizerToGetUserTagOn == null)
            {
                LogSystem.LogError("[VOSSynchronization:GetUserTag] Can't find synchronizer to get user tag on.");
                return null;
            }

            return synchronizerToGetUserTagOn.GetUserTag(userID);
        }

        /// <summary>
        /// Invoke a Javascript Callback.
        /// </summary>
        /// <param name="function">Logic to invoke.</param>
        private static void InvokeCallback(string function)
        {
            WebVerseRuntime.Instance.javascriptHandler.Run(function);
        }
    }
}
#endif