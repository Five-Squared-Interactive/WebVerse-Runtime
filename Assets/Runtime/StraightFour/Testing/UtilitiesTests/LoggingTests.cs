// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.StraightFour.Utilities;

public class LoggingTests
{
    [Test]
    public void LoggingTests_General()
    {
        string message = "qwertyuiopasdfghjklzxcvbnm1234567890-=`!@#$%^&*()~-=_+[]{};':\",./<>?";

        // Log.
        LogAssert.Expect(LogType.Log, message);
        LogSystem.Log(message);
        LogAssert.Expect(LogType.Log, message);
        LogSystem.Log(message, LogSystem.Type.Debug);
        LogAssert.Expect(LogType.Log, message);
        LogSystem.Log(message, LogSystem.Type.Default);
        LogAssert.Expect(LogType.Warning, message);
        LogSystem.Log(message, LogSystem.Type.Warning);
        LogAssert.Expect(LogType.Error, message);
        LogSystem.Log(message, LogSystem.Type.Error);

        // Log Debug.
        LogAssert.Expect(LogType.Log, message);
        LogSystem.LogDebug(message);

        // Log Warning.
        LogAssert.Expect(LogType.Warning, message);
        LogSystem.LogWarning(message);

        // Log Error.
        LogAssert.Expect(LogType.Error, message);
        LogSystem.LogError(message);
    }
}