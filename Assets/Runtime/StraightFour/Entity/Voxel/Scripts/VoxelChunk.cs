// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.StraightFour.Utilities;

namespace FiveSQD.StraightFour.Entity.Voxels
{
    /// <summary>
    /// Class for a voxel chunk.
    /// </summary>
    public class VoxelChunk : MonoBehaviour
    {
        /// <summary>
        /// Create a voxel chunk.
        /// </summary>
        /// <param name="gameObject">GameObject to place chunk on.</param>
        /// <param name="defaultBlock">Default block for the chunk.</param>
        /// <param name="size">Size of the chunk.</param>
        /// <returns>The voxel chunk.</returns>
        public static VoxelChunk Create(GameObject gameObject,
            BlockSubtype defaultBlock, int size)
        {
            VoxelChunk voxelChunk = gameObject.AddComponent<VoxelChunk>();
            voxelChunk.Initialize(size, defaultBlock);
            return voxelChunk;
        }

        /// <summary>
        /// Size of the chunk.
        /// </summary>
        private int size;

        /// <summary>
        /// Dictionary of the voxel objects and their indices in this chunk.
        /// </summary>
        private Dictionary<Vector3Int, Voxel> voxels;

        /// <summary>
        /// Initialize the voxel chunk.
        /// </summary>
        /// <param name="size">Size of the voxel chunk.</param>
        /// <param name="defaultBlock">Default block for the voxel chunk.</param>
        public void Initialize(int size, BlockSubtype defaultBlock)
        {
            this.size = size;
            voxels = new Dictionary<Vector3Int, Voxel>();
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        GameObject voxelObject = Instantiate(StraightFour.ActiveWorld.entityManager.voxelPrefab);
                        voxelObject.transform.SetParent(transform);
                        voxelObject.transform.localPosition = new Vector3(x, y, z);
                        voxelObject.transform.localRotation = Quaternion.identity;
                        voxelObject.transform.localScale = Vector3.one;
                        Voxel voxel = voxelObject.GetComponent<Voxel>();
                        if (voxel == null)
                        {
                            LogSystem.LogWarning("[VoxelChunk->Initialize] Error loading voxel.");
                            return;
                        }
                        voxels[new Vector3Int(x, y, z)] = voxel;
                        SetBlock(x, y, z, defaultBlock);

                    }
                }
            }
        }

        /// <summary>
        /// Set a block in the voxel chunk.
        /// </summary>
        /// <param name="x">X index.</param>
        /// <param name="y">Y index.</param>
        /// <param name="z">Z index.</param>
        /// <param name="blockSubtype">Block subtype to apply at this index.</param>
        public void SetBlock(int x, int y, int z, BlockSubtype blockSubtype)
        {
            Voxel voxelToSet = voxels[new Vector3Int(x, y, z)];
            if (voxelToSet == null)
            {
                LogSystem.LogError("[VoxelChunk->SetBlock] Unable to find block to set.");
                return;
            }
            SetBlockMaterials(voxelToSet, blockSubtype.topTex, blockSubtype.bottomTex,
                blockSubtype.leftTex, blockSubtype.rightTex,
                blockSubtype.frontTex, blockSubtype.backTex);
            if (blockSubtype.invisible)
            {
                voxelToSet.gameObject.SetActive(false);
                ShowAroundBlock(x, y, z);
            }
            else
            {
                voxelToSet.gameObject.SetActive(true);
                OptimizeAroundBlock(x, y, z);
            }
        }

        /// <summary>
        /// Get the block at a given index.
        /// </summary>
        /// <param name="x">X index.</param>
        /// <param name="y">Y index.</param>
        /// <param name="z">Z index.</param>
        /// <returns>2D array with the first element being the voxel type and the second element
        /// being the voxel subtype.</returns>
        public int[] GetBlock(int x, int y, int z)
        {
            Voxel voxelToGet = voxels[new Vector3Int(x, y, z)];
            if (voxelToGet == null)
            {
                LogSystem.LogError("[VoxelChunk->GetBlock] Unable to find block to set.");
                return null;
            }
            return new int[] { voxelToGet.type, voxelToGet.subType };
        }

        /// <summary>
        /// Set the block materials for a voxel.
        /// </summary>
        /// <param name="voxel">Voxel to apply the materials to.</param>
        /// <param name="top">Top material.</param>
        /// <param name="bottom">Bottom material.</param>
        /// <param name="left">Left material.</param>
        /// <param name="right">Right material.</param>
        /// <param name="front">Front material.</param>
        /// <param name="back">Back material.</param>
        private void SetBlockMaterials(Voxel voxel, Texture top, Texture2D bottom,
            Texture2D left, Texture2D right, Texture2D front, Texture2D back)
        {
            SetSegmentMaterial(voxel.top, top);
            SetSegmentMaterial(voxel.bottom, bottom);
            SetSegmentMaterial(voxel.left, left);
            SetSegmentMaterial(voxel.right, right);
            SetSegmentMaterial(voxel.front, front);
            SetSegmentMaterial(voxel.back, back);
        }

        /// <summary>
        /// Set the material for a voxel segment.
        /// </summary>
        /// <param name="segment">Segment on which to set the material.</param>
        /// <param name="material">Material to apply to the segment.</param>
        private void SetSegmentMaterial(GameObject segment, Texture material)
        {
            MeshRenderer rend = segment.GetComponent<MeshRenderer>();
            if (rend == null)
            {
                LogSystem.LogError("[VoxelChunk->SetSegmentMaterial] Error accessing mesh renderer.");
                return;
            }
            if (rend.material == null)
            {
                LogSystem.LogError("[VoxelChunk->SetSegmentMaterial] Error accessing material.");
                return;
            }
            rend.material.SetTexture("_BaseMap", material);
        }

        /// <summary>
        /// Show relevant segments around a block.
        /// </summary>
        /// <param name="x">X index.</param>
        /// <param name="y">Y index.</param>
        /// <param name="z">Z index.</param>
        private void ShowAroundBlock(int x, int y, int z)
        {
            // Show segment above.
            if (y < size - 1 && voxels.ContainsKey(new Vector3Int(x, y + 1, z)))
            {
                if (voxels[new Vector3Int(x, y + 1, z)].gameObject.activeSelf)
                {
                    voxels[new Vector3Int(x, y + 1, z)].bottom.SetActive(true);
                }
            }

            // Show segment below.
            if (y > 0 && voxels.ContainsKey(new Vector3Int(x, y - 1, z)))
            {
                if (voxels[new Vector3Int(x, y - 1, z)].gameObject.activeSelf)
                {
                    voxels[new Vector3Int(x, y - 1, z)].top.SetActive(true);
                }
            }

            // Show segment to left.
            if (x < size - 1 && voxels.ContainsKey(new Vector3Int(x + 1, y, z)))
            {
                if (voxels[new Vector3Int(x + 1, y, z)].gameObject.activeSelf)
                {
                    voxels[new Vector3Int(x + 1, y, z)].left.SetActive(true);
                }
            }

            // Show segment to right.
            if (x > 0 && voxels.ContainsKey(new Vector3Int(x - 1, y, z)))
            {
                if (voxels[new Vector3Int(x - 1, y, z)].gameObject.activeSelf)
                {
                    voxels[new Vector3Int(x - 1, y, z)].right.SetActive(true);
                }
            }

            // Show segment in front.
            if (z < size -1 && voxels.ContainsKey(new Vector3Int(x, y, z + 1)))
            {
                if (voxels[new Vector3Int(x, y, z + 1)].gameObject.activeSelf)
                {
                    voxels[new Vector3Int(x, y, z + 1)].front.SetActive(true);
                }
            }

            // Show segment in back.
            if (z > 0 && voxels.ContainsKey(new Vector3Int(x, y, z - 1)))
            {
                if (voxels[new Vector3Int(x, y, z - 1)].gameObject.activeSelf)
                {
                    voxels[new Vector3Int(x, y, z - 1)].back.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Hide relevant segments around a block.
        /// </summary>
        /// <param name="x">X index.</param>
        /// <param name="y">Y index.</param>
        /// <param name="z">Z index.</param>
        private void OptimizeAroundBlock(int x, int y, int z)
        {
            // Hide segment above.
            if (y < size - 1 && voxels.ContainsKey(new Vector3Int(x, y + 1, z)))
            {
                voxels[new Vector3Int(x, y + 1, z)].bottom.SetActive(false);
            }

            // Hide segment below.
            if (y > 0 && voxels.ContainsKey(new Vector3Int(x, y - 1, z)))
            {
                voxels[new Vector3Int(x, y - 1, z)].top.SetActive(false);
            }

            // Hide segment to left.
            if (x < size - 1 && voxels.ContainsKey(new Vector3Int(x + 1, y, z)))
            {
                voxels[new Vector3Int(x + 1, y, z)].left.SetActive(false);
            }

            // Hide segment to right.
            if (x > 0 && voxels.ContainsKey(new Vector3Int(x - 1, y, z)))
            {
                voxels[new Vector3Int(x - 1, y, z)].right.SetActive(false);
            }

            // Hide segment in front.
            if (z < size - 1 && voxels.ContainsKey(new Vector3Int(x, y, z + 1)))
            {
                voxels[new Vector3Int(x, y, z + 1)].front.SetActive(false);
            }

            // Hide segment in back.
            if (z > 0 && voxels.ContainsKey(new Vector3Int(x, y, z - 1)))
            {
                voxels[new Vector3Int(x, y, z - 1)].back.SetActive(false);
            }
        }
    }
}