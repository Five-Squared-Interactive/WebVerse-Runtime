// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using FiveSQD.WebVerse.Handlers.Javascript.APIs.WorldTypes;
using FiveSQD.WebVerse.Runtime;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Entity
{
    /// <summary>
    /// Class for an automobile entity.
    /// </summary>
    public class AutomobileEntity : BaseEntity
    {
        /// <summary>
        /// Enumeration for automobile type.
        /// </summary>
        public enum AutomobileType { Default = 0 }

        public bool engineStartStop
        {
            get
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).engineStartStop;
            }

            set
            {
                ((WorldEngine.Entity.AutomobileEntity) internalEntity).engineStartStop = value;
            }
        }

        public float brake
        {
            get
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).brake;
            }

            set
            {
                ((WorldEngine.Entity.AutomobileEntity) internalEntity).brake = value;
            }
        }

        public float handBrake
        {
            get
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).handBrake;
            }

            set
            {
                ((WorldEngine.Entity.AutomobileEntity) internalEntity).handBrake = value;
            }
        }

        public bool horn
        {
            get
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).horn;
            }

            set
            {
                ((WorldEngine.Entity.AutomobileEntity) internalEntity).horn = value;
            }
        }

        public float throttle
        {
            get
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).throttle;
            }

            set
            {
                ((WorldEngine.Entity.AutomobileEntity) internalEntity).throttle = value;
            }
        }

        public float steer
        {
            get
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).steer;
            }

            set
            {
                ((WorldEngine.Entity.AutomobileEntity) internalEntity).steer = value;
            }
        }

        public int gear
        {
            get
            {
                return ((WorldEngine.Entity.AutomobileEntity) internalEntity).gear;
            }

            set
            {
                ((WorldEngine.Entity.AutomobileEntity) internalEntity).gear = value;
            }
        }

        /// <summary>
        /// Create an automobile entity.
        /// </summary>
        /// <param name="parent">Parent of the entity to create.</param>
        /// <param name="meshObject">Path to the mesh object to load for this entity.</param>
        /// <param name="meshResources">Paths to mesh resources for this entity.</param>
        /// <param name="position">Position of the entity relative to its parent.</param>
        /// <param name="rotation">Rotation of the entity relative to its parent.</param>
        /// <param name="wheels">Wheels for the automobile entity.</param>
        /// <param name="mass">Mass of the automobile entity.</param>
        /// <param name="type">Type of automobile entity.</param>
        /// <param name="id">ID of the entity. One will be created if not provided.</param>
        /// <param name="tag">Tag of the entity.</param>
        /// <param name="onLoaded">Action to perform on load. This takes a single parameter containing the created
        /// audio entity object.</param>
        /// <param name="checkForUpdateIfCached">Whether or not to check for update if in cache.</param>
        /// <returns>The ID of the automobile entity object.</returns>
        public static AutomobileEntity Create(BaseEntity parent, string meshObject, string[] meshResources,
            Vector3 position, Quaternion rotation, AutomobileEntityWheel[] wheels, float mass, AutomobileType type,
            string id = null, string tag = null, string onLoaded = null, bool checkForUpdateIfCached = true)
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

            AutomobileEntity ae = new AutomobileEntity();

            System.Action<WorldEngine.Entity.AutomobileEntity> onEntityLoadedAction =
                new System.Action<WorldEngine.Entity.AutomobileEntity>((automobileEntity) =>
            {
                if (automobileEntity == null)
                {
                    Logging.LogError("[AutomobileEntity:Create] Error loading automobile entity.");
                }
                else
                {
                    automobileEntity.SetParent(pBE);
                    automobileEntity.SetPosition(pos, true);
                    automobileEntity.SetRotation(rot, true);

                    ae.internalEntity = WorldEngine.WorldEngine.ActiveWorld.entityManager.FindEntity(guid);
                    EntityAPIHelper.AddEntityMapping(ae.internalEntity, ae);
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(onLoaded, new object[] { ae });
                    }
                }

            });

            WebVerseRuntime.Instance.gltfHandler.LoadGLTFResourceAsAutomobileEntity(meshObject, meshResources,
                pos, rot, wheels, mass, type, guid, onEntityLoadedAction, 10, checkForUpdateIfCached);

            return ae;
        }

        internal AutomobileEntity()
        {
            internalEntityType = typeof(WorldEngine.Entity.AutomobileEntity);
        }
    }
}