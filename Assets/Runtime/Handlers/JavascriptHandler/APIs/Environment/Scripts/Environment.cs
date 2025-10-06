// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Environment
{
    /// <summary>
    /// Class for Environment.
    /// </summary>
    public class Environment
    {
        /// <summary>
        /// Set the offset for the world.
        /// </summary>
        /// <param name="worldOffset">Offset.</param>
        public static void SetWorldOffset(WorldTypes.Vector3 worldOffset)
        {
            StraightFour.StraightFour.ActiveWorld.worldOffset = new Vector3(
                worldOffset.x, worldOffset.y, worldOffset.z);
        }

        /// <summary>
        /// Get the offset for the world.
        /// </summary>
        /// <returns>Offset for the world.</returns>
        public static WorldTypes.Vector3 GetWorldOffset()
        {
            Vector3 worldOffset = StraightFour.StraightFour.ActiveWorld.worldOffset;
                return new WorldTypes.Vector3(worldOffset.x, worldOffset.y, worldOffset.z);
        }

        /// <summary>
        /// Set the threshold for updating world offset.
        /// </summary>
        /// <param name="threshold">Threshold distance.</param>
        public static void SetWorldOffsetUpdateThreshold(float threshold)
        {
            StraightFour.StraightFour.ActiveWorld.worldOffsetUpdateThreshold = threshold;
        }

        /// <summary>
        /// Get the threshold for updating world offset.
        /// </summary>
        /// <returns>Threshold distance.</returns>
        public static float GetWorldOffsetUpdateThreshold()
        {
            return StraightFour.StraightFour.ActiveWorld.worldOffsetUpdateThreshold;
        }

        /// <summary>
        /// Set the character entity to track for the camera.
        /// </summary>
        /// <param name="entity">Character entity to track, or null to stop tracking.</param>
        public static void SetTrackedCharacterEntity(BaseEntity entity)
        {
            StraightFour.Entity.BaseEntity be = EntityAPIHelper.GetPrivateEntity(entity);
            if (be != null && be is StraightFour.Entity.CharacterEntity)
            {
                StraightFour.StraightFour.ActiveWorld.SetTrackedCharacterEntity(
                    (StraightFour.Entity.CharacterEntity)be);
            }
            else
            {
                StraightFour.StraightFour.ActiveWorld.SetTrackedCharacterEntity(null);
            }
        }

        /// <summary>
        /// Set the sky to a texture.
        /// </summary>
        /// <param name="texture">URI of the texture to set the sky to.</param>
        public static void SetSkyTexture(string skyTextureURI)
        {
            Action<byte[]> onDownloaded = new Action<byte[]>((rawData) =>
            {
                if (rawData != null)
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                    texture.LoadImage(rawData);
                    StraightFour.StraightFour.ActiveWorld.environmentManager.SetSkyTexture(texture);
                }
            });
            WebVerseRuntime.Instance.vemlHandler.DownloadFileWithoutCache(
                VEML.VEMLUtilities.FullyQualifyURI(skyTextureURI, WebVerseRuntime.Instance.currentBasePath), onDownloaded);
        }

        /// <summary>
        /// Set the sky to a solid color.
        /// </summary>
        /// <param name="color">Color to set the sky to.</param>
        public static bool SetSolidColorSky(WorldTypes.Color color)
        {
            Color convertedColor = new Color(color.r, color.g, color.b, color.a);
            return StraightFour.StraightFour.ActiveWorld.environmentManager.SetSolidColorSky(convertedColor);
        }

        /// <summary>
        /// Set the sky to lite-mode day/night procedural.
        /// </summary>
        /// <param name="sunEntity">Entity to use for the sun.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetLiteDayNightSky(LightEntity sunEntity)
        {
            return SetLiteDayNightSky(sunEntity, true, new WorldTypes.Color(0.0275f, 0.0275f, 0.0275f, 0), -0.02f,
                0.02f, 0.579f, new WorldTypes.Color(0.2667f, 0.5451f, 0.7725f, 1),
                new WorldTypes.Color(0.0314f, 0.0706f, 0.2549f, 0), new WorldTypes.Color(0.0275f, 0.0196f, 0.0510f, 0),
                new WorldTypes.Color(0.0118f, 0.0039f, 0.0118f, 0), 0.894f, 4.45f, true, 2,
                new WorldTypes.Color(155, 72, 33, 0), new WorldTypes.Color(158, 72, 33, 0), true, 10, 18, 0.039f, 0.1f,
                0.6f, 0.5f, false, 2, new WorldTypes.Color(0.6706f, 0.7255f, 0.7490f, 0), 60, true, 0.85f, 0.1f, 0, 0.3f,
                true, 1, 1, true, null, WorldTypes.Color.white, 0.5f, 0.25f, true, null, new WorldTypes.Vector2(2, 1),
                new WorldTypes.Vector2(0.2f, 0.3f), 0.279f, 1, 0.8f, 0.8f, 0.31f, 3, 0.5f, 2,
                new WorldTypes.Color(0.8431f, 0.8667f, 0.9059f, 1), new WorldTypes.Color(0.4902f, 0.3804f, 0.4353f, 1));
        }

        /// <summary>
        /// Set the sky to lite-mode day/night procedural.
        /// </summary>
        /// <param name="sunEntity">Entity to use for the sun.</param>
        /// <param name="enableGround">Whether or not to enable procedural ground.</param>
        /// <param name="groundColor">Color for the ground.</param>
        /// <param name="groundHeight">Height to place ground at.</param>
        /// <param name="dayHorizonColor">Color for horizon during day.</param>
        /// <param name="daySkyColor">Color for sky during day.</param>
        /// <param name="nightHorizonColor">Color for horizon during night.</param>
        /// <param name="nightSkyColor">Color for sky during night.</param>
        /// <param name="enableSun">Whether or not to enable procedural sun.</param>
        /// <param name="sunDiameter">Diameter of sun.</param>
        /// <param name="sunColor">Color of sun.</param>
        /// <param name="enableMoon">Whether or not to enable procedural moon.</param>
        /// <param name="moonDiameter">Diameter of moon.</param>
        /// <param name="moonColor">Color of moon.</param>
        /// <param name="enableStars">Whether or not to enable stars.</param>
        /// <param name="starsBrightness">Brightness for stars.</param>
        /// <param name="starRotationSpeed">Rotation speed for non-procedural stars.</param>
        /// <param name="enableClouds">Whether or not to enable procedural clouds.</param>
        /// <param name="cloudsSpeed">Speed for clouds.</param>
        /// <param name="cloudiness">Cloudiness level.</param>
        /// <param name="cloudsOpacity">Cloud opacity level.</param>
        /// <param name="cloudsDayColor">Color for clouds during day.</param>
        /// <param name="cloudsNightColor">Color for clouds during night.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetLiteDayNightSky(LightEntity sunEntity, bool enableGround, WorldTypes.Color groundColor,
            float groundHeight, WorldTypes.Color dayHorizonColor, WorldTypes.Color daySkyColor,
            WorldTypes.Color nightHorizonColor, WorldTypes.Color nightSkyColor, bool enableSun, float sunDiameter,
            WorldTypes.Color sunColor, bool enableMoon, float moonDiameter, WorldTypes.Color moonColor,
            bool enableStars, float starsBrightness, float starRotationSpeed, bool enableClouds,
            WorldTypes.Vector2 cloudsSpeed, float cloudiness, float cloudsOpacity, WorldTypes.Color cloudsDayColor,
            WorldTypes.Color cloudsNightColor)
        {
            return SetLiteDayNightSky(sunEntity, enableGround, groundColor, groundHeight, 0.02f, 0.579f,
                dayHorizonColor, daySkyColor, nightHorizonColor, nightSkyColor, 0.894f, 4.45f, enableSun, sunDiameter,
                sunColor, sunColor, true, 10, 18, 0.039f, 0.1f, 0.6f, 0.5f, enableMoon, moonDiameter, moonColor, 60, enableStars,
                starsBrightness, 0.1f, 0, 0.3f, true, 1, 1, true, null, WorldTypes.Color.white, 0.5f, starRotationSpeed,
                enableClouds, null, new WorldTypes.Vector2(2, 1), cloudsSpeed, cloudiness, cloudsOpacity, 0.8f, 0.8f, 0.31f, 3,
                0.5f, 2, cloudsDayColor, cloudsNightColor);
        }

        /// <summary>
        /// Set the sky to lite-mode day/night procedural.
        /// </summary>
        /// <param name="sunEntity">Entity to use for the sun.</param>
        /// <param name="enableGround">Whether or not to enable procedural ground.</param>
        /// <param name="groundColor">Color for the ground.</param>
        /// <param name="groundHeight">Height to place ground at.</param>
        /// <param name="dayHorizonColor">Color for horizon during day.</param>
        /// <param name="daySkyColor">Color for sky during day.</param>
        /// <param name="nightHorizonColor">Color for horizon during night.</param>
        /// <param name="nightSkyColor">Color for sky during night.</param>
        /// <param name="enableSun">Whether or not to enable procedural sun.</param>
        /// <param name="sunDiameter">Diameter of sun.</param>
        /// <param name="sunColor">Color of sun.</param>
        /// <param name="enableMoon">Whether or not to enable procedural moon.</param>
        /// <param name="moonDiameter">Diameter of moon.</param>
        /// <param name="moonColor">Color of moon.</param>
        /// <param name="enableStars">Whether or not to enable stars.</param>
        /// <param name="starsBrightness">Brightness for stars.</param>
        /// <param name="starTextureURI">URI to the texture for non-procedural stars.</param>
        /// <param name="starTint">Tint for non-procedural stars.</param>
        /// <param name="starScale">Scale for non-procedural stars.</param>
        /// <param name="starRotationSpeed">Rotation speed for non-procedural stars.</param>
        /// <param name="enableClouds">Whether or not to enable procedural clouds.</param>
        /// <param name="cloudsTextureURI">URI to texture for clouds.</param>
        /// <param name="cloudsScale">Scale for clouds.</param>
        /// <param name="cloudsSpeed">Speed for clouds.</param>
        /// <param name="cloudiness">Cloudiness level.</param>
        /// <param name="cloudsOpacity">Cloud opacity level.</param>
        /// <param name="cloudsDayColor">Color for clouds during day.</param>
        /// <param name="cloudsNightColor">Color for clouds during night.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetLiteDayNightSky(LightEntity sunEntity, bool enableGround, WorldTypes.Color groundColor,
            float groundHeight, WorldTypes.Color dayHorizonColor, WorldTypes.Color daySkyColor,
            WorldTypes.Color nightHorizonColor, WorldTypes.Color nightSkyColor, bool enableSun, float sunDiameter,
            WorldTypes.Color sunColor, bool enableMoon, float moonDiameter, WorldTypes.Color moonColor,
            bool enableStars, float starsBrightness, string starTextureURI, WorldTypes.Color starTint, float starScale,
            float starRotationSpeed, bool enableClouds, string cloudsTextureURI, WorldTypes.Vector2 cloudsScale,
            WorldTypes.Vector2 cloudsSpeed, float cloudiness, float cloudsOpacity, WorldTypes.Color cloudsDayColor,
            WorldTypes.Color cloudsNightColor)
        {
            return SetLiteDayNightSky(sunEntity, enableGround, groundColor, groundHeight, 0.02f, 0.579f,
                dayHorizonColor, daySkyColor, nightHorizonColor, nightSkyColor, 0.894f, 4.45f, enableSun, sunDiameter,
                sunColor, sunColor, true, 10, 18, 0.039f, 0.1f, 0.6f, 0.5f, enableMoon, moonDiameter, moonColor, 60, enableStars,
                starsBrightness, 0.1f, 0, 0.3f, true, 1, 1, true, starTextureURI, starTint, starScale, starRotationSpeed,
                enableClouds, cloudsTextureURI, cloudsScale, cloudsSpeed, cloudiness, cloudsOpacity, 0.8f, 0.8f, 0.31f, 3, 0.5f,
                2, cloudsDayColor, cloudsNightColor);
        }

        /// <summary>
        /// Set the sky to lite-mode day/night procedural.
        /// </summary>
        /// <param name="sunEntity">Entity to use for the sun.</param>
        /// <param name="enableGround">Whether or not to enable procedural ground.</param>
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
        /// <param name="enableSun">Whether or not to enable procedural sun.</param>
        /// <param name="sunDiameter">Diameter of sun.</param>
        /// <param name="sunHorizonColor">Color of sun at horizon.</param>
        /// <param name="sunZenithColor">Color of sun at zenith.</param>
        /// <param name="enableSunSkyLighting">Whether or not to enable lighting of sky from sun.</param>
        /// <param name="skyLightingFalloffAmount">Falloff amount for sky lighting.</param>
        /// <param name="skyLightingFalloffIntensity">Falloff intensity for sky lighting.</param>
        /// <param name="sunsetIntensity">Intensity of sunset.</param>
        /// <param name="sunsetRadialFalloff">Radial falloff of sunset.</param>
        /// <param name="sunsetHorizontalFalloff">Horizontal falloff of sunset.</param>
        /// <param name="sunsetVerticalFalloff">Vertical falloff of sunset.</param>
        /// <param name="enableMoon">Whether or not to enable procedural moon.</param>
        /// <param name="moonDiameter">Diameter of moon.</param>
        /// <param name="moonColor">Color of moon.</param>
        /// <param name="moonFalloffAmount">Falloff amount for moonlight.</param>
        /// <param name="enableStars">Whether or not to enable stars.</param>
        /// <param name="starsBrightness">Brightness for stars.</param>
        /// <param name="starsDaytimeBrightness">Daytime brightness for stars.</param>
        /// <param name="starsHorizonFalloff">Falloff for stars at horizon.</param>
        /// <param name="starsSaturation">Saturation for stars.</param>
        /// <param name="enableProceduralStars">Whether or not to enable procedural stars.</param>
        /// <param name="proceduralStarsSharpness">Sharpness for procedural stars.</param>
        /// <param name="proceduralStarsAmount">Amount of procedural stars.</param>
        /// <param name="enableStarsTexture">Whether or not to enable star texture (overlaid on top
        /// of procedural stars).</param>
        /// <param name="starTextureURI">URI to the texture for non-procedural stars.</param>
        /// <param name="starTint">Tint for non-procedural stars.</param>
        /// <param name="starScale">Scale for non-procedural stars.</param>
        /// <param name="starRotationSpeed">Rotation speed for non-procedural stars.</param>
        /// <param name="enableClouds">Whether or not to enable procedural clouds.</param>
        /// <param name="cloudsTextureURI">URI to texture for clouds.</param>
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
        public static bool SetLiteDayNightSky(LightEntity sunEntity, bool enableGround, WorldTypes.Color groundColor,
            float groundHeight, float groundFadeAmount, float horizonSkyBlend, WorldTypes.Color dayHorizonColor,
            WorldTypes.Color daySkyColor, WorldTypes.Color nightHorizonColor, WorldTypes.Color nightSkyColor,
            float horizonSaturationAmount, float horizonSaturationFalloff, bool enableSun, float sunDiameter,
            WorldTypes.Color sunHorizonColor, WorldTypes.Color sunZenithColor, bool enableSunSkyLighting,
            float skyLightingFalloffAmount, float skyLightingFalloffIntensity, float sunsetIntensity,
            float sunsetRadialFalloff, float sunsetHorizontalFalloff, float sunsetVerticalFalloff, bool enableMoon,
            float moonDiameter, WorldTypes.Color moonColor, float moonFalloffAmount, bool enableStars, float starsBrightness,
            float starsDaytimeBrightness, float starsHorizonFalloff, float starsSaturation, bool enableProceduralStars,
            float proceduralStarsSharpness, float proceduralStarsAmount, bool enableStarsTexture, string starTextureURI,
            WorldTypes.Color starTint, float starScale, float starRotationSpeed, bool enableClouds, string cloudsTextureURI,
            WorldTypes.Vector2 cloudsScale, WorldTypes.Vector2 cloudsSpeed, float cloudiness, float cloudsOpacity, float cloudsSharpness,
            float cloudsShadingIntensity, float cloudsZenithFalloff, int cloudsIterations, float cloudsGain, int cloudsLacunarity,
            WorldTypes.Color cloudsDayColor, WorldTypes.Color cloudsNightColor)
        {
            if (sunEntity == null)
            {
                Logging.LogWarning("[Environment->SetLiteDayNightSky] Invalid sun entity.");
                return false;
            }

            if (sunEntity.internalEntity == null)
            {
                Logging.LogError("[Environment->SetLiteDayNightSky] Invalid sun entity.");
                return false;
            }

            if (groundHeight < -1 || groundHeight > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid ground height. Must be between -1 and 1.");
                return false;
            }

            if (groundFadeAmount < 0 || groundFadeAmount > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid ground fade amount. Must be between 0 and 1.");
                return false;
            }

            if (horizonSkyBlend < 0.1f || horizonSkyBlend > 2)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid horizon sky blend. Must be between 0.1 and 2.");
                return false;
            }

            if (horizonSaturationAmount < 0 || horizonSaturationAmount > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid horizon saturation amount. Must be between 0 and 1.");
                return false;
            }

            if (horizonSaturationFalloff < 1 || horizonSaturationFalloff > 10)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid horizon saturation falloff. Must be between 1 and 10.");
                return false;
            }

            if (sunDiameter < 0)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid sun diameter. Must be at least 0.");
                return false;
            }

            if (skyLightingFalloffAmount < 0)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid sky lighting falloff amount. Must be at least 0.");
                return false;
            }

            if (skyLightingFalloffIntensity < 0)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid sky lighting falloff intensity. Must be at least 0.");
                return false;
            }

            if (sunsetIntensity < 0 || sunsetIntensity > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid sunset intensity. Must be between 0 and 1.");
                return false;
            }

            if (sunsetRadialFalloff < 0.01f || sunsetRadialFalloff > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid sunset radial falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (sunsetHorizontalFalloff < 0.01f || sunsetHorizontalFalloff > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid sunset horizontal falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (sunsetVerticalFalloff < 0.01f || sunsetVerticalFalloff > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid sunset vertical falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (moonDiameter < 0)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid moon diameter. Must be at least 0.");
                return false;
            }

            if (moonFalloffAmount < 0)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid moon falloff amount. Must be at least 0.");
                return false;
            }

            if (starsBrightness < 0 || starsBrightness > 3)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid stars brightness. Must be between 0 and 3.");
                return false;
            }

            if (starsDaytimeBrightness < 0 || starsDaytimeBrightness > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid stars daytime brightness. Must be between 0 and 1.");
                return false;
            }

            if (starsHorizonFalloff < 0 || starsHorizonFalloff > 2)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid stars horizon falloff. Must be between 0 and 2.");
                return false;
            }

            if (starsSaturation < 0)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid stars saturation. Must be at least 0.");
                return false;
            }

            if (cloudiness < 0 || cloudiness > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid cloudiness. Must be between 0 and 1.");
                return false;
            }

            if (cloudsOpacity < 0 || cloudsOpacity > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid clouds opacity. Must be between 0 and 1.");
                return false;
            }

            if (cloudsSharpness < 0 || cloudsSharpness > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid clouds sharpness. Must be between 0 and 1.");
                return false;
            }

            if (cloudsShadingIntensity < 0 || cloudsShadingIntensity > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid clouds shading intensity. Must be between 0 and 1.");
                return  false;
            }

            if (cloudsZenithFalloff < 0 || cloudsZenithFalloff > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid clouds zenith falloff. Must be between 0 and 1.");
                return false;
            }

            if (cloudsIterations < 1 || cloudsIterations > 4)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid clouds iterations. Must be between 1 and 4.");
                return false;
            }

            if (cloudsGain < 0 || cloudsGain > 1)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid clouds gain. Must be between 0 and 1.");
                return false;
            }

            if (cloudsLacunarity < 2 || cloudsLacunarity > 5)
            {
                Debug.LogWarning("[Environment->SetLiteDayNightSky] Invalid clouds lacunarity. Must be between 2 and 5.");
                return false;
            }

            Color convertedGroundColor = new Color(groundColor.r, groundColor.g, groundColor.b, groundColor.a);
            Color convertedDayHorizonColor = new Color(dayHorizonColor.r, dayHorizonColor.g, dayHorizonColor.b, dayHorizonColor.a);
            Color convertedDaySkyColor = new Color(daySkyColor.r, daySkyColor.g, daySkyColor.b, daySkyColor.a);
            Color convertedNightHorizonColor = new Color(nightHorizonColor.r, nightHorizonColor.g, nightHorizonColor.b, nightHorizonColor.a);
            Color convertedNightSkyColor = new Color(nightSkyColor.r, nightSkyColor.g, nightSkyColor.b, nightSkyColor.a);
            Color convertedSunHorizonColor = new Color(sunHorizonColor.r, sunHorizonColor.g, sunHorizonColor.b, sunHorizonColor.a);
            Color convertedZenithColor = new Color(sunZenithColor.r, sunZenithColor.g, sunZenithColor.b, sunZenithColor.a);
            Color convertedMoonColor = new Color(moonColor.r, moonColor.g, moonColor.b, moonColor.a);
            Color convertedStarTint = new Color(starTint.r, starTint.g, starTint.b, starTint.a);
            Color convertedCloudsDayColor = new Color(cloudsDayColor.r, cloudsDayColor.g, cloudsDayColor.b, cloudsDayColor.a);
            Color convertedCloudsNightColor = new Color(cloudsNightColor.r, cloudsNightColor.g, cloudsNightColor.b, cloudsNightColor.a);

            Vector2 convertedCloudsScale = new Vector2(cloudsScale.x, cloudsScale.y);
            Vector2 convertedCloudsSpeed = new Vector2(cloudsSpeed.x, cloudsSpeed.y);

            Action<byte[]> onStarTextureDownloaded = new Action<byte[]>((rawData) =>
            {
                Texture2D starTexture;
                if (rawData == null)
                {
                    starTexture = WebVerseRuntime.Instance.straightFour.defaultStarTexture;
                }
                else
                {
                    starTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                    starTexture.LoadImage(rawData);
                }

                Action<byte[]> onCloudsTextureDownloaded = new Action<byte[]>((rawData) =>
                {
                    Texture2D cloudsTexture;
                    if (rawData == null)
                    {
                        cloudsTexture = WebVerseRuntime.Instance.straightFour.defaultCloudTexture;
                    }
                    else
                    {
                        cloudsTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                        cloudsTexture.LoadImage(rawData);
                    }

                    StraightFour.StraightFour.ActiveWorld.environmentManager.CreateDayNightLiteSky(sunEntity.internalEntity.gameObject,
                        enableGround, convertedGroundColor, groundHeight, groundFadeAmount, horizonSkyBlend, convertedDayHorizonColor,
                        convertedDaySkyColor, convertedNightHorizonColor, convertedNightSkyColor, horizonSaturationAmount,
                        horizonSaturationFalloff, enableSun, sunDiameter, convertedSunHorizonColor, convertedZenithColor,
                        enableSunSkyLighting, skyLightingFalloffAmount, skyLightingFalloffIntensity, sunsetIntensity, sunsetRadialFalloff,
                        sunsetHorizontalFalloff, sunsetVerticalFalloff, enableMoon, moonDiameter, convertedMoonColor, moonFalloffAmount,
                        enableStars, starsBrightness, starsDaytimeBrightness, starsHorizonFalloff, starsSaturation, enableProceduralStars,
                        proceduralStarsSharpness, proceduralStarsAmount, enableStarsTexture, starTexture, convertedStarTint, starScale,
                        starRotationSpeed, enableClouds, cloudsTexture, convertedCloudsScale, convertedCloudsSpeed, cloudiness, cloudsOpacity,
                        cloudsSharpness, cloudsShadingIntensity, cloudsZenithFalloff, cloudsIterations, cloudsGain, cloudsLacunarity,
                        convertedCloudsDayColor, convertedCloudsNightColor);
                });

                if (string.IsNullOrEmpty(cloudsTextureURI))
                {
                    onCloudsTextureDownloaded.Invoke(null);
                }
                else
                {
                    WebVerseRuntime.Instance.vemlHandler.DownloadFileWithoutCache(
                        VEML.VEMLUtilities.FullyQualifyURI(cloudsTextureURI, WebVerseRuntime.Instance.currentBasePath),
                            onCloudsTextureDownloaded);
                }
            });
            if (string.IsNullOrEmpty(starTextureURI))
            {
                onStarTextureDownloaded.Invoke(null);
            }
            else
            {
                WebVerseRuntime.Instance.vemlHandler.DownloadFileWithoutCache(
                    VEML.VEMLUtilities.FullyQualifyURI(starTextureURI, WebVerseRuntime.Instance.currentBasePath), onStarTextureDownloaded);
            }
            
            return true;

        }

        /// <summary>
        /// Set the sky to lite-mode constant color procedural.
        /// </summary>
        /// <param name="sunEntity">Entity to use for the sun.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetLiteConstantColorSky(LightEntity sunEntity)
        {
            return SetLiteConstantColorSky(sunEntity, true, new WorldTypes.Color(0.0275f, 0.0275f, 0.0275f, 0), -0.02f,
                0.02f, 0.579f, new WorldTypes.Color(0.2667f, 0.5451f, 0.7725f, 1),
                new WorldTypes.Color(0.0314f, 0.0706f, 0.2549f, 0), 0.894f, 4.45f, true, 2,
                new WorldTypes.Color(155, 72, 33, 0), new WorldTypes.Color(158, 72, 33, 0), true, 10, 18, 0.039f, 0.1f,
                0.6f, 0.5f, true, 2, new WorldTypes.Color(0.6706f, 0.7255f, 0.7490f, 0), 60, true, 0.85f, 0.1f, 0, 0.3f,
                true, 1, 1, true, null, WorldTypes.Color.white, 0.5f, 0.25f, true, null, new WorldTypes.Vector2(2, 1),
                new WorldTypes.Vector2(0.2f, 0.3f), 0.279f, 1, 0.8f, 0.8f, 0.31f, 3, 0.5f, 2,
                new WorldTypes.Color(0.8431f, 0.8667f, 0.9059f, 1));
        }

        /// <summary>
        /// Set the sky to lite-mode constant color procedural.
        /// </summary>
        /// <param name="sunEntity">Entity to use for the sun.</param>
        /// <param name="enableGround">Whether or not to enable procedural ground.</param>
        /// <param name="groundColor">Color for the ground.</param>
        /// <param name="groundHeight">Height to place ground at.</param>
        /// <param name="horizonColor">Color for horizon during day.</param>
        /// <param name="skyColor">Color for sky during day.</param>
        /// <param name="enableSun">Whether or not to enable procedural sun.</param>
        /// <param name="sunDiameter">Diameter of sun.</param>
        /// <param name="sunColor">Color of sun.</param>
        /// <param name="enableMoon">Whether or not to enable procedural moon.</param>
        /// <param name="moonDiameter">Diameter of moon.</param>
        /// <param name="moonColor">Color of moon.</param>
        /// <param name="enableStars">Whether or not to enable stars.</param>
        /// <param name="starsBrightness">Brightness for stars.</param>
        /// <param name="starRotationSpeed">Rotation speed for non-procedural stars.</param>
        /// <param name="enableClouds">Whether or not to enable procedural clouds.</param>
        /// <param name="cloudsSpeed">Speed for clouds.</param>
        /// <param name="cloudiness">Cloudiness level.</param>
        /// <param name="cloudsOpacity">Cloud opacity level.</param>
        /// <param name="cloudsColor">Color for clouds.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetLiteConstantColorSky(LightEntity sunEntity, bool enableGround, WorldTypes.Color groundColor,
            float groundHeight, WorldTypes.Color horizonColor, WorldTypes.Color skyColor, bool enableSun, float sunDiameter,
            WorldTypes.Color sunColor, bool enableMoon, float moonDiameter, WorldTypes.Color moonColor, bool enableStars,
            float starsBrightness, float starRotationSpeed, bool enableClouds, WorldTypes.Vector2 cloudsSpeed, float cloudiness,
            float cloudsOpacity, WorldTypes.Color cloudsColor)
        {
            return SetLiteConstantColorSky(sunEntity, enableGround, groundColor, groundHeight, 0.02f, 0.579f,
                horizonColor, skyColor, 0.894f, 4.45f, enableSun, sunDiameter, sunColor, sunColor, true, 10, 18, 0.039f, 0.1f, 0.6f,
                0.5f, enableMoon, moonDiameter, moonColor, 60, enableStars, starsBrightness, 0.1f, 0, 0.3f, true, 1, 1, true, null,
                WorldTypes.Color.white, 0.5f, starRotationSpeed, enableClouds, null, new WorldTypes.Vector2(2, 1), cloudsSpeed,
                cloudiness, cloudsOpacity, 0.8f, 0.8f, 0.31f, 3, 0.5f, 2, cloudsColor);
        }

        /// <summary>
        /// Set the sky to lite-mode constant color procedural.
        /// </summary>
        /// <param name="sunEntity">Entity to use for the sun.</param>
        /// <param name="enableGround">Whether or not to enable procedural ground.</param>
        /// <param name="groundColor">Color for the ground.</param>
        /// <param name="groundHeight">Height to place ground at.</param>
        /// <param name="horizonColor">Color for horizon during day.</param>
        /// <param name="skyColor">Color for sky during day.</param>
        /// <param name="enableSun">Whether or not to enable procedural sun.</param>
        /// <param name="sunDiameter">Diameter of sun.</param>
        /// <param name="sunColor">Color of sun.</param>
        /// <param name="enableMoon">Whether or not to enable procedural moon.</param>
        /// <param name="moonDiameter">Diameter of moon.</param>
        /// <param name="moonColor">Color of moon.</param>
        /// <param name="enableStars">Whether or not to enable stars.</param>
        /// <param name="starsBrightness">Brightness for stars.</param>
        /// <param name="starTextureURI">URI to the texture for non-procedural stars.</param>
        /// <param name="starTint">Tint for non-procedural stars.</param>
        /// <param name="starScale">Scale for non-procedural stars.</param>
        /// <param name="starRotationSpeed">Rotation speed for non-procedural stars.</param>
        /// <param name="enableClouds">Whether or not to enable procedural clouds.</param>
        /// <param name="cloudsTextureURI">URI to texture for clouds.</param>
        /// <param name="cloudsScale">Scale for clouds.</param>
        /// <param name="cloudsSpeed">Speed for clouds.</param>
        /// <param name="cloudiness">Cloudiness level.</param>
        /// <param name="cloudsOpacity">Cloud opacity level.</param>
        /// <param name="cloudsColor">Color for clouds.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetLiteConstantColorSky(LightEntity sunEntity, bool enableGround, WorldTypes.Color groundColor,
            float groundHeight, WorldTypes.Color horizonColor, WorldTypes.Color skyColor, bool enableSun, float sunDiameter,
            WorldTypes.Color sunColor, bool enableMoon, float moonDiameter, WorldTypes.Color moonColor,
            bool enableStars, float starsBrightness, string starTextureURI, WorldTypes.Color starTint, float starScale,
            float starRotationSpeed, bool enableClouds, string cloudsTextureURI, WorldTypes.Vector2 cloudsScale,
            WorldTypes.Vector2 cloudsSpeed, float cloudiness, float cloudsOpacity, WorldTypes.Color cloudsColor)
        {
            return SetLiteConstantColorSky(sunEntity, enableGround, groundColor, groundHeight, 0.02f, 0.579f,
                horizonColor, skyColor, 0.894f, 4.45f, enableSun, sunDiameter, sunColor, sunColor, true, 10, 18, 0.039f, 0.1f, 0.6f,
                0.5f, enableMoon, moonDiameter, moonColor, 60, enableStars, starsBrightness, 0.1f, 0, 0.3f, true, 1, 1, true,
                starTextureURI, starTint, starScale, starRotationSpeed, enableClouds, cloudsTextureURI, cloudsScale, cloudsSpeed,
                cloudiness, cloudsOpacity, 0.8f, 0.8f, 0.31f, 3, 0.5f, 2, cloudsColor);
        }

        /// <summary>
        /// Set the sky to lite-mode constant color procedural.
        /// </summary>
        /// <param name="sunEntity">Entity to use for the sun.</param>
        /// <param name="enableGround">Whether or not to enable procedural ground.</param>
        /// <param name="groundColor">Color for the ground.</param>
        /// <param name="groundHeight">Height to place ground at.</param>
        /// <param name="groundFadeAmount">Factor for fade between ground and horizon.</param>
        /// <param name="horizonSkyBlend">Blending factor between horizon and sky.</param>
        /// <param name="horizonColor">Color for horizon during day.</param>
        /// <param name="skyColor">Color for sky during day.</param>
        /// <param name="horizonSaturationAmount">Saturation amount for horizon.</param>
        /// <param name="horizonSaturationFalloff">Saturation falloff for horizon.</param>
        /// <param name="enableSun">Whether or not to enable procedural sun.</param>
        /// <param name="sunDiameter">Diameter of sun.</param>
        /// <param name="sunHorizonColor">Color of sun at horizon.</param>
        /// <param name="sunZenithColor">Color of sun at zenith.</param>
        /// <param name="enableSunSkyLighting">Whether or not to enable lighting of sky from sun.</param>
        /// <param name="skyLightingFalloffAmount">Falloff amount for sky lighting.</param>
        /// <param name="skyLightingFalloffIntensity">Falloff intensity for sky lighting.</param>
        /// <param name="sunsetIntensity">Intensity of sunset.</param>
        /// <param name="sunsetRadialFalloff">Radial falloff of sunset.</param>
        /// <param name="sunsetHorizontalFalloff">Horizontal falloff of sunset.</param>
        /// <param name="sunsetVerticalFalloff">Vertical falloff of sunset.</param>
        /// <param name="enableMoon">Whether or not to enable procedural moon.</param>
        /// <param name="moonDiameter">Diameter of moon.</param>
        /// <param name="moonColor">Color of moon.</param>
        /// <param name="moonFalloffAmount">Falloff amount for moonlight.</param>
        /// <param name="enableStars">Whether or not to enable stars.</param>
        /// <param name="starsBrightness">Brightness for stars.</param>
        /// <param name="starsDaytimeBrightness">Daytime brightness for stars.</param>
        /// <param name="starsHorizonFalloff">Falloff for stars at horizon.</param>
        /// <param name="starsSaturation">Saturation for stars.</param>
        /// <param name="enableProceduralStars">Whether or not to enable procedural stars.</param>
        /// <param name="proceduralStarsSharpness">Sharpness for procedural stars.</param>
        /// <param name="proceduralStarsAmount">Amount of procedural stars.</param>
        /// <param name="enableStarsTexture">Whether or not to enable star texture (overlaid on top
        /// of procedural stars).</param>
        /// <param name="starTextureURI">URI to the texture for non-procedural stars.</param>
        /// <param name="starTint">Tint for non-procedural stars.</param>
        /// <param name="starScale">Scale for non-procedural stars.</param>
        /// <param name="starRotationSpeed">Rotation speed for non-procedural stars.</param>
        /// <param name="enableClouds">Whether or not to enable procedural clouds.</param>
        /// <param name="cloudsTextureURI">URI to texture for clouds.</param>
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
        /// <param name="cloudsColor">Color for clouds.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetLiteConstantColorSky(LightEntity sunEntity, bool enableGround, WorldTypes.Color groundColor,
            float groundHeight, float groundFadeAmount, float horizonSkyBlend, WorldTypes.Color horizonColor, WorldTypes.Color skyColor,
            float horizonSaturationAmount, float horizonSaturationFalloff, bool enableSun, float sunDiameter, WorldTypes.Color sunHorizonColor,
            WorldTypes.Color sunZenithColor, bool enableSunSkyLighting, float skyLightingFalloffAmount, float skyLightingFalloffIntensity,
            float sunsetIntensity, float sunsetRadialFalloff, float sunsetHorizontalFalloff, float sunsetVerticalFalloff, bool enableMoon,
            float moonDiameter, WorldTypes.Color moonColor, float moonFalloffAmount, bool enableStars, float starsBrightness,
            float starsDaytimeBrightness, float starsHorizonFalloff, float starsSaturation, bool enableProceduralStars,
            float proceduralStarsSharpness, float proceduralStarsAmount, bool enableStarsTexture, string starTextureURI, WorldTypes.Color starTint,
            float starScale, float starRotationSpeed, bool enableClouds, string cloudsTextureURI, WorldTypes.Vector2 cloudsScale,
            WorldTypes.Vector2 cloudsSpeed, float cloudiness, float cloudsOpacity, float cloudsSharpness, float cloudsShadingIntensity,
            float cloudsZenithFalloff, int cloudsIterations, float cloudsGain, int cloudsLacunarity, WorldTypes.Color cloudsColor)
        {
            if (sunEntity == null)
            {
                Logging.LogWarning("[Environment->SetLiteConstantColorSky] Invalid sun entity.");
                return false;
            }

            if (sunEntity.internalEntity == null)
            {
                Logging.LogError("[Environment->SetLiteConstantColorSky] Invalid sun entity.");
                return false;
            }

            if (groundHeight < -1 || groundHeight > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid ground height. Must be between -1 and 1.");
                return false;
            }

            if (groundFadeAmount < 0 || groundFadeAmount > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid ground fade amount. Must be between 0 and 1.");
                return false;
            }

            if (horizonSkyBlend < 0.1f || horizonSkyBlend > 2)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid horizon sky blend. Must be between 0.1 and 2.");
                return false;
            }

            if (horizonSaturationAmount < 0 || horizonSaturationAmount > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid horizon saturation amount. Must be between 0 and 1.");
                return false;
            }

            if (horizonSaturationFalloff < 1 || horizonSaturationFalloff > 10)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid horizon saturation falloff. Must be between 1 and 10.");
                return false;
            }

            if (sunDiameter < 0)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid sun diameter. Must be at least 0.");
                return false;
            }

            if (skyLightingFalloffAmount < 0)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid sky lighting falloff amount. Must be at least 0.");
                return false;
            }

            if (skyLightingFalloffIntensity < 0)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid sky lighting falloff intensity. Must be at least 0.");
                return false;
            }

            if (sunsetIntensity < 0 || sunsetIntensity > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid sunset intensity. Must be between 0 and 1.");
                return false;
            }

            if (sunsetRadialFalloff < 0.01f || sunsetRadialFalloff > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid sunset radial falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (sunsetHorizontalFalloff < 0.01f || sunsetHorizontalFalloff > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid sunset horizontal falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (sunsetVerticalFalloff < 0.01f || sunsetVerticalFalloff > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid sunset vertical falloff. Must be between 0.01 and 1.");
                return false;
            }

            if (moonDiameter < 0)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid moon diameter. Must be at least 0.");
                return false;
            }

            if (moonFalloffAmount < 0)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid moon falloff amount. Must be at least 0.");
                return false;
            }

            if (starsBrightness < 0 || starsBrightness > 3)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid stars brightness. Must be between 0 and 3.");
                return false;
            }

            if (starsDaytimeBrightness < 0 || starsDaytimeBrightness > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid stars daytime brightness. Must be between 0 and 1.");
                return false;
            }

            if (starsHorizonFalloff < 0 || starsHorizonFalloff > 2)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid stars horizon falloff. Must be between 0 and 2.");
                return false;
            }

            if (starsSaturation < 0)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid stars saturation. Must be at least 0.");
                return false;
            }

            if (cloudiness < 0 || cloudiness > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid cloudiness. Must be between 0 and 1.");
                return false;
            }

            if (cloudsOpacity < 0 || cloudsOpacity > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid clouds opacity. Must be between 0 and 1.");
                return false;
            }

            if (cloudsSharpness < 0 || cloudsSharpness > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid clouds sharpness. Must be between 0 and 1.");
                return false;
            }

            if (cloudsShadingIntensity < 0 || cloudsShadingIntensity > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid clouds shading intensity. Must be between 0 and 1.");
                return  false;
            }

            if (cloudsZenithFalloff < 0 || cloudsZenithFalloff > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid clouds zenith falloff. Must be between 0 and 1.");
                return false;
            }

            if (cloudsIterations < 1 || cloudsIterations > 4)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid clouds iterations. Must be between 1 and 4.");
                return false;
            }

            if (cloudsGain < 0 || cloudsGain > 1)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid clouds gain. Must be between 0 and 1.");
                return false;
            }

            if (cloudsLacunarity < 2 || cloudsLacunarity > 5)
            {
                Debug.LogWarning("[Environment->SetLiteConstantColorSky] Invalid clouds lacunarity. Must be between 2 and 5.");
                return false;
            }

            Color convertedGroundColor = new Color(groundColor.r, groundColor.g, groundColor.b, groundColor.a);
            Color convertedHorizonColor = new Color(horizonColor.r, horizonColor.g, horizonColor.b, horizonColor.a);
            Color convertedSkyColor = new Color(skyColor.r, skyColor.g, skyColor.b, skyColor.a);
            Color convertedSunHorizonColor = new Color(sunHorizonColor.r, sunHorizonColor.g, sunHorizonColor.b, sunHorizonColor.a);
            Color convertedZenithColor = new Color(sunZenithColor.r, sunZenithColor.g, sunZenithColor.b, sunZenithColor.a);
            Color convertedMoonColor = new Color(moonColor.r, moonColor.g, moonColor.b, moonColor.a);
            Color convertedStarTint = new Color(starTint.r, starTint.g, starTint.b, starTint.a);
            Color convertedCloudsColor = new Color(cloudsColor.r, cloudsColor.g, cloudsColor.b, cloudsColor.a);

            Vector2 convertedCloudsScale = new Vector2(cloudsScale.x, cloudsScale.y);
            Vector2 convertedCloudsSpeed = new Vector2(cloudsSpeed.x, cloudsSpeed.y);

            Action<byte[]> onStarTextureDownloaded = new Action<byte[]>((rawData) =>
            {
                Texture2D starTexture;
                if (rawData == null)
                {
                    starTexture = WebVerseRuntime.Instance.straightFour.defaultStarTexture;
                }
                else
                {
                    starTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                    starTexture.LoadImage(rawData);
                }

                Action<byte[]> onCloudsTextureDownloaded = new Action<byte[]>((rawData) =>
                {
                    Texture2D cloudsTexture;
                    if (rawData == null)
                    {
                        cloudsTexture = WebVerseRuntime.Instance.straightFour.defaultCloudTexture;
                    }
                    else
                    {
                        cloudsTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                        cloudsTexture.LoadImage(rawData);
                    }

                    StraightFour.StraightFour.ActiveWorld.environmentManager.CreateConstantColorLiteSky(sunEntity.internalEntity.gameObject,
                        enableGround, convertedGroundColor, groundHeight, groundFadeAmount, horizonSkyBlend, convertedHorizonColor,
                        convertedSkyColor, horizonSaturationAmount, horizonSaturationFalloff, enableSun, sunDiameter,
                        convertedSunHorizonColor, convertedZenithColor, enableSunSkyLighting, skyLightingFalloffAmount,
                        skyLightingFalloffIntensity, sunsetIntensity, sunsetRadialFalloff, sunsetHorizontalFalloff, sunsetVerticalFalloff,
                        enableMoon, moonDiameter, convertedMoonColor, moonFalloffAmount, enableStars, starsBrightness,
                        starsDaytimeBrightness, starsHorizonFalloff, starsSaturation, enableProceduralStars, proceduralStarsSharpness,
                        proceduralStarsAmount, enableStarsTexture, starTexture, convertedStarTint, starScale, starRotationSpeed,
                        enableClouds, cloudsTexture, convertedCloudsScale, convertedCloudsSpeed, cloudiness, cloudsOpacity,
                        cloudsSharpness, cloudsShadingIntensity, cloudsZenithFalloff, cloudsIterations, cloudsGain, cloudsLacunarity,
                        convertedCloudsColor);
                    });
                    if (string.IsNullOrEmpty(cloudsTextureURI))
                    {
                        onCloudsTextureDownloaded.Invoke(null);
                    }
                    else
                    {
                        WebVerseRuntime.Instance.vemlHandler.DownloadFileWithoutCache(
                            VEML.VEMLUtilities.FullyQualifyURI(cloudsTextureURI, WebVerseRuntime.Instance.currentBasePath),
                                onCloudsTextureDownloaded);
                    }
            });
            if (string.IsNullOrEmpty(starTextureURI))
            {
                onStarTextureDownloaded.Invoke(null);
            }
            else
            {
                WebVerseRuntime.Instance.vemlHandler.DownloadFileWithoutCache(
                    VEML.VEMLUtilities.FullyQualifyURI(starTextureURI, WebVerseRuntime.Instance.currentBasePath), onStarTextureDownloaded);
            }
            
            return true;
        }

        /// <summary>
        /// Activate light-mode fog.
        /// </summary>
        /// <param name="color">Color of the fog.</param>
        /// <param name="density">Density of the fog.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool ActivateLiteFog(WorldTypes.Color color, float density)
        {
            Color convertedColor = new Color(color.r, color.g, color.b, color.a);
            return StraightFour.StraightFour.ActiveWorld.environmentManager.ActivateLiteFog(convertedColor, density);
        }

        /// <summary>
        /// Disable all fog.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool DisableFog()
        {
            return StraightFour.StraightFour.ActiveWorld.environmentManager.DisableFog();
        }
    }
}