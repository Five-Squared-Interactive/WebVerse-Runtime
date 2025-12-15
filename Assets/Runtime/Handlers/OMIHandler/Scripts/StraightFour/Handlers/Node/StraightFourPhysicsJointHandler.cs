// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.PhysicsJoint;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_physics_joint.
    /// Creates Unity ConfigurableJoint components based on OMI joint settings.
    /// </summary>
    public class StraightFourPhysicsJointHandler : StraightFourNodeHandlerBase<OMIPhysicsJointNode>
    {
        public override string ExtensionName => OMIPhysicsJointExtension.ExtensionName;
        public override int Priority => 70; // After physics bodies

        public override Task OnNodeImportAsync(OMIPhysicsJointNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            if (data == null || targetObject == null)
            {
                return Task.CompletedTask;
            }

            if (data.ConnectedNode < 0 || data.Joint < 0)
            {
                LogVerbose(context, $"[StraightFour] Invalid joint data for node {nodeIndex}");
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing joint for node {nodeIndex} -> connected to node {data.ConnectedNode}");

            // Get joint settings from context
            if (!context.CustomData.TryGetValue(StraightFourCustomDataKeys.JointSettings, out var settingsObj))
            {
                Logging.LogWarning("[StraightFour] No joint settings found in context");
                return Task.CompletedTask;
            }

            var settings = settingsObj as List<OMIPhysicsJointSettings>;
            if (settings == null || data.Joint >= settings.Count)
            {
                Logging.LogWarning($"[StraightFour] Invalid joint settings index {data.Joint}");
                return Task.CompletedTask;
            }

            var jointSettings = settings[data.Joint];

            // Find connected GameObject via node-to-GameObject mapping
            GameObject connectedObject = FindConnectedObject(context, data.ConnectedNode);
            
            // Create ConfigurableJoint
            var joint = targetObject.AddComponent<ConfigurableJoint>();
            
            if (connectedObject != null)
            {
                var connectedRb = connectedObject.GetComponent<Rigidbody>();
                if (connectedRb != null)
                {
                    joint.connectedBody = connectedRb;
                }
            }

            joint.enableCollision = data.EnableCollision;

            // Configure joint from settings
            ConfigureJoint(joint, jointSettings);

            Logging.Log($"[StraightFour] Created joint on {targetObject.name} connected to node {data.ConnectedNode}");

            return Task.CompletedTask;
        }

        private GameObject FindConnectedObject(OMIImportContext context, int nodeIndex)
        {
            // Try to get from node-to-entity map
            if (context.CustomData.TryGetValue(StraightFourCustomDataKeys.NodeToEntity, out var mapObj))
            {
                var map = mapObj as Dictionary<int, FiveSQD.StraightFour.Entity.BaseEntity>;
                if (map != null && map.TryGetValue(nodeIndex, out var entity))
                {
                    return entity.gameObject;
                }
            }

            // Fallback: try NodeToGameObject if available
            if (context.NodeToGameObject != null && context.NodeToGameObject.TryGetValue(nodeIndex, out var go))
            {
                return go;
            }

            return null;
        }

        private void ConfigureJoint(ConfigurableJoint joint, OMIPhysicsJointSettings settings)
        {
            if (settings == null)
                return;

            // Default to locked on all axes
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            // Process limits
            if (settings.Limits != null)
            {
                foreach (var limit in settings.Limits)
                {
                    ProcessLimit(joint, limit);
                }
            }

            // Process drives
            if (settings.Drives != null)
            {
                foreach (var drive in settings.Drives)
                {
                    ProcessDrive(joint, drive);
                }
            }
        }

        private void ProcessLimit(ConfigurableJoint joint, OMIPhysicsJointLimit limit)
        {
            if (limit == null)
                return;

            bool isLimited = limit.Min.HasValue || limit.Max.HasValue;
            bool isFree = !limit.Min.HasValue && !limit.Max.HasValue;
            var motion = isFree ? ConfigurableJointMotion.Free : 
                        (isLimited ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked);

            // Linear axes
            if (limit.LinearAxes != null)
            {
                foreach (int axis in limit.LinearAxes)
                {
                    switch (axis)
                    {
                        case 0: // X
                            joint.xMotion = motion;
                            if (isLimited)
                            {
                                var linearLimit = joint.linearLimit;
                                linearLimit.limit = Mathf.Max(Mathf.Abs(limit.Min ?? 0), Mathf.Abs(limit.Max ?? 0));
                                joint.linearLimit = linearLimit;
                            }
                            break;
                        case 1: // Y
                            joint.yMotion = motion;
                            break;
                        case 2: // Z
                            joint.zMotion = motion;
                            break;
                    }
                }

                // Configure linear spring
                if (limit.Stiffness.HasValue || limit.Damping > 0)
                {
                    var spring = joint.linearLimitSpring;
                    spring.spring = limit.Stiffness ?? 0;
                    spring.damper = limit.Damping;
                    joint.linearLimitSpring = spring;
                }
            }

            // Angular axes
            if (limit.AngularAxes != null)
            {
                foreach (int axis in limit.AngularAxes)
                {
                    switch (axis)
                    {
                        case 0: // X (twist)
                            joint.angularXMotion = motion;
                            if (isLimited)
                            {
                                var lowLimit = joint.lowAngularXLimit;
                                lowLimit.limit = (limit.Min ?? 0) * Mathf.Rad2Deg;
                                joint.lowAngularXLimit = lowLimit;

                                var highLimit = joint.highAngularXLimit;
                                highLimit.limit = (limit.Max ?? 0) * Mathf.Rad2Deg;
                                joint.highAngularXLimit = highLimit;
                            }
                            break;
                        case 1: // Y
                            joint.angularYMotion = motion;
                            if (isLimited)
                            {
                                var yLimit = joint.angularYLimit;
                                yLimit.limit = Mathf.Max(Mathf.Abs(limit.Min ?? 0), Mathf.Abs(limit.Max ?? 0)) * Mathf.Rad2Deg;
                                joint.angularYLimit = yLimit;
                            }
                            break;
                        case 2: // Z
                            joint.angularZMotion = motion;
                            if (isLimited)
                            {
                                var zLimit = joint.angularZLimit;
                                zLimit.limit = Mathf.Max(Mathf.Abs(limit.Min ?? 0), Mathf.Abs(limit.Max ?? 0)) * Mathf.Rad2Deg;
                                joint.angularZLimit = zLimit;
                            }
                            break;
                    }
                }

                // Configure angular spring
                if (limit.Stiffness.HasValue || limit.Damping > 0)
                {
                    var spring = joint.angularXLimitSpring;
                    spring.spring = limit.Stiffness ?? 0;
                    spring.damper = limit.Damping;
                    joint.angularXLimitSpring = spring;
                    joint.angularYZLimitSpring = spring;
                }
            }
        }

        private void ProcessDrive(ConfigurableJoint joint, OMIPhysicsJointDrive drive)
        {
            if (drive == null)
                return;

            var jd = new JointDrive
            {
                positionSpring = drive.Stiffness,
                positionDamper = drive.Damping,
                maximumForce = drive.MaxForce ?? float.MaxValue
            };

            // New API: Type + Axis instead of LinearAxes/AngularAxes arrays
            bool isLinear = drive.Type == OMIJointDriveType.Linear;
            int axis = drive.Axis;

            if (isLinear)
            {
                // Linear drive on single axis
                switch (axis)
                {
                    case 0:
                        joint.xDrive = jd;
                        if (drive.VelocityTarget.HasValue)
                        {
                            var vel = joint.targetVelocity;
                            vel.x = drive.VelocityTarget.Value;
                            joint.targetVelocity = vel;
                        }
                        if (drive.PositionTarget.HasValue)
                        {
                            var pos = joint.targetPosition;
                            pos.x = drive.PositionTarget.Value;
                            joint.targetPosition = pos;
                        }
                        break;
                    case 1:
                        joint.yDrive = jd;
                        if (drive.VelocityTarget.HasValue)
                        {
                            var vel = joint.targetVelocity;
                            vel.y = drive.VelocityTarget.Value;
                            joint.targetVelocity = vel;
                        }
                        if (drive.PositionTarget.HasValue)
                        {
                            var pos = joint.targetPosition;
                            pos.y = drive.PositionTarget.Value;
                            joint.targetPosition = pos;
                        }
                        break;
                    case 2:
                        joint.zDrive = jd;
                        if (drive.VelocityTarget.HasValue)
                        {
                            var vel = joint.targetVelocity;
                            vel.z = drive.VelocityTarget.Value;
                            joint.targetVelocity = vel;
                        }
                        if (drive.PositionTarget.HasValue)
                        {
                            var pos = joint.targetPosition;
                            pos.z = drive.PositionTarget.Value;
                            joint.targetPosition = pos;
                        }
                        break;
                }
            }
            else if (drive.Type == OMIJointDriveType.Angular)
            {
                // Angular drive on single axis
                switch (axis)
                {
                    case 0:
                        joint.angularXDrive = jd;
                        if (drive.VelocityTarget.HasValue)
                        {
                            var vel = joint.targetAngularVelocity;
                            vel.x = drive.VelocityTarget.Value;
                            joint.targetAngularVelocity = vel;
                        }
                        break;
                    case 1:
                    case 2:
                        joint.angularYZDrive = jd;
                        if (drive.VelocityTarget.HasValue)
                        {
                            var vel = joint.targetAngularVelocity;
                            if (axis == 1) vel.y = drive.VelocityTarget.Value;
                            else vel.z = drive.VelocityTarget.Value;
                            joint.targetAngularVelocity = vel;
                        }
                        break;
                }

                // Set target rotation if position target is specified
                if (drive.PositionTarget.HasValue)
                {
                    var angles = joint.targetRotation.eulerAngles;
                    switch (axis)
                    {
                        case 0: angles.x = drive.PositionTarget.Value * Mathf.Rad2Deg; break;
                        case 1: angles.y = drive.PositionTarget.Value * Mathf.Rad2Deg; break;
                        case 2: angles.z = drive.PositionTarget.Value * Mathf.Rad2Deg; break;
                    }
                    joint.targetRotation = Quaternion.Euler(angles);
                }
            }
        }
    }
}
#endif
