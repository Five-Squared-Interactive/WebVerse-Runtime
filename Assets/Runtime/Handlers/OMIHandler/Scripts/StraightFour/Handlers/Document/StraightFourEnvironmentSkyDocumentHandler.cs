// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.EnvironmentSky;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Document-level handler for OMI_environment_sky.
    /// Stores sky definitions and applies the first sky using StraightFour's EnvironmentManager.
    /// </summary>
    public class StraightFourEnvironmentSkyDocumentHandler : StraightFourDocumentHandlerBase<OMIEnvironmentSkyDocumentData>
    {
        public override string ExtensionName => "OMI_environment_sky";
        public override int Priority => 95;

        public override Task OnDocumentImportAsync(OMIEnvironmentSkyDocumentData data, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data?.Skies == null || data.Skies.Count == 0)
            {
                Logging.Log("[StraightFour] OMI_environment_sky: No skies in document data");
                return Task.CompletedTask;
            }

            // Store sky definitions for scene handler to reference
            context.CustomData[StraightFourCustomDataKeys.Skies] = data.Skies;

            Logging.Log($"[StraightFour] OMI_environment_sky: Stored {data.Skies.Count} sky definitions from document.");

            // Log sky details if verbose
            if (context.Settings.VerboseLogging)
            {
                for (int i = 0; i < data.Skies.Count; i++)
                {
                    var sky = data.Skies[i];
                    Logging.Log($"[StraightFour]   Sky {i}: type={sky.Type ?? "unknown"}");
                }
            }

            // Apply the first sky by default (scene-level extension would override this if present)
            var firstSky = data.Skies[0];
            ApplySky(firstSky, context);

            return Task.CompletedTask;
        }

        private void ApplySky(OMIEnvironmentSkySkyData skyData, OMIImportContext context)
        {
            if (skyData == null) return;

            Logging.Log($"[StraightFour] Applying sky of type: {skyData.Type}");

            // Get the EnvironmentManager from StraightFour
            var environmentManager = FiveSQD.StraightFour.StraightFour.ActiveWorld?.environmentManager;
            if (environmentManager == null)
            {
                Logging.LogWarning("[StraightFour] EnvironmentManager not available, cannot apply sky");
                return;
            }

            // Apply sky based on type
            var skyType = skyData.GetSkyType();
            switch (skyType)
            {
                case OMIEnvironmentSkyType.Plain:
                    ApplyPlainSky(skyData, environmentManager);
                    break;

                case OMIEnvironmentSkyType.Gradient:
                    ApplyGradientSky(skyData, environmentManager);
                    break;

                case OMIEnvironmentSkyType.Panorama:
                    ApplyPanoramaSky(skyData, environmentManager, context);
                    break;

                case OMIEnvironmentSkyType.Physical:
                    ApplyPhysicalSky(skyData, environmentManager);
                    break;

                default:
                    Logging.LogWarning($"[StraightFour] Unknown sky type: {skyData.Type}");
                    break;
            }
        }

        private void ApplyPlainSky(OMIEnvironmentSkySkyData skyData, FiveSQD.StraightFour.Environment.EnvironmentManager environmentManager)
        {
            if (skyData.Plain == null) return;

            var (r, g, b) = skyData.Plain.GetColor();
            var color = new Color(r, g, b);

            Logging.Log($"[StraightFour] Applying plain sky color: RGB({r:F2}, {g:F2}, {b:F2})");

            environmentManager.SetSolidColorSky(color);

            Logging.Log($"[StraightFour] Applied plain sky via EnvironmentManager");
        }

        private void ApplyGradientSky(OMIEnvironmentSkySkyData skyData, FiveSQD.StraightFour.Environment.EnvironmentManager environmentManager)
        {
            if (skyData.Gradient == null) return;

            var (tr, tg, tb) = skyData.Gradient.GetTopColor();
            var (br, bg, bb) = skyData.Gradient.GetBottomColor();
            
            var topColor = new Color(tr, tg, tb);
            var bottomColor = new Color(br, bg, bb);
            var horizonColor = Color.Lerp(bottomColor, topColor, 0.5f);

            Logging.Log($"[StraightFour] Applying gradient sky: top=RGB({tr:F2}, {tg:F2}, {tb:F2}), bottom=RGB({br:F2}, {bg:F2}, {bb:F2})");

            environmentManager.SetGradientSky(bottomColor, horizonColor, topColor);
            Logging.Log($"[StraightFour] Applied gradient sky via EnvironmentManager");
        }

        private void ApplyPanoramaSky(OMIEnvironmentSkySkyData skyData, FiveSQD.StraightFour.Environment.EnvironmentManager environmentManager, OMIImportContext context)
        {
            if (skyData.Panorama == null) return;

            var panoramaData = skyData.Panorama;

            // Handle cubemap (6 face textures)
            if (panoramaData.HasCubemap)
            {
                var cubemapIndices = panoramaData.Cubemap;
                Logging.Log($"[StraightFour] Panorama sky with cubemap texture indices: {string.Join(", ", cubemapIndices)}");

                // Get the 6 textures from glTFast
                // Cubemap order in OMI: +X, -X, +Y, -Y, +Z, -Z
                var textures = new Texture2D[6];
                bool allTexturesLoaded = true;

                for (int i = 0; i < 6; i++)
                {
                    int textureIndex = cubemapIndices[i];
                    var texture = context.GltfImport?.GetTexture(textureIndex);
                    if (texture == null)
                    {
                        Logging.LogWarning($"[StraightFour] Could not get texture at index {textureIndex} for cubemap face {i}");
                        allTexturesLoaded = false;
                        break;
                    }
                    textures[i] = texture;
                }

                if (allTexturesLoaded)
                {
                    // posX, negX, posY, negY, posZ, negZ
                    environmentManager.SetCubemapSky(textures[0], textures[1], textures[2], textures[3], textures[4], textures[5]);
                    Logging.Log($"[StraightFour] Applied cubemap panorama sky via EnvironmentManager");
                }
                else
                {
                    Logging.LogWarning("[StraightFour] Failed to load all cubemap face textures");
                }
            }
            // Handle equirectangular panorama
            else if (panoramaData.HasEquirectangular)
            {
                int textureIndex = panoramaData.Equirectangular.Value;
                Logging.Log($"[StraightFour] Panorama sky with equirectangular texture index: {textureIndex}");

                var texture = context.GltfImport?.GetTexture(textureIndex);
                if (texture != null)
                {
                    environmentManager.SetPanoramaSky(texture);
                    Logging.Log($"[StraightFour] Applied equirectangular panorama sky via EnvironmentManager");
                }
                else
                {
                    Logging.LogWarning($"[StraightFour] Could not get texture at index {textureIndex} for equirectangular panorama");
                }
            }
            else
            {
                Logging.LogWarning("[StraightFour] Panorama sky has neither cubemap nor equirectangular texture");
            }
        }

        private void ApplyPhysicalSky(OMIEnvironmentSkySkyData skyData, FiveSQD.StraightFour.Environment.EnvironmentManager environmentManager)
        {
            if (skyData.Physical == null) return;

            var physical = skyData.Physical;
            
            // Get physical sky parameters
            var (gr, gg, gb) = physical.GetGroundColor();
            var (rr, rg, rb) = physical.GetRayleighColor();
            
            var groundColor = new Color(gr, gg, gb);
            var rayleighColor = new Color(rr, rg, rb);
            var horizonColor = Color.Lerp(groundColor, rayleighColor, 0.4f);

            Logging.Log($"[StraightFour] Physical sky: ground=RGB({gr:F2}, {gg:F2}, {gb:F2}), rayleigh=RGB({rr:F2}, {rg:F2}, {rb:F2})");

            // Use gradient sky with physical parameters
            environmentManager.SetGradientSky(groundColor, horizonColor, rayleighColor, 0f, 0.3f, 1.2f);
            Logging.Log($"[StraightFour] Applied physical sky via EnvironmentManager as gradient");
        }
    }
}
#endif
