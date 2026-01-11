using System.Collections;
using UnityEngine;

namespace WebVerseRuntime
{
    public class WebVerseRuntime : MonoBehaviour
    {
        [Header("WebGL Optimization Settings")]
        [SerializeField] private bool enableResourceManagement = true;
        [SerializeField] private bool logMemoryUsage = false;
        
        private ResourceManager resourceManager;
        
        private void Awake()
        {
            // Initialize WebGL-specific optimizations
#if UNITY_WEBGL && !UNITY_EDITOR
            InitializeWebGLOptimizations();
#endif
        }
        
        private void Start()
        {
            Debug.Log("WebVerse Runtime initialized");
            
            // Initialize resource management for WebGL
#if UNITY_WEBGL && !UNITY_EDITOR
            if (enableResourceManagement)
            {
                SetupResourceManagement();
            }
            
            if (logMemoryUsage)
            {
                StartCoroutine(LogMemoryUsagePeriodically());
            }
#endif
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        private void InitializeWebGLOptimizations()
        {
            // Apply WebGL-specific quality settings at runtime
            ApplyWebGLQualitySettings();
            
            Debug.Log("[WebVerseRuntime] WebGL optimizations initialized");
        }
        
        private void SetupResourceManagement()
        {
            // Get or create ResourceManager instance
            resourceManager = ResourceManager.Instance;
            
            // Parent it to this runtime for organization
            if (resourceManager.transform.parent == null)
            {
                resourceManager.transform.SetParent(this.transform);
            }
            
            Debug.Log("[WebVerseRuntime] Resource management setup completed");
        }
        
        private void ApplyWebGLQualitySettings()
        {
            // Find and apply WebGL-Optimized quality settings if available
            string[] qualityNames = QualitySettings.names;
            for (int i = 0; i < qualityNames.Length; i++)
            {
                if (qualityNames[i] == "WebGL-Optimized")
                {
                    QualitySettings.SetQualityLevel(i, false);
                    Debug.Log($"[WebVerseRuntime] Applied WebGL-Optimized quality settings");
                    return;
                }
            }
            
            // Fallback: Apply WebGL optimizations directly
            ApplyDirectWebGLOptimizations();
        }
        
        private void ApplyDirectWebGLOptimizations()
        {
            // Apply streaming mipmaps settings
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = 268435456; // 256 MB
            QualitySettings.streamingMipmapsAddAllCameras = true;
            QualitySettings.streamingMipmapsMaxLevelReduction = 3;
            
            // Apply performance optimizations
            QualitySettings.pixelLightCount = 1;
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.antiAliasing = 0;
            QualitySettings.asyncUploadBufferSize = 8;
            QualitySettings.asyncUploadTimeSlice = 2;
            QualitySettings.particleRaycastBudget = 64;
            
            Debug.Log("[WebVerseRuntime] Applied direct WebGL optimizations");
        }
        
        private IEnumerator LogMemoryUsagePeriodically()
        {
            while (true)
            {
                yield return new WaitForSeconds(30f); // Log every 30 seconds
                
                long memoryUsage = System.GC.GetTotalMemory(false);
                Debug.Log($"[WebVerseRuntime] Memory usage: {memoryUsage / 1048576}MB");
            }
        }
#endif
        
        // Public API for external access
        public void RequestResourceCleanup()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ResourceManager.RequestCleanup();
#endif
        }
        
        public void ToggleResourceManagement(bool enabled)
        {
            enableResourceManagement = enabled;
            
#if UNITY_WEBGL && !UNITY_EDITOR
            if (resourceManager != null)
            {
                if (enabled)
                {
                    resourceManager.StartPeriodicCleanup();
                }
                else
                {
                    resourceManager.StopPeriodicCleanup();
                }
            }
#endif
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            // Perform cleanup when application loses focus (WebGL tab switching)
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!hasFocus && enableResourceManagement)
            {
                RequestResourceCleanup();
            }
#endif
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Perform cleanup when application is paused
#if UNITY_WEBGL && !UNITY_EDITOR
            if (pauseStatus && enableResourceManagement)
            {
                RequestResourceCleanup();
            }
#endif
        }
    }
}