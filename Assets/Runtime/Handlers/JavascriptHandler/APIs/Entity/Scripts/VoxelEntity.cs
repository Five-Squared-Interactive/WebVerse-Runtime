// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for a voxel entity.
    /// </summary>
    public class VoxelEntity : BaseEntity
    {
        /// <summary>
        /// Create a voxel entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="scale">Scale of the entity relative to its parent.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// terrain entity object.</param>
        /// <returns>The ID of the terrain entity object.</returns>
        public static VoxelEntity Create(BaseEntity parent,
            Vector3 position, Quaternion rotation, Vector3 scale,
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

            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);
            UnityEngine.Vector3 pos = new UnityEngine.Vector3(position.x, position.y, position.z);
            UnityEngine.Quaternion rot = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
            UnityEngine.Vector3 scl = new UnityEngine.Vector3(scale.x, scale.y, scale.z);

            VoxelEntity ve = new VoxelEntity();

            System.Action onLoadAction = null;
            onLoadAction = () =>
            {
                ve.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
                EntityAPIHelper.AddEntityMapping(ve.internalEntity, ve);
                if (!string.IsNullOrEmpty(onLoaded))
                {
                    WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ve });
                }
            };

            StraightFour.StraightFour.ActiveWorld.entityManager.LoadVoxelEntity(pBE, pos, rot, scl, guid, tag, onLoadAction);

            return ve;
        }

        /// <summary>
        /// Create a voxel entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the voxel entity configuration.</param>
        /// <param name="parent">Parent entity for the voxel entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created voxel entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, voxelEntity) =>
            {
                if (!success || voxelEntity == null || !(voxelEntity is StraightFour.Entity.VoxelEntity))
                {
                    Logging.LogError("[VoxelEntity:Create] Error loading voxel entity from JSON.");
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
                                (StraightFour.Entity.VoxelEntity) voxelEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadVoxelEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        public VoxelEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.VoxelEntity);
        }

        /// <summary>
        /// Set the blockinfo for a given block with an ID.
        /// </summary>
        /// <param name="id">ID of the block.</param>
        /// <param name="info">Info for the block.</param>
        public bool SetBlockInfo(int id, VoxelBlockInfo info)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[VoxelEntity:SetBlockInfo] Unknown entity.");
                return false;
            }

            if (info == null)
            {
                Logging.LogWarning("[VoxelEntity:SetBlockInfo] Invalid Block Info.");
                return false;
            }

            return EntityAPIHelper.SetBlockInfoAsync(id, info, (StraightFour.Entity.VoxelEntity) internalEntity);
        }

        /// <summary>
        /// Set the block at a given coordinate.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        /// <param name="type">Block type at coordinate.</param>
        /// <param name="subType">Block subtype at coordinate.</param>
        public bool SetBlock(int x, int y, int z, int type, int subType)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[VoxelEntity:SetBlock] Unknown entity.");
                return false;
            }

            ((StraightFour.Entity.VoxelEntity) internalEntity).SetBlock(x, y, z, type, subType);
            return true;
        }

        /// <summary>
        /// Get the block at a given coordinate.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        /// <returns>Integer array with the first element being the block type,
        /// and the second being the block subtype.</returns>
        public int[] GetBlock(int x, int y, int z)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[VoxelEntity:GetBlock] Unknown entity.");
                return null;
            }

            return ((StraightFour.Entity.VoxelEntity) internalEntity).GetBlock(x, y, z);
        }

        /// <summary>
        /// Whether or not the voxel entity contains a given chunk.
        /// </summary>
        /// <param name="x">X index of the chunk.</param>
        /// <param name="y">Y index of the chunk.</param>
        /// <param name="z">Z index of the chunk.</param>
        /// <returns>Whether or not the chunk exists.</returns>
        public bool ContainsChunk(int x, int y, int z)
        {
            if (IsValid() == false)
            {
                Logging.LogError("[VoxelEntity:ContainsChunk] Unknown entity.");
                return false;
            }

            return ((StraightFour.Entity.VoxelEntity) internalEntity).ContainsChunk(x, y, z);
        }
    }
}