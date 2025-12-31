// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.Seat;
using UnityEngine;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_seat.
    /// Creates seat components for character IK positioning.
    /// </summary>
    public class StraightFourSeatHandler : StraightFourNodeHandlerBase<OMISeatNode>
    {
        public override string ExtensionName => OMISeatExtension.ExtensionName;
        public override int Priority => 50;

        public override Task OnNodeImportAsync(OMISeatNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null)
            {
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing seat for node {nodeIndex}: {targetObject.name}");

            // Add seat component
            var seatComponent = targetObject.AddComponent<OMISeatBehavior>();
            seatComponent.Initialize(data);

            // Find parent entity using glTF parent node index
            BaseEntity parentEntity = null;
            if (context.CustomData != null && context.CustomData.TryGetValue("SF_NodeParentIndices", out var parentMapObj))
            {
                var parentMap = parentMapObj as Dictionary<int, int>;
                if (parentMap != null && parentMap.TryGetValue(nodeIndex, out var parentNodeIndex))
                {
                    parentEntity = GetEntityForNode(context, parentNodeIndex);
                }
            }
            // Create entity for the seat with correct parent
            GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);

            Logging.Log($"[StraightFour] Created seat at {targetObject.name}");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Behavior component for OMI seat functionality.
    /// Provides IK target positions for seated characters.
    /// </summary>
    public class OMISeatBehavior : MonoBehaviour
    {
        /// <summary>
        /// Back/hip position limit in local space.
        /// </summary>
        public Vector3 BackPosition { get; private set; }

        /// <summary>
        /// Foot position limit in local space.
        /// </summary>
        public Vector3 FootPosition { get; private set; }

        /// <summary>
        /// Knee base position in local space.
        /// </summary>
        public Vector3 KneePosition { get; private set; }

        /// <summary>
        /// Angle between spine and back-knee line in radians.
        /// </summary>
        public float Angle { get; private set; }

        /// <summary>
        /// Whether the seat is currently occupied.
        /// </summary>
        public bool IsOccupied { get; private set; }

        /// <summary>
        /// The character currently occupying the seat.
        /// </summary>
        public GameObject OccupyingCharacter { get; private set; }

        public void Initialize(OMISeatNode data)
        {
            BackPosition = data.Back != null && data.Back.Length >= 3 
                ? new Vector3(data.Back[0], data.Back[1], data.Back[2]) 
                : Vector3.zero;
            
            FootPosition = data.Foot != null && data.Foot.Length >= 3 
                ? new Vector3(data.Foot[0], data.Foot[1], data.Foot[2]) 
                : new Vector3(0, 0, 0.5f);
            
            KneePosition = data.Knee != null && data.Knee.Length >= 3 
                ? new Vector3(data.Knee[0], data.Knee[1], data.Knee[2]) 
                : new Vector3(0, 0.3f, 0.3f);
            
            Angle = data.Angle;
        }

        /// <summary>
        /// Get back position in world space.
        /// </summary>
        public Vector3 GetWorldBackPosition()
        {
            return transform.TransformPoint(BackPosition);
        }

        /// <summary>
        /// Get foot position in world space.
        /// </summary>
        public Vector3 GetWorldFootPosition()
        {
            return transform.TransformPoint(FootPosition);
        }

        /// <summary>
        /// Get knee position in world space.
        /// </summary>
        public Vector3 GetWorldKneePosition()
        {
            return transform.TransformPoint(KneePosition);
        }

        /// <summary>
        /// Attempt to sit a character in this seat.
        /// </summary>
        public bool TrySit(GameObject character)
        {
            if (IsOccupied)
                return false;

            IsOccupied = true;
            OccupyingCharacter = character;
            return true;
        }

        /// <summary>
        /// Stand up from this seat.
        /// </summary>
        public void StandUp()
        {
            IsOccupied = false;
            OccupyingCharacter = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GetWorldBackPosition(), 0.1f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(GetWorldFootPosition(), 0.1f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(GetWorldKneePosition(), 0.1f);
        }
    }
}
#endif
