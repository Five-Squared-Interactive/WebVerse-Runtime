// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.Rendering;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Input.Quest3
{
    /// <summary>
    /// Manages environment settings (skybox, lighting) when switching between VR and AR modes.
    /// Ensures VR worlds remain visible in AR by suppressing skybox and applying fallback lighting.
    /// </summary>
    public class AREnvironmentManager
    {
        private AmbientMode _originalAmbientMode;
        private Color _originalAmbientColor;
        private Material _originalSkybox;
        private bool _originalSettingsSaved;

        private static readonly Color AR_FALLBACK_AMBIENT = new Color(0.5f, 0.5f, 0.5f, 1f);

        /// <summary>
        /// Apply AR environment settings: suppress skybox, apply fallback ambient lighting.
        /// Call when switching to AR mode.
        /// </summary>
        public void ApplyAREnvironment()
        {
            SaveOriginalSettings();

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AR_FALLBACK_AMBIENT;

            Logging.Log("[AREnvironmentManager] AR environment applied: skybox suppressed, fallback ambient set.");
        }

        /// <summary>
        /// Restore VR environment settings: skybox and original ambient lighting.
        /// Call when switching back to VR mode.
        /// </summary>
        public void RestoreVREnvironment()
        {
            if (!_originalSettingsSaved) return;

            RenderSettings.skybox = _originalSkybox;
            RenderSettings.ambientMode = _originalAmbientMode;
            RenderSettings.ambientLight = _originalAmbientColor;

            Logging.Log("[AREnvironmentManager] VR environment restored.");
        }

        private void SaveOriginalSettings()
        {
            if (_originalSettingsSaved) return;

            _originalAmbientMode = RenderSettings.ambientMode;
            _originalAmbientColor = RenderSettings.ambientLight;
            _originalSkybox = RenderSettings.skybox;
            _originalSettingsSaved = true;
        }
    }
}