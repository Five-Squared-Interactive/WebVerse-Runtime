// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using System;
using FiveSQD.WebVerse.Runtime;
#if USE_WEBINTERFACE
using FiveSQD.WebVerse.WebInterface.HTTP;
#endif
using FiveSQD.StraightFour.Entity;
using System.Collections;
using Newtonsoft.Json;
using System.Linq;

namespace FiveSQD.WebVerse.Handlers.JSONEntity
{
    /// <summary>
    /// JSON representation of a Vector3.
    /// </summary>
    [Serializable]
    public class JSONVector3
    {
        public float x;
        public float y;
        public float z;

        public JSONVector3() { }

        public JSONVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// JSON representation of a Quaternion.
    /// </summary>
    [Serializable]
    public class JSONQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public JSONQuaternion() { }

        public JSONQuaternion(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }

    /// <summary>
    /// JSON representation of a container entity.
    /// </summary>
    [Serializable]
    public class JSONContainerEntity
    {
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        public bool isSize;
        public string parentId;
        public List<JSONContainerEntity> children;

        public JSONContainerEntity()
        {
            children = new List<JSONContainerEntity>();
        }
    }

    /// <summary>
    /// JSON representation of a color.
    /// </summary>
    [Serializable]
    public class JSONColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public JSONColor() 
        { 
            a = 1f; // Default alpha to 1
        }

