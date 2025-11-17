// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;

namespace FiveSQD.WebVerse.Interface.MultibarMenu
{
    /// <summary>
    /// Class for right click menu.
    /// </summary>
    public class RightClickMenu : MonoBehaviour
    {
        /// <summary>
        /// The multibar input.
        /// </summary>
        public MultibarInput multibarInput;

        /// <summary>
        /// Perform a cut.
        /// </summary>
        public void Cut()
        {
            if (multibarInput.selectionStart != -1 && multibarInput.selectionEnd != -1)
            {
                Copy();
                
                if (multibarInput.selectionStart == 0)
                {
                    if (multibarInput.selectionEnd == multibarInput.inputField.text.Length - 1)
                    {
                        // Entire text field selected.
                        multibarInput.inputField.text = "";
                    }
                    else
                    {
                        // Part of text field selected, starting at beginning.
                        multibarInput.inputField.text = multibarInput.inputField.text.Substring(multibarInput.selectionEnd + 1);
                    }
                }
                else if (multibarInput.selectionEnd == multibarInput.inputField.text.Length - 1)
                {
                    // Part of text field selected, ending at end.
                    multibarInput.inputField.text = multibarInput.inputField.text.Substring(0, multibarInput.selectionStart);
                }
                else
                {
                    // Part of text field selected, in between beginning and end.
                    multibarInput.inputField.text = multibarInput.inputField.text.Substring(0, multibarInput.selectionStart)
                        + multibarInput.inputField.text.Substring(multibarInput.selectionEnd + 1);
                }
            }
            multibarInput.multibar.rightClickMenu.SetActive(false);
        }

        /// <summary>
        /// Perfom a copy.
        /// </summary>
        public void Copy()
        {
            if (multibarInput.selectionStart != -1 && multibarInput.selectionEnd != -1)
            {
                if (multibarInput.selectionStart == 0)
                {
                    if (multibarInput.selectionEnd == multibarInput.inputField.text.Length - 1)
                    {
                        // Entire text field selected.
                        GUIUtility.systemCopyBuffer = multibarInput.inputField.text;
                    }
                    else
                    {
                        // Part of text field selected, starting at beginning.
                        GUIUtility.systemCopyBuffer = multibarInput.inputField.text.Substring(0, multibarInput.selectionEnd + 1);
                    }
                }
                else if (multibarInput.selectionEnd == multibarInput.inputField.text.Length - 1)
                {
                    // Part of text field selected, ending at end.
                    GUIUtility.systemCopyBuffer = multibarInput.inputField.text.Substring(multibarInput.selectionStart);
                }
                else
                {
                    // Part of text field selected, in between beginning and end.
                    GUIUtility.systemCopyBuffer = multibarInput.inputField.text.Substring(
                        multibarInput.selectionStart, multibarInput.selectionEnd);
                }
            }
            multibarInput.multibar.rightClickMenu.SetActive(false);
        }

        /// <summary>
        /// Perform a paste.
        /// </summary>
        public void Paste()
        {
            if (multibarInput.selectionStart != -1 && multibarInput.selectionEnd != -1)
            {
                if (multibarInput.selectionStart == 0)
                {
                    if (multibarInput.selectionEnd == multibarInput.inputField.text.Length - 1)
                    {
                        // Entire text field selected.
                        multibarInput.inputField.text = GUIUtility.systemCopyBuffer;
                    }
                    else
                    {
                        // Part of text field selected, starting at beginning.
                        multibarInput.inputField.text = GUIUtility.systemCopyBuffer +
                            multibarInput.inputField.text.Substring(multibarInput.selectionEnd + 1);
                    }
                }
                else if (multibarInput.selectionEnd == multibarInput.inputField.text.Length - 1)
                {
                    // Part of text field selected, ending at end.
                    multibarInput.inputField.text = multibarInput.inputField.text.Substring(0, multibarInput.selectionStart)
                        + GUIUtility.systemCopyBuffer;
                }
                else
                {
                    // Part of text field selected, in between beginning and end.
                    multibarInput.inputField.text = multibarInput.inputField.text.Substring(0, multibarInput.selectionStart)
                        + GUIUtility.systemCopyBuffer + multibarInput.inputField.text.Substring(multibarInput.selectionEnd + 1);
                }
            }
            else
            {
                if (multibarInput.inputField.caretPosition < multibarInput.inputField.text.Length &&
                    multibarInput.inputField.caretPosition > 0)
                {
                    multibarInput.inputField.text =
                        multibarInput.inputField.text.Substring(0, multibarInput.inputField.caretPosition)
                        + GUIUtility.systemCopyBuffer + multibarInput.inputField.text.Substring(
                        multibarInput.inputField.caretPosition,
                        multibarInput.inputField.text.Length - multibarInput.inputField.caretPosition);
                }
                else if (multibarInput.inputField.caretPosition == 0)
                {
                    multibarInput.inputField.text = GUIUtility.systemCopyBuffer + multibarInput.inputField.text;
                }
                else if (multibarInput.inputField.caretPosition == multibarInput.inputField.text.Length)
                {
                    multibarInput.inputField.text = multibarInput.inputField.text + GUIUtility.systemCopyBuffer;
                }
            }
            multibarInput.multibar.rightClickMenu.SetActive(false);
        }
    }
}