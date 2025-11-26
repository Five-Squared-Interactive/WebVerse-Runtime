using UnityEngine;
#if GLTFAST_PRESENT
using GLTFast;
#endif

namespace X3D
{
    /// <summary>
    /// Component for loading external 3D models from Inline nodes.
    /// Supports GLTF/GLB when GLTFast is available.
    /// </summary>
    public class X3DInlineModelLoader : MonoBehaviour
    {
        public string[] urls;
        public string metadata;

        /// <summary>
        /// Load the model at runtime from the first available URL.
        /// </summary>
#if GLTFAST_PRESENT
        public async void LoadModel()
        {
            if (urls == null || urls.Length == 0)
            {
                Debug.LogWarning("[X3DInlineModelLoader] No URLs provided for Inline model.");
                return;
            }
            string url = urls[0];
            var gltf = new GltfImport();
            bool success = await gltf.Load(url);
            if (success)
            {
                bool instSuccess = gltf.InstantiateMainScene(transform);
                if (!instSuccess)
                {
                    Debug.LogError($"[X3DInlineModelLoader] Failed to instantiate model from {url}");
                }
            }
            else
            {
                Debug.LogError($"[X3DInlineModelLoader] Failed to load model from {url}");
            }
        }
#else
        public void LoadModel()
        {
            Debug.LogWarning("[X3DInlineModelLoader] GLTFast is not available. Cannot load Inline models.");
        }
#endif
    }
}
