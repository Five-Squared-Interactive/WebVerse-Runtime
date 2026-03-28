// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.WorldSync
{
    /// <summary>
    /// 3D vector for WorldSync (matches server-side Vector3).
    /// </summary>
    [Serializable]
    public struct SyncVector3
    {
        public float x;
        public float y;
        public float z;

        public SyncVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static SyncVector3 Zero => new SyncVector3(0, 0, 0);
        public static SyncVector3 One => new SyncVector3(1, 1, 1);

        public static implicit operator Vector3(SyncVector3 v) => new Vector3(v.x, v.y, v.z);
        public static implicit operator SyncVector3(Vector3 v) => new SyncVector3(v.x, v.y, v.z);
    }

    /// <summary>
    /// Quaternion for WorldSync (matches server-side Quaternion).
    /// </summary>
    [Serializable]
    public struct SyncQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SyncQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static SyncQuaternion Identity => new SyncQuaternion(0, 0, 0, 1);

        public static implicit operator Quaternion(SyncQuaternion q) => new Quaternion(q.x, q.y, q.z, q.w);
        public static implicit operator SyncQuaternion(Quaternion q) => new SyncQuaternion(q.x, q.y, q.z, q.w);
    }

    /// <summary>
    /// RGBA color for WorldSync.
    /// </summary>
    [Serializable]
    public struct SyncColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SyncColor(float r, float g, float b, float a = 1.0f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static implicit operator Color(SyncColor c) => new Color(c.r, c.g, c.b, c.a);
        public static implicit operator SyncColor(Color c) => new SyncColor(c.r, c.g, c.b, c.a);
    }

    /// <summary>
    /// Entity interaction state.
    /// </summary>
    public enum InteractionState
    {
        Static,
        Grabbable,
        Grabbed,
        Usable,
        InUse
    }

    /// <summary>
    /// Motion state for an entity.
    /// </summary>
    [Serializable]
    public class MotionState
    {
        public SyncVector3 Velocity { get; set; } = SyncVector3.Zero;
        public SyncVector3 AngularVelocity { get; set; } = SyncVector3.Zero;
    }

    /// <summary>
    /// Physical properties for an entity.
    /// </summary>
    [Serializable]
    public class PhysicalProperties
    {
        public float Mass { get; set; } = 1.0f;
        public float Drag { get; set; } = 0.0f;
        public float AngularDrag { get; set; } = 0.05f;
        public bool UseGravity { get; set; } = false;
        public bool IsKinematic { get; set; } = true;
        public SyncVector3 CenterOfMass { get; set; } = SyncVector3.Zero;
    }

    /// <summary>
    /// Synchronized entity data.
    /// </summary>
    [Serializable]
    public class SyncEntity
    {
        /// <summary>
        /// Unique entity identifier.
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Client ID of the entity owner.
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Entity type (mesh, avatar, light, etc.).
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Human-readable entity tag.
        /// </summary>
        public string EntityTag { get; set; }

        /// <summary>
        /// Entity position.
        /// </summary>
        public SyncVector3 Position { get; set; } = SyncVector3.Zero;

        /// <summary>
        /// Entity rotation.
        /// </summary>
        public SyncQuaternion Rotation { get; set; } = SyncQuaternion.Identity;

        /// <summary>
        /// Entity scale.
        /// </summary>
        public SyncVector3 Scale { get; set; } = SyncVector3.One;

        /// <summary>
        /// Parent entity ID (null for root-level entities).
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// Entity visibility.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Entity highlight state.
        /// </summary>
        public bool Highlight { get; set; } = false;

        /// <summary>
        /// Entity interaction state.
        /// </summary>
        public InteractionState InteractionState { get; set; } = InteractionState.Static;

        /// <summary>
        /// Entity motion state.
        /// </summary>
        public MotionState Motion { get; set; } = new MotionState();

        /// <summary>
        /// Entity physical properties.
        /// </summary>
        public PhysicalProperties PhysicalProperties { get; set; } = new PhysicalProperties();

        /// <summary>
        /// Type-specific properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Check if this entity is owned by the specified client.
        /// </summary>
        public bool IsOwnedBy(string clientId)
        {
            return OwnerId == clientId;
        }
    }

    /// <summary>
    /// Client information.
    /// </summary>
    [Serializable]
    public class SyncClient
    {
        /// <summary>
        /// Unique client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Human-readable client tag.
        /// </summary>
        public string ClientTag { get; set; }

        /// <summary>
        /// Timestamp when client joined.
        /// </summary>
        public string JoinedAt { get; set; }

        /// <summary>
        /// Last heartbeat timestamp.
        /// </summary>
        public string LastHeartbeat { get; set; }
    }

    /// <summary>
    /// Sky configuration.
    /// </summary>
    [Serializable]
    public class SkyState
    {
        public string SkyType { get; set; } = "procedural";
        public SyncColor SolidColor { get; set; }
        public string TextureUri { get; set; }
        public float SunAzimuth { get; set; }
        public float SunElevation { get; set; }
    }

    /// <summary>
    /// Fog configuration.
    /// </summary>
    [Serializable]
    public class FogState
    {
        public bool Enabled { get; set; } = false;
        public SyncColor Color { get; set; } = new SyncColor(0.5f, 0.5f, 0.5f);
        public float Density { get; set; } = 0.01f;
        public float StartDistance { get; set; } = 10.0f;
        public float EndDistance { get; set; } = 100.0f;
    }

    /// <summary>
    /// Ambient lighting configuration.
    /// </summary>
    [Serializable]
    public class AmbientState
    {
        public SyncColor Color { get; set; } = new SyncColor(0.2f, 0.2f, 0.2f);
        public float Intensity { get; set; } = 1.0f;
    }

    /// <summary>
    /// Environment state.
    /// </summary>
    [Serializable]
    public class EnvironmentState
    {
        public SkyState Sky { get; set; }
        public FogState Fog { get; set; }
        public AmbientState Ambient { get; set; }
        public float TimeOfDay { get; set; } = 0.5f;
    }

    /// <summary>
    /// Session state.
    /// </summary>
    [Serializable]
    public class SessionState
    {
        /// <summary>
        /// Session identifier.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Session tag/name.
        /// </summary>
        public string SessionTag { get; set; }

        /// <summary>
        /// Timestamp when session was created.
        /// </summary>
        public string CreatedAt { get; set; }

        /// <summary>
        /// List of connected clients.
        /// </summary>
        public List<SyncClient> Clients { get; set; } = new List<SyncClient>();

        /// <summary>
        /// List of entities in session.
        /// </summary>
        public List<SyncEntity> Entities { get; set; } = new List<SyncEntity>();

        /// <summary>
        /// Environment state.
        /// </summary>
        public EnvironmentState Environment { get; set; } = new EnvironmentState();
    }
}