        public JSONColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }
    }

    /// <summary>
    /// JSON representation of a mesh entity.
    /// </summary>
    [Serializable]
    public class JSONMeshEntity
    {
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        public bool isSize;
        public string parentId;
        public string meshType; // "primitive", "gltf" (prefab removed)
        public string meshSource; // Path to mesh file or primitive type
        public string[] meshResources; // Additional resources (textures, etc.)
        public JSONColor color; // Single color for primitive entities
        public List<JSONMeshEntity> children;

        public JSONMeshEntity()
        {
            meshResources = new string[0];
            children = new List<JSONMeshEntity>();
        }
    }

    /// <summary>
    /// JSON representation of a collection of mesh entities.
    /// Expected format: {"mesh-entities":[<mesh-entity-format>,...]}
    /// </summary>
    [Serializable]
    public class JSONMeshEntityCollection
    {
        [JsonProperty("mesh-entities")]
        public JSONMeshEntity[] meshEntities;

        public JSONMeshEntityCollection()
        {
            meshEntities = new JSONMeshEntity[0];
        }
    }

    /// <summary>
    /// WebVerse's JSON Entity Handler.
    /// Processes JSON-encoded container entities and mesh entities, creating them using the EntityManager.
    /// 
    /// Container Entity Example:
    /// {
    ///   "id": "550e8400-e29b-41d4-a716-446655440000",
    ///   "tag": "MyContainer",
    ///   "position": { "x": 0, "y": 1, "z": 0 },
    ///   "rotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
    ///   "scale": { "x": 1, "y": 1, "z": 1 },
    ///   "isSize": false,
    ///   "parentId": null,
    ///   "children": []
    /// }
    /// 
    /// Mesh Entity Example:
    /// {
    ///   "id": "550e8400-e29b-41d4-a716-446655440001",
    ///   "tag": "MyCube",
    ///   "position": { "x": 2, "y": 1, "z": 0 },
    ///   "rotation": { "x": 0, "y": 0, "z": 0, "w": 1 },
    ///   "scale": { "x": 1, "y": 1, "z": 1 },
    ///   "isSize": false,
    ///   "meshType": "primitive",
    ///   "meshSource": "cube",
    ///   "color": { "r": 1, "g": 0, "b": 0, "a": 1 },
    ///   "children": []
    /// }
    /// </summary>
    
    /// <summary>
    /// JSON representation of a Terrain Entity.
    /// Supports both standard format and alternative base_ground/ground_mods format.
    /// 
    /// Standard Format:
    /// {
    ///   "terrainType": "hybrid",
    ///   "heights": [[1,2,3,...], [...], ...],
    ///   "layers": [...],
    ///   "layerMasks": [...],
    ///   "modifications": [...]
    /// }
    /// 
    /// Alternative Format:
    /// {
    ///   "terrainType": "hybrid",
    ///   "base_ground": {
    ///     "heights": [[1,2,3,...], [...], ...],
    ///     "layers": [[[1,1,1,...], [...], ...], [[...], ...], ...]
    ///   },
    ///   "ground_mods": [
    ///     {"operation": "dig", "x": 200, "y": 146, "z": 408, "brushtype": "roundedCube", "layer": 2, "brushsize": 1},
    ///     ...
    ///   ]
    /// }
    /// </summary>
    [Serializable]
    public class JSONTerrainEntity
    {
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        public bool isSize;
        public string parentId; // For reference only, not used in creation
        public string terrainType; // "heightmap" or "hybrid"
        public float length;
        public float width;
        public float height;
        
        // Standard format properties
        public float[][] heights; // 2D array of heights (jagged array for JSON serialization)
        public JSONTerrainEntityLayer[] layers;
        public JSONTerrainEntityLayerMask[] layerMasks; // Array of layer masks, each defining strength coefficients for the corresponding layer
        public JSONTerrainEntityModification[] modifications; // For hybrid terrain only
        
        // Alternative format properties
        public JSONTerrainBaseGround base_ground; // Alternative format: base terrain data
        public JSONTerrainEntityModification[] ground_mods; // Alternative format: terrain modifications
        
        public bool stitchTerrains;
        public JSONTerrainEntity[] children;

        public JSONTerrainEntity()
        {
            position = new JSONVector3();
            rotation = new JSONQuaternion();
            scale = new JSONVector3 { x = 1, y = 1, z = 1 };
            isSize = false;
            terrainType = "heightmap";
            stitchTerrains = false;
            children = new JSONTerrainEntity[0];
        }
    }

    /// <summary>
    /// JSON representation of a Terrain Entity Layer.
    /// </summary>
    [Serializable]
    public class JSONTerrainEntityLayer
    {
        public string diffuseTexture;
        public string normalTexture;
        public string maskTexture;
        public JSONColor specular;
        public float metallic;
        public float smoothness;
        public int sizeFactor;

        public JSONTerrainEntityLayer()
        {
            specular = new JSONColor { r = 0.2f, g = 0.2f, b = 0.2f, a = 1.0f };
            metallic = 0.0f;
            smoothness = 0.5f;
            sizeFactor = 1;
        }
    }

    /// <summary>
    /// JSON representation of a Named Terrain Entity Layer for alternative format.
    /// Used when layers are provided as named objects instead of arrays.
    /// 
    /// Example JSON structure:
    /// {
    ///   "basalt": {
    ///     "layer": 0,
    ///     "color_texture": "Rock_047_BaseColor.png",
    ///     "normal_texture": "Rock_047_Normal.png",
    ///     "specular": { "r": 0.2, "g": 0.2, "b": 0.2, "a": 1.0 },
    ///     "metallic": 0.0,
    ///     "smoothness": 0.5,
    ///     "sizeFactor": 1
    ///   },
    ///   "grass": {
    ///     "layer": 1,
    ///     "color_texture": "Grass_001_BaseColor.png",
    ///     "normal_texture": "Grass_001_Normal.png",
    ///     ...
    ///   }
    /// }
    /// </summary>
    [Serializable]
    public class JSONNamedTerrainLayer
    {
        [JsonProperty("layer")]
        public int layer;
        [JsonProperty("color_texture")]
        public string color_texture;
        [JsonProperty("normal_texture")]
        public string normal_texture;
        [JsonProperty("mask_texture")]
        public string mask_texture;
        [JsonProperty("specular")]
        public JSONColor specular;
        [JsonProperty("metallic")]
        public float metallic;
        [JsonProperty("smoothness")]
        public float smoothness;
        [JsonProperty("sizeFactor")]
        public int sizeFactor;

        public JSONNamedTerrainLayer()
        {
            layer = 0;
            specular = new JSONColor { r = 0.2f, g = 0.2f, b = 0.2f, a = 1.0f };
            metallic = 0.0f;
            smoothness = 0.5f;
            sizeFactor = 1;
        }

        /// <summary>
        /// Convert to standard JSONTerrainEntityLayer format.
        /// </summary>
        /// <returns>Standard terrain layer</returns>
        public JSONTerrainEntityLayer ToStandardLayer()
        {
            var standardLayer = new JSONTerrainEntityLayer
            {
                diffuseTexture = color_texture,
                normalTexture = normal_texture,
                maskTexture = mask_texture,
                specular = specular,
                metallic = metallic,
                smoothness = smoothness,
                sizeFactor = sizeFactor
            };
            
            return standardLayer;
        }
    }

    /// <summary>
    /// JSON representation of a Terrain Entity Layer Mask.
    /// Defines the strength/coefficient of a terrain layer at specific x-y coordinates.
    /// Input format: A 2D jagged array where each element represents the layer strength at that x-y position.
    /// Example JSON structure:
    /// {
    ///   "heights": [
    ///     [1.0, 0.8, 0.5, 1.0, 1.0, 1.0, ...], // First row (y=0)
    ///     [0.9, 0.7, 0.4, 0.8, 0.9, 1.0, ...], // Second row (y=1) 
    ///     [...],                                 // Additional rows
    ///     ...
    ///   ]
    /// }
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(TerrainEntityLayerMaskConverter))]
    public class JSONTerrainEntityLayerMask
    {
        /// <summary>
        /// 2D array of layer mask coefficients. Each sub-array represents a row (y-coordinate),
        /// and each element within represents the layer strength at that x-y position (0.0 to 1.0).
        /// </summary>
        public float[][] heights; // 2D array of mask heights (jagged array for JSON serialization)

        public JSONTerrainEntityLayerMask()
        {
        }
    }

    /// <summary>
    /// Custom JSON converter for JSONTerrainEntityLayerMask to handle both object and direct array formats.
    /// Supports both {"heights": [[1,2,3]]} and [[1,2,3]] formats.
    /// </summary>
    public class TerrainEntityLayerMaskConverter : JsonConverter<JSONTerrainEntityLayerMask>
    {
        public override void WriteJson(JsonWriter writer, JSONTerrainEntityLayerMask value, JsonSerializer serializer)
        {
            // Write as standard object format
            writer.WriteStartObject();
            writer.WritePropertyName("heights");
            serializer.Serialize(writer, value.heights);
            writer.WriteEndObject();
        }

        public override JSONTerrainEntityLayerMask ReadJson(JsonReader reader, Type objectType, JSONTerrainEntityLayerMask existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var layerMask = new JSONTerrainEntityLayerMask();

            if (reader.TokenType == JsonToken.StartArray)
            {
                // Direct array format: [[1,2,3], [4,5,6]]
                layerMask.heights = serializer.Deserialize<float[][]>(reader);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                // Object format: {"heights": [[1,2,3], [4,5,6]]}
                var jObject = Newtonsoft.Json.Linq.JObject.Load(reader);
                if (jObject["heights"] != null)
                {
                    layerMask.heights = jObject["heights"].ToObject<float[][]>();
                }
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when deserializing JSONTerrainEntityLayerMask");
            }

            return layerMask;
        }
    }

    /// <summary>
    /// Custom JSON converter for JSONTerrainBaseGround to handle both array and named layer formats.
    /// Supports both "layers": [[[]]] and "layers": {"basalt": {...}, "grass": {...}} formats.
    /// </summary>
    public class TerrainBaseGroundConverter : JsonConverter<JSONTerrainBaseGround>
    {
        public override void WriteJson(JsonWriter writer, JSONTerrainBaseGround value, JsonSerializer serializer)
        {
            // Write in standard format
            writer.WriteStartObject();
            
            if (value.heights != null)
            {
                writer.WritePropertyName("heights");
                serializer.Serialize(writer, value.heights);
            }
            
            if (value.layers != null)
            {
                writer.WritePropertyName("layers");
                serializer.Serialize(writer, value.layers);
            }
            
            if (value.namedLayers != null)
            {
                writer.WritePropertyName("namedLayers");
                serializer.Serialize(writer, value.namedLayers);
            }
            
            if (value.layerMasks != null)
            {
                writer.WritePropertyName("layerMasks");
                serializer.Serialize(writer, value.layerMasks);
            }
            
            writer.WriteEndObject();
        }

        public override JSONTerrainBaseGround ReadJson(JsonReader reader, Type objectType, JSONTerrainBaseGround existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var baseGround = new JSONTerrainBaseGround();
            var jObject = Newtonsoft.Json.Linq.JObject.Load(reader);

            // Handle heights
            if (jObject["heights"] != null)
            {
                baseGround.heights = jObject["heights"].ToObject<float[][]>();
            }

            // Handle layers - check if it's array format or named object format
            if (jObject["layers"] != null)
            {
                var layersToken = jObject["layers"];
                
                if (layersToken.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                {
                    // Array format: [[[...]], [[...]], ...]
                    baseGround.layers = layersToken.ToObject<float[][][]>();
                }
                else if (layersToken.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                {
                    // Named object format: {"basalt": {...}, "grass": {...}}
                    baseGround.namedLayers = layersToken.ToObject<object>();
                }
            }

            // Handle explicit namedLayers property
            if (jObject["namedLayers"] != null)
            {
                baseGround.namedLayers = jObject["namedLayers"].ToObject<object>();
            }

            // Handle layerMasks
            if (jObject["layerMasks"] != null)
            {
                baseGround.layerMasks = jObject["layerMasks"].ToObject<JSONTerrainEntityLayerMask[]>(serializer);
            }

            return baseGround;
        }
    }

    /// <summary>
    /// JSON representation of a Terrain Base Ground structure for alternative JSON format.
    /// Used when terrain data is provided in the base_ground object format.
    /// 
    /// Example JSON structure (Array format):
    /// {
    ///   "heights": [[1,2,3,...], [...], ...],  // Height map data
    ///   "layers": [                             // Layer mask data for each layer
    ///     [[1,1,1,...], [...], ...],           // Layer 0 mask coefficients
    ///     [[0,0,0,...], [...], ...],           // Layer 1 mask coefficients
    ///     ...
    ///   ]
    /// }
    /// 
    /// Example JSON structure (Named format):
    /// {
    ///   "heights": [[1,2,3,...], [...], ...],
    ///   "layers": {
    ///     "basalt": {
    ///       "layer": 0,
    ///       "color_texture": "Rock_047_BaseColor.png",
    ///       "normal_texture": "Rock_047_Normal.png",
    ///       "specular": {...}, "metallic": 0.0, "smoothness": 0.5, "sizeFactor": 1
    ///     },
    ///     "grass": {
    ///       "layer": 1,
    ///       "color_texture": "Grass_001_BaseColor.png",
    ///       ...
    ///     }
    ///   }
    /// }
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(TerrainBaseGroundConverter))]
    public class JSONTerrainBaseGround
    {
        /// <summary>
        /// 2D array of terrain heights (jagged array for JSON serialization).
        /// </summary>
        public float[][] heights;
        
        /// <summary>
        /// 3D array of layer mask coefficients (jagged array for JSON serialization).
        /// First dimension: layer index, Second/Third dimensions: x/y coordinates.
        /// Each element represents the strength of that layer at the x-y position (0.0 to 1.0).
        /// Optional when using namedLayers format.
        /// </summary>
        public float[][][] layers;

        /// <summary>
        /// Named layer definitions as raw object for flexible deserialization.
        /// This allows for named layers like "basalt", "grass", etc. with their properties.
        /// Will be processed at runtime to extract layer information.
        /// </summary>
        public object namedLayers;

        /// <summary>
        /// Layer masks for named layers format.
        /// Array of JSONTerrainEntityLayerMask objects that define where each layer appears on the terrain.
        /// Used in conjunction with namedLayers when layers are defined as named objects.
        /// </summary>
        public JSONTerrainEntityLayerMask[] layerMasks;

        public JSONTerrainBaseGround()
        {
        }
    }

    /// <summary>
    /// JSON representation of a Terrain Entity Modification.
    /// Supports both position object format and individual x/y/z coordinate format.
    /// 
    /// JSON Format:
    /// {
    ///   "operation": "dig",           // "dig" or "build"
    ///   "x": 200,                     // X coordinate
    ///   "y": 146,                     // Y coordinate  
    ///   "z": 408,                     // Z coordinate
    ///   "brushtype": "roundedCube",   // "sphere" or "roundedCube"
    ///   "layer": 2,                   // Layer index
    ///   "brushsize": 1                // Size/radius of the brush
    /// }
    /// </summary>
    [Serializable]
    public class JSONTerrainEntityModification
    {
        public string operation; // "build" or "dig"
        
        // Support for position object format
        public JSONVector3 position;
        
        // Support for individual coordinate format
        public float x;
        public float y; 
        public float z;
        
        public string brushType; // "sphere" or "roundedCube"
        public string brushtype; // Alternative lowercase property name for JSON compatibility
        public int layer;
        public float size;
        public float brushsize; // Alternative property name for JSON compatibility

        public JSONTerrainEntityModification()
        {
            operation = "build";
            position = new JSONVector3();
            x = 0;
            y = 0;
            z = 0;
            brushType = "sphere";
            brushtype = "sphere";
            layer = 0;
            size = 1.0f;
            brushsize = 1.0f;
        }
    }

    /// <summary>
    /// JSON representation of an Airplane Entity.
    /// </summary>
    [Serializable]
    public class JSONAirplaneEntity
    {
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        public bool isSize;
        public string parentId; // For reference only, not used in creation
        public string meshObject; // Path to the mesh object (GLTF file)
        public string[] meshResources; // Additional mesh resources
        public float mass;
        public float throttle;
        public float pitch;
        public float roll;
        public float yaw;
        public bool checkForUpdateIfCached;
        public JSONAirplaneEntity[] children;

        public JSONAirplaneEntity()
        {
            position = new JSONVector3();
            rotation = new JSONQuaternion();
            scale = new JSONVector3 { x = 1, y = 1, z = 1 };
            isSize = false;
            mass = 1000f;
            throttle = 0f;
            pitch = 0f;
            roll = 0f;
            yaw = 0f;
            checkForUpdateIfCached = true;
            children = new JSONAirplaneEntity[0];
        }
    }

    /// <summary>
    /// JSON representation of an Automobile Entity.
    /// </summary>
    [Serializable]
    public class JSONAutomobileEntity
    {
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        public bool isSize;
        public string parentId; // For reference only, not used in creation
        public string meshObject; // Path to the mesh object (GLTF file)
        public string[] meshResources; // Additional mesh resources
        public JSONAutomobileEntityWheel[] wheels;
        public float mass;
        public string automobileType; // "Default" (for now, only one type)
        public float throttle;
        public float steer;
        public float brake;
        public float handBrake;
        public bool horn;
        public int gear;
        public bool engineStartStop;
        public bool checkForUpdateIfCached;
        public JSONAutomobileEntity[] children;

        public JSONAutomobileEntity()
        {
            position = new JSONVector3();
            rotation = new JSONQuaternion();
            scale = new JSONVector3 { x = 1, y = 1, z = 1 };
            isSize = false;
            mass = 1500f;
            automobileType = "Default";
            throttle = 0f;
            steer = 0f;
            brake = 0f;
            handBrake = 0f;
            horn = false;
            gear = 0;
            engineStartStop = false;
            checkForUpdateIfCached = true;
            children = new JSONAutomobileEntity[0];
        }
    }

    /// <summary>
    /// JSON representation of an Automobile Entity Wheel.
    /// </summary>
    [Serializable]
    public class JSONAutomobileEntityWheel
    {
        public string wheelSubMesh;
        public float wheelRadius;

        public JSONAutomobileEntityWheel()
        {
            wheelRadius = 0.5f;
        }

        public JSONAutomobileEntityWheel(string wheelSubMesh, float wheelRadius)
        {
            this.wheelSubMesh = wheelSubMesh;
            this.wheelRadius = wheelRadius;
        }
    }

    /// <summary>
    /// JSON representation of a Vector2.
    /// </summary>
    [Serializable]
    public class JSONVector2
    {
        public float x;
        public float y;

        public JSONVector2() { }

        public JSONVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// JSON representation of a Canvas Entity.
    /// </summary>
    [Serializable]
    public class JSONCanvasEntity
    {
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        public bool isSize;
        public string parentId; // For reference only, not used in creation
        public string canvasType; // "world" or "screen"
        public JSONCanvasEntity[] children;

        public JSONCanvasEntity()
        {
            position = new JSONVector3();
            rotation = new JSONQuaternion();
            scale = new JSONVector3 { x = 1, y = 1, z = 1 };
            isSize = false;
            canvasType = "screen";
            children = new JSONCanvasEntity[0];
        }
    }

    /// <summary>
    /// JSON representation of a Button Entity.
    /// </summary>
    [Serializable]
    public class JSONButtonEntity
    {
        public string id;
        public string tag;
        public JSONVector2 positionPercent;
        public JSONVector2 sizePercent;
        public string parentId; // For reference only, not used in creation
        public string onClick; // JavaScript function to call on click
        public string text; // Button text
        public int fontSize;
        public JSONColor textColor;
        public string backgroundImage; // Path to background image
        public JSONColor backgroundColor;
        public JSONButtonEntity[] children;

        public JSONButtonEntity()
        {
            positionPercent = new JSONVector2 { x = 0.5f, y = 0.5f };
            sizePercent = new JSONVector2 { x = 0.2f, y = 0.1f };
            text = "Button";
            fontSize = 14;
            textColor = new JSONColor { r = 0, g = 0, b = 0, a = 1 }; // Black text
            backgroundColor = new JSONColor { r = 1, g = 1, b = 1, a = 1 }; // White background
            children = new JSONButtonEntity[0];
        }
    }

    /// <summary>
    /// JSON representation of a Text Entity.
    /// </summary>
    [Serializable]
    public class JSONTextEntity
    {
        public string id;
        public string tag;
        public JSONVector2 positionPercent;
        public JSONVector2 sizePercent;
        public string parentId; // For reference only, not used in creation
        public string text;
        public int fontSize;
        public JSONColor color;
        public string fontStyle; // "normal", "bold", "italic", "boldItalic"
        public string alignment; // "left", "center", "right"
        public bool wordWrap;
        public JSONTextEntity[] children;

        public JSONTextEntity()
        {
            positionPercent = new JSONVector2 { x = 0.5f, y = 0.5f };
            sizePercent = new JSONVector2 { x = 0.3f, y = 0.1f };
            text = "Text";
            fontSize = 14;
            color = new JSONColor { r = 0, g = 0, b = 0, a = 1 }; // Black
            fontStyle = "normal";
            alignment = "center";
            wordWrap = true;
            children = new JSONTextEntity[0];
        }
    }

    /// <summary>
    /// JSON representation of an Input Entity.
    /// </summary>
    [Serializable]
    public class JSONInputEntity
    {
        public string id;
        public string tag;
        public JSONVector2 positionPercent;
        public JSONVector2 sizePercent;
        public string parentId; // For reference only, not used in creation
        public string placeholder; // Placeholder text
        public string text; // Initial text value
        public string inputType; // "text", "password", "multiline"
        public int characterLimit;
        public bool readOnly;
        public string onValueChanged; // JavaScript function to call when value changes
        public string onEndEdit; // JavaScript function to call when editing ends
        public JSONInputEntity[] children;

        public JSONInputEntity()
        {
            positionPercent = new JSONVector2 { x = 0.5f, y = 0.5f };
            sizePercent = new JSONVector2 { x = 0.3f, y = 0.1f };
            placeholder = "Enter text...";
            text = "";
            inputType = "text";
            characterLimit = 0; // No limit
            readOnly = false;
            children = new JSONInputEntity[0];
        }
    }

    /// <summary>
    /// JSON representation of a Dropdown Entity.
    /// </summary>
    [Serializable]
    public class JSONDropdownEntity
    {
        public string id;
        public string tag;
        public JSONVector2 positionPercent;
        public JSONVector2 sizePercent;
        public string parentId; // For reference only, not used in creation
        public string[] options; // Dropdown options
        public int selectedIndex; // Currently selected option
        public string onChange; // JavaScript function to call when selection changes
        public string captionText; // Text shown when no option is selected
        public JSONDropdownEntity[] children;

        public JSONDropdownEntity()
        {
            positionPercent = new JSONVector2 { x = 0.5f, y = 0.5f };
            sizePercent = new JSONVector2 { x = 0.2f, y = 0.05f };
            options = new string[] { "Option 1", "Option 2", "Option 3" };
            selectedIndex = 0;
            captionText = "Select...";
            children = new JSONDropdownEntity[0];
        }
    }

    /// <summary>
    /// JSON representation of an image entity for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONImageEntity
    {
        public string type = "image";
        public string id;
        public string tag;
        public JSONVector2 positionPercent;
        public JSONVector2 sizePercent;
        public string imageFile;
        public bool stretchToParent = false;
        public string alignment = "center"; // "center", "left", "right", "top", "bottom"

        public JSONImageEntity()
        {
            positionPercent = new JSONVector2 { x = 0.5f, y = 0.5f };
            sizePercent = new JSONVector2 { x = 0.3f, y = 0.3f };
            imageFile = "";
            alignment = "center";
        }
    }

    /// <summary>
    /// JSON representation of an HTML entity for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONHTMLEntity
    {
        public string type = "html";
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        public JSONVector2 positionPercent; // For UI canvas placement
        public JSONVector2 sizePercent; // For UI canvas sizing
        public bool isSize = false;
        public bool isCanvasElement = false; // true for UI placement, false for 3D world placement
        public string url; // URL to load content from
        public string html; // Direct HTML content
        public string onMessage; // JavaScript callback for messages

        public JSONHTMLEntity()
        {
            position = new JSONVector3 { x = 0.0f, y = 0.0f, z = 0.0f };
            rotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
            scale = new JSONVector3 { x = 1.0f, y = 1.0f, z = 1.0f };
            positionPercent = new JSONVector2 { x = 0.5f, y = 0.5f };
            sizePercent = new JSONVector2 { x = 0.5f, y = 0.5f };
            isSize = false;
            isCanvasElement = false;
            url = "";
            html = "";
            onMessage = "";
        }
    }

    /// <summary>
    /// JSON representation of a light entity for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONLightEntity
    {
        public string type = "light";
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public string lightType = "point"; // "point", "directional", "spot"
        public JSONColor color;
        public int temperature = 6500;
        public float intensity = 1.0f;
        public float range = 10.0f;
        public float innerSpotAngle = 21.8f; // For spot lights
        public float outerSpotAngle = 30.0f; // For spot lights

        public JSONLightEntity()
        {
            position = new JSONVector3 { x = 0.0f, y = 0.0f, z = 0.0f };
            rotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
            lightType = "point";
            color = new JSONColor { r = 1.0f, g = 1.0f, b = 1.0f, a = 1.0f };
            temperature = 6500;
            intensity = 1.0f;
            range = 10.0f;
            innerSpotAngle = 21.8f;
            outerSpotAngle = 30.0f;
        }
    }

    /// <summary>
    /// JSON representation of a voxel block subtype for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONVoxelBlockSubType
    {
        public int id;
        public bool invisible = false;
        public string topTexture = "";
        public string bottomTexture = "";
        public string leftTexture = "";
        public string rightTexture = "";
        public string frontTexture = "";
        public string backTexture = "";

        public JSONVoxelBlockSubType()
        {
            invisible = false;
            topTexture = "";
            bottomTexture = "";
            leftTexture = "";
            rightTexture = "";
            frontTexture = "";
            backTexture = "";
        }
    }

    /// <summary>
    /// JSON representation of a voxel block info for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONVoxelBlockInfo
    {
        public int id;
        public JSONVoxelBlockSubType[] subTypes;

        public JSONVoxelBlockInfo()
        {
            subTypes = new JSONVoxelBlockSubType[0];
        }
    }

    /// <summary>
    /// JSON representation of a voxel entity for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONVoxelEntity
    {
        public string type = "voxel";
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        public JSONVoxelBlockInfo[] blockInfos; // Array of block definitions
        public JSONVoxelBlock[] blocks; // Array of individual block placements

        public JSONVoxelEntity()
        {
            position = new JSONVector3 { x = 0.0f, y = 0.0f, z = 0.0f };
            rotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
            scale = new JSONVector3 { x = 1.0f, y = 1.0f, z = 1.0f };
            blockInfos = new JSONVoxelBlockInfo[0];
            blocks = new JSONVoxelBlock[0];
        }
    }

    /// <summary>
    /// JSON representation of a voxel block placement for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONVoxelBlock
    {
        public int x;
        public int y;
        public int z;
        public int type;
        public int subType;
    }

    /// <summary>
    /// JSON representation of a water entity for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONWaterEntity
    {
        public string type = "water";
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        
        // Water appearance properties
        public JSONColor shallowColor;
        public JSONColor deepColor;
        public JSONColor specularColor;
        public JSONColor scatteringColor;
        
        // Water depth properties
        public float deepStart = 0.5f;
        public float deepEnd = 10.0f;
        
        // Water visual properties
        public float distortion = 32.0f; // 0-128
        public float smoothness = 0.8f; // 0-1
        public float intensity = 0.7f; // 0-1
        
        // Wave properties
        public float numWaves = 4.0f; // 1-32
        public float waveAmplitude = 0.3f; // 0-1
        public float waveSteepness = 0.5f; // 0-1
        public float waveSpeed = 1.0f;
        public float waveLength = 10.0f;
        public float waveScale = 1.0f;

        public JSONWaterEntity()
        {
            position = new JSONVector3 { x = 0.0f, y = 0.0f, z = 0.0f };
            rotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
            scale = new JSONVector3 { x = 10.0f, y = 1.0f, z = 10.0f };
            
            // Default water colors
            shallowColor = new JSONColor { r = 0.4f, g = 0.8f, b = 1.0f, a = 0.8f };
            deepColor = new JSONColor { r = 0.1f, g = 0.3f, b = 0.7f, a = 1.0f };
            specularColor = new JSONColor { r = 1.0f, g = 1.0f, b = 1.0f, a = 1.0f };
            scatteringColor = new JSONColor { r = 0.2f, g = 0.6f, b = 0.8f, a = 1.0f };
            
            deepStart = 0.5f;
            deepEnd = 10.0f;
            distortion = 32.0f;
            smoothness = 0.8f;
            intensity = 0.7f;
            numWaves = 4.0f;
            waveAmplitude = 0.3f;
            waveSteepness = 0.5f;
            waveSpeed = 1.0f;
            waveLength = 10.0f;
            waveScale = 1.0f;
        }
    }

    /// <summary>
    /// JSON representation of a water blocker entity for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONWaterBlockerEntity
    {
        public string type = "waterBlocker";
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;

        public JSONWaterBlockerEntity()
        {
            position = new JSONVector3 { x = 0.0f, y = 0.0f, z = 0.0f };
            rotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
            scale = new JSONVector3 { x = 1.0f, y = 1.0f, z = 1.0f };
        }
    }

    /// <summary>
    /// JSON representation of an audio entity for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONAudioEntity
    {
        public string type = "audio";
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public string audioFile; // Path to audio file
        public bool loop = false;
        public int priority = 128; // 0-256, lower is higher priority
        public float volume = 1.0f; // 0-1
        public float pitch = 1.0f; // -3 to 3
        public float stereoPan = 0.0f; // -1 to 1 (left to right)
        public bool playOnLoad = false; // Whether to start playing immediately after loading

        public JSONAudioEntity()
        {
            position = new JSONVector3 { x = 0.0f, y = 0.0f, z = 0.0f };
            rotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
            audioFile = "";
            loop = false;
            priority = 128;
            volume = 1.0f;
            pitch = 1.0f;
            stereoPan = 0.0f;
            playOnLoad = false;
        }
    }

    /// <summary>
    /// JSON representation of a character entity for serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class JSONCharacterEntity
    {
        public string type = "character";
        public string id;
        public string tag;
        public JSONVector3 position;
        public JSONQuaternion rotation;
        public JSONVector3 scale;
        public bool isSize = false;
        public string meshObject; // Path to the character mesh (GLTF file)
        public string[] meshResources; // Additional mesh resources
        public JSONVector3 meshOffset; // Offset for the character mesh
        public JSONQuaternion meshRotation; // Rotation for the character mesh
        public JSONVector3 avatarLabelOffset; // Offset for the character label
        public bool fixHeight = true; // Whether to fix height if below ground
        public bool checkForUpdateIfCached = true; // Whether to check for updates if cached

        public JSONCharacterEntity()
        {
            position = new JSONVector3 { x = 0.0f, y = 0.0f, z = 0.0f };
            rotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
            scale = new JSONVector3 { x = 1.0f, y = 1.0f, z = 1.0f };
            isSize = false;
            meshObject = "";
            meshResources = new string[0];
            meshOffset = new JSONVector3 { x = 0.0f, y = 0.0f, z = 0.0f };
            meshRotation = new JSONQuaternion { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
            avatarLabelOffset = new JSONVector3 { x = 0.0f, y = 2.0f, z = 0.0f };
            fixHeight = true;
            checkForUpdateIfCached = true;
        }
    }

    public class JSONEntityHandler : BaseHandler
    {
        /// <summary>
        /// Tracks whether the handler is initialized.
        /// </summary>
        private bool isInitialized = false;
        /// <summary>
        /// Parse JSON string into a JSONContainerEntity.
        /// </summary>
        /// <param name="jsonString">JSON string to parse.</param>
        /// <returns>Parsed JSONContainerEntity or null if parsing fails.</returns>
        public JSONContainerEntity ParseEntityFromJSON(string jsonString)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonString))
                {
                    Logging.LogError("[JSONEntityHandler->ParseEntityFromJSON] JSON string is null or empty.");
                    return null;
                }

                JSONContainerEntity entity = JsonConvert.DeserializeObject<JSONContainerEntity>(jsonString);
                
                if (!ValidateEntity(entity))
                {
                    Logging.LogError("[JSONEntityHandler->ParseEntityFromJSON] Entity validation failed.");
                    return null;
                }

                return entity;
            }
            catch (JsonException ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseEntityFromJSON] JSON parsing error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseEntityFromJSON] Unexpected error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Validate a JSONContainerEntity for required fields and proper structure.
        /// </summary>
        /// <param name="entity">Entity to validate.</param>
        /// <returns>True if entity is valid, false otherwise.</returns>
        private bool ValidateEntity(JSONContainerEntity entity)
        {
            if (entity == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateEntity] Entity is null.");
                return false;
            }

            // ID validation
            if (string.IsNullOrEmpty(entity.id))
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateEntity] Entity ID is null or empty. A new ID will be generated.");
            }
            else if (!Guid.TryParse(entity.id, out _))
            {
                Logging.LogError("[JSONEntityHandler->ValidateEntity] Entity ID is not a valid GUID format.");
                return false;
            }

            // Position validation
            if (entity.position == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateEntity] Position is null. Using Vector3.zero.");
                entity.position = new JSONVector3();
            }

            // Rotation validation
            if (entity.rotation == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateEntity] Rotation is null. Using Quaternion.identity.");
                entity.rotation = new JSONQuaternion() { w = 1 };
            }

            // Scale validation
            if (entity.scale == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateEntity] Scale is null. Using Vector3.one.");
                entity.scale = new JSONVector3() { x = 1, y = 1, z = 1 };
            }

            // Validate children recursively
            if (entity.children != null)
            {
                foreach (var child in entity.children)
                {
                    if (!ValidateEntity(child))
                    {
                        Logging.LogError("[JSONEntityHandler->ValidateEntity] Child entity validation failed.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Create a container entity from JSON data using the EntityManager.
        /// </summary>
        /// <param name="entity">JSONContainerEntity to create.</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root.</param>
        /// <param name="onComplete">Callback when creation is complete.</param>
        /// <returns>The GUID of the created entity, or null if creation failed.</returns>
        public Guid? CreateContainerEntity(JSONContainerEntity entity, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                if (entity == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateContainerEntity] Entity is null.");
                    onComplete?.Invoke(null, null);
                    return null;
                }

                if (StraightFour.StraightFour.ActiveWorld?.entityManager == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateContainerEntity] EntityManager not available.");
                    onComplete?.Invoke(null, null);
                    return null;
                }

                // Generate GUID if not provided
                Guid entityGuid;
                if (string.IsNullOrEmpty(entity.id))
                {
                    entityGuid = Guid.NewGuid();
                }
                else
                {
                    if (!Guid.TryParse(entity.id, out entityGuid))
                    {
                        Logging.LogError($"[JSONEntityHandler->CreateContainerEntity] Invalid GUID format: {entity.id}");
                        onComplete?.Invoke(null, null);
                        return null;
                    }
                }

                // Extract transform data
                Vector3 position = entity.position?.ToVector3() ?? Vector3.zero;
                Quaternion rotation = entity.rotation?.ToQuaternion() ?? Quaternion.identity;
                Vector3 scale = entity.scale?.ToVector3() ?? Vector3.one;

                // Callback for when the entity is loaded
                Action onEntityLoaded = () =>
                {
                    try
                    {
                        BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(entityGuid);
                        if (loadedEntity != null)
                        {
                            // Process children if any
                            if (entity.children != null && entity.children.Count > 0)
                            {
                                StartCoroutine(CreateChildrenCoroutine(entity.children, loadedEntity, () =>
                                {
                                    onComplete?.Invoke(entityGuid, loadedEntity);
                                }));
                            }
                            else
                            {
                                onComplete?.Invoke(entityGuid, loadedEntity);
                            }
                        }
                        else
                        {
                            Logging.LogError($"[JSONEntityHandler->CreateContainerEntity] Failed to find created entity: {entityGuid}");
                            onComplete?.Invoke(null, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError($"[JSONEntityHandler->CreateContainerEntity] Error in onEntityLoaded callback: {ex.Message}");
                        onComplete?.Invoke(null, null);
                    }
                };

                // Create the container entity using EntityManager
                Guid createdId = StraightFour.StraightFour.ActiveWorld.entityManager.LoadContainerEntity(
                    parentEntity: parentEntity,
                    position: position,
                    rotation: rotation,
                    scale: scale,
                    id: entityGuid,
                    tag: entity.tag,
                    isSize: entity.isSize,
                    onLoaded: onEntityLoaded
                );

                return createdId;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->CreateContainerEntity] Unexpected error: {ex.Message}");
                onComplete?.Invoke(null, null);
                return null;
            }
        }

        /// <summary>
        /// Coroutine to create child entities sequentially.
        /// </summary>
        /// <param name="children">List of child entities to create.</param>
        /// <param name="parentEntity">Parent entity for the children.</param>
        /// <param name="onComplete">Callback when all children are created.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        private IEnumerator CreateChildrenCoroutine(List<JSONContainerEntity> children, BaseEntity parentEntity, Action onComplete)
        {
            int completedChildren = 0;
            int totalChildren = children.Count;

            foreach (var child in children)
            {
                bool childComplete = false;
                
                CreateContainerEntity(child, parentEntity, (childId, childEntity) =>
                {
                    completedChildren++;
                    childComplete = true;
                    
                    if (childId == null)
                    {
                        Logging.LogWarning($"[JSONEntityHandler->CreateChildrenCoroutine] Failed to create child entity.");
                    }
                });

                // Wait for child creation to complete
                yield return new WaitUntil(() => childComplete);
            }

            // Wait for all children to be completed
            yield return new WaitUntil(() => completedChildren >= totalChildren);
            
            onComplete?.Invoke();
        }

        /// <summary>
        /// Load and create a container entity from a JSON string.
        /// </summary>
        /// <param name="jsonString">JSON string containing entity data.</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root.</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity).</param>
        public void LoadContainerEntityFromJSON(string jsonString, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadContainerEntityFromJSON] Starting JSON entity loading process.");

                // Parse JSON
                JSONContainerEntity entityData = ParseEntityFromJSON(jsonString);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadContainerEntityFromJSON] Failed to parse JSON data.");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                // Create entity
                Guid? entityId = CreateContainerEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadContainerEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadContainerEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadContainerEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadContainerEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Load and create a container entity from a JSON file.
        /// </summary>
        /// <param name="filePath">Path to JSON file containing entity data.</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root.</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity).</param>
        public void LoadContainerEntityFromFile(string filePath, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log($"[JSONEntityHandler->LoadContainerEntityFromFile] Loading entity from file: {filePath}");

                // Start coroutine to read file asynchronously
                StartCoroutine(LoadFileCoroutine(filePath, (jsonContent) =>
                {
                    if (string.IsNullOrEmpty(jsonContent))
                    {
                        Logging.LogError($"[JSONEntityHandler->LoadContainerEntityFromFile] Failed to read file or file is empty: {filePath}");
                        onComplete?.Invoke(false, null, null);
                        return;
                    }

                    // Load entity from JSON content
                    LoadContainerEntityFromJSON(jsonContent, parentEntity, onComplete);
                }));
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadContainerEntityFromFile] Error loading file {filePath}: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Coroutine to load file content asynchronously.
        /// </summary>
        /// <param name="filePath">Path to the file to read.</param>
        /// <param name="onComplete">Callback with file content.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        private IEnumerator LoadFileCoroutine(string filePath, Action<string> onComplete)
        {
            string jsonContent = null;
            Exception loadException = null;

            // Use a background thread for file I/O
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        jsonContent = System.IO.File.ReadAllText(filePath);
                    }
                    else
                    {
                        loadException = new System.IO.FileNotFoundException($"File not found: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    loadException = ex;
                }
            });

            // Wait for file loading to complete
            yield return new WaitUntil(() => jsonContent != null || loadException != null);

            if (loadException != null)
            {
                Logging.LogError($"[JSONEntityHandler->LoadFileCoroutine] Error reading file: {loadException.Message}");
                onComplete?.Invoke(null);
            }
            else
            {
                onComplete?.Invoke(jsonContent);
            }
        }

        /// <summary>
        /// Convert a container entity back to JSON string.
        /// </summary>
        /// <param name="entity">BaseEntity to convert.</param>
        /// <returns>JSON string representation or null if conversion fails.</returns>
        public string ConvertEntityToJSON(BaseEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    Logging.LogError("[JSONEntityHandler->ConvertEntityToJSON] Entity is null.");
                    return null;
                }

                if (!(entity is StraightFour.Entity.ContainerEntity containerEntity))
                {
                    Logging.LogError("[JSONEntityHandler->ConvertEntityToJSON] Entity is not a container entity.");
                    return null;
                }

                // Create JSON representation
                JSONContainerEntity jsonEntity = new JSONContainerEntity
                {
                    id = containerEntity.id.ToString(),
                    tag = containerEntity.entityTag,
                    position = new JSONVector3(containerEntity.transform.position),
                    rotation = new JSONQuaternion(containerEntity.transform.rotation),
                    scale = new JSONVector3(containerEntity.transform.localScale),
                    isSize = false // Default assumption; you might want to track this differently
                };

                // Serialize to JSON
                return JsonConvert.SerializeObject(jsonEntity, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->ConvertEntityToJSON] Error converting entity to JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if the handler is ready to process entities.
        /// </summary>
        /// <returns>True if ready, false otherwise.</returns>
        public bool IsReady()
        {
            if (StraightFour.StraightFour.ActiveWorld == null)
            {
                Logging.LogError("[JSONEntityHandler->IsReady] No active world available.");
                return false;
            }

            if (StraightFour.StraightFour.ActiveWorld.entityManager == null)
            {
                Logging.LogError("[JSONEntityHandler->IsReady] EntityManager not available.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate JSON string format before processing.
        /// </summary>
        /// <param name="jsonString">JSON string to validate.</param>
        /// <returns>True if valid JSON, false otherwise.</returns>
        public bool ValidateJSONFormat(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                Logging.LogError("[JSONEntityHandler->ValidateJSONFormat] JSON string is null or empty.");
                return false;
            }

            try
            {
                JsonConvert.DeserializeObject(jsonString);
                return true;
            }
            catch (JsonException ex)
            {
                Logging.LogError($"[JSONEntityHandler->ValidateJSONFormat] Invalid JSON format: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get statistics about the handler's current state.
        /// </summary>
        /// <returns>Dictionary containing handler statistics.</returns>
        public Dictionary<string, object> GetHandlerStats()
        {
            var stats = new Dictionary<string, object>
            {
                { "IsInitialized", isInitialized },
                { "IsReady", IsReady() },
                { "ActiveWorldExists", StraightFour.StraightFour.ActiveWorld != null },
                { "EntityManagerExists", StraightFour.StraightFour.ActiveWorld?.entityManager != null }
            };

            if (StraightFour.StraightFour.ActiveWorld?.entityManager != null)
            {
                // Add entity count if available through reflection or public API
                stats.Add("EntityManagerActive", true);
            }
            else
            {
                stats.Add("EntityManagerActive", false);
            }

            return stats;
        }

        public override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
            
            // Validate dependencies
            /*if (!IsReady())
            {
                Logging.LogWarning("[JSONEntityHandler->Initialize] Handler initialized but dependencies not ready. Some operations may fail until EntityManager is available.");
            }
            else
            {
                Logging.Log("[JSONEntityHandler->Initialize] Handler initialized and ready for entity processing.");
            }*/
        }

        public override void Terminate()
        {
            try
            {
                Logging.Log("[JSONEntityHandler->Terminate] Terminating JSON Entity Handler.");
                
                // Stop any running coroutines
                StopAllCoroutines();
                
                isInitialized = false;
                base.Terminate();
                
                Logging.Log("[JSONEntityHandler->Terminate] JSON Entity Handler terminated successfully.");
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->Terminate] Error during termination: {ex.Message}");
                isInitialized = false;
                base.Terminate();
            }
        }

        /// <summary>
        /// High-level API method to process JSON entity with full validation.
        /// This is the recommended entry point for most use cases.
        /// </summary>
        /// <param name="jsonString">JSON string containing entity data.</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root.</param>
        /// <param name="onSuccess">Callback for successful entity creation (entityId, createdEntity).</param>
        /// <param name="onError">Callback for error cases (errorMessage).</param>
        public void ProcessContainerEntityJSON(string jsonString, BaseEntity parentEntity = null, 
            Action<Guid, BaseEntity> onSuccess = null, Action<string> onError = null)
        {
            try
            {
                // Pre-flight checks
                if (!isInitialized)
                {
                    string error = "[JSONEntityHandler->ProcessContainerEntityJSON] Handler not initialized.";
                    Logging.LogError(error);
                    onError?.Invoke(error);
                    return;
                }

                if (!IsReady())
                {
                    string error = "[JSONEntityHandler->ProcessContainerEntityJSON] Handler not ready - EntityManager unavailable.";
                    Logging.LogError(error);
                    onError?.Invoke(error);
                    return;
                }

                if (!ValidateJSONFormat(jsonString))
                {
                    string error = "[JSONEntityHandler->ProcessContainerEntityJSON] Invalid JSON format.";
                    Logging.LogError(error);
                    onError?.Invoke(error);
                    return;
                }

                // Process the entity
                LoadContainerEntityFromJSON(jsonString, parentEntity, (success, entityId, createdEntity) =>
                {
                    if (success && entityId.HasValue && createdEntity != null)
                    {
                        Logging.Log($"[JSONEntityHandler->ProcessContainerEntityJSON] Successfully processed entity: {entityId}");
                        onSuccess?.Invoke(entityId.Value, createdEntity);
                    }
                    else
                    {
                        string error = "[JSONEntityHandler->ProcessContainerEntityJSON] Failed to create entity from JSON data.";
                        Logging.LogError(error);
                        onError?.Invoke(error);
                    }
                });
            }
            catch (Exception ex)
            {
                string error = $"[JSONEntityHandler->ProcessContainerEntityJSON] Unexpected error: {ex.Message}";
                Logging.LogError(error);
                onError?.Invoke(error);
            }
        }

        #region Mesh Entity Methods

        /// <summary>
        /// Parse JSON string into a JSONMeshEntity.
        /// </summary>
        /// <param name="jsonString">JSON string to parse.</param>
        /// <returns>Parsed JSONMeshEntity or null if parsing fails.</returns>
        public JSONMeshEntity ParseMeshEntityFromJSON(string jsonString)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonString))
                {
                    Logging.LogError("[JSONEntityHandler->ParseMeshEntityFromJSON] JSON string is null or empty.");
                    return null;
                }

                JSONMeshEntity entity = JsonConvert.DeserializeObject<JSONMeshEntity>(jsonString);
                
                if (!ValidateMeshEntity(entity))
                {
                    Logging.LogError("[JSONEntityHandler->ParseMeshEntityFromJSON] Mesh entity validation failed.");
                    return null;
                }

                return entity;
            }
            catch (JsonException ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseMeshEntityFromJSON] JSON parsing error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseMeshEntityFromJSON] Unexpected error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Validate a JSONMeshEntity for required fields and proper structure.
        /// </summary>
        /// <param name="entity">Mesh entity to validate.</param>
        /// <returns>True if entity is valid, false otherwise.</returns>
        private bool ValidateMeshEntity(JSONMeshEntity entity)
        {
            if (entity == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateMeshEntity] Entity is null.");
                return false;
            }

            // ID validation
            if (string.IsNullOrEmpty(entity.id))
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateMeshEntity] Entity ID is null or empty. A new ID will be generated.");
            }
            else if (!Guid.TryParse(entity.id, out _))
            {
                Logging.LogError("[JSONEntityHandler->ValidateMeshEntity] Entity ID is not a valid GUID format.");
                return false;
            }

            // Mesh type validation
            if (string.IsNullOrEmpty(entity.meshType))
            {
                Logging.LogError("[JSONEntityHandler->ValidateMeshEntity] Mesh type is required.");
                return false;
            }

            string[] validMeshTypes = { "primitive", "gltf" };
            if (!validMeshTypes.Contains(entity.meshType.ToLower()))
            {
                Logging.LogError($"[JSONEntityHandler->ValidateMeshEntity] Invalid mesh type: {entity.meshType}. Valid types: {string.Join(", ", validMeshTypes)}");
                return false;
            }

            // Mesh source validation
            if (string.IsNullOrEmpty(entity.meshSource))
            {
                Logging.LogError("[JSONEntityHandler->ValidateMeshEntity] Mesh source is required.");
                return false;
            }

            // Primitive type validation
            if (entity.meshType.ToLower() == "primitive")
            {
                string[] validPrimitives = { "cube", "sphere", "capsule", "cylinder", "plane", "torus", "cone", "rectangularpyramid" };
                if (!validPrimitives.Contains(entity.meshSource.ToLower()))
                {
                    Logging.LogError($"[JSONEntityHandler->ValidateMeshEntity] Invalid primitive type: {entity.meshSource}. Valid types: {string.Join(", ", validPrimitives)}");
                    return false;
                }
            }

            // Position validation
            if (entity.position == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateMeshEntity] Position is null. Using Vector3.zero.");
                entity.position = new JSONVector3();
            }

            // Rotation validation
            if (entity.rotation == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateMeshEntity] Rotation is null. Using Quaternion.identity.");
                entity.rotation = new JSONQuaternion() { w = 1 };
            }

            // Scale validation
            if (entity.scale == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateMeshEntity] Scale is null. Using Vector3.one.");
                entity.scale = new JSONVector3() { x = 1, y = 1, z = 1 };
            }

            // Validate children recursively
            if (entity.children != null)
            {
                foreach (var child in entity.children)
                {
                    if (!ValidateMeshEntity(child))
                    {
                        Logging.LogError("[JSONEntityHandler->ValidateMeshEntity] Child mesh entity validation failed.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Create a mesh entity from JSON data using the EntityManager.
        /// </summary>
        /// <param name="entity">JSONMeshEntity to create.</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root.</param>
        /// <param name="onComplete">Callback when creation is complete.</param>
        /// <returns>The GUID of the created entity, or null if creation failed.</returns>
        public Guid? CreateMeshEntity(JSONMeshEntity entity, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                if (entity == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateMeshEntity] Entity is null.");
                    onComplete?.Invoke(null, null);
                    return null;
                }

                if (StraightFour.StraightFour.ActiveWorld?.entityManager == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateMeshEntity] EntityManager not available.");
                    onComplete?.Invoke(null, null);
                    return null;
                }

                // Generate GUID if not provided
                Guid entityGuid;
                if (string.IsNullOrEmpty(entity.id))
                {
                    entityGuid = Guid.NewGuid();
                }
                else
                {
                    if (!Guid.TryParse(entity.id, out entityGuid))
                    {
                        Logging.LogError($"[JSONEntityHandler->CreateMeshEntity] Invalid GUID format: {entity.id}");
                        onComplete?.Invoke(null, null);
                        return null;
                    }
                }

                // Extract transform data
                Vector3 position = entity.position?.ToVector3() ?? Vector3.zero;
                Quaternion rotation = entity.rotation?.ToQuaternion() ?? Quaternion.identity;
                Vector3 scale = entity.scale?.ToVector3() ?? Vector3.one;

                // Handle different mesh types
                switch (entity.meshType.ToLower())
                {
                    case "primitive":
                        return CreatePrimitiveMeshEntity(entity, entityGuid, parentEntity, position, rotation, scale, onComplete);
                    
                    case "gltf":
                        return CreateGLTFMeshEntity(entity, entityGuid, parentEntity, position, rotation, scale, onComplete);
                    
                    default:
                        Logging.LogError($"[JSONEntityHandler->CreateMeshEntity] Unsupported mesh type: {entity.meshType}");
                        onComplete?.Invoke(null, null);
                        return null;
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->CreateMeshEntity] Unexpected error: {ex.Message}");
                onComplete?.Invoke(null, null);
                return null;
            }
        }

        /// <summary>
        /// Create a primitive mesh entity using MeshEntity static creation methods directly.
        /// </summary>
        private Guid? CreatePrimitiveMeshEntity(JSONMeshEntity entity, Guid entityGuid, BaseEntity parentEntity, 
            Vector3 position, Quaternion rotation, Vector3 scale, Action<Guid?, BaseEntity> onComplete)
        {
            try
            {
                // Convert parent to API BaseEntity
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
                if (parentEntity != null)
                {
                    // Convert internal entity to API entity by finding existing mapping
                    var internalEntity = parentEntity as StraightFour.Entity.BaseEntity;
                    if (internalEntity != null)
                    {
                        apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(internalEntity);
                    }
                }

                // Convert to WorldTypes
                var apiColor = entity.color?.ToColor() ?? Color.white;
                var worldColor = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(
                    apiColor.r, apiColor.g, apiColor.b, apiColor.a);
                
                var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                    position.x, position.y, position.z);
                
                var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                    rotation.x, rotation.y, rotation.z, rotation.w);

                // Call the appropriate MeshEntity static method
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity meshEntity = 
                    CreatePrimitiveEntityByType(entity.meshSource, apiParent, worldColor, worldPosition, worldRotation, entityGuid.ToString());

                if (meshEntity == null)
                {
                    Logging.LogError($"[JSONEntityHandler->CreatePrimitiveMeshEntity] Failed to create primitive: {entity.meshSource}");
                    onComplete?.Invoke(null, null);
                    return null;
                }

                // Apply scale after creation
                if (entity.scale != null)
                {
                    var worldScale = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(scale.x, scale.y, scale.z);
                    if (entity.isSize)
                    {
                        meshEntity.SetSize(worldScale, true);
                    }
                    else
                    {
                        meshEntity.SetScale(worldScale, true);
                    }
                }

                // Set tag if specified
                if (!string.IsNullOrEmpty(entity.tag))
                {
                    meshEntity.tag = entity.tag;
                }

                Logging.Log($"[JSONEntityHandler->CreatePrimitiveMeshEntity] Successfully created primitive entity: {entityGuid}");

                // Get the internal entity for the callback
                var internalMeshEntity = meshEntity.internalEntity;

                // Process children if any
                if (entity.children != null && entity.children.Count > 0)
                {
                    StartCoroutine(CreateMeshChildrenCoroutine(entity.children, internalMeshEntity, () =>
                    {
                        onComplete?.Invoke(entityGuid, internalMeshEntity);
                    }));
                }
                else
                {
                    onComplete?.Invoke(entityGuid, internalMeshEntity);
                }

                return entityGuid;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->CreatePrimitiveMeshEntity] Error creating primitive mesh: {ex.Message}");
                onComplete?.Invoke(null, null);
                return null;
            }
        }

        /// <summary>
        /// Create the appropriate primitive mesh entity using MeshEntity static methods.
        /// </summary>
        private FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity CreatePrimitiveEntityByType(
            string meshSource, FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity parent, 
            FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color color, 
            FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3 position, 
            FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion rotation, string id)
        {
            switch (meshSource.ToLower())
            {
                case "cube": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreateCube(parent, color, position, rotation, id);
                case "sphere": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreateSphere(parent, color, position, rotation, id);
                case "cylinder": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreateCylinder(parent, color, position, rotation, id);
                case "capsule": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreateCapsule(parent, color, position, rotation, id);
                case "plane": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreatePlane(parent, color, position, rotation, id);
                case "torus": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreateTorus(parent, color, position, rotation, id);
                case "cone": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreateCone(parent, color, position, rotation, id);
                case "rectangularpyramid": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreateRectangularPyramid(parent, color, position, rotation, id);
                case "tetrahedron": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreateTetrahedron(parent, color, position, rotation, id);
                case "prism": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreatePrism(parent, color, position, rotation, id);
                case "arch": 
                    return FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.MeshEntity.CreateArch(parent, color, position, rotation, id);
                default: 
                    return null;
            }
        }



        /// <summary>
        /// Create a GLTF mesh entity using GLTFHandler.
        /// </summary>
        private Guid? CreateGLTFMeshEntity(JSONMeshEntity entity, Guid entityGuid, BaseEntity parentEntity, 
            Vector3 position, Quaternion rotation, Vector3 scale, Action<Guid?, BaseEntity> onComplete)
        {
            try
            {
                if (WebVerseRuntime.Instance?.gltfHandler == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateGLTFMeshEntity] GLTFHandler not available.");
                    onComplete?.Invoke(null, null);
                    return null;
                }

                // Use the existing GLTF loading infrastructure
                WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsMeshEntity(
                    entity.meshSource, 
                    entity.meshResources, 
                    entityGuid, 
                    (meshEntity) =>
                    {
                        if (meshEntity != null)
                        {
                            // Set position, rotation, and parent
                            meshEntity.SetPosition(position, true);
                            meshEntity.SetRotation(rotation, true);
                            meshEntity.SetScale(scale, true);
                            
                            if (parentEntity != null)
                            {
                                meshEntity.SetParent(parentEntity);
                            }
                            
                            meshEntity.entityTag = entity.tag;

                            Logging.Log($"[JSONEntityHandler->CreateGLTFMeshEntity] Successfully created GLTF entity: {entityGuid}");
                            
                            // Process children if any
                            if (entity.children != null && entity.children.Count > 0)
                            {
                                StartCoroutine(CreateMeshChildrenCoroutine(entity.children, meshEntity, () =>
                                {
                                    onComplete?.Invoke(entityGuid, meshEntity);
                                }));
                            }
                            else
                            {
                                onComplete?.Invoke(entityGuid, meshEntity);
                            }
                        }
                        else
                        {
                            Logging.LogError($"[JSONEntityHandler->CreateGLTFMeshEntity] Failed to load GLTF mesh: {entity.meshSource}");
                            onComplete?.Invoke(null, null);
                        }
                    }, 
                    10f // timeout
                );

                return entityGuid;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->CreateGLTFMeshEntity] Error creating GLTF mesh: {ex.Message}");
                onComplete?.Invoke(null, null);
                return null;
            }
        }



        /// <summary>
        /// Coroutine to create mesh entity children sequentially.
        /// </summary>
        private IEnumerator CreateMeshChildrenCoroutine(List<JSONMeshEntity> children, BaseEntity parentEntity, Action onComplete)
        {
            int completedChildren = 0;
            int totalChildren = children.Count;

            foreach (var child in children)
            {
                bool childComplete = false;
                
                CreateMeshEntity(child, parentEntity, (childId, childEntity) =>
                {
                    completedChildren++;
                    childComplete = true;
                    
                    if (childId == null)
                    {
                        Logging.LogWarning($"[JSONEntityHandler->CreateMeshChildrenCoroutine] Failed to create child mesh entity.");
                    }
                });

                // Wait for child creation to complete
                yield return new WaitUntil(() => childComplete);
            }

            // Wait for all children to be completed
            yield return new WaitUntil(() => completedChildren >= totalChildren);
            
            Logging.Log($"[JSONEntityHandler->CreateMeshChildrenCoroutine] Completed creating {completedChildren}/{totalChildren} child mesh entities.");
            onComplete?.Invoke();
        }

        /// <summary>
        /// Load and create a mesh entity from a JSON string.
        /// </summary>
        /// <param name="jsonString">JSON string containing mesh entity data.</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root.</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity).</param>
        public void LoadMeshEntityFromJSON(string jsonString, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadMeshEntityFromJSON] Starting JSON mesh entity loading process.");

                // Parse JSON
                JSONMeshEntity entityData = ParseMeshEntityFromJSON(jsonString);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadMeshEntityFromJSON] Failed to parse JSON data.");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                // Create entity
                Guid? entityId = CreateMeshEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadMeshEntityFromJSON] Successfully loaded mesh entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadMeshEntityFromJSON] Failed to create mesh entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadMeshEntityFromJSON] Failed to initiate mesh entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadMeshEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// High-level API method to process JSON mesh entity with full validation.
        /// This is the recommended entry point for most mesh entity use cases.
        /// </summary>
        /// <param name="jsonString">JSON string containing mesh entity data.</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root.</param>
        /// <param name="onSuccess">Callback for successful entity creation (entityId, createdEntity).</param>
        /// <param name="onError">Callback for error cases (errorMessage).</param>
        public void ProcessMeshEntityJSON(string jsonString, BaseEntity parentEntity = null, 
            Action<Guid, BaseEntity> onSuccess = null, Action<string> onError = null)
        {
            try
            {
                // Pre-flight checks
                if (!isInitialized)
                {
                    string error = "[JSONEntityHandler->ProcessMeshEntityJSON] Handler not initialized.";
                    Logging.LogError(error);
                    onError?.Invoke(error);
                    return;
                }

                if (!IsReady())
                {
                    string error = "[JSONEntityHandler->ProcessMeshEntityJSON] Handler not ready - EntityManager unavailable.";
                    Logging.LogError(error);
                    onError?.Invoke(error);
                    return;
                }

                if (!ValidateJSONFormat(jsonString))
                {
                    string error = "[JSONEntityHandler->ProcessMeshEntityJSON] Invalid JSON format.";
                    Logging.LogError(error);
                    onError?.Invoke(error);
                    return;
                }

                // Process the entity
                LoadMeshEntityFromJSON(jsonString, parentEntity, (success, entityId, createdEntity) =>
                {
                    if (success && entityId.HasValue && createdEntity != null)
                    {
                        Logging.Log($"[JSONEntityHandler->ProcessMeshEntityJSON] Successfully processed mesh entity: {entityId}");
                        onSuccess?.Invoke(entityId.Value, createdEntity);
                    }
                    else
                    {
                        string error = "[JSONEntityHandler->ProcessMeshEntityJSON] Failed to create mesh entity from JSON data.";
                        Logging.LogError(error);
                        onError?.Invoke(error);
                    }
                });
            }
            catch (Exception ex)
            {
                string error = $"[JSONEntityHandler->ProcessMeshEntityJSON] Unexpected error: {ex.Message}";
                Logging.LogError(error);
                onError?.Invoke(error);
            }
        }

        /// <summary>
        /// Parse JSON string containing a collection of mesh entities.
        /// Expected format: {"mesh-entities":[<mesh-entity-format>,...]}
        /// </summary>
        /// <param name="jsonString">JSON string containing mesh entity collection.</param>
        /// <returns>Parsed JSONMeshEntityCollection or null if parsing fails.</returns>
        public JSONMeshEntityCollection ParseMeshEntityCollectionFromJSON(string jsonString)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonString))
                {
                    Logging.LogError("[JSONEntityHandler->ParseMeshEntityCollectionFromJSON] JSON string is null or empty.");
                    return null;
                }

                JSONMeshEntityCollection collection = JsonConvert.DeserializeObject<JSONMeshEntityCollection>(jsonString);
                
                if (collection == null || collection.meshEntities == null)
                {
                    Logging.LogError("[JSONEntityHandler->ParseMeshEntityCollectionFromJSON] Failed to parse mesh entity collection or collection is empty.");
                    return null;
                }

                // Validate each mesh entity in the collection
                foreach (var entity in collection.meshEntities)
                {
                    if (!ValidateMeshEntity(entity))
                    {
                        Logging.LogError("[JSONEntityHandler->ParseMeshEntityCollectionFromJSON] One or more mesh entities in collection failed validation.");
                        return null;
                    }
                }

                return collection;
            }
            catch (JsonException ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseMeshEntityCollectionFromJSON] JSON parsing error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseMeshEntityCollectionFromJSON] Unexpected error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a collection of mesh entities from JSON data using the EntityManager.
        /// </summary>
        /// <param name="collection">JSONMeshEntityCollection to create.</param>
        /// <param name="parentEntity">Parent entity to attach all mesh entities to, or null for world root.</param>
        /// <param name="onComplete">Callback when creation is complete with list of created entity GUIDs and entities.</param>
        /// <returns>True if creation process started successfully, false otherwise.</returns>
        public bool CreateMeshEntityCollection(JSONMeshEntityCollection collection, BaseEntity parentEntity = null, 
            Action<List<Guid?>, List<BaseEntity>> onComplete = null)
        {
            try
            {
                if (collection == null || collection.meshEntities == null || collection.meshEntities.Length == 0)
                {
                    Logging.LogError("[JSONEntityHandler->CreateMeshEntityCollection] Collection is null or empty.");
                    onComplete?.Invoke(new List<Guid?>(), new List<BaseEntity>());
                    return false;
                }

                if (StraightFour.StraightFour.ActiveWorld?.entityManager == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateMeshEntityCollection] EntityManager not available.");
                    onComplete?.Invoke(new List<Guid?>(), new List<BaseEntity>());
                    return false;
                }

                // Start coroutine to create entities sequentially
                StartCoroutine(CreateMeshEntitiesCoroutine(collection.meshEntities, parentEntity, onComplete));
                return true;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->CreateMeshEntityCollection] Unexpected error: {ex.Message}");
                onComplete?.Invoke(new List<Guid?>(), new List<BaseEntity>());
                return false;
            }
        }

        /// <summary>
        /// Coroutine to create mesh entities sequentially from a collection.
        /// </summary>
        /// <param name="meshEntities">Array of mesh entities to create.</param>
        /// <param name="parentEntity">Parent entity for all mesh entities.</param>
        /// <param name="onComplete">Callback when all entities are created.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        private IEnumerator CreateMeshEntitiesCoroutine(JSONMeshEntity[] meshEntities, BaseEntity parentEntity, 
            Action<List<Guid?>, List<BaseEntity>> onComplete)
        {
            List<Guid?> createdIds = new List<Guid?>();
            List<BaseEntity> createdEntities = new List<BaseEntity>();
            int completedEntities = 0;
            int totalEntities = meshEntities.Length;

            foreach (var meshEntity in meshEntities)
            {
                bool entityComplete = false;
                
                CreateMeshEntity(meshEntity, parentEntity, (entityId, entity) =>
                {
                    completedEntities++;
                    
                    if (entityId.HasValue && entity != null)
                    {
                        createdIds.Add(entityId.Value);
                        createdEntities.Add(entity);
                        Logging.Log($"[JSONEntityHandler->CreateMeshEntitiesCoroutine] Successfully created mesh entity {entityId.Value} ({completedEntities}/{totalEntities})");
                    }
                    else
                    {
                        Logging.LogWarning($"[JSONEntityHandler->CreateMeshEntitiesCoroutine] Failed to create mesh entity ({completedEntities}/{totalEntities})");
                    }
                    
                    entityComplete = true;
                });

                // Wait for entity creation to complete
                yield return new WaitUntil(() => entityComplete);
            }

            // Wait for all entities to be completed
            yield return new WaitUntil(() => completedEntities >= totalEntities);
            
            Logging.Log($"[JSONEntityHandler->CreateMeshEntitiesCoroutine] Completed creating {createdIds.Count}/{totalEntities} mesh entities from collection.");
            onComplete?.Invoke(createdIds, createdEntities);
        }

        /// <summary>
        /// Load and create a collection of mesh entities from a JSON string.
        /// Expected format: {"mesh-entities":[<mesh-entity-format>,...]}
        /// </summary>
        /// <param name="jsonString">JSON string containing mesh entity collection data.</param>
        /// <param name="parentEntity">Parent entity to attach all mesh entities to, or null for world root.</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityIds, createdEntities).</param>
        public void LoadMeshEntityCollectionFromJSON(string jsonString, BaseEntity parentEntity = null, 
            Action<bool, List<Guid?>, List<BaseEntity>> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadMeshEntityCollectionFromJSON] Starting mesh entity collection loading process.");

                // Parse JSON
                JSONMeshEntityCollection collectionData = ParseMeshEntityCollectionFromJSON(jsonString);
                if (collectionData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadMeshEntityCollectionFromJSON] Failed to parse JSON collection data.");
                    onComplete?.Invoke(false, new List<Guid?>(), new List<BaseEntity>());
                    return;
                }

                // Create entities
                bool success = CreateMeshEntityCollection(collectionData, parentEntity, (createdIds, createdEntities) =>
                {
                    bool collectionSuccess = createdIds.Count > 0;
                    if (collectionSuccess)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadMeshEntityCollectionFromJSON] Successfully created {createdIds.Count} mesh entities from collection.");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadMeshEntityCollectionFromJSON] Failed to create any mesh entities from collection.");
                    }
                    onComplete?.Invoke(collectionSuccess, createdIds, createdEntities);
                });

                if (!success)
                {
                    Logging.LogError("[JSONEntityHandler->LoadMeshEntityCollectionFromJSON] Failed to initiate mesh entity collection creation.");
                    onComplete?.Invoke(false, new List<Guid?>(), new List<BaseEntity>());
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadMeshEntityCollectionFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, new List<Guid?>(), new List<BaseEntity>());
            }
        }

        /// <summary>
        /// High-level API method to process mesh entity collection JSON with full validation.
        /// This is the recommended entry point for most use cases.
        /// Expected format: {"mesh-entities":[<mesh-entity-format>,...]}
        /// </summary>
        /// <param name="jsonString">JSON string containing mesh entity collection data.</param>
        /// <param name="parentEntity">Parent entity to attach all mesh entities to, or null for world root.</param>
        /// <param name="onSuccess">Callback for successful mesh entity collection creation (entityIds, createdEntities).</param>
        public void ProcessMeshEntityCollectionJSON(string jsonString, BaseEntity parentEntity = null, 
            Action<List<Guid?>, List<BaseEntity>> onSuccess = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->ProcessMeshEntityCollectionJSON] Processing mesh entity collection JSON...");
                
                if (!isInitialized)
                {
                    Logging.LogError("[JSONEntityHandler->ProcessMeshEntityCollectionJSON] Handler not initialized.");
                    return;
                }

                if (!IsReady())
                {
                    Logging.LogError("[JSONEntityHandler->ProcessMeshEntityCollectionJSON] Handler not ready - EntityManager unavailable.");
                    return;
                }

                if (!ValidateJSONFormat(jsonString))
                {
                    Logging.LogError("[JSONEntityHandler->ProcessMeshEntityCollectionJSON] Invalid JSON format.");
                    return;
                }

                LoadMeshEntityCollectionFromJSON(jsonString, parentEntity, (success, entityIds, entities) =>
                {
                    if (success && entityIds.Count > 0)
                    {
                        Logging.Log($"[JSONEntityHandler->ProcessMeshEntityCollectionJSON] Successfully processed {entityIds.Count} mesh entities from collection.");
                        onSuccess?.Invoke(entityIds, entities);
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->ProcessMeshEntityCollectionJSON] Failed to create mesh entity collection from JSON data.");
                    }
                });
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->ProcessMeshEntityCollectionJSON] Unexpected error: {ex.Message}");
            }
        }

        #endregion

        #region Terrain Entity Methods

        /// <summary>
        /// Process a terrain entity JSON string with full validation and error handling (Recommended).
        /// This method now uses background threading for optimal performance:
        /// - Phase 1: JSON parsing in background thread
        /// - Phase 2: Data normalization in background thread  
        /// - Phase 3: Modifications pre-processing in background thread
        /// - Phase 4: Unity entity creation on main thread
        /// </summary>
        /// <param name="jsonString">JSON string containing the terrain entity definition</param>
        /// <param name="parentEntity">Parent entity (optional)</param>
        /// <param name="onSuccess">Success callback with entity ID and created entity</param>
        /// <param name="onError">Error callback with error message</param>
        public void ProcessTerrainEntityJSON(string jsonString, BaseEntity parentEntity = null,
            System.Action<System.Guid, BaseEntity> onSuccess = null, System.Action<string> onError = null)
        {
            try
            {
                if (!isInitialized)
                {
                    string error = "[JSONEntityHandler->ProcessTerrainEntityJSON] Handler not initialized.";
                    Logging.LogError(error);
                    onError?.Invoke(error);
                    return;
                }

                if (!IsReady())
                {
                    string error = "[JSONEntityHandler->ProcessTerrainEntityJSON] Handler not ready - EntityManager unavailable.";
                    Logging.LogError(error);
                    onError?.Invoke(error);
                    return;
                }

                if (!ValidateJSONFormat(jsonString))
                {
                    string error = "[JSONEntityHandler->ProcessTerrainEntityJSON] Invalid JSON format.";
                    Logging.LogError(error);
                    onError?.Invoke(error);
                    return;
                }

                LoadTerrainEntityFromJSON(jsonString, parentEntity, (success, entityId, entity) =>
                {
                    if (success && entityId.HasValue)
                    {
                        Logging.Log($"[JSONEntityHandler->ProcessTerrainEntityJSON] Successfully processed terrain entity: {entityId}");
                        onSuccess?.Invoke(entityId.Value, entity);
                    }
                    else
                    {
                        string error = "[JSONEntityHandler->ProcessTerrainEntityJSON] Failed to create terrain entity from JSON data.";
                        Logging.LogError(error);
                        onError?.Invoke(error);
                    }
                });
            }
            catch (Exception ex)
            {
                string error = $"[JSONEntityHandler->ProcessTerrainEntityJSON] Unexpected error: {ex.Message}";
                Logging.LogError(error);
                onError?.Invoke(error);
            }
        }

        /// <summary>
        /// Load a terrain entity from JSON string using background processing.
        /// This method processes JSON parsing and data normalization in background threads.
        /// If background processing fails, it falls back to coroutine-based processing.
        /// </summary>
        /// <param name="jsonString">JSON string containing the terrain entity definition</param>
        /// <param name="parentEntity">Parent entity (optional)</param>
        /// <param name="onComplete">Callback with success status, entity ID, and created entity</param>
        public void LoadTerrainEntityFromJSON(string jsonString, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            // Try background processing first, with fallback to main thread coroutine
            StartCoroutine(LoadTerrainEntityFromJSONAsync(jsonString, parentEntity, onComplete));
        }

        /// <summary>
        /// Coroutine that handles background processing of terrain JSON data.
        /// </summary>
        /// <param name="jsonString">JSON string to process</param>
        /// <param name="parentEntity">Parent entity (optional)</param>
        /// <param name="onComplete">Completion callback</param>
        /// <returns>IEnumerator for coroutine</returns>
        private IEnumerator LoadTerrainEntityFromJSONAsync(string jsonString, BaseEntity parentEntity, Action<bool, Guid?, BaseEntity> onComplete)
        {
            JSONTerrainEntity terrainData = null;
            Exception parseException = null;
            bool parsingComplete = false;

            // Phase 1: Parse JSON in background thread
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // Note: Cannot use Unity Logging in background thread
                    System.Console.WriteLine("[Background] Starting JSON parsing...");
                    System.Console.WriteLine($"[Background] JSON length: {jsonString?.Length ?? 0} characters");
                    
                    terrainData = JSONEntityHandler.ParseTerrainEntityFromJSONThreadSafe(jsonString);
                    
                    if (terrainData != null)
                    {
                        System.Console.WriteLine($"[Background] JSON parsing completed successfully. Terrain type: {terrainData.terrainType}");
                    }
                    else
                    {
                        System.Console.WriteLine("[Background] JSON parsing returned null result");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[Background] JSON parsing failed: {ex.GetType().Name}: {ex.Message}");
                    System.Console.WriteLine($"[Background] Stack trace: {ex.StackTrace}");
                    parseException = ex;
                }
                finally
                {
                    parsingComplete = true;
                }
            });

            // Wait for parsing to complete
            yield return new WaitUntil(() => parsingComplete);

            if (parseException != null)
            {
                // Check if this is a Unity main thread issue
                if (parseException.Message.Contains("can only be called from the main thread"))
                {
                    Logging.LogWarning("[JSONEntityHandler->LoadTerrainEntityFromJSONAsync] Background processing failed due to Unity main thread requirements. Falling back to coroutine-based processing.");
                    // Switch to coroutine-based processing
                    StartCoroutine(LoadTerrainEntityFromJSONCoroutine(jsonString, parentEntity, onComplete));
                    yield break;
                }
                
                Logging.LogError($"[JSONEntityHandler->LoadTerrainEntityFromJSONAsync] JSON parsing failed: {parseException.Message}");
                onComplete?.Invoke(false, null, null);
                yield break;
            }

            if (terrainData == null)
            {
                string detailedError = parseException != null ? 
                    $"Parsing failed with error: {parseException.Message}" : 
                    "Parsing returned null result (unknown reason)";
                
                Logging.LogError($"[JSONEntityHandler->LoadTerrainEntityFromJSONAsync] Failed to parse terrain data from JSON. {detailedError}");
                
                // If it's a Unity main thread issue, fall back to coroutine processing
                if (parseException != null && parseException.Message.Contains("can only be called from the main thread"))
                {
                    Logging.LogWarning("[JSONEntityHandler->LoadTerrainEntityFromJSONAsync] Detected Unity main thread issue. Falling back to coroutine-based processing.");
                    StartCoroutine(LoadTerrainEntityFromJSONCoroutine(jsonString, parentEntity, onComplete));
                    yield break;
                }
                
                onComplete?.Invoke(false, null, null);
                yield break;
            }

            // Phase 2: Validate on main thread (quick operation)
            if (!ValidateTerrainEntity(terrainData))
            {
                Logging.LogError("[JSONEntityHandler->LoadTerrainEntityFromJSONAsync] Terrain entity validation failed.");
                onComplete?.Invoke(false, null, null);
                yield break;
            }

            // Phase 3: Normalize data and pre-process modifications in background thread
            JSONTerrainEntity normalizedData = null;
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[] apiModifications = null;
            Exception processingException = null;
            bool processingComplete = false;

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // Note: Cannot use Unity Logging in background thread
                    System.Console.WriteLine("[Background] Starting data normalization...");
                    normalizedData = JSONEntityHandler.NormalizeTerrainDataThreadSafe(terrainData);
                    System.Console.WriteLine("[Background] Data normalization completed.");
                    
                    // Pre-process modifications for hybrid terrain
                    if (normalizedData.terrainType == "hybrid" && normalizedData.modifications != null)
                    {
                        // Note: Cannot use Unity Logging in background thread
                        System.Console.WriteLine($"[Background] Starting modifications pre-processing for {normalizedData.modifications.Length} modifications...");
                        apiModifications = JSONEntityHandler.ConvertToAPIModificationsThreadSafe(normalizedData.modifications);
                        System.Console.WriteLine($"[Background] Modifications pre-processing completed. Generated {apiModifications?.Length ?? 0} API modifications.");
                    }
                    else
                    {
                        System.Console.WriteLine($"[Background] No modifications to process. TerrainType: {normalizedData.terrainType}, Modifications: {normalizedData.modifications?.Length ?? 0}");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[Background] Processing failed: {ex.Message}");
                    processingException = ex;
                }
                finally
                {
                    processingComplete = true;
                }
            });

            // Wait for processing to complete
            yield return new WaitUntil(() => processingComplete);

            if (processingException != null)
            {
                // Check if this is a Unity main thread issue
                if (processingException.Message.Contains("can only be called from the main thread"))
                {
                    Logging.LogWarning("[JSONEntityHandler->LoadTerrainEntityFromJSONAsync] Background processing failed due to Unity main thread requirements. Falling back to coroutine-based processing.");
                    // Switch to coroutine-based processing
                    StartCoroutine(LoadTerrainEntityFromJSONCoroutine(jsonString, parentEntity, onComplete));
                    yield break;
                }
                
                Logging.LogError($"[JSONEntityHandler->LoadTerrainEntityFromJSONAsync] Data processing failed: {processingException.Message}");
                onComplete?.Invoke(false, null, null);
                yield break;
            }

            // Phase 4: Create entity (must be on main thread due to Unity API requirements)
            try
            {
                Logging.Log("[JSONEntityHandler->LoadTerrainEntityFromJSONAsync] Creating terrain entity on main thread...");
                Guid? entityId = CreateTerrainEntity(normalizedData, parentEntity, (id, entity) =>
                {
                    onComplete?.Invoke(true, entity?.id, entity);
                }, apiModifications);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadTerrainEntityFromJSONAsync] Entity creation failed: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Fallback coroutine-based terrain processing that runs entirely on the main thread.
        /// This method yields control periodically to maintain frame rate.
        /// </summary>
        /// <param name="jsonString">JSON string to process</param>
        /// <param name="parentEntity">Parent entity (optional)</param>
        /// <param name="onComplete">Completion callback</param>
        /// <returns>IEnumerator for coroutine</returns>
        private IEnumerator LoadTerrainEntityFromJSONCoroutine(string jsonString, BaseEntity parentEntity, Action<bool, Guid?, BaseEntity> onComplete)
        {
            JSONTerrainEntity terrainData = null;
            JSONTerrainEntity normalizedData = null;
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[] apiModifications = null;
            bool hasError = false;
            string errorMessage = "";
            
            // Phase 1: Parse JSON on main thread
            try
            {
                terrainData = ParseTerrainEntityFromJSON(jsonString);
                
                if (terrainData == null)
                {
                    hasError = true;
                    errorMessage = "Failed to parse terrain data from JSON.";
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = $"JSON parsing error: {ex.Message}";
            }
            
            yield return null; // Yield control after parsing
            
            if (hasError)
            {
                Logging.LogError($"[JSONEntityHandler->LoadTerrainEntityFromJSONCoroutine] {errorMessage}");
                onComplete?.Invoke(false, null, null);
                yield break;
            }

            // Phase 2: Validate
            if (!ValidateTerrainEntity(terrainData))
            {
                Logging.LogError("[JSONEntityHandler->LoadTerrainEntityFromJSONCoroutine] Terrain entity validation failed.");
                onComplete?.Invoke(false, null, null);
                yield break;
            }

            yield return null; // Yield control after validation

            // Phase 3: Normalize data on main thread
            try
            {
                normalizedData = terrainData.base_ground != null ? NormalizeTerrainData(terrainData) : terrainData;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadTerrainEntityFromJSONCoroutine] Data normalization error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
                yield break;
            }
            
            yield return null; // Yield control after normalization

            // Phase 3.5: Process modifications for hybrid terrain (on main thread)
            if (normalizedData.terrainType == "hybrid" && normalizedData.modifications != null)
            {
                try
                {
                    apiModifications = ConvertToAPIModifications(normalizedData.modifications);
                }
                catch (Exception ex)
                {
                    Logging.LogError($"[JSONEntityHandler->LoadTerrainEntityFromJSONCoroutine] Modifications processing error: {ex.Message}");
                    onComplete?.Invoke(false, null, null);
                    yield break;
                }
                
                yield return null; // Yield control after modifications processing
            }

            // Phase 4: Create entity
            try
            {
                Guid? entityId = CreateTerrainEntity(normalizedData, parentEntity, (id, entity) =>
                {
                    onComplete?.Invoke(true, entity?.id, entity);
                }, apiModifications);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadTerrainEntityFromJSONCoroutine] Entity creation error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Parse terrain entity from JSON without creating the entity.
        /// Handles both standard format and alternative base_ground/named layer formats.
        /// </summary>
        /// <param name="jsonString">JSON string containing the terrain entity definition</param>
        /// <returns>Parsed JSONTerrainEntity object or null if parsing failed</returns>
        public JSONTerrainEntity ParseTerrainEntityFromJSON(string jsonString)
        {
            try
            {
                // Use streaming parser to detect format without full object creation
                var formatInfo = DetectTerrainFormat(jsonString);
                
                if (formatInfo.IsNamedLayerFormat)
                {
                    return ParseNamedLayerFormatOptimized(jsonString, formatInfo);
                }
                else
                {
                    // Standard format - direct deserialization with custom converters
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new TerrainEntityLayerMaskConverter());
                    settings.Converters.Add(new TerrainBaseGroundConverter());
                    return JsonConvert.DeserializeObject<JSONTerrainEntity>(jsonString, settings);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseTerrainEntityFromJSON] Failed to parse JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Thread-safe version of ParseTerrainEntityFromJSON for background processing.
        /// This method avoids Unity-specific operations that must run on the main thread.
        /// </summary>
        /// <param name="jsonString">JSON string containing the terrain entity definition</param>
        /// <returns>Parsed JSONTerrainEntity object or null if parsing failed</returns>
        private static JSONTerrainEntity ParseTerrainEntityFromJSONThreadSafe(string jsonString)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonString))
                {
                    System.Console.WriteLine("[ParseTerrainEntityFromJSONThreadSafe] JSON string is null or empty");
                    return null;
                }

                System.Console.WriteLine("[ParseTerrainEntityFromJSONThreadSafe] Detecting terrain format...");
                
                // Use streaming parser to detect format without full object creation
                var formatInfo = DetectTerrainFormatThreadSafe(jsonString);
                
                System.Console.WriteLine($"[ParseTerrainEntityFromJSONThreadSafe] Format detected - IsNamedLayer: {formatInfo.IsNamedLayerFormat}, HasBaseGround: {formatInfo.HasBaseGround}");
                
                if (formatInfo.IsNamedLayerFormat)
                {
                    System.Console.WriteLine("[ParseTerrainEntityFromJSONThreadSafe] Parsing named layer format...");
                    return ParseNamedLayerFormatOptimizedThreadSafe(jsonString, formatInfo);
                }
                else
                {
                    System.Console.WriteLine("[ParseTerrainEntityFromJSONThreadSafe] Parsing standard format...");
                    // Standard format - direct deserialization with custom converters
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new TerrainEntityLayerMaskConverter());
                    settings.Converters.Add(new TerrainBaseGroundConverter());
                    var result = JsonConvert.DeserializeObject<JSONTerrainEntity>(jsonString, settings);
                    System.Console.WriteLine($"[ParseTerrainEntityFromJSONThreadSafe] Standard format parsed, result is null: {result == null}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                // Note: Cannot use Unity's Logging in background thread, will be handled by caller
                System.Console.WriteLine($"[ParseTerrainEntityFromJSONThreadSafe] Exception: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Console.WriteLine($"[ParseTerrainEntityFromJSONThreadSafe] Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
                throw; // Re-throw to be caught by caller
            }
        }

        /// <summary>
        /// Thread-safe terrain format detection.
        /// </summary>
        /// <param name="jsonString">JSON string to analyze</param>
        /// <returns>Format information</returns>
        private static TerrainFormatInfo DetectTerrainFormatThreadSafe(string jsonString)
        {
            var formatInfo = new TerrainFormatInfo();
            
            try
            {
                System.Console.WriteLine("[DetectTerrainFormatThreadSafe] Starting format detection...");
                
                using (var reader = new System.IO.StringReader(jsonString))
                using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader))
                {
                    int tokenCount = 0;
                    while (jsonReader.Read() && tokenCount < 100) // Limit to prevent infinite loops
                    {
                        tokenCount++;
                        if (jsonReader.TokenType == Newtonsoft.Json.JsonToken.PropertyName)
                        {
                            string propertyName = jsonReader.Value?.ToString();
                            System.Console.WriteLine($"[DetectTerrainFormatThreadSafe] Found property: {propertyName}");
                            
                            switch (propertyName)
                            {
                                case "base_ground":
                                    formatInfo.HasBaseGround = true;
                                    System.Console.WriteLine("[DetectTerrainFormatThreadSafe] Detected base_ground format");
                                    break;
                                case "layers" when !formatInfo.HasBaseGround:
                                    // Check if layers at root level is an object (named layers)
                                    jsonReader.Read(); // Move to value
                                    if (jsonReader.TokenType == Newtonsoft.Json.JsonToken.StartObject)
                                    {
                                        formatInfo.HasRootNamedLayers = true;
                                        formatInfo.IsNamedLayerFormat = true;
                                        System.Console.WriteLine("[DetectTerrainFormatThreadSafe] Detected named layer format");
                                    }
                                    break;
                            }
                            
                            // Early exit if we have enough information
                            if (formatInfo.HasBaseGround || formatInfo.IsNamedLayerFormat)
                            {
                                System.Console.WriteLine("[DetectTerrainFormatThreadSafe] Early exit - format determined");
                                break;
                            }
                        }
                    }
                }
                
                System.Console.WriteLine($"[DetectTerrainFormatThreadSafe] Format detection complete - HasBaseGround: {formatInfo.HasBaseGround}, IsNamedLayer: {formatInfo.IsNamedLayerFormat}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DetectTerrainFormatThreadSafe] Format detection failed: {ex.Message}");
                throw;
            }
            
            return formatInfo;
        }

        /// <summary>
        /// Thread-safe version of ParseNamedLayerFormatOptimized.
        /// </summary>
        /// <param name="jsonString">JSON string to parse</param>
        /// <param name="formatInfo">Format information</param>
        /// <returns>Parsed terrain entity</returns>
        private static JSONTerrainEntity ParseNamedLayerFormatOptimizedThreadSafe(string jsonString, TerrainFormatInfo formatInfo)
        {
            try
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new TerrainEntityLayerMaskConverter());
                settings.Converters.Add(new TerrainBaseGroundConverter());
                return JsonConvert.DeserializeObject<JSONTerrainEntity>(jsonString, settings);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[JSONEntityHandler->ParseNamedLayerFormatOptimizedThreadSafe] Failed to parse: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Terrain format information for memory-efficient parsing.
        /// </summary>
        private struct TerrainFormatInfo
        {
            public bool IsNamedLayerFormat;
            public bool HasBaseGround;
            public bool HasRootNamedLayers;
            public int EstimatedLayerCount;
        }

        /// <summary>
        /// Efficiently detect terrain format without creating full object copies.
        /// </summary>
        /// <param name="jsonString">JSON string to analyze</param>
        /// <returns>Format information</returns>
        private TerrainFormatInfo DetectTerrainFormat(string jsonString)
        {
            var formatInfo = new TerrainFormatInfo();
            
            using (var reader = new System.IO.StringReader(jsonString))
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == Newtonsoft.Json.JsonToken.PropertyName)
                    {
                        string propertyName = jsonReader.Value.ToString();
                        
                        switch (propertyName)
                        {
                            case "base_ground":
                                formatInfo.HasBaseGround = true;
                                break;
                            case "layers" when !formatInfo.HasBaseGround:
                                // Check if layers at root level is an object (named layers)
                                jsonReader.Read(); // Move to value
                                if (jsonReader.TokenType == Newtonsoft.Json.JsonToken.StartObject)
                                {
                                    formatInfo.HasRootNamedLayers = true;
                                    formatInfo.IsNamedLayerFormat = true;
                                }
                                break;
                        }
                        
                        // Early exit if we've detected the format
                        if (formatInfo.HasBaseGround || formatInfo.HasRootNamedLayers)
                        {
                            break;
                        }
                    }
                }
            }
            
            // If we found base_ground, need to check if it contains named layers
            if (formatInfo.HasBaseGround)
            {
                formatInfo.IsNamedLayerFormat = HasNamedLayersInBaseGround(jsonString);
            }
            
            return formatInfo;
        }

        /// <summary>
        /// Check if base_ground contains named layers without full parsing.
        /// </summary>
        private bool HasNamedLayersInBaseGround(string jsonString)
        {
            using (var reader = new System.IO.StringReader(jsonString))
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader))
            {
                bool inBaseGround = false;
                int depth = 0;
                
                while (jsonReader.Read())
                {
                    switch (jsonReader.TokenType)
                    {
                        case Newtonsoft.Json.JsonToken.PropertyName:
                            if (jsonReader.Value.ToString() == "base_ground" && depth == 1)
                            {
                                inBaseGround = true;
                            }
                            else if (inBaseGround && jsonReader.Value.ToString() == "layers")
                            {
                                jsonReader.Read(); // Move to value
                                return jsonReader.TokenType == Newtonsoft.Json.JsonToken.StartObject;
                            }
                            break;
                        case Newtonsoft.Json.JsonToken.StartObject:
                            depth++;
                            break;
                        case Newtonsoft.Json.JsonToken.EndObject:
                            depth--;
                            if (inBaseGround && depth == 1)
                            {
                                return false; // Exited base_ground without finding named layers
                            }
                            break;
                    }
                }
                
                return false;
            }
        }

        /// <summary>
        /// Parse named layer format with minimal memory allocation.
        /// </summary>
        private JSONTerrainEntity ParseNamedLayerFormatOptimized(string jsonString, TerrainFormatInfo formatInfo)
        {
            try
            {
                // Use selective JObject parsing only for the parts that need transformation
                using (var reader = new System.IO.StringReader(jsonString))
                using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader))
                {
                    var jObject = Newtonsoft.Json.Linq.JObject.Load(jsonReader);
                    
                    // Apply minimal transformations needed for named layers
                    if (formatInfo.HasRootNamedLayers)
                    {
                        var rootLayersToken = jObject["layers"];
                        jObject["namedLayers"] = rootLayersToken;
                        jObject["layers"] = new Newtonsoft.Json.Linq.JArray();
                    }
                    else if (formatInfo.HasBaseGround)
                    {
                        ProcessBaseGroundNamedLayers(jObject);
                    }
                    
                    // Convert back to JSON and deserialize once with custom converters
                    using (var stringReader = new System.IO.StringReader(jObject.ToString()))
                    using (var deserializeReader = new Newtonsoft.Json.JsonTextReader(stringReader))
                    {
                        var serializer = new Newtonsoft.Json.JsonSerializer();
                        serializer.Converters.Add(new TerrainEntityLayerMaskConverter());
                        serializer.Converters.Add(new TerrainBaseGroundConverter());
                        return serializer.Deserialize<JSONTerrainEntity>(deserializeReader);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseNamedLayerFormatOptimized] Failed to parse: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Process base_ground named layers with minimal object manipulation.
        /// </summary>
        private void ProcessBaseGroundNamedLayers(Newtonsoft.Json.Linq.JObject jObject)
        {
            var baseGroundToken = jObject["base_ground"];
            if (baseGroundToken is Newtonsoft.Json.Linq.JObject baseGroundObj)
            {
                var layersToken = baseGroundObj["layers"];
                if (layersToken?.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                {
                    // Move layers to namedLayers
                    baseGroundObj["namedLayers"] = layersToken;
                    baseGroundObj["layers"] = new Newtonsoft.Json.Linq.JArray();
                    
                    // Handle layerMasks conversion if needed
                    var layerMasksToken = baseGroundObj["layerMasks"];
                    if (layerMasksToken is Newtonsoft.Json.Linq.JArray layerMasksArray && 
                        layerMasksArray.Count > 0 && 
                        layerMasksArray[0].Type == Newtonsoft.Json.Linq.JTokenType.Array)
                    {
                        // Convert array format to object format
                        var convertedMasks = new Newtonsoft.Json.Linq.JArray();
                        foreach (var maskArray in layerMasksArray)
                        {
                            if (maskArray.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                            {
                                var maskObject = new Newtonsoft.Json.Linq.JObject { ["heights"] = maskArray };
                                convertedMasks.Add(maskObject);
                            }
                        }
                        baseGroundObj["layerMasks"] = convertedMasks;
                    }
                }
            }
        }

        /// <summary>
        /// Create optimized layer masks with minimal memory allocation.
        /// Uses lazy generation to reduce memory usage for terrains with default masks.
        /// </summary>
        /// <param name="layerCount">Number of layers</param>
        /// <param name="heightRows">Height map rows</param>
        /// <param name="heightCols">Height map columns</param>
        /// <returns>Array of lazy layer masks</returns>
        private JSONTerrainEntityLayerMask[] CreateLazyLayerMasks(int layerCount, int heightRows, int heightCols)
        {
            var lazyMasks = new JSONTerrainEntityLayerMask[layerCount];
            
            // Store lazy mask generators for deferred creation
            _lazyMaskGenerators = new Dictionary<JSONTerrainEntityLayerMask, LazyMaskInfo>();
            
            for (int i = 0; i < layerCount; i++)
            {
                var mask = new JSONTerrainEntityLayerMask
                {
                    heights = null // Will be generated on-demand
                };
                
                // Store generation parameters
                _lazyMaskGenerators[mask] = new LazyMaskInfo
                {
                    Rows = heightRows,
                    Cols = heightCols,
                    DefaultValue = (i == 0) ? 1.0f : 0.0f,
                    LayerIndex = i
                };
                
                lazyMasks[i] = mask;
            }
            
            return lazyMasks;
        }

        /// <summary>
        /// Information for lazy mask generation.
        /// </summary>
        private struct LazyMaskInfo
        {
            public int Rows;
            public int Cols;
            public float DefaultValue;
            public int LayerIndex;
        }

        /// <summary>
        /// Dictionary to track lazy mask generators.
        /// </summary>
        private static Dictionary<JSONTerrainEntityLayerMask, LazyMaskInfo> _lazyMaskGenerators = 
            new Dictionary<JSONTerrainEntityLayerMask, LazyMaskInfo>();

        /// <summary>
        /// Memory-efficient array pool for terrain processing.
        /// </summary>
        private static TerrainArrayPool _arrayPool = new TerrainArrayPool();

        /// <summary>
        /// Array pool for reducing GC pressure from float[][] allocations.
        /// </summary>
        private class TerrainArrayPool
        {
            private readonly Dictionary<string, Queue<float[][]>> _pool = new Dictionary<string, Queue<float[][]>>();
            private readonly object _lock = new object();
            private const int MaxPoolSize = 10; // Limit pool size to prevent memory leaks

            /// <summary>
            /// Get a pooled 2D array or create a new one.
            /// </summary>
            public float[][] GetArray(int rows, int cols)
            {
                string key = $"{rows}x{cols}";
                
                lock (_lock)
                {
                    if (_pool.TryGetValue(key, out var queue) && queue.Count > 0)
                    {
                        var pooledArray = queue.Dequeue();
                        ClearArray(pooledArray); // Clear previous data
                        return pooledArray;
                    }
                }
                
                // Create new array if none available in pool
                return CreateArray(rows, cols);
            }

            /// <summary>
            /// Return an array to the pool for reuse.
            /// </summary>
            public void ReturnArray(float[][] array)
            {
                if (array == null || array.Length == 0) return;
                
                int rows = array.Length;
                int cols = array[0]?.Length ?? 0;
                if (cols == 0) return;
                
                string key = $"{rows}x{cols}";
                
                lock (_lock)
                {
                    if (!_pool.TryGetValue(key, out var queue))
                    {
                        queue = new Queue<float[][]>();
                        _pool[key] = queue;
                    }
                    
                    if (queue.Count < MaxPoolSize)
                    {
                        queue.Enqueue(array);
                    }
                    // If pool is full, let array be garbage collected
                }
            }

            private float[][] CreateArray(int rows, int cols)
            {
                var array = new float[rows][];
                for (int i = 0; i < rows; i++)
                {
                    array[i] = new float[cols];
                }
                return array;
            }

            private void ClearArray(float[][] array)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] != null)
                    {
                        System.Array.Clear(array[i], 0, array[i].Length);
                    }
                }
            }
        }

        /// <summary>
        /// Generate heights for a lazy mask if not already generated.
        /// </summary>
        /// <param name="mask">The layer mask to generate heights for</param>
        private void EnsureMaskHeightsGenerated(JSONTerrainEntityLayerMask mask)
        {
            if (mask.heights == null && _lazyMaskGenerators.TryGetValue(mask, out var info))
            {
                mask.heights = GenerateDefaultMaskHeights(info.Rows, info.Cols, info.DefaultValue);
                Logging.Log($"[EnsureMaskHeightsGenerated] Generated {info.Rows}x{info.Cols} mask for layer {info.LayerIndex} with default value {info.DefaultValue}");
                
                // Remove from lazy generators once generated
                _lazyMaskGenerators.Remove(mask);
            }
        }

        /// <summary>
        /// Generate default mask heights for a layer using memory pool.
        /// </summary>
        private float[][] GenerateDefaultMaskHeights(int rows, int cols, float defaultValue)
        {
            // Use array pool to reduce GC pressure
            var heights = _arrayPool.GetArray(rows, cols);
            
            // Fill with default values
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    heights[row][col] = defaultValue;
                }
            }
            
            return heights;
        }

        /// <summary>
        /// Validate terrain entity data.
        /// Supports both standard format and alternative base_ground/ground_mods format.
        /// </summary>
        /// <param name="terrainData">Terrain entity data to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateTerrainEntity(JSONTerrainEntity terrainData)
        {
            if (terrainData == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateTerrainEntity] Terrain data is null.");
                return false;
            }

            if (string.IsNullOrEmpty(terrainData.terrainType))
            {
                Logging.LogError("[JSONEntityHandler->ValidateTerrainEntity] Terrain type is required.");
                return false;
            }

            if (terrainData.terrainType != "heightmap" && terrainData.terrainType != "hybrid")
            {
                Logging.LogError($"[JSONEntityHandler->ValidateTerrainEntity] Invalid terrain type: {terrainData.terrainType}. Must be 'heightmap' or 'hybrid'.");
                return false;
            }

            if (terrainData.length <= 0 || terrainData.width <= 0 || terrainData.height <= 0)
            {
                Logging.LogError("[JSONEntityHandler->ValidateTerrainEntity] Terrain dimensions must be positive.");
                return false;
            }

            // Check if using alternative base_ground format
            bool usingBaseGroundFormat = terrainData.base_ground != null;
            
            if (usingBaseGroundFormat)
            {
                // Validate base_ground format
                if (terrainData.base_ground.heights == null || terrainData.base_ground.heights.Length == 0)
                {
                    Logging.LogError("[JSONEntityHandler->ValidateTerrainEntity] base_ground.heights array is required.");
                    return false;
                }

                // Check for either layers array or namedLayers object
                bool hasLayers = (terrainData.base_ground.layers != null && terrainData.base_ground.layers.Length > 0);
                bool hasNamedLayers = (terrainData.base_ground.namedLayers != null);
                
                if (!hasLayers && !hasNamedLayers)
                {
                    Logging.LogError("[JSONEntityHandler->ValidateTerrainEntity] base_ground requires either layers array or namedLayers object.");
                    return false;
                }

                // Validate hybrid-specific requirements for base_ground format
                if (terrainData.terrainType == "hybrid" && (terrainData.ground_mods == null))
                {
                    Logging.LogWarning("[JSONEntityHandler->ValidateTerrainEntity] Hybrid terrain has no ground_mods - will behave like heightmap terrain.");
                }
            }
            else
            {
                // Validate standard format
                if (terrainData.heights == null || terrainData.heights.Length == 0)
                {
                    Logging.LogError("[JSONEntityHandler->ValidateTerrainEntity] Terrain heights array is required.");
                    return false;
                }

                if (terrainData.layers == null || terrainData.layers.Length == 0)
                {
                    Logging.LogError("[JSONEntityHandler->ValidateTerrainEntity] At least one terrain layer is required.");
                    return false;
                }

                // Validate hybrid-specific requirements for standard format
                if (terrainData.terrainType == "hybrid" && (terrainData.modifications == null || terrainData.modifications.Length == 0))
                {
                    Logging.LogWarning("[JSONEntityHandler->ValidateTerrainEntity] Hybrid terrain has no modifications - will behave like heightmap terrain.");
                }
            }

            return true;
        }

        /// <summary>
        /// Create a terrain entity from parsed JSONTerrainEntity data.
        /// Supports both standard format and alternative base_ground/ground_mods format.
        /// </summary>
        /// <param name="terrainData">Parsed terrain entity data</param>
        /// <param name="parentEntity">Parent entity (optional)</param>
        /// <param name="onComplete">Callback with entity ID and created entity</param>
        /// <returns>Entity GUID if creation started successfully, null otherwise</returns>
        public Guid? CreateTerrainEntity(JSONTerrainEntity terrainData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            return CreateTerrainEntity(terrainData, parentEntity, onComplete, null);
        }

        /// <summary>
        /// Create a terrain entity from parsed JSONTerrainEntity data with optional pre-processed modifications.
        /// This overload is used by background processing to avoid re-processing modifications.
        /// </summary>
        /// <param name="terrainData">Parsed terrain entity data</param>
        /// <param name="parentEntity">Parent entity (optional)</param>
        /// <param name="onComplete">Callback with entity ID and created entity</param>
        /// <param name="preProcessedModifications">Pre-processed API modifications (optional)</param>
        /// <returns>Entity GUID if creation started successfully, null otherwise</returns>
        public Guid? CreateTerrainEntity(JSONTerrainEntity terrainData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete, 
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[] preProcessedModifications)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->CreateTerrainEntity] Starting terrain entity creation");
                
                Guid entityGuid = string.IsNullOrEmpty(terrainData.id) ? Guid.NewGuid() : Guid.Parse(terrainData.id);

                Vector3 position = terrainData.position != null ? 
                    new Vector3(terrainData.position.x, terrainData.position.y, terrainData.position.z) : Vector3.zero;
                Quaternion rotation = terrainData.rotation != null ? 
                    new Quaternion(terrainData.rotation.x, terrainData.rotation.y, terrainData.rotation.z, terrainData.rotation.w) : Quaternion.identity;

                // Normalize terrain data format if using base_ground format (skip if already normalized)
                JSONTerrainEntity normalizedData = terrainData.base_ground != null ? NormalizeTerrainData(terrainData) : terrainData;

                // Create terrain entity based on type
                if (normalizedData.terrainType == "heightmap")
                {
                    var result = CreateHeightmapTerrainEntity(normalizedData, entityGuid, parentEntity, position, rotation, onComplete);
                    return result;
                }
                else if (normalizedData.terrainType == "hybrid")
                {
                    var result = CreateHybridTerrainEntity(normalizedData, entityGuid, parentEntity, position, rotation, onComplete, preProcessedModifications);
                    return result;
                }
                else
                {
                    Logging.LogError($"[JSONEntityHandler->CreateTerrainEntity] Unsupported terrain type: {normalizedData.terrainType}");
                    onComplete?.Invoke(null, null);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->CreateTerrainEntity] Error: {ex.Message}");
                onComplete?.Invoke(null, null);
                return null;
            }
        }

        /// <summary>
        /// Normalize terrain data from alternative base_ground format to standard format.
        /// Handles both array-based and named layer formats.
        /// </summary>
        /// <param name="terrainData">Original terrain data (may be in base_ground format)</param>
        /// <returns>Normalized terrain data in standard format</returns>
        private JSONTerrainEntity NormalizeTerrainData(JSONTerrainEntity terrainData)
        {
            // If not using base_ground format, return as-is
            if (terrainData.base_ground == null)
            {
                return terrainData;
            }

            // Create normalized copy
            JSONTerrainEntity normalized = new JSONTerrainEntity
            {
                id = terrainData.id,
                tag = terrainData.tag,
                position = terrainData.position,
                rotation = terrainData.rotation,
                scale = terrainData.scale,
                isSize = terrainData.isSize,
                parentId = terrainData.parentId,
                terrainType = terrainData.terrainType,
                length = terrainData.length,
                width = terrainData.width,
                height = terrainData.height,
                stitchTerrains = terrainData.stitchTerrains,
                children = terrainData.children
            };

            // Convert base_ground.heights to standard heights
            normalized.heights = terrainData.base_ground.heights;

            // Handle layers - check if it's named layer format or array format
            if (terrainData.base_ground.namedLayers != null)
            {
                // Handle named layer format like {"basalt": {...}, "grass": {...}}
                ProcessNamedLayers(terrainData.base_ground.namedLayers, normalized);
                
                // For named layers, check for layerMasks data (not layers array)
                if (terrainData.base_ground.layerMasks != null && terrainData.base_ground.layerMasks.Length > 0)
                {
                    // LayerMasks are already in the correct format for named layers
                    normalized.layerMasks = terrainData.base_ground.layerMasks;
                }
                // Fallback: check if layer mask data is in the layers array (old format)
                else if (terrainData.base_ground.layers != null && terrainData.base_ground.layers.Length > 0)
                {
                    // Convert base_ground.layers (3D array) to layerMasks (array of 2D arrays)
                    normalized.layerMasks = new JSONTerrainEntityLayerMask[terrainData.base_ground.layers.Length];
                    for (int i = 0; i < terrainData.base_ground.layers.Length; i++)
                    {
                        normalized.layerMasks[i] = new JSONTerrainEntityLayerMask
                        {
                            heights = terrainData.base_ground.layers[i]
                        };
                    }
                }
                else
                {
                    // Store metadata for lazy mask generation instead of creating large arrays immediately
                    int layerCount = normalized.layers?.Length ?? 1;
                    int heightRows = normalized.heights?.Length ?? 1;
                    int heightCols = (normalized.heights?.Length > 0 && normalized.heights[0] != null) ? normalized.heights[0].Length : 1;
                    
                    // Create lazy mask placeholders that will generate data when needed
                    normalized.layerMasks = CreateLazyLayerMasks(layerCount, heightRows, heightCols);
                }
            }
            else if (terrainData.base_ground.layers != null && terrainData.base_ground.layers.Length > 0)
            {
                // Handle array format - convert base_ground.layers (3D array) to layerMasks (array of 2D arrays)
                normalized.layerMasks = new JSONTerrainEntityLayerMask[terrainData.base_ground.layers.Length];
                for (int i = 0; i < terrainData.base_ground.layers.Length; i++)
                {
                    normalized.layerMasks[i] = new JSONTerrainEntityLayerMask
                    {
                        heights = terrainData.base_ground.layers[i]
                    };
                }

                // Create default layers if not provided
                normalized.layers = new JSONTerrainEntityLayer[terrainData.base_ground.layers.Length];
                for (int i = 0; i < terrainData.base_ground.layers.Length; i++)
                {
                    normalized.layers[i] = new JSONTerrainEntityLayer();
                }
            }

            // Convert ground_mods to modifications
            if (terrainData.ground_mods != null)
            {
                normalized.modifications = terrainData.ground_mods;
            }

            return normalized;
        }

        /// <summary>
        /// Thread-safe version of NormalizeTerrainData for background processing.
        /// This method avoids Unity-specific logging operations.
        /// </summary>
        /// <param name="terrainData">Original terrain data (may be in base_ground format)</param>
        /// <returns>Normalized terrain data in standard format</returns>
        private static JSONTerrainEntity NormalizeTerrainDataThreadSafe(JSONTerrainEntity terrainData)
        {
            // If not using base_ground format, return as-is
            if (terrainData.base_ground == null)
            {
                return terrainData;
            }

            // Create normalized copy
            JSONTerrainEntity normalized = new JSONTerrainEntity
            {
                id = terrainData.id,
                tag = terrainData.tag,
                position = terrainData.position,
                rotation = terrainData.rotation,
                scale = terrainData.scale,
                isSize = terrainData.isSize,
                parentId = terrainData.parentId,
                terrainType = terrainData.terrainType,
                length = terrainData.length,
                width = terrainData.width,
                height = terrainData.height,
                stitchTerrains = terrainData.stitchTerrains,
                children = terrainData.children
            };

            // Convert base_ground.heights to standard heights
            normalized.heights = terrainData.base_ground.heights;

            // Handle layers - check if it's named layer format or array format
            if (terrainData.base_ground.namedLayers != null)
            {
                // Handle named layer format like {"basalt": {...}, "grass": {...}}
                ProcessNamedLayersThreadSafe(terrainData.base_ground.namedLayers, normalized);
                
                // Handle layer masks from base_ground
                if (terrainData.base_ground.layerMasks != null && terrainData.base_ground.layerMasks.Length > 0)
                {
                    normalized.layerMasks = terrainData.base_ground.layerMasks;
                }
                else if (terrainData.base_ground.layers != null && terrainData.base_ground.layers.Length > 0)
                {
                    // Create layer masks from layers array (fallback)
                    normalized.layerMasks = new JSONTerrainEntityLayerMask[terrainData.base_ground.layers.Length];
                    for (int i = 0; i < terrainData.base_ground.layers.Length; i++)
                    {
                        normalized.layerMasks[i] = new JSONTerrainEntityLayerMask
                        {
                            heights = terrainData.base_ground.layers[i]
                        };
                    }
                }
                else
                {
                    // Create lazy mask placeholders for named layers without explicit mask data
                    int layerCount = normalized.layers?.Length ?? 1;
                    int heightRows = normalized.heights?.Length ?? 1;
                    int heightCols = (normalized.heights?.Length > 0 && normalized.heights[0] != null) ? normalized.heights[0].Length : 1;
                    
                    // Create lazy mask placeholders that will generate data when needed
                    normalized.layerMasks = CreateLazyLayerMasksThreadSafe(layerCount, heightRows, heightCols);
                }
            }
            else if (terrainData.base_ground.layers != null && terrainData.base_ground.layers.Length > 0)
            {
                // Handle array format - convert base_ground.layers (3D array) to layerMasks (array of 2D arrays)
                normalized.layerMasks = new JSONTerrainEntityLayerMask[terrainData.base_ground.layers.Length];
                for (int i = 0; i < terrainData.base_ground.layers.Length; i++)
                {
                    normalized.layerMasks[i] = new JSONTerrainEntityLayerMask
                    {
                        heights = terrainData.base_ground.layers[i]
                    };
                }

                // Create default layers if not provided
                normalized.layers = new JSONTerrainEntityLayer[terrainData.base_ground.layers.Length];
                for (int i = 0; i < terrainData.base_ground.layers.Length; i++)
                {
                    normalized.layers[i] = new JSONTerrainEntityLayer();
                }
            }

            // Convert ground_mods to modifications
            if (terrainData.ground_mods != null)
            {
                normalized.modifications = terrainData.ground_mods;
            }

            return normalized;
        }

        /// <summary>
        /// Thread-safe version of ProcessNamedLayers.
        /// </summary>
        /// <param name="namedLayersObj">Named layers object from JSON</param>
        /// <param name="normalized">Normalized terrain entity to populate</param>
        private static void ProcessNamedLayersThreadSafe(object namedLayersObj, JSONTerrainEntity normalized)
        {
            try
            {
                // Convert the object to a JObject for easier manipulation
                var jObject = Newtonsoft.Json.Linq.JObject.FromObject(namedLayersObj);
                var layerList = new List<JSONTerrainEntityLayer>();

                // Process each named layer
                foreach (var kvp in jObject)
                {
                    try
                    {
                        var namedLayer = kvp.Value.ToObject<JSONNamedTerrainLayer>();
                        if (namedLayer != null)
                        {
                            layerList.Add(namedLayer.ToStandardLayer());
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[JSONEntityHandler->ProcessNamedLayersThreadSafe] Failed to process layer {kvp.Key}: {ex.Message}");
                    }
                }

                // Sort by layer index to maintain proper order
                layerList.Sort((a, b) => a.sizeFactor.CompareTo(b.sizeFactor)); // Using sizeFactor as proxy for layer index
                
                normalized.layers = layerList.ToArray();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[JSONEntityHandler->ProcessNamedLayersThreadSafe] Failed to process named layers: {ex.Message}");
                // Create default single layer as fallback
                normalized.layers = new JSONTerrainEntityLayer[] { new JSONTerrainEntityLayer() };
            }
        }

        /// <summary>
        /// Thread-safe version of CreateLazyLayerMasks.
        /// </summary>
        /// <param name="layerCount">Number of layers</param>
        /// <param name="heightRows">Number of height rows</param>
        /// <param name="heightCols">Number of height columns</param>
        /// <returns>Array of lazy layer mask placeholders</returns>
        private static JSONTerrainEntityLayerMask[] CreateLazyLayerMasksThreadSafe(int layerCount, int heightRows, int heightCols)
        {
            var layerMasks = new JSONTerrainEntityLayerMask[layerCount];
            
            for (int i = 0; i < layerCount; i++)
            {
                var layerMask = new JSONTerrainEntityLayerMask();
                // Create placeholder that will be populated later
                layerMask.heights = new float[heightRows][];
                for (int row = 0; row < heightRows; row++)
                {
                    layerMask.heights[row] = new float[heightCols];
                    // Initialize with default values (layer 0 = full strength, others = 0)
                    float defaultValue = (i == 0) ? 1.0f : 0.0f;
                    for (int col = 0; col < heightCols; col++)
                    {
                        layerMask.heights[row][col] = defaultValue;
                    }
                }
                layerMasks[i] = layerMask;
            }
            
            return layerMasks;
        }

        /// <summary>
        /// Process named layers from JSON object format.
        /// </summary>
        /// <param name="namedLayersObj">Named layers object from JSON</param>
        /// <param name="normalized">Normalized terrain entity to populate</param>
        private void ProcessNamedLayers(object namedLayersObj, JSONTerrainEntity normalized)
        {
            try
            {
                // Convert the object to a JSON string and back to JObject for processing
                string layersJson = JsonConvert.SerializeObject(namedLayersObj);
                
                Newtonsoft.Json.Linq.JObject layersJObject = Newtonsoft.Json.Linq.JObject.Parse(layersJson);

                List<JSONTerrainEntityLayer> layersList = new List<JSONTerrainEntityLayer>();

                // Process each named layer
                foreach (var kvp in layersJObject)
                {
                    string layerName = kvp.Key;
                    var layerObj = kvp.Value;

                    // Extract layer properties
                    JSONNamedTerrainLayer namedLayer = layerObj.ToObject<JSONNamedTerrainLayer>();
                    
                    if (namedLayer != null)
                    {
                        // Convert to standard layer format
                        JSONTerrainEntityLayer standardLayer = namedLayer.ToStandardLayer();
                        
                        // Ensure we have enough slots in the list for the layer index
                        int layerIndex = namedLayer.layer;
                        while (layersList.Count <= layerIndex)
                        {
                            layersList.Add(new JSONTerrainEntityLayer());
                        }
                        
                        // Set the layer at the correct index
                        layersList[layerIndex] = standardLayer;
                    }
                }

                // Set the layers array
                normalized.layers = layersList.ToArray();
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->ProcessNamedLayers] Error processing named layers: {ex.Message}");
                
                // Fallback to empty layers
                normalized.layers = new JSONTerrainEntityLayer[1] { new JSONTerrainEntityLayer() };
            }
        }

        /// <summary>
        /// Create a heightmap terrain entity using TerrainEntity.CreateHeightmap method directly.
        /// </summary>
        private Guid? CreateHeightmapTerrainEntity(JSONTerrainEntity terrainData, Guid entityGuid, BaseEntity parentEntity, 
            Vector3 position, Quaternion rotation, Action<Guid?, BaseEntity> onComplete)
        {
            try
            {
                // Convert parent to API BaseEntity
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
                if (parentEntity != null)
                {
                    var internalEntity = parentEntity as StraightFour.Entity.BaseEntity;
                    if (internalEntity != null)
                    {
                        apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(internalEntity);
                    }
                }

                // Convert to WorldTypes
                var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                    position.x, position.y, position.z);
                var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                    rotation.x, rotation.y, rotation.z, rotation.w);

                // Convert layers
                var apiLayers = ConvertToAPILayers(terrainData.layers);

                // Convert layer masks
                var apiLayerMasks = ConvertToAPILayerMasks(terrainData.layerMasks);

                // Call TerrainEntity.CreateHeightmap
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntity terrainEntity = null;
                try 
                {
                    terrainEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntity.CreateHeightmap(
                        apiParent, 
                        terrainData.length,
                        terrainData.width,
                        terrainData.height,
                        terrainData.heights,
                        apiLayers,
                        apiLayerMasks,
                        worldPosition,
                        worldRotation,
                        entityGuid.ToString(),
                        terrainData.tag,
                        null, // onLoaded callback
                        terrainData.stitchTerrains);
                }
                catch (Exception createEx)
                {
                    Logging.LogError($"[CreateHeightmapTerrainEntity] Exception during TerrainEntity.CreateHeightmap: {createEx.Message}");
                    Logging.LogError($"[CreateHeightmapTerrainEntity] Stack trace: {createEx.StackTrace}");
                }

                if (terrainEntity == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateHeightmapTerrainEntity] Failed to create heightmap terrain entity");
                    onComplete?.Invoke(null, null);
                    return null;
                }

                // Set tag if specified
                if (!string.IsNullOrEmpty(terrainData.tag))
                {
                    terrainEntity.tag = terrainData.tag;
                }

                // Get the internal entity for the callback
                var internalTerrainEntity = terrainEntity.internalEntity;

                // Process children if any
                if (terrainData.children != null && terrainData.children.Length > 0)
                {
                    StartCoroutine(CreateTerrainChildrenCoroutine(terrainData.children, internalTerrainEntity, () =>
                    {
                        onComplete?.Invoke(entityGuid, internalTerrainEntity);
                    }));
                }
                else
                {
                    onComplete?.Invoke(entityGuid, internalTerrainEntity);
                }

                return entityGuid;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->CreateHeightmapTerrainEntity] Error creating heightmap terrain: {ex.Message}");
                onComplete?.Invoke(null, null);
                return null;
            }
        }

        /// <summary>
        /// Create a hybrid terrain entity using TerrainEntity.CreateHybrid method directly.
        /// </summary>
        private Guid? CreateHybridTerrainEntity(JSONTerrainEntity terrainData, Guid entityGuid, BaseEntity parentEntity, 
            Vector3 position, Quaternion rotation, Action<Guid?, BaseEntity> onComplete)
        {
            return CreateHybridTerrainEntity(terrainData, entityGuid, parentEntity, position, rotation, onComplete, null);
        }

        /// <summary>
        /// Create hybrid terrain entity with optional pre-processed modifications.
        /// </summary>
        private Guid? CreateHybridTerrainEntity(JSONTerrainEntity terrainData, Guid entityGuid, BaseEntity parentEntity, 
            Vector3 position, Quaternion rotation, Action<Guid?, BaseEntity> onComplete, 
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[] preProcessedModifications)
        {
            try
            {
                // Convert parent to API BaseEntity
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
                if (parentEntity != null)
                {
                    var internalEntity = parentEntity as StraightFour.Entity.BaseEntity;
                    if (internalEntity != null)
                    {
                        apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(internalEntity);
                    }
                }

                // Convert to WorldTypes
                var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                    position.x, position.y, position.z);
                var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                    rotation.x, rotation.y, rotation.z, rotation.w);

                // Convert layers, layer masks, and modifications
                var apiLayers = ConvertToAPILayers(terrainData.layers);
                var apiLayerMasks = ConvertToAPILayerMasks(terrainData.layerMasks);
                var apiModifications = preProcessedModifications ?? ConvertToAPIModifications(terrainData.modifications);
                
                System.Action<Javascript.APIs.Entity.TerrainEntity> onLoadedCallback = (terrainEntity) =>
                {
                    if (terrainEntity == null)
                    {
                        Logging.LogError("[JSONEntityHandler->CreateHybridTerrainEntity] Failed to create hybrid terrain entity");
                        onComplete?.Invoke(null, null);
                        return;
                    }

                    // Set tag if specified
                    if (!string.IsNullOrEmpty(terrainData.tag))
                    {
                        terrainEntity.tag = terrainData.tag;
                    }

                    // Get the internal entity for the callback
                    var internalTerrainEntity = terrainEntity.internalEntity;

                    // Process children if any
                    if (terrainData.children != null && terrainData.children.Length > 0)
                    {
                        StartCoroutine(CreateTerrainChildrenCoroutine(terrainData.children, internalTerrainEntity, () =>
                        {
                            onComplete?.Invoke(entityGuid, internalTerrainEntity);
                        }));
                    }
                    else
                    {
                        onComplete?.Invoke(entityGuid, internalTerrainEntity);
                    }
                };

                // Call TerrainEntity.CreateHybrid
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntity.CreateHybrid(
                        apiParent, 
                        terrainData.length,
                        terrainData.width,
                        terrainData.height,
                        terrainData.heights,
                        apiLayers,
                        apiLayerMasks,
                        apiModifications,
                        worldPosition,
                        worldRotation,
                        entityGuid.ToString(),
                        terrainData.tag,
                        onLoadedCallback,
                        terrainData.stitchTerrains); // need to add version that takes function and put callback in there

                return entityGuid;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->CreateHybridTerrainEntity] Error creating hybrid terrain: {ex.Message}");
                onComplete?.Invoke(null, null);
                return null;
            }
        }

        /// <summary>
        /// Convert JSON terrain entity layers to API terrain entity layers.
        /// </summary>
        private FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityLayer[] ConvertToAPILayers(JSONTerrainEntityLayer[] jsonLayers)
        {
            if (jsonLayers == null) return new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityLayer[0];

            var apiLayers = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityLayer[jsonLayers.Length];
            for (int i = 0; i < jsonLayers.Length; i++)
            {
                var jsonLayer = jsonLayers[i];
                
                var apiLayer = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityLayer
                {
                    diffuseTexture = jsonLayer.diffuseTexture,
                    normalTexture = jsonLayer.normalTexture,
                    maskTexture = jsonLayer.maskTexture,
                    specular = jsonLayer.specular != null ? 
                        new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(
                            jsonLayer.specular.r, jsonLayer.specular.g, jsonLayer.specular.b, jsonLayer.specular.a) :
                        new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(0.2f, 0.2f, 0.2f, 1.0f),
                    metallic = jsonLayer.metallic,
                    smoothness = jsonLayer.smoothness,
                    sizeFactor = jsonLayer.sizeFactor
                };
                
                apiLayers[i] = apiLayer;
            }
            return apiLayers;
        }

        /// <summary>
        /// Convert JSON terrain entity layer masks to API terrain entity layer mask collection.
        /// </summary>
        private FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityLayerMaskCollection ConvertToAPILayerMasks(JSONTerrainEntityLayerMask[] jsonLayerMasks)
        {
            var apiLayerMasks = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityLayerMaskCollection();
                       
            if (jsonLayerMasks != null)
            {
                for (int i = 0; i < jsonLayerMasks.Length; i++)
                {
                    var jsonMask = jsonLayerMasks[i];
                    
                    // Ensure heights are generated for lazy masks
                    if (jsonMask != null)
                    {
                        EnsureMaskHeightsGenerated(jsonMask);
                        
                        if (jsonMask.heights != null)
                        {
                            int rows = jsonMask.heights.Length;
                            int cols = rows > 0 ? jsonMask.heights[0]?.Length ?? 0 : 0;
                            
                            if (rows > 0 && cols > 0)
                            {
                                var apiMask = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityLayerMask(jsonMask.heights);
                                apiLayerMasks.AddLayerMask(apiMask);
                            }
                            else
                            {
                                Logging.LogWarning($"[ConvertToAPILayerMasks] Mask {i} has invalid dimensions: {rows}x{cols}");
                            }
                        }
                        else
                        {
                            Logging.LogWarning($"[ConvertToAPILayerMasks] Mask {i} has null heights data after lazy generation");
                        }
                    }
                    else
                    {
                        Logging.LogWarning($"[ConvertToAPILayerMasks] Mask {i} is null");
                    }
                }
            }
            else
            {
                Logging.LogWarning("[ConvertToAPILayerMasks] jsonLayerMasks is null");
            }

            return apiLayerMasks;
        }

        /// <summary>
        /// Convert JSON terrain entity modifications to API terrain entity modifications.
        /// Supports both position object format and individual x/y/z coordinate format.
        /// </summary>
        private FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[] ConvertToAPIModifications(JSONTerrainEntityModification[] jsonModifications)
        {
            if (jsonModifications == null) return new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[0];

            var apiModifications = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[jsonModifications.Length];
            for (int i = 0; i < jsonModifications.Length; i++)
            {
                var jsonMod = jsonModifications[i];
                
                // Convert operation string to enum
                var operation = jsonMod.operation == "dig" ? 
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Dig :
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Build;

                // Convert brush type string to enum (support both camelCase and lowercase)
                string brushTypeStr = !string.IsNullOrEmpty(jsonMod.brushType) ? jsonMod.brushType : jsonMod.brushtype;
                var brushType = brushTypeStr == "roundedCube" ?
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.roundedCube :
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.sphere;

                // Convert position (support both position object and individual x/y/z coordinates)
                FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3 position;
                if (jsonMod.position != null && (jsonMod.position.x != 0 || jsonMod.position.y != 0 || jsonMod.position.z != 0))
                {
                    // Use position object if it exists and has non-zero values
                    position = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                        jsonMod.position.x, jsonMod.position.y, jsonMod.position.z);
                }
                else
                {
                    // Use individual x/y/z coordinates
                    position = new Javascript.APIs.WorldTypes.Vector3(
                        jsonMod.x, jsonMod.y, jsonMod.z);
                }

                float modificationSize = jsonMod.brushsize;
                if (modificationSize <= 0) 
                {
                    modificationSize = 1.0f; // Default to 1.0 if size is 0 or negative
                }
                
                apiModifications[i] = new Javascript.APIs.Entity.TerrainEntityModification(
                    operation, position, brushType, jsonMod.layer, modificationSize);
                
                // Verify the API modification was created with the correct size
                var createdMod = apiModifications[i];
            }
            return apiModifications;
        }

        /// <summary>
        /// Thread-safe version of ConvertToAPIModifications for background processing.
        /// </summary>
        /// <param name="jsonModifications">JSON modifications to convert</param>
        /// <returns>Array of API terrain entity modifications</returns>
        private static FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[] ConvertToAPIModificationsThreadSafe(JSONTerrainEntityModification[] jsonModifications)
        {
            if (jsonModifications == null) return new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[0];

            var apiModifications = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification[jsonModifications.Length];
            for (int i = 0; i < jsonModifications.Length; i++)
            {
                var jsonMod = jsonModifications[i];
                
                // Convert operation string to enum
                var operation = jsonMod.operation == "dig" ? 
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Dig :
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityModification.TerrainEntityOperation.Build;

                // Convert brush type string to enum (support both camelCase and lowercase)
                string brushTypeStr = !string.IsNullOrEmpty(jsonMod.brushType) ? jsonMod.brushType : jsonMod.brushtype;
                var brushType = brushTypeStr == "roundedCube" ?
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.roundedCube :
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TerrainEntityBrushType.sphere;

                // Convert position (support both position object and individual x/y/z coordinates)
                FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3 position;
                if (jsonMod.position != null && (jsonMod.position.x != 0 || jsonMod.position.y != 0 || jsonMod.position.z != 0))
                {
                    // Use position object if it exists and has non-zero values
                    position = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                        jsonMod.position.x, jsonMod.position.y, jsonMod.position.z);
                }
                else
                {
                    // Use individual x/y/z coordinates
                    position = new Javascript.APIs.WorldTypes.Vector3(
                        jsonMod.x, jsonMod.y, jsonMod.z);
                }

                float modificationSize = jsonMod.brushsize;
                if (modificationSize <= 0) 
                {
                    modificationSize = 1.0f; // Default to 1.0 if size is 0 or negative
                }

                apiModifications[i] = new Javascript.APIs.Entity.TerrainEntityModification(
                    operation, position, brushType, jsonMod.layer, modificationSize);
            }
            return apiModifications;
        }

        /// <summary>
        /// Coroutine to create child terrain entities.
        /// </summary>
        private IEnumerator CreateTerrainChildrenCoroutine(JSONTerrainEntity[] children, BaseEntity parentEntity, Action onComplete)
        {
            if (children == null || children.Length == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            int completedCount = 0;
            int totalCount = children.Length;

            foreach (var child in children)
            {
                CreateTerrainEntity(child, parentEntity, (childId, childEntity) =>
                {
                    completedCount++;
                    if (completedCount >= totalCount)
                    {
                        onComplete?.Invoke();
                    }
                });
                
                // Yield control to prevent blocking
                yield return null;
            }
        }

        #endregion

        #region Airplane Entity Methods

        /// <summary>
        /// Create an airplane entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="airplaneData">JSON airplane entity data</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateAirplaneEntity(JSONAirplaneEntity airplaneData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (airplaneData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateAirplaneEntity] Airplane data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateAirplaneEntity(airplaneData))
            {
                Logging.LogError("[JSONEntityHandler->CreateAirplaneEntity] Invalid airplane entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateAirplaneEntityCoroutine(airplaneData, parentEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create an airplane entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing airplane entity data</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadAirplaneEntityFromJSON(string jsonData, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadAirplaneEntityFromJSON] Processing airplane entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONAirplaneEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadAirplaneEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateAirplaneEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadAirplaneEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadAirplaneEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadAirplaneEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadAirplaneEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate airplane entity data.
        /// </summary>
        private bool ValidateAirplaneEntity(JSONAirplaneEntity airplaneData)
        {
            if (string.IsNullOrEmpty(airplaneData.meshObject))
            {
                Logging.LogError("[JSONEntityHandler->ValidateAirplaneEntity] Mesh object path is required");
                return false;
            }

            if (airplaneData.mass <= 0)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateAirplaneEntity] Mass should be greater than 0, using default value");
                airplaneData.mass = 1000f;
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create an airplane entity.
        /// </summary>
        private IEnumerator CreateAirplaneEntityCoroutine(JSONAirplaneEntity airplaneData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateAirplaneEntityCoroutine] Starting airplane entity creation");

            // Convert positions
            UnityEngine.Vector3 position = new UnityEngine.Vector3(
                airplaneData.position.x, airplaneData.position.y, airplaneData.position.z);
            UnityEngine.Quaternion rotation = new UnityEngine.Quaternion(
                airplaneData.rotation.x, airplaneData.rotation.y, airplaneData.rotation.z, airplaneData.rotation.w);

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(airplaneData.id))
            {
                if (System.Guid.TryParse(airplaneData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Use GLTFHandler to create airplane entity directly
            Guid finalEntityId = entityId ?? Guid.NewGuid();
            Guid createdEntityId = WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAirplaneEntity(
                airplaneData.meshObject,
                airplaneData.meshResources,
                position,
                rotation,
                airplaneData.mass,
                finalEntityId,
                (airplaneEntity) =>
                {
                    if (airplaneEntity != null)
                    {
                        Logging.Log($"[JSONEntityHandler->CreateAirplaneEntity] Airplane entity created successfully with ID: {finalEntityId}");

                        // Set parent if provided
                        if (parentEntity != null)
                        {
                            airplaneEntity.SetParent(parentEntity);
                        }

                        // Set additional airplane properties
                        airplaneEntity.throttle = airplaneData.throttle;
                        airplaneEntity.pitch = airplaneData.pitch;
                        airplaneEntity.roll = airplaneData.roll;
                        airplaneEntity.yaw = airplaneData.yaw;

                        // Handle children if any
                        if (airplaneData.children != null && airplaneData.children.Length > 0)
                        {
                            StartCoroutine(CreateAirplaneChildrenCoroutine(airplaneData.children, airplaneEntity, () =>
                            {
                                onComplete?.Invoke(finalEntityId, airplaneEntity);
                            }));
                        }
                        else
                        {
                            onComplete?.Invoke(finalEntityId, airplaneEntity);
                        }
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->CreateAirplaneEntity] Failed to create airplane entity");
                        onComplete?.Invoke(null, null);
                    }
                },
                10, // timeout
                airplaneData.checkForUpdateIfCached
            );

            yield return null;
        }

        /// <summary>
        /// Create airplane entity children recursively.
        /// </summary>
        private IEnumerator CreateAirplaneChildrenCoroutine(JSONAirplaneEntity[] children, BaseEntity parentEntity, Action onComplete)
        {
            if (children == null || children.Length == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            int completedCount = 0;
            int totalCount = children.Length;

            foreach (var child in children)
            {
                CreateAirplaneEntity(child, parentEntity, (childId, childEntity) =>
                {
                    completedCount++;
                    if (completedCount >= totalCount)
                    {
                        onComplete?.Invoke();
                    }
                });
                
                // Yield control to prevent blocking
                yield return null;
            }
        }

        #endregion

        #region Automobile Entity Methods

        /// <summary>
        /// Create an automobile entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="automobileData">JSON automobile entity data</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateAutomobileEntity(JSONAutomobileEntity automobileData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (automobileData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateAutomobileEntity] Automobile data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateAutomobileEntity(automobileData))
            {
                Logging.LogError("[JSONEntityHandler->CreateAutomobileEntity] Invalid automobile entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateAutomobileEntityCoroutine(automobileData, parentEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create an automobile entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing automobile entity data</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadAutomobileEntityFromJSON(string jsonData, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadAutomobileEntityFromJSON] Processing automobile entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONAutomobileEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadAutomobileEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateAutomobileEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadAutomobileEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadAutomobileEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadAutomobileEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadAutomobileEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate automobile entity data.
        /// </summary>
        private bool ValidateAutomobileEntity(JSONAutomobileEntity automobileData)
        {
            if (string.IsNullOrEmpty(automobileData.meshObject))
            {
                Logging.LogError("[JSONEntityHandler->ValidateAutomobileEntity] Mesh object path is required");
                return false;
            }

            if (automobileData.mass <= 0)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateAutomobileEntity] Mass should be greater than 0, using default value");
                automobileData.mass = 1500f;
            }

            if (automobileData.wheels == null || automobileData.wheels.Length == 0)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateAutomobileEntity] No wheels defined, using default configuration");
                automobileData.wheels = new JSONAutomobileEntityWheel[]
                {
                    new JSONAutomobileEntityWheel("FrontLeft", 0.5f),
                    new JSONAutomobileEntityWheel("FrontRight", 0.5f),
                    new JSONAutomobileEntityWheel("RearLeft", 0.5f),
                    new JSONAutomobileEntityWheel("RearRight", 0.5f)
                };
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create an automobile entity.
        /// </summary>
        private IEnumerator CreateAutomobileEntityCoroutine(JSONAutomobileEntity automobileData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateAutomobileEntityCoroutine] Starting automobile entity creation");

            // Convert positions
            UnityEngine.Vector3 position = new UnityEngine.Vector3(
                automobileData.position.x, automobileData.position.y, automobileData.position.z);
            UnityEngine.Quaternion rotation = new UnityEngine.Quaternion(
                automobileData.rotation.x, automobileData.rotation.y, automobileData.rotation.z, automobileData.rotation.w);

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(automobileData.id))
            {
                if (System.Guid.TryParse(automobileData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert JSON wheels to AutomobileEntityWheel array
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AutomobileEntityWheel[] wheels = 
                new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AutomobileEntityWheel[automobileData.wheels.Length];
            for (int i = 0; i < automobileData.wheels.Length; i++)
            {
                wheels[i] = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AutomobileEntityWheel(
                    automobileData.wheels[i].wheelSubMesh,
                    automobileData.wheels[i].wheelRadius
                );
            }

            // Convert automobile type string to enum
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AutomobileEntity.AutomobileType automobileType = 
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AutomobileEntity.AutomobileType.Default;
            if (!string.IsNullOrEmpty(automobileData.automobileType))
            {
                if (!System.Enum.TryParse<FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AutomobileEntity.AutomobileType>(
                    automobileData.automobileType, out automobileType))
                {
                    Logging.LogWarning($"[JSONEntityHandler->CreateAutomobileEntity] Unknown automobile type: {automobileData.automobileType}, using Default");
                    automobileType = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AutomobileEntity.AutomobileType.Default;
                }
            }

            // Use GLTFHandler to create automobile entity directly
            Guid finalEntityId = entityId ?? Guid.NewGuid();
            Guid createdEntityId = WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAutomobileEntity(
                automobileData.meshObject,
                automobileData.meshResources,
                position,
                rotation,
                wheels,
                automobileData.mass,
                automobileType,
                finalEntityId,
                (automobileEntity) =>
                {
                    if (automobileEntity != null)
                    {
                        Logging.Log($"[JSONEntityHandler->CreateAutomobileEntity] Automobile entity created successfully with ID: {finalEntityId}");

                        // Set parent if provided
                        if (parentEntity != null)
                        {
                            automobileEntity.SetParent(parentEntity);
                        }

                        // Set additional automobile properties
                        automobileEntity.throttle = automobileData.throttle;
                        automobileEntity.steer = automobileData.steer;
                        automobileEntity.brake = automobileData.brake;
                        automobileEntity.handBrake = automobileData.handBrake;
                        automobileEntity.horn = automobileData.horn;
                        automobileEntity.gear = automobileData.gear;
                        automobileEntity.engineStartStop = automobileData.engineStartStop;

                        // Handle children if any
                        if (automobileData.children != null && automobileData.children.Length > 0)
                        {
                            StartCoroutine(CreateAutomobileChildrenCoroutine(automobileData.children, automobileEntity, () =>
                            {
                                onComplete?.Invoke(finalEntityId, automobileEntity);
                            }));
                        }
                        else
                        {
                            onComplete?.Invoke(finalEntityId, automobileEntity);
                        }
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->CreateAutomobileEntity] Failed to create automobile entity");
                        onComplete?.Invoke(null, null);
                    }
                },
                10, // timeout
                automobileData.checkForUpdateIfCached
            );

            yield return null;
        }

        /// <summary>
        /// Create automobile entity children recursively.
        /// </summary>
        private IEnumerator CreateAutomobileChildrenCoroutine(JSONAutomobileEntity[] children, BaseEntity parentEntity, Action onComplete)
        {
            if (children == null || children.Length == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            int completedCount = 0;
            int totalCount = children.Length;

            foreach (var child in children)
            {
                CreateAutomobileEntity(child, parentEntity, (childId, childEntity) =>
                {
                    completedCount++;
                    if (completedCount >= totalCount)
                    {
                        onComplete?.Invoke();
                    }
                });
                
                // Yield control to prevent blocking
                yield return null;
            }
        }

        #endregion

        #region Canvas Entity Methods

        /// <summary>
        /// Create a canvas entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="canvasData">JSON canvas entity data</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateCanvasEntity(JSONCanvasEntity canvasData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (canvasData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateCanvasEntity] Canvas data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateCanvasEntity(canvasData))
            {
                Logging.LogError("[JSONEntityHandler->CreateCanvasEntity] Invalid canvas entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateCanvasEntityCoroutine(canvasData, parentEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create a canvas entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing canvas entity data</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadCanvasEntityFromJSON(string jsonData, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadCanvasEntityFromJSON] Processing canvas entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONCanvasEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadCanvasEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateCanvasEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadCanvasEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadCanvasEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadCanvasEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadCanvasEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate canvas entity data.
        /// </summary>
        private bool ValidateCanvasEntity(JSONCanvasEntity canvasData)
        {
            // Canvas entities have minimal requirements - mostly just need valid transform data
            return true;
        }

        /// <summary>
        /// Coroutine to create a canvas entity.
        /// </summary>
        private IEnumerator CreateCanvasEntityCoroutine(JSONCanvasEntity canvasData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateCanvasEntityCoroutine] Starting canvas entity creation");

            // Convert JSON types to WorldTypes
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                canvasData.position.x, canvasData.position.y, canvasData.position.z);
            var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                canvasData.rotation.x, canvasData.rotation.y, canvasData.rotation.z, canvasData.rotation.w);
            var worldScale = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                canvasData.scale.x, canvasData.scale.y, canvasData.scale.z);

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(canvasData.id))
            {
                if (System.Guid.TryParse(canvasData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentEntity to public API entity
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
            if (parentEntity != null)
            {
                apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity);
            }

            // Create the canvas entity using CanvasEntity.Create
            var canvasEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CanvasEntity.Create(
                apiParent,
                worldPosition,
                worldRotation,
                worldScale,
                canvasData.isSize,
                entityId?.ToString(),
                canvasData.tag,
                null // onLoaded callback handled differently
            );

            if (canvasEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateCanvasEntity] Canvas entity created successfully");

                // Set canvas type (world or screen canvas)
                if (!string.IsNullOrEmpty(canvasData.canvasType))
                {
                    if (canvasData.canvasType.ToLower() == "world")
                    {
                        canvasEntity.MakeWorldCanvas();
                    }
                    else
                    {
                        canvasEntity.MakeScreenCanvas();
                    }
                }

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(canvasEntity);

                // Handle children if any
                if (canvasData.children != null && canvasData.children.Length > 0)
                {
                    StartCoroutine(CreateCanvasChildrenCoroutine(canvasData.children, internalEntity, () =>
                    {
                        onComplete?.Invoke(internalEntity.id, internalEntity);
                    }));
                }
                else
                {
                    onComplete?.Invoke(internalEntity.id, internalEntity);
                }
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateCanvasEntity] Failed to create canvas entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        /// <summary>
        /// Create canvas entity children recursively.
        /// </summary>
        private IEnumerator CreateCanvasChildrenCoroutine(JSONCanvasEntity[] children, BaseEntity parentEntity, Action onComplete)
        {
            if (children == null || children.Length == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            int completedCount = 0;
            int totalCount = children.Length;

            foreach (var child in children)
            {
                CreateCanvasEntity(child, parentEntity, (childId, childEntity) =>
                {
                    completedCount++;
                    if (completedCount >= totalCount)
                    {
                        onComplete?.Invoke();
                    }
                });
                
                // Yield control to prevent blocking
                yield return null;
            }
        }

        #endregion

        #region Button Entity Methods

        /// <summary>
        /// Create a button entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="buttonData">JSON button entity data</param>
        /// <param name="parentCanvasEntity">Parent canvas entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateButtonEntity(JSONButtonEntity buttonData, BaseEntity parentCanvasEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (buttonData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateButtonEntity] Button data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateButtonEntity(buttonData))
            {
                Logging.LogError("[JSONEntityHandler->CreateButtonEntity] Invalid button entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateButtonEntityCoroutine(buttonData, parentCanvasEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create a button entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing button entity data</param>
        /// <param name="parentCanvasEntity">Parent canvas entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadButtonEntityFromJSON(string jsonData, BaseEntity parentCanvasEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadButtonEntityFromJSON] Processing button entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONButtonEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadButtonEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateButtonEntity(entityData, parentCanvasEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadButtonEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadButtonEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadButtonEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadButtonEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate button entity data.
        /// </summary>
        private bool ValidateButtonEntity(JSONButtonEntity buttonData)
        {
            if (buttonData.positionPercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateButtonEntity] Position percent is required");
                return false;
            }

            if (buttonData.sizePercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateButtonEntity] Size percent is required");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create a button entity.
        /// </summary>
        private IEnumerator CreateButtonEntityCoroutine(JSONButtonEntity buttonData, BaseEntity parentCanvasEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateButtonEntityCoroutine] Starting button entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(buttonData.id))
            {
                if (System.Guid.TryParse(buttonData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentCanvasEntity to public API canvas entity
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CanvasEntity apiParentCanvas = null;
            if (parentCanvasEntity != null)
            {
                var publicParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentCanvasEntity);
                if (publicParent is FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CanvasEntity canvasParent)
                {
                    apiParentCanvas = canvasParent;
                }
                else
                {
                    Logging.LogError("[JSONEntityHandler->CreateButtonEntity] Parent entity is not a canvas entity");
                    onComplete?.Invoke(null, null);
                    yield break;
                }
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateButtonEntity] Parent canvas entity is required");
                onComplete?.Invoke(null, null);
                yield break;
            }

            // Convert JSON position and size to WorldTypes Vector2
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                buttonData.positionPercent.x, buttonData.positionPercent.y);
            var worldSize = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                buttonData.sizePercent.x, buttonData.sizePercent.y);

            // Create the button entity using ButtonEntity.Create
            var buttonEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.ButtonEntity.Create(
                apiParentCanvas,
                buttonData.onClick,
                worldPosition,
                worldSize,
                entityId?.ToString(),
                buttonData.tag,
                null // onLoaded callback handled differently
            );

            if (buttonEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateButtonEntity] Button entity created successfully");

                // Set additional button properties
                if (buttonData.backgroundColor != null)
                {
                    var bgColor = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(
                        buttonData.backgroundColor.r, buttonData.backgroundColor.g, buttonData.backgroundColor.b, buttonData.backgroundColor.a);
                    buttonEntity.SetBaseColor(bgColor);
                }

                if (!string.IsNullOrEmpty(buttonData.backgroundImage))
                {
                    buttonEntity.SetBackground(buttonData.backgroundImage);
                }

                // Note: Text properties are typically handled by child TextEntity elements

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(buttonEntity);

                // Handle children if any (though buttons typically don't have children)
                if (buttonData.children != null && buttonData.children.Length > 0)
                {
                    StartCoroutine(CreateButtonChildrenCoroutine(buttonData.children, internalEntity, () =>
                    {
                        onComplete?.Invoke(internalEntity.id, internalEntity);
                    }));
                }
                else
                {
                    onComplete?.Invoke(internalEntity.id, internalEntity);
                }
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateButtonEntity] Failed to create button entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        /// <summary>
        /// Create button entity children recursively.
        /// </summary>
        private IEnumerator CreateButtonChildrenCoroutine(JSONButtonEntity[] children, BaseEntity parentEntity, Action onComplete)
        {
            if (children == null || children.Length == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            int completedCount = 0;
            int totalCount = children.Length;

            foreach (var child in children)
            {
                CreateButtonEntity(child, parentEntity, (childId, childEntity) =>
                {
                    completedCount++;
                    if (completedCount >= totalCount)
                    {
                        onComplete?.Invoke();
                    }
                });
                
                // Yield control to prevent blocking
                yield return null;
            }
        }

        #endregion

        #region Text Entity Methods

        /// <summary>
        /// Create a text entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="textData">JSON text entity data</param>
        /// <param name="parentUIEntity">Parent UI entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateTextEntity(JSONTextEntity textData, BaseEntity parentUIEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (textData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateTextEntity] Text data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateTextEntity(textData))
            {
                Logging.LogError("[JSONEntityHandler->CreateTextEntity] Invalid text entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateTextEntityCoroutine(textData, parentUIEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create a text entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing text entity data</param>
        /// <param name="parentUIEntity">Parent UI entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadTextEntityFromJSON(string jsonData, BaseEntity parentUIEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadTextEntityFromJSON] Processing text entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONTextEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadTextEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateTextEntity(entityData, parentUIEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadTextEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadTextEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadTextEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadTextEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate text entity data.
        /// </summary>
        private bool ValidateTextEntity(JSONTextEntity textData)
        {
            if (string.IsNullOrEmpty(textData.text))
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateTextEntity] Text is empty, using default");
                textData.text = "Text";
            }

            if (textData.positionPercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateTextEntity] Position percent is required");
                return false;
            }

            if (textData.sizePercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateTextEntity] Size percent is required");
                return false;
            }

            if (textData.fontSize <= 0)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateTextEntity] Invalid font size, using default");
                textData.fontSize = 14;
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create a text entity.
        /// </summary>
        private IEnumerator CreateTextEntityCoroutine(JSONTextEntity textData, BaseEntity parentUIEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateTextEntityCoroutine] Starting text entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(textData.id))
            {
                if (System.Guid.TryParse(textData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentUIEntity to public API entity
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParentUI = null;
            if (parentUIEntity != null)
            {
                apiParentUI = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentUIEntity);
                if (apiParentUI == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateTextEntity] Failed to convert parent entity to public API");
                    onComplete?.Invoke(null, null);
                    yield break;
                }
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateTextEntity] Parent UI entity is required");
                onComplete?.Invoke(null, null);
                yield break;
            }

            // Convert JSON position and size to WorldTypes Vector2
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                textData.positionPercent.x, textData.positionPercent.y);
            var worldSize = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                textData.sizePercent.x, textData.sizePercent.y);

            // Create the text entity using TextEntity.Create
            var textEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextEntity.Create(
                apiParentUI,
                textData.text,
                textData.fontSize,
                worldPosition,
                worldSize,
                entityId?.ToString(),
                textData.tag,
                null // onLoaded callback handled differently
            );

            if (textEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateTextEntity] Text entity created successfully");

                // Set additional text properties
                if (textData.color != null)
                {
                    var textColor = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(
                        textData.color.r, textData.color.g, textData.color.b, textData.color.a);
                    textEntity.SetColor(textColor);
                }

                // Set text alignment if specified
                if (!string.IsNullOrEmpty(textData.alignment))
                {
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextAlignment alignment = 
                        FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextAlignment.Left;
                    
                    switch (textData.alignment.ToLower())
                    {
                        case "center":
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextAlignment.Center;
                            break;
                        case "right":
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextAlignment.Right;
                            break;
                        case "top":
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextAlignment.Top;
                            break;
                        case "bottom":
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextAlignment.Bottom;
                            break;
                        default:
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextAlignment.Left;
                            break;
                    }
                    
                    textEntity.SetTextAlignment(alignment);
                }

                // Set text wrapping
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextWrapping wrapping = 
                    textData.wordWrap ? FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextWrapping.Wrap 
                                     : FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.TextWrapping.NoWrap;
                textEntity.SetTextWrapping(wrapping);

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(textEntity);

                // Handle children if any (though text entities typically don't have children)
                if (textData.children != null && textData.children.Length > 0)
                {
                    StartCoroutine(CreateTextChildrenCoroutine(textData.children, internalEntity, () =>
                    {
                        onComplete?.Invoke(internalEntity.id, internalEntity);
                    }));
                }
                else
                {
                    onComplete?.Invoke(internalEntity.id, internalEntity);
                }
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateTextEntity] Failed to create text entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        /// <summary>
        /// Create text entity children recursively.
        /// </summary>
        private IEnumerator CreateTextChildrenCoroutine(JSONTextEntity[] children, BaseEntity parentEntity, Action onComplete)
        {
            if (children == null || children.Length == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            int completedCount = 0;
            int totalCount = children.Length;

            foreach (var child in children)
            {
                CreateTextEntity(child, parentEntity, (childId, childEntity) =>
                {
                    completedCount++;
                    if (completedCount >= totalCount)
                    {
                        onComplete?.Invoke();
                    }
                });
                
                // Yield control to prevent blocking
                yield return null;
            }
        }

        #endregion

        #region Input Entity Methods

        /// <summary>
        /// Create an input entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="inputData">JSON input entity data</param>
        /// <param name="parentUIEntity">Parent UI entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateInputEntity(JSONInputEntity inputData, BaseEntity parentUIEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (inputData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateInputEntity] Input data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateInputEntity(inputData))
            {
                Logging.LogError("[JSONEntityHandler->CreateInputEntity] Invalid input entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateInputEntityCoroutine(inputData, parentUIEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create an input entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing input entity data</param>
        /// <param name="parentUIEntity">Parent UI entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadInputEntityFromJSON(string jsonData, BaseEntity parentUIEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadInputEntityFromJSON] Processing input entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONInputEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadInputEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateInputEntity(entityData, parentUIEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadInputEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadInputEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadInputEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadInputEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate input entity data.
        /// </summary>
        private bool ValidateInputEntity(JSONInputEntity inputData)
        {
            if (inputData.positionPercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateInputEntity] Position percent is required");
                return false;
            }

            if (inputData.sizePercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateInputEntity] Size percent is required");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create an input entity.
        /// </summary>
        private IEnumerator CreateInputEntityCoroutine(JSONInputEntity inputData, BaseEntity parentUIEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateInputEntityCoroutine] Starting input entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(inputData.id))
            {
                if (System.Guid.TryParse(inputData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentUIEntity to canvas entity
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CanvasEntity apiCanvas = null;
            if (parentUIEntity != null)
            {
                var publicEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentUIEntity);
                if (publicEntity is FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CanvasEntity canvas)
                {
                    apiCanvas = canvas;
                }
                else
                {
                    Logging.LogError("[JSONEntityHandler->CreateInputEntity] Parent must be a canvas entity");
                    onComplete?.Invoke(null, null);
                    yield break;
                }
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateInputEntity] Parent canvas entity is required");
                onComplete?.Invoke(null, null);
                yield break;
            }

            // Convert JSON position and size to WorldTypes Vector2
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                inputData.positionPercent.x, inputData.positionPercent.y);
            var worldSize = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                inputData.sizePercent.x, inputData.sizePercent.y);

            // Create the input entity using InputEntity.Create
            var inputEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.InputEntity.Create(
                apiCanvas,
                worldPosition,
                worldSize,
                entityId?.ToString(),
                inputData.tag,
                null // onLoaded callback handled differently
            );

            if (inputEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateInputEntity] Input entity created successfully");

                // Set input text if provided
                if (!string.IsNullOrEmpty(inputData.text))
                {
                    inputEntity.SetText(inputData.text);
                }

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(inputEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateInputEntity] Failed to create input entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        #endregion

        #region Dropdown Entity Methods

        /// <summary>
        /// Create a dropdown entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="dropdownData">JSON dropdown entity data</param>
        /// <param name="parentUIEntity">Parent UI entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateDropdownEntity(JSONDropdownEntity dropdownData, BaseEntity parentUIEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (dropdownData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateDropdownEntity] Dropdown data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateDropdownEntity(dropdownData))
            {
                Logging.LogError("[JSONEntityHandler->CreateDropdownEntity] Invalid dropdown entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateDropdownEntityCoroutine(dropdownData, parentUIEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create a dropdown entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing dropdown entity data</param>
        /// <param name="parentUIEntity">Parent UI entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadDropdownEntityFromJSON(string jsonData, BaseEntity parentUIEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadDropdownEntityFromJSON] Processing dropdown entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONDropdownEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadDropdownEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateDropdownEntity(entityData, parentUIEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadDropdownEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadDropdownEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadDropdownEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadDropdownEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate dropdown entity data.
        /// </summary>
        private bool ValidateDropdownEntity(JSONDropdownEntity dropdownData)
        {
            if (dropdownData.positionPercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateDropdownEntity] Position percent is required");
                return false;
            }

            if (dropdownData.sizePercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateDropdownEntity] Size percent is required");
                return false;
            }

            if (dropdownData.options == null || dropdownData.options.Length == 0)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateDropdownEntity] No options provided, using default");
                dropdownData.options = new string[] { "Option 1", "Option 2" };
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create a dropdown entity.
        /// </summary>
        private IEnumerator CreateDropdownEntityCoroutine(JSONDropdownEntity dropdownData, BaseEntity parentUIEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateDropdownEntityCoroutine] Starting dropdown entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(dropdownData.id))
            {
                if (System.Guid.TryParse(dropdownData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentUIEntity to canvas entity
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CanvasEntity apiCanvas = null;
            if (parentUIEntity != null)
            {
                var publicEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentUIEntity);
                if (publicEntity is FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CanvasEntity canvas)
                {
                    apiCanvas = canvas;
                }
                else
                {
                    Logging.LogError("[JSONEntityHandler->CreateDropdownEntity] Parent must be a canvas entity");
                    onComplete?.Invoke(null, null);
                    yield break;
                }
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateDropdownEntity] Parent canvas entity is required");
                onComplete?.Invoke(null, null);
                yield break;
            }

            // Convert JSON position and size to WorldTypes Vector2
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                dropdownData.positionPercent.x, dropdownData.positionPercent.y);
            var worldSize = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                dropdownData.sizePercent.x, dropdownData.sizePercent.y);

            // Create the dropdown entity using DropdownEntity.Create
            var dropdownEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.DropdownEntity.Create(
                apiCanvas,
                dropdownData.onChange, // onChange callback as string
                worldPosition,
                worldSize,
                dropdownData.options,
                entityId?.ToString(),
                dropdownData.tag,
                null // onLoaded callback handled differently
            );

            if (dropdownEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateDropdownEntity] Dropdown entity created successfully");

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(dropdownEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateDropdownEntity] Failed to create dropdown entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        #endregion

        #region Image Entity Methods

        /// <summary>
        /// Create an image entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="imageData">JSON image entity data</param>
        /// <param name="parentUIEntity">Parent UI entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateImageEntity(JSONImageEntity imageData, BaseEntity parentUIEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (imageData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateImageEntity] Image data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateImageEntity(imageData))
            {
                Logging.LogError("[JSONEntityHandler->CreateImageEntity] Invalid image entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateImageEntityCoroutine(imageData, parentUIEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create an image entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing image entity data</param>
        /// <param name="parentUIEntity">Parent UI entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadImageEntityFromJSON(string jsonData, BaseEntity parentUIEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadImageEntityFromJSON] Processing image entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONImageEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadImageEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateImageEntity(entityData, parentUIEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadImageEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadImageEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadImageEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadImageEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate image entity data.
        /// </summary>
        private bool ValidateImageEntity(JSONImageEntity imageData)
        {
            if (string.IsNullOrEmpty(imageData.imageFile))
            {
                Logging.LogError("[JSONEntityHandler->ValidateImageEntity] Image file path is required");
                return false;
            }

            if (imageData.positionPercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateImageEntity] Position percent is required");
                return false;
            }

            if (imageData.sizePercent == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateImageEntity] Size percent is required");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create an image entity.
        /// </summary>
        private IEnumerator CreateImageEntityCoroutine(JSONImageEntity imageData, BaseEntity parentUIEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateImageEntityCoroutine] Starting image entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(imageData.id))
            {
                if (System.Guid.TryParse(imageData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentUIEntity to public API entity
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParentUI = null;
            if (parentUIEntity != null)
            {
                apiParentUI = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentUIEntity);
                if (apiParentUI == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateImageEntity] Failed to convert parent entity to public API");
                    onComplete?.Invoke(null, null);
                    yield break;
                }
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateImageEntity] Parent UI entity is required");
                onComplete?.Invoke(null, null);
                yield break;
            }

            // Convert JSON position and size to WorldTypes Vector2
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                imageData.positionPercent.x, imageData.positionPercent.y);
            var worldSize = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                imageData.sizePercent.x, imageData.sizePercent.y);

            // Create the image entity using ImageEntity.Create
            var imageEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.ImageEntity.Create(
                apiParentUI,
                imageData.imageFile,
                worldPosition,
                worldSize,
                entityId?.ToString(),
                imageData.tag,
                null // onLoaded callback handled differently
            );

            if (imageEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateImageEntity] Image entity created successfully");

                // Set stretch to parent if specified
                if (imageData.stretchToParent)
                {
                    imageEntity.StretchToParent(true);
                }

                // Set alignment if specified
                if (!string.IsNullOrEmpty(imageData.alignment))
                {
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.UIElementAlignment alignment = 
                        FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.UIElementAlignment.Center;
                    
                    switch (imageData.alignment.ToLower())
                    {
                        case "left":
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.UIElementAlignment.Left;
                            break;
                        case "right":
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.UIElementAlignment.Right;
                            break;
                        case "top":
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.UIElementAlignment.Top;
                            break;
                        case "bottom":
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.UIElementAlignment.Bottom;
                            break;
                        default:
                            alignment = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.UIElementAlignment.Center;
                            break;
                    }
                    
                    imageEntity.SetAlignment(alignment);
                }

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(imageEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateImageEntity] Failed to create image entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        #endregion

        #region HTML Entity Methods

        /// <summary>
        /// Create an HTML entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="htmlData">JSON HTML entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateHTMLEntity(JSONHTMLEntity htmlData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (htmlData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateHTMLEntity] HTML data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateHTMLEntity(htmlData))
            {
                Logging.LogError("[JSONEntityHandler->CreateHTMLEntity] Invalid HTML entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateHTMLEntityCoroutine(htmlData, parentEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create an HTML entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing HTML entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadHTMLEntityFromJSON(string jsonData, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadHTMLEntityFromJSON] Processing HTML entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONHTMLEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadHTMLEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateHTMLEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadHTMLEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadHTMLEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadHTMLEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadHTMLEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate HTML entity data.
        /// </summary>
        private bool ValidateHTMLEntity(JSONHTMLEntity htmlData)
        {
            if (htmlData.isCanvasElement)
            {
                if (htmlData.positionPercent == null)
                {
                    Logging.LogError("[JSONEntityHandler->ValidateHTMLEntity] Position percent is required for canvas elements");
                    return false;
                }

                if (htmlData.sizePercent == null)
                {
                    Logging.LogError("[JSONEntityHandler->ValidateHTMLEntity] Size percent is required for canvas elements");
                    return false;
                }
            }
            else
            {
                if (htmlData.position == null)
                {
                    Logging.LogError("[JSONEntityHandler->ValidateHTMLEntity] Position is required for 3D HTML entities");
                    return false;
                }

                if (htmlData.rotation == null)
                {
                    Logging.LogWarning("[JSONEntityHandler->ValidateHTMLEntity] Rotation not specified, using default");
                    htmlData.rotation = new JSONQuaternion { x = 0, y = 0, z = 0, w = 1 };
                }

                if (htmlData.scale == null)
                {
                    Logging.LogWarning("[JSONEntityHandler->ValidateHTMLEntity] Scale not specified, using default");
                    htmlData.scale = new JSONVector3 { x = 1, y = 1, z = 1 };
                }
            }

            if (string.IsNullOrEmpty(htmlData.url) && string.IsNullOrEmpty(htmlData.html))
            {
                Logging.LogError("[JSONEntityHandler->ValidateHTMLEntity] Either URL or HTML content must be provided");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create an HTML entity.
        /// </summary>
        private IEnumerator CreateHTMLEntityCoroutine(JSONHTMLEntity htmlData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateHTMLEntityCoroutine] Starting HTML entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(htmlData.id))
            {
                if (System.Guid.TryParse(htmlData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.HTMLEntity htmlEntity = null;

            if (htmlData.isCanvasElement)
            {
                // Create HTML UI element for canvas placement
                // Convert parentEntity to public API entity
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
                if (parentEntity != null)
                {
                    apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity);
                    if (apiParent == null)
                    {
                        Logging.LogError("[JSONEntityHandler->CreateHTMLEntity] Failed to convert parent entity to public API");
                        onComplete?.Invoke(null, null);
                        yield break;
                    }
                }
                else
                {
                    Logging.LogError("[JSONEntityHandler->CreateHTMLEntity] Parent entity is required for canvas HTML elements");
                    onComplete?.Invoke(null, null);
                    yield break;
                }

                var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                    htmlData.positionPercent.x, htmlData.positionPercent.y);
                var worldSize = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector2(
                    htmlData.sizePercent.x, htmlData.sizePercent.y);

                htmlEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.HTMLEntity.Create(
                    apiParent,
                    worldPosition,
                    worldSize,
                    entityId?.ToString(),
                    htmlData.tag,
                    htmlData.onMessage,
                    null // onLoaded callback handled differently
                );
            }
            else
            {
                // Create 3D world HTML entity
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
                if (parentEntity != null)
                {
                    apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity);
                }

                var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                    htmlData.position.x, htmlData.position.y, htmlData.position.z);
                var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                    htmlData.rotation.x, htmlData.rotation.y, htmlData.rotation.z, htmlData.rotation.w);
                var worldScale = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                    htmlData.scale.x, htmlData.scale.y, htmlData.scale.z);

                htmlEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.HTMLEntity.Create(
                    apiParent,
                    worldPosition,
                    worldRotation,
                    worldScale,
                    htmlData.isSize,
                    entityId?.ToString(),
                    htmlData.tag,
                    htmlData.onMessage,
                    null // onLoaded callback handled differently
                );
            }

            if (htmlEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateHTMLEntity] HTML entity created successfully");

                // Load content if specified
                if (!string.IsNullOrEmpty(htmlData.url))
                {
                    htmlEntity.LoadFromURL(htmlData.url);
                }
                else if (!string.IsNullOrEmpty(htmlData.html))
                {
                    htmlEntity.LoadHTML(htmlData.html);
                }

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(htmlEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateHTMLEntity] Failed to create HTML entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        #endregion

        #region Light Entity Methods

        /// <summary>
        /// Create a light entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="lightData">JSON light entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateLightEntity(JSONLightEntity lightData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (lightData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateLightEntity] Light data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateLightEntity(lightData))
            {
                Logging.LogError("[JSONEntityHandler->CreateLightEntity] Invalid light entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateLightEntityCoroutine(lightData, parentEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create a light entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing light entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadLightEntityFromJSON(string jsonData, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadLightEntityFromJSON] Processing light entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONLightEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadLightEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateLightEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadLightEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadLightEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadLightEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadLightEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate light entity data.
        /// </summary>
        private bool ValidateLightEntity(JSONLightEntity lightData)
        {
            if (lightData.position == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateLightEntity] Position not specified, using default");
                lightData.position = new JSONVector3 { x = 0, y = 0, z = 0 };
            }

            if (lightData.rotation == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateLightEntity] Rotation not specified, using default");
                lightData.rotation = new JSONQuaternion { x = 0, y = 0, z = 0, w = 1 };
            }

            if (lightData.color == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateLightEntity] Color not specified, using default white");
                lightData.color = new JSONColor { r = 1.0f, g = 1.0f, b = 1.0f, a = 1.0f };
            }

            if (lightData.intensity <= 0)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateLightEntity] Invalid intensity, using default");
                lightData.intensity = 1.0f;
            }

            if (lightData.range <= 0)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateLightEntity] Invalid range, using default");
                lightData.range = 10.0f;
            }

            if (string.IsNullOrEmpty(lightData.lightType))
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateLightEntity] Light type not specified, using point");
                lightData.lightType = "point";
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create a light entity.
        /// </summary>
        private IEnumerator CreateLightEntityCoroutine(JSONLightEntity lightData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateLightEntityCoroutine] Starting light entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(lightData.id))
            {
                if (System.Guid.TryParse(lightData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentEntity to public API entity
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
            if (parentEntity != null)
            {
                apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity);
            }

            // Convert JSON types to WorldTypes
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                lightData.position.x, lightData.position.y, lightData.position.z);
            var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                lightData.rotation.x, lightData.rotation.y, lightData.rotation.z, lightData.rotation.w);

            // Create the light entity using LightEntity.Create
            var lightEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.LightEntity.Create(
                apiParent,
                worldPosition,
                worldRotation,
                entityId?.ToString(),
                lightData.tag,
                null // onLoaded callback handled differently
            );

            if (lightEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateLightEntity] Light entity created successfully");

                // Set light type
                FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.LightType lightType = 
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.LightType.Point;
                
                switch (lightData.lightType.ToLower())
                {
                    case "directional":
                        lightType = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.LightType.Directional;
                        break;
                    case "spot":
                        lightType = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.LightType.Spot;
                        break;
                    default:
                        lightType = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.LightType.Point;
                        break;
                }
                
                lightEntity.SetLightType(lightType);

                // Set light properties based on light type
                var lightColor = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(
                    lightData.color.r, lightData.color.g, lightData.color.b, lightData.color.a);

                if (lightType == FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.LightType.Spot)
                {
                    // Spot light with full properties
                    lightEntity.SetLightProperties(
                        lightData.range,
                        lightData.innerSpotAngle,
                        lightData.outerSpotAngle,
                        lightColor,
                        lightData.temperature,
                        lightData.intensity
                    );
                }
                else if (lightType == FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.LightType.Point)
                {
                    // Point light with range and intensity
                    lightEntity.SetLightProperties(lightData.range, lightData.intensity);
                    lightEntity.SetLightProperties(lightColor, lightData.temperature, lightData.intensity);
                }
                else
                {
                    // Directional light with basic properties
                    lightEntity.SetLightProperties(lightColor, lightData.temperature, lightData.intensity);
                }

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(lightEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateLightEntity] Failed to create light entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        #endregion

        #region Voxel Entity Methods

        /// <summary>
        /// Create a voxel entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="voxelData">JSON voxel entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateVoxelEntity(JSONVoxelEntity voxelData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (voxelData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateVoxelEntity] Voxel data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateVoxelEntity(voxelData))
            {
                Logging.LogError("[JSONEntityHandler->CreateVoxelEntity] Invalid voxel entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateVoxelEntityCoroutine(voxelData, parentEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create a voxel entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing voxel entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadVoxelEntityFromJSON(string jsonData, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadVoxelEntityFromJSON] Processing voxel entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONVoxelEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadVoxelEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateVoxelEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadVoxelEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadVoxelEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadVoxelEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadVoxelEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate voxel entity data.
        /// </summary>
        private bool ValidateVoxelEntity(JSONVoxelEntity voxelData)
        {
            if (voxelData.position == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateVoxelEntity] Position not specified, using default");
                voxelData.position = new JSONVector3 { x = 0, y = 0, z = 0 };
            }

            if (voxelData.rotation == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateVoxelEntity] Rotation not specified, using default");
                voxelData.rotation = new JSONQuaternion { x = 0, y = 0, z = 0, w = 1 };
            }

            if (voxelData.scale == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateVoxelEntity] Scale not specified, using default");
                voxelData.scale = new JSONVector3 { x = 1, y = 1, z = 1 };
            }

            if (voxelData.blockInfos == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateVoxelEntity] No block infos specified, using empty array");
                voxelData.blockInfos = new JSONVoxelBlockInfo[0];
            }

            if (voxelData.blocks == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateVoxelEntity] No blocks specified, using empty array");
                voxelData.blocks = new JSONVoxelBlock[0];
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create a voxel entity.
        /// </summary>
        private IEnumerator CreateVoxelEntityCoroutine(JSONVoxelEntity voxelData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateVoxelEntityCoroutine] Starting voxel entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(voxelData.id))
            {
                if (System.Guid.TryParse(voxelData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentEntity to public API entity if provided
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
            if (parentEntity != null)
            {
                apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity);
            }

            // Convert JSON vectors to WorldTypes
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                voxelData.position.x, voxelData.position.y, voxelData.position.z);
            var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                voxelData.rotation.x, voxelData.rotation.y, voxelData.rotation.z, voxelData.rotation.w);
            var worldScale = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                voxelData.scale.x, voxelData.scale.y, voxelData.scale.z);

            // Create the voxel entity using VoxelEntity.Create
            var voxelEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.VoxelEntity.Create(
                apiParent,
                worldPosition,
                worldRotation,
                worldScale,
                entityId?.ToString(),
                voxelData.tag,
                null // onLoaded callback handled differently
            );

            if (voxelEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateVoxelEntity] Voxel entity created successfully");

                // Set up block infos first
                foreach (var blockInfo in voxelData.blockInfos)
                {
                    var voxelBlockInfo = new FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.VoxelBlockInfo(blockInfo.id);
                    
                    foreach (var subType in blockInfo.subTypes)
                    {
                        voxelBlockInfo.AddSubType(
                            subType.id,
                            subType.invisible,
                            subType.topTexture,
                            subType.bottomTexture,
                            subType.leftTexture,
                            subType.rightTexture,
                            subType.frontTexture,
                            subType.backTexture
                        );
                    }
                    
                    voxelEntity.SetBlockInfo(blockInfo.id, voxelBlockInfo);
                }

                // Place individual blocks
                foreach (var block in voxelData.blocks)
                {
                    voxelEntity.SetBlock(block.x, block.y, block.z, block.type, block.subType);
                }

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(voxelEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateVoxelEntity] Failed to create voxel entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        #endregion

        #region Water Entity Methods

        /// <summary>
        /// Create a water entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="waterData">JSON water entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateWaterEntity(JSONWaterEntity waterData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (waterData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateWaterEntity] Water data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateWaterEntity(waterData))
            {
                Logging.LogError("[JSONEntityHandler->CreateWaterEntity] Invalid water entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateWaterEntityCoroutine(waterData, parentEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create a water entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing water entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadWaterEntityFromJSON(string jsonData, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadWaterEntityFromJSON] Processing water entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONWaterEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadWaterEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateWaterEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadWaterEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadWaterEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadWaterEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadWaterEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate water entity data.
        /// </summary>
        private bool ValidateWaterEntity(JSONWaterEntity waterData)
        {
            if (waterData.scale == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateWaterEntity] Scale not specified, using default");
                waterData.scale = new JSONVector3 { x = 10, y = 1, z = 10 };
            }

            if (waterData.position == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateWaterEntity] Position not specified, using default");
                waterData.position = new JSONVector3 { x = 0, y = 0, z = 0 };
            }

            if (waterData.rotation == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateWaterEntity] Rotation not specified, using default");
                waterData.rotation = new JSONQuaternion { x = 0, y = 0, z = 0, w = 1 };
            }

            if (waterData.deepColor == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateWaterEntity] Deep color not specified, using default");
                waterData.deepColor = new JSONColor { r = 0.1f, g = 0.3f, b = 0.7f, a = 1.0f };
            }

            if (waterData.shallowColor == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateWaterEntity] Shallow color not specified, using default");
                waterData.shallowColor = new JSONColor { r = 0.4f, g = 0.8f, b = 1.0f, a = 0.8f };
            }

            // Validate numeric ranges using actual property names
            if (waterData.distortion < 0 || waterData.distortion > 128) waterData.distortion = 32.0f;
            if (waterData.smoothness < 0 || waterData.smoothness > 1) waterData.smoothness = 0.8f;
            if (waterData.intensity < 0 || waterData.intensity > 1) waterData.intensity = 0.7f;
            if (waterData.waveSpeed < 0) waterData.waveSpeed = 1.0f;
            if (waterData.waveAmplitude < 0 || waterData.waveAmplitude > 1) waterData.waveAmplitude = 0.3f;
            if (waterData.waveSteepness < 0 || waterData.waveSteepness > 1) waterData.waveSteepness = 0.5f;
            if (waterData.numWaves < 1 || waterData.numWaves > 32) waterData.numWaves = 4.0f;
            if (waterData.waveLength <= 0) waterData.waveLength = 10.0f;
            if (waterData.waveScale <= 0) waterData.waveScale = 1.0f;

            return true;
        }

        /// <summary>
        /// Coroutine to create a water entity.
        /// </summary>
        private IEnumerator CreateWaterEntityCoroutine(JSONWaterEntity waterData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateWaterEntityCoroutine] Starting water entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(waterData.id))
            {
                if (System.Guid.TryParse(waterData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentEntity to public API entity if provided
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
            if (parentEntity != null)
            {
                apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity);
            }

            // Convert JSON data to WorldTypes
            var worldScale = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                waterData.scale.x, waterData.scale.y, waterData.scale.z);
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                waterData.position.x, waterData.position.y, waterData.position.z);
            var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                waterData.rotation.x, waterData.rotation.y, waterData.rotation.z, waterData.rotation.w);
            
            var shallowColor = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(
                waterData.shallowColor.r, waterData.shallowColor.g, waterData.shallowColor.b, waterData.shallowColor.a);
            var deepColor = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(
                waterData.deepColor.r, waterData.deepColor.g, waterData.deepColor.b, waterData.deepColor.a);
            var specularColor = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(
                waterData.specularColor.r, waterData.specularColor.g, waterData.specularColor.b, waterData.specularColor.a);
            var scatteringColor = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Color(
                waterData.scatteringColor.r, waterData.scatteringColor.g, waterData.scatteringColor.b, waterData.scatteringColor.a);

            // Create the water entity using WaterEntity.CreateWaterBody with correct parameter order
            var waterEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.WaterEntity.CreateWaterBody(
                apiParent,
                shallowColor,
                deepColor,
                specularColor,
                scatteringColor,
                waterData.deepStart,
                waterData.deepEnd,
                waterData.distortion,
                waterData.smoothness,
                waterData.numWaves,
                waterData.waveAmplitude,
                waterData.waveSteepness,
                waterData.waveSpeed,
                waterData.waveLength,
                waterData.waveScale,
                waterData.intensity,
                worldPosition,
                worldRotation,
                worldScale,
                entityId?.ToString(),
                waterData.tag,
                null // onLoaded callback handled differently
            );

            if (waterEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateWaterEntity] Water entity created successfully");

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(waterEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateWaterEntity] Failed to create water entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        #endregion

        #region Water Blocker Entity Methods

        /// <summary>
        /// Create a water blocker entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="blockerData">JSON water blocker entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateWaterBlockerEntity(JSONWaterBlockerEntity blockerData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (blockerData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateWaterBlockerEntity] Water blocker data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateWaterBlockerEntity(blockerData))
            {
                Logging.LogError("[JSONEntityHandler->CreateWaterBlockerEntity] Invalid water blocker entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateWaterBlockerEntityCoroutine(blockerData, parentEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create a water blocker entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing water blocker entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadWaterBlockerEntityFromJSON(string jsonData, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadWaterBlockerEntityFromJSON] Processing water blocker entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONWaterBlockerEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadWaterBlockerEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateWaterBlockerEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadWaterBlockerEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadWaterBlockerEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadWaterBlockerEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadWaterBlockerEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate water blocker entity data.
        /// </summary>
        private bool ValidateWaterBlockerEntity(JSONWaterBlockerEntity blockerData)
        {
            if (blockerData.position == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateWaterBlockerEntity] Position not specified, using default");
                blockerData.position = new JSONVector3 { x = 0, y = 0, z = 0 };
            }

            if (blockerData.rotation == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateWaterBlockerEntity] Rotation not specified, using default");
                blockerData.rotation = new JSONQuaternion { x = 0, y = 0, z = 0, w = 1 };
            }

            if (blockerData.scale == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateWaterBlockerEntity] Scale not specified, using default");
                blockerData.scale = new JSONVector3 { x = 1, y = 1, z = 1 };
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create a water blocker entity.
        /// </summary>
        private IEnumerator CreateWaterBlockerEntityCoroutine(JSONWaterBlockerEntity blockerData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateWaterBlockerEntityCoroutine] Starting water blocker entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(blockerData.id))
            {
                if (System.Guid.TryParse(blockerData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentEntity to public API entity if provided
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
            if (parentEntity != null)
            {
                apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity);
            }

            // Convert JSON vectors to WorldTypes
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                blockerData.position.x, blockerData.position.y, blockerData.position.z);
            var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                blockerData.rotation.x, blockerData.rotation.y, blockerData.rotation.z, blockerData.rotation.w);
            var worldScale = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                blockerData.scale.x, blockerData.scale.y, blockerData.scale.z);

            // Create the water blocker entity using WaterBlockerEntity.CreateWaterBlocker
            var blockerEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.WaterBlockerEntity.CreateWaterBlocker(
                apiParent,
                worldPosition,
                worldRotation,
                worldScale,
                entityId?.ToString(),
                blockerData.tag,
                null // onLoaded callback handled differently
            );

            if (blockerEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateWaterBlockerEntity] Water blocker entity created successfully");

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(blockerEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateWaterBlockerEntity] Failed to create water blocker entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        #endregion

        #region Audio Entity Methods

        /// <summary>
        /// Create an audio entity from JSON data and invoke callback with result.
        /// </summary>
        /// <param name="audioData">JSON audio entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when creation is complete (entityId, createdEntity)</param>
        /// <returns>Entity ID if creation was initiated, null otherwise</returns>
        public Guid? CreateAudioEntity(JSONAudioEntity audioData, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            if (audioData == null)
            {
                Logging.LogError("[JSONEntityHandler->CreateAudioEntity] Audio data is null");
                onComplete?.Invoke(null, null);
                return null;
            }

            if (!ValidateAudioEntity(audioData))
            {
                Logging.LogError("[JSONEntityHandler->CreateAudioEntity] Invalid audio entity data");
                onComplete?.Invoke(null, null);
                return null;
            }

            StartCoroutine(CreateAudioEntityCoroutine(audioData, parentEntity, onComplete));
            return Guid.NewGuid(); // Placeholder ID until actual creation
        }

        /// <summary>
        /// Load and create an audio entity from JSON string.
        /// </summary>
        /// <param name="jsonData">JSON string containing audio entity data</param>
        /// <param name="parentEntity">Parent entity to attach to</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity)</param>
        public void LoadAudioEntityFromJSON(string jsonData, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadAudioEntityFromJSON] Processing audio entity JSON data");

                var entityData = JsonConvert.DeserializeObject<JSONAudioEntity>(jsonData);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadAudioEntityFromJSON] Failed to parse JSON data");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                Guid? entityId = CreateAudioEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadAudioEntityFromJSON] Successfully loaded entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadAudioEntityFromJSON] Failed to create entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadAudioEntityFromJSON] Failed to initiate entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadAudioEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        /// <summary>
        /// Validate audio entity data.
        /// </summary>
        private bool ValidateAudioEntity(JSONAudioEntity audioData)
        {
            if (audioData.position == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateAudioEntity] Position not specified, using default");
                audioData.position = new JSONVector3 { x = 0, y = 0, z = 0 };
            }

            if (audioData.rotation == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateAudioEntity] Rotation not specified, using default");
                audioData.rotation = new JSONQuaternion { x = 0, y = 0, z = 0, w = 1 };
            }

            if (string.IsNullOrEmpty(audioData.audioFile))
            {
                Logging.LogError("[JSONEntityHandler->ValidateAudioEntity] Audio file path is required");
                return false;
            }

            // Validate audio properties ranges
            if (audioData.priority < 0 || audioData.priority > 256)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateAudioEntity] Priority out of range (0-256), clamping");
                audioData.priority = Math.Max(0, Math.Min(256, audioData.priority));
            }

            if (audioData.volume < 0 || audioData.volume > 1)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateAudioEntity] Volume out of range (0-1), clamping");
                audioData.volume = Math.Max(0, Math.Min(1, audioData.volume));
            }

            if (audioData.pitch < -3 || audioData.pitch > 3)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateAudioEntity] Pitch out of range (-3 to 3), clamping");
                audioData.pitch = Math.Max(-3, Math.Min(3, audioData.pitch));
            }

            if (audioData.stereoPan < -1 || audioData.stereoPan > 1)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateAudioEntity] Stereo pan out of range (-1 to 1), clamping");
                audioData.stereoPan = Math.Max(-1, Math.Min(1, audioData.stereoPan));
            }

            return true;
        }

        /// <summary>
        /// Coroutine to create an audio entity.
        /// </summary>
        private IEnumerator CreateAudioEntityCoroutine(JSONAudioEntity audioData, BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            Logging.Log("[JSONEntityHandler->CreateAudioEntityCoroutine] Starting audio entity creation");

            System.Guid? entityId = null;
            if (!string.IsNullOrEmpty(audioData.id))
            {
                if (System.Guid.TryParse(audioData.id, out System.Guid parsedId))
                {
                    entityId = parsedId;
                }
            }

            // Convert parentEntity to public API entity if provided
            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.BaseEntity apiParent = null;
            if (parentEntity != null)
            {
                apiParent = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity);
            }

            // Convert JSON vectors to WorldTypes
            var worldPosition = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(
                audioData.position.x, audioData.position.y, audioData.position.z);
            var worldRotation = new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(
                audioData.rotation.x, audioData.rotation.y, audioData.rotation.z, audioData.rotation.w);

            // Create the audio entity using AudioEntity.Create
            var audioEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.AudioEntity.Create(
                apiParent,
                worldPosition,
                worldRotation,
                entityId?.ToString(),
                audioData.tag,
                null // onLoaded callback handled differently
            );

            if (audioEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateAudioEntity] Audio entity created successfully");

                // Set audio properties
                audioEntity.loop = audioData.loop;
                audioEntity.priority = audioData.priority;
                audioEntity.volume = audioData.volume;
                audioEntity.pitch = audioData.pitch;
                audioEntity.stereoPan = audioData.stereoPan;

                // Load audio file if specified
                if (!string.IsNullOrEmpty(audioData.audioFile))
                {
                    bool loadSuccess = audioEntity.LoadAudioClipFromWAV(audioData.audioFile);
                    if (!loadSuccess)
                    {
                        Logging.LogWarning($"[JSONEntityHandler->CreateAudioEntity] Failed to load audio file: {audioData.audioFile}");
                    }
                    else if (audioData.playOnLoad)
                    {
                        // Start playing if requested
                        audioEntity.Play();
                    }
                }

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(audioEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateAudioEntity] Failed to create audio entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        #endregion

        #region Character Entity Methods

        /// <summary>
        /// Parse JSON string into a JSONCharacterEntity.
        /// </summary>
        /// <param name="jsonString">JSON string to parse.</param>
        /// <returns>Parsed JSONCharacterEntity or null if parsing fails.</returns>
        public JSONCharacterEntity ParseCharacterEntityFromJSON(string jsonString)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonString))
                {
                    Logging.LogError("[JSONEntityHandler->ParseCharacterEntityFromJSON] JSON string is null or empty.");
                    return null;
                }

                JSONCharacterEntity entity = JsonConvert.DeserializeObject<JSONCharacterEntity>(jsonString);
                
                if (!ValidateCharacterEntity(entity))
                {
                    Logging.LogError("[JSONEntityHandler->ParseCharacterEntityFromJSON] Character entity validation failed.");
                    return null;
                }

                return entity;
            }
            catch (JsonException ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseCharacterEntityFromJSON] JSON parsing error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->ParseCharacterEntityFromJSON] Unexpected error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Validate a JSONCharacterEntity for required fields and proper structure.
        /// </summary>
        /// <param name="entity">Character entity to validate.</param>
        /// <returns>True if entity is valid, false otherwise.</returns>
        private bool ValidateCharacterEntity(JSONCharacterEntity entity)
        {
            if (entity == null)
            {
                Logging.LogError("[JSONEntityHandler->ValidateCharacterEntity] Entity is null.");
                return false;
            }

            // ID validation
            if (string.IsNullOrEmpty(entity.id))
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateCharacterEntity] Entity ID is null or empty. A new ID will be generated.");
            }
            else if (!Guid.TryParse(entity.id, out _))
            {
                Logging.LogError("[JSONEntityHandler->ValidateCharacterEntity] Entity ID is not a valid GUID format.");
                return false;
            }

            // Position validation
            if (entity.position == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateCharacterEntity] Position is null. Using Vector3.zero.");
                entity.position = new JSONVector3();
            }

            // Rotation validation
            if (entity.rotation == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateCharacterEntity] Rotation is null. Using Quaternion.identity.");
                entity.rotation = new JSONQuaternion() { w = 1 };
            }

            // Scale validation
            if (entity.scale == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateCharacterEntity] Scale is null. Using Vector3.one.");
                entity.scale = new JSONVector3() { x = 1, y = 1, z = 1 };
            }

            // Mesh offset validation
            if (entity.meshOffset == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateCharacterEntity] Mesh offset is null. Using Vector3.zero.");
                entity.meshOffset = new JSONVector3();
            }

            // Mesh rotation validation
            if (entity.meshRotation == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateCharacterEntity] Mesh rotation is null. Using Quaternion.identity.");
                entity.meshRotation = new JSONQuaternion() { w = 1 };
            }

            // Avatar label offset validation
            if (entity.avatarLabelOffset == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateCharacterEntity] Avatar label offset is null. Using default offset.");
                entity.avatarLabelOffset = new JSONVector3() { x = 0, y = 2, z = 0 };
            }

            // Mesh resources validation
            if (entity.meshResources == null)
            {
                Logging.LogWarning("[JSONEntityHandler->ValidateCharacterEntity] Mesh resources is null. Using empty array.");
                entity.meshResources = new string[0];
            }

            return true;
        }

        /// <summary>
        /// Create a character entity from JSON data using the EntityManager.
        /// </summary>
        /// <param name="entity">JSONCharacterEntity to create.</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root.</param>
        /// <param name="onComplete">Callback when creation is complete.</param>
        /// <returns>The GUID of the created entity, or null if creation failed.</returns>
        public Guid? CreateCharacterEntity(JSONCharacterEntity entity, BaseEntity parentEntity = null, Action<Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                if (entity == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateCharacterEntity] Entity is null.");
                    onComplete?.Invoke(null, null);
                    return null;
                }

                if (StraightFour.StraightFour.ActiveWorld?.entityManager == null)
                {
                    Logging.LogError("[JSONEntityHandler->CreateCharacterEntity] EntityManager not available.");
                    onComplete?.Invoke(null, null);
                    return null;
                }

                // Generate GUID if not provided
                Guid entityGuid;
                if (string.IsNullOrEmpty(entity.id))
                {
                    entityGuid = Guid.NewGuid();
                }
                else
                {
                    if (!Guid.TryParse(entity.id, out entityGuid))
                    {
                        Logging.LogError($"[JSONEntityHandler->CreateCharacterEntity] Invalid GUID format: {entity.id}");
                        onComplete?.Invoke(null, null);
                        return null;
                    }
                }

                // Start the creation process using a coroutine
                StartCoroutine(CreateCharacterEntityCoroutine(entity, entityGuid, parentEntity, onComplete));

                return entityGuid;
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->CreateCharacterEntity] Unexpected error: {ex.Message}");
                onComplete?.Invoke(null, null);
                return null;
            }
        }

        /// <summary>
        /// Coroutine to create a character entity asynchronously.
        /// </summary>
        private IEnumerator CreateCharacterEntityCoroutine(JSONCharacterEntity characterData, Guid entityGuid, 
            BaseEntity parentEntity, Action<Guid?, BaseEntity> onComplete)
        {
            // Extract transform data
            Vector3 position = characterData.position?.ToVector3() ?? Vector3.zero;
            Quaternion rotation = characterData.rotation?.ToQuaternion() ?? Quaternion.identity;
            Vector3 scale = characterData.scale?.ToVector3() ?? Vector3.one;
            Vector3 meshOffset = characterData.meshOffset?.ToVector3() ?? Vector3.zero;
            Quaternion meshRotation = characterData.meshRotation?.ToQuaternion() ?? Quaternion.identity;
            Vector3 avatarLabelOffset = characterData.avatarLabelOffset?.ToVector3() ?? new Vector3(0, 2, 0);

            FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CharacterEntity characterEntity = null;

            if (string.IsNullOrEmpty(characterData.meshObject))
            {
                // Create default character entity without mesh
                characterEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CharacterEntity.Create(
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity),
                    new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(position.x, position.y, position.z),
                    new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w),
                    new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(scale.x, scale.y, scale.z),
                    characterData.isSize,
                    characterData.tag,
                    characterData.id,
                    null // onLoaded callback handled differently
                );
            }
            else
            {
                // Create character entity with mesh
                characterEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.CharacterEntity.Create(
                    FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPublicEntity(parentEntity),
                    characterData.meshObject,
                    characterData.meshResources,
                    new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(meshOffset.x, meshOffset.y, meshOffset.z),
                    new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(meshRotation.x, meshRotation.y, meshRotation.z, meshRotation.w),
                    new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(avatarLabelOffset.x, avatarLabelOffset.y, avatarLabelOffset.z),
                    new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(position.x, position.y, position.z),
                    new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w),
                    new FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes.Vector3(scale.x, scale.y, scale.z),
                    characterData.isSize,
                    characterData.tag,
                    characterData.id,
                    null // onLoaded callback handled differently
                );
            }

            if (characterEntity != null)
            {
                Logging.Log($"[JSONEntityHandler->CreateCharacterEntity] Character entity created successfully");

                // Set character properties
                characterEntity.fixHeight = characterData.fixHeight;

                // Get the internal entity for the callback
                var internalEntity = FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity.EntityAPIHelper.GetPrivateEntity(characterEntity);

                onComplete?.Invoke(internalEntity.id, internalEntity);
            }
            else
            {
                Logging.LogError("[JSONEntityHandler->CreateCharacterEntity] Failed to create character entity");
                onComplete?.Invoke(null, null);
            }

            yield return null;
        }

        /// <summary>
        /// Load and create a character entity from a JSON string.
        /// </summary>
        /// <param name="jsonString">JSON string containing character entity data.</param>
        /// <param name="parentEntity">Parent entity to attach to, or null for world root.</param>
        /// <param name="onComplete">Callback when loading is complete (success, entityId, createdEntity).</param>
        public void LoadCharacterEntityFromJSON(string jsonString, BaseEntity parentEntity = null, Action<bool, Guid?, BaseEntity> onComplete = null)
        {
            try
            {
                Logging.Log("[JSONEntityHandler->LoadCharacterEntityFromJSON] Starting JSON character entity loading process.");

                // Parse JSON
                JSONCharacterEntity entityData = ParseCharacterEntityFromJSON(jsonString);
                if (entityData == null)
                {
                    Logging.LogError("[JSONEntityHandler->LoadCharacterEntityFromJSON] Failed to parse JSON data.");
                    onComplete?.Invoke(false, null, null);
                    return;
                }

                // Create entity
                Guid? entityId = CreateCharacterEntity(entityData, parentEntity, (createdId, createdEntity) =>
                {
                    bool success = createdId.HasValue && createdEntity != null;
                    if (success)
                    {
                        Logging.Log($"[JSONEntityHandler->LoadCharacterEntityFromJSON] Successfully loaded character entity: {createdId}");
                    }
                    else
                    {
                        Logging.LogError("[JSONEntityHandler->LoadCharacterEntityFromJSON] Failed to create character entity from JSON data.");
                    }
                    
                    onComplete?.Invoke(success, createdId, createdEntity);
                });

                if (!entityId.HasValue)
                {
                    Logging.LogError("[JSONEntityHandler->LoadCharacterEntityFromJSON] Failed to initiate character entity creation.");
                    onComplete?.Invoke(false, null, null);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[JSONEntityHandler->LoadCharacterEntityFromJSON] Unexpected error: {ex.Message}");
                onComplete?.Invoke(false, null, null);
            }
        }

        #endregion
    }
}