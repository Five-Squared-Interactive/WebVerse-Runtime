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
using Newtonsoft.Json.Linq;

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
            BaseEntity entity = GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);

            // Configure vehicle physics using adapter pattern (Task 2R.7)
            if (entity != null)
            {
                ConfigureVehiclePhysicsFromOMI(entity, nodeIndex, context);
            }

            Logging.Log($"[StraightFour] Created vehicle body on {targetObject.name}");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Configures vehicle entity by setting Rigidbody properties directly from OMI data.
        /// No configuration structs or methods - uses existing entity/Unity APIs directly.
        /// </summary>
        private void ConfigureVehiclePhysicsFromOMI(BaseEntity entity, int nodeIndex, OMIImportContext context)
        {
            // Get the raw glTF JSON to extract OMI extension data
            if (!context.CustomData.TryGetValue("SF_GltfJson", out var jsonObj))
            {
                Logging.LogWarning("[StraightFourVehicleBodyHandler] No glTF JSON found in context");
                return;
            }

            var root = jsonObj as JObject;
            if (root == null)
            {
                return;
            }

            // Navigate to the node's OMI_vehicle_body extension
            var nodes = root["nodes"] as JArray;
            if (nodes == null || nodeIndex < 0 || nodeIndex >= nodes.Count)
            {
                return;
            }

            var node = nodes[nodeIndex] as JObject;
            var extensions = node?["extensions"] as JObject;
            var omiVehicleBody = extensions?["OMI_vehicle_body"] as JObject;

            if (omiVehicleBody == null)
            {
                Logging.LogWarning($"[StraightFourVehicleBodyHandler] No OMI_vehicle_body extension found for node {nodeIndex}");
                return;
            }

            // Set properties directly based on entity type
            if (entity is AutomobileEntity automobile)
            {
                ConfigureAutomobile(automobile, omiVehicleBody);
            }
            else if (entity is AirplaneEntity airplane)
            {
                ConfigureAirplane(airplane, omiVehicleBody);
            }
            else
            {
                Logging.LogWarning($"[StraightFourVehicleBodyHandler] Entity is not a vehicle type: {entity.GetType().Name}");
            }
        }

        /// <summary>
        /// Configure automobile by setting Rigidbody properties directly.
        /// </summary>
        private void ConfigureAutomobile(AutomobileEntity automobile, JObject omiVehicleBody)
        {
            var rbody = automobile.rbody;
            if (rbody == null)
            {
                Logging.LogWarning("[StraightFourVehicleBodyHandler] Automobile has no Rigidbody");
                return;
            }

            // Check if OMI_physics_body already configured this Rigidbody
            var physicsBodyConfigured = automobile.gameObject.GetComponent<OMIPhysicsBodyConfigured>();
            if (physicsBodyConfigured != null)
            {
                Logging.LogWarning("[StraightFourVehicleBodyHandler] Node has both OMI_physics_body and OMI_vehicle_body. " +
                                   "Physics body configuration takes precedence for Rigidbody properties (mass, centerOfMass, etc.). " +
                                   "Vehicle handler will only configure damping properties.");
            }

            // OMI field: linearDampeners → Unity property: linearDamping
            bool linearDampeners = omiVehicleBody["linearDampeners"]?.Value<bool>() ?? true;
            if (linearDampeners)
            {
                rbody.linearDamping = 0.5f;
            }

            // OMI field: angularDampeners → Unity property: angularDamping
            bool angularDampeners = omiVehicleBody["angularDampeners"]?.Value<bool>() ?? true;
            if (angularDampeners)
            {
                rbody.angularDamping = 0.5f;
            }

            // OMI field: centerOfMass → Unity property: centerOfMass
            // Only set if physics body handler hasn't already configured it
            if (physicsBodyConfigured == null)
            {
                var centerOfMassArray = omiVehicleBody["centerOfMass"] as JArray;
                if (centerOfMassArray != null && centerOfMassArray.Count >= 3)
                {
                    Vector3 centerOfMass = new Vector3(
                        centerOfMassArray[0]?.Value<float>() ?? 0f,
                        centerOfMassArray[1]?.Value<float>() ?? 0f,
                        centerOfMassArray[2]?.Value<float>() ?? 0f
                    );
                    if (centerOfMass != Vector3.zero)
                    {
                        rbody.centerOfMass = centerOfMass;
                    }
                }
            }

            float maxSpeed = omiVehicleBody["maxSpeed"]?.Value<float>() ?? 50f;
            bool useThrottle = omiVehicleBody["useThrottle"]?.Value<bool>() ?? true;

            Logging.Log($"[StraightFourVehicleBodyHandler] Configured automobile: " +
                       $"maxSpeed={maxSpeed}m/s, useThrottle={useThrottle}, " +
                       $"linearDamping={rbody.linearDamping}, angularDamping={rbody.angularDamping}");
        }

        /// <summary>
        /// Configure airplane by setting Rigidbody and throttle properties directly.
        /// </summary>
        private void ConfigureAirplane(AirplaneEntity airplane, JObject omiVehicleBody)
        {
            var rbody = airplane.rbody;
            if (rbody == null)
            {
                Logging.LogWarning("[StraightFourVehicleBodyHandler] Airplane has no Rigidbody");
                return;
            }

            // Check if OMI_physics_body already configured this Rigidbody
            var physicsBodyConfigured = airplane.gameObject.GetComponent<OMIPhysicsBodyConfigured>();
            if (physicsBodyConfigured != null)
            {
                Logging.LogWarning("[StraightFourVehicleBodyHandler] Node has both OMI_physics_body and OMI_vehicle_body. " +
                                   "Physics body configuration takes precedence for Rigidbody properties (mass, centerOfMass, etc.). " +
                                   "Vehicle handler will only configure damping properties.");
            }

            // OMI field: linearDampeners → Unity property: linearDamping
            bool linearDampeners = omiVehicleBody["linearDampeners"]?.Value<bool>() ?? true;
            if (linearDampeners)
            {
                rbody.linearDamping = Mathf.Max(rbody.linearDamping, 1f);
            }

            // OMI field: angularDampeners → Unity property: angularDamping
            bool angularDampeners = omiVehicleBody["angularDampeners"]?.Value<bool>() ?? true;
            if (angularDampeners)
            {
                rbody.angularDamping = Mathf.Max(rbody.angularDamping, 2f);
            }

            // OMI field: centerOfMass → Unity property: centerOfMass
            // Only set if physics body handler hasn't already configured it
            if (physicsBodyConfigured == null)
            {
                var centerOfMassArray = omiVehicleBody["centerOfMass"] as JArray;
                if (centerOfMassArray != null && centerOfMassArray.Count >= 3)
                {
                    Vector3 centerOfMass = new Vector3(
                        centerOfMassArray[0]?.Value<float>() ?? 0f,
                        centerOfMassArray[1]?.Value<float>() ?? 0f,
                        centerOfMassArray[2]?.Value<float>() ?? 0f
                    );
                    if (centerOfMass != Vector3.zero)
                    {
                        rbody.centerOfMass = centerOfMass;
                    }
                }
            }

            // OMI field: useThrottle → AirplaneEntity property: throttle
            bool useThrottle = omiVehicleBody["useThrottle"]?.Value<bool>() ?? true;
            airplane.throttle = useThrottle ? 0f : 1f;

            float maxSpeed = omiVehicleBody["maxSpeed"]?.Value<float>() ?? 50f;

            Logging.Log($"[StraightFourVehicleBodyHandler] Configured airplane: " +
                       $"maxSpeed={maxSpeed}m/s, throttle={airplane.throttle}, " +
                       $"linearDamping={rbody.linearDamping}, angularDamping={rbody.angularDamping}");
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
