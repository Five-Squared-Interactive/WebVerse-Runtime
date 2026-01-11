// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.StraightFour.Utilities;
using UnityEngine;
using OccaSoftware.SuperSimpleSkybox.Runtime;

namespace FiveSQD.StraightFour.Environment
{
    /// <summary>
    /// Manager for the environment.
    /// </summary>
    public class EnvironmentManager : BaseManager
    {
        /// <summary>
        /// Material to use for the sky.
        /// </summary>
        public Material skyMaterial;

        /// <summary>
        /// Material to use for the lite procedural sky.
        /// </summary>
        [Tooltip("Material to use for the lite procedural sky.")]
        public Material liteProceduralSkyMaterial;

        /// <summary>
        /// GameObject for the lite procedural sky.
        /// </summary>
        [Tooltip("GameObject for the lite procedural sky.")]
        public GameObject liteProceduralSkyObject;

        /// <summary>
        /// Texture to use for default clouds.
        /// </summary>
        [Tooltip("Texture to use for default clouds.")]
        public Texture defaultCloudTexture;

        /// <summary>
        /// Texture to use for default stars.
        /// </summary>
        [Tooltip("Texture to use for default stars.")]
        public Texture defaultStarTexture;

        /// <summary>
        /// Initialize the environment manager.
        /// </summary>
        public override void Initialize()
        {
            ResetSky();
            ApplySkyMaterial();
            DisableFog();
            base.Initialize();
        }

        /// <summary>
        /// Apply the sky material.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool ApplySkyMaterial()
        {
            if (skyMaterial == null)
            {
                LogSystem.LogError("[EnvironmentManager->ApplySkyMaterial] No sky material.");
                return false;
            }

            RenderSettings.skybox = skyMaterial;

            liteProceduralSkyObject?.SetActive(false);

            return true;
        }

        /// <summary>
        /// Apply the lite procedural sky material.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool ApplyLiteProceduralSkyMaterial()
        {
            if (liteProceduralSkyMaterial == null)
            {
                LogSystem.LogError("[EnvironmentManager->ApplyLiteProceduralSkyMaterial] No lite procedural sky material.");
                return false;
            }

            if (liteProceduralSkyObject == null)
            {
                LogSystem.LogError("[EnvironmentManager->ApplyLiteProceduralSkyMaterial] No lite procedural sky object.");
                return false;
            }

            liteProceduralSkyObject.SetActive(true);

            return true;
        }

        /// <summary>
        /// Set the sky to a solid color.
        /// </summary>
        /// <param name="color">Color to set the sky to.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetSolidColorSky(Color color)
        {
            skyMaterial.SetColor("_Tint", color);

            ApplySkyMaterial();

            return true;
        }

        /// <summary>
        /// Set the sky to a gradient with ground, horizon, and zenith colors.
        /// </summary>
        /// <param name="groundColor">Color for the ground.</param>
        /// <param name="horizonColor">Color for the horizon.</param>
        /// <param name="zenithColor">Color for the sky/zenith.</param>
        /// <param name="groundHeight">Height to place ground at (-1 to 1).</param>
        /// <param name="groundFadeAmount">Factor for fade between ground and horizon (0 to 1).</param>
        /// <param name="horizonSkyBlend">Blending factor between horizon and sky (0.1 to 2).</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetGradientSky(Color groundColor, Color horizonColor, Color zenithColor,
            float groundHeight = 0f, float groundFadeAmount = 0.5f, float horizonSkyBlend = 1f)
        {
            if (liteProceduralSkyMaterial == null)
            {
                LogSystem.LogError("[EnvironmentManager->SetGradientSky] No lite procedural sky material.");
                return false;
            }

            if (liteProceduralSkyObject == null)
            {
                LogSystem.LogError("[EnvironmentManager->SetGradientSky] No lite procedural sky object.");
                return false;
            }

            groundHeight = Mathf.Clamp(groundHeight, -1f, 1f);
            groundFadeAmount = Mathf.Clamp01(groundFadeAmount);
            horizonSkyBlend = Mathf.Clamp(horizonSkyBlend, 0.1f, 2f);

            // Set gradient colors
            liteProceduralSkyMaterial.SetInt("_GroundEnabled", 1);
            liteProceduralSkyMaterial.SetColor("_GroundColor", groundColor);
            liteProceduralSkyMaterial.SetFloat("_Ground_Height", groundHeight);
            liteProceduralSkyMaterial.SetFloat("_Constant_Color_Mode", 1);
            liteProceduralSkyMaterial.SetFloat("_GroundFadeAmount", groundFadeAmount);
            liteProceduralSkyMaterial.SetFloat("_SkyColorBlend", horizonSkyBlend);
            liteProceduralSkyMaterial.SetColor("_HorizonColorDay", horizonColor);
            liteProceduralSkyMaterial.SetColor("_SkyColorDay", zenithColor);
            liteProceduralSkyMaterial.SetFloat("_HorizonSaturationAmount", 0f);
            liteProceduralSkyMaterial.SetFloat("_HorizonSaturationFalloff", 1f);

            // Disable sun, moon, stars, clouds for simple gradient
            liteProceduralSkyMaterial.SetInt("_Sun_Enabled", 0);
            liteProceduralSkyMaterial.SetInt("_Moon_Enabled", 0);
            liteProceduralSkyMaterial.SetInt("_Stars_Enabled", 0);
            liteProceduralSkyMaterial.SetInt("_Clouds_Enabled", 0);

            ApplyLiteProceduralSkyMaterial();

            return true;
        }

