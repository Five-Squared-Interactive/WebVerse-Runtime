// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;
using System.Collections;
using System.Threading.Tasks;

namespace FiveSQD.WebVerse.Handlers.GLTF
{
    /// <summary>
    /// Component to load a GLTF scene with
    /// </summary>
    public class GLTFLoader : MonoBehaviour
    {
        /// <summary>
        /// Load a GLTF model asynchronously.
        /// </summary>
        /// <param name="pathToModel">Path to the GLTF model.</param>
        /// <param name="callback">Action to invoke when loading is complete. Provides reference
        /// to the loaded gameobject and any animation clips.</param>
        public void LoadModelAsync(string pathToModel, Action<GameObject, AnimationClip[]> callback)
        {
#if UNITY_WEBGL
            StartCoroutine(ImportAsync(pathToModel, callback));
#else
            if (pathToModel.EndsWith(".gltf"))
            {
                StartCoroutine(ImportAsync(pathToModel, callback));
            }
            else
            {
                if (!pathToModel.EndsWith(".glb"))
                {
                    Logging.LogError("[GLTFLoader->LoadModelAsync] Unknown model file format. Trying GLB.");
                }

                byte[] modelData = System.IO.File.ReadAllBytes(pathToModel);
                StartCoroutine(LoadModelCoroutine(modelData, callback));
            }
#endif
        }

        /// <summary>
        /// Load a GLB model asynchronously.
        /// </summary>
        /// <param name="modelData">GLB model data.</param>
        /// <param name="callback">Action to invoke when loading is complete. Provides reference
        /// to the loaded gameobject and any animation clips.</param>
        public void LoadModelAsync(byte[] modelData, Action<GameObject, AnimationClip[]> callback)
        {
#if UNITY_WEBGL
            StartCoroutine(LoadModelCoroutine(modelData, callback));
#endif
        }

        /// <summary>
        /// Load a GLTF model.
        /// </summary>
        /// <param name="pathToModel">Path to the GLTF model.</param>
        public static GameObject LoadModel(string pathToModel)
        {
            GameObject go = new GameObject("LoadedGLTF");
            Runtime.WebVerseRuntime.Instance.gltfHandler.gltfLoader.StartCoroutine(
                Runtime.WebVerseRuntime.Instance.gltfHandler.gltfLoader.ImportAsync(go, pathToModel, null));
            return go;
        }

        /// <summary>
        /// Load a GLTF model in a coroutine.
        /// </summary>
        /// <param name="pathToModel">Path to the GLTF model.</param>
        /// <param name="callback">Action to invoke when loading is complete. Takes a reference
        /// to the loaded gameobject and any animation clips.</param>
        private IEnumerator LoadModelCoroutine(string pathToModel, Action<GameObject, AnimationClip[]> callback)
        {
            if (pathToModel.EndsWith(".gltf"))
            {
                StartCoroutine(ImportAsync(pathToModel, callback));
            }
            else
            {
                if (!pathToModel.EndsWith(".glb"))
                {
                    Logging.LogError("[GLTFLoader->LoadModelCoroutine] Unknown model file format. Trying GLB.");
                }

                byte[] modelData = System.IO.File.ReadAllBytes(pathToModel);
                StartCoroutine(LoadModelCoroutine(modelData, callback));
            }

            yield return null;
        }

        /// <summary>
        /// Load a GLB model directly in a coroutine.
        /// </summary>
        /// <param name="data">GLB model data.</param>
        /// <param name="callback">Action to invoke when loading is complete. Takes a reference
        /// to the loaded gameobject and any animation clips.</param>
        private IEnumerator LoadModelCoroutine(byte[] data, Action<GameObject, AnimationClip[]> callback)
        {
            StartCoroutine(ImportBinaryAsync(data, callback));
            yield return null;
        }

        /// <summary>
        /// Import a model asynchronously.
        /// </summary>
        /// <param name="gltfObject">GLTF GameObject.</param>
        /// <param name="assetPath">Path of the GLTF to load.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Takes a reference
        /// to the loaded gameobject and any animation clips.</param>
        public IEnumerator ImportAsync(GameObject gltfObject, string assetPath, Action<GameObject, AnimationClip[]> onLoaded)
        {
            GLTFast.GltfAsset gltfAsset = gltfObject.AddComponent<GLTFast.GltfAsset>();
            Task loadTask = gltfAsset.Load(assetPath);
            yield return new WaitUntil(() => loadTask.IsCompleted);
            if (loadTask.IsCompletedSuccessfully)
            {
                if (onLoaded != null)
                {
                    onLoaded.Invoke(gltfObject, null);
                }
            }
            else
            {
                Logging.LogError("[GLTFLoader->ImportAsync] Error loading GLTF.");
            }
            Destroy(gltfAsset);
        }

        /// <summary>
        /// Import a model asynchronously.
        /// </summary>
        /// <param name="assetPath">Path of the GLTF to load.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Takes a reference
        /// to the loaded gameobject and any animation clips.</param>
        public IEnumerator ImportAsync(string assetPath, Action<GameObject, AnimationClip[]> onLoaded)
        {
            GameObject gltfObject = new GameObject("LoadedGLTF");
            ImportAsync(gltfObject, assetPath, onLoaded);
            yield return null;
        }

        /// <summary>
        /// Import a model from binary asynchronously.
        /// </summary>
        /// <param name="glbObject">GLB GameObject.</param>
        /// <param name="data">Data to load.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Takes a reference
        /// to the loaded gameobject and any animation clips.</param>
        public IEnumerator ImportBinaryAsync(GameObject glbObject, byte[] data, Action<GameObject, AnimationClip[]> onLoaded)
        {
            GLTFast.GltfImport import = new GLTFast.GltfImport();
            Task loadTask = import.LoadGltfBinary(data, null, new GLTFast.ImportSettings()
            {
                GenerateMipMaps = true,
                NodeNameMethod = GLTFast.NameImportMethod.OriginalUnique
            });
            yield return new WaitUntil(() => loadTask.IsCompleted);
            if (loadTask.IsCompletedSuccessfully)
            {
                Task completeTask = import.InstantiateMainSceneAsync(glbObject.transform);
                yield return new WaitUntil(() => completeTask.IsCompleted);
                if (onLoaded != null)
                {
                    onLoaded.Invoke(glbObject, null);
                }
            }
            else
            {
                Logging.LogError("[GLTFLoader->ImportBinaryAsync] Error loading GLTF.");
            }
        }

        /// <summary>
        /// Import a model from binary asynchronously.
        /// </summary>
        /// <param name="data">Data to load.</param>
        /// <param name="onLoaded">Action to invoke when loading is complete. Takes a reference
        /// to the loaded gameobject and any animation clips.</param>
        public IEnumerator ImportBinaryAsync(byte[] data, Action<GameObject, AnimationClip[]> onLoaded)
        {
            GameObject gltfObject = new GameObject("LoadedGLTF");
            StartCoroutine(ImportBinaryAsync(gltfObject, data, onLoaded));
            yield return null;
        }
    }
}