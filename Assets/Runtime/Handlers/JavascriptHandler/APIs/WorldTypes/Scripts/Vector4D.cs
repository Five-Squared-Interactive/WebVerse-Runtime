// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a 4-dimensional double vector.
    /// </summary>
    public class Vector4D
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
        /// Z component of the vector.
        /// </summary>
        public double z;

        /// <summary>
        /// W component of the vector.
        /// </summary>
        public double w;

        /// <summary>
        /// Magnitude of the vector.
        /// </summary>
        public double magnitude
        {
            get
            {
                return Math.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
            }
        }

        /// <summary>
        /// Squared magnitude of the vector.
        /// </summary>
        public double squaredMagnitude
        {
            get
            {
                return (x * x) + (y * y) + (z * z) + (w * w);
            }
        }

        /// <summary>
        /// Constructor for a Vector4D.
        /// </summary>
        public Vector4D()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
        }

        /// <summary>
        /// Constructor for a Vector4D.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="w">W component.</param>
        public Vector4D(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Add 2 Vector4Ds.
        /// </summary>
        /// <param name="first">First Vector4D to add.</param>
        /// <param name="second">Second Vector4D to add.</param>
        /// <returns>The sum of the Vector4Ds.</returns>
        public static Vector4D operator +(Vector4D first, Vector4D second) => new Vector4D(first.x + second.x, first.y + second.y, first.z + second.z, first.w + second.w);

        /// <summary>
        /// Subtract two Vector4Ds.
        /// </summary>
        /// <param name="first">Vector4D to subtract from.</param>
        /// <param name="second">Vector4D to subtract.</param>
        /// <returns>The difference of the Vector4Ds.</returns>
        public static Vector4D operator -(Vector4D first, Vector4D second) => new Vector4D(first.x - second.x, first.y - second.y, first.z - second.z, first.w - second.w);

        /// <summary>
        /// Multiply (component-wise) two Vector4Ds.
        /// </summary>
        /// <param name="first">First Vector4D to multiply.</param>
        /// <param name="second">Second Vector4D to multiply.</param>
        /// <returns>The product of the Vector4Ds.</returns>
        public static Vector4D operator *(Vector4D first, Vector4D second) => new Vector4D(first.x * second.x, first.y * second.y, first.z * second.z, first.w * second.w);

        /// <summary>
        /// Multiply a Vector4D by a double floating-point value.
        /// </summary>
        /// <param name="first">Vector4D to multiply.</param>
        /// <param name="second">Double to multiply Vector4D components by.</param>
        /// <returns>The product of the Vector4D and double.</returns>
        public static Vector4D operator *(Vector4D first, double second) => new Vector4D(first.x * second, first.y * second, first.z * second, first.w * second);

        /// <summary>
        /// Multiply a Vector4D by a double floating-point value.
        /// </summary>
        /// <param name="first">Double to multiply Vector4D components by.</param>
        /// <param name="second">Vector4D to multiply.</param>
        /// <returns>The product of the Vector4D and double.</returns>
        public static Vector4D operator *(double first, Vector4D second) => new Vector4D(first * second.x, first * second.y, first * second.z, first * second.w);

        /// <summary>
        /// Divide (component-wise) two Vector4Ds.
        /// </summary>
        /// <param name="first">Dividend.</param>
        /// <param name="second">Divisor.</param>
        /// <returns>The quotient of the two Vector4Ds.</returns>
        public static Vector4D operator /(Vector4D first, Vector4D second) => new Vector4D(first.x / second.x, first.y / second.y, first.z / second.z, first.w / second.w);

        /// <summary>
        /// Divide a Vector4D by a double floating-point value.
        /// </summary>
        /// <param name="first">Vector4D dividend.</param>
        /// <param name="second">Double divisor.</param>
        /// <returns>The quotient of the Vector4D and double.</returns>
        public static Vector4D operator /(Vector4D first, double second) => new Vector4D(first.x / second, first.y / second, first.z / second, first.w / second);

        /// <summary>
        /// Determine whether two Vector4Ds are equal.
        /// </summary>
        /// <param name="first">First Vector4D.</param>
        /// <param name="second">Second Vector4D.</param>
        /// <returns>Whether or not the Vector4Ds are equal.</returns>
        public static bool operator ==(Vector4D first, Vector4D second) => first.AreEqual(second);

        /// <summary>
        /// Determine whether two Vector4Ds are not equal.
        /// </summary>
        /// <param name="first">First Vector4D.</param>
        /// <param name="second">Second Vector4D.</param>
        /// <returns>Whether or not the Vector4Ds are different.</returns>
        public static bool operator !=(Vector4D first, Vector4D second) => !first.AreEqual(second);

        /// <summary>
        /// Get a vector with all components set to 0.
        /// </summary>
        public static Vector4D zero
        {
            get
            {
                return new Vector4D(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Get a vector with all components set to 1.
        /// </summary>
        public static Vector4D one
        {
            get
            {
                return new Vector4D(1, 1, 1, 1);
            }
        }

        /// <summary>
        /// Get a vector with all components set to +infinity.
        /// </summary>
        public static Vector4D positiveInfinity
        {
            get
            {
                return new Vector4D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            }
        }

        /// <summary>
        /// Get a vector with all components set to -infinity.
        /// </summary>
        public static Vector4D negativeInfinity
        {
            get
            {
                return new Vector4D(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            }
        }

        /// <summary>
        /// Get the distance between two Vector4Ds.
        /// </summary>
        /// <param name="vector1">First Vector4D.</param>
        /// <param name="vector2">Second Vector4D.</param>
        /// <returns>The distance between two Vector4Ds.</returns>
        public static double GetDistance(Vector4D vector1, Vector4D vector2)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4((float) vector1.x, (float) vector1.y, (float) vector1.z, (float) vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4((float) vector2.x, (float) vector2.y, (float) vector2.z, (float) vector2.w);
            return UnityEngine.Vector4.Distance(first, second);
        }

        /// <summary>
        /// Get the dot product between two Vector4Ds.
        /// </summary>
        /// <param name="leftHand">Left hand Vector4D.</param>
        /// <param name="rightHand">Right hand Vector4D.</param>
        /// <returns>The dot product between two Vector4Ds.</returns>
        public static double GetDotProduct(Vector4D leftHand, Vector4D rightHand)
        {
            UnityEngine.Vector4 l = new UnityEngine.Vector4((float) leftHand.x, (float) leftHand.y, (float) leftHand.z, (float) leftHand.w);
            UnityEngine.Vector4 r = new UnityEngine.Vector4((float) rightHand.x, (float) rightHand.y, (float) rightHand.z, (float) rightHand.w);
            return UnityEngine.Vector4.Dot(l, r);
        }

        /// <summary>
        /// Get the result of projecting a vector on a normal.
        /// </summary>
        /// <param name="vector">Vector to project.</param>
        /// <param name="normal">Normal to project vector on.</param>
        /// <returns>The result of projecting a vector on a normal.</returns>
        public static Vector4D Project(Vector4D vector, Vector4D normal)
        {
            UnityEngine.Vector4 vec = new UnityEngine.Vector4((float) vector.x, (float) vector.y, (float) vector.z, (float) vector.w);
            UnityEngine.Vector4 norm = new UnityEngine.Vector4((float) normal.x, (float) normal.y, (float) normal.z, (float) normal.w);
            UnityEngine.Vector4 res = UnityEngine.Vector4.Project(vec, norm);
            return new Vector4D(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector4Ds percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector4D.</param>
        /// <param name="vector2">Second Vector4D.</param>
        /// <param name="percent">Percent between first and second Vector4D.</param>
        /// <returns>A linearly interpolated Vector4D between the two Vector4Ds.</returns>
        public static Vector4D LinearlyInterpolatePercent(Vector4D vector1, Vector4D vector2, double percent)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4((float) vector1.x, (float) vector1.y, (float) vector1.z, (float) vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4((float) vector2.x, (float) vector2.y, (float) vector2.z, (float) vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.Lerp(first, second, (float) percent);
            return new Vector4D(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector4Ds distance-wise.
        /// </summary>
        /// <param name="vector1">First Vector4D.</param>
        /// <param name="vector2">Second Vector4D.</param>
        /// <param name="maxDistance">Maximum distance between first and second Vector4D.</param>
        /// <returns>A linearly interpolated Vector4D between the two Vector4Ds.</returns>
        public static Vector4D LinearlyInterpolate(Vector4D vector1, Vector4D vector2, double maxDistance)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4((float) vector1.x, (float) vector1.y, (float) vector1.z, (float) vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4((float) vector2.x, (float) vector2.y, (float) vector2.z, (float) vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.MoveTowards(first, second, (float) maxDistance);
            return new Vector4D(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Lineraly interpolate unclamped between two Vector4Ds percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector4D.</param>
        /// <param name="vector2">Second Vector4D.</param>
        /// <param name="percent">Percent between first and second Vector4D.</param>
        /// <returns>A linearly interpolated unclamped Vector4D between the two Vector4Ds.</returns>
        public static Vector4D LinearlyInterpolatePercentUnclamped(Vector4D vector1, Vector4D vector2, double percent)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4((float) vector1.x, (float) vector1.y, (float) vector1.z, (float) vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4((float) vector2.x, (float) vector2.y, (float) vector2.z, (float) vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.LerpUnclamped(first, second, (float) percent);
            return new Vector4D(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Get smallest Vector4D using the components of two Vector4Ds.
        /// </summary>
        /// <param name="vector1">First Vector4D.</param>
        /// <param name="vector2">Second Vector4D.</param>
        /// <returns>Smallest Vector4D using the components of two Vector4Ds.</returns>
        public static Vector4D GetMin(Vector4D vector1, Vector4D vector2)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4((float) vector1.x, (float) vector1.y, (float) vector1.z, (float) vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4((float) vector2.x, (float) vector2.y, (float) vector2.z, (float) vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.Min(first, second);
            return new Vector4D(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Get largest Vector4D using the components of two Vector4Ds.
        /// </summary>
        /// <param name="vector1">First Vector4D.</param>
        /// <param name="vector2">Second Vector4D.</param>
        /// <returns>Largest Vector4D using the components of two Vector4Ds.</returns>
        public static Vector4D GetMax(Vector4D vector1, Vector4D vector2)
        {
            UnityEngine.Vector4 first = new UnityEngine.Vector4((float) vector1.x, (float) vector1.y, (float) vector1.z, (float) vector1.w);
            UnityEngine.Vector4 second = new UnityEngine.Vector4((float) vector2.x, (float) vector2.y, (float) vector2.z, (float) vector2.w);
            UnityEngine.Vector4 result = UnityEngine.Vector4.Max(first, second);
            return new Vector4D(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Get a normalized (magnitude 1) version of the vector.
        /// </summary>
        /// <returns>A normalized (magnitude 1) version of the vector.</returns>
        public Vector4D GetNormalized()
        {
            double scaleFactor = 1 / magnitude;

            return new Vector4D(x * scaleFactor, y * scaleFactor, z * scaleFactor, w * scaleFactor);
        }

        /// <summary>
        /// Normalize the Vector4D.
        /// </summary>
        public void Normalize()
        {
            double scaleFactor = 1 / magnitude;

            x *= scaleFactor;
            y *= scaleFactor;
            z *= scaleFactor;
        }

        /// <summary>
        /// Determine whether or not this Vector4D equals another.
        /// </summary>
        /// <param name="other">Vector4D to compare this one with.</param>
        /// <returns>Whether or not this Vector4D equals the other.</returns>
        public bool AreEqual(Vector4D other)
        {
            if (other.x == x && other.y == y && other.z == z)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert this Vector4D to a string.
        /// </summary>
        /// <returns>String representation of this Vector4D.</returns>
        public override string ToString()
        {
            UnityEngine.Vector4 v2 = new UnityEngine.Vector4((float) x, (float) y, (float) z);
            return v2.ToString();
        }

        /// <summary>
        /// Convert this Vector4D to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Vector4D.</returns>
        public string ToString(string format)
        {
            UnityEngine.Vector4 v2 = new UnityEngine.Vector4((float) x, (float) y, (float) z);
            return v2.ToString(format);
        }

        /// <summary>
        /// Check this Vector4D for equality with another object.
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