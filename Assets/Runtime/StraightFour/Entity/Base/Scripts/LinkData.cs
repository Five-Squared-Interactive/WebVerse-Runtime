// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Represents a clickable link or interaction point on an entity.
    /// Used by BaseEntity for format-agnostic link support (OMI, VEML, X3D).
    /// </summary>
    [System.Serializable]
    public struct LinkData
    {
        /// <summary>
        /// Target URI (URL, world reference, or entity reference).
        /// Examples: "https://example.com", "world://anotherworld", "entity://123"
        /// </summary>
        public string uri;

        /// <summary>
        /// Human-readable title/description of the link.
        /// Displayed in UI when hovering/clicking.
        /// </summary>
        public string title;

        /// <summary>
        /// Optional icon sprite for link visualization.
        /// </summary>
        public Sprite icon;

        /// <summary>
        /// Link activation behavior.
        /// </summary>
        public LinkActivationMode activationMode;

        /// <summary>
        /// Custom metadata for format-specific extensions.
        /// JSON string for flexibility.
        /// </summary>
        public string metadata;

        /// <summary>
        /// Create a simple link with URI and title.
        /// </summary>
        public static LinkData Create(string uri, string title)
        {
            return new LinkData
            {
                uri = uri,
                title = title,
                icon = null,
                activationMode = LinkActivationMode.Click,
                metadata = null
            };
        }
    }

    /// <summary>
    /// How the link is activated by users.
    /// </summary>
    public enum LinkActivationMode
    {
        Click,      // Single click/tap
        Proximity,  // Enter trigger zone
        Gaze,       // Look at for duration (VR)
        Interact    // Press interact button
    }

    /// <summary>
    /// Represents a seat or mounting point where characters can sit/attach.
    /// Used by BaseEntity for format-agnostic seat support (OMI, VEML, X3D).
    /// </summary>
    [System.Serializable]
    public struct SeatData
    {
        /// <summary>
        /// Position of the seat in local space.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Rotation of the seat in local space.
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        /// Back direction vector (where sitter's back faces).
        /// Used for posture calculation.
        /// </summary>
        public Vector3 backDirection;

        /// <summary>
        /// Foot position vector relative to seat origin.
        /// Used for IK and posture.
        /// </summary>
        public Vector3 footOffset;

        /// <summary>
        /// Knee position vector relative to seat origin.
        /// Used for IK and posture.
        /// </summary>
        public Vector3 kneeOffset;

        /// <summary>
        /// Seat angle in degrees (0 = upright, 90 = laying down).
        /// </summary>
        public float seatAngle;

        /// <summary>
        /// Seat type classification.
        /// </summary>
        public SeatType type;

        /// <summary>
        /// Is this seat currently occupied?
        /// </summary>
        public bool isOccupied;

        /// <summary>
        /// Create a basic seat at a position and rotation.
        /// </summary>
        public static SeatData Create(Vector3 position, Quaternion rotation, SeatType type = SeatType.Standard)
        {
            return new SeatData
            {
                position = position,
                rotation = rotation,
                backDirection = rotation * Vector3.back,
                footOffset = Vector3.down * 0.5f,
                kneeOffset = Vector3.down * 0.25f,
                seatAngle = 90f,
                type = type,
                isOccupied = false
            };
        }
    }

    /// <summary>
    /// Type of seat for specialized behavior.
    /// </summary>
    public enum SeatType
    {
        Standard,   // Regular chair
        Pilot,      // Vehicle control seat
        Passenger,  // Non-control vehicle seat
        Mount       // Generic attachment point
    }
}
