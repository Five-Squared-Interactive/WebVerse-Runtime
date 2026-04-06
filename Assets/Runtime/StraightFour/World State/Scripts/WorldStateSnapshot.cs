// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.StraightFour.WorldState
{
    /// <summary>
    /// Complete snapshot of a world's state for serialization.
    /// Used for tab switching and world state persistence.
    /// </summary>
    [Serializable]
    public class WorldStateSnapshot
    {
        /// <summary>
        /// Snapshot format version.
        /// </summary>
        public string version = "1.0";

        /// <summary>
        /// Timestamp when snapshot was created (milliseconds since epoch).
        /// </summary>
        public long timestamp;

        /// <summary>
        /// Name/URL of the world.
        /// </summary>
        public string worldName;

        /// <summary>
        /// Base path for the world resources.
        /// </summary>
        public string basePath;

        /// <summary>
        /// World position offset for floating-point precision.
        /// </summary>
        public SerializableVector3 worldOffset;

        /// <summary>
        /// All entity snapshots.
        /// </summary>
        public List<EntitySnapshot> entities = new List<EntitySnapshot>();

        /// <summary>
        /// Camera state.
        /// </summary>
        public CameraSnapshot camera;

        /// <summary>
        /// World storage items (key-value pairs).
        /// </summary>
        public Dictionary<string, string> storage = new Dictionary<string, string>();

        /// <summary>
        /// Environment state (sky, lighting).
        /// </summary>
        public EnvironmentSnapshot environment;

        /// <summary>
        /// Create a timestamp for the current moment.
        /// </summary>
        public void SetTimestamp()
        {
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    /// <summary>
    /// Snapshot of an individual entity.
    /// </summary>
    [Serializable]
    public class EntitySnapshot
    {
        /// <summary>
        /// Entity unique identifier.
        /// </summary>
        public string id;

        /// <summary>
        /// Entity type name (MeshEntity, CharacterEntity, etc.).
        /// </summary>
        public string entityType;

        /// <summary>
        /// Entity tag/name.
        /// </summary>
        public string tag;

        /// <summary>
        /// Parent entity ID (null if root entity).
        /// </summary>
        public string parentId;

        /// <summary>
        /// Entity position in world space.
        /// </summary>
        public SerializableVector3 position;

        /// <summary>
        /// Entity rotation.
        /// </summary>
        public SerializableQuaternion rotation;

        /// <summary>
        /// Entity scale.
        /// </summary>
        public SerializableVector3 scale;

        /// <summary>
        /// Interaction state (Hidden, Static, Physical, Placing).
        /// </summary>
        public string interactionState;

        /// <summary>
        /// Whether entity is visible/active.
        /// </summary>
        public bool visible = true;

        /// <summary>
        /// Physics properties (if applicable).
        /// </summary>
        public PhysicsSnapshot physics;

        /// <summary>
        /// Type-specific data stored as JSON string for flexibility.
        /// </summary>
        public string typeSpecificData;
    }

    /// <summary>
    /// Physics state for an entity.
    /// </summary>
    [Serializable]
    public class PhysicsSnapshot
    {
        public float mass = 1f;
        public float drag = 0f;
        public float angularDrag = 0.05f;
        public bool useGravity = true;
        public bool isKinematic = false;
        public SerializableVector3 velocity;
        public SerializableVector3 angularVelocity;
        public SerializableVector3 centerOfMass;
    }

    /// <summary>
    /// Camera state snapshot.
    /// </summary>
    [Serializable]
    public class CameraSnapshot
    {
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
        public float fieldOfView = 60f;
        public float nearClipPlane = 0.1f;
        public float farClipPlane = 1000f;
    }

    /// <summary>
    /// Environment state snapshot.
    /// </summary>
    [Serializable]
    public class EnvironmentSnapshot
    {
        public SerializableColor ambientColor;
        public SerializableColor fogColor;
        public float fogDensity;
        public bool fogEnabled;
        public string skyboxType;
        public string skyboxTexture;
    }

    #region Type-Specific Snapshots

    /// <summary>
    /// Mesh entity specific data.
    /// </summary>
    [Serializable]
    public class MeshEntityData
    {
        public string meshType; // "primitive" or "gltf"
        public string meshSource; // path or primitive name
        public string[] meshResources; // textures, etc.
        public SerializableColor color;
    }

    /// <summary>
    /// Light entity specific data.
    /// </summary>
    [Serializable]
    public class LightEntityData
    {
        public string lightType; // Directional, Spot, Point
        public SerializableColor color;
        public float intensity = 1f;
        public float range = 10f;
        public float spotAngleInner = 30f;
        public float spotAngleOuter = 45f;
        public float temperature = 6500f;
    }

    /// <summary>
    /// Character entity specific data.
    /// </summary>
    [Serializable]
    public class CharacterEntityData
    {
        public float radius = 0.5f;
        public float height = 2f;
        public SerializableVector3 center;
        public SerializableVector3 characterObjectOffset;
        public SerializableQuaternion characterObjectRotation;
        public SerializableVector3 characterLabelOffset;
    }

    /// <summary>
    /// Terrain entity specific data.
    /// </summary>
    [Serializable]
    public class TerrainEntityData
    {
        public string terrainType; // "hybrid" or "standard"
        public float length;
        public float width;
        public float height;
        public float[] heights; // Flattened 2D array
        public int heightsWidth;
        public int heightsHeight;
        public TerrainLayerData[] layers;
    }

    /// <summary>
    /// Terrain layer data.
    /// </summary>
    [Serializable]
    public class TerrainLayerData
    {
        public string diffuseTexture;
        public string normalTexture;
        public string maskTexture;
        public SerializableVector2 tileSize;
        public SerializableVector2 tileOffset;
        public float metallic;
        public float smoothness;
    }

    /// <summary>
    /// Water entity specific data.
    /// </summary>
    [Serializable]
    public class WaterEntityData
    {
        public SerializableColor shallowColor;
        public SerializableColor deepColor;
        public SerializableVector3 size;
    }

    /// <summary>
    /// HTML/UI entity specific data.
    /// </summary>
    [Serializable]
    public class HTMLEntityData
    {
        public string url;
        public SerializableVector2 size;
    }

    #endregion

    #region Serializable Unity Types

    /// <summary>
    /// Serializable Vector3 for JSON compatibility.
    /// </summary>
    [Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3() { }

        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static SerializableVector3 FromVector3(Vector3 v)
        {
            return new SerializableVector3(v);
        }
    }

    /// <summary>
    /// Serializable Vector2 for JSON compatibility.
    /// </summary>
    [Serializable]
    public class SerializableVector2
    {
        public float x;
        public float y;

        public SerializableVector2() { }

        public SerializableVector2(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

    /// <summary>
    /// Serializable Quaternion for JSON compatibility.
    /// </summary>
    [Serializable]
    public class SerializableQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SerializableQuaternion() { w = 1f; }

        public SerializableQuaternion(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

        public static SerializableQuaternion FromQuaternion(Quaternion q)
        {
            return new SerializableQuaternion(q);
        }
    }

    /// <summary>
    /// Serializable Color for JSON compatibility.
    /// </summary>
    [Serializable]
    public class SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a = 1f;

        public SerializableColor() { }

        public SerializableColor(Color c)
        {
            r = c.r;
            g = c.g;
            b = c.b;
            a = c.a;
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }
    }

    #endregion
}
