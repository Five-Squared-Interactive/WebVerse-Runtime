// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a 2-dimensional vector.
    /// </summary>
    public class Vector2
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
        /// Constructor for a Vector2.
        /// </summary>
        public Vector2()
        {
            x = 0;
            y = 0;
        }

        /// <summary>
        /// Constructor for a Vector2.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Add 2 Vector2s.
        /// </summary>
        /// <param name="first">First Vector2 to add.</param>
        /// <param name="second">Second Vector2 to add.</param>
        /// <returns>The sum of the Vector2s.</returns>
        public static Vector2 operator +(Vector2 first, Vector2 second) => new Vector2(first.x + second.x, first.y + second.y);

        /// <summary>
        /// Subtract two Vector2s.
        /// </summary>
        /// <param name="first">Vector2 to subtract from.</param>
        /// <param name="second">Vector2 to subtract.</param>
        /// <returns>The difference of the Vector2s.</returns>
        public static Vector2 operator -(Vector2 first, Vector2 second) => new Vector2(first.x - second.x, first.y - second.y);

        /// <summary>
        /// Multiply (component-wise) two Vector2s.
        /// </summary>
        /// <param name="first">First Vector2 to multiply.</param>
        /// <param name="second">Second Vector2 to multiply.</param>
        /// <returns>The product of the Vector2s.</returns>
        public static Vector2 operator *(Vector2 first, Vector2 second) => new Vector2(first.x * second.x, first.y * second.y);

        /// <summary>
        /// Multiply a Vector2 by a floating-point value.
        /// </summary>
        /// <param name="first">Vector2 to multiply.</param>
        /// <param name="second">Float to multiply Vector2 components by.</param>
        /// <returns>The product of the Vector2 and float.</returns>
        public static Vector2 operator *(Vector2 first, float second) => new Vector2(first.x * second, first.y * second);

        /// <summary>
        /// Multiply a Vector2 by a floating-point value.
        /// </summary>
        /// <param name="first">Float to multiply Vector2 components by.</param>
        /// <param name="second">Vector2 to multiply.</param>
        /// <returns>The product of the Vector2 and float.</returns>
        public static Vector2 operator *(float first, Vector2 second) => new Vector2(first * second.x, first * second.y);

        /// <summary>
        /// Divide (component-wise) two Vector2s.
        /// </summary>
        /// <param name="first">Dividend.</param>
        /// <param name="second">Divisor.</param>
        /// <returns>The quotient of the two Vector2s.</returns>
        public static Vector2 operator /(Vector2 first, Vector2 second) => new Vector2(first.x / second.x, first.y / second.y);

        /// <summary>
        /// Divide a Vector2 by a floating-point value.
        /// </summary>
        /// <param name="first">Vector2 dividend.</param>
        /// <param name="second">Float divisor.</param>
        /// <returns>The quotient of the Vector2 and float.</returns>
        public static Vector2 operator /(Vector2 first, float second) => new Vector2(first.x / second, first.y / second);

        /// <summary>
        /// Determine whether two Vector2s are equal.
        /// </summary>
        /// <param name="first">First Vector2.</param>
        /// <param name="second">Second Vector2.</param>
        /// <returns>Whether or not the Vector2s are equal.</returns>
        public static bool operator ==(Vector2 first, Vector2 second) => first.AreEqual(second);

        /// <summary>
        /// Determine whether two Vector2s are not equal.
        /// </summary>
        /// <param name="first">First Vector2.</param>
        /// <param name="second">Second Vector2.</param>
        /// <returns>Whether or not the Vector2s are different.</returns>
        public static bool operator !=(Vector2 first, Vector2 second) => !first.AreEqual(second);

        /// <summary>
        /// Get a down (0, -1) vector.
        /// </summary>
        public static Vector2 down
        {
            get
            {
                return new Vector2(0, -1);
            }
        }

        /// <summary>
        /// Get an up (0, 1) vector.
        /// </summary>
        public static Vector2 up
        {
            get
            {
                return new Vector2(0, 1);
            }
        }

        /// <summary>
        /// Get a left (-1, 0) vector.
        /// </summary>
        public static Vector2 left
        {
            get
            {
                return new Vector2(-1, 0);
            }
        }

        /// <summary>
        /// Get a right (1, 0) vector.
        /// </summary>
        public static Vector2 right
        {
            get
            {
                return new Vector2(1, 0);
            }
            
        }

        /// <summary>
        /// Get a vector with both components set to 0.
        /// </summary>
        public static Vector2 zero
        {
            get
            {
                return new Vector2(0, 0);
            }
        }

        /// <summary>
        /// Get a vector with both components set to 1.
        /// </summary>
        public static Vector2 one
        {
            get
            {
                return new Vector2(1, 1);
            }
        }

        /// <summary>
        /// Get a vector with both components set to +infinity.
        /// </summary>
        public static Vector2 positiveInfinity
        {
            get
            {
                return new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            }
        }

        /// <summary>
        /// Get a vector with both components set to -infinity.
        /// </summary>
        public static Vector2 negativeInfinity
        {
            get
            {
                return new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            }
        }

        /// <summary>
        /// Get the angle between two Vector2s.
        /// </summary>
        /// <param name="vector1">First Vector2.</param>
        /// <param name="vector2">SecondVector2.</param>
        /// <returns>The angle between the two vectors.</returns>
        public static float GetAngle(Vector2 vector1, Vector2 vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2(vector1.x, vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2(vector2.x, vector2.y);
            return UnityEngine.Vector2.Angle(first, second);
        }

        /// <summary>
        /// Get a Vector2 clamped to a certain length.
        /// </summary>
        /// <param name="vector">Vector2 to clamp.</param>
        /// <param name="maxLength">Maximum length for the Vector2.</param>
        /// <returns>A Vector2 clamped to a certain length.</returns>
        public static Vector2 GetClampedVector2(Vector2 vector, float maxLength)
        {
            UnityEngine.Vector2 vec = new UnityEngine.Vector2(vector.x, vector.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.ClampMagnitude(vec, maxLength);
            return new Vector2(result.x, result.y);
        }

        /// <summary>
        /// Get the distance between two Vector2s.
        /// </summary>
        /// <param name="vector1">First Vector2.</param>
        /// <param name="vector2">Second Vector2.</param>
        /// <returns>The distance between two Vector2s.</returns>
        public static float GetDistance(Vector2 vector1, Vector2 vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2(vector1.x, vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2(vector2.x, vector2.y);
            return UnityEngine.Vector2.Distance(first, second);
        }

        /// <summary>
        /// Get the dot product between two Vector2s.
        /// </summary>
        /// <param name="leftHand">Left hand Vector2.</param>
        /// <param name="rightHand">Right hand Vector2.</param>
        /// <returns>The dot product between two Vector2s.</returns>
        public static float GetDotProduct(Vector2 leftHand, Vector2 rightHand)
        {
            UnityEngine.Vector2 l = new UnityEngine.Vector2(leftHand.x, leftHand.y);
            UnityEngine.Vector2 r = new UnityEngine.Vector2(rightHand.x, rightHand.y);
            return UnityEngine.Vector2.Dot(l, r);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector2s percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector2.</param>
        /// <param name="vector2">Second Vector2.</param>
        /// <param name="percent">Percent between first and second Vector2.</param>
        /// <returns>A linearly interpolated Vector2 between the two Vector2s.</returns>
        public static Vector2 LinearlyInterpolatePercent(Vector2 vector1, Vector2 vector2, float percent)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2(vector1.x, vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2(vector2.x, vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.Lerp(first, second, percent);
            return new Vector2(result.x, result.y);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector2s distance-wise.
        /// </summary>
        /// <param name="vector1">First Vector2.</param>
        /// <param name="vector2">Second Vector2.</param>
        /// <param name="maxDistance">Maximum distance between first and second Vector2.</param>
        /// <returns>A linearly interpolated Vector2 between the two Vector2s.</returns>
        public static Vector2 LinearlyInterpolate(Vector2 vector1, Vector2 vector2, float maxDistance)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2(vector1.x, vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2(vector2.x, vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.MoveTowards(first, second, maxDistance);
            return new Vector2(result.x, result.y);
        }

        /// <summary>
        /// Lineraly interpolate unclamped between two Vector2s percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector2.</param>
        /// <param name="vector2">Second Vector2.</param>
        /// <param name="percent">Percent between first and second Vector2.</param>
        /// <returns>A linearly interpolated unclamped Vector2 between the two Vector2s.</returns>
        public static Vector2 LinearlyInterpolatePercentUnclamped(Vector2 vector1, Vector2 vector2, float percent)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2(vector1.x, vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2(vector2.x, vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.LerpUnclamped(first, second, percent);
            return new Vector2(result.x, result.y);
        }

        /// <summary>
        /// Get smallest Vector2 using the components of two Vector2s.
        /// </summary>
        /// <param name="vector1">First Vector2.</param>
        /// <param name="vector2">Second Vector2.</param>
        /// <returns>Smallest Vector2 using the components of two Vector2s.</returns>
        public static Vector2 GetMin(Vector2 vector1, Vector2 vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2(vector1.x, vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2(vector2.x, vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.Min(first, second);
            return new Vector2(result.x, result.y);
        }

        /// <summary>
        /// Get largest Vector2 using the components of two Vector2s.
        /// </summary>
        /// <param name="vector1">First Vector2.</param>
        /// <param name="vector2">Second Vector2.</param>
        /// <returns>Largest Vector2 using the components of two Vector2s.</returns>
        public static Vector2 GetMax(Vector2 vector1, Vector2 vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2(vector1.x, vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2(vector2.x, vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.Max(first, second);
            return new Vector2(result.x, result.y);
        }

        /// <summary>
        /// Get Vector2 perpendicular to the provided one.
        /// </summary>
        /// <param name="vector">Vector2 for which to get perpendicular Vector2.</param>
        /// <returns>Vector2 perpendicular to the provided one.</returns>
        public static Vector2 GetPerpendicularVector(Vector2 vector)
        {
            UnityEngine.Vector2 vec = new UnityEngine.Vector2(vector.x, vector.y);
            UnityEngine.Vector2 res = UnityEngine.Vector2.Perpendicular(vec);
            return new Vector2(res.x, res.y);
        }

        /// <summary>
        /// Get a Vector2 that reflects the provided vector off of the provided normal line.
        /// </summary>
        /// <param name="vector">Vector2 to reflect.</param>
        /// <param name="normal">Vector2 to reflect vector off of.</param>
        /// <returns>A Vector2 that reflects the provided vector off of the provided normal line.</returns>
        public static Vector2 GetReflectedVector(Vector2 vector, Vector2 normal)
        {
            UnityEngine.Vector2 vec = new UnityEngine.Vector2(vector.x, vector.y);
            UnityEngine.Vector2 norm = new UnityEngine.Vector2(normal.x, normal.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.Reflect(vec, norm);
            return new Vector2(result.x, result.y);
        }

        /// <summary>
        /// Get a signed angle between the two Vector2s.
        /// </summary>
        /// <param name="vector1">First Vector2.</param>
        /// <param name="vector2">Second Vector2.</param>
        /// <returns>Signed angle between the two Vector2s.</returns>
        public static float GetSignedAngle(Vector2 vector1, Vector2 vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2(vector1.x, vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2(vector2.x, vector2.y);
            return UnityEngine.Vector2.SignedAngle(first, second);
        }

        /// <summary>
        /// Get a normalized (magnitude 1) version of the vector.
        /// </summary>
        /// <returns>A normalized (magnitude 1) version of the vector.</returns>
        public Vector2 GetNormalized()
        {
            float scaleFactor = 1 / magnitude;

            return new Vector2(x * scaleFactor, y * scaleFactor);
        }

        /// <summary>
        /// Normalize the Vector2.
        /// </summary>
        public void Normalize()
        {
            float scaleFactor = 1 / magnitude;

            x *= scaleFactor;
            y *= scaleFactor;
        }

        /// <summary>
        /// Determine whether or not this Vector2 equals another.
        /// </summary>
        /// <param name="other">Vector2 to compare this one with.</param>
        /// <returns>Whether or not this Vector2 equals the other.</returns>
        public bool AreEqual(Vector2 other)
        {
            if (other.x == x && other.y == y)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert this Vector2 to a string.
        /// </summary>
        /// <returns>String representation of this Vector2.</returns>
        public override string ToString()
        {
            UnityEngine.Vector2 v2 = new UnityEngine.Vector2(x, y);
            return v2.ToString();
        }

        /// <summary>
        /// Convert this Vector2 to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Vector2.</returns>
        public string ToString(string format)
        {
            UnityEngine.Vector2 v2 = new UnityEngine.Vector2(x, y);
            return v2.ToString(format);
        }

        /// <summary>
        /// Check this Vector2 for equality with another object.
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