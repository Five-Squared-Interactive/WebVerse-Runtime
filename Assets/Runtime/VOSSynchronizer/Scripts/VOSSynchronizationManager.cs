// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if USE_WEBINTERFACE
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.WebInterface.MQTT;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.VOSSynchronization
{
    /// <summary>
    /// Class for a VOS Synchronization Manager.
    /// </summary>
    public class VOSSynchronizationManager : BaseManager
    {
        /// <summary>
        /// VOS synchronizers being managed.
        /// </summary>
        private List<VOSSynchronizer> vosSynchronizers;

        /// <summary>
        /// Mapping of VOS synchronizers to session IDs.
        /// </summary>
        private Dictionary<string, Tuple<VOSSynchronizer, Guid>> vosSynchronizersAndSessions;

        /// <summary>
        /// Initialize the synchronization manager.
        /// </summary>
        public override void Initialize()
        {
            if (vosSynchronizers != null)
            {
                Logging.LogError("[VOSSynchronizationManager->Initialize] Already initialized.");
                return;
            }

            vosSynchronizers = new List<VOSSynchronizer>();
            vosSynchronizersAndSessions = new Dictionary<string, Tuple<VOSSynchronizer, Guid>>();

            base.Initialize();
        }

        /// <summary>
        /// Reset the synchronization manager.
        /// </summary>
        public void Reset()
        {
            if (vosSynchronizers != null)
            {
                foreach (VOSSynchronizer synchronizer in vosSynchronizers)
                {
                    synchronizer.Terminate();
                }
            }
            vosSynchronizers = new List<VOSSynchronizer>();
            vosSynchronizersAndSessions = new Dictionary<string, Tuple<VOSSynchronizer, Guid>>();
        }

        /// <summary>
        /// Terminate the synchronization manager.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();

            Reset();
        }

        /// <summary>
        /// Add a synchronizer and session.
        /// </summary>
        /// <param name="id">ID for the synchronizer.</param>
        /// <param name="host">Host for the synchronizer.</param>
        /// <param name="port">Port for the synchronizer.</param>
        /// <param name="tls">Whether or not to use TLS.</param>
        /// <param name="transport">Transport to use.</param>
        /// <param name="worldOffset">Offset for this synchronized client in the world.</param>
        /// <param name="session">Name for the session.</param>
        /// <returns>Tuple containing the synchronizer and session ID.</returns>
        public Tuple<VOSSynchronizer, Guid> AddSynchronizerAndSession(string id, string host, int port, bool tls,
            MQTTClient.Transports transport, Vector3 worldOffset, string session, string clientID = null,
            string clientToken = null)
        {
            Guid newSessionID = Guid.NewGuid();
            return AddSynchronizerAndSession(id, host, port, tls, transport, worldOffset, newSessionID, session,
                clientID, clientToken);
        }

        /// <summary>
        /// Add a synchronizer and session.
        /// </summary>
        /// <param name="id">ID for the synchronizer.</param>
        /// <param name="host">Host for the synchronizer.</param>
        /// <param name="port">Port for the synchronizer.</param>
        /// <param name="tls">Whether or not to use TLS.</param>
        /// <param name="transport">Transport to use.</param>
        /// <param name="worldOffset">Offset for this synchronized client in the world.</param>
        /// <param name="sessionTag">Name for the session.</param>
        /// <returns>Tuple containing the synchronizer and session ID.</returns>
        public Tuple<VOSSynchronizer, Guid> AddSynchronizerAndSession(string id, string host, int port, bool tls,
            MQTTClient.Transports transport, Vector3 worldOffset, Guid sessionID, string sessionTag,
            string clientID = null, string clientToken = null)
        {
            if (vosSynchronizers == null)
            {
                Logging.LogError("[VOSSynchronizationManager->AddSynchronizerAndSession] Not initialized.");
                return null;
            }

            VOSSynchronizer newSync = AddSynchronizer(host, port, tls, transport, worldOffset, clientID, clientToken);
            Action onConnected = () =>
            {
                newSync.CreateSession(sessionID, sessionTag);
                newSync.JoinSession(sessionID, "VEML");
            };
            newSync.Connect(onConnected);
            Tuple<VOSSynchronizer, Guid> combo = new Tuple<VOSSynchronizer, Guid>(newSync, sessionID);
            vosSynchronizersAndSessions.Add(id, combo);

            return combo;
        }

        /// <summary>
        /// Add a synchronizer and session.
        /// </summary>
        /// <param name="id">ID for the synchronizer.</param>
        /// <param name="host">Host for the synchronizer.</param>
        /// <param name="port">Port for the synchronizer.</param>
        /// <param name="tls">Whether or not to use TLS.</param>
        /// <param name="transport">Transport to use.</param>
        /// <param name="worldOffset">Offset for this synchronized client in the world.</param>
        /// <param name="sessionTag">Name for the session.</param>
        /// <returns>Tuple containing the synchronizer, session ID, and client ID.</returns>
        public Tuple<VOSSynchronizer, Guid, string> AddSynchronizerAndJoinSession(string clientTag,
            string host, int port, bool tls, MQTTClient.Transports transport, Vector3 worldOffset,
            Guid sessionID, Action onJoined = null, string clientID = null, string clientToken = null)
        {
            if (vosSynchronizers == null)
            {
                Logging.LogError("[VOSSynchronizationManager->AddSynchronizerAndJoinSession] Not initialized.");
                return null;
            }

            string clID = null;
            VOSSynchronizer newSync = AddSynchronizer(host, port, tls, transport, worldOffset, clientID, clientToken);
            Action onConnected = () =>
            {
                clID = newSync.JoinSession(sessionID, clientTag);
                if (onJoined != null)
                {
                    onJoined.Invoke();
                }
            };
            newSync.Connect(onConnected);
            Tuple<VOSSynchronizer, Guid> combo = new Tuple<VOSSynchronizer, Guid>(newSync, sessionID);
            Tuple<VOSSynchronizer, Guid, string> returnCombo =
                new Tuple<VOSSynchronizer, Guid, string>(newSync, sessionID, clID);
            vosSynchronizersAndSessions.Add(sessionID.ToString(), combo);

            return returnCombo;
        }

        /// <summary>
        /// Add a synchronizer.
        /// </summary>
        /// <param name="host">Host for the synchronizer.</param>
        /// <param name="port">Port for the synchronizer.</param>
        /// <param name="tls">Whether or not to use TLS.</param>
        /// <param name="transport">Transport to use.</param>
        /// <param name="worldOffset">Offset for this synchronized client in the world.</param>
        /// <returns>Synchronizer that has been added.</returns>
        public VOSSynchronizer AddSynchronizer(string host, int port, bool tls,
            MQTTClient.Transports transport, Vector3 worldOffset, string clientID = null,
            string clientToken = null)
        {
            if (vosSynchronizers == null)
            {
                Logging.LogError("[VOSSynchronizationManager->AddSynchronizer] Not initialized.");
                return null;
            }

            GameObject newSynchronizerGO = new GameObject("Synchronizer-"
                + host + ":" + port);
            VOSSynchronizer vosSynchronizer = newSynchronizerGO.AddComponent<VOSSynchronizer>();
            vosSynchronizer.Initialize(host, port, tls, transport, worldOffset, clientID, clientToken);
            vosSynchronizers.Add(vosSynchronizer);
            return vosSynchronizer;
        }

        /// <summary>
        /// Remove a synchronizer.
        /// </summary>
        /// <param name="synchronizer">Synchronizer to use.</param>
        public void RemoveSynchronizer(VOSSynchronizer synchronizer)
        {
            if (vosSynchronizers == null)
            {
                Logging.LogError("[VOSSynchronizationManager->RemoveSynchronizer] Not initialized.");
                return;
            }

            if (vosSynchronizers.Contains(synchronizer))
            {
                vosSynchronizers.Remove(synchronizer);
            }
        }

        /// <summary>
        /// Remove a synchronizer.
        /// </summary>
        /// <param name="host">Host of the synchronizer to remove.</param>
        /// <param name="port">Port of the synchronizer to remove.</param>
        public void RemoveSynchronizers(string host, int port)
        {
            VOSSynchronizer[] vosSynchronizers = GetSynchronizers(host, port);
            if (vosSynchronizers == null)
            {
                Logging.LogWarning("[VOSSynchronizationManager->RemoveSynchronizer] Synchronizers not found.");
                return;
            }
            foreach (VOSSynchronizer vosSynchronizer in vosSynchronizers)
            {
                RemoveSynchronizer(vosSynchronizer);
            }
        }

        /// <summary>
        /// Get synchronizers for a host and port.
        /// </summary>
        /// <param name="host">Host of the synchronizers to get.</param>
        /// <param name="port">Port of the synchronizers to get.</param>
        /// <returns>Synchronizers.</returns>
        public VOSSynchronizer[] GetSynchronizers(string host, int port)
        {
            if (vosSynchronizers == null)
            {
                Logging.LogError("[VOSSynchronizationManager->GetSynchronizers] Not initialized.");
                return null;
            }

            List<VOSSynchronizer> synchronizers = new List<VOSSynchronizer>();
            foreach (VOSSynchronizer vosSynchronizer in vosSynchronizers)
            {
                if (vosSynchronizer.host == host && vosSynchronizer.port == port)
                {
                    synchronizers.Add(vosSynchronizer);
                }
            }
            return synchronizers.ToArray();
        }

        /// <summary>
        /// Get a synchronizer and session.
        /// </summary>
        /// <param name="id">ID of the session.</param>
        /// <returns>A tuple containing the synchronizer and session, or null if not found.</returns>
        public Tuple<VOSSynchronizer, Guid> GetSynchronizerAndSession(string id)
        {
            if (vosSynchronizersAndSessions == null)
            {
                Logging.LogError("[VOSSynchronizationManager->GetSynchronizerAndSession] Not initialized.");
                return null;
            }

            if (vosSynchronizersAndSessions.ContainsKey(id))
            {
                return vosSynchronizersAndSessions[id];
            }
            return null;
        }

        /// <summary>
        /// Get a synchronizer for a session.
        /// </summary>
        /// <param name="id">ID of the session.</param>
        /// <returns>The synchronizer corresponding to the session, or null if not found.</returns>
        public VOSSynchronizer GetSynchronizerForSession(string id)
        {
            if (vosSynchronizersAndSessions == null)
            {
                Logging.LogError("[VOSSynchronizationManager->GetSynchronizerForSession] Not initialized.");
                return null;
            }

            if (string.IsNullOrEmpty(id))
            {
                Logging.LogError("[VOSSynchronizationManager->GetSynchronizerForSession] ID is null or empty.");
                return null;
            }
            
            if (vosSynchronizersAndSessions.ContainsKey(id))
            {
                return vosSynchronizersAndSessions[id].Item1;
            }
            return null;
        }
    }
}
#endif