// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.IO;
using FiveSQD.WebVerse.Handlers.File;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.X3D.Abstraction;
using UnityEngine;
using X3D;
#if USE_WEBINTERFACE
using FiveSQD.WebVerse.WebInterface.HTTP;
#endif

namespace FiveSQD.WebVerse.Handlers.X3D
{
    /// <summary>
    /// Handler for loading and processing X3D documents.
    /// </summary>
    public class X3DHandler : BaseHandler
    {
        /// <summary>
        /// Reference to the WebVerse runtime.
        /// </summary>
        [Tooltip("Reference to the WebVerse runtime.")]
        public WebVerseRuntime runtime;

        /// <summary>
        /// Timeout for HTTP requests in seconds.
        /// </summary>
        [Tooltip("Timeout for HTTP requests in seconds.")]
        public float timeout = 30f;

        /// <summary>
        /// The world adapter used by this handler.
        /// </summary>
        private IX3DWorldAdapter worldAdapter;

        /// <summary>
        /// The world builder instance.
        /// </summary>
        private X3DWorldBuilder worldBuilder;

        /// <summary>
        /// Initialize the X3D Handler.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            Logging.Log("[X3DHandler] Initialized.");
        }

        /// <summary>
        /// Set the world adapter. This must be called before loading X3D documents.
        /// </summary>
        /// <param name="adapter">The world adapter implementation.</param>
        public void SetWorldAdapter(IX3DWorldAdapter adapter)
        {
            if (adapter == null)
            {
                Logging.LogError("[X3DHandler] Cannot set null world adapter.");
                return;
            }

            worldAdapter = adapter;
            worldBuilder = new X3DWorldBuilder(adapter);

            Logging.Log("[X3DHandler] World adapter set.");
        }

        /// <summary>
        /// Terminate the X3D Handler.
        /// </summary>
        public override void Terminate()
        {
            worldAdapter = null;
            worldBuilder = null;
            base.Terminate();
            Logging.Log("[X3DHandler] Terminated.");
        }

        /// <summary>
        /// Load an X3D document from a URI into the current world.
        /// </summary>
        /// <param name="resourceURI">URI of the X3D file (http/https or local file).</param>
        /// <param name="onComplete">Callback when loading is complete. Provides success/failure.</param>
        public void LoadX3DDocumentIntoWorld(string resourceURI, Action<bool> onComplete)
        {
            if (!ValidateHandler(onComplete))
                return;

            Logging.Log($"[X3DHandler] Loading X3D from: {resourceURI}");

            StartCoroutine(LoadX3DDocumentCoroutine(resourceURI, onComplete));
        }

