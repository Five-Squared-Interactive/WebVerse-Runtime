// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using FiveSQD.StraightFour.Tags;
using FiveSQD.StraightFour.Utilities;

namespace FiveSQD.StraightFour.MeshManager
{
    /// <summary>
    /// WIP: Class that manages mesh objects.
    /// </summary>
    public class MeshManager : BaseManager
    {
        /// <summary>
        /// Container for loaded mesh prefabs.
        /// </summary>
        [Tooltip("Container for loaded mesh prefabs.")]
        public GameObject meshPrefabContainer;

        /// <summary>
        /// Location for mesh prefabs.
        /// </summary>
        private static readonly Vector3 prefabLocation = new Vector3(9999, 9999, 9999);

        /// <summary>
        /// Dictionary of loaded mesh prefabs and their names.
        /// </summary>
        private Dictionary<string, GameObject> loadedMeshPrefabs = new Dictionary<string, GameObject>();

        /// <summary>
        /// Static instance of the mesh manager class.
        /// </summary>
        private static MeshManager instance;

        /// <summary>
        /// Initializes the mesh manager.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            instance = this;
        }

        /// <summary>
        /// Set up a mesh prefab.
        /// </summary>
        /// <param name="prefab">Prefab to be set up.</param>
        private void SetUpMeshPrefab(GameObject prefab)
        {
            prefab.SetActive(false);
            prefab.transform.SetParent(meshPrefabContainer.transform);
            MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
            List<Mesh> meshes = new List<Mesh>();
            if (meshFilters.Length > 0)
            {
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    meshes.Add(meshFilter.mesh);

                    GameObject mcGO = new GameObject("MeshCollider");
                    mcGO.transform.SetParent(meshFilter.transform);
                    mcGO.transform.localPosition = Vector3.zero;
                    mcGO.transform.localRotation = Quaternion.identity;
                    mcGO.transform.localScale = Vector3.one;
                    MeshCollider meshCollider = mcGO.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = meshFilter.mesh;
                    meshCollider.tag = TagManager.meshColliderTag;
                }
            }
            else
            {
                Utilities.LogSystem.LogWarning("[MeshManager->SetUpMeshPrefab] Unable to set up mesh.");
                return;
            }

            Bounds bounds = new Bounds(prefab.transform.position, Vector3.zero);
            MeshRenderer[] rends = prefab.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rend in rends)
            {
                bounds.Encapsulate(rend.bounds);
            }

            bounds.center = bounds.center - prefab.transform.position;

            GameObject bcGO = new GameObject("BoxCollider");
            bcGO.transform.SetParent(prefab.transform);
            bcGO.transform.localPosition = Vector3.zero;
            BoxCollider boxCollider = bcGO.AddComponent<BoxCollider>();
            boxCollider.tag = TagManager.physicsColliderTag;
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;

            Rigidbody rigidbody = prefab.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        }
    }
}