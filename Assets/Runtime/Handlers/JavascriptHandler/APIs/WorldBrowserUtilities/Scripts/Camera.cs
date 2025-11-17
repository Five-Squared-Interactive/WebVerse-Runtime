// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for camera manipulation.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Attach camera to an entity.
        /// </summary>
        /// <param name="entityToAttachTo">Entity to attach camera to, or null if to make root.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool AttachToEntity(BaseEntity entityToAttachTo)
        {
            if (entityToAttachTo == null)
            {
                StraightFour.StraightFour.ActiveWorld.cameraManager.SetParent(null);
            }
            else
            {
                entityToAttachTo.PlaceCameraOn();
            }

            return true;
        }

        public static bool AddCameraFollower(BaseEntity entity)
        {
            if (entity == null)
            {
                Logging.LogWarning("[Camera:AddCameraFollower] Invalid entity.");
                return false;
            }

            StraightFour.StraightFour.ActiveWorld.cameraManager.AddFollower(EntityAPIHelper.GetPrivateEntity(entity));
            return true;
        }

        public static bool RemoveCameraFollower(BaseEntity entity)
        {
            if (entity == null)
            {
                Logging.LogWarning("[Camera:RemoveCameraFollower] Invalid entity.");
                return false;
            }

            StraightFour.StraightFour.ActiveWorld.cameraManager.RemoveFollower(EntityAPIHelper.GetPrivateEntity(entity));
            return true;
        }

        /// <summary>
        /// Set position of the camera.
        /// </summary>
        /// <param name="position">Position to apply to camera.</param>
        /// <param name="local">Whether or not the position is local.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetPosition(Vector3 position, bool local)
        {
            StraightFour.StraightFour.ActiveWorld.cameraManager.SetPosition(
                new UnityEngine.Vector3(position.x, position.y, position.z), local);

            return true;
        }

        /// <summary>
        /// Get the position of the camera.
        /// </summary>
        /// <param name="local">Whether or not the position is local.</param>
        /// <returns>The position of the camera.</returns>
        public static Vector3 GetPosition(bool local)
        {
            UnityEngine.Vector3 pos = StraightFour.StraightFour.ActiveWorld.cameraManager.GetPosition(local);
            return new Vector3(pos.x, pos.y, pos.z);
        }

        /// <summary>
        /// Set the rotation of the camera.
        /// </summary>
        /// <param name="rotation">Rotation to apply to camera.</param>
        /// <param name="local">Whether or not the rotation is local.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetRotation(Quaternion rotation, bool local)
        {
            StraightFour.StraightFour.ActiveWorld.cameraManager.SetRotation(
                new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w), local);

            return true;
        }

        /// <summary>
        /// Get the rotation of the camera.
        /// </summary>
        /// <param name="local">Whether or not the rotation is local.</param>
        /// <returns>The rotation of the camera.</returns>
        public static Quaternion GetRotation(bool local)
        {
            UnityEngine.Quaternion rot = StraightFour.StraightFour.ActiveWorld.cameraManager.GetRotation(local);
            return new Quaternion(rot.x, rot.y, rot.z, rot.w);
        }

        /// <summary>
        /// Set the Euler rotation of the camera.
        /// </summary>
        /// <param name="rotation">Euler rotation to apply to camera.</param>
        /// <param name="local">Whether or not the Euler rotation is local.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetEulerRotation(Vector3 rotation, bool local)
        {
            StraightFour.StraightFour.ActiveWorld.cameraManager.SetEulerRotation(
                new UnityEngine.Vector3(rotation.x, rotation.y, rotation.z), local);

            return true;
        }

        /// <summary>
        /// Get the Euler rotation of the camera.
        /// </summary>
        /// <param name="local">Whether or not the Euler rotation is local.</param>
        /// <returns>The Euler rotation of the camera.</returns>
        public static Vector3 GetEulerRotation(bool local)
        {
            UnityEngine.Vector3 rot = StraightFour.StraightFour.ActiveWorld.cameraManager.GetEulerRotation(local);
            return new Vector3(rot.x, rot.y, rot.z);
        }

        /// <summary>
        /// Set the scale of the camera.
        /// </summary>
        /// <param name="scale">Scale to apply to camera.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public static bool SetScale(Vector3 scale)
        {
            StraightFour.StraightFour.ActiveWorld.cameraManager.SetScale(
                new UnityEngine.Vector3(scale.x, scale.y, scale.z));

            return true;
        }

        /// <summary>
        /// Get the scale of the camera.
        /// </summary>
        /// <returns>The scale of the camera.</returns>
        public static Vector3 GetScale()
        {
            UnityEngine.Vector3 scl = StraightFour.StraightFour.ActiveWorld.cameraManager.GetScale();
            return new Vector3(scl.x, scl.y, scl.z);
        }

        /// <summary>
        /// Get a raycast from the camera.
        /// </summary>
        /// <returns>A raycast from the camera, or null.</returns>
        public static RaycastHitInfo? GetRaycast()
        {
            UnityEngine.RaycastHit? hit = StraightFour.StraightFour.ActiveWorld.cameraManager.GetRaycast();
            if (hit == null)
            {
                return null;
            }
            else
            {
                StraightFour.Entity.BaseEntity hitEntity = null;
                if (hitEntity = hit.Value.collider.GetComponentInParent<StraightFour.Entity.BaseEntity>())
                {
                    BaseEntity hitPublicEntity = EntityAPIHelper.GetPublicEntity(hitEntity);
                    if (hitPublicEntity == null)
                    {
                        return null;
                    }

                    return new RaycastHitInfo()
                    {
                        entity = hitPublicEntity,
                        hitPoint = new Vector3(hit.Value.point.x, hit.Value.point.y, hit.Value.point.z),
                        hitPointNormal = new Vector3(hit.Value.normal.x, hit.Value.normal.y, hit.Value.normal.z),
                        origin = GetPosition(false)
                    };
                }

                return null;
            }
        }

        public static bool PlaceEntityInFrontOfCamera(BaseEntity entityToPlace, float distance)
        {
            UnityEngine.Vector3 newCamPos =
                StraightFour.StraightFour.ActiveWorld.cameraManager.cam.transform.position +
                StraightFour.StraightFour.ActiveWorld.cameraManager.cam.transform.forward * distance;
            return entityToPlace.SetPosition(new Vector3(newCamPos.x, newCamPos.y, newCamPos.z), false);
        }
    }
}