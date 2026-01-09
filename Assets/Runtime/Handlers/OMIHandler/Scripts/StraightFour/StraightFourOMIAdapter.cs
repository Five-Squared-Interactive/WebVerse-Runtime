// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FiveSQD.StraightFour.Entity;
using OMI;
using OMI.Extensions.PhysicsBody;
#if NEWTONSOFT_JSON
using Newtonsoft.Json.Linq;
using FiveSQD.WebVerse.Handlers.OMI.StraightFour.Handlers;
#endif

namespace FiveSQD.WebVerse.Handlers.OMI.StraightFour
{
    /// <summary>
    /// Adapter for integrating OMI-loaded content with StraightFour entities.
    /// This adapter parses OMI extension JSON directly and creates StraightFour entities
    /// with appropriate physics properties.
    /// </summary>
    public class StraightFourOMIAdapter : MonoBehaviour
    {
        private WebVerseRuntime runtime;
        private SpawnPointRegistry spawnPointRegistry;
        private Dictionary<int, List<ShapeData>> nodeShapes = new Dictionary<int, List<ShapeData>>();
        private Dictionary<int, PhysicsBodyData> nodePhysicsBodies = new Dictionary<int, PhysicsBodyData>();
        private Dictionary<int, SeatData> nodeSeats = new Dictionary<int, SeatData>();
        private Dictionary<int, LinkData> nodeLinks = new Dictionary<int, LinkData>();
        private Dictionary<int, SpawnPointNodeData> nodeSpawnPoints = new Dictionary<int, SpawnPointNodeData>();
        private Dictionary<string, int> nodeNameToIndex = new Dictionary<string, int>();

        /// <summary>
        /// Structure to hold parsed shape data.
        /// </summary>
        private class ShapeData
        {
            public string type;
            public Vector3 size;
            public float radius;
            public float height;
            public int meshIndex = -1;
        }

        /// <summary>
        /// Parsed physics body data.
        /// </summary>
        private class PhysicsBodyData
        {
            public string motionType;
            public float mass;
            public Vector3 centerOfMass;
            public Vector3 linearVelocity;
            public Vector3 angularVelocity;
        }

        /// <summary>
        /// Parsed seat data.
        /// </summary>
        private class SeatData
        {
            public Vector3 back;
            public Vector3 foot;
            public Vector3 knee;
            public float angle;
        }

        /// <summary>
        /// Parsed link data.
        /// </summary>
        private class LinkData
        {
            public string uri;
            public string title;
        }

        /// <summary>
        /// Parsed spawn point data.
        /// </summary>
        private class SpawnPointNodeData
        {
            public string title;
            public string team;
            public string group;
        }

        /// <summary>
        /// Initialize the adapter.
        /// </summary>
        public void Initialize(WebVerseRuntime runtime, SpawnPointRegistry registry)
        {
            this.runtime = runtime;
            this.spawnPointRegistry = registry;
            Logging.Log("[StraightFourOMIAdapter] Initialized.");
        }

