// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.File;
using FiveSQD.WebVerse.Utilities;
using System;
using UnityEngine;
using FiveSQD.WebVerse.Runtime;
using System.Collections.Generic;
#if USE_WEBINTERFACE
using FiveSQD.WebVerse.WebInterface.HTTP;
#endif

namespace FiveSQD.WebVerse.Handlers.Image
{
    /// <summary>
    /// Class for the Image Handler.
    /// </summary>
    public class ImageHandler : BaseHandler
    {
        /// <summary>
        /// Reference to the WebVerse Runtime.
        /// </summary>
        public WebVerseRuntime runtime;

        /// <summary>
        /// Initialize the Image Handler.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Terminate the Image Handler.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();
        }

        /// <summary>
        /// Load an image resource as a Texture2D.
        /// </summary>
        /// <param name="resourceURI">URI to get the resource from.</param>
        /// <param name="onLoaded">Action to perform when loading is complete. Provides the loaded
        /// resource as a Texture2D.</param>
        /// <param name="format">Format to use.</param>
        public void LoadImageResourceAsTexture2D(string resourceURI, Action<Texture2D> onLoaded, TextureFormat? format = null)
        {
            Action onDownloaded = () =>
            {
                Texture2D img = LoadImage(System.IO.Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(resourceURI)), format);
                onLoaded.Invoke(img);
            };
            DownloadImage(resourceURI, onDownloaded);
        }

        /// <summary>
        /// Download an image.
        /// </summary>
        /// <param name="uri">URI to get the image from.</param>
        /// <param name="onDownloaded">Action to perform when downloading is complete.</param>
        public void DownloadImage(string uri, Action onDownloaded)
        {
            uri = uri.Replace("\\", "/");
#if USE_WEBINTERFACE
            Action<int, Dictionary<string, string>, Texture2D> onDownloadedAction
            = new Action<int, Dictionary<string, string>, Texture2D>((code, headers, data) =>
            {
                FinishImageDownload(uri, code, data);
                onDownloaded.Invoke();
            });

            HTTPRequest request = new HTTPRequest(uri, HTTPRequest.HTTPMethod.Get, onDownloadedAction);

            if (runtime.fileHandler.FileExistsInFileDirectory(FileHandler.ToFileURI(uri)))
            {
                if (uri.StartsWith("file:/") || uri.StartsWith("/") || uri.StartsWith(".") || uri[1] == ':')
                {
                    Logging.Log("[ImageHandler->DownloadImage] File " + uri + " already exists. Using stored version.");
                    onDownloaded.Invoke();
                    return;
                }
                Logging.Log("[ImageHandler->DownloadImage] File " + uri + " already exists. Checking for newer version.");

                Action<int, Dictionary<string, string>, byte[]> onResponseAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        if (header.Key.ToLower() == "last-modified")
                        {
                            DateTime timestamp;
                            if (DateTime.TryParse(header.Value, out timestamp))
                            {
                                if (timestamp > System.IO.File.GetLastWriteTime(System.IO.Path.Combine(
                                    runtime.fileHandler.fileDirectory, FileHandler.ToFileURI(uri))))
                                {
                                    Logging.Log("[ImageHandler->DownloadImage] Cached version of file " + uri + " is outdated. Getting new version.");
                                }
                                else
                                {
                                    Logging.Log("[ImageHandler->DownloadImage] Cached version of file " + uri + " is current. Using stored version.");
                                    onDownloaded.Invoke();
                                    return;
                                }
                            }
                        }
                    }
                    Logging.Log("[ImageHandler->DownloadImage] Getting " + uri + ".");
                    request.Send();
                });
                HTTPRequest headRequest = new HTTPRequest(uri, HTTPRequest.HTTPMethod.Head, onResponseAction);
                headRequest.Send();
            }
            else
            {
                request.Send();
            }
#endif
        }

        /// <summary>
        /// Load an image from a path.
        /// </summary>
        /// <param name="path">Path to load the image from.</param>
        /// <param name="format">Format to use.</param>
        /// <returns>A loaded Texture2D, or null.</returns>
        public Texture2D LoadImage(string path, TextureFormat? format = null)
        {
            byte[] rawData = System.IO.File.ReadAllBytes(path);
            Texture2D texture;
            if (format.HasValue)
            {
                texture = new Texture2D(2, 2, format.Value, true);
                texture.LoadImage(rawData);

                // It seems that the texture needs to be copied to actually set the texture format.
                if (format.Value != TextureFormat.ARGB32 && format.Value != TextureFormat.RGBA32)
                {
                    Color[] pixels = texture.GetPixels();
                    texture.SetPixels(pixels);
                    Texture2D copyTexture = new Texture2D(texture.width, texture.height, format.Value, true);
                    copyTexture.SetPixels(pixels);
                    copyTexture.Apply();
                    texture = copyTexture;
                }
            }
            else
            {
                texture = new Texture2D(2, 2);
                texture.LoadImage(rawData);
            }
            
            return texture;
        }

        /// <summary>
        /// Finish the downloading of an image.
        /// </summary>
        /// <param name="uri">URI of the Image.</param>
        /// <param name="responseCode">Response code from the download.</param>
        /// <param name="rawImage">The raw image that was downloaded.</param>
        private void FinishImageDownload(string uri, int responseCode, Texture2D rawImage)
        {
            Logging.Log("[ImageHandler->FinishImageDownload] Got response " + responseCode + " for request " + uri);

            if (responseCode != 200)
            {
                Logging.Log("[ImageHandler->FinishImageDownload] Error loading file.");
                return;
            }

            string filePath = FileHandler.ToFileURI(uri);
            if (runtime.fileHandler.FileExistsInFileDirectory(filePath))
            {
                runtime.fileHandler.DeleteFileInFileDirectory(filePath);
            }
            runtime.fileHandler.CreateFileInFileDirectory(filePath, DuplicateTexture(rawImage));
        }

        /// <summary>
        /// Duplicate a texture.
        /// </summary>
        /// <param name="source">Source texture.</param>
        /// <returns>The duplicated texture.</returns>
        private Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}