        /// <summary>
        /// Load an X3D document from a string into the current world.
        /// </summary>
        /// <param name="x3dContent">X3D XML content as string.</param>
        /// <param name="baseUrl">Base URL for resolving relative paths.</param>
        /// <param name="onComplete">Callback when loading is complete.</param>
        public void LoadX3DFromString(string x3dContent, string baseUrl, Action<bool> onComplete)
        {
            if (!ValidateHandler(onComplete))
                return;

            Logging.Log("[X3DHandler] Loading X3D from string.");

            try
            {
                // Use the existing X3D parser
                X3DNode rootNode = X3DParser.Parse(x3dContent);

                if (rootNode == null)
                {
                    Logging.LogError("[X3DHandler] Failed to parse X3D content.");
                    onComplete?.Invoke(false);
                    return;
                }

                // Extract version if available
                string version = "unknown";
                if (rootNode.Attributes.TryGetValue("version", out string ver))
                    version = ver;
                Logging.Log($"[X3DHandler] Parsed X3D version: {version}");

                // Build the world using the adapter
                worldBuilder.BuildFromNodes(rootNode, () => {
                    Logging.Log("[X3DHandler] World building complete.");
                    onComplete?.Invoke(true);
                });
            }
            catch (Exception ex)
            {
                Logging.LogError($"[X3DHandler] Error loading X3D: {ex.Message}");
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// Coroutine to load X3D document from URI.
        /// </summary>
        private IEnumerator LoadX3DDocumentCoroutine(string resourceURI, Action<bool> onComplete)
        {
            // Determine if this is a remote or local file
            bool isRemote = resourceURI.StartsWith("http://") || resourceURI.StartsWith("https://");
            string baseUrl = GetBaseUrl(resourceURI);

            if (isRemote)
            {
                yield return StartCoroutine(LoadFromRemote(resourceURI, baseUrl, onComplete));
            }
            else
            {
                yield return StartCoroutine(LoadFromLocal(resourceURI, baseUrl, onComplete));
            }
        }

        /// <summary>
        /// Load X3D from a remote URL.
        /// </summary>
        private IEnumerator LoadFromRemote(string url, string baseUrl, Action<bool> onComplete)
        {
#if USE_WEBINTERFACE
            bool downloadComplete = false;
            string x3dContent = null;

            Action<int, System.Collections.Generic.Dictionary<string, string>, byte[]> onDownloaded =
                (code, headers, data) =>
                {
                    downloadComplete = true;
                    if (code >= 200 && code < 300 && data != null)
                    {
                        x3dContent = System.Text.Encoding.UTF8.GetString(data);
                    }
                    else
                    {
                        Logging.LogError($"[X3DHandler] Failed to download X3D. HTTP {code}");
                    }
                };

            HTTPRequest request = new HTTPRequest(url, HTTPRequest.HTTPMethod.Get, onDownloaded);
            request.Send();

            // Wait for download
            float elapsed = 0f;
            while (!downloadComplete && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!downloadComplete)
            {
                Logging.LogError("[X3DHandler] X3D download timed out.");
                onComplete?.Invoke(false);
                yield break;
            }

            if (string.IsNullOrEmpty(x3dContent))
            {
                onComplete?.Invoke(false);
                yield break;
            }

            LoadX3DFromString(x3dContent, baseUrl, onComplete);
#else
            Logging.LogError("[X3DHandler] Web interface not available for remote loading.");
            onComplete?.Invoke(false);
            yield break;
#endif
        }

        /// <summary>
        /// Load X3D from a local file.
        /// </summary>
        private IEnumerator LoadFromLocal(string filePath, string baseUrl, Action<bool> onComplete)
        {
            // Resolve the file path
            string resolvedPath = filePath;

            // Check if this is a relative path that needs resolution
            if (runtime != null && runtime.fileHandler != null)
            {
                string fileHandlerPath = Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(filePath));
                if (System.IO.File.Exists(fileHandlerPath))
                {
                    resolvedPath = fileHandlerPath;
                }
            }

            // Try to load the file
            if (!System.IO.File.Exists(resolvedPath))
            {
                Logging.LogError($"[X3DHandler] File not found: {resolvedPath}");
                onComplete?.Invoke(false);
                yield break;
            }

            string x3dContent = null;
            try
            {
                x3dContent = System.IO.File.ReadAllText(resolvedPath);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[X3DHandler] Error reading file: {ex.Message}");
                onComplete?.Invoke(false);
                yield break;
            }

            if (string.IsNullOrEmpty(x3dContent))
            {
                Logging.LogError("[X3DHandler] File is empty.");
                onComplete?.Invoke(false);
                yield break;
            }

            // If baseUrl wasn't provided, derive it from file path
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = Path.GetDirectoryName(resolvedPath);
            }

            LoadX3DFromString(x3dContent, baseUrl, onComplete);
            yield return null;
        }

        /// <summary>
        /// Get the base URL from a resource URI (directory containing the file).
        /// </summary>
        private string GetBaseUrl(string resourceURI)
        {
            if (string.IsNullOrEmpty(resourceURI))
                return "";

            // For URLs
            if (resourceURI.StartsWith("http://") || resourceURI.StartsWith("https://"))
            {
                int lastSlash = resourceURI.LastIndexOf('/');
                if (lastSlash > 8) // After "https://"
                {
                    return resourceURI.Substring(0, lastSlash + 1);
                }
            }

            // For file paths
            return Path.GetDirectoryName(resourceURI) ?? "";
        }

        /// <summary>
        /// Validate that the handler is ready to process X3D.
        /// </summary>
        private bool ValidateHandler(Action<bool> onComplete)
        {
            if (worldAdapter == null)
            {
                Logging.LogError("[X3DHandler] World adapter not set. Call SetWorldAdapter first.");
                onComplete?.Invoke(false);
                return false;
            }

            if (worldBuilder == null)
            {
                Logging.LogError("[X3DHandler] World builder not initialized.");
                onComplete?.Invoke(false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the handler has a world adapter set.
        /// </summary>
        public bool HasWorldAdapter => worldAdapter != null;

        /// <summary>
        /// Get X3D version from a document URI.
        /// </summary>
        /// <param name="resourceURI">URI of the X3D file.</param>
        /// <param name="onComplete">Callback with version string.</param>
        public void GetX3DVersion(string resourceURI, Action<string> onComplete)
        {
            StartCoroutine(GetVersionCoroutine(resourceURI, onComplete));
        }

        private IEnumerator GetVersionCoroutine(string resourceURI, Action<string> onComplete)
        {
            string x3dContent = null;

            if (System.IO.File.Exists(resourceURI))
            {
                try
                {
                    x3dContent = System.IO.File.ReadAllText(resourceURI);
                }
                catch
                {
                    onComplete?.Invoke("unknown");
                    yield break;
                }
            }
            else
            {
                // Would need to download - simplified for now
                onComplete?.Invoke("unknown");
                yield break;
            }

            X3DNode rootNode = X3DParser.Parse(x3dContent);
            
            // Extract version from X3D root node attributes
            string version = "unknown";
            if (rootNode != null && rootNode.Attributes.TryGetValue("version", out string ver))
            {
                version = ver;
            }
            onComplete?.Invoke(version);
        }
    }
}