        /// <summary>
        /// Creates an OMIExtensionManager with all StraightFour handlers registered.
        /// Call this before loading with OMINewtonsoftLoader.
        /// </summary>
        public OMIExtensionManager CreateExtensionManager()
        {
            var manager = new OMIExtensionManager();
            
#if NEWTONSOFT_JSON
            // Register document-level handlers (process glTF extension roots)
            manager.RegisterHandler(new StraightFourPhysicsShapeHandler());
            manager.RegisterHandler(new StraightFourPhysicsBodyDocumentHandler());
            manager.RegisterHandler(new StraightFourPhysicsGravityDocumentHandler());
            manager.RegisterHandler(new StraightFourPhysicsJointDocumentHandler());
            manager.RegisterHandler(new StraightFourEnvironmentSkyDocumentHandler());
            manager.RegisterHandler(new StraightFourAudioDocumentHandler());
            manager.RegisterHandler(new StraightFourVehicleWheelDocumentHandler());
            var thrusterDocHandler = new StraightFourVehicleThrusterDocumentHandler();
            Logging.Log($"[StraightFourOMIAdapter] Registering thruster doc handler, ExtensionName={thrusterDocHandler.ExtensionName}");
            manager.RegisterHandler(thrusterDocHandler);
            
            // Register node-level handlers (process per-node extension data)
            manager.RegisterHandler(new StraightFourPhysicsBodyHandler());
            manager.RegisterHandler(new StraightFourPhysicsShapeNodeHandler()); // Handle shape refs on nodes
            manager.RegisterHandler(new StraightFourSpawnPointHandler());
            manager.RegisterHandler(new StraightFourSeatHandler());
            manager.RegisterHandler(new StraightFourLinkHandler());
            manager.RegisterHandler(new StraightFourPhysicsGravityNodeHandler());
            manager.RegisterHandler(new StraightFourPhysicsJointHandler());
            manager.RegisterHandler(new StraightFourAudioEmitterHandler());
            manager.RegisterHandler(new StraightFourPersonalityHandler());
            manager.RegisterHandler(new StraightFourVehicleBodyHandler());
            manager.RegisterHandler(new StraightFourVehicleWheelHandler());
            manager.RegisterHandler(new StraightFourVehicleThrusterHandler());
#endif
            
            Logging.Log("[StraightFourOMIAdapter] Created extension manager with all handlers.");
            return manager;
        }

        /// <summary>
        /// Post-process the scene after OMI handlers have run.
        /// Creates StraightFour entities from the node-to-entity map if needed.
        /// </summary>
        /// <param name="root">Root GameObject of the loaded scene.</param>
        /// <param name="context">Import context from OMINewtonsoftLoader.</param>
        public IEnumerator PostProcessScene(GameObject root, OMIImportContext context)
        {
            Logging.Log("[StraightFourOMIAdapter] Post-processing scene...");
            
            if (context == null)
            {
                Logging.LogWarning("[StraightFourOMIAdapter] No import context available for post-processing.");
                yield break;
            }
            
            // Get the entity map if handlers created any
            if (context.CustomData.TryGetValue(StraightFourCustomDataKeys.NodeToEntity, out var entityMapObj))
            {
                var entityMap = entityMapObj as Dictionary<int, BaseEntity>;
                if (entityMap != null)
                {
                    Logging.Log($"[StraightFourOMIAdapter] Found {entityMap.Count} entities created by handlers.");
                }
            }
            
            // TODO: Any additional post-processing like applying spawn points to player
            // For now, handlers do most of the work
            
            yield return null;
            Logging.Log("[StraightFourOMIAdapter] Post-processing complete.");
        }

#if NEWTONSOFT_JSON
        /// <summary>
        /// Process a loaded glTF scene and convert to StraightFour entities.
        /// Legacy method for backward compatibility - uses direct JSON parsing.
        /// </summary>
        /// <param name="root">Root GameObject of the loaded scene.</param>
        /// <param name="rawJson">Parsed JSON from the glTF file, or null if not available.</param>
        public IEnumerator ProcessLoadedScene(GameObject root, JObject rawJson)
        {
            // Clear previous data
            nodeShapes.Clear();
            nodePhysicsBodies.Clear();
            nodeSeats.Clear();
            nodeLinks.Clear();
            nodeSpawnPoints.Clear();
            nodeNameToIndex.Clear();

            if (rawJson != null)
            {
                ParseOMIExtensionsFromJson(rawJson);
            }
            else
            {
                Logging.LogWarning("[StraightFourOMIAdapter] No RawJson available, OMI extensions will not be processed.");
            }

            // Create entities from the hierarchy
            yield return StartCoroutine(ProcessHierarchy(root.transform, null));

            Logging.Log("[StraightFourOMIAdapter] Scene processing complete.");
        }

        /// <summary>
        /// Parse OMI extensions directly from the glTF JSON.
        /// </summary>
        private void ParseOMIExtensionsFromJson(JObject json)
        {
            Logging.Log("[StraightFourOMIAdapter] Parsing OMI extensions from JSON...");
            
            // Build node name to index mapping
            BuildNodeNameMapping(json);
            
            // Parse document-level extensions for shapes
            ParseDocumentExtensions(json);
            
            // Parse node extensions
            ParseNodeExtensions(json);
        }

