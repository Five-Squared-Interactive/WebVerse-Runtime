// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using NUnit.Framework;

using WT = FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

/// <summary>
/// Tests for JavascriptHandler WorldTypes (Vector3, Quaternion, Color, UUID).
/// Pure math/data classes with no external dependencies.
/// </summary>
[TestFixture]
public class WorldTypesTests
{
    // ─── Vector3 Construction ───

    [Test]
    public void Vector3_DefaultConstructor_IsZero()
    {

        var v = new WT.Vector3();
        Assert.AreEqual(0f, v.x, 0.001f);
        Assert.AreEqual(0f, v.y, 0.001f);
        Assert.AreEqual(0f, v.z, 0.001f);
    }

    [Test]
    public void Vector3_ParameterizedConstructor_SetsFields()
    {

        var v = new WT.Vector3(1f, 2f, 3f);
        Assert.AreEqual(1f, v.x, 0.001f);
        Assert.AreEqual(2f, v.y, 0.001f);
        Assert.AreEqual(3f, v.z, 0.001f);
    }

    // ─── Vector3 Properties ───

    [Test]
    public void Vector3_Magnitude_IsCorrect()
    {

        var v = new WT.Vector3(3f, 4f, 0f);
        Assert.AreEqual(5f, v.magnitude, 0.001f);
    }

    [Test]
    public void Vector3_SquaredMagnitude_IsCorrect()
    {

        var v = new WT.Vector3(3f, 4f, 0f);
        Assert.AreEqual(25f, v.squaredMagnitude, 0.001f);
    }

    [Test]
    public void Vector3_Magnitude_UnitVector()
    {

        var v = new WT.Vector3(1f, 0f, 0f);
        Assert.AreEqual(1f, v.magnitude, 0.001f);
    }

    [Test]
    public void Vector3_Magnitude_Zero()
    {

        var v = new WT.Vector3();
        Assert.AreEqual(0f, v.magnitude, 0.001f);
    }

    // ─── Vector3 Operators ───

