// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Input.SteamVR;
using FiveSQD.WebVerse.Input.Desktop;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Class for the Input Manager.
    /// </summary>
    public class InputManager : BaseManager
    {
        /// <summary>
        /// Platform input.
        /// </summary>
        public BasePlatformInput platformInput;

        /// <summary>
        /// VR rig.
        /// </summary>
        public VRRig vRRig;

        /// <summary>
        /// Desktop rig.
        /// </summary>
        public DesktopRig desktopRig;

        /// <summary>
        /// Whether or not input is enabled.
        /// </summary>
        public bool inputEnabled;

        /// <summary>
        /// The move value.
        /// </summary>
        public Vector2 moveValue;

        /// <summary>
        /// The look value.
        /// </summary>
        public Vector2 lookValue;

        /// <summary>
        /// Pressed keys.
        /// </summary>
        public List<string> pressedKeys;

        /// <summary>
        /// Keycodes of pressed keys.
        /// </summary>
        public List<string> pressedKeyCodes;

        /// <summary>
        /// The left value.
        /// </summary>
        public bool leftValue;

        /// <summary>
        /// The middle value.
        /// </summary>
        public bool middleValue;

        /// <summary>
        /// The right value.
        /// </summary>
        public bool rightValue;

        /// <summary>
        /// The left menu value.
        /// </summary>
        public bool leftMenuValue;

        /// <summary>
        /// The right menu value.
        /// </summary>
        public bool rightMenuValue;

        /// <summary>
        /// The menu value.
        /// </summary>
        public bool menuValue
        {
            get
            {
                return leftMenuValue || rightMenuValue;
            }
        }

        /// <summary>
        /// The left trigger touch value.
        /// </summary>
        public bool leftTriggerTouchValue;

        /// <summary>
        /// The right trigger touch value.
        /// </summary>
        public bool rightTriggerTouchValue;

        /// <summary>
        /// The trigger touch value.
        /// </summary>
        public bool triggerTouchValue
        {
            get
            {
                return leftTriggerTouchValue || rightTriggerTouchValue;
            }
        }

        /// <summary>
        /// The left trigger press value.
        /// </summary>
        public bool leftTriggerPressValue;

        /// <summary>
        /// The right trigger press value.
        /// </summary>
        public bool rightTriggerPressValue;

        /// <summary>
        /// The trigger press value.
        /// </summary>
        public bool triggerPressValue
        {
            get
            {
                return leftTriggerPressValue || rightTriggerPressValue;
            }
        }

        /// <summary>
        /// The left grip press value.
        /// </summary>
        public bool leftGripPressValue;

        /// <summary>
        /// The right grip press value.
        /// </summary>
        public bool rightGripPressValue;

        /// <summary>
        /// The grip press value.
        /// </summary>
        public bool gripPressValue
        {
            get
            {
                return leftGripPressValue || rightGripPressValue;
            }
        }

        /// <summary>
        /// The left touchpad touch value.
        /// </summary>
        public bool leftTouchPadTouchValue;

        /// <summary>
        /// The right touchpad touch value.
        /// </summary>
        public bool rightTouchPadTouchValue;

        /// <summary>
        /// The left touchpad touch location.
        /// </summary>
        public Vector2 leftTouchPadTouchLocation;

        /// <summary>
        /// The right touchpad touch location.
        /// </summary>
        public Vector2 rightTouchPadTouchLocation;

        /// <summary>
        /// The touchpad touch value.
        /// </summary>
        public bool touchPadTouchValue
        {
            get
            {
                return leftTouchPadTouchValue || rightTouchPadTouchValue;
            }
        }

        /// <summary>
        /// The left touchpad press value.
        /// </summary>
        public bool leftTouchPadPressValue;

        /// <summary>
        /// The right touchpad press value.
        /// </summary>
        public bool rightTouchPadPressValue;

        /// <summary>
        /// The touchpad press value.
        /// </summary>
        public bool touchPadPressValue
        {
            get
            {
                return leftTouchPadPressValue || rightTouchPadPressValue;
            }
        }

        /// <summary>
        /// The left primary touch value.
        /// </summary>
        public bool leftPrimaryTouchValue;

        /// <summary>
        /// The right primary touch value.
        /// </summary>
        public bool rightPrimaryTouchValue;

        /// <summary>
        /// The primary touch value.
        /// </summary>
        public bool primaryTouchValue
        {
            get
            {
                return leftPrimaryTouchValue || rightPrimaryTouchValue;
            }
        }

        /// <summary>
        /// The left primary press value.
        /// </summary>
        public bool leftPrimaryPressValue;

        /// <summary>
        /// The right primary press value.
        /// </summary>
        public bool rightPrimaryPressValue;

        /// <summary>
        /// The primary press value.
        /// </summary>
        public bool primaryPressValue
        {
            get
            {
                return leftPrimaryPressValue || rightPrimaryPressValue;
            }
        }

        /// <summary>
        /// The left secondary touch value.
        /// </summary>
        public bool leftSecondaryTouchValue;

        /// <summary>
        /// The right secondary touch value.
        /// </summary>
        public bool rightSecondaryTouchValue;

        /// <summary>
        /// The secondary touch value.
        /// </summary>
        public bool secondaryTouchValue
        {
            get
            {
                return leftSecondaryTouchValue || rightSecondaryTouchValue;
            }
        }

        /// <summary>
        /// The left secondary press value.
        /// </summary>
        public bool leftSecondaryPressValue;

        /// <summary>
        /// The right secondary press value.
        /// </summary>
        public bool rightSecondaryPressValue;

        /// <summary>
        /// The touchpad press value.
        /// </summary>
        public bool secondaryPressValue
        {
            get
            {
                return leftSecondaryPressValue || rightSecondaryPressValue;
            }
        }

        /// <summary>
        /// The left stick value.
        /// </summary>
        public bool leftStickValue;

        /// <summary>
        /// The right stick value.
        /// </summary>
        public bool rightStickValue;

        /// <summary>
        /// Move functions.
        /// </summary>
        private List<string> moveFunctions;

        /// <summary>
        /// End Move functions.
        /// </summary>
        private List<string> endMoveFunctions;

        /// <summary>
        /// Look functions.
        /// </summary>
        private List<string> lookFunctions;

        /// <summary>
        /// End Look functions.
        /// </summary>
        private List<string> endLookFunctions;

        /// <summary>
        /// Key functions.
        /// </summary>
        private List<string> keyFunctions;

        /// <summary>
        /// Key Code functions.
        /// </summary>
        private List<string> keyCodeFunctions;

        /// <summary>
        /// End Key functions.
        /// </summary>
        private List<string> endKeyFunctions;

        /// <summary>
        /// End Key Code functions.
        /// </summary>
        private List<string> endKeyCodeFunctions;

        /// <summary>
        /// Left functions.
        /// </summary>
        private List<string> leftFunctions;

        /// <summary>
        /// End Left functions.
        /// </summary>
        private List<string> endLeftFunctions;

        /// <summary>
        /// Middle functions.
        /// </summary>
        private List<string> middleFunctions;

        /// <summary>
        /// End Middle functions.
        /// </summary>
        private List<string> endMiddleFunctions;

        /// <summary>
        /// Right functions.
        /// </summary>
        private List<string> rightFunctions;

        /// <summary>
        /// End Right functions.
        /// </summary>
        private List<string> endRightFunctions;

        /// <summary>
        /// Left Menu functions.
        /// </summary>
        private List<string> leftMenuFunctions;

        /// <summary>
        /// End Left Menu functions.
        /// </summary>
        private List<string> endLeftMenuFunctions;

        /// <summary>
        /// Right Menu functions.
        /// </summary>
        private List<string> rightMenuFunctions;

        /// <summary>
        /// End Right Menu functions.
        /// </summary>
        private List<string> endRightMenuFunctions;

        /// <summary>
        /// Menu functions.
        /// </summary>
        private List<string> menuFunctions;

        /// <summary>
        /// End Menu functions.
        /// </summary>
        private List<string> endMenuFunctions;

        /// <summary>
        /// Left Trigger Touch functions.
        /// </summary>
        private List<string> leftTriggerTouchFunctions;

        /// <summary>
        /// End Left Trigger Touch functions.
        /// </summary>
        private List<string> endLeftTriggerTouchFunctions;

        /// <summary>
        /// Right Trigger Touch functions.
        /// </summary>
        private List<string> rightTriggerTouchFunctions;

        /// <summary>
        /// End Right Trigger Touch functions.
        /// </summary>
        private List<string> endRightTriggerTouchFunctions;

        /// <summary>
        /// Trigger Touch functions.
        /// </summary>
        private List<string> triggerTouchFunctions;

        /// <summary>
        /// End Trigger Touch functions.
        /// </summary>
        private List<string> endTriggerTouchFunctions;

        /// <summary>
        /// Left Trigger Press functions.
        /// </summary>
        private List<string> leftTriggerPressFunctions;

        /// <summary>
        /// End Left Trigger Press functions.
        /// </summary>
        private List<string> endLeftTriggerPressFunctions;

        /// <summary>
        /// Right Trigger Press functions.
        /// </summary>
        private List<string> rightTriggerPressFunctions;

        /// <summary>
        /// End Right Trigger Press functions.
        /// </summary>
        private List<string> endRightTriggerPressFunctions;

        /// <summary>
        /// Trigger Press functions.
        /// </summary>
        private List<string> triggerPressFunctions;

        /// <summary>
        /// End Trigger Press functions.
        /// </summary>
        private List<string> endTriggerPressFunctions;

        /// <summary>
        /// Left Grip Press functions.
        /// </summary>
        private List<string> leftGripPressFunctions;

        /// <summary>
        /// End Left Grip Press functions.
        /// </summary>
        private List<string> endLeftGripPressFunctions;

        /// <summary>
        /// Right Grip Press functions.
        /// </summary>
        private List<string> rightGripPressFunctions;

        /// <summary>
        /// End Right Grip Press functions.
        /// </summary>
        private List<string> endRightGripPressFunctions;

        /// <summary>
        /// Grip Press functions.
        /// </summary>
        private List<string> gripPressFunctions;

        /// <summary>
        /// End Grip Press functions.
        /// </summary>
        private List<string> endGripPressFunctions;

        /// <summary>
        /// Left TouchPad Touch functions.
        /// </summary>
        private List<string> leftTouchPadTouchFunctions;

        /// <summary>
        /// End Left TouchPad Touch functions.
        /// </summary>
        private List<string> endLeftTouchPadTouchFunctions;

        /// <summary>
        /// Left TouchPad Value Change functions.
        /// </summary>
        private List<string> leftTouchPadValueChangeFunctions;

        /// <summary>
        /// Right TouchPad Touch functions.
        /// </summary>
        private List<string> rightTouchPadTouchFunctions;

        /// <summary>
        /// End Right TouchPad Touch functions.
        /// </summary>
        private List<string> endRightTouchPadTouchFunctions;

        /// <summary>
        /// Left TouchPad Value Change functions.
        /// </summary>
        private List<string> rightTouchPadValueChangeFunctions;

        /// <summary>
        /// TouchPad Touch functions.
        /// </summary>
        private List<string> touchPadTouchFunctions;

        /// <summary>
        /// End TouchPad Touch functions.
        /// </summary>
        private List<string> endTouchPadTouchFunctions;

        /// <summary>
        /// Left TouchPad Press functions.
        /// </summary>
        private List<string> leftTouchPadPressFunctions;

        /// <summary>
        /// End Left TouchPad Press functions.
        /// </summary>
        private List<string> endLeftTouchPadPressFunctions;

        /// <summary>
        /// Right TouchPad Press functions.
        /// </summary>
        private List<string> rightTouchPadPressFunctions;

        /// <summary>
        /// End Right TouchPad Press functions.
        /// </summary>
        private List<string> endRightTouchPadPressFunctions;

        /// <summary>
        /// TouchPad Press functions.
        /// </summary>
        private List<string> touchPadPressFunctions;

        /// <summary>
        /// End TouchPad Press functions.
        /// </summary>
        private List<string> endTouchPadPressFunctions;

        /// <summary>
        /// Left Primary Touch functions.
        /// </summary>
        private List<string> leftPrimaryTouchFunctions;

        /// <summary>
        /// End Left Primary Touch functions.
        /// </summary>
        private List<string> endLeftPrimaryTouchFunctions;

        /// <summary>
        /// Right Primary Touch functions.
        /// </summary>
        private List<string> rightPrimaryTouchFunctions;

        /// <summary>
        /// End Right Primary Touch functions.
        /// </summary>
        private List<string> endRightPrimaryTouchFunctions;

        /// <summary>
        /// Primary Touch functions.
        /// </summary>
        private List<string> primaryTouchFunctions;

        /// <summary>
        /// End Primary Touch functions.
        /// </summary>
        private List<string> endPrimaryTouchFunctions;

        /// <summary>
        /// Left Primary Press functions.
        /// </summary>
        private List<string> leftPrimaryPressFunctions;

        /// <summary>
        /// End Left Primary Press functions.
        /// </summary>
        private List<string> endLeftPrimaryPressFunctions;

        /// <summary>
        /// Right Primary Press functions.
        /// </summary>
        private List<string> rightPrimaryPressFunctions;

        /// <summary>
        /// End Right Primary Press functions.
        /// </summary>
        private List<string> endRightPrimaryPressFunctions;

        /// <summary>
        /// Primary Press functions.
        /// </summary>
        private List<string> primaryPressFunctions;

        /// <summary>
        /// End Primary Press functions.
        /// </summary>
        private List<string> endPrimaryPressFunctions;

        /// <summary>
        /// Left Secondary Touch functions.
        /// </summary>
        private List<string> leftSecondaryTouchFunctions;

        /// <summary>
        /// End Left Secondary Touch functions.
        /// </summary>
        private List<string> endLeftSecondaryTouchFunctions;

        /// <summary>
        /// Right Secondary Touch functions.
        /// </summary>
        private List<string> rightSecondaryTouchFunctions;

        /// <summary>
        /// End Right Secondary Touch functions.
        /// </summary>
        private List<string> endRightSecondaryTouchFunctions;

        /// <summary>
        /// Secondary Touch functions.
        /// </summary>
        private List<string> secondaryTouchFunctions;

        /// <summary>
        /// End Secondary Touch functions.
        /// </summary>
        private List<string> endSecondaryTouchFunctions;

        /// <summary>
        /// Left Secondary Press functions.
        /// </summary>
        private List<string> leftSecondaryPressFunctions;

        /// <summary>
        /// End Left Secondary Press functions.
        /// </summary>
        private List<string> endLeftSecondaryPressFunctions;

        /// <summary>
        /// Right Secondary Press functions.
        /// </summary>
        private List<string> rightSecondaryPressFunctions;

        /// <summary>
        /// End Right Secondary Press functions.
        /// </summary>
        private List<string> endRightSecondaryPressFunctions;

        /// <summary>
        /// Secondary Press functions.
        /// </summary>
        private List<string> secondaryPressFunctions;

        /// <summary>
        /// End Secondary Press functions.
        /// </summary>
        private List<string> endSecondaryPressFunctions;

        /// <summary>
        /// Left Stick functions.
        /// </summary>
        private List<string> leftStickFunctions;

        /// <summary>
        /// End Left Stick functions.
        /// </summary>
        private List<string> endLeftStickFunctions;

        /// <summary>
        /// Left Stick Value Change functions.
        /// </summary>
        private List<string> leftStickValueChangeFunctions;

        /// <summary>
        /// Right Stick functions.
        /// </summary>
        private List<string> rightStickFunctions;

        /// <summary>
        /// End Right Stick functions.
        /// </summary>
        private List<string> endRightStickFunctions;

        /// <summary>
        /// Right Stick Value Change functions.
        /// </summary>
        private List<string> rightStickValueChangeFunctions;

        /// <summary>
        /// Initialize the Input Manager.
        /// </summary>
        public override void Initialize()
        {
            Reset();

            base.Initialize();
        }

        /// <summary>
        /// Reset the Input Manager.
        /// </summary>
        public void Reset()
        {
            if (vRRig != null)
            {
                vRRig.Terminate();
                vRRig.Initialize();
            }

            if (desktopRig != null)
            {
                desktopRig.Terminate();
                desktopRig.Initialize();
            }

            inputEnabled = false;

            moveFunctions = new List<string>();
            endMoveFunctions = new List<string>();
            lookFunctions = new List<string>();
            endLookFunctions = new List<string>();
            keyFunctions = new List<string>();
            keyCodeFunctions = new List<string>();
            endKeyFunctions = new List<string>();
            endKeyCodeFunctions = new List<string>();
            pressedKeys = new List<string>();
            pressedKeyCodes = new List<string>();
            leftFunctions = new List<string>();
            endLeftFunctions = new List<string>();
            middleFunctions = new List<string>();
            endMiddleFunctions = new List<string>();
            rightFunctions = new List<string>();
            endRightFunctions = new List<string>();
            leftMenuFunctions = new List<string>();
            endLeftMenuFunctions = new List<string>();
            rightMenuFunctions = new List<string>();
            endRightMenuFunctions = new List<string>();
            menuFunctions = new List<string>();
            endMenuFunctions = new List<string>();
            leftTriggerTouchFunctions = new List<string>();
            endLeftTriggerTouchFunctions = new List<string>();
            rightTriggerTouchFunctions = new List<string>();
            endRightTriggerTouchFunctions = new List<string>();
            triggerTouchFunctions = new List<string>();
            endTriggerTouchFunctions = new List<string>();
            leftTriggerPressFunctions = new List<string>();
            endLeftTriggerPressFunctions = new List<string>();
            rightTriggerPressFunctions = new List<string>();
            endRightTriggerPressFunctions = new List<string>();
            triggerPressFunctions = new List<string>();
            endTriggerPressFunctions = new List<string>();
            leftGripPressFunctions = new List<string>();
            endLeftGripPressFunctions = new List<string>();
            rightGripPressFunctions = new List<string>();
            endRightGripPressFunctions = new List<string>();
            gripPressFunctions = new List<string>();
            endGripPressFunctions = new List<string>();
            leftTouchPadTouchFunctions = new List<string>();
            endLeftTouchPadTouchFunctions = new List<string>();
            leftTouchPadValueChangeFunctions = new List<string>();
            rightTouchPadTouchFunctions = new List<string>();
            endRightTouchPadTouchFunctions = new List<string>();
            rightTouchPadValueChangeFunctions = new List<string>();
            touchPadTouchFunctions = new List<string>();
            endTouchPadTouchFunctions = new List<string>();
            leftTouchPadPressFunctions = new List<string>();
            endLeftTouchPadPressFunctions = new List<string>();
            rightTouchPadPressFunctions = new List<string>();
            endRightTouchPadPressFunctions = new List<string>();
            touchPadPressFunctions = new List<string>();
            endTouchPadPressFunctions = new List<string>();
            leftPrimaryTouchFunctions = new List<string>();
            endLeftPrimaryTouchFunctions = new List<string>();
            rightPrimaryTouchFunctions = new List<string>();
            endRightPrimaryTouchFunctions = new List<string>();
            primaryTouchFunctions = new List<string>();
            endPrimaryTouchFunctions = new List<string>();
            leftPrimaryPressFunctions = new List<string>();
            endLeftPrimaryPressFunctions = new List<string>();
            rightPrimaryPressFunctions = new List<string>();
            endRightPrimaryPressFunctions = new List<string>();
            primaryPressFunctions = new List<string>();
            endPrimaryPressFunctions = new List<string>();
            leftSecondaryTouchFunctions = new List<string>();
            endLeftSecondaryTouchFunctions = new List<string>();
            rightSecondaryTouchFunctions = new List<string>();
            endRightSecondaryTouchFunctions = new List<string>();
            secondaryTouchFunctions = new List<string>();
            endSecondaryTouchFunctions = new List<string>();
            leftSecondaryPressFunctions = new List<string>();
            endLeftSecondaryPressFunctions = new List<string>();
            rightSecondaryPressFunctions = new List<string>();
            endRightSecondaryPressFunctions = new List<string>();
            secondaryPressFunctions = new List<string>();
            endSecondaryPressFunctions = new List<string>();
            leftStickFunctions = new List<string>();
            endLeftStickFunctions = new List<string>();
            leftStickValueChangeFunctions = new List<string>();
            rightStickFunctions = new List<string>();
            endRightStickFunctions = new List<string>();
            rightStickValueChangeFunctions = new List<string>();
        }

        /// <summary>
        /// Terminate the Input Manager.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();
        }

        /// <summary>
        /// Register an Input Event.
        /// </summary>
        /// <param name="eventName">Name of the Input Event.</param>
        /// <param name="call">Logic to call on invocation of Input Event.</param>
        public void RegisterInputEvent(string eventName, string call)
        {
            switch (eventName.ToLower())
            {
                case "move":
                    moveFunctions.Add(call);
                    break;

                case "endmove":
                    endMoveFunctions.Add(call);
                    break;

                case "look":
                    lookFunctions.Add(call);
                    break;

                case "endlook":
                    endLookFunctions.Add(call);
                    break;

                case "key":
                    keyFunctions.Add(call);
                    break;

                case "endkey":
                    endKeyFunctions.Add(call);
                    break;

                case "keycode":
                    keyCodeFunctions.Add(call);
                    break;

                case "endkeycode":
                    endKeyCodeFunctions.Add(call);
                    break;

                case "left":
                    leftFunctions.Add(call);
                    break;

                case "endleft":
                    endLeftFunctions.Add(call);
                    break;

                case "middle":
                    middleFunctions.Add(call);
                    break;

                case "endmiddle":
                    endMiddleFunctions.Add(call);
                    break;

                case "right":
                    rightFunctions.Add(call);
                    break;

                case "endright":
                    endRightFunctions.Add(call);
                    break;

                case "leftmenu":
                    leftMenuFunctions.Add(call);
                    break;

                case "endleftmenu":
                    endLeftMenuFunctions.Add(call);
                    break;

                case "rightmenu":
                    rightMenuFunctions.Add(call);
                    break;

                case "endrightmenu":
                    endRightMenuFunctions.Add(call);
                    break;

                case "menu":
                    menuFunctions.Add(call);
                    break;

                case "endmenu":
                    endMenuFunctions.Add(call);
                    break;

                case "lefttriggertouch":
                    leftTriggerTouchFunctions.Add(call);
                    break;

                case "endlefttriggertouch":
                    endLeftTriggerTouchFunctions.Add(call);
                    break;

                case "righttriggertouch":
                    rightTriggerTouchFunctions.Add(call);
                    break;

                case "endrighttriggertouch":
                    endRightTriggerTouchFunctions.Add(call);
                    break;

                case "triggertouch":
                    triggerTouchFunctions.Add(call);
                    break;

                case "endtriggertouch":
                    endTriggerTouchFunctions.Add(call);
                    break;

                case "lefttriggerpress":
                    leftTriggerPressFunctions.Add(call);
                    break;

                case "endlefttriggerpress":
                    endLeftTriggerPressFunctions.Add(call);
                    break;

                case "righttriggerpress":
                    rightTriggerPressFunctions.Add(call);
                    break;

                case "endrighttriggerpress":
                    endRightTriggerPressFunctions.Add(call);
                    break;

                case "triggerpress":
                    triggerPressFunctions.Add(call);
                    break;

                case "endtriggerpress":
                    endTriggerPressFunctions.Add(call);
                    break;

                case "leftgrippress":
                    leftGripPressFunctions.Add(call);
                    break;

                case "endleftgrippress":
                    endLeftGripPressFunctions.Add(call);
                    break;

                case "rightgrippress":
                    rightGripPressFunctions.Add(call);
                    break;

                case "endrightgrippress":
                    endRightGripPressFunctions.Add(call);
                    break;

                case "grippress":
                    gripPressFunctions.Add(call);
                    break;

                case "endgrippress":
                    endGripPressFunctions.Add(call);
                    break;

                case "lefttouchpadtouch":
                    leftTouchPadTouchFunctions.Add(call);
                    break;

                case "endlefttouchpadtouch":
                    endLeftTouchPadTouchFunctions.Add(call);
                    break;

                case "lefttouchpadvaluechange":
                    leftTouchPadValueChangeFunctions.Add(call);
                    break;

                case "righttouchpadtouch":
                    rightTouchPadTouchFunctions.Add(call);
                    break;

                case "endrighttouchpadtouch":
                    endRightTouchPadTouchFunctions.Add(call);
                    break;

                case "righttouchpadvaluechange":
                    rightTouchPadValueChangeFunctions.Add(call);
                    break;

                case "touchpadtouch":
                    touchPadTouchFunctions.Add(call);
                    break;

                case "endtouchpadtouch":
                    endTouchPadTouchFunctions.Add(call);
                    break;

                case "lefttouchpadpress":
                    leftTouchPadPressFunctions.Add(call);
                    break;

                case "endlefttouchpadpress":
                    endLeftTouchPadPressFunctions.Add(call);
                    break;

                case "righttouchpadpress":
                    rightTouchPadPressFunctions.Add(call);
                    break;

                case "endrighttouchpadpress":
                    endRightTouchPadPressFunctions.Add(call);
                    break;

                case "touchpadpress":
                    touchPadPressFunctions.Add(call);
                    break;

                case "endtouchpadpress":
                    endTouchPadPressFunctions.Add(call);
                    break;

                case "leftprimarytouch":
                    leftPrimaryTouchFunctions.Add(call);
                    break;

                case "endleftprimarytouch":
                    endLeftPrimaryTouchFunctions.Add(call);
                    break;

                case "rightprimarytouch":
                    rightPrimaryTouchFunctions.Add(call);
                    break;

                case "endrightprimarytouch":
                    endRightPrimaryTouchFunctions.Add(call);
                    break;

                case "primarytouch":
                    primaryTouchFunctions.Add(call);
                    break;

                case "endprimarytouch":
                    endPrimaryTouchFunctions.Add(call);
                    break;

                case "leftprimarypress":
                    leftPrimaryPressFunctions.Add(call);
                    break;

                case "endleftprimarypress":
                    endLeftPrimaryPressFunctions.Add(call);
                    break;

                case "rightprimarypress":
                    rightPrimaryPressFunctions.Add(call);
                    break;

                case "endrightprimarypress":
                    endRightPrimaryPressFunctions.Add(call);
                    break;

                case "primarypress":
                    primaryPressFunctions.Add(call);
                    break;

                case "endprimarypress":
                    endPrimaryPressFunctions.Add(call);
                    break;

                case "leftsecondarytouch":
                    leftSecondaryTouchFunctions.Add(call);
                    break;

                case "endleftsecondarytouch":
                    endLeftSecondaryTouchFunctions.Add(call);
                    break;

                case "rightsecondarytouch":
                    rightSecondaryTouchFunctions.Add(call);
                    break;

                case "endrightsecondarytouch":
                    endRightSecondaryTouchFunctions.Add(call);
                    break;

                case "secondarytouch":
                    secondaryTouchFunctions.Add(call);
                    break;

                case "endsecondarytouch":
                    endSecondaryTouchFunctions.Add(call);
                    break;

                case "leftsecondarypress":
                    leftSecondaryPressFunctions.Add(call);
                    break;

                case "endleftsecondarypress":
                    endLeftSecondaryPressFunctions.Add(call);
                    break;

                case "rightsecondarypress":
                    rightSecondaryPressFunctions.Add(call);
                    break;

                case "endrightsecondarypress":
                    endRightSecondaryPressFunctions.Add(call);
                    break;

                case "secondarypress":
                    secondaryPressFunctions.Add(call);
                    break;

                case "endsecondarypress":
                    endSecondaryPressFunctions.Add(call);
                    break;

                case "leftstick":
                    leftStickFunctions.Add(call);
                    break;

                case "endleftstick":
                    endLeftStickFunctions.Add(call);
                    break;

                case "leftstickvaluechange":
                    leftStickValueChangeFunctions.Add(call);
                    break;

                case "rightstick":
                    rightStickFunctions.Add(call);
                    break;

                case "endrightstick":
                    endRightStickFunctions.Add(call);
                    break;

                case "rightstickvaluechange":
                    rightStickValueChangeFunctions.Add(call);
                    break;
            }
        }

        /// <summary>
        /// Deregister an Input Event.
        /// </summary>
        /// <param name="eventName">Name of the Input Event.</param>
        /// <param name="call">Logic being called on invocation of Input Event.</param>
        public void DeregisterInputEvent(string eventName, string call)
        {
            switch (eventName.ToLower())
            {
                case "move":
                    if (!moveFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    moveFunctions.Remove(call);
                    break;

                case "endmove":
                    if (!endMoveFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endMoveFunctions.Remove(call);
                    break;

                case "look":
                    if (!lookFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    lookFunctions.Remove(call);
                    break;

                case "endlook":
                    if (!endLookFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLookFunctions.Remove(call);
                    break;

                case "key":
                    if (!keyFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    keyFunctions.Remove(call);
                    break;

                case "endkey":
                    if (!endKeyFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endKeyFunctions.Remove(call);
                    break;

                case "keycode":
                    if (!keyCodeFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    keyCodeFunctions.Remove(call);
                    break;

                case "endkeycode":
                    if (!endKeyCodeFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endKeyCodeFunctions.Remove(call);
                    break;

                case "left":
                    if (!leftFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftFunctions.Remove(call);
                    break;

                case "endleft":
                    if (!endLeftFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftFunctions.Remove(call);
                    break;

                case "middle":
                    if (!middleFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    middleFunctions.Remove(call);
                    break;

                case "endmiddle":
                    if (!endMiddleFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endMiddleFunctions.Remove(call);
                    break;

                case "right":
                    if (!rightFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightFunctions.Remove(call);
                    break;

                case "endright":
                    if (!endRightFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightFunctions.Remove(call);
                    break;

                case "leftmenu":
                    if (!leftMenuFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftMenuFunctions.Remove(call);
                    break;

                case "endleftmenu":
                    if (!endLeftMenuFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftMenuFunctions.Remove(call);
                    break;

                case "rightmenu":
                    if (!rightMenuFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightMenuFunctions.Remove(call);
                    break;

                case "endrightmenu":
                    if (!endRightMenuFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightMenuFunctions.Remove(call);
                    break;

                case "menu":
                    if (!menuFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    menuFunctions.Remove(call);
                    break;

                case "endmenu":
                    if (!endMenuFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endMenuFunctions.Remove(call);
                    break;

                case "lefttriggertouch":
                    if (!leftTriggerTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftTriggerTouchFunctions.Remove(call);
                    break;

                case "endlefttriggertouch":
                    if (!endLeftTriggerTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftTriggerTouchFunctions.Remove(call);
                    break;

                case "righttriggertouch":
                    if (!rightTriggerTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightTriggerTouchFunctions.Remove(call);
                    break;

                case "endrighttriggertouch":
                    if (!endRightTriggerTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightTriggerTouchFunctions.Remove(call);
                    break;

                case "triggertouch":
                    if (!triggerTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    triggerTouchFunctions.Remove(call);
                    break;

                case "endtriggertouch":
                    if (!endTriggerTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endTriggerTouchFunctions.Remove(call);
                    break;

                case "lefttriggerpress":
                    if (!leftTriggerPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftTriggerPressFunctions.Remove(call);
                    break;

                case "endlefttriggerpress":
                    if (!endLeftTriggerPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftTriggerPressFunctions.Remove(call);
                    break;

                case "righttriggerpress":
                    if (!rightTriggerPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightTriggerPressFunctions.Remove(call);
                    break;

                case "endrighttriggerpress":
                    if (!endRightTriggerPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightTriggerPressFunctions.Remove(call);
                    break;

                case "triggerpress":
                    if (!triggerPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    triggerPressFunctions.Remove(call);
                    break;

                case "endtriggerpress":
                    if (!endTriggerPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endTriggerPressFunctions.Remove(call);
                    break;

                case "leftgrippress":
                    if (!leftGripPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftGripPressFunctions.Remove(call);
                    break;

                case "endleftgrippress":
                    if (!endLeftGripPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftGripPressFunctions.Remove(call);
                    break;

                case "rightgrippress":
                    if (!rightGripPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightGripPressFunctions.Remove(call);
                    break;

                case "endrightgrippress":
                    if (!endRightGripPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightGripPressFunctions.Remove(call);
                    break;

                case "grippress":
                    if (!gripPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    gripPressFunctions.Remove(call);
                    break;

                case "endgrippress":
                    if (!endGripPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endGripPressFunctions.Remove(call);
                    break;

                case "lefttouchpadtouch":
                    if (!leftTouchPadTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftTouchPadTouchFunctions.Remove(call);
                    break;

                case "endlefttouchpadtouch":
                    if (!endLeftTouchPadTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftTouchPadTouchFunctions.Remove(call);
                    break;

                case "lefttouchpadvaluechange":
                    if (!leftTouchPadValueChangeFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftTouchPadValueChangeFunctions.Remove(call);
                    break;

                case "righttouchpadtouch":
                    if (!rightTouchPadTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightTouchPadTouchFunctions.Remove(call);
                    break;

                case "endrighttouchpadtouch":
                    if (!endRightTouchPadTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightTouchPadTouchFunctions.Remove(call);
                    break;

                case "righttouchpadvaluechange":
                    if (!rightTouchPadValueChangeFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightTouchPadValueChangeFunctions.Remove(call);
                    break;

                case "touchpadtouch":
                    if (!touchPadTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    touchPadTouchFunctions.Remove(call);
                    break;

                case "endtouchpadtouch":
                    if (!endTouchPadTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endTouchPadTouchFunctions.Remove(call);
                    break;

                case "lefttouchpadpress":
                    if (!leftTouchPadPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftTouchPadPressFunctions.Remove(call);
                    break;

                case "endlefttouchpadpress":
                    if (!endLeftTouchPadPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftTouchPadPressFunctions.Remove(call);
                    break;

                case "righttouchpadpress":
                    if (!rightTouchPadPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightTouchPadPressFunctions.Remove(call);
                    break;

                case "endrighttouchpadpress":
                    if (!endRightTouchPadPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightTouchPadPressFunctions.Remove(call);
                    break;

                case "touchpadpress":
                    if (!touchPadPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    touchPadPressFunctions.Remove(call);
                    break;

                case "endtouchpadpress":
                    if (!endTouchPadPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endTouchPadPressFunctions.Remove(call);
                    break;

                case "leftprimarytouch":
                    if (!leftPrimaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftPrimaryTouchFunctions.Remove(call);
                    break;

                case "endleftprimarytouch":
                    if (!endLeftPrimaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftPrimaryTouchFunctions.Remove(call);
                    break;

                case "rightprimarytouch":
                    if (!rightPrimaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightPrimaryTouchFunctions.Remove(call);
                    break;

                case "endrightprimarytouch":
                    if (!endRightPrimaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightPrimaryTouchFunctions.Remove(call);
                    break;

                case "primarytouch":
                    if (!primaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    primaryTouchFunctions.Remove(call);
                    break;

                case "endprimarytouch":
                    if (!endPrimaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endPrimaryTouchFunctions.Remove(call);
                    break;

                case "leftprimarypress":
                    if (!leftPrimaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftPrimaryPressFunctions.Remove(call);
                    break;

                case "endleftprimarypress":
                    if (!endLeftPrimaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftPrimaryPressFunctions.Remove(call);
                    break;

                case "rightprimarypress":
                    if (!rightPrimaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightPrimaryPressFunctions.Remove(call);
                    break;

                case "endrightprimarypress":
                    if (!endRightPrimaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightPrimaryPressFunctions.Remove(call);
                    break;

                case "primarypress":
                    if (!primaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    primaryPressFunctions.Remove(call);
                    break;

                case "endprimarypress":
                    if (!endPrimaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endPrimaryPressFunctions.Remove(call);
                    break;

                case "leftsecondarytouch":
                    if (!leftSecondaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftSecondaryTouchFunctions.Remove(call);
                    break;

                case "endleftsecondarytouch":
                    if (!endLeftSecondaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftSecondaryTouchFunctions.Remove(call);
                    break;

                case "rightsecondarytouch":
                    if (!rightSecondaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightSecondaryTouchFunctions.Remove(call);
                    break;

                case "endrightsecondarytouch":
                    if (!endRightSecondaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightSecondaryTouchFunctions.Remove(call);
                    break;

                case "secondarytouch":
                    if (!secondaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    secondaryTouchFunctions.Remove(call);
                    break;

                case "endsecondarytouch":
                    if (!endSecondaryTouchFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endSecondaryTouchFunctions.Remove(call);
                    break;

                case "leftsecondarypress":
                    if (!leftSecondaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftSecondaryPressFunctions.Remove(call);
                    break;

                case "endleftsecondarypress":
                    if (!endLeftSecondaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftSecondaryPressFunctions.Remove(call);
                    break;

                case "rightsecondarypress":
                    if (!rightSecondaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightSecondaryPressFunctions.Remove(call);
                    break;

                case "endrightsecondarypress":
                    if (!endRightSecondaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightSecondaryPressFunctions.Remove(call);
                    break;

                case "secondarypress":
                    if (!secondaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    secondaryPressFunctions.Remove(call);
                    break;

                case "endsecondarypress":
                    if (!endSecondaryPressFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endSecondaryPressFunctions.Remove(call);
                    break;

                case "leftstick":
                    if (!leftStickFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftStickFunctions.Remove(call);
                    break;

                case "endleftstick":
                    if (!endLeftStickFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endLeftStickFunctions.Remove(call);
                    break;

                case "leftstickvaluechange":
                    if (!leftStickValueChangeFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    leftStickValueChangeFunctions.Remove(call);
                    break;

                case "rightstick":
                    if (!rightStickFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightStickFunctions.Remove(call);
                    break;

                case "endrightstick":
                    if (!endRightStickFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    endRightStickFunctions.Remove(call);
                    break;
                
                case "rightstickvaluechange":
                    if (!rightStickValueChangeFunctions.Contains(call))
                    {
                        Logging.LogWarning("[InputManager->DeregisterInputEvent] Event " + call + " does not exist.");
                        break;
                    }
                    rightStickValueChangeFunctions.Remove(call);
                    break;
            }
        }

        /// <summary>
        /// Perform a move.
        /// </summary>
        /// <param name="amount">Amount on X and Y axes to move.</param>
        public void Move(Vector2 amount)
        {
            if (inputEnabled)
            {
                foreach (string function in moveFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", amount.x + ", " + amount.y));
                }
            }
        }

        /// <summary>
        /// End a move.
        /// </summary>
        public void EndMove()
        {
            if (inputEnabled)
            {
                foreach (string function in endMoveFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a look.
        /// </summary>
        /// <param name="amount">Amount on X and Y axes to look.</param>
        public void Look(Vector2 amount)
        {
            if (inputEnabled)
            {
                foreach (string function in lookFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", amount.x + ", " + amount.y));
                }
            }
        }

        /// <summary>
        /// End a look.
        /// </summary>
        public void EndLook()
        {
            if (inputEnabled)
            {
                foreach (string function in endLookFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a key press.
        /// </summary>
        /// <param name="key">Key being pressed.</param>
        /// <param name="keyCode">Keycode of key being pressed.</param>
        public void Key(string key, string keyCode)
        {
            if (inputEnabled)
            {
                foreach (string function in keyFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", "\"" + key.Replace("\\", "\\\\") + "\""));
                }

                foreach (string function in keyCodeFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", "\"" + keyCode.Replace("\\", "\\\\") + "\""));
                }
            }
        }

        /// <summary>
        /// End a key press.
        /// </summary>
        /// <param name="key">Key no longer being pressed.</param>
        /// <param name="keyCode">Keycode of key no longer being pressed.</param>
        public void EndKey(string key, string keyCode)
        {
            if (inputEnabled)
            {
                foreach (string function in endKeyFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", "\"" + key.Replace("\\", "\\\\") + "\""));
                }

                foreach (string function in endKeyCodeFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", "\"" + keyCode.Replace("\\", "\\\\") + "\""));
                }
            }
        }

        /// <summary>
        /// Perform a left.
        /// </summary>
        public void Left()
        {
            if (inputEnabled)
            {
                foreach (string function in leftFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left.
        /// </summary>
        public void EndLeft()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a middle.
        /// </summary>
        public void Middle()
        {
            if (inputEnabled)
            {
                foreach (string function in middleFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a middle.
        /// </summary>
        public void EndMiddle()
        {
            if (inputEnabled)
            {
                foreach (string function in endMiddleFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right.
        /// </summary>
        public void Right()
        {
            if (inputEnabled)
            {
                foreach (string function in rightFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right.
        /// </summary>
        public void EndRight()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left menu press.
        /// </summary>
        public void LeftMenu()
        {
            if (inputEnabled)
            {
                foreach (string function in leftMenuFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left menu press.
        /// </summary>
        public void EndLeftMenu()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftMenuFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right menu press.
        /// </summary>
        public void RightMenu()
        {
            if (inputEnabled)
            {
                foreach (string function in rightMenuFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right menu press.
        /// </summary>
        public void EndRightMenu()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightMenuFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a menu press.
        /// </summary>
        public void Menu()
        {
            if (inputEnabled)
            {
                foreach (string function in menuFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a menu press.
        /// </summary>
        public void EndMenu()
        {
            if (inputEnabled)
            {
                foreach (string function in endMenuFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left trigger touch.
        /// </summary>
        public void LeftTriggerTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in leftTriggerTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left trigger touch.
        /// </summary>
        public void EndLeftTriggerTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftTriggerTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right trigger touch.
        /// </summary>
        public void RightTriggerTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in rightTriggerTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right trigger touch.
        /// </summary>
        public void EndRightTriggerTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightTriggerTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a trigger touch.
        /// </summary>
        public void TriggerTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in triggerTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a trigger touch.
        /// </summary>
        public void EndTriggerTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endTriggerTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left trigger press.
        /// </summary>
        public void LeftTriggerPress()
        {
            if (inputEnabled)
            {
                foreach (string function in leftTriggerPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left trigger press.
        /// </summary>
        public void EndLeftTriggerPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftTriggerPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right trigger press.
        /// </summary>
        public void RightTriggerPress()
        {
            if (inputEnabled)
            {
                foreach (string function in rightTriggerPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right trigger press.
        /// </summary>
        public void EndRightTriggerPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightTriggerPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a trigger press.
        /// </summary>
        public void TriggerPress()
        {
            if (inputEnabled)
            {
                foreach (string function in triggerPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a trigger press.
        /// </summary>
        public void EndTriggerPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endTriggerPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left grip press.
        /// </summary>
        public void LeftGripPress()
        {
            if (inputEnabled)
            {
                foreach (string function in leftGripPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left grip press.
        /// </summary>
        public void EndLeftGripPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftGripPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right grip press.
        /// </summary>
        public void RightGripPress()
        {
            if (inputEnabled)
            {
                foreach (string function in rightGripPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right grip press.
        /// </summary>
        public void EndRightGripPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightGripPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a grip press.
        /// </summary>
        public void GripPress()
        {
            if (inputEnabled)
            {
                foreach (string function in gripPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a grip press.
        /// </summary>
        public void EndGripPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endGripPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left touchpad touch.
        /// </summary>
        public void LeftTouchPadTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in leftTouchPadTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left touchpad touch.
        /// </summary>
        public void EndLeftTouchPadTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftTouchPadTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        // <summary>
        /// Perform a left touchpad touch vaue change.
        /// </summary>
        /// <param name="position">Position of the touchpad touch.</param>
        public void LeftTouchPadTouchValueChange(Vector2 position)
        {
            if (inputEnabled)
            {
                foreach (string function in leftTouchPadValueChangeFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", position.x + ", " + position.y));
                }
            }
        }

        /// <summary>
        /// Perform a right touchpad touch.
        /// </summary>
        public void RightTouchPadTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in rightTouchPadTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right touchpad touch.
        /// </summary>
        public void EndRightTouchPadTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightTouchPadTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        // <summary>
        /// Perform a right touchpad touch vaue change.
        /// </summary>
        /// <param name="position">Position of the touchpad touch.</param>
        public void RightTouchPadTouchValueChange(Vector2 position)
        {
            if (inputEnabled)
            {
                foreach (string function in rightTouchPadValueChangeFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", position.x + ", " + position.y));
                }
            }
        }

        /// <summary>
        /// Perform a touchpad touch.
        /// </summary>
        public void TouchPadTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in touchPadTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a touchpad touch.
        /// </summary>
        public void EndTouchPadTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endTouchPadTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left touchpad press.
        /// </summary>
        public void LeftTouchPadPress()
        {
            if (inputEnabled)
            {
                foreach (string function in leftTouchPadPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left touchpad press.
        /// </summary>
        public void EndLeftTouchPadPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftTouchPadPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right touchpad press.
        /// </summary>
        public void RightTouchPadPress()
        {
            if (inputEnabled)
            {
                foreach (string function in rightTouchPadPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right touchpad press.
        /// </summary>
        public void EndRightTouchPadPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightTouchPadPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a touchpad press.
        /// </summary>
        public void TouchPadPress()
        {
            if (inputEnabled)
            {
                foreach (string function in touchPadPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a touchpad press.
        /// </summary>
        public void EndTouchPadPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endTouchPadPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left primary touch.
        /// </summary>
        public void LeftPrimaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in leftPrimaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left primary touch.
        /// </summary>
        public void EndLeftPrimaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftPrimaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right primary touch.
        /// </summary>
        public void RightPrimaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in rightPrimaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right primary touch.
        /// </summary>
        public void EndRightPrimaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightPrimaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a primary touch.
        /// </summary>
        public void PrimaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in primaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a primary touch.
        /// </summary>
        public void EndPrimaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endPrimaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left primary press.
        /// </summary>
        public void LeftPrimaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in leftPrimaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left primary press.
        /// </summary>
        public void EndLeftPrimaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftPrimaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right primary press.
        /// </summary>
        public void RightPrimaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in rightPrimaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right primary press.
        /// </summary>
        public void EndRightPrimaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightPrimaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a primary press.
        /// </summary>
        public void PrimaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in primaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a primary press.
        /// </summary>
        public void EndPrimaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endPrimaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left secondary touch.
        /// </summary>
        public void LeftSecondaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in leftSecondaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left secondary touch.
        /// </summary>
        public void EndLeftSecondaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftSecondaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right secondary touch.
        /// </summary>
        public void RightSecondaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in rightSecondaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right secondary touch.
        /// </summary>
        public void EndRightSecondaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightSecondaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a secondary touch.
        /// </summary>
        public void SecondaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in secondaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a secondary touch.
        /// </summary>
        public void EndSecondaryTouch()
        {
            if (inputEnabled)
            {
                foreach (string function in endSecondaryTouchFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left secondary press.
        /// </summary>
        public void LeftSecondaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in leftSecondaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left secondary press.
        /// </summary>
        public void EndLeftSecondaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftSecondaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a right secondary press.
        /// </summary>
        public void RightSecondaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in rightSecondaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right secondary press.
        /// </summary>
        public void EndRightSecondaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightSecondaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a secondary press.
        /// </summary>
        public void SecondaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in secondaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a secondary press.
        /// </summary>
        public void EndSecondaryPress()
        {
            if (inputEnabled)
            {
                foreach (string function in endSecondaryPressFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// Perform a left stick.
        /// </summary>
        public void LeftStick()
        {
            if (inputEnabled)
            {
                foreach (string function in leftStickFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a left stick.
        /// </summary>
        public void EndLeftStick()
        {
            if (inputEnabled)
            {
                foreach (string function in endLeftStickFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        // <summary>
        /// Perform a left stick vaue change.
        /// </summary>
        /// <param name="position">Position of the stick.</param>
        public void LeftStickValueChange(Vector2 position)
        {
            if (inputEnabled)
            {
                foreach (string function in leftStickValueChangeFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", position.x + ", " + position.y));
                }
            }
        }

        /// <summary>
        /// Perform a right stick.
        /// </summary>
        public void RightStick()
        {
            if (inputEnabled)
            {
                foreach (string function in rightStickFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        /// <summary>
        /// End a right stick.
        /// </summary>
        public void EndRightStick()
        {
            if (inputEnabled)
            {
                foreach (string function in endRightStickFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function);
                }
            }
        }

        // <summary>
        /// Perform a right stick vaue change.
        /// </summary>
        /// <param name="position">Position of the stick.</param>
        public void RightStickValueChange(Vector2 position)
        {
            if (inputEnabled)
            {
                foreach (string function in rightStickValueChangeFunctions)
                {
                    WebVerseRuntime.Instance.javascriptHandler.Run(function.Replace("?", position.x + ", " + position.y));
                }
            }
        }

        /// <summary>
        /// Get a raycast from the pointer.
        /// </summary>
        /// <param name="direction">Direction to cast the ray in.</param>
        /// <param name="pointerIndex">Index of the pointer to get raycast from.</param>
        /// <returns>A raycast from the pointer, or null.</returns>
        public Tuple<RaycastHit, Vector3> GetPointerRaycast(Vector3 direction, int pointerIndex = 0)
        {
            if (platformInput == null)
            {
                return null;
            }

            Tuple<RaycastHit, Vector3> raycast = platformInput.GetPointerRaycast(direction, pointerIndex);
            if (raycast != null)
            {
                RaycastHit hit = raycast.Item1;
                hit.point -= StraightFour.StraightFour.ActiveWorld.worldOffset;
                return new Tuple<RaycastHit, Vector3>(hit, raycast.Item2);
            }

            return null;
        }
    }
}