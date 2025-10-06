// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.File;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System;
#if USE_WEBINTERFACE
using FiveSQD.WebVerse.WebInterface.HTTP;
#endif
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0;
using FiveSQD.WebVerse.VOSSynchronization;
using FiveSQD.StraightFour.Utilities;
using FiveSQD.StraightFour.Entity;
using System.Xml;

namespace FiveSQD.WebVerse.Handlers.VEML
{
    /// <summary>
    /// Class for the VEML Handler.
    /// </summary>
    public class VEMLHandler : BaseHandler
    {
        /// <summary>
        /// Enumeration for a VEML version number.
        /// </summary>
        public enum VEMLVersion { Unknown, v1_0, v1_1, v1_2, v1_3, v2_0, v2_1, V2_2, V2_3, V2_4, V3_0 }

        /// <summary>
        /// Reference to the WebVerse runtime.
        /// </summary>
        public WebVerseRuntime runtime;

        /// <summary>
        /// Timeout for requests.
        /// </summary>
        public float timeout = 10;

        /// <summary>
        /// Number of entities that are being loaded.
        /// </summary>
        private int loadingEntities = 0;
        
        private liteproceduralsky liteProceduralSkyToLoadOnLoadCompletion = null;

        /// <summary>
        /// Avatar entity tag to be set after entities are loaded.
        /// </summary>
        private string pendingAvatarEntityTag = null;

        /// <summary>
        /// Rig offset to be set after entities are loaded.
        /// </summary>
        private string pendingRigOffset = null;

        /// <summary>
        /// Initialize the VEML Handler.
        /// </summary>
        public override void Initialize()
        {
            liteProceduralSkyToLoadOnLoadCompletion = null;
            pendingAvatarEntityTag = null;
            pendingRigOffset = null;
            base.Initialize();
        }

        /// <summary>
        /// Terminate the VEML Handler.
        /// </summary>
        public override void Terminate()
        {
            liteProceduralSkyToLoadOnLoadCompletion = null;
            base.Terminate();
        }

        /// <summary>
        /// Get the name of a world.
        /// </summary>
        /// <param name="resourceURI">URI of the world file.</param>
        /// <param name="onComplete">Action to invoke when the name has been determined. Provides
        /// a string containing the world name.</param>
        public void GetWorldName(string resourceURI, Action<string> onComplete)
        {
            Action onDownloaded = () =>
            {
                Schema.V3_0.veml veml = LoadVEML(Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(resourceURI)));

                if (veml == null)
                {
                    Logging.Log("[VEMLHandler->GetWorldName] Not a VEML document: "
                        + resourceURI);
                    onComplete.Invoke("ERROR");
                }
                else
                {
                    if (veml.metadata == null)
                    {
                        Logging.Log("[VEMLHandler->GetWorldName] No metadata: "
                            + resourceURI);
                        onComplete.Invoke("ERROR");
                    }
                    else
                    {
                        onComplete.Invoke(veml.metadata.title);
                    }
                }
            };
            DownloadVEML(resourceURI, onDownloaded);
        }

