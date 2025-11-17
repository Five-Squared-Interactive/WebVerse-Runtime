// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.VOSSynchronization
{
    /// <summary>
    /// Class for JSON-serializable VOS Synchronization Messages.
    /// </summary>
    public class VOSSynchronizationMessages
    {
        /// <summary>
        /// Class for a JSON-serializable Vector2.
        /// </summary>
        public class SerializableVector2
        {
            /// <summary>
            /// X value.
            /// </summary>
            [JsonProperty(PropertyName = "x")]
            public float x;

            /// <summary>
            /// Y value.
            /// </summary>
            [JsonProperty(PropertyName = "y")]
            public float y;

            /// <summary>
            /// Constructor for a JSON-serializable Vector2.
            /// </summary>
            /// <param name="vector2">Unity Vector2 to generate JSON-serializable Vector2 from.</param>
            public SerializableVector2(Vector2 vector2)
            {
                x = vector2.x;
                y = vector2.y;
            }

            /// <summary>
            /// Convert to Unity Vector2.
            /// </summary>
            /// <returns>Unity Vector2 representation of this class.</returns>
            public Vector2 ToVector2()
            {
                return new Vector2()
                {
                    x = x,
                    y = y
                };
            }
        }

        /// <summary>
        /// Class for a JSON-serializable Vector3.
        /// </summary>
        public class SerializableVector3
        {
            /// <summary>
            /// X value.
            /// </summary>
            [JsonProperty(PropertyName = "x")]
            public float x;

            /// <summary>
            /// Y value.
            /// </summary>
            [JsonProperty(PropertyName = "y")]
            public float y;

            /// <summary>
            /// Z value.
            /// </summary>
            [JsonProperty(PropertyName = "z")]
            public float z;

            /// <summary>
            /// Constructor for a JSON-serializable Vector3.
            /// </summary>
            /// <param name="vector3">Unity Vector3 to generate JSON-serializable Vector3 from.</param>
            public SerializableVector3(Vector3 vector3)
            {
                x = vector3.x;
                y = vector3.y;
                z = vector3.z;
            }

            /// <summary>
            /// Convert to Unity Vector3.
            /// </summary>
            /// <returns>Unity Vector3 representation of this class.</returns>
            public Vector3 ToVector3()
            {
                return new Vector3()
                {
                    x = x,
                    y = y,
                    z = z
                };
            }

            /// <summary>
            /// Convert to API Vector3.
            /// </summary>
            /// <returns>API Vector3 representation of this class.</returns>
            public Handlers.Javascript.APIs.WorldTypes.Vector3 ToAPIVector3()
            {
                return new Handlers.Javascript.APIs.WorldTypes.Vector3()
                {
                    x = x,
                    y = y,
                    z = z
                };
            }
        }

        /// <summary>
        /// Class for a JSON-serializable Quaternion.
        /// </summary>
        public class SerializableQuaternion
        {
            /// <summary>
            /// X value.
            /// </summary>
            [JsonProperty(PropertyName = "x")]
            public float x;

            /// <summary>
            /// Y value.
            /// </summary>
            [JsonProperty(PropertyName = "y")]
            public float y;

            /// <summary>
            /// Z value.
            /// </summary>
            [JsonProperty(PropertyName = "z")]
            public float z;

            /// <summary>
            /// W value.
            /// </summary>
            [JsonProperty(PropertyName = "w")]
            public float w;

            /// <summary>
            /// Constructor for a JSON-serializable Quaternion.
            /// </summary>
            /// <param name="quaternion">Unity Quaternion to generate JSON-serializable Quaternion from.</param>
            public SerializableQuaternion(Quaternion quaternion)
            {
                x = quaternion.x;
                y = quaternion.y;
                z = quaternion.z;
                w = quaternion.w;
            }

            /// <summary>
            /// Convert to Unity Quaternion.
            /// </summary>
            /// <returns>Unity Quaternion representation of this class.</returns>
            public Quaternion ToQuaternion()
            {
                return new Quaternion()
                {
                    x = x,
                    y = y,
                    z = z,
                    w = w
                };
            }
        }

        /// <summary>
        /// Class for a JSON-serializable Terrain Modification.
        /// </summary>
        public class TerrainModification
        {
            /// <summary>
            /// Modification to be made to the terrain.
            /// </summary>
            [JsonProperty(PropertyName = "modification")]
            public string modification;

            /// <summary>
            /// Size (Vector3 representation).
            /// </summary>
            [JsonProperty(PropertyName = "position")]
            public SerializableVector3 position;

            /// <summary>
            /// Brush type to be used for the terrain modification.
            /// </summary>
            [JsonProperty(PropertyName = "brush-type")]
            public string brushType;

            /// <summary>
            /// Layer on which modification is to be made to the terrain.
            /// </summary>
            [JsonProperty(PropertyName = "layer")]
            public int layer;

            /// <summary>
            /// Size of the modification.
            /// </summary>
            [JsonProperty(PropertyName = "size")]
            public float size;

            public TerrainModification(string _modification,
                Vector3 _position, string _brushType, int _layer, float _size)
            {
                modification = _modification;
                position = new SerializableVector3(_position);
                brushType = _brushType;
                layer = _layer;
                size = _size;
            }
        }

        /// <summary>
        /// JSON-serializable class for a VOS client's information.
        /// </summary>
        public class ClientInfo
        {
            /// <summary>
            /// ID of the VOS client (string representation of UUID).
            /// </summary>
            [JsonProperty(PropertyName = "id")]
            public string id;

            /// <summary>
            /// Tag for the VOS client.
            /// </summary>
            [JsonProperty(PropertyName = "tag")]
            public string tag;

            /// <summary>
            /// Constructor for JSON-serializable VOS client information.
            /// </summary>
            /// <param name="_id">ID to use.</param>
            /// <param name="_tag">Tag to use.</param>
            public ClientInfo(Guid _id, string _tag)
            {
                id = _id.ToString();
                tag = _tag;
            }
        }

        /// <summary>
        /// Class for JSON-serializable entity information.
        /// </summary>
        public class EntityInfo
        {
            /// <summary>
            /// Entity ID (string representation of UUID).
            /// </summary>
            [JsonProperty(PropertyName = "id")]
            public string id;

            /// <summary>
            /// Entity tag.
            /// </summary>
            [JsonProperty(PropertyName = "tag")]
            public string tag;

            /// <summary>
            /// Entity type.
            /// </summary>
            [JsonProperty(PropertyName = "type")]
            public string type;

            /// <summary>
            /// Entity subtype
            /// </summary>
            [JsonProperty(PropertyName = "subtype")]
            public string subType;

            /// <summary>
            /// Path to entity resource.
            /// </summary>
            [JsonProperty(PropertyName = "path")]
            public string path;

            /// <summary>
            /// Paths to additional entity resources.
            /// </summary>
            [JsonProperty(PropertyName = "resources")]
            public string[] resources;

            /// <summary>
            /// Offset for the character entity model.
            /// </summary>
            [JsonProperty(PropertyName = "model-offset")]
            public SerializableVector3 modelOffset;

            /// <summary>
            /// Rotation for the character entity model.
            /// </summary>
            [JsonProperty(PropertyName = "model-rotation")]
            public SerializableQuaternion modelRotation;

            /// <summary>
            /// Offset for entity label.
            /// </summary>
            [JsonProperty(PropertyName = "label-offset")]
            public SerializableVector3 labelOffset;

            /// <summary>
            /// ID of the entity's parent.
            /// </summary>
            [JsonProperty(PropertyName = "parent-id")]
            public string parentID;

            /// <summary>
            /// Position of the entity.
            /// </summary>
            [JsonProperty(PropertyName = "position")]
            public SerializableVector3 position;

            /// <summary>
            /// Rotation of the entity.
            /// </summary>
            [JsonProperty(PropertyName = "rotation")]
            public SerializableQuaternion rotation;

            /// <summary>
            /// Scale of the entity. One of (scale, size, size-percent) must be set.
            /// </summary>
            [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
            public SerializableVector3 scale;

            /// <summary>
            /// Size of the entity. One of (scale, size, size-percent) must be set.
            /// </summary>
            [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
            public SerializableVector3 size;

            /// <summary>
            /// Position percent (Vector2 representation, each value ranging from 0-1).
            /// </summary>
            [JsonProperty(PropertyName = "position-percent")]
            public SerializableVector2 positionPercent;

            /// <summary>
            /// Size percent (Vector2 representation, each value ranging from 0-1).
            /// </summary>
            [JsonProperty(PropertyName = "size-percent")]
            public SerializableVector2 sizePercent;

            /// <summary>
            /// Script logic to perform when button is clicked. Will attempt to invoke
            /// loaded JavaScript.
            /// </summary>
            [JsonProperty(PropertyName = "on-click")]
            public string onClick;

            /// <summary>
            /// Length of the terrain in meters.
            /// </summary>
            [JsonProperty(PropertyName = "length")]
            public float length;

            /// <summary>
            /// Width of the terrain in meters.
            /// </summary>
            [JsonProperty(PropertyName = "width")]
            public float width;

            /// <summary>
            /// Height of the terrain in meters.
            /// </summary>
            [JsonProperty(PropertyName = "height")]
            public float height;

            /// <summary>
            /// 2D array of height values for the terrain.
            /// </summary>
            [JsonProperty(PropertyName = "heights")]
            public float[,] heights;

            /// <summary>
            /// Text to display in the text field.
            /// </summary>
            [JsonProperty(PropertyName = "text")]
            public string text;

            /// <summary>
            /// Font size for the text in the text field.
            /// </summary>
            [JsonProperty(PropertyName = "font-size")]
            public int fontSize;

            /// <summary>
            /// Array of diffuse textures for the terrain entity's layers.
            /// </summary>
            [JsonProperty(PropertyName = "diffuse-texture")]
            public string[] diffuseTextures;

            /// <summary>
            /// Array of normal textures for the terrain entity's layers.
            /// </summary>
            [JsonProperty(PropertyName = "normal-texture")]
            public string[] normalTextures;

            /// <summary>
            /// Array of mask textures for the terrain entity's layers.
            /// </summary>
            [JsonProperty(PropertyName = "mask-texture")]
            public string[] maskTextures;

            /// <summary>
            /// Array of specular values for the terrain entity's layers
            /// </summary>
            [JsonProperty(PropertyName = "specular-values")]
            public string[] specularValues;

            /// <summary>
            /// Array of metallic values for the terrain entity's layers
            /// </summary>
            [JsonProperty(PropertyName = "metallic-values")]
            public float[] metallicValues;

            /// <summary>
            /// Array of smoothness values for the terrain entity's layers
            /// </summary>
            [JsonProperty(PropertyName = "smoothness-values")]
            public float[] smoothnessValues;

            /// <summary>
            /// The layer mask (VEML CSV-formatted) for the terrain entity.
            /// </summary>
            [JsonProperty(PropertyName = "layer-mask")]
            public string layerMask;

            /// <summary>
            /// Terrain modifications for the terrain entity.
            /// </summary>
            [JsonProperty(PropertyName = "terrain-modification")]
            public TerrainModification[] modifications;

            /// <summary>
            /// Mass for the entity.
            /// </summary>
            [JsonProperty(PropertyName = "mass")]
            public float mass;

            /// <summary>
            /// Automobile entity wheels.
            /// </summary>
            [JsonProperty(PropertyName = "wheels")]
            public string wheels;

            /// <summary>
            /// On change event (takes int param for index of selected row in dropdown).
            /// </summary>
            [JsonProperty(PropertyName = "on-change")]
            public string onChange;

            /// <summary>
            /// Options for the dropdown.
            /// </summary>
            [JsonProperty(PropertyName = "options")]
            public string[] options;

            /// <summary>
            /// On message event (takes string param for message received).
            /// </summary>
            [JsonProperty(PropertyName = "on-message")]
            public string onMessage;

            /// <summary>
            /// The path to the image file.
            /// </summary>
            [JsonProperty("image-file")]
            public string imageFile;

            /// <summary>
            /// Guid representation of the ID.
            /// </summary>
            public Guid uuid
            {
                get
                {
                    return Guid.Parse(id);
                }
            }

            /// <summary>
            /// Constructor for JSON-serializable entity information.
            /// </summary>
            /// <param name="_id">ID of the entity.</param>
            /// <param name="_tag">Tag for the entity.</param>
            /// <param name="_type">Type of the entity.</param>
            /// <param name="_path">Path to a resource for the entity.</param>
            /// <param name="_resources">Paths to resources for the entity.</param>
            /// <param name="_modelOffset">Offset for the entity model.</param>
            /// <param name="_modelRotation">Rotation for the entity model.</param>
            /// <param name="_labelOffset">Offset for the entity label.</param>
            /// <param name="_parentID">ID of the entity's parent.</param>
            /// <param name="_position">Position of the entity.</param>
            /// <param name="_rotation">Rotation of the entity.</param>
            /// <param name="_scaleSize">Scale or size of the entity.</param>
            /// <param name="isSize">Whether or not the scaleSize parameter is a size.</param>
            /// <param name="_positionPercent">Position percent.</param>
            /// <param name="_sizePercent">Size Percent.</param>
            /// <param name="_onClick">Script logic to perform when button is clicked.</param>
            /// <param name="_length">Length of the terrain.</param>
            /// <param name="_width">Width of the terrain.</param>
            /// <param name="_height">Height of the terrain.</param>
            /// <param name="_heights">2D array of height values for the terrain.</param>
            /// <param name="_text">Text to display in the text field.</param>
            /// <param name="_fontSize">Font size for the text in the text field.</param>
            /// <param name="_diffuseTextures">Diffuse textures to use for the terrain.</param>
            /// <param name="_normalTextures">Normal textures to use for the terrain.</param>
            /// <param name="_maskTextures">Mask textures to use for the terrain.</param>
            /// <param name="_specularValues">Specular values to use for the terrain.</param>
            /// <param name="_metallicValues">Metallic values to use for the terrain.</param>
            /// <param name="_smoothnessValues">Smoothness values to use for the terrain.</param>
            /// <param name="_layerMask">Layer mask to use for the terrain.</param>
            /// <param name="_subType">Entity subtype.</param>
            /// <param name="_modifications">Modifications to use for the terrain.</param>
            /// <param name="_mass">Mass.</mass>
            /// <param name="_wheels">Wheels.</param
            /// <param name="_onChange">On change event (takes int param for index of
            /// selected row in dropdown).</param>
            /// <param name="_options">Options for the dropdown.</param>
            /// <param name="_onMessage">On message event (takes string param for message received).</param>
            /// <param name="_imageFile">The path to the image file.</param>
            public EntityInfo(Guid _id, string _tag, string _type, string _path,
                string[] _resources, Vector3 _modelOffset, Quaternion _modelRotation,
                Vector3 _labelOffset, Guid? _parentID, Vector3 _position,
                Quaternion _rotation, Vector3 _scaleSize, bool isSize,
                Vector2 _positionPercent, Vector2 _sizePercent, string _onClick,
                float _length, float _width, float _height, float[,] _heights,
                string _text, int _fontSize, string[] _diffuseTextures,
                string[] _normalTextures, string[] _maskTextures,
                string[] _specularValues, float[] _metallicValues,
                float[] _smoothnessValues, string _layerMask, string _subType,
                Dictionary<Vector3Int, Tuple<StraightFour.Entity.HybridTerrainEntity.TerrainOperation,
                    int, StraightFour.Entity.Terrain.TerrainEntityBrushType, float>> _modifications,
                int _mass, string _wheels, string _onChange, string[] _options, string _onMessage,
                string _imageFile)
            {
                id = _id.ToString();
                tag = _tag;
                type = _type;
                path = _path;
                resources = _resources;
                modelOffset = new SerializableVector3(_modelOffset);
                modelRotation = new SerializableQuaternion(_modelRotation);
                labelOffset = new SerializableVector3(_labelOffset);
                parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                position = new SerializableVector3(_position);
                rotation = new SerializableQuaternion(_rotation);
                if (isSize)
                {
                    size = new SerializableVector3(_scaleSize);
                }
                else
                {
                    scale = new SerializableVector3(_scaleSize);
                }
                positionPercent = new SerializableVector2(_positionPercent);
                sizePercent = new SerializableVector2(_sizePercent);
                onClick = _onClick;
                length = _length;
                width = _width;
                height = _height;
                heights = _heights;
                text = _text;
                fontSize = _fontSize;
                diffuseTextures = _diffuseTextures;
                normalTextures = _normalTextures;
                maskTextures = _maskTextures;
                specularValues = _specularValues;
                metallicValues = _metallicValues;
                smoothnessValues = _smoothnessValues;
                layerMask = _layerMask;
                subType = _subType;
                System.Collections.Generic.List<TerrainModification> mods
                        = new System.Collections.Generic.List<TerrainModification>();
                if (_modifications != null)
                {
                    foreach (System.Collections.Generic.KeyValuePair<
                        Vector3Int, Tuple<StraightFour.Entity.HybridTerrainEntity.TerrainOperation,
                        int, StraightFour.Entity.Terrain.TerrainEntityBrushType, float>> mod
                        in _modifications)
                    {
                        if (mod.Key == null)
                        {
                            continue;
                        }

                        string modName = "";
                        switch (mod.Value.Item1)
                        {
                            case StraightFour.Entity.HybridTerrainEntity.TerrainOperation.Dig:
                                modName = "dig";
                                break;

                            case StraightFour.Entity.HybridTerrainEntity.TerrainOperation.Build:
                                modName = "build";
                                break;

                            case StraightFour.Entity.HybridTerrainEntity.TerrainOperation.Unset:
                            default:
                                modName = "unset";
                                break;
                        }

                        string bt = "";
                        switch (mod.Value.Item3)
                        {
                            case StraightFour.Entity.Terrain.TerrainEntityBrushType.sphere:
                                bt = "sphere";
                                break;

                            case StraightFour.Entity.Terrain.TerrainEntityBrushType.roundedCube:
                            default:
                                bt = "roundedcube";
                                break;
                        }
                        mods.Add(new TerrainModification(modName, mod.Key, bt, mod.Value.Item2, mod.Value.Item4));
                    }
                }
                modifications = mods.ToArray();
                mass = _mass;
                wheels = _wheels;
                onChange = _onChange;
                options = _options;
                onMessage = _onMessage;
                imageFile = _imageFile;
            }
        }

        /// <summary>
        /// Class for VOS Synchronization Session messages.
        /// </summary>
        public class SessionMessages
        {
            /// <summary>
            /// Create Session Message.
            /// </summary>
            public class CreateSessionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session to create
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// Tag of the session to create.
                /// </summary>
                [JsonProperty(PropertyName = "session-tag")]
                public string sessionTag;

                public CreateSessionMessage(Guid _messageID, string _clientID,
                    string _clientToken, Guid _sessionID, string tag)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    sessionTag = tag;
                }
            }

            /// <summary>
            /// Destroy Session Message.
            /// </summary>
            public class DestroySessionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session to destroy
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                public DestroySessionMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                }
            }

            /// <summary>
            /// New Session Message.
            /// </summary>
            public class NewSessionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the session
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// Tag of the session.
                /// </summary>
                [JsonProperty(PropertyName = "session-tag")]
                public string sessionTag;

                public NewSessionMessage(Guid _messageID, Guid _sessionID,
                    string tag)
                {
                    messageID = _messageID.ToString();
                    sessionID = _sessionID.ToString();
                    sessionTag = tag;
                }
            }

            /// <summary>
            /// Closed Session Message.
            /// </summary>
            public class ClosedSessionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                public ClosedSessionMessage(Guid _messageID, Guid _sessionID)
                {
                    messageID = _messageID.ToString();
                    sessionID = _sessionID.ToString();
                }
            }

            /// <summary>
            /// Join Session Message.
            /// </summary>
            public class JoinSessionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the session to join
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// Tag of the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-tag")]
                public string clientTag;

                public JoinSessionMessage(Guid _messageID, Guid sessionID,
                    string clientID, string clientToken, string clientTag)
                {
                    messageID = _messageID.ToString();
                    this.sessionID = sessionID.ToString();
                    this.clientID = clientID;
                    this.clientToken = clientToken;
                    this.clientTag = clientTag;
                }
            }

            /// <summary>
            /// Leave Session Message.
            /// </summary>
            public class LeaveSessionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the session to leave
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                public LeaveSessionMessage(Guid _messageID, Guid _sessionID,
                    string _clientID, string _clientToken)
                {
                    messageID = _messageID.ToString();
                    sessionID = _sessionID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                }
            }

            /// <summary>
            /// New Client Message.
            /// </summary>
            public class NewClientMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// Tag of the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-tag")]
                public string clientTag;

                public NewClientMessage(Guid _messageID, Guid _sessionID,
                    string _clientID, string _clientToken, string _clientTag)
                {
                    messageID = _messageID.ToString();
                    sessionID = _sessionID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    clientTag = _clientTag;
                }
            }

            /// <summary>
            /// Client Left Message.
            /// </summary>
            public class ClientLeftMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                public ClientLeftMessage(Guid _messageID, Guid _sessionID,
                    string _clientID, string _clientToken)
                {
                    messageID = _messageID.ToString();
                    sessionID = _sessionID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                }
            }

            /// <summary>
            /// Client Heartbeat Message.
            /// </summary>
            public class ClientHeartbeatMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                public ClientHeartbeatMessage(Guid _messageID, Guid _sessionID,
                    string _clientID, string _clientToken)
                {
                    messageID = _messageID.ToString();
                    sessionID = _sessionID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                }
            }

            /// <summary>
            /// Get Session State Message.
            /// </summary>
            public class GetSessionStateMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                public GetSessionStateMessage(Guid _messageID, Guid _sessionID,
                    string _clientID, string _clientToken)
                {
                    messageID = _messageID.ToString();
                    sessionID = _sessionID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                }
            }

            /// <summary>
            /// Session State Message.
            /// </summary>
            public class SessionStateMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// Clients in the session.
                /// </summary>
                [JsonProperty(PropertyName = "clients")]
                public ClientInfo[] clients;

                /// <summary>
                /// Entities in the session.
                /// </summary>
                [JsonProperty(PropertyName = "entities")]
                public EntityInfo[] entities;

                public SessionStateMessage(Guid _messageID, Guid _sessionID,
                    ClientInfo[] _clients, EntityInfo[] _entities)
                {
                    messageID = _messageID.ToString();
                    sessionID = _sessionID.ToString();
                    clients = _clients;
                    entities = _entities;
                }
            }
        }

        /// <summary>
        /// Class for VOS Synchronization Request Messages.
        /// </summary>
        public class RequestMessages
        {
            /// <summary>
            /// Add Container Entity Message.
            /// </summary>
            public class AddContainerEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddContainerEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag, Guid? _parentID,
                    Vector3 _position, Quaternion _rotation, Vector3 _scale,
                    bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    scale = new SerializableVector3(_scale);
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Character Entity Message.
            /// </summary>
            public class AddCharacterEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// Path to a character entity model.
                /// </summary>
                [JsonProperty(PropertyName = "path")]
                public string path;

                /// <summary>
                /// Paths to character entity resources
                /// </summary>
                [JsonProperty(PropertyName = "resources")]
                public string[] resources;

                /// <summary>
                /// Offset for the character entity model.
                /// </summary>
                [JsonProperty(PropertyName = "model-offset")]
                public SerializableVector3 modelOffset;

                /// <summary>
                /// Rotation for the character entity model.
                /// </summary>
                [JsonProperty(PropertyName = "model-rotation")]
                public SerializableQuaternion modelRotation;

                /// <summary>
                /// Offset for the character entity label.
                /// </summary>
                [JsonProperty(PropertyName = "label-offset")]
                public SerializableVector3 labelOffset;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddCharacterEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag, Guid? _parentID,
                    string _path, string[] _resources, Vector3 _modelOffset,
                    Quaternion _modelRotation, Vector3 _labelOffset,
                    Vector3 _position, Quaternion _rotation, Vector3 _scaleSize,
                    bool isSize, bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    path = _path;
                    resources = _resources;
                    modelOffset = new SerializableVector3(_modelOffset);
                    modelRotation = new SerializableQuaternion(_modelRotation);
                    labelOffset = new SerializableVector3(_labelOffset);
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Mesh Entity Message.
            /// </summary>
            public class AddMeshEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// Path to a mesh entity model.
                /// </summary>
                [JsonProperty(PropertyName = "path")]
                public string path;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddMeshEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag, string _path,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scaleSize, bool isSize, bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    path = _path;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Button Entity Message.
            /// </summary>
            public class AddButtonEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                /// <summary>
                /// Script logic to perform when button is clicked. Will attempt to invoke
                /// loaded JavaScript.
                /// </summary>
                [JsonProperty(PropertyName = "on-click")]
                public string onClick;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddButtonEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent,
                    Vector2 _sizePercent, string _onClick,
                    bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                    onClick = _onClick;
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Canvas Entity Message.
            /// </summary>
            public class AddCanvasEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddCanvasEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scaleSize, bool isSize, bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Input Entity Message.
            /// </summary>
            public class AddInputEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddInputEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent,
                    Vector2 _sizePercent, bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Light Entity Message.
            /// </summary>
            public class AddLightEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddLightEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Terrain Entity Message.
            /// </summary>
            public class AddTerrainEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Length of the terrain in meters.
                /// </summary>
                [JsonProperty(PropertyName = "length")]
                public float length;

                /// <summary>
                /// Width of the terrain in meters.
                /// </summary>
                [JsonProperty(PropertyName = "width")]
                public float width;

                /// <summary>
                /// Height of the terrain in meters.
                /// </summary>
                [JsonProperty(PropertyName = "height")]
                public float height;

                /// <summary>
                /// 2D array of height values for the terrain.
                /// </summary>
                [JsonProperty(PropertyName = "heights")]
                public float[,] heights;

                /// <summary>
                /// Array of diffuse textures for the terrain entity's layers.
                /// </summary>
                [JsonProperty(PropertyName = "diffuse-texture")]
                public string[] diffuseTextures;

                /// <summary>
                /// Array of normal textures for the terrain entity's layers.
                /// </summary>
                [JsonProperty(PropertyName = "normal-texture")]
                public string[] normalTextures;

                /// <summary>
                /// Array of mask textures for the terrain entity's layers.
                /// </summary>
                [JsonProperty(PropertyName = "mask-texture")]
                public string[] maskTextures;

                /// <summary>
                /// Array of specular values for the terrain entity's layers
                /// </summary>
                [JsonProperty(PropertyName = "specular-values")]
                public string[] specularValues;

                /// <summary>
                /// Array of metallic values for the terrain entity's layers
                /// </summary>
                [JsonProperty(PropertyName = "metallic-values")]
                public float[] metallicValues;

                /// <summary>
                /// Array of smoothness values for the terrain entity's layers
                /// </summary>
                [JsonProperty(PropertyName = "smoothness-values")]
                public float[] smoothnessValues;

                /// <summary>
                /// The layer mask (VEML CSV-formatted) for the terrain entity.
                /// </summary>
                [JsonProperty(PropertyName = "layer-mask")]
                public string layerMask;

                /// <summary>
                /// Type of terrain entity ("heightmap, voxel, or hybrid").
                /// </summary>
                [JsonProperty(PropertyName = "type")]
                public string type;

                /// <summary>
                /// Terrain modifications for the terrain entity.
                /// </summary>
                [JsonProperty(PropertyName = "terrain-modification")]
                public TerrainModification[] modifications;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddTerrainEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    float _length, float _width, float _height, float[,] _heights,
                    StraightFour.Entity.Terrain.TerrainEntityLayer[] layers,
                    string _layerMask, string _type,
                    Dictionary<Vector3Int, Tuple<StraightFour.Entity.HybridTerrainEntity.TerrainOperation,
                        int, StraightFour.Entity.Terrain.TerrainEntityBrushType, float>> _modifications,
                    bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    length = _length;
                    width = _width;
                    height = _height;
                    heights = _heights;
                    if (layers != null)
                    {
                        string[] _diffuseTextures = new string[layers.Length];
                        string[] _normalTextures = new string[layers.Length];
                        string[] _maskTextures = new string[layers.Length];
                        string[] _specularValues = new string[layers.Length];
                        float[] _metallicValues = new float[layers.Length];
                        float[] _smoothnessValues = new float[layers.Length];
                        int idx = 0;
                        foreach (StraightFour.Entity.Terrain.TerrainEntityLayer layer in layers)
                        {
                            _diffuseTextures[idx] = layer.diffusePath;
                            _normalTextures[idx] = layer.normalPath;
                            _maskTextures[idx] = layer.maskPath;
                            _specularValues[idx] = ColorUtility.ToHtmlStringRGBA(layer.specular);
                            _metallicValues[idx] = layer.metallic;
                            _smoothnessValues[idx] = layer.smoothness;
                            idx++;
                        }
                        diffuseTextures = _diffuseTextures;
                        normalTextures = _normalTextures;
                        maskTextures = _maskTextures;
                        specularValues = _specularValues;
                        metallicValues = _metallicValues;
                        smoothnessValues = _smoothnessValues;
                    }
                    layerMask = _layerMask;
                    type = _type;
                    System.Collections.Generic.List<TerrainModification> mods
                        = new System.Collections.Generic.List<TerrainModification>();
                    if (_modifications != null)
                    {
                        foreach (System.Collections.Generic.KeyValuePair<
                            Vector3Int, Tuple<StraightFour.Entity.HybridTerrainEntity.TerrainOperation,
                            int, StraightFour.Entity.Terrain.TerrainEntityBrushType, float>> mod
                            in _modifications)
                        {
                            if (mod.Key == null)
                            {
                                continue;
                            }

                            string modName = "";
                            switch (mod.Value.Item1)
                            {
                                case StraightFour.Entity.HybridTerrainEntity.TerrainOperation.Dig:
                                    modName = "dig";
                                    break;

                                case StraightFour.Entity.HybridTerrainEntity.TerrainOperation.Build:
                                    modName = "build";
                                    break;

                                case StraightFour.Entity.HybridTerrainEntity.TerrainOperation.Unset:
                                default:
                                    modName = "unset";
                                    break;
                            }

                            string bt = "";
                            switch (mod.Value.Item3)
                            {
                                case StraightFour.Entity.Terrain.TerrainEntityBrushType.sphere:
                                    bt = "sphere";
                                    break;

                                case StraightFour.Entity.Terrain.TerrainEntityBrushType.roundedCube:
                                default:
                                    bt = "roundedcube";
                                    break;
                            }
                            mods.Add(new TerrainModification(modName, mod.Key, bt, mod.Value.Item2, mod.Value.Item4));
                        }
                    }
                    modifications = mods.ToArray();
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Text Entity Message.
            /// </summary>
            public class AddTextEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                /// <summary>
                /// Text to display in the text field.
                /// </summary>
                [JsonProperty(PropertyName = "text")]
                public string text;

                /// <summary>
                /// Font size for the text in the text field.
                /// </summary>
                [JsonProperty(PropertyName = "font-size")]
                public int fontSize;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddTextEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent,
                    Vector2 _sizePercent, string _text, int _fontSize,
                    bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                    text = _text;
                    fontSize = _fontSize;
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Voxel Entity Message.
            /// </summary>
            public class AddVoxelEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "scale")]
                public SerializableVector3 scale;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddVoxelEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scale, bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    scale = new SerializableVector3(_scale);
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Airplane Entity Message.
            /// </summary>
            public class AddAirplaneEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// Path to a mesh entity model.
                /// </summary>
                [JsonProperty(PropertyName = "path")]
                public string path;

                /// <summary>
                /// Mesh position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "mesh-position")]
                public SerializableVector3 meshPosition;

                /// <summary>
                /// Mesh rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "mesh-rotation")]
                public SerializableQuaternion meshRotation;

                /// <summary>
                /// Mass.
                /// </summary>
                [JsonProperty(PropertyName = "mass")]
                public float mass;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddAirplaneEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scaleSize, bool isSize, string _path, Vector3 _meshPosition,
                    Quaternion _meshRotation, float _mass, bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                    path = _path;
                    meshPosition = new SerializableVector3(_meshPosition);
                    meshRotation = new SerializableQuaternion(_meshRotation);
                    mass = _mass;
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Voxel Entity Message.
            /// </summary>
            public class AddAudioEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddAudioEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scale, bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    scale = new SerializableVector3(_scale);
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Automobile Entity Message.
            /// </summary>
            public class AddAutomobileEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// Path to a mesh entity model.
                /// </summary>
                [JsonProperty(PropertyName = "path")]
                public string path;

                /// <summary>
                /// Mesh position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "mesh-position")]
                public SerializableVector3 meshPosition;

                /// <summary>
                /// Mesh rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "mesh-rotation")]
                public SerializableQuaternion meshRotation;

                /// <summary>
                /// Mass.
                /// </summary>
                [JsonProperty(PropertyName = "mass")]
                public float mass;

                /// <summary>
                /// Automobile entity type.
                /// </summary>
                [JsonProperty(PropertyName = "automobile-entity-type")]
                public string automobileEntityType;

                /// <summary>
                /// Automobile entity wheels.
                /// </summary>
                [JsonProperty(PropertyName = "wheels")]
                public string wheels;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddAutomobileEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scaleSize, bool isSize, string _path, Vector3 _meshPosition,
                    Quaternion _meshRotation, float _mass, string _automobileEntityType, string _wheels,
                    bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                    path = _path;
                    meshPosition = new SerializableVector3(_meshPosition);
                    meshRotation = new SerializableQuaternion(_meshRotation);
                    mass = _mass;
                    automobileEntityType = _automobileEntityType;
                    wheels = _wheels;
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Dropdown Entity Message.
            /// </summary>
            public class AddDropdownEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                /// <summary>
                /// On change event (takes int param for index of selected row in dropdown).
                /// </summary>
                [JsonProperty(PropertyName = "on-change")]
                public string onChange;

                /// <summary>
                /// Options for the dropdown.
                /// </summary>
                [JsonProperty(PropertyName = "options")]
                public string[] options;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddDropdownEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent, Vector2 _sizePercent,
                    string _onChange, string[] _options, bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                    onChange = _onChange;
                    options = _options;
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add HTML Entity Message.
            /// </summary>
            public class AddHTMLEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// On message event (takes string param for message received).
                /// </summary>
                [JsonProperty(PropertyName = "on-message")]
                public string onMessage;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddHTMLEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scaleSize, bool isSize, string _onMessage, bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                    onMessage = _onMessage;
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Image Entity Message.
            /// </summary>
            public class AddImageEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                /// <summary>
                /// The path to the image file.
                /// </summary>
                [JsonProperty("image-file")]
                public string imageFile;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddImageEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent, Vector2 _sizePercent, string _imageFile,
                    bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                    imageFile = _imageFile;
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Remove Entity Message.
            /// </summary>
            public class RemoveEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                public RemoveEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                }
            }

            /// <summary>
            /// Delete Entity Message.
            /// </summary>
            public class DeleteEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                public DeleteEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                }
            }

            /// <summary>
            /// Set Canvas Type Message.
            /// </summary>
            public class SetCanvasTypeMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Type (world or screen) of canvas.
                /// </summary>
                [JsonProperty(PropertyName = "canvas-type")]
                public string type;

                public SetCanvasTypeMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _type)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    type = _type;
                }
            }

            /// <summary>
            /// Set Highlight State Message.
            /// </summary>
            public class SetHighlightStateMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Whether or not to turn on highlighting.
                /// </summary>
                [JsonProperty(PropertyName = "highlighted")]
                public bool highlighted;

                public SetHighlightStateMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, bool _highlighted)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    highlighted = _highlighted;
                }
            }

            /// <summary>
            /// Set Interaction State Message.
            /// </summary>
            public class SetInteractionStateMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Interaction state to set.
                /// </summary>
                [JsonProperty(PropertyName = "interaction-state")]
                public string interactionState;

                public SetInteractionStateMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _interactionState)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    interactionState = _interactionState;
                }
            }

            /// <summary>
            /// Set Motion Message.
            /// </summary>
            public class SetMotionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Angular velocity (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "angular-velocity")]
                public SerializableVector3 angularVelocity;

                /// <summary>
                /// Velocity (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "velocity")]
                public SerializableVector3 velocity;

                /// <summary>
                /// Whether or not the entity is stationary.
                /// </summary>
                [JsonProperty(PropertyName = "stationary")]
                public bool stationary;

                public SetMotionMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Vector3 _angularVelocity,
                    Vector3 _velocity, bool _stationary)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    angularVelocity = new SerializableVector3(_angularVelocity);
                    velocity = new SerializableVector3(_velocity);
                    stationary = _stationary;
                }
            }

            /// <summary>
            /// Set Parent Message.
            /// </summary>
            public class SetParentMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                public SetParentMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Guid? _parentID)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    parentID = parentID == null ? null : _parentID.ToString();
                }
            }

            /// <summary>
            /// Set Physical Properties Message.
            /// </summary>
            public class SetPhysicalPropertiesMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Angular drag.
                /// </summary>
                [JsonProperty(PropertyName = "angular-drag")]
                public float angularDrag;

                /// <summary>
                /// Center of mass (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "center-of-mass")]
                public SerializableVector3 centerOfMass;

                /// <summary>
                /// Drag.
                /// </summary>
                [JsonProperty(PropertyName = "drag")]
                public float drag;

                /// <summary>
                /// Whether or not the entity is gravitational.
                /// </summary>
                [JsonProperty(PropertyName = "gravitational")]
                public bool gravitational;

                /// <summary>
                /// Mass.
                /// </summary>
                [JsonProperty(PropertyName = "mass")]
                public float mass;

                public SetPhysicalPropertiesMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, float _angularDrag,
                    Vector3 _centerOfMass, float _drag, bool _gravitational, float _mass)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    angularDrag = _angularDrag;
                    centerOfMass = new SerializableVector3(_centerOfMass);
                    drag = _drag;
                    gravitational = _gravitational;
                    mass = _mass;
                }
            }

            /// <summary>
            /// Set Visibility Message.
            /// </summary>
            public class SetVisibilityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Whether or not the entity is visible.
                /// </summary>
                [JsonProperty(PropertyName = "visible")]
                public bool visible;

                public SetVisibilityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, bool _visible)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    visible = _visible;
                }
            }

            /// <summary>
            /// Update Entity Position Message.
            /// </summary>
            public class UpdateEntityPositionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                public UpdateEntityPositionMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Vector3 _position)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    position = new SerializableVector3(_position);
                }
            }

            /// <summary>
            /// Update Entity Rotation Message.
            /// </summary>
            public class UpdateEntityRotationMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                public UpdateEntityRotationMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Quaternion _rotation)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    rotation = new SerializableQuaternion(_rotation);
                }
            }

            /// <summary>
            /// Update Entity Scale Message.
            /// </summary>
            public class UpdateEntityScaleMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Scale (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "scale")]
                public SerializableVector3 scale;

                public UpdateEntityScaleMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Vector3 _scale)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    scale = new SerializableVector3(_scale);
                }
            }

            /// <summary>
            /// Update Entity Size Message.
            /// </summary>
            public class UpdateEntitySizeMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Size (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "size")]
                public SerializableVector3 size;

                public UpdateEntitySizeMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Vector3 _size)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    size = new SerializableVector3(_size);
                }
            }

            /// <summary>
            /// Modify Terrain Entity Message
            /// </summary>
            public class ModifyTerrainEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Modification to be made to the terrain.
                /// </summary>
                [JsonProperty(PropertyName = "modification")]
                public string modification;

                /// <summary>
                /// Size (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Brush type to be used for the terrain modification.
                /// </summary>
                [JsonProperty(PropertyName = "brush-type")]
                public string brushType;

                /// <summary>
                /// Layer on which modification is to be made to the terrain.
                /// </summary>
                [JsonProperty(PropertyName = "layer")]
                public int layer;

                public ModifyTerrainEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                Guid _sessionID, Guid _entityID, string _modification,
                Vector3 _position, string _brushType, int _layer)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    modification = _modification;
                    position = new SerializableVector3(_position);
                    brushType = _brushType;
                    layer = _layer;
                }
            }

            /// <summary>
            /// Publish Message Message.
            /// </summary>
            public class PublishMessageMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// Topic of message to publish.
                /// </summary>
                [JsonProperty(PropertyName = "topic")]
                public string topic;

                /// <summary>
                /// Message to publish.
                /// </summary>
                [JsonProperty(PropertyName = "message")]
                public string message;

                public PublishMessageMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, string _topic, string _message)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    topic = _topic;
                    message = _message;
                }
            }
        }

        /// <summary>
        /// Class for VOS Synchronization Status Messages.
        /// </summary>
        public class StatusMessages
        {
            /// <summary>
            /// Add Container Entity Message.
            /// </summary>
            public class AddContainerEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Whether or not to delete the entity when the client leaves the session.
                /// </summary>
                [JsonProperty(PropertyName = "delete-with-client")]
                public bool deleteWithClient;

                public AddContainerEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag, Guid? _parentID,
                    Vector3 _position, Quaternion _rotation, Vector3 _scale,
                    bool _deleteWithClient)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    scale = new SerializableVector3(_scale);
                    deleteWithClient = _deleteWithClient;
                }
            }

            /// <summary>
            /// Add Character Entity Message.
            /// </summary>
            public class AddCharacterEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// Path to a character entity model.
                /// </summary>
                [JsonProperty(PropertyName = "path")]
                public string path;

                /// <summary>
                /// Paths to character entity resources.
                /// </summary>
                [JsonProperty(PropertyName = "resources")]
                public string[] resources;

                /// <summary>
                /// Offset for the character entity model.
                /// </summary>
                [JsonProperty(PropertyName = "model-offset")]
                public SerializableVector3 modelOffset;

                /// <summary>
                /// Rotation for the character entity model.
                /// </summary>
                [JsonProperty(PropertyName = "model-rotation")]
                public SerializableQuaternion modelRotation;

                /// <summary>
                /// Offset for the character entity label.
                /// </summary>
                [JsonProperty(PropertyName = "label-offset")]
                public SerializableVector3 labelOffset;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                public AddCharacterEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag, string _path,
                    string[] _resources, Vector3 _modelOffset, Quaternion _modelRotation,
                    Vector3 _labelOffset, Guid? _parentID, Vector3 _position,
                    Quaternion _rotation, Vector3 _scaleSize, bool isSize)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    path = _path;
                    resources = _resources;
                    modelOffset = new SerializableVector3(_modelOffset);
                    modelRotation = new SerializableQuaternion(_modelRotation);
                    labelOffset = new SerializableVector3(_labelOffset);
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                }
            }

            /// <summary>
            /// Add Mesh Entity Message.
            /// </summary>
            public class AddMeshEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// Path to a mesh entity model.
                /// </summary>
                [JsonProperty(PropertyName = "path")]
                public string path;

                /// <summary>
                /// Paths to mesh entity resources.
                /// </summary>
                [JsonProperty(PropertyName = "resources")]
                public string[] resources;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                public AddMeshEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag, string _path,
                    string[] _resources, Guid? _parentID, Vector3 _position,
                    Quaternion _rotation, Vector3 _scaleSize, bool isSize)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    path = _path;
                    resources = _resources;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                }
            }

            /// <summary>
            /// Add Button Entity Message.
            /// </summary>
            public class AddButtonEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                /// <summary>
                /// Script logic to perform when button is clicked. Will attempt to invoke
                /// loaded JavaScript.
                /// </summary>
                [JsonProperty(PropertyName = "on-click")]
                public string onClick;

                public AddButtonEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent,
                    Vector2 _sizePercent, string _onClick)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                    onClick = _onClick;
                }
            }

            /// <summary>
            /// Add Canvas Entity Message.
            /// </summary>
            public class AddCanvasEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                public AddCanvasEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scaleSize, bool isSize)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                }
            }

            /// <summary>
            /// Add Input Entity Message.
            /// </summary>
            public class AddInputEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                public AddInputEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent,
                    Vector2 _sizePercent)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                }
            }

            /// <summary>
            /// Add Light Entity Message.
            /// </summary>
            public class AddLightEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                public AddLightEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                }
            }

            /// <summary>
            /// Add Terrain Entity Message.
            /// </summary>
            public class AddTerrainEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Length of the terrain in meters.
                /// </summary>
                [JsonProperty(PropertyName = "length")]
                public float length;

                /// <summary>
                /// Width of the terrain in meters.
                /// </summary>
                [JsonProperty(PropertyName = "width")]
                public float width;

                /// <summary>
                /// Height of the terrain in meters.
                /// </summary>
                [JsonProperty(PropertyName = "height")]
                public float height;

                /// <summary>
                /// 2D array of height values for the terrain.
                /// </summary>
                [JsonProperty(PropertyName = "heights")]
                public float[,] heights;

                /// <summary>
                /// Array of diffuse textures for the terrain entity's layers.
                /// </summary>
                [JsonProperty(PropertyName = "diffuse-texture")]
                public string[] diffuseTextures;

                /// <summary>
                /// Array of normal textures for the terrain entity's layers.
                /// </summary>
                [JsonProperty(PropertyName = "normal-texture")]
                public string[] normalTextures;

                /// <summary>
                /// Array of mask textures for the terrain entity's layers.
                /// </summary>
                [JsonProperty(PropertyName = "mask-texture")]
                public string[] maskTextures;

                /// <summary>
                /// Array of specular values for the terrain entity's layers
                /// </summary>
                [JsonProperty(PropertyName = "specular-values")]
                public string[] specularValues;

                /// <summary>
                /// Array of metallic values for the terrain entity's layers
                /// </summary>
                [JsonProperty(PropertyName = "metallic-values")]
                public float[] metallicValues;

                /// <summary>
                /// Array of smoothness values for the terrain entity's layers
                /// </summary>
                [JsonProperty(PropertyName = "smoothness-values")]
                public float[] smoothnessValues;

                /// <summary>
                /// The layer mask (VEML CSV-formatted) for the terrain entity.
                /// </summary>
                [JsonProperty(PropertyName = "layer-mask")]
                public string layerMask;

                /// <summary>
                /// Type of terrain entity ("heightmap, voxel, or hybrid").
                /// </summary>
                [JsonProperty(PropertyName = "type")]
                public string type;

                /// <summary>
                /// Terrain modifications for the terrain entity.
                /// </summary>
                [JsonProperty(PropertyName = "terrain-modification")]
                public TerrainModification[] modifications;

                public AddTerrainEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    float _length, float _width, float _height, float[,] _heights,
                    string[] _diffuseTextures, string[] _normalTextures,
                    string[] _maskTextures, string[] _specularValues,
                    float[] _metallicValues, float[] _smoothnessValues,
                    string _layerMask, string _type,
                    Dictionary<Vector3Int, Tuple<StraightFour.Entity.HybridTerrainEntity.TerrainOperation,
                    int, StraightFour.Entity.Terrain.TerrainEntityBrushType, float>> _modifications)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    length = _length;
                    width = _width;
                    height = _height;
                    heights = _heights;
                    diffuseTextures = _diffuseTextures;
                    normalTextures = _normalTextures;
                    maskTextures = _maskTextures;
                    specularValues = _specularValues;
                    metallicValues = _metallicValues;
                    smoothnessValues = _smoothnessValues;
                    layerMask = _layerMask;
                    type = _type;
                    System.Collections.Generic.List<TerrainModification> mods
                        = new System.Collections.Generic.List<TerrainModification>();
                    if (_modifications != null)
                    {
                        foreach (System.Collections.Generic.KeyValuePair<
                            Vector3Int, Tuple<StraightFour.Entity.HybridTerrainEntity.TerrainOperation,
                            int, StraightFour.Entity.Terrain.TerrainEntityBrushType, float>> mod
                            in _modifications)
                        {
                            if (mod.Key == null)
                            {
                                continue;
                            }

                            string modName = "";
                            switch (mod.Value.Item1)
                            {
                                case StraightFour.Entity.HybridTerrainEntity.TerrainOperation.Dig:
                                    modName = "dig";
                                    break;

                                case StraightFour.Entity.HybridTerrainEntity.TerrainOperation.Build:
                                    modName = "build";
                                    break;

                                case StraightFour.Entity.HybridTerrainEntity.TerrainOperation.Unset:
                                default:
                                    modName = "unset";
                                    break;
                            }

                            string bt = "";
                            switch (mod.Value.Item3)
                            {
                                case StraightFour.Entity.Terrain.TerrainEntityBrushType.sphere:
                                    bt = "sphere";
                                    break;

                                case StraightFour.Entity.Terrain.TerrainEntityBrushType.roundedCube:
                                default:
                                    bt = "roundedcube";
                                    break;
                            }
                            mods.Add(new TerrainModification(modName, mod.Key, bt, mod.Value.Item2, mod.Value.Item4));
                        }
                    }
                    modifications = mods.ToArray();
                }
            }

            /// <summary>
            /// Add Text Entity Message.
            /// </summary>
            public class AddTextEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                /// <summary>
                /// Text to display in the text field.
                /// </summary>
                [JsonProperty(PropertyName = "text")]
                public string text;

                /// <summary>
                /// Font size for the text in the text field.
                /// </summary>
                [JsonProperty(PropertyName = "font-size")]
                public int fontSize;

                public AddTextEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent,
                    Vector2 _sizePercent, string _text, int _fontSize)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                    text = _text;
                    fontSize = _fontSize;
                }
            }

            /// <summary>
            /// Add Voxel Entity Message.
            /// </summary>
            public class AddVoxelEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                public AddVoxelEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scale)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    scale = new SerializableVector3(_scale);
                }
            }

            /// <summary>
            /// Add Airplane Entity Message.
            /// </summary>
            public class AddAirplaneEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// Path to a mesh entity model.
                /// </summary>
                [JsonProperty(PropertyName = "path")]
                public string path;

                /// <summary>
                /// Mesh position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "mesh-position")]
                public SerializableVector3 meshPosition;

                /// <summary>
                /// Mesh rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "mesh-rotation")]
                public SerializableQuaternion meshRotation;

                /// <summary>
                /// Mass.
                /// </summary>
                [JsonProperty(PropertyName = "mass")]
                public float mass;

                public AddAirplaneEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scaleSize, bool isSize, string _path, Vector3 _meshPosition,
                    Quaternion _meshRotation, float _mass)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                    path = _path;
                    meshPosition = new SerializableVector3(_meshPosition);
                    meshRotation = new SerializableQuaternion(_meshRotation);
                    mass = _mass;
                }
            }

            /// <summary>
            /// Add Voxel Entity Message.
            /// </summary>
            public class AddAudioEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                public AddAudioEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scale)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    scale = new SerializableVector3(_scale);
                }
            }

            /// <summary>
            /// Add Automobile Entity Message.
            /// </summary>
            public class AddAutomobileEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// Path to a mesh entity model.
                /// </summary>
                [JsonProperty(PropertyName = "path")]
                public string path;

                /// <summary>
                /// Mesh position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "mesh-position")]
                public SerializableVector3 meshPosition;

                /// <summary>
                /// Mesh rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "mesh-rotation")]
                public SerializableQuaternion meshRotation;

                /// <summary>
                /// Mass.
                /// </summary>
                [JsonProperty(PropertyName = "mass")]
                public float mass;

                /// <summary>
                /// Automobile entity type.
                /// </summary>
                [JsonProperty(PropertyName = "automobile-entity-type")]
                public string automobileEntityType;

                /// <summary>
                /// Automobile entity wheels.
                /// </summary>
                [JsonProperty(PropertyName = "wheels")]
                public string wheels;

                public AddAutomobileEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scaleSize, bool isSize, string _path, Vector3 _meshPosition,
                    Quaternion _meshRotation, float _mass, string _automobileEntityType, string _wheels)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                    path = _path;
                    meshPosition = new SerializableVector3(_meshPosition);
                    meshRotation = new SerializableQuaternion(_meshRotation);
                    mass = _mass;
                    automobileEntityType = _automobileEntityType;
                    wheels = _wheels;
                }
            }

            /// <summary>
            /// Add Dropdown Entity Message.
            /// </summary>
            public class AddDropdownEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                /// <summary>
                /// On change event (takes int param for index of selected row in dropdown).
                /// </summary>
                [JsonProperty(PropertyName = "on-change")]
                public string onChange;

                /// <summary>
                /// Options for the dropdown.
                /// </summary>
                [JsonProperty(PropertyName = "options")]
                public string[] options;

                public AddDropdownEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent, Vector2 _sizePercent,
                    string _onChange, string[] _options)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                    onChange = _onChange;
                    options = _options;
                }
            }

            /// <summary>
            /// Add HTML Entity Message.
            /// </summary>
            public class AddHTMLEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale", Required = Required.AllowNull)]
                public SerializableVector3 scale;

                /// <summary>
                /// Size (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "size", Required = Required.AllowNull)]
                public SerializableVector3 size;

                /// <summary>
                /// On message event (takes string param for message received).
                /// </summary>
                [JsonProperty(PropertyName = "on-message")]
                public string onMessage;

                public AddHTMLEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector3 _position, Quaternion _rotation,
                    Vector3 _scaleSize, bool isSize, string _onMessage)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    position = new SerializableVector3(_position);
                    rotation = new SerializableQuaternion(_rotation);
                    if (isSize)
                    {
                        size = new SerializableVector3(_scaleSize);
                    }
                    else
                    {
                        scale = new SerializableVector3(_scaleSize);
                    }
                    onMessage = _onMessage;
                }
            }

            /// <summary>
            /// Add Image Entity Message.
            /// </summary>
            public class AddImageEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Tag of the entity.
                /// </summary>
                [JsonProperty(PropertyName = "tag")]
                public string tag;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                /// <summary>
                /// Position percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "position-percent")]
                public SerializableVector2 positionPercent;

                /// <summary>
                /// Size percent (Vector2 representation, each value ranging from 0-1).
                /// </summary>
                [JsonProperty(PropertyName = "size-percent")]
                public SerializableVector2 sizePercent;

                /// <summary>
                /// The path to the image file.
                /// </summary>
                [JsonProperty("image-file")]
                public string imageFile;

                public AddImageEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _tag,
                    Guid? _parentID, Vector2 _positionPercent, Vector2 _sizePercent, string _imageFile)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    tag = _tag;
                    parentID = _parentID.HasValue ? _parentID.Value.ToString() : null;
                    positionPercent = new SerializableVector2(_positionPercent);
                    sizePercent = new SerializableVector2(_sizePercent);
                    imageFile = _imageFile;
                }
            }

            /// <summary>
            /// Remove Entity Message.
            /// </summary>
            public class RemoveEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                public RemoveEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                }
            }

            /// <summary>
            /// Delete Entity Message.
            /// </summary>
            public class DeleteEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                public DeleteEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                }
            }

            /// <summary>
            /// Set Canvas Type Message.
            /// </summary>
            public class SetCanvasTypeMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Type (world or screen) of canvas.
                /// </summary>
                [JsonProperty(PropertyName = "canvas-type")]
                public string type;

                public SetCanvasTypeMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _type)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    type = _type;
                }
            }

            /// <summary>
            /// Set Highlight State Message.
            /// </summary>
            public class SetHighlightStateMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Whether or not to turn on highlighting.
                /// </summary>
                [JsonProperty(PropertyName = "highlighted")]
                public bool highlighted;

                public SetHighlightStateMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, bool _highlighted)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    highlighted = _highlighted;
                }
            }

            /// <summary>
            /// Set Interaction State Message.
            /// </summary>
            public class SetInteractionStateMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Interaction state to set.
                /// </summary>
                [JsonProperty(PropertyName = "interaction-state")]
                public string interactionState;

                public SetInteractionStateMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, string _interactionState)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    interactionState = _interactionState;
                }
            }

            /// <summary>
            /// Set Motion Message.
            /// </summary>
            public class SetMotionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Angular velocity (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "angular-velocity")]
                public SerializableVector3 angularVelocity;

                /// <summary>
                /// Velocity (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "velocity")]
                public SerializableVector3 velocity;

                /// <summary>
                /// Whether or not the entity is stationary.
                /// </summary>
                [JsonProperty(PropertyName = "stationary")]
                public bool stationary;

                public SetMotionMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Vector3 _angularVelocity,
                    Vector3 _velocity, bool _stationary)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    angularVelocity = new SerializableVector3(_angularVelocity);
                    velocity = new SerializableVector3(_velocity);
                    stationary = _stationary;
                }
            }

            /// <summary>
            /// Set Parent Message.
            /// </summary>
            public class SetParentMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// ID of the entity's parent
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "parent-id")]
                public string parentID;

                public SetParentMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Guid _parentID)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    parentID = _parentID.ToString();
                }
            }

            /// <summary>
            /// Set Physical Properties Message.
            /// </summary>
            public class SetPhysicalPropertiesMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Angular drag.
                /// </summary>
                [JsonProperty(PropertyName = "angular-drag")]
                public float angularDrag;

                /// <summary>
                /// Center of mass (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "center-of-mass")]
                public SerializableVector3 centerOfMass;

                /// <summary>
                /// Drag.
                /// </summary>
                [JsonProperty(PropertyName = "drag")]
                public float drag;

                /// <summary>
                /// Whether or not the entity is gravitational.
                /// </summary>
                [JsonProperty(PropertyName = "gravitational")]
                public bool gravitational;

                /// <summary>
                /// Mass.
                /// </summary>
                [JsonProperty(PropertyName = "mass")]
                public float mass;

                public SetPhysicalPropertiesMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, float _angularDrag,
                    Vector3 _centerOfMass, float _drag, bool _gravitational, float _mass)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    angularDrag = _angularDrag;
                    centerOfMass = new SerializableVector3(_centerOfMass);
                    drag = _drag;
                    gravitational = _gravitational;
                    mass = _mass;
                }
            }

            /// <summary>
            /// Set Visibility Message.
            /// </summary>
            public class SetVisibilityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Whether or not the entity is visible.
                /// </summary>
                [JsonProperty(PropertyName = "visible")]
                public bool visible;

                public SetVisibilityMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, bool _visible)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    visible = _visible;
                }
            }

            /// <summary>
            /// Update Entity Position Message.
            /// </summary>
            public class UpdateEntityPositionMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Position (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                public UpdateEntityPositionMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Vector3 _position)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    position = new SerializableVector3(_position);
                }
            }

            /// <summary>
            /// Update Entity Rotation Message.
            /// </summary>
            public class UpdateEntityRotationMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Rotation (Quaternion representation).
                /// </summary>
                [JsonProperty(PropertyName = "rotation")]
                public SerializableQuaternion rotation;

                public UpdateEntityRotationMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Quaternion _rotation)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    rotation = new SerializableQuaternion(_rotation);
                }
            }

            /// <summary>
            /// Update Entity Scale Message.
            /// </summary>
            public class UpdateEntityScaleMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Scale (Vector3 representation). One of (scale, size) must be provided.
                /// </summary>
                [JsonProperty(PropertyName = "scale")]
                public SerializableVector3 scale;

                public UpdateEntityScaleMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Vector3 _scale)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    scale = new SerializableVector3(_scale);
                }
            }

            /// <summary>
            /// Update Entity Size Message.
            /// </summary>
            public class UpdateEntitySizeMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Size (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "size")]
                public SerializableVector3 size;

                public UpdateEntitySizeMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, Guid _entityID, Vector3 _size)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    size = new SerializableVector3(_size);
                }
            }

            /// <summary>
            /// Modify Terrain Entity Message
            /// </summary>
            public class ModifyTerrainEntityMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// ID of the entity (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "entity-id")]
                public string id;

                /// <summary>
                /// Modification to be made to the terrain.
                /// </summary>
                [JsonProperty(PropertyName = "modification")]
                public string modification;

                /// <summary>
                /// Size (Vector3 representation).
                /// </summary>
                [JsonProperty(PropertyName = "position")]
                public SerializableVector3 position;

                /// <summary>
                /// Brush type to be used for the terrain modification.
                /// </summary>
                [JsonProperty(PropertyName = "brush-type")]
                public string brushType;

                /// <summary>
                /// Layer on which modification is to be made to the terrain.
                /// </summary>
                [JsonProperty(PropertyName = "layer")]
                public int layer;

                /// <summary>
                /// Size of the modification.
                /// </summary>
                [JsonProperty(PropertyName = "size")]
                public float size;

                public ModifyTerrainEntityMessage(Guid _messageID, string _clientID, string _clientToken,
                Guid _sessionID, Guid _entityID, string _modification,
                Vector3 _position, string _brushType, int _layer)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    id = _entityID.ToString();
                    position = new SerializableVector3(_position);
                    brushType = _brushType;
                    layer = _layer;
                }
            }

            /// <summary>
            /// New Message Message.
            /// </summary>
            public class NewMessageMessage
            {
                /// <summary>
                /// ID of the message (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "message-id")]
                public string messageID;

                /// <summary>
                /// ID of the client sending the message
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "client-id")]
                public string clientID;

                /// <summary>
                /// Authentication token for the client sending the message.
                /// </summary>
                [JsonProperty(PropertyName = "client-token")]
                public string clientToken;

                /// <summary>
                /// ID of the session the message pertains to
                /// (string representation of UUID).
                /// </summary>
                [JsonProperty(PropertyName = "session-id")]
                public string sessionID;

                /// <summary>
                /// Topic of message to publish.
                /// </summary>
                [JsonProperty(PropertyName = "topic")]
                public string topic;

                /// <summary>
                /// Message to publish.
                /// </summary>
                [JsonProperty(PropertyName = "message")]
                public string message;

                public NewMessageMessage(Guid _messageID, string _clientID, string _clientToken,
                    Guid _sessionID, string _topic, string _message)
                {
                    messageID = _messageID.ToString();
                    clientID = _clientID;
                    clientToken = _clientToken;
                    sessionID = _sessionID.ToString();
                    topic = _topic;
                    message = _message;
                }
            }
        }
    }
}