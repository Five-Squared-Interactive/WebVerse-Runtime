// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.StraightFour.Utilities;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Base class for a UI entity.
    /// </summary>
    public class UIEntity : BaseEntity
    {
        /// <summary>
        /// Get the motion state for this entity. UI element entities are always motionless.
        /// </summary>
        /// <returns>The motion state for this entity.</returns>
        public override EntityMotion? GetMotion()
        {
            return new EntityMotion
            {
                angularVelocity = Vector3.zero,
                stationary = true,
                velocity = Vector3.zero
            };
        }

        /// <summary>
        /// Get the physical properties for the entity. UI element entities always contain no physical properties.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public override EntityPhysicalProperties? GetPhysicalProperties()
        {
            return new EntityPhysicalProperties
            {
                angularDrag = 0,
                centerOfMass = Vector3.zero,
                drag = 0,
                gravitational = false,
                mass = 0
            };
        }

        /// <summary>
        /// Set the physical properties of the entity. This is an invalid operation for a UI element.
        /// </summary>
        /// <param name="propertiesToSet">Properties to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetPhysicalProperties(EntityPhysicalProperties? epp)
        {
            LogSystem.LogWarning("[UIEntity->SetPhysicalProperties] Physical properties not settable for UI.");

            return false;
        }

        /// <summary>
        /// Set the motion state for this entity. This is an invalid operation for a UI element.
        /// </summary>
        /// <param name="motionToSet">Motion state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetMotion(EntityMotion? motionToSet)
        {
            LogSystem.LogWarning("[UIEntity->SetMotion] Motion not settable for UI.");

            return false;
        }

        /// <summary>
        /// Set the interaction state for the entity. UI elements can only be in a Hidden or Static state.
        /// </summary>
        /// <param name="stateToSet">Interaction state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetInteractionState(InteractionState stateToSet)
        {
            switch (stateToSet)
            {
                case InteractionState.Physical:
                    LogSystem.LogWarning("[UIEntity->SetInteractionState] Physical not valid for UI.");
                    return false;

                case InteractionState.Placing:
                    LogSystem.LogWarning("[UIEntity->SetInteractionState] Placing not valid for UI.");
                    return false;

                case InteractionState.Static:
                    MakeStatic();
                    return true;

                case InteractionState.Hidden:
                    MakeHidden();
                    return true;

                default:
                    LogSystem.LogWarning("UIEntity->SetInteractionState] Interaction state invalid.");
                    return false;
            }
        }

        /// <summary>
        /// Set the visibility state of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to set the entity to visible.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetVisibility(bool visible, bool synchronize = true)
        {
            // Use base functionality.
            return base.SetVisibility(visible, synchronize);
        }

        /// <summary>
        /// Make the entity hidden.
        /// </summary>
        protected virtual void MakeHidden()
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

            gameObject.SetActive(false);
            interactionState = InteractionState.Hidden;
        }

        /// <summary>
        /// Make the entity static.
        /// </summary>
        protected virtual void MakeStatic()
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
            interactionState = InteractionState.Static;
        }
    }
}