// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for a dropdown entity.
    /// </summary>
    public class DropdownEntity : UIElementEntity
    {
        /// <summary>
        /// Image object for the dropdown entity.
        /// </summary>
        public Image imageObject;

        /// <summary>
        /// Button object for the dropdown entity.
        /// </summary>
        public Dropdown dropdownObject;

        /// <summary>
        /// Add an option to the dropdown entity.
        /// </summary>
        /// <param name="option">Option to add.</param>
        public int AddOption(string option)
        {
            if (dropdownObject == null)
            {
                Utilities.LogSystem.LogError("[DropdownEntity->AddOption] No dropdown.");
                return -1;
            }

            Dropdown.OptionData optionData = new Dropdown.OptionData(option);

            dropdownObject.AddOptions(new List<Dropdown.OptionData>(){ optionData });

            return dropdownObject.options.Count - 1;
        }

        /// <summary>
        /// Clear options from the dropdown entity.
        /// </summary>
        public void ClearOptions()
        {
            if (dropdownObject == null)
            {
                Utilities.LogSystem.LogError("[DropdownEntity->AddOption] No dropdown.");
                return;
            }

            dropdownObject.ClearOptions();
        }

        /// <summary>
        /// Set the onChange event for the dropdown entity.
        /// </summary>
        /// <param name="onClick">Action to perform on change. Takes an integer parameter corresponding to
        /// the index in the dropdown that was selected.</param>
        public void SetOnChange(Action<int> onChange)
        {
            if (dropdownObject == null)
            {
                Utilities.LogSystem.LogError("[DropdownEntity->SetOnClick] No dropdown.");
                return;
            }

            dropdownObject.onValueChanged = new Dropdown.DropdownEvent();
            dropdownObject.onValueChanged.AddListener((index) => {
                if (onChange != null)
                {
                    onChange.Invoke(index);
                }
            });
        }

        /// <summary>
        /// Set the background sprite for the dropdown entity.
        /// </summary>
        /// <param name="background">Background to set the sprite for the dropdown entity to.</param>
        public void SetBackground(Sprite background)
        {
            if (dropdownObject == null)
            {
                Utilities.LogSystem.LogError("[DropdownEntity->SetBackground] No dropdown.");
                return;
            }

            imageObject.sprite = background;
        }

        /// <summary>
        /// Set the base color for the dropdown entity.
        /// </summary>
        /// <param name="color">Color to set the base color for the dropdown entity to.</param>
        public void SetBaseColor(Color color)
        {
            if (dropdownObject == null)
            {
                Utilities.LogSystem.LogError("[DropdownEntity->SetBaseColor] No dropdown.");
                return;
            }

            if (imageObject == null)
            {
                Utilities.LogSystem.LogError("[DropdownEntity->SetBaseColor] No image.");
                return;
            }

            imageObject.color = color;
        }

        /// <summary>
        /// Set the colors for the dropdown entity.
        /// </summary>
        /// <param name="defaultColor">Color to set the default color for the dropdown entity to.</param>
        /// <param name="hoverColor">Color to set the hover color for the dropdown entity to.</param>
        /// <param name="clickColor">Color to set the click color for the dropdown entity to.</param>
        /// <param name="inactiveColor">Color to set the inactive color for the dropdown entity to.</param>
        public void SetColors(Color defaultColor, Color hoverColor, Color clickColor, Color inactiveColor)
        {
            if (dropdownObject == null)
            {
                Utilities.LogSystem.LogError("[DropdownEntity->SetDefaultColor] No dropdown.");
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
            dropdownObject.colors = newColors;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /// <param name="parentCanvas">Canvas to place the dropdown entity on.</param>
        public override void Initialize(Guid idToSet, UIEntity parentCanvas)
        {
            base.Initialize(idToSet, parentCanvas);

            imageObject = gameObject.GetComponent<Image>();

            dropdownObject = gameObject.GetComponent<Dropdown>();
            RectTransform rt = dropdownObject.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = dropdownObject.gameObject.AddComponent<RectTransform>();
            }
            rt.position = Vector3.zero;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            uiElementRectTransform = rt;

            GraphicRaycaster graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            dropdownObject.ClearOptions();

            if (parentCanvas != null)
            {
                SetParent(parentCanvas);
            }

            MakeHidden();
        }
    }
}