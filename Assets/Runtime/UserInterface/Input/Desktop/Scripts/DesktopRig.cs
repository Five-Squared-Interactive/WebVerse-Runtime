// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using FiveSQD.StraightFour.Entity;
using UnityEngine;

namespace FiveSQD.WebVerse.Input.Desktop
{
    /// <summary>
    /// Class for a Desktop rig that manages desktop locomotion.
    /// </summary>
    public class DesktopRig : MonoBehaviour
    {
        /// <summary>
        /// The avatar entity for movement and character control.
        /// </summary>
        [Tooltip("The avatar entity for movement and character control.")]
        public CharacterEntity avatarEntity;

        /// <summary>
        /// The camera transform for mouse look.
        /// </summary>
        [Tooltip("The camera transform for mouse look.")]
        public Transform cameraTransform;

        /// <summary>
        /// The rig origin/root transform.
        /// </summary>
        [Tooltip("The rig origin/root transform.")]
        public GameObject rigOrigin;

        /// <summary>
        /// Entities following the rig.
        /// </summary>
        [Tooltip("Entities following the rig.")]
        public List<BaseEntity> rigFollowers;

        /// <summary>
        /// Cycles to wait in between rig follower updates.
        /// </summary>
        [Tooltip("Cycles to wait in between rig follower updates.")]
        public int cyclesPerRigFollowerUpdate;

        /// <summary>
        /// Movement speed for WASD locomotion.
        /// </summary>
        [Tooltip("Movement speed for WASD locomotion.")]
        public float movementSpeed = 5.0f;

        /// <summary>
        /// Mouse sensitivity for look control.
        /// </summary>
        [Tooltip("Mouse sensitivity for look control.")]
        public float mouseSensitivity = 2.0f;

        /// <summary>
        /// Jump strength when jumping is enabled.
        /// </summary>
        [Tooltip("Jump strength when jumping is enabled.")]
        public float jumpStrength = 8.0f;

        /// <summary>
        /// Offset position relative to the character entity.
        /// </summary>
        [Tooltip("Offset position relative to the character entity.")]
        public Vector3 rigOffset = Vector3.zero;

        /// <summary>
        /// Gravity strength when gravity is enabled.
        /// </summary>
        [Tooltip("Gravity strength when gravity is enabled.")]
        public float gravityStrength = -9.81f;

        /// <summary>
        /// The rig follower update counter.
        /// </summary>
        private int followersUpdateCount = 0;

        /// <summary>
        /// Current vertical velocity for gravity.
        /// </summary>
        private float verticalVelocity = 0f;

        /// <summary>
        /// Current rotation around X axis for mouse look.
        /// </summary>
        private float xRotation = 0f;
        
        /// <summary>
        /// Current movement input for continuous movement.
        /// </summary>
        private Vector2 currentMovementInput = Vector2.zero;

        /// <summary>
        /// Current jump input for continuous jumping.
        /// </summary>
        private bool currentJumpInput = false;

        /// <summary>
        /// Current lower input for continuous lowering.
        /// </summary>
        private bool currentLowerInput = false;

        /// <summary>
        /// Whether gravity is enabled for desktop locomotion.
        /// </summary>
        public bool gravityEnabled
        {
            get { return _gravityEnabled; }
            set
            {
                _gravityEnabled = value;
                if (!value)
                {
                    verticalVelocity = 0f; // Reset vertical velocity when gravity is disabled
                }

                if (avatarEntity != null)
                {
                    BaseEntity.EntityPhysicalProperties newProps = new BaseEntity.EntityPhysicalProperties();
                    newProps.gravitational = value;
                    if (!value)
                    {
                        BaseEntity.EntityMotion motion = new BaseEntity.EntityMotion();
                        motion.velocity = new Vector3(0f, 0f, 0f);
                        motion.stationary = true;
                        avatarEntity.SetMotion(motion);
                    }
                    avatarEntity.SetPhysicalProperties(newProps);
                }
            }
        }
        private bool _gravityEnabled = false;

        /// <summary>
        /// Whether WASD motion is enabled for desktop locomotion.
        /// </summary>
        public bool wasdMotionEnabled { get; set; } = false;

        /// <summary>
        /// Whether mouse look is enabled for desktop locomotion.
        /// </summary>
        public bool mouseLookEnabled { get; set; } = false;
        
        /// <summary>
        /// Whether jump is enabled for desktop locomotion.
        /// </summary>
        public bool jumpEnabled { get; set; } = false;

