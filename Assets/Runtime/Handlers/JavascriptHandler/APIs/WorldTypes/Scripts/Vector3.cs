// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a 3-dimensional vector.
    /// </summary>
    public class Vector3
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
        /// Constructor for a Vector3.
        /// </summary>
        public Vector3()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        /// <summary>
        /// Constructor for a Vector3.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Add 2 Vector3s.
        /// </summary>
        /// <param name="first">First Vector3 to add.</param>
        /// <param name="second">Second Vector3 to add.</param>
        /// <returns>The sum of the Vector3s.</returns>
        public static Vector3 operator +(Vector3 first, Vector3 second) => new Vector3(first.x + second.x, first.y + second.y, first.z + second.z);

        /// <summary>
        /// Subtract two Vector3s.
        /// </summary>
        /// <param name="first">Vector3 to subtract from.</param>
        /// <param name="second">Vector3 to subtract.</param>
        /// <returns>The difference of the Vector3s.</returns>
        public static Vector3 operator -(Vector3 first, Vector3 second) => new Vector3(first.x - second.x, first.y - second.y, first.z - second.z);

        /// <summary>
        /// Multiply (component-wise) two Vector3s.
        /// </summary>
        /// <param name="first">First Vector3 to multiply.</param>
        /// <param name="second">Second Vector3 to multiply.</param>
        /// <returns>The product of the Vector3s.</returns>
        public static Vector3 operator *(Vector3 first, Vector3 second) => new Vector3(first.x * second.x, first.y * second.y, first.z * second.z);

        /// <summary>
        /// Multiply a Vector3 by a floating-point value.
        /// </summary>
        /// <param name="first">Vector3 to multiply.</param>
        /// <param name="second">Float to multiply Vector3 components by.</param>
        /// <returns>The product of the Vector3 and float.</returns>
        public static Vector3 operator *(Vector3 first, float second) => new Vector3(first.x * second, first.y * second, first.z * second);

        /// <summary>
        /// Multiply a Vector3 by a floating-point value.
        /// </summary>
        /// <param name="first">Float to multiply Vector3 components by.</param>
        /// <param name="second">Vector3 to multiply.</param>
        /// <returns>The product of the Vector3 and float.</returns>
        public static Vector3 operator *(float first, Vector3 second) => new Vector3(first * second.x, first * second.y, first * second.z);

        /// <summary>
        /// Divide (component-wise) two Vector3s.
        /// </summary>
        /// <param name="first">Dividend.</param>
        /// <param name="second">Divisor.</param>
        /// <returns>The quotient of the two Vector3s.</returns>
        public static Vector3 operator /(Vector3 first, Vector3 second) => new Vector3(first.x / second.x, first.y / second.y, first.z / second.z);

        /// <summary>
        /// Divide a Vector3 by a floating-point value.
        /// </summary>
        /// <param name="first">Vector3 dividend.</param>
        /// <param name="second">Float divisor.</param>
        /// <returns>The quotient of the Vector3 and float.</returns>
        public static Vector3 operator /(Vector3 first, float second) => new Vector3(first.x / second, first.y / second, first.z / second);

        /// <summary>
        /// Determine whether two Vector3s are equal.
        /// </summary>
        /// <param name="first">First Vector3.</param>
        /// <param name="second">Second Vector3.</param>
        /// <returns>Whether or not the Vector3s are equal.</returns>
        public static bool operator ==(Vector3 first, Vector3 second) => first.AreEqual(second);

        /// <summary>
        /// Determine whether two Vector3s are not equal.
        /// </summary>
        /// <param name="first">First Vector3.</param>
        /// <param name="second">Second Vector3.</param>
        /// <returns>Whether or not the Vector3s are different.</returns>
        public static bool operator !=(Vector3 first, Vector3 second) => !first.AreEqual(second);

        /// <summary>
        /// Get a forward (0, 0, 1) vector.
        /// </summary>
        public static Vector3 forward
        {
            get
            {
                return new Vector3(0, 0, 1);
            }
        }

        /// <summary>
        /// Get a back (0, 0, -1) vector.
        /// </summary>
        public static Vector3 back
        {
            get
            {
                return new Vector3(0, 0, -1);
            }
        }

        /// <summary>
        /// Get a down (0, -1, 0) vector.
        /// </summary>
        public static Vector3 down
        {
            get
            {
                return new Vector3(0, -1, 0);
            }
        }

        /// <summary>
        /// Get an up (0, 1, 0) vector.
        /// </summary>
        public static Vector3 up
        {
            get
            {
                return new Vector3(0, 1, 0);
            }
        }

        /// <summary>
        /// Get a left (-1, 0, 0) vector.
        /// </summary>
        public static Vector3 left
        {
            get
            {
                return new Vector3(-1, 0, 0);
            }
        }

        /// <summary>
        /// Get a right (1, 0, 0) vector.
        /// </summary>
        public static Vector3 right
        {
            get
            {
                return new Vector3(1, 0, 0);
            }
        }

        /// <summary>
        /// Get a vector with all components set to 0.
        /// </summary>
        public static Vector3 zero
        {
            get
            {
                return new Vector3(0, 0, 0);
            }
        }

        /// <summary>
        /// Get a vector with all components set to 1.
        /// </summary>
        public static Vector3 one
        {
            get
            {
                return new Vector3(1, 1, 1);
            }
        }

        /// <summary>
        /// Get a vector with all components set to +infinity.
        /// </summary>
        public static Vector3 positiveInfinity
        {
            get
            {
                return new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            }
        }

        /// <summary>
        /// Get a vector with all components set to -infinity.
        /// </summary>
        public static Vector3 negativeInfinity
        {
            get
            {
                return new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            }
        }

        /// <summary>
        /// Get the angle between two Vector3s.
        /// </summary>
        /// <param name="vector1">First Vector3.</param>
        /// <param name="vector2">Second Vector3.</param>
        /// <returns>The angle between the two vectors.</returns>
        public static float GetAngle(Vector3 vector1, Vector3 vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3(vector2.x, vector2.y, vector2.z);
            return UnityEngine.Vector3.Angle(first, second);
        }

        /// <summary>
        /// Get a Vector3 clamped to a certain length.
        /// </summary>
        /// <param name="vector">Vector3 to clamp.</param>
        /// <param name="maxLength">Maximum length for the Vector3.</param>
        /// <returns>A Vector3 clamped to a certain length.</returns>
        public static Vector3 GetClampedVector3(Vector3 vector, float maxLength)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3(vector.x, vector.y, vector.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.ClampMagnitude(vec, maxLength);
            return new Vector3(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get the distance between two Vector3s.
        /// </summary>
        /// <param name="vector1">First Vector3.</param>
        /// <param name="vector2">Second Vector3.</param>
        /// <returns>The distance between two Vector3s.</returns>
        public static float GetDistance(Vector3 vector1, Vector3 vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3(vector2.x, vector2.y, vector2.z);
            return UnityEngine.Vector3.Distance(first, second);
        }

        /// <summary>
        /// Get the dot product between two Vector3s.
        /// </summary>
        /// <param name="leftHand">Left hand Vector3.</param>
        /// <param name="rightHand">Right hand Vector3.</param>
        /// <returns>The dot product between two Vector3s.</returns>
        public static float GetDotProduct(Vector3 leftHand, Vector3 rightHand)
        {
            UnityEngine.Vector3 l = new UnityEngine.Vector3(leftHand.x, leftHand.y, leftHand.z);
            UnityEngine.Vector3 r = new UnityEngine.Vector3(rightHand.x, rightHand.y, rightHand.z);
            return UnityEngine.Vector3.Dot(l, r);
        }

        /// <summary>
        /// Get the cross product between two Vector3s.
        /// </summary>
        /// <param name="leftHand">Left hand Vector3.</param>
        /// <param name="rightHand">Right hand Vector3.</param>
        /// <returns>The cross product between two Vector3s.</returns>
        public static Vector3 GetCrossProduct(Vector3 leftHand, Vector3 rightHand)
        {
            UnityEngine.Vector3 l = new UnityEngine.Vector3(leftHand.x, leftHand.y, leftHand.z);
            UnityEngine.Vector3 r = new UnityEngine.Vector3(rightHand.x, rightHand.y, rightHand.z);
            UnityEngine.Vector3 res = UnityEngine.Vector3.Cross(l, r);
            return new Vector3(res.x, res.y, res.z);
        }

        /// <summary>
        /// Get the result of projecting a vector on a normal.
        /// </summary>
        /// <param name="vector">Vector to project.</param>
        /// <param name="normal">Normal to project vector on.</param>
        /// <returns>The result of projecting a vector on a normal.</returns>
        public static Vector3 Project(Vector3 vector, Vector3 normal)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3(vector.x, vector.y, vector.z);
            UnityEngine.Vector3 norm = new UnityEngine.Vector3(normal.x, normal.y, normal.z);
            UnityEngine.Vector3 res = UnityEngine.Vector3.Project(vec, norm);
            return new Vector3(res.x, res.y, res.z);
        }

        /// <summary>
        /// Get the result of projecting a vector on a plane.
        /// </summary>
        /// <param name="vector">Vector to project.</param>
        /// <param name="normalOfPlane">Normal of plane to project vector on.</param>
        /// <returns>The result of projecting a vector on a plane.</returns>
        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 normalOfPlane)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3(vector.x, vector.y, vector.z);
            UnityEngine.Vector3 norm = new UnityEngine.Vector3(normalOfPlane.x, normalOfPlane.y, normalOfPlane.z);
            UnityEngine.Vector3 res = UnityEngine.Vector3.ProjectOnPlane(vec, norm);
            return new Vector3(res.x, res.y, res.z);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector3s percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector3.</param>
        /// <param name="vector2">Second Vector3.</param>
        /// <param name="percent">Percent between first and second Vector3.</param>
        /// <returns>A linearly interpolated Vector3 between the two Vector3s.</returns>
        public static Vector3 LinearlyInterpolatePercent(Vector3 vector1, Vector3 vector2, float percent)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3(vector2.x, vector2.y, vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.Lerp(first, second, percent);
            return new Vector3(result.x, result.y, result.z);
        }

        /// <summary>
        /// Lineraly interpolate between two Vector3s distance-wise.
        /// </summary>
        /// <param name="vector1">First Vector3.</param>
        /// <param name="vector2">Second Vector3.</param>
        /// <param name="maxDistance">Maximum distance between first and second Vector3.</param>
        /// <returns>A linearly interpolated Vector3 between the two Vector3s.</returns>
        public static Vector3 LinearlyInterpolate(Vector3 vector1, Vector3 vector2, float maxDistance)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3(vector2.x, vector2.y, vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.MoveTowards(first, second, maxDistance);
            return new Vector3(result.x, result.y, result.z);
        }

        /// <summary>
        /// Lineraly interpolate unclamped between two Vector3s percentage-wise.
        /// </summary>
        /// <param name="vector1">First Vector3.</param>
        /// <param name="vector2">Second Vector3.</param>
        /// <param name="percent">Percent between first and second Vector3.</param>
        /// <returns>A linearly interpolated unclamped Vector3 between the two Vector3s.</returns>
        public static Vector3 LinearlyInterpolatePercentUnclamped(Vector3 vector1, Vector3 vector2, float percent)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3(vector2.x, vector2.y, vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.LerpUnclamped(first, second, percent);
            return new Vector3(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get smallest Vector3 using the components of two Vector3s.
        /// </summary>
        /// <param name="vector1">First Vector3.</param>
        /// <param name="vector2">Second Vector3.</param>
        /// <returns>Smallest Vector3 using the components of two Vector3s.</returns>
        public static Vector3 GetMin(Vector3 vector1, Vector3 vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3(vector2.x, vector2.y, vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.Min(first, second);
            return new Vector3(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get largest Vector3 using the components of two Vector3s.
        /// </summary>
        /// <param name="vector1">First Vector3.</param>
        /// <param name="vector2">Second Vector3.</param>
        /// <returns>Largest Vector3 using the components of two Vector3s.</returns>
        public static Vector3 GetMax(Vector3 vector1, Vector3 vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3(vector2.x, vector2.y, vector2.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.Max(first, second);
            return new Vector3(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get a Vector3 that reflects the provided vector off of the provided normal line.
        /// </summary>
        /// <param name="vector">Vector3 to reflect.</param>
        /// <param name="normal">Vector3 to reflect vector off of.</param>
        /// <returns>A Vector3 that reflects the provided vector off of the provided normal line.</returns>
        public static Vector3 GetReflectedVector(Vector3 vector, Vector3 normal)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3(vector.x, vector.y, vector.z);
            UnityEngine.Vector3 norm = new UnityEngine.Vector3(normal.x, normal.y, normal.z);
            UnityEngine.Vector3 result = UnityEngine.Vector3.Reflect(vec, norm);
            return new Vector3(result.x, result.y, result.z);
        }

        /// <summary>
        /// Get a signed angle between the two Vector3s.
        /// </summary>
        /// <param name="vector1">First Vector3.</param>
        /// <param name="vector2">Second Vector3.</param>
        /// <returns>Signed angle between the two Vector3s.</returns>
        public static float GetSignedAngle(Vector3 vector1, Vector3 vector2, Vector3 axis)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3(vector2.x, vector2.y, vector2.z);
            UnityEngine.Vector3 ax = new UnityEngine.Vector3(axis.x, axis.y, axis.z);
            return UnityEngine.Vector3.SignedAngle(first, second, ax);
        }

        /// <summary>
        /// Get a normalized (magnitude 1) version of the vector.
        /// </summary>
        /// <returns>A normalized (magnitude 1) version of the vector.</returns>
        public Vector3 GetNormalized()
        {
            float scaleFactor = 1 / magnitude;

            return new Vector3(x * scaleFactor, y * scaleFactor, z * scaleFactor);
        }

        /// <summary>
        /// Get normalized (magnitude 1) and orthagonal versions of the vectors.
        /// </summary>
        /// <returns>A normalized (magnitude 1) and orthagonal version of the vectors.</returns>
        public Tuple<Vector3, Vector3> GetNormalizedAndOrthagonal(Vector3 vector1, Vector3 vector2)
        {
            UnityEngine.Vector3 first = new UnityEngine.Vector3(vector1.x, vector1.y, vector1.z);
            UnityEngine.Vector3 second = new UnityEngine.Vector3(vector2.x, vector2.y, vector2.z);
            UnityEngine.Vector3.OrthoNormalize(ref first, ref second);
            return new Tuple<Vector3, Vector3>(new Vector3(first.x, first.y, first.z), new Vector3(second.x, second.y, second.z));
        }

        /// <summary>
        /// Normalize the Vector3.
        /// </summary>
        public void Normalize()
        {
            float scaleFactor = 1 / magnitude;

            x *= scaleFactor;
            y *= scaleFactor;
            z *= scaleFactor;
        }

        /// <summary>
        /// Determine whether or not this Vector3 equals another.
        /// </summary>
        /// <param name="other">Vector3 to compare this one with.</param>
        /// <returns>Whether or not this Vector3 equals the other.</returns>
        public bool AreEqual(Vector3 other)
        {
            if (other.x == x && other.y == y && other.z == z)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert this Vector3 to a string.
        /// </summary>
        /// <returns>String representation of this Vector3.</returns>
        public override string ToString()
        {
            UnityEngine.Vector3 v2 = new UnityEngine.Vector3(x, y, z);
            return v2.ToString();
        }

        /// <summary>
        /// Convert this Vector3 to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this Vector3.</returns>
        public string ToString(string format)
        {
            UnityEngine.Vector3 v2 = new UnityEngine.Vector3(x, y, z);
            return v2.ToString(format);
        }

        /// <summary>
        /// Check this Vector3 for equality with another object.
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