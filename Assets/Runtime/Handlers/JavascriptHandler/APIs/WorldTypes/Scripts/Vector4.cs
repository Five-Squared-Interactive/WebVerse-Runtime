// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a 4-dimensional vector.
    /// </summary>
    public class Vector4
    {
        /// <summary>
        /// X component of the vector.
        /// </summary>
        public float x;

        /// <summary>
        /// Y component of the vector.
        /// </summary>
        public float y;

        /// <summary>
        /// Z component of the vector.
        /// </summary>
        public float z;

        /// <summary>
        /// W component of the vector.
        /// </summary>
        public float w;

        /// <summary>
        /// Magnitude of the vector.
        /// </summary>
        public float magnitude
        {
            get
            {
                return MathF.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
            }
        }

        /// <summary>
        /// Squared magnitude of the vector.
        /// </summary>
        public float squaredMagnitude
        {
            get
            {
                return (x * x) + (y * y) + (z * z) + (w * w);
            }
        }

        /// <summary>
        /// Constructor for a Vector4.
        /// </summary>
        public Vector4()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
        }

        /// <summary>
        /// Constructor for a Vector4.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="w">W component.</param>
        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Add 2 Vector4s.
        /// </summary>
        /// <param name="first">First Vector4 to add.</param>
        /// <param name="second">Second Vector4 to add.</param>
        /// <returns>The sum of the Vector4s.</returns>
        public static Vector4 operator +(Vector4 first, Vector4 second) => new Vector4(first.x + second.x, first.y + second.y, first.z + second.z, first.w + second.w);

        /// <summary>
        /// Subtract two Vector4s.
        /// </summary>
        /// <param name="first">Vector4 to subtract from.</param>
        /// <param name="second">Vector4 to subtract.</param>
        /// <returns>The difference of the Vector4s.</returns>
        public static Vector4 operator -(Vector4 first, Vector4 second) => new Vector4(first.x - second.x, first.y - second.y, first.z - second.z, first.w - second.w);

        /// <summary>
        /// Multiply (component-wise) two Vector4s.
        /// </summary>
        /// <param name="first">First Vector4 to multiply.</param>
        /// <param name="second">Second Vector4 to multiply.</param>
        /// <returns>The product of the Vector4s.</returns>
        public static Vector4 operator *(Vector4 first, Vector4 second) => new Vector4(first.x * second.x, first.y * second.y, first.z * second.z, first.w * second.w);

        /// <summary>
        /// Multiply a Vector4 by a floating-point value.
        /// </summary>
        /// <param name="first">Vector4 to multiply.</param>
        /// <param name="second">Float to multiply Vector4 components by.</param>
        /// <returns>The product of the Vector4 and float.</returns>
        public static Vector4 operator *(Vector4 first, float second) => new Vector4(first.x * second, first.y * second, first.z * second, first.w * second);

        /// <summary>
        /// Multiply a Vector4 by a floating-point value.
        /// </summary>
        /// <param name="first">Float to multiply Vector4 components by.</param>
        /// <param name="second">Vector4 to multiply.</param>
        /// <returns>The product of the Vector4 and float.</returns>
        public static Vector4 operator *(float first, Vector4 second) => new Vector4(first * second.x, first * second.y, first * second.z, first * second.w);

        /// <summary>
        /// Divide (component-wise) two Vector4s.
        /// </summary>
        /// <param name="first">Dividend.</param>
        /// <param name="second">Divisor.</param>
        /// <returns>The quotient of the two Vector4s.</returns>
        public static Vector4 operator /(Vector4 first, Vector4 second) => new Vector4(first.x / second.x, first.y / second.y, first.z / second.z, first.w / second.w);

        /// <summary>
        /// Divide a Vector4 by a floating-point value.
        /// </summary>
        /// <param name="first">Vector4 dividend.</param>
        /// <param name="second">Float divisor.</param>
        /// <returns>The quotient of the Vector4 and float.</returns>
        public static Vector4 operator /(Vector4 first, float second) => new Vector4(first.x / second, first.y / second, first.z / second, first.w / second);

        /// <summary>
        /// Determine whether two Vector4s are equal.
        /// </summary>
        /// <param name="first">First Vector4.</param>
        /// <param name="second">Second Vector4.</param>
        /// <returns>Whether or not the Vector4s are equal.</returns>
        public static bool operator ==(Vector4 first, Vector4 second) => first.AreEqual(second);

        /// <summary>
        /// Determine whether two Vector4s are not equal.
        /// </summary>
        /// <param name="first">First Vector4.</param>
        /// <param name="second">Second Vector4.</param>
        /// <returns>Whether or not the Vector4s are different.</returns>
        public static bool operator !=(Vector4 first, Vector4 second) => !first.AreEqual(second);

        /// <summary>
        /// Get a vector with all components set to 0.
        /// </summary>
        public static Vector4 zero
        {
            get
            {
                return new Vector4(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Get a vector with all components set to 1.
        /// </summary>
        public static Vector4 one
        {
            get
            {
                return new Vector4(1, 1, 1, 1);
            }
        }

        /// <summary>
        /// Get a vector with all components set to +infinity.
        /// </summary>
        public static Vector4 positiveInfinity
        {
            get
            {
                return new Vector4(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            }
        }

        /// <summary>
        /// Get a vector with all components set to -infinity.
        /// </summary>
        public static Vector4 negativeInfinity
        {
            get
            {
                return new Vector4(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            }
        }

        /// <summary>
        /// Get the distance between two Vector4s.
        /// </summary>
        /// <param name="vector1">First Vector4.</param>
        /// <param name="vector2">Second Vector4.</param>
        /// <returns>The distance between two Vector4s.</returns>
        public static float GetDistance(Vector4 vector1, Vector4 vector2)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4(vector1.x, vector1.y, vector1.z, vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4(vector2.x, vector2.y, vector2.z, vector2.w);
            return UnityEngine.Vector4.Distance(first, second);
        }

        /// <summary>
        /// Get the dot product between two Vector4s.
        /// </summary>
        /// <param name="leftHand">Left hand Vector4.</param>
        /// <param name="rightHand">Right hand Vector4.</param>
        /// <returns>The dot product between two Vector4s.</returns>
        public static float GetDotProduct(Vector4 leftHand, Vector4 rightHand)
        {
            UnityEngine.Vector4 l = new UnityEngine.Vector4(leftHand.x, leftHand.y, leftHand.z, leftHand.w);
            UnityEngine.Vector4 r = new UnityEngine.Vector4(rightHand.x, rightHand.y, rightHand.z, rightHand.w);
            return UnityEngine.Vector4.Dot(l, r);
        }

        /// <summary>
        /// Get the result of projecting a vector on a normal.
        /// </summary>
        /// <param name="vector">Vector to project.</param>
        /// <param name="normal">Normal to project vector on.</param>
        /// <returns>The result of projecting a vector on a normal.</returns>
        public static Vector4 Project(Vector4 vector, Vector4 normal)
        {
            UnityEngine.Vector4 vec = new UnityEngine.Vector4(vector.x, vector.y, vector.z, vector.w);
            UnityEngine.Vector4 norm = new UnityEngine.Vector4(normal.x, normal.y, normal.z, normal.w);
            UnityEngine.Vector4 res = UnityEngine.Vector4.Project(vec, norm);
            return new Vector4(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector4s percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector4.</param>
        /// <param name="vector2">Second Vector4.</param>
        /// <param name="percent">Percent between first and second Vector4.</param>
        /// <returns>A linearly interpolated Vector4 between the two Vector4s.</returns>
        public static Vector4 LinearlyInterpolatePercent(Vector4 vector1, Vector4 vector2, float percent)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4(vector1.x, vector1.y, vector1.z, vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4(vector2.x, vector2.y, vector2.z, vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.Lerp(first, second, percent);
            return new Vector4(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector4s distance-wise.
        /// </summary>
        /// <param name="vector1">First Vector4.</param>
        /// <param name="vector2">Second Vector4.</param>
        /// <param name="maxDistance">Maximum distance between first and second Vector4.</param>
        /// <returns>A linearly interpolated Vector4 between the two Vector4s.</returns>
        public static Vector4 LinearlyInterpolate(Vector4 vector1, Vector4 vector2, float maxDistance)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4(vector1.x, vector1.y, vector1.z, vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4(vector2.x, vector2.y, vector2.z, vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.MoveTowards(first, second, maxDistance);
            return new Vector4(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Lineraly interpolate unclamped between two Vector4s percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector4.</param>
        /// <param name="vector2">Second Vector4.</param>
        /// <param name="percent">Percent between first and second Vector4.</param>
        /// <returns>A linearly interpolated unclamped Vector4 between the two Vector4s.</returns>
        public static Vector4 LinearlyInterpolatePercentUnclamped(Vector4 vector1, Vector4 vector2, float percent)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4(vector1.x, vector1.y, vector1.z, vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4(vector2.x, vector2.y, vector2.z, vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.LerpUnclamped(first, second, percent);
            return new Vector4(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Get smallest Vector4 using the components of two Vector4s.
        /// </summary>
        /// <param name="vector1">First Vector4.</param>
        /// <param name="vector2">Second Vector4.</param>
        /// <returns>Smallest Vector4 using the components of two Vector4s.</returns>
        public static Vector4 GetMin(Vector4 vector1, Vector4 vector2)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4(vector1.x, vector1.y, vector1.z, vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4(vector2.x, vector2.y, vector2.z, vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.Min(first, second);
            return new Vector4(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Get largest Vector4 using the components of two Vector4s.
        /// </summary>
        /// <param name="vector1">First Vector4.</param>
        /// <param name="vector2">Second Vector4.</param>
        /// <returns>Largest Vector4 using the components of two Vector4s.</returns>
        public static Vector4 GetMax(Vector4 vector1, Vector4 vector2)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4(vector1.x, vector1.y, vector1.z, vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4(vector2.x, vector2.y, vector2.z, vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.Max(first, second);
            return new Vector4(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Get a normalized (magnitude 1) version of the vector.
        /// </summary>
        /// <returns>A normalized (magnitude 1) version of the vector.</returns>
        public Vector4 GetNormalized()
        {
            float scaleFactor = 1 / magnitude;

            return new Vector4(x * scaleFactor, y * scaleFactor, z * scaleFactor, w * scaleFactor);
        }

        /// <summary>
        /// Normalize the Vector4.
        /// </summary>
        public void Normalize()
        {
            float scaleFactor = 1 / magnitude;

            x *= scaleFactor;
            y *= scaleFactor;
            z *= scaleFactor;
        }

        /// <summary>
        /// Determine whether or not this Vector4 equals another.
        /// </summary>
        /// <param name="other">Vector4 to compare this one with.</param>
        /// <returns>Whether or not this Vector4 equals the other.</returns>
        public bool AreEqual(Vector4 other)
        {
            if (other.x == x && other.y == y && other.z == z)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert this Vector4 to a string.
        /// </summary>
        /// <returns>String representation of this Vector4.</returns>
        public override string ToString()
        {
            UnityEngine.Vector4 v2 = new UnityEngine.Vector4(x, y, z);
            return v2.ToString();
        }

        /// <summary>
        /// Convert this Vector4 to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Vector4.</returns>
        public string ToString(string format)
        {
            UnityEngine.Vector4 v2 = new UnityEngine.Vector4(x, y, z);
            return v2.ToString(format);
        }

        /// <summary>
        /// Check this Vector4 for equality with another object.
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
    }
}