// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.Vehicle;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_vehicle_wheel.
    /// Creates wheel components for vehicles.
    /// </summary>
    public class StraightFourVehicleWheelHandler : StraightFourNodeHandlerBase<OMIVehicleWheelNode>
    {
        public override string ExtensionName => "OMI_vehicle_wheel";
        public override int Priority => 55;

        public override Task OnNodeImportAsync(OMIVehicleWheelNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null || data.wheel < 0)
            {
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing vehicle wheel for node {nodeIndex}");

            // Get wheel settings from context
            if (!context.CustomData.TryGetValue(StraightFourCustomDataKeys.WheelSettings, out var settingsObj))
            {
                Logging.LogWarning("[StraightFour] No wheel settings found in context");
                return Task.CompletedTask;
            }

            var settings = settingsObj as List<OMIVehicleWheelSettings>;
            if (settings == null || data.wheel >= settings.Count)
            {
                Logging.LogWarning($"[StraightFour] Invalid wheel settings index {data.wheel}");
                return Task.CompletedTask;
            }

            var wheelSettings = settings[data.wheel];

            // Add wheel collider
            var wheelCollider = targetObject.AddComponent<WheelCollider>();
            ConfigureWheelCollider(wheelCollider, wheelSettings);

            // Add wheel behavior component
            var wheelBehavior = targetObject.AddComponent<OMIVehicleWheelBehavior>();
            wheelBehavior.Initialize(wheelSettings, wheelCollider);

            Logging.Log($"[StraightFour] Created vehicle wheel on {targetObject.name}");

            return Task.CompletedTask;
        }

        private void ConfigureWheelCollider(WheelCollider wheel, OMIVehicleWheelSettings settings)
        {
            wheel.radius = settings.radius;
            wheel.suspensionDistance = settings.suspensionTravel;

            // Configure suspension spring
            var spring = wheel.suspensionSpring;
            spring.spring = settings.suspensionStiffness;
            spring.damper = (settings.suspensionDampingCompression + settings.suspensionDampingRebound) / 2f;
            spring.targetPosition = 0.5f;
            wheel.suspensionSpring = spring;

            // Configure friction
            var forwardFriction = wheel.forwardFriction;
            forwardFriction.stiffness = 1f;
            wheel.forwardFriction = forwardFriction;

            var sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.stiffness = 1f;
            wheel.sidewaysFriction = sidewaysFriction;
        }
    }

    /// <summary>
    /// Behavior component for OMI vehicle wheels.
    /// </summary>
    public class OMIVehicleWheelBehavior : MonoBehaviour
    {
        public float Radius { get; private set; }
        public float Width { get; private set; }
        public float MaxForce { get; private set; }
        public float MaxSteeringAngle { get; private set; }
        public float CurrentForceRatio { get; set; }
        public float CurrentSteeringRatio { get; set; }

        private WheelCollider wheelCollider;
        private Transform wheelMesh;

        public void Initialize(OMIVehicleWheelSettings settings, WheelCollider collider)
        {
            Radius = settings.radius;
            Width = settings.width;
            MaxForce = settings.maxForce;
            MaxSteeringAngle = settings.maxSteeringAngle;
            CurrentForceRatio = settings.currentForceRatio;
            CurrentSteeringRatio = settings.currentSteeringRatio;
            wheelCollider = collider;

            // Find visual mesh child
            if (transform.childCount > 0)
            {
                wheelMesh = transform.GetChild(0);
            }
        }

        private void FixedUpdate()
        {
            if (wheelCollider == null)
                return;

            // Apply motor torque
            wheelCollider.motorTorque = MaxForce * CurrentForceRatio;

            // Apply steering
            wheelCollider.steerAngle = MaxSteeringAngle * Mathf.Rad2Deg * CurrentSteeringRatio;

            // Update visual mesh position
            UpdateWheelMeshPosition();
        }

        private void UpdateWheelMeshPosition()
        {
            if (wheelMesh == null || wheelCollider == null)
                return;

            wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheelMesh.position = pos;
            wheelMesh.rotation = rot;
        }

        /// <summary>
        /// Apply brake force to the wheel.
        /// </summary>
        public void ApplyBrake(float brakeTorque)
        {
            if (wheelCollider != null)
            {
                wheelCollider.brakeTorque = brakeTorque;
            }
        }
    }
}
#endif
