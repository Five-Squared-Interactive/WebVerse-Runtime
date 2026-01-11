// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.StraightFour.Materials;
using FiveSQD.StraightFour.Entity.Placement;
using FiveSQD.StraightFour.Tags;
using FiveSQD.StraightFour.Entity.Voxels;
using FiveSQD.StraightFour.Utilities;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for a voxel entity.
    /// </summary>
    public class VoxelEntity : BaseEntity
    {
        /// <summary>
        /// Meshes on the character model.
        /// </summary>
        private Mesh[] meshes;

        /// <summary>
        /// Box collider for the voxel entity.
        /// </summary>
        private BoxCollider boxCollider;

        /// <summary>
        /// Rigidbody for the voxel entity.
        /// </summary>
        private Rigidbody rigidBody;

        /// <summary>
        /// Highlight cube for the voxel entity.
        /// </summary>
        private GameObject highlightCube;

        /// <summary>
        /// Size of a chunk.
        /// </summary>
        private int chunkSize = 16;

        /// <summary>
        /// Dictionary of voxel blocks and their IDs.
        /// </summary>
        private Dictionary<int, BlockInfo> blocks;

        /// <summary>
        /// Dictionary of voxel chunks and their 3D indices.
        /// </summary>
        private Dictionary<Vector3Int, VoxelChunk> voxelChunks;

        /// <summary>
        /// Get the motion state for this entity.
        /// </summary>
        /// <returns>The motion state for this entity.</returns>
        public override EntityMotion? GetMotion()
        {
            return new EntityMotion
            {
                angularVelocity = Vector3.zero,
                stationary = true,
                velocity = Vector3.zero
            };
        }

        /// <summary>
        /// Get the physical properties for the entity.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public override EntityPhysicalProperties? GetPhysicalProperties()
        {
            return new EntityPhysicalProperties()
            {
                angularDrag = float.PositiveInfinity,
                centerOfMass = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity),
                drag = float.PositiveInfinity,
                gravitational = false,
                mass = float.PositiveInfinity
            };
        }

        /// <summary>
        /// Set the interaction state for the entity.
        /// </summary>
        /// <param name="stateToSet">Interaction state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetInteractionState(InteractionState stateToSet)
        {
            switch (stateToSet)
            {
                case InteractionState.Physical:
                    LogSystem.LogWarning("[VoxelEntity->SetInteractionState] Physical not valid for voxel.");
                    return true;

                case InteractionState.Placing:
                    LogSystem.LogWarning("[VoxelEntity->SetInteractionState] Placing not valid for voxel.");
                    return false;

                case InteractionState.Static:
                    MakeStatic();
                    return true;

                case InteractionState.Hidden:
                    MakeHidden();
                    return true;

                default:
                    LogSystem.LogWarning("[VoxelEntity->SetInteractionState] Interaction state invalid.");
                    return false;
            }
        }

        /// <summary>
        /// Set the blockinfo for a given block with an ID.
        /// </summary>
        /// <param name="id">ID of the block.</param>
        /// <param name="info">Info for the block.</param>
        public void SetBlockInfo(int id, BlockInfo info)
        {
            blocks[id] = info;
        }

        /// <summary>
        /// Set the block at a given coordinate.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        /// <param name="type">Block type at coordinate.</param>
        /// <param name="subType">Block subtype at coordinate.</param>
        public void SetBlock(int x, int y, int z, int type, int subType)
        {
            BlockInfo foundType = blocks[type];
            if (foundType == null)
            {
                LogSystem.LogWarning("[VoxelEntity->SetBlock] Invalid block type.");
                return;
            }
            BlockSubtype foundSubType = foundType.subTypes[subType];
            if (foundSubType == null)
            {
                LogSystem.LogWarning("[VoxelEntity->SetBlock] Invalid block subtype.");
                return;
            }

            Vector3Int chunkIndex = GetChunkIndexForBlock(new Vector3Int(x, y, z));
            if (!voxelChunks.ContainsKey(chunkIndex))
            {
                GameObject voxelChunkGameObject = new GameObject();
                voxelChunkGameObject.name = "Chunk-" +
                    chunkIndex.x + "-" + chunkIndex.y + "-" + chunkIndex.z;
                voxelChunkGameObject.transform.parent = transform;
                voxelChunkGameObject.transform.localPosition = chunkIndex * chunkSize;
                voxelChunkGameObject.transform.localRotation = Quaternion.identity;
                voxelChunkGameObject.transform.localScale = Vector3.one;
                if (!blocks.ContainsKey(0))
                {
                    LogSystem.LogWarning(
                        "[VoxelEntity->SetBlock] Default block has not been defined.");
                    return;
                }
                voxelChunks[chunkIndex] = VoxelChunk.Create(
                    voxelChunkGameObject, blocks[0].subTypes[0], chunkSize);
                if (!voxelChunks.ContainsKey(chunkIndex))
                {
                    LogSystem.LogWarning("[VoxelEntity->SetBlock] Unable to get chunk.");
                    return;
                }
            }

            Vector3Int blockIndex = GetBlockIndexInChunk(new Vector3Int(x, y, z), chunkIndex);
            voxelChunks[chunkIndex].SetBlock(blockIndex.x, blockIndex.y,
                blockIndex.z, foundSubType);
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
            Vector3Int chunkIndex = GetChunkIndexForBlock(new Vector3Int(x, y, z));
            if (!voxelChunks.ContainsKey(chunkIndex))
            {
                GameObject voxelChunkGameObject = new GameObject();
                voxelChunkGameObject.name = "Chunk-" +
                    chunkIndex.x + "-" + chunkIndex.y + "-" + chunkIndex.z;
                voxelChunkGameObject.transform.parent = transform;
                voxelChunkGameObject.transform.localPosition = chunkIndex * chunkSize;
                voxelChunkGameObject.transform.localRotation = Quaternion.identity;
                voxelChunkGameObject.transform.localScale = Vector3.one;
                if (!blocks.ContainsKey(0))
                {
                    LogSystem.LogWarning(
                        "[VoxelEntity->GetBlock] Default block has not been defined.");
                    return null;
                }
                voxelChunks[chunkIndex] = VoxelChunk.Create(
                    voxelChunkGameObject, blocks[0].subTypes[0], chunkSize);
                if (!voxelChunks.ContainsKey(chunkIndex))
                {
                    LogSystem.LogWarning("[VoxelEntity->GetBlock] Unable to get chunk.");
                    return null;
                }
            }

            Vector3Int blockIndex = GetBlockIndexInChunk(new Vector3Int(x, y, z), chunkIndex);
            return voxelChunks[chunkIndex].GetBlock(blockIndex.x, blockIndex.y, blockIndex.z);
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
            return voxelChunks.ContainsKey(new Vector3Int(x, y, z));
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(System.Guid idToSet)
        {
            base.Initialize(idToSet);

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            SetRigidbody(rb);

            List<Mesh> ms = new List<Mesh>();
            foreach (MeshFilter filt in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                ms.Add(filt.sharedMesh);
            }
            SetRenderers(ms.ToArray());

            BoxCollider boxCollider = null;
            foreach (BoxCollider bc in gameObject.GetComponentsInChildren<BoxCollider>())
            {
                if (bc.tag == TagManager.physicsColliderTag)
                {
                    boxCollider = bc;
                    break;
                }
            }

            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }

            SetColliders(boxCollider);

            MakeHidden();
            SetUpHighlightVolume();

            blocks = new Dictionary<int, BlockInfo>();

            voxelChunks = new Dictionary<Vector3Int, VoxelChunk>();
        }

        /// <summary>
        /// Tear down the entity.
        /// </summary>
        public override void TearDown()
        {
            base.TearDown();
        }

        /// <summary>
        /// Set mesh renderers for the entity.
        /// </summary>
        /// <param name="ms">Meshes to apply.</param>
        private void SetRenderers(Mesh[] ms)
        {
            if (ms == null)
            {
                LogSystem.LogWarning("[VoxelEntity->SetRenderer] No mesh.");
            }
            meshes = ms;
        }

        /// <summary>
        /// Set colliders for the entity.
        /// </summary>
        /// <param name="cc">Colliders to apply.</param>
        private void SetColliders(BoxCollider bc)
        {
            if (bc == null)
            {
                LogSystem.LogWarning("[VoxelEntity->SetColliders] No box collider.");
            }
            boxCollider = bc;
        }

        /// <summary>
        /// Set rigidbody for the entity.
        /// </summary>
        /// <param name="rb">Rigidbody to apply.</param>
        private void SetRigidbody(Rigidbody rb)
        {
            if (rb == null)
            {
                LogSystem.LogWarning("[VoxelEntity->SetRigidbody] No rigidbody.");
            }
            rigidBody = rb;
        }

        /// <summary>
        /// Make the entity hidden.
        /// </summary>
        private void MakeHidden()
        {
            switch (interactionState)
            {
                case InteractionState.Physical:
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                    break;

                case InteractionState.Hidden:
                default:
                    break;
            }

            rigidBody.isKinematic = true;
            boxCollider.enabled = false;
            gameObject.SetActive(false);
            interactionState = InteractionState.Hidden;
        }

        /// <summary>
        /// Make the entity static.
        /// </summary>
        private void MakeStatic()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    // Handled in main sequence.
                    break;

                case InteractionState.Physical:
                    // Handled in main sequence.
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                default:
                    break;
            }

            gameObject.SetActive(true);
            rigidBody.isKinematic = true;
            boxCollider.enabled = false;
            interactionState = InteractionState.Static;
        }

        /// <summary>
        /// Make the entity physical.
        /// </summary>
        private void MakePhysical()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    // Handled in main sequence.
                    break;

                case InteractionState.Placing:
                    break;

                case InteractionState.Static:
                    // Handled in main sequence.
                    break;

                case InteractionState.Physical:
                default:
                    break;
            }

            gameObject.SetActive(true);
            interactionState = InteractionState.Physical;
        }

        /// <summary>
        /// Make the entity placing.
        /// </summary>
        private void MakePlacing()
        {
            switch (interactionState)
            {
                case InteractionState.Hidden:
                    break;

                case InteractionState.Physical:
                    break;

                case InteractionState.Static:
                    break;

                case InteractionState.Placing:
                default:
                    break;
            }

            //EBSPartCollectionManager.StartPlacing(this);

            gameObject.SetActive(true);
            interactionState = InteractionState.Placing;
        }

        /// <summary>
        /// Set up the highlight volume for the entity.
        /// </summary>
        private void SetUpHighlightVolume()
        {
            Bounds bounds = new Bounds(transform.position, Vector3.zero);
            MeshRenderer[] rends = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rend in rends)
            {
                bounds.Encapsulate(rend.bounds);
            }

            bounds.center = bounds.center - transform.position;

            highlightCube = new GameObject("HighlightVolume");

            Vector3[] vertices =
            {
                    new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                    new Vector3 (bounds.max.x, bounds.min.y, bounds.min.z),
                    new Vector3 (bounds.max.x, bounds.max.y, bounds.min.z),
                    new Vector3 (bounds.min.x, bounds.min.y, bounds.min.z),
                    new Vector3 (bounds.min.x, bounds.max.y, bounds.max.z),
                    new Vector3 (bounds.max.x, bounds.max.y, bounds.max.z),
                    new Vector3 (bounds.max.x, bounds.min.y, bounds.max.z),
                    new Vector3 (bounds.min.x, bounds.min.y, bounds.max.z),
                };

            int[] triangles =
            {
                    0, 2, 1, //face front
			        0, 3, 2,
                    2, 3, 4, //face top
			        2, 4, 5,
                    1, 2, 5, //face right
			        1, 5, 6,
                    0, 7, 4, //face left
			        0, 4, 3,
                    5, 4, 7, //face back
			        5, 7, 6,
                    0, 6, 7, //face bottom
			        0, 1, 6
                };

            Mesh mesh = highlightCube.AddComponent<MeshFilter>().mesh;
            MeshRenderer hRend = highlightCube.AddComponent<MeshRenderer>();
            hRend.material = MaterialManager.HighlightMaterial;
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.Optimize();
            mesh.RecalculateNormals();

            highlightCube.transform.SetParent(transform);
            highlightCube.transform.localPosition = Vector3.zero;
            highlightCube.transform.localRotation = Quaternion.identity;
            highlightCube.transform.localScale = Vector3.one;
            highlightCube.SetActive(false);
        }

        /// <summary>
        /// Get the chunk index for a given block index.
        /// </summary>
        /// <param name="block">Block coordinate.</param>
        /// <returns>Chunk coordinate.</returns>
        private Vector3Int GetChunkIndexForBlock(Vector3Int block)
        {
            return new Vector3Int(block.x / chunkSize,
                block.y / chunkSize, block.z / chunkSize);
        }

        /// <summary>
        /// Get index of a block within its chunk.
        /// </summary>
        /// <param name="block">Global block index.</param>
        /// <param name="chunkIndex">Chunk index.</param>
        /// <returns>Chunk-local block index.</returns>
        private Vector3Int GetBlockIndexInChunk(Vector3Int block, Vector3Int chunkIndex)
        {
            return new Vector3Int(block.x - chunkIndex.x * chunkSize,
                block.y - chunkIndex.y * chunkSize, block.z - chunkIndex.z * chunkSize);
        }
    }
}