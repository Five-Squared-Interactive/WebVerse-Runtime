// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

namespace FiveSQD.WebVerse.Input
{
    /// <summary>
    /// Unified VR rig for all platforms (Quest, SteamVR, etc.).
    /// Configure via Inspector or enable auto-detection for platform-specific behavior.
    /// </summary>
    public class VRRig : MonoBehaviour
    {
        #region Enums

        /// <summary>
        /// Pointer modes for ray interaction.
        /// </summary>
        public enum PointerMode
        {
            None = 0,
            Teleport = 1,
            UI = 2
        }

        /// <summary>
        /// Turn locomotion modes.
        /// </summary>
        public enum TurnLocomotionMode
        {
            None = 0,
            Smooth = 1,
            Snap = 2
        }

        /// <summary>
        /// Ray interactor type to use.
        /// </summary>
        public enum RayInteractorType
        {
            /// <summary>
            /// Standard XRRayInteractor (Quest, general use).
            /// </summary>
            Standard = 0,

            /// <summary>
            /// NearFarInteractor (SteamVR starter assets).
            /// </summary>
            NearFar = 1
        }

        #endregion

        #region Platform Configuration

        /// <summary>
        /// Automatically detect platform and configure settings.
        /// </summary>
        [Header("Platform Configuration")]
        [Tooltip("Auto-detect platform and configure accordingly.")]
        public bool autoDetectPlatform = false;

        /// <summary>
        /// Which ray interactor type to use.
        /// </summary>
        [Tooltip("Ray interactor type: Standard (Quest) or NearFar (SteamVR).")]
        public RayInteractorType rayInteractorType = RayInteractorType.Standard;

        /// <summary>
        /// Enable hand tracking support (Quest 3, etc.).
        /// </summary>
        [Tooltip("Enable hand tracking support.")]
        public bool enableHandTracking = false;

        /// <summary>
        /// Enable grab move locomotion (SteamVR style).
        /// </summary>
        [Tooltip("Enable grab move locomotion.")]
        public bool enableGrabMove = false;

        /// <summary>
        /// Enable dynamic move provider (SteamVR style).
        /// </summary>
        [Tooltip("Enable dynamic move provider.")]
        public bool enableDynamicMove = false;

        #endregion

        #region Controller References

        /// <summary>
        /// The left controller transform.
        /// </summary>
        [Header("Controller References")]
        [Tooltip("The left controller transform.")]
        public Transform leftController;

        /// <summary>
        /// The right controller transform.
        /// </summary>
        [Tooltip("The right controller transform.")]
        public Transform rightController;

        /// <summary>
        /// The left controller model (for rotation offset).
        /// </summary>
        [Tooltip("The left controller model (optional).")]
        public Transform leftControllerModel;

        /// <summary>
        /// The right controller model (for rotation offset).
        /// </summary>
        [Tooltip("The right controller model (optional).")]
        public Transform rightControllerModel;

        /// <summary>
        /// Left controller input action manager (SteamVR style).
        /// </summary>
        [Tooltip("Left controller manager (optional, SteamVR).")]
        public MonoBehaviour leftControllerManager;

        /// <summary>
        /// Right controller input action manager (SteamVR style).
        /// </summary>
        [Tooltip("Right controller manager (optional, SteamVR).")]
        public MonoBehaviour rightControllerManager;

        #endregion

        #region Ray Interactors (Standard)

        /// <summary>
        /// The left ray interactor (standard).
        /// </summary>
        [Header("Ray Interactors - Standard")]
        [Tooltip("The left ray interactor.")]
        public XRRayInteractor leftRayInteractor;

        /// <summary>
        /// The right ray interactor (standard).
        /// </summary>
        [Tooltip("The right ray interactor.")]
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

        #endregion

        #region Ray Interactors (NearFar - SteamVR Style)

        /// <summary>
        /// The left near-far interactor (SteamVR style).
        /// </summary>
        [Header("Ray Interactors - NearFar (SteamVR)")]
        [Tooltip("The left near-far interactor (optional).")]
        public NearFarInteractor leftNearFarInteractor;

