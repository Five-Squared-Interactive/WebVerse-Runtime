// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Utilities;
using OMI.Extensions.Seat;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI
{
    /// <summary>
    /// Behavior component for OMI seat interaction.
    /// Allows players to sit in this seat with proper positioning.
    /// </summary>
    public class OMISeatBehavior : MonoBehaviour
    {
        /// <summary>
        /// Offset for the player's back when seated (for IK).
        /// </summary>
        [Tooltip("Offset for the player's back when seated (for IK).")]
        public Vector3 BackOffset;

        /// <summary>
        /// Offset for the player's feet when seated (for IK).
        /// </summary>
        [Tooltip("Offset for the player's feet when seated (for IK).")]
        public Vector3 FootOffset;

        /// <summary>
        /// Rotation offset for the seated player.
        /// </summary>
        [Tooltip("Rotation offset for the seated player.")]
        public Quaternion SeatRotation = Quaternion.identity;

        /// <summary>
        /// Whether someone is currently sitting in this seat.
        /// </summary>
        public bool IsOccupied { get; private set; }

        /// <summary>
        /// The entity currently occupying this seat.
        /// </summary>
        private CharacterEntity occupant;

        /// <summary>
        /// Original parent of the occupant before sitting.
        /// </summary>
        private Transform originalOccupantParent;

        /// <summary>
        /// Original position of the occupant before sitting.
        /// </summary>
        private Vector3 originalOccupantPosition;

        /// <summary>
        /// Original rotation of the occupant before sitting.
        /// </summary>
        private Quaternion originalOccupantRotation;

        /// <summary>
        /// Initialize the seat behavior from an OMISeat component.
        /// </summary>
        /// <param name="seat">The OMI seat component.</param>
        public void Initialize(OMISeat seat)
        {
            if (seat != null)
            {
                BackOffset = seat.Back;
                FootOffset = seat.Foot;
                // Angle is around up axis
                SeatRotation = Quaternion.Euler(0, seat.Angle * Mathf.Rad2Deg, 0);
            }
        }

        /// <summary>
        /// Initialize the seat behavior with explicit values.
        /// </summary>
        /// <param name="backOffset">Back offset for IK.</param>
        /// <param name="footOffset">Foot offset for IK.</param>
        /// <param name="angle">Seat angle in radians.</param>
        public void Initialize(Vector3 backOffset, Vector3 footOffset, float angle)
        {
            BackOffset = backOffset;
            FootOffset = footOffset;
            SeatRotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
        }

        /// <summary>
        /// Attempt to sit a character in this seat.
        /// </summary>
        /// <param name="character">The character entity to seat.</param>
        /// <returns>True if successfully seated.</returns>
        public bool Sit(CharacterEntity character)
        {
            if (IsOccupied)
            {
                Logging.LogWarning("[OMISeatBehavior] Seat is already occupied.");
                return false;
            }

            if (character == null)
            {
                Logging.LogWarning("[OMISeatBehavior] Cannot seat null character.");
                return false;
            }

            occupant = character;
            IsOccupied = true;

            // Store original transform info
            originalOccupantParent = character.transform.parent;
            originalOccupantPosition = character.transform.position;
            originalOccupantRotation = character.transform.rotation;

            // Parent character to seat
            character.transform.SetParent(transform);
            character.transform.localPosition = Vector3.zero;
            character.transform.localRotation = SeatRotation;

            // Disable character controller movement
            // This will depend on how CharacterEntity handles movement
            // character.SetMovementEnabled(false);

            Logging.Log($"[OMISeatBehavior] Character seated at {gameObject.name}");

            return true;
        }

        /// <summary>
        /// Make the current occupant stand up from the seat.
        /// </summary>
        /// <returns>True if successfully stood up.</returns>
        public bool Stand()
        {
            if (!IsOccupied || occupant == null)
            {
                Logging.LogWarning("[OMISeatBehavior] No occupant to stand.");
                return false;
            }

            // Restore original parent (or world if null)
            occupant.transform.SetParent(originalOccupantParent);

            // Position character in front of seat
            Vector3 exitPosition = transform.position + transform.forward * 0.5f;
            occupant.transform.position = exitPosition;
            occupant.transform.rotation = transform.rotation;

            // Re-enable character controller movement
            // character.SetMovementEnabled(true);

            Logging.Log($"[OMISeatBehavior] Character stood up from {gameObject.name}");

            occupant = null;
            IsOccupied = false;

            return true;
        }

        /// <summary>
        /// Get the world position for a seated character.
        /// </summary>
        public Vector3 GetSeatPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Get the world rotation for a seated character.
        /// </summary>
        public Quaternion GetSeatRotation()
        {
            return transform.rotation * SeatRotation;
        }

        /// <summary>
        /// Get the world position for back IK target.
        /// </summary>
        public Vector3 GetBackIKPosition()
        {
            return transform.TransformPoint(BackOffset);
        }

        /// <summary>
        /// Get the world position for foot IK target.
        /// </summary>
        public Vector3 GetFootIKPosition()
        {
            return transform.TransformPoint(FootOffset);
        }

        /// <summary>
        /// Called when something enters the trigger.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            // Check if it's a player that can sit
            if (!IsOccupied && IsInteractablePlayer(other))
            {
                // TODO: Show "Press E to sit" prompt or similar
                Logging.Log("[OMISeatBehavior] Player can interact with seat.");
            }
        }

        /// <summary>
        /// Called when something exits the trigger.
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            if (IsInteractablePlayer(other))
            {
                // TODO: Hide interaction prompt
            }
        }

        /// <summary>
        /// Check if a collider belongs to an interactable player.
        /// </summary>
        private bool IsInteractablePlayer(Collider other)
        {
            return other.CompareTag("Player") ||
                   other.GetComponent<CharacterEntity>() != null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw gizmo in editor to visualize the seat.
        /// </summary>
        private void OnDrawGizmos()
        {
            // Draw seat position
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.1f, 0.5f));

            // Draw seat direction
            Gizmos.color = Color.blue;
            Vector3 forward = transform.rotation * SeatRotation * Vector3.forward;
            Gizmos.DrawRay(transform.position, forward * 0.5f);

            // Draw back offset
            if (BackOffset != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Vector3 backPos = transform.TransformPoint(BackOffset);
                Gizmos.DrawWireSphere(backPos, 0.05f);
                Gizmos.DrawLine(transform.position, backPos);
            }

            // Draw foot offset
            if (FootOffset != Vector3.zero)
            {
                Gizmos.color = Color.cyan;
                Vector3 footPos = transform.TransformPoint(FootOffset);
                Gizmos.DrawWireSphere(footPos, 0.05f);
                Gizmos.DrawLine(transform.position, footPos);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.1f, 0.5f));
        }
#endif
    }
}
