// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for an image entity.
    /// </summary>
    public class ImageEntity : BaseEntity
    {
        /// <summary>
        /// Create an image entity.
        /// </summary>
        /// <param name="parent">Parent canvas of the entity to create.</param>
        /// <param name="imageFile">Path to image file to apply to the new image entity.</param>
        /// <param name="positionPercent">Position of the entity within its canvas.</param>
        /// <param name="sizePercent">Size of the entity relative to its canvas.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// image entity object.</param>
        /// <returns>The ID of the image entity object.</returns>
        public static ImageEntity Create(BaseEntity parent, string imageFile,
            Vector2 positionPercent, Vector2 sizePercent,
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

            StraightFour.Entity.BaseEntity pCE = EntityAPIHelper.GetPrivateEntity(parent);
            if (pCE == null)
            {
                Logging.LogWarning("[ImageEntity->Create] Invalid parent entity.");
                return null;
            }
            if (pCE is not StraightFour.Entity.UIEntity)
            {
                Logging.LogWarning("[ImageEntity->Create] Parent entity not UI element.");
                return null;
            }

            UnityEngine.Vector2 pos = new UnityEngine.Vector2(positionPercent.x, positionPercent.y);
            UnityEngine.Vector2 size = new UnityEngine.Vector2(sizePercent.x, sizePercent.y);

            ImageEntity ie = new ImageEntity();

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                ie.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(ie.internalEntity, ie);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ie });
                }
            };

            return EntityAPIHelper.LoadImageEntityAsync(parent, imageFile, positionPercent, sizePercent, id, tag, onLoaded);
        }
        
        /// <summary>
        /// Create an image entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the image entity configuration.</param>
        /// <param name="parent">Parent entity for the image entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created image entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, imageEntity) =>
            {
                if (!success || imageEntity == null || !(imageEntity is StraightFour.Entity.ImageEntity))
                {
                    Logging.LogError("[ImageEntity:Create] Error loading image entity from JSON.");
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
                                (StraightFour.Entity.ImageEntity) imageEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadImageEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        /// <summary>
        /// Stretch the image entity to fill its parent.
        /// </summary>
        /// <param name="stretch">Whether to stretch to parent. If false, restores normal sizing.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool StretchToParent(bool stretch = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[ImageEntity:StretchToParent] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.UIElementEntity)internalEntity).StretchToParent(stretch);
        }

        /// <summary>
        /// Set the alignment of the image entity within its parent.
        /// </summary>
        /// <param name="alignment">Alignment to set.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetAlignment(UIElementAlignment alignment)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[ImageEntity:SetAlignment] Unknown entity.");
                return false;
            }
            
            StraightFour.Entity.UIElementAlignment convertedAlignment = StraightFour.Entity.UIElementAlignment.Center;
            switch (alignment)
            {
                case UIElementAlignment.Center:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Center;
                    break;

                case UIElementAlignment.Left:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Left;
                    break;

                case UIElementAlignment.Right:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Right;
                    break;

                case UIElementAlignment.Top:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Top;
                    break;

                case UIElementAlignment.Bottom:
                    convertedAlignment = StraightFour.Entity.UIElementAlignment.Bottom;
                    break;

                default:
                    Logging.LogError("[ButtonEntity:SetAlignment] Invalid alignment.");
                    return false;
            }

            return ((StraightFour.Entity.UIElementEntity)internalEntity).SetAlignment(convertedAlignment);
        }

        internal ImageEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.ImageEntity);
        }
    }
}