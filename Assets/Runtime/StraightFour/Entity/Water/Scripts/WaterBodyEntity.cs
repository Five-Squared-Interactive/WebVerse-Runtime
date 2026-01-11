// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using FiveSQD.StraightFour.Materials;
using FiveSQD.StraightFour.Tags;
using FiveSQD.StraightFour.Utilities;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for a water body entity.
    /// </summary>
    public class WaterBodyEntity : BaseEntity
    {
        /// <summary>
        /// Mesh renderer used by the entity.
        /// </summary>
        public MeshRenderer meshRenderer;

        /// <summary>
        /// Box colliders used by the entity.
        /// </summary>
        private MeshCollider meshCollider;
        //private BoxCollider boxCollider;

        /// <summary>
        /// Rigidbody used by the water body.
        /// </summary>
        private Rigidbody rigidBody;

        /// <summary>
        /// Whether or not the water body is gravitational.
        /// </summary>
        private bool gravitational = false;

        /// <summary>
        /// Cube used for highlighting the water body entity.
        /// </summary>
        private GameObject highlightCube;

        /// <summary>
        /// Object to use for previewing.
        /// </summary>
        private GameObject previewObject;

        /// <summary>
        /// Create a water body entity.
        /// </summary>
        /// <param name="shallowColor">Color for the shallow zone.</param>
        /// <param name="deepColor">Color for the deep zone.</param>
        /// <param name="specularColor">Specular color.</param>
        /// <param name="scatteringColor">Scattering color.</param>
        /// <param name="deepStart">Start of deep zone.</param>
        /// <param name="deepEnd">End of deep zone.</param>
        /// <param name="distortion">Distortion factor (range 0-128).</param>
        /// <param name="smoothness">Smoothness factor (range 0-1).</param>
        /// <param name="numWaves">Number of waves (range 1-32).</param>
        /// <param name="waveAmplitude">Wave amplitude (range 0-1).</param>
        /// <param name="waveSteepness">Wave steepness (range 0-1).</param>
        /// <param name="waveSpeed">Wave speed.</param>
        /// <param name="waveLength">Wave length.</param>
        /// <param name="scale">Scale of the waves.</param>
        /// <param name="intensity">Intensity factor (range 0-1).</param>
        /// <param name="id">ID for the water body entity.</param>
        /// <returns>The requested water body entity.</returns>
        public static WaterBodyEntity Create(Color shallowColor, Color deepColor, Color specularColor, Color scatteringColor,
            float deepStart, float deepEnd, float distortion, float smoothness, float numWaves, float waveAmplitude,
            float waveSteepness, float waveSpeed, float waveLength, float scale, float intensity, Guid id)
        {
            GameObject waterBodyGO = new GameObject("WaterBodyEntity-" + id.ToString());
            WaterBodyEntity waterBodyEntity = waterBodyGO.AddComponent<WaterBodyEntity>();
            waterBodyEntity.Initialize(id);
            waterBodyEntity.SetProperties(shallowColor, deepColor, specularColor, scatteringColor,
                deepStart, deepEnd, distortion, smoothness, numWaves, waveAmplitude,
                waveSteepness, waveSpeed, waveLength, scale, intensity);
            return waterBodyEntity;
        }

        /// <summary>
        /// Set properties for the water body.
        /// </summary>
        /// <param name="shallowColor">Color for the shallow zone.</param>
        /// <param name="deepColor">Color for the deep zone.</param>
        /// <param name="specularColor">Specular color.</param>
        /// <param name="scatteringColor">Scattering color.</param>
        /// <param name="deepStart">Start of deep zone.</param>
        /// <param name="deepEnd">End of deep zone.</param>
        /// <param name="distortion">Distortion factor (range 0-128).</param>
        /// <param name="smoothness">Smoothness factor (range 0-1).</param>
        /// <param name="numWaves">Number of waves (range 1-32).</param>
        /// <param name="waveAmplitude">Wave amplitude (range 0-1).</param>
        /// <param name="waveSteepness">Wave steepness (range 0-1).</param>
        /// <param name="waveSpeed">Wave speed.</param>
        /// <param name="waveLength">Wave length.</param>
        /// <param name="scale">Scale of the waves.</param>
        /// <param name="intensity">Intensity factor (range 0-1).</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool SetProperties(Color shallowColor, Color deepColor, Color specularColor, Color scatteringColor,
            float deepStart, float deepEnd, float distortion, float smoothness, float numWaves, float waveAmplitude,
            float waveSteepness, float waveSpeed, float waveLength, float scale, float intensity)
        {
            if (distortion < 0 || distortion > 128)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetProperties] Parameter distortion out of range.");
                return false;
            }

            if (smoothness < 0 || smoothness > 1)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetProperties] Parameter smoothness out of range.");
                return false;
            }

            if (numWaves < 1 || numWaves > 32)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetProperties] Parameter numWaves out of range.");
                return false;
            }

            if (waveAmplitude < 0 || waveAmplitude > 1)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetProperties] Parameter waveAmplitude out of range.");
                return false;
            }

            if (waveSteepness < 0 || waveSteepness > 1)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetProperties] Parameter waveSteepness out of range.");
                return false;
            }

            if (intensity < 0 || intensity > 1)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetProperties] Parameter intensity out of range.");
                return false;
            }

            Material mat = meshRenderer.material;
            mat.SetColor("_Color", shallowColor);
            mat.SetColor("_DepthColor", deepColor);
            mat.SetColor("_SpecColor", specularColor);
            mat.SetColor("_ScatteringColor", scatteringColor);
            mat.SetFloat("_DepthStart", deepStart);
            mat.SetFloat("_DepthEnd", deepEnd);
            mat.SetFloat("_Distortion", distortion);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_WaveCount", numWaves);
            mat.SetFloat("_WaveAmplitude", waveAmplitude);
            mat.SetFloat("_WaveSteepness", waveSteepness);
            mat.SetFloat("_WaveSpeed", waveSpeed);
            mat.SetFloat("_WaveLength", waveLength);
            mat.SetVector("_NormalMapATilings", new Vector4(scale, scale, scale * 0.75f, scale * 0.75f));
            mat.SetVector("_NormalMapASpeeds", new Vector4(waveSpeed, waveSpeed, waveSpeed * 0.75f, waveSpeed * 0.75f));
            mat.SetFloat("_NormalMapAIntensity", intensity);
            
            return true;
        }

        public bool SetSpecular(Color shallowColor, Color deepColor, float depthStart, float depthEnd, float distortion)
        {
            if (distortion < 0 || distortion > 128)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetRefraction] Parameter distortion out of range.");
                return false;
            }

            Material mat = meshRenderer.material;
            mat.SetColor("_Color", shallowColor);
            mat.SetColor("_DepthColor", deepColor);
            mat.SetFloat("_DepthStart", depthStart);
            mat.SetFloat("_DepthEnd", depthEnd);
            mat.SetFloat("_Distortion", distortion);
            
            return true;
        }

        /// <summary>
        /// Delete the entity.
        /// </summary>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool Delete(bool synchronize = true)
        {
            return base.Delete(synchronize);
        }

        /// <summary>
        /// Get the motion state for this entity.
        /// </summary>
        /// <returns>The motion state for this entity.</returns>
        public override EntityMotion? GetMotion()
        {
            if (rigidBody == null)
            {
                LogSystem.LogError("[WaterBodyEntity->GetMotion] No rigidbody for water body entity.");
                return null;
            }

            return new EntityMotion
            {
                angularVelocity = rigidBody.angularVelocity,
                stationary = rigidBody.isKinematic,
                velocity = rigidBody.linearVelocity
            };
        }

        /// <summary>
        /// Get the physical properties for the entity.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public override EntityPhysicalProperties? GetPhysicalProperties()
        {
            if (rigidBody == null)
            {
                LogSystem.LogError("[WaterBodyEntity->GetPhysicalProperties] No rigidbody for water body entity.");
                return null;
            }

            return new EntityPhysicalProperties
            {
                angularDrag = rigidBody.angularDamping,
                centerOfMass = rigidBody.centerOfMass,
                drag = rigidBody.linearDamping,
                gravitational = gravitational,
                mass = rigidBody.mass
            };
        }

        /// <summary>
        /// Get the size of the entity.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public override Vector3 GetSize()
        {
            return gameObject.transform.localScale;
        }

        /// <summary>
        /// Set the highlight state of the entity.
        /// </summary>
        /// <param name="highlight">Whether or not to turn on the highlight.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetHighlight(bool highlight)
        {
            highlightCube.SetActive(highlight);

            return true;
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
                    MakePhysical();
                    return true;

                case InteractionState.Placing:
                    MakePlacing();
                    return true;

                case InteractionState.Static:
                    MakeStatic();
                    return true;
                
                case InteractionState.Hidden:
                    MakeHidden();
                    return true;
                
                default:
                    LogSystem.LogWarning("[WaterBodyEntity->SetInteractionState] Interaction state invalid.");
                    return false;
            }
        }

        /// <summary>
        /// Set the motion state for this entity.
        /// </summary>
        /// <param name="motionToSet">Motion state to set.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetMotion(EntityMotion? motionToSet)
        {
            if (!motionToSet.HasValue)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetMotion] Invalid motion.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[WaterBodyEntity->SetMotion] No rigidbody for water body entity.");
                return false;
            }

            if (motionToSet.Value.stationary.HasValue)
            {
                if (motionToSet.Value.stationary == true)
                {
                    rigidBody.isKinematic = true;
                    rigidBody.useGravity = false;
                    rigidBody.angularVelocity = Vector3.zero;
                    rigidBody.linearVelocity = Vector3.zero;
                    return true;
                }
                else
                {
                    rigidBody.isKinematic = false;
                    rigidBody.useGravity = gravitational;
                }
            }

            if (motionToSet.Value.angularVelocity != null)
            {
                rigidBody.angularVelocity = motionToSet.Value.angularVelocity;
            }

            if (motionToSet.Value.velocity != null)
            {
                rigidBody.linearVelocity = motionToSet.Value.velocity;
            }

            return true;
        }

        /// <summary>
        /// Set the physical properties of the entity.
        /// </summary>
        /// <param name="propertiesToSet">Properties to apply.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetPhysicalProperties(EntityPhysicalProperties? epp)
        {
            if (!epp.HasValue)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetPhysicalProperties] Invalid physical properties.");
                return false;
            }

            if (rigidBody == null)
            {
                LogSystem.LogError("[WaterBodyEntity->SetPhysicalProperties] No rigidbody for water body entity.");
                return false;
            }

            if (epp.Value.angularDrag.HasValue)
            {
                rigidBody.angularDamping = epp.Value.angularDrag.Value;
            }

            if (epp.Value.centerOfMass != null)
            {
                rigidBody.centerOfMass = epp.Value.centerOfMass.Value;
            }

            if (epp.Value.drag.HasValue)
            {
                rigidBody.linearDamping = epp.Value.drag.Value;
            }

            if (epp.Value.gravitational.HasValue)
            {
                gravitational = epp.Value.gravitational.Value;
                rigidBody.useGravity = gravitational;
            }

            if (epp.Value.mass.HasValue)
            {
                rigidBody.mass = epp.Value.mass.Value;
            }

            return true;
        }

        /// <summary>
        /// Set the size of the entity.
        /// </summary>
        /// <param name="size">Size to set.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetSize(Vector3 size, bool synchronize = true)
        {
            transform.localScale = size;

            return true;
        }

        /// <summary>
        /// Set the visibility state of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to set the entity to visible.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetVisibility(bool visible, bool synchronize)
        {
            // Use base functionality.
            if (base.SetVisibility(visible, synchronize))
            {
                SetPreviewVisibility(false);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Initialize this entity. This should only be called once.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(System.Guid idToSet)
        {
            base.Initialize(idToSet);

            GameObject waterBody = Instantiate(StraightFour.ActiveWorld.entityManager.waterBodyPrefab);
            waterBody.transform.SetParent(transform);

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            SetRigidbody(rb);

            List<MeshRenderer> rends = new List<MeshRenderer>();
            foreach (MeshRenderer rend in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                rends.Add(rend);
            }

            meshRenderer = waterBody.GetComponentInChildren<MeshRenderer>();

            SetUpPreviewObject();

            /*BoxCollider boxCollider = null;
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

            SetCollider(boxCollider);*/

            meshCollider = GetComponentInChildren<MeshCollider>();
            if (meshCollider == null)
            {
                LogSystem.LogError("[WaterBodyEntity->Initialize] Unable to find mesh collider.");
            }

            MakeHidden();
            SetUpHighlightVolume();
        }

        /// <summary>
        /// Tear down the entity.
        /// </summary>
        public override void TearDown()
        {
            base.TearDown();
        }

        /// <summary>
        /// Set the position of the preview.
        /// </summary>
        /// <param name="position">Position to apply to the preview.</param>
        /// <param name="local">Whether or not the position is local.</param>
        public override void SetPreviewPosition(Vector3 position, bool local)
        {
            if (local)
            {
                previewObject.transform.localPosition = position;
            }
            else
            {
                Vector3 worldOffset = StraightFour.ActiveWorld.worldOffset;
                previewObject.transform.position = new Vector3(position.x + worldOffset.x,
                    position.y + worldOffset.y, position.z + worldOffset.z);;
            }
            base.SetPreviewPosition(position, local);
        }

        /// <summary>
        /// Set the rotation of the preview.
        /// </summary>
        /// <param name="rotation">Rotation to apply to the preview.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        public override void SetPreviewRotation(Quaternion rotation, bool local)
        {
            if (local)
            {
                previewObject.transform.localRotation = rotation;
            }
            else
            {
                previewObject.transform.rotation = rotation;
            }
            base.SetPreviewRotation(rotation, local);
        }

        /// <summary>
        /// Accept the current preview.
        /// </summary>
        public override void AcceptPreview()
        {
            transform.position = previewObject.transform.position;
            transform.rotation = previewObject.transform.rotation;
            base.AcceptPreview();
        }

        /// <summary>
        /// Set the visibility of the preview.
        /// </summary>
        /// <param name="visible">Whether or not to make the preview visible.</param>
        protected override void SetPreviewVisibility(bool visible)
        {
            base.SetPreviewVisibility(visible);

            previewObject.transform.localPosition = Vector3.zero;
            previewObject.transform.localRotation = Quaternion.identity;
            previewObject.transform.localScale = Vector3.one;

            previewObject.SetActive(visible);
        }

        /// <summary>
        /// Set the visibility of the main mesh.
        /// </summary>
        /// <param name="visible">Whether or not to make the main mesh visible.</param>
        private void SetMainMeshVisibility(bool visible)
        {
            meshRenderer.enabled = visible;
        }

        /// <summary>
        /// Set collider for the entity.
        /// </summary>
        /// <param name="cc">Collider to apply.</param>
        private void SetCollider(BoxCollider bc)
        {
            if (bc == null)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetColliders] No box collider.");
            }
            //boxCollider = bc;
        }

        /// <summary>
        /// Set rigidbody for the entity.
        /// </summary>
        /// <param name="rb">Rigidbody to apply.</param>
        private void SetRigidbody(Rigidbody rb)
        {
            if (rb == null)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetRigidbody] No rigidbody.");
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
                    SetPreviewVisibility(false);
                    SetMainMeshVisibility(false);
                    break;

                case InteractionState.Static:
                    break;

                case InteractionState.Hidden:
                default:
                    break;
            }

            rigidBody.isKinematic = true;

            meshCollider.enabled = false;
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
                    SetPreviewVisibility(false);
                    SetMainMeshVisibility(true);
                    break;

                case InteractionState.Static:
                default:
                    break;
            }

            gameObject.SetActive(true);
            rigidBody.isKinematic = true;

            meshCollider.enabled = true;
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
                    SetPreviewVisibility(false);
                    SetMainMeshVisibility(true);
                    break;

                case InteractionState.Static:
                    // Handled in main sequence.
                    break;

                case InteractionState.Physical:
                default:
                    break;
            }

            gameObject.SetActive(true);
            rigidBody.isKinematic = false;

            meshCollider.enabled = true;
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

            SetMainMeshVisibility(false);
            SetPreviewVisibility(true);

            gameObject.SetActive(true);
            rigidBody.isKinematic = true;

            meshCollider.enabled = true;
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
            highlightCube.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
            highlightCube.SetActive(false);
        }

        /// <summary>
        /// Set up the preview object.
        /// </summary>
        private void SetUpPreviewObject()
        {
            previewObject = Instantiate(gameObject, transform);
            previewObject.name = "Preview";
            BaseEntity entity = previewObject.GetComponent<BaseEntity>();
            if (entity)
            {
                DestroyImmediate(entity);
            }

            Collider collider = previewObject.GetComponent<Collider>();
            if (collider)
            {
                Destroy(collider);
            }

            Rigidbody rbody = previewObject.GetComponent<Rigidbody>();
            if (rbody)
            {
                Destroy(rbody);
            }    

            foreach (MeshRenderer rend in previewObject.GetComponentsInChildren<MeshRenderer>())
            {
                rend.material = MaterialManager.PreviewMaterial;
            }

            SetPreviewVisibility(false);
        }
    }
}