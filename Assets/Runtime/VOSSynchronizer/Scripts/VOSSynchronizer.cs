// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if USE_WEBINTERFACE
using FiveSQD.WebVerse.WebInterface.MQTT;
using FiveSQD.StraightFour.Entity;
using FiveSQD.StraightFour.Synchronization;
using FiveSQD.StraightFour.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.StraightFour.Entity.Terrain;
using System.Linq;

namespace FiveSQD.WebVerse.VOSSynchronization
{
    /// <summary>
    /// Class for a VOS Synchronizer.
    /// </summary>
    public class VOSSynchronizer : BaseSynchronizer
    {
        /// <summary>
        /// Host address for the synchronizer.
        /// </summary>
        public string host
        {
            get
            {
                if (mqttClient == null)
                {
                    LogSystem.LogError("[VOSSynchronizer->host] Not initialized.");
                    return null;
                }
                return mqttClient.host;
            }
        }

        /// <summary>
        /// Port for the synchronizer.
        /// </summary>
        public int port
        {
            get
            {
                if (mqttClient == null)
                {
                    LogSystem.LogError("[VOSSynchronizer->port] Not initialized.");
                    return -1;
                }
                return mqttClient.port;
            }
        }

        /// <summary>
        /// Whether or not the synchronizer is connected.
        /// </summary>
        public bool isConnected { get; private set; }

        /// <summary>
        /// Interval in seconds between heartbeats.
        /// </summary>
        public float heartbeatInterval = 5;

        /// <summary>
        /// Offset for this synchronized client in the world.
        /// </summary>
        public Vector3 synchronizationOffset { get; private set; }

        /// <summary>
        /// ID of the current session.
        /// </summary>
        public Guid? currentSessionID { get; private set; }

        /// <summary>
        /// MQTT client.
        /// </summary>
        private MQTTClient mqttClient;

        /// <summary>
        /// Currently available sessions.
        /// </summary>
        private Dictionary<Guid, string> availableSessions;

        /// <summary>
        /// Currently synchronized users.
        /// </summary>
        private Dictionary<string, string> synchronizedUsers;

        /// <summary>
        /// Currently synchronized entities.
        /// </summary>
        private List<BaseEntity> synchronizedEntities;

        /// <summary>
        /// ID of the current client.
        /// </summary>
        private string currentClientID;

        /// <summary>
        /// Token for the current client.
        /// </summary>
        private string currentClientToken;

        /// <summary>
        /// Tag of the current client.
        /// </summary>
        private string currentClientTag;

        /// <summary>
        /// Action to perform upon connection.
        /// </summary>
        private Action onConnected;

        /// <summary>
        /// Action to perform upon getting state.
        /// </summary>
        private Action onGotState;

        /// <summary>
        /// Action to perform upon getting message.
        /// </summary>
        private List<Action<string, string, string>> onMessage;

        /// <summary>
        /// The current session state message.
        /// </summary>
        private VOSSynchronizationMessages.SessionMessages.SessionStateMessage currentStateMessage;

        /// <summary>
        /// Whether or not state has been requested.
        /// </summary>
        private bool requestedState = false;

        /// <summary>
        /// Whether or not to attempt reconnection.
        /// </summary>
        private bool attemptReconnect = false;

        /// <summary>
        /// Whether or not disconnection has been initiated.
        /// </summary>
        private bool initiatedDisconnect = false;

        /// <summary>
        /// Initialize the synchronizer.
        /// </summary>
        /// <param name="host">Host to use.</param>
        /// <param name="port">Port to use.</param>
        /// <param name="tls">Whether or not to use TLS.</param>
        /// <param name="transport">Transport to use (tcp or ws).</param>
        /// <param name="worldOffset">Offset for this synchronized client in the world.</param>
        public void Initialize(string host, int port, bool tls,
            MQTTClient.Transports transport, Vector3 worldOffset,
            string clientID = null, string clientToken = null)
        {
            if (mqttClient != null)
            {
                LogSystem.LogError("[VOSSynchronizer->Initialize] Already initialized.");
                return;
            }

            Action<MQTTClient> onConnectedAction = new Action<MQTTClient>((cl) => { OnConnected();  });
            Action<MQTTClient, byte, string> onDisconnectedAction =
                new Action<MQTTClient, byte, string>((cl, code, msg) => { OnDisconnected(code, msg); });
            Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState> onStateChangedAction =
                new Action<MQTTClient, MQTTClient.ClientState, MQTTClient.ClientState>((cl, oldS, newS)
                => { OnStateChanged(oldS + " => " + newS); });
            Action<MQTTClient, string> onErrorAction = new Action<MQTTClient, string>((cl, msg) => { OnError(msg); });
            mqttClient = new MQTTClient(host, port, tls, transport,
                onConnectedAction, onDisconnectedAction, onStateChangedAction, onErrorAction, "/mqtt");
            isConnected = false;
            synchronizedUsers = new Dictionary<string, string>();
            synchronizedEntities = new List<BaseEntity>();
            availableSessions = new Dictionary<Guid, string>();
            currentClientID = clientID;
            currentClientToken = clientToken;
            currentSessionID = null;
            currentStateMessage = null;
            onMessage = new List<Action<string, string, string>>();
            onGotState = null;
            synchronizationOffset = worldOffset;
        }

        /// <summary>
        /// Terminate the synchronizer.
        /// </summary>
        public void Terminate()
        {
            ExitSession();
            Disconnect();
            mqttClient = null;
        }

        /// <summary>
        /// Time since the last heartbeat.
        /// </summary>
        private float timeSinceLastHeartbeat = 0;
        private void Update()
        {
            if (mqttClient == null || currentSessionID == null || currentClientID == null)
            {
                return;
            }

            timeSinceLastHeartbeat += Time.deltaTime;
            if (timeSinceLastHeartbeat > heartbeatInterval)
            {
                timeSinceLastHeartbeat = 0;
                SendHeartbeatMessage();
            }
        }

        /// <summary>
        /// Add a message listener.
        /// </summary>
        /// <param name="action">Action to add to the message listener list.</param>
        public void AddMessageListener(Action<string, string, string> action)
        {
            onMessage.Add(action);
        }

        /// <summary>
        /// Get the tag of a given user.
        /// </summary>
        /// <param name="userID">ID of the user to get the tag for.</param>
        /// <returns>Tag of the requested user, or null if not found.</returns>
        public string GetUserTag(string userID)
        {
            if (synchronizedUsers == null)
            {
                return null;
            }

            return synchronizedUsers[userID];
        }