        /// <summary>
        /// Build a mapping of node names to their indices for later lookup.
        /// </summary>
        private void BuildNodeNameMapping(JObject json)
        {
            var nodes = json["nodes"] as JArray;
            if (nodes == null) return;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i] as JObject;
                if (node == null) continue;

                var name = node["name"]?.Value<string>();
                if (!string.IsNullOrEmpty(name))
                {
                    nodeNameToIndex[name] = i;
                    Logging.Log($"[StraightFourOMIAdapter] Node mapping: '{name}' -> index {i}");
                }
            }

            Logging.Log($"[StraightFourOMIAdapter] Built node name mapping with {nodeNameToIndex.Count} entries.");
        }

        /// <summary>
        /// Parse document-level extensions (shapes defined at root).
        /// </summary>
        private void ParseDocumentExtensions(JObject json)
        {
            var extensions = json["extensions"] as JObject;
            if (extensions == null) return;

            // Parse OMI_physics_shape at document level
            var physicsShapeExt = extensions["OMI_physics_shape"] as JObject;
            if (physicsShapeExt != null)
            {
                var shapes = physicsShapeExt["shapes"] as JArray;
                if (shapes != null)
                {
                    Logging.Log($"[StraightFourOMIAdapter] Found {shapes.Count} shapes at document level.");
                }
            }
        }

        /// <summary>
        /// Parse node-level extensions.
        /// </summary>
        private void ParseNodeExtensions(JObject json)
        {
            var nodes = json["nodes"] as JArray;
            if (nodes == null) return;

            // First, get the document-level shapes array
            var docExtensions = json["extensions"] as JObject;
            var shapes = docExtensions?["OMI_physics_shape"]?["shapes"] as JArray;

            for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
            {
                var node = nodes[nodeIndex] as JObject;
                if (node == null) continue;

                var extensions = node["extensions"] as JObject;
                if (extensions == null) continue;

                // Parse OMI_physics_body
                var physicsBody = extensions["OMI_physics_body"] as JObject;
                if (physicsBody != null)
                {
                    var bodyData = ParsePhysicsBody(physicsBody);
                    if (bodyData != null)
                    {
                        nodePhysicsBodies[nodeIndex] = bodyData;
                        Logging.Log($"[StraightFourOMIAdapter] Node {nodeIndex}: physics body motion={bodyData.motionType}");
                    }
                }

                // Parse OMI_physics_shape reference on node
                var physicsShapeRef = extensions["OMI_physics_shape"] as JObject;
                if (physicsShapeRef != null && shapes != null)
                {
                    var shapeIndex = physicsShapeRef["shape"]?.Value<int>() ?? -1;
                    if (shapeIndex >= 0 && shapeIndex < shapes.Count)
                    {
                        var shapeData = ParseShape(shapes[shapeIndex] as JObject);
                        if (shapeData != null)
                        {
                            if (!nodeShapes.ContainsKey(nodeIndex))
                                nodeShapes[nodeIndex] = new List<ShapeData>();
                            nodeShapes[nodeIndex].Add(shapeData);
                            Logging.Log($"[StraightFourOMIAdapter] Node {nodeIndex}: shape type={shapeData.type}");
                        }
                    }
                }

                // Parse OMI_spawn_point
                var spawnPoint = extensions["OMI_spawn_point"] as JObject;
                if (spawnPoint != null)
                {
                    var spawnData = ParseSpawnPoint(spawnPoint);
                    if (spawnData != null)
                    {
                        nodeSpawnPoints[nodeIndex] = spawnData;
                        Logging.Log($"[StraightFourOMIAdapter] Node {nodeIndex}: spawn point team={spawnData.team}");
                    }
                }

                // Parse OMI_seat
                var seat = extensions["OMI_seat"] as JObject;
                if (seat != null)
                {
                    var seatData = ParseSeat(seat);
                    if (seatData != null)
                    {
                        nodeSeats[nodeIndex] = seatData;
                        Logging.Log($"[StraightFourOMIAdapter] Node {nodeIndex}: seat");
                    }
                }

                // Parse OMI_link
                var link = extensions["OMI_link"] as JObject;
                if (link != null)
                {
                    var linkData = ParseLink(link);
                    if (linkData != null)
                    {
                        nodeLinks[nodeIndex] = linkData;
                        Logging.Log($"[StraightFourOMIAdapter] Node {nodeIndex}: link uri={linkData.uri}");
                    }
                }
            }
        }

        /// <summary>
        /// Parse OMI_physics_body extension data.
        /// </summary>
        private PhysicsBodyData ParsePhysicsBody(JObject data)
        {
            try
            {
                var bodyData = new PhysicsBodyData();
                
                // Motion can be a string or object with "type" property
                var motion = data["motion"];
                if (motion is JObject motionObj)
                {
                    bodyData.motionType = motionObj["type"]?.Value<string>() ?? "static";
                    bodyData.mass = motionObj["mass"]?.Value<float>() ?? 1.0f;
                    bodyData.linearVelocity = ParseVector3(motionObj["linearVelocity"] as JArray);
                    bodyData.angularVelocity = ParseVector3(motionObj["angularVelocity"] as JArray);
                    bodyData.centerOfMass = ParseVector3(motionObj["centerOfMass"] as JArray);
                }
                else if (motion != null)
                {
                    // Old format: just a string
                    bodyData.motionType = motion.Value<string>() ?? "static";
                    bodyData.mass = data["mass"]?.Value<float>() ?? 1.0f;
                    bodyData.linearVelocity = ParseVector3(data["linearVelocity"] as JArray);
                    bodyData.angularVelocity = ParseVector3(data["angularVelocity"] as JArray);
                    bodyData.centerOfMass = ParseVector3(data["centerOfMass"] as JArray);
                }
                else
                {
                    bodyData.motionType = "static";
                    bodyData.mass = 1.0f;
                }
                
                return bodyData;
            }
            catch (System.Exception ex)
            {
                Logging.LogError($"[StraightFourOMIAdapter] Error parsing physics body: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse a shape definition from OMI_physics_shape.
        /// </summary>
        private ShapeData ParseShape(JObject shape)
        {
            if (shape == null) return null;

            try
            {
                var data = new ShapeData();
                data.type = shape["type"]?.Value<string>() ?? "box";

                switch (data.type)
                {
                    case "box":
                        var size = shape["box"]?["size"] as JArray;
                        if (size != null && size.Count >= 3)
                        {
                            data.size = new Vector3(
                                size[0].Value<float>(),
                                size[1].Value<float>(),
                                size[2].Value<float>()
                            );
                        }
                        else
                        {
                            data.size = Vector3.one;
                        }
                        break;

                    case "sphere":
                        data.radius = shape["sphere"]?["radius"]?.Value<float>() ?? 0.5f;
                        break;

                    case "capsule":
                        data.radius = shape["capsule"]?["radius"]?.Value<float>() ?? 0.5f;
                        data.height = shape["capsule"]?["height"]?.Value<float>() ?? 2.0f;
                        break;

                    case "cylinder":
                        data.radius = shape["cylinder"]?["radius"]?.Value<float>() ?? 0.5f;
                        data.height = shape["cylinder"]?["height"]?.Value<float>() ?? 2.0f;
                        break;

                    case "convex":
                    case "trimesh":
                        data.meshIndex = shape[data.type]?["mesh"]?.Value<int>() ?? -1;
                        break;
                }

                return data;
            }
            catch (System.Exception ex)
            {
                Logging.LogError($"[StraightFourOMIAdapter] Error parsing shape: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse OMI_spawn_point extension data.
        /// </summary>
        private SpawnPointNodeData ParseSpawnPoint(JObject data)
        {
            try
            {
                return new SpawnPointNodeData
                {
                    title = data["title"]?.Value<string>() ?? "",
                    team = data["team"]?.Value<string>() ?? "",
                    group = data["group"]?.Value<string>() ?? ""
                };
            }
            catch (System.Exception ex)
            {
                Logging.LogError($"[StraightFourOMIAdapter] Error parsing spawn point: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse OMI_seat extension data.
        /// </summary>
        private SeatData ParseSeat(JObject data)
        {
            try
            {
                return new SeatData
                {
                    back = ParseVector3(data["back"] as JArray),
                    foot = ParseVector3(data["foot"] as JArray),
                    knee = ParseVector3(data["knee"] as JArray),
                    angle = data["angle"]?.Value<float>() ?? 1.5707963267948966f
                };
            }
            catch (System.Exception ex)
            {
                Logging.LogError($"[StraightFourOMIAdapter] Error parsing seat: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse OMI_link extension data.
        /// </summary>
        private LinkData ParseLink(JObject data)
        {
            try
            {
                return new LinkData
                {
                    uri = data["uri"]?.Value<string>() ?? "",
                    title = data["title"]?.Value<string>()
                };
            }
            catch (System.Exception ex)
            {
                Logging.LogError($"[StraightFourOMIAdapter] Error parsing link: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Helper to parse a JSON array to Vector3.
        /// </summary>
        private Vector3 ParseVector3(JArray arr)
        {
            if (arr == null || arr.Count < 3)
                return Vector3.zero;
            return new Vector3(
                arr[0].Value<float>(),
                arr[1].Value<float>(),
                arr[2].Value<float>()
            );
        }

        /// <summary>
        /// Recursively process the Unity hierarchy to create StraightFour entities.
        /// </summary>
        private IEnumerator ProcessHierarchy(Transform transform, BaseEntity parentEntity)
        {
            int frameCount = 0;

            foreach (Transform child in transform)
            {
                // Yield periodically to avoid blocking
                frameCount++;
                if (frameCount % 10 == 0)
                {
                    yield return null;
                }

                // Try to get the node index from the GameObject name or extras
                int nodeIndex = TryGetNodeIndex(child.gameObject);

                BaseEntity entity = CreateEntityFromTransform(child, parentEntity, nodeIndex);

                if (entity != null)
                {
                    // Process children recursively
                    yield return StartCoroutine(ProcessHierarchy(child, entity));
                }
            }
        }

        /// <summary>
        /// Try to extract the node index from a GameObject.
        /// </summary>
        private int TryGetNodeIndex(GameObject go)
        {
            string name = go.name;
            
            // First, try exact name match from our mapping
            if (nodeNameToIndex.TryGetValue(name, out int idx))
                return idx;

            // Some loaders add suffixes like " (1)" for duplicate names
            // Try to match base name
            int parenIndex = name.LastIndexOf('(');
            if (parenIndex > 0)
            {
                string baseName = name.Substring(0, parenIndex).TrimEnd();
                if (nodeNameToIndex.TryGetValue(baseName, out int baseIdx))
                    return baseIdx;
            }

            // Fallback: check for index naming patterns
            // Pattern 1: "Node_0", "Node_1", etc.
            if (name.StartsWith("Node_") && int.TryParse(name.Substring(5), out int idx1))
                return idx1;

            // Pattern 2: Just a number
            if (int.TryParse(name, out int idx2))
                return idx2;
            
            return -1;
        }

        /// <summary>
        /// Create a StraightFour entity from a Unity transform.
        /// </summary>
        private BaseEntity CreateEntityFromTransform(Transform transform, BaseEntity parent, int nodeIndex)
        {
            BaseEntity entity = null;
            System.Guid entityId = System.Guid.NewGuid();

            // Check if this object has a mesh
            var meshFilter = transform.GetComponent<MeshFilter>();
            var meshRenderer = transform.GetComponent<MeshRenderer>();

            if (meshFilter != null && meshRenderer != null && meshFilter.sharedMesh != null)
            {
                // Add MeshEntity component to existing GameObject
                var meshEntity = transform.gameObject.AddComponent<MeshEntity>();
                if (parent != null)
                {
                    Debug.Log($"[StraightFourOMIAdapter] SetParent for {transform.name}: parent entity = {parent.name}, parent GameObject = {parent.gameObject.name}");
                }
                else
                {
                    Debug.Log($"[StraightFourOMIAdapter] SetParent for {transform.name}: parent entity = null");
                }
                meshEntity.SetParent(parent);
                meshEntity.Initialize(entityId);
                entity = meshEntity;
            }
            else
            {
                // Add ContainerEntity component to existing GameObject  
                var containerEntity = transform.gameObject.AddComponent<ContainerEntity>();
                if (parent != null)
                {
                    Debug.Log($"[StraightFourOMIAdapter] SetParent for {transform.name}: parent entity = {parent.name}, parent GameObject = {parent.gameObject.name}");
                }
                else
                {
                    Debug.Log($"[StraightFourOMIAdapter] SetParent for {transform.name}: parent entity = null");
                }
                containerEntity.SetParent(parent);
                containerEntity.Initialize(entityId);
                entity = containerEntity;
            }

            if (entity == null)
            {
                Logging.LogWarning($"[StraightFourOMIAdapter] Failed to create entity for {transform.name}");
                return null;
            }

            // Apply OMI extensions if we have data for this node
            if (nodeIndex >= 0)
            {
                ApplyOMIExtensions(entity, transform, nodeIndex);
            }

            // Store entity handle information
            entity.entityTag = entityId.ToString();

            Logging.Log($"[StraightFourOMIAdapter] Created entity for {transform.name} (nodeIndex={nodeIndex})");

            return entity;
        }

        /// <summary>
        /// Apply OMI extensions to a StraightFour entity.
        /// </summary>
        private void ApplyOMIExtensions(BaseEntity entity, Transform transform, int nodeIndex)
        {
            // Apply physics body
            if (nodePhysicsBodies.TryGetValue(nodeIndex, out var physicsBody))
            {
                ApplyPhysicsBody(entity, physicsBody);
            }

            // Register spawn points
            if (nodeSpawnPoints.TryGetValue(nodeIndex, out var spawnPoint))
            {
                RegisterSpawnPoint(transform, spawnPoint);
            }

            // Apply seat behavior
            if (nodeSeats.TryGetValue(nodeIndex, out var seat))
            {
                ApplySeatBehavior(entity, transform, seat);
            }

            // Apply link behavior
            if (nodeLinks.TryGetValue(nodeIndex, out var link))
            {
                ApplyLinkBehavior(entity, transform, link);
            }
        }

        /// <summary>
        /// Apply physics properties from OMI_physics_body to entity.
        /// </summary>
        private void ApplyPhysicsBody(BaseEntity entity, PhysicsBodyData physicsBody)
        {
            // Set physical properties based on OMI physics body
            // StraightFour EntityPhysicalProperties has: angularDrag, centerOfMass, drag, gravitational, mass
            var physProps = new BaseEntity.EntityPhysicalProperties
            {
                mass = physicsBody.mass,
                gravitational = physicsBody.motionType == OMIPhysicsMotionType.Dynamic,
                centerOfMass = physicsBody.centerOfMass != Vector3.zero ? physicsBody.centerOfMass : (Vector3?)null
            };

            // Set physical properties
            entity.SetPhysicalProperties(physProps);

            // Set interaction state based on motion type
            switch (physicsBody.motionType)
            {
                case OMIPhysicsMotionType.Dynamic:
                    // Physical = visible and interactable with physics
                    entity.SetInteractionState(BaseEntity.InteractionState.Physical);
                    break;
                case OMIPhysicsMotionType.Kinematic:
                case OMIPhysicsMotionType.Static:
                default:
                    // Static = visible but not interactable (no physics simulation)
                    entity.SetInteractionState(BaseEntity.InteractionState.Static);
                    break;
            }

            // Apply velocities if dynamic
            if (physicsBody.motionType == OMIPhysicsMotionType.Dynamic)
            {
                var motion = new BaseEntity.EntityMotion
                {
                    velocity = physicsBody.linearVelocity,
                    angularVelocity = physicsBody.angularVelocity,
                    stationary = false
                };
                entity.SetMotion(motion);
            }
            else if (physicsBody.motionType == OMIPhysicsMotionType.Kinematic)
            {
                var motion = new BaseEntity.EntityMotion
                {
                    velocity = Vector3.zero,
                    angularVelocity = Vector3.zero,
                    stationary = true
                };
                entity.SetMotion(motion);
            }

            Logging.Log($"[StraightFourOMIAdapter] Applied physics: motion={physicsBody.motionType}, mass={physicsBody.mass}");
        }

        /// <summary>
        /// Register a spawn point with the registry.
        /// </summary>
        private void RegisterSpawnPoint(Transform transform, SpawnPointNodeData spawnPoint)
        {
            if (spawnPointRegistry == null) return;
            
            var spawnData = new SpawnPointData
            {
                Title = string.IsNullOrEmpty(spawnPoint.title) ? transform.name : spawnPoint.title,
                Position = transform.position,
                Rotation = transform.rotation,
                Team = spawnPoint.team,
                Group = spawnPoint.group
            };
            
            spawnPointRegistry.Register(spawnData.Position, spawnData.Rotation, spawnData.Title, spawnData.Team, spawnData.Group);
            Logging.Log($"[StraightFourOMIAdapter] Registered spawn point: {spawnData.Title}");
        }

        /// <summary>
        /// Apply seat behavior to an entity.
        /// </summary>
        private void ApplySeatBehavior(BaseEntity entity, Transform transform, SeatData seat)
        {
            // Make the entity interactable for sitting - Physical state enables interaction
            entity.SetInteractionState(BaseEntity.InteractionState.Physical);
            
            // Store seat data in the entity for interaction handling
            var seatBehavior = transform.gameObject.AddComponent<OMISeatBehavior>();
            seatBehavior.Initialize(seat.back, seat.foot, seat.angle);
            
            Logging.Log($"[StraightFourOMIAdapter] Applied seat behavior to {transform.name}");
        }

        /// <summary>
        /// Apply link behavior to an entity.
        /// </summary>
        private void ApplyLinkBehavior(BaseEntity entity, Transform transform, LinkData link)
        {
            if (string.IsNullOrEmpty(link.uri)) return;

            // Make the entity interactable for clicking - Physical state enables interaction
            entity.SetInteractionState(BaseEntity.InteractionState.Physical);
            
            // Add link behavior
            var linkBehavior = transform.gameObject.AddComponent<OMILinkBehavior>();
            linkBehavior.Initialize(link.uri, link.title);
            
            Logging.Log($"[StraightFourOMIAdapter] Applied link behavior to {transform.name}: {link.uri}");
        }
#endif

        public void Cleanup()
        {
            nodeShapes.Clear();
            nodePhysicsBodies.Clear();
            nodeSeats.Clear();
            nodeLinks.Clear();
            nodeSpawnPoints.Clear();
            nodeNameToIndex.Clear();
            Logging.Log("[StraightFourOMIAdapter] Cleaned up.");
        }

        /// <summary>
        /// Build a mapping of child node index to parent node index from glTF JSON.
        /// </summary>
        public static Dictionary<int, int> BuildNodeParentMapping(Newtonsoft.Json.Linq.JObject json)
        {
            var parentMap = new Dictionary<int, int>();
            var nodes = json["nodes"] as Newtonsoft.Json.Linq.JArray;
            if (nodes == null) return parentMap;
            for (int parentIdx = 0; parentIdx < nodes.Count; parentIdx++)
            {
                var node = nodes[parentIdx] as Newtonsoft.Json.Linq.JObject;
                if (node == null) continue;
                var children = node["children"] as Newtonsoft.Json.Linq.JArray;
                if (children == null) continue;
                foreach (var childToken in children)
                {
                    int childIdx = childToken.Value<int>();
                    parentMap[childIdx] = parentIdx;
                }
            }
            return parentMap;
        }
    }
}