        /// <summary>
        /// Initiate the loading of a VEML document into the current world.
        /// </summary>
        /// <param name="resourceURI">URI of the world file.</param>
        /// <param name="onComplete">Action to invoke upon completion of world loading.
        /// Provides a success/fail indication.</param>
        public void LoadVEMLDocumentIntoWorld(string resourceURI, Action<bool> onComplete)
        {
            Action onDownloaded = () =>
            {
                Schema.V3_0.veml veml = LoadVEML(Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(resourceURI)));

                if (veml == null)
                {
                    Logging.Log("[VEMLHandler->LoadVEMLDocumentIntoWorld] Not a VEML document: "
                        + resourceURI);
                }
                else
                {
                    StartCoroutine(ApplyVEMLDocument(veml, Path.GetDirectoryName(resourceURI), onComplete));
                }
            };
            DownloadVEML(resourceURI, onDownloaded);
        }

        /// <summary>
        /// Download a VEML document.
        /// </summary>
        /// <param name="uri">URI of the VEML file.</param>
        /// <param name="onDownloaded">Action to invoke when the file has been downloaded.</param>
        public void DownloadVEML(string uri, Action onDownloaded)
        {
#if USE_WEBINTERFACE
            Action<int, Dictionary<string, string>, byte[]> onDownloadedAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
            {
                FinishVEMLDownload(uri, code, data);
                onDownloaded.Invoke();
            });

            HTTPRequest request = new HTTPRequest(uri, HTTPRequest.HTTPMethod.Get, onDownloadedAction);

            if (runtime.fileHandler.FileExistsInFileDirectory(FileHandler.ToFileURI(uri)))
            {
                if (uri.StartsWith("file:/") || uri.StartsWith("/") || uri.StartsWith(".") || uri[1] == ':')
                {
                    Logging.Log("[VEMLHandler->DownloadVEML] File " + uri + " already exists. Using stored version.");
                    onDownloaded.Invoke();
                    return;
                }
                Logging.Log("[VEMLHandler->DownloadVEML] File " + uri + " already exists. Checking for newer version.");

                Action<int, Dictionary<string, string>, byte[]> onResponseAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
                {
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> header in headers)
                        {
                            if (header.Key.ToLower() == "last-modified")
                            {
                                DateTime timestamp;
                                if (DateTime.TryParse(header.Value, out timestamp))
                                {
                                    if (timestamp > System.IO.File.GetLastWriteTime(Path.Combine(
                                        runtime.fileHandler.fileDirectory, FileHandler.ToFileURI(uri))))
                                    {
                                        Logging.Log("[VEMLHandler->DownloadVEML] Cached version of file " + uri + " is outdated. Getting new version.");
                                    }
                                    else
                                    {
                                        Logging.Log("[VEMLHandler->DownloadVEML] Cached version of file " + uri + " is current. Using stored version.");
                                        onDownloaded.Invoke();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    Logging.Log("[VEMLHandler->DownloadVEML] Getting " + uri + ".");
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
        /// Load a VEML document from a path.
        /// </summary>
        /// <param name="path">Path to load the document from.</param>
        /// <returns>A loaded VEML class, or null.</returns>
        public Schema.V3_0.veml LoadVEML(string path)
        {
            byte[] rawData = System.IO.File.ReadAllBytes(path);

            //VEMLVersion version = VEMLVersion.V3_0;
            Schema.V3_0.veml veml = null;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Schema.V3_0.veml));
                TextReader reader = new StringReader(VEMLUtilities.FullyNotateVEML3_0(System.Text.Encoding.UTF8.GetString(rawData)));
                XmlReader xmlReader = XmlReader.Create(reader);

                // Attempt deserialization using v3.0 (latest) of the schema. Old XML will be converted
                // to current version before being deserialized.
                if (ser.CanDeserialize(xmlReader) && !VEMLUtilities.IsPreVEML3_0(System.Text.Encoding.UTF8.GetString(rawData)))
                {
                    //version = VEMLVersion.V3_0;
                    Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v3.0");
                    veml = (Schema.V3_0.veml) ser.Deserialize(xmlReader);
                }

                // Attempt deserialization using v2.4, v2.3, v2.2, v2.1, v2.0, v1.3, v1.2, v1.1, or v1.0 of the schema. Convert to
                // latest if deserialization succeeds.
                else
                {
                    XmlSerializer ser2_4 = new XmlSerializer(typeof(Schema.V2_4.veml));
                    TextReader reader2_4 = new StringReader(VEMLUtilities.FullyNotateVEML2_4(System.Text.Encoding.UTF8.GetString(rawData)));
                    XmlReader xmlReader2_4 = XmlReader.Create(reader2_4);
                    XmlSerializer ser2_3 = new XmlSerializer(typeof(Schema.V2_3.veml));
                    XmlSerializer ser2_2 = new XmlSerializer(typeof(Schema.V2_2.veml));
                    XmlSerializer ser2_1 = new XmlSerializer(typeof(Schema.V2_1.veml));
                    XmlSerializer ser2_0 = new XmlSerializer(typeof(Schema.V2_0.veml));
                    XmlSerializer ser1_3 = new XmlSerializer(typeof(Schema.V1_3.veml));
                    XmlSerializer ser1_2 = new XmlSerializer(typeof(Schema.V1_2.veml));
                    XmlSerializer ser1_1 = new XmlSerializer(typeof(Schema.V1_1.veml));
                    XmlSerializer ser1_0 = new XmlSerializer(typeof(Schema.V1_0.veml));

                    if (ser2_4.CanDeserialize(xmlReader2_4))
                    {
                        //version = VEMLVersion.v2_4;
                        Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v2.4. Upgrading to VEML v3.0.");
                        reader = new StringReader(VEMLUtilities.FullyNotateVEML2_4(System.Text.Encoding.UTF8.GetString(rawData)));
                        xmlReader = XmlReader.Create(reader);
                        Schema.V2_4.veml v2_4VEML = (Schema.V2_4.veml) ser2_4.Deserialize(xmlReader);
                        veml = VEMLUtilities.ConvertFromV2_4(v2_4VEML);
                    }
                    else if (ser2_3.CanDeserialize(xmlReader))
                    {
                        //version = VEMLVersion.v2_3;
                        Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v2.3. Upgrading to VEML v3.0.");
                        reader = new StringReader(VEMLUtilities.FullyNotateVEML2_3(System.Text.Encoding.UTF8.GetString(rawData)));
                        xmlReader = XmlReader.Create(reader);
                        Schema.V2_3.veml v2_3VEML = (Schema.V2_3.veml)ser2_3.Deserialize(xmlReader);
                        veml = VEMLUtilities.ConvertFromV2_3(v2_3VEML);
                    }
                    else if (ser2_2.CanDeserialize(xmlReader))
                    {
                        //version = VEMLVersion.v2_2;
                        Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v2.2. Upgrading to VEML v3.0.");
                        reader = new StringReader(VEMLUtilities.FullyNotateVEML2_2(System.Text.Encoding.UTF8.GetString(rawData)));
                        xmlReader = XmlReader.Create(reader);
                        Schema.V2_2.veml v2_2VEML = (Schema.V2_2.veml)ser2_2.Deserialize(xmlReader);
                        veml = VEMLUtilities.ConvertFromV2_2(v2_2VEML);
                    }
                    else if (ser2_1.CanDeserialize(xmlReader))
                    {
                        //version = VEMLVersion.v2_1;
                        Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v2.1. Upgrading to VEML v3.0.");
                        reader = new StringReader(VEMLUtilities.FullyNotateVEML2_1(System.Text.Encoding.UTF8.GetString(rawData)));
                        xmlReader = XmlReader.Create(reader);
                        Schema.V2_1.veml v2_1VEML = (Schema.V2_1.veml) ser2_1.Deserialize(xmlReader);
                        veml = VEMLUtilities.ConvertFromV2_1(v2_1VEML);
                    }
                    else if (ser2_0.CanDeserialize(xmlReader))
                    {
                        //version = VEMLVersion.v2_0;
                        Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v2.0. Upgrading to VEML v3.0.");
                        reader = new StringReader(VEMLUtilities.FullyNotateVEML2_0(System.Text.Encoding.UTF8.GetString(rawData)));
                        xmlReader = XmlReader.Create(reader);
                        Schema.V2_0.veml v2_0VEML = (Schema.V2_0.veml) ser2_0.Deserialize(xmlReader);
                        veml = VEMLUtilities.ConvertFromV2_0(v2_0VEML);
                    }

                    else if (ser1_3.CanDeserialize(xmlReader))
                    {
                        //version = VEMLVersion.v1_3;
                        Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v1.3. Upgrading to VEML v3.0.");
                        reader = new StringReader(VEMLUtilities.FullyNotateVEML1_3(System.Text.Encoding.UTF8.GetString(rawData)));
                        xmlReader = XmlReader.Create(reader);
                        Schema.V1_3.veml v1_3VEML = (Schema.V1_3.veml) ser1_3.Deserialize(xmlReader);
                        veml = VEMLUtilities.ConvertFromV1_3(v1_3VEML);
                    }

                    else if (ser1_2.CanDeserialize(xmlReader))
                    {
                        //version = VEMLVersion.v1_2;
                        Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v1.2. Upgrading to VEML v3.0.");
                        reader = new StringReader(VEMLUtilities.FullyNotateVEML1_2(System.Text.Encoding.UTF8.GetString(rawData)));
                        xmlReader = XmlReader.Create(reader);
                        Schema.V1_2.veml v1_2VEML = (Schema.V1_2.veml) ser1_2.Deserialize(xmlReader);
                        veml = VEMLUtilities.ConvertFromV1_2(v1_2VEML);
                    }

                    else if (ser1_1.CanDeserialize(xmlReader))
                    {
                        //version = VEMLVersion.v1_1;
                        Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v1.1. Upgrading to VEML v3.0.");
                        reader = new StringReader(VEMLUtilities.FullyNotateVEML1_1(System.Text.Encoding.UTF8.GetString(rawData)));
                        xmlReader = XmlReader.Create(reader);
                        Schema.V1_1.veml v1_1VEML = (Schema.V1_1.veml) ser1_1.Deserialize(xmlReader);
                        veml = VEMLUtilities.ConvertFromV1_1(v1_1VEML);
                    }

                    else if (ser1_0.CanDeserialize(xmlReader))
                    {
                        //version = VEMLVersion.v1_1;
                        Logging.Log("[VEMLHandler->LoadVEML] Document is VEML v1.0. Upgrading to VEML v3.0.");
                        reader = new StringReader(VEMLUtilities.FullyNotateVEML1_0(System.Text.Encoding.UTF8.GetString(rawData)));
                        xmlReader = XmlReader.Create(reader);
                        Schema.V1_0.veml v1_0VEML = (Schema.V1_0.veml) ser1_0.Deserialize(xmlReader);
                        veml = VEMLUtilities.ConvertFromV1_0(v1_0VEML);
                    }

                    // Unknown VEML Version. Throw Error.
                    else
                    {
                        //version = VEMLVersion.Unknown;
                        Logging.LogWarning("[VEMLHandler->LoadVEML] Unknown VEML version.");
                        // Attempt deserialization to throw appropriate exceptions.
                        ser.Deserialize(xmlReader);
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogWarning("[VEMLHandler->LoadVEML] VEML document deserialization failure " + e.ToString());
                return null;
            }

            return veml;
        }

        /// <summary>
        /// Load a script resource as a string.
        /// </summary>
        /// <param name="resourceURI">URI of the script resource.</param>
        /// <param name="onLoaded">Action to invoke when loading of the script resource
        /// is complete. Provides a string containing the script resource.</param>
        public void LoadScriptResourceAsString(string resourceURI, Action<string> onLoaded)
        {
            Action onDownloaded = () =>
            {
                string script = LoadScript(Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(resourceURI)));
                onLoaded.Invoke(script);
            };
            DownloadScript(resourceURI, onDownloaded);
        }

        /// <summary>
        /// Download a script.
        /// </summary>
        /// <param name="uri">URI of the script resource.</param>
        /// <param name="onDownloaded">Action to invoke when downloading of the script is complete.</param>
        public void DownloadScript(string uri, Action onDownloaded)
        {
#if USE_WEBINTERFACE
            Action<int, Dictionary<string, string>, byte[]> onDownloadedAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
            {
                FinishScriptDownload(uri, code, data);
                onDownloaded.Invoke();
            });

            HTTPRequest request = new HTTPRequest(uri, HTTPRequest.HTTPMethod.Get, onDownloadedAction);

            if (runtime.fileHandler.FileExistsInFileDirectory(FileHandler.ToFileURI(uri)))
            {
                if (uri.StartsWith("file:/") || uri.StartsWith("/") || uri.StartsWith(".") || uri[1] == ':')
                {
                    Logging.Log("[VEMLHandler->DownloadScript] File " + uri + " already exists. Using stored version.");
                    onDownloaded.Invoke();
                    return;
                }
                Logging.Log("[VEMLHandler->DownloadScript] File " + uri + " already exists. Checking for newer version.");

                Action<int, Dictionary<string, string>, byte[]> onResponseAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        if (header.Key.ToLower() == "last-modified")
                        {
                            DateTime timestamp;
                            if (DateTime.TryParse(header.Value, out timestamp))
                            {
                                if (timestamp > System.IO.File.GetLastWriteTime(Path.Combine(
                                    runtime.fileHandler.fileDirectory, FileHandler.ToFileURI(uri))))
                                {
                                    Logging.Log("[VEMLHandler->DownloadScript] Cached version of file " + uri + " is outdated. Getting new version.");
                                }
                                else
                                {
                                    Logging.Log("[VEMLHandler->DownloadScript] Cached version of file " + uri + " is current. Using stored version.");
                                    onDownloaded.Invoke();
                                    return;
                                }
                            }
                        }
                    }
                    Logging.Log("[VEMLHandler->DownloadScript] Getting " + uri + ".");
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
        /// Load a script from a path.
        /// </summary>
        /// <param name="path">Path to load the script from.</param>
        /// <returns>A string containing the loaded script, or null.</returns>
        public string LoadScript(string path)
        {
            byte[] rawData = System.IO.File.ReadAllBytes(path);
            string script = System.Text.Encoding.UTF8.GetString(rawData);
            return script;
        }

        /// <summary>
        /// Download a file without caching.
        /// </summary>
        /// <param name="uri">URI from which to get the file.</param>
        /// <param name="onDownloaded">Action to invoke when downloading the file is complete. Provides
        /// a byte array containing the file data.</param>
        public void DownloadFileWithoutCache(string uri, Action<byte[]> onDownloaded)
        {
#if USE_WEBINTERFACE
            Action<int, Dictionary<string, string>, byte[]> onDownloadedAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
            {
                FinishFileDownload(uri, code, data);
                onDownloaded.Invoke(data);
            });

            HTTPRequest request = new HTTPRequest(uri, HTTPRequest.HTTPMethod.Get, onDownloadedAction);
            request.Send();
#endif
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="uri">URI from which to get the file.</param>
        /// <param name="onDownloaded">Action to invoke when downloading the file is complete. Provides
        /// a byte array containing the file data.</param>
        public void DownloadFile(string uri, Action<byte[]> onDownloaded)
        {
#if USE_WEBINTERFACE
            Action<int, Dictionary<string, string>, byte[]> onDownloadedAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
            {
                FinishFileDownload(uri, code, data);
                onDownloaded.Invoke(data);
            });

            HTTPRequest request = new HTTPRequest(uri, HTTPRequest.HTTPMethod.Get, onDownloadedAction);

            if (runtime.fileHandler.FileExistsInFileDirectory(FileHandler.ToFileURI(uri)))
            {
                if (uri.StartsWith("file:/") || uri.StartsWith("/") || uri.StartsWith(".") || uri[1] == ':')
                {
                    Logging.Log("[VEMLHandler->DownloadFile] File " + uri + " already exists. Using stored version.");
                    onDownloaded.Invoke(runtime.fileHandler.GetFileInFileDirectory(FileHandler.ToFileURI(uri)));
                    return;
                }
                Logging.Log("[VEMLHandler->DownloadFile] File " + uri + " already exists. Checking for newer version.");

                Action<int, Dictionary<string, string>, byte[]> onResponseAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        if (header.Key.ToLower() == "last-modified")
                        {
                            DateTime timestamp;
                            if (DateTime.TryParse(header.Value, out timestamp))
                            {
                                if (timestamp > System.IO.File.GetLastWriteTime(Path.Combine(
                                    runtime.fileHandler.fileDirectory, FileHandler.ToFileURI(uri))))
                                {
                                    Logging.Log("[VEMLHandler->DownloadFile] Cached version of file " + uri + " is outdated. Getting new version.");
                                }
                                else
                                {
                                    Logging.Log("[VEMLHandler->DownloadFile] Cached version of file " + uri + " is current. Using stored version.");
                                    onDownloaded.Invoke(data);
                                    return;
                                }
                            }
                        }
                    }
                    Logging.Log("[VEMLHandler->DownloadFile] Getting " + uri + ".");
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
        /// Finish downloading a file.
        /// </summary>
        /// <param name="uri">URI which the file is from.</param>
        /// <param name="responseCode">Response code received.</param>
        /// <param name="rawData">Raw data received.</param>
        private void FinishFileDownload(string uri, int responseCode, byte[] rawData)
        {
            Logging.Log("[VEMLHandler->FinishFileDownload] Got response " + responseCode + " for request " + uri);

            if (responseCode != 200)
            {
                Logging.Log("[VEMLHandler->FinishFileDownload] Error loading file.");
                return;
            }

            string filePath = FileHandler.ToFileURI(uri);
            if (runtime.fileHandler.FileExistsInFileDirectory(filePath))
            {
                runtime.fileHandler.DeleteFileInFileDirectory(filePath);
            }
            runtime.fileHandler.CreateFileInFileDirectory(filePath, rawData);
        }

        /// <summary>
        /// Finish downloading a script.
        /// </summary>
        /// <param name="uri">URI which the script is from.</param>
        /// <param name="responseCode">Response code received.</param>
        /// <param name="rawData">Raw data received.</param>
        private void FinishScriptDownload(string uri, int responseCode, byte[] rawData)
        {
            Logging.Log("[VEMLHandler->FinishScriptDownload] Got response " + responseCode + " for request " + uri);

            if (responseCode != 200)
            {
                Logging.Log("[VEMLHandler->FinishScriptDownload] Error loading file.");
                return;
            }

            string filePath = FileHandler.ToFileURI(uri);
            if (runtime.fileHandler.FileExistsInFileDirectory(filePath))
            {
                runtime.fileHandler.DeleteFileInFileDirectory(filePath);
            }
            runtime.fileHandler.CreateFileInFileDirectory(filePath, rawData);
        }

        /// <summary>
        /// Finish downloading a VEML file.
        /// </summary>
        /// <param name="uri">URI which the VEML file is from.</param>
        /// <param name="responseCode">Response code received.</param>
        /// <param name="rawData">Raw data received.</param>
        private void FinishVEMLDownload(string uri, int responseCode, byte[] rawData)
        {
            Logging.Log("[VEMLHandler->FinishVEMLDownload] Got response " + responseCode + " for request " + uri);

            if (responseCode != 200)
            {
                Logging.Log("[VEMLHandler->FinishVEMLDownload] Error loading file.");
                return;
            }

            string filePath = FileHandler.ToFileURI(uri);
            if (runtime.fileHandler.FileExistsInFileDirectory(filePath))
            {
                runtime.fileHandler.DeleteFileInFileDirectory(filePath);
            }
            runtime.fileHandler.CreateFileInFileDirectory(filePath, rawData);
        }

        /// <summary>
        /// Apply a VEML document.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <param name="onComplete">Action to invoke upon completion of world loading.
        /// Provides a success/fail indication.</param>
        private IEnumerator ApplyVEMLDocument(Schema.V3_0.veml vemlDocument, string baseURI, Action<bool> onComplete)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            string[] scripts = null;

            bool scriptsDoneProcessing = false;
            Action<string[]> onScriptsProcessed = new Action<string[]>((scrs) =>
            {
                scripts = scrs;
                scriptsDoneProcessing = true;
            });

            if (ProcessMetadata(vemlDocument, baseURI, onScriptsProcessed) == false)
            {
                Logging.LogWarning("[VEMLHandler->ApplyVEMLDocument] Error processing metadata.");
                onComplete.Invoke(false);
                yield break;
            }

            if (ProcessEnvironment(vemlDocument, baseURI) == false)
            {
                Logging.LogWarning("[VEMLHandler->ApplyVEMLDocument] Error processing environment.");
                onComplete.Invoke(false);
                yield break;
            }

            // Wait for all scripts to download.
            float elapsedTime = 0f;
            while (scriptsDoneProcessing == false && elapsedTime < timeout)
            {
                yield return new WaitForSeconds(0.25f);
                elapsedTime += 0.25f;
            }

            // Wait for all entities to load.
            elapsedTime = 0f;
            while (loadingEntities > 0 && elapsedTime < timeout)
            {
                yield return new WaitForSeconds(0.25f);
                elapsedTime += 0.25f;
            }

            // Set avatar entity now that all entities have been loaded
            if (!string.IsNullOrEmpty(pendingAvatarEntityTag))
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.SetAvatarEntityByTag(pendingAvatarEntityTag);
                }
                pendingAvatarEntityTag = null; // Clear the pending tag
            }

            // Apply rig parenting and offset now that both avatar entity and rig offset are set
            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
            {
                WebVerseRuntime.Instance.inputManager.desktopRig.ApplyRigParentingAndOffset();
            }

            // Set rig offset now that the avatar entity has been set
            if (!string.IsNullOrEmpty(pendingRigOffset))
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.SetRigOffsetFromString(pendingRigOffset);
                }
                pendingRigOffset = null; // Clear the pending offset
            }

            if (scripts != null)
            {
                foreach (string script in scripts)
                {
                    WebVerseRuntime.Instance.javascriptHandler.RunScript(script);
                }
            }

            WebVerseRuntime.Instance.inputManager.inputEnabled = true;

            onComplete.Invoke(true);
        }

        /// <summary>
        /// Process VEML document metadata.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <param name="onScriptsProcessed">Action to invoke when scripts are processed. Provides an array of
        /// strings containing the script contents.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessMetadata(Schema.V3_0.veml vemlDocument, string baseURI, Action<string[]> onScriptsProcessed)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            // Validate metadata.
            if (vemlDocument.metadata == null)
            {
                Logging.LogWarning("[VEMLHandler->ProcessMetadata] VEML document missing required field: metadata.");
                return false;
            }
            else
            {
                if (string.IsNullOrEmpty(vemlDocument.metadata.title))
                {
                    Logging.LogWarning("[VEMLHandler->ProcessMetadata] VEML document metadata missing required field: title.");
                    return false;
                }
                else
                {

                }

                if (ProcessCapabilities(vemlDocument, baseURI) == false)
                {
                    Logging.LogWarning("[VEMLHandler->ProcessMetadata] Error processing capabilities.");
                    return false;
                }

                StartCoroutine(ProcessScripts(vemlDocument, baseURI, onScriptsProcessed));

                if (ProcessInputEvents(vemlDocument, baseURI) == false)
                {
                    Logging.LogWarning("[VEMLHandler->ProcessMetadata] Error processing input events.");
                    return false;
                }

                if (ProcessControlFlags(vemlDocument, baseURI) == false)
                {
                    Logging.LogWarning("[VEMLHandler->ProcessMetadata] Error processing control flags.");
                    return false;
                }

                if (ProcessSynchronizers(vemlDocument, baseURI) == false)
                {
                    Logging.LogWarning("[VEMLHandler->ProcessMetadata] Error processing synchronizers.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Process VEML document environment.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessEnvironment(Schema.V3_0.veml vemlDocument, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            // Validate environment.
            if (vemlDocument.environment == null)
            {
                Logging.LogWarning("[VEMLHandler->ProcessEnvironment] VEML document missing required field: environment.");
                return false;
            }
            else
            {
                // Validate background.
                if (vemlDocument.environment.background == null)
                {
                    Logging.LogWarning("[VEMLHandler->ProcessEnvironment] VEML document environment missing required field: background.");
                    return false;
                }
                else
                {
                    ProcessBackground(vemlDocument.environment.background.Item,
                        vemlDocument.environment.background.ItemElementName, formattedBaseURI);
                }

                if (vemlDocument.environment.effects != null)
                {
                    if (ProcessEffects(vemlDocument, baseURI) == false)
                    {
                        Logging.LogWarning("[VEMLHandler->ProcessEnvironment] Error processing effects.");
                        return false;
                    }
                }

                if (ProcessEntities(vemlDocument, baseURI) == false)
                {
                    Logging.LogWarning("[VEMLHandler->ProcessEnvironment] Error processing entities.");
                    return false;
                }

                if (liteProceduralSkyToLoadOnLoadCompletion != null)
                {
                    if (LoadLiteProceduralSky(liteProceduralSkyToLoadOnLoadCompletion) == false)
                    {
                        Logging.LogWarning("[VEMLHandler->ProcessEnvironment] Error loading lite procedural sky.");
                        liteProceduralSkyToLoadOnLoadCompletion = null;
                        return false;
                    }
                    liteProceduralSkyToLoadOnLoadCompletion = null;
                }
            }

            return true;
        }

        /// <summary>
        /// Process VEML document metadata.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <param name="onProcessed">Action to invoke when scripts are processed. Provides an array of
        /// strings containing the script contents.</param>
        private IEnumerator ProcessScripts(Schema.V3_0.veml vemlDocument, string baseURI, Action<string[]> onProcessed)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            Dictionary<Guid, string> scriptsToRun = new Dictionary<Guid, string>();

            // Set up scripts.
            if (vemlDocument.metadata.script != null)
            {
                foreach (string script in vemlDocument.metadata.script)
                {
                    Guid scriptID = Guid.NewGuid();
                    if (script.EndsWith(".js"))
                    {
                        // Path to script has been provided. Load script and place in dictionary.
                        Action<string> onLoadedAction = new Action<string>((scr) =>
                        {
                            scriptsToRun[scriptID] = scr;
                        });
                        scriptsToRun.Add(scriptID, null);

                        LoadScriptResourceAsString(VEMLUtilities.FullyQualifyURI(script, formattedBaseURI), onLoadedAction);
                    }
                    else
                    {
                        // Raw script has been provided. Place in dictionary.
                        scriptsToRun.Add(scriptID, script);
                    }
                }
            }

            // Wait for all scripts to download.
            float elapsedTime = 0f;
            bool allLoaded = true;
            do
            {
                allLoaded = true;
                foreach (string script in scriptsToRun.Values)
                {
                    if (script == null)
                    {
                        allLoaded = false;
                        yield return new WaitForSeconds(0.25f);
                        elapsedTime += 0.25f;
                        break;
                    }
                }
            } while (allLoaded == false && elapsedTime < timeout);

            List<string> scripts = new List<string>();
            foreach (string script in scriptsToRun.Values)
            {
                scripts.Add(script);
            }

            onProcessed.Invoke(scripts.ToArray());

            yield return null;
        }

        /// <summary>
        /// Process VEML document metadata capabilities.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessCapabilities(Schema.V3_0.veml vemlDocument, string baseURI)
        {
            // Check capabilities.
            if (vemlDocument.metadata.capability != null)
            {
                foreach (string capability in vemlDocument.metadata.capability)
                {
                    Capabilities.CapabilitySupport support = Capabilities.GetCapabilitySupportValue(capability);
                    if (support == Capabilities.CapabilitySupport.Supported)
                    {
                        return true;
                    }
                    else if (support == Capabilities.CapabilitySupport.Unsupported)
                    {
                        Logging.LogWarning("[VEMLHandler->ProcessCapabilities] Capability " + capability + " unsupported.");
                        return false;
                    }
                    else if (support == Capabilities.CapabilitySupport.LimitedSupport)
                    {
                        Logging.LogWarning("[VEMLHandler->ProcessCapabilities] Capability " + capability + " has limited support.");
                    }
                    else if (support == Capabilities.CapabilitySupport.InsufficientResources)
                    {
                        Logging.LogWarning("[VEMLHandler->ProcessCapabilities] Capability " + capability + " has insufficient resources.");
                    }
                    else
                    {
                        Logging.LogError("[VEMLHandler->ProcessCapabilities] Error getting capability " + capability + " support.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Process VEML document metadata input events.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessInputEvents(Schema.V3_0.veml vemlDocument, string baseURI)
        {
            // Set up input events.
            if (vemlDocument.metadata.inputevent != null)
            {
                foreach (inputevent inputEvent in vemlDocument.metadata.inputevent)
                {
                    WebVerseRuntime.Instance.inputManager.RegisterInputEvent(inputEvent.input, inputEvent.@event);
                }
            }

            return true;
        }

        /// <summary>
        /// Process VEML document metadata control flags.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessControlFlags(Schema.V3_0.veml vemlDocument, string baseURI)
        {
            // Set up control flags.
            if (vemlDocument.metadata.controlflags != null)
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    if (vemlDocument.metadata.controlflags.joystickmotionSpecified)
                    {
                        WebVerseRuntime.Instance.vrRig.joystickMotionEnabled = vemlDocument.metadata.controlflags.joystickmotion;
                    }
                    if (vemlDocument.metadata.controlflags.leftgrabmoveSpecified)
                    {
                        WebVerseRuntime.Instance.vrRig.leftGrabMoveEnabled = vemlDocument.metadata.controlflags.leftgrabmove;
                    }
                    if (vemlDocument.metadata.controlflags.rightgrabmoveSpecified)
                    {
                        WebVerseRuntime.Instance.vrRig.rightGrabMoveEnabled = vemlDocument.metadata.controlflags.rightgrabmove;
                    }
                    if (vemlDocument.metadata.controlflags.lefthandinteractionSpecified)
                    {
                        WebVerseRuntime.Instance.vrRig.leftInteractionEnabled = vemlDocument.metadata.controlflags.lefthandinteraction;
                    }
                    if (vemlDocument.metadata.controlflags.righthandinteractionSpecified)
                    {
                        WebVerseRuntime.Instance.vrRig.rightInteractionEnabled = vemlDocument.metadata.controlflags.righthandinteraction;
                    }
                    if (!string.IsNullOrEmpty(vemlDocument.metadata.controlflags.leftvrpointer))
                    {
                        if (vemlDocument.metadata.controlflags.leftvrpointer.ToLower().Replace("\"", "") == "none")
                        {
                            WebVerseRuntime.Instance.vrRig.leftPointerMode = Input.SteamVR.VRRig.PointerMode.None;
                        }
                        else if (vemlDocument.metadata.controlflags.leftvrpointer.ToLower().Replace("\"", "") == "teleport")
                        {
                            WebVerseRuntime.Instance.vrRig.leftPointerMode = Input.SteamVR.VRRig.PointerMode.Teleport;
                        }
                        else if (vemlDocument.metadata.controlflags.leftvrpointer.ToLower().Replace("\"", "") == "ui")
                        {
                            WebVerseRuntime.Instance.vrRig.leftPointerMode = Input.SteamVR.VRRig.PointerMode.UI;
                        }
                        else
                        {
                            Logging.LogWarning("[VEMLHandler->ProcessControlFlags] VEML document defines unknown left VR pointer type: "
                                + vemlDocument.metadata.controlflags.leftvrpointer);
                        }
                    }
                    if (!string.IsNullOrEmpty(vemlDocument.metadata.controlflags.rightvrpointer))
                    {
                        if (vemlDocument.metadata.controlflags.rightvrpointer.ToLower().Replace("\"", "") == "none")
                        {
                            WebVerseRuntime.Instance.vrRig.rightPointerMode = Input.SteamVR.VRRig.PointerMode.None;
                        }
                        else if (vemlDocument.metadata.controlflags.rightvrpointer.ToLower().Replace("\"", "") == "teleport")
                        {
                            WebVerseRuntime.Instance.vrRig.rightPointerMode = Input.SteamVR.VRRig.PointerMode.Teleport;
                        }
                        else if (vemlDocument.metadata.controlflags.rightvrpointer.ToLower().Replace("\"", "") == "ui")
                        {
                            WebVerseRuntime.Instance.vrRig.rightPointerMode = Input.SteamVR.VRRig.PointerMode.UI;
                        }
                        else
                        {
                            Logging.LogWarning("[VEMLHandler->ProcessControlFlags] VEML document defines unknown right VR pointer type: "
                                + vemlDocument.metadata.controlflags.rightvrpointer);
                        }
                    }
                    if (vemlDocument.metadata.controlflags.leftvrpokerSpecified)
                    {
                        WebVerseRuntime.Instance.vrRig.leftPokerEnabled = vemlDocument.metadata.controlflags.leftvrpoker;
                    }
                    if (vemlDocument.metadata.controlflags.rightvrpokerSpecified)
                    {
                        WebVerseRuntime.Instance.vrRig.rightPokerEnabled = vemlDocument.metadata.controlflags.rightvrpoker;
                    }
                    if (!string.IsNullOrEmpty(vemlDocument.metadata.controlflags.turnlocomotion))
                    {
                        if (vemlDocument.metadata.controlflags.turnlocomotion.ToLower().Replace("\"", "") == "none")
                        {
                            WebVerseRuntime.Instance.vrRig.turnLocomotionMode = Input.SteamVR.VRRig.TurnLocomotionMode.None;
                        }
                        else if (vemlDocument.metadata.controlflags.turnlocomotion.ToLower().Replace("\"", "") == "smooth")
                        {
                            WebVerseRuntime.Instance.vrRig.turnLocomotionMode = Input.SteamVR.VRRig.TurnLocomotionMode.Smooth;
                        }
                        else if (vemlDocument.metadata.controlflags.turnlocomotion.ToLower().Replace("\"", "") == "snap")
                        {
                            WebVerseRuntime.Instance.vrRig.turnLocomotionMode = Input.SteamVR.VRRig.TurnLocomotionMode.Snap;
                        }
                        else
                        {
                            Logging.LogWarning("[VEMLHandler->ProcessControlFlags] VEML document defines unknown turn locomotion type: "
                                + vemlDocument.metadata.controlflags.turnlocomotion);
                        }
                    }
                    if (vemlDocument.metadata.controlflags.twohandedgrabmoveSpecified)
                    {
                        WebVerseRuntime.Instance.vrRig.twoHandedGrabMoveEnabled = vemlDocument.metadata.controlflags.twohandedgrabmove;
                    }

                    // Set up desktop control flags.
                    if (WebVerseRuntime.Instance.platformInput is Input.Desktop.DesktopInput)
                    {
                        Input.Desktop.DesktopInput desktopInput = (Input.Desktop.DesktopInput)WebVerseRuntime.Instance.platformInput;

                        if (vemlDocument.metadata.controlflags.gravityenabledSpecified)
                        {
                            desktopInput.gravityEnabled = vemlDocument.metadata.controlflags.gravityenabled;
                            // Also set on DesktopRig if available
                            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                            {
                                WebVerseRuntime.Instance.inputManager.desktopRig.gravityEnabled = vemlDocument.metadata.controlflags.gravityenabled;
                            }
                        }

                        if (vemlDocument.metadata.controlflags.wasdmotionenabledSpecified)
                        {
                            desktopInput.wasdMotionEnabled = vemlDocument.metadata.controlflags.wasdmotionenabled;
                            // Also set on DesktopRig if available
                            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                            {
                                WebVerseRuntime.Instance.inputManager.desktopRig.wasdMotionEnabled = vemlDocument.metadata.controlflags.wasdmotionenabled;
                            }
                        }

                        if (vemlDocument.metadata.controlflags.mouselookenabledSpecified)
                        {
                            desktopInput.mouseLookEnabled = vemlDocument.metadata.controlflags.mouselookenabled;
                            // Also set on DesktopRig if available
                            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                            {
                                WebVerseRuntime.Instance.inputManager.desktopRig.mouseLookEnabled = vemlDocument.metadata.controlflags.mouselookenabled;
                            }
                        }

                        // Store avatar entity tag to be set after entities are loaded
                        if (!string.IsNullOrEmpty(vemlDocument.metadata.controlflags.avatarentity))
                        {
                            pendingAvatarEntityTag = vemlDocument.metadata.controlflags.avatarentity;
                        }

                        // Process jump enabled flag
                        if (vemlDocument.metadata.controlflags.jumpenabledSpecified)
                        {
                            desktopInput.jumpEnabled = vemlDocument.metadata.controlflags.jumpenabled;

                            // Also apply to DesktopRig if it exists
                            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                            {
                                WebVerseRuntime.Instance.inputManager.desktopRig.jumpEnabled = vemlDocument.metadata.controlflags.jumpenabled;
                            }
                        }

                        // Process movement speed flag
                        if (vemlDocument.metadata.controlflags.movementspeedSpecified)
                        {
                            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                            {
                                WebVerseRuntime.Instance.inputManager.desktopRig.movementSpeed = vemlDocument.metadata.controlflags.movementspeed;
                            }
                        }

                        // Process look speed flag
                        if (vemlDocument.metadata.controlflags.lookspeedSpecified)
                        {
                            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                            {
                                WebVerseRuntime.Instance.inputManager.desktopRig.mouseSensitivity = vemlDocument.metadata.controlflags.lookspeed;
                            }
                        }
                        
                        // Process rig offset flag
                        if (!string.IsNullOrEmpty(vemlDocument.metadata.controlflags.rigoffset))
                        {
                            // Store the rig offset to be applied after entities are loaded
                            pendingRigOffset = vemlDocument.metadata.controlflags.rigoffset;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Process VEML document metadata synchronizers.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessSynchronizers(Schema.V3_0.veml vemlDocument, string baseURI)
        {
            // Set up synchronizers.
            if (vemlDocument.metadata.synchronizationservice != null)
            {
                foreach (synchronizationservice synchronizationservice in vemlDocument.metadata.synchronizationservice)
                {
                    switch (synchronizationservice.type.ToLower())
                    {
                        case "vss":
#if USE_WEBINTERFACE
                            bool tls = false;
                            string hostPortSection = "";
                            if (synchronizationservice.address.StartsWith("vss:"))
                            {
                                tls = false;
                                hostPortSection = synchronizationservice.address.Substring(4);
                            }
                            else if (synchronizationservice.address.StartsWith("vsss:"))
                            {
                                tls = true;
                                hostPortSection = synchronizationservice.address.Substring(5);
                            }
                            else
                            {
                                hostPortSection = synchronizationservice.address;
                            }
                            string[] parts = hostPortSection.Split(':');
                            if (parts.Length != 2)
                            {
                                Logging.LogWarning("[VEMLHandler->ProcessSynchronizers] VEML document contains invalid synchronization service address: "
                                    + synchronizationservice.address);
                                break;
                            }
                            WebVerseRuntime.Instance.vosSynchronizationManager.AddSynchronizerAndSession(
                                synchronizationservice.id, parts[0], int.Parse(parts[1]), tls, WebInterface.MQTT.MQTTClient.Transports.TCP,
                                Vector3.zero, synchronizationservice.session);
#endif
                            break;

                        default:
                            Logging.LogWarning("[VEMLHandler->ProcessSynchronizers] VEML document defines unknown synchronization service type: "
                                + synchronizationservice.type);
                            break;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Process background.
        /// </summary>
        /// <param name="entry">The entry for the background setting.</param>
        /// <param name="choiceType">Type that the background setting is</param>
        /// <param name="uri">Base URI of the VEML document.</param>
        private void ProcessBackground(object backgroundInfo, ItemChoiceType choiceType, string uri)
        {
            if (choiceType == ItemChoiceType.liteproceduralsky)
            {
                if (backgroundInfo is not liteproceduralsky)
                {
                    Logging.LogError("[VEMLHandler->ProcessBackground] Expected lite procedural sky, not a lite procedural sky.");
                    return;
                }
                liteProceduralSkyToLoadOnLoadCompletion = (liteproceduralsky) backgroundInfo;
            }
            else if (choiceType == ItemChoiceType.color || choiceType == ItemChoiceType.panorama)
            {
                if (backgroundInfo is not string)
                {
                    Logging.LogError("[VEMLHandler->ProcessBackground] Expected string type, not a string.");
                    return;
                }

                string entry = (string) backgroundInfo;
                switch (entry.ToLower())
                {
                    case "white":
                        StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(Color.white);
                        break;

                    case "black":
                        StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(Color.black);
                        break;

                    case "grey":
                    case "gray":
                        StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(Color.gray);
                        break;

                    case "red":
                        StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(Color.red);
                        break;

                    case "yellow":
                        StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(Color.yellow);
                        break;

                    case "blue":
                        StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(Color.blue);
                        break;

                    case "cyan":
                        StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(Color.cyan);
                        break;

                    case "magenta":
                        StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(Color.magenta);
                        break;

                    case "green":
                        StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(Color.green);
                        break;

                    default:
                        uri = uri.Replace("http:\\", "http://").Replace("https:\\", "https://");
                        if (uri.Contains("http:/") && !uri.Contains("http://"))
                        {
                            uri = uri.Replace("http:/", "http://");
                        }
                        if (uri.Contains("https:/") && !uri.Contains("https://"))
                        {
                            uri = uri.Replace("https:/", "https://");
                        }
                        if (entry[0] != '#')
                        {
                            // Assume texture.
                            Action<byte[]> onDownloaded = new Action<byte[]>((rawData) =>
                            {
                                if (rawData != null)
                                {
                                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                                    texture.LoadImage(rawData);
                                    StraightFour.StraightFour.ActiveWorld.environmentManager.SetSkyTexture(texture);
                                }
                            });
                            DownloadFileWithoutCache(VEMLUtilities.FullyQualifyURI(entry, uri), onDownloaded);
                        }
                        else
                        {
                            // Assume color code.
                            StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(FromHex(entry));
                        }
                        break;
                }
            }
            else
            {
                Logging.LogError("[VEMLHandler->ProcessBackground] Unknown background type detected.");
            }
        }
        
        /// <summary>
        /// Load a lite procedural sky.
        /// </summary>
        /// <param name="proceduralSky">Lite procedural sky to load.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        private bool LoadLiteProceduralSky(liteproceduralsky proceduralSky)
        {
            if (string.IsNullOrEmpty(proceduralSky.sunentitytag))
                {
                    Logging.LogWarning("[VEMLHandler->LoadLiteProceduralSky] Sun light entity required for lite procedural sky.");
                    return false;
                }

                object sunEntity = Javascript.APIs.Entity.Entity.GetByTag(proceduralSky.sunentitytag);
                if (sunEntity == null)
                {
                    Logging.LogWarning("[VEMLHandler->LoadLiteProceduralSky] Sun light entity with tag " + proceduralSky.sunentitytag
                        + " not found.");
                    return false;
                }

                if (sunEntity is not Javascript.APIs.Entity.LightEntity)
                {
                    Logging.LogWarning("[VEMLHandler->LoadLiteProceduralSky] Sun light entity with tag " + proceduralSky.sunentitytag
                        + " not a light entity.");
                    return false;
                }

                if (proceduralSky.daynightcycleenabled == true)
                {
                    return Javascript.APIs.Environment.Environment.SetLiteDayNightSky((Javascript.APIs.Entity.LightEntity) sunEntity,
                        proceduralSky.groundenabled, ProcessColorIntoJSAPIColor(proceduralSky.groundcolor), proceduralSky.groundheight,
                        proceduralSky.groundfadeamount, proceduralSky.horizonskyblend,
                        ProcessColorIntoJSAPIColor(proceduralSky.dayhorizoncolor), ProcessColorIntoJSAPIColor(proceduralSky.dayskycolor),
                        ProcessColorIntoJSAPIColor(proceduralSky.nighthorizoncolor), ProcessColorIntoJSAPIColor(proceduralSky.nightskycolor),
                        proceduralSky.horizonsaturationamount, proceduralSky.horizonsaturationfalloff, proceduralSky.sunenabled,
                        proceduralSky.sundiameter, ProcessColorIntoJSAPIColor(proceduralSky.sunhorizoncolor),
                        ProcessColorIntoJSAPIColor(proceduralSky.sunzenithcolor), proceduralSky.sunskylightingenabled,
                        proceduralSky.skylightingfalloffamount, proceduralSky.skylightingfalloffintensity, proceduralSky.sunsetintensity,
                        proceduralSky.sunsetradialfalloff, proceduralSky.sunsethorizontalfalloff, proceduralSky.sunsetverticalfalloff,
                        proceduralSky.moonenabled, proceduralSky.moondiameter, ProcessColorIntoJSAPIColor(proceduralSky.mooncolor),
                        proceduralSky.moonfalloffamount, proceduralSky.starsenabled, proceduralSky.starsbrightness,
                        proceduralSky.starsdaytimebrightness, proceduralSky.starshorizonfalloff, proceduralSky.starssaturation,
                        proceduralSky.proceduralstarsenabled, proceduralSky.proceduralstarssharpness, proceduralSky.proceduralstarsamount,
                        proceduralSky.starstextureenabled, proceduralSky.startextureuri, ProcessColorIntoJSAPIColor(proceduralSky.startint),
                        proceduralSky.starscale, proceduralSky.starrotationspeed, proceduralSky.cloudsenabled, proceduralSky.cloudstextureuri,
                        new Javascript.APIs.WorldTypes.Vector2(proceduralSky.cloudsscalex, proceduralSky.cloudsscaley),
                        new Javascript.APIs.WorldTypes.Vector2(proceduralSky.cloudsspeedx, proceduralSky.cloudsspeedy), proceduralSky.cloudiness,
                        proceduralSky.cloudsopacity, proceduralSky.cloudssharpness, proceduralSky.cloudsshadingintensity,
                        proceduralSky.cloudszenithfalloff, proceduralSky.cloudsiterations, proceduralSky.cloudsgain, proceduralSky.cloudslacunarity,
                        ProcessColorIntoJSAPIColor(proceduralSky.cloudsdaycolor), ProcessColorIntoJSAPIColor(proceduralSky.cloudsnightcolor));
                }
                else
                {
                    return Javascript.APIs.Environment.Environment.SetLiteConstantColorSky((Javascript.APIs.Entity.LightEntity) sunEntity,
                        proceduralSky.groundenabled, ProcessColorIntoJSAPIColor(proceduralSky.groundcolor), proceduralSky.groundheight,
                        proceduralSky.groundfadeamount, proceduralSky.horizonskyblend,
                        ProcessColorIntoJSAPIColor(proceduralSky.dayhorizoncolor), ProcessColorIntoJSAPIColor(proceduralSky.dayskycolor),
                        proceduralSky.horizonsaturationamount, proceduralSky.horizonsaturationfalloff, proceduralSky.sunenabled,
                        proceduralSky.sundiameter, ProcessColorIntoJSAPIColor(proceduralSky.sunhorizoncolor),
                        ProcessColorIntoJSAPIColor(proceduralSky.sunzenithcolor), proceduralSky.sunskylightingenabled,
                        proceduralSky.skylightingfalloffamount, proceduralSky.skylightingfalloffintensity, proceduralSky.sunsetintensity,
                        proceduralSky.sunsetradialfalloff, proceduralSky.sunsethorizontalfalloff, proceduralSky.sunsetverticalfalloff,
                        proceduralSky.moonenabled, proceduralSky.moondiameter, ProcessColorIntoJSAPIColor(proceduralSky.mooncolor),
                        proceduralSky.moonfalloffamount, proceduralSky.starsenabled, proceduralSky.starsbrightness,
                        proceduralSky.starsdaytimebrightness, proceduralSky.starshorizonfalloff, proceduralSky.starssaturation,
                        proceduralSky.proceduralstarsenabled, proceduralSky.proceduralstarssharpness, proceduralSky.proceduralstarsamount,
                        proceduralSky.starstextureenabled, proceduralSky.startextureuri, ProcessColorIntoJSAPIColor(proceduralSky.startint),
                        proceduralSky.starscale, proceduralSky.starrotationspeed, proceduralSky.cloudsenabled, proceduralSky.cloudstextureuri,
                        new Javascript.APIs.WorldTypes.Vector2(proceduralSky.cloudsscalex, proceduralSky.cloudsscaley),
                        new Javascript.APIs.WorldTypes.Vector2(proceduralSky.cloudsspeedx, proceduralSky.cloudsspeedy), proceduralSky.cloudiness,
                        proceduralSky.cloudsopacity, proceduralSky.cloudssharpness, proceduralSky.cloudsshadingintensity,
                        proceduralSky.cloudszenithfalloff, proceduralSky.cloudsiterations, proceduralSky.cloudsgain, proceduralSky.cloudslacunarity,
                        ProcessColorIntoJSAPIColor(proceduralSky.cloudsdaycolor));
                }
        }

        /// <summary>
        /// Process VEML document environment effects.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessEffects(Schema.V3_0.veml vemlDocument, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (vemlDocument.environment.effects.litefog != null)
            {
                if (vemlDocument.environment.effects.litefog.fogenabled == true)
                {
                    Javascript.APIs.Environment.Environment.ActivateLiteFog(
                        ProcessColorIntoJSAPIColor(vemlDocument.environment.effects.litefog.color),
                        vemlDocument.environment.effects.litefog.density);
                }
            }

            return true;
        }

        /// <summary>
        /// Process VEML document environment entities.
        /// </summary>
        /// <param name="vemlDocument">The VEML document.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessEntities(Schema.V3_0.veml vemlDocument, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            Dictionary<entity, entity> entities;

            // Set up entities.
            entities = new Dictionary<entity, entity>();
            if (vemlDocument.environment.entity != null)
            {
                entity[] es = vemlDocument.environment.entity;
                SetUpEntityIDs(ref es);
                vemlDocument.environment.entity = es;

                loadingEntities = 0;
                entities = GetAllChildEntities(vemlDocument.environment.entity);

                foreach (entity entity in entities.Keys)
                {
                    if (entity.tag == null)
                    {
                        Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity missing required field: tag.");
                        return false;
                    }
                    else
                    {

                    }

                    if (entity.transform == null)
                    {
                        Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity missing required field: transform.");
                        return false;
                    }
                    else
                    {
                        if (entity.transform is scaletransform)
                        {
                            if (((scaletransform) entity.transform).position == null)
                            {
                                Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity transform missing required field: position.");
                                return false;
                            }
                            else
                            {

                            }

                            if (((scaletransform) entity.transform).rotation == null)
                            {
                                Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity transform missing required field: rotation.");
                                return false;
                            }
                            else
                            {

                            }

                            if (((scaletransform) entity.transform).scale == null)
                            {
                                Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity transform missing required field: scale.");
                                return false;
                            }
                            else
                            {

                            }
                        }
                        else if (entity.transform is sizetransform)
                        {
                            if (((sizetransform) entity.transform).position == null)
                            {
                                Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity transform missing required field: position.");
                                return false;
                            }
                            else
                            {

                            }

                            if (((sizetransform) entity.transform).rotation == null)
                            {
                                Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity transform missing required field: rotation.");
                                return false;
                            }
                            else
                            {

                            }

                            if (((sizetransform) entity.transform).size == null)
                            {
                                Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity transform missing required field: size.");
                                return false;
                            }
                            else
                            {

                            }
                        }
                        else if (entity.transform is canvastransform)
                        {
                            if (((canvastransform) entity.transform).positionpercent == null)
                            {
                                Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity transform missing required field: position-percent.");
                                return false;
                            }
                            else
                            {

                            }

                            if (((canvastransform) entity.transform).sizepercent == null)
                            {
                                Logging.LogWarning("[VEMLHandler->ProcessEntities] VEML document environment entity transform missing required field: size-percent.");
                                return false;
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            Logging.LogWarning("[VEMLHandler->ProcessEntities] Invalid transform.");
                            return false;
                        }
                    }

                    // Handle entity specific properties.
                    if (entity is mesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshEntity((mesh)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is container)
                    {
                        loadingEntities++;
                        if (ProcessContainerEntity((container)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing container entity.");
                            return false;
                        }
                    }
                    else if (entity is character)
                    {
                        loadingEntities++;
                        if (ProcessCharacterEntity((character)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing character entity.");
                            return false;
                        }
                    }
                    else if (entity is light)
                    {
                        loadingEntities++;
                        if (ProcessLightEntity((light)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing light entity.");
                            return false;
                        }
                    }
                    else if (entity is terrain)
                    {
                        loadingEntities++;
                        if (ProcessTerrainEntity((terrain)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing terrain entity.");
                            return false;
                        }
                    }
                    else if (entity is text)
                    {
                        loadingEntities++;
                        if (ProcessTextEntity((text)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing text entity.");
                            return false;
                        }
                    }
                    else if (entity is button)
                    {
                        loadingEntities++;
                        if (ProcessButtonEntity((button)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing button entity.");
                            return false;
                        }
                    }
                    else if (entity is canvas)
                    {
                        loadingEntities++;
                        if (ProcessCanvasEntity((canvas)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing canvas entity.");
                            return false;
                        }
                    }
                    else if (entity is html)
                    {
                        loadingEntities++;
                        if (ProcessHTMLEntity((html)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing HTML entity.");
                            return false;
                        }
                    }
                    else if (entity is input)
                    {
                        loadingEntities++;
                        if (ProcessInputEntity((input)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing input entity.");
                            return false;
                        }
                    }
                    else if (entity is voxel)
                    {
                        loadingEntities++;
                        if (ProcessVoxelEntity((voxel)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing voxel entity.");
                            return false;
                        }
                    }
                    else if (entity is audio)
                    {
                        loadingEntities++;
                        if (ProcessAudioEntity((audio)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing audio entity.");
                            return false;
                        }
                    }
                    else if (entity is image)
                    {
                        loadingEntities++;
                        if (ProcessImageEntity((image)entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing image entity.");
                            return false;
                        }
                    }
                    else if (entity is cubemesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.cubeMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing cube mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is spheremesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.sphereMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing sphere mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is capsulemesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.capsuleMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing capsule mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is cylindermesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.cylinderMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing cylinder mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is planemesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.planeMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing plane mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is torusmesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.torusMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing torus mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is conemesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.coneMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing cone mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is rectangularpyramidmesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.rectangularPyramidMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing rectangular pyramid mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is tetrahedronmesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.tetrahedronMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing tetrahedron mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is prismmesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.prismMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing prism mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is archmesh)
                    {
                        loadingEntities++;
                        if (ProcessMeshPrimitiveEntity(entity, Javascript.APIs.Entity.EntityAPIHelper.archMeshPrefab, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing arch mesh entity.");
                            return false;
                        }
                    }
                    else if (entity is water)
                    {
                        loadingEntities++;
                        if (ProcessWaterEntity((water) entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing water entity.");
                            return false;
                        }
                    }
                    else if (entity is waterblocker)
                    {
                        loadingEntities++;
                        if (ProcessWaterBlockerEntity((waterblocker) entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing water blocker entity.");
                            return false;
                        }
                    }
                    else if (entity is automobile)
                    {
                        loadingEntities++;
                        if (ProcessAutomobileEntity((automobile) entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing automobile entity.");
                            return false;
                        }
                    }
                    else if (entity is airplane)
                    {
                        loadingEntities++;
                        if (ProcessAirplaneEntity((airplane) entity, baseURI) == false)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessEntity] Error processing airplane entity.");
                            return false;
                        }
                    }
                    else
                    {
                        Logging.LogWarning("[VEMLHandler->ApplyVEMLDocument] Unknown kind of entity: " + typeof(entity).ToString());
                    }
                }

                StartCoroutine(ApplyEntityHierarchy(entities, baseURI));
            }

            return true;
        }

        /// <summary>
        /// Process a mesh entity.
        /// </summary>
        /// <param name="entity">The mesh entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessMeshEntity(mesh entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (entity.meshresource == null)
            {
                Logging.LogWarning("[VEMLHandler->ProcessMeshEntity] VEML document environment mesh entity missing required field: mesh-resource.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.meshname))
            {
                Logging.LogWarning("[VEMLHandler->ProcessMeshEntity] VEML document environment mesh entity missing required field: mesh-name.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessMeshEntity] VEML document environment mesh entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Action<StraightFour.Entity.MeshEntity> onLoadedAction
                = new Action<StraightFour.Entity.MeshEntity>((meshEntity) =>
                {
                    meshEntity.entityTag = entity.tag;
                    meshEntity.SetVisibility(true);
                    meshEntity.SetParent(null);
                    meshEntity.SetInteractionState(BaseEntity.InteractionState.Static);
                    ApplyTransform(meshEntity, entity.transform, true, true, false);
#if USE_WEBINTERFACE
                    if (!string.IsNullOrEmpty(entity.synchronizer))
                    {
                        Tuple<VOSSynchronizer, Guid> synchronizer
                            = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                        if (synchronizer == null || synchronizer.Item1 == null)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessMeshEntity] Error synchronizing entity.");
                            return;
                        }
                        synchronizer.Item1.AddSynchronizedEntity(meshEntity, false, entity.meshname);
                    }
#endif
                    Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(meshEntity);
                    if (entity.placementsocket != null)
                    {
                        foreach (placementsocket sock in entity.placementsocket)
                        {
                            if (sock == null || sock.position == null || sock.rotation == null || sock.connectingoffset == null)
                            {
                                LogSystem.LogWarning("[VEMLHandler->ProcessMeshEntity] Invalid placement socket.");
                                return;
                            }

                            meshEntity.AddSocket(new Vector3((float)sock.position.x, (float)sock.position.y, (float)sock.position.z),
                                new Quaternion((float)sock.rotation.x, (float)sock.rotation.y, (float)sock.rotation.z, (float)sock.rotation.w),
                                new Vector3((float)sock.connectingoffset.x, (float)sock.connectingoffset.y, (float)sock.connectingoffset.z));
                        }
                    }
                    loadingEntities--;
                });
            List<string> resources = new List<string>();
            if (entity.meshresource != null)
            {
                foreach (string res in entity.meshresource)
                {
                    resources.Add(VEMLUtilities.FullyQualifyURI(res, formattedBaseURI));
                }
            }
            WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsMeshEntity(VEMLUtilities.FullyQualifyURI(entity.meshname, formattedBaseURI),
                resources.ToArray(), Guid.Parse(entity.id), onLoadedAction);

            return true;
        }

        /// <summary>
        /// Process a container entity.
        /// </summary>
        /// <param name="entity">The container entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessContainerEntity(container entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessContainerEntity] VEML document environment container entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector3 positionValue;
            Quaternion rotationValue;
            Vector3 sizeValue;
            if (entity.transform is scaletransform)
            {
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((scaletransform) entity.transform).scale.x,
                    (float) ((scaletransform) entity.transform).scale.y,
                    (float) ((scaletransform) entity.transform).scale.z);
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessContainerEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessContainerEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessContainerEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessContainerEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessContainerEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadContainerEntity(null, positionValue, rotationValue,
                sizeValue, Guid.Parse(entity.id), entity.tag, false, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process a character entity.
        /// </summary>
        /// <param name="entity">The character entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessCharacterEntity(character entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessCharacterEntity] VEML document environment character entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            bool isSize;
            Vector3 positionValue;
            Quaternion rotationValue;
            Vector3 sizeValue;
            if (entity.transform is scaletransform)
            {
                isSize = false;
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((scaletransform) entity.transform).scale.x,
                    (float) ((scaletransform) entity.transform).scale.y,
                    (float) ((scaletransform) entity.transform).scale.z);
            }
            else if (entity.transform is sizetransform)
            {
                isSize = true;
                positionValue = new Vector3((float) ((sizetransform) entity.transform).position.x,
                    (float) ((sizetransform) entity.transform).position.y,
                    (float) ((sizetransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((sizetransform) entity.transform).rotation.x,
                    (float) ((sizetransform) entity.transform).rotation.y,
                    (float) ((sizetransform) entity.transform).rotation.z,
                    (float) ((sizetransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((sizetransform) entity.transform).size.x,
                    (float) ((sizetransform) entity.transform).size.y,
                    (float) ((sizetransform) entity.transform).size.z);
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessCharacterEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessCharacterEntity] Unknown transform type.");
                return false;
            }

            Action<CharacterEntity> onLoadEvent = new Action<CharacterEntity>((ce) =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessCharacterEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.entityTag = entity.tag;
                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessCharacterEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false,
                        VEMLUtilities.FullyQualifyURI(entity.meshname, formattedBaseURI), entity.meshresource,
                        new Vector3((float) entity.labeloffset.x, (float) entity.labeloffset.y, (float) entity.labeloffset.z));
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            Action onLoadDefaultEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessCharacterEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.entityTag = entity.tag;
                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessCharacterEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false, null, null, null);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            if (entity.meshname == null)
            {
                StraightFour.StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(null, null, Vector3.zero, Quaternion.identity,
                    Vector3.zero, positionValue, rotationValue, sizeValue, Guid.Parse(entity.id), entity.tag, isSize, onLoadDefaultEvent);
            }
            else
            {
                WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsCharacterEntity(entity.meshname, entity.meshresource,
                    entity.meshoffset == null ? Vector3.zero :
                    new Vector3((float) entity.meshoffset.x, (float) entity.meshoffset.y, (float) entity.meshoffset.z),
                    entity.meshrotation == null ? Quaternion.identity :
                    new Quaternion((float) entity.meshrotation.x, (float) entity.meshrotation.y,
                    (float) entity.meshrotation.z, (float) entity.meshrotation.w),
                    entity.labeloffset == null ? Vector3.zero :
                    new Vector3((float) entity.labeloffset.x, (float) entity.labeloffset.y, (float) entity.labeloffset.z),
                    Guid.Parse(entity.id), onLoadEvent);
            }

            return true;
        }

        /// <summary>
        /// Process a light entity.
        /// </summary>
        /// <param name="entity">The light entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessLightEntity(light entity, string baseURI)
        {
            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessLightEntity] VEML document environment light entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector3 positionValue;
            Quaternion rotationValue;
            if (entity.transform is scaletransform)
            {
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessLightEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessLightEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessLightEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessLightEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessLightEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadLightEntity(null, positionValue, rotationValue,
                Guid.Parse(entity.id), entity.tag, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process a terrain entity.
        /// </summary>
        /// <param name="entity">The terrain entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessTerrainEntity(terrain entity, string baseURI)
        {
            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessTerrainEntity] VEML document environment terrain entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector3 positionValue;
            Quaternion rotationValue;
            Vector3 sizeValue;
            if (entity.transform is scaletransform)
            {
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((scaletransform) entity.transform).scale.x,
                    (float) ((scaletransform) entity.transform).scale.y,
                    (float) ((scaletransform) entity.transform).scale.z);
            }
            else if (entity.transform is sizetransform)
            {
                positionValue = new Vector3((float) ((sizetransform) entity.transform).position.x,
                    (float) ((sizetransform) entity.transform).position.y,
                    (float) ((sizetransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((sizetransform) entity.transform).rotation.x,
                    (float) ((sizetransform) entity.transform).rotation.y,
                    (float) ((sizetransform) entity.transform).rotation.z,
                    (float) ((sizetransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((sizetransform) entity.transform).size.x,
                    (float) ((sizetransform) entity.transform).size.y,
                    (float) ((sizetransform) entity.transform).size.z);
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessTerrainEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessTerrainEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessTerrainEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessTerrainEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            if (entity.type == "heightmap")
            {
                if (entity.layer == null)
                {
                    LogSystem.LogWarning("[VEMLHandler->ProcessTerrainEntity] At least 1 layer required for Heightmap Terrain Entity.");
                    return false;
                }

                List<StraightFour.Entity.Terrain.TerrainEntityLayer> layers
                    = new List<StraightFour.Entity.Terrain.TerrainEntityLayer>();
                foreach (terrainlayer layer in entity.layer)
                {
                    Color specValue = ProcessColor(layer.specular);
                    StraightFour.Entity.Terrain.TerrainEntityLayer tel
                        = new StraightFour.Entity.Terrain.TerrainEntityLayer();
                    tel.normalPath = layer.normaltexture;
                    tel.maskPath = layer.masktexture;
                    tel.diffusePath = layer.diffusetexture;
                    tel.smoothness = layer.smoothness;
                    tel.metallic = layer.metallic;
                    tel.specular = specValue;
                    layers.Add(tel);
                }

                float[,] heights = Handlers.VEML.VEMLUtilities.ParseCSVHeights(entity.heights);

                StraightFour.StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(
                    (float) entity.length, (float) entity.width, (float) entity.height,
                    heights, layers.ToArray(), VEMLUtilities.ParseCSVLayerMasksToInternalFormat(entity.layermasks), null, positionValue, rotationValue, false, Guid.Parse(entity.id),
                    entity.tag, onLoadEvent);
            }
            else if (entity.type == "voxel")
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessTerrainEntity] Voxel Terrain Entities not currently supported.");
                return false;
            }
            else if (entity.type == "hybrid")
            {
                if (entity.layer == null)
                {
                    LogSystem.LogWarning("[VEMLHandler->ProcessTerrainEntity] At least 1 layer required for Hybrid Terrain Entity.");
                    return false;
                }

                List<Javascript.APIs.Entity.TerrainEntityLayer> layers = new List<Javascript.APIs.Entity.TerrainEntityLayer>();
                foreach (terrainlayer layer in entity.layer)
                {
                    Color specValue = ProcessColor(layer.specular);
                    Javascript.APIs.Entity.TerrainEntityLayer tel
                        = new Javascript.APIs.Entity.TerrainEntityLayer();
                    tel.normalTexture = layer.normaltexture;
                    tel.maskTexture = layer.masktexture;
                    tel.diffuseTexture = layer.diffusetexture;
                    tel.smoothness = layer.smoothness;
                    tel.metallic = layer.metallic;
                    tel.specular = new Javascript.APIs.WorldTypes.Color(specValue.r, specValue.g, specValue.b, specValue.a);
                    layers.Add(tel);
                }

                if (string.IsNullOrEmpty(entity.layermasks))
                {
                    LogSystem.LogWarning("[VEMLHandler->ProcessTerrainEntity] At least 1 layer mask required for Hybrid Terrain Entity.");
                    return false;
                }

                float[][] heights = VEMLUtilities.ParseCSVHeightsArrayOfArray(entity.heights);

                Javascript.APIs.Entity.EntityAPIHelper.LoadHybridTerrainEntityAsync(
                    null, (float) entity.length, (float) entity.width, (float) entity.height, heights,
                    layers.ToArray(), VEMLUtilities.ParseCSVLayerMasks(entity.layermasks), null,
                    new Javascript.APIs.WorldTypes.Vector3(positionValue.x, positionValue.y, positionValue.z),
                    new Javascript.APIs.WorldTypes.Quaternion(rotationValue.x, rotationValue.y, rotationValue.z, rotationValue.w),
                    false, entity.id, entity.tag, onLoadEvent);
            }

            return true;
        }

        /// <summary>
        /// Process a text entity.
        /// </summary>
        /// <param name="entity">The text entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessTextEntity(text entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessTextEntity] VEML document environment text entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector2 positionPercentValue;
            Vector2 sizePercentValue;
            if (entity.transform is scaletransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessTextEntity] Scale Transform not allowed.");
                return false;
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessTextEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                positionPercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).positionpercent.x,
                    (float) ((canvastransform) entity.transform).positionpercent.y);
                sizePercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).sizepercent.x,
                    (float) ((canvastransform) entity.transform).sizepercent.y);
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessTextEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessTextEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessTextEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                TextEntity textEntity = loadedEntity as TextEntity;
                if (textEntity != null)
                {
                    if (entity.boldSpecified && entity.bold)
                    {
                        textEntity.SetBold(true);
                    }
                    if (entity.italicSpecified && entity.italic)
                    {
                        textEntity.SetItalic(true);
                    }
                    if (entity.underlineSpecified && entity.underline)
                    {
                        textEntity.SetUnderline(true);
                    }
                    if (entity.strikethroughSpecified && entity.strikethrough)
                    {
                        textEntity.SetStrikethrough(true);
                    }
                    if (!string.IsNullOrEmpty(entity.font))
                    {
                        textEntity.SetFont(entity.font);
                    }
                    if (!string.IsNullOrEmpty(entity.color))
                    {
                        Color col = ProcessColor(entity.color);
                        textEntity.SetColor(col);
                    }
                    if (!string.IsNullOrEmpty(entity.textwrap))
                    {
                        if (entity.textwrap == "no-wrap")
                        {
                            textEntity.SetTextWrapping(TextWrapping.NoWrap);
                        }
                        else if (entity.textwrap == "wrap")
                        {
                            textEntity.SetTextWrapping(TextWrapping.Wrap);
                        }
                    }
                    if (!string.IsNullOrEmpty(entity.textalignhorizontal))
                    {
                        if (entity.textalignhorizontal == "left")
                        {
                            textEntity.SetTextAlignment(StraightFour.Entity.TextAlignment.Left);
                        }
                        else if (entity.textalignhorizontal == "center")
                        {
                            textEntity.SetTextAlignment(StraightFour.Entity.TextAlignment.Center);
                        }
                        else if (entity.textalignhorizontal == "right")
                        {
                            textEntity.SetTextAlignment(StraightFour.Entity.TextAlignment.Right);
                        }
                    }
                    if (!string.IsNullOrEmpty(entity.textalignvertical))
                    {
                        if (entity.textalignvertical == "top")
                        {
                            textEntity.SetTextAlignment(StraightFour.Entity.TextAlignment.Top);
                        }
                        else if (entity.textalignvertical == "middle")
                        {
                            textEntity.SetTextAlignment(StraightFour.Entity.TextAlignment.Middle);
                        }
                        else if (entity.textalignvertical == "bottom")
                        {
                            textEntity.SetTextAlignment(StraightFour.Entity.TextAlignment.Bottom);
                        }
                    }
                }
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadTextEntity(entity.text1, (int) entity.fontsize,
                null, positionPercentValue, sizePercentValue, Guid.Parse(entity.id), entity.tag, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process a button entity.
        /// </summary>
        /// <param name="entity">The button entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessButtonEntity(button entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessButtonEntity] VEML document environment button entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector2 positionPercentValue;
            Vector2 sizePercentValue;
            if (entity.transform is scaletransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessButtonEntity] Scale Transform not allowed.");
                return false;
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessButtonEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                positionPercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).positionpercent.x,
                    (float) ((canvastransform) entity.transform).positionpercent.y);
                sizePercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).sizepercent.x,
                    (float) ((canvastransform) entity.transform).sizepercent.y);
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessButtonEntity] Unknown transform type.");
                return false;
            }

            Action onClickEvent = new Action(() =>
            {
                if (WebVerseRuntime.Instance.inputManager.inputEnabled)
                {
                    if (!string.IsNullOrEmpty(entity.onclickevent))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.Run(entity.onclickevent);
                    }
                }
            });

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessButtonEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessButtonEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                if (!string.IsNullOrEmpty(entity.color))
                {
                    Color col = ProcessColor(entity.color);
                    ((ButtonEntity) loadedEntity).SetBaseColor(col);
                }
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadButtonEntity(null, positionPercentValue,
                sizePercentValue, onClickEvent, Guid.Parse(entity.id), entity.tag, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process a canvas entity.
        /// </summary>
        /// <param name="entity">The canvas entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessCanvasEntity(canvas entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessCanvasEntity] VEML document environment canvas entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            bool isSize;
            Vector3 positionValue;
            Quaternion rotationValue;
            Vector3 sizeValue;
            if (entity.transform is scaletransform)
            {
                isSize = false;
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((scaletransform) entity.transform).scale.x,
                    (float) ((scaletransform) entity.transform).scale.y,
                    (float) ((scaletransform) entity.transform).scale.z);
            }
            else if (entity.transform is sizetransform)
            {
                isSize = true;
                positionValue = new Vector3((float) ((sizetransform) entity.transform).position.x,
                    (float) ((sizetransform) entity.transform).position.y,
                    (float) ((sizetransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((sizetransform) entity.transform).rotation.x,
                    (float) ((sizetransform) entity.transform).rotation.y,
                    (float) ((sizetransform) entity.transform).rotation.z,
                    (float) ((sizetransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((sizetransform) entity.transform).size.x,
                    (float) ((sizetransform) entity.transform).size.y,
                    (float) ((sizetransform) entity.transform).size.z);
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessCanvasEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessCanvasEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessCanvasEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);

                // Default to screen canvas.
                ((CanvasEntity) loadedEntity).MakeScreenCanvas();

#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessCanvasEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadCanvasEntity(null, positionValue, rotationValue,
                sizeValue, Guid.Parse(entity.id), isSize, entity.tag, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process an HTML entity.
        /// </summary>
        /// <param name="entity">The canvas entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessHTMLEntity(html entity, string baseURI)
        {
#if NON_CANVAS_HTML_ENTITY
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessHTMLEntity] VEML document environment HTML entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            bool isSize;
            Vector3 positionValue;
            Quaternion rotationValue;
            Vector3 sizeValue;
            if (entity.transform is scaletransform)
            {
                isSize = false;
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((scaletransform) entity.transform).scale.x,
                    (float) ((scaletransform) entity.transform).scale.y,
                    (float) ((scaletransform) entity.transform).scale.z);
            }
            else if (entity.transform is sizetransform)
            {
                isSize = true;
                positionValue = new Vector3((float) ((sizetransform) entity.transform).position.x,
                    (float) ((sizetransform) entity.transform).position.y,
                    (float) ((sizetransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((sizetransform) entity.transform).rotation.x,
                    (float) ((sizetransform) entity.transform).rotation.y,
                    (float) ((sizetransform) entity.transform).rotation.z,
                    (float) ((sizetransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((sizetransform) entity.transform).size.x,
                    (float) ((sizetransform) entity.transform).size.y,
                    (float) ((sizetransform) entity.transform).size.z);
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessHTMLEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessHTMLEntity] Unknown transform type.");
                return false;
            }
            
            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessHTMLEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);

                if (!string.IsNullOrEmpty(entity.url))
                {
                    ((HTMLEntity) loadedEntity).LoadFromURL(entity.url);
                }

#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessHTMLEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadHTMLEntity(null, positionValue, rotationValue,
                sizeValue, Guid.Parse(entity.id), isSize, entity.tag, onLoadEvent);

            return true;
#endif
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessHTMLEntity] VEML document environment HTML entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector2 positionPercentValue;
            Vector2 sizePercentValue;
            if (entity.transform is scaletransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessHTMLEntity] Scale Transform not allowed.");
                return false;
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessHTMLEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                positionPercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).positionpercent.x,
                    (float) ((canvastransform) entity.transform).positionpercent.y);
                sizePercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).sizepercent.x,
                    (float) ((canvastransform) entity.transform).sizepercent.y);
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessHTMLEntity] Unknown transform type.");
                return false;
            }

            Action<string> onMessageEvent = null;
            if (entity.onmessage != null)
            {
                onMessageEvent = (msg) =>
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(entity.onmessage, new object[] { msg });
                };
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessHTMLEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);

                if (!string.IsNullOrEmpty(entity.url))
                {
                    ((HTMLUIElementEntity) loadedEntity).LoadFromURL(VEMLUtilities.FullyQualifyURI(entity.url, baseURI));
                }

#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessHTMLEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);

                WebVerseRuntime.Instance.outputManager.RegisterScreenSizeChangeAction(new Action<int, int>((width, height) =>
                {
                    ((StraightFour.Entity.HTMLUIElementEntity) loadedEntity).CorrectSizeAndPosition(width, height);
                }));

                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadHTMLUIElementEntity(null, positionPercentValue,
                sizePercentValue, Guid.Parse(entity.id), entity.tag, onMessageEvent, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process an input entity.
        /// </summary>
        /// <param name="entity">The input entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessInputEntity(input entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessInputEntity] VEML document environment input entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector2 positionPercentValue;
            Vector2 sizePercentValue;
            if (entity.transform is scaletransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessInputEntity] Scale Transform not allowed.");
                return false;
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessInputEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                positionPercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).positionpercent.x,
                    (float) ((canvastransform) entity.transform).positionpercent.y);
                sizePercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).sizepercent.x,
                    (float) ((canvastransform) entity.transform).sizepercent.y);
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessInputEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessInputEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessInputEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadInputEntity(null, positionPercentValue,
                sizePercentValue, Guid.Parse(entity.id), entity.tag, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process a voxel entity.
        /// </summary>
        /// <param name="entity">The voxel entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessVoxelEntity(voxel entity, string baseURI)
        {
            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessVoxelEntity] VEML document environment voxel entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector3 positionValue;
            Quaternion rotationValue;
            Vector3 sizeValue;
            if (entity.transform is scaletransform)
            {
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((scaletransform) entity.transform).scale.x,
                    (float) ((scaletransform) entity.transform).scale.y,
                    (float) ((scaletransform) entity.transform).scale.z);
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessVoxelEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessVoxelEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessVoxelEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessVoxelEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessVoxelEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(null, positionValue, rotationValue,
                sizeValue, Guid.Parse(entity.id), entity.tag, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process an audio entity.
        /// </summary>
        /// <param name="entity">The audio entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessAudioEntity(audio entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessAudioEntity] VEML document environment audio entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector3 positionValue;
            Quaternion rotationValue;
            if (entity.transform is scaletransform)
            {
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessAudioEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessAudioEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessAudioEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessAudioEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);

                string fullAudioFilePath = VEML.VEMLUtilities.FullyQualifyURI(entity.audiofile, formattedBaseURI);

                Javascript.APIs.Entity.EntityAPIHelper.LoadAudioFromFileAsync(
                    fullAudioFilePath, (AudioEntity) loadedEntity);
                ((AudioEntity) loadedEntity).loop = entity.loop;
                ((AudioEntity) loadedEntity).priority = entity.priority;
                ((AudioEntity) loadedEntity).volume = entity.volume;
                ((AudioEntity) loadedEntity).pitch = entity.pitch;
                ((AudioEntity) loadedEntity).stereoPan = entity.stereopan;
                if (entity.autoplay)
                {
                    ((AudioEntity) loadedEntity).Play();
                }
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessAudioEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadAudioEntity(null, positionValue, rotationValue,
                Guid.Parse(entity.id), entity.tag, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process an image entity.
        /// </summary>
        /// <param name="entity">The image entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessImageEntity(image entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessImageEntity] VEML document environment audio image missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector2 positionPercentValue;
            Vector2 sizePercentValue;
            if (entity.transform is scaletransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessImageEntity] Scale Transform not allowed.");
                return false;
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessImageEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                positionPercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).positionpercent.x,
                    (float) ((canvastransform) entity.transform).positionpercent.y);
                sizePercentValue = new Vector2(
                    (float) ((canvastransform) entity.transform).sizepercent.x,
                    (float) ((canvastransform) entity.transform).sizepercent.y);
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessImageEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessImageEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessImageEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(entity.imagefile, formattedBaseURI),
                new Action<Texture2D>((tex) =>
                {
                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadImageEntity(
                        tex, null, positionPercentValue, sizePercentValue,
                        Guid.Parse(entity.id), entity.tag, onLoadEvent);
                }));
            

            return true;
        }

        /// <summary>
        /// Process a mesh entity.
        /// </summary>
        /// <param name="entity">The mesh primitive entity.</param>
        /// <param name="primitive">Primitive to load the mesh from.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessMeshPrimitiveEntity(entity entity, GameObject primitive, string baseURI)
        {
            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessMeshPrimitiveEntity] VEML document environment " +
                    "mesh primitive entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Action onLoadedAction = new Action(() =>
            {
                BaseEntity rawEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (rawEntity == null)
                {
                    Logging.LogError("[VEMLHandler->ProcessMeshPrimitiveEntity] Unable to find loaded entity.");
                    return;
                }

                MeshEntity meshEntity = (MeshEntity) rawEntity;

                meshEntity.entityTag = entity.tag;
                meshEntity.SetVisibility(true);
                meshEntity.SetParent(null);
                ApplyTransform(meshEntity, entity.transform, true, true, false);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessMeshPrimitiveEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(meshEntity, false, null);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(meshEntity);
                if (entity.placementsocket != null)
                {
                    foreach (placementsocket sock in entity.placementsocket)
                    {
                        if (sock == null || sock.position == null || sock.rotation == null || sock.connectingoffset == null)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessMeshPrimitiveEntity] Invalid placement socket.");
                            return;
                        }

                        meshEntity.AddSocket(new Vector3((float) sock.position.x, (float) sock.position.y, (float) sock.position.z),
                            new Quaternion((float) sock.rotation.x, (float) sock.rotation.y, (float) sock.rotation.z, (float) sock.rotation.w),
                            new Vector3((float) sock.connectingoffset.x, (float) sock.connectingoffset.y, (float) sock.connectingoffset.z));
                    }
                }
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadMeshEntity(null, primitive, Vector3.zero, Quaternion.identity,
                Guid.Parse(entity.id), null, onLoadedAction);

            return true;
        }

        /// <summary>
        /// Process a water entity.
        /// </summary>
        /// <param name="entity">The water entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessWaterEntity(water entity, string baseURI)
        {
            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessWaterEntity] VEML document environment water entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector3 positionValue;
            Quaternion rotationValue;
            Vector3 sizeValue;
            if (entity.transform is scaletransform)
            {
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((scaletransform) entity.transform).scale.x,
                    (float) ((scaletransform) entity.transform).scale.y,
                    (float) ((scaletransform) entity.transform).scale.z);
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessWaterEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessWaterEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessWaterEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessWaterEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
                loadedEntity.SetInteractionState(BaseEntity.InteractionState.Static);
                loadedEntity.SetScale(sizeValue);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessWaterEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadWaterBodyEntity(
                ProcessColor(entity.shallowcolor), ProcessColor(entity.deepcolor), ProcessColor(entity.specularcolor),
                ProcessColor(entity.scatteringcolor), entity.deepstart, entity.deepend, entity.distortion, entity.smoothness,
                entity.numwaves, entity.waveamplitude, entity.wavesteepness, entity.wavespeed, entity.wavelength, entity.wavescale,
                entity.waveintensity, null, positionValue, rotationValue, Guid.Parse(entity.id), entity.tag, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process a water blocker entity.
        /// </summary>
        /// <param name="entity">The water blocker entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessWaterBlockerEntity(waterblocker entity, string baseURI)
        {
            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessWaterBlockerEntity] VEML document environment water blocker entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Vector3 positionValue;
            Quaternion rotationValue;
            Vector3 sizeValue;
            if (entity.transform is scaletransform)
            {
                positionValue = new Vector3((float) ((scaletransform) entity.transform).position.x,
                    (float) ((scaletransform) entity.transform).position.y,
                    (float) ((scaletransform) entity.transform).position.z);
                rotationValue = new Quaternion((float) ((scaletransform) entity.transform).rotation.x,
                    (float) ((scaletransform) entity.transform).rotation.y,
                    (float) ((scaletransform) entity.transform).rotation.z,
                    (float) ((scaletransform) entity.transform).rotation.w);
                sizeValue = new Vector3((float) ((scaletransform) entity.transform).scale.x,
                    (float) ((scaletransform) entity.transform).scale.y,
                    (float) ((scaletransform) entity.transform).scale.z);
            }
            else if (entity.transform is sizetransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessWaterBlockerEntity] Size Transform not allowed.");
                return false;
            }
            else if (entity.transform is canvastransform)
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessWaterBlockerEntity] Canvas Transform not allowed.");
                return false;
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ProcessWaterBlockerEntity] Unknown transform type.");
                return false;
            }

            Action onLoadEvent = new Action(() =>
            {
                BaseEntity loadedEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entity.id));
                if (loadedEntity == null)
                {
                    LogSystem.LogError("[VEMLHandler->ProcessWaterBlockerEntity] Error finding entity to synchronize.");
                    return;
                }

                loadedEntity.SetVisibility(true);
#if USE_WEBINTERFACE
                if (!string.IsNullOrEmpty(entity.synchronizer))
                {
                    Tuple<VOSSynchronizer, Guid> synchronizer
                        = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                    if (synchronizer == null || synchronizer.Item1 == null)
                    {
                        LogSystem.LogWarning("[VEMLHandler->ProcessWaterBlockerEntity] Error synchronizing entity.");
                        return;
                    }
                    synchronizer.Item1.AddSynchronizedEntity(loadedEntity, false);
                }
#endif
                Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(loadedEntity);
                loadingEntities--;
            });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadWaterBlockerEntity(null,
                positionValue, rotationValue, Guid.Parse(entity.id), entity.tag, onLoadEvent);

            return true;
        }

        /// <summary>
        /// Process an automobile entity.
        /// </summary>
        /// <param name="entity">The automobile entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessAutomobileEntity(automobile entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (entity.meshresource == null)
            {
                Logging.LogWarning("[VEMLHandler->ProcessAutomobileEntity] VEML document environment automobile entity missing required field: mesh-resource.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.meshname))
            {
                Logging.LogWarning("[VEMLHandler->ProcessAutomobileEntity] VEML document environment automobile entity missing required field: mesh-name.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessAutomobileEntity] VEML document environment automobile entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Action<StraightFour.Entity.AutomobileEntity> onLoadedAction
                = new Action<StraightFour.Entity.AutomobileEntity>((automobileEntity) =>
                {
                    automobileEntity.entityTag = entity.tag;
                    automobileEntity.SetVisibility(true);
                    automobileEntity.SetParent(null);
                    automobileEntity.SetInteractionState(BaseEntity.InteractionState.Static);
                    ApplyTransform(automobileEntity, entity.transform, true, true, false);
#if USE_WEBINTERFACE
                    if (!string.IsNullOrEmpty(entity.synchronizer))
                    {
                        Tuple<VOSSynchronizer, Guid> synchronizer
                            = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                        if (synchronizer == null || synchronizer.Item1 == null)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessAutomobileEntity] Error synchronizing entity.");
                            return;
                        }
                        synchronizer.Item1.AddSynchronizedEntity(automobileEntity, false, entity.meshname);
                    }
#endif
                    Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(automobileEntity);
                    if (entity.placementsocket != null)
                    {
                        foreach (placementsocket sock in entity.placementsocket)
                        {
                            if (sock == null || sock.position == null || sock.rotation == null || sock.connectingoffset == null)
                            {
                                LogSystem.LogWarning("[VEMLHandler->ProcessAutomobileEntity] Invalid placement socket.");
                                return;
                            }

                            automobileEntity.AddSocket(new Vector3((float)sock.position.x, (float)sock.position.y, (float)sock.position.z),
                                new Quaternion((float)sock.rotation.x, (float)sock.rotation.y, (float)sock.rotation.z, (float)sock.rotation.w),
                                new Vector3((float)sock.connectingoffset.x, (float)sock.connectingoffset.y, (float)sock.connectingoffset.z));
                        }
                    }
                    loadingEntities--;
                });
            List<string> resources = new List<string>();
            if (entity.meshresource != null)
            {
                foreach (string res in entity.meshresource)
                {
                    resources.Add(VEMLUtilities.FullyQualifyURI(res, formattedBaseURI));
                }
            }

            List<Javascript.APIs.Entity.AutomobileEntityWheel> convertedWheels
                = new List<Javascript.APIs.Entity.AutomobileEntityWheel>();
            if (entity.wheels != null)
            {
                foreach (automobilewheel wheel in entity.wheels)
                {
                    convertedWheels.Add(new Javascript.APIs.Entity.AutomobileEntityWheel(
                        wheel.wheelsubmesh, wheel.wheelradius));
                }
            }

            Javascript.APIs.Entity.AutomobileEntity.AutomobileType convertedType =
                Javascript.APIs.Entity.AutomobileEntity.AutomobileType.Default;
            if (entity.automobiletype != null)
            {
                switch (entity.automobiletype.ToLower())
                {
                    case "default":
                    default:
                        convertedType = Javascript.APIs.Entity.AutomobileEntity.AutomobileType.Default;
                        break;
                }
            }

            WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAutomobileEntity(
                VEMLUtilities.FullyQualifyURI(entity.meshname, formattedBaseURI),
                resources.ToArray(), Vector3.one, Quaternion.identity, convertedWheels.ToArray(),
                entity.mass, convertedType, Guid.Parse(entity.id), onLoadedAction);

            return true;
        }

        /// <summary>
        /// Process an airplane entity.
        /// </summary>
        /// <param name="entity">The airplane entity.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        /// <returns>Whether or not the operation succeeded.</returns>
        private bool ProcessAirplaneEntity(airplane entity, string baseURI)
        {
            string formattedBaseURI = VEMLUtilities.FormatURI(baseURI);

            if (entity.meshresource == null)
            {
                Logging.LogWarning("[VEMLHandler->ProcessAirplaneEntity] VEML document environment airplane entity missing required field: mesh-resource.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.meshname))
            {
                Logging.LogWarning("[VEMLHandler->ProcessAirplaneEntity] VEML document environment airplane entity missing required field: mesh-name.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.tag))
            {
                Logging.LogWarning("[VEMLHandler->ProcessAirplaneEntity] VEML document environment airplane entity missing required field: tag.");
                return false;
            }

            if (string.IsNullOrEmpty(entity.id))
            {
                entity.id = Guid.NewGuid().ToString();
            }

            Action<StraightFour.Entity.AirplaneEntity> onLoadedAction
                = new Action<StraightFour.Entity.AirplaneEntity>((airplaneEntity) =>
                {
                    airplaneEntity.entityTag = entity.tag;
                    airplaneEntity.SetVisibility(true);
                    airplaneEntity.SetParent(null);
                    airplaneEntity.SetInteractionState(BaseEntity.InteractionState.Static);
                    ApplyTransform(airplaneEntity, entity.transform, true, true, false);
#if USE_WEBINTERFACE
                    if (!string.IsNullOrEmpty(entity.synchronizer))
                    {
                        Tuple<VOSSynchronizer, Guid> synchronizer
                            = WebVerseRuntime.Instance.vosSynchronizationManager.GetSynchronizerAndSession(entity.synchronizer);
                        if (synchronizer == null || synchronizer.Item1 == null)
                        {
                            LogSystem.LogWarning("[VEMLHandler->ProcessAirplaneEntity] Error synchronizing entity.");
                            return;
                        }
                        synchronizer.Item1.AddSynchronizedEntity(airplaneEntity, false, entity.meshname);
                    }
#endif
                    Javascript.APIs.Entity.EntityAPIHelper.RegisterPrivateEntity(airplaneEntity);
                    if (entity.placementsocket != null)
                    {
                        foreach (placementsocket sock in entity.placementsocket)
                        {
                            if (sock == null || sock.position == null || sock.rotation == null || sock.connectingoffset == null)
                            {
                                LogSystem.LogWarning("[VEMLHandler->ProcessAirplaneEntity] Invalid placement socket.");
                                return;
                            }

                            airplaneEntity.AddSocket(new Vector3((float)sock.position.x, (float)sock.position.y, (float)sock.position.z),
                                new Quaternion((float)sock.rotation.x, (float)sock.rotation.y, (float)sock.rotation.z, (float)sock.rotation.w),
                                new Vector3((float)sock.connectingoffset.x, (float)sock.connectingoffset.y, (float)sock.connectingoffset.z));
                        }
                    }
                    loadingEntities--;
                });
            List<string> resources = new List<string>();
            if (entity.meshresource != null)
            {
                foreach (string res in entity.meshresource)
                {
                    resources.Add(VEMLUtilities.FullyQualifyURI(res, formattedBaseURI));
                }
            }

            WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAirplaneEntity(
                VEMLUtilities.FullyQualifyURI(entity.meshname, formattedBaseURI),
                resources.ToArray(), Vector3.one, Quaternion.identity, entity.mass,
                Guid.Parse(entity.id), onLoadedAction);

            return true;
        }

        /// <summary>
        /// Apply the hierarchy of entities.
        /// </summary>
        /// <param name="entities">Dictionary of entities and their parents.</param>
        /// <param name="baseURI">Base URI of the VEML document.</param>
        private IEnumerator ApplyEntityHierarchy(Dictionary<entity, entity> entities, string baseURI)
        {
            if (entities == null)
            {
                LogSystem.Log("[VEMLHandler->ApplyEntityHierarchy] No entities found.");
            }

            float elapsedTime = 0;
            while (loadingEntities > 0)
            {
                if (elapsedTime > timeout)
                {
                    Logging.LogError("[VEMLHandler->ApplyEntityHierarchy] Load timed out.");
                    yield break;
                }
                elapsedTime += 0.25f;
                yield return new WaitForSeconds(0.25f);
            }

            foreach (KeyValuePair<entity, entity> entityParentRelationship in entities)
            {
                if (entityParentRelationship.Key == null)
                {
                    LogSystem.LogError("[VEMLHandler->ApplyEntityHierarchy] Invalid entity parent relationship.");
                    yield break;
                }

                if (string.IsNullOrEmpty(entityParentRelationship.Key.id))
                {
                    LogSystem.LogError("[VEMLHandler->ApplyEntityHierarchy] Invalid entity.");
                    yield break;
                }

                BaseEntity entityToSet =
                    StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityParentRelationship.Key.id));

                BaseEntity entityParent = entityParentRelationship.Value == null ? null :
                    string.IsNullOrEmpty(entityParentRelationship.Value.id) ? null :
                    StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(Guid.Parse(entityParentRelationship.Value.id));

                if (entityToSet == null)
                {
                    LogSystem.LogError("[VEMLHandler->ApplyEntityHierarchy] Error finding entity to set parent of.");
                    yield break;
                }

                if (entityToSet is UIElementEntity)
                {
                    if (entityParent is not UIEntity)
                    {
                        LogSystem.LogError("[VEMLHandler->ApplyEntityHierarchy] UI Element Entity not parented by UI Entity.");
                        yield break;
                    }
                    ((UIElementEntity) entityToSet).SetParent((UIEntity) entityParent);

                    Vector2 posPercent = new Vector2((float) ((canvastransform) entityParentRelationship.Key.transform).positionpercent.x,
                        (float) ((canvastransform) entityParentRelationship.Key.transform).positionpercent.y);
                    Vector2 sizePercent = new Vector2((float) ((canvastransform) entityParentRelationship.Key.transform).sizepercent.x,
                        (float) ((canvastransform) entityParentRelationship.Key.transform).sizepercent.y);
                    ((UIElementEntity) entityToSet).SetPositionPercent(posPercent, false);
                    ((UIElementEntity) entityToSet).SetSizePercent(sizePercent, false);
                }
                else
                {
                    entityToSet.SetParent(entityParent);
                }
            }
        }

        /// <summary>
        /// Get a color from a hex string.
        /// </summary>
        /// <param name="hex">The VEML-compliant hex color string.</param>
        /// <returns>A color matching the hex string.</returns>
        private Color FromHex(string hex)
        {
            hex = hex.Replace("#", "");
            if (hex.Length == 6)
            {
                // RGB.
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                return new Color((float) r / 255, (float) g / 255, (float) b / 255, 1);
            }
            else if (hex.Length == 8)
            {
                // RGBA.
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                int a = Convert.ToInt32(hex.Substring(6, 2), 16);
                return new Color((float) r / 255, (float) g / 255, (float) b / 255, (float) a / 255);
            }
            else
            {
                Logging.LogWarning("[VEMLHandler->FromHex] Invalid hex color.");
                return new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }

        /// <summary>
        /// Get a color from a color value string (formatted "color(r g b a)").
        /// </summary>
        /// <param name="colorValueString">Color value string (formatted "color(r g b a)").</param>
        /// <returns>A color matching the color value string.</returns>
        private Color FromColorValue(string colorValueString)
        {
            string elementsString = colorValueString.ToLower().Replace("color(", "").Replace(")", "");
            string[] elements = elementsString.Split(" ");
            if (elements.Length != 3 && elements.Length != 4)
            {
                Logging.LogWarning("[VEMLHandler->FromColorValue] Invalid color value.");
                return new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }

            int r = Convert.ToInt32(elements[0]);
            int g = Convert.ToInt32(elements[1]);
            int b = Convert.ToInt32(elements[2]);
            int a = 1;
            if (elements.Length == 4)
            {
                a = Convert.ToInt32(elements[3]);
            }

            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Process a color from a string.
        /// </summary>
        /// <param name="color">Color string.</param>
        /// <returns>A color matching the string.</returns>
        private Color ProcessColor(string color)
        {
            switch (color.ToLower())
            {
                case "white":
                    return Color.white;

                case "black":
                    return Color.black;

                case "grey":
                case "gray":
                    return Color.gray;

                case "red":
                    return Color.red;

                case "yellow":
                    return Color.yellow;

                case "blue":
                    return Color.blue;

                case "cyan":
                    return Color.cyan;

                case "magenta":
                    return Color.magenta;

                case "green":
                    return Color.green;

                default:
                    if (color.ToLower().StartsWith("color("))
                    {
                        return FromColorValue(color);
                    }
                    else
                    {
                        // Assume color code.
                        return FromHex(color);
                    }
            }
        }
        
        /// <summary>
        /// Process a color into a JavaScript API compatible color from a string.
        /// </summary>
        /// <param name="color">Color string.</param>
        /// <returns>A JavaScript API color matching the string.</returns>
        private Javascript.APIs.WorldTypes.Color ProcessColorIntoJSAPIColor(string color)
        {
            Color processedColor = ProcessColor(color);
            return new Javascript.APIs.WorldTypes.Color(processedColor.r,
                processedColor.g, processedColor.b, processedColor.a);
        }

        /// <summary>
        /// Sets up entity IDs for entities.
        /// </summary>
        /// <param name="entities">A reference to an array of entities to set up the IDs of. Will
        /// assign an ID to each entity for which one does not already exist.</param>
        private void SetUpEntityIDs(ref entity[] entities)
        {
            Queue<entity> entityQueue = new Queue<entity>();
            foreach (entity entity in entities)
            {
                entityQueue.Enqueue(entity);
            }

            while (entityQueue.Count > 0)
            {
                entity entity = entityQueue.Dequeue();
                if (entity.entity1 != null)
                {
                    foreach (entity child in entity.entity1)
                    {
                        entityQueue.Enqueue(child);
                    }
                }

                if (entity.id == null)
                {
                    entity.id = Guid.NewGuid().ToString();
                }
            }
        }

        /// <summary>
        /// Get all child entity relationships for the provided entities.
        /// </summary>
        /// <param name="entities">List of entities for which to get the child entities of.</param>
        /// <returns>A dictionary of the provided entities (and any children) and their parents.</returns>
        private Dictionary<entity, entity> GetAllChildEntities(entity[] entities)
        {
            Dictionary<entity, entity> foundEntities = new Dictionary<entity, entity>();
            foreach (entity entity in entities)
            {
                Dictionary<entity, entity> newEntities = GetAllChildEntities(entity);
                foreach (KeyValuePair<entity, entity> newEntity in newEntities)
                {
                    foundEntities.Add(newEntity.Key, newEntity.Value);
                }
            }
            return foundEntities;
        }

        /// <summary>
        /// Get all child entity relationships for the provided entity.
        /// </summary>
        /// <param name="entity">Entity for which to get the child entities of.</param>
        /// <returns>A dictionary of the provided entity (and any children) and their parents.</returns>
        private Dictionary<entity, entity> GetAllChildEntities(entity entity)
        {
            Dictionary<entity, entity> entities = new Dictionary<entity, entity>();

            Queue<KeyValuePair<entity, entity>> entityQueue = new Queue<KeyValuePair<entity, entity>>();
            entityQueue.Enqueue(new KeyValuePair<entity, entity>(entity, null));
            while (entityQueue.Count > 0)
            {
                KeyValuePair<entity, entity> item = entityQueue.Dequeue();
                if (item.Key.entity1 != null)
                {
                    foreach (entity child in item.Key.entity1)
                    {
                        entityQueue.Enqueue(new KeyValuePair<entity, entity>(child, item.Key));
                    }
                }
                entities.Add(item.Key, item.Value);
            }

            return entities;
        }

        /// <summary>
        /// Apply a transform to an entity.
        /// </summary>
        /// <param name="entity">Entity to apply transform to.</param>
        /// <param name="tf">Transform to apply.</param>
        /// <param name="scaleTransformValid">Whether or not a scale transform will be treated as valid.</param>
        /// <param name="sizeTransformValid">Whether or not a size transform will be treated as valid.</param>
        /// <param name="canvasTransformValid">Whether or not a canvas transform will be treated as valid.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        private bool ApplyTransform(BaseEntity entity, basetransform tf,
            bool scaleTransformValid, bool sizeTransformValid, bool canvasTransformValid)
        {
            if (tf is scaletransform)
            {
                if (scaleTransformValid == false)
                {
                    LogSystem.LogWarning("[VEMLHandler->ApplyTransform] Scale Transform not allowed.");
                    return false;
                }

                entity.SetPosition(new Vector3((float) ((scaletransform) tf).position.x,
                    (float) ((scaletransform) tf).position.y, (float) ((scaletransform) tf).position.z), true, false);
                entity.SetRotation(new Quaternion((float) ((scaletransform) tf).rotation.x, (float) ((scaletransform) tf).rotation.y,
                    (float) ((scaletransform) tf).rotation.z, (float) ((scaletransform) tf).rotation.w), true, false);
                entity.SetScale(new Vector3((float) ((scaletransform) tf).scale.x,
                    (float) ((scaletransform) tf).scale.y, (float) ((scaletransform) tf).scale.z), false);
            }
            else if (tf is sizetransform)
            {
                if (sizeTransformValid == false)
                {
                    LogSystem.LogWarning("[VEMLHandler->ApplyTransform] Size Transform not allowed.");
                    return false;
                }

                entity.SetPosition(new Vector3((float) ((sizetransform) tf).position.x,
                    (float) ((sizetransform) tf).position.y, (float) ((sizetransform) tf).position.z), true, false);
                entity.SetRotation(new Quaternion((float) ((sizetransform) tf).rotation.x, (float) ((sizetransform) tf).rotation.y,
                    (float) ((sizetransform) tf).rotation.z, (float) ((sizetransform) tf).rotation.w), true, false);
                entity.SetSize(new Vector3((float) ((sizetransform) tf).size.x,
                    (float) ((sizetransform) tf).size.y, (float) ((sizetransform) tf).size.z), false);
            }
            else if (tf is canvastransform)
            {
                if (canvasTransformValid == false)
                {
                    LogSystem.LogWarning("[VEMLHandler->ApplyTransform] Canvas Transform not allowed.");
                    return false;
                }

                if (entity is not UIElementEntity)
                {
                    LogSystem.LogError("[VEMLHandler->ApplyTransform] Canvas Transform must be applied to UI Element Entity.");
                    return false;
                }

                ((UIElementEntity) entity).SetPositionPercent(new Vector2((float) ((canvastransform) tf).positionpercent.x,
                    (float) ((canvastransform) tf).positionpercent.y), false);
                ((UIElementEntity) entity).SetSizePercent(new Vector3((float) ((canvastransform) tf).sizepercent.x,
                    (float) ((canvastransform) tf).sizepercent.y), false);
                if (!string.IsNullOrEmpty(((canvastransform) tf).alignhorizontal))
                {
                    if (((canvastransform) tf).alignhorizontal == "left")
                    {
                        ((UIElementEntity) entity).SetAlignment(UIElementAlignment.Left);
                    }
                    else if (((canvastransform) tf).alignhorizontal == "center")
                    {
                        ((UIElementEntity) entity).SetAlignment(UIElementAlignment.Center);
                    }
                    else if (((canvastransform) tf).alignhorizontal == "right")
                    {
                        ((UIElementEntity) entity).SetAlignment(UIElementAlignment.Right);
                    }
                    else
                    {
                        LogSystem.LogWarning("[VEMLHandler->ApplyTransform] Unknown horizontal alignment.");
                    }
                }
                if (!string.IsNullOrEmpty(((canvastransform) tf).alignvertical))
                {
                    if (((canvastransform) tf).alignvertical == "top")
                    {
                        ((UIElementEntity) entity).SetAlignment(UIElementAlignment.Top);
                    }
                    else if (((canvastransform) tf).alignvertical == "middle")
                    {
                        ((UIElementEntity) entity).SetAlignment(UIElementAlignment.Middle);
                    }
                    else if (((canvastransform) tf).alignvertical == "bottom")
                    {
                        ((UIElementEntity) entity).SetAlignment(UIElementAlignment.Bottom);
                    }
                    else
                    {
                        LogSystem.LogWarning("[VEMLHandler->ApplyTransform] Unknown vertical alignment.");
                    }
                }
                if (((canvastransform) tf).stretchtoparentSpecified)
                {
                    if (((canvastransform) tf).stretchtoparent)
                    {
                        ((UIElementEntity) entity).StretchToParent(true, false);
                    }
                    else
                    {
                        ((UIElementEntity) entity).StretchToParent(false, false);
                    }
                }
            }
            else
            {
                LogSystem.LogWarning("[VEMLHandler->ApplyTransform] Unknown transform type.");
                return false;
            }

            return true;
        }
    }
}