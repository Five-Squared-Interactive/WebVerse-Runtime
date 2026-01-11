// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using UnityEngine;
using TMPro;
using FiveSQD.StraightFour.Materials;
using FiveSQD.StraightFour.Tags;
using FiveSQD.StraightFour.Utilities;
using System.Collections.Generic;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for a character entity.
    /// </summary>
    public class CharacterEntity : BaseEntity
    {
        /// <summary>
        /// Tag for the entity.
        /// </summary>
        public override string entityTag
        {
            get => base.entityTag;
            set
            {
                base.entityTag = value;
                TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = value;
                }
            }
        }

        /// <summary>
        /// Center of the character model.
        /// </summary>
        public float center;

        /// <summary>
        /// Radius of the character model.
        /// </summary>
        [Range(0, float.MaxValue)]
        public float radius;

        /// <summary>
        /// Height of the character model.
        /// </summary>
        [Range(0, float.MaxValue)]
        public float height;

        /// <summary>
        /// The offset of the character object.
        /// </summary>
        [Tooltip("The offset of the character object.")]
        public Vector3 characterObjectOffset { get; private set; }

        /// <summary>
        /// The rotation of the character object.
        /// </summary>
        [Tooltip("The rotation of the character object.")]
        public Quaternion characterObjectRotation { get; private set; }

        /// <summary>
        /// The offset of the character label.
        /// </summary>
        [Tooltip("The offset of the character label.")]
        public Vector3 characterLabelOffset { get; private set; }

        /// <summary>
        /// Whether or not to fix the height if below ground.
        /// </summary>
        [Tooltip("Whether or not to fix the height if below ground.")]
        public bool fixHeight = true;

        /// <summary>
        /// Minimum height to allow character entity to be at.
        /// </summary>
        [Tooltip("Minimum height to allow character entity to be at.")]
        public float minHeight = -8192;

        /// <summary>
        /// Maximum height to allow character entity to be at.
        /// </summary>
        [Tooltip("Maximum height to allow character entity to be at.")]
        public float maxHeight = 8192;

        /// <summary>
        /// Character model GameObject.
        /// </summary>
        private GameObject characterGO;

        /// <summary>
        /// Meshes on the character model.
        /// </summary>
        private Mesh[] meshes;

        /// <summary>
        /// Original size of the mesh.
        /// </summary>
        private Vector3 originalMeshSize;

        /// <summary>
        /// Current scaling of the mesh.
        /// </summary>
        private Vector3 meshScaling;

        /// <summary>
        /// Capsule collider used by the character.
        /// </summary>
        private CapsuleCollider capsuleCollider;

        /// <summary>
        /// Character controller used by the character.
        /// </summary>
        private CharacterController characterController;

        /// <summary>
        /// Rigidbody used by the character.
        /// </summary>
        private Rigidbody rigidBody;

        /// <summary>
        /// Whether or not the character is gravitational.
        /// </summary>
        private bool gravitational = false;

        /// <summary>
        /// Cube used for highlighting the character entity.
        /// </summary>
        private GameObject highlightCube;

        /// <summary>
        /// The current applied velocity.
        /// </summary>
        private Vector3 currentVelocity = Vector3.zero;

        /// <summary>
        /// Get the character GameObject.
        /// </summary>
        /// <returns>The current character GameObject.</returns>
        public GameObject GetCharacterGO()
        {
            return characterGO;
        }

        /// <summary>
        /// Set the character GameObject at runtime.
        /// </summary>
        /// <param name="newCharacterGO">The new character GameObject to use.</param>
        /// <param name="synchronize">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetCharacterGO(GameObject newCharacterGO, bool synchronize = true)
        {
            if (newCharacterGO == null)
            {
                LogSystem.LogError("[CharacterEntity->SetCharacterGO] Character GameObject cannot be null.");
                return false;
            }

            // Store old character GameObject
            GameObject oldCharacterGO = characterGO;

            // Set the new character GameObject
            characterGO = newCharacterGO;
            characterGO.SetActive(true);
            characterGO.transform.SetParent(transform);
            characterGO.transform.localPosition = characterObjectOffset;
            characterGO.transform.localRotation = characterObjectRotation;

            // Update meshes for the new character GameObject
            List<Mesh> ms = new List<Mesh>();
            foreach (MeshFilter filt in characterGO.GetComponentsInChildren<MeshFilter>())
            {
                ms.Add(filt.sharedMesh);
            }
            SetRenderers(ms.ToArray());

            // Calculate new bounds
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Mesh m in meshes)
            {
                m.RecalculateBounds();
                bounds.Encapsulate(m.bounds);
            }
            originalMeshSize = bounds.size;

            // Clean up old character GameObject
            if (oldCharacterGO != null)
            {
                DestroyImmediate(oldCharacterGO);
            }

            GameObject characterLabel = Instantiate(StraightFour.ActiveWorld.entityManager.characterControllerLabelPrefab);
            characterLabel.transform.SetParent(characterGO.transform);
            Billboard billboard = characterLabel.transform.parent.gameObject.AddComponent<Billboard>();

            // Find the character label and update its text.
            TextMeshProUGUI[] labels = characterGO.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI label in labels)
            {
                label.text = entityTag;
                break; // Assume there's only one label per character
            }

            if (synchronize && synchronizer != null)
            {
                // Note: Synchronization logic would go here if needed
            }

            return true;
        }

        /// <summary>
        /// Set the character object offset at runtime.
        /// </summary>
        /// <param name="newOffset">The new offset to apply.</param>
        /// <param name="synchronize">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetCharacterObjectOffset(Vector3 newOffset, bool synchronize = true)
        {
            if (characterGO == null)
            {
                LogSystem.LogError("[CharacterEntity->SetCharacterObjectOffset] No character GameObject.");
                return false;
            }

            characterObjectOffset = newOffset;
            characterGO.transform.localPosition = characterObjectOffset;

            if (synchronize && synchronizer != null)
            {
                // Note: Synchronization logic would go here if needed
            }

            return true;
        }

        /// <summary>
        /// Set the character object rotation at runtime.
        /// </summary>
        /// <param name="newRotation">The new rotation to apply.</param>
        /// <param name="synchronize">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetCharacterObjectRotation(Quaternion newRotation, bool synchronize = true)
        {
            if (characterGO == null)
            {
                LogSystem.LogError("[CharacterEntity->SetCharacterObjectRotation] No character GameObject.");
                return false;
            }

            characterObjectRotation = newRotation;
            characterGO.transform.localRotation = characterObjectRotation;

            if (synchronize && synchronizer != null)
            {
                // Note: Synchronization logic would go here if needed
            }

            return true;
        }

        /// <summary>
        /// Set the character label offset at runtime.
        /// </summary>
        /// <param name="newOffset">The new offset to apply.</param>
        /// <param name="synchronize">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public bool SetCharacterLabelOffset(Vector3 newOffset, bool synchronize = true)
        {
            if (characterGO == null)
            {
                LogSystem.LogError("[CharacterEntity->SetCharacterLabelOffset] No character GameObject.");
                return false;
            }

            characterLabelOffset = newOffset;

            // Find the character label and update its position
            TextMeshProUGUI[] labels = characterGO.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI label in labels)
            {
                label.transform.parent.localPosition = characterLabelOffset;
                break; // Assume there's only one label per character
            }

            if (synchronize && synchronizer != null)
            {
                // Note: Synchronization logic would go here if needed
            }

            return true;
        }

        /// <summary>
        /// Apply motion to the character entity with the given vector.
        /// </summary>
        /// <param name="amount">Amount to move the character entity.</param>
        /// <param name="synchronize">Whether or not to synchronize the move.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Move(Vector3 amount, bool synchronize = true)
        {
            if (characterController == null)
            {
                LogSystem.LogError("[CharacterEntity->Move] No character controller for character entity.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[CharacterEntity->Move] No rigidbody for character entity.");
                return false;
            }

            currentVelocity = new Vector3(currentVelocity.x + amount.x, currentVelocity.y + amount.y, currentVelocity.z + amount.z);
            characterController.Move(amount);

            if (synchronizer != null && synchronize == true)
            {

            }

            return true;
        }

        /// <summary>
        /// Apply a jump to the character entity by the given amount.
        /// </summary>
        /// <param name="amount">Amount to jump the character entity.</param>
        /// <param name="discardIfFalling">Whether or not to discard jump if currently falling.</param>
        /// <param name="synchronize">Whether or not to synchronize the jump.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Jump(float amount, bool discardIfFalling = true, bool synchronize = true)
        {
            if (characterController == null)
            {
                LogSystem.LogError("[CharacterEntity->Jump] No character controller for character entity.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[CharacterEntity->Jump] No rigidbody for character entity.");
                return false;
            }

            if (IsOnSurface() || !discardIfFalling)
            {
                currentVelocity.y += amount;
            }

            if (synchronizer != null && synchronize == true)
            {

            }

            return true;
        }

        /// <summary>
        /// Returns whether or not the character entity is on a surface.
        /// </summary>
        /// <returns>Whether or not the character entity is on a surface.</returns>
        public bool IsOnSurface()
        {
            if (characterController == null)
            {
                LogSystem.LogError("[CharacterEntity->IsOnSurface] No character controller for character entity.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[CharacterEntity->IsOnSurface] No rigidbody for character entity.");
                return false;
            }

            return Physics.Raycast(transform.position - new Vector3(0, characterController.height / 2, 0), Vector3.down, 0.25f);
        }

        public bool IsAboveGround()
        {
            if (characterController == null)
            {
                LogSystem.LogError("[CharacterEntity->IsOnSurface] No character controller for character entity.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[CharacterEntity->IsOnSurface] No rigidbody for character entity.");
                return false;
            }

            return Physics.Raycast(transform.position - new Vector3(0, characterController.height / 2, 0), Vector3.down);
        }

        /// <summary>
        /// Delete the entity.
        /// </summary>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool Delete(bool synchronize = true)
        {
            return base.Delete(synchronize);
        }

        /// <summary>
        /// Get the motion state for this entity.
        /// </summary>
        /// <returns>The motion state for this entity.</returns>
        public override EntityMotion? GetMotion()
        {
            if (characterController == null)
            {
                LogSystem.LogError("[CharacterEntity->GetMotion] No character controller for character entity.");
                return null;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[CharacterEntity->GetMotion] No rigidbody for character entity.");
                return null;
            }

            return new EntityMotion
            {
                angularVelocity = rigidBody.angularVelocity,
                stationary = rigidBody.isKinematic,
                velocity = rigidBody.linearVelocity
            };
        }

        /// <summary>
        /// Get the physical properties for the entity.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public override EntityPhysicalProperties? GetPhysicalProperties()
        {
            if (characterController == null)
            {
                LogSystem.LogError("[CharacterEntity->GetPhysicalProperties] No character controller for character entity.");
                return null;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[CharacterEntity->GetPhysicalProperties] No rigidbody for character entity.");
                return null;
            }

            return new EntityPhysicalProperties
            {
                angularDrag = rigidBody.angularDamping,
                centerOfMass = rigidBody.centerOfMass,
                drag = rigidBody.linearDamping,
                gravitational = gravitational,
                mass = rigidBody.mass
            };
        }

        /// <summary>
        /// Get the size of the entity.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public override Vector3 GetSize()
        {
            return new Vector3(originalMeshSize.x * meshScaling.x,
                originalMeshSize.y * meshScaling.y, originalMeshSize.z * meshScaling.z);
        }

        /// <summary>
        /// Set the highlight state of the entity.
        /// </summary>
        /// <param name="highlight">Whether or not to turn on the highlight.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetHighlight(bool highlight)
        {
            highlightCube.SetActive(highlight);

            return true;
        }

        /// <summary>
        /// Set the interaction state for the entity.
        /// </summary>
        /// <param name="stateToSet">Interaction state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetInteractionState(InteractionState stateToSet)
        {
            switch (stateToSet)
            {
                case InteractionState.Physical:
                    MakePhysical();
                    return true;

                case InteractionState.Placing:
                    LogSystem.LogWarning("[CharacterEntity->SetInteractionState] Placing not valid for character.");
                    return true;

                case InteractionState.Static:
                    MakeStatic();
                    return true;

                case InteractionState.Hidden:
                    MakeHidden();
                    return true;

                default:
                    LogSystem.LogWarning("[CharacterEntity->SetInteractionState] Interaction state invalid.");
                    return false;
            }
        }

        /// <summary>
        /// Set the motion state for this entity.
        /// </summary>
        /// <param name="motionToSet">Motion state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetMotion(EntityMotion? motionToSet)
        {
            if (!motionToSet.HasValue)
            {
                LogSystem.LogWarning("[CharacterEntity->SetMotion] Invalid motion.");
                return false;
            }

            if (characterController == null)
            {
                LogSystem.LogError("[CharacterEntity->SetPhysicalProperties] No character controller for character entity.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[CharacterEntity->SetPhysicalProperties] No rigidbody for character entity.");
                return false;
            }

            if (motionToSet.Value.stationary.HasValue)
            {
                if (motionToSet.Value.stationary == true)
                {
                    rigidBody.isKinematic = true;
                    rigidBody.useGravity = false;
                    rigidBody.angularVelocity = Vector3.zero;
                    rigidBody.linearVelocity = Vector3.zero;
                    return true;
                }
                else
                {
                    rigidBody.isKinematic = false;
                    rigidBody.useGravity = gravitational;
                }
            }

            if (motionToSet.Value.angularVelocity != null)
            {
                rigidBody.angularVelocity = motionToSet.Value.angularVelocity;
            }

            if (motionToSet.Value.velocity != null)
            {
                rigidBody.linearVelocity = motionToSet.Value.velocity;
            }

            return true;
        }

        /// <summary>
        /// Set the physical properties of the entity.
        /// </summary>
        /// <param name="propertiesToSet">Properties to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetPhysicalProperties(EntityPhysicalProperties? epp)
        {
            if (!epp.HasValue)
            {
                LogSystem.LogWarning("[CharacterEntity->SetPhysicalProperties] Invalid physical properties.");
                return false;
            }

            if (characterController == null)
            {
                LogSystem.LogError("[CharacterEntity->SetPhysicalProperties] No character controller for character entity.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[CharacterEntity->SetPhysicalProperties] No rigidbody for character entity.");
                return false;
            }

            if (epp.Value.angularDrag.HasValue)
            {
                rigidBody.angularDamping = epp.Value.angularDrag.Value;
            }

            if (epp.Value.centerOfMass != null)
            {
                rigidBody.centerOfMass = epp.Value.centerOfMass.Value;
            }

            if (epp.Value.drag.HasValue)
            {
                rigidBody.linearDamping = epp.Value.drag.Value;
            }

            if (epp.Value.gravitational.HasValue)
            {
                gravitational = epp.Value.gravitational.Value;
                rigidBody.useGravity = gravitational;
            }

            if (epp.Value.mass.HasValue)
            {
                rigidBody.mass = epp.Value.mass.Value;
            }

            return true;
        }

        /// <summary>
        /// Set the size of the entity.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetSize(Vector3 size, bool synchronize = true)
        {
            if (characterGO == null)
            {
                LogSystem.LogError("[CharacterEntity->SetSize] No Character Object.");
                return false;
            }

            float xSclFactor = size.x / originalMeshSize.x;
            float ySclFactor = size.y / originalMeshSize.y;
            float zSclFactor = size.z / originalMeshSize.z;
            meshScaling = new Vector3(xSclFactor, ySclFactor, zSclFactor);
            transform.localScale = new Vector3(transform.localScale.x * xSclFactor,
                transform.localScale.y * ySclFactor, transform.localScale.z * zSclFactor);

            return true;
        }

        /// <summary>
        /// Set the visibility state of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to set the entity to visible.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetVisibility(bool visible, bool synchronize = true)
        {
            // Use base functionality.
            //return base.SetVisibility(visible);
            if (meshes != null)
            {
                foreach (MeshRenderer ms in characterGO.gameObject.GetComponentsInChildren<MeshRenderer>(true))
                {
                    ms.enabled = visible;
                }
            }
            //characterGO.gameObject.SetActive(visible);
            if (synchronizer != null && synchronize == true)
            {
                synchronizer.SetVisibility(this, visible);
            }
            return true;
        }

        /// <summary>
        /// Initialize this entity. This method cannot be used for a character entity; it must be called
        /// with a character object prefab (nullable).
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /*public override void Initialize(Guid idToSet)
        {
            LogSystem.LogError("[CharacterEntity->Initialize] Character entity must be " +
                "initialized with a character object prefab (nullable).");

            return;
        }*/

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        /// <param name="characterObjectPrefab">Prefab to use for the character object.</param>
        /// <param name="characterObjectOffset">Offset for the character object.</param>
        /// <param name="characterObjectRotation">Rotation for the character object.</param>
        /// <param name="avatarLabelOffset">Offset for the avatar label.</param>
        public void Initialize(Guid idToSet, GameObject characterObjectPrefab,
            Vector3 characterObjectOffset, Quaternion characterObjectRotation, Vector3 avatarLabelOffset)
        {
            base.Initialize(idToSet);

            if (characterObjectPrefab == null)
            {
                characterGO = Instantiate(StraightFour.ActiveWorld.entityManager.characterControllerPrefab);

                // Check if the default prefab has a label, if not create one
                TextMeshProUGUI existingLabel = characterGO.GetComponentInChildren<TextMeshProUGUI>();
                if (existingLabel == null && StraightFour.ActiveWorld.entityManager.characterControllerLabelPrefab != null)
                {
                    GameObject characterLabel = Instantiate(StraightFour.ActiveWorld.entityManager.characterControllerLabelPrefab);
                    characterLabel.transform.SetParent(characterGO.transform);
                    characterLabel.transform.localPosition = avatarLabelOffset;

                    // Add Billboard component to make the label always face the camera
                    Billboard billboard = characterLabel.GetComponent<Billboard>();
                    if (billboard == null)
                    {
                        billboard = characterLabel.transform.parent.gameObject.AddComponent<Billboard>();
                    }
                    // Lock X axis for character labels to prevent tilting
                    billboard.lockXAxis = true;

                    characterLabel.SetActive(true);
                }
                else if (existingLabel != null)
                {
                    // Add Billboard component to existing label
                    Billboard billboard = existingLabel.GetComponent<Billboard>();
                    if (billboard == null)
                    {
                        billboard = existingLabel.gameObject.AddComponent<Billboard>();
                    }
                    // Lock X axis for character labels to prevent tilting
                    billboard.lockXAxis = true;
                }
            }
            else
            {
                characterGO = Instantiate(characterObjectPrefab);
                characterGO.SetActive(true);
                GameObject characterLabel = Instantiate(StraightFour.ActiveWorld.entityManager.characterControllerLabelPrefab);
                characterLabel.transform.SetParent(characterGO.transform);
                characterLabel.transform.localPosition = avatarLabelOffset;
                //characterLabel.transform.localRotation = Quaternion.identity;
                //characterLabel.transform.localScale = Vector3.one;

                // Add Billboard component to make the label always face the camera
                Billboard billboard = characterLabel.GetComponent<Billboard>();
                if (billboard == null)
                {
                    billboard = characterLabel.AddComponent<Billboard>();
                }
                // Lock X axis for character labels to prevent tilting
                billboard.lockXAxis = true;

                characterLabel.SetActive(true);
            }
            characterGO.transform.SetParent(transform);
            characterGO.transform.localPosition = characterObjectOffset;
            characterGO.transform.localRotation = characterObjectRotation;
            this.characterObjectOffset = characterObjectOffset;
            this.characterObjectRotation = characterObjectRotation;
            this.characterLabelOffset = avatarLabelOffset;

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            SetRigidbody(rb);

            List<Mesh> ms = new List<Mesh>();
            foreach (MeshFilter filt in characterGO.GetComponentsInChildren<MeshFilter>())
            {
                ms.Add(filt.sharedMesh);
            }
            SetRenderers(ms.ToArray());
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Mesh m in meshes)
            {
                m.RecalculateBounds();
                bounds.Encapsulate(m.bounds);
            }
            originalMeshSize = bounds.size;
            meshScaling = Vector3.one;

            CapsuleCollider capsuleCollider = null;
            foreach (CapsuleCollider cc in gameObject.GetComponentsInChildren<CapsuleCollider>())
            {
                if (cc.tag == TagManager.physicsColliderTag)
                {
                    capsuleCollider = cc;
                    break;
                }
            }

            if (capsuleCollider == null)
            {
                capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            }

            SetColliders(capsuleCollider);

            CharacterController characterController = gameObject.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
            }

            SetController(characterController);

            MakeHidden();
            SetUpHighlightVolume();
        }

        /// <summary>
        /// Tear down the entity.
        /// </summary>
        public override void TearDown()
        {
            base.TearDown();
        }

        /// <summary>
        /// Set mesh renderers for the entity.
        /// </summary>
        /// <param name="ms">Meshes to apply.</param>
        private void SetRenderers(Mesh[] ms)
        {
            if (ms == null)
            {
                LogSystem.LogWarning("[CharacterEntity->SetRenderer] No mesh.");
            }
            meshes = ms;
        }

        /// <summary>
        /// Set colliders for the entity.
        /// </summary>
        /// <param name="cc">Colliders to apply.</param>
        private void SetColliders(CapsuleCollider cc)
        {
            if (cc == null)
            {
                LogSystem.LogWarning("[CharacterEntity->SetColliders] No capsule collider.");
            }
            capsuleCollider = cc;
            capsuleCollider.center = new Vector3(0, center, 0);
            capsuleCollider.radius = radius;
            capsuleCollider.height = height;
        }

        /// <summary>
        /// Set rigidbody for the entity.
        /// </summary>
        /// <param name="rb">Rigidbody to apply.</param>
        private void SetRigidbody(Rigidbody rb)
        {
            if (rb == null)
            {
                LogSystem.LogWarning("[CharacterEntity->SetRigidbody] No rigidbody.");
            }
            rigidBody = rb;
        }

        /// <summary>
        /// Set character controller for the entity.
        /// </summary>
        /// <param name="cc">Character controller to apply.</param>
        private void SetController(CharacterController cc)
        {
            if (cc == null)
            {
                LogSystem.LogWarning("[CharacterEntity->SetController] No character controller.");
            }
            characterController = cc;
            characterController.slopeLimit = 45;
            characterController.stepOffset = Math.Min(0.3f, 0.9f * height);
            characterController.skinWidth = radius * 0.1f;
            characterController.minMoveDistance = 0;
            characterController.radius = radius;
            characterController.height = height;
        }

        /// <summary>
        /// Make the entity hidden.
        /// </summary>
        private void MakeHidden()
        {
            switch (interactionState)
            {
                case InteractionState.Physical:
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                    break;

                case InteractionState.Hidden:
                default:
                    break;
            }

            rigidBody.isKinematic = true;

            capsuleCollider.enabled = false;
            gameObject.SetActive(false);
            interactionState = InteractionState.Hidden;
        }

        /// <summary>
        /// Make the entity static.
        /// </summary>
        private void MakeStatic()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    // Handled in main sequence.
                    break;

                case InteractionState.Physical:
                    // Handled in main sequence.
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                default:
                    break;
            }

            gameObject.SetActive(true);
            rigidBody.isKinematic = true;
            capsuleCollider.enabled = false;
            interactionState = InteractionState.Static;
        }

        /// <summary>
        /// Make the entity physical.
        /// </summary>
        private void MakePhysical()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    // Handled in main sequence.
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                    // Handled in main sequence.
                    break;

                case InteractionState.Physical:
                default:
                    break;
            }

            gameObject.SetActive(true);
            interactionState = InteractionState.Physical;
        }

        /// <summary>
        /// Set up the highlight volume for the entity.
        /// </summary>
        private void SetUpHighlightVolume()
        {
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            MeshRenderer[] rends = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rend in rends)
            {
                bounds.Encapsulate(rend.bounds);
            }

            bounds.center = bounds.center - transform.position;

            highlightCube = new GameObject("HighlightVolume");

            Vector3[] vertices =
            {
                    new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                    new Vector3 (bounds.max.x, bounds.min.y, bounds.min.z),
                    new Vector3 (bounds.max.x, bounds.max.y, bounds.min.z),
                    new Vector3 (bounds.min.x, bounds.min.y, bounds.min.z),
                    new Vector3 (bounds.min.x, bounds.max.y, bounds.max.z),
                    new Vector3 (bounds.max.x, bounds.max.y, bounds.max.z),
                    new Vector3 (bounds.max.x, bounds.min.y, bounds.max.z),
                    new Vector3 (bounds.min.x, bounds.min.y, bounds.max.z),
                };

            int[] triangles =
            {
                    0, 2, 1, //face front
			        0, 3, 2,
                    2, 3, 4, //face top
			        2, 4, 5,
                    1, 2, 5, //face right
			        1, 5, 6,
                    0, 7, 4, //face left
			        0, 4, 3,
                    5, 4, 7, //face back
			        5, 7, 6,
                    0, 6, 7, //face bottom
			        0, 1, 6
                };

            Mesh mesh = highlightCube.AddComponent<MeshFilter>().mesh;
            MeshRenderer hRend = highlightCube.AddComponent<MeshRenderer>();
            hRend.material = MaterialManager.HighlightMaterial;
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.Optimize();
            mesh.RecalculateNormals();

            highlightCube.transform.SetParent(transform);
            highlightCube.transform.localPosition = Vector3.zero;
            highlightCube.transform.localRotation = Quaternion.identity;
            highlightCube.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
            highlightCube.SetActive(false);
        }

        private float timeToWaitForUpdate = 0.025f;
        private float timeWaitedForUpdate = 0;
        private int stepToRaise = 1;
        private int maxStepToRaise = 1024;
        void FixedUpdate()
        {
            timeWaitedForUpdate += Time.deltaTime;
            if (timeWaitedForUpdate >= timeToWaitForUpdate)
            {
                timeWaitedForUpdate = 0;
            }
            else
            {
                return;
            }

            if (characterController == null)
            {
                //LogSystem.LogError("[CharacterEntity->Update] No character controller for character entity.");
                return;
            }

            if (IsOnSurface() && currentVelocity.y < 0)
            {
                currentVelocity.y = 0f;
            }

            if (rigidBody.useGravity)
            {
                currentVelocity.y += -9.81f * Time.deltaTime; // TODO: Magic number, tie into larger gravity system.
            }
            characterController.Move(currentVelocity);
            currentVelocity.x = currentVelocity.z = 0;

            if (fixHeight)
            {
                if (!IsAboveGround())
                {
                    characterController.transform.position = new Vector3(characterController.transform.position.x,
                        characterController.transform.position.y + (stepToRaise *= 2), characterController.transform.position.z);
                    if (stepToRaise > maxStepToRaise)
                    {
                        stepToRaise = maxStepToRaise;
                    }
                }
                else
                {
                    stepToRaise = 1;
                }

                if (characterController.transform.position.y < minHeight)
                {
                    characterController.transform.position = new Vector3(characterController.transform.position.x,
                        maxHeight - 1, characterController.transform.position.z);
                }

                if (characterController.transform.position.y > maxHeight)
                {
                    characterController.transform.position = new Vector3(characterController.transform.position.x,
                        minHeight + 1, characterController.transform.position.z);
                }
            }
        }
    }
}