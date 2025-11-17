// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a canvas entity.
    /// </summary>
    public class CanvasEntity : BaseEntity
    {
        /// <summary>
        /// Create a canvas entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="scale">Scale of the entity relative to its parent.</param>
        /// <param name="isSize">Whether or not the scale parameter is a size.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// canvas entity object.</param>
        /// <returns>The ID of the canvas entity object.</returns>
        public static CanvasEntity Create(BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale, bool isSize = false,
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
            UnityEngine.Vector3 scl = new UnityEngine.Vector3(scale.x, scale.y, scale.z);

            CanvasEntity ce = new CanvasEntity();

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                ce.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(ce.internalEntity, ce);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ce });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadCanvasEntity(pBE, pos, rot, scl, guid, isSize, tag, onLoadAction);

            return ce;
        }

        /// <summary>
        /// Create a canvas entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the canvas entity configuration.</param>
        /// <param name="parent">Parent entity for the canvas entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created canvas entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, canvasEntity) =>
            {
                if (!success || canvasEntity == null || !(canvasEntity is StraightFour.Entity.CanvasEntity))
                {
                    Logging.LogError("[CanvasEntity:Create] Error loading canvas entity from JSON.");
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
                                (StraightFour.Entity.CanvasEntity) canvasEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadCanvasEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal CanvasEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.CanvasEntity);
        }

        /// <summary>
        /// Make the canvas a world canvas.
        /// </summary>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool MakeWorldCanvas()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CanvasEntity:MakeWorldCanvas] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CanvasEntity) internalEntity).MakeWorldCanvas();
        }

        /// <summary>
        /// Make the canvas a screen canvas.
        /// </summary>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool MakeScreenCanvas(bool synchronize = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CanvasEntity:MakeScreenCanvas] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CanvasEntity) internalEntity).MakeScreenCanvas();
        }

        /// <summary>
        /// Returns whether or not the canvas entity is a screen canvas.
        /// </summary>
        /// <returns>Whether or not the canvas entity is a screen canvas.</returns>
        public bool IsScreenCanvas()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CanvasEntity:IsScreenCanvas] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CanvasEntity) internalEntity).IsScreenCanvas();
        }

        /// <summary>
        /// Set the size for the screen canvas.
        /// </summary>
        /// <param name="size">Size to set the screen canvas to.</param>
        /// <param name="synchronizeChange">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetSize(Vector2 size, bool synchronizeChange = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[CanvasEntity:SetSize] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.CanvasEntity) internalEntity).SetSize(new UnityEngine.Vector2(size.x, size.y), synchronizeChange);
        }
    }
}