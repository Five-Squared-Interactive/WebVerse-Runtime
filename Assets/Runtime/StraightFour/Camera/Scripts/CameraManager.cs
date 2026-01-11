// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using FiveSQD.StraightFour.Utilities;
using System.Collections.Generic;
using FiveSQD.StraightFour.Entity;

namespace FiveSQD.StraightFour.Camera
{
    /// <summary>
    /// Class that manages the world's camera.
    /// </summary>
    public class CameraManager : BaseManager
    {
        /// <summary>
        /// Whether or not the camera is in VR mode.
        /// </summary>
        public bool vr;

        /// <summary>
        /// Camera that is being controlled.
        /// </summary>
        [Tooltip("Camera that is being controlled.")]
        public UnityEngine.Camera cam { get; private set; }

        /// <summary>
        /// Camera offset (for VR mode).
        /// </summary>
        public GameObject cameraOffset { get; private set; }

        /// <summary>
        /// Default parent for the camera (i.e. when null is provided).
        /// </summary>
        [Tooltip("Default parent for the camera (i.e. when null is provided).")]
        public GameObject defaultCameraParent { get; private set; }

        public bool crosshairEnabled
        {
            get
            {
                return StraightFour.ActiveWorld.crosshair.activeSelf;
            }
            set
            {
                StraightFour.ActiveWorld.crosshair.SetActive(value);
            }
        }

        /// <summary>
        /// Entities that will follow the camera's rotation.
        /// </summary>
        private List<BaseEntity> followers;

        /// <summary>
        /// Initialize the camera manager.
        /// </summary>
        /// <param name="cam">Camera to initialize with.</param>
        /// <param name="defaultCameraParent">Default camera parent to initialize with.</param>
        public void Initialize(UnityEngine.Camera cam, GameObject cameraOffset, bool vr, GameObject defaultCameraParent)
        {
            base.Initialize();
            this.cam = cam;
            this.cameraOffset = cameraOffset;
            this.vr = vr;
            this.defaultCameraParent = defaultCameraParent;
            followers = new List<BaseEntity>();
            this.crosshairEnabled = false;
        }

        /// <summary>
        /// Terminate the camera manager.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();
            followers = null;
        }

        /// <summary>
        /// Add a follower.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public void AddFollower(BaseEntity entity)
        {
            if (entity == null)
            {
                LogSystem.LogError("[CameraManager->AddFollower] Invalid entity.");
                return;
            }

            if (followers == null)
            {
                LogSystem.LogError("[CameraManager->AddFollower] CameraManager not initialized.");
                return;
            }

            if (entity is UIEntity)
            {
                LogSystem.LogWarning("[CameraManager->AddFollower] UI Entity cannot follow the camera.");
                return;
            }

            followers.Add(entity);
        }

        /// <summary>
        /// Remove a follower.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        public void RemoveFollower(BaseEntity entity)
        {
            if (entity == null)
            {
                LogSystem.LogError("[CameraManager->AddFollower] Invalid entity.");
                return;
            }

            if (followers == null)
            {
                LogSystem.LogError("[CameraManager->AddFollower] CameraManager not initialized.");
                return;
            }

            if (followers.Contains(entity))
            {
                followers.Remove(entity);
            }
        }

        /// <summary>
        /// Set parent for the camera. If null is provided, will use the default camera parent.
        /// </summary>
        /// <param name="parent">Parent GameObject to apply to the camera.</param>
        public void SetParent(GameObject parent)
        {
            if (parent == null)
            {
                if (vr)
                {
                    cameraOffset.transform.SetParent(defaultCameraParent.transform);
                }
                else
                {
                    cam.transform.SetParent(defaultCameraParent == null ? null : defaultCameraParent.transform);
                }
            }
            else
            {
                if (vr)
                {
                    cameraOffset.transform.SetParent(parent.transform);
                }
                else
                {
                    cam.transform.SetParent(parent.transform);
                }
            }
        }

        /// <summary>
        /// Set the position of the camera.
        /// </summary>
        /// <param name="position">Position to apply.</param>
        /// <param name="local">Whether or not that position is local.</param>
        public void SetPosition(Vector3 position, bool local)
        {
            if (local)
            {
                if (vr)
                {
                    cameraOffset.transform.localPosition = position;
                }
                else
                {
                    cam.transform.localPosition = position;
                }
            }
            else
            {
                Vector3 worldOffset = StraightFour.ActiveWorld.worldOffset;
                if (vr)
                {
                    cameraOffset.transform.position = new Vector3(position.x + worldOffset.x,
                        position.y + worldOffset.y, position.z + worldOffset.z);
                }
                else
                {
                    cam.transform.position = new Vector3(position.x + worldOffset.x,
                        position.y + worldOffset.y, position.z + worldOffset.z);
                }
            }
        }

