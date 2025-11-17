// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Utilities;
using System;
using TMPro;
using UnityEngine;

namespace FiveSQD.WebVerse.Input.Keyboard
{
    /// <summary>
    /// Class for a keyboard.
    /// </summary>
    public class Keyboard : MonoBehaviour
    {
        /// <summary>
        /// Current input being modified.
        /// </summary>
        [Tooltip("Current input being modified.")]
        public TMP_InputField currentInput;

        /// <summary>
        /// Action to perform on enter. Takes string from input.
        /// </summary>
        [Tooltip("Action to perform on enter. Takes string from input.")]
        public Action<string> onEnter;

        /// <summary>
        /// The main keyboard.
        /// </summary>
        [Tooltip("The main keyboard.")]
        public GameObject mainKeyboard;

        /// <summary>
        /// The shift keyboard.
        /// </summary>
        [Tooltip("The shift keyboard.")]
        public GameObject shiftKeyboard;

        /// <summary>
        /// Whether or not the keyboard is currently on shift.
        /// </summary>
        private bool shifted;

        private void Start()
        {
            if (mainKeyboard == null || shiftKeyboard == null)
            {
                Logging.LogError("[Keybard->Start] Invalid Keyboard.");
                return;
            }

            mainKeyboard.SetActive(true);
            shiftKeyboard.SetActive(false);
            shifted = false;
        }

        /// <summary>
        /// Add a character to the current input at the current cursor position.
        /// </summary>
        /// <param name="character">Character to add.</param>
        public void AddCharacter(string character)
        {
            if (currentInput != null)
            {
                if (string.IsNullOrEmpty(currentInput.text))
                {
                    currentInput.text = character;
                }
                else
                {
                    currentInput.text = currentInput.text.Insert(currentInput.stringPosition, character);
                }
                currentInput.stringPosition += 1;
            }

            if (shifted)
            {
                UnShift();
            }
        }

        /// <summary>
        /// Remove character from the current input at the current cursor position.
        /// </summary>
        public void RemoveCharacter()
        {
            if (currentInput != null)
            {
                if (!string.IsNullOrEmpty(currentInput.text))
                {
                    if (currentInput.stringPosition > 0)
                    {
                        int newPos = currentInput.stringPosition - 1;
                        currentInput.text = currentInput.text.Remove(currentInput.stringPosition - 1, 1);
                        currentInput.stringPosition = newPos;
                    }
                }
            }

            if (shifted)
            {
                UnShift();
            }
        }

        /// <summary>
        /// Perform an enter.
        /// </summary>
        /// <param name="clear">Whether or not to clear the current input.</param>
        public void Enter(bool clear)
        {
            if (currentInput != null)
            {
                if (onEnter != null)
                {
                    onEnter.Invoke(currentInput.text);
                }

                if (clear)
                {
                    Clear();
                }
            }

            if (shifted)
            {
                UnShift();
            }
        }

        /// <summary>
        /// Clear the current input.
        /// </summary>
        public void Clear()
        {
            if (currentInput != null)
            {
                currentInput.text = "";
            }

            if (shifted)
            {
                UnShift();
            }
        }

        /// <summary>
        /// Perform a shift.
        /// </summary>
        public void Shift()
        {
            if (mainKeyboard == null || shiftKeyboard == null)
            {
                Logging.LogError("[Keybard->Shift] Invalid Keyboard.");
                return;
            }

            if (mainKeyboard.activeSelf == true && shiftKeyboard.activeSelf == false)
            {
                mainKeyboard.SetActive(false);
                shiftKeyboard.SetActive(true);
                shifted = true;
            }
        }

        /// <summary>
        /// Perform an unshift.
        /// </summary>
        public void UnShift()
        {
            if (mainKeyboard == null || shiftKeyboard == null)
            {
                Logging.LogError("[Keybard->UnShift] Invalid Keyboard.");
                return;
            }

            if (mainKeyboard.activeSelf == false && shiftKeyboard.activeSelf == true)
            {
                mainKeyboard.SetActive(true);
                shiftKeyboard.SetActive(false);
                shifted = false;
            }
        }

        /// <summary>
        /// Perform a caps.
        /// </summary>
        public void Caps()
        {
            if (mainKeyboard == null || shiftKeyboard == null)
            {
                Logging.LogError("[Keybard->Caps] Invalid Keyboard.");
                return;
            }

            if (mainKeyboard.activeSelf == true && shiftKeyboard.activeSelf == false)
            {
                mainKeyboard.SetActive(false);
                shiftKeyboard.SetActive(true);
            }
        }

        /// <summary>
        /// Perform an uncaps.
        /// </summary>
        public void UnCaps()
        {
            if (mainKeyboard == null || shiftKeyboard == null)
            {
                Logging.LogError("[Keybard->UnCaps] Invalid Keyboard.");
                return;
            }

            if (mainKeyboard.activeSelf == false && shiftKeyboard.activeSelf == true)
            {
                mainKeyboard.SetActive(true);
                shiftKeyboard.SetActive(false);
            }
        }
    }
}