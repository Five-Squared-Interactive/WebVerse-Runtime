// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

#if NEWTONSOFT_JSON
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiveSQD.StraightFour.Entity;
using FiveSQD.WebVerse.Utilities;
using OMI;
using OMI.Extensions.PhysicsShape;
using UnityEngine;
using Newtonsoft.Json;

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers
{
    /// <summary>
    /// Node-level data for OMI_physics_shape reference on a node.
    /// When a node has this extension, it defines a collider shape for a parent physics body.
    /// </summary>
    [Serializable]
    public class OMIPhysicsShapeNodeRef
    {
        /// <summary>
        /// Index into the document-level shapes array.
        /// </summary>
        [JsonProperty("shape")]
        public int Shape = -1;
    }

    /// <summary>
    /// Node-level handler for OMI_physics_shape.
    /// Creates colliders for nodes that reference physics shapes.
    /// These are typically children of a node with OMI_physics_body.
    /// </summary>
    public class StraightFourPhysicsShapeNodeHandler : StraightFourNodeHandlerBase<OMIPhysicsShapeNodeRef>
    {
        public override string ExtensionName => OMIPhysicsShapeExtension.ExtensionName;
        public override int Priority => 85; // After physics body (90), so body is set up first

        public override Task OnNodeImportAsync(OMIPhysicsShapeNodeRef data, int nodeIndex, GameObject targetObject, OMIImportContext context, CancellationToken cancellationToken = default)
        {
            Logging.Log($"[StraightFour] PhysicsShapeNodeHandler called for node {nodeIndex}: {targetObject?.name}, shapeIndex={data?.Shape}");

            if (data == null || targetObject == null)
            {
                return Task.CompletedTask;
            }

            if (data.Shape < 0)
            {
                Logging.LogWarning($"[StraightFour] Invalid shape index {data.Shape} on node {nodeIndex}");
                return Task.CompletedTask;
            }

            // Get shapes from context
            if (!context.CustomData.TryGetValue(StraightFourCustomDataKeys.PhysicsShapes, out var shapesObj))
            {
                Logging.LogWarning("[StraightFour] No physics shapes found in context for shape node handler");
                return Task.CompletedTask;
            }

            var shapes = shapesObj as List<OMIPhysicsShape>;
            if (shapes == null || data.Shape >= shapes.Count)
            {
                Logging.LogWarning($"[StraightFour] Shape index {data.Shape} out of range (have {shapes?.Count ?? 0} shapes)");
                return Task.CompletedTask;
            }

            var shape = shapes[data.Shape];
            Logging.Log($"[StraightFour] Creating collider for shape {data.Shape} (type={shape.Type}) on {targetObject.name}");

            // Create the collider
            bool isTrigger = false; // Shape nodes are typically colliders, not triggers
            CreateCollider(shape, targetObject, context, isTrigger);

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

            // Create entity for this shape node with correct parent
            var entity = GetOrCreateEntity(context, nodeIndex, targetObject, parentEntity);
            if (entity != null)
            {
                // Shape nodes are usually static colliders (unless parent has dynamic motion)
                entity.SetInteractionState(BaseEntity.InteractionState.Static);
            }

            return Task.CompletedTask;
        }

        private void CreateCollider(OMIPhysicsShape shape, GameObject targetObject, OMIImportContext context, bool isTrigger)
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
                    Logging.Log($"[StraightFour] Created BoxCollider on {targetObject.name} with size {boxCollider.size}");
                    break;

                case OMIPhysicsShapeType.Sphere:
                    var sphereCollider = targetObject.AddComponent<SphereCollider>();
                    if (shape.Sphere != null)
                    {
                        sphereCollider.radius = shape.Sphere.Radius;
                    }
                    collider = sphereCollider;
                    Logging.Log($"[StraightFour] Created SphereCollider on {targetObject.name} with radius {sphereCollider.radius}");
                    break;

                case OMIPhysicsShapeType.Capsule:
                    var capsuleCollider = targetObject.AddComponent<CapsuleCollider>();
                    if (shape.Capsule != null)
                    {
                        capsuleCollider.radius = (shape.Capsule.RadiusTop + shape.Capsule.RadiusBottom) / 2f;
                        capsuleCollider.height = shape.Capsule.Height + capsuleCollider.radius * 2f;
                    }
                    collider = capsuleCollider;
                    Logging.Log($"[StraightFour] Created CapsuleCollider on {targetObject.name}");
                    break;

                case OMIPhysicsShapeType.Cylinder:
                    // Unity doesn't have a native cylinder collider, approximate with capsule
                    var cylCollider = targetObject.AddComponent<CapsuleCollider>();
                    if (shape.Cylinder != null)
                    {
                        // Use average of top and bottom radius
                        cylCollider.radius = (shape.Cylinder.RadiusTop + shape.Cylinder.RadiusBottom) / 2f;
                        cylCollider.height = shape.Cylinder.Height;
                    }
                    collider = cylCollider;
                    Logging.Log($"[StraightFour] Created CapsuleCollider (cylinder approx) on {targetObject.name}");
                    break;

                case OMIPhysicsShapeType.Convex:
                case OMIPhysicsShapeType.Trimesh:
                    Mesh shapeMesh = null;

                    // Get mesh from shape's mesh index reference
                    int meshIndex = shape.Type == OMIPhysicsShapeType.Convex ? shape.Convex?.Mesh ?? -1 : shape.Trimesh?.Mesh ?? -1;
                    if (meshIndex >= 0 && context.GltfImport != null)
                    {
                        shapeMesh = context.GltfImport.GetMesh(meshIndex, 0);
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
                        meshCollider.convex = (shape.Type == OMIPhysicsShapeType.Convex);
                        collider = meshCollider;
                        Logging.Log($"[StraightFour] Created MeshCollider on {targetObject.name}, convex={meshCollider.convex}");
                    }
                    else
                    {
                        Logging.LogWarning($"[StraightFour] No mesh found for {shape.Type} collider on {targetObject.name}");
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
    }
}
#endif