        /// <summary>
        /// Initialize the Desktop rig.
        /// </summary>
        public void Initialize()
        {
            gravityEnabled = false;
            wasdMotionEnabled = false;
            mouseLookEnabled = false;
            rigFollowers = new List<BaseEntity>();

            // Try to find the avatar entity if not assigned
            if (avatarEntity == null && StraightFour.StraightFour.ActiveWorld != null)
            {
                // Look for the avatar entity in the world
                foreach (var entity in StraightFour.StraightFour.ActiveWorld.entityManager.GetAllEntities())
                {
                    if (entity is CharacterEntity characterEntity &&
                        entity.entityTag == "avatar") // Assuming avatar entities are tagged as "avatar"
                    {
                        avatarEntity = characterEntity;
                        break;
                    }
                }
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (rigOrigin == null)
            {
                rigOrigin = gameObject;
            }
        }

        /// <summary>
        /// Terminate the Desktop rig.
        /// </summary>
        public void Terminate()
        {
            rigFollowers = null;
        }

        /// <summary>
        /// Set the avatar entity by tag.
        /// </summary>
        /// <param name="entityTag">Tag of the entity to use as avatar</param>
        public void SetAvatarEntityByTag(string entityTag)
        {
            if (string.IsNullOrEmpty(entityTag) || StraightFour.StraightFour.ActiveWorld == null)
            {
                return;
            }

            // Look for the entity with the specified tag
            foreach (var entity in StraightFour.StraightFour.ActiveWorld.entityManager.GetAllEntities())
            {
                if (entity is CharacterEntity characterEntity && 
                    entity.entityTag == entityTag)
                {
                    avatarEntity = characterEntity;
                    break;
                }
            }

            if (avatarEntity == null)
            {
                Utilities.Logging.LogWarning($"[DesktopRig->SetAvatarEntityByTag] Could not find character entity with tag: {entityTag}");
            }
            else
            {
                BaseEntity.EntityPhysicalProperties newProps = new BaseEntity.EntityPhysicalProperties();
                newProps.gravitational = gravityEnabled;
                avatarEntity.SetPhysicalProperties(newProps);
            }
        }

        /// <summary>
        /// Set the rig offset relative to the character entity.
        /// </summary>
        /// <param name="offset">The offset vector</param>
        public void SetRigOffset(Vector3 offset)
        {
            rigOffset = offset;
            ApplyRigOffset();
        }

        /// <summary>
        /// Set the rig offset from a string representation (e.g., "1.0,2.0,0.5").
        /// </summary>
        /// <param name="offsetString">String representation of the offset</param>
        public void SetRigOffsetFromString(string offsetString)
        {
            if (string.IsNullOrEmpty(offsetString))
            {
                return;
            }

            try
            {
                string[] parts = offsetString.Split(',');
                if (parts.Length == 3)
                {
                    float x = float.Parse(parts[0].Trim());
                    float y = float.Parse(parts[1].Trim());
                    float z = float.Parse(parts[2].Trim());
                    SetRigOffset(new Vector3(x, y, z));
                }
                else
                {
                    Utilities.Logging.LogWarning($"[DesktopRig->SetRigOffsetFromString] Invalid offset format: {offsetString}. Expected format: 'x,y,z'");
                }
            }
            catch (System.Exception e)
            {
                Utilities.Logging.LogWarning($"[DesktopRig->SetRigOffsetFromString] Error parsing offset: {offsetString}. Error: {e.Message}");
            }
        }

        /// <summary>
        /// Apply rig parenting and offset. Should be called after both avatar entity and rig offset are set.
        /// </summary>
        public void ApplyRigParentingAndOffset()
        {
            SetupRigParenting();
        }

        /// <summary>
        /// Setup parenting of the rig to the character entity.
        /// </summary>
        private void SetupRigParenting()
        {
            if (avatarEntity == null || avatarEntity.gameObject == null)
            {
                return;
            }

            avatarEntity.SetVisibility(true);
            avatarEntity.SetInteractionState(BaseEntity.InteractionState.Physical);

            // Parent the rig to the character entity
            transform.SetParent(avatarEntity.transform, false);

            // Apply the offset
            ApplyRigOffset();
        }

        /// <summary>
        /// Apply the current rig offset relative to the character entity.
        /// </summary>
        private void ApplyRigOffset()
        {
            if (avatarEntity != null && transform.parent == avatarEntity.transform)
            {
                transform.localPosition = rigOffset;
            }
        }

        /// <summary>
        /// Apply WASD movement based on input.
        /// </summary>
        /// <param name="moveInput">Movement input vector (x = horizontal, y = vertical)</param>
        public void ApplyMovement(Vector2 moveInput)
        {
            // Store the current movement input for continuous application
            currentMovementInput = moveInput;
        }

        /// <summary>
        /// Apply jump input.
        /// </summary>
        public void ApplyJump()
        {
            if (!jumpEnabled || avatarEntity == null)
            {
                return;
            }

            if (gravityEnabled)
            {
                // Only allow jumping if the avatar is on the ground
                if (!avatarEntity.IsOnSurface())
                {
                    return;
                }

                // Apply jump using the character entity's built-in jump system
                avatarEntity.Jump(jumpStrength / 10);
            }
            else
            {
                // For no gravity, manually adjust vertical position
                UnityEngine.Vector3 currentPosition = avatarEntity.transform.position;
                currentPosition.y += jumpStrength * Time.deltaTime;
                avatarEntity.SetPosition(currentPosition, false, true);
            }
        }

        /// <summary>
        /// Apply jump input state for continuous jumping.
        /// </summary>
        /// <param name="isJumping">Whether jump input is being held</param>
        public void ApplyJumpInput(bool isJumping)
        {
            // Store the current jump input for continuous application
            currentJumpInput = isJumping;
        }

        /// <summary>
        /// Apply lower input.
        /// </summary>
        public void ApplyLower()
        {
            if (!jumpEnabled || avatarEntity == null)
            {
                return;
            }

            if (!gravityEnabled)
            {
                // For no gravity, manually adjust vertical position downward
                UnityEngine.Vector3 currentPosition = avatarEntity.transform.position;
                currentPosition.y -= jumpStrength * Time.deltaTime;
                avatarEntity.SetPosition(currentPosition, false, true);
            }
        }

        public void ApplyLowerInput(bool isLowering)
        {
            // Store the current lower input for continuous application
            currentLowerInput = isLowering;
        }

        /// <summary>
        /// Apply the stored movement input. Called from Update() for continuous movement.
        /// </summary>
        private void ProcessMovement()
        {
            if (!wasdMotionEnabled || avatarEntity == null || currentMovementInput == Vector2.zero)
            {
                return;
            }

            // Calculate movement direction relative to camera
            Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;

            // Remove Y component to keep movement horizontal
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Calculate movement vector
            Vector3 movement = (forward * currentMovementInput.y + right * currentMovementInput.x) * movementSpeed * Time.deltaTime;

            // Apply gravity if enabled (handled by the character entity itself)
            if (gravityEnabled)
            {
                // Use the character entity's built-in movement system which handles gravity
                avatarEntity.Move(new UnityEngine.Vector3(movement.x, movement.y, movement.z));
            }
            else
            {
                // For no gravity, only apply horizontal movement
                avatarEntity.Move(new UnityEngine.Vector3(movement.x, 0f, movement.z));
            }
        }

        /// <summary>
        /// Apply the stored jump input. Called from Update() for continuous jumping.
        /// </summary>
        private void ProcessJump()
        {
            if (!jumpEnabled || avatarEntity == null || !currentJumpInput)
            {
                return;
            }

            if (gravityEnabled)
            {
                // Only allow jumping if the avatar is on the ground
                if (!avatarEntity.IsOnSurface())
                {
                    return;
                }

                // Apply jump using the character entity's built-in jump system
                avatarEntity.Jump(jumpStrength / 10);
            }
            else
            {
                // For no gravity, manually adjust vertical position
                UnityEngine.Vector3 currentPosition = avatarEntity.transform.position;
                currentPosition.y += jumpStrength * Time.deltaTime;
                avatarEntity.SetPosition(currentPosition, false, true);
            }
        }

        private void ProcessLowering()
        {
            if (!jumpEnabled || avatarEntity == null || !currentLowerInput)
            {
                return;
            }

            if (!gravityEnabled)
            {
                UnityEngine.Vector3 currentPosition = avatarEntity.transform.position;
                currentPosition.y -= jumpStrength * Time.deltaTime;
                avatarEntity.SetPosition(currentPosition, false, true);
            }
        }

        /// <summary>
        /// Apply mouse look based on input.
        /// </summary>
        /// <param name="lookInput">Look input vector (x = horizontal, y = vertical)</param>
        public void ApplyLook(Vector2 lookInput)
        {
            if (!mouseLookEnabled || cameraTransform == null)
            {
                return;
            }

            // Apply horizontal rotation to both the rig and avatar entity
            float mouseX = lookInput.x * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);

            // Also rotate the avatar entity left/right
            if (avatarEntity != null)
            {
                // Get current avatar rotation and add horizontal rotation
                UnityEngine.Quaternion currentRotation = avatarEntity.GetRotation(false);
                UnityEngine.Quaternion horizontalRotation = UnityEngine.Quaternion.AngleAxis(mouseX, UnityEngine.Vector3.up);
                avatarEntity.SetRotation(horizontalRotation * currentRotation, false, true);
            }

            // Apply vertical rotation to the camera
            float mouseY = lookInput.y * mouseSensitivity;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        void Update()
        {
            // Process continuous movement
            ProcessMovement();

            // Process continuous jumping
            ProcessJump();

            // Process continuous lowering
            ProcessLowering();

            // Update rig followers similar to VRRig
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

                followersUpdateCount = 0;
            }
        }
    }
}