// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a water blocker entity.
    /// </summary>
    public class WaterBlockerEntity : BaseEntity
    {
        /// <summary>
        /// Create a water blocker entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="scale">Scale of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// water entity object.</param>
        /// <returns>The ID of the water blocker entity object.</returns>
        public static WaterBlockerEntity CreateWaterBlocker(BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale, string id = null, string tag = null,
            string onLoaded = null)
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
            UnityEngine.Vector3 scl = new UnityEngine.Vector3(scale.x, scale.y, scale.z);

            WaterBlockerEntity we = new WaterBlockerEntity();

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                we.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(we.internalEntity, we);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { we });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadWaterBlockerEntity(
                null, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity, guid, tag, onLoadAction);

            return we;
        }

        /// <summary>
        /// Create a water blocker entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the water blocker entity configuration.</param>
        /// <param name="parent">Parent entity for the water blocker entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created water blocker entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, waterBlockerEntity) =>
            {
                if (!success || waterBlockerEntity == null || !(waterBlockerEntity is StraightFour.Entity.WaterBlockerEntity))
                {
                    Logging.LogError("[WaterBlockerEntity:Create] Error loading water blocker entity from JSON.");
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
                                (StraightFour.Entity.WaterBlockerEntity) waterBlockerEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadWaterBlockerEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        public WaterBlockerEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.WaterBlockerEntity);
        }
    }
}