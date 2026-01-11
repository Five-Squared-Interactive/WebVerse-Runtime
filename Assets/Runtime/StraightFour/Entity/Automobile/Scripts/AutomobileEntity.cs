// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using NWH.VehiclePhysics2;
using NWH.VehiclePhysics2.Damage;
using NWH.VehiclePhysics2.Modules.FlipOver;
using NWH.WheelController3D;
using UnityEngine;
using FiveSQD.StraightFour.Utilities;
using FiveSQD.StraightFour.Tags;
using FiveSQD.StraightFour.Materials;

namespace FiveSQD.StraightFour.Entity
{
    /// <summary>
    /// Class for an automobile entity.
    /// </summary>
    public class AutomobileEntity : BaseEntity
    {
        public List<GameObject> wheels;

        public StateSettings stateSettings;

        public Rigidbody rbody;

        public VehicleController vehicleController;

        public FlipOverModuleWrapper flipOverModuleWrapper;

        public DamageHandler damageHandler;

        public List<WheelController> wheelControllers;

        /// <summary>
        /// Meshes on the mesh model.
        /// </summary>
        private Mesh[] meshes;

        /// <summary>
        /// Renderers on the mesh model.
        /// </summary>
        private MeshRenderer[] renderers;

        /// <summary>
        /// Original size of the mesh.
        /// </summary>
        private Vector3 originalMeshSize;

        /// <summary>
        /// Current scaling of the mesh.
        /// </summary>
        private Vector3 meshScaling;

        /// <summary>
        /// Mesh colliders used by the AutomobileEntity.
        /// </summary>
        private MeshCollider[] meshColliders;

        /// <summary>
        /// Whether or not the character is gravitational.
        /// </summary>
        private bool gravitational = false;

        /// <summary>
        /// Cube used for highlighting the character entity.
        /// </summary>
        private GameObject highlightCube;

        /// <summary>
        /// Object to use for previewing.
        /// </summary>
        private GameObject previewObject;

        private bool wheelControllersNeedSetup = false;

        public bool engineStartStop
        {
            get
            {
                return vehicleController.input.EngineStartStop;
            }

            set
            {
                vehicleController.input.EngineStartStop = value;
            }
        }

        public float brake
        {
            get
            {
                return vehicleController.input.Brakes;
            }

            set
            {
                vehicleController.input.Brakes = value;
            }
        }

        public float handBrake
        {
            get
            {
                return vehicleController.input.Handbrake;
            }

            set
            {
                vehicleController.input.Handbrake = value;
            }
        }

        public bool horn
        {
            get
            {
                return vehicleController.input.Horn;
            }

            set
            {
                vehicleController.input.Horn = value;
            }
        }

        public float throttle
        {
            get
            {
                return vehicleController.input.Throttle;
            }

            set
            {
                vehicleController.input.Throttle = value;
            }
        }

        public float steer
        {
            get
            {
                return vehicleController.input.Steering;
            }

            set
            {
                vehicleController.input.Steering = value;
            }
        }

        public int gear
        {
            get
            {
                return vehicleController.input.ShiftInto;
            }

            set
            {
                vehicleController.input.ShiftInto = value;
            }
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
            if (rbody == null)
            {
                LogSystem.LogError("[MeshEntity->GetMotion] No rigidbody for mesh entity.");
                return null;
            }

            return new EntityMotion
            {
                angularVelocity = rbody.angularVelocity,
                stationary = rbody.isKinematic,
                velocity = rbody.linearVelocity
            };
        }

        /// <summary>
        /// Get the physical properties for the entity.
        /// </summary>
        /// <returns>The physical properties for this entity.</returns>
        public override EntityPhysicalProperties? GetPhysicalProperties()
        {
            if (rbody == null)
            {
                LogSystem.LogError("[MeshEntity->GetPhysicalProperties] No rigidbody for mesh entity.");
                return null;
            }

            return new EntityPhysicalProperties
            {
                angularDrag = rbody.angularDamping,
                centerOfMass = rbody.centerOfMass,
                drag = rbody.linearDamping,
                gravitational = gravitational,
                mass = rbody.mass
            };
        }

        /// <summary>
        /// Get the size of the entity.
        /// </summary>
        /// <returns>The size of the entity.</returns>
        public override Vector3 GetSize()
        {
            return new Vector3(originalMeshSize.x * meshScaling.x,
                originalMeshSize.y * meshScaling.y, originalMeshSize.z * meshScaling.z);
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
                    LogSystem.LogWarning("[MeshEntity->SetInteractionState] Interaction state invalid.");
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
                LogSystem.LogWarning("[MeshEntity->SetMotion] Invalid motion.");
                return false;
            }

            if (rbody == null)
            {
                LogSystem.LogError("[MeshEntity->SetMotion] No rigidbody for mesh entity.");
                return false;
            }

