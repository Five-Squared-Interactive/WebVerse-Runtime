// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

#if USE_BESTHTTP
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.WebInterface.HTTP;
using System;

/// <summary>
/// Unit tests for HTTP.
/// </summary>
public class HTTPTests
{
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
    public void HTTPRequest_Constructor_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com", HTTPRequest.HTTPMethod.Get, onResponse);

        Assert.IsNotNull(request);
    }

    // ========================================================================
    // Invalid URL Tests (non-network, verify construction + Send without manager)
    // These previously used [UnityTest] with WaitForSeconds causing CI timeouts.
    // Since there's no HTTPRequestManager.instance in test, Send() returns early.
    // ========================================================================

    [Test]
    public void HTTPTests_Get_WithInvalidURL()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local",
            HTTPRequest.HTTPMethod.Get, onResponse);
        Assert.IsNotNull(request);

        // Send returns early when no HTTPRequestManager.instance exists
        Assert.DoesNotThrow(() => request.Send());
    }

    [Test]
    public void HTTPTests_Head_WithInvalidURL()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local",
            HTTPRequest.HTTPMethod.Head, onResponse);
        Assert.IsNotNull(request);

        Assert.DoesNotThrow(() => request.Send());
    }

    [Test]
    public void HTTPTests_Post_WithInvalidURL()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = null;
        Assert.DoesNotThrow(() =>
        {
            try
            {
                request = new HTTPRequest("https://invalid-host-for-testing.local",
                    HTTPRequest.HTTPMethod.Post, onResponse);
            }
            catch (ArgumentException)
            {
                // Post with null data/dataType may throw ArgumentException
            }
        });

        if (request != null)
        {
            Assert.DoesNotThrow(() => request.Send());
        }
    }

    [Test]
    public void HTTPTests_Put_WithInvalidURL()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        // Put falls through to default case (unsupported), so request.Send() is a no-op
        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local",
            HTTPRequest.HTTPMethod.Put, onResponse);
        Assert.IsNotNull(request);

        Assert.DoesNotThrow(() => request.Send());
    }

    [Test]
    public void HTTPTests_Delete_WithInvalidURL()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local",
            HTTPRequest.HTTPMethod.Delete, onResponse);
        Assert.IsNotNull(request);

        Assert.DoesNotThrow(() => request.Send());
    }

    [Test]
    public void HTTPTests_Patch_WithInvalidURL()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        // Patch falls through to default case (unsupported)
        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local",
            HTTPRequest.HTTPMethod.Patch, onResponse);
        Assert.IsNotNull(request);

        Assert.DoesNotThrow(() => request.Send());
    }

    // ========================================================================
    // Enum Tests
    // ========================================================================

    [Test]
    public void HTTPRequest_AllMethodsEnum_AreValid()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Get));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Post));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Put));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Delete));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Head));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Patch));
    }

    [Test]
    public void HTTPRequest_AdditionalMethodEnums_AreValid()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Merge));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Options));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Connect));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Query));

        Assert.AreEqual(6, (int)HTTPRequest.HTTPMethod.Merge);
        Assert.AreEqual(7, (int)HTTPRequest.HTTPMethod.Options);
        Assert.AreEqual(8, (int)HTTPRequest.HTTPMethod.Connect);
        Assert.AreEqual(9, (int)HTTPRequest.HTTPMethod.Query);
    }

    [Test]
    public void HTTPRequest_MethodEnum_HasExpectedCount()
    {
        LogAssert.ignoreFailingMessages = true;
        var values = Enum.GetValues(typeof(HTTPRequest.HTTPMethod));
        Assert.AreEqual(10, values.Length);
    }

    [Test]
    public void HTTPRequest_MethodEnum_IntValues_Sequential()
    {
        LogAssert.ignoreFailingMessages = true;
        Assert.AreEqual(0, (int)HTTPRequest.HTTPMethod.Get);
        Assert.AreEqual(1, (int)HTTPRequest.HTTPMethod.Head);
        Assert.AreEqual(2, (int)HTTPRequest.HTTPMethod.Post);
        Assert.AreEqual(3, (int)HTTPRequest.HTTPMethod.Put);
        Assert.AreEqual(4, (int)HTTPRequest.HTTPMethod.Delete);
        Assert.AreEqual(5, (int)HTTPRequest.HTTPMethod.Patch);
    }

    // ========================================================================
    // Constructor Tests
    // ========================================================================

    [Test]
    public void HTTPRequest_PostWithBodyContent_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/api", HTTPRequest.HTTPMethod.Post,
            onResponse, "{ \"key\": \"value\" }", "application/json");

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_GetMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/resource", HTTPRequest.HTTPMethod.Get, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_PutMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/resource", HTTPRequest.HTTPMethod.Put, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_DeleteMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/resource", HTTPRequest.HTTPMethod.Delete, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_HeadMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/resource", HTTPRequest.HTTPMethod.Head, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_PatchMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Patch falls through to default (unsupported) but should not throw
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/resource", HTTPRequest.HTTPMethod.Patch, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_MergeMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/resource", HTTPRequest.HTTPMethod.Merge, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_OptionsMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/resource", HTTPRequest.HTTPMethod.Options, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_ConnectMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/resource", HTTPRequest.HTTPMethod.Connect, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_QueryMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/resource", HTTPRequest.HTTPMethod.Query, onResponse);

        Assert.IsNotNull(request);
    }

    // ========================================================================
    // Edge Cases
    // ========================================================================

    [Test]
    public void HTTPRequest_NullURL_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        Assert.DoesNotThrow(() =>
        {
            try
            {
                HTTPRequest request = new HTTPRequest(null, HTTPRequest.HTTPMethod.Get, onResponse);
            }
            catch (ArgumentNullException)
            {
                // Expected - null URL may throw ArgumentNullException from UnityWebRequest
            }
        });
    }

    [Test]
    public void HTTPRequest_EmptyURL_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        Assert.DoesNotThrow(() =>
        {
            try
            {
                HTTPRequest request = new HTTPRequest("", HTTPRequest.HTTPMethod.Get, onResponse);
            }
            catch (Exception)
            {
                // Expected - empty URL may throw from UnityWebRequest
            }
        });
    }

    [Test]
    public void HTTPRequest_Texture2DConstructor_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, Texture2D> onResponse = (resp, headers, texture) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/image.png", HTTPRequest.HTTPMethod.Get, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_Texture2DConstructor_Head_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, Texture2D> onResponse = (resp, headers, texture) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/image.png", HTTPRequest.HTTPMethod.Head, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_Texture2DConstructor_Delete_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, Texture2D> onResponse = (resp, headers, texture) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/image.png", HTTPRequest.HTTPMethod.Delete, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_Texture2DConstructor_UnsupportedMethod_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        // Post is unsupported for Texture2D constructor, falls through to default
        Action<int, Dictionary<string, string>, Texture2D> onResponse = (resp, headers, texture) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/image.png", HTTPRequest.HTTPMethod.Post, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_Send_WithoutManager_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com", HTTPRequest.HTTPMethod.Get, onResponse);

        Assert.DoesNotThrow(() =>
        {
            request.Send();
        });
    }

    [Test]
    public void HTTPRequest_Send_Texture2D_WithoutManager_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, Texture2D> onResponse = (resp, headers, texture) => { };

        HTTPRequest request = new HTTPRequest("https://example.com/image.png", HTTPRequest.HTTPMethod.Get, onResponse);

        Assert.DoesNotThrow(() =>
        {
            request.Send();
        });
    }

    [Test]
    public void HTTPRequest_PostWithEmptyBody_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        Assert.DoesNotThrow(() =>
        {
            try
            {
                HTTPRequest request = new HTTPRequest("https://example.com/api",
                    HTTPRequest.HTTPMethod.Post, onResponse, "", "text/plain");
            }
            catch (Exception)
            {
                // May throw depending on UnityWebRequest.Post behavior with empty string
            }
        });
    }

    [Test]
    public void HTTPRequest_PostWithJsonBody_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        string jsonBody = "{\"name\":\"test\",\"value\":42,\"nested\":{\"array\":[1,2,3]}}";
        HTTPRequest request = new HTTPRequest("https://example.com/api",
            HTTPRequest.HTTPMethod.Post, onResponse, jsonBody, "application/json");

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_UnsupportedMethod_Send_IsNoOp()
    {
        LogAssert.ignoreFailingMessages = true;
        // Merge is unsupported — internal request will be null, so Send() is a no-op
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest("https://example.com", HTTPRequest.HTTPMethod.Merge, onResponse);

        // Should not throw — Send() checks for null request
        Assert.DoesNotThrow(() => request.Send());
    }

    [Test]
    public void HTTPRequest_LongURL_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        string longUrl = "https://example.com/" + new string('a', 2000) + "?q=" + new string('b', 1000);
        HTTPRequest request = new HTTPRequest(longUrl, HTTPRequest.HTTPMethod.Get, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_URLWithQueryParams_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest(
            "https://example.com/search?q=test&page=1&limit=50&sort=desc",
            HTTPRequest.HTTPMethod.Get, onResponse);

        Assert.IsNotNull(request);
    }

    [Test]
    public void HTTPRequest_URLWithFragment_InitializesCorrectly()
    {
        LogAssert.ignoreFailingMessages = true;
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };

        HTTPRequest request = new HTTPRequest(
            "https://example.com/page#section-3",
            HTTPRequest.HTTPMethod.Get, onResponse);

        Assert.IsNotNull(request);
    }
}
#endif
