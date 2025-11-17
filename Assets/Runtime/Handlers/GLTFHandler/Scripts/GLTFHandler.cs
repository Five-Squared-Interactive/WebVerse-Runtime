// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.File;
using System;
using FiveSQD.WebVerse.Runtime;
#if USE_WEBINTERFACE
using FiveSQD.WebVerse.WebInterface.HTTP;
#endif
using FiveSQD.StraightFour.Entity;
using System.Collections;
using UnityEditor;

namespace FiveSQD.WebVerse.Handlers.GLTF
{
    /// <summary>
    /// WebVerse's GLTF Handler.
    /// </summary>
    public class GLTFHandler : BaseHandler
    {
        /// <summary>
        /// The GLTF loader.
        /// </summary>
        public GLTFLoader gltfLoader;

        /// <summary>
        /// The WebVerse runtime.
        /// </summary>
        public WebVerseRuntime runtime;

        /// <summary>
        /// Container to use for mesh prefabs.
        /// </summary>
        public GameObject meshPrefabContainer;

        /// <summary>
        /// Position to apply to mesh prefabs.
        /// </summary>
        private static readonly Vector3 prefabLocation = new Vector3(9999, 9999, 9999);

        /// <summary>
        /// Dictionary of mesh paths and their corresponding prefabs.
        /// </summary>
        private Dictionary<string, GameObject> gltfMeshPrefabs = new Dictionary<string, GameObject>();

        public override void Initialize()
        {
            gltfLoader = gameObject.AddComponent<GLTFLoader>();
            base.Initialize();
        }