        /// <summary>
        /// Connect the synchronizer to the bus.
        /// </summary>
        /// <param name="onConnected">Action to perform upon connection.</param>
        /// <param name="autoRecconect">Whether or not to automatically attempt reconnection.</param>
        public void Connect(Action onConnected = null, bool autoRecconect = true)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->Connect] Not initialized.");
                return;
            }

            this.onConnected = onConnected;
            this.attemptReconnect = autoRecconect;
            mqttClient.Connect();
        }

        /// <summary>
        /// Disconnect the synchronizer from the bus.
        /// </summary>
        public void Disconnect()
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->Disconnect] Not initialized.");
                return;
            }

            LogSystem.Log("[VOSSynchronizer->Disconnect] Disconnecting " +
                (mqttClient.host == null ? "unk" : mqttClient.host) + ":" + mqttClient.port);

            initiatedDisconnect = true;

            mqttClient.Disconnect("Ending connection.");
        }

        /// <summary>
        /// Create a session.
        /// </summary>
        /// <param name="id">ID of the session.</param>
        /// <param name="tag">Tag for the session.</param>
        public void CreateSession(Guid id, string tag)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->CreateSession] Not initialized.");
                return;
            }

            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.SessionMessages.CreateSessionMessage
                createSessionMessage = new VOSSynchronizationMessages.SessionMessages.CreateSessionMessage(
                    messageID, currentClientID, currentClientToken, id, tag);
            mqttClient.Publish("vos/session/create", JsonConvert.SerializeObject(createSessionMessage));
        }

        /// <summary>
        /// Destroy the session that the synchronizer is in.
        /// </summary>
        public void DestroySession()
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->DestroySession] Not initialized.");
                return;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->DestroySession] Not in session.");
                return;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->DestroySession] No client ID.");
                return;
            }

            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.SessionMessages.DestroySessionMessage
                destroySessionMessage = new VOSSynchronizationMessages.SessionMessages.DestroySessionMessage(
                    messageID, currentClientID, currentClientToken, currentSessionID.Value);
            mqttClient.Publish("vos/session/destroy", JsonConvert.SerializeObject(destroySessionMessage));
        }

        /// <summary>
        /// Join a session.
        /// </summary>
        /// <param name="sessionID">ID of the session to join.</param>
        /// <param name="clientTag">Tag for the client.</param>
        /// <returns>ID of the client in the session, or null if unsuccessful.</returns>
        public string JoinSession(Guid sessionID, string clientTag)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->JoinSession] Not initialized.");
                return null;
            }

            if (currentSessionID != null)
            {
                LogSystem.LogError("[VOSSynchronizer->JoinSession] Already in session.");
                return null;
            }

            currentSessionID = sessionID;
            currentClientTag = clientTag;
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.SessionMessages.JoinSessionMessage
                createSessionMessage = new VOSSynchronizationMessages.SessionMessages
                .JoinSessionMessage(messageID, sessionID, currentClientID, currentClientToken, clientTag);
            mqttClient.Publish("vos/session/join", JsonConvert.SerializeObject(createSessionMessage));
            mqttClient.Subscribe("vos/status/" + sessionID.ToString() + "/#", (info) =>
                LogSystem.Log("Joined session " + currentSessionID.ToString() + "."),
                (cl, topic, topicName, msg) =>
                {
                    if (msg != null)
                    {
                        string message = System.Text.Encoding.UTF8.GetString(
                            msg.payload.data, msg.payload.offset, msg.payload.count);
                        OnMessage(msg.topic, message);
                    }
                }
            );
            return currentClientID;
        }

        /// <summary>
        /// Exit the current session.
        /// </summary>
        public void ExitSession()
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->ExitSession] Not initialized.");
                return;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->ExitSession] Not in session.");
                return;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->ExitSession] No client ID.");
                return;
            }

            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.SessionMessages.LeaveSessionMessage
                exitSessionMessage = new VOSSynchronizationMessages.SessionMessages
                .LeaveSessionMessage(messageID, currentSessionID.Value, currentClientID, currentClientToken);
            mqttClient.Publish("vos/session/exit", JsonConvert.SerializeObject(exitSessionMessage));
            mqttClient.UnSubscribe("vos/status/" + currentSessionID.Value.ToString() + "/#", (info) =>
                LogSystem.Log("Exited session " + currentSessionID.Value.ToString() + ".")
            );
        }

        /// <summary>
        /// Send a heartbeat message.
        /// </summary>
        public void SendHeartbeatMessage()
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SendHeartbeatMessage] Not initialized.");
                return;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SendHeartbeatMessage] Not in session.");
                return;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SendHeartbeatMessage] No client ID.");
                return;
            }

            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.SessionMessages.ClientHeartbeatMessage
                clientHeartbeatMessage = new VOSSynchronizationMessages.SessionMessages
                .ClientHeartbeatMessage(messageID, currentSessionID.Value, currentClientID, currentClientToken);
            mqttClient.Publish("vos/session/heartbeat", JsonConvert.SerializeObject(clientHeartbeatMessage));
        }

        /// <summary>
        /// Send a session state request message and invoke an action when a response is received..
        /// </summary>
        /// <param name="onStateReceived">Action to invoke upon receiving a response.</param>
        public void GetSessionState(Action onStateReceived = null)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->GetSessionState] Not initialized.");
                return;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->GetSessionState] Not in session.");
                return;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->GetSessionState] No client ID.");
                return;
            }

            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.SessionMessages.GetSessionStateMessage
                getSessionStateMessage = new VOSSynchronizationMessages.SessionMessages
                .GetSessionStateMessage(messageID, currentSessionID.Value, currentClientID, currentClientToken);
            requestedState = true;
            onGotState = onStateReceived;
            mqttClient.Publish("vos/session/getstate", JsonConvert.SerializeObject(getSessionStateMessage));
        }

        /// <summary>
        /// Start synchronizing an entity in the current session.
        /// </summary>
        /// <param name="entityToSynchronize">Entity to start synchronizing.</param>
        /// <param name="deleteWithClient">Whether or not to delete the entity when the client disconnects.</param>
        /// <param name="filePath">Path to a resource for the entity.</param>
        /// <param name="resourcesPaths">Paths to additional resources.</param>
        /// <param name="modelOffset">Offset for a model.</param>
        /// <param name="modelRotation">Rotation for a model.</param>
        /// <param name="labelOffset">Offset for a label.</param>
        /// <param name="mass">Mass for an airplane/automobile entity.</param>
        /// <param name="type">Type of the entity.</param>
        /// <param name="wheels">Wheels for an automobile entity.</param>
        /// <param name="menuOptions">Options for a menu.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode AddSynchronizedEntity(BaseEntity entityToSynchronize,
            bool deleteWithClient, string filePath = null, string[] resourcesPaths = null,
            Vector3? modelOffset = null, Quaternion? modelRotation = null, Vector3? labelOffset = null,
            float? mass = null, string type = null, Dictionary<string, float> wheels = null,
            string[] menuOptions = null)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->AddSynchronizedEntity] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->AddSynchronizedEntity] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->AddSynchronizedEntity] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSynchronize == null)
            {
                LogSystem.LogError("[VOSSynchronizer->AddSynchronizedEntity] Invalid entity.");
                return StatusCode.FAILED;
            }

            Guid messageID = Guid.NewGuid();

            Guid? parentID = null;
            BaseEntity parentEntity = entityToSynchronize.GetParent();
            if (parentEntity != null)
            {
                parentID = parentEntity.id;
            }

            if (entityToSynchronize is MeshEntity)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    LogSystem.LogError("[VOSSynchronizer->AddSynchronizedEntity] Invalid file path.");
                    return StatusCode.FAILED;
                }

                VOSSynchronizationMessages.RequestMessages.AddMeshEntityMessage
                    addMeshEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddMeshEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, filePath, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    entityToSynchronize.GetScale(), false, deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createmeshentity",
                    JsonConvert.SerializeObject(addMeshEntityMessage));
            }
            else if (entityToSynchronize is ContainerEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddContainerEntityMessage
                    addContainerEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddContainerEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    entityToSynchronize.GetScale(), deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createcontainerentity",
                    JsonConvert.SerializeObject(addContainerEntityMessage));
            }
            else if (entityToSynchronize is CharacterEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddCharacterEntityMessage
                    addCharacterEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddCharacterEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID, filePath, resourcesPaths,
                    ((CharacterEntity) entityToSynchronize).characterObjectOffset,
                    ((CharacterEntity) entityToSynchronize).characterObjectRotation,
                    ((CharacterEntity) entityToSynchronize).characterLabelOffset, entityToSynchronize.GetPosition(true),
                    entityToSynchronize.GetRotation(true), entityToSynchronize.GetScale(), false, deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createcharacterentity",
                    JsonConvert.SerializeObject(addCharacterEntityMessage));
            }
            else if (entityToSynchronize is ButtonEntity)
            {
                if (parentEntity is not CanvasEntity)
                {
                    Logging.LogWarning("[VOSSynchronizer->AddSynchronizedEntity] Button entity parent must be canvas entity.");
                    return StatusCode.FAILED;
                }

                VOSSynchronizationMessages.RequestMessages.AddButtonEntityMessage
                    addButtonEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddButtonEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    ((ButtonEntity) entityToSynchronize).GetPositionPercent(),
                    ((ButtonEntity) entityToSynchronize).GetSizePercent(), null, deleteWithClient); // TODO event.
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createbuttonentity",
                    JsonConvert.SerializeObject(addButtonEntityMessage));
            }
            else if (entityToSynchronize is CanvasEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddCanvasEntityMessage
                    addCanvasEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddCanvasEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    entityToSynchronize.GetScale(), false, deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createcanvasentity",
                    JsonConvert.SerializeObject(addCanvasEntityMessage));
            }
            else if (entityToSynchronize is InputEntity)
            {
                if (parentEntity is not CanvasEntity)
                {
                    Logging.LogWarning("[VOSSynchronizer->AddSynchronizedEntity] Input entity parent must be canvas entity.");
                    return StatusCode.FAILED;
                }

                VOSSynchronizationMessages.RequestMessages.AddInputEntityMessage
                    addInputEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddInputEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    ((InputEntity) entityToSynchronize).GetPositionPercent(),
                    ((InputEntity) entityToSynchronize).GetSizePercent(), deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createinputentity",
                    JsonConvert.SerializeObject(addInputEntityMessage));
            }
            else if (entityToSynchronize is LightEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddLightEntityMessage
                    addLightEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddLightEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createlightentity",
                    JsonConvert.SerializeObject(addLightEntityMessage));
            }
            else if (entityToSynchronize is TerrainEntity)
            {
                Vector3 size =  ((TerrainEntity) entityToSynchronize).GetSize();
                Handlers.Javascript.APIs.Entity.TerrainEntityLayerMaskCollection lmc
                    = new Handlers.Javascript.APIs.Entity.TerrainEntityLayerMaskCollection();
                Dictionary<int, float[,]> lMasks = ((TerrainEntity) entityToSynchronize).GetLayerMasks();
                if (lMasks != null)
                {
                    foreach (float[,] mask in lMasks.Values)
                    {
                        lmc.AddLayerMask(new Handlers.Javascript.APIs.Entity.TerrainEntityLayerMask(mask));
                    }
                }
                VOSSynchronizationMessages.RequestMessages.AddTerrainEntityMessage
                    addTerrainEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddTerrainEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    size.x, size.y, size.z, ((TerrainEntity) entityToSynchronize).GetHeights(),
                    ((TerrainEntity) entityToSynchronize).GetLayers(), Handlers.VEML.VEMLUtilities.ToCSVLayerMasks(lmc),
                    "heightmap", null, deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createterrainentity",
                    JsonConvert.SerializeObject(addTerrainEntityMessage));
            }
            else if (entityToSynchronize is HybridTerrainEntity)
            {
                Vector3 size = ((HybridTerrainEntity) entityToSynchronize).GetSize();
                Handlers.Javascript.APIs.Entity.TerrainEntityLayerMaskCollection lmc
                    = new Handlers.Javascript.APIs.Entity.TerrainEntityLayerMaskCollection();
                Dictionary<int, float[,]> lMasks = ((HybridTerrainEntity) entityToSynchronize).GetLayerMasks();
                if (lMasks != null)
                {
                    foreach (float[,] mask in lMasks.Values)
                    {
                        lmc.AddLayerMask(new Handlers.Javascript.APIs.Entity.TerrainEntityLayerMask(mask));
                    }
                }

                VOSSynchronizationMessages.RequestMessages.AddTerrainEntityMessage
                    addTerrainEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddTerrainEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    size.x, size.y, size.z, ((HybridTerrainEntity) entityToSynchronize).GetBaseHeights(),
                    ((HybridTerrainEntity) entityToSynchronize).GetLayers(),
                    Handlers.VEML.VEMLUtilities.ToCSVLayerMasks(lmc), "hybrid",
                    ((HybridTerrainEntity) entityToSynchronize).GetTerrainModifications(), deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createterrainentity",
                    JsonConvert.SerializeObject(addTerrainEntityMessage));
            }
            else if (entityToSynchronize is TextEntity)
            {
                if (parentEntity is not CanvasEntity)
                {
                    Logging.LogWarning("[VOSSynchronizer->AddSynchronizedEntity] Text entity parent must be canvas entity.");
                    return StatusCode.FAILED;
                }

                VOSSynchronizationMessages.RequestMessages.AddTextEntityMessage
                    addTextEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddTextEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    ((TextEntity) entityToSynchronize).GetPositionPercent(),
                    ((TextEntity) entityToSynchronize).GetSizePercent(),
                    ((TextEntity) entityToSynchronize).GetText(),
                    ((TextEntity) entityToSynchronize).GetFontSize(), deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createtextentity",
                    JsonConvert.SerializeObject(addTextEntityMessage));
            }
            else if (entityToSynchronize is VoxelEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddVoxelEntityMessage
                    addVoxelEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddVoxelEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    entityToSynchronize.GetScale(), deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createvoxelentity",
                    JsonConvert.SerializeObject(addVoxelEntityMessage));
            }
            else if (entityToSynchronize is AirplaneEntity)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    LogSystem.LogError("[VOSSynchronizer->AddSynchronizedEntity] Invalid file path.");
                    return StatusCode.FAILED;
                }

                VOSSynchronizationMessages.RequestMessages.AddAirplaneEntityMessage
                    addAirplaneEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddAirplaneEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    entityToSynchronize.GetScale(), false, filePath, modelOffset.HasValue ? modelOffset.Value : Vector3.zero,
                    modelRotation.HasValue ? modelRotation.Value : Quaternion.identity, mass.HasValue ? mass.Value : 0,
                    deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createairplaneentity",
                    JsonConvert.SerializeObject(addAirplaneEntityMessage));
            }
            else if (entityToSynchronize is AudioEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddAudioEntityMessage
                    addAudioEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddAudioEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    entityToSynchronize.GetScale(), deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createaudioentity",
                    JsonConvert.SerializeObject(addAudioEntityMessage));
            }
            else if (entityToSynchronize is AutomobileEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddAutomobileEntityMessage
                    addAutomobileEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddAutomobileEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    entityToSynchronize.GetScale(), false, filePath, modelOffset.HasValue ? modelOffset.Value : Vector3.zero,
                    modelRotation.HasValue ? modelRotation.Value : Quaternion.identity, mass.HasValue ? mass.Value : 0,
                    type, ToWheelString(wheels), deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createautomobileentity",
                    JsonConvert.SerializeObject(addAutomobileEntityMessage));
            }
            else if (entityToSynchronize is DropdownEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddDropdownEntityMessage
                    addDropdownEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddDropdownEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    ((DropdownEntity) entityToSynchronize).GetPositionPercent(),
                    ((DropdownEntity) entityToSynchronize).GetSizePercent(),
                    null, menuOptions, deleteWithClient); // TODO event
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createdropdownentity",
                    JsonConvert.SerializeObject(addDropdownEntityMessage));
            }
            else if (entityToSynchronize is HTMLEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddHTMLEntityMessage
                    addHTMLEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddHTMLEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    entityToSynchronize.GetPosition(true), entityToSynchronize.GetRotation(true),
                    entityToSynchronize.GetScale(), false, null, deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createhtmlentity",
                    JsonConvert.SerializeObject(addHTMLEntityMessage));
            }
            else if (entityToSynchronize is ImageEntity)
            {
                VOSSynchronizationMessages.RequestMessages.AddImageEntityMessage
                    addImageEntityMessage = new VOSSynchronizationMessages.RequestMessages
                    .AddImageEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                    entityToSynchronize.id, entityToSynchronize.entityTag, parentID,
                    ((DropdownEntity) entityToSynchronize).GetPositionPercent(),
                    ((DropdownEntity) entityToSynchronize).GetSizePercent(), filePath, deleteWithClient);
                mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString() + "/createimageentity",
                    JsonConvert.SerializeObject(addImageEntityMessage));
            }
            else
            {
                Logging.LogWarning("[VOSSynchronizer->AddSynchronizedEntity] Unhandled entity type. Cannot synchronize.");
                return StatusCode.FAILED;
            }
            synchronizedEntities.Add(entityToSynchronize);
            entityToSynchronize.StartSynchronizing(this);
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Remove an entity from the session.
        /// </summary>
        /// <param name="entityToUnSynchronize">Entity to stop synchronizing.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode RemoveSynchronizedEntity(BaseEntity entityToUnSynchronize)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->RemoveSynchronizedEntity] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->RemoveSynchronizedEntity] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->RemoveSynchronizedEntity] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToUnSynchronize == null)
            {
                LogSystem.LogError("[VOSSynchronizer->RemoveSynchronizedEntity] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.RemoveEntityMessage
                removeEntityMessage = new VOSSynchronizationMessages.RequestMessages
                .RemoveEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToUnSynchronize.id);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToUnSynchronize.id.ToString() + "/remove",
                JsonConvert.SerializeObject(removeEntityMessage));
            synchronizedEntities.Remove(entityToUnSynchronize);
            entityToUnSynchronize.StopSynchronizing();
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Delete an entity that is being synchronized.
        /// </summary>
        /// <param name="entityToDelete">Entity to delete.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode DeleteSynchronizedEntity(BaseEntity entityToDelete)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->DeleteSynchronizedEntity] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->DeleteSynchronizedEntity] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->DeleteSynchronizedEntity] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToDelete == null)
            {
                LogSystem.LogError("[VOSSynchronizer->DeleteSynchronizedEntity] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.DeleteEntityMessage
                deleteEntityMessage = new VOSSynchronizationMessages.RequestMessages
                .DeleteEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToDelete.id);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToDelete.id.ToString() + "/delete",
                JsonConvert.SerializeObject(deleteEntityMessage));
            synchronizedEntities.Remove(entityToDelete);
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Make a canvas entity a screen canvas.
        /// </summary>
        /// <param name="entityToSet">Canvas entity to apply this to.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode MakeScreenCanvas(CanvasEntity entityToSet)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->MakeScreenCanvas] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->MakeScreenCanvas] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->MakeScreenCanvas] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->MakeScreenCanvas] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.SetCanvasTypeMessage
                setCanvasTypeMessage = new VOSSynchronizationMessages.RequestMessages
                .SetCanvasTypeMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, "screen");
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/canvastype",
                JsonConvert.SerializeObject(setCanvasTypeMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Make a canvas entity a world canvas.
        /// </summary>
        /// <param name="entityToSet">Canvas entity to apply this to.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode MakeWorldCanvas(CanvasEntity entityToSet)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->MakeWorldCanvas] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->MakeWorldCanvas] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->MakeWorldCanvas] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->MakeWorldCanvas] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.SetCanvasTypeMessage
                setCanvasTypeMessage = new VOSSynchronizationMessages.RequestMessages
                .SetCanvasTypeMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, "world");
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/canvastype",
                JsonConvert.SerializeObject(setCanvasTypeMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the highlight state of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the highlight state of.</param>
        /// <param name="highlight">Whether or not to enable highlighting.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetHighlight(BaseEntity entityToSet, bool highlight)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetHighlight] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetHighlight] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetHighlight] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetHighlight] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.SetHighlightStateMessage
                setHighlightStateMessage = new VOSSynchronizationMessages.RequestMessages
                .SetHighlightStateMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, highlight);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/highlight",
                JsonConvert.SerializeObject(setHighlightStateMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the interaction state of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the interaction state of.</param>
        /// <param name="state">State to apply.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetInteractionState(BaseEntity entityToSet, BaseEntity.InteractionState? state)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetInteractionState] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetInteractionState] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetInteractionState] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetInteractionState] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.SetInteractionStateMessage
                setInteractionStateMessage = new VOSSynchronizationMessages.RequestMessages
                .SetInteractionStateMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, state.HasValue ? state.Value.ToString().ToLower() : "static");
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/interactionstate",
                JsonConvert.SerializeObject(setInteractionStateMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the motion state of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the motion state of.</param>
        /// <param name="motion">Motion state to set.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetMotion(BaseEntity entityToSet, BaseEntity.EntityMotion? motion)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetMotion] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetMotion] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetMotion] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetMotion] Invalid entity.");
                return StatusCode.FAILED;
            }

            Vector3 angularVelocity = Vector3.zero;
            Vector3 velocity = Vector3.zero;
            bool stationary = true;
            if (motion.HasValue)
            {
                angularVelocity = motion.Value.angularVelocity;
                velocity = motion.Value.velocity;
                stationary = motion.Value.stationary.HasValue ? motion.Value.stationary.Value : true;
            }

            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.SetMotionMessage
                setMotionMessage = new VOSSynchronizationMessages.RequestMessages
                .SetMotionMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, angularVelocity, velocity, stationary);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/motion",
                JsonConvert.SerializeObject(setMotionMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the parent of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the parent of.</param>
        /// <param name="parentToSet">Parent to set.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetParent(BaseEntity entityToSet, BaseEntity parentToSet)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetParent] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetParent] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetParent] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetParent] Invalid entity.");
                return StatusCode.FAILED;
            }

            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.SetParentMessage
                setParentMessage = new VOSSynchronizationMessages.RequestMessages
                .SetParentMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, parentToSet == null ? null : parentToSet.id);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/parent",
                JsonConvert.SerializeObject(setParentMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the physical properties of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the physical properties of.</param>
        /// <param name="properties">Properties to apply.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetPhysicalProperties(BaseEntity entityToSet, BaseEntity.EntityPhysicalProperties? properties)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetMotion] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetMotion] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetMotion] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetMotion] Invalid entity.");
                return StatusCode.FAILED;
            }

            float angularDrag = 0;
            Vector3? centerOfMass = Vector3.zero;
            float drag = 0;
            bool gravitational = false;
            float mass = 0;
            if (properties.HasValue)
            {
                angularDrag = properties.Value.angularDrag.HasValue ? properties.Value.angularDrag.Value : 0;
                centerOfMass = properties.Value.centerOfMass;
                drag = properties.Value.drag.HasValue ? properties.Value.drag.Value : 0;
                gravitational = properties.Value.gravitational.HasValue ? properties.Value.gravitational.Value : false;
            }

            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.SetPhysicalPropertiesMessage
                setPhysicalPropertiesMessage = new VOSSynchronizationMessages.RequestMessages
                .SetPhysicalPropertiesMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, angularDrag, centerOfMass == null ? Vector3.zero : centerOfMass.Value, drag, gravitational, mass);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/physicalproperties",
                JsonConvert.SerializeObject(setPhysicalPropertiesMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the position of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the position of.</param>
        /// <param name="position">Position to apply.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetPosition(BaseEntity entityToSet, Vector3 position)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityPosition] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityPosition] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityPosition] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityPosition] Invalid entity.");
                return StatusCode.FAILED;
            }

            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.UpdateEntityPositionMessage
                updateEntityPositionMessage = new VOSSynchronizationMessages.RequestMessages
                .UpdateEntityPositionMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, ToWorldPosition(position));
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/position",
                JsonConvert.SerializeObject(updateEntityPositionMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the rotation of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the rotation of.</param>
        /// <param name="rotation">Rotation to apply.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetRotation(BaseEntity entityToSet, Quaternion rotation)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityRotation] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityRotation] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityRotation] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityRotation] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.UpdateEntityRotationMessage
                updateEntityRotationMessage = new VOSSynchronizationMessages.RequestMessages
                .UpdateEntityRotationMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, rotation);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/rotation",
                JsonConvert.SerializeObject(updateEntityRotationMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the scale of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the scale of.</param>
        /// <param name="scale">Scale to apply.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetScale(BaseEntity entityToSet, Vector3 scale)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityScale] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityScale] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityScale] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntityScale] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.UpdateEntityScaleMessage
                updateEntityScaleMessage = new VOSSynchronizationMessages.RequestMessages
                .UpdateEntityScaleMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, scale);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/scale",
                JsonConvert.SerializeObject(updateEntityScaleMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the size of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the size of.</param>
        /// <param name="size">Size to apply.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetSize(BaseEntity entityToSet, Vector3 size)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntitySize] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntitySize] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntitySize] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->UpdateSynchronizedEntitySize] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.UpdateEntitySizeMessage
                updateEntitySizeMessage = new VOSSynchronizationMessages.RequestMessages
                .UpdateEntitySizeMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, size);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/size",
                JsonConvert.SerializeObject(updateEntitySizeMessage));
            return StatusCode.SUCCESS;
        }

        public override StatusCode ModifyTerrainEntity(BaseEntity entityToSet,
            HybridTerrainEntity.TerrainOperation modification, Vector3 position,
            TerrainEntityBrushType brushType, int layer)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->ModifyTerrainEntity] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->ModifyTerrainEntity] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->ModifyTerrainEntity] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->ModifyTerrainEntity] Invalid entity.");
                return StatusCode.FAILED;
            }

            string mod;
            switch (modification)
            {
                case HybridTerrainEntity.TerrainOperation.Build:
                    mod = "build";
                    break;

                case HybridTerrainEntity.TerrainOperation.Dig:
                    mod = "dig";
                    break;

                case HybridTerrainEntity.TerrainOperation.Unset:
                default:
                    LogSystem.LogError("[VOSSynchronizer->ModifyTerrainEntity] Invalid modification.");
                    return StatusCode.FAILED;
            }

            string bt;
            switch (brushType)
            {
                case TerrainEntityBrushType.sphere:
                    bt = "sphere";
                    break;

                case TerrainEntityBrushType.roundedCube:
                default:
                    bt = "rounded-cube";
                    break;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.ModifyTerrainEntityMessage
                modifyTerrainEntityMessage = new VOSSynchronizationMessages.RequestMessages
                .ModifyTerrainEntityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, mod, position, bt, layer);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/terrain-mod",
                JsonConvert.SerializeObject(modifyTerrainEntityMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Set the visibility of an entity.
        /// </summary>
        /// <param name="entityToSet">Entity to set the visibility of.</param>
        /// <param name="visible">Visibility to apply.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SetVisibility(BaseEntity entityToSet, bool visible)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetVisibility] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetVisibility] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetVisibility] No client ID.");
                return StatusCode.FAILED;
            }

            if (entityToSet == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SetVisibility] Invalid entity.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.SetVisibilityMessage
                setVisibilityMessage = new VOSSynchronizationMessages.RequestMessages
                .SetVisibilityMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                entityToSet.id, visible);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/entity/" + entityToSet.id.ToString() + "/visibility",
                JsonConvert.SerializeObject(setVisibilityMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Send a message on a synchronizer.
        /// </summary>
        /// <param name="topic">Topic of the message to send.</param>
        /// <param name="message">Message to send.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public override StatusCode SendMessage(string topic, string message)
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SendMessage] Not initialized.");
                return StatusCode.FAILED;
            }

            if (currentSessionID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SendMessage] Not in session.");
                return StatusCode.FAILED;
            }

            if (currentClientID == null)
            {
                LogSystem.LogError("[VOSSynchronizer->SendMessage] No client ID.");
                return StatusCode.FAILED;
            }
            Guid messageID = Guid.NewGuid();
            VOSSynchronizationMessages.RequestMessages.PublishMessageMessage
                publishMessageMessage = new VOSSynchronizationMessages.RequestMessages
                .PublishMessageMessage(messageID, currentClientID, currentClientToken, currentSessionID.Value,
                topic, message);
            mqttClient.Publish("vos/request/" + currentSessionID.Value.ToString()
                + "/message/create",
                JsonConvert.SerializeObject(publishMessageMessage));
            return StatusCode.SUCCESS;
        }

        /// <summary>
        /// Called when synchronizer is connected.
        /// </summary>
        private void OnConnected()
        {
            if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->OnConnected] Not initialized.");
                return;
            }
            LogSystem.Log("[VOSSynchronizer] Connected on "
                + mqttClient.host + ":" + mqttClient.port + ".");
            mqttClient.Subscribe("vos/session/#", (info) =>
            {
                LogSystem.Log("[VOSSynchronizer] Subscribed to vos/session/#.");
                isConnected = true;
            },
            (cl, topic, topicName, msg) =>
            {
                if (msg != null)
                {
                    string message = System.Text.Encoding.UTF8.GetString(
                        msg.payload.data, msg.payload.offset, msg.payload.count);
                    OnMessage(msg.topic, message);
                }
            });
            if (onConnected != null)
            {
                onConnected.Invoke();
                onConnected = null;
            }
        }

        /// <summary>
        /// Called when synchronizer is disconnected.
        /// </summary>
        /// <param name="code">Code for disconnection.</param>
        /// <param name="reason">Reason for disconnection.</param>
        private void OnDisconnected(byte code, string reason)
        {
            if (mqttClient == null)
            {
                //LogSystem.LogError("[VOSSynchronizer->OnDisconnected] Not initialized.");
                LogSystem.Log("[VOSSynchronizer] Disconnected - " + reason);
                //return;
            }
            else
            {
                LogSystem.Log("[VOSSynchronizer] Disconnected from "
                    + mqttClient.host + ":" + mqttClient.port
                    + " - " + reason);
            }
            isConnected = false;

            // If disconnection unexpected, and so configured, attempt reconnection.
            if (!initiatedDisconnect && attemptReconnect)
            {
                AttemptReconnection();
            }

            initiatedDisconnect = false;
        }

        /// <summary>
        /// Called when the connection state of the synchronizer has changed.
        /// </summary>
        /// <param name="info"></param>
        private void OnStateChanged(string info)
        {
            if (mqttClient == null)
            {
                //LogSystem.LogError("[VOSSynchronizer->OnStateChanged] Not initialized.");
                LogSystem.Log("[VOSSynchronizer] State changed - " + info);
                //return;
            }
            else
            {
                LogSystem.Log("[VOSSynchronizer] " + mqttClient.host
                    + ":" + mqttClient.port + " state changed - "
                    + info);
            }
        }

        /// <summary>
        /// Called when a connection error occurs with the synchronizer.
        /// </summary>
        /// <param name="info">Information about the error.</param>
        private void OnError(string info)
        {
            /*if (mqttClient == null)
            {
                LogSystem.LogError("[VOSSynchronizer->OnError] Not intialized.");
                return;
            }*/
            LogSystem.LogError("[VOSSynchronizer] Error: " + info);
        }

        /// <summary>
        /// Called to process a message.
        /// </summary>
        /// <param name="topic">Topic of the message.</param>
        /// <param name="message">Message.</param>
        private void OnMessage(string topic, string message)
        {
            if (topic == "vos/session/new")
            {
                VOSSynchronizationMessages.SessionMessages.NewSessionMessage
                    newSessionMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.SessionMessages.NewSessionMessage>(message);
                if (availableSessions.ContainsKey(Guid.Parse(newSessionMessage.sessionID)))
                {
                    LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Skipping new session "
                        + newSessionMessage.sessionID + ", already saved.");
                }
                else
                {
                    availableSessions.Add(Guid.Parse(newSessionMessage.sessionID),
                        newSessionMessage.sessionTag);
                }
            }
            else if (topic == "vos/session/closed")
            {
                VOSSynchronizationMessages.SessionMessages.ClosedSessionMessage
                    closedSessionMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.SessionMessages.ClosedSessionMessage>(message);
                if (!availableSessions.ContainsKey(Guid.Parse(closedSessionMessage.sessionID)))
                {
                    LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Skipping close session "
                        + closedSessionMessage.sessionID + ", already closed.");
                }
                else
                {
                    availableSessions.Remove(Guid.Parse(closedSessionMessage.sessionID));
                }
            }
            else if (topic == "vos/session/state")
            {
                VOSSynchronizationMessages.SessionMessages.SessionStateMessage
                    sessionStateMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.SessionMessages.SessionStateMessage>(message);
                if (sessionStateMessage.sessionID == currentSessionID.ToString())
                {
                    if (requestedState)
                    {
                        foreach (BaseEntity entity in synchronizedEntities)
                        {
                            DeleteSynchronizedEntity(entity);
                        }

                        synchronizedUsers = new Dictionary<string, string>();
                        foreach (VOSSynchronizationMessages.ClientInfo clientInfo in sessionStateMessage.clients)
                        {
                            synchronizedUsers.Add(clientInfo.id, clientInfo.tag);
                        }

                        synchronizedEntities = new List<BaseEntity>();
                        foreach (VOSSynchronizationMessages.EntityInfo entityInfo in sessionStateMessage.entities)
                        {
                            BaseEntity newEntity = AddEntity(entityInfo);
                            synchronizedEntities.Add(newEntity);
                        }
                    }
                }
            }
            else if (topic.StartsWith("vos/status/"))
            {
                if (topic == "vos/status/" + currentSessionID.ToString() + "/newclient")
                {
                    VOSSynchronizationMessages.SessionMessages.NewClientMessage
                        newClientMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.SessionMessages.NewClientMessage>(message);
                    synchronizedUsers.Add(newClientMessage.clientID, newClientMessage.clientTag);
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/clientleft")
                {
                    VOSSynchronizationMessages.SessionMessages.ClientLeftMessage
                        clientLeftMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.SessionMessages.ClientLeftMessage>(message);
                    synchronizedUsers.Remove(clientLeftMessage.clientID);
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createcontainerentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddContainerEntityMessage
                        addContainerEntityMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.AddContainerEntityMessage>(message);

                    BaseEntity parent = null;
                    if (!string.IsNullOrEmpty(addContainerEntityMessage.parentID))
                    {
                        parent = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addContainerEntityMessage.parentID));
                    }

                    Vector3 scaleSize = Vector3.zero;
                    if (addContainerEntityMessage.scale != null)
                    {
                        scaleSize = addContainerEntityMessage.scale.ToVector3();
                    }
                    else
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Invalid " +
                            "createcontainerentity message.");
                    }
                    if (addContainerEntityMessage.clientID == currentClientID.ToString())
                    {
                        return;
                    }
                    if (addContainerEntityMessage.id != null &&
                        StraightFour.StraightFour.ActiveWorld.entityManager.Exists(Guid.Parse(addContainerEntityMessage.id)))
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Entity " +
                            "already exists.");
                        return;
                    }
                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadContainerEntity(parent,
                        ToOffsetPosition(addContainerEntityMessage.position.ToVector3()),
                        addContainerEntityMessage.rotation.ToQuaternion(), scaleSize,
                        Guid.Parse(addContainerEntityMessage.id));
                    BaseEntity ce = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addContainerEntityMessage.id));
                    if (ce == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        ce.SetVisibility(true, false);
                        ce.entityTag = addContainerEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createcharacterentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddCharacterEntityMessage
                        addCharacterEntityMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.AddCharacterEntityMessage>(message);

                    BaseEntity parent = null;
                    if (!string.IsNullOrEmpty(addCharacterEntityMessage.parentID))
                    {
                        parent = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addCharacterEntityMessage.parentID));
                    }

                    bool isSize = false;
                    Vector3 scaleSize = Vector3.zero;
                    if (addCharacterEntityMessage.scale != null)
                    {
                        isSize = false;
                        scaleSize = addCharacterEntityMessage.scale.ToVector3();
                    }
                    else if (addCharacterEntityMessage.size != null)
                    {
                        isSize = true;
                        scaleSize = addCharacterEntityMessage.size.ToVector3();
                    }
                    else
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Invalid " +
                            "createcharacterentity message.");
                    }
                    if (addCharacterEntityMessage.clientID == currentClientID.ToString())
                    {
                        return;
                    }
                    if (addCharacterEntityMessage.id != null &&
                        StraightFour.StraightFour.ActiveWorld.entityManager.Exists(Guid.Parse(addCharacterEntityMessage.id)))
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Entity " +
                            "already exists.");
                        return;
                    }
                    if (addCharacterEntityMessage.path == null)
                    {
                        StraightFour.StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(parent,
                            null, Vector3.zero, Quaternion.identity, Vector3.zero,
                            ToOffsetPosition(addCharacterEntityMessage.position.ToVector3()),
                        addCharacterEntityMessage.rotation.ToQuaternion(), scaleSize,
                        Guid.Parse(addCharacterEntityMessage.id), addCharacterEntityMessage.tag, isSize);
                    }
                    else
                    {
                        WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsCharacterEntity(addCharacterEntityMessage.path,
                            addCharacterEntityMessage.resources, addCharacterEntityMessage.modelOffset.ToVector3(),
                            addCharacterEntityMessage.modelRotation.ToQuaternion(), addCharacterEntityMessage.labelOffset.ToVector3(),
                            Guid.Parse(addCharacterEntityMessage.id));
                    }
                    BaseEntity ce = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addCharacterEntityMessage.id));
                    if (ce == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        ce.SetInteractionState(BaseEntity.InteractionState.Static);
                        ce.SetVisibility(true, false);
                        ce.SetPosition(ToOffsetPosition(addCharacterEntityMessage.position.ToVector3()), false, false);
                        ce.SetRotation(addCharacterEntityMessage.rotation.ToQuaternion(), false, false);
                        ce.SetPhysicalProperties(new BaseEntity.EntityPhysicalProperties()
                        {
                            gravitational = false,
                            centerOfMass = Vector3.zero,
                            angularDrag = 0,
                            drag = 0,
                            mass = 0
                        });
                        if (isSize)
                        {
                            ce.SetSize(scaleSize, false);
                        }
                        else
                        {
                            ce.SetScale(scaleSize, false);
                        }
                        ce.entityTag = addCharacterEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createmeshentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddMeshEntityMessage
                        addMeshEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddMeshEntityMessage>(message);
                    bool isSize = false;
                    Vector3 scaleSize = Vector3.zero;
                    if (addMeshEntityMessage.scale != null)
                    {
                        isSize = false;
                        scaleSize = addMeshEntityMessage.scale.ToVector3();
                    }
                    else if (addMeshEntityMessage.size != null)
                    {
                        isSize = true;
                        scaleSize = addMeshEntityMessage.size.ToVector3();
                    }
                    else
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Invalid " +
                            "createmeshentity message.");
                    }
                    WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsMeshEntity(addMeshEntityMessage.path, addMeshEntityMessage.resources, Guid.Parse(addMeshEntityMessage.parentID));
                    BaseEntity me = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addMeshEntityMessage.id));
                    if (me == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        me.SetVisibility(true, false);
                        me.SetPosition(ToOffsetPosition(addMeshEntityMessage.position.ToVector3()), false, false);
                        me.SetRotation(addMeshEntityMessage.rotation.ToQuaternion(), false, false);
                        if (isSize)
                        {
                            me.SetSize(scaleSize, false);
                        }
                        else
                        {
                            me.SetScale(scaleSize, false);
                        }
                        me.entityTag = addMeshEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createbuttonentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddButtonEntityMessage
                        addButtonEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddButtonEntityMessage>(message);

                    CanvasEntity parent = null;
                    if (!string.IsNullOrEmpty(addButtonEntityMessage.parentID))
                    {
                        BaseEntity foundEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addButtonEntityMessage.parentID));
                        if (foundEntity is CanvasEntity)
                        {
                            parent = (CanvasEntity) foundEntity;
                        }
                        else
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Parent is not a canvas entity.");
                        }
                    }

                    Action onClickAction = null;
                    if (addButtonEntityMessage.onClick != null)
                    {
                        onClickAction = new Action(() =>
                        {
                            WebVerseRuntime.Instance.javascriptHandler.Run(addButtonEntityMessage.onClick);
                        });
                    }

                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadButtonEntity(parent,
                        addButtonEntityMessage.positionPercent.ToVector2(), addButtonEntityMessage.sizePercent.ToVector2(),
                        onClickAction, Guid.Parse(addButtonEntityMessage.id), null);
                    BaseEntity be = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addButtonEntityMessage.id));
                    if (be == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        be.SetVisibility(true, false);
                        be.entityTag = addButtonEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createcanvasentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddCanvasEntityMessage
                        addCanvasEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddCanvasEntityMessage>(message);

                    BaseEntity parent = null;
                    if (!string.IsNullOrEmpty(addCanvasEntityMessage.parentID))
                    {
                        parent = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addCanvasEntityMessage.parentID));
                    }

                    bool isSize = false;
                    Vector3 scaleSize = Vector3.zero;
                    if (addCanvasEntityMessage.scale != null)
                    {
                        isSize = false;
                        scaleSize = addCanvasEntityMessage.scale.ToVector3();
                    }
                    else if (addCanvasEntityMessage.size != null)
                    {
                        isSize = true;
                        scaleSize = addCanvasEntityMessage.size.ToVector3();
                    }
                    else
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Invalid " +
                            "createcanvasentity message.");
                    }

                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadCanvasEntity(parent,
                        ToOffsetPosition(addCanvasEntityMessage.position.ToVector3()),
                        addCanvasEntityMessage.rotation.ToQuaternion(), scaleSize,
                        Guid.Parse(addCanvasEntityMessage.id), isSize, null);
                    BaseEntity ce = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                        Guid.Parse(addCanvasEntityMessage.id));
                    if (ce == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        ce.SetVisibility(true, false);
                        ce.entityTag = addCanvasEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createinputentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddInputEntityMessage
                        addInputEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddInputEntityMessage>(message);

                    CanvasEntity parent = null;
                    if (!string.IsNullOrEmpty(addInputEntityMessage.parentID))
                    {
                        BaseEntity foundEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addInputEntityMessage.parentID));
                        if (foundEntity is CanvasEntity)
                        {
                            parent = (CanvasEntity) foundEntity;
                        }
                        else
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Parent is not a canvas entity.");
                        }
                    }

                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadInputEntity(parent,
                        addInputEntityMessage.positionPercent.ToVector2(), addInputEntityMessage.sizePercent.ToVector2(),
                        Guid.Parse(addInputEntityMessage.id), null);
                    BaseEntity be = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addInputEntityMessage.id));
                    if (be == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        be.SetVisibility(true, false);
                        be.entityTag = addInputEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createlightentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddLightEntityMessage
                        addLightEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddLightEntityMessage>(message);

                    BaseEntity parentEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addLightEntityMessage.parentID));

                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadLightEntity(parentEntity,
                        ToOffsetPosition(addLightEntityMessage.position.ToVector3()),
                        addLightEntityMessage.rotation.ToQuaternion(), null);
                    BaseEntity me = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                        Guid.Parse(addLightEntityMessage.id));
                    if (me == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        me.SetVisibility(true, false);
                        me.entityTag = addLightEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createterrainentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddTerrainEntityMessage
                        addTerrainEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddTerrainEntityMessage>(message);

                    BaseEntity parentEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addTerrainEntityMessage.parentID));

                    if (addTerrainEntityMessage.type == "heightmap")
                    {
                        List<TerrainEntityLayer> layers = new List<TerrainEntityLayer>();
                        if (addTerrainEntityMessage.normalTextures != null && addTerrainEntityMessage.maskTextures != null &&
                                addTerrainEntityMessage.specularValues != null && addTerrainEntityMessage.metallicValues != null &&
                                addTerrainEntityMessage.smoothnessValues != null)
                        {
                            int diffArrayLength = addTerrainEntityMessage.diffuseTextures.Length;
                            if (addTerrainEntityMessage.normalTextures.Length == diffArrayLength &&
                                addTerrainEntityMessage.maskTextures.Length == diffArrayLength &&
                                addTerrainEntityMessage.specularValues.Length == diffArrayLength &&
                                addTerrainEntityMessage.metallicValues.Length == diffArrayLength &&
                                addTerrainEntityMessage.smoothnessValues.Length == diffArrayLength)
                            {
                                for (int idx = 0; idx < diffArrayLength; idx++)
                                {
                                    Color spec = new Color(127, 127, 127, 127);
                                    ColorUtility.TryParseHtmlString(addTerrainEntityMessage.specularValues[idx], out spec);
                                    layers.Add(new TerrainEntityLayer()
                                    {
                                        diffusePath = addTerrainEntityMessage.diffuseTextures[idx],
                                        normalPath = addTerrainEntityMessage.maskTextures[idx],
                                        maskPath = addTerrainEntityMessage.maskTextures[idx],
                                        specular = spec,
                                        metallic = addTerrainEntityMessage.metallicValues[idx],
                                        smoothness = addTerrainEntityMessage.smoothnessValues[idx]
                                    });
                                }
                            }
                            else
                            {
                                LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Received terrain entity layer arrays of unequal length.");
                            }
                        }

                        StraightFour.StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(addTerrainEntityMessage.length,
                            addTerrainEntityMessage.width, addTerrainEntityMessage.height, addTerrainEntityMessage.heights,
                            layers.ToArray(), Handlers.VEML.VEMLUtilities.ParseCSVLayerMasksToInternalFormat(addTerrainEntityMessage.layerMask),
                            parentEntity, ToOffsetPosition(addTerrainEntityMessage.position.ToVector3()),
                            addTerrainEntityMessage.rotation.ToQuaternion(), false, Guid.Parse(addTerrainEntityMessage.id),
                            addTerrainEntityMessage.tag, null);
                    }
                    else if (addTerrainEntityMessage.type == "voxel")
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Voxel terrain entities not yet supported.");
                    }
                    else if (addTerrainEntityMessage.type == "hybrid")
                    {
                        List<Handlers.Javascript.APIs.Entity.TerrainEntityLayer> layers
                            = new List<Handlers.Javascript.APIs.Entity.TerrainEntityLayer>();
                        if (addTerrainEntityMessage.normalTextures != null && addTerrainEntityMessage.maskTextures != null &&
                                addTerrainEntityMessage.specularValues != null && addTerrainEntityMessage.metallicValues != null &&
                                addTerrainEntityMessage.smoothnessValues != null)
                        {
                            int diffArrayLength = addTerrainEntityMessage.diffuseTextures.Length;
                            if (addTerrainEntityMessage.normalTextures.Length == diffArrayLength &&
                                addTerrainEntityMessage.maskTextures.Length == diffArrayLength &&
                                addTerrainEntityMessage.specularValues.Length == diffArrayLength &&
                                addTerrainEntityMessage.metallicValues.Length == diffArrayLength &&
                                addTerrainEntityMessage.smoothnessValues.Length == diffArrayLength)
                            {
                                for (int idx = 0; idx < diffArrayLength; idx++)
                                {
                                    Color spec = new Color(127, 127, 127, 127);
                                    ColorUtility.TryParseHtmlString(addTerrainEntityMessage.specularValues[idx], out spec);
                                    layers.Add(new Handlers.Javascript.APIs.Entity.TerrainEntityLayer()
                                    {
                                        diffuseTexture = addTerrainEntityMessage.diffuseTextures[idx],
                                        normalTexture = addTerrainEntityMessage.maskTextures[idx],
                                        maskTexture = addTerrainEntityMessage.maskTextures[idx],
                                        specular = new Handlers.Javascript.APIs.WorldTypes.Color(spec.r, spec.g, spec.b, spec.a),
                                        metallic = addTerrainEntityMessage.metallicValues[idx],
                                        smoothness = addTerrainEntityMessage.smoothnessValues[idx]
                                    });
                                }
                            }
                            else
                            {
                                LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Received terrain entity layer arrays of unequal length.");
                            }
                        }

                        List<Handlers.Javascript.APIs.Entity.TerrainEntityModification> formattedMods
                            = new List<Handlers.Javascript.APIs.Entity.TerrainEntityModification>();
                        foreach (VOSSynchronizationMessages.TerrainModification mod in addTerrainEntityMessage.modifications)
                        {
                            Handlers.Javascript.APIs.Entity.TerrainEntityBrushType bt
                                = Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.sphere;
                            if (mod.brushType == "sphere")
                            {
                                bt = Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.sphere;
                            }
                            else if (mod.brushType == "rounded-cube")
                            {
                                bt = Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.roundedCube;
                            }
                            else
                            {
                                LogSystem.LogError("[VOSSynchronizer->OnMessage] Invalid Terrain Modification brush type.");
                            }

                            Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation op
                                = Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Unset;
                            if (mod.modification == "build")
                            {
                                op = Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Build;
                            }
                            else if (mod.modification == "dig")
                            {
                                op = Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Dig;
                            }
                            formattedMods.Add(new Handlers.Javascript.APIs.Entity.TerrainEntityModification(op,
                                new Handlers.Javascript.APIs.WorldTypes.Vector3(mod.position.x, mod.position.y, mod.position.z),
                                bt, mod.layer, mod.size));
                        }

                        Handlers.Javascript.APIs.Entity.EntityAPIHelper.LoadHybridTerrainEntityAsync(
                            Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity),
                            addTerrainEntityMessage.length, addTerrainEntityMessage.width, addTerrainEntityMessage.height,
                            addTerrainEntityMessage.heights, layers.ToArray(),
                            Handlers.VEML.VEMLUtilities.ParseCSVLayerMasks(addTerrainEntityMessage.layerMask),
                            formattedMods.ToArray(), ToOffsetPosition(addTerrainEntityMessage.position.ToAPIVector3()),
                            new Handlers.Javascript.APIs.WorldTypes.Quaternion(addTerrainEntityMessage.rotation.x,
                                addTerrainEntityMessage.rotation.y, addTerrainEntityMessage.rotation.z, addTerrainEntityMessage.rotation.w),
                                false, addTerrainEntityMessage.id, addTerrainEntityMessage.tag, null);
                    }
                    else
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Invalid createterrainentity message type.");
                    }

                    BaseEntity me = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addTerrainEntityMessage.id));
                    if (me == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        me.SetVisibility(true, false);
                        me.entityTag = addTerrainEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createtextentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddTextEntityMessage
                        addTextEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddTextEntityMessage>(message);

                    CanvasEntity parent = null;
                    if (!string.IsNullOrEmpty(addTextEntityMessage.parentID))
                    {
                        BaseEntity foundEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addTextEntityMessage.parentID));
                        if (foundEntity is CanvasEntity)
                        {
                            parent = (CanvasEntity) foundEntity;
                        }
                        else
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Parent is not a canvas entity.");
                        }
                    }

                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadTextEntity(addTextEntityMessage.text,
                        addTextEntityMessage.fontSize, parent,
                        addTextEntityMessage.positionPercent.ToVector2(), addTextEntityMessage.sizePercent.ToVector2(),
                        Guid.Parse(addTextEntityMessage.id), null);
                    BaseEntity be = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addTextEntityMessage.id));
                    if (be == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        be.SetVisibility(true, false);
                        be.entityTag = addTextEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createvoxelentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddVoxelEntityMessage
                        addVoxelEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddVoxelEntityMessage>(message);

                    BaseEntity parentEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addVoxelEntityMessage.parentID));

                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(parentEntity,
                        ToOffsetPosition(addVoxelEntityMessage.position.ToVector3()),
                        addVoxelEntityMessage.rotation.ToQuaternion(), addVoxelEntityMessage.scale.ToVector3(),
                        Guid.Parse(addVoxelEntityMessage.id), null);
                    BaseEntity ve = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addVoxelEntityMessage.id));
                    if (ve == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        ve.SetVisibility(true, false);
                        ve.entityTag = addVoxelEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createairplaneentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddAirplaneEntityMessage
                        addAirplaneEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddAirplaneEntityMessage>(message);

                    bool isSize = false;
                    Vector3 scaleSize = Vector3.zero;
                    if (addAirplaneEntityMessage.scale != null)
                    {
                        isSize = false;
                        scaleSize = addAirplaneEntityMessage.scale.ToVector3();
                    }
                    else if (addAirplaneEntityMessage.size != null)
                    {
                        isSize = true;
                        scaleSize = addAirplaneEntityMessage.size.ToVector3();
                    }
                    else
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Invalid " +
                            "createairplaneentity message.");
                    }

                    WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAirplaneEntity(
                        addAirplaneEntityMessage.path, new string[] { addAirplaneEntityMessage.path },
                        addAirplaneEntityMessage.position.ToVector3(),
                        addAirplaneEntityMessage.rotation.ToQuaternion(), addAirplaneEntityMessage.mass,
                        Guid.Parse(addAirplaneEntityMessage.id), null);

                    BaseEntity ae = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addAirplaneEntityMessage.id));
                    if (ae == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        ae.SetVisibility(true, false);
                        ae.SetPosition(ToOffsetPosition(addAirplaneEntityMessage.position.ToVector3()), false, false);
                        ae.SetRotation(addAirplaneEntityMessage.rotation.ToQuaternion(), false, false);
                        if (isSize)
                        {
                            ae.SetSize(scaleSize, false);
                        }
                        else
                        {
                            ae.SetScale(scaleSize, false);
                        }
                        ae.entityTag = addAirplaneEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createaudioentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddAudioEntityMessage
                        addAudioEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddAudioEntityMessage>(message);

                    BaseEntity parentEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addAudioEntityMessage.parentID));

                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadAudioEntity(parentEntity,
                        ToOffsetPosition(addAudioEntityMessage.position.ToVector3()),
                        addAudioEntityMessage.rotation.ToQuaternion(),
                        Guid.Parse(addAudioEntityMessage.id), null);
                    BaseEntity ae = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addAudioEntityMessage.id));
                    if (ae == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        ae.SetVisibility(true, false);
                        ae.entityTag = addAudioEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createautomobileentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddAutomobileEntityMessage
                        addAutomobileEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddAutomobileEntityMessage>(message);

                    bool isSize = false;
                    Vector3 scaleSize = Vector3.zero;
                    if (addAutomobileEntityMessage.scale != null)
                    {
                        isSize = false;
                        scaleSize = addAutomobileEntityMessage.scale.ToVector3();
                    }
                    else if (addAutomobileEntityMessage.size != null)
                    {
                        isSize = true;
                        scaleSize = addAutomobileEntityMessage.size.ToVector3();
                    }
                    else
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Invalid " +
                            "createautomobileentity message.");
                    }

                    Handlers.Javascript.APIs.Entity.AutomobileEntity.AutomobileType at =
                        Handlers.Javascript.APIs.Entity.AutomobileEntity.AutomobileType.Default;

                    Handlers.Javascript.APIs.Entity.AutomobileEntityWheel[] wheels =
                        FromWheelString(addAutomobileEntityMessage.wheels);

                    WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAutomobileEntity(
                        addAutomobileEntityMessage.path, new string[] { addAutomobileEntityMessage.path },
                        ToOffsetPosition(addAutomobileEntityMessage.position.ToVector3()),
                        addAutomobileEntityMessage.rotation.ToQuaternion(),
                        wheels,
                        addAutomobileEntityMessage.mass,
                        at,
                        Guid.Parse(addAutomobileEntityMessage.id), null);

                    BaseEntity ae = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addAutomobileEntityMessage.id));
                    if (ae == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        ae.SetVisibility(true, false);
                        ae.SetPosition(ToOffsetPosition(addAutomobileEntityMessage.position.ToVector3()), false, false);
                        ae.SetRotation(addAutomobileEntityMessage.rotation.ToQuaternion(), false, false);
                        if (isSize)
                        {
                            ae.SetSize(scaleSize, false);
                        }
                        else
                        {
                            ae.SetScale(scaleSize, false);
                        }
                        ae.entityTag = addAutomobileEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createdropdownentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddDropdownEntityMessage
                        addDropdownEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddDropdownEntityMessage>(message);

                    CanvasEntity parent = null;
                    if (!string.IsNullOrEmpty(addDropdownEntityMessage.parentID))
                    {
                        BaseEntity foundEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addDropdownEntityMessage.parentID));
                        if (foundEntity is CanvasEntity)
                        {
                            parent = (CanvasEntity) foundEntity;
                        }
                        else
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Parent is not a canvas entity.");
                        }
                    }

                    Action<int> onChangeAction = null;
                    if (addDropdownEntityMessage.onChange != null)
                    {
                        onChangeAction = new Action<int>((idx) =>
                        {
                            WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                                addDropdownEntityMessage.onChange, new object[] { idx });
                        });
                    }

                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadDropdownEntity(parent,
                        addDropdownEntityMessage.positionPercent.ToVector2(),
                        addDropdownEntityMessage.sizePercent.ToVector2(),
                        onChangeAction, addDropdownEntityMessage.options.ToList(),
                        Guid.Parse(addDropdownEntityMessage.id), null);
                    BaseEntity de = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addDropdownEntityMessage.id));
                    if (de == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        de.SetVisibility(true, false);
                        de.entityTag = addDropdownEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createhtmlentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddHTMLEntityMessage
                        addHTMLEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddHTMLEntityMessage>(message);

                    bool isSize = false;
                    Vector3 scaleSize = Vector3.zero;
                    if (addHTMLEntityMessage.scale != null)
                    {
                        isSize = false;
                        scaleSize = addHTMLEntityMessage.scale.ToVector3();
                    }
                    else if (addHTMLEntityMessage.size != null)
                    {
                        isSize = true;
                        scaleSize = addHTMLEntityMessage.size.ToVector3();
                    }
                    else
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Invalid " +
                            "createhtmlentity message.");
                    }

                    BaseEntity parentEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addHTMLEntityMessage.parentID));

                    Action<string> onMessageAction = null;
                    if (addHTMLEntityMessage.onMessage != null)
                    {
                        onMessageAction = new Action<string>((msg) =>
                        {
                            WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                                addHTMLEntityMessage.onMessage, new object[] { msg });
                        });
                    }

                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadHTMLEntity(parentEntity,
                        ToOffsetPosition(addHTMLEntityMessage.position.ToVector3()),
                        addHTMLEntityMessage.rotation.ToQuaternion(), addHTMLEntityMessage.scale.ToVector3(),
                        Guid.Parse(addHTMLEntityMessage.id), isSize, null, onMessageAction);
                    BaseEntity he = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addHTMLEntityMessage.id));
                    if (he == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        he.SetVisibility(true, false);
                        he.entityTag = addHTMLEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/createimageentity")
                {
                    VOSSynchronizationMessages.StatusMessages.AddImageEntityMessage
                        addImageEntityMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.AddImageEntityMessage>(message);

                    BaseEntity parentEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(
                            Guid.Parse(addImageEntityMessage.parentID));

                    Handlers.Javascript.APIs.Entity.EntityAPIHelper.LoadImageEntityAsync(
                        Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity),
                        addImageEntityMessage.imageFile,
                        new Handlers.Javascript.APIs.WorldTypes.Vector2(
                            addImageEntityMessage.positionPercent.x, addImageEntityMessage.positionPercent.y),
                        new Handlers.Javascript.APIs.WorldTypes.Vector2(
                            addImageEntityMessage.sizePercent.x, addImageEntityMessage.sizePercent.y),
                        addImageEntityMessage.id, null);
                    BaseEntity ie = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(addImageEntityMessage.id));
                    if (ie == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                    }
                    else
                    {
                        ie.SetVisibility(true, false);
                        ie.entityTag = addImageEntityMessage.tag;
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/state")
                {
                    VOSSynchronizationMessages.SessionMessages.SessionStateMessage
                        sessionStateMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.SessionMessages.SessionStateMessage>(message);
                    currentStateMessage = sessionStateMessage;
                    if (sessionStateMessage.sessionID == currentSessionID.ToString())
                    {
                        if (requestedState)
                        {
                            List<BaseEntity> entitiesToDelete = new List<BaseEntity>();
                            foreach (BaseEntity entity in synchronizedEntities)
                            {
                                entitiesToDelete.Add(entity);
                            }
                            foreach (BaseEntity entity in entitiesToDelete)
                            {
                                RemoveSynchronizedEntity(entity);
                            }

                            synchronizedUsers = new Dictionary<string, string>();
                            if (sessionStateMessage.clients != null)
                            {
                                foreach (VOSSynchronizationMessages.ClientInfo clientInfo in sessionStateMessage.clients)
                                {
                                    synchronizedUsers.Add(clientInfo.id, clientInfo.tag);
                                }
                            }

                            synchronizedEntities = new List<BaseEntity>();
                            if (sessionStateMessage.entities != null)
                            {
                                foreach (VOSSynchronizationMessages.EntityInfo entityInfo in sessionStateMessage.entities)
                                {
                                    BaseEntity newEntity = AddEntity(entityInfo);
                                    synchronizedEntities.Add(newEntity);
                                }
                            }
                            requestedState = false;
                            if (onGotState != null)
                            {
                                onGotState.Invoke();
                                onGotState = null;
                            }
                        }
                    }
                }
                else if (topic == "vos/status/" + currentSessionID.ToString() + "/message/new")
                {
                    VOSSynchronizationMessages.StatusMessages.NewMessageMessage
                        newMessageMessage = JsonConvert.DeserializeObject<
                        VOSSynchronizationMessages.StatusMessages.NewMessageMessage>(message);
                    foreach (Action<string, string, string> action in onMessage)
                    {
                        if (action != null)
                        {
                            action.Invoke(newMessageMessage.topic, newMessageMessage.clientID, newMessageMessage.message);
                        }
                    }
                }
                else if (topic.StartsWith("vos/status/" + currentSessionID.ToString() + "/entity/"))
                {
                    if (topic.EndsWith("/remove"))
                    {
                        VOSSynchronizationMessages.StatusMessages.RemoveEntityMessage
                            removeEntityMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.RemoveEntityMessage>(message);
                    }
                    else if (topic.EndsWith("/delete"))
                    {
                        VOSSynchronizationMessages.StatusMessages.DeleteEntityMessage
                            deleteEntityMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.DeleteEntityMessage>(message);
                        BaseEntity de = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(deleteEntityMessage.id));
                        if (de == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            de.Delete();
                        }
                    }
                    else if (topic.EndsWith("/position"))
                    {
                        VOSSynchronizationMessages.StatusMessages.UpdateEntityPositionMessage
                            updateEntityPositionMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.UpdateEntityPositionMessage>(message);
                        if (updateEntityPositionMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity pe = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(updateEntityPositionMessage.id));
                        if (pe == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            pe.SetPosition(ToOffsetPosition(updateEntityPositionMessage.position.ToVector3()), false, false);
                        }
                    }
                    else if (topic.EndsWith("/rotation"))
                    {
                        VOSSynchronizationMessages.StatusMessages.UpdateEntityRotationMessage
                            updateEntityRotationMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.UpdateEntityRotationMessage>(message);
                        if (updateEntityRotationMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity re = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(updateEntityRotationMessage.id));
                        if (re == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            re.SetRotation(updateEntityRotationMessage.rotation.ToQuaternion(), false, false);
                        }
                    }
                    else if (topic.EndsWith("/scale"))
                    {
                        VOSSynchronizationMessages.StatusMessages.UpdateEntityScaleMessage
                            updateEntityScaleMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.UpdateEntityScaleMessage>(message);
                        if (updateEntityScaleMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity se = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(updateEntityScaleMessage.id));
                        if (se == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            se.SetScale(updateEntityScaleMessage.scale.ToVector3(), false);
                        }
                    }
                    else if (topic.EndsWith("/size"))
                    {
                        VOSSynchronizationMessages.StatusMessages.UpdateEntitySizeMessage
                            updateEntitySizeMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.UpdateEntitySizeMessage>(message);
                        if (updateEntitySizeMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity se = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(updateEntitySizeMessage.id));
                        if (se == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            se.SetSize(updateEntitySizeMessage.size.ToVector3(), false);
                        }
                    }
                    else if (topic.EndsWith("/terrain-mod"))
                    {
                        VOSSynchronizationMessages.StatusMessages.ModifyTerrainEntityMessage
                            modifyTerrainEntityMessage = JsonConvert.DeserializeObject<
                                VOSSynchronizationMessages.StatusMessages.ModifyTerrainEntityMessage>(message);
                        if (modifyTerrainEntityMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity te = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(modifyTerrainEntityMessage.id));
                        if (te == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            if (te is not HybridTerrainEntity)
                            {
                                LogSystem.LogError("[VOSSynchronizer->OnMessage] Terrain Modification only valid on HybridTerrainEntity.");
                                return;
                            }

                            TerrainEntityBrushType bt = TerrainEntityBrushType.sphere;
                            if (modifyTerrainEntityMessage.brushType == "sphere")
                            {
                                bt = TerrainEntityBrushType.sphere;
                            }
                            else if (modifyTerrainEntityMessage.brushType == "rounded-cube")
                            {
                                bt = TerrainEntityBrushType.roundedCube;
                            }
                            else
                            {
                                LogSystem.LogError("[VOSSynchronizer->OnMessage] Invalid Terrain Modification brush type.");
                            }

                            if (modifyTerrainEntityMessage.modification == "build")
                            {
                                ((HybridTerrainEntity) te).Build(modifyTerrainEntityMessage.position.ToVector3(), bt,
                                    modifyTerrainEntityMessage.layer, modifyTerrainEntityMessage.size, false);
                            }
                            else if (modifyTerrainEntityMessage.modification == "dig")
                            {
                                ((HybridTerrainEntity) te).Dig(modifyTerrainEntityMessage.position.ToVector3(), bt,
                                    modifyTerrainEntityMessage.layer, modifyTerrainEntityMessage.size, false);
                            }
                        }
                    }
                    else if (topic.EndsWith("/canvastype"))
                    {
                        VOSSynchronizationMessages.StatusMessages.SetCanvasTypeMessage
                            setEntityCanvasTypeMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.SetCanvasTypeMessage>(message);
                        if (setEntityCanvasTypeMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity ce = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(setEntityCanvasTypeMessage.id));
                        if (ce == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            if (ce is CanvasEntity)
                            {
                                if (setEntityCanvasTypeMessage.type.ToLower() == "world")
                                {
                                    ((CanvasEntity) ce).MakeWorldCanvas();
                                }
                                else if (setEntityCanvasTypeMessage.type.ToLower() == "screen")
                                {
                                    ((CanvasEntity) ce).MakeScreenCanvas();
                                }
                                else
                                {
                                    LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Unknown canvas type.");
                                }
                            }
                            else
                            {
                                LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Expected a canvas entity but entity is of different type.");
                            }
                        }
                    }
                    else if (topic.EndsWith("/highlight"))
                    {
                        VOSSynchronizationMessages.StatusMessages.SetHighlightStateMessage
                            setEntityHighlightStateMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.SetHighlightStateMessage>(message);
                        if (setEntityHighlightStateMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity he = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(setEntityHighlightStateMessage.id));
                        if (he == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            he.SetHighlight(setEntityHighlightStateMessage.highlighted);
                        }
                    }
                    else if (topic.EndsWith("/motion"))
                    {
                        VOSSynchronizationMessages.StatusMessages.SetMotionMessage
                            setEntityMotionMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.SetMotionMessage>(message);
                        if (setEntityMotionMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity me = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(setEntityMotionMessage.id));
                        if (me == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            BaseEntity.EntityMotion motionToSet = new BaseEntity.EntityMotion()
                            {
                                angularVelocity = setEntityMotionMessage.angularVelocity.ToVector3(),
                                stationary = setEntityMotionMessage.stationary,
                                velocity = setEntityMotionMessage.velocity.ToVector3()
                            };
                            me.SetMotion(motionToSet);
                        }
                    }
                    else if (topic.EndsWith("/parent"))
                    {
                        VOSSynchronizationMessages.StatusMessages.SetParentMessage
                            setEntityParentMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.SetParentMessage>(message);
                        if (setEntityParentMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity ce = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(setEntityParentMessage.id));
                        BaseEntity pe = setEntityParentMessage.parentID == null ? null :
                            StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(setEntityParentMessage.parentID));
                        if (ce == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            ce.SetParent(pe);
                        }
                    }
                    else if (topic.EndsWith("/physicalproperties"))
                    {
                        VOSSynchronizationMessages.StatusMessages.SetPhysicalPropertiesMessage
                            setEntityPhysicalPropertiesMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.SetPhysicalPropertiesMessage>(message);
                        if (setEntityPhysicalPropertiesMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity pe
                            = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(setEntityPhysicalPropertiesMessage.id));
                        if (pe == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            BaseEntity.EntityPhysicalProperties physicalPropertiesToSet = new BaseEntity.EntityPhysicalProperties()
                            {
                                angularDrag = setEntityPhysicalPropertiesMessage.angularDrag,
                                centerOfMass = setEntityPhysicalPropertiesMessage.centerOfMass.ToVector3(),
                                drag = setEntityPhysicalPropertiesMessage.drag,
                                gravitational = setEntityPhysicalPropertiesMessage.gravitational,
                                mass = setEntityPhysicalPropertiesMessage.mass
                            };
                            pe.SetPhysicalProperties(physicalPropertiesToSet);
                        }
                    }
                    else if (topic.EndsWith("/visibility"))
                    {
                        VOSSynchronizationMessages.StatusMessages.SetVisibilityMessage
                            setEntityVisibilityMessage = JsonConvert.DeserializeObject<
                            VOSSynchronizationMessages.StatusMessages.SetVisibilityMessage>(message);
                        if (setEntityVisibilityMessage.clientID == currentClientID.ToString())
                        {
                            return;
                        }
                        BaseEntity ve = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(setEntityVisibilityMessage.id));
                        if (ve == null)
                        {
                            LogSystem.LogWarning("[VOSSynchronizer->OnMessage] Could not find entity.");
                        }
                        else
                        {
                            ve.SetVisibility(setEntityVisibilityMessage.visible, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add an entity locally that is being synchronized.
        /// </summary>
        /// <param name="entityInfo">Information about the entity to add.</param>
        /// <returns>The entity that has been added.</returns>
        private BaseEntity AddEntity(VOSSynchronizationMessages.EntityInfo entityInfo)
        {
            bool isSize = false;
            Vector3 scaleSize = Vector3.one;
            if (entityInfo.scale != null)
            {
                scaleSize = new Vector3(entityInfo.scale.x, entityInfo.scale.y, entityInfo.scale.z);
            }
            else if (entityInfo.size != null)
            {
                scaleSize = new Vector3(entityInfo.size.x, entityInfo.size.y, entityInfo.size.z);
                isSize = true;
            }

            Guid newEntityID;
            BaseEntity ne;
            BaseEntity parentEntity = null;
            if (entityInfo.parentID != null)
            {
                StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.parentID));
            }

            CanvasEntity parentCE = null;
            if (parentEntity is CanvasEntity)
            {
                parentCE = (CanvasEntity) parentEntity;
            }

            switch (entityInfo.type)
            {
                case "mesh":
                    WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsMeshEntity(entityInfo.path, entityInfo.resources,
                        Guid.Parse(entityInfo.parentID));
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetParent(parentEntity);
                        ne.SetVisibility(true, false);
                        ne.SetPosition(ToOffsetPosition(entityInfo.position.ToVector3()), false, false);
                        ne.SetRotation(entityInfo.rotation.ToQuaternion(), false, false);
                        if (isSize)
                        {
                            ne.SetSize(scaleSize, false);
                        }
                        else
                        {
                            ne.SetScale(scaleSize, false);
                        }
                        ne.entityTag = entityInfo.tag;
                    }
                    return ne;

                case "character":
                    if (entityInfo.path == null)
                    {
                        newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(parentEntity,
                            null, Vector3.zero, Quaternion.identity, Vector3.zero,
                            ToOffsetPosition(new Vector3(entityInfo.position.x, entityInfo.position.y, entityInfo.position.z)),
                            new Quaternion(entityInfo.rotation.x, entityInfo.rotation.y,
                            entityInfo.rotation.z, entityInfo.rotation.w), scaleSize, Guid.Parse(entityInfo.id),
                            entityInfo.tag, isSize, null);
                    }
                    else
                    {
                        WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsCharacterEntity(entityInfo.path,
                            entityInfo.resources, entityInfo.modelOffset == null ? Vector3.zero : entityInfo.modelOffset.ToVector3(),
                            entityInfo.modelRotation == null ? Quaternion.identity : entityInfo.modelRotation.ToQuaternion(),
                            entityInfo.labelOffset == null ? Vector3.zero : entityInfo.labelOffset.ToVector3(),
                            Guid.Parse(entityInfo.id));
                    }

                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetParent(parentEntity);
                        ne.SetInteractionState(BaseEntity.InteractionState.Static);
                        ne.SetVisibility(true, false);
                        ne.SetPosition(ToOffsetPosition(entityInfo.position.ToVector3()), false, false);
                        ne.SetRotation(entityInfo.rotation.ToQuaternion(), false, false);
                        if (isSize)
                        {
                            ne.SetSize(scaleSize, false);
                        }
                        else
                        {
                            ne.SetScale(scaleSize, false);
                        }
                        ne.entityTag = entityInfo.tag;
                    }
                    return ne;

                case "container":
                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadContainerEntity(parentEntity,
                        ToOffsetPosition(new Vector3(entityInfo.position.x, entityInfo.position.y, entityInfo.position.z)),
                        new Quaternion(entityInfo.rotation.x, entityInfo.rotation.y,
                        entityInfo.rotation.z, entityInfo.rotation.w), scaleSize, Guid.Parse(entityInfo.id));
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                        ne.entityTag = entityInfo.tag;
                    }
                    return ne;

                case "button":
                    Action onClickAction = null;
                    if (entityInfo.onClick != null)
                    {
                        onClickAction = new Action(() =>
                        {
                            WebVerseRuntime.Instance.javascriptHandler.Run(entityInfo.onClick);
                        });
                    }

                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadButtonEntity(parentCE,
                        entityInfo.positionPercent.ToVector2(), entityInfo.sizePercent.ToVector2(), onClickAction,
                        Guid.Parse(entityInfo.id), null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                    }
                    return ne;

                case "canvas":
                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadCanvasEntity(parentEntity,
                        ToOffsetPosition(entityInfo.position.ToVector3()), entityInfo.rotation.ToQuaternion(),
                        scaleSize, Guid.Parse(entityInfo.id), isSize, null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                    }
                    return ne;

                case "input":
                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadInputEntity(parentCE,
                        entityInfo.positionPercent.ToVector2(), entityInfo.sizePercent.ToVector2(),
                        Guid.Parse(entityInfo.id), null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                    }
                    return ne;

                case "light":
                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadLightEntity(parentEntity,
                        ToOffsetPosition(entityInfo.position.ToVector3()), entityInfo.rotation.ToQuaternion(),
                        Guid.Parse(entityInfo.id), null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                    }
                    return ne;

                case "terrain":
                    ne = null;
                    if (entityInfo.subType == "heightmap")
                    {
                        List<StraightFour.Entity.Terrain.TerrainEntityLayer> layers
                            = new List<StraightFour.Entity.Terrain.TerrainEntityLayer>();
                        if (entityInfo.normalTextures != null && entityInfo.maskTextures != null &&
                                entityInfo.specularValues != null && entityInfo.metallicValues != null &&
                                entityInfo.smoothnessValues != null)
                        {
                            int diffArrayLength = entityInfo.diffuseTextures.Length;
                            if (entityInfo.normalTextures.Length == diffArrayLength &&
                                entityInfo.maskTextures.Length == diffArrayLength &&
                                entityInfo.specularValues.Length == diffArrayLength &&
                                entityInfo.metallicValues.Length == diffArrayLength &&
                                entityInfo.smoothnessValues.Length == diffArrayLength)
                            {
                                for (int idx = 0; idx < diffArrayLength; idx++)
                                {
                                    Color spec = new Color(127, 127, 127, 127);
                                    ColorUtility.TryParseHtmlString(entityInfo.specularValues[idx], out spec);
                                    layers.Add(new StraightFour.Entity.Terrain.TerrainEntityLayer()
                                    {
                                        diffusePath = entityInfo.diffuseTextures[idx],
                                        normalPath = entityInfo.maskTextures[idx],
                                        maskPath = entityInfo.maskTextures[idx],
                                        specular = spec,
                                        metallic = entityInfo.metallicValues[idx],
                                        smoothness = entityInfo.smoothnessValues[idx]
                                    });
                                }
                            }
                            else
                            {
                                LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Received terrain entity layer arrays of unequal length.");
                            }
                        }

                        newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(entityInfo.length,
                        entityInfo.width, entityInfo.height, entityInfo.heights, layers.ToArray(),
                            Handlers.VEML.VEMLUtilities.ParseCSVLayerMasksToInternalFormat(entityInfo.layerMask), parentEntity,
                            ToOffsetPosition(entityInfo.position.ToVector3()), entityInfo.rotation.ToQuaternion(),
                            false, Guid.Parse(entityInfo.id), null);
                        ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    }
                    else if (entityInfo.subType == "voxel")
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Voxel terrain entities not yet supported.");
                    }
                    else if (entityInfo.subType == "hybrid")
                    {
                        List<Handlers.Javascript.APIs.Entity.TerrainEntityLayer> layers
                            = new List<Handlers.Javascript.APIs.Entity.TerrainEntityLayer>();
                        if (entityInfo.normalTextures != null && entityInfo.maskTextures != null &&
                                entityInfo.specularValues != null && entityInfo.metallicValues != null &&
                                entityInfo.smoothnessValues != null)
                        {
                            int diffArrayLength = entityInfo.diffuseTextures.Length;
                            if (entityInfo.normalTextures.Length == diffArrayLength &&
                                entityInfo.maskTextures.Length == diffArrayLength &&
                                entityInfo.specularValues.Length == diffArrayLength &&
                                entityInfo.metallicValues.Length == diffArrayLength &&
                                entityInfo.smoothnessValues.Length == diffArrayLength)
                            {
                                for (int idx = 0; idx < diffArrayLength; idx++)
                                {
                                    Color spec = new Color(127, 127, 127, 127);
                                    ColorUtility.TryParseHtmlString(entityInfo.specularValues[idx], out spec);
                                    layers.Add(new Handlers.Javascript.APIs.Entity.TerrainEntityLayer()
                                    {
                                        diffuseTexture = entityInfo.diffuseTextures[idx],
                                        normalTexture = entityInfo.maskTextures[idx],
                                        maskTexture = entityInfo.maskTextures[idx],
                                        specular = new Handlers.Javascript.APIs.WorldTypes.Color(spec.r, spec.g, spec.b, spec.a),
                                        metallic = entityInfo.metallicValues[idx],
                                        smoothness = entityInfo.smoothnessValues[idx]
                                    });
                                }
                            }
                            else
                            {
                                LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Received terrain entity layer arrays of unequal length.");
                            }
                        }

                        List<Handlers.Javascript.APIs.Entity.TerrainEntityModification> formattedMods
                            = new List<Handlers.Javascript.APIs.Entity.TerrainEntityModification>();
                        foreach (VOSSynchronizationMessages.TerrainModification mod in entityInfo.modifications)
                        {
                            Handlers.Javascript.APIs.Entity.TerrainEntityBrushType bt
                                = Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.sphere;
                            if (mod.brushType == "sphere")
                            {
                                bt = Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.sphere;
                            }
                            else if (mod.brushType == "rounded-cube")
                            {
                                bt = Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.roundedCube;
                            }
                            else
                            {
                                LogSystem.LogError("[VOSSynchronizer->OnMessage] Invalid Terrain Modification brush type.");
                            }

                            Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation op
                                = Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Unset;
                            if (mod.modification == "build")
                            {
                                op = Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Build;
                            }
                            else if (mod.modification == "dig")
                            {
                                op = Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Dig;
                            }
                            formattedMods.Add(new Handlers.Javascript.APIs.Entity.TerrainEntityModification(op,
                                new Handlers.Javascript.APIs.WorldTypes.Vector3(mod.position.x, mod.position.y, mod.position.z),
                                bt, mod.layer, mod.size));
                        }

                        Handlers.Javascript.APIs.Entity.EntityAPIHelper.LoadHybridTerrainEntityAsync(
                            Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity),
                            entityInfo.length, entityInfo.width, entityInfo.height,
                            entityInfo.heights, layers.ToArray(),
                            Handlers.VEML.VEMLUtilities.ParseCSVLayerMasks(entityInfo.layerMask),
                            formattedMods.ToArray(),
                            ToOffsetPosition(new Handlers.Javascript.APIs.WorldTypes.Vector3(entityInfo.position.x,
                                entityInfo.position.y, entityInfo.position.z)),
                            new Handlers.Javascript.APIs.WorldTypes.Quaternion(entityInfo.rotation.x,
                                entityInfo.rotation.y, entityInfo.rotation.z, entityInfo.rotation.w),
                                false, entityInfo.id, entityInfo.tag, null);
                        ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    }

                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                    }
                    return ne;

                case "text":
                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadTextEntity(entityInfo.text,
                        entityInfo.fontSize, parentCE, entityInfo.positionPercent.ToVector2(),
                        entityInfo.sizePercent.ToVector2(), Guid.Parse(entityInfo.id), null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                    }
                    return ne;

                case "voxel":
                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(parentEntity,
                        ToOffsetPosition(entityInfo.position.ToVector3()), entityInfo.rotation.ToQuaternion(),
                        entityInfo.scale.ToVector3(), Guid.Parse(entityInfo.id), null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                    }
                    return ne;

                case "airplane":
                    newEntityID = Guid.Parse(entityInfo.id);
                    WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAirplaneEntity(
                        entityInfo.path, new string[] { entityInfo.path },
                        ToOffsetPosition(entityInfo.position.ToVector3()), entityInfo.rotation.ToQuaternion(),
                        entityInfo.mass, newEntityID, null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                        ne.SetPosition(ToOffsetPosition(entityInfo.position.ToVector3()), false, false);
                        ne.SetRotation(entityInfo.rotation.ToQuaternion(), false, false);
                        if (isSize)
                        {
                            ne.SetSize(scaleSize, false);
                        }
                        else
                        {
                            ne.SetScale(scaleSize, false);
                        }
                        ne.entityTag = entityInfo.tag;
                    }
                    return ne;

                case "audio":
                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadAudioEntity(
                        parentEntity, ToOffsetPosition(entityInfo.position.ToVector3()),
                        entityInfo.rotation.ToQuaternion(),
                        Guid.Parse(entityInfo.id), null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                        ne.entityTag = entityInfo.tag;
                    }
                    return ne;

                case "automobile":
                    Handlers.Javascript.APIs.Entity.AutomobileEntity.AutomobileType at =
                        Handlers.Javascript.APIs.Entity.AutomobileEntity.AutomobileType.Default;

                    Handlers.Javascript.APIs.Entity.AutomobileEntityWheel[] wheels =
                        FromWheelString(entityInfo.wheels);

                    newEntityID = Guid.Parse(entityInfo.id);
                    WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAutomobileEntity(
                        entityInfo.path, new string[] { entityInfo.path },
                        ToOffsetPosition(entityInfo.position.ToVector3()),
                        entityInfo.rotation.ToQuaternion(), wheels, entityInfo.mass, at,
                        Guid.Parse(entityInfo.id), null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                        ne.SetPosition(ToOffsetPosition(entityInfo.position.ToVector3()), false, false);
                        ne.SetRotation(entityInfo.rotation.ToQuaternion(), false, false);
                        if (isSize)
                        {
                            ne.SetSize(scaleSize, false);
                        }
                        else
                        {
                            ne.SetScale(scaleSize, false);
                        }
                        ne.entityTag = entityInfo.tag;
                    }
                    return ne;

                case "dropdown":
                    Action<int> onChangeAction = null;
                    if (entityInfo.onChange != null)
                    {
                        onChangeAction = new Action<int>((idx) =>
                        {
                            WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                                entityInfo.onChange, new object[] { idx });
                        });
                    }

                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadDropdownEntity(
                        parentCE, entityInfo.positionPercent.ToVector2(),
                        entityInfo.sizePercent.ToVector2(), onChangeAction, entityInfo.options.ToList(),
                        Guid.Parse(entityInfo.id), null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                        ne.entityTag = entityInfo.tag;
                    }
                    return ne;

                case "html":
                    Action<string> onMessageAction = null;
                    if (entityInfo.onMessage != null)
                    {
                        onMessageAction = new Action<string>((msg) =>
                        {
                            WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                                entityInfo.onMessage, new object[] { msg });
                        });
                    }

                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadHTMLEntity(parentEntity,
                        ToOffsetPosition(entityInfo.position.ToVector3()), entityInfo.rotation.ToQuaternion(),
                        entityInfo.scale.ToVector3(), Guid.Parse(entityInfo.id), isSize, null, onMessageAction);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                        ne.entityTag = entityInfo.tag;
                    }
                    return ne;

                case "image":
                    newEntityID = Guid.Parse(entityInfo.id);
                    Handlers.Javascript.APIs.Entity.EntityAPIHelper.LoadImageEntityAsync(
                        Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity),
                        entityInfo.imageFile, new Handlers.Javascript.APIs.WorldTypes.Vector2(
                            entityInfo.positionPercent.x, entityInfo.positionPercent.y),
                        new Handlers.Javascript.APIs.WorldTypes.Vector2(
                            entityInfo.sizePercent.x, entityInfo.sizePercent.y),
                        entityInfo.id, null);

                    newEntityID = StraightFour.StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(parentEntity,
                        ToOffsetPosition(entityInfo.position.ToVector3()), entityInfo.rotation.ToQuaternion(),
                        entityInfo.scale.ToVector3(), Guid.Parse(entityInfo.id), null);
                    ne = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityInfo.id));
                    if (ne == null)
                    {
                        LogSystem.LogWarning("[VOSSynchronizer->AddEntity] Could not find entity.");
                    }
                    else
                    {
                        ne.SetVisibility(true, false);
                        ne.entityTag = entityInfo.tag;
                    }
                    return ne;
            }

            return null;
        }

        /// <summary>
        /// Get offset/local position for a given world/synchronized position.
        /// </summary>
        /// <param name="worldPosition">World/synchronized position.</param>
        /// <returns>Offset/local position for a given world/synchronized position.</returns>
        private Vector3 ToOffsetPosition(Vector3 worldPosition)
        {
            return new Vector3(worldPosition.x - synchronizationOffset.x,
                worldPosition.y - synchronizationOffset.y, worldPosition.z - synchronizationOffset.z);
        }

        /// <summary>
        /// Get world/synchronized position for a given offset/local position.
        /// </summary>
        /// <param name="offsetPosition">Offset position.</param>
        /// <returns>World/synchronized position for a given offset/local position.</returns>
        private Vector3 ToWorldPosition(Vector3 offsetPosition)
        {
            return new Vector3(offsetPosition.x + synchronizationOffset.x,
                offsetPosition.y + synchronizationOffset.y, offsetPosition.z + synchronizationOffset.z);
        }

        /// <summary>
        /// Get offset/local position for a given world/synchronized position.
        /// </summary>
        /// <param name="worldPosition">World/synchronized position.</param>
        /// <returns>Offset/local position for a given world/synchronized position.</returns>
        private Handlers.Javascript.APIs.WorldTypes.Vector3 ToOffsetPosition(
            Handlers.Javascript.APIs.WorldTypes.Vector3 worldPosition)
        {
            return new Handlers.Javascript.APIs.WorldTypes.Vector3(worldPosition.x - synchronizationOffset.x,
                worldPosition.y - synchronizationOffset.y, worldPosition.z - synchronizationOffset.z);
        }

        /// <summary>
        /// Get world/synchronized position for a given offset/local position.
        /// </summary>
        /// <param name="offsetPosition">Offset position.</param>
        /// <returns>World/synchronized position for a given offset/local position.</returns>
        private Handlers.Javascript.APIs.WorldTypes.Vector3 ToWorldPosition(
            Handlers.Javascript.APIs.WorldTypes.Vector3 offsetPosition)
        {
            return new Handlers.Javascript.APIs.WorldTypes.Vector3(offsetPosition.x + synchronizationOffset.x,
                offsetPosition.y + synchronizationOffset.y, offsetPosition.z + synchronizationOffset.z);
        }

        private Handlers.Javascript.APIs.Entity.AutomobileEntityWheel[] FromWheelString(string wheelString)
        {
            List<Handlers.Javascript.APIs.Entity.AutomobileEntityWheel> wheels
                = new List<Handlers.Javascript.APIs.Entity.AutomobileEntityWheel>();
            
            if (!string.IsNullOrEmpty(wheelString))
            {
                string[] wls = wheelString.Split(";");
                foreach (string wl in wls)
                {
                    string[] wlParts = wl.Split(",");
                    if (wlParts.Length != 2)
                    {
                        LogSystem.LogWarning("[] Invalid wheel definition.");
                    }
                    else
                    {
                        wheels.Add(new Handlers.Javascript.APIs.Entity.AutomobileEntityWheel(
                            wlParts[0], int.Parse(wlParts[1])));
                    }
                }
            }

            return wheels.ToArray();
        }

        private string ToWheelString(Dictionary<string, float> wheels)
        {
            string wheelString = "";

            if (wheels != null)
            {
                foreach ( System.Collections.Generic.KeyValuePair<string, float> wheel in wheels)
                {
                    if (!string.IsNullOrEmpty(wheelString))
                    {
                        wheelString = wheelString + ";";
                    }
                    wheelString = wheelString + wheel.Key + "," + wheel.Value;
                }
            }

            return wheelString;
        }

        /// <summary>
        /// Attempt to reconnect to the active synchronizer and session.
        /// </summary>
        private void AttemptReconnection()
        {
            Disconnect();
            string host = mqttClient.host;
            int port = mqttClient.port;
            bool useTLS = mqttClient.useTLS;
            Guid? sessionID = currentSessionID;
            string clientTag = currentClientTag;
            MQTTClient.Transports transport =
#if !UNITY_WEBGL
                (mqttClient.transport == Best.MQTT.SupportedTransports.TCP) ? 
                MQTTClient.Transports.TCP :
#endif
                MQTTClient.Transports.WebSockets;
                mqttClient = null;
            Initialize(host, port, useTLS, transport, synchronizationOffset);
            Connect(new Action(() => {
                if (sessionID.HasValue)
                {
                    JoinSession(sessionID.Value, clientTag);
                }
            }));
        }
    }
}
#endif