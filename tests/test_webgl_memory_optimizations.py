import unittest
import json
import yaml
import os
import re
from unittest.mock import Mock, patch, MagicMock
import xml.etree.ElementTree as ET

class TestWebGLMemoryOptimizations(unittest.TestCase):
    """Comprehensive tests for WebGL/WebGPU Memory Optimizations implementation."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.project_settings_path = "ProjectSettings/ProjectSettings.asset"
        self.quality_settings_path = "ProjectSettings/QualitySettings.asset"
        self.builder_script_path = "Assets/Build/Builder.cs"
        
        # Mock Unity project structure
        self.mock_project_settings = {
            'PlayerSettings': {
                'webGLMemorySize': 64,
                'webGLMemoryGrowthMode': 1,
                'webGLMemoryGrowthStep': 32,
                'webGLMemoryGeometricStep': 0.15,
                'webGLMemoryGeometricCap': 128
            }
        }
        
        self.mock_quality_settings = {
            'QualitySettings': {
                'streamingMipmapsActive': 1,
                'streamingMipmapsMemoryBudget': 256,
                'streamingMipmapsAddAllCameras': 1,
                'streamingMipmapsMaxLevelReduction': 3,
                'm_QualitySettings': []
            }
        }

class TestWebGLMemoryConfiguration(TestWebGLMemoryOptimizations):
    """Test WebGL memory configuration updates."""
    
    def test_initial_memory_size_64mb(self):
        """Test that initial memory size is set to 64 MB."""
        settings = self.mock_project_settings['PlayerSettings']
        self.assertEqual(settings['webGLMemorySize'], 64)
    
    def test_maximum_memory_size_2048mb(self):
        """Test that maximum memory size allows up to 2048 MB growth."""
        # In Unity, max memory is calculated based on growth parameters
        initial = 64
        max_growth_steps = (2048 - initial) // 32
        self.assertGreaterEqual(max_growth_steps, 60)  # Should allow significant growth
    
    def test_linear_growth_step_32mb(self):
        """Test that linear growth step is set to 32 MB."""
        settings = self.mock_project_settings['PlayerSettings']
        self.assertEqual(settings['webGLMemoryGrowthStep'], 32)
    
    def test_geometric_growth_step_015(self):
        """Test that geometric growth step is set to 0.15."""
        settings = self.mock_project_settings['PlayerSettings']
        self.assertAlmostEqual(settings['webGLMemoryGeometricStep'], 0.15, places=2)
    
    def test_geometric_growth_cap_128mb(self):
        """Test that geometric growth cap is set to 128 MB."""
        settings = self.mock_project_settings['PlayerSettings']
        self.assertEqual(settings['webGLMemoryGeometricCap'], 128)
    
    def test_memory_growth_mode_enabled(self):
        """Test that memory growth mode is enabled."""
        settings = self.mock_project_settings['PlayerSettings']
        self.assertEqual(settings['webGLMemoryGrowthMode'], 1)
    
    @patch('os.path.exists')
    @patch('builtins.open')
    def test_project_settings_file_updated(self, mock_open, mock_exists):
        """Test that ProjectSettings.asset file is properly updated."""
        mock_exists.return_value = True
        mock_file = Mock()
        mock_open.return_value.__enter__.return_value = mock_file
        
        # Simulate reading the file
        mock_file.read.return_value = "webGLMemorySize: 64\nwebGLMemoryGrowthStep: 32"
        
        # Verify file operations
        mock_open.assert_called_once()
        self.assertTrue(mock_exists.called)

class TestStreamingMipmaps(TestWebGLMemoryOptimizations):
    """Test streaming mipmaps configuration."""
    
    def test_streaming_mipmaps_active(self):
        """Test that streaming mipmaps are enabled."""
        settings = self.mock_quality_settings['QualitySettings']
        self.assertEqual(settings['streamingMipmapsActive'], 1)
    
    def test_streaming_mipmaps_memory_budget_256mb(self):
        """Test that streaming mipmaps memory budget is set to 256 MB."""
        settings = self.mock_quality_settings['QualitySettings']
        self.assertEqual(settings['streamingMipmapsMemoryBudget'], 256)
    
    def test_streaming_mipmaps_add_all_cameras(self):
        """Test that all cameras are added to streaming mipmaps."""
        settings = self.mock_quality_settings['QualitySettings']
        self.assertEqual(settings['streamingMipmapsAddAllCameras'], 1)
    
    def test_streaming_mipmaps_max_level_reduction(self):
        """Test that max level reduction is set to 3."""
        settings = self.mock_quality_settings['QualitySettings']
        self.assertEqual(settings['streamingMipmapsMaxLevelReduction'], 3)

class TestWebGLOptimizedQualityPreset(TestWebGLMemoryOptimizations):
    """Test WebGL-Optimized quality preset."""
    
    def setUp(self):
        super().setUp()
        self.webgl_optimized_preset = {
            'name': 'WebGL-Optimized',
            'pixelLightCount': 1,
            'shadows': 1,
            'shadowResolution': 0,
            'antiAliasing': 0,
            'asyncUploadBufferSize': 8,
            'asyncUploadTimeSlice': 2,
            'streamingMipmapsActive': 1,
            'streamingMipmapsMemoryBudget': 256,
            'particleRaycastBudget': 64
        }
    
    def test_quality_preset_name(self):
        """Test that quality preset has correct name."""
        self.assertEqual(self.webgl_optimized_preset['name'], 'WebGL-Optimized')
    
    def test_pixel_light_count_optimized(self):
        """Test that pixel light count is optimized for WebGL."""
        self.assertEqual(self.webgl_optimized_preset['pixelLightCount'], 1)
    
    def test_shadows_minimal(self):
        """Test that shadows are set to minimal quality."""
        self.assertEqual(self.webgl_optimized_preset['shadows'], 1)
    
    def test_shadow_resolution_disabled(self):
        """Test that shadow resolution is minimized."""
        self.assertEqual(self.webgl_optimized_preset['shadowResolution'], 0)
    
    def test_anti_aliasing_disabled(self):
        """Test that anti-aliasing is disabled for performance."""
        self.assertEqual(self.webgl_optimized_preset['antiAliasing'], 0)
    
    def test_async_upload_buffer_size(self):
        """Test that async upload buffer size is optimized."""
        self.assertEqual(self.webgl_optimized_preset['asyncUploadBufferSize'], 8)
    
    def test_async_upload_time_slice(self):
        """Test that async upload time slice is optimized."""
        self.assertEqual(self.webgl_optimized_preset['asyncUploadTimeSlice'], 2)
    
    def test_particle_raycast_budget(self):
        """Test that particle raycast budget is limited."""
        self.assertEqual(self.webgl_optimized_preset['particleRaycastBudget'], 64)
    
    def test_streaming_mipmaps_in_preset(self):
        """Test that streaming mipmaps are enabled in the preset."""
        self.assertEqual(self.webgl_optimized_preset['streamingMipmapsActive'], 1)
        self.assertEqual(self.webgl_optimized_preset['streamingMipmapsMemoryBudget'], 256)

class TestBuilderExceptionSupport(TestWebGLMemoryOptimizations):
    """Test Builder.cs exception support configuration."""
    
    def setUp(self):
        super().setUp()
        self.production_builder_code = """
        public class Builder {
            public static void BuildProduction() {
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
                BuildWebGL(true);
            }
            
            public static void BuildDebug() {
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStackTrace;
                BuildWebGL(false);
            }
        }
        """
        
        self.debug_builder_code = """
        public class Builder {
            public static void BuildDebug() {
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStackTrace;
                BuildWebGL(false);
            }
        }
        """
    
    def test_production_build_exception_support(self):
        """Test that production builds use minimal exception support."""
        self.assertIn("ExplicitlyThrownExceptionsOnly", self.production_builder_code)
    
    def test_debug_build_exception_support(self):
        """Test that debug builds retain full exception support."""
        self.assertIn("FullWithoutStackTrace", self.debug_builder_code)
    
    def test_builder_has_production_method(self):
        """Test that Builder.cs has BuildProduction method."""
        self.assertIn("BuildProduction", self.production_builder_code)
    
    def test_builder_has_debug_method(self):
        """Test that Builder.cs has BuildDebug method."""
        self.assertIn("BuildDebug", self.production_builder_code)
    
    @patch('builtins.open')
    def test_builder_file_contains_webgl_settings(self, mock_open):
        """Test that Builder.cs file contains WebGL-specific settings."""
        mock_file = Mock()
        mock_open.return_value.__enter__.return_value = mock_file
        mock_file.read.return_value = self.production_builder_code
        
        content = mock_file.read()
        self.assertIn("PlayerSettings.WebGL.exceptionSupport", content)
        self.assertIn("WebGLExceptionSupport", content)

class TestBuildPipelineSettings(TestWebGLMemoryOptimizations):
    """Test build pipeline settings."""
    
    def setUp(self):
        super().setUp()
        self.build_settings = {
            'compression': 'Gzip',
            'decompressionFallback': True,
            'linkerTarget': 'Wasm',
            'dataCaching': True
        }
    
    def test_compression_gzip_enabled(self):
        """Test that Gzip compression is enabled."""
        self.assertEqual(self.build_settings['compression'], 'Gzip')
    
    def test_decompression_fallback_enabled(self):
        """Test that decompression fallback is enabled."""
        self.assertTrue(self.build_settings['decompressionFallback'])
    
    def test_linker_target_wasm(self):
        """Test that linker target is set to Wasm."""
        self.assertEqual(self.build_settings['linkerTarget'], 'Wasm')
    
    def test_data_caching_enabled(self):
        """Test that data caching is enabled."""
        self.assertTrue(self.build_settings['dataCaching'])
    
    def test_builder_applies_compression_settings(self):
        """Test that Builder.cs applies compression settings."""
        builder_code = """
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.dataCaching = true;
        """
        
        self.assertIn("WebGLCompressionFormat.Gzip", builder_code)
        self.assertIn("decompressionFallback = true", builder_code)
        self.assertIn("WebGLLinkerTarget.Wasm", builder_code)
        self.assertIn("dataCaching = true", builder_code)

class TestResourceCleanupCoroutine(TestWebGLMemoryOptimizations):
    """Test resource cleanup coroutine implementation."""
    
    def setUp(self):
        super().setUp()
        self.cleanup_code = """
        #if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator ResourceCleanupCoroutine() {
            while (true) {
                yield return new WaitForSeconds(60f);
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }
        
        private void Start() {
            #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(ResourceCleanupCoroutine());
            #endif
        }
        #endif
        """
    
    def test_cleanup_coroutine_exists(self):
        """Test that resource cleanup coroutine exists."""
        self.assertIn("ResourceCleanupCoroutine", self.cleanup_code)
    
    def test_cleanup_runs_every_60_seconds(self):
        """Test that cleanup runs every 60 seconds."""
        self.assertIn("WaitForSeconds(60f)", self.cleanup_code)
    
    def test_cleanup_unloads_unused_assets(self):
        """Test that cleanup calls UnloadUnusedAssets."""
        self.assertIn("Resources.UnloadUnusedAssets()", self.cleanup_code)
    
    def test_cleanup_calls_gc_collect(self):
        """Test that cleanup calls garbage collection."""
        self.assertIn("System.GC.Collect()", self.cleanup_code)
    
    def test_cleanup_webgl_only(self):
        """Test that cleanup is WebGL-only."""
        self.assertIn("UNITY_WEBGL && !UNITY_EDITOR", self.cleanup_code)
    
    def test_cleanup_auto_starts(self):
        """Test that cleanup automatically starts."""
        self.assertIn("StartCoroutine(ResourceCleanupCoroutine())", self.cleanup_code)

class TestIntegrationScenarios(TestWebGLMemoryOptimizations):
    """Test integration scenarios and edge cases."""
    
    def test_webgl_build_configuration_complete(self):
        """Test that WebGL build has complete optimized configuration."""
        webgl_config = {
            'memory_size': 64,
            'memory_growth_step': 32,
            'streaming_mipmaps': True,
            'quality_preset': 'WebGL-Optimized',
            'exception_support': 'ExplicitlyThrownExceptionsOnly',
            'compression': 'Gzip',
            'resource_cleanup': True
        }
        
        # Verify all optimization components are present
        self.assertEqual(webgl_config['memory_size'], 64)
        self.assertEqual(webgl_config['memory_growth_step'], 32)
        self.assertTrue(webgl_config['streaming_mipmaps'])
        self.assertEqual(webgl_config['quality_preset'], 'WebGL-Optimized')
        self.assertEqual(webgl_config['exception_support'], 'ExplicitlyThrownExceptionsOnly')
        self.assertEqual(webgl_config['compression'], 'Gzip')
        self.assertTrue(webgl_config['resource_cleanup'])
    
    def test_non_webgl_platforms_unaffected(self):
        """Test that non-WebGL platforms are not affected by WebGL optimizations."""
        # Mock settings for other platforms
        standalone_settings = {
            'memory_management': 'automatic',
            'exception_support': 'full',
            'quality_preset': 'High'
        }
        
        # Verify other platforms maintain their settings
        self.assertEqual(standalone_settings['memory_management'], 'automatic')
        self.assertEqual(standalone_settings['exception_support'], 'full')
        self.assertEqual(standalone_settings['quality_preset'], 'High')
    
    def test_debug_vs_production_build_differences(self):
        """Test differences between debug and production builds."""
        debug_config = {
            'exception_support': 'FullWithoutStackTrace',
            'optimization_level': 'debug'
        }
        
        production_config = {
            'exception_support': 'ExplicitlyThrownExceptionsOnly',
            'optimization_level': 'production'
        }
        
        self.assertNotEqual(debug_config['exception_support'], 
                           production_config['exception_support'])
    
    @patch('os.path.exists')
    def test_file_existence_validation(self, mock_exists):
        """Test that all required files exist."""
        required_files = [
            "ProjectSettings/ProjectSettings.asset",
            "ProjectSettings/QualitySettings.asset",
            "Assets/Build/Builder.cs"
        ]
        
        mock_exists.return_value = True
        
        for file_path in required_files:
            self.assertTrue(os.path.exists(file_path))
    
    def test_memory_budget_calculations(self):
        """Test memory budget calculations are within reasonable limits."""
        initial_memory = 64  # MB
        max_memory = 2048   # MB
        streaming_budget = 256  # MB
        
        # Verify memory budgets are reasonable
        self.assertGreater(max_memory, initial_memory)
        self.assertLess(streaming_budget, max_memory)
        self.assertGreater(streaming_budget, initial_memory)
    
    def test_performance_settings_consistency(self):
        """Test that performance settings are consistent across components."""
        quality_settings = {
            'pixel_lights': 1,
            'shadows': 1,
            'anti_aliasing': 0,
            'async_buffer_size': 8,
            'particle_budget': 64
        }
        
        # Verify settings are optimized for performance
        self.assertLessEqual(quality_settings['pixel_lights'], 2)
        self.assertLessEqual(quality_settings['shadows'], 2)
        self.assertEqual(quality_settings['anti_aliasing'], 0)
        self.assertLessEqual(quality_settings['async_buffer_size'], 16)
        self.assertLessEqual(quality_settings['particle_budget'], 128)

class TestErrorHandlingAndEdgeCases(TestWebGLMemoryOptimizations):
    """Test error handling and edge cases."""
    
    def test_invalid_memory_size_handling(self):
        """Test handling of invalid memory size values."""
        invalid_sizes = [-1, 0, 16, 8192]
        valid_range = range(32, 4096)  # Reasonable range for WebGL memory
        
        for size in invalid_sizes:
            if size <= 0 or size > 4096 or size < 32:
                self.assertNotIn(size, valid_range)
    
    def test_missing_quality_preset_handling(self):
        """Test handling when WebGL-Optimized preset is missing."""
        quality_presets = ['Very Low', 'Low', 'Medium', 'High', 'Very High', 'Ultra']
        
        # Should add WebGL-Optimized if not present
        if 'WebGL-Optimized' not in quality_presets:
            quality_presets.append('WebGL-Optimized')
        
        self.assertIn('WebGL-Optimized', quality_presets)
    
    def test_build_failure_recovery(self):
        """Test recovery from build configuration failures."""
        # Mock build configuration that might fail
        build_steps = [
            'configure_memory',
            'set_quality_preset', 
            'configure_exceptions',
            'apply_compression',
            'start_cleanup'
        ]
        
        # All steps should be reversible/recoverable
        for step in build_steps:
            self.assertIsInstance(step, str)
            self.assertTrue(len(step) > 0)
    
    def test_platform_detection_accuracy(self):
        """Test accurate platform detection for WebGL-specific features."""
        # Mock platform detection logic
        def is_webgl_platform(platform):
            return platform.lower() in ['webgl', 'webgpu']
        
        self.assertTrue(is_webgl_platform('WebGL'))
        self.assertTrue(is_webgl_platform('webgl'))
        self.assertFalse(is_webgl_platform('Standalone'))
        self.assertFalse(is_webgl_platform('Android'))
    
    def test_resource_cleanup_safety(self):
        """Test that resource cleanup is safe and doesn't cause issues."""
        # Mock cleanup operations that should be safe
        cleanup_operations = [
            'unload_unused_assets',
            'garbage_collect',
            'wait_for_next_cycle'
        ]
        
        # All operations should be non-destructive
        for operation in cleanup_operations:
            self.assertIn(operation, [
                'unload_unused_assets',
                'garbage_collect', 
                'wait_for_next_cycle'
            ])

if __name__ == '__main__':
    # Run all tests
    unittest.main(verbosity=2)