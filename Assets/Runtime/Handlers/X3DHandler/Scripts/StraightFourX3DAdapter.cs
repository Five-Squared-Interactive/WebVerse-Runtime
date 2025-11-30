// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using FiveSQD.StraightFour;
using FiveSQD.StraightFour.Entity;
using FiveSQD.StraightFour.Utilities;
using FiveSQD.WebVerse.X3D.Abstraction;
using UnityEngine;
using StraightFourEngine = FiveSQD.StraightFour.StraightFour;

namespace FiveSQD.WebVerse.Handlers.X3D
{
    /// <summary>
    /// Adapter that connects the X3D library to the StraightFour world engine.
    /// This implements the IX3DWorldAdapter interface, allowing X3D to create
    /// entities through StraightFour without direct coupling.
    /// </summary>
    public class StraightFourX3DAdapter : MonoBehaviour, IX3DWorldAdapter
    {
        /// <summary>
        /// Reference to the active world's entity manager.
        /// </summary>
        private EntityManager EntityManager => StraightFourEngine.ActiveWorld?.entityManager;

        /// <summary>
        /// Reference to the active world's environment manager.
        /// </summary>
        private FiveSQD.StraightFour.Environment.EnvironmentManager EnvironmentManager => 
            StraightFourEngine.ActiveWorld?.environmentManager;

        /// <summary>
        /// Reference to the active world's camera manager.
        /// </summary>
        private FiveSQD.StraightFour.Camera.CameraManager CameraManager => 
            StraightFourEngine.ActiveWorld?.cameraManager;

        /// <summary>
        /// Map from X3D entity handles to StraightFour entities.
        /// </summary>
        private Dictionary<Guid, BaseEntity> entityMap = new Dictionary<Guid, BaseEntity>();

        /// <summary>
        /// Map from DEF tags to entity handles.
        /// </summary>
        private Dictionary<string, X3DEntityHandle> tagToEntityMap = new Dictionary<string, X3DEntityHandle>();

        /// <summary>
        /// Default shader for materials.
        /// </summary>
        private Shader defaultShader;

        /// <summary>
        /// Initialize the adapter.
        /// </summary>
        public void Initialize()
        {
            entityMap.Clear();
            tagToEntityMap.Clear();

            // Find the URP Lit shader or fallback to Standard
            defaultShader = Shader.Find("Universal Render Pipeline/Lit");
            if (defaultShader == null)
            {
                defaultShader = Shader.Find("Standard");
            }

            LogSystem.Log("[StraightFourX3DAdapter] Initialized.");
        }

        /// <summary>
        /// Clean up the adapter.
        /// </summary>
        public void Cleanup()
        {
            entityMap.Clear();
            tagToEntityMap.Clear();
        }

        #region Entity Creation

        public X3DEntityHandle CreateContainerEntity(
            X3DEntityHandle parent,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null)
        {
            if (EntityManager == null)
            {
                LogSystem.LogError("[StraightFourX3DAdapter] No active world.");
                return null;
            }

            BaseEntity parentEntity = GetBaseEntity(parent);

            X3DEntityHandle handle = new X3DEntityHandle
            {
                Id = Guid.NewGuid(),
                Tag = tag,
                Success = false
            };

            Action onLoaded = () =>
            {
                handle.Success = true;
                BaseEntity entity = EntityManager.FindEntity(handle.Id);
                if (entity != null)
                {
                    entityMap[handle.Id] = entity;
                    entity.SetScale(scale, false);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        tagToEntityMap[tag] = handle;
                    }
                }
                onCreated?.Invoke(handle);
            };

            EntityManager.LoadContainerEntity(
                parentEntity,
                position,
                rotation,
                scale,
                handle.Id,
                tag,
                false,
                onLoaded
            );

            return handle;
        }

        public X3DEntityHandle CreateMeshEntity(
            X3DEntityHandle parent,
            Mesh mesh,
            Material material,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null)
        {
            if (EntityManager == null)
            {
                LogSystem.LogError("[StraightFourX3DAdapter] No active world.");
                return null;
            }

            BaseEntity parentEntity = GetBaseEntity(parent);

            // Create a temporary GameObject as a prefab for the mesh entity
            GameObject meshPrefab = new GameObject("X3DMesh_Prefab");
            MeshFilter meshFilter = meshPrefab.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = meshPrefab.AddComponent<MeshRenderer>();
            meshRenderer.material = material ?? CreateDefaultMaterial();

            X3DEntityHandle handle = new X3DEntityHandle
            {
                Id = Guid.NewGuid(),
                Tag = tag,
                Success = false
            };

            Action onLoaded = () =>
            {
                handle.Success = true;
                BaseEntity entity = EntityManager.FindEntity(handle.Id);
                if (entity != null)
                {
                    entityMap[handle.Id] = entity;
                    entity.SetScale(scale, false);
                    entity.SetVisibility(true, false);
                    
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tagToEntityMap[tag] = handle;
                    }
                }

                // Clean up the temporary prefab
                Destroy(meshPrefab);

                onCreated?.Invoke(handle);
            };

