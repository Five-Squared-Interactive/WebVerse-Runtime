// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

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
        /// The filled volume (surface level + XZ footprint) this water body represents.
        /// Set by <see cref="FitToBounds"/> / <see cref="FitToEntity"/>.
        /// </summary>
        private WaterVolume waterVolume;

        /// <summary>
        /// Whether a water volume has been assigned via a fill operation.
        /// </summary>
        private bool hasWaterVolume = false;

        /// <summary>
        /// Current Gerstner wave parameters (mirrors the shader) so buoyancy can sample the
        /// wavy surface and floating bodies ride the visible waves. Captured in SetProperties.
        /// </summary>
        private WaterWaves currentWaves;

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

            WaterMaterialAdapter.Apply(meshRenderer.material, new WaterSurfaceProperties
            {
                shallowColor = shallowColor,
                deepColor = deepColor,
                specularColor = specularColor,
                scatteringColor = scatteringColor,
                deepStart = deepStart,
                deepEnd = deepEnd,
                distortion = distortion,
                smoothness = smoothness,
                numWaves = numWaves,
                waveAmplitude = waveAmplitude,
                waveSteepness = waveSteepness,
                waveSpeed = waveSpeed,
                waveLength = waveLength,
                scale = scale,
                intensity = intensity
            });

            currentWaves = WaterWaves.FromProperties(numWaves, waveLength, waveSteepness, waveSpeed, waveAmplitude);
            if (hasWaterVolume)
            {
                waterVolume.waves = currentWaves;
            }

            return true;
        }

        public bool SetSpecular(Color shallowColor, Color deepColor, float depthStart, float depthEnd, float distortion)
        {
            if (distortion < 0 || distortion > 128)
            {
                LogSystem.LogWarning("[WaterBodyEntity->SetRefraction] Parameter distortion out of range.");
                return false;
            }

            WaterMaterialAdapter.ApplyDepth(meshRenderer.material, shallowColor, deepColor,
                depthStart, depthEnd, distortion);

            return true;
        }

        /// <summary>
        /// Fill the volume defined by a world-space AABB up to a water level. The surface is
        /// positioned at <paramref name="waterLevel"/> and scaled to the footprint XZ. The
        /// depth-color gradient fades against any geometry rising through the surface.
        /// </summary>
        /// <param name="worldBounds">World-space AABB of the basin/region to fill.</param>
        /// <param name="waterLevel">World-space Y of the water surface.</param>
        /// <returns>Whether the fill was applied. False for degenerate (zero-area) bounds.</returns>
        public bool FitToBounds(Bounds worldBounds, float waterLevel)
        {
            WaterFill.FillPlan plan = WaterFill.Plan(worldBounds, waterLevel);
            if (plan.result != WaterFill.FillResult.Ok)
            {
                LogSystem.LogWarning("[WaterBodyEntity->FitToBounds] Degenerate (zero-area) or non-finite fill parameters; not filling.");
                return false;
            }

            transform.position = plan.surfacePosition;
            transform.localScale = plan.surfaceScale;
            waterVolume = plan.volume;
            waterVolume.waves = currentWaves;
            hasWaterVolume = true;
            BuoyancyController.Instance.RegisterWater(this);

            if (plan.levelBelowFloor)
            {
                LogSystem.Log("[WaterBodyEntity->FitToBounds] Water level is below the basin floor; volume is empty.");
            }
            if (plan.levelAboveTop)
            {
                LogSystem.Log("[WaterBodyEntity->FitToBounds] Water level is above the basin top; overfilling.");
            }

            return true;
        }

        /// <summary>
        /// Fill the volume of a target mesh entity up to a water level. The footprint is the
        /// target's world-space AABB. A rotated target produces a world AABB larger than its
        /// silhouette (by design) and logs an over-fill warning.
        /// </summary>
        /// <param name="meshEntity">Target whose bounds define the fill footprint.</param>
        /// <param name="waterLevel">World-space Y of the water surface.</param>
        /// <returns>Whether the fill was applied.</returns>
        public bool FitToEntity(BaseEntity meshEntity, float waterLevel)
        {
            if (meshEntity == null)
            {
                LogSystem.LogError("[WaterBodyEntity->FitToEntity] Null target entity.");
                return false;
            }

            if (!WaterFill.TryGetWorldRendererBounds(meshEntity.gameObject, out Bounds worldBounds))
            {
                LogSystem.LogError("[WaterBodyEntity->FitToEntity] Target entity has no renderer bounds.");
                return false;
            }

            if (meshEntity.transform.rotation != Quaternion.identity)
            {
                LogSystem.LogWarning("[WaterBodyEntity->FitToEntity] Target has non-identity rotation; "
                    + "using its world AABB, which may over-fill beyond the mesh silhouette.");
            }

            return FitToBounds(worldBounds, waterLevel);
        }

        /// <summary>
        /// Move the water surface to a new world-space Y without changing the footprint or
        /// recreating the entity.
        /// </summary>
        /// <param name="waterLevel">New world-space Y of the surface.</param>
        /// <returns>Always true.</returns>
        public bool SetWaterLevel(float waterLevel)
        {
            Vector3 p = transform.position;
            transform.position = new Vector3(p.x, waterLevel, p.z);
            if (hasWaterVolume)
            {
                waterVolume.waterLevel = waterLevel;
                BuoyancyController.Instance.RegisterWater(this);
            }
            else
            {
                // No footprint yet: record the level so a later fill can honour it, but do not
                // fabricate a zero-area volume or register a non-functional water body.
                waterVolume = new WaterVolume(waterLevel, waterVolume.footprint);
                waterVolume.waves = currentWaves;
            }
            return true;
        }

        /// <summary>
        /// Get the current water volume (surface level + footprint).
        /// </summary>
        public WaterVolume GetWaterVolume()
        {
            return waterVolume;
        }

        /// <summary>
        /// Whether a water volume has been assigned via a fill operation.
        /// </summary>
        public bool HasWaterVolume()
        {
            return hasWaterVolume;
        }

        /// <summary>
        /// Get the current water level (world-space Y of the surface).
        /// </summary>
        public float GetWaterLevel()
        {
            return waterVolume.waterLevel;
        }

        /// <summary>
        /// Get the current XZ footprint bounds.
        /// </summary>
        public Bounds GetFootprint()
        {
            return waterVolume.footprint;
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
        /// Unity callback on GameObject destruction. Entities are destroyed via
        /// Delete() -> Destroy(gameObject); TearDown() is not called on world unload, so
        /// buoyancy unregistration must happen here to avoid leaking stale water references
        /// in the BuoyancyController across world loads.
        /// </summary>
        private void OnDestroy()
        {
            if (BuoyancyController.HasInstance)
            {
                BuoyancyController.Instance.UnregisterWater(this);
            }
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

            // Disable colliders during placement so the placement raycast passes through the
            // preview to hit world geometry. See MeshEntity.MakePlacing for context.
            meshCollider.enabled = false;
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

            // Remove ALL colliders on the preview (root + descendants). See MeshEntity for context.
            foreach (Collider c in previewObject.GetComponentsInChildren<Collider>(true))
            {
                DestroyImmediate(c);
            }

            foreach (Rigidbody rb in previewObject.GetComponentsInChildren<Rigidbody>(true))
            {
                DestroyImmediate(rb);
            }

            foreach (MeshRenderer rend in previewObject.GetComponentsInChildren<MeshRenderer>())
            {
                rend.material = MaterialManager.PreviewMaterial;
            }

            SetPreviewVisibility(false);
        }
    }
}