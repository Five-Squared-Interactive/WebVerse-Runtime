// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a 2-dimensional integer vector.
    /// </summary>
    public class Vector2Int
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
        /// Magnitude of the vector.
        /// </summary>
        public float magnitude
        {
            get
            {
                return MathF.Sqrt((x * x) + (y * y));
            }
        }

        /// <summary>
        /// Squared magnitude of the vector.
        /// </summary>
        public float squaredMagnitude
        {
            get
            {
                return (x * x) + (y * y);
            }
        }

        /// <summary>
        /// Constructor for a Vector2Int.
        /// </summary>
        public Vector2Int()
        {
            x = 0;
            y = 0;
        }

        /// <summary>
        /// Constructor for a Vector2Int.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Add 2 Vector2s.
        /// </summary>
        /// <param name="first">First Vector2Int to add.</param>
        /// <param name="second">Second Vector2Int to add.</param>
        /// <returns>The sum of the Vector2Ints.</returns>
        public static Vector2Int operator +(Vector2Int first, Vector2Int second) => new Vector2Int(first.x + second.x, first.y + second.y);

        /// <summary>
        /// Subtract two Vector2Ints.
        /// </summary>
        /// <param name="first">Vector2Int to subtract from.</param>
        /// <param name="second">Vector2Int to subtract.</param>
        /// <returns>The difference of the Vector2Ints.</returns>
        public static Vector2Int operator -(Vector2Int first, Vector2Int second) => new Vector2Int(first.x - second.x, first.y - second.y);

        /// <summary>
        /// Multiply (component-wise) two Vector2Ints.
        /// </summary>
        /// <param name="first">First Vector2Int to multiply.</param>
        /// <param name="second">Second Vector2Int to multiply.</param>
        /// <returns>The product of the Vector2Ints.</returns>
        public static Vector2Int operator *(Vector2Int first, Vector2Int second) => new Vector2Int(first.x * second.x, first.y * second.y);

        /// <summary>
        /// Multiply a Vector2Int by an integer value.
        /// </summary>
        /// <param name="first">Vector2Int to multiply.</param>
        /// <param name="second">Integer to multiply Vector2Int components by.</param>
        /// <returns>The product of the Vector2Int and float.</returns>
        public static Vector2Int operator *(Vector2Int first, int second) => new Vector2Int(first.x * second, first.y * second);

        /// <summary>
        /// Multiply a Vector2Int by an integer value.
        /// </summary>
        /// <param name="first">Integer to multiply Vector2Int components by.</param>
        /// <param name="second">Vector2Int to multiply.</param>
        /// <returns>The product of the Vector2Int and float.</returns>
        public static Vector2Int operator *(int first, Vector2Int second) => new Vector2Int(first * second.x, first * second.y);

        /// <summary>
        /// Divide (component-wise) two Vector2Ints.
        /// </summary>
        /// <param name="first">Dividend.</param>
        /// <param name="second">Divisor.</param>
        /// <returns>The quotient of the two Vector2Ints.</returns>
        public static Vector2Int operator /(Vector2Int first, Vector2Int second) => new Vector2Int(first.x / second.x, first.y / second.y);

        /// <summary>
        /// Divide a Vector2Int by an integer value.
        /// </summary>
        /// <param name="first">Vector2Int dividend.</param>
        /// <param name="second">Integer divisor.</param>
        /// <returns>The quotient of the Vector2Int and float.</returns>
        public static Vector2Int operator /(Vector2Int first, int second) => new Vector2Int(first.x / second, first.y / second);

        /// <summary>
        /// Determine whether two Vector2Ints are equal.
        /// </summary>
        /// <param name="first">First Vector2Int.</param>
        /// <param name="second">Second Vector2Int.</param>
        /// <returns>Whether or not the Vector2Ints are equal.</returns>
        public static bool operator ==(Vector2Int first, Vector2Int second) => first.AreEqual(second);

        /// <summary>
        /// Determine whether two Vector2Ints are not equal.
        /// </summary>
        /// <param name="first">First Vector2Int.</param>
        /// <param name="second">Second Vector2Int.</param>
        /// <returns>Whether or not the Vector2Ints are different.</returns>
        public static bool operator !=(Vector2Int first, Vector2Int second) => !first.AreEqual(second);

        /// <summary>
        /// Get a down (0, -1) vector.
        /// </summary>
        public static Vector2Int down
        {
            get
            {
                return new Vector2Int(0, -1);
            }
        }

        /// <summary>
        /// Get an up (0, 1) vector.
        /// </summary>
        public static Vector2Int up
        {
            get
            {
                return new Vector2Int(0, 1);
            }
        }

        /// <summary>
        /// Get a left (-1, 0) vector.
        /// </summary>
        public static Vector2Int left
        {
            get
            {
                return new Vector2Int(-1, 0);
            }
        }

        /// <summary>
        /// Get a right (1, 0) vector.
        /// </summary>
        public static Vector2Int right
        {
            get
            {
                return new Vector2Int(1, 0);
            }
        }

        /// <summary>
        /// Get a vector with both components set to 0.
        /// </summary>
        public static Vector2Int zero
        {
            get
            {
                return new Vector2Int(0, 0);
            }
        }

        /// <summary>
        /// Get a vector with both components set to 1.
        /// </summary>
        public static Vector2Int one
        {
            get
            {
                return new Vector2Int(1, 1);
            }
        }

        /// <summary>
        /// Get a vector with both components set to +infinity.
        /// </summary>
        public static Vector2Int positiveInfinity
        {
            get
            {
                return new Vector2Int(int.MaxValue, int.MaxValue);
            }
        }

        /// <summary>
        /// Get a vector with both components set to -infinity.
        /// </summary>
        public static Vector2Int negativeInfinity
        {
            get
            {
                return new Vector2Int(int.MinValue, int.MinValue);
            }
        }

        /// <summary>
        /// Get the distance between two Vector2Ints.
        /// </summary>
        /// <param name="vector1">First Vector2Int.</param>
        /// <param name="vector2">Second Vector2Int.</param>
        /// <returns>The distance between two Vector2Ints.</returns>
        public static float GetDistance(Vector2Int vector1, Vector2Int vector2)
        {
            UnityEngine.Vector2Int first = new UnityEngine.Vector2Int(vector1.x, vector1.y);
            UnityEngine.Vector2Int second = new UnityEngine.Vector2Int(vector2.x, vector2.y);
            return UnityEngine.Vector2Int.Distance(first, second);
        }

        /// <summary>
        /// Get smallest Vector2Int using the components of two Vector2Ints.
        /// </summary>
        /// <param name="vector1">First Vector2Int.</param>
        /// <param name="vector2">Second Vector2Int.</param>
        /// <returns>Smallest Vector2Int using the components of two Vector2Ints.</returns>
        public static Vector2Int GetMin(Vector2Int vector1, Vector2Int vector2)
        {
            UnityEngine.Vector2Int first = new UnityEngine.Vector2Int(vector1.x, vector1.y);
            UnityEngine.Vector2Int second = new UnityEngine.Vector2Int(vector2.x, vector2.y);
            UnityEngine.Vector2Int result = UnityEngine.Vector2Int.Min(first, second);
            return new Vector2Int(result.x, result.y);
        }

        /// <summary>
        /// Get largest Vector2Int using the components of two Vector2Ints.
        /// </summary>
        /// <param name="vector1">First Vector2Int.</param>
        /// <param name="vector2">Second Vector2Int.</param>
        /// <returns>Largest Vector2Int using the components of two Vector2Ints.</returns>
        public static Vector2Int GetMax(Vector2Int vector1, Vector2Int vector2)
        {
            UnityEngine.Vector2Int first = new UnityEngine.Vector2Int(vector1.x, vector1.y);
            UnityEngine.Vector2Int second = new UnityEngine.Vector2Int(vector2.x, vector2.y);
            UnityEngine.Vector2Int result = UnityEngine.Vector2Int.Max(first, second);
            return new Vector2Int(result.x, result.y);
        }

        /// <summary>
        /// Determine whether or not this Vector2Int equals another.
        /// </summary>
        /// <param name="other">Vector2Int to compare this one with.</param>
        /// <returns>Whether or not this Vector2Int equals the other.</returns>
        public bool AreEqual(Vector2Int other)
        {
            if (other.x == x && other.y == y)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert this Vector2Int to a string.
        /// </summary>
        /// <returns>String representation of this Vector2Int.</returns>
        public override string ToString()
        {
            UnityEngine.Vector2Int v2 = new UnityEngine.Vector2Int(x, y);
            return v2.ToString();
        }

        /// <summary>
        /// Convert this Vector2Int to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Vector2Int.</returns>
        public string ToString(string format)
        {
            UnityEngine.Vector2Int v2 = new UnityEngine.Vector2Int(x, y);
            return v2.ToString(format);
        }

        /// <summary>
        /// Check this Vector2Int for equality with another object.
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