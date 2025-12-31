// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.Vehicle;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_vehicle_body.
    /// Creates vehicle controller components.
    /// </summary>
    public class StraightFourVehicleBodyHandler : StraightFourNodeHandlerBase<OMIVehicleBodyNode>
    {
        public override string ExtensionName => "OMI_vehicle_body";
        public override int Priority => 60;

        public override Task OnNodeImportAsync(OMIVehicleBodyNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null)
            {
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing vehicle body for node {nodeIndex}");

            // Add vehicle body component
            var vehicleBody = targetObject.AddComponent<OMIVehicleBodyBehavior>();
            vehicleBody.Initialize(data);

            // Ensure rigidbody exists
            var rb = targetObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = targetObject.AddComponent<Rigidbody>();
            }

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
            // Create entity for the vehicle with correct parent
            GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);

            Logging.Log($"[StraightFour] Created vehicle body on {targetObject.name}");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Behavior component for OMI vehicle body.
    /// </summary>
    public class OMIVehicleBodyBehavior : MonoBehaviour
    {
        public Vector3 AngularActivation { get; set; }
        public Vector3 LinearActivation { get; set; }
        public Vector3 GyroTorque { get; private set; }
        public float MaxSpeed { get; private set; }
        public int PilotSeatIndex { get; private set; }
        public bool AngularDampeners { get; private set; }
        public bool LinearDampeners { get; private set; }
        public bool UseThrottle { get; private set; }

        private Rigidbody rb;
        private float currentThrottle = 0f;

        public void Initialize(OMIVehicleBodyNode data)
        {
            AngularActivation = data.AngularActivationVector;
            LinearActivation = data.LinearActivationVector;
            GyroTorque = data.GyroTorqueVector;
            MaxSpeed = data.maxSpeed;
            PilotSeatIndex = data.pilotSeat;
            AngularDampeners = data.angularDampeners;
            LinearDampeners = data.linearDampeners;
            UseThrottle = data.useThrottle;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (rb == null)
                return;

            // Apply angular forces (pitch, yaw, roll)
            if (GyroTorque.sqrMagnitude > 0 && AngularActivation.sqrMagnitude > 0)
            {
                Vector3 torque = new Vector3(
                    AngularActivation.x * GyroTorque.x,
                    AngularActivation.y * GyroTorque.y,
                    AngularActivation.z * GyroTorque.z
                );
                rb.AddRelativeTorque(torque, ForceMode.Force);
            }

            // Apply angular dampening
            if (AngularDampeners && AngularActivation.sqrMagnitude < 0.01f)
            {
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * 2f);
            }

            // Apply linear dampening
            if (LinearDampeners && LinearActivation.sqrMagnitude < 0.01f && !UseThrottle)
            {
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 0.5f);
            }

            // Enforce max speed
            if (MaxSpeed > 0 && rb.linearVelocity.magnitude > MaxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * MaxSpeed;
            }
        }

        /// <summary>
        /// Set control inputs for the vehicle.
        /// </summary>
        public void SetInputs(Vector3 linearInput, Vector3 angularInput)
        {
            LinearActivation = linearInput;
            AngularActivation = angularInput;

            if (UseThrottle)
            {
                currentThrottle = Mathf.Clamp(currentThrottle + linearInput.z * Time.deltaTime, 0f, 1f);
            }
        }
    }
}
#endif
