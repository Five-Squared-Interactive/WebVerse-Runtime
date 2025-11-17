// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a 3-dimensional double vector.
    /// </summary>
    public class Vector3D
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
        /// Magnitude of the vector.
        /// </summary>
        public double magnitude
        {
            get
            {
                return Math.Sqrt((x * x) + (y * y) + (z * z));
            }
        }

        /// <summary>
        /// Squared magnitude of the vector.
        /// </summary>
        public double squaredMagnitude
        {
            get
            {
                return (x * x) + (y * y) + (z * z);
            }
        }

        /// <summary>
        /// Constructor for a Vector3D.
        /// </summary>
        public Vector3D()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        /// <summary>
        /// Constructor for a Vector3D.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vector3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Add 2 Vector3Ds.
        /// </summary>
        /// <param name="first">First Vector3D to add.</param>
        /// <param name="second">Second Vector3D to add.</param>
        /// <returns>The sum of the Vector3Ds.</returns>
        public static Vector3D operator +(Vector3D first, Vector3D second) => new Vector3D(first.x + second.x, first.y + second.y, first.z + second.z);

        /// <summary>
        /// Subtract two Vector3Ds.
        /// </summary>
        /// <param name="first">Vector3D to subtract from.</param>
        /// <param name="second">Vector3D to subtract.</param>
        /// <returns>The difference of the Vector3Ds.</returns>
        public static Vector3D operator -(Vector3D first, Vector3D second) => new Vector3D(first.x - second.x, first.y - second.y, first.z - second.z);

        /// <summary>
        /// Multiply (component-wise) two Vector3Ds.
        /// </summary>
        /// <param name="first">First Vector3D to multiply.</param>
        /// <param name="second">Second Vector3D to multiply.</param>
        /// <returns>The product of the Vector3Ds.</returns>
        public static Vector3D operator *(Vector3D first, Vector3D second) => new Vector3D(first.x * second.x, first.y * second.y, first.z * second.z);

        /// <summary>
        /// Multiply a Vector3D by a double floating-point value.
        /// </summary>
        /// <param name="first">Vector3D to multiply.</param>
        /// <param name="second">Double to multiply Vector3D components by.</param>
        /// <returns>The product of the Vector3D and double.</returns>
        public static Vector3D operator *(Vector3D first, double second) => new Vector3D(first.x * second, first.y * second, first.z * second);

        /// <summary>
        /// Multiply a Vector3D by a double floating-point value.
        /// </summary>
        /// <param name="first">Double to multiply Vector3D components by.</param>
        /// <param name="second">Vector3D to multiply.</param>
        /// <returns>The product of the Vector3D and double.</returns>
        public static Vector3D operator *(double first, Vector3D second) => new Vector3D(first * second.x, first * second.y, first * second.z);

        /// <summary>
        /// Divide (component-wise) two Vector3Ds.
        /// </summary>
        /// <param name="first">Dividend.</param>
        /// <param name="second">Divisor.</param>
        /// <returns>The quotient of the two Vector3Ds.</returns>
        public static Vector3D operator /(Vector3D first, Vector3D second) => new Vector3D(first.x / second.x, first.y / second.y, first.z / second.z);

        /// <summary>
        /// Divide a Vector3D by a double floating-point value.
        /// </summary>
        /// <param name="first">Vector3D dividend.</param>
        /// <param name="second">Double divisor.</param>
        /// <returns>The quotient of the Vector3D and double.</returns>
        public static Vector3D operator /(Vector3D first, double second) => new Vector3D(first.x / second, first.y / second, first.z / second);

        /// <summary>
        /// Determine whether two Vector3Ds are equal.
        /// </summary>
        /// <param name="first">First Vector3D.</param>
        /// <param name="second">Second Vector3D.</param>
        /// <returns>Whether or not the Vector3Ds are equal.</returns>
        public static bool operator ==(Vector3D first, Vector3D second) => first.AreEqual(second);

        /// <summary>
        /// Determine whether two Vector3Ds are not equal.
        /// </summary>
        /// <param name="first">First Vector3D.</param>
        /// <param name="second">Second Vector3D.</param>
        /// <returns>Whether or not the Vector3Ds are different.</returns>
        public static bool operator !=(Vector3D first, Vector3D second) => !first.AreEqual(second);

        /// <summary>
        /// Get a forward (0, 0, 1) vector.
        /// </summary>
        public static Vector3D forward
        {
            get
            {
                return new Vector3D(0, 0, 1);
            }
        }

        /// <summary>
        /// Get a back (0, 0, -1) vector.
        /// </summary>
        public static Vector3D back
        {
            get
            {
                return new Vector3D(0, 0, -1);
            }
        }

        /// <summary>
        /// Get a down (0, -1, 0) vector.
        /// </summary>
        public static Vector3D down
        {
            get
            {
                return new Vector3D(0, -1, 0);
            }
        }

        /// <summary>
        /// Get an up (0, 1, 0) vector.
        /// </summary>
        public static Vector3D up
        {
            get
            {
                return new Vector3D(0, 1, 0);
            }
        }

        /// <summary>
        /// Get a left (-1, 0, 0) vector.
        /// </summary>
        public static Vector3D left
        {
            get
            {
                return new Vector3D(-1, 0, 0);
            }
        }

        /// <summary>
        /// Get a right (1, 0, 0) vector.
        /// </summary>
        public static Vector3D right
        {
            get
            {
                return new Vector3D(1, 0, 0);
            }
        }

        /// <summary>
        /// Get a vector with all components set to 0.
        /// </summary>
        public static Vector3D zero
        {
            get
            {
                return new Vector3D(0, 0, 0);
            }
        }

        /// <summary>
        /// Get a vector with all components set to 1.
        /// </summary>
        public static Vector3D one
        {
            get
            {
                return new Vector3D(1, 1, 1);
            }
        }

        /// <summary>
        /// Get a vector with all components set to +infinity.
        /// </summary>
        public static Vector3D positiveInfinity
        {
            get
            {
                return new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            }
        }

        /// <summary>
        /// Get a vector with all components set to -infinity.
        /// </summary>
        public static Vector3D negativeInfinity
        {
            get
            {
                return new Vector3D(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            }
        }

        /// <summary>
        /// Get the angle between two Vector3Ds.
        /// </summary>
        /// <param name="vector1">First Vector3D.</param>
        /// <param name="vector2">Second Vector3D.</param>
        /// <returns>The angle between the two vectors.</returns>
        public static double GetAngle(Vector3D vector1, Vector3D vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3((float) vector1.x, (float) vector1.y, (float) vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3((float) vector2.x, (float) vector2.y, (float) vector2.z);
            return UnityEngine.Vector3.Angle(first, second);
        }

        /// <summary>
        /// Get a Vector3D clamped to a certain length.
        /// </summary>
        /// <param name="vector">Vector3D to clamp.</param>
        /// <param name="maxLength">Maximum length for the Vector3D.</param>
        /// <returns>A Vector3D clamped to a certain length.</returns>
        public static Vector3D GetClampedVector3D(Vector3D vector, double maxLength)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3((float) vector.x, (float) vector.y, (float) vector.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.ClampMagnitude(vec, (float) maxLength);
            return new Vector3D(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get the distance between two Vector3Ds.
        /// </summary>
        /// <param name="vector1">First Vector3D.</param>
        /// <param name="vector2">Second Vector3D.</param>
        /// <returns>The distance between two Vector3Ds.</returns>
        public static double GetDistance(Vector3D vector1, Vector3D vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3((float) vector1.x, (float) vector1.y, (float) vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3((float) vector2.x, (float) vector2.y, (float) vector2.z);
            return UnityEngine.Vector3.Distance(first, second);
        }

        /// <summary>
        /// Get the dot product between two Vector3Ds.
        /// </summary>
        /// <param name="leftHand">Left hand Vector3D.</param>
        /// <param name="rightHand">Right hand Vector3D.</param>
        /// <returns>The dot product between two Vector3Ds.</returns>
        public static double GetDotProduct(Vector3D leftHand, Vector3D rightHand)
        {
            UnityEngine.Vector3 l = new UnityEngine.Vector3((float) leftHand.x, (float) leftHand.y, (float) leftHand.z);
            UnityEngine.Vector3 r = new UnityEngine.Vector3((float) rightHand.x, (float) rightHand.y, (float) rightHand.z);
            return UnityEngine.Vector3.Dot(l, r);
        }

        /// <summary>
        /// Get the cross product between two Vector3Ds.
        /// </summary>
        /// <param name="leftHand">Left hand Vector3D.</param>
        /// <param name="rightHand">Right hand Vector3D.</param>
        /// <returns>The cross product between two Vector3Ds.</returns>
        public static Vector3D GetCrossProduct(Vector3D leftHand, Vector3D rightHand)
        {
            UnityEngine.Vector3 l = new UnityEngine.Vector3((float) leftHand.x, (float) leftHand.y, (float) leftHand.z);
            UnityEngine.Vector3 r = new UnityEngine.Vector3((float) rightHand.x, (float) rightHand.y, (float) rightHand.z);
            UnityEngine.Vector3 res = UnityEngine.Vector3.Cross(l, r);
            return new Vector3D(res.x, res.y, res.z);
        }

        /// <summary>
        /// Get the result of projecting a vector on a normal.
        /// </summary>
        /// <param name="vector">Vector to project.</param>
        /// <param name="normal">Normal to project vector on.</param>
        /// <returns>The result of projecting a vector on a normal.</returns>
        public static Vector3D Project(Vector3D vector, Vector3D normal)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3((float) vector.x, (float) vector.y, (float) vector.z);
            UnityEngine.Vector3 norm = new UnityEngine.Vector3((float) normal.x, (float) normal.y, (float) normal.z);
            UnityEngine.Vector3 res = UnityEngine.Vector3.Project(vec, norm);
            return new Vector3D(res.x, res.y, res.z);
        }

        /// <summary>
        /// Get the result of projecting a vector on a plane.
        /// </summary>
        /// <param name="vector">Vector to project.</param>
        /// <param name="normal">Normal of plane to project vector on.</param>
        /// <returns>The result of projecting a vector on a plane.</returns>
        public static Vector3D ProjectOnPlane(Vector3D vector, Vector3D normalOfPlane)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3((float) vector.x, (float) vector.y, (float) vector.z);
            UnityEngine.Vector3 norm = new UnityEngine.Vector3((float) normalOfPlane.x, (float) normalOfPlane.y, (float) normalOfPlane.z);
            UnityEngine.Vector3 res = UnityEngine.Vector3.ProjectOnPlane(vec, norm);
            return new Vector3D(res.x, res.y, res.z);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector3Ds percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector3D.</param>
        /// <param name="Vector3D">Second Vector3D.</param>
        /// <param name="percent">Percent between first and second Vector3D.</param>
        /// <returns>A linearly interpolated Vector3D between the two Vector3Ds.</returns>
        public static Vector3D LinearlyInterpolatePercent(Vector3D vector1, Vector3D vector2, double percent)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3((float) vector1.x, (float) vector1.y, (float) vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3((float) vector2.x, (float) vector2.y, (float) vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.Lerp(first, second, (float) percent);
            return new Vector3D(result.x, result.y, result.z);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector3Ds distance-wise.
        /// </summary>
        /// <param name="vector1">First Vector3D.</param>
        /// <param name="Vector3D">Second Vector3D.</param>
        /// <param name="maxDistance">Maximum distance between first and second Vector3D.</param>
        /// <returns>A linearly interpolated Vector3D between the two Vector3Ds.</returns>
        public static Vector3D LinearlyInterpolate(Vector3D vector1, Vector3D vector2, double maxDistance)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3((float) vector1.x, (float) vector1.y, (float) vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3((float) vector2.x, (float) vector2.y, (float) vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.MoveTowards(first, second, (float) maxDistance);
            return new Vector3D(result.x, result.y, result.z);
        }

        /// <summary>
        /// Lineraly interpolate unclamped between two Vector3Ds percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector3D.</param>
        /// <param name="vector2">Second Vector3D.</param>
        /// <param name="percent">Percent between first and second Vector3D.</param>
        /// <returns>A linearly interpolated unclamped Vector3D between the two Vector3Ds.</returns>
        public static Vector3D LinearlyInterpolatePercentUnclamped(Vector3D vector1, Vector3D vector2, double percent)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3((float) vector1.x, (float) vector1.y, (float) vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3((float) vector2.x, (float) vector2.y, (float) vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.LerpUnclamped(first, second, (float) percent);
            return new Vector3D(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get smallest Vector3D using the components of two Vector3Ds.
        /// </summary>
        /// <param name="vector1">First Vector3D.</param>
        /// <param name="Vector3D">Second Vector3D.</param>
        /// <returns>Smallest Vector3D using the components of two Vector3Ds.</returns>
        public static Vector3D GetMin(Vector3D vector1, Vector3D vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3((float) vector1.x, (float) vector1.y, (float) vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3((float) vector2.x, (float) vector2.y, (float) vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.Min(first, second);
            return new Vector3D(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get largest Vector3D using the components of two Vector3Ds.
        /// </summary>
        /// <param name="vector1">First Vector3D.</param>
        /// <param name="Vector3D">Second Vector3D.</param>
        /// <returns>Largest Vector3D using the components of two Vector3Ds.</returns>
        public static Vector3D GetMax(Vector3D vector1, Vector3D vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3((float) vector1.x, (float) vector1.y, (float) vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3((float) vector2.x, (float) vector2.y, (float) vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.Max(first, second);
            return new Vector3D(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get a Vector3D that reflects the provided vector off of the provided normal line.
        /// </summary>
        /// <param name="vector">Vector3D to reflect.</param>
        /// <param name="normal">Vector3D to reflect vector off of.</param>
        /// <returns>A Vector3D that reflects the provided vector off of the provided normal line.</returns>
        public static Vector3D GetReflectedVector(Vector3D vector, Vector3D normal)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3((float) vector.x, (float) vector.y, (float) vector.z);
            UnityEngine.Vector3 norm = new UnityEngine.Vector3((float) normal.x, (float) normal.y, (float) normal.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.Reflect(vec, norm);
            return new Vector3D(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get a signed angle between the two Vector3Ds.
        /// </summary>
        /// <param name="vector1">First Vector3D.</param>
        /// <param name="vector2">Second Vector3D.</param>
        /// <returns>Signed angle between the two Vector3Ds.</returns>
        public static double GetSignedAngle(Vector3D vector1, Vector3D vector2, Vector3D axis)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3((float) vector1.x, (float) vector1.y, (float) vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3((float) vector2.x, (float) vector2.y, (float) vector2.z);
            UnityEngine.Vector3 ax = new UnityEngine.Vector3((float) axis.x, (float) axis.y, (float) axis.z);
            return UnityEngine.Vector3.SignedAngle(first, second, ax);
        }

        /// <summary>
        /// Get a normalized (magnitude 1) version of the vector.
        /// </summary>
        /// <returns>A normalized (magnitude 1) version of the vector.</returns>
        public Vector3D GetNormalized()
        {
            double scaleFactor = 1 / magnitude;

            return new Vector3D(x * scaleFactor, y * scaleFactor, z * scaleFactor);
        }

        /// <summary>
        /// Get normalized (magnitude 1) and orthagonal versions of the vectors.
        /// </summary>
        /// <returns>A normalized (magnitude 1) and orthagonal version of the vectors.</returns>
        public Tuple<Vector3D, Vector3D> GetNormalizedAndOrthagonal(Vector3D vector1, Vector3D vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3((float) vector1.x, (float) vector1.y, (float) vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3((float) vector2.x, (float) vector2.y, (float) vector2.z);
            UnityEngine.Vector3.OrthoNormalize(ref first, ref second);
            return new Tuple<Vector3D, Vector3D>(new Vector3D(first.x, first.y, first.z), new Vector3D(second.x, second.y, second.z));
        }

        /// <summary>
        /// Normalize the Vector3D.
        /// </summary>
        public void Normalize()
        {
            double scaleFactor = 1 / magnitude;

            x *= scaleFactor;
            y *= scaleFactor;
            z *= scaleFactor;
        }

        /// <summary>
        /// Determine whether or not this Vector3D equals another.
        /// </summary>
        /// <param name="other">Vector3D to compare this one with.</param>
        /// <returns>Whether or not this Vector3D equals the other.</returns>
        public bool AreEqual(Vector3D other)
        {
            if (other.x == x && other.y == y && other.z == z)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert this Vector3D to a string.
        /// </summary>
        /// <returns>String representation of this Vector3D.</returns>
        public override string ToString()
        {
            UnityEngine.Vector3 v2 = new UnityEngine.Vector3((float) x, (float) y, (float) z);
            return v2.ToString();
        }

        /// <summary>
        /// Convert this Vector3D to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Vector3D.</returns>
        public string ToString(string format)
        {
            UnityEngine.Vector3 v2 = new UnityEngine.Vector3((float) x, (float) y, (float) z);
            return v2.ToString(format);
        }

        /// <summary>
        /// Check this Vector3D for equality with another object.
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