// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

#if USE_BESTHTTP
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WebInterface.WebSocket;
using System;

/// <summary>
/// Unit tests for WebSockets.
/// </summary>
public class WebSocketTests
{
    private float waitTime = 2; // Reduced wait time for better test performance

    [Test]
    public void WebSocket_Constructor_InitializesCorrectly()
    {
        // Test WebSocket initialization without actually connecting
        bool errorOccurred = false;
        
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { errorOccurred = true; };

        WebSocket webSocket = new WebSocket("wss://invalid-test-url.local",
            onOpen, onClosed, onBinary, onMessage, onError);
        
        // Test that WebSocket was created
        Assert.IsNotNull(webSocket);
        Assert.IsFalse(webSocket.isOpen);
    }

    [Test]
    public void WebSocket_AddActions_DoesNotThrow()
    {
        // Test adding additional actions
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);
        
        // Test adding additional actions
        Action<WebSocket> onOpen2 = (ws) => { };
        Action<WebSocket, ushort, string> onClosed2 = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary2 = (ws, data) => { };
        Action<WebSocket, string> onMessage2 = (ws, msg) => { };
        Action<WebSocket, string> onError2 = (ws, msg) => { };

        Assert.DoesNotThrow(() =>
        {
            webSocket.AddOnOpenAction(onOpen2);
            webSocket.AddOnClosedAction(onClosed2);
            webSocket.AddOnBinaryAction(onBinary2);
            webSocket.AddOnMessageAction(onMessage2);
            webSocket.AddOnErrorAction(onError2);
        });
    }

    [UnityTest]
    public IEnumerator WebSocket_ConnectionToInvalidHost_HandlesGracefully()
    {
        // Test connection to invalid host - should handle gracefully
        bool connected = false;
        bool connected2 = false;
        bool errorOccurred = false;
        
        Action<WebSocket> onOpen = (ws) => { connected = true; };
        Action<WebSocket> onOpen2 = (ws) => { connected2 = true; };
        
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { connected = false; };
        Action<WebSocket, ushort, string> onClosed2 = (ws, code, msg) => { connected2 = false; };
        
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, byte[]> onBinary2 = (ws, data) => { };
        
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onMessage2 = (ws, msg) => { };
        
        Action<WebSocket, string> onError = (ws, msg) => { errorOccurred = true; };
        Action<WebSocket, string> onError2 = (ws, msg) => { errorOccurred = true; };

        WebSocket webSocket = new WebSocket("wss://invalid-host-for-testing.local",
            onOpen, onClosed, onBinary, onMessage, onError);
        webSocket.AddOnOpenAction(onOpen2);
        webSocket.AddOnClosedAction(onClosed2);
        webSocket.AddOnBinaryAction(onBinary2);
        webSocket.AddOnMessageAction(onMessage2);
        webSocket.AddOnErrorAction(onError2);

        Assert.IsFalse(connected);
        Assert.IsFalse(connected2);
        Assert.IsFalse(webSocket.isOpen);
        
        try
        {
            webSocket.Open();
        }
        catch (Exception)
        {
            // Expected for invalid host
        }
        
        yield return new WaitForSeconds(waitTime);
        
        // Should not connect to invalid host
        Assert.IsFalse(webSocket.isOpen);
        Assert.IsFalse(connected);
        Assert.IsFalse(connected2);
        
        // Test sending to closed socket
        webSocket.Send("test");
        webSocket.Send(new byte[] { 0, 1, 2, 3 });
        
        // Test closing already closed socket
        webSocket.Close();
    }

    [Test]
    public void WebSocket_SendWithoutConnection_DoesNotThrow()
    {
        // Test sending data without connection
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);
        
        // Should not throw when sending without connection
        Assert.DoesNotThrow(() =>
        {
            webSocket.Send("test message");
            webSocket.Send(new byte[] { 0, 1, 2, 3, 4, 5 });
        });
    }
}
#endif