            EntityManager.LoadMeshEntity(
                parentEntity,
                meshPrefab,
                position,
                rotation,
                handle.Id,
                tag,
                onLoaded
            );

            return handle;
        }

        public X3DEntityHandle CreateLightEntity(
            X3DEntityHandle parent,
            X3DLightType lightType,
            Color color,
            float intensity,
            Vector3 position,
            Quaternion rotation,
            float range = 10f,
            float spotAngle = 45f,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null)
        {
            if (EntityManager == null)
            {
                LogSystem.LogError("[StraightFourX3DAdapter] No active world.");
                return null;
            }

            BaseEntity parentEntity = GetBaseEntity(parent);

            X3DEntityHandle handle = new X3DEntityHandle
            {
                Id = Guid.NewGuid(),
                Tag = tag,
                Success = false
            };

            Action onLoaded = () =>
            {
                handle.Success = true;
                BaseEntity entity = EntityManager.FindEntity(handle.Id);
                if (entity != null)
                {
                    entityMap[handle.Id] = entity;

                    // Configure light properties
                    Light lightComponent = entity.GetComponentInChildren<Light>();
                    if (lightComponent != null)
                    {
                        lightComponent.type = ConvertLightType(lightType);
                        lightComponent.color = color;
                        lightComponent.intensity = intensity;
                        lightComponent.range = range;
                        if (lightType == X3DLightType.Spot)
                        {
                            lightComponent.spotAngle = spotAngle;
                        }
                    }

                    if (!string.IsNullOrEmpty(tag))
                    {
                        tagToEntityMap[tag] = handle;
                    }
                }
                onCreated?.Invoke(handle);
            };

            EntityManager.LoadLightEntity(
                parentEntity,
                position,
                rotation,
                handle.Id,
                tag,
                onLoaded
            );

            return handle;
        }

        public X3DEntityHandle CreateTerrainEntity(
            X3DEntityHandle parent,
            float[,] heights,
            float width,
            float length,
            float height,
            Vector3 position,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null)
        {
            if (EntityManager == null)
            {
                LogSystem.LogError("[StraightFourX3DAdapter] No active world.");
                return null;
            }

            BaseEntity parentEntity = GetBaseEntity(parent);

            X3DEntityHandle handle = new X3DEntityHandle
            {
                Id = Guid.NewGuid(),
                Tag = tag,
                Success = false
            };

            Action onLoaded = () =>
            {
                handle.Success = true;
                BaseEntity entity = EntityManager.FindEntity(handle.Id);
                if (entity != null)
                {
                    entityMap[handle.Id] = entity;

                    if (!string.IsNullOrEmpty(tag))
                    {
                        tagToEntityMap[tag] = handle;
                    }
                }
                onCreated?.Invoke(handle);
            };

            EntityManager.LoadTerrainEntity(
                length,
                width,
                height,
                heights,
                null, // layers
                null, // layerMasks
                parentEntity,
                position,
                Quaternion.identity,
                false, // stitchTerrain
                handle.Id,
                tag,
                onLoaded
            );

            return handle;
        }

        public X3DEntityHandle CreateAudioEntity(
            X3DEntityHandle parent,
            Vector3 position,
            Vector3 direction,
            float minDistance,
            float maxDistance,
            float volume,
            bool spatialize,
            string tag = null,
            Action<X3DEntityHandle> onCreated = null)
        {
            if (EntityManager == null)
            {
                LogSystem.LogError("[StraightFourX3DAdapter] No active world.");
                return null;
            }

            BaseEntity parentEntity = GetBaseEntity(parent);

            X3DEntityHandle handle = new X3DEntityHandle
            {
                Id = Guid.NewGuid(),
                Tag = tag,
                Success = false
            };

            Action onLoaded = () =>
            {
                handle.Success = true;
                BaseEntity entity = EntityManager.FindEntity(handle.Id);
                if (entity != null)
                {
                    entityMap[handle.Id] = entity;

                    // Configure audio source
                    AudioSource audioSource = entity.GetComponentInChildren<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.minDistance = minDistance;
                        audioSource.maxDistance = maxDistance;
                        audioSource.volume = volume;
                        audioSource.spatialize = spatialize;
                        audioSource.spatialBlend = spatialize ? 1f : 0f;
                    }

                    if (!string.IsNullOrEmpty(tag))
                    {
                        tagToEntityMap[tag] = handle;
                    }
                }
                onCreated?.Invoke(handle);
            };

            EntityManager.LoadAudioEntity(
                parentEntity,
                position,
                Quaternion.LookRotation(direction),
                handle.Id,
                tag,
                onLoaded
            );

            return handle;
        }

        #endregion

        #region Environment

        public void SetBackground(X3DBackgroundSettings settings)
        {
            if (EnvironmentManager == null)
            {
                LogSystem.LogWarning("[StraightFourX3DAdapter] No active world for background.");
                return;
            }

            // X3D Background defines gradients from zenith (skyColors) and nadir (groundColors)
            // skyColors[0] is at zenith, skyColors[last] is at horizon
            // groundColors[0] is at nadir, groundColors[last] is at horizon
            
            Color zenithSkyColor = settings.PrimarySkyColor;
            Color horizonSkyColor = settings.HorizonSkyColor;
            Color horizonGroundColor = settings.PrimaryGroundColor;
            
            // Find or create a sun object for the sky system
            GameObject sunObject = FindOrCreateSunObject();
            if (sunObject == null)
            {
                // Fallback to solid color sky if no sun available
                EnvironmentManager.SetSolidColorSky(horizonSkyColor);
                return;
            }

            // Determine if we have ground colors
            bool hasGround = settings.GroundColors != null && settings.GroundColors.Length > 0;
            
            // Use CreateConstantColorLiteSky with X3D colors mapped to the appropriate parameters
            // daySkyColor = zenith sky color
            // dayHorizonColor = horizon sky color
            // groundColor = horizon ground color
            EnvironmentManager.CreateConstantColorLiteSky(
                sunObject: sunObject,
                groundEnabled: hasGround,
                groundColor: horizonGroundColor,
                groundHeight: 0f,           // Ground at horizon
                groundFadeAmount: 0f,       // No fade - sharp transition
                horizonSkyBlend: 0.1f,      // Minimal blend for X3D-style hard gradient
                dayHorizonColor: horizonSkyColor,
                daySkyColor: zenithSkyColor,
                horizonSaturationAmount: 0f,
                horizonSaturationFalloff: 10f, // High falloff for crisp colors
                sunEnabled: false,          // No sun disc
                sunDiameter: 0f,
                sunHorizonColor: Color.white,
                sunZenithColor: Color.white,
                sunSkyLightingEnabled: false,
                skyLightingFalloffAmount: 1f,
                skyLightingFalloffIntensity: 1f,
                sunsetIntensity: 0f,
                sunsetRadialFalloff: 1f,
                sunsetHorizontalFalloff: 1f,
                sunsetVerticalFalloff: 1f,
                moonEnabled: false,
                moonDiameter: 0f,
                moonColor: Color.white,
                moonFalloffAmount: 1f,
                starsEnabled: false,
                starsBrightness: 0f,
                starsDaytimeBrightness: 0f,
                starsHorizonFalloff: 1f,
                starsSaturation: 1f,
                proceduralStarsEnabled: false,
                proceduralStarsSharpness: 1f,
                proceduralStarsAmount: 1f,
                starTextureEnabled: false,
                starTexture: null,
                starTint: Color.white,
                starScale: 1f,
                starRotationSpeed: 0f,
                cloudsEnabled: false,
                cloudsTexture: null,
                cloudsScale: Vector2.one,
                cloudsSpeed: Vector2.zero,
                cloudiness: 0f,
                cloudsOpacity: 0f,
                cloudsSharpness: 1f,
                cloudsShadingIntensity: 0f,
                cloudsZenithFalloff: 1f,
                cloudsIterations: 3,
                cloudsGain: 0.5f,
                cloudsLacunarity: 2,
                cloudsDayColor: Color.white
            );

            // TODO: Handle skybox textures if provided (frontUrl, backUrl, etc.)
        }

        /// <summary>
        /// Find an existing directional light to use as sun, or create one if needed.
        /// </summary>
        private GameObject FindOrCreateSunObject()
        {
            // First, try to find an existing directional light in the scene
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    return light.gameObject;
                }
            }

            // If no directional light exists, create a temporary one
            GameObject sunGO = new GameObject("X3D_TempSun");
            Light sunLight = sunGO.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = 1f;
            sunLight.color = Color.white;
            sunGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f); // Default sun angle
            
            return sunGO;
        }

        public void SetFog(X3DFogSettings settings)
        {
            if (EnvironmentManager == null)
            {
                LogSystem.LogWarning("[StraightFourX3DAdapter] No active world for fog.");
                return;
            }

            if (settings.Enabled && settings.VisibilityRange > 0)
            {
                // Convert visibility range to density (inverse relationship)
                // X3D uses visibility range in meters, Unity fog uses density
                float density = Mathf.Clamp01(1f / settings.VisibilityRange * 10f);
                EnvironmentManager.ActivateLiteFog(settings.Color, density);
            }
            else
            {
                EnvironmentManager.DisableFog();
            }
        }

        public void SetAmbientLight(Color color, float intensity)
        {
            if (EnvironmentManager == null)
            {
                LogSystem.LogWarning("[StraightFourX3DAdapter] No active world for ambient light.");
                return;
            }

            // Set ambient light directly through RenderSettings
            RenderSettings.ambientLight = color * intensity;
        }

        #endregion

        #region Camera/Viewpoint

        public void SetViewpoint(Vector3 position, Quaternion rotation, float fieldOfView, string description = null)
        {
            if (CameraManager == null)
            {
                LogSystem.LogWarning("[StraightFourX3DAdapter] No active world for viewpoint.");
                return;
            }

            CameraManager.SetPosition(position, false);
            CameraManager.SetRotation(rotation, false);

            // Convert X3D FOV (radians) to Unity FOV (degrees) and set directly on camera
            float fovDegrees = fieldOfView * Mathf.Rad2Deg;
            if (CameraManager.cam != null)
            {
                CameraManager.cam.fieldOfView = fovDegrees;
            }

            if (!string.IsNullOrEmpty(description))
            {
                LogSystem.Log($"[StraightFourX3DAdapter] Viewpoint: {description}");
            }
        }

        #endregion

        #region Entity Manipulation

        public void SetEntityParent(X3DEntityHandle entity, X3DEntityHandle newParent)
        {
            if (entity == null) return;

            BaseEntity baseEntity = GetBaseEntity(entity);
            BaseEntity parentEntity = GetBaseEntity(newParent);

            if (baseEntity != null)
            {
                baseEntity.SetParent(parentEntity);
            }
        }

        public void SetEntityTransform(X3DEntityHandle entity, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (entity == null) return;

            BaseEntity baseEntity = GetBaseEntity(entity);
            if (baseEntity != null)
            {
                baseEntity.SetPosition(position, true, false);
                baseEntity.SetRotation(rotation, true, false);
                baseEntity.SetScale(scale, false);
            }
        }

        public void SetEntityVisibility(X3DEntityHandle entity, bool visible)
        {
            if (entity == null) return;

            BaseEntity baseEntity = GetBaseEntity(entity);
            if (baseEntity != null)
            {
                baseEntity.SetVisibility(visible, false);
            }
        }

        public void DeleteEntity(X3DEntityHandle entity)
        {
            if (entity == null) return;

            BaseEntity baseEntity = GetBaseEntity(entity);
            if (baseEntity != null)
            {
                baseEntity.Delete(false);
                entityMap.Remove(entity.Id);

                if (!string.IsNullOrEmpty(entity.Tag))
                {
                    tagToEntityMap.Remove(entity.Tag);
                }
            }
        }

        public X3DEntityHandle FindEntityByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return null;

            if (tagToEntityMap.TryGetValue(tag, out X3DEntityHandle handle))
            {
                return handle;
            }

            return null;
        }

        #endregion

        #region Resource Loading

        public void LoadTextureAsync(string url, Action<Texture2D> onLoaded)
        {
            if (string.IsNullOrEmpty(url))
            {
                onLoaded?.Invoke(null);
                return;
            }

            StartCoroutine(LoadTextureCoroutine(url, onLoaded));
        }

        private IEnumerator LoadTextureCoroutine(string url, Action<Texture2D> onLoaded)
        {
            // Try loading from Resources first (for local files)
            if (!url.StartsWith("http"))
            {
                string resourcePath = url.Replace("\\", "/");
                if (resourcePath.StartsWith("/"))
                    resourcePath = resourcePath.Substring(1);

                // Remove extension for Resources.Load
                int extIndex = resourcePath.LastIndexOf('.');
                if (extIndex > 0)
                    resourcePath = resourcePath.Substring(0, extIndex);

                Texture2D tex = Resources.Load<Texture2D>(resourcePath);
                if (tex != null)
                {
                    onLoaded?.Invoke(tex);
                    yield break;
                }
            }

            // Try UnityWebRequest for URLs or file paths
            using (UnityEngine.Networking.UnityWebRequest request =
                UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
                    onLoaded?.Invoke(texture);
                }
                else
                {
                    LogSystem.LogWarning($"[StraightFourX3DAdapter] Failed to load texture: {url}");
                    onLoaded?.Invoke(null);
                }
            }
        }

        public void LoadAudioClipAsync(string url, Action<AudioClip> onLoaded)
        {
            if (string.IsNullOrEmpty(url))
            {
                onLoaded?.Invoke(null);
                return;
            }

            StartCoroutine(LoadAudioCoroutine(url, onLoaded));
        }

        private IEnumerator LoadAudioCoroutine(string url, Action<AudioClip> onLoaded)
        {
            // Determine audio type from extension
            AudioType audioType = AudioType.UNKNOWN;
            string lowerUrl = url.ToLowerInvariant();
            if (lowerUrl.EndsWith(".mp3"))
                audioType = AudioType.MPEG;
            else if (lowerUrl.EndsWith(".wav"))
                audioType = AudioType.WAV;
            else if (lowerUrl.EndsWith(".ogg"))
                audioType = AudioType.OGGVORBIS;

            using (UnityEngine.Networking.UnityWebRequest request =
                UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(request);
                    onLoaded?.Invoke(clip);
                }
                else
                {
                    LogSystem.LogWarning($"[StraightFourX3DAdapter] Failed to load audio: {url}");
                    onLoaded?.Invoke(null);
                }
            }
        }

        public void LoadModelAsync(string url, Action<GameObject> onLoaded)
        {
            if (string.IsNullOrEmpty(url))
            {
                onLoaded?.Invoke(null);
                return;
            }

            // TODO: Integrate with GLTF handler or other model loaders
            LogSystem.LogWarning($"[StraightFourX3DAdapter] Model loading not yet implemented: {url}");
            onLoaded?.Invoke(null);
        }

        #endregion

        #region Material Creation

        public Material CreateMaterial(
            Color diffuseColor,
            Color emissiveColor,
            Color specularColor,
            float ambientIntensity,
            float shininess,
            float transparency)
        {
            Material material = new Material(defaultShader);

            // Set base color
            material.color = diffuseColor;

            // Handle transparency
            if (transparency > 0.001f)
            {
                // Configure for transparency (URP)
                material.SetFloat("_Surface", 1); // Transparent
                material.SetFloat("_Blend", 0); // Alpha blend
                material.SetFloat("_AlphaClip", 0);

                Color colorWithAlpha = diffuseColor;
                colorWithAlpha.a = 1f - transparency;
                material.color = colorWithAlpha;

                // Set render queue for transparency
                material.renderQueue = 3000;
            }

            // Set emission
            if (emissiveColor.r > 0.001f || emissiveColor.g > 0.001f || emissiveColor.b > 0.001f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emissiveColor);
            }

            // Set smoothness (X3D shininess is 0-1, Unity smoothness is 0-1)
            material.SetFloat("_Smoothness", shininess);

            return material;
        }

        public void ApplyTextureToMaterial(Material material, Texture2D texture, bool repeatS, bool repeatT)
        {
            if (material == null || texture == null)
                return;

            material.mainTexture = texture;

            // Set texture wrap mode
            texture.wrapModeU = repeatS ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            texture.wrapModeV = repeatT ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        }

        #endregion

        #region Helpers

        private BaseEntity GetBaseEntity(X3DEntityHandle handle)
        {
            if (handle == null)
                return null;

            if (entityMap.TryGetValue(handle.Id, out BaseEntity entity))
            {
                return entity;
            }

            // Try to get from EntityManager
            return EntityManager?.FindEntity(handle.Id);
        }

        private LightType ConvertLightType(X3DLightType x3dType)
        {
            switch (x3dType)
            {
                case X3DLightType.Directional:
                    return LightType.Directional;
                case X3DLightType.Point:
                    return LightType.Point;
                case X3DLightType.Spot:
                    return LightType.Spot;
                default:
                    return LightType.Point;
            }
        }

        private Material CreateDefaultMaterial()
        {
            return CreateMaterial(
                new Color(0.8f, 0.8f, 0.8f),
                Color.black,
                Color.white,
                0.2f,
                0.2f,
                0f
            );
        }

        #endregion
    }
}
