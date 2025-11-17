// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using System.Linq;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a dropdown entity.
    /// </summary>
    public class DropdownEntity : BaseEntity
    {
        /// <summary>
        /// Create a dropdown entity.
        /// </summary>
        /// <param name="parent">Parent canvas of the entity to create.</param>
        /// <param name="onChange">Action to perform on dropdown change. Takes integer parameter
        /// which corresponds to index of selected option.</param>
        /// <param name="positionPercent">Position of the entity within its canvas.</param>
        /// <param name="sizePercent">Size of the entity relative to its canvas.</param>
        /// <param name="options">Options to apply to the dropdown entity.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// dropdown entity object.</param>
        /// <returns>The ID of the dropdown entity object.</returns>
        public static DropdownEntity Create(CanvasEntity parent, string onChange,
            Vector2 positionPercent, Vector2 sizePercent, string[] options = null,
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
                Logging.LogWarning("[DropdownEntity->Create] Invalid parent entity.");
                return null;
            }

            UnityEngine.Vector2 pos = new UnityEngine.Vector2(positionPercent.x, positionPercent.y);
            UnityEngine.Vector2 size = new UnityEngine.Vector2(sizePercent.x, sizePercent.y);

            DropdownEntity de = new DropdownEntity();

            System.Action<int> onChangeAction = null;
            if (!string.IsNullOrEmpty(onChange))
            {
                onChangeAction = (index) =>
                {
                    if (WebVerseRuntime.Instance.inputManager.inputEnabled)
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onChange, new object[] { index });
                    }
                };
            }

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                de.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(de.internalEntity, de);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { de });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadDropdownEntity(pCE, pos, size, onChangeAction,
                options.ToList(), guid, tag, onLoadAction);

            return de;
        }

        /// <summary>
        /// Create a dropdown entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the dropdown entity configuration.</param>
        /// <param name="parent">Parent entity for the dropdown entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created dropdown entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, dropdownEntity) =>
            {
                if (!success || dropdownEntity == null || !(dropdownEntity is StraightFour.Entity.DropdownEntity))
                {
                    Logging.LogError("[DropdownEntity:Create] Error loading dropdown entity from JSON.");
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
                                (StraightFour.Entity.DropdownEntity) dropdownEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadDropdownEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal DropdownEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.DropdownEntity);
        }

        /// <summary>
        /// Set the onChange event for the dropdown entity.
        /// </summary>
        /// <param name="onClick">Action to perform on change.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetOnChange(string onChange)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[DropdownEntity:SetOnClick] Unknown entity.");
                return false;
            }

            System.Action<int> onChangeAction = null;
            if (!string.IsNullOrEmpty(onChange))
            {
                onChangeAction = (index) =>
                {
                    if (WebVerseRuntime.Instance.inputManager.inputEnabled)
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onChange, new object[] { index });
                    }
                };
            }

            ((StraightFour.Entity.DropdownEntity) internalEntity).SetOnChange(onChangeAction);

            return true;
        }

        /// <summary>
        /// Set the background image for the dropdown entity.
        /// </summary>
        /// <param name="imagePath">Path to the image to set the background to.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetBackground(string imagePath)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[DropdownEntity:SetBackground] Unknown entity.");
                return false;
            }

            EntityAPIHelper.ApplyImageToDropdownAsync(imagePath, (StraightFour.Entity.DropdownEntity) internalEntity);

            return true;
        }

        /// <summary>
        /// Set the base color for the dropdown entity.
        /// </summary>
        /// <param name="color">Color to set the dropdown entity to.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetBaseColor(Color color)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[DropdownEntity:SetBaseColor] Unknown entity.");
                return false;
            }

            ((StraightFour.Entity.DropdownEntity) internalEntity).SetBaseColor(
                new UnityEngine.Color(color.r, color.g, color.b, color.a));

            return true;
        }

        /// <summary>
        /// Set the colors for the dropdown entity.
        /// </summary>
        /// <param name="defaultColor">Color to set the default color for the dropdown entity to.</param>
        /// <param name="hoverColor">Color to set the hover color for the dropdown entity to.</param>
        /// <param name="clickColor">Color to set the click color for the dropdown entity to.</param>
        /// <param name="inactiveColor">Color to set the inactive color for the dropdown entity to.</param>
        public bool SetColors(Color defaultColor, Color hoverColor, Color clickColor, Color inactiveColor)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[DropdownEntity:SetColors] Unknown entity.");
                return false;
            }

            ((StraightFour.Entity.DropdownEntity) internalEntity).SetColors(
                new UnityEngine.Color(defaultColor.r, defaultColor.g, defaultColor.b, defaultColor.a),
                new UnityEngine.Color(hoverColor.r, hoverColor.g, hoverColor.b, hoverColor.a),
                new UnityEngine.Color(clickColor.r, clickColor.g, clickColor.b, clickColor.a),
                new UnityEngine.Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, inactiveColor.a));

            return true;
        }

        /// <summary>
        /// Add an option to the dropdown entity.
        /// </summary>
        /// <param name="option">Option to add.</param>
        public int AddOption(string option)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[DropdownEntity:AddOption] Unknown entity.");
                return -1;
            }

            return ((StraightFour.Entity.DropdownEntity) internalEntity).AddOption(option);
        }

        /// <summary>
        /// Clear options from the dropdown entity.
        /// </summary>
        public bool ClearOptions()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[DropdownEntity:ClearOptions] Unknown entity.");
                return false;
            }

            ((StraightFour.Entity.DropdownEntity) internalEntity).ClearOptions();

            return true;
        }

        /// <summary>
        /// Stretch the dropdown entity to fill its parent.
        /// </summary>
        /// <param name="stretch">Whether to stretch to parent. If false, restores normal sizing.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool StretchToParent(bool stretch = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[DropdownEntity:StretchToParent] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.UIElementEntity) internalEntity).StretchToParent(stretch);
        }

        /// <summary>
        /// Set the alignment of the dropdown entity within its parent.
        /// </summary>
        /// <param name="alignment">Alignment to set.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetAlignment(UIElementAlignment alignment)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[DropdownEntity:SetAlignment] Unknown entity.");
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