// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities;
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
        /// Whether or not VR is active.
        /// </summary>
        public static bool IsVR
        {
            get
            {
                return WebVerseRuntime.Instance.vr;
            }
        }

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
        /// Set the current movement value programmatically.
        /// </summary>
        /// <param name="amount">A Vector2 representing the desired movement direction and magnitude.</param>
        public static void SetMovement(Vector2 amount)
        {
            UnityEngine.Vector2 moveValue = new UnityEngine.Vector2(amount.x, amount.y);
            WebVerseRuntime.Instance.inputManager.moveValue = moveValue;
        }

        /// <summary>
        /// Trigger a jump action.
        /// </summary>
        public static void Jump()
        {
            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
            {
                WebVerseRuntime.Instance.inputManager.desktopRig.ApplyJump();
            }
        }

        /// <summary>
        /// Trigger a lower action.
        /// </summary>
        public static void Lower()
        {
            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
            {
                WebVerseRuntime.Instance.inputManager.desktopRig.ApplyLowerInput(true);
                // Reset after a brief moment to simulate a single lower action
                // This mimics the Jump behavior which is a one-time action
            }
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
        /// Get the current position of the left hand.
        /// </summary>
        /// <returns>Current position of the left hand, or Vector3.zero if it does not exist.</returns>
        public static Vector3 GetLeftHandPosition()
        {
            if (WebVerseRuntime.Instance.inputManager.vRRig == null)
            {
                return Vector3.zero;
            }

            if (WebVerseRuntime.Instance.inputManager.vRRig.leftControllerManager == null)
            {
                return Vector3.zero;
            }

            UnityEngine.Vector3 pos = WebVerseRuntime.Instance.inputManager.vRRig.leftControllerManager.transform.position;

            return new Vector3(pos.x, pos.y, pos.z);
        }

        /// <summary>
        /// Get the current position of the right hand.
        /// </summary>
        /// <returns>Current position of the right hand, or Vector3.zero if it does not exist.</returns>
        public static Vector3 GetRightHandPosition()
        {
            if (WebVerseRuntime.Instance.inputManager.vRRig == null)
            {
                return Vector3.zero;
            }

            if (WebVerseRuntime.Instance.inputManager.vRRig.rightControllerManager == null)
            {
                return Vector3.zero;
            }

            UnityEngine.Vector3 pos = WebVerseRuntime.Instance.inputManager.vRRig.rightControllerManager.transform.position;

            return new Vector3(pos.x, pos.y, pos.z);
        }

        /// <summary>
        /// Get the current rotation of the left hand.
        /// </summary>
        /// <returns>Current rotation of the left hand, or Quaternion.identity if it does not exist.</returns>
        public static Quaternion GetLeftHandRotation()
        {
            if (WebVerseRuntime.Instance.inputManager.vRRig == null)
            {
                return Quaternion.identity;
            }

            if (WebVerseRuntime.Instance.inputManager.vRRig.leftControllerManager == null)
            {
                return Quaternion.identity;
            }

            UnityEngine.Quaternion rot = WebVerseRuntime.Instance.inputManager.vRRig.leftControllerManager.transform.rotation;

            return new Quaternion(rot.x, rot.y, rot.z, rot.w);
        }

        /// <summary>
        /// Get the current rotation of the right hand.
        /// </summary>
        /// <returns>Current rotation of the right hand, or Quaternion.identity if it does not exist.</returns>
        public static Quaternion GetRightHandRotation()
        {
            if (WebVerseRuntime.Instance.inputManager.vRRig == null)
            {
                return Quaternion.identity;
            }

            if (WebVerseRuntime.Instance.inputManager.vRRig.rightControllerManager == null)
            {
                return Quaternion.identity;
            }

            UnityEngine.Quaternion rot = WebVerseRuntime.Instance.inputManager.vRRig.rightControllerManager.transform.rotation;

            return new Quaternion(rot.x, rot.y, rot.z, rot.w);
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
                StraightFour.Entity.BaseEntity hitEntity = null;
                if (hitEntity = hitInfo.Item1.collider.GetComponentInParent<StraightFour.Entity.BaseEntity>())
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

        /// <summary>
        /// Add a rig follower (an entity that follows the rig).
        /// </summary>
        /// <param name="entityToFollowLeftHand">Entity to follow the rig.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool AddRigFollower(BaseEntity entityToFollowRig)
        {
            if (entityToFollowRig == null)
            {
                Logging.LogWarning("[Input->AddRigFollower] Invalid entityToFollowRig.");
                return false;
            }

            if (entityToFollowRig.internalEntity == null)
            {
                Logging.LogError("[Input->AddRigFollower] Invalid entityToFollowRig.");
                return false;
            }

            if (WebVerseRuntime.Instance.vrRig != null && WebVerseRuntime.Instance.vrRig.rigFollowers != null)
            {
                if (!WebVerseRuntime.Instance.vrRig.rigFollowers.Contains(entityToFollowRig.internalEntity))
                {
                    WebVerseRuntime.Instance.vrRig.rigFollowers.Add(entityToFollowRig.internalEntity);
                }
            }
            return true;
        }

        /// <summary>
        /// Add a left hand follower (an entity that follows the left hand).
        /// </summary>
        /// <param name="entityToFollowLeftHand">Entity to follow the rig.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool AddLeftHandFollower(BaseEntity entityToFollowLeftHand)
        {
            if (entityToFollowLeftHand == null)
            {
                Logging.LogWarning("[Input->AddLeftHandFollower] Invalid entityToFollowLeftHand.");
                return false;
            }

            if (entityToFollowLeftHand.internalEntity == null)
            {
                Logging.LogError("[Input->AddLeftHandFollower] Invalid entityToFollowLeftHand.");
                return false;
            }

            if (WebVerseRuntime.Instance.vrRig != null && WebVerseRuntime.Instance.vrRig.leftHandFollowers != null)
            {
                if (!WebVerseRuntime.Instance.vrRig.leftHandFollowers.Contains(entityToFollowLeftHand.internalEntity))
                {
                    WebVerseRuntime.Instance.vrRig.leftHandFollowers.Add(entityToFollowLeftHand.internalEntity);
                }
            }
            return true;
        }

        /// <summary>
        /// Add a right hand follower (an entity that follows the right hand).
        /// </summary>
        /// <param name="entityToFollowRightHand">Entity to follow the right hand.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool AddRightHandFollower(BaseEntity entityToFollowRightHand)
        {
            if (entityToFollowRightHand == null)
            {
                Logging.LogWarning("[Input->AddRightHandFollower] Invalid entityToFollowRightHand.");
                return false;
            }

            if (entityToFollowRightHand.internalEntity == null)
            {
                Logging.LogError("[Input->AddRightHandFollower] Invalid entityToFollowRightHand.");
                return false;
            }

            if (WebVerseRuntime.Instance.vrRig != null && WebVerseRuntime.Instance.vrRig.rightHandFollowers != null)
            {
                if (!WebVerseRuntime.Instance.vrRig.rightHandFollowers.Contains(entityToFollowRightHand.internalEntity))
                {
                    WebVerseRuntime.Instance.vrRig.rightHandFollowers.Add(entityToFollowRightHand.internalEntity);
                }
            }
            return true;
        }

        /// <summary>
        /// Remove a rig follower (an entity that follows the rig).
        /// </summary>
        /// <param name="entityToFollowRightHand">Entity that follow the rig to remove.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool RemoveRigFollower(BaseEntity entityToFollowRig)
        {
            if (entityToFollowRig == null)
            {
                Logging.LogWarning("[Input->RemoveRigFollower] Invalid entityToFollowRig.");
                return false;
            }

            if (entityToFollowRig.internalEntity == null)
            {
                Logging.LogError("[Input->RemoveRigFollower] Invalid entityToFollowRig.");
                return false;
            }

            if (WebVerseRuntime.Instance.vrRig != null && WebVerseRuntime.Instance.vrRig.rigFollowers != null)
            {
                if (WebVerseRuntime.Instance.vrRig.rigFollowers.Contains(entityToFollowRig.internalEntity))
                {
                    WebVerseRuntime.Instance.vrRig.rigFollowers.Remove(entityToFollowRig.internalEntity);
                }
            }
            return true;
        }

        /// <summary>
        /// Remove a left hand follower (an entity that follows the left hand).
        /// </summary>
        /// <param name="entityToFollowLeftHand">Entity that follow the left hand to remove.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool RemoveLeftHandFollower(BaseEntity entityToFollowLeftHand)
        {
            if (entityToFollowLeftHand == null)
            {
                Logging.LogWarning("[Input->RemoveLeftHandFollower] Invalid entityToFollowLeftHand.");
                return false;
            }

            if (entityToFollowLeftHand.internalEntity == null)
            {
                Logging.LogError("[Input->RemoveLeftHandFollower] Invalid entityToFollowLeftHand.");
                return false;
            }

            if (WebVerseRuntime.Instance.vrRig != null && WebVerseRuntime.Instance.vrRig.leftHandFollowers != null)
            {
                if (WebVerseRuntime.Instance.vrRig.leftHandFollowers.Contains(entityToFollowLeftHand.internalEntity))
                {
                    WebVerseRuntime.Instance.vrRig.leftHandFollowers.Remove(entityToFollowLeftHand.internalEntity);
                }
            }
            return true;
        }

        /// <summary>
        /// Remove a right hand follower (an entity that follows the right hand).
        /// </summary>
        /// <param name="entityToFollowRightHand">Entity that follow the right hand to remove.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool RemoveRightHandFollower(BaseEntity entityToFollowRightHand)
        {
            if (entityToFollowRightHand == null)
            {
                Logging.LogWarning("[Input->RemoveRightHandFollower] Invalid entityToFollowRightHand.");
                return false;
            }

            if (entityToFollowRightHand.internalEntity == null)
            {
                Logging.LogError("[Input->RemoveRightHandFollower] Invalid entityToFollowRightHand.");
                return false;
            }

            if (WebVerseRuntime.Instance.vrRig != null && WebVerseRuntime.Instance.vrRig.rightHandFollowers != null)
            {
                if (WebVerseRuntime.Instance.vrRig.rightHandFollowers.Contains(entityToFollowRightHand.internalEntity))
                {
                    WebVerseRuntime.Instance.vrRig.rightHandFollowers.Remove(entityToFollowRightHand.internalEntity);
                }
            }
            return true;
        }

        /// <summary>
        /// Whether or not gravity is enabled for desktop input.
        /// </summary>
        public static bool gravityEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.gravityEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    return WebVerseRuntime.Instance.inputManager.desktopRig.gravityEnabled;
                }
                return false;
            }
        }

        /// <summary>
        /// Whether or not WASD motion is enabled for desktop input.
        /// </summary>
        public static bool wasdMotionEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.wasdMotionEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    return WebVerseRuntime.Instance.inputManager.desktopRig.wasdMotionEnabled;
                }
                return false;
            }
        }

        /// <summary>
        /// Whether or not mouse look is enabled for desktop input.
        /// </summary>
        public static bool mouseLookEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.mouseLookEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    return WebVerseRuntime.Instance.inputManager.desktopRig.mouseLookEnabled;
                }
                return false;
            }
        }

        /// <summary>
        /// Whether or not jump is enabled for desktop input.
        /// </summary>
        public static bool jumpEnabled
        {
            set
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.jumpEnabled = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    return WebVerseRuntime.Instance.inputManager.desktopRig.jumpEnabled;
                }
                return false;
            }
        }

        /// <summary>
        /// The movement speed for desktop input.
        /// </summary>
        public static float movementSpeed
        {
            set
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.movementSpeed = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    return WebVerseRuntime.Instance.inputManager.desktopRig.movementSpeed;
                }
                return 0f;
            }
        }

        /// <summary>
        /// The look speed (mouse sensitivity) for desktop input.
        /// </summary>
        public static float lookSpeed
        {
            set
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    WebVerseRuntime.Instance.inputManager.desktopRig.mouseSensitivity = value;
                }
            }

            get
            {
                if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
                {
                    return WebVerseRuntime.Instance.inputManager.desktopRig.mouseSensitivity;
                }
                return 0f;
            }
        }

        /// <summary>
        /// Set the avatar entity by tag for desktop input.
        /// </summary>
        /// <param name="entityTag">The tag of the entity to use as the avatar.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetAvatarEntityByTag(string entityTag)
        {
            if (string.IsNullOrEmpty(entityTag))
            {
                Logging.LogWarning("[Input->SetAvatarEntityByTag] Invalid entityTag.");
                return false;
            }

            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
            {
                WebVerseRuntime.Instance.inputManager.desktopRig.SetAvatarEntityByTag(entityTag);
                return true;
            }

            Logging.LogWarning("[Input->SetAvatarEntityByTag] Desktop rig not available.");
            return false;
        }

        /// <summary>
        /// Set the rig offset from a string for desktop input.
        /// </summary>
        /// <param name="rigOffsetString">The rig offset string (format: "x,y,z").</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetRigOffset(Vector3 rigOffset)
        {
            if (WebVerseRuntime.Instance.inputManager.desktopRig != null)
            {
                WebVerseRuntime.Instance.inputManager.desktopRig.SetRigOffset(
                    new UnityEngine.Vector3(rigOffset.x, rigOffset.y, rigOffset.z));
                return true;
            }

            Logging.LogWarning("[Input->SetRigOffset] Desktop rig not available.");
            return false;
        }
    }
}