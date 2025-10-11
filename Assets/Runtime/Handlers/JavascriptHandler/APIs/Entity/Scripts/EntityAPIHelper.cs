// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.VEML.Schema.V3_0;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// A helper class for the Entity API.
    /// </summary>
    public class EntityAPIHelper : MonoBehaviour
    {
        /// <summary>
        /// A job for creating a mesh entity.
        /// </summary>
        public class MeshEntityCreationJob
        {
            /// <summary>
            /// Parent of the entity to create.
            /// </summary>
            public BaseEntity parent;

            /// <summary>
            /// Path to the mesh object to load for this entity.
            /// </summary>
            public string meshObject;

            /// <summary>
            /// Paths to mesh resources for this entity.
            /// </summary>
            public string[] meshResources;

            /// <summary>
            /// Position of the entity relative to its parent.
            /// </summary>
            public WorldTypes.Vector3 position;

            /// <summary>
            /// Rotation of the entity relative to its parent.
            /// </summary>
            public WorldTypes.Quaternion rotation;

            /// <summary>
            /// ID of the entity. One will be created if not provided.
            /// </summary>
            public string id;

            /// <summary>
            /// Action to perform on load. This takes a single parameter containing the created mesh entity object.
            /// </summary>
            public string onLoaded;

            /// <summary>
            /// Whether or not to check for model update if in cache.
            /// </summary>
            public bool checkForUpdateIfCached;

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
            /// <returns>The mesh entity creation job.</returns>
            public MeshEntityCreationJob(BaseEntity parent, string meshObject, string[] meshResources,
                WorldTypes.Vector3 position, WorldTypes.Quaternion rotation, string id, string onLoaded,
                bool checkForUpdateIfCached)
            {
                this.parent = parent;
                this.meshObject = meshObject;
                this.meshResources = meshResources;
                this.position = position;
                this.rotation = rotation;
                this.id = id;
                this.onLoaded = onLoaded;
                this.checkForUpdateIfCached = checkForUpdateIfCached;
            }
        }

        /// <summary>
        /// Prefab for a cube mesh.
        /// </summary>
        public static GameObject cubeMeshPrefab;

        /// <summary>
        /// Prefab for a sphere mesh.
        /// </summary>
        public static GameObject sphereMeshPrefab;

        /// <summary>
        /// Prefab for a capsule mesh.
        /// </summary>
        public static GameObject capsuleMeshPrefab;

        /// <summary>
        /// Prefab for a cylinder mesh.
        /// </summary>
        public static GameObject cylinderMeshPrefab;

        /// <summary>
        /// Prefab for a plane mesh.
        /// </summary>
        public static GameObject planeMeshPrefab;

        /// <summary>
        /// Prefab for a torus mesh.
        /// </summary>
        public static GameObject torusMeshPrefab;

        /// <summary>
        /// Prefab for a cone mesh.
        /// </summary>
        public static GameObject coneMeshPrefab;

        /// <summary>
        /// Prefab for a rectangular pyramid.
        /// </summary>
        public static GameObject rectangularPyramidMeshPrefab;

        /// <summary>
        /// Prefab for a tetrahedron.
        /// </summary>
        public static GameObject tetrahedronMeshPrefab;
        /// <summary>
        /// Prefab for a prism.
        /// </summary>
        public static GameObject prismMeshPrefab;

        /// <summary>
        /// Prefab for an arch.
        /// </summary>
        public static GameObject archMeshPrefab;

        /// <summary>
        /// Dictionary of Entity API references and internal entity references.
        /// </summary>
        private static Dictionary<StraightFour.Entity.BaseEntity, BaseEntity> loadedEntities;

        /// <summary>
        /// Instance of the Entity API Helper.
        /// </summary>
        private static EntityAPIHelper instance;

        /// <summary>
        /// Pending Mesh Entity creation jobs.
        /// </summary>
        private Queue<MeshEntityCreationJob> meshEntityCreationJobs;

        /// <summary>
        /// The current Mesh Entity creation job.
        /// </summary>
        private MeshEntityCreationJob currentMeshEntityCreationJob;

        /// <summary>
        /// Initialize the entity dictionary.
        /// </summary>
        public static void InitializeEntityMapping()
        {
            loadedEntities = new Dictionary<StraightFour.Entity.BaseEntity, BaseEntity>();
        }

        /// <summary>
        /// Add an entity mapping.
        /// </summary>
        /// <param name="internalEntity">Internal entity reference.</param>
        /// <param name="publicEntity">API entity reference.</param>
        public static void AddEntityMapping(StraightFour.Entity.BaseEntity internalEntity, BaseEntity publicEntity)
        {
            if (loadedEntities.ContainsKey(internalEntity))
            {
                Logging.LogWarning("[EntityAPIHelper:AddEntityMapping] Key already exists.");
                return;
            }
            loadedEntities.Add(internalEntity, publicEntity);
        }

        /// <summary>
        /// Remove an entity mapping.
        /// </summary>
        /// <param name="internalEntity">Internal entity reference.</param>
        public static void RemoveEntityMapping(StraightFour.Entity.BaseEntity internalEntity)
        {
            if (!loadedEntities.ContainsKey(internalEntity))
            {
                Logging.LogWarning("[EntityAPIHelper:RemoveEntityMapping] Key does not exist.");
                return;
            }

            loadedEntities.Remove(internalEntity);
        }

        /// <summary>
        /// Get the API entity reference for an internal entity.
        /// </summary>
        /// <param name="internalEntity">Internal entity reference.</param>
        public static BaseEntity GetPublicEntity(StraightFour.Entity.BaseEntity internalEntity)
        {
            if (internalEntity == null)
            {
                return null;
            }

            if (loadedEntities.ContainsKey(internalEntity))
            {
                return loadedEntities[internalEntity];
            }

            return null;
        }

        /// <summary>
        /// Get the internal entity reference for an API entity.
        /// </summary>
        /// <param name="publicEntity">API entity reference.</param>
        public static StraightFour.Entity.BaseEntity GetPrivateEntity(BaseEntity publicEntity)
        {
            if (publicEntity == null)
            {
                return null;
            }

            return loadedEntities.ContainsValue(publicEntity) ?
                loadedEntities.FirstOrDefault(x => x.Value == publicEntity).Key : null;
        }

        /// <summary>
        /// Set information for a voxel block type.
        /// </summary>
        /// <param name="id">Block ID.</param>
        /// <param name="info">Information to apply to voxel block type.</param>
        /// <param name="internalEntity">Internal entity reference.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetBlockInfoAsync(int id, VoxelBlockInfo info, StraightFour.Entity.VoxelEntity internalEntity)
        {
            if (info == null)
            {
                Logging.LogWarning("[EntityAPIHelper:SetBlockInfoAsync] Invalid Block Info.");
                return false;
            }

            if (instance == null)
            {
                Logging.LogError("[EntityAPIHelper:SetBlockInfoAsync] Entity API Helper not set up.");
                return false;
            }
            instance.StartCoroutine(instance.SetBlockInfoCoroutine(id, info, internalEntity));
            return true;
        }

        /// <summary>
        /// Create a hybrid terrain entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="length">Length of the terrain in terrain units.</param>
        /// <param name="width">Width of the terrain in terrain units.</param>
        /// <param name="height">Height of the terrain in terrain units.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="modifications">Modifications to be made to the terrain.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// terrain entity object.</param>
        /// <param name="timeout">Timeout for PNG loads.</param>
        /// <returns>The hybrid terrain entity object.</returns>
        public static TerrainEntity LoadHybridTerrainEntityAsync(BaseEntity parent, float length, float width,
            float height, float[,] heights, TerrainEntityLayer[] layers, TerrainEntityLayerMaskCollection layerMasks,
            TerrainEntityModification[] modifications, WorldTypes.Vector3 position, WorldTypes.Quaternion rotation,
            bool stitchTerrains, string id = null, string tag = null, string onLoaded = null,
            float timeout = 10)
        {
            TerrainEntity te = new TerrainEntity(TerrainEntity.TerrainEntityType.hybrid);

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
            Vector3 pos = new Vector3(position.x, position.y, position.z);
            Quaternion rot = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            if (heights == null)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Invalid heights array.");
                return null;
            }

            if (layers == null || layers.Length < 1)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Invalid layers array.");
                return null;
            }

            if (layerMasks == null)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Invalid layer masks.");
                return null;
            }

            Action onLoadAction = null;
            onLoadAction = () =>
            {
                te.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                AddEntityMapping(te.internalEntity, te);
                if (modifications != null)
                {
                    foreach (TerrainEntityModification mod in modifications)
                    {
                        if (mod.operation == TerrainEntityModification.TerrainEntityOperation.Build)
                        {
                            te.Build(mod.position, mod.brushType, mod.layer, mod.size, false);
                        }
                        else if (mod.operation == TerrainEntityModification.TerrainEntityOperation.Dig)
                        {
                            Logging.Log("bs " + mod.size);
                            te.Dig(mod.position, mod.brushType, mod.layer, mod.size, false);
                        }
                        else
                        {
                            Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Unsupported modification. Skipping.");
                        }
                    }
                }
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { te });
                }
            };

            instance.StartCoroutine(instance.LoadHybridTerrainEntityCoroutine(te, pBE, guid, length, width, height, heights, layers,
                layerMasks.ToFloatArrays(), pos, rot, stitchTerrains, tag, onLoadAction, timeout));

            return te;
        }

        /// <summary>
        /// Create a hybrid terrain entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="length">Length of the terrain in terrain units.</param>
        /// <param name="width">Width of the terrain in terrain units.</param>
        /// <param name="height">Height of the terrain in terrain units.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="modifications">Modifications to be made to the terrain.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// terrain entity object.</param>
        /// <param name="timeout">Timeout for PNG loads.</param>
        /// <returns>The hybrid terrain entity object.</returns>
        public static TerrainEntity LoadHybridTerrainEntityAsync(BaseEntity parent, float length, float width,
            float height, float[][] heights, TerrainEntityLayer[] layers, TerrainEntityLayerMaskCollection layerMasks,
            TerrainEntityModification[] modifications, WorldTypes.Vector3 position, WorldTypes.Quaternion rotation,
            bool stitchTerrains, string id = null, string tag = null, string onLoaded = null, float timeout = 10,
            System.Action<TerrainEntity> onLoadedAction = null)
        {
            TerrainEntity te = new TerrainEntity(TerrainEntity.TerrainEntityType.hybrid);

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
            Vector3 pos = new Vector3(position.x, position.y, position.z);
            Quaternion rot = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            if (heights == null || heights[0] == null)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Invalid heights array.");
                return null;
            }

            if (layers == null || layers.Length < 1)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Invalid layers array.");
                return null;
            }

            if (layerMasks == null)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Invalid layer masks.");
                return null;
            }

            Action onLoadAction = null;
            onLoadAction = () =>
            {
                te.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                AddEntityMapping(te.internalEntity, te);
                if (modifications != null)
                {
                    foreach (TerrainEntityModification mod in modifications)
                    {
                        if (mod.operation == TerrainEntityModification.TerrainEntityOperation.Build)
                        {
                            te.Build(mod.position, mod.brushType, mod.layer, mod.size, false);
                        }
                        else if (mod.operation == TerrainEntityModification.TerrainEntityOperation.Dig)
                        {
                            te.Dig(mod.position, mod.brushType, mod.layer, mod.size, false);
                        }
                        else
                        {
                            Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Unsupported modification. Skipping.");
                        }
                    }
                }
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { te });
                }
                if (onLoadedAction != null)
                {
                    onLoadedAction.Invoke(te);
                }
            };

            instance.StartCoroutine(instance.LoadHybridTerrainEntityCoroutine(te, pBE, guid, length, width, height, heights, layers,
                layerMasks.ToFloatArrays(), pos, rot, stitchTerrains, tag, onLoadAction, timeout));

            return te;
        }

        /// <summary>
        /// Create a hybrid terrain entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="length">Length of the terrain in terrain units.</param>
        /// <param name="width">Width of the terrain in terrain units.</param>
        /// <param name="height">Height of the terrain in terrain units.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="modifications">Modifications to be made to the terrain.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load.</param>
        /// <param name="timeout">Timeout for PNG loads.</param>
        /// <returns>The hybrid terrain entity object.</returns>
        public static TerrainEntity LoadHybridTerrainEntityAsync(BaseEntity parent, float length, float width,
        float height, float[][] heights, TerrainEntityLayer[] layers, TerrainEntityLayerMaskCollection layerMasks,
        TerrainEntityModification[] modifications, WorldTypes.Vector3 position, WorldTypes.Quaternion rotation,
        bool stitchTerrains, string id = null, string tag = null, Action onLoaded = null,
        float timeout = 10)
        {
            TerrainEntity te = new TerrainEntity(TerrainEntity.TerrainEntityType.hybrid);

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
            Vector3 pos = new Vector3(position.x, position.y, position.z);
            Quaternion rot = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            if (heights == null || heights[0] == null)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Invalid heights array.");
                return null;
            }

            if (layers == null || layers.Length < 1)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Invalid layers array.");
                return null;
            }

            if (layerMasks == null)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadHybridTerrainEntityAsync] Invalid layer masks.");
                return null;
            }

            instance.StartCoroutine(instance.LoadHybridTerrainEntityCoroutine(te, pBE, guid, length, width, height, heights, layers,
                layerMasks.ToFloatArrays(), pos, rot, stitchTerrains, tag, onLoaded, timeout));

            return te;
        }

        /// <summary>
        /// Load an image entity asynchronously.
        /// </summary>
        /// <param name="parent">Parent entity.</param>
        /// <param name="imageFile">Image file.</param>
        /// <param name="positionPercent">Position.</param>
        /// <param name="sizePercent">Size.</param>
        /// <param name="id">ID.</param>
        /// <param name="tag">Tag.</param>
        /// <param name="onLoaded">Action to perform on load.</param>
        /// <returns></returns>
        public static ImageEntity LoadImageEntityAsync(BaseEntity parent, string imageFile,
            WorldTypes.Vector2 positionPercent, WorldTypes.Vector2 sizePercent,
            string id = null, string tag = null, string onLoaded = null)
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

            StraightFour.Entity.BaseEntity pCE = EntityAPIHelper.GetPrivateEntity(parent);
            if (pCE == null)
            {
                Logging.LogWarning("[ImageEntity->LoadImageEntityAsync] Invalid parent entity.");
                return null;
            }
            if (pCE is not StraightFour.Entity.UIEntity)
            {
                Logging.LogWarning("[ImageEntity->LoadImageEntityAsync] Parent entity not UI element.");
                return null;
            }

            UnityEngine.Vector2 pos = new UnityEngine.Vector2(positionPercent.x, positionPercent.y);
            UnityEngine.Vector2 size = new UnityEngine.Vector2(sizePercent.x, sizePercent.y);

            ImageEntity ie = new ImageEntity();

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                ie.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(ie.internalEntity, ie);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ie });
                }
            };

            instance.StartCoroutine(instance.LoadImageEntityAsync(ie, (StraightFour.Entity.UIEntity) pCE,
                guid, imageFile, pos, size, tag, onLoadAction));

            return ie;
        }

        /// <summary>
        /// Load audio from a file asynchronously.
        /// </summary>
        /// <param name="file">Audio file.</param>
        /// <param name="audioEntity">Audio entity to play audio in.</param>
        public static void LoadAudioFromFileAsync(string file, StraightFour.Entity.AudioEntity audioEntity)
        {
            instance.StartCoroutine(instance.LoadAudioFromFile(file, audioEntity));
        }

        /// <summary>
        /// Apply image to button asynchronously.
        /// </summary>
        /// <param name="file">Image file.</param>
        /// <param name="buttonEntity">Button entity to apply image to.</param>
        public static void ApplyImageToButtonAsync(string file, StraightFour.Entity.ButtonEntity buttonEntity)
        {
            instance.StartCoroutine(instance.ApplyImageToButton(file, buttonEntity));
        }

        /// <summary>
        /// Apply image to dropdown asynchronously.
        /// </summary>
        /// <param name="file">Image file.</param>
        /// <param name="buttonEntity">Dropdown entity to apply image to.</param>
        public static void ApplyImageToDropdownAsync(string file, StraightFour.Entity.DropdownEntity dropdownEntity)
        {
            instance.StartCoroutine(instance.ApplyImageToDropdown(file, dropdownEntity));
        }

        /// <summary>
        /// Register a private entity.
        /// </summary>
        /// <param name="entityToRegister">Entity to register.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool RegisterPrivateEntity(StraightFour.Entity.BaseEntity entityToRegister)
        {
            if (entityToRegister is StraightFour.Entity.AirplaneEntity)
            {
                AirplaneEntity ae = new AirplaneEntity();
                ae.internalEntity = entityToRegister;
                ae.internalEntityType = typeof(StraightFour.Entity.AirplaneEntity);
                AddEntityMapping(entityToRegister, ae);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.AudioEntity)
            {
                AudioEntity ae = new AudioEntity();
                ae.internalEntity = entityToRegister;
                ae.internalEntityType = typeof(StraightFour.Entity.AudioEntity);
                AddEntityMapping(entityToRegister, ae);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.AutomobileEntity)
            {
                AutomobileEntity ae = new AutomobileEntity();
                ae.internalEntity = entityToRegister;
                ae.internalEntityType = typeof(StraightFour.Entity.AutomobileEntity);
                AddEntityMapping(entityToRegister, ae);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.ButtonEntity)
            {
                ButtonEntity be = new ButtonEntity();
                be.internalEntity = entityToRegister;
                be.internalEntityType = typeof(StraightFour.Entity.ButtonEntity);
                AddEntityMapping(entityToRegister, be);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.CanvasEntity)
            {
                CanvasEntity ce = new CanvasEntity();
                ce.internalEntity = entityToRegister;
                ce.internalEntityType = typeof(StraightFour.Entity.CanvasEntity);
                AddEntityMapping(entityToRegister, ce);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.CharacterEntity)
            {
                CharacterEntity ce = new CharacterEntity();
                ce.internalEntity = entityToRegister;
                ce.internalEntityType = typeof(StraightFour.Entity.CanvasEntity);
                AddEntityMapping(entityToRegister, ce);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.ContainerEntity)
            {
                ContainerEntity ce = new ContainerEntity();
                ce.internalEntity = entityToRegister;
                ce.internalEntityType = typeof(StraightFour.Entity.ContainerEntity);
                AddEntityMapping(entityToRegister, ce);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.HTMLEntity)
            {
                HTMLEntity he = new HTMLEntity(false);
                he.internalEntity = entityToRegister;
                he.internalEntityType = typeof(StraightFour.Entity.HTMLEntity);
                AddEntityMapping(entityToRegister, he);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.HTMLUIElementEntity)
            {
                HTMLEntity he = new HTMLEntity(false);
                he.internalEntity = entityToRegister;
                he.internalEntityType = typeof(StraightFour.Entity.HTMLUIElementEntity);
                AddEntityMapping(entityToRegister, he);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.ImageEntity)
            {
                ImageEntity ie = new ImageEntity();
                ie.internalEntity = entityToRegister;
                ie.internalEntityType = typeof(StraightFour.Entity.ImageEntity);
                AddEntityMapping(entityToRegister, ie);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.InputEntity)
            {
                InputEntity ie = new InputEntity();
                ie.internalEntity = entityToRegister;
                ie.internalEntityType = typeof(StraightFour.Entity.InputEntity);
                AddEntityMapping(entityToRegister, ie);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.LightEntity)
            {
                LightEntity le = new LightEntity();
                le.internalEntity = entityToRegister;
                le.internalEntityType = typeof(StraightFour.Entity.LightEntity);
                AddEntityMapping(entityToRegister, le);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.MeshEntity)
            {
                MeshEntity me = new MeshEntity();
                me.internalEntity = entityToRegister;
                me.internalEntityType = typeof(StraightFour.Entity.MeshEntity);
                AddEntityMapping(entityToRegister, me);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.TerrainEntity)
            {
                TerrainEntity te = new TerrainEntity(TerrainEntity.TerrainEntityType.heightmap);
                te.internalEntity = entityToRegister;
                te.internalEntityType = typeof(StraightFour.Entity.TerrainEntity);
                AddEntityMapping(entityToRegister, te);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.HybridTerrainEntity)
            {
                TerrainEntity te = new TerrainEntity(TerrainEntity.TerrainEntityType.hybrid);
                te.internalEntity = entityToRegister;
                te.internalEntityType = typeof(StraightFour.Entity.HybridTerrainEntity);
                AddEntityMapping(entityToRegister, te);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.TextEntity)
            {
                TextEntity te = new TextEntity();
                te.internalEntity = entityToRegister;
                te.internalEntityType = typeof(StraightFour.Entity.TextEntity);
                AddEntityMapping(entityToRegister, te);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.VoxelEntity)
            {
                VoxelEntity ve = new VoxelEntity();
                ve.internalEntity = entityToRegister;
                ve.internalEntityType = typeof(StraightFour.Entity.VoxelEntity);
                AddEntityMapping(entityToRegister, ve);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.WaterBlockerEntity)
            {
                WaterBlockerEntity we = new WaterBlockerEntity();
                we.internalEntity = entityToRegister;
                we.internalEntityType = typeof(StraightFour.Entity.WaterBlockerEntity);
                AddEntityMapping(entityToRegister, we);
                return true;
            }
            else if (entityToRegister is StraightFour.Entity.WaterBodyEntity)
            {
                WaterEntity we = new WaterEntity();
                we.internalEntity = entityToRegister;
                we.internalEntityType = typeof(StraightFour.Entity.WaterBodyEntity);
                AddEntityMapping(entityToRegister, we);
                return true;
            }
            else
            {
                Logging.LogError("[EntityAPIHelper->RegisterPrivateEntity] Invalid entity type.");
                return false;
            }
        }

        /// <summary>
        /// Initialize the Entity API Helper.
        /// </summary>
        public void Initialize()
        {
            instance = this;
            meshEntityCreationJobs = new Queue<MeshEntityCreationJob>();
            currentMeshEntityCreationJob = null;
        }

        /// <summary>
        /// Add a job for Mesh Entity creation.
        /// </summary>
        /// <param name="job">Job to add.</param>
        public static void AddMeshEntityCreationJob(MeshEntityCreationJob job)
        {
            instance.meshEntityCreationJobs.Enqueue(job);
        }

        /// <summary>
        /// Set information for a voxel block type in a coroutine.
        /// </summary>
        /// <param name="id">Block ID.</param>
        /// <param name="info">Information to apply to voxel block type.</param>
        /// <param name="internalEntity">Internal entity reference.</param>
        /// <param name="timeout">Timeout period after which to abort setting the block information.</param>
        private IEnumerator SetBlockInfoCoroutine(int id, VoxelBlockInfo info, StraightFour.Entity.VoxelEntity internalEntity, float timeout = 10)
        {
            if (info == null)
            {
                Logging.LogWarning("[EntityAPIHelper:SetBlockInfoCoroutine] Invalid Block Info.");
                yield return null;
            }

            StraightFour.Entity.Voxels.BlockInfo blockInfo = new StraightFour.Entity.Voxels.BlockInfo(info.id);

            foreach (int key in blockInfo.subTypes.Keys)
            {
                uint completedRequests = 0;

                Texture2D leftTex = null;
                Texture2D rightTex = null;
                Texture2D topTex = null;
                Texture2D bottomTex = null;
                Texture2D frontTex = null;
                Texture2D backTex = null;

                Action<Texture2D> lOnDownloaded = new Action<Texture2D>((tex) =>
                {
                    leftTex = tex;
                    completedRequests++;
                });
                WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(info.subTypes[key].leftTex, WebVerseRuntime.Instance.currentBasePath), lOnDownloaded);

                Action<Texture2D> rOnDownloaded = new Action<Texture2D>((tex) =>
                {
                    rightTex = tex;
                    completedRequests++;
                });
                WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(info.subTypes[key].rightTex, WebVerseRuntime.Instance.currentBasePath), rOnDownloaded);

                Action<Texture2D> fOnDownloaded = new Action<Texture2D>((tex) =>
                {
                    frontTex = tex;
                    completedRequests++;
                });
                WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(info.subTypes[key].frontTex, WebVerseRuntime.Instance.currentBasePath), fOnDownloaded);

                Action<Texture2D> bOnDownloaded = new Action<Texture2D>((tex) =>
                {
                    backTex = tex;
                    completedRequests++;
                });
                WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(info.subTypes[key].backTex, WebVerseRuntime.Instance.currentBasePath), bOnDownloaded);

                Action<Texture2D> tOnDownloaded = new Action<Texture2D>((tex) =>
                {
                    topTex = tex;
                    completedRequests++;
                });
                WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(info.subTypes[key].topTex, WebVerseRuntime.Instance.currentBasePath), tOnDownloaded);

                Action<Texture2D> boOnDownloaded = new Action<Texture2D>((tex) =>
                {
                    bottomTex = tex;
                    completedRequests++;
                });
                WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(info.subTypes[key].bottomTex, WebVerseRuntime.Instance.currentBasePath), boOnDownloaded);

                float elapsedTime = 0;
                do
                {
                    yield return new WaitForSeconds(0.25f);
                    elapsedTime += 0.25f;
                } while (elapsedTime < timeout && completedRequests < 6);

                blockInfo.AddSubType(blockInfo.subTypes[key].id, blockInfo.subTypes[key].invisible,
                    topTex, bottomTex, leftTex, rightTex, frontTex, backTex);
            }

            internalEntity.SetBlockInfo(id, blockInfo);
        }

        /// <summary>
        /// Create a hybrid terrain entity in a coroutine.
        /// </summary>
        /// <param name="te">Public terrain entity object.</param>
        /// <param name="pBE">Parent entity.</param>
        /// <param name="guid">ID for the terrain entity.</param>
        /// <param name="length">Length of the terrain in terrain units.</param>
        /// <param name="width">Width of the terrain in terrain units.</param>
        /// <param name="height">Height of the terrain in terrain units.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="pos">Position of the entity relative to its parent.</param>
        /// <param name="rot">Rotation of the entity relative to its parent.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// terrain entity object.</param>
        /// <param name="timeout">Timeout for PNG loads.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator LoadHybridTerrainEntityCoroutine(TerrainEntity te, StraightFour.Entity.BaseEntity pBE, Guid guid,
            float length, float width, float height, float[,] heights, TerrainEntityLayer[] layers, float[][,] layerMasks,
            Vector3 pos, Quaternion rot, bool stitchTerrains, string tag = null, Action onLoaded = null, float timeout = 10)
        {
            Dictionary<int, float[,]> formattedMasks = new Dictionary<int, float[,]>();
            if (layerMasks != null)
            {
                int layerIdx = 0;
                foreach (float[,] layerMask in layerMasks)
                {
                    formattedMasks.Add(layerIdx++, layerMask);
                }
            }

            List<StraightFour.Entity.Terrain.TerrainEntityLayer> formattedLayers
                    = new List<StraightFour.Entity.Terrain.TerrainEntityLayer>();
            if (layers != null)
            {
                foreach (TerrainEntityLayer layer in layers)
                {
                    uint completedRequests = 0;

                    StraightFour.Entity.Terrain.TerrainEntityLayer newFormattedLayer
                        = new StraightFour.Entity.Terrain.TerrainEntityLayer();
                    newFormattedLayer.metallic = layer.metallic;
                    newFormattedLayer.smoothness = layer.smoothness;
                    newFormattedLayer.specular = new Color(layer.specular.r,
                        layer.specular.g, layer.specular.b, layer.specular.a);

                    if (!string.IsNullOrEmpty(layer.diffuseTexture))
                    {
                        WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                            VEML.VEMLUtilities.FullyQualifyURI(layer.diffuseTexture, WebVerseRuntime.Instance.currentBasePath),
                        new Action<Texture2D>((tex) =>
                        {
                            newFormattedLayer.diffuse = tex;
                            newFormattedLayer.diffusePath = layer.diffuseTexture;
                            completedRequests++;
                        }), TextureFormat.RGB24);
                    }
                    else
                    {
                        completedRequests++;
                    }

                    if (!string.IsNullOrEmpty(layer.normalTexture))
                    {
                        WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                            VEML.VEMLUtilities.FullyQualifyURI(layer.normalTexture, WebVerseRuntime.Instance.currentBasePath),
                        new Action<Texture2D>((tex) =>
                        {
                            newFormattedLayer.normal = tex;
                            newFormattedLayer.normalPath = layer.normalTexture;
                            completedRequests++;
                        }), TextureFormat.RGB24);
                    }
                    else
                    {
                        completedRequests++;
                    }

                    if (!string.IsNullOrEmpty(layer.maskTexture))
                    {
                        WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                            VEML.VEMLUtilities.FullyQualifyURI(layer.maskTexture, WebVerseRuntime.Instance.currentBasePath),
                        new Action<Texture2D>((tex) =>
                        {
                            newFormattedLayer.mask = tex;
                            newFormattedLayer.maskPath = layer.maskTexture;
                            completedRequests++;
                        }), TextureFormat.RGB24);
                    }
                    else
                    {
                        completedRequests++;
                    }

                    newFormattedLayer.sizeFactor = layer.sizeFactor;
                    if (layer.sizeFactor < 1)
                    {
                        newFormattedLayer.sizeFactor = 1;
                    }

                    float elapsedTime = 0;
                    do
                    {
                        yield return new WaitForSeconds(0.25f);
                        elapsedTime += 0.25f;
                    } while (elapsedTime < timeout && completedRequests < 3);

                    formattedLayers.Add(newFormattedLayer);
                }
            }

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadHybridTerrainEntity(length, width, height, heights,
                formattedLayers.ToArray(), formattedMasks, pBE, pos, rot, stitchTerrains, guid, tag, onLoaded);
        }

        /// <summary>
        /// Create a hybrid terrain entity in a coroutine.
        /// </summary>
        /// <param name="te">Public terrain entity object.</param>
        /// <param name="pBE">Parent entity.</param>
        /// <param name="guid">ID for the terrain entity.</param>
        /// <param name="length">Length of the terrain in terrain units.</param>
        /// <param name="width">Width of the terrain in terrain units.</param>
        /// <param name="height">Height of the terrain in terrain units.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="pos">Position of the entity relative to its parent.</param>
        /// <param name="rot">Rotation of the entity relative to its parent.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// terrain entity object.</param>
        /// <param name="timeout">Timeout for PNG loads.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator LoadHybridTerrainEntityCoroutine(TerrainEntity te, StraightFour.Entity.BaseEntity pBE, Guid guid,
            float length, float width, float height, float[][] heights, TerrainEntityLayer[] layers, float[][,] layerMasks,
            Vector3 pos, Quaternion rot, bool stitchTerrains, string tag = null, Action onLoaded = null, float timeout = 10)
        {
            Dictionary<int, float[,]> formattedMasks = new Dictionary<int, float[,]>();
            if (layerMasks != null)
            {
                int layerIdx = 0;
                foreach (float[,] layerMask in layerMasks)
                {
                    formattedMasks.Add(layerIdx++, layerMask);
                }
            }

            float[,] processedHeights = new float[heights.Length, heights[0].Length];
            for (int i = 0; i < heights.Length; i++)
            {
                for (int j = 0; j < heights[0].Length; j++)
                {
                    processedHeights[i, j] = heights[i][j];
                }
                yield return null;
            }

            List<StraightFour.Entity.Terrain.TerrainEntityLayer> formattedLayers
                    = new List<StraightFour.Entity.Terrain.TerrainEntityLayer>();
            if (layers != null)
            {
                foreach (TerrainEntityLayer layer in layers)
                {
                    uint completedRequests = 0;

                    StraightFour.Entity.Terrain.TerrainEntityLayer newFormattedLayer
                        = new StraightFour.Entity.Terrain.TerrainEntityLayer();
                    newFormattedLayer.metallic = layer.metallic;
                    newFormattedLayer.smoothness = layer.smoothness;
                    newFormattedLayer.specular = new Color(layer.specular.r,
                        layer.specular.g, layer.specular.b, layer.specular.a);

                    if (!string.IsNullOrEmpty(layer.diffuseTexture))
                    {
                        WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                            VEML.VEMLUtilities.FullyQualifyURI(layer.diffuseTexture, WebVerseRuntime.Instance.currentBasePath),
                        new Action<Texture2D>((tex) =>
                        {
                            newFormattedLayer.diffuse = tex;
                            newFormattedLayer.diffusePath = layer.diffuseTexture;
                            completedRequests++;
                        }), TextureFormat.RGB24);
                    }
                    else
                    {
                        completedRequests++;
                    }

                    if (!string.IsNullOrEmpty(layer.normalTexture))
                    {
                        WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                            VEML.VEMLUtilities.FullyQualifyURI(layer.normalTexture, WebVerseRuntime.Instance.currentBasePath),
                        new Action<Texture2D>((tex) =>
                        {
                            newFormattedLayer.normal = tex;
                            newFormattedLayer.normalPath = layer.normalTexture;
                            completedRequests++;
                        }), TextureFormat.RGB24);
                    }
                    else
                    {
                        completedRequests++;
                    }

                    if (!string.IsNullOrEmpty(layer.maskTexture))
                    {
                        WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                            VEML.VEMLUtilities.FullyQualifyURI(layer.maskTexture, WebVerseRuntime.Instance.currentBasePath),
                        new Action<Texture2D>((tex) =>
                        {
                            newFormattedLayer.mask = tex;
                            newFormattedLayer.maskPath = layer.maskTexture;
                            completedRequests++;
                        }), TextureFormat.RGB24);
                    }
                    else
                    {
                        completedRequests++;
                    }

                    newFormattedLayer.sizeFactor = layer.sizeFactor;
                    if (layer.sizeFactor < 1)
                    {
                        newFormattedLayer.sizeFactor = 1;
                    }

                    float elapsedTime = 0;
                    do
                    {
                        yield return new WaitForSeconds(0.25f);
                        elapsedTime += 0.25f;
                    } while (elapsedTime < timeout && completedRequests < 3);

                    formattedLayers.Add(newFormattedLayer);
                }
            }

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadHybridTerrainEntity(length, width, height, processedHeights,
                formattedLayers.ToArray(), formattedMasks, pBE, pos, rot, stitchTerrains, guid, tag, onLoaded);
        }

        /// <summary>
        /// Load an image entity asynchronously.
        /// </summary>
        /// <param name="ie">Image entity to load on.</param>
        /// <param name="pCE">Parent entity.</param>
        /// <param name="imageFile">Image file.</param>
        /// <param name="positionPercent">Position.</param>
        /// <param name="sizePercent">Size.</param>
        /// <param name="id">ID.</param>
        /// <param name="tag">Tag.</param>
        /// <param name="onLoaded">Action to perform on load.</param>
        /// <returns></returns>
        private IEnumerator LoadImageEntityAsync(ImageEntity ie, StraightFour.Entity.UIEntity pCE, Guid id, string imageFile,
            Vector2 positionPercent, Vector2 sizePercent, string tag = null, Action onLoaded = null)
        {
            if (!string.IsNullOrEmpty(imageFile))
            {
                WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(imageFile, WebVerseRuntime.Instance.currentBasePath),
                new Action<Texture2D>((tex) =>
                {
                    StraightFour.StraightFour.ActiveWorld.entityManager.LoadImageEntity(
                        tex, pCE, positionPercent, sizePercent, id, tag, onLoaded);
                }));
            }

            yield return null;
        }

        /// <summary>
        /// Load audio from a file in a coroutine.
        /// </summary>
        /// <param name="file">Audio file to load.</param>
        /// <param name="audioEntity">Audio entity to load audio in.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator LoadAudioFromFile(string file, StraightFour.Entity.AudioEntity audioEntity)
        {
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(file, AudioType.UNKNOWN);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Logging.LogWarning("[EntityAPIHelper->LoadAudioFromFile] Invalid audio file.");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                audioEntity.audioClip = clip;
            }
        }

        /// <summary>
        /// Apply image to button entity in a coroutine.
        /// </summary>
        /// <param name="file">Image file.</param>
        /// <param name="buttonEntity">Button entity to apply image to.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator ApplyImageToButton(string file, StraightFour.Entity.ButtonEntity buttonEntity)
        {
            if (!string.IsNullOrEmpty(file))
            {
                WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(file, WebVerseRuntime.Instance.currentBasePath),
                new Action<Texture2D>((tex) =>
                {
                    buttonEntity.SetBackground(Sprite.Create(
                        tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                }));
            }

            yield return null;
        }

        /// <summary>
        /// Apply image to dropdown entity in a coroutine.
        /// </summary>
        /// <param name="file">Image file.</param>
        /// <param name="dropdownEntity">Dropdown entity to apply image to.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator ApplyImageToDropdown(string file, StraightFour.Entity.DropdownEntity dropdownEntity)
        {
            if (!string.IsNullOrEmpty(file))
            {
                WebVerseRuntime.Instance.imageHandler.LoadImageResourceAsTexture2D(
                    VEML.VEMLUtilities.FullyQualifyURI(file, WebVerseRuntime.Instance.currentBasePath),
                new Action<Texture2D>((tex) =>
                {
                    dropdownEntity.SetBackground(Sprite.Create(
                        tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                }));
            }

            yield return null;
        }

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
        private MeshEntity CreateMeshEntity(BaseEntity parent, string meshObject, string[] meshResources,
            Vector3 position, Quaternion rotation, string id, Action<MeshEntity> onLoaded, bool checkForUpdateIfCached)
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

            StraightFour.Entity.BaseEntity pBE = GetPrivateEntity(parent);
            Vector3 pos = new Vector3(position.x, position.y, position.z);
            Quaternion rot = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            MeshEntity me = new MeshEntity();

            Action<StraightFour.Entity.MeshEntity> onEntityLoadedAction =
                new Action<StraightFour.Entity.MeshEntity>((meshEntity) =>
            {
                if (meshEntity == null)
                {
                    Logging.LogError("[EntityAPIHelper:Create] Error loading mesh entity.");
                }
                else
                {
                    meshEntity.SetParent(pBE);
                    meshEntity.SetPosition(pos, true);
                    meshEntity.SetRotation(rot, true);

                    me.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                    AddEntityMapping(me.internalEntity, me);
                    if (onLoaded != null)
                    {
                        onLoaded.Invoke(me);
                    }
                }
            });

            WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsMeshEntity(
                meshObject, meshResources, guid, onEntityLoadedAction, 10, checkForUpdateIfCached);

            return me;
        }

        private void Update()
        {
            if (meshEntityCreationJobs.Count > 0 && currentMeshEntityCreationJob == null)
            {
                currentMeshEntityCreationJob = meshEntityCreationJobs.Dequeue();
                CreateMeshEntity(currentMeshEntityCreationJob.parent, currentMeshEntityCreationJob.meshObject,
                    currentMeshEntityCreationJob.meshResources, new Vector3(currentMeshEntityCreationJob.position.x,
                    currentMeshEntityCreationJob.position.y, currentMeshEntityCreationJob.position.z), new Quaternion(
                        currentMeshEntityCreationJob.rotation.x, currentMeshEntityCreationJob.rotation.y,
                        currentMeshEntityCreationJob.rotation.z, currentMeshEntityCreationJob.rotation.w),
                        currentMeshEntityCreationJob.id, new Action<MeshEntity>((me) => {
                            if (!string.IsNullOrEmpty(currentMeshEntityCreationJob.onLoaded))
                            {
                                WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                                    currentMeshEntityCreationJob.onLoaded, new object[] { me });
                            }
                            currentMeshEntityCreationJob = null;
                    }), currentMeshEntityCreationJob.checkForUpdateIfCached);
            }
        }
    }
}