// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a 2-dimensional double vector.
    /// </summary>
    public class Vector2D
    {
        /// <summary>
        /// X component of the vector.
        /// </summary>
        public double x;

        /// <summary>
        /// Y component of the vector.
        /// </summary>
        public double y;

        /// <summary>
        /// Magnitude of the vector.
        /// </summary>
        public double magnitude
        {
            get
            {
                return Math.Sqrt((x * x) + (y * y));
            }
        }

        /// <summary>
        /// Squared magnitude of the vector.
        /// </summary>
        public double squaredMagnitude
        {
            get
            {
                return (x * x) + (y * y);
            }
        }

        /// <summary>
        /// Constructor for a Vector2D.
        /// </summary>
        public Vector2D()
        {
            x = 0;
            y = 0;
        }

        /// <summary>
        /// Constructor for a Vector2D.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        public Vector2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Add 2 Vector2s.
        /// </summary>
        /// <param name="first">First Vector2D to add.</param>
        /// <param name="second">Second Vector2D to add.</param>
        /// <returns>The sum of the Vector2Ds.</returns>
        public static Vector2D operator +(Vector2D first, Vector2D second) => new Vector2D(first.x + second.x, first.y + second.y);

        /// <summary>
        /// Subtract two Vector2Ds.
        /// </summary>
        /// <param name="first">Vector2D to subtract from.</param>
        /// <param name="second">Vector2D to subtract.</param>
        /// <returns>The difference of the Vector2Ds.</returns>
        public static Vector2D operator -(Vector2D first, Vector2D second) => new Vector2D(first.x - second.x, first.y - second.y);

        /// <summary>
        /// Multiply (component-wise) two Vector2Ds.
        /// </summary>
        /// <param name="first">First Vector2D to multiply.</param>
        /// <param name="second">Second Vector2D to multiply.</param>
        /// <returns>The product of the Vector2Ds.</returns>
        public static Vector2D operator *(Vector2D first, Vector2D second) => new Vector2D(first.x * second.x, first.y * second.y);

        /// <summary>
        /// Multiply a Vector2D by a double floating-point value.
        /// </summary>
        /// <param name="first">Vector2D to multiply.</param>
        /// <param name="second">Float to multiply Vector2D components by.</param>
        /// <returns>The product of the Vector2D and float.</returns>
        public static Vector2D operator *(Vector2D first, double second) => new Vector2D(first.x * second, first.y * second);

        /// <summary>
        /// Multiply a Vector2D by a double floating-point value.
        /// </summary>
        /// <param name="first">Double to multiply Vector2D components by.</param>
        /// <param name="second">Vector2D to multiply.</param>
        /// <returns>The product of the Vector2D and double.</returns>
        public static Vector2D operator *(double first, Vector2D second) => new Vector2D(first * second.x, first * second.y);

        /// <summary>
        /// Divide (component-wise) two Vector2Ds.
        /// </summary>
        /// <param name="first">Dividend.</param>
        /// <param name="second">Divisor.</param>
        /// <returns>The quotient of the two Vector2Ds.</returns>
        public static Vector2D operator /(Vector2D first, Vector2D second) => new Vector2D(first.x / second.x, first.y / second.y);

        /// <summary>
        /// Divide a Vector2D by a double floating-point value.
        /// </summary>
        /// <param name="first">Vector2D dividend.</param>
        /// <param name="second">Float divisor.</param>
        /// <returns>The quotient of the Vector2D and double.</returns>
        public static Vector2D operator /(Vector2D first, double second) => new Vector2D(first.x / second, first.y / second);

        /// <summary>
        /// Determine whether two Vector2Ds are equal.
        /// </summary>
        /// <param name="first">First Vector2D.</param>
        /// <param name="second">Second Vector2D.</param>
        /// <returns>Whether or not the Vector2Ds are equal.</returns>
        public static bool operator ==(Vector2D first, Vector2D second) => first.AreEqual(second);

        /// <summary>
        /// Determine whether two Vector2Ds are not equal.
        /// </summary>
        /// <param name="first">First Vector2D.</param>
        /// <param name="second">Second Vector2D.</param>
        /// <returns>Whether or not the Vector2Ds are different.</returns>
        public static bool operator !=(Vector2D first, Vector2D second) => !first.AreEqual(second);

        /// <summary>
        /// Get a down (0, -1) vector.
        /// </summary>
        public static Vector2D down
        {
            get
            {
                return new Vector2D(0, -1);
            }
        }

        /// <summary>
        /// Get an up (0, 1) vector.
        /// </summary>
        public static Vector2D up
        {
            get
            {
                return new Vector2D(0, 1);
            }
        }

        /// <summary>
        /// Get a left (-1, 0) vector.
        /// </summary>
        public static Vector2D left
        {
            get
            {
                return new Vector2D(-1, 0);
            }
        }

        /// <summary>
        /// Get a right (1, 0) vector.
        /// </summary>
        public static Vector2D right
        {
            get
            {
                return new Vector2D(1, 0);
            }
        }

        /// <summary>
        /// Get a vector with both components set to 0.
        /// </summary>
        public static Vector2D zero
        {
            get
            {
                return new Vector2D(0, 0);
            }
        }

        /// <summary>
        /// Get a vector with both components set to 1.
        /// </summary>
        public static Vector2D one
        {
            get
            {
                return new Vector2D(1, 1);
            }
        }

        /// <summary>
        /// Get a vector with both components set to +infinity.
        /// </summary>
        public static Vector2D positiveInfinity
        {
            get
            {
                return new Vector2D(double.PositiveInfinity, double.PositiveInfinity);
            }
        }

        /// <summary>
        /// Get a vector with both components set to -infinity.
        /// </summary>
        public static Vector2D negativeInfinity
        {
            get
            {
                 return new Vector2D(double.NegativeInfinity, double.NegativeInfinity);
            }
        }

        /// <summary>
        /// Get the angle between two Vector2Ds.
        /// </summary>
        /// <param name="vector1">First Vector2D.</param>
        /// <param name="vector2">SecondVector2D.</param>
        /// <returns>The angle between the two vectors.</returns>
        public static float GetAngle(Vector2D vector1, Vector2D vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2((float) vector1.x, (float) vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2((float) vector2.x, (float) vector2.y);
            return UnityEngine.Vector2.Angle(first, second);
        }

        /// <summary>
        /// Get a Vector2D clamped to a certain length.
        /// </summary>
        /// <param name="vector">Vector2D to clamp.</param>
        /// <param name="maxLength">Maximum length for the Vector2D.</param>
        /// <returns>A Vector2D clamped to a certain length.</returns>
        public static Vector2D GetClampedVector2D(Vector2D vector, float maxLength)
        {
            UnityEngine.Vector2 vec = new UnityEngine.Vector2((float) vector.x, (float) vector.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.ClampMagnitude(vec, maxLength);
            return new Vector2D(result.x, result.y);
        }

        /// <summary>
        /// Get the distance between two Vector2Ds.
        /// </summary>
        /// <param name="vector1">First Vector2D.</param>
        /// <param name="vector2">Second Vector2D.</param>
        /// <returns>The distance between two Vector2Ds.</returns>
        public static float GetDistance(Vector2D vector1, Vector2D vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2((float) vector1.x, (float) vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2((float) vector2.x, (float) vector2.y);
            return UnityEngine.Vector2.Distance(first, second);
        }

        /// <summary>
        /// Get the dot product between two Vector2Ds.
        /// </summary>
        /// <param name="leftHand">Left hand Vector2D.</param>
        /// <param name="rightHand">Right hand Vector2D.</param>
        /// <returns>The dot product between two Vector2Ds.</returns>
        public static float GetDotProduct(Vector2D leftHand, Vector2D rightHand)
        {
            UnityEngine.Vector2 l = new UnityEngine.Vector2((float) leftHand.x, (float) leftHand.y);
            UnityEngine.Vector2 r = new UnityEngine.Vector2((float) rightHand.x, (float) rightHand.y);
            return UnityEngine.Vector2.Dot(l, r);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector2Ds percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector2D.</param>
        /// <param name="vector2">Second Vector2D.</param>
        /// <param name="percent">Percent between first and second Vector2D.</param>
        /// <returns>A linearly interpolated Vector2D between the two Vector2Ds.</returns>
        public static Vector2D LinearlyInterpolatePercent(Vector2D vector1, Vector2D vector2, float percent)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2((float) vector1.x, (float) vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2((float) vector2.x, (float) vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.Lerp(first, second, percent);
            return new Vector2D(result.x, result.y);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector2Ds distance-wise.
        /// </summary>
        /// <param name="vector1">First Vector2D.</param>
        /// <param name="vector2">Second Vector2D.</param>
        /// <param name="maxDistance">Maximum distance between first and second Vector2D.</param>
        /// <returns>A linearly interpolated Vector2D between the two Vector2Ds.</returns>
        public static Vector2D LinearlyInterpolate(Vector2D vector1, Vector2D vector2, float maxDistance)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2((float) vector1.x, (float) vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2((float) vector2.x, (float) vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.MoveTowards(first, second, maxDistance);
            return new Vector2D(result.x, result.y);
        }

        /// <summary>
        /// Lineraly interpolate unclamped between two Vector2Ds percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector2D.</param>
        /// <param name="vector2">Second Vector2D.</param>
        /// <param name="percent">Percent between first and second Vector2D.</param>
        /// <returns>A linearly interpolated unclamped Vector2D between the two Vector2Ds.</returns>
        public static Vector2D LinearlyInterpolatePercentUnclamped(Vector2D vector1, Vector2D vector2, float percent)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2((float) vector1.x, (float) vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2((float) vector2.x, (float) vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.LerpUnclamped(first, second, percent);
            return new Vector2D(result.x, result.y);
        }

        /// <summary>
        /// Get smallest Vector2D using the components of two Vector2Ds.
        /// </summary>
        /// <param name="vector1">First Vector2D.</param>
        /// <param name="vector2">Second Vector2D.</param>
        /// <returns>Smallest Vector2D using the components of two Vector2Ds.</returns>
        public static Vector2D GetMin(Vector2D vector1, Vector2D vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2((float) vector1.x, (float) vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2((float) vector2.x, (float) vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.Min(first, second);
            return new Vector2D(result.x, result.y);
        }

        /// <summary>
        /// Get largest Vector2D using the components of two Vector2Ds.
        /// </summary>
        /// <param name="vector1">First Vector2D.</param>
        /// <param name="vector2">Second Vector2D.</param>
        /// <returns>Largest Vector2D using the components of two Vector2Ds.</returns>
        public static Vector2D GetMax(Vector2D vector1, Vector2D vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2((float) vector1.x, (float) vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2((float) vector2.x, (float) vector2.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.Max(first, second);
            return new Vector2D(result.x, result.y);
        }

        /// <summary>
        /// Get Vector2D perpendicular to the provided one.
        /// </summary>
        /// <param name="vector">Vector2D for which to get perpendicular Vector2D.</param>
        /// <returns>Vector2D perpendicular to the provided one.</returns>
        public static Vector2D GetPerpendicularVector(Vector2D vector)
        {
            UnityEngine.Vector2 vec = new UnityEngine.Vector2((float) vector.x, (float) vector.y);
            UnityEngine.Vector2 res = UnityEngine.Vector2.Perpendicular(vec);
            return new Vector2D(res.x, res.y);
        }

        /// <summary>
        /// Get a Vector2D that reflects the provided vector off of the provided normal line.
        /// </summary>
        /// <param name="vector">Vector2D to reflect.</param>
        /// <param name="normal">Vector2D to reflect vector off of.</param>
        /// <returns>A Vector2D that reflects the provided vector off of the provided normal line.</returns>
        public static Vector2D GetReflectedVector(Vector2D vector, Vector2D normal)
        {
            UnityEngine.Vector2 vec = new UnityEngine.Vector2((float) vector.x, (float) vector.y);
            UnityEngine.Vector2 norm = new UnityEngine.Vector2((float) normal.x, (float) normal.y);
            UnityEngine.Vector2 result = UnityEngine.Vector2.Reflect(vec, norm);
            return new Vector2D(result.x, result.y);
        }

        /// <summary>
        /// Get a signed angle between the two Vector2Ds.
        /// </summary>
        /// <param name="vector1">First Vector2D.</param>
        /// <param name="vector2">Second Vector2D.</param>
        /// <returns>Signed angle between the two Vector2Ds.</returns>
        public static float GetSignedAngle(Vector2D vector1, Vector2D vector2)
        {
            UnityEngine.Vector2 first = new UnityEngine.Vector2((float) vector1.x, (float) vector1.y);
            UnityEngine.Vector2 second = new UnityEngine.Vector2((float) vector2.x, (float) vector2.y);
            return UnityEngine.Vector2.SignedAngle(first, second);
        }

        /// <summary>
        /// Get a normalized (magnitude 1) version of the vector.
        /// </summary>
        /// <returns>A normalized (magnitude 1) version of the vector.</returns>
        public Vector2D GetNormalized()
        {
            double scaleFactor = 1 / magnitude;

            return new Vector2D(x * scaleFactor, y * scaleFactor);
        }

        /// <summary>
        /// Normalize the Vector2D.
        /// </summary>
        public void Normalize()
        {
            double scaleFactor = 1 / magnitude;

            x *= scaleFactor;
            y *= scaleFactor;
        }

        /// <summary>
        /// Determine whether or not this Vector2D equals another.
        /// </summary>
        /// <param name="other">Vector2D to compare this one with.</param>
        /// <returns>Whether or not this Vector2D equals the other.</returns>
        public bool AreEqual(Vector2D other)
        {
            if (other.x == x && other.y == y)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert this Vector2D to a string.
        /// </summary>
        /// <returns>String representation of this Vector2D.</returns>
        public override string ToString()
        {
            UnityEngine.Vector2 v2 = new UnityEngine.Vector2((float) x, (float) y);
            return v2.ToString();
        }

        /// <summary>
        /// Convert this Vector2D to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Vector2D.</returns>
        public string ToString(string format)
        {
            UnityEngine.Vector2 v2 = new UnityEngine.Vector2((float) x, (float) y);
            return v2.ToString(format);
        }

        /// <summary>
        /// Check this Vector2D for equality with another object.
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