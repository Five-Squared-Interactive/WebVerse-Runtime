// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a light entity.
    /// </summary>
    public class LightEntity : BaseEntity
    {
        /// <summary>
        /// Create a light entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// light entity object.</param>
        /// <returns>The ID of the light entity object.</returns>
        public static LightEntity Create(BaseEntity parent,
            Vector3 position, Quaternion rotation,
            string id = null, string tag = null, string onLoaded = null)
        {
            Guid guid;
            if (string.IsNullOrEmpty(id))
            {
                guid = Guid.NewGuid();
            }
            else
            {
                guid = Guid.Parse(id);
            }

            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);
            UnityEngine.Vector3 pos = new UnityEngine.Vector3(position.x, position.y, position.z);
            UnityEngine.Quaternion rot = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            LightEntity le = new LightEntity();

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                le.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(le.internalEntity, le);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { le });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadLightEntity(pBE, pos, rot, guid, tag, onLoadAction);

            return le;
        }

        /// <summary>
        /// Create a light entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the light entity configuration.</param>
        /// <param name="parent">Parent entity for the light entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created light entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, lightEntity) =>
            {
                if (!success || lightEntity == null || !(lightEntity is StraightFour.Entity.LightEntity))
                {
                    Logging.LogError("[LightEntity:Create] Error loading light entity from JSON.");
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                            onLoaded, new object[] { null });
                    }
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                            onLoaded, new object[] { EntityAPIHelper.GetPublicEntity(
                                (StraightFour.Entity.LightEntity) lightEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadLightEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal LightEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.LightEntity);
        }

        /// <summary>
        /// Set the light type for the light entity.
        /// </summary>
        /// <param name="type">Light type to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetLightType(LightType type)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[LightEntity:SetLightType] Unknown entity.");
                return false;
            }

            switch (type)
            {
                case LightType.Spot:
                    return ((StraightFour.Entity.LightEntity) internalEntity).SetLightType(StraightFour.Entity.LightEntity.LightType.Spot);


                case LightType.Directional:
                    return ((StraightFour.Entity.LightEntity) internalEntity).SetLightType(StraightFour.Entity.LightEntity.LightType.Directional);

                case LightType.Point:
                    return ((StraightFour.Entity.LightEntity) internalEntity).SetLightType(StraightFour.Entity.LightEntity.LightType.Point);

                default:
                    Logging.LogError("[LightEntity:SetLightType] Unknown light type.");
                    return false;
            }
        }

        /// <summary>
        /// Set the properties for the light.
        /// </summary>
        /// <param name="color">Color to apply to the light.</param>
        /// <param name="temperature">Temperature to apply to the light.</param>
        /// <param name="intensity">Intensity to apply to the light.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetLightProperties(Color color, int temperature, float intensity)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[LightEntity:SetLightProperties] Unknown entity.");
                return false;
            }

            UnityEngine.Color32 col = new UnityEngine.Color(color.r, color.g, color.b, color.a);
            return ((StraightFour.Entity.LightEntity) internalEntity).SetLightProperties(col, temperature, intensity);
        }

        /// <summary>
        /// Set the properties for the light.
        /// </summary>
        /// <param name="range">Range to apply to the light.</param>
        /// <param name="innerSpotAngle">Inner spot angle to apply to the light.</param>
        /// <param name="outerSpotAngle">Outer spot angle to apply to the light.</param>
        /// <param name="color">Color to apply to the light.</param>
        /// <param name="temperature">Temperature to apply to the light.</param>
        /// <param name="intensity">Intensity to apply to the light.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetLightProperties(float range, float innerSpotAngle, float outerSpotAngle,
            Color color, int temperature, float intensity)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[LightEntity:SetLightProperties] Unknown entity.");
                return false;
            }

            UnityEngine.Color32 col = new UnityEngine.Color(color.r, color.g, color.b, color.a);
            return ((StraightFour.Entity.LightEntity) internalEntity).SetLightProperties(
                range, innerSpotAngle, outerSpotAngle, col, temperature, intensity);
        }

        /// <summary>
        /// Set the properties for the light.
        /// </summary>
        /// <param name="range">Range to apply to the light.</param>
        /// <param name="intensity">Intensity to apply to the light.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetLightProperties(float range, float intensity)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[LightEntity:SetLightProperties] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.LightEntity) internalEntity).SetLightProperties(range, intensity);
        }

        /// <summary>
        /// Get the properties for the light.
        /// </summary>
        /// <returns>The light properties of the light.</returns>
        public LightProperties GetLightProperties()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[LightEntity:GetLightProperties] Unknown entity.");
                return new LightProperties()
                { color = Color.black, innerSpotAngle = 0, intensity = 0, outerSpotAngle = 0, range = 0, temperature = 0 };
            }

            StraightFour.Entity.LightEntity.LightProperties props = ((StraightFour.Entity.LightEntity) internalEntity).GetLightProperties();

            return new LightProperties()
            {
                color = new Color(props.color.r, props.color.g, props.color.b, props.color.a),
                temperature = props.temperature,
                intensity = props.intensity,
                range = props.range,
                innerSpotAngle = props.innerSpotAngle,
                outerSpotAngle = props.outerSpotAngle
            };
        }
    }
}