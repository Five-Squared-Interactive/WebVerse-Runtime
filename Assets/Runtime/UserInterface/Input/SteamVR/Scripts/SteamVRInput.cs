// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace FiveSQD.WebVerse.Input.SteamVR
{
    /// <summary>
    /// Class for interpreting SteamVR input.
    /// </summary>
    public class SteamVRInput : BasePlatformInput
    {
        /// <summary>
        /// The left controller gameobject.
        /// </summary>
        [Tooltip("The left controller gameobject.")]
        public GameObject leftControllerGO;

        /// <summary>
        /// The right controller gameobject.
        /// </summary>
        [Tooltip("The right controller gameobject.")]
        public GameObject rightControllerGO;

        /// <summary>
        /// The left controller.
        /// </summary>
        [Tooltip("The left controller.")]
        public XRController leftController;

        /// <summary>
        /// The right controller.
        /// </summary>
        [Tooltip("The right controller.")]
        public XRController rightController;

        /// <summary>
        /// Invoked on a left menu.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftMenu(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftMenu();
                WebVerseRuntime.Instance.inputManager.leftMenuValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightMenuValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.Menu();
                }
                //leftMenuHoldTime = 0;
                //leftMenuHoldActivated = false;
            }
            else if (context.phase == InputActionPhase.Performed)
            {
                
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftMenu();
                WebVerseRuntime.Instance.inputManager.leftMenuValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightMenuValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndMenu();
                }
                //leftMenuHoldTime = -1;
            }
        }

        /// <summary>
        /// Invoked on a right menu.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightMenu(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightMenu();
                WebVerseRuntime.Instance.inputManager.rightMenuValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftMenuValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.Menu();
                }
                //rightMenuHoldTime = 0;
                //rightMenuHoldActivated = false;
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightMenu();
                WebVerseRuntime.Instance.inputManager.rightMenuValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftMenuValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndMenu();
                }
                //rightMenuHoldTime -= 1;
            }
        }

        /// <summary>
        /// Invoked on a left trigger touch.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftTriggerTouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftTriggerTouch();
                WebVerseRuntime.Instance.inputManager.leftTriggerTouchValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightTriggerTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TriggerTouch();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftTriggerTouch();
                WebVerseRuntime.Instance.inputManager.leftTriggerTouchValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightTriggerTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndTriggerTouch();
                }
            }
        }

        /// <summary>
        /// Invoked on a right trigger touch.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightTriggerTouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightTriggerTouch();
                WebVerseRuntime.Instance.inputManager.rightTriggerTouchValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftTriggerTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TriggerTouch();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightTriggerTouch();
                WebVerseRuntime.Instance.inputManager.rightTriggerTouchValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftTriggerTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndTriggerTouch();
                }
            }
        }

        /// <summary>
        /// Invoked on a left trigger press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftTriggerPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftTriggerPress();
                WebVerseRuntime.Instance.inputManager.leftTriggerPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightTriggerPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TriggerPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftTriggerPress();
                WebVerseRuntime.Instance.inputManager.leftTriggerPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightTriggerPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndTriggerPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a right trigger press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightTriggerPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightTriggerPress();
                WebVerseRuntime.Instance.inputManager.rightTriggerPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftTriggerPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TriggerPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightTriggerPress();
                WebVerseRuntime.Instance.inputManager.rightTriggerPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftTriggerPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndTriggerPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a left grip press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftGripPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftGripPress();
                WebVerseRuntime.Instance.inputManager.leftGripPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightGripPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.GripPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftGripPress();
                WebVerseRuntime.Instance.inputManager.leftGripPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightGripPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndGripPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a right grip press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightGripPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightGripPress();
                WebVerseRuntime.Instance.inputManager.rightGripPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftGripPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.GripPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightGripPress();
                WebVerseRuntime.Instance.inputManager.rightGripPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftGripPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndGripPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a left touchpad touch location change.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftTouchPadValue(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            WebVerseRuntime.Instance.inputManager.LeftTouchPadTouchValueChange(value);
            WebVerseRuntime.Instance.inputManager.leftTouchPadTouchLocation = value;
        }

        /// <summary>
        /// Invoked on a right touchpad touch location change.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightTouchPadValue(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            WebVerseRuntime.Instance.inputManager.RightTouchPadTouchValueChange(value);
            WebVerseRuntime.Instance.inputManager.rightTouchPadTouchLocation = value;
        }

        /// <summary>
        /// Invoked on a left touchpad touch.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftTouchPadTouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftTouchPadTouch();
                WebVerseRuntime.Instance.inputManager.leftTouchPadTouchValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightTouchPadTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TouchPadTouch();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftTouchPadTouch();
                WebVerseRuntime.Instance.inputManager.leftTouchPadTouchValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightTouchPadTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndTouchPadTouch();
                }
            }
        }

        /// <summary>
        /// Invoked on a right touchpad touch.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightTouchPadTouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightTouchPadTouch();
                WebVerseRuntime.Instance.inputManager.rightTouchPadTouchValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftTouchPadTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TouchPadTouch();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightTouchPadTouch();
                WebVerseRuntime.Instance.inputManager.rightTouchPadTouchValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftTouchPadTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndTouchPadTouch();
                }
            }
        }

        /// <summary>
        /// Invoked on a left touchpad press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftTouchPadPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftTouchPadPress();
                WebVerseRuntime.Instance.inputManager.leftTouchPadPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightTouchPadPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TouchPadPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftTouchPadPress();
                WebVerseRuntime.Instance.inputManager.leftTouchPadPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightTouchPadPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndTouchPadPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a right touchpad press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightTouchPadPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightTouchPadPress();
                WebVerseRuntime.Instance.inputManager.rightTouchPadPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftTouchPadPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TouchPadPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightTouchPadPress();
                WebVerseRuntime.Instance.inputManager.rightTouchPadPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftTouchPadPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndTouchPadPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a left primary touch.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftPrimaryTouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftPrimaryTouch();
                WebVerseRuntime.Instance.inputManager.leftPrimaryTouchValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightPrimaryTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.PrimaryTouch();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftPrimaryTouch();
                WebVerseRuntime.Instance.inputManager.leftPrimaryTouchValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightPrimaryTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndPrimaryTouch();
                }
            }
        }

        /// <summary>
        /// Invoked on a right primary touch.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightPrimaryTouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightPrimaryTouch();
                WebVerseRuntime.Instance.inputManager.rightPrimaryTouchValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftPrimaryTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.PrimaryTouch();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightPrimaryTouch();
                WebVerseRuntime.Instance.inputManager.rightPrimaryTouchValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftPrimaryTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndPrimaryTouch();
                }
            }
        }

        /// <summary>
        /// Invoked on a left primary press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftPrimaryPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftPrimaryPress();
                WebVerseRuntime.Instance.inputManager.leftPrimaryPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightPrimaryPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.PrimaryPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftPrimaryPress();
                WebVerseRuntime.Instance.inputManager.leftPrimaryPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightPrimaryPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndPrimaryPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a right primary press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightPrimaryPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightPrimaryPress();
                WebVerseRuntime.Instance.inputManager.rightPrimaryPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftPrimaryPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TouchPadPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightPrimaryPress();
                WebVerseRuntime.Instance.inputManager.rightPrimaryPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftPrimaryPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndPrimaryPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a left secondary touch.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftSecondaryTouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftSecondaryTouch();
                WebVerseRuntime.Instance.inputManager.leftSecondaryTouchValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightSecondaryTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.SecondaryTouch();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftSecondaryTouch();
                WebVerseRuntime.Instance.inputManager.leftSecondaryTouchValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightSecondaryTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndSecondaryTouch();
                }
            }
        }

        /// <summary>
        /// Invoked on a right secondary touch.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightSecondaryTouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightSecondaryTouch();
                WebVerseRuntime.Instance.inputManager.rightSecondaryTouchValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftSecondaryTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.SecondaryTouch();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightSecondaryTouch();
                WebVerseRuntime.Instance.inputManager.rightSecondaryTouchValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftSecondaryTouchValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndSecondaryTouch();
                }
            }
        }

        /// <summary>
        /// Invoked on a left secondary press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftSecondaryPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.LeftSecondaryPress();
                WebVerseRuntime.Instance.inputManager.leftSecondaryPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.rightSecondaryPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.SecondaryPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftSecondaryPress();
                WebVerseRuntime.Instance.inputManager.leftSecondaryPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.rightSecondaryPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndSecondaryPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a right secondary press.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightSecondaryPress(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                WebVerseRuntime.Instance.inputManager.RightSecondaryPress();
                WebVerseRuntime.Instance.inputManager.rightSecondaryPressValue = true;
                if (WebVerseRuntime.Instance.inputManager.leftSecondaryPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.TouchPadPress();
                }
            }
            else if (context.phase == InputActionPhase.Performed)
            {

            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightSecondaryPress();
                WebVerseRuntime.Instance.inputManager.rightSecondaryPressValue = false;
                if (WebVerseRuntime.Instance.inputManager.leftSecondaryPressValue == false)
                {
                    WebVerseRuntime.Instance.inputManager.EndSecondaryPress();
                }
            }
        }

        /// <summary>
        /// Invoked on a left stick.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnLeftStick(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                Vector2 value = context.ReadValue<Vector2>();
                WebVerseRuntime.Instance.inputManager.LeftStick();
                WebVerseRuntime.Instance.inputManager.LeftStickValueChange(value);
                WebVerseRuntime.Instance.inputManager.leftStickValue = true;
            }
            else if (context.phase == InputActionPhase.Performed)
            {
                Vector2 value = context.ReadValue<Vector2>();
                WebVerseRuntime.Instance.inputManager.LeftStickValueChange(value);
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndLeftStick();
                WebVerseRuntime.Instance.inputManager.LeftStickValueChange(Vector2.zero);
                WebVerseRuntime.Instance.inputManager.leftStickValue = false;
            }
        }

        /// <summary>
        /// Invoked on a right stick.
        /// </summary>
        /// <param name="context">Callback context.</param>
        public void OnRightStick(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                Vector2 value = context.ReadValue<Vector2>();
                WebVerseRuntime.Instance.inputManager.RightStick();
                WebVerseRuntime.Instance.inputManager.LeftStickValueChange(value);
                WebVerseRuntime.Instance.inputManager.rightStickValue = true;
            }
            else if (context.phase == InputActionPhase.Performed)
            {
                Vector2 value = context.ReadValue<Vector2>();
                WebVerseRuntime.Instance.inputManager.RightStickValueChange(value);
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                WebVerseRuntime.Instance.inputManager.EndRightStick();
                WebVerseRuntime.Instance.inputManager.LeftStickValueChange(Vector2.zero);
                WebVerseRuntime.Instance.inputManager.rightStickValue = false;
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
                if (Physics.Raycast(leftControllerGO.transform.position,
                    rightControllerGO.transform.rotation * direction, out hit))
                {
                    return new Tuple<RaycastHit, Vector3>(hit, leftControllerGO.transform.position);
                }
            }
            else if (pointerIndex == 1)
            {
                RaycastHit hit;
                if (Physics.Raycast(rightControllerGO.transform.position,
                    rightControllerGO.transform.rotation * direction, out hit))
                {
                    return new Tuple<RaycastHit, Vector3>(hit, rightControllerGO.transform.position);
                }
            }
            else
            {
                Logging.LogWarning("[SteamVRInput->GetPointerRaycast] Only indices of 0 (left) or 1 (right)" +
                    " are supported for SteamVR.");
            }

            return null;
        }
    }
}