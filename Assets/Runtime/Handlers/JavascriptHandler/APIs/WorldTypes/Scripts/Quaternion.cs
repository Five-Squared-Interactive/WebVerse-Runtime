// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a quaternion.
    /// </summary>
    public class Quaternion
    {
        /// <summary>
        /// X component of the quaternion.
        /// </summary>
        public float x;

        /// <summary>
        /// Y component of the quaternion.
        /// </summary>
        public float y;

        /// <summary>
        /// Z component of the quaternion.
        /// </summary>
        public float z;

        /// <summary>
        /// W component of the quaternion.
        /// </summary>
        public float w;

        /// <summary>
        /// Constructor for a Quaternion.
        /// </summary>
        public Quaternion()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
        }

        /// <summary>
        /// Constructor for a Quaternion.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="w">W component.</param>
        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Constructor for a Quaternion using an angle axis input.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="axis"></param>
        public Quaternion(float angle, Vector3 axis)
        {
            UnityEngine.Quaternion res = UnityEngine.Quaternion.AngleAxis(angle, new UnityEngine.Vector3(axis.x, axis.y, axis.z));
            x = res.x;
            y = res.y;
            z = res.z;
            w = res.w;
        }

        /// <summary>
        /// Multiply (combine) two Quaternions.
        /// </summary>
        /// <param name="first">First Quaternion to multiply.</param>
        /// <param name="second">Second Quaternion to multiply.</param>
        /// <returns>The product of the Quaternions.</returns>
        public static Quaternion operator *(Quaternion first, Quaternion second) => Combine(first, second);

        /// <summary>
        /// Determine whether two Quaternions are equal.
        /// </summary>
        /// <param name="first">First Quaternion.</param>
        /// <param name="second">Second Quaternion.</param>
        /// <returns>Whether or not the Quaternions are equal.</returns>
        public static bool operator ==(Quaternion first, Quaternion second) => AreEqual(first, second);

        /// <summary>
        /// Determine whether two Quaternions are not equal.
        /// </summary>
        /// <param name="first">First Quaternion.</param>
        /// <param name="second">Second Quaternion.</param>
        /// <returns>Whether or not the Quaternions are different.</returns>
        public static bool operator !=(Quaternion first, Quaternion second) => !AreEqual(first, second);

        /// <summary>
        /// An identity (0, 0, 0, 1) quaternion.
        /// </summary>
        public static Quaternion identity
        {
            get
            {
                return new Quaternion(0, 0, 0, 1);
            }
        }

        /// <summary>
        /// Get the angle between two Quaternions.
        /// </summary>
        /// <param name="quaternion1">First Quaternion.</param>
        /// <param name="quaternion2">Second Quaternion.</param>
        /// <returns>The angle between the two quaternions.</returns>
        public static float GetAngle(Quaternion quaternion1, Quaternion quaternion2)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion(quaternion1.x, quaternion1.y, quaternion1.z, quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
            return UnityEngine.Quaternion.Angle(first, second);
        }

        /// <summary>
        /// Get a Quaternion from Euler angles.
        /// </summary>
        /// <param name="x">Rotation around x axis.</param>
        /// <param name="y">Rotation around y axis.</param>
        /// <param name="z">Rotation around z axis.</param>
        /// <returns>Quaternion from provided Euler angles.</returns>
        public static Quaternion FromEulerAngles(float x, float y, float z)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3(x, y, z);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.Euler(vec);
            return new Quaternion(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Get the inverse of a Quaternions.
        /// </summary>
        /// <param name="quaternion1">First Quaternion.</param>
        /// <param name="quaternion2">Second Quaternion.</param>
        /// <returns>The angle between the two quaternions.</returns>
        public static Quaternion GetInverse(Quaternion quaternion)
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.Inverse(quat);
            return new Quaternion(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Lineraly interpolate between two Quaternions percentage-wise.
        /// </summary>
        /// <param name="quaternion1">First Quaternion.</param>
        /// <param name="quaternion2">Second Quaternion.</param>
        /// <param name="percent">Percent between first and second Quaternion.</param>
        /// <returns>A linearly interpolated Quaternion between the two Quaternions.</returns>
        public static Quaternion LinearlyInterpolatePercent(Quaternion quaternion1, Quaternion quaternion2, float percent)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion(quaternion1.x, quaternion1.y, quaternion1.z, quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.Lerp(first, second, percent);
            return new Quaternion(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Lineraly interpolate between two Quaternions distance-wise.
        /// </summary>
        /// <param name="quaternion1">First Quaternion.</param>
        /// <param name="quaternion2">Second Quaternion.</param>
        /// <param name="maxDistance">Maximum distance between first and second Quaternion.</param>
        /// <returns>A linearly interpolated Quaternion between the two Quaternions.</returns>
        public static Quaternion LinearlyInterpolate(Quaternion quaternion1, Quaternion quaternion2, float maxDistance)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion(quaternion1.x, quaternion1.y, quaternion1.z, quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.RotateTowards(first, second, maxDistance);
            return new Quaternion(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Lineraly interpolate unclamped between two Quaternions percentage-wise.
        /// </summary>
        /// <param name="quaternion1">First Quaternion.</param>
        /// <param name="quaternion2">Second Quaternion.</param>
        /// <param name="percent">Percent between first and second Quaternion.</param>
        /// <returns>A linearly interpolated unclamped Quaternion between the two Quaternions.</returns>
        public static Quaternion LinearlyInterpolatePercentUnclamped(Quaternion quaternion1, Quaternion quaternion2, float percent)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion(quaternion1.x, quaternion1.y, quaternion1.z, quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.LerpUnclamped(first, second, percent);
            return new Quaternion(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Create a quaternion with a given forward and up direction.
        /// </summary>
        /// <param name="forward">Forward direction.</param>
        /// <param name="up">Up direction.</param>
        /// <returns>A quaternion with a given forward and up direction.</returns>
        public static Quaternion CreateLookRotation(Vector3 forward, Vector3 up)
        {
            UnityEngine.Vector3 f = new UnityEngine.Vector3(forward.x, forward.y, forward.z);
            UnityEngine.Vector3 u = new UnityEngine.Vector3(up.x, up.y, up.z);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.LookRotation(f, u);
            return new Quaternion(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Create a quaternion with a given forward direction and an up direction facing Vector3.Up.
        /// </summary>
        /// <param name="forward">Forward direction.</param>
        /// <returns>A quaternion with a given forward direction and an up direction facing Vector3.up.</returns>
        public static Quaternion CreateLookRotation(Vector3 forward)
        {
            UnityEngine.Vector3 f = new UnityEngine.Vector3(forward.x, forward.y, forward.z);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.LookRotation(f, UnityEngine.Vector3.up);
            return new Quaternion(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Get Euler angle representation of the quaternion.
        /// </summary>
        /// <returns>Euler angle representation of the quaternion.</returns>
        public Vector3 GetEulerAngles()
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion(x, y, z, w);
            UnityEngine.Vector3 res = quat.eulerAngles;
            return new Vector3(res.x, res.y, res.z);
        }

        /// <summary>
        /// Get the dot product between two Quaternions.
        /// </summary>
        /// <param name="first">First Quaternion.</param>
        /// <param name="second">Second Quaternion.</param>
        /// <returns>The dot product between two Quaternions.</returns>
        public static float GetDotProduct(Quaternion first, Quaternion second)
        {
            UnityEngine.Quaternion f = new UnityEngine.Quaternion(first.x, first.y, first.z, first.w);
            UnityEngine.Quaternion s = new UnityEngine.Quaternion(second.x, second.y, second.z, second.w);
            return UnityEngine.Quaternion.Dot(f, s);
        }

        /// <summary>
        /// Get the rotation from one Vector3 to another Vector3.
        /// </summary>
        /// <param name="from">First Vector3.</param>
        /// <param name="to">Second Vector3.</param>
        /// <returns>The rotation from one Vector3 to another Vector3.</returns>
        public static Quaternion GetRotationFromToVector3s(Vector3 from, Vector3 to)
        {
            UnityEngine.Vector3 f = new UnityEngine.Vector3(from.x, from.y, from.z);
            UnityEngine.Vector3 t = new UnityEngine.Vector3(to.x, to.y, to.z);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.FromToRotation(f, t);
            return new Quaternion(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Get a normalized Quaternion (with a magnitude of 1).
        /// </summary>
        /// <param name="quaternion">Input Quaternion.</param>
        /// <returns>A normalized Quaternion.</returns>
        public static Quaternion GetNormalized(Quaternion quaternion)
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.Normalize(quat);
            return new Quaternion(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Interpolate between two Quaternions.
        /// </summary>
        /// <param name="quaternion1">First Quaternion.</param>
        /// <param name="quaternion2">Second Quaternion.</param>
        /// <param name="maxDegrees">Maximum degrees between first and second Quaternion.</param>
        /// <returns>An interpolated Quaternion between the two Quaternions.</returns>
        public static Quaternion Interpolate(Quaternion quaternion1, Quaternion quaternion2, float maxDegrees)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion(quaternion1.x, quaternion1.y, quaternion1.z, quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.RotateTowards(first, second, maxDegrees);
            return new Quaternion(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Spherically interpolate between two Quaternions.
        /// </summary>
        /// <param name="quaternion1">First Quaternion.</param>
        /// <param name="quaternion2">Second Quaternion.</param>
        /// <param name="percent">Percentage between the two quaternions to interpolate.</param>
        /// <returns>A spherically interpolated Quaternion between the two Quaternions.</returns>
        public static Quaternion SphericallyInterpolate(Quaternion quaternion1, Quaternion quaternion2, float percent)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion(quaternion1.x, quaternion1.y, quaternion1.z, quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.Slerp(first, second, percent);
            return new Quaternion(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Spherically interpolate unclamped between two Quaternions.
        /// </summary>
        /// <param name="quaternion1">First Quaternion.</param>
        /// <param name="quaternion2">Second Quaternion.</param>
        /// <param name="percent">Percentage between the two quaternions to interpolate.</param>
        /// <returns>A spherically interpolated unclamped Quaternion between the two Quaternions.</returns>
        public static Quaternion SphericallyInterpolateUnclamped(Quaternion quaternion1, Quaternion quaternion2, float percent)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion(quaternion1.x, quaternion1.y, quaternion1.z, quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.SlerpUnclamped(first, second, percent);
            return new Quaternion(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Get angle axis representation of the quaternion.
        /// </summary>
        /// <returns>Angle axis representation of the quaternion.</returns>
        public Tuple<float, Vector3> GetAngleAxis()
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion(x, y, z, w);
            float angle = 0;
            UnityEngine.Vector3 axis = UnityEngine.Vector3.zero;
            quat.ToAngleAxis(out angle, out axis);
            return new Tuple<float, Vector3>(angle, new Vector3(axis.x, axis.y, axis.z));
        }

        /// <summary>
        /// Convert this Quaternion to a string.
        /// </summary>
        /// <returns>String representation of this Quaternion.</returns>
        public override string ToString()
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion(x, y, z, w);
            return quat.ToString();
        }

        /// <summary>
        /// Convert this Quaternion to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Quaternion.</returns>
        public string ToString(string format)
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion(x, y, z, w);
            return quat.ToString(format);
        }

        /// <summary>
        /// Check this Quaternion for equality with another object.
        /// </summary>
        /// <param name="obj">Object to compare.</param>
        /// <returns>Whether or not the objects are equal.</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Get a hash code.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Check the equality of two Quaternions.
        /// </summary>
        /// <param name="quaternion1">First quaternion.</param>
        /// <param name="quaternion2">Second quaternion.</param>
        /// <returns>Whether or not the quaternions are equal.</returns>
        private static bool AreEqual(Quaternion quaternion1, Quaternion quaternion2)
        {
            return quaternion1.x == quaternion2.x && quaternion1.y == quaternion2.y
                && quaternion1.z == quaternion2.z && quaternion1.w == quaternion2.w;
        }

        /// <summary>
        /// Combine two Quaternions.
        /// </summary>
        /// <param name="quaternion1">First quaternion.</param>
        /// <param name="quaternion2">Second quaternion.</param>
        /// <returns>The combination of the two Quaternions.</returns>
        public static Quaternion Combine(Quaternion quaternion1, Quaternion quaternion2)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion(quaternion1.x, quaternion1.y, quaternion1.z, quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion(quaternion2.x, quaternion2.y, quaternion2.z, quaternion2.w);
            UnityEngine.Quaternion result = first * second;
            return new Quaternion(result.x, result.y, result.z, result.w);
        }
    }
}