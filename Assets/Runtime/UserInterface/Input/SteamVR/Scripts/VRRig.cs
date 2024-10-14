// Copyright (c) 2019-2024 Five Squared Interactive. All rights reserved.

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
        public ActionBasedControllerManager leftControllerManager;

        /// <summary>
        /// The right top-level controller manager.
        /// </summary>
        [Tooltip("The right top-level controller manager.")]
        public ActionBasedControllerManager rightControllerManager;

        /// <summary>
        /// The left UI ray interactor.
        /// </summary>
        [Tooltip("The left UI ray interactor.")]
        public XRRayInteractor leftRayInteractor;

        /// <summary>
        /// The right UI ray interactor.
        /// </summary>
        [Tooltip("The right UI ray interactor.")]
        public XRRayInteractor rightRayInteractor;

        /// <summary>
        /// The left teleport ray interactor.
        /// </summary>
        [Tooltip("The left teleport ray interactor.")]
        public XRRayInteractor leftTeleportInteractor;

        /// <summary>
        /// The right teleport ray interactor.
        /// </summary>
        [Tooltip("The right teleport ray interactor.")]
        public XRRayInteractor rightTeleportInteractor;

        /// <summary>
        /// The left poke interactor.
        /// </summary>
        [Tooltip("The left poke interactor.")]
        public XRPokeInteractor leftPokeInteractor;

        /// <summary>
        /// The right poke interactor.
        /// </summary>
        [Tooltip("The right poke interactor.")]
        public XRPokeInteractor rightPokeInteractor;

        /// <summary>
        /// The left direct interactor.
        /// </summary>
        [Tooltip("The left direct interactor.")]
        public XRDirectInteractor leftDirectInteractor;

        /// <summary>
        /// The right direct interactor.
        /// </summary>
        [Tooltip("The right direct interactor.")]
        public XRDirectInteractor rightDirectInteractor;

        /// <summary>
        /// The gaze interactor.
        /// </summary>
        [Tooltip("The gaze interactor.")]
        public XRGazeInteractor gazeInteractor;

        /// <summary>
        /// The input modality manager.
        /// </summary>
        [Tooltip("The input modality manager.")]
        public XRInputModalityManager inputModalityManager;

        /// <summary>
        /// The snap turn provider.
        /// </summary>
        [Tooltip("The snap turn provider.")]
        public ActionBasedSnapTurnProvider snapTurnProvider;

        /// <summary>
        /// The continuous turn provider.
        /// </summary>
        [Tooltip("The continuous turn provider.")]
        public ActionBasedContinuousTurnProvider continuousTurnProvider;

        /// <summary>
        /// The dynamic move provider.
        /// </summary>
        [Tooltip("The dynamic move provider.")]
        public DynamicMoveProvider dynamicMoveProvider;

        /// <summary>
        /// The left grab move provider.
        /// </summary>
        [Tooltip("The left grab move provider.")]
        public GrabMoveProvider leftGrabMoveProvider;

        /// <summary>
        /// The right grab move provider.
        /// </summary>
        [Tooltip("The right grab move provider.")]
        public GrabMoveProvider rightGrabMoveProvider;

        /// <summary>
        /// The two handed grab move provider.
        /// </summary>
        [Tooltip("The two handed grab move provider.")]
        public TwoHandedGrabMoveProvider twoHandedGrabMoveProvider;

        /// <summary>
        /// The teleportation provider.
        /// </summary>
        [Tooltip("The teleportation provider.")]
        public TeleportationProvider teleportationProvider;

        /// <summary>
        /// The left pointer mode.
        /// </summary>
        public PointerMode leftPointerMode
        {
            get
            {
                if (leftRayInteractor.enabled == true)
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
                        leftRayInteractor.enabled = false;
                        teleportationProvider.enabled = true;
                        break;

                    case PointerMode.UI:
                        leftTeleportInteractor.enabled = false;
                        leftRayInteractor.enabled = true;
                        if (rightPointerMode != PointerMode.Teleport)
                        {
                            teleportationProvider.enabled = false;
                        }
                        break;

                    case PointerMode.None:
                    default:
                        leftTeleportInteractor.enabled = false;
                        leftRayInteractor.enabled = false;
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
                if (rightRayInteractor.enabled == true)
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
                        rightRayInteractor.enabled = false;
                        teleportationProvider.enabled = true;
                        break;

                    case PointerMode.UI:
                        rightTeleportInteractor.enabled = false;
                        rightRayInteractor.enabled = true;
                        if (leftPointerMode != PointerMode.Teleport)
                        {
                            teleportationProvider.enabled = false;
                        }
                        break;

                    case PointerMode.None:
                    default:
                        rightTeleportInteractor.enabled = false;
                        rightRayInteractor.enabled = false;
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
                    leftDirectInteractor.gameObject.SetActive(false);
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
                    rightDirectInteractor.gameObject.SetActive(false);
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
                return leftDirectInteractor.enabled;
            }

            set
            {
                if (value == true)
                {
                    leftDirectInteractor.enabled = true;
                }
                else
                {
                    leftDirectInteractor.enabled = false;
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
                return rightDirectInteractor.enabled;
            }

            set
            {
                if (value == true)
                {
                    rightDirectInteractor.enabled = true;
                }
                else
                {
                    rightDirectInteractor.enabled = false;
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
        }

        /// <summary>
        /// Terminate the VR rig.
        /// </summary>
        public void Terminate()
        {

        }
    }
}