        /// <summary>
        /// The right near-far interactor (SteamVR style).
        /// </summary>
        [Tooltip("The right near-far interactor (optional).")]
        public NearFarInteractor rightNearFarInteractor;

        #endregion

        #region Direct Interactors

        /// <summary>
        /// The left direct interactor for grabbing.
        /// </summary>
        [Header("Direct Interactors")]
        [Tooltip("The left direct interactor.")]
        public XRDirectInteractor leftDirectInteractor;

        /// <summary>
        /// The right direct interactor for grabbing.
        /// </summary>
        [Tooltip("The right direct interactor.")]
        public XRDirectInteractor rightDirectInteractor;

        /// <summary>
        /// The left poke interactor for UI touch.
        /// </summary>
        [Tooltip("The left poke interactor.")]
        public XRPokeInteractor leftPokeInteractor;

        /// <summary>
        /// The right poke interactor for UI touch.
        /// </summary>
        [Tooltip("The right poke interactor.")]
        public XRPokeInteractor rightPokeInteractor;

        /// <summary>
        /// The gaze interactor.
        /// </summary>
        [Tooltip("The gaze interactor (optional).")]
        public XRGazeInteractor gazeInteractor;

        #endregion

        #region Locomotion

        /// <summary>
        /// The snap turn provider.
        /// </summary>
        [Header("Locomotion - Turning")]
        [Tooltip("The snap turn provider.")]
        public SnapTurnProvider snapTurnProvider;

        /// <summary>
        /// The continuous turn provider.
        /// </summary>
        [Tooltip("The continuous turn provider.")]
        public ContinuousTurnProvider continuousTurnProvider;

        /// <summary>
        /// The teleportation provider.
        /// </summary>
        [Header("Locomotion - Teleport")]
        [Tooltip("The teleportation provider.")]
        public TeleportationProvider teleportationProvider;

        /// <summary>
        /// The dynamic move provider (SteamVR style).
        /// </summary>
        [Header("Locomotion - Movement (Optional)")]
        [Tooltip("The dynamic move provider (optional).")]
        public MonoBehaviour dynamicMoveProvider;

        /// <summary>
        /// The left grab move provider (SteamVR style).
        /// </summary>
        [Tooltip("The left grab move provider (optional).")]
        public GrabMoveProvider leftGrabMoveProvider;

        /// <summary>
        /// The right grab move provider (SteamVR style).
        /// </summary>
        [Tooltip("The right grab move provider (optional).")]
        public GrabMoveProvider rightGrabMoveProvider;

        /// <summary>
        /// The two handed grab move provider (SteamVR style).
        /// </summary>
        [Tooltip("The two handed grab move provider (optional).")]
        public TwoHandedGrabMoveProvider twoHandedGrabMoveProvider;

        /// <summary>
        /// The input modality manager (SteamVR style).
        /// </summary>
        [Tooltip("The input modality manager (optional).")]
        public XRInputModalityManager inputModalityManager;

        #endregion

        #region Rig Origin & Followers

        /// <summary>
        /// Origin of the rig.
        /// </summary>
        [Header("Rig Origin")]
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
        public int cyclesPerRigFollowerUpdate = 1;

        #endregion

        #region Controller Model Settings

        /// <summary>
        /// Quest/Meta controller model prefab (uses OVRControllerHelper).
        /// Assign OVRRuntimeControllerPrefab for proper Quest controller rendering.
        /// The same prefab is used for both hands (configured at runtime).
        /// </summary>
        [Header("Controller Model Settings")]
        [Tooltip("Quest controller model prefab. Assign OVRRuntimeControllerPrefab from Meta XR SDK.")]
        public GameObject questControllerModelPrefab;

        /// <summary>
        /// Whether to use platform-specific controller models.
        /// When enabled, Quest uses OVR controller models, other platforms use XRI models.
        /// </summary>
        [Tooltip("Automatically use platform-specific controller models (Quest uses OVR, others use XRI).")]
        public bool usePlatformSpecificModels = true;

        #endregion

        #region Hand Tracking References (Optional)