        /// <summary>
        /// Load a GLTF resource as a mesh entity.
        /// </summary>
        /// <param name="gltfResourceURI">URI of the top-level GLTF resource.</param>
        /// <param name="resourceURIs">URIs of resources needed by the top-level GLTF.</param>
        /// <param name="id">ID of the mesh entity.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Provides reference
        /// to the loaded mesh entity.</param>
        /// <param name="timeout">Timeout period after which loading will be aborted.</param>
        /// <param name="checkForUpdateIfCached">Whether or not to check for update if in cache.</param>
        /// <returns>ID of the mesh entity being loaded.</returns>
        public Guid LoadGLTFResourceAsMeshEntity(string gltfResourceURI, string[] resourceURIs,
            Guid? id = null, Action<MeshEntity> onLoaded = null, float timeout = 10, bool checkForUpdateIfCached = true)
        {
#if !UNITY_WEBGL
            Dictionary<string, bool> downloadedState = new Dictionary<string, bool>();

            if (resourceURIs != null)
            {
                foreach (string resourceURI in resourceURIs)
                {
                    if (!string.IsNullOrEmpty(resourceURI))
                    {
                        downloadedState.Add(resourceURI, false);
                        Action onResouceDownloaded = () =>
                        {
                            downloadedState[resourceURI] = true;
                        };
                        
                        DownloadGLTFResource(VEML.VEMLUtilities.FullyQualifyURI(resourceURI,
                            WebVerseRuntime.Instance.currentBasePath), onResouceDownloaded, !checkForUpdateIfCached);
                    }
                }
            }
            Guid guid = id.HasValue ? id.Value : Guid.NewGuid();
            Action onDownloaded = () =>
            {
                LoadGLTF(
                    System.IO.Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(VEML.VEMLUtilities.FullyQualifyURI(
                        gltfResourceURI, WebVerseRuntime.Instance.currentBasePath))),
                    new Action<GameObject>((meshObject) =>
                    {
                        SetUpLoadedGLTFMeshAsMeshEntity(meshObject, guid, onLoaded);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFResource(gltfResourceURI, onDownloaded, !checkForUpdateIfCached);
            return guid;
#else
            Guid guid = id.HasValue ? id.Value : Guid.NewGuid();
            Action<byte[]> onDownloaded = (data) =>
            {
                LoadGLTF(gltfResourceURI.Substring(gltfResourceURI.LastIndexOf("/")), data,
                    new Action<GameObject>((meshObject) =>
                    {
                        SetUpLoadedGLTFMeshAsMeshEntity(meshObject, guid, onLoaded);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFDirect(gltfResourceURI, onDownloaded);
            return guid;
#endif
        }

        /// <summary>
        /// Load a GLTF resource as a character entity.
        /// </summary>
        /// <param name="gltfResourceURI">URI of the top-level GLTF resource.</param>
        /// <param name="resourceURIs">URIs of resources needed by the top-level GLTF.</param>
        /// <param name="meshOffset">Offset for the mesh character entity object.</param>
        /// <param name="meshRotation">Rotation for the mesh character entity object.</param>
        /// <param name="avatarLabelOffset">Offset for the avatar label.</param>
        /// <param name="id">ID of the character entity.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Provides reference
        /// to the loaded character entity.</param>
        /// <param name="timeout">Timeout period after which loading will be aborted.</param>
        /// <param name="checkForUpdateIfCached">Whether or not to check for update if in cache.</param>
        /// <returns>ID of the character entity being loaded.</returns>
        public Guid LoadGLTFResourceAsCharacterEntity(string gltfResourceURI, string[] resourceURIs,
            Vector3 meshOffset, Quaternion meshRotation, Vector3 avatarLabelOffset, Guid? id = null,
            Action<CharacterEntity> onLoaded = null, float timeout = 10, bool checkForUpdateIfCached = true)
        {
#if !UNITY_WEBGL
            Dictionary<string, bool> downloadedState = new Dictionary<string, bool>();

            if (resourceURIs != null)
            {
                foreach (string resourceURI in resourceURIs)
                {
                    if (!string.IsNullOrEmpty(resourceURI))
                    {
                        downloadedState.Add(resourceURI, false);
                        Action onResouceDownloaded = () =>
                        {
                            downloadedState[resourceURI] = true;
                        };

                        DownloadGLTFResource(VEML.VEMLUtilities.FullyQualifyURI(resourceURI,
                            WebVerseRuntime.Instance.currentBasePath), onResouceDownloaded,
                            !checkForUpdateIfCached);
                    }
                }
            }
            Guid guid = id.HasValue ? id.Value : Guid.NewGuid();
            Action onDownloaded = () =>
            {
                LoadGLTF(
                    System.IO.Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(VEML.VEMLUtilities.FullyQualifyURI(
                        gltfResourceURI, WebVerseRuntime.Instance.currentBasePath))),
                    new Action<GameObject>((meshObject) =>
                    {
                        SetUpLoadedGLTFMeshAsCharacterEntity(meshObject, meshOffset, meshRotation, avatarLabelOffset, guid, onLoaded);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFResource(gltfResourceURI, onDownloaded, !checkForUpdateIfCached);
            return guid;
#else
            Guid guid = id.HasValue ? id.Value : Guid.NewGuid();
            Action<byte[]> onDownloaded = (data) =>
            {
                LoadGLTF(gltfResourceURI.Substring(gltfResourceURI.LastIndexOf("/")), data,
                    new Action<GameObject>((meshObject) =>
                    {
                        SetUpLoadedGLTFMeshAsCharacterEntity(meshObject, meshOffset, meshRotation, avatarLabelOffset, guid, onLoaded);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFDirect(gltfResourceURI, onDownloaded);
            return guid;
#endif
        }

        /// <summary>
        /// Load a GLTF resource as an automobile entity.
        /// </summary>
        /// <param name="gltfResourceURI">URI of the top-level GLTF resource.</param>
        /// <param name="resourceURIs">URIs of resources needed by the top-level GLTF.</param>
        /// <param name="meshPosition">Position for the automobile entity object.</param>
        /// <param name="meshRotation">Rotation for the automobile entity object.</param>
        /// <param name="wheels">Wheels for the automobile entity.</param>
        /// <param name="mass">Mass of the automobile entity.</param>
        /// <param name="type">Type of automobile entity.</param>
        /// <param name="id">ID of the automobile entity.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Provides reference
        /// to the loaded automobile entity.</param>
        /// <param name="timeout">Timeout period after which loading will be aborted.</param>
        /// <param name="checkForUpdateIfCached">Whether or not to check for update if in cache.</param>
        /// <returns>ID of the automobile entity being loaded.</returns>
        public Guid LoadGLTFResourceAsAutomobileEntity(string gltfResourceURI, string[] resourceURIs,
            Vector3 meshPosition, Quaternion meshRotation, Javascript.APIs.Entity.AutomobileEntityWheel[] wheels,
            float mass, Javascript.APIs.Entity.AutomobileEntity.AutomobileType type, Guid? id = null,
            Action<AutomobileEntity> onLoaded = null, float timeout = 10, bool checkForUpdateIfCached = true)
        {
            Dictionary<string, float> convertedWheels = new Dictionary<string, float>();

            EntityManager.AutomobileEntityType convertedType = EntityManager.AutomobileEntityType.Default;
            switch (type)
            {
                case Javascript.APIs.Entity.AutomobileEntity.AutomobileType.Default:
                default:
                    convertedType = EntityManager.AutomobileEntityType.Default;
                    break;
            }

#if !UNITY_WEBGL
            Dictionary<string, bool> downloadedState = new Dictionary<string, bool>();

            if (resourceURIs != null)
            {
                foreach (string resourceURI in resourceURIs)
                {
                    if (!string.IsNullOrEmpty(resourceURI))
                    {
                        downloadedState.Add(resourceURI, false);
                        Action onResouceDownloaded = () =>
                        {
                            downloadedState[resourceURI] = true;
                        };

                        DownloadGLTFResource(VEML.VEMLUtilities.FullyQualifyURI(resourceURI,
                            WebVerseRuntime.Instance.currentBasePath), onResouceDownloaded,
                            !checkForUpdateIfCached);
                    }
                }
            }
            Guid guid = id.HasValue ? id.Value : Guid.NewGuid();
            Action onDownloaded = () =>
            {
                LoadGLTF(
                    System.IO.Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(VEML.VEMLUtilities.FullyQualifyURI(
                        gltfResourceURI, WebVerseRuntime.Instance.currentBasePath))),
                    new Action<GameObject>((meshObject) =>
                    {
                        foreach (Javascript.APIs.Entity.AutomobileEntityWheel wheel in wheels)
                        {
                            convertedWheels.Add(wheel.wheelSubMesh, wheel.wheelRadius);
                        }

                        SetUpLoadedGLTFMeshAsAutomobileEntity(meshObject, meshPosition, meshRotation,
                            convertedWheels, mass, convertedType, guid, tag, onLoaded);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFResource(gltfResourceURI, onDownloaded, !checkForUpdateIfCached);
            return guid;
#else
            Guid guid = id.HasValue ? id.Value : Guid.NewGuid();
            Action<byte[]> onDownloaded = (data) =>
            {
                LoadGLTF(gltfResourceURI.Substring(gltfResourceURI.LastIndexOf("/")), data,
                    new Action<GameObject>((meshObject) =>
                    {
                        foreach (Javascript.APIs.Entity.AutomobileEntityWheel wheel in wheels)
                        {
                            /*GameObject wheelSubMesh = FindChildObjectByName(meshObject, wheel.wheelSubMesh);
                            if (wheelSubMesh == null)
                            {
                                Logging.LogWarning("[GLTFHandler->LoadGLTFResourceAsAutomobileEntity] Unable to find wheel submesh "
                                    + wheel.wheelSubMesh);
                            }
                            else*/
                            {
                                convertedWheels.Add(wheel.wheelSubMesh, wheel.wheelRadius);
                            }
                        }

                        SetUpLoadedGLTFMeshAsAutomobileEntity(meshObject, meshPosition, meshRotation,
                            convertedWheels, mass, convertedType, guid, tag, onLoaded);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFDirect(gltfResourceURI, onDownloaded);
            return guid;
#endif
        }

        /// <summary>
        /// Load a GLTF resource as an airplane entity.
        /// </summary>
        /// <param name="gltfResourceURI">URI of the top-level GLTF resource.</param>
        /// <param name="resourceURIs">URIs of resources needed by the top-level GLTF.</param>
        /// <param name="meshPosition">Position for the airplane entity object.</param>
        /// <param name="meshRotation">Rotation for the airplane entity object.</param>
        /// <param name="wheels">Wheels for the airplane entity.</param>
        /// <param name="mass">Mass of the airplane entity.</param>
        /// <param name="type">Type of airplane entity.</param>
        /// <param name="id">ID of the airplane entity.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Provides reference
        /// to the loaded airplane entity.</param>
        /// <param name="timeout">Timeout period after which loading will be aborted.</param>
        /// <param name="checkForUpdateIfCached">Whether or not to check for update if in cache.</param>
        /// <returns>ID of the airplane entity being loaded.</returns>
        public Guid LoadGLTFResourceAsAirplaneEntity(string gltfResourceURI, string[] resourceURIs,
            Vector3 meshPosition, Quaternion meshRotation, float mass, Guid? id = null,
            Action<AirplaneEntity> onLoaded = null, float timeout = 10, bool checkForUpdateIfCached = true)
        {
#if !UNITY_WEBGL
            Dictionary<string, bool> downloadedState = new Dictionary<string, bool>();

            if (resourceURIs != null)
            {
                foreach (string resourceURI in resourceURIs)
                {
                    if (!string.IsNullOrEmpty(resourceURI))
                    {
                        downloadedState.Add(resourceURI, false);
                        Action onResouceDownloaded = () =>
                        {
                            downloadedState[resourceURI] = true;
                        };

                        DownloadGLTFResource(VEML.VEMLUtilities.FullyQualifyURI(resourceURI,
                            WebVerseRuntime.Instance.currentBasePath), onResouceDownloaded,
                            !checkForUpdateIfCached);
                    }
                }
            }
            Guid guid = id.HasValue ? id.Value : Guid.NewGuid();
            Action onDownloaded = () =>
            {
                LoadGLTF(
                    System.IO.Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(VEML.VEMLUtilities.FullyQualifyURI(
                        gltfResourceURI, WebVerseRuntime.Instance.currentBasePath))),
                    new Action<GameObject>((meshObject) =>
                    {
                        SetUpLoadedGLTFMeshAsAirplaneEntity(meshObject, meshPosition, meshRotation,
                            mass, guid, tag, onLoaded);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFResource(gltfResourceURI, onDownloaded, !checkForUpdateIfCached);
            return guid;
#else
            Guid guid = id.HasValue ? id.Value : Guid.NewGuid();
            Action<byte[]> onDownloaded = (data) =>
            {
                LoadGLTF(gltfResourceURI.Substring(gltfResourceURI.LastIndexOf("/")), data,
                    new Action<GameObject>((meshObject) =>
                    {
                        SetUpLoadedGLTFMeshAsAirplaneEntity(meshObject, meshPosition, meshRotation,
                            mass, guid, tag, onLoaded);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFDirect(gltfResourceURI, onDownloaded);
            return guid;
#endif
        }

        /// <summary>
        /// Apply a GLTF resource to a character entity.
        /// </summary>
        /// <param name="entity">Entity to apply the GLTF resource to.
        /// <param name="gltfResourceURI">URI of the top-level GLTF resource.</param>
        /// <param name="resourceURIs">URIs of resources needed by the top-level GLTF.</param>
        /// <param name="meshOffset">Offset for the mesh character entity object.</param>
        /// <param name="meshRotation">Rotation for the mesh character entity object.</param>
        /// <param name="avatarLabelOffset">Offset for the avatar label.</param>
        /// <param name="timeout">Timeout period after which loading will be aborted.</param>
        /// <param name="checkForUpdateIfCached">Whether or not to check for update if in cache.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool ApplyGLTFResourceToCharacterEntity(CharacterEntity entity, string gltfResourceURI,
            string[] resourceURIs, Vector3 meshOffset, Quaternion meshRotation, Vector3 avatarLabelOffset,
            float timeout = 10, bool checkForUpdateIfCached = true)
        {
#if !UNITY_WEBGL
            Dictionary<string, bool> downloadedState = new Dictionary<string, bool>();

            if (resourceURIs != null)
            {
                foreach (string resourceURI in resourceURIs)
                {
                    if (!string.IsNullOrEmpty(resourceURI))
                    {
                        downloadedState.Add(resourceURI, false);
                        Action onResouceDownloaded = () =>
                        {
                            downloadedState[resourceURI] = true;
                        };

                        DownloadGLTFResource(VEML.VEMLUtilities.FullyQualifyURI(resourceURI,
                            WebVerseRuntime.Instance.currentBasePath), onResouceDownloaded,
                            !checkForUpdateIfCached);
                    }
                }
            }
            Action onDownloaded = () =>
            {
                LoadGLTF(
                    System.IO.Path.Combine(runtime.fileHandler.fileDirectory,
                    FileHandler.ToFileURI(VEML.VEMLUtilities.FullyQualifyURI(
                        gltfResourceURI, WebVerseRuntime.Instance.currentBasePath))),
                    new Action<GameObject>((meshObject) =>
                    {
                        entity.SetCharacterGO(meshObject);
                        entity.SetCharacterObjectOffset(meshOffset);
                        entity.SetCharacterObjectRotation(meshRotation);
                        entity.SetCharacterLabelOffset(avatarLabelOffset);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFResource(gltfResourceURI, onDownloaded, !checkForUpdateIfCached);
            return true;
#else
            Action<byte[]> onDownloaded = (data) =>
            {
                LoadGLTF(gltfResourceURI.Substring(gltfResourceURI.LastIndexOf("/")), data,
                    new Action<GameObject>((meshObject) =>
                    {
                        entity.SetCharacterGO(meshObject);
                        entity.SetCharacterObjectOffset(meshOffset);
                        entity.SetCharacterObjectRotation(meshRotation);
                        entity.SetCharacterLabelOffset(avatarLabelOffset);
                        CleanUpExtraPrefabs();
                    }));
            };

            DownloadGLTFDirect(gltfResourceURI, onDownloaded);
            return true;
#endif
        }

        /// <summary>
        /// Download a GLTF resource.
        /// </summary>
        /// <param name="uri">URI of the GLTF resource.</param>
        /// <param name="onDownloaded">Action to invoke when downloading is complete.</param>
        /// <param name="bypassUpdateCheck">Whether or not to bypass check for update.</param>
        public void DownloadGLTFResource(string uri, Action onDownloaded, bool bypassUpdateCheck)
        {
            uri = VEML.VEMLUtilities.FullyQualifyURI(uri, WebVerseRuntime.Instance.currentBasePath);
#if USE_WEBINTERFACE
            Action<int, Dictionary<string, string>, byte[]> onDownloadedAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
            {
                FinishGLTFDownload(uri, code, data);
                onDownloaded.Invoke();
            });

            HTTPRequest request = new HTTPRequest(uri, HTTPRequest.HTTPMethod.Get, onDownloadedAction);

            if (runtime.fileHandler.FileExistsInFileDirectory(FileHandler.ToFileURI(uri)))
            {
                if (uri.StartsWith("file:/") || uri.StartsWith("/") || uri.StartsWith(".") || uri[1] == ':')
                {
                    Logging.LogDebug("[GLTFHandler->DownloadGLTF] File " + uri + " already exists. Using stored version.");
                    onDownloaded.Invoke();
                    return;
                }

                if (bypassUpdateCheck)
                {
                    onDownloaded.Invoke();
                }
                else
                {
                    Logging.LogDebug("[GLTFHandler->DownloadGLTF] File " + uri + " already exists. Checking for newer version.");

                    Action<int, Dictionary<string, string>, byte[]> onResponseAction = new Action<int, Dictionary<string, string>, byte[]>(
                        (code, headers, data) =>
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
                                        Logging.LogDebug("[GLTFHandler->DownloadGLTF] Cached version of file " + uri +
                                            " is outdated. Getting new version.");
                                    }
                                    else
                                    {
                                        Logging.LogDebug("[GLTFHandler->DownloadGLTF] Cached version of file " + uri +
                                            " is current. Using stored version.");
                                        onDownloaded.Invoke();
                                        return;
                                    }
                                }
                            }
                        }
                        Logging.LogDebug("[GLTFHandler->DownloadGLTF] Getting " + uri + ".");
                        request.Send();
                    });
                    HTTPRequest headRequest = new HTTPRequest(uri, HTTPRequest.HTTPMethod.Head, onResponseAction);
                    headRequest.Send();
                }
            }
            else
            {
                request.Send();
            }
#endif
        }

        /// <summary>
        /// Download a GLTF model and load directly.
        /// </summary>
        /// <param name="uri">URI of the GLTF resource.</param>
        /// <param name="onDownloaded">Action to invoke when downloading is complete.</param>
        /// <param name="reDownload">Whether or not to redownload the resource if it already
        /// exists locally.</param>
        public void DownloadGLTFDirect(string uri, Action<byte[]> onDownloaded)
        {
            uri = VEML.VEMLUtilities.FullyQualifyURI(uri, WebVerseRuntime.Instance.currentBasePath);
#if USE_WEBINTERFACE
            Action<int, Dictionary<string, string>, byte[]> onDownloadedAction = new Action<int, Dictionary<string, string>, byte[]>((code, headers, data) =>
            {
                onDownloaded.Invoke(data);
            });
            HTTPRequest request = new HTTPRequest(uri, HTTPRequest.HTTPMethod.Get, onDownloadedAction);
            request.Send();
#endif
        }

        /// <summary>
        /// Load a local GLTF resource.
        /// </summary>
        /// <param name="path">path to the GLTF resource (relative to the file directory).</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Provides reference
        /// to the loaded gameobject.</param>
        public void LoadGLTF(string path, Action<GameObject> onLoaded)
        {
            if (gltfMeshPrefabs.ContainsKey(path))
            {
                InstantiateMeshFromPrefab(gltfMeshPrefabs[path], onLoaded);
            }
            else
            {
                if (!runtime.fileHandler.FileExistsInFileDirectory(path) && !System.IO.File.Exists(path))
                {
                    Logging.LogWarning("[GLTFHandler->LoadGLTF] File not found: " + path);
                    return;
                }
                
                Action<GameObject, AnimationClip[]> callback =
                    (GameObject go, AnimationClip[] ac) => { MeshLoadedFromGLTF(path, go, ac, onLoaded); };
                gltfLoader.LoadModelAsync(path, callback);
            }
        }

        /// <summary>
        /// Load a GLB resource directly.
        /// </summary>
        /// <param name="modelName">GLB model name.</param>
        /// <param name="modelData">GLB resource data.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Provides reference
        /// to the loaded gameobject.</param>
        public void LoadGLTF(string modelName, byte[] modelData, Action<GameObject> onLoaded)
        {
            if (gltfMeshPrefabs.ContainsKey(modelName))
            {
                InstantiateMeshFromPrefab(gltfMeshPrefabs[modelName], onLoaded);
            }
            else
            {
                Action<GameObject, AnimationClip[]> callback =
                    (GameObject go, AnimationClip[] ac) => { MeshLoadedFromGLTF(modelName, go, ac, onLoaded); };
                gltfLoader.LoadModelAsync(modelData, callback);
            }
        }

        /// <summary>
        /// Invoked when a mesh is loaded from a GLTF.
        /// </summary>
        /// <param name="path">Path to the GLTF resource.</param>
        /// <param name="result">Resultant gameobject or null.</param>
        /// <param name="clips">Animation clips associated with the GLTF.</param>
        /// <param name="callback">Callback to invoke. Takes a reference to the loaded gameobject.</param>
        private void MeshLoadedFromGLTF(string path, GameObject result, AnimationClip[] clips, Action<GameObject> callback)
        {
            if (gltfMeshPrefabs.ContainsKey(path))
            {
                gltfMeshPrefabs[path] = result;
            }
            else
            {
                gltfMeshPrefabs.Add(path, result);
            }
            result.transform.position = prefabLocation;
            SetUpMeshPrefab(result);
            callback.Invoke(result);
            //InstantiateMeshFromPrefab(result, callback);
        }

        /// <summary>
        /// Invoked to finish the downloading of a GLTF resource post-receiving an HTTP response.
        /// </summary>
        /// <param name="uri">URI of the GLTF resource.</param>
        /// <param name="responseCode">Received response code.</param>
        /// <param name="rawData">Data received in the response.</param>
        private void FinishGLTFDownload(string uri, int responseCode, byte[] rawData)
        {
            uri = VEML.VEMLUtilities.FullyQualifyURI(uri, WebVerseRuntime.Instance.currentBasePath);
            Logging.Log("[GLTFHandler->FinishGLTFDownload] Got response " + responseCode + " for request " + uri);

            if (responseCode != 200)
            {
                Logging.Log("[GLTFHandler->FinishGLTFDownload] Error loading file.");
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
        /// Instantiate a mesh from an existing prefab.
        /// </summary>
        /// <param name="prefab">Prefab to load instance of.</param>
        /// <param name="callback">Callback to invoke when instantiation is complete.
        /// Takes a reference to the loaded gameobject.</param>
        /// <param name="timeout">Timeout period after which loading will be aborted.</param>
        private void InstantiateMeshFromPrefab(GameObject prefab, Action<GameObject> callback)
        {
            GameObject loadedMesh = Instantiate(prefab);
            loadedMesh.SetActive(true);
            callback.Invoke(loadedMesh);
        }

        /// <summary>
        /// Set up a gameobject as a mesh prefab.
        /// </summary>
        /// <param name="prefab">GameObject to set up as a mesh prefab.</param>
        private void SetUpMeshPrefab(GameObject prefab)
        {
            prefab.SetActive(false);
            prefab.transform.SetParent(meshPrefabContainer.transform);
            MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
            List<Mesh> meshes = new List<Mesh>();
            if (meshFilters.Length > 0)
            {
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    meshes.Add(meshFilter.mesh);

                    GameObject mcGO = new GameObject("MeshCollider");
                    mcGO.transform.SetParent(meshFilter.transform);
                    mcGO.transform.localPosition = Vector3.zero;
                    mcGO.transform.localRotation = Quaternion.identity;
                    mcGO.transform.localScale = Vector3.one;
                    MeshCollider meshCollider = mcGO.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = meshFilter.mesh;
                    meshCollider.tag = StraightFour.Tags.TagManager.meshColliderTag;
                }
            }
            else
            {
                //Logging.LogWarning("[GLTFHandler->SetUpMeshPrefab] Unable to set up mesh.");
                //return;
            }

            Bounds bounds = new Bounds(prefab.transform.position, Vector3.zero);
            MeshRenderer[] rends = prefab.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rend in rends)
            {
                bounds.Encapsulate(rend.bounds);
            }

            bounds.center = bounds.center - prefab.transform.position;

            GameObject bcGO = new GameObject("BoxCollider");
            bcGO.transform.SetParent(prefab.transform);
            bcGO.transform.localPosition = Vector3.zero;
            BoxCollider boxCollider = bcGO.AddComponent<BoxCollider>();
            boxCollider.tag = StraightFour.Tags.TagManager.physicsColliderTag;
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;

            Rigidbody rigidbody = prefab.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        }

        /// <summary>
        /// Set up a loaded GLTF as a mesh entity.
        /// </summary>
        /// <param name="loadedMesh">Loaded gameobject containing mesh.</param>
        /// <param name="guid">ID of the mesh entity.</param>
        /// <param name="onLoaded">Action to invoke when setup is complete. Takes a reference
        /// to the set up mesh entity.</param>
        private void SetUpLoadedGLTFMeshAsMeshEntity(GameObject loadedMesh, Guid guid, Action<MeshEntity> onLoaded)
        {
            Action onLoad = () =>
            {
                BaseEntity entity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                if (entity == null)
                {
                    Logging.LogError("[GLTFHandler->SetUpLoadedGLTFMeshAsMeshEntity] Unable to find loaded entity.");
                    return;
                }

                if (onLoaded != null)
                {
                    onLoaded.Invoke((MeshEntity) entity);
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadMeshEntity(
                null, loadedMesh, Vector3.zero, Quaternion.identity, guid, null, onLoad);
        }

        /// <summary>
        /// Set up a loaded GLTF as a character entity.
        /// </summary>
        /// <param name="loadedMesh">Loaded gameobject containing mesh.</param>
        /// <param name="meshOffset">Offset for the mesh character entity object.</param>
        /// <param name="meshRotation">Rotation for the mesh character entity object.</param>
        /// <param name="avatarLabelOffset">Offset for the avatar label.</param>
        /// <param name="guid">ID of the mesh character entity.</param>
        /// <param name="onLoaded">Action to invoke when setup is complete. Takes a reference
        /// to the set up character entity.</param>
        private void SetUpLoadedGLTFMeshAsCharacterEntity(GameObject loadedMesh, Vector3 meshOffset,
            Quaternion meshRotation, Vector3 avatarLabelOffset, Guid guid, Action<CharacterEntity> onLoaded)
        {
            Action onLoad = () =>
            {
                BaseEntity entity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                if (entity == null)
                {
                    Logging.LogError("[GLTFHandler->SetUpLoadedGLTFMeshAsCharacterEntity] Unable to find loaded entity.");
                    return;
                }

                if (onLoaded != null)
                {
                    onLoaded.Invoke((CharacterEntity) entity);
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadCharacterEntity(
                null, loadedMesh, meshOffset, meshRotation, avatarLabelOffset, Vector3.zero, Quaternion.identity,
                Vector3.one, guid, null, false, onLoad);
        }

        /// <summary>
        /// Set up a loaded GLTF as an automobile entity.
        /// </summary>
        /// <param name="loadedMesh">Loaded gameobject containing mesh.</param>
        /// <param name="meshPosition">Position for the automobile entity object.</param>
        /// <param name="meshRotation">Rotation for the automobile entity object.</param>
        /// <param name="wheels">Wheels for the automobile entity.</param>
        /// <param name="mass">Mass of the automobile entity.</param>
        /// <param name="type">Type of automobile entity.</param>
        /// <param name="guid">ID of the automobile entity.</param>
        /// <param name="tag">Tag for the automobile entity.</param>
        /// <param name="onLoaded">Action to invoke when setup is complete. Takes a reference
        /// to the set up automobile entity.</param>
        private void SetUpLoadedGLTFMeshAsAutomobileEntity(GameObject loadedMesh, Vector3 meshPosition,
            Quaternion meshRotation, Dictionary<string, float> wheels, float mass,
            EntityManager.AutomobileEntityType type, Guid guid, string tag, Action<AutomobileEntity> onLoaded)
        {
            Action onLoad = () =>
            {
                BaseEntity entity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                if (entity == null)
                {
                    Logging.LogError("[GLTFHandler->SetUpLoadedGLTFMeshAsAutomobileEntity] Unable to find loaded entity.");
                    return;
                }

                if (onLoaded != null)
                {
                    onLoaded.Invoke((AutomobileEntity) entity);
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadAutomobileEntity(
                null, meshPosition, meshRotation, loadedMesh, wheels, mass, type, guid, tag, onLoad);
        }

        /// <summary>
        /// Set up a loaded GLTF as an airplane entity.
        /// </summary>
        /// <param name="loadedMesh">Loaded gameobject containing mesh.</param>
        /// <param name="meshPosition">Position for the airplane entity object.</param>
        /// <param name="meshRotation">Rotation for the airplane entity object.</param>
        /// <param name="mass">Mass of the airplane entity.</param>
        /// <param name="guid">ID of the airplane entity.</param>
        /// <param name="tag">Tag for the airplane entity.</param>
        /// <param name="onLoaded">Action to invoke when setup is complete. Takes a reference
        /// to the set up airplane entity.</param>
        private void SetUpLoadedGLTFMeshAsAirplaneEntity(GameObject loadedMesh, Vector3 meshPosition,
            Quaternion meshRotation, float mass,
            Guid guid, string tag, Action<AirplaneEntity> onLoaded)
        {
            Action onLoad = () =>
            {
                BaseEntity entity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                if (entity == null)
                {
                    Logging.LogError("[GLTFHandler->SetUpLoadedGLTFMeshAsAirplaneEntity] Unable to find loaded entity.");
                    return;
                }

                if (onLoaded != null)
                {
                    onLoaded.Invoke((AirplaneEntity) entity);
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadAirplaneEntity(
                null, meshPosition, meshRotation, loadedMesh, mass, guid, tag, onLoad);
        }

        /// <summary>
        /// Load a GLTF as a mesh entity in the background using a coroutine.
        /// </summary>
        /// <param name="guid">ID of the mesh entity.</param>
        /// <param name="gltfResourceURI">URI of the top-level GLTF resource.</param>
        /// <param name="resourceDownloadStates">Download states of resources the GLTF
        /// is dependent on. Dictionary of resource URIs and whether or not they have
        /// been downlaoded.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Takes a reference
        /// to the loaded mesh entity.</param>
        /// <param name="timeout">Timeout period after which loading will be aborted.</param>
        private IEnumerator LoadGLTFInBackground(Guid guid, string gltfResourceURI,
            Dictionary<string, bool> resourceDownloadStates, Action<MeshEntity> onLoaded, float timeout = 10)
        {
            bool allLoaded = true;
            float elapsedTime = 0f;
            if (resourceDownloadStates != null)
            {
                do
                {
                    allLoaded = true;
                    foreach (bool resource in resourceDownloadStates.Values)
                    {
                        if (resource == false)
                        {
                            allLoaded = false;
                            yield return new WaitForSeconds(0.25f);
                            elapsedTime += 0.25f;
                            break;
                        }
                    }
                } while (allLoaded == false && elapsedTime < timeout);

                LoadGLTF(
                    System.IO.Path.Combine(runtime.fileHandler.fileDirectory, FileHandler.ToFileURI(gltfResourceURI)),
                    new Action<GameObject>((meshObject) =>
                    {
                        SetUpLoadedGLTFMeshAsMeshEntity(meshObject, guid, onLoaded);
                    }));

                yield return null;
            }
        }

        private void CleanUpExtraPrefabs()
        {
            GameObject extraObject = GameObject.Find("LoadedGLTF(Clone)");
            while (extraObject != null)
            {
                DestroyImmediate(extraObject);
                extraObject = GameObject.Find("LoadedGLTF(Clone)");
            }
        }
    }
}