// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.Vehicle;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_vehicle_thruster.
    /// Creates thruster components for vehicle propulsion.
    /// </summary>
    public class StraightFourVehicleThrusterHandler : StraightFourNodeHandlerBase<OMIVehicleThrusterNode>
    {
        public override string ExtensionName => "OMI_vehicle_thruster";
        public override int Priority => 55;

        public override Task OnNodeImportAsync(OMIVehicleThrusterNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            Logging.Log($"[StraightFour] VehicleThrusterHandler.OnNodeImportAsync called for node {nodeIndex}, target={targetObject?.name}");
            
            if (data == null || targetObject == null)
            {
                Logging.Log($"[StraightFour] VehicleThrusterHandler: data or targetObject is null");
                return Task.CompletedTask;
            }

            Logging.Log($"[StraightFour] Processing vehicle thruster for node {nodeIndex}, thruster index={data.thruster}");

            // Get thruster settings from document-level data
            OMIVehicleThrusterSettings thrusterSettings = null;
            bool hasThrustersData = context.CustomData.TryGetValue("SF_VehicleThrusters", out var thrustersObj);
            Logging.Log($"[StraightFour] Has SF_VehicleThrusters in context: {hasThrustersData}");
            
            if (data.thruster >= 0 && hasThrustersData)
            {
                var thrusters = thrustersObj as List<OMIVehicleThrusterSettings>;
                if (thrusters != null && data.thruster < thrusters.Count)
                {
                    thrusterSettings = thrusters[data.thruster];
                }
            }

            if (thrusterSettings == null)
            {
                Logging.LogWarning($"[StraightFour] Invalid thruster index {data.thruster} on node {nodeIndex}");
                return Task.CompletedTask;
            }

            // Add thruster component
            var thruster = targetObject.AddComponent<OMIVehicleThrusterBehavior>();
            thruster.Initialize(thrusterSettings);

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
            // Create entity for the thruster with correct parent
            GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);

            Logging.Log($"[StraightFour] Created vehicle thruster on {targetObject.name} (maxForce={thrusterSettings.maxForce}N)");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Behavior component for OMI vehicle thruster.
    /// </summary>
    public class OMIVehicleThrusterBehavior : MonoBehaviour
    {
        /// <summary>
        /// Maximum thrust force in Newtons.
        /// </summary>
        public float MaxForce { get; private set; }

        /// <summary>
        /// Current force ratio (0 to 1).
        /// </summary>
        public float CurrentForceRatio { get; set; }

        /// <summary>
        /// Maximum gimbal angle in radians.
        /// </summary>
        public float MaxGimbal { get; private set; }

        /// <summary>
        /// Current gimbal ratio (XY).
        /// </summary>
        public Vector2 CurrentGimbalRatio { get; set; }

        private Rigidbody parentRigidbody;

        public void Initialize(OMIVehicleThrusterSettings settings)
        {
            MaxForce = settings.maxForce;
            CurrentForceRatio = settings.currentForceRatio;
            MaxGimbal = settings.maxGimbal;
            CurrentGimbalRatio = settings.CurrentGimbalRatioVector;
        }

        private void Awake()
        {
            // Find parent rigidbody for applying forces
            parentRigidbody = GetComponentInParent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (parentRigidbody == null || CurrentForceRatio <= 0)
                return;

            // Calculate thrust direction (local forward, adjusted by gimbal)
            Vector3 thrustDirection = transform.forward;
            if (MaxGimbal > 0 && CurrentGimbalRatio.sqrMagnitude > 0)
            {
                // Apply gimbal rotation
                float gimbalX = CurrentGimbalRatio.x * MaxGimbal * Mathf.Rad2Deg;
                float gimbalY = CurrentGimbalRatio.y * MaxGimbal * Mathf.Rad2Deg;
                Quaternion gimbalRotation = Quaternion.Euler(gimbalX, gimbalY, 0);
                thrustDirection = gimbalRotation * transform.forward;
            }

            // Apply thrust force
            float force = MaxForce * CurrentForceRatio;
            parentRigidbody.AddForceAtPosition(thrustDirection * force, transform.position, ForceMode.Force);
        }

        /// <summary>
        /// Set the thruster activation (0 to 1).
        /// </summary>
        public void SetThrust(float ratio)
        {
            CurrentForceRatio = Mathf.Clamp01(ratio);
        }

        /// <summary>
        /// Set the gimbal direction (-1 to 1 for each axis).
        /// </summary>
        public void SetGimbal(Vector2 ratio)
        {
            CurrentGimbalRatio = new Vector2(
                Mathf.Clamp(ratio.x, -1f, 1f),
                Mathf.Clamp(ratio.y, -1f, 1f)
            );
        }
    }
}
#endif
