using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.VOSSynchronization;
using System;

public class VOSSynchronizerTests
{
    private float waitTime = 5;

    [UnityTest]
    public IEnumerator VOSSynchronizerTests_General()
    {
        bool connected = false;
        Action onConnected = () =>
        {
            connected = true;
        };
        VOSSynchronizer synchronizer = new VOSSynchronizer();
        synchronizer.Initialize("test.mosquitto.org", 1883, false, FiveSQD.WebVerse.WebInterface.MQTT.MQTTClient.Transports.TCP);
        synchronizer.Connect(onConnected);
        yield return new WaitForSeconds(waitTime);
        Assert.IsTrue(connected);
        int messageCount = 0;
        Action<string, string, string> onMessage = (first, second, third) =>
        {
            messageCount++;
        };
        synchronizer.AddMessageListener(onMessage);
        synchronizer.Disconnect();
        LogAssert.Expect(LogType.Error, "[VOSSynchronizer->ExitSession] Not in session.");
        synchronizer.Terminate();
    }
}