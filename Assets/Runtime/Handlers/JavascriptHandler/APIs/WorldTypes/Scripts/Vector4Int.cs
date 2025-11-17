// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a 4-dimensional integer vector.
    /// </summary>
    public class Vector4Int
    {
        /// <summary>
        /// X component of the vector.
        /// </summary>
        public int x;

        /// <summary>
        /// Y component of the vector.
        /// </summary>
        public int y;

        /// <summary>
        /// Z component of the vector.
        /// </summary>
        public int z;

        /// <summary>
        /// W component of the vector.
        /// </summary>
        public int w;

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
        /// Constructor for a Vector4Int.
        /// </summary>
        public Vector4Int()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
        }

        /// <summary>
        /// Constructor for a Vector4Int.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="w">W component.</param>
        public Vector4Int(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Add 2 Vector4Ints.
        /// </summary>
        /// <param name="first">First Vector4Int to add.</param>
        /// <param name="second">Second Vector4Int to add.</param>
        /// <returns>The sum of the Vector4Ints.</returns>
        public static Vector4Int operator +(Vector4Int first, Vector4Int second) => new Vector4Int(first.x + second.x, first.y + second.y, first.z + second.z, first.w + second.w);

        /// <summary>
        /// Subtract two Vector4Ints.
        /// </summary>
        /// <param name="first">Vector4Int to subtract from.</param>
        /// <param name="second">Vector4Int to subtract.</param>
        /// <returns>The difference of the Vector4Ints.</returns>
        public static Vector4Int operator -(Vector4Int first, Vector4Int second) => new Vector4Int(first.x - second.x, first.y - second.y, first.z - second.z, first.w - second.w);

        /// <summary>
        /// Multiply (component-wise) two Vector4Ints.
        /// </summary>
        /// <param name="first">First Vector4Int to multiply.</param>
        /// <param name="second">Second Vector4Int to multiply.</param>
        /// <returns>The product of the Vector4Ints.</returns>
        public static Vector4Int operator *(Vector4Int first, Vector4Int second) => new Vector4Int(first.x * second.x, first.y * second.y, first.z * second.z, first.w * second.w);

        /// <summary>
        /// Multiply a Vector4Int by an integer value.
        /// </summary>
        /// <param name="first">Vector4Int to multiply.</param>
        /// <param name="second">Integer to multiply Vector4Int components by.</param>
        /// <returns>The product of the Vector4Int and int.</returns>
        public static Vector4Int operator *(Vector4Int first, int second) => new Vector4Int(first.x * second, first.y * second, first.z * second, first.w * second);

        /// <summary>
        /// Multiply a Vector4Int by an integer value.
        /// </summary>
        /// <param name="first">Integer to multiply Vector4Int components by.</param>
        /// <param name="second">Vector4Int to multiply.</param>
        /// <returns>The product of the Vector4Int and int.</returns>
        public static Vector4Int operator *(int first, Vector4Int second) => new Vector4Int(first * second.x, first * second.y, first * second.z, first * second.w);

        /// <summary>
        /// Divide (component-wise) two Vector4Ints.
        /// </summary>
        /// <param name="first">Dividend.</param>
        /// <param name="second">Divisor.</param>
        /// <returns>The quotient of the two Vector4Ints.</returns>
        public static Vector4Int operator /(Vector4Int first, Vector4Int second) => new Vector4Int(first.x / second.x, first.y / second.y, first.z / second.z, first.w / second.w);

        /// <summary>
        /// Divide a Vector4Int by an integer value.
        /// </summary>
        /// <param name="first">Vector4Int dividend.</param>
        /// <param name="second">Integer divisor.</param>
        /// <returns>The quotient of the Vector4Int and int.</returns>
        public static Vector4Int operator /(Vector4Int first, int second) => new Vector4Int(first.x / second, first.y / second, first.z / second, first.w / second);

        /// <summary>
        /// Determine whether two Vector4Ints are equal.
        /// </summary>
        /// <param name="first">First Vector4Int.</param>
        /// <param name="second">Second Vector4Int.</param>
        /// <returns>Whether or not the Vector4Ints are equal.</returns>
        public static bool operator ==(Vector4Int first, Vector4Int second) => first.AreEqual(second);

        /// <summary>
        /// Determine whether two Vector4Ints are not equal.
        /// </summary>
        /// <param name="first">First Vector4Int.</param>
        /// <param name="second">Second Vector4Int.</param>
        /// <returns>Whether or not the Vector4Ints are different.</returns>
        public static bool operator !=(Vector4Int first, Vector4Int second) => !first.AreEqual(second);

        /// <summary>
        /// Get a vector with all components set to 0.
        /// </summary>
        public static Vector4Int zero
        {
            get
            {
                return new Vector4Int(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Get a vector with all components set to 1.
        /// </summary>
        public static Vector4Int one
        {
            get
            {
                return new Vector4Int(1, 1, 1, 1);
            }
        }

        /// <summary>
        /// Get a vector with all components set to +infinity.
        /// </summary>
        public static Vector4Int positiveInfinity
        {
            get
            {
                return new Vector4Int(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
            }
        }

        /// <summary>
        /// Get a vector with all components set to -infinity.
        /// </summary>
        public static Vector4Int negativeInfinity
        {
            get
            {
                return new Vector4Int(int.MinValue, int.MinValue, int.MinValue, int.MinValue);
            }
        }

        /// <summary>
        /// Get the distance between two Vector4Ints.
        /// </summary>
        /// <param name="vector1">First Vector4Int.</param>
        /// <param name="vector2">Second Vector4Int.</param>
        /// <returns>The distance between two Vector4Ints.</returns>
        public static float GetDistance(Vector4Int vector1, Vector4Int vector2)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4(vector1.x, vector1.y, vector1.z, vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4(vector2.x, vector2.y, vector2.z, vector2.w);
            return UnityEngine.Vector4.Distance(first, second);
        }

        /// <summary>
        /// Get smallest Vector4Int using the components of two Vector4Ints.
        /// </summary>
        /// <param name="vector1">First Vector4Int.</param>
        /// <param name="vector2">Second Vector4Int.</param>
        /// <returns>Smallest Vector4Int using the components of two Vector4Ints.</returns>
        public static Vector4Int GetMin(Vector4Int vector1, Vector4Int vector2)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4(vector1.x, vector1.y, vector1.z, vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4(vector2.x, vector2.y, vector2.z, vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.Min(first, second);
            return new Vector4Int((int) result.x, (int) result.y, (int) result.z, (int) result.w);
        }

        /// <summary>
        /// Get largest Vector4Int using the components of two Vector4Ints.
        /// </summary>
        /// <param name="vector1">First Vector4Int.</param>
        /// <param name="vector2">Second Vector4Int.</param>
        /// <returns>Largest Vector4Int using the components of two Vector4Ints.</returns>
        public static Vector4Int GetMax(Vector4Int vector1, Vector4Int vector2)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4(vector1.x, vector1.y, vector1.z, vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4(vector2.x, vector2.y, vector2.z, vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.Max(first, second);
            return new Vector4Int((int) result.x, (int) result.y, (int) result.z, (int) result.w);
        }

        /// <summary>
        /// Determine whether or not this Vector4Int equals another.
        /// </summary>
        /// <param name="other">Vector4Int to compare this one with.</param>
        /// <returns>Whether or not this Vector4Int equals the other.</returns>
        public bool AreEqual(Vector4Int other)
        {
            if (other.x == x && other.y == y && other.z == z)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert this Vector4Int to a string.
        /// </summary>
        /// <returns>String representation of this Vector4Int.</returns>
        public override string ToString()
        {
            UnityEngine.Vector4 v2 = new UnityEngine.Vector4(x, y, z);
            return v2.ToString();
        }

        /// <summary>
        /// Convert this Vector4Int to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Vector4Int.</returns>
        public string ToString(string format)
        {
            UnityEngine.Vector4 v2 = new UnityEngine.Vector4(x, y, z);
            return v2.ToString(format);
        }

        /// <summary>
        /// Check this Vector4Int for equality with another object.
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