        /// <summary>
        /// Get the position of the camera.
        /// </summary>
        /// <param name="local">Whether or not the position is local.</param>
        /// <returns>The position of the camera.</returns>
        public Vector3 GetPosition(bool local)
        {
            if (local)
            {
                if (vr)
                {
                    return cameraOffset.transform.localPosition;
                }
                else
                {
                    return cam.transform.localPosition;
                }
            }
            else
            {
                Vector3 worldOffset = StraightFour.ActiveWorld.worldOffset;
                if (vr)
                {
                    return new Vector3(cameraOffset.transform.position.x - worldOffset.x,
                        cameraOffset.transform.position.y - worldOffset.y,
                        cameraOffset.transform.position.z - worldOffset.z);
                }
                else
                {
                    return new Vector3(cam.transform.position.x - worldOffset.x,
                        cam.transform.position.y - worldOffset.y, cam.transform.position.z - worldOffset.z);
                }
            }
        }

        /// <summary>
        /// Set the rotation of the camera.
        /// </summary>
        /// <param name="rotation">Rotation to apply.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        public void SetRotation(Quaternion rotation, bool local)
        {
            if (local)
            {
                if (vr)
                {
                    cameraOffset.transform.localRotation = rotation;
                }
                else
                {
                    cam.transform.localRotation = rotation;
                }
            }
            else
            {
                if (vr)
                {
                    cameraOffset.transform.rotation = rotation;
                }
                else
                {
                    cam.transform.rotation = rotation;
                }
            }
        }

        /// <summary>
        /// Get the rotation of the camera.
        /// </summary>
        /// <param name="local">Whether or not the rotation is local.</param>
        /// <returns>The rotation of the camera.</returns>
        public Quaternion GetRotation(bool local)
        {
            if (local)
            {
                if (vr)
                {
                    return cameraOffset.transform.localRotation;
                }
                else
                {
                    return cam.transform.localRotation;
                }
            }
            else
            {
                if (vr)
                {
                    return cameraOffset.transform.rotation;
                }
                else
                {
                    return cam.transform.rotation;
                }
            }
        }

        /// <summary>
        /// Set the euler rotation of the camera.
        /// </summary>
        /// <param name="rotation">Euler rotation to apply.</param>
        /// <param name="local">Whether or not the euler rotation is local.</param>
        public void SetEulerRotation(Vector3 rotation, bool local)
        {
            if (local)
            {
                if (vr)
                {
                    cameraOffset.transform.localEulerAngles = rotation;
                }
                else
                {
                    cam.transform.localEulerAngles = rotation;
                }
            }
            else
            {
                if (vr)
                {
                    cameraOffset.transform.eulerAngles = rotation;
                }
                else
                {
                    cam.transform.eulerAngles = rotation;
                }
            }
        }

        /// <summary>
        /// Get the Euler rotation of the camera.
        /// </summary>
        /// <param name="local">Whether or not the Euler rotation is local.</param>
        /// <returns>The Euler rotation of the camera.</returns>
        public Vector3 GetEulerRotation(bool local)
        {
            if (local)
            {
                if (vr)
                {
                    return cameraOffset.transform.localEulerAngles;
                }
                else
                {
                    return cam.transform.localEulerAngles;
                }
            }
            else
            {
                if (vr)
                {
                    return cameraOffset.transform.eulerAngles;
                }
                else
                {
                    return cam.transform.eulerAngles;
                }
            }
        }

        /// <summary>
        /// Set the scale of the camera.
        /// </summary>
        /// <param name="scale">Scale to apply.</param>
        public void SetScale(Vector3 scale)
        {
            if (vr)
            {
                cameraOffset.transform.localScale = scale;
            }
            else
            {
                cam.transform.localScale = scale;
            }
        }

        /// <summary>
        /// Get the scale of the camera.
        /// </summary>
        /// <returns>The scale of the camera.</returns>
        public Vector3 GetScale()
        {
            if (vr)
            {
                return cameraOffset.transform.localScale;
            }
            else
            {
                return cam.transform.localScale;
            }
        }

        /// <summary>
        /// Get a raycast from the camera.
        /// </summary>
        /// <returns>A raycast from the camera, or null.</returns>
        public RaycastHit? GetRaycast()
        {
            RaycastHit hit;
            
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit))
            {
                hit.point -= StraightFour.ActiveWorld.worldOffset;
                return hit;
            }
            return null;
        }

        public RaycastHit? GetScreenPointerRaycast()
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;

                hit.point -= StraightFour.ActiveWorld.worldOffset;
                return hit;
            }

            return null;
        }

        private void Update()
        {
            if (cam != null)
            {
                foreach (BaseEntity follower in followers)
                {
                    Vector3 worldOffset = StraightFour.ActiveWorld.worldOffset;
                    //Vector3 cameraEulerAngles = new Vector3(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y + 90, cam.transform.eulerAngles.z);
                    follower.SetRotation(cam.transform.rotation, false);
                    Vector3 cameraEulerAngles = new Vector3(cam.transform.localEulerAngles.x, cam.transform.localEulerAngles.y, cam.transform.localEulerAngles.z);
                    follower.SetEulerRotation(cameraEulerAngles, true, false);
                    follower.SetPosition(new Vector3(cam.transform.position.x - worldOffset.x, cam.transform.position.y - worldOffset.y, cam.transform.position.z - worldOffset.z),
                        false);
                }
            }
        }
    }
}