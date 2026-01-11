// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.UI;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for a button entity.
    /// </summary>
    public class ButtonEntity : UIElementEntity
    {
        /// <summary>
        /// Image object for the button entity.
        /// </summary>
        public Image imageObject;

        /// <summary>
        /// Button object for the button entity.
        /// </summary>
        public Button buttonObject;

        /// <summary>
        /// Set the onClick event for the button entity.
        /// </summary>
        /// <param name="onClick">Action to perform on click.</param>
        public void SetOnClick(Action onClick)
        {
            if (buttonObject == null)
            {
                Utilities.LogSystem.LogError("[ButtonEntity->SetOnClick] No button.");
                return;
            }

            buttonObject.onClick = new Button.ButtonClickedEvent();
            buttonObject.onClick.AddListener(() => {
                if (onClick != null)
                {
                    onClick.Invoke();
                }
            });
        }

        /// <summary>
        /// Set the background sprite for the button entity.
        /// </summary>
        /// <param name="background">Background to set the sprite for the button entity to.</param>
        public void SetBackground(Sprite background)
        {
            if (buttonObject == null)
            {
                Utilities.LogSystem.LogError("[ButtonEntity->SetBackground] No button.");
                return;
            }

            imageObject.sprite = background;
        }

        /// <summary>
        /// Set the base color for the button entity.
        /// </summary>
        /// <param name="color">Color to set the base color for the button entity to.</param>
        public void SetBaseColor(Color color)
        {
            if (buttonObject == null)
            {
                Utilities.LogSystem.LogError("[ButtonEntity->SetBaseColor] No button.");
                return;
            }

            if (imageObject == null)
            {
                Utilities.LogSystem.LogError("[ButtonEntity->SetBaseColor] No image.");
                return;
            }

            imageObject.color = color;
        }

        /// <summary>
        /// Set the colors for the button entity.
        /// </summary>
        /// <param name="defaultColor">Color to set the default color for the button entity to.</param>
        /// <param name="hoverColor">Color to set the hover color for the button entity to.</param>
        /// <param name="clickColor">Color to set the click color for the button entity to.</param>
        /// <param name="inactiveColor">Color to set the inactive color for the button entity to.</param>
        public void SetColors(Color defaultColor, Color hoverColor, Color clickColor, Color inactiveColor)
        {
            if (buttonObject == null)
            {
                Utilities.LogSystem.LogError("[ButtonEntity->SetDefaultColor] No button.");
                return;
            }

            ColorBlock newColors = new ColorBlock();
            newColors.colorMultiplier = 1;
            newColors.fadeDuration = 0.1f;
            newColors.normalColor = defaultColor;
            newColors.highlightedColor = hoverColor;
            newColors.selectedColor = hoverColor;
            newColors.pressedColor = clickColor;
            newColors.disabledColor = inactiveColor;
            buttonObject.colors = newColors;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /// <param name="parentCanvas">Canvas to place the button entity on.</param>
        public override void Initialize(Guid idToSet, UIEntity parentCanvas)
        {
            base.Initialize(idToSet, parentCanvas);

            imageObject = gameObject.AddComponent<Image>();

            buttonObject = gameObject.AddComponent<Button>();
            buttonObject.targetGraphic = imageObject;
            RectTransform rt = buttonObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = buttonObject.gameObject.AddComponent<RectTransform>();
            }
            rt.position = Vector3.zero;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            uiElementRectTransform = rt;

            GraphicRaycaster graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            if (parentCanvas != null)
            {
                SetParent(parentCanvas);
            }

            MakeHidden();
        }
    }
}