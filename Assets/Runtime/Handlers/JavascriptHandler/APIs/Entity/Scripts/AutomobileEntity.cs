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
                return ((StraightFour.Entity.AutomobileEntity) internalEntity).engineStartStop;
            }

            set
            {
                ((StraightFour.Entity.AutomobileEntity) internalEntity).engineStartStop = value;
            }
        }

        public float brake
        {
            get
            {
                return ((StraightFour.Entity.AutomobileEntity) internalEntity).brake;
            }

            set
            {
                ((StraightFour.Entity.AutomobileEntity) internalEntity).brake = value;
            }
        }

        public float handBrake
        {
            get
            {
                return ((StraightFour.Entity.AutomobileEntity) internalEntity).handBrake;
            }

            set
            {
                ((StraightFour.Entity.AutomobileEntity) internalEntity).handBrake = value;
            }
        }

        public bool horn
        {
            get
            {
                return ((StraightFour.Entity.AutomobileEntity) internalEntity).horn;
            }

            set
            {
                ((StraightFour.Entity.AutomobileEntity) internalEntity).horn = value;
            }
        }

        public float throttle
        {
            get
            {
                return ((StraightFour.Entity.AutomobileEntity) internalEntity).throttle;
            }

            set
            {
                ((StraightFour.Entity.AutomobileEntity) internalEntity).throttle = value;
            }
        }

        public float steer
        {
            get
            {
                return ((StraightFour.Entity.AutomobileEntity) internalEntity).steer;
            }

            set
            {
                ((StraightFour.Entity.AutomobileEntity) internalEntity).steer = value;
            }
        }

        public int gear
        {
            get
            {
                return ((StraightFour.Entity.AutomobileEntity) internalEntity).gear;
            }

            set
            {
                ((StraightFour.Entity.AutomobileEntity) internalEntity).gear = value;
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

            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);
            UnityEngine.Vector3 pos = new UnityEngine.Vector3(position.x, position.y, position.z);
            UnityEngine.Quaternion rot = new UnityEngine.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            AutomobileEntity ae = new AutomobileEntity();

            System.Action<StraightFour.Entity.AutomobileEntity> onEntityLoadedAction =
                new System.Action<StraightFour.Entity.AutomobileEntity>((automobileEntity) =>
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

                    ae.internalEntity = StraightFour.StraightFour.ActiveWorld.entityManager.FindEntity(guid);
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

        /// <summary>
        /// Create an automobile entity from a JSON string.
        /// </summary>
        /// <param name="jsonEntity">JSON string containing the automobile entity configuration.</param>
        /// <param name="parent">Parent entity for the automobile entity. If null, the entity will be created at the world root.</param>
        /// <param name="onLoaded">JavaScript callback function to execute when the entity is created. The callback will receive the created automobile entity as a parameter.</param>
        public static void Create(string jsonEntity, BaseEntity parent = null, string onLoaded = null)
        {
            StraightFour.Entity.BaseEntity pBE = EntityAPIHelper.GetPrivateEntity(parent);

            Action<bool, Guid?, StraightFour.Entity.BaseEntity> onComplete =
                new Action<bool, Guid?, StraightFour.Entity.BaseEntity>((success, entityId, automobileEntity) =>
            {
                if (!success || automobileEntity == null || !(automobileEntity is StraightFour.Entity.AutomobileEntity))
                {
                    Logging.LogError("[AutomobileEntity:Create] Error loading automobile entity from JSON.");
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                            onLoaded, new object[] { null });
                    }
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(onLoaded))
                    {
                        WebVerseRuntime.Instance.javascriptHandler.CallWithParams(
                            onLoaded, new object[] { EntityAPIHelper.GetPublicEntity(
                                (StraightFour.Entity.AutomobileEntity) automobileEntity) });
                    }
                }
            });

            WebVerseRuntime.Instance.jsonEntityHandler.LoadAutomobileEntityFromJSON(jsonEntity, pBE, onComplete);
        }

        internal AutomobileEntity()
        {
            internalEntityType = typeof(StraightFour.Entity.AutomobileEntity);
        }
    }
}