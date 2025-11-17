// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FiveSQD.WebVerse.Interface.MultibarMenu
{
    /// <summary>
    /// Class for multibar input.
    /// </summary>
    public class MultibarInput : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// The multibar.
        /// </summary>
        public Multibar multibar;

        /// <summary>
        /// The input field.
        /// </summary>
        public TMP_InputField inputField;

        /// <summary>
        /// Keyboard for the input.
        /// </summary>
        public GameObject keyboard;

        /// <summary>
        /// Start of selection.
        /// </summary>
        public int selectionStart { get; private set; }

        /// <summary>
        /// End of selection.
        /// </summary>
        public int selectionEnd { get; private set; }

        /// <summary>
        /// Whether or not mouse is in input.
        /// </summary>
        private bool mouseIn;

        /// <summary>
        /// Whether or not to reset selection.
        /// </summary>
        private bool resetSelection;

        /// <summary>
        /// Delay for sekection.
        /// </summary>
        private int deselectDelay = 8;

        /// <summary>
        /// Countdown for deselection.
        /// </summary>
        private int deselectCountDown;

        private void Awake()
        {
            mouseIn = false;
            resetSelection = false;
            inputField.onTextSelection.AddListener(TextSelected);
            inputField.onEndTextSelection.AddListener(TextUnSelected);
            selectionStart = selectionEnd = -1;
            deselectCountDown = -1;
        }

        private void Update()
        {
            if (resetSelection)
            {
                deselectCountDown = deselectDelay;
                resetSelection = false;
            }

            if (deselectCountDown == 0)
            {
                selectionStart = selectionEnd = -1;
                deselectCountDown = -1;
            }
            else if (deselectCountDown > 0)
            {
                deselectCountDown--;
            }
        }

        /// <summary>
        /// Called when pointer enters.
        /// </summary>
        /// <param name="data">Pointer event data.</param>
        public void OnPointerEnter(PointerEventData data)
        {
            mouseIn = true;
        }

        /// <summary>
        /// Called when pointer exits.
        /// </summary>
        /// <param name="data">Pointer event data.</param>
        public void OnPointerExit(PointerEventData data)
        {
            mouseIn = false;
        }

        /// <summary>
        /// Called on left click.
        /// </summary>
        /// <param name="context">Calling context.</param>
        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (mouseIn)
                {
                    DisableRightClickMenu();
                }
            }
        }

        /// <summary>
        /// Called on right click.
        /// </summary>
        /// <param name="context">Calling context.</param>
        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (mouseIn)
            {
                EnableRightClickMenu();
            }
        }

        /// <summary>
        /// Enable the right click menu.
        /// </summary>
        public void EnableRightClickMenu()
        {
            multibar.rightClickMenu.SetActive(true);
        }

        /// <summary>
        /// Disable the right click menu.
        /// </summary>
        public void DisableRightClickMenu()
        {
            multibar.rightClickMenu.SetActive(false);
        }

        /// <summary>
        /// Called on text selection.
        /// </summary>
        /// <param name="str">Complete string.</param>
        /// <param name="pos1">Position 1 of selection.</param>
        /// <param name="pos2">Position 2 of selection.</param>
        public void TextSelected(string str, int pos1, int pos2)
        {
            if (inputField.isFocused)
            {
                selectionStart = Mathf.Min(pos1, pos2);
                selectionEnd = Mathf.Max(pos1 - 1, pos2 - 1);
            }
        }

        /// <summary>
        /// Called on text deselect.
        /// </summary>
        /// <param name="str">Complete string.</param>
        /// <param name="pos1">Position 1 of selection.</param>
        /// <param name="pos2">Position 2 of selection.</param>
        public void TextUnSelected(string str, int pos1, int pos2)
        {
            resetSelection = true;
        }
    }
}