        /// <summary>
        /// Set the sky to a texture.
        /// </summary>
        /// <param name="texture">Texture to set the sky to.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetSkyTexture(Texture2D texture)
        {
            skyMaterial.SetTexture("_MainTex", texture);
            skyMaterial.SetColor("_Tint", new Color((float) 128 / 255,
                (float) 128 / 255, (float) 128 / 255, (float) 128 / 255));
            
            ApplySkyMaterial();
            
            return true;
        }

        /// <summary>
        /// Set the sky to a cubemap from 6 individual face textures.
        /// Face order follows glTF/OMI convention: +X, -X, +Y, -Y, +Z, -Z (right-handed).
        /// This method handles the coordinate system conversion to Unity (left-handed).
        /// </summary>
        /// <param name="posX">Positive X face texture (+X, right in glTF).</param>
        /// <param name="negX">Negative X face texture (-X, left in glTF).</param>
        /// <param name="posY">Positive Y face texture (+Y, top/up).</param>
        /// <param name="negY">Negative Y face texture (-Y, bottom/down).</param>
        /// <param name="posZ">Positive Z face texture (+Z, front in glTF, back in Unity).</param>
        /// <param name="negZ">Negative Z face texture (-Z, back in glTF, front in Unity).</param>
        /// <param name="exposure">Exposure/brightness multiplier (default 1).</param>
        /// <param name="rotation">Rotation in degrees around up axis (default 0).</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetCubemapSky(Texture2D posX, Texture2D negX, Texture2D posY, Texture2D negY, Texture2D posZ, Texture2D negZ,
            float exposure = 1f, float rotation = 0f)
        {
            if (posX == null || negX == null || posY == null || negY == null || posZ == null || negZ == null)
            {
                LogSystem.LogError("[EnvironmentManager->SetCubemapSky] One or more face textures are null.");
                return false;
            }

            // All faces must be the same size and square
            int size = posX.width;
            if (posX.height != size)
            {
                LogSystem.LogError("[EnvironmentManager->SetCubemapSky] Cubemap face textures must be square.");
                return false;
            }

            // Create the cubemap
            Cubemap cubemap = new Cubemap(size, posX.format, posX.mipmapCount > 1);
            
            // glTF uses right-handed coordinates, Unity uses left-handed
            // Face mapping and rotation corrections based on testing:
            // - glTF +X -> Unity +X (rotated 180)
            // - glTF -X -> Unity -X (rotated 180)
            // - glTF +Y -> Unity +Y (rotated 180)
            // - glTF -Y -> Unity -Y (rotated 180)
            // - glTF +Z -> Unity +Z (rotated 180)
            // - glTF -Z -> Unity -Z (rotated 180)
            
            CopyTextureRotated180(posX, cubemap, CubemapFace.PositiveX);
            CopyTextureRotated180(negX, cubemap, CubemapFace.NegativeX);
            CopyTextureRotated180(posY, cubemap, CubemapFace.PositiveY);
            CopyTextureRotated180(negY, cubemap, CubemapFace.NegativeY);
            CopyTextureRotated180(posZ, cubemap, CubemapFace.PositiveZ);
            CopyTextureRotated180(negZ, cubemap, CubemapFace.NegativeZ);

            // Find or create cubemap material
            var shader = Shader.Find("Skybox/Cubemap");
            if (shader == null)
            {
                LogSystem.LogError("[EnvironmentManager->SetCubemapSky] Could not find Skybox/Cubemap shader.");
                return false;
            }

            var cubemapMaterial = new Material(shader);
            cubemapMaterial.SetTexture("_Tex", cubemap);
            cubemapMaterial.SetFloat("_Exposure", exposure);
            cubemapMaterial.SetFloat("_Rotation", rotation);

            RenderSettings.skybox = cubemapMaterial;
            liteProceduralSkyObject?.SetActive(false);
            
            DynamicGI.UpdateEnvironment();

            return true;
        }

        /// <summary>
        /// Copy a texture to a cubemap face with 180 degree rotation using GPU blit.
        /// </summary>
        private void CopyTextureRotated180(Texture2D source, Cubemap cubemap, CubemapFace face)
        {
            int size = source.width;
            
            // Create a temporary RenderTexture
            RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;
            
            // Blit with scale (-1, -1) to rotate 180 degrees
            Graphics.Blit(source, rt, new Vector2(-1, -1), new Vector2(1, 1));
            
            // Read back from RenderTexture to a temporary texture, then copy to cubemap
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D tempTex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            tempTex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tempTex.Apply();
            RenderTexture.active = previous;
            
            // Copy mip 0 to mip 0 of the cubemap face
            Graphics.CopyTexture(tempTex, 0, 0, cubemap, (int)face, 0);
            
            // Cleanup
            Object.Destroy(tempTex);
            RenderTexture.ReleaseTemporary(rt);
        }

        /// <summary>
        /// Set the sky to an equirectangular panorama texture.
        /// </summary>
        /// <param name="panoramaTexture">The equirectangular panorama texture.</param>
        /// <param name="exposure">Exposure/brightness multiplier (default 1).</param>
        /// <param name="rotation">Rotation in degrees around up axis (default 0).</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetPanoramaSky(Texture2D panoramaTexture, float exposure = 1f, float rotation = 0f)
        {
            if (panoramaTexture == null)
            {
                LogSystem.LogError("[EnvironmentManager->SetPanoramaSky] Panorama texture is null.");
                return false;
            }

            var shader = Shader.Find("Skybox/Panoramic");
            if (shader == null)
            {
                LogSystem.LogError("[EnvironmentManager->SetPanoramaSky] Could not find Skybox/Panoramic shader.");
                return false;
            }

            var panoramaMaterial = new Material(shader);
            panoramaMaterial.SetTexture("_MainTex", panoramaTexture);
            panoramaMaterial.SetFloat("_Exposure", exposure);
            panoramaMaterial.SetFloat("_Rotation", rotation);
            panoramaMaterial.SetInt("_Mapping", 1); // Latitude Longitude Layout

            RenderSettings.skybox = panoramaMaterial;
            liteProceduralSkyObject?.SetActive(false);
            
            DynamicGI.UpdateEnvironment();

            return true;
        }

        /// <summary>
        /// Create a lite-mode day/night procedural sky.
        /// </summary>
        /// <param name="sunObject">The object containing the sun.</param>
        /// <param name="groundEnabled">Whether or not to enable procedural ground.</param>
        /// <param name="groundColor">Color for the ground.</param>
        /// <param name="groundHeight">Height to place ground at.</param>
        /// <param name="groundFadeAmount">Factor for fade between ground and horizon.</param>
        /// <param name="horizonSkyBlend">Blending factor between horizon and sky.</param>
        /// <param name="dayHorizonColor">Color for horizon during day.</param>
        /// <param name="daySkyColor">Color for sky during day.</param>
        /// <param name="nightHorizonColor">Color for horizon during night.</param>
        /// <param name="nightSkyColor">Color for sky during night.</param>
        /// <param name="horizonSaturationAmount">Saturation amount for horizon.</param>
        /// <param name="horizonSaturationFalloff">Saturation falloff for horizon.</param>
        /// <param name="sunEnabled">Whether or not to enable procedural sun.</param>
        /// <param name="sunDiameter">Diameter of sun.</param>
        /// <param name="sunHorizonColor">Color of sun at horizon.</param>
        /// <param name="sunZenithColor">Color of sun at zenith.</param>
        /// <param name="sunSkyLightingEnabled">Whether or not to enable lighting of sky from sun.</param>
        /// <param name="skyLightingFalloffAmount">Falloff amount for sky lighting.</param>
        /// <param name="skyLightingFalloffIntensity">Falloff intensity for sky lighting.</param>
        /// <param name="sunsetIntensity">Intensity of sunset.</param>
        /// <param name="sunsetRadialFalloff">Radial falloff of sunset.</param>
        /// <param name="sunsetHorizontalFalloff">Horizontal falloff of sunset.</param>
        /// <param name="sunsetVerticalFalloff">Vertical falloff of sunset.</param>
        /// <param name="moonEnabled">Whether or not to enable procedural moon.</param>
        /// <param name="moonDiameter">Diameter of moon.</param>
        /// <param name="moonColor">Color of moon.</param>
        /// <param name="moonFalloffAmount">Falloff amount for moonlight.</param>
        /// <param name="starsEnabled">Whether or not to enable stars.</param>
        /// <param name="starsBrightness">Brightness for stars.</param>
        /// <param name="starsDaytimeBrightness">Daytime brightness for stars.</param>
        /// <param name="starsHorizonFalloff">Falloff for stars at horizon.</param>
        /// <param name="starsSaturation">Saturation for stars.</param>
        /// <param name="proceduralStarsEnabled">Whether or not to enable procedural stars.</param>
        /// <param name="proceduralStarsSharpness">Sharpness for procedural stars.</param>
        /// <param name="proceduralStarsAmount">Amount of procedural stars.</param>
        /// <param name="starTextureEnabled">Whether or not to enable star texture (overlaid on top
        /// of procedural stars).</param>
        /// <param name="starTexture">Texture for non-procedural stars.</param>
        /// <param name="starTint">Tint for non-procedural stars.</param>
        /// <param name="starScale">Scale for non-procedural stars.</param>
        /// <param name="starRotationSpeed">Rotation speed for non-procedural stars.</param>
        /// <param name="cloudsEnabled">Whether or not to enable procedural clouds.</param>
        /// <param name="cloudsTexture">Texture for clouds.</param>
        /// <param name="cloudsScale">Scale for clouds.</param>
        /// <param name="cloudsSpeed">Speed for clouds.</param>
        /// <param name="cloudiness">Cloudiness level.</param>
        /// <param name="cloudsOpacity">Cloud opacity level.</param>
        /// <param name="cloudsSharpness">Cloud sharpness level.</param>
        /// <param name="cloudsShadingIntensity">Intensity of cloud shading.</param>
        /// <param name="cloudsZenithFalloff">Falloff for clouds at zenith.</param>
        /// <param name="cloudsIterations">Number of iterations for procedural clouds.</param>
        /// <param name="cloudsGain">Gain for procedural clouds.</param>
        /// <param name="cloudsLacunarity">Lacunarity for procedural clouds.</param>
        /// <param name="cloudsDayColor">Color for clouds during day.</param>
        /// <param name="cloudsNightColor">Color for clouds during night.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool CreateDayNightLiteSky(GameObject sunObject, bool groundEnabled, Color groundColor, float groundHeight, float groundFadeAmount,
        float horizonSkyBlend, Color dayHorizonColor, Color daySkyColor, Color nightHorizonColor, Color nightSkyColor,
        float horizonSaturationAmount, float horizonSaturationFalloff, bool sunEnabled, float sunDiameter, Color sunHorizonColor,
        Color sunZenithColor, bool sunSkyLightingEnabled, float skyLightingFalloffAmount, float skyLightingFalloffIntensity,
        float sunsetIntensity, float sunsetRadialFalloff, float sunsetHorizontalFalloff, float sunsetVerticalFalloff, bool moonEnabled,
        float moonDiameter, Color moonColor, float moonFalloffAmount, bool starsEnabled, float starsBrightness,
        float starsDaytimeBrightness, float starsHorizonFalloff, float starsSaturation, bool proceduralStarsEnabled,
        float proceduralStarsSharpness, float proceduralStarsAmount, bool starTextureEnabled, Texture2D starTexture, Color starTint,
        float starScale, float starRotationSpeed, bool cloudsEnabled, Texture2D cloudsTexture, Vector2 cloudsScale, Vector2 cloudsSpeed,
        float cloudiness, float cloudsOpacity, float cloudsSharpness, float cloudsShadingIntensity, float cloudsZenithFalloff,
        int cloudsIterations, float cloudsGain, int cloudsLacunarity, Color cloudsDayColor, Color cloudsNightColor)
        {
            if (sunObject == null)
            {
                Debug.LogError("[EnvironmentManager->CreateDayNightSky] Invalid sun object.");
                return false;
            }

            if (groundHeight < -1 || groundHeight > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid ground height. Must be between -1 and 1.");
                return false;
            }

            if (groundFadeAmount < 0 || groundFadeAmount > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid ground fade amount. Must be between 0 and 1.");
                return false;
            }

            if (horizonSkyBlend < 0.1f || horizonSkyBlend > 2)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid horizon sky blend. Must be between 0.1 and 2.");
                return false;
            }

            if (horizonSaturationAmount < 0 || horizonSaturationAmount > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid horizon saturation amount. Must be between 0 and 1.");
                return false;
            }

            if (horizonSaturationFalloff < 1 || horizonSaturationFalloff > 10)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid horizon saturation falloff. Must be between 1 and 10.");
                return false;
            }

            if (sunDiameter < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid sun diameter. Must be at least 0.");
                return false;
            }

            if (skyLightingFalloffAmount < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid sky lighting falloff amount. Must be at least 0.");
                return false;
            }

            if (skyLightingFalloffIntensity < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid sky lighting falloff intensity. Must be at least 0.");
                return false;
            }

            if (sunsetIntensity < 0 || sunsetIntensity > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid sunset intensity. Must be between 0 and 1.");
                return false;
            }

            if (sunsetRadialFalloff < 0.01f || sunsetRadialFalloff > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid sunset radial falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (sunsetHorizontalFalloff < 0.01f || sunsetHorizontalFalloff > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid sunset horizontal falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (sunsetVerticalFalloff < 0.01f || sunsetVerticalFalloff > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid sunset vertical falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (moonDiameter < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid moon diameter. Must be at least 0.");
                return false;
            }

            if (moonFalloffAmount < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid moon falloff amount. Must be at least 0.");
                return false;
            }

            if (starsBrightness < 0 || starsBrightness > 3)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid stars brightness. Must be between 0 and 3.");
                return false;
            }

            if (starsDaytimeBrightness < 0 || starsDaytimeBrightness > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid stars daytime brightness. Must be between 0 and 1.");
                return false;
            }

            if (starsHorizonFalloff < 0 || starsHorizonFalloff > 2)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid stars horizon falloff. Must be between 0 and 2.");
                return false;
            }

            if (starsSaturation < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid stars saturation. Must be at least 0.");
                return false;
            }

            if (cloudiness < 0 || cloudiness > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid cloudiness. Must be between 0 and 1.");
                return false;
            }

            if (cloudsOpacity < 0 || cloudsOpacity > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid clouds opacity. Must be between 0 and 1.");
                return false;
            }

            if (cloudsSharpness < 0 || cloudsSharpness > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid clouds sharpness. Must be between 0 and 1.");
                return false;
            }

            if (cloudsShadingIntensity < 0 || cloudsShadingIntensity > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid clouds shading intensity. Must be between 0 and 1.");
                return  false;
            }

            if (cloudsZenithFalloff < 0 || cloudsZenithFalloff > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid clouds zenith falloff. Must be between 0 and 1.");
                return false;
            }

            if (cloudsIterations < 1 || cloudsIterations > 4)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid clouds iterations. Must be between 1 and 4.");
                return false;
            }

            if (cloudsGain < 0 || cloudsGain > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid clouds gain. Must be between 0 and 1.");
                return false;
            }

            if (cloudsLacunarity < 2 || cloudsLacunarity > 5)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid clouds lacunarity. Must be between 2 and 5.");
                return false;
            }

            foreach (Sun sunToRemove in sunObject.GetComponents<Sun>())
            {
                Destroy(sunToRemove);
            }

            foreach (SetStarRotation setStarRotationToRemove in sunObject.GetComponents<SetStarRotation>())
            {
                Destroy(setStarRotationToRemove);
            }

            Light light = sunObject.GetComponent<Light>();
            if (light == null)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid sun object.");
                return false;
            }

            Sun sun = sunObject.AddComponent<Sun>();
            sun.MaximumLightIntensity = light.intensity;

            SetStarRotation setStarRotation = sunObject.AddComponent<SetStarRotation>();

            liteProceduralSkyMaterial.SetInt("_GroundEnabled", groundEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetColor("_GroundColor", groundColor);
            liteProceduralSkyMaterial.SetFloat("_Ground_Height", groundHeight);
            liteProceduralSkyMaterial.SetFloat("_Constant_Color_Mode", 0);
            liteProceduralSkyMaterial.SetFloat("_GroundFadeAmount", groundFadeAmount);
            liteProceduralSkyMaterial.SetFloat("_SkyColorBlend", horizonSkyBlend);
            liteProceduralSkyMaterial.SetColor("_HorizonColorDay", dayHorizonColor);
            liteProceduralSkyMaterial.SetColor("_SkyColorDay", daySkyColor);
            liteProceduralSkyMaterial.SetColor("_HorizonColorNight", nightHorizonColor);
            liteProceduralSkyMaterial.SetColor("_SkyColorNight", nightSkyColor);
            liteProceduralSkyMaterial.SetFloat("_HorizonSaturationAmount", horizonSaturationAmount);
            liteProceduralSkyMaterial.SetFloat("_HorizonSaturationFalloff", horizonSaturationFalloff);
            liteProceduralSkyMaterial.SetInt("_Sun_Enabled", sunEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_SunAngularDiameter", sunDiameter);
            liteProceduralSkyMaterial.SetColor("_SunColorHorizon", sunHorizonColor);
            liteProceduralSkyMaterial.SetColor("_SunColorZenith", sunZenithColor);
            liteProceduralSkyMaterial.SetInt("_SunSkyLightingEnabled", sunSkyLightingEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_SunFalloff", skyLightingFalloffAmount);
            liteProceduralSkyMaterial.SetFloat("_SunFalloffIntensity", skyLightingFalloffIntensity);
            liteProceduralSkyMaterial.SetFloat("_SunsetIntensity", sunsetIntensity);
            liteProceduralSkyMaterial.SetFloat("_SunsetRadialFalloff", sunsetRadialFalloff);
            liteProceduralSkyMaterial.SetFloat("_SunsetHorizontalFalloff", sunsetHorizontalFalloff);
            liteProceduralSkyMaterial.SetFloat("_SunsetVerticalFalloff", sunsetVerticalFalloff);
            liteProceduralSkyMaterial.SetInt("_Moon_Enabled", moonEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_MoonAngularDiameter", moonDiameter);
            liteProceduralSkyMaterial.SetColor("_MoonColor", moonColor);
            liteProceduralSkyMaterial.SetFloat("_MoonFalloff", moonFalloffAmount);
            liteProceduralSkyMaterial.SetInt("_Stars_Enabled", starsEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_StarIntensity", starsBrightness);
            liteProceduralSkyMaterial.SetFloat("_StarDaytimeBrightness", starsDaytimeBrightness);
            liteProceduralSkyMaterial.SetFloat("_StarHorizonFalloff", starsHorizonFalloff);
            liteProceduralSkyMaterial.SetFloat("_StarSaturation", starsSaturation);
            liteProceduralSkyMaterial.SetInt("_ProceduralStarsEnabled", proceduralStarsEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_StarSharpness", proceduralStarsSharpness);
            liteProceduralSkyMaterial.SetFloat("_StarFrequency", proceduralStarsAmount);
            liteProceduralSkyMaterial.SetInt("_Use_Texture_Stars", starTextureEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetTexture("_StarTexture", starTexture == null ? defaultStarTexture : starTexture);
            liteProceduralSkyMaterial.SetColor("_Star_Texture_Tint", starTint);
            liteProceduralSkyMaterial.SetFloat("_StarScale", starScale);
            liteProceduralSkyMaterial.SetFloat("_StarSpeed", starRotationSpeed);
            liteProceduralSkyMaterial.SetInt("_Clouds_Enabled", cloudsEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetTexture("_CloudTexture", cloudsTexture == null ? defaultCloudTexture : cloudsTexture);
            liteProceduralSkyMaterial.SetVector("_CloudScale", new Vector4(cloudsScale.x, cloudsScale.y, 0, 0));
            liteProceduralSkyMaterial.SetVector("_CloudWindSpeed", new Vector4(cloudsSpeed.x, cloudsSpeed.y, 0, 0));
            liteProceduralSkyMaterial.SetFloat("_Cloudiness", cloudiness);
            liteProceduralSkyMaterial.SetFloat("_CloudOpacity", cloudsOpacity);
            liteProceduralSkyMaterial.SetFloat("_CloudSharpness", cloudsSharpness);
            liteProceduralSkyMaterial.SetFloat("_Shading_Intensity", cloudsShadingIntensity);
            liteProceduralSkyMaterial.SetFloat("_CloudFalloff", cloudsZenithFalloff);
            liteProceduralSkyMaterial.SetInt("_Cloud_Iterations", cloudsIterations);
            liteProceduralSkyMaterial.SetFloat("_Cloud_Gain", cloudsGain);
            liteProceduralSkyMaterial.SetInt("_Cloud_Lacunarity", cloudsLacunarity);
            liteProceduralSkyMaterial.SetColor("_CloudColorDay", cloudsDayColor);
            liteProceduralSkyMaterial.SetColor("_CloudColorNight", cloudsNightColor);

            ApplyLiteProceduralSkyMaterial();

            return true;
        }

        /// <summary>
        /// Create a lite-mode constant color procedural sky.
        /// </summary>
        /// <param name="sunObject">The object containing the sun.</param>
        /// <param name="groundEnabled">Whether or not to enable procedural ground.</param>
        /// <param name="groundColor">Color for the ground.</param>
        /// <param name="groundHeight">Height to place ground at.</param>
        /// <param name="groundFadeAmount">Factor for fade between ground and horizon.</param>
        /// <param name="horizonSkyBlend">Blending factor between horizon and sky.</param>
        /// <param name="dayHorizonColor">Color for horizon during day.</param>
        /// <param name="daySkyColor">Color for sky during day.</param>
        /// <param name="horizonSaturationAmount">Saturation amount for horizon.</param>
        /// <param name="horizonSaturationFalloff">Saturation falloff for horizon.</param>
        /// <param name="sunEnabled">Whether or not to enable procedural sun.</param>
        /// <param name="sunDiameter">Diameter of sun.</param>
        /// <param name="sunHorizonColor">Color of sun at horizon.</param>
        /// <param name="sunZenithColor">Color of sun at zenith.</param>
        /// <param name="sunSkyLightingEnabled">Whether or not to enable lighting of sky from sun.</param>
        /// <param name="skyLightingFalloffAmount">Falloff amount for sky lighting.</param>
        /// <param name="skyLightingFalloffIntensity">Falloff intensity for sky lighting.</param>
        /// <param name="sunsetIntensity">Intensity of sunset.</param>
        /// <param name="sunsetRadialFalloff">Radial falloff of sunset.</param>
        /// <param name="sunsetHorizontalFalloff">Horizontal falloff of sunset.</param>
        /// <param name="sunsetVerticalFalloff">Vertical falloff of sunset.</param>
        /// <param name="moonEnabled">Whether or not to enable procedural moon.</param>
        /// <param name="moonDiameter">Diameter of moon.</param>
        /// <param name="moonColor">Color of moon.</param>
        /// <param name="moonFalloffAmount">Falloff amount for moonlight.</param>
        /// <param name="starsEnabled">Whether or not to enable stars.</param>
        /// <param name="starsBrightness">Brightness for stars.</param>
        /// <param name="starsDaytimeBrightness">Daytime brightness for stars.</param>
        /// <param name="starsHorizonFalloff">Falloff for stars at horizon.</param>
        /// <param name="starsSaturation">Saturation for stars.</param>
        /// <param name="proceduralStarsEnabled">Whether or not to enable procedural stars.</param>
        /// <param name="proceduralStarsSharpness">Sharpness for procedural stars.</param>
        /// <param name="proceduralStarsAmount">Amount of procedural stars.</param>
        /// <param name="starTextureEnabled">Whether or not to enable star texture (overlaid on top
        /// of procedural stars).</param>
        /// <param name="starTexture">Texture for non-procedural stars.</param>
        /// <param name="starTint">Tint for non-procedural stars.</param>
        /// <param name="starScale">Scale for non-procedural stars.</param>
        /// <param name="starRotationSpeed">Rotation speed for non-procedural stars.</param>
        /// <param name="cloudsEnabled">Whether or not to enable procedural clouds.</param>
        /// <param name="cloudsTexture">Texture for clouds.</param>
        /// <param name="cloudsScale">Scale for clouds.</param>
        /// <param name="cloudsSpeed">Speed for clouds.</param>
        /// <param name="cloudiness">Cloudiness level.</param>
        /// <param name="cloudsOpacity">Cloud opacity level.</param>
        /// <param name="cloudsSharpness">Cloud sharpness level.</param>
        /// <param name="cloudsShadingIntensity">Intensity of cloud shading.</param>
        /// <param name="cloudsZenithFalloff">Falloff for clouds at zenith.</param>
        /// <param name="cloudsIterations">Number of iterations for procedural clouds.</param>
        /// <param name="cloudsGain">Gain for procedural clouds.</param>
        /// <param name="cloudsLacunarity">Lacunarity for procedural clouds.</param>
        /// <param name="cloudsDayColor">Color for clouds during day.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool CreateConstantColorLiteSky(GameObject sunObject, bool groundEnabled, Color groundColor, float groundHeight, float groundFadeAmount,
            float horizonSkyBlend, Color dayHorizonColor, Color daySkyColor, float horizonSaturationAmount, float horizonSaturationFalloff,
            bool sunEnabled, float sunDiameter, Color sunHorizonColor, Color sunZenithColor, bool sunSkyLightingEnabled,
            float skyLightingFalloffAmount, float skyLightingFalloffIntensity, float sunsetIntensity, float sunsetRadialFalloff,
            float sunsetHorizontalFalloff, float sunsetVerticalFalloff, bool moonEnabled, float moonDiameter, Color moonColor,
            float moonFalloffAmount, bool starsEnabled, float starsBrightness, float starsDaytimeBrightness, float starsHorizonFalloff,
            float starsSaturation, bool proceduralStarsEnabled, float proceduralStarsSharpness, float proceduralStarsAmount,
            bool starTextureEnabled, Texture2D starTexture, Color starTint, float starScale, float starRotationSpeed, bool cloudsEnabled,
            Texture2D cloudsTexture, Vector2 cloudsScale, Vector2 cloudsSpeed, float cloudiness, float cloudsOpacity, float cloudsSharpness,
            float cloudsShadingIntensity, float cloudsZenithFalloff, int cloudsIterations, float cloudsGain, int cloudsLacunarity,
            Color cloudsDayColor)
        {
            if (sunObject == null)
            {
                Debug.LogError("[EnvironmentManager->CreateConstantColorSky] Invalid sun object.");
                return false;
            }

            if (groundHeight < -1 || groundHeight > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid ground height. Must be between -1 and 1.");
                return false;
            }

            if (groundFadeAmount < 0 || groundFadeAmount > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid ground fade amount. Must be between 0 and 1.");
                return false;
            }

            if (horizonSkyBlend < 0.1f || horizonSkyBlend > 2)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid horizon sky blend. Must be between 0.1 and 2.");
                return false;
            }

            if (horizonSaturationAmount < 0 || horizonSaturationAmount > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid horizon saturation amount. Must be between 0 and 1.");
                return false;
            }

            if (horizonSaturationFalloff < 1 || horizonSaturationFalloff > 10)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid horizon saturation falloff. Must be between 1 and 10.");
                return false;
            }

            if (sunDiameter < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid sun diameter. Must be at least 0.");
                return false;
            }

            if (skyLightingFalloffAmount < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid sky lighting falloff amount. Must be at least 0.");
                return false;
            }

            if (skyLightingFalloffIntensity < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid sky lighting falloff intensity. Must be at least 0.");
                return false;
            }

            if (sunsetIntensity < 0 || sunsetIntensity > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid sunset intensity. Must be between 0 and 1.");
                return false;
            }

            if (sunsetRadialFalloff < 0.01f || sunsetRadialFalloff > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid sunset radial falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (sunsetHorizontalFalloff < 0.01f || sunsetHorizontalFalloff > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid sunset horizontal falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (sunsetVerticalFalloff < 0.01f || sunsetVerticalFalloff > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid sunset vertical falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (moonDiameter < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid moon diameter. Must be at least 0.");
                return false;
            }

            if (moonFalloffAmount < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid moon falloff amount. Must be at least 0.");
                return false;
            }

            if (starsBrightness < 0 || starsBrightness > 3)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid stars brightness. Must be between 0 and 3.");
                return false;
            }

            if (starsDaytimeBrightness < 0 || starsDaytimeBrightness > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid stars daytime brightness. Must be between 0 and 1.");
                return false;
            }

            if (starsHorizonFalloff < 0 || starsHorizonFalloff > 2)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid stars horizon falloff. Must be between 0 and 2.");
                return false;
            }

            if (starsSaturation < 0)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid stars saturation. Must be at least 0.");
                return false;
            }

            if (cloudiness < 0 || cloudiness > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid cloudiness. Must be between 0 and 1.");
                return false;
            }

            if (cloudsOpacity < 0 || cloudsOpacity > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid clouds opacity. Must be between 0 and 1.");
                return false;
            }

            if (cloudsSharpness < 0 || cloudsSharpness > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid clouds sharpness. Must be between 0 and 1.");
                return false;
            }

            if (cloudsShadingIntensity < 0 || cloudsShadingIntensity > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid clouds shading intensity. Must be between 0 and 1.");
                return  false;
            }

            if (cloudsZenithFalloff < 0 || cloudsZenithFalloff > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid clouds zenith falloff. Must be between 0 and 1.");
                return false;
            }

            if (cloudsIterations < 1 || cloudsIterations > 4)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid clouds iterations. Must be between 1 and 4.");
                return false;
            }

            if (cloudsGain < 0 || cloudsGain > 1)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid clouds gain. Must be between 0 and 1.");
                return false;
            }

            if (cloudsLacunarity < 2 || cloudsLacunarity > 5)
            {
                Debug.LogWarning("[EnvironmentManager->CreateConstantColorSky] Invalid clouds lacunarity. Must be between 2 and 5.");
                return false;
            }

            foreach (Sun sunToRemove in sunObject.GetComponents<Sun>())
            {
                Destroy(sunToRemove);
            }

            foreach (SetStarRotation setStarRotationToRemove in sunObject.GetComponents<SetStarRotation>())
            {
                Destroy(setStarRotationToRemove);
            }

            Light light = sunObject.GetComponent<Light>();
            if (light == null)
            {
                Debug.LogWarning("[EnvironmentManager->CreateDayNightSky] Invalid sun object.");
                return false;
            }

            Sun sun = sunObject.AddComponent<Sun>();
            sun.MaximumLightIntensity = light.intensity;

            SetStarRotation setStarRotation = sunObject.AddComponent<SetStarRotation>();

            liteProceduralSkyMaterial.SetInt("_GroundEnabled", groundEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetColor("_GroundColor", groundColor);
            liteProceduralSkyMaterial.SetFloat("_Ground_Height", groundHeight);
            liteProceduralSkyMaterial.SetFloat("_Constant_Color_Mode", 1);
            liteProceduralSkyMaterial.SetFloat("_GroundFadeAmount", groundFadeAmount);
            liteProceduralSkyMaterial.SetFloat("_SkyColorBlend", horizonSkyBlend);
            liteProceduralSkyMaterial.SetColor("_HorizonColorDay", dayHorizonColor);
            liteProceduralSkyMaterial.SetColor("_SkyColorDay", daySkyColor);
            liteProceduralSkyMaterial.SetFloat("_HorizonSaturationAmount", horizonSaturationAmount);
            liteProceduralSkyMaterial.SetFloat("_HorizonSaturationFalloff", horizonSaturationFalloff);
            liteProceduralSkyMaterial.SetInt("_Sun_Enabled", sunEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_SunAngularDiameter", sunDiameter);
            liteProceduralSkyMaterial.SetColor("_SunColorHorizon", sunHorizonColor);
            liteProceduralSkyMaterial.SetColor("_SunColorZenith", sunZenithColor);
            liteProceduralSkyMaterial.SetInt("_SunSkyLightingEnabled", sunSkyLightingEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_SunFalloff", skyLightingFalloffAmount);
            liteProceduralSkyMaterial.SetFloat("_SunFalloffIntensity", skyLightingFalloffIntensity);
            liteProceduralSkyMaterial.SetFloat("_SunsetIntensity", sunsetIntensity);
            liteProceduralSkyMaterial.SetFloat("_SunsetRadialFalloff", sunsetRadialFalloff);
            liteProceduralSkyMaterial.SetFloat("_SunsetHorizontalFalloff", sunsetHorizontalFalloff);
            liteProceduralSkyMaterial.SetFloat("_SunsetVerticalFalloff", sunsetVerticalFalloff);
            liteProceduralSkyMaterial.SetInt("_Moon_Enabled", moonEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_MoonAngularDiameter", moonDiameter);
            liteProceduralSkyMaterial.SetColor("_MoonColor", moonColor);
            liteProceduralSkyMaterial.SetFloat("_MoonFalloff", moonFalloffAmount);
            liteProceduralSkyMaterial.SetInt("_Stars_Enabled", starsEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_StarIntensity", starsBrightness);
            liteProceduralSkyMaterial.SetFloat("_StarDaytimeBrightness", starsDaytimeBrightness);
            liteProceduralSkyMaterial.SetFloat("_StarHorizonFalloff", starsHorizonFalloff);
            liteProceduralSkyMaterial.SetFloat("_StarSaturation", starsSaturation);
            liteProceduralSkyMaterial.SetInt("_ProceduralStarsEnabled", proceduralStarsEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetFloat("_StarSharpness", proceduralStarsSharpness);
            liteProceduralSkyMaterial.SetFloat("_StarFrequency", proceduralStarsAmount);
            liteProceduralSkyMaterial.SetInt("_Use_Texture_Stars", starTextureEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetTexture("_StarTexture", starTexture == null ? defaultStarTexture : starTexture);
            liteProceduralSkyMaterial.SetColor("_Star_Texture_Tint", starTint);
            liteProceduralSkyMaterial.SetFloat("_StarScale", starScale);
            liteProceduralSkyMaterial.SetFloat("_StarSpeed", starRotationSpeed);
            liteProceduralSkyMaterial.SetInt("_Clouds_Enabled", cloudsEnabled == true ? 1 : 0);
            liteProceduralSkyMaterial.SetTexture("_CloudTexture", cloudsTexture == null ? defaultCloudTexture : cloudsTexture);
            liteProceduralSkyMaterial.SetVector("_CloudScale", new Vector4(cloudsScale.x, cloudsScale.y, 0, 0));
            liteProceduralSkyMaterial.SetVector("_CloudWindSpeed", new Vector4(cloudsSpeed.x, cloudsSpeed.y, 0, 0));
            liteProceduralSkyMaterial.SetFloat("_Cloudiness", cloudiness);
            liteProceduralSkyMaterial.SetFloat("_CloudOpacity", cloudsOpacity);
            liteProceduralSkyMaterial.SetFloat("_CloudSharpness", cloudsSharpness);
            liteProceduralSkyMaterial.SetFloat("_Shading_Intensity", cloudsShadingIntensity);
            liteProceduralSkyMaterial.SetFloat("_CloudFalloff", cloudsZenithFalloff);
            liteProceduralSkyMaterial.SetInt("_Cloud_Iterations", cloudsIterations);
            liteProceduralSkyMaterial.SetFloat("_Cloud_Gain", cloudsGain);
            liteProceduralSkyMaterial.SetInt("_Cloud_Lacunarity", cloudsLacunarity);
            liteProceduralSkyMaterial.SetColor("_CloudColorDay", cloudsDayColor);

            ApplyLiteProceduralSkyMaterial();

            return true;
        }

        /// <summary>
        /// Activate light-mode fog.
        /// </summary>
        /// <param name="color">Color of the fog.</param>
        /// <param name="density">Density of the fog.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool ActivateLiteFog(Color color, float density)
        {
            if (density < 0 || density > 1)
            {
                LogSystem.LogWarning("[EnvironmentManager->ActivateLiteFog] Invalid density. Must be between 0 and 1.");
                return false;
            }

            RenderSettings.fog = true;
            RenderSettings.fogColor = color;
            RenderSettings.fogDensity = density;
            RenderSettings.fogMode = FogMode.Exponential;

            return true;
        }

        /// <summary>
        /// Disable all fog.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool DisableFog()
        {
            RenderSettings.fog = false;

            return true;
        }

        /// <summary>
        /// Reset the sky to the default.
        /// </summary>
        public void ResetSky()
        {
            if (skyMaterial == null)
            {
                return;
            }

            skyMaterial.SetColor("_Tint", new Color((float) 128 / 255,
                (float) 128 / 255, (float) 128 / 255, (float) 128 / 255));
            skyMaterial.SetTexture("_MainTex", null);
        }
    }
}