// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Utilities;


namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a container entity.
    /// </summary>
    public class ContainerEntity : BaseEntity
    {
        /// <summary>
        /// Create a container entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="scale">Scale of the entity relative to its parent.</param>
        /// <param name="isSize">Whether or not the scale parameter is a size.</param>
        /// <param name="tag">Tag of the container entity.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// container entity object.</param>
        /// <returns>The ID of the container entity object.</returns>
        public static ContainerEntity Create(BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale, bool isSize = false,
            string tag = null, string id = null, string onLoaded = null)
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

            ContainerEntity ce = new ContainerEntity();

            System.Action onLoadAction = null;
                onLoadAction = () =>
                {
                    ce.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                    EntityAPIHelper.AddEntityMapping(ce.internalEntity, ce);
                    if (ce.internalEntity != null)
                    {
                        ce.internalEntity.entityTag = tag;
                    }
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ce });
                    }
                };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadContainerEntity(pBE, pos, rot, scl, guid, tag, isSize, onLoadAction);

            return ce;
        }

        /// <summary>
        /// Create a container entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the container entity configuration.</param>
        /// <param name="parent">Parent entity for the container entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created container entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, containerEntity) =>
            {
                if (!success || containerEntity == null || !(containerEntity is StraightFour.Entity.ContainerEntity))
                {
                    Logging.LogError("[ContainerEntity:Create] Error loading container entity from JSON.");
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
                                (StraightFour.Entity.ContainerEntity) containerEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadContainerEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal ContainerEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.ContainerEntity);
        }
    }
}