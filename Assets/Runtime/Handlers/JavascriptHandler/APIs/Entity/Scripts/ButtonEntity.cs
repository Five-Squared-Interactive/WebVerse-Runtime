// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a button entity.
    /// </summary>
    public class ButtonEntity : BaseEntity
    {
        /// <summary>
        /// Create a button entity.
        /// </summary>
        /// <param name="parent">Parent canvas of the entity to create.</param>
        /// <param name="onClick">Action to perform on button click.</param>
        /// <param name="positionPercent">Position of the entity within its canvas.</param>
        /// <param name="sizePercent">Size of the entity relative to its canvas.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// button entity object.</param>
        /// <returns>The ID of the button entity object.</returns>
        public static ButtonEntity Create(CanvasEntity parent, string onClick,
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

            StraightFour.Entity.CanvasEntity pCE = (StraightFour.Entity.CanvasEntity) EntityAPIHelper.GetPrivateEntity(parent);
            if (pCE == null)
            {
                Logging.LogWarning("[ButtonEntity->Create] Invalid parent entity.");
                return null;
            }

            UnityEngine.Vector2 pos = new UnityEngine.Vector2(positionPercent.x, positionPercent.y);
            UnityEngine.Vector2 size = new UnityEngine.Vector2(sizePercent.x, sizePercent.y);

            ButtonEntity be = new ButtonEntity();

            System.Action onClickAction = null;
            if (!string.IsNullOrEmpty(onClick))
            {
                onClickAction = () =>
                {
                    if (WebVerseRuntime.Instance.inputManager.inputEnabled)
                    {
                        WebVerseRuntime.Instance.javascriptHandler.Run(onClick);
                    }
                };
            }

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                be.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(be.internalEntity, be);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { be });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadButtonEntity(pCE, pos, size, onClickAction, guid, tag, onLoadAction);

            return be;
        }

        /// <summary>
        /// Create a button entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the button entity configuration.</param>
        /// <param name="parent">Parent entity for the button entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created button entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, buttonEntity) =>
            {
                if (!success || buttonEntity == null || !(buttonEntity is StraightFour.Entity.ButtonEntity))
                {
                    Logging.LogError("[ButtonEntity:Create] Error loading button entity from JSON.");
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
                                (StraightFour.Entity.ButtonEntity) buttonEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadButtonEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal ButtonEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.ButtonEntity);
        }

        /// <summary>
        /// Set the onClick event for the button entity.
        /// </summary>
        /// <param name="onClick">Action to perform on click.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetOnClick(string onClick)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[ButtonEntity:SetOnClick] Unknown entity.");
                return false;
            }

            System.Action onClickAction = null;
            if (!string.IsNullOrEmpty(onClick))
            {
                onClickAction = () =>
                {
                    if (WebVerseRuntime.Instance.inputManager.inputEnabled)
                    {
                        WebVerseRuntime.Instance.javascriptHandler.Run(onClick);
                    }
                };
            }

            ((StraightFour.Entity.ButtonEntity) internalEntity).SetOnClick(onClickAction);

            return true;
        }

        /// <summary>
        /// Set the background image for the button entity.
        /// </summary>
        /// <param name="imagePath">Path to the image to set the background to.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetBackground(string imagePath)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[ButtonEntity:SetBackground] Unknown entity.");
                return false;
            }

            EntityAPIHelper.ApplyImageToButtonAsync(imagePath, (StraightFour.Entity.ButtonEntity) internalEntity);

            return true;
        }

        /// <summary>
        /// Set the base color for the button entity.
        /// </summary>
        /// <param name="color">Color to set the button entity to.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetBaseColor(Color color)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[ButtonEntity:SetBaseColor] Unknown entity.");
                return false;
            }

            ((StraightFour.Entity.ButtonEntity) internalEntity).SetBaseColor(
                new UnityEngine.Color(color.r, color.g, color.b, color.a));

            return true;
        }

        /// <summary>
        /// Set the colors for the button entity.
        /// </summary>
        /// <param name="defaultColor">Color to set the default color for the button entity to.</param>
        /// <param name="hoverColor">Color to set the hover color for the button entity to.</param>
        /// <param name="clickColor">Color to set the click color for the button entity to.</param>
        /// <param name="inactiveColor">Color to set the inactive color for the button entity to.</param>
        public bool SetColors(Color defaultColor, Color hoverColor, Color clickColor, Color inactiveColor)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[ButtonEntity:SetColors] Unknown entity.");
                return false;
            }

            ((StraightFour.Entity.ButtonEntity) internalEntity).SetColors(
                new UnityEngine.Color(defaultColor.r, defaultColor.g, defaultColor.b, defaultColor.a),
                new UnityEngine.Color(hoverColor.r, hoverColor.g, hoverColor.b, hoverColor.a),
                new UnityEngine.Color(clickColor.r, clickColor.g, clickColor.b, clickColor.a),
                new UnityEngine.Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, inactiveColor.a));

            return true;
        }

        /// <summary>
        /// Stretch the button entity to fill its parent.
        /// </summary>
        /// <param name="stretch">Whether to stretch to parent. If false, restores normal sizing.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool StretchToParent(bool stretch = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[ButtonEntity:StretchToParent] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.UIElementEntity) internalEntity).StretchToParent(stretch);
        }

        /// <summary>
        /// Set the alignment of the button entity within its parent.
        /// </summary>
        /// <param name="alignment">Alignment to set.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetAlignment(UIElementAlignment alignment)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[ButtonEntity:SetAlignment] Unknown entity.");
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
    }
}