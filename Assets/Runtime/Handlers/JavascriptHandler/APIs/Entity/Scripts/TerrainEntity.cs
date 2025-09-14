// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.StraightFour.Entity;
using System.Collections.Generic;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a terrain entity.
    /// </summary>
    public class TerrainEntity : BaseEntity
    {
        internal enum TerrainEntityType { heightmap = 0, voxel = 1, hybrid = 2 }

        /// <summary>
        /// Create a heightmap terrain entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="length">Length of the terrain in terrain units.</param>
        /// <param name="width">Width of the terrain in terrain units.</param>
        /// <param name="height">Height of the terrain in terrain units.</param>
        /// <param name="heights">2D array of heights for the terrain.</param>
        /// <param name="layers">Layers for the terrain.</param>
        /// <param name="layerMasks">Layer masks for the terrain.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// terrain entity object.</param>
        /// <returns>The heightmap terrain entity object.</returns>
        public static TerrainEntity CreateHeightmap(BaseEntity parent, float length, float width, float height, float[][] heights,
            TerrainEntityLayer[] layers, TerrainEntityLayerMaskCollection layerMasks, Vector3 position, Quaternion rotation,
            string id = null, string tag = null, string onLoaded = null, bool stitchTerrains = false)
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

            List<StraightFour.Entity.Terrain.TerrainEntityLayer> convertedLayers
                            = new List<StraightFour.Entity.Terrain.TerrainEntityLayer>();
            foreach (TerrainEntityLayer layer in layers)
            {
                convertedLayers.Add(new StraightFour.Entity.Terrain.TerrainEntityLayer()
                {
                    diffusePath = layer.diffuseTexture,
                    normalPath = layer.normalTexture,
                    maskPath = layer.maskTexture,
                    specular = new UnityEngine.Color(layer.specular.r, layer.specular.g, layer.specular.b, layer.specular.a),
                    metallic = layer.metallic,
                    smoothness = layer.smoothness
                });
            }

            Dictionary<int, float[,]> convertedLayerMasks = new Dictionary<int, float[,]>();
            int idx = 0;
            foreach (float[,] layerMask in layerMasks.ToFloatArrays())
            {
                convertedLayerMasks.Add(idx++, layerMask);
            }

            TerrainEntity te = new TerrainEntity(TerrainEntityType.heightmap);

            Action onLoadAction = null;
            onLoadAction = () =>
            {
                te.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(te.internalEntity, te);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { te });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadTerrainEntity(length, width, height, heights,
                convertedLayers.ToArray(), convertedLayerMasks, pBE, pos, rot, stitchTerrains, guid, tag, onLoadAction);

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
        /// <param name="modifications">Modifications for the terrain.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// terrain entity object.</param>
        /// <returns>The hybrid terrain entity object.</returns>
        public static TerrainEntity CreateHybrid(BaseEntity parent, float length, float width, float height, float[][] heights,
            TerrainEntityLayer[] layers, TerrainEntityLayerMaskCollection layerMasks, TerrainEntityModification[] modifications,
            Vector3 position, Quaternion rotation, string id = null, string tag = null, string onLoaded = null, bool stitchTerrains = false)
        {
            return EntityAPIHelper.LoadHybridTerrainEntityAsync(parent, length, width, height, heights,
                layers, layerMasks, modifications, position, rotation, stitchTerrains, id, tag, onLoaded);
        }

        /// <summary>
        /// Build on the terrain entity. Only valid for hybrid terrain entities.
        /// </summary>
        /// <param name="position">Position at which to build.</param>
        /// <param name="brushType">Type of brush to use.</param>
        /// <param name="layer">Layer to build on.</param>
        /// <param name="size">Size of the addition, in meters.</param>
        /// <param name="synchronizeChange">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Build(Vector3 position, TerrainEntityBrushType brushType,
            int layer, float size = 1, bool synchronizeChange = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TerrainEntity:Build] Unknown entity.");
                return false;
            }

            if (internalEntity is not HybridTerrainEntity)
            {
                Logging.LogWarning("[TerrainEntity:Build] Operation only valid on hybrid terrain entities.");
                return false;
            }

            StraightFour.Entity.Terrain.TerrainEntityBrushType internalBrushType;
            switch (brushType)
            {
                case TerrainEntityBrushType.sphere:
                    internalBrushType = StraightFour.Entity.Terrain.TerrainEntityBrushType.sphere;
                    break;

                case TerrainEntityBrushType.roundedCube:
                default:
                    internalBrushType = StraightFour.Entity.Terrain.TerrainEntityBrushType.roundedCube;
                    break;
            }
            return ((HybridTerrainEntity) internalEntity).Build(
                new UnityEngine.Vector3(position.x, position.y, position.z),
                internalBrushType, layer, size, synchronizeChange);
        }

        /// <summary>
        /// Dig on the terrain entity. Only valid for hybrid terrain entities.
        /// </summary>
        /// <param name="position">Position at which to dig.</param>
        /// <param name="brushType">Type of brush to use.</param>
        /// <param name="layer">Layer to dig on.</param>
        /// <param name="size">Size of the hole, in meters.</param>
        /// <param name="synchronizeChange">Whether or not to synchronize the change.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Dig(Vector3 position, TerrainEntityBrushType brushType,
            int layer, float size, bool synchronizeChange = true)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TerrainEntity:Dig] Unknown entity.");
                return false;
            }

            if (internalEntity is not HybridTerrainEntity)
            {
                Logging.LogWarning("[TerrainEntity:Dig] Operation only valid on hybrid terrain entities.");
                return false;
            }

            StraightFour.Entity.Terrain.TerrainEntityBrushType internalBrushType;
            switch (brushType)
            {
                case TerrainEntityBrushType.sphere:
                    internalBrushType = StraightFour.Entity.Terrain.TerrainEntityBrushType.sphere;
                    break;

                case TerrainEntityBrushType.roundedCube:
                default:
                    internalBrushType = StraightFour.Entity.Terrain.TerrainEntityBrushType.roundedCube;
                    break;
            }
            return ((HybridTerrainEntity) internalEntity).Dig(
                new UnityEngine.Vector3(position.x, position.y, position.z),
                internalBrushType, layer, size, synchronizeChange);
        }

        /// <summary>
        /// Get the height at a given x and y index.
        /// </summary>
        /// <param name="xIndex">X index at which to get the height.</param>
        /// <param name="yIndex">Y index at which to get the height.</param>
        /// <returns>The height at the given x and y index.</returns>
        public float GetHeight(int xIndex, int yIndex)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TerrainEntity:GetHeight] Unknown entity.");
                return 0;
            }

            if (internalEntity is HybridTerrainEntity)
            {
                return ((HybridTerrainEntity) internalEntity).GetHeight(xIndex, yIndex);
            }
            else if (internalEntity is StraightFour.Entity.TerrainEntity)
            {
                return ((StraightFour.Entity.TerrainEntity) internalEntity).GetHeight(xIndex, yIndex);
            }
            else
            {
                Logging.LogError("[TerrainEntity:GetHeight] Invalid entity type.");
                return 0;
            }
        }

        /// <summary>
        /// Get the layer mask for a given terrain entity layer.
        /// </summary>
        /// <param name="index">Index for which to get the layer mask.</param>
        /// <returns>A layer mask (a 2d array of values between 0 and 1 corresponding to the
        /// intensity of the given layer).</returns>
        public float[,] GetLayerMask(int index)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TerrainEntity:GetLayerMask] Unknown entity.");
                return null;
            }

            if (internalEntity is HybridTerrainEntity)
            {
                return ((HybridTerrainEntity) internalEntity).GetLayerMask(index);
            }
            else if (internalEntity is StraightFour.Entity.TerrainEntity)
            {
                return ((StraightFour.Entity.TerrainEntity) internalEntity).GetLayerMask(index);
            }
            else
            {
                Logging.LogError("[TerrainEntity:GetLayerMask] Invalid entity type.");
                return null;
            }
        }

        /// <summary>
        /// Get the block at a given position.
        /// </summary>
        /// <param name="position">Position to get the block at.</param>
        /// <returns>Block at the given position (array containing the operation
        /// (as a string) and layer index (as an int)). Operations: build, dig, unset.</returns>
        public object[] GetBlockAtPosition(Vector3 position)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TerrainEntity:GetBlockAtPosition] Unknown entity.");
                return null;
            }

            if (internalEntity is not HybridTerrainEntity)
            {
                Logging.LogWarning("[TerrainEntity:GetBlockAtPosition] Operation only valid on hybrid terrain entities.");
                return null;
            }

            Tuple<HybridTerrainEntity.TerrainOperation, int, StraightFour.Entity.Terrain.TerrainEntityBrushType, float> block
                = ((HybridTerrainEntity) internalEntity).GetBlockAtPosition(
                new UnityEngine.Vector3(position.x, position.y, position.z));

            if (block == null)
            {
                return null;
            }

            return new object[] {
                (block.Item1 == HybridTerrainEntity.TerrainOperation.Build ? "build"
                : block.Item1 == HybridTerrainEntity.TerrainOperation.Dig ? "dig" : "unset"),
                block.Item2,
                (block.Item3 == StraightFour.Entity.Terrain.TerrainEntityBrushType.sphere ? "sphere"
                : "roundedcube")
            };
        }

        /// <summary>
        /// Get all modifications for the terrain.
        /// </summary>
        /// <returns>All modifications for the terrain.</returns>
        public TerrainEntityModification[] GetModifications()
        {
            if (IsValid() == false)
            {
                Logging.LogError("[TerrainEntity:GetModifications] Unknown entity.");
                return null;
            }

            if (internalEntity is not HybridTerrainEntity)
            {
                Logging.LogWarning("[TerrainEntity:GetModifications] Operation only valid on hybrid terrain entities.");
                return null;
            }

            Dictionary<UnityEngine.Vector3Int, Tuple<HybridTerrainEntity.TerrainOperation,
                int, StraightFour.Entity.Terrain.TerrainEntityBrushType, float>> mods
                = ((HybridTerrainEntity) internalEntity).GetTerrainModifications();

            List<TerrainEntityModification> outputMods = new List<TerrainEntityModification>();
            if (mods != null)
            {
                foreach (KeyValuePair<UnityEngine.Vector3Int, Tuple<HybridTerrainEntity.TerrainOperation,
                    int, StraightFour.Entity.Terrain.TerrainEntityBrushType, float>> mod in mods)
                {
                    if (mod.Key == null)
                    {
                        continue;
                    }

                    TerrainEntityModification.TerrainEntityOperation modName;
                    switch (mod.Value.Item1)
                    {
                        case HybridTerrainEntity.TerrainOperation.Dig:
                            modName = TerrainEntityModification.TerrainEntityOperation.Dig;
                            break;

                        case HybridTerrainEntity.TerrainOperation.Build:
                            modName = TerrainEntityModification.TerrainEntityOperation.Build;
                            break;

                        case HybridTerrainEntity.TerrainOperation.Unset:
                        default:
                            modName = TerrainEntityModification.TerrainEntityOperation.Unset;
                            break;
                    }

                    TerrainEntityBrushType bt;
                    switch (mod.Value.Item3)
                    {
                        case StraightFour.Entity.Terrain.TerrainEntityBrushType.sphere:
                            bt = TerrainEntityBrushType.sphere;
                            break;

                        case StraightFour.Entity.Terrain.TerrainEntityBrushType.roundedCube:
                        default:
                            bt = TerrainEntityBrushType.roundedCube;
                            break;
                    }
                    outputMods.Add(new TerrainEntityModification(modName,
                        new Vector3(mod.Key.x, mod.Key.y, mod.Key.z), bt, mod.Value.Item2, mod.Value.Item4));
                }
            }

            return outputMods.ToArray();
        }

        internal TerrainEntity(TerrainEntityType t)
        {
            switch (t)
            {
                case TerrainEntityType.heightmap:
                    internalEntityType = typeof(StraightFour.Entity.TerrainEntity);
                    break;

                case TerrainEntityType.voxel:
                    Logging.LogError("[TerrainEntity] Voxel terrain entity not suppported.");
                    break;

                case TerrainEntityType.hybrid:
                    internalEntityType = typeof(StraightFour.Entity.HybridTerrainEntity);
                    break;

                default:
                    Logging.LogError("[TerrainEntity] Unknown terrain entity type.");
                    break;
            }
        }
    }
}