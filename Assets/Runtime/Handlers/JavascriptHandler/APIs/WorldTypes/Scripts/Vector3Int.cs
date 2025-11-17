// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a 3-dimensional integer vector.
    /// </summary>
    public class Vector3Int
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
        /// Magnitude of the vector.
        /// </summary>
        public float magnitude
        {
            get
            {
                return MathF.Sqrt((x * x) + (y * y) + (z * z));
            }
        }

        /// <summary>
        /// Squared magnitude of the vector.
        /// </summary>
        public float squaredMagnitude
        {
            get
            {
                return (x * x) + (y * y) + (z * z);
            }
        }

        /// <summary>
        /// Constructor for a Vector3Int.
        /// </summary>
        public Vector3Int()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        /// <summary>
        /// Constructor for a Vector3Int.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Add 2 Vector3Ints.
        /// </summary>
        /// <param name="first">First Vector3Int to add.</param>
        /// <param name="second">Second Vector3Int to add.</param>
        /// <returns>The sum of the Vector3Ints.</returns>
        public static Vector3Int operator +(Vector3Int first, Vector3Int second) => new Vector3Int(first.x + second.x, first.y + second.y, first.z + second.z);

        /// <summary>
        /// Subtract two Vector3Ints.
        /// </summary>
        /// <param name="first">Vector3Int to subtract from.</param>
        /// <param name="second">Vector3Int to subtract.</param>
        /// <returns>The difference of the Vector3Ints.</returns>
        public static Vector3Int operator -(Vector3Int first, Vector3Int second) => new Vector3Int(first.x - second.x, first.y - second.y, first.z - second.z);

        /// <summary>
        /// Multiply (component-wise) two Vector3Ints.
        /// </summary>
        /// <param name="first">First Vector3Int to multiply.</param>
        /// <param name="second">Second Vector3Int to multiply.</param>
        /// <returns>The product of the Vector3Ints.</returns>
        public static Vector3Int operator *(Vector3Int first, Vector3Int second) => new Vector3Int(first.x * second.x, first.y * second.y, first.z * second.z);

        /// <summary>
        /// Multiply a Vector3Int by an integer value.
        /// </summary>
        /// <param name="first">Vector3Int to multiply.</param>
        /// <param name="second">Int to multiply Vector3Int components by.</param>
        /// <returns>The product of the Vector3Int and int.</returns>
        public static Vector3Int operator *(Vector3Int first, int second) => new Vector3Int(first.x * second, first.y * second, first.z * second);

        /// <summary>
        /// Multiply a Vector3Int by an integer value.
        /// </summary>
        /// <param name="first">Int to multiply Vector3Int components by.</param>
        /// <param name="second">Vector3Int to multiply.</param>
        /// <returns>The product of the Vector3Int and int.</returns>
        public static Vector3Int operator *(int first, Vector3Int second) => new Vector3Int(first * second.x, first * second.y, first * second.z);

        /// <summary>
        /// Divide (component-wise) two Vector3Ints.
        /// </summary>
        /// <param name="first">Dividend.</param>
        /// <param name="second">Divisor.</param>
        /// <returns>The quotient of the two Vector3Ints.</returns>
        public static Vector3Int operator /(Vector3Int first, Vector3Int second) => new Vector3Int(first.x / second.x, first.y / second.y, first.z / second.z);

        /// <summary>
        /// Divide a Vector3Int by an integer value.
        /// </summary>
        /// <param name="first">Vector3Int dividend.</param>
        /// <param name="second">Int divisor.</param>
        /// <returns>The quotient of the Vector3Int and int.</returns>
        public static Vector3Int operator /(Vector3Int first, int second) => new Vector3Int(first.x / second, first.y / second, first.z / second);

        /// <summary>
        /// Determine whether two Vector3Ints are equal.
        /// </summary>
        /// <param name="first">First Vector3Int.</param>
        /// <param name="second">Second Vector3Int.</param>
        /// <returns>Whether or not the Vector3Ints are equal.</returns>
        public static bool operator ==(Vector3Int first, Vector3Int second) => first.AreEqual(second);

        /// <summary>
        /// Determine whether two Vector3Ints are not equal.
        /// </summary>
        /// <param name="first">First Vector3Int.</param>
        /// <param name="second">Second Vector3Int.</param>
        /// <returns>Whether or not the Vector3Ints are different.</returns>
        public static bool operator !=(Vector3Int first, Vector3Int second) => !first.AreEqual(second);

        /// <summary>
        /// Get a forward (0, 0, 1) vector.
        /// </summary>
        public static Vector3Int forward
        {
            get
            {
                return new Vector3Int(0, 0, 1);
            }
        }

        /// <summary>
        /// Get a back (0, 0, -1) vector.
        /// </summary>
        public static Vector3Int back
        {
            get
            {
                return new Vector3Int(0, 0, -1);
            }
        }

        /// <summary>
        /// Get a down (0, -1, 0) vector.
        /// </summary>
        public static Vector3Int down
        {
            get
            {
                return new Vector3Int(0, -1, 0);
            }
        }

        /// <summary>
        /// Get an up (0, 1, 0) vector.
        /// </summary>
        public static Vector3Int up
        {
            get
            {
                return new Vector3Int(0, 1, 0);
            }
        }

        /// <summary>
        /// Get a left (-1, 0, 0) vector.
        /// </summary>
        public static Vector3Int left
        {
            get
            {
                return new Vector3Int(-1, 0, 0);
            }
        }

        /// <summary>
        /// Get a right (1, 0, 0) vector.
        /// </summary>
        public static Vector3Int right
        {
            get
            {
                return new Vector3Int(1, 0, 0);
            }
        }

        /// <summary>
        /// Get a vector with all components set to 0.
        /// </summary>
        public static Vector3Int zero
        {
            get
            {
                return new Vector3Int(0, 0, 0);
            }
        }

        /// <summary>
        /// Get a vector with all components set to 1.
        /// </summary>
        public static Vector3Int one
        {
            get
            {
                return new Vector3Int(1, 1, 1);
            }
        }

        /// <summary>
        /// Get a vector with all components set to +infinity.
        /// </summary>
        public static Vector3Int positiveInfinity
        {
            get
            {
                return new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            }
        }

        /// <summary>
        /// Get a vector with all components set to -infinity.
        /// </summary>
        public static Vector3Int negativeInfinity
        {
            get
            {
                return new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            }
        }

        /// <summary>
        /// Get the distance between two Vector3Ints.
        /// </summary>
        /// <param name="vector1">First Vector3Int.</param>
        /// <param name="vector2">Second Vector3Int.</param>
        /// <returns>The distance between two Vector3Ints.</returns>
        public static float GetDistance(Vector3Int vector1, Vector3Int vector2)
        {
            UnityEngine.Vector3Int first = new UnityEngine.Vector3Int(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3Int second = new UnityEngine.Vector3Int(vector2.x, vector2.y, vector2.z);
            return UnityEngine.Vector3Int.Distance(first, second);
        }

        /// <summary>
        /// Get smallest Vector3Int using the components of two Vector3Ints.
        /// </summary>
        /// <param name="vector1">First Vector3Int.</param>
        /// <param name="vector2">Second Vector3Int.</param>
        /// <returns>Smallest Vector3Int using the components of two Vector3Ints.</returns>
        public static Vector3Int GetMin(Vector3Int vector1, Vector3Int vector2)
        {
            UnityEngine.Vector3Int first = new UnityEngine.Vector3Int(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3Int second = new UnityEngine.Vector3Int(vector2.x, vector2.y, vector2.z);
            UnityEngine.Vector3Int result = UnityEngine.Vector3Int.Min(first, second);
            return new Vector3Int(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get largest Vector3Int using the components of two Vector3Ints.
        /// </summary>
        /// <param name="vector1">First Vector3Int.</param>
        /// <param name="vector2">Second Vector3Int.</param>
        /// <returns>Largest Vector3Int using the components of two Vector3Ints.</returns>
        public static Vector3Int GetMax(Vector3Int vector1, Vector3Int vector2)
        {
            UnityEngine.Vector3Int first = new UnityEngine.Vector3Int(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3Int second = new UnityEngine.Vector3Int(vector2.x, vector2.y, vector2.z);
            UnityEngine.Vector3Int result = UnityEngine.Vector3Int.Max(first, second);
            return new Vector3Int(result.x, result.y, result.z);
        }

        /// <summary>
        /// Determine whether or not this Vector3Int equals another.
        /// </summary>
        /// <param name="other">Vector3Int to compare this one with.</param>
        /// <returns>Whether or not this Vector3Int equals the other.</returns>
        public bool AreEqual(Vector3Int other)
        {
            if (other.x == x && other.y == y && other.z == z)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert this Vector3Int to a string.
        /// </summary>
        /// <returns>String representation of this Vector3Int.</returns>
        public override string ToString()
        {
            UnityEngine.Vector3Int v2 = new UnityEngine.Vector3Int(x, y, z);
            return v2.ToString();
        }

        /// <summary>
        /// Convert this Vector3Int to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Vector3Int.</returns>
        public string ToString(string format)
        {
            UnityEngine.Vector3Int v2 = new UnityEngine.Vector3Int(x, y, z);
            return v2.ToString(format);
        }

        /// <summary>
        /// Check this Vector3Int for equality with another object.
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