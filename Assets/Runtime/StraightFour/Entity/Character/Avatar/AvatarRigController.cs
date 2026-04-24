// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace FiveSQD.WebVerse.Avatar
{
    /// <summary>
    /// Manages IK constraints for VR avatar embodiment.
    /// Creates a RigBuilder with head aim and two-hand IK constraints.
    /// References AvatarAnimationManager's Animator (unidirectional dependency).
    /// </summary>
    public class AvatarRigController : MonoBehaviour
    {
        private AvatarAnimationManager _animationManager;
        private Animator _animator;
        private RigBuilder _rigBuilder;
        private Rig _rig;
        private GameObject _rigObject;
        private MultiAimConstraint _headAimConstraint;
        private TwoBoneIKConstraint _leftHandIKConstraint;
        private TwoBoneIKConstraint _rightHandIKConstraint;
        private Transform _headTarget;
        private Transform _leftHandTarget;
        private Transform _rightHandTarget;
        private Transform _headTrackingSource;
        private Transform _leftHandTrackingSource;
        private Transform _rightHandTrackingSource;
        private Renderer _headRenderer;
        private int _originalHeadLayer;
        private bool _isFirstPersonEnabled;
        private float _heightScale = 1f;
        private float _armSpanScale = 1f;
        private const float ReferenceHeight = 1.7f;
        private const float ReferenceArmSpan = 1.5f;
        private bool _isInitialized;

        /// <summary>
        /// The RigBuilder component managing IK evaluation.
        /// </summary>
        public RigBuilder RigBuilder => _rigBuilder;

        /// <summary>
        /// Transform that drives the head IK aim target.
        /// Positioned by VR tracking in Story 3.2.
        /// </summary>
        public Transform HeadTarget => _headTarget;

        /// <summary>
        /// Transform that drives the left hand IK target.
        /// Positioned by VR tracking in Story 3.2.
        /// </summary>
        public Transform LeftHandTarget => _leftHandTarget;

        /// <summary>
        /// Transform that drives the right hand IK target.
        /// Positioned by VR tracking in Story 3.2.
        /// </summary>
        public Transform RightHandTarget => _rightHandTarget;

        /// <summary>
        /// The current height scale factor from calibration (1.0 = uncalibrated).
        /// </summary>
        public float HeightScale => _heightScale;

        /// <summary>
        /// The current arm span scale factor from calibration (1.0 = uncalibrated).
        /// </summary>
        public float ArmSpanScale => _armSpanScale;

        /// <summary>
        /// Initializes the IK rig with constraints for head aim and two-hand IK.
        /// Must be called after AvatarAnimationManager is initialized.
        /// </summary>
        /// <param name="animationManager">The animation manager providing the Animator.</param>
        public void Initialize(AvatarAnimationManager animationManager)
        {
            if (animationManager == null)
            {
                return;
            }

            if (_isInitialized)
            {
                return;
            }

            _animationManager = animationManager;
            _animator = animationManager.Animator;

            SetupRigBuilder();
            SetupIKTargets();
            SetupConstraints();
            BindConstraintsToSkeleton();

            // Set rig weight to 1 (VR mode active)
            _rig.weight = 1f;
            _isInitialized = true;
        }

        /// <summary>
        /// Updates the cached Animator reference and rebinds constraints.
        /// Called when AvatarLoader switches to a custom avatar's Animator.
        /// </summary>
        /// <param name="animator">The new Animator reference.</param>
        public void UpdateAnimator(Animator animator)
        {
            _animator = animator;
            // Re-resolve head renderer for the new avatar
            _headRenderer = null;
            ResolveHeadRenderer();
            // Re-apply first-person mode to the new avatar's head renderer
            if (_isFirstPersonEnabled && _headRenderer != null)
            {
                int firstPersonHiddenLayer = LayerMask.NameToLayer("FirstPersonHidden");
                if (firstPersonHiddenLayer >= 0)
                {
                    _originalHeadLayer = _headRenderer.gameObject.layer;
                    _headRenderer.gameObject.layer = firstPersonHiddenLayer;
                }
            }
            BindConstraintsToSkeleton();
            if (_rigBuilder != null)
            {
                _rigBuilder.Build();
            }
        }

        /// <summary>
        /// Sets the rig weight to enable (1) or disable (0) IK solving.
        /// </summary>
        /// <param name="weight">Weight value between 0 and 1.</param>
        public void SetRigWeight(float weight)
        {
            if (_rig != null)
            {
                _rig.weight = Mathf.Clamp01(weight);
            }
        }

        /// <summary>
        /// Cleans up IK rig resources. Disables RigBuilder and sets weight to 0.
        /// </summary>
        public void Cleanup()
        {
            // Reset calibration scale
            ResetCalibration();

            // Restore head layer before cleanup
            if (_headRenderer != null)
            {
                _headRenderer.gameObject.layer = _originalHeadLayer;
                _headRenderer = null;
            }
            _isFirstPersonEnabled = false;

            if (_rig != null)
            {
                _rig.weight = 0f;
            }

            if (_rigBuilder != null)
            {
                _rigBuilder.enabled = false;
            }

            if (_rigObject != null)
            {
                Destroy(_rigObject);
                _rigObject = null;
            }

            _headTarget = null;
            _leftHandTarget = null;
            _rightHandTarget = null;
            _isInitialized = false;
        }

        /// <summary>
        /// Populates the IK-related fields of an AvatarState struct.
        /// </summary>
        /// <param name="state">The AvatarState to populate (passed by reference).</param>
        public void PopulateState(ref AvatarState state)
        {
            if (_headTarget != null)
            {
                state.HeadPosition = _headTarget.position;
                state.HeadRotation = _headTarget.rotation;
            }

            if (_leftHandTarget != null)
            {
                state.LeftHandPosition = _leftHandTarget.position;
                state.LeftHandRotation = _leftHandTarget.rotation;
            }

            if (_rightHandTarget != null)
            {
                state.RightHandPosition = _rightHandTarget.position;
                state.RightHandRotation = _rightHandTarget.rotation;
            }

            state.IsVRMode = true;
            state.HeightScale = _heightScale;
            state.ArmSpanScale = _armSpanScale;
        }

        /// <summary>
        /// Sets the VR tracking source transforms that drive IK targets each frame.
        /// Sources are read-only — only position/rotation are copied from them.
        /// </summary>
        /// <param name="headSource">VR camera transform (headset pose).</param>
        /// <param name="leftHandSource">Left controller transform.</param>
        /// <param name="rightHandSource">Right controller transform.</param>
        public void SetTrackingSources(Transform headSource, Transform leftHandSource, Transform rightHandSource)
        {
            _headTrackingSource = headSource;
            _leftHandTrackingSource = leftHandSource;
            _rightHandTrackingSource = rightHandSource;
        }

        /// <summary>
        /// Copies VR tracking source poses into IK target transforms.
        /// Called from LateUpdate to ensure tracking data is fresh.
        /// </summary>
        public void UpdateTracking()
        {
            if (!_isInitialized) return;

            if (_headTrackingSource != null && _headTarget != null)
            {
                _headTarget.position = _headTrackingSource.position;
                _headTarget.rotation = _headTrackingSource.rotation;
            }

            if (_leftHandTrackingSource != null && _leftHandTarget != null)
            {
                _leftHandTarget.position = _leftHandTrackingSource.position;
                _leftHandTarget.rotation = _leftHandTrackingSource.rotation;
            }

            if (_rightHandTrackingSource != null && _rightHandTarget != null)
            {
                _rightHandTarget.position = _rightHandTrackingSource.position;
                _rightHandTarget.rotation = _rightHandTrackingSource.rotation;
            }
        }

        /// <summary>
        /// Enables or disables first-person mode by moving the head mesh to/from
        /// the FirstPersonHidden layer. Only the head is hidden; body remains visible.
        /// </summary>
        /// <param name="enabled">True to hide head from local VR camera, false to restore.</param>
        public void SetFirstPersonMode(bool enabled)
        {
            if (!_isInitialized) return;

            _isFirstPersonEnabled = enabled;

            // Resolve head renderer if not cached
            if (_headRenderer == null)
            {
                ResolveHeadRenderer();
                if (_headRenderer == null) return;
            }

            int firstPersonHiddenLayer = LayerMask.NameToLayer("FirstPersonHidden");
            if (firstPersonHiddenLayer < 0) return;

            if (enabled)
            {
                // Only store original layer if not already on the hidden layer (guards against double-call)
                if (_headRenderer.gameObject.layer != firstPersonHiddenLayer)
                {
                    _originalHeadLayer = _headRenderer.gameObject.layer;
                }
                _headRenderer.gameObject.layer = firstPersonHiddenLayer;
            }
            else
            {
                _headRenderer.gameObject.layer = _originalHeadLayer;
            }
        }

        /// <summary>
        /// Calibrates the avatar to match the user's physical proportions.
        /// Measures height from headset Y and arm span from controller distance.
        /// </summary>
        /// <param name="headsetHeight">User's headset height in meters (Y position).</param>
        /// <param name="armSpan">Distance between left and right controllers in meters.</param>
        public void Calibrate(float headsetHeight, float armSpan)
        {
            if (!_isInitialized) return;

            // Guard against invalid or extreme values
            if (headsetHeight < 0.5f || headsetHeight > 3f) return;
            if (armSpan < 0.3f || armSpan > 4f) return;

            _heightScale = headsetHeight / ReferenceHeight;
            _armSpanScale = armSpan / ReferenceArmSpan;

            // Apply uniform scale based on height
            transform.localScale = Vector3.one * _heightScale;
        }

        /// <summary>
        /// Resets calibration to default (scale 1.0).
        /// </summary>
        public void ResetCalibration()
        {
            _heightScale = 1f;
            _armSpanScale = 1f;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Configures a VR camera to exclude the FirstPersonHidden layer from rendering.
        /// </summary>
        /// <param name="vrCamera">The VR camera to configure.</param>
        public static void SetupFirstPersonCamera(Camera vrCamera)
        {
            if (vrCamera == null) return;

            int firstPersonHiddenLayer = LayerMask.NameToLayer("FirstPersonHidden");
            if (firstPersonHiddenLayer < 0) return;

            vrCamera.cullingMask &= ~(1 << firstPersonHiddenLayer);
        }

        /// <summary>
        /// Restores a camera's culling mask to include the FirstPersonHidden layer.
        /// </summary>
        /// <param name="vrCamera">The camera to restore.</param>
        public static void RestoreCamera(Camera vrCamera)
        {
            if (vrCamera == null) return;

            int firstPersonHiddenLayer = LayerMask.NameToLayer("FirstPersonHidden");
            if (firstPersonHiddenLayer < 0) return;

            vrCamera.cullingMask |= (1 << firstPersonHiddenLayer);
        }

        private void ResolveHeadRenderer()
        {
            if (_animator == null || !_animator.isHuman) return;

            var headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
            if (headBone == null) return;

            _headRenderer = headBone.GetComponentInChildren<Renderer>();
        }

        /// <summary>
        /// Injects a head renderer and marks controller as initialized for testing.
        /// Bypasses full IK setup which requires Animation.Rigging in scene context.
        /// Only available to test assemblies.
        /// </summary>
        internal void SetHeadRendererForTesting(Renderer renderer)
        {
            _headRenderer = renderer;
            _isInitialized = true;
        }

        private void LateUpdate()
        {
            if (!_isInitialized) return;
            UpdateTracking();
        }

        private void SetupRigBuilder()
        {
            // Add RigBuilder to the entity GameObject (same as Animator)
            _rigBuilder = gameObject.GetComponent<RigBuilder>();
            if (_rigBuilder == null)
            {
                _rigBuilder = gameObject.AddComponent<RigBuilder>();
            }

            // Create Rig child object
            _rigObject = new GameObject("AvatarRig");
            _rigObject.transform.SetParent(transform, false);
            _rig = _rigObject.AddComponent<Rig>();

            // Add rig to builder layers
            _rigBuilder.layers.Clear();
            _rigBuilder.layers.Add(new RigLayer(_rig));
        }

        private void SetupIKTargets()
        {
            // Create empty target transforms under the rig object
            var headTargetGO = new GameObject("HeadTarget");
            headTargetGO.transform.SetParent(_rigObject.transform, false);
            _headTarget = headTargetGO.transform;

            var leftHandTargetGO = new GameObject("LeftHandTarget");
            leftHandTargetGO.transform.SetParent(_rigObject.transform, false);
            _leftHandTarget = leftHandTargetGO.transform;

            var rightHandTargetGO = new GameObject("RightHandTarget");
            rightHandTargetGO.transform.SetParent(_rigObject.transform, false);
            _rightHandTarget = rightHandTargetGO.transform;
        }

        private void SetupConstraints()
        {
            // HeadAim — MultiAimConstraint
            var headAimGO = new GameObject("HeadAim");
            headAimGO.transform.SetParent(_rigObject.transform, false);
            _headAimConstraint = headAimGO.AddComponent<MultiAimConstraint>();

            // LeftHandIK — TwoBoneIKConstraint
            var leftHandIKGO = new GameObject("LeftHandIK");
            leftHandIKGO.transform.SetParent(_rigObject.transform, false);
            _leftHandIKConstraint = leftHandIKGO.AddComponent<TwoBoneIKConstraint>();

            // RightHandIK — TwoBoneIKConstraint
            var rightHandIKGO = new GameObject("RightHandIK");
            rightHandIKGO.transform.SetParent(_rigObject.transform, false);
            _rightHandIKConstraint = rightHandIKGO.AddComponent<TwoBoneIKConstraint>();
        }

        private void BindConstraintsToSkeleton()
        {
            if (_animator == null || !_animator.isHuman)
            {
                return;
            }

            // Resolve humanoid bones from Animator
            var headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
            var leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            var rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            var leftUpperArm = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var leftLowerArm = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            var rightUpperArm = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            var rightLowerArm = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);

            // Configure HeadAim constraint
            if (_headAimConstraint != null && headBone != null)
            {
                _headAimConstraint.data.constrainedObject = headBone;
                var sourceObjects = new WeightedTransformArray(1);
                sourceObjects.SetTransform(0, _headTarget);
                sourceObjects.SetWeight(0, 1f);
                _headAimConstraint.data.sourceObjects = sourceObjects;
            }

            // Configure LeftHandIK constraint
            if (_leftHandIKConstraint != null)
            {
                _leftHandIKConstraint.data.root = leftUpperArm;
                _leftHandIKConstraint.data.mid = leftLowerArm;
                _leftHandIKConstraint.data.tip = leftHand;
                _leftHandIKConstraint.data.target = _leftHandTarget;
                _leftHandIKConstraint.data.targetPositionWeight = 1f;
                _leftHandIKConstraint.data.targetRotationWeight = 1f;
            }

            // Configure RightHandIK constraint
            if (_rightHandIKConstraint != null)
            {
                _rightHandIKConstraint.data.root = rightUpperArm;
                _rightHandIKConstraint.data.mid = rightLowerArm;
                _rightHandIKConstraint.data.tip = rightHand;
                _rightHandIKConstraint.data.target = _rightHandTarget;
                _rightHandIKConstraint.data.targetPositionWeight = 1f;
                _rightHandIKConstraint.data.targetRotationWeight = 1f;
            }
        }
    }
}