            if (motionToSet.Value.stationary.HasValue)
            {
                if (motionToSet.Value.stationary == true)
                {
                    rbody.isKinematic = true;
                    rbody.useGravity = false;
                    rbody.angularVelocity = Vector3.zero;
                    rbody.linearVelocity = Vector3.zero;
                    return true;
                }
                else
                {
                    rbody.isKinematic = false;
                    rbody.useGravity = gravitational;
                }
            }

            if (motionToSet.Value.angularVelocity != null)
            {
                rbody.angularVelocity = motionToSet.Value.angularVelocity;
            }

            if (motionToSet.Value.velocity != null)
            {
                rbody.linearVelocity = motionToSet.Value.velocity;
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
                LogSystem.LogWarning("[MeshEntity->SetPhysicalProperties] Invalid physical properties.");
                return false;
            }

            if (rbody == null)
            {
                LogSystem.LogError("[MeshEntity->SetPhysicalProperties] No rigidbody for mesh entity.");
                return false;
            }

            if (epp.Value.angularDrag.HasValue)
            {
                rbody.angularDamping = epp.Value.angularDrag.Value;
            }

            if (epp.Value.centerOfMass != null)
            {
                rbody.centerOfMass = epp.Value.centerOfMass.Value;
            }

            if (epp.Value.drag.HasValue)
            {
                rbody.linearDamping = epp.Value.drag.Value;
            }

            if (epp.Value.gravitational.HasValue)
            {
                gravitational = epp.Value.gravitational.Value;
                rbody.useGravity = gravitational;
            }

            if (epp.Value.mass.HasValue)
            {
                rbody.mass = epp.Value.mass.Value;
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
            float xSclFactor = size.x / originalMeshSize.x;
            float ySclFactor = size.y / originalMeshSize.y;
            float zSclFactor = size.z / originalMeshSize.z;
            meshScaling = new Vector3(xSclFactor, ySclFactor, zSclFactor);
            transform.localScale = new Vector3(transform.localScale.x * xSclFactor,
                transform.localScale.y * ySclFactor, transform.localScale.z * zSclFactor);

            return true;
        }

        /// <summary>
        /// Set the visibility state of the entity.
        /// </summary>
        /// <param name="visible">Whether or not to set the entity to visible.</param>
        /// <param name="synchronize">Whether or not to synchronize the setting.</param>
        /// <returns>Whether or not the setting was successful.</returns>
        public override bool SetVisibility(bool visible, bool synchronize = true)
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
        /// Initialize this entity. This method cannot be used for an automobile entity;
        /// it must be called with wheels, mass, and a type.
        /// </summary>
        /// <param name="idToSet">ID to apply to the entity.</param>
        public override void Initialize(Guid idToSet)
        {
            LogSystem.LogError("[AutomobileEntity->Initialize] UI element entity " +
                "must be initialized with wheels, mass, and a type.");

            return;
        }

        public void Initialize(Guid idToSet, Dictionary<GameObject, float> wheels,
            float mass, EntityManager.AutomobileEntityType type)
        {
            base.Initialize(idToSet);

            gameObject.SetActive(true);

            rbody = gameObject.GetComponent<Rigidbody>();
            if (rbody == null)
            {
                rbody = gameObject.AddComponent<Rigidbody>();
            }
            SetRigidbody(rbody);

            List<Mesh> ms = new List<Mesh>();
            foreach (MeshFilter filt in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                ms.Add(filt.sharedMesh);
            }
            SetRenderers(ms.ToArray());
            List<MeshRenderer> rends = new List<MeshRenderer>();
            foreach (MeshRenderer rend in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                rends.Add(rend);
            }
            renderers = rends.ToArray();
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Mesh m in meshes)
            {
                m.RecalculateBounds();
                bounds.Encapsulate(m.bounds);
            }
            originalMeshSize = bounds.size;
            meshScaling = Vector3.one;

            SetUpPreviewObject();

            rbody.mass = mass;
            rbody.interpolation = RigidbodyInterpolation.Interpolate;
            rbody.ResetCenterOfMass();
            rbody.ResetInertiaTensor();
            vehicleController = gameObject.AddComponent<VehicleController>();

            List<MeshCollider> mcs = new List<MeshCollider>();
            foreach (MeshCollider mc in gameObject.GetComponentsInChildren<MeshCollider>())
            {
                if (mc.tag == TagManager.meshColliderTag)
                {
                    mcs.Add(mc);
                    mc.convex = true;
                }
            }

            SetColliders(mcs.ToArray());