        /// <summary>
        /// Reference to hand tracking component (Quest 3, etc.).
        /// </summary>
        [Header("Hand Tracking (Optional)")]
        [Tooltip("Hand tracking component reference.")]
        public MonoBehaviour handTracking;

        /// <summary>
        /// Reference to input mode manager (hands/controllers switching).
        /// </summary>
        [Tooltip("Input mode manager reference.")]
        public MonoBehaviour inputModeManager;

        #endregion

        #region Private Fields

        private int followersUpdateCount = 0;

        #endregion

        #region Pointer Mode Properties

        /// <summary>
        /// The left pointer mode.
        /// </summary>
        public PointerMode leftPointerMode
        {
            get
            {
                if (rayInteractorType == RayInteractorType.NearFar)
                {
                    if (leftNearFarInteractor != null && leftNearFarInteractor.enabled)
                        return PointerMode.UI;
                }
                else
                {
                    if (leftRayInteractor != null && leftRayInteractor.enabled)
                        return PointerMode.UI;
                }

                if (leftTeleportInteractor != null && leftTeleportInteractor.enabled)
                    return PointerMode.Teleport;

                return PointerMode.None;
            }
            set
            {
                switch (value)
                {
                    case PointerMode.Teleport:
                        SetInteractorEnabled(leftTeleportInteractor, true);
                        SetRayInteractorEnabled(true, false);
                        if (teleportationProvider != null) teleportationProvider.enabled = true;
                        break;

                    case PointerMode.UI:
                        SetInteractorEnabled(leftTeleportInteractor, false);
                        SetRayInteractorEnabled(true, true);
                        if (rightPointerMode != PointerMode.Teleport && teleportationProvider != null)
                            teleportationProvider.enabled = false;
                        break;

                    case PointerMode.None:
                    default:
                        SetInteractorEnabled(leftTeleportInteractor, false);
                        SetRayInteractorEnabled(true, false);
                        if (rightPointerMode != PointerMode.Teleport && teleportationProvider != null)
                            teleportationProvider.enabled = false;
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
                if (rayInteractorType == RayInteractorType.NearFar)
                {
                    if (rightNearFarInteractor != null && rightNearFarInteractor.enabled)
                        return PointerMode.UI;
                }
                else
                {
                    if (rightRayInteractor != null && rightRayInteractor.enabled)
                        return PointerMode.UI;
                }

                if (rightTeleportInteractor != null && rightTeleportInteractor.enabled)
                    return PointerMode.Teleport;

                return PointerMode.None;
            }
            set
            {
                switch (value)
                {
                    case PointerMode.Teleport:
                        SetInteractorEnabled(rightTeleportInteractor, true);
                        SetRayInteractorEnabled(false, false);
                        if (teleportationProvider != null) teleportationProvider.enabled = true;
                        break;

                    case PointerMode.UI:
                        SetInteractorEnabled(rightTeleportInteractor, false);
                        SetRayInteractorEnabled(false, true);
                        if (leftPointerMode != PointerMode.Teleport && teleportationProvider != null)
                            teleportationProvider.enabled = false;
                        break;

                    case PointerMode.None:
                    default:
                        SetInteractorEnabled(rightTeleportInteractor, false);
                        SetRayInteractorEnabled(false, false);
                        if (leftPointerMode != PointerMode.Teleport && teleportationProvider != null)
                            teleportationProvider.enabled = false;
                        break;
                }
            }
        }

        #endregion

        #region Direct Interaction Properties

        public bool leftDirectGrabEnabled
        {
            get => leftDirectInteractor != null && leftDirectInteractor.enabled;
            set => SetInteractorEnabled(leftDirectInteractor, value);
        }

        public bool rightDirectGrabEnabled
        {
            get => rightDirectInteractor != null && rightDirectInteractor.enabled;
            set => SetInteractorEnabled(rightDirectInteractor, value);
        }

        public bool leftPokerEnabled
        {
            get => leftPokeInteractor != null && leftPokeInteractor.enabled;
            set => SetInteractorEnabled(leftPokeInteractor, value);
        }

        public bool rightPokerEnabled
        {
            get => rightPokeInteractor != null && rightPokeInteractor.enabled;
            set => SetInteractorEnabled(rightPokeInteractor, value);
        }

        public bool leftInteractionEnabled
        {
            get => leftNearFarInteractor != null && leftNearFarInteractor.enabled;
            set { if (leftNearFarInteractor != null) leftNearFarInteractor.enabled = value; }
        }

        public bool rightInteractionEnabled
        {
            get => rightNearFarInteractor != null && rightNearFarInteractor.enabled;
            set { if (rightNearFarInteractor != null) rightNearFarInteractor.enabled = value; }
        }

        #endregion

        #region Locomotion Properties

        public TurnLocomotionMode turnLocomotionMode
        {
            get
            {
                if (continuousTurnProvider != null && continuousTurnProvider.enabled)
                    return TurnLocomotionMode.Smooth;
                if (snapTurnProvider != null && snapTurnProvider.enabled)
                    return TurnLocomotionMode.Snap;
                return TurnLocomotionMode.None;
            }
            set
            {
                switch (value)
                {
                    case TurnLocomotionMode.Smooth:
                        if (continuousTurnProvider != null) continuousTurnProvider.enabled = true;
                        if (snapTurnProvider != null) snapTurnProvider.enabled = false;
                        break;
                    case TurnLocomotionMode.Snap:
                        if (continuousTurnProvider != null) continuousTurnProvider.enabled = false;
                        if (snapTurnProvider != null) snapTurnProvider.enabled = true;
                        break;
                    default:
                        if (continuousTurnProvider != null) continuousTurnProvider.enabled = false;
                        if (snapTurnProvider != null) snapTurnProvider.enabled = false;
                        break;
                }
            }
        }

        public bool joystickMotionEnabled
        {
            get => dynamicMoveProvider != null && dynamicMoveProvider.enabled;
            set { if (dynamicMoveProvider != null) dynamicMoveProvider.enabled = value; }
        }

        public bool leftGrabMoveEnabled
        {
            get => leftGrabMoveProvider != null && leftGrabMoveProvider.enabled;
            set { if (leftGrabMoveProvider != null) leftGrabMoveProvider.enabled = value; }
        }

        public bool rightGrabMoveEnabled
        {
            get => rightGrabMoveProvider != null && rightGrabMoveProvider.enabled;
            set { if (rightGrabMoveProvider != null) rightGrabMoveProvider.enabled = value; }
        }

        public bool twoHandedGrabMoveEnabled
        {
            get => twoHandedGrabMoveProvider != null && twoHandedGrabMoveProvider.enabled;
            set { if (twoHandedGrabMoveProvider != null) twoHandedGrabMoveProvider.enabled = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the VR rig.
        /// </summary>
        public void Initialize()
        {
            if (autoDetectPlatform)
            {
                DetectAndConfigurePlatform();
            }

            // Default pointer modes
            leftPointerMode = PointerMode.None;
            rightPointerMode = PointerMode.UI;

            // Enable direct interaction
            leftDirectGrabEnabled = true;
            rightDirectGrabEnabled = true;

            // Enable poke for UI
            leftPokerEnabled = true;
            rightPokerEnabled = true;

            // Default to snap turn
            turnLocomotionMode = TurnLocomotionMode.Snap;

            // Configure optional features based on settings
            joystickMotionEnabled = enableDynamicMove;
            leftGrabMoveEnabled = enableGrabMove;
            rightGrabMoveEnabled = enableGrabMove;
            twoHandedGrabMoveEnabled = enableGrabMove;

            // Initialize follower lists
            if (rigFollowers == null) rigFollowers = new List<BaseEntity>();
            if (leftHandFollowers == null) leftHandFollowers = new List<BaseEntity>();
            if (rightHandFollowers == null) rightHandFollowers = new List<BaseEntity>();

            // Set up platform-specific controller models
            SetupPlatformControllerModels();

            Logging.Log($"[VRRig] Initialized. RayType={rayInteractorType}, HandTracking={enableHandTracking}");
        }

        /// <summary>
        /// Terminate the VR rig.
        /// </summary>
        public void Terminate()
        {
            rigFollowers?.Clear();
            leftHandFollowers?.Clear();
            rightHandFollowers?.Clear();
        }

        /// <summary>
        /// Enable ray pointing on both hands.
        /// </summary>
        public void EnableRayPointing()
        {
            leftPointerMode = PointerMode.UI;
            rightPointerMode = PointerMode.UI;
        }

        /// <summary>
        /// Disable ray pointing on both hands.
        /// </summary>
        public void DisableRayPointing()
        {
            leftPointerMode = PointerMode.None;
            rightPointerMode = PointerMode.None;
        }

        /// <summary>
        /// Get raycast hit from controller.
        /// </summary>
        public bool TryGetRaycastHit(bool isLeft, out RaycastHit hit)
        {
            hit = default;

            if (rayInteractorType == RayInteractorType.NearFar)
            {
                // NearFarInteractor doesn't have TryGetCurrent3DRaycastHit
                // Fall back to standard ray interactor if available
                XRRayInteractor interactor = isLeft ? leftRayInteractor : rightRayInteractor;
                if (interactor != null && interactor.enabled)
                    return interactor.TryGetCurrent3DRaycastHit(out hit);
                return false;
            }

            XRRayInteractor rayInteractor = isLeft ? leftRayInteractor : rightRayInteractor;
            if (rayInteractor == null || !rayInteractor.enabled)
                return false;

            return rayInteractor.TryGetCurrent3DRaycastHit(out hit);
        }

        /// <summary>
        /// Set up platform-specific controller models.
        /// </summary>
        private void SetupPlatformControllerModels()
        {
            if (!usePlatformSpecificModels) return;

            string deviceName = UnityEngine.XR.XRSettings.loadedDeviceName?.ToLower() ?? "";
            string deviceModel = SystemInfo.deviceModel?.ToLower() ?? "";

            bool isQuest = deviceName.Contains("oculus") || deviceName.Contains("quest") ||
                           deviceName.Contains("meta") || deviceModel.Contains("quest");

            if (isQuest)
            {
                // Hide XRI controller models and use Quest-specific models
                if (leftControllerModel != null)
                {
                    leftControllerModel.gameObject.SetActive(false);
                }
                if (rightControllerModel != null)
                {
                    rightControllerModel.gameObject.SetActive(false);
                }

                // Instantiate Quest controller models (use same prefab, configure for left/right)
                if (questControllerModelPrefab != null)
                {
                    if (leftController != null)
                    {
                        GameObject leftModel = Instantiate(questControllerModelPrefab, leftController);
                        leftModel.transform.localPosition = Vector3.zero;
                        leftModel.transform.localRotation = Quaternion.identity;
                        ConfigureOVRController(leftModel, true);
                        Logging.Log("[VRRig] Instantiated Quest left controller model.");
                    }

                    if (rightController != null)
                    {
                        GameObject rightModel = Instantiate(questControllerModelPrefab, rightController);
                        rightModel.transform.localPosition = Vector3.zero;
                        rightModel.transform.localRotation = Quaternion.identity;
                        ConfigureOVRController(rightModel, false);
                        Logging.Log("[VRRig] Instantiated Quest right controller model.");
                    }
                }
            }
            // For non-Quest platforms, use the existing XRI controller models (no changes needed)
        }

        /// <summary>
        /// Configure OVRControllerHelper component for left or right hand using reflection.
        /// </summary>
        private void ConfigureOVRController(GameObject controllerModel, bool isLeft)
        {
            // Use reflection to set m_controller field on OVRControllerHelper
            // This avoids compile-time dependency on Meta XR SDK
            var ovrHelper = controllerModel.GetComponent("OVRControllerHelper");
            if (ovrHelper == null)
            {
                Logging.LogWarning("[VRRig] OVRControllerHelper component not found on Quest controller model.");
                return;
            }

            // Get OVRInput.Controller enum type and values
            var ovrInputType = System.Type.GetType("OVRInput, Assembly-CSharp") ??
                               System.Type.GetType("OVRInput, Oculus.VR") ??
                               System.Type.GetType("OVRInput, Meta.XR.Core");
            if (ovrInputType == null)
            {
                Logging.LogWarning("[VRRig] OVRInput type not found.");
                return;
            }

            var controllerEnumType = ovrInputType.GetNestedType("Controller");
            if (controllerEnumType == null)
            {
                Logging.LogWarning("[VRRig] OVRInput.Controller enum not found.");
                return;
            }

            // LTouch = 1, RTouch = 2
            object controllerValue = System.Enum.ToObject(controllerEnumType, isLeft ? 1 : 2);

            // Set the m_controller field
            var field = ovrHelper.GetType().GetField("m_controller",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(ovrHelper, controllerValue);
                Logging.Log($"[VRRig] Configured OVRControllerHelper for {(isLeft ? "left" : "right")} hand.");
            }
            else
            {
                Logging.LogWarning("[VRRig] m_controller field not found on OVRControllerHelper.");
            }
        }

        #endregion

        #region Private Methods

        private void DetectAndConfigurePlatform()
        {
            // Check for Quest/Meta devices
            string deviceName = UnityEngine.XR.XRSettings.loadedDeviceName?.ToLower() ?? "";
            string deviceModel = SystemInfo.deviceModel?.ToLower() ?? "";

            bool isQuest = deviceName.Contains("oculus") || deviceName.Contains("quest") ||
                           deviceName.Contains("meta") || deviceModel.Contains("quest");

            if (isQuest)
            {
                rayInteractorType = RayInteractorType.Standard;
                enableHandTracking = true;
                enableGrabMove = false;
                enableDynamicMove = false;
                Logging.Log("[VRRig] Detected Quest platform.");
            }
            else
            {
                // Assume PC VR (SteamVR, etc.)
                rayInteractorType = RayInteractorType.NearFar;
                enableHandTracking = false;
                enableGrabMove = true;
                enableDynamicMove = true;
                Logging.Log("[VRRig] Detected PC VR platform.");
            }
        }

        private void SetInteractorEnabled(MonoBehaviour interactor, bool enabled)
        {
            if (interactor != null)
            {
                interactor.enabled = enabled;
                interactor.gameObject.SetActive(enabled);
            }
        }

        private void SetRayInteractorEnabled(bool isLeft, bool enabled)
        {
            if (rayInteractorType == RayInteractorType.NearFar)
            {
                var interactor = isLeft ? leftNearFarInteractor : rightNearFarInteractor;
                if (interactor != null) interactor.enabled = enabled;
            }
            else
            {
                var interactor = isLeft ? leftRayInteractor : rightRayInteractor;
                SetInteractorEnabled(interactor, enabled);
            }
        }

        private Transform GetLeftHandTransform()
        {
            if (leftControllerManager != null) return leftControllerManager.transform;
            return leftController;
        }

        private Transform GetRightHandTransform()
        {
            if (rightControllerManager != null) return rightControllerManager.transform;
            return rightController;
        }

        private void UpdateFollowers()
        {
            if (rigFollowers != null && rigOrigin != null)
            {
                foreach (var follower in rigFollowers)
                {
                    if (follower != null)
                    {
                        follower.SetPosition(rigOrigin.transform.position, false, true);
                        follower.SetRotation(rigOrigin.transform.rotation, false, true);
                    }
                }
            }

            Transform leftHand = GetLeftHandTransform();
            if (leftHandFollowers != null && leftHand != null)
            {
                foreach (var follower in leftHandFollowers)
                {
                    if (follower != null)
                    {
                        follower.SetPosition(leftHand.position, false, true);
                        follower.SetRotation(leftHand.rotation, false, true);
                    }
                }
            }

            Transform rightHand = GetRightHandTransform();
            if (rightHandFollowers != null && rightHand != null)
            {
                foreach (var follower in rightHandFollowers)
                {
                    if (follower != null)
                    {
                        follower.SetPosition(rightHand.position, false, true);
                        follower.SetRotation(rightHand.rotation, false, true);
                    }
                }
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (followersUpdateCount++ >= cyclesPerRigFollowerUpdate)
            {
                UpdateFollowers();
                followersUpdateCount = 0;
            }
        }

        #endregion
    }
}
