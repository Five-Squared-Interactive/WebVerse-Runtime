// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Serializable avatar animation state for sync broadcasting.
    /// Compact struct (~200 bytes) designed for WorldSync MQTT constraints.
    /// </summary>
    [Serializable]
    public struct AvatarState
    {
        // Animation state (from AvatarAnimationManager)
        public float LocomotionSpeed;
        public float LocomotionDirection;
        public string ActiveEmote;
        public float HeadYaw;
        public float HeadPitch;

        // IK state (from AvatarRigController, VR only)
        public Vector3 HeadPosition;
        public Quaternion HeadRotation;
        public Vector3 LeftHandPosition;
        public Quaternion LeftHandRotation;
        public Vector3 RightHandPosition;
        public Quaternion RightHandRotation;

        // Calibration (from AvatarRigController, VR only)
        public float HeightScale;
        public float ArmSpanScale;

        // Metadata
        public bool IsVRMode;
        public string AvatarModelUri;

        private const int MaxEmoteLength = 32;
        private const int MaxUriLength = 64;

        /// <summary>
        /// Serialize this AvatarState to a compact binary format (~200 bytes).
        /// Uses BinaryWriter with fixed field order for WorldSync MQTT compatibility.
        /// </summary>
        /// <returns>Byte array containing the serialized state.</returns>
        public byte[] Serialize()
        {
            using (var ms = new MemoryStream(256))
            using (var writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                // Animation floats (16 bytes)
                writer.Write(LocomotionSpeed);
                writer.Write(LocomotionDirection);
                writer.Write(HeadYaw);
                writer.Write(HeadPitch);

                // ActiveEmote: length-prefixed UTF8, max 32 chars
                WriteString(writer, ActiveEmote, MaxEmoteLength);

                // IK state: 6 pose values (84 bytes)
                WriteVector3(writer, HeadPosition);
                WriteQuaternion(writer, HeadRotation);
                WriteVector3(writer, LeftHandPosition);
                WriteQuaternion(writer, LeftHandRotation);
                WriteVector3(writer, RightHandPosition);
                WriteQuaternion(writer, RightHandRotation);

                // Calibration (8 bytes)
                writer.Write(HeightScale);
                writer.Write(ArmSpanScale);

                // Metadata
                writer.Write(IsVRMode);
                WriteString(writer, AvatarModelUri, MaxUriLength);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserialize an AvatarState from binary data produced by Serialize().
        /// </summary>
        /// <param name="data">Binary data to deserialize.</param>
        /// <returns>The deserialized AvatarState.</returns>
        /// <summary>
        /// Minimum valid serialized size: 4 animation floats + 1 emote length byte +
        /// 6 pose values (3 Vec3 + 3 Quat) + 2 calibration floats + 1 bool + 1 URI length byte.
        /// </summary>
        private const int MinSerializedSize = 16 + 1 + 84 + 8 + 1 + 1; // = 111 bytes

        public static AvatarState Deserialize(byte[] data)
        {
            if (data == null || data.Length < MinSerializedSize)
            {
                return default;
            }

            var state = new AvatarState();

            try
            {
                using (var ms = new MemoryStream(data))
                using (var reader = new BinaryReader(ms, Encoding.UTF8))
                {
                    // Animation floats
                    state.LocomotionSpeed = reader.ReadSingle();
                    state.LocomotionDirection = reader.ReadSingle();
                    state.HeadYaw = reader.ReadSingle();
                    state.HeadPitch = reader.ReadSingle();

                    // ActiveEmote
                    state.ActiveEmote = ReadString(reader);

                    // IK state
                    state.HeadPosition = ReadVector3(reader);
                    state.HeadRotation = ReadQuaternion(reader);
                    state.LeftHandPosition = ReadVector3(reader);
                    state.LeftHandRotation = ReadQuaternion(reader);
                    state.RightHandPosition = ReadVector3(reader);
                    state.RightHandRotation = ReadQuaternion(reader);

                    // Calibration
                    state.HeightScale = reader.ReadSingle();
                    state.ArmSpanScale = reader.ReadSingle();

                    // Metadata
                    state.IsVRMode = reader.ReadBoolean();
                    state.AvatarModelUri = ReadString(reader);
                }
            }
            catch (System.Exception)
            {
                // Corrupted data from network — return default state rather than crashing
                return default;
            }

            return state;
        }

        private static void WriteString(BinaryWriter writer, string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                writer.Write((byte)0);
                return;
            }

            if (value.Length > maxLength)
            {
                value = value.Substring(0, maxLength);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);

            // Guard against multi-byte UTF8 exceeding the byte-length byte (max 255)
            if (bytes.Length > 255)
            {
                // Truncate to fit within 255 bytes, respecting UTF8 char boundaries
                int safeLength = 255;
                while (safeLength > 0 && (bytes[safeLength] & 0xC0) == 0x80)
                {
                    safeLength--;
                }
                byte[] truncated = new byte[safeLength];
                Array.Copy(bytes, truncated, safeLength);
                bytes = truncated;
            }

            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader)
        {
            byte length = reader.ReadByte();
            if (length == 0) return "";
            byte[] bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        private static void WriteVector3(BinaryWriter writer, Vector3 v)
        {
            writer.Write(v.x);
            writer.Write(v.y);
            writer.Write(v.z);
        }

        private static Vector3 ReadVector3(BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        private static void WriteQuaternion(BinaryWriter writer, Quaternion q)
        {
            writer.Write(q.x);
            writer.Write(q.y);
            writer.Write(q.z);
            writer.Write(q.w);
        }

        private static Quaternion ReadQuaternion(BinaryReader reader)
        {
            return new Quaternion(
                reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