            flipOverModuleWrapper = gameObject.AddComponent<FlipOverModuleWrapper>();
            damageHandler = gameObject.AddComponent<DamageHandler>();
            wheelControllers = new List<WheelController>();
            if (wheels != null)
            {
                foreach (KeyValuePair<GameObject, float> wheel in wheels)
                {
                    // Add a container object for wheel to constrain scale to 1.
                    GameObject wheelHolder = new GameObject("Wheel");
                    GameObject wheelObject = new GameObject("WheelObject");
                    wheelHolder.tag = "Wheel";
                    wheel.Key.tag = "Wheel";
                    foreach (Transform t in wheel.Key.GetComponentsInChildren<Transform>())
                    {
                        t.gameObject.tag = "Wheel";
                    }
                    wheelHolder.transform.SetParent(wheel.Key.transform.parent);
                    wheelHolder.transform.localPosition = wheel.Key.transform.localPosition;
                    wheelHolder.transform.localRotation = Quaternion.identity;
                    wheelHolder.transform.SetParent(transform);
                    wheelHolder.transform.localScale = Vector3.one;
                    wheelObject.transform.SetParent(wheelHolder.transform);
                    wheel.Key.transform.SetParent(wheelObject.transform);
                    foreach (Collider c in wheel.Key.GetComponentsInChildren<Collider>())
                    {
                        Destroy(c.gameObject);
                    }
                    wheel.Key.transform.localPosition = Vector3.zero;
                    WheelController wc = wheelHolder.AddComponent<WheelController>();
                    wc.Radius = wheel.Value;
                    wc.SpringMaxLength = 0;
                    wheelControllers.Add(wc);
                }
            }

            vehicleController.stateSettings = StraightFour.ActiveWorld.entityManager.automobileEntityTypeMap[type];
            vehicleController.soundManager.mixer = StraightFour.ActiveWorld.entityManager.automobileAudioMixer;
            vehicleController.SetDefaults();
            vehicleController.input.autoSetInput = false;
            wheelControllersNeedSetup = true;

            //MakeHidden();
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
                    position.y + worldOffset.y, position.z + worldOffset.z);
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

        protected override void Update()
        {
            base.Update();
            if (wheelControllersNeedSetup && wheelControllers != null)
            {
                foreach (WheelController wheelController in wheelControllers)
                {
                    Transform wheel = wheelController.transform.Find("WheelObject");
                    Transform rotating = wheelController.transform.Find("Rotating");
                    if (wheel == null || rotating == null)
                    {
                        Debug.LogError("Invalid wheel: " + wheel + " " + rotating);
                    }
                    else
                    {
                        wheel.SetParent(rotating);
                        wheel.transform.localPosition = Vector3.zero;
                    }
                }
                wheelControllersNeedSetup = false;

                /*foreach (Renderer rend in renderers)
                {
                    if (rend.tag != "Wheel")
                    {
                        if (rend.GetComponent<MeshCollider>() == null)
                        {
                            MeshCollider mc = rend.gameObject.AddComponent<MeshCollider>();
                            mc.convex = true;
                        }
                    }
                }*/

                foreach (StandardGroundDetection gd in GetComponentsInChildren<StandardGroundDetection>())
                {
                    Destroy(gd);
                }
            }
        }

        /// <summary>
        /// Set the visibility of the main mesh.
        /// </summary>
        /// <param name="visible">Whether or not to make the main mesh visible.</param>
        private void SetMainMeshVisibility(bool visible)
        {
            foreach (MeshRenderer rend in renderers)
            {
                rend.enabled = visible;
            }
        }

        /// <summary>
        /// Set mesh renderers for the entity.
        /// </summary>
        /// <param name="ms">Meshes to apply.</param>
        private void SetRenderers(Mesh[] ms)
        {
            if (ms == null)
            {
                Debug.LogWarning("[AutomobileEntity->SetRenderer] No mesh.");
            }
            meshes = ms;
        }

        /// <summary>
        /// Set colliders for the entity.
        /// </summary>
        /// <param name="cc">Colliders to apply.</param>
        private void SetColliders(MeshCollider[] mc)
        {
            if (mc == null)
            {
                Debug.LogWarning("[AutomobileEntity->SetColliders] No mesh collider.");
            }
            meshColliders = mc;
            foreach (BoxCollider bc in GetComponentsInChildren<BoxCollider>())
            {
                bc.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Set rigidbody for the entity.
        /// </summary>
        /// <param name="rb">Rigidbody to apply.</param>
        private void SetRigidbody(Rigidbody rb)
        {
            if (rb == null)
            {
                Debug.LogWarning("[AutomobileEntity->SetRigidbody] No rigidbody.");
            }
            rbody = rb;
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

            rbody.isKinematic = true;

            foreach (MeshCollider meshCollider in meshColliders)
            {
                meshCollider.enabled = false;
            }
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
            rbody.isKinematic = true;
            foreach (MeshCollider meshCollider in meshColliders)
            {
                meshCollider.enabled = true;
            }
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
            rbody.isKinematic = false;
            foreach (MeshCollider meshCollider in meshColliders)
            {
                if (meshCollider == null) continue;
                meshCollider.enabled = true;
            }
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
            rbody.isKinematic = true;
            foreach (MeshCollider meshCollider in meshColliders)
            {
                meshCollider.enabled = true;
            }
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