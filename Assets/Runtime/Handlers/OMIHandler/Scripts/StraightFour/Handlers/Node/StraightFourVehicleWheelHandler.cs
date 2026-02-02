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
using Newtonsoft.Json.Linq;
#if NWH_VEHICLE_PHYSICS
using NWH.WheelController3D;
#endif

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

            // Configure wheel on parent vehicle entity using adapter pattern (Task 2R.7)
            AttachWheelToVehicleEntity(targetObject, data.wheel, nodeIndex, context);

            Logging.Log($"[StraightFour] Created vehicle wheel on {targetObject.name}");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Attaches wheel to parent vehicle entity by directly manipulating entity properties.
        /// No configuration structs - uses existing entity lists and Unity APIs directly.
        /// </summary>
        private void AttachWheelToVehicleEntity(GameObject wheelObject, int wheelIndex, int nodeIndex, OMIImportContext context)
        {
            // Find parent vehicle entity
            BaseEntity parentEntity = null;
            if (context.CustomData != null && context.CustomData.TryGetValue("SF_NodeParentIndices", out var parentMapObj))
            {
                var parentMap = parentMapObj as Dictionary<int, int>;
                if (parentMap != null && parentMap.TryGetValue(nodeIndex, out var parentNodeIndex))
                {
                    parentEntity = GetEntityForNode(context, parentNodeIndex);
                }
            }

            if (!(parentEntity is AutomobileEntity automobile))
            {
                Logging.LogWarning($"[StraightFourVehicleWheelHandler] Parent entity is not an AutomobileEntity: {parentEntity?.GetType().Name ?? "null"}");
                return;
            }

            // Get the raw glTF JSON to extract wheel configuration
            if (!context.CustomData.TryGetValue("SF_GltfJson", out var jsonObj))
            {
                Logging.LogWarning("[StraightFourVehicleWheelHandler] No glTF JSON found in context");
                return;
            }

            var root = jsonObj as JObject;
            if (root == null)
            {
                return;
            }

            // Get document-level wheel definitions
            var extensions = root["extensions"] as JObject;
            var vehicleExt = extensions?["OMI_vehicle"] as JObject;
            var wheels = vehicleExt?["wheels"] as JArray;

            if (wheels == null || wheelIndex < 0 || wheelIndex >= wheels.Count)
            {
                Logging.LogWarning($"[StraightFourVehicleWheelHandler] Invalid wheel index {wheelIndex}");
                return;
            }

            var omiWheel = wheels[wheelIndex] as JObject;
            if (omiWheel == null)
            {
                return;
            }

            // Initialize wheel lists if needed
            if (automobile.wheels == null)
            {
                automobile.wheels = new List<GameObject>();
            }

#if NWH_VEHICLE_PHYSICS
            if (automobile.wheelControllers == null)
            {
                automobile.wheelControllers = new List<WheelController>();
            }
#endif

            // Add wheel to tracking list
            automobile.wheels.Add(wheelObject);

#if NWH_VEHICLE_PHYSICS
            // Configure WheelController from OMI parameters
            WheelController wc = wheelObject.GetComponent<WheelController>();
            if (wc == null)
            {
                wc = wheelObject.AddComponent<WheelController>();
            }

            // Set properties directly from OMI data
            wc.Radius = omiWheel["radius"]?.Value<float>() ?? 0.35f;
            wc.SpringMaxLength = omiWheel["suspensionTravel"]?.Value<float>() ?? 0.2f;

            // Optional properties
            float springStiffness = omiWheel["springStiffness"]?.Value<float>() ?? 0f;
            if (springStiffness > 0)
            {
                wc.SpringMaxForce = springStiffness;
            }

            float damping = omiWheel["damping"]?.Value<float>() ?? 0f;
            if (damping > 0)
            {
                wc.DamperBumpForce = damping;
                wc.DamperReboundForce = damping;
            }

            // Add to wheel controller list
            automobile.wheelControllers.Add(wc);

            Logging.Log($"[StraightFourVehicleWheelHandler] Attached wheel: {wheelObject.name}, " +
                       $"radius={wc.Radius}m, suspensionTravel={wc.SpringMaxLength}m");
#else
            Logging.Log($"[StraightFourVehicleWheelHandler] Attached wheel: {wheelObject.name} (NWH Vehicle Physics not available)");
#endif
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
