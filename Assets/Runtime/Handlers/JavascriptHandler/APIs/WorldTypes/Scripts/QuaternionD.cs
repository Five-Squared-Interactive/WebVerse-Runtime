// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes
{
    /// <summary>
    /// Class for a double quaternion.
    /// </summary>
    public class QuaternionD
    {
        /// <summary>
        /// X component of the QuaternionD.
        /// </summary>
        public double x;

        /// <summary>
        /// Y component of the QuaternionD.
        /// </summary>
        public double y;

        /// <summary>
        /// Z component of the QuaternionD.
        /// </summary>
        public double z;

        /// <summary>
        /// W component of the QuaternionD.
        /// </summary>
        public double w;

        /// <summary>
        /// Constructor for a QuaternionD.
        /// </summary>
        public QuaternionD()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
        }

        /// <summary>
        /// Constructor for a QuaternionD.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="w">W component.</param>
        public QuaternionD(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Constructor for a QuaternionD using an angle axis input.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="axis"></param>
        public QuaternionD(double angle, Vector3 axis)
        {
            UnityEngine.Quaternion res = UnityEngine.Quaternion.AngleAxis((float) angle, new UnityEngine.Vector3(axis.x, axis.y, axis.z));
            x = res.x;
            y = res.y;
            z = res.z;
            w = res.w;
        }

        /// <summary>
        /// Multiply (combine) two QuaternionDs.
        /// </summary>
        /// <param name="first">First QuaternionD to multiply.</param>
        /// <param name="second">Second QuaternionD to multiply.</param>
        /// <returns>The product of the QuaternionDs.</returns>
        public static QuaternionD operator *(QuaternionD first, QuaternionD second) => Combine(first, second);

        /// <summary>
        /// Determine whether two QuaternionDs are equal.
        /// </summary>
        /// <param name="first">First QuaternionD.</param>
        /// <param name="second">Second QuaternionD.</param>
        /// <returns>Whether or not the QuaternionDs are equal.</returns>
        public static bool operator ==(QuaternionD first, QuaternionD second) => AreEqual(first, second);

        /// <summary>
        /// Determine whether two QuaternionDs are not equal.
        /// </summary>
        /// <param name="first">First QuaternionD.</param>
        /// <param name="second">Second QuaternionD.</param>
        /// <returns>Whether or not the QuaternionDs are different.</returns>
        public static bool operator !=(QuaternionD first, QuaternionD second) => !AreEqual(first, second);

        /// <summary>
        /// An identity (0, 0, 0, 1) QuaternionD.
        /// </summary>
        public static QuaternionD Identity
        {
            get
            {
                return new QuaternionD(0, 0, 0, 1);
            }
        }

        /// <summary>
        /// Get the angle between two QuaternionDs.
        /// </summary>
        /// <param name="quaternion1">First QuaternionD.</param>
        /// <param name="quaternion2">Second QuaternionD.</param>
        /// <returns>The angle between the two QuaternionDs.</returns>
        public static double GetAngle(QuaternionD quaternion1, QuaternionD quaternion2)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion((float) quaternion1.x,
                (float) quaternion1.y, (float) quaternion1.z, (float) quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion((float) quaternion2.x,
                (float) quaternion2.y, (float) quaternion2.z, (float) quaternion2.w);
            return UnityEngine.Quaternion.Angle(first, second);
        }

        /// <summary>
        /// Get a QuaternionD from Euler angles.
        /// </summary>
        /// <param name="x">Rotation around x axis.</param>
        /// <param name="y">Rotation around y axis.</param>
        /// <param name="z">Rotation around z axis.</param>
        /// <returns>QuaternionD from provided Euler angles.</returns>
        public static QuaternionD FromEulerAngles(double x, double y, double z)
        {
            UnityEngine.Vector3 vec = new UnityEngine.Vector3((float) x, (float) y, (float) z);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.Euler(vec);
            return new QuaternionD(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Get the inverse of a QuaternionDs.
        /// </summary>
        /// <param name="QuaternionD1">First QuaternionD.</param>
        /// <param name="QuaternionD2">Second QuaternionD.</param>
        /// <returns>The angle between the two QuaternionDs.</returns>
        public static QuaternionD GetInverse(QuaternionD QuaternionD)
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion((float) QuaternionD.x,
                (float) QuaternionD.y, (float) QuaternionD.z, (float) QuaternionD.w);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.Inverse(quat);
            return new QuaternionD(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Lineraly interpolate between two QuaternionDs percentage-wise.
        /// </summary>
        /// <param name="quaternion1">First QuaternionD.</param>
        /// <param name="quaternion2">Second QuaternionD.</param>
        /// <param name="percent">Percent between first and second QuaternionD.</param>
        /// <returns>A linearly interpolated QuaternionD between the two QuaternionDs.</returns>
        public static QuaternionD LinearlyInterpolatePercent(QuaternionD quaternion1, QuaternionD quaternion2, double percent)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion((float) quaternion1.x, (float) quaternion1.y,
                (float) quaternion1.z, (float) quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion((float) quaternion2.x, (float) quaternion2.y,
                (float) quaternion2.z, (float) quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.Lerp(first, second, (float) percent);
            return new QuaternionD(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Lineraly interpolate between two QuaternionDs distance-wise.
        /// </summary>
        /// <param name="quaternion1">First QuaternionD.</param>
        /// <param name="quaternion2">Second QuaternionD.</param>
        /// <param name="maxDistance">Maximum distance between first and second QuaternionD.</param>
        /// <returns>A linearly interpolated QuaternionD between the two QuaternionDs.</returns>
        public static QuaternionD LinearlyInterpolate(QuaternionD quaternion1, QuaternionD quaternion2, double maxDistance)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion((float) quaternion1.x,
                (float) quaternion1.y, (float) quaternion1.z, (float) quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion((float) quaternion2.x,
                (float) quaternion2.y, (float) quaternion2.z, (float) quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.RotateTowards(first, second, (float) maxDistance);
            return new QuaternionD(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Lineraly interpolate unclamped between two QuaternionDs percentage-wise.
        /// </summary>
        /// <param name="quaternion1">First QuaternionD.</param>
        /// <param name="quaternion2">Second QuaternionD.</param>
        /// <param name="percent">Percent between first and second QuaternionD.</param>
        /// <returns>A linearly interpolated unclamped QuaternionD between the two QuaternionDs.</returns>
        public static QuaternionD LinearlyInterpolatePercentUnclamped(QuaternionD quaternion1, QuaternionD quaternion2, double percent)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion((float) quaternion1.x,
                (float) quaternion1.y, (float) quaternion1.z, (float) quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion((float) quaternion2.x,
                (float) quaternion2.y, (float) quaternion2.z, (float) quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.LerpUnclamped(first, second, (float) percent);
            return new QuaternionD(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Create a QuaternionD with a given forward and up direction.
        /// </summary>
        /// <param name="forward">Forward direction.</param>
        /// <param name="up">Up direction.</param>
        /// <returns>A QuaternionD with a given forward and up direction.</returns>
        public static QuaternionD CreateLookRotation(Vector3 forward, Vector3 up)
        {
            UnityEngine.Vector3 f = new UnityEngine.Vector3(forward.x, forward.y, forward.z);
            UnityEngine.Vector3 u = new UnityEngine.Vector3(up.x, up.y, up.z);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.LookRotation(f, u);
            return new QuaternionD(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Create a QuaternionD with a given forward direction and an up direction facing Vector3.Up.
        /// </summary>
        /// <param name="forward">Forward direction.</param>
        /// <returns>A QuaternionD with a given forward direction and an up direction facing Vector3.up.</returns>
        public static QuaternionD CreateLookRotation(Vector3 forward)
        {
            UnityEngine.Vector3 f = new UnityEngine.Vector3(forward.x, forward.y, forward.z);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.LookRotation(f, UnityEngine.Vector3.up);
            return new QuaternionD(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Get Euler angle representation of the QuaternionD.
        /// </summary>
        /// <returns>Euler angle representation of the QuaternionD.</returns>
        public Vector3 GetEulerAngles()
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion((float) x, (float) y, (float) z, (float) w);
            UnityEngine.Vector3 res = quat.eulerAngles;
            return new Vector3(res.x, res.y, res.z);
        }

        /// <summary>
        /// Get the dot product between two QuaternionDs.
        /// </summary>
        /// <param name="first">First QuaternionD.</param>
        /// <param name="second">Second QuaternionD.</param>
        /// <returns>The dot product between two QuaternionDs.</returns>
        public static double GetDotProduct(QuaternionD first, QuaternionD second)
        {
            UnityEngine.Quaternion f = new UnityEngine.Quaternion((float) first.x, (float) first.y, (float) first.z, (float) first.w);
            UnityEngine.Quaternion s = new UnityEngine.Quaternion((float) second.x, (float) second.y, (float) second.z, (float) second.w);
            return UnityEngine.Quaternion.Dot(f, s);
        }

        /// <summary>
        /// Get the rotation from one Vector3 to another Vector3.
        /// </summary>
        /// <param name="from">First Vector3.</param>
        /// <param name="to">Second Vector3.</param>
        /// <returns>The rotation from one Vector3 to another Vector3.</returns>
        public static QuaternionD GetRotationFromToVector3s(Vector3 from, Vector3 to)
        {
            UnityEngine.Vector3 f = new UnityEngine.Vector3(from.x, from.y, from.z);
            UnityEngine.Vector3 t = new UnityEngine.Vector3(to.x, to.y, to.z);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.FromToRotation(f, t);
            return new QuaternionD(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Get a normalized QuaternionD (with a magnitude of 1).
        /// </summary>
        /// <param name="quaternion">Input QuaternionD.</param>
        /// <returns>A normalized QuaternionD.</returns>
        public static QuaternionD GetNormalized(QuaternionD quaternion)
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion((float) quaternion.x,
                (float) quaternion.y, (float) quaternion.z, (float) quaternion.w);
            UnityEngine.Quaternion res = UnityEngine.Quaternion.Normalize(quat);
            return new QuaternionD(res.x, res.y, res.z, res.w);
        }

        /// <summary>
        /// Interpolate between two QuaternionDs.
        /// </summary>
        /// <param name="quaternion1">First QuaternionD.</param>
        /// <param name="quaternion2">Second QuaternionD.</param>
        /// <param name="maxDegrees">Maximum degrees between first and second QuaternionD.</param>
        /// <returns>An interpolated QuaternionD between the two QuaternionDs.</returns>
        public static QuaternionD Interpolate(QuaternionD quaternion1, QuaternionD quaternion2, double maxDegrees)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion((float) quaternion1.x,
                (float) quaternion1.y, (float) quaternion1.z, (float) quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion((float) quaternion2.x,
                (float) quaternion2.y, (float) quaternion2.z, (float) quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.RotateTowards(first, second, (float) maxDegrees);
            return new QuaternionD(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Spherically interpolate between two QuaternionDs.
        /// </summary>
        /// <param name="quaternion1">First QuaternionD.</param>
        /// <param name="quaternion2">Second QuaternionD.</param>
        /// <param name="percent">Percentage between the two QuaternionDs to interpolate.</param>
        /// <returns>A spherically interpolated QuaternionD between the two QuaternionDs.</returns>
        public static QuaternionD SphericallyInterpolate(QuaternionD quaternion1, QuaternionD quaternion2, double percent)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion((float) quaternion1.x,
                (float) quaternion1.y, (float) quaternion1.z, (float) quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion((float) quaternion2.x,
                (float) quaternion2.y, (float) quaternion2.z, (float) quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.Slerp(first, second, (float) percent);
            return new QuaternionD(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Spherically interpolate unclamped between two QuaternionDs.
        /// </summary>
        /// <param name="quaternion1">First QuaternionD.</param>
        /// <param name="quaternion2">Second QuaternionD.</param>
        /// <param name="percent">Percentage between the two QuaternionDs to interpolate.</param>
        /// <returns>A spherically interpolated unclamped QuaternionD between the two QuaternionDs.</returns>
        public static QuaternionD SphericallyInterpolateUnclamped(QuaternionD quaternion1, QuaternionD quaternion2, double percent)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion((float) quaternion1.x,
                (float) quaternion1.y, (float) quaternion1.z, (float) quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion((float) quaternion2.x,
                (float) quaternion2.y, (float) quaternion2.z, (float) quaternion2.w);
            UnityEngine.Quaternion result = UnityEngine.Quaternion.SlerpUnclamped(first, second, (float) percent);
            return new QuaternionD(result.x, result.y, result.z, result.w);
        }

        /// <summary>
        /// Get angle axis representation of the QuaternionD.
        /// </summary>
        /// <returns>Angle axis representation of the QuaternionD.</returns>
        public Tuple<double, Vector3> GetAngleAxis()
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion((float) x, (float) y, (float) z, (float) w);
            float angle = 0;
            UnityEngine.Vector3 axis = UnityEngine.Vector3.zero;
            quat.ToAngleAxis(out angle, out axis);
            return new Tuple<double, Vector3>(angle, new Vector3(axis.x, axis.y, axis.z));
        }

        /// <summary>
        /// Convert this QuaternionD to a string.
        /// </summary>
        /// <returns>String representation of this QuaternionD.</returns>
        public override string ToString()
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion((float) x, (float) y, (float) z, (float) w);
            return quat.ToString();
        }

        /// <summary>
        /// Convert this QuaternionD to a string.
        /// </summary>
        /// <param name="format">Format specifiers.</param>
        /// <returns>String representation of this QuaternionD.</returns>
        public string ToString(string format)
        {
            UnityEngine.Quaternion quat = new UnityEngine.Quaternion((float) x, (float) y, (float) z, (float) w);
            return quat.ToString(format);
        }

        /// <summary>
        /// Check this QuaternionD for equality with another object.
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
        private static bool AreEqual(QuaternionD quaternion1, QuaternionD quaternion2)
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
        public static QuaternionD Combine(QuaternionD quaternion1, QuaternionD quaternion2)
        {
            UnityEngine.Quaternion first = new UnityEngine.Quaternion((float) quaternion1.x,
                (float) quaternion1.y, (float) quaternion1.z, (float) quaternion1.w);
            UnityEngine.Quaternion second = new UnityEngine.Quaternion((float) quaternion2.x,
                (float) quaternion2.y, (float) quaternion2.z, (float) quaternion2.w);
            UnityEngine.Quaternion result = first * second;
            return new QuaternionD(result.x, result.y, result.z, result.w);
        }
    }
}