// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a mesh entity.
    /// </summary>
    public class MeshEntity : BaseEntity
    {
        /// <summary>
        /// Create a mesh entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="meshObject">Path to the mesh object to load for this entity.</param>
        /// <param name="meshResources">Paths to mesh resources for this entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <param name="checkForUpdateIfCached">Whether or not to check for update if in cache.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity Create(BaseEntity parent, string meshObject, string[] meshResources,
            Vector3 position, Quaternion rotation, string id = null, string onLoaded = null,
            bool checkForUpdateIfCached = true)
        {
            Guid guid;
            if (string.IsNullOrEmpty(id))
            {
                guid = Guid.NewGuid();
            }
            else
            {
                guid = Guid.Parse(id);
            }

            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);
            UnityEngine.Vector3 pos = new UnityEngine.Vector3(position.x, position.y, position.z);
            UnityEngine.Quaternion rot = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            MeshEntity me = new MeshEntity();

            System.Action<StraightFour.Entity.MeshEntity> onEntityLoadedAction =
                new System.Action<StraightFour.Entity.MeshEntity>((meshEntity) =>
            {
                if (meshEntity == null)
                {
                    Logging.LogError("[MeshEntity:Create] Error loading mesh entity.");
                }
                else
                {
                    meshEntity.SetParent(pBE);
                    meshEntity.SetPosition(pos, true);
                    meshEntity.SetRotation(rot, true);

                    me.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                    EntityAPIHelper.AddEntityMapping(me.internalEntity, me);
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { me });
                    }
                }

            });

            WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsMeshEntity(
                meshObject, meshResources, guid, onEntityLoadedAction, 10, checkForUpdateIfCached);

            return me;
        }

        /// <summary>
        /// Create a mesh entity syncronously.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="meshObject">Path to the mesh object to load for this entity.</param>
        /// <param name="meshResources">Paths to mesh resources for this entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <param name="checkForUpdateIfCached">Whether or not to check for update if in cache.</param>
        public static void QueueCreate(BaseEntity parent, string meshObject, string[] meshResources,
            Vector3 position, Quaternion rotation, string id = null, string onLoaded = null,
            bool checkForUpdateIfCached = true)
        {
            EntityAPIHelper.AddMeshEntityCreationJob(new EntityAPIHelper.MeshEntityCreationJob(
                parent, meshObject, meshResources, position, rotation, id, onLoaded, checkForUpdateIfCached));
        }

        /// <summary>
        /// Create a mesh entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the mesh entity configuration.</param>
        /// <param name="parent">Parent entity for the mesh entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created mesh entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, meshEntity) =>
            {
                if (!success || meshEntity == null || !(meshEntity is StraightFour.Entity.MeshEntity))
                {
                    Logging.LogError("[MeshEntity:Create] Error loading mesh entity from JSON.");
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                            onLoaded, new object[] { null });
                    }
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                            onLoaded, new object[] { EntityAPIHelper.GetPublicEntity(
                                (StraightFour.Entity.MeshEntity) meshEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadMeshEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        public static void CreateCollection(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, List<Guid?>, List<StraightFour.Entity.BaseEntity>> onComplete =
                new Action<bool, List<Guid?>, List<StraightFour.Entity.BaseEntity>>((success, entityIds, meshEntities) =>
            {
                if (!success || meshEntities == null || meshEntities.Count == 0 || !(meshEntities[0] is StraightFour.Entity.MeshEntity))
                {
                    Logging.LogError("[MeshEntity:CreateCollection] Error loading mesh entity collection from JSON.");
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                            onLoaded, new object[] { null });
                    }
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        List<APIs.Entity.MeshEntity> publicEntities = new List<APIs.Entity.MeshEntity>();
                        foreach (var meshEntity in meshEntities)
                        {
                            publicEntities.Add(
                                (APIs.Entity.MeshEntity) EntityAPIHelper.GetPublicEntity(meshEntity));
                        }
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                            onLoaded, new object[] { publicEntities.ToArray() });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadMeshEntityCollectionFromJSON(jsonEntity, pBE, onComplete);
        }

        /// <summary>
        /// Create a mesh entity from a cube primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreateCube(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.cubeMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from a sphere primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreateSphere(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.sphereMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from a capsule primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreateCapsule(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.capsuleMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from a cylinder primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreateCylinder(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.cylinderMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from a plane primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreatePlane(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.planeMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from a torus primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreateTorus(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.torusMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from a torus primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreateCone(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.coneMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from a rectangular pyramid primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreateRectangularPyramid(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.rectangularPyramidMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from a tetrahedron primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreateTetrahedron(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.tetrahedronMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from a prism primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreatePrism(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.prismMeshPrefab, color, position, rotation, id, onLoaded);
        }

        /// <summary>
        /// Create a mesh entity from an arch primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        public static MeshEntity CreateArch(BaseEntity parent, Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            return CreatePrimitiveEntity(parent, EntityAPIHelper.archMeshPrefab, color, position, rotation, id, onLoaded);
        }

        internal MeshEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.MeshEntity);
        }

        /// <summary>
        /// Create a mesh entity from a primitive.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="primitive">The mesh object primitive to load for this entity.</param>
        /// <param name="color">Color to apply to the mesh entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// mesh entity object.</param>
        /// <returns>The mesh entity object.</returns>
        private static MeshEntity CreatePrimitiveEntity(BaseEntity parent, UnityEngine.GameObject primitive,
            Color color, Vector3 position, Quaternion rotation,
            string id = null, string onLoaded = null)
        {
            Guid guid;
            if (string.IsNullOrEmpty(id))
            {
                guid = Guid.NewGuid();
            }
            else
            {
                guid = Guid.Parse(id);
            }

            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);
            UnityEngine.Vector3 pos = new UnityEngine.Vector3(position.x, position.y, position.z);
            UnityEngine.Quaternion rot = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            MeshEntity me = new MeshEntity();

            System.Action onEntityLoadedAction = new System.Action(() =>
                {
                    StraightFour.Entity.BaseEntity entity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                    if (entity == null)
                    {
                        Logging.LogError("[GLTFHandler->SetUpLoadedGLTFMeshAsMeshEntity] Unable to find loaded entity.");
                        return;
                    }

                    StraightFour.Entity.MeshEntity meshEntity = (StraightFour.Entity.MeshEntity) entity;

                    if (meshEntity == null)
                    {
                        Logging.LogError("[MeshEntity:CreatePrimitiveEntity] Error loading mesh entity.");
                    }
                    else
                    {
                        meshEntity.SetParent(pBE);
                        meshEntity.SetPosition(pos, true);
                        meshEntity.SetRotation(rot, true);

                        UnityEngine.Renderer rend = meshEntity.gameObject.GetComponent<UnityEngine.Renderer>();
                        if (rend == null)
                        {
                            Logging.LogError("[MeshEntity:CreatePrimitiveEntity] Invalid primitive entity.");
                        }
                        else
                        {
                            rend.material.SetColor("_Color", new UnityEngine.Color(color.r, color.g, color.b, color.a));
                        }

                        me.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                        EntityAPIHelper.AddEntityMapping(me.internalEntity, me);
                        if (!string.IsNullOrEmpty(onLoaded))
                        {
                            WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { me });
                        }
                    }

                });

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadMeshEntity(
                null, primitive, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity,
                guid, null, onEntityLoadedAction);

            return me;
        }
    }
}