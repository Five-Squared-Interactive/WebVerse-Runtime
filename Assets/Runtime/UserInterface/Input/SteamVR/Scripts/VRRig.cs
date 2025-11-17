// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using FiveSQD.StraightFour.Entity;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace FiveSQD.WebVerse.Input.SteamVR
{
    /// <summary>
    /// Class for a VR rig.
    /// </summary>
    public class VRRig : MonoBehaviour
    {
        /// <summary>
        /// Pointer modes enum.
        /// </summary>
        public enum PointerMode { None = 0, Teleport = 1, UI = 2 }

        /// <summary>
        /// Turn locomotion modes enum.
        /// </summary>
        public enum TurnLocomotionMode { None = 0, Smooth = 1, Snap = 2 }

        /// <summary>
        /// The left top-level controller manager.
        /// </summary>
        [Tooltip("The left top-level controller manager.")]
        public ControllerInputActionManager leftControllerManager;

        /// <summary>
        /// The right top-level controller manager.
        /// </summary>
        [Tooltip("The right top-level controller manager.")]
        public ControllerInputActionManager rightControllerManager;

        /// <summary>
        /// The left UI ray interactor.
        /// </summary>
        [Tooltip("The left UI ray interactor.")]
        public UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor leftNearFarInteractor;

        /// <summary>
        /// The right UI ray interactor.
        /// </summary>
        [Tooltip("The right UI ray interactor.")]
        public UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor rightNearFarInteractor;

        /// <summary>
        /// The left teleport ray interactor.
        /// </summary>
        [Tooltip("The left teleport ray interactor.")]
        public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor leftTeleportInteractor;

        /// <summary>
        /// The right teleport ray interactor.
        /// </summary>
        [Tooltip("The right teleport ray interactor.")]
        public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rightTeleportInteractor;

        /// <summary>
        /// The left poke interactor.
        /// </summary>
        [Tooltip("The left poke interactor.")]
        public UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor leftPokeInteractor;

        /// <summary>
        /// The right poke interactor.
        /// </summary>
        [Tooltip("The right poke interactor.")]
        public UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor rightPokeInteractor;

        /// <summary>
        /// The gaze interactor.
        /// </summary>
        [Tooltip("The gaze interactor.")]
        public UnityEngine.XR.Interaction.Toolkit.Interactors.XRGazeInteractor gazeInteractor;

        /// <summary>
        /// The input modality manager.
        /// </summary>
        [Tooltip("The input modality manager.")]
        public XRInputModalityManager inputModalityManager;

        /// <summary>
        /// The snap turn provider.
        /// </summary>
        [Tooltip("The snap turn provider.")]
        public UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning.SnapTurnProvider snapTurnProvider;

        /// <summary>
        /// The continuous turn provider.
        /// </summary>
        [Tooltip("The continuous turn provider.")]
        public UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning.ContinuousTurnProvider continuousTurnProvider;

        /// <summary>
        /// The dynamic move provider.
        /// </summary>
        [Tooltip("The dynamic move provider.")]
        public UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets.DynamicMoveProvider dynamicMoveProvider;

        /// <summary>
        /// The left grab move provider.
        /// </summary>
        [Tooltip("The left grab move provider.")]
        public UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.GrabMoveProvider leftGrabMoveProvider;

        /// <summary>
        /// The right grab move provider.
        /// </summary>
        [Tooltip("The right grab move provider.")]
        public UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.GrabMoveProvider rightGrabMoveProvider;

        /// <summary>
        /// The two handed grab move provider.
        /// </summary>
        [Tooltip("The two handed grab move provider.")]
        public UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.TwoHandedGrabMoveProvider twoHandedGrabMoveProvider;

        /// <summary>
        /// The teleportation provider.
        /// </summary>
        [Tooltip("The teleportation provider.")]
        public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;

        /// <summary>
        /// Origin of the rig.
        /// </summary>
        [Tooltip("Origin of the rig.")]
        public GameObject rigOrigin;

        /// <summary>
        /// Entities following the rig.
        /// </summary>
        [Tooltip("Entities following the rig.")]
        public List<BaseEntity> rigFollowers;

        /// <summary>
        /// Entities following the left hand.
        /// </summary>
        [Tooltip("Entities following the left hand.")]
        public List<BaseEntity> leftHandFollowers;

        /// <summary>
        /// Entities following the right hand.
        /// </summary>
        [Tooltip("Entities following the right hand.")]
        public List<BaseEntity> rightHandFollowers;

        /// <summary>
        /// Cycles to wait in between rig follower updates.
        /// </summary>
        [Tooltip("Cycles to wait in between rig follower updates.")]
        public int cyclesPerRigFollowerUpdate;

        /// <summary>
        /// The rig follower update counter.
        /// </summary>
        private int followersUpdateCount = 0;

        /// <summary>
        /// The left pointer mode.
        /// </summary>
        public PointerMode leftPointerMode
        {
            get
            {
                if (leftNearFarInteractor.enabled == true)
                {
                    return PointerMode.UI;
                }
                else if (leftTeleportInteractor.enabled == true)
                {
                    return PointerMode.Teleport;
                }
                else
                {
                    return PointerMode.None;
                }
            }

            set
            {
                switch (value)
                {
                    case PointerMode.Teleport:
                        leftTeleportInteractor.enabled = true;
                        leftTeleportInteractor.gameObject.SetActive(true);
                        leftNearFarInteractor.enabled = false;
                        teleportationProvider.enabled = true;
                        break;

                    case PointerMode.UI:
                        leftTeleportInteractor.enabled = false;
                        leftTeleportInteractor.gameObject.SetActive(false);
                        leftNearFarInteractor.enabled = true;
                        if (rightPointerMode != PointerMode.Teleport)
                        {
                            teleportationProvider.enabled = false;
                        }
                        break;

                    case PointerMode.None:
                    default:
                        leftTeleportInteractor.enabled = false;
                        leftTeleportInteractor.gameObject.SetActive(false);
                        leftNearFarInteractor.enabled = false;
                        if (rightPointerMode != PointerMode.Teleport)
                        {
                            teleportationProvider.enabled = false;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// The right pointer mode.
        /// </summary>
        public PointerMode rightPointerMode
        {
            get
            {
                if (rightNearFarInteractor.enabled == true)
                {
                    return PointerMode.UI;
                }
                else if (rightTeleportInteractor.enabled == true)
                {
                    return PointerMode.Teleport;
                }
                else
                {
                    return PointerMode.None;
                }
            }

            set
            {
                switch (value)
                {
                    case PointerMode.Teleport:
                        rightTeleportInteractor.enabled = true;
                        rightNearFarInteractor.enabled = false;
                        teleportationProvider.enabled = true;
                        break;

                    case PointerMode.UI:
                        rightTeleportInteractor.enabled = false;
                        rightNearFarInteractor.enabled = true;
                        if (leftPointerMode != PointerMode.Teleport)
                        {
                            teleportationProvider.enabled = false;
                        }
                        break;

                    case PointerMode.None:
                    default:
                        rightTeleportInteractor.enabled = false;
                        rightNearFarInteractor.enabled = false;
                        if (leftPointerMode != PointerMode.Teleport)
                        {
                            teleportationProvider.enabled = false;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Whether the left poker is enabled.
        /// </summary>
        public bool leftPokerEnabled
        {
            get
            {
                return leftPokeInteractor.enabled;
            }

            set
            {
                if (value == true)
                {
                    leftPokeInteractor.enabled = true;
                    leftPokeInteractor.gameObject.SetActive(true);
                }
                else
                {
                    leftPokeInteractor.enabled = false;
                    leftPokeInteractor.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Whether the right poker is enabled.
        /// </summary>
        public bool rightPokerEnabled
        {
            get
            {
                return rightPokeInteractor.enabled;
            }

            set
            {
                if (value == true)
                {
                    rightPokeInteractor.enabled = true;
                    rightPokeInteractor.gameObject.SetActive(true);
                }
                else
                {
                    rightPokeInteractor.enabled = false;
                    rightPokeInteractor.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Whether left interaction is enabled.
        /// </summary>
        public bool leftInteractionEnabled
        {
            get
            {
                return leftNearFarInteractor.enabled;
            }

            set
            {
                if (value == true)
                {
                    leftNearFarInteractor.enabled = true;
                }
                else
                {
                    leftNearFarInteractor.enabled = false;
                }
            }
        }

        /// <summary>
        /// Whether right interaction is enabled.
        /// </summary>
        public bool rightInteractionEnabled
        {
            get
            {
                return rightNearFarInteractor.enabled;
            }

            set
            {
                if (value == true)
                {
                    rightNearFarInteractor.enabled = true;
                }
                else
                {
                    rightNearFarInteractor.enabled = false;
                }
            }
        }

        /// <summary>
        /// The turn locomotion mode.
        /// </summary>
        public TurnLocomotionMode turnLocomotionMode
        {
            get
            {
                if (continuousTurnProvider.enabled == true)
                {
                    return TurnLocomotionMode.Smooth;
                }
                else if (snapTurnProvider.enabled == true)
                {
                    return TurnLocomotionMode.Snap;
                }
                else
                {
                    return TurnLocomotionMode.None;
                }
            }
            set
            {
                switch (value)
                {
                    case TurnLocomotionMode.Smooth:
                        continuousTurnProvider.enabled = true;
                        snapTurnProvider.enabled = false;
                        break;

                    case TurnLocomotionMode.Snap:
                        continuousTurnProvider.enabled = false;
                        snapTurnProvider.enabled = true;
                        break;

                    case TurnLocomotionMode.None:
                    default:
                        continuousTurnProvider.enabled = false;
                        snapTurnProvider.enabled = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Whether joystick motion is enabled.
        /// </summary>
        public bool joystickMotionEnabled
        {
            get
            {
                return dynamicMoveProvider.enabled;
            }

            set
            {
                if (value == true)
                {
                    dynamicMoveProvider.enabled = true;
                }
                else
                {
                    dynamicMoveProvider.enabled = false;
                }
            }
        }

        /// <summary>
        /// Whether left grab move is enabled.
        /// </summary>
        public bool leftGrabMoveEnabled
        {
            get
            {
                return leftGrabMoveProvider.enabled;
            }

            set
            {
                if (value == true)
                {
                    leftGrabMoveProvider.enabled = true;
                }
                else
                {
                    leftGrabMoveProvider.enabled = false;
                }
            }
        }

        /// <summary>
        /// Whether right grab move is enabled.
        /// </summary>
        public bool rightGrabMoveEnabled
        {
            get
            {
                return rightGrabMoveProvider.enabled;
            }

            set
            {
                if (value == true)
                {
                    rightGrabMoveProvider.enabled = true;
                }
                else
                {
                    rightGrabMoveProvider.enabled = false;
                }
            }
        }

        /// <summary>
        /// Whether two handed grab move is enabled.
        /// </summary>
        public bool twoHandedGrabMoveEnabled
        {
            get
            {
                return twoHandedGrabMoveProvider.enabled;
            }

            set
            {
                if (value == true)
                {
                    twoHandedGrabMoveProvider.enabled = true;
                }
                else
                {
                    twoHandedGrabMoveProvider.enabled = false;
                }
            }
        }

        /// <summary>
        /// Initialize the VR rig.
        /// </summary>
        public void Initialize()
        {
            leftPointerMode = PointerMode.None;
            rightPointerMode = PointerMode.None;
            leftPokerEnabled = false;
            rightPokerEnabled = false;
            leftInteractionEnabled = false;
            rightInteractionEnabled = false;
            turnLocomotionMode = TurnLocomotionMode.None;
            joystickMotionEnabled = false;
            leftGrabMoveEnabled = false;
            rightGrabMoveEnabled = false;
            twoHandedGrabMoveEnabled = false;
            rigFollowers = new List<BaseEntity>();
            leftHandFollowers = new List<BaseEntity>();
            rightHandFollowers = new List<BaseEntity>();
        }

        /// <summary>
        /// Terminate the VR rig.
        /// </summary>
        public void Terminate()
        {
            rigFollowers = null;
        }
        
        void Update()
        {
            if (followersUpdateCount++ >= cyclesPerRigFollowerUpdate)
            {
                if (rigFollowers != null)
                {
                    foreach (BaseEntity follower in rigFollowers)
                    {
                        if (follower != null)
                        {
                            follower.SetPosition(rigOrigin.transform.position, false, true);
                            follower.SetRotation(rigOrigin.transform.rotation, false, true);
                        }
                    }
                }

                if (leftHandFollowers != null)
                {
                    foreach (BaseEntity follower in leftHandFollowers)
                    {
                        if (follower != null)
                        {
                            follower.SetPosition(leftControllerManager.transform.position, false, true);
                            follower.SetRotation(leftControllerManager.transform.rotation, false, true);
                        }
                    }
                }

                if (rightHandFollowers != null)
                {
                    foreach (BaseEntity follower in rightHandFollowers)
                    {
                        if (follower != null)
                        {
                            follower.SetPosition(rightControllerManager.transform.position, false, true);
                            follower.SetRotation(rightControllerManager.transform.rotation, false, true);
                        }
                    }
                }

                followersUpdateCount = 0;
            }
        }
    }
}