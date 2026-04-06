// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

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

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [SetUp]
    public void SetUp()
    {
        LogAssert.ignoreFailingMessages = true;
    }

    [Test]
    public void WebSocket_Constructor_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test WebSocket initialization without actually connecting
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://invalid-test-url.local",
            onOpen, onClosed, onBinary, onMessage, onError);
        
        // Test that WebSocket was created
        Assert.IsNotNull(webSocket);
        Assert.IsFalse(webSocket.isOpen);
    }

    [Test]
    public void WebSocket_AddActions_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
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
        LogAssert.ignoreFailingMessages = true;
        // Test connection to invalid host - should handle gracefully
        bool connected = false;
        bool connected2 = false;
        
        Action<WebSocket> onOpen = (ws) => { connected = true; };
        Action<WebSocket> onOpen2 = (ws) => { connected2 = true; };
        
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { connected = false; };
        Action<WebSocket, ushort, string> onClosed2 = (ws, code, msg) => { connected2 = false; };
        
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, byte[]> onBinary2 = (ws, data) => { };
        
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onMessage2 = (ws, msg) => { };
        
        Action<WebSocket, string> onError = (ws, msg) => { };
        Action<WebSocket, string> onError2 = (ws, msg) => { };

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
        LogAssert.ignoreFailingMessages = true;
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

    [Test]
    public void WebSocket_IsOpen_FalseAfterConstruction()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that isOpen is false immediately after construction
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        Assert.IsFalse(webSocket.isOpen);
    }

    [Test]
    public void WebSocket_SendBinaryData_WithoutConnection_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test sending binary data without an active connection
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        byte[] binaryData = new byte[] { 0xFF, 0xFE, 0xFD, 0x00, 0x01, 0x02, 0x03, 0x04 };

        Assert.DoesNotThrow(() =>
        {
            webSocket.Send(binaryData);
        });
    }

    [Test]
    public void WebSocket_Close_WithoutConnection_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test closing a WebSocket that was never opened
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        Assert.DoesNotThrow(() =>
        {
            webSocket.Close();
        });
    }

    [Test]
    public void WebSocket_LargeMessage_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test sending a large string message without connection
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        // Build a large message (64KB)
        string largeMessage = new string('A', 65536);

        Assert.DoesNotThrow(() =>
        {
            webSocket.Send(largeMessage);
        });
    }

    [Test]
    public void WebSocket_MultipleSequentialSends_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test sending multiple messages sequentially without connection
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        Assert.DoesNotThrow(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                webSocket.Send("message " + i);
            }
        });
    }

    [Test]
    public void WebSocket_AddActions_ReturnTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test that AddOn*Action methods return true when WebSocket is initialized
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        Assert.IsTrue(webSocket.AddOnOpenAction((ws) => { }));
        Assert.IsTrue(webSocket.AddOnClosedAction((ws, code, msg) => { }));
        Assert.IsTrue(webSocket.AddOnBinaryAction((ws, data) => { }));
        Assert.IsTrue(webSocket.AddOnMessageAction((ws, msg) => { }));
        Assert.IsTrue(webSocket.AddOnErrorAction((ws, msg) => { }));
    }

    [UnityTest]
    public IEnumerator WebSocket_OpenAndClose_InvalidHost_HandlesGracefully()
    {
        LogAssert.ignoreFailingMessages = true;
        // Test open then close sequence on invalid host
        bool errorReceived = false;

        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { errorReceived = true; };

        WebSocket webSocket = new WebSocket("wss://invalid-host-for-testing.local",
            onOpen, onClosed, onBinary, onMessage, onError);

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

        // Should not be open after failed connection
        Assert.IsFalse(webSocket.isOpen);

        // Close should not throw even after failed open
        Assert.DoesNotThrow(() =>
        {
            webSocket.Close();
        });
    }

    // ========================================================================
    // Extended WebSocket Tests
    // ========================================================================

    [Test]
    public void WebSocket_MultipleAddOpenActions_AllReturnTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        for (int i = 0; i < 5; i++)
        {
            Assert.IsTrue(webSocket.AddOnOpenAction((ws) => { }));
        }
    }

    [Test]
    public void WebSocket_MultipleAddMessageActions_AllReturnTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        for (int i = 0; i < 5; i++)
        {
            Assert.IsTrue(webSocket.AddOnMessageAction((ws, msg) => { }));
        }
    }

    [Test]
    public void WebSocket_SendEmptyString_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        Assert.DoesNotThrow(() => webSocket.Send(""));
    }

    [Test]
    public void WebSocket_SendEmptyByteArray_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        Assert.DoesNotThrow(() => webSocket.Send(new byte[0]));
    }

    [Test]
    public void WebSocket_MultipleCloseWithoutOpen_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        Assert.DoesNotThrow(() =>
        {
            webSocket.Close();
            webSocket.Close();
        });
    }

    [Test]
    public void WebSocket_LargeBinaryData_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        // 128KB binary payload
        byte[] largeData = new byte[131072];
        for (int i = 0; i < largeData.Length; i++)
        {
            largeData[i] = (byte)(i % 256);
        }

        Assert.DoesNotThrow(() => webSocket.Send(largeData));
    }

    [Test]
    public void WebSocket_AddAllActions_ReturnTrue()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        // Add all action types and verify all return true
        bool openResult = webSocket.AddOnOpenAction((ws) => { });
        bool closedResult = webSocket.AddOnClosedAction((ws, code, msg) => { });
        bool binaryResult = webSocket.AddOnBinaryAction((ws, data) => { });
        bool messageResult = webSocket.AddOnMessageAction((ws, msg) => { });
        bool errorResult = webSocket.AddOnErrorAction((ws, msg) => { });

        Assert.IsTrue(openResult);
        Assert.IsTrue(closedResult);
        Assert.IsTrue(binaryResult);
        Assert.IsTrue(messageResult);
        Assert.IsTrue(errorResult);
    }

    [Test]
    public void WebSocket_IsOpen_RemainsClosedAfterSend()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        WebSocket webSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);

        Assert.IsFalse(webSocket.isOpen);
        webSocket.Send("test");
        Assert.IsFalse(webSocket.isOpen);
        webSocket.Send(new byte[] { 1, 2, 3 });
        Assert.IsFalse(webSocket.isOpen);
    }

    [Test]
    public void WebSocket_DifferentURLSchemes_InitializeCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<WebSocket> onOpen = (ws) => { };
        Action<WebSocket, ushort, string> onClosed = (ws, code, msg) => { };
        Action<WebSocket, byte[]> onBinary = (ws, data) => { };
        Action<WebSocket, string> onMessage = (ws, msg) => { };
        Action<WebSocket, string> onError = (ws, msg) => { };

        // ws:// scheme
        WebSocket wsSocket = new WebSocket("ws://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);
        Assert.IsNotNull(wsSocket);
        Assert.IsFalse(wsSocket.isOpen);

        // wss:// scheme
        WebSocket wssSocket = new WebSocket("wss://test.local",
            onOpen, onClosed, onBinary, onMessage, onError);
        Assert.IsNotNull(wssSocket);
        Assert.IsFalse(wssSocket.isOpen);
    }
}
#endif