    [Test]
    public void Vector3_Addition_IsCorrect()
    {

        var a = new WT.Vector3(1f, 2f, 3f);
        var b = new WT.Vector3(4f, 5f, 6f);
        var result = a + b;
        Assert.AreEqual(5f, result.x, 0.001f);
        Assert.AreEqual(7f, result.y, 0.001f);
        Assert.AreEqual(9f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_Subtraction_IsCorrect()
    {

        var a = new WT.Vector3(5f, 7f, 9f);
        var b = new WT.Vector3(1f, 2f, 3f);
        var result = a - b;
        Assert.AreEqual(4f, result.x, 0.001f);
        Assert.AreEqual(5f, result.y, 0.001f);
        Assert.AreEqual(6f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_ScalarMultiply_IsCorrect()
    {

        var v = new WT.Vector3(1f, 2f, 3f);
        var result = v * 2f;
        Assert.AreEqual(2f, result.x, 0.001f);
        Assert.AreEqual(4f, result.y, 0.001f);
        Assert.AreEqual(6f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_ScalarMultiply_LeftSide()
    {

        var v = new WT.Vector3(1f, 2f, 3f);
        var result = 3f * v;
        Assert.AreEqual(3f, result.x, 0.001f);
        Assert.AreEqual(6f, result.y, 0.001f);
        Assert.AreEqual(9f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_ComponentMultiply_IsCorrect()
    {

        var a = new WT.Vector3(2f, 3f, 4f);
        var b = new WT.Vector3(5f, 6f, 7f);
        var result = a * b;
        Assert.AreEqual(10f, result.x, 0.001f);
        Assert.AreEqual(18f, result.y, 0.001f);
        Assert.AreEqual(28f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_ScalarDivide_IsCorrect()
    {

        var v = new WT.Vector3(6f, 8f, 10f);
        var result = v / 2f;
        Assert.AreEqual(3f, result.x, 0.001f);
        Assert.AreEqual(4f, result.y, 0.001f);
        Assert.AreEqual(5f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_ComponentDivide_IsCorrect()
    {

        var a = new WT.Vector3(10f, 20f, 30f);
        var b = new WT.Vector3(2f, 4f, 5f);
        var result = a / b;
        Assert.AreEqual(5f, result.x, 0.001f);
        Assert.AreEqual(5f, result.y, 0.001f);
        Assert.AreEqual(6f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_Equality_SameValues_IsTrue()
    {

        var a = new WT.Vector3(1f, 2f, 3f);
        var b = new WT.Vector3(1f, 2f, 3f);
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [Test]
    public void Vector3_Equality_DifferentValues_IsFalse()
    {

        var a = new WT.Vector3(1f, 2f, 3f);
        var b = new WT.Vector3(4f, 5f, 6f);
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    // ─── Vector3 Static Constants ───

    [Test]
    public void Vector3_Forward_IsCorrect()
    {

        var v = WT.Vector3.forward;
        Assert.AreEqual(0f, v.x, 0.001f);
        Assert.AreEqual(0f, v.y, 0.001f);
        Assert.AreEqual(1f, v.z, 0.001f);
    }

    [Test]
    public void Vector3_Up_IsCorrect()
    {

        var v = WT.Vector3.up;
        Assert.AreEqual(0f, v.x, 0.001f);
        Assert.AreEqual(1f, v.y, 0.001f);
        Assert.AreEqual(0f, v.z, 0.001f);
    }

    [Test]
    public void Vector3_Right_IsCorrect()
    {

        var v = WT.Vector3.right;
        Assert.AreEqual(1f, v.x, 0.001f);
        Assert.AreEqual(0f, v.y, 0.001f);
        Assert.AreEqual(0f, v.z, 0.001f);
    }

    [Test]
    public void Vector3_Zero_IsCorrect()
    {

        var v = WT.Vector3.zero;
        Assert.AreEqual(0f, v.x, 0.001f);
        Assert.AreEqual(0f, v.y, 0.001f);
        Assert.AreEqual(0f, v.z, 0.001f);
    }

    [Test]
    public void Vector3_One_IsCorrect()
    {

        var v = WT.Vector3.one;
        Assert.AreEqual(1f, v.x, 0.001f);
        Assert.AreEqual(1f, v.y, 0.001f);
        Assert.AreEqual(1f, v.z, 0.001f);
    }

    // ─── Vector3 Static Methods ───

    [Test]
    public void Vector3_GetAngle_PerpendicularVectors_Is90()
    {

        float angle = WT.Vector3.GetAngle(WT.Vector3.right, WT.Vector3.up);
        Assert.AreEqual(90f, angle, 0.1f);
    }

    [Test]
    public void Vector3_GetDistance_IsCorrect()
    {

        var a = new WT.Vector3(0f, 0f, 0f);
        var b = new WT.Vector3(3f, 4f, 0f);
        float dist = WT.Vector3.GetDistance(a, b);
        Assert.AreEqual(5f, dist, 0.001f);
    }

    [Test]
    public void Vector3_GetDotProduct_PerpendicularIsZero()
    {

        float dot = WT.Vector3.GetDotProduct(WT.Vector3.right, WT.Vector3.up);
        Assert.AreEqual(0f, dot, 0.001f);
    }

    [Test]
    public void Vector3_GetDotProduct_ParallelIsPositive()
    {

        float dot = WT.Vector3.GetDotProduct(WT.Vector3.right, WT.Vector3.right);
        Assert.AreEqual(1f, dot, 0.001f);
    }

    [Test]
    public void Vector3_GetCrossProduct_IsCorrect()
    {

        var result = WT.Vector3.GetCrossProduct(WT.Vector3.right, WT.Vector3.up);
        Assert.AreEqual(0f, result.x, 0.001f);
        Assert.AreEqual(0f, result.y, 0.001f);
        Assert.AreEqual(1f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_LinearlyInterpolate_MovesTowards()
    {

        var a = new WT.Vector3(0f, 0f, 0f);
        var b = new WT.Vector3(10f, 0f, 0f);
        // LinearlyInterpolate uses MoveTowards with maxDistance parameter.
        var result = WT.Vector3.LinearlyInterpolate(a, b, 3f);
        Assert.AreEqual(3f, result.x, 0.001f);
        Assert.AreEqual(0f, result.y, 0.001f);
        Assert.AreEqual(0f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_GetNormalized_IsUnitLength()
    {

        var v = new WT.Vector3(3f, 4f, 0f);
        var n = v.GetNormalized();
        Assert.AreEqual(1f, n.magnitude, 0.001f);
    }

    [Test]
    public void Vector3_GetMin_ReturnsComponentwiseMin()
    {

        var a = new WT.Vector3(1f, 5f, 3f);
        var b = new WT.Vector3(4f, 2f, 6f);
        var result = WT.Vector3.GetMin(a, b);
        Assert.AreEqual(1f, result.x, 0.001f);
        Assert.AreEqual(2f, result.y, 0.001f);
        Assert.AreEqual(3f, result.z, 0.001f);
    }

    [Test]
    public void Vector3_GetMax_ReturnsComponentwiseMax()
    {

        var a = new WT.Vector3(1f, 5f, 3f);
        var b = new WT.Vector3(4f, 2f, 6f);
        var result = WT.Vector3.GetMax(a, b);
        Assert.AreEqual(4f, result.x, 0.001f);
        Assert.AreEqual(5f, result.y, 0.001f);
        Assert.AreEqual(6f, result.z, 0.001f);
    }

    // ─── Quaternion Construction ───

    [Test]
    public void Quaternion_DefaultConstructor_IsAllZero()
    {

        var q = new WT.Quaternion();
        Assert.AreEqual(0f, q.x, 0.001f);
        Assert.AreEqual(0f, q.y, 0.001f);
        Assert.AreEqual(0f, q.z, 0.001f);
        Assert.AreEqual(0f, q.w, 0.001f);
    }

    [Test]
    public void Quaternion_ParameterizedConstructor_SetsFields()
    {

        var q = new WT.Quaternion(0.1f, 0.2f, 0.3f, 0.9f);
        Assert.AreEqual(0.1f, q.x, 0.001f);
        Assert.AreEqual(0.2f, q.y, 0.001f);
        Assert.AreEqual(0.3f, q.z, 0.001f);
        Assert.AreEqual(0.9f, q.w, 0.001f);
    }

    [Test]
    public void Quaternion_Identity_IsCorrect()
    {

        var q = WT.Quaternion.identity;
        Assert.AreEqual(0f, q.x, 0.001f);
        Assert.AreEqual(0f, q.y, 0.001f);
        Assert.AreEqual(0f, q.z, 0.001f);
        Assert.AreEqual(1f, q.w, 0.001f);
    }

    // ─── Quaternion Methods ───

    [Test]
    public void Quaternion_FromEulerAngles_AndBack_RoundTrips()
    {

        var q = WT.Quaternion.FromEulerAngles(0f, 90f, 0f);
        var euler = q.GetEulerAngles();
        Assert.AreEqual(0f, euler.x, 1f);
        Assert.AreEqual(90f, euler.y, 1f);
        Assert.AreEqual(0f, euler.z, 1f);
    }

    [Test]
    public void Quaternion_GetAngle_BetweenSame_IsZero()
    {

        var q = WT.Quaternion.identity;
        float angle = WT.Quaternion.GetAngle(q, q);
        Assert.AreEqual(0f, angle, 0.1f);
    }

    [Test]
    public void Quaternion_GetAngle_Between90DegRotations()
    {

        var a = WT.Quaternion.FromEulerAngles(0, 0, 0);
        var b = WT.Quaternion.FromEulerAngles(0, 90, 0);
        float angle = WT.Quaternion.GetAngle(a, b);
        Assert.AreEqual(90f, angle, 1f);
    }

    [Test]
    public void Quaternion_GetInverse_TimesOriginal_IsIdentity()
    {

        var q = WT.Quaternion.FromEulerAngles(30, 60, 90);
        var inv = WT.Quaternion.GetInverse(q);
        var product = q * inv;
        // Should be close to identity.
        Assert.AreEqual(0f, product.x, 0.01f);
        Assert.AreEqual(0f, product.y, 0.01f);
        Assert.AreEqual(0f, product.z, 0.01f);
        Assert.AreEqual(1f, Math.Abs(product.w), 0.01f);
    }

    [Test]
    public void Quaternion_LinearlyInterpolatePercent_Midpoint()
    {

        var a = WT.Quaternion.FromEulerAngles(0, 0, 0);
        var b = WT.Quaternion.FromEulerAngles(0, 90, 0);
        var mid = WT.Quaternion.LinearlyInterpolatePercent(a, b, 0.5f);
        var euler = mid.GetEulerAngles();
        Assert.AreEqual(45f, euler.y, 2f);
    }

    [Test]
    public void Quaternion_GetDotProduct_SameQuaternion_IsOne()
    {

        var q = WT.Quaternion.identity;
        float dot = WT.Quaternion.GetDotProduct(q, q);
        Assert.AreEqual(1f, dot, 0.001f);
    }

    [Test]
    public void Quaternion_GetNormalized_HasUnitMagnitude()
    {

        var q = new WT.Quaternion(1f, 2f, 3f, 4f);
        var n = WT.Quaternion.GetNormalized(q);
        float mag = MathF.Sqrt(n.x * n.x + n.y * n.y + n.z * n.z + n.w * n.w);
        Assert.AreEqual(1f, mag, 0.001f);
    }

    [Test]
    public void Quaternion_Multiply_CombinesRotations()
    {

        var a = WT.Quaternion.FromEulerAngles(0, 45, 0);
        var b = WT.Quaternion.FromEulerAngles(0, 45, 0);
        var combined = a * b;
        var euler = combined.GetEulerAngles();
        Assert.AreEqual(90f, euler.y, 2f);
    }

    [Test]
    public void Quaternion_AngleAxisConstructor_IsCorrect()
    {

        var axis = new WT.Vector3(0f, 1f, 0f);
        var q = new WT.Quaternion(90f, axis);
        var euler = q.GetEulerAngles();
        Assert.AreEqual(90f, euler.y, 2f);
    }

    [Test]
    public void Quaternion_Equality_SameValues_IsTrue()
    {

        var a = WT.Quaternion.identity;
        var b = WT.Quaternion.identity;
        Assert.IsTrue(a == b);
    }

    [Test]
    public void Quaternion_CreateLookRotation_IsValid()
    {

        var forward = WT.Vector3.forward;
        var up = WT.Vector3.up;
        var q = WT.Quaternion.CreateLookRotation(forward, up);
        // Looking forward with up should be identity.
        Assert.AreEqual(0f, q.x, 0.01f);
        Assert.AreEqual(0f, q.y, 0.01f);
        Assert.AreEqual(0f, q.z, 0.01f);
        Assert.AreEqual(1f, q.w, 0.01f);
    }

    // ─── Color ───

    [Test]
    public void Color_Constructor_SetsFields()
    {

        var c = new WT.Color(0.1f, 0.2f, 0.3f, 0.4f);
        Assert.AreEqual(0.1f, c.r, 0.001f);
        Assert.AreEqual(0.2f, c.g, 0.001f);
        Assert.AreEqual(0.3f, c.b, 0.001f);
        Assert.AreEqual(0.4f, c.a, 0.001f);
    }

    [Test]
    public void Color_Black_IsCorrect()
    {

        var c = WT.Color.black;
        Assert.AreEqual(0f, c.r, 0.001f);
        Assert.AreEqual(0f, c.g, 0.001f);
        Assert.AreEqual(0f, c.b, 0.001f);
        Assert.AreEqual(1f, c.a, 0.001f);
    }

    [Test]
    public void Color_White_IsCorrect()
    {

        var c = WT.Color.white;
        Assert.AreEqual(1f, c.r, 0.001f);
        Assert.AreEqual(1f, c.g, 0.001f);
        Assert.AreEqual(1f, c.b, 0.001f);
        Assert.AreEqual(1f, c.a, 0.001f);
    }

    [Test]
    public void Color_Red_IsCorrect()
    {

        var c = WT.Color.red;
        Assert.AreEqual(1f, c.r, 0.001f);
        Assert.AreEqual(0f, c.g, 0.001f);
        Assert.AreEqual(0f, c.b, 0.001f);
    }

    [Test]
    public void Color_Green_IsCorrect()
    {

        var c = WT.Color.green;
        Assert.AreEqual(0f, c.r, 0.001f);
        Assert.AreEqual(1f, c.g, 0.001f);
        Assert.AreEqual(0f, c.b, 0.001f);
    }

    [Test]
    public void Color_Blue_IsCorrect()
    {

        var c = WT.Color.blue;
        Assert.AreEqual(0f, c.r, 0.001f);
        Assert.AreEqual(0f, c.g, 0.001f);
        Assert.AreEqual(1f, c.b, 0.001f);
    }

    [Test]
    public void Color_Clear_HasZeroAlpha()
    {

        var c = WT.Color.clear;
        Assert.AreEqual(0f, c.a, 0.001f);
    }

    [Test]
    public void Color_GrayAndGrey_AreEqual()
    {

        var gray = WT.Color.gray;
        var grey = WT.Color.grey;
        Assert.AreEqual(gray.r, grey.r, 0.001f);
        Assert.AreEqual(gray.g, grey.g, 0.001f);
        Assert.AreEqual(gray.b, grey.b, 0.001f);
    }

    // ─── UUID ───

    [Test]
    public void UUID_DefaultConstructor_IsEmpty()
    {

        var uuid = new WT.UUID();
        Assert.AreEqual(Guid.Empty.ToString(), uuid.ToString());
    }

    [Test]
    public void UUID_WithValidString_ParsesCorrectly()
    {

        string guidStr = "12345678-1234-1234-1234-123456789abc";
        var uuid = new WT.UUID(guidStr);
        Assert.AreEqual(guidStr, uuid.ToString());
    }

    [Test]
    public void UUID_NewUUID_GeneratesUniqueValues()
    {

        var a = WT.UUID.NewUUID();
        var b = WT.UUID.NewUUID();
        Assert.AreNotEqual(a.ToString(), b.ToString());
    }

    [Test]
    public void UUID_NewUUID_IsNotEmpty()
    {

        var uuid = WT.UUID.NewUUID();
        Assert.AreNotEqual(Guid.Empty.ToString(), uuid.ToString());
    }

    [Test]
    public void UUID_Parse_ValidString_ReturnsUUID()
    {

        string guidStr = Guid.NewGuid().ToString();
        var uuid = WT.UUID.Parse(guidStr);
        Assert.AreEqual(guidStr, uuid.ToString());
    }

    [Test]
    public void UUID_Parse_InvalidString_Throws()
    {

        Assert.Throws<FormatException>(() => WT.UUID.Parse("not-a-guid"));
    }

    [Test]
    public void UUID_ToString_RoundTrips()
    {

        var original = WT.UUID.NewUUID();
        string str = original.ToString();
        var parsed = new WT.UUID(str);
        Assert.AreEqual(str, parsed.ToString());
    }

    [Test]
    public void UUID_WithInvalidString_Throws()
    {

        Assert.Throws<FormatException>(() => new WT.UUID("invalid"));
    }
}
