// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FiveSQD.WebVerse.Input.Desktop
{
    /// <summary>
    /// Class for interpreting Desktop input.
    /// </summary>
    public class DesktopInput : BasePlatformInput
    {
        /// <summary>
        /// Whether gravity is enabled for desktop locomotion.
        /// </summary>
        public bool gravityEnabled { get; set; } = true;

        /// <summary>
        /// Whether WASD motion is enabled for desktop locomotion.
        /// </summary>
        public bool wasdMotionEnabled { get; set; } = true;

        /// <summary>
        /// Whether mouse look is enabled for desktop locomotion.
        /// </summary>
        public bool mouseLookEnabled { get; set; } = true;

        /// <summary>
        /// Whether jump is enabled for desktop locomotion.
        /// </summary>
        public bool jumpEnabled { get; set; } = true;

        /// <summary>
        /// Translation of Unity keys to Javascript standard keys.
        /// </summary>
        private static readonly Dictionary<string, string> keyKeyTranslations = new Dictionary<string, string>()
        {
            { "escape", "Escape"},
            { "f1", "F1" },
            { "f2", "F2" },
            { "f3", "F3" },
            { "f4", "F4" },
            { "f5", "F5" },
            { "f6", "F6" },
            { "f7", "F7" },
            { "f8", "F8" },
            { "f9", "F9" },
            { "f10", "F10" },
            { "f11", "F11" },
            { "f12", "F12" },
            { "delete", "Delete" },
            { "backquote", "`"},
            { "1", "1"},
            { "2", "2"},
            { "3", "3"},
            { "4", "4"},
            { "5", "5"},
            { "6", "6"},
            { "7", "7"},
            { "8", "8"},
            { "9", "9"},
            { "0", "0"},
            { "minus", "-"},
            { "equals", "="},
            { "backspace", "Backspace" },
            { "tab", "Tab"},
            { "q", "q"},
            { "w", "w"},
            { "e", "e"},
            { "r", "r"},
            { "t", "t"},
            { "y", "y"},
            { "u", "u"},
            { "i", "i"},
            { "o", "o"},
            { "p", "p"},
            { "leftBracket", "["},
            { "rightBracket", "]"},
            { "backslash", "\\"},
            { "capsLock", "CapsLock"},
            { "a", "a"},
            { "s", "s"},
            { "d", "d"},
            { "f", "f"},
            { "g", "g"},
            { "h", "h"},
            { "j", "j"},
            { "k", "k"},
            { "l", "l"},
            { "semicolon", ";"},
            { "quote", "'"},
            { "enter", "Enter" },
            { "leftShift", "Shift"},
            { "z", "z"},
            { "x", "x"},
            { "c", "c"},
            { "v", "v"},
            { "b", "b"},
            { "n", "n"},
            { "m", "m"},
            { "comma", ","},
            { "period", "."},
            { "slash", "/"},
            { "rightShift", "Shift"},
            { "leftCtrl", "Control"},
            { "leftAlt", "Alt"},
            { "space", " "},
            { "rightAlt", "Alt"},
            { "contextMenu", "ContextMenu"},
            { "rightCtrl", "Control"},
            { "upArrow", "ArrowUp"},
            { "downArrow", "ArrowDown"},
            { "leftArrow", "ArrowLeft"},
            { "rightArrow", "ArrowRight"},
            { "numpad0", "0" },
            { "numpad1", "1" },
            { "numpad2", "2" },
            { "numpad3", "3" },
            { "numpad4", "4" },
            { "numpad5", "5" },
            { "numpad6", "6" },
            { "numpad7", "7" },
            { "numpad8", "8" },
            { "numpad9", "9" },
            { "numpadPeriod", "." },
            { "numpadDivide", "/" },
            { "numpadMultiply", "*" },
            { "numpadMinus", "-" },
            { "numpadPlus", "+" },
            { "pageUp", "PageUp" },
            { "pageDown", "PageDown" },
            { "home", "Home" },
            { "end", "End" }
        };

        /// <summary>
        /// Translation of Unity keys to Javascript standard key codes.
        /// </summary>
        private static readonly Dictionary<string, string> keyCodeTranslations = new Dictionary<string, string>()
        {
            { "escape", "Escape"},
            { "f1", "F1" },
            { "f2", "F2" },
            { "f3", "F3" },
            { "f4", "F4" },
            { "f5", "F5" },
            { "f6", "F6" },
            { "f7", "F7" },
            { "f8", "F8" },
            { "f9", "F9" },
            { "f10", "F10" },
            { "f11", "F11" },
            { "f12", "F12" },
            { "delete", "Delete" },
            { "backquote", "Backquote"},
            { "1", "Digit1"},
            { "2", "Digit2"},
            { "3", "Digit3"},
            { "4", "Digit4"},
            { "5", "Digit5"},
            { "6", "Digit6"},
            { "7", "Digit7"},
            { "8", "Digit8"},
            { "9", "Digit9"},
            { "0", "Digit0"},
            { "minus", "Minus"},
            { "equals", "Equal"},
            { "backspace", "Backspace" },
            { "tab", "Tab"},
            { "q", "KeyQ"},
            { "w", "KeyW"},
            { "e", "KeyE"},
            { "r", "KeyR"},
            { "t", "KeyT"},
            { "y", "KeyY"},
            { "u", "KeyU"},
            { "i", "KeyI"},
            { "o", "KeyO"},
            { "p", "KeyP"},
            { "leftBracket", "BracketLeft"},
            { "rightBracket", "BracketRight"},
            { "backslash", "Backslash"},
            { "capsLock", "CapsLock"},
            { "a", "KeyA"},
            { "s", "KeyS"},
            { "d", "KeyD"},
            { "f", "KeyF"},
            { "g", "KeyG"},
            { "h", "KeyH"},
            { "j", "KeyJ"},
            { "k", "KeyK"},
            { "l", "KeyL"},
            { "semicolon", "Semicolon"},
            { "quote", "Quote"},
            { "enter", "Enter" },
            { "leftShift", "ShiftLeft"},
            { "z", "KeyZ"},
            { "x", "KeyX"},
            { "c", "KeyC"},
            { "v", "KeyV"},
            { "b", "KeyB"},
            { "n", "KeyN"},
            { "m", "KeyM"},
            { "comma", "Comma"},
            { "period", "Period"},
            { "slash", "Slash"},
            { "rightShift", "ShiftRight"},
            { "leftCtrl", "ControlLeft"},
            { "leftAlt", "AltLeft"},
            { "space", "Space"},
            { "rightAlt", "AltRight"},
            { "contextMenu", "ContextMenu"},
            { "rightCtrl", "ControlRight"},
            { "upArrow", "ArrowUp"},
            { "downArrow", "ArrowDown"},
            { "leftArrow", "ArrowLeft"},
            { "rightArrow", "ArrowRight"},
            { "numpad0", "Numpad0" },
            { "numpad1", "Numpad1" },
            { "numpad2", "Numpad2" },
            { "numpad3", "Numpad3" },
            { "numpad4", "Numpad4" },
            { "numpad5", "Numpad5" },
            { "numpad6", "Numpad6" },
            { "numpad7", "Numpad7" },
            { "numpad8", "Numpad8" },
            { "numpad9", "Numpad9" },
            { "numpadPeriod", "NumpadDecimal" },
            { "numpadDivide", "NumpadDivide" },
            { "numpadMultiply", "NumpadMultiply" },
            { "numpadMinus", "NumpadSubtract" },
            { "numpadPlus", "NumpadAdd" },
            { "pageUp", "PageUp" },
            { "pageDown", "PageDown" },
            { "home", "Home" },
            { "end", "End" }
        };

        /// <summary>
        /// Invoked on a move.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            Vector2 lastValue = WebVerseRuntime.Instance.inputManager.moveValue;
            WebVerseRuntime.Instance.inputManager.moveValue = value;
            if (wasdMotionEnabled)
            {
                // Always apply movement to DesktopRig for continuous movement
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.ApplyMovement(value);
                }
            }

            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.Move(value);
            }
            else if (context.phase == InputActionPhase.Performed)
            {
                if (value != lastValue)
                {
                    WebVerseRuntime.Instance.inputManager.Move(value);
                }
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndMove();
            }
        }

        /// <summary>
        /// Invoked on a look.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLook(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            WebVerseRuntime.Instance.inputManager.lookValue = value;

            if (mouseLookEnabled)
            {
                // Apply look to DesktopRig if available
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.ApplyLook(value);
                }
            }

            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.Look(value);
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLook();
            }
        }

        /// <summary>
        /// Invoked on a keyboard press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnKeyboard(InputAction.CallbackContext context)
        {
            string key = context.control.name;
            
            if (!keyKeyTranslations.ContainsKey(key))
            {
                // This appears to be nominal, particularly with device callbacks.
                //Logging.LogError("[DesktopInput->OnKeyboard] Unable to find key translation.");
                return;
            }

            if (!keyCodeTranslations.ContainsKey(key))
            {
                //Logging.LogError("[DesktopInput->OnKeyboard] Unable to find keyCode translation.");
                return;
            }

            if (context.phase == InputActionPhase.Started)
            {
                // Handle jump input for space key
                if (key == "space" && jumpEnabled)
                {
                    if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                    {
                        WebVerseRuntime.Instance.inputManager.desktopRig.ApplyJumpInput(true);
                    }
                }

                // Handle lower input for shift keys
                if ((key == "leftShift" || key == "rightShift") && jumpEnabled)
                {
                    if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                    {
                        WebVerseRuntime.Instance.inputManager.desktopRig.ApplyLowerInput(true);
                    }
                }

                WebVerseRuntime.Instance.inputManager.Key(keyKeyTranslations[key], keyCodeTranslations[key]);
                WebVerseRuntime.Instance.inputManager.pressedKeys.Add(keyKeyTranslations[key]);
                WebVerseRuntime.Instance.inputManager.pressedKeyCodes.Add(keyCodeTranslations[key]);
            }
            else if (context.phase == InputActionPhase.Performed)
            {
                
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                // Handle jump input release for space key
                if (key == "space" && jumpEnabled)
                {
                    if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                    {
                        WebVerseRuntime.Instance.inputManager.desktopRig.ApplyJumpInput(false);
                    }
                }

                // Handle lower input release for shift keys
                if ((key == "leftShift" || key == "rightShift") && jumpEnabled)
                {
                    if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                    {
                        WebVerseRuntime.Instance.inputManager.desktopRig.ApplyLowerInput(false);
                    }
                }
                
                WebVerseRuntime.Instance.inputManager.EndKey(keyKeyTranslations[key], keyCodeTranslations[key]);
                WebVerseRuntime.Instance.inputManager.pressedKeys.Remove(keyKeyTranslations[key]);
                WebVerseRuntime.Instance.inputManager.pressedKeyCodes.Remove(keyCodeTranslations[key]);
            }
        }

        /// <summary>
        /// Invoked on a left click.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftClick(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.Left();
                WebVerseRuntime.Instance.inputManager.leftValue = true;
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeft();
                WebVerseRuntime.Instance.inputManager.leftValue = false;
            }
        }

        /// <summary>
        /// Invoked on a middle click.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnMiddleClick(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.Middle();
                WebVerseRuntime.Instance.inputManager.middleValue = true;
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndMiddle();
                WebVerseRuntime.Instance.inputManager.middleValue = false;
            }
        }

        /// <summary>
        /// Invoked on a right click.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.Right();
                WebVerseRuntime.Instance.inputManager.rightValue = true;
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRight();
                WebVerseRuntime.Instance.inputManager.rightValue = false;
            }
        }

        /// <summary>
        /// Get a raycast from the pointer.
        /// </summary>
        /// <param name="direction">Direction to cast the ray in.</param>
        /// <param name="pointerIndex">Index of the pointer to get raycast from.</param>
        /// <returns>A raycast from the pointer, or null.</returns>
        public override Tuple<RaycastHit, Vector3> GetPointerRaycast(Vector3 direction, int pointerIndex = 0)
        {
            if (pointerIndex == 0)
            {
                RaycastHit hit;
                Ray ray = StraightFour.StraightFour.ActiveWorld.cameraManager.cam.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;

                    if (objectHit)
                    {
                        return new Tuple<RaycastHit, Vector3>(hit, StraightFour.StraightFour.ActiveWorld.cameraManager.GetPosition(false));
                    }
                }
            }
            else
            {
                Logging.LogWarning("[DesktopInput->GetPointerRaycast] Only indices of 0 are supported for Desktop.");
            }

            return null;
        }
    }
}