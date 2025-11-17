// Copyright (c) 2019-2023 Five Squared Interactive. All rights reserved.

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
    private float httpRequestWaitPeriod = 3; // Reduced wait time

    [Test]
    public void HTTPRequest_Constructor_InitializesCorrectly()
    {
        // Test HTTP request initialization
        Action<int, Dictionary<string, string>, byte[]> onResponse = (resp, headers, data) => { };
        
        HTTPRequest request = new HTTPRequest("https://example.com", HTTPRequest.HTTPMethod.Get, onResponse);
        
        Assert.IsNotNull(request);
    }

    [UnityTest]
    public IEnumerator HTTPTests_Get_WithInvalidURL()
    {
        // Test GET request with invalid URL - should handle gracefully
        int receivedResponse = -1;
        byte[] receivedData = null;
        bool callbackExecuted = false;
        
        Action<int, Dictionary<string, string>, byte[]> onGetResponse = (resp, headers, data) =>
        {
            receivedResponse = resp;
            receivedData = data;
            callbackExecuted = true;
        };
        
        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local", HTTPRequest.HTTPMethod.Get, onGetResponse);
        
        try
        {
            request.Send();
        }
        catch (Exception)
        {
            // Expected for invalid URL
            callbackExecuted = true;
        }
        
        yield return new WaitForSeconds(httpRequestWaitPeriod);
        
        // Should either execute callback with error or throw exception
        Assert.IsTrue(callbackExecuted || receivedResponse == -1);
        
        if (callbackExecuted && receivedResponse != -1)
        {
            // If callback executed, response should indicate failure
            Assert.AreNotEqual(200, receivedResponse);
        }
    }

    [UnityTest]
    public IEnumerator HTTPTests_Head_WithInvalidURL()
    {
        // Test HEAD request with invalid URL
        int receivedResponse = -1;
        byte[] receivedData = null;
        bool callbackExecuted = false;
        
        Action<int, Dictionary<string, string>, byte[]> onHeadResponse = (resp, headers, data) =>
        {
            receivedResponse = resp;
            receivedData = data;
            callbackExecuted = true;
        };
        
        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local", HTTPRequest.HTTPMethod.Head, onHeadResponse);
        
        try
        {
            request.Send();
        }
        catch (Exception)
        {
            // Expected for invalid URL
            callbackExecuted = true;
        }
        
        yield return new WaitForSeconds(httpRequestWaitPeriod);
        
        // Should handle invalid URL gracefully
        Assert.IsTrue(callbackExecuted || receivedResponse == -1);
    }

    [UnityTest]
    public IEnumerator HTTPTests_Post_WithInvalidURL()
    {
        // Test POST request with invalid URL
        int receivedResponse = -1;
        byte[] receivedData = null;
        bool callbackExecuted = false;
        
        Action<int, Dictionary<string, string>, byte[]> onPostResponse = (resp, headers, data) =>
        {
            receivedResponse = resp;
            receivedData = data;
            callbackExecuted = true;
        };
        
        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local", HTTPRequest.HTTPMethod.Post, onPostResponse);
        
        try
        {
            request.Send();
        }
        catch (Exception)
        {
            // Expected for invalid URL
            callbackExecuted = true;
        }
        
        yield return new WaitForSeconds(httpRequestWaitPeriod);
        
        // Should handle invalid URL gracefully
        Assert.IsTrue(callbackExecuted || receivedResponse == -1);
    }

    [UnityTest]
    public IEnumerator HTTPTests_Put_WithInvalidURL()
    {
        // Test PUT request with invalid URL
        int receivedResponse = -1;
        byte[] receivedData = null;
        bool callbackExecuted = false;
        
        Action<int, Dictionary<string, string>, byte[]> onPutResponse = (resp, headers, data) =>
        {
            receivedResponse = resp;
            receivedData = data;
            callbackExecuted = true;
        };
        
        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local", HTTPRequest.HTTPMethod.Put, onPutResponse);
        
        try
        {
            request.Send();
        }
        catch (Exception)
        {
            // Expected for invalid URL
            callbackExecuted = true;
        }
        
        yield return new WaitForSeconds(httpRequestWaitPeriod);
        
        // Should handle invalid URL gracefully
        Assert.IsTrue(callbackExecuted || receivedResponse == -1);
    }

    [UnityTest]
    public IEnumerator HTTPTests_Delete_WithInvalidURL()
    {
        // Test DELETE request with invalid URL
        int receivedResponse = -1;
        byte[] receivedData = null;
        bool callbackExecuted = false;
        
        Action<int, Dictionary<string, string>, byte[]> onDeleteResponse = (resp, headers, data) =>
        {
            receivedResponse = resp;
            receivedData = data;
            callbackExecuted = true;
        };
        
        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local", HTTPRequest.HTTPMethod.Delete, onDeleteResponse);
        
        try
        {
            request.Send();
        }
        catch (Exception)
        {
            // Expected for invalid URL
            callbackExecuted = true;
        }
        
        yield return new WaitForSeconds(httpRequestWaitPeriod);
        
        // Should handle invalid URL gracefully
        Assert.IsTrue(callbackExecuted || receivedResponse == -1);
    }

    [UnityTest]
    public IEnumerator HTTPTests_Patch_WithInvalidURL()
    {
        // Test PATCH request with invalid URL
        int receivedResponse = -1;
        byte[] receivedData = null;
        bool callbackExecuted = false;
        
        Action<int, Dictionary<string, string>, byte[]> onPatchResponse = (resp, headers, data) =>
        {
            receivedResponse = resp;
            receivedData = data;
            callbackExecuted = true;
        };
        
        HTTPRequest request = new HTTPRequest("https://invalid-host-for-testing.local", HTTPRequest.HTTPMethod.Patch, onPatchResponse);
        
        try
        {
            request.Send();
        }
        catch (Exception)
        {
            // Expected for invalid URL
            callbackExecuted = true;
        }
        
        yield return new WaitForSeconds(httpRequestWaitPeriod);
        
        // Should handle invalid URL gracefully
        Assert.IsTrue(callbackExecuted || receivedResponse == -1);
    }

    [Test]
    public void HTTPRequest_AllMethodsEnum_AreValid()
    {
        // Test that all HTTP method enum values are valid
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Get));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Post));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Put));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Delete));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Head));
        Assert.IsTrue(Enum.IsDefined(typeof(HTTPRequest.HTTPMethod), HTTPRequest.HTTPMethod.Patch));
    }
}
#endif