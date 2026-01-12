using System.Collections;
using UnityEngine;

namespace WebVerseRuntime
{
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager instance;
        private Coroutine cleanupCoroutine;
        
        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("ResourceManager");
                    instance = go.AddComponent<ResourceManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            StartPeriodicCleanup();
            Debug.Log("[ResourceManager] WebGL resource management initialized");
#else
            Debug.Log("[ResourceManager] Initialized (cleanup disabled for non-WebGL builds)");
#endif
        }
        
        private void OnDestroy()
        {
            if (cleanupCoroutine != null)
            {
                StopCoroutine(cleanupCoroutine);
            }
        }
        
        public void StartPeriodicCleanup()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (cleanupCoroutine == null)
            {
                cleanupCoroutine = StartCoroutine(PeriodicResourceCleanup());
                Debug.Log("[ResourceManager] Periodic cleanup started (60 second intervals)");
            }
#endif
        }
        
        public void StopPeriodicCleanup()
        {
            if (cleanupCoroutine != null)
            {
                StopCoroutine(cleanupCoroutine);
                cleanupCoroutine = null;
                Debug.Log("[ResourceManager] Periodic cleanup stopped");
            }
        }
        
        private IEnumerator PeriodicResourceCleanup()
        {
            while (true)
            {
                yield return new WaitForSeconds(60f); // Run every 60 seconds
                
                yield return PerformCleanup();
            }
        }
        
        public void ForceCleanup()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(ForceCleanupCoroutine());
#else
            Debug.Log("[ResourceManager] Force cleanup called but disabled for non-WebGL builds");
#endif
        }
        
        private IEnumerator ForceCleanupCoroutine()
        {
            Debug.Log("[ResourceManager] Force cleanup initiated");
            yield return PerformCleanup();
        }
        
        private IEnumerator PerformCleanup()
        {
            // Log memory usage before cleanup (WebGL only)
#if UNITY_WEBGL && !UNITY_EDITOR
            LogMemoryUsage("Before cleanup");
#endif
            
            // Unload unused assets
            AsyncOperation unloadOperation = Resources.UnloadUnusedAssets();
            yield return unloadOperation;
            
            // Force garbage collection
            System.GC.Collect();
            
            // Wait a frame for GC to complete
            yield return null;
            
#if UNITY_WEBGL && !UNITY_EDITOR
            LogMemoryUsage("After cleanup");
            Debug.Log("[ResourceManager] Cleanup cycle completed");
#endif
        }
        
        private void LogMemoryUsage(string phase)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            long memoryUsage = System.GC.GetTotalMemory(false);
            Debug.Log($"[ResourceManager] {phase} - Memory usage: {memoryUsage / 1048576}MB");
#endif
        }
        
        // Public API for manual resource management
        public static void RequestCleanup()
        {
            if (Instance != null)
            {
                Instance.ForceCleanup();
            }
        }
        
        // Initialize ResourceManager automatically for WebGL builds
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeForWebGL()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Ensure ResourceManager is created early in WebGL builds
            var manager = Instance;
            Debug.Log("[ResourceManager] Auto-initialized for WebGL build");
#endif
        }
    }
}