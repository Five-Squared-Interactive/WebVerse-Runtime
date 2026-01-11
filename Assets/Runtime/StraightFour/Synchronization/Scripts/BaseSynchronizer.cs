// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using FiveSQD.StraightFour.Entity;
using System.Collections.Generic;

namespace FiveSQD.StraightFour.Synchronization
{
    /// <summary>
    /// Base class for a Synchronizer. The World Engine interfaces with a Synchronizer following
    /// this structure.
    /// </summary>
    public class BaseSynchronizer : MonoBehaviour
    {
        /// <summary>
        /// Status Code Enumeration for Synchronizer Calls.
        /// </summary>
        public enum StatusCode { SUCCESS = 0, FAILED = 1, UNSUPPORTED = 99 }

        /// <summary>
        /// Method to set the visibility of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the visibility of.</param>
        /// <param name="visible">Visibility value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetVisibility(BaseEntity entityToSet, bool visible)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to add a synchronized entity.
        /// </summary>
        /// <param name="entityToSynchronize">The entity to synchronize.</param>
        /// <param name="deleteWithClient">Whether or not to delete the entity when the client leaves.</param>
        /// <param name="filePath">Path to a file containing the entity.</param>
        /// <param name="resources">Paths to additonal file resources for the entity.</param>
        /// <param name="modelOffset">Offset for a model.</param>
        /// <param name="modelRotation">Rotation for a model.</param>
        /// <param name="labelOffset">Offset for a label.</param>
        /// <param name="mass">Mass for an airplane/automobile entity.</param>
        /// <param name="type">Type of the entity.</param>
        /// <param name="wheels">Wheels for an automobile entity.</param>
        /// <param name="menuOptions">Options for a menu.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode AddSynchronizedEntity(BaseEntity entityToSynchronize, bool deleteWithClient,
            string filePath = null, string[] resources = null, Vector3? modelOffset = null, Quaternion? modelRotation = null,
            Vector3? labelOffset = null, float? mass = null, string type = null, Dictionary<string, float> wheels = null,
            string[] menuOptions = null)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to remove a synchronized entity.
        /// </summary>
        /// <param name="entityToUnSynchronize">The entity to remove.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode RemoveSynchronizedEntity(BaseEntity entityToUnSynchronize)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to delete a synchronized entity.
        /// </summary>
        /// <param name="entityToDelete">The entity to delete.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode DeleteSynchronizedEntity(BaseEntity entityToDelete)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the highlight of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the highlight of.</param>
        /// <param name="highlight">Highlight value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetHighlight(BaseEntity entityToSet, bool highlight)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the parent of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the parent of.</param>
        /// <param name="parentToSet">Parent to set.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetParent(BaseEntity entityToSet, BaseEntity parentToSet)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the position of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the position of.</param>
        /// <param name="position">Position value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetPosition(BaseEntity entityToSet, Vector3 position)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the position of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the position of.</param>
        /// <param name="position">Position value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetPositionPercent(BaseEntity entityToSet, Vector2 position)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the rotation of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the rotation of.</param>
        /// <param name="rotation">Rotation value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetRotation(BaseEntity entityToSet, Quaternion rotation)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the scale of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the scale of.</param>
        /// <param name="scale">Scale value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetScale(BaseEntity entityToSet, Vector3 scale)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the size of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the size of.</param>
        /// <param name="size">Size value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetSize(BaseEntity entityToSet, Vector3 size)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the size of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the size of.</param>
        /// <param name="size">Size value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetSizePercent(BaseEntity entityToSet, Vector2 size)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to modify a synchronized terrain entity.
        /// </summary>
        /// <param name="entityToSet">Terrain entity to modify.</param>
        /// <param name="modification">Modification to be performed.</param>
        /// <param name="position">Position value.</param>
        /// <param name="brushType">Brush type.</param>
        /// <param name="layer">Layer.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode ModifyTerrainEntity(BaseEntity entityToSet,
            HybridTerrainEntity.TerrainOperation modification, Vector3 position,
            Entity.Terrain.TerrainEntityBrushType brushType, int layer)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the physical properties of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the physical properties of.</param>
        /// <param name="properties">Properties value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetPhysicalProperties(BaseEntity entityToSet,
            BaseEntity.EntityPhysicalProperties? properties)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the interaction state of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the interaction state of.</param>
        /// <param name="state">Interaction State value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetInteractionState(BaseEntity entityToSet,
            BaseEntity.InteractionState? state)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to set the motion of a synchronized entity.
        /// </summary>
        /// <param name="entityToSet">The entity to set the motion of.</param>
        /// <param name="motion">Motion value.</param>
        /// <returns><see cref="StatusCode"/>StatusCode</returns>
        public virtual StatusCode SetMotion(BaseEntity entityToSet,
            BaseEntity.EntityMotion? motion)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to make a synchronized canvas entity a world canvas.
        /// </summary>
        /// <param name="entityToSet">The entity to make a world canvas.</param>
        /// <returns><see cref="StatusCode">StatusCode</returns>
        public virtual StatusCode MakeWorldCanvas(CanvasEntity entityToSet)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to make a synchronized canvas entity a screen canvas.
        /// </summary>
        /// <param name="entityToSet">The entity to make a screen canvas.</param>
        /// <returns><see cref="StatusCode">StatusCode</returns>
        public virtual StatusCode MakeScreenCanvas(CanvasEntity entityToSet)
        {
            return StatusCode.UNSUPPORTED;
        }

        /// <summary>
        /// Method to send a text message.
        /// </summary>
        /// <param name="topic">Topic for the message.</param>
        /// <param name="message">Message to send.</param>
        /// <returns><see cref="StatusCode">StatusCode</returns>
        public virtual StatusCode SendMessage(string topic, string message)
        {
            return StatusCode.UNSUPPORTED;
        }
    }
}