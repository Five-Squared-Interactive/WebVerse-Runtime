// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour;
using OMI;
using OMI.Extensions.PhysicsBody;
using OMI.Extensions.PhysicsShape;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level handler for OMI_physics_body.
    /// Creates StraightFour entities with physics properties based on the extension data.
    /// </summary>
    public class StraightFourPhysicsBodyHandler : StraightFourNodeHandlerBase<OMIPhysicsBodyNode>
    {
        public override string ExtensionName => OMIPhysicsBodyExtension.ExtensionName;
        public override int Priority => 90; // High priority so colliders are ready for joints

        public override Task OnNodeImportAsync(OMIPhysicsBodyNode data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            Logging.Log($"[StraightFour] PhysicsBodyHandler called for node {nodeIndex}: {targetObject?.name}, hasMotion={data?.Motion != null}, hasCollider={data?.Collider != null}, hasTrigger={data?.Trigger != null}");
            
            if (data == null || targetObject == null)
            {
                return Task.CompletedTask;
            }

            LogVerbose(context, $"[StraightFour] Processing physics body for node {nodeIndex}: {targetObject.name}");

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
            // Get or create entity with correct parent
            var entity = GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);
            if (entity == null)
            {
                Logging.LogWarning($"[StraightFour] Failed to create entity for node {nodeIndex}");
                return Task.CompletedTask;
            }

            // Determine interaction state from motion type
            var interactionState = BaseEntity.InteractionState.Static;
            bool addRigidbody = false;

            if (data.Motion != null)
            {
                switch (data.Motion.Type)
                {
                    case OMIPhysicsMotionType.Dynamic:
                        interactionState = BaseEntity.InteractionState.Physical;
                        addRigidbody = true;
                        break;
                    case OMIPhysicsMotionType.Kinematic:
                        interactionState = BaseEntity.InteractionState.Physical;
                        addRigidbody = true;
                        break;
                    case OMIPhysicsMotionType.Static:
                    default:
                        interactionState = BaseEntity.InteractionState.Static;
                        break;
                }
            }

            // Set entity interaction state
            entity.SetInteractionState(interactionState);

            // Process collider
            if (data.Collider != null)
            {
                Logging.Log($"[StraightFour] Processing collider for node {nodeIndex}, shape index={data.Collider.Shape}");
                ProcessCollider(data.Collider, targetObject, context, false);
            }
            else
            {
                Logging.Log($"[StraightFour] No collider data for node {nodeIndex}. Data type: {data.GetType().FullName}, Motion={data.Motion != null}, Trigger={data.Trigger != null}");
                
                // Debug: Try to see what's actually in the object
                try
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    Logging.Log($"[StraightFour] Serialized data object: {json}");
                }
                catch (System.Exception ex)
                {
                    Logging.Log($"[StraightFour] Failed to serialize data: {ex.Message}");
                }
            }

            // Process trigger
            if (data.Trigger != null)
            {
                Logging.Log($"[StraightFour] Processing trigger for node {nodeIndex}");
                ProcessCollider(data.Trigger.Shape, targetObject, context, true);
            }

            // Add and configure Rigidbody if needed
            if (addRigidbody)
            {
                ConfigureRigidbody(data.Motion, targetObject, context);
            }

            // Set StraightFour physics properties
            if (data.Motion != null)
            {
                var physProps = new BaseEntity.EntityPhysicalProperties
                {
                    mass = data.Motion.Mass,
                    gravitational = data.Motion.GravityFactor > 0f, // Convert float to bool (non-zero = gravitational)
                    centerOfMass = ToVector3(data.Motion.CenterOfMass, Vector3.zero),
                    drag = 0f, // OMI doesn't specify drag directly
                    angularDrag = 0.05f
                };
                entity.SetPhysicalProperties(physProps);

                // Set initial motion if velocities specified
                if (data.Motion.LinearVelocity != null || data.Motion.AngularVelocity != null)
                {
                    var motion = new BaseEntity.EntityMotion
                    {
                        stationary = false,
                        angularVelocity = ToVector3(data.Motion.AngularVelocity, Vector3.zero),
                        velocity = ToVector3(data.Motion.LinearVelocity, Vector3.zero)
                    };
                    entity.SetMotion(motion);
                }
            }

            LogVerbose(context, $"[StraightFour] Created physics entity for {targetObject.name}, state={interactionState}");

            return Task.CompletedTask;
        }

        private void ProcessCollider(OMIPhysicsBodyCollider colliderData, GameObject targetObject, OMIImportContext context, bool isTrigger)
        {
            ProcessCollider(colliderData.Shape, targetObject, context, isTrigger);
        }

        private void ProcessCollider(int shapeIndex, GameObject targetObject, OMIImportContext context, bool isTrigger)
        {
            Logging.Log($"[StraightFour] ProcessCollider: shapeIndex={shapeIndex}, target={targetObject.name}, isTrigger={isTrigger}");
            
            if (shapeIndex < 0)
            {
                Logging.Log($"[StraightFour] ProcessCollider: Invalid shape index {shapeIndex}");
                return;
            }

            // Get shapes from context
            if (!context.CustomData.TryGetValue(StraightFourCustomDataKeys.PhysicsShapes, out var shapesObj))
            {
                Logging.LogWarning("[StraightFour] No physics shapes found in context (SF_PhysicsShapes key missing)");
                return;
            }

            var shapes = shapesObj as List<OMIPhysicsShape>;
            if (shapes == null || shapeIndex >= shapes.Count)
            {
                Logging.LogWarning($"[StraightFour] Invalid shape index {shapeIndex}");
                return;
            }

            var shape = shapes[shapeIndex];
            CreateCollider(shape, targetObject, isTrigger, context);
        }

        private void CreateCollider(OMIPhysicsShape shape, GameObject targetObject, bool isTrigger, OMIImportContext importContext)
        {
            Collider collider = null;

            switch (shape.Type)
            {
                case OMIPhysicsShapeType.Box:
                    var boxCollider = targetObject.AddComponent<BoxCollider>();
                    if (shape.Box?.Size != null && shape.Box.Size.Length >= 3)
                    {
                        boxCollider.size = new Vector3(shape.Box.Size[0], shape.Box.Size[1], shape.Box.Size[2]);
                    }
                    collider = boxCollider;
                    break;

                case OMIPhysicsShapeType.Sphere:
                    var sphereCollider = targetObject.AddComponent<SphereCollider>();
                    if (shape.Sphere != null)
                    {
                        sphereCollider.radius = shape.Sphere.Radius;
                    }
                    collider = sphereCollider;
                    break;

                case OMIPhysicsShapeType.Capsule:
                    var capsuleCollider = targetObject.AddComponent<CapsuleCollider>();
                    if (shape.Capsule != null)
                    {
                        // Use average of top and bottom radius (Unity capsules have uniform radius)
                        capsuleCollider.radius = (shape.Capsule.RadiusTop + shape.Capsule.RadiusBottom) / 2f;
                        capsuleCollider.height = shape.Capsule.Height + capsuleCollider.radius * 2f; // Unity height includes caps
                    }
                    collider = capsuleCollider;
                    break;

                case OMIPhysicsShapeType.Cylinder:
                    // Unity doesn't have a native cylinder collider, use capsule as approximation
                    var cylinderCollider = targetObject.AddComponent<CapsuleCollider>();
                    if (shape.Cylinder != null)
                    {
                        // Use average of top and bottom radius
                        cylinderCollider.radius = (shape.Cylinder.RadiusTop + shape.Cylinder.RadiusBottom) / 2f;
                        cylinderCollider.height = shape.Cylinder.Height;
                    }
                    Logging.LogWarning("[StraightFour] Cylinder shape approximated with capsule collider");
                    collider = cylinderCollider;
                    break;

                case OMIPhysicsShapeType.Convex:
                case OMIPhysicsShapeType.Trimesh:
                    // For convex and trimesh, get mesh from the shape's mesh reference
                    Mesh shapeMesh = null;
                    
                    // First try to get mesh from shape's mesh index
                    int meshIndex = shape.Type == OMIPhysicsShapeType.Convex ? shape.Convex?.Mesh ?? -1 : shape.Trimesh?.Mesh ?? -1;
                    if (meshIndex >= 0 && importContext.GltfImport != null)
                    {
                        // Get mesh from glTF import (meshIndex, primitiveIndex)
                        shapeMesh = importContext.GltfImport.GetMesh(meshIndex, 0);
                        Logging.Log($"[StraightFour] Got mesh {meshIndex} from glTF for {shape.Type} collider: {(shapeMesh != null ? shapeMesh.name : "null")}");
                    }
                    
                    // Fallback to mesh on target object
                    if (shapeMesh == null)
                    {
                        var meshFilter = targetObject.GetComponent<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                        {
                            shapeMesh = meshFilter.sharedMesh;
                            Logging.Log($"[StraightFour] Using mesh from target object for {shape.Type} collider: {shapeMesh.name}");
                        }
                    }
                    
                    if (shapeMesh != null)
                    {
                        var meshCollider = targetObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = shapeMesh;
                        meshCollider.convex = shape.Type == OMIPhysicsShapeType.Convex;
                        collider = meshCollider;
                        Logging.Log($"[StraightFour] Created {shape.Type} MeshCollider on {targetObject.name}");
                    }
                    else
                    {
                        Logging.LogWarning($"[StraightFour] No mesh found for {shape.Type} collider on {targetObject.name} (meshIndex={meshIndex})");
                    }
                    break;

                default:
                    Logging.LogWarning($"[StraightFour] Unknown shape type: {shape.Type}");
                    break;
            }

            if (collider != null)
            {
                collider.isTrigger = isTrigger;
            }
        }

        private void ConfigureRigidbody(OMIPhysicsBodyMotion motion, GameObject targetObject, OMIImportContext context)
        {
            var rb = targetObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = targetObject.AddComponent<Rigidbody>();
            }

            rb.mass = motion.Mass;
            rb.isKinematic = motion.Type == OMIPhysicsMotionType.Kinematic;
            rb.useGravity = motion.GravityFactor > 0f;

            if (motion.CenterOfMass != null && motion.CenterOfMass.Length >= 3)
            {
                rb.centerOfMass = new Vector3(motion.CenterOfMass[0], motion.CenterOfMass[1], motion.CenterOfMass[2]);
            }

            if (motion.InertiaDiagonal != null && motion.InertiaDiagonal.Length >= 3)
            {
                rb.inertiaTensor = new Vector3(motion.InertiaDiagonal[0], motion.InertiaDiagonal[1], motion.InertiaDiagonal[2]);
            }

            if (motion.InertiaOrientation != null && motion.InertiaOrientation.Length >= 4)
            {
                rb.inertiaTensorRotation = new Quaternion(
                    motion.InertiaOrientation[0],
                    motion.InertiaOrientation[1],
                    motion.InertiaOrientation[2],
                    motion.InertiaOrientation[3]);
            }

            if (motion.LinearVelocity != null && motion.LinearVelocity.Length >= 3)
            {
                rb.linearVelocity = new Vector3(motion.LinearVelocity[0], motion.LinearVelocity[1], motion.LinearVelocity[2]);
            }

            if (motion.AngularVelocity != null && motion.AngularVelocity.Length >= 3)
            {
                rb.angularVelocity = new Vector3(motion.AngularVelocity[0], motion.AngularVelocity[1], motion.AngularVelocity[2]);
            }

            // Apply gravity factor via custom gravity handler if needed
            if (motion.GravityFactor != 1f && motion.GravityFactor > 0f)
            {
                // Store gravity factor for custom gravity behavior
                var gravityComponent = targetObject.AddComponent<OMIGravityModifier>();
                gravityComponent.gravityFactor = motion.GravityFactor;
            }

            // Add marker to indicate physics body handler configured this Rigidbody
            // This prevents conflicts with other handlers (e.g., OMI_vehicle_body)
            if (targetObject.GetComponent<OMIPhysicsBodyConfigured>() == null)
            {
                targetObject.AddComponent<OMIPhysicsBodyConfigured>();
            }
        }
    }

    /// <summary>
    /// Marker component indicating that OMI_physics_body handler configured this Rigidbody.
    /// Used to prevent configuration conflicts with other handlers like OMI_vehicle_body.
    /// </summary>
    public class OMIPhysicsBodyConfigured : MonoBehaviour
    {
        // Marker component - no implementation needed
    }

    /// <summary>
    /// Component that applies custom gravity factor to a Rigidbody.
    /// </summary>
    public class OMIGravityModifier : MonoBehaviour
    {
        public float gravityFactor = 1f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
            }
        }

        private void FixedUpdate()
        {
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(Physics.gravity * gravityFactor, ForceMode.Acceleration);
            }
        }
    }
}
#endif
