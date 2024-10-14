// Copyright (c) 2019-2024 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Runtime;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Input
{
    /// <summary>
    /// Input methods.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Pointer modes enum.
        /// </summary>
        public enum VRPointerMode { None = 0, Teleport = 1, UI = 2 }

        /// <summary>
        /// Turn locomotion modes enum.
        /// </summary>
        public enum VRTurnLocomotionMode { None = 0, Smooth = 1, Snap = 2 }

        /// <summary>
        /// Get the current move value.
        /// </summary>
        /// <returns>A Vector2 representation of the current move value.</returns>
        public static Vector2 GetMoveValue()
        {
            UnityEngine.Vector2 moveValue = WebVerseRuntime.Instance.inputManager.moveValue;
            return new Vector2(moveValue.x, moveValue.y);
        }

        /// <summary>
        /// Get the current look value.
        /// </summary>
        /// <returns>A Vector2 representation of the current look value.</returns>
        public static Vector2 GetLookValue()
        {
            UnityEngine.Vector2 lookValue = WebVerseRuntime.Instance.inputManager.lookValue;
            return new Vector2(lookValue.x, lookValue.y);
        }

        /// <summary>
        /// Get the current pressed state of a key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether or not the key is pressed.</returns>
        public static bool GetKeyValue(string key)
        {
            return WebVerseRuntime.Instance.inputManager.pressedKeys.Contains(key);
        }

        /// <summary>
        /// Get the current pressed state of a key.
        /// </summary>
        /// <param name="keycode">The keycode to check.</param>
        /// <returns>Whether or not the key is pressed.</returns>
        public static bool GetKeyCodeValue(string keycode)
        {
            return WebVerseRuntime.Instance.inputManager.pressedKeyCodes.Contains(keycode);
        }

        /// <summary>
        /// Get the current pressed state of the left mouse button.
        /// </summary>
        /// <returns>Whether or not the left mouse button is pressed.</returns>
        public static bool GetLeft()
        {
            return WebVerseRuntime.Instance.inputManager.leftValue;
        }

        /// <summary>
        /// Get the current pressed state of the middle mouse button.
        /// </summary>
        /// <returns>Whether or not the middle mouse button is pressed.</returns>
        public static bool GetMiddle()
        {
            return WebVerseRuntime.Instance.inputManager.middleValue;
        }

        /// <summary>
        /// Get the current pressed state of the right mouse button.
        /// </summary>
        /// <returns>Whether or not the right mouse button is pressed.</returns>
        public static bool GetRight()
        {
            return WebVerseRuntime.Instance.inputManager.rightValue;
        }

        /// <summary>
        /// Get a raycast from the pointer.
        /// </summary>
        /// <param name="direction">Direction to cast the ray in.</param>
        /// <param name="pointerIndex">Index of the pointer to get raycast from.</param>
        /// <returns>A raycast from the pointer, or null.</returns>
        public static RaycastHitInfo? GetPointerRaycast(Vector3 direction, int pointerIndex = 0)
        {
            System.Tuple<UnityEngine.RaycastHit, UnityEngine.Vector3> hitInfo
                = WebVerseRuntime.Instance.inputManager.GetPointerRaycast(
                    new UnityEngine.Vector3(direction.x, direction.y, direction.z),
                    pointerIndex);

            if (hitInfo != null)
            {
                WorldEngine.Entity.BaseEntity hitEntity = null;
                if (hitEntity = hitInfo.Item1.collider.GetComponentInParent<WorldEngine.Entity.BaseEntity>())
                {
                    BaseEntity hitPublicEntity = EntityAPIHelper.GetPublicEntity(hitEntity);
                    if (hitPublicEntity != null)
                    {
                        return new RaycastHitInfo()
                        {
                            entity = hitPublicEntity,
                            hitPoint = new Vector3(hitInfo.Item1.point.x, hitInfo.Item1.point.y, hitInfo.Item1.point.z),
                            hitPointNormal = new Vector3(hitInfo.Item1.normal.x, hitInfo.Item1.normal.y, hitInfo.Item1.normal.z),
                            origin = new Vector3(hitInfo.Item2.x, hitInfo.Item2.y, hitInfo.Item2.z)
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// The pointer mode for the left hand.
        /// </summary>
        public static VRPointerMode leftVRPointerMode
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.leftPointerMode =
                        value == VRPointerMode.None ? WebVerse.Input.SteamVR.VRRig.PointerMode.None :
                        value == VRPointerMode.Teleport ? WebVerse.Input.SteamVR.VRRig.PointerMode.Teleport :
                        WebVerse.Input.SteamVR.VRRig.PointerMode.UI;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return VRPointerMode.None;
                }
                else
                {
                    switch (WebVerseRuntime.Instance.vrRig.leftPointerMode)
                    {
                        case WebVerse.Input.SteamVR.VRRig.PointerMode.Teleport:
                            return VRPointerMode.Teleport;

                        case WebVerse.Input.SteamVR.VRRig.PointerMode.UI:
                            return VRPointerMode.UI;

                        case WebVerse.Input.SteamVR.VRRig.PointerMode.None:
                        default:
                            return VRPointerMode.None;
                    }
                }
            }
        }

        /// <summary>
        /// The pointer mode for the right hand.
        /// </summary>
        public static VRPointerMode rightVRPointerMode
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.rightPointerMode =
                        value == VRPointerMode.None ? WebVerse.Input.SteamVR.VRRig.PointerMode.None :
                        value == VRPointerMode.Teleport ? WebVerse.Input.SteamVR.VRRig.PointerMode.Teleport :
                        WebVerse.Input.SteamVR.VRRig.PointerMode.UI;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return VRPointerMode.None;
                }
                else
                {
                    switch (WebVerseRuntime.Instance.vrRig.rightPointerMode)
                    {
                        case WebVerse.Input.SteamVR.VRRig.PointerMode.Teleport:
                            return VRPointerMode.Teleport;

                        case WebVerse.Input.SteamVR.VRRig.PointerMode.UI:
                            return VRPointerMode.UI;

                        case WebVerse.Input.SteamVR.VRRig.PointerMode.None:
                        default:
                            return VRPointerMode.None;
                    }
                }
            }
        }

        /// <summary>
        /// The poker mode for the left hand.
        /// </summary>
        public static bool leftVRPokerEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.leftPokerEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return false;
                }
                else
                {
                    return WebVerseRuntime.Instance.vrRig.leftPokerEnabled;
                }
            }
        }

        /// <summary>
        /// The poker mode for the right hand.
        /// </summary>
        public static bool rightVRPokerEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.rightPokerEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return false;
                }
                else
                {
                    return WebVerseRuntime.Instance.vrRig.rightPokerEnabled;
                }
            }
        }

        /// <summary>
        /// The interaction mode for the left hand.
        /// </summary>
        public static bool leftInteractionEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.leftInteractionEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return false;
                }
                else
                {
                    return WebVerseRuntime.Instance.vrRig.leftInteractionEnabled;
                }
            }
        }

        /// <summary>
        /// The interaction mode for the right hand.
        /// </summary>
        public static bool rightInteractionEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.rightInteractionEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return false;
                }
                else
                {
                    return WebVerseRuntime.Instance.vrRig.rightInteractionEnabled;
                }
            }
        }

        /// <summary>
        /// The turn locomotion mode.
        /// </summary>
        public static VRTurnLocomotionMode turnLocomotionMode
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.turnLocomotionMode =
                        value == VRTurnLocomotionMode.None ? WebVerse.Input.SteamVR.VRRig.TurnLocomotionMode.None :
                        value == VRTurnLocomotionMode.Smooth ? WebVerse.Input.SteamVR.VRRig.TurnLocomotionMode.Smooth :
                        WebVerse.Input.SteamVR.VRRig.TurnLocomotionMode.Snap;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return VRTurnLocomotionMode.None;
                }
                else
                {
                    switch (WebVerseRuntime.Instance.vrRig.turnLocomotionMode)
                    {
                        case WebVerse.Input.SteamVR.VRRig.TurnLocomotionMode.Smooth:
                            return VRTurnLocomotionMode.Smooth;

                        case WebVerse.Input.SteamVR.VRRig.TurnLocomotionMode.Snap:
                            return VRTurnLocomotionMode.Snap;

                        case WebVerse.Input.SteamVR.VRRig.TurnLocomotionMode.None:
                        default:
                            return VRTurnLocomotionMode.None;
                    }
                }
            }
        }

        /// <summary>
        /// The joystick motion mode.
        /// </summary>
        public static bool joystickMotionEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.joystickMotionEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return false;
                }
                else
                {
                    return WebVerseRuntime.Instance.vrRig.joystickMotionEnabled;
                }
            }
        }

        /// <summary>
        /// The grab move mode for the left hand.
        /// </summary>
        public static bool leftGrabMoveEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.leftGrabMoveEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return false;
                }
                else
                {
                    return WebVerseRuntime.Instance.vrRig.leftGrabMoveEnabled;
                }
            }
        }

        /// <summary>
        /// The grab move mode for the right hand.
        /// </summary>
        public static bool rightGrabMoveEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.rightGrabMoveEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return false;
                }
                else
                {
                    return WebVerseRuntime.Instance.vrRig.rightGrabMoveEnabled;
                }
            }
        }

        /// <summary>
        /// The two-handed grab move mode.
        /// </summary>
        public static bool twoHandedGrabMoveEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.vrRig != null)
                {
                    WebVerseRuntime.Instance.vrRig.twoHandedGrabMoveEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.vrRig == null)
                {
                    return false;
                }
                else
                {
                    return WebVerseRuntime.Instance.vrRig.twoHandedGrabMoveEnabled;
                }
            }
        }
    }
}