// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for an airplane entity.
    /// </summary>
    public class AirplaneEntity : BaseEntity
    {
        public float throttle
        {
            get
            {
                return ((WorldEngine.Entity.AirplaneEntity) internalEntity).throttle;
            }

            set
            {
                ((WorldEngine.Entity.AirplaneEntity) internalEntity).throttle = value;
            }
        }

        public float pitch
        {
            get
            {
                return ((WorldEngine.Entity.AirplaneEntity) internalEntity).pitch;
            }

            set
            {
                ((WorldEngine.Entity.AirplaneEntity) internalEntity).pitch = value;
            }
        }

        public float roll
        {
            get
            {
                return ((WorldEngine.Entity.AirplaneEntity) internalEntity).roll;
            }

            set
            {
                ((WorldEngine.Entity.AirplaneEntity) internalEntity).roll = value;
            }
        }

        public float yaw
        {
            get
            {
                return ((WorldEngine.Entity.AirplaneEntity) internalEntity).yaw;
            }

            set
            {
                ((WorldEngine.Entity.AirplaneEntity) internalEntity).yaw = value;
            }
        }

        public void StartEngine()
        {
            ((WorldEngine.Entity.AirplaneEntity) internalEntity).StartEngine();
        }

        public void StopEngine()
        {
            ((WorldEngine.Entity.AirplaneEntity) internalEntity).StopEngine();
        }

        /// <summary>
        /// Create an airplane entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="meshObject">Path to the mesh object to load for this entity.</param>
        /// <param name="meshResources">Paths to mesh resources for this entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="wheels">Wheels for the airplane entity.</param>
        /// <param name="mass">Mass of the airplane entity.</param>
        /// <param name="type">Type of airplane entity.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// audio entity object.</param>
        /// <param name="checkForUpdateIfCached">Whether or not to check for update if in cache.</param>
        /// <returns>The ID of the airplane entity object.</returns>
        public static AirplaneEntity Create(BaseEntity parent, string meshObject, string[] meshResources,
            Vector3 position, Quaternion rotation, float mass, string id = null, string tag = null,
            string onLoaded = null, bool checkForUpdateIfCached = true)
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

            WorldEngine.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);
            UnityEngine.Vector3 pos = new UnityEngine.Vector3(position.x, position.y, position.z);
            UnityEngine.Quaternion rot = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            AirplaneEntity ae = new AirplaneEntity();

            System.Action<WorldEngine.Entity.AirplaneEntity> onEntityLoadedAction =
                new System.Action<WorldEngine.Entity.AirplaneEntity>((airplaneEntity) =>
            {
                if (airplaneEntity == null)
                {
                    Logging.LogError("[AirplaneEntity:Create] Error loading airplane entity.");
                }
                else
                {
                    airplaneEntity.SetParent(pBE);
                    airplaneEntity.SetPosition(pos, true);
                    airplaneEntity.SetRotation(rot, true);

                    ae.internalEntity = WorldEngine.WorldEngine.ActiveWorld.entityManager.FindEntity(guid);
                    EntityAPIHelper.AddEntityMapping(ae.internalEntity, ae);
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ae });
                    }
                }

            });

            WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAirplaneEntity(meshObject, meshResources,
                pos, rot, mass, guid, onEntityLoadedAction, 10, checkForUpdateIfCached);

            return ae;
        }

        internal AirplaneEntity()
        {
            internalEntityType = typeof(WorldEngine.Entity.AirplaneEntity);
        }
    }
}