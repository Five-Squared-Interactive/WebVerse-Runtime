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
using Newtonsoft.Json.Linq;

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
            BaseEntity entity = GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);

            // Add seat to entity using adapter pattern (Task 2R.7)
            if (entity != null)
            {
                AddSeatToEntity(entity, targetObject.transform, nodeIndex, context);
            }

            Logging.Log($"[StraightFour] Created seat at {targetObject.name}");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds seat to entity by creating SeatData directly from OMI JSON.
        /// No adapter - creates data struct directly.
        /// </summary>
        private void AddSeatToEntity(BaseEntity entity, Transform seatTransform, int nodeIndex, OMIImportContext context)
        {
            // Get the raw glTF JSON to extract OMI_seat extension data
            if (!context.CustomData.TryGetValue("SF_GltfJson", out var jsonObj))
            {
                Logging.LogWarning("[StraightFourSeatHandler] No glTF JSON found in context");
                return;
            }

            var root = jsonObj as JObject;
            if (root == null)
            {
                return;
            }

            // Navigate to the node's OMI_seat extension
            var nodes = root["nodes"] as JArray;
            if (nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Count)
            {
                return;
            }

            var node = nodes[nodeIndex] as JObject;
            var extensions = node?["extensions"] as JObject;
            var omiSeat = extensions?["OMI_seat"] as JObject;

            if (omiSeat == null)
            {
                Logging.LogWarning($"[StraightFourSeatHandler] No OMI_seat extension found for node {nodeIndex}");
                return;
            }

            // Create SeatData struct directly from OMI JSON
            var seatData = SeatData.Create(seatTransform.position, seatTransform.rotation, SeatType.Standard);

            // OMI field: back → SeatData property: backDirection
            var backArray = omiSeat["back"] as JArray;
            if (backArray != null && backArray.Count >= 3)
            {
                seatData.backDirection = new Vector3(
                    backArray[0]?.Value<float>() ?? 0f,
                    backArray[1]?.Value<float>() ?? 0f,
                    backArray[2]?.Value<float>() ?? -1f
                );
            }

            // OMI field: foot → SeatData property: footOffset
            var footArray = omiSeat["foot"] as JArray;
            if (footArray != null && footArray.Count >= 3)
            {
                seatData.footOffset = new Vector3(
                    footArray[0]?.Value<float>() ?? 0f,
                    footArray[1]?.Value<float>() ?? -0.5f,
                    footArray[2]?.Value<float>() ?? 0f
                );
            }

            // OMI field: knee → SeatData property: kneeOffset
            var kneeArray = omiSeat["knee"] as JArray;
            if (kneeArray != null && kneeArray.Count >= 3)
            {
                seatData.kneeOffset = new Vector3(
                    kneeArray[0]?.Value<float>() ?? 0f,
                    kneeArray[1]?.Value<float>() ?? -0.25f,
                    kneeArray[2]?.Value<float>() ?? 0f
                );
            }

            // OMI field: angle → SeatData property: seatAngle (convert radians to degrees)
            float angleRadians = omiSeat["angle"]?.Value<float>() ?? 1.5707963267948966f; // Default: π/2 (90 degrees)
            seatData.seatAngle = angleRadians * Mathf.Rad2Deg;

            // Add seat to entity using BaseEntity API
            entity.AddSeat(seatData);
            Logging.Log($"[StraightFourSeatHandler] Added seat to entity: seatAngle={seatData.seatAngle}°");
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
