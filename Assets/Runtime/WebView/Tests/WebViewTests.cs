// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using FiveSQD.WebVerse.WebView;

/// <summary>
/// Tests for WebVerseWebView. Uses reflection to set private fields since
/// Initialize() requires WebVerseRuntime singleton. Matches pattern from MemoryCleanupTests.
/// </summary>
public class WebViewTests
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

    /// <summary>
    /// Helper to create a WebVerseWebView with its private webViewObject set via reflection.
    /// </summary>
    private WebVerseWebView CreateWebViewWithObject(out GameObject webViewGO, out GameObject containerGO)
    {
        containerGO = new GameObject("WebViewContainer");
        WebVerseWebView wv = containerGO.AddComponent<WebVerseWebView>();

        webViewGO = new GameObject("WebViewObject");

        // Set private webViewObject field via reflection.
        FieldInfo field = typeof(WebVerseWebView).GetField("webViewObject",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(wv, webViewGO);

        // Set urlsToLoad via reflection.
        FieldInfo urlField = typeof(WebVerseWebView).GetField("urlsToLoad",
            BindingFlags.NonPublic | BindingFlags.Instance);
        urlField.SetValue(wv, new Queue<string>());

        return wv;
    }

    [UnityTest]
    public IEnumerator WebView_ShowHide_TogglesVisibility()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject webViewGO, containerGO;
        WebVerseWebView wv = CreateWebViewWithObject(out webViewGO, out containerGO);
        yield return null;

        wv.Show();
        Assert.IsTrue(webViewGO.activeSelf);
        Assert.IsTrue(wv.IsVisible());

        wv.Hide();
        Assert.IsFalse(webViewGO.activeSelf);
        Assert.IsFalse(wv.IsVisible());

        UnityEngine.Object.DestroyImmediate(webViewGO);
        UnityEngine.Object.DestroyImmediate(containerGO);
    }

    [UnityTest]
    public IEnumerator WebView_IsVisible_FalseWhenHidden()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject webViewGO, containerGO;
        WebVerseWebView wv = CreateWebViewWithObject(out webViewGO, out containerGO);
        yield return null;

        wv.Hide();
        Assert.IsFalse(wv.IsVisible());

        UnityEngine.Object.DestroyImmediate(webViewGO);
        UnityEngine.Object.DestroyImmediate(containerGO);
    }

    [UnityTest]
    public IEnumerator WebView_IsVisible_FalseWhenNoWebViewObject()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("WebViewContainer");
        WebVerseWebView wv = go.AddComponent<WebVerseWebView>();
        yield return null;

        // webViewObject is null by default.
        Assert.IsFalse(wv.IsVisible());

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebView_OnVisibilityChanged_FiresOnShow()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject webViewGO, containerGO;
        WebVerseWebView wv = CreateWebViewWithObject(out webViewGO, out containerGO);
        yield return null;

        bool eventFired = false;
        bool eventValue = false;
        wv.OnVisibilityChanged += (visible) =>
        {
            eventFired = true;
            eventValue = visible;
        };

        wv.Show();
        Assert.IsTrue(eventFired);
        Assert.IsTrue(eventValue);

        UnityEngine.Object.DestroyImmediate(webViewGO);
        UnityEngine.Object.DestroyImmediate(containerGO);
    }

    [UnityTest]
    public IEnumerator WebView_OnVisibilityChanged_FiresOnHide()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject webViewGO, containerGO;
        WebVerseWebView wv = CreateWebViewWithObject(out webViewGO, out containerGO);
        yield return null;

        wv.Show();

        bool eventFired = false;
        bool eventValue = true;
        wv.OnVisibilityChanged += (visible) =>
        {
            eventFired = true;
            eventValue = visible;
        };

        wv.Hide();
        Assert.IsTrue(eventFired);
        Assert.IsFalse(eventValue);

        UnityEngine.Object.DestroyImmediate(webViewGO);
        UnityEngine.Object.DestroyImmediate(containerGO);
    }

    [UnityTest]
    public IEnumerator WebView_Show_WithNullObject_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("WebViewContainer");
        WebVerseWebView wv = go.AddComponent<WebVerseWebView>();
        yield return null;

        // webViewObject is null - Show should log error but not throw.
        wv.Show();

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebView_Hide_WithNullObject_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("WebViewContainer");
        WebVerseWebView wv = go.AddComponent<WebVerseWebView>();
        yield return null;

        // webViewObject is null - Hide should log error but not throw.
        wv.Hide();

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebView_Terminate_ClearsState()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject webViewGO, containerGO;
        WebVerseWebView wv = CreateWebViewWithObject(out webViewGO, out containerGO);
        yield return null;

        wv.Terminate();
        yield return null;

        Assert.IsFalse(wv.IsVisible());

        // webViewObject should be destroyed.
        FieldInfo field = typeof(WebVerseWebView).GetField("webViewObject",
            BindingFlags.NonPublic | BindingFlags.Instance);
        // After Destroy, the reference becomes null in next frame.

        FieldInfo urlField = typeof(WebVerseWebView).GetField("urlsToLoad",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNull(urlField.GetValue(wv));

        UnityEngine.Object.DestroyImmediate(containerGO);
    }

    [UnityTest]
    public IEnumerator WebView_Initialize_WithoutRuntime_SetsUpUrlQueue()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("WebViewContainer");
        WebVerseWebView wv = go.AddComponent<WebVerseWebView>();
        yield return null;

        // Initialize without WebVerseRuntime should still set up urlsToLoad.
        wv.Initialize();

        FieldInfo urlField = typeof(WebVerseWebView).GetField("urlsToLoad",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(urlField.GetValue(wv));

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebView_SetupVRMode_WithNullObject_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject go = new GameObject("WebViewContainer");
        WebVerseWebView wv = go.AddComponent<WebVerseWebView>();
        yield return null;

        // webViewObject is null - should log error but not throw.
        wv.SetupVRMode(go.transform);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator WebView_SetupVRMode_WithNullTransform_DoesNotThrow()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject webViewGO, containerGO;
        WebVerseWebView wv = CreateWebViewWithObject(out webViewGO, out containerGO);
        yield return null;

        // Null multibar transform - should log error but not throw.
        wv.SetupVRMode(null);

        UnityEngine.Object.DestroyImmediate(webViewGO);
        UnityEngine.Object.DestroyImmediate(containerGO);
    }

    [UnityTest]
    public IEnumerator WebView_SetupVRMode_ParentsToMultibar()
    {
        LogAssert.ignoreFailingMessages = true;
        GameObject webViewGO, containerGO;
        WebVerseWebView wv = CreateWebViewWithObject(out webViewGO, out containerGO);
        // Add Canvas component (required for VR mode setup).
        webViewGO.AddComponent<Canvas>();
        webViewGO.AddComponent<GraphicRaycaster>();
        yield return null;

        GameObject multibar = new GameObject("Multibar");
        wv.SetupVRMode(multibar.transform);

        Assert.AreEqual(multibar.transform, webViewGO.transform.parent);

        UnityEngine.Object.DestroyImmediate(multibar);
        UnityEngine.Object.DestroyImmediate(containerGO);